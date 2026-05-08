using LexiCore.Primitives;

namespace LexiCore.Tokens.Syntax;

/// <summary>
/// Represents a lexical instruction extracted from the source span.
/// </summary>
/// <remarks>
/// Lexical instructions are structural projections produced by the lexer.
/// They describe the layout of a style token in terms of source-relative spans,
/// including identifier and value segments.
///
/// <para>
/// These instructions are not semantic and do not represent executable or
/// intermediate representation (IR) data. They are intended for further
/// syntactic processing in later pipeline stages.
/// </para>
///
/// <para>
/// Instructions are ordered according to their appearance in the source and
/// may represent repeated structural patterns for the same logical construct.
/// </para>
/// </remarks>
public sealed class StyleInstruction(
    int relativeStart,
    int length,
    StyleInstructionKind kind,
    int identifierStart,
    int identifierLength,
    int valueStart,
    int valueLength
)
{
    /// <summary>
    /// Gets the instruction segment relative to the beginning of <see cref="StyleToken.Source"/>.
    /// </summary>
    public SourceSegment Source { get; } = new(relativeStart, length);

    /// <summary>
    /// Gets the structural instruction kind.
    /// </summary>
    public StyleInstructionKind Kind { get; } = kind;
    
    /// <summary>
    /// Gets the identifier segment within the token source.
    /// </summary>
    public SourceSegment Identifier { get; } = new(identifierStart, identifierLength);

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
}
