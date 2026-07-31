# [RASM_GRASSHOPPER_OBJECTS]

`NativeObject` is the native document-object catalog: the interactive `Grasshopper2.Parameters.Special` family, the `Shout`/`Listen`/`Relay` routing pins, the public `Cluster`/`Chain` composite family, and the `Grasshopper2.SpecialObjects.ScribbleObject` annotation land as rows of one `NativeKind` catalog. `PersistedValue` closes the values the public host surface can read or assign, and each catalog row carries every per-type correspondence as a column — parameterless mint, `GrasshopperIO.IReader` rehydration, read, seeded mint, and write — so a row is unconstructible until it answers all five and no type-switch roster stands beside the catalog.

One polymorphic owner mints, rehydrates, persists, reads, assigns, pulses a button, steps a list selection, reconciles timer targets, and resolves cluster maps on the rail; GH1 interop remains one explicit live-host boundary returning a typed receipt beside the host component. GH2's loop driver, looping iterations, repeat discriminants, bitmap sampler kernel, and incomplete chain ordering and validation kernels stay outside the package contract.

## [01]-[INDEX]

- [02]-[CONTROL_VOCABULARY]: object families carrying the public boundary and accumulation vocabularies
- [03]-[VALUE_AND_CATALOG]: `PersistedValue` closes the state shapes and `NativeKind` catalogs the rows with all five construction columns
- [04]-[OBJECT_OPERATIONS]: one owner covers mint, archive round trip, value, verbs, timer targets, and cluster maps
- [05]-[GH1_BOUNDARY]: live GH1 host admission returns its typed receipt

## [02]-[CONTROL_VOCABULARY]

- Owner: three keyless `[SmartEnum]` vocabularies close the object-family and the public cluster-control discriminants; each control row carries its host value as a column, so host values cross only at member calls.
- Cases: `ObjectFamily` partitions the catalog; `AccumulationMode` and `BoundaryRole` mirror the public `Accumulation` and `ClusterBoundary` enums.
- Growth: a new host discriminant value is one row on the owning vocabulary.
- Boundary: `LoopingAction`, `LoopRepeats`, `Loop`, and `LoopingIteration` are assembly-internal; the public `Cluster.LoopSolution` switch is the only loop state this boundary may assign.

```csharp signature
namespace Rasm.Grasshopper.Components;

// --- [TYPES] -----------------------------------------------------------------------------

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

[SmartEnum]
public sealed partial class AccumulationMode {
    public static readonly AccumulationMode Skip = new(Grasshopper2.Components.Standard.Accumulation.None);
    public static readonly AccumulationMode Listed = new(Grasshopper2.Components.Standard.Accumulation.List);
    public static readonly AccumulationMode Layered = new(Grasshopper2.Components.Standard.Accumulation.Layered);
    public static readonly AccumulationMode Last = new(Grasshopper2.Components.Standard.Accumulation.Last);

    public Grasshopper2.Components.Standard.Accumulation Host { get; }
}

[SmartEnum]
public sealed partial class BoundaryRole {
    public static readonly BoundaryRole Free = new(Grasshopper2.Components.Standard.ClusterBoundary.None);
    public static readonly BoundaryRole Input = new(Grasshopper2.Components.Standard.ClusterBoundary.Input);
    public static readonly BoundaryRole Output = new(Grasshopper2.Components.Standard.ClusterBoundary.Output);
    public static readonly BoundaryRole Index = new(Grasshopper2.Components.Standard.ClusterBoundary.Index);

    public Grasshopper2.Components.Standard.ClusterBoundary Host { get; }
}
```

## [03]-[VALUE_AND_CATALOG]

- Owner: `PersistedValue` is the one union over empty construction and publicly readable or writable object state, one case per distinct state shape the catalog publishes; `NativeKind` carries family, exact host type, and FIVE construction columns — parameterless `Create`, `IReader` `Rehydrate`, `Read`, seeded `Mint`, and `Write`.
- Entry: `NativeKind.ForHost(Type)` walks the candidate's own ancestry against the `Items`-derived frozen index, so a host subclass resolves onto the nearest catalogued row and only a genuinely foreign type misses.
- Law: read and write are ROW COLUMNS, never a type-switch roster beside the catalog — a row is minted through one of three generic factories that close the host type and the payload case together, so the compiler demands every arm at the declaration and a catalogued row missing one cannot be constructed. The three factories ARE the seeding discriminant: `Of` for a row the parameterless constructor mints and the write column completes, `Seeded` for a row whose constructor takes the value AND whose remaining columns stay settable, `Sealed` for a row whose constructor is the whole write and whose `Write` therefore refuses.
- Law: the archive round trip is two columns and one interface call, never a case family — every catalogued type declares a `public T(IReader)` constructor, so `Rehydrate` sits beside `Create`, while `Grasshopper2.Doc.IDocumentObject : GrasshopperIO.IStorable` publishes `Store(IWriter)` on the interface itself, so the write leg is one polymorphic call with no per-row data. `Grasshopper2.SpecialObjects.ScribbleObject` overrides no `Store` and persists through `CustomValues` under the base body; the interface call still reaches it, which is exactly why that leg carries no column.
- Law: a seeding constructor's first argument is the host's own default user name, read once per row off a lazily minted instance — the row KEY is wire identity in the `[a-z0-9-]` grammar and never crosses into a canvas-visible name, so the two spaces cannot drift into each other.
- Packages: every host type column is a verified `Grasshopper2.Parameters.Special`, `Grasshopper2.Components.Standard`, or `Grasshopper2.SpecialObjects` type; canvas control state stays each object's own `CreateAttributes` projection.
- Growth: a new interactive object is one catalog row naming its factory arm and, where its state shape is new, one `PersistedValue` case.
- Boundary: `Empty` selects the row's parameterless constructor; every other value enters through the row's own `Mint`. Four payload columns are read-and-seed only, because the host publishes no public setter: `Slider.Value` and `Number.Value` (`InternalNumber` is derived from the internal slider and picker), `Complexes.Values` and `Moment.Value` (`internal set`), and `Ramp.Value` (`Gradient` projects `GripGradientInteraction.ModifiedGradient`, so `Interaction` is the writable source). `Histogram` carries the integral `BucketCount` and never its `BucketCountText` spelling, which restates the same fact through the negative `Buckets*` sentinel constants.

