using System.Collections.Concurrent;
using System.Collections.Immutable;
using System.Reflection;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Operations;
using Microsoft.CodeAnalysis.Text;

namespace Rasm.Analyzers;

// --- [CONSTANTS] -----------------------------------------------------------------------

internal static class Categories {
    internal const string LanguageExt = "LanguageExt";

    internal const string Thinktecture = "Thinktecture";

    internal const string Logging = "Microsoft.Extensions.Logging";

    internal const string RhinoCommon = "RhinoCommon";

    internal const string Eto = "Eto";

    internal const string CSharp = "CSharp";
}

// --- [OPERATIONS] ----------------------------------------------------------------------

internal static class Semantics {

    // --- [REPORTS]

    internal static readonly SymbolDisplayFormat Shown = SymbolDisplayFormat.MinimallyQualifiedFormat.AddMiscellaneousOptions(SymbolDisplayMiscellaneousOptions.IncludeNullableReferenceTypeModifier);

    private static readonly string Help = typeof(Semantics).Assembly.GetCustomAttributes<AssemblyMetadataAttribute>().Single(static held => held.Key is nameof(Help)).Value;

    internal static string Link(string id) => $"{Help}#{id}";

    internal static ImmutableArray<string> Option(AnalyzerOptions options, string key) =>
        options.AnalyzerConfigOptionsProvider.GlobalOptions.TryGetValue(key, out string? value) ? [.. value.Split(['|'], StringSplitOptions.RemoveEmptyEntries).Select(static held => held.Trim())] : [];

    internal static void Report<T>(CompilationStartAnalysisContext context, ImmutableArray<OperationKind> kinds, Func<T, OperationAnalysisContext, IEnumerable<Diagnostic>> found) where T : IOperation =>
        context.RegisterOperationAction(
            analysis => {
                foreach (Diagnostic diagnostic in found((T)analysis.Operation, analysis)) {
                    analysis.CancellationToken.ThrowIfCancellationRequested();
                    analysis.ReportDiagnostic(diagnostic);
                }
            },
            kinds);

    internal static void Report<T>(CompilationStartAnalysisContext context, ImmutableArray<OperationKind> kinds, DiagnosticDescriptor rule, Func<T, OperationAnalysisContext, bool> found) where T : IOperation =>
        Report<T>(context, kinds, (operation, analysis) => found(operation, analysis) ? [Diagnostic.Create(rule, operation.Syntax.GetLocation(), operation.Syntax)] : []);

    internal static void Writes(CompilationStartAnalysisContext context, ISymbol? written, DiagnosticDescriptor rule, Func<IAssignmentOperation, IOperation?, OperationAnalysisContext, bool> wrong) {
        if (written is null) return;
        bool Found(IAssignmentOperation assignment, OperationAnalysisContext analysis) {
            if (assignment.Target is not ITupleOperation)
                return assignment.Target is IPropertyReferenceOperation direct && Same(direct.Property, written) && wrong(assignment, assignment.Value, analysis);
            Queue<(IOperation Target, IOperation? Value)> pending = new([(assignment.Target, assignment.Value)]);
            while (pending.Count is > 0) {
                analysis.CancellationToken.ThrowIfCancellationRequested();
                (IOperation target, IOperation? value) = pending.Dequeue();
                if (target is IPropertyReferenceOperation reference && Same(reference.Property, written) && wrong(assignment, value, analysis)) return true;
                if (target is not ITupleOperation tuple) continue;
                ImmutableArray<IOperation> values = value is not null && Stripped(value) is ITupleOperation supplied ? supplied.Elements : [];
                for (int index = 0; index < tuple.Elements.Length; index++)
                    pending.Enqueue((tuple.Elements[index], index < values.Length ? values[index] : null));
            }
            return false;
        }
        Report<IAssignmentOperation>(context, [OperationKind.SimpleAssignment, OperationKind.CompoundAssignment, OperationKind.CoalesceAssignment, OperationKind.DeconstructionAssignment], rule, Found);
    }

