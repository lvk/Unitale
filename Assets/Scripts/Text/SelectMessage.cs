using System;

public class SelectMessage : TextMessage
{
    public SelectMessage(string[] options, bool singleList, string[] colorPrefixes = null)
        : base("", false, true)
    {
        string finalMessage = "";
        string rowOneSpacing = "  ";
        string rowTwoSpacing = "\t";
        string prefix = "* ";
        if (options.Length == 0)
            throw new ArgumentException("Can't create a text message for zero options.");

        for (int i = 0; i < options.Length; i++)
        {
            string intermedPrefix = "";
            string intermedSuffix = "";
            if (colorPrefixes != null && i < colorPrefixes.Length && !String.IsNullOrEmpty(colorPrefixes[i]))
            {
                intermedPrefix = colorPrefixes[i];
                intermedSuffix = "[color:ffffff]";
            }
            if (options[i] == null || options[i] == "")
                prefix = "";
            if (singleList)
            {
                finalMessage += rowOneSpacing + intermedPrefix + prefix + options[i] + intermedSuffix + "\n";
            }
            else
            {
                if (i % 2 == 0)
                {
                    finalMessage += rowOneSpacing + intermedPrefix + prefix + options[i] + intermedSuffix;
                }
                else
                {
                    finalMessage += rowTwoSpacing + intermedPrefix + prefix + options[i] + intermedSuffix + "\n";
                }
            }
        }

        setup(finalMessage, false, true);
    }
}