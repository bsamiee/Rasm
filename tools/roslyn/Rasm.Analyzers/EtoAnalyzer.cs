using System.Collections.Concurrent;
using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Operations;

namespace Rasm.Analyzers;

// --- [COMPOSITION] ---------------------------------------------------------------------

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class EtoAnalyzer : RasmAnalyzer {

    // --- [RULES]

    private const string Category = "Eto";

    public static readonly DiagnosticDescriptor Cached = Rule(
        "RASM2201",
        Category,
        "Cached system font disposed, read without release",
        "`{1}` disposes font `SystemFonts.{0}` caches for every caller, read the font without a release",
        "`SystemFonts` fonts reach no `Dispose`, `using`, `use`, `release`, acquire, or retaining call, directly or through bindings",
        WellKnownDiagnosticTags.CompilationEnd);

    public static readonly DiagnosticDescriptor Framed = Rule(
        "RASM2202",
        Category,
        "Eto container acquired by plain dispose, detach children before `Dispose`",
        "`{0}` disposes `{1}` with children attached, acquire `{1}` through `use` with a release detaching children before `Dispose`",
        "Plain-dispose acquires and retaining calls of `Eto.Forms.Container` detach earlier-held or `StackLayout` children before `Dispose`",
        WellKnownDiagnosticTags.CompilationEnd);

    public static readonly DiagnosticDescriptor Twice = Rule(
        "RASM2203",
        Category,
        "Retained control transferred, pass it without `Transfer`",
        "`{0}` disposes `{1}` on failure after `{2}` handed `{1}` to an owner, pass `{1}` to its consumer without `Transfer`",
        "`Products.Transfer` takes no `Eto.Widget` value a retaining call handed to an owner earlier on one path",
        WellKnownDiagnosticTags.CompilationEnd);

    public static readonly DiagnosticDescriptor Escaped = Rule(
        "RASM2204",
        Category,
        "Native callback or Eto override reading host outside `Session`, route through `Session`",
        "`{1}` in native callback `{0}` runs outside `Session` and an exception aborts Rhino, route through `Session.Run(<effect>, <failed>)` with `[CallerMemberName]`",
        "Nonempty native callbacks (Objective-C, `AddMethod`, `[UnmanagedFunctionPointer]`, `[Export]`, `[UnmanagedCallersOnly]`, `NSObject`) are one `Session` call or a constant return, Eto overrides read host members inside `Session`",
        WellKnownDiagnosticTags.CompilationEnd);

    public static readonly DiagnosticDescriptor Handler = Rule(
        "RASM2205",
        Category,
        "Delegate registered as platform handler, declare a handler interface",
        "`{0}` registers delegate `{1}` as a platform handler, declare one handler interface each platform implements",
        "`Eto.Platform.Add<T>` and `Find<T>` take as `T` a handler interface each platform implements in place of a delegate");

    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => [Cached, Framed, Twice, Escaped, Handler];

    protected override void Start(CompilationStartAnalysisContext context) {
        Compilation compilation = context.Compilation;
        (SystemSymbols system, EtoSymbols eto, RasmRhinoSymbols rasm) = (new(compilation), new(compilation), new(compilation));
        SemanticModels models = new(context);
        ControlFlow flow = ControlFlow.For(compilation);
        if (!eto.Handlers.IsEmpty) Handlers(context, eto.Handlers);
        if (rasm.Session is { } callbacks) Natives(context, models, flow, system, eto, new MacSymbols(compilation), rasm, callbacks);
        if (Lifetimes.Of(system, rasm, new LanguageExtSymbols(compilation)) is not { } lifetimes) return;
        if (eto is { SystemFonts: { } fonts, Font: { } font }) Fonts(models, lifetimes, fonts, font);
        if (eto is { Container: { } container, Control: { } control }) Frames(models, lifetimes, container, control, eto.StackLayout);
        if (eto.Widget is { } widget && !lifetimes.Transfers.IsEmpty) Transfers(models, flow, lifetimes, widget);
    }

    // --- [FONTS]

    private static void Fonts(SemanticModels models, Lifetimes lifetimes, INamedTypeSymbol fonts, INamedTypeSymbol font) =>
        models.Report<IInvocationOperation>([OperationKind.Invocation], (invocation, _, reading) =>
            Same(invocation.TargetMethod.ContainingType, fonts) && Same(invocation.Type, font)
            && lifetimes.Released(invocation, font, reading).FirstOrDefault() is { } release
                ? [Diagnostic.Create(Cached, invocation.Syntax.GetLocation(), invocation.TargetMethod.Name, release)]
                : []);

    // --- [FRAMES]

    private enum ControlReach { Type, Properties, Collection }

    private sealed class ControlFactComparer : IEqualityComparer<(ITypeSymbol Type, ControlReach Reach)> {
        public bool Equals((ITypeSymbol Type, ControlReach Reach) x, (ITypeSymbol Type, ControlReach Reach) y) =>
            x.Reach == y.Reach && SymbolEqualityComparer.Default.Equals(x.Type, y.Type);

        public int GetHashCode((ITypeSymbol Type, ControlReach Reach) obj) => SymbolEqualityComparer.Default.GetHashCode(obj.Type) ^ obj.Reach.GetHashCode();
    }

    private static void Frames(SemanticModels models, Lifetimes lifetimes, INamedTypeSymbol container, INamedTypeSymbol control, INamedTypeSymbol? stack) {
        ImmutableArray<IMethodSymbol> plain = [.. lifetimes.Acquires.Where(lifetimes.Plain)];
        ControlFactComparer comparer = new();
        ImmutableHashSet<(ITypeSymbol Type, ControlReach Reach)> empty = ImmutableHashSet.Create<(ITypeSymbol Type, ControlReach Reach)>(comparer);
        ImmutableHashSet<(ITypeSymbol Type, ControlReach Reach)> truth = empty.Add((control, ControlReach.Type));
        ConcurrentDictionary<(ITypeSymbol Type, ControlReach Reach), ImmutableHashSet<(ITypeSymbol Type, ControlReach Reach)>> completed = new(comparer);
        ConcurrentDictionary<ITypeSymbol, bool> holdings = new(SymbolEqualityComparer.Default);
        bool Holding(ITypeSymbol type, CancellationToken token) => holdings.GetOrAdd(type, source => {
            Dictionary<(ITypeSymbol Type, ControlReach Reach), ImmutableHashSet<(ITypeSymbol Type, ControlReach Reach)>> equations = [with(comparer)];
            ImmutableHashSet<(ITypeSymbol Type, ControlReach Reach)> Formula((ITypeSymbol Type, ControlReach Reach) key) {
                if (completed.TryGetValue(key, out ImmutableHashSet<(ITypeSymbol Type, ControlReach Reach)>? solved)) return solved;
                if (equations.TryGetValue(key, out ImmutableHashSet<(ITypeSymbol Type, ControlReach Reach)>? current)) return current;
                equations.Add(key, empty);
                return empty;
            }
            ImmutableHashSet<(ITypeSymbol Type, ControlReach Reach)> Evaluate(ITypeSymbol input, ControlReach reach) {
                ImmutableHashSet<(ITypeSymbol Type, ControlReach Reach)>.Builder result = empty.ToBuilder();
                HashSet<(ITypeSymbol Type, ControlReach Reach)> seen = [with(comparer)];
                Queue<(ITypeSymbol Type, ControlReach Reach)> pending = new([(input, reach)]);
                while (pending.Count is > 0) {
                    token.ThrowIfCancellationRequested();
                    (ITypeSymbol current, ControlReach mode) = pending.Dequeue();
                    if (!seen.Add((current, mode))) continue;
                    if (mode is ControlReach.Type) {
                        if (Derives(current, control)) return truth;
                        if (current is ITypeParameterSymbol) _ = result.Add((current, mode));
                        continue;
                    }
                    ImmutableHashSet<(ITypeSymbol Type, ControlReach Reach)> formula = Formula((current.OriginalDefinition, mode));
                    if (formula.Contains((control, ControlReach.Type))) return truth;
                    foreach ((ITypeSymbol parameter, ControlReach required) in formula) {
                        ITypeSymbol argument = parameter is ITypeParameterSymbol variable
                            && Containers(current).OfType<INamedTypeSymbol>().FirstOrDefault(part => Same(part, variable.ContainingSymbol)) is { } owner
                                ? owner.TypeArguments[variable.Ordinal] : parameter;
                        if (SymbolEqualityComparer.Default.Equals(argument, parameter)) _ = result.Add((parameter, required));
                        else pending.Enqueue((argument, required));
                    }
                }
                return result.ToImmutable();
            }
            _ = Evaluate(source, ControlReach.Collection);
            bool changed;
            do {
                token.ThrowIfCancellationRequested();
                int count = equations.Count;
                changed = false;
                foreach ((ITypeSymbol owner, ControlReach mode) in equations.Keys.ToImmutableArray()) {
                    ImmutableHashSet<(ITypeSymbol Type, ControlReach Reach)> result = owner is ITypeParameterSymbol ? empty.Add((owner, mode)) : empty;
                    result = mode is ControlReach.Collection
                        ? result.Union(owner.AllInterfaces.Append(owner).OfType<INamedTypeSymbol>()
                            .Where(static face => face.OriginalDefinition.SpecialType is SpecialType.System_Collections_Generic_IEnumerable_T)
                            .SelectMany(face => Evaluate(face.TypeArguments[0], ControlReach.Properties)))
                        : result.Union(Evaluate(owner, ControlReach.Type)).Union(Bases(owner).SelectMany(static type => type.GetMembers()).OfType<IPropertySymbol>()
                            .SelectMany(property => Evaluate(property.Type, ControlReach.Type).Union(Evaluate(property.Type, ControlReach.Collection))));
                    if (equations[(owner, mode)].SetEquals(result)) continue;
                    equations[(owner, mode)] = result;
                    changed = true;
                }
                _ = Evaluate(source, ControlReach.Collection);
                changed |= count != equations.Count;
            } while (changed);
            foreach (KeyValuePair<(ITypeSymbol Type, ControlReach Reach), ImmutableHashSet<(ITypeSymbol Type, ControlReach Reach)>> equation in equations) _ = completed.TryAdd(equation.Key, equation.Value);
            return Evaluate(source, ControlReach.Collection).Contains((control, ControlReach.Type));
        });
        IEnumerable<IOperation> Initialized(IOperation? initializer, CancellationToken token) =>
            initializer is IObjectOrCollectionInitializerOperation members
                ? members.Initializers.SelectMany(member => member switch {
                    ISimpleAssignmentOperation { Target: IPropertyReferenceOperation property } assignment when Derives(property.Property.Type, control) => [assignment.Value],
                    IMemberInitializerOperation { InitializedMember: IPropertyReferenceOperation property } nested when Holding(property.Property.Type, token) => [nested.Initializer],
                    _ => Enumerable.Empty<IOperation>(),
                })
                : [];
        IEnumerable<IOperation> Created(IOperation? origin, CancellationToken token) =>
            origin is null ? [] : origin.DescendantsAndSelf().OfType<IObjectCreationOperation>().Where(creation => Derives(creation.Type, container)).SelectMany(creation => Initialized(creation.Initializer, token));
        static IEnumerable<IOperation> Paired(IPropertyReferenceOperation property, IDeconstructionAssignmentOperation deconstruction) =>
            property.Parent is ITupleOperation targets && Stripped(deconstruction.Value) is ITupleOperation values && targets.Elements.Length == values.Elements.Length
                ? [values.Elements[targets.Elements.IndexOf(property)]]
                : [deconstruction.Value];
        static IEnumerable<IOperation> Assigned(IPropertyReferenceOperation property) =>
            Consumer(property) is ISimpleAssignmentOperation assignment && assignment.Target == property ? [assignment.Value]
            : property.Parent is ITupleOperation && Ancestor<IDeconstructionAssignmentOperation>(property, assignment => assignment.Target.DescendantsAndSelf().Contains(property)) is { } deconstruction
                ? Paired(property, deconstruction)
            : [];
        static IEnumerable<IOperation> Grouped(IOperation property) =>
            Consumer(property) switch {
                IInvocationOperation call => call.Arguments.Select(static argument => argument.Value),
                IMethodReferenceOperation reference when Consumer(reference) is IDelegateCreationOperation creation && Consumer(creation) is IArgumentOperation { Parent: IInvocationOperation call }
                    && Receiver(call) is { } items => [items],
                _ => [],
            };
        IEnumerable<IOperation> Attached(IOperation reference, Reading reading) =>
            Consumer(reference) switch {
                IPropertyReferenceOperation property when Derives(property.Property.Type, control) => Assigned(property),
                IPropertyReferenceOperation property when Holding(property.Property.Type, reading.Token) => Grouped(property),
                IInvocationOperation call => call.Arguments.Select(static argument => argument.Value).Where(value => Derives(Stripped(value).Type, control)),
                _ => [],
            };
        static bool Earlier(ISymbol symbol, int start) =>
            symbol is IParameterSymbol { ContainingSymbol: IMethodSymbol { MethodKind: not MethodKind.AnonymousFunction } }
            || symbol.DeclaringSyntaxReferences.Any(declared => declared.Span.Start < start);
        bool Hazard(IOperation child, ITypeSymbol held, int start, Reading reading) =>
            (stack is not null && Derives(held, stack)) || child.DescendantsAndSelf().Select(value => Held(value, reading)).Any(symbol => symbol is not null && Earlier(symbol, start));
        static ISymbol? Holder(IInvocationOperation call, ITypeSymbol held, Reading reading) =>
            Clause(call.Syntax) is { } clause && reading.Model.GetDeclaredSymbol(clause, reading.Token) is { } range ? range
            : Consumer(call) is IInvocationOperation outer && Lifetimes.Handed(outer, held).FirstOrDefault() is { } chained ? chained
            : Lifetimes.Handed(call, held).FirstOrDefault();
        IEnumerable<IOperation> Children(ISymbol? holder, IOperation site, IOperation? origin, Reading reading) =>
            Created(origin, reading.Token).Concat(new[] { holder, origin is IArgumentOperation { Value: var handed } ? Held(handed, reading) : null }
                .OfType<ISymbol>()
                .Distinct(SymbolEqualityComparer.Default)
                .SelectMany(held => References(held, site, reading).SelectMany(reference => Attached(reference, reading)).Concat(held is ILocalSymbol local ? Bindings(local, reading).SelectMany(bound => Created(bound, reading.Token)) : [])));
        (string Owner, ITypeSymbol Held, ISymbol? Holder, IOperation? Origin)? Acquired(IInvocationOperation call, Reading reading) =>
            Among(call.TargetMethod, plain)
                ? (Label(call), call.TargetMethod.TypeArguments[0], Holder(call, call.TargetMethod.TypeArguments[0], reading), call.Arguments.FirstOrDefault(static argument => argument.Parameter?.Ordinal is 0))
            : call.Arguments is { Length: 1 } arguments && arguments[0] is { Parameter: { } parameter, Value: var value } && lifetimes.Retaining(call, parameter) && Stripped(value).Type is { } handed
                ? (Label(call), handed, Held(value, reading), arguments[0])
            : null;
        models.Report<IInvocationOperation>([OperationKind.Invocation], (call, _, reading) =>
            Acquired(call, reading) is { } acquired && Derives(acquired.Held, container)
            && Children(acquired.Holder, call, acquired.Origin, reading).Any(child => Hazard(child, acquired.Held, call.Syntax.SpanStart, reading))
                ? [Diagnostic.Create(Framed, call.Syntax.GetLocation(), acquired.Owner, acquired.Held.Name)]
                : []);
    }

    // --- [TRANSFERS]

    private static void Transfers(SemanticModels models, ControlFlow flow, Lifetimes lifetimes, INamedTypeSymbol widget) {
        IEnumerable<IOperation> Origins(ISymbol held, Reading reading) =>
            held switch {
                IRangeVariableSymbol or ILocalSymbol => Bindings(held, reading),
                IParameterSymbol { Ordinal: 0, ContainingSymbol: IMethodSymbol { MethodKind: MethodKind.AnonymousFunction } function }
                    when Declarations(function, reading).FirstOrDefault() is { } lambda && Consumer(lambda) is IDelegateCreationOperation creation
                        && Consumer(creation) is IArgumentOperation { Parent: IInvocationOperation call } argument =>
                    new[] { call.Instance ?? (call.TargetMethod.IsExtensionMethod && argument.Parameter?.Ordinal is not 0 ? Receiver(call) : null) }.OfType<IOperation>(),
                _ => [],
            };
        string? Source(ISymbol held, Reading reading) {
            HashSet<ISymbol> seen = [with(SymbolEqualityComparer.Default), held];
            Stack<IOperation> pending = new(Origins(held, reading));
            while (pending.Count is > 0) {
                IOperation value = Stripped(pending.Pop());
                reading.Token.ThrowIfCancellationRequested();
                if (value is IInvocationOperation call && Among(call.TargetMethod, lifetimes.Retains)) return Label(call);
                if (value is ITranslatedQueryOperation { Syntax: QueryExpressionSyntax { Body.SelectOrGroup: SelectClauseSyntax select } }
                    && reading.For(select.SyntaxTree).GetOperation(select.Expression, reading.Token) is { } result) pending.Push(result);
                else if (Held(value, reading) is { } bound && seen.Add(bound))
                    foreach (IOperation origin in Origins(bound, reading)) pending.Push(origin);
            }
            return null;
        }
        string? Retained(ISymbol held, IOperation transfer, Reading reading) {
            IOperation? transferScope = Ancestor<IOperation>(transfer, static operation => operation is IAnonymousFunctionOperation or ILocalFunctionOperation);
            bool Reaches(IInvocationOperation retain) {
                IOperation? retainScope = Ancestor<IOperation>(retain, static operation => operation is IAnonymousFunctionOperation or ILocalFunctionOperation);
                IOperation? within = retainScope == transferScope ? transfer : Ancestor<ITranslatedQueryOperation>(retain, query => query.Syntax.Span.Contains(transfer.Syntax.Span));
                return within is not null && flow.Precedes(retain, transfer, within, reading.Token);
            }
            return References(held, transfer, reading)
                .Select(static reference => Consumer(reference)).OfType<IArgumentOperation>()
                .Select(argument => argument is { Parent: IInvocationOperation call, Parameter: { } parameter } && lifetimes.Retaining(call, parameter) ? call : null)
                .OfType<IInvocationOperation>().FirstOrDefault(Reaches) is { } retained ? Label(retained) : null;
        }
        models.Report<IInvocationOperation>([OperationKind.Invocation], (transfer, _, reading) =>
            Among(transfer.TargetMethod, lifetimes.Transfers)
            && transfer.Arguments.FirstOrDefault(static argument => argument.Parameter?.Ordinal is 0) is { Value: var value } && Derives(Carried(value).Type, widget)
            && Held(value, reading) is { } held && (Retained(held, transfer, reading) ?? Source(held, reading)) is { } owner
                ? [Diagnostic.Create(Twice, transfer.Syntax.GetLocation(), Label(transfer), held.Name, owner)]
                : []);
    }

    // --- [NATIVES]

    private static void Natives(CompilationStartAnalysisContext context, SemanticModels models, ControlFlow flow, SystemSymbols system, EtoSymbols eto, MacSymbols mac, RasmRhinoSymbols rasm, INamedTypeSymbol callbacks) {
        (INamedTypeSymbol? register, INamedTypeSymbol? export, INamedTypeSymbol? pointer, INamedTypeSymbol? callers, INamedTypeSymbol? widget) = (mac.Register, mac.Export, system.FunctionPointer, system.CallersOnly, eto.Widget);
        (ImmutableArray<IMethodSymbol> boundaries, ImmutableArray<IMethodSymbol> adders) = (rasm.Boundaries, eto.AddMethod);
        bool Wrapper(INamedTypeSymbol type) =>
            Marked(type, register) is { } data
            && ((data.ConstructorArguments.Length > 1 && data.ConstructorArguments[1].Value is true) || data.NamedArguments.Any(static named => named.Value.Value is true && string.Equals(named.Key, "IsWrapper", StringComparison.Ordinal)));
        ConcurrentDictionary<INamedTypeSymbol, bool> wrappers = new(SymbolEqualityComparer.Default);
        bool Bound(INamedTypeSymbol? type) => type is not null && wrappers.GetOrAdd(type, owner => Containers(owner).OfType<INamedTypeSymbol>().Any(Wrapper));
        bool Wrapped(IMethodSymbol method) => method.IsOverride && Bound(Overridden(method).Last().ContainingType);
        bool Overriding(IMethodSymbol method, INamedTypeSymbol? root) =>
            root is not null && method.IsOverride && Overridden(method).Last() is var origin && !Sourced(origin) && Derives(origin.ContainingType, root);
        ConcurrentDictionary<IMethodSymbol, IOperation?> bodies = new(SymbolEqualityComparer.Default);
        IOperation? Bodied(IMethodSymbol method, Reading reading) => bodies.GetOrAdd(method.OriginalDefinition, held => Declarations(held, reading).Select(static declared => declared switch {
            IMethodBodyOperation body => body.BlockBody ?? body.ExpressionBody,
            ILocalFunctionOperation local => local.Body,
            IAnonymousFunctionOperation function => function.Body,
            _ => null,
        }).FirstOrDefault(static body => body is not null));
        bool Routed(IOperation step, Reading reading) {
            HashSet<IMethodSymbol> seen = [with(SymbolEqualityComparer.Default)];
            while (step is IInvocationOperation call) {
                reading.Token.ThrowIfCancellationRequested();
                if (Among(call.TargetMethod, boundaries)) return true;
                if (!seen.Add(call.TargetMethod.OriginalDefinition) || Bodied(call.TargetMethod, reading) is not IBlockOperation body || Sole(body) is not { } inner) return false;
                step = inner;
            }
            return step.ConstantValue.HasValue || step is IDefaultValueOperation;
        }
        static bool Rooted(IOperation? instance) => instance is not null && Stripped(instance) is IInstanceReferenceOperation;
        static bool Inherited(ISymbol member) => !Sourced(member) && member.ContainingType is { SpecialType: not SpecialType.System_Object };
        static bool Read(IOperation operation) =>
            operation switch {
                IMethodReferenceOperation => false,
                IMemberReferenceOperation member => Rooted(member.Instance) && Inherited(member.Member),
                IInvocationOperation call => Rooted(call.Instance) && Inherited(call.TargetMethod),
                _ => false,
            };
        IEnumerable<(IOperation Site, string Run)> Eager(IOperation? body, Reading reading) =>
            body is null ? [] : ControlFlow.Operations(body, reading.Token).OfType<IInvocationOperation>()
                .Select(call => (Site: call, Reads: call.Arguments.SelectMany(argument => ControlFlow.Operations(argument.Value, reading.Token)).Where(Read).Select(static read => read.Syntax.ToString()).Distinct(StringComparer.Ordinal).ToImmutableArray()))
                .Where(eager => !eager.Reads.IsEmpty && (Among(eager.Site.TargetMethod, boundaries) || Routed(eager.Site, reading)))
                .Select(static eager => ((IOperation)eager.Site, string.Join("`, `", eager.Reads)));
        static string Shown(IOperation step) =>
            step switch {
                IInvocationOperation call => Label(call),
                IVariableDeclarationGroupOperation declared when declared.DescendantsAndSelf().OfType<IInvocationOperation>().FirstOrDefault() is { } call => Label(call),
                _ => step.Syntax.ToString().Split('\n')[0].Trim(),
            };
        IEnumerable<(IOperation Site, string Run)> Escapes(IOperation? body, Reading reading) =>
            body is IBlockOperation block && Steps(block) is { IsEmpty: false } steps && steps.FirstOrDefault(step => !Routed(step, reading)) is var unrouted
            && (steps.Length is not 1 || unrouted is not null) && (unrouted ?? block) is var site ? [(site, Shown(site))] : [];
        IEnumerable<(IOperation Site, string Run)> Checked(IOperation? body, Reading reading) => Escapes(body, reading).Concat(Eager(body, reading));
        IEnumerable<(string Callback, IOperation Value)> Handed(IOperation operation) =>
            operation switch {
                IDelegateCreationOperation { Type: { } type } creation when Marked(type, pointer) is not null =>
                    [(type.Name, creation)],
                IInvocationOperation call when Bound(call.TargetMethod.ContainingType) || Among(call.TargetMethod, adders) =>
                    call.Arguments.Where(argument => argument.Parameter?.Type is { TypeKind: TypeKind.Delegate } handed && Marked(handed, pointer) is null)
                        .Select(argument => (Label(call.TargetMethod), argument.Value)),
                _ => [],
            };
        models.Report<IOperation>([OperationKind.DelegateCreation, OperationKind.Invocation], (operation, owner, reading) =>
            !Within(owner, callbacks)
                ? Handed(operation).SelectMany(handed => flow.Targets(ControlFlow.At(handed.Value), reading.Token).OfType<IMethodSymbol>()
                    .SelectMany(method => Checked(Bodied(method, reading), reading))
                    .Select(escape => Diagnostic.Create(Escaped, escape.Site.Syntax.GetLocation(), handed.Callback, escape.Run)))
                : []);
        ConcurrentQueue<(IMethodSymbol Method, IOperation Root, Func<IOperation?, Reading, IEnumerable<(IOperation Site, string Run)>> Check)> blocks = new();
        context.RegisterOperationBlockAction(block => {
            if (block.IsGeneratedCode || block.OwningSymbol is not IMethodSymbol method || Within(method, callbacks)) return;
            Func<IOperation?, Reading, IEnumerable<(IOperation Site, string Run)>>? check =
                Marked(method, export) is not null || Marked(method, callers) is not null || Wrapped(method) ? Checked : Overriding(method, widget) ? Eager : null;
            if (check is not null && block.OperationBlocks.FirstOrDefault() is { } root) blocks.Enqueue((method, root, check));
        });
        models.Complete((end, provided) => {
            foreach ((IMethodSymbol method, IOperation root, Func<IOperation?, Reading, IEnumerable<(IOperation Site, string Run)>> check) in blocks) {
                Reading reading = new(root.SemanticModel!, provided, end.CancellationToken);
                IOperation? body = root is IMethodBodyOperation bodied ? bodied.BlockBody ?? bodied.ExpressionBody : root;
                foreach ((IOperation site, string run) in check(body, reading))
                    end.ReportDiagnostic(Diagnostic.Create(Escaped, site.Syntax.GetLocation(), method.Name, run));
            }
        });
    }

    // --- [HANDLERS]

    private static void Handlers(CompilationStartAnalysisContext context, ImmutableArray<IMethodSymbol> handlers) =>
        Report<IInvocationOperation>(context, [OperationKind.Invocation], (invocation, _) =>
            Among(invocation.TargetMethod, handlers) && invocation.TargetMethod.TypeArguments[0] is { TypeKind: TypeKind.Delegate } handed
                ? [Diagnostic.Create(Handler, Named(invocation), Label(invocation.TargetMethod), handed.Name)]
                : []);

    // --- [BINDINGS]

    private static IOperation Carried(IOperation reference) {
        IOperation value = Stripped(Lambda(reference) is { Symbol.Parameters.IsEmpty: true } factory && Sole(factory.Body) is { } made ? made : reference);
        while (value is IConversionOperation { OperatorMethod: null, Conversion: { IsIdentity: true } or { IsReference: true } } conversion) value = Stripped(conversion.Operand);
        return value;
    }

    private static ISymbol? Held(IOperation reference, Reading reading) =>
        Carried(reference) switch {
            { Syntax: IdentifierNameSyntax name } when reading.For(name.SyntaxTree).GetSymbolInfo(name, reading.Token).Symbol is { } held
                && held is IParameterSymbol or IRangeVariableSymbol => held,
            ILocalReferenceOperation local => local.Local,
            IParameterReferenceOperation parameter => parameter.Parameter,
            _ => null,
        };

    private static IEnumerable<IOperation> Bindings(ISymbol symbol, Reading reading) =>
        symbol.DeclaringSyntaxReferences.Select(declared => declared.GetSyntax(reading.Token) switch {
            FromClauseSyntax from => from.Expression,
            LetClauseSyntax let => let.Expression,
            VariableDeclaratorSyntax { Initializer.Value: var initial } => initial,
            _ => null,
        }).OfType<ExpressionSyntax>().Select(bound => reading.For(bound.SyntaxTree).GetOperation(bound, reading.Token)).OfType<IOperation>();
}
