using System.Collections.Concurrent;
using System.Collections.Immutable;
using System.Text.RegularExpressions;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.FlowAnalysis;
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
        "Inline private constants, `Contains` sets, and one-line expression-bodied members their type reads once when inlining keeps each name word.",
        WellKnownDiagnosticTags.CompilationEnd);

    public static readonly DiagnosticDescriptor Covered = Rule(
        "RASM3002",
        Category,
        "Catch-all arm over covered enum, delete the arm",
        "Catch-all arm over `{0}` matches no declared member, delete the arm",
        "Switch expressions over non-`[Flags]` enums with arms naming every member hold no catch-all arm.");

    public static readonly DiagnosticDescriptor Discarded = Rule(
        "RASM3003",
        Category,
        "Discarded awaitable, await or return the value",
        "`_ =` discards awaitable `{0}`, await `{0}` or return `{0}` to an awaiting caller",
        "Await each awaitable value or return it to an awaiting caller in place of a `_ =` discard, outside host start requests with a task that only reports completion.");

    public static readonly DiagnosticDescriptor Fallback = Rule(
        "RASM3004",
        Category,
        "Catch-all arm over enum, name each member",
        "Catch-all arm over `{0}` maps {1} to one result, name {1} in arms joined by `or` per shared result",
        "Switch expressions over non-`[Flags]` enums name every member in arms joined by `or`, with no catch-all arm.");

    public static readonly DiagnosticDescriptor Borrowed = Rule(
        "RASM3005",
        Category,
        "Namespace owned by a referenced assembly, declare under project namespace",
        "`{1}` declares `{0}`, declare types under the project namespace",
        "Declare types under project namespaces, outside every namespace a referenced assembly declares.");

    public static readonly DiagnosticDescriptor Clash = Rule(
        "RASM3006",
        Category,
        "Type name clashes with an imported type, rename the declaration",
        "`{0}` clashes with `{1}`, rename the project declaration",
        "Name namespace-level types apart from public imported types and public types of project references, of equal arity.",
        WellKnownDiagnosticTags.CompilationEnd);

    public static readonly DiagnosticDescriptor Tested = Rule(
        "RASM3007",
        Category,
        "Type test on a type parameter, declare a static virtual member",
        "`{0}` tests `{1}` for concrete `{2}`, declare behavior as a static virtual member of the `{1}` constraint",
        "Static virtual constraint members replace type-parameter tests (pattern, `is`, `as`, `typeof` equality outside external overrides) for concrete project types.");

    public static readonly DiagnosticDescriptor Folder = Rule(
        "RASM3008",
        Category,
        "File named after its folder, rename for its concept family",
        "`{0}` takes its folder's name, name the file for its concept family",
        "Name source files for their concept family in place of their parent directory.");

    public static readonly DiagnosticDescriptor Shadowed = Rule(
        "RASM3009",
        Category,
        "Qualified name around a project declaration, rename the declaration",
        "`{0}` is qualified around project declaration `{1}`, rename `{1}` and import `{2}`",
        "Rename each project type, member, parameter, or local shadowing a package or statically imported name, then import its owner.");

    public static readonly DiagnosticDescriptor Qualified = Rule(
        "RASM3010",
        Category,
        "Qualified name with no clash, import and write the simple name",
        "`{0}` clashes with nothing in scope, import `{1}` and write `{2}`",
        "Import package namespaces and write simple names when no namespace type clashes with project declarations or file simple names.");

    public static readonly DiagnosticDescriptor Restated = Rule(
        "RASM3011",
        Category,
        "Static instance restated at a construction site, read the instance",
        "`{0}` restates `{1}`, read `{1}`",
        "Read same-type static members over immutable record or readonly value constructions and transparent factories restating their constant initializers, outside smart enum item initializers.",
        WellKnownDiagnosticTags.CompilationEnd);

    private static readonly DiagnosticDescriptor Forwarded = Rule(
        "RASM3012",
        Category,
        "Forwarding property, read member at its uses",
        "`{0}` forwards `{1}`, read `{1}` at each use",
        "Properties and indexers add behavior or implement a contract instead of returning another owner's accessible member unchanged.",
        WellKnownDiagnosticTags.CompilationEnd);

    private static readonly DiagnosticDescriptor Factory = Rule(
        "RASM3013",
        Category,
        "Overloads take a value and its factory, keep one overload",
        "`{0}` takes `{1}` as a value and `{2}` as a factory, which multiplies binds at calls passing a lambda in that position, keep one overload",
        "Overload sets take a method type parameter at one position as a value or as a parameterless delegate returning it, never both when each overload holds a delegate over that type parameter.");

    private static readonly Regex Word = new("[A-Z]?[a-z0-9]+|[A-Z]+(?![a-z])", RegexOptions.CultureInvariant, TimeSpan.FromSeconds(1));

    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => [Reader, Covered, Discarded, Fallback, Borrowed, Clash, Tested, Folder, Shadowed, Qualified, Restated, Forwarded, Factory];

    protected override void Start(CompilationStartAnalysisContext context) {
        SystemSymbols symbols = new(context.Compilation);
        SemanticModels models = new(context);
        Readers(context, models);
        if (symbols.Flags is { } flags) Enums(context, flags);
        Awaitables(context);
        Namespaces(context);
        ImmutableHashSet<string> projects = [
            with(StringComparer.Ordinal),
            .. context.Options.AnalyzerConfigOptionsProvider.GlobalOptions.TryGetValue("build_property.ProjectReferenceAssemblies", out string? listed)
                ? listed.Split([','], StringSplitOptions.RemoveEmptyEntries)
                : [],
        ];
        ImmutableArray<IAssemblySymbol> sources = [.. context.Compilation.SourceModule.ReferencedAssemblySymbols.Where(assembly => projects.Contains(assembly.Identity.Name))];
        Clashes(context, models, sources);
        Qualifications(context);
        Restatements(context, models);
        if (symbols.Type is { } type) Tests(context, sources, type);
        Overloads(context);
        Files(context);
    }

    // --- [READERS]

    private static IOperation? Expressed(IOperation block) =>
        block switch {
            IFieldInitializerOperation { Value: var value } => value,
            IMethodBodyOperation { BlockBody: { } body } => Expressed(body),
            IMethodBodyOperation { BlockBody: null, ExpressionBody: { } body } => Expressed(body),
            IBlockOperation { Operations: { Length: 1 } held } when held[0] is IReturnOperation { ReturnedValue: { } value } => value,
            IBlockOperation { Operations: { IsEmpty: false } held } when held[0] is IExpressionStatementOperation { Operation: var value } && held.Skip(1).All(static rest => rest.IsImplicit) => value,
            _ => null,
        };

    private static void Readers(CompilationStartAnalysisContext context, SemanticModels models) {
        ImmutableArray<INamedTypeSymbol> cells = [
            .. Declared(context.Compilation, "T:System.Lazy`1", "T:System.Threading.ThreadLocal`1", "T:System.Threading.AsyncLocal`1", "T:System.Runtime.CompilerServices.StrongBox`1",
                "T:System.Threading.Channels.Channel`1", "T:System.Threading.Channels.Channel`2").OfType<INamedTypeSymbol>(),
            .. context.Compilation.GetTypeByMetadataName("LanguageExt.Atom`1")?.ContainingNamespace.GetTypeMembers()
                .Where(static type => type.Name is "Atom" or "AtomHashMap" or "AtomSeq" or "AtomQue" or "Ref" or "VersionHashMap") ?? [],
        ];
        ImmutableArray<INamedTypeSymbol> sets = [.. new[] {
            context.Compilation.GetTypeByMetadataName("System.Collections.Generic.HashSet`1"),
            context.Compilation.GetTypeByMetadataName("System.Collections.Frozen.FrozenSet`1"),
            context.Compilation.GetTypeByMetadataName("System.Collections.Immutable.ImmutableHashSet`1"),
        }.OfType<INamedTypeSymbol>()];
        ImmutableArray<IMethodSymbol> contains = [
            .. sets.SelectMany(static set => Members(set, "Contains")),
            .. Members(context.Compilation.GetTypeByMetadataName("System.Collections.Generic.ICollection`1"), "Contains"),
            .. Members(context.Compilation.GetTypeByMetadataName("System.Collections.Generic.IReadOnlySet`1"), "Contains"),
            .. Declared(context.Compilation, "T:System.Linq.Enumerable", "T:System.MemoryExtensions").OfType<INamedTypeSymbol>()
                .SelectMany(static owner => Members(owner, "Contains")).Where(static method => method.Parameters.Length is 2),
        ];
        ImmutableArray<IMethodSymbol> factories = [
            .. Members(context.Compilation.GetTypeByMetadataName("System.Collections.Frozen.FrozenSet"), "ToFrozenSet"),
            .. Members(context.Compilation.GetTypeByMetadataName("System.Collections.Immutable.ImmutableHashSet"), "ToImmutableHashSet"),
            .. Members(context.Compilation.GetTypeByMetadataName("System.Linq.Enumerable"), "ToHashSet", "ToArray"),
        ];
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
        ImmutableArray<IOperation> Elements(IOperation value) {
            while (Stripped(value) is IInvocationOperation { Instance: null, Arguments: { IsEmpty: false } arguments, TargetMethod: { } method } && Among(method, factories)
                && arguments[0].Value is { Syntax.Parent: MemberAccessExpressionSyntax } source
                && arguments.Skip(1).All(static argument => argument.Value.ConstantValue is { HasValue: true, Value: null })) value = source;
            return Stripped(value) switch {
                ICollectionExpressionOperation collection => collection.Elements,
                IArrayCreationOperation { Initializer: { } initializer } => initializer.ElementValues,
                IObjectCreationOperation { Type: { } type, Arguments.IsEmpty: true, Initializer: { } initializer } when Among(type, sets) && initializer.Initializers.All(static added => added is IInvocationOperation { Arguments.Length: 1 }) =>
                    [.. initializer.Initializers.OfType<IInvocationOperation>().Select(static added => added.Arguments[0].Value)],
                _ => [],
            };
        }
        bool Listed(IFieldSymbol field, IOperation value) =>
            !field.IsConst && Elements(value) is { IsEmpty: false } elements && elements.All(static element => element.ConstantValue.HasValue)
            && (Stripped(value).Type is IArrayTypeSymbol || Among(Stripped(value).Type, sets));
        static bool Literal(IFieldSymbol field, IOperation value) =>
            value is ILiteralOperation or IUnaryOperation { Operand: ILiteralOperation } or IFieldReferenceOperation { Field.IsConst: true } && SymbolEqualityComparer.Default.Equals(value.Type, field.Type);
        static ImmutableHashSet<string> Words(string name) => [with(StringComparer.Ordinal), .. Word.Matches(name).Cast<Match>().Select(static word => word.Value.ToUpperInvariant())];
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
        bool Contained(IOperation read) =>
            Consumer(read) switch {
                IInvocationOperation { TargetMethod: { } method, Instance: not null, Arguments.Length: 1 } => Among(method, contains),
                IArgumentOperation { Parameter.Ordinal: 0, Parent: IInvocationOperation { TargetMethod: { } method, Arguments.Length: 2 } } => Among(method, contains),
                _ => false,
            };
        static bool Targeted(IOperation value) {
            Stack<IOperation> remaining = new([value]);
            while (remaining.Count is not 0) {
                IOperation current = Stripped(remaining.Pop());
                if (current is ICollectionExpressionOperation or IDefaultValueOperation or IAnonymousFunctionOperation or IDelegateCreationOperation or IThrowOperation
                    or ILiteralOperation { ConstantValue: { HasValue: true, Value: null } } or IObjectCreationOperation { Syntax: ImplicitObjectCreationExpressionSyntax }
                    or IConditionalOperation { WhenFalse: null }) return true;
                foreach (IOperation child in current.ChildOperations.Where(child =>
                    (current is IConditionalOperation conditional && (child == conditional.WhenTrue || child == conditional.WhenFalse))
                    || (current is ISwitchExpressionOperation && child is ISwitchExpressionArmOperation)
                    || (current is ISwitchExpressionArmOperation arm && child == arm.Value))) remaining.Push(child);
            }
            return false;
        }
        static bool Pure(IOperation value) =>
            !value.DescendantsAndSelf().Any(static held => held is IInvocationOperation or IObjectCreationOperation or ITypeParameterObjectCreationOperation or IAnonymousObjectCreationOperation
                or IArrayCreationOperation or ICollectionExpressionOperation or IAssignmentOperation or IIncrementOrDecrementOperation or IAwaitOperation or IAnonymousFunctionOperation
                or IDelegateCreationOperation or IThrowOperation or IDynamicInvocationOperation or IDynamicMemberReferenceOperation or IDynamicIndexerAccessOperation
                or IArrayElementReferenceOperation
                or IPropertyReferenceOperation or IEventReferenceOperation or IFieldReferenceOperation { ConstantValue.HasValue: false }
                or IConversionOperation { OperatorMethod: not null } or IConversionOperation { IsImplicit: false, IsTryCast: false }
                or IUnaryOperation { OperatorMethod: not null } or IUnaryOperation { IsChecked: true }
                or IBinaryOperation { OperatorMethod: not null } or IBinaryOperation { IsChecked: true }
                or IBinaryOperation { OperatorKind: BinaryOperatorKind.Divide or BinaryOperatorKind.Remainder });
        static bool Deferred(SyntaxNode node, SyntaxNode root) =>
            node.AncestorsAndSelf().TakeWhile(held => held != root).Any(static held => held.Parent switch {
                ConditionalExpressionSyntax conditional => conditional.Condition != held,
                BinaryExpressionSyntax binary => binary.Right == held && binary.Kind() is SyntaxKind.LogicalAndExpression or SyntaxKind.LogicalOrExpression or SyntaxKind.CoalesceExpression,
                ConditionalAccessExpressionSyntax access => access.WhenNotNull == held,
                SwitchExpressionArmSyntax or AnonymousFunctionExpressionSyntax or QueryExpressionSyntax => true,
                _ => false,
            });
        static SyntaxNode Unwrapped(SyntaxNode node) => node.AncestorsAndSelf().First(static held => held.Parent is not ParenthesizedExpressionSyntax);
        static int Level(SyntaxNode node) =>
            node switch {
                IfStatementSyntax { Parent: ElseClauseSyntax } => 0,
                ConditionalExpressionSyntax conditional when Unwrapped(conditional) is { Parent: ConditionalExpressionSyntax outer } alternative && outer.WhenFalse == alternative => 0,
                IfStatementSyntax or ConditionalExpressionSyntax or CommonForEachStatementSyntax or ForStatementSyntax or WhileStatementSyntax or DoStatementSyntax or SwitchStatementSyntax
                    or SwitchExpressionSyntax or AnonymousFunctionExpressionSyntax or LocalFunctionStatementSyntax => 1,
                _ => 0,
            };
        static int Depth(SyntaxNode node) {
            Dictionary<SyntaxNode, int> levels = [];
            foreach (SyntaxNode held in node.DescendantNodesAndSelf()) levels[held] = Level(held) + (held == node ? 0 : levels[held.Parent!]);
            return levels.Values.Max();
        }
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
        ConcurrentDictionary<ISymbol, IOperation> values = new(SymbolEqualityComparer.Default);
        ConcurrentDictionary<ISymbol, (IOperation Read, ISymbol Reader, int Count)> reads = new(SymbolEqualityComparer.Default);
        ConcurrentDictionary<ISymbol, byte> named = new(SymbolEqualityComparer.Default);
        ConcurrentDictionary<IPropertySymbol, IOperation> forwards = new(SymbolEqualityComparer.Default);
        ConcurrentBag<(IPropertyReferenceOperation Read, INamedTypeSymbol Reader)> propertyReads = [];
        bool Forwarding(IPropertySymbol property, IOperation returned, CancellationToken token) {
            if (property is { SetMethod: not null } or { IsVirtual: true } or { IsAbstract: true } or { IsOverride: true } or { IsExtern: true }
                or { IsPartialDefinition: true } or { PartialDefinitionPart: not null } or { ReturnsByRef: true } or { ReturnsByRefReadonly: true }
                || !property.GetAttributes().IsEmpty || property.GetMethod is not { } getter || !getter.GetAttributes().IsEmpty || !getter.GetReturnTypeAttributes().IsEmpty
                || property.DeclaringSyntaxReferences.Any(declared => declared.GetSyntax(token) is BasePropertyDeclarationSyntax syntax && syntax.Modifiers.Any(SyntaxKind.NewKeyword))
                || property.ContainingType.AllInterfaces.SelectMany(static face => face.GetMembers()).OfType<IPropertySymbol>()
                    .Any(member => Same(property.ContainingType.FindImplementationForInterfaceMember(member), property)))
                return false;
            bool Rooted(IOperation value) {
                while (true) {
                    token.ThrowIfCancellationRequested();
                    if (value is IConditionalAccessInstanceOperation conditional
                        && Ancestor<IConditionalAccessOperation>(conditional, held => held.Operation.Syntax == conditional.Syntax) is { } access) {
                        value = access.Operation;
                        continue;
                    }
                    if (value is not IMemberReferenceOperation member) return value is IParameterReferenceOperation parameter
                        && parameter.Parameter.ContainingSymbol is IMethodSymbol { MethodKind: MethodKind.Constructor } constructor
                        && Same(constructor.ContainingType, property.ContainingType) && !Among(value.Type, cells);
                    if (member.Instance is null) return member.Member.IsStatic
                        && (!Same(member.Member.ContainingType, property.ContainingType) || !Among(value.Type, cells));
                    if (member.Instance is IInstanceReferenceOperation { ReferenceKind: InstanceReferenceKind.ContainingTypeInstance })
                        return !Among(value.Type, cells);
                    value = member.Instance;
                }
            }
            bool Parameter(IOperation value, int ordinal) => value is IParameterReferenceOperation parameter
                && Same(Member(parameter.Parameter.ContainingSymbol), property) && parameter.Parameter.Ordinal == ordinal;
            bool Indexed(ImmutableArray<IArgumentOperation> arguments) => arguments.Length == property.Parameters.Length
                && arguments.All(argument => argument is { ArgumentKind: ArgumentKind.Explicit, Parameter: { } parameter } && Parameter(argument.Value, parameter.Ordinal));
            IOperation value = returned;
            if (value is IDelegateCreationOperation { IsImplicit: true } creation) value = creation.Target;
            while (value is IConditionalAccessOperation access) {
                if (!Rooted(access.Operation)) return false;
                value = access.WhenNotNull;
            }
            return value switch {
                IPropertyReferenceOperation { Instance: { } instance, Property.IsIndexer: true } indexer => Rooted(instance) && Indexed(indexer.Arguments),
                IArrayElementReferenceOperation element => Rooted(element.ArrayReference) && element.Indices.Length == property.Parameters.Length
                    && element.Indices.Select((index, ordinal) => Parameter(index, ordinal)).All(static same => same),
                IMemberReferenceOperation member when member is IFieldReferenceOperation or IPropertyReferenceOperation or IMethodReferenceOperation => member.Instance is { } instance
                    ? Rooted(instance) : member.Member.IsStatic && !Same(member.Member.ContainingType, property.ContainingType),
                _ => false,
            };
        }
        context.RegisterOperationBlockAction(block => {
            if (block.IsGeneratedCode) return;
            ISymbol owner = Member(block.OwningSymbol);
            if (block.OperationBlocks is not { Length: 1 } blocks || Expressed(blocks[0]) is not { } value) return;
            if (block.OwningSymbol is IMethodSymbol { MethodKind: MethodKind.PropertyGet } && owner is IPropertySymbol property && Forwarding(property, value, block.CancellationToken)) {
                forwards[property] = value;
                return;
            }
            if (Candidate(owner) && owner.DeclaringSyntaxReferences is { Length: 1 } declared && Shaped(declared[0].GetSyntax(block.CancellationToken))) values[owner] = value;
        });
        context.RegisterOperationAction(
            analysis => {
                if (analysis.Operation is INameOfOperation name) {
                    SymbolInfo info = analysis.Operation.SemanticModel!.GetSymbolInfo(name.Argument.Syntax, analysis.CancellationToken);
                    foreach (ISymbol symbol in info.Symbol is { } found ? [found] : info.CandidateSymbols)
                        _ = named.TryAdd(symbol.OriginalDefinition, 0);
                    return;
                }
                if (analysis.Operation is IPropertyReferenceOperation { Property.DeclaredAccessibility: Accessibility.Private } property && analysis.ContainingSymbol.ContainingType is { } containing)
                    propertyReads.Add((property, containing));
                if (Target(analysis.Operation)?.OriginalDefinition is not { } target || !Candidate(target)) return;
                ISymbol reader = Member(Enclosing(analysis.ContainingSymbol));
                _ = reads.AddOrUpdate(target, (analysis.Operation, reader, 1), static (_, held) => (held.Read, held.Reader, held.Count + 1));
            },
            OperationKind.FieldReference,
            OperationKind.PropertyReference,
            OperationKind.Invocation,
            OperationKind.MethodReference,
            OperationKind.NameOf);
        models.Complete((end, available) => {
            bool Public(ISymbol source, INamedTypeSymbol owner) {
                Stack<ISymbol> remaining = new([source]);
                while (remaining.Count is not 0) {
                    end.CancellationToken.ThrowIfCancellationRequested();
                    ISymbol symbol = remaining.Pop();
                    if (symbol is ITypeParameterSymbol or IDynamicTypeSymbol || SymbolEqualityComparer.Default.Equals(symbol, owner)) continue;
                    if (symbol is IArrayTypeSymbol array) {
                        remaining.Push(array.ElementType);
                        continue;
                    }
                    if (symbol is IErrorTypeSymbol || symbol.DeclaredAccessibility is not Accessibility.Public) return false;
                    if (symbol.ContainingType is { } containing) remaining.Push(containing);
                    foreach (ITypeSymbol argument in symbol switch {
                        INamedTypeSymbol type => type.TypeArguments,
                        IMethodSymbol method => method.TypeArguments,
                        _ => [],
                    }) remaining.Push(argument);
                    if (symbol is IPropertySymbol property) {
                        if (property.GetMethod is not { } getter) return false;
                        remaining.Push(getter);
                    }
                }
                return true;
            }
            IEqualityComparer<IPropertySymbol> propertyComparer = SymbolEqualityComparer.Default;
            ILookup<IPropertySymbol, (IPropertyReferenceOperation Read, INamedTypeSymbol Reader)> privateReads = propertyReads
                .ToLookup(static held => held.Read.Property.OriginalDefinition, propertyComparer);
            bool Accessible(IPropertySymbol property, IOperation value) {
                ImmutableArray<IMemberReferenceOperation> members = [.. value.DescendantsAndSelf().OfType<IMemberReferenceOperation>()];
                ImmutableArray<IParameterReferenceOperation> parameters = [.. value.DescendantsAndSelf().OfType<IParameterReferenceOperation>()
                    .Where(static read => read.Parameter.ContainingSymbol is IMethodSymbol { MethodKind: MethodKind.Constructor })];
                if (property.DeclaredAccessibility is not Accessibility.Private)
                    return parameters.IsEmpty && members.All(member => Public(member.Member, property.ContainingType));
                if (named.ContainsKey(property.OriginalDefinition)) return false;
                IEnumerable<(IPropertyReferenceOperation Read, INamedTypeSymbol Reader)> uses = privateReads[property.OriginalDefinition];
                return uses.Any() && uses.All(held => {
                    end.CancellationToken.ThrowIfCancellationRequested();
                    if (!SymbolEqualityComparer.Default.Equals(held.Reader, property.ContainingType)
                        || held.Read.Instance is not (null or IInstanceReferenceOperation { ReferenceKind: InstanceReferenceKind.ContainingTypeInstance })) return false;
                    bool Bound(ISymbol symbol, SyntaxNode syntax) {
                        if (syntax is not ExpressionSyntax expression) return false;
                        SymbolInfo binding = held.Read.SemanticModel!.GetSpeculativeSymbolInfo(held.Read.Syntax.SpanStart, expression, SpeculativeBindingOption.BindAsExpression);
                        return SymbolEqualityComparer.Default.Equals(binding.Symbol, symbol) || (symbol is IMethodSymbol
                            && binding.CandidateReason is CandidateReason.MemberGroup && binding.CandidateSymbols.Contains(symbol, SymbolEqualityComparer.Default));
                    }
                    return parameters.All(parameter => Bound(parameter.Parameter, parameter.Syntax)) && members.All(member =>
                        context.Compilation.IsSymbolAccessibleWithin(member.Member, held.Reader, member.Instance?.Type)
                        && (member is not IPropertyReferenceOperation getter || (getter.Property.GetMethod is { } method
                            && context.Compilation.IsSymbolAccessibleWithin(method, held.Reader, getter.Instance?.Type)))
                        && (member.Instance is not (null or IInstanceReferenceOperation) || Bound(member.Member, member.Syntax)));
                });
            }
            foreach (KeyValuePair<IPropertySymbol, IOperation> pair in forwards) {
                end.CancellationToken.ThrowIfCancellationRequested();
                if (Accessible(pair.Key, pair.Value)) end.ReportDiagnostic(Diagnostic.Create(Forwarded, pair.Key.Locations[0], pair.Key.Name, pair.Value.Syntax));
            }
            ImmutableHashSet<ISymbol> candidates = ImmutableHashSet.CreateRange(SymbolEqualityComparer.Default,
                values.Keys.Where(member => reads.TryGetValue(member, out (IOperation Read, ISymbol Reader, int Count) read) && read.Count is 1));
            bool Substituted(IOperation value, int nesting, ImmutableHashSet<string> local, ImmutableArray<IParameterSymbol> parameters, ImmutableArray<IArgumentOperation> arguments) {
                Reading reading = new(value.SemanticModel!, available, end.CancellationToken);
                bool Plain(IOperation operand) =>
                    operand.Syntax is IdentifierNameSyntax && Variable(operand) is { } held and not IParameterSymbol { RefKind: not RefKind.None }
                    && !References(held, operand, reading).Any(Written);
                SyntaxNode body = value.Syntax;
                ImmutableHashSet<string> names = [with(StringComparer.Ordinal), .. parameters.Select(static parameter => parameter.Name)];
                IEqualityComparer<IParameterSymbol> comparer = SymbolEqualityComparer.Default;
                ILookup<IParameterSymbol, IParameterReferenceOperation> used = value.DescendantsAndSelf().OfType<IParameterReferenceOperation>().ToLookup(static read => read.Parameter, comparer);
                ImmutableHashSet<string> passed = [with(StringComparer.Ordinal), .. arguments.SelectMany(static argument => Scopes.Referenced(argument.Syntax)).Select(static name => name.Identifier.ValueText)];
                bool pure = Pure(value);
                return value is not IConversionOperation { IsImplicit: true } && !Targeted(value) && Free(body, names, local)
                    && body.SyntaxTree.GetLineSpan(body.Span, end.CancellationToken) is var lines && lines.StartLinePosition.Line == lines.EndLinePosition.Line
                    && !value.DescendantsAndSelf().OfType<INameOfOperation>().Any()
                    && !Scopes.Declared(body).Any(name => names.Contains(name) || local.Contains(name) || passed.Contains(name))
                    && nesting + Depth(body) <= 3
                    && arguments.All(argument => argument is { ArgumentKind: ArgumentKind.Explicit, Parameter: { } parameter, Value: var passedValue }
                        && passedValue is not IConversionOperation { IsImplicit: true }
                        && (Plain(passedValue) || passedValue.ConstantValue.HasValue || (pure && Pure(passedValue) && used[parameter].Take(2).ToImmutableArray() is { Length: 1 } uses && !Deferred(uses[0].Syntax, body))));
            }
            bool Inlined(ISymbol member, IOperation value, IOperation read, ISymbol reader) {
                if (read.Syntax.SyntaxTree != value.Syntax.SyntaxTree || !Same(reader.ContainingType, member.ContainingType) || Inside<IAttributeOperation>(read) || Inside<IParameterInitializerOperation>(read)
                    || Inside<INameOfOperation>(read) || (Unsafe(value.Syntax) && !Unsafe(read.Syntax)) || read.Syntax.FirstAncestorOrSelf<MemberDeclarationSyntax>() is not { } site
                    || (read switch { IMemberReferenceOperation reference => reference.Instance, IInvocationOperation call => call.Instance, _ => null }) is not (null or IInstanceReferenceOperation { ReferenceKind: InstanceReferenceKind.ContainingTypeInstance }))
                    return false;
                ImmutableHashSet<string> local = [with(StringComparer.Ordinal), .. Scopes.Declared(site), "value"];
                bool Kept(IOperation named) =>
                    Words(member.Name).IsSubsetOf((Naming(named, candidates) is { } name ? Words(name) : [with(StringComparer.Ordinal)]).Union(
                        value.Syntax.DescendantTokens().Where(static token => token.IsKind(SyntaxKind.IdentifierToken)).SelectMany(static token => Words(token.ValueText))));
                return (member, read) switch {
                    (IFieldSymbol field, _) when Listed(field, value) => Contained(read) && Free(value.Syntax, [with(StringComparer.Ordinal)], local),
                    (IFieldSymbol field, _) => Literal(field, value) && Naming(read, candidates) is { } name && Words(field.Name).IsSubsetOf(Words(name)) && Free(value.Syntax, [with(StringComparer.Ordinal)], local),
                    (IPropertySymbol, IPropertyReferenceOperation) => Kept(read) && Substituted(value, Nesting(read.Syntax), local, [], []),
                    (IMethodSymbol method, IInvocationOperation call) => Kept(read) && Substituted(value, Nesting(read.Syntax), local, method.Parameters, call.Arguments),
                    (IMethodSymbol method, IMethodReferenceOperation { Parent: IDelegateCreationOperation creation } group) when !Grouped(group) =>
                        Kept(creation) && Substituted(value, Nesting(read.Syntax) + 1, local, method.Parameters, []),
                    _ => false,
                };
            }
            HashSet<ISymbol> processed = [with(SymbolEqualityComparer.Default)];
            HashSet<ISymbol> cyclic = [with(SymbolEqualityComparer.Default)];
            foreach (ISymbol member in candidates) {
                end.CancellationToken.ThrowIfCancellationRequested();
                List<ISymbol> path = [];
                Dictionary<ISymbol, int> positions = [with(SymbolEqualityComparer.Default)];
                ISymbol next = member;
                while (!processed.Contains(next) && reads.TryGetValue(next, out (IOperation Read, ISymbol Reader, int Count) read) && read.Count is 1 && candidates.Contains(read.Reader)) {
                    if (positions.TryGetValue(next, out int cycle)) {
                        cyclic.UnionWith(path.Skip(cycle));
                        break;
                    }
                    positions.Add(next, path.Count);
                    path.Add(next);
                    next = read.Reader;
                }
                processed.UnionWith(path);
            }
            foreach (KeyValuePair<ISymbol, (IOperation Read, ISymbol Reader, int Count)> pair in reads) {
                end.CancellationToken.ThrowIfCancellationRequested();
                if (pair.Value.Count is 1 && !named.ContainsKey(pair.Key) && !cyclic.Contains(pair.Key) && values.TryGetValue(pair.Key, out IOperation? value) && Inlined(pair.Key, value, pair.Value.Read, pair.Value.Reader))
                    end.ReportDiagnostic(Diagnostic.Create(Reader, pair.Key.Locations[0], [pair.Value.Read.Syntax.GetLocation()], pair.Key.Name, Named(pair.Value.Reader)));
            }
        });
    }

    // --- [ENUMS]

    private static void Enums(CompilationStartAnalysisContext context, INamedTypeSymbol flags) {
        static IEnumerable<object?> Named(IOperation? test, CancellationToken token) {
            Stack<IOperation> remaining = new(test is null ? [] : [test]);
            while (remaining.Count is not 0) {
                token.ThrowIfCancellationRequested();
                IOperation pattern = remaining.Pop();
                if (pattern is IConstantPatternOperation { Value.ConstantValue: { HasValue: true } constant }) yield return constant.Value;
                foreach (IPatternOperation child in pattern.ChildOperations.OfType<IPatternOperation>()
                    .Where(static child => child.Parent is IBinaryPatternOperation { OperatorKind: BinaryOperatorKind.Or })) remaining.Push(child);
            }
        }
        static bool CatchesRest((IOperation Arm, IOperation? Test, IOperation? Guard) arm, CancellationToken token) =>
            CatchesAll(arm, token) || (arm is { Guard: null, Test: INegatedPatternOperation { Pattern: var excluded } } && Named(excluded, token).Any());
        Report<ISwitchExpressionOperation>(context, [OperationKind.SwitchExpression], (dispatch, analysis) => {
            if (Dispatch(dispatch) is not (_, var arms) || dispatch.Value.Type is not INamedTypeSymbol { TypeKind: TypeKind.Enum } subject
                || !Sourced(subject) || Marked(subject, flags) is not null) return [];
            ImmutableHashSet<object?> named = [.. arms.Where(static arm => arm.Guard is null).SelectMany(arm => Named(arm.Test, analysis.CancellationToken))];
            ImmutableArray<IFieldSymbol> unnamed = [.. subject.GetMembers().OfType<IFieldSymbol>().Where(member => member.HasConstantValue && !named.Contains(member.ConstantValue))];
            return named.IsEmpty ? [] : arms.Where(arm => CatchesRest(arm, analysis.CancellationToken)).Select(arm => Diagnostic.Create(
                unnamed.IsEmpty ? Covered : Fallback,
                arm.Arm.Syntax.GetLocation(),
                subject.Name,
                string.Join(", ", unnamed.Select(member => $"`{subject.Name}.{member.Name}`"))));
        });
    }

    // --- [AWAITABLES]

    private static void Awaitables(CompilationStartAnalysisContext context) {
        static bool Awaitable(IOperation value, OperationAnalysisContext analysis) {
            if (value.Type is not { } type || analysis.Operation.SemanticModel is not { } source) return false;
            int position = analysis.Operation.Syntax.SpanStart;
            SyntaxAnnotation mark = new();
            ExpressionStatementSyntax statement = SyntaxFactory.ExpressionStatement(
                SyntaxFactory.AwaitExpression(SyntaxFactory.DefaultExpression(SyntaxFactory.ParseTypeName(type.ToMinimalDisplayString(source, position)))).WithAdditionalAnnotations(mark));
            return source.TryGetSpeculativeSemanticModel(position, statement, out SemanticModel? model)
                && model.GetAwaitExpressionInfo(model.SyntaxTree.GetRoot(analysis.CancellationToken).GetAnnotatedNodes(mark).OfType<AwaitExpressionSyntax>().Single())
                    is { GetAwaiterMethod: not null, IsCompletedProperty: not null, GetResultMethod: not null };
        }
        ImmutableArray<ISymbol> requests = Declared(
            context.Compilation,
            "M:Grasshopper2.Doc.SolutionServer.Start(Grasshopper2.Doc.SolutionMode)",
            "M:Grasshopper2.Doc.SolutionServer.Start(System.Threading.CancellationTokenSource,Grasshopper2.Doc.SolutionMode)");
        Report<ISimpleAssignmentOperation>(context, [OperationKind.SimpleAssignment], (assignment, analysis) =>
            assignment is { Target: IDiscardOperation, Value: var value } && Stripped(value) is var awaited && Awaitable(awaited, analysis)
            && !(awaited is IInvocationOperation { TargetMethod: var requested } && requests.Any(request => Same(request, requested)))
                ? [Diagnostic.Create(Discarded, assignment.Syntax.GetLocation(), awaited.Syntax)]
                : []);
    }

    // --- [NAMESPACES]

    private static void Namespaces(CompilationStartAnalysisContext context) =>
        context.RegisterSyntaxNodeAction(
            static node => {
                if (node.IsGeneratedCode) return;
                BaseNamespaceDeclarationSyntax declaration = (BaseNamespaceDeclarationSyntax)node.Node;
                if (node.SemanticModel.GetDeclaredSymbol(declaration, node.CancellationToken) is INamespaceSymbol declared
                    && node.Compilation.GetCompilationNamespace(declared)?.ConstituentNamespaces.FirstOrDefault(space => space.ContainingAssembly is { } owner && !Same(owner, node.Compilation.Assembly)) is { ContainingAssembly: { } assembly })
                    node.ReportDiagnostic(Diagnostic.Create(Borrowed, declaration.Name.GetLocation(), declared.ToDisplayString(), assembly.Name));
            },
            SyntaxKind.NamespaceDeclaration,
            SyntaxKind.FileScopedNamespaceDeclaration);

    // --- [CLASHES]

    private static void Clashes(CompilationStartAnalysisContext context, SemanticModels models, ImmutableArray<IAssemblySymbol> sources) {
        ConcurrentBag<(INamedTypeSymbol Type, Location Location)> declarations = [];
        context.RegisterSyntaxNodeAction(
            node => {
                if (node.IsGeneratedCode) return;
                if (node.SemanticModel.GetDeclaredSymbol(node.Node, node.CancellationToken) is not INamedTypeSymbol { ContainingType: null } type
                    || type.DeclaringSyntaxReferences.FirstOrDefault() is not { } first || first.SyntaxTree != node.Node.SyntaxTree || first.Span != node.Node.Span) return;
                declarations.Add((type, node.Node switch {
                    BaseTypeDeclarationSyntax declaration => declaration.Identifier.GetLocation(),
                    DelegateDeclarationSyntax declaration => declaration.Identifier.GetLocation(),
                    var other => other.GetLocation(),
                }));
            },
            SyntaxKind.ClassDeclaration,
            SyntaxKind.StructDeclaration,
            SyntaxKind.RecordDeclaration,
            SyntaxKind.RecordStructDeclaration,
            SyntaxKind.InterfaceDeclaration,
            SyntaxKind.EnumDeclaration,
            SyntaxKind.DelegateDeclaration);
        models.Complete((end, availableModels) => {
            IAssemblySymbol own = end.Compilation.Assembly;
            bool Exposed(INamedTypeSymbol other) => other.DeclaredAccessibility is Accessibility.Public && !Same(other.ContainingAssembly, own);
            List<INamedTypeSymbol> referenced = [];
            Stack<INamespaceSymbol> remaining = new(sources.Select(static source => source.GlobalNamespace));
            while (remaining.Count is not 0) {
                end.CancellationToken.ThrowIfCancellationRequested();
                INamespaceSymbol space = remaining.Pop();
                referenced.AddRange(space.GetTypeMembers().Where(Exposed));
                foreach (INamespaceSymbol nested in space.GetNamespaceMembers()) remaining.Push(nested);
            }
            ILookup<(string Name, int Arity), INamedTypeSymbol> available = referenced.ToLookup(static type => (type.Name, type.Arity));
            foreach ((INamedTypeSymbol type, Location location) in declarations.OrderBy(static declared => declared.Location.SourceTree?.FilePath, StringComparer.Ordinal).ThenBy(static declared => declared.Location.SourceSpan.Start)) {
                end.CancellationToken.ThrowIfCancellationRequested();
                (string Name, int Arity) key = (type.Name, type.Arity);
                IEnumerable<INamedTypeSymbol> imported = type.DeclaringSyntaxReferences
                    .SelectMany(declared => availableModels[declared.SyntaxTree].GetImportScopes(declared.Span.Start, end.CancellationToken))
                    .SelectMany(static scope => scope.Imports).Select(static import => import.NamespaceOrType).OfType<INamespaceSymbol>()
                    .Distinct<INamespaceSymbol>(SymbolEqualityComparer.Default).SelectMany(space => space.GetTypeMembers(type.Name, type.Arity)).Where(Exposed)
                    .OrderBy(static found => found.ContainingAssembly.Identity.ToString(), StringComparer.Ordinal).ThenBy(static found => found.ToDisplayString(), StringComparer.Ordinal);
                if (imported.Concat(available[key]).FirstOrDefault() is { } clash)
                    end.ReportDiagnostic(Diagnostic.Create(Clash, location, type.ToDisplayString(SymbolDisplayFormat.MinimallyQualifiedFormat), clash.ToDisplayString()));
            }
        });
    }

    // --- [QUALIFICATIONS]

    private static void Qualifications(CompilationStartAnalysisContext context) {
        ConcurrentDictionary<SyntaxTree, ILookup<string, SimpleNameSyntax>> unqualified = new();
        ConcurrentDictionary<SyntaxTree, ConcurrentDictionary<INamespaceSymbol, bool>> ambiguous = new();
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
                if (node.IsGeneratedCode) return;
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
                if (owner is not (INamespaceSymbol or INamedTypeSymbol) || Sourced(owner)) return;
                ImmutableArray<ISymbol> scopes = [.. model.GetImportScopes(node.Node.SpanStart, token).SelectMany(static scope => scope.Imports).Select(static import => import.NamespaceOrType)];
                if ((owner is INamedTypeSymbol) != Among(owner, scopes) || model.GetSymbolInfo(right, token).Symbol is not { } target || target is INamespaceSymbol) return;
                ImmutableArray<ISymbol> visible = [.. model.LookupSymbols(node.Node.SpanStart, name: right.Identifier.ValueText).Where(found => !Same(found, target))];
                ILookup<string, SimpleNameSyntax> used = unqualified.GetOrAdd(
                    node.Node.SyntaxTree,
                    tree => tree.GetRoot(token).DescendantNodes().OfType<SimpleNameSyntax>().Where(Looked).ToLookup(static held => held.Identifier.ValueText, StringComparer.Ordinal));
                bool Ambiguous(INamedTypeSymbol member) =>
                    used[member.Name].Any(held => held.Arity == member.Arity && model.GetAliasInfo(held, token) is null
                        && model.GetSymbolInfo(held, token).Symbol is INamedTypeSymbol { ContainingType: null } bound && !Same(bound, member));
                if (visible.FirstOrDefault(Sourced) is { } shadowing)
                    node.ReportDiagnostic(Diagnostic.Create(Shadowed, node.Node.GetLocation(), node.Node, shadowing.ToDisplayString(SymbolDisplayFormat.MinimallyQualifiedFormat), owner.ToDisplayString()));
                else if (visible.IsEmpty && owner is INamespaceSymbol space) {
                    ImmutableHashSet<string> sourceNames = [with(StringComparer.Ordinal), .. model.LookupSymbols(node.Node.SpanStart).Where(Sourced).Select(static symbol => symbol.Name)];
                    ConcurrentDictionary<INamespaceSymbol, bool> imports = ambiguous.GetOrAdd(node.Node.SyntaxTree, static _ => new(SymbolEqualityComparer.Default));
                    if (!space.GetTypeMembers().Any(member => sourceNames.Contains(member.Name))
                        && !imports.GetOrAdd(space, importedSpace => importedSpace.GetTypeMembers().Any(Ambiguous)))
                        node.ReportDiagnostic(Diagnostic.Create(Qualified, node.Node.GetLocation(), node.Node, owner.ToDisplayString(), right));
                }
            },
            SyntaxKind.QualifiedName,
            SyntaxKind.SimpleMemberAccessExpression);
    }

    // --- [RESTATEMENTS]

    private sealed class ShapeComparer : EqualityComparer<ImmutableArray<(ISymbol Symbol, object? Constant)>> {
        public override bool Equals(ImmutableArray<(ISymbol Symbol, object? Constant)> x, ImmutableArray<(ISymbol Symbol, object? Constant)> y) =>
            x.Length == y.Length && x.Zip(y, static (one, other) => SymbolEqualityComparer.Default.Equals(one.Symbol, other.Symbol) && Equals(one.Constant, other.Constant)).All(static same => same);

        public override int GetHashCode(ImmutableArray<(ISymbol Symbol, object? Constant)> obj) =>
            obj.Aggregate(0, static (hash, part) => (hash, SymbolEqualityComparer.Default.GetHashCode(part.Symbol), part.Constant).GetHashCode());
    }

    private static void Restatements(CompilationStartAnalysisContext context, SemanticModels models) {
        ConcurrentDictionary<ISymbol, IOperation> initializers = new(SymbolEqualityComparer.Default);
        ConcurrentDictionary<IMethodSymbol, IOperation> factories = new(SymbolEqualityComparer.Default);
        ConcurrentDictionary<INamedTypeSymbol, ConcurrentBag<IOperation>> instanceInitializers = new(SymbolEqualityComparer.Default);
        ConcurrentBag<(IOperation Site, INamedTypeSymbol Type, ISymbol Owner)> sites = [];
        ThinktectureSymbols thinktecture = new(context.Compilation);
        static bool ValueBased(INamedTypeSymbol type) => type.IsRecord || (type.IsValueType && type.IsReadOnly);
        bool Item(ISymbol owner, INamedTypeSymbol type) =>
            Same(owner.ContainingType, type) && initializers.ContainsKey(owner) && Marked(type, thinktecture.SmartEnums) is not null;
        static INamedTypeSymbol? Made(IOperation operation) =>
            operation switch {
                IObjectCreationOperation { Constructor: { } made, Initializer: null } => made.ContainingType,
                IInvocationOperation { TargetMethod: { IsStatic: true } method } when Same(method.ReturnType, method.ContainingType) => method.ContainingType,
                _ => null,
            };
        context.RegisterOperationBlockAction(block => {
            if (block.OwningSymbol is not IMethodSymbol { IsStatic: true, TypeParameters.IsEmpty: true } method || !Same(method.ReturnType, method.ContainingType)
                || block.OperationBlocks is not { Length: 1 } bodies || Expressed(bodies[0]) is not { } value) return;
            IEqualityComparer<IParameterSymbol> comparer = SymbolEqualityComparer.Default;
            ILookup<IParameterSymbol, IParameterReferenceOperation> references = value.DescendantsAndSelf().OfType<IParameterReferenceOperation>().ToLookup(static read => read.Parameter, comparer);
            if (method.Parameters.All(parameter => references[parameter].Take(2).Count() is 1)) factories[method] = value;
        });
        context.RegisterOperationAction(
            analysis => {
                switch (analysis.Operation) {
                    case IFieldInitializerOperation { InitializedFields: { Length: > 0 } fields, Value: var value } when !fields[0].IsStatic && ValueBased(fields[0].ContainingType):
                        instanceInitializers.GetOrAdd(fields[0].ContainingType, static _ => []).Add(value);
                        break;
                    case IPropertyInitializerOperation { InitializedProperties: { Length: > 0 } properties, Value: var value } when !properties[0].IsStatic && ValueBased(properties[0].ContainingType):
                        instanceInitializers.GetOrAdd(properties[0].ContainingType, static _ => []).Add(value);
                        break;
                    case IFieldInitializerOperation { InitializedFields: { Length: 1 } fields, Value: var value } when fields[0] is { IsStatic: true, IsReadOnly: true } field && Same(field.Type, field.ContainingType):
                        initializers[field] = value;
                        break;
                    case IPropertyInitializerOperation { InitializedProperties: { Length: 1 } properties, Value: var value } when properties[0] is { IsStatic: true, SetMethod: null } property && Same(property.Type, property.ContainingType):
                        initializers[property] = value;
                        break;
                    case var operation:
                        if (!analysis.IsGeneratedCode && Made(operation) is { } type && Sourced(type) && type.TypeArguments.IsEmpty) sites.Add((operation, type, Member(analysis.ContainingSymbol)));
                        break;
                }
            },
            OperationKind.FieldInitializer,
            OperationKind.PropertyInitializer,
            OperationKind.ObjectCreation,
            OperationKind.Invocation);
        models.Complete((end, available) => {
            Dictionary<IMethodSymbol, bool> transparent = [with(SymbolEqualityComparer.Default)];
            bool Transparent(IMethodSymbol constructor) {
                Stack<(IMethodSymbol Constructor, bool Expanded)> pending = new([(constructor.OriginalDefinition, false)]);
                Dictionary<IMethodSymbol, ImmutableArray<IMethodSymbol>> dependencies = [with(SymbolEqualityComparer.Default)];
                while (pending.Count is not 0) {
                    end.CancellationToken.ThrowIfCancellationRequested();
                    (IMethodSymbol held, bool expanded) = pending.Pop();
                    if (expanded) {
                        transparent[held] = dependencies[held].All(called => transparent[called]);
                        continue;
                    }
                    if (transparent.ContainsKey(held)) continue;
                    INamedTypeSymbol type = held.ContainingType;
                    transparent[held] = type.SpecialType is SpecialType.System_Object or SpecialType.System_ValueType;
                    if (transparent[held]) continue;
                    ImmutableArray<IFieldSymbol> fields = [.. type.GetMembers().OfType<IFieldSymbol>().Where(static field => !field.IsStatic)];
                    if (!ValueBased(type) || held.Parameters.Any(static parameter => parameter.RefKind is not RefKind.None) || fields.Any(static field => !field.IsReadOnly)
                        || type.GetMembers().OfType<IPropertySymbol>().Any(static property => !property.IsStatic && property.SetMethod is { IsInitOnly: false })) continue;
                    ImmutableHashSet<ISymbol> automatic = fields.Select(static field => field.AssociatedSymbol).OfType<IPropertySymbol>().ToImmutableHashSet<ISymbol>(SymbolEqualityComparer.Default);
                    ImmutableArray<SyntaxNode> declarations = [.. held.DeclaringSyntaxReferences.Where(reference => end.Compilation.ContainsSyntaxTree(reference.SyntaxTree))
                        .Select(reference => reference.GetSyntax(end.CancellationToken))];
                    ImmutableArray<IOperation> bodies = [.. declarations.Select(declaration => available[declaration.SyntaxTree].GetOperation(declaration, end.CancellationToken)).OfType<IOperation>()];
                    TypeDeclarationSyntax? primary = declarations.OfType<TypeDeclarationSyntax>().FirstOrDefault(static declaration => declaration.ParameterList is not null);
                    if (bodies.IsEmpty && primary is null) continue;
                    IEnumerable<IOperation> operations = bodies.Concat(instanceInitializers.TryGetValue(type, out ConcurrentBag<IOperation>? initialized) ? initialized : []).SelectMany(static root => root.DescendantsAndSelf());
                    bool Plain(IOperation operation) => operation switch {
                        IConstructorBodyOperation or IBlockOperation or IExpressionStatementOperation or IArgumentOperation or IFieldInitializerOperation or IPropertyInitializerOperation or ISimpleAssignmentOperation
                            or IParameterReferenceOperation or IInstanceReferenceOperation or ILiteralOperation or IDefaultValueOperation or IReturnOperation { ReturnedValue: null } => true,
                        IConversionOperation { OperatorMethod: null } => true,
                        IInvocationOperation { TargetMethod.MethodKind: MethodKind.Constructor } or IObjectCreationOperation { Constructor: not null, Initializer: null } => true,
                        IFieldReferenceOperation field => field.Parent is ISimpleAssignmentOperation assignment && assignment.Target == field && field.Field.IsReadOnly && Same(field.Field.ContainingType, type),
                        IPropertyReferenceOperation property => property.Parent is ISimpleAssignmentOperation assignment && assignment.Target == property && automatic.Contains(property.Property),
                        _ => false,
                    };
                    ImmutableArray<IOperation> parts = [.. operations];
                    if (!parts.All(Plain)) continue;
                    IMethodSymbol? parent = bodies.IsEmpty && primary is not null && type.BaseType is { } baseType
                        ? baseType.InstanceConstructors.FirstOrDefault(static member => member.Parameters.IsEmpty)
                        : null;
                    if (bodies.IsEmpty && primary is not null && type.BaseType is not null && parent is null) continue;
                    ImmutableArray<IMethodSymbol> called = [.. parts.Select(static operation => operation switch {
                        IInvocationOperation call => call.TargetMethod,
                        IObjectCreationOperation creation => creation.Constructor,
                        _ => null,
                    }).Append(parent).OfType<IMethodSymbol>().Select(static member => member.OriginalDefinition).Distinct<IMethodSymbol>(SymbolEqualityComparer.Default)];
                    dependencies[held] = called;
                    pending.Push((held, true));
                    foreach (IMethodSymbol target in called) pending.Push((target, false));
                }
                return transparent[constructor.OriginalDefinition];
            }
            Dictionary<IOperation, ImmutableArray<(ISymbol Symbol, object? Constant)>?> shapes = [];
            ImmutableDictionary<IParameterSymbol, IOperation> unbound = ImmutableDictionary.Create<IParameterSymbol, IOperation>(SymbolEqualityComparer.Default);
            ImmutableHashSet<ISymbol> unseen = ImmutableHashSet.Create<ISymbol>(SymbolEqualityComparer.Default);
            ImmutableArray<(ISymbol Symbol, object? Constant)>? Shape(IOperation root) {
                if (shapes.TryGetValue(root, out ImmutableArray<(ISymbol Symbol, object? Constant)>? cached)) return cached;
                ImmutableArray<(ISymbol Symbol, object? Constant)>.Builder shape = ImmutableArray.CreateBuilder<(ISymbol Symbol, object? Constant)>();
                Stack<(IOperation Value, ImmutableDictionary<IParameterSymbol, IOperation> Arguments, ImmutableHashSet<ISymbol> Seen)> remaining = new([(root, unbound, unseen)]);
                while (remaining.Count is not 0) {
                    end.CancellationToken.ThrowIfCancellationRequested();
                    (IOperation value, ImmutableDictionary<IParameterSymbol, IOperation> arguments, ImmutableHashSet<ISymbol> seen) = remaining.Pop();
                    switch (value) {
                        case { ConstantValue: { HasValue: true, Value: decimal number }, Type: { } type }:
                            foreach (int bit in decimal.GetBits(number)) shape.Add((type, bit));
                            break;
                        case { ConstantValue: { HasValue: true, Value: var constant }, Type: { } type }:
                            shape.Add((type, constant switch { float single => BitConverter.DoubleToInt64Bits(single), double wide => BitConverter.DoubleToInt64Bits(wide), _ => constant }));
                            break;
                        case IConversionOperation { IsImplicit: true, OperatorMethod: null, Operand: var operand }:
                            remaining.Push((operand, arguments, seen));
                            break;
                        case IParameterReferenceOperation parameter when arguments.TryGetValue(parameter.Parameter, out IOperation? argument):
                            remaining.Push((argument, arguments, seen));
                            break;
                        case IObjectCreationOperation { Constructor: { } made, Initializer: null, Arguments: { IsEmpty: false } passed } when Transparent(made):
                            shape.Add((made, passed.Length));
                            foreach (IArgumentOperation item in passed.OrderByDescending(static item => item.Parameter!.Ordinal)) remaining.Push((item.Value, arguments, seen));
                            break;
                        case IInvocationOperation { TargetMethod: { IsStatic: true } method, Arguments: { IsEmpty: false } passed } when !seen.Contains(method) && factories.TryGetValue(method, out IOperation? body):
                            remaining.Push((body, arguments.SetItems(passed.Select(static item => new KeyValuePair<IParameterSymbol, IOperation>(item.Parameter!, item.Value))), seen.Add(method)));
                            break;
                        case IMemberReferenceOperation { Member: { ContainingType: { } holder } member } when Item(member, holder):
                            shape.Add((member, null));
                            break;
                        case IMemberReferenceOperation reference when !seen.Contains(reference.Member) && initializers.TryGetValue(reference.Member, out IOperation? initializer):
                            remaining.Push((initializer, arguments, seen.Add(reference.Member)));
                            break;
                        default:
                            shapes[root] = null;
                            return null;
                    }
                }
                return shapes[root] = shape.DrainToImmutable();
            }
            ILookup<ImmutableArray<(ISymbol Symbol, object? Constant)>, ISymbol> statics = initializers
                .Select(pair => (Member: pair.Key, Shape: Shape(pair.Value))).Where(static held => held.Shape is not null)
                .OrderBy(static held => held.Member.Locations[0].SourceTree?.FilePath, StringComparer.Ordinal).ThenBy(static held => held.Member.Locations[0].SourceSpan.Start)
                .ToLookup(static held => held.Shape!.Value, static held => held.Member, new ShapeComparer());
            foreach ((IOperation site, INamedTypeSymbol type, ISymbol owner) in sites)
                if (!Item(owner, type) && Shape(site) is { } shape && statics[shape].FirstOrDefault(member => Same(member.ContainingType, type) && !Same(member, owner)) is { } restated)
                    end.ReportDiagnostic(Diagnostic.Create(Restated, site.Syntax.GetLocation(), site.Syntax, $"{type.Name}.{restated.Name}"));
        });
    }

    // --- [TESTS]

    private static void Tests(CompilationStartAnalysisContext context, ImmutableArray<IAssemblySymbol> sources, INamedTypeSymbol type) {
        IAssemblySymbol own = context.Compilation.Assembly;
        ControlFlow flow = ControlFlow.For(context.Compilation);
        ImmutableArray<IMethodSymbol> runtime = Members(context.Compilation.GetSpecialType(SpecialType.System_Object), nameof(GetType));
        bool Concrete(ITypeSymbol? tested) =>
            tested is INamedTypeSymbol { TypeKind: TypeKind.Class or TypeKind.Struct, IsAbstract: false } named
            && (Same(named.ContainingAssembly, own) || sources.Any(source => Same(named.ContainingAssembly, source)));
        static bool External(ISymbol owner) => Overridden(owner).Skip(1).LastOrDefault() is { } origin && !Sourced(origin);
        ITypeParameterSymbol? Parameter(IOperation source, CancellationToken token) {
            Queue<ControlFlow.Value> pending = new(flow.Evaluations(source, token));
            HashSet<ControlFlow.Value> seen = [];
            ITypeParameterSymbol? origin = null;
            while (pending.Count is > 0) {
                token.ThrowIfCancellationRequested();
                ControlFlow.Value current = pending.Dequeue();
                if (!seen.Add(current)) continue;
                IOperation value = Stripped(current.Operation);
                ITypeParameterSymbol? parameter = value switch {
                    ITypeOfOperation { TypeOperand: ITypeParameterSymbol input } => input,
                    IInvocationOperation { Instance: { } instance } call when Among(call.TargetMethod, runtime) => Stripped(instance).Type as ITypeParameterSymbol,
                    _ => null,
                };
                if (parameter is not null) {
                    if (origin is not null && !Same(origin, parameter)) return null;
                    origin = parameter;
                    continue;
                }
                ImmutableArray<Optional<ControlFlow.Value>> definitions = value is ILocalReferenceOperation or IParameterReferenceOperation or IFlowCaptureReferenceOperation
                    ? flow.Values(current.With(value), token) : [];
                if (definitions.IsEmpty || definitions.Any(static definition => !definition.HasValue)) return null;
                foreach (Optional<ControlFlow.Value> definition in definitions) pending.Enqueue(definition.Value);
            }
            return origin;
        }
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
                Test(operation) is ( { } site, ITypeParameterSymbol input, var tested) && Concrete(tested)
                    ? [Diagnostic.Create(Tested, site.Syntax.GetLocation(), site.Syntax, input.Name, tested!.Name)]
                    : []);
        Report<IBinaryOperation>(context, [OperationKind.Binary], (binary, analysis) =>
            binary is { OperatorKind: BinaryOperatorKind.Equals or BinaryOperatorKind.NotEquals } && Same(binary.LeftOperand.Type, type) && Same(binary.RightOperand.Type, type)
            && !External(Member(analysis.ContainingSymbol))
            && (Stripped(binary.LeftOperand), Stripped(binary.RightOperand)) switch {
                (ITypeOfOperation typed, var other) when Concrete(typed.TypeOperand) => (Other: other, Typed: typed.TypeOperand),
                (var other, ITypeOfOperation typed) when Concrete(typed.TypeOperand) => (Other: other, Typed: typed.TypeOperand),
                _ => default,
            } is { Other: { } compared, Typed: { } concrete }
            && Parameter(compared, analysis.CancellationToken) is { } parameter
                ? [Diagnostic.Create(Tested, binary.Syntax.GetLocation(), binary.Syntax, parameter.Name, concrete.Name)]
                : []);
    }

    // --- [OVERLOADS]

    private static void Overloads(CompilationStartAnalysisContext context) {
        static bool Delegates(IParameterSymbol parameter, ITypeParameterSymbol held) =>
            parameter.Type is INamedTypeSymbol { DelegateInvokeMethod: { } invoke }
            && (Mentions(invoke.ReturnType, held) || invoke.Parameters.Any(argument => Mentions(argument.Type, held)));
        static IEnumerable<(IParameterSymbol Held, IParameterSymbol Made, IMethodSymbol Factory)> Pairs(IMethodSymbol value, ImmutableArray<IMethodSymbol> set) =>
            from held in value.Parameters
            where held.Type is ITypeParameterSymbol { TypeParameterKind: TypeParameterKind.Method } type
                && value.Parameters.Any(other => !SymbolEqualityComparer.Default.Equals(other, held) && Delegates(other, type))
            from factory in set
            where held.Ordinal < factory.Parameters.Length
                && factory.Parameters[held.Ordinal].Type is INamedTypeSymbol { DelegateInvokeMethod: { Parameters.IsEmpty: true, ReturnType: ITypeParameterSymbol { TypeParameterKind: TypeParameterKind.Method } made } }
                && made.Ordinal == ((ITypeParameterSymbol)held.Type).Ordinal
                && factory.Parameters.Any(other => other.Ordinal != held.Ordinal && Delegates(other, made))
            select (held, factory.Parameters[held.Ordinal], factory);
        context.RegisterSymbolAction(static analysis => {
            if (analysis.IsGeneratedCode) return;
            INamedTypeSymbol type = (INamedTypeSymbol)analysis.Symbol;
            ImmutableArray<ImmutableArray<IMethodSymbol>> sets = [.. type.GetMembers().OfType<IMethodSymbol>()
                .Where(static method => !method.TypeParameters.IsEmpty && !method.IsImplicitlyDeclared && !method.IsOverride && method.ExplicitInterfaceImplementations.IsEmpty)
                .GroupBy(static method => method.Name, StringComparer.Ordinal).Select(static named => named.ToImmutableArray()).Where(static named => named.Length > 1)];
            ImmutableHashSet<ISymbol> implementations = sets.IsEmpty ? [] : type.AllInterfaces.SelectMany(static face => face.GetMembers())
                .Select(type.FindImplementationForInterfaceMember).OfType<ISymbol>().ToImmutableHashSet(SymbolEqualityComparer.Default);
            foreach (ImmutableArray<IMethodSymbol> set in sets.Select(named => named.RemoveAll(implementations.Contains)).Where(static named => named.Length > 1))
                foreach (IMethodSymbol value in set)
                    foreach ((IParameterSymbol held, IParameterSymbol made, IMethodSymbol factory) in Pairs(value, set).Take(1)) {
                        analysis.CancellationToken.ThrowIfCancellationRequested();
                        analysis.ReportDiagnostic(Diagnostic.Create(Factory, value.Locations[0], [factory.Locations[0]], Label(value), held.Name, made.Name));
                    }
        }, SymbolKind.NamedType);
    }

    // --- [FILES]

    private static void Files(CompilationStartAnalysisContext context) =>
        context.RegisterSyntaxTreeAction(static tree => {
            if (tree.IsGeneratedCode) return;
            string path = tree.Tree.FilePath;
            if (Path.GetFileNameWithoutExtension(path) is { Length: > 0 } name && string.Equals(name, Path.GetFileName(Path.GetDirectoryName(path)), StringComparison.Ordinal))
                tree.ReportDiagnostic(Diagnostic.Create(Folder, tree.Tree.GetRoot(tree.CancellationToken).GetFirstToken().GetLocation(), Path.GetFileName(path)));
        });
}
