using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Operations;

namespace Rasm.Analyzers.LanguageExt;

// --- [COMPOSITION] ---------------------------------------------------------------------
[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class ExplicitResultLiftAnalyzer : RasmAnalyzer {
    private static readonly DiagnosticDescriptor Success = Rule(
        "RASM1001",
        "LanguageExt",
        "Explicit Unit success",
        "`{0}` restates implicit conversion of `{1}` to target type",
        "Write the `Unit` value alone where target type is `Fin<Unit>` or `Validation<E, Unit>`.");

    private static readonly DiagnosticDescriptor Failure = Rule(
        "RASM1002",
        "LanguageExt",
        "Explicit Fin failure",
        "`{0}` restates implicit conversion of `{1}` to target type",
        "Write the error alone where target type is `Fin<A>`.");

    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => [Success, Failure];

    protected override void Start(CompilationStartAnalysisContext context) {
        Compilation compilation = context.Compilation;
        if (compilation.GetTypeByMetadataName("LanguageExt.Fin`1") is not { } fin
            || compilation.GetTypeByMetadataName("LanguageExt.Validation`2") is not { } validation
            || compilation.GetTypeByMetadataName("LanguageExt.Unit") is not { } unit
            || compilation.GetTypeByMetadataName("LanguageExt.Common.Error") is not { } error
            || compilation.GetTypeByMetadataName("LanguageExt.Prelude") is not { } prelude
            || compilation.GetTypeByMetadataName("LanguageExt.Fin") is not { } fins
            || compilation.GetTypeByMetadataName("LanguageExt.Validation") is not { } validations) return;
        ImmutableArray<ISymbol> succeeding = [.. fins.GetMembers("Succ"), .. prelude.GetMembers("FinSucc"), .. prelude.GetMembers("Pure"), .. prelude.GetMembers("Success"), .. validations.GetMembers("Success")];
        ImmutableArray<ISymbol> failing = [.. fins.GetMembers("Fail"), .. prelude.GetMembers("FinFail"), .. prelude.GetMembers("Fail")];
        INamedTypeSymbol? succeeded = compilation.GetTypeByMetadataName("LanguageExt.Fin`1+Succ");
        INamedTypeSymbol? failed = compilation.GetTypeByMetadataName("LanguageExt.Fin`1+Fail");
        bool Unit(IOperation value) => Same(value.Type, unit);
        bool Failed(IOperation value) => Derives(value.Type, error);
        bool Inferred(ITypeSymbol? type, IMethodSymbol method) =>
            type switch {
                ITypeParameterSymbol parameter => Same(parameter.DeclaringMethod, method),
                INamedTypeSymbol named => named.TypeArguments.Any(argument => Inferred(argument, method)),
                IArrayTypeSymbol array => Inferred(array.ElementType, method),
                _ => false,
            };
        bool Untyped(IOperation? position) =>
            position switch {
                IArgumentOperation { Parameter: { } parameter, Parent: IInvocationOperation call } => Inferred(parameter.OriginalDefinition.Type, call.TargetMethod.OriginalDefinition),
                IVariableInitializerOperation { Parent.Syntax: VariableDeclaratorSyntax { Parent: VariableDeclarationSyntax { Type.IsVar: true } } } => true,
                _ => false,
            };
        ITypeSymbol? Returned(IOperation operation, ISymbol owner) =>
            operation.Parent switch {
                IAnonymousFunctionOperation callback => callback.Symbol.IsAsync || (callback.Parent is IDelegateCreationOperation { Parent: var position } && Untyped(position))
                    ? null : callback.Symbol.ReturnType,
                ILocalFunctionOperation local => local.Symbol.IsAsync ? null : local.Symbol.ReturnType,
                null => owner switch {
                    IMethodSymbol { IsAsync: false } method => method.ReturnType,
                    IPropertySymbol property => property.Type,
                    _ => null,
                },
                var parent => Returned(parent, owner),
            };
        ITypeSymbol? Fixed(IOperation value, ISymbol owner) =>
            value.Parent switch {
                IConversionOperation { IsImplicit: true } conversion => Fixed(conversion, owner),
                IConditionalOperation conditional when conditional.Condition != value
                    && conditional.WhenTrue is not IDefaultValueOperation && conditional.WhenFalse is not IDefaultValueOperation => Fixed(conditional, owner),
                ISwitchExpressionArmOperation { Parent: ISwitchExpressionOperation dispatch } arm when arm.Value == value
                    && !dispatch.Arms.Any(static other => other.Value is IDefaultValueOperation) => Fixed(dispatch, owner),
                IReturnOperation returned => Returned(returned, owner),
                IVariableInitializerOperation { Parent: IVariableDeclaratorOperation declarator } initializer when !Untyped(initializer) => declarator.Symbol.Type,
                IFieldInitializerOperation field => field.InitializedFields.FirstOrDefault()?.Type,
                IPropertyInitializerOperation property => property.InitializedProperties.FirstOrDefault()?.Type,
                IArgumentOperation { Parameter: { } parameter } argument when !Untyped(argument) => parameter.Type,
                ISimpleAssignmentOperation assignment when assignment.Value == value => assignment.Target.Type,
                _ => null,
            };
        context.RegisterOperationAction(
            operation => {
                IOperation lift = operation.Operation;
                IOperation? value = lift switch {
                    IInvocationOperation { Arguments.Length: 1 } invocation => invocation.Arguments[0].Value,
                    IObjectCreationOperation { Arguments.Length: 1, Initializer: null } creation => creation.Arguments[0].Value,
                    IConversionOperation { IsImplicit: false, Parent: IConditionalOperation or ISwitchExpressionArmOperation } cast => cast.Operand,
                    _ => null,
                };
                bool? succeeds = value is null ? null : lift switch {
                    IInvocationOperation { TargetMethod: var method } when Unit(value) && succeeding.Any(member => Same(method, member)) => true,
                    IInvocationOperation { TargetMethod: var method } when Failed(value) && failing.Any(member => Same(method, member)) => false,
                    IObjectCreationOperation { Type: var created } when Unit(value) && Same(created, succeeded) => true,
                    IObjectCreationOperation { Type: var created } when Failed(value) && Same(created, failed) => false,
                    IConversionOperation cast when Unit(value) && !Same(cast.Type, unit) => true,
                    IConversionOperation cast when Failed(value) && Same(cast.Type, error) => false,
                    _ => null,
                };
                if (value is not null && succeeds is { } success
                    && Fixed(lift, operation.ContainingSymbol) is INamedTypeSymbol { TypeArguments: var arguments } result
                    && (success
                        ? (Same(result, fin) && Same(arguments[0], unit)) || (Same(result, validation) && Same(arguments[1], unit) && !Same(arguments[0], unit))
                        : Same(result, fin) && !Derives(arguments[0], error)))
                    operation.ReportDiagnostic(Diagnostic.Create(success ? Success : Failure, lift.Syntax.GetLocation(), lift.Syntax, value.Syntax));
            },
            OperationKind.Invocation,
            OperationKind.ObjectCreation,
            OperationKind.Conversion);
    }
}
