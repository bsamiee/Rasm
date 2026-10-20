using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Operations;

namespace Rasm.Analyzers;

// --- [COMPOSITION] ---------------------------------------------------------------------
[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class RhinoAnalyzer : RasmAnalyzer {
    // --- [RULES]
    private const string Host = "RhinoCommon";

    private const string Library = "Rasm.Rhino";

    public static readonly DiagnosticDescriptor Count = Rule(
        "RASM2002",
        Host,
        "Unchecked item count",
        "Item count from `{0}` reaches no `==` or `!=` comparison",
        "Compare the returned count with the input count through `==` or `!=`, and fail on a difference with an error naming the method and both counts.");

    public static readonly DiagnosticDescriptor OffUiThread = Rule(
        "RASM2003",
        Host,
        "RhinoDoc access off the UI thread",
        "`{0}` accesses `RhinoDoc` in a lambda that runs off the UI thread",
        "Access `RhinoDoc` in an `IO` composed with `OnUiThread()`, and pass the lambda values or duplicated geometry read there.");

    public static readonly DiagnosticDescriptor Active = Rule(
        "RASM2004",
        Host,
        "ActiveDoc read with a document in scope",
        "`RhinoDoc.ActiveDoc` read where `{0}` holds a `RhinoDoc`, Mac Rhino can activate another document",
        "Use the held document, and read `RhinoDoc.ActiveDoc` only in an `==` or `!=` comparison that tests which document is active.");

    public static readonly DiagnosticDescriptor Script = Rule(
        "RASM2005",
        Host,
        "RunScript in a command without ScriptRunner style",
        "`RhinoApp.RunScript` in command `{0}` lacks `Style.ScriptRunner`",
        "Mark the command `[CommandStyle(Style.ScriptRunner)]`, under which `RunScript` runs its whole script before returning.");

    public static readonly DiagnosticDescriptor Marshal = Rule(
        "RASM2006",
        Library,
        "Single call marshaled to the UI thread",
        "`{0}` marshals one call to the UI thread",
        "Compose UI-thread calls into one `IO` run through `OnUiThread()`, which `Callbacks` owns.");

    public static readonly DiagnosticDescriptor Settings = Rule(
        "RASM2101",
        Library,
        "PersistentSettings read outside SettingType",
        "`{0}` reads `PersistentSettings` outside `SettingType`",
        "Read through `SettingType.<Kind>.Read(<settings>, <key>)`. It fails with `Errors.ParseError` on stored text direct reads return as absent or default.");

    public static readonly DiagnosticDescriptor Redraw = Rule(
        "RASM2102",
        Library,
        "View redraw outside Commits",
        "`{0}` controls view redraw outside `Commits`",
        "Pass a `RedrawPolicy` to `Commits.Commit` or `Commits.WithinRedraw`. Both apply it after the body on every path and restore prior redraw state.");

    public static readonly DiagnosticDescriptor Narrowed = Rule(
        "RASM2103",
        Library,
        "Cast or type test on a GeometryBase duplicate",
        "Cast or type test in `{0}` narrows `{1}` to `{2}`",
        "Call `Copies.Duplicate(<source>, <retain>)` or `Copies.DuplicateShallow(<source>, <retain>)`. Both return the static type of `<source>` and pass the copy to `<retain>` for disposal.");

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

    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => [Count, OffUiThread, Active, Script, Marshal, Settings, Redraw, Narrowed];

    protected override void Start(CompilationStartAnalysisContext context) {
        Compilation compilation = context.Compilation;
        if (compilation.GetTypeByMetadataName("Rhino.RhinoDoc") is not { } document) return;
        Counts(context);
        Threads(context, document);
        ActiveDocs(context, document);
        Scripts(context);
        Marshals(context);
        Reads(context);
        Redraws(context);
        Duplicates(context);
    }

    // --- [COUNTS]
    private static void Counts(CompilationStartAnalysisContext context) {
        ImmutableArray<IMethodSymbol> counting = [
            .. Counted.SelectMany(row => context.Compilation.GetTypeByMetadataName(row.Owner)?.GetMembers(row.Member) ?? []).OfType<IMethodSymbol>()
                .Where(static method => method.ReturnType.SpecialType is SpecialType.System_Int32
                    && method.Parameters.Any(static parameter => parameter is { RefKind: RefKind.None, Type: var type }
                        && type.AllInterfaces.Append(type).Any(static face => face.OriginalDefinition.SpecialType is SpecialType.System_Collections_Generic_IEnumerable_T))),
        ];
        if (counting.IsEmpty) return;
        context.RegisterOperationAction(
            analysis => {
                IInvocationOperation invocation = (IInvocationOperation)analysis.Operation;
                if (counting.Any(method => Same(invocation.TargetMethod, method))
                    && Consumer(invocation) is not IBinaryOperation { OperatorKind: BinaryOperatorKind.Equals or BinaryOperatorKind.NotEquals })
                    analysis.ReportDiagnostic(Diagnostic.Create(Count, invocation.Syntax.GetLocation(), invocation.TargetMethod.Name));
            },
            OperationKind.Invocation);
    }

    // --- [THREADS]
    private static void Threads(CompilationStartAnalysisContext context, INamedTypeSymbol document) {
        ImmutableArray<INamedTypeSymbol> schedulers = [.. Schedulers.Select(context.Compilation.GetTypeByMetadataName).OfType<INamedTypeSymbol>()];
        bool Schedules(IMethodSymbol method) =>
            schedulers.Any(scheduler => Same(method.ContainingType, scheduler)) && method.Parameters.Any(static parameter => parameter.Type.TypeKind is TypeKind.Delegate);
        context.RegisterOperationAction(
            analysis => {
                (IMethodSymbol? method, ImmutableArray<IArgumentOperation> arguments) = analysis.Operation switch {
                    IInvocationOperation invocation => (invocation.TargetMethod, invocation.Arguments),
                    IObjectCreationOperation creation => (creation.Constructor, creation.Arguments),
                    _ => default,
                };
                if (method is null || !Schedules(method)) return;
                foreach (IOperation reach in arguments
                    .SelectMany(static argument => argument.Value.DescendantsAndSelf().OfType<IAnonymousFunctionOperation>().Take(1))
                    .SelectMany(static callback => callback.Body.Descendants())
                    .Where(reach => reach is not IConversionOperation && Same(reach.Type, document) && !Same(Consumer(reach)?.Type, document)))
                    analysis.ReportDiagnostic(Diagnostic.Create(OffUiThread, reach.Syntax.GetLocation(), reach.Syntax));
            },
            OperationKind.Invocation,
            OperationKind.ObjectCreation);
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
        context.RegisterOperationAction(
            analysis => {
                IPropertyReferenceOperation reference = (IPropertyReferenceOperation)analysis.Operation;
                if (!Same(reference.Property, active) || Inside<INameOfOperation>(reference) || Compared(reference) || reference.SemanticModel is not { } model) return;
                ISymbol owner = analysis.ContainingSymbol;
                int position = reference.Syntax.SpanStart;
                if (model.LookupSymbols(position).FirstOrDefault(held => held switch {
                    IParameterSymbol parameter => Same(parameter.Type, document),
                    ILocalSymbol local => Same(local.Type, document) && local.DeclaringSyntaxReferences.All(declared => declared.Span.End <= position),
                    IFieldSymbol field => Same(field.Type, document) && (field.IsStatic || !owner.IsStatic),
                    IPropertySymbol property => Same(property.Type, document) && (property.IsStatic || !owner.IsStatic)
                        && !Same(property, owner) && !Same(property, (owner as IMethodSymbol)?.AssociatedSymbol),
                    _ => false,
                }) is { } holder)
                    analysis.ReportDiagnostic(Diagnostic.Create(Active, reference.Syntax.GetLocation(), holder.Name));
            },
            OperationKind.PropertyReference);
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
            type is not null && (type.GetAttributes().FirstOrDefault(data => Same(data.AttributeClass, attribute)) is { } style
                ? style.ConstructorArguments is { Length: 1 } arguments && arguments[0].Value is int styles && (styles & runner) is not 0
                : Runs(type.BaseType));
        context.RegisterOperationAction(
            analysis => {
                IInvocationOperation invocation = (IInvocationOperation)analysis.Operation;
                if (scripts.Any(script => Same(invocation.TargetMethod, script)) && analysis.ContainingSymbol.ContainingType is { } owner && Derives(owner, command) && !Runs(owner))
                    analysis.ReportDiagnostic(Diagnostic.Create(Script, invocation.Syntax.GetLocation(), owner.Name));
            },
            OperationKind.Invocation);
    }

    // --- [MARSHALS]
    private static void Marshals(CompilationStartAnalysisContext context) {
        Compilation compilation = context.Compilation;
        if (compilation.GetTypeByMetadataName("Rasm.Rhino.Callbacks") is not { } callbacks) return;
        INamedTypeSymbol? application = compilation.GetTypeByMetadataName("Eto.Forms.Application");
        INamedTypeSymbol? queue = compilation.GetTypeByMetadataName("CoreFoundation.DispatchQueue");
        INamedTypeSymbol? objects = compilation.GetTypeByMetadataName("Foundation.NSObject");
        INamedTypeSymbol? threads = compilation.GetTypeByMetadataName("Foundation.NSThread");
        ImmutableArray<ISymbol> mains = Declared(compilation, "P:CoreFoundation.DispatchQueue.MainQueue", "P:Foundation.NSThread.MainThread");
        static bool Delegated(IMethodSymbol method) => method.Parameters.Any(static parameter => parameter.Type.TypeKind is TypeKind.Delegate);
        bool Main(IOperation? value) => value is IMemberReferenceOperation reference && mains.Any(main => Same(reference.Member, main));
        context.RegisterOperationAction(
            analysis => {
                (IMethodSymbol? method, IOperation? instance, ImmutableArray<IArgumentOperation> arguments) = analysis.Operation switch {
                    IInvocationOperation invocation => (invocation.TargetMethod, invocation.Instance, invocation.Arguments),
                    IMethodReferenceOperation reference => (reference.Method, reference.Instance, ImmutableArray<IArgumentOperation>.Empty),
                    _ => default,
                };
                if (method is not null && !Inside<INameOfOperation>(analysis.Operation) && !Within(analysis.ContainingSymbol, callbacks)
                    && ((Same(method.ContainingType, application) && Delegated(method))
                        || (Same(method.ContainingType, queue) && Delegated(method) && Main(instance))
                        || (objects is not null && Derives(method.ContainingType, objects) && method.Parameters.Any(parameter => Same(parameter.Type, threads)) && arguments.Any(argument => Main(argument.Value)))))
                    analysis.ReportDiagnostic(Diagnostic.Create(Marshal, analysis.Operation.Syntax.GetLocation(), method.Name));
            },
            OperationKind.Invocation,
            OperationKind.MethodReference);
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
        bool Kind(string name, string prefix) => name.StartsWith(prefix, StringComparison.Ordinal) && kinds.Contains(name.Substring(prefix.Length));
        ImmutableArray<IMethodSymbol> reads = [.. methods.Where(method => method.Parameters.Length > 1 && (Kind(method.Name, "Get") || Kind(method.Name, "TryGet")))];
        context.RegisterOperationAction(
            analysis => {
                IInvocationOperation invocation = (IInvocationOperation)analysis.Operation;
                if (reads.Any(read => Same(invocation.TargetMethod, read)) && !Within(analysis.ContainingSymbol, rows) && !Within(analysis.ContainingSymbol, row))
                    analysis.ReportDiagnostic(Diagnostic.Create(Settings, invocation.Syntax.GetLocation(), invocation.TargetMethod.Name));
            },
            OperationKind.Invocation);
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
        context.RegisterOperationAction(
            analysis => {
                IOperation call = analysis.Operation;
                if (call switch {
                    IInvocationOperation invocation => (ISymbol)invocation.TargetMethod,
                    IMethodReferenceOperation reference => reference.Method,
                    IPropertyReferenceOperation { Parent: ISimpleAssignmentOperation assignment } reference when assignment.Target == reference => reference.Property,
                    _ => null,
                } is { } member
                    && redraws.Any(redraw => Same(member, redraw)) && !Inside<INameOfOperation>(call) && !Within(analysis.ContainingSymbol, commits))
                    analysis.ReportDiagnostic(Diagnostic.Create(Redraw, call.Syntax.GetLocation(), call.Syntax));
            },
            OperationKind.Invocation,
            OperationKind.MethodReference,
            OperationKind.PropertyReference);
    }

    // --- [DUPLICATES]
    private static void Duplicates(CompilationStartAnalysisContext context) {
        Compilation compilation = context.Compilation;
        if (compilation.GetTypeByMetadataName("Rasm.Rhino.Modeling.Copies") is not { } copies) return;
        ImmutableArray<ISymbol> duplicates = Declared(compilation, "M:Rhino.Geometry.GeometryBase.Duplicate", "M:Rhino.Geometry.GeometryBase.DuplicateShallow");
        bool Copies(IMethodSymbol? method) => method is not null && (duplicates.Any(duplicate => Same(method, duplicate)) || Copies(method.OverriddenMethod));
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
        static IOperation Root(IOperation operation) => operation.Parent is { } parent ? Root(parent) : operation;
        static bool Written(IOperation reference) =>
            reference.Parent switch {
                IAssignmentOperation assignment => assignment.Target == reference,
                IArgumentOperation { Parameter.RefKind: RefKind.Out or RefKind.Ref } => true,
                IIncrementOrDecrementOperation => true,
                _ => false,
            };
        static string Owner(ISymbol member) => ((member as IMethodSymbol)?.AssociatedSymbol ?? member).Name;
        context.RegisterOperationAction(
            analysis => {
                IInvocationOperation invocation = (IInvocationOperation)analysis.Operation;
                if (!Copies(invocation.TargetMethod) || Within(analysis.ContainingSymbol, copies) || invocation.Type is not INamedTypeSymbol copied) return;
                (SyntaxNode? site, ISymbol holder, IEnumerable<IOperation> operands) = Consumer(invocation) switch {
                    IArgumentOperation { Parameter: { } parameter } argument when invocation.SemanticModel is { } model => (argument.Syntax, parameter.ContainingSymbol, parameter.ContainingSymbol.OriginalDefinition.DeclaringSyntaxReferences
                        .Where(declared => declared.SyntaxTree == model.SyntaxTree)
                        .SelectMany(declared => model.GetOperation(declared.GetSyntax(analysis.CancellationToken), analysis.CancellationToken)?.Descendants() ?? [])
                        .Where(reference => reference is IParameterReferenceOperation held && Same(held.Parameter, parameter))),
                    IVariableInitializerOperation { Parent: IVariableDeclaratorOperation { Symbol: var local } } => (null, analysis.ContainingSymbol, Root(invocation).Descendants()
                        .Where(reference => reference is ILocalReferenceOperation held && Same(held.Local, local))
                        .OrderBy(static reference => reference.Syntax.SpanStart)
                        .TakeWhile(static reference => !Written(reference))),
                    _ => (null, analysis.ContainingSymbol, [invocation]),
                };
                foreach ((SyntaxNode test, ITypeSymbol narrowed) in operands.Select(operand => Narrowing(operand, copied)).OfType<(SyntaxNode Test, ITypeSymbol Narrowed)>())
                    analysis.ReportDiagnostic(Diagnostic.Create(Narrowed, (site ?? test).GetLocation(), additionalLocations: site is null ? null : [test.GetLocation()], messageArgs: [Owner(holder), invocation.Syntax, narrowed.Name]));
            },
            OperationKind.Invocation);
    }
}
