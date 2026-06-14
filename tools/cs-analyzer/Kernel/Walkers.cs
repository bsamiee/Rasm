using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Operations;

namespace Rasm.Csp.Kernel;

// --- [OPERATIONS] ----------------------------------------------------------------------

internal static class Walkers {
    // Extension invocations carry receivers in Arguments[0]; instance calls use Instance.
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

    // Boundary-legal Match sites are terminal return, Unit expression, discard, or arrow body.
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
