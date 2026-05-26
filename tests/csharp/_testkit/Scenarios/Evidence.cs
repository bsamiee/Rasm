using System.Globalization;
using System.Text.Json;
using Rasm.RhinoBridge.Protocol;

namespace Rasm.TestKit.Scenarios;

// --- [SERVICES] -----------------------------------------------------------------------------
public static class Evidence {
    private static readonly JsonSerializerOptions Json = new(defaults: JsonSerializerDefaults.Web) {
        WriteIndented = false,
    };
    public static void Emit(string key, object value) =>
        Console.WriteLine(value: string.Create(provider: CultureInfo.InvariantCulture, $"{key}={Format(value: value)}"));
    public static void EmitFacts(IReadOnlyDictionary<string, object> facts) {
        ArgumentNullException.ThrowIfNull(argument: facts);
        string serialized = JsonSerializer.Serialize(value: facts, options: Json);
        Console.WriteLine(value: string.Create(provider: CultureInfo.InvariantCulture, $"facts={serialized}"));
        Console.WriteLine(value: new BridgeMarker.Evidence(Key: "facts", Value: serialized).Serialize());
    }
    public static void EmitFacts(object facts) {
        ArgumentNullException.ThrowIfNull(argument: facts);
        string serialized = JsonSerializer.Serialize(value: facts, options: Json);
        Console.WriteLine(value: string.Create(provider: CultureInfo.InvariantCulture, $"facts={serialized}"));
        Console.WriteLine(value: new BridgeMarker.Evidence(Key: "facts", Value: serialized).Serialize());
    }
    public static void EmitScenarioHeader(string scenario, string capturePath) {
        Console.WriteLine(value: string.Create(provider: CultureInfo.InvariantCulture, $"scenario={scenario}"));
        Console.WriteLine(value: string.Create(provider: CultureInfo.InvariantCulture, $"capture={capturePath}"));
    }
    private static string Format(object value) =>
        value switch {
            string text => text,
            bool flag => flag ? "true" : "false",
            IFormattable formattable => formattable.ToString(format: null, formatProvider: CultureInfo.InvariantCulture),
            _ => value.ToString() ?? "<null>",
        };
}
