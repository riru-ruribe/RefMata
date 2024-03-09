using Microsoft.CodeAnalysis;

namespace RefMataGenerator;

public static class DiagnosticDescriptors
{
    const string Category = "RefMata";

    public static readonly DiagnosticDescriptor E0001 = new(
        id: Category + nameof(E0001),
        title: "invalid accessibility",
        messageFormat: "'public' or 'protected' or 'internal' or 'private' is allowed.",
        category: Category,
        defaultSeverity: DiagnosticSeverity.Error,
        isEnabledByDefault: true
    );

    public static readonly DiagnosticDescriptor E0002 = new(
        id: Category + nameof(E0002),
        title: "invalid syntax",
        messageFormat: "'partial' class required.",
        category: Category,
        defaultSeverity: DiagnosticSeverity.Error,
        isEnabledByDefault: true
    );

    public static readonly DiagnosticDescriptor E0003 = new(
        id: Category + nameof(E0003),
        title: "invalid syntax",
        messageFormat: "require inherit 'Component' or 'ScriptableObject' or interface.",
        category: Category,
        defaultSeverity: DiagnosticSeverity.Error,
        isEnabledByDefault: true
    );

    public static readonly DiagnosticDescriptor E0004 = new(
        id: Category + nameof(E0004),
        title: "invalid syntax",
        messageFormat: "'System.Collections.Generic' are not allowed.",
        category: Category,
        defaultSeverity: DiagnosticSeverity.Error,
        isEnabledByDefault: true
    );

    public static readonly DiagnosticDescriptor E0005 = new(
        id: Category + nameof(E0005),
        title: "invalid attribute",
        messageFormat: "multiple attributes starting with 'RefMata' can not be set.",
        category: Category,
        defaultSeverity: DiagnosticSeverity.Error,
        isEnabledByDefault: true
    );

    public static readonly DiagnosticDescriptor E0006 = new(
        id: Category + nameof(E0006),
        title: "invalid attribute",
        messageFormat: "'RefMata' attribute is not needed because fields does not have any attributes.",
        category: Category,
        defaultSeverity: DiagnosticSeverity.Error,
        isEnabledByDefault: true
    );

    public static readonly DiagnosticDescriptor E0007 = new(
        id: Category + nameof(E0007),
        title: "invalid attribute",
        messageFormat: "'ScriptableObject' is valid only for 'RefMataLoadAttribute'.",
        category: Category,
        defaultSeverity: DiagnosticSeverity.Error,
        isEnabledByDefault: true
    );

    public static readonly DiagnosticDescriptor W0001 = new(
        id: Category + nameof(W0001),
        title: "warning syntax",
        messageFormat: "''OnValidateGen' method will be generated, call it with 'OnValidate' or add component 'OnValidateHook'.",
        category: Category,
        defaultSeverity: DiagnosticSeverity.Warning,
        isEnabledByDefault: true
    );
}
