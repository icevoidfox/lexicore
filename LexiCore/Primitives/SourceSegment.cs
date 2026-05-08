namespace LexiCore.Primitives;

/// <summary>
/// Represents a contiguous segment of a source buffer using a start offset and length.
/// </summary>
/// <remarks>
/// This is a lightweight, allocation-free value type used to reference a slice of an existing character buffer
/// (typically <see cref="ReadOnlySpan{Char}"/> or <see cref="ReadOnlyMemory{Char}"/>).
///
/// <para>
/// The segment does not own the underlying data and must be used with the original source it was created for.
/// </para>
///
/// <para>
/// A segment is considered <b>not present</b> when <see cref="Start"/> is less than 0.
/// In this case, <see cref="Slice"/> returns an empty span.
/// </para>
/// </remarks>
public readonly struct SourceSegment(int start, int length)
{
    /// <summary>
    /// Represents a segment that is not present.
    /// </summary>
    public static readonly SourceSegment Missing = new(-1, 0);

    /// <summary>
    /// Gets the start offset of the segment within the source buffer.
    /// A negative value indicates that the segment is not present.
    /// </summary>
    public int Start { get; } = start;

    /// <summary>
    /// Gets the length of the segment.
    /// </summary>
    /// <remarks>
    /// The value is expected to be non-negative for valid segments.
    /// </remarks>
    public int Length { get; } = length;

    /// <summary>
    /// Gets the end offset of the segment within the source buffer.
    /// A negative value indicates that the segment is not present.
    /// </summary>
    public int End => Start + Length;

    /// <summary>
    /// Gets a value indicating whether this segment represents a valid slice
    /// (i.e., <see cref="Start"/> is non-negative).
    /// </summary>
    public bool IsPresent => Start >= 0;

    /// <summary>
    /// Returns a span representing this segment over the specified source buffer.
    /// </summary>
    /// <param name="source">The source buffer from which to slice.</param>
    /// <returns>
    /// A <see cref="ReadOnlySpan{Char}"/> corresponding to this segment,
    /// or an empty span if the segment is not present.
    /// </returns>
    /// <remarks>
    /// The caller must ensure that this segment is valid for the provided source.
    /// Invalid ranges will result in an exception thrown by <see cref="ReadOnlySpan{T}.Slice(int, int)"/>.
    /// </remarks>
    public ReadOnlySpan<char> Slice(ReadOnlySpan<char> sourceSpan)
        => IsPresent ? sourceSpan.Slice(Start, Length) : default;

    public ReadOnlySpan<char> Slice(ReadOnlyMemory<char> source) => Slice(source.Span);
}
