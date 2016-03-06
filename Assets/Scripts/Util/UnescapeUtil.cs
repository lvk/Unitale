public static class UnescapeUtil
{
    public static string unescape(string str)
    {
        str = str.Replace("\\n", "\n");
        str = str.Replace("\\r", "\r");
        str = str.Replace("\\t", "\t");
        return str;
    }
}