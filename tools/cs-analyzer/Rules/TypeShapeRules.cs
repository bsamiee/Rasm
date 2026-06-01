using System.Collections.Immutable;
using Foundation.CSharp.Analyzers.Kernel;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Operations;

namespace Foundation.CSharp.Analyzers.Rules;

// --- [TYPE_SHAPE_RULES] ------------------------------------------------------

internal static class TypeShapeRules {
    // --- [PRIMITIVE_SHAPE] ----------------------------------------------------

    internal static void CheckDomainPrimitiveShape(SymbolAnalysisContext context, ScopeInfo scope, INamedTypeSymbol namedType) {
        bool candidate = namedType.TypeKind == TypeKind.Struct
            && namedType.GetMembers().OfType<IPropertySymbol>().Any(property => property.Name == "Value");
        bool hasPublicCtor = namedType.InstanceConstructors.Any(constructor =>
            !constructor.IsImplicitlyDeclared
            && constructor.DeclaredAccessibility == Accessibility.Public);
        bool shapeValid = namedType.IsRecord && namedType.IsReadOnly && SymbolFacts.HasCreateFactory(namedType) && !hasPublicCtor;
        AnalyzerState.Report(context.ReportDiagnostic, (scope.IsDomainOrApplication, candidate, shapeValid, namedType.Locations.Length) switch {
            (true, true, false, > 0) => Diagnostic.Create(RuleCatalog.CSP0701, namedType.Locations[0], namedType.Name),
            _ => null,
        });
    }

    // --- [CREATE_FACTORY_RETURN] ----------------------------------------------

    internal static void CheckCreateFactoryReturnType(SymbolAnalysisContext context, ScopeInfo scope, INamedTypeSymbol namedType) {
        // Thinktecture [ValueObject<T>] / [ComplexValueObject] / [SmartEnum<T>] source-gen factories
        // are canonical per coding-csharp objects.md ("Create is for trusted internal construction") --
        // they intentionally return raw T paired with TryCreate for boundary-safe parsing.
        bool isThinktectureValueObject = SymbolFacts.HasAnyAttribute(namedType,
            "ValueObjectAttribute", "ValueObject",
            "ComplexValueObjectAttribute", "ComplexValueObject",
            "SmartEnumAttribute", "SmartEnum");
        IEnumerable<IMethodSymbol> factories = namedType.GetMembers().OfType<IMethodSymbol>()
            .Where(method => method.IsStatic && method.Name is "Create" or "CreateK");
        IEnumerable<Diagnostic> diagnostics = (scope.IsDomainOrApplication, isThinktectureValueObject, namedType.Locations.Length) switch {
            (true, false, > 0) => factories
                .Where(factory => !SymbolFacts.IsFinOrKReturnType(factory))
                .Select(factory => Diagnostic.Create(RuleCatalog.CSP0713, namedType.Locations[0], namedType.Name)),
            _ => [],
        };
        AnalyzerState.ReportEach(context.ReportDiagnostic, diagnostics);
    }

    // --- [DU_SHAPE] -----------------------------------------------------------

