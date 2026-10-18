using System.Collections.Immutable;
using System.Composition;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Formatting;
using Microsoft.CodeAnalysis.Simplification;
using Rasm.Analyzers.CSharp;

namespace Rasm.Analyzers.CodeFixes;

// --- [COMPOSITION] ---------------------------------------------------------------------
[ExportCodeFixProvider(LanguageNames.CSharp)]
[Shared]
public sealed class SingleReaderCodeFix : CodeFixProvider {
    public override ImmutableArray<string> FixableDiagnosticIds => [.. new SingleReaderAnalyzer().SupportedDiagnostics.Select(static descriptor => descriptor.Id)];

    public override FixAllProvider GetFixAllProvider() =>
        FixAllProvider.Create(static (context, document, diagnostics) => InlinedAsync(document, diagnostics, context.CancellationToken));

    public override Task RegisterCodeFixesAsync(CodeFixContext context) {
        foreach (Diagnostic diagnostic in context.Diagnostics)
            context.RegisterCodeFix(
                CodeAction.Create("Inline at its read", async token => await InlinedAsync(context.Document, [diagnostic], token).ConfigureAwait(false) ?? context.Document, nameof(SingleReaderCodeFix)),
                diagnostic);
        return Task.CompletedTask;
    }

    private static async Task<Document?> InlinedAsync(Document document, ImmutableArray<Diagnostic> diagnostics, CancellationToken cancellation) {
        if (await document.GetSyntaxRootAsync(cancellation).ConfigureAwait(false) is not { } root) return null;
        ImmutableArray<(SyntaxNode Declaration, SyntaxNode Site, SyntaxAnnotation Declared, SyntaxAnnotation Read)> moves = [
            .. diagnostics.Where(static diagnostic => diagnostic.AdditionalLocations.Count is 1).Select(diagnostic => (
                root.FindToken(diagnostic.Location.SourceSpan.Start).Parent ?? root,
                root.FindNode(diagnostic.AdditionalLocations[0].SourceSpan, getInnermostNodeForTie: true),
                new SyntaxAnnotation(),
                new SyntaxAnnotation())),
        ];
        ILookup<SyntaxNode, SyntaxAnnotation> marks = moves.SelectMany(static move => new[] { (Node: move.Declaration, Mark: move.Declared), (Node: move.Site, Mark: move.Read) })
            .ToLookup(static held => held.Node, static held => held.Mark);
        int Rank(SyntaxNode declaration, int depth) =>
            depth < moves.Length ? moves.Where(other => declaration.Span.Contains(other.Site.Span)).Select(other => Rank(other.Declaration, depth + 1) + 1).DefaultIfEmpty().Max() : 0;
        SyntaxNode inlined = moves.OrderBy(move => Rank(move.Declaration, 0)).ThenBy(static move => move.Site.SpanStart).Aggregate(
            root.ReplaceNodes(marks.Select(static group => group.Key), (original, rewritten) => rewritten.WithAdditionalAnnotations(marks[original])),
            static (current, move) => Inlined(current, move.Declared, move.Read));
        Document simplified = await Simplifier.ReduceAsync(document.WithSyntaxRoot(inlined), Simplifier.Annotation, cancellationToken: cancellation).ConfigureAwait(false);
        Document formatted = await Formatter.FormatAsync(simplified, Formatter.Annotation, cancellationToken: cancellation).ConfigureAwait(false);
        return await Formatter.FormatAsync(formatted, SyntaxAnnotation.ElasticAnnotation, cancellationToken: cancellation).ConfigureAwait(false);
    }

