using System.Collections.Immutable;
using Foundation.CSharp.Analyzers.Kernel;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Operations;

namespace Foundation.CSharp.Analyzers.Rules;

// --- [SHAPE_RULES] -----------------------------------------------------------

internal static class ShapeRules {
    // --- [CONSTANTS] ----------------------------------------------------------

    private static readonly SpecialType[] PrimitiveSpecialTypes = [
        SpecialType.System_String,
        SpecialType.System_SByte,
        SpecialType.System_Byte,
        SpecialType.System_Int16,
        SpecialType.System_UInt16,
        SpecialType.System_Int32,
        SpecialType.System_UInt32,
        SpecialType.System_Int64,
        SpecialType.System_UInt64,
        SpecialType.System_Single,
        SpecialType.System_Double,
        SpecialType.System_Boolean,
        SpecialType.System_Decimal,
    ];
    private static readonly HashSet<string> PrimitiveMetaNames = new(["Guid", "DateTime", "DateTimeOffset"], StringComparer.Ordinal);
    private static readonly HashSet<string> ConcurrentCollectionNames = new(["ConcurrentDictionary`2", "ConcurrentBag`1", "ConcurrentQueue`1", "ConcurrentStack`1"], StringComparer.Ordinal);
    private static readonly string[] InflationPrefixes = ["Get", "TryGet", "GetOr"];
    private static readonly string[] SourceGeneratorAttributeNamespaces = ["Thinktecture", "Vogen", "StronglyTypedIds"];
    private static readonly HashSet<string> InterfaceExemptionAttributes = new(["UnionAttribute", "Union", "SmartEnumAttribute", "SmartEnum", "ValueObjectAttribute", "ValueObject", "ComplexValueObjectAttribute", "ComplexValueObject", "KeyedAttribute", "Keyed"], StringComparer.Ordinal);
    private static readonly HashSet<string> TypeClassHintAttributes = new(["TypeClassAttribute", "TypeClass", "TraitAttribute", "Trait"], StringComparer.Ordinal);

    // --- [SIGNATURE_RULES] ----------------------------------------------------

    internal static void CheckSignatures(SymbolAnalysisContext context, ScopeInfo scope, ISymbol symbol) {
        IEnumerable<ITypeSymbol> signatureTypes = symbol switch {
            ISymbol candidate when IsAnalysisOperationSurface(candidate) => [],
            ISymbol candidate when IsUnionCasePayload(candidate) => [],
            IMethodSymbol method when IsValidatedPrimitiveValueAccessor(method) => [],
            IMethodSymbol method when IsValidatedPrimitiveFactory(method) => ExpandSignatureTypes(method.ReturnType),
            IMethodSymbol method => method.Parameters.SelectMany(parameter => ExpandSignatureTypes(parameter.Type)).Concat(ExpandSignatureTypes(method.ReturnType)),
            IPropertySymbol property when IsValidatedPrimitiveValueProjection(property) => [],
            IPropertySymbol property => ExpandSignatureTypes(property.Type),
            _ => [],
        };
        IEnumerable<Diagnostic> diagnostics = (scope.IsDomainOrApplication, symbol.DeclaredAccessibility) switch {
            (true, Accessibility.Public) => signatureTypes.SelectMany(type => {
                ITypeSymbol original = type.OriginalDefinition;
                string namespaceName = type.ContainingNamespace?.ToDisplayString() ?? string.Empty;
                bool bclCollection = namespaceName == "System.Collections"
                    || namespaceName == "System.Collections.Generic"
                    || namespaceName.StartsWith(value: "System.Collections.", comparisonType: StringComparison.Ordinal);
                return (PrimitiveSpecialTypes.Contains(original.SpecialType), PrimitiveMetaNames.Contains(original.MetadataName), bclCollection, type.TypeKind == TypeKind.Array, symbol.Locations.Length) switch {
                    (true, _, _, _, > 0) => [Diagnostic.Create(RuleCatalog.CSP0003, symbol.Locations[0], original.MetadataName)],
                    (_, true, _, _, > 0) => [Diagnostic.Create(RuleCatalog.CSP0003, symbol.Locations[0], original.MetadataName)],
                    (_, _, true, _, > 0) => [Diagnostic.Create(RuleCatalog.CSP0004, symbol.Locations[0], original.MetadataName)],
                    (_, _, _, true, > 0) => [Diagnostic.Create(RuleCatalog.CSP0201, symbol.Locations[0], original.MetadataName)],
                    _ => Array.Empty<Diagnostic>(),
                };
            }),
            _ => [],
        };
        AnalyzerState.ReportEach(context.ReportDiagnostic, diagnostics);
    }

