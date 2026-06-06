using System.Collections.Immutable;
using System.Globalization;
using System.Text.RegularExpressions;
using Foundation.CSharp.Analyzers.Contracts;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Operations;

namespace Foundation.CSharp.Analyzers.Kernel;

// --- [TYPES] -------------------------------------------------------------------

internal sealed class ScopeKind {
    // --- [SINGLETONS] ----------------------------------------------------------

    internal static readonly ScopeKind Generated = new(key: "Generated", isAnalyzable: false, isBoundary: false, isDomainOrApplication: false, isComposition: false);
    internal static readonly ScopeKind Test = new(key: "Test", isAnalyzable: false, isBoundary: false, isDomainOrApplication: false, isComposition: false);
    internal static readonly ScopeKind Boundary = new(key: "Boundary", isAnalyzable: true, isBoundary: true, isDomainOrApplication: false, isComposition: false);
    internal static readonly ScopeKind Domain = new(key: "Domain", isAnalyzable: true, isBoundary: false, isDomainOrApplication: true, isComposition: false);
    internal static readonly ScopeKind Application = new(key: "Application", isAnalyzable: true, isBoundary: false, isDomainOrApplication: true, isComposition: false);
    internal static readonly ScopeKind Shared = new(key: "Shared", isAnalyzable: true, isBoundary: false, isDomainOrApplication: true, isComposition: false);
    internal static readonly ScopeKind Analysis = new(key: "Analysis", isAnalyzable: true, isBoundary: false, isDomainOrApplication: false, isComposition: false);
    internal static readonly ScopeKind Composition = new(key: "Composition", isAnalyzable: true, isBoundary: false, isDomainOrApplication: false, isComposition: true);
    internal static readonly ScopeKind Other = new(key: "Other", isAnalyzable: false, isBoundary: false, isDomainOrApplication: false, isComposition: false);

    // --- [CONSTRUCTORS] --------------------------------------------------------

    private ScopeKind(string key, bool isAnalyzable, bool isBoundary, bool isDomainOrApplication, bool isComposition) {
        Key = key;
        IsAnalyzable = isAnalyzable;
        IsBoundary = isBoundary;
        IsDomainOrApplication = isDomainOrApplication;
        IsComposition = isComposition;
    }

    // --- [PROPERTIES] ----------------------------------------------------------

    internal string Key { get; }
    internal bool IsAnalyzable { get; }
    internal bool IsBoundary { get; }
    internal bool IsDomainOrApplication { get; }
    internal bool IsComposition { get; }

}

// --- [CONSTANTS] ---------------------------------------------------------------

internal static class Markers {
    internal static readonly string[] GeneratedPath = [".g.cs", ".designer.cs", "/obj/", "\\obj\\"];
    internal static readonly string[] TestPath = ["/tests/", "\\tests\\", ".Tests", "Test.cs", "Tests.cs"];
    internal static readonly string[] BoundaryNamespace = [".Boundary", ".Rhino", ".Grasshopper", ".Adapter", ".Adapters", ".Routes", ".Endpoints", ".Controllers"];
    internal static readonly string[] BoundaryPath = ["/Rhino/", "\\Rhino\\", "/grasshopper/", "\\grasshopper\\", "/Rasm.Grasshopper/", "\\Rasm.Grasshopper\\", "Adapter.cs", "Boundary.cs", "Endpoint.cs", "Controller.cs"];
    internal const string DomainNamespace = "Domain";
    internal const string DomainPrefix = ".Domain";
    internal const string ApplicationNamespace = "Application";
    internal const string ApplicationPrefix = ".Application";
    internal const string SharedNamespace = "Shared";
    internal const string SharedPrefix = ".Shared";
    internal const string SharedKernelNamespace = "SharedKernel";
    internal const string SharedKernelPrefix = ".SharedKernel";
    internal static readonly string[] SharedPath = ["/Shared/", "\\Shared\\", "/SharedKernel/", "\\SharedKernel\\"];
    internal const string AnalysisNamespace = "Rasm.Analysis";
    internal const string AnalysisPrefix = "Rasm.Analysis.";
    internal static readonly string[] AnalysisPath = ["/libs/csharp/Rasm/Analysis/", "\\libs\\csharp\\Rasm\\Analysis\\"];
    internal static readonly string[] CompositionNamespace = [".Bootstrap", ".Composition", ".DependencyInjection", ".Infrastructure"];
    internal static readonly string[] CompositionPath = ["Composition", "Bootstrap", "DependencyInjection", "Infrastructure"];
    internal const string LanguageExtNamespace = "LanguageExt";
    internal const string MatchMethodName = "Match";
    internal static readonly string[] ObservabilityParts = ["Observability", "Telemetry"];
}

// --- [MODELS] ------------------------------------------------------------------

internal sealed class ScopeInfo {
    // --- [CONSTRUCTORS] --------------------------------------------------------

    internal ScopeInfo(ScopeKind kind, string namespaceName, string filePath) {
        Kind = kind;
        NamespaceName = namespaceName;
        FilePath = filePath;
    }

    // --- [PROPERTIES] ----------------------------------------------------------

    internal ScopeKind Kind { get; }
    internal string NamespaceName { get; }
    internal string FilePath { get; }
    internal bool IsAnalyzable => Kind.IsAnalyzable;
    internal bool IsBoundary => Kind.IsBoundary;
    internal bool IsDomainOrApplication => Kind.IsDomainOrApplication;
    internal bool IsFunctional => Kind.IsDomainOrApplication || ReferenceEquals(objA: Kind, objB: ScopeKind.Analysis);
    internal bool IsComposition => Kind.IsComposition;
    internal bool IsHotPath =>
        NamespaceName.Contains(value: ".Performance", comparisonType: StringComparison.Ordinal)
        || FilePath.Contains(value: "Performance", comparisonType: StringComparison.OrdinalIgnoreCase);
}

internal readonly struct ManualClosedUnionOverrideFacts {
    internal ManualClosedUnionOverrideFacts(string memberName, int overrideCount) {
        MemberName = memberName;
        OverrideCount = overrideCount;
    }

    internal string MemberName { get; }
    internal int OverrideCount { get; }
}

internal readonly struct ForwardingRequestCaseFamilyFacts {
    internal ForwardingRequestCaseFamilyFacts(int caseCount) => CaseCount = caseCount;

    internal int CaseCount { get; }
}

internal enum ClosedUnionDispatchKind {
    Metadata,
    Behavior,
}

internal enum ClosedUnionDispatchReceiverRole {
    Other,
    This,
    Parameter,
}

internal readonly struct ClosedUnionDispatchFact {
    internal ClosedUnionDispatchFact(
        INamedTypeSymbol union,
        ClosedUnionDispatchKind kind,
        int caseCount,
        string caseSetKey,
        string ownerKey,
        bool ownerIsUnion,
        ClosedUnionDispatchReceiverRole receiverRole,
        string filePath,
        Location location) {
        Union = union;
        Kind = kind;
        CaseCount = caseCount;
        CaseSetKey = caseSetKey;
        OwnerKey = ownerKey;
        OwnerIsUnion = ownerIsUnion;
        ReceiverRole = receiverRole;
        FilePath = filePath;
        Location = location;
    }

    internal INamedTypeSymbol Union { get; }
    internal ClosedUnionDispatchKind Kind { get; }
    internal int CaseCount { get; }
    internal string CaseSetKey { get; }
    internal string OwnerKey { get; }
    internal bool OwnerIsUnion { get; }
    internal ClosedUnionDispatchReceiverRole ReceiverRole { get; }
    internal string FilePath { get; }
    internal Location Location { get; }
}

// --- [OPERATIONS] --------------------------------------------------------------

internal static class ScopeModel {
    // --- [OPERATIONS] ----------------------------------------------------------

    internal static ScopeInfo Classify(ISymbol symbol) {
        string namespaceName = symbol.ContainingNamespace?.ToDisplayString() ?? string.Empty;
        string filePath = symbol.DeclaringSyntaxReferences switch {
            { IsDefaultOrEmpty: true } => string.Empty,
            ImmutableArray<SyntaxReference> refs => refs[0].SyntaxTree.FilePath ?? string.Empty,
        };
        bool generated = IsGeneratedPath(filePath)
            || SymbolFacts.HasAnyAttribute(symbol, "GeneratedCodeAttribute", "CompilerGeneratedAttribute");
        bool test = IsTestScope(namespaceName: namespaceName, filePath: filePath)
            || symbol.ContainingAssembly?.Name?.EndsWith(value: ".Tests", comparisonType: StringComparison.OrdinalIgnoreCase) == true;
        bool boundary = Markers.BoundaryNamespace.Any(marker => namespaceName.Contains(value: marker, comparisonType: StringComparison.OrdinalIgnoreCase))
            || Markers.BoundaryPath.Any(marker => filePath.Contains(value: marker, comparisonType: StringComparison.OrdinalIgnoreCase))
            || SymbolFacts.HasAnyAttribute(symbol, nameof(BoundaryAdapterAttribute), "BoundaryAdapter");
        bool domain = SymbolFacts.IsDomainNamespace(namespaceName)
            || SymbolFacts.HasAnyAttribute(symbol, nameof(DomainScopeAttribute), "DomainScope");
        bool application = SymbolFacts.IsApplicationNamespace(namespaceName)
            || SymbolFacts.HasAnyAttribute(symbol, nameof(ApplicationScopeAttribute), "ApplicationScope");
        bool shared = SymbolFacts.IsSharedNamespace(namespaceName)
            || Markers.SharedPath.Any(marker => filePath.Contains(value: marker, comparisonType: StringComparison.OrdinalIgnoreCase));
        bool analysis = namespaceName.Equals(value: Markers.AnalysisNamespace, comparisonType: StringComparison.OrdinalIgnoreCase)
            || namespaceName.StartsWith(value: Markers.AnalysisPrefix, comparisonType: StringComparison.OrdinalIgnoreCase)
            || Markers.AnalysisPath.Any(marker => filePath.Contains(value: marker, comparisonType: StringComparison.OrdinalIgnoreCase));
        bool composition = SymbolFacts.IsCompositionScope(namespaceName: namespaceName, filePath: filePath);
        ScopeKind kind = (generated, test, boundary, domain, application, shared, analysis, composition) switch {
            (true, _, _, _, _, _, _, _) => ScopeKind.Generated,
            (_, true, _, _, _, _, _, _) => ScopeKind.Test,
            (_, _, true, _, _, _, _, _) => ScopeKind.Boundary,
            (_, _, _, true, _, _, _, _) => ScopeKind.Domain,
            (_, _, _, _, true, _, _, _) => ScopeKind.Application,
            (_, _, _, _, _, true, _, _) => ScopeKind.Shared,
            (_, _, _, _, _, _, true, _) => ScopeKind.Analysis,
            (_, _, _, _, _, _, _, true) => ScopeKind.Composition,
            _ => ScopeKind.Other,
        };
        return new ScopeInfo(kind: kind, namespaceName: namespaceName, filePath: filePath);
    }
    internal static bool IsGeneratedPath(string filePath) =>
        Markers.GeneratedPath.Any(marker => filePath.Contains(value: marker, comparisonType: StringComparison.OrdinalIgnoreCase));
    internal static bool IsTestScope(string namespaceName, string filePath) =>
        namespaceName.Contains(value: ".Tests", comparisonType: StringComparison.OrdinalIgnoreCase)
        || namespaceName.StartsWith(value: "Tests", comparisonType: StringComparison.OrdinalIgnoreCase)
        || Markers.TestPath.Any(marker => filePath.Contains(value: marker, comparisonType: StringComparison.OrdinalIgnoreCase));
}

// --- [SYNTAX_FACTS] ------------------------------------------------------------

internal static class SymbolFacts {
    // --- [CONSTANTS] -----------------------------------------------------------

