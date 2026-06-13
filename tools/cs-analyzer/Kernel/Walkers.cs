using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Operations;

namespace Rasm.Csp.Kernel;

// --- [OPERATIONS] ----------------------------------------------------------------------

internal static class Walkers {
    // Fluent-pipeline receiver walk: extension methods carry their receiver in Arguments[0], not Instance.
    // Use ExtractReceiver to obtain the receiver of any invocation regardless of extension-vs-instance shape;
    // chain with UnwrapReceiver to peel implicit IConversionOperation wrappers (boxing, generic constraints).
    internal static IOperation? ExtractReceiver(IInvocationOperation invocation) =>
        invocation.Instance switch {
            IOperation receiver => receiver,
            _ => invocation.TargetMethod.IsExtensionMethod switch {
                true when invocation.Arguments.Length > 0 => invocation.Arguments[0].Value,
                _ => null,
            },
        };

    internal static IOperation? UnwrapReceiver(IOperation? operation) =>
        operation switch {
            IConversionOperation { Operand: IOperation inner } => UnwrapReceiver(inner),
            _ => operation,
        };

    internal static IOperation? UnwrapValue(IOperation? operation) =>
        operation switch {
            IConversionOperation conversion => UnwrapValue(conversion.Operand),
            IParenthesizedOperation parenthesized => UnwrapValue(parenthesized.Operand),
            IBlockOperation { Operations.Length: 1 } block => UnwrapValue(block.Operations[0]),
            IReturnOperation { ReturnedValue: IOperation returned } => UnwrapValue(returned),
            _ => operation,
        };

    internal static IAnonymousFunctionOperation? UnwrapLambda(IOperation operation) =>
        operation switch {
            IAnonymousFunctionOperation lambda => lambda,
            IDelegateCreationOperation { Target: IAnonymousFunctionOperation lambda } => lambda,
            IConversionOperation { Operand: IOperation inner } => UnwrapLambda(inner),
            _ => null,
        };

    internal static SyntaxNode UnwrapTransparentExpression(SyntaxNode node) =>
        node switch {
            ParenthesizedExpressionSyntax parenthesized => UnwrapTransparentExpression(parenthesized.Expression),
            CastExpressionSyntax castExpression => UnwrapTransparentExpression(castExpression.Expression),
            CheckedExpressionSyntax checkedExpression => UnwrapTransparentExpression(checkedExpression.Expression),
            AwaitExpressionSyntax awaitExpression => UnwrapTransparentExpression(awaitExpression.Expression),
            PostfixUnaryExpressionSyntax { RawKind: (int)SyntaxKind.SuppressNullableWarningExpression } suppressNullable
                => UnwrapTransparentExpression(suppressNullable.Operand),
            _ => node,
        };

    internal static IOperation? CollapseTransparentParents(IOperation operation) =>
        operation.Parent switch {
            IConversionOperation or IParenthesizedOperation => CollapseTransparentParents(operation.Parent),
            IOperation parent => parent,
            _ => null,
        };

    // Terminal-Match-position analysis: Match is boundary-legal only as a returned value, a terminal
    // Unit-typed expression statement, a terminal discard assignment, or the whole arrow body.
    // unitType is the Facts-resolved LanguageExt.Unit symbol (SymbolEqualityComparer, never display strings).
    internal static bool IsBoundaryMatchUsage(IInvocationOperation invocation, INamedTypeSymbol? unitType) =>
        CollapseTransparentParents(invocation) is IReturnOperation
        || (unitType is not null
            && SymbolEqualityComparer.Default.Equals(invocation.Type, unitType)
            && (IsTerminalExpressionStatement(operation: CollapseTransparentParents(operation: invocation))
                || IsTerminalDiscardAssignment(operation: CollapseTransparentParents(operation: invocation))))
        || (invocation.Syntax.Parent is ArrowExpressionClauseSyntax clause
            && clause.Expression is ExpressionSyntax expression
            && SyntaxFactory.AreEquivalent(UnwrapTransparentExpression(invocation.Syntax), UnwrapTransparentExpression(expression)));

    private static bool IsTerminalExpressionStatement(IOperation? operation) =>
        operation is IExpressionStatementOperation statement
        && statement.Parent is IBlockOperation block
        && block.Operations.LastOrDefault() == statement;

    private static bool IsTerminalDiscardAssignment(IOperation? operation) =>
        operation is ISimpleAssignmentOperation { Target: IDiscardOperation, Parent: IExpressionStatementOperation statement }
        && statement.Parent is IBlockOperation block
        && block.Operations.LastOrDefault() == statement;
}
