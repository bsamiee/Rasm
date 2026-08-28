# [RASM_GRASSHOPPER_OBJECTS]

`NativeObject` is the native document-object catalog: the interactive `Grasshopper2.Parameters.Special` family, the `Shout`/`Listen`/`Relay` routing pins, the public `Cluster`/`Chain` composite family, and the `Grasshopper2.SpecialObjects.ScribbleObject` annotation land as rows of one `NativeKind` catalog. `PersistedValue` closes the values the public host surface can read or assign, and each catalog row carries every per-type correspondence as a column — parameterless mint, `GrasshopperIO.IReader` rehydration, read, seeded mint, and write — so a row is unconstructible until it answers all five and no type-switch roster stands beside the catalog.

One polymorphic owner mints, rehydrates, persists, reads, assigns, pulses a button, steps a list selection, reconciles timer targets, and resolves cluster maps on the result. GH2's loop driver, looping iterations, repeat discriminants, bitmap sampler kernel, and incomplete chain ordering and validation kernels stay outside the package contract.

## [01]-[INDEX]

- [02]-[CONTROL_VOCABULARY]: object families, timer modes, and per-object capability vocabularies
- [03]-[VALUE_AND_CATALOG]: `PersistedValue` closes the state shapes and `NativeKind` catalogs the rows with all five construction columns
- [04]-[OBJECT_OPERATIONS]: one owner covers mint, archive round trip, value, verbs, timer targets, and cluster maps

## [02]-[CONTROL_VOCABULARY]

- Owner: the generated vocabularies close the object-family, timer-mode, and PER-OBJECT FLAG discriminants. Every multi-flag surface is a CAPABILITY vocabulary — seven `ICapability` families (`PanelFacet`, `MappingFlag`, `SamplerFlag`, `RecorderFlag`, `RoutingFlag`, `ListenerFlag`, `ClusterFlag`) carry what twenty-plus parallel booleans spelled — and the timer's `(Running, Manual)` pair is `TimerMode`, four named rows whose host columns are the pair, so an impossible or unnamed combination cannot ride two independent flags.
- Cases: `ObjectFamily` partitions the catalog; each flag family's rows mirror the host members they project onto.
- Law: a flag family's law is `Open` — every membership combination is a lawful host state — and the projection to host booleans happens at the ROW's write arm alone, so membership algebra (diff two values, count engaged facets) arrives free from the capability owner.
- Growth: a new host discriminant value is one row on the owning vocabulary; a new per-object boolean is one capability row, never a payload field.
- Boundary: `LoopingAction`, `LoopRepeats`, `Loop`, and `LoopingIteration` are assembly-internal; the public `Cluster.LoopSolution` switch is the only loop state this boundary may assign.

```csharp
// --- [IMPORTS] -------------------------------------------------------------------------
using Rasm.Domain;

namespace Rasm.Grasshopper.Components;

// --- [TYPES] ---------------------------------------------------------------------------

[SmartEnum]
public sealed partial class ObjectFamily {
    public static readonly ObjectFamily ValueInput = new();
    public static readonly ObjectFamily Editor = new();
    public static readonly ObjectFamily Sampler = new();
    public static readonly ObjectFamily Picker = new();
    public static readonly ObjectFamily List = new();
    public static readonly ObjectFamily Data = new();
    public static readonly ObjectFamily Utility = new();
    public static readonly ObjectFamily Routing = new();
    public static readonly ObjectFamily Composite = new();
    public static readonly ObjectFamily Annotation = new();
}

[SmartEnum<int>]
public sealed partial class TimerMode {
    public static readonly TimerMode Idle = new(key: 0, running: false, manual: false);
    public static readonly TimerMode Auto = new(key: 1, running: true, manual: false);
    public static readonly TimerMode Armed = new(key: 2, running: false, manual: true);
    public static readonly TimerMode Manual = new(key: 3, running: true, manual: true);
    internal bool RunningHost { get; }
    internal bool ManualHost { get; }
    public static TimerMode Of(bool running, bool manual) =>
        (running, manual) switch { (false, false) => Idle, (true, false) => Auto, (false, true) => Armed, _ => Manual };
}

[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class PanelFacet : ICapability<PanelFacet> {
    public static readonly PanelFacet Columns = new(key: "columns");
    public static readonly PanelFacet Paths = new(key: "paths");
    public static readonly PanelFacet Indices = new(key: "indices");
    public static readonly PanelFacet Types = new(key: "types");
    public static readonly PanelFacet Items = new(key: "items");
    public static readonly PanelFacet Metas = new(key: "metas");
    public static CapabilityLaw<PanelFacet> Law => CapabilityLaw<PanelFacet>.Open;
}

[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class MappingFlag : ICapability<MappingFlag> {
    public static readonly MappingFlag OmitUnaffected = new(key: "omit-unaffected");
    public static readonly MappingFlag WarnPaths = new(key: "warn-paths");
    public static readonly MappingFlag WarnSites = new(key: "warn-sites");
    public static CapabilityLaw<MappingFlag> Law => CapabilityLaw<MappingFlag>.Open;
}

[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class SamplerFlag : ICapability<SamplerFlag> {
    public static readonly SamplerFlag Normalised = new(key: "normalised");
    public static readonly SamplerFlag Luminance = new(key: "luminance");
    public static readonly SamplerFlag DrawSamples = new(key: "draw-samples");
    public static CapabilityLaw<SamplerFlag> Law => CapabilityLaw<SamplerFlag>.Open;
}

[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class RecorderFlag : ICapability<RecorderFlag> {
    public static readonly RecorderFlag Paused = new(key: "paused");
    public static readonly RecorderFlag MergeTrees = new(key: "merge-trees");
    public static CapabilityLaw<RecorderFlag> Law => CapabilityLaw<RecorderFlag>.Open;
}

[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class RoutingFlag : ICapability<RoutingFlag> {
    public static readonly RoutingFlag ClusterOutput = new(key: "cluster-output");
    public static readonly RoutingFlag Stream = new(key: "stream");
    public static readonly RoutingFlag Backup = new(key: "backup");
    public static CapabilityLaw<RoutingFlag> Law => CapabilityLaw<RoutingFlag>.Open;
}

[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class ListenerFlag : ICapability<ListenerFlag> {
    public static readonly ListenerFlag ClusterInput = new(key: "cluster-input");
    public static readonly ListenerFlag ClusterIndex = new(key: "cluster-index");
    public static CapabilityLaw<ListenerFlag> Law => CapabilityLaw<ListenerFlag>.Open;
}

[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class ClusterFlag : ICapability<ClusterFlag> {
    public static readonly ClusterFlag LoopSolution = new(key: "loop-solution");
    public static readonly ClusterFlag RelayMessages = new(key: "relay-messages");
    public static CapabilityLaw<ClusterFlag> Law => CapabilityLaw<ClusterFlag>.Open;
}
```

## [03]-[VALUE_AND_CATALOG]

