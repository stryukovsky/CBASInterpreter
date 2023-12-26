namespace CBASInterpreter;

public static class StringExtension
{
    public static bool IsSpace(this string str)
    {
        return str.All(item => char.IsWhiteSpace(item));
    }

    public static bool IsEqualsBracketCount(this string str)
    {
        var open = 0;
        var close = 0;

        foreach (var t in str)
        {
            if (t == '(')
                open++;
            else if (t == ')')
                close++;
        }

        return open == close;
    }
}
