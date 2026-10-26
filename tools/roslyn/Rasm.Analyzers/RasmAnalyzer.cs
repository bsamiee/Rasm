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
        return node.DescendantNodesAndSelf().OfType<IdentifierNameSyntax>().Where(used => !own.Contains(used.Identifier.ValueText) && used.Identifier.ValueText is not "_"
            && used.Parent switch { MemberAccessExpressionSyntax access => access.Name != used, MemberBindingExpressionSyntax or NameColonSyntax or NameEqualsSyntax => false, _ => true });
    }
}

// --- [COMPOSITION] ---------------------------------------------------------------------
public abstract class RasmAnalyzer(GeneratedCodeAnalysisFlags generated = GeneratedCodeAnalysisFlags.None) : DiagnosticAnalyzer {
    public sealed override void Initialize(AnalysisContext context) {
        context.EnableConcurrentExecution();
        context.ConfigureGeneratedCodeAnalysis(generated);
        context.RegisterCompilationStartAction(Start);
    }

    protected abstract void Start(CompilationStartAnalysisContext context);

    // --- [REPORTS]
    protected static DiagnosticDescriptor Rule(string id, string category, string title, string message, string description) =>
        new(id, title, message, category, DiagnosticSeverity.Error, isEnabledByDefault: true, description);

    protected static void Report<T>(CompilationStartAnalysisContext context, ImmutableArray<OperationKind> kinds, Func<T, OperationAnalysisContext, IEnumerable<Diagnostic>> found) where T : IOperation =>
        context.RegisterOperationAction(
            analysis => {
                foreach (Diagnostic diagnostic in found((T)analysis.Operation, analysis)) analysis.ReportDiagnostic(diagnostic);
            },
            kinds);

    protected static void Report<T>(CompilationStartAnalysisContext context, ImmutableArray<OperationKind> kinds, DiagnosticDescriptor rule, Func<T, OperationAnalysisContext, bool> found) where T : IOperation =>
        Report<T>(context, kinds, (operation, analysis) => found(operation, analysis) ? [Diagnostic.Create(rule, operation.Syntax.GetLocation(), operation.Syntax)] : []);

