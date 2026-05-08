using LexiCore.Diagnostics;
using LexiCore.Diagnostics.Factories;
using LexiCore.Facts;
using LexiCore.Syntax;
using LexiCore.Tokens.Syntax;

namespace LexiCore.Lexing.Lexers;

// IMPORTANT: This lexer is highly optimized and relies on subtle invariants.
// Changes should be made with caution and full understanding of the state machine.
//
// If you decide to change something, be guided by this:
// - Parses (examples): {Identifier}, {Identifier::[Value]}, {Identifier::ValueKind[Value]}
// - Continues parsing even on malformed input
// - Once the token becomes invalid (`isValid == false`), it is not restored
// - Parsing is strictly single-pass FSM (backtracking is intentionally not used)
// - Diagnostic severity depends on the structural integrity and recoverability

/// <summary>
/// Lexer for template tokens with built-in validation and diagnostic reporting.
/// </summary>
/// <remarks>
/// Parses template expressions defined by <see cref="TemplateSyntax"/>
/// and produces a <see cref="TemplateToken"/> with associated diagnostics.
///
/// <para>
/// The lexer is fault-tolerant and attempts to recover from malformed input,
/// reporting issues instead of failing immediately.
/// </para>
///
/// <para>
/// Behavior can be adjusted via <see cref="TemplateLexerOptions"/>,
/// including length limits and tolerance rules.
/// </para>
/// </remarks>
public sealed class TemplateLexer(TemplateLexerOptions? options = null) : DelimitedTokenLexerBase<TemplateToken>
{
    private readonly TemplateLexerOptions _options = options ?? TemplateLexerOptions.Default;
    
    /// <inheritdoc/>
    protected override SyntaxTerminal TokenOpen => TemplateSyntax.TokenOpen;
    /// <inheritdoc/>
    protected override SyntaxTerminal TokenClose => TemplateSyntax.TokenClose;

    private enum TemplateState : byte
    {
        Start,
        Identifier,
        AfterIdentifier,
        Separator,
        AfterSeparator,
        AfterValueKind,
        ValueOpen,
        Value,
        ValueClose,
        AfterValue,
        End
    }

