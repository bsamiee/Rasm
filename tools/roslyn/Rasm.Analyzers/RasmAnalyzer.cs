using System.Collections.Concurrent;
using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Operations;
using Microsoft.CodeAnalysis.Text;

namespace Rasm.Analyzers;

// --- [OPERATIONS] ----------------------------------------------------------------------
public static class Scopes {
    public static IEnumerable<string> Declared(SyntaxNode node) =>
        node.DescendantNodesAndSelf().Select(static declared => declared switch {
            ParameterSyntax parameter => parameter.Identifier,
            VariableDeclaratorSyntax variable => variable.Identifier,
            SingleVariableDesignationSyntax designation => designation.Identifier,
            LocalFunctionStatementSyntax local => local.Identifier,
            ForEachStatementSyntax each => each.Identifier,
            CatchDeclarationSyntax caught => caught.Identifier,
            FromClauseSyntax range => range.Identifier,
            LetClauseSyntax range => range.Identifier,
            JoinClauseSyntax range => range.Identifier,
            JoinIntoClauseSyntax range => range.Identifier,
            QueryContinuationSyntax range => range.Identifier,
            TypeParameterSyntax parameter => parameter.Identifier,
            _ => default,
        }).Where(static token => !token.IsKind(SyntaxKind.None)).Select(static token => token.ValueText);

    public static IEnumerable<IdentifierNameSyntax> Referenced(SyntaxNode node) {
        ImmutableHashSet<string> own = [.. Declared(node)];
        return node.DescendantNodesAndSelf().OfType<IdentifierNameSyntax>().Where(used => !own.Contains(used.Identifier.ValueText)
            && used.Parent switch { MemberAccessExpressionSyntax access => access.Name != used, MemberBindingExpressionSyntax or NameColonSyntax or NameEqualsSyntax => false, _ => true });
    }
}