    protected static void Writes(CompilationStartAnalysisContext context, string property, DiagnosticDescriptor rule, Func<IAssignmentOperation, OperationAnalysisContext, bool> wrong) {
        if (Declared(context.Compilation, property).FirstOrDefault() is not { } written) return;
        Report<IAssignmentOperation>(context, [OperationKind.SimpleAssignment, OperationKind.CompoundAssignment, OperationKind.CoalesceAssignment], rule, (assignment, analysis) =>
            assignment.Target is IPropertyReferenceOperation reference && Same(reference.Property, written) && wrong(assignment, analysis));
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
        SymbolEqualityComparer.Default.Equals(symbol?.OriginalDefinition, other?.OriginalDefinition);

    protected static bool Among<T>(ISymbol? symbol, ImmutableArray<T> members) where T : class, ISymbol => members.Any(member => Same(symbol, member));

    protected static bool Within(ISymbol? symbol, INamedTypeSymbol? owner) =>
        symbol is not null && owner is not null && (Same(symbol, owner) || Within(symbol.ContainingSymbol, owner));

    protected static bool Derives(ITypeSymbol? type, INamedTypeSymbol root) =>
        type is not null && (Same(type, root) || Derives(type.BaseType, root));

    protected static AttributeData? Marked(ISymbol symbol, INamedTypeSymbol? attribute) => symbol.GetAttributes().FirstOrDefault(data => attribute is not null && Same(data.AttributeClass, attribute));

    protected static bool Sourced(ISymbol symbol) => symbol.Locations.Any(static location => location.IsInSource);

    protected static bool Delegated(IMethodSymbol method) => method.Parameters.Any(static parameter => parameter.Type.TypeKind is TypeKind.Delegate);

    protected static INamedTypeSymbol? Enumerated(ITypeSymbol type) =>
        type.AllInterfaces.Append(type).OfType<INamedTypeSymbol>().FirstOrDefault(static face => face.OriginalDefinition.SpecialType is SpecialType.System_Collections_Generic_IEnumerable_T);

    protected static bool Implements(IMethodSymbol method, ISymbol member) => Same(method, member) || Same(method.ContainingType?.FindImplementationForInterfaceMember(member), method);

    protected static ISymbol Member(ISymbol symbol) => symbol is IMethodSymbol { AssociatedSymbol: { } associated } ? associated : symbol;

    protected static IEnumerable<ISymbol> Overridden(ISymbol? member) =>
        member is null ? [] : Overridden(member switch { IMethodSymbol method => method.OverriddenMethod, IPropertySymbol property => property.OverriddenProperty, _ => null }).Prepend(member);

    protected static string Label(IMethodSymbol method) =>
        method.MethodKind is MethodKind.Constructor ? $"new {method.ContainingType.Name}" : $"{method.ContainingType.Name}.{method.Name}";

    protected static string Label(IInvocationOperation call) =>
        call.TargetMethod.MethodKind is MethodKind.DelegateInvoke && call.Instance is { } handed ? handed.Syntax.ToString() : Label(call.TargetMethod);

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

    protected static IOperation Root(IOperation operation) => operation.Parent is { } parent ? Root(parent) : operation;

    protected static bool Written(IOperation reference) =>
        reference.Parent switch {
            IAssignmentOperation assignment => assignment.Target == reference,
            IIncrementOrDecrementOperation or IArgumentOperation { Parameter.RefKind: RefKind.Ref or RefKind.Out } => true,
            _ => false,
        };

    protected static ISymbol? Target(IOperation operation) =>
        operation switch {
            IInvocationOperation call => call.TargetMethod,
            IMemberReferenceOperation reference => reference.Member,
            _ => null,
        };

    protected static IEnumerable<IOperation> Uses(ISymbol symbol, IOperation within) =>
        Root(within).Descendants().Where(reference => Same(reference switch { ILocalReferenceOperation local => local.Local, IParameterReferenceOperation parameter => parameter.Parameter, _ => null }, symbol));

    private protected static IEnumerable<IOperation> Declarations(ISymbol symbol, Reading reading) =>
        symbol.DeclaringSyntaxReferences.Where(declared => declared.SyntaxTree == reading.Model.SyntaxTree)
            .Select(declared => reading.Model.GetOperation(declared.GetSyntax(reading.Token), reading.Token)).OfType<IOperation>();

    private protected static IEnumerable<IOperation> References(ISymbol symbol, IOperation within, Reading reading) =>
        Root(within).Syntax.DescendantNodes().OfType<IdentifierNameSyntax>()
            .Where(name => string.Equals(name.Identifier.ValueText, symbol.Name, StringComparison.Ordinal)
                && SymbolEqualityComparer.Default.Equals(reading.Model.GetSymbolInfo(name, reading.Token).Symbol, symbol))
            .Select(name => reading.Model.GetOperation(name, reading.Token))
            .OfType<IOperation>();

    private protected static QueryClauseSyntax? Clause(SyntaxNode syntax) =>
        syntax.Parent switch {
            ParenthesizedExpressionSyntax parenthesized => Clause(parenthesized),
            FromClauseSyntax from when from.Expression == syntax => from,
            LetClauseSyntax let when let.Expression == syntax => let,
            _ => null,
        };

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

    // --- [READING]
    private protected sealed class Reading(SemanticModel model, CancellationToken token) {
        public SemanticModel Model { get; } = model;

        public CancellationToken Token { get; } = token;
    }

    // --- [LIFETIMES]
    private protected sealed class Lifetimes {
        private Lifetimes(Compilation compilation, IMethodSymbol dispose, INamedTypeSymbol action) {
            INamedTypeSymbol? scopes = compilation.GetTypeByMetadataName("Rasm.Rhino.DisposalOps");
            INamedTypeSymbol? io = compilation.GetTypeByMetadataName("LanguageExt.IO`1");
            INamedTypeSymbol? thunk = compilation.GetTypeByMetadataName("System.Func`1");
            bool Acquiring(IMethodSymbol method) =>
                method.TypeParameters.Length > 0 && method.Parameters.Length > 0
                && method.Parameters[0].Type is INamedTypeSymbol { TypeArguments.Length: 1 } holder
                && (Same(holder, io) || Same(holder, thunk)) && SymbolEqualityComparer.Default.Equals(holder.TypeArguments[0], method.TypeParameters[0]);
            (Dispose, Action) = (dispose, action);
            Acquires = [.. Members(scopes, "Acquire", "Within", "Found").Concat(Members(compilation.GetTypeByMetadataName("Rasm.Rhino.Modeling.Copies"), "Owned")).Where(Acquiring)];
            Releases = Members(scopes, "Release", "Present");
            Failures = Members(scopes, "OnFailure");
            Holders = compilation.GetTypeByMetadataName("Rasm.Rhino.Disposal`1")?.InstanceConstructors ?? [];
            Retains = Members(compilation.GetTypeByMetadataName("Rasm.Rhino.Retained`1"), "Retain");
            Lifts = Members(compilation.GetTypeByMetadataName("LanguageExt.IO"), "lift", "pure");
        }

        public IMethodSymbol Dispose { get; }

        public INamedTypeSymbol Action { get; }

        public ImmutableArray<IMethodSymbol> Acquires { get; }

        public ImmutableArray<IMethodSymbol> Releases { get; }

        public ImmutableArray<IMethodSymbol> Failures { get; }

        public ImmutableArray<IMethodSymbol> Holders { get; }

        public ImmutableArray<IMethodSymbol> Retains { get; }

        public ImmutableArray<IMethodSymbol> Lifts { get; }

        public static Lifetimes? Of(Compilation compilation) =>
            Declared(compilation, "M:System.IDisposable.Dispose").FirstOrDefault() is IMethodSymbol dispose && compilation.GetTypeByMetadataName("System.Action`1") is { } action
                ? new Lifetimes(compilation, dispose, action)
                : null;

        public bool Plain(IMethodSymbol method) =>
            method.TypeParameters.Length > 0 && method.TypeParameters[0].ConstraintTypes.Any(constraint => Same(constraint, Dispose.ContainingType));

        public bool Disposes(IMethodSymbol method) => Implements(method, Dispose);

        public bool Retaining(IInvocationOperation call, IParameterSymbol parameter) =>
            Among(call.TargetMethod, Retains) || (call.TargetMethod.MethodKind is MethodKind.DelegateInvoke && Same(parameter.Type, Dispose.ContainingType));

        public static IEnumerable<IParameterSymbol> Handed(IInvocationOperation call, ITypeSymbol held) =>
            call.Arguments.Select(static argument => Stripped(argument.Value)).OfType<IDelegateCreationOperation>().Select(static creation => creation.Target).OfType<IAnonymousFunctionOperation>()
                .Select(static function => function.Symbol.Parameters.FirstOrDefault()).OfType<IParameterSymbol>().Where(parameter => Same(parameter.Type, held));

        public IEnumerable<string> Released(IOperation value, ITypeSymbol held, Reading reading) => Released(value, held, reading, []);

        private IEnumerable<string> Released(IOperation value, ITypeSymbol held, Reading reading, ImmutableHashSet<ISymbol> seen) =>
            !value.IsImplicit && Clause(value.Syntax) is { } clause && reading.Model.GetDeclaredSymbol(clause, reading.Token) is { } range
                ? Followed(range, value, held, reading, seen)
                : Consumer(value) switch {
                    IInvocationOperation call when Disposes(call.TargetMethod) => [call.TargetMethod.Name],
                    IInvocationOperation call => Passed(call, held, reading, seen),
                    IMethodReferenceOperation reference when Disposes(reference.Method) => [reference.Method.Name],
                    IArgumentOperation { Parent: IInvocationOperation call, Parameter: { } parameter } => Argued(call, parameter, held, reading, seen),
                    IArgumentOperation { Parent: IObjectCreationOperation { Constructor: { } constructor }, Parameter.Ordinal: 0 } when Among(constructor, Holders) => [Label(constructor)],
                    IReturnOperation { Parent: IBlockOperation { Parent: IAnonymousFunctionOperation function } } => Released(function, held, reading, seen),
                    IDelegateCreationOperation creation => Released(creation, held, reading, seen),
                    IVariableInitializerOperation { Parent: IVariableDeclaratorOperation declarator } =>
                        declarator.Parent?.Parent is IVariableDeclarationGroupOperation { Parent: IUsingDeclarationOperation or IUsingOperation } ? ["using"] : Followed(declarator.Symbol, value, held, reading, seen),
                    ISimpleAssignmentOperation { Target: ILocalReferenceOperation local } assignment when Stripped(assignment.Value) == value => Followed(local.Local, value, held, reading, seen),
                    IUsingOperation => ["using"],
                    ICoalesceOperation or IConditionalOperation or ICollectionExpressionOperation or IArrayInitializerOperation or IArrayCreationOperation or ISpreadOperation =>
                        Released(Consumer(value)!, held, reading, seen),
                    _ => [],
                };

        private IEnumerable<string> Followed(ISymbol carrier, IOperation value, ITypeSymbol held, Reading reading, ImmutableHashSet<ISymbol> seen) =>
            seen.Contains(carrier) ? [] : References(carrier, value, reading).SelectMany(reference => Released(reference, held, reading, seen.Add(carrier)));

        private IEnumerable<string> Passed(IInvocationOperation call, ITypeSymbol held, Reading reading, ImmutableHashSet<ISymbol> seen) =>
            Handed(call, held).SelectMany(parameter => Followed(parameter, call, held, reading, seen));

        private IEnumerable<string> Argued(IInvocationOperation call, IParameterSymbol parameter, ITypeSymbol held, Reading reading, ImmutableHashSet<ISymbol> seen) =>
            (Among(call.TargetMethod, Acquires) && parameter.Ordinal is 0) || Among(call.TargetMethod, Releases) || Retaining(call, parameter) ? [Label(call)]
            : Among(call.TargetMethod, Lifts) ? Released(call, held, reading, seen)
            : call.TargetMethod.IsExtensionMethod && parameter.Ordinal is 0 ? Passed(call, held, reading, seen)
            : [];
    }
}
