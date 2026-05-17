using System.Diagnostics;
using System.IO.Pipes;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Rasm.RhinoBridge.Client;

internal static class Program {
    private const string Schema = "rasm.rhino-bridge.v1";
    private const string RhinoWipBundleId = "com.mcneel.rhinoceros.9";
    private static readonly JsonSerializerOptions Json = new(JsonSerializerDefaults.Web) {
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
        WriteIndented = true,
    };
    public static async Task<int> Main(string[] args) {
        try {
            return args switch {
                ["doctor"] => await SendAsync(Request(command: "doctor", job: null), timeout: TimeSpan.FromSeconds(15)).ConfigureAwait(false),
                ["check", string jobPath] => await CheckAsync(jobPath: jobPath).ConfigureAwait(false),
                ["launch"] => await LaunchAsync().ConfigureAwait(false),
                _ => Usage(),
            };
        } catch (IOException error) {
            return Fail(error.Message);
        } catch (JsonException error) {
            return Fail(error.Message);
        } catch (InvalidOperationException error) {
            return Fail(error.Message);
        } catch (OperationCanceledException error) {
            return Fail(error.Message);
        }
    }
    private static int Usage() {
        Console.Error.WriteLine("Usage: rhino-bridge-client doctor | check <job.json> | launch");
        return 2;
    }
    private static async Task<int> CheckAsync(string jobPath) {
        string json = await File.ReadAllTextAsync(path: jobPath, cancellationToken: CancellationToken.None).ConfigureAwait(false);
        BridgeJob job = ReadJob(json: json);
        return await SendAsync(Request(command: "check", job: job), timeout: TimeSpan.FromMilliseconds(job.TimeoutMs)).ConfigureAwait(false);
    }
    private static async Task<int> LaunchAsync() {
        Open("-b", RhinoWipBundleId);
        DateTimeOffset deadline = DateTimeOffset.UtcNow.AddSeconds(45.0);
        while (DateTimeOffset.UtcNow < deadline) {
            if (File.Exists(EndpointPath())) {
                Console.WriteLine(await File.ReadAllTextAsync(path: EndpointPath(), cancellationToken: CancellationToken.None).ConfigureAwait(false));
                return 0;
            }
            await Task.Delay(TimeSpan.FromMilliseconds(500.0), CancellationToken.None).ConfigureAwait(false);
        }
        return Fail($"RhinoWIP opened, but no bridge endpoint appeared at {EndpointPath()}.");
    }
    private static void Open(params string[] arguments) {
        ProcessStartInfo start = new() {
            FileName = "open",
        };
        foreach (string argument in arguments) {
            start.ArgumentList.Add(argument);
        }
        using Process? process = Process.Start(start);
        _ = process;
    }
    private static BridgeRequest Request(string command, BridgeJob? job) {
        BridgeEndpoint endpoint = ReadEndpoint();
        return new BridgeRequest(
            Schema: Schema,
            Command: command,
            TokenHash: endpoint.TokenHash,
            JobName: job?.Name,
            TimeoutMs: job?.TimeoutMs ?? 15000,
            Checks: job?.Checks ?? []);
    }
    private static async Task<int> SendAsync(BridgeRequest request, TimeSpan timeout) {
        using CancellationTokenSource cancellation = new(timeout);
        BridgeEndpoint endpoint = ReadEndpoint();
        using NamedPipeClientStream pipe = new(serverName: ".", pipeName: endpoint.PipeName, direction: PipeDirection.InOut, options: PipeOptions.Asynchronous);
        await pipe.ConnectAsync(cancellation.Token).ConfigureAwait(false);
        StreamWriter writer = new(pipe, Encoding.UTF8, bufferSize: 4096, leaveOpen: true);
        using StreamReader reader = new(pipe, Encoding.UTF8, detectEncodingFromByteOrderMarks: false, bufferSize: 4096, leaveOpen: true);
        await using (writer.ConfigureAwait(false)) {
            string payload = JsonSerializer.Serialize(request, Json);
            await writer.WriteLineAsync(payload.AsMemory(), cancellation.Token).ConfigureAwait(false);
            await writer.FlushAsync(cancellation.Token).ConfigureAwait(false);
            string? line = await reader.ReadLineAsync(cancellation.Token).ConfigureAwait(false);
            if (string.IsNullOrWhiteSpace(line)) {
                return Fail("Bridge returned no response.");
            }
            Console.WriteLine(JsonSerializer.Serialize(JsonElement.Parse(line), Json));
        }
        return 0;
    }
    private static BridgeEndpoint ReadEndpoint() {
        string path = EndpointPath();
        string json = File.ReadAllText(path, Encoding.UTF8);
        JsonElement root = JsonElement.Parse(json);
        return new BridgeEndpoint(
            Schema: Text(root: root, name: "schema"),
            PipeName: Text(root: root, name: "pipeName"),
            RhinoPid: root.GetProperty("rhinoPid").GetInt32(),
            TokenHash: Text(root: root, name: "tokenHash"),
            StartedAt: root.GetProperty("startedAt").GetDateTimeOffset(),
            BridgeAssemblyVersion: Text(root: root, name: "bridgeAssemblyVersion"),
            RhinoVersion: Text(root: root, name: "rhinoVersion"));
    }
    private static BridgeJob ReadJob(string json) {
        JsonElement root = JsonElement.Parse(json);
        List<string> checks = [];
        foreach (JsonElement check in root.GetProperty("checks").EnumerateArray()) {
            checks.Add(Text(root: check));
        }
        return new BridgeJob(
            Schema: Text(root: root, name: "schema"),
            Name: Text(root: root, name: "name"),
            TimeoutMs: root.GetProperty("timeoutMs").GetInt32(),
            Checks: checks);
    }
    private static string Text(JsonElement root, string? name = null) =>
        (name is null ? root.GetString() : root.GetProperty(name).GetString()) ?? throw new InvalidOperationException(name is null ? "JSON string value is missing." : $"JSON property '{name}' is missing.");
    private static string EndpointPath() =>
        Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), ".rasm", "rhino-bridge.json");
    private static int Fail(string message) {
        Console.Error.WriteLine(message);
        return 1;
    }
}

internal sealed record BridgeEndpoint(string Schema, string PipeName, int RhinoPid, string TokenHash, DateTimeOffset StartedAt, string BridgeAssemblyVersion, string RhinoVersion);
internal sealed record BridgeRequest(string Schema, string Command, string TokenHash, string? JobName, int TimeoutMs, IReadOnlyList<string> Checks);
internal sealed record BridgeJob(string Schema, string Name, int TimeoutMs, IReadOnlyList<string> Checks);
