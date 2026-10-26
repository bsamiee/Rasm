using System.Collections.Concurrent;
using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Operations;

namespace Rasm.Analyzers;

// --- [COMPOSITION] ---------------------------------------------------------------------
[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class RhinoAnalyzer : RasmAnalyzer {
    // --- [RULES]
    private const string Category = "RhinoCommon";

    public static readonly DiagnosticDescriptor Count = Rule(
        "RASM2002",
        Category,
        "Unchecked item count, compare with input count",
        "`{0}` count reaches no `==` or `!=`, compare with `{1}` count and fail naming `{0}` and both counts",
        "RhinoCommon delete, remove, set, and attach counts over a collection reach `==` or `!=`");

    public static readonly DiagnosticDescriptor OffUiThread = Rule(
        "RASM2003",
        Category,
        "`RhinoDoc` read off UI thread, read through `OnUiThread()`",
        "`{0}` reads `RhinoDoc` off UI thread in `{1}`, read in an `OnUiThread()` `IO` and pass values or duplicated geometry",
        "Lambdas passed to `Task`, `TaskFactory`, `Parallel`, `ThreadPool`, or `Thread` read no `RhinoDoc` expression");

    public static readonly DiagnosticDescriptor Active = Rule(
        "RASM2004",
        Category,
        "`ActiveDoc` read with document in scope, read the held document",
        "`RhinoDoc.ActiveDoc` read where `{0}` holds a `RhinoDoc`, write `{0}`",
        "Read an in-scope `RhinoDoc` parameter, local, field, or property over `RhinoDoc.ActiveDoc` outside `==` or `!=` comparisons");

    public static readonly DiagnosticDescriptor Script = Rule(
        "RASM2005",
        Category,
        "`RunScript` in command without `ScriptRunner` style, add `Style.ScriptRunner`",
        "Command `{0}` calls `RhinoApp.RunScript` without `Style.ScriptRunner`, add `Style.ScriptRunner` to `[CommandStyle]` on `{0}`",
        "`Command` subclasses calling `RhinoApp.RunScript` declare `Style.ScriptRunner` in `[CommandStyle]`");

    public static readonly DiagnosticDescriptor Marshal = Rule(
        "RASM2006",
        Category,
        "Single call marshaled to UI thread, compose one `OnUiThread()` `IO`",
        "`{0}` marshals one call to UI thread, compose UI-thread calls into one `IO` run through `OnUiThread()`",
        "Compose UI-thread calls into one `OnUiThread()` `IO` over delegate-taking Eto `Application`, main `DispatchQueue`, or `NSObject` main-thread calls outside `Callbacks`");

    public static readonly DiagnosticDescriptor Override = Rule(
        "RASM2007",
        Category,
        "`SetDisplayModeOverride` result read, discard through `_ =`",
        "`{0}` returns false after storing the override, discard its result with `_ =`",
        "`ObjectAttributes.SetDisplayModeOverride` results go to a discard or an expression statement");

    public static readonly DiagnosticDescriptor Mac = Rule(
        "RASM2008",
        Category,
        "macOS-breaking member outside macOS guard, guard or replace the call",
        "`{0}` runs outside the false branch of `OperatingSystem.IsMacOS()` and on macOS {1}, {2}",
        "`NamedViewTable.RestoreWithAspectRatio` and `Environment.GetCommandLineArgs` run in an `IsMacOS()` conditional's false branch, after `!IsMacOS() &&`, or after `IsMacOS() ||`");

    public static readonly DiagnosticDescriptor Grayscale = Rule(
        "RASM2009",
        Category,
        "`GrayscaleDib` result read without null check, acquire through `Optional`",
        "`{0}` returns null on macOS, acquire `Optional({0}).ToFin(new Refused(...))` with `use` and `[CallerArgumentExpression]` call text",
        "`ZBufferCapture.GrayscaleDib` results enter as first argument of `Optional` or a null-checking acquire");

    public static readonly DiagnosticDescriptor Page = Rule(
        "RASM2010",
        Category,
        "Page view built in `PageControl`, realize in `OnActivate(true)`",
        "`{0}.PageControl` reaches `{1}` and Rhino disposes every page's result on panel close, return a held control realized in `OnActivate(true)`",
        "`ObjectPropertiesPage.PageControl` getters create no Eto control, call no acquire or `Rasm.Rhino.UI` `Open` or `Realize`, directly or through type members");

    public static readonly DiagnosticDescriptor Docked = Rule(
        "RASM2011",
        Category,
        "Floating view docked by assignment, delete the assignment",
        "`{0}` destroys a floated view on macOS in place of docking it, delete the assignment",
        "`RhinoView.Floating` takes no `false` assignment");

    public static readonly DiagnosticDescriptor Mode = Rule(
        "RASM2012",
        Category,
        "Owned `DisplayModeDescription` left undisposed, acquire through `use`",
        "`{0}` returns an owned `DisplayModeDescription` copy no release reaches, acquire `Optional({0}).ToFin(new Refused(...))` with `use` and `[CallerArgumentExpression]` call text",
        "`DisplayModeDescription.GetDisplayMode`, `FindByName`, and `RhinoViewport.DisplayMode` copies reach `Dispose`, `using`, `use`, `release`, acquire, holder, or retaining call");

    public static readonly DiagnosticDescriptor Settings = Rule(
        "RASM2101",
        Category,
        "`PersistentSettings` read through a default, call `TryGet<Kind>`",
        "`{0}` reads `PersistentSettings` through a default value, call `{1}`",
        "Read `PersistentSettings` keys through `TryGet<Kind>` over `Get<Kind>` overloads taking a default value");

    public static readonly DiagnosticDescriptor Redraw = Rule(
        "RASM2102",
        Category,
        "View redraw outside `Commits`, pass a `RedrawPolicy`",
        "`{0}` controls view redraw outside `Commits`, pass a `RedrawPolicy` to `Commits.Commit` or `Commits.WithinRedraw`",
        "`ViewTable.Redraw`, `ViewTable.EnableRedraw`, and `RhinoView.Redraw` calls and `ViewTable.RedrawEnabled` writes run inside `Commits`");

    public static readonly DiagnosticDescriptor Narrowed = Rule(
        "RASM2103",
        Category,
        "Cast or type test on `GeometryBase` duplicate, acquire typed copy with `use`",
        "`{0}` narrows `{1}` to `{2}`, acquire a typed copy with `use(() => <product>)` under the owner's `Bracket()`",
        "`GeometryBase.Duplicate` and `DuplicateShallow` results reach no narrowing cast, type test, or switch");

    public static readonly DiagnosticDescriptor Toggled = Rule(
        "RASM2104",
        Category,
        "Conduit toggled outside `Conduits`, toggle through `Conduits`",
        "`{0}` toggles a conduit outside `Conduits`, toggle through the one library enable operation",
        "`DisplayConduit.Enabled` writes run inside `Conduits`");

    public static readonly DiagnosticDescriptor Rendering = Rule(
        "RASM2105",
        Category,
        "`RenderSettings` written outside `RenderSettingsSources`, write through `Edit`",
        "`{0}` writes `RhinoDoc.RenderSettings` with no undo record, write through `RenderSettingsSources.Edit`",
        "`RhinoDoc.RenderSettings` assignments run inside `RenderSettingsSources`");

    public static readonly DiagnosticDescriptor Owner = Rule(
        "RASM2107",
        Category,
        "`typeof` owner differs from subscribed event owner, name the event's owner",
        "`typeof({2})` names no owner of event `{1}`, write `typeof({0})`",
        "`Subscriptions.Host` and `During` `typeof` arguments name the event owner or add-lambda `[UnsafeAccessorType]` target, when accessible to callers");

    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics =>
        [Count, OffUiThread, Active, Script, Marshal, Override, Mac, Grayscale, Page, Docked, Mode, Settings, Redraw, Narrowed, Toggled, Rendering, Owner];

    protected override void Start(CompilationStartAnalysisContext context) {
        Compilation compilation = context.Compilation;
        (SystemSymbols system, RhinoCommonSymbols rhino, RasmRhinoSymbols rasm, LanguageExtSymbols languageExt) = (new(compilation), new(compilation), new(compilation), new(compilation));
        if (rasm.Callbacks is { } callbacks) Marshals(context, new EtoSymbols(compilation), new MacSymbols(compilation), callbacks);
        if (!rasm.Subscriptions.IsEmpty && system.Type is { } type) Owners(context, rasm.Subscriptions, type, system.UnsafeAccessor, system.UnsafeAccessorType);
        if (rhino.Document is not { } document) return;
        Counts(context, rhino.Counted);
        Threads(context, system.Schedulers, document);
        if (rhino.ActiveDoc is { } active) ActiveDocs(context, active, document);
        if (rhino is { Command: { } command, CommandStyle: { } style, ScriptRunner: IFieldSymbol { ConstantValue: int runner } }) Scripts(context, rhino.RunScript, command, style, runner);
        if (!rhino.SetDisplayModeOverride.IsEmpty) Overrides(context, rhino.SetDisplayModeOverride);
        if (system.IsMacOS is { } mac) Platforms(context, rhino, system, mac);
        if (rhino.GrayscaleDib is { } grayscale) Captures(context, grayscale, [.. rasm.UnlessFound, .. languageExt.Optional], rasm.Found);
        if (rhino is { ObjectPropertiesPage: { } page, PageControl: IPropertySymbol shown } && new EtoSymbols(compilation).Control is { } control) Pages(context, rasm, page, shown, control);
        Writes(context, rhino.Floating, Docked, static (assignment, _) => assignment is ISimpleAssignmentOperation { Value.ConstantValue: { HasValue: true, Value: false } });
        if (rhino.DisplayModeDescription is { } description && Lifetimes.Of(system, rasm, languageExt) is { } lifetimes) Modes(context, lifetimes, rhino.OwnedModes, description);
        if (rhino.PersistentSettings is { } settings) Reads(context, settings);
        if (rasm.Commits is { } commits) Redraws(context, rhino.Redraws, commits);
        if (rasm.Copies is { } copies) Duplicates(context, rhino.Duplicates, copies);
        if (rasm.Conduits is { } conduits) Writes(context, rhino.ConduitEnabled, Toggled, (_, analysis) => !Within(analysis.ContainingSymbol, conduits));
        if (rasm.RenderSettingsSources is { } sources) Writes(context, rhino.RenderSettings, Rendering, (_, analysis) => !Within(analysis.ContainingSymbol, sources));
    }

    // --- [MARSHALS]
    private static void Marshals(CompilationStartAnalysisContext context, EtoSymbols eto, MacSymbols mac, INamedTypeSymbol callbacks) {
        (INamedTypeSymbol? application, INamedTypeSymbol? queue, INamedTypeSymbol? objects, INamedTypeSymbol? threads, ImmutableArray<ISymbol> mains) =
            (eto.Application, mac.DispatchQueue, mac.NSObject, mac.NSThread, mac.Mains);
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

    // --- [COUNTS]
    private static void Counts(CompilationStartAnalysisContext context, ImmutableArray<IMethodSymbol> counted) {
        static bool Collected(IParameterSymbol? parameter) => parameter is { RefKind: RefKind.None, Type: var type } && Enumerated(type) is not null;
        ImmutableArray<IMethodSymbol> counting = [.. counted.Where(static method => method.ReturnType.SpecialType is SpecialType.System_Int32 && method.Parameters.Any(Collected))];
        if (counting.IsEmpty) return;
        Report<IInvocationOperation>(context, [OperationKind.Invocation], (invocation, _) =>
            Among(invocation.TargetMethod, counting) && Consumer(invocation) is not IBinaryOperation { OperatorKind: BinaryOperatorKind.Equals or BinaryOperatorKind.NotEquals }
                ? [Diagnostic.Create(Count, invocation.Syntax.GetLocation(), invocation.TargetMethod.Name, invocation.Arguments.First(static argument => Collected(argument.Parameter)).Value.Syntax)]
                : []);
    }

    // --- [THREADS]
    private static void Threads(CompilationStartAnalysisContext context, ImmutableArray<INamedTypeSymbol> schedulers, INamedTypeSymbol document) =>
        Report<IOperation>(context, [OperationKind.Invocation, OperationKind.ObjectCreation], (operation, _) =>
            operation switch {
                IInvocationOperation invocation => (invocation.TargetMethod, invocation.Arguments),
                IObjectCreationOperation creation => (creation.Constructor, creation.Arguments),
                _ => default,
            } is ( { } method, var arguments) && Among(method.ContainingType, schedulers) && Delegated(method)
                ? arguments.SelectMany(static argument => argument.Value.DescendantsAndSelf().OfType<IAnonymousFunctionOperation>().Take(1))
                    .SelectMany(static callback => callback.Body.Descendants())
                    .Where(reach => reach is not IConversionOperation && Same(reach.Type, document) && !Same(Consumer(reach)?.Type, document))
                    .Select(reach => Diagnostic.Create(OffUiThread, reach.Syntax.GetLocation(), reach.Syntax, Label(method)))
                : []);

    // --- [ACTIVE_DOCS]
    private static void ActiveDocs(CompilationStartAnalysisContext context, ISymbol active, INamedTypeSymbol document) {
        static bool Compared(IOperation operation) =>
            operation.Parent switch {
                IBinaryOperation { OperatorKind: BinaryOperatorKind.Equals or BinaryOperatorKind.NotEquals } => true,
                IConversionOperation or IConditionalAccessOperation or IMemberReferenceOperation => Compared(operation.Parent),
                _ => false,
            };
        Report<IPropertyReferenceOperation>(context, [OperationKind.PropertyReference], (reference, analysis) => {
            if (!Same(reference.Property, active) || Inside<INameOfOperation>(reference) || Compared(reference) || reference.SemanticModel is not { } model) return [];
            ISymbol owner = analysis.ContainingSymbol;
            int position = reference.Syntax.SpanStart;
            return model.LookupSymbols(position).FirstOrDefault(held => held switch {
                IParameterSymbol parameter => Same(parameter.Type, document),
                ILocalSymbol local => Same(local.Type, document) && local.DeclaringSyntaxReferences.All(declared => declared.Span.End <= position),
                IFieldSymbol field => Same(field.Type, document) && (field.IsStatic || !owner.IsStatic),
                IPropertySymbol property => Same(property.Type, document) && (property.IsStatic || !owner.IsStatic)
                    && !Same(property, owner) && !Same(property, (owner as IMethodSymbol)?.AssociatedSymbol),
                _ => false,
            }) is { } holder
                ? [Diagnostic.Create(Active, reference.Syntax.GetLocation(), holder.Name)]
                : [];
        });
    }

    // --- [SCRIPTS]
    private static void Scripts(CompilationStartAnalysisContext context, ImmutableArray<ISymbol> scripts, INamedTypeSymbol command, INamedTypeSymbol attribute, int runner) {
        bool Runs(INamedTypeSymbol type) =>
            Bases(type).Select(held => Marked(held, attribute)).FirstOrDefault(static style => style is not null) is { ConstructorArguments: { Length: 1 } arguments }
            && arguments[0].Value is int styles && (styles & runner) is not 0;
        Report<IInvocationOperation>(context, [OperationKind.Invocation], (invocation, analysis) =>
            Among(invocation.TargetMethod, scripts) && analysis.ContainingSymbol.ContainingType is { } owner && Derives(owner, command) && !Runs(owner)
                ? [Diagnostic.Create(Script, invocation.Syntax.GetLocation(), owner.Name)]
                : []);
    }

    // --- [OVERRIDES]
    private static void Overrides(CompilationStartAnalysisContext context, ImmutableArray<ISymbol> setters) =>
        Report<IInvocationOperation>(context, [OperationKind.Invocation], Override, (invocation, _) =>
            Among(invocation.TargetMethod, setters) && Consumer(invocation) is not (IExpressionStatementOperation or ISimpleAssignmentOperation { Target: IDiscardOperation }));

    // --- [PLATFORMS]
    private static void Platforms(CompilationStartAnalysisContext context, RhinoCommonSymbols rhino, SystemSymbols system, ISymbol mac) {
        ImmutableArray<(ISymbol Member, string Fact, string Action)> broken = [
            .. rhino.RestoreWithAspectRatio.Select(static member => ((ISymbol)member, "asserts and aborts Rhino when a docked target's aspect differs from its named view's", "guard it with `!OperatingSystem.IsMacOS() &&`")),
            .. system.CommandLine.Select(static member => ((ISymbol)member, "returns only Rhino's .NET startup assembly", "read macOS launch arguments through `NSProcessInfo.ProcessInfo.Arguments`")),
        ];
        if (broken.IsEmpty) return;
        bool OnMac(IOperation condition) => Stripped(condition) is IInvocationOperation call && Same(call.TargetMethod, mac);
        bool OffMac(IOperation condition) => Stripped(condition) is IUnaryOperation { OperatorKind: UnaryOperatorKind.Not } negated && OnMac(negated.Operand);
        bool Shields(IOperation parent, IOperation child) =>
            parent switch {
                IConditionalOperation branch when branch.WhenFalse == child => OnMac(branch.Condition),
                IConditionalOperation branch when branch.WhenTrue == child => OffMac(branch.Condition),
                IBinaryOperation { OperatorKind: BinaryOperatorKind.ConditionalAnd } both when both.RightOperand == child => OffMac(both.LeftOperand),
                IBinaryOperation { OperatorKind: BinaryOperatorKind.ConditionalOr } either when either.RightOperand == child => OnMac(either.LeftOperand),
                _ => false,
            };
        bool Guarded(IOperation node) => node.Parent is { } parent && (Shields(parent, node) || Guarded(parent));
        Report<IOperation>(context, [OperationKind.Invocation, OperationKind.MethodReference], (operation, _) =>
            Target(operation) is { } member && broken.FirstOrDefault(row => Same(member, row.Member)) is { Member: not null } row && !Inside<INameOfOperation>(operation) && !Guarded(operation)
                ? [Diagnostic.Create(Mac, operation.Syntax.GetLocation(), operation.Syntax, row.Fact, row.Action)]
                : []);
    }

    // --- [CAPTURES]
    private static void Captures(CompilationStartAnalysisContext context, ISymbol grayscale, ImmutableArray<IMethodSymbol> checks, ImmutableArray<IMethodSymbol> found) {
        static bool First(IOperation? consumer, ImmutableArray<IMethodSymbol> owners) =>
            consumer is IArgumentOperation { Parameter.Ordinal: 0, Parent: IInvocationOperation call } && Among(call.TargetMethod, owners);
        Report<IInvocationOperation>(context, [OperationKind.Invocation], Grayscale, (invocation, _) =>
            Same(invocation.TargetMethod, grayscale) && Consumer(invocation) is var consumer && consumer is not IExpressionStatementOperation && !First(consumer, checks)
            && !(consumer is IReturnOperation { Parent: IBlockOperation { Parent: IAnonymousFunctionOperation function } } && Consumer(function) is IDelegateCreationOperation creation && First(Consumer(creation), found)));
    }

    // --- [PAGES]
    private static void Pages(CompilationStartAnalysisContext context, RasmRhinoSymbols rasm, INamedTypeSymbol page, IPropertySymbol shown, INamedTypeSymbol control) {
        (ImmutableArray<IMethodSymbol> scopes, ISymbol? views) = (rasm.Acquire, rasm.UI);
        string? Realization(IOperation operation) =>
            operation switch {
                IObjectCreationOperation { Constructor: { } made } when Derives(made.ContainingType, control) => Label(made),
                IInvocationOperation call when Among(call.TargetMethod, scopes)
                    || (call.TargetMethod.Name is "Open" or "Realize" && Namespaced(context.Compilation, call.TargetMethod, views)) => Label(call.TargetMethod),
                _ => null,
            };
        context.RegisterSymbolStartAction(
            start => {
                if (start.Symbol is not INamedTypeSymbol type || !Derives(type.BaseType, page)) return;
                ConcurrentDictionary<ISymbol, (string? Realized, ImmutableArray<ISymbol> Reads)> bodies = new(SymbolEqualityComparer.Default);
                start.RegisterOperationBlockAction(block => {
                    ISymbol owner = Member(block.OwningSymbol).OriginalDefinition;
                    ImmutableArray<IOperation> operations = [.. block.OperationBlocks.SelectMany(static root => root.DescendantsAndSelf())];
                    (string? Realized, ImmutableArray<ISymbol> Reads) read = (
                        operations.Select(Realization).FirstOrDefault(static label => label is not null),
                        [.. operations.Select(Target).OfType<ISymbol>().Where(member => member is not IEventSymbol && Same(member.ContainingType, type)).Select(static member => member.OriginalDefinition).Distinct(SymbolEqualityComparer.Default)]);
                    _ = bodies.AddOrUpdate(owner, read, (_, prior) => (prior.Realized ?? read.Realized, prior.Reads.AddRange(read.Reads)));
                });
                start.RegisterSymbolEndAction(end => {
                    string? Reached(ISymbol member) => bodies.TryGetValue(member, out (string? Realized, ImmutableArray<ISymbol> Reads) held) ? held.Realized : null;
                    foreach (IPropertySymbol property in type.GetMembers().OfType<IPropertySymbol>().Where(property => !Same(property, shown) && Overridden(property).Any(held => Same(held, shown))))
                        if (bodies.TryGetValue(property, out (string? Realized, ImmutableArray<ISymbol> Reads) held) && (held.Realized ?? held.Reads.Select(Reached).FirstOrDefault(static label => label is not null)) is { } label)
                            end.ReportDiagnostic(Diagnostic.Create(Page, property.Locations[0], type.Name, label));
                });
            },
            SymbolKind.NamedType);
    }

    // --- [MODES]
    private static void Modes(CompilationStartAnalysisContext context, Lifetimes lifetimes, ImmutableArray<ISymbol> owned, INamedTypeSymbol description) =>
        Report<IOperation>(context, [OperationKind.Invocation, OperationKind.PropertyReference], Mode, (operation, analysis) =>
            !Written(operation) && Among(Target(operation), owned) && !Inside<INameOfOperation>(operation) && operation.SemanticModel is { } model
            && !lifetimes.Released(operation, description, new Reading(model, analysis.CancellationToken)).Any());

    // --- [READS]
    private static void Reads(CompilationStartAnalysisContext context, INamedTypeSymbol settings) {
        ImmutableArray<IMethodSymbol> methods = [
            .. settings.GetMembers().OfType<IMethodSymbol>().Where(static method => method is { IsStatic: false, Parameters: { IsEmpty: false } parameters } && parameters[0].Type.SpecialType is SpecialType.System_String),
        ];
        ImmutableHashSet<string> tried = [.. methods.Where(static method => method.Name.StartsWith("TryGet", StringComparison.Ordinal)).Select(static method => method.Name.Substring(6))];
        ImmutableHashSet<string> kinds = [
            .. methods.Where(static method => method is { ReturnsVoid: true, Parameters.Length: 2 } && method.Name.StartsWith("Set", StringComparison.Ordinal))
                .GroupBy(static method => method.Name.Substring(3), StringComparer.Ordinal)
                .Where(family => tried.Contains(family.Key) && family.Select(static method => method.Parameters[1].Type).Distinct(SymbolEqualityComparer.Default).Count() is 1)
                .Select(static family => family.Key),
        ];
        ImmutableArray<IMethodSymbol> defaulted = [
            .. methods.Where(method => method.Parameters.Length > 1 && method.Name.StartsWith("Get", StringComparison.Ordinal) && kinds.Contains(method.Name.Substring(3))),
        ];
        Report<IInvocationOperation>(context, [OperationKind.Invocation], (invocation, _) =>
            invocation is { Instance: { } source, TargetMethod: var method } && Among(method, defaulted)
                ? [Diagnostic.Create(
                    Settings,
                    invocation.Syntax.GetLocation(),
                    method.Name,
                    $"{source.Syntax}.Try{method.Name}{(method.IsGenericMethod ? $"<{method.TypeArguments[0].ToDisplayString(SymbolDisplayFormat.MinimallyQualifiedFormat)}>" : "")}({invocation.Arguments[0].Value.Syntax}, out {method.ReturnType.ToDisplayString(SymbolDisplayFormat.MinimallyQualifiedFormat)} value)")]
                : []);
    }

    // --- [REDRAWS]
    private static void Redraws(CompilationStartAnalysisContext context, ImmutableArray<ISymbol> redraws, INamedTypeSymbol commits) =>
        Report<IOperation>(context, [OperationKind.Invocation, OperationKind.MethodReference, OperationKind.PropertyReference], Redraw, (call, analysis) =>
            (call is not IPropertyReferenceOperation || Written(call)) && Among(Target(call), redraws) && !Inside<INameOfOperation>(call) && !Within(analysis.ContainingSymbol, commits));

    // --- [DUPLICATES]
    private static void Duplicates(CompilationStartAnalysisContext context, ImmutableArray<ISymbol> duplicates, INamedTypeSymbol copies) {
        static IEnumerable<ITypeSymbol?> Tested(IOperation operand) =>
            operand.Parent switch {
                IConversionOperation { IsImplicit: false } conversion => [conversion.Type],
                IIsTypeOperation test => [test.TypeOperand],
                IIsPatternOperation test => [test.Pattern.NarrowedType],
                ISwitchExpressionOperation dispatch when dispatch.Value == operand => dispatch.Arms.Select(static ITypeSymbol? (arm) => arm.Pattern.NarrowedType),
                _ => [],
            };
        static (SyntaxNode Test, ITypeSymbol Narrowed)? Narrowing(IOperation operand, INamedTypeSymbol copied) =>
            operand.Parent is { } parent && Tested(operand).FirstOrDefault(type => !Same(type, copied) && Derives(type, copied)) is { } narrowed ? (parent.Syntax, narrowed) : null;
        Report<IInvocationOperation>(context, [OperationKind.Invocation], (invocation, analysis) => {
            if (!Overridden(invocation.TargetMethod).Any(method => Among(method, duplicates)) || Within(analysis.ContainingSymbol, copies)
                || invocation is not { Type: INamedTypeSymbol copied, Instance.Type: not null, SemanticModel: { } model }) return [];
            Reading reading = new(model, analysis.CancellationToken);
            (SyntaxNode? site, ISymbol holder, IEnumerable<IOperation> operands) = Consumer(invocation) switch {
                IArgumentOperation { Parameter: { } parameter } argument => (argument.Syntax, parameter.ContainingSymbol,
                    Declarations(parameter.ContainingSymbol.OriginalDefinition, reading).SelectMany(body => References(parameter, body, reading))),
                IVariableInitializerOperation { Parent: IVariableDeclaratorOperation { Symbol: var local } } => (null, analysis.ContainingSymbol,
                    References(local, invocation, reading).OrderBy(static reference => reference.Syntax.SpanStart).TakeWhile(static reference => !Written(reference))),
                _ => (null, analysis.ContainingSymbol, [invocation]),
            };
            return operands.Select(operand => Narrowing(operand, copied)).OfType<(SyntaxNode Test, ITypeSymbol Narrowed)>().Select(found => Diagnostic.Create(
                Narrowed,
                (site ?? found.Test).GetLocation(),
                additionalLocations: site is null ? null : [found.Test.GetLocation()],
                messageArgs: [Member(holder).Name, invocation.Syntax, found.Narrowed.Name]));
        });
    }

    // --- [OWNERS]
    private static void Owners(CompilationStartAnalysisContext context, ImmutableArray<IMethodSymbol> subscriptions, INamedTypeSymbol type, INamedTypeSymbol? accessor, INamedTypeSymbol? target) {
        static string Full(ITypeSymbol owner) =>
            owner.OriginalDefinition.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat.WithGlobalNamespaceStyle(SymbolDisplayGlobalNamespaceStyle.Omitted).WithGenericsOptions(SymbolDisplayGenericsOptions.None));
        Compilation compilation = context.Compilation;
        bool Accessible(ITypeSymbol owner) => compilation.IsSymbolAccessibleWithin(owner, compilation.Assembly);
        (string Event, string Owner, bool Matches, bool Nameable)? Subscribed(IOperation step, ITypeSymbol written) =>
            step switch {
                IEventAssignmentOperation { EventReference: IEventReferenceOperation reference } =>
                    (reference.Event.Name, reference.Event.ContainingType.Name, Same(reference.Event.ContainingType, written), Accessible(reference.Event.ContainingType)),
                IInvocationOperation { TargetMethod: { Parameters: { IsEmpty: false } parameters } method } when Marked(method, accessor) is { } data && parameters[0] is var first =>
                    (data.NamedArguments.Select(static named => named.Value.Value).OfType<string>().FirstOrDefault() ?? method.Name) is var member
                    && Marked(first, target)?.ConstructorArguments.FirstOrDefault().Value is string named && named.Split(',')[0].Trim() is var owner
                        ? (member, owner, string.Equals(owner, Full(written), StringComparison.Ordinal), compilation.GetTypesByMetadataName(owner).Any(Accessible))
                        : (member, first.Type.Name, Same(first.Type, written), Accessible(first.Type)),
                _ => null,
            };
        Report<IInvocationOperation>(context, [OperationKind.Invocation], (call, _) =>
            Among(call.TargetMethod, subscriptions)
            && call.Arguments.FirstOrDefault(argument => Same(argument.Parameter?.Type, type)) is { Value: var owned } && Stripped(owned) is ITypeOfOperation { TypeOperand: var written } typed
            && call.Arguments.FirstOrDefault(static argument => argument.Parameter?.Type.TypeKind is TypeKind.Delegate) is { Value: var added }
            && Lambda(added) is { } function
            && function.Body.Descendants().Select(step => Subscribed(step, written)).FirstOrDefault(static found => found is not null) is { Matches: false, Nameable: true } found
                ? [Diagnostic.Create(Owner, typed.Syntax.GetLocation(), found.Owner, found.Event, written.Name)]
                : []);
    }
}
