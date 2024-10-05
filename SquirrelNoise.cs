using System;
using System.Text;

// from https://gist.github.com/mrcarriere/90d63c6dd8f470e3994db8adedb9b4f5
public static class Squirrel3
{
	// From Squirrel Eiserloh's gdc '17 talk
	// https://www.gdcvault.com/play/1024365/Math-for-Game-Programmers-Noise
	const uint BIT_NOISE1 = 0xB5297A4D;
	const uint BIT_NOISE2 = 0x68E31DA4;
	const uint BIT_NOISE3 = 0x1B56C4E9;

	public static uint Hash(uint value, uint seed = 0u)
	{
		uint mangled = value;
		mangled *= BIT_NOISE1;
		mangled += seed;
		mangled ^= mangled >> 8;
		mangled += BIT_NOISE2;
		mangled ^= mangled << 8;
		mangled *= BIT_NOISE3;
		mangled ^= mangled >> 8;
		return mangled;
	}
}

public static class SquirrelNoise
{
	public static uint Get1D(uint x, uint seed)
	{
		return Squirrel3.Hash(x, seed);
	}

	public static uint Get2D(uint x, uint y, uint seed)
	{
		const int PRIME_NUMBER = 198491317; // Large prime number with non-bring bits
		return Get1D(x + PRIME_NUMBER * y, seed);
	}

	public static uint Get3D(uint x, uint y, uint z, uint seed)
	{
		const int PRIME1 = 198491317; // Large prime number with non-bring bits
		const int PRIME2 = 6542989; // Large prime number with non-bring bits
		return Get1D(x + PRIME1 * y + PRIME2 * z, seed);
	}
}

[Serializable]
public class SquirrelRandom
{
	// Public so they're serialized correctly.
	public uint seed;
	public uint state;

	public SquirrelRandom(uint seed)
	{
		this.seed = seed;
		state = 0;
	}

	public void ResetState()
	{
		state = 0;
	}

	public uint Next()
	{
		return SquirrelNoise.Get1D(state++, seed);
	}

	public float Next01()
	{
		return (float)Next() / uint.MaxValue;
	}

	public int Range(int upperBoundExc)
	{
		return Range(0, upperBoundExc);
	}

	public int Range(int lowerBoundIncl, int upperBoundExcl)
	{
		if (upperBoundExcl <= lowerBoundIncl)
		{
			return lowerBoundIncl;
		}

		return lowerBoundIncl + (int)System.Math.Floor(Next01() * (upperBoundExcl - lowerBoundIncl));
	}

	public float Range(float lowerBoundIncl, float upperBoundIncl)
	{
		if (upperBoundIncl <= lowerBoundIncl)
		{
			return lowerBoundIncl;
		}
		return lowerBoundIncl + Next01() * (upperBoundIncl - lowerBoundIncl);
	}

	public RNG Clone()
	{
		return (RNG)MemberwiseClone();
	}

	public override string ToString()
	{
		return $"RNG[seed={seed} state={state}]";
	}

	public int Sign()
	{
		return Range(0, 2) == 0 ? -1 : 1;
	}
}
