using Lifx;

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

    public static async Task SetColourFromBackgroundImage(this ILight light, int imageNumber,
        bool useRedVariant = false)
    {
        if (useRedVariant)
        {
            var colour = imageNumber switch
            {
                1 => new Color(0, 1),
                _ => new Color(0, 0.68)
            };

            await light.SetColorAndBrightness(colour, 1.0, 1000);
        }
        else
        {
            var colour = imageNumber switch
            {
                1 => new Color(210, 1),
                2 => new Color(220, 1),
                3 => new Color(240, 1),
                _ => new Color(25, 0.501)
            };

            var brightness = imageNumber switch
            {
                3 => 0.75,
                _ => 1
            };

            await light.SetColorAndBrightness(colour, brightness, 1000);
        }
    }
}