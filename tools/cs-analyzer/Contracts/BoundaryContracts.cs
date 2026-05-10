namespace Foundation.CSharp.Analyzers.Contracts;

// --- [ATTRIBUTES] ------------------------------------------------------------

/// <summary>Marks a type or member as an imperative host/runtime boundary.</summary>
[AttributeUsage(validOn: AttributeTargets.Class | AttributeTargets.Struct | AttributeTargets.Method, AllowMultiple = false, Inherited = false)]
public sealed class BoundaryAdapterAttribute : Attribute { }
/// <summary>Marks a type or member as domain scope when namespace or path classification is insufficient.</summary>
[AttributeUsage(validOn: AttributeTargets.Class | AttributeTargets.Struct | AttributeTargets.Method, AllowMultiple = false, Inherited = false)]
public sealed class DomainScopeAttribute : Attribute { }
/// <summary>Marks a type or member as application scope when namespace or path classification is insufficient.</summary>
[AttributeUsage(validOn: AttributeTargets.Class | AttributeTargets.Struct | AttributeTargets.Method, AllowMultiple = false, Inherited = false)]
public sealed class ApplicationScopeAttribute : Attribute { }
