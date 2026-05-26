using System.Globalization;
using Rasm.Domain;
using Rasm.Vectors;

namespace Rasm.TestKit.Scenarios;

// --- [SERVICES] -----------------------------------------------------------------------------
public static class Probe {
    public static void Require(bool condition, string message) =>
        _ = condition ? true : throw new InvalidOperationException(message: message);
    public static T Expect<T>(Fin<T> result, string label) {
        ArgumentNullException.ThrowIfNull(argument: result);
        return result.Match(
            Succ: static value => value,
            Fail: error => throw new InvalidOperationException(message: $"{label}: {error.Message}"));
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
    public static TOut Project<TOut>(Fin<VectorIntent> intent, Context context, Op key, string label) =>
        Expect(
            result: Expect(result: intent, label: string.Create(provider: CultureInfo.InvariantCulture, $"{label}: intent"))
                .Project<TOut>(context: context, key: key),
            label: string.Create(provider: CultureInfo.InvariantCulture, $"{label}: project"));
}
