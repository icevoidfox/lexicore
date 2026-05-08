using LexiCore.Diagnostics;
using LexiCore.Primitives;

namespace LexiCore.Tokens.Syntax;

/// <summary>
/// Represents a template token containing a identifier and an optional value segment.
/// </summary>
/// <param name="source">Input source memory segment produced by the lexer.</param>
/// <param name="identifierStart">Start offset of the identifier segment.</param>
/// <param name="identifierLength">Length of the identifier segment.</param>
/// <param name="valueKind">Structural interpretation mode of the value segment.</param>
/// <param name="valueStart">Start offset of the value segment.</param>
/// <param name="valueLength">Length of the value segment.</param>
/// <param name="isValid">Whether the token is syntactically correct and can be processed.</param>
/// <param name="diagnostics">Diagnostics associated with this token.</param>
/// <remarks>
/// A template token is produced by <see cref="Lexing.Lexers.TemplateLexer"/>
/// and represents a structured slice of the input source.
///
/// <para>
/// Stores identifier and value segments as offsets within its own <see cref="SyntaxTokenBase.Source"/>.
/// Provides both zero-allocation span access and lazily materialized string representations.
/// </para>
/// </remarks>
public sealed class TemplateToken(
    ReadOnlyMemory<char> source,
    int identifierStart,
    int identifierLength,
    TemplateValueKind valueKind,
    int valueStart,
    int valueLength,
    bool isValid,
    ReadOnlyMemory<Diagnostic> diagnostics = default
) : SyntaxTokenBase(source, isValid, diagnostics)
{
    /// <summary>
    /// Gets the identifier segment within the token source.
    /// </summary>
    public SourceSegment Identifier { get; } = new(identifierStart, identifierLength);

    /// <summary>
    /// Gets the structural interpretation mode of the value segment.
    /// </summary>
    public TemplateValueKind ValueKind { get; } = valueKind;

    /// <summary>
    /// Gets the value segment within the token source.
    /// </summary>
    public SourceSegment Value { get; } = new(valueStart, valueLength);

    /// <summary>
    /// Gets a value indicating whether the token contains a identifier segment.
    /// </summary>
    public bool HasIdentifier => Identifier.IsPresent;

    /// <summary>
    /// Gets a value indicating whether the token contains a value segment.
    /// </summary>
    public bool HasValue => Value.IsPresent;

    /// <summary>
    /// Gets the raw identifier segment as a span over the token source.
    /// Returns an empty span if the identifier is not present.
    /// </summary>
    public ReadOnlySpan<char> RawIdentifier => Identifier.Slice(Source);

    /// <summary>
    /// Gets the raw value segment as a span over the token source.
    /// Returns an empty span if the value is not present.
    /// </summary>
    public ReadOnlySpan<char> RawValue => Value.Slice(Source);
}
