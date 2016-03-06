using System.Collections;
using UnityEngine;

/// <summary>
/// Utility class for the Unitale engine.
/// </summary>
public static class UnitaleUtil
{
    internal static bool firstErrorShown = false; //Keeps track of whether an error already appeared, prevents subsequent errors from overriding the source.

    /// <summary>
    /// This was previously used to create error messages for display in the UI controller, but is now obsolete as this is displayed in a separate scene.
    /// </summary>
    /// <param name="source">Name of the offending script</param>
    /// <param name="decoratedMessage">Decorated error messages as given by the InterpreterException thrown by the Lua script</param>
    /// <returns>TextMessage for display in a TextManager.</returns>
    public static TextMessage createLuaError(string source, string decoratedMessage)
    {
        string returnValue = "[font:monster][color:ffffff]error in script " + source + "\n";
        int lineLetterCount = 0;
        int maxChars = 50;
        for (int i = 0; i < decoratedMessage.Length; i++)
        {
            if (lineLetterCount >= maxChars && decoratedMessage[i] == ' ' || decoratedMessage[i] == '\n')
            {
                returnValue += "\n"; // linebreak on spaces after maxChars characters
                lineLetterCount = 0;
            }
            else
            {
                returnValue += decoratedMessage[i];
                lineLetterCount++;
            }
        }
        return new TextMessage(returnValue, false, true);
    }

    /// <summary>
    /// Loads the Error scene with the Lua error that occurred.
    /// </summary>
    /// <param name="source">Name of the script that caused the error.</param>
    /// <param name="decoratedMessage">Error that was thrown. In MoonSharp's case, this is the DecoratedMessage property from its InterpreterExceptions.</param>
    public static void displayLuaError(string source, string decoratedMessage)
    {
        if (firstErrorShown)
        {
            return;
        }

        /*if (UIController.instance == null)
            UIController.errorMsg = createLuaError(source, decoratedMessage);
        else
            UIController.instance.ShowError(createLuaError(source, decoratedMessage));*/

        firstErrorShown = true;
        ErrorDisplay.Message = "error in script " + source + "\n\n" + decoratedMessage;
        if (Application.isEditor)
        {
            Application.LoadLevelAsync("Error"); // prevents editor from crashing
        }
        else
        {
            Application.LoadLevel("Error");
        }
        Debug.Log("It's a Lua error!");
    }

    public static int fontStringWidth(UnderFont font, string s, int hSpacing = 3)
    {
        int width = 0;
        foreach (char c in s)
        {
            if (font.Letters.ContainsKey(c))
            {
                width += (int)font.Letters[c].rect.width + hSpacing;
            }
        }
        return width;
    }
}