    // --- [MUTABILITY_RULES] ---------------------------------------------------

    internal static void CheckMutableAutoProperty(SymbolAnalysisContext context, ScopeInfo scope, IPropertySymbol property) =>
        AnalyzerState.Report(context.ReportDiagnostic, (scope.IsDomainOrApplication, property.SetMethod, property.Locations.Length) switch {
            (true, IMethodSymbol setter, > 0) when !setter.IsInitOnly && setter.DeclaredAccessibility != Accessibility.Private
                => Diagnostic.Create(RuleCatalog.CSP0012, property.Locations[0], property.Name),
            _ => null,
        });
    internal static void CheckMutableCollections(OperationAnalysisContext context, ScopeInfo scope, IObjectCreationOperation objectCreation) {
        AnalyzerState.Report(context.ReportDiagnostic, (scope.IsDomainOrApplication, objectCreation.Type is INamedTypeSymbol mutableType && SymbolFacts.MutableCollectionNames.Contains(mutableType.OriginalDefinition.MetadataName)) switch {
            (true, true) => Diagnostic.Create(RuleCatalog.CSP0011, context.Operation.Syntax.GetLocation(), objectCreation.Type?.MetadataName ?? string.Empty),
            _ => null,
        });
        AnalyzerState.Report(context.ReportDiagnostic, (scope.IsDomainOrApplication, objectCreation.Type is INamedTypeSymbol concurrentType && ConcurrentCollectionNames.Contains(concurrentType.OriginalDefinition.MetadataName)) switch {
            (true, true) => Diagnostic.Create(RuleCatalog.CSP0204, context.Operation.Syntax.GetLocation(), objectCreation.Type?.MetadataName ?? string.Empty),
            _ => null,
        });
    }
    internal static void CheckMutableFields(SymbolAnalysisContext context, ScopeInfo scope, INamedTypeSymbol namedType) {
        IEnumerable<Diagnostic> diagnostics = scope.IsDomainOrApplication switch {
            true => namedType.GetMembers()
                .OfType<IFieldSymbol>()
                .Where(field => !field.IsReadOnly && !field.IsConst)
                .Where(field => field.DeclaredAccessibility is not Accessibility.Private)
                .Where(field => field.Locations.Length > 0)
                .Select(field => Diagnostic.Create(RuleCatalog.CSP0202, field.Locations[0], field.Name)),
            _ => [],
        };
        AnalyzerState.ReportEach(context.ReportDiagnostic, diagnostics);
    }

    // --- [MODEL_RULES] --------------------------------------------------------

