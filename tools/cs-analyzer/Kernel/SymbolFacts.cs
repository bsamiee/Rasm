using System.Collections.Immutable;
using System.Globalization;
using System.Text.RegularExpressions;
using Foundation.CSharp.Analyzers.Contracts;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Operations;

namespace Foundation.CSharp.Analyzers.Kernel;

// --- [SCOPE_KIND] -------------------------------------------------------------

internal sealed class ScopeKind {
    // --- [CONSTRUCTORS] -------------------------------------------------------

    private ScopeKind(string key, bool isAnalyzable, bool isBoundary, bool isDomainOrApplication, bool isComposition) {
        Key = key;
        IsAnalyzable = isAnalyzable;
        IsBoundary = isBoundary;
        IsDomainOrApplication = isDomainOrApplication;
        IsComposition = isComposition;
    }

    // --- [PROPERTIES] ---------------------------------------------------------

    internal string Key { get; }
    internal bool IsAnalyzable { get; }
    internal bool IsBoundary { get; }
    internal bool IsDomainOrApplication { get; }
    internal bool IsComposition { get; }

    // --- [SINGLETONS] ---------------------------------------------------------

    internal static readonly ScopeKind Generated = new(key: "Generated", isAnalyzable: false, isBoundary: false, isDomainOrApplication: false, isComposition: false);
    internal static readonly ScopeKind Test = new(key: "Test", isAnalyzable: false, isBoundary: false, isDomainOrApplication: false, isComposition: false);
    internal static readonly ScopeKind Boundary = new(key: "Boundary", isAnalyzable: true, isBoundary: true, isDomainOrApplication: false, isComposition: false);
    internal static readonly ScopeKind Domain = new(key: "Domain", isAnalyzable: true, isBoundary: false, isDomainOrApplication: true, isComposition: false);
    internal static readonly ScopeKind Application = new(key: "Application", isAnalyzable: true, isBoundary: false, isDomainOrApplication: true, isComposition: false);
    internal static readonly ScopeKind Shared = new(key: "Shared", isAnalyzable: true, isBoundary: false, isDomainOrApplication: true, isComposition: false);
    internal static readonly ScopeKind Analysis = new(key: "Analysis", isAnalyzable: true, isBoundary: false, isDomainOrApplication: false, isComposition: false);
    internal static readonly ScopeKind Composition = new(key: "Composition", isAnalyzable: true, isBoundary: false, isDomainOrApplication: false, isComposition: true);
    internal static readonly ScopeKind Other = new(key: "Other", isAnalyzable: false, isBoundary: false, isDomainOrApplication: false, isComposition: false);
}

// --- [SCOPE_MODEL] -----------------------------------------------------------

internal sealed class ScopeInfo {
    // --- [CONSTRUCTORS] -------------------------------------------------------

    internal ScopeInfo(ScopeKind kind, string namespaceName, string filePath) {
        Kind = kind;
        NamespaceName = namespaceName;
        FilePath = filePath;
    }

    // --- [PROPERTIES] ---------------------------------------------------------

    internal ScopeKind Kind { get; }
    internal string NamespaceName { get; }
    internal string FilePath { get; }
    internal bool IsAnalyzable => Kind.IsAnalyzable;
    internal bool IsBoundary => Kind.IsBoundary;
    internal bool IsDomainOrApplication => Kind.IsDomainOrApplication;
    internal bool IsComposition => Kind.IsComposition;
    internal bool IsHotPath =>
        NamespaceName.Contains(value: ".Performance", comparisonType: StringComparison.Ordinal)
        || FilePath.Contains(value: "Performance", comparisonType: StringComparison.OrdinalIgnoreCase);
}

// --- [MARKERS] ---------------------------------------------------------------

