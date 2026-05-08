using System.Collections.Frozen;

namespace LexiCore.Diagnostics;

/// <summary>
/// Internal registry responsible for resolving diagnostic identifiers into diagnostic descriptors.
/// </summary>
/// <remarks>
/// The registry acts as the central mapping layer between <see cref="DiagnosticId"/> and <see cref="DiagnosticDescriptor"/>.
///
/// <para>
/// It is populated during static initialization via a partial <see cref="Register"/> method
/// and stored as an immutable <see cref="FrozenDictionary{DiagnosticId,DiagnosticDescriptor}"/> for fast lookups.
/// </para>
///
/// <para>
/// If a diagnostic identifier is not registered, <see cref="UnknownDescriptor"/> is returned.
/// </para>
/// </remarks>
internal static partial class DiagnosticRegistry
{
    public static readonly DiagnosticDescriptor UnknownDescriptor = Create(
        DiagnosticCode.UnknownError,
        "The diagnostic descriptor is not registered."
    );

    private static readonly FrozenDictionary<DiagnosticId, DiagnosticDescriptor> _map;

    static DiagnosticRegistry()
    {
        Dictionary<DiagnosticId, DiagnosticDescriptor> map = [];
        Register(map);
        _map = map.ToFrozenDictionary();
    }

    public static DiagnosticDescriptor Get(DiagnosticId id)
    {
        return _map.TryGetValue(id, out var descriptor) ? descriptor : UnknownDescriptor;
    }

    private static partial void Register(Dictionary<DiagnosticId, DiagnosticDescriptor> map);
    
    private static DiagnosticDescriptor Create(
        DiagnosticCode code,
        string message
    ) => new(code, (_) => message);

    private static DiagnosticDescriptor Create<T>(
        DiagnosticCode code,
        Func<T, string> formatter
    ) where T : struct
    {
        return new(code, (args) =>
        {
#if DEBUG
            if (args is not T typed)
            {
                throw new InvalidOperationException($"Invalid args for '{typeof(T).Name}'.");
            }
            return formatter(typed);
#else
            return formatter((T)args!);
#endif
        });
    }
}
