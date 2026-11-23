namespace Rasm.UI.Styling;

// --- [MODELS] --------------------------------------------------------------------------

[SmartEnum<string>(SwitchMethods = SwitchMapMethodsGeneration.None, MapMethods = SwitchMapMethodsGeneration.None)]
public sealed partial class StyleRole {

    // --- [SURFACES]

    public static readonly StyleRole Window = new(nameof(Window));
    public static readonly StyleRole Accent = new(nameof(Accent));
    public static readonly StyleRole Panel = new(nameof(Panel));
    public static readonly StyleRole Toolbar = new(nameof(Toolbar));
    public static readonly StyleRole Header = new(nameof(Header));
    public static readonly StyleRole Section = new(nameof(Section));
    public static readonly StyleRole Tab = new(nameof(Tab));
    public static readonly StyleRole TabList = new(nameof(TabList));
    public static readonly StyleRole Card = new(nameof(Card));
    public static readonly StyleRole Dialog = new(nameof(Dialog));
    public static readonly StyleRole Sheet = new(nameof(Sheet));
    public static readonly StyleRole Hud = new(nameof(Hud));
    public static readonly StyleRole Scrim = new(nameof(Scrim));
    public static readonly StyleRole Popover = new(nameof(Popover));
    public static readonly StyleRole Tooltip = new(nameof(Tooltip));
    public static readonly StyleRole Menu = new(nameof(Menu));
    public static readonly StyleRole Well = new(nameof(Well));
    public static readonly StyleRole Stack = new(nameof(Stack));

    // --- [CONTROLS]

    public static readonly StyleRole Button = new(nameof(Button));
    public static readonly StyleRole Field = new(nameof(Field));
    public static readonly StyleRole Toggle = new(nameof(Toggle));
    public static readonly StyleRole Switch = new(nameof(Switch));
    public static readonly StyleRole Segment = new(nameof(Segment));
    public static readonly StyleRole Chip = new(nameof(Chip));
    public static readonly StyleRole Indicator = new(nameof(Indicator));
    public static readonly StyleRole Divider = new(nameof(Divider));
    public static readonly StyleRole Scrollbar = new(nameof(Scrollbar));
    public static readonly StyleRole Track = new(nameof(Track));
    public static readonly StyleRole Span = new(nameof(Span));
    public static readonly StyleRole Handle = new(nameof(Handle));
    public static readonly StyleRole Needle = new(nameof(Needle));
    public static readonly StyleRole Stop = new(nameof(Stop));
    public static readonly StyleRole Ring = new(nameof(Ring));
    public static readonly StyleRole Spinner = new(nameof(Spinner));
    public static readonly StyleRole Selection = new(nameof(Selection));
    public static readonly StyleRole FocusRing = new(nameof(FocusRing));

    // --- [TEXT]

    public static readonly StyleRole Label = new(nameof(Label));
    public static readonly StyleRole Title = new(nameof(Title));
    public static readonly StyleRole Headline = new(nameof(Headline));
    public static readonly StyleRole Caption = new(nameof(Caption));
    public static readonly StyleRole Placeholder = new(nameof(Placeholder));
    public static readonly StyleRole Code = new(nameof(Code));
    public static readonly StyleRole OverlayCaption = new(nameof(OverlayCaption));
    public static readonly StyleRole Keycap = new(nameof(Keycap));
    public static readonly StyleRole Badge = new(nameof(Badge));
    public static readonly StyleRole Mark = new(nameof(Mark));
    public static readonly StyleRole Stat = new(nameof(Stat));
    public static readonly StyleRole Emphasis = new(nameof(Emphasis));
    public static readonly StyleRole Strong = new(nameof(Strong));
    public static readonly StyleRole Link = new(nameof(Link));

    // --- [DATA]

    public static readonly StyleRole ListRow = new(nameof(ListRow));
    public static readonly StyleRole Cell = new(nameof(Cell));
    public static readonly StyleRole Gutter = new(nameof(Gutter));
    public static readonly StyleRole Line = new(nameof(Line));
    public static readonly StyleRole Lane = new(nameof(Lane));
    public static readonly StyleRole Port = new(nameof(Port));
    public static readonly StyleRole Wire = new(nameof(Wire));
    public static readonly StyleRole Group = new(nameof(Group));
    public static readonly StyleRole Grid = new(nameof(Grid));
    public static readonly StyleRole GridFine = new(nameof(GridFine));
    public static readonly StyleRole Series = new(nameof(Series));
    public static readonly StyleRole Trace = new(nameof(Trace));
    public static readonly StyleRole Tick = new(nameof(Tick));
    public static readonly StyleRole Crosshair = new(nameof(Crosshair));
    public static readonly StyleRole Readout = new(nameof(Readout));
    public static readonly StyleRole Legend = new(nameof(Legend));

    // --- [GLYPHS]

    public static readonly StyleRole Icon = new(nameof(Icon));
    public static readonly StyleRole Chevron = new(nameof(Chevron));
    public static readonly StyleRole Checkmark = new(nameof(Checkmark));
    public static readonly StyleRole Dismiss = new(nameof(Dismiss));
    public static readonly StyleRole Magnifier = new(nameof(Magnifier));
    public static readonly StyleRole Add = new(nameof(Add));
    public static readonly StyleRole Subtract = new(nameof(Subtract));
    public static readonly StyleRole Overflow = new(nameof(Overflow));
    public static readonly StyleRole Revert = new(nameof(Revert));
    public static readonly StyleRole Pin = new(nameof(Pin));
    public static readonly StyleRole Visibility = new(nameof(Visibility));
    public static readonly StyleRole Lock = new(nameof(Lock));
    public static readonly StyleRole Status = new(nameof(Status));
    public static readonly StyleRole Sorter = new(nameof(Sorter));
    public static readonly StyleRole Grip = new(nameof(Grip));
}
