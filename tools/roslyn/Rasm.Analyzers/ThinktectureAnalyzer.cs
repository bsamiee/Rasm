using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Operations;

namespace Rasm.Analyzers;

// --- [COMPOSITION] ---------------------------------------------------------------------
[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class ThinktectureAnalyzer : RasmAnalyzer {
    // --- [RULES]
    private const string Category = "Thinktecture";

    public static readonly DiagnosticDescriptor CatchAll = Rule(
        "RASM1101",
        Category,
        "Catch-all arm over smart enum or union",
        "Catch-all arm over `{0}` hides unhandled cases, call generated `Switch` or `Map` with one argument per case",
        "Catch-all arm in a `switch` over a Thinktecture union or smart enum with other arms naming a case.");

    public static readonly DiagnosticDescriptor Validation = Rule(
        "RASM1102",
        Category,
        "Thinktecture validation outside Validated",
        "`{0}` validates outside `Validated` and returns no `Fin`, call `{1}`",
        "Thinktecture `Validate` or `TryCreate` call outside a validation hook, object factory, or `ValidationResults`.");

    public static readonly DiagnosticDescriptor Conversion = Rule(
        "RASM1103",
        Category,
        "ValidationError converted to Error by hand",
        "`{0}` converts a `ValidationError` message to an `Error` by hand, call `ValidationResults.ToFin({1}, <value>)`",
        "`ValidationError.Message` passed to an `Error` factory or constructor outside `ValidationResults`.");

    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => [CatchAll, Validation, Conversion];

    protected override void Start(CompilationStartAnalysisContext context) {
        ThinktectureSymbols symbols = new(context.Compilation);
        if (symbols is { UnionAttributeBase: { } adHoc, SmartEnumAttribute1: { } keyed }) CatchAlls(context, symbols.Marks, adHoc, keyed);
        if (symbols is { ObjectFactory: { } factory, ValidationError: { } invalid, Validate: { } validate })
            Validations(context, factory, invalid, validate, symbols.SmartEnum, symbols.ValidationResults);
        if (new LanguageExtSymbols(context.Compilation).Error is { } error && symbols.Message is { } message) Conversions(context, error, message, symbols.ValidationResults);
    }

    // --- [CATCH_ALLS]
    private static void CatchAlls(CompilationStartAnalysisContext context, ImmutableArray<INamedTypeSymbol> marks, INamedTypeSymbol adHoc, INamedTypeSymbol keyed) {
        AttributeData? Mark(ITypeSymbol? type) => type?.GetAttributes().FirstOrDefault(data => marks.Any(mark => Derives(data.AttributeClass, mark)));
        static string Key(AttributeData mark) => mark.NamedArguments.Where(static named => named.Key is "KeyMemberName").Select(static named => named.Value.Value).OfType<string>().FirstOrDefault() ?? "Key";
        INamedTypeSymbol? Marked(ITypeSymbol? type) =>
            type switch {
                INamedTypeSymbol { IsTupleType: true } tuple => tuple.TupleElements.Select(element => Marked(element.Type)).FirstOrDefault(static found => found is not null),
                INamedTypeSymbol { OriginalDefinition.SpecialType: SpecialType.System_Nullable_T } nullable => Marked(nullable.TypeArguments[0]),
                INamedTypeSymbol named when Mark(named) is not null => named,
                _ => null,
            };
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
        Report<IOperation>(context, [OperationKind.SwitchExpression, OperationKind.Switch], (operation, _) =>
            Dispatch(operation) is ( { } subject, var arms) && Closed(subject) is ( { } owner, var names)
            && arms.Any(arm => !CatchesAll(arm) && ((arm.Test is { } test && test.DescendantsAndSelf().Any(names)) || (arm.Guard is { } guard && guard.DescendantsAndSelf().Any(names))))
                ? arms.Where(CatchesAll).Select(arm => Diagnostic.Create(CatchAll, arm.Arm.Syntax.GetLocation(), owner.Name))
                : []);
    }

    // --- [VALIDATIONS]
    private static void Validations(CompilationStartAnalysisContext context, INamedTypeSymbol factory, INamedTypeSymbol invalid, ISymbol validate, INamedTypeSymbol? items, INamedTypeSymbol? results) {
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
        string Validated(IInvocationOperation call) =>
            (call.TargetMethod.ContainingType, call.Arguments[0].Value.Syntax) is var (owner, raw) && !Same(owner, factory) && Factories(owner).Count() is 1
                ? $"{owner.ToDisplayString(SymbolDisplayFormat.MinimallyQualifiedFormat)}.Validated({raw})"
                : $"ValidationResults.Validated<{(Same(owner, factory) ? owner.TypeArguments[0] : owner).ToDisplayString(SymbolDisplayFormat.MinimallyQualifiedFormat)}, {call.TargetMethod.Parameters[0].Type.ToDisplayString(SymbolDisplayFormat.MinimallyQualifiedFormat)}>({raw})";
        Report<IInvocationOperation>(context, [OperationKind.Invocation], (invocation, analysis) =>
            Crosses(invocation.TargetMethod) && !Hooked(analysis.ContainingSymbol) && !Within(analysis.ContainingSymbol, results)
                ? [Diagnostic.Create(Validation, invocation.Syntax.GetLocation(), invocation.Syntax, Validated(invocation))]
                : []);
    }

    // --- [CONVERSIONS]
    private static void Conversions(CompilationStartAnalysisContext context, INamedTypeSymbol error, ISymbol message, INamedTypeSymbol? results) =>
        Report<IPropertyReferenceOperation>(context, [OperationKind.PropertyReference], (reference, analysis) =>
            reference is { Instance: { } source } && Same(reference.Property, message) && !Within(analysis.ContainingSymbol, results)
            && Consumer(reference) is IArgumentOperation { Parent: IInvocationOperation or IObjectCreationOperation } argument && argument.Parent is { } site && Derives(site.Type, error)
                ? [Diagnostic.Create(Conversion, site.Syntax.GetLocation(), site.Syntax, source.Syntax)]
                : []);
}
