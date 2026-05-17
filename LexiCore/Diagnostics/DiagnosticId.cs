namespace LexiCore.Diagnostics;

/// <summary>
/// Defines identifiers for diagnostics produced by the lexer and parser.
/// </summary>
/// <remarks>
/// Diagnostic identifiers represent semantic diagnostic cases used internally by the system
/// and are mapped to <see cref="DiagnosticCode"/> via <see cref="DiagnosticRegistry"/>.
///
/// <para>
/// This API may be used by external consumers,
/// but is not guaranteed to remain stable across major versions.
/// </para>
/// </remarks>
public enum DiagnosticId : ushort
{
    #region Core Lexer/Parser Id (0-999)
    None = 0, // Default
    Unknown = 1,
    #endregion Core Lexer/Parser Id

    #region Style (1000-1999)
    StyleGeneral = 1000, // Reserve (1000-1099)
    #endregion Style

    #region Template (2000-2999)
    TemplateGeneral = 2000, // Reserve (2000-2099)

    // Token (2100-2199)
    TemplateToken = 2100, // Reserve

    TemplateTokenEmpty = 2101,
    TemplateTokenTooLong = 2102,
    TemplateTokenUnclosed = 2103,
    TemplateTokenTrailingEscape = 2104,
    TemplateTokenUnexpectedEOF = 2105,

    // Identifier (2200-2299)
    TemplateIdentifier = 2200, // Reserve

    TemplateIdentifierMissing = 2201,
    TemplateIdentifierInvalidStart = 2202,
    TemplateIdentifierInvalidChars = 2203,
    TemplateIdentifierTooLong = 2204,
    TemplateIdentifierUnexpectedContent = 2205,

    // Separator (2300-2399)
    TemplateSeparator = 2300, // Reserve

    TemplateSeparatorMissing = 2301,
    TemplateSeparatorInvalid = 2302,
    TemplateSeparatorIncomplete = 2303,
    TemplateSeparatorUnexpectedEOF = 2304,

    // ValueKind (2400-2499)
    TemplateValueKind = 2400, // Reserve

    TemplateValueKindInvalid = 2401,
    TemplateValueKindUnexpectedContent = 2402,

    // Value (2500-2599)
    TemplateValue = 2500, // Reserve

    TemplateValueMissing = 2501,
    TemplateValueOpenInvalid = 2502,
    TemplateValueOpenIncomplete = 2503,
    TemplateValueOpenUnexpectedEOF = 2504,
    TemplateValueCloseInvalid = 2505,
    TemplateValueCloseIncomplete = 2506,
    TemplateValueTooLong = 2507,
    TemplateValueUnclosed = 2508,
    TemplateValueUnexpectedContent = 2509,
    #endregion Template
}
