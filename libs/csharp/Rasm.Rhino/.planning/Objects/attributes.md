# [RASM_RHINO_OBJECTS_ATTRIBUTES]

Typed attribute mutation belongs to `Rasm.Rhino.Objects`. `AttributeEdit` closes the writable `ObjectAttributes` families with verified payload carriers, parameterizes group, decal, and face-material set operations over detached seeds, and covers layer, source-resolved display, space, overrides, section state, hatch state, frames, meshing, and tags. `AttributeProgram` admits and folds edits over the duplicate supplied by `TableOp.Amend`; this page exposes no local write entry. One `AttributeAsk` owns the read side: `AttributeSnapshot` captures detached scalar and census state, `EffectiveDisplay` resolves document- and viewport-dependent display values.

## [01]-[INDEX]

- [02]-[EDIT_FAMILY]: `OverrideMove`, the detached seed carriers, and the `AttributeEdit` union — the closed mutation vocabulary with its total dispatch.
- [03]-[PROGRAM]: `AttributeProgram` — the fold, the `Amend` handoff, and the write-path law.
- [04]-[SNAPSHOT_AND_EFFECTIVE]: `AttributeAsk`, `AttributeSnapshot`, `EffectiveDisplay`, and the one read entry.
- [05]-[SURFACE_LEDGER]: the page's owner table.

## [02]-[EDIT_FAMILY]

- Owner: `OverrideMove` `[SmartEnum<int>]` parameterizes impose, extend, and retract over set-valued carriers; `ShadowPolicy` owns every cast/receive combination; `DecalSeed` and `MaterialRefSeed` are generated admitted products; `AttributeEdit` `[Union]` owns the assigned stored-attribute mutations and one total `Apply` over the working duplicate.
- Law: source-dependent payloads admit one coherent product. Object-sourced color, plot color, plot weight, linetype, and material edits require their object value; every other source rejects that irrelevant value. `LinetypePatternScale` remains independent of source and may accompany any line-pattern edit.
- Law: mode and visibility are refused by absence — no case writes `Mode` or `Visible`, because object mode transitions are the table rail's `TableOp.State` and a second write path forks the undo story; `Realm` writes the catalogued space and optional viewport anchor, which no table op carries.
- Law: the three override families are three cases — `ModeOverride` collapses the four display-mode host members onto one `(viewport, mode)` option pair where `None` removes, `DetailHide` is one per-detail toggle, and `Activity` is the `OverrideMove`-dispatched set edit over `SetActiveInViewportOverrides`/`AddActiveInViewportOverride`/`RemoveActiveInViewportOverride`; a per-member sibling verb roster is the deleted form.
- Law: removal is the `None` arm of the same case — `CustomLine`, `SectionFace`, and `Meshing` remove their custom carrier when absent and install it when present; `Meshing` admits and normalizes `MeshingParameters.ToEncodedString()` output once, then reconstructs the disposable carrier only inside `Apply`.
- Law: display-mode override carries the mode id and resolves it with `DisplayModeDescription.GetDisplayMode(Guid)` inside `Apply`; an unresolved id is typed failure, never a retained `DisplayModeDescription`.
- Law: `RenderingReset` is the sole rendering-attribute clear and composes `ClearRenderingAttributes`; piecemeal reset calls never compete with it.
- Law: user strings reuse the document vocabulary — the `Tag` case carries the geometry page's `TagOp` and admits only its mutating verbs, applied against the attribute set's own user-string store; a read verb inside a mutation program is refused at the factory.
- Law: groups, decals, and face materials share the impose/extend/retract grammar without sharing payload identity. `Impose` clears then installs, `Extend` installs a non-empty roster, `Retract` removes a non-empty roster, and empty `Impose` is the sole clear form.
- Law: decal and face-material payloads are detached seeds, never live carriers — `DecalSeed.Build` fills a `DecalCreateParams` for `Decal.Create` and `MaterialRefSeed.Build` fills a `MaterialRefCreateParams` for `MaterialRefs.Create`, each minted and released inside its apply arm, because a live `Decal` or `MaterialRef` enumerated under an earlier grant is host state whose mutation does not persist.
- Law: decal removal keys on `Decal.CRC` — the host removes by decal identity, so retract carries the snapshot's `Crc` column and the arm removes every live decal whose `CRC` matches; face-material removal keys on the plug-in guid the dictionary indexes.
- Law: host quirks cross verbatim — `DecalCreateParams.StartLatitude`/`EndLatitude` carry the horizontal sweep and `StartLongitude`/`EndLongitude` the vertical (the host inverts the names), and `MaterialRefs.Create` swaps front and back values across its native boundary; neither is locally corrected, so a host repair never double-swaps.
- Growth: a new writable axis adds one edit case, one admission arm, one apply arm, and its detached read projection when the page owns that read.

