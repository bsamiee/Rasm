namespace Rasm.Rhino.Viewport.Pages;

// --- [TYPES] ---------------------------------------------------------------------------
internal enum Codes {
    MissingLayout = 1,
}

// --- [ERRORS] --------------------------------------------------------------------------
public sealed record MissingLayout(LayoutName Name) : Expected("No layout is named {Name}", (int)Codes.MissingLayout);
