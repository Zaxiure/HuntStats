namespace HuntStats.Extensions;

public static class GeneralExtensions
{
    public static string FirstLetterUpper(this string value)
    {
        return char.ToUpper(value[0]) + value.Substring(1);
    }

    public static int ToStarRating(this int value)
    {
        if (value >= 0 && value < 2000) return 1;

        if (value >= 2000 && value < 2300) return 2;

        if (value >= 2300 && value < 2600) return 3;

        if (value >= 2600 && value < 2750) return 4;

        if (value >= 2750 && value < 3000) return 5;

        return 6;
    }
}