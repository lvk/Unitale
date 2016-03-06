public class TextMessage
{
    public TextMessage(string text, bool formatted, bool showImmediate)
    {
        setup(text, formatted, showImmediate);
    }

    public string Text { get; private set; }
    public bool Formatted { get; private set; }
    public bool ShowImmediate { get; private set; }

    protected void setup(string text, bool formatted, bool showImmediate)
    {
        text = UnescapeUtil.unescape(text); // compensate for unity inspector autoescaping control characters
        if (formatted)
            this.Text = formatText(text);
        else
            this.Text = text;
        this.Formatted = formatted;
        this.ShowImmediate = showImmediate;
    }

    private string formatText(string text)
    {
        string newText = "* ";
        foreach (char c in text)
        {
            if (c == '\n')
                newText += "\n* ";
            else if (c == '\r')
                newText += "\n  ";
            else
                newText += c;
        }
        return newText;
    }
}