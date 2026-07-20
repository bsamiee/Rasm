# [RASM_GRASSHOPPER_OBJECTS]

`NativeObject` is the native document-object catalog: the interactive `Grasshopper2.Parameters.Special` family, the `Shout`/`Listen`/`Relay` routing pins, the public `Cluster`/`Chain` composite family, and the `Grasshopper2.SpecialObjects.ScribbleObject` annotation land as rows of one `NativeKind` catalog. `PersistedValue` closes the values the public host surface can read or assign, and each catalog row carries an executable public-constructor factory.

One polymorphic owner mints, reads, assigns, reconciles timer targets, and resolves cluster maps on the rail; GH1 interop remains one explicit live-host boundary returning a typed receipt beside the host component. GH2's loop driver, looping iterations, repeat discriminants, bitmap sampler kernel, and incomplete chain ordering and validation kernels stay outside the package contract.

## [01]-[INDEX]

- [02]-[CONTROL_VOCABULARY]: the object families with the public boundary and accumulation vocabularies
- [03]-[VALUE_AND_CATALOG]: the `PersistedValue` union and the `NativeKind` object-row catalog
- [04]-[OBJECT_OPERATIONS]: the one owner over mint, value, timer targets, and cluster maps
- [05]-[GH1_BOUNDARY]: the live GH1 host admission and its receipt

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

- Owner: `PersistedValue` is the one union over empty construction and publicly readable or writable object state; `NativeKind` carries family, exact host type, and an executable public parameterless-constructor factory per row.
- Entry: `NativeKind.ForHost(Type)` is the `Items`-derived frozen index dispatching a live document object onto its row.
- Packages: every host type column is a verified `Grasshopper2.Parameters.Special` or `Grasshopper2.Components.Standard` type; canvas control state stays each object's own `CreateAttributes` projection.
- Growth: a new interactive object is one catalog row with its public factory and, where its value shape is new, one `PersistedValue` case.
- Boundary: `Empty` selects the row's public parameterless constructor; a non-empty seed enters only through an exact public constructor or writable host member arm.

