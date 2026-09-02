using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Operations;

namespace Rasm.Policy.Analyzers;

/// <summary>Reports RASM0006 when an executable or plugin host references an interop facade but never invokes its initialization</summary>
/// <remarks>
/// Rasm.Interop.RuntimeInitialization.Initialize covers the Initialize entry point of every facade, and a host without any of those calls fails its libraries at first use.
/// Method references count because the delegate reaches a startup hook the analyzer cannot trace, and libraries opt in as hosts through the RasmPluginHost build property
/// </remarks>
[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class RuntimeInitializationAnalyzer : DiagnosticAnalyzer {
    private const string InitializeName = "Initialize";
    private const string AggregateTypeName = "Rasm.Interop.RuntimeInitialization";

    private static readonly DiagnosticDescriptor MissingInitialization = new(
        id: "RASM0006",
        title: "Runtime initialization is never invoked",
        messageFormat: "Host '{0}' references {1} but its composition root never invokes {2}.Initialize() or {3}.Initialize()",
        category: "Usage",
        defaultSeverity: DiagnosticSeverity.Error,
        isEnabledByDefault: true,
        description: "Interop facade packages require runtime initialization that referencing executables and plugin hosts invoke before first library use.",
        customTags: WellKnownDiagnosticTags.CompilationEnd);

    private static readonly ImmutableArray<(string PackageName, string FacadeTypeName)> Facades =
    [
        ("Rasm.Interop.Excel", "Rasm.Interop.Excel.ExcelInterop"),
        ("Rasm.Interop.Gdal", "Rasm.Interop.Gdal.GdalInterop"),
        ("Rasm.Interop.Hdf5", "Rasm.Interop.Hdf5.Hdf5Interop"),
        ("Rasm.Interop.OpenCv", "Rasm.Interop.OpenCv.OpenCvInterop"),
        ("Rasm.Interop.Pdf", "Rasm.Interop.Pdf.PdfInterop"),
    ];

    /// <inheritdoc />
    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => [MissingInitialization];

    /// <inheritdoc />
    public override void Initialize(AnalysisContext context) {
        if (context is null) throw new ArgumentNullException(nameof(context));
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.Analyze);
        context.EnableConcurrentExecution();
        context.RegisterCompilationStartAction(static startContext => {
            bool pluginHost = startContext.Options.AnalyzerConfigOptionsProvider.GlobalOptions.TryGetValue("build_property.RasmPluginHost", out string? optIn)
                && string.Equals(optIn, "true", StringComparison.OrdinalIgnoreCase);
            if (startContext.Compilation.Options.OutputKind is not (OutputKind.ConsoleApplication or OutputKind.WindowsApplication) && !pluginHost) return;
            INamedTypeSymbol? aggregate = startContext.Compilation.GetTypeByMetadataName(AggregateTypeName);
            ImmutableArray<(string PackageName, INamedTypeSymbol Facade)> referenced =
            [
                .. Facades
                    .Select(facade => (facade.PackageName, Facade: startContext.Compilation.GetTypeByMetadataName(facade.FacadeTypeName)))
                    .Where(static facade => facade.Facade is not null)
                    .Select(static facade => (facade.PackageName, facade.Facade!)),
            ];
            if (referenced.IsEmpty) return;
            int[] invoked = new int[referenced.Length];
            startContext.RegisterOperationAction(operationContext => {
                IMethodSymbol? target = operationContext.Operation switch {
                    IInvocationOperation invocation => invocation.TargetMethod,
                    IMethodReferenceOperation reference => reference.Method,
                    _ => null,
                };
                if (target is not { Name: InitializeName, IsStatic: true }) return;
                bool coversAll = SymbolEqualityComparer.Default.Equals(target.ContainingType, aggregate);
                for (int index = 0; index < referenced.Length; index++) {
                    if (coversAll || SymbolEqualityComparer.Default.Equals(target.ContainingType, referenced[index].Facade))
                        _ = Interlocked.Exchange(ref invoked[index], 1);
                }
            }, OperationKind.Invocation, OperationKind.MethodReference);
            startContext.RegisterCompilationEndAction(endContext => {
                for (int index = 0; index < referenced.Length; index++) {
                    if (invoked[index] == 0)
                        endContext.ReportDiagnostic(Diagnostic.Create(
                            MissingInitialization,
                            Location.None,
                            endContext.Compilation.AssemblyName,
                            referenced[index].PackageName,
                            referenced[index].Facade.ToDisplayString(),
                            AggregateTypeName));
                }
            });
        });
    }
}