- Owner: `PersistedValue` is the one union over empty construction and publicly readable or writable object state, one case per distinct state shape the catalog publishes — flag surfaces ride the `[02]` capability sets, the timer posture rides `TimerMode`, and every COLOUR payload is the kernel `PerceptualColor`, quantized to the host colour type at the row boundary alone; `NativeKind` carries family, exact host type, and FIVE construction columns — parameterless `Create`, `IReader` `Rehydrate`, `Read`, seeded `Mint`, and `Write`.
- Owner: `ObjectMap` is the Mapperly mapper for the property-writable rows: one existing-target `Update(value, host)` per pure-property pairing and one `Read(host)` per pure projection, renames as `[MapProperty]` rows and set/colour projections as `[MapPropertyFromSource]` reads, so the hand assignment blocks survive ONLY where the host demands them — an ordering-sensitive write (the value member last: `Value`, `TextInput`, `Sampler`, `PathMapper`, `Scribble`, `MaterialEditor`), a method-call write (`DataPanel.ChangeDisplay`, `ColourSwatch.SetColour`), or a reconciling verb (`Timer`, pickers, `ValueList`) — each named at its arm.
- Entry: `NativeKind.ForHost(Type)` walks the candidate's own ancestry against the `Items`-derived frozen index through the kernel `Admit.Probe` out-parameter lift (S1-24 — a `TryGetValue` reads as an `Option`, never a bool-and-out pair), so a host subclass resolves onto the nearest catalogued row and only a genuinely foreign type misses.
- Law: read and write are ROW COLUMNS, never a type-switch roster beside the catalog — a row is minted through one of three generic factories that close the host type and the payload case together, so the compiler demands every arm at the declaration and a catalogued row missing one cannot be constructed. Three factories ARE the seeding discriminant: `Of` for a row the parameterless constructor mints and the write column completes, `Seeded` for a row whose constructor takes the value AND whose remaining columns stay settable, `Sealed` for a row whose constructor is the whole write and whose `Write` therefore refuses.
- Law: the archive round trip is two columns and one interface call, never a case family — every catalogued type declares a `public T(IReader)` constructor, so `Rehydrate` sits beside `Create`, while `Grasshopper2.Doc.IDocumentObject : GrasshopperIO.IStorable` publishes `Store(IWriter)` on the interface itself, so the write leg is one polymorphic call with no per-row data. `Grasshopper2.SpecialObjects.ScribbleObject` overrides no `Store` and persists through `CustomValues` under the base body; the interface call still reaches it, which is exactly why that leg carries no column.
- Law: a seeding constructor's first argument is the host's own default user name, read once per row off a lazily minted instance — the row KEY is wire identity in the `[a-z0-9-]` grammar and never crosses into a canvas-visible name, so the two spaces cannot drift into each other.
- Law: colour crosses ONCE per direction — a row's read arm lifts the host colour into `PerceptualColor` and its write arm quantizes back; interior consumers hold the kernel owner and an Eto or host colour past the row boundary is the deleted form.
- Packages: every host type column is a verified `Grasshopper2.Parameters.Special`, `Grasshopper2.Components.Standard`, or `Grasshopper2.SpecialObjects` type; Riok.Mapperly generates the `ObjectMap` mapper; `Rasm.Numerics` carries `PerceptualColor`; canvas control state stays each object's own `CreateAttributes` projection.
- Growth: a new interactive object is one catalog row naming its factory arm and, where its state shape is new, one `PersistedValue` case with its `ObjectMap` pair or its named hand arm.
- Boundary: `Empty` selects the row's parameterless constructor; every other value enters through the row's own `Mint`. Four payload columns are read-and-seed only, because the host publishes no public setter: `Slider.Value` and `Number.Value` (`InternalNumber` is derived from the internal slider and picker), `Complexes.Values` and `Moment.Value` (`internal set`), and `Ramp.Value` (`Gradient` projects `GripGradientInteraction.ModifiedGradient`, so `Interaction` is the writable source). `Histogram` carries the integral `BucketCount` and never its `BucketCountText` spelling, which restates the same fact through the negative `Buckets*` sentinel constants.

