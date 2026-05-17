namespace Rasm.RhinoBridge.Rhino;

public sealed class Host : PlugIn {
    public override PlugInLoadTime LoadTime => PlugInLoadTime.AtStartup;
    protected override LoadReturnCode OnLoad(ref string errorMessage) {
        RhinoApp.Idle += StartOnIdle;
        return LoadReturnCode.Success;
    }
    protected override void OnShutdown() {
        RhinoApp.Idle -= StartOnIdle;
        BridgeRuntime.Stop();
    }
    private static void StartOnIdle(object? sender, EventArgs args) {
        RhinoApp.Idle -= StartOnIdle;
        BridgeRuntimeState status = BridgeRuntime.Start();
        if (status.Endpoint is null) {
            RhinoApp.WriteLine($"[RasmBridge] startup failed: {status.Fault?.Message ?? "unknown bridge startup failure"}");
        }
    }
}

[System.Runtime.InteropServices.Guid("3A865BB4-0A47-4B4B-96BB-AE8B5E4ACDC1")]
public sealed class RasmBridgeStart : Command {
    public override string EnglishName => nameof(RasmBridgeStart);
    protected override Result RunCommand(RhinoDoc doc, RunMode mode) {
        BridgeRuntimeState status = BridgeRuntime.Start();
        RhinoApp.WriteLine(status.Endpoint is BridgeEndpoint endpoint
            ? $"[{EnglishName}] {BridgeWire.Ok}: pipe={endpoint.PipeName}, pid={endpoint.RhinoPid}"
            : $"[{EnglishName}] {BridgeWire.Failed}: {status.Fault?.Message ?? "Bridge did not start."}");
        return status.Endpoint is not null ? Result.Success : Result.Failure;
    }
}

[System.Runtime.InteropServices.Guid("834EEDA0-F2BD-462C-B29C-FB75B76EAD77")]
public sealed class RasmBridgeStop : Command {
    public override string EnglishName => nameof(RasmBridgeStop);
    protected override Result RunCommand(RhinoDoc doc, RunMode mode) {
        BridgeRuntime.Stop();
        RhinoApp.WriteLine("[RasmBridgeStop] ok");
        return Result.Success;
    }
}

[System.Runtime.InteropServices.Guid("6DEAF1B5-F1B2-463C-9F0F-F3F6B81C3157")]
public sealed class RasmBridgeStatus : Command {
    public override string EnglishName => nameof(RasmBridgeStatus);
    protected override Result RunCommand(RhinoDoc doc, RunMode mode) {
        BridgeRuntimeState status = BridgeRuntime.Status();
        RhinoApp.WriteLine(status.Endpoint is BridgeEndpoint endpoint
            ? $"[RasmBridgeStatus] ok: pipe={endpoint.PipeName}, rhino={endpoint.RhinoVersion}, doc={(doc is null ? "none" : "active")}"
            : $"[RasmBridgeStatus] stopped: {status.Fault?.Message ?? "No active bridge listener."}");
        return Result.Success;
    }
}