```csharp signature
// --- [MODELS] ----------------------------------------------------------------------------

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record PersistedValue {
    private PersistedValue() { }

    public sealed record Empty : PersistedValue;
    public sealed record Slider(Grasshopper2.UI.UiNumber Value) : PersistedValue;
    public sealed record Number(decimal Value) : PersistedValue;
    public sealed record Flag(bool Value) : PersistedValue;
    public sealed record Momentary(IPear Up, IPear Down) : PersistedValue;
    public sealed record Text(string Value) : PersistedValue;
    public sealed record Parsed(string Source) : PersistedValue;
    public sealed record Swatch(Grasshopper2.Types.Colour.Colour Value, bool Apply) : PersistedValue;
    public sealed record Selection(Option<Seq<string>> UserNames) : PersistedValue;
    public sealed record Targets(Seq<Guid> Ids) : PersistedValue;
}

// --- [SERVICES] --------------------------------------------------------------------------

[SmartEnum<string>]
public sealed partial class NativeKind {
    public static readonly NativeKind NumberSlider =
        Of<Grasshopper2.Parameters.Special.NumberSliderObject>("numberSlider", ObjectFamily.ValueInput);
    public static readonly NativeKind NumberPicker =
        Of<Grasshopper2.Parameters.Special.NumberPickerObject>("numberPicker", ObjectFamily.ValueInput);
    public static readonly NativeKind Toggle =
        Of<Grasshopper2.Parameters.Special.ToggleObject>("toggle", ObjectFamily.ValueInput);
    public static readonly NativeKind Button =
        Of<Grasshopper2.Parameters.Special.ButtonObject>("button", ObjectFamily.ValueInput);
    public static readonly NativeKind Value =
        Of<Grasshopper2.Parameters.Special.ValueObject>("value", ObjectFamily.ValueInput);
    public static readonly NativeKind TextInput =
        Of<Grasshopper2.Parameters.Special.TextInputObject>("textInput", ObjectFamily.ValueInput);
    public static readonly NativeKind ColourSwatch =
        Of<Grasshopper2.Parameters.Special.ColourSwatchObject>("colourSwatch", ObjectFamily.ValueInput);
    public static readonly NativeKind GradientEditor =
        Of<Grasshopper2.Parameters.Special.GradientEditorObject>("gradientEditor", ObjectFamily.Editor);
    public static readonly NativeKind FunctionEditor =
        Of<Grasshopper2.Parameters.Special.FunctionEditorObject>("functionEditor", ObjectFamily.Editor);
    public static readonly NativeKind MaterialEditor =
        Of<Grasshopper2.Parameters.Special.MaterialEditorObject>("materialEditor", ObjectFamily.Editor);
    public static readonly NativeKind Histogram =
        Of<Grasshopper2.Parameters.Special.HistogramObject>("histogram", ObjectFamily.Editor);
    public static readonly NativeKind QuickGraph =
        Of<Grasshopper2.Parameters.Special.QuickGraphObject>("quickGraph", ObjectFamily.Editor);
    public static readonly NativeKind Protractor =
        Of<Grasshopper2.Parameters.Special.ProtractorObject>("protractor", ObjectFamily.Editor);
    public static readonly NativeKind ImageSampler =
        Of<Grasshopper2.Parameters.Special.ImageSamplerObject>("imageSampler", ObjectFamily.Sampler);
    public static readonly NativeKind PresetPicker =
        Of<Grasshopper2.Parameters.Special.PresetPickerObject>("presetPicker", ObjectFamily.Picker);
    public static readonly NativeKind ComplexPicker =
        Of<Grasshopper2.Parameters.Special.ComplexPickerObject>("complexPicker", ObjectFamily.Picker);
    public static readonly NativeKind ConstantPicker =
        Of<Grasshopper2.Parameters.Special.ConstantPickerObject>("constantPicker", ObjectFamily.Picker);
    public static readonly NativeKind MetaNamePicker =
        Of<Grasshopper2.Parameters.Special.MetaNamePickerObject>("metaNamePicker", ObjectFamily.Picker);
    public static readonly NativeKind TemporalPicker =
        Of<Grasshopper2.Parameters.Special.TemporalPickerObject>("temporalPicker", ObjectFamily.Picker);
    public static readonly NativeKind ValueList =
        Of<Grasshopper2.Parameters.Special.ValueListObject>("valueList", ObjectFamily.List);
    public static readonly NativeKind PathMapper =
        Of<Grasshopper2.Parameters.Special.PathMapperObject>("pathMapper", ObjectFamily.Data);
    public static readonly NativeKind DataPanel =
        Of<Grasshopper2.Parameters.Special.DataPanelObject>("dataPanel", ObjectFamily.Data);
    public static readonly NativeKind DataRecorder =
        Of<Grasshopper2.Parameters.Special.DataRecorderObject>("dataRecorder", ObjectFamily.Data);
    public static readonly NativeKind TreeViewer =
        Of<Grasshopper2.Parameters.Special.TreeViewerObject>("treeViewer", ObjectFamily.Data);
    public static readonly NativeKind Timer =
        Of<Grasshopper2.Parameters.Special.TimerObject>("timer", ObjectFamily.Utility);
    public static readonly NativeKind Shout =
        Of<Grasshopper2.Parameters.Special.Shout>("shout", ObjectFamily.Routing);
    public static readonly NativeKind Listen =
        Of<Grasshopper2.Parameters.Special.Listen>("listen", ObjectFamily.Routing);
    public static readonly NativeKind Relay =
        Of<Grasshopper2.Parameters.Special.Relay>("relay", ObjectFamily.Routing);
    public static readonly NativeKind Cluster =
        Of<Grasshopper2.Components.Standard.Cluster>("cluster", ObjectFamily.Composite);
    public static readonly NativeKind Chain =
        Of<Grasshopper2.Components.Standard.Chain>("chain", ObjectFamily.Composite);
    public static readonly NativeKind Scribble =
        Of<Grasshopper2.SpecialObjects.ScribbleObject>("scribble", ObjectFamily.Annotation);

    public ObjectFamily Family { get; }

    public Type Host { get; }

    [UseDelegateFromConstructor]
    public partial Grasshopper2.Doc.IDocumentObject Create();

    private static readonly Lazy<FrozenDictionary<Type, NativeKind>> ByHost =
        new(static () => Items.ToFrozenDictionary(static row => row.Host));

    public static Option<NativeKind> ForHost(Type? host) =>
        host is not null && ByHost.Value.TryGetValue(host, out NativeKind? row) ? Optional(row) : None;

    private static NativeKind Of<T>(string key, ObjectFamily family)
        where T : Grasshopper2.Doc.IDocumentObject, new() =>
        new(key, family, typeof(T), static () => new T());
}
```

