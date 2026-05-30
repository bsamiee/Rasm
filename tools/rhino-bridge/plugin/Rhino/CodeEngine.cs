namespace Rasm.RhinoBridge.Rhino;

// --- [MODELS] ---------------------------------------------------------------------------
internal sealed record CodeOutcome(
    PhaseStatus Status,
    BridgeFault? Fault,
    int DurationMs,
    string Stdout,
    string Stderr,
    BridgeReturnValue? ReturnValue,
    BridgeRhinoCodeReport RhinoCode,
    IReadOnlyList<BridgeDiagnostic> Diagnostics);

// --- [SERVICES] -------------------------------------------------------------------------
// Sole RhinoCodePlatform/Rhino.Runtime.Code boundary — WIP signature drift breaks here; Readiness() surfaces mismatch early.
internal static class CodeEngine {
    private static readonly Lazy<bool> Language = new(
        valueFactory: static () => {
            RhinoCodePlatform.Rhino3D.Registrar.StartScriptingLanguages(spec: global::Rhino.Runtime.Code.Languages.LanguageSpec.CSharp, startServer: false);
            return true;
        },
        mode: LazyThreadSafetyMode.PublicationOnly);
    internal static CodeOutcome Run(BridgeExecuteRequest request) {
        ArgumentNullException.ThrowIfNull(argument: request);
        Stopwatch timer = Stopwatch.StartNew();
        global::Rhino.Runtime.Code.Execution.RunContextStream stdout = new();
        global::Rhino.Runtime.Code.Execution.RunContextStream stderr = new();
        using global::Rhino.Runtime.Code.Execution.RunContext context = new(defaultOutputStream: false, defaultErrorStream: false) {
            CachePolicy = global::Rhino.Runtime.Code.Execution.CachePolicy.NeverCache,
            PreferBasePathResolution = false,
            // RhinoCode pins csx to C# 10 (CSharpVersion); only compiler/resolver options apply — scenarios stay C# 10-clean.
            Options = { ["csharp.resolver.isolate"] = true },
            OutputStream = stdout,
            ErrorStream = stderr,
            ExclusiveStreams = false,
            ResetStreamsPolicy = global::Rhino.Runtime.Code.Execution.ResetStreamPolicy.ResetToPreviousStream,
        };
        (global::Rhino.Runtime.Code.Code? code, Exception? error) = TryRunScript(request: request, context: context);
        timer.Stop();
        string stdoutText = stdout.GetContents();
        string stderrText = stderr.GetContents();
        BridgeReturnValue? returnValue = BridgeMarker.Scan(stdout: stdoutText).OfType<BridgeMarker.Returned>().LastOrDefault() is { } returned
            ? new(Value: returned.Value, Source: BridgeWire.OutputStdout)
            : null;
        BridgeDiagnostic[] diagnostics = Diagnose(code: code, error: error);
        return new CodeOutcome(
            Status: error is null ? PhaseStatus.Ok : PhaseStatus.Failed,
            Fault: error is null ? null : BridgeFault.FromException(category: diagnostics.Length > 0 ? "diagnostics" : "execute", error: error),
            DurationMs: (int)timer.ElapsedMilliseconds,
            Stdout: stdoutText,
            Stderr: stderrText,
            ReturnValue: returnValue,
            RhinoCode: new(
                CachePolicy: context.CachePolicy.ToString(),
                ResolverIsolated: context.Options.Get(key: "csharp.resolver.isolate", defaultValue: false),
                PreferBasePathResolution: context.PreferBasePathResolution),
            Diagnostics: diagnostics);
    }
    internal static BridgeFault? Readiness() {
        // BOUNDARY ADAPTER — probe one-time language registration without propagating the failure.
        try {
            _ = Language.Value;
            return null;
        } catch (Exception error) when (NonFatal(error: error)) {
            return BridgeFault.FromException(category: "rhinocode", error: error);
        }
    }
    private static (global::Rhino.Runtime.Code.Code? Code, Exception? Error) TryRunScript(BridgeExecuteRequest request, global::Rhino.Runtime.Code.Execution.RunContext context) {
        // BOUNDARY ADAPTER — RhinoCode compile/execute throws; collapse to outcome data.
        try {
            _ = Language.Value;
            global::Rhino.Runtime.Code.Code code = request.ScriptPath is string scriptPath && File.Exists(path: scriptPath)
                ? global::Rhino.Runtime.Code.RhinoCode.CreateCode(uri: new Uri(uriString: scriptPath))
                : global::Rhino.Runtime.Code.RhinoCode.CreateCode(text: request.Script);
            AddHostReferences(code: code, pluginIds: request.HostPlugins);
            _ = global::Rhino.Runtime.Code.RhinoCode.RunScript(code: code, context: context);
            return (Code: code, Error: null);
        } catch (Exception error) when (error is global::Rhino.Runtime.Code.Execution.CompileException or global::Rhino.Runtime.Code.Execution.ExecuteException || NonFatal(error: error)) {
            return (Code: null, Error: error);
        }
    }

