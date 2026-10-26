using System.Collections.Concurrent;
using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Operations;

namespace Rasm.Analyzers.Rhino;

// --- [COMPOSITION] ---------------------------------------------------------------------
[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class RhinoAnalyzer : RasmAnalyzer {
    // --- [RULES]
    internal const string Host = "RhinoCommon";

    internal const string Library = "Rasm.Rhino";

    public static readonly DiagnosticDescriptor Count = Rule(
        "RASM2002",
        Host,
        "Unchecked item count",
        "Item count from `{0}` reaches no `==` or `!=`. Compare it with `{1}` count and fail with an error naming `{0}` and both counts",
        "Count returned by a RhinoCommon delete, remove, set, or attach method over a collection, unread by `==` or `!=`.");

    public static readonly DiagnosticDescriptor OffUiThread = Rule(
        "RASM2003",
        Host,
        "RhinoDoc access off the UI thread",
        "`RhinoDoc` read `{0}` runs off the UI thread in a `{1}` callback. Read it in an `IO` run through `OnUiThread()` and pass the callback values or duplicated geometry",
        "`RhinoDoc` expression inside a lambda passed to `Task`, `TaskFactory`, `Parallel`, `ThreadPool`, or `Thread`.");

    public static readonly DiagnosticDescriptor Active = Rule(
        "RASM2004",
        Host,
        "ActiveDoc read with a document in scope",
        "`RhinoDoc.ActiveDoc` read where `{0}` holds a `RhinoDoc`. Replace with `{0}`",
        "`RhinoDoc.ActiveDoc` read outside an `==` or `!=` comparison where a `RhinoDoc` parameter, local, field, or property is in scope.");

    public static readonly DiagnosticDescriptor Script = Rule(
        "RASM2005",
        Host,
        "RunScript in a command without ScriptRunner style",
        "Command `{0}` calls `RhinoApp.RunScript` without `Style.ScriptRunner`. Add `Style.ScriptRunner` to `[CommandStyle]` on `{0}`",
        "`RhinoApp.RunScript` call in a `Command` subclass with a `CommandStyle` lacking `Style.ScriptRunner`.");

    public static readonly DiagnosticDescriptor Override = Rule(
        "RASM2007",
        Host,
        "SetDisplayModeOverride result read",
        "`{0}` returns false after storing the override. Discard the result with `_ =`",
        "`ObjectAttributes.SetDisplayModeOverride` result read outside a discard or an expression statement.");

    public static readonly DiagnosticDescriptor Mac = Rule(
        "RASM2008",
        Host,
        "macOS-breaking member outside a macOS guard",
        "`{0}` runs outside the false branch of `OperatingSystem.IsMacOS()`, and on macOS it {1}. {2}",
        "`NamedViewTable.RestoreWithAspectRatio` or `Environment.GetCommandLineArgs` outside the false branch of an `OperatingSystem.IsMacOS()` conditional, `!IsMacOS() &&`, or `IsMacOS() ||`.");

    public static readonly DiagnosticDescriptor Grayscale = Rule(
        "RASM2009",
        Host,
        "GrayscaleDib result read without a null check",
        "`{0}` returns null on macOS. Read it through `Refused.UnlessFound`, `Optional`, or the `DisposalOps.Found` acquire",
        "`ZBufferCapture.GrayscaleDib` result read outside the first argument of `Refused.UnlessFound` or `Optional` and the `DisposalOps.Found` acquire.");

    public static readonly DiagnosticDescriptor Page = Rule(
        "RASM2010",
        Host,
        "PageControl builds the page view",
        "`{0}.PageControl` reaches `{1}`, and Rhino calls `PageControl` on panel close for every page and disposes the result. Return the held control and realize it in `OnActivate(true)`",
        "`ObjectPropertiesPage.PageControl` getter that creates an Eto control, calls `DisposalOps.Acquire` or `Within`, or calls a `Rasm.Rhino.UI` `Open` or `Realize`, directly or through a member of its type.");

    public static readonly DiagnosticDescriptor Docked = Rule(
        "RASM2011",
        Host,
        "Floating view docked by assignment",
        "`{0}` destroys a floated view on macOS in place of docking it. Delete the assignment",
        "`false` constant assigned to `RhinoView.Floating`.");

    public static readonly DiagnosticDescriptor Mode = Rule(
        "RASM2012",
        Host,
        "Owned DisplayModeDescription left undisposed",
        "`{0}` returns an owned `DisplayModeDescription` copy that no release reaches. Acquire it through `DisposalOps.Found`",
        "`DisplayModeDescription.GetDisplayMode`, `DisplayModeDescription.FindByName`, or `RhinoViewport.DisplayMode` read reaching no `Dispose`, `using`, `DisposalOps` or `Copies.Owned` acquire, `DisposalOps` release, `new Disposal<T>`, or `retain` call.");

    public static readonly DiagnosticDescriptor Settings = Rule(
        "RASM2101",
        Library,
        "PersistentSettings read outside SettingType",
        "`{0}` reads `PersistentSettings` outside `SettingType`. Call `{1}`",
        "`PersistentSettings` `Get` or `TryGet` read outside `SettingType` and `SettingType<T>`.");

    public static readonly DiagnosticDescriptor Redraw = Rule(
        "RASM2102",
        Library,
        "View redraw outside Commits",
        "`{0}` controls view redraw outside `Commits`. Pass a `RedrawPolicy` to `Commits.Commit` or `Commits.WithinRedraw`",
        "`ViewTable.Redraw`, `ViewTable.EnableRedraw`, or `RhinoView.Redraw` call or `ViewTable.RedrawEnabled` write outside `Commits`.");

    public static readonly DiagnosticDescriptor Narrowed = Rule(
        "RASM2103",
        Library,
        "Cast or type test on a GeometryBase duplicate",
        "Cast or type test in `{0}` narrows `{1}` to `{2}`. Bind `IO<{5}>` from `Copies.{3}({4}, <retain>)`",
        "Cast, type test, or switch narrowing a `GeometryBase.Duplicate` or `DuplicateShallow` result outside `Copies`.");

    public static readonly DiagnosticDescriptor Toggled = Rule(
        "RASM2104",
        Library,
        "Conduit toggled outside Conduits",
        "`{0}` toggles a conduit outside `Conduits`. Declare it as a `ConduitDefinition` for `Conduits.Enable`",
        "`DisplayConduit.Enabled` write outside `Conduits`.");

    private static readonly ImmutableArray<(string Owner, string Member, string Fact, string Action)> MacBroken = [
        ("Rhino.DocObjects.Tables.NamedViewTable", "RestoreWithAspectRatio", "asserts and aborts Rhino when a docked target's aspect differs from the named view's", "Guard the call with `!OperatingSystem.IsMacOS() &&`"),
        ("System.Environment", "GetCommandLineArgs", "returns only Rhino's .NET startup assembly", "Read macOS launch arguments through `SafeNativeMethods.Arguments()`"),
    ];

    private static readonly ImmutableArray<(string Owner, string Member)> Counted = [
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

    private static readonly ImmutableArray<string> Schedulers = [
        "System.Threading.Tasks.Parallel", "System.Threading.ThreadPool", "System.Threading.Thread", "System.Threading.Tasks.TaskFactory", "System.Threading.Tasks.TaskFactory`1", "System.Threading.Tasks.Task",
    ];

    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => [Count, OffUiThread, Active, Script, Override, Mac, Grayscale, Page, Docked, Mode, Settings, Redraw, Narrowed, Toggled];

    protected override void Start(CompilationStartAnalysisContext context) {
        Compilation compilation = context.Compilation;
        if (compilation.GetTypeByMetadataName("Rhino.RhinoDoc") is not { } document) return;
        Counts(context);
        Threads(context, document);
        ActiveDocs(context, document);
        Scripts(context);
        Overrides(context);
        Platforms(context);
        Captures(context);
        Pages(context);
        Writes(context, "P:Rhino.Display.RhinoView.Floating", Docked, static (assignment, _) => assignment is ISimpleAssignmentOperation { Value.ConstantValue: { HasValue: true, Value: false } });
        if (compilation.GetTypeByMetadataName("Rhino.Display.DisplayModeDescription") is { } description && Lifetimes.Of(compilation) is { } lifetimes) Modes(context, lifetimes, description);
        Reads(context);
        Redraws(context);
        Duplicates(context);
        if (compilation.GetTypeByMetadataName("Rasm.Rhino.Display.Conduits") is { } conduits)
            Writes(context, "P:Rhino.Display.DisplayConduit.Enabled", Toggled, (_, analysis) => !Within(analysis.ContainingSymbol, conduits));
    }

    // --- [COUNTS]
    private static void Counts(CompilationStartAnalysisContext context) {
        static bool Collected(IParameterSymbol? parameter) => parameter is { RefKind: RefKind.None, Type: var type } && Enumerated(type) is not null;
        ImmutableArray<IMethodSymbol> counting = [
            .. Counted.SelectMany(row => Members(context.Compilation.GetTypeByMetadataName(row.Owner), row.Member))
                .Where(static method => method.ReturnType.SpecialType is SpecialType.System_Int32 && method.Parameters.Any(Collected)),
        ];
        if (counting.IsEmpty) return;
        Report<IInvocationOperation>(context, [OperationKind.Invocation], (invocation, _) =>
            Among(invocation.TargetMethod, counting) && Consumer(invocation) is not IBinaryOperation { OperatorKind: BinaryOperatorKind.Equals or BinaryOperatorKind.NotEquals }
                ? [Diagnostic.Create(Count, invocation.Syntax.GetLocation(), invocation.TargetMethod.Name, invocation.Arguments.First(static argument => Collected(argument.Parameter)).Value.Syntax)]
                : []);
    }

    // --- [THREADS]
    private static void Threads(CompilationStartAnalysisContext context, INamedTypeSymbol document) {
        ImmutableArray<INamedTypeSymbol> schedulers = [.. Schedulers.Select(context.Compilation.GetTypeByMetadataName).OfType<INamedTypeSymbol>()];
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
    }

    // --- [ACTIVE_DOCS]
    private static void ActiveDocs(CompilationStartAnalysisContext context, INamedTypeSymbol document) {
        if (Declared(context.Compilation, "P:Rhino.RhinoDoc.ActiveDoc").FirstOrDefault() is not { } active) return;
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
    private static void Scripts(CompilationStartAnalysisContext context) {
        Compilation compilation = context.Compilation;
        if (compilation.GetTypeByMetadataName("Rhino.Commands.Command") is not { } command
            || compilation.GetTypeByMetadataName("Rhino.Commands.CommandStyleAttribute") is not { } attribute
            || Declared(compilation, "F:Rhino.Commands.Style.ScriptRunner").FirstOrDefault() is not IFieldSymbol { ConstantValue: int runner }) return;
        ImmutableArray<ISymbol> scripts = Declared(
            compilation,
            "M:Rhino.RhinoApp.RunScript(System.String,System.Boolean)",
            "M:Rhino.RhinoApp.RunScript(System.String,System.String,System.Boolean)",
            "M:Rhino.RhinoApp.RunScript(System.UInt32,System.String,System.Boolean)",
            "M:Rhino.RhinoApp.RunScript(System.UInt32,System.String,System.String,System.Boolean)");
        bool Runs(INamedTypeSymbol? type) =>
            type is not null && (Marked(type, attribute) is { } style
                ? style.ConstructorArguments is { Length: 1 } arguments && arguments[0].Value is int styles && (styles & runner) is not 0
                : Runs(type.BaseType));
        Report<IInvocationOperation>(context, [OperationKind.Invocation], (invocation, analysis) =>
            Among(invocation.TargetMethod, scripts) && analysis.ContainingSymbol.ContainingType is { } owner && Derives(owner, command) && !Runs(owner)
                ? [Diagnostic.Create(Script, invocation.Syntax.GetLocation(), owner.Name)]
                : []);
    }

    // --- [OVERRIDES]
    private static void Overrides(CompilationStartAnalysisContext context) {
        ImmutableArray<ISymbol> setters = Declared(
            context.Compilation,
            "M:Rhino.DocObjects.ObjectAttributes.SetDisplayModeOverride(Rhino.Display.DisplayModeDescription)",
            "M:Rhino.DocObjects.ObjectAttributes.SetDisplayModeOverride(Rhino.Display.DisplayModeDescription,System.Guid)");
        if (setters.IsEmpty) return;
        Report<IInvocationOperation>(context, [OperationKind.Invocation], Override, (invocation, _) =>
            Among(invocation.TargetMethod, setters) && Consumer(invocation) is not (IExpressionStatementOperation or ISimpleAssignmentOperation { Target: IDiscardOperation }));
    }

    // --- [PLATFORMS]
    private static void Platforms(CompilationStartAnalysisContext context) {
        Compilation compilation = context.Compilation;
        if (Declared(compilation, "M:System.OperatingSystem.IsMacOS").FirstOrDefault() is not { } mac) return;
        ImmutableArray<(ISymbol Member, string Fact, string Action)> broken = [
            .. MacBroken.SelectMany(row => Members(compilation.GetTypeByMetadataName(row.Owner), row.Member).Select(member => ((ISymbol)member, row.Fact, row.Action))),
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
    private static void Captures(CompilationStartAnalysisContext context) {
        Compilation compilation = context.Compilation;
        if (Declared(compilation, "M:Rhino.Display.ZBufferCapture.GrayscaleDib").FirstOrDefault() is not { } grayscale) return;
        ImmutableArray<IMethodSymbol> checks = [
            .. Members(compilation.GetTypeByMetadataName("Rasm.Rhino.Refused"), "UnlessFound"), .. Members(compilation.GetTypeByMetadataName("LanguageExt.Prelude"), "Optional"),
        ];
        ImmutableArray<IMethodSymbol> found = Members(compilation.GetTypeByMetadataName("Rasm.Rhino.DisposalOps"), "Found");
        static bool First(IOperation? consumer, ImmutableArray<IMethodSymbol> owners) =>
            consumer is IArgumentOperation { Parameter.Ordinal: 0, Parent: IInvocationOperation call } && Among(call.TargetMethod, owners);
        Report<IInvocationOperation>(context, [OperationKind.Invocation], Grayscale, (invocation, _) =>
            Same(invocation.TargetMethod, grayscale) && Consumer(invocation) is var consumer && consumer is not IExpressionStatementOperation && !First(consumer, checks)
            && !(consumer is IReturnOperation { Parent: IBlockOperation { Parent: IAnonymousFunctionOperation function } } && Consumer(function) is IDelegateCreationOperation creation && First(Consumer(creation), found)));
    }

    // --- [PAGES]
    private static void Pages(CompilationStartAnalysisContext context) {
        Compilation compilation = context.Compilation;
        if (compilation.GetTypeByMetadataName("Rhino.UI.ObjectPropertiesPage") is not { } page || compilation.GetTypeByMetadataName("Eto.Forms.Control") is not { } control
            || Declared(compilation, "P:Rhino.UI.ObjectPropertiesPage.PageControl").FirstOrDefault() is not IPropertySymbol shown) return;
        ImmutableArray<IMethodSymbol> scopes = Members(compilation.GetTypeByMetadataName("Rasm.Rhino.DisposalOps"), "Acquire", "Within");
        static bool Viewed(INamespaceSymbol? space) =>
            space?.ToDisplayString() is { } name && (string.Equals(name, "Rasm.Rhino.UI", StringComparison.Ordinal) || name.StartsWith("Rasm.Rhino.UI.", StringComparison.Ordinal));
        string? Realization(IOperation operation) =>
            operation switch {
                IObjectCreationOperation { Constructor: { } made } when Derives(made.ContainingType, control) => Label(made),
                IInvocationOperation call when Among(call.TargetMethod, scopes) || (call.TargetMethod.Name is "Open" or "Realize" && Viewed(call.TargetMethod.ContainingNamespace)) => Label(call.TargetMethod),
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
    private static void Modes(CompilationStartAnalysisContext context, Lifetimes lifetimes, INamedTypeSymbol description) {
        ImmutableArray<ISymbol> owned = Declared(
            context.Compilation,
            "M:Rhino.Display.DisplayModeDescription.GetDisplayMode(System.Guid)",
            "M:Rhino.Display.DisplayModeDescription.FindByName(System.String)",
            "P:Rhino.Display.RhinoViewport.DisplayMode");
        Report<IOperation>(context, [OperationKind.Invocation, OperationKind.PropertyReference], Mode, (operation, analysis) =>
            !Written(operation) && Among(Target(operation), owned) && !Inside<INameOfOperation>(operation) && operation.SemanticModel is { } model
            && !lifetimes.Released(operation, description, new Reading(model, analysis.CancellationToken)).Any());
    }

    // --- [READS]
    private static void Reads(CompilationStartAnalysisContext context) {
        Compilation compilation = context.Compilation;
        if (compilation.GetTypeByMetadataName("Rhino.PersistentSettings") is not { } settings
            || compilation.GetTypeByMetadataName("Rasm.Rhino.Persistence.Settings.SettingType") is not { } rows
            || compilation.GetTypeByMetadataName("Rasm.Rhino.Persistence.Settings.SettingType`1") is not { } row) return;
        ImmutableArray<IMethodSymbol> methods = [
            .. settings.GetMembers().OfType<IMethodSymbol>().Where(static method => method is { IsStatic: false, Parameters: { IsEmpty: false } parameters } && parameters[0].Type.SpecialType is SpecialType.System_String),
        ];
        ImmutableHashSet<string> kinds = [
            .. methods.Where(static method => method is { ReturnsVoid: true, Parameters.Length: 2 } && method.Name.StartsWith("Set", StringComparison.Ordinal))
                .GroupBy(static method => method.Name.Substring(3), StringComparer.Ordinal)
                .Where(static family => family.Select(static method => method.Parameters[1].Type).Distinct(SymbolEqualityComparer.Default).Count() is 1)
                .Select(static family => family.Key),
        ];
        string? Kind(string name) =>
            ((ImmutableArray<string>)["TryGet", "Get"]).Where(prefix => name.StartsWith(prefix, StringComparison.Ordinal)).Select(prefix => name.Substring(prefix.Length)).FirstOrDefault(kinds.Contains);
        ImmutableArray<(IMethodSymbol Read, string Kind)> reads = [
            .. methods.Where(static method => method.Parameters.Length > 1).Select(method => (Read: method, Kind: Kind(method.Name))).Where(static read => read.Kind is not null).Select(static read => (read.Read, read.Kind!)),
        ];
        Report<IInvocationOperation>(context, [OperationKind.Invocation], (invocation, analysis) =>
            invocation is { Instance: { } source, TargetMethod: var method } && reads.FirstOrDefault(read => Same(method, read.Read)).Kind is { } kind
            && !Within(analysis.ContainingSymbol, rows) && !Within(analysis.ContainingSymbol, row)
                ? [Diagnostic.Create(
                    Settings,
                    invocation.Syntax.GetLocation(),
                    method.Name,
                    $"SettingType.{(method.IsGenericMethod ? $"Enumeration<{method.TypeArguments[0].ToDisplayString(SymbolDisplayFormat.MinimallyQualifiedFormat)}>()" : kind)}.Read({source.Syntax}, {invocation.Arguments[0].Value.Syntax})")]
                : []);
    }

    // --- [REDRAWS]
    private static void Redraws(CompilationStartAnalysisContext context) {
        Compilation compilation = context.Compilation;
        if (compilation.GetTypeByMetadataName("Rasm.Rhino.Document.Commits") is not { } commits) return;
        ImmutableArray<ISymbol> redraws = Declared(
            compilation,
            "M:Rhino.DocObjects.Tables.ViewTable.Redraw",
            "M:Rhino.DocObjects.Tables.ViewTable.Redraw(System.Boolean)",
            "M:Rhino.DocObjects.Tables.ViewTable.EnableRedraw(System.Boolean,System.Boolean,System.Boolean)",
            "M:Rhino.Display.RhinoView.Redraw",
            "P:Rhino.DocObjects.Tables.ViewTable.RedrawEnabled");
        Report<IOperation>(context, [OperationKind.Invocation, OperationKind.MethodReference, OperationKind.PropertyReference], Redraw, (call, analysis) =>
            (call is not IPropertyReferenceOperation || Written(call)) && Among(Target(call), redraws) && !Inside<INameOfOperation>(call) && !Within(analysis.ContainingSymbol, commits));
    }

    // --- [DUPLICATES]
    private static void Duplicates(CompilationStartAnalysisContext context) {
        Compilation compilation = context.Compilation;
        if (compilation.GetTypeByMetadataName("Rasm.Rhino.Modeling.Copies") is not { } copies) return;
        ImmutableArray<ISymbol> duplicates = Declared(compilation, "M:Rhino.Geometry.GeometryBase.Duplicate", "M:Rhino.Geometry.GeometryBase.DuplicateShallow");
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
                || invocation is not { Type: INamedTypeSymbol copied, Instance: { Type: { } origin } source }) return [];
            (SyntaxNode? site, ISymbol holder, IEnumerable<IOperation> operands) = Consumer(invocation) switch {
                IArgumentOperation { Parameter: { } parameter } argument when invocation.SemanticModel is { } model => (argument.Syntax, parameter.ContainingSymbol,
                    Declarations(parameter.ContainingSymbol.OriginalDefinition, new Reading(model, analysis.CancellationToken)).SelectMany(body => Uses(parameter, body))),
                IVariableInitializerOperation { Parent: IVariableDeclaratorOperation { Symbol: var local } } => (null, analysis.ContainingSymbol,
                    Uses(local, invocation).OrderBy(static reference => reference.Syntax.SpanStart).TakeWhile(static reference => !Written(reference))),
                _ => (null, analysis.ContainingSymbol, [invocation]),
            };
            return operands.Select(operand => Narrowing(operand, copied)).OfType<(SyntaxNode Test, ITypeSymbol Narrowed)>().Select(found => Diagnostic.Create(
                Narrowed,
                (site ?? found.Test).GetLocation(),
                additionalLocations: site is null ? null : [found.Test.GetLocation()],
                messageArgs: [Member(holder).Name, invocation.Syntax, found.Narrowed.Name, invocation.TargetMethod.Name, source.Syntax, origin.Name]));
        });
    }
}