    internal static void CheckPublicCtorOnValidatedPrimitive(SymbolAnalysisContext context, ScopeInfo scope, INamedTypeSymbol namedType) {
        bool hasPublicInstanceCtor = namedType.InstanceConstructors.Any(constructor =>
            !constructor.IsImplicitlyDeclared
            && constructor.DeclaredAccessibility == Accessibility.Public);
        AnalyzerState.Report(context.ReportDiagnostic, (scope.IsDomainOrApplication, namedType.TypeKind, namedType.IsReadOnly, SymbolFacts.HasCreateFactory(namedType), hasPublicInstanceCtor, namedType.Locations.Length) switch {
            (true, TypeKind.Struct, true, true, true, > 0)
                => Diagnostic.Create(RuleCatalog.CSP0203, namedType.Locations[0], namedType.Name),
            _ => null,
        });
    }
    internal static void CheckValidationTypeUsage(SymbolAnalysisContext context, ScopeInfo scope, INamedTypeSymbol namedType) {
        IEnumerable<ITypeSymbol> publicTypes = namedType.GetMembers().OfType<IMethodSymbol>()
            .Where(method => method.DeclaredAccessibility == Accessibility.Public)
            .SelectMany(method => method.Parameters.Select(parameter => parameter.Type).Append(method.ReturnType))
            .Concat(namedType.GetMembers().OfType<IPropertySymbol>()
                .Where(property => property.DeclaredAccessibility == Accessibility.Public)
                .Select(property => property.Type))
            .Select(UnwrapNullable);
        IEnumerable<Diagnostic> diagnostics = (scope.IsDomainOrApplication, namedType.Locations.Length) switch {
            (true, > 0) => publicTypes
                .SelectMany(ExpandSignatureTypes)
                .Where(type => SymbolFacts.IsLanguageExtValidationType(type) && !SymbolFacts.IsValidationErrorReturnType(type))
                .Select(type => Diagnostic.Create(RuleCatalog.CSP0703, namedType.Locations[0], type.ToDisplayString())),
            _ => [],
        };
        AnalyzerState.ReportEach(context.ReportDiagnostic, diagnostics);
    }

    // --- [SURFACE_RULES] ------------------------------------------------------

