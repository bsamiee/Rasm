using System.Runtime.CompilerServices;

namespace Rasm.UI.Styling;

// --- [MODELS] --------------------------------------------------------------------------
public sealed record Role([CallerMemberName] string Name = "") {
    // --- [SURFACES]
    public static readonly Role Window = new();
    public static readonly Role Accent = new();
    public static readonly Role Panel = new();
    public static readonly Role Toolbar = new();
    public static readonly Role Header = new();
    public static readonly Role Section = new();
    public static readonly Role Tab = new();
    public static readonly Role TabList = new();
    public static readonly Role StatusBar = new();
    public static readonly Role Card = new();
    public static readonly Role Dialog = new();
    public static readonly Role Sheet = new();
    public static readonly Role Hud = new();
    public static readonly Role Scrim = new();
    public static readonly Role Popover = new();
    public static readonly Role Tooltip = new();
    public static readonly Role Menu = new();
    public static readonly Role Well = new();
    public static readonly Role Stack = new();

    // --- [CONTROLS]
    public static readonly Role Button = new();
    public static readonly Role Field = new();
    public static readonly Role Toggle = new();
    public static readonly Role Switch = new();
    public static readonly Role Segment = new();
    public static readonly Role Chip = new();
    public static readonly Role Indicator = new();
    public static readonly Role Divider = new();
    public static readonly Role Scrollbar = new();
    public static readonly Role Track = new();
    public static readonly Role Span = new();
    public static readonly Role Handle = new();
    public static readonly Role Needle = new();
    public static readonly Role Stop = new();
    public static readonly Role Ring = new();
    public static readonly Role Spinner = new();
    public static readonly Role Selection = new();
    public static readonly Role FocusRing = new();

    // --- [TEXT]
    public static readonly Role Label = new();
    public static readonly Role Title = new();
    public static readonly Role Headline = new();
    public static readonly Role Caption = new();
    public static readonly Role Placeholder = new();
    public static readonly Role Code = new();
    public static readonly Role OverlayCaption = new();
    public static readonly Role Keycap = new();
    public static readonly Role Badge = new();
    public static readonly Role Mark = new();
    public static readonly Role Stat = new();
    public static readonly Role Emphasis = new();
    public static readonly Role Strong = new();
    public static readonly Role Link = new();

    // --- [DATA]
    public static readonly Role ListRow = new();
    public static readonly Role Cell = new();
    public static readonly Role Gutter = new();
    public static readonly Role Line = new();
    public static readonly Role Lane = new();
    public static readonly Role Port = new();
    public static readonly Role Wire = new();
    public static readonly Role Group = new();
    public static readonly Role Grid = new();
    public static readonly Role GridFine = new();
    public static readonly Role Series = new();
    public static readonly Role Trace = new();
    public static readonly Role Tick = new();
    public static readonly Role Crosshair = new();
    public static readonly Role Readout = new();
    public static readonly Role Legend = new();

    // --- [GLYPHS]
    public static readonly Role Icon = new();
    public static readonly Role Chevron = new();
    public static readonly Role Checkmark = new();
    public static readonly Role Dismiss = new();
    public static readonly Role Magnifier = new();
    public static readonly Role Add = new();
    public static readonly Role Subtract = new();
    public static readonly Role Overflow = new();
    public static readonly Role Revert = new();
    public static readonly Role Pin = new();
    public static readonly Role Visibility = new();
    public static readonly Role Lock = new();
    public static readonly Role Status = new();
    public static readonly Role Sorter = new();
    public static readonly Role Grip = new();
}
