using System.Text.Json;
using Rasm.Bridge.Contract;

namespace Rasm.Bridge.Cargo;

// Trivial cargo stub for the U4 hello/load/unload live proof: one scenario, one capability row,
// facts published through the shell-owned delegate. The type name + assembly name are the
// CargoGate activation convention under test.
public sealed class CargoHost : IBridgeCargo {
    private static readonly ScenarioEntry HelloEntry = new(Theme: "probe", Name: "hello", Requires: [], BudgetMs: 1_000);
    private bool disposed;

    public ScenarioEntry[] Discover() => [HelloEntry];

    public CapabilityEntry[] Probe(Action<BridgeEvent> publish) {
        ArgumentNullException.ThrowIfNull(publish);
        publish(Fact(key: "cargo.stub.probe", value: "alive", scenario: null));
        return [new CapabilityEntry(Key: "cargo.stub", Outcome: PhaseStatus.Ok, Receipt: "trivial cargo stub resident")];
    }

    public ScenarioReceipt Run(ScenarioEntry scenario, Action<BridgeEvent> publish) {
        ArgumentNullException.ThrowIfNull(publish);
        long started = Environment.TickCount64;
        publish(Fact(key: "cargo.stub.alc", value: System.Runtime.Loader.AssemblyLoadContext.GetLoadContext(typeof(CargoHost).Assembly)?.Name ?? "unknown", scenario: scenario.Name));
        publish(Fact(key: "cargo.stub.contractIdentity", value: System.Runtime.Loader.AssemblyLoadContext.GetLoadContext(typeof(IBridgeCargo).Assembly)?.Name ?? "unknown", scenario: scenario.Name));
        return new ScenarioReceipt(Scenario: scenario.Name, Status: PhaseStatus.Ok, DurationMs: Environment.TickCount64 - started, Fault: null);
    }

    public void Dispose() => disposed = true;

    private static BridgeEvent.FactCase Fact(string key, string value, string? scenario) =>
        new(Key: key, Value: JsonSerializer.SerializeToElement(value: value, jsonTypeInfo: BridgeJsonContext.Default.String)) {
            Stamp = new EventStamp(SessionId: Guid.Empty, Sequence: 0, AtUnixMs: 0, Scenario: scenario),
        };
}
