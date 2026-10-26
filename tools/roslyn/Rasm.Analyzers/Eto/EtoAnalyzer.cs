using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Operations;
using Rasm.Analyzers.Rhino;

namespace Rasm.Analyzers.Eto;

// --- [COMPOSITION] ---------------------------------------------------------------------
[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class EtoAnalyzer : RasmAnalyzer {
    // --- [RULES]
    private const string Host = "Eto";

    private const string Native = "Microsoft.macOS";

    public static readonly DiagnosticDescriptor Marshal = Rule(
        "RASM2006",
        RhinoAnalyzer.Library,
        "Single call marshaled to the UI thread",
        "`{0}` marshals one call to the UI thread. Compose UI-thread calls into one `IO` run through `OnUiThread()`",
        "Eto `Application`, main `DispatchQueue`, or `NSObject` main-thread call taking a delegate outside `Callbacks`.");

    public static readonly DiagnosticDescriptor Cached = Rule(
        "RASM2201",
        Host,
        "Cached system font disposed",
        "`{1}` disposes the font `SystemFonts.{0}` caches for every caller. Read the font without a release",
        "`SystemFonts` font reaching `Dispose`, a `using`, a `DisposalOps` acquire or release argument, `new Disposal<T>`, or a `retain` call, directly or through a local, range variable, or lambda parameter.");

    public static readonly DiagnosticDescriptor Framed = Rule(
        "RASM2202",
        RhinoAnalyzer.Library,
        "Eto container acquired by plain dispose",
        "`{0}` disposes `{1}` with its children attached. Acquire `{1}` through `Containers.Frame`",
        "`Eto.Forms.Container` reaching a plain-dispose `DisposalOps` overload, `new Disposal<T>` with a release that only disposes, or a `retain` call, while the acquiring member attaches a child held before it, or any child to a `StackLayout`.");

    public static readonly DiagnosticDescriptor Twice = Rule(
        "RASM2203",
        RhinoAnalyzer.Library,
        "Retained control disposed on failure",
        "`{0}` disposes `{1}` on failure after `{2}` handed it to its owner. Delete the release",
        "`DisposalOps.OnFailure` release disposing an `Eto.Widget` that a plain-dispose `DisposalOps.Acquire` or a `retain` call handed to its owner earlier on the same path.");

    public static readonly DiagnosticDescriptor Escaped = Rule(
        "RASM2204",
        Native,
        "Native callback outside the Callbacks boundary",
        "Native callback `{0}` runs `{1}` outside the `Callbacks` boundary, and an exception there aborts Rhino. Route it through `Callbacks.Run(<effect>, <site>)`",
        "Body of a delegate handed to an Objective-C binding or `ObjCExtensions.AddMethod`, an `[UnmanagedFunctionPointer]` delegate, or an `[Export]`, `[UnmanagedCallersOnly]`, or `NSObject` override that is more than one `Callbacks` call, or a host member read before the boundary in a `Callbacks` call of such a body or an Eto override.");

    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => [Marshal, Cached, Framed, Twice, Escaped];

    protected override void Start(CompilationStartAnalysisContext context) {
        Compilation compilation = context.Compilation;
        INamedTypeSymbol? callbacks = compilation.GetTypeByMetadataName("Rasm.Rhino.Callbacks");
        INamedTypeSymbol? widget = compilation.GetTypeByMetadataName("Eto.Widget");
        if (callbacks is not null) Marshals(context, callbacks);
        if (Lifetimes.Of(compilation) is not { } lifetimes) return;
        if (compilation.GetTypeByMetadataName("Eto.Drawing.SystemFonts") is { } fonts && compilation.GetTypeByMetadataName("Eto.Drawing.Font") is { } font)
            Fonts(context, lifetimes, fonts, font);
        if (compilation.GetTypeByMetadataName("Eto.Forms.Container") is { } container && compilation.GetTypeByMetadataName("Eto.Forms.Control") is { } control)
            Frames(context, lifetimes, container, control, compilation.GetTypeByMetadataName("Eto.Forms.StackLayout"));
        if (widget is not null && !lifetimes.Failures.IsEmpty) Failures(context, lifetimes, widget);
        if (callbacks is not null) Natives(context, lifetimes, callbacks, widget);
    }

    // --- [MARSHALS]
    private static void Marshals(CompilationStartAnalysisContext context, INamedTypeSymbol callbacks) {
        Compilation compilation = context.Compilation;
        INamedTypeSymbol? application = compilation.GetTypeByMetadataName("Eto.Forms.Application");
        INamedTypeSymbol? queue = compilation.GetTypeByMetadataName("CoreFoundation.DispatchQueue");
        INamedTypeSymbol? objects = compilation.GetTypeByMetadataName("Foundation.NSObject");
        INamedTypeSymbol? threads = compilation.GetTypeByMetadataName("Foundation.NSThread");
        ImmutableArray<ISymbol> mains = Declared(compilation, "P:CoreFoundation.DispatchQueue.MainQueue", "P:Foundation.NSThread.MainThread");
        static bool Delegated(IMethodSymbol method) => method.Parameters.Any(static parameter => parameter.Type.TypeKind is TypeKind.Delegate);
        bool Main(IOperation? value) => value is IMemberReferenceOperation reference && Among(reference.Member, mains);
        Report<IOperation>(context, [OperationKind.Invocation, OperationKind.MethodReference], (operation, analysis) =>
            operation switch {
                IInvocationOperation invocation => (invocation.TargetMethod, invocation.Instance, invocation.Arguments),
                IMethodReferenceOperation reference => (reference.Method, reference.Instance, ImmutableArray<IArgumentOperation>.Empty),
                _ => default,
            } is ( { } method, var instance, var arguments) && !Inside<INameOfOperation>(operation) && !Within(analysis.ContainingSymbol, callbacks)
            && ((Same(method.ContainingType, application) && Delegated(method))
                || (Same(method.ContainingType, queue) && Delegated(method) && Main(instance))
                || (objects is not null && Derives(method.ContainingType, objects) && method.Parameters.Any(parameter => Same(parameter.Type, threads)) && arguments.Any(argument => Main(argument.Value))))
                ? [Diagnostic.Create(Marshal, operation.Syntax.GetLocation(), Label(method))]
                : []);
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
            type.AllInterfaces.Append(type).OfType<INamedTypeSymbol>().FirstOrDefault(static face => face.OriginalDefinition.SpecialType is SpecialType.System_Collections_Generic_IEnumerable_T) is { } items
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
        static IEnumerable<IOperation> Written(IPropertyReferenceOperation property) =>
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
                IPropertyReferenceOperation property when Derives(property.Property.Type, control) => Written(property),
                IPropertyReferenceOperation property when Holding(property.Property.Type, 2) => Grouped(property),
                IInvocationOperation call => call.Arguments.Select(static argument => argument.Value).Where(value => Derives(Stripped(value).Type, control)),
                _ => [],
            };
        static bool Earlier(ISymbol symbol, int start) =>
            symbol is IParameterSymbol { ContainingSymbol: IMethodSymbol { MethodKind: not MethodKind.AnonymousFunction } }
            || symbol.DeclaringSyntaxReferences.Any(declared => declared.Span.Start < start);
        bool Hazard(IOperation child, ITypeSymbol held, int start, Reading reading) =>
            (stack is not null && Derives(held, stack)) || child.DescendantsAndSelf().Select(value => Held(value, reading)).Any(symbol => symbol is not null && Earlier(symbol, start));
        static IParameterSymbol? Bound(IInvocationOperation call, ITypeSymbol held) =>
            call.Arguments.Select(static argument => Stripped(argument.Value)).OfType<IDelegateCreationOperation>()
                .Select(static creation => creation.Target).OfType<IAnonymousFunctionOperation>()
                .Select(static function => function.Symbol.Parameters.FirstOrDefault())
                .FirstOrDefault(parameter => parameter is not null && SymbolEqualityComparer.Default.Equals(parameter.Type, held));
        static ISymbol? Holder(IInvocationOperation call, ITypeSymbol held, Reading reading) =>
            Clause(call.Syntax) is { } clause && reading.Model.GetDeclaredSymbol(clause, reading.Token) is { } range ? range
            : Consumer(call) is IInvocationOperation outer && Bound(outer, held) is { } chained ? chained
            : Bound(call, held);
        IEnumerable<IOperation> Children(ISymbol? holder, IOperation site, IOperation? origin, Reading reading) =>
            Created(origin)
                .Concat(holder is null ? [] : References(holder, site, reading).SelectMany(Attached))
                .Concat(holder is ILocalSymbol local
                    ? local.DeclaringSyntaxReferences.Select(declared => declared.GetSyntax(reading.Token)).OfType<VariableDeclaratorSyntax>()
                        .SelectMany(declarator => declarator.Initializer is { } initial ? Created(reading.Model.GetOperation(initial.Value, reading.Token)) : [])
                    : []);
        bool Disposing(IOperation release) =>
            Stripped(release) is IDelegateCreationOperation { Target: IAnonymousFunctionOperation { Symbol.Parameters.Length: 1 } function }
            && Steps(function.Body) is { Length: 1 } steps && steps[0] is IInvocationOperation { Instance: { } instance } call
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
                IRangeVariableSymbol => held.DeclaringSyntaxReferences.Select(reference => reference.GetSyntax(reading.Token)).FirstOrDefault() switch {
                    FromClauseSyntax from => Origin(reading.Model.GetOperation(from.Expression, reading.Token), reading, seen.Add(held)),
                    LetClauseSyntax let => Origin(reading.Model.GetOperation(let.Expression, reading.Token), reading, seen.Add(held)),
                    _ => null,
                },
                ILocalSymbol => held.DeclaringSyntaxReferences.Select(reference => reference.GetSyntax(reading.Token)).FirstOrDefault() is VariableDeclaratorSyntax { Initializer.Value: var initial }
                    ? Origin(reading.Model.GetOperation(initial, reading.Token), reading, seen.Add(held))
                    : null,
                IParameterSymbol { Ordinal: 0, ContainingSymbol: IMethodSymbol { MethodKind: MethodKind.AnonymousFunction } function } => Chained(function, held, reading, seen),
                _ => null,
            };
        string? Chained(IMethodSymbol function, ISymbol held, Reading reading, ImmutableHashSet<ISymbol> seen) =>
            function.DeclaringSyntaxReferences.Select(reference => reading.Model.GetOperation(reference.GetSyntax(reading.Token), reading.Token)).FirstOrDefault() is { } lambda
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
    private static void Natives(CompilationStartAnalysisContext context, Lifetimes lifetimes, INamedTypeSymbol callbacks, INamedTypeSymbol? widget) {
        Compilation compilation = context.Compilation;
        INamedTypeSymbol? register = compilation.GetTypeByMetadataName("Foundation.RegisterAttribute");
        INamedTypeSymbol? export = compilation.GetTypeByMetadataName("Foundation.ExportAttribute");
        INamedTypeSymbol? pointer = compilation.GetTypeByMetadataName("System.Runtime.InteropServices.UnmanagedFunctionPointerAttribute");
        INamedTypeSymbol? callers = compilation.GetTypeByMetadataName("System.Runtime.InteropServices.UnmanagedCallersOnlyAttribute");
        ImmutableArray<IMethodSymbol> boundaries = Members(callbacks, "Run", "Answer", "Succeeded", "Started");
        ImmutableArray<IMethodSymbol> producers = Members(callbacks, "Handler", "Synchronous");
        ImmutableArray<IMethodSymbol> adders = Members(compilation.GetTypeByMetadataName("Eto.Mac.ObjCExtensions"), "AddMethod");
        static bool Hosted(ISymbol symbol) => !symbol.Locations.Any(static location => location.IsInSource);
        static bool Marked(ISymbol symbol, INamedTypeSymbol? attribute) => attribute is not null && symbol.GetAttributes().Any(data => Same(data.AttributeClass, attribute));
        bool Wrapper(INamedTypeSymbol type) =>
            type.GetAttributes().Any(data => Same(data.AttributeClass, register)
                && ((data.ConstructorArguments.Length > 1 && data.ConstructorArguments[1].Value is true) || data.NamedArguments.Any(static named => named.Value.Value is true && string.Equals(named.Key, "IsWrapper", StringComparison.Ordinal))));
        bool Bound(INamedTypeSymbol? type) => type is not null && (Wrapper(type) || Bound(type.ContainingType));
        bool Wrapped(IMethodSymbol method) => method.IsOverride && Bound(Overridden(method).Last().ContainingType);
        bool Overriding(IMethodSymbol method, INamedTypeSymbol? root) =>
            root is not null && method.IsOverride && Overridden(method).Last() is var origin && Hosted(origin) && Derives(origin.ContainingType, root);
        bool Produced(IOperation? value, Reading reading, int depth) =>
            value is not null && depth > 0 && Stripped(value) switch {
                IInvocationOperation call when Among(call.TargetMethod, producers) => true,
                IInvocationOperation call when Among(call.TargetMethod, lifetimes.Lifts) => call.Arguments.Any(argument => Produced(argument.Value, reading, depth - 1)),
                var reference when Held(reference, reading) is { } held => held.DeclaringSyntaxReferences.Select(declared => declared.GetSyntax(reading.Token)).Any(declared => declared switch {
                    FromClauseSyntax from => Produced(reading.Model.GetOperation(from.Expression, reading.Token), reading, depth - 1),
                    LetClauseSyntax let => Produced(reading.Model.GetOperation(let.Expression, reading.Token), reading, depth - 1),
                    VariableDeclaratorSyntax { Initializer.Value: var initial } => Produced(reading.Model.GetOperation(initial, reading.Token), reading, depth - 1),
                    _ => false,
                }),
                _ => false,
            };
        IOperation? Bodied(IMethodSymbol method, Reading reading) =>
            method.DeclaringSyntaxReferences.Where(declared => declared.SyntaxTree == reading.Model.SyntaxTree)
                .Select(declared => reading.Model.GetOperation(declared.GetSyntax(reading.Token), reading.Token)).OfType<IMethodBodyOperation>()
                .Select(static body => (IOperation?)body.BlockBody ?? body.ExpressionBody).FirstOrDefault();
        bool Routed(IOperation step, Reading reading, int depth) =>
            step.ConstantValue.HasValue || step is IDefaultValueOperation
            || (step is IInvocationOperation call && (Among(call.TargetMethod, boundaries)
                || (call.TargetMethod.MethodKind is MethodKind.DelegateInvoke && Produced(call.Instance, reading, 4))
                || (depth > 0 && !Hosted(call.TargetMethod) && Bodied(call.TargetMethod, reading) is IBlockOperation body && Steps(body) is { Length: 1 } inner && Routed(inner[0], reading, depth - 1))));
        static bool Rooted(IOperation? instance) => instance is not null && Stripped(instance) is IInstanceReferenceOperation;
        static bool Inherited(ISymbol member) => Hosted(member) && member.ContainingType is { SpecialType: not SpecialType.System_Object };
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
            body switch {
                IBlockOperation block when Steps(block) is var steps && !(steps.Length is 1 && Routed(steps[0], reading, 1)) =>
                    [(steps.FirstOrDefault(step => !Routed(step, reading, 1)) ?? block, Shown(steps.FirstOrDefault(step => !Routed(step, reading, 1)) ?? block))],
                _ => [],
            };
        static IOperation? Initialized(ISymbol symbol, Reading reading) =>
            symbol.DeclaringSyntaxReferences.Select(declared => declared.GetSyntax(reading.Token)).OfType<VariableDeclaratorSyntax>()
                .Select(static declarator => declarator.Initializer?.Value).OfType<ExpressionSyntax>()
                .Select(initial => reading.Model.GetOperation(initial, reading.Token)).FirstOrDefault(static found => found is not null);
        IOperation? Target(IOperation? value, Reading reading) =>
            value is null ? null : Stripped(value) switch {
                IDelegateCreationOperation { Target: IAnonymousFunctionOperation function } => function.Body,
                IDelegateCreationOperation { Target: IMethodReferenceOperation reference } => Bodied(reference.Method, reading),
                var held when Held(held, reading) is { } symbol => Target(Initialized(symbol, reading), reading),
                _ => null,
            };
        IEnumerable<(IOperation Site, string Run)> Checked(IOperation? body, Reading reading) => body is null ? [] : Escapes(body, reading).Concat(Eager(body));
        IEnumerable<(string Callback, IOperation Site, string Run)> Handed(IOperation operation, Reading reading) =>
            operation switch {
                IDelegateCreationOperation { Type: { } type } creation when Marked(type, pointer) =>
                    Checked(Target(creation, reading), reading).Select(escape => (type.Name, escape.Site, escape.Run)),
                IInvocationOperation call when Bound(call.TargetMethod.ContainingType) || Among(call.TargetMethod, adders) =>
                    call.Arguments.Where(argument => argument.Parameter?.Type is { TypeKind: TypeKind.Delegate } handed && !Marked(handed, pointer) && !Produced(argument.Value, reading, 4))
                        .SelectMany(argument => Checked(Target(argument.Value, reading), reading))
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
            IOperation? body = root is IMethodBodyOperation bodied ? (IOperation?)bodied.BlockBody ?? bodied.ExpressionBody : root;
            IEnumerable<(IOperation Site, string Run)> escapes =
                Marked(method, export) || Marked(method, callers) || Wrapped(method) ? Checked(body, reading)
                : Overriding(method, widget) && body is not null ? Eager(body)
                : [];
            foreach ((IOperation site, string run) in escapes)
                block.ReportDiagnostic(Diagnostic.Create(Escaped, site.Syntax.GetLocation(), method.Name, run));
        });
    }

    // --- [STEPS]
    private static IOperation Unwrapped(IOperation step) =>
        step switch {
            IReturnOperation { ReturnedValue: { } returned } => Stripped(returned),
            IExpressionStatementOperation statement => Stripped(statement.Operation),
            _ => step,
        };

    private static ImmutableArray<IOperation> Steps(IBlockOperation body) =>
        [.. body.Operations.Where(static step => step is not IReturnOperation { ReturnedValue: null, IsImplicit: true }).Select(Unwrapped)];
}
