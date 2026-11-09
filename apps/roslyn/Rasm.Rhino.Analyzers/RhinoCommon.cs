using System.Collections.Concurrent;
using System.Collections.Immutable;
using Analyzer.Utilities;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Operations;
using Rasm.Analyzers;

namespace Rasm.Rhino.Analyzers;

// --- [COMPOSITION] ---------------------------------------------------------------------

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class UncheckedCountAnalyzer : DiagnosticAnalyzer {

    // --- [RULES]

    private static readonly DiagnosticDescriptor Rule = new(
        "RASM2002",
        "Unchecked item count, compare with input count",
        "`{0}` count reaches no `==` or `!=`, compare with `{1}` count and fail naming `{0}` and both counts",
        Categories.RhinoCommon,
        DiagnosticSeverity.Error,
        isEnabledByDefault: true,
        "RhinoCommon delete, remove, set, and attach counts over a collection reach `==` or `!=`.",
        Link("RASM2002"));

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

    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => [Rule];

    public override void Initialize(AnalysisContext context) {
        context.EnableConcurrentExecution();
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
        context.RegisterCompilationStartAction(static start => {
            WellKnownTypeProvider types = WellKnownTypeProvider.GetOrCreate(start.Compilation);
            static bool Collected(IParameterSymbol? parameter) => parameter is { RefKind: RefKind.None, Type: var type } && Enumerated(type) is not null;
            ImmutableArray<IMethodSymbol> counting = [.. Counts.SelectMany(row => Members(types.GetOrCreateTypeByMetadataName(row.Owner), row.Member))
                .Where(static method => method.ReturnType.SpecialType is SpecialType.System_Int32 && method.Parameters.Any(Collected))];
            if (counting.IsEmpty) return;
            Report<IInvocationOperation>(start, [OperationKind.Invocation], (invocation, _) =>
                Among(invocation.TargetMethod, counting) && Consumer(invocation) is not IBinaryOperation { OperatorKind: BinaryOperatorKind.Equals or BinaryOperatorKind.NotEquals }
                    ? [Diagnostic.Create(Rule, invocation.Syntax.GetLocation(), invocation.TargetMethod.Name, invocation.Arguments.First(static argument => Collected(argument.Parameter)).Value.Syntax)]
                    : []);
        });
    }
}

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class OffUiThreadAnalyzer : DiagnosticAnalyzer {

    // --- [RULES]

    private static readonly DiagnosticDescriptor Rule = new(
        "RASM2003",
        "`RhinoDoc` read off UI thread, read through `OnUiThread()`",
        "`{0}` reads `RhinoDoc` off UI thread in `{1}`, read in an `OnUiThread()` `IO` and pass values or duplicated geometry",
        Categories.RhinoCommon,
        DiagnosticSeverity.Error,
        isEnabledByDefault: true,
        "Lambdas passed to `Task`, `TaskFactory`, `Parallel`, `ThreadPool`, or `Thread` read no `RhinoDoc` expression.",
        Link("RASM2003"));

    private static readonly ImmutableArray<string> Schedulers = [
        "System.Threading.Tasks.Parallel", "System.Threading.ThreadPool", "System.Threading.Thread", "System.Threading.Tasks.TaskFactory", "System.Threading.Tasks.TaskFactory`1", "System.Threading.Tasks.Task",
    ];

    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => [Rule];

    public override void Initialize(AnalysisContext context) {
        context.EnableConcurrentExecution();
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
        context.RegisterCompilationStartAction(static start => {
            WellKnownTypeProvider types = WellKnownTypeProvider.GetOrCreate(start.Compilation);
            if (types.GetOrCreateTypeByMetadataName("Rhino.RhinoDoc") is not { } document || types.GetOrCreateTypeByMetadataName("Rasm.Rhino.Session") is null) return;
            ImmutableArray<INamedTypeSymbol> schedulers = [.. Schedulers.Select(types.GetOrCreateTypeByMetadataName).OfType<INamedTypeSymbol>()];
            Report<IOperation>(start, [OperationKind.Invocation, OperationKind.ObjectCreation], (operation, _) =>
                operation switch {
                    IInvocationOperation invocation => (invocation.TargetMethod, invocation.Arguments),
                    IObjectCreationOperation creation => (creation.Constructor, creation.Arguments),
                    _ => default,
                } is ( { } method, var arguments) && Among(method.ContainingType, schedulers) && Delegated(method)
                    ? arguments.SelectMany(static argument => argument.Value.DescendantsAndSelf().OfType<IAnonymousFunctionOperation>().Take(1))
                        .SelectMany(static callback => callback.Body.Descendants())
                        .Where(reach => reach is not IConversionOperation && Same(reach.Type, document) && !Same(Consumer(reach)?.Type, document))
                        .Select(reach => Diagnostic.Create(Rule, reach.Syntax.GetLocation(), reach.Syntax, Label(method)))
                    : []);
        });
    }
}

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class ActiveDocumentAnalyzer : DiagnosticAnalyzer {

    // --- [RULES]

    private static readonly DiagnosticDescriptor Rule = new(
        "RASM2004",
        "`ActiveDoc` read with document in scope, read the held document",
        "`RhinoDoc.ActiveDoc` read where `{0}` holds a `RhinoDoc`, write `{0}`",
        Categories.RhinoCommon,
        DiagnosticSeverity.Error,
        isEnabledByDefault: true,
        "Read an in-scope `RhinoDoc` parameter, local, field, or property over `RhinoDoc.ActiveDoc` outside `==` or `!=` comparisons.",
        Link("RASM2004"));

    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => [Rule];

    public override void Initialize(AnalysisContext context) {
        context.EnableConcurrentExecution();
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
        context.RegisterCompilationStartAction(static start => {
            if (WellKnownTypeProvider.GetOrCreate(start.Compilation).GetOrCreateTypeByMetadataName("Rhino.RhinoDoc") is not { } document
                || Declared(start.Compilation, "P:Rhino.RhinoDoc.ActiveDoc").FirstOrDefault() is not { } active) return;
            static bool Compared(IOperation operation) {
                while (operation.Parent is IConversionOperation or IConditionalAccessOperation or IMemberReferenceOperation) operation = operation.Parent;
                return operation.Parent is IBinaryOperation { OperatorKind: BinaryOperatorKind.Equals or BinaryOperatorKind.NotEquals };
            }
            Report<IPropertyReferenceOperation>(start, [OperationKind.PropertyReference], (reference, analysis) => {
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
                    ? [Diagnostic.Create(Rule, reference.Syntax.GetLocation(), holder.Name)]
                    : [];
            });
        });
    }
}

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class ScriptRunnerAnalyzer : DiagnosticAnalyzer {

    // --- [RULES]

    private static readonly DiagnosticDescriptor Rule = new(
        "RASM2005",
        "`RunScript` in command without `ScriptRunner` style, add `Style.ScriptRunner`",
        "Command `{0}` calls `RhinoApp.RunScript` without `Style.ScriptRunner`, add `Style.ScriptRunner` to `[CommandStyle]` on `{0}`",
        Categories.RhinoCommon,
        DiagnosticSeverity.Error,
        isEnabledByDefault: true,
        "`Command` subclasses calling `RhinoApp.RunScript` declare `Style.ScriptRunner` in `[CommandStyle]`.",
        Link("RASM2005"));

    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => [Rule];

    public override void Initialize(AnalysisContext context) {
        context.EnableConcurrentExecution();
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
        context.RegisterCompilationStartAction(static start => {
            WellKnownTypeProvider types = WellKnownTypeProvider.GetOrCreate(start.Compilation);
            if (types.GetOrCreateTypeByMetadataName("Rhino.Commands.Command") is not { } command || types.GetOrCreateTypeByMetadataName("Rhino.Commands.CommandStyleAttribute") is not { } attribute
                || Declared(start.Compilation, "F:Rhino.Commands.Style.ScriptRunner").FirstOrDefault() is not IFieldSymbol { ConstantValue: int runner }) return;
            ImmutableArray<IMethodSymbol> scripts = Members(types.GetOrCreateTypeByMetadataName("Rhino.RhinoApp"), "RunScript");
            bool Runs(INamedTypeSymbol type) =>
                Bases(type).Select(held => Marked(held, attribute)).FirstOrDefault(static style => style is not null) is { ConstructorArguments: { Length: 1 } arguments }
                && arguments[0].Value is int styles && (styles & runner) is not 0;
            Report<IInvocationOperation>(start, [OperationKind.Invocation], (invocation, analysis) =>
                Among(invocation.TargetMethod, scripts) && analysis.ContainingSymbol.ContainingType is { } owner && Derives(owner, command) && !Runs(owner)
                    ? [Diagnostic.Create(Rule, invocation.Syntax.GetLocation(), owner.Name)]
                    : []);
        });
    }
}

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class UiMarshalAnalyzer : DiagnosticAnalyzer {

    // --- [RULES]

    private static readonly DiagnosticDescriptor Rule = new(
        "RASM2006",
        "Single call marshaled to UI thread, compose one `OnUiThread()` `IO`",
        "`{0}` marshals one call to UI thread, compose UI-thread calls into one `IO` run through `OnUiThread()`",
        Categories.RhinoCommon,
        DiagnosticSeverity.Error,
        isEnabledByDefault: true,
        "Compose UI-thread calls into one `OnUiThread()` `IO` over `RhinoApp.InvokeOnUiThread` and `InvokeAndWait`, delegate-taking Eto `Application`, main `DispatchQueue`, or `NSObject` main-thread calls outside `Session`.",
        Link("RASM2006"));

    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => [Rule];

    public override void Initialize(AnalysisContext context) {
        context.EnableConcurrentExecution();
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
        context.RegisterCompilationStartAction(static start => {
            WellKnownTypeProvider types = WellKnownTypeProvider.GetOrCreate(start.Compilation);
            INamedTypeSymbol? callbacks = types.GetOrCreateTypeByMetadataName("Rasm.Rhino.Session");
            INamedTypeSymbol? application = types.GetOrCreateTypeByMetadataName("Eto.Forms.Application");
            INamedTypeSymbol? queue = types.GetOrCreateTypeByMetadataName("CoreFoundation.DispatchQueue");
            INamedTypeSymbol? objects = types.GetOrCreateTypeByMetadataName("Foundation.NSObject");
            INamedTypeSymbol? threads = types.GetOrCreateTypeByMetadataName("Foundation.NSThread");
            ImmutableArray<ISymbol> mains = Declared(start.Compilation, "P:CoreFoundation.DispatchQueue.MainQueue", "P:Foundation.NSThread.MainThread");
            ImmutableArray<IMethodSymbol> marshals = [
                .. Members(types.GetOrCreateTypeByMetadataName("Rhino.RhinoApp"), "InvokeOnUiThread", "InvokeAndWait"),
                .. Members(objects, "InvokeOnMainThread", "BeginInvokeOnMainThread"),
            ];
            bool Main(IOperation? value) => value is IMemberReferenceOperation reference && Among(reference.Member, mains);
            Report<IOperation>(start, [OperationKind.Invocation, OperationKind.MethodReference], (operation, analysis) =>
                operation switch {
                    IInvocationOperation invocation => (invocation.TargetMethod, invocation.Instance, invocation.Arguments),
                    IMethodReferenceOperation reference => (reference.Method, reference.Instance, ImmutableArray<IArgumentOperation>.Empty),
                    _ => default,
                } is ( { } method, var instance, var arguments) && !Inside<INameOfOperation>(operation) && !Within(analysis.ContainingSymbol, callbacks)
                && (Among(method, marshals)
                    || (Same(method.ContainingType, application) && Delegated(method))
                    || (Same(method.ContainingType, queue) && Delegated(method) && Main(instance))
                    || (objects is not null && Derives(method.ContainingType, objects) && method.Parameters.Any(parameter => Same(parameter.Type, threads)) && arguments.Any(argument => Main(argument.Value))))
                    ? [Diagnostic.Create(Rule, operation.Syntax.GetLocation(), Label(method))]
                    : []);
        });
    }
}

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class DisplayModeOverrideAnalyzer : DiagnosticAnalyzer {

    // --- [RULES]

    private static readonly DiagnosticDescriptor Rule = new(
        "RASM2007",
        "`SetDisplayModeOverride` result read, discard through `_ =`",
        "`{0}` returns false after storing the override, discard its result with `_ =`",
        Categories.RhinoCommon,
        DiagnosticSeverity.Error,
        isEnabledByDefault: true,
        "`ObjectAttributes.SetDisplayModeOverride` results go to a discard or an expression statement.",
        Link("RASM2007"));

    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => [Rule];

    public override void Initialize(AnalysisContext context) {
        context.EnableConcurrentExecution();
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
        context.RegisterCompilationStartAction(static start => {
            ImmutableArray<IMethodSymbol> overrides = Members(WellKnownTypeProvider.GetOrCreate(start.Compilation).GetOrCreateTypeByMetadataName("Rhino.DocObjects.ObjectAttributes"), "SetDisplayModeOverride");
            if (overrides.IsEmpty) return;
            Report<IInvocationOperation>(start, [OperationKind.Invocation], Rule, (invocation, _) =>
                Among(invocation.TargetMethod, overrides) && Consumer(invocation) is not (IExpressionStatementOperation or ISimpleAssignmentOperation { Target: IDiscardOperation }));
        });
    }
}

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class MacGuardAnalyzer : DiagnosticAnalyzer {

    // --- [RULES]

    private static readonly DiagnosticDescriptor Rule = new(
        "RASM2008",
        "macOS-breaking member outside macOS guard, guard or replace the call",
        "`{0}` runs outside the false branch of `OperatingSystem.IsMacOS()` and on macOS {1}, {2}",
        Categories.RhinoCommon,
        DiagnosticSeverity.Error,
        isEnabledByDefault: true,
        "`NamedViewTable.RestoreWithAspectRatio` and `Environment.GetCommandLineArgs` run in an `IsMacOS()` conditional's false branch, after `!IsMacOS() &&`, or after `IsMacOS() ||`.",
        Link("RASM2008"));

    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => [Rule];

    public override void Initialize(AnalysisContext context) {
        context.EnableConcurrentExecution();
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
        context.RegisterCompilationStartAction(static start => {
            WellKnownTypeProvider types = WellKnownTypeProvider.GetOrCreate(start.Compilation);
            if (Declared(start.Compilation, "M:System.OperatingSystem.IsMacOS").FirstOrDefault() is not { } mac) return;
            ImmutableArray<(ISymbol Member, string Fact, string Action)> broken = [
                .. Members(types.GetOrCreateTypeByMetadataName("Rhino.DocObjects.Tables.NamedViewTable"), "RestoreWithAspectRatio")
                    .Select(static member => ((ISymbol)member, "asserts and aborts Rhino when a docked target's aspect differs from its named view's", "guard it with `!OperatingSystem.IsMacOS() &&`")),
                .. Members(types.GetOrCreateTypeByMetadataName("System.Environment"), "GetCommandLineArgs")
                    .Select(static member => ((ISymbol)member, "returns only Rhino's .NET startup assembly", "read macOS launch arguments through `NSProcessInfo.ProcessInfo.Arguments`")),
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
            bool Guarded(IOperation node) {
                for (IOperation child = node; child.Parent is { } parent; child = parent)
                    if (Shields(parent, child)) return true;
                return false;
            }
            Report<IOperation>(start, [OperationKind.Invocation, OperationKind.MethodReference], (operation, _) =>
                Target(operation) is { } member && broken.FirstOrDefault(row => Same(member, row.Member)) is { Member: not null } row && !Inside<INameOfOperation>(operation) && !Guarded(operation)
                    ? [Diagnostic.Create(Rule, operation.Syntax.GetLocation(), operation.Syntax, row.Fact, row.Action)]
                    : []);
        });
    }
}

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class GrayscaleCaptureAnalyzer : DiagnosticAnalyzer {

    // --- [RULES]

    private static readonly DiagnosticDescriptor Rule = new(
        "RASM2009",
        "`GrayscaleDib` result read without null check, acquire through `Products.Of` or `Conversions.Required`",
        "`{0}` returns null on macOS, acquire it through `Products.Of` or `Conversions.Required`",
        Categories.RhinoCommon,
        DiagnosticSeverity.Error,
        isEnabledByDefault: true,
        "`ZBufferCapture.GrayscaleDib` results enter as first argument of `Optional` or a null-checking acquire.",
        Link("RASM2009"));

    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => [Rule];

    public override void Initialize(AnalysisContext context) {
        context.EnableConcurrentExecution();
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
        context.RegisterCompilationStartAction(static start => {
            WellKnownTypeProvider types = WellKnownTypeProvider.GetOrCreate(start.Compilation);
            INamedTypeSymbol? products = types.GetOrCreateTypeByMetadataName("Rasm.Rhino.Products");
            ImmutableArray<IMethodSymbol> found = Members(products, "Of", "Many", "Maybe", "Transfer");
            if (Declared(start.Compilation, "M:Rhino.Display.ZBufferCapture.GrayscaleDib").FirstOrDefault() is not { } grayscale || found.IsEmpty) return;
            ImmutableArray<IMethodSymbol> checks = [.. Members(types.GetOrCreateTypeByMetadataName("Rasm.Rhino.Conversions"), "Required"), .. Members(types.GetOrCreateTypeByMetadataName("LanguageExt.Prelude"), "Optional")];
            static bool First(IOperation? consumer, ImmutableArray<IMethodSymbol> owners) =>
                consumer is IArgumentOperation { Parameter.Ordinal: 0, Parent: IInvocationOperation call } && Among(call.TargetMethod, owners);
            Report<IInvocationOperation>(start, [OperationKind.Invocation], Rule, (invocation, _) =>
                Same(invocation.TargetMethod, grayscale) && Consumer(invocation) is var consumer && consumer is not IExpressionStatementOperation && !First(consumer, checks)
                && !(consumer is IReturnOperation { Parent: IBlockOperation { Parent: IAnonymousFunctionOperation function } } && Consumer(function) is IDelegateCreationOperation creation && First(Consumer(creation), found)));
        });
    }
}

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class PageControlAnalyzer : DiagnosticAnalyzer {

    // --- [RULES]

    private static readonly DiagnosticDescriptor Rule = new(
        "RASM2010",
        "Page view built in `PageControl`, realize in `OnActivate(true)`",
        "`{0}.PageControl` reaches `{1}` and Rhino disposes every page's result on panel close, return a held control realized in `OnActivate(true)`",
        Categories.RhinoCommon,
        DiagnosticSeverity.Error,
        isEnabledByDefault: true,
        "`ObjectPropertiesPage.PageControl` getters create no Eto control, call no acquire or `Rasm.Rhino.UI` `Open` or `Realize`, directly or through type members.",
        Link("RASM2010"),
        WellKnownDiagnosticTags.CompilationEnd);

    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => [Rule];

    public override void Initialize(AnalysisContext context) {
        context.EnableConcurrentExecution();
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
        context.RegisterCompilationStartAction(static start => {
            Compilation compilation = start.Compilation;
            WellKnownTypeProvider types = WellKnownTypeProvider.GetOrCreate(compilation);
            if (types.GetOrCreateTypeByMetadataName("Rhino.UI.ObjectPropertiesPage") is not { } page || Declared(compilation, "P:Rhino.UI.ObjectPropertiesPage.PageControl").FirstOrDefault() is not IPropertySymbol shown
                || types.GetOrCreateTypeByMetadataName("Eto.Forms.Control") is not { } control) return;
            ImmutableArray<IMethodSymbol> scopes = Members(types.GetOrCreateTypeByMetadataName("Rasm.Rhino.Products"), "Of", "Many", "Maybe", "Transfer");
            ISymbol? views = Declared(compilation, "N:Rasm.Rhino.UI").FirstOrDefault();
            ImmutableArray<IMethodSymbol> callbacks = Members(types.GetOrCreateTypeByMetadataName("LanguageExt.IO"), "lift", "liftAsync", "liftVAsync", "pure");
            SemanticModels models = new(start);
            ControlFlow flow = ControlFlow.For(compilation);
            IMethodSymbol? Realization(IOperation operation) =>
                operation switch {
                    IObjectCreationOperation { Constructor: { } made } when Derives(made.ContainingType, control) => made,
                    IInvocationOperation call when Among(call.TargetMethod, scopes)
                        || (call.TargetMethod.Name is "Open" or "Realize" && Namespaced(compilation, call.TargetMethod, views)) => call.TargetMethod,
                    _ => null,
                };
            ConcurrentDictionary<INamedTypeSymbol, ConcurrentDictionary<ISymbol, (IMethodSymbol? Realized, ImmutableArray<IOperation> Operations)>> pages = new(SymbolEqualityComparer.Default);
            start.RegisterSymbolStartAction(
                symbol => {
                    if (symbol.Symbol is not INamedTypeSymbol type || !Derives(type.BaseType, page)) return;
                    ConcurrentDictionary<ISymbol, (IMethodSymbol? Realized, ImmutableArray<IOperation> Operations)> bodies = pages.GetOrAdd(type, static _ => new(SymbolEqualityComparer.Default));
                    symbol.RegisterOperationBlockAction(block => {
                        foreach (IOperation root in block.OperationBlocks.SelectMany(static body => body.DescendantsAndSelf().Where(operation => operation == body || operation is IAnonymousFunctionOperation or ILocalFunctionOperation))) {
                            ISymbol owner = (root switch {
                                IAnonymousFunctionOperation function => function.Symbol,
                                ILocalFunctionOperation function => function.Symbol,
                                _ => block.OwningSymbol,
                            }).OriginalDefinition;
                            ImmutableArray<IOperation> operations = [.. ControlFlow.Operations(root, block.CancellationToken)];
                            (IMethodSymbol? Realized, ImmutableArray<IOperation> Operations) read = (operations.Select(Realization).FirstOrDefault(static made => made is not null),
                                [.. operations.Where(static operation => operation is IPropertyReferenceOperation or IFieldReferenceOperation or IInvocationOperation)]);
                            _ = bodies.AddOrUpdate(owner, read, (_, prior) => (prior.Realized ?? read.Realized, prior.Operations.AddRange(read.Operations)));
                        }
                    });
                },
                SymbolKind.NamedType);
            models.Complete((end, provided) => {
                foreach (KeyValuePair<INamedTypeSymbol, ConcurrentDictionary<ISymbol, (IMethodSymbol? Realized, ImmutableArray<IOperation> Operations)>> type in pages) {
                    Dictionary<ISymbol, (IMethodSymbol? Realized, ImmutableArray<ISymbol> Reads)> bodies = [with(SymbolEqualityComparer.Default)];
                    foreach (KeyValuePair<ISymbol, (IMethodSymbol? Realized, ImmutableArray<IOperation> Operations)> body in type.Value) {
                        IEnumerable<ISymbol?> Reads(IOperation operation) => operation switch {
                            IPropertyReferenceOperation property when Written(property) =>
                                operation.Parent is ISimpleAssignmentOperation or ITupleOperation ? [property.Property.SetMethod] : [property.Property.GetMethod, property.Property.SetMethod],
                            IPropertyReferenceOperation property => [property.Property.GetMethod],
                            IInvocationOperation call => Calls(call, new Reading(call.SemanticModel!, provided, end.CancellationToken), flow, callbacks),
                            _ => [Target(operation)],
                        };
                        bodies.Add(body.Key, (body.Value.Realized, [.. body.Value.Operations.SelectMany(Reads).OfType<ISymbol>()
                            .Where(member => member is not IEventSymbol && Same(member.ContainingType, type.Key)).Select(static member => member.OriginalDefinition).Distinct(SymbolEqualityComparer.Default)]));
                    }
                    IMethodSymbol? Reached(ISymbol member) {
                        HashSet<ISymbol> seen = [with(SymbolEqualityComparer.Default)];
                        Queue<ISymbol> pending = new([member]);
                        while (pending.Count is > 0) {
                            ISymbol current = pending.Dequeue();
                            end.CancellationToken.ThrowIfCancellationRequested();
                            if (!seen.Add(current) || !bodies.TryGetValue(current, out (IMethodSymbol? Realized, ImmutableArray<ISymbol> Reads) held)) continue;
                            if (held.Realized is { } made) return made;
                            foreach (ISymbol read in held.Reads) pending.Enqueue(read);
                        }
                        return null;
                    }
                    foreach (IPropertySymbol property in type.Key.GetMembers().OfType<IPropertySymbol>().Where(property => !Same(property, shown) && Overridden(property).Any(held => Same(held, shown))))
                        if (property.GetMethod is { } getter && Reached(getter.OriginalDefinition) is { } made) end.ReportDiagnostic(Diagnostic.Create(Rule, property.Locations[0], type.Key.Name, Label(made)));
                }
            });
        });
    }
}

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class PropertyWriteAnalyzer : DiagnosticAnalyzer {

    // --- [RULES]

    private static readonly DiagnosticDescriptor Docked = new(
        "RASM2011",
        "Floating view docked by assignment, delete the assignment",
        "`{0}` destroys a floated view on macOS in place of docking it, delete the assignment",
        Categories.RhinoCommon,
        DiagnosticSeverity.Error,
        isEnabledByDefault: true,
        "`RhinoView.Floating` takes no `false` assignment.",
        Link("RASM2011"));

    private static readonly DiagnosticDescriptor Toggled = new(
        "RASM2104",
        "Conduit toggled outside `Conduits`, toggle through `Conduits`",
        "`{0}` toggles a conduit outside `Conduits`, toggle through the one library enable operation",
        Categories.RhinoCommon,
        DiagnosticSeverity.Error,
        isEnabledByDefault: true,
        "`DisplayConduit.Enabled` writes run inside `Conduits`.",
        Link("RASM2104"));

    private static readonly DiagnosticDescriptor Rendering = new(
        "RASM2105",
        "`RenderSettings` written outside `RenderSettingsSources`, write through `Edit`",
        "`{0}` writes `RhinoDoc.RenderSettings` with no undo record, write through `RenderSettingsSources.Edit`",
        Categories.RhinoCommon,
        DiagnosticSeverity.Error,
        isEnabledByDefault: true,
        "`RhinoDoc.RenderSettings` assignments run inside `RenderSettingsSources`.",
        Link("RASM2105"));

    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => [Docked, Toggled, Rendering];

    public override void Initialize(AnalysisContext context) {
        context.EnableConcurrentExecution();
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
        context.RegisterCompilationStartAction(static start => {
            WellKnownTypeProvider types = WellKnownTypeProvider.GetOrCreate(start.Compilation);
            Writes(start, Declared(start.Compilation, "P:Rhino.Display.RhinoView.Floating").FirstOrDefault(), Docked,
                static (assignment, value, _) => assignment is ISimpleAssignmentOperation or IDeconstructionAssignmentOperation && value?.ConstantValue is { HasValue: true, Value: false });
            if (types.GetOrCreateTypeByMetadataName("Rasm.Rhino.Display.Conduits") is { } conduits)
                Writes(start, Declared(start.Compilation, "P:Rhino.Display.DisplayConduit.Enabled").FirstOrDefault(), Toggled, (_, _, analysis) => !Within(analysis.ContainingSymbol, conduits));
            if (types.GetOrCreateTypeByMetadataName("Rasm.Rhino.Render.Settings.RenderSettingsSources") is { } sources)
                Writes(start, Declared(start.Compilation, "P:Rhino.RhinoDoc.RenderSettings").FirstOrDefault(), Rendering, (_, _, analysis) => !Within(analysis.ContainingSymbol, sources));
        });
    }
}

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class OwnedDisplayModeAnalyzer : DiagnosticAnalyzer {

    // --- [RULES]

    private static readonly DiagnosticDescriptor Rule = new(
        "RASM2012",
        "Owned `DisplayModeDescription` left undisposed, acquire through `Products.Of`",
        "`{0}` returns an owned `DisplayModeDescription` copy no release reaches, acquire it through `Products.Of`",
        Categories.RhinoCommon,
        DiagnosticSeverity.Error,
        isEnabledByDefault: true,
        "`DisplayModeDescription.GetDisplayMode`, `FindByName`, and `RhinoViewport.DisplayMode` copies reach `Dispose`, `using`, `use`, `release`, acquire, or retaining call.",
        Link("RASM2012"),
        WellKnownDiagnosticTags.CompilationEnd);

    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => [Rule];

    public override void Initialize(AnalysisContext context) {
        context.EnableConcurrentExecution();
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
        context.RegisterCompilationStartAction(static start => {
            WellKnownTypeProvider types = WellKnownTypeProvider.GetOrCreate(start.Compilation);
            if (types.GetOrCreateTypeByMetadataName("Rhino.Display.DisplayModeDescription") is not { } description || types.GetOrCreateTypeByMetadataName("Rasm.Rhino.Products") is null
                || Lifetimes.Of(start.Compilation) is not { } lifetimes) return;
            ImmutableArray<ISymbol> owned = [.. Members(description, "GetDisplayMode", "FindByName"), .. Declared(start.Compilation, "P:Rhino.Display.RhinoViewport.DisplayMode")];
            new SemanticModels(start).Report<IOperation>([OperationKind.Invocation, OperationKind.PropertyReference], (operation, _, reading) =>
                !Written(operation) && Among(Target(operation), owned) && !Inside<INameOfOperation>(operation) && !lifetimes.Released(operation, description, reading).Any()
                    ? [Diagnostic.Create(Rule, operation.Syntax.GetLocation(), operation.Syntax)] : []);
        });
    }
}

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class DefaultedSettingAnalyzer : DiagnosticAnalyzer {

    // --- [RULES]

    private static readonly DiagnosticDescriptor Rule = new(
        "RASM2101",
        "`PersistentSettings` read through a default, call `TryGet<Kind>`",
        "`{0}` reads `PersistentSettings` through a default value, call `{1}`",
        Categories.RhinoCommon,
        DiagnosticSeverity.Error,
        isEnabledByDefault: true,
        "Read `PersistentSettings` keys through `TryGet<Kind>` over `Get<Kind>` overloads taking a default value.",
        Link("RASM2101"));

    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => [Rule];

    public override void Initialize(AnalysisContext context) {
        context.EnableConcurrentExecution();
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
        context.RegisterCompilationStartAction(static start => {
            if (WellKnownTypeProvider.GetOrCreate(start.Compilation).GetOrCreateTypeByMetadataName("Rhino.PersistentSettings") is not { } settings) return;
            ImmutableArray<IMethodSymbol> methods = [
                .. settings.GetMembers().OfType<IMethodSymbol>().Where(static method => method is { IsStatic: false, Parameters: { IsEmpty: false } parameters } && parameters[0].Type.SpecialType is SpecialType.System_String),
            ];
            ImmutableArray<(IMethodSymbol Get, IMethodSymbol Try)> defaulted = [
                .. methods.Where(static method => method.Parameters.Length > 1 && method.Name.StartsWith("Get", StringComparison.Ordinal) && Same(method.Parameters[1].Type, method.ReturnType))
                    .SelectMany(get => settings.GetMembers($"Try{get.Name}").OfType<IMethodSymbol>().Where(tried => tried.Arity == get.Arity)
                        .Select(tried => tried.IsGenericMethod ? tried.Construct([.. get.TypeArguments]) : tried)
                        .Where(tried => tried is { ReturnType.SpecialType: SpecialType.System_Boolean, Parameters.Length: 2 } && tried.Parameters[1] is { RefKind: RefKind.Out } result && Same(result.Type, get.ReturnType))
                        .Select(tried => (get, tried))),
            ];
            Report<IInvocationOperation>(start, [OperationKind.Invocation], (invocation, _) =>
                invocation is { Instance: { } source, TargetMethod: var method } && defaulted.FirstOrDefault(pair => Same(method, pair.Get)).Try is { } tried
                    ? [Diagnostic.Create(
                        Rule,
                        invocation.Syntax.GetLocation(),
                        method.Name,
                        $"{source.Syntax}.{tried.Name}{(method.IsGenericMethod ? $"<{string.Join(", ", method.TypeArguments.Select(static argument => argument.ToDisplayString(SymbolDisplayFormat.MinimallyQualifiedFormat)))}>" : "")}({invocation.Arguments.First(static argument => argument.Parameter?.Ordinal is 0).Value.Syntax}, out {method.ReturnType.ToDisplayString(SymbolDisplayFormat.MinimallyQualifiedFormat)} value)")]
                    : []);
        });
    }
}

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class RedrawOwnerAnalyzer : DiagnosticAnalyzer {

    // --- [RULES]

    private static readonly DiagnosticDescriptor Rule = new(
        "RASM2102",
        "View redraw outside `Commits`, pass a `RedrawPolicy`",
        "`{0}` controls view redraw outside `Commits`, pass a `RedrawPolicy` to `Commits.Commit` or `Commits.WithinRedraw`",
        Categories.RhinoCommon,
        DiagnosticSeverity.Error,
        isEnabledByDefault: true,
        "`ViewTable.Redraw`, `ViewTable.EnableRedraw`, and `RhinoView.Redraw` calls and `ViewTable.RedrawEnabled` writes run inside `Commits`.",
        Link("RASM2102"));

    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => [Rule];

    public override void Initialize(AnalysisContext context) {
        context.EnableConcurrentExecution();
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
        context.RegisterCompilationStartAction(static start => {
            WellKnownTypeProvider types = WellKnownTypeProvider.GetOrCreate(start.Compilation);
            if (types.GetOrCreateTypeByMetadataName("Rasm.Rhino.Document.Commits") is not { } commits) return;
            ImmutableArray<ISymbol> redraws = [
                .. Members(types.GetOrCreateTypeByMetadataName("Rhino.DocObjects.Tables.ViewTable"), "Redraw", "EnableRedraw"),
                .. Members(types.GetOrCreateTypeByMetadataName("Rhino.Display.RhinoView"), "Redraw"),
                .. Declared(start.Compilation, "P:Rhino.DocObjects.Tables.ViewTable.RedrawEnabled"),
            ];
            Report<IOperation>(start, [OperationKind.Invocation, OperationKind.MethodReference, OperationKind.PropertyReference], Rule, (call, analysis) =>
                (call is not IPropertyReferenceOperation || Written(call)) && Among(Target(call), redraws) && !Inside<INameOfOperation>(call) && !Within(analysis.ContainingSymbol, commits));
        });
    }
}

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class NarrowedDuplicateAnalyzer : DiagnosticAnalyzer {

    // --- [RULES]

    private static readonly DiagnosticDescriptor Rule = new(
        "RASM2103",
        "Cast or type test on `GeometryBase` duplicate, acquire a typed copy through `Products.Copied`",
        "`{0}` narrows `{1}` to `{2}`, acquire a copy typed `{2}` through `Products.Copied`",
        Categories.RhinoCommon,
        DiagnosticSeverity.Error,
        isEnabledByDefault: true,
        "`GeometryBase.Duplicate` and `DuplicateShallow` results reach no narrowing cast, type test, or switch.",
        Link("RASM2103"),
        WellKnownDiagnosticTags.CompilationEnd);

    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => [Rule];

    public override void Initialize(AnalysisContext context) {
        context.EnableConcurrentExecution();
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
        context.RegisterCompilationStartAction(static start => {
            WellKnownTypeProvider types = WellKnownTypeProvider.GetOrCreate(start.Compilation);
            if (Members(types.GetOrCreateTypeByMetadataName("Rasm.Rhino.Products"), "Copied").IsEmpty) return;
            ImmutableArray<IMethodSymbol> duplicates = Members(types.GetOrCreateTypeByMetadataName("Rhino.Geometry.GeometryBase"), "Duplicate", "DuplicateShallow");
            static IEnumerable<ITypeSymbol?> Tested(IOperation test) {
                IEnumerable<IPatternOperation> patterns = test switch {
                    IIsPatternOperation patterned => [patterned.Pattern],
                    ISwitchExpressionOperation dispatch => dispatch.Arms.Select(static arm => arm.Pattern),
                    ISwitchOperation dispatch => dispatch.Cases.SelectMany(static section => section.Clauses).OfType<IPatternCaseClauseOperation>().Select(static clause => clause.Pattern),
                    _ => [],
                };
                return test switch {
                    IConversionOperation { IsImplicit: false } conversion => [conversion.Type],
                    IIsTypeOperation typed => [typed.TypeOperand],
                    _ => patterns.SelectMany(static pattern => pattern.DescendantsAndSelf()).OfType<IPatternOperation>().Select(static ITypeSymbol? (pattern) => pattern.NarrowedType),
                };
            }
            static (SyntaxNode Test, ITypeSymbol Narrowed)? Narrowing(IOperation operand, INamedTypeSymbol copied) =>
                Consumer(operand) is { } parent && Tested(parent).FirstOrDefault(type => !Same(type, copied) && Derives(type, copied)) is { } narrowed ? (parent.Syntax, narrowed) : null;
            new SemanticModels(start).Report<IInvocationOperation>([OperationKind.Invocation], (invocation, owner, reading) => {
                if (!Overridden(invocation.TargetMethod).Any(method => Among(method, duplicates))
                    || invocation is not { Type: INamedTypeSymbol copied, Instance.Type: not null }) return [];
                (SyntaxNode? site, ISymbol holder, IEnumerable<IOperation> operands) = Consumer(invocation) switch {
                    IArgumentOperation { Parameter: { } parameter } argument => (argument.Syntax, parameter.ContainingSymbol,
                        Declarations(parameter.ContainingSymbol.OriginalDefinition, reading).SelectMany(body => References(parameter.OriginalDefinition, body, reading))),
                    IVariableInitializerOperation { Parent: IVariableDeclaratorOperation { Symbol: var local } } => (null, owner,
                        References(local, invocation, reading).OrderBy(static reference => reference.Syntax.SpanStart).TakeWhile(static reference => !Written(reference))),
                    _ => (null, owner, [invocation]),
                };
                return operands.Select(operand => Narrowing(operand, copied)).OfType<(SyntaxNode Test, ITypeSymbol Narrowed)>().Select(found => Diagnostic.Create(
                    Rule,
                    (site ?? found.Test).GetLocation(),
                    additionalLocations: site is null ? null : [found.Test.GetLocation()],
                    messageArgs: [Member(holder).Name, invocation.Syntax, found.Narrowed.Name]));
            });
        });
    }
}

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class TextRoundTripAnalyzer : DiagnosticAnalyzer {

    // --- [RULES]

    private static readonly DiagnosticDescriptor Rule = new(
        "RASM2014",
        "Host object round-tripped through text, hold the object",
        "`{0}` rebuilds a host object from text on every decode, hold the object in one `use` scope across every host call",
        Categories.RhinoCommon,
        DiagnosticSeverity.Error,
        isEnabledByDefault: true,
        "RhinoCommon JSON, encoded-string, Base64, and Draco string conversions rebuild native objects per decode, one held object serves every host call.",
        Link("RASM2014"));

    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => [Rule];

    public override void Initialize(AnalysisContext context) {
        context.EnableConcurrentExecution();
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
        context.RegisterCompilationStartAction(static start => {
            WellKnownTypeProvider types = WellKnownTypeProvider.GetOrCreate(start.Compilation);
            ImmutableArray<IMethodSymbol> conversions = [
                .. Members(types.GetOrCreateTypeByMetadataName("Rhino.Runtime.CommonObject"), "ToJSON", "FromJSON", "FromBase64String"),
                .. Members(types.GetOrCreateTypeByMetadataName("Rhino.Geometry.SerializableExtensions"), "ToEncodedString", "FromEncodedString"),
                .. Members(types.GetOrCreateTypeByMetadataName("Rhino.Geometry.MeshingParameters"), "ToEncodedString", "FromEncodedString"),
                .. Members(types.GetOrCreateTypeByMetadataName("Rhino.Geometry.SubDDisplayParameters"), "ToEncodedString", "FromEncodedString"),
                .. Members(types.GetOrCreateTypeByMetadataName("Rhino.FileIO.DracoCompression"), "ToBase64String", "DecompressBase64String"),
            ];
            if (conversions.IsEmpty) return;
            Report<IOperation>(start, [OperationKind.Invocation, OperationKind.MethodReference], (operation, _) =>
                Target(operation) is IMethodSymbol method && Among(method, conversions) && !Inside<INameOfOperation>(operation)
                    ? [Diagnostic.Create(Rule, operation.Syntax.GetLocation(), Label(method))]
                    : []);
        });
    }
}

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class HostDefectAnalyzer : DiagnosticAnalyzer {

    // --- [RULES]

    private static readonly DiagnosticDescriptor Rule = new(
        "RASM2015",
        "RhinoCommon member with a host defect, call the working form",
        "`{0}` {1}, {2}",
        Categories.RhinoCommon,
        DiagnosticSeverity.Error,
        isEnabledByDefault: true,
        "RhinoCommon and Rhino's macOS bindings hold members whose result is wrong on a path the signature hides, each with the form that works.",
        Link("RASM2015"));

    private static readonly ImmutableArray<(string Id, string Fact, string Action)> Defects = [
        ("M:Rhino.Display.RhinoViewport.FromId(System.Guid)", "returns null for detail viewports and another document's viewport for repeated ids",
            "match `doc.Objects.FindId(id) is DetailViewObject`, then `doc.Views.Find(id)`"),
        ("P:Rhino.Display.DisplayPipeline.DpiScale", "reads 1 for a view not on screen", "read `RhinoEtoApp.MainWindowForDocument(view.Document).LogicalPixelSize`"),
        ("M:Rhino.Display.DisplayBitmap.Update(System.Drawing.Bitmap)", "flips images vertically while its constructor does not", "create `new DisplayBitmap(bitmap)` per change and dispose the old one"),
        ("M:Rhino.Render.PostEffects.PostEffectCollection.PostEffectDataFromId(System.Guid)", "answers a missing id with data that throws on first use",
            "find the id among enumerated `PostEffectData` rows"),
        ("P:AppKit.NSWindow.Animator", "throws `RuntimeException` on an Eto `FloatingForm`", "write Eto `Window.Bounds` from an `Animated` sampled on `FrameLinks.Linked` ticks"),
        ("M:AppKit.NSWorkspace.Notifications.ObserveDisplayOptionsDidChange(System.EventHandler{Foundation.NSNotificationEventArgs})",
            "registers on `NSNotificationCenter.DefaultCenter` while AppKit posts to the workspace center",
            "call `NSWorkspace.SharedWorkspace.NotificationCenter.AddObserver(NSWorkspace.DisplayOptionsDidChangeNotification, handler)`"),
        ("M:AppKit.NSWorkspace.Notifications.ObserveDisplayOptionsDidChange(Foundation.NSObject,System.EventHandler{Foundation.NSNotificationEventArgs})",
            "registers on `NSNotificationCenter.DefaultCenter` while AppKit posts to the workspace center",
            "call `NSWorkspace.SharedWorkspace.NotificationCenter.AddObserver(NSWorkspace.DisplayOptionsDidChangeNotification, handler)`"),
    ];

    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => [Rule];

    public override void Initialize(AnalysisContext context) {
        context.EnableConcurrentExecution();
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
        context.RegisterCompilationStartAction(static start => {
            ImmutableArray<(ISymbol Member, string Fact, string Action)> defects = [.. Defects.SelectMany(row => Declared(start.Compilation, row.Id).Select(member => (member, row.Fact, row.Action)))];
            if (defects.IsEmpty) return;
            Report<IOperation>(start, [OperationKind.Invocation, OperationKind.MethodReference, OperationKind.PropertyReference], (operation, _) =>
                Target(operation) is { } member && defects.FirstOrDefault(row => Same(member, row.Member)) is { Member: not null } row && !Inside<INameOfOperation>(operation)
                    ? [Diagnostic.Create(Rule, operation.Syntax.GetLocation(), operation.Syntax, row.Fact, row.Action)]
                    : []);
        });
    }
}
