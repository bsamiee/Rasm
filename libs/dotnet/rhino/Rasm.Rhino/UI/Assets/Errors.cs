namespace Rasm.Rhino.UI.Assets;

// --- [TYPES] ---------------------------------------------------------------------------
internal enum Codes {
    GlyphMissing = 1,
}

// --- [ERRORS] --------------------------------------------------------------------------
public sealed record GlyphMissing(string Resource) : Expected("Resource {Resource} holds no readable image", (int)Codes.GlyphMissing) {
    public static Fin<T> Unless<T>(T? value, string resource) where T : class => value is { } found ? found : new GlyphMissing(resource);
}
