using System.Collections.Immutable;
using Analyzer.Utilities;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Operations;
using Rasm.Analyzers;

namespace Rasm.Rhino.Analyzers;

// --- [SERVICES] ------------------------------------------------------------------------

internal sealed class Lifetimes {
    private sealed class PathComparer<T>(IEqualityComparer<T> values) : IEqualityComparer<(T Value, ImmutableArray<int> Path)> where T : notnull {
        public bool Equals((T Value, ImmutableArray<int> Path) x, (T Value, ImmutableArray<int> Path) y) =>
            values.Equals(x.Value, y.Value) && x.Path.SequenceEqual(y.Path);

        public int GetHashCode((T Value, ImmutableArray<int> Path) obj) {
            int hash = values.GetHashCode(obj.Value);
            foreach (int index in obj.Path) hash = (hash, index).GetHashCode();
            return hash;
        }
    }

    private Lifetimes(Compilation compilation, IMethodSymbol dispose) {
        WellKnownTypeProvider types = WellKnownTypeProvider.GetOrCreate(compilation);
        INamedTypeSymbol? products = types.GetOrCreateTypeByMetadataName("Rasm.Rhino.Products");
        INamedTypeSymbol? prelude = types.GetOrCreateTypeByMetadataName("LanguageExt.Prelude");
        INamedTypeSymbol? effect = types.GetOrCreateTypeByMetadataName("LanguageExt.IO`1");
        INamedTypeSymbol? thunk = types.GetOrCreateTypeByMetadataName("System.Func`1");
        bool Acquiring(IMethodSymbol method) =>
            method.TypeParameters.Length > 0 && method.Parameters.Length > 0
            && method.Parameters[0].Type is INamedTypeSymbol { TypeArguments.Length: 1 } holder
            && (Same(holder, effect) || Same(holder, thunk)) && SymbolEqualityComparer.Default.Equals(holder.TypeArguments[0], method.TypeParameters[0]);
        Dispose = dispose;
        Transfers = Members(products, "Transfer");
        Acquires = [.. Members(products, "Of", "Many", "Maybe"), .. Transfers, .. Members(prelude, "use").Where(Acquiring)];
        Releases = [.. Members(products, "Released"), .. Members(prelude, "release")];
        Retains = Members(products, "Replace");
        Lifts = Members(types.GetOrCreateTypeByMetadataName("LanguageExt.IO"), "lift", "liftAsync", "liftVAsync", "pure");
        Conversions = [.. Members(prelude, "Optional"), .. Members(types.GetOrCreateTypeByMetadataName("LanguageExt.Option`1"), "ToFin")];
    }

    public IMethodSymbol Dispose { get; }

    public ImmutableArray<IMethodSymbol> Acquires { get; }

    public ImmutableArray<IMethodSymbol> Releases { get; }

    public ImmutableArray<IMethodSymbol> Transfers { get; }

    public ImmutableArray<IMethodSymbol> Retains { get; }

    public ImmutableArray<IMethodSymbol> Lifts { get; }

    public ImmutableArray<IMethodSymbol> Conversions { get; }

    public static Lifetimes? Of(Compilation compilation) =>
        compilation.GetSpecialType(SpecialType.System_IDisposable).GetMembers(WellKnownMemberNames.DisposeMethodName).OfType<IMethodSymbol>().FirstOrDefault() is { } dispose
            ? new Lifetimes(compilation, dispose)
            : null;

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
        HashSet<(IOperation Value, ImmutableArray<int> Path)> visited = [with(new PathComparer<IOperation>(EqualityComparer<IOperation>.Default))];
        HashSet<(ISymbol Value, ImmutableArray<int> Path)> carriers = [with(new PathComparer<ISymbol>(SymbolEqualityComparer.Default))];
        IEnumerable<IOperation> Follow(ISymbol carrier, IOperation site, ImmutableArray<int> path) =>
            carriers.Add((carrier, path)) ? References(carrier, site, reading) : [];
        IEnumerable<IOperation> Pass(IInvocationOperation call, ImmutableArray<int> path, ITypeSymbol carried) =>
            Handed(call, path.IsEmpty ? held : carried).SelectMany(parameter => Follow(parameter, call, path));
        while (pending.Count is > 0) {
            reading.Token.ThrowIfCancellationRequested();
            (IOperation current, ImmutableArray<int> path, ITypeSymbol carried) = pending.Dequeue();
            if (!visited.Add((current, path))) continue;
            ISymbol? range = !current.IsImplicit && Clause(current.Syntax) is { } clause ? reading.For(clause.SyntaxTree)?.GetDeclaredSymbol(clause, reading.Token) : null;
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
