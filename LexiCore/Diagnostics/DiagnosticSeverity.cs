namespace LexiCore.Diagnostics;

public enum DiagnosticSeverity : byte
{
    Hidden = 0, // For tools
    Information = 1,
    Warning = 2,
    Error = 3
}
