using System.Globalization;
using System.Numerics;
using Rasm.UI.Plotting;
using Rasm.UI.Styling;

namespace Rasm.UI.Automation;

// --- [MODELS] --------------------------------------------------------------------------

[SmartEnum]
public sealed partial class Check {
    public static readonly Check Off = new(None);
    public static readonly Check On = new(Some(Condition.Selected));
    public static readonly Check Mixed = new(Some(Condition.Mixed));

    internal Option<Condition> Conditioned { get; }

    internal static Check Of(Option<bool> value) => value.Match(Some: static on => on ? On : Off, None: static () => Mixed);
}

[SmartEnum(SwitchMethods = SwitchMapMethodsGeneration.None)]
public sealed partial class AccessRole {
    public static readonly AccessRole Text = new(namedByContents: true);
    public static readonly AccessRole Image = new(namedByContents: false);
    public static readonly AccessRole Heading = new(namedByContents: true);
    public static readonly AccessRole Link = new(namedByContents: true);
    public static readonly AccessRole Button = new(namedByContents: true);
    public static readonly AccessRole MenuButton = new(namedByContents: true);
    public static readonly AccessRole PopUpButton = new(namedByContents: true);
    public static readonly AccessRole Disclosure = new(namedByContents: true);
    public static readonly AccessRole CheckBox = new(namedByContents: true);
    public static readonly AccessRole Switch = new(namedByContents: true);
    public static readonly AccessRole Radio = new(namedByContents: true);
    public static readonly AccessRole Tab = new(namedByContents: true);
    public static readonly AccessRole ListItem = new(namedByContents: true);
    public static readonly AccessRole TreeItem = new(namedByContents: true);
    public static readonly AccessRole GridCell = new(namedByContents: true);
    public static readonly AccessRole MenuItem = new(namedByContents: true);
    public static readonly AccessRole Slider = new(namedByContents: false);
    public static readonly AccessRole Handle = new(namedByContents: false);
    public static readonly AccessRole Separator = new(namedByContents: false);
    public static readonly AccessRole TextBox = new(namedByContents: false);
    public static readonly AccessRole ProgressBar = new(namedByContents: false);
    public static readonly AccessRole Group = new(namedByContents: false);
    public static readonly AccessRole ScrollArea = new(namedByContents: false);
    public static readonly AccessRole Status = new(namedByContents: false);
    public static readonly AccessRole Dialog = new(namedByContents: false);
    public static readonly AccessRole Popover = new(namedByContents: false);
    public static readonly AccessRole Tooltip = new(namedByContents: true);
    public static readonly AccessRole Toolbar = new(composite: true, namedByContents: false, None, picks: false);
    public static readonly AccessRole RadioGroup = new(Radio, picks: true);
    public static readonly AccessRole TabList = new(Tab, picks: false);
    public static readonly AccessRole List = new(ListItem, picks: false);
    public static readonly AccessRole Tree = new(TreeItem, picks: false);
    public static readonly AccessRole Row = new(GridCell, picks: false);
    public static readonly AccessRole Grid = new(Row, picks: false);
    public static readonly AccessRole Menu = new(MenuItem, picks: true);

    private AccessRole(bool namedByContents) : this(composite: false, namedByContents, None, picks: false) { }

    private AccessRole(AccessRole owns, bool picks) : this(composite: true, namedByContents: false, Some(owns), picks) { }

    internal bool Composite { get; }
    internal bool NamedByContents { get; }
    internal Option<AccessRole> Owns { get; }
    internal bool Picks { get; }
}

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record AccessValue {
    public abstract string Spoken(CultureInfo culture);

    public sealed record Text(string Value) : AccessValue {
        public override string Spoken(CultureInfo culture) => Value;
    }

    public sealed record Ranged(double Value, double Min, double Max) : AccessValue {
        public Ranged(Axis axis, double value) : this(value, axis.Domain.Low, axis.Domain.High) => Axis = axis;

        public Option<Axis> Axis { get; private init; }

        public override string Spoken(CultureInfo culture) =>
            Axis.Match(Some: axis => axis.Spoken((float)Value, culture), None: () => string.Create(culture, $"{(Value - Min) / (Max - Min):P0}"));
    }

    public sealed record Point(PlotPlane Plane, Vector2 At) : AccessValue {
        public override string Spoken(CultureInfo culture) => Plane.Spoken(At, culture);
    }
}

public sealed record AxisDescription(Option<string> Title, Axis Axis) {
    public Seq<float> Gridlines { get; init; }
}

public sealed record SeriesDescription(string Label, Figure Figure);

public sealed record ChartDescription(string Title, Seq<AxisDescription> Axes, Seq<SeriesDescription> Series) {
    public Option<string> Summary { get; init; }
}

public sealed record Accessible : Monoid<Accessible> {
    public static Accessible Empty { get; } = new();

    public Option<AccessRole> Role { get; init; }
    public Option<string> Label { get; init; }
    public Option<string> Help { get; init; }
    public Option<string> Shortcut { get; init; }
    public Option<AccessValue> Value { get; init; }
    public Option<bool> Expanded { get; init; }
    public Option<PhysicalAxis> Orientation { get; init; }
    public Option<bool> Interrupts { get; init; }
    public Option<ChartDescription> Chart { get; init; }

    public Accessible Combine(Accessible rhs) =>
        new() {
            Role = Role | rhs.Role,
            Label = Label | rhs.Label,
            Help = Help | rhs.Help,
            Shortcut = Shortcut | rhs.Shortcut,
            Value = Value | rhs.Value,
            Expanded = Expanded | rhs.Expanded,
            Orientation = Orientation | rhs.Orientation,
            Interrupts = Interrupts | rhs.Interrupts,
            Chart = Chart | rhs.Chart,
        };

    internal Option<string> Spoken(CultureInfo culture) => AutomationElement.Joined(Seq(Label, Value.Map(value => value.Spoken(culture))).Somes());
}
