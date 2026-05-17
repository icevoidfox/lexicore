using FluentAssertions;
using FluentAssertions.Execution;

using LexiCore.Abstractions;
using LexiCore.Diagnostics;
using LexiCore.Tokens.Syntax;

namespace LexiCore.Tests.Assertions;

internal static class LexerAssert
{
    internal static void LexicalToken(ILexicalToken actual, string expectedSource)
        => actual.Source.ToString().Should().Be(expectedSource);
    
    internal static void SyntaxToken(
        SyntaxTokenBase actual,
        string expectedSource,
        bool expectedIsValid,
        DiagnosticExpectation[]? expectedDiagnostics = null
    )
    {
        using (new AssertionScope())
        {
            LexicalToken(actual, expectedSource);
            actual.IsValid.Should().Be(expectedIsValid);
            Diagnostics(expectedDiagnostics ?? [], actual.Diagnostics.ToArray());
        }
    }
    
    internal static void TemplateToken(
        TemplateToken actual,
        string expectedSource,
        bool expectedIsValid,
        string expectedIdentifier = "",
        TemplateValueKind expectedValueKind = TemplateValueKind.None,
        string expectedValue = "",
        DiagnosticExpectation[]? expectedDiagnostics = null
    )
    {
        using (new AssertionScope())
        {
            SyntaxToken(actual, expectedSource, expectedIsValid, expectedDiagnostics);
            actual.RawIdentifier.ToString().Should().Be(expectedIdentifier);
            actual.ValueKind.Should().Be(expectedValueKind);
            actual.RawValue.ToString().Should().Be(expectedValue);
        }
    }

    // This is a lightweight check exclusively for the semantic correctness of diagnostics.
    internal static void Diagnostics(
        DiagnosticExpectation[] expected,
        Diagnostic[] actual
    )
    {
        actual.Should().HaveCount(expected.Length);
        for (int i = 0; i < expected.Length; i++)
        {
            actual[i].Id.Should().Be(expected[i].Id);
            actual[i].Severity.Should().Be(expected[i].Severity);
        }
    }
}
