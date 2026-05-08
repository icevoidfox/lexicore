namespace LexiCore.Sandbox.Utils;

public readonly record struct AnsiStyle
(
    AnsiTextStyle Text = AnsiTextStyle.None,
    AnsiForegroundColor? Foreground = null,
    AnsiBackgroundColor? Background = null
)
{
    public static readonly AnsiStyle Reset = new();

    public override string ToString()
    {
        List<int> styles = [];


        if (Text == AnsiTextStyle.None && Foreground is null && Background is null)
        {
            styles.Add(0);
        }
        else
        {
            // I want to say that I'm usually against such helpers and shortened if/else blocks,
            // but I'm betting on simplicity in this temporary functionality.
            if (Text != AnsiTextStyle.None)
            {
                if (Has(AnsiTextStyle.Bold)) styles.Add(1);
                if (Has(AnsiTextStyle.Italic)) styles.Add(3);
                if (Has(AnsiTextStyle.Underline)) styles.Add(4);
                // This is a bad way to reset the text style, but I find it most convenient here and now.
                if (Has(AnsiTextStyle.ResetBold)) styles.Add(22);
                if (Has(AnsiTextStyle.ResetItalic)) styles.Add(23);
                if (Has(AnsiTextStyle.ResetUnderline)) styles.Add(24);
            }

            if (Foreground is not null) styles.Add((int)Foreground);
            if (Background is not null) styles.Add((int)Background);
        }

        return $"\x1b[{string.Join(';', styles)}m";
    }

    public string Apply(string text) => $"{this}{text}{Reset}";

    private bool Has(AnsiTextStyle style) => Text.HasFlag(style);
}