// --- [COMPOSITION] ---------------------------------------------------------------------
public abstract class RasmAnalyzer : DiagnosticAnalyzer {
    public sealed override void Initialize(AnalysisContext context) {
        context.EnableConcurrentExecution();
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.Analyze);
        context.RegisterCompilationStartAction(Start);
    }

    protected abstract void Start(CompilationStartAnalysisContext context);

    // --- [REPORTS]
    protected static DiagnosticDescriptor Rule(string id, string category, string title, string message, string description, params string[] tags) =>
        new(id, title, message, category, DiagnosticSeverity.Error, isEnabledByDefault: true, description, customTags: tags);

    protected static void Report<T>(CompilationStartAnalysisContext context, ImmutableArray<OperationKind> kinds, Func<T, OperationAnalysisContext, IEnumerable<Diagnostic>> found) where T : IOperation =>
        context.RegisterOperationAction(
            analysis => {
                if (analysis.IsGeneratedCode) return;
                foreach (Diagnostic diagnostic in found((T)analysis.Operation, analysis)) {
                    analysis.CancellationToken.ThrowIfCancellationRequested();
                    analysis.ReportDiagnostic(diagnostic);
                }
            },
            kinds);

    protected static void Report<T>(CompilationStartAnalysisContext context, ImmutableArray<OperationKind> kinds, DiagnosticDescriptor rule, Func<T, OperationAnalysisContext, bool> found) where T : IOperation =>
        Report<T>(context, kinds, (operation, analysis) => found(operation, analysis) ? [Diagnostic.Create(rule, operation.Syntax.GetLocation(), operation.Syntax)] : []);

    protected static void Writes(CompilationStartAnalysisContext context, ISymbol? written, DiagnosticDescriptor rule, Func<IAssignmentOperation, IOperation?, OperationAnalysisContext, bool> wrong) {
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

    protected static Location Named(IInvocationOperation call) =>
        call.Syntax is InvocationExpressionSyntax { Expression: MemberAccessExpressionSyntax access } syntax
            ? Location.Create(syntax.SyntaxTree, TextSpan.FromBounds(access.Name.SpanStart, syntax.Span.End))
            : call.Syntax.GetLocation();

    // --- [SYMBOLS]
    protected static ImmutableArray<ISymbol> Declared(Compilation compilation, params string[] ids) =>
        [.. ids.SelectMany(id => DocumentationCommentId.GetSymbolsForDeclarationId(id, compilation))];

    protected static ImmutableArray<IMethodSymbol> Members(INamedTypeSymbol? owner, params string[] names) =>
        [.. names.SelectMany(name => owner?.GetMembers(name) ?? []).OfType<IMethodSymbol>()];

    protected static bool Same(ISymbol? symbol, ISymbol? other) =>
        symbol is not null && other is not null && SymbolEqualityComparer.Default.Equals(Declaration(symbol), Declaration(other));

    protected static bool Among<T>(ISymbol? symbol, ImmutableArray<T> members) where T : class, ISymbol {
        if (symbol is null || members.IsEmpty) return false;
        ISymbol declaration = Declaration(symbol);
        foreach (T member in members)
            if (SymbolEqualityComparer.Default.Equals(declaration, Declaration(member))) return true;
        return false;
    }

    protected static IEnumerable<ISymbol> Containers(ISymbol? symbol) {
        for (ISymbol? held = symbol; held is not null; held = held.ContainingSymbol) yield return held;
    }

    protected static IEnumerable<ITypeSymbol> Bases(ITypeSymbol? type) {
        for (ITypeSymbol? held = type; held is not null; held = held.BaseType) yield return held;
    }

    protected static bool Within(ISymbol? symbol, ISymbol? owner) => owner is not null && Containers(symbol).Any(held => Same(held, owner));

    protected static bool Namespaced(Compilation compilation, ISymbol symbol, ISymbol? space) =>
        symbol.ContainingNamespace is { } held && Within(compilation.GetCompilationNamespace(held), space);

    protected static ISymbol Enclosing(ISymbol symbol) =>
        Containers(symbol).First(static held => held is not IMethodSymbol { MethodKind: MethodKind.AnonymousFunction or MethodKind.LocalFunction });

    protected static bool Derives(ITypeSymbol? type, INamedTypeSymbol root) => Bases(type).Any(held => Same(held, root));

    protected static IEnumerable<INamedTypeSymbol> Faces(ITypeSymbol type, ISymbol? face) => type.AllInterfaces.Where(held => Same(held, face));

    protected static AttributeData? Marked(ISymbol symbol, INamedTypeSymbol? attribute) => symbol.GetAttributes().FirstOrDefault(data => attribute is not null && Same(data.AttributeClass, attribute));

    protected static AttributeData? Marked(ISymbol symbol, ImmutableArray<INamedTypeSymbol> attributes) => symbol.GetAttributes().FirstOrDefault(data => Among(data.AttributeClass, attributes));

    protected static bool Sourced(ISymbol symbol) => symbol.Locations.Any(static location => location.IsInSource);

    protected static bool Delegated(IMethodSymbol method) => method.Parameters.Any(static parameter => parameter.Type.TypeKind is TypeKind.Delegate);

    protected static INamedTypeSymbol? Enumerated(ITypeSymbol type) =>
        type.AllInterfaces.Append(type).OfType<INamedTypeSymbol>().FirstOrDefault(static face => face.OriginalDefinition.SpecialType is SpecialType.System_Collections_Generic_IEnumerable_T);

    protected static bool Implements(IMethodSymbol method, ISymbol member) => Same(method, member) || Same(method.ContainingType?.FindImplementationForInterfaceMember(member), method);

    protected static ISymbol Member(ISymbol symbol) => symbol is IMethodSymbol { AssociatedSymbol: { } associated } ? associated : symbol;

    protected static IEnumerable<ISymbol> Overridden(ISymbol? member) {
        for (ISymbol? held = member; held is not null; held = held switch {
            IMethodSymbol method => method.OverriddenMethod,
            IPropertySymbol property => property.OverriddenProperty,
            IEventSymbol notification => notification.OverriddenEvent,
            _ => null,
        }) yield return held;
    }

    protected static string Label(IMethodSymbol method) =>
        method.MethodKind is MethodKind.Constructor ? $"new {method.ContainingType.Name}" : $"{method.ContainingType.Name}.{method.Name}";

    protected static string Label(IInvocationOperation call) =>
        call.TargetMethod.MethodKind is MethodKind.DelegateInvoke && call.Instance is { } handed ? handed.Syntax.ToString() : Label(call.TargetMethod);

    private static ISymbol Declaration(ISymbol value) =>
        (value is IMethodSymbol method ? method.AssociatedExtensionImplementation ?? method.ReducedFrom ?? method : value).OriginalDefinition;

    // --- [OPERATIONS]
    protected static T? Ancestor<T>(IOperation operation, Func<T, bool> wanted) where T : class, IOperation {
        for (IOperation? parent = operation.Parent; parent is not null; parent = parent.Parent)
            if (parent is T held && wanted(held)) return held;
        return null;
    }

    protected static bool Inside<T>(IOperation operation) where T : class, IOperation => Ancestor<T>(operation, static _ => true) is not null;

    protected static IOperation? Consumer(IOperation operation) {
        IOperation? parent = operation.Parent;
        while (parent is IConversionOperation { IsImplicit: true, OperatorMethod: null } conversion) parent = conversion.Parent;
        return parent;
    }

    protected static IOperation Stripped(IOperation operation) {
        while (operation is IConversionOperation { IsImplicit: true, OperatorMethod: null } conversion) operation = conversion.Operand;
        return operation;
    }

    protected static IAnonymousFunctionOperation? Lambda(IOperation value) => Stripped(value) is IDelegateCreationOperation { Target: IAnonymousFunctionOperation function } ? function : null;

    protected internal static ISymbol? Variable(IOperation reference) =>
        reference switch {
            IParameterReferenceOperation parameter => parameter.Parameter,
            ILocalReferenceOperation local => local.Local,
            _ => null,
        };

    protected static ImmutableArray<IOperation> Steps(IBlockOperation body) =>
        [.. body.Operations.Where(static step => step is not IReturnOperation { ReturnedValue: null, IsImplicit: true }).Select(static step => step switch {
            IReturnOperation { ReturnedValue: { } returned } => Stripped(returned),
            IExpressionStatementOperation statement => Stripped(statement.Operation),
            _ => step,
        })];

    protected static IOperation? Sole(IBlockOperation body) => Steps(body) is { Length: 1 } steps ? steps[0] : null;

    protected static IOperation? Receiver(IInvocationOperation call) =>
        call.Instance ?? call.Arguments.FirstOrDefault(static argument => argument.Parameter?.Ordinal is 0)?.Value;

    protected static IOperation Root(IOperation operation) {
        while (operation.Parent is { } parent) operation = parent;
        return operation;
    }

    protected static bool Written(IOperation reference) {
        while (reference.Parent is ITupleOperation tuple) reference = tuple;
        return reference.Parent switch {
            IAssignmentOperation assignment => assignment.Target == reference,
            IIncrementOrDecrementOperation or IArgumentOperation { Parameter.RefKind: RefKind.Ref or RefKind.Out } => true,
            _ => false,
        };
    }

    protected static ISymbol? Target(IOperation operation) =>
        operation switch {
            IInvocationOperation call => call.TargetMethod,
            IMemberReferenceOperation reference => reference.Member,
            _ => null,
        };

    private protected static IEnumerable<IOperation> Declarations(ISymbol symbol, Reading reading) =>
        symbol.DeclaringSyntaxReferences.Where(declared => reading.Model.Compilation.ContainsSyntaxTree(declared.SyntaxTree)).Select(declared =>
            reading.For(declared.SyntaxTree).GetOperation(declared.GetSyntax(reading.Token), reading.Token)).OfType<IOperation>();

    private protected static IEnumerable<IMethodSymbol> Calls(IInvocationOperation call, Reading reading, ControlFlow flow, ImmutableArray<IMethodSymbol> callbacks) {
        IEnumerable<IMethodSymbol> invoked = call is { TargetMethod.MethodKind: MethodKind.DelegateInvoke, Instance: { } instance }
            ? flow.Targets(ControlFlow.At(instance), reading.Token).OfType<IMethodSymbol>() : [call.TargetMethod];
        return invoked.Concat(call.Arguments.Where(argument => argument.Parameter is { Type.TypeKind: TypeKind.Delegate } parameter
            && flow.Invoked(parameter, method => Declarations(method, reading), native => native.Type.TypeKind is TypeKind.Delegate && Among(native.ContainingSymbol, callbacks), reading.Token))
            .SelectMany(argument => flow.Targets(ControlFlow.At(argument.Value), reading.Token).OfType<IMethodSymbol>()));
    }

    private protected static IEnumerable<IOperation> References(ISymbol symbol, IOperation within, Reading reading) {
        IOperation root = Root(within);
        if (symbol is IRangeVariableSymbol) {
            if (!reading.Ranges.TryGetValue(root, out ILookup<ISymbol?, IOperation>? ranges)) {
                SemanticModel model = reading.For(root.Syntax.SyntaxTree);
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

    private protected static QueryClauseSyntax? Clause(SyntaxNode syntax) {
        while (syntax.Parent is ParenthesizedExpressionSyntax parenthesized) syntax = parenthesized;
        return syntax.Parent switch {
            FromClauseSyntax from when from.Expression == syntax => from,
            LetClauseSyntax let when let.Expression == syntax => let,
            _ => null,
        };
    }

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

    protected static bool CatchesAll((IOperation Arm, IOperation? Test, IOperation? Guard) arm, CancellationToken cancellation) =>
        arm is { Guard: null, Test: null or IPatternOperation } && (arm.Test is not IPatternOperation pattern || Open(pattern, cancellation));

    private static bool Open(IPatternOperation pattern, CancellationToken cancellation) {
        static bool Whole(ITypeSymbol? matched, IPatternOperation tested) =>
            matched is null or { SpecialType: SpecialType.System_Object } || SymbolEqualityComparer.Default.Equals(matched, tested.InputType);
        static bool Leaf(IPatternOperation tested) => tested switch {
            IDiscardPatternOperation => true,
            IDeclarationPatternOperation declaration => Whole(declaration.MatchedType, declaration),
            ITypePatternOperation type => Whole(type.MatchedType, type),
            INegatedPatternOperation { Pattern: IConstantPatternOperation { Value.ConstantValue: { HasValue: true, Value: null } } } => true,
            _ => false,
        };
        if (pattern is not (IBinaryPatternOperation or IRecursivePatternOperation)) return Leaf(pattern);
        Dictionary<IPatternOperation, bool> results = [];
        Stack<(IPatternOperation Pattern, bool Expanded)> pending = new([(pattern, false)]);
        while (pending.Count is > 0) {
            cancellation.ThrowIfCancellationRequested();
            (IPatternOperation held, bool expanded) = pending.Pop();
            switch (held) {
                case IBinaryPatternOperation binary when !expanded:
                    pending.Push((held, true));
                    pending.Push((binary.RightPattern, false));
                    pending.Push((binary.LeftPattern, false));
                    break;
                case IBinaryPatternOperation binary:
                    results[held] = binary.OperatorKind is BinaryOperatorKind.Or
                        ? results[binary.LeftPattern] || results[binary.RightPattern]
                        : results[binary.LeftPattern] && results[binary.RightPattern];
                    break;
                case IRecursivePatternOperation recursive when Whole(recursive.MatchedType, recursive) && recursive.PropertySubpatterns.IsEmpty && !expanded:
                    pending.Push((held, true));
                    foreach (IPatternOperation child in recursive.DeconstructionSubpatterns) pending.Push((child, false));
                    break;
                case IRecursivePatternOperation recursive:
                    results[held] = Whole(recursive.MatchedType, recursive) && recursive.PropertySubpatterns.IsEmpty && recursive.DeconstructionSubpatterns.All(child => results[child]);
                    break;
                default:
                    results[held] = Leaf(held);
                    break;
            }
        }
        return results[pattern];
    }

    // --- [READING]
    private protected sealed class Reading(SemanticModel model, ImmutableDictionary<SyntaxTree, SemanticModel> models, CancellationToken token) {
        public SemanticModel Model { get; } = model;

        public CancellationToken Token { get; } = token;

        public Dictionary<IOperation, ILookup<ISymbol?, IOperation>> References { get; } = [];

        public Dictionary<IOperation, ILookup<ISymbol?, IOperation>> Ranges { get; } = [];

        public SemanticModel For(SyntaxTree tree) => tree == Model.SyntaxTree ? Model : models[tree];
    }

    private protected sealed class SemanticModels {
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
            context.RegisterOperationAction(analysis => {
                if (!analysis.IsGeneratedCode) operations.Add(((T)analysis.Operation, analysis.ContainingSymbol));
            }, kinds);
            Complete((end, models) => {
                Func<SemanticModel, Reading> read = model => new Reading(model, models, end.CancellationToken);
                foreach ((T operation, ISymbol owner) in operations)
                    foreach (Diagnostic diagnostic in found(operation, owner, readings.GetOrAdd(models[operation.Syntax.SyntaxTree], read))) {
                        end.CancellationToken.ThrowIfCancellationRequested();
                        end.ReportDiagnostic(diagnostic);
                    }
            });
        }
    }

    // --- [PACKAGES]
    private protected sealed class SystemSymbols(Compilation compilation) {
        public IMethodSymbol? Dispose { get; } =
            compilation.GetSpecialType(SpecialType.System_IDisposable).GetMembers(WellKnownMemberNames.DisposeMethodName).OfType<IMethodSymbol>().FirstOrDefault();

        public INamedTypeSymbol? Thunk { get; } = compilation.GetTypeByMetadataName("System.Func`1");

        public INamedTypeSymbol? Flags { get; } = compilation.GetTypeByMetadataName("System.FlagsAttribute");

        public ISymbol? IsMacOS { get; } = Declared(compilation, "M:System.OperatingSystem.IsMacOS").FirstOrDefault();

        public ImmutableArray<IMethodSymbol> CommandLine { get; } = Members(compilation.GetTypeByMetadataName("System.Environment"), "GetCommandLineArgs");

        public ImmutableArray<INamedTypeSymbol> Schedulers { get; } = [
            .. new[] {
                "System.Threading.Tasks.Parallel", "System.Threading.ThreadPool", "System.Threading.Thread", "System.Threading.Tasks.TaskFactory", "System.Threading.Tasks.TaskFactory`1",
                "System.Threading.Tasks.Task",
            }.Select(compilation.GetTypeByMetadataName).OfType<INamedTypeSymbol>(),
        ];

        public INamedTypeSymbol? FunctionPointer { get; } = compilation.GetTypeByMetadataName("System.Runtime.InteropServices.UnmanagedFunctionPointerAttribute");

        public INamedTypeSymbol? CallersOnly { get; } = compilation.GetTypeByMetadataName("System.Runtime.InteropServices.UnmanagedCallersOnlyAttribute");

        public INamedTypeSymbol? Type { get; } = compilation.GetTypeByMetadataName("System.Type");
    }

    private protected sealed class LanguageExtSymbols {
        public LanguageExtSymbols(Compilation compilation) {
            Fin = compilation.GetTypeByMetadataName("LanguageExt.Fin`1");
            Validation = compilation.GetTypeByMetadataName("LanguageExt.Validation`2");
            Error = compilation.GetTypeByMetadataName("LanguageExt.Common.Error");
            ManyErrors = compilation.GetTypeByMetadataName("LanguageExt.Common.ManyErrors");
            IO = compilation.GetTypeByMetadataName("LanguageExt.IO");
            IO1 = compilation.GetTypeByMetadataName("LanguageExt.IO`1");
            Seq = compilation.GetTypeByMetadataName("LanguageExt.Seq`1");
            K = compilation.GetTypeByMetadataName("LanguageExt.Traits.K`2");
            Option = compilation.GetTypeByMetadataName("LanguageExt.Option`1");
            Namespace = Declared(compilation, "N:LanguageExt").FirstOrDefault();
            INamedTypeSymbol? prelude = compilation.GetTypeByMetadataName("LanguageExt.Prelude");
            Lifts = Members(IO, "lift", "liftAsync", "liftVAsync", "pure");
            Optional = Members(prelude, "Optional");
            Uses = Members(prelude, "use");
            Releases = Members(prelude, "release");
            Conversions = Members(Option, "ToFin");
            Lenses = Members(compilation.GetTypeByMetadataName("LanguageExt.Lens`2"), "New");
            Maps = Members(Option, "Map");
            Nullable = Members(compilation.GetTypeByMetadataName("LanguageExt.OptionExtensions"), "ToNullable").SingleOrDefault();
            Unsafe = Members(compilation.GetTypeByMetadataName("LanguageExt.UnsafeValueAccess.UnsafeValueAccessExtensions"), "ValueUnsafe")
                .FirstOrDefault(method => Same(method.Parameters[0].Type, Option));
            Applies = Members(compilation.GetTypeByMetadataName("LanguageExt.ApplicativeExtensions"), "Apply");
            Matches = [.. Members(Fin, "Match"), .. Members(Option, "Match")];
            Factories = Declared(
                compilation,
                "M:LanguageExt.Fin.Succ``1(``0)",
                "M:LanguageExt.Fin.Fail``1(LanguageExt.Common.Error)",
                "M:LanguageExt.Validation.Success``2(``1)",
                "M:LanguageExt.Validation.Fail``2(``0)",
                "M:LanguageExt.Prelude.FinSucc``1(``0)",
                "M:LanguageExt.Prelude.FinFail``1(LanguageExt.Common.Error)",
                "M:LanguageExt.Prelude.Pure``1(``0)",
                "M:LanguageExt.Prelude.Fail``1(``0)",
                "M:LanguageExt.Prelude.Success``2(``1)",
                "M:LanguageExt.Prelude.Fail``2(``0)");
            Leaves = Declared(
                compilation,
                "P:LanguageExt.Common.Error.Head",
                "M:LanguageExt.Common.Error.AsIterable",
                "M:LanguageExt.Common.Error.FoldM``2(System.Func{LanguageExt.Common.Error,LanguageExt.Traits.K{``0,``1}})",
                "M:LanguageExt.Common.Error.ForAllM``2(System.Func{LanguageExt.Common.Error,LanguageExt.Traits.K{``0,``1}})");
            Identity = Declared(compilation, "M:LanguageExt.Prelude.identity``1(``0)").FirstOrDefault();
            Case = Declared(compilation, "P:LanguageExt.Option`1.Case").FirstOrDefault();
            IsSome = Declared(compilation, "P:LanguageExt.Option`1.IsSome").FirstOrDefault();
            IsNone = Declared(compilation, "P:LanguageExt.Option`1.IsNone").FirstOrDefault();
        }

        public INamedTypeSymbol? Fin { get; }

        public INamedTypeSymbol? Validation { get; }

        public INamedTypeSymbol? Error { get; }

        public INamedTypeSymbol? ManyErrors { get; }

        public INamedTypeSymbol? IO { get; }

        public INamedTypeSymbol? IO1 { get; }

        public INamedTypeSymbol? Seq { get; }

        public INamedTypeSymbol? K { get; }

        public INamedTypeSymbol? Option { get; }

        public ISymbol? Namespace { get; }

        public ImmutableArray<IMethodSymbol> Lifts { get; }

        public ImmutableArray<IMethodSymbol> Optional { get; }

        public ImmutableArray<IMethodSymbol> Uses { get; }

        public ImmutableArray<IMethodSymbol> Releases { get; }

        public ImmutableArray<IMethodSymbol> Conversions { get; }

        public ImmutableArray<IMethodSymbol> Lenses { get; }

        public ImmutableArray<IMethodSymbol> Maps { get; }

        public IMethodSymbol? Nullable { get; }

        public IMethodSymbol? Unsafe { get; }

        public ImmutableArray<IMethodSymbol> Applies { get; }

        public ImmutableArray<IMethodSymbol> Matches { get; }

        public ImmutableArray<ISymbol> Factories { get; }

        public ImmutableArray<ISymbol> Leaves { get; }

        public ISymbol? Identity { get; }

        public ISymbol? Case { get; }

        public ISymbol? IsSome { get; }

        public ISymbol? IsNone { get; }
    }

    private protected sealed class ThinktectureSymbols {
        public ThinktectureSymbols(Compilation compilation) {
            ValidationError = compilation.GetTypeByMetadataName("Thinktecture.ValidationError");
            ObjectFactory = compilation.GetTypeByMetadataName("Thinktecture.IObjectFactory`3");
            SmartEnum = compilation.GetTypeByMetadataName("Thinktecture.ISmartEnum`3");
            UnionAttributeBase = compilation.GetTypeByMetadataName("Thinktecture.UnionAttributeBase");
            SmartEnumAttribute1 = compilation.GetTypeByMetadataName("Thinktecture.SmartEnumAttribute`1");
            UnionAttribute = compilation.GetTypeByMetadataName("Thinktecture.UnionAttribute");
            SmartEnumAttribute = compilation.GetTypeByMetadataName("Thinktecture.SmartEnumAttribute");
            SmartEnums = [.. new[] { SmartEnumAttribute, SmartEnumAttribute1 }.OfType<INamedTypeSymbol>()];
            Marks = [.. new[] { UnionAttributeBase, UnionAttribute }.OfType<INamedTypeSymbol>(), .. SmartEnums];
            Validated = [
                .. new[] {
                    compilation.GetTypeByMetadataName("Thinktecture.ValueObjectAttribute`1"),
                    compilation.GetTypeByMetadataName("Thinktecture.ComplexValueObjectAttribute"),
                }.OfType<INamedTypeSymbol>(),
                .. SmartEnums,
            ];
            Validate = Declared(compilation, "M:Thinktecture.IObjectFactory`3.Validate(`1,System.IFormatProvider,`0@)").FirstOrDefault();
            Message = Declared(compilation, "P:Thinktecture.ValidationError.Message").FirstOrDefault();
        }

        public INamedTypeSymbol? ValidationError { get; }

        public INamedTypeSymbol? ObjectFactory { get; }

        public INamedTypeSymbol? SmartEnum { get; }

        public INamedTypeSymbol? UnionAttributeBase { get; }

        public INamedTypeSymbol? SmartEnumAttribute1 { get; }

        public INamedTypeSymbol? UnionAttribute { get; }

        public INamedTypeSymbol? SmartEnumAttribute { get; }

        public ImmutableArray<INamedTypeSymbol> Marks { get; }

        public ImmutableArray<INamedTypeSymbol> SmartEnums { get; }

        public ImmutableArray<INamedTypeSymbol> Validated { get; }

        public ISymbol? Validate { get; }

        public ISymbol? Message { get; }
    }

    private protected sealed class RhinoCommonSymbols {
        private static readonly ImmutableArray<(string Owner, string Member)> Counts = [
            ("Rhino.DocObjects.Tables.ObjectTable", "Delete"),
            ("Rhino.DocObjects.Tables.ObjectTable", "DeleteGrips"),
            ("Rhino.DocObjects.Tables.LayerTable", "Delete"),
            ("Rhino.DocObjects.Tables.HatchPatternTable", "Delete"),
            ("Rhino.DocObjects.Tables.MarkupTable", "Delete"),
            ("Rhino.DocObjects.Tables.SectionStyleTable", "Delete"),
            ("Rhino.FileIO.File3dmObjectTable", "Delete"),
            ("Rhino.Geometry.Collections.MeshFaceList", "DeleteFaces"),
            ("Rhino.Geometry.Collections.MeshNgonList", "RemoveNgons"),
            ("Rhino.Geometry.PointCloud", "RemoveRange"),
            ("Rhino.DocObjects.HatchPattern", "SetHatchLines"),
            ("Rhino.DocObjects.Worksession", "Attach"),
        ];

        public RhinoCommonSymbols(Compilation compilation) {
            Document = compilation.GetTypeByMetadataName("Rhino.RhinoDoc");
            ActiveDoc = Declared(compilation, "P:Rhino.RhinoDoc.ActiveDoc").FirstOrDefault();
            RenderSettings = Declared(compilation, "P:Rhino.RhinoDoc.RenderSettings").FirstOrDefault();
            Counted = [.. Counts.SelectMany(row => Members(compilation.GetTypeByMetadataName(row.Owner), row.Member))];
            Command = compilation.GetTypeByMetadataName("Rhino.Commands.Command");
            CommandStyle = compilation.GetTypeByMetadataName("Rhino.Commands.CommandStyleAttribute");
            ScriptRunner = Declared(compilation, "F:Rhino.Commands.Style.ScriptRunner").FirstOrDefault();
            RunScript = [.. Members(compilation.GetTypeByMetadataName("Rhino.RhinoApp"), "RunScript")];
            SetDisplayModeOverride = [.. Members(compilation.GetTypeByMetadataName("Rhino.DocObjects.ObjectAttributes"), "SetDisplayModeOverride")];
            RestoreWithAspectRatio = Members(compilation.GetTypeByMetadataName("Rhino.DocObjects.Tables.NamedViewTable"), "RestoreWithAspectRatio");
            GrayscaleDib = Declared(compilation, "M:Rhino.Display.ZBufferCapture.GrayscaleDib").FirstOrDefault();
            ObjectPropertiesPage = compilation.GetTypeByMetadataName("Rhino.UI.ObjectPropertiesPage");
            PageControl = Declared(compilation, "P:Rhino.UI.ObjectPropertiesPage.PageControl").FirstOrDefault();
            DisplayModeDescription = compilation.GetTypeByMetadataName("Rhino.Display.DisplayModeDescription");
            OwnedModes = [
                .. Members(DisplayModeDescription, "GetDisplayMode", "FindByName"),
                .. Declared(compilation, "P:Rhino.Display.RhinoViewport.DisplayMode"),
            ];
            PersistentSettings = compilation.GetTypeByMetadataName("Rhino.PersistentSettings");
            Redraws = [
                .. Members(compilation.GetTypeByMetadataName("Rhino.DocObjects.Tables.ViewTable"), "Redraw", "EnableRedraw"),
                .. Members(compilation.GetTypeByMetadataName("Rhino.Display.RhinoView"), "Redraw"),
                .. Declared(compilation, "P:Rhino.DocObjects.Tables.ViewTable.RedrawEnabled"),
            ];
            Duplicates = [.. Members(compilation.GetTypeByMetadataName("Rhino.Geometry.GeometryBase"), "Duplicate", "DuplicateShallow")];
            Floating = Declared(compilation, "P:Rhino.Display.RhinoView.Floating").FirstOrDefault();
            ConduitEnabled = Declared(compilation, "P:Rhino.Display.DisplayConduit.Enabled").FirstOrDefault();
        }

        public INamedTypeSymbol? Document { get; }

        public ISymbol? ActiveDoc { get; }

        public ISymbol? RenderSettings { get; }

        public ImmutableArray<IMethodSymbol> Counted { get; }

        public INamedTypeSymbol? Command { get; }

        public INamedTypeSymbol? CommandStyle { get; }

        public ISymbol? ScriptRunner { get; }

        public ImmutableArray<ISymbol> RunScript { get; }

        public ImmutableArray<ISymbol> SetDisplayModeOverride { get; }

        public ImmutableArray<IMethodSymbol> RestoreWithAspectRatio { get; }

        public ISymbol? GrayscaleDib { get; }

        public INamedTypeSymbol? ObjectPropertiesPage { get; }

        public ISymbol? PageControl { get; }

        public INamedTypeSymbol? DisplayModeDescription { get; }

        public ImmutableArray<ISymbol> OwnedModes { get; }

        public INamedTypeSymbol? PersistentSettings { get; }

        public ImmutableArray<ISymbol> Redraws { get; }

        public ImmutableArray<ISymbol> Duplicates { get; }

        public ISymbol? Floating { get; }

        public ISymbol? ConduitEnabled { get; }
    }

    private protected sealed class EtoSymbols(Compilation compilation) {
        public INamedTypeSymbol? Application { get; } = compilation.GetTypeByMetadataName("Eto.Forms.Application");

        public INamedTypeSymbol? Widget { get; } = compilation.GetTypeByMetadataName("Eto.Widget");

        public INamedTypeSymbol? Control { get; } = compilation.GetTypeByMetadataName("Eto.Forms.Control");

        public INamedTypeSymbol? Container { get; } = compilation.GetTypeByMetadataName("Eto.Forms.Container");

        public INamedTypeSymbol? StackLayout { get; } = compilation.GetTypeByMetadataName("Eto.Forms.StackLayout");

        public INamedTypeSymbol? SystemFonts { get; } = compilation.GetTypeByMetadataName("Eto.Drawing.SystemFonts");

        public INamedTypeSymbol? Font { get; } = compilation.GetTypeByMetadataName("Eto.Drawing.Font");

        public ImmutableArray<IMethodSymbol> AddMethod { get; } = Members(compilation.GetTypeByMetadataName("Eto.Mac.ObjCExtensions"), "AddMethod");

        public ImmutableArray<IMethodSymbol> Handlers { get; } = [.. Members(compilation.GetTypeByMetadataName("Eto.Platform"), "Add", "Find").Where(static method => method.TypeParameters.Length is 1)];
    }

    private protected sealed class MacSymbols(Compilation compilation) {
        public INamedTypeSymbol? DispatchQueue { get; } = compilation.GetTypeByMetadataName("CoreFoundation.DispatchQueue");

        public INamedTypeSymbol? NSObject { get; } = compilation.GetTypeByMetadataName("Foundation.NSObject");

        public INamedTypeSymbol? NSThread { get; } = compilation.GetTypeByMetadataName("Foundation.NSThread");

        public ImmutableArray<ISymbol> Mains { get; } = Declared(compilation, "P:CoreFoundation.DispatchQueue.MainQueue", "P:Foundation.NSThread.MainThread");

        public INamedTypeSymbol? Register { get; } = compilation.GetTypeByMetadataName("Foundation.RegisterAttribute");

        public INamedTypeSymbol? Export { get; } = compilation.GetTypeByMetadataName("Foundation.ExportAttribute");
    }

    private protected sealed class RasmRhinoSymbols {
        public RasmRhinoSymbols(Compilation compilation) {
            INamedTypeSymbol? products = compilation.GetTypeByMetadataName("Rasm.Rhino.Products");
            Transfer = Members(products, "Transfer");
            Acquire = [.. Members(products, "Of", "Many", "Maybe"), .. Transfer];
            Required = Members(compilation.GetTypeByMetadataName("Rasm.Rhino.Conversions"), "Required");
            Release = Members(products, "Released");
            Retain = Members(products, "Replace");
            Session = compilation.GetTypeByMetadataName("Rasm.Rhino.Session");
            Boundaries = Members(Session, "Run", "Start", "RunSafe");
            Conduits = compilation.GetTypeByMetadataName("Rasm.Rhino.Display.Conduits");
            RenderSettingsSources = compilation.GetTypeByMetadataName("Rasm.Rhino.Render.Settings.RenderSettingsSources");
            Commits = compilation.GetTypeByMetadataName("Rasm.Rhino.Document.Commits");
            UI = Declared(compilation, "N:Rasm.Rhino.UI").FirstOrDefault();
        }

        public ImmutableArray<IMethodSymbol> Acquire { get; }

        public ImmutableArray<IMethodSymbol> Required { get; }

        public ImmutableArray<IMethodSymbol> Release { get; }

        public ImmutableArray<IMethodSymbol> Transfer { get; }

        public ImmutableArray<IMethodSymbol> Retain { get; }

        public INamedTypeSymbol? Session { get; }

        public ImmutableArray<IMethodSymbol> Boundaries { get; }

        public INamedTypeSymbol? Conduits { get; }

        public INamedTypeSymbol? RenderSettingsSources { get; }

        public INamedTypeSymbol? Commits { get; }

        public ISymbol? UI { get; }
    }

    // --- [LIFETIMES]
    private protected sealed class Lifetimes {
        private sealed class PathComparer<T>(IEqualityComparer<T> values) : IEqualityComparer<(T Value, ImmutableArray<int> Path)> where T : notnull {
            public bool Equals((T Value, ImmutableArray<int> Path) x, (T Value, ImmutableArray<int> Path) y) =>
                values.Equals(x.Value, y.Value) && x.Path.SequenceEqual(y.Path);

            public int GetHashCode((T Value, ImmutableArray<int> Path) obj) {
                int hash = values.GetHashCode(obj.Value);
                foreach (int index in obj.Path) hash = (hash, index).GetHashCode();
                return hash;
            }
        }

        private Lifetimes(SystemSymbols system, RasmRhinoSymbols rasm, LanguageExtSymbols languageExt, IMethodSymbol dispose) {
            bool Acquiring(IMethodSymbol method) =>
                method.TypeParameters.Length > 0 && method.Parameters.Length > 0
                && method.Parameters[0].Type is INamedTypeSymbol { TypeArguments.Length: 1 } holder
                && (Same(holder, languageExt.IO1) || Same(holder, system.Thunk)) && SymbolEqualityComparer.Default.Equals(holder.TypeArguments[0], method.TypeParameters[0]);
            Dispose = dispose;
            Acquires = [.. rasm.Acquire, .. languageExt.Uses.Where(Acquiring)];
            Releases = [.. rasm.Release, .. languageExt.Releases];
            Transfers = rasm.Transfer;
            Retains = rasm.Retain;
            Lifts = languageExt.Lifts;
            Conversions = [.. languageExt.Optional, .. languageExt.Conversions];
        }

        public IMethodSymbol Dispose { get; }

        public ImmutableArray<IMethodSymbol> Acquires { get; }

        public ImmutableArray<IMethodSymbol> Releases { get; }

        public ImmutableArray<IMethodSymbol> Transfers { get; }

        public ImmutableArray<IMethodSymbol> Retains { get; }

        public ImmutableArray<IMethodSymbol> Lifts { get; }

        public ImmutableArray<IMethodSymbol> Conversions { get; }

        public static Lifetimes? Of(SystemSymbols system, RasmRhinoSymbols rasm, LanguageExtSymbols languageExt) =>
            system.Dispose is { } dispose ? new Lifetimes(system, rasm, languageExt, dispose) : null;

        public bool Plain(IMethodSymbol method) =>
            method.TypeParameters.Length > 0 && method.TypeParameters[0].ConstraintTypes.Any(constraint => Same(constraint, Dispose.ContainingType));

        public bool Disposes(IMethodSymbol method) => Implements(method, Dispose);

        public bool Retaining(IInvocationOperation call, IParameterSymbol parameter) =>
            Among(call.TargetMethod, Retains) || (call.TargetMethod.MethodKind is MethodKind.DelegateInvoke && Same(parameter.Type, Dispose.ContainingType));

        public static IEnumerable<IParameterSymbol> Handed(IInvocationOperation call, ITypeSymbol held) =>
            call.Arguments.Select(static argument => Lambda(argument.Value)).OfType<IAnonymousFunctionOperation>()
                .Select(static function => function.Symbol.Parameters.FirstOrDefault()).OfType<IParameterSymbol>().Where(parameter => SymbolEqualityComparer.Default.Equals(parameter.Type, held));

        public IEnumerable<string> Released(IOperation value, ITypeSymbol held, Reading reading) {
            Queue<(IOperation Value, ImmutableArray<int> Path, ITypeSymbol Carrier)> pending = new([(value, [], held)]);
            HashSet<(IOperation Value, ImmutableArray<int> Path)> visited = new(new PathComparer<IOperation>(EqualityComparer<IOperation>.Default));
            HashSet<(ISymbol Value, ImmutableArray<int> Path)> carriers = new(new PathComparer<ISymbol>(SymbolEqualityComparer.Default));
            IEnumerable<IOperation> Follow(ISymbol carrier, IOperation site, ImmutableArray<int> path) =>
                carriers.Add((carrier, path)) ? References(carrier, site, reading) : [];
            IEnumerable<IOperation> Pass(IInvocationOperation call, ImmutableArray<int> path, ITypeSymbol carried) =>
                Handed(call, path.IsEmpty ? held : carried).SelectMany(parameter => Follow(parameter, call, path));
            while (pending.Count is > 0) {
                reading.Token.ThrowIfCancellationRequested();
                (IOperation current, ImmutableArray<int> path, ITypeSymbol carried) = pending.Dequeue();
                if (!visited.Add((current, path))) continue;
                ISymbol? range = !current.IsImplicit && Clause(current.Syntax) is { } clause ? reading.For(clause.SyntaxTree).GetDeclaredSymbol(clause, reading.Token) : null;
                IOperation? consumer = range is null ? Consumer(current) : null;
                string? release = consumer switch {
                    IInvocationOperation call when path.IsEmpty && Disposes(call.TargetMethod) => call.TargetMethod.Name,
                    IMethodReferenceOperation reference when path.IsEmpty && Disposes(reference.Method) => reference.Method.Name,
                    IArgumentOperation { Parent: IInvocationOperation call, Parameter: { } parameter } when path.IsEmpty
                        && ((Among(call.TargetMethod, Acquires) && parameter.Ordinal is 0) || Among(call.TargetMethod, Releases) || Retaining(call, parameter)) => Label(call),
                    IUsingOperation or IVariableInitializerOperation { Parent: IVariableDeclaratorOperation { Parent.Parent: IVariableDeclarationGroupOperation { Parent: IUsingDeclarationOperation or IUsingOperation } } } when path.IsEmpty => "using",
                    _ => null,
                };
                if (release is not null) {
                    yield return release;
                    continue;
                }
                IEnumerable<IOperation> next = consumer switch {
                    null when range is { } bound => Follow(bound, current, path),
                    IInvocationOperation call when Among(call.TargetMethod, Conversions) => [call],
                    IInvocationOperation call => Pass(call, path, carried),
                    IArgumentOperation { Parent: IInvocationOperation call, Parameter: not null } when Among(call.TargetMethod, Lifts) || Among(call.TargetMethod, Conversions) => [call],
                    IArgumentOperation { Parent: IInvocationOperation { TargetMethod.IsExtensionMethod: true } call, Parameter.Ordinal: 0 } => Pass(call, path, carried),
                    IReturnOperation { Parent: IBlockOperation { Parent: IAnonymousFunctionOperation function } } => [function],
                    IDelegateCreationOperation creation => [creation],
                    IConversionOperation { Conversion: { IsIdentity: true } or { IsReference: true } } conversion => [conversion],
                    IConditionalAccessOperation conditional when conditional.Operation.Syntax == current.Syntax =>
                        conditional.WhenNotNull.DescendantsAndSelf().OfType<IConditionalAccessInstanceOperation>().Where(instance => instance.Syntax == conditional.Operation.Syntax),
                    IVariableInitializerOperation { Parent: IVariableDeclaratorOperation declarator } => Follow(declarator.Symbol, current, path),
                    ISimpleAssignmentOperation { Target: ILocalReferenceOperation local } assignment when Stripped(assignment.Value) == current => Follow(local.Local, current, path),
                    IOperation aggregate when aggregate is ICoalesceOperation or IConditionalOperation or ICollectionExpressionOperation or IArrayInitializerOperation or IArrayCreationOperation or ISpreadOperation => [aggregate],
                    _ => [],
                };
                foreach (IOperation reference in next) pending.Enqueue((reference, path, carried));
                if (consumer is ITupleOperation { Type: { IsTupleType: true } type } tuple && (path.IsEmpty || SymbolEqualityComparer.Default.Equals(current.Type, carried))) {
                    IOperation element = current;
                    while (element.Parent is IConversionOperation { IsImplicit: true, OperatorMethod: null } conversion) element = conversion;
                    pending.Enqueue((tuple, path.Insert(0, tuple.Elements.IndexOf(element)), type));
                    continue;
                }
                if (consumer is IFieldReferenceOperation { Field: { CorrespondingTupleField: { } field, ContainingType.TupleElements: var elements } } projected
                    && !path.IsEmpty && path[0] < elements.Length && Same(elements[path[0]].CorrespondingTupleField, field)) {
                    ImmutableArray<int> remainder = path.RemoveAt(0);
                    pending.Enqueue((projected, remainder, remainder.IsEmpty ? held : projected.Field.Type));
                    continue;
                }
                if (consumer is not IDeconstructionAssignmentOperation deconstruction || path.IsEmpty || Stripped(deconstruction.Value) != current) continue;
                IOperation target = deconstruction.Target;
                ImmutableArray<int> remaining = path;
                while (target is IDeclarationExpressionOperation || (target is ITupleOperation && !remaining.IsEmpty)) {
                    reading.Token.ThrowIfCancellationRequested();
                    if (target is IDeclarationExpressionOperation declaration) target = declaration.Expression;
                    else if (target is ITupleOperation targets && remaining[0] < targets.Elements.Length) {
                        target = targets.Elements[remaining[0]];
                        remaining = remaining.RemoveAt(0);
                    } else break;
                }
                if (Variable(target) is { } assigned && (remaining.IsEmpty ? held : target.Type) is { } extracted)
                    foreach (IOperation reference in Follow(assigned, current, remaining)) pending.Enqueue((reference, remaining, extracted));
            }
        }
    }
}
