using System;

using LexiCore.Diagnostics;
using LexiCore.Lexing;
using LexiCore.Lexing.Lexers;
using LexiCore.Syntax;
using LexiCore.Tokens.Syntax;

using LexiCore.Tests.Assertions;

namespace LexiCore.Tests.Lexing.Lexers;

public class TemplateLexerTests
{
    #region Happy Path

    [Theory]
    [InlineData("{User}", "User", TemplateValueKind.None, "")]
    [InlineData("{User::[Lyrica]}", "User", TemplateSyntax.DefaultValueKind, "Lyrica")]
    [InlineData("{Timestamp::f[HH:mm]}", "Timestamp", TemplateValueKind.Format, "HH:mm")]
    public void LexToken_ValidSyntax_ParsesCorrectly(
        string input,
        string expectedIdentifier,
        TemplateValueKind expectedValueKind,
        string expectedValue
    )
    {
        var token = Run(input);

        LexerAssert.TemplateToken(
            token,
            expectedSource: input,
            expectedIsValid: true,
            expectedIdentifier,
            expectedValueKind,
            expectedValue
        );
    }

    #endregion

    #region Validation (options)
    
    [Fact]
    public void LexToken_WhenIdentifierExceedsHardLimit_ReturnsInvalidWithError()
    {
        var source = "{MegaLongUser}";

        var token = Run(source, options: new(MaxIdentifierLength: 10));

        LexerAssert.TemplateToken(
            token,
            source,
            expectedIsValid: false,
            expectedIdentifier: "MegaLongUser",
            expectedDiagnostics: [
                new(DiagnosticId.TemplateIdentifierTooLong, DiagnosticSeverity.Error),
            ]
        );
    }

    [Fact]
    public void LexToken_WhenIdentifierExceedsSoftLimit_ReturnsValidWithWarning()
    {
        var source = "{MegaLongUser}";

        var token = Run(source, options: new(MaxIdentifierLength: 10, Tolerance: TemplateTolerance.AllowLongIdentifiers));

        LexerAssert.TemplateToken(
            token,
            source,
            expectedIsValid: true,
            expectedIdentifier: "MegaLongUser",
            expectedDiagnostics: [
                new(DiagnosticId.TemplateIdentifierTooLong, DiagnosticSeverity.Warning),
            ]
        );
    }
    
    [Fact]
    public void LexToken_WhenValueExceedsSoftLimit_ReturnsValidWithWarning()
    {
        var source = "{User::[Lyrica]}";

        var token = Run(source, options: new(MaxValueLength: 4));

        LexerAssert.TemplateToken(
            token,
            source,
            expectedIsValid: true,
            expectedIdentifier: "User",
            expectedValueKind: TemplateValueKind.Value,
            expectedValue: "Lyrica",
            expectedDiagnostics: [
                new(DiagnosticId.TemplateValueTooLong, DiagnosticSeverity.Warning),
            ]
        );
    }

    [Fact]
    public void LexToken_WhenValueExceedsHardLimit_ReturnsInvalidWithError()
    {
        var source = "{User::[Lyrica]}";

        var token = Run(source, options: new(MaxValueLength: 4, Tolerance: TemplateTolerance.None));

        LexerAssert.TemplateToken(
            token,
            source,
            expectedIsValid: false,
            expectedIdentifier: "User",
            expectedValueKind: TemplateValueKind.Value,
            expectedValue: "Lyrica",
            expectedDiagnostics: [
                new(DiagnosticId.TemplateValueTooLong, DiagnosticSeverity.Error),
            ]
        );
    }

    #endregion

    #region Recovery

    [Fact]
    public void LexToken_WithUnexpectedContentAfterIdentifier_ReturnsValidWithWarnings()
    {
        var source = "{User unexpectedContent garbage2}";

        var token = Run(source);

        LexerAssert.TemplateToken(
            token,
            source,
            expectedIsValid: true,
            expectedIdentifier: "User",
            expectedDiagnostics: [
                new(DiagnosticId.TemplateIdentifierUnexpectedContent, DiagnosticSeverity.Warning),
                new(DiagnosticId.TemplateIdentifierUnexpectedContent, DiagnosticSeverity.Warning),
            ]
        );
    }