    private static SyntaxNode Inlined(SyntaxNode root, SyntaxAnnotation declared, SyntaxAnnotation read) {
        if (root.GetAnnotatedNodes(declared).FirstOrDefault() is not { } declaration || root.GetAnnotatedNodes(read).FirstOrDefault() is not { } site) return root;
        ExpressionSyntax Grouped(ExpressionSyntax expression) =>
            site.Parent is ExpressionStatementSyntax
                ? expression.WithTriviaFrom(site).WithAdditionalAnnotations(Formatter.Annotation)
                : SyntaxFactory.ParenthesizedExpression(expression.WithoutTrivia()).WithTriviaFrom(site).WithAdditionalAnnotations(Simplifier.Annotation, Formatter.Annotation);
        static ImmutableArray<ExpressionSyntax> Elements(ExpressionSyntax value) =>
            value switch {
                CollectionExpressionSyntax collection => [.. collection.Elements.OfType<ExpressionElementSyntax>().Select(static element => element.Expression)],
                ArrayCreationExpressionSyntax { Initializer: { } initializer } => [.. initializer.Expressions],
                ImplicitArrayCreationExpressionSyntax { Initializer: var initializer } => [.. initializer.Expressions],
                BaseObjectCreationExpressionSyntax { Initializer: { } initializer } => [.. initializer.Expressions],
                InvocationExpressionSyntax { Expression: MemberAccessExpressionSyntax access } => Elements(access.Expression),
                _ => [],
            };
        static ExpressionSyntax Tested(ExpressionSyntax operand, PatternSyntax pattern, SyntaxNode replaced) =>
            SyntaxFactory.ParenthesizedExpression(SyntaxFactory.IsPatternExpression(SyntaxFactory.ParenthesizedExpression(operand.WithoutTrivia()).WithAdditionalAnnotations(Simplifier.Annotation), pattern))
                .WithTriviaFrom(replaced)
                .WithAdditionalAnnotations(Simplifier.Annotation, Formatter.Annotation);
        static (SyntaxNode Replaced, SyntaxNode Replacement)? Matched(SyntaxNode site, ImmutableArray<ExpressionSyntax> elements) {
            PatternSyntax pattern = elements.Select(static element => (PatternSyntax)SyntaxFactory.ConstantPattern(element.WithoutTrivia()))
                .Aggregate(static (left, right) => SyntaxFactory.BinaryPattern(SyntaxKind.OrPattern, left, right));
            (InvocationExpressionSyntax Call, ExpressionSyntax Operand)? contained = site.Parent switch {
                MemberAccessExpressionSyntax { Parent: InvocationExpressionSyntax { ArgumentList.Arguments: { Count: 1 } arguments } call } access when access.Expression == site => (call, arguments[0].Expression),
                ArgumentSyntax { Parent: ArgumentListSyntax { Arguments: { Count: 2 } arguments, Parent: InvocationExpressionSyntax call } } when arguments[0] == site.Parent => (call, arguments[1].Expression),
                _ => null,
            };
            return contained is { } found
                ? found.Call.Parent is PrefixUnaryExpressionSyntax negated && negated.IsKind(SyntaxKind.LogicalNotExpression)
                    ? (negated, Tested(found.Operand, SyntaxFactory.UnaryPattern(SyntaxFactory.Token(SyntaxKind.NotKeyword), elements.Length is 1 ? pattern
                        : SyntaxFactory.ParenthesizedPattern(SyntaxFactory.Token(default, SyntaxKind.OpenParenToken, default), pattern, SyntaxFactory.Token(default, SyntaxKind.CloseParenToken, default))), negated))
                    : (found.Call, Tested(found.Operand, pattern, found.Call))
                : null;
        }
        static ExpressionSyntax Substituted(ExpressionSyntax body, ParameterListSyntax parameters, SeparatedSyntaxList<ArgumentSyntax> arguments) {
            ImmutableDictionary<string, ExpressionSyntax> bound = arguments
                .Select((argument, index) => (Name: argument.NameColon?.Name.Identifier.ValueText ?? parameters.Parameters[index].Identifier.ValueText, argument.Expression))
                .ToImmutableDictionary(static pair => pair.Name, static pair => pair.Expression, StringComparer.Ordinal);
            return body.ReplaceNodes(
                Scopes.Referenced(body).Where(used => bound.ContainsKey(used.Identifier.ValueText)),
                (original, _) => SyntaxFactory.ParenthesizedExpression(bound[original.Identifier.ValueText].WithoutTrivia()).WithTriviaFrom(original).WithAdditionalAnnotations(Simplifier.Annotation));
        }
        static ExpressionSyntax Lambda(MethodDeclarationSyntax method, ExpressionSyntax body) =>
            (method.Modifiers.Any(SyntaxKind.StaticKeyword) ? SyntaxFactory.TokenList(SyntaxFactory.Token(SyntaxKind.StaticKeyword)) : default) is var modifiers
            && method.ParameterList.Parameters is { Count: 1 } single
                ? SyntaxFactory.SimpleLambdaExpression(SyntaxFactory.Parameter(single[0].Identifier.WithoutTrivia()), body.WithoutTrivia()).WithModifiers(modifiers)
                : SyntaxFactory.ParenthesizedLambdaExpression(
                        SyntaxFactory.ParameterList(SyntaxFactory.SeparatedList(method.ParameterList.Parameters.Select(static parameter => SyntaxFactory.Parameter(parameter.Identifier.WithoutTrivia())))),
                        body.WithoutTrivia())
                    .WithModifiers(modifiers);
        (SyntaxNode Replaced, SyntaxNode Replacement)? replaced = (declaration, site) switch {
            (VariableDeclaratorSyntax { Initializer.Value: var value }, _) when Elements(value) is { IsEmpty: false } elements => Matched(site, elements),
            (VariableDeclaratorSyntax { Initializer.Value: var value }, _) when value is LiteralExpressionSyntax or PrefixUnaryExpressionSyntax or MemberAccessExpressionSyntax or IdentifierNameSyntax =>
                (site, value.WithTriviaFrom(site).WithAdditionalAnnotations(Formatter.Annotation)),
            (PropertyDeclarationSyntax { ExpressionBody.Expression: var body }, _) => (site, Grouped(body)),
            (PropertyDeclarationSyntax { AccessorList.Accessors: { Count: 1 } accessors }, _) when accessors[0].ExpressionBody?.Expression is { } body => (site, Grouped(body)),
            (MethodDeclarationSyntax { ExpressionBody.Expression: var body } method, InvocationExpressionSyntax { ArgumentList.Arguments: var arguments }) =>
                (site, Grouped(Substituted(body, method.ParameterList, arguments))),
            (MethodDeclarationSyntax { ExpressionBody.Expression: var body } method, _) => (site, Grouped(Lambda(method, body))),
            _ => null,
        };
        if (replaced is not { } pair) return root;
        SyntaxNode placed = root.ReplaceNode(pair.Replaced, pair.Replacement);
        static bool Commented(SyntaxTriviaList trivia) => trivia.Any(SyntaxKind.SingleLineCommentTrivia) || trivia.Any(SyntaxKind.MultiLineCommentTrivia);
        SyntaxNode Removed(MemberDeclarationSyntax member) {
            if (member.Parent is not TypeDeclarationSyntax { Members: var members } type || (members.IndexOf(member) is var index && index + 1 >= members.Count))
                return placed.RemoveNode(member, SyntaxRemoveOptions.KeepNoTrivia) ?? placed;
            SyntaxTriviaList leading = member.GetLeadingTrivia();
            MemberDeclarationSyntax next = members[index + 1];
            SyntaxTriviaList after = next.GetLeadingTrivia();
            bool carried = Commented(leading) && !Commented(after);
            IEnumerable<SyntaxTrivia> kept = carried ? leading.Reverse().SkipWhile(static trivia => !trivia.IsKind(SyntaxKind.EndOfLineTrivia)).Reverse()
                : index is 0 || (after.Count is not 0 && after[0].IsKind(SyntaxKind.EndOfLineTrivia)) ? []
                : leading.TakeWhile(static trivia => trivia.IsKind(SyntaxKind.EndOfLineTrivia));
            IEnumerable<SyntaxTrivia> following = carried || index is 0 ? after.SkipWhile(static trivia => trivia.IsKind(SyntaxKind.EndOfLineTrivia)) : after;
            return placed.ReplaceNode(type, type.WithMembers(members.Replace(next, next.WithLeadingTrivia(kept.Concat(following))).RemoveAt(index)));
        }
        return placed.GetAnnotatedNodes(declared).FirstOrDefault() switch {
            VariableDeclaratorSyntax { Parent: VariableDeclarationSyntax { Variables.Count: 1, Parent: FieldDeclarationSyntax field } } => Removed(field),
            MemberDeclarationSyntax member => Removed(member),
            { } node => placed.RemoveNode(node, SyntaxRemoveOptions.KeepNoTrivia) ?? placed,
            null => placed,
        };
    }
}