    internal static void CheckOverloadSpam(SymbolAnalysisContext context, ScopeInfo scope, INamedTypeSymbol namedType) {
        IEnumerable<Diagnostic> diagnostics = scope.IsDomainOrApplication switch {
            true => namedType.GetMembers()
                .OfType<IMethodSymbol>()
                .Where(method => method.MethodKind == MethodKind.Ordinary)
                .Where(method => !method.IsImplicitlyDeclared)
                .GroupBy(method => method.Name)
                .Select(group => group.ToImmutableArray())
                .Where(ShouldReportOverloadCollapse)
                .Where(members => members[0].Locations.Length > 0)
                .Select(members => Diagnostic.Create(RuleCatalog.CSP0005, members[0].Locations[0], members[0].Name, members.Length)),
            _ => [],
        };
        AnalyzerState.ReportEach(context.ReportDiagnostic, diagnostics);
    }
    internal static void CheckApiSurfaceInflationByPrefix(SymbolAnalysisContext context, ScopeInfo scope, INamedTypeSymbol namedType) {
        // Source-generator-driven types (Thinktecture, Vogen, StronglyTypedIds) emit Get/TryGet companions by convention.
        // Detection is namespace-prefix based so future attributes from these generators inherit the exemption automatically.
        bool generatedSurface = namedType.GetAttributes().Any(attribute =>
            attribute.AttributeClass is INamedTypeSymbol attributeClass
            && (InterfaceExemptionAttributes.Contains(attributeClass.Name)
                || SourceGeneratorAttributeNamespaces.Any(prefix => (attributeClass.ContainingNamespace?.ToDisplayString() ?? string.Empty).StartsWith(prefix, StringComparison.Ordinal))));
        ImmutableArray<IMethodSymbol> familyMethods = generatedSurface ? [] : [
            .. namedType.GetMembers().OfType<IMethodSymbol>()
                .Where(method => method.MethodKind == MethodKind.Ordinary)
                .Where(method => method.DeclaredAccessibility == Accessibility.Public)
                .Where(method => InflationPrefixes.Any(prefix => method.Name.StartsWith(value: prefix, comparisonType: StringComparison.Ordinal))),
        ];
        int distinctNames = familyMethods.Select(method => method.Name).Distinct(StringComparer.Ordinal).Count();
        bool inflation = familyMethods.Length > 3 && distinctNames > 2;
        AnalyzerState.Report(context.ReportDiagnostic, (scope.IsDomainOrApplication, inflation, namedType.Locations.Length) switch {
            (true, true, > 0) => Diagnostic.Create(RuleCatalog.CSP0708, namedType.Locations[0], namedType.Name),
            _ => null,
        });
    }
    internal static void CheckPositionalArguments(OperationAnalysisContext context, ScopeInfo scope, IInvocationOperation invocation) {
        ImmutableArray<ArgumentSyntax> positionalArguments = invocation.Syntax is InvocationExpressionSyntax { ArgumentList.Arguments: { Count: > 0 } arguments }
            ? [.. arguments.Where(argument => argument.NameColon is null)]
            : [];
        string targetNamespace = invocation.TargetMethod.ContainingNamespace?.ToDisplayString() ?? string.Empty;
        bool domainTarget = SymbolFacts.IsDomainOrApplicationNamespace(targetNamespace);
        IEnumerable<Diagnostic> diagnostics = (scope.IsDomainOrApplication, domainTarget) switch {
            (true, true) => positionalArguments.Select(argument => Diagnostic.Create(RuleCatalog.CSP0502, argument.GetLocation())),
            _ => [],
        };
        AnalyzerState.ReportEach(context.ReportDiagnostic, diagnostics);
    }
    internal static void CheckPositionalRecordConstructor(OperationAnalysisContext context, ScopeInfo scope, IObjectCreationOperation objectCreation) {
        int parameterCount = objectCreation.Constructor?.Parameters.Length ?? 0;
        ImmutableArray<ArgumentSyntax> positionalArguments = objectCreation.Syntax switch {
            ObjectCreationExpressionSyntax { ArgumentList.Arguments: { Count: > 0 } arguments } =>
                [.. arguments.Where(argument => argument.NameColon is null)],
            ImplicitObjectCreationExpressionSyntax { ArgumentList.Arguments: { Count: > 0 } arguments } =>
                [.. arguments.Where(argument => argument.NameColon is null)],
            _ => [],
        };
        IEnumerable<Diagnostic> diagnostics = (scope.IsDomainOrApplication, objectCreation.Type is INamedTypeSymbol { IsRecord: true } recordType, parameterCount >= 3, positionalArguments.Length > 0) switch {
            (true, true, true, true) => positionalArguments.Select(argument => Diagnostic.Create(RuleCatalog.CSP0726, argument.GetLocation(), ((INamedTypeSymbol)objectCreation.Type!).Name, parameterCount)),
            _ => [],
        };
        AnalyzerState.ReportEach(context.ReportDiagnostic, diagnostics);
    }
    internal static void CheckEffectReturnPolicy(SymbolAnalysisContext context, ScopeInfo scope, IMethodSymbol method) {
        bool ordinaryMethod = method.MethodKind == MethodKind.Ordinary;
        bool disallowedReturn = method.ReturnsVoid || SymbolFacts.IsTaskLikeType(method.ReturnType);
        bool interfaceContract = method.IsOverride || method.ExplicitInterfaceImplementations.Length > 0;
        string returnType = method.ReturnsVoid ? "void" : method.ReturnType.ToDisplayString();
        AnalyzerState.Report(context.ReportDiagnostic, (scope.IsDomainOrApplication, ordinaryMethod, disallowedReturn, interfaceContract, method.Locations.Length) switch {
            (true, true, true, false, > 0) => Diagnostic.Create(RuleCatalog.CSP0504, method.Locations[0], method.Name, returnType),
            _ => null,
        });
    }
    internal static void CheckTypeClassStaticAbstractPolicy(SymbolAnalysisContext context, ScopeInfo scope, INamedTypeSymbol namedType) {
        bool candidate = namedType.TypeKind == TypeKind.Interface && IsTypeClassInterfaceCandidate(namedType);
        bool hasStaticAbstractMember = namedType.GetMembers().Any(member =>
            member switch {
                IMethodSymbol method => method.IsStatic && method.IsAbstract,
                IPropertySymbol property => property.IsStatic && property.IsAbstract,
                IEventSymbol @event => @event.IsStatic && @event.IsAbstract,
                _ => false,
            });
        AnalyzerState.Report(context.ReportDiagnostic, (scope.IsDomainOrApplication, candidate, hasStaticAbstractMember, namedType.Locations.Length) switch {
            (true, true, false, > 0) => Diagnostic.Create(RuleCatalog.CSP0505, namedType.Locations[0], namedType.Name),
            _ => null,
        });
    }
    internal static void CheckExtensionProjectionPolicy(SymbolAnalysisContext context, ScopeInfo scope, IMethodSymbol method) {
        ITypeSymbol? receiverType = method.Parameters.Length > 0 ? UnwrapNullable(method.Parameters[0].Type) : null;
        string receiverName = receiverType?.ToDisplayString() ?? string.Empty;
        bool staticOrdinaryMethod = method.MethodKind == MethodKind.Ordinary && method.IsStatic;
        bool validatedFactory = method.Name is "Create" or "CreateK";
        bool receiverDomainOrApplication = receiverType is ITypeSymbol type && SymbolFacts.IsDomainOrApplicationNamespace(type.ContainingNamespace?.ToDisplayString() ?? string.Empty);
        bool externalReceiver = receiverType switch {
            INamedTypeSymbol namedReceiver => !SymbolEqualityComparer.Default.Equals(namedReceiver, method.ContainingType),
            _ => true,
        };
        bool returnsContainingType = SymbolEqualityComparer.Default.Equals(method.ReturnType, method.ContainingType);
        AnalyzerState.Report(context.ReportDiagnostic, (scope.IsDomainOrApplication, staticOrdinaryMethod, validatedFactory, method.IsExtensionMethod, receiverDomainOrApplication, externalReceiver, returnsContainingType, method.Locations.Length) switch {
            (true, true, false, false, true, true, false, > 0) => Diagnostic.Create(RuleCatalog.CSP0506, method.Locations[0], method.Name, receiverName),
            _ => null,
        });
    }

