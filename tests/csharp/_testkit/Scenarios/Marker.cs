using System.Text.Json;
using Rasm.RhinoBridge.Protocol;

namespace Rasm.TestKit.Scenarios;

// --- [SERVICES] -----------------------------------------------------------------------------
public static class Marker {
    public static void Return(object value) {
        ArgumentNullException.ThrowIfNull(argument: value);
        JsonElement element = JsonSerializer.SerializeToElement(value: value, options: BridgeWire.CompactJson);
        Console.WriteLine(value: new BridgeMarker.Returned(Value: element).Serialize());
    }
}