    [Fact]
    public void LexToken_WithUnexpectedContentAfterValue_ReturnsValidWithWarnings()
    {
        var source = "{User::[] unexpectedContent garbage2}";

        var token = Run(source);

        LexerAssert.TemplateToken(
            token,
            source,
            expectedIsValid: true,
            expectedIdentifier: "User",
            expectedValueKind: TemplateValueKind.Value,
            expectedValue: "",
            expectedDiagnostics: [
                new(DiagnosticId.TemplateValueUnexpectedContent, DiagnosticSeverity.Warning),
                new(DiagnosticId.TemplateValueUnexpectedContent, DiagnosticSeverity.Warning),
            ]
        );
    }

    [Fact]
    public void LexToken_WhenInvalidValueKind_ReturnsInvalidWithError()
    {
        var source = "{User::Z[Lyrica]}";

        var token = Run(source);

        LexerAssert.TemplateToken(
            token,
            source,
            expectedIsValid: false,
            expectedIdentifier: "User",
            expectedValueKind: TemplateValueKind.Invalid,
            expectedValue: "Lyrica",
            expectedDiagnostics: [
                new(DiagnosticId.TemplateValueKindInvalid, DiagnosticSeverity.Error),
            ]
        );
    }

    [Fact]
    public void LexToken_WhenValueUnclosed_ReturnsValidWithWarning()
    {
        var source = "{User::[Lyrica}";

        var token = Run(source);

        LexerAssert.TemplateToken(
            token,
            source,
            expectedIsValid: true,
            expectedIdentifier: "User",
            expectedValueKind: TemplateValueKind.Value,
            expectedValue: "Lyrica",
            expectedDiagnostics: [
                new(DiagnosticId.TemplateValueUnclosed, DiagnosticSeverity.Warning),
            ]
        );
    }

    #endregion

    #region Interrupted Tokens

    [Fact]
    public void LexToken_WhenInterruptedWithUnexpectedContentAfterIdentifier_ReturnsValidWithWarnings()
    {
        var token = Run("{User  unexpectedContent garbage2  {NewToken}");

        LexerAssert.TemplateToken(
            token,
            expectedSource: "{User  unexpectedContent garbage2  ",
            expectedIsValid: true,
            expectedIdentifier: "User",
            expectedDiagnostics: [
                new(DiagnosticId.TemplateIdentifierUnexpectedContent, DiagnosticSeverity.Warning),
                new(DiagnosticId.TemplateIdentifierUnexpectedContent, DiagnosticSeverity.Warning),
                new(DiagnosticId.TemplateTokenUnclosed, DiagnosticSeverity.Warning),
            ]
        );
    }
    
    [Fact]
    public void LexToken_WhenInterruptedAfterSeparator_ReturnsInvalidWithErrors()
    {
        var token = Run("{User  ::  {NewToken}");

        LexerAssert.TemplateToken(
            token,
            expectedSource: "{User  ::  ",
            expectedIsValid: false,
            expectedIdentifier: "User",
            expectedDiagnostics: [
                new(DiagnosticId.TemplateValueMissing, DiagnosticSeverity.Error),
                new(DiagnosticId.TemplateTokenUnclosed, DiagnosticSeverity.Error),
            ]
        );
    }
    
    [Fact]
    public void LexToken_WhenInterruptedAfterUnclosedValue_ReturnsValidWithWarnings()
    {
        var token = Run("{User::[Lyrica{NewToken}");

        LexerAssert.TemplateToken(
            token,
            expectedSource: "{User::[Lyrica",
            expectedIsValid: true,
            expectedIdentifier: "User",
            expectedValueKind: TemplateValueKind.Value,
            expectedValue: "Lyrica",
            expectedDiagnostics: [
                new(DiagnosticId.TemplateValueUnclosed, DiagnosticSeverity.Warning),
                new(DiagnosticId.TemplateTokenUnclosed, DiagnosticSeverity.Warning),
            ]
        );
    }

    #endregion

    #region EOF
    #endregion

    #region Escaping
    #endregion

    #region Token Limits
    #endregion

    private static TemplateToken Run(
        string input,
        LexerContext context = default,
        TemplateLexerOptions? options = null
    )
    {
        var lexer = new TemplateLexer(options);
        var source = input.AsMemory();

        var result = lexer.LexToken(source, sliced: source.Span, tokenStart: 0, context);

        Assert.NotNull(result);
        return Assert.IsType<TemplateToken>(result?.token);
    }
}
