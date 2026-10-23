using Rasm.UI.Access;
using SkiaSharp;

namespace Rasm.UI.Inputs;

// --- [TYPES] ---------------------------------------------------------------------------
public enum Response {
    Ignored,
    Release,
    Handled,
    Capture,
}

public enum PointerPhase {
    Down,
    Move,
    Up,
    Leave,
}

[Flags]
public enum PointerButtons {
    None = 0,
    Primary = 1 << 0,
    Secondary = 1 << 1,
    Middle = 1 << 2,
}

[Flags]
public enum Modifiers {
    None = 0,
    Fine = 1 << 0,
    Snap = 1 << 1,
    Alternate = 1 << 2,
}

public enum Gesture {
    Magnify,
    Rotate,
}

public enum CursorKind {
    Default,
    Pointer,
    Text,
    Crosshair,
    Move,
    Grab,
    Grabbing,
    ResizeHorizontal,
    ResizeVertical,
}

public enum KeyCode {
    KeyA, KeyB, KeyC, KeyD, KeyE, KeyF, KeyG, KeyH, KeyI, KeyJ, KeyK, KeyL, KeyM,
    KeyN, KeyO, KeyP, KeyQ, KeyR, KeyS, KeyT, KeyU, KeyV, KeyW, KeyX, KeyY, KeyZ,
    Digit0, Digit1, Digit2, Digit3, Digit4, Digit5, Digit6, Digit7, Digit8, Digit9,
    Enter, Escape, Tab, Space, Backspace, Delete,
    ArrowLeft, ArrowRight, ArrowUp, ArrowDown, Home, End, PageUp, PageDown,
    Minus, Equal, BracketLeft, BracketRight, Backslash, Semicolon, Quote, Comma, Period, Slash, Backquote,
}

// --- [MODELS] --------------------------------------------------------------------------
[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record Input {
    public sealed record Pointer(PointerPhase Phase, SKPoint At, PointerButtons Buttons, Modifiers Modifiers, float Pressure, TimeSpan Time) : Input;
    public sealed record Click(SKPoint At, PointerButtons Button, Modifiers Modifiers, int Count) : Input;
    public sealed record Scroll(SKPoint At, SKPoint Delta, Modifiers Modifiers) : Input;
    public sealed record Gestured(SKPoint At, Gesture Gesture, float Amount) : Input;
    public sealed record Key(KeyCode Code, Modifiers Modifiers, bool Down) : Input;
    public sealed record Composition(string Text, bool Final) : Input;
    public sealed record Focus(bool Gained) : Input;
    public sealed record Act(AccessAction Action, SKPoint At) : Input;
}
