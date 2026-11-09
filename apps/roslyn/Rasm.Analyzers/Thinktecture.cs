using System.Collections.Concurrent;
using System.Collections.Immutable;
using Analyzer.Utilities;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Operations;

namespace Rasm.Analyzers;

// --- [COMPOSITION] ---------------------------------------------------------------------

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class DefaultValidationErrorAnalyzer : DiagnosticAnalyzer {

    // --- [RULES]

    private static readonly DiagnosticDescriptor Rule = new(
        "RASM1102",
        "Validation through default `ValidationError`, declare `[ValidationError<T>]`",
        "`{0}` returns default `ValidationError`, declare `[ValidationError<T>]` on `{1}` with the package error type as `T`",
        Categories.Thinktecture,
        DiagnosticSeverity.Error,
        isEnabledByDefault: true,
        "Thinktecture `Validate` and `TryCreate` calls outside validation hooks and object factories reach no type raising default `ValidationError`.",
        Link("RASM1102"));

    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => [Rule];

    public override void Initialize(AnalysisContext context) {
        context.EnableConcurrentExecution();
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
        context.RegisterCompilationStartAction(static start => {
            WellKnownTypeProvider types = WellKnownTypeProvider.GetOrCreate(start.Compilation);
            if (types.GetOrCreateTypeByMetadataName("Thinktecture.IObjectFactory`3") is not { } factory || types.GetOrCreateTypeByMetadataName("Thinktecture.ValidationError") is not { } invalid
                || Declared(start.Compilation, "M:Thinktecture.IObjectFactory`3.Validate(`1,System.IFormatProvider,`0@)").FirstOrDefault() is not { } validate) return;
            INamedTypeSymbol? items = types.GetOrCreateTypeByMetadataName("Thinktecture.ISmartEnum`3");
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
            Report<IInvocationOperation>(start, [OperationKind.Invocation], (invocation, analysis) =>
                Crosses(invocation.TargetMethod) && !Hooked(analysis.ContainingSymbol)
                    ? [Diagnostic.Create(Rule, invocation.Syntax.GetLocation(), invocation.Syntax, Validated(invocation).ToDisplayString(SymbolDisplayFormat.MinimallyQualifiedFormat))]
                    : []);
        });
    }
}

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class ValidationMessageAnalyzer : DiagnosticAnalyzer {

    // --- [RULES]

    private static readonly DiagnosticDescriptor Rule = new(
        "RASM1103",
        "`ValidationError` converted to `Error` by hand, declare `[ValidationError<T>]`",
        "`{0}` converts a `ValidationError` message to `Error` by hand, declare `[ValidationError<T>]` with package error `T` on the value object",
        Categories.Thinktecture,
        DiagnosticSeverity.Error,
        isEnabledByDefault: true,
        "`ValidationError.Message` reaches no `Error` factory or constructor.",
        Link("RASM1103"));

    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => [Rule];

    public override void Initialize(AnalysisContext context) {
        context.EnableConcurrentExecution();
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
        context.RegisterCompilationStartAction(static start => {
            if (WellKnownTypeProvider.GetOrCreate(start.Compilation).GetOrCreateTypeByMetadataName("LanguageExt.Common.Error") is not { } error
                || Declared(start.Compilation, "P:Thinktecture.ValidationError.Message").FirstOrDefault() is not { } message) return;
            Report<IPropertyReferenceOperation>(start, [OperationKind.PropertyReference], (reference, _) =>
                reference is { Instance: not null } && Same(reference.Property, message)
                && Consumer(reference) is IArgumentOperation { Parent: IInvocationOperation or IObjectCreationOperation } argument && argument.Parent is { } site && Derives(site.Type, error)
                    ? [Diagnostic.Create(Rule, site.Syntax.GetLocation(), site.Syntax)]
                    : []);
        });
    }
}

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class ThrowingFactoryAnalyzer : DiagnosticAnalyzer {

    // --- [RULES]

    private const string Hook = "ValidateFactoryArguments";

    private static readonly DiagnosticDescriptor Rule = new(
        "RASM1104",
        "Throwing factory on caller data, return `Fin` through `Validate`",
        "`{0}.Create` throws on `{1}` from the caller, return `Fin<{0}>` through `Validate`",
        Categories.Thinktecture,
        DiagnosticSeverity.Error,
        isEnabledByDefault: true,
        "Generated `Create` on validating value objects and smart enums takes no caller data outside static initializers and `ValidateFactoryArguments`.",
        Link("RASM1104"));

    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => [Rule];

    public override void Initialize(AnalysisContext context) {
        context.EnableConcurrentExecution();
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
        context.RegisterCompilationStartAction(static start => {
            if (Declared(start.Compilation, "M:Thinktecture.IObjectFactory`3.Validate(`1,System.IFormatProvider,`0@)").FirstOrDefault() is not { Name: var validator }) return;
            WellKnownTypeProvider types = WellKnownTypeProvider.GetOrCreate(start.Compilation);
            ImmutableArray<INamedTypeSymbol> enums = [.. new[] { types.GetOrCreateTypeByMetadataName("Thinktecture.SmartEnumAttribute"), types.GetOrCreateTypeByMetadataName("Thinktecture.SmartEnumAttribute`1") }.OfType<INamedTypeSymbol>()];
            ImmutableArray<INamedTypeSymbol> validated = [
                .. new[] { types.GetOrCreateTypeByMetadataName("Thinktecture.ValueObjectAttribute`1"), types.GetOrCreateTypeByMetadataName("Thinktecture.ComplexValueObjectAttribute") }.OfType<INamedTypeSymbol>(),
                .. enums,
            ];
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
            Report<IOperation>(start, [OperationKind.Invocation, OperationKind.MethodReference], (operation, analysis) =>
                analysis.ContainingSymbol.Name is Hook || Enclosing(analysis.ContainingSymbol) is IFieldSymbol { IsStatic: true } or IPropertySymbol { IsStatic: true } ? []
                : operation switch {
                    IInvocationOperation invocation when Throwing(invocation.TargetMethod)
                        && invocation.Arguments.SelectMany(static argument => argument.Value.DescendantsAndSelf()).Select(Variable).FirstOrDefault(static held => held is not (null or ILocalSymbol { HasConstantValue: true })) is { } read =>
                        [Diagnostic.Create(Rule, invocation.Syntax.GetLocation(), invocation.TargetMethod.ContainingType.Name, read.Name)],
                    IMethodReferenceOperation group when Throwing(group.Method) && !Inside<INameOfOperation>(group) =>
                        [Diagnostic.Create(Rule, group.Syntax.GetLocation(), group.Method.ContainingType.Name, Passed(group))],
                    _ => [],
                });
        });
    }
}

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class DataFreeSmartEnumAnalyzer : DiagnosticAnalyzer {

    // --- [RULES]

    private static readonly DiagnosticDescriptor Rule = new(
        "RASM1105",
        "Smart enum with no data, move behavior onto items or declare a union",
        "`{0}` {1}, move consumer data onto items or declare a shape-differing record union",
        Categories.Thinktecture,
        DiagnosticSeverity.Error,
        isEnabledByDefault: true,
        "`[SmartEnum]` items hold a key or data, and code reads `[SmartEnum<string>]` member-name keys (key, `Get`, `TryGet`, `Validate`, `Parse`).",
        Link("RASM1105"),
        WellKnownDiagnosticTags.CompilationEnd);

    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => [Rule];

    public override void Initialize(AnalysisContext context) {
        context.EnableConcurrentExecution();
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
        context.RegisterCompilationStartAction(static start => {
            WellKnownTypeProvider types = WellKnownTypeProvider.GetOrCreate(start.Compilation);
            if (types.GetOrCreateTypeByMetadataName("Thinktecture.SmartEnumAttribute") is not { } plain || types.GetOrCreateTypeByMetadataName("Thinktecture.SmartEnumAttribute`1") is not { } keyed) return;
            ConcurrentDictionary<INamedTypeSymbol, (bool Plain, bool Items)> free = new(SymbolEqualityComparer.Default);
            ConcurrentDictionary<INamedTypeSymbol, byte> read = new(SymbolEqualityComparer.Default);
            ConcurrentDictionary<INamedTypeSymbol, (bool Keyed, ImmutableArray<ISymbol> Readers)?> forms = new(SymbolEqualityComparer.Default);
            ImmutableArray<ISymbol> allowed = [plain, keyed, .. new[] {
                types.GetOrCreateTypeByMetadataName("Thinktecture.KeyMemberEqualityComparerAttribute`2"),
                types.GetOrCreateTypeByMetadataName("Thinktecture.KeyMemberComparerAttribute`2"),
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
            start.RegisterSyntaxNodeAction(
                node => {
                    ClassDeclarationSyntax declaration = (ClassDeclarationSyntax)node.Node;
                    if (node.SemanticModel.GetDeclaredSymbol(declaration, node.CancellationToken) is not { } type || Form(type) is not (var keyedForm, _)) return;
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
            start.RegisterOperationAction(
                analysis => {
                    if (Target(analysis.Operation) is { ContainingType: { } owner } member && Form(owner) is (true, var readers) && Among(member, readers)) read[owner] = 1;
                },
                OperationKind.Invocation,
                OperationKind.PropertyReference,
                OperationKind.FieldReference);
            start.RegisterCompilationEndAction(end => {
                foreach (KeyValuePair<INamedTypeSymbol, (bool Plain, bool Items)> pair in free.Where(static pair => pair.Value is (true, true)))
                    if (Form(pair.Key) is (var keyedForm, _) && (!keyedForm || !read.ContainsKey(pair.Key)))
                        end.ReportDiagnostic(Diagnostic.Create(Rule, pair.Key.Locations[0], pair.Key.Name, keyedForm ? "keys items by unread member names" : "holds data-free items"));
            });
        });
    }
}

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class HostPickingUnionAnalyzer : DiagnosticAnalyzer {

    // --- [RULES]

    private static readonly DiagnosticDescriptor Rule = new(
        "RASM1106",
        "Union case per host member, call the member at its caller",
        "`{0}` cases each pick one host member, call host members at their callers inside one convention lift",
        Categories.Thinktecture,
        DiagnosticSeverity.Error,
        isEnabledByDefault: true,
        "`[Union]` types with every `Switch` or `Map` lambda one lifted or direct host call or construction over case data.",
        Link("RASM1106"),
        WellKnownDiagnosticTags.CompilationEnd);

    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => [Rule];

    public override void Initialize(AnalysisContext context) {
        context.EnableConcurrentExecution();
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
        context.RegisterCompilationStartAction(static start => {
            Compilation compilation = start.Compilation;
            WellKnownTypeProvider types = WellKnownTypeProvider.GetOrCreate(compilation);
            if (types.GetOrCreateTypeByMetadataName("Thinktecture.UnionAttribute") is not { } union) return;
            INamedTypeSymbol? io = types.GetOrCreateTypeByMetadataName("LanguageExt.IO");
            ISymbol? languageExt = Declared(compilation, "N:LanguageExt").FirstOrDefault();
            ImmutableArray<IMethodSymbol> lifts = [.. Members(io, "lift", "liftAsync", "liftVAsync", "pure"), .. Members(types.GetOrCreateTypeByMetadataName("LanguageExt.Prelude"), "use")];
            bool Hosted(ISymbol target) =>
                !Same(target.ContainingAssembly, compilation.Assembly)
                && !Namespaced(compilation, target, languageExt);
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
            start.RegisterOperationAction(
                analysis => {
                    if (analysis.Operation is not IInvocationOperation { TargetMethod: { Name: "Switch" or "Map", ContainingType: { } owner } } call || !Sourced(owner) || Marked(owner, union) is null) return;
                    ImmutableArray<IOperation> arms = [.. call.Arguments.Where(static argument => argument.Parameter?.Type.TypeKind is TypeKind.Delegate).Select(static argument => Stripped(argument.Value))];
                    if (arms.IsEmpty) return;
                    bool each = arms.All(arm => Lambda(arm) is { } function && Picks(function, owner, analysis.CancellationToken));
                    _ = picked.AddOrUpdate(owner.OriginalDefinition, each, (_, prior) => prior && each);
                },
                OperationKind.Invocation);
            start.RegisterCompilationEndAction(end => {
                foreach (KeyValuePair<INamedTypeSymbol, bool> pair in picked.Where(static pair => pair.Value))
                    end.ReportDiagnostic(Diagnostic.Create(Rule, pair.Key.Locations[0], pair.Key.Name));
            });
        });
    }
}
