namespace Rasm.RhinoBridge.Rhino;

public sealed class Host : PlugIn {
    public override PlugInLoadTime LoadTime => PlugInLoadTime.AtStartup;
    protected override LoadReturnCode OnLoad(ref string errorMessage) {
        RhinoApp.Idle += StartOnIdle;
        return LoadReturnCode.Success;
    }
    protected override void OnShutdown() => BridgeRuntime.Stop();
    private static void StartOnIdle(object? sender, EventArgs args) {
        RhinoApp.Idle -= StartOnIdle;
        _ = BridgeRuntime.Start();
    }
}

public sealed class RasmBridgeStart : Command {
    public override string EnglishName => nameof(RasmBridgeStart);
    protected override Result RunCommand(RhinoDoc doc, RunMode mode) {
        BridgeRuntimeStatus status = BridgeRuntime.Start();
        WriteStatus(command: EnglishName, status: status);
        return status.Running ? Result.Success : Result.Failure;
    }
    private static void WriteStatus(string command, BridgeRuntimeStatus status) =>
        RhinoApp.WriteLine(status.Endpoint is BridgeEndpoint endpoint
            ? $"[{command}] {BridgeProtocol.Ok}: pipe={endpoint.PipeName}, pid={endpoint.RhinoPid}"
            : $"[{command}] {BridgeProtocol.Failed}: {status.Fault?.Message ?? "Bridge did not start."}");
}

public sealed class RasmBridgeStop : Command {
    public override string EnglishName => nameof(RasmBridgeStop);
    protected override Result RunCommand(RhinoDoc doc, RunMode mode) {
        BridgeRuntime.Stop();
        RhinoApp.WriteLine("[RasmBridgeStop] ok");
        return Result.Success;
    }
}

public sealed class RasmBridgeStatus : Command {
    public override string EnglishName => nameof(RasmBridgeStatus);
    protected override Result RunCommand(RhinoDoc doc, RunMode mode) {
        BridgeRuntimeStatus status = BridgeRuntime.Status();
        RhinoApp.WriteLine(status.Endpoint is BridgeEndpoint endpoint
            ? $"[RasmBridgeStatus] ok: pipe={endpoint.PipeName}, rhino={endpoint.RhinoVersion}, doc={(RhinoDoc.ActiveDoc is null ? "none" : "active")}"
            : $"[RasmBridgeStatus] stopped: {status.Fault?.Message ?? "No active bridge listener."}");
        return Result.Success;
    }
}