```csharp signature
// --- [RUNTIME_PRELUDE] --------------------------------------------------------------------
using Rasm.Domain;
using Rasm.Rhino.Document;
using Rhino.DocObjects;
using Rhino.Display;
using Rhino.FileIO;
using Rhino.Geometry;
using Rhino.Render;

namespace Rasm.Rhino.Objects;

// --- [TYPES] ------------------------------------------------------------------------------
[SmartEnum<int>]
public sealed partial class OverrideMove {
    public static readonly OverrideMove Impose = new(key: 0);
    public static readonly OverrideMove Extend = new(key: 1);
    public static readonly OverrideMove Retract = new(key: 2);

    internal Fin<Seq<T>> Admit<T>(Seq<T> values, Func<T, bool> valid, Op key) =>
        from roster in values.TraverseM(value => valid(value)
            ? Fin.Succ(value: value)
            : Fin.Fail<T>(error: key.InvalidInput())).As()
        from _ in guard(this == Impose || !roster.IsEmpty, key.InvalidInput()).ToFin()
        select roster.Distinct();
}

[SmartEnum]
public sealed partial class ShadowPolicy {
    public static readonly ShadowPolicy None = new(casts: false, receives: false);
    public static readonly ShadowPolicy Cast = new(casts: true, receives: false);
    public static readonly ShadowPolicy Receive = new(casts: false, receives: true);
    public static readonly ShadowPolicy Both = new(casts: true, receives: true);

    public bool Casts { get; }
    public bool Receives { get; }

    internal static ShadowPolicy Of(bool casts, bool receives) => (casts, receives) switch {
        (false, false) => None,
        (true, false) => Cast,
        (false, true) => Receive,
        _ => Both,
    };
}

[ComplexValueObject]
public sealed partial class DecalSeed {
    public Guid Texture { get; }
    public DecalMapping Mapping { get; }
    public DecalProjection Projection { get; }
    public Point3d Origin { get; }
    public Vector3d Up { get; }
    public Vector3d Across { get; }
    public double Transparency { get; }
    public ObjectSignal MapToInside { get; }
    public double Height { get; }
    public double Radius { get; }
    public double HorzStart { get; }
    public double HorzEnd { get; }
    public double VertStart { get; }
    public double VertEnd { get; }
    public double MinU { get; }
    public double MinV { get; }
    public double MaxU { get; }
    public double MaxV { get; }

    [BoundaryAdapter]
    static partial void ValidateFactoryArguments(
        ref ValidationError? validationError,
        ref Guid texture,
        ref DecalMapping mapping,
        ref DecalProjection projection,
        ref Point3d origin,
        ref Vector3d up,
        ref Vector3d across,
        ref double transparency,
        ref ObjectSignal mapToInside,
        ref double height,
        ref double radius,
        ref double horzStart,
        ref double horzEnd,
        ref double vertStart,
        ref double vertEnd,
        ref double minU,
        ref double minV,
        ref double maxU,
        ref double maxV) {
        validationError = texture != Guid.Empty
            && origin.IsValid && up.IsValid && across.IsValid
            && mapToInside is not null
            && transparency is >= 0.0 and <= 1.0
            && double.IsFinite(height) && height > 0.0
            && double.IsFinite(radius) && radius > 0.0
            && double.IsFinite(horzStart) && double.IsFinite(horzEnd)
            && double.IsFinite(vertStart) && double.IsFinite(vertEnd)
            && double.IsFinite(minU) && double.IsFinite(minV)
            && double.IsFinite(maxU) && double.IsFinite(maxV)
            && minU <= maxU && minV <= maxV
            ? validationError
            : new ValidationError(message: "decal seed is invalid");
    }

    internal DecalCreateParams Build() => new() {
        TextureInstanceId = Texture,
        DecalMapping = Mapping,
        DecalProjection = Projection,
        MapToInside = MapToInside.On,
        Transparency = Transparency,
        Origin = Origin,
        VectorUp = Up,
        VectorAcross = Across,
        Height = Height,
        Radius = Radius,
        StartLatitude = HorzStart,
        EndLatitude = HorzEnd,
        StartLongitude = VertStart,
        EndLongitude = VertEnd,
        MinU = MinU,
        MinV = MinV,
        MaxU = MaxU,
        MaxV = MaxV,
    };
}

[ComplexValueObject]
public sealed partial class MaterialRefSeed {
    public Guid PlugIn { get; }
    public ObjectMaterialSource Source { get; }
    public Guid FrontId { get; }
    public Guid BackId { get; }
    public int FrontIndex { get; }
    public int BackIndex { get; }

    [BoundaryAdapter]
    static partial void ValidateFactoryArguments(
        ref ValidationError? validationError,
        ref Guid plugIn,
        ref ObjectMaterialSource source,
        ref Guid frontId,
        ref Guid backId,
        ref int frontIndex,
        ref int backIndex) {
        validationError = plugIn != Guid.Empty
            && frontIndex >= -1 && backIndex >= -1
            && (frontId != Guid.Empty || frontIndex >= 0)
            && (backId != Guid.Empty || backIndex >= 0)
            ? validationError
            : new ValidationError(message: "material reference seed is invalid");
    }

    internal MaterialRefCreateParams Build() => new() {
        PlugInId = PlugIn,
        MaterialSource = Source,
        FrontFaceMaterialId = FrontId,
        FrontFaceMaterialIndex = FrontIndex,
        BackFaceMaterialId = BackId,
        BackFaceMaterialIndex = BackIndex,
    };
}

[Union(SwitchMapStateParameterName = "context", ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record AttributeEdit {
    private AttributeEdit() { }
    public sealed record Identity(Option<string> Name, Option<string> Url) : AttributeEdit;
    public sealed record Layer(int Index) : AttributeEdit;
    public sealed record Paint(ObjectColorSource Source, Option<System.Drawing.Color> Value = default) : AttributeEdit;
    public sealed record Plot(ObjectPlotColorSource Source, Option<System.Drawing.Color> Value = default) : AttributeEdit;
    public sealed record PlotWeight(ObjectPlotWeightSource Source, Option<double> Millimeters = default) : AttributeEdit;
    public sealed record LinePattern(ObjectLinetypeSource Source, Option<int> Index = default, Option<double> PatternScale = default) : AttributeEdit;
    public sealed record CustomLine(Option<Linetype> Pattern) : AttributeEdit;
    public sealed record MaterialBind(ObjectMaterialSource Source, Option<int> Index = default) : AttributeEdit;
    public sealed record Shadows(ShadowPolicy Policy) : AttributeEdit;
    public sealed record Wires(int Density) : AttributeEdit;
    public sealed record DrawOrder(int Rank) : AttributeEdit;
    public sealed record Decorate(ObjectDecoration Ends) : AttributeEdit;
    public sealed record Realm(ActiveSpace Space, Option<Guid> Viewport = default) : AttributeEdit;
    public sealed record Groups(OverrideMove Move, Seq<int> Indices) : AttributeEdit;
    public sealed record ModeOverride(Option<Guid> Viewport, Option<Guid> Mode) : AttributeEdit;
    public sealed record DetailHide(Guid Detail, ObjectSignal Signal) : AttributeEdit;
    public sealed record DetailBackground(ObjectSignal Signal) : AttributeEdit;
    public sealed record Activity(OverrideMove Move, Seq<Guid> Viewports, ObjectSignal Signal) : AttributeEdit;
    public sealed record SectionSource(ObjectSectionAttributesSource Source) : AttributeEdit;
    public sealed record SectionIndex(int Index) : AttributeEdit;
    public sealed record SectionFace(Option<SectionStyle> Style) : AttributeEdit;
    public sealed record SectionLabel(SectionLabelStyle Style) : AttributeEdit;
    public sealed record HatchFill(Option<System.Drawing.Color> Fill = default, Option<System.Drawing.Color> Print = default) : AttributeEdit;
    public sealed record HatchBoundary(
        Option<ObjectSignal> Visible = default,
        Option<System.Drawing.Color> Color = default,
        Option<System.Drawing.Color> PlotColor = default,
        Option<ItemColorSource> ColorSource = default,
        Option<ItemColorSource> PlotColorSource = default,
        Option<double> PlotWeightMillimeters = default) : AttributeEdit;
    public sealed record AnchorFrame(Plane Frame) : AttributeEdit;
    public sealed record AnchorMove(Transform Motion) : AttributeEdit;
    public sealed record Meshing(Option<string> Encoded) : AttributeEdit;
    public sealed record RenderingReset : AttributeEdit;
    public sealed record Decals(OverrideMove Move, Seq<DecalSeed> Seeds, Seq<int> Crcs) : AttributeEdit;
    public sealed record Tag(TagOp Operation) : AttributeEdit;
    public sealed record FaceMaterials(OverrideMove Move, Seq<MaterialRefSeed> Seeds, Seq<Guid> Keys) : AttributeEdit;

    internal Fin<AttributeEdit> Admit(Op op) =>
        Switch(
            op,
            identity: static (key, edit) =>
                from name in edit.Name.Traverse(text => key.AcceptText(value: text)).As()
                from url in edit.Url.Traverse(text => key.AcceptText(value: text)).As()
                from _ in guard(name.IsSome || url.IsSome, key.InvalidInput()).ToFin()
                select (AttributeEdit)new Identity(Name: name, Url: url),
            layer: static (key, edit) => guard(edit.Index >= 0, key.InvalidInput()).ToFin().Map(_ => (AttributeEdit)edit),
            paint: static (key, edit) => SourceValue(edit.Source is ObjectColorSource.ColorFromObject, edit.Value, edit, key),
            plot: static (key, edit) => SourceValue(edit.Source is ObjectPlotColorSource.PlotColorFromObject, edit.Value, edit, key),
            plotWeight: static (key, edit) =>
                from admitted in SourceValue(
                    edit.Source is ObjectPlotWeightSource.PlotWeightFromObject, edit.Millimeters, edit, key)
                from _ in guard(edit.Millimeters
                    .Map(static value => double.IsFinite(value) && value >= 0.0)
                    .IfNone(noneValue: true), key.InvalidInput()).ToFin()
                select admitted,
            linePattern: static (key, edit) =>
                from _ in guard((edit.Source is ObjectLinetypeSource.LinetypeFromObject) == edit.Index.IsSome, key.InvalidInput()).ToFin()
                from __ in guard(edit.Index.Map(static value => value >= 0).IfNone(noneValue: true), key.InvalidInput()).ToFin()
                from ___ in guard(edit.PatternScale
                    .Map(static value => double.IsFinite(value) && value > 0.0)
                    .IfNone(noneValue: true), key.InvalidInput()).ToFin()
                select (AttributeEdit)edit,
            customLine: static (_, edit) => Fin.Succ<AttributeEdit>(edit),
            materialBind: static (key, edit) =>
                from _ in guard((edit.Source is ObjectMaterialSource.MaterialFromObject) == edit.Index.IsSome, key.InvalidInput()).ToFin()
                from __ in guard(edit.Index.Map(static value => value >= 0).IfNone(noneValue: true), key.InvalidInput()).ToFin()
                select (AttributeEdit)edit,
            shadows: static (key, edit) => key.Need(edit.Policy)
                .Map(policy => (AttributeEdit)new Shadows(Policy: policy)),
            wires: static (_, edit) => Fin.Succ<AttributeEdit>(edit),
            drawOrder: static (_, edit) => Fin.Succ<AttributeEdit>(edit),
            decorate: static (_, edit) => Fin.Succ<AttributeEdit>(edit),
            realm: static (key, edit) => guard(
                edit.Space is not ActiveSpace.None
                && edit.Viewport.Map(static value => value != Guid.Empty).IfNone(noneValue: true),
                key.InvalidInput()).ToFin().Map(_ => (AttributeEdit)edit),
            groups: static (key, edit) => SetValues(
                move: edit.Move,
                values: edit.Indices,
                valid: static value => value >= 0,
                build: static (move, values) => new Groups(Move: move, Indices: values),
                key: key),
            modeOverride: static (key, edit) => guard(
                edit.Viewport.Map(static value => value != Guid.Empty).IfNone(noneValue: true)
                && edit.Mode.Map(static value => value != Guid.Empty).IfNone(noneValue: true),
                key.InvalidInput()).ToFin().Map(_ => (AttributeEdit)edit),
            detailHide: static (key, edit) =>
                from _ in guard(edit.Detail != Guid.Empty, key.InvalidInput()).ToFin()
                from __ in key.Need(edit.Signal)
                select (AttributeEdit)edit,
            detailBackground: static (key, edit) => key.Need(edit.Signal).Map(_ => (AttributeEdit)edit),
            activity: static (key, edit) =>
                from signal in key.Need(edit.Signal)
                from values in SetValues(
                    move: edit.Move,
                    values: edit.Viewports,
                    valid: static value => value != Guid.Empty,
                    build: (move, values) => new Activity(Move: move, Viewports: values, Signal: signal),
                    key: key)
                select values,
            sectionSource: static (_, edit) => Fin.Succ<AttributeEdit>(edit),
            sectionIndex: static (key, edit) => guard(edit.Index >= -1, key.InvalidInput()).ToFin().Map(_ => (AttributeEdit)edit),
            sectionFace: static (_, edit) => Fin.Succ<AttributeEdit>(edit),
            sectionLabel: static (_, edit) => Fin.Succ<AttributeEdit>(edit),
            hatchFill: static (key, edit) => guard(edit.Fill.IsSome || edit.Print.IsSome, key.InvalidInput()).ToFin().Map(_ => (AttributeEdit)edit),
            hatchBoundary: static (key, edit) =>
                from visibility in edit.Visible.Traverse(signal => key.Need(signal)).As()
                from _ in guard(
                    visibility.IsSome || edit.Color.IsSome || edit.PlotColor.IsSome || edit.ColorSource.IsSome
                    || edit.PlotColorSource.IsSome || edit.PlotWeightMillimeters.IsSome,
                    key.InvalidInput()).ToFin()
                from __ in guard(edit.PlotWeightMillimeters
                    .Map(static value => double.IsFinite(value) && value >= 0.0)
                    .IfNone(noneValue: true), key.InvalidInput()).ToFin()
                select (AttributeEdit)new HatchBoundary(
                    Visible: visibility,
                    Color: edit.Color,
                    PlotColor: edit.PlotColor,
                    ColorSource: edit.ColorSource,
                    PlotColorSource: edit.PlotColorSource,
                    PlotWeightMillimeters: edit.PlotWeightMillimeters),
            anchorFrame: static (key, edit) => key.AcceptInput(value: edit.Frame).Map(_ => (AttributeEdit)edit),
            anchorMove: static (key, edit) => key.AcceptInput(value: edit.Motion).Map(_ => (AttributeEdit)edit),
            meshing: static (key, edit) => edit.Encoded
                .Traverse(text =>
                    from accepted in key.AcceptText(value: text)
                    from normalized in key.Catch(() => {
                        using MeshingParameters? parameters = MeshingParameters.FromEncodedString(accepted);
                        return parameters is null
                            ? Fin.Fail<string>(error: key.InvalidInput())
                            : Fin.Succ(value: parameters.ToEncodedString());
                    })
                    select normalized)
                .As()
                .Map(encoded => (AttributeEdit)new Meshing(Encoded: encoded)),
            renderingReset: static (_, edit) => Fin.Succ<AttributeEdit>(edit),
            decals: static (key, edit) =>
                from rosters in MoveShaped(
                    move: edit.Move,
                    seeds: edit.Seeds,
                    admitSeed: seed => key.Need(seed),
                    keys: edit.Crcs,
                    validKey: static crc => crc != 0,
                    key: key)
                select (AttributeEdit)new Decals(Move: edit.Move, Seeds: rosters.Seeds, Crcs: rosters.Keys),
            tag: static (key, edit) =>
                from operation in key.Need(edit.Operation)
                from _ in guard(operation.Mutates, key.InvalidInput()).ToFin()
                select (AttributeEdit)new Tag(Operation: operation),
            faceMaterials: static (key, edit) =>
                from rosters in MoveShaped(
                    move: edit.Move,
                    seeds: edit.Seeds,
                    admitSeed: seed => key.Need(seed),
                    keys: edit.Keys,
                    validKey: static value => value != Guid.Empty,
                    key: key)
                select (AttributeEdit)new FaceMaterials(Move: edit.Move, Seeds: rosters.Seeds, Keys: rosters.Keys));

    private static Fin<AttributeEdit> SourceValue<TValue>(bool requires, Option<TValue> value, AttributeEdit edit, Op key) =>
        guard(requires == value.IsSome, key.InvalidInput()).ToFin().Map(_ => edit);

    private static Fin<AttributeEdit> SetValues<T>(
        OverrideMove move,
        Seq<T> values,
        Func<T, bool> valid,
        Func<OverrideMove, Seq<T>, AttributeEdit> build,
        Op key) =>
        from mode in key.Need(move)
        from admitted in mode.Admit(values: values, valid: valid, key: key)
        select build(mode, admitted);

    private static Fin<(Seq<TSeed> Seeds, Seq<TKey> Keys)> MoveShaped<TSeed, TKey>(
        OverrideMove move,
        Seq<TSeed> seeds,
        Func<TSeed, Fin<TSeed>> admitSeed,
        Seq<TKey> keys,
        Func<TKey, bool> validKey,
        Op key) =>
        from mode in key.Need(move)
        from grown in seeds.TraverseM(admitSeed).As()
        from identities in keys.TraverseM(value => validKey(value)
            ? Fin.Succ(value: value)
            : Fin.Fail<TKey>(error: key.InvalidInput())).As()
        from _ in guard(
            mode.Switch(
                impose: () => identities.IsEmpty,
                extend: () => !grown.IsEmpty && identities.IsEmpty,
                retract: () => grown.IsEmpty && !identities.IsEmpty),
            key.InvalidInput()).ToFin()
        select (grown, identities.Distinct());

    internal Fin<Unit> Apply(ObjectAttributes attributes, Op op) =>
        Switch(
            (Attributes: attributes, Op: op),
            identity: static (context, edit) => context.Op.Catch(() => {
                _ = edit.Name.Iter(name => context.Attributes.Name = name);
                _ = edit.Url.Iter(url => context.Attributes.Url = url);
            }),
            layer: static (context, edit) => context.Op.Catch(() => context.Attributes.LayerIndex = edit.Index),
            paint: static (context, edit) => context.Op.Catch(() => {
                context.Attributes.ColorSource = edit.Source;
                _ = edit.Value.Iter(color => context.Attributes.ObjectColor = color);
            }),
            plot: static (context, edit) => context.Op.Catch(() => {
                context.Attributes.PlotColorSource = edit.Source;
                _ = edit.Value.Iter(color => context.Attributes.PlotColor = color);
            }),
            plotWeight: static (context, edit) => context.Op.Catch(() => {
                context.Attributes.PlotWeightSource = edit.Source;
                _ = edit.Millimeters.Iter(weight => context.Attributes.PlotWeight = weight);
            }),
            linePattern: static (context, edit) => context.Op.Catch(() => {
                context.Attributes.LinetypeSource = edit.Source;
                _ = edit.Index.Iter(index => context.Attributes.LinetypeIndex = index);
                _ = edit.PatternScale.Iter(scale => context.Attributes.LinetypePatternScale = scale);
            }),
            customLine: static (context, edit) => context.Op.Catch(() => {
                edit.Pattern.Match(
                    Some: pattern => context.Attributes.SetCustomLinetype(linetype: pattern),
                    None: () => context.Attributes.RemoveCustomLinetype());
            }),
            materialBind: static (context, edit) => context.Op.Catch(() => {
                context.Attributes.MaterialSource = edit.Source;
                _ = edit.Index.Iter(index => context.Attributes.MaterialIndex = index);
            }),
            shadows: static (context, edit) => context.Op.Catch(() => {
                context.Attributes.CastsShadows = edit.Policy.Casts;
                context.Attributes.ReceivesShadows = edit.Policy.Receives;
            }),
            wires: static (context, edit) => context.Op.Catch(() => context.Attributes.WireDensity = edit.Density),
            drawOrder: static (context, edit) => context.Op.Catch(() => context.Attributes.DisplayOrder = edit.Rank),
            decorate: static (context, edit) => context.Op.Catch(() => context.Attributes.ObjectDecoration = edit.Ends),
            realm: static (context, edit) => context.Op.Catch(() => {
                context.Attributes.Space = edit.Space;
                context.Attributes.ViewportId = edit.Viewport.IfNone(noneValue: Guid.Empty);
            }),
            groups: static (context, edit) => edit.Move.Switch(
                (context.Attributes, context.Op, edit.Indices),
                impose: static held => held.Op.Catch(() => {
                    held.Attributes.RemoveFromAllGroups();
                    _ = held.Indices.Iter(index => held.Attributes.AddToGroup(groupIndex: index));
                }),
                extend: static held => held.Op.Catch(() => _ = held.Indices.Iter(index => held.Attributes.AddToGroup(groupIndex: index))),
                retract: static held => held.Op.Catch(() => _ = held.Indices.Iter(index => held.Attributes.RemoveFromGroup(groupIndex: index)))),
            modeOverride: static (context, edit) => edit.Mode
                .Traverse(id => Optional(DisplayModeDescription.GetDisplayMode(id)).ToFin(Fail: context.Op.MissingContext()))
                .As()
                .Bind(mode => (mode.Case, edit.Viewport.Case) switch {
                    (DisplayModeDescription resolved, Guid viewport) => context.Op.Confirm(
                        success: context.Attributes.SetDisplayModeOverride(mode: resolved, rhinoViewportId: viewport)),
                    (DisplayModeDescription resolved, null) => context.Op.Confirm(
                        success: context.Attributes.SetDisplayModeOverride(mode: resolved)),
                    (null, Guid viewport) => context.Op.Catch(() => context.Attributes.RemoveDisplayModeOverride(rhinoViewportId: viewport)),
                    _ => context.Op.Catch(() => context.Attributes.RemoveDisplayModeOverride()),
                }),
            detailHide: static (context, edit) => context.Op.Confirm(success: edit.Signal.On
                ? context.Attributes.AddHideInDetailOverride(detailId: edit.Detail)
                : context.Attributes.RemoveHideInDetailOverride(detailId: edit.Detail)),
            detailBackground: static (context, edit) => context.Op.Catch(() => context.Attributes.DetailBackgroundVisible = edit.Signal.On),
            activity: static (context, edit) => edit.Move.Switch(
                (context.Attributes, context.Op, edit.Viewports, edit.Signal.On),
                impose: static held => held.Op.Confirm(
                    success: held.Attributes.SetActiveInViewportOverrides(viewportIds: held.Viewports.ToArray(), active: held.Active)),
                extend: static held => held.Viewports.TraverseM(viewport => held.Op.Confirm(
                    success: held.Attributes.AddActiveInViewportOverride(viewportId: viewport, active: held.Active))).As()
                    .Map(static _ => unit),
                retract: static held => held.Viewports.TraverseM(viewport => held.Op.Confirm(
                    success: held.Attributes.RemoveActiveInViewportOverride(viewportId: viewport, active: held.Active))).As()
                    .Map(static _ => unit)),
            sectionSource: static (context, edit) => context.Op.Catch(() => context.Attributes.SectionAttributesSource = edit.Source),
            sectionIndex: static (context, edit) => context.Op.Catch(() => context.Attributes.SectionStyleIndex = edit.Index),
            sectionFace: static (context, edit) => context.Op.Catch(() => {
                edit.Style.Match(
                    Some: style => context.Attributes.SetCustomSectionStyle(sectionStyle: style),
                    None: () => context.Attributes.RemoveCustomSectionStyle());
            }),
            sectionLabel: static (context, edit) => context.Op.Catch(() => context.Attributes.ClippingPlaneLabelStyle = edit.Style),
            hatchFill: static (context, edit) => context.Op.Catch(() => {
                _ = edit.Fill.Iter(color => context.Attributes.HatchBackgroundFillColor = color);
                _ = edit.Print.Iter(color => context.Attributes.HatchBackgroundFillPrintColor = color);
            }),
            hatchBoundary: static (context, edit) => context.Op.Catch(() => {
                _ = edit.Visible.Iter(signal => context.Attributes.HatchBoundaryVisible = signal.On);
                _ = edit.Color.Iter(color => context.Attributes.HatchBoundaryColor = color);
                _ = edit.PlotColor.Iter(color => context.Attributes.HatchBoundaryPlotColor = color);
                _ = edit.ColorSource.Iter(source => context.Attributes.HatchBoundaryColorSource = source);
                _ = edit.PlotColorSource.Iter(source => context.Attributes.HatchBoundaryPlotColorSource = source);
                _ = edit.PlotWeightMillimeters.Iter(weight => context.Attributes.HatchBoundaryPlotWeightMillimeters = weight);
            }),
            anchorFrame: static (context, edit) => context.Op.Catch(() => context.Attributes.SetObjectFrame(plane: edit.Frame)),
            anchorMove: static (context, edit) => context.Op.Catch(() => context.Attributes.SetObjectFrame(xform: edit.Motion)),
            meshing: static (context, edit) => context.Op.Catch(() => {
                if (edit.Encoded.Case is string encoded) {
                    using MeshingParameters? parameters = MeshingParameters.FromEncodedString(encoded);
                    if (parameters is null) { return Fin.Fail<Unit>(context.Op.InvalidResult()); }
                    context.Attributes.CustomMeshingParameters = parameters;
                    context.Attributes.EnableCustomMeshingParameters = true;
                } else {
                    context.Attributes.EnableCustomMeshingParameters = false;
                    context.Attributes.CustomMeshingParameters = null;
                }
                return Fin.Succ(value: unit);
            }),
            renderingReset: static (context, _) => context.Op.Catch(() => context.Attributes.ClearRenderingAttributes()),
            decals: static (context, edit) => edit.Move.Switch(
                (context.Attributes, context.Op, edit.Seeds, edit.Crcs),
                impose: static held => held.Op.Catch(() => held.Attributes.Decals.RemoveAllDecals())
                    .Bind(_ => Grown(attributes: held.Attributes, seeds: held.Seeds, key: held.Op)),
                extend: static held => Grown(attributes: held.Attributes, seeds: held.Seeds, key: held.Op),
                retract: static held => held.Op.Catch(() =>
                    toSeq(held.Attributes.Decals)
                        .Filter(decal => held.Crcs.Exists(crc => crc == decal.CRC))
                        .TraverseM(decal => held.Op.Confirm(success: held.Attributes.Decals.Remove(decal: decal))).As()
                        .Map(static _ => unit))),
            tag: static (context, edit) => edit.Operation.Switch(
                (context.Attributes, context.Op),
                set: static (held, verb) =>
                    from key in held.Op.AcceptText(value: verb.Key)
                    from _ in held.Op.Confirm(success: held.Attributes.SetUserString(key: key, value: verb.Value))
                    select unit,
                read: static (held, _) => Fin.Fail<Unit>(error: held.Op.InvalidInput()),
                readAll: static (held, _) => Fin.Fail<Unit>(error: held.Op.InvalidInput()),
                delete: static (held, verb) =>
                    from key in held.Op.AcceptText(value: verb.Key)
                    from _ in held.Op.Confirm(success: held.Attributes.DeleteUserString(key: key))
                    select unit,
                clear: static (held, _) => held.Op.Catch(() => held.Attributes.DeleteAllUserStrings())),
            faceMaterials: static (context, edit) => edit.Move.Switch(
                (context.Attributes, context.Op, edit.Seeds, edit.Keys),
                impose: static held => held.Op.Catch(() => held.Attributes.MaterialRefs.Clear())
                    .Bind(_ => Bound(attributes: held.Attributes, seeds: held.Seeds, key: held.Op)),
                extend: static held => Bound(attributes: held.Attributes, seeds: held.Seeds, key: held.Op),
                retract: static held => held.Keys.TraverseM(plugin => held.Op.Confirm(
                    success: held.Attributes.MaterialRefs.Remove(key: plugin))).As().Map(static _ => unit)));

    private static Fin<Unit> Grown(ObjectAttributes attributes, Seq<DecalSeed> seeds, Op key) =>
        seeds.TraverseM(seed => key.Catch(() => {
                using Decal? minted = Decal.Create(createParams: seed.Build());
                return minted is null
                    ? Fin.Fail<Unit>(error: key.InvalidResult())
                    : guard(attributes.Decals.Add(decal: minted) != 0u, key.InvalidResult()).ToFin();
            })).As()
            .Map(static _ => unit);

    private static Fin<Unit> Bound(ObjectAttributes attributes, Seq<MaterialRefSeed> seeds, Op key) =>
        seeds.TraverseM(seed => key.Catch(() => {
                using MaterialRef minted = attributes.MaterialRefs.Create(createParams: seed.Build());
                attributes.MaterialRefs.Add(key: seed.PlugIn, value: minted);
            })).As()
            .Map(static _ => unit);
}
```

