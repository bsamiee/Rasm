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
            // 9.0.26160+ no longer pre-registers scripting languages at startup. Two-step drive is mandatory and
            // order-sensitive:
            //   1. StartScripting -> PrepareLanguages registers the C# *loader* as an awaiting entry
            //      (HasLoader=true), gated once on Registrar._registeredLanguages. This only registers; it never
            //      invokes the loader, so RhinoCode's resolver finds the entry but OfIdentity(m_registry) misses
            //      and CreateCode throws LanguageMissingException.
            //   2. WaitStatusComplete runs InvokeLoaders (populates the LOADED m_registry) AND drives the
            //      language Status query Continue()->Start()->EndInit() synchronously on this thread, leaving C#
            //      resolvable. Registrar.StartScriptingLanguages is NOT used: on a live host (Application.Instance
            //      != null) it queues WaitStatusComplete onto Application.Instance.Invoke, a deferred UI callback
            //      that has not run when CreateCode executes inside the same UI-thread bridge handler.
            // RhinoWriteQueryResponder is UI-free (RhinoApp.WriteLine only); ReportProgressToConsole pins any
            // internal re-route to the synchronous console responder. startServer:false keeps the script server
            // down.
            global::Rhino.Runtime.Code.RhinoCode.ReportProgressToConsole = true;
            RhinoCodePlatform.Rhino3D.Registrar.StartScripting(startServer: false);
            global::Rhino.Runtime.Code.RhinoCode.Languages.WaitStatusComplete(
                spec: global::Rhino.Runtime.Code.Languages.LanguageSpec.CSharp,
                responder: new RhinoCodePlatform.Rhino3D.Languages.RhinoWriteQueryResponder());
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
            try {
                _ = CSharpLanguage().CreateCode(text: "// readiness probe");
                return null;
            } catch (Exception probe) when (NonFatal(error: probe)) {
                // The drive ran without throwing yet resolution fails. Surface the two facts that discriminate the
                // failure mode without a decompile session: (a) the RhinoCode ALC topology — a duplicated static
                // LanguageRegistry across load contexts would split registration from resolution; (b) the language
                // registry census — entry count, identities, and IsAwaiting per entry. count==0 means PrepareLanguages
                // never registered; entries all IsAwaiting==true means InvokeLoaders never ran (m_registry empty);
                // a present-but-unawaiting C# entry that still fails to resolve points at a resolver/spec change.
                string[] copies = [.. AppDomain.CurrentDomain.GetAssemblies()
                    .Where(predicate: static a => a.GetName().Name is "Rhino.Runtime.Code" or "RhinoCodePlatform.Rhino3D")
                    .Select(selector: static a => $"{a.GetName().Name}@{System.Runtime.Loader.AssemblyLoadContext.GetLoadContext(assembly: a)?.Name ?? "?"}#{a.GetHashCode():x}")];
                string bound =
                    $"bound rhinocode@{System.Runtime.Loader.AssemblyLoadContext.GetLoadContext(assembly: typeof(global::Rhino.Runtime.Code.RhinoCode).Assembly)?.Name ?? "?"}"
                    + $" registrar@{System.Runtime.Loader.AssemblyLoadContext.GetLoadContext(assembly: typeof(RhinoCodePlatform.Rhino3D.Registrar).Assembly)?.Name ?? "?"}";
                return BridgeFault.FromException(
                    category: "rhinocode",
                    error: new InvalidOperationException(message: $"create-probe failed; {RegistryCensus()}; copies=[{string.Join(separator: ";", values: copies)}]; {bound}", innerException: probe));
            }
        } catch (Exception error) when (NonFatal(error: error)) {
            return BridgeFault.FromException(category: "rhinocode", error: error);
        }
    }
    private static (global::Rhino.Runtime.Code.Code? Code, Exception? Error) TryRunScript(BridgeExecuteRequest request, global::Rhino.Runtime.Code.Execution.RunContext context) {
        // BOUNDARY ADAPTER — RhinoCode compile/execute throws; collapse to outcome data.
        try {
            _ = Language.Value;
            // Build through the resolved C# ILanguage, never RhinoCode.CreateCode(text/uri). Plain C# source carries
            // no language discriminator (the only C# text specifier is a shebang; .cs/.csx is an extension specifier),
            // so spec auto-detection from text yields LanguageSpec.Any, which the resolver hard-rejects ->
            // LanguageMissingException. The .csx-uri path resolves via extension only when ScriptPath exists on the
            // HOST filesystem, which is not guaranteed (the client stages scripts client-local). Resolving the loaded
            // language and calling its CreateCode binds the language directly and is path-independent.
            global::Rhino.Runtime.Code.Languages.ILanguage csharp = CSharpLanguage();
            global::Rhino.Runtime.Code.Code code = request.ScriptPath is string scriptPath && ResolveClientPath(path: scriptPath) is string resolvedScript
                ? csharp.CreateCode(uri: new Uri(uriString: resolvedScript))
                : csharp.CreateCode(text: request.Script);
            AddReferences(code: code, request: request);
            _ = global::Rhino.Runtime.Code.RhinoCode.RunScript(code: code, context: context);
            return (Code: code, Error: null);
        } catch (Exception error) when (error is global::Rhino.Runtime.Code.Execution.CompileException or global::Rhino.Runtime.Code.Execution.ExecuteException || NonFatal(error: error)) {
            return (Code: null, Error: error);
        }
    }

    // RhinoCode's C# transformer comments out every #r/#load line before Roslyn parses, so script-embedded #r
    // directives never reach the compilation — references must be added programmatically to code.References, which
    // RoslynCode.TryCompile reads (gated on PathExists) regardless of text-vs-uri construction. Dedupe by simple
    // assembly name: RoslynLoadContext keys references by filename-without-extension (first wins), so duplicate
    // simple names across fingerprint dirs would bind the wrong path. The loaded GH host assemblies are added by
    // identity when a scenario opts into the Grasshopper plugin.
    private static void AddReferences(global::Rhino.Runtime.Code.Code code, BridgeExecuteRequest request) {
        HashSet<string> seen = new(comparer: StringComparer.OrdinalIgnoreCase);
        Array.ForEach(
            array: [.. request.References.Select(selector: ResolveClientPath).OfType<string>().Where(predicate: path => seen.Add(item: Path.GetFileNameWithoutExtension(path: path)))],
            action: path => _ = code.References.Add(item: global::Rhino.Runtime.Code.Execution.CompileReference.FromPath(path: path)));
        if (!request.HostPlugins.Any(predicate: static id => string.Equals(a: id, b: BridgeWire.GrasshopperPluginId, comparisonType: StringComparison.Ordinal))) {
            return;
        }
        Array.ForEach(
            array: [.. AppDomain.CurrentDomain.GetAssemblies().Where(predicate: assembly => IsGrasshopperReference(name: assembly.GetName().Name) && seen.Add(item: assembly.GetName().Name ?? string.Empty))],
            action: assembly => _ = code.References.Add(item: global::Rhino.Runtime.Code.Execution.CompileReference.FromAssembly(assembly: assembly)));
    }

    // The client emits reference and script paths relative to ITS working directory (the repo root); the macOS app
    // bundle resets the Rhino process cwd to "/", so a raw File.Exists silently drops every relative reference. The
    // launching client is Rhino's parent process, so the client cwd survives as the inherited PWD env var even though
    // GetCurrentDirectory does not. Resolve relative paths against PWD; absolute paths and the cwd-coincident case
    // pass through unchanged. Returns null when no anchor yields an existing file.
    private static string? ResolveClientPath(string path) =>
        Path.IsPathRooted(path: path) && File.Exists(path: path) ? path
        : File.Exists(path: Path.GetFullPath(path: path)) ? Path.GetFullPath(path: path)
        : Environment.GetEnvironmentVariable(variable: "PWD") is string clientRoot && File.Exists(path: Path.GetFullPath(path: path, basePath: clientRoot))
            ? Path.GetFullPath(path: path, basePath: clientRoot)
            : null;
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

    // Loaded C# ILanguage, resolved by spec from the post-WaitStatusComplete registry. QueryLatest returns null
    // when the loader registered but never ran; forcing Language.Value (which drives WaitStatusComplete) before any
    // call here keeps that null path unreachable, but the throw makes a regression loud instead of an NRE.
    private static global::Rhino.Runtime.Code.Languages.ILanguage CSharpLanguage() =>
        global::Rhino.Runtime.Code.RhinoCode.Languages.QueryLatest(spec: global::Rhino.Runtime.Code.Languages.LanguageSpec.CSharp)
        ?? throw new InvalidOperationException(message: $"C# language unresolved after drive; {RegistryCensus()}");

    // Language registry census for the create-probe fault — entry identities + awaiting flags discriminate
    // "loader never registered" (count==0) from "loader never invoked" (all awaiting) from a resolver change.
    private static string RegistryCensus() {
        try {
            global::Rhino.Runtime.Code.Languages.LanguageRegistryEntry[] entries = [.. global::Rhino.Runtime.Code.RhinoCode.Languages.QueryRegistered()];
            return $"registry[count={entries.Length}]=[{string.Join(separator: ';', values: entries.Select(selector: static e => $"{e.Id}|awaiting={e.IsAwaiting}"))}]";
        } catch (Exception census) when (NonFatal(error: census)) {
            return $"registry[census-failed:{census.GetType().Name}]";
        }
    }
}