```csharp signature
// --- [MODELS] ----------------------------------------------------------------------------

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record PersistedValue {
    private PersistedValue() { }

    public sealed record Empty : PersistedValue;
    public sealed record Flag(bool Value) : PersistedValue;
    public sealed record Slider(
        Grasshopper2.UI.UiNumber Value, Grasshopper2.UI.Slider.GripShape Grip,
        Eto.Drawing.Color Colour, string Format) : PersistedValue;
    public sealed record Number(decimal Value, Eto.Drawing.Color Colour, bool Snap) : PersistedValue;
    public sealed record Protraction(
        Grasshopper2.Types.Numeric.Angle Value, Grasshopper2.Parameters.Special.ProtractorMode Mode) : PersistedValue;
    public sealed record Moment(DateTime Value) : PersistedValue;
    public sealed record Complexes(Seq<System.Numerics.Complex> Values) : PersistedValue;
    public sealed record Constant(Grasshopper2.Maths.Constant Value) : PersistedValue;
    public sealed record MetaKey(Grasshopper2.Data.Meta.MetaName Value) : PersistedValue;
    public sealed record Momentary(
        Grasshopper2.Parameters.Special.ButtonAction Action,
        Option<Grasshopper2.Data.ITree> Up, Option<Grasshopper2.Data.ITree> Down,
        Grasshopper2.Types.Colour.Colour UpColour, Grasshopper2.Types.Colour.Colour DownColour,
        string UpText, string DownText) : PersistedValue;
    public sealed record Text(
        string Value, bool PerLine, Grasshopper2.Parameters.Special.TextInputEscaping Escaping) : PersistedValue;
    public sealed record Parsed(string Source, Grasshopper2.Parsing.Notation Notations) : PersistedValue;
    public sealed record Annotation(
        string Value, int Angle, Grasshopper2.SpecialObjects.ScribbleFont Font,
        Eto.Drawing.FontStyle Style, Eto.Drawing.OpenColor.Family Colour,
        Eto.Forms.TextAlignment Align) : PersistedValue;
    public sealed record Swatch(Grasshopper2.Types.Colour.Colour Value, bool Apply) : PersistedValue;
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
        string ImageUri, bool Normalised, bool Luminance,
        Grasshopper2.Parameters.Special.ImageSamplerObject.SamplingLimit LimitBehaviour, bool DrawSamples) : PersistedValue;
    public sealed record Selection(
        Option<Seq<string>> UserNames, bool MultiSelect, float Scroll0, float Scroll1) : PersistedValue;
    public sealed record Listing(
        Grasshopper2.Parameters.Special.ValueListMode Mode, Seq<int> Selected) : PersistedValue;
    public sealed record Mapping(
        string Notation, bool OmitUnaffected, bool WarnAboutPaths, bool WarnAboutSites) : PersistedValue;
    public sealed record PanelDisplay(
        bool ShowColumns, bool ShowPaths, bool ShowIndices, bool ShowTypes,
        bool ShowItems, bool ShowMetas, float VerticalOffset) : PersistedValue;
    public sealed record TreeDisplay(
        Grasshopper2.Parameters.Special.TreeCanvasDisplay CanvasDisplay,
        Grasshopper2.Parameters.Special.TreeViewportDisplay ViewportDisplay,
        Grasshopper2.Types.Colour.Gradient DisplayGradient) : PersistedValue;
    public sealed record Recording(bool Paused, bool MergeTrees, int FrameLimit) : PersistedValue;
    public sealed record Targets(Seq<Guid> Ids, TimeSpan Delay, bool Running, bool Manual) : PersistedValue;
    public sealed record Routing(
        bool ClusterOutput, bool StreamData, string StreamPath, bool StreamBackup) : PersistedValue;
    public sealed record Listener(
        Grasshopper2.Parameters.Special.Listen.Dependency Index,
        GrasshopperIO.AbsRelPaths A, GrasshopperIO.AbsRelPaths B,
        GrasshopperIO.AbsRelPaths C, GrasshopperIO.AbsRelPaths D,
        bool ClusterInput, bool ClusterIndex) : PersistedValue;
    public sealed record Grouping(bool LoopSolution, bool RelayMessages) : PersistedValue;
}

// --- [SERVICES] --------------------------------------------------------------------------

[SmartEnum<string>]
public sealed partial class NativeKind {
    public static readonly NativeKind NumberSlider = Seeded<Grasshopper2.Parameters.Special.NumberSliderObject, PersistedValue.Slider>(
        "number-slider", ObjectFamily.ValueInput, static reader => new(reader),
        static host => new PersistedValue.Slider(host.InternalNumber, host.GripDisplay, host.GripColour, host.GripFormat),
        static (name, held) => new Grasshopper2.Parameters.Special.NumberSliderObject(name, held.Value),
        static (host, held, key) => Hosted.Bound(() => {
            host.GripDisplay = held.Grip;
            host.GripColour = held.Colour;
            host.GripFormat = held.Format;
        }, key));
    // The host picker publishes decimal and constructs from double, so the seed crosses that lossy boundary once.
    public static readonly NativeKind NumberPicker = Seeded<Grasshopper2.Parameters.Special.NumberPickerObject, PersistedValue.Number>(
        "number-picker", ObjectFamily.ValueInput, static reader => new(reader),
        static host => new PersistedValue.Number(host.InternalNumber, host.GripColour, host.SnapToTicks),
        static (name, held) => new Grasshopper2.Parameters.Special.NumberPickerObject(name, (double)held.Value),
        static (host, held, key) => Hosted.Bound(() => {
            host.GripColour = held.Colour;
            host.SnapToTicks = held.Snap;
        }, key));
    public static readonly NativeKind Toggle = Of<Grasshopper2.Parameters.Special.ToggleObject, PersistedValue.Flag>(
        "toggle", ObjectFamily.ValueInput, static reader => new(reader),
        static host => new PersistedValue.Flag(host.ToggleState),
        static (host, held, key) => Hosted.Bound(() => { host.ToggleState = held.Value; }, key));
    public static readonly NativeKind Button = Of<Grasshopper2.Parameters.Special.ButtonObject, PersistedValue.Momentary>(
        "button", ObjectFamily.ValueInput, static reader => new(reader),
        static host => new PersistedValue.Momentary(
            host.Action, Optional(host.UpTree), Optional(host.DownTree),
            host.UpColour, host.DownColour, host.UpText, host.DownText),
        static (host, held, key) => Hosted.Bound(() => {
            host.Action = held.Action;
            held.Up.Iter(tree => host.UpTree = tree);
            held.Down.Iter(tree => host.DownTree = tree);
            host.UpColour = held.UpColour;
            host.DownColour = held.DownColour;
            host.UpText = held.UpText;
            host.DownText = held.DownText;
        }, key));
    public static readonly NativeKind Value = Of<Grasshopper2.Parameters.Special.ValueObject, PersistedValue.Parsed>(
        "value", ObjectFamily.ValueInput, static reader => new(reader),
        static host => new PersistedValue.Parsed(host.Text, host.Notations),
        static (host, held, key) => Hosted.Bound(() => {
            host.Notations = held.Notations;
            host.AssignTextAndValue(held.Source);
        }, key));
    public static readonly NativeKind TextInput = Of<Grasshopper2.Parameters.Special.TextInputObject, PersistedValue.Text>(
        "text-input", ObjectFamily.ValueInput, static reader => new(reader),
        static host => new PersistedValue.Text(host.Contents, host.OneEntryPerLine, host.Escaping),
        static (host, held, key) => Hosted.Bound(() => {
            host.OneEntryPerLine = held.PerLine;
            host.Escaping = held.Escaping;
            host.Contents = held.Value;
        }, key));
    public static readonly NativeKind ColourSwatch = Of<Grasshopper2.Parameters.Special.ColourSwatchObject, PersistedValue.Swatch>(
        "colour-swatch", ObjectFamily.ValueInput, static reader => new(reader),
        static host => new PersistedValue.Swatch(host.Colour, Apply: false),
        static (host, held, key) => Hosted.Bound(() => host.SetColour(held.Value, held.Apply), key));
    public static readonly NativeKind GradientEditor = Of<Grasshopper2.Parameters.Special.GradientEditorObject, PersistedValue.Ramp>(
        "gradient-editor", ObjectFamily.Editor, static reader => new(reader),
        static host => new PersistedValue.Ramp(host.Gradient, host.Parameter0, host.Parameter1, host.Interaction),
        static (host, held, key) => Hosted.Bound(() => {
            host.Parameter0 = held.Parameter0;
            host.Parameter1 = held.Parameter1;
            host.Interaction = held.Interaction;
        }, key));
    public static readonly NativeKind FunctionEditor = Inert<Grasshopper2.Parameters.Special.FunctionEditorObject>(
        "function-editor", ObjectFamily.Editor, static reader => new(reader));
    public static readonly NativeKind MaterialEditor = Of<Grasshopper2.Parameters.Special.MaterialEditorObject, PersistedValue.Material>(
        "material-editor", ObjectFamily.Editor, static reader => new(reader),
        static host => new PersistedValue.Material(host.Material, host.ForeRotation, host.BackRotation, host.IdenticalForeAndBack),
        static (host, held, key) => Hosted.Bound(() => {
            host.ForeRotation = held.Fore;
            host.BackRotation = held.Back;
            host.IdenticalForeAndBack = held.Identical;
            host.Material = held.Value;
        }, key));
    public static readonly NativeKind Histogram = Of<Grasshopper2.Parameters.Special.HistogramObject, PersistedValue.Histogram>(
        "histogram", ObjectFamily.Editor, static reader => new(reader),
        static host => new PersistedValue.Histogram(host.Style, host.Palette, host.BucketCount, host.BucketRange),
        static (host, held, key) => Hosted.Bound(() => {
            host.Style = held.Style;
            host.Palette = held.Palette;
            host.BucketCount = held.BucketCount;
            host.BucketRange = held.BucketRange;
        }, key));
    public static readonly NativeKind QuickGraph = Inert<Grasshopper2.Parameters.Special.QuickGraphObject>(
        "quick-graph", ObjectFamily.Editor, static reader => new(reader));
    public static readonly NativeKind Protractor = Of<Grasshopper2.Parameters.Special.ProtractorObject, PersistedValue.Protraction>(
        "protractor", ObjectFamily.Editor, static reader => new(reader),
        static host => new PersistedValue.Protraction(host.Angle, host.Mode),
        static (host, held, key) => Hosted.Bound(() => {
            host.Mode = held.Mode;
            host.Angle = held.Value;
        }, key));
    public static readonly NativeKind ImageSampler = Of<Grasshopper2.Parameters.Special.ImageSamplerObject, PersistedValue.Sampler>(
        "image-sampler", ObjectFamily.Sampler, static reader => new(reader),
        static host => new PersistedValue.Sampler(
            host.ImageUri, host.Normalised, host.Luminance, host.LimitBehaviour, host.DrawSamples),
        static (host, held, key) => Hosted.Bound(() => {
            host.Normalised = held.Normalised;
            host.Luminance = held.Luminance;
            host.LimitBehaviour = held.LimitBehaviour;
            host.DrawSamples = held.DrawSamples;
            host.ImageUri = held.ImageUri;
        }, key));
    public static readonly NativeKind PresetPicker = Of<Grasshopper2.Parameters.Special.PresetPickerObject, PersistedValue.Selection>(
        "preset-picker", ObjectFamily.Picker, static reader => new(reader),
        static host => new PersistedValue.Selection(
            Optional(host.UserNames).Map(static names => toSeq(names)), host.MultiSelect, host.Scroll0, host.Scroll1),
        static (host, held, key) => NativeObject.Select(host, held, key));
    public static readonly NativeKind ComplexPicker = Sealed<Grasshopper2.Parameters.Special.ComplexPickerObject, PersistedValue.Complexes>(
        "complex-picker", ObjectFamily.Picker, static reader => new(reader),
        static host => new PersistedValue.Complexes(toSeq(host.Values)),
        static (name, held) => new Grasshopper2.Parameters.Special.ComplexPickerObject(name, [.. held.Values]));
    public static readonly NativeKind ConstantPicker = Of<Grasshopper2.Parameters.Special.ConstantPickerObject, PersistedValue.Constant>(
        "constant-picker", ObjectFamily.Picker, static reader => new(reader),
        static host => new PersistedValue.Constant(host.Constant),
        static (host, held, key) => Hosted.Bound(() => { host.Constant = held.Value; }, key));
    public static readonly NativeKind MetaNamePicker = Of<Grasshopper2.Parameters.Special.MetaNamePickerObject, PersistedValue.MetaKey>(
        "meta-name-picker", ObjectFamily.Picker, static reader => new(reader),
        static host => new PersistedValue.MetaKey(host.MetaKey),
        static (host, held, key) => Hosted.Bound(() => { host.MetaKey = held.Value; }, key));
    public static readonly NativeKind TemporalPicker = Sealed<Grasshopper2.Parameters.Special.TemporalPickerObject, PersistedValue.Moment>(
        "temporal-picker", ObjectFamily.Picker, static reader => new(reader),
        static host => new PersistedValue.Moment(host.Date),
        static (name, held) => new Grasshopper2.Parameters.Special.TemporalPickerObject(name, held.Value));
    public static readonly NativeKind ValueList = Of<Grasshopper2.Parameters.Special.ValueListObject, PersistedValue.Listing>(
        "value-list", ObjectFamily.List, static reader => new(reader),
        static host => new PersistedValue.Listing(
            host.Mode, toSeq(Enumerable.Range(0, host.ItemCount)).Filter(host.ItemSelected)),
        static (host, held, key) => NativeObject.Reselect(host, held, key));
    public static readonly NativeKind PathMapper = Of<Grasshopper2.Parameters.Special.PathMapperObject, PersistedValue.Mapping>(
        "path-mapper", ObjectFamily.Data, static reader => new(reader),
        static host => new PersistedValue.Mapping(
            host.Notation, host.OmitUnaffected, host.WarnAboutPaths, host.WarnAboutSites),
        static (host, held, key) => Hosted.Bound(() => {
            host.OmitUnaffected = held.OmitUnaffected;
            host.WarnAboutPaths = held.WarnAboutPaths;
            host.WarnAboutSites = held.WarnAboutSites;
            host.Notation = held.Notation;
        }, key));
    public static readonly NativeKind DataPanel = Of<Grasshopper2.Parameters.Special.DataPanelObject, PersistedValue.PanelDisplay>(
        "data-panel", ObjectFamily.Data, static reader => new(reader),
        static host => new PersistedValue.PanelDisplay(
            host.ShowColumns, host.ShowPaths, host.ShowIndices, host.ShowTypes,
            host.ShowItems, host.ShowMetas, host.VerticalOffset),
        static (host, held, key) => Hosted.Bound(() => {
            host.ChangeDisplay(
                held.ShowColumns, held.ShowPaths, held.ShowIndices, held.ShowTypes, held.ShowItems, held.ShowMetas);
            host.VerticalOffset = held.VerticalOffset;
        }, key));
    public static readonly NativeKind DataRecorder = Of<Grasshopper2.Parameters.Special.DataRecorderObject, PersistedValue.Recording>(
        "data-recorder", ObjectFamily.Data, static reader => new(reader),
        static host => new PersistedValue.Recording(host.Paused, host.MergeTrees, host.FrameLimit),
        static (host, held, key) => Hosted.Bound(() => {
            host.MergeTrees = held.MergeTrees;
            host.FrameLimit = held.FrameLimit;
            host.Paused = held.Paused;
        }, key));
    public static readonly NativeKind TreeViewer = Of<Grasshopper2.Parameters.Special.TreeViewerObject, PersistedValue.TreeDisplay>(
        "tree-viewer", ObjectFamily.Data, static reader => new(reader),
        static host => new PersistedValue.TreeDisplay(host.CanvasDisplay, host.ViewportDisplay, host.DisplayGradient),
        static (host, held, key) => Hosted.Bound(() => {
            host.CanvasDisplay = held.CanvasDisplay;
            host.ViewportDisplay = held.ViewportDisplay;
            host.DisplayGradient = held.DisplayGradient;
        }, key));
    public static readonly NativeKind Timer = Of<Grasshopper2.Parameters.Special.TimerObject, PersistedValue.Targets>(
        "timer", ObjectFamily.Utility, static reader => new(reader),
        static host => new PersistedValue.Targets(toSeq(host.TargetIds), host.Delay, host.Running, host.Manual),
        static (host, held, key) => NativeObject.Retarget(host, held.Ids, key).Bind(_ => Hosted.Bound(() => {
            host.Delay = held.Delay;
            host.Running = held.Running;
            host.Manual = held.Manual;
        }, key)));
    public static readonly NativeKind Shout = Of<Grasshopper2.Parameters.Special.Shout, PersistedValue.Routing>(
        "shout", ObjectFamily.Routing, static reader => new(reader),
        static host => new PersistedValue.Routing(host.ClusterOutput, host.StreamData, host.StreamPath, host.StreamBackup),
        static (host, held, key) => Hosted.Bound(() => {
            host.ClusterOutput = held.ClusterOutput;
            host.StreamData = held.StreamData;
            host.StreamPath = held.StreamPath;
            host.StreamBackup = held.StreamBackup;
        }, key));
    public static readonly NativeKind Listen = Of<Grasshopper2.Parameters.Special.Listen, PersistedValue.Listener>(
        "listen", ObjectFamily.Routing, static reader => new(reader),
        static host => new PersistedValue.Listener(
            host.DependencyIndex, host.DependencyA, host.DependencyB, host.DependencyC,
            host.DependencyD, host.ClusterInput, host.ClusterIndex),
        static (host, held, key) => Hosted.Bound(() => {
            host.DependencyA = held.A;
            host.DependencyB = held.B;
            host.DependencyC = held.C;
            host.DependencyD = held.D;
            host.DependencyIndex = held.Index;
            host.ClusterInput = held.ClusterInput;
            host.ClusterIndex = held.ClusterIndex;
        }, key));
    public static readonly NativeKind Relay = Of<Grasshopper2.Parameters.Special.Relay, PersistedValue.Flag>(
        "relay", ObjectFamily.Routing, static reader => new(reader),
        static host => new PersistedValue.Flag(host.Frozen),
        static (host, held, key) => Hosted.Bound(() => { host.Frozen = held.Value; }, key));
    public static readonly NativeKind Cluster = Of<Grasshopper2.Components.Standard.Cluster, PersistedValue.Grouping>(
        "cluster", ObjectFamily.Composite, static reader => new(reader),
        static host => new PersistedValue.Grouping(host.LoopSolution, host.RelayMessages),
        static (host, held, key) => Hosted.Bound(() => {
            host.LoopSolution = held.LoopSolution;
            host.RelayMessages = held.RelayMessages;
        }, key));
    public static readonly NativeKind Chain = Inert<Grasshopper2.Components.Standard.Chain>(
        "chain", ObjectFamily.Composite, static reader => new(reader));
    public static readonly NativeKind Scribble = Of<Grasshopper2.SpecialObjects.ScribbleObject, PersistedValue.Annotation>(
        "scribble", ObjectFamily.Annotation, static reader => new(reader),
        static host => new PersistedValue.Annotation(
            host.Text, host.TextAngle, host.TextFont, host.TextStyle, host.TextColour, host.TextAlign),
        static (host, held, key) => Hosted.Bound(() => {
            host.TextAngle = held.Angle;
            host.TextFont = held.Font;
            host.TextStyle = held.Style;
            host.TextColour = held.Colour;
            host.TextAlign = held.Align;
            host.Text = held.Value;
        }, key));

    public ObjectFamily Family { get; }

    public Type Host { get; }

    [UseDelegateFromConstructor]
    public partial Grasshopper2.Doc.IDocumentObject Create();

    [UseDelegateFromConstructor]
    public partial Grasshopper2.Doc.IDocumentObject Rehydrate(GrasshopperIO.IReader reader);

    [UseDelegateFromConstructor]
    public partial PersistedValue Read(Grasshopper2.Doc.IDocumentObject host);

    [UseDelegateFromConstructor]
    public partial Fin<Grasshopper2.Doc.IDocumentObject> Mint(PersistedValue value, Op key);

    [UseDelegateFromConstructor]
    public partial Fin<Unit> Write(Grasshopper2.Doc.IDocumentObject host, PersistedValue value, Op key);

    private static readonly Lazy<FrozenDictionary<Type, NativeKind>> ByHost =
        new(static () => Items.ToFrozenDictionary(static row => row.Host), LazyThreadSafetyMode.ExecutionAndPublication);

    // A host subclass resolves onto the nearest catalogued ancestor, so a derived slider still reads and writes.
    public static Option<NativeKind> ForHost(Type? host) =>
        Ancestry(host)
            .Choose(static probe => ByHost.Value.TryGetValue(probe, out NativeKind? row) ? Optional(row) : None)
            .Head;

    private static Seq<Type> Ancestry(Type? host) => host is null ? Seq<Type>() : host.Cons(Ancestry(host.BaseType));

    // The parameterless constructor mints and the write column completes the state.
    private static NativeKind Of<THost, TValue>(
        string key, ObjectFamily family,
        Func<GrasshopperIO.IReader, THost> rehydrate,
        Func<THost, TValue> read,
        Func<THost, TValue, Op, Fin<Unit>> write)
        where THost : class, Grasshopper2.Doc.IDocumentObject, new()
        where TValue : PersistedValue =>
        new(key, family, typeof(THost),
            static () => (Grasshopper2.Doc.IDocumentObject)new THost(),
            reader => rehydrate(reader),
            host => read((THost)host),
            (value, op) => Hosted.Bound(static () => (Grasshopper2.Doc.IDocumentObject)new THost(), op)
                .Bind(host => Pair<THost, TValue>(host, value, key, op)
                    .Bind(pair => write(pair.Host, pair.Value, op))
                    .Map(_ => host)),
            (host, value, op) => Pair<THost, TValue>(host, value, key, op).Bind(pair => write(pair.Host, pair.Value, op)));

    // The seeding constructor carries the state no setter reaches; the write column completes the rest.
    private static NativeKind Seeded<THost, TValue>(
        string key, ObjectFamily family,
        Func<GrasshopperIO.IReader, THost> rehydrate,
        Func<THost, TValue> read,
        Func<string, TValue, THost> seed,
        Func<THost, TValue, Op, Fin<Unit>> write)
        where THost : class, Grasshopper2.Doc.IDocumentObject, new()
        where TValue : PersistedValue {
        Lazy<string> label = Label<THost>();
        return new(key, family, typeof(THost),
            static () => (Grasshopper2.Doc.IDocumentObject)new THost(),
            reader => rehydrate(reader),
            host => read((THost)host),
            (value, op) => Admitted<TValue>(value, key, op).Bind(held =>
                Hosted.Bound(() => (Grasshopper2.Doc.IDocumentObject)seed(label.Value, held), op)
                    .Bind(host => write((THost)host, held, op).Map(_ => host))),
            (host, value, op) => Pair<THost, TValue>(host, value, key, op).Bind(pair => write(pair.Host, pair.Value, op)));
    }

    // The seeding constructor IS the whole write, so assignment from outside the host assembly refuses.
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
            (value, op) => Admitted<TValue>(value, key, op).Bind(held =>
                Hosted.Bound(() => (Grasshopper2.Doc.IDocumentObject)seed(label.Value, held), op)),
            (_, value, op) => Fin.Fail<Unit>(new GhFault.Refused(op, $"{key}:sealed:{value.GetType().Name}")));
    }

    // The host publishes no readable or writable state, so the row reads Empty rather than falling to a refusal.
    private static NativeKind Inert<THost>(string key, ObjectFamily family, Func<GrasshopperIO.IReader, THost> rehydrate)
        where THost : class, Grasshopper2.Doc.IDocumentObject, new() =>
        Of<THost, PersistedValue.Empty>(key, family, rehydrate,
            static _ => new PersistedValue.Empty(),
            static (_, _, _) => Fin.Succ(unit));

    // One construction per row per process reads the host's OWN default user name, so a seeding constructor
    // never carries the catalog key into a canvas-visible name and no default is hand-copied.
    private static Lazy<string> Label<THost>() where THost : Grasshopper2.Doc.IDocumentObject, new() =>
        new(static () => new THost().UserName, LazyThreadSafetyMode.ExecutionAndPublication);

    private static Fin<TValue> Admitted<TValue>(PersistedValue value, string key, Op op) where TValue : PersistedValue =>
        value is TValue held
            ? Fin.Succ(held)
            : Fin.Fail<TValue>(new GhFault.Refused(op, $"{key}:{value.GetType().Name}"));

    private static Fin<(THost Host, TValue Value)> Pair<THost, TValue>(
        Grasshopper2.Doc.IDocumentObject host, PersistedValue value, string key, Op op)
        where THost : class, Grasshopper2.Doc.IDocumentObject
        where TValue : PersistedValue =>
        (host, value) switch {
            (THost typed, TValue held) => Fin.Succ((typed, held)),
            _ => Fin.Fail<(THost, TValue)>(new GhFault.Refused(op, $"{key}:{host.GetType().Name}:{value.GetType().Name}")),
        };
}
```

