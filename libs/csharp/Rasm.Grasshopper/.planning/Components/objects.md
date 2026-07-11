# [RASM_GRASSHOPPER_OBJECTS]

`NativeObject` is the native document-object catalog: the interactive `Grasshopper2.Parameters.Special` family, the `Shout`/`Listen`/`Relay` routing pins, and the `Cluster`/`Chain`/`Loop` composite-iterative family land as rows of one `NativeKind` catalog, their persistent values as cases of one `PersistedValue` union, and every loop, boundary, and chain discriminant as a generated vocabulary over the host smart enums. One polymorphic owner mints, reads, and assigns values, resolves cluster boundaries, validates chains, and drives loops onto the rail; GH1 interop is one explicit import boundary row that returns a typed receipt beside the wrapped component.

## [01]-[INDEX]

- [02]-[CONTROL_VOCABULARY]: the object families and the loop, boundary, and accumulation vocabularies over the host discriminants
- [03]-[VALUE_AND_CATALOG]: the `PersistedValue` union and the `NativeKind` object-row catalog
- [04]-[OBJECT_OPERATIONS]: the one owner over mint, value, boundary, chain, loop, sampling, and expiry
- [05]-[GH1_BOUNDARY]: the one-way import row and its receipt
- [06]-[RESEARCH]

## [02]-[CONTROL_VOCABULARY]

- Owner: six keyless `[SmartEnum]` vocabularies close the object-family and loop-control discriminants; each control row carries its host value as a column with an `Of` reverse projection, so folder dispatch is exhaustive while host values cross only at member calls.
- Cases: `ObjectFamily` partitions the catalog; `BreakTiming`, `StepAction`, `RepeatBound`, `AccumulationMode`, and `BoundaryRole` mirror `LoopContinuation`, `LoopingAction`, `LoopRepeats`, `Accumulation`, and `ClusterBoundary`.
- Entry: `BreakTiming.Of(LoopContinuation)` is the one continuation read the loop drive folds on; `Halts` is its behavior column.
- Growth: a new host discriminant value is one row on the owning vocabulary.
- Boundary: `BreakBefore` and `BreakAfter` both halt the drive — the before/after distinction stays the host's own iteration semantics, read after each push.

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
    public static readonly ObjectFamily Iterative = new();
    public static readonly ObjectFamily Interop = new();
}

[SmartEnum]
public sealed partial class BreakTiming {
    public static readonly BreakTiming Flowing = new(Grasshopper2.Components.Standard.LoopContinuation.Continue, halts: false);
    public static readonly BreakTiming Before = new(Grasshopper2.Components.Standard.LoopContinuation.BreakBefore, halts: true);
    public static readonly BreakTiming After = new(Grasshopper2.Components.Standard.LoopContinuation.BreakAfter, halts: true);

    public Grasshopper2.Components.Standard.LoopContinuation Host { get; }

    public bool Halts { get; }

    public static BreakTiming Of(Grasshopper2.Components.Standard.LoopContinuation host) =>
        Items.Find(row => row.Host == host).IfNone(Flowing);
}

[SmartEnum]
public sealed partial class StepAction {
    public static readonly StepAction Advance = new(Grasshopper2.Components.Standard.LoopingAction.Continue);
    public static readonly StepAction Halt = new(Grasshopper2.Components.Standard.LoopingAction.Break);

    public Grasshopper2.Components.Standard.LoopingAction Host { get; }
}

[SmartEnum]
public sealed partial class RepeatBound {
    public static readonly RepeatBound Exact = new(Grasshopper2.Components.Standard.LoopRepeats.Exact);
    public static readonly RepeatBound Lower = new(Grasshopper2.Components.Standard.LoopRepeats.Lower);
    public static readonly RepeatBound Upper = new(Grasshopper2.Components.Standard.LoopRepeats.Upper);

