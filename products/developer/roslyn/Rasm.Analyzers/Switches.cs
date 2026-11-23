using System.Collections.Concurrent;
using System.Collections.Immutable;
using Analyzer.Utilities;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Operations;

namespace Rasm.Analyzers;

// --- [COMPOSITION] ---------------------------------------------------------------------

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class CatchAllArmAnalyzer : DiagnosticAnalyzer {

    // --- [RULES]

    private static readonly DiagnosticDescriptor Closed = new(
        "RASM1101",
        "Catch-all arm over smart enum or union, call `Switch` or `Map`",
        "Catch-all arm over `{0}` hides unhandled cases, call generated `Switch` or `Map` with one argument per case",
        Categories.Thinktecture,
        DiagnosticSeverity.Error,
        isEnabledByDefault: true,
        "Switches over Thinktecture unions or smart enums with an arm naming a case hold no catch-all arm.",
        Link("RASM1101"));

    private static readonly DiagnosticDescriptor Covered = new(
        "RASM3002",
        "Catch-all arm over covered enum, delete the arm",
        "Catch-all arm over `{0}` matches no declared member, delete the arm",
        Categories.CSharp,
        DiagnosticSeverity.Error,
        isEnabledByDefault: true,
        "Switch expressions over non-`[Flags]` enums with arms naming every member hold no catch-all arm.",
        Link("RASM3002"));

    private static readonly DiagnosticDescriptor Fallback = new(
        "RASM3004",
        "Catch-all arm or fallback branch over enum, name each member",
        "Catch-all arm or fallback branch over `{0}` maps {1} to one result, name {1} in switch arms joined by `or` per shared result",
        Categories.CSharp,
        DiagnosticSeverity.Error,
        isEnabledByDefault: true,
        "Switch expressions over non-`[Flags]` enums name every member in arms joined by `or` with no catch-all arm, and `==` or `is` chains over one enum value end in no fallback branch once two tests or all members but one are named.",
        Link("RASM3004"));

    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => [Closed, Covered, Fallback];

    public override void Initialize(AnalysisContext context) {
        context.EnableConcurrentExecution();
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
        context.RegisterCompilationStartAction(static start => {
            WellKnownTypeProvider types = WellKnownTypeProvider.GetOrCreate(start.Compilation);
            INamedTypeSymbol? flags = types.GetOrCreateTypeByMetadataName("System.FlagsAttribute");
            INamedTypeSymbol? adHoc = types.GetOrCreateTypeByMetadataName("Thinktecture.UnionAttributeBase");
            INamedTypeSymbol? keyed = types.GetOrCreateTypeByMetadataName("Thinktecture.SmartEnumAttribute`1");
            ImmutableArray<INamedTypeSymbol> marks = [.. new[] {
                adHoc, types.GetOrCreateTypeByMetadataName("Thinktecture.UnionAttribute"), types.GetOrCreateTypeByMetadataName("Thinktecture.SmartEnumAttribute"), keyed,
            }.OfType<INamedTypeSymbol>()];
            ConcurrentDictionary<ITypeSymbol, AttributeData?> attributes = new(SymbolEqualityComparer.Default);
            AttributeData? Mark(ITypeSymbol type) => attributes.GetOrAdd(type, held => held.GetAttributes().FirstOrDefault(data => marks.Any(mark => Derives(data.AttributeClass, mark))));
            INamedTypeSymbol? Owned(ITypeSymbol? type, CancellationToken token) {
                Stack<ITypeSymbol> pending = new(type is null ? [] : [type]);
                while (pending.Count is > 0) {
                    ITypeSymbol current = pending.Pop();
                    token.ThrowIfCancellationRequested();
                    if (current is INamedTypeSymbol { IsTupleType: true } tuple)
                        for (int index = tuple.TupleElements.Length - 1; index >= 0; index--) pending.Push(tuple.TupleElements[index].Type);
                    else if (current is INamedTypeSymbol { OriginalDefinition.SpecialType: SpecialType.System_Nullable_T } nullable) pending.Push(nullable.TypeArguments[0]);
                    else if (current is INamedTypeSymbol named && Mark(named) is not null) return named;
                }
                return null;
            }
            (INamedTypeSymbol Owner, Func<IOperation, bool> Case)? Union(IOperation subject, CancellationToken token) =>
                Stripped(subject) switch {
                    IMemberReferenceOperation { Instance.Type: INamedTypeSymbol owner, Member: var member }
                        when keyed is not null && Mark(owner) is { AttributeClass: { TypeArguments.Length: 1 } attribute } mark && Same(attribute, keyed)
                            && member is IPropertySymbol or IFieldSymbol && string.Equals(member.Name, Key(mark), StringComparison.Ordinal) =>
                        (owner, static test => test is IConstantPatternOperation or IRelationalPatternOperation || (test is not IPatternOperation && test.ConstantValue.HasValue)),
                    IPropertyReferenceOperation { Instance.Type: INamedTypeSymbol owner, Property.Type.SpecialType: SpecialType.System_Object }
                        when adHoc is not null && Mark(owner) is { AttributeClass: { } union } mark && Derives(union, adHoc)
                            && union.TypeArguments.AddRange(mark.ConstructorArguments.Select(static held => held.Value).OfType<ITypeSymbol>()) is { IsEmpty: false } cases =>
                        (owner, test => test is IPatternOperation { NarrowedType: var narrowed } && cases.Any(held => SymbolEqualityComparer.Default.Equals(held, narrowed))),
                    _ when Owned(subject.Type, token) is { } owner => (owner, test => test switch {
                        IPatternOperation { NarrowedType: var narrowed } => !Same(narrowed, owner) && Derives(narrowed, owner),
                        IFieldReferenceOperation { Field: { IsStatic: true, Type: var item } } => Same(item, owner),
                        _ => false,
                    }),
                    _ => null,
                };
            static IEnumerable<object?> Constants(IOperation? test, CancellationToken token) {
                Stack<IOperation> remaining = new(test is null ? [] : [test]);
                while (remaining.Count is not 0) {
                    token.ThrowIfCancellationRequested();
                    IOperation pattern = remaining.Pop();
                    if (pattern is IConstantPatternOperation { Value.ConstantValue: { HasValue: true } constant }) yield return constant.Value;
                    foreach (IPatternOperation child in pattern.ChildOperations.OfType<IPatternOperation>()
                        .Where(static child => child.Parent is IBinaryPatternOperation { OperatorKind: BinaryOperatorKind.Or })) remaining.Push(child);
                }
            }
            INamedTypeSymbol? Exhaustible(ITypeSymbol? type) =>
                type is INamedTypeSymbol { TypeKind: TypeKind.Enum } subject && Sourced(subject) && (flags is null || Marked(subject, flags) is null) ? subject : null;
            static ImmutableArray<IFieldSymbol> Unnamed(INamedTypeSymbol subject, IReadOnlyCollection<object?> named) =>
                [.. subject.GetMembers().OfType<IFieldSymbol>().Where(member => member.HasConstantValue && !named.Contains(member.ConstantValue))];
            static string Listed(INamedTypeSymbol subject, ImmutableArray<IFieldSymbol> unnamed) => string.Join(", ", unnamed.Select(member => $"`{subject.Name}.{member.Name}`"));
            IEnumerable<Diagnostic> Enumerated(IOperation operation, ImmutableArray<(IOperation Arm, IOperation? Test, IOperation? Guard)> arms, CancellationToken token) {
                if (operation is not ISwitchExpressionOperation { Value.Type: var type } || Exhaustible(type) is not { } subject) return [];
                ImmutableHashSet<object?> named = [.. arms.Where(static arm => arm.Guard is null).SelectMany(arm => Constants(arm.Test, token))];
                ImmutableArray<IFieldSymbol> unnamed = Unnamed(subject, named);
                return named.IsEmpty ? [] : arms.Where(arm => CatchesAll(arm, token) || (arm is { Guard: null, Test: INegatedPatternOperation { Pattern: var excluded } } && Constants(excluded, token).Any()))
                    .Select(arm => Diagnostic.Create(unnamed.IsEmpty ? Covered : Fallback, arm.Arm.Syntax.GetLocation(), subject.Name, Listed(subject, unnamed)));
            }
            IEnumerable<Diagnostic> Chained(IConditionalOperation top, CancellationToken token) {
                if (Tested(top.Condition) is not ( { } opened, _) || Exhaustible(opened.Type) is not { } subject) return [];
                ImmutableArray<object?>? Link(IOperation? operation) =>
                    operation is IConditionalOperation link && Tested(link.Condition) is ( { } tested, var named)
                    && SymbolEqualityComparer.Default.Equals(tested.Type, subject) && tested.Syntax.IsEquivalentTo(opened.Syntax) ? named : null;
                if ((top.Parent is IConditionalOperation outer && outer.WhenFalse == top && Link(outer) is not null)
                    || (Sibling(top, -1) is IConditionalOperation { WhenFalse: null } previous && Exits(previous.WhenTrue) && Link(previous) is not null)) return [];
                HashSet<object?> named = [];
                int tests = 0;
                IOperation? fallback = top;
                while (fallback is IConditionalOperation link && Link(link) is { } more) {
                    token.ThrowIfCancellationRequested();
                    named.UnionWith(more);
                    tests++;
                    fallback = link.WhenFalse ?? (Exits(link.WhenTrue) ? Sibling(link, 1) : null);
                }
                ImmutableArray<IFieldSymbol> unnamed = Unnamed(subject, named);
                return fallback is null || unnamed.IsEmpty || (tests < 2 && unnamed.Length is not 1) ? []
                    : [Diagnostic.Create(Fallback, fallback.Syntax.GetLocation(), subject.Name, Listed(subject, unnamed))];
            }
            Report<IConditionalOperation>(start, [OperationKind.Conditional], (conditional, analysis) => Chained(conditional, analysis.CancellationToken));
            Report<IOperation>(start, [OperationKind.SwitchExpression, OperationKind.Switch], (operation, analysis) =>
                Dispatch(operation) is ( { } subject, var arms)
                    ? Enumerated(operation, arms, analysis.CancellationToken).Concat(Union(subject, analysis.CancellationToken) is ( { } owner, var names)
                        && arms.Any(arm => !CatchesAll(arm, analysis.CancellationToken) && ((arm.Test is { } test && test.DescendantsAndSelf().Any(names)) || (arm.Guard is { } guard && guard.DescendantsAndSelf().Any(names))))
                            ? arms.Where(arm => CatchesAll(arm, analysis.CancellationToken)).Select(arm => Diagnostic.Create(Closed, arm.Arm.Syntax.GetLocation(), owner.Name))
                            : [])
                    : []);
        });
    }

    // --- [BRANCHES]

    private static (IOperation Subject, ImmutableArray<object?> Named)? Tested(IOperation condition) =>
        Stripped(condition) switch {
            IBinaryOperation { OperatorKind: BinaryOperatorKind.Equals } binary => (Stripped(binary.LeftOperand), Stripped(binary.RightOperand)) switch {
                ( { ConstantValue.HasValue: false } subject, { ConstantValue: { HasValue: true } constant }) => (subject, [constant.Value]),
                ( { ConstantValue: { HasValue: true } constant }, { ConstantValue.HasValue: false } subject) => (subject, [constant.Value]),
                _ => null,
            },
            IIsPatternOperation { Value: var subject, Pattern: var pattern } when Disjunction(pattern) is { IsEmpty: false } named => (Stripped(subject), named),
            _ => null,
        };

    private static ImmutableArray<object?> Disjunction(IPatternOperation pattern) =>
        pattern switch {
            IConstantPatternOperation { Value.ConstantValue: { HasValue: true } constant } => [constant.Value],
            IBinaryPatternOperation { OperatorKind: BinaryOperatorKind.Or } either when Disjunction(either.LeftPattern) is { IsEmpty: false } left
                && Disjunction(either.RightPattern) is { IsEmpty: false } right => left.AddRange(right),
            _ => [],
        };

    private static IOperation? Sibling(IOperation link, int offset) =>
        link.Parent is IBlockOperation { Operations: var steps } && steps.IndexOf(link) + offset is var at and >= 0 && at < steps.Length ? steps[at] : null;

    private static bool Exits(IOperation? branch) =>
        branch is IReturnOperation or IThrowOperation or IBranchOperation or IExpressionStatementOperation { Operation: IThrowOperation }
        || (branch is IBlockOperation { Operations: { IsEmpty: false } steps } && Exits(steps[steps.Length - 1]));

    // --- [ARMS]

    private static bool CatchesAll((IOperation Arm, IOperation? Test, IOperation? Guard) arm, CancellationToken cancellation) =>
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
}