## [04]-[OBJECT_OPERATIONS]

- Owner: `NativeObject` is the one operation surface over the catalog — mint, archive rehydration and persistence, value read, value assignment, momentary pulse, list stepping, timer-target reconciliation, cluster construction and map resolution, and disentanglement. Every per-type correspondence lives on its `NativeKind` row, so this surface holds no type roster and grows by zero lines when a row lands.
- Entry: `Row(IDocumentObject)` is the one dispatch — `NativeKind.ForHost` over the live object's own ancestry — and an uncatalogued type is the single refusal every value-bearing entry shares.
- Entry: `Mint` is the row's own column: `Empty` selects the parameterless constructor and every other value enters the row's factory arm, so nothing re-enters a second write and no payload pairing survives outside its row's declaration.
- Entry: `Rehydrate(NativeKind, IReader)` and `Persist(IDocumentObject, IWriter)` close the archive round trip — the read leg dispatches the row's `Rehydrate` column and the write leg calls `IStorable.Store` on the interface, so a document object read out of `GrasshopperIO` and written back never leaves the catalog.
- Receipt: `Clustered` names the constructor's first `out Guid[][]` as the input mapping and the second as the output mapping; `Boundary` calls the public `void EnsureMaps(out Listen[], out Shout[])` and returns the resulting pin rosters without inventing a success probe.
- Auto: `Retarget` rejects the timer's own instance id before mutation, compares `TimerObject.TargetIds` with the distinct desired set, and admits each `AddTarget`/`RemoveTarget` only when its public boolean result confirms the mutation.
- Growth: a new interactive object is one catalog row; a host verb that pulses or steps rather than sets is one delegate vocabulary beside `Assign` (`ButtonPulse`, `ListStep`), because a verb has no persisted value to carry.
- Boundary: `NumberSliderObject.InternalNumber` and `NumberPickerObject.InternalNumber` are read-only and seed only through their public constructors; the slider preserves its complete non-null `UiNumber`, while the picker carries its public `decimal` read through the host's lossy `double` constructor boundary. `ComplexPickerObject.Values` and `TemporalPickerObject.Date` carry `internal set`, so both rows are `Sealed` and their `Write` refuses rather than silently succeeding. `PresetPickerObject.UserNames`, including its `null` state, owns persisted selection; assignment expires and restarts a solution only when that state changes. `ValueListObject.ItemCount` and `Items` are read-only, so a selection assigns index by index through `SelectItem`/`DeselectItem` under the list's own `Mode` law, and `SelectPrev`/`SelectNext` are cyclic steps carried by `Step`, never a selection set. `DataRecorderObject.Paused` is the recorder's writable state, and its `IsEmpty` answers true when recorded buckets exist — the member name and XML summary invert the installed behavior — so a presence probe reads a true `IsEmpty` as data-present evidence. `DataPanelObject.ChangeDisplay` writes the six display flags in one host call and `VerticalOffset` writes beside it. `FunctionEditorObject`, `QuickGraphObject`, and `Chain` publish no readable or writable value state, so they take the `Inert` arm and read as `Empty`. `Boundary` rejects null pins from the oblivious host arrays. Incomplete host chain ordering and validation members, the internal loop driver, and private `ImageSamplerObject.SampleContinuous` never enter this operation surface.