    public Grasshopper2.Components.Standard.LoopRepeats Host { get; }
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

- Owner: `PersistedValue` is the one persistent-value union every interactive object reads and assigns through — a number, a flag, a momentary pair, text, a parsed source, a colour, a selection, and an expiry-target set are cases of one carrier; `NativeKind` is the object-row catalog carrying family, host type, and mintability per row.
- Entry: `NativeKind.ForHost(Type)` is the `Items`-derived frozen index dispatching a live document object onto its row.
- Packages: every host type column is a verified `Grasshopper2.Parameters.Special` or `Grasshopper2.Components.Standard` type; canvas control state stays each object's own `CreateAttributes` projection.
- Growth: a new interactive object is one catalog row plus, where its value shape is new, one `PersistedValue` case.
- Boundary: rows with `Mintable = false` place through the host canvas, and their values still read and assign through the one owner.

```csharp signature
// --- [MODELS] ----------------------------------------------------------------------------

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record PersistedValue {
    private PersistedValue() { }

    public sealed record Number(double Value) : PersistedValue;
    public sealed record Flag(bool Value) : PersistedValue;
    public sealed record Momentary(IPear Up, IPear Down) : PersistedValue;
    public sealed record Text(string Value) : PersistedValue;
    public sealed record Parsed(string Source) : PersistedValue;
    public sealed record Swatch(Grasshopper2.Types.Colour.Colour Value, bool Apply) : PersistedValue;
    public sealed record Selection(Seq<string> Names) : PersistedValue;
    public sealed record Targets(Seq<Guid> Ids) : PersistedValue;
}

// --- [SERVICES] --------------------------------------------------------------------------

[SmartEnum<string>]
public sealed partial class NativeKind {
    public static readonly NativeKind NumberSlider = new("numberSlider", ObjectFamily.ValueInput, typeof(Grasshopper2.Parameters.Special.NumberSliderObject), mintable: false);
    public static readonly NativeKind NumberPicker = new("numberPicker", ObjectFamily.ValueInput, typeof(Grasshopper2.Parameters.Special.NumberPickerObject), mintable: true);
    public static readonly NativeKind Toggle = new("toggle", ObjectFamily.ValueInput, typeof(Grasshopper2.Parameters.Special.ToggleObject), mintable: true);
    public static readonly NativeKind Button = new("button", ObjectFamily.ValueInput, typeof(Grasshopper2.Parameters.Special.ButtonObject), mintable: true);
    public static readonly NativeKind Value = new("value", ObjectFamily.ValueInput, typeof(Grasshopper2.Parameters.Special.ValueObject), mintable: false);
    public static readonly NativeKind TextInput = new("textInput", ObjectFamily.ValueInput, typeof(Grasshopper2.Parameters.Special.TextInputObject), mintable: false);
    public static readonly NativeKind ColourSwatch = new("colourSwatch", ObjectFamily.ValueInput, typeof(Grasshopper2.Parameters.Special.ColourSwatchObject), mintable: false);
    public static readonly NativeKind GradientEditor = new("gradientEditor", ObjectFamily.Editor, typeof(Grasshopper2.Parameters.Special.GradientEditorObject), mintable: false);
    public static readonly NativeKind FunctionEditor = new("functionEditor", ObjectFamily.Editor, typeof(Grasshopper2.Parameters.Special.FunctionEditorObject), mintable: false);
    public static readonly NativeKind MaterialEditor = new("materialEditor", ObjectFamily.Editor, typeof(Grasshopper2.Parameters.Special.MaterialEditorObject), mintable: false);
    public static readonly NativeKind Histogram = new("histogram", ObjectFamily.Editor, typeof(Grasshopper2.Parameters.Special.HistogramObject), mintable: false);
    public static readonly NativeKind QuickGraph = new("quickGraph", ObjectFamily.Editor, typeof(Grasshopper2.Parameters.Special.QuickGraphObject), mintable: false);
    public static readonly NativeKind Protractor = new("protractor", ObjectFamily.Editor, typeof(Grasshopper2.Parameters.Special.ProtractorObject), mintable: false);
    public static readonly NativeKind ImageSampler = new("imageSampler", ObjectFamily.Sampler, typeof(Grasshopper2.Parameters.Special.ImageSamplerObject), mintable: false);
    public static readonly NativeKind PresetPicker = new("presetPicker", ObjectFamily.Picker, typeof(Grasshopper2.Parameters.Special.PresetPickerObject), mintable: true);
    public static readonly NativeKind ComplexPicker = new("complexPicker", ObjectFamily.Picker, typeof(Grasshopper2.Parameters.Special.ComplexPickerObject), mintable: false);
    public static readonly NativeKind ConstantPicker = new("constantPicker", ObjectFamily.Picker, typeof(Grasshopper2.Parameters.Special.ConstantPickerObject), mintable: false);
    public static readonly NativeKind MetaNamePicker = new("metaNamePicker", ObjectFamily.Picker, typeof(Grasshopper2.Parameters.Special.MetaNamePickerObject), mintable: false);
    public static readonly NativeKind TemporalPicker = new("temporalPicker", ObjectFamily.Picker, typeof(Grasshopper2.Parameters.Special.TemporalPickerObject), mintable: false);
    public static readonly NativeKind ValueList = new("valueList", ObjectFamily.List, typeof(Grasshopper2.Parameters.Special.ValueListObject), mintable: false);
    public static readonly NativeKind PathMapper = new("pathMapper", ObjectFamily.Data, typeof(Grasshopper2.Parameters.Special.PathMapperObject), mintable: false);
    public static readonly NativeKind DataPanel = new("dataPanel", ObjectFamily.Data, typeof(Grasshopper2.Parameters.Special.DataPanelObject), mintable: false);
    public static readonly NativeKind DataRecorder = new("dataRecorder", ObjectFamily.Data, typeof(Grasshopper2.Parameters.Special.DataRecorderObject), mintable: false);
    public static readonly NativeKind TreeViewer = new("treeViewer", ObjectFamily.Data, typeof(Grasshopper2.Parameters.Special.TreeViewerObject), mintable: false);
    public static readonly NativeKind Timer = new("timer", ObjectFamily.Utility, typeof(Grasshopper2.Parameters.Special.TimerObject), mintable: false);
    public static readonly NativeKind Shout = new("shout", ObjectFamily.Routing, typeof(Grasshopper2.Parameters.Special.Shout), mintable: false);
    public static readonly NativeKind Listen = new("listen", ObjectFamily.Routing, typeof(Grasshopper2.Parameters.Special.Listen), mintable: false);
    public static readonly NativeKind Relay = new("relay", ObjectFamily.Routing, typeof(Grasshopper2.Parameters.Special.Relay), mintable: false);
    public static readonly NativeKind Cluster = new("cluster", ObjectFamily.Composite, typeof(Grasshopper2.Components.Standard.Cluster), mintable: true);
    public static readonly NativeKind Chain = new("chain", ObjectFamily.Composite, typeof(Grasshopper2.Components.Standard.Chain), mintable: true);
    public static readonly NativeKind Loop = new("loop", ObjectFamily.Iterative, typeof(Grasshopper2.Components.Standard.Loop), mintable: false);
    public static readonly NativeKind Gh1 = new("gh1", ObjectFamily.Interop, typeof(Grasshopper2.Components.Standard.GH1InteropComponent), mintable: true);

    public ObjectFamily Family { get; }

    public Type Host { get; }

    public bool Mintable { get; }

    private static readonly Lazy<FrozenDictionary<Type, NativeKind>> ByHost =
        new(static () => Items.ToFrozenDictionary(static row => row.Host));

    public static Option<NativeKind> ForHost(Type host) =>
        ByHost.Value.TryGetValue(host, out NativeKind? row) ? Optional(row) : None;
}
```

## [04]-[OBJECT_OPERATIONS]

- Owner: `NativeObject` is the one operation surface over the catalog — mint, value read, value assignment, momentary pulse, expiry retargeting, cluster boundary resolution, disentanglement, chain validation, loop drive, and bitmap sampling dispatch through joint patterns over (row, value) and the live host object's own type.
- Entry: every entry is polymorphic on its host argument's runtime shape; the `var` floor arms are the open host-object space, refused with the row key in evidence.
- Receipt: the loop drive seals a `LoopReceipt` carrying steps run, cyclicality, and properness after `Flush`; accumulated trees stay host-read through `AccumulatedTrees`.
- Auto: `Retarget` reconciles the timer's target set — absent ids add, surplus ids remove — so expiry membership is declarative.
- Growth: a mintable row is one `Mint` arm; a new value correspondence is one `ValueOf`/`Assign` arm pair.
- Boundary: the loop drive's sequential push protocol is the named boundary-kernel statement seam; every host call crosses `data#FAULT_AND_NOTICE` `Hosted.Bound`.

```csharp signature
// --- [MODELS] ----------------------------------------------------------------------------

public sealed record LoopPolicy(Func<int, Fin<Grasshopper2.Components.Standard.LoopingIteration>> Step);

public sealed record LoopReceipt(int Steps, bool Cyclical, bool Proper);

// --- [OPERATIONS] ------------------------------------------------------------------------

public static class NativeObject {
    public static Fin<Grasshopper2.Doc.IDocumentObject> Mint(NativeKind kind, PersistedValue seed, Op? key = null) =>
        (kind, seed) switch {
            var (row, value) when row == NativeKind.Toggle && value is PersistedValue.Flag flag =>
                Hosted.Bound(() => (Grasshopper2.Doc.IDocumentObject)new Grasshopper2.Parameters.Special.ToggleObject(flag.Value), key.OrDefault()),
            var (row, value) when row == NativeKind.Button && value is PersistedValue.Momentary pair =>
                Hosted.Bound(() => (Grasshopper2.Doc.IDocumentObject)new Grasshopper2.Parameters.Special.ButtonObject(pair.Up, pair.Down), key.OrDefault()),
            var (row, value) when row == NativeKind.NumberPicker && value is PersistedValue.Number number =>
                Hosted.Bound(() => (Grasshopper2.Doc.IDocumentObject)new Grasshopper2.Parameters.Special.NumberPickerObject(nameof(NativeKind.NumberPicker), number.Value), key.OrDefault()),
            var (row, value) when row == NativeKind.PresetPicker && value is PersistedValue.Selection { Names.IsEmpty: true } =>
                Hosted.Bound(() => (Grasshopper2.Doc.IDocumentObject)new Grasshopper2.Parameters.Special.PresetPickerObject(nameof(NativeKind.PresetPicker)), key.OrDefault()),
            var (row, _) => Fin.Fail<Grasshopper2.Doc.IDocumentObject>(new GhFault.Refused(key.OrDefault(), $"{nameof(Mint)}:{row.Key}")),
        };

    public static Fin<PersistedValue> ValueOf(Grasshopper2.Doc.IDocumentObject host, Op? key = null) =>
        host switch {
            Grasshopper2.Parameters.Special.NumberSliderObject slider =>
                Hosted.Bound(() => (PersistedValue)new PersistedValue.Number(slider.InternalNumber), key.OrDefault()),
            Grasshopper2.Parameters.Special.ToggleObject toggle =>
                Hosted.Bound(() => (PersistedValue)new PersistedValue.Flag(toggle.ToggleState), key.OrDefault()),
            Grasshopper2.Parameters.Special.NumberPickerObject picker =>
                Hosted.Bound(() => (PersistedValue)new PersistedValue.Number(picker.InternalNumber), key.OrDefault()),
            Grasshopper2.Parameters.Special.ValueObject value =>
                Hosted.Bound(() => (PersistedValue)new PersistedValue.Parsed(value.Text), key.OrDefault()),
            Grasshopper2.Parameters.Special.ColourSwatchObject swatch =>
                Hosted.Bound(() => (PersistedValue)new PersistedValue.Swatch(swatch.Colour, Apply: false), key.OrDefault()),
            Grasshopper2.Parameters.Special.PresetPickerObject picker =>
                Hosted.Bound(() => (PersistedValue)new PersistedValue.Selection(toSeq(picker.SelectedNames)), key.OrDefault()),
            Grasshopper2.Parameters.Special.TimerObject timer =>
                Hosted.Bound(() => (PersistedValue)new PersistedValue.Targets(toSeq(timer.Targets)), key.OrDefault()),
            var unknown => Fin.Fail<PersistedValue>(new GhFault.Refused(key.OrDefault(), $"{nameof(ValueOf)}:{unknown.GetType().Name}")),
        };

    public static Fin<Unit> Assign(Grasshopper2.Doc.IDocumentObject host, PersistedValue value, Op? key = null) =>
        (host, value) switch {
            (Grasshopper2.Parameters.Special.ToggleObject toggle, PersistedValue.Flag flag) =>
                Hosted.Bound(() => { toggle.ToggleState = flag.Value; }, key.OrDefault()),
            (Grasshopper2.Parameters.Special.NumberPickerObject picker, PersistedValue.Number number) =>
                Hosted.Bound(() => { picker.InternalNumber = number.Value; }, key.OrDefault()),
            (Grasshopper2.Parameters.Special.ValueObject target, PersistedValue.Parsed parsed) =>
                Hosted.Bound(() => target.AssignTextAndValue(parsed.Source), key.OrDefault()),
            (Grasshopper2.Parameters.Special.ColourSwatchObject swatch, PersistedValue.Swatch colour) =>
                Hosted.Bound(() => swatch.SetColour(colour.Value, colour.Apply), key.OrDefault()),
            (Grasshopper2.Parameters.Special.ButtonObject button, PersistedValue.Flag flag) =>
                Hosted.Bound(() => { if (flag.Value) { button.Press(); } else { button.Release(); } }, key.OrDefault()),
            (Grasshopper2.Parameters.Special.TimerObject timer, PersistedValue.Targets targets) => Retarget(timer, targets.Ids, key.OrDefault()),
            var (unknown, _) => Fin.Fail<Unit>(new GhFault.Refused(key.OrDefault(), $"{nameof(Assign)}:{unknown.GetType().Name}")),
        };

    public static Fin<Unit> Retarget(Grasshopper2.Parameters.Special.TimerObject timer, Seq<Guid> desired, Op? key = null) =>
        Hosted.Bound(() => toSeq(timer.Targets), key.OrDefault())
            .Bind(current => current.Filter(id => !desired.Contains(id))
                .Map(id => Hosted.Bound(() => timer.RemoveTarget(id), key.OrDefault()))
                .TraverseM(identity)
                .As()
                .Bind(_ => desired.Filter(id => !timer.IsTarget(id))
                    .Map(id => Hosted.Bound(() => timer.AddTarget(id), key.OrDefault()))
                    .TraverseM(identity)
                    .As())
                .Map(static _ => unit));

    public static Fin<(Grasshopper2.Components.Standard.Cluster Cluster, Guid[][] Inner, Guid[][] Outer)> Clustered(
        Grasshopper2.Doc.IDocumentObject[] members, Op? key = null) =>
        Hosted.Bound(() => {
            Grasshopper2.Components.Standard.Cluster cluster = new(members, out Guid[][] inner, out Guid[][] outer);
            return (cluster, inner, outer);
        }, key.OrDefault());

    public static Fin<(Seq<Grasshopper2.Parameters.Special.Listen> Inputs, Seq<Grasshopper2.Parameters.Special.Shout> Outputs)> Boundary(
        Grasshopper2.Components.Standard.Cluster cluster, Op? key = null) =>
        Hosted.Bound(() => cluster.EnsureMaps(out Grasshopper2.Parameters.Special.Listen[] listeners, out Grasshopper2.Parameters.Special.Shout[] shouters)
                ? Fin.Succ((toSeq(listeners), toSeq(shouters)))
                : Fin.Fail<(Seq<Grasshopper2.Parameters.Special.Listen>, Seq<Grasshopper2.Parameters.Special.Shout>)>(
                    new GhFault.Refused(key.OrDefault(), nameof(Boundary))), key.OrDefault())
            .Bind(identity);

    public static Fin<Unit> Disentangle(Grasshopper2.Components.Standard.Cluster cluster, Grasshopper2.Undo.ActionList actions, Op? key = null) =>
        Hosted.Bound(() => cluster.Disentangle(actions), key.OrDefault());

    public static Validation<Error, Grasshopper2.Components.Standard.Chain> Chained(Grasshopper2.Doc.IDocumentObject[] run, Op? key = null) =>
        Hosted.Bound(() => Grasshopper2.Components.Standard.Chain.ValidateChain(
                run, out Grasshopper2.Doc.IDocumentObject leading, out Grasshopper2.Doc.IDocumentObject trailing, out string reason)
                ? Fin.Succ(new Grasshopper2.Components.Standard.Chain(leading.InstanceId, trailing.InstanceId, run))
                : Fin.Fail<Grasshopper2.Components.Standard.Chain>(new GhFault.Refused(key.OrDefault(), reason)), key.OrDefault())
            .Bind(identity)
            .ToValidation();

    public static Fin<LoopReceipt> Drive(Grasshopper2.Components.Standard.Loop loop, LoopPolicy policy, Op? key = null) =>
        Hosted.Bound(() => loop.ResolveRepetition(), key.OrDefault())
            .Bind(repeats => Driven(loop, policy, repeats, key.OrDefault()))
            .Bind(steps => Hosted.Bound(() => loop.Flush(), key.OrDefault()).Map(_ => steps))
            .Map(steps => new LoopReceipt(steps, loop.IsCyclical, loop.IsProperLoop));

    public static Fin<float> Sample(
        Grasshopper2.Parameters.Special.ImageSamplerObject sampler, Eto.Drawing.BitmapData data, int width, int height,
        float u, float v, Grasshopper2.Parameters.Special.ImageSamplerObject.SamplingLimit limit, Op? key = null) =>
        Hosted.Bound(() => sampler.SampleContinuous(data, width, height, u, v, limit), key.OrDefault());

    private static Fin<int> Driven(Grasshopper2.Components.Standard.Loop loop, LoopPolicy policy, int repeats, Op key) {
        for (int step = 0; step < repeats; step++) {
            Fin<Unit> pushed = policy.Step(step).Bind(iteration => Hosted.Bound(() => loop.Push(iteration), key));
            if (pushed.IsFail) {
                return pushed.Map(static _ => 0);
            }
            if (BreakTiming.Of(loop.Continuation).Halts) {
                return Fin.Succ(step + 1);
            }
        }
        return Fin.Succ(repeats);
    }
}
```

## [05]-[GH1_BOUNDARY]

- Owner: `Gh1Import` is the one legacy admission — a `Grasshopper2.Interop.IGH_Component` wraps into the host interop shim and returns beside a typed receipt carrying the source identity and XML, so provenance survives the crossing.
- Receipt: `Gh1Receipt` holds the legacy id, name, and source XML for round-trip evidence.
- Boundary: no other page names a GH1 surface — a `Grasshopper.Kernel` idiom anywhere else in the folder is a defect, and the import lands a GH2 `Component` only.

```csharp signature
// --- [BOUNDARIES] ------------------------------------------------------------------------

public sealed record Gh1Receipt(Guid SourceId, string SourceName, string SourceXml);

public static class Gh1Import {
    public static Fin<(Component Component, Gh1Receipt Receipt)> Admit(Grasshopper2.Interop.IGH_Component legacy, Op? key = null) =>
        Hosted.Bound(() => new Grasshopper2.Components.Standard.GH1InteropComponent(legacy), key.OrDefault())
            .Map(shim => (
                (Component)shim,
                new Gh1Receipt(shim.Grasshopper1Id, shim.Grasshopper1Name, shim.Grasshopper1Xml)));
}
```

## [06]-[RESEARCH]

- [SPECIAL_CTORS]-[OPEN]: the constructor arities behind the `Mintable = false` rows — `NumberSliderObject(string, UiNumber)`, `ValueObject(string, string, Notation)`, `GradientEditorObject(string, GripGradient)`, and the editor, picker, list, data, timer, and routing objects; verify `UiNumber`, `Notation`, and `GripGradient` namespaces through the decompile rail, then flip rows mintable with their `Mint` arms.
- [VALUE_MEMBERS]-[OPEN]: writability of `ToggleObject.ToggleState` and the picker/slider `InternalNumber` pair, the `TextInputObject` text member, the `Relay.Frozen` assignment contract, the `PresetPickerObject` selection setter behind read-only `SelectedNames` — verified, it flips the mint arm's empty-selection refusal into seed application and lands the `Assign` selection arm — and the `ValueListObject.Set(ValueListItem[], bool)` item namespace.
- [CLUSTER_MAPS]-[OPEN]: which of the two `Cluster` constructor out-arrays is the inner map and which the outer; verify against the host XML sidecar.
- [LOOP_DRIVE_SHAPES]-[OPEN]: the `ResolveRepetition` call shape, the `LoopingIteration` constructor, the `AccumulatedTrees` carrier type, and the `SampleContinuous` return type; the drive and sampler assume `int`, caller-built iterations, and `float` until the decompile census lands.
- [ENSUREMAPS_RETURN]-[OPEN]: whether `Cluster.EnsureMaps` and `Chain.ValidateChain` return `bool` probes and whether `ValidateChain` is static; the folds assume both.
