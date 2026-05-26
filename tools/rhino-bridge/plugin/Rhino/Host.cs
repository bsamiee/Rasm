namespace Rasm.RhinoBridge.Rhino;

// --- [COMPOSITION] ----------------------------------------------------------------------
public sealed class Host : PlugIn {
    private static readonly Lock Sync = new();
    private static BridgeServer? server;
    public override PlugInLoadTime LoadTime => PlugInLoadTime.AtStartup;
    protected override LoadReturnCode OnLoad(ref string errorMessage) {
        RhinoApp.Idle += StartOnIdle;
        return LoadReturnCode.Success;
    }
    protected override void OnShutdown() {
        RhinoApp.Idle -= StartOnIdle;
        Stop();
    }
    private static void StartOnIdle(object? sender, EventArgs args) {
        RhinoApp.Idle -= StartOnIdle;
        Fin<BridgeEndpoint> fin = Start();
        if (fin.IsFail) {
            RhinoApp.WriteLine(message: $"[RasmBridge] startup failed: {((Error)fin).Message}");
        }
    }
    internal static Fin<BridgeEndpoint> Start() {
        lock (Sync) {
            return server is { IsRunning: true } active ? active.State() : StartFresh();
        }
    }
    internal static Fin<BridgeEndpoint> Status() {
        lock (Sync) {
            return server?.State() ?? Fin.Fail<BridgeEndpoint>(error: Error.New(message: "No active bridge listener."));
        }
    }
    internal static void Stop() {
        lock (Sync) {
            server?.Dispose();
            server = null;
        }
    }
    private static Fin<BridgeEndpoint> StartFresh() {
        // BOUNDARY ADAPTER — native pipe / endpoint IO can throw and must collapse into Fin.
        try {
            server?.Dispose();
            BridgeServer fresh = BridgeServer.Start();
            server = fresh;
            return fresh.State();
        } catch (Exception error) when (error is IOException or UnauthorizedAccessException or InvalidOperationException) {
            return Fin.Fail<BridgeEndpoint>(error: Error.New(error.Message, error));
        }
    }
    internal static Result Report(string name, Fin<BridgeEndpoint> fin, RhinoDoc? doc) {
        string message = fin.IsSucc
            ? $"[{name}] {PhaseStatus.Ok.Wire}: pipe={((BridgeEndpoint)fin).PipeName}, pid={((BridgeEndpoint)fin).RhinoPid}, doc={(doc is null ? "none" : "active")}"
            : $"[{name}] {PhaseStatus.Failed.Wire}: {((Error)fin).Message}";
        RhinoApp.WriteLine(message: message);
        return fin.IsSucc ? Result.Success : Result.Failure;
    }
}

[System.Runtime.InteropServices.Guid("3A865BB4-0A47-4B4B-96BB-AE8B5E4ACDC1")]
public sealed class RasmBridgeStart : Command {
    public override string EnglishName => nameof(RasmBridgeStart);
    protected override Result RunCommand(RhinoDoc doc, RunMode mode) =>
        Host.Report(name: EnglishName, fin: Host.Start(), doc: doc);
}

[System.Runtime.InteropServices.Guid("834EEDA0-F2BD-462C-B29C-FB75B76EAD77")]
public sealed class RasmBridgeStop : Command {
    public override string EnglishName => nameof(RasmBridgeStop);
    protected override Result RunCommand(RhinoDoc doc, RunMode mode) {
        Host.Stop();
        RhinoApp.WriteLine(message: $"[{EnglishName}] {PhaseStatus.Ok.Wire}");
        return Result.Success;
    }
}

[System.Runtime.InteropServices.Guid("6DEAF1B5-F1B2-463C-9F0F-F3F6B81C3157")]
public sealed class RasmBridgeStatus : Command {
    public override string EnglishName => nameof(RasmBridgeStatus);
    protected override Result RunCommand(RhinoDoc doc, RunMode mode) =>
        Host.Report(name: EnglishName, fin: Host.Status(), doc: doc);
}