    /// <inheritdoc/>
    protected override (TemplateToken token, int nextPosition) Lex(
        ReadOnlyMemory<char> source,
        ReadOnlySpan<char> sliced,
        int tokenStart,
        LexerContext context
    )
    {
        // Token Data
        int identifierStart = -1;
        int identifierLength = 0;
        TemplateValueKind kind = TemplateValueKind.None;
        int valueStart = -1;
        int valueLength = 0;
        bool isValid = true;
        List<Diagnostic>? diagnostics = null;

        // Context
        int position = TokenOpen.Length;
        bool isEscaped = false;
        bool enableDiagnostics = context.EnableDiagnostics;

        // Lexer State
        TemplateState state = TemplateState.Start;
        bool isParsing = true;

        // Grammar expectations (used for diagnostics)
        int expectedValueStart = -1;

        // Utils
        int invalidCharsCount = 0; // counter

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        void Report(Diagnostic d)
        {
            diagnostics ??= new List<Diagnostic>(4);
            diagnostics.Add(d);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        void FlushInvalidChars(Func<int, int, int, Diagnostic> factory)
        {
            if (invalidCharsCount > 0)
            {
                if (enableDiagnostics)
                {
                    Report(factory(
                        tokenStart,
                        position - invalidCharsCount, // relativePosition
                        invalidCharsCount
                    ));
                }
                invalidCharsCount = 0;
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        void FlushOverflowChars(
            Func<int, int, int, bool, int, Diagnostic> tooLongFactory,
            int currentLength,
            bool isCritical,
            int maxLength
        )
        {
            if (enableDiagnostics && currentLength > maxLength)
            {
                int overflow = currentLength - maxLength;
                Report(tooLongFactory(
                    tokenStart,
                    position - overflow, // relativePosition
                    overflow,
                    isCritical,
                    maxLength
                ));
            }
        }

        // This should only be called after `identifierStart` has been initialized
        // (when the current state is TemplateState.Identifier).
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        void CommitIdentifier()
        {
            identifierLength = position - identifierStart;

            FlushInvalidChars(TemplateDiagnosticFactory.Identifier.InvalidChars);
            FlushOverflowChars(
                TemplateDiagnosticFactory.Identifier.TooLong,
                identifierLength,
                isCritical: !_options.Allows(TemplateTolerance.AllowLongIdentifiers),
                _options.MaxIdentifierLength
            );
        }

        // This should only be called after `valueStart` has been initialized
        // (when the current state is TemplateState.Value).
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        void CommitValue()
        {
            valueLength = position - valueStart;

            FlushOverflowChars(
                TemplateDiagnosticFactory.Value.TooLong,
                valueLength,
                isCritical: !_options.Allows(TemplateTolerance.AllowLongValues),
                _options.MaxValueLength
            );
        }

        // IMPORTANT: If the state machine was paused in the TemplateState.Identifier or TemplateState.Value states,
        // you must ensure that `identifierLength` and `valueLength` are up to date before using this function.
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        bool IsRecoverable()
        {
            // Boundary
            if (state < TemplateState.Identifier)
            {
                return false;
            }
            if (state >= TemplateState.Separator && state < TemplateState.Value)
            {
                return false;
            }

            // Restrictions
            if (!_options.Allows(TemplateTolerance.AllowLongTokens)
                && position > _options.MaxTokenLength)
            {
                return false;
            }
            if (!_options.Allows(TemplateTolerance.AllowLongIdentifiers)
                && identifierLength > _options.MaxIdentifierLength)
            {
                return false;
            }
            if (state >= TemplateState.Value
                && !_options.Allows(TemplateTolerance.AllowLongValues)
                && valueLength > _options.MaxValueLength)
            {
                return false;
            }
            return true;
        }

        // =============
        // State Machine
        // =============
        int maxTokenLength = Math.Min(sliced.Length, _options.MaxTokenLength);
        while (isParsing && position < maxTokenLength)
        {
            char c = sliced[position];
            isEscaped = false;

            // Escape Sequence Handling:
            // '\' disables special meaning of the next character within the current token.
            if (c == SharedSyntax.EscapeChar)
            {
                position++;

                if (position >= sliced.Length) // EOF
                {
                    isValid = false;

                    if (enableDiagnostics)
                    {
                        Report(TemplateDiagnosticFactory.Token.TrailingEscape(tokenStart, position - 1));
                    }
                    break;
                }

                isEscaped = true;
                c = sliced[position];
            }

            // State Logic
            switch (state)
            {
                // Identifier
                case TemplateState.Start:
                    if (TemplateFacts.IsValidIdentifierChar(c, isFirstChar: true))
                    {
                        identifierStart = position;
                        identifierLength = 1;

                        state = TemplateState.Identifier;
                    }
                    else if (char.IsWhiteSpace(c)) { }
                    else if (!isEscaped && IsTokenEnd(sliced, position))
                    {
                        isValid = false;

                        if (enableDiagnostics)
                        {
                            Report(TemplateDiagnosticFactory.Token.Empty(
                                tokenStart,
                                relativePosition: TokenOpen.Length,
                                errorLength: position - TokenOpen.Length
                            ));
                        }

                        state = TemplateState.End;
                        isParsing = false;
                    }
                    else if (!isEscaped && c == TemplateSyntax.ContentSeparator.FirstChar)
                    {
                        isValid = false;

                        if (enableDiagnostics)
                        {
                            Report(TemplateDiagnosticFactory.Identifier.Missing(
                                tokenStart,
                                relativePosition: TokenOpen.Length,
                                errorLength: position - TokenOpen.Length
                            ));
                        }

                        state = TemplateState.Separator;
                        goto case TemplateState.Separator;
                    }
                    else if (!isEscaped && c == TemplateSyntax.ValueOpen.FirstChar)
                    {
                        isValid = false;

                        if (enableDiagnostics)
                        {
                            Report(TemplateDiagnosticFactory.Identifier.Missing(
                                tokenStart,
                                relativePosition: TokenOpen.Length,
                                errorLength: position - TokenOpen.Length
                            ));
                            Report(TemplateDiagnosticFactory.Separator.Missing(tokenStart, position));
                        }

                        state = TemplateState.ValueOpen;
                        goto case TemplateState.ValueOpen;
                    }
                    else if (!isEscaped && IsTokenStart(sliced, position))
                    {
                        isValid = false;

                        if (enableDiagnostics)
                        {
                            Report(TemplateDiagnosticFactory.Token.Empty(
                                tokenStart,
                                relativePosition: TokenOpen.Length,
                                errorLength: position - TokenOpen.Length
                            ));
                        }

                        isParsing = false;
                    }
                    else
                    {
                        identifierStart = position;
                        identifierLength = 1;
                        isValid = false;

                        if (enableDiagnostics)
                        {
                            Report(TemplateDiagnosticFactory.Identifier.InvalidStart(tokenStart, position, c));
                        }

                        state = TemplateState.Identifier;
                    }
                    break;
                case TemplateState.Identifier:
                    if (TemplateFacts.IsValidIdentifierChar(c))
                    {
                        FlushInvalidChars(TemplateDiagnosticFactory.Identifier.InvalidChars);
                    }
                    else if (!isEscaped && IsTokenEnd(sliced, position))
                    {
                        CommitIdentifier();

                        state = TemplateState.End;
                        isParsing = false;
                    }
                    else if (!isEscaped && c == TemplateSyntax.ContentSeparator.FirstChar)
                    {
                        CommitIdentifier();

                        state = TemplateState.Separator;
                        goto case TemplateState.Separator;
                    }
                    else if (char.IsWhiteSpace(c))
                    {
                        CommitIdentifier();

                        state = TemplateState.AfterIdentifier;
                    }
                    else if (!isEscaped && c == TemplateSyntax.ValueOpen.FirstChar)
                    {
                        isValid = false;
                        CommitIdentifier();

                        if (enableDiagnostics)
                        {
                            Report(TemplateDiagnosticFactory.Separator.Missing(tokenStart, position));
                        }

                        state = TemplateState.ValueOpen;
                        goto case TemplateState.ValueOpen;
                    }
                    else if (!isEscaped && IsTokenStart(sliced, position))
                    {
                        isValid = false;
                        CommitIdentifier();

                        isParsing = false;
                    }
                    else
                    {
                        isValid = false;

                        invalidCharsCount++;
                    }
                    break;
                case TemplateState.AfterIdentifier:
                    if (!isEscaped && IsTokenEnd(sliced, position))
                    {
                        FlushInvalidChars(TemplateDiagnosticFactory.Identifier.UnexpectedContent);

                        state = TemplateState.End;
                        isParsing = false;
                    }
                    else if (!isEscaped && c == TemplateSyntax.ContentSeparator.FirstChar)
                    {
                        FlushInvalidChars(TemplateDiagnosticFactory.Identifier.UnexpectedContent);

                        state = TemplateState.Separator;
                        goto case TemplateState.Separator;
                    }
                    else if (char.IsWhiteSpace(c))
                    {
                        FlushInvalidChars(TemplateDiagnosticFactory.Identifier.UnexpectedContent);
                    }
                    else if (!isEscaped && c == TemplateSyntax.ValueOpen.FirstChar)
                    {
                        isValid = false;
                        FlushInvalidChars(TemplateDiagnosticFactory.Identifier.UnexpectedContent);

                        if (enableDiagnostics)
                        {
                            Report(TemplateDiagnosticFactory.Separator.Missing(tokenStart, position));
                        }

                        state = TemplateState.ValueOpen;
                        goto case TemplateState.ValueOpen;
                    }
                    else if (!isEscaped && IsTokenStart(sliced, position))
                    {
                        isValid = false;
                        FlushInvalidChars(TemplateDiagnosticFactory.Identifier.UnexpectedContent);

                        isParsing = false;
                    }
                    else
                    {
                        invalidCharsCount++;
                    }
                    break;
                
                // Separator
                case TemplateState.Separator: // Symbol
                {
                    int increment = TemplateSyntax.ContentSeparator.Length;

                    if (!SyntaxFacts.IsTokenSymbol(sliced, position, TemplateSyntax.ContentSeparator))
                    {
                        increment = sliced[position..].CommonPrefixLength(TemplateSyntax.ContentSeparator);

                        if (enableDiagnostics
                            && position + increment < sliced.Length && char.IsWhiteSpace(sliced[position + increment]))
                        {   // Example (separator = "::"): "{identifier:   " — WhiteSpace
                            Report(TemplateDiagnosticFactory.Separator.Incomplete(tokenStart, position, increment));
                        }
                        else if (enableDiagnostics && position + increment == sliced.Length)
                        {   // Example (separator = "::"): "{identifier:" — EOF
                            Report(TemplateDiagnosticFactory.Separator.Incomplete(tokenStart, position, increment));
                        }
                        else
                        {   // Example (separator = "::"): "{identifier:[ " — Invalid Char
                            isValid = false;

                            if (enableDiagnostics)
                            {
                                Report(TemplateDiagnosticFactory.Separator.Invalid(tokenStart, position, increment));
                            }
                        }
                    }
                    position += increment;

                    state = TemplateState.AfterSeparator;
                    expectedValueStart = position;
                    continue;
                }
                
                // Value Kind
                case TemplateState.AfterSeparator:
                    if (!isEscaped)
                    {
                        kind = TemplateFacts.GetValueKind(c);
                    }

                    if (kind != TemplateValueKind.None)
                    {
                        state = TemplateState.AfterValueKind;
                        expectedValueStart = position;
                    }
                    else if (!isEscaped && c == TemplateSyntax.ValueOpen.FirstChar)
                    {
                        kind = TemplateSyntax.DefaultValueKind;

                        state = TemplateState.ValueOpen;
                        goto case TemplateState.ValueOpen;
                    }
                    else if (char.IsWhiteSpace(c)) { }
                    else if (!isEscaped && IsTokenEnd(sliced, position))
                    {
                        isValid = false;

                        if (enableDiagnostics)
                        {
                            Report(TemplateDiagnosticFactory.Value.Missing(
                                tokenStart,
                                expectedValueStart,
                                errorLength: position - expectedValueStart
                            ));
                        }

                        state = TemplateState.End;
                        isParsing = false;
                    }
                    else if (!isEscaped && IsTokenStart(sliced, position))
                    {
                        isValid = false;

                        if (enableDiagnostics)
                        {
                            Report(TemplateDiagnosticFactory.Value.Missing(
                                tokenStart,
                                expectedValueStart,
                                errorLength: position - expectedValueStart
                            ));
                        }

                        isParsing = false;
                    }
                    else
                    {
                        isValid = false;

                        if (enableDiagnostics)
                        {
                            Report(TemplateDiagnosticFactory.ValueKind.Invalid(tokenStart, position));
                        }

                        state = TemplateState.AfterValueKind;
                        expectedValueStart = position;
                    }
                    break;
                case TemplateState.AfterValueKind:
                    if (!isEscaped && c == TemplateSyntax.ValueOpen.FirstChar)
                    {
                        FlushInvalidChars(TemplateDiagnosticFactory.ValueKind.UnexpectedContent);

                        state = TemplateState.ValueOpen;
                        goto case TemplateState.ValueOpen;
                    }
                    else if (char.IsWhiteSpace(c))
                    {
                        FlushInvalidChars(TemplateDiagnosticFactory.ValueKind.UnexpectedContent);
                    }
                    else if (!isEscaped && IsTokenEnd(sliced, position))
                    {
                        isValid = false;
                        FlushInvalidChars(TemplateDiagnosticFactory.ValueKind.UnexpectedContent);

                        if (enableDiagnostics)
                        {
                            Report(TemplateDiagnosticFactory.Value.Missing(
                                tokenStart,
                                expectedValueStart,
                                errorLength: position - expectedValueStart
                            ));
                        }

                        state = TemplateState.End;
                        isParsing = false;
                    }
                    else if (!isEscaped && IsTokenStart(sliced, position))
                    {
                        isValid = false;
                        FlushInvalidChars(TemplateDiagnosticFactory.ValueKind.UnexpectedContent);

                        if (enableDiagnostics)
                        {
                            Report(TemplateDiagnosticFactory.Value.Missing(
                                tokenStart,
                                expectedValueStart,
                                errorLength: position - expectedValueStart
                            ));
                        }

                        isParsing = false;
                    }
                    else
                    {
                        invalidCharsCount++;
                    }
                    break;
                
                // Value
                case TemplateState.ValueOpen: // Symbol
                {    
                    int increment = TemplateSyntax.ValueOpen.Length;

                    if (!SyntaxFacts.IsTokenSymbol(sliced, position, TemplateSyntax.ValueOpen))
                    {
                        increment = sliced[position..].CommonPrefixLength(TemplateSyntax.ValueOpen);

                        if (enableDiagnostics 
                            && position + increment < sliced.Length && char.IsWhiteSpace(sliced[position + increment]))
                        {   // Example (VO = "[["): "{identifier::[  " — WhiteSpace
                            Report(TemplateDiagnosticFactory.Value.IncompleteOpen(tokenStart, position, increment));
                        }
                        else if (enableDiagnostics && position + increment == sliced.Length)
                        {   // Example (VO = "[["): "{identifier::[" — EOF
                            Report(TemplateDiagnosticFactory.Value.IncompleteOpen(tokenStart, position, increment));
                        }
                        else
                        {   // Example (VO = "[["): "{identifier::[< " — Invalid Char
                            isValid = false;

                            if (enableDiagnostics)
                            {
                                Report(TemplateDiagnosticFactory.Value.InvalidOpen(tokenStart, position, increment));
                            } 
                        }
                    }
                    position += increment;
                    valueStart = position;

                    state = TemplateState.Value;
                    continue;
                }
                case TemplateState.Value:
                    if (!isEscaped && c == TemplateSyntax.ValueClose.FirstChar)
                    {
                        CommitValue();

                        state = TemplateState.ValueClose;
                        goto case TemplateState.ValueClose;
                    }
                    else if (!isEscaped && IsTokenEnd(sliced, position))
                    {
                        CommitValue();

                        if (enableDiagnostics)
                        {
                            Report(TemplateDiagnosticFactory.Value.Unclosed(tokenStart, position, isCritical: false));
                        }

                        state = TemplateState.End;
                        isParsing = false;
                    }
                    else if (!isEscaped && IsTokenStart(sliced, position))
                    {
                        CommitValue();

                        if (enableDiagnostics)
                        {
                            Report(TemplateDiagnosticFactory.Value.Unclosed(tokenStart, position, isCritical: false));
                        }

                        isParsing = false;
                    }
                    break;
                case TemplateState.ValueClose: // Symbol
                {
                    int increment = TemplateSyntax.ValueClose.Length;

                    if (!SyntaxFacts.IsTokenSymbol(sliced, position, TemplateSyntax.ValueClose))
                    {
                        increment = sliced[position..].CommonPrefixLength(TemplateSyntax.ValueClose);

                        if (enableDiagnostics
                            && position + increment < sliced.Length && char.IsWhiteSpace(sliced[position + increment]))
                        {   // Example (VC = "]]"): "{identifier::[[value]  " — WhiteSpace
                            Report(TemplateDiagnosticFactory.Value.IncompleteClose(tokenStart, position, increment));
                        }
                        else if (enableDiagnostics && position + increment == sliced.Length)
                        {   // Example (VC = "]]"): "{identifier::[[value]" — EOF
                            Report(TemplateDiagnosticFactory.Value.IncompleteClose(tokenStart, position, increment));
                        }
                        else
                        {   // Example (VC = "]]"): "{identifier::[[value]> " — Invalid Char
                            isValid = false;

                            if (enableDiagnostics)
                            {
                                Report(TemplateDiagnosticFactory.Value.InvalidClose(tokenStart, position, increment));
                            }
                        }
                    }
                    position += increment;

                    state = TemplateState.AfterValue;
                    continue;
                }
                case TemplateState.AfterValue:
                    if (!isEscaped && IsTokenEnd(sliced, position))
                    {
                        FlushInvalidChars(TemplateDiagnosticFactory.Value.UnexpectedContent);

                        state = TemplateState.End;
                        isParsing = false;
                    }
                    else if (char.IsWhiteSpace(c))
                    {
                        FlushInvalidChars(TemplateDiagnosticFactory.Value.UnexpectedContent);
                    }
                    else if (!isEscaped && IsTokenStart(sliced, position))
                    {
                        FlushInvalidChars(TemplateDiagnosticFactory.Value.UnexpectedContent);

                        isParsing = false;
                    }
                    else
                    {
                        invalidCharsCount++;
                    }
                    break;
            }

            // Increment position unless the current branch already advanced or terminated execution:
            // - via `continue` in symbol-handling branches,
            // - or via state transitions that terminate parsing.
            if (isParsing)
            {
                position++;
            }
        }

        // ============
        // Commit Token
        // ============
        if (state == TemplateState.End)
        {
            position += TokenClose.Length;
            isValid = isValid && IsRecoverable();

            // TokenClose is added after a successful parse completion.
            // This can still cause MaxTokenLength overflow even for valid tokens,
            // so overflow diagnostics must be evaluated in this branch as well.
            FlushOverflowChars(
                TemplateDiagnosticFactory.Token.TooLong,
                position,
                isCritical: !isValid,
                _options.MaxTokenLength
            );
        }
        else if (!isEscaped && IsTokenStart(sliced, position))
        {   // interrupt: next token starts, current token is cut
            if (enableDiagnostics)
            {
                Report(TemplateDiagnosticFactory.Token.Unclosed(tokenStart, position, isCritical: !isValid));
            }
        }
        else
        {
            // The severity of the diagnostics is dynamic:
            // - If the token remains structurally recoverable, diagnostics are reported as warnings
            // - If the token becomes structurally invalid, diagnostics are reported as errors
            //
            // IMPORTANT:
            // This is done to ensure a cascading severity level of structural diagnostic messages,
            // starting from the first point where the token's structural integrity was violated.
            //
            // Backtracking is deliberately avoided to ensure deterministic behavior.

            if (position >= sliced.Length) // EOF
            {
                // State Finalization
                switch (state)
                {
                    // Identifier
                    case TemplateState.Start:
                        if (enableDiagnostics)
                        {
                            Report(TemplateDiagnosticFactory.Token.Empty(
                                tokenStart,
                                relativePosition: TokenOpen.Length,
                                errorLength: position - TokenOpen.Length
                            ));
                        }
                        break;
                    case TemplateState.Identifier:
                        CommitIdentifier();
                        break;
                    case TemplateState.AfterIdentifier:
                        FlushInvalidChars(TemplateDiagnosticFactory.Identifier.UnexpectedContent);
                        break;

                    // Value Kind
                    case TemplateState.AfterSeparator:
                        if (enableDiagnostics)
                        {
                            Report(TemplateDiagnosticFactory.Value.Missing(
                                tokenStart,
                                expectedValueStart,
                                errorLength: position - expectedValueStart
                            ));
                        }
                        break;
                    case TemplateState.AfterValueKind:
                        FlushInvalidChars(TemplateDiagnosticFactory.ValueKind.UnexpectedContent);

                        if (enableDiagnostics)
                        {
                            Report(TemplateDiagnosticFactory.Value.Missing(
                                tokenStart,
                                expectedValueStart,
                                errorLength: position - expectedValueStart
                            ));
                        }
                        break;

                    // Value
                    case TemplateState.Value:
                        CommitValue();
                        isValid = isValid && IsRecoverable();

                        if (enableDiagnostics)
                        {
                            Report(TemplateDiagnosticFactory.Value.Unclosed(tokenStart, position, isCritical: !isValid));
                        }
                        break;
                    case TemplateState.AfterValue:
                        FlushInvalidChars(TemplateDiagnosticFactory.Value.UnexpectedContent);
                        break;
                }

                isValid = isValid && IsRecoverable();

                if (enableDiagnostics)
                {
                    Report(TemplateDiagnosticFactory.Token.UnexpectedEOF(tokenStart, sliced.Length, isCritical: !isValid));
                }
            }
            else
            {
                isValid = false;

                if (enableDiagnostics)
                {
                    Report(TemplateDiagnosticFactory.Token.Unclosed(
                        tokenStart,
                        relativePosition: Math.Min(position, sliced.Length),
                        isCritical: !isValid
                    ));
                }
            }

            FlushOverflowChars(
                TemplateDiagnosticFactory.Token.TooLong,
                position,
                isCritical: !isValid,
                _options.MaxTokenLength
            );
        }

        return (
            new(
                source.Slice(tokenStart, position),
                identifierStart,
                identifierLength,
                kind,
                valueStart,
                valueLength,
                isValid,
                diagnostics?.ToArray() ?? []
            ),
            tokenStart + position
        );
    }
}
