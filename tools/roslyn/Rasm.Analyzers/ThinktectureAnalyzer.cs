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
        "Catch-all arm over smart enum or union, call `Switch` or `Map`",
        "Catch-all arm over `{0}` hides unhandled cases, call generated `Switch` or `Map` with one argument per case",
        "Switches over Thinktecture unions or smart enums with an arm naming a case hold no catch-all arm");

    public static readonly DiagnosticDescriptor Validation = Rule(
        "RASM1102",
        Category,
        "Validation through default `ValidationError`, declare `[ValidationError<T>]`",
        "`{0}` returns default `ValidationError`, declare `[ValidationError<T>]` on `{1}` with the package error type as `T`",
        "Thinktecture `Validate` and `TryCreate` calls outside validation hooks and object factories reach no type raising default `ValidationError`");

    public static readonly DiagnosticDescriptor Conversion = Rule(
        "RASM1103",
        Category,
        "`ValidationError` converted to `Error` by hand, declare `[ValidationError<T>]`",
        "`{0}` converts a `ValidationError` message to `Error` by hand, declare `[ValidationError<T>]` with package error `T` on the value object",
        "`ValidationError.Message` reaches no `Error` factory or constructor");

    public static readonly DiagnosticDescriptor Thrown = Rule(
        "RASM1104",
        Category,
        "Throwing factory on caller data, return `Fin` through `Validate`",
        "`{0}.Create` throws on `{1}` from the caller, return `Fin<{0}>` through `Validate`",
        "Generated `Create` on validating value objects and smart enums takes no caller data outside static initializers and `ValidateFactoryArguments`");

    public static readonly DiagnosticDescriptor Plain = Rule(
        "RASM1105",
        Category,
        "Smart enum with no data, move behavior onto items or declare a union",
        "`{0}` {1}, move consumer data onto items or declare a shape-differing record union",
        "`[SmartEnum]` items hold a key or data, and code reads `[SmartEnum<string>]` member-name keys (key, `Get`, `TryGet`, `Validate`, `Parse`)",
        WellKnownDiagnosticTags.CompilationEnd);

    public static readonly DiagnosticDescriptor Picked = Rule(
        "RASM2013",
        Category,
        "Union case per host member, call the member at its caller",
        "`{0}` cases each pick one host member, call host members at their callers inside one convention lift",
        "`[Union]` types with every `Switch` or `Map` lambda one lifted or direct host call or construction over case data",
        WellKnownDiagnosticTags.CompilationEnd);

    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => [CatchAll, Validation, Conversion, Thrown, Plain, Picked];

    protected override void Start(CompilationStartAnalysisContext context) {
        ThinktectureSymbols symbols = new(context.Compilation);
        LanguageExtSymbols languageExt = new(context.Compilation);
        if (symbols is { UnionAttributeBase: { } adHoc, SmartEnumAttribute1: { } keyed }) CatchAlls(context, symbols.Marks, adHoc, keyed);
        if (symbols is { ObjectFactory: { } factory, ValidationError: { } invalid, Validate: { } validate })
            Validations(context, factory, invalid, validate, symbols.SmartEnum);
        if (languageExt.Error is { } error && symbols.Message is { } message) Conversions(context, error, message);
        if (symbols.Validate is { } validating) Creations(context, validating.Name, symbols.Validated, symbols.SmartEnums);
        if (symbols is { SmartEnumAttribute: { } plain, SmartEnumAttribute1: { } stringKeyed }) Enumerations(context, plain, stringKeyed);
        if (symbols.UnionAttribute is { } union) Pickers(context, union, languageExt);
    }

    // --- [CATCH_ALLS]

    private static void CatchAlls(CompilationStartAnalysisContext context, ImmutableArray<INamedTypeSymbol> marks, INamedTypeSymbol adHoc, INamedTypeSymbol keyed) {
        ConcurrentDictionary<ITypeSymbol, AttributeData?> attributes = new(SymbolEqualityComparer.Default);
        AttributeData? Mark(ITypeSymbol type) => attributes.GetOrAdd(type, held => held.GetAttributes().FirstOrDefault(data => marks.Any(mark => Derives(data.AttributeClass, mark))));
        INamedTypeSymbol? Marked(ITypeSymbol? type, CancellationToken token) {
            Stack<ITypeSymbol> pending = new(type is null ? [] : [type]);
            while (pending.Count is > 0) {
                ITypeSymbol current = pending.Pop();
                token.ThrowIfCancellationRequested();
                if (current is INamedTypeSymbol { IsTupleType: true } tuple)
                    for (int index = tuple.TupleElements.Length - 1; index >= 0; index--) pending.Push(tuple.TupleElements[index].Type);
                else if (current is INamedTypeSymbol { OriginalDefinition.SpecialType: SpecialType.System_Nullable_T } nullable) pending.Push(nullable.TypeArguments[0]);
                else if (current is INamedTypeSymbol named && Mark(named) is not null) return named;
            }
            return null;
        }
        (INamedTypeSymbol Owner, Func<IOperation, bool> Case)? Closed(IOperation subject, CancellationToken token) =>
            Stripped(subject) switch {
                IMemberReferenceOperation { Instance.Type: INamedTypeSymbol owner, Member: var member }
                    when Mark(owner) is { AttributeClass: { TypeArguments.Length: 1 } attribute } mark && Same(attribute, keyed)
                        && member is IPropertySymbol or IFieldSymbol && string.Equals(member.Name, Key(mark), StringComparison.Ordinal) =>
                    (owner, static test => test is IConstantPatternOperation or IRelationalPatternOperation || (test is not IPatternOperation && test.ConstantValue.HasValue)),
                IPropertyReferenceOperation { Instance.Type: INamedTypeSymbol owner, Property.Type.SpecialType: SpecialType.System_Object }
                    when Mark(owner) is { AttributeClass: { } union } mark && Derives(union, adHoc)
                        && union.TypeArguments.AddRange(mark.ConstructorArguments.Select(static held => held.Value).OfType<ITypeSymbol>()) is { IsEmpty: false } cases =>
                    (owner, test => test is IPatternOperation { NarrowedType: var narrowed } && cases.Any(held => SymbolEqualityComparer.Default.Equals(held, narrowed))),
                _ when Marked(subject.Type, token) is { } owner => (owner, test => test switch {
                    IPatternOperation { NarrowedType: var narrowed } => !Same(narrowed, owner) && Derives(narrowed, owner),
                    IFieldReferenceOperation { Field: { IsStatic: true, Type: var item } } => Same(item, owner),
                    _ => false,
                }),
                _ => null,
            };
        Report<IOperation>(context, [OperationKind.SwitchExpression, OperationKind.Switch], (operation, analysis) =>
            Dispatch(operation) is ( { } subject, var arms) && Closed(subject, analysis.CancellationToken) is ( { } owner, var names)
            && arms.Any(arm => !CatchesAll(arm, analysis.CancellationToken) && ((arm.Test is { } test && test.DescendantsAndSelf().Any(names)) || (arm.Guard is { } guard && guard.DescendantsAndSelf().Any(names))))
                ? arms.Where(arm => CatchesAll(arm, analysis.CancellationToken)).Select(arm => Diagnostic.Create(CatchAll, arm.Arm.Syntax.GetLocation(), owner.Name))
                : []);
    }

    // --- [VALIDATIONS]

    private static void Validations(CompilationStartAnalysisContext context, INamedTypeSymbol factory, INamedTypeSymbol invalid, ISymbol validate, INamedTypeSymbol? items) {
        IEnumerable<INamedTypeSymbol> Factories(INamedTypeSymbol owner) =>
            Faces(owner, factory).Where(face => Same(face.TypeArguments[0], owner) && Same(face.TypeArguments[2], invalid));
        bool Implements(IMethodSymbol method, INamedTypeSymbol owner, IEnumerable<INamedTypeSymbol> faces, Func<ISymbol, bool> wanted) =>
            faces.SelectMany(static face => face.GetMembers()).Any(member => wanted(member) && Same(owner.FindImplementationForInterfaceMember(member), method));
        bool Creates(IMethodSymbol method, INamedTypeSymbol owner) =>
            method is { ReturnType.SpecialType: SpecialType.System_Boolean, Parameters: { Length: 2 } parameters } && parameters[1] is { RefKind: RefKind.Out, Type: var made } && Same(made, owner)
            && !Implements(method, owner, Faces(owner, items), static member => member.IsStatic)
            && Factories(owner).Any(face => Same(face.TypeArguments[1], parameters[0].Type));
        bool Crosses(IMethodSymbol method) =>
            (Same(method, validate) && Same(method.ContainingType.TypeArguments[2], invalid))
            || (method is { IsStatic: true, ContainingType: { } owner } && (Implements(method, owner, Factories(owner), member => Same(member, validate)) || Creates(method, owner)));
        bool Hooked(ISymbol owner) =>
            owner is IMethodSymbol method
            && ((method.ReturnType is { NullableAnnotation: NullableAnnotation.Annotated } returned && Same(returned, invalid))
                || method.Parameters.Any(parameter => parameter.RefKind is RefKind.Ref && Same(parameter.Type, invalid)));
        static ITypeSymbol Validated(IInvocationOperation call) => call.TargetMethod.ContainingType is { TypeKind: TypeKind.Interface } face ? face.TypeArguments[0] : call.TargetMethod.ContainingType;
        Report<IInvocationOperation>(context, [OperationKind.Invocation], (invocation, analysis) =>
            Crosses(invocation.TargetMethod) && !Hooked(analysis.ContainingSymbol)
                ? [Diagnostic.Create(Validation, invocation.Syntax.GetLocation(), invocation.Syntax, Validated(invocation).ToDisplayString(SymbolDisplayFormat.MinimallyQualifiedFormat))]
                : []);
    }

    // --- [CONVERSIONS]

    private static void Conversions(CompilationStartAnalysisContext context, INamedTypeSymbol error, ISymbol message) =>
        Report<IPropertyReferenceOperation>(context, [OperationKind.PropertyReference], (reference, _) =>
            reference is { Instance: not null } && Same(reference.Property, message)
            && Consumer(reference) is IArgumentOperation { Parent: IInvocationOperation or IObjectCreationOperation } argument && argument.Parent is { } site && Derives(site.Type, error)
                ? [Diagnostic.Create(Conversion, site.Syntax.GetLocation(), site.Syntax)]
                : []);

    // --- [CREATIONS]

    private static void Creations(CompilationStartAnalysisContext context, string validator, ImmutableArray<INamedTypeSymbol> validated, ImmutableArray<INamedTypeSymbol> enums) {
        bool Validating(INamedTypeSymbol owner) =>
            Marked(owner, validated) is not null
            && (Marked(owner, enums) is not null || owner.GetMembers(Hook).OfType<IMethodSymbol>().Any(static hook => hook.PartialImplementationPart is not null || !hook.IsPartialDefinition));
        bool Throwing(IMethodSymbol method) =>
            method is { IsStatic: true, Name: "Create", ContainingType: { } owner } && Same(method.ReturnType, owner)
            && owner.GetMembers(validator).OfType<IMethodSymbol>().Any(validate => validate is { IsStatic: true, Parameters: { IsEmpty: false } parameters } && parameters[parameters.Length - 1] is { RefKind: RefKind.Out } made && Same(made.Type, owner))
            && Validating(owner);
        static string Passed(IMethodReferenceOperation group) =>
            (Consumer(group) is IDelegateCreationOperation creation && Consumer(creation) is IArgumentOperation { Parent: IInvocationOperation call } && Receiver(call) is { } receiver
                ? receiver.Syntax
                : group.Syntax).ToString();
        Report<IOperation>(context, [OperationKind.Invocation, OperationKind.MethodReference], (operation, analysis) =>
            analysis.ContainingSymbol.Name is Hook || Enclosing(analysis.ContainingSymbol) is IFieldSymbol { IsStatic: true } or IPropertySymbol { IsStatic: true } ? []
            : operation switch {
                IInvocationOperation invocation when Throwing(invocation.TargetMethod)
                    && invocation.Arguments.SelectMany(static argument => argument.Value.DescendantsAndSelf()).Select(Variable).FirstOrDefault(static held => held is not (null or ILocalSymbol { HasConstantValue: true })) is { } read =>
                    [Diagnostic.Create(Thrown, invocation.Syntax.GetLocation(), invocation.TargetMethod.ContainingType.Name, read.Name)],
                IMethodReferenceOperation group when Throwing(group.Method) && !Inside<INameOfOperation>(group) =>
                    [Diagnostic.Create(Thrown, group.Syntax.GetLocation(), group.Method.ContainingType.Name, Passed(group))],
                _ => [],
            });
    }

    // --- [ENUMERATIONS]

    private static void Enumerations(CompilationStartAnalysisContext context, INamedTypeSymbol plain, INamedTypeSymbol keyed) {
        ConcurrentDictionary<INamedTypeSymbol, (bool Plain, bool Items)> free = new(SymbolEqualityComparer.Default);
        ConcurrentDictionary<INamedTypeSymbol, byte> read = new(SymbolEqualityComparer.Default);
        ConcurrentDictionary<INamedTypeSymbol, (bool Keyed, ImmutableArray<ISymbol> Readers)?> forms = new(SymbolEqualityComparer.Default);
        ImmutableArray<ISymbol> allowed = [plain, keyed, .. new[] {
            context.Compilation.GetTypeByMetadataName("Thinktecture.KeyMemberEqualityComparerAttribute`2"),
            context.Compilation.GetTypeByMetadataName("Thinktecture.KeyMemberComparerAttribute`2"),
        }.OfType<INamedTypeSymbol>()];
        (bool Keyed, ImmutableArray<ISymbol> Readers)? Form(INamedTypeSymbol type) =>
            forms.GetOrAdd(type, symbol => symbol.GetAttributes().Select(data => data.AttributeClass switch {
                { } held when Same(held, plain) => (false, ImmutableArray<ISymbol>.Empty),
                { TypeArguments: { Length: 1 } arguments } held when Same(held, keyed) && arguments[0].SpecialType is SpecialType.System_String =>
                    (true, ImmutableArray.CreateRange(symbol.GetMembers(Key(data)).Concat(symbol.AllInterfaces.SelectMany(static face => face.GetMembers()).OfType<IMethodSymbol>()
                        .Where(method => method.IsStatic && (Same(method.ReturnType, symbol) || method.Parameters.Any(parameter => parameter.RefKind is RefKind.Out && Same(parameter.Type, symbol))))
                        .Select(symbol.FindImplementationForInterfaceMember).OfType<ISymbol>()))),
                _ => ((bool, ImmutableArray<ISymbol>)?)null,
            }).FirstOrDefault(static form => form is not null));
        static bool Named(string literal, string member) => string.Equals(literal.Replace("-", ""), member, StringComparison.OrdinalIgnoreCase);
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
                if (node.IsGeneratedCode || node.SemanticModel.GetDeclaredSymbol(declaration, node.CancellationToken) is not { } type || Form(type) is not (var keyedForm, _)) return;
                bool held = declaration.Members.All(member => member switch {
                    FieldDeclarationSyntax field => Item(field, type, keyedForm, node.SemanticModel, node.CancellationToken),
                    ConstructorDeclarationSyntax { ParameterList.Parameters.Count: <= 1, ExpressionBody: null, Body: null or { Statements.Count: 0 } } => true,
                    _ => false,
                })
                    && declaration.AttributeLists.SelectMany(static list => list.Attributes).All(attribute => Among(node.SemanticModel.GetSymbolInfo(attribute, node.CancellationToken).Symbol?.ContainingType, allowed));
                bool items = declaration.Members.Any(static member => member is FieldDeclarationSyntax);
                _ = free.AddOrUpdate(type, (held, items), (_, prior) => (prior.Plain && held, prior.Items || items));
            },
            SyntaxKind.ClassDeclaration);
        context.RegisterOperationAction(
            analysis => {
                if (!analysis.IsGeneratedCode && Target(analysis.Operation) is { ContainingType: { } owner } member && Form(owner) is (true, var readers) && Among(member, readers)) read[owner] = 1;
            },
            OperationKind.Invocation,
            OperationKind.PropertyReference,
            OperationKind.FieldReference);
        context.RegisterCompilationEndAction(end => {
            foreach (KeyValuePair<INamedTypeSymbol, (bool Plain, bool Items)> pair in free.Where(static pair => pair.Value is (true, true)))
                if (Form(pair.Key) is (var keyedForm, _) && (!keyedForm || !read.ContainsKey(pair.Key)))
                    end.ReportDiagnostic(Diagnostic.Create(
                        Plain,
                        pair.Key.Locations[0],
                        pair.Key.Name,
                        keyedForm
                            ? "keys items by unread member names"
                            : "holds data-free items"));
        });
    }

    // --- [PICKERS]

    private static void Pickers(CompilationStartAnalysisContext context, INamedTypeSymbol union, LanguageExtSymbols languageExt) {
        Compilation compilation = context.Compilation;
        ImmutableArray<IMethodSymbol> lifts = [.. languageExt.Lifts, .. languageExt.Uses];
        bool Hosted(ISymbol target) =>
            !Same(target.ContainingAssembly, compilation.Assembly)
            && !Namespaced(compilation, target, languageExt.Namespace);
        IAnonymousFunctionOperation? Thunk(IOperation value) =>
            Lambda(value) ?? (Stripped(value) is IInvocationOperation { Arguments.Length: 1 } call && Among(call.TargetMethod, lifts)
                && call.TargetMethod.OriginalDefinition.Parameters[0].Type is INamedTypeSymbol { DelegateInvokeMethod: not null } ? Lambda(call.Arguments[0].Value) : null);
        bool Picks(IAnonymousFunctionOperation function, INamedTypeSymbol owner, CancellationToken token) {
            HashSet<IParameterSymbol> held = [with(SymbolEqualityComparer.Default)];
            bool Read(IOperation value) =>
                Stripped(value) switch {
                    IParameterReferenceOperation reference => held.Contains(reference.Parameter),
                    IPropertyReferenceOperation { Instance: IParameterReferenceOperation reference } => held.Contains(reference.Parameter) && !Same(reference.Parameter.Type, owner) && Derives(reference.Parameter.Type, owner),
                    _ => false,
                };
            while (true) {
                token.ThrowIfCancellationRequested();
                held.UnionWith(function.Symbol.Parameters);
                if (Sole(function.Body) is not { } value) return false;
                if (Thunk(value) is { } inner) { function = inner; continue; }
                return value switch {
                    IInvocationOperation call => (Target: call.TargetMethod, call.Arguments),
                    IObjectCreationOperation { Constructor: { } made } creation => (Target: made, creation.Arguments),
                    _ => default,
                } is ( { } target, var arguments) && Hosted(target) && arguments.Where(static argument => argument.ArgumentKind is ArgumentKind.Explicit).All(argument => Read(argument.Value));
            }
        }
        ConcurrentDictionary<INamedTypeSymbol, bool> picked = new(SymbolEqualityComparer.Default);
        context.RegisterOperationAction(
            analysis => {
                if (analysis.IsGeneratedCode || analysis.Operation is not IInvocationOperation { TargetMethod: { Name: "Switch" or "Map", ContainingType: { } owner } } call || !Sourced(owner) || Marked(owner, union) is null) return;
                ImmutableArray<IOperation> arms = [.. call.Arguments.Where(static argument => argument.Parameter?.Type.TypeKind is TypeKind.Delegate).Select(static argument => Stripped(argument.Value))];
                if (arms.IsEmpty) return;
                bool each = arms.All(arm => Lambda(arm) is { } function && Picks(function, owner, analysis.CancellationToken));
                _ = picked.AddOrUpdate(owner.OriginalDefinition, each, (_, prior) => prior && each);
            },
            OperationKind.Invocation);
        context.RegisterCompilationEndAction(end => {
            foreach (KeyValuePair<INamedTypeSymbol, bool> pair in picked.Where(static pair => pair.Value))
                end.ReportDiagnostic(Diagnostic.Create(Picked, pair.Key.Locations[0], pair.Key.Name));
        });
    }

    // --- [KEYS]

    private static string Key(AttributeData mark) {
        bool Selected(string option, string member) => mark.NamedArguments.FirstOrDefault(named => string.Equals(named.Key, option, StringComparison.Ordinal)).Value is { Type: INamedTypeSymbol type, Value: { } value }
            && type.GetMembers(member).OfType<IFieldSymbol>().Any(field => Equals(field.ConstantValue, value));
        return mark.NamedArguments.Where(static named => named.Key is "KeyMemberName").Select(static named => named.Value.Value).OfType<string>().FirstOrDefault()
            ?? (Selected("KeyMemberAccessModifier", "Private") && Selected("KeyMemberKind", "Field") ? "_key" : "Key");
    }
}
