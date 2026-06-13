namespace Rasm.Csp;

// --- [TYPES] ---------------------------------------------------------------------------

/// <summary>Scope classification consumed by the Csp analyzer's rule gates.</summary>
public enum CspScope { Domain, Application, Boundary, Composition, HotPath, Test, Tooling }

/// <summary>Declares the Csp scope of an assembly or type, overriding build-property classification.</summary>
[AttributeUsage(AttributeTargets.Assembly | AttributeTargets.Class | AttributeTargets.Struct | AttributeTargets.Interface)]
public sealed class CspScopeAttribute(CspScope scope) : Attribute {
    /// <summary>Declared scope.</summary>
    public CspScope Scope { get; } = scope;
}

/// <summary>Marks a member as an imperative host/runtime boundary adapter.</summary>
[AttributeUsage(AttributeTargets.Method | AttributeTargets.Property | AttributeTargets.Constructor)]
public sealed class BoundaryAdapterAttribute : Attribute;

/// <summary>Exempts a member or type from Csp enforcement; the justification is mandatory and audited.</summary>
[AttributeUsage(AttributeTargets.Method | AttributeTargets.Property | AttributeTargets.Field
              | AttributeTargets.Constructor | AttributeTargets.Class | AttributeTargets.Struct)]
public sealed class CspExemptAttribute(string justification) : Attribute {
    /// <summary>Reason this site is exempt from enforcement.</summary>
    public string Justification { get; } = justification;
}
