using System.Text;

using LexiCore.Diagnostics;
using LexiCore.Lexing;
using LexiCore.Lexing.Lexers;
using LexiCore.Primitives;
using LexiCore.Tokens.Syntax;

using LexiCore.Sandbox.Utils;

// I understand that this is difficult to read and that there are better
// and more understandable ways to implement it,
// but for now, this will serve as a rigorous testing ground.
// I needed quick results.

// Data
Dictionary<TestCase, string> texts = new()
{
    [TestCase.ComplexTemplate] = "Meow, {User}! { : f    } Any {text::  g[Woof! weg9g032'] {for you ааааа ааа: v meow[Фыр} my [<sweet friend. { \\\\{{\\",
    [TestCase.FormatAlignment] = "Any { 0,  -4 }",
    [TestCase.FormatStandard] = "Any {0,10:C}",
    [TestCase.MoreTokens] = "{Correct::[Token]}  {[Test]} {  meow   :: f   [] afterValue}",

    [TestCase.Manual] = "{User  ::  {NewToken}",
};

// Configuration
var text = texts[TestCase.Manual];
const int ReadableAreaStart = 0, ReadableAreaLength = 999;

var lexer = new Lexer(
    lexers: [
        new TemplateLexer(TemplateLexerOptions.Default with { } )
    ]
);
var context = new LexerContext(TokenBufferConfig.Default with { }, enableDiagnostics: true);

// Layout
var readableArea = new SourceSegment(
    ReadableAreaStart,
    length: Math.Min(ReadableAreaLength, text.Length - ReadableAreaStart)
);

var source = text.AsMemory();
var readable = readableArea.Slice(source);

// Step 1: Lexing
var tokens = lexer.Tokenize(source, readable, readableArea.Start, context);

// Render
Console.Write(ConsoleTemplates.TextInfo(text, readableArea));

int position = readableArea.Start;
var sb = new StringBuilder();

foreach (var token in tokens)
{
    sb.Append(ConsoleTemplates.TokenInfo(token, position));
    sb.Append(ConsoleTemplates.TokenMetadata(token));

    if (token is SyntaxTokenBase syntaxToken && syntaxToken.HasDiagnostics)
    {
        sb.Append(ConsoleTemplates.DiagnosticsInfo(
            readable.ToString(),
            readableArea,
            tokenArea: new(start: position - readableArea.Start, syntaxToken.Length),
            diagnostics: syntaxToken.Diagnostics.Span,
            messageFormatter: GetDiagnosticFormattedMessage
        ));
    }

    position += token.Length;

    Console.WriteLine(sb);
    sb.Clear();
}

// Helpers
string GetDiagnosticFormattedMessage(Diagnostic d)
{
    if (d.Code == DiagnosticCode.TemplateTokenUnexpectedEOF)
    { // The message with this code already contains words indicating the end of the file.
        return $"[{d.FormattedCode}] {d.PlainMessage}";
    }

    string location;
    if (d.AbsoluteEndPosition == source.Length)
    {
        location = "at end of file";
    }
    else if (d.Length == 1 || d.Code == DiagnosticCode.TemplateTokenEmpty)
    {
        location = $"at position {d.AbsolutePosition}";
    }
    else if (d.Length == 0)
    {
        location = $"at insertion point {d.AbsolutePosition}";
    }
    else
    {
        location = $"in positions [{d.AbsolutePosition}..{d.AbsoluteEndPosition}]";
    }

    return $"[{d.FormattedCode}] {d.PlainMessage} {location}";
}

enum TestCase
{
    ComplexTemplate,
    FormatAlignment,
    FormatStandard,
    MoreTokens,

    Manual,
}
