namespace Foundation.CSharp.Analyzers.Contracts;

// --- [TYPES] ---------------------------------------------------------------------------

[AttributeUsage(validOn: AttributeTargets.Class | AttributeTargets.Struct | AttributeTargets.Interface | AttributeTargets.Method, AllowMultiple = false, Inherited = false)]
public sealed class BoundaryAdapterAttribute : Attribute;

[AttributeUsage(validOn: AttributeTargets.Class | AttributeTargets.Struct | AttributeTargets.Method, AllowMultiple = false, Inherited = false)]
public sealed class DomainScopeAttribute : Attribute;

[AttributeUsage(validOn: AttributeTargets.Class | AttributeTargets.Struct | AttributeTargets.Method, AllowMultiple = false, Inherited = false)]
public sealed class ApplicationScopeAttribute : Attribute;