## [03]-[PROGRAM]

- Owner: `AttributeProgram` — the admitted edit sequence with one fold: `Apply(ObjectAttributes) : Fin<Unit>` runs every edit in declaration order over the working set and short-circuits on the first refusal, matching the `TableOp.Amend` change-callback contract exactly.
- Law: the program IS the `Amend` payload — `TableOp.Amend(target, program.Apply, notice)` is the one write path: the table rail duplicates the live attribute set, the program mutates the duplicate, `ModifyAttributes` commits it under the undo bracket, and the duplicate disposes before the operation leaves the host boundary; a consumer holding a live `ObjectAttributes` and mutating it in place has no undo story and is the deleted form.
- Law: the fold is short-circuit by construction — a program is one attribute transaction, so a mid-sequence refusal abandons the working duplicate uncommitted and the live object never sees a half-applied program; accumulation belongs to the caller batching programs across objects on the table rail's traversal.
- Law: `Tag` read verbs are refused at admission — `Of` rejects a program carrying a non-mutating `TagOp` so the refusal is a construction fact, never a mid-commit surprise.
- Growth: a new edit case rides every existing program untouched; a program-level policy is a field on this record, never a parallel program type.

```csharp signature
// --- [MODELS] -----------------------------------------------------------------------------
public sealed class AttributeProgram {
    private AttributeProgram(Seq<AttributeEdit> edits) => Edits = edits;

    public Seq<AttributeEdit> Edits { get; }

    public static Fin<AttributeProgram> Of(params ReadOnlySpan<AttributeEdit> edits) {
        Op op = Op.Of(name: nameof(AttributeProgram));
        return from values in toSeq(edits.ToArray())
                   .TraverseM(edit => op.Need(edit)).As()
               from _ in guard(!values.IsEmpty, op.InvalidInput()).ToFin()
               from admitted in values.TraverseM(edit => edit.Admit(op: op)).As()
               select new AttributeProgram(edits: admitted);
    }

    public Fin<Unit> Apply(ObjectAttributes attributes) {
        Op op = Op.Of(name: nameof(AttributeProgram));
        return from working in op.Need(attributes)
               from _ in Edits.TraverseM(edit => edit.Apply(attributes: working, op: op)).As()
               select unit;
    }
}
```

