namespace LexiCore.Syntax;

/// <summary>
/// Shared lexical constants used across multiple language syntaxes.
/// </summary>
/// <remarks>
/// Provides common character sets and lookup tables for identifier parsing
/// and escaping rules used by lexers in <c>LexiCore</c>.
/// </remarks>
public static class SharedSyntax
{
    public const char EscapeChar = '\\';

    public const string LatinLetters = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz";
    public const string Digits = "0123456789";

    public const string ExtraIdentifierStart = "_";
    public const string ExtraIdentifierBody = "_-";

    /// <summary>
    /// Lookup table for valid identifier starting characters.
    /// </summary>
    public static readonly SearchValues<char> IdentifierStart = SearchValues.Create(LatinLetters + ExtraIdentifierStart);

    /// <summary>
    /// Lookup table for valid identifier body characters.
    /// </summary>
    public static readonly SearchValues<char> IdentifierBody = SearchValues.Create(LatinLetters + Digits + ExtraIdentifierBody);
}
