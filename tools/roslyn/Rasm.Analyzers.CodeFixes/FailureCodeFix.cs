using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Formatting;
using Microsoft.CodeAnalysis.Operations;
using Microsoft.CodeAnalysis.Simplification;

namespace Rasm.Analyzers.CodeFixes;

// --- [COMPOSITION] ---------------------------------------------------------------------
[ExportCodeFixProvider(LanguageNames.CSharp)]
public sealed class FailureCodeFix() : DocumentEditorCodeFix(
    (FailureAnalyzer.Predicate, static (_, _) => "Apply failure predicate through @catch", static (editor, site, _, token) => {
        IInvocationOperation operation = (IInvocationOperation)editor.SemanticModel.GetOperation(site, token)!;
        InvocationExpressionSyntax invocation = (InvocationExpressionSyntax)site;
        ImmutableArray<ITypeSymbol> types = ((INamedTypeSymbol)((INamedTypeSymbol)operation.TargetMethod.Parameters[1].Type).DelegateInvokeMethod!.ReturnType).TypeArguments;
        IMethodSymbol method = (IMethodSymbol)DocumentationCommentId.GetFirstSymbolForDeclarationId(
            "M:LanguageExt.Prelude.catch``2(System.Func{LanguageExt.Common.Error,System.Boolean},System.Func{LanguageExt.Common.Error,LanguageExt.Traits.K{``0,``1}})",
            editor.SemanticModel.Compilation)!;
        ImmutableArray<string> names = [.. operation.Arguments.Select(argument => method.Parameters[argument.Parameter!.Ordinal].Name)];
        Replaced(editor, invocation, (rewritten, generator) => {
            InvocationExpressionSyntax current = (InvocationExpressionSyntax)rewritten;
            ExpressionSyntax source = Extracted(current.Expression, ((MemberAccessExpressionSyntax)current.Expression).Expression);
            InvocationExpressionSyntax recovery = Invoked(Access(TypeExpression(generator, method.ContainingType),
                SyntaxFactory.GenericName(Identifier(generator, method.Name).Identifier,
                    SyntaxFactory.TypeArgumentList(Punctuation(SyntaxKind.LessThanToken), Separated(types.Select(type => TypeExpression(generator, type))), Punctuation(SyntaxKind.GreaterThanToken)))), []);
            SeparatedSyntaxList<ArgumentSyntax> arguments = SyntaxFactory.SeparatedList(current.ArgumentList.Arguments.Zip(names, (argument, name) => {
                IdentifierNameSyntax identifier = Identifier(generator, name);
                return argument.WithNameColon(argument.NameColon is { } named
                    ? named.WithName(identifier.WithTriviaFrom(named.Name))
                    : SyntaxFactory.NameColon(identifier, Punctuation(SyntaxKind.ColonToken)));
            }), current.ArgumentList.Arguments.GetSeparators());
            return current.CopyAnnotationsTo(Parenthesized(SyntaxFactory.BinaryExpression(SyntaxKind.BitwiseOrExpression, source, Punctuation(SyntaxKind.BarToken),
                    recovery.WithArgumentList(current.ArgumentList.WithArguments(arguments)))))
                .WithAdditionalAnnotations(Simplifier.Annotation, Formatter.Annotation);
        });
    }
));
