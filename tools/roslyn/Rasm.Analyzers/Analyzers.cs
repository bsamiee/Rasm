using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Operations;
using Microsoft.CodeAnalysis.Text;

namespace Rasm.Analyzers;

// --- [COMPOSITION] ---------------------------------------------------------------------
public abstract class RasmAnalyzer(GeneratedCodeAnalysisFlags generated = GeneratedCodeAnalysisFlags.None) : DiagnosticAnalyzer {
    public sealed override void Initialize(AnalysisContext context) {
        context.EnableConcurrentExecution();
        context.ConfigureGeneratedCodeAnalysis(generated);
        context.RegisterCompilationStartAction(Start);
    }

    protected abstract void Start(CompilationStartAnalysisContext context);

    // --- [DESCRIPTORS]
    protected static DiagnosticDescriptor Rule(string id, string category, string title, string message, string description) =>
        new(id, title, message, category, DiagnosticSeverity.Error, isEnabledByDefault: true, description);

    protected static ImmutableArray<ISymbol> Declared(Compilation compilation, params string[] ids) =>
        [.. ids.SelectMany(id => DocumentationCommentId.GetSymbolsForDeclarationId(id, compilation))];

    protected static Location Named(IInvocationOperation call) =>
        call.Syntax is InvocationExpressionSyntax { Expression: MemberAccessExpressionSyntax access } syntax
            ? Location.Create(syntax.SyntaxTree, TextSpan.FromBounds(access.Name.SpanStart, syntax.Span.End))
            : call.Syntax.GetLocation();

    // --- [SYMBOLS]
    protected static bool Same(ISymbol? symbol, ISymbol? other) =>
        SymbolEqualityComparer.Default.Equals(symbol?.OriginalDefinition, other?.OriginalDefinition);

    protected static bool Within(ISymbol? symbol, INamedTypeSymbol? owner) =>
        symbol is not null && owner is not null && (Same(symbol, owner) || Within(symbol.ContainingSymbol, owner));

    protected static bool Derives(ITypeSymbol? type, INamedTypeSymbol root) =>
        type is not null && (Same(type, root) || Derives(type.BaseType, root));

    // --- [OPERATIONS]
    protected static T? Ancestor<T>(IOperation operation, Func<T, bool> wanted) where T : class, IOperation =>
        operation.Parent switch {
            null => null,
            T held when wanted(held) => held,
            var parent => Ancestor(parent, wanted),
        };

    protected static bool Inside<T>(IOperation operation) where T : class, IOperation => Ancestor<T>(operation, static _ => true) is not null;

    protected static IOperation? Consumer(IOperation operation) =>
        operation.Parent is IConversionOperation { IsImplicit: true } conversion ? Consumer(conversion) : operation.Parent;

    protected static IOperation Stripped(IOperation operation) =>
        operation is IConversionOperation { IsImplicit: true } conversion ? Stripped(conversion.Operand) : operation;

    protected static IOperation? Receiver(IInvocationOperation call) =>
        call.Instance ?? (call.Arguments.IsEmpty ? null : call.Arguments[0].Value);

    protected static bool Parameterless(IInvocationOperation call) => call.Arguments.Length == (call.Instance is null ? 1 : 0);

    // --- [SWITCHES]
    protected static (IOperation Subject, ImmutableArray<(IOperation Arm, IOperation? Test, IOperation? Guard)> Arms)? Dispatch(IOperation operation) =>
        operation switch {
            ISwitchExpressionOperation expression => (expression.Value, [.. expression.Arms.Select(static arm => ((IOperation)arm, (IOperation?)arm.Pattern, arm.Guard))]),
            ISwitchOperation statement => (statement.Value, [
                .. statement.Cases.SelectMany(static section => section.Clauses).Select(static clause => clause switch {
                    IPatternCaseClauseOperation patterned => ((IOperation)patterned, (IOperation?)patterned.Pattern, patterned.Guard),
                    ISingleValueCaseClauseOperation valued => (valued, valued.Value, null),
                    IDefaultCaseClauseOperation => (clause, null, null),
                    _ => (clause, clause, null),
                }),
            ]),
            _ => null,
        };

    protected static bool CatchesAll((IOperation Arm, IOperation? Test, IOperation? Guard) arm) =>
        arm is { Guard: null, Test: null or IPatternOperation } && (arm.Test is not IPatternOperation pattern || Open(pattern));

    private static bool Open(IPatternOperation pattern) =>
        pattern switch {
            IDiscardPatternOperation => true,
            IDeclarationPatternOperation declaration => Whole(declaration.MatchedType, declaration),
            ITypePatternOperation type => Whole(type.MatchedType, type),
            INegatedPatternOperation { Pattern: IConstantPatternOperation { Value.ConstantValue: { HasValue: true, Value: null } } } => true,
            IRecursivePatternOperation recursive => Whole(recursive.MatchedType, recursive) && recursive.PropertySubpatterns.IsEmpty && recursive.DeconstructionSubpatterns.All(Open),
            IBinaryPatternOperation { OperatorKind: BinaryOperatorKind.Or } either => Open(either.LeftPattern) || Open(either.RightPattern),
            IBinaryPatternOperation { OperatorKind: BinaryOperatorKind.And } both => Open(both.LeftPattern) && Open(both.RightPattern),
            _ => false,
        };

    private static bool Whole(ITypeSymbol? matched, IPatternOperation pattern) =>
        matched is null or { SpecialType: SpecialType.System_Object } || SymbolEqualityComparer.Default.Equals(matched, pattern.InputType);
}