    internal static Location Named(IInvocationOperation call) =>
        call.Syntax is InvocationExpressionSyntax { Expression: MemberAccessExpressionSyntax access } syntax
            ? Location.Create(syntax.SyntaxTree, TextSpan.FromBounds(access.Name.SpanStart, syntax.Span.End))
            : call.Syntax.GetLocation();

    // --- [SYMBOLS]

    internal static ImmutableArray<ISymbol> Declared(Compilation compilation, params string[] ids) =>
        [.. ids.SelectMany(id => DocumentationCommentId.GetSymbolsForDeclarationId(id, compilation))];

    internal static ImmutableArray<IMethodSymbol> Members(INamedTypeSymbol? owner, params string[] names) =>
        [.. names.SelectMany(name => owner?.GetMembers(name) ?? []).OfType<IMethodSymbol>()];

    internal static bool Same(ISymbol? symbol, ISymbol? other) =>
        symbol is not null && other is not null && SymbolEqualityComparer.Default.Equals(Declaration(symbol), Declaration(other));

    internal static bool Among<T>(ISymbol? symbol, ImmutableArray<T> members) where T : class, ISymbol {
        if (symbol is null || members.IsEmpty) return false;
        ISymbol declaration = Declaration(symbol);
        foreach (T member in members)
            if (SymbolEqualityComparer.Default.Equals(declaration, Declaration(member))) return true;
        return false;
    }

    internal static IEnumerable<ISymbol> Containers(ISymbol? symbol) {
        for (ISymbol? held = symbol; held is not null; held = held.ContainingSymbol) yield return held;
    }

    internal static IEnumerable<ITypeSymbol> Bases(ITypeSymbol? type) {
        for (ITypeSymbol? held = type; held is not null; held = held.BaseType) yield return held;
    }

    internal static bool Within(ISymbol? symbol, ISymbol? owner) => owner is not null && Containers(symbol).Any(held => Same(held, owner));

    internal static bool Namespaced(Compilation compilation, ISymbol symbol, ISymbol? space) =>
        symbol.ContainingNamespace is { } held && Within(compilation.GetCompilationNamespace(held), space);

    internal static ISymbol Enclosing(ISymbol symbol) =>
        Containers(symbol).First(static held => held is not IMethodSymbol { MethodKind: MethodKind.AnonymousFunction or MethodKind.LocalFunction });

    internal static bool Derives(ITypeSymbol? type, INamedTypeSymbol root) => Bases(type).Any(held => Same(held, root));

    internal static IEnumerable<INamedTypeSymbol> Faces(ITypeSymbol type, ISymbol? face) => type.AllInterfaces.Where(held => Same(held, face));

    internal static AttributeData? Marked(ISymbol symbol, INamedTypeSymbol? attribute) => symbol.GetAttributes().FirstOrDefault(data => attribute is not null && Same(data.AttributeClass, attribute));

    internal static AttributeData? Marked(ISymbol symbol, ImmutableArray<INamedTypeSymbol> attributes) => symbol.GetAttributes().FirstOrDefault(data => Among(data.AttributeClass, attributes));

    internal static bool Sourced(ISymbol symbol) => symbol.Locations.Any(static location => location.IsInSource);

    internal static bool Mentions(ITypeSymbol type, ITypeParameterSymbol held) =>
        SymbolEqualityComparer.Default.Equals(type, held)
        || (type is INamedTypeSymbol { TypeArguments: var arguments } && arguments.Any(argument => Mentions(argument, held)))
        || (type is IArrayTypeSymbol array && Mentions(array.ElementType, held));

    internal static bool Delegated(IMethodSymbol method) => method.Parameters.Any(static parameter => parameter.Type.TypeKind is TypeKind.Delegate);

    internal static INamedTypeSymbol? Enumerated(ITypeSymbol type) =>
        type.AllInterfaces.Append(type).OfType<INamedTypeSymbol>().FirstOrDefault(static face => face.OriginalDefinition.SpecialType is SpecialType.System_Collections_Generic_IEnumerable_T);