    private static readonly HashSet<string> BlockingMethods = new(["Wait", "WaitAll", "WaitAny", "GetResult", "Sleep"], StringComparer.Ordinal);
    private static readonly HashSet<string> OperationalReceiptNames = new([
        "Added", "Attached", "Attributes", "Changed", "Created", "Deleted", "Detached", "Flashed", "Hidden", "Lifecycle",
        "Locked", "Moved", "Redraw", "Removed", "Replaced", "Resource", "Resources", "Selected", "Transformed", "Undo",
        "Unselected", "Updated",
    ], StringComparer.Ordinal);
    private static readonly HashSet<string> ProofReceiptFragments = new([
        "Admission", "Approximation", "Backend", "Cloud", "Eigen", "Finite", "Grid", "Hull", "Iteration", "Iterations",
        "Kernel", "Lipschitz", "Matrix", "Mesh", "Native", "Path", "Policy", "Residual", "Route", "Sample", "Sampling",
        "Sdf", "Search", "Solver", "Spectral", "Status", "Stop", "Tolerance", "Topology", "Volume",
    ], StringComparer.Ordinal);
    private static readonly HashSet<string> AcceptValueIsValidTypeNames = new([
        "Arc", "BoundingBox", "Box", "Circle", "ClosestHit", "Cone", "Cylinder", "Ellipse", "Interval", "IntersectionHit", "Line",
        "Plane", "Point2d", "Point3d", "Polyline", "RayQuery", "Rectangle3d", "Sphere", "Torus", "Transform", "Vector3d",
    ], StringComparer.Ordinal);
    private const int RegexOptionNonBacktrackingBit = 1024;
    private const int SearchValuesFriendlyRegexOptionMask =
        (int)RegexOptions.CultureInvariant
        | (int)RegexOptions.Compiled
        | RegexOptionNonBacktrackingBit;

    // --- [ATTRIBUTE_FACTS] -----------------------------------------------------

    internal static bool HasAnyAttribute(ISymbol symbol, params string[] names) =>
        AllAttributes(symbol).Any(attribute => names.Contains(attribute.AttributeClass?.Name ?? string.Empty, StringComparer.Ordinal));
    internal static IEnumerable<AttributeData> AllAttributes(ISymbol symbol) =>
        symbol.GetAttributes()
            .Concat(symbol is IMethodSymbol { AssociatedSymbol: ISymbol associatedSymbol } ? associatedSymbol.GetAttributes() : [])
            .Concat(symbol.ContainingType?.GetAttributes() ?? []);

    // --- [OPERATION_FACTS] -----------------------------------------------------

    // Fluent-pipeline receiver walk: extension methods carry their receiver in Arguments[0], not Instance.
    // Use ExtractReceiver to obtain the receiver of any invocation regardless of extension-vs-instance shape;
    // chain with UnwrapReceiver to peel implicit IConversionOperation wrappers (boxing, generic constraints).
    internal static IOperation? ExtractReceiver(IInvocationOperation invocation) =>
        invocation.Instance switch {
            IOperation receiver => receiver,
            _ => invocation.TargetMethod.IsExtensionMethod switch {
                true when invocation.Arguments.Length > 0 => invocation.Arguments[0].Value,
                _ => null,
            },
        };
    internal static IOperation? UnwrapReceiver(IOperation? operation) =>
        operation switch {
            IConversionOperation { Operand: IOperation inner } => UnwrapReceiver(inner),
            _ => operation,
        };
    internal static IOperation? UnwrapValue(IOperation? operation) =>
        operation switch {
            IConversionOperation conversion => UnwrapValue(conversion.Operand),
            IParenthesizedOperation parenthesized => UnwrapValue(parenthesized.Operand),
            IBlockOperation { Operations.Length: 1 } block => UnwrapValue(block.Operations[0]),
            IReturnOperation { ReturnedValue: IOperation returned } => UnwrapValue(returned),
            _ => operation,
        };
    internal static IAnonymousFunctionOperation? UnwrapLambda(IOperation operation) =>
        operation switch {
            IAnonymousFunctionOperation lambda => lambda,
            IDelegateCreationOperation { Target: IAnonymousFunctionOperation lambda } => lambda,
            IConversionOperation { Operand: IOperation inner } => UnwrapLambda(inner),
            _ => null,
        };
    private static SyntaxNode UnwrapTransparentExpression(SyntaxNode node) =>
        node switch {
            ParenthesizedExpressionSyntax parenthesized => UnwrapTransparentExpression(parenthesized.Expression),
            CastExpressionSyntax castExpression => UnwrapTransparentExpression(castExpression.Expression),
            CheckedExpressionSyntax checkedExpression => UnwrapTransparentExpression(checkedExpression.Expression),
            AwaitExpressionSyntax awaitExpression => UnwrapTransparentExpression(awaitExpression.Expression),
            PostfixUnaryExpressionSyntax { RawKind: (int)SyntaxKind.SuppressNullableWarningExpression } suppressNullable
                => UnwrapTransparentExpression(suppressNullable.Operand),
            _ => node,
        };
    private static IOperation? CollapseTransparentParents(IOperation operation) =>
        operation.Parent switch {
            IConversionOperation or IParenthesizedOperation => CollapseTransparentParents(operation.Parent),
            IOperation parent => parent,
            _ => null,
        };

    // --- [FLOW_FACTS] ----------------------------------------------------------

    internal static bool IsLanguageExtMatch(IInvocationOperation invocation, INamespaceSymbol? languageExtNamespace) {
        bool typeNamespaceMatch = invocation.TargetMethod.ContainingType?.ContainingNamespace is INamespaceSymbol ns
            && languageExtNamespace is not null
            && (SymbolEqualityComparer.Default.Equals(ns, languageExtNamespace)
                || ns.ToDisplayString().StartsWith(value: languageExtNamespace.ToDisplayString(), comparisonType: StringComparison.Ordinal));
        return invocation.TargetMethod.Name == Markers.MatchMethodName && typeNamespaceMatch;
    }
    internal static bool IsLanguageExtInvocation(IInvocationOperation invocation, params string[] names) =>
        names.Contains(invocation.TargetMethod.Name, StringComparer.Ordinal)
        && (invocation.TargetMethod.ContainingType?.ContainingNamespace?.ToDisplayString() ?? string.Empty)
            .StartsWith(value: Markers.LanguageExtNamespace, comparisonType: StringComparison.Ordinal);
    internal static bool IsRegexMatchCall(IInvocationOperation invocation) =>
        invocation.TargetMethod.Name == Markers.MatchMethodName
        && invocation.TargetMethod.ContainingType?.OriginalDefinition.ToDisplayString() == "System.Text.RegularExpressions.Regex";
    internal static bool TryGetGeneratedRegexPattern(IMethodSymbol method, out string pattern, out RegexOptions options) {
        AttributeData? generatedRegex = method
            .GetAttributes()
            .FirstOrDefault(attribute => IsGeneratedRegexAttribute(attribute.AttributeClass));
        ImmutableArray<TypedConstant> constructorArguments = generatedRegex?.ConstructorArguments ?? [];
        pattern = constructorArguments.Length switch {
            > 0 when constructorArguments[0].Value is string text => text,
            _ => string.Empty,
        };
        options = ReadGeneratedRegexOptions(constructorArguments);
        return pattern.Length > 0;
    }
    private static bool IsGeneratedRegexAttribute(INamedTypeSymbol? attributeType) =>
        attributeType?.ToDisplayString() == "System.Text.RegularExpressions.GeneratedRegexAttribute";
    private static RegexOptions ReadGeneratedRegexOptions(ImmutableArray<TypedConstant> constructorArguments) =>
        constructorArguments.Length switch {
            > 1 when constructorArguments[1].Value is int raw => (RegexOptions)raw,
            _ => RegexOptions.None,
        };
    internal static bool IsSearchValuesFriendlyRegexOptions(RegexOptions options) =>
        ((int)options & ~SearchValuesFriendlyRegexOptionMask) == 0;
    internal static bool IsSimpleCharsetLengthRegex(string pattern) {
        bool anchored = pattern.StartsWith(value: "^[", comparisonType: StringComparison.Ordinal)
            && pattern.EndsWith(value: "$", comparisonType: StringComparison.Ordinal);
        int closeBracket = anchored ? pattern.IndexOf(']') : -1;
        bool hasBracket = closeBracket > 2;
        int openBrace = hasBracket ? closeBracket + 1 : -1;
        bool hasBrace = openBrace >= 0 && openBrace < pattern.Length && pattern[openBrace] == '{';
        int closeBrace = hasBrace ? pattern.IndexOf('}', openBrace + 1) : -1;
        bool quantifierAtEnd = closeBrace == pattern.Length - 2;
        bool singleClass = hasBracket
            && pattern.IndexOf('[', 2) < 0
            && pattern.IndexOf(']', closeBracket + 1) < 0;
        bool singleQuantifier = hasBrace
            && pattern.IndexOf('{', openBrace + 1) < 0
            && pattern.IndexOf('}', closeBrace + 1) < 0;
        string charSet = hasBracket
            ? pattern.Substring(startIndex: 2, length: closeBracket - 2)
            : string.Empty;
        string quantifier = quantifierAtEnd && hasBrace
            ? pattern.Substring(startIndex: openBrace + 1, length: closeBrace - openBrace - 1)
            : string.Empty;
        bool literalCharSet = charSet.Length > 0
            && !HasPotentialRange(charSet)
            && charSet.All(IsSearchValuesLiteralChar);
        bool fixedQuantifier = IsFixedLengthQuantifier(quantifier);
        return anchored && hasBracket && hasBrace && quantifierAtEnd && singleClass && singleQuantifier && literalCharSet && fixedQuantifier;
    }
    private static bool IsSearchValuesLiteralChar(char value) =>
        char.IsLetterOrDigit(value) || value is '_' or ':' or '-';
    private static bool HasPotentialRange(string charSet) =>
        charSet.Length > 2 && charSet.AsSpan(start: 1, length: charSet.Length - 2).IndexOf('-') >= 0;
    private static bool IsFixedLengthQuantifier(string value) {
        int separator = value.IndexOf(',');
        return separator switch {
            < 0 => int.TryParse(value, NumberStyles.None, CultureInfo.InvariantCulture, out int exact) && exact > 0,
            > 0 when separator < value.Length - 1 && value.IndexOf(',', separator + 1) < 0
                => int.TryParse(value.Substring(startIndex: 0, length: separator), NumberStyles.None, CultureInfo.InvariantCulture, out int min)
                    && int.TryParse(value.Substring(startIndex: separator + 1), NumberStyles.None, CultureInfo.InvariantCulture, out int max)
                    && min > 0
                    && max >= min,
            _ => false,
        };
    }
    internal static bool IsEarlyReturnGuard(IConditionalOperation conditional) =>
        conditional.Syntax is IfStatementSyntax {
            Else: null,
            Condition: PrefixUnaryExpressionSyntax { RawKind: (int)SyntaxKind.LogicalNotExpression },
            Statement: StatementSyntax statement,
        }
        && statement.DescendantNodesAndSelf().Any(node => node.IsKind(SyntaxKind.ReturnStatement));

    // --- [ASYNC_FACTS] ---------------------------------------------------------

    internal static bool IsTaskRun(IInvocationOperation invocation) =>
        (invocation.TargetMethod.ContainingType?.OriginalDefinition?.ToDisplayString() == "System.Threading.Tasks.Task"
            && invocation.TargetMethod.Name == "Run")
        || (invocation.TargetMethod.ContainingType?.OriginalDefinition?.ToDisplayString() == "System.Threading.Tasks.TaskFactory"
            && invocation.TargetMethod.Name == "StartNew")
        || (invocation.TargetMethod.ContainingType?.ToDisplayString() == "System.Threading.ThreadPool"
            && invocation.TargetMethod.Name == "QueueUserWorkItem");
    internal static bool IsTaskWhenAllUnbounded(IInvocationOperation invocation) {
        bool isWhenAll = invocation.TargetMethod.Name == "WhenAll"
            && invocation.TargetMethod.ContainingType.OriginalDefinition.ToDisplayString() == "System.Threading.Tasks.Task";
        ITypeSymbol? argumentType = (isWhenAll, invocation.Arguments.Length) switch {
            (true, 1) => invocation.Arguments[0].Value.Type,
            _ => null,
        };
        return argumentType is not null
            && argumentType is not IArrayTypeSymbol
            && ((argumentType is INamedTypeSymbol directType && IsGenericIEnumerableTaskLike(directType))
                || argumentType.AllInterfaces.Any(static interfaceSymbol => IsGenericIEnumerableTaskLike(interfaceSymbol)));
    }
    private static bool IsGenericIEnumerableTaskLike(INamedTypeSymbol namedType) =>
        namedType.MetadataName == "IEnumerable`1"
        && namedType.ContainingNamespace.ToDisplayString() == "System.Collections.Generic"
        && namedType.TypeArguments.Length == 1
        && IsTaskLikeType(namedType.TypeArguments[0]);
    internal static bool IsLanguageExtRunCollapse(IInvocationOperation invocation) =>
        invocation.TargetMethod.Name is "Run" or "RunAsync"
        && (invocation.TargetMethod.ContainingType?.ContainingNamespace?.ToDisplayString() ?? string.Empty)
            .StartsWith(value: Markers.LanguageExtNamespace, comparisonType: StringComparison.Ordinal)
        && (invocation.TargetMethod.ContainingType?.Name ?? string.Empty) is "Eff" or "IO" or "Fin" or "Try" or "TryOption" or "Option" or "Validation";
    internal static bool IsBlockingInvocation(IInvocationOperation invocation) =>
        BlockingMethods.Contains(invocation.TargetMethod.Name)
        && invocation.TargetMethod.ContainingType is INamedTypeSymbol containingType
        && (IsTaskOrAwaiterType(containingType)
            || containingType.ToDisplayString() == "System.Threading.Thread");
    internal static bool IsTaskLikeType(ITypeSymbol? type) =>
        type?.OriginalDefinition.ToDisplayString() is "System.Threading.Tasks.Task" or "System.Threading.Tasks.Task<TResult>" or "System.Threading.Tasks.ValueTask" or "System.Threading.Tasks.ValueTask<TResult>";
    private static bool IsTaskOrAwaiterType(INamedTypeSymbol type) {
        string displayName = type.OriginalDefinition.ToDisplayString();
        bool knownTaskLike = displayName is
            "System.Threading.Tasks.Task"
            or "System.Threading.Tasks.Task<TResult>"
            or "System.Threading.Tasks.TaskFactory"
            or "System.Runtime.CompilerServices.TaskAwaiter"
            or "System.Runtime.CompilerServices.TaskAwaiter<TResult>"
            or "System.Runtime.CompilerServices.ConfiguredTaskAwaitable.ConfiguredTaskAwaiter"
            or "System.Runtime.CompilerServices.ConfiguredTaskAwaitable<TResult>.ConfiguredTaskAwaiter"
            or "System.Runtime.CompilerServices.ValueTaskAwaiter"
            or "System.Runtime.CompilerServices.ValueTaskAwaiter<TResult>"
            or "System.Runtime.CompilerServices.ConfiguredValueTaskAwaitable.ConfiguredValueTaskAwaiter"
            or "System.Runtime.CompilerServices.ConfiguredValueTaskAwaitable<TResult>.ConfiguredValueTaskAwaiter";
        bool awaiterByContract = type.Name.EndsWith(value: "Awaiter", comparisonType: StringComparison.Ordinal)
            && type.AllInterfaces.Any(interfaceType =>
                interfaceType.ToDisplayString() is "System.Runtime.CompilerServices.INotifyCompletion" or "System.Runtime.CompilerServices.ICriticalNotifyCompletion");
        return knownTaskLike || awaiterByContract;
    }

