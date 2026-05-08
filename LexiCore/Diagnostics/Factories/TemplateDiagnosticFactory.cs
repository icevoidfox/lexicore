namespace LexiCore.Diagnostics.Factories;

internal static class TemplateDiagnosticFactory
{
    public static class Token
    {
        public static Diagnostic Empty(
            int tokenStart,
            int relativePosition,
            int errorLength
        ) => new(
            Id: DiagnosticId.TemplateTokenEmpty,
            SourceOffset: tokenStart,
            RelativePosition: relativePosition,
            Length: errorLength
        );

        public static Diagnostic TooLong(
            int tokenStart,
            int relativePosition,
            int overflowTokenLength,
            bool isCritical,
            int maxTokenLength
        ) => new(
            Id: DiagnosticId.TemplateTokenTooLong,
            Args: new TooLongArgs(overflowTokenLength, maxTokenLength),
            SourceOffset: tokenStart,
            RelativePosition: relativePosition,
            Length: overflowTokenLength,
            Severity: GetSeverity(isCritical)
        );
        
        public static Diagnostic Unclosed(
            int tokenStart,
            int relativePosition,
            bool isCritical
        ) => new(
            Id: DiagnosticId.TemplateTokenUnclosed,
            SourceOffset: tokenStart,
            RelativePosition: relativePosition,
            Length: 0,
            Severity: GetSeverity(isCritical)
        );

        public static Diagnostic TrailingEscape(
            int tokenStart,
            int relativePosition
        ) => new(
            Id: DiagnosticId.TemplateTokenTrailingEscape,
            SourceOffset: tokenStart,
            RelativePosition: relativePosition
        );

        public static Diagnostic UnexpectedEOF(
            int tokenStart,
            int relativePosition,
            bool isCritical
        ) => new(
            Id: DiagnosticId.TemplateTokenUnexpectedEOF,
            SourceOffset: tokenStart,
            RelativePosition: relativePosition,
            Length: 0,
            Severity: GetSeverity(isCritical)
        );
    }

    public static class Identifier
    {
        public static Diagnostic Missing(
            int tokenStart,
            int relativePosition,
            int errorLength
        ) => new(
            Id: DiagnosticId.TemplateIdentifierMissing,
            SourceOffset: tokenStart,
            RelativePosition: relativePosition,
            Length: errorLength
        );

        public static Diagnostic InvalidStart(
            int tokenStart,
            int relativePosition,
            char foundChar
        ) => new(
            Id: DiagnosticId.TemplateIdentifierInvalidStart,
            Args: new CharArgs(foundChar),
            SourceOffset: tokenStart,
            RelativePosition: relativePosition
        );

        public static Diagnostic InvalidChars(
            int tokenStart,
            int relativePosition,
            int invalidCharsCount
        ) => new(
            Id: DiagnosticId.TemplateIdentifierInvalidChars,
            SourceOffset: tokenStart,
            RelativePosition: relativePosition,
            Length: invalidCharsCount
        );
        
        public static Diagnostic TooLong(
            int tokenStart,
            int relativePosition,
            int overflowIdentifierLength,
            bool isCritical,
            int maxIdentifierLength
        ) => new(
            Id: DiagnosticId.TemplateIdentifierTooLong,
            Args: new TooLongArgs(overflowIdentifierLength, maxIdentifierLength),
            SourceOffset: tokenStart,
            RelativePosition: relativePosition,
            Length: overflowIdentifierLength,
            Severity: GetSeverity(isCritical)
        );

        public static Diagnostic UnexpectedContent(
            int tokenStart,
            int relativePosition,
            int invalidCharsCount
        ) => new(
            Id: DiagnosticId.TemplateIdentifierUnexpectedContent,
            SourceOffset: tokenStart,
            RelativePosition: relativePosition,
            Length: invalidCharsCount,
            Severity: DiagnosticSeverity.Warning
        );
    }

    public static class Separator
    {
        public static Diagnostic Missing(
            int tokenStart,
            int relativePosition
        ) => new(
            Id: DiagnosticId.TemplateSeparatorMissing,
            SourceOffset: tokenStart,
            RelativePosition: relativePosition,
            Length: 0
        );
        
