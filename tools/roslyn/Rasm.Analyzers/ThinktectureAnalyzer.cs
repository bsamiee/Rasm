using System.Collections.Concurrent;
using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Operations;

namespace Rasm.Analyzers;

// --- [COMPOSITION] ---------------------------------------------------------------------
[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class ThinktectureAnalyzer : RasmAnalyzer {
    // --- [RULES]
    private const string Category = "Thinktecture";

    private const string Hook = "ValidateFactoryArguments";

    public static readonly DiagnosticDescriptor CatchAll = Rule(
        "RASM1101",
        Category,
        "Catch-all arm over smart enum or union",
        "Catch-all arm over `{0}` hides unhandled cases, call generated `Switch` or `Map` with one argument per case",
        "Catch-all arm in a `switch` over a Thinktecture union or smart enum with other arms naming a case.");

    public static readonly DiagnosticDescriptor Validation = Rule(
        "RASM1102",
        Category,
        "Validation through default ValidationError",
        "`{0}` returns default `ValidationError`, declare `[ValidationError<Invalid>]` on `{1}` so `Validate` returns the package error that converts to `Fin`",
        "Thinktecture `Validate` or `TryCreate` call on a type raising default `ValidationError`, outside a validation hook or object factory.");

    public static readonly DiagnosticDescriptor Conversion = Rule(
        "RASM1103",
        Category,
        "ValidationError converted to Error by hand",
        "`{0}` converts a `ValidationError` message to an `Error` by hand, declare `[ValidationError<Invalid>]` on the value object so `Validate` returns the package error that converts to `Fin`",
        "`ValidationError.Message` passed to an `Error` factory or constructor.");

    public static readonly DiagnosticDescriptor Thrown = Rule(
        "RASM1104",
        Category,
        "Throwing factory on caller data",
        "`{0}.Create` throws on `{1}` from the caller, return `Fin<{0}>` through `Validate`",
        "Generated `Create` call on a validating value object or smart enum with an argument reading a parameter, outside `ValidateFactoryArguments`.");

    public static readonly DiagnosticDescriptor Plain = Rule(
        "RASM1105",
        Category,
        "Smart enum with no data",
        "Smart enum `{0}` {1}",
        "`[SmartEnum]` type whose items carry no key or data, or `[SmartEnum<string>]` type whose keys are its member names with no read of a key, `Get`, `TryGet`, `Validate`, or `Parse` in the compilation.",
        WellKnownDiagnosticTags.CompilationEnd);

    public static readonly DiagnosticDescriptor Picked = Rule(
        "RASM2013",
        Category,
        "Union case per host member",
        "`{0}` cases each pick one host member, call the member at the caller inside one convention lift",
        "`[Union]` type with every `Switch` or `Map` lambda one referenced-assembly call or construction over case properties and lambda parameters, through `IO.lift`, `use`, or thunk lambdas.",
        WellKnownDiagnosticTags.CompilationEnd);

    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => [CatchAll, Validation, Conversion, Thrown, Plain, Picked];

    protected override void Start(CompilationStartAnalysisContext context) {
        ThinktectureSymbols symbols = new(context.Compilation);
        LanguageExtSymbols languageExt = new(context.Compilation);
        if (symbols is { UnionAttributeBase: { } adHoc, SmartEnumAttribute1: { } keyed }) CatchAlls(context, symbols.Marks, adHoc, keyed);
        if (symbols is { ObjectFactory: { } factory, ValidationError: { } invalid, Validate: { } validate })
            Validations(context, factory, invalid, validate, symbols.SmartEnum, symbols.ValidationResults);
        if (languageExt.Error is { } error && symbols.Message is { } message) Conversions(context, error, message, symbols.ValidationResults);
        if (symbols.Validate is { } validating) Creations(context, validating.Name, symbols.Validated, [.. new[] { symbols.SmartEnumAttribute, symbols.SmartEnumAttribute1 }.OfType<INamedTypeSymbol>()]);
        if (symbols is { SmartEnumAttribute: { } plain, SmartEnumAttribute1: { } stringKeyed }) Enumerations(context, plain, stringKeyed);
        if (symbols.UnionAttribute is { } union) Pickers(context, union, languageExt);
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
        static ITypeSymbol Validated(IInvocationOperation call) => call.TargetMethod.ContainingType is { TypeKind: TypeKind.Interface } face ? face.TypeArguments[0] : call.TargetMethod.ContainingType;
        Report<IInvocationOperation>(context, [OperationKind.Invocation], (invocation, analysis) =>
            Crosses(invocation.TargetMethod) && !Hooked(analysis.ContainingSymbol) && !Within(analysis.ContainingSymbol, results)
                ? [Diagnostic.Create(Validation, invocation.Syntax.GetLocation(), invocation.Syntax, Validated(invocation).ToDisplayString(SymbolDisplayFormat.MinimallyQualifiedFormat))]
                : []);
    }

    // --- [CONVERSIONS]
    private static void Conversions(CompilationStartAnalysisContext context, INamedTypeSymbol error, ISymbol message, INamedTypeSymbol? results) =>
        Report<IPropertyReferenceOperation>(context, [OperationKind.PropertyReference], (reference, analysis) =>
            reference is { Instance: not null } && Same(reference.Property, message) && !Within(analysis.ContainingSymbol, results)
            && Consumer(reference) is IArgumentOperation { Parent: IInvocationOperation or IObjectCreationOperation } argument && argument.Parent is { } site && Derives(site.Type, error)
                ? [Diagnostic.Create(Conversion, site.Syntax.GetLocation(), site.Syntax)]
                : []);

    // --- [CREATIONS]
    private static void Creations(CompilationStartAnalysisContext context, string validator, ImmutableArray<INamedTypeSymbol> validated, ImmutableArray<INamedTypeSymbol> enums) {
        bool Carries(INamedTypeSymbol owner, ImmutableArray<INamedTypeSymbol> marks) => owner.GetAttributes().Any(data => marks.Any(mark => Same(data.AttributeClass, mark)));
        bool Validating(INamedTypeSymbol owner) =>
            Carries(owner, validated)
            && (Carries(owner, enums) || owner.GetMembers(Hook).OfType<IMethodSymbol>().Any(static hook => hook.PartialImplementationPart is not null || !hook.IsPartialDefinition));
        bool Throwing(IMethodSymbol method) =>
            method is { IsStatic: true, Name: "Create", ContainingType: { } owner } && Same(method.ReturnType, owner)
            && owner.GetMembers(validator).OfType<IMethodSymbol>().Any(validate => validate is { IsStatic: true, Parameters: { IsEmpty: false } parameters } && parameters[parameters.Length - 1] is { RefKind: RefKind.Out } made && Same(made.Type, owner))
            && Validating(owner);
        Report<IInvocationOperation>(context, [OperationKind.Invocation], (invocation, analysis) =>
            Throwing(invocation.TargetMethod) && analysis.ContainingSymbol.Name is not Hook
            && invocation.Arguments.SelectMany(static argument => argument.Value.DescendantsAndSelf()).Select(static held => held switch {
                IParameterReferenceOperation reference => reference.Parameter,
                ILocalReferenceOperation { Local: { HasConstantValue: false } local } => (ISymbol)local,
                _ => null,
            }).OfType<ISymbol>().FirstOrDefault() is { } read
                ? [Diagnostic.Create(Thrown, invocation.Syntax.GetLocation(), invocation.TargetMethod.ContainingType.Name, read.Name)]
                : []);
    }

    // --- [ENUMERATIONS]
    private static void Enumerations(CompilationStartAnalysisContext context, INamedTypeSymbol plain, INamedTypeSymbol keyed) {
        ConcurrentDictionary<INamedTypeSymbol, bool> free = new(SymbolEqualityComparer.Default);
        ConcurrentDictionary<INamedTypeSymbol, byte> read = new(SymbolEqualityComparer.Default);
        static string Key(AttributeData mark) => mark.NamedArguments.Where(static named => named.Key is "KeyMemberName").Select(static named => named.Value.Value).OfType<string>().FirstOrDefault() ?? "Key";
        (AttributeData Mark, bool Keyed)? Form(INamedTypeSymbol type) =>
            type.GetAttributes().Select(data => data.AttributeClass switch {
                { } held when Same(held, plain) => (data, false),
                { TypeArguments: { Length: 1 } arguments } held when Same(held, keyed) && arguments[0].SpecialType is SpecialType.System_String => (data, true),
                _ => ((AttributeData, bool)?)null,
            }).FirstOrDefault(static form => form is not null);
        static bool Named(string literal, string member) => string.Equals(literal.Replace("-", ""), member, StringComparison.OrdinalIgnoreCase);
        static bool Allowed(AttributeSyntax attribute, SemanticModel model, CancellationToken token) =>
            model.GetSymbolInfo(attribute, token).Symbol?.ContainingType is { ContainingNamespace.Name: "Thinktecture", Name: "SmartEnumAttribute" or "KeyMemberEqualityComparerAttribute" or "KeyMemberComparerAttribute" };
        static bool Item(FieldDeclarationSyntax field, INamedTypeSymbol owner, bool keyedForm, SemanticModel model, CancellationToken token) =>
            field.Modifiers.Any(SyntaxKind.StaticKeyword) && field.Declaration.Variables.All(variable =>
                variable.Initializer?.Value is BaseObjectCreationExpressionSyntax { Initializer: null } creation && Same(model.GetTypeInfo(creation, token).Type, owner)
                && (creation.ArgumentList?.Arguments ?? default) is var arguments
                && (keyedForm
                    ? arguments.Count is 1 && arguments[0].Expression is LiteralExpressionSyntax { Token.Value: string key } && Named(key, variable.Identifier.ValueText)
                    : arguments.Count is 0));
        context.RegisterSyntaxNodeAction(
            node => {
                ClassDeclarationSyntax declaration = (ClassDeclarationSyntax)node.Node;
                if (node.IsGeneratedCode || node.SemanticModel.GetDeclaredSymbol(declaration, node.CancellationToken) is not { } type || Form(type) is not (_, var keyedForm)) return;
                bool held = declaration.Members.Any(static member => member is FieldDeclarationSyntax)
                    && declaration.Members.All(member => member switch {
                        FieldDeclarationSyntax field => Item(field, type, keyedForm, node.SemanticModel, node.CancellationToken),
                        ConstructorDeclarationSyntax { ParameterList.Parameters.Count: <= 1, ExpressionBody: null, Body: null or { Statements.Count: 0 } } => true,
                        _ => false,
                    })
                    && declaration.AttributeLists.SelectMany(static list => list.Attributes).All(attribute => Allowed(attribute, node.SemanticModel, node.CancellationToken));
                _ = free.AddOrUpdate(type, held, (_, prior) => prior && held);
            },
            SyntaxKind.ClassDeclaration);
        context.RegisterOperationAction(
            analysis => {
                (ISymbol? Member, INamedTypeSymbol? Owner) found = analysis.Operation switch {
                    IInvocationOperation { TargetMethod: { IsExtensionMethod: true, Parameters: { Length: > 0 } parameters } extension } when parameters[0].Type is INamedTypeSymbol receiver => (extension, receiver),
                    IInvocationOperation { TargetMethod: var method } => (method, method.ContainingType),
                    IPropertyReferenceOperation { Property: var property } => (property, property.ContainingType),
                    _ => (null, null),
                };
                if (found is not ( { } member, { } owner) || Form(owner) is not ( { } mark, true)) return;
                bool keyRead = member.Name is "Get" or "TryGet" or "Validate" or "Validated" or "Parse" or "TryParse" || string.Equals(member.Name, Key(mark), StringComparison.Ordinal);
                if (keyRead) read[owner] = 1;
            },
            OperationKind.Invocation,
            OperationKind.PropertyReference);
        context.RegisterCompilationEndAction(end => {
            foreach (KeyValuePair<INamedTypeSymbol, bool> pair in free.Where(static pair => pair.Value))
                if (Form(pair.Key) is (_, var keyedForm) && (!keyedForm || !read.ContainsKey(pair.Key)))
                    end.ReportDiagnostic(Diagnostic.Create(
                        Plain,
                        pair.Key.Locations[0],
                        pair.Key.Name,
                        keyedForm
                            ? "keys its items by their member names and nothing reads a key, declare it as a plain `enum` under `JsonStringEnumConverter(JsonNamingPolicy.KebabCaseLower)`"
                            : "holds items with no key, data, or behavior, declare it as a plain `enum`"));
        });
    }

    // --- [PICKERS]
    private static void Pickers(CompilationStartAnalysisContext context, INamedTypeSymbol union, LanguageExtSymbols languageExt) {
        Compilation compilation = context.Compilation;
        ImmutableArray<IMethodSymbol> lifts = [.. Members(languageExt.IO, "lift"), .. languageExt.Uses];
        bool Hosted(ISymbol target) =>
            !Same(target.ContainingAssembly, compilation.Assembly)
            && (languageExt.Namespace is not { } space || !Within(compilation.GetCompilationNamespace(target.ContainingNamespace), space));
        IAnonymousFunctionOperation? Thunk(IOperation value) =>
            Stripped(value) switch {
                IDelegateCreationOperation { Target: IAnonymousFunctionOperation inner } => inner,
                IInvocationOperation { Arguments.Length: 1 } call when Among(call.TargetMethod, lifts) && Stripped(call.Arguments[0].Value) is IDelegateCreationOperation { Target: IAnonymousFunctionOperation inner } => inner,
                _ => null,
            };
        static IOperation? Single(IAnonymousFunctionOperation function) =>
            function.Body.Operations.Where(static step => step is not IReturnOperation { ReturnedValue: null, IsImplicit: true }).ToImmutableArray() is { Length: 1 } steps
                ? steps[0] switch {
                    IReturnOperation { ReturnedValue: { } returned } => returned,
                    IExpressionStatementOperation statement => statement.Operation,
                    _ => null,
                }
                : null;
        bool Picks(IAnonymousFunctionOperation function, INamedTypeSymbol owner, ImmutableArray<IParameterSymbol> bound) {
            ImmutableArray<IParameterSymbol> held = bound.AddRange(function.Symbol.Parameters);
            bool Read(IOperation value) =>
                Stripped(value) switch {
                    IParameterReferenceOperation reference => held.Contains(reference.Parameter, SymbolEqualityComparer.Default),
                    IPropertyReferenceOperation { Instance: IParameterReferenceOperation reference } => held.Any(parameter => SymbolEqualityComparer.Default.Equals(parameter, reference.Parameter) && !Same(parameter.Type, owner) && Derives(parameter.Type, owner)),
                    _ => false,
                };
            return Single(function) is { } value && (Thunk(value) is { } inner ? Picks(inner, owner, held) : Stripped(value) switch {
                IInvocationOperation call => Hosted(call.TargetMethod) && call.Arguments.Where(static argument => argument.ArgumentKind is ArgumentKind.Explicit).All(argument => Read(argument.Value)),
                IObjectCreationOperation { Constructor: { } made } creation => Hosted(made) && creation.Arguments.Where(static argument => argument.ArgumentKind is ArgumentKind.Explicit).All(argument => Read(argument.Value)),
                _ => false,
            });
        }
        ConcurrentDictionary<INamedTypeSymbol, bool> picked = new(SymbolEqualityComparer.Default);
        context.RegisterOperationAction(
            analysis => {
                if (analysis.Operation is not IInvocationOperation { TargetMethod: { Name: "Switch" or "Map", ContainingType: { } owner } } call || !Sourced(owner) || Marked(owner, union) is null) return;
                ImmutableArray<IOperation> arms = [.. call.Arguments.Where(static argument => argument.Parameter?.Type.TypeKind is TypeKind.Delegate).Select(static argument => Stripped(argument.Value))];
                if (arms.IsEmpty) return;
                bool each = arms.All(arm => arm is IDelegateCreationOperation { Target: IAnonymousFunctionOperation function } && Picks(function, owner, []));
                _ = picked.AddOrUpdate(owner.OriginalDefinition, each, (_, prior) => prior && each);
            },
            OperationKind.Invocation);
        context.RegisterCompilationEndAction(end => {
            foreach (KeyValuePair<INamedTypeSymbol, bool> pair in picked.Where(static pair => pair.Value))
                end.ReportDiagnostic(Diagnostic.Create(Picked, pair.Key.Locations[0], pair.Key.Name));
        });
    }
}