    // --- [RUNTIME_FACTS] -------------------------------------------------------

    internal static bool IsNullComparison(IBinaryOperation binary) =>
        binary.OperatorKind is BinaryOperatorKind.Equals or BinaryOperatorKind.NotEquals
        && (binary.LeftOperand.ConstantValue is { HasValue: true, Value: null }
            || binary.RightOperand.ConstantValue is { HasValue: true, Value: null });
    internal static bool IsNullPattern(IIsPatternOperation isPatternOperation) =>
        isPatternOperation.Pattern switch {
            IConstantPatternOperation { Value.ConstantValue: { HasValue: true, Value: null } } => true,
            INegatedPatternOperation { Pattern: IConstantPatternOperation { Value.ConstantValue: { HasValue: true, Value: null } } } => true,
            _ => false,
        };
    internal static bool IsReferenceEqualsNull(IInvocationOperation invocation) =>
        invocation.TargetMethod.Name == "ReferenceEquals"
        && invocation.TargetMethod.ContainingType.SpecialType == SpecialType.System_Object
        && invocation.Arguments.Any(argument => argument.Value.ConstantValue is { HasValue: true, Value: null });
    internal static bool IsUnreachableThrow(IThrowOperation throwOperation) =>
        throwOperation.Exception is IObjectCreationOperation { Type: INamedTypeSymbol type }
        && type.OriginalDefinition.ToDisplayString() == "System.Diagnostics.UnreachableException";

    // --- [BOUNDARY_FACTS] ------------------------------------------------------

    internal static bool IsBoundaryMatchUsage(IInvocationOperation invocation) =>
        CollapseTransparentParents(invocation) is IReturnOperation
        || (IsLanguageExtUnit(type: invocation.Type)
            && (IsTerminalExpressionStatement(operation: CollapseTransparentParents(operation: invocation))
                || IsTerminalDiscardAssignment(operation: CollapseTransparentParents(operation: invocation))))
        || (invocation.Syntax.Parent is ArrowExpressionClauseSyntax clause
            && clause.Expression is ExpressionSyntax expression
            && SyntaxFactory.AreEquivalent(UnwrapTransparentExpression(invocation.Syntax), UnwrapTransparentExpression(expression)));
    private static bool IsLanguageExtUnit(ITypeSymbol? type) =>
        type?.ToDisplayString() == "LanguageExt.Unit";
    private static bool IsTerminalExpressionStatement(IOperation? operation) =>
        operation is IExpressionStatementOperation statement
        && statement.Parent is IBlockOperation block
        && block.Operations.LastOrDefault() == statement;
    private static bool IsTerminalDiscardAssignment(IOperation? operation) =>
        operation is ISimpleAssignmentOperation { Target: IDiscardOperation, Parent: IExpressionStatementOperation statement }
        && statement.Parent is IBlockOperation block
        && block.Operations.LastOrDefault() == statement;

    // --- [DOMAIN_FACTS] --------------------------------------------------------

    internal static bool IsDomainNamespace(string namespaceName) =>
        namespaceName.StartsWith(value: Markers.DomainNamespace, comparisonType: StringComparison.OrdinalIgnoreCase)
        || namespaceName.Contains(value: Markers.DomainPrefix, comparisonType: StringComparison.OrdinalIgnoreCase);
    internal static bool IsApplicationNamespace(string namespaceName) =>
        namespaceName.StartsWith(value: Markers.ApplicationNamespace, comparisonType: StringComparison.OrdinalIgnoreCase)
        || namespaceName.Contains(value: Markers.ApplicationPrefix, comparisonType: StringComparison.OrdinalIgnoreCase);
    internal static bool IsSharedNamespace(string namespaceName) =>
        namespaceName.StartsWith(value: Markers.SharedNamespace, comparisonType: StringComparison.OrdinalIgnoreCase)
        || namespaceName.Contains(value: Markers.SharedPrefix, comparisonType: StringComparison.OrdinalIgnoreCase)
        || namespaceName.StartsWith(value: Markers.SharedKernelNamespace, comparisonType: StringComparison.OrdinalIgnoreCase)
        || namespaceName.Contains(value: Markers.SharedKernelPrefix, comparisonType: StringComparison.OrdinalIgnoreCase);
    internal static bool IsDomainOrApplicationNamespace(string namespaceName) =>
        IsDomainNamespace(namespaceName) || IsApplicationNamespace(namespaceName) || IsSharedNamespace(namespaceName);
    internal static bool IsCompositionScope(string namespaceName, string filePath) =>
        Markers.CompositionNamespace.Any(marker => namespaceName.Contains(value: marker, comparisonType: StringComparison.OrdinalIgnoreCase))
        || Markers.CompositionPath.Any(marker => filePath.Contains(value: marker, comparisonType: StringComparison.OrdinalIgnoreCase));
    internal static bool IsObservabilitySurface(ISymbol symbol, string namespaceName) =>
        namespaceName.Split(separator: '.').Any(part =>
            Markers.ObservabilityParts.Any(marker => string.Equals(part, marker, StringComparison.Ordinal)))
        || HasAnyAttribute(symbol, "ObservabilitySurfaceAttribute", "ObservabilitySurface");

    // --- [TYPE_SHAPE_FACTS] ----------------------------------------------------

