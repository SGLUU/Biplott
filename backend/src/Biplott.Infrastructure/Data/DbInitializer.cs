using Biplott.Core.Entities;
using Biplott.Core.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Biplott.Infrastructure.Data;

public static class DbInitializer
{
    public static async Task SeedAsync(BiplottDbContext context, ILogger logger, CancellationToken cancellationToken = default)
    {
        try
        {
            // Seed Games & Pools
            if (!await context.Games.AnyAsync(cancellationToken))
            {
                logger.LogInformation("Seeding default Games & GamePools...");

                var games = new List<Game>
                {
                    new()
                    {
                        Code = "POWER_655",
                        Name = "Power 6/55",
                        Description = "6 con số cơ hội đổi đời hoặc đổi chỗ ngủ",
                        Tagline = "Jackpot trăm tỷ vẫy gọi",
                        IconUrl = "/icons/power-655.svg",
                        IsActive = true,
                        SortOrder = 1,
                        Pools = new List<GamePool>
                        {
                            new()
                            {
                                PoolIndex = 0,
                                Name = "Dãy số chính",
                                MinNumber = 1,
                                MaxNumber = 55,
                                PickCount = 6,
                                AllowDuplicates = false,
                                BadgeColor = "#EF4444" // Đỏ
                            }
                        }
                    },
                    new()
                    {
                        Code = "MEGA_645",
                        Name = "Mega 6/45",
                        Description = "6 con số khởi đầu giấc mơ tự do tài chính",
                        Tagline = "Dễ thở hơn Power, nát vừa phải",
                        IconUrl = "/icons/mega-645.svg",
                        IsActive = true,
                        SortOrder = 2,
                        Pools = new List<GamePool>
                        {
                            new()
                            {
                                PoolIndex = 0,
                                Name = "Dãy số chính",
                                MinNumber = 1,
                                MaxNumber = 45,
                                PickCount = 6,
                                AllowDuplicates = false,
                                BadgeColor = "#F97316" // Cam
                            }
                        }
                    },
                    new()
                    {
                        Code = "LOTTO_535",
                        Name = "Lotto 5/35",
                        Description = "5 số chính kết hợp 1 con số vận mệnh đặc biệt",
                        Tagline = "5 số đời thường + 1 số tâm linh",
                        IconUrl = "/icons/lotto-535.svg",
                        IsActive = true,
                        SortOrder = 3,
                        Pools = new List<GamePool>
                        {
                            new()
                            {
                                PoolIndex = 0,
                                Name = "Dãy số chính",
                                MinNumber = 1,
                                MaxNumber = 35,
                                PickCount = 5,
                                AllowDuplicates = false,
                                BadgeColor = "#F97316" // Cam
                            },
                            new()
                            {
                                PoolIndex = 1,
                                Name = "Số đặc biệt",
                                MinNumber = 1,
                                MaxNumber = 12,
                                PickCount = 1,
                                AllowDuplicates = false,
                                BadgeColor = "#FACC15" // Vàng
                            }
                        }
                    }
                };

                await context.Games.AddRangeAsync(games, cancellationToken);
                await context.SaveChangesAsync(cancellationToken);
            }

            // Seed Themes & Traits
            if (!await context.Themes.AnyAsync(cancellationToken))
            {
                logger.LogInformation("Seeding default Themes and Traits...");

                var themes = new List<Theme>
                {
                    new() { Code = "THEME_WORK", Name = "Chuyện công sở", Description = "Deadline, drama, sếp và đồng nghiệp", Icon = "briefcase", SortOrder = 1 },
                    new() { Code = "THEME_FINANCE", Name = "Tài chính & Đu đỉnh", Description = "Lương, thưởng, coin, chứng và mì tôm", Icon = "trending-down", SortOrder = 2 },
                    new() { Code = "THEME_LOVE", Name = "Tình duyên & Thính", Description = "Độc thân quý tộc, người yêu cũ và quẻ bói tình", Icon = "heart-crack", SortOrder = 3 },
                    new() { Code = "THEME_SPIRIT", Name = "Tâm linh Meme", Description = "Mèo thần tài, giấc mơ báo mộng và điềm lành", Icon = "sparkles", SortOrder = 4 }
                };

                var traits = new List<Trait>
                {
                    new() { Code = "ChaosEnergy", Name = "Năng lượng Hỗn loạn", Description = "Thích nổi loạn, bùng nổ, phá vỡ quy tắc", Category = "Personality" },
                    new() { Code = "RiskTolerance", Name = "Chấp nhận Rủi ro", Description = "Dám liều, all-in, được ăn cả ngã về không", Category = "Behavior" },
                    new() { Code = "SpiritualVibe", Name = "Độ nhạy Tâm linh", Description = "Tin vào trực giác, duyên số, vũ trụ mách bảo", Category = "Belief" },
                    new() { Code = "DesperationLevel", Name = "Mức độ Bất lực", Description = "Khát khao đổi đời, nát toàn phần", Category = "Status" },
                    new() { Code = "Patience", Name = "Độ Kiên nhẫn", Description = "Trầm tĩnh, tích tiểu thành đại", Category = "Personality" }
                };

                await context.Themes.AddRangeAsync(themes, cancellationToken);
                await context.Traits.AddRangeAsync(traits, cancellationToken);
                await context.SaveChangesAsync(cancellationToken);
            }

            // Seed Sample Questions
            if (!await context.Questions.AnyAsync(cancellationToken))
            {
                logger.LogInformation("Seeding initial Sample Questions...");

                var workTheme = await context.Themes.FirstAsync(t => t.Code == "THEME_WORK", cancellationToken);
                var chaosTrait = await context.Traits.FirstAsync(t => t.Code == "ChaosEnergy", cancellationToken);
                var riskTrait = await context.Traits.FirstAsync(t => t.Code == "RiskTolerance", cancellationToken);

                var q1 = new Question
                {
                    ThemeId = workTheme.Id,
                    QuestionType = QuestionType.ThisOrThat,
                    Content = "Sáng thứ 2 bước vào văn phòng, bạn muốn điều gì xảy ra hơn?",
                    Subtitle = "Chọn thật lòng, vận mệnh sẽ thành tâm",
                    IsActive = true,
                    Choices = new List<QuestionChoice>
                    {
                        new()
                        {
                            Content = "Sếp thông báo đi công tác nguyên tuần",
                            SubContent = "Văn phòng tự do muôn năm",
                            OrderIndex = 1,
                            ChoiceTraits = new List<ChoiceTrait>
                            {
                                new() { TraitId = chaosTrait.Id, Weight = 0.8 }
                            }
                        },
                        new()
                        {
                            Content = "Máy pha cà phê vừa được nạp hạt mới",
                            SubContent = "Tập trung nạp caffein chiến deadline",
                            OrderIndex = 2,
                            ChoiceTraits = new List<ChoiceTrait>
                            {
                                new() { TraitId = riskTrait.Id, Weight = -0.5 }
                            }
                        }
                    }
                };

                await context.Questions.AddAsync(q1, cancellationToken);
                await context.SaveChangesAsync(cancellationToken);
            }

            // Seed full 10 Themes & 100+ Questions Content
            await ContentSeeder.SeedContentAsync(context, logger, cancellationToken);

            logger.LogInformation("Database seeding completed successfully.");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "An error occurred during database seeding.");
            throw;
        }
    }
}