```csharp
// --- [IMPORTS] -------------------------------------------------------------------------
using System.Collections.Frozen;
using Rasm.Domain;
using Rasm.Numerics;
using Riok.Mapperly.Abstractions;

namespace Rasm.Grasshopper.Components;

// --- [MODELS] --------------------------------------------------------------------------

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record PersistedValue {
    private PersistedValue() { }

    public sealed record Empty : PersistedValue;
    public sealed record Flag(bool Value) : PersistedValue;
    public sealed record Slider(
        Grasshopper2.UI.UiNumber Value, Grasshopper2.UI.Slider.GripShape Grip,
        PerceptualColor Colour, string Format) : PersistedValue;
    public sealed record Number(decimal Value, PerceptualColor Colour, bool Snap) : PersistedValue;
    public sealed record Protraction(
        Grasshopper2.Types.Numeric.Angle Value, Grasshopper2.Parameters.Special.ProtractorMode Mode) : PersistedValue;
    public sealed record Moment(DateTime Value) : PersistedValue;
    public sealed record Complexes(Seq<System.Numerics.Complex> Values) : PersistedValue;
    public sealed record Constant(Grasshopper2.Maths.Constant Value) : PersistedValue;
    public sealed record MetaKey(Grasshopper2.Data.Meta.MetaName Value) : PersistedValue;
    public sealed record Momentary(
        Grasshopper2.Parameters.Special.ButtonAction Action,
        Option<Grasshopper2.Data.ITree> Up, Option<Grasshopper2.Data.ITree> Down,
        PerceptualColor UpColour, PerceptualColor DownColour,
        string UpText, string DownText) : PersistedValue;
    public sealed record Text(
        string Value, bool PerLine, Grasshopper2.Parameters.Special.TextInputEscaping Escaping) : PersistedValue;
    public sealed record Parsed(string Source, Grasshopper2.Parsing.Notation Notations) : PersistedValue;
    public sealed record Annotation(
        string Value, int Angle, Grasshopper2.SpecialObjects.ScribbleFont Font,
        Eto.Drawing.FontStyle Style, Eto.Drawing.OpenColor.Family Colour,
        Eto.Forms.TextAlignment Align) : PersistedValue;
    public sealed record Swatch(PerceptualColor Value, bool Apply) : PersistedValue;
    public sealed record Ramp(
        Grasshopper2.Types.Colour.Gradient Value, double Parameter0, double Parameter1,
        Grasshopper2.Types.Colour.GripGradientInteraction Interaction) : PersistedValue;
    public sealed record Material(
        Rhino.Display.DisplayMaterial Value, Eto.Forms.RotationF Fore,
        Eto.Forms.RotationF Back, bool Identical) : PersistedValue;
    public sealed record Histogram(
        Grasshopper2.Parameters.Special.HistogramStyle Style,
        Grasshopper2.Parameters.Special.HistogramPalette Palette,
        int BucketCount, Rhino.Geometry.Interval BucketRange) : PersistedValue;
    public sealed record Sampler(
        string ImageUri, CapabilitySet<SamplerFlag> Flags,
        Grasshopper2.Parameters.Special.ImageSamplerObject.SamplingLimit LimitBehaviour) : PersistedValue;
    public sealed record Selection(
        Option<Seq<string>> UserNames, bool MultiSelect, float Scroll0, float Scroll1) : PersistedValue;
    public sealed record Listing(
        Grasshopper2.Parameters.Special.ValueListMode Mode, Seq<int> Selected) : PersistedValue;
    public sealed record Mapping(string Notation, CapabilitySet<MappingFlag> Flags) : PersistedValue;
    public sealed record PanelDisplay(CapabilitySet<PanelFacet> Shown, float VerticalOffset) : PersistedValue;
    public sealed record TreeDisplay(
        Grasshopper2.Parameters.Special.TreeCanvasDisplay CanvasDisplay,
        Grasshopper2.Parameters.Special.TreeViewportDisplay ViewportDisplay,
        Grasshopper2.Types.Colour.Gradient DisplayGradient) : PersistedValue;
    public sealed record Recording(CapabilitySet<RecorderFlag> Flags, int FrameLimit) : PersistedValue;
    public sealed record Targets(Seq<Guid> Ids, TimeSpan Delay, TimerMode Mode) : PersistedValue;
    public sealed record Routing(CapabilitySet<RoutingFlag> Flags, string StreamPath) : PersistedValue;
    public sealed record Listener(
        Grasshopper2.Parameters.Special.Listen.Dependency Index,
        GrasshopperIO.AbsRelPaths A, GrasshopperIO.AbsRelPaths B,
        GrasshopperIO.AbsRelPaths C, GrasshopperIO.AbsRelPaths D,
        CapabilitySet<ListenerFlag> Flags) : PersistedValue;
    public sealed record Grouping(CapabilitySet<ClusterFlag> Flags) : PersistedValue;
}

// --- [OPERATIONS] ----------------------------------------------------------------------
[Mapper]
internal static partial class ObjectMap {
    [MapProperty(nameof(PersistedValue.Slider.Grip), nameof(Grasshopper2.Parameters.Special.NumberSliderObject.GripDisplay))]
    [MapPropertyFromSource(nameof(Grasshopper2.Parameters.Special.NumberSliderObject.GripColour), Use = nameof(SliderColour))]
    [MapProperty(nameof(PersistedValue.Slider.Format), nameof(Grasshopper2.Parameters.Special.NumberSliderObject.GripFormat))]
    internal static partial void Update(PersistedValue.Slider value, Grasshopper2.Parameters.Special.NumberSliderObject host);

    [MapPropertyFromSource(nameof(Grasshopper2.Parameters.Special.NumberPickerObject.GripColour), Use = nameof(PickerColour))]
    [MapProperty(nameof(PersistedValue.Number.Snap), nameof(Grasshopper2.Parameters.Special.NumberPickerObject.SnapToTicks))]
    internal static partial void Update(PersistedValue.Number value, Grasshopper2.Parameters.Special.NumberPickerObject host);

    [MapProperty(nameof(PersistedValue.Ramp.Parameter0), nameof(Grasshopper2.Parameters.Special.GradientEditorObject.Parameter0))]
    [MapProperty(nameof(PersistedValue.Ramp.Parameter1), nameof(Grasshopper2.Parameters.Special.GradientEditorObject.Parameter1))]
    [MapProperty(nameof(PersistedValue.Ramp.Interaction), nameof(Grasshopper2.Parameters.Special.GradientEditorObject.Interaction))]
    internal static partial void Update(PersistedValue.Ramp value, Grasshopper2.Parameters.Special.GradientEditorObject host);

    [MapProperty(nameof(PersistedValue.Histogram.Style), nameof(Grasshopper2.Parameters.Special.HistogramObject.Style))]
    [MapProperty(nameof(PersistedValue.Histogram.Palette), nameof(Grasshopper2.Parameters.Special.HistogramObject.Palette))]
    [MapProperty(nameof(PersistedValue.Histogram.BucketCount), nameof(Grasshopper2.Parameters.Special.HistogramObject.BucketCount))]
    [MapProperty(nameof(PersistedValue.Histogram.BucketRange), nameof(Grasshopper2.Parameters.Special.HistogramObject.BucketRange))]
    internal static partial void Update(PersistedValue.Histogram value, Grasshopper2.Parameters.Special.HistogramObject host);

    [MapProperty(nameof(PersistedValue.TreeDisplay.CanvasDisplay), nameof(Grasshopper2.Parameters.Special.TreeViewerObject.CanvasDisplay))]
    [MapProperty(nameof(PersistedValue.TreeDisplay.ViewportDisplay), nameof(Grasshopper2.Parameters.Special.TreeViewerObject.ViewportDisplay))]
    [MapProperty(nameof(PersistedValue.TreeDisplay.DisplayGradient), nameof(Grasshopper2.Parameters.Special.TreeViewerObject.DisplayGradient))]
    internal static partial void Update(PersistedValue.TreeDisplay value, Grasshopper2.Parameters.Special.TreeViewerObject host);

    [MapProperty(nameof(PersistedValue.Protraction.Mode), nameof(Grasshopper2.Parameters.Special.ProtractorObject.Mode))]
    [MapProperty(nameof(PersistedValue.Protraction.Value), nameof(Grasshopper2.Parameters.Special.ProtractorObject.Angle))]
    internal static partial void Update(PersistedValue.Protraction value, Grasshopper2.Parameters.Special.ProtractorObject host);

    private static Eto.Drawing.Color SliderColour(PersistedValue.Slider value) => Quantized(value.Colour);
    private static Eto.Drawing.Color PickerColour(PersistedValue.Number value) => Quantized(value.Colour);

    internal static Eto.Drawing.Color Quantized(PerceptualColor colour);
    internal static PerceptualColor Perceptual(Eto.Drawing.Color colour);
    internal static Grasshopper2.Types.Colour.Colour Tinted(PerceptualColor colour);
    internal static PerceptualColor Lifted(Grasshopper2.Types.Colour.Colour colour);
}

// --- [SERVICES] ------------------------------------------------------------------------

[SmartEnum<string>]
public sealed partial class NativeKind {
    public static readonly NativeKind NumberSlider = Seeded<Grasshopper2.Parameters.Special.NumberSliderObject, PersistedValue.Slider>(
        "number-slider", ObjectFamily.ValueInput, static reader => new(reader),
        static host => new PersistedValue.Slider(host.InternalNumber, host.GripDisplay, ObjectMap.Perceptual(host.GripColour), host.GripFormat),
        static (name, held) => new Grasshopper2.Parameters.Special.NumberSliderObject(name, held.Value),
        static (host, held, key) => Try.lift(() => ObjectMap.Update(held, host)).Run().Bind(static inner => inner));
    public static readonly NativeKind NumberPicker = Seeded<Grasshopper2.Parameters.Special.NumberPickerObject, PersistedValue.Number>(
        "number-picker", ObjectFamily.ValueInput, static reader => new(reader),
        static host => new PersistedValue.Number(host.InternalNumber, ObjectMap.Perceptual(host.GripColour), host.SnapToTicks),
        static (name, held) => new Grasshopper2.Parameters.Special.NumberPickerObject(name, (double)held.Value),
        static (host, held, key) => Try.lift(() => ObjectMap.Update(held, host)).Run().Bind(static inner => inner));
    public static readonly NativeKind Toggle = Of<Grasshopper2.Parameters.Special.ToggleObject, PersistedValue.Flag>(
        "toggle", ObjectFamily.ValueInput, static reader => new(reader),
        static host => new PersistedValue.Flag(host.ToggleState),
        static (host, held, key) => Try.lift(() => { host.ToggleState = held.Value; }).Run());
    public static readonly NativeKind Button = Of<Grasshopper2.Parameters.Special.ButtonObject, PersistedValue.Momentary>(
        "button", ObjectFamily.ValueInput, static reader => new(reader),
        static host => new PersistedValue.Momentary(
            host.Action, Optional(host.UpTree), Optional(host.DownTree),
            ObjectMap.Lifted(host.UpColour), ObjectMap.Lifted(host.DownColour), host.UpText, host.DownText),
        static (host, held, key) => Try.lift(() => {
            host.Action = held.Action;
            held.Up.Iter(tree => host.UpTree = tree);
            held.Down.Iter(tree => host.DownTree = tree);
            host.UpColour = ObjectMap.Tinted(held.UpColour);
            host.DownColour = ObjectMap.Tinted(held.DownColour);
            host.UpText = held.UpText;
            host.DownText = held.DownText;
        }).Run());
    public static readonly NativeKind Value = Of<Grasshopper2.Parameters.Special.ValueObject, PersistedValue.Parsed>(
        "value", ObjectFamily.ValueInput, static reader => new(reader),
        static host => new PersistedValue.Parsed(host.Text, host.Notations),
        static (host, held, key) => Try.lift(() => {
            host.Notations = held.Notations;
            host.AssignTextAndValue(held.Source);
        }).Run());
    public static readonly NativeKind TextInput = Of<Grasshopper2.Parameters.Special.TextInputObject, PersistedValue.Text>(
        "text-input", ObjectFamily.ValueInput, static reader => new(reader),
        static host => new PersistedValue.Text(host.Contents, host.OneEntryPerLine, host.Escaping),
        static (host, held, key) => Try.lift(() => {
            host.Escaping = held.Escaping;
            host.Contents = held.Value;
        }).Run());
    public static readonly NativeKind ColourSwatch = Of<Grasshopper2.Parameters.Special.ColourSwatchObject, PersistedValue.Swatch>(
        "colour-swatch", ObjectFamily.ValueInput, static reader => new(reader),
        static host => new PersistedValue.Swatch(ObjectMap.Lifted(host.Colour), Apply: false),
        static (host, held, key) => Try.lift(() => host.SetColour(ObjectMap.Tinted(held.Value), held.Apply)).Run().Bind(static inner => inner));
    public static readonly NativeKind GradientEditor = Of<Grasshopper2.Parameters.Special.GradientEditorObject, PersistedValue.Ramp>(
        "gradient-editor", ObjectFamily.Editor, static reader => new(reader),
        static host => new PersistedValue.Ramp(host.Gradient, host.Parameter0, host.Parameter1, host.Interaction),
        static (host, held, key) => Try.lift(() => ObjectMap.Update(held, host)).Run().Bind(static inner => inner));
    public static readonly NativeKind FunctionEditor = Inert<Grasshopper2.Parameters.Special.FunctionEditorObject>(
        "function-editor", ObjectFamily.Editor, static reader => new(reader));
    public static readonly NativeKind MaterialEditor = Of<Grasshopper2.Parameters.Special.MaterialEditorObject, PersistedValue.Material>(
        "material-editor", ObjectFamily.Editor, static reader => new(reader),
        static host => new PersistedValue.Material(host.Material, host.ForeRotation, host.BackRotation, host.IdenticalForeAndBack),
        static (host, held, key) => Try.lift(() => {
            host.ForeRotation = held.Fore;
            host.BackRotation = held.Back;
            host.IdenticalForeAndBack = held.Identical;
            host.Material = held.Value;
        }).Run());
    public static readonly NativeKind Histogram = Of<Grasshopper2.Parameters.Special.HistogramObject, PersistedValue.Histogram>(
        "histogram", ObjectFamily.Editor, static reader => new(reader),
        static host => new PersistedValue.Histogram(host.Style, host.Palette, host.BucketCount, host.BucketRange),
        static (host, held, key) => Try.lift(() => ObjectMap.Update(held, host)).Run().Bind(static inner => inner));
    public static readonly NativeKind QuickGraph = Inert<Grasshopper2.Parameters.Special.QuickGraphObject>(
        "quick-graph", ObjectFamily.Editor, static reader => new(reader));
    public static readonly NativeKind Protractor = Of<Grasshopper2.Parameters.Special.ProtractorObject, PersistedValue.Protraction>(
        "protractor", ObjectFamily.Editor, static reader => new(reader),
        static host => new PersistedValue.Protraction(host.Angle, host.Mode),
        static (host, held, key) => Try.lift(() => ObjectMap.Update(held, host)).Run().Bind(static inner => inner));
    public static readonly NativeKind ImageSampler = Of<Grasshopper2.Parameters.Special.ImageSamplerObject, PersistedValue.Sampler>(
        "image-sampler", ObjectFamily.Sampler, static reader => new(reader),
        static host => new PersistedValue.Sampler(
            host.ImageUri,
            CapabilitySet<SamplerFlag>.Of([.. Seq(
                host.Normalised ? Some(SamplerFlag.Normalised) : None,
                host.Luminance ? Some(SamplerFlag.Luminance) : None,
                host.DrawSamples ? Some(SamplerFlag.DrawSamples) : None).Somes()]),
            host.LimitBehaviour),
        static (host, held, key) => Try.lift(() => {
            host.Normalised = held.Flags.Admits(SamplerFlag.Normalised);
            host.Luminance = held.Flags.Admits(SamplerFlag.Luminance);
            host.DrawSamples = held.Flags.Admits(SamplerFlag.DrawSamples);
            host.LimitBehaviour = held.LimitBehaviour;
            host.ImageUri = held.ImageUri;
        }).Run());
    public static readonly NativeKind PresetPicker = Of<Grasshopper2.Parameters.Special.PresetPickerObject, PersistedValue.Selection>(
        "preset-picker", ObjectFamily.Picker, static reader => new(reader),
        static host => new PersistedValue.Selection(
            Optional(host.UserNames).Map(static names => toSeq(names)), host.MultiSelect, host.Scroll0, host.Scroll1),
        static (host, held, key) => NativeObject.Select(host, held));
    public static readonly NativeKind ComplexPicker = Sealed<Grasshopper2.Parameters.Special.ComplexPickerObject, PersistedValue.Complexes>(
        "complex-picker", ObjectFamily.Picker, static reader => new(reader),
        static host => new PersistedValue.Complexes(toSeq(host.Values)),
        static (name, held) => new Grasshopper2.Parameters.Special.ComplexPickerObject(name, [.. held.Values]));
    public static readonly NativeKind ConstantPicker = Of<Grasshopper2.Parameters.Special.ConstantPickerObject, PersistedValue.Constant>(
        "constant-picker", ObjectFamily.Picker, static reader => new(reader),
        static host => new PersistedValue.Constant(host.Constant),
        static (host, held, key) => Try.lift(() => { host.Constant = held.Value; }).Run());
    public static readonly NativeKind MetaNamePicker = Of<Grasshopper2.Parameters.Special.MetaNamePickerObject, PersistedValue.MetaKey>(
        "meta-name-picker", ObjectFamily.Picker, static reader => new(reader),
        static host => new PersistedValue.MetaKey(host.MetaKey),
        static (host, held, key) => Try.lift(() => { host.MetaKey = held.Value; }).Run());
    public static readonly NativeKind TemporalPicker = Sealed<Grasshopper2.Parameters.Special.TemporalPickerObject, PersistedValue.Moment>(
        "temporal-picker", ObjectFamily.Picker, static reader => new(reader),
        static host => new PersistedValue.Moment(host.Date),
        static (name, held) => new Grasshopper2.Parameters.Special.TemporalPickerObject(name, held.Value));
    public static readonly NativeKind ValueList = Of<Grasshopper2.Parameters.Special.ValueListObject, PersistedValue.Listing>(
        "value-list", ObjectFamily.List, static reader => new(reader),
        static host => new PersistedValue.Listing(
            host.Mode, toSeq(Enumerable.Range(0, host.ItemCount)).Filter(host.ItemSelected)),
        static (host, held, key) => NativeObject.Reselect(host, held));
    public static readonly NativeKind PathMapper = Of<Grasshopper2.Parameters.Special.PathMapperObject, PersistedValue.Mapping>(
        "path-mapper", ObjectFamily.Data, static reader => new(reader),
        static host => new PersistedValue.Mapping(
            host.Notation,
            CapabilitySet<MappingFlag>.Of([.. Seq(
                host.OmitUnaffected ? Some(MappingFlag.OmitUnaffected) : None,
                host.WarnAboutPaths ? Some(MappingFlag.WarnPaths) : None,
                host.WarnAboutSites ? Some(MappingFlag.WarnSites) : None).Somes()])),
        static (host, held, key) => Try.lift(() => {
            host.OmitUnaffected = held.Flags.Admits(MappingFlag.OmitUnaffected);
            host.WarnAboutPaths = held.Flags.Admits(MappingFlag.WarnPaths);
            host.WarnAboutSites = held.Flags.Admits(MappingFlag.WarnSites);
            host.Notation = held.Notation;
        }).Run());
    public static readonly NativeKind DataPanel = Of<Grasshopper2.Parameters.Special.DataPanelObject, PersistedValue.PanelDisplay>(
        "data-panel", ObjectFamily.Data, static reader => new(reader),
        static host => new PersistedValue.PanelDisplay(
            CapabilitySet<PanelFacet>.Of([.. Seq(
                host.ShowColumns ? Some(PanelFacet.Columns) : None,
                host.ShowPaths ? Some(PanelFacet.Paths) : None,
                host.ShowIndices ? Some(PanelFacet.Indices) : None,
                host.ShowTypes ? Some(PanelFacet.Types) : None,
                host.ShowItems ? Some(PanelFacet.Items) : None,
                host.ShowMetas ? Some(PanelFacet.Metas) : None).Somes()]),
            host.VerticalOffset),
        static (host, held, key) => Try.lift(() => {
            host.ChangeDisplay(
                held.Shown.Admits(PanelFacet.Columns), held.Shown.Admits(PanelFacet.Paths),
                held.Shown.Admits(PanelFacet.Indices), held.Shown.Admits(PanelFacet.Types),
                held.Shown.Admits(PanelFacet.Items), held.Shown.Admits(PanelFacet.Metas));
            host.VerticalOffset = held.VerticalOffset;
        }).Run());
    public static readonly NativeKind DataRecorder = Of<Grasshopper2.Parameters.Special.DataRecorderObject, PersistedValue.Recording>(
        "data-recorder", ObjectFamily.Data, static reader => new(reader),
        static host => new PersistedValue.Recording(
            CapabilitySet<RecorderFlag>.Of([.. Seq(
                host.Paused ? Some(RecorderFlag.Paused) : None,
                host.MergeTrees ? Some(RecorderFlag.MergeTrees) : None).Somes()]),
            host.FrameLimit),
        static (host, held, key) => Try.lift(() => {
            host.MergeTrees = held.Flags.Admits(RecorderFlag.MergeTrees);
            host.FrameLimit = held.FrameLimit;
            host.Paused = held.Flags.Admits(RecorderFlag.Paused);
        }).Run());
    public static readonly NativeKind TreeViewer = Of<Grasshopper2.Parameters.Special.TreeViewerObject, PersistedValue.TreeDisplay>(
        "tree-viewer", ObjectFamily.Data, static reader => new(reader),
        static host => new PersistedValue.TreeDisplay(host.CanvasDisplay, host.ViewportDisplay, host.DisplayGradient),
        static (host, held, key) => Try.lift(() => ObjectMap.Update(held, host)).Run().Bind(static inner => inner));
    public static readonly NativeKind Timer = Of<Grasshopper2.Parameters.Special.TimerObject, PersistedValue.Targets>(
        "timer", ObjectFamily.Utility, static reader => new(reader),
        static host => new PersistedValue.Targets(
            toSeq(host.TargetIds), host.Delay, TimerMode.Of(running: host.Running, manual: host.Manual)),
        static (host, held, key) => NativeObject.Retarget(host, held.Ids).Bind(_ => Try.lift(() => {
            host.Delay = held.Delay;
            host.Running = held.Mode.RunningHost;
            host.Manual = held.Mode.ManualHost;
        }).Run()));
    public static readonly NativeKind Shout = Of<Grasshopper2.Parameters.Special.Shout, PersistedValue.Routing>(
        "shout", ObjectFamily.Routing, static reader => new(reader),
        static host => new PersistedValue.Routing(
            CapabilitySet<RoutingFlag>.Of([.. Seq(
                host.ClusterOutput ? Some(RoutingFlag.ClusterOutput) : None,
                host.StreamData ? Some(RoutingFlag.Stream) : None,
                host.StreamBackup ? Some(RoutingFlag.Backup) : None).Somes()]),
            host.StreamPath),
        static (host, held, key) => Try.lift(() => {
            host.ClusterOutput = held.Flags.Admits(RoutingFlag.ClusterOutput);
            host.StreamPath = held.StreamPath;
            host.StreamBackup = held.Flags.Admits(RoutingFlag.Backup);
            host.StreamData = held.Flags.Admits(RoutingFlag.Stream);
        }).Run());
    public static readonly NativeKind Listen = Of<Grasshopper2.Parameters.Special.Listen, PersistedValue.Listener>(
        "listen", ObjectFamily.Routing, static reader => new(reader),
        static host => new PersistedValue.Listener(
            host.DependencyIndex, host.DependencyA, host.DependencyB, host.DependencyC,
            host.DependencyD,
            CapabilitySet<ListenerFlag>.Of([.. Seq(
                host.ClusterInput ? Some(ListenerFlag.ClusterInput) : None,
                host.ClusterIndex ? Some(ListenerFlag.ClusterIndex) : None).Somes()])),
        static (host, held, key) => Try.lift(() => {
            host.DependencyA = held.A;
            host.DependencyB = held.B;
            host.DependencyC = held.C;
            host.DependencyD = held.D;
            host.DependencyIndex = held.Index;
            host.ClusterInput = held.Flags.Admits(ListenerFlag.ClusterInput);
            host.ClusterIndex = held.Flags.Admits(ListenerFlag.ClusterIndex);
        }).Run());
    public static readonly NativeKind Relay = Of<Grasshopper2.Parameters.Special.Relay, PersistedValue.Flag>(
        "relay", ObjectFamily.Routing, static reader => new(reader),
        static host => new PersistedValue.Flag(host.Frozen),
        static (host, held, key) => Try.lift(() => { host.Frozen = held.Value; }).Run());
    public static readonly NativeKind Cluster = Of<Grasshopper2.Components.Standard.Cluster, PersistedValue.Grouping>(
        "cluster", ObjectFamily.Composite, static reader => new(reader),
        static host => new PersistedValue.Grouping(
            CapabilitySet<ClusterFlag>.Of([.. Seq(
                host.LoopSolution ? Some(ClusterFlag.LoopSolution) : None,
                host.RelayMessages ? Some(ClusterFlag.RelayMessages) : None).Somes()])),
        static (host, held, key) => Try.lift(() => {
            host.LoopSolution = held.Flags.Admits(ClusterFlag.LoopSolution);
            host.RelayMessages = held.Flags.Admits(ClusterFlag.RelayMessages);
        }).Run());
    public static readonly NativeKind Chain = Inert<Grasshopper2.Components.Standard.Chain>(
        "chain", ObjectFamily.Composite, static reader => new(reader));
    public static readonly NativeKind Scribble = Of<Grasshopper2.SpecialObjects.ScribbleObject, PersistedValue.Annotation>(
        "scribble", ObjectFamily.Annotation, static reader => new(reader),
        static host => new PersistedValue.Annotation(
            host.Text, host.TextAngle, host.TextFont, host.TextStyle, host.TextColour, host.TextAlign),
        static (host, held, key) => Try.lift(() => {
            host.TextAngle = held.Angle;
            host.TextFont = held.Font;
            host.TextStyle = held.Style;
            host.TextColour = held.Colour;
            host.TextAlign = held.Align;
            host.Text = held.Value;
        }).Run());

    public ObjectFamily Family { get; }

    public Type Host { get; }

    [UseDelegateFromConstructor]
    public partial Grasshopper2.Doc.IDocumentObject Create();

    [UseDelegateFromConstructor]
    public partial Grasshopper2.Doc.IDocumentObject Rehydrate(GrasshopperIO.IReader reader);

    [UseDelegateFromConstructor]
    public partial PersistedValue Read(Grasshopper2.Doc.IDocumentObject host);

    [UseDelegateFromConstructor]
    public partial Fin<Grasshopper2.Doc.IDocumentObject> Mint(PersistedValue value);

    [UseDelegateFromConstructor]
    public partial Fin<Unit> Write(Grasshopper2.Doc.IDocumentObject host, PersistedValue value);

    private static readonly Lazy<FrozenDictionary<Type, NativeKind>> ByHost =
        new(static () => Items.ToFrozenDictionary(static row => row.Host), LazyThreadSafetyMode.ExecutionAndPublication);

    public static Option<NativeKind> ForHost(Type? host) =>
        toSeq(LanguageExt.List.unfold(host, static current =>
                Optional(current).Map(type => (type, type.BaseType))))
            .Choose(static probe => Admit.Probe<NativeKind>((out NativeKind row) => ByHost.Value.TryGetValue(probe, out row!)))
            .Head;

    private static NativeKind Of<THost, TValue>(
        string key, ObjectFamily family,
        Func<GrasshopperIO.IReader, THost> rehydrate,
        Func<THost, TValue> read,
        Func<THost, TValue, Fin<Unit>> write)
        where THost : class, Grasshopper2.Doc.IDocumentObject, new()
        where TValue : PersistedValue =>
        new(key, family, typeof(THost),
            static () => (Grasshopper2.Doc.IDocumentObject)new THost(),
            reader => rehydrate(reader),
            host => read((THost)host),
            (value, op) => Try.lift(static () => Fin.Succ((Grasshopper2.Doc.IDocumentObject)new THost())).Run().Bind(static inner => inner)
                .Bind(host => Pair<THost, TValue>(host, value, key, op)
                    .Bind(pair => write(pair.Host, pair.Value, op))
                    .Map(_ => host)),
            (host, value, op) => Pair<THost, TValue>(host, value).Bind(pair => write(pair.Host, pair.Value)));

    private static NativeKind Seeded<THost, TValue>(
        string key, ObjectFamily family,
        Func<GrasshopperIO.IReader, THost> rehydrate,
        Func<THost, TValue> read,
        Func<string, TValue, THost> seed,
        Func<THost, TValue, Fin<Unit>> write)
        where THost : class, Grasshopper2.Doc.IDocumentObject, new()
        where TValue : PersistedValue {
        Lazy<string> label = Label<THost>();
        return new(key, family, typeof(THost),
            static () => (Grasshopper2.Doc.IDocumentObject)new THost(),
            reader => rehydrate(reader),
            host => read((THost)host),
            (value, op) => Admitted<TValue>(value).Bind(held =>
                Try.lift(() => (Grasshopper2.Doc.IDocumentObject)seed(label.Value, held)).Run()
                    .Bind(host => write((THost)host, held, op).Map(_ => host))),
            (host, value, op) => Pair<THost, TValue>(host, value).Bind(pair => write(pair.Host, pair.Value)));
    }

    private static NativeKind Sealed<THost, TValue>(
        string key, ObjectFamily family,
        Func<GrasshopperIO.IReader, THost> rehydrate,
        Func<THost, TValue> read,
        Func<string, TValue, THost> seed)
        where THost : class, Grasshopper2.Doc.IDocumentObject, new()
        where TValue : PersistedValue {
        Lazy<string> label = Label<THost>();
        return new(key, family, typeof(THost),
            static () => (Grasshopper2.Doc.IDocumentObject)new THost(),
            reader => rehydrate(reader),
            host => read((THost)host),
            (value, op) => Admitted<TValue>(value).Bind(held =>
                Try.lift(() => (Grasshopper2.Doc.IDocumentObject)seed(label.Value, held)).Run()),
            (_, value, op) => Fin.Fail<Unit>(new GhFault.ContractRefused(GhContract.Object, new GhEvidence($"{key}:sealed:{value.GetType().Name}"))));
    }

    private static NativeKind Inert<THost>(string key, ObjectFamily family, Func<GrasshopperIO.IReader, THost> rehydrate)
        where THost : class, Grasshopper2.Doc.IDocumentObject, new() =>
        Of<THost, PersistedValue.Empty>(key, family, rehydrate,
            static _ => new PersistedValue.Empty(),
            static (_, _, _) => Fin.Succ(unit));

    private static Lazy<string> Label<THost>() where THost : Grasshopper2.Doc.IDocumentObject, new() =>
        new(static () => new THost().UserName, LazyThreadSafetyMode.ExecutionAndPublication);

    private static Fin<TValue> Admitted<TValue>(PersistedValue value, string key) where TValue : PersistedValue =>
        value is TValue held
            ? Fin.Succ(held)
            : Fin.Fail<TValue>(new GhFault.ContractRefused(GhContract.Object, new GhEvidence($"{key}:{value.GetType().Name}")));

    private static Fin<(THost Host, TValue Value)> Pair<THost, TValue>(
        Grasshopper2.Doc.IDocumentObject host, PersistedValue value, string key)
        where THost : class, Grasshopper2.Doc.IDocumentObject
        where TValue : PersistedValue =>
        (host, value) switch {
            (THost typed, TValue held) => Fin.Succ((typed, held)),
            _ => Fin.Fail<(THost, TValue)>(new GhFault.ContractRefused(GhContract.Object, new GhEvidence($"{key}:{host.GetType().Name}:{value.GetType().Name}"))),
        };
}
```

