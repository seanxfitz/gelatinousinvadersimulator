using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System;
using Godot;

public class RNG
{
    public static void Initialize(uint seed)
    {
        RNG.Combat = new RandomOperations(seed);
        RNG.Generation = new RandomOperations(seed);
    }

    public static RandomOperations Combat;
    public static RandomOperations Generation;
    public static RandomOperations Debug = new RandomOperations(RandomSeed());

    public static uint RandomSeed()
    {
        var buff = new byte[sizeof(uint)];
        new System.Random().NextBytes(buff);
        uint res = System.BitConverter.ToUInt32(buff, 0);
        return res;
    }
    private static char[] chars = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ1234567890".ToCharArray();

    public static string RandomSeedString()
    {
        var rand = new System.Random();
        var length = rand.Next(1, 9);
        var stringChars = new char[length];
        for(int i = 0; i < length; i ++)
        {
            stringChars[i] = chars[rand.Next(chars.Length)];
        }
        return new string(stringChars);
    }
}

public class RandomOperations
{
    public RandomOperations(uint seed)
    {
        this.rand = new SquirrelRandom(seed);
    }

    public SquirrelRandom rand;
    /// <summary>
    /// Given a "chance" (float) between 0.0 and 1.0 return true or false based on if a number is randomly selected below the chance
    /// A chance of 0.0 will always return `false`
    /// A chance of >= 1.0 will always return `true`
    /// </summary>
    /// <returns></returns>
    public bool PassWithChance(double chance)
    {
        if (chance <= 0) return false;
        return rand.Range(0, 100) <= (chance * 100);
    }

    public bool PassWithChance(int chance)
    {
        if (chance <= 0) return false;
        return rand.Range(0, 100) <= chance;
    }

    public int GetInRange(int min, int max)
    {
        return rand.Range(min, max);
    }

    public float GetInRange(float min, float max)
    {
        return rand.Range(min, max);
    }

    public T RandomPick<T>(List<T> list)
    {
        var index = rand.Range(0, list.Count);
        return list[index];
    }

    public T RandomPick<T>(IEnumerable<T> enumerable)
    {
        var index = rand.Range(0, enumerable.Count());
        return enumerable.ElementAt(index);
    }

    public IEnumerable<T> RandomPickCount<T>(IEnumerable<T> enumerable, int amount)
    {
        var resultList = new List<T>();
        var availableList = new List<T>();
        foreach (T item in enumerable)
        {
            availableList.Add(item);
        }

        for (int maxAmount = Mathf.Clamp(amount, 0, availableList.Count); maxAmount > 0; maxAmount--)
        {
            var picked = RandomPick(availableList);
            resultList.Add(picked);
            availableList.Remove(picked);
        }

        return resultList;
    }

    public List<T> RandomPickCount<T>(List<T> list, int amount)
    {
        var resultList = new List<T>();
        var availableList = new List<T>();
        foreach (T item in list)
        {
            availableList.Add(item);
        }

        for (int maxAmount = Mathf.Clamp(amount, 0, availableList.Count); maxAmount > 0; maxAmount--)
        {
            var picked = RandomPick(availableList);
            resultList.Add(picked);
            availableList.Remove(picked);
        }

        return resultList;
    }

    public T WeightedPick<T>(List<T> list) where T : IWeighted
    {
        var totalWeight = list.Sum(item => item.weight);
        var selection = GetInRange(0, totalWeight);
        var sum = 0;

        foreach (var item in list)
        {
            sum += item.weight;
            if (sum >= selection)
                return item;
        }

        return list.First();
    }
}

public interface IWeighted
{
    int weight { get; }
}
