using System.Globalization;
using Rasm.Domain;
using Rasm.RhinoBridge.Protocol;

namespace Rasm.TestKit.Scenarios;

// --- [SERVICES] -----------------------------------------------------------------------------
public static class Probe {
    public static void Require(bool condition, string message) =>
        _ = condition ? true : throw new InvalidOperationException(message: message);
    public static T Expect<T>(Fin<T> result, string label, FactBag? facts = null) {
        ArgumentNullException.ThrowIfNull(argument: result);
        return result.Match(
            Succ: static value => value,
            Fail: error => {
                facts?.Add(key: string.Create(provider: CultureInfo.InvariantCulture, $"{label}.error.category"), value: error.Category());
                facts?.Add(key: string.Create(provider: CultureInfo.InvariantCulture, $"{label}.error"), value: error.Message);
                throw new InvalidOperationException(message: $"{label}: {error.Message}");
            });
    }
    public static T ExpectSome<T>(Option<T> result, string label) =>
        result.Match(
            Some: static value => value,
            None: () => throw new InvalidOperationException(message: $"{label}: missing"));
    public static Unit ExpectRejected<T>(Fin<T> result, string label) {
        ArgumentNullException.ThrowIfNull(argument: result);
        return result.Match(
            Succ: value => throw new InvalidOperationException(message: $"{label}: expected rejection, got {value?.ToString() ?? "<null>"}"),
            Fail: static _ => unit);
    }
    public static Unit ExpectRejectedContains<T>(Fin<T> result, string substring, string label) {
        ArgumentNullException.ThrowIfNull(argument: result);
        ArgumentException.ThrowIfNullOrWhiteSpace(argument: substring);
        return result.Match(
            Succ: value => throw new InvalidOperationException(message: $"{label}: expected rejection, got {value?.ToString() ?? "<null>"}"),
            Fail: error => error.Message.Contains(value: substring, comparisonType: StringComparison.Ordinal)
                ? unit
                : throw new InvalidOperationException(message: $"{label}: rejection missing '{substring}': {error.Message}"));
    }
    public static TCase ExpectCase<T, TCase>(Fin<T> result, string label, Func<T, Option<TCase>> select) {
        ArgumentNullException.ThrowIfNull(argument: select);
        return ExpectSome(
            result: select(arg: Expect(result: result, label: label)),
            label: string.Create(provider: CultureInfo.InvariantCulture, $"{label}: case"));
    }
}

public static class Scenario {
    public static Unit Run(string theme, string capturePath, Action<Op, FactBag> body) {
        ArgumentNullException.ThrowIfNull(argument: theme);
        ArgumentNullException.ThrowIfNull(argument: capturePath);
        ArgumentNullException.ThrowIfNull(argument: body);
        string scenePath = ScenePath(theme: theme, captureBase: capturePath);
        Console.WriteLine(value: string.Create(provider: CultureInfo.InvariantCulture, $"scenario={theme}"));
        Console.WriteLine(value: string.Create(provider: CultureInfo.InvariantCulture, $"capture={scenePath}"));
        Op key = Op.Of(name: theme);
        FactBag bag = new();
        bag.Add(key: "scenario.capturePath", value: scenePath);
        // BOUNDARY ADAPTER — flush partial facts even when the body throws; a failed Probe must not erase the
        // diagnostic trail leading to it. The failing assertion is surfaced separately as the bridge execute fault.
        try {
            body(arg1: key, arg2: bag);
        } finally {
            IReadOnlyDictionary<string, object> snapshot = bag.Snapshot();
            if (snapshot.Count > 0) {
                BridgeMarker.EmitFacts(facts: snapshot);
            }
        }
        return unit;
    }
    // Each theme derives its own capture path from the shared bridge-injected base, so multi-Run files no longer overwrite a single PNG.
    private static string ScenePath(string theme, string captureBase) =>
        string.IsNullOrEmpty(value: captureBase)
            ? string.Create(provider: CultureInfo.InvariantCulture, $"{theme}.png")
            : Path.Combine(
                path1: Path.GetDirectoryName(path: captureBase) ?? Directory.GetCurrentDirectory(),
                path2: string.Create(provider: CultureInfo.InvariantCulture, $"{Path.GetFileNameWithoutExtension(path: captureBase)}.{theme}.png"));
}

// --- [MODELS] -------------------------------------------------------------------------------
public sealed class FactBag {
    private readonly Dictionary<string, object> facts = new(StringComparer.Ordinal);
    public void Add(string key, object value) {
        ArgumentNullException.ThrowIfNull(argument: key);
        ArgumentNullException.ThrowIfNull(argument: value);
        facts[key] = value;
    }
    public void AddIfSome<T>(string key, Option<T> value, Func<T, object> serialize) {
        ArgumentNullException.ThrowIfNull(argument: key);
        ArgumentNullException.ThrowIfNull(argument: serialize);
        _ = value.IfSome(v => Add(key: key, value: serialize(arg: v)));
    }
    internal IReadOnlyDictionary<string, object> Snapshot() => facts;
}
