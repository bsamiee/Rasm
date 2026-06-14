namespace Rasm.Csp;

// --- [TYPES] ---------------------------------------------------------------------------

public enum CspScope { Domain, Application, Boundary, Composition, HotPath, Test, Tooling }

[AttributeUsage(AttributeTargets.Assembly | AttributeTargets.Class | AttributeTargets.Struct | AttributeTargets.Interface)]
public sealed class CspScopeAttribute(CspScope scope) : Attribute {
    public CspScope Scope { get; } = scope;
}

[AttributeUsage(AttributeTargets.Method | AttributeTargets.Property | AttributeTargets.Constructor)]
public sealed class BoundaryAdapterAttribute : Attribute;

[AttributeUsage(AttributeTargets.Method | AttributeTargets.Property | AttributeTargets.Field
              | AttributeTargets.Constructor | AttributeTargets.Class | AttributeTargets.Struct)]
public sealed class CspExemptAttribute(string justification) : Attribute {
    public string Justification { get; } = justification;
}
