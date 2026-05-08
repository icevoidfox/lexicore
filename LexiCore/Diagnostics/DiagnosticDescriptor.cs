namespace LexiCore.Diagnostics;

/// <summary>
/// Defines how a diagnostic is represented: its code and message formatter.
/// </summary>
/// <remarks>
/// Resolved via <see cref="DiagnosticRegistry"/> from a <see cref="DiagnosticId"/>.
/// </remarks>
public readonly record struct DiagnosticDescriptor(
    DiagnosticCode Code,
    Func<object?, string> Formatter
);
