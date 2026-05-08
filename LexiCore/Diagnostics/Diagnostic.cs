namespace LexiCore.Diagnostics;

/// <summary>
/// Represents a diagnostic produced during lexical or syntactic analysis.
/// </summary>
/// <remarks>
/// Diagnostics describe issues detected in the input during parsing.
/// Each diagnostic is associated with a location in the source text
/// and a diagnostic descriptor that defines its meaning and formatting.
/// 
/// <para>
/// Diagnostics describe issues detected in the input during analysis.
/// </para>
/// </remarks>
public readonly record struct Diagnostic(
    DiagnosticId Id,
    int SourceOffset,
    int RelativePosition,
    int Length = 1,
    object? Args = null,
    DiagnosticSeverity Severity = DiagnosticSeverity.Error
)
{
    public int AbsolutePosition => SourceOffset + RelativePosition;
    public int AbsoluteEndPosition => AbsolutePosition + Length;

    public DiagnosticDescriptor Descriptor => DiagnosticRegistry.Get(Id);
    public DiagnosticCode Code => Descriptor.Code;
    public string FormattedCode => $"LC{(ushort)Descriptor.Code:D4}";
    public string Message => Descriptor.Formatter(Args);
    public string PlainMessage => Message.TrimEnd('.');
}
