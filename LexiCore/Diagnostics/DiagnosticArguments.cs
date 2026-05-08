namespace LexiCore.Diagnostics;

// Internal payload types for diagnostic formatting.
//
// Provide structured arguments for diagnostic formatters without exposing
// untyped `object?` payloads in the public API surface.

internal readonly record struct TooLongArgs(int Overflow, int Max);

internal readonly record struct CharArgs(char Character);
