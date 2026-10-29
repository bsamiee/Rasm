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
        "`SystemFonts` fonts reach no `Dispose`, `using`, `use`, `release`, acquire, holder, or retaining call, directly or through bindings");

    public static readonly DiagnosticDescriptor Framed = Rule(
        "RASM2202",
        Category,
        "Eto container acquired by plain dispose, detach children before `Dispose`",
        "`{0}` disposes `{1}` with children attached, acquire `{1}` through `use` with a release detaching children before `Dispose`",
        "Plain-dispose acquires, holders, and retaining calls of `Eto.Forms.Container` detach earlier-held or `StackLayout` children before `Dispose`");

    public static readonly DiagnosticDescriptor Twice = Rule(
        "RASM2203",
        Category,
        "Retained control disposed on failure, delete the release",
        "`{0}` disposes `{1}` on failure after `{2}` handed `{1}` to an owner, delete the release",
        "Failure releases skip `Eto.Widget` values a plain-dispose acquire or retaining call handed to an owner earlier on one path");

    public static readonly DiagnosticDescriptor Escaped = Rule(
        "RASM2204",
        Category,
        "Native callback or Eto override reading host outside `Session`, route through `Session`",
        "`{1}` in native callback `{0}` runs outside `Session` and an exception aborts Rhino, route through `Session.Run(<effect>, <failed>)` with `[CallerMemberName]`",
        "Native callbacks (Objective-C, `AddMethod`, `[UnmanagedFunctionPointer]`, `[Export]`, `[UnmanagedCallersOnly]`, `NSObject`) are one `Session` call, Eto overrides read host members inside `Session`");

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
        if (!eto.Handlers.IsEmpty) Handlers(context, eto.Handlers);
        if (Lifetimes.Of(system, rasm, new LanguageExtSymbols(compilation)) is not { } lifetimes) return;
        if (eto is { SystemFonts: { } fonts, Font: { } font }) Fonts(context, lifetimes, fonts, font);
        if (eto is { Container: { } container, Control: { } control }) Frames(context, lifetimes, container, control, eto.StackLayout);
        if (eto.Widget is { } widget && !lifetimes.Failures.IsEmpty) Failures(context, lifetimes, widget);
        if (rasm.Session is { } callbacks) Natives(context, lifetimes, system, eto, new MacSymbols(compilation), rasm, callbacks);
    }

    // --- [FONTS]
    private static void Fonts(CompilationStartAnalysisContext context, Lifetimes lifetimes, INamedTypeSymbol fonts, INamedTypeSymbol font) =>
        Report<IInvocationOperation>(context, [OperationKind.Invocation], (invocation, analysis) =>
            Same(invocation.TargetMethod.ContainingType, fonts) && Same(invocation.Type, font) && invocation.SemanticModel is { } model
            && lifetimes.Released(invocation, font, new Reading(model, analysis.CancellationToken)).FirstOrDefault() is { } release
                ? [Diagnostic.Create(Cached, invocation.Syntax.GetLocation(), invocation.TargetMethod.Name, release)]
                : []);

    // --- [FRAMES]
    private static void Frames(CompilationStartAnalysisContext context, Lifetimes lifetimes, INamedTypeSymbol container, INamedTypeSymbol control, INamedTypeSymbol? stack) {
        ImmutableArray<IMethodSymbol> plain = [.. lifetimes.Acquires.Where(lifetimes.Plain)];
        bool Holding(ITypeSymbol type, int depth) =>
            Enumerated(type) is { } items
            && (Derives(items.TypeArguments[0], control)
                || (depth > 0 && items.TypeArguments[0].GetMembers().OfType<IPropertySymbol>().Any(property => Derives(property.Type, control) || Holding(property.Type, depth - 1))));
        IEnumerable<IOperation> Seated(IOperation? initializer) =>
            initializer is IObjectOrCollectionInitializerOperation members
                ? members.Initializers.SelectMany(member => member switch {
                    ISimpleAssignmentOperation { Target: IPropertyReferenceOperation property } assignment when Derives(property.Property.Type, control) => [assignment.Value],
                    IMemberInitializerOperation { InitializedMember: IPropertyReferenceOperation property } nested when Holding(property.Property.Type, 2) => [nested.Initializer],
                    _ => Enumerable.Empty<IOperation>(),
                })
                : [];
        IEnumerable<IOperation> Created(IOperation? origin) =>
            origin is null ? [] : origin.DescendantsAndSelf().OfType<IObjectCreationOperation>().Where(creation => Derives(creation.Type, container)).SelectMany(creation => Seated(creation.Initializer));
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
        IEnumerable<IOperation> Attached(IOperation reference) =>
            Consumer(reference) switch {
                IPropertyReferenceOperation property when Derives(property.Property.Type, control) => Assigned(property),
                IPropertyReferenceOperation property when Holding(property.Property.Type, 2) => Grouped(property),
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
            Created(origin)
                .Concat(holder is null ? [] : References(holder, site, reading).SelectMany(Attached))
                .Concat(holder is ILocalSymbol local ? Bindings(local, reading).SelectMany(Created) : []);
        bool Disposing(IOperation release) =>
            Lambda(release) is { Symbol.Parameters.Length: 1 } function && Sole(function.Body) is IInvocationOperation { Instance: { } instance } call
            && lifetimes.Disposes(call.TargetMethod) && Stripped(instance) is IParameterReferenceOperation reference
            && SymbolEqualityComparer.Default.Equals(reference.Parameter, function.Symbol.Parameters[0]);
        (string Owner, ITypeSymbol Held, ISymbol? Holder, IOperation? Origin)? Acquired(IOperation operation, Reading reading) =>
            operation switch {
                IInvocationOperation call when Among(call.TargetMethod, plain) =>
                    (Label(call), call.TargetMethod.TypeArguments[0], Holder(call, call.TargetMethod.TypeArguments[0], reading), call.Arguments.FirstOrDefault(static argument => argument.Parameter?.Ordinal is 0)),
                IInvocationOperation { Arguments.Length: 1 } call when lifetimes.Retaining(call, call.Arguments[0].Parameter!) && Stripped(call.Arguments[0].Value).Type is { } handed =>
                    (Label(call), handed, Held(call.Arguments[0].Value, reading), call.Arguments[0]),
                IObjectCreationOperation { Constructor: { } constructor, Type: INamedTypeSymbol { TypeArguments.Length: 1 } lease, Arguments.Length: 2 } creation
                    when Among(constructor, lifetimes.Holders) && Disposing(creation.Arguments[1].Value) =>
                    (Label(constructor), lease.TypeArguments[0], Held(creation.Arguments[0].Value, reading), creation.Arguments[0]),
                _ => null,
            };
        Report<IOperation>(context, [OperationKind.Invocation, OperationKind.ObjectCreation], (operation, analysis) => {
            if (operation.SemanticModel is not { } model) return [];
            Reading reading = new(model, analysis.CancellationToken);
            return Acquired(operation, reading) is { } acquired && Derives(acquired.Held, container)
                && Children(acquired.Holder, operation, acquired.Origin, reading).Any(child => Hazard(child, acquired.Held, operation.Syntax.SpanStart, reading))
                    ? [Diagnostic.Create(Framed, operation.Syntax.GetLocation(), acquired.Owner, acquired.Held.Name)]
                    : [];
        });
    }

    // --- [FAILURES]
    private static void Failures(CompilationStartAnalysisContext context, Lifetimes lifetimes, INamedTypeSymbol widget) {
        ImmutableArray<IMethodSymbol> handing = [
            .. lifetimes.Acquires.Where(method => method.Parameters.Length > 1 && lifetimes.Plain(method) && Same(method.Parameters[method.Parameters.Length - 1].Type, lifetimes.Action)),
        ];
        IEnumerable<(IOperation Site, ISymbol Held)> Disposed(IOperation release, Reading reading) =>
            release.DescendantsAndSelf()
                .SelectMany(step => step switch {
                    IInvocationOperation { Instance: { } instance } call when lifetimes.Disposes(call.TargetMethod) => [(Site: step, Value: instance)],
                    IMethodReferenceOperation { Instance: { } instance } reference when lifetimes.Disposes(reference.Method) => [(Site: step, Value: instance)],
                    IInvocationOperation call when Among(call.TargetMethod, lifetimes.Releases) =>
                        call.Arguments.SelectMany(static argument => argument.Value.DescendantsAndSelf()).Select(value => (Site: step, Value: value)),
                    _ => [],
                })
                .Where(disposed => Derives(Stripped(disposed.Value).Type, widget))
                .Select(disposed => (disposed.Site, Held: Held(disposed.Value, reading)))
                .Where(static disposed => disposed.Held is not null)
                .Select(static disposed => (disposed.Site, disposed.Held!))
                .Distinct();
        string? Origin(IOperation? value, Reading reading, ImmutableHashSet<ISymbol> seen) =>
            value is null ? null : Stripped(value) switch {
                IInvocationOperation call when Among(call.TargetMethod, handing) => Label(call),
                IInvocationOperation call when Among(call.TargetMethod, lifetimes.Failures) =>
                    Origin(call.Arguments.FirstOrDefault(static argument => argument.Parameter?.Ordinal is 0)?.Value, reading, seen),
                ITranslatedQueryOperation { Syntax: QueryExpressionSyntax { Body.SelectOrGroup: SelectClauseSyntax select } } =>
                    Origin(reading.Model.GetOperation(select.Expression, reading.Token), reading, seen),
                var reference when Held(reference, reading) is { } held => Source(held, reading, seen),
                _ => null,
            };
        string? Source(ISymbol held, Reading reading, ImmutableHashSet<ISymbol> seen) =>
            seen.Contains(held) ? null : held switch {
                IRangeVariableSymbol or ILocalSymbol => Bindings(held, reading).Select(bound => Origin(bound, reading, seen.Add(held))).FirstOrDefault(),
                IParameterSymbol { Ordinal: 0, ContainingSymbol: IMethodSymbol { MethodKind: MethodKind.AnonymousFunction } function } => Chained(function, held, reading, seen),
                _ => null,
            };
        string? Chained(IMethodSymbol function, ISymbol held, Reading reading, ImmutableHashSet<ISymbol> seen) =>
            Declarations(function, reading).FirstOrDefault() is { } lambda
            && Consumer(lambda) is IDelegateCreationOperation creation && Consumer(creation) is IArgumentOperation { Parent: IInvocationOperation call } argument
                ? Origin(call.Instance ?? (call.TargetMethod.IsExtensionMethod && argument.Parameter?.Ordinal is not 0 ? call.Arguments[0].Value : null), reading, seen.Add(held))
                : null;
        string? Retained(ISymbol held, IOperation failure, Reading reading) =>
            References(held, failure, reading)
                .Where(reference => reference.Syntax.Span.End <= failure.Syntax.SpanStart)
                .Select(static reference => Consumer(reference))
                .OfType<IArgumentOperation>()
                .Select(argument => argument is { Parent: IInvocationOperation call, Parameter: { } parameter } && lifetimes.Retaining(call, parameter) ? Label(call) : null)
                .FirstOrDefault(static owner => owner is not null);
        Report<IInvocationOperation>(context, [OperationKind.Invocation], (failure, analysis) => {
            if (!Among(failure.TargetMethod, lifetimes.Failures) || failure.SemanticModel is not { } model
                || failure.Arguments.FirstOrDefault(static argument => argument.Parameter?.Ordinal is 1) is not { } release) return [];
            Reading reading = new(model, analysis.CancellationToken);
            return Disposed(release.Value, reading)
                .Select(disposed => (disposed.Site, disposed.Held, Owner: Retained(disposed.Held, failure, reading) ?? Source(disposed.Held, reading, [])))
                .Where(static found => found.Owner is not null)
                .Select(static found => Diagnostic.Create(Twice, found.Site.Syntax.GetLocation(), found.Site.Syntax, found.Held.Name, found.Owner));
        });
    }

    // --- [NATIVES]
    private static void Natives(CompilationStartAnalysisContext context, Lifetimes lifetimes, SystemSymbols system, EtoSymbols eto, MacSymbols mac, RasmRhinoSymbols rasm, INamedTypeSymbol callbacks) {
        (INamedTypeSymbol? register, INamedTypeSymbol? export, INamedTypeSymbol? pointer, INamedTypeSymbol? callers, INamedTypeSymbol? widget) = (mac.Register, mac.Export, system.FunctionPointer, system.CallersOnly, eto.Widget);
        (ImmutableArray<IMethodSymbol> boundaries, ImmutableArray<IMethodSymbol> producers, ImmutableArray<IMethodSymbol> adders) = (rasm.Boundaries, rasm.Producers, eto.AddMethod);
        bool Wrapper(INamedTypeSymbol type) =>
            Marked(type, register) is { } data
            && ((data.ConstructorArguments.Length > 1 && data.ConstructorArguments[1].Value is true) || data.NamedArguments.Any(static named => named.Value.Value is true && string.Equals(named.Key, "IsWrapper", StringComparison.Ordinal)));
        bool Bound(INamedTypeSymbol? type) => Containers(type).OfType<INamedTypeSymbol>().Any(Wrapper);
        bool Wrapped(IMethodSymbol method) => method.IsOverride && Bound(Overridden(method).Last().ContainingType);
        bool Overriding(IMethodSymbol method, INamedTypeSymbol? root) =>
            root is not null && method.IsOverride && Overridden(method).Last() is var origin && !Sourced(origin) && Derives(origin.ContainingType, root);
        bool Produced(IOperation? value, Reading reading, int depth) =>
            value is not null && depth > 0 && Stripped(value) switch {
                IInvocationOperation call when Among(call.TargetMethod, producers) => true,
                IInvocationOperation call when Among(call.TargetMethod, lifetimes.Lifts) => call.Arguments.Any(argument => Produced(argument.Value, reading, depth - 1)),
                var reference when Held(reference, reading) is { } held => Bindings(held, reading).Any(bound => Produced(bound, reading, depth - 1)),
                _ => false,
            };
        IOperation? Bodied(IMethodSymbol method, Reading reading) => Declarations(method, reading).OfType<IMethodBodyOperation>().Select(Body).FirstOrDefault();
        bool Routed(IOperation step, Reading reading, int depth) =>
            step.ConstantValue.HasValue || step is IDefaultValueOperation
            || (step is IInvocationOperation call && (Among(call.TargetMethod, boundaries)
                || (call.TargetMethod.MethodKind is MethodKind.DelegateInvoke && Produced(call.Instance, reading, 4))
                || (depth > 0 && Sourced(call.TargetMethod) && Bodied(call.TargetMethod, reading) is IBlockOperation body && Sole(body) is { } inner && Routed(inner, reading, depth - 1))));
        static bool Rooted(IOperation? instance) => instance is not null && Stripped(instance) is IInstanceReferenceOperation;
        static bool Inherited(ISymbol member) => !Sourced(member) && member.ContainingType is { SpecialType: not SpecialType.System_Object };
        static bool Read(IOperation operation) =>
            operation switch {
                IMethodReferenceOperation => false,
                IMemberReferenceOperation member => Rooted(member.Instance) && Inherited(member.Member),
                IInvocationOperation call => Rooted(call.Instance) && Inherited(call.TargetMethod),
                _ => false,
            };
        static IEnumerable<IOperation> Reads(IOperation operation) =>
            operation is IAnonymousFunctionOperation ? [] : Read(operation) ? [operation] : operation.ChildOperations.SelectMany(Reads);
        IEnumerable<(IOperation Site, string Run)> Eager(IOperation body) =>
            body.DescendantsAndSelf().OfType<IInvocationOperation>().Where(call => Among(call.TargetMethod, boundaries))
                .Select(static call => (Site: (IOperation)call, Reads: call.Arguments.SelectMany(static argument => Reads(argument.Value)).Select(static read => read.Syntax.ToString()).Distinct(StringComparer.Ordinal).ToImmutableArray()))
                .Where(static eager => !eager.Reads.IsEmpty)
                .Select(static eager => (eager.Site, string.Join("`, `", eager.Reads)));
        static string Shown(IOperation step) =>
            step switch {
                IInvocationOperation call => Label(call),
                IVariableDeclarationGroupOperation declared when declared.DescendantsAndSelf().OfType<IInvocationOperation>().FirstOrDefault() is { } call => Label(call),
                _ => step.Syntax.ToString().Split('\n')[0].Trim(),
            };
        IEnumerable<(IOperation Site, string Run)> Escapes(IOperation? body, Reading reading) =>
            body is IBlockOperation block && Steps(block) is var steps && !(steps.Length is 1 && Routed(steps[0], reading, 1))
            && (steps.FirstOrDefault(step => !Routed(step, reading, 1)) ?? block) is var site ? [(site, Shown(site))] : [];
        IOperation? Implementation(IOperation? value, Reading reading) =>
            value is null ? null : Stripped(value) switch {
                IDelegateCreationOperation { Target: IAnonymousFunctionOperation function } => function.Body,
                IDelegateCreationOperation { Target: IMethodReferenceOperation reference } => Bodied(reference.Method, reading),
                var held when Held(held, reading) is ILocalSymbol local => Implementation(Bindings(local, reading).FirstOrDefault(), reading),
                _ => null,
            };
        IEnumerable<(IOperation Site, string Run)> Checked(IOperation? body, Reading reading) => body is null ? [] : Escapes(body, reading).Concat(Eager(body));
        IEnumerable<(string Callback, IOperation Site, string Run)> Handed(IOperation operation, Reading reading) =>
            operation switch {
                IDelegateCreationOperation { Type: { } type } creation when Marked(type, pointer) is not null =>
                    Checked(Implementation(creation, reading), reading).Select(escape => (type.Name, escape.Site, escape.Run)),
                IInvocationOperation call when Bound(call.TargetMethod.ContainingType) || Among(call.TargetMethod, adders) =>
                    call.Arguments.Where(argument => argument.Parameter?.Type is { TypeKind: TypeKind.Delegate } handed && Marked(handed, pointer) is null && !Produced(argument.Value, reading, 4))
                        .SelectMany(argument => Checked(Implementation(argument.Value, reading), reading))
                        .Select(escape => (Label(call.TargetMethod), escape.Site, escape.Run)),
                _ => [],
            };
        Report<IOperation>(context, [OperationKind.DelegateCreation, OperationKind.Invocation], (operation, analysis) =>
            !Within(analysis.ContainingSymbol, callbacks) && operation.SemanticModel is { } model
                ? Handed(operation, new Reading(model, analysis.CancellationToken)).Select(static escape => Diagnostic.Create(Escaped, escape.Site.Syntax.GetLocation(), escape.Callback, escape.Run))
                : []);
        context.RegisterOperationBlockAction(block => {
            if (block.OwningSymbol is not IMethodSymbol method || Within(method, callbacks) || block.OperationBlocks.FirstOrDefault() is not { SemanticModel: { } model } root) return;
            Reading reading = new(model, block.CancellationToken);
            IOperation? body = root is IMethodBodyOperation bodied ? Body(bodied) : root;
            IEnumerable<(IOperation Site, string Run)> escapes =
                Marked(method, export) is not null || Marked(method, callers) is not null || Wrapped(method) ? Checked(body, reading)
                : Overriding(method, widget) && body is not null ? Eager(body)
                : [];
            foreach ((IOperation site, string run) in escapes)
                block.ReportDiagnostic(Diagnostic.Create(Escaped, site.Syntax.GetLocation(), method.Name, run));
        });
    }

    // --- [HANDLERS]
    private static void Handlers(CompilationStartAnalysisContext context, ImmutableArray<IMethodSymbol> handlers) =>
        Report<IInvocationOperation>(context, [OperationKind.Invocation], (invocation, _) =>
            Among(invocation.TargetMethod, handlers) && invocation.TargetMethod.TypeArguments[0] is { TypeKind: TypeKind.Delegate } handed
                ? [Diagnostic.Create(Handler, Named(invocation), Label(invocation.TargetMethod), handed.Name)]
                : []);

    // --- [BINDINGS]
    private static ISymbol? Held(IOperation reference, Reading reading) =>
        Stripped(reference).Syntax is IdentifierNameSyntax name && reading.Model.GetSymbolInfo(name, reading.Token).Symbol is { } held
        && held is ILocalSymbol or IParameterSymbol or IRangeVariableSymbol ? held : null;

    private static IEnumerable<IOperation> Bindings(ISymbol symbol, Reading reading) =>
        symbol.DeclaringSyntaxReferences.Select(declared => declared.GetSyntax(reading.Token) switch {
            FromClauseSyntax from => from.Expression,
            LetClauseSyntax let => let.Expression,
            VariableDeclaratorSyntax { Initializer.Value: var initial } => initial,
            _ => null,
        }).OfType<ExpressionSyntax>().Select(bound => reading.Model.GetOperation(bound, reading.Token)).OfType<IOperation>();

    // --- [STEPS]
    private static IOperation? Body(IMethodBodyOperation body) => (IOperation?)body.BlockBody ?? body.ExpressionBody;
}
