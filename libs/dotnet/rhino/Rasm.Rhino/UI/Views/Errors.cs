namespace Rasm.Rhino.UI.Views;

// --- [TYPES] ---------------------------------------------------------------------------
internal enum Codes {
    UnsupportedView = 1,
}

// --- [ERRORS] --------------------------------------------------------------------------
public sealed record UnsupportedView : Expected {
    public UnsupportedView(Type view, Option<Error> cause) : base("{Identity} is a view its host cannot hold", (int)Codes.UnsupportedView, cause) => Identity = view;

    public Type Identity { get; }
}
