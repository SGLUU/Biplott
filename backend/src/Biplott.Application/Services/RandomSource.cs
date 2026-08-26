using System.Security.Cryptography;
using Biplott.Core.Interfaces;

namespace Biplott.Application.Services;

public class CryptographicRandomSource : IRandomSource
{
    public int NextInt(int minInclusive, int maxExclusive)
    {
        if (minInclusive >= maxExclusive)
            throw new ArgumentOutOfRangeException(nameof(minInclusive), "minInclusive must be less than maxExclusive");

        return RandomNumberGenerator.GetInt32(minInclusive, maxExclusive);
    }

    public double NextDouble()
    {
        Span<byte> buffer = stackalloc byte[8];
        RandomNumberGenerator.Fill(buffer);
        ulong randUInt64 = BitConverter.ToUInt64(buffer);
        // Normalize to [0.0, 1.0)
        return (double)(randUInt64 >> 11) * (1.0 / (1UL << 53));
    }
}

public class DeterministicRandomSource : IRandomSource
{
    private readonly Random _random;

    public DeterministicRandomSource(int seed = 42)
    {
        _random = new Random(seed);
    }

    public int NextInt(int minInclusive, int maxExclusive)
    {
        return _random.Next(minInclusive, maxExclusive);
    }

    public double NextDouble()
    {
        return _random.NextDouble();
    }
}