internal static class Markers {
    internal const string DomainNamespace = "Domain";
    internal const string DomainPrefix = ".Domain";
    internal const string ApplicationNamespace = "Application";
    internal const string ApplicationPrefix = ".Application";
    internal const string SharedNamespace = "Shared";
    internal const string SharedPrefix = ".Shared";
    internal const string SharedKernelNamespace = "SharedKernel";
    internal const string SharedKernelPrefix = ".SharedKernel";
    internal const string LanguageExtNamespace = "LanguageExt";
    internal const string MatchMethodName = "Match";
    internal static readonly string[] BoundaryNamespace = [".Boundary", ".Rhino", ".Adapter", ".Adapters", ".Routes", ".Endpoints", ".Controllers"];
    internal static readonly string[] BoundaryPath = ["/Rhino/", "\\Rhino\\", "Adapter.cs", "Boundary.cs", "Endpoint.cs", "Controller.cs"];
    internal static readonly string[] CompositionNamespace = [".Bootstrap", ".Composition", ".DependencyInjection", ".Infrastructure"];
    internal static readonly string[] CompositionPath = ["Composition", "Bootstrap", "DependencyInjection", "Infrastructure"];
    internal static readonly string[] SharedPath = ["/Shared/", "\\Shared\\", "/SharedKernel/", "\\SharedKernel\\"];
    internal static readonly string[] AnalysisPath = ["/libs/csharp/Rasm/Analysis/", "\\libs\\csharp\\Rasm\\Analysis\\"];
    internal static readonly string[] GeneratedPath = [".g.cs", ".designer.cs", "/obj/", "\\obj\\"];
    internal static readonly string[] TestPath = ["/tests/", "\\tests\\", ".Tests", "Test.cs", "Tests.cs"];
    internal static readonly string[] ObservabilityParts = ["Observability", "Telemetry"];
}

// --- [CLASSIFICATION] --------------------------------------------------------

internal static class ScopeModel {
    // --- [FUNCTIONS] ----------------------------------------------------------

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
        bool analysis = Markers.AnalysisPath.Any(marker => filePath.Contains(value: marker, comparisonType: StringComparison.OrdinalIgnoreCase));
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

// --- [SYNTAX_FACTS] ----------------------------------------------------------

internal static class SymbolFacts {
    // --- [CONSTANTS] ----------------------------------------------------------

    private static readonly HashSet<string> BlockingMethods = new(["Wait", "WaitAll", "WaitAny", "GetResult", "Sleep"], StringComparer.Ordinal);
    private const int RegexOptionNonBacktrackingBit = 1024;
    private const int SearchValuesFriendlyRegexOptionMask =
        (int)RegexOptions.CultureInvariant
        | (int)RegexOptions.Compiled
        | RegexOptionNonBacktrackingBit;
    // --- [FLOW_FACTS] ---------------------------------------------------------
    internal static bool IsLanguageExtMatch(IInvocationOperation invocation, INamespaceSymbol? languageExtNamespace) {
        bool typeNamespaceMatch = invocation.TargetMethod.ContainingType?.ContainingNamespace is INamespaceSymbol ns
            && languageExtNamespace is not null
            && (SymbolEqualityComparer.Default.Equals(ns, languageExtNamespace)
                || ns.ToDisplayString().StartsWith(value: languageExtNamespace.ToDisplayString(), comparisonType: StringComparison.Ordinal));
        return invocation.TargetMethod.Name == Markers.MatchMethodName && typeNamespaceMatch;
    }
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

    // --- [ASYNC_FACTS] --------------------------------------------------------
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

    // --- [RUNTIME_FACTS] ------------------------------------------------------

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

    // --- [BOUNDARY_FACTS] -----------------------------------------------------