    // --- [REPORTS] ------------------------------------------------------------

    internal static void ReportInterfacePollution(CompilationAnalysisContext context, AnalyzerState state) {
        IEnumerable<Diagnostic> diagnostics = state.SingleImplementationInterfaces()
            .Where(tuple => tuple.Interface.TypeKind == TypeKind.Interface)
            .Where(tuple => tuple.Interface.Name.StartsWith(value: "I", comparisonType: StringComparison.Ordinal))
            .Where(tuple => state.ScopeFor(tuple.Interface).IsDomainOrApplication)
            .Where(tuple => !IsExemptInterface(tuple.Interface))
            .Where(tuple => tuple.Interface.Locations.Length > 0)
            .Select(tuple => Diagnostic.Create(RuleCatalog.CSP0501, tuple.Interface.Locations[0], tuple.Interface.Name, tuple.Implementation.Name));
        AnalyzerState.ReportEach(context.ReportDiagnostic, diagnostics);
    }
    internal static void ReportSingleUseHelpers(CompilationAnalysisContext context, AnalyzerState state) {
        IEnumerable<Diagnostic> diagnostics = state.SingleUsePrivateMethods()
            .Where(method => state.ScopeFor(method).IsDomainOrApplication)
            .Where(method => method.Locations.Length > 0)
            .Select(method => Diagnostic.Create(RuleCatalog.CSP0503, method.Locations[0], method.Name));
        AnalyzerState.ReportEach(context.ReportDiagnostic, diagnostics);
    }

    // --- [CONVENTION_RULES] ---------------------------------------------------