    internal static ImmutableArray<INamedTypeSymbol> CheckDiscriminatedUnionShape(SymbolAnalysisContext context, ScopeInfo scope, INamedTypeSymbol namedType) {
        ImmutableArray<INamedTypeSymbol> unionCases = SymbolFacts.ClosedUnionCases(compilation: context.Compilation, namedType: namedType);
        bool candidate = namedType.IsRecord && namedType.IsAbstract && unionCases.Length > 0;
        bool thinktectureUnion = SymbolFacts.HasAnyAttribute(namedType, "UnionAttribute", "Union");
        bool privateProtectedCtor = namedType.InstanceConstructors.Any(constructor => constructor.DeclaredAccessibility == Accessibility.ProtectedAndInternal);
        bool privateCtor = namedType.InstanceConstructors.Any(constructor => constructor.DeclaredAccessibility == Accessibility.Private);
        bool allCasesSealed = unionCases.All(caseType => caseType.IsSealed);
        bool hasUnreachableGuard = namedType.DeclaringSyntaxReferences
            .GroupBy(static reference => reference.SyntaxTree)
            .Any(group => {
                SemanticModel model = context.Compilation.GetSemanticModel(group.Key);
                return group.Any(reference => reference.GetSyntax(context.CancellationToken)
                    .DescendantNodesAndSelf()
                    .OfType<ThrowExpressionSyntax>()
                    .Select(node => model.GetOperation(node, context.CancellationToken))
                    .Concat(reference.GetSyntax(context.CancellationToken).DescendantNodesAndSelf()
                        .OfType<ThrowStatementSyntax>()
                        .Select(node => model.GetOperation(node, context.CancellationToken)))
                    .OfType<IThrowOperation>()
                    .Any(SymbolFacts.IsUnreachableThrow));
            });
        bool valid = (thinktectureUnion, privateProtectedCtor, privateCtor, allCasesSealed, hasUnreachableGuard) switch {
            (true, _, true, true, _) => true,
            (true, true, _, true, _) => true,
            (false, true, _, true, true) => true,
            _ => false,
        };
        AnalyzerState.Report(context.ReportDiagnostic, (scope.IsDomainOrApplication, candidate, valid, namedType.Locations.Length) switch {
            (true, true, false, > 0) => Diagnostic.Create(RuleCatalog.CSP0702, namedType.Locations[0], namedType.Name),
            _ => null,
        });
        return unionCases;
    }

    // --- [UNION_OPS_QUALIFICATION] --------------------------------------------

    internal static void CheckUnionOpsQualification(SymbolAnalysisContext context, ScopeInfo scope, INamedTypeSymbol namedType) {
        bool thinktectureUnion = SymbolFacts.HasAnyAttribute(namedType, "UnionAttribute", "Union");
        bool generate = SymbolFacts.HasAnyAttribute(namedType, "GenerateUnionOpsAttribute", "GenerateUnionOps");
        bool skip = SymbolFacts.HasAnyAttribute(namedType, "SkipUnionOpsAttribute", "SkipUnionOps");
        AnalyzerState.Report(context.ReportDiagnostic, (scope.IsFunctional, thinktectureUnion, generate, skip, namedType.Locations.Length) switch {
            (true, true, false, false, > 0) => Diagnostic.Create(RuleCatalog.CSP0802, namedType.Locations[0], namedType.Name),
            _ => null,
        });
    }

    // --- [SAME_PAYLOAD_UNION_CASES] ------------------------------------------

    internal static void CheckSamePayloadUnionCases(SymbolAnalysisContext context, ScopeInfo scope, INamedTypeSymbol namedType, ImmutableArray<INamedTypeSymbol> unionCases) {
        bool samePayloadCases = SymbolFacts.HasSamePayloadUnionCaseSurface(
            namedType: namedType,
            unionCases: unionCases,
            caseCount: out int caseCount);
        AnalyzerState.Report(context.ReportDiagnostic, (scope.IsAnalyzable, samePayloadCases, namedType.Locations.Length) switch {
            (true, true, > 0) => Diagnostic.Create(RuleCatalog.CSP0737, namedType.Locations[0], namedType.Name, caseCount),
            _ => null,
        });
    }

    // --- [EXCLUSIVE_OPTION_PAYLOAD_BAG] --------------------------------------

    internal static void CheckExclusiveOptionalPayloadBag(SymbolAnalysisContext context, ScopeInfo scope, INamedTypeSymbol namedType) {
        bool exclusiveBag = SymbolFacts.HasExclusiveOptionalPayloadBag(compilation: context.Compilation, namedType: namedType, slotCount: out int slotCount, projectionWidth: out int projectionWidth);
        AnalyzerState.Report(context.ReportDiagnostic, (scope.IsAnalyzable, exclusiveBag, namedType.Locations.Length) switch {
            (true, true, > 0) => Diagnostic.Create(RuleCatalog.CSP0738, namedType.Locations[0], namedType.Name, slotCount, projectionWidth),
            _ => null,
        });
    }

    // --- [MANUAL_CLOSED_UNION_OVERRIDE] -------------------------------------