        public static Diagnostic Invalid(
            int tokenStart,
            int relativePosition,
            int errorLength
        ) => new(
            Id: DiagnosticId.TemplateSeparatorInvalid,
            SourceOffset: tokenStart,
            RelativePosition: relativePosition,
            Length: errorLength
        );
        
        public static Diagnostic Incomplete(
            int tokenStart,
            int relativePosition,
            int errorLength
        ) => new(
            Id: DiagnosticId.TemplateSeparatorIncomplete,
            SourceOffset: tokenStart,
            RelativePosition: relativePosition,
            Length: errorLength,
            Severity: DiagnosticSeverity.Warning
        );
    }

    public static class ValueKind
    {
        public static Diagnostic Invalid(
            int tokenStart,
            int relativePosition
        ) => new(
            Id: DiagnosticId.TemplateValueKindInvalid,
            SourceOffset: tokenStart,
            RelativePosition: relativePosition
        );

        public static Diagnostic UnexpectedContent(
            int tokenStart,
            int relativePosition,
            int invalidCharsCount
        ) => new(
            Id: DiagnosticId.TemplateValueKindUnexpectedContent,
            SourceOffset: tokenStart,
            RelativePosition: relativePosition,
            Length: invalidCharsCount,
            Severity: DiagnosticSeverity.Warning
        );
    }

    public static class Value
    {
        public static Diagnostic Missing(
            int tokenStart,
            int relativePosition,
            int errorLength
        ) => new(
            Id: DiagnosticId.TemplateValueMissing,
            SourceOffset: tokenStart,
            RelativePosition: relativePosition,
            Length: errorLength
        );

        public static Diagnostic InvalidOpen(
            int tokenStart,
            int relativePosition,
            int errorLength
        ) => new(
            Id: DiagnosticId.TemplateValueInvalidOpen,
            SourceOffset: tokenStart,
            RelativePosition: relativePosition,
            Length: errorLength
        );

        public static Diagnostic IncompleteOpen(
            int tokenStart,
            int relativePosition,
            int errorLength
        ) => new(
            Id: DiagnosticId.TemplateValueIncompleteOpen,
            SourceOffset: tokenStart,
            RelativePosition: relativePosition,
            Length: errorLength,
            Severity: DiagnosticSeverity.Warning
        );

        public static Diagnostic InvalidClose(
            int tokenStart,
            int relativePosition,
            int errorLength
        ) => new(
            Id: DiagnosticId.TemplateValueInvalidClose,
            SourceOffset: tokenStart,
            RelativePosition: relativePosition,
            Length: errorLength
        );

        public static Diagnostic IncompleteClose(
            int tokenStart,
            int relativePosition,
            int errorLength
        ) => new(
            Id: DiagnosticId.TemplateValueIncompleteClose,
            SourceOffset: tokenStart,
            RelativePosition: relativePosition,
            Length: errorLength,
            Severity: DiagnosticSeverity.Warning
        );

        public static Diagnostic TooLong(
            int tokenStart,
            int relativePosition,
            int overflowValueLength,
            bool isCritical,
            int maxValueLength
        ) => new(
            Id: DiagnosticId.TemplateValueTooLong,
            Args: new TooLongArgs(overflowValueLength, maxValueLength),
            SourceOffset: tokenStart,
            RelativePosition: relativePosition,
            Length: overflowValueLength,
            Severity: GetSeverity(isCritical)
        );
        
        public static Diagnostic Unclosed(
            int tokenStart,
            int relativePosition,
            bool isCritical
        ) => new(
            Id: DiagnosticId.TemplateValueUnclosed,
            SourceOffset: tokenStart,
            RelativePosition: relativePosition,
            Length: 0,
            Severity: GetSeverity(isCritical)
        );

        public static Diagnostic UnexpectedContent(
            int tokenStart,
            int relativePosition,
            int invalidCharsCount
        ) => new(
            Id: DiagnosticId.TemplateValueUnexpectedContent,
            SourceOffset: tokenStart,
            RelativePosition: relativePosition,
            Length: invalidCharsCount,
            Severity: DiagnosticSeverity.Warning
        );
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static DiagnosticSeverity GetSeverity(bool isCritical)
    {
        return isCritical ? DiagnosticSeverity.Error : DiagnosticSeverity.Warning;
    }
}