    internal static void CheckVarInference(SyntaxNodeAnalysisContext context, AnalyzerState state) {
        ScopeInfo scope = context.ContainingSymbol switch {
            ISymbol symbol => state.ScopeFor(symbol: symbol),
            _ => new ScopeInfo(kind: ScopeKind.Other, namespaceName: string.Empty, filePath: context.Node.SyntaxTree.FilePath),
        };
        IEnumerable<(Location Location, string Name)> varUsages = (scope.IsDomainOrApplication, context.Node) switch {
            (true, VariableDeclarationSyntax { Type.IsVar: true } declaration) =>
                declaration.Variables.Select(variable => (Location: declaration.Type.GetLocation(), Name: variable.Identifier.ValueText)),
            (true, ForEachStatementSyntax { Type.IsVar: true } statement) =>
                [(Location: statement.Type.GetLocation(), Name: statement.Identifier.ValueText)],
            (true, DeclarationExpressionSyntax { Type: IdentifierNameSyntax { Identifier.ValueText: "var" } } declaration) =>
                declaration.Designation switch {
                    SingleVariableDesignationSyntax single => [(Location: declaration.Type.GetLocation(), Name: single.Identifier.ValueText)],
                    _ => Array.Empty<(Location Location, string Name)>(),
                },
            _ => [],
        };
        AnalyzerState.ReportEach(context.ReportDiagnostic, varUsages.Select(usage =>
            Diagnostic.Create(RuleCatalog.CSP0015, usage.Location, usage.Name)));
    }
    // --- [PRIVATE_FUNCTIONS] --------------------------------------------------