## [04]-[SNAPSHOT_AND_EFFECTIVE]

- Owner: `AttributeAsk` `[Union]` closes stored and source-resolved questions; `AttributeAnswer` `[Union]` owns their detached rosters; `AttributeSnapshot` captures stored scalar state, group and override rosters, shadow policy, normalized meshing policy, render-material identity, user strings, complete decal rows, material-reference rows, and foreign-owner presence facts; `EffectiveDisplay` captures resolved color, plot, mode, and activity values.
- Entry: `Attributes.Ask(DocumentSession, TableTarget, AttributeAsk) : Fin<AttributeAnswer>` — one entry resolves through the state page's object fold and reads inside one `SessionNeed.Read` grant.
- Law: stored and effective are different questions — the snapshot reports what the attribute set declares, `EffectiveDisplay` reports what `DrawColor`/`ComputedPlotColor`/`ComputedPlotWeight` resolve after source dispatch against layer, parent, and material; a consumer diffing the two reads exactly which sources defer.
- Law: detail-hide is census membership — `HasHideInDetailOverrideSet(detailId)` is set membership in `GetHideInDetailOverrides()`, so the snapshot's `HiddenInDetails` roster answers any detail id and `EffectiveDisplay` stays a pure viewport question; a detail object id passed where a viewport id belongs is the conflation this split forecloses.
- Law: snapshot products contain detached values only. Decals and material references project their catalogued read surfaces into records, and custom meshing round-trips through the normalized encoded value. Custom linetype, custom section style, mapping, and mesh modifiers remain foreign-owner presence facts.
- Boundary: `ComputedSectionStyle` demands a sectioner's attributes and stays a direct host call at the display seam; this page resolves the three display scalars every consumer needs.