    internal static bool Implements(IMethodSymbol method, ISymbol member) => Same(method, member) || Same(method.ContainingType?.FindImplementationForInterfaceMember(member), method);

    internal static ISymbol Member(ISymbol symbol) => symbol is IMethodSymbol { AssociatedSymbol: { } associated } ? associated : symbol;

    internal static IEnumerable<ISymbol> Overridden(ISymbol? member) {
        for (ISymbol? held = member; held is not null; held = held switch {
            IMethodSymbol method => method.OverriddenMethod,
            IPropertySymbol property => property.OverriddenProperty,
            IEventSymbol notification => notification.OverriddenEvent,
            _ => null,
        }) yield return held;
    }

    internal static string Label(IMethodSymbol method) =>
        method.MethodKind is MethodKind.Constructor ? $"new {method.ContainingType.Name}" : $"{method.ContainingType.Name}.{method.Name}";

    internal static string Label(IInvocationOperation call) =>
        call.TargetMethod.MethodKind is MethodKind.DelegateInvoke && call.Instance is { } handed ? handed.Syntax.ToString() : Label(call.TargetMethod);

    private static ISymbol Declaration(ISymbol value) =>
        (value is IMethodSymbol method ? method.AssociatedExtensionImplementation ?? method.ReducedFrom ?? method : value).OriginalDefinition;

    // --- [OPERATIONS]

    internal static T? Ancestor<T>(IOperation operation, Func<T, bool> wanted) where T : class, IOperation {
        for (IOperation? parent = operation.Parent; parent is not null; parent = parent.Parent)
            if (parent is T held && wanted(held)) return held;
        return null;
    }

    internal static bool Inside<T>(IOperation operation) where T : class, IOperation => Ancestor<T>(operation, static _ => true) is not null;

    internal static IOperation? Consumer(IOperation operation) {
        IOperation? parent = operation.Parent;
        while (parent is IConversionOperation { IsImplicit: true, OperatorMethod: null } conversion) parent = conversion.Parent;
        return parent;
    }

    internal static IOperation Stripped(IOperation operation) {
        while (operation is IConversionOperation { IsImplicit: true, OperatorMethod: null } conversion) operation = conversion.Operand;
        return operation;
    }

    internal static IAnonymousFunctionOperation? Lambda(IOperation value) => Stripped(value) is IDelegateCreationOperation { Target: IAnonymousFunctionOperation function } ? function : null;

    internal static ISymbol? Variable(IOperation reference) =>
        reference switch {
            IParameterReferenceOperation parameter => parameter.Parameter,
            ILocalReferenceOperation local => local.Local,
            _ => null,
        };

    internal static ImmutableArray<IOperation> Steps(IBlockOperation body) =>
        [.. body.Operations.Where(static step => step is not IReturnOperation { ReturnedValue: null, IsImplicit: true }).Select(static step => step switch {
            IReturnOperation { ReturnedValue: { } returned } => Stripped(returned),
            IExpressionStatementOperation statement => Stripped(statement.Operation),
            _ => step,
        })];

    internal static IOperation? Sole(IBlockOperation body) => Steps(body) is { Length: 1 } steps ? steps[0] : null;

    internal static IOperation? Receiver(IInvocationOperation call) =>
        call.Instance ?? call.Arguments.FirstOrDefault(static argument => argument.Parameter?.Ordinal is 0)?.Value;

    internal static IOperation? Expressed(IOperation block) =>
        block switch {
            IFieldInitializerOperation { Value: var value } => value,
            IMethodBodyOperation { BlockBody: { } body } => Expressed(body),
            IMethodBodyOperation { BlockBody: null, ExpressionBody: { } body } => Expressed(body),
            IBlockOperation { Operations: { Length: 1 } held } when held[0] is IReturnOperation { ReturnedValue: { } value } => value,
            IBlockOperation { Operations: { IsEmpty: false } held } when held[0] is IExpressionStatementOperation { Operation: var value } && held.Skip(1).All(static rest => rest.IsImplicit) => value,
            _ => null,
        };

