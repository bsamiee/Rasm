using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Operations;

namespace Rasm.Policy.Analyzers;

/// <summary>Reports RASM0006 when an executable references an interop facade but never invokes its initialization.</summary>
/// <remarks>
/// Each interop facade owns one Initialize entry point, and Rasm.Interop.RuntimeInitialization.Initialize covers all
/// of them. Executables referencing a facade without any of those calls fail their libraries at first use. A method
/// reference to an entry point counts: the delegate reaches a startup hook the analyzer cannot trace.
/// </remarks>
[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class RuntimeInitializationAnalyzer : DiagnosticAnalyzer {
    private const string InitializeName = "Initialize";
    private const string AggregateTypeName = "Rasm.Interop.RuntimeInitialization";

    private static readonly DiagnosticDescriptor MissingInitialization = new(
        id: "RASM0006",
        title: "Runtime initialization is never invoked",
        messageFormat: "Executable '{0}' references {1} but never invokes {2}.Initialize() or {3}.Initialize(); add one call at the composition root",
        category: "Usage",
        defaultSeverity: DiagnosticSeverity.Error,
        isEnabledByDefault: true,
        description: "Interop facade packages carry mandatory runtime initialization; executables referencing one must invoke it before first library use.",
        customTags: WellKnownDiagnosticTags.CompilationEnd);

    private static readonly ImmutableArray<(string PackageName, string FacadeTypeName)> Facades =
    [
        ("Rasm.Interop.Excel", "Rasm.Interop.Excel.ExcelInterop"),
        ("Rasm.Interop.Gdal", "Rasm.Interop.Gdal.GdalInterop"),
        ("Rasm.Interop.Hdf5", "Rasm.Interop.Hdf5.Hdf5Interop"),
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
            if (startContext.Compilation.Options.OutputKind is not (OutputKind.ConsoleApplication or OutputKind.WindowsApplication)) return;
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