    internal static readonly HashSet<string> MutableCollectionNames =
        new(["List`1", "Dictionary`2", "HashSet`1", "Queue`1", "Stack`1", "SortedDictionary`2", "SortedSet`1"], StringComparer.Ordinal);
    internal static bool IsInsideLoop(IOperation? operation) =>
        operation switch {
            null => false,
            ILoopOperation => true,
            _ => IsInsideLoop(operation.Parent),
        };
    internal static string OuterAccumulatorTarget(ILoopOperation loop, ISimpleAssignmentOperation assignment) =>
        assignment.Target switch {
            ILocalReferenceOperation localRef when IsLocalDeclaredOutsideLoop(loop: loop, local: localRef.Local)
                => localRef.Local.Name,
            _ => string.Empty,
        };
    internal static bool IsSelfReassignment(ISimpleAssignmentOperation assignment) =>
        assignment.Target switch {
            ILocalReferenceOperation targetLocal => assignment.Value
                .DescendantsAndSelf()
                .OfType<ILocalReferenceOperation>()
                .Any(local => SymbolEqualityComparer.Default.Equals(local.Local, targetLocal.Local)),
            _ => false,
        };
    private static bool IsLocalDeclaredOutsideLoop(ILoopOperation loop, ILocalSymbol local) =>
        local.DeclaringSyntaxReferences switch {
            { IsDefaultOrEmpty: true } => false,
            ImmutableArray<SyntaxReference> refs => refs[0].GetSyntax() switch {
                SyntaxNode declarationNode => !loop.Syntax.Span.Contains(declarationNode.Span)
                    && !IsForLoopHeaderDeclaration(loop: loop, declarationNode: declarationNode),
                _ => false,
            },
        };
    private static bool IsForLoopHeaderDeclaration(ILoopOperation loop, SyntaxNode declarationNode) =>
        loop.Syntax switch {
            ForStatementSyntax forStatement when forStatement.Declaration is VariableDeclarationSyntax declaration
                => declaration.Span.Contains(declarationNode.Span),
            ForEachStatementSyntax => false,
            _ => false,
        };
    private static readonly HashSet<string> ValidatedReturnTypes = new(["Fin`1", "K`2"], StringComparer.Ordinal);
    internal static bool IsFinOrKReturnType(IMethodSymbol method) =>
        IsValidatedFactoryReturnType(method.ReturnType);
    internal static bool IsValidatedFactoryReturnType(ITypeSymbol returnType) =>
        (ValidatedReturnTypes.Contains(returnType.OriginalDefinition.MetadataName)
            && (returnType.ContainingNamespace?.ToDisplayString() ?? string.Empty)
                .StartsWith(value: Markers.LanguageExtNamespace, comparisonType: StringComparison.Ordinal))
        || IsValidationErrorReturnType(returnType);
    internal static bool IsValidationErrorReturnType(ITypeSymbol returnType) =>
        IsLanguageExtValidationType(returnType)
        && returnType is INamedTypeSymbol { TypeArguments.Length: 2 } validation
        && validation.TypeArguments[0] is INamedTypeSymbol { Name: "Error" } errorType
        && errorType.ContainingNamespace.ToDisplayString().StartsWith(value: "LanguageExt.Common", comparisonType: StringComparison.Ordinal);
    internal static bool IsLanguageExtCommonErrorType(ITypeSymbol? type) =>
        type is INamedTypeSymbol errorType
        && errorType.Name == "Error"
        && errorType.ContainingNamespace.ToDisplayString().StartsWith(value: "LanguageExt.Common", comparisonType: StringComparison.Ordinal);
    private static IEnumerable<INamedTypeSymbol> TypeAndBases(INamedTypeSymbol type) {
        for (INamedTypeSymbol? current = type; current is not null; current = current.BaseType) {
            yield return current;
        }
    }
    internal static bool IsLanguageExtCommonErrorAssignableType(ITypeSymbol? type, INamedTypeSymbol? commonErrorType = null) =>
        type is INamedTypeSymbol candidate
        && TypeAndBases(candidate).Any(baseType => commonErrorType switch {
            INamedTypeSymbol errorType => SymbolEqualityComparer.Default.Equals(baseType, errorType),
            _ => IsLanguageExtCommonErrorType(baseType),
        });
    internal static bool IsLanguageExtValidationType(ITypeSymbol type) =>
        type is INamedTypeSymbol { OriginalDefinition.MetadataName: "Validation`2", TypeArguments.Length: 2 } validation
        && validation.ContainingNamespace.ToDisplayString().StartsWith(value: Markers.LanguageExtNamespace, comparisonType: StringComparison.Ordinal);
    internal static bool IsLanguageExtUnitType(ITypeSymbol? type) =>
        type is INamedTypeSymbol unitType
        && unitType.Name == "Unit"
        && unitType.ContainingNamespace.ToDisplayString().StartsWith(value: Markers.LanguageExtNamespace, comparisonType: StringComparison.Ordinal);
    internal static bool IsLanguageExtUnitValue(IOperation? operation) =>
        operation switch {
            IFieldReferenceOperation { Field.Name: "unit", Field.ContainingType.Name: "Prelude" } field =>
                IsLanguageExtUnitType(field.Type)
                && field.Field.ContainingType.ContainingNamespace.ToDisplayString().StartsWith(value: Markers.LanguageExtNamespace, comparisonType: StringComparison.Ordinal),
            IPropertyReferenceOperation { Property.Name: "unit", Property.ContainingType.Name: "Prelude" } property =>
                IsLanguageExtUnitType(property.Type)
                && property.Property.ContainingType.ContainingNamespace.ToDisplayString().StartsWith(value: Markers.LanguageExtNamespace, comparisonType: StringComparison.Ordinal),
            _ => false,
        };
    internal static bool IsLanguageExtFinUnitType(ITypeSymbol? type) =>
        type is INamedTypeSymbol { OriginalDefinition.MetadataName: "Fin`1", TypeArguments.Length: 1 } fin
        && fin.ContainingNamespace.ToDisplayString().StartsWith(value: Markers.LanguageExtNamespace, comparisonType: StringComparison.Ordinal)
        && IsLanguageExtUnitType(fin.TypeArguments[0]);
    internal static bool IsLanguageExtFinType(ITypeSymbol? type) =>
        type is INamedTypeSymbol { OriginalDefinition.MetadataName: "Fin`1", TypeArguments.Length: 1 } fin
        && fin.ContainingNamespace.ToDisplayString().StartsWith(value: Markers.LanguageExtNamespace, comparisonType: StringComparison.Ordinal);
    internal static bool IsLanguageExtFinSuccessInvocation(IInvocationOperation invocation) =>
        invocation.TargetMethod.Name == "Succ"
        && IsLanguageExtFinType(invocation.Type)
        && invocation.Arguments.Length == 1;
    internal static bool IsLanguageExtFinFailureInvocation(IInvocationOperation invocation) =>
        IsLanguageExtFinFailureInvocation(invocation: invocation, commonErrorType: null);
    internal static bool IsLanguageExtFinFailureInvocation(IInvocationOperation invocation, INamedTypeSymbol? commonErrorType) =>
        invocation.TargetMethod.Name == "Fail"
        && IsLanguageExtFinType(invocation.Type)
        && invocation.Arguments.Length > 0
        && IsLanguageExtCommonErrorAssignableType(type: UnwrapValue(invocation.Arguments[0].Value)?.Type, commonErrorType: commonErrorType);
    internal static bool IsManualGenericProjectionGate(IConditionalOperation conditional, ISymbol? containingSymbol) =>
        TryProjectionTypeParameter(type: conditional.Type, parameter: out ITypeParameterSymbol? parameter)
        && !IsProjectionOwner(symbol: containingSymbol)
        && conditional.WhenFalse is IOperation whenFalse
        && ContainsTypeof(operation: conditional.Condition, parameter: parameter)
        && HasProjectionAndUnsupported(success: conditional.WhenTrue, failure: whenFalse, parameter: parameter);
    internal static bool IsManualGenericProjectionGate(ISwitchExpressionOperation switchExpression, ISymbol? containingSymbol) {
        if (!TryProjectionTypeParameter(type: switchExpression.Type, parameter: out ITypeParameterSymbol? parameter)
            || IsProjectionOwner(symbol: containingSymbol)
            || !ContainsTypeof(operation: switchExpression.Value, parameter: parameter)) {
            return false;
        }
        int projectionArms = switchExpression.Arms.Count(arm => ContainsObjectMediatedCast(operation: arm.Value, parameter: parameter));
        int unsupportedArms = switchExpression.Arms.Count(arm => ContainsUnsupportedFailure(operation: arm.Value, parameter: parameter));
        return projectionArms == 1 && unsupportedArms == 1;
    }
    private static bool TryProjectionTypeParameter(ITypeSymbol? type, out ITypeParameterSymbol parameter) {
        parameter = null!;
        if (type is INamedTypeSymbol { OriginalDefinition.MetadataName: "Fin`1", TypeArguments.Length: 1 } fin
            && fin.ContainingNamespace.ToDisplayString().StartsWith(value: Markers.LanguageExtNamespace, comparisonType: StringComparison.Ordinal)
            && fin.TypeArguments[0] is ITypeParameterSymbol typeParameter) {
            parameter = typeParameter;
            return true;
        }
        return false;
    }
    private static bool IsProjectionOwner(ISymbol? symbol) =>
        symbol is IMethodSymbol method
        && (method.ContainingType?.Name is "AtomProjection" or "OpAcceptance"
            || method.Name is "Self" or "Value" or "Values" or "Custom" or "AcceptResults");
    private static bool HasProjectionAndUnsupported(IOperation success, IOperation failure, ITypeParameterSymbol parameter) =>
        (ContainsObjectMediatedCast(operation: success, parameter: parameter) && ContainsUnsupportedFailure(operation: failure, parameter: parameter))
        || (ContainsObjectMediatedCast(operation: failure, parameter: parameter) && ContainsUnsupportedFailure(operation: success, parameter: parameter));
    private static bool ContainsTypeof(IOperation operation, ITypeParameterSymbol parameter) =>
        operation.DescendantsAndSelf()
            .OfType<ITypeOfOperation>()
            .Any(typeOf => SymbolEqualityComparer.Default.Equals(typeOf.TypeOperand, parameter));
    private static bool ContainsObjectMediatedCast(IOperation operation, ITypeParameterSymbol parameter) =>
        operation.DescendantsAndSelf()
            .OfType<IConversionOperation>()
            .Any(conversion => SymbolEqualityComparer.Default.Equals(conversion.Type, parameter) && IsObjectBridge(operation: conversion.Operand));
    private static bool IsObjectBridge(IOperation operation) =>
        operation.Type?.SpecialType == SpecialType.System_Object
        || (operation is IConversionOperation conversion && IsObjectBridge(operation: conversion.Operand));
    private static bool ContainsUnsupportedFailure(IOperation operation, ITypeParameterSymbol parameter) =>
        operation.DescendantsAndSelf()
            .OfType<IInvocationOperation>()
            .Any(invocation => IsLanguageExtFinFailureInvocation(invocation)
                && invocation.DescendantsAndSelf().OfType<IInvocationOperation>().Any(static nested => nested.TargetMethod.Name == "Unsupported")
                && ContainsTypeof(operation: invocation, parameter: parameter));
    internal static bool IsManualOpAdmissionGate(SemanticModel model, ConditionalExpressionSyntax conditional) =>
        IsNonUnitFinExpression(model: model, expression: conditional)
        && TryFinSuccessValue(model: model, expression: conditional.WhenTrue, value: out ExpressionSyntax? successValue)
        && IsFinInvalidResultFailure(model: model, expression: conditional.WhenFalse)
        && IsKnownValidityProbe(model: model, condition: conditional.Condition, successValue: successValue);
    internal static bool IsManualOpAdmissionGate(SemanticModel model, SwitchExpressionSyntax switchExpression) {
        if (!IsNonUnitFinExpression(model: model, expression: switchExpression) || switchExpression.Arms.Count != 2) {
            return false;
        }
        ImmutableArray<(SwitchExpressionArmSyntax Arm, ExpressionSyntax Value)> successArms = [
            .. switchExpression.Arms
                .Select(arm => (Arm: arm, Success: TryFinSuccessValue(model: model, expression: arm.Expression, value: out ExpressionSyntax? successValue), Value: successValue))
                .Where(static item => item.Success && item.Value is not null)
                .Select(static item => (item.Arm, item.Value!)),
        ];
        ImmutableArray<SwitchExpressionArmSyntax> failureArms = [
            .. switchExpression.Arms
                .Where(arm => IsFinInvalidResultFailure(model: model, expression: arm.Expression)),
        ];
        return successArms.Length == 1
            && failureArms.Length == 1
            && IsKnownSwitchValidityProbe(model: model, switchExpression: switchExpression, arm: successArms[0].Arm, successValue: successArms[0].Value);
    }
    internal static bool IsManualOpConfirmGate(SemanticModel model, ConditionalExpressionSyntax conditional) {
        if (!IsFinUnitExpression(model: model, expression: conditional)) {
            return false;
        }
        bool trueSuccess = IsFinUnitSuccess(model: model, expression: conditional.WhenTrue);
        bool falseSuccess = IsFinUnitSuccess(model: model, expression: conditional.WhenFalse);
        bool trueFailure = IsFinInvalidResultFailure(model: model, expression: conditional.WhenTrue);
        bool falseFailure = IsFinInvalidResultFailure(model: model, expression: conditional.WhenFalse);
        return (trueSuccess, falseFailure, trueFailure, falseSuccess) switch {
            (true, true, _, _) => IsConfirmableNativeStatusCondition(model: model, condition: conditional.Condition, successWhenTrue: true),
            (_, _, true, true) => IsConfirmableNativeStatusCondition(model: model, condition: conditional.Condition, successWhenTrue: false),
            _ => false,
        };
    }
    internal static bool IsManualOpConfirmGate(SemanticModel model, SwitchExpressionSyntax switchExpression) {
        if (!IsFinUnitExpression(model: model, expression: switchExpression) || switchExpression.Arms.Count != 2) {
            return false;
        }
        ImmutableArray<SwitchExpressionArmSyntax> successArms = [
            .. switchExpression.Arms.Where(arm => IsFinUnitSuccess(model: model, expression: arm.Expression)),
        ];
        ImmutableArray<SwitchExpressionArmSyntax> failureArms = [
            .. switchExpression.Arms.Where(arm => IsFinInvalidResultFailure(model: model, expression: arm.Expression)),
        ];
        return successArms.Length == 1
            && failureArms.Length == 1
            && IsConfirmableNativeStatusSwitchArm(model: model, switchExpression: switchExpression, arm: successArms[0]);
    }
    private static ExpressionSyntax UnwrapExpression(ExpressionSyntax expression) =>
        expression switch {
            ParenthesizedExpressionSyntax parenthesized => UnwrapExpression(parenthesized.Expression),
            CastExpressionSyntax cast => UnwrapExpression(cast.Expression),
            _ => expression,
        };
    private static bool IsNonUnitFinExpression(SemanticModel model, ExpressionSyntax expression) =>
        model.GetTypeInfo(expression).ConvertedType switch {
            INamedTypeSymbol fin => IsLanguageExtFinType(fin) && !IsLanguageExtUnitType(fin.TypeArguments[0]),
            _ => false,
        };
    private static bool IsFinUnitExpression(SemanticModel model, ExpressionSyntax expression) =>
        model.GetTypeInfo(expression).ConvertedType switch {
            INamedTypeSymbol fin => IsLanguageExtFinUnitType(fin),
            _ => false,
        };
    private static bool TryFinSuccessValue(SemanticModel model, ExpressionSyntax expression, out ExpressionSyntax value) {
        value = expression;
        InvocationExpressionSyntax? invocation = UnwrapExpression(expression) as InvocationExpressionSyntax;
        bool success = invocation is not null
            && model.GetSymbolInfo(invocation).Symbol is IMethodSymbol method
            && method.Name is "Succ" or "Accept" or "AcceptValue"
            && IsLanguageExtFinType(method.ReturnType)
            && invocation.ArgumentList.Arguments.Count > 0;
        value = success
            ? invocation!.ArgumentList.Arguments
                .FirstOrDefault(argument => argument.NameColon?.Name.Identifier.ValueText == "value")?.Expression
                ?? invocation.ArgumentList.Arguments[0].Expression
            : expression;
        return success;
    }
    private static bool IsFinInvalidResultFailure(SemanticModel model, ExpressionSyntax expression) =>
        UnwrapExpression(expression) is InvocationExpressionSyntax invocation
        && model.GetSymbolInfo(invocation).Symbol is IMethodSymbol { Name: "Fail" } method
        && IsLanguageExtFinType(method.ReturnType)
        && invocation.ArgumentList.Arguments.Any(argument => ContainsPlainInvalidResult(model: model, expression: argument.Expression));
    private static bool IsLanguageExtUnitExpression(SemanticModel model, ExpressionSyntax expression) =>
        (model.GetTypeInfo(UnwrapExpression(expression)).ConvertedType ?? model.GetTypeInfo(UnwrapExpression(expression)).Type) switch {
            ITypeSymbol type => IsLanguageExtUnitType(type),
            _ => false,
        };
    private static bool IsFinUnitSuccess(SemanticModel model, ExpressionSyntax expression) =>
        UnwrapExpression(expression) is InvocationExpressionSyntax invocation
        && model.GetSymbolInfo(invocation).Symbol is IMethodSymbol { Name: "Succ" } method
        && IsLanguageExtFinUnitType(method.ReturnType)
        && invocation.ArgumentList.Arguments.Any(argument =>
            IsLanguageExtUnitExpression(model: model, expression: argument.Expression)
            && !argument.Expression.DescendantNodesAndSelf().OfType<InvocationExpressionSyntax>().Any());
    private static bool ContainsPlainInvalidResult(SemanticModel model, ExpressionSyntax expression) =>
        expression.DescendantNodesAndSelf()
            .OfType<InvocationExpressionSyntax>()
            .Any(invocation => invocation.ArgumentList.Arguments.Count == 0
                && model.GetSymbolInfo(invocation).Symbol is IMethodSymbol { Name: "InvalidResult" });
    internal static bool ContainsPlainInvalidResult(IOperation operation) =>
        operation.DescendantsAndSelf()
            .OfType<IInvocationOperation>()
            .Any(static invocation => invocation.TargetMethod.Name == "InvalidResult" && invocation.Arguments.Length == 0);
    private static bool SameReference(SemanticModel model, ExpressionSyntax left, ExpressionSyntax right) =>
        (ReferencedSymbol(model: model, expression: left), ReferencedSymbol(model: model, expression: right)) switch {
            (ISymbol a, ISymbol b) => SymbolEqualityComparer.Default.Equals(a, b),
            _ => string.Equals(
                a: UnwrapExpression(left).ToString(),
                b: UnwrapExpression(right).ToString(),
                comparisonType: StringComparison.Ordinal),
        };
    private static ISymbol? ReferencedSymbol(SemanticModel model, ExpressionSyntax expression) =>
        model.GetOperation(UnwrapExpression(expression)) switch {
            ILocalReferenceOperation local => local.Local,
            IParameterReferenceOperation parameter => parameter.Parameter,
            IFieldReferenceOperation field => field.Field,
            IPropertyReferenceOperation property => property.Property,
            _ => model.GetSymbolInfo(UnwrapExpression(expression)).Symbol,
        };
    private static bool IsGuidEmpty(SemanticModel model, ExpressionSyntax expression) =>
        model.GetSymbolInfo(UnwrapExpression(expression)).Symbol switch {
            IFieldSymbol { Name: "Empty", ContainingType: INamedTypeSymbol type } when IsGuidType(type) => true,
            IPropertySymbol { Name: "Empty", ContainingType: INamedTypeSymbol type } when IsGuidType(type) => true,
            _ => false,
        };
    private static bool IsGuidType(INamedTypeSymbol type) =>
        type.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat) == "global::System.Guid";
    private static bool IsKnownValidityProbe(SemanticModel model, ExpressionSyntax condition, ExpressionSyntax successValue) {
        ExpressionSyntax probe = UnwrapExpression(condition);
        return probe switch {
            MemberAccessExpressionSyntax member when member.Name.Identifier.ValueText == "IsValid"
                => IsOpAcceptValueAdmissibleType(model: model, expression: successValue)
                    && SameReference(model: model, left: member.Expression, right: successValue),
            InvocationExpressionSyntax invocation when model.GetSymbolInfo(invocation).Symbol is IMethodSymbol { Name: "IsValidDouble" }
                => IsFiniteScalarType(model: model, expression: successValue)
                    && invocation.ArgumentList.Arguments.Any(argument => SameReference(model: model, left: argument.Expression, right: successValue)),
            BinaryExpressionSyntax binary when binary.IsKind(SyntaxKind.NotEqualsExpression)
                => IsGuidExpression(model: model, expression: successValue)
                    && ((SameReference(model: model, left: binary.Left, right: successValue) && IsGuidEmpty(model: model, expression: binary.Right))
                    || (SameReference(model: model, left: binary.Right, right: successValue) && IsGuidEmpty(model: model, expression: binary.Left))),
            _ => false,
        };
    }
    private static bool IsConfirmableNativeStatusCondition(SemanticModel model, ExpressionSyntax condition, bool successWhenTrue) {
        ExpressionSyntax probe = UnwrapExpression(condition);
        return probe switch {
            InvocationExpressionSyntax invocation when IsBooleanExpression(model: model, expression: invocation)
                => true,
            BinaryExpressionSyntax binary when successWhenTrue
                => IsConfirmableNativeStatusBinary(model: model, binary: binary),
            _ => false,
        };
    }
    private static bool IsKnownSwitchValidityProbe(SemanticModel model, SwitchExpressionSyntax switchExpression, SwitchExpressionArmSyntax arm, ExpressionSyntax successValue) =>
        arm.WhenClause?.Condition is ExpressionSyntax condition
            ? IsKnownValidityProbe(model: model, condition: condition, successValue: successValue)
            : IsKnownIsValidPattern(model: model, pattern: arm.Pattern, successValue: successValue)
                || IsKnownSwitchInputPattern(model: model, switchExpression: switchExpression, arm: arm, successValue: successValue);
    private static bool IsConfirmableNativeStatusSwitchArm(SemanticModel model, SwitchExpressionSyntax switchExpression, SwitchExpressionArmSyntax arm) {
        ExpressionSyntax governing = UnwrapExpression(switchExpression.GoverningExpression);
        return (
            model.GetTypeInfo(governing).ConvertedType ?? model.GetTypeInfo(governing).Type,
            arm.Pattern,
            arm.WhenClause?.Condition) switch {
                ( { SpecialType: SpecialType.System_Boolean },
                    ConstantPatternSyntax { Expression: LiteralExpressionSyntax literal },
                    null)
                    => literal.IsKind(SyntaxKind.TrueLiteralExpression),
                ( { SpecialType: SpecialType.System_Int32 },
                    RelationalPatternSyntax { OperatorToken.RawKind: (int)SyntaxKind.GreaterThanEqualsToken, Expression: LiteralExpressionSyntax literal },
                    null)
                    => literal.Token.Value is 0,
                ( { SpecialType: SpecialType.System_Int32 },
                    DeclarationPatternSyntax declaration,
                    ExpressionSyntax condition)
                    => PatternDesignationName(declaration.Designation) is string designation && IsCountEqualityCondition(model: model, designation: designation, condition: condition),
                (
                    INamedTypeSymbol type,
                    DeclarationPatternSyntax declaration,
                    ExpressionSyntax condition) when IsGuidType(type)
                    => PatternDesignationName(declaration.Designation) is string designation && IsGuidNonEmptyCondition(model: model, designation: designation, condition: condition),
                _ => false,
            };
    }
    private static bool IsConfirmableNativeStatusBinary(SemanticModel model, BinaryExpressionSyntax binary) =>
        binary.Kind() switch {
            SyntaxKind.GreaterThanOrEqualExpression => IsZeroLiteral(binary.Right) && IsIntExpression(model: model, expression: binary.Left),
            SyntaxKind.EqualsExpression => IsCountEqualityCondition(model: model, designation: null, condition: binary),
            SyntaxKind.NotEqualsExpression => IsGuidNonEmptyCondition(model: model, designation: null, condition: binary),
            _ => false,
        };
    private static bool IsCountEqualityCondition(SemanticModel model, string? designation, ExpressionSyntax condition) =>
        UnwrapExpression(condition) is BinaryExpressionSyntax binary
        && binary.IsKind(SyntaxKind.EqualsExpression)
        && ((MatchesDesignationOrInt(model: model, expression: binary.Left, designation: designation) && !SameReference(model: model, left: binary.Left, right: binary.Right))
            || (MatchesDesignationOrInt(model: model, expression: binary.Right, designation: designation) && !SameReference(model: model, left: binary.Left, right: binary.Right)));
    private static bool IsGuidNonEmptyCondition(SemanticModel model, string? designation, ExpressionSyntax condition) =>
        UnwrapExpression(condition) is BinaryExpressionSyntax binary
        && binary.IsKind(SyntaxKind.NotEqualsExpression)
        && ((MatchesDesignationOrGuid(model: model, expression: binary.Left, designation: designation) && IsGuidEmpty(model: model, expression: binary.Right))
            || (MatchesDesignationOrGuid(model: model, expression: binary.Right, designation: designation) && IsGuidEmpty(model: model, expression: binary.Left)));
    private static bool MatchesDesignationOrInt(SemanticModel model, ExpressionSyntax expression, string? designation) =>
        designation switch {
            string name => IdentifierName(expression) == name,
            _ => IsIntExpression(model: model, expression: expression),
        };
    private static bool MatchesDesignationOrGuid(SemanticModel model, ExpressionSyntax expression, string? designation) =>
        designation switch {
            string name => IdentifierName(expression) == name,
            _ => IsGuidExpression(model: model, expression: expression),
        };
    private static bool IsBooleanExpression(SemanticModel model, ExpressionSyntax expression) =>
        (model.GetTypeInfo(UnwrapExpression(expression)).ConvertedType ?? model.GetTypeInfo(UnwrapExpression(expression)).Type)?.SpecialType == SpecialType.System_Boolean;
    private static bool IsIntExpression(SemanticModel model, ExpressionSyntax expression) =>
        (model.GetTypeInfo(UnwrapExpression(expression)).ConvertedType ?? model.GetTypeInfo(UnwrapExpression(expression)).Type)?.SpecialType == SpecialType.System_Int32;
    private static bool IsZeroLiteral(ExpressionSyntax expression) =>
        UnwrapExpression(expression) is LiteralExpressionSyntax literal && literal.Token.Value is 0;
    private static bool IsKnownIsValidPattern(SemanticModel model, PatternSyntax pattern, ExpressionSyntax successValue) =>
        pattern is RecursivePatternSyntax recursive
        && PatternDesignationName(recursive.Designation) is string designation
        && IdentifierName(successValue) == designation
        && IsOpAcceptValueAdmissibleType(model: model, expression: successValue)
        && recursive.PropertyPatternClause?.Subpatterns.Any(IsIsValidTrueSubpattern) == true;
    private static bool IsKnownSwitchInputPattern(SemanticModel model, SwitchExpressionSyntax switchExpression, SwitchExpressionArmSyntax arm, ExpressionSyntax successValue) =>
        SameReference(model: model, left: switchExpression.GoverningExpression, right: successValue)
        && IsOpAcceptValueAdmissibleType(model: model, expression: successValue)
        && arm.Pattern is RecursivePatternSyntax recursive
        && recursive.PropertyPatternClause?.Subpatterns.Any(IsIsValidTrueSubpattern) == true;
    private static bool IsIsValidTrueSubpattern(SubpatternSyntax subpattern) =>
        subpattern.NameColon?.Name.Identifier.ValueText == "IsValid"
        && subpattern.Pattern is ConstantPatternSyntax { Expression: LiteralExpressionSyntax literal }
        && literal.IsKind(SyntaxKind.TrueLiteralExpression);
    private static string? PatternDesignationName(VariableDesignationSyntax? designation) =>
        designation switch {
            SingleVariableDesignationSyntax single => single.Identifier.ValueText,
            _ => null,
        };
    private static string? IdentifierName(ExpressionSyntax expression) =>
        UnwrapExpression(expression) switch {
            IdentifierNameSyntax identifier => identifier.Identifier.ValueText,
            _ => null,
        };
    private static bool IsOpAcceptValueAdmissibleType(SemanticModel model, ExpressionSyntax expression) {
        TypeInfo info = model.GetTypeInfo(UnwrapExpression(expression));
        return (info.ConvertedType ?? info.Type) switch {
            ITypeSymbol type => IsOpAcceptValueAdmissibleType(type),
            _ => false,
        };
    }
    private static bool IsOpAcceptValueAdmissibleType(ITypeSymbol type) =>
        type.SpecialType is SpecialType.System_String or SpecialType.System_Double or SpecialType.System_Single or SpecialType.System_Int32 or SpecialType.System_Boolean
        || type.TypeKind == TypeKind.Enum
        || (type is INamedTypeSymbol named && (IsGuidType(named) || IsGeometryBaseOrAcceptedIsValidType(named)));
    private static bool IsGeometryBaseOrAcceptedIsValidType(INamedTypeSymbol type) =>
        AcceptValueIsValidTypeNames.Contains(type.Name)
        || type.AllInterfaces.Any(static candidate => candidate.OriginalDefinition.Name == "ISmartEnum")
        || TypeAndBases(type).Any(static candidate => candidate.Name == "GeometryBase");
    private static bool IsFiniteScalarType(SemanticModel model, ExpressionSyntax expression) =>
        (model.GetTypeInfo(UnwrapExpression(expression)).ConvertedType ?? model.GetTypeInfo(UnwrapExpression(expression)).Type)?.SpecialType
            is SpecialType.System_Double or SpecialType.System_Single;
    private static bool IsGuidExpression(SemanticModel model, ExpressionSyntax expression) {
        TypeInfo info = model.GetTypeInfo(UnwrapExpression(expression));
        return (info.ConvertedType ?? info.Type) switch {
            INamedTypeSymbol type => IsGuidType(type),
            _ => false,
        };
    }
    private static readonly HashSet<(string From, string To)> NarrowingCasts = [
        ("Int64", "Int32"), ("Int64", "Int16"), ("Int64", "Byte"), ("Int64", "SByte"),
        ("Int32", "Int16"), ("Int32", "Byte"), ("Int32", "SByte"), ("Double", "Single"),
        ("Double", "Decimal"), ("Decimal", "Single"), ("Decimal", "Double"), ("Single", "Decimal"),
        ("UInt64", "UInt32"), ("UInt64", "UInt16"), ("UInt64", "Byte"), ("UInt32", "UInt16"), ("UInt32", "Byte")];
    internal static bool IsNarrowingNumericCast(ITypeSymbol fromType, ITypeSymbol toType) =>
        NarrowingCasts.Contains((fromType.MetadataName, toType.MetadataName));
    internal static bool HasCreateFactory(INamedTypeSymbol namedType) =>
        namedType.GetMembers().OfType<IMethodSymbol>().Any(method => method.IsStatic && method.Name is "Create" or "CreateK");
    internal static bool IsAtomOrRefType(ITypeSymbol type) =>
        type.OriginalDefinition.MetadataName is "Atom`1" or "Ref`1"
        && (type.ContainingNamespace?.ToDisplayString() ?? string.Empty)
            .StartsWith(value: Markers.LanguageExtNamespace, comparisonType: StringComparison.Ordinal);
    internal static bool IsDateTimeType(ITypeSymbol type) =>
        type.OriginalDefinition.MetadataName is "DateTime" or "DateTimeOffset";
    internal static ITypeSymbol UnwrapNullable(ITypeSymbol type) =>
        type is INamedTypeSymbol { OriginalDefinition.SpecialType: SpecialType.System_Nullable_T } nullableType
            ? nullableType.TypeArguments[0]
            : type;
    internal static bool IsOperationalReceiptType(ITypeSymbol? type) =>
        UnwrapNamed(type) is INamedTypeSymbol receipt
        && receipt.Name.EndsWith(value: "Receipt", comparisonType: StringComparison.Ordinal)
        && !IsProofReceiptType(receipt)
        && OperationalReceiptMemberScore(receipt) > 0;
    internal static bool HasReceiptFactStream(INamedTypeSymbol receipt) =>
        receipt.GetTypeMembers().Any(static member => member.Name == "Change")
        && receipt.GetMembers()
            .OfType<IFieldSymbol>()
            .Any(static field =>
                field.Name.Contains(value: "changes", comparisonType: StringComparison.OrdinalIgnoreCase)
                && field.Type.ToDisplayString().Contains(value: "Seq<", comparisonType: StringComparison.Ordinal));
    internal static int OperationalReceiptMemberScore(INamedTypeSymbol receipt) =>
        receipt.GetMembers()
            .Where(static member => member is IFieldSymbol or IPropertySymbol)
            .Where(static member => MemberTypeName(member) is not "bool" and not "Boolean" and not "System.Boolean")
            .Select(static member => ReceiptMemberName(name: member.Name))
            .Where(static name => ContainsAnyFragment(name: name, fragments: OperationalReceiptNames))
            .Distinct(StringComparer.Ordinal)
            .Count();
    internal static string MemberTypeName(ISymbol member) =>
        member switch {
            IFieldSymbol field => field.Type.ToDisplayString(SymbolDisplayFormat.MinimallyQualifiedFormat),
            IPropertySymbol property => property.Type.ToDisplayString(SymbolDisplayFormat.MinimallyQualifiedFormat),
            _ => string.Empty,
        };
    internal static bool IsOperationalReceiptFactoryTerm(IOperation operation) =>
        UnwrapReceiver(operation) switch {
            IInvocationOperation invocation => IsOperationalReceiptType(invocation.TargetMethod.ReturnType),
            IConversionOperation { Operand: IOperation operand } => IsOperationalReceiptFactoryTerm(operand),
            _ => false,
        };
    internal static bool IsMutationReceiptDocumentWrapper(IInvocationOperation invocation) =>
        invocation.TargetMethod is {
            Name: "Of",
            ContainingType.Name: "MutationReceipt",
            Parameters.Length: > 0,
        }
        && invocation.Arguments.Any(static argument => IsDocumentReceiptFactoryTerm(argument.Value));
    private static INamedTypeSymbol? UnwrapNamed(ITypeSymbol? type) =>
        type switch {
            INamedTypeSymbol named => UnwrapNullable(named) as INamedTypeSymbol,
            _ => null,
        };
    private static string ReceiptMemberName(string name) {
        int open = name.StartsWith(value: "<", comparisonType: StringComparison.Ordinal) ? 1 : -1;
        int close = open > 0 ? name.IndexOf('>', startIndex: open) : -1;
        return close > open ? name.Substring(startIndex: open, length: close - open) : name;
    }
    private static bool IsProofReceiptType(INamedTypeSymbol receipt) =>
        receipt.GetMembers()
            .Where(static member => member is IFieldSymbol or IPropertySymbol)
            .Count(static member =>
                ContainsAnyFragment(name: member.Name, fragments: ProofReceiptFragments)
                || ContainsAnyFragment(name: MemberTypeName(member), fragments: ProofReceiptFragments)) >= 2;
    private static bool ContainsAnyFragment(string name, HashSet<string> fragments) =>
        fragments.Any(fragment => name.Contains(value: fragment, comparisonType: StringComparison.Ordinal));
    private static bool IsDocumentReceiptFactoryTerm(IOperation operation) =>
        UnwrapReceiver(operation) switch {
            IInvocationOperation invocation => invocation.TargetMethod is {
                ContainingType.Name: "DocumentReceipt",
                ReturnType: INamedTypeSymbol { Name: "DocumentReceipt" },
            },
            IConversionOperation { Operand: IOperation operand } => IsDocumentReceiptFactoryTerm(operand),
            _ => false,
        };
    internal static bool HasThinktectureGeneratedDispatch(INamedTypeSymbol? type) =>
        type is not null && HasAnyAttribute(type, "UnionAttribute", "Union", "SmartEnumAttribute", "SmartEnum");
    internal static ImmutableArray<INamedTypeSymbol> ClosedUnionCases(Compilation compilation, INamedTypeSymbol namedType) =>
        [
            .. namedType.GetTypeMembers()
                .Where(caseType => SymbolEqualityComparer.Default.Equals(caseType.BaseType, namedType))
                .Concat(compilation.GlobalNamespace.GetNamespaceMembers().SelectMany(namespaceSymbol => ClosedUnionCases(namespaceSymbol, namedType))),
        ];
    private static IEnumerable<INamedTypeSymbol> ClosedUnionCases(INamespaceSymbol namespaceSymbol, INamedTypeSymbol namedType) =>
        namespaceSymbol.GetTypeMembers()
            .Where(caseType => SymbolEqualityComparer.Default.Equals(caseType.BaseType, namedType))
            .Concat(namespaceSymbol.GetNamespaceMembers().SelectMany(childNamespace => ClosedUnionCases(childNamespace, namedType)));
    internal static bool HasSamePayloadUnionCaseSurface(INamedTypeSymbol namedType, ImmutableArray<INamedTypeSymbol> unionCases, out int caseCount) {
        ImmutableArray<string> shapes = [
            .. unionCases
                .Where(static caseType => caseType.IsRecord && caseType.IsSealed)
                .Where(IsPassivePayloadCase)
                .Select(PayloadShape)
                .Where(static shape => shape.Length > 0),
        ];
        bool candidate = namedType.IsRecord
            && !namedType.IsGenericType
            && !IsErrorLike(namedType)
            && (namedType.IsAbstract || HasAnyAttribute(namedType, "UnionAttribute", "Union"))
            && shapes.Length >= 3;
        caseCount = shapes
            .GroupBy(static shape => shape, StringComparer.Ordinal)
            .Select(static group => group.Count())
            .DefaultIfEmpty(defaultValue: 0)
            .Max();
        return candidate
            && caseCount >= 3;
    }
    private static IMethodSymbol? PrimaryConstructor(INamedTypeSymbol type) =>
        type.InstanceConstructors
            .Where(static constructor => !constructor.IsImplicitlyDeclared)
            .Where(constructor => !IsRecordCopyConstructor(type: type, constructor: constructor))
            .OrderByDescending(static constructor => constructor.Parameters.Length)
            .FirstOrDefault();
    private static string PayloadShape(INamedTypeSymbol caseType) =>
        PrimaryConstructor(caseType) switch {
            { Parameters.Length: > 0 } constructor => string.Join(separator: "|", values: constructor.Parameters.Select(static parameter =>
                $"{parameter.Name}:{parameter.Type.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat)}")),
            _ => string.Empty,
        };
    private static bool IsPassivePayloadCase(INamedTypeSymbol caseType) =>
        PrimaryConstructor(caseType) is IMethodSymbol constructor
        && caseType.GetMembers()
            .Where(static member => !member.IsImplicitlyDeclared)
            .Where(static member => member is not IMethodSymbol { MethodKind: MethodKind.Constructor })
            .All(member => IsPayloadProjectionMember(member: member, parameters: constructor.Parameters));
    private static bool IsPayloadProjectionMember(ISymbol member, ImmutableArray<IParameterSymbol> parameters) =>
        member switch {
            IFieldSymbol { AssociatedSymbol: IPropertySymbol property } => IsPayloadProjectionMember(member: property, parameters: parameters),
            IPropertySymbol { Name: "EqualityContract" } => true,
            IPropertySymbol property => parameters.Any(parameter =>
                string.Equals(a: parameter.Name, b: property.Name, comparisonType: StringComparison.OrdinalIgnoreCase)
                && SymbolEqualityComparer.Default.Equals(parameter.Type, property.Type)),
            IMethodSymbol { MethodKind: MethodKind.PropertyGet or MethodKind.PropertySet } => true,
            IMethodSymbol { IsOverride: true } => true,
            IMethodSymbol { Name: "Deconstruct" or "PrintMembers" } => true,
            _ => false,
        };
    private static bool IsRecordCopyConstructor(INamedTypeSymbol type, IMethodSymbol constructor) =>
        constructor.Parameters.Length == 1
        && SymbolEqualityComparer.Default.Equals(constructor.Parameters[0].Type, type);
    private static bool IsErrorLike(INamedTypeSymbol type) =>
        type.Name is "Error" or "Expected"
        || (type.BaseType is INamedTypeSymbol parent && IsErrorLike(parent));
    internal static bool HasExclusiveOptionalPayloadBag(Compilation compilation, INamedTypeSymbol namedType, out int slotCount, out int projectionWidth) {
        slotCount = 0;
        projectionWidth = 0;
        if (IsProofOrProjectionContainer(namedType) || HasAdditiveOwnerSemantics(namedType)) {
            return false;
        }
        ImmutableArray<IPropertySymbol> slots = [
            .. namedType.GetMembers()
                .OfType<IPropertySymbol>()
                .Where(static property => !property.IsStatic && property.Parameters.Length == 0)
                .Where(property => IsPrimaryConstructorProperty(namedType: namedType, property: property))
                .Where(static property => TryOptionPayload(property.Type, out ITypeSymbol? payload) && IsComplexPayload(payload)),
        ];
        slotCount = slots.Length;
        return slots.Length >= 3
            && HasExclusivePayloadDiscriminator(namedType: namedType)
            && HasCrossSlotProjection(compilation: compilation, namedType: namedType, slots: slots, width: out projectionWidth)
            && projectionWidth >= 3;
    }
    private static bool TryOptionPayload(ITypeSymbol type, out ITypeSymbol payload) {
        bool option = type is INamedTypeSymbol {
            OriginalDefinition.MetadataName: "Option`1",
            TypeArguments.Length: 1,
        } optionType
            && (optionType.ContainingNamespace?.ToDisplayString() ?? string.Empty)
                .StartsWith(value: Markers.LanguageExtNamespace, comparisonType: StringComparison.Ordinal);
        payload = option && type is INamedTypeSymbol named ? named.TypeArguments[0] : type;
        return option;
    }
    private static bool IsComplexPayload(ITypeSymbol type) =>
        type.TypeKind is TypeKind.Class or TypeKind.Interface
        && type.SpecialType == SpecialType.None
        && type.Name is not "String"
        && type.Name is not "Object"
        && !type.Name.EndsWith(value: "Receipt", comparisonType: StringComparison.Ordinal);
    private static bool IsProofOrProjectionContainer(INamedTypeSymbol namedType) =>
        namedType.Name.EndsWith(value: "Receipt", comparisonType: StringComparison.Ordinal)
        || namedType.Name.EndsWith(value: "Shape", comparisonType: StringComparison.Ordinal)
        || namedType.Name.EndsWith(value: "Hit", comparisonType: StringComparison.Ordinal)
        || namedType.Name.EndsWith(value: "Snapshot", comparisonType: StringComparison.Ordinal);
    private static bool HasExclusivePayloadDiscriminator(INamedTypeSymbol namedType) =>
        namedType.GetMembers()
            .OfType<IPropertySymbol>()
            .Where(static property => !property.IsStatic && property.Parameters.Length == 0)
            .Where(property => IsPrimaryConstructorProperty(namedType: namedType, property: property))
            .Any(static property => IsPayloadDiscriminatorName(name: property.Name) && !TryOptionPayload(type: property.Type, payload: out _));
    private static bool IsPayloadDiscriminatorName(string name) =>
        name is "Kind" or "Phase" or "Mode" or "State" or "Action";
    private static bool HasAdditiveOwnerSemantics(INamedTypeSymbol namedType) =>
        namedType.GetMembers()
            .OfType<IMethodSymbol>()
            .Any(method => SymbolEqualityComparer.Default.Equals(method.ReturnType, namedType)
                && ((method.MethodKind == MethodKind.UserDefinedOperator && method.Name is "op_Addition" or "op_BitwiseOr")
                    || (method.IsStatic && method.Name is "Add" or "Merge" or "Combine")));
    private static bool IsPrimaryConstructorProperty(INamedTypeSymbol namedType, IPropertySymbol property) =>
        PrimaryConstructor(namedType) is IMethodSymbol constructor
        && constructor.Parameters.Any(parameter =>
            parameter.Name == property.Name
            && SymbolEqualityComparer.Default.Equals(parameter.Type, property.Type));
    private static bool HasCrossSlotProjection(Compilation compilation, INamedTypeSymbol namedType, ImmutableArray<IPropertySymbol> slots, out int width) {
        width = namedType.DeclaringSyntaxReferences
            .Select(reference => (Reference: reference, Syntax: reference.GetSyntax()))
            .GroupBy(static item => item.Reference.SyntaxTree)
            .SelectMany(group => {
                SemanticModel model = compilation.GetSemanticModel(group.Key);
                return group.SelectMany(item => item.Syntax.DescendantNodes().OfType<MemberDeclarationSyntax>())
                    .Where(member => HasOptionReturn(model: model, member: member))
                    .SelectMany(member => ProjectionOperations(model: model, member: member));
            })
            .Select(operation => ReferencedSlotCount(operation: operation, slots: slots))
            .DefaultIfEmpty(defaultValue: 0)
            .Max();
        return width >= 3;
    }
    private static bool HasOptionReturn(SemanticModel model, MemberDeclarationSyntax member) =>
        member switch {
            PropertyDeclarationSyntax property => IsOptionType(model: model, syntax: property.Type),
            MethodDeclarationSyntax method => IsOptionType(model: model, syntax: method.ReturnType),
            _ => false,
        };
    private static bool IsOptionType(SemanticModel model, TypeSyntax syntax) =>
        model.GetTypeInfo(syntax).Type is ITypeSymbol type && TryOptionPayload(type: type, payload: out _);
    private static IEnumerable<IOperation> ProjectionOperations(SemanticModel model, MemberDeclarationSyntax member) {
        IEnumerable<ExpressionSyntax> expressions = member switch {
            PropertyDeclarationSyntax { ExpressionBody.Expression: ExpressionSyntax expression } => [expression],
            MethodDeclarationSyntax { ExpressionBody.Expression: ExpressionSyntax expression } => [expression],
            _ => member.DescendantNodes()
                .OfType<ReturnStatementSyntax>()
                .Select(static statement => statement.Expression)
                .OfType<ExpressionSyntax>(),
        };
        return expressions.Select(expression => model.GetOperation(expression)).OfType<IOperation>();
    }
    private static int ReferencedSlotCount(IOperation operation, ImmutableArray<IPropertySymbol> slots) =>
        operation.DescendantsAndSelf()
            .OfType<IPropertyReferenceOperation>()
            .Select(property => property.Property)
            .Where(property => slots.Any(slot => SymbolEqualityComparer.Default.Equals(slot, property)))
            .Distinct<IPropertySymbol>(SymbolEqualityComparer.Default)
            .Count();
    internal static bool TryManualClosedUnionOverride(INamedTypeSymbol namedType, ImmutableHashSet<INamedTypeSymbol> derivedTypes, out ManualClosedUnionOverrideFacts facts) {
        facts = default;
        ImmutableArray<INamedTypeSymbol> nestedCases = [
            .. namedType.GetTypeMembers()
                .Where(caseType => IsManualUnionCase(baseType: namedType, caseType: caseType)),
        ];
        bool closedByInheritance = derivedTypes.Count > 0
            && derivedTypes.All(derived => nestedCases.Any(caseType => SymbolEqualityComparer.Default.Equals(caseType.OriginalDefinition, derived.OriginalDefinition)));
        bool candidate = namedType is { TypeKind: TypeKind.Class, IsAbstract: true }
            && !HasThinktectureGeneratedDispatch(namedType)
            && (HasPrivateConstructorBarrier(namedType) || (closedByInheritance && !IsExternallyVisible(namedType)))
            && nestedCases.Length >= 3;
        if (!candidate) {
            return false;
        }
        ImmutableArray<ISymbol> dispatchMembers = [
            .. ManualDispatchMembers(namedType)
                .Where(member => nestedCases.All(caseType =>
                    MatchingOverride(caseType: caseType, baseMember: member) is ISymbol overrideMember
                    && IsSimpleDispatchOverride(overrideMember))),
        ];
        if (dispatchMembers.Length == 0 || !nestedCases.All(caseType => HasNoExtraManualBehavior(caseType: caseType, dispatchMembers: dispatchMembers))) {
            return false;
        }
        facts = new ManualClosedUnionOverrideFacts(
            memberName: DescribeDispatchMembers(dispatchMembers),
            overrideCount: nestedCases.Length * dispatchMembers.Length);
        return true;
    }
    private static bool HasPrivateConstructorBarrier(INamedTypeSymbol namedType) =>
        namedType.InstanceConstructors
            .Where(constructor => !constructor.IsImplicitlyDeclared)
            .Where(constructor => !IsRecordCopyConstructor(type: namedType, constructor: constructor))
            .Any(static constructor => constructor.DeclaredAccessibility == Accessibility.Private)
        && !namedType.InstanceConstructors
            .Where(constructor => !constructor.IsImplicitlyDeclared)
            .Where(constructor => !IsRecordCopyConstructor(type: namedType, constructor: constructor))
            .Any(static constructor => constructor.DeclaredAccessibility
                is Accessibility.Public
                or Accessibility.Protected
                or Accessibility.ProtectedOrInternal
                or Accessibility.ProtectedAndInternal);
    private static bool IsExternallyVisible(INamedTypeSymbol namedType) =>
        namedType.DeclaredAccessibility is Accessibility.Public or Accessibility.Protected or Accessibility.ProtectedOrInternal
        && (namedType.ContainingType is not INamedTypeSymbol containing || IsExternallyVisible(containing));
    private static bool IsManualUnionCase(INamedTypeSymbol baseType, INamedTypeSymbol caseType) =>
        caseType is { TypeKind: TypeKind.Class, IsRecord: true, IsSealed: true, IsGenericType: false }
        && caseType.BaseType is INamedTypeSymbol parent
        && SymbolEqualityComparer.Default.Equals(parent.OriginalDefinition, baseType.OriginalDefinition);
    private static IEnumerable<ISymbol> ManualDispatchMembers(INamedTypeSymbol namedType) =>
        namedType.GetMembers()
            .Where(static member => member switch {
                IPropertySymbol property => property is { IsStatic: false, IsAbstract: true },
                IMethodSymbol method => method is {
                    IsStatic: false,
                    IsAbstract: true,
                    MethodKind: MethodKind.Ordinary,
                    IsImplicitlyDeclared: false,
                },
                _ => false,
            });
    private static ISymbol? MatchingOverride(INamedTypeSymbol caseType, ISymbol baseMember) =>
        baseMember switch {
            IPropertySymbol property => caseType.GetMembers(property.Name)
                .OfType<IPropertySymbol>()
                .FirstOrDefault(candidate => candidate.IsOverride
                    && SymbolEqualityComparer.Default.Equals(candidate.OverriddenProperty?.OriginalDefinition, property.OriginalDefinition)),
            IMethodSymbol method => caseType.GetMembers(method.Name)
                .OfType<IMethodSymbol>()
                .FirstOrDefault(candidate => candidate.IsOverride
                    && SymbolEqualityComparer.Default.Equals(candidate.OverriddenMethod?.OriginalDefinition, method.OriginalDefinition)),
            _ => null,
        };
    private static bool HasNoExtraManualBehavior(INamedTypeSymbol caseType, ImmutableArray<ISymbol> dispatchMembers) =>
        caseType.GetMembers()
            .Where(static member => !member.IsImplicitlyDeclared)
            .All(member => IsAllowedManualUnionCaseMember(member: member, parameters: PrimaryConstructor(caseType)?.Parameters ?? [], dispatchMembers: dispatchMembers));
    private static bool IsAllowedManualUnionCaseMember(ISymbol member, ImmutableArray<IParameterSymbol> parameters, ImmutableArray<ISymbol> dispatchMembers) =>
        member switch {
            IFieldSymbol { AssociatedSymbol: IPropertySymbol property } => IsAllowedManualUnionCaseMember(member: property, parameters: parameters, dispatchMembers: dispatchMembers),
            IPropertySymbol { Name: "EqualityContract" } => true,
            IPropertySymbol property when MatchingBaseMember(dispatchMembers: dispatchMembers, overrideMember: property) is not null => IsSimpleDispatchOverride(property),
            IPropertySymbol property => parameters.Any(parameter =>
                string.Equals(a: parameter.Name, b: property.Name, comparisonType: StringComparison.OrdinalIgnoreCase)
                && SymbolEqualityComparer.Default.Equals(parameter.Type, property.Type)),
            IMethodSymbol { MethodKind: MethodKind.PropertyGet or MethodKind.PropertySet } method => method.AssociatedSymbol is IPropertySymbol property
                && IsAllowedManualUnionCaseMember(member: property, parameters: parameters, dispatchMembers: dispatchMembers),
            IMethodSymbol { MethodKind: MethodKind.Constructor } => true,
            IMethodSymbol method when MatchingBaseMember(dispatchMembers: dispatchMembers, overrideMember: method) is not null => IsSimpleDispatchOverride(method),
            IMethodSymbol { Name: "Deconstruct" or "PrintMembers" } => true,
            _ => false,
        };
    internal static bool TryForwardingRequestCaseFamily(Compilation compilation, INamedTypeSymbol namedType, out ForwardingRequestCaseFamilyFacts facts) {
        facts = default;
        ImmutableArray<INamedTypeSymbol> nestedCases = [
            .. namedType.GetTypeMembers()
                .Where(caseType => IsForwardingRequestCase(baseType: namedType, caseType: caseType)),
        ];
        if (namedType is not { TypeKind: TypeKind.Class, IsAbstract: true, IsGenericType: true }
            || HasThinktectureGeneratedDispatch(namedType)
            || nestedCases.Length < 3) {
            return false;
        }
        ImmutableArray<IMethodSymbol> abstractMethods = [
            .. namedType.GetMembers()
                .OfType<IMethodSymbol>()
                .Where(static method => method is {
                    IsStatic: false,
                    IsAbstract: true,
                    MethodKind: MethodKind.Ordinary,
                    Parameters.Length: 1,
                })
                .Where(method => IsLanguageExtFinType(method.ReturnType)),
        ];
        bool forwards = abstractMethods.Any(method => nestedCases.All(caseType =>
            MatchingOverride(caseType: caseType, baseMember: method) is IMethodSymbol overrideMethod
            && IsTerminalRunForwarder(compilation: compilation, method: overrideMethod)
            && HasNoExtraForwardingCaseBehavior(caseType: caseType, dispatchMethod: method)));
        if (!forwards) {
            return false;
        }
        facts = new ForwardingRequestCaseFamilyFacts(caseCount: nestedCases.Length);
        return true;
    }
    private static bool IsForwardingRequestCase(INamedTypeSymbol baseType, INamedTypeSymbol caseType) =>
        caseType is { TypeKind: TypeKind.Class, IsSealed: true }
        && caseType.BaseType is INamedTypeSymbol parent
        && SymbolEqualityComparer.Default.Equals(parent.OriginalDefinition, baseType.OriginalDefinition);
    private static bool HasNoExtraForwardingCaseBehavior(INamedTypeSymbol caseType, IMethodSymbol dispatchMethod) =>
        caseType.GetMembers()
            .Where(static member => !member.IsImplicitlyDeclared)
            .All(member => IsAllowedForwardingCaseMember(member: member, parameters: PrimaryConstructor(caseType)?.Parameters ?? [], dispatchMethod: dispatchMethod));
    private static bool IsAllowedForwardingCaseMember(ISymbol member, ImmutableArray<IParameterSymbol> parameters, IMethodSymbol dispatchMethod) =>
        member switch {
            IFieldSymbol { AssociatedSymbol: IPropertySymbol property } => IsAllowedForwardingCaseMember(member: property, parameters: parameters, dispatchMethod: dispatchMethod),
            IPropertySymbol { Name: "EqualityContract" } => true,
            IPropertySymbol property when property.IsOverride => IsSimpleDispatchOverride(property),
            IPropertySymbol property => parameters.Any(parameter =>
                string.Equals(a: parameter.Name, b: property.Name, comparisonType: StringComparison.OrdinalIgnoreCase)
                && SymbolEqualityComparer.Default.Equals(parameter.Type, property.Type)),
            IMethodSymbol { MethodKind: MethodKind.PropertyGet or MethodKind.PropertySet } method => method.AssociatedSymbol is IPropertySymbol property
                && IsAllowedForwardingCaseMember(member: property, parameters: parameters, dispatchMethod: dispatchMethod),
            IMethodSymbol { MethodKind: MethodKind.Constructor } => true,
            IMethodSymbol method when SymbolEqualityComparer.Default.Equals(method.OverriddenMethod?.OriginalDefinition, dispatchMethod.OriginalDefinition)
                => IsSimpleDispatchOverride(method),
            IMethodSymbol { Name: "Deconstruct" or "PrintMembers" } => true,
            _ => false,
        };
    private static bool IsTerminalRunForwarder(Compilation compilation, IMethodSymbol method) =>
        method.Parameters.Length == 1
        && method.DeclaringSyntaxReferences.Any(reference => reference.GetSyntax() is MethodDeclarationSyntax syntax
            && SingleReturnExpression(method: syntax) is ExpressionSyntax expression
            && compilation.GetSemanticModel(syntax.SyntaxTree).GetOperation(expression) is IOperation operation
            && IsTerminalRunForwarder(operation: operation, parameter: method.Parameters[0]));
    private static ExpressionSyntax? SingleReturnExpression(MethodDeclarationSyntax method) =>
        method.ExpressionBody?.Expression switch {
            ExpressionSyntax expression => expression,
            _ => method.Body is { Statements.Count: 1 }
                && method.Body.Statements[0] is ReturnStatementSyntax { Expression: ExpressionSyntax returned }
                    ? returned
                    : null,
        };
    private static bool IsTerminalRunForwarder(IOperation operation, IParameterSymbol parameter) =>
        UnwrapReceiver(operation) is IInvocationOperation invocation
        && invocation.TargetMethod.Name == "Run"
        && invocation.Arguments.Any(argument => IsSameParameterReference(operation: argument.Value, parameter: parameter))
        && UnwrapReceiver(ExtractReceiver(invocation)) is IInvocationOperation;
    private static bool IsSameParameterReference(IOperation operation, IParameterSymbol parameter) =>
        UnwrapReceiver(operation) switch {
            IParameterReferenceOperation parameterReference => SymbolEqualityComparer.Default.Equals(parameterReference.Parameter, parameter),
            _ => false,
        };
    private static ISymbol? MatchingBaseMember(ImmutableArray<ISymbol> dispatchMembers, ISymbol overrideMember) =>
        overrideMember switch {
            IPropertySymbol property => dispatchMembers
                .OfType<IPropertySymbol>()
                .FirstOrDefault(candidate => SymbolEqualityComparer.Default.Equals(property.OverriddenProperty?.OriginalDefinition, candidate.OriginalDefinition)),
            IMethodSymbol method => dispatchMembers
                .OfType<IMethodSymbol>()
                .FirstOrDefault(candidate => SymbolEqualityComparer.Default.Equals(method.OverriddenMethod?.OriginalDefinition, candidate.OriginalDefinition)),
            _ => null,
        };
    private static bool IsSimpleDispatchOverride(ISymbol member) =>
        member.DeclaringSyntaxReferences.Any(reference => reference.GetSyntax() switch {
            MethodDeclarationSyntax method => SimpleDispatchMethod(method),
            PropertyDeclarationSyntax property => SimpleDispatchProperty(property),
            _ => false,
        });
    private static bool SimpleDispatchMethod(MethodDeclarationSyntax method) =>
        method.ExpressionBody?.Expression is ExpressionSyntax expression
            ? IsSimpleProjectionOrDispatchExpression(expression)
            : method.Body is BlockSyntax body
                && body.Statements.Count == 1
                && body.Statements[0] is ReturnStatementSyntax { Expression: ExpressionSyntax returned }
                && IsSimpleProjectionOrDispatchExpression(returned);
    private static bool SimpleDispatchProperty(PropertyDeclarationSyntax property) =>
        property.ExpressionBody?.Expression is ExpressionSyntax expression
            ? IsSimpleProjectionOrDispatchExpression(expression)
            : property.AccessorList?.Accessors is SyntaxList<AccessorDeclarationSyntax> accessors
                && accessors.Count == 1
                && accessors[0] is {
                    Keyword.RawKind: (int)SyntaxKind.GetKeyword,
                    Body: BlockSyntax accessorBody,
                }
                && accessorBody.Statements.Count == 1
                && accessorBody.Statements[0] is ReturnStatementSyntax { Expression: ExpressionSyntax returned }
                && IsSimpleProjectionOrDispatchExpression(returned);
    private static bool IsSimpleProjectionOrDispatchExpression(ExpressionSyntax expression) =>
        UnwrapTransparentExpression(expression) switch {
            LiteralExpressionSyntax => true,
            ThisExpressionSyntax => true,
            IdentifierNameSyntax => true,
            MemberAccessExpressionSyntax memberAccess => IsSimpleProjectionOrDispatchExpression(memberAccess.Expression),
            InvocationExpressionSyntax invocation => IsSimpleInvocation(invocation),
            _ => false,
        };
    private static bool IsSimpleInvocation(InvocationExpressionSyntax invocation) =>
        invocation.Expression is IdentifierNameSyntax or MemberAccessExpressionSyntax
        && invocation.ArgumentList.Arguments.All(argument => IsSimpleProjectionOrDispatchExpression(argument.Expression));
    private static string DescribeDispatchMembers(ImmutableArray<ISymbol> dispatchMembers) =>
        dispatchMembers.Length switch {
            1 => dispatchMembers[0].Name,
            _ => $"{dispatchMembers[0].Name} and {dispatchMembers.Length - 1} more members",
        };
    internal static bool TryClosedUnionDispatch(Compilation compilation, ISymbol containingSymbol, IInvocationOperation invocation, out ClosedUnionDispatchFact fact) {
        fact = default;
        ImmutableArray<IAnonymousFunctionOperation> lambdas = [
            .. invocation.Arguments
                .Select(argument => UnwrapLambda(argument.Value))
                .Where(static lambda => lambda is not null)
                .Select(static lambda => lambda!),
        ];
        INamedTypeSymbol? union = ClosedUnionDispatchReceiver(invocation);
        ImmutableArray<INamedTypeSymbol> cases = union is null ? [] : ClosedUnionCases(compilation: compilation, namedType: union);
        ImmutableArray<INamedTypeSymbol> dispatchedCases = union is null ? [] : ClosedUnionDispatchCases(union: union, lambdas: lambdas);
        ClosedUnionDispatchKind? kind = ClosedUnionDispatchKindOf(invocation: invocation, lambdas: lambdas);
        bool complete = cases.Length >= 3
            && dispatchedCases.Length == cases.Length
            && CaseSetKey(cases: dispatchedCases) == CaseSetKey(cases: cases);
        if (invocation.TargetMethod.Name != "Switch" || union is null || kind is null || !complete) {
            return false;
        }
        INamedTypeSymbol? ownerType = containingSymbol.ContainingType;
        fact = new ClosedUnionDispatchFact(
            union: union.OriginalDefinition,
            kind: kind.Value,
            caseCount: cases.Length,
            caseSetKey: CaseSetKey(cases: cases),
            ownerKey: OwnerKey(ownerType: ownerType),
            ownerIsUnion: ownerType is not null && SymbolEqualityComparer.Default.Equals(ownerType.OriginalDefinition, union.OriginalDefinition),
            receiverRole: ClosedUnionReceiverRole(invocation: invocation),
            filePath: invocation.Syntax.SyntaxTree.FilePath,
            location: invocation.Syntax.GetLocation());
        return true;
    }
    private static INamedTypeSymbol? ClosedUnionDispatchReceiver(IInvocationOperation invocation) =>
        invocation.TargetMethod.ContainingType switch {
            INamedTypeSymbol targetType when HasAnyAttribute(targetType, "UnionAttribute", "Union") => targetType,
            _ => UnwrapReceiver(ExtractReceiver(invocation))?.Type is INamedTypeSymbol receiverType
                && HasAnyAttribute(receiverType, "UnionAttribute", "Union")
                    ? receiverType
                    : null,
        };
    private static ClosedUnionDispatchKind? ClosedUnionDispatchKindOf(IInvocationOperation invocation, ImmutableArray<IAnonymousFunctionOperation> lambdas) =>
        invocation.Type switch {
            INamedTypeSymbol type when IsMetadataLikeType(type) && lambdas.All(static lambda => !HasBehaviorInvocation(lambda)) => ClosedUnionDispatchKind.Metadata,
            INamedTypeSymbol type when IsPlanLikeType(type) || IsErasedObjectType(type) => null,
            { SpecialType: SpecialType.System_Boolean } => null,
            _ when lambdas.Any(static lambda => HasBehaviorInvocation(lambda) || HasBehaviorObjectCreation(lambda)) => ClosedUnionDispatchKind.Behavior,
            _ => null,
        };
    private static bool IsMetadataLikeType(INamedTypeSymbol type) =>
        type.Name.EndsWith(value: "Meta", comparisonType: StringComparison.Ordinal)
        || type.Name.EndsWith(value: "Metadata", comparisonType: StringComparison.Ordinal);
    private static bool IsPlanLikeType(INamedTypeSymbol type) =>
        type.Name.EndsWith(value: "Plan", comparisonType: StringComparison.Ordinal);
    private static bool IsErasedObjectType(INamedTypeSymbol type) =>
        type.SpecialType == SpecialType.System_Object;
    private static bool HasBehaviorInvocation(IAnonymousFunctionOperation lambda) =>
        lambda.Body.DescendantsAndSelf()
            .OfType<IInvocationOperation>()
            .Any(static invocation => invocation.TargetMethod.Name is not "Switch" and not "Map");
    private static bool HasBehaviorObjectCreation(IAnonymousFunctionOperation lambda) =>
        lambda.Body.DescendantsAndSelf()
            .OfType<IObjectCreationOperation>()
            .Any();
    private static ImmutableArray<INamedTypeSymbol> ClosedUnionDispatchCases(INamedTypeSymbol union, ImmutableArray<IAnonymousFunctionOperation> lambdas) =>
        [
            .. lambdas
                .Select(lambda => lambda.Symbol.Parameters
                    .Select(static parameter => parameter.Type)
                    .OfType<INamedTypeSymbol>()
                    .FirstOrDefault(type => IsClosedUnionCase(union: union, type: type)))
                .Where(static type => type is not null)
                .Select(static type => type!),
        ];
    private static bool IsClosedUnionCase(INamedTypeSymbol union, INamedTypeSymbol type) =>
        type.BaseType is INamedTypeSymbol baseType
        && SymbolEqualityComparer.Default.Equals(baseType.OriginalDefinition, union.OriginalDefinition);
    private static string CaseSetKey(ImmutableArray<INamedTypeSymbol> cases) =>
        string.Join(
            separator: "|",
            values: cases
                .Select(static type => type.OriginalDefinition.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat))
                .OrderBy(static value => value, StringComparer.Ordinal));
    private static string OwnerKey(INamedTypeSymbol? ownerType) =>
        ownerType?.OriginalDefinition.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat) ?? string.Empty;
    private static ClosedUnionDispatchReceiverRole ClosedUnionReceiverRole(IInvocationOperation invocation) =>
        UnwrapReceiver(ExtractReceiver(invocation)) switch {
            IInstanceReferenceOperation => ClosedUnionDispatchReceiverRole.This,
            IParameterReferenceOperation => ClosedUnionDispatchReceiverRole.Parameter,
            _ => ClosedUnionDispatchReceiverRole.Other,
        };
}