    internal static bool Parameterless(IInvocationOperation call) => (call.Instance is not null || call.TargetMethod.IsExtensionMethod) && call.Arguments.Length == (call.Instance is null ? 1 : 0);

    internal static IOperation Root(IOperation operation) {
        while (operation.Parent is { } parent) operation = parent;
        return operation;
    }

    internal static bool Written(IOperation reference) {
        while (reference.Parent is ITupleOperation tuple) reference = tuple;
        return reference.Parent switch {
            IAssignmentOperation assignment => assignment.Target == reference,
            IIncrementOrDecrementOperation or IArgumentOperation { Parameter.RefKind: RefKind.Ref or RefKind.Out } => true,
            _ => false,
        };
    }

    internal static ISymbol? Target(IOperation operation) =>
        operation switch {
            IInvocationOperation call => call.TargetMethod,
            IMemberReferenceOperation reference => reference.Member,
            _ => null,
        };

    internal static IEnumerable<IOperation> Declarations(ISymbol symbol, Reading reading) =>
        symbol.DeclaringSyntaxReferences.Where(declared => reading.Model.Compilation.ContainsSyntaxTree(declared.SyntaxTree)).Select(declared =>
            reading.For(declared.SyntaxTree)?.GetOperation(declared.GetSyntax(reading.Token), reading.Token)).OfType<IOperation>();

    internal static IEnumerable<IMethodSymbol> Calls(IInvocationOperation call, Reading reading, ControlFlow flow, ImmutableArray<IMethodSymbol> callbacks) {
        IEnumerable<IMethodSymbol> invoked = call is { TargetMethod.MethodKind: MethodKind.DelegateInvoke, Instance: { } instance }
            ? flow.Targets(ControlFlow.At(instance), reading.Token).OfType<IMethodSymbol>() : [call.TargetMethod];
        return invoked.Concat(call.Arguments.Where(argument => argument.Parameter is { Type.TypeKind: TypeKind.Delegate } parameter
            && flow.Invoked(parameter, method => Declarations(method, reading), native => native.Type.TypeKind is TypeKind.Delegate && Among(native.ContainingSymbol, callbacks), reading.Token))
            .SelectMany(argument => flow.Targets(ControlFlow.At(argument.Value), reading.Token).OfType<IMethodSymbol>()));
    }

    internal static IEnumerable<IOperation> References(ISymbol symbol, IOperation within, Reading reading) {
        IOperation root = Root(within);
        if (symbol is IRangeVariableSymbol) {
            if (!reading.Ranges.TryGetValue(root, out ILookup<ISymbol?, IOperation>? ranges)) {
                SemanticModel model = root.SemanticModel!;
                ranges = root.Syntax.DescendantNodes().OfType<IdentifierNameSyntax>().Select(name => {
                    reading.Token.ThrowIfCancellationRequested();
                    return (name, model.GetSymbolInfo(name, reading.Token).Symbol);
                }).Where(static reference => reference.Symbol is IRangeVariableSymbol)
                    .Select(reference => (reference.Symbol, Operation: model.GetOperation(reference.name, reading.Token)))
                    .Where(static reference => reference.Operation is not null)
                    .ToLookup(static reference => reference.Symbol, static reference => reference.Operation!, SymbolEqualityComparer.Default);
                reading.Ranges.Add(root, ranges);
            }
            return ranges[symbol];
        }
        if (!reading.References.TryGetValue(root, out ILookup<ISymbol?, IOperation>? references)) {
            references = root.DescendantsAndSelf().Where(operation => {
                reading.Token.ThrowIfCancellationRequested();
                return operation is ILocalReferenceOperation or IParameterReferenceOperation && operation.Syntax is IdentifierNameSyntax;
            }).ToLookup(Variable, SymbolEqualityComparer.Default);
            reading.References.Add(root, references);
        }
        return references[symbol];
    }