## [04]-[OBJECT_OPERATIONS]

- Owner: `NativeObject` is the one operation surface over the catalog — mint, value read, value assignment, momentary pulse, timer-target reconciliation, cluster construction and map resolution, and disentanglement dispatch through joint patterns over (row, value) and the live host object's own type.
- Entry: every entry is polymorphic on its host argument's runtime shape; the `var` floor arms are the open host-object space, refused with the row key in evidence.
- Receipt: `Clustered` names the constructor's first `out Guid[][]` as the input mapping and the second as the output mapping; `Boundary` calls the public `void EnsureMaps(out Listen[], out Shout[])` and returns the resulting pin rosters without inventing a success probe.
- Auto: `Retarget` rejects the timer's own instance id before mutation, compares `TimerObject.TargetIds` with the distinct desired set, and admits each `AddTarget`/`RemoveTarget` only when its public boolean result confirms the mutation.
- Growth: a public parameterless constructor is one `[UseDelegateFromConstructor]` `NativeKind.Create()` row; a seeded public constructor or writable value correspondence is one `Mint` or `ValueOf`/`Assign` arm.
- Boundary: `NumberSliderObject.InternalNumber` and `NumberPickerObject.InternalNumber` are read-only and seed only through their public constructors; the slider preserves its complete non-null `UiNumber`, while the picker carries its public `decimal` read through the host's lossy `double` constructor boundary. `PresetPickerObject.UserNames`, including its `null` state, owns persisted selection; assignment expires and restarts a solution only when that state changes. `DataRecorderObject.Paused` is the recorder's writable state, and its `IsEmpty` answers true when recorded buckets exist — the member name and XML summary invert the installed behavior — so a presence probe reads a true `IsEmpty` as data-present evidence. `Boundary` rejects null pins from the oblivious host arrays. Incomplete host chain ordering and validation members, the internal loop driver, and private `ImageSamplerObject.SampleContinuous` never enter this operation surface.