```csharp signature
// --- [TYPES] -----------------------------------------------------------------------------

[SmartEnum]
public sealed partial class ListStep {
    public static readonly ListStep Previous = new(static list => list.SelectPrev());
    public static readonly ListStep Next = new(static list => list.SelectNext());

    [UseDelegateFromConstructor]
    public partial void Advance(Grasshopper2.Parameters.Special.ValueListObject list);
}

// A press is a momentary verb with no persisted state, so it rides its own vocabulary rather than a Flag
// payload the button's Momentary value would collide with.
[SmartEnum]
public sealed partial class ButtonPulse {
    public static readonly ButtonPulse Press = new(static button => button.Press());
    public static readonly ButtonPulse Release = new(static button => button.Release());

    [UseDelegateFromConstructor]
    public partial void Drive(Grasshopper2.Parameters.Special.ButtonObject button);
}

// --- [OPERATIONS] ------------------------------------------------------------------------

public static class NativeObject {
    public static Fin<Grasshopper2.Doc.IDocumentObject> Mint(NativeKind? kind, PersistedValue? seed, Op? key = null) =>
        (kind, seed) switch {
            (null, _) or (_, null) => Fin.Fail<Grasshopper2.Doc.IDocumentObject>(
                new GhFault.Refused(key.OrDefault(), nameof(Mint))),
            (var row, PersistedValue.Empty) => Hosted.Bound(row.Create, key.OrDefault()),
            var (row, value) => row.Mint(value, key.OrDefault()),
        };

    public static Fin<Grasshopper2.Doc.IDocumentObject> Rehydrate(
        NativeKind? kind, GrasshopperIO.IReader? reader, Op? key = null) =>
        (kind, reader) switch {
            (null, _) or (_, null) => Fin.Fail<Grasshopper2.Doc.IDocumentObject>(
                new GhFault.Refused(key.OrDefault(), nameof(Rehydrate))),
            var (row, source) => Hosted.Bound(() => row.Rehydrate(source), key.OrDefault()),
        };

    public static Fin<Unit> Persist(
        Grasshopper2.Doc.IDocumentObject? host, GrasshopperIO.IWriter? writer, Op? key = null) =>
        (host, writer) switch {
            (null, _) or (_, null) => Fin.Fail<Unit>(new GhFault.Refused(key.OrDefault(), nameof(Persist))),
            var (target, sink) => Hosted.Bound(() => target.Store(sink), key.OrDefault()),
        };

    public static Fin<PersistedValue> ValueOf(Grasshopper2.Doc.IDocumentObject? host, Op? key = null) =>
        Row(host, key.OrDefault(), nameof(ValueOf))
            .Bind(pair => Hosted.Bound(() => pair.Row.Read(pair.Host), key.OrDefault()));

    public static Fin<Unit> Assign(Grasshopper2.Doc.IDocumentObject? host, PersistedValue? value, Op? key = null) =>
        value is null
            ? Fin.Fail<Unit>(new GhFault.Refused(key.OrDefault(), nameof(Assign)))
            : Row(host, key.OrDefault(), nameof(Assign))
                .Bind(pair => pair.Row.Write(pair.Host, value, key.OrDefault()));

    public static Fin<Unit> Step(
        Grasshopper2.Parameters.Special.ValueListObject? list, ListStep step, Op? key = null) =>
        (list, step) switch {
            (null, _) or (_, null) => Fin.Fail<Unit>(new GhFault.Refused(key.OrDefault(), nameof(Step))),
            var (live, row) => Hosted.Bound(() => row.Advance(live), key.OrDefault()),
        };

    public static Fin<Unit> Pulse(
        Grasshopper2.Parameters.Special.ButtonObject? button, ButtonPulse pulse, Op? key = null) =>
        (button, pulse) switch {
            (null, _) or (_, null) => Fin.Fail<Unit>(new GhFault.Refused(key.OrDefault(), nameof(Pulse))),
            var (live, row) => Hosted.Bound(() => row.Drive(live), key.OrDefault()),
        };

    private static Fin<(NativeKind Row, Grasshopper2.Doc.IDocumentObject Host)> Row(
        Grasshopper2.Doc.IDocumentObject? host, Op key, string verb) =>
        host is null
            ? Fin.Fail<(NativeKind, Grasshopper2.Doc.IDocumentObject)>(new GhFault.Refused(key, verb))
            : NativeKind.ForHost(host.GetType())
                .Map(row => (row, host))
                .ToFin(new GhFault.Refused(key, $"{verb}:{host.GetType().Name}"));

    public static Fin<Unit> Retarget(Grasshopper2.Parameters.Special.TimerObject? timer, Seq<Guid> desired, Op? key = null) =>
        timer is null || desired.Contains(timer.InstanceId)
            ? Fin.Fail<Unit>(new GhFault.Refused(key.OrDefault(), nameof(Retarget)))
            : Reconcile(timer, toSeq(desired.Distinct()), key.OrDefault());

    private static Fin<Unit> Reconcile(Grasshopper2.Parameters.Special.TimerObject timer, Seq<Guid> desired, Op key) =>
        Hosted.Bound(() => toSeq(timer.TargetIds), key)
            .Bind(current => current.Filter(id => !desired.Contains(id))
                .Map(id => Target(timer, id, add: false, key))
                .TraverseM(identity)
                .As()
                .Bind(_ => desired.Filter(id => !timer.IsTarget(id))
                    .Map(id => Target(timer, id, add: true, key))
                    .TraverseM(identity)
                    .As())
                .Map(static _ => unit));

    public static Fin<(Grasshopper2.Components.Standard.Cluster Cluster, Guid[][] InputMapping, Guid[][] OutputMapping)> Clustered(
        Grasshopper2.Doc.IDocumentObject[] members, Op? key = null) =>
        Hosted.Bound(() => {
            Grasshopper2.Components.Standard.Cluster cluster = new(
                members, out Guid[][] inputMapping, out Guid[][] outputMapping);
            return (cluster, inputMapping, outputMapping);
        }, key.OrDefault());

    public static Fin<(Seq<Grasshopper2.Parameters.Special.Listen> Inputs, Seq<Grasshopper2.Parameters.Special.Shout> Outputs)> Boundary(
        Grasshopper2.Components.Standard.Cluster cluster, Op? key = null) =>
        Hosted.Bound(() => {
            cluster.EnsureMaps(
                out Grasshopper2.Parameters.Special.Listen[] listeners,
                out Grasshopper2.Parameters.Special.Shout[] shouters);
            return listeners is null ||
                   shouters is null ||
                   listeners.Any(static item => item is null) ||
                   shouters.Any(static item => item is null)
                ? Fin.Fail<(Seq<Grasshopper2.Parameters.Special.Listen>, Seq<Grasshopper2.Parameters.Special.Shout>)>(
                    new GhFault.Refused(key.OrDefault(), nameof(Boundary)))
                : Fin.Succ((toSeq(listeners), toSeq(shouters)));
        }, key.OrDefault())
            .Bind(identity);

    public static Fin<Unit> Disentangle(Grasshopper2.Components.Standard.Cluster cluster, Grasshopper2.Undo.ActionList actions, Op? key = null) =>
        Hosted.Bound(() => cluster.Disentangle(actions), key.OrDefault());

    private static Fin<Unit> Target(
        Grasshopper2.Parameters.Special.TimerObject timer, Guid id, bool add, Op key) =>
        Hosted.Bound(() => add ? timer.AddTarget(id) : timer.RemoveTarget(id), key)
            .Bind(changed => changed
                ? Fin.Succ(unit)
                : Fin.Fail<Unit>(new GhFault.Refused(key, $"{nameof(Target)}:{(add ? "add" : "remove")}:{id}")));

    internal static Fin<Unit> Select(
        Grasshopper2.Parameters.Special.PresetPickerObject picker,
        PersistedValue.Selection desired,
        Op key) =>
        Hosted.Bound(() => {
            picker.MultiSelect = desired.MultiSelect;
            picker.Scroll0 = desired.Scroll0;
            picker.Scroll1 = desired.Scroll1;
            string[]? names = desired.UserNames.Match(
                Some: static values => values.ToArray(),
                None: static () => (string[]?)null);
            if ((picker.UserNames is null && names is null) ||
                (picker.UserNames is { } current && names is not null && current.SequenceEqual(names))) {
                return;
            }
            picker.UserNames = names;
            picker.Expire();
            picker.Document?.Solution.Start();
        }, key);

    // Mode leads because SelectItem dispatches on it: PickAll admits a set while ShowOne and ShowAll
    // collapse every selection to the last index the fold touches.
    internal static Fin<Unit> Reselect(
        Grasshopper2.Parameters.Special.ValueListObject list,
        PersistedValue.Listing desired,
        Op key) =>
        Hosted.Bound(() => { list.Mode = desired.Mode; }, key)
            .Bind(_ => Hosted.Bound(() => toSeq(Enumerable.Range(0, list.ItemCount)), key))
            .Bind(indexes => indexes
                .Map(index => Hosted.Bound(() => {
                    if (desired.Selected.Contains(index) == list.ItemSelected(index)) { return; }
                    if (desired.Selected.Contains(index)) { list.SelectItem(index); } else { list.DeselectItem(index); }
                }, key))
                .TraverseM(identity)
                .As()
                .Map(static _ => unit));
}
```

