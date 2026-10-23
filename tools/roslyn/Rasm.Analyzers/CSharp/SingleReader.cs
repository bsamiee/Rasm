using System.Collections.Concurrent;
using System.Collections.Immutable;
using System.Text.RegularExpressions;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Operations;

namespace Rasm.Analyzers.CSharp;

// --- [OPERATIONS] ----------------------------------------------------------------------
public static class Scopes {
    public static IEnumerable<string> Declared(SyntaxNode node) =>
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

    public static IEnumerable<IdentifierNameSyntax> Referenced(SyntaxNode node) {
        ImmutableHashSet<string> own = [.. Declared(node)];
        return node.DescendantNodesAndSelf().OfType<IdentifierNameSyntax>().Where(used => !own.Contains(used.Identifier.ValueText) && used.Identifier.ValueText is not "_"
            && used.Parent switch { MemberAccessExpressionSyntax access => access.Name != used, MemberBindingExpressionSyntax or NameColonSyntax or NameEqualsSyntax => false, _ => true });
    }
}

// --- [COMPOSITION] ---------------------------------------------------------------------
[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class SingleReaderAnalyzer : RasmAnalyzer {
    private static readonly DiagnosticDescriptor Reader = Rule(
        "RASM3001",
        "CSharp",
        "Private member read once",
        "`{0}` has one read, in `{1}`. Inline `{0}` at that read",
        "Private constant, `Contains` set, or one-line expression-bodied member with one read in its own type that inlines without losing a word of its name.");

    private static readonly Regex Word = new("[A-Z]?[a-z0-9]+|[A-Z]+(?![a-z])", RegexOptions.CultureInvariant);

    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => [Reader];

    protected override void Start(CompilationStartAnalysisContext context) {
        ImmutableArray<INamedTypeSymbol?> sets = [context.Compilation.GetTypeByMetadataName("System.Collections.Generic.ISet`1"), context.Compilation.GetTypeByMetadataName("System.Collections.Generic.IReadOnlySet`1")];
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
        static ImmutableArray<IOperation> Elements(IOperation value) =>
            value switch {
                IConversionOperation { IsImplicit: true } conversion => Elements(conversion.Operand),
                ICollectionExpressionOperation collection => collection.Elements,
                IArrayCreationOperation { Initializer: { } initializer } => initializer.ElementValues,
                IObjectCreationOperation { Arguments.IsEmpty: true, Initializer: { } initializer } when initializer.Initializers.All(static added => added is IInvocationOperation { Arguments.Length: 1 }) =>
                    [.. initializer.Initializers.Select(static added => ((IInvocationOperation)added).Arguments[0].Value)],
                IInvocationOperation { Instance: null, Arguments: { IsEmpty: false } arguments, TargetMethod: { IsExtensionMethod: true, ContainingNamespace: var space } }
                    when arguments[0].Value is { Syntax.Parent: MemberAccessExpressionSyntax } source && arguments.Skip(1).All(static argument => argument.ArgumentKind is ArgumentKind.DefaultValue)
                    && space.ToDisplayString() is "System.Linq" or "System.Collections.Frozen" or "System.Collections.Immutable" => Elements(source),
                _ => [],
            };
        bool Listed(IFieldSymbol field, IOperation value) =>
            !field.IsConst && Elements(value) is { IsEmpty: false } elements && elements.All(static element => element.ConstantValue.HasValue)
            && (field.Type is IArrayTypeSymbol || field.Type.AllInterfaces.Append(field.Type).Any(face => sets.Any(wanted => Same(face, wanted))));
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
        static IOperation Root(IOperation operation) => operation.Parent is { } parent ? Root(parent) : operation;
        static bool Written(IOperation operation) =>
            operation.Parent switch {
                IAssignmentOperation assignment => assignment.Target == operation,
                IIncrementOrDecrementOperation => true,
                IArgumentOperation { Parameter.RefKind: RefKind.Ref or RefKind.Out } => true,
                _ => false,
            };
        static bool Plain(IOperation value) =>
            value.Syntax is IdentifierNameSyntax && value switch {
                ILocalReferenceOperation local => !Root(value).Descendants().Any(write => write is ILocalReferenceOperation other && Same(other.Local, local.Local) && Written(other)),
                IParameterReferenceOperation { Parameter.RefKind: RefKind.None } parameter =>
                    !Root(value).Descendants().Any(write => write is IParameterReferenceOperation other && Same(other.Parameter, parameter.Parameter) && Written(other)),
                _ => false,
            };
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
        static int Depth(SyntaxNode node) => node.ChildNodes().Select(Depth).DefaultIfEmpty().Max() + Level(node);
        static int Nesting(SyntaxNode node) => node.Ancestors().TakeWhile(static held => held is not (MemberDeclarationSyntax or AccessorDeclarationSyntax)).Sum(Level);
        static bool Unsafe(SyntaxNode node) =>
            node.AncestorsAndSelf().Any(static held => held is UnsafeStatementSyntax || (held is MemberDeclarationSyntax member && member.Modifiers.Any(SyntaxKind.UnsafeKeyword)));
        static IOperation? Initializer(IOperation operation) => operation.Parent switch { null => null, ISymbolInitializerOperation initializer => initializer, var parent => Initializer(parent) };
        static bool Grouped(IOperation reference) =>
            Initializer(reference) is { } initializer
            && initializer.Descendants().OfType<IMethodReferenceOperation>().Select(static group => group.Method.OriginalDefinition).Distinct(SymbolEqualityComparer.Default).Skip(1).Any();
        static ISymbol Member(ISymbol symbol) => symbol is IMethodSymbol { MethodKind: MethodKind.AnonymousFunction or MethodKind.LocalFunction } nested ? Member(nested.ContainingSymbol) : symbol;
        static string Named(ISymbol symbol) =>
            symbol switch {
                IMethodSymbol { AssociatedSymbol: { } owner } => owner.Name,
                IMethodSymbol { MethodKind: MethodKind.Constructor or MethodKind.StaticConstructor } constructor => constructor.ContainingType.Name,
                _ => symbol.Name,
            };
        static bool Free(SyntaxNode moved, ImmutableHashSet<string> bound, ImmutableHashSet<string> local) =>
            !Scopes.Referenced(moved).Any(used => !bound.Contains(used.Identifier.ValueText) && local.Contains(used.Identifier.ValueText));
        static bool Bound(IArgumentOperation argument, ImmutableArray<IdentifierNameSyntax> used, SyntaxNode body) =>
            argument is { ArgumentKind: ArgumentKind.Explicit, Parameter: { } parameter, Value: var passed } && passed is not IConversionOperation { IsImplicit: true }
            && (Plain(passed) || (Pure(passed) && used.Where(name => string.Equals(name.Identifier.ValueText, parameter.Name, StringComparison.Ordinal)).ToImmutableArray() switch {
                { IsEmpty: true } => true,
                { Length: 1 } uses => !Deferred(uses[0], body),
                _ => false,
            }));
        static bool Substituted(IOperation value, int nesting, ImmutableHashSet<string> local, ImmutableArray<IParameterSymbol> parameters, ImmutableArray<IArgumentOperation> arguments) {
            SyntaxNode body = value.Syntax;
            ImmutableHashSet<string> names = [.. parameters.Select(static parameter => parameter.Name)];
            ImmutableArray<IdentifierNameSyntax> used = [.. Scopes.Referenced(body)];
            ImmutableHashSet<string> passed = [.. arguments.SelectMany(static argument => Scopes.Referenced(argument.Syntax)).Select(static name => name.Identifier.ValueText)];
            return value is not IConversionOperation { IsImplicit: true } && !Targeted(value) && Free(body, names, local)
                && body.SyntaxTree.GetLineSpan(body.Span) is var lines && lines.StartLinePosition.Line == lines.EndLinePosition.Line
                && !body.DescendantNodesAndSelf().OfType<InvocationExpressionSyntax>().Any(static call => call.Expression is IdentifierNameSyntax { Identifier.ValueText: "nameof" })
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
                    ISymbol owner = block.OwningSymbol is IMethodSymbol { AssociatedSymbol: IPropertySymbol property } ? property : block.OwningSymbol;
                    if (candidates.Contains(owner) && block.OperationBlocks is { Length: 1 } blocks && Expressed(blocks[0]) is { } value) values[owner] = value;
                });
                start.RegisterOperationAction(
                    operation => {
                        ISymbol? target = operation.Operation switch {
                            IMemberReferenceOperation reference => reference.Member.OriginalDefinition,
                            IInvocationOperation call => call.TargetMethod.OriginalDefinition,
                            _ => null,
                        };
                        if (target is not null && candidates.Contains(target)) reads[target] = (operation.Operation, Member(operation.ContainingSymbol));
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
}
