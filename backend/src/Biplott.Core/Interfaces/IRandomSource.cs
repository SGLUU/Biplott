namespace Biplott.Core.Interfaces;

public interface IRandomSource
{
    int NextInt(int minInclusive, int maxExclusive);
    double NextDouble();
}