    private static bool IsExemptInterface(INamedTypeSymbol interfaceSymbol) {
        // Exempt Has<RT,Trait> pattern interfaces
        bool hasTraitPattern = interfaceSymbol.IsGenericType
            && interfaceSymbol.Name.StartsWith(value: "Has", comparisonType: StringComparison.Ordinal);
        // Exempt static abstract interfaces (type classes)
        bool hasStaticAbstract = interfaceSymbol.GetMembers().OfType<IMethodSymbol>()
            .Any(method => method.IsStatic && method.IsAbstract);
        // Exempt [Union]/[SmartEnum] attributed types
        bool hasExemptionAttribute = interfaceSymbol.GetAttributes()
            .Any(attribute => InterfaceExemptionAttributes.Contains(attribute.AttributeClass?.Name ?? string.Empty));
        return hasTraitPattern || hasStaticAbstract || hasExemptionAttribute;
    }
    private static bool IsTypeClassInterfaceCandidate(INamedTypeSymbol interfaceSymbol) {
        bool nameHint = interfaceSymbol.Name.Contains(value: "TypeClass", comparisonType: StringComparison.Ordinal)
            || interfaceSymbol.Name.EndsWith(value: "Trait", comparisonType: StringComparison.Ordinal);
        bool attributeHint = interfaceSymbol.GetAttributes()
            .Any(attribute => TypeClassHintAttributes.Contains(attribute.AttributeClass?.Name ?? string.Empty));
        bool selfTypeHint = interfaceSymbol.TypeParameters.Any(parameter =>
            parameter.Name.Equals(value: "TSelf", comparisonType: StringComparison.Ordinal)
            || parameter.Name.Equals(value: "TSelfType", comparisonType: StringComparison.Ordinal));
        return nameHint || attributeHint || selfTypeHint;
    }
    private static bool ShouldReportOverloadCollapse(ImmutableArray<IMethodSymbol> overloads) {
        bool hasReadOnlySpanCollapse = overloads.Any(IsParamsReadOnlySpanOverload);
        bool unionPolymorphicPair = IsUnionDispatchingPair(overloads);
        bool arityLadder = overloads.Select(overload => overload.Parameters.Length).Distinct().Count() > 1;
        bool overloadPressure = overloads.Length > 2 || (overloads.Length > 1 && arityLadder);
        return overloadPressure && !hasReadOnlySpanCollapse && !unionPolymorphicPair;
    }
    private static bool IsUnionDispatchingPair(ImmutableArray<IMethodSymbol> overloads) {
        ImmutableArray<IMethodSymbol> ordered = overloads.Length switch {
            2 => [.. overloads.OrderBy(method => method.TypeParameters.Length)],
            _ => [],
        };
        return ordered.Length switch {
            2 when ordered[1].TypeParameters.Length > ordered[0].TypeParameters.Length
                && CarriesUnionDiscriminator(ordered[1])
                && !CarriesUnionDiscriminator(ordered[0]) => true,
            _ => false,
        };
    }
    private static bool CarriesUnionDiscriminator(IMethodSymbol method) =>
        method.Parameters.Any(parameter =>
            parameter.Type is INamedTypeSymbol named
                && IsTypeParameterizedDiscriminator(named));
    // Only type-parameterized Unions count as polymorphic discriminators; non-generic [Union] types
    // act as vocabularies, not dispatchers, and must not gate the relaxation.
    private static bool IsTypeParameterizedDiscriminator(INamedTypeSymbol type) =>
        type.TypeArguments.Length > 0
        && type.TypeArguments.Any(argument => argument.TypeKind == TypeKind.TypeParameter)
        && SymbolFacts.HasAnyAttribute(type, "UnionAttribute", "Union")
        && SymbolFacts.IsDomainNamespace(type.ContainingNamespace?.ToDisplayString() ?? string.Empty);
    private static bool IsParamsReadOnlySpanOverload(IMethodSymbol overload) =>
        overload.Parameters switch {
            { Length: > 0 } parameters => parameters[parameters.Length - 1] switch {
                { IsParams: true, Type: INamedTypeSymbol { OriginalDefinition.MetadataName: "ReadOnlySpan`1", ContainingNamespace: { } ns } }
                    => ns.ToDisplayString() == "System",
                _ => false,
            },
            _ => false,
        };
    private static bool IsValidatedPrimitiveFactory(IMethodSymbol method) =>
        method is {
            IsStatic: true,
            Name: "Create" or "CreateK",
            ContainingType: INamedTypeSymbol containingType,
        } && IsPotentialValidatedPrimitive(containingType);
    private static bool IsValidatedPrimitiveValueProjection(IPropertySymbol property) =>
        property is {
            Name: "Value",
            ContainingType: INamedTypeSymbol containingType,
        } && IsPotentialValidatedPrimitive(containingType);
    private static bool IsValidatedPrimitiveValueAccessor(IMethodSymbol method) =>
        method.MethodKind is MethodKind.PropertyGet or MethodKind.PropertySet
        && method.AssociatedSymbol is IPropertySymbol property
        && IsValidatedPrimitiveValueProjection(property);
    private static bool IsPotentialValidatedPrimitive(INamedTypeSymbol type) =>
        type.TypeKind == TypeKind.Struct
        && SymbolFacts.HasCreateFactory(type)
        && type.GetMembers().OfType<IPropertySymbol>().Any(property => property.Name == "Value");
    private static bool IsAnalysisOperationSurface(ISymbol symbol) =>
        symbol.ContainingType is INamedTypeSymbol { Name: "Operation", ContainingNamespace: { } operationNamespace }
        && operationNamespace.ToDisplayString() == "Rasm.Analysis";
    private static bool IsUnionCasePayload(ISymbol symbol) =>
        symbol.ContainingType is INamedTypeSymbol containingType
        && (SymbolFacts.HasAnyAttribute(containingType, "UnionAttribute", "Union")
            || (containingType is { IsSealed: true, IsRecord: true } && containingType.ContainingType is INamedTypeSymbol unionBase
                && SymbolFacts.HasAnyAttribute(unionBase, "UnionAttribute", "Union")));
    private static IEnumerable<ITypeSymbol> ExpandSignatureTypes(ITypeSymbol type) {
        ITypeSymbol unwrapped = UnwrapNullable(type);
        IEnumerable<ITypeSymbol> nested = unwrapped switch {
            IArrayTypeSymbol arrayType => ExpandSignatureTypes(arrayType.ElementType),
            INamedTypeSymbol namedType => namedType.TypeArguments.SelectMany(ExpandSignatureTypes),
            _ => [],
        };
        return [unwrapped, .. nested];
    }
    private static ITypeSymbol UnwrapNullable(ITypeSymbol type) => SymbolFacts.UnwrapNullable(type);
}
