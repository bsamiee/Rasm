using System.Collections.Concurrent;
using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.FlowAnalysis;
using Microsoft.CodeAnalysis.Operations;

namespace Rasm.Analyzers;

// --- [COMPOSITION] ---------------------------------------------------------------------

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class ConstructionAnalyzer : RasmAnalyzer {

    // --- [RULES]

    private const string Category = "CSharp";

    private static readonly DiagnosticDescriptor TypeName = Rule(
        "RASM3101",
        Category,
        "Type name stored as text, store the type",
        "`{0}` stores a CLR type name in `{1}`, store the `Type` or value and render its name where a person reads it",
        "Records and errors hold a `Type` or value in place of a CLR type name, rendered where a person reads it.",
        WellKnownDiagnosticTags.CompilationEnd);

    private static readonly DiagnosticDescriptor Sentinel = Rule(
        "RASM3102",
        Category,
        "Sentinel for an absent member, state absence in the type",
        "`{0}` receives sentinel `{1}` for an absent member, take `Option` or a case holding only the facts its path supplies",
        "Record members state absence through `Option` or a case holding only the facts its path supplies, never a sentinel value.",
        WellKnownDiagnosticTags.CompilationEnd);

    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => [TypeName, Sentinel];

    protected override void Start(CompilationStartAnalysisContext context) {
        Compilation compilation = context.Compilation;
        ControlFlow flow = ControlFlow.For(compilation);
        SemanticModels semantics = new(context);
        ConcurrentBag<IOperation> sources = [];
        ConcurrentDictionary<ISymbol, byte> compared = new(SymbolEqualityComparer.Default);
        ConcurrentDictionary<INamedTypeSymbol, bool> data = new(SymbolEqualityComparer.Default);
        ConcurrentDictionary<INamedTypeSymbol, ImmutableArray<ISymbol>> unsets = new(SymbolEqualityComparer.Default);
        ConcurrentBag<IOperation> records = [];
        INamedTypeSymbol? type = compilation.GetTypeByMetadataName("System.Type");
        INamedTypeSymbol? exception = compilation.GetTypeByMetadataName("System.Exception");
        INamedTypeSymbol? error = compilation.GetTypeByMetadataName("LanguageExt.Common.Error");
        INamedTypeSymbol? nomen = compilation.GetTypeByMetadataName("Grasshopper2.UI.Nomen");
        INamedTypeSymbol? option = compilation.GetTypeByMetadataName("LanguageExt.Option`1");
        INamedTypeSymbol? rhinoMath = compilation.GetTypeByMetadataName("Rhino.RhinoMath");
        ThinktectureSymbols thinktecture = new(compilation);
        ImmutableArray<ISymbol> missingTypes = Declared(compilation, "T:System.Guid", "T:System.DateTime", "T:System.DateTimeOffset");
        ImmutableArray<ISymbol> missingValues = Declared(compilation,
            "F:System.Guid.Empty", "F:System.DateTime.MinValue", "F:System.DateTimeOffset.MinValue", "F:System.Double.NaN", "F:System.Single.NaN");
        ImmutableArray<ISymbol> names = [
            .. Bases(type).OfType<INamedTypeSymbol>().SelectMany(static owner => owner.GetMembers()).Where(static member => member is IPropertySymbol { Name: nameof(Type.Name) or nameof(Type.FullName) or nameof(Type.AssemblyQualifiedName) }),
            .. Members(type, nameof(object.ToString)).Where(static method => method.Parameters.IsEmpty),
        ];
        INamedTypeSymbol text = compilation.GetSpecialType(SpecialType.System_String);
        ISymbol? empty = Declared(compilation, "F:System.String.Empty").FirstOrDefault();
        ImmutableArray<IMethodSymbol> transforms = Members(text, nameof(string.ToUpperInvariant), nameof(string.ToLowerInvariant), nameof(string.Trim), nameof(string.TrimEnd), nameof(string.TrimStart));
        ImmutableArray<IMethodSymbol> emptyTests = Members(text, nameof(string.IsNullOrEmpty), nameof(string.IsNullOrWhiteSpace));
        ImmutableArray<IParameterSymbol> fallbacks = [
            .. Members(option, "IfNone").SelectMany(static method => method.Parameters),
            .. Members(option, "Match").SelectMany(static method => method.Parameters.Where(static parameter => parameter.Type is not INamedTypeSymbol { DelegateInvokeMethod.Parameters.Length: > 0 })),
        ];

        bool Data(INamedTypeSymbol made) => data.GetOrAdd(made, held => held.IsRecord || Containers(held.ContainingType).OfType<INamedTypeSymbol>().Any(owner =>
            owner.GetAttributes().Any(attribute => Same(attribute.AttributeClass, thinktecture.UnionAttribute)
                || (thinktecture.UnionAttributeBase is { } union && Derives(attribute.AttributeClass, union)))));

        bool Rendered(ITypeSymbol made) =>
            (exception is not null && Derives(made, exception)) || (error is not null && Derives(made, error)) || (nomen is not null && Derives(made, nomen));

        bool Failure(IOperation value) => Inside<IThrowOperation>(value) || Ancestor<IArgumentOperation>(value, argument =>
            argument.Parent is IInvocationOperation call && Same(call.TargetMethod.ContainingAssembly, error?.ContainingAssembly)
            && call.TargetMethod.Name is "ToFin" or "ToValidation" or "ToEither" or "Fail" or "fail" or "guard" or "guardnot") is not null;

        bool Empty(IOperation value) => Stripped(value) switch {
            { ConstantValue: { HasValue: true, Value: "" } } => true,
            IFieldReferenceOperation reference => Same(reference.Field, empty),
            _ => false,
        };

        static ISymbol? Compared(IOperation value) => Stripped(value) is IMemberReferenceOperation reference ? reference.Member : null;

        bool Typed(IOperation? receiver) => type is not null && receiver is not null && Derives(Stripped(receiver).Type, type);

        context.RegisterOperationAction(analysis => {
            IOperation value = analysis.Operation;
            bool name = value switch {
                IPropertyReferenceOperation property => Typed(property.Instance) && Overridden(property.Property).Any(member => Among(member, names)),
                IInvocationOperation call => Typed(call.Instance) && Overridden(call.TargetMethod).Any(member => Among(member, names)),
                INameOfOperation => true,
                _ => false,
            };
            if (name) sources.Add(value);
        }, OperationKind.PropertyReference, OperationKind.Invocation, OperationKind.NameOf);

        context.RegisterOperationAction(analysis => {
            IOperation value = analysis.Operation;
            ISymbol? member = value switch {
                IBinaryOperation { OperatorKind: BinaryOperatorKind.Equals or BinaryOperatorKind.NotEquals } binary when Empty(binary.RightOperand) => Compared(binary.LeftOperand),
                IBinaryOperation { OperatorKind: BinaryOperatorKind.Equals or BinaryOperatorKind.NotEquals } binary when Empty(binary.LeftOperand) => Compared(binary.RightOperand),
                IBinaryOperation { OperatorKind: BinaryOperatorKind.Equals or BinaryOperatorKind.NotEquals, LeftOperand: IPropertyReferenceOperation { Property: { Name: nameof(string.Length), ContainingType.SpecialType: SpecialType.System_String }, Instance: { } instance }, RightOperand.ConstantValue: { HasValue: true, Value: 0 } } => Compared(instance),
                IBinaryOperation { OperatorKind: BinaryOperatorKind.Equals or BinaryOperatorKind.NotEquals, RightOperand: IPropertyReferenceOperation { Property: { Name: nameof(string.Length), ContainingType.SpecialType: SpecialType.System_String }, Instance: { } instance }, LeftOperand.ConstantValue: { HasValue: true, Value: 0 } } => Compared(instance),
                IInvocationOperation call when Among(call.TargetMethod, emptyTests) => Compared(call.Arguments[0].Value),
                IIsPatternOperation { Pattern: IConstantPatternOperation constant, Value: var tested } when Empty(constant.Value) => Compared(tested),
                IPropertySubpatternOperation { Pattern: IConstantPatternOperation constant, Member: IMemberReferenceOperation reference } when Empty(constant.Value) => reference.Member,
                _ => null,
            };
            if (member is not null) _ = compared.TryAdd(member.OriginalDefinition, 0);
        }, OperationKind.Binary, OperationKind.Invocation, OperationKind.IsPattern, OperationKind.PropertySubpattern);

        context.RegisterOperationAction(analysis => {
            if (analysis.Operation.Type is INamedTypeSymbol made && Data(made)) records.Add(analysis.Operation);
        }, OperationKind.ObjectCreation, OperationKind.With);

        semantics.Complete((end, models) => {
            ConcurrentDictionary<ISymbol, ImmutableArray<IOperation>> declarations = new(SymbolEqualityComparer.Default);
            ConcurrentDictionary<IOperation, ILookup<ISymbol?, IOperation>> references = [];
            ConcurrentDictionary<IParameterSymbol, ImmutableArray<ISymbol>> members = new(SymbolEqualityComparer.Default);

            static IEnumerable<IOperation> Nodes(IOperation root, CancellationToken token) {
                foreach (IOperation operation in root.DescendantsAndSelf()) {
                    token.ThrowIfCancellationRequested();
                    yield return operation;
                }
            }

            ImmutableArray<IOperation> Operations(ISymbol symbol, CancellationToken token) =>
                declarations.GetOrAdd(symbol.OriginalDefinition, held => [
                    .. held.DeclaringSyntaxReferences.Concat(held is IPropertySymbol { GetMethod: { } getter } ? getter.DeclaringSyntaxReferences : [])
                        .Where(declared => end.Compilation.ContainsSyntaxTree(declared.SyntaxTree))
                        .Select(declared => (Syntax: declared.GetSyntax(token), Model: models[declared.SyntaxTree]))
                        .Select(declared => declared.Model.GetOperation(declared.Syntax switch {
                            VariableDeclaratorSyntax { Initializer.Value: var value } => value,
                            PropertyDeclarationSyntax { Initializer.Value: var value } => value,
                            PropertyDeclarationSyntax { ExpressionBody.Expression: var value } => value,
                            AccessorDeclarationSyntax { ExpressionBody.Expression: var value } => value,
                            AccessorDeclarationSyntax { Body: { } body } => body,
                            var syntax => syntax,
                        }, token)).OfType<IOperation>(),
                ]);

            IEnumerable<ControlFlow.Value> Reads(ISymbol symbol, ControlFlow.Value at, CancellationToken token) {
                ImmutableArray<IOperation> uses = [.. (symbol is IParameterSymbol parameter ? Operations(parameter.ContainingSymbol, token) : [Root(at.Operation)])
                    .SelectMany(root => references.GetOrAdd(root, held => Nodes(held, token).Where(static value => value is IParameterReferenceOperation or ILocalReferenceOperation).ToLookup(Variable, SymbolEqualityComparer.Default))[symbol.OriginalDefinition])];
                bool unchanged = !uses.Any(static used => Written(used) || used.Parent is IVariableInitializerOperation { Parent: IVariableDeclaratorOperation { Symbol.RefKind: not RefKind.None } });
                return uses.Where(static used => !Written(used)).SelectMany(used => flow.Readings(used, token).Where(reading => !reading.Definition.HasValue
                    ? unchanged || (symbol is IParameterSymbol captured && (Ancestor<IOperation>(used, static owner => owner is IAnonymousFunctionOperation or ILocalFunctionOperation) switch {
                        IAnonymousFunctionOperation function => Same(function.Symbol, captured.ContainingSymbol),
                        ILocalFunctionOperation function => Same(function.Symbol, captured.ContainingSymbol),
                        _ => true,
                    }))
                    : symbol is ILocalSymbol && reading.Definition.Value.Originates(at, token)).Select(static reading => reading.Reference));
            }

            INamedTypeSymbol? Stored(IOperation source, CancellationToken token) {
                Stack<ControlFlow.Value> pending = new(flow.Evaluations(source, token));
                HashSet<ControlFlow.Value> visited = [];
                while (pending.Count > 0) {
                    token.ThrowIfCancellationRequested();
                    ControlFlow.Value current = pending.Pop();
                    if (!visited.Add(current)) continue;
                    IOperation value = current.Operation;
                    IOperation? parent = Consumer(value);
                    INamedTypeSymbol? made = parent switch {
                        IArgumentOperation { Parent: IObjectCreationOperation { Type: INamedTypeSymbol created } } => created,
                        ISimpleAssignmentOperation { Target: IMemberReferenceOperation, Parent: IObjectOrCollectionInitializerOperation { Parent.Type: INamedTypeSymbol created } } => created,
                        _ => null,
                    };
                    if (made is not null && !Rendered(made) && !Failure(value)) return made;
                    IEnumerable<ControlFlow.Value> next = parent switch {
                        IVariableInitializerOperation { Parent: IVariableDeclaratorOperation declarator } => Reads(declarator.Symbol, current, token),
                        ISimpleAssignmentOperation { Target: ILocalReferenceOperation local } => Reads(local.Local, current, token),
                        IArgumentOperation { Parameter: { } parameter, Parent: IInvocationOperation call } when !Failure(value) && !Rendered(call.TargetMethod.ReturnType) => Reads(parameter, current.With(call), token),
                        IInvocationOperation { TargetMethod.ReducedFrom: { } method } call when call.Instance == value => Reads(method.Parameters[0], current.With(call), token),
                        IInvocationOperation call when Among(call.TargetMethod, transforms) => [current.With(call)],
                        ICoalesceOperation or IConditionalOperation => [current.With(parent)],
                        IConditionalAccessOperation conditional when conditional.WhenNotNull == value || (conditional.WhenNotNull is IInvocationOperation call && Among(call.TargetMethod, transforms)) => [current.With(conditional)],
                        IInterpolationOperation or IInterpolatedStringOperation => [current.With(parent)],
                        IBinaryOperation { OperatorKind: BinaryOperatorKind.Add, Type.SpecialType: SpecialType.System_String } => [current.With(parent)],
                        _ => [],
                    };
                    foreach (ControlFlow.Value nextValue in next) pending.Push(nextValue);
                }
                return null;
            }

            ImmutableArray<ISymbol> MembersOf(IParameterSymbol parameter, CancellationToken token) =>
                members.GetOrAdd(parameter.OriginalDefinition, held => [
                    .. held.ContainingType.GetMembers().Where(member => member is IFieldSymbol or IPropertySymbol && (
                        member.DeclaringSyntaxReferences.Any(declared => end.Compilation.ContainsSyntaxTree(declared.SyntaxTree)
                            && Same(models[declared.SyntaxTree].GetDeclaredSymbol(declared.GetSyntax(token), token), held))
                        || Operations(member, token).Any(initializer => Stripped(initializer) is IParameterReferenceOperation read && Same(read.Parameter, held)))),
                    .. Operations(held.ContainingSymbol, token).SelectMany(body => Nodes(body, token)).OfType<ISimpleAssignmentOperation>()
                        .Where(assignment => assignment.Target is IMemberReferenceOperation && Stripped(assignment.Value) is IParameterReferenceOperation read && Same(read.Parameter, held))
                        .Select(static assignment => ((IMemberReferenceOperation)assignment.Target).Member),
                ]);

            IOperation? Absent(IOperation source, ISymbol? member, CancellationToken token) {
                Stack<ControlFlow.Value> pending = new([ControlFlow.At(source)]);
                HashSet<ControlFlow.Value> visited = [];
                bool checkedEmpty = member is IParameterSymbol parameter
                    ? MembersOf(parameter, token).Any(held => compared.ContainsKey(held.OriginalDefinition))
                    : member is not null && compared.ContainsKey(member.OriginalDefinition);
                while (pending.Count > 0) {
                    token.ThrowIfCancellationRequested();
                    ControlFlow.Value current = pending.Pop();
                    if (!visited.Add(current)) continue;
                    IOperation value = Stripped(current.Operation);
                    if ((Empty(value) && checkedEmpty) || (value switch {
                        IMemberReferenceOperation reference => Among(reference.Member, missingValues)
                            || (Same(reference.Member.ContainingAssembly, rhinoMath?.ContainingAssembly) && reference.Member.ContainingType is { } owner && Among(reference.Member,
                                unsets.GetOrAdd(owner, static native => [.. native.GetMembers().Where(static held => held.IsStatic && held.Name.StartsWith("Unset", StringComparison.Ordinal))]))),
                        IDefaultValueOperation or IObjectCreationOperation { Arguments.IsEmpty: true, Initializer: null } => Among(value.Type, missingTypes),
                        _ => false,
                    })) return value;
                    IEnumerable<ControlFlow.Value> next = value switch {
                        ILocalReferenceOperation or IFlowCaptureReferenceOperation => flow.Values(current.With(value), token)
                            .Where(static definition => definition.HasValue).Select(static definition => definition.Value),
                        IMemberReferenceOperation reference => Operations(reference.Member, token).Select(ControlFlow.At)
                            .Concat(reference.Instance is { } instance ? [current.With(instance)] : []),
                        ICoalesceOperation coalesce => [current.With(coalesce.WhenNull)],
                        IConditionalOperation { WhenFalse: { } alternative } conditional => [current.With(conditional.WhenTrue), current.With(alternative)],
                        ISwitchExpressionOperation expression => expression.Arms.Select(arm => current.With(arm.Value)),
                        IInvocationOperation call => call.Arguments.Where(argument => Among(argument.Parameter, fallbacks))
                            .Select(argument => Lambda(argument.Value) is { } function ? ControlFlow.At(function.Body) : current.With(argument.Value)),
                        IBlockOperation body when Sole(body) is { } returned => [current.With(returned)],
                        _ => [],
                    };
                    foreach (ControlFlow.Value nextValue in next) pending.Push(nextValue);
                }
                return null;
            }

            foreach (IOperation source in sources) {
                if (source is INameOfOperation named && models[named.Syntax.SyntaxTree].GetSymbolInfo(named.Argument.Syntax, end.CancellationToken).Symbol is not ITypeParameterSymbol) continue;
                if (Stored(source, end.CancellationToken) is { } made)
                    end.ReportDiagnostic(Diagnostic.Create(TypeName, source.Syntax.GetLocation(), source.Syntax, made.Name));
            }
            foreach (IOperation site in records) {
                IEnumerable<(IOperation Value, ISymbol? Member)> values =
                    site.ChildOperations.OfType<IArgumentOperation>().Where(static argument => !argument.IsImplicit).Select(static argument => (argument.Value, (ISymbol?)argument.Parameter))
                        .Concat(site.ChildOperations.OfType<IObjectOrCollectionInitializerOperation>().SelectMany(static initializer => initializer.Initializers)
                            .OfType<ISimpleAssignmentOperation>().Where(static assignment => assignment.Target is IMemberReferenceOperation)
                            .Select(static assignment => (assignment.Value, (ISymbol?)((IMemberReferenceOperation)assignment.Target).Member)));
                if (values.Select(value => Absent(value.Value, value.Member, end.CancellationToken)).OfType<IOperation>().FirstOrDefault() is { } absent)
                    end.ReportDiagnostic(Diagnostic.Create(Sentinel, site.Syntax.GetLocation(), site.Type!.Name, absent.Syntax));
            }
        });
    }
}
