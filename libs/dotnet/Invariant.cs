using System.Numerics;

namespace System.Globalization;

// --- [OPERATIONS] ----------------------------------------------------------------------
internal static class Invariant {
    public static Option<T> Number<T>(ReadOnlySpan<char> text, NumberStyles styles = NumberStyles.Float) where T : INumber<T> =>
        T.TryParse(text, styles, CultureInfo.InvariantCulture, out T? number) && T.IsFinite(number) ? Some(number) : None;
}
