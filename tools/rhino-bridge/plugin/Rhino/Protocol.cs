using System.Text.Json;
using System.Text.Json.Serialization;

namespace Rasm.RhinoBridge.Rhino;

internal static class BridgeProtocol {
    internal const string Schema = "rasm.rhino-bridge.v1";
    internal const string Hello = "hello";
    internal const string Doctor = "doctor";
    internal const string Check = "check";
    internal const string Ok = "ok";
    internal const string Failed = "failed";
    internal const string Unsupported = "unsupported";
    internal const string Skipped = "skipped";
    internal const string Busy = "busy";
    internal const string Unauthorized = "unauthorized";
    internal static JsonSerializerOptions Json { get; } = new(JsonSerializerDefaults.Web) {
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
        WriteIndented = false,
    };
    internal static string EndpointDirectory => System.IO.Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), ".rasm");
    internal static string EndpointPath => System.IO.Path.Combine(EndpointDirectory, "rhino-bridge.json");
}

public sealed record BridgeEndpoint(
    string Schema,
    string PipeName,
    int RhinoPid,
    string TokenHash,
    DateTimeOffset StartedAt,
    string BridgeAssemblyVersion,
    string RhinoVersion);

public sealed record BridgeRequest(
    string Schema,
    string Command,
    string TokenHash,
    string? JobName,
    int TimeoutMs,
    IReadOnlyList<string> Checks);

public sealed record BridgeReply(
    string Schema,
    string Command,
    string Status,
    string? JobName,
    BridgeEndpoint? Endpoint,
    BridgeDoctor? Doctor,
    IReadOnlyList<BridgeCheckResult>? Checks,
    BridgeFault? Fault) {
    internal static BridgeReply HelloOk(BridgeEndpoint endpoint) =>
        new(BridgeProtocol.Schema, BridgeProtocol.Hello, BridgeProtocol.Ok, null, endpoint, null, null, null);
    internal static BridgeReply DoctorOk(BridgeDoctor doctor) =>
        new(BridgeProtocol.Schema, BridgeProtocol.Doctor, BridgeProtocol.Ok, null, null, doctor, null, null);
    internal static BridgeReply CheckOk(string? jobName, IReadOnlyList<BridgeCheckResult> checks) =>
        new(BridgeProtocol.Schema, BridgeProtocol.Check, BridgeProtocol.Ok, jobName, null, null, checks, null);
    internal static BridgeReply Rejected(string command, string status, BridgeFault fault) =>
        new(BridgeProtocol.Schema, command, status, null, null, null, null, fault);
}

public sealed record BridgeRuntimeStatus(bool Running, BridgeEndpoint? Endpoint, BridgeFault? Fault) {
    internal static BridgeRuntimeStatus Stopped(BridgeFault? fault = null) => new(false, null, fault);
    internal static BridgeRuntimeStatus Started(BridgeEndpoint endpoint) => new(true, endpoint, null);
}

public sealed record BridgeDoctor(
    string RhinoName,
    string RhinoVersion,
    int RhinoPid,
    bool ActiveDocument,
    string? ContextStatus,
    BridgeFault? ContextFault,
    IReadOnlyList<AssemblyReport> Assemblies);

public sealed record AssemblyReport(string Name, string Status, string? Version, string? Location, BridgeFault? Fault) {
    internal static AssemblyReport Loaded(string label, Assembly assembly) {
        ArgumentNullException.ThrowIfNull(assembly);
        AssemblyName name = assembly.GetName();
        return new(label, BridgeProtocol.Ok, name.Version?.ToString(), assembly.Location, null);
    }
    internal static AssemblyReport Missing(string label) =>
        new(label, BridgeProtocol.Skipped, null, null, new BridgeFault("assembly", $"{label} assembly is not loaded in this Rhino session."));
}

public sealed record BridgeCheckResult(string Id, string Status, int DurationMs, int Count, BridgeFault? Fault) {
    internal static BridgeCheckResult Ok(string id, int count = 0) => new(id, BridgeProtocol.Ok, 0, count, null);
    internal static BridgeCheckResult Failed(string id, BridgeFault fault) => new(id, BridgeProtocol.Failed, 0, 0, fault);
    internal static BridgeCheckResult Unsupported(string id) =>
        new(id, BridgeProtocol.Unsupported, 0, 0, new BridgeFault("check", $"Check '{id}' is not in the Rhino bridge catalog."));
    internal static BridgeCheckResult Skipped(string id, BridgeFault fault) => new(id, BridgeProtocol.Skipped, 0, 0, fault);
}

public sealed record BridgeFault(string Category, string Message) {
    internal static BridgeFault Of(Error error) {
        ArgumentNullException.ThrowIfNull(error);
        return new(error.Category(), error.Message);
    }
    internal static BridgeFault Of(string category, Exception error) {
        ArgumentNullException.ThrowIfNull(error);
        return new(category, error.Message);
    }
    internal static BridgeFault MessageOnly(string category, string message) => new(category, message);
}
