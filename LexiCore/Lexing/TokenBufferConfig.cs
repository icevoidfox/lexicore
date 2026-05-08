namespace LexiCore.Lexing;

/// <summary>
/// Configuration used to estimate the initial capacity of the token buffer during lexing.
/// </summary>
/// <remarks>
/// This configuration helps reduce reallocations by applying a heuristic based on the input length.
/// It does not affect the correctness of lexing,
/// only memory allocation behavior.
/// </remarks>
/// <param name="InitialCapacityMin">Minimum initial capacity for the internal token list.</param>
/// <param name="InitialCapacityMax">Maximum initial capacity for the internal token list.</param>
/// <param name="CharsPerTokenRatio">
/// Heuristic ratio of characters per token used to estimate capacity.
/// Lower values increase initial allocation, reducing resizing at the cost of memory.
/// </param>
public sealed record TokenBufferConfig(
    int InitialCapacityMin = 4,
    int InitialCapacityMax = 1024,
    int CharsPerTokenRatio = 32
)
{
    /// <summary>
    /// Default configuration used when no custom settings are provided.
    /// </summary>
    public static readonly TokenBufferConfig Default = new();

    /// <summary>
    /// Estimates the initial capacity for a token buffer based on the input length.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public int GetInitialCapacity(int sourceLength) => Math.Min(
        InitialCapacityMax,
        Math.Max(InitialCapacityMin, sourceLength / CharsPerTokenRatio)
    );
}
