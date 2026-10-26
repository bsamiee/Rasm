using System.Collections.Concurrent;
using System.Collections.Immutable;
using System.Text.RegularExpressions;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Operations;

namespace Rasm.Analyzers;

// --- [COMPOSITION] ---------------------------------------------------------------------
[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class LanguageAnalyzer : RasmAnalyzer {
    // --- [RULES]
    private const string Category = "CSharp";

    public static readonly DiagnosticDescriptor Reader = Rule(
        "RASM3001",
        Category,
        "Private member with one read, inline at the read",
        "`{0}` has one read, in `{1}`, inline `{0}` at the read",
        "Inline private constants, `Contains` sets, and one-line expression-bodied members their type reads once when inlining keeps each name word");

    public static readonly DiagnosticDescriptor Covered = Rule(
        "RASM3002",
        Category,
        "Catch-all arm over covered enum, delete the arm",
        "Catch-all arm over `{0}` matches no declared member, delete the arm",
        "Switch expressions over non-`[Flags]` enums with arms naming every member hold no catch-all arm");

    public static readonly DiagnosticDescriptor Discarded = Rule(
        "RASM3003",
        Category,
        "Discarded awaitable, await or return the value",
        "`_ =` discards awaitable `{0}`, await `{0}` or return `{0}` to an awaiting caller",
        "Await each awaitable value or return it to an awaiting caller in place of a `_ =` discard");

    public static readonly DiagnosticDescriptor Fallback = Rule(
        "RASM3004",
        Category,
        "Catch-all arm over enum, name each member",
        "Catch-all arm over `{0}` maps {1} to one result, name {1} in arms joined by `or` per shared result",
        "Switch expressions over non-`[Flags]` enums name every member in arms joined by `or`, with no catch-all arm");

    public static readonly DiagnosticDescriptor Borrowed = Rule(
        "RASM3005",
        Category,
        "Namespace owned by a referenced assembly, declare under project namespace",
        "`{1}` declares `{0}`, declare types under the project namespace",
        "Declare types under project namespaces, outside every namespace a referenced assembly declares");

    public static readonly DiagnosticDescriptor Clash = Rule(
        "RASM3006",
        Category,
        "Type name clashes with an imported type, rename the declaration",
        "`{0}` clashes with `{1}`, rename the project declaration",
        "Name namespace-level types apart from public imported or referenced-project types of equal arity and same-namespace types of other arity");

    public static readonly DiagnosticDescriptor Tested = Rule(
        "RASM3007",
        Category,
        "Type test on a type parameter, declare a static virtual member",
        "`{0}` tests `{1}` for concrete `{2}`, declare behavior as a static virtual member of the `{1}` constraint",
        "Static virtual constraint members replace type-parameter tests (pattern, `is`, `as`, `typeof` equality outside external overrides) for concrete project types");

    public static readonly DiagnosticDescriptor Folder = Rule(
        "RASM3008",
        Category,
        "File named after its folder, rename for its concept family",
        "`{0}` takes its folder's name, name the file for its concept family",
        "Name source files for their concept family in place of their parent directory");

    public static readonly DiagnosticDescriptor Shadowed = Rule(
        "RASM3009",
        Category,
        "Qualified name around a project declaration, rename the declaration",
        "`{0}` is qualified around project declaration `{1}`, rename `{1}` and import `{2}`",
        "Rename each project type, member, parameter, or local shadowing a package or statically imported name, then import its owner");

    public static readonly DiagnosticDescriptor Qualified = Rule(
        "RASM3010",
        Category,
        "Qualified name with no clash, import and write the simple name",
        "`{0}` clashes with nothing in scope, import `{1}` and write `{2}`",
        "Import package namespaces and write simple names when no namespace type clashes with project declarations or file simple names");

    public static readonly DiagnosticDescriptor Restated = Rule(
        "RASM3011",
        Category,
        "Static instance restated at a construction site, read the instance",
        "`{0}` restates `{1}`, read `{1}`",
        "Read same-type static members over constructions or factory calls restating their constant initializers, outside smart enum item initializers",
        WellKnownDiagnosticTags.CompilationEnd);

    private static readonly Regex Word = new("[A-Z]?[a-z0-9]+|[A-Z]+(?![a-z])", RegexOptions.CultureInvariant);

    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => [Reader, Covered, Discarded, Fallback, Borrowed, Clash, Tested, Folder, Shadowed, Qualified, Restated];

    protected override void Start(CompilationStartAnalysisContext context) {
        SystemSymbols symbols = new(context.Compilation);
        LanguageExtSymbols languageExt = new(context.Compilation);
        Readers(context, symbols.Sets, symbols.Collections);
        if (symbols.Flags is { } flags) Enums(context, flags);
        Awaitables(context);
        Namespaces(context);
        ImmutableArray<IAssemblySymbol> projects = Projects(context.Compilation);
        Clashes(context, projects, Imported(context.Compilation, context.CancellationToken), languageExt.K);
        Qualifications(context);
        Restatements(context);
        if (symbols.Type is { } type) Tests(context, projects, type);
        Files(context);
    }

    // --- [PROJECTS]
    private static ImmutableArray<IAssemblySymbol> Projects(Compilation compilation) =>
        compilation.AssemblyName?.Split('.') is { Length: > 1 } segments
            ? [.. compilation.SourceModule.ReferencedAssemblySymbols.Where(assembly => assembly.Name.StartsWith($"{segments[0]}.", StringComparison.Ordinal))]
            : [];

    // --- [READERS]
    private static void Readers(CompilationStartAnalysisContext context, ImmutableArray<INamedTypeSymbol> sets, ImmutableArray<ISymbol> collections) {
        static bool Candidate(ISymbol symbol) =>
            symbol is { DeclaredAccessibility: Accessibility.Private, IsImplicitlyDeclared: false, ContainingType.IsExtension: false } && symbol.GetAttributes().IsEmpty && symbol switch {
                IFieldSymbol field => field.IsConst || field is { IsStatic: true, IsReadOnly: true },
                IPropertySymbol property => property is { IsIndexer: false, SetMethod: null, IsVirtual: false, IsAbstract: false, IsOverride: false, ReturnsByRef: false, ReturnsByRefReadonly: false }
                    && (property.IsStatic || !property.ContainingType.IsValueType),
                IMethodSymbol method => method is { MethodKind: MethodKind.Ordinary, TypeParameters.IsEmpty: true, IsAsync: false, IsExtern: false, IsVirtual: false, IsAbstract: false, IsOverride: false, IsExtensionMethod: false, IsPartialDefinition: false, PartialDefinitionPart: null, ReturnsByRef: false, ReturnsByRefReadonly: false }
                    && method.GetReturnTypeAttributes().IsEmpty && method.Parameters.All(static parameter => parameter is { RefKind: RefKind.None, IsParams: false, HasExplicitDefaultValue: false } && parameter.GetAttributes().IsEmpty)
                    && (method.IsStatic || !method.ContainingType.IsValueType),
                _ => false,
            };
        static bool Shaped(SyntaxNode syntax) =>
            syntax switch {
                VariableDeclaratorSyntax { Initializer: not null } or MethodDeclarationSyntax { ExpressionBody: not null } or PropertyDeclarationSyntax { ExpressionBody: not null } => true,
                PropertyDeclarationSyntax { AccessorList.Accessors: { Count: 1 } accessors } => accessors[0] is { RawKind: (int)SyntaxKind.GetAccessorDeclaration, ExpressionBody: not null },
                _ => false,
            };
        static IOperation? Expressed(IOperation block) =>
            block switch {
                IFieldInitializerOperation { Value: var value } => value,
                IMethodBodyOperation { BlockBody: null, ExpressionBody: { } body } => Expressed(body),
                IBlockOperation { Operations: { Length: 1 } held } when held[0] is IReturnOperation { ReturnedValue: { } value } => value,
                IBlockOperation { Operations: { IsEmpty: false } held } when held[0] is IExpressionStatementOperation { Operation: var value } && held.Skip(1).All(static rest => rest.IsImplicit) => value,
                _ => null,
            };
        ImmutableArray<IOperation> Elements(IOperation value) =>
            value switch {
                IConversionOperation { IsImplicit: true } conversion => Elements(conversion.Operand),
                ICollectionExpressionOperation collection => collection.Elements,
                IArrayCreationOperation { Initializer: { } initializer } => initializer.ElementValues,
                IObjectCreationOperation { Arguments.IsEmpty: true, Initializer: { } initializer } when initializer.Initializers.All(static added => added is IInvocationOperation { Arguments.Length: 1 }) =>
                    [.. initializer.Initializers.Select(static added => ((IInvocationOperation)added).Arguments[0].Value)],
                IInvocationOperation { Instance: null, Arguments: { IsEmpty: false } arguments, TargetMethod: { IsExtensionMethod: true, ContainingNamespace: var space } }
                    when arguments[0].Value is { Syntax.Parent: MemberAccessExpressionSyntax } source && arguments.Skip(1).All(static argument => argument.ArgumentKind is ArgumentKind.DefaultValue)
                    && Among(context.Compilation.GetCompilationNamespace(space), collections) => Elements(source),
                _ => [],
            };
        bool Listed(IFieldSymbol field, IOperation value) =>
            !field.IsConst && Elements(value) is { IsEmpty: false } elements && elements.All(static element => element.ConstantValue.HasValue)
            && (field.Type is IArrayTypeSymbol || field.Type.AllInterfaces.Append(field.Type).Any(face => Among(face, sets)));
        static bool Literal(IFieldSymbol field, IOperation value) =>
            value is ILiteralOperation or IUnaryOperation { Operand: ILiteralOperation } or IFieldReferenceOperation { Field.IsConst: true } && SymbolEqualityComparer.Default.Equals(value.Type, field.Type);
        static ImmutableHashSet<string> Words(string name) => [.. Word.Matches(name).Cast<Match>().Select(static word => word.Value.ToUpperInvariant())];
        static string? Naming(IOperation read, ImmutableHashSet<ISymbol> candidates) =>
            Consumer(read) switch {
                IArgumentOperation { ArgumentKind: ArgumentKind.Explicit, Parameter: { } parameter } when !candidates.Contains(parameter.ContainingSymbol.OriginalDefinition) => parameter.Name,
                IVariableInitializerOperation { Parent: IVariableDeclaratorOperation declarator } => declarator.Symbol.Name,
                IFieldInitializerOperation { InitializedFields: { Length: 1 } fields } => fields[0].Name,
                IPropertyInitializerOperation { InitializedProperties: { Length: 1 } properties } => properties[0].Name,
                ISimpleAssignmentOperation { Target: IMemberReferenceOperation target, Parent: IObjectOrCollectionInitializerOperation or IAnonymousObjectCreationOperation } assignment when Stripped(assignment.Value) == read =>
                    target.Member.Name,
                _ => null,
            };
        static bool Contained(IOperation read) =>
            Consumer(read) is IInvocationOperation { TargetMethod.Name: "Contains", Instance: not null, Arguments.Length: 1 }
                or IArgumentOperation { Parameter.Ordinal: 0, Parent: IInvocationOperation { TargetMethod: { IsExtensionMethod: true, Name: "Contains" }, Arguments.Length: 2 } };
        static bool Targeted(IOperation value) =>
            Stripped(value) switch {
                ICollectionExpressionOperation or IDefaultValueOperation or IAnonymousFunctionOperation or IDelegateCreationOperation or IThrowOperation => true,
                ILiteralOperation { ConstantValue: { HasValue: true, Value: null } } => true,
                IObjectCreationOperation { Syntax: ImplicitObjectCreationExpressionSyntax } => true,
                IConditionalOperation conditional => conditional.WhenFalse is null || Targeted(conditional.WhenTrue) || Targeted(conditional.WhenFalse),
                ISwitchExpressionOperation dispatch => dispatch.Arms.Any(static arm => Targeted(arm.Value)),
                _ => false,
            };
        static bool Pure(IOperation value) =>
            !value.DescendantsAndSelf().Any(static held => held is IInvocationOperation or IObjectCreationOperation or ITypeParameterObjectCreationOperation or IAnonymousObjectCreationOperation
                or IArrayCreationOperation or ICollectionExpressionOperation or IAssignmentOperation or IIncrementOrDecrementOperation or IAwaitOperation or IAnonymousFunctionOperation
                or IDelegateCreationOperation or IThrowOperation or IDynamicInvocationOperation);
        bool Plain(IOperation value) =>
            value is { Syntax: IdentifierNameSyntax, SemanticModel: { } model }
            && Variable(value) is { } held and not IParameterSymbol { RefKind: not RefKind.None }
            && !References(held, value, new Reading(model, context.CancellationToken)).Any(Written);
        static bool Deferred(SyntaxNode node, SyntaxNode root) =>
            node != root && node.Parent is { } parent && (parent switch {
                ConditionalExpressionSyntax conditional => conditional.Condition != node,
                BinaryExpressionSyntax binary => binary.Right == node && binary.Kind() is SyntaxKind.LogicalAndExpression or SyntaxKind.LogicalOrExpression or SyntaxKind.CoalesceExpression,
                ConditionalAccessExpressionSyntax access => access.WhenNotNull == node,
                SwitchExpressionArmSyntax or AnonymousFunctionExpressionSyntax or QueryExpressionSyntax => true,
                _ => false,
            } || Deferred(parent, root));
        static SyntaxNode Unwrapped(SyntaxNode node) => node.Parent is ParenthesizedExpressionSyntax parent ? Unwrapped(parent) : node;
        static int Level(SyntaxNode node) =>
            node switch {
                IfStatementSyntax { Parent: ElseClauseSyntax } => 0,
                ConditionalExpressionSyntax conditional when Unwrapped(conditional) is { Parent: ConditionalExpressionSyntax outer } alternative && outer.WhenFalse == alternative => 0,
                IfStatementSyntax or ConditionalExpressionSyntax or CommonForEachStatementSyntax or ForStatementSyntax or WhileStatementSyntax or DoStatementSyntax or SwitchStatementSyntax
                    or SwitchExpressionSyntax or AnonymousFunctionExpressionSyntax or LocalFunctionStatementSyntax => 1,
                _ => 0,
            };
        static int Depth(SyntaxNode node) => node.DescendantNodesAndSelf().Max(held => held.AncestorsAndSelf().TakeWhile(ancestor => ancestor != node).Sum(Level)) + Level(node);
        static int Nesting(SyntaxNode node) => node.Ancestors().TakeWhile(static held => held is not (MemberDeclarationSyntax or AccessorDeclarationSyntax)).Sum(Level);
        static bool Unsafe(SyntaxNode node) =>
            node.AncestorsAndSelf().Any(static held => held is UnsafeStatementSyntax || (held is MemberDeclarationSyntax member && member.Modifiers.Any(SyntaxKind.UnsafeKeyword)));
        static bool Grouped(IOperation reference) =>
            Ancestor<ISymbolInitializerOperation>(reference, static _ => true) is { } initializer
            && initializer.Descendants().OfType<IMethodReferenceOperation>().Select(static group => group.Method.OriginalDefinition).Distinct(SymbolEqualityComparer.Default).Skip(1).Any();
        static string Named(ISymbol symbol) =>
            symbol is IMethodSymbol { MethodKind: MethodKind.Constructor or MethodKind.StaticConstructor } constructor ? constructor.ContainingType.Name : Member(symbol).Name;
        static bool Free(SyntaxNode moved, ImmutableHashSet<string> bound, ImmutableHashSet<string> local) =>
            !Scopes.Referenced(moved).Any(used => !bound.Contains(used.Identifier.ValueText) && local.Contains(used.Identifier.ValueText));
        bool Bound(IArgumentOperation argument, ImmutableArray<IdentifierNameSyntax> used, SyntaxNode body) =>
            argument is { ArgumentKind: ArgumentKind.Explicit, Parameter: { } parameter, Value: var passed } && passed is not IConversionOperation { IsImplicit: true }
            && (Plain(passed) || (Pure(passed) && used.Where(name => string.Equals(name.Identifier.ValueText, parameter.Name, StringComparison.Ordinal)).ToImmutableArray() switch {
                { IsEmpty: true } => true,
                { Length: 1 } uses => !Deferred(uses[0], body),
                _ => false,
            }));
        bool Substituted(IOperation value, int nesting, ImmutableHashSet<string> local, ImmutableArray<IParameterSymbol> parameters, ImmutableArray<IArgumentOperation> arguments) {
            SyntaxNode body = value.Syntax;
            ImmutableHashSet<string> names = [.. parameters.Select(static parameter => parameter.Name)];
            ImmutableArray<IdentifierNameSyntax> used = [.. Scopes.Referenced(body)];
            ImmutableHashSet<string> passed = [.. arguments.SelectMany(static argument => Scopes.Referenced(argument.Syntax)).Select(static name => name.Identifier.ValueText)];
            return value is not IConversionOperation { IsImplicit: true } && !Targeted(value) && Free(body, names, local)
                && body.SyntaxTree.GetLineSpan(body.Span, context.CancellationToken) is var lines && lines.StartLinePosition.Line == lines.EndLinePosition.Line
                && !value.DescendantsAndSelf().OfType<INameOfOperation>().Any()
                && !Scopes.Declared(body).Any(name => names.Contains(name) || local.Contains(name) || passed.Contains(name))
                && nesting + Depth(body) <= 3
                && arguments.All(argument => Bound(argument, used, body));
        }
        bool Inlined(ISymbol member, IOperation value, IOperation read, ISymbol reader, ImmutableHashSet<ISymbol> candidates) {
            if (read.Syntax.SyntaxTree != value.Syntax.SyntaxTree || !Same(reader.ContainingType, member.ContainingType) || Inside<IAttributeOperation>(read) || Inside<IParameterInitializerOperation>(read)
                || Inside<INameOfOperation>(read) || (Unsafe(value.Syntax) && !Unsafe(read.Syntax)) || read.Syntax.FirstAncestorOrSelf<MemberDeclarationSyntax>() is not { } site
                || (read switch { IMemberReferenceOperation reference => reference.Instance, IInvocationOperation call => call.Instance, _ => null }) is not (null or IInstanceReferenceOperation { ReferenceKind: InstanceReferenceKind.ContainingTypeInstance }))
                return false;
            ImmutableHashSet<string> local = [.. Scopes.Declared(site), "value"];
            bool Kept(IOperation named) =>
                Words(member.Name).IsSubsetOf(Words(Naming(named, candidates) ?? "").Union(
                    value.Syntax.DescendantTokens().Where(static token => token.IsKind(SyntaxKind.IdentifierToken)).SelectMany(static token => Words(token.ValueText))));
            return (member, read) switch {
                (IFieldSymbol field, _) when Listed(field, value) => Contained(read) && Free(value.Syntax, [], local),
                (IFieldSymbol field, _) => Literal(field, value) && Naming(read, candidates) is { } name && Words(field.Name).IsSubsetOf(Words(name)) && Free(value.Syntax, [], local),
                (IPropertySymbol, IPropertyReferenceOperation) => Kept(read) && Substituted(value, Nesting(read.Syntax), local, [], []),
                (IMethodSymbol method, IInvocationOperation call) => Kept(read) && Substituted(value, Nesting(read.Syntax), local, method.Parameters, call.Arguments),
                (IMethodSymbol method, IMethodReferenceOperation { Parent: IDelegateCreationOperation creation } group) when !Grouped(group) =>
                    Kept(creation) && Substituted(value, Nesting(read.Syntax) + 1, local, method.Parameters, []),
                _ => false,
            };
        }
        context.RegisterSymbolStartAction(
            start => {
                INamedTypeSymbol type = (INamedTypeSymbol)start.Symbol;
                ILookup<string, SimpleNameSyntax> named = type.DeclaringSyntaxReferences
                    .SelectMany(reference => reference.GetSyntax(start.CancellationToken).DescendantNodes().OfType<SimpleNameSyntax>())
                    .ToLookup(static name => name.Identifier.ValueText, StringComparer.Ordinal);
                ImmutableHashSet<ISymbol> candidates = ImmutableHashSet.CreateRange(
                    SymbolEqualityComparer.Default,
                    type.GetMembers().Where(member => Candidate(member) && named[member.Name].Take(2).Count() is 1
                        && member.DeclaringSyntaxReferences is { Length: 1 } declared && Shaped(declared[0].GetSyntax(start.CancellationToken))));
                if (candidates.IsEmpty) return;
                ConcurrentDictionary<ISymbol, IOperation> values = new(SymbolEqualityComparer.Default);
                ConcurrentDictionary<ISymbol, (IOperation Read, ISymbol Reader)> reads = new(SymbolEqualityComparer.Default);
                start.RegisterOperationBlockAction(block => {
                    ISymbol owner = Member(block.OwningSymbol);
                    if (candidates.Contains(owner) && block.OperationBlocks is { Length: 1 } blocks && Expressed(blocks[0]) is { } value) values[owner] = value;
                });
                start.RegisterOperationAction(
                    operation => {
                        if (Target(operation.Operation)?.OriginalDefinition is { } target && candidates.Contains(target)) reads[target] = (operation.Operation, Enclosing(operation.ContainingSymbol));
                    },
                    OperationKind.FieldReference,
                    OperationKind.PropertyReference,
                    OperationKind.Invocation,
                    OperationKind.MethodReference);
                start.RegisterSymbolEndAction(end => {
                    foreach (KeyValuePair<ISymbol, (IOperation Read, ISymbol Reader)> pair in reads)
                        if (values.TryGetValue(pair.Key, out IOperation? value) && Inlined(pair.Key, value, pair.Value.Read, pair.Value.Reader, candidates))
                            end.ReportDiagnostic(Diagnostic.Create(Reader, pair.Key.Locations[0], [pair.Value.Read.Syntax.GetLocation()], pair.Key.Name, Named(pair.Value.Reader)));
                });
            },
            SymbolKind.NamedType);
    }

    // --- [ENUMS]
    private static void Enums(CompilationStartAnalysisContext context, INamedTypeSymbol flags) {
        static IEnumerable<object?> Named(IOperation? test) =>
            test switch {
                IConstantPatternOperation { Value.ConstantValue: { HasValue: true } constant } => [constant.Value],
                IBinaryPatternOperation { OperatorKind: BinaryOperatorKind.Or } either => Named(either.LeftPattern).Concat(Named(either.RightPattern)),
                _ => [],
            };
        static bool CatchesRest((IOperation Arm, IOperation? Test, IOperation? Guard) arm) =>
            CatchesAll(arm) || (arm is { Guard: null, Test: INegatedPatternOperation { Pattern: var excluded } } && Named(excluded).Any());
        Report<ISwitchExpressionOperation>(context, [OperationKind.SwitchExpression], (dispatch, _) => {
            if (Dispatch(dispatch) is not (_, var arms) || dispatch.Value.Type is not INamedTypeSymbol { TypeKind: TypeKind.Enum } subject
                || !Sourced(subject) || Marked(subject, flags) is not null) return [];
            ImmutableHashSet<object?> named = [.. arms.Where(static arm => arm.Guard is null).SelectMany(static arm => Named(arm.Test))];
            ImmutableArray<IFieldSymbol> unnamed = [.. subject.GetMembers().OfType<IFieldSymbol>().Where(member => member.HasConstantValue && !named.Contains(member.ConstantValue))];
            return named.IsEmpty ? [] : arms.Where(CatchesRest).Select(arm => Diagnostic.Create(
                unnamed.IsEmpty ? Covered : Fallback,
                arm.Arm.Syntax.GetLocation(),
                subject.Name,
                string.Join(", ", unnamed.Select(member => $"`{subject.Name}.{member.Name}`"))));
        });
    }

    // --- [AWAITABLES]
    private static void Awaitables(CompilationStartAnalysisContext context) {
        static bool Awaitable(ITypeSymbol? type) =>
            Bases(type).Any(static held => held.GetMembers(WellKnownMemberNames.GetAwaiter).Any(static member => member is IMethodSymbol { IsStatic: false, Parameters.IsEmpty: true }));
        Report<ISimpleAssignmentOperation>(context, [OperationKind.SimpleAssignment], static (assignment, _) =>
            assignment is { Target: IDiscardOperation, Value: var value } && Stripped(value) is var awaited && Awaitable(awaited.Type)
                ? [Diagnostic.Create(Discarded, assignment.Syntax.GetLocation(), awaited.Syntax)]
                : []);
    }

    // --- [NAMESPACES]
    private static void Namespaces(CompilationStartAnalysisContext context) =>
        context.RegisterSyntaxNodeAction(
            static node => {
                BaseNamespaceDeclarationSyntax declaration = (BaseNamespaceDeclarationSyntax)node.Node;
                if (node.SemanticModel.GetDeclaredSymbol(declaration, node.CancellationToken) is INamespaceSymbol declared
                    && node.Compilation.GetCompilationNamespace(declared)?.ConstituentNamespaces.FirstOrDefault(space => space.ContainingAssembly is { } owner && !Same(owner, node.Compilation.Assembly)) is { ContainingAssembly: { } assembly })
                    node.ReportDiagnostic(Diagnostic.Create(Borrowed, declaration.Name.GetLocation(), declared.ToDisplayString(), assembly.Name));
            },
            SyntaxKind.NamespaceDeclaration,
            SyntaxKind.FileScopedNamespaceDeclaration);

    // --- [CLASHES]
    private static ImmutableHashSet<INamespaceSymbol> Imported(Compilation compilation, CancellationToken token) {
        static INamespaceSymbol? Resolve(INamespaceSymbol root, NameSyntax name) =>
            name switch {
                IdentifierNameSyntax leaf => root.GetNamespaceMembers().FirstOrDefault(held => string.Equals(held.Name, leaf.Identifier.ValueText, StringComparison.Ordinal)),
                QualifiedNameSyntax qualified => Resolve(root, qualified.Left) is { } left ? Resolve(left, qualified.Right) : null,
                AliasQualifiedNameSyntax aliased => Resolve(root, aliased.Name),
                _ => null,
            };
        IEqualityComparer<INamespaceSymbol> comparer = SymbolEqualityComparer.Default;
        return ImmutableHashSet.CreateRange(
            comparer,
            compilation.SyntaxTrees
                .SelectMany(tree => tree.GetRoot(token).DescendantNodes(static node => node is CompilationUnitSyntax or BaseNamespaceDeclarationSyntax).OfType<UsingDirectiveSyntax>())
                .Where(static directive => directive.Alias is null && directive.StaticKeyword.IsKind(SyntaxKind.None) && directive.NamespaceOrType is NameSyntax)
                .Select(directive => Resolve(compilation.GlobalNamespace, (NameSyntax)directive.NamespaceOrType))
                .OfType<INamespaceSymbol>());
    }

    private static void Clashes(CompilationStartAnalysisContext context, ImmutableArray<IAssemblySymbol> projects, ImmutableHashSet<INamespaceSymbol> spaces, INamedTypeSymbol? brand) =>
        context.RegisterSyntaxNodeAction(
            node => {
                if (node.SemanticModel.GetDeclaredSymbol(node.Node, node.CancellationToken) is not INamedTypeSymbol { ContainingType: null, ContainingNamespace: { } space } type
                    || type.DeclaringSyntaxReferences.FirstOrDefault() is not { } first || first.SyntaxTree != node.Node.SyntaxTree || first.Span != node.Node.Span) return;
                IAssemblySymbol own = node.Compilation.Assembly;
                bool Exposed(INamedTypeSymbol other) => other.DeclaredAccessibility is Accessibility.Public && !Same(other.ContainingAssembly, own);
                bool Branded(INamedTypeSymbol generic, INamedTypeSymbol plain) =>
                    plain.Arity is 0 && Faces(generic, brand).Any(face => face.TypeArguments.Length > 0 && Same(face.TypeArguments[0], plain));
                IEnumerable<INamedTypeSymbol> imported = node.SemanticModel.GetImportScopes(node.Node.SpanStart, node.CancellationToken)
                    .SelectMany(static scope => scope.Imports).Select(static import => import.NamespaceOrType).OfType<INamespaceSymbol>().Concat(spaces)
                    .SelectMany(held => held.GetTypeMembers(type.Name, type.Arity)).Where(Exposed);
                IEnumerable<INamedTypeSymbol> declared = projects.Where(project => project.TypeNames.Contains(type.Name))
                    .SelectMany(project => project.NamespaceNames.Select(name => project.GetTypeByMetadataName(name.Length is 0 ? type.MetadataName : $"{name}.{type.MetadataName}")))
                    .OfType<INamedTypeSymbol>().Where(Exposed);
                IEnumerable<INamedTypeSymbol> siblings = space.GetTypeMembers(type.Name).Where(other => other.Arity != type.Arity && Sourced(other) && !Branded(type, other) && !Branded(other, type));
                if (imported.Concat(declared).Concat(siblings).FirstOrDefault() is { } clash)
                    node.ReportDiagnostic(Diagnostic.Create(Clash, node.Node switch {
                        BaseTypeDeclarationSyntax declaration => declaration.Identifier.GetLocation(),
                        DelegateDeclarationSyntax declaration => declaration.Identifier.GetLocation(),
                        var other => other.GetLocation(),
                    }, type.ToDisplayString(SymbolDisplayFormat.MinimallyQualifiedFormat), clash.ToDisplayString()));
            },
            SyntaxKind.ClassDeclaration,
            SyntaxKind.StructDeclaration,
            SyntaxKind.RecordDeclaration,
            SyntaxKind.RecordStructDeclaration,
            SyntaxKind.InterfaceDeclaration,
            SyntaxKind.EnumDeclaration,
            SyntaxKind.DelegateDeclaration);

    // --- [QUALIFICATIONS]
    private static void Qualifications(CompilationStartAnalysisContext context) {
        ConcurrentDictionary<SyntaxTree, ILookup<string, SimpleNameSyntax>> bare = new();
        static bool Looked(SimpleNameSyntax name) =>
            name.Parent switch {
                QualifiedNameSyntax qualified => qualified.Right != name,
                MemberAccessExpressionSyntax access => access.Name != name,
                AliasQualifiedNameSyntax aliased => aliased.Name != name,
                MemberBindingExpressionSyntax => false,
                _ => true,
            };
        context.RegisterSyntaxNodeAction(
            node => {
                (ExpressionSyntax Left, SimpleNameSyntax Right)? parts = node.Node switch {
                    QualifiedNameSyntax qualified => (qualified.Left, qualified.Right),
                    MemberAccessExpressionSyntax access => (access.Expression, access.Name),
                    _ => null,
                };
                if (parts is not (var left, var right)
                    || (node.Node.Parent is QualifiedNameSyntax { Left: var outer } && outer == node.Node)
                    || (node.Node.Parent is MemberAccessExpressionSyntax { Expression: var receiver } && receiver == node.Node)
                    || node.Node.Ancestors().Any(static held => held is UsingDirectiveSyntax)
                    || (node.Node.FirstAncestorOrSelf<BaseNamespaceDeclarationSyntax>() is { Name: var declared } && declared.Span.Contains(node.Node.Span)))
                    return;
                CancellationToken token = node.CancellationToken;
                SemanticModel model = node.SemanticModel;
                ISymbol? owner = model.GetSymbolInfo(left, token).Symbol;
                if (owner is not (INamespaceSymbol or INamedTypeSymbol) || Sourced(owner) || model.GetSymbolInfo(right, token).Symbol is not { } target || target is INamespaceSymbol) return;
                ImmutableArray<ISymbol> scopes = [.. model.GetImportScopes(node.Node.SpanStart, token).SelectMany(static scope => scope.Imports).Select(static import => import.NamespaceOrType)];
                bool imported = Among(owner, scopes);
                if (owner is INamedTypeSymbol ? !imported : imported) return;
                string name = right.Identifier.ValueText;
                ImmutableArray<ISymbol> visible = [.. model.LookupSymbols(node.Node.SpanStart, name: name).Where(found => !Same(found, target))];
                ILookup<string, SimpleNameSyntax> used = bare.GetOrAdd(
                    node.Node.SyntaxTree,
                    tree => tree.GetRoot(token).DescendantNodes().OfType<SimpleNameSyntax>().Where(Looked).ToLookup(static held => held.Identifier.ValueText, StringComparer.Ordinal));
                bool Ambiguous(INamedTypeSymbol member) =>
                    used[member.Name].Any(held => held.Arity == member.Arity && model.GetAliasInfo(held, token) is null
                        && model.GetSymbolInfo(held, token).Symbol is INamedTypeSymbol { ContainingType: null } bound && !Same(bound, member));
                if (visible.FirstOrDefault(Sourced) is { } shadowing)
                    node.ReportDiagnostic(Diagnostic.Create(Shadowed, node.Node.GetLocation(), node.Node, shadowing.ToDisplayString(SymbolDisplayFormat.MinimallyQualifiedFormat), owner.ToDisplayString()));
                else if (visible.IsEmpty && owner is INamespaceSymbol space
                    && !space.GetTypeMembers().Any(member => model.LookupSymbols(node.Node.SpanStart, name: member.Name).Any(Sourced) || Ambiguous(member)))
                    node.ReportDiagnostic(Diagnostic.Create(Qualified, node.Node.GetLocation(), node.Node, owner.ToDisplayString(), right));
            },
            SyntaxKind.QualifiedName,
            SyntaxKind.SimpleMemberAccessExpression);
    }

    // --- [RESTATEMENTS]
    private static void Restatements(CompilationStartAnalysisContext context) {
        ConcurrentDictionary<ISymbol, IOperation> initializers = new(SymbolEqualityComparer.Default);
        ConcurrentBag<(IOperation Site, INamedTypeSymbol Type, ISymbol Owner)> sites = [];
        ThinktectureSymbols thinktecture = new(context.Compilation);
        bool Item(ISymbol owner, INamedTypeSymbol type) =>
            Same(owner.ContainingType, type) && initializers.ContainsKey(owner) && Marked(type, thinktecture.SmartEnums) is not null;
        static INamedTypeSymbol? Made(IOperation operation) =>
            operation switch {
                IObjectCreationOperation { Constructor: { } made, Initializer: null } => made.ContainingType,
                IInvocationOperation { TargetMethod: { IsStatic: true } method } when Same(method.ReturnType, method.ContainingType) => method.ContainingType,
                _ => null,
            };
        context.RegisterOperationAction(
            analysis => {
                switch (analysis.Operation) {
                    case IFieldInitializerOperation { InitializedFields: { Length: 1 } fields, Value: var value } when fields[0] is { IsStatic: true, IsReadOnly: true } field && Same(field.Type, field.ContainingType):
                        initializers[field] = value;
                        break;
                    case IPropertyInitializerOperation { InitializedProperties: { Length: 1 } properties, Value: var value } when properties[0] is { IsStatic: true, SetMethod: null } property && Same(property.Type, property.ContainingType):
                        initializers[property] = value;
                        break;
                    case var operation:
                        if (Made(operation) is { } type && Sourced(type) && type.TypeArguments.IsEmpty) sites.Add((operation, type, Member(analysis.ContainingSymbol)));
                        break;
                }
            },
            OperationKind.FieldInitializer,
            OperationKind.PropertyInitializer,
            OperationKind.ObjectCreation,
            OperationKind.Invocation);
        context.RegisterCompilationEndAction(end => {
            string? Shape(IOperation held, int depth) =>
                depth > 8 ? null : Stripped(held) switch {
                    { ConstantValue: { HasValue: true, Value: var constant }, Type: var type } => $"{type?.ToDisplayString()}={constant ?? "null"}",
                    IObjectCreationOperation { Constructor: { } made, Initializer: null } creation => Shaped(made.ContainingType, creation.Arguments, depth),
                    IInvocationOperation { TargetMethod: { IsStatic: true } method } call when Same(method.ReturnType, method.ContainingType) => Shaped(method.ContainingType, call.Arguments, depth),
                    IMemberReferenceOperation reference when initializers.TryGetValue(reference.Member, out IOperation? initializer) => Shape(initializer, depth + 1),
                    _ => null,
                };
            string? Shaped(INamedTypeSymbol type, ImmutableArray<IArgumentOperation> arguments, int depth) =>
                !arguments.IsEmpty && arguments.Select(argument => Shape(argument.Value, depth + 1)).ToImmutableArray() is var shapes && shapes.All(static shape => shape is not null)
                    ? $"{type.ToDisplayString()}({string.Join(",", shapes)})"
                    : null;
            IEqualityComparer<INamedTypeSymbol> comparer = SymbolEqualityComparer.Default;
            ILookup<INamedTypeSymbol, (ISymbol Member, string? Shape)> statics = initializers
                .Select(pair => (pair.Key.ContainingType, Member: pair.Key, Shape: Shape(pair.Value, 1)))
                .ToLookup(static held => held.ContainingType, static held => (held.Member, held.Shape), comparer);
            foreach ((IOperation site, INamedTypeSymbol type, ISymbol owner) in sites)
                if (!Item(owner, type) && Shape(site, 0) is { } shape && statics[type].FirstOrDefault(held => !Same(held.Member, owner) && string.Equals(held.Shape, shape, StringComparison.Ordinal)) is { Member: { } restated })
                    end.ReportDiagnostic(Diagnostic.Create(Restated, site.Syntax.GetLocation(), site.Syntax, $"{type.Name}.{restated.Name}"));
        });
    }

    // --- [TESTS]
    private static void Tests(CompilationStartAnalysisContext context, ImmutableArray<IAssemblySymbol> projects, INamedTypeSymbol type) {
        IAssemblySymbol own = context.Compilation.Assembly;
        static bool Open(ITypeSymbol? input) => input is ITypeParameterSymbol || (input is INamedTypeSymbol { TypeArguments: var arguments } && arguments.Any(Open));
        bool Concrete(ITypeSymbol? tested) =>
            tested is INamedTypeSymbol { TypeKind: TypeKind.Class or TypeKind.Struct, IsAbstract: false } named
            && (Same(named.ContainingAssembly, own) || projects.Any(project => Same(named.ContainingAssembly, project)));
        static bool External(ISymbol owner) => Overridden(owner).Skip(1).LastOrDefault() is { } origin && !Sourced(origin);
        static (IOperation Site, ITypeSymbol? Input, ITypeSymbol? Tested)? Test(IOperation operation) =>
            operation switch {
                IDeclarationPatternOperation { MatchesNull: true } => null,
                IPatternOperation pattern => (pattern.Parent is IIsPatternOperation test ? test : pattern, pattern.InputType, pattern.NarrowedType),
                IIsTypeOperation test => (test, test.ValueOperand.Type, test.TypeOperand),
                IConversionOperation { IsTryCast: true } cast => (cast, cast.Operand.Type, cast.Type),
                _ => null,
            };
        Report<IOperation>(
            context,
            [OperationKind.DeclarationPattern, OperationKind.TypePattern, OperationKind.RecursivePattern, OperationKind.IsType, OperationKind.Conversion],
            (operation, _) =>
                Test(operation) is ( { } site, var input, var tested) && Open(input) && Concrete(tested)
                    ? [Diagnostic.Create(Tested, site.Syntax.GetLocation(), site.Syntax, input!.Name, tested!.Name)]
                    : []);
        Report<IBinaryOperation>(context, [OperationKind.Binary], (binary, analysis) =>
            binary is { OperatorKind: BinaryOperatorKind.Equals or BinaryOperatorKind.NotEquals } && Same(binary.LeftOperand.Type, type) && Same(binary.RightOperand.Type, type)
            && !External(Member(analysis.ContainingSymbol))
            && (Stripped(binary.LeftOperand), Stripped(binary.RightOperand)) switch {
                (ITypeOfOperation typed, var other) when Concrete(typed.TypeOperand) => (Other: other, Typed: typed.TypeOperand),
                (var other, ITypeOfOperation typed) when Concrete(typed.TypeOperand) => (Other: other, Typed: typed.TypeOperand),
                _ => default,
            } is { Other: { } compared, Typed: { } concrete }
                ? [Diagnostic.Create(Tested, binary.Syntax.GetLocation(), binary.Syntax, compared.Syntax, concrete.Name)]
                : []);
    }

    // --- [FILES]
    private static void Files(CompilationStartAnalysisContext context) =>
        context.RegisterSyntaxTreeAction(static tree => {
            string path = tree.Tree.FilePath;
            if (Path.GetFileNameWithoutExtension(path) is { Length: > 0 } name && string.Equals(name, Path.GetFileName(Path.GetDirectoryName(path)), StringComparison.Ordinal))
                tree.ReportDiagnostic(Diagnostic.Create(Folder, tree.Tree.GetRoot(tree.CancellationToken).GetFirstToken().GetLocation(), Path.GetFileName(path)));
        });
}
