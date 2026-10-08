using Rhino.ApplicationSettings;

namespace Rasm.Rhino.Persistence.Settings;

// --- [TYPES] ---------------------------------------------------------------------------
internal enum Codes {
    ReadOnlyKey = 1,
    Unloaded,
    ThemedPaintColor,
    UnknownThemeKey,
    SizeOutOfRange,
    OddPadding,
}

// --- [ERRORS] --------------------------------------------------------------------------
public sealed record ReadOnlyKey(string Key) : Expected("Key {Key} is read-only", (int)Codes.ReadOnlyKey) {
    public static Fin<Unit> Unless(bool writable, string key) => writable ? unit : new ReadOnlyKey(key);
}

public sealed record Unloaded(Guid Id) : Expected("Plug-in {Id} is not loaded", (int)Codes.Unloaded) {
    public static Fin<Unit> Unless(bool loaded, Guid id) => loaded ? unit : new Unloaded(id);
}

public sealed record ThemedPaintColor(PaintColor Slot) : Expected("A theme hook draws paint slot {Slot}", (int)Codes.ThemedPaintColor) {
    public static Fin<Unit> Unless(bool unhooked, PaintColor slot) => unhooked ? unit : new ThemedPaintColor(slot);
}

public sealed record UnknownThemeKey(string Key) : Expected("No theme zone enumerates {Key}", (int)Codes.UnknownThemeKey) {
    public static Fin<Unit> Unless(bool known, string key) => known ? unit : new UnknownThemeKey(key);
}

public sealed record SizeOutOfRange(string Member, int Size, int Minimum, int Maximum) : Expected("{Member} takes {Size} outside {Minimum} to {Maximum}", (int)Codes.SizeOutOfRange) {
    public static Fin<Unit> Unless(int size, int minimum, int maximum, string member) =>
        size >= minimum && size <= maximum ? unit : new SizeOutOfRange(member, size, minimum, maximum);
}

public sealed record OddPadding(int Row, int Glyph) : Expected("Row {Row} and glyph {Glyph} leave no whole padding", (int)Codes.OddPadding) {
    public static Fin<Unit> Unless(bool even, int row, int glyph) => even ? unit : new OddPadding(row, glyph);
}