## [04]-[OBJECT_OPERATIONS]

- Owner: `NativeObject` is the one operation surface over the catalog — mint, archive rehydration and persistence, value read, value assignment, momentary pulse, list stepping, timer-target reconciliation, cluster construction and map resolution, and disentanglement. Every per-type correspondence lives on its `NativeKind` row, so this surface holds no type roster and grows by zero lines when a row lands.
- Entry: `Row(IDocumentObject)` is the one dispatch — `NativeKind.ForHost` over the live object's own ancestry — and an uncatalogued type is the single refusal every value-bearing entry shares.
- Entry: `Mint` is the row's own column: `Empty` selects the parameterless constructor and every other value enters the row's factory arm, so nothing re-enters a second write and no payload pairing survives outside its row's declaration.
- Entry: `Rehydrate(NativeKind, IReader)` and `Persist(IDocumentObject, IWriter)` close the archive round trip — the read leg dispatches the row's `Rehydrate` column and the write leg calls `IStorable.Store` on the interface, so a document object read out of `GrasshopperIO` and written back never leaves the catalog.
- Law: `Clustered` names the constructor's first `out Guid[][]` as the input mapping and the second as the output mapping; `Boundary` calls the public `void EnsureMaps(out Listen[], out Shout[])` and returns the resulting pin rosters without inventing a success probe.
- Auto: `Retarget` rejects the timer's own instance id before mutation, then reconciles against ONE snapshot — both the remove set and the add set derive from the same captured `TargetIds` read, because the former add-leg re-queried the LIVE `IsTarget` after removals had already mutated it (a time-of-check drift the snapshot deletes); each `AddTarget`/`RemoveTarget` admits through the kernel `Admit.Confirm` on its public boolean result, and every out-parameter host probe in the catalog lifts through `Admit.Probe` (S1-24).
- Growth: a new interactive object is one catalog row; a host verb that pulses or steps rather than sets is one delegate vocabulary beside `Assign` (`ButtonPulse`, `ListStep`), because a verb has no persisted value to carry.
- Boundary: `NumberSliderObject.InternalNumber` and `NumberPickerObject.InternalNumber` are read-only and seed only through their public constructors; the slider preserves its complete non-null `UiNumber`, while the picker carries its public `decimal` read through the host's lossy `double` constructor boundary. `ComplexPickerObject.Values` and `TemporalPickerObject.Date` carry `internal set`, so both rows are `Sealed` and their `Write` refuses rather than silently succeeding. `PresetPickerObject.UserNames`, including its `null` state, owns persisted selection; assignment expires and restarts a solution only when that state changes. `ValueListObject.ItemCount` and `Items` are read-only, so a selection assigns index by index through `SelectItem`/`DeselectItem` under the list's own `Mode` law, and `SelectPrev`/`SelectNext` are cyclic steps carried by `Step`, never a selection set. `DataRecorderObject.Paused` is the recorder's writable state, and its `IsEmpty` answers true when recorded buckets exist — the member name and XML summary invert the installed behavior — so a presence probe reads a true `IsEmpty` as data-present evidence. `DataPanelObject.ChangeDisplay` writes the six display flags in one host call and `VerticalOffset` writes beside it. `FunctionEditorObject`, `QuickGraphObject`, and `Chain` publish no readable or writable value state, so they take the `Inert` arm and read as `Empty`. `Boundary` rejects null pins from the oblivious host arrays. Incomplete host chain ordering and validation members, the internal loop driver, and private `ImageSamplerObject.SampleContinuous` never enter this operation surface.

