# [RASM_RHINO_OBJECTS_ATTRIBUTES]

Typed attribute mutation belongs to `Rasm.Rhino.Objects`. `AttributeEdit` closes the writable `ObjectAttributes` families with verified payload carriers, parameterizes group, decal, and face-material set operations over detached seeds, and covers layer, source-resolved display, space, overrides, section state, hatch state, frames, meshing, and tags. `AttributeProgram` admits and folds edits over the duplicate supplied by `TableOp.Amend`; this page exposes no local write entry. One `AttributeAsk` owns the read side: `AttributeSnapshot` captures detached scalar and census state, `EffectiveDisplay` resolves document- and viewport-dependent display values.

## [01]-[INDEX]

- [02]-[EDIT_FAMILY]: `OverrideMove`, the detached seed carriers, and the `AttributeEdit` union — the closed mutation vocabulary with its total dispatch.
- [03]-[PROGRAM]: `AttributeProgram` — the fold, the `Amend` handoff, and the write-path law.
- [04]-[SNAPSHOT_AND_EFFECTIVE]: `AttributeAsk`, `AttributeSnapshot`, `EffectiveDisplay`, and the one read entry.
- [05]-[SURFACE_LEDGER]: the page's owner table.

## [02]-[EDIT_FAMILY]

- Owner: `OverrideMove` `[SmartEnum<int>]` parameterizes replace, add, and remove behavior over set-valued carriers. `AttributeEdit` `[Union]` keeps one case per distinct host payload shape and one total `Apply` over the working duplicate.
- Law: source-dependent payloads admit one coherent product. Object-sourced color, plot color, plot weight, linetype, and material edits require their object value; every other source rejects that irrelevant value. `LinetypePatternScale` remains independent of source and may accompany any line-pattern edit.
- Law: mode and visibility are refused by absence — no case writes `Mode` or `Visible`, because object mode transitions are the table rail's `TableOp.State` and a second write path forks the undo story; `Realm` writes the catalogued space and optional viewport anchor, which no table op carries.
- Law: the three override families are three cases — `ModeOverride` collapses the four display-mode host members onto one `(viewport, mode)` option pair where `None` removes, `DetailHide` is one per-detail toggle, and `Activity` is the `OverrideMove`-dispatched set edit over `SetActiveInViewportOverrides`/`AddActiveInViewportOverride`/`RemoveActiveInViewportOverride`; a per-member sibling verb roster is the deleted form.
- Law: removal is the `None` arm of the same case — `CustomLine`, `SectionFace`, and `Meshing` remove their custom carrier when the option is absent and install it when present, so set-versus-remove never becomes a boolean knob or a sibling case.
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

