using Biplott.Application.DTOs;
using Biplott.Core.Entities;
using Biplott.Core.Interfaces;
using Microsoft.AspNetCore.Identity;

namespace Biplott.Application.Services;

public interface IAuthService
{
    Task<AuthResponse> RegisterAsync(RegisterRequest request, CancellationToken cancellationToken = default);
    Task<AuthResponse> LoginAsync(LoginRequest request, CancellationToken cancellationToken = default);
    Task<AuthResponse> RefreshTokenAsync(string refreshToken, CancellationToken cancellationToken = default);
    Task LogoutAsync(string userId, CancellationToken cancellationToken = default);
    Task<UserDto> GetCurrentUserAsync(string userId, CancellationToken cancellationToken = default);
}

public class AuthService : IAuthService
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly RoleManager<IdentityRole> _roleManager;
    private readonly IJwtTokenService _jwtTokenService;

    public AuthService(
        UserManager<ApplicationUser> userManager,
        RoleManager<IdentityRole> roleManager,
        IJwtTokenService jwtTokenService)
    {
        _userManager = userManager;
        _roleManager = roleManager;
        _jwtTokenService = jwtTokenService;
    }

    public async Task<AuthResponse> RegisterAsync(RegisterRequest request, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(request.Email))
        {
            throw new ArgumentException("Email không được để trống.");
        }

        if (string.IsNullOrWhiteSpace(request.Password))
        {
            throw new ArgumentException("Mật khẩu không được để trống.");
        }

        if (request.Password != request.ConfirmPassword)
        {
            throw new ArgumentException("Mật khẩu xác nhận không khớp.");
        }

        if (request.Password.Length < 6)
        {
            throw new ArgumentException("Mật khẩu phải có độ dài tối thiểu 6 ký tự.");
        }

        var existingUser = await _userManager.FindByEmailAsync(request.Email);
        if (existingUser != null)
        {
            throw new ArgumentException("Email này đã được sử dụng để đăng ký tài khoản.");
        }

        var displayName = !string.IsNullOrWhiteSpace(request.DisplayName)
            ? request.DisplayName.Trim()
            : request.Email.Split('@')[0];

        var user = new ApplicationUser
        {
            UserName = request.Email.Trim().ToLowerInvariant(),
            Email = request.Email.Trim().ToLowerInvariant(),
            DisplayName = displayName,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow,
            IsActive = true
        };

        var result = await _userManager.CreateAsync(user, request.Password);
        if (!result.Succeeded)
        {
            var errors = string.Join(" ", result.Errors.Select(e => e.Description));
            throw new ArgumentException($"Đăng ký thất bại: {errors}");
        }

        // Assign default User role
        if (!await _roleManager.RoleExistsAsync("User"))
        {
            await _roleManager.CreateAsync(new IdentityRole("User"));
        }
        await _userManager.AddToRoleAsync(user, "User");

        // Generate Refresh Token
        var refreshToken = _jwtTokenService.GenerateRefreshToken();
        user.RefreshToken = refreshToken;
        user.RefreshTokenExpiryTime = DateTime.UtcNow.AddDays(14);
        user.RefreshTokenRevokedAt = null;
        await _userManager.UpdateAsync(user);

        var roles = new List<string> { "User" };
        var (accessToken, expiresIn) = _jwtTokenService.GenerateAccessToken(
            user.Id,
            user.Email!,
            user.DisplayName,
            roles);

        return new AuthResponse
        {
            AccessToken = accessToken,
            RefreshToken = refreshToken,
            ExpiresIn = expiresIn,
            User = new UserDto
            {
                Id = user.Id,
                Email = user.Email!,
                DisplayName = user.DisplayName,
                Roles = roles,
                CreatedAt = user.CreatedAt
            }
        };
    }

    public async Task<AuthResponse> LoginAsync(LoginRequest request, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(request.Email) || string.IsNullOrWhiteSpace(request.Password))
        {
            throw new UnauthorizedAccessException("Vui lòng nhập đầy đủ email và mật khẩu.");
        }

        var user = await _userManager.FindByEmailAsync(request.Email.Trim());
        if (user == null)
        {
            throw new UnauthorizedAccessException("Email hoặc mật khẩu không chính xác.");
        }

        if (!user.IsActive)
        {
            throw new UnauthorizedAccessException("Tài khoản của bạn đã bị vô hiệu hóa.");
        }

        var isPasswordValid = await _userManager.CheckPasswordAsync(user, request.Password);
        if (!isPasswordValid)
        {
            throw new UnauthorizedAccessException("Email hoặc mật khẩu không chính xác.");
        }

        // Token Rotation: Generate a fresh refresh token upon each login
        var refreshToken = _jwtTokenService.GenerateRefreshToken();
        user.RefreshToken = refreshToken;
        user.RefreshTokenExpiryTime = DateTime.UtcNow.AddDays(14);
        user.RefreshTokenRevokedAt = null;
        user.UpdatedAt = DateTime.UtcNow;
        await _userManager.UpdateAsync(user);

        var roles = (await _userManager.GetRolesAsync(user)).ToList();
        var (accessToken, expiresIn) = _jwtTokenService.GenerateAccessToken(
            user.Id,
            user.Email!,
            user.DisplayName,
            roles);

        return new AuthResponse
        {
            AccessToken = accessToken,
            RefreshToken = refreshToken,
            ExpiresIn = expiresIn,
            User = new UserDto
            {
                Id = user.Id,
                Email = user.Email!,
                DisplayName = user.DisplayName,
                Roles = roles,
                CreatedAt = user.CreatedAt
            }
        };
    }

    public async Task<AuthResponse> RefreshTokenAsync(string refreshToken, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(refreshToken))
        {
            throw new UnauthorizedAccessException("Refresh token không hợp lệ.");
        }

        var users = _userManager.Users.Where(u => u.RefreshToken == refreshToken).ToList();
        var user = users.FirstOrDefault();

        if (user == null ||
            user.RefreshTokenExpiryTime == null ||
            user.RefreshTokenExpiryTime <= DateTime.UtcNow ||
            user.RefreshTokenRevokedAt != null ||
            !user.IsActive)
        {
            throw new UnauthorizedAccessException("Refresh token đã hết hạn hoặc bị thu hồi. Vui lòng đăng nhập lại.");
        }

        // Token Rotation: Issue a new refresh token and invalidate the old one
        var newRefreshToken = _jwtTokenService.GenerateRefreshToken();
        user.RefreshToken = newRefreshToken;
        user.RefreshTokenExpiryTime = DateTime.UtcNow.AddDays(14);
        user.UpdatedAt = DateTime.UtcNow;
        await _userManager.UpdateAsync(user);

        var roles = (await _userManager.GetRolesAsync(user)).ToList();
        var (accessToken, expiresIn) = _jwtTokenService.GenerateAccessToken(
            user.Id,
            user.Email!,
            user.DisplayName,
            roles);

        return new AuthResponse
        {
            AccessToken = accessToken,
            RefreshToken = newRefreshToken,
            ExpiresIn = expiresIn,
            User = new UserDto
            {
                Id = user.Id,
                Email = user.Email!,
                DisplayName = user.DisplayName,
                Roles = roles,
                CreatedAt = user.CreatedAt
            }
        };
    }

    public async Task LogoutAsync(string userId, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(userId)) return;

        var user = await _userManager.FindByIdAsync(userId);
        if (user != null)
        {
            user.RefreshTokenRevokedAt = DateTime.UtcNow;
            user.RefreshToken = null;
            user.UpdatedAt = DateTime.UtcNow;
            await _userManager.UpdateAsync(user);
        }
    }

    public async Task<UserDto> GetCurrentUserAsync(string userId, CancellationToken cancellationToken = default)
    {
        var user = await _userManager.FindByIdAsync(userId);
        if (user == null || !user.IsActive)
        {
            throw new KeyNotFoundException("Không tìm thấy thông tin tài khoản hoặc tài khoản đã bị khóa.");
        }

        var roles = (await _userManager.GetRolesAsync(user)).ToList();

        return new UserDto
        {
            Id = user.Id,
            Email = user.Email ?? string.Empty,
            DisplayName = user.DisplayName,
            Roles = roles,
            CreatedAt = user.CreatedAt
        };
    }
}