```csharp signature
// --- [OPERATIONS] ------------------------------------------------------------------------

public static class NativeObject {
    public static Fin<Grasshopper2.Doc.IDocumentObject> Mint(NativeKind? kind, PersistedValue? seed, Op? key = null) =>
        (kind, seed) switch {
            (null, _) or (_, null) => Fin.Fail<Grasshopper2.Doc.IDocumentObject>(
                new GhFault.Refused(key.OrDefault(), nameof(Mint))),
            var (row, value) when row == NativeKind.NumberSlider && value is PersistedValue.Slider number =>
                Hosted.Bound(
                    () => (Grasshopper2.Doc.IDocumentObject)new Grasshopper2.Parameters.Special.NumberSliderObject(
                        row.Key, number.Value),
                    key.OrDefault()),
            var (row, value) when row == NativeKind.NumberPicker && value is PersistedValue.Number number =>
                Hosted.Bound(
                    () => (Grasshopper2.Doc.IDocumentObject)new Grasshopper2.Parameters.Special.NumberPickerObject(
                        row.Key, (double)number.Value),
                    key.OrDefault()),
            var (row, value) when row == NativeKind.Toggle && value is PersistedValue.Flag flag =>
                Hosted.Bound(() => (Grasshopper2.Doc.IDocumentObject)new Grasshopper2.Parameters.Special.ToggleObject(flag.Value), key.OrDefault()),
            var (row, value) when row == NativeKind.Button && value is PersistedValue.Momentary pair =>
                Hosted.Bound(
                    () => (Grasshopper2.Doc.IDocumentObject)new Grasshopper2.Parameters.Special.ButtonObject(pair.Up, pair.Down),
                    key.OrDefault()),
            var (row, value) when row == NativeKind.TextInput && value is PersistedValue.Text text =>
                Hosted.Bound(
                    () => (Grasshopper2.Doc.IDocumentObject)new Grasshopper2.Parameters.Special.TextInputObject(
                        row.Key, text.Value),
                    key.OrDefault()),
            var (row, value) when row == NativeKind.PresetPicker && value is PersistedValue.Selection names =>
                Hosted.Bound(() => {
                    Grasshopper2.Parameters.Special.PresetPickerObject picker = new(row.Key);
                    names.UserNames.Iter(selected => picker.UserNames = selected.ToArray());
                    return (Grasshopper2.Doc.IDocumentObject)picker;
                }, key.OrDefault()),
            var (row, value) when row == NativeKind.Scribble && value is PersistedValue.Text text =>
                Hosted.Bound(
                    () => (Grasshopper2.Doc.IDocumentObject)new Grasshopper2.SpecialObjects.ScribbleObject(text.Value),
                    key.OrDefault()),
            (var row, PersistedValue.Empty _) => Hosted.Bound(row.Create, key.OrDefault()),
            var (row, value) => Hosted.Bound(row.Create, key.OrDefault())
                .Bind(host => Assign(host, value, key.OrDefault()).Map(_ => host)),
        };

    public static Fin<PersistedValue> ValueOf(Grasshopper2.Doc.IDocumentObject? host, Op? key = null) =>
        host switch {
            Grasshopper2.Parameters.Special.NumberSliderObject slider =>
                Hosted.Bound(() => (PersistedValue)new PersistedValue.Slider(slider.InternalNumber), key.OrDefault()),
            Grasshopper2.Parameters.Special.ToggleObject toggle =>
                Hosted.Bound(() => (PersistedValue)new PersistedValue.Flag(toggle.ToggleState), key.OrDefault()),
            Grasshopper2.Parameters.Special.NumberPickerObject picker =>
                Hosted.Bound(() => (PersistedValue)new PersistedValue.Number(picker.InternalNumber), key.OrDefault()),
            Grasshopper2.Parameters.Special.ValueObject value =>
                Hosted.Bound(() => (PersistedValue)new PersistedValue.Parsed(value.Text), key.OrDefault()),
            Grasshopper2.Parameters.Special.TextInputObject input =>
                Hosted.Bound(() => (PersistedValue)new PersistedValue.Text(input.Contents), key.OrDefault()),
            Grasshopper2.Parameters.Special.ColourSwatchObject swatch =>
                Hosted.Bound(() => (PersistedValue)new PersistedValue.Swatch(swatch.Colour, Apply: false), key.OrDefault()),
            Grasshopper2.Parameters.Special.PresetPickerObject picker =>
                Hosted.Bound(
                    () => (PersistedValue)new PersistedValue.Selection(
                        Optional(picker.UserNames).Map(static names => toSeq(names))),
                    key.OrDefault()),
            Grasshopper2.Parameters.Special.TimerObject timer =>
                Hosted.Bound(() => (PersistedValue)new PersistedValue.Targets(toSeq(timer.TargetIds)), key.OrDefault()),
            Grasshopper2.Parameters.Special.DataRecorderObject recorder =>
                Hosted.Bound(() => (PersistedValue)new PersistedValue.Flag(recorder.Paused), key.OrDefault()),
            Grasshopper2.Parameters.Special.Relay relay =>
                Hosted.Bound(() => (PersistedValue)new PersistedValue.Flag(relay.Frozen), key.OrDefault()),
            Grasshopper2.Components.Standard.Cluster cluster =>
                Hosted.Bound(() => (PersistedValue)new PersistedValue.Flag(cluster.LoopSolution), key.OrDefault()),
            Grasshopper2.SpecialObjects.ScribbleObject scribble =>
                Hosted.Bound(() => (PersistedValue)new PersistedValue.Text(scribble.Text), key.OrDefault()),
            null => Fin.Fail<PersistedValue>(new GhFault.Refused(key.OrDefault(), nameof(ValueOf))),
            var unknown => Fin.Fail<PersistedValue>(new GhFault.Refused(key.OrDefault(), $"{nameof(ValueOf)}:{unknown.GetType().Name}")),
        };

    public static Fin<Unit> Assign(Grasshopper2.Doc.IDocumentObject? host, PersistedValue value, Op? key = null) =>
        (host, value) switch {
            (Grasshopper2.Parameters.Special.ToggleObject toggle, PersistedValue.Flag flag) =>
                Hosted.Bound(() => { toggle.ToggleState = flag.Value; }, key.OrDefault()),
            (Grasshopper2.Parameters.Special.ValueObject target, PersistedValue.Parsed parsed) =>
                Hosted.Bound(() => target.AssignTextAndValue(parsed.Source), key.OrDefault()),
            (Grasshopper2.Parameters.Special.TextInputObject target, PersistedValue.Text text) =>
                Hosted.Bound(() => { target.Contents = text.Value; }, key.OrDefault()),
            (Grasshopper2.Parameters.Special.ColourSwatchObject swatch, PersistedValue.Swatch colour) =>
                Hosted.Bound(() => swatch.SetColour(colour.Value, colour.Apply), key.OrDefault()),
            (Grasshopper2.Parameters.Special.ButtonObject button, PersistedValue.Flag flag) =>
                Hosted.Bound(() => { if (flag.Value) { button.Press(); } else { button.Release(); } }, key.OrDefault()),
            (Grasshopper2.Parameters.Special.PresetPickerObject picker, PersistedValue.Selection names) =>
                Select(picker, names.UserNames, key.OrDefault()),
            (Grasshopper2.Parameters.Special.TimerObject timer, PersistedValue.Targets targets) => Retarget(timer, targets.Ids, key.OrDefault()),
            (Grasshopper2.Parameters.Special.DataRecorderObject recorder, PersistedValue.Flag flag) =>
                Hosted.Bound(() => { recorder.Paused = flag.Value; }, key.OrDefault()),
            (Grasshopper2.Parameters.Special.Relay relay, PersistedValue.Flag flag) =>
                Hosted.Bound(() => { relay.Frozen = flag.Value; }, key.OrDefault()),
            (Grasshopper2.Components.Standard.Cluster cluster, PersistedValue.Flag flag) =>
                Hosted.Bound(() => { cluster.LoopSolution = flag.Value; }, key.OrDefault()),
            (Grasshopper2.SpecialObjects.ScribbleObject scribble, PersistedValue.Text text) =>
                Hosted.Bound(() => { scribble.Text = text.Value; }, key.OrDefault()),
            (null, _) => Fin.Fail<Unit>(new GhFault.Refused(key.OrDefault(), nameof(Assign))),
            var (unknown, _) => Fin.Fail<Unit>(new GhFault.Refused(key.OrDefault(), $"{nameof(Assign)}:{unknown.GetType().Name}")),
        };

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

    private static Fin<Unit> Select(
        Grasshopper2.Parameters.Special.PresetPickerObject picker,
        Option<Seq<string>> desired,
        Op key) =>
        Hosted.Bound(() => {
            string[]? names = desired.Match(
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
