using System.Collections.Concurrent;
using System.Collections.Immutable;
using System.Text.RegularExpressions;
using Analyzer.Utilities;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Operations;

namespace Rasm.Analyzers;

// --- [COMPOSITION] ---------------------------------------------------------------------

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class SingleReaderAnalyzer : DiagnosticAnalyzer {

    // --- [RULES]

    private static readonly DiagnosticDescriptor Rule = new(
        "RASM3001",
        "Private member with one read, inline at the read",
        "`{0}` has one read, in `{1}`, inline `{0}` at the read",
        Categories.CSharp,
        DiagnosticSeverity.Error,
        isEnabledByDefault: true,
        "Inline private constants, `Contains` sets, and one-line expression-bodied members their type reads once when inlining keeps each name word.",
        Link("RASM3001"),
        WellKnownDiagnosticTags.CompilationEnd);

    private static readonly DiagnosticDescriptor Forwarded = new(
        "RASM3012",
        "Forwarding property, read member at its uses",
        "`{0}` forwards `{1}`, read `{1}` at each use",
        Categories.CSharp,
        DiagnosticSeverity.Error,
        isEnabledByDefault: true,
        "Properties and indexers add behavior or implement a contract instead of returning another owner's accessible member unchanged, counted across every reader in the assembly.",
        Link("RASM3012"),
        WellKnownDiagnosticTags.CompilationEnd);

    private static readonly DiagnosticDescriptor Folded = new(
        "RASM3015",
        "Member with one reader in the assembly, fold it into the reader",
        "`{0}` has one reader in the assembly, in `{1}`, fold `{0}` into `{1}`",
        Categories.CSharp,
        DiagnosticSeverity.Error,
        isEnabledByDefault: true,
        "Fold each non-private method, computed property, constant, and static read-only field no other assembly can read into its one reader, counted across every file of the compilation, generated code included, when every member and constructor parameter its body reads is accessible from the reader, the folded body stays within three nesting levels, each receiver and argument the read passes is a constant, an unwritten local or parameter, or a pure expression a pure body reads once outside a branch, loop, or lambda, no `[CallerArgumentExpression]` text the body captures changes, and a static read-only field holds a constant or its reader runs once in a static initializer.",
        Link("RASM3015"),
        WellKnownDiagnosticTags.CompilationEnd);

    private static readonly ImmutableArray<string> Sets = ["System.Collections.Generic.HashSet`1", "System.Collections.Frozen.FrozenSet`1", "System.Collections.Immutable.ImmutableHashSet`1"];

    private static readonly Regex Word = new("[A-Z]?[a-z0-9]+|[A-Z]+(?![a-z])", RegexOptions.CultureInvariant, TimeSpan.FromSeconds(1));

    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => [Rule, Forwarded, Folded];

    public override void Initialize(AnalysisContext context) {
        context.EnableConcurrentExecution();
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.Analyze);
        context.RegisterCompilationStartAction(Start);
    }

    private static void Start(CompilationStartAnalysisContext context) {
        WellKnownTypeProvider types = WellKnownTypeProvider.GetOrCreate(context.Compilation);
        SemanticModels models = new(context);
        ImmutableArray<INamedTypeSymbol> cells = Cells(context.Compilation);
        ImmutableArray<INamedTypeSymbol> sets = [.. Sets.Select(types.GetOrCreateTypeByMetadataName).OfType<INamedTypeSymbol>()];
        ImmutableArray<IMethodSymbol> contains = [
            .. sets.SelectMany(static set => Members(set, "Contains")),
            .. Members(types.GetOrCreateTypeByMetadataName("System.Collections.Generic.ICollection`1"), "Contains"),
            .. Members(types.GetOrCreateTypeByMetadataName("System.Collections.Generic.IReadOnlySet`1"), "Contains"),
            .. Declared(context.Compilation, "T:System.Linq.Enumerable", "T:System.MemoryExtensions").OfType<INamedTypeSymbol>()
                .SelectMany(static owner => Members(owner, "Contains")).Where(static method => method.Parameters.Length is 2),
        ];
        ImmutableArray<IMethodSymbol> factories = [
            .. Members(types.GetOrCreateTypeByMetadataName("System.Collections.Frozen.FrozenSet"), "ToFrozenSet"),
            .. Members(types.GetOrCreateTypeByMetadataName("System.Collections.Immutable.ImmutableHashSet"), "ToImmutableHashSet"),
            .. Members(types.GetOrCreateTypeByMetadataName("System.Linq.Enumerable"), "ToHashSet", "ToArray"),
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
                IfStatementSyntax branch => branch.Condition != held,
                CommonForEachStatementSyntax each => each.Expression != held,
                SwitchExpressionArmSyntax or AnonymousFunctionExpressionSyntax or QueryExpressionSyntax or ForStatementSyntax or WhileStatementSyntax or DoStatementSyntax
                    or SwitchSectionSyntax or CatchClauseSyntax or LocalFunctionStatementSyntax => true,
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
        static bool Once(IOperation read) =>
            !Inside<IAnonymousFunctionOperation>(read) && Ancestor<ISymbolInitializerOperation>(read, static _ => true) switch {
                IFieldInitializerOperation initializer => initializer.InitializedFields.All(static field => field.IsStatic),
                IPropertyInitializerOperation initializer => initializer.InitializedProperties.All(static property => property.IsStatic),
                _ => false,
            };
        static bool Free(SyntaxNode moved, ImmutableHashSet<string> bound, ImmutableHashSet<string> local) =>
            !Referenced(moved).Any(used => !bound.Contains(used.Identifier.ValueText) && local.Contains(used.Identifier.ValueText));
        Func<ISymbol, bool> hidden = Hidden(context.Compilation);
        INamedTypeSymbol? captioned = types.GetOrCreateTypeByMetadataName("System.Runtime.CompilerServices.CallerArgumentExpressionAttribute");
        bool Folding(ISymbol symbol) =>
            symbol is { IsImplicitlyDeclared: false, IsVirtual: false, IsAbstract: false, IsOverride: false, ContainingType: { IsExtension: false, IsAnonymousType: false, TypeKind: not TypeKind.Enum } owner } && symbol.GetAttributes().IsEmpty
            && hidden(symbol)
            && !owner.AllInterfaces.SelectMany(static face => face.GetMembers()).Any(member => Same(owner.FindImplementationForInterfaceMember(member), symbol))
            && symbol switch {
                IFieldSymbol field => field.IsConst || field is { IsStatic: true, IsReadOnly: true },
                IPropertySymbol property => property is { IsIndexer: false, IsPartialDefinition: false, PartialDefinitionPart: null, ExplicitInterfaceImplementations.IsEmpty: true }
                    && !owner.GetMembers().OfType<IFieldSymbol>().Any(field => Same(field.AssociatedSymbol, property)),
                IMethodSymbol method => method is { MethodKind: MethodKind.Ordinary, IsAsync: false, IsPartialDefinition: false, PartialDefinitionPart: null, ExplicitInterfaceImplementations.IsEmpty: true }
                    && method.Parameters.All(static parameter => parameter.RefKind is RefKind.None && parameter.GetAttributes().IsEmpty)
                    && !Same(method, context.Compilation.GetEntryPoint(context.CancellationToken)),
                _ => false,
            };
        ConcurrentDictionary<ISymbol, IOperation> values = new(SymbolEqualityComparer.Default);
        ConcurrentDictionary<ISymbol, (IOperation Read, ISymbol Reader, int Count)> reads = new(SymbolEqualityComparer.Default);
        ConcurrentDictionary<ISymbol, (IOperation Read, ISymbol Reader, int Count)> assembled = new(SymbolEqualityComparer.Default);
        ConcurrentDictionary<ISymbol, byte> pinned = new(SymbolEqualityComparer.Default);
        ConcurrentDictionary<IPropertySymbol, IOperation> forwards = new(SymbolEqualityComparer.Default);
        ConcurrentBag<(IPropertyReferenceOperation Read, INamedTypeSymbol Reader)> uses = [];
        context.RegisterOperationBlockAction(block => {
            ISymbol owner = Member(block.OwningSymbol);
            if (block.IsGeneratedCode || block.OperationBlocks is not { Length: 1 } blocks) return;
            if (owner.DeclaredAccessibility is not Accessibility.Private && block.OwningSymbol is not IMethodSymbol { MethodKind: MethodKind.PropertySet }) values[owner] = blocks[0];
            if (Expressed(blocks[0]) is not { } value) return;
            if (block.OwningSymbol is IMethodSymbol { MethodKind: MethodKind.PropertyGet } && owner is IPropertySymbol property && Forwarding(property, value, cells, block.CancellationToken)) forwards[property] = value;
            else if (Candidate(owner) && owner.DeclaringSyntaxReferences is { Length: 1 } declared && Shaped(declared[0].GetSyntax(block.CancellationToken))) values[owner] = value;
        });
        context.RegisterOperationAction(
            analysis => {
                if (analysis.Operation is INameOfOperation name) {
                    SymbolInfo info = analysis.Operation.SemanticModel!.GetSymbolInfo(name.Argument.Syntax, analysis.CancellationToken);
                    foreach (ISymbol symbol in info.Symbol is { } found ? [found] : info.CandidateSymbols)
                        _ = pinned.TryAdd(symbol.OriginalDefinition, 0);
                    return;
                }
                if (Target(analysis.Operation) is not { } referenced) return;
                ISymbol target = (referenced is IMethodSymbol { ReducedFrom: { } reduced } ? reduced : referenced).OriginalDefinition;
                if (analysis.IsGeneratedCode) {
                    _ = pinned.TryAdd(Member(target), 0);
                    return;
                }
                ISymbol reader = Member(Enclosing(analysis.ContainingSymbol));
                if (analysis.Operation is IPropertyReferenceOperation read && hidden(read.Property) && analysis.ContainingSymbol.ContainingType is { } containing) uses.Add((read, containing));
                if (Candidate(target)) _ = reads.AddOrUpdate(target, (analysis.Operation, reader, 1), static (_, held) => (held.Read, held.Reader, held.Count + 1));
                else if (target.DeclaredAccessibility is not Accessibility.Private && Sourced(target)) _ = assembled.AddOrUpdate(Member(target), (analysis.Operation, reader, 1), static (_, held) => (held.Read, held.Reader, held.Count + 1));
            },
            OperationKind.FieldReference,
            OperationKind.PropertyReference,
            OperationKind.Invocation,
            OperationKind.MethodReference,
            OperationKind.NameOf);
        models.Complete((end, available) => {
            ImmutableHashSet<ISymbol> candidates = ImmutableHashSet.CreateRange(SymbolEqualityComparer.Default,
                values.Keys.Where(member => reads.TryGetValue(member, out (IOperation Read, ISymbol Reader, int Count) read) && read.Count is 1));
            bool Passed(IOperation value, IOperation? instance, ImmutableArray<IArgumentOperation> arguments) {
                Reading reading = new(value.SemanticModel!, available, end.CancellationToken);
                ILookup<ISymbol?, IOperation> used = value.DescendantsAndSelf()
                    .Where(static held => held is IParameterReferenceOperation or IInstanceReferenceOperation { ReferenceKind: InstanceReferenceKind.ContainingTypeInstance })
                    .ToLookup(Variable, SymbolEqualityComparer.Default);
                bool Kept(IOperation passed, ISymbol? parameter) =>
                    !used[parameter].Any(Written) && (passed.ConstantValue.HasValue
                        || (passed.Syntax is IdentifierNameSyntax && Variable(passed) is { } held and not IParameterSymbol { RefKind: not RefKind.None } && !References(held, passed, reading).Any(Written))
                        || (used[parameter].Take(2).ToImmutableArray() is { Length: 1 } uses && !Deferred(uses[0].Syntax, value.Syntax) && Pure(passed) && Pure(value)));
                return (instance is null or IInstanceReferenceOperation || Kept(instance, parameter: null))
                    && ((instance is null or IInstanceReferenceOperation && arguments.All(static argument => argument.Value.Syntax is IdentifierNameSyntax name && string.Equals(name.Identifier.ValueText, argument.Parameter?.Name, StringComparison.Ordinal)))
                        || !(from call in value.DescendantsAndSelf().OfType<IInvocationOperation>()
                             from supplied in call.Arguments
                             where supplied.ArgumentKind is ArgumentKind.DefaultValue
                             from mark in supplied.Parameter?.GetAttributes() ?? []
                             where Same(mark.AttributeClass, captioned) && mark.ConstructorArguments.Length is 1
                             from shown in call.Arguments
                             where string.Equals(shown.Parameter?.Name, mark.ConstructorArguments[0].Value as string, StringComparison.Ordinal)
                             from held in shown.Value.DescendantsAndSelf()
                             where held is IInstanceReferenceOperation { IsImplicit: true } or IParameterReferenceOperation { Parameter.ContainingSymbol: IMethodSymbol { MethodKind: not (MethodKind.AnonymousFunction or MethodKind.LocalFunction) } }
                             select held).Any())
                    && arguments.All(argument => argument is { ArgumentKind: ArgumentKind.Explicit, Parameter: { } parameter, Value: var passed } && passed is not IConversionOperation { IsImplicit: true } && Kept(passed, parameter));
            }
            bool Substituted(IOperation value, int nesting, ImmutableHashSet<string> local, ImmutableArray<IParameterSymbol> parameters, ImmutableArray<IArgumentOperation> arguments) {
                SyntaxNode body = value.Syntax;
                ImmutableHashSet<string> names = [with(StringComparer.Ordinal), .. parameters.Select(static parameter => parameter.Name)];
                ImmutableHashSet<string> passed = [with(StringComparer.Ordinal), .. arguments.SelectMany(static argument => Referenced(argument.Syntax)).Select(static name => name.Identifier.ValueText)];
                return value is not IConversionOperation { IsImplicit: true } && !Targeted(value) && Free(body, names, local)
                    && body.SyntaxTree.GetLineSpan(body.Span, end.CancellationToken) is var lines && lines.StartLinePosition.Line == lines.EndLinePosition.Line
                    && !value.DescendantsAndSelf().OfType<INameOfOperation>().Any()
                    && !Bound(body).Any(name => names.Contains(name) || local.Contains(name) || passed.Contains(name))
                    && nesting + Depth(body) <= 3
                    && Passed(value, instance: null, arguments);
            }
            bool Inlined(ISymbol member, IOperation value, IOperation read, ISymbol reader) {
                if (read.Syntax.SyntaxTree != value.Syntax.SyntaxTree || !Same(reader.ContainingType, member.ContainingType) || Inside<IAttributeOperation>(read) || Inside<IParameterInitializerOperation>(read)
                    || Inside<INameOfOperation>(read) || (Unsafe(value.Syntax) && !Unsafe(read.Syntax)) || read.Syntax.FirstAncestorOrSelf<MemberDeclarationSyntax>() is not { } site
                    || (read switch { IMemberReferenceOperation reference => reference.Instance, IInvocationOperation call => call.Instance, _ => null }) is not (null or IInstanceReferenceOperation { ReferenceKind: InstanceReferenceKind.ContainingTypeInstance }))
                    return false;
                ImmutableHashSet<string> local = [with(StringComparer.Ordinal), .. Bound(site), "value"];
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
            ISymbol? Reader(ISymbol member) =>
                (candidates.Contains(member) ? reads : assembled).TryGetValue(member, out (IOperation Read, ISymbol Reader, int Count) read) && read.Count is 1 ? read.Reader : null;
            foreach (ISymbol member in candidates.Concat(assembled.Keys)) {
                end.CancellationToken.ThrowIfCancellationRequested();
                List<ISymbol> path = [];
                Dictionary<ISymbol, int> positions = [with(SymbolEqualityComparer.Default)];
                for (ISymbol? next = member; next is not null && !processed.Contains(next); next = Reader(next)) {
                    if (positions.TryGetValue(next, out int cycle)) {
                        cyclic.UnionWith(path.Skip(cycle));
                        break;
                    }
                    positions.Add(next, path.Count);
                    path.Add(next);
                }
                processed.UnionWith(path);
            }
            HashSet<ISymbol> reported = [with(SymbolEqualityComparer.Default)];
            foreach (KeyValuePair<ISymbol, (IOperation Read, ISymbol Reader, int Count)> pair in reads) {
                end.CancellationToken.ThrowIfCancellationRequested();
                if (pair.Value.Count is 1 && !pinned.ContainsKey(pair.Key) && !cyclic.Contains(pair.Key) && values.TryGetValue(pair.Key, out IOperation? value) && Inlined(pair.Key, value, pair.Value.Read, pair.Value.Reader)) {
                    _ = reported.Add(pair.Key);
                    end.ReportDiagnostic(Diagnostic.Create(Rule, pair.Key.Locations[0], [pair.Value.Read.Syntax.GetLocation()], pair.Key.Name, Named(pair.Value.Reader)));
                }
            }
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
            IEqualityComparer<IPropertySymbol> properties = SymbolEqualityComparer.Default;
            ILookup<IPropertySymbol, (IPropertyReferenceOperation Read, INamedTypeSymbol Reader)> observed = uses.ToLookup(static held => held.Read.Property.OriginalDefinition, properties);
            bool Readable(IPropertySymbol property, IOperation value) {
                ImmutableArray<IMemberReferenceOperation> members = [.. value.DescendantsAndSelf().OfType<IMemberReferenceOperation>()];
                ImmutableArray<IParameterReferenceOperation> parameters = [.. value.DescendantsAndSelf().OfType<IParameterReferenceOperation>()
                    .Where(static read => read.Parameter.ContainingSymbol is IMethodSymbol { MethodKind: MethodKind.Constructor })];
                return (property.DeclaredAccessibility is not Accessibility.Private && parameters.IsEmpty && members.All(member => Public(member.Member, property.ContainingType)))
                    || (hidden(property) && !pinned.ContainsKey(property.OriginalDefinition) && observed[property.OriginalDefinition].ToImmutableArray() is { IsEmpty: false } held && held.All(use => {
                        end.CancellationToken.ThrowIfCancellationRequested();
                        bool Bound(ISymbol symbol, SyntaxNode syntax) =>
                            syntax is ExpressionSyntax expression && use.Read.SemanticModel!.GetSpeculativeSymbolInfo(use.Read.Syntax.SpanStart, expression, SpeculativeBindingOption.BindAsExpression) is var binding
                            && (SymbolEqualityComparer.Default.Equals(binding.Symbol, symbol)
                                || (symbol is IMethodSymbol && binding.CandidateReason is CandidateReason.MemberGroup && binding.CandidateSymbols.Contains(symbol, SymbolEqualityComparer.Default)));
                        bool own = SymbolEqualityComparer.Default.Equals(use.Reader, property.ContainingType)
                            && use.Read.Instance is null or IInstanceReferenceOperation { ReferenceKind: InstanceReferenceKind.ContainingTypeInstance };
                        return (own ? parameters.All(parameter => Bound(parameter.Parameter, parameter.Syntax)) : parameters.IsEmpty) && members.All(member =>
                            end.Compilation.IsSymbolAccessibleWithin(member.Member, use.Reader, member.Instance?.Type)
                            && (member is not IPropertyReferenceOperation getter || (getter.Property.GetMethod is { } method
                                && end.Compilation.IsSymbolAccessibleWithin(method, use.Reader, getter.Instance?.Type)))
                            && (!own || member.Instance is not (null or IInstanceReferenceOperation) || Bound(member.Member, member.Syntax)));
                    }));
            }
            foreach (KeyValuePair<IPropertySymbol, IOperation> pair in forwards) {
                end.CancellationToken.ThrowIfCancellationRequested();
                if (!Readable(pair.Key, pair.Value)) continue;
                _ = reported.Add(pair.Key);
                end.ReportDiagnostic(Diagnostic.Create(Forwarded, pair.Key.Locations[0], pair.Key.Name, pair.Value.Syntax));
            }
            foreach (KeyValuePair<ISymbol, (IOperation Read, ISymbol Reader, int Count)> pair in assembled) {
                end.CancellationToken.ThrowIfCancellationRequested();
                (ISymbol member, IOperation site, ISymbol reader) = (pair.Key, pair.Value.Read, pair.Value.Reader);
                (IOperation? instance, ImmutableArray<IArgumentOperation> arguments) = site is IInvocationOperation call ? (call.Instance, call.Arguments) : ((site as IMemberReferenceOperation)?.Instance, []);
                if (pair.Value.Count is 1 && Folding(member) && !pinned.ContainsKey(member) && !reported.Contains(member) && !cyclic.Contains(member) && reader.ContainingType is { } within
                    && member.DeclaringSyntaxReferences is { Length: 1 } declared && available.TryGetValue(declared[0].SyntaxTree, out SemanticModel? model)
                    && declared[0].GetSyntax(end.CancellationToken) is var body && Nesting(site.Syntax) + (site is IMethodReferenceOperation ? 1 : 0) + Depth(body) <= 3
                    && (member is not IFieldSymbol { IsConst: false } || Once(site)
                        || (body is VariableDeclaratorSyntax { Initializer.Value: var stated } && model.GetConstantValue(stated, end.CancellationToken).HasValue))
                    && ((arguments.IsEmpty && instance is null or IInstanceReferenceOperation) || (values.TryGetValue(member, out IOperation? value) && Passed(value, instance, arguments)))
                    && body.DescendantNodes().All(node => model.GetSymbolInfo(node, end.CancellationToken).Symbol is not { } used
                        || ((used is not IParameterSymbol { ContainingSymbol: IMethodSymbol { MethodKind: MethodKind.Constructor } } || Same(within, member.ContainingType))
                            && (used.ContainingSymbol is not INamedTypeSymbol || end.Compilation.IsSymbolAccessibleWithin(used, within)))))
                    end.ReportDiagnostic(Diagnostic.Create(Folded, member.Locations[0], [site.Syntax.GetLocation()], $"{member.ContainingType.Name}.{member.Name}", $"{within.Name}.{Named(reader)}"));
            }
        });
    }

    // --- [SCOPES]

    private static IEnumerable<string> Bound(SyntaxNode node) =>
        node.DescendantNodesAndSelf().Select(static declared => declared switch {
            ParameterSyntax parameter => parameter.Identifier,
            VariableDeclaratorSyntax variable => variable.Identifier,
            SingleVariableDesignationSyntax designation => designation.Identifier,
            LocalFunctionStatementSyntax local => local.Identifier,
            ForEachStatementSyntax each => each.Identifier,
            CatchDeclarationSyntax caught => caught.Identifier,
            FromClauseSyntax range => range.Identifier,
            LetClauseSyntax range => range.Identifier,
            JoinClauseSyntax range => range.Identifier,
            JoinIntoClauseSyntax range => range.Identifier,
            QueryContinuationSyntax range => range.Identifier,
            TypeParameterSyntax parameter => parameter.Identifier,
            _ => default,
        }).Where(static token => !token.IsKind(SyntaxKind.None)).Select(static token => token.ValueText);

    private static IEnumerable<IdentifierNameSyntax> Referenced(SyntaxNode node) {
        ImmutableHashSet<string> own = [with(StringComparer.Ordinal), .. Bound(node)];
        return node.DescendantNodesAndSelf().OfType<IdentifierNameSyntax>().Where(used => !own.Contains(used.Identifier.ValueText)
            && used.Parent switch { MemberAccessExpressionSyntax access => access.Name != used, MemberBindingExpressionSyntax or NameColonSyntax or NameEqualsSyntax => false, _ => true });
    }

    // --- [FORWARDS]

    private static ImmutableArray<INamedTypeSymbol> Cells(Compilation compilation) => [
        .. Declared(compilation, "T:System.Lazy`1", "T:System.Threading.ThreadLocal`1", "T:System.Threading.AsyncLocal`1", "T:System.Runtime.CompilerServices.StrongBox`1",
            "T:System.Threading.Channels.Channel`1", "T:System.Threading.Channels.Channel`2").OfType<INamedTypeSymbol>(),
        .. WellKnownTypeProvider.GetOrCreate(compilation).GetOrCreateTypeByMetadataName("LanguageExt.Atom`1")?.ContainingNamespace.GetTypeMembers()
            .Where(static type => type.Name is "Atom" or "AtomHashMap" or "AtomSeq" or "AtomQue" or "Ref" or "VersionHashMap") ?? [],
    ];

    private static bool Forwarding(IPropertySymbol property, IOperation returned, ImmutableArray<INamedTypeSymbol> cells, CancellationToken token) {
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
                && element.Indices.Select(Parameter).All(static same => same),
            IMemberReferenceOperation member when member is IFieldReferenceOperation or IPropertyReferenceOperation or IMethodReferenceOperation => member.Instance is { } instance
                ? Rooted(instance) : member.Member.IsStatic && !Same(member.Member.ContainingType, property.ContainingType),
            _ => false,
        };
    }
}

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class RestatedInstanceAnalyzer : DiagnosticAnalyzer {

    // --- [RULES]

    private static readonly DiagnosticDescriptor Rule = new(
        "RASM3011",
        "Static instance restated at a construction site, read the instance",
        "`{0}` restates `{1}`, read `{1}`",
        Categories.CSharp,
        DiagnosticSeverity.Error,
        isEnabledByDefault: true,
        "Read same-type static members over immutable record or readonly value constructions and transparent factories restating their constant initializers, outside smart enum item initializers.",
        Link("RASM3011"),
        WellKnownDiagnosticTags.CompilationEnd);

    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => [Rule];

    public override void Initialize(AnalysisContext context) {
        context.EnableConcurrentExecution();
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
        context.RegisterCompilationStartAction(Start);
    }

    private sealed class ShapeComparer : EqualityComparer<ImmutableArray<(ISymbol Symbol, object? Constant)>> {
        public override bool Equals(ImmutableArray<(ISymbol Symbol, object? Constant)> x, ImmutableArray<(ISymbol Symbol, object? Constant)> y) =>
            x.Length == y.Length && x.Zip(y, static (one, other) => SymbolEqualityComparer.Default.Equals(one.Symbol, other.Symbol) && Equals(one.Constant, other.Constant)).All(static same => same);

        public override int GetHashCode(ImmutableArray<(ISymbol Symbol, object? Constant)> obj) =>
            obj.Aggregate(0, static (hash, part) => (hash, SymbolEqualityComparer.Default.GetHashCode(part.Symbol), part.Constant).GetHashCode());
    }

    private static void Start(CompilationStartAnalysisContext context) {
        SemanticModels models = new(context);
        ConcurrentDictionary<ISymbol, IOperation> initializers = new(SymbolEqualityComparer.Default);
        ConcurrentDictionary<IMethodSymbol, IOperation> factories = new(SymbolEqualityComparer.Default);
        ConcurrentDictionary<INamedTypeSymbol, ConcurrentBag<IOperation>> instanceInitializers = new(SymbolEqualityComparer.Default);
        ConcurrentBag<(IOperation Site, INamedTypeSymbol Type, ISymbol Owner)> sites = [];
        WellKnownTypeProvider types = WellKnownTypeProvider.GetOrCreate(context.Compilation);
        ImmutableArray<INamedTypeSymbol> smartEnums = [.. new[] { types.GetOrCreateTypeByMetadataName("Thinktecture.SmartEnumAttribute"), types.GetOrCreateTypeByMetadataName("Thinktecture.SmartEnumAttribute`1") }.OfType<INamedTypeSymbol>()];
        static bool ValueBased(INamedTypeSymbol type) => type.IsRecord || (type.IsValueType && type.IsReadOnly);
        bool Item(ISymbol owner, INamedTypeSymbol type) =>
            Same(owner.ContainingType, type) && initializers.ContainsKey(owner) && Marked(type, smartEnums) is not null;
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
                        if (Made(operation) is { } type && Sourced(type) && type.TypeArguments.IsEmpty) sites.Add((operation, type, Member(analysis.ContainingSymbol)));
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
                    ImmutableArray<IOperation> bodies = [.. declarations.Where(declaration => available.ContainsKey(declaration.SyntaxTree))
                        .Select(declaration => available[declaration.SyntaxTree].GetOperation(declaration, end.CancellationToken)).OfType<IOperation>()];
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
                    end.ReportDiagnostic(Diagnostic.Create(Rule, site.Syntax.GetLocation(), site.Syntax, $"{type.Name}.{restated.Name}"));
        });
    }
}

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class ValueFactoryOverloadAnalyzer : DiagnosticAnalyzer {

    // --- [RULES]

    private static readonly DiagnosticDescriptor Rule = new(
        "RASM3013",
        "Overloads take a value and its factory, keep one overload",
        "`{0}` takes `{1}` as a value and `{2}` as a factory, which multiplies binds at calls passing a lambda in that position, keep one overload",
        Categories.CSharp,
        DiagnosticSeverity.Error,
        isEnabledByDefault: true,
        "Overload sets take a method type parameter at one position as a value or as a parameterless delegate returning it, never both when each overload holds a delegate over that type parameter.",
        Link("RASM3013"));

    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => [Rule];

    public override void Initialize(AnalysisContext context) {
        context.EnableConcurrentExecution();
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
        context.RegisterSymbolAction(static analysis => {
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
                        analysis.ReportDiagnostic(Diagnostic.Create(Rule, value.Locations[0], [factory.Locations[0]], Label(value), held.Name, made.Name));
                    }
        }, SymbolKind.NamedType);
    }

    private static bool Delegates(IParameterSymbol parameter, ITypeParameterSymbol held) =>
        parameter.Type is INamedTypeSymbol { DelegateInvokeMethod: { } invoke }
        && (Mentions(invoke.ReturnType, held) || invoke.Parameters.Any(argument => Mentions(argument.Type, held)));

    private static IEnumerable<(IParameterSymbol Held, IParameterSymbol Made, IMethodSymbol Factory)> Pairs(IMethodSymbol value, ImmutableArray<IMethodSymbol> set) =>
        from held in value.Parameters
        where held.Type is ITypeParameterSymbol { TypeParameterKind: TypeParameterKind.Method } type
            && value.Parameters.Any(other => !SymbolEqualityComparer.Default.Equals(other, held) && Delegates(other, type))
        from factory in set
        where held.Ordinal < factory.Parameters.Length
            && factory.Parameters[held.Ordinal].Type is INamedTypeSymbol { DelegateInvokeMethod: { Parameters.IsEmpty: true, ReturnType: ITypeParameterSymbol { TypeParameterKind: TypeParameterKind.Method } made } }
            && made.Ordinal == ((ITypeParameterSymbol)held.Type).Ordinal
            && factory.Parameters.Any(other => other.Ordinal != held.Ordinal && Delegates(other, made))
        select (held, factory.Parameters[held.Ordinal], factory);
}