## [05]-[GH1_BOUNDARY]

- Owner: `Gh1Host` is the one legacy admission — a non-null `Grasshopper2.Interop.IGH_Component` wraps into the runtime-backed host component and returns beside a typed receipt carrying the source identity and XML, so provenance survives the crossing.
- Receipt: `Gh1Receipt` holds the legacy id, name, and source XML for round-trip evidence.
- Boundary: the host requires a loadable GH1 runtime during processing. Wrapper conversion allocates the ETO bitmap retained by `GH1InteropComponent`; the receipt neither exposes nor disposes that component-retained icon.

```csharp signature
// --- [BOUNDARIES] ------------------------------------------------------------------------

public sealed record Gh1Receipt(Guid SourceId, string SourceName, string SourceXml);

public static class Gh1Host {
    public static Fin<(Grasshopper2.Components.Standard.GH1InteropComponent Host, Gh1Receipt Receipt)> Admit(
        Grasshopper2.Interop.IGH_Component? legacy, Op? key = null) =>
        legacy is null
            ? Fin.Fail<(Grasshopper2.Components.Standard.GH1InteropComponent, Gh1Receipt)>(
                new GhFault.Refused(key.OrDefault(), nameof(Admit)))
            : Hosted.Bound(() => new Grasshopper2.Components.Standard.GH1InteropComponent(legacy), key.OrDefault())
                .Map(host => (host, new Gh1Receipt(host.Grasshopper1Id, host.Grasshopper1Name, host.Grasshopper1Xml)));
}
```

## [06]-[RESEARCH]

<!-- source-only: research row template:
[TOKEN]-[OPEN|BLOCKED]: <exact question>; <verification route>.
[SPLIT_MEMBER]-[OPEN]: does `shape-core` expose `split_all`; verify against the member rail.
-->

(none)