public readonly record struct DecalSeed(
    Guid Texture,
    DecalMapping Mapping,
    DecalProjection Projection,
    Point3d Origin,
    Vector3d Up,
    Vector3d Across,
    double Transparency = 0.0,
    bool MapToInside = false,
    double Height = 1.0,
    double Radius = 1.0,
    double HorzStart = 0.0,
    double HorzEnd = 360.0,
    double VertStart = 0.0,
    double VertEnd = 180.0,
    double MinU = 0.0,
    double MinV = 0.0,
    double MaxU = 1.0,
    double MaxV = 1.0) {
    internal Fin<DecalSeed> Admit(Op key) =>
        from origin in key.AcceptInput(value: Origin)
        from up in key.AcceptInput(value: Up)
        from across in key.AcceptInput(value: Across)
        from _ in guard(
            Texture != Guid.Empty
            && Transparency is >= 0.0 and <= 1.0
            && double.IsFinite(Height) && double.IsFinite(Radius)
            && double.IsFinite(HorzStart) && double.IsFinite(HorzEnd)
            && double.IsFinite(VertStart) && double.IsFinite(VertEnd)
            && MinU <= MaxU && MinV <= MaxV,
            key.InvalidInput()).ToFin()
        select this with { Origin = origin, Up = up, Across = across };

    internal DecalCreateParams Build() => new() {
        TextureInstanceId = Texture,
        DecalMapping = Mapping,
        DecalProjection = Projection,
        MapToInside = MapToInside,
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

public readonly record struct MaterialRefSeed(
    Guid PlugIn,
    ObjectMaterialSource Source,
    Guid FrontId,
    Guid BackId,
    int FrontIndex = -1,
    int BackIndex = -1) {
    internal Fin<MaterialRefSeed> Admit(Op key) =>
        guard(PlugIn != Guid.Empty && FrontIndex >= -1 && BackIndex >= -1, key.InvalidInput()).ToFin().Map(_ => this);

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
    public sealed record Shadows(bool Casts, bool Receives) : AttributeEdit;
    public sealed record Wires(int Density) : AttributeEdit;
    public sealed record DrawOrder(int Rank) : AttributeEdit;
    public sealed record Decorate(ObjectDecoration Ends) : AttributeEdit;
    public sealed record Realm(ActiveSpace Space, Option<Guid> Viewport = default) : AttributeEdit;
    public sealed record Groups(OverrideMove Move, Seq<int> Indices) : AttributeEdit;
    public sealed record ModeOverride(Option<Guid> Viewport, Option<DisplayModeDescription> Mode) : AttributeEdit;
    public sealed record DetailHide(Guid Detail, bool Hidden) : AttributeEdit;
    public sealed record DetailBackground(bool Visible) : AttributeEdit;
    public sealed record Activity(OverrideMove Move, Seq<Guid> Viewports, bool Active) : AttributeEdit;
    public sealed record SectionSource(ObjectSectionAttributesSource Source) : AttributeEdit;
    public sealed record SectionIndex(int Index) : AttributeEdit;
    public sealed record SectionFace(Option<SectionStyle> Style) : AttributeEdit;
    public sealed record SectionLabel(SectionLabelStyle Style) : AttributeEdit;
    public sealed record HatchFill(Option<System.Drawing.Color> Fill = default, Option<System.Drawing.Color> Print = default) : AttributeEdit;
    public sealed record HatchBoundary(
        Option<bool> Visible = default,
        Option<System.Drawing.Color> Color = default,
        Option<System.Drawing.Color> PlotColor = default,
        Option<ItemColorSource> ColorSource = default,
        Option<ItemColorSource> PlotColorSource = default,
        Option<double> PlotWeightMillimeters = default) : AttributeEdit;
    public sealed record AnchorFrame(Plane Frame) : AttributeEdit;
    public sealed record AnchorMove(Transform Motion) : AttributeEdit;
    public sealed record Meshing(Option<MeshingParameters> Custom) : AttributeEdit;
    public sealed record Decals(OverrideMove Move, Seq<DecalSeed> Seeds, Seq<int> Crcs) : AttributeEdit;
    public sealed record Tag(TagOp Operation) : AttributeEdit;
    public sealed record FaceMaterials(OverrideMove Move, Seq<MaterialRefSeed> Seeds, Seq<Guid> Keys) : AttributeEdit;

    internal Fin<AttributeEdit> Admit(Op op) =>
        Switch(
            context: op,
            identity: static (key, edit) => guard(edit.Name.IsSome || edit.Url.IsSome, key.InvalidInput()).ToFin().Map(_ => (AttributeEdit)edit),
            layer: static (key, edit) => guard(edit.Index >= 0, key.InvalidInput()).ToFin().Map(_ => (AttributeEdit)edit),
            paint: static (key, edit) => SourceValue(edit.Source is ObjectColorSource.ColorFromObject, edit.Value, edit, key),
            plot: static (key, edit) => SourceValue(edit.Source is ObjectPlotColorSource.PlotColorFromObject, edit.Value, edit, key),
            plotWeight: static (key, edit) => SourceValue(edit.Source is ObjectPlotWeightSource.PlotWeightFromObject, edit.Millimeters, edit, key),
            linePattern: static (key, edit) =>
                from _ in guard((edit.Source is ObjectLinetypeSource.LinetypeFromObject) == edit.Index.IsSome, key.InvalidInput()).ToFin()
                from __ in guard(edit.Index.Map(static value => value >= 0).IfNone(noneValue: true), key.InvalidInput()).ToFin()
                select (AttributeEdit)edit,
            customLine: static (_, edit) => Fin.Succ<AttributeEdit>(edit),
            materialBind: static (key, edit) =>
                from _ in guard((edit.Source is ObjectMaterialSource.MaterialFromObject) == edit.Index.IsSome, key.InvalidInput()).ToFin()
                from __ in guard(edit.Index.Map(static value => value >= 0).IfNone(noneValue: true), key.InvalidInput()).ToFin()
                select (AttributeEdit)edit,
            shadows: static (_, edit) => Fin.Succ<AttributeEdit>(edit),
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
            modeOverride: static (key, edit) => guard(edit.Viewport.Map(static value => value != Guid.Empty).IfNone(noneValue: true), key.InvalidInput()).ToFin().Map(_ => (AttributeEdit)edit),
            detailHide: static (key, edit) => guard(edit.Detail != Guid.Empty, key.InvalidInput()).ToFin().Map(_ => (AttributeEdit)edit),
            detailBackground: static (_, edit) => Fin.Succ<AttributeEdit>(edit),
            activity: static (key, edit) => SetValues(
                move: edit.Move,
                values: edit.Viewports,
                valid: static value => value != Guid.Empty,
                build: (move, values) => new Activity(Move: move, Viewports: values, Active: edit.Active),
                key: key),
            sectionSource: static (_, edit) => Fin.Succ<AttributeEdit>(edit),
            sectionIndex: static (key, edit) => guard(edit.Index >= -1, key.InvalidInput()).ToFin().Map(_ => (AttributeEdit)edit),
            sectionFace: static (_, edit) => Fin.Succ<AttributeEdit>(edit),
            sectionLabel: static (_, edit) => Fin.Succ<AttributeEdit>(edit),
            hatchFill: static (key, edit) => guard(edit.Fill.IsSome || edit.Print.IsSome, key.InvalidInput()).ToFin().Map(_ => (AttributeEdit)edit),
            hatchBoundary: static (key, edit) => guard(
                edit.Visible.IsSome || edit.Color.IsSome || edit.PlotColor.IsSome || edit.ColorSource.IsSome
                || edit.PlotColorSource.IsSome || edit.PlotWeightMillimeters.IsSome,
                key.InvalidInput()).ToFin().Map(_ => (AttributeEdit)edit),
            anchorFrame: static (key, edit) => key.AcceptInput(value: edit.Frame).Map(_ => (AttributeEdit)edit),
            anchorMove: static (key, edit) => key.AcceptInput(value: edit.Motion).Map(_ => (AttributeEdit)edit),
            meshing: static (_, edit) => Fin.Succ<AttributeEdit>(edit),
            decals: static (key, edit) =>
                from rosters in MoveShaped(
                    move: edit.Move,
                    seeds: edit.Seeds,
                    admitSeed: seed => seed.Admit(key: key),
                    keys: edit.Crcs,
                    validKey: static crc => crc != 0,
                    key: key)
                select (AttributeEdit)new Decals(Move: edit.Move, Seeds: rosters.Seeds, Crcs: rosters.Keys),
            tag: static (key, edit) =>
                from operation in Optional(edit.Operation).ToFin(Fail: key.InvalidInput())
                from _ in guard(operation.Mutates, key.InvalidInput()).ToFin()
                select (AttributeEdit)new Tag(Operation: operation),
            faceMaterials: static (key, edit) =>
                from rosters in MoveShaped(
                    move: edit.Move,
                    seeds: edit.Seeds,
                    admitSeed: seed => seed.Admit(key: key),
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
        from mode in Optional(move).ToFin(Fail: key.InvalidInput())
        from admitted in mode.Admit(values: values, valid: valid, key: key)
        select build(mode, admitted);

    private static Fin<(Seq<TSeed> Seeds, Seq<TKey> Keys)> MoveShaped<TSeed, TKey>(
        OverrideMove move,
        Seq<TSeed> seeds,
        Func<TSeed, Fin<TSeed>> admitSeed,
        Seq<TKey> keys,
        Func<TKey, bool> validKey,
        Op key) =>
        from mode in Optional(move).ToFin(Fail: key.InvalidInput())
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
            context: (Attributes: attributes, Op: op),
            identity: static (context, edit) => context.Op.Catch(() => {
                _ = edit.Name.Iter(name => context.Attributes.Name = name);
                _ = edit.Url.Iter(url => context.Attributes.Url = url);
                return Fin.Succ(value: unit);
            }),
            layer: static (context, edit) => context.Op.Catch(() => {
                context.Attributes.LayerIndex = edit.Index;
                return Fin.Succ(value: unit);
            }),
            paint: static (context, edit) => context.Op.Catch(() => {
                context.Attributes.ColorSource = edit.Source;
                _ = edit.Value.Iter(color => context.Attributes.ObjectColor = color);
                return Fin.Succ(value: unit);
            }),
            plot: static (context, edit) => context.Op.Catch(() => {
                context.Attributes.PlotColorSource = edit.Source;
                _ = edit.Value.Iter(color => context.Attributes.PlotColor = color);
                return Fin.Succ(value: unit);
            }),
            plotWeight: static (context, edit) => context.Op.Catch(() => {
                context.Attributes.PlotWeightSource = edit.Source;
                _ = edit.Millimeters.Iter(weight => context.Attributes.PlotWeight = weight);
                return Fin.Succ(value: unit);
            }),
            linePattern: static (context, edit) => context.Op.Catch(() => {
                context.Attributes.LinetypeSource = edit.Source;
                _ = edit.Index.Iter(index => context.Attributes.LinetypeIndex = index);
                _ = edit.PatternScale.Iter(scale => context.Attributes.LinetypePatternScale = scale);
                return Fin.Succ(value: unit);
            }),
            customLine: static (context, edit) => context.Op.Catch(() => {
                edit.Pattern.Match(
                    Some: pattern => context.Attributes.SetCustomLinetype(linetype: pattern),
                    None: () => context.Attributes.RemoveCustomLinetype());
                return Fin.Succ(value: unit);
            }),
            materialBind: static (context, edit) => context.Op.Catch(() => {
                context.Attributes.MaterialSource = edit.Source;
                _ = edit.Index.Iter(index => context.Attributes.MaterialIndex = index);
                return Fin.Succ(value: unit);
            }),
            shadows: static (context, edit) => context.Op.Catch(() => {
                context.Attributes.CastsShadows = edit.Casts;
                context.Attributes.ReceivesShadows = edit.Receives;
                return Fin.Succ(value: unit);
            }),
            wires: static (context, edit) => context.Op.Catch(() => {
                context.Attributes.WireDensity = edit.Density;
                return Fin.Succ(value: unit);
            }),
            drawOrder: static (context, edit) => context.Op.Catch(() => {
                context.Attributes.DisplayOrder = edit.Rank;
                return Fin.Succ(value: unit);
            }),
            decorate: static (context, edit) => context.Op.Catch(() => {
                context.Attributes.ObjectDecoration = edit.Ends;
                return Fin.Succ(value: unit);
            }),
            realm: static (context, edit) => context.Op.Catch(() => {
                context.Attributes.Space = edit.Space;
                context.Attributes.ViewportId = edit.Viewport.IfNone(noneValue: Guid.Empty);
                return Fin.Succ(value: unit);
            }),
            groups: static (context, edit) => context.Op.Catch(() => {
                _ = Op.SideWhen(edit.Move == OverrideMove.Impose, context.Attributes.RemoveFromAllGroups);
                _ = edit.Indices.Iter(index => {
                    if (edit.Move == OverrideMove.Retract) {
                        context.Attributes.RemoveFromGroup(groupIndex: index);
                    } else {
                        context.Attributes.AddToGroup(groupIndex: index);
                    }
                });
                return Fin.Succ(value: unit);
            }),
            modeOverride: static (context, edit) => (edit.Mode.Case, edit.Viewport.Case) switch {
                (DisplayModeDescription mode, Guid viewport) => context.Op.Confirm(
                    success: context.Attributes.SetDisplayModeOverride(mode: mode, rhinoViewportId: viewport)),
                (DisplayModeDescription mode, null) => context.Op.Confirm(
                    success: context.Attributes.SetDisplayModeOverride(mode: mode)),
                (null, Guid viewport) => context.Op.Catch(() => {
                    context.Attributes.RemoveDisplayModeOverride(rhinoViewportId: viewport);
                    return Fin.Succ(value: unit);
                }),
                _ => context.Op.Catch(() => {
                    context.Attributes.RemoveDisplayModeOverride();
                    return Fin.Succ(value: unit);
                }),
            },
            detailHide: static (context, edit) => context.Op.Confirm(success: edit.Hidden
                ? context.Attributes.AddHideInDetailOverride(detailId: edit.Detail)
                : context.Attributes.RemoveHideInDetailOverride(detailId: edit.Detail)),
            detailBackground: static (context, edit) => context.Op.Catch(() => {
                context.Attributes.DetailBackgroundVisible = edit.Visible;
                return Fin.Succ(value: unit);
            }),
            activity: static (context, edit) => edit.Move.Switch(
                state: (context.Attributes, context.Op, edit.Viewports, edit.Active),
                impose: static held => held.Op.Confirm(
                    success: held.Attributes.SetActiveInViewportOverrides(viewportIds: held.Viewports.ToArray(), active: held.Active)),
                extend: static held => held.Viewports.TraverseM(viewport => held.Op.Confirm(
                    success: held.Attributes.AddActiveInViewportOverride(viewportId: viewport, active: held.Active))).As()
                    .Map(static _ => unit),
                retract: static held => held.Viewports.TraverseM(viewport => held.Op.Confirm(
                    success: held.Attributes.RemoveActiveInViewportOverride(viewportId: viewport, active: held.Active))).As()
                    .Map(static _ => unit)),
            sectionSource: static (context, edit) => context.Op.Catch(() => {
                context.Attributes.SectionAttributesSource = edit.Source;
                return Fin.Succ(value: unit);
            }),
            sectionIndex: static (context, edit) => context.Op.Catch(() => {
                context.Attributes.SectionStyleIndex = edit.Index;
                return Fin.Succ(value: unit);
            }),
            sectionFace: static (context, edit) => context.Op.Catch(() => {
                edit.Style.Match(
                    Some: style => context.Attributes.SetCustomSectionStyle(sectionStyle: style),
                    None: () => context.Attributes.RemoveCustomSectionStyle());
                return Fin.Succ(value: unit);
            }),
            sectionLabel: static (context, edit) => context.Op.Catch(() => {
                context.Attributes.ClippingPlaneLabelStyle = edit.Style;
                return Fin.Succ(value: unit);
            }),
            hatchFill: static (context, edit) => context.Op.Catch(() => {
                _ = edit.Fill.Iter(color => context.Attributes.HatchBackgroundFillColor = color);
                _ = edit.Print.Iter(color => context.Attributes.HatchBackgroundFillPrintColor = color);
                return Fin.Succ(value: unit);
            }),
            hatchBoundary: static (context, edit) => context.Op.Catch(() => {
                _ = edit.Visible.Iter(visible => context.Attributes.HatchBoundaryVisible = visible);
                _ = edit.Color.Iter(color => context.Attributes.HatchBoundaryColor = color);
                _ = edit.PlotColor.Iter(color => context.Attributes.HatchBoundaryPlotColor = color);
                _ = edit.ColorSource.Iter(source => context.Attributes.HatchBoundaryColorSource = source);
                _ = edit.PlotColorSource.Iter(source => context.Attributes.HatchBoundaryPlotColorSource = source);
                _ = edit.PlotWeightMillimeters.Iter(weight => context.Attributes.HatchBoundaryPlotWeightMillimeters = weight);
                return Fin.Succ(value: unit);
            }),
            anchorFrame: static (context, edit) => context.Op.Catch(() => {
                context.Attributes.SetObjectFrame(plane: edit.Frame);
                return Fin.Succ(value: unit);
            }),
            anchorMove: static (context, edit) => context.Op.Catch(() => {
                context.Attributes.SetObjectFrame(xform: edit.Motion);
                return Fin.Succ(value: unit);
            }),
            meshing: static (context, edit) => context.Op.Catch(() => {
                edit.Custom.Match(
                    Some: parameters => {
                        context.Attributes.CustomMeshingParameters = parameters;
                        context.Attributes.EnableCustomMeshingParameters = true;
                    },
                    None: () => context.Attributes.EnableCustomMeshingParameters = false);
                return Fin.Succ(value: unit);
            }),
            decals: static (context, edit) => edit.Move.Switch(
                state: (context.Attributes, context.Op, edit.Seeds, edit.Crcs),
                impose: static held => held.Op.Catch(() => {
                        held.Attributes.Decals.RemoveAllDecals();
                        return Fin.Succ(value: unit);
                    })
                    .Bind(_ => Grown(attributes: held.Attributes, seeds: held.Seeds, key: held.Op)),
                extend: static held => Grown(attributes: held.Attributes, seeds: held.Seeds, key: held.Op),
                retract: static held => held.Op.Catch(() =>
                    toSeq(held.Attributes.Decals)
                        .Filter(decal => held.Crcs.Exists(crc => crc == decal.CRC))
                        .TraverseM(decal => held.Op.Confirm(success: held.Attributes.Decals.Remove(decal: decal))).As()
                        .Map(static _ => unit))),
            tag: static (context, edit) => edit.Operation.Switch(
                state: (context.Attributes, context.Op),
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
                clear: static (held, _) => held.Op.Catch(() => {
                    held.Attributes.DeleteAllUserStrings();
                    return Fin.Succ(value: unit);
                })),
            faceMaterials: static (context, edit) => edit.Move.Switch(
                state: (context.Attributes, context.Op, edit.Seeds, edit.Keys),
                impose: static held => held.Op.Catch(() => {
                        held.Attributes.MaterialRefs.Clear();
                        return Fin.Succ(value: unit);
                    })
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
                return Fin.Succ(value: unit);
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
                   .TraverseM(edit => Optional(edit).ToFin(Fail: op.InvalidInput())).As()
               from _ in guard(!values.IsEmpty, op.InvalidInput()).ToFin()
               from admitted in values.TraverseM(edit => edit.Admit(op: op)).As()
               select new AttributeProgram(edits: admitted);
    }

    public Fin<Unit> Apply(ObjectAttributes attributes) {
        Op op = Op.Of(name: nameof(AttributeProgram));
        return from working in Optional(attributes).ToFin(Fail: op.InvalidInput())
               from _ in Edits.TraverseM(edit => edit.Apply(attributes: working, op: op)).As()
               select unit;
    }
}
```

## [04]-[SNAPSHOT_AND_EFFECTIVE]

- Owner: `AttributeAsk` `[Union]` — the two read questions: `Stored` the declared attribute state, `Resolved` the source-resolved display values for an optional viewport; `AttributeAnswer` `[Union]` — one typed roster per question. `AttributeSnapshot` captures stored scalar state, group and override rosters, user strings, decal rows with their `CRC` identities, material-reference rows, and carrier-presence facts in one pass. `EffectiveDisplay` captures resolved color and plot values plus the display-mode and activity overrides for the asked viewport.
- Entry: `Attributes.Ask(DocumentSession, TableTarget, AttributeAsk) : Fin<AttributeAnswer>` — one entry resolves through the state page's object fold and reads inside one `SessionNeed.Read` grant.
- Law: stored and effective are different questions — the snapshot reports what the attribute set declares, `EffectiveDisplay` reports what `DrawColor`/`ComputedPlotColor`/`ComputedPlotWeight` resolve after source dispatch against layer, parent, and material; a consumer diffing the two reads exactly which sources defer.
- Law: detail-hide is census membership — `HasHideInDetailOverrideSet(detailId)` is set membership in `GetHideInDetailOverrides()`, so the snapshot's `HiddenInDetails` roster answers any detail id and `EffectiveDisplay` stays a pure viewport question; a detail object id passed where a viewport id belongs is the conflation this split forecloses.
- Law: snapshot products contain detached values only. Decals and material references project their catalogued read surfaces into records; no live carrier crosses the grant. Custom linetype, custom section style, meshing parameters, and mesh modifiers remain presence facts because their value programs belong to their owning pages.
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
            context: op,
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
    Option<(Seq<Guid> Viewports, bool Active)> Activity,
    bool DetailBackgroundVisible);

public readonly record struct DecalSnapshot(
    int Crc,
    DecalMapping Mapping,
    DecalProjection Projection,
    Point3d Origin,
    Vector3d Up,
    Vector3d Across,
    double Transparency,
    bool MapToInside,
    bool Visible,
    double Height,
    double Radius,
    double StartLatitude,
    double EndLatitude,
    double StartLongitude,
    double EndLongitude,
    Guid TextureInstanceId);

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
    ObjectSectionAttributesSource SectionSource,
    System.Drawing.Color ObjectColor,
    System.Drawing.Color PlotColor,
    double PlotWeight,
    double LinetypePatternScale,
    int WireDensity,
    int DisplayOrder,
    ObjectDecoration Decoration,
    bool CastsShadows,
    bool ReceivesShadows,
    Seq<int> Groups,
    OverrideCensus Overrides,
    int SectionStyleIndex,
    SectionLabelStyle SectionLabel,
    bool CustomSectionStyle,
    bool CustomLinetype,
    Plane Frame,
    bool CustomMeshing,
    bool HasMapping,
    bool DefinitionMember,
    Seq<(string Key, string Value)> UserStrings,
    Seq<DecalSnapshot> Decals,
    Seq<MaterialRefSnapshot> MaterialRefs,
    System.Drawing.Color HatchFill,
    System.Drawing.Color HatchPrint,
    bool HatchBoundaryVisible,
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
                SectionSource: attributes.SectionAttributesSource,
                ObjectColor: attributes.ObjectColor,
                PlotColor: attributes.PlotColor,
                PlotWeight: attributes.PlotWeight,
                LinetypePatternScale: attributes.LinetypePatternScale,
                WireDensity: attributes.WireDensity,
                DisplayOrder: attributes.DisplayOrder,
                Decoration: attributes.ObjectDecoration,
                CastsShadows: attributes.CastsShadows,
                ReceivesShadows: attributes.ReceivesShadows,
                Groups: toSeq(attributes.GetGroupList()),
                Overrides: new OverrideCensus(
                    HiddenInDetails: toSeq(attributes.GetHideInDetailOverrides()),
                    Activity: overrides ? Some((toSeq(viewports), active)) : Option<(Seq<Guid>, bool)>.None,
                    DetailBackgroundVisible: attributes.DetailBackgroundVisible),
                SectionStyleIndex: attributes.SectionStyleIndex,
                SectionLabel: attributes.ClippingPlaneLabelStyle,
                CustomSectionStyle: customSection is not null,
                CustomLinetype: customLine is not null,
                Frame: attributes.ObjectFrame(),
                CustomMeshing: attributes.EnableCustomMeshingParameters,
                HasMapping: attributes.HasMapping,
                DefinitionMember: attributes.IsInstanceDefinitionObject,
                UserStrings: toSeq(tags.AllKeys).Choose(key => Optional(key)
                    .Bind(name => Optional(tags[name]).Map(value => (name, value)))),
                Decals: attributes.Decals.AsIterable().Map(static decal => new DecalSnapshot(
                    Crc: decal.CRC,
                    Mapping: decal.Mapping,
                    Projection: decal.Projection,
                    Origin: decal.Origin,
                    Up: decal.VectorUp,
                    Across: decal.VectorAcross,
                    Transparency: decal.Transparency,
                    MapToInside: decal.MapToInside,
                    Visible: decal.IsVisible,
                    Height: decal.Height,
                    Radius: decal.Radius,
                    StartLatitude: decal.StartLatitude,
                    EndLatitude: decal.EndLatitude,
                    StartLongitude: decal.StartLongitude,
                    EndLongitude: decal.EndLongitude,
                    TextureInstanceId: decal.TextureInstanceId)).ToSeq(),
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
                HatchBoundaryVisible: attributes.HatchBoundaryVisible,
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
    Option<bool> ActiveOverride) : IDetachedDocumentResult {
    internal static Fin<EffectiveDisplay> Of(RhinoObject native, Rhino.RhinoDoc document, Option<Guid> viewport, Op key) =>
        key.Catch(() => viewport.Match(
            Some: scoped => {
                ObjectAttributes attributes = native.Attributes;
                bool active = attributes.HasActiveInViewportOverride(viewportId: scoped, active: out bool enabled);
                return Fin.Succ(value: new EffectiveDisplay(
                    Id: native.Id,
                    Draw: attributes.DrawColor(document: document, viewportId: scoped),
                    Plot: attributes.ComputedPlotColor(document: document, viewportId: scoped),
                    PlotWeight: attributes.ComputedPlotWeight(document: document, viewportId: scoped),
                    ModeOverride: attributes.HasDisplayModeOverride(viewportId: scoped)
                        ? Some(attributes.GetDisplayModeOverride(viewportId: scoped))
                        : Option<Guid>.None,
                    ActiveOverride: active ? Some(enabled) : Option<bool>.None));
            },
            None: () => Fin.Succ(value: new EffectiveDisplay(
                Id: native.Id,
                Draw: native.Attributes.DrawColor(document: document),
                Plot: native.Attributes.ComputedPlotColor(document: document),
                PlotWeight: native.Attributes.ComputedPlotWeight(document: document),
                ModeOverride: Option<Guid>.None,
                ActiveOverride: Option<bool>.None))));
}

// --- [OPERATIONS] -------------------------------------------------------------------------
public static class Attributes {
    public static Fin<AttributeAnswer> Ask(DocumentSession session, TableTarget target, AttributeAsk ask) {
        Op op = Op.Of();
        return from active in Optional(ask).ToFin(Fail: op.InvalidInput()).Bind(value => value.Admit(op: op))
               from answer in session.Demand(
                   use: document =>
                       from natives in Objects.Resolve(document: document, target: target, key: op)
                       from folded in active.Switch(
                           context: (Document: document, Natives: natives, Op: op),
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
|  [03]   | detached carriers  | seed records        | `DecalSeed`/`MaterialRefSeed` onto host create params   | `Decals` / `FaceMaterials`     |
|  [04]   | write program      | `AttributeProgram`  | short-circuit fold matching the `Amend` callback shape  | `TableOp.Amend(target, Apply)` |
|  [05]   | read dispatch      | `AttributeAsk`      | stored and resolved questions, one typed answer union   | `Attributes.Ask`               |
|  [06]   | stored state       | `AttributeSnapshot` | detached scalars, rosters, and catalogued carriers      | `AttributeAsk.Stored`          |
|  [07]   | resolved display   | `EffectiveDisplay`  | resolved scalars and viewport overrides                 | `AttributeAsk.Resolved`        |
