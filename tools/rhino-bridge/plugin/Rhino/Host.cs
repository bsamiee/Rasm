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
        BridgeHostState state = Start();
        if (!state.IsOk) {
            RhinoApp.WriteLine(message: $"[RasmBridge] startup failed: {state.Fault?.Message}");
        }
    }
    internal static BridgeHostState Start() {
        lock (Sync) {
            return server is { IsRunning: true } active
                ? BridgeHostState.Ok(endpoint: active.State())
                : StartFresh();
        }
    }
    internal static BridgeHostState Status() {
        lock (Sync) {
            return server is { } active
                ? BridgeHostState.Ok(endpoint: active.State())
                : BridgeHostState.Fail(message: "No active bridge listener.");
        }
    }
    internal static void Stop() {
        lock (Sync) {
            server?.Dispose();
            server = null;
        }
    }
    private static BridgeHostState StartFresh() {
        // BOUNDARY ADAPTER — native pipe / endpoint IO can throw during plugin load.
        try {
            server?.Dispose();
            BridgeServer fresh = BridgeServer.Start();
            server = fresh;
            return BridgeHostState.Ok(endpoint: fresh.State());
        } catch (Exception error) when (error is IOException or UnauthorizedAccessException or InvalidOperationException) {
            return BridgeHostState.Fail(message: error.Message);
        }
    }
    internal static Result Report(string name, BridgeHostState state, RhinoDoc? doc) {
        string message = state.Endpoint is { } endpoint
            ? $"[{name}] {PhaseStatus.Ok.Wire}: pipe={endpoint.PipeName}, pid={endpoint.RhinoPid}, doc={(doc is null ? "none" : "active")}"
            : $"[{name}] {PhaseStatus.Failed.Wire}: {state.Fault?.Message}";
        RhinoApp.WriteLine(message: message);
        return state.IsOk ? Result.Success : Result.Failure;
    }
}

internal sealed record BridgeHostState(BridgeEndpoint? Endpoint, BridgeFault? Fault) {
    internal bool IsOk => Endpoint is not null && Fault is null;
    internal static BridgeHostState Ok(BridgeEndpoint endpoint) => new(Endpoint: endpoint, Fault: null);
    internal static BridgeHostState Fail(string message) => new(Endpoint: null, Fault: BridgeFault.MessageOnly(category: "host", message: message));
}

[System.Runtime.InteropServices.Guid("3A865BB4-0A47-4B4B-96BB-AE8B5E4ACDC1")]
public sealed class RasmBridgeStart : Command {
    public override string EnglishName => nameof(RasmBridgeStart);
    protected override Result RunCommand(RhinoDoc doc, RunMode mode) =>
        Host.Report(name: EnglishName, state: Host.Start(), doc: doc);
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
        Host.Report(name: EnglishName, state: Host.Status(), doc: doc);
}
