namespace MultiMillionaire.Models;

public static class Extensions
{
    public static void Shuffle<T>(this IList<T> list, Random rng)
    {
        var n = list.Count;
        while (n > 1)
        {
            n--;
            var k = rng.Next(n + 1);
            (list[k], list[n]) = (list[n], list[k]);
        }
    }

    public static T ChooseRandom<T>(this IList<T> list, Random rng)
    {
        return list[rng.Next(list.Count)];
    }
}