    internal static void ReportManualClosedUnionOverride(CompilationAnalysisContext context, AnalyzerState state) {
        IEnumerable<Diagnostic> diagnostics = state.NamedTypes()
            .Where(namedType => SymbolEqualityComparer.Default.Equals(namedType.ContainingAssembly, context.Compilation.Assembly))
            .Where(namedType => state.ScopeFor(namedType).IsAnalyzable)
            .Where(namedType => namedType.Locations.Length > 0)
            .Select(namedType => SymbolFacts.TryManualClosedUnionOverride(
                namedType: namedType,
                derivedTypes: state.DerivedTypes(baseType: namedType),
                facts: out ManualClosedUnionOverrideFacts facts)
                ? Diagnostic.Create(RuleCatalog.CSP0740, namedType.Locations[0], namedType.Name, facts.MemberName, facts.OverrideCount)
                : null)
            .OfType<Diagnostic>();
        AnalyzerState.ReportEach(context.ReportDiagnostic, diagnostics);
    }

    // --- [CLOSED_UNION_PLAN_FUSION] -----------------------------------------

    internal static void ReportClosedUnionPlanFusion(CompilationAnalysisContext context, AnalyzerState state) {
        IEnumerable<Diagnostic> diagnostics = state.ClosedUnionDispatches()
            .GroupBy(static fact => fact.Union.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat), StringComparer.Ordinal)
            .Select(ClosedUnionPlanFusionDiagnostic)
            .OfType<Diagnostic>();
        AnalyzerState.ReportEach(context.ReportDiagnostic, diagnostics);
    }
    private static Diagnostic? ClosedUnionPlanFusionDiagnostic(IGrouping<string, ClosedUnionDispatchFact> group) {
        ImmutableArray<ClosedUnionDispatchFact> facts = [.. group];
        ClosedUnionDispatchFact? metadata = facts
            .Where(static fact => fact.Kind == ClosedUnionDispatchKind.Metadata)
            .OrderBy(static fact => fact.Location.SourceSpan.Start)
            .Cast<ClosedUnionDispatchFact?>()
            .FirstOrDefault();
        bool duplicateBehavior = metadata is ClosedUnionDispatchFact metadataFact
            && facts.Any(fact => fact.Kind == ClosedUnionDispatchKind.Behavior && fact.CaseCount == metadataFact.CaseCount);
        return metadata is ClosedUnionDispatchFact diagnosticFact && duplicateBehavior
            ? Diagnostic.Create(RuleCatalog.CSP0744, diagnosticFact.Location, diagnosticFact.Union.Name)
            : null;
    }

    // --- [DATETIME_FIELD] -----------------------------------------------------

    internal static void CheckDateTimeFieldInDomain(SymbolAnalysisContext context, ScopeInfo scope, INamedTypeSymbol namedType) {
        IEnumerable<Diagnostic> diagnostics = scope.IsDomainOrApplication switch {
            true => namedType.GetMembers()
                .Where(member => member switch {
                    IFieldSymbol field => SymbolFacts.IsDateTimeType(UnwrapNullable(field.Type).OriginalDefinition),
                    IPropertySymbol property => SymbolFacts.IsDateTimeType(UnwrapNullable(property.Type).OriginalDefinition),
                    _ => false,
                })
                .Where(member => member.Locations.Length > 0)
                .Select(member => Diagnostic.Create(RuleCatalog.CSP0714, member.Locations[0], member.Name))
                .Concat(namedType.GetMembers().OfType<IMethodSymbol>()
                    .SelectMany(method => method.Parameters)
                    .Where(parameter => SymbolFacts.IsDateTimeType(UnwrapNullable(parameter.Type).OriginalDefinition))
                    .Where(parameter => parameter.Locations.Length > 0)
                    .Select(parameter => Diagnostic.Create(RuleCatalog.CSP0714, parameter.Locations[0], parameter.Name))),
            _ => [],
        };
        AnalyzerState.ReportEach(context.ReportDiagnostic, diagnostics);
    }

    // --- [ANEMIC_ENTITY] ------------------------------------------------------

    internal static void CheckAnemicEntity(SymbolAnalysisContext context, ScopeInfo scope, INamedTypeSymbol namedType) {
        // TypeKind.Class covers both class and record declarations in Roslyn
        bool isConcreteClass = namedType.TypeKind == TypeKind.Class && !namedType.IsAbstract && !namedType.IsStatic;
        ImmutableArray<IPropertySymbol> publicProperties = [
            .. namedType.GetMembers().OfType<IPropertySymbol>()
                .Where(property => property.DeclaredAccessibility == Accessibility.Public),
        ];
        bool allGetSet = publicProperties.Length > 0
            && publicProperties.All(property => property.GetMethod is not null && property.SetMethod is not null && !property.SetMethod.IsInitOnly);
        bool hasFactory = SymbolFacts.HasCreateFactory(namedType);
        bool hasDomainMethods = namedType.GetMembers().OfType<IMethodSymbol>()
            .Any(method => method.MethodKind == MethodKind.Ordinary
                && method.DeclaredAccessibility == Accessibility.Public
                && !method.IsStatic
                && method.Name is not "ToString" and not "Equals" and not "GetHashCode");
        AnalyzerState.Report(context.ReportDiagnostic, (scope.IsDomainOrApplication, isConcreteClass, allGetSet, hasFactory, hasDomainMethods, namedType.Locations.Length) switch {
            (true, true, true, false, false, > 0) => Diagnostic.Create(RuleCatalog.CSP0715, namedType.Locations[0], namedType.Name),
            _ => null,
        });
    }

    // --- [ATOM_REF_PROPERTY] --------------------------------------------------

    internal static void CheckAtomRefAsProperty(SymbolAnalysisContext context, ScopeInfo scope, IPropertySymbol property) =>
        AnalyzerState.Report(context.ReportDiagnostic, (scope.IsDomainOrApplication, SymbolFacts.IsAtomOrRefType(UnwrapNullable(property.Type)), property.Locations.Length) switch {
            (true, true, > 0) => Diagnostic.Create(RuleCatalog.CSP0712, property.Locations[0], property.Name),
            _ => null,
        });

    // --- [WITH_EXPRESSION_BYPASS] ---------------------------------------------

    internal static void CheckWithExpressionBypass(OperationAnalysisContext context, ScopeInfo scope, IWithOperation withOperation) {
        INamedTypeSymbol? createdType = withOperation.Type as INamedTypeSymbol;
        string typeName = createdType?.Name ?? string.Empty;
        bool hasPrivateCtor = createdType?.InstanceConstructors.Any(constructor => constructor.DeclaredAccessibility is Accessibility.Private or Accessibility.ProtectedAndInternal) == true;
        bool hasFactory = createdType is not null && SymbolFacts.HasCreateFactory(createdType);
        bool externalCaller = createdType is not null
            && !SymbolEqualityComparer.Default.Equals(
                context.ContainingSymbol?.ContainingType,
                createdType);
        AnalyzerState.Report(context.ReportDiagnostic, (scope.IsDomainOrApplication, createdType?.IsRecord, hasPrivateCtor, hasFactory, externalCaller) switch {
            (true, true, true, true, true) => Diagnostic.Create(RuleCatalog.CSP0717, context.Operation.Syntax.GetLocation(), typeName),
            _ => null,
        });
    }

    // --- [INIT_ONLY_BYPASS] ---------------------------------------------------

    internal static void CheckInitOnlyBypassOnValidated(SymbolAnalysisContext context, ScopeInfo scope, INamedTypeSymbol namedType) {
        bool hasValidatedFactory = SymbolFacts.HasCreateFactory(namedType)
            && namedType.GetMembers().OfType<IMethodSymbol>()
                .Where(method => method.IsStatic && method.Name is "Create" or "CreateK")
                .Any(SymbolFacts.IsFinOrKReturnType);
        IEnumerable<Diagnostic> diagnostics = (scope.IsDomainOrApplication, hasValidatedFactory, namedType.Locations.Length) switch {
            (true, true, > 0) => namedType.GetMembers().OfType<IPropertySymbol>()
                .Where(property => property.SetMethod is IMethodSymbol setter && setter.IsInitOnly)
                .Where(property => property.Locations.Length > 0)
                .Select(property => Diagnostic.Create(RuleCatalog.CSP0720, property.Locations[0], property.Name)),
            _ => [],
        };
        AnalyzerState.ReportEach(context.ReportDiagnostic, diagnostics);
    }

    // --- [RECEIPT_SHAPE] -----------------------------------------------------

    internal static void CheckOperationalReceiptFactStream(SymbolAnalysisContext context, ScopeInfo scope, INamedTypeSymbol namedType) {
        bool shapePressure = SymbolFacts.OperationalReceiptMemberScore(receipt: namedType) >= 3
            && !SymbolFacts.HasReceiptFactStream(receipt: namedType);
        AnalyzerState.Report(context.ReportDiagnostic, (scope.IsAnalyzable, SymbolFacts.IsOperationalReceiptType(namedType), shapePressure, namedType.Locations.Length) switch {
            (true, true, true, > 0) => Diagnostic.Create(RuleCatalog.CSP0730, namedType.Locations[0], namedType.Name),
            _ => null,
        });
    }

    // --- [FORWARDING_REQUEST_CASE_FAMILY] -----------------------------------

    internal static void CheckForwardingRequestCaseFamily(SymbolAnalysisContext context, ScopeInfo scope, INamedTypeSymbol namedType) {
        bool candidate = SymbolFacts.TryForwardingRequestCaseFamily(
            compilation: context.Compilation,
            namedType: namedType,
            facts: out ForwardingRequestCaseFamilyFacts facts);
        AnalyzerState.Report(context.ReportDiagnostic, (scope.IsAnalyzable, candidate, namedType.Locations.Length) switch {
            (true, true, > 0) => Diagnostic.Create(RuleCatalog.CSP0741, namedType.Locations[0], namedType.Name, facts.CaseCount),
            _ => null,
        });
    }

    // --- [FLAGS_ENUM_OVERUSE] -------------------------------------------------

    internal static void TrackFlagsEnumDeclaration(SymbolAnalysisContext context, AnalyzerState state, INamedTypeSymbol namedType) {
        bool flagsEnum = namedType.TypeKind == TypeKind.Enum
            && SymbolFacts.HasAnyAttribute(namedType, "FlagsAttribute", "Flags");
        _ = (context, flagsEnum) switch {
            (_, true) => RegisterFlags(state: state, enumType: namedType),
            _ => 0,
        };
    }
    internal static void TrackFlagsEnumComposition(OperationAnalysisContext context, AnalyzerState state, IBinaryOperation binary) {
        bool bitwise = binary.OperatorKind is BinaryOperatorKind.Or or BinaryOperatorKind.And or BinaryOperatorKind.ExclusiveOr;
        INamedTypeSymbol? enumType = (bitwise, UnwrapEnumType(binary.LeftOperand.Type), UnwrapEnumType(binary.RightOperand.Type)) switch {
            (true, INamedTypeSymbol left, _) => left,
            (true, _, INamedTypeSymbol right) => right,
            _ => null,
        };
        _ = (context, enumType) switch {
            (_, INamedTypeSymbol target) => RegisterFlagsComposition(state: state, enumType: target),
            _ => 0,
        };
    }
    internal static void ReportFlagsEnumOveruse(CompilationAnalysisContext context, AnalyzerState state) {
        IEnumerable<Diagnostic> diagnostics = state.FlagsEnumsWithoutComposition()
            .Where(enumType => SymbolEqualityComparer.Default.Equals(enumType.ContainingAssembly, context.Compilation.Assembly))
            .Where(enumType => enumType.Locations.Length > 0)
            .Where(enumType => state.ScopeFor(enumType).IsAnalyzable)
            .Select(enumType => Diagnostic.Create(RuleCatalog.CSP0724, enumType.Locations[0], enumType.Name));
        AnalyzerState.ReportEach(context.ReportDiagnostic, diagnostics);
    }

    // --- [PRIVATE_FUNCTIONS] --------------------------------------------------

    private static int RegisterFlags(AnalyzerState state, INamedTypeSymbol enumType) {
        state.TrackFlagsEnum(enumType: enumType);
        return 0;
    }
    private static int RegisterFlagsComposition(AnalyzerState state, INamedTypeSymbol enumType) {
        state.TrackFlagsEnumCompositionSite(enumType: enumType);
        return 0;
    }
    private static INamedTypeSymbol? UnwrapEnumType(ITypeSymbol? type) =>
        type switch {
            INamedTypeSymbol named when named.TypeKind == TypeKind.Enum => named,
            _ => null,
        };

    private static ITypeSymbol UnwrapNullable(ITypeSymbol type) => SymbolFacts.UnwrapNullable(type);
}
