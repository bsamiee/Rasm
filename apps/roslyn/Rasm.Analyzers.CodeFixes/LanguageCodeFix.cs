using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Editing;
using Microsoft.CodeAnalysis.Formatting;
using Microsoft.CodeAnalysis.Operations;
using Microsoft.CodeAnalysis.Simplification;
using QuikGraph;
using QuikGraph.Algorithms.TopologicalSort;

namespace Rasm.Analyzers.CodeFixes;

// --- [COMPOSITION] ---------------------------------------------------------------------

[ExportCodeFixProvider(LanguageNames.CSharp)]
public sealed class LanguageCodeFix() : DocumentEditorCodeFix((
    LanguageAnalyzer.Covered,
    static (site, _) => FormattableString.Invariant($"Delete `{site.FirstAncestorOrSelf<SwitchExpressionArmSyntax>()!}`"),
    static (editor, site, _, _) => editor.RemoveNode(site.FirstAncestorOrSelf<SwitchExpressionArmSyntax>()!, SyntaxRemoveOptions.KeepExteriorTrivia | SyntaxRemoveOptions.KeepUnbalancedDirectives | SyntaxRemoveOptions.AddElasticMarker)));

[ExportCodeFixProvider(LanguageNames.CSharp)]
public sealed class SingleReaderCodeFix() : RasmCodeFix((LanguageAnalyzer.Reader, static (site, generator) => $"Inline `{generator.GetName(site)}`")) {
    protected override async Task<Document> ChangedAsync(Document document, ImmutableArray<Diagnostic> diagnostics, CancellationToken cancellation) {
        SyntaxNode root = (await document.GetSyntaxRootAsync(cancellation).ConfigureAwait(false))!;
        SemanticModel model = (await document.GetSemanticModelAsync(cancellation).ConfigureAwait(false))!;
        ImmutableArray<(SyntaxNode Declaration, SyntaxNode Site)> moves = [
            .. diagnostics.OrderBy(static diagnostic => diagnostic.Location.SourceSpan.Start).Select(diagnostic => (
                root.FindToken(diagnostic.Location.SourceSpan.Start).Parent!,
                root.FindNode(diagnostic.AdditionalLocations[0].SourceSpan, getInnermostNodeForTie: true))),
        ];
        ImmutableDictionary<SyntaxNode, ImmutableDictionary<SyntaxNode, ExpressionSyntax>> bindings = moves.ToImmutableDictionary(
            static move => move.Declaration,
            move => {
                cancellation.ThrowIfCancellationRequested();
                if (move.Declaration is not MethodDeclarationSyntax || model.GetOperation(move.Site, cancellation) is not IInvocationOperation call) return [];
                ILookup<IParameterSymbol, SyntaxNode> uses = model.GetOperation(move.Declaration, cancellation)!.DescendantsAndSelf().OfType<IParameterReferenceOperation>()
                    .ToLookup<IParameterReferenceOperation, IParameterSymbol, SyntaxNode>(static reference => reference.Parameter, static reference => reference.Syntax, SymbolEqualityComparer.Default);
                return call.Arguments.SelectMany(argument => uses[argument.Parameter!].Select(use => (Use: use, Value: (ExpressionSyntax)argument.Value.Syntax)))
                    .ToImmutableDictionary(static binding => binding.Use, static binding => binding.Value);
            });
        ImmutableDictionary<SyntaxNode, SyntaxNode> declarations = moves.ToImmutableDictionary(static move => move.Declaration, static move => move.Site);
        AdjacencyGraph<SyntaxNode, SEdge<SyntaxNode>> dependencies = moves.Select(static move => move.Declaration).ToAdjacencyGraph<SyntaxNode, SEdge<SyntaxNode>>(
            declaration => {
                cancellation.ThrowIfCancellationRequested();
                return declarations[declaration].AncestorsAndSelf().FirstOrDefault(declarations.ContainsKey) is { } reader ? [new SEdge<SyntaxNode>(declaration, reader)] : [];
            },
            allowParallelEdges: false);
        SourceFirstTopologicalSortAlgorithm<SyntaxNode, SEdge<SyntaxNode>> order = new(dependencies, dependencies.VertexCount);
        order.VertexAdded += _ => cancellation.ThrowIfCancellationRequested();
        order.Compute();
        cancellation.ThrowIfCancellationRequested();
        SyntaxGenerator generator = SyntaxGenerator.GetGenerator(document);
        SyntaxNode inlined = order.SortedVertices.Aggregate(
            root.TrackNodes(moves.SelectMany(static move => new[] { move.Declaration, move.Site }).Concat(bindings.Values.SelectMany(static bound => bound.Keys.Concat(bound.Values)))),
            (current, declaration) => {
                cancellation.ThrowIfCancellationRequested();
                return Inlined(current, declaration, declarations[declaration], bindings[declaration], generator, model, cancellation);
            });
        return document.WithSyntaxRoot(inlined);
    }

