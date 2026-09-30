namespace Rasm.Rhino.Objects;

// --- [CONSTANTS] -----------------------------------------------------------------------
public static class Codes {
    public const int WrongLightStyle = 1600;
}

// --- [ERRORS] --------------------------------------------------------------------------
public sealed record WrongLightStyle(LightStyle Style, string Member) : Expected("{Member} does not apply to a {Style} light", Codes.WrongLightStyle) {
    public static Fin<Unit> Unless(bool applies, LightStyle style, string member) => applies ? unit : new WrongLightStyle(style, member);
}