    internal static QueryClauseSyntax? Clause(SyntaxNode syntax) {
        while (syntax.Parent is ParenthesizedExpressionSyntax parenthesized) syntax = parenthesized;
        return syntax.Parent switch {
            FromClauseSyntax from when from.Expression == syntax => from,
            LetClauseSyntax let when let.Expression == syntax => let,
            _ => null,
        };
    }

    internal static string Key(AttributeData mark) {
        bool Selected(string option, string member) => mark.NamedArguments.FirstOrDefault(named => string.Equals(named.Key, option, StringComparison.Ordinal)).Value is { Type: INamedTypeSymbol type, Value: { } value }
            && type.GetMembers(member).OfType<IFieldSymbol>().Any(field => Equals(field.ConstantValue, value));
        return mark.NamedArguments.Where(static named => named.Key is "KeyMemberName").Select(static named => named.Value.Value).OfType<string>().FirstOrDefault()
            ?? (Selected("KeyMemberAccessModifier", "Private") && Selected("KeyMemberKind", "Field") ? "_key" : "Key");
    }

    internal static (IOperation Subject, ImmutableArray<(IOperation Arm, IOperation? Test, IOperation? Guard)> Arms)? Dispatch(IOperation operation) =>
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
}

// --- [SERVICES] ------------------------------------------------------------------------

internal sealed class Reading(SemanticModel model, ImmutableDictionary<SyntaxTree, SemanticModel> models, CancellationToken token) {
    public SemanticModel Model { get; } = model;

    public CancellationToken Token { get; } = token;

    public Dictionary<IOperation, ILookup<ISymbol?, IOperation>> References { get; } = [];

    public Dictionary<IOperation, ILookup<ISymbol?, IOperation>> Ranges { get; } = [];

    public SemanticModel? For(SyntaxTree tree) => tree == Model.SyntaxTree ? Model : models.TryGetValue(tree, out SemanticModel? held) ? held : null;
}

internal sealed class SemanticModels {
    private readonly CompilationStartAnalysisContext context;
    private readonly List<Action<CompilationAnalysisContext, ImmutableDictionary<SyntaxTree, SemanticModel>>> completed = [];
    private readonly ConcurrentDictionary<SemanticModel, Reading> readings = [];

    public SemanticModels(CompilationStartAnalysisContext context) {
        this.context = context;
        ConcurrentDictionary<SyntaxTree, SemanticModel> models = [];
        context.RegisterSemanticModelAction(analysis => models[analysis.SemanticModel.SyntaxTree] = analysis.SemanticModel);
        context.RegisterCompilationEndAction(end => {
            ImmutableDictionary<SyntaxTree, SemanticModel> snapshot = models.ToImmutableDictionary();
            foreach (Action<CompilationAnalysisContext, ImmutableDictionary<SyntaxTree, SemanticModel>> action in completed) action(end, snapshot);
        });
    }

    public void Complete(Action<CompilationAnalysisContext, ImmutableDictionary<SyntaxTree, SemanticModel>> action) => completed.Add(action);

    public void Report<T>(ImmutableArray<OperationKind> kinds, Func<T, ISymbol, Reading, IEnumerable<Diagnostic>> found) where T : IOperation {
        ConcurrentBag<(T Operation, ISymbol Owner)> operations = [];
        context.RegisterOperationAction(analysis => operations.Add(((T)analysis.Operation, analysis.ContainingSymbol)), kinds);
        Complete((end, models) => {
            Func<SemanticModel, Reading> read = model => new Reading(model, models, end.CancellationToken);
            foreach ((T operation, ISymbol owner) in operations)
                foreach (Diagnostic diagnostic in found(operation, owner, readings.GetOrAdd(operation.SemanticModel!, read))) {
                    end.CancellationToken.ThrowIfCancellationRequested();
                    end.ReportDiagnostic(diagnostic);
                }
        });
    }
}