    private static SyntaxNode Inlined(SyntaxNode root, SyntaxNode original, SyntaxNode read, ImmutableDictionary<SyntaxNode, ExpressionSyntax> bindings, SyntaxGenerator generator, SemanticModel model, CancellationToken cancellation) {
        SyntaxNode declaration = root.GetCurrentNode(original)!;
        SyntaxNode site = root.GetCurrentNode(read)!;
        static ExpressionSyntax Placed(ExpressionSyntax expression, SyntaxNode site) =>
            expression.WithLeadingTrivia(site.GetLeadingTrivia().Concat(expression.GetLeadingTrivia()))
                .WithTrailingTrivia(expression.GetTrailingTrivia().Concat(site.GetTrailingTrivia()))
                .WithAdditionalAnnotations(Formatter.Annotation);
        ExpressionSyntax Grouped(ExpressionSyntax expression) =>
            Placed(site.Parent is ExpressionStatementSyntax ? expression : Parenthesized(expression), site);
        static ImmutableArray<ExpressionSyntax> Elements(ExpressionSyntax value) =>
            value.DescendantNodesAndSelf(static node => node is InvocationExpressionSyntax or MemberAccessExpressionSyntax or ParenthesizedExpressionSyntax)
                .First(static node => node is not (InvocationExpressionSyntax or MemberAccessExpressionSyntax or ParenthesizedExpressionSyntax)) switch {
                    CollectionExpressionSyntax collection => [.. collection.Elements.OfType<ExpressionElementSyntax>().Select(static element => element.Expression)],
                    ArrayCreationExpressionSyntax { Initializer: { } initializer } => [.. initializer.Expressions],
                    ImplicitArrayCreationExpressionSyntax { Initializer: var initializer } => [.. initializer.Expressions],
                    BaseObjectCreationExpressionSyntax { Initializer: { } initializer } => [.. initializer.Expressions],
                    _ => [],
                };
        static ExpressionSyntax Tested(ExpressionSyntax operand, PatternSyntax pattern, SyntaxNode replaced) =>
            Parenthesized(SyntaxFactory.IsPatternExpression(Parenthesized(operand.WithoutTrivia()), Punctuation(SyntaxKind.IsKeyword), pattern))
                .WithTriviaFrom(replaced)
                .WithAdditionalAnnotations(Simplifier.Annotation, Formatter.Annotation);
        (SyntaxNode Replaced, SyntaxNode Replacement) Matched(SyntaxNode site, ImmutableArray<ExpressionSyntax> elements) {
            PatternSyntax pattern = elements.Select(static element => (PatternSyntax)SyntaxFactory.ConstantPattern(element.WithoutTrivia()))
                .Aggregate(static (left, right) => SyntaxFactory.BinaryPattern(SyntaxKind.OrPattern, left, Punctuation(SyntaxKind.OrKeyword), right));
            InvocationExpressionSyntax call = site.Ancestors().OfType<InvocationExpressionSyntax>().First();
            ExpressionSyntax operand = call.ArgumentList.Arguments.Select(static argument => argument.Expression).Single(expression => !expression.Span.Contains(site.Span));
            IInvocationOperation invoked = (IInvocationOperation)model.GetOperation(read.Ancestors().OfType<InvocationExpressionSyntax>().First(), cancellation)!;
            if (invoked.Arguments.Single(argument => !argument.Syntax.Span.Contains(read.Span)).Value is IConversionOperation { IsImplicit: true, Type: { } type })
                operand = SyntaxFactory.CastExpression(Punctuation(SyntaxKind.OpenParenToken), TypeExpression(generator, type), Punctuation(SyntaxKind.CloseParenToken), Parenthesized(operand));
            SyntaxNode grouped = call.AncestorsAndSelf().First(static node => node.Parent is not ParenthesizedExpressionSyntax);
            return grouped.Parent is PrefixUnaryExpressionSyntax negated && negated.IsKind(SyntaxKind.LogicalNotExpression)
                    ? (negated, Tested(operand, SyntaxFactory.UnaryPattern(Punctuation(SyntaxKind.NotKeyword), elements.Length is 1 ? pattern
                        : SyntaxFactory.ParenthesizedPattern(Punctuation(SyntaxKind.OpenParenToken), pattern, Punctuation(SyntaxKind.CloseParenToken))), negated))
                    : (call, Tested(operand, pattern, call));
        }
        ExpressionSyntax Substituted(ExpressionSyntax body) {
            ImmutableDictionary<SyntaxNode, ExpressionSyntax> bound = bindings.SelectMany(binding =>
                root.GetCurrentNodes(binding.Key).Select(use => (Use: use, Value: root.GetCurrentNode(binding.Value)!)))
                .ToImmutableDictionary(static binding => binding.Use, static binding => binding.Value);
            return body.ReplaceNodes(bound.Keys,
                (original, _) => original.CopyAnnotationsTo(Placed(Parenthesized(bound[original]), original)));
        }
        static ExpressionSyntax Lambda(MethodDeclarationSyntax method, ExpressionSyntax body) =>
            (method.Modifiers.Any(SyntaxKind.StaticKeyword) ? SyntaxFactory.TokenList(Punctuation(SyntaxKind.StaticKeyword)) : default) is var modifiers
            && method.ParameterList.Parameters is { Count: 1 } single
                ? SyntaxFactory.SimpleLambdaExpression(default, modifiers, SyntaxFactory.Parameter(single[0].Identifier.WithoutTrivia()), Punctuation(SyntaxKind.EqualsGreaterThanToken), block: null, body.WithoutTrivia())
                : SyntaxFactory.ParenthesizedLambdaExpression(default, modifiers, returnType: null,
                    SyntaxFactory.ParameterList(Punctuation(SyntaxKind.OpenParenToken), Separated(method.ParameterList.Parameters.Select(static parameter => SyntaxFactory.Parameter(parameter.Identifier.WithoutTrivia()))), Punctuation(SyntaxKind.CloseParenToken)),
                    Punctuation(SyntaxKind.EqualsGreaterThanToken), block: null, body.WithoutTrivia());
        ExpressionSyntax body = declaration switch {
            PropertyDeclarationSyntax { ExpressionBody: null, AccessorList.Accessors: var accessors } => accessors[0].ExpressionBody!.Expression,
            _ => (ExpressionSyntax)generator.GetExpression(declaration)!,
        };
        ImmutableArray<ExpressionSyntax> elements = declaration is VariableDeclaratorSyntax ? Elements(body) : [];
        (SyntaxNode replaced, SyntaxNode replacement) = !elements.IsEmpty ? Matched(site, elements)
            : (site, Grouped(declaration is MethodDeclarationSyntax method
                ? site is InvocationExpressionSyntax ? Substituted(body) : Lambda(method, body)
                : body));
        SyntaxNode placed = root.ReplaceNode(replaced, replaced.CopyAnnotationsTo(replacement));
        SyntaxNode removed = placed.GetCurrentNode(original)! switch {
            VariableDeclaratorSyntax { Parent: VariableDeclarationSyntax { Variables.Count: 1, Parent: FieldDeclarationSyntax field } } => field,
            var node => node,
        };
        SyntaxRemoveOptions options = (Relocated(removed.GetLeadingTrivia().Concat(removed.GetTrailingTrivia())).Count is 0 ? SyntaxRemoveOptions.KeepNoTrivia : SyntaxRemoveOptions.KeepExteriorTrivia)
            | SyntaxRemoveOptions.KeepUnbalancedDirectives;
        if (removed is MemberDeclarationSyntax member && member.Parent is TypeDeclarationSyntax { Members.Count: >= 2 } parent && parent.Members[0] == member) {
            MemberDeclarationSyntax next = parent.Members[1];
            TypeDeclarationSyntax prepared = parent.WithMembers(parent.Members.Replace(next, next.WithLeadingTrivia(Relocated(next.GetLeadingTrivia())).WithAdditionalAnnotations(Formatter.Annotation)));
            return placed.ReplaceNode(parent, prepared.RemoveNode(prepared.Members[0], options)!);
        }
        return placed.RemoveNode(removed, options)!;
    }
}