```csharp signature
// --- [TYPES] ------------------------------------------------------------------------------
[Union(SwitchMapStateParameterName = "context", ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record AttributeAsk {
    private AttributeAsk() { }
    public sealed record Stored : AttributeAsk;
    public sealed record Resolved(Option<Guid> Viewport = default) : AttributeAsk;

    internal Fin<AttributeAsk> Admit(Op op) =>
        Switch(
            op,
            stored: static (_, ask) => Fin.Succ<AttributeAsk>(ask),
            resolved: static (key, ask) => guard(
                ask.Viewport.Map(static value => value != Guid.Empty).IfNone(noneValue: true),
                key.InvalidInput()).ToFin().Map(_ => (AttributeAsk)ask));
}

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record AttributeAnswer : IDetachedDocumentResult {
    private AttributeAnswer() { }
    public sealed record Declared(Seq<AttributeSnapshot> Rows) : AttributeAnswer;
    public sealed record Effective(Seq<EffectiveDisplay> Rows) : AttributeAnswer;
}

// --- [MODELS] -----------------------------------------------------------------------------
public sealed record OverrideCensus(
    Seq<Guid> HiddenInDetails,
    Option<(Seq<Guid> Viewports, ObjectSignal Signal)> Activity,
    ObjectSignal DetailBackgroundVisible);

public readonly record struct DecalSnapshot(
    int Crc,
    DecalMapping Mapping,
    DecalProjection Projection,
    Point3d Origin,
    Vector3d Up,
    Vector3d Across,
    double Transparency,
    ObjectSignal MapToInside,
    bool Visible,
    double Height,
    double Radius,
    double StartLatitude,
    double EndLatitude,
    double StartLongitude,
    double EndLongitude,
    double MinU,
    double MinV,
    double MaxU,
    double MaxV,
    Guid TextureInstanceId) {
    internal static DecalSnapshot Of(Decal decal) {
        decal.GetUVBounds(out double minU, out double minV, out double maxU, out double maxV);
        return new DecalSnapshot(
            Crc: decal.CRC,
            Mapping: decal.Mapping,
            Projection: decal.Projection,
            Origin: decal.Origin,
            Up: decal.VectorUp,
            Across: decal.VectorAcross,
            Transparency: decal.Transparency,
            MapToInside: decal.MapToInside ? ObjectSignal.Enabled : ObjectSignal.Disabled,
            Visible: decal.IsVisible,
            Height: decal.Height,
            Radius: decal.Radius,
            StartLatitude: decal.StartLatitude,
            EndLatitude: decal.EndLatitude,
            StartLongitude: decal.StartLongitude,
            EndLongitude: decal.EndLongitude,
            MinU: minU,
            MinV: minV,
            MaxU: maxU,
            MaxV: maxV,
            TextureInstanceId: decal.TextureInstanceId);
    }
}

public readonly record struct MaterialRefSnapshot(
    Guid DictionaryKey,
    ObjectMaterialSource Source,
    Guid PlugInId,
    Guid FrontId,
    Guid BackId,
    int FrontIndex,
    int BackIndex);

public sealed record AttributeSnapshot(
    Guid ObjectId,
    Option<string> Name,
    Option<string> Url,
    int LayerIndex,
    int LinetypeIndex,
    int MaterialIndex,
    Guid ViewportId,
    ActiveSpace Space,
    ObjectColorSource ColorSource,
    ObjectPlotColorSource PlotColorSource,
    ObjectPlotWeightSource PlotWeightSource,
    ObjectLinetypeSource LinetypeSource,
    ObjectMaterialSource MaterialSource,
    Option<Guid> RenderMaterialId,
    ObjectSectionAttributesSource SectionSource,
    System.Drawing.Color ObjectColor,
    System.Drawing.Color PlotColor,
    double PlotWeight,
    double LinetypePatternScale,
    int WireDensity,
    int DisplayOrder,
    ObjectDecoration Decoration,
    ShadowPolicy Shadows,
    Seq<int> Groups,
    OverrideCensus Overrides,
    int SectionStyleIndex,
    SectionLabelStyle SectionLabel,
    bool CustomSectionStyle,
    bool CustomLinetype,
    Plane Frame,
    Option<string> Meshing,
    bool HasMapping,
    bool DefinitionMember,
    Seq<(string Key, string Value)> UserStrings,
    Seq<DecalSnapshot> Decals,
    Seq<MaterialRefSnapshot> MaterialRefs,
    System.Drawing.Color HatchFill,
    System.Drawing.Color HatchPrint,
    ObjectSignal HatchBoundaryVisible,
    System.Drawing.Color HatchBoundaryColor,
    System.Drawing.Color HatchBoundaryPlotColor,
    ItemColorSource HatchBoundaryColorSource,
    ItemColorSource HatchBoundaryPlotColorSource,
    double HatchBoundaryPlotWeight,
    (bool Displacement, bool EdgeSoftening, bool Thickening, bool CurvePiping, bool ShutLining) Modifiers)
    : IDetachedDocumentResult {
    internal static Fin<AttributeSnapshot> Of(ObjectAttributes attributes, Op key) =>
        key.Catch(() => {
            bool overrides = attributes.GetActiveInViewportOverrides(viewportIds: out Guid[] viewports, active: out bool active);
            File3dmMeshModifiers modifiers = attributes.File3dmMeshModifiers;
            System.Collections.Specialized.NameValueCollection tags = attributes.GetUserStrings();
            using SectionStyle? customSection = attributes.GetCustomSectionStyle();
            using Linetype? customLine = attributes.GetCustomLinetype();
            using MeshingParameters? customMesh = attributes.EnableCustomMeshingParameters
                ? attributes.CustomMeshingParameters
                : null;
            return Fin.Succ(value: new AttributeSnapshot(
                ObjectId: attributes.ObjectId,
                Name: Optional(attributes.Name).Filter(static text => text.Length > 0),
                Url: Optional(attributes.Url).Filter(static text => text.Length > 0),
                LayerIndex: attributes.LayerIndex,
                LinetypeIndex: attributes.LinetypeIndex,
                MaterialIndex: attributes.MaterialIndex,
                ViewportId: attributes.ViewportId,
                Space: attributes.Space,
                ColorSource: attributes.ColorSource,
                PlotColorSource: attributes.PlotColorSource,
                PlotWeightSource: attributes.PlotWeightSource,
                LinetypeSource: attributes.LinetypeSource,
                MaterialSource: attributes.MaterialSource,
                RenderMaterialId: Optional(attributes.RenderMaterial).Map(static material => material.Id),
                SectionSource: attributes.SectionAttributesSource,
                ObjectColor: attributes.ObjectColor,
                PlotColor: attributes.PlotColor,
                PlotWeight: attributes.PlotWeight,
                LinetypePatternScale: attributes.LinetypePatternScale,
                WireDensity: attributes.WireDensity,
                DisplayOrder: attributes.DisplayOrder,
                Decoration: attributes.ObjectDecoration,
                Shadows: ShadowPolicy.Of(casts: attributes.CastsShadows, receives: attributes.ReceivesShadows),
                Groups: toSeq(attributes.GetGroupList()),
                Overrides: new OverrideCensus(
                    HiddenInDetails: toSeq(attributes.GetHideInDetailOverrides()),
                    Activity: overrides
                        ? Some((toSeq(viewports), active ? ObjectSignal.Enabled : ObjectSignal.Disabled))
                        : Option<(Seq<Guid>, ObjectSignal)>.None,
                    DetailBackgroundVisible: attributes.DetailBackgroundVisible
                        ? ObjectSignal.Enabled
                        : ObjectSignal.Disabled),
                SectionStyleIndex: attributes.SectionStyleIndex,
                SectionLabel: attributes.ClippingPlaneLabelStyle,
                CustomSectionStyle: customSection is not null,
                CustomLinetype: customLine is not null,
                Frame: attributes.ObjectFrame(),
                Meshing: Optional(customMesh).Map(static parameters => parameters.ToEncodedString()),
                HasMapping: attributes.HasMapping,
                DefinitionMember: attributes.IsInstanceDefinitionObject,
                UserStrings: toSeq(tags.AllKeys).Choose(key => Optional(key)
                    .Bind(name => Optional(tags[name]).Map(value => (name, value)))),
                Decals: attributes.Decals.AsIterable().Map(DecalSnapshot.Of).ToSeq(),
                MaterialRefs: attributes.MaterialRefs.AsIterable().Map(static pair => new MaterialRefSnapshot(
                    DictionaryKey: pair.Key,
                    Source: pair.Value.MaterialSource,
                    PlugInId: pair.Value.PlugInId,
                    FrontId: pair.Value.FrontFaceMaterialId,
                    BackId: pair.Value.BackFaceMaterialId,
                    FrontIndex: pair.Value.FrontFaceMaterialIndex,
                    BackIndex: pair.Value.BackFaceMaterialIndex)).ToSeq(),
                HatchFill: attributes.HatchBackgroundFillColor,
                HatchPrint: attributes.HatchBackgroundFillPrintColor,
                HatchBoundaryVisible: attributes.HatchBoundaryVisible
                    ? ObjectSignal.Enabled
                    : ObjectSignal.Disabled,
                HatchBoundaryColor: attributes.HatchBoundaryColor,
                HatchBoundaryPlotColor: attributes.HatchBoundaryPlotColor,
                HatchBoundaryColorSource: attributes.HatchBoundaryColorSource,
                HatchBoundaryPlotColorSource: attributes.HatchBoundaryPlotColorSource,
                HatchBoundaryPlotWeight: attributes.HatchBoundaryPlotWeightMillimeters,
                Modifiers: (
                    Optional(modifiers.Displacement).IsSome,
                    Optional(modifiers.EdgeSoftening).IsSome,
                    Optional(modifiers.Thickening).IsSome,
                    Optional(modifiers.CurvePiping).IsSome,
                    Optional(modifiers.ShutLining).IsSome)));
        });
}

public readonly record struct EffectiveDisplay(
    Guid Id,
    System.Drawing.Color Draw,
    System.Drawing.Color Plot,
    double PlotWeight,
    Option<Guid> ModeOverride,
    Option<ObjectSignal> ActiveOverride) : IDetachedDocumentResult {
    internal static Fin<EffectiveDisplay> Of(RhinoObject native, Rhino.RhinoDoc document, Option<Guid> viewport, Op key) =>
        key.Catch(() => {
            ObjectAttributes attributes = native.Attributes;
            return Fin.Succ(value: viewport.Case is Guid scoped
                ? new EffectiveDisplay(
                    Id: native.Id,
                    Draw: attributes.DrawColor(document: document, viewportId: scoped),
                    Plot: attributes.ComputedPlotColor(document: document, viewportId: scoped),
                    PlotWeight: attributes.ComputedPlotWeight(document: document, viewportId: scoped),
                    ModeOverride: attributes.HasDisplayModeOverride(viewportId: scoped)
                        ? Some(attributes.GetDisplayModeOverride(viewportId: scoped))
                        : Option<Guid>.None,
                    ActiveOverride: attributes.HasActiveInViewportOverride(viewportId: scoped, active: out bool enabled)
                        ? Some(enabled ? ObjectSignal.Enabled : ObjectSignal.Disabled)
                        : Option<ObjectSignal>.None)
                : new EffectiveDisplay(
                    Id: native.Id,
                    Draw: attributes.DrawColor(document: document),
                    Plot: attributes.ComputedPlotColor(document: document),
                    PlotWeight: attributes.ComputedPlotWeight(document: document),
                    ModeOverride: Option<Guid>.None,
                    ActiveOverride: Option<ObjectSignal>.None));
        });
}

// --- [OPERATIONS] -------------------------------------------------------------------------
public static class Attributes {
    public static Fin<AttributeAnswer> Ask(DocumentSession session, TableTarget target, AttributeAsk ask) {
        Op op = Op.Of();
        return from active in op.Need(ask).Bind(value => value.Admit(op: op))
               from answer in session.Demand(
                   use: document =>
                       from natives in Objects.Resolve(document: document, target: target, key: op)
                       from folded in active.Switch(
                           (Document: document, Natives: natives, Op: op),
                           stored: static (ctx, _) => ctx.Natives
                               .TraverseM(native => AttributeSnapshot.Of(attributes: native.Attributes, key: ctx.Op)).As()
                               .Map(static rows => (AttributeAnswer)new AttributeAnswer.Declared(Rows: rows)),
                           resolved: static (ctx, ask) => ctx.Natives
                               .TraverseM(native => EffectiveDisplay.Of(
                                   native: native, document: ctx.Document, viewport: ask.Viewport, key: ctx.Op)).As()
                               .Map(static rows => (AttributeAnswer)new AttributeAnswer.Effective(Rows: rows)))
                       select folded,
                   key: op,
                   needs: [SessionNeed.Read])
               select answer;
    }
}
```