    internal static bool IsBoundaryIfCancellationGuard(IConditionalOperation conditional) =>
        conditional.Condition.DescendantsAndSelf().Any(operation =>
            operation switch {
                IPropertyReferenceOperation { Property: { Name: "IsCancellationRequested", ContainingType: INamedTypeSymbol { } type } }
                    when type.ToDisplayString() == "System.Threading.CancellationToken" => true,
                IInvocationOperation { TargetMethod: { Name: "ThrowIfCancellationRequested", ContainingType: INamedTypeSymbol { } type } }
                    when type.ToDisplayString() == "System.Threading.CancellationToken" => true,
                _ => false,
            });
    internal static bool IsAsyncIteratorYieldGate(IConditionalOperation conditional) =>
        HasYieldDescendant(conditional.WhenTrue.Syntax) || (conditional.WhenFalse?.Syntax is SyntaxNode falseBranch && HasYieldDescendant(falseBranch));
    internal static bool IsBoundaryMatchUsage(IInvocationOperation invocation) =>
        CollapseTransparentParents(invocation) is IReturnOperation
        || (invocation.Syntax.Parent is ArrowExpressionClauseSyntax clause
            && clause.Expression is ExpressionSyntax expression
            && SyntaxFactory.AreEquivalent(UnwrapTransparentExpression(invocation.Syntax), UnwrapTransparentExpression(expression)));
    internal static bool IsEarlyReturnGuard(IConditionalOperation conditional) =>
        conditional.Syntax is IfStatementSyntax {
            Else: null,
            Condition: PrefixUnaryExpressionSyntax { RawKind: (int)SyntaxKind.LogicalNotExpression },
            Statement: StatementSyntax statement,
        }
        && statement.DescendantNodesAndSelf().Any(node => node.IsKind(SyntaxKind.ReturnStatement));
    internal static bool IsSelfReassignment(ISimpleAssignmentOperation assignment) =>
        assignment.Target switch {
            ILocalReferenceOperation targetLocal => assignment.Value
                .DescendantsAndSelf()
                .OfType<ILocalReferenceOperation>()
                .Any(local => SymbolEqualityComparer.Default.Equals(local.Local, targetLocal.Local)),
            _ => false,
        };

    // --- [DOMAIN_FACTS] -------------------------------------------------------

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

    // --- [TYPE_SHAPE_FACTS] ---------------------------------------------------

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
    internal static bool IsLanguageExtValidationType(ITypeSymbol type) =>
        type is INamedTypeSymbol { OriginalDefinition.MetadataName: "Validation`2", TypeArguments.Length: 2 } validation
        && validation.ContainingNamespace.ToDisplayString().StartsWith(value: Markers.LanguageExtNamespace, comparisonType: StringComparison.Ordinal);
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

    // --- [ATTRIBUTE_FACTS] ----------------------------------------------------

    internal static bool HasAnyAttribute(ISymbol symbol, params string[] names) =>
        AllAttributes(symbol).Any(attribute => names.Contains(attribute.AttributeClass?.Name ?? string.Empty, StringComparer.Ordinal));
    internal static IEnumerable<AttributeData> AllAttributes(ISymbol symbol) =>
        symbol.GetAttributes()
            .Concat(symbol is IMethodSymbol { AssociatedSymbol: ISymbol associatedSymbol } ? associatedSymbol.GetAttributes() : [])
            .Concat(symbol.ContainingType?.GetAttributes() ?? []);

    // --- [PRIVATE_FUNCTIONS] --------------------------------------------------

    private static bool IsGenericIEnumerableTaskLike(INamedTypeSymbol namedType) =>
        namedType.MetadataName == "IEnumerable`1"
        && namedType.ContainingNamespace.ToDisplayString() == "System.Collections.Generic"
        && namedType.TypeArguments.Length == 1
        && IsTaskLikeType(namedType.TypeArguments[0]);
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
    private static bool IsGeneratedRegexAttribute(INamedTypeSymbol? attributeType) =>
        attributeType?.ToDisplayString() == "System.Text.RegularExpressions.GeneratedRegexAttribute";
    private static RegexOptions ReadGeneratedRegexOptions(ImmutableArray<TypedConstant> constructorArguments) =>
        constructorArguments.Length switch {
            > 1 when constructorArguments[1].Value is int raw => (RegexOptions)raw,
            _ => RegexOptions.None,
        };
    private static bool HasYieldDescendant(SyntaxNode syntax) =>
        syntax.DescendantNodes(descendIntoChildren: static _ => true)
            .Any(node => node.IsKind(SyntaxKind.YieldReturnStatement) || node.IsKind(SyntaxKind.YieldBreakStatement));
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
}
