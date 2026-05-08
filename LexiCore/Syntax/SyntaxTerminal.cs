namespace LexiCore.Syntax;

/// <summary>
/// Represents a lexical terminal symbol used in syntax definitions.
/// A terminal can be either a single character or a multi-character string sequence
/// that participates in lexical analysis (e.g. "{", "}", "::").
/// </summary>
public readonly struct SyntaxTerminal
{
    public const char SentinelMarker = '\0';

    /// <summary>
    /// Represents a sentinel value for invalid or missing terminal symbols.
    /// Not part of the valid syntax definitions.
    /// </summary>
    public static readonly SyntaxTerminal Sentinel = new(SentinelMarker);

    public string Value { get; }
    public char FirstChar { get; }

    public SyntaxTerminal(string value)
    {
        if (string.IsNullOrEmpty(value))
        {
            throw new ArgumentException($"{nameof(SyntaxTerminal)} cannot be of zero length.", nameof(value));
        }
        Value = value;
        FirstChar = value[0];
    }

    public SyntaxTerminal(char c)
    {
        Value = c.ToString();
        FirstChar = c;
    }

    public int Length => Value.Length;
    public bool IsChar => Length == 1;
    public bool IsSentinel => IsChar && FirstChar == SentinelMarker;

    public char this[int index] => Value[index];

    public static implicit operator string(SyntaxTerminal terminal) => terminal.Value;
    public static implicit operator ReadOnlySpan<char>(SyntaxTerminal terminal) => terminal.Value.AsSpan();

    public override string ToString() => Value;
}