## [05]-[SURFACE_LEDGER]

| [INDEX] | [CONCERN]          | [OWNER]             | [FORM]                                                  | [ENTRY]                        |
| :-----: | :----------------- | :------------------ | :------------------------------------------------------ | :----------------------------- |
|  [01]   | attribute mutation | `AttributeEdit`     | admitted union, total `Apply`, host enums at the seam   | program payloads               |
|  [02]   | set-valued edits   | `OverrideMove`      | impose/extend/retract over groups, overrides, carriers  | owning edit payloads           |
|  [03]   | detached carriers  | generated products  | `DecalSeed`/`MaterialRefSeed` onto host create params   | `Decals` / `FaceMaterials`     |
|  [04]   | write program      | `AttributeProgram`  | short-circuit fold matching the `Amend` callback shape  | `TableOp.Amend(target, Apply)` |
|  [05]   | read dispatch      | `AttributeAsk`      | stored and resolved questions, one typed answer union   | `Attributes.Ask`               |
|  [06]   | stored state       | `AttributeSnapshot` | detached scalars, rosters, and catalogued carriers      | `AttributeAsk.Stored`          |
|  [07]   | resolved display   | `EffectiveDisplay`  | resolved scalars and viewport overrides                 | `AttributeAsk.Resolved`        |
