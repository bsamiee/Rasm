using System.Collections.Concurrent;
using System.Collections.Immutable;
using Analyzer.Utilities;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.FlowAnalysis;
using Microsoft.CodeAnalysis.Operations;

namespace Rasm.Analyzers;

// --- [COMPOSITION] ---------------------------------------------------------------------

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class TypeNameTextAnalyzer : DiagnosticAnalyzer {

    // --- [RULES]

    private static readonly DiagnosticDescriptor Rule = new(
        "RASM3101",
        "Type name stored as text, store the type",
        "`{0}` stores a CLR type name in `{1}`, store the `Type` or value and render its name where a person reads it",
        Categories.CSharp,
        DiagnosticSeverity.Error,
        isEnabledByDefault: true,
        "Records and errors hold a `Type` or value in place of a CLR type name, rendered where a person reads it, and a type converting implicitly from `string` holds text by design.",
        Link("RASM3101"),
        WellKnownDiagnosticTags.CompilationEnd);

    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => [Rule];

    public override void Initialize(AnalysisContext context) {
        context.EnableConcurrentExecution();
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
        context.RegisterCompilationStartAction(static start => {
            Compilation compilation = start.Compilation;
            WellKnownTypeProvider types = WellKnownTypeProvider.GetOrCreate(compilation);
            if (types.GetOrCreateTypeByMetadataName("System.Type") is not { } type) return;
            ControlFlow flow = ControlFlow.For(compilation);
            SemanticModels semantics = new(start);
            ConcurrentBag<IOperation> sources = [];
            INamedTypeSymbol? error = types.GetOrCreateTypeByMetadataName("LanguageExt.Common.Error");
            ImmutableArray<INamedTypeSymbol> rendered = [.. new[] { types.GetOrCreateTypeByMetadataName("System.Exception"), error }.OfType<INamedTypeSymbol>()];
            ImmutableArray<ISymbol> names = [
                .. Bases(type).OfType<INamedTypeSymbol>().SelectMany(static owner => owner.GetMembers()).Where(static member => member is IPropertySymbol { Name: nameof(Type.Name) or nameof(Type.FullName) or nameof(Type.AssemblyQualifiedName) }),
                .. Members(type, nameof(object.ToString)).Where(static method => method.Parameters.IsEmpty),
            ];
            ImmutableArray<IMethodSymbol> transforms = Members(compilation.GetSpecialType(SpecialType.System_String), nameof(string.ToUpperInvariant), nameof(string.ToLowerInvariant), nameof(string.Trim), nameof(string.TrimEnd), nameof(string.TrimStart));
            bool Rendered(ITypeSymbol made) => rendered.Any(root => Derives(made, root))
                || made.GetMembers(WellKnownMemberNames.ImplicitConversionName).OfType<IMethodSymbol>().Any(static conversion => conversion.Parameters is { Length: 1 } held && held[0].Type.SpecialType is SpecialType.System_String);
            bool Failure(IOperation value) => Inside<IThrowOperation>(value) || Ancestor<IArgumentOperation>(value, argument =>
                argument.Parent is IInvocationOperation call && Same(call.TargetMethod.ContainingAssembly, error?.ContainingAssembly)
                && call.TargetMethod.Name is "ToFin" or "ToValidation" or "ToEither" or "Fail" or "fail" or "guard" or "guardnot") is not null;
            bool Typed(IOperation? receiver) => receiver is not null && Derives(Stripped(receiver).Type, type);
            start.RegisterOperationAction(analysis => {
                if (analysis.Operation switch {
                    IPropertyReferenceOperation property => Typed(property.Instance) && Overridden(property.Property).Any(member => Among(member, names)),
                    IInvocationOperation call => Typed(call.Instance) && Overridden(call.TargetMethod).Any(member => Among(member, names)),
                    INameOfOperation => true,
                    _ => false,
                }) sources.Add(analysis.Operation);
            }, OperationKind.PropertyReference, OperationKind.Invocation, OperationKind.NameOf);
            ConcurrentDictionary<ISymbol, ImmutableArray<IOperation>> declarations = new(SymbolEqualityComparer.Default);
            ConcurrentDictionary<IOperation, ILookup<ISymbol?, IOperation>> references = [];
            IEnumerable<ControlFlow.Value> Reads(ISymbol symbol, ControlFlow.Value at, ImmutableDictionary<SyntaxTree, SemanticModel> models, CancellationToken token) {
                ImmutableArray<IOperation> uses = [.. (symbol is IParameterSymbol parameter ? Initializers(parameter.ContainingSymbol, compilation, models, declarations, token) : [Root(at.Operation)])
                    .SelectMany(root => references.GetOrAdd(root, static held => held.DescendantsAndSelf().Where(static value => value is IParameterReferenceOperation or ILocalReferenceOperation).ToLookup(Variable, SymbolEqualityComparer.Default))[symbol.OriginalDefinition])];
                bool unchanged = !uses.Any(static used => Written(used) || used.Parent is IVariableInitializerOperation { Parent: IVariableDeclaratorOperation { Symbol.RefKind: not RefKind.None } });
                return from used in uses
                       where !Written(used)
                       from reading in flow.Readings(used, token)
                       where !reading.Definition.HasValue
                       ? unchanged || (symbol is IParameterSymbol captured
                           && (Ancestor<IOperation>(used, static owner => owner is IAnonymousFunctionOperation or ILocalFunctionOperation) is not { } function
                               || (function is IAnonymousFunctionOperation lambda && Same(lambda.Symbol, captured.ContainingSymbol))
                               || (function is ILocalFunctionOperation local && Same(local.Symbol, captured.ContainingSymbol))))
                       : symbol is ILocalSymbol && reading.Definition.Value.Originates(at, token)
                       select reading.Reference;
            }
            IEnumerable<INamedTypeSymbol> Stored(IOperation source, ImmutableDictionary<SyntaxTree, SemanticModel> models, CancellationToken token) =>
                ControlFlow.Trace<ControlFlow.Value, INamedTypeSymbol?>(flow.Evaluations(source, token), current => Consumer(current.Operation) switch {
                    IArgumentOperation { Parent: IObjectCreationOperation { Type: INamedTypeSymbol created } } when !Rendered(created) && !Failure(current.Operation) => ([], [created]),
                    ISimpleAssignmentOperation { Target: IMemberReferenceOperation, Parent: IObjectOrCollectionInitializerOperation { Parent.Type: INamedTypeSymbol created } } when !Rendered(created) && !Failure(current.Operation) => ([], [created]),
                    IVariableInitializerOperation { Parent: IVariableDeclaratorOperation declarator } => (Reads(declarator.Symbol, current, models, token), []),
                    ISimpleAssignmentOperation { Target: ILocalReferenceOperation local } => (Reads(local.Local, current, models, token), []),
                    IArgumentOperation { Parameter: { } parameter, Parent: IInvocationOperation call } when !Failure(current.Operation) && !Rendered(call.TargetMethod.ReturnType) => (Reads(parameter, current.With(call), models, token), []),
                    IInvocationOperation { TargetMethod.ReducedFrom: { } method } call when call.Instance == current.Operation => (Reads(method.Parameters[0], current.With(call), models, token), []),
                    IInvocationOperation call when Among(call.TargetMethod, transforms) => ([current.With(call)], []),
                    IConditionalAccessOperation conditional when conditional.WhenNotNull == current.Operation || (conditional.WhenNotNull is IInvocationOperation call && Among(call.TargetMethod, transforms)) => ([current.With(conditional)], []),
                    (ICoalesceOperation or IConditionalOperation or IInterpolationOperation or IInterpolatedStringOperation or IBinaryOperation { OperatorKind: BinaryOperatorKind.Add, Type.SpecialType: SpecialType.System_String }) and var parent => ([current.With(parent)], []),
                    _ => ([], []),
                }, depthFirst: true, token).OfType<INamedTypeSymbol>();
            semantics.Complete((end, models) => {
                foreach (Diagnostic diagnostic in from source in sources
                                                  where source is not INameOfOperation named || named.SemanticModel!.GetSymbolInfo(named.Argument.Syntax, end.CancellationToken).Symbol is ITypeParameterSymbol
                                                  from made in Stored(source, models, end.CancellationToken).Take(1)
                                                  select Diagnostic.Create(Rule, source.Syntax.GetLocation(), source.Syntax, made.Name))
                    end.ReportDiagnostic(diagnostic);
            });
        });
    }

    internal static ImmutableArray<IOperation> Initializers(ISymbol symbol, Compilation compilation, ImmutableDictionary<SyntaxTree, SemanticModel> models, ConcurrentDictionary<ISymbol, ImmutableArray<IOperation>> declarations, CancellationToken token) =>
        declarations.GetOrAdd(symbol.OriginalDefinition, held => [
            .. held.DeclaringSyntaxReferences.Concat(held is IPropertySymbol { GetMethod: { } getter } ? getter.DeclaringSyntaxReferences : [])
                .Where(declared => compilation.ContainsSyntaxTree(declared.SyntaxTree) && models.ContainsKey(declared.SyntaxTree))
                .Select(declared => models[declared.SyntaxTree].GetOperation(declared.GetSyntax(token) switch {
                    VariableDeclaratorSyntax { Initializer.Value: var value } => value,
                    PropertyDeclarationSyntax { Initializer.Value: var value } => value,
                    PropertyDeclarationSyntax { ExpressionBody.Expression: var value } => value,
                    AccessorDeclarationSyntax { ExpressionBody.Expression: var value } => value,
                    AccessorDeclarationSyntax { Body: { } body } => body,
                    var syntax => syntax,
                }, token)).OfType<IOperation>(),
        ]);
}

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class SentinelAnalyzer : DiagnosticAnalyzer {

    // --- [RULES]

    private static readonly DiagnosticDescriptor Rule = new(
        "RASM3102",
        "Sentinel stands for absence, state absence in the type",
        "`{1}` stands for absence in `{0}`, state absence through `Option`, a nullable, or a case holding only the facts its path supplies",
        Categories.CSharp,
        DiagnosticSeverity.Error,
        isEnabledByDefault: true,
        "Absence is `Option`, a nullable, or a case holding only the facts its path supplies. Record members receive no sentinel (`RASM3102.sentinels` naming an assembly's sentinel member prefix as `<assembly>:<prefix>`), and no coalesce, `IfNone`, `None` arm, fallback argument, or string parameter default yields an empty string for absence, outside a method whose whole body converts its one `Option` parameter for a host.",
        Link("RASM3102"),
        WellKnownDiagnosticTags.CompilationEnd);

    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => [Rule];

    public override void Initialize(AnalysisContext context) {
        context.EnableConcurrentExecution();
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
        context.RegisterCompilationStartAction(static start => {
            Compilation compilation = start.Compilation;
            WellKnownTypeProvider types = WellKnownTypeProvider.GetOrCreate(compilation);
            ControlFlow flow = ControlFlow.For(compilation);
            SemanticModels semantics = new(start);
            ConcurrentDictionary<ISymbol, byte> compared = new(SymbolEqualityComparer.Default);
            ConcurrentDictionary<INamedTypeSymbol, bool> data = new(SymbolEqualityComparer.Default);
            ConcurrentBag<IOperation> records = [];
            INamedTypeSymbol? option = types.GetOrCreateTypeByMetadataName("LanguageExt.Option`1");
            INamedTypeSymbol? union = types.GetOrCreateTypeByMetadataName("Thinktecture.UnionAttribute");
            INamedTypeSymbol? unionBase = types.GetOrCreateTypeByMetadataName("Thinktecture.UnionAttributeBase");
            ImmutableArray<(string Assembly, string Prefix)> prefixes = [.. Option(start.Options, "RASM3102.sentinels").Select(static row => row.Split(':')).Where(static parts => parts.Length is 2).Select(static parts => (parts[0], parts[1]))];
            ImmutableArray<ISymbol> missingTypes = Declared(compilation, "T:System.Guid", "T:System.DateTime", "T:System.DateTimeOffset");
            ImmutableArray<ISymbol> missingValues = Declared(compilation,
                "F:System.Guid.Empty", "F:System.DateTime.MinValue", "F:System.DateTimeOffset.MinValue", "F:System.Double.NaN", "F:System.Single.NaN");
            INamedTypeSymbol text = compilation.GetSpecialType(SpecialType.System_String);
            ISymbol? empty = Declared(compilation, "F:System.String.Empty").FirstOrDefault();
            ImmutableArray<IMethodSymbol> emptyTests = Members(text, nameof(string.IsNullOrEmpty), nameof(string.IsNullOrWhiteSpace));
            ImmutableArray<ISymbol> callers = Declared(compilation,
                "T:System.Runtime.CompilerServices.CallerArgumentExpressionAttribute", "T:System.Runtime.CompilerServices.CallerMemberNameAttribute", "T:System.Runtime.CompilerServices.CallerFilePathAttribute");
            ImmutableArray<IParameterSymbol> fallbacks = [
                .. Members(option, "IfNone").SelectMany(static method => method.Parameters),
                .. Members(option, "Match").SelectMany(static method => method.Parameters).Where(static parameter => parameter.Type is not INamedTypeSymbol { DelegateInvokeMethod.Parameters.Length: > 0 }),
            ];
            bool Data(INamedTypeSymbol made) => data.GetOrAdd(made, held => held.IsRecord || Containers(held.ContainingType).OfType<INamedTypeSymbol>().Any(owner =>
                owner.GetAttributes().Any(attribute => Same(attribute.AttributeClass, union) || (unionBase is not null && Derives(attribute.AttributeClass, unionBase)))));
            bool Empty(IOperation value) => Stripped(value) switch {
                { ConstantValue: { HasValue: true, Value: "" } } => true,
                IFieldReferenceOperation reference => Same(reference.Field, empty),
                _ => false,
            };
            bool Sentinel(ISymbol member) =>
                Among(member, missingValues) || (member.IsStatic && prefixes.Any(row => string.Equals(member.ContainingAssembly?.Name, row.Assembly, StringComparison.Ordinal)
                    && member.Name.StartsWith(row.Prefix, StringComparison.Ordinal)));
            static ISymbol? Compared(IOperation value) => Stripped(value) is IMemberReferenceOperation reference ? reference.Member : null;
            start.RegisterOperationAction(analysis => {
                ISymbol? member = analysis.Operation switch {
                    IBinaryOperation { OperatorKind: BinaryOperatorKind.Equals or BinaryOperatorKind.NotEquals } binary when Empty(binary.RightOperand) => Compared(binary.LeftOperand),
                    IBinaryOperation { OperatorKind: BinaryOperatorKind.Equals or BinaryOperatorKind.NotEquals } binary when Empty(binary.LeftOperand) => Compared(binary.RightOperand),
                    IBinaryOperation { OperatorKind: BinaryOperatorKind.Equals or BinaryOperatorKind.NotEquals, LeftOperand: IPropertyReferenceOperation { Property: { Name: nameof(string.Length), ContainingType.SpecialType: SpecialType.System_String }, Instance: { } instance }, RightOperand.ConstantValue: { HasValue: true, Value: 0 } } => Compared(instance),
                    IBinaryOperation { OperatorKind: BinaryOperatorKind.Equals or BinaryOperatorKind.NotEquals, RightOperand: IPropertyReferenceOperation { Property: { Name: nameof(string.Length), ContainingType.SpecialType: SpecialType.System_String }, Instance: { } instance }, LeftOperand.ConstantValue: { HasValue: true, Value: 0 } } => Compared(instance),
                    IInvocationOperation call when Among(call.TargetMethod, emptyTests) => Compared(call.Arguments[0].Value),
                    IIsPatternOperation {
                        Value: IPropertyReferenceOperation { Property: { Name: nameof(string.Length), ContainingType.SpecialType: SpecialType.System_String }, Instance: { } instance },
                        Pattern: IConstantPatternOperation { Value.ConstantValue: { HasValue: true, Value: 0 } }
                            or INegatedPatternOperation { Pattern: IConstantPatternOperation { Value.ConstantValue: { HasValue: true, Value: 0 } } }
                            or IRelationalPatternOperation { Value.ConstantValue: { HasValue: true, Value: 0 } },
                    } => Compared(instance),
                    IIsPatternOperation { Pattern: IConstantPatternOperation constant, Value: var tested } when Empty(constant.Value) => Compared(tested),
                    IPropertySubpatternOperation { Pattern: IConstantPatternOperation constant, Member: IMemberReferenceOperation reference } when Empty(constant.Value) => reference.Member,
                    _ => null,
                };
                if (member is not null) compared.TryAdd(member.OriginalDefinition, 0);
            }, OperationKind.Binary, OperationKind.Invocation, OperationKind.IsPattern, OperationKind.PropertySubpattern);
            start.RegisterOperationAction(analysis => {
                if (analysis.Operation.Type is INamedTypeSymbol made && Data(made)) records.Add(analysis.Operation);
            }, OperationKind.ObjectCreation, OperationKind.With);
            bool Yielded(IOperation value) => Empty(value) || (Lambda(value) is { } function && Sole(function.Body) is { } returned && Empty(returned));
            static bool Spliced(IOperation value) {
                IOperation? parent = value.Parent;
                while (parent is IConversionOperation or IConditionalOperation or IParenthesizedOperation) parent = parent.Parent;
                return parent is IInterpolationOperation or IBinaryOperation { OperatorKind: BinaryOperatorKind.Add, Type.SpecialType: SpecialType.System_String };
            }
            static bool Converted(IOperation value, IOperation? receiver) =>
                value.Parent is IReturnOperation { Parent: IBlockOperation { Operations.Length: 1, Parent: IMethodBodyOperation } }
                && receiver is not null && Stripped(receiver) is IParameterReferenceOperation { Parameter.ContainingSymbol: IMethodSymbol { Parameters.Length: 1 } };
            Report<IOperation>(start, [OperationKind.Coalesce, OperationKind.Invocation, OperationKind.Argument], (value, _) => value switch {
                ICoalesceOperation coalesce when Empty(coalesce.WhenNull) && !Spliced(coalesce) && !Converted(coalesce, coalesce.Value) =>
                    [Diagnostic.Create(Rule, coalesce.Syntax.GetLocation(), coalesce.Syntax, coalesce.WhenNull.Syntax)],
                IInvocationOperation call when !Spliced(call) && !Converted(call, Receiver(call))
                    && call.Arguments.FirstOrDefault(argument => Among(argument.Parameter, fallbacks) && Yielded(argument.Value)) is { } absent =>
                    [Diagnostic.Create(Rule, call.Syntax.GetLocation(), call.Syntax, absent.Value.Syntax)],
                IArgumentOperation { Parameter.Name: "fallback" or "defaultValue" } argument when Empty(argument.Value) =>
                    [Diagnostic.Create(Rule, argument.Syntax.GetLocation(), argument.Parameter.ContainingSymbol.Name, argument.Value.Syntax)],
                _ => [],
            });
            start.RegisterSymbolAction(analysis => {
                if (analysis.Symbol is not IMethodSymbol { IsOverride: false, ExplicitInterfaceImplementations.IsEmpty: true } method) return;
                foreach (IParameterSymbol parameter in method.Parameters.Where(parameter => parameter is { HasExplicitDefaultValue: true, ExplicitDefaultValue: "", Type.SpecialType: SpecialType.System_String }
                             && !parameter.GetAttributes().Any(attribute => Among(attribute.AttributeClass, callers))))
                    analysis.ReportDiagnostic(Diagnostic.Create(Rule, parameter.Locations.FirstOrDefault(), method.Name, "\"\""));
            }, SymbolKind.Method);
            ConcurrentDictionary<ISymbol, ImmutableArray<IOperation>> declarations = new(SymbolEqualityComparer.Default);
            ConcurrentDictionary<IParameterSymbol, ImmutableArray<ISymbol>> members = new(SymbolEqualityComparer.Default);
            ImmutableArray<IOperation> Operations(ISymbol symbol, ImmutableDictionary<SyntaxTree, SemanticModel> models, CancellationToken token) => TypeNameTextAnalyzer.Initializers(symbol, compilation, models, declarations, token);
            ImmutableArray<ISymbol> MembersOf(IParameterSymbol parameter, ImmutableDictionary<SyntaxTree, SemanticModel> models, CancellationToken token) =>
                members.GetOrAdd(parameter.OriginalDefinition, held => [
                    .. held.ContainingType.GetMembers().Where(member => member is IFieldSymbol or IPropertySymbol && (
                        member.DeclaringSyntaxReferences.Any(declared => models.TryGetValue(declared.SyntaxTree, out SemanticModel? model)
                            && Same(model.GetDeclaredSymbol(declared.GetSyntax(token), token), held))
                        || Operations(member, models, token).Any(initializer => Stripped(initializer) is IParameterReferenceOperation read && Same(read.Parameter, held)))),
                    .. Operations(held.ContainingSymbol, models, token).SelectMany(static body => body.DescendantsAndSelf()).OfType<ISimpleAssignmentOperation>()
                        .Where(assignment => assignment.Target is IMemberReferenceOperation && Stripped(assignment.Value) is IParameterReferenceOperation read && Same(read.Parameter, held))
                        .Select(static assignment => ((IMemberReferenceOperation)assignment.Target).Member),
                ]);
            IOperation? Absent(IOperation source, ISymbol? member, ImmutableDictionary<SyntaxTree, SemanticModel> models, CancellationToken token) =>
                ControlFlow.Trace<ControlFlow.Value, IOperation>([ControlFlow.At(source)], current => Stripped(current.Operation) switch {
                    var value when (Empty(value) && ((member is IParameterSymbol parameter && MembersOf(parameter, models, token).Any(held => compared.ContainsKey(held.OriginalDefinition)))
                            || (member is not (null or IParameterSymbol) && compared.ContainsKey(member.OriginalDefinition))))
                        || (value is IMemberReferenceOperation sentinel && Sentinel(sentinel.Member))
                        || (value is IDefaultValueOperation or IObjectCreationOperation { Arguments.IsEmpty: true, Initializer: null } && Among(value.Type, missingTypes)) => ([], [value]),
                    (ILocalReferenceOperation or IFlowCaptureReferenceOperation) and var value => (flow.Values(current.With(value), token)
                        .Where(static definition => definition.HasValue).Select(static definition => definition.Value), []),
                    IMemberReferenceOperation { Instance: { } instance } reference => (Operations(reference.Member, models, token).Select(ControlFlow.At).Append(current.With(instance)), []),
                    IMemberReferenceOperation reference => (Operations(reference.Member, models, token).Select(ControlFlow.At), []),
                    ICoalesceOperation coalesce => ([current.With(coalesce.WhenNull)], []),
                    IConditionalOperation { WhenFalse: { } alternative } conditional => ([current.With(conditional.WhenTrue), current.With(alternative)], []),
                    ISwitchExpressionOperation expression => (expression.Arms.Select(arm => current.With(arm.Value)), []),
                    IInvocationOperation call => (call.Arguments.Where(argument => Among(argument.Parameter, fallbacks)).Select(argument => current.With(argument.Value)), []),
                    IDelegateCreationOperation { Target: IAnonymousFunctionOperation function } when current.Operation.Parent is IArgumentOperation argument && Among(argument.Parameter, fallbacks) => ([ControlFlow.At(function.Body)], []),
                    IBlockOperation body when Sole(body) is { } returned => ([current.With(returned)], []),
                    _ => ([], []),
                }, depthFirst: true, token).FirstOrDefault();
            semantics.Complete((end, models) => {
                foreach (Diagnostic diagnostic in from site in records
                                                  let values = site.ChildOperations.OfType<IArgumentOperation>().Where(static argument => !argument.IsImplicit).Select(static argument => (argument.Value, Member: (ISymbol?)argument.Parameter))
                                                      .Concat(site.ChildOperations.OfType<IObjectOrCollectionInitializerOperation>().SelectMany(static initializer => initializer.Initializers)
                                                          .OfType<ISimpleAssignmentOperation>().Where(static assignment => assignment.Target is IMemberReferenceOperation)
                                                          .Select(static assignment => (assignment.Value, Member: (ISymbol?)((IMemberReferenceOperation)assignment.Target).Member)))
                                                  from absent in values.Select(value => Absent(value.Value, value.Member, models, end.CancellationToken)).OfType<IOperation>().Take(1)
                                                  select Diagnostic.Create(Rule, site.Syntax.GetLocation(), site.Type!.Name, absent.Syntax))
                    end.ReportDiagnostic(diagnostic);
            });
        });
    }
}

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class NullForgivingAnalyzer : DiagnosticAnalyzer {

    // --- [RULES]

    private static readonly DiagnosticDescriptor Rule = new(
        "RASM3105",
        "Null-forgiving operator on a nullable result, lift the absence",
        "`{0}` silences a nullable result, lift it through `Optional` into `Option` or `Fin` with the failure it stands for",
        Categories.CSharp,
        DiagnosticSeverity.Error,
        isEnabledByDefault: true,
        "A member annotated to return null states that its result can be absent. `!` on that result hides the absence and fails later with a null dereference; `Optional(...)` names the outcome. A result an override or interface implementation passes to a host signature that cannot carry null is exempt.",
        Link("RASM3105"));

    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => [Rule];

    public override void Initialize(AnalysisContext context) {
        context.EnableConcurrentExecution();
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
        context.RegisterSyntaxNodeAction(static analysis => {
            if (analysis.Node is not PostfixUnaryExpressionSyntax { Operand: var operand } forgiven) return;
            ISymbol? target = forgiven.Parent switch {
                AssignmentExpressionSyntax { Left: var left } assignment when assignment.Right == forgiven =>
                    analysis.SemanticModel.GetSymbolInfo(left, analysis.CancellationToken).Symbol is IParameterSymbol { RefKind: RefKind.Out } parameter ? parameter.ContainingSymbol : null,
                ReturnStatementSyntax or ArrowExpressionClauseSyntax => analysis.SemanticModel.GetEnclosingSymbol(forgiven.SpanStart, analysis.CancellationToken),
                _ => null,
            };
            bool nullable = analysis.SemanticModel.GetSymbolInfo(operand, analysis.CancellationToken).Symbol switch {
                IMethodSymbol method => method.ReturnNullableAnnotation is NullableAnnotation.Annotated,
                IPropertySymbol property => property.NullableAnnotation is NullableAnnotation.Annotated,
                _ => false,
            };
            if (nullable && !Hosted(target)) analysis.ReportDiagnostic(Diagnostic.Create(Rule, forgiven.GetLocation(), forgiven));
        }, SyntaxKind.SuppressNullableWarningExpression);
    }

    private static bool Hosted(ISymbol? member) => member is IMethodSymbol { IsOverride: true } or IMethodSymbol { ExplicitInterfaceImplementations.IsEmpty: false }
        || (member is IMethodSymbol method && method.ContainingType.AllInterfaces.SelectMany(static contract => contract.GetMembers())
            .Any(contract => SymbolEqualityComparer.Default.Equals(method.ContainingType.FindImplementationForInterfaceMember(contract), method)));
}