    // GH2: add already-loaded assemblies as CompileReference.FromAssembly — nameable in scenarios without `#r` collectible reload.
    private static void AddHostReferences(global::Rhino.Runtime.Code.Code code, IReadOnlyList<string> pluginIds) {
        if (!pluginIds.Any(predicate: static id => string.Equals(a: id, b: BridgeWire.GrasshopperPluginId, comparisonType: StringComparison.Ordinal))) {
            return;
        }
        Array.ForEach(
            array: [.. AppDomain.CurrentDomain.GetAssemblies().Where(predicate: static assembly => IsGrasshopperReference(name: assembly.GetName().Name))],
            action: assembly => _ = code.References.Add(item: global::Rhino.Runtime.Code.Execution.CompileReference.FromAssembly(assembly: assembly)));
    }

    private static bool IsGrasshopperReference(string? name) =>
        string.Equals(a: name, b: "Grasshopper2", comparisonType: StringComparison.OrdinalIgnoreCase)
        || string.Equals(a: name, b: "GrasshopperIO", comparisonType: StringComparison.OrdinalIgnoreCase);
    private static BridgeDiagnostic[] Diagnose(global::Rhino.Runtime.Code.Code? code, Exception? error) =>
        CompileFailure(error: error) switch {
            global::Rhino.Runtime.Code.Execution.CompileException compile => [.. compile.Diagnosis.Select(ToDiagnostic)],
            _ => code?.Diagnostics.Select(ToDiagnostic).ToArray() ?? [],
        };
    private static global::Rhino.Runtime.Code.Execution.CompileException? CompileFailure(Exception? error) =>
        error switch {
            null => null,
            global::Rhino.Runtime.Code.Execution.CompileException compile => compile,
            global::Rhino.Runtime.Code.Execution.ExecuteException execute when execute.TryGetCompileException(out global::Rhino.Runtime.Code.Execution.CompileException compile) => compile,
            { InnerException: Exception inner } => CompileFailure(error: inner),
            _ => null,
        };
    private static BridgeDiagnostic ToDiagnostic(global::Rhino.Runtime.Code.Diagnostics.Diagnostic diagnostic) =>
        new(
            Severity: diagnostic.Severity.ToString(),
            Message: diagnostic.Message,
            Source: diagnostic.Reference.Uri?.ToString(),
            Code: diagnostic.HasId ? diagnostic.Id : null,
            File: diagnostic.Reference.Uri?.LocalPath,
            Line: diagnostic.Reference.Position.LineNumber,
            Column: diagnostic.Reference.Position.ColumnNumber,
            Category: "rhinocode");
    private static bool NonFatal(Exception error) =>
        error is not OutOfMemoryException and not StackOverflowException and not AccessViolationException;
}