```csharp
// --- [IMPORTS] -------------------------------------------------------------------------
using Rasm.Domain;

namespace Rasm.Grasshopper.Components;

// --- [TYPES] ---------------------------------------------------------------------------

[SmartEnum]
public sealed partial class ListStep {
    public static readonly ListStep Previous = new(static list => list.SelectPrev());
    public static readonly ListStep Next = new(static list => list.SelectNext());

    [UseDelegateFromConstructor]
    public partial void Advance(Grasshopper2.Parameters.Special.ValueListObject list);
}

[SmartEnum]
public sealed partial class ButtonPulse {
    public static readonly ButtonPulse Press = new(static button => button.Press());
    public static readonly ButtonPulse Release = new(static button => button.Release());

    [UseDelegateFromConstructor]
    public partial void Drive(Grasshopper2.Parameters.Special.ButtonObject button);
}

// --- [OPERATIONS] ----------------------------------------------------------------------

public static class NativeObject {
    public static Fin<Grasshopper2.Doc.IDocumentObject> Mint(NativeKind? kind, PersistedValue? seed) =>
        (kind, seed) switch {
            (null, _) or (_, null) => Fin.Fail<Grasshopper2.Doc.IDocumentObject>(
                new GhFault.ContractRefused(GhContract.Object, new GhEvidence(nameof(Mint)))),
            (var row, PersistedValue.Empty) => Try.lift(() => row.Create()).Run(),
            var (row, value) => row.Mint(value),
        };

    public static Fin<Grasshopper2.Doc.IDocumentObject> Rehydrate(
        NativeKind? kind, GrasshopperIO.IReader? reader) =>
        (kind, reader) switch {
            (null, _) or (_, null) => Fin.Fail<Grasshopper2.Doc.IDocumentObject>(
                new GhFault.ContractRefused(GhContract.Object, new GhEvidence(nameof(Rehydrate)))),
            var (row, source) => Try.lift(() => row.Rehydrate(source)).Run(),
        };

    public static Fin<Unit> Persist(
        Grasshopper2.Doc.IDocumentObject? host, GrasshopperIO.IWriter? writer) =>
        (host, writer) switch {
            (null, _) or (_, null) => Fin.Fail<Unit>(new GhFault.ContractRefused(GhContract.Object, new GhEvidence(nameof(Persist)))),
            var (target, sink) => Try.lift(() => target.Store(sink)).Run().Bind(static inner => inner),
        };

    public static Fin<PersistedValue> ValueOf(Grasshopper2.Doc.IDocumentObject? host) =>
        Row(host, nameof(ValueOf))
            .Bind(pair => Try.lift(() => pair.Row.Read(pair.Host)).Run());

    public static Fin<Unit> Assign(Grasshopper2.Doc.IDocumentObject? host, PersistedValue? value) =>
        value is null
            ? Fin.Fail<Unit>(new GhFault.ContractRefused(GhContract.Object, new GhEvidence(nameof(Assign))))
            : Row(host, nameof(Assign))
                .Bind(pair => pair.Row.Write(pair.Host, value));

    public static Fin<Unit> Step(
        Grasshopper2.Parameters.Special.ValueListObject? list, ListStep step) =>
        (list, step) switch {
            (null, _) or (_, null) => Fin.Fail<Unit>(new GhFault.ContractRefused(GhContract.Object, new GhEvidence(nameof(Step)))),
            var (live, row) => Try.lift(() => row.Advance(live)).Run().Bind(static inner => inner),
        };

    public static Fin<Unit> Pulse(
        Grasshopper2.Parameters.Special.ButtonObject? button, ButtonPulse pulse) =>
        (button, pulse) switch {
            (null, _) or (_, null) => Fin.Fail<Unit>(new GhFault.ContractRefused(GhContract.Object, new GhEvidence(nameof(Pulse)))),
            var (live, row) => Try.lift(() => row.Drive(live)).Run().Bind(static inner => inner),
        };

    private static Fin<(NativeKind Row, Grasshopper2.Doc.IDocumentObject Host)> Row(
        Grasshopper2.Doc.IDocumentObject? host, string verb) =>
        host is null
            ? Fin.Fail<(NativeKind, Grasshopper2.Doc.IDocumentObject)>(new GhFault.ContractRefused(GhContract.Object, new GhEvidence(verb)))
            : NativeKind.ForHost(host.GetType())
                .Map(row => (row, host))
                .ToFin(new GhFault.ContractRefused(GhContract.Object, new GhEvidence($"{verb}:{host.GetType().Name}")));

    public static Fin<Unit> Retarget(Grasshopper2.Parameters.Special.TimerObject? timer, Seq<Guid> desired) =>
        timer is null || desired.Contains(timer.InstanceId)
            ? Fin.Fail<Unit>(new GhFault.ContractRefused(GhContract.Object, new GhEvidence(nameof(Retarget))))
            : Reconcile(timer, toSeq(desired.Distinct()));

    private static Fin<Unit> Reconcile(Grasshopper2.Parameters.Special.TimerObject timer, Seq<Guid> desired) =>
        Try.lift(() => toSeq(timer.TargetIds)).Run()
            .Bind(current => current.Filter(id => !desired.Contains(id))
                .TraverseM(id => Target(timer, id, add: false))
                .As()
                .Bind(_ => desired.Filter(id => !current.Contains(id))
                    .TraverseM(id => Target(timer, id, add: true))
                    .As())
                .Map(static _ => unit));

    public static Fin<(Grasshopper2.Components.Standard.Cluster Cluster, Guid[][] InputMapping, Guid[][] OutputMapping)> Clustered(
        Grasshopper2.Doc.IDocumentObject[] members) =>
        Try.lift(() => {
            Grasshopper2.Components.Standard.Cluster cluster = new(
                members, out Guid[][] inputMapping, out Guid[][] outputMapping);
            return Fin.Succ((cluster, inputMapping, outputMapping));
        }).Run().Bind(static inner => inner);

    public static Fin<(Seq<Grasshopper2.Parameters.Special.Listen> Inputs, Seq<Grasshopper2.Parameters.Special.Shout> Outputs)> Boundary(
        Grasshopper2.Components.Standard.Cluster cluster) =>
        Try.lift(() => {
            cluster.EnsureMaps(
                out Grasshopper2.Parameters.Special.Listen[] listeners,
                out Grasshopper2.Parameters.Special.Shout[] shouters);
            return listeners is null ||
                   shouters is null ||
                   listeners.Any(static item => item is null) ||
                   shouters.Any(static item => item is null)
                ? Fin.Fail<(Seq<Grasshopper2.Parameters.Special.Listen>, Seq<Grasshopper2.Parameters.Special.Shout>)>(
                    new GhFault.ContractRefused(GhContract.Object, new GhEvidence(nameof(Boundary))))
                : Fin.Succ((toSeq(listeners), toSeq(shouters)));
        }).Run().Bind(static inner => inner);

    public static Fin<Unit> Disentangle(Grasshopper2.Components.Standard.Cluster cluster, Grasshopper2.Undo.ActionList actions) =>
        Try.lift(() => cluster.Disentangle(actions)).Run().Bind(static inner => inner);

    private static Fin<Unit> Target(
        Grasshopper2.Parameters.Special.TimerObject timer, Guid id, bool add) =>
        Try.lift(() => add ? timer.AddTarget(id) : timer.RemoveTarget(id)).Run()
            .Bind(changed => Admit.Confirm(success: changed));

    internal static Fin<Unit> Select(
        Grasshopper2.Parameters.Special.PresetPickerObject picker,
        PersistedValue.Selection desired) =>
        Try.lift(() => {
            picker.MultiSelect = desired.MultiSelect;
            picker.Scroll0 = desired.Scroll0;
            picker.Scroll1 = desired.Scroll1;
            string[]? names = HostEdge.Slot(desired.UserNames.Map(static values => values.ToArray()));
            if ((picker.UserNames is null && names is null) ||
                (picker.UserNames is { } current && names is not null && current.SequenceEqual(names))) {
                return;
            }
            picker.UserNames = names;
            picker.Expire();
        }).Run().Bind(static inner => inner);

    internal static Fin<Unit> Reselect(
        Grasshopper2.Parameters.Special.ValueListObject list,
        PersistedValue.Listing desired) =>
        Try.lift(() => { list.Mode = desired.Mode; }).Run()
            .Bind(_ => Try.lift(() => toSeq(Enumerable.Range(0, list.ItemCount))).Run())
            .Bind(indexes => indexes
                .TraverseM(index => Try.lift(() => {
                    if (desired.Selected.Contains(index) == list.ItemSelected(index)) { return; }
                    if (desired.Selected.Contains(index)) { list.SelectItem(index); } else { list.DeselectItem(index); }
                }).Run().Bind(static inner => inner))
                .As()
                .Map(static _ => unit));
}
```

## [05]-[RESEARCH]

<!-- source-only: research row template:
[TOKEN]-[OPEN|BLOCKED]: <exact question>; <verification route>.
-->

(none)
