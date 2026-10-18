using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Operations;

namespace Rasm.Analyzers;

// --- [COMPOSITION] ---------------------------------------------------------------------
[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class ThinktectureAnalyzer : RasmAnalyzer {
    // --- [RULES]
    private const string Domain = "Thinktecture";

    public static readonly DiagnosticDescriptor CatchAll = Rule(
        "RASM1101",
        Domain,
        "Catch-all arm over smart enum or union",
        "Catch-all arm in `switch` over `{0}` hides cases without an arm",
        "Call the generated exhaustive `Switch` or `Map`, which fails to compile when a case gains no arm.");

    public static readonly DiagnosticDescriptor Validation = Rule(
        "RASM1102",
        Domain,
        "Thinktecture validation outside Validated",
        "`{0}` validates outside `Validated` and returns no `Fin`",
        "Call `<type>.Validated(<raw>)`, or `ValidationResults.Validated<T, TKey>(<raw>)` in generic code and for a type with more than one object factory. Direct calls belong in a validation hook, object factory, or `ValidationResults`.");

    public static readonly DiagnosticDescriptor Conversion = Rule(
        "RASM1103",
        Domain,
        "ValidationError converted to Error by hand",
        "`{0}` converts a `ValidationError` message into an `Error` by hand",
        "Call `ValidationResults.ToFin(<validation error>, <value>)`, the conversion `<type>.Validated(<raw>)` runs.");

    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => [CatchAll, Validation, Conversion];

    protected override void Start(CompilationStartAnalysisContext context) {
        Compilation compilation = context.Compilation;
        INamedTypeSymbol? results = compilation.GetTypeByMetadataName("Thinktecture.ValidationResults");
        CatchAlls(context);
        if (compilation.GetTypeByMetadataName("Thinktecture.IObjectFactory`3") is { } factory
            && compilation.GetTypeByMetadataName("Thinktecture.ValidationError") is { } invalid)
            Validations(context, factory, invalid, results);
        if (compilation.GetTypeByMetadataName("LanguageExt.Common.Error") is { } error)
            Conversions(context, error, results);
    }

    // --- [CATCH_ALLS]
    private static void CatchAlls(CompilationStartAnalysisContext context) {
        Compilation compilation = context.Compilation;
        if (compilation.GetTypeByMetadataName("Thinktecture.UnionAttributeBase") is not { } adHoc
            || compilation.GetTypeByMetadataName("Thinktecture.SmartEnumAttribute`1") is not { } keyed) return;
        ImmutableArray<INamedTypeSymbol> marks = [
            .. new[] { adHoc, keyed, compilation.GetTypeByMetadataName("Thinktecture.UnionAttribute"), compilation.GetTypeByMetadataName("Thinktecture.SmartEnumAttribute") }.OfType<INamedTypeSymbol>(),
        ];
        AttributeData? Mark(ITypeSymbol? type) => type?.GetAttributes().FirstOrDefault(data => marks.Any(mark => Derives(data.AttributeClass, mark)));
        static string Key(AttributeData mark) => mark.NamedArguments.Where(static named => named.Key is "KeyMemberName").Select(static named => named.Value.Value).OfType<string>().FirstOrDefault() ?? "Key";
        INamedTypeSymbol? Marked(ITypeSymbol? type) =>
            type switch {
                INamedTypeSymbol { IsTupleType: true } tuple => tuple.TupleElements.Select(element => Marked(element.Type)).FirstOrDefault(static found => found is not null),
                INamedTypeSymbol { OriginalDefinition.SpecialType: SpecialType.System_Nullable_T } nullable => Marked(nullable.TypeArguments[0]),
                INamedTypeSymbol named when Mark(named) is not null => named,
                _ => null,
            };
        static bool Names(IOperation arm, Func<IOperation, bool> wanted) => arm.DescendantsAndSelf().Any(wanted);
        (INamedTypeSymbol Owner, Func<IOperation, bool> Case)? Closed(IOperation subject) =>
            Stripped(subject) switch {
                IPropertyReferenceOperation { Instance.Type: INamedTypeSymbol owner, Property: var property }
                    when Mark(owner) is { AttributeClass: { TypeArguments.Length: 1 } attribute } mark && Same(attribute, keyed)
                        && SymbolEqualityComparer.Default.Equals(property.Type, attribute.TypeArguments[0]) && string.Equals(property.Name, Key(mark), StringComparison.Ordinal) =>
                    (owner, static test => test is IConstantPatternOperation or IRelationalPatternOperation || (test is not IPatternOperation && test.ConstantValue.HasValue)),
                IPropertyReferenceOperation { Instance.Type: INamedTypeSymbol owner, Property.Type.SpecialType: SpecialType.System_Object }
                    when Mark(owner) is { AttributeClass: { } union } mark && Derives(union, adHoc)
                        && (ImmutableArray<ITypeSymbol>)[.. union.TypeArguments, .. mark.ConstructorArguments.Select(static held => held.Value).OfType<ITypeSymbol>()] is { IsEmpty: false } cases =>
                    (owner, test => test is IPatternOperation { NarrowedType: var narrowed } && cases.Any(held => SymbolEqualityComparer.Default.Equals(held, narrowed))),
                _ when Marked(subject.Type) is { } owner => (owner, test => test switch {
                    IPatternOperation { NarrowedType: var narrowed } => !Same(narrowed, owner) && Derives(narrowed, owner),
                    IFieldReferenceOperation { Field: { IsStatic: true, Type: var item } } => Same(item, owner),
                    _ => false,
                }),
                _ => null,
            };
        context.RegisterOperationAction(
            analysis => {
                if (Dispatch(analysis.Operation) is not ( { } subject, var arms) || Closed(subject) is not ( { } owner, var names)) return;
                if (arms.Any(arm => !CatchesAll(arm) && ((arm.Test is { } test && Names(test, names)) || (arm.Guard is { } guard && Names(guard, names)))))
                    foreach ((IOperation arm, _, _) in arms.Where(CatchesAll))
                        analysis.ReportDiagnostic(Diagnostic.Create(CatchAll, arm.Syntax.GetLocation(), owner.Name));
            },
            OperationKind.SwitchExpression,
            OperationKind.Switch);
    }

    // --- [VALIDATIONS]
    private static void Validations(CompilationStartAnalysisContext context, INamedTypeSymbol factory, INamedTypeSymbol invalid, INamedTypeSymbol? results) {
        if (Declared(context.Compilation, "M:Thinktecture.IObjectFactory`3.Validate(`1,System.IFormatProvider,`0@)").FirstOrDefault() is not { } validate) return;
        INamedTypeSymbol? items = context.Compilation.GetTypeByMetadataName("Thinktecture.ISmartEnum`3");
        IEnumerable<INamedTypeSymbol> Factories(INamedTypeSymbol owner) =>
            owner.AllInterfaces.Where(face => Same(face, factory) && Same(face.TypeArguments[0], owner) && Same(face.TypeArguments[2], invalid));
        bool Implements(IMethodSymbol method, INamedTypeSymbol owner, IEnumerable<INamedTypeSymbol> faces, Func<ISymbol, bool> wanted) =>
            faces.SelectMany(static face => face.GetMembers()).Any(member => wanted(member) && Same(owner.FindImplementationForInterfaceMember(member), method));
        bool Creates(IMethodSymbol method, INamedTypeSymbol owner) =>
            method is { ReturnType.SpecialType: SpecialType.System_Boolean, Parameters: { Length: 2 } parameters } && parameters[1] is { RefKind: RefKind.Out, Type: var made } && Same(made, owner)
            && !Implements(method, owner, owner.AllInterfaces.Where(face => Same(face, items)), static member => member.IsStatic)
            && Factories(owner).Any(face => Same(face.TypeArguments[1], parameters[0].Type));
        bool Crosses(IMethodSymbol method) =>
            Same(method, validate)
            || (method is { IsStatic: true, ContainingType: { } owner } && (Implements(method, owner, Factories(owner), member => Same(member, validate)) || Creates(method, owner)));
        bool Hooked(ISymbol owner) =>
            owner is IMethodSymbol method
            && ((method.ReturnType is { NullableAnnotation: NullableAnnotation.Annotated } returned && Same(returned, invalid))
                || method.Parameters.Any(parameter => parameter.RefKind is RefKind.Ref && Same(parameter.Type, invalid)));
        context.RegisterOperationAction(
            analysis => {
                IInvocationOperation invocation = (IInvocationOperation)analysis.Operation;
                if (Crosses(invocation.TargetMethod) && !Hooked(analysis.ContainingSymbol) && !Within(analysis.ContainingSymbol, results))
                    analysis.ReportDiagnostic(Diagnostic.Create(Validation, invocation.Syntax.GetLocation(), invocation.Syntax));
            },
            OperationKind.Invocation);
    }

    // --- [CONVERSIONS]
    private static void Conversions(CompilationStartAnalysisContext context, INamedTypeSymbol error, INamedTypeSymbol? results) {
        if (Declared(context.Compilation, "P:Thinktecture.ValidationError.Message").FirstOrDefault() is not { } message) return;
        context.RegisterOperationAction(
            analysis => {
                IPropertyReferenceOperation reference = (IPropertyReferenceOperation)analysis.Operation;
                if (Same(reference.Property, message) && !Within(analysis.ContainingSymbol, results)
                    && Consumer(reference) is IArgumentOperation { Parent: IInvocationOperation or IObjectCreationOperation } argument && argument.Parent is { } site && Derives(site.Type, error))
                    analysis.ReportDiagnostic(Diagnostic.Create(Conversion, site.Syntax.GetLocation(), site.Syntax));
            },
            OperationKind.PropertyReference);
    }
}
