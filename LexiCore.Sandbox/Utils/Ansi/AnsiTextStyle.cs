namespace LexiCore.Sandbox.Utils;

[Flags]
public enum AnsiTextStyle
{
    None = 0,

    Bold = 1 << 0,
    Italic = 1 << 1,
    Underline = 1 << 2,

    ResetBold = 1 << 3,
    ResetItalic = 1 << 4,
    ResetUnderline = 1 << 5,
}
