namespace LexiCore.Sandbox.Utils;

public static class Styles
{
    // General
    public static readonly AnsiStyle Default = AnsiStyle.Reset;
    public static readonly AnsiStyle Active = new(Foreground: AnsiForegroundColor.Black, Background: AnsiBackgroundColor.White);
    public static readonly AnsiStyle Inactive = new(Foreground: AnsiForegroundColor.BrightBlack, Background: AnsiBackgroundColor.Reset);

    // Structures
    public static readonly AnsiStyle Label = new(Foreground: AnsiForegroundColor.BrightWhite);
    public static readonly AnsiStyle ListMark = new(Foreground: AnsiForegroundColor.Cyan);
    public static readonly AnsiStyle Caret = Default;

    // Value Types
    public static readonly AnsiStyle Type = new(Foreground: AnsiForegroundColor.Yellow);
    public static readonly AnsiStyle Range = new(Foreground: AnsiForegroundColor.BrightYellow);
    public static readonly AnsiStyle String = new(Foreground: AnsiForegroundColor.Green);
    public static readonly AnsiStyle Identifier = new(Foreground: AnsiForegroundColor.Cyan);
    public static readonly AnsiStyle Value = Default;

    // Token Diagnostic
    public static readonly AnsiStyle DiagnosticRange = new(Foreground: AnsiForegroundColor.Red);
    public static readonly AnsiStyle DiagnosticInsertion = new(Foreground: AnsiForegroundColor.Reset, Background: AnsiBackgroundColor.Red);
    public static readonly AnsiStyle DiagnosticExternalInsertion = new(Background: AnsiBackgroundColor.Red);

    // Diagnostic Severity
    public static readonly AnsiStyle DiagnosticHidden = Default;
    public static readonly AnsiStyle DiagnosticInfo = new(Foreground: AnsiForegroundColor.Cyan);
    public static readonly AnsiStyle DiagnosticWarn = new(Foreground: AnsiForegroundColor.Yellow);
    public static readonly AnsiStyle DiagnosticError = new(Foreground: AnsiForegroundColor.Red);
}
