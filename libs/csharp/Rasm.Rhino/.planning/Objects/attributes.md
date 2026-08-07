# [RASM_RHINO_OBJECTS_ATTRIBUTES]

Typed attribute mutation belongs to `Rasm.Rhino.Objects`. `AttributeEdit` closes the writable `ObjectAttributes` families with verified payload carriers, parameterizes group, decal, and face-material set operations over detached seeds, and covers layer, source-resolved display, space, overrides, section state, hatch state, frames, meshing, and tags. `AttributeProgram` admits and folds edits over the duplicate supplied by `TableOp.Amend`; this page exposes no local write entry. One `AttributeAsk` owns the read side: `AttributeSnapshot` captures detached scalar and census state, `EffectiveDisplay` resolves document- and viewport-dependent display values.

## [01]-[INDEX]

- [02]-[EDIT_FAMILY]: the keyed attribute-axis owners, `AttributeShade`, `RosterMove`, the detached seed carriers, and the `AttributeEdit` union — the closed mutation vocabulary with its total dispatch.
- [03]-[PROGRAM]: `AttributeProgram` — the fold, the `Amend` handoff, and the write-path law.
- [04]-[SNAPSHOT_AND_EFFECTIVE]: `AttributeAsk`, `AttributeSnapshot`, `EffectiveDisplay`, and the one read entry.
- [05]-[SURFACE_LEDGER]: the page's owner table.

## [02]-[EDIT_FAMILY]

- Owner: `RosterMove<TGrow, TCut>` closes impose, extend, and retract as cases carrying their own payload — grow cases hold value rosters, retract holds identity keys; `ShadowPolicy` owns every cast/receive combination; `DecalSeed` and `MaterialRefSeed` are generated admitted products; `AttributeEdit` `[Union]` owns the assigned stored-attribute mutations and one total `Apply` over the working duplicate.
- Law: no raw host discriminant crosses a signature on this page. Every attribute axis re-closes as a keyed row over its host ordinal — `ColorOrigin`, `PlotColorOrigin`, `PlotWeightOrigin`, `MaterialOrigin`, `SectionOrigin`, `ItemColorOrigin`, `EndDecoration`, `SectionLabel`, `DecalFrame`, `DecalFacing` — and the linetype axis composes `Annotation/linetype.md`'s `LinetypeSource` at BOTH touch points rather than minting a sibling. Each roster mirrors its host enum completely, so a read is a total `Get` and a write is one `.Key`; every SOURCE axis — the four minted here and the composed `LinetypeSource` alike — carries a `FromObject` column, so source-payload coherence is the one `SourceValue` guard reading a row instead of an `is` comparison repeated per admission arm.
- Law: colour is perceptual at the seam and nowhere else. `AttributeShade.Of` admits on read and `AttributeShade.Rgb` quantizes on write, so `System.Drawing.Color` exists only inside those two members: no stored column, no edit payload, and no snapshot field carries it, which closes both the named-colour equality trap and every ad-hoc component fold at once.
- Law: source-dependent payloads admit one coherent product. Object-sourced color, plot color, plot weight, linetype, and material edits require their object value; every other source rejects that irrelevant value. `LinetypePatternScale` remains independent of source and may accompany any line-pattern edit.
- Law: mode and visibility are refused by absence — no case writes `Mode` or `Visible`, because object mode transitions are the table rail's `TableOp.State` and a second write path forks the undo story; `Realm` writes the catalogued space and optional viewport anchor, which no table op carries.
- Law: the space partition is `Document`'s `ActiveSpaceUse`, composed here and re-declared nowhere — the same rows the enumerator's `SpaceFilter`, a conduit criterion, and a gumball seat read — so a `Realm` edit carries a row and writes its `Key`, and the snapshot resolves the host read through the complete roster.
- Law: the three override families are three cases — `ModeOverride` collapses the four display-mode host members onto one `(viewport, mode)` option pair where `None` removes, `DetailHide` is one per-detail toggle, and `Activity` is the `RosterMove`-cased set edit over `SetActiveInViewportOverrides`/`AddActiveInViewportOverride`/`RemoveActiveInViewportOverride`; a per-member sibling verb roster is the deleted form.
- Law: removal is the `None` arm of the same case — `CustomLine`, `SectionFace`, and `Meshing` remove their custom carrier when absent and install it when present, and `SectionIndex` writes the host's absent sentinel when `None`; `Meshing` admits and normalizes `MeshingParameters.ToEncodedString()` output once, then reconstructs the disposable carrier only inside `Apply`.
- Law: display-mode override carries the mode id and resolves it with `DisplayModeDescription.GetDisplayMode(Guid)` inside `Apply`; an unresolved id is typed failure, never a retained `DisplayModeDescription`.
- Law: `RenderingReset` is the sole rendering-attribute clear and composes `ClearRenderingAttributes`; piecemeal reset calls never compete with it.
- Law: user strings reuse the document vocabulary — the `Tag` case carries the geometry page's `TagOp` and admits only its mutating verbs, applied against the attribute set's own user-string store; a read verb inside a mutation program is refused at the factory.
- Law: groups, viewport activity, decals, and face materials share one `RosterMove` grammar without sharing payload identity — each case carries exactly its own roster, `Impose` and `Extend` the grow values and `Retract` the identity keys, so no edit holds a sibling case's roster and coherence needs no guard. `Impose` clears then installs, `Extend` and `Retract` require a non-empty roster, empty `Impose` is the sole clear form, and every roster admits distinct.
- Law: decal and face-material payloads are detached seeds, never live carriers — `DecalSeed.Build` fills a `DecalCreateParams` for `Decal.Create` and `MaterialRefSeed.Build` fills a `MaterialRefCreateParams` for `MaterialRefs.Create`, each minted and released inside its apply arm, because a live `Decal` or `MaterialRef` enumerated under an earlier grant is host state whose mutation does not persist.
- Law: decal removal keys on `Decal.CRC` — the host removes by decal identity, so retract carries the snapshot's `Crc` column and the arm removes every live decal whose `CRC` matches; face-material removal keys on the plug-in guid the dictionary indexes.
- Law: host quirks cross verbatim — `DecalCreateParams.StartLatitude`/`EndLatitude` carry the horizontal sweep and `StartLongitude`/`EndLongitude` the vertical (the host inverts the names), and `MaterialRefs.Create` swaps front and back values across its native boundary; neither is locally corrected, so a host repair never double-swaps.
- Growth: a new writable axis adds one edit case, one admission arm, one apply arm, and its detached read projection when the page owns that read.

```csharp signature
// --- [RUNTIME_PRELUDE] --------------------------------------------------------------------
// `Rasm.Numerics` carries the kernel colour owner, so every host colour on this page spells
// `System.Drawing.Color` in full and appears only inside the two crossing helpers.
using Rasm.Domain;
using Rasm.Numerics;
using Rasm.Rhino.Annotation;
using Rasm.Rhino.Document;
using Rhino.DocObjects;
using Rhino.Display;
using Rhino.FileIO;
using Rhino.Geometry;
using Rhino.Render;

namespace Rasm.Rhino.Objects;

// --- [TYPES] ------------------------------------------------------------------------------
// Every host attribute discriminant re-closes as a keyed row keyed on the host ordinal, so a raw host enum never
// crosses a public signature, a roster that grows in a Rhino release refuses at admission instead of widening
// silently, and each source axis reads its own `FromObject` question off a column rather than an `is` comparison
// repeated at every admission and apply arm. `ObjectLinetypeSource` is the one axis that does NOT mint here:
// `Annotation/linetype.md` already owns `LinetypeSource`, so this page composes it at both touch points.
[SmartEnum<ObjectColorSource>]
public sealed partial class ColorOrigin {
    public static readonly ColorOrigin ByLayer = new(key: ObjectColorSource.ColorFromLayer, fromObject: false);
    public static readonly ColorOrigin ByObject = new(key: ObjectColorSource.ColorFromObject, fromObject: true);
    public static readonly ColorOrigin ByMaterial = new(key: ObjectColorSource.ColorFromMaterial, fromObject: false);
    public static readonly ColorOrigin ByParent = new(key: ObjectColorSource.ColorFromParent, fromObject: false);

    internal bool FromObject { get; }
}

[SmartEnum<ObjectPlotColorSource>]
public sealed partial class PlotColorOrigin {
    public static readonly PlotColorOrigin ByLayer = new(key: ObjectPlotColorSource.PlotColorFromLayer, fromObject: false);
    public static readonly PlotColorOrigin ByObject = new(key: ObjectPlotColorSource.PlotColorFromObject, fromObject: true);
    public static readonly PlotColorOrigin ByDisplay = new(key: ObjectPlotColorSource.PlotColorFromDisplay, fromObject: false);
    public static readonly PlotColorOrigin ByParent = new(key: ObjectPlotColorSource.PlotColorFromParent, fromObject: false);

    internal bool FromObject { get; }
}

[SmartEnum<ObjectPlotWeightSource>]
public sealed partial class PlotWeightOrigin {
    public static readonly PlotWeightOrigin ByLayer = new(key: ObjectPlotWeightSource.PlotWeightFromLayer, fromObject: false);
    public static readonly PlotWeightOrigin ByObject = new(key: ObjectPlotWeightSource.PlotWeightFromObject, fromObject: true);
    public static readonly PlotWeightOrigin ByParent = new(key: ObjectPlotWeightSource.PlotWeightFromParent, fromObject: false);

    internal bool FromObject { get; }
}

[SmartEnum<ObjectMaterialSource>]
public sealed partial class MaterialOrigin {
    public static readonly MaterialOrigin ByLayer = new(key: ObjectMaterialSource.MaterialFromLayer, fromObject: false);
    public static readonly MaterialOrigin ByObject = new(key: ObjectMaterialSource.MaterialFromObject, fromObject: true);
    public static readonly MaterialOrigin ByParent = new(key: ObjectMaterialSource.MaterialFromParent, fromObject: false);

    internal bool FromObject { get; }
}

[SmartEnum<ObjectSectionAttributesSource>]
public sealed partial class SectionOrigin {
    public static readonly SectionOrigin ByLayer = new(key: ObjectSectionAttributesSource.FromLayer);
    public static readonly SectionOrigin ByObject = new(key: ObjectSectionAttributesSource.FromObject);
    public static readonly SectionOrigin ByParent = new(key: ObjectSectionAttributesSource.FromParent);
    public static readonly SectionOrigin BySectioner = new(key: ObjectSectionAttributesSource.FromSectioner);
}

[SmartEnum<ItemColorSource>]
public sealed partial class ItemColorOrigin {
    public static readonly ItemColorOrigin ByLayer = new(key: ItemColorSource.ColorFromLayer);
    public static readonly ItemColorOrigin ByObject = new(key: ItemColorSource.ColorFromObject);
    public static readonly ItemColorOrigin ByParent = new(key: ItemColorSource.ColorFromParent);
    public static readonly ItemColorOrigin Custom = new(key: ItemColorSource.ColorCustom);
}

[SmartEnum<ObjectDecoration>]
public sealed partial class EndDecoration {
    public static readonly EndDecoration None = new(key: ObjectDecoration.None);
    public static readonly EndDecoration Start = new(key: ObjectDecoration.StartArrowhead);
    public static readonly EndDecoration End = new(key: ObjectDecoration.EndArrowhead);
    public static readonly EndDecoration Both = new(key: ObjectDecoration.BothArrowhead);
}

[SmartEnum<SectionLabelStyle>]
public sealed partial class SectionLabel {
    public static readonly SectionLabel None = new(key: SectionLabelStyle.None);
    public static readonly SectionLabel Text = new(key: SectionLabelStyle.TextFromName);
    public static readonly SectionLabel Dot = new(key: SectionLabelStyle.TextDotFromName);
}

[SmartEnum<DecalMapping>]
public sealed partial class DecalFrame {
    public static readonly DecalFrame None = new(key: DecalMapping.None);
    public static readonly DecalFrame Planar = new(key: DecalMapping.Planar);
    public static readonly DecalFrame Cylindrical = new(key: DecalMapping.Cylindrical);
    public static readonly DecalFrame Spherical = new(key: DecalMapping.Spherical);
    public static readonly DecalFrame Uv = new(key: DecalMapping.UV);
}

[SmartEnum<DecalProjection>]
public sealed partial class DecalFacing {
    public static readonly DecalFacing None = new(key: DecalProjection.None);
    public static readonly DecalFacing Forward = new(key: DecalProjection.Forward);
    public static readonly DecalFacing Backward = new(key: DecalProjection.Backward);
    public static readonly DecalFacing Both = new(key: DecalProjection.Both);
}

// The page's TWO host-colour crossings. `System.Drawing.Color` reaches the boundary as the byte quadruple a host
// read answers with and leaves as the quadruple a host write takes; no stored column and no public signature
// carries it, so the named-colour equality trap and the sRGB component arithmetic both die at this seam.
public static class AttributeShade {
    internal static Fin<PerceptualColor> Of(System.Drawing.Color color, Op key) =>
        PerceptualColor.OfRgb(color.R, color.G, color.B, alpha: color.A, key: key);

    internal static System.Drawing.Color Rgb(PerceptualColor shade) =>
        shade.ToRgb() switch {
            var (red, green, blue, alpha) => System.Drawing.Color.FromArgb(alpha: alpha, red: red, green: green, blue: blue),
        };
}

// Payload rides the case, so a grow move CARRIES values and a retract move CARRIES keys — no arm sniffs a
// sibling roster and no move/payload coherence guard exists to drift. Hand-rolled with a private constructor
// because the generated union stamps `allows ref struct` onto its own type parameters, which `Seq<T>` payloads
// refuse; the terminal switch arm is the closed-set discharge the generator would otherwise prove.
public abstract record RosterMove<TGrow, TCut> {
    private RosterMove() { }
    public sealed record Impose(Seq<TGrow> Values) : RosterMove<TGrow, TCut>;
    public sealed record Extend(Seq<TGrow> Values) : RosterMove<TGrow, TCut>;
    public sealed record Retract(Seq<TCut> Keys) : RosterMove<TGrow, TCut>;

    internal Fin<RosterMove<TGrow, TCut>> Admit(Func<TGrow, bool> grow, Func<TCut, bool> cut, Op key) => this switch {
        Impose(var values) => Roster(values: values, valid: grow, floor: 0, key: key)
            .Map(static admitted => (RosterMove<TGrow, TCut>)new Impose(Values: admitted)),
        Extend(var values) => Roster(values: values, valid: grow, floor: 1, key: key)
            .Map(static admitted => (RosterMove<TGrow, TCut>)new Extend(Values: admitted)),
        Retract(var keys) => Roster(values: keys, valid: cut, floor: 1, key: key)
            .Map(static admitted => (RosterMove<TGrow, TCut>)new Retract(Keys: admitted)),
        _ => Fin.Fail<RosterMove<TGrow, TCut>>(error: key.InvalidInput()),
    };

    private static Fin<Seq<T>> Roster<T>(Seq<T> values, Func<T, bool> valid, int floor, Op key) =>
        from roster in values.TraverseM(value => valid(value)
            ? Fin.Succ(value: value)
            : Fin.Fail<T>(error: key.InvalidInput())).As()
        let admitted = roster.Distinct()
        from _ in guard(admitted.Count >= floor, key.InvalidInput()).ToFin()
        select admitted;
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
    public DecalFrame Mapping { get; }
    public DecalFacing Projection { get; }
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
        ref DecalFrame mapping,
        ref DecalFacing projection,
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
            && mapping is not null && projection is not null
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
        DecalMapping = Mapping.Key,
        DecalProjection = Projection.Key,
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
    public MaterialOrigin Source { get; }
    public Guid FrontId { get; }
    public Guid BackId { get; }
    public int FrontIndex { get; }
    public int BackIndex { get; }

    [BoundaryAdapter]
    static partial void ValidateFactoryArguments(
        ref ValidationError? validationError,
        ref Guid plugIn,
        ref MaterialOrigin source,
        ref Guid frontId,
        ref Guid backId,
        ref int frontIndex,
        ref int backIndex) {
        validationError = plugIn != Guid.Empty
            && source is not null
            && frontIndex >= -1 && backIndex >= -1
            && (frontId != Guid.Empty || frontIndex >= 0)
            && (backId != Guid.Empty || backIndex >= 0)
            ? validationError
            : new ValidationError(message: "material reference seed is invalid");
    }

    internal MaterialRefCreateParams Build() => new() {
        PlugInId = PlugIn,
        MaterialSource = Source.Key,
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
    public sealed record Layer(ResourceIndex Index) : AttributeEdit;
    public sealed record Paint(ColorOrigin Source, Option<PerceptualColor> Value = default) : AttributeEdit;
    public sealed record Plot(PlotColorOrigin Source, Option<PerceptualColor> Value = default) : AttributeEdit;
    public sealed record PlotWeight(PlotWeightOrigin Source, Option<double> Millimeters = default) : AttributeEdit;
    public sealed record LinePattern(LinetypeSource Source, Option<ResourceIndex> Index = default, Option<double> PatternScale = default) : AttributeEdit;
    public sealed record CustomLine(Option<Linetype> Pattern) : AttributeEdit;
    public sealed record MaterialBind(MaterialOrigin Source, Option<ResourceIndex> Index = default) : AttributeEdit;
    public sealed record Shadows(ShadowPolicy Policy) : AttributeEdit;
    public sealed record Wires(int Density) : AttributeEdit;
    public sealed record DrawOrder(int Rank) : AttributeEdit;
    public sealed record Decorate(EndDecoration Ends) : AttributeEdit;
    public sealed record Realm(ActiveSpaceUse Space, Option<Guid> Viewport = default) : AttributeEdit;
    public sealed record Groups(RosterMove<int, int> Move) : AttributeEdit;
    public sealed record ModeOverride(Option<Guid> Viewport, Option<Guid> Mode) : AttributeEdit;
    public sealed record DetailHide(Guid Detail, ObjectSignal Signal) : AttributeEdit;
    public sealed record DetailBackground(ObjectSignal Signal) : AttributeEdit;
    public sealed record Activity(RosterMove<Guid, Guid> Move, ObjectSignal Signal) : AttributeEdit;
    public sealed record SectionSource(SectionOrigin Source) : AttributeEdit;
    public sealed record SectionIndex(Option<ResourceIndex> Index = default) : AttributeEdit;
    public sealed record SectionFace(Option<SectionStyle> Style) : AttributeEdit;
    public sealed record Label(SectionLabel Style) : AttributeEdit;
    public sealed record HatchFill(Option<PerceptualColor> Fill = default, Option<PerceptualColor> Print = default) : AttributeEdit;
    public sealed record HatchBoundary(
        Option<ObjectSignal> Visible = default,
        Option<PerceptualColor> Color = default,
        Option<PerceptualColor> PlotColor = default,
        Option<ItemColorOrigin> ColorSource = default,
        Option<ItemColorOrigin> PlotColorSource = default,
        Option<double> PlotWeightMillimeters = default) : AttributeEdit;
    public sealed record AnchorFrame(Plane Frame) : AttributeEdit;
    public sealed record AnchorMove(Transform Motion) : AttributeEdit;
    public sealed record Meshing(Option<string> Encoded) : AttributeEdit;
    public sealed record RenderingReset : AttributeEdit;
    public sealed record Decals(RosterMove<DecalSeed, int> Move) : AttributeEdit;
    public sealed record Tag(TagOp Operation) : AttributeEdit;
    public sealed record FaceMaterials(RosterMove<MaterialRefSeed, Guid> Move) : AttributeEdit;

    internal Fin<AttributeEdit> Admit(Op op) =>
        Switch(
            op,
            identity: static (key, edit) =>
                from name in edit.Name.Traverse(text => key.AcceptText(value: text)).As()
                from url in edit.Url.Traverse(text => key.AcceptText(value: text)).As()
                from _ in guard(name.IsSome || url.IsSome, key.InvalidInput()).ToFin()
                select (AttributeEdit)new Identity(Name: name, Url: url),
            layer: static (_, edit) => Fin.Succ<AttributeEdit>(edit),
            paint: static (key, edit) => key.Need(edit.Source)
                .Bind(source => SourceValue(source.FromObject, edit.Value, edit, key)),
            plot: static (key, edit) => key.Need(edit.Source)
                .Bind(source => SourceValue(source.FromObject, edit.Value, edit, key)),
            plotWeight: static (key, edit) =>
                from source in key.Need(edit.Source)
                from admitted in SourceValue(source.FromObject, edit.Millimeters, edit, key)
                from _ in guard(edit.Millimeters
                    .Map(static value => double.IsFinite(value) && value >= 0.0)
                    .IfNone(noneValue: true), key.InvalidInput()).ToFin()
                select admitted,
            // `LinetypeSource` is Annotation's owner composed, not a second wrap: the linetype axis has ONE
            // vocabulary and this page reads its `FromObject` column exactly as it reads the four minted axes.
            linePattern: static (key, edit) =>
                from source in key.Need(edit.Source)
                from admitted in SourceValue(source.FromObject, edit.Index, edit, key)
                from _ in guard(edit.PatternScale
                    .Map(static value => double.IsFinite(value) && value > 0.0)
                    .IfNone(noneValue: true), key.InvalidInput()).ToFin()
                select admitted,
            customLine: static (_, edit) => Fin.Succ<AttributeEdit>(edit),
            materialBind: static (key, edit) => key.Need(edit.Source)
                .Bind(source => SourceValue(source.FromObject, edit.Index, edit, key)),
            shadows: static (key, edit) => key.Need(edit.Policy).Map(_ => (AttributeEdit)edit),
            wires: static (_, edit) => Fin.Succ<AttributeEdit>(edit),
            drawOrder: static (_, edit) => Fin.Succ<AttributeEdit>(edit),
            decorate: static (key, edit) => key.Need(edit.Ends).Map(_ => (AttributeEdit)edit),
            realm: static (key, edit) => guard(
                edit.Space is not null && edit.Space != ActiveSpaceUse.None
                && edit.Viewport.Map(static value => value != Guid.Empty).IfNone(noneValue: true),
                key.InvalidInput()).ToFin().Map(_ => (AttributeEdit)edit),
            groups: static (key, edit) => key.Need(edit.Move)
                .Bind(move => move.Admit(grow: static index => index >= 0, cut: static index => index >= 0, key: key))
                .Map(static move => (AttributeEdit)new Groups(Move: move)),
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
                from move in key.Need(edit.Move)
                from admitted in move.Admit(grow: static id => id != Guid.Empty, cut: static id => id != Guid.Empty, key: key)
                select (AttributeEdit)new Activity(Move: admitted, Signal: signal),
            sectionSource: static (key, edit) => key.Need(edit.Source).Map(_ => (AttributeEdit)edit),
            sectionIndex: static (_, edit) => Fin.Succ<AttributeEdit>(edit),
            sectionFace: static (_, edit) => Fin.Succ<AttributeEdit>(edit),
            label: static (key, edit) => key.Need(edit.Style).Map(_ => (AttributeEdit)edit),
            hatchFill: static (key, edit) => guard(edit.Fill.IsSome || edit.Print.IsSome, key.InvalidInput()).ToFin().Map(_ => (AttributeEdit)edit),
            // `Option` payloads admit no null value, so presence and the weight bound are the whole admission.
            hatchBoundary: static (key, edit) =>
                from _ in guard(
                    edit.Visible.IsSome || edit.Color.IsSome || edit.PlotColor.IsSome || edit.ColorSource.IsSome
                    || edit.PlotColorSource.IsSome || edit.PlotWeightMillimeters.IsSome,
                    key.InvalidInput()).ToFin()
                from __ in guard(edit.PlotWeightMillimeters
                    .Map(static value => double.IsFinite(value) && value >= 0.0)
                    .IfNone(noneValue: true), key.InvalidInput()).ToFin()
                select (AttributeEdit)edit,
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
            decals: static (key, edit) => key.Need(edit.Move)
                .Bind(move => move.Admit(grow: static seed => seed is not null, cut: static crc => crc != 0, key: key))
                .Map(static move => (AttributeEdit)new Decals(Move: move)),
            tag: static (key, edit) =>
                from operation in key.Need(edit.Operation)
                from _ in guard(operation.Mutates, key.InvalidInput()).ToFin()
                select (AttributeEdit)edit,
            faceMaterials: static (key, edit) => key.Need(edit.Move)
                .Bind(move => move.Admit(grow: static seed => seed is not null, cut: static plugin => plugin != Guid.Empty, key: key))
                .Map(static move => (AttributeEdit)new FaceMaterials(Move: move)));

    private static Fin<AttributeEdit> SourceValue<TValue>(bool requires, Option<TValue> value, AttributeEdit edit, Op key) =>
        guard(requires == value.IsSome, key.InvalidInput()).ToFin().Map(_ => edit);

    internal Fin<Unit> Apply(ObjectAttributes attributes, Op op) =>
        Switch(
            (Attributes: attributes, Op: op),
            identity: static (context, edit) => context.Op.Catch(() => {
                _ = edit.Name.Iter(name => context.Attributes.Name = name);
                _ = edit.Url.Iter(url => context.Attributes.Url = url);
            }),
            layer: static (context, edit) => context.Op.Catch(() => context.Attributes.LayerIndex = edit.Index.Value),
            paint: static (context, edit) => context.Op.Catch(() => {
                context.Attributes.ColorSource = edit.Source.Key;
                _ = edit.Value.Iter(shade => context.Attributes.ObjectColor = AttributeShade.Rgb(shade: shade));
            }),
            plot: static (context, edit) => context.Op.Catch(() => {
                context.Attributes.PlotColorSource = edit.Source.Key;
                _ = edit.Value.Iter(shade => context.Attributes.PlotColor = AttributeShade.Rgb(shade: shade));
            }),
            plotWeight: static (context, edit) => context.Op.Catch(() => {
                context.Attributes.PlotWeightSource = edit.Source.Key;
                _ = edit.Millimeters.Iter(weight => context.Attributes.PlotWeight = weight);
            }),
            linePattern: static (context, edit) => context.Op.Catch(() => {
                context.Attributes.LinetypeSource = edit.Source.Key;
                _ = edit.Index.Iter(index => context.Attributes.LinetypeIndex = index.Value);
                _ = edit.PatternScale.Iter(scale => context.Attributes.LinetypePatternScale = scale);
            }),
            customLine: static (context, edit) => context.Op.Catch(() => {
                edit.Pattern.Match(
                    Some: pattern => context.Attributes.SetCustomLinetype(linetype: pattern),
                    None: () => context.Attributes.RemoveCustomLinetype());
            }),
            materialBind: static (context, edit) => context.Op.Catch(() => {
                context.Attributes.MaterialSource = edit.Source.Key;
                _ = edit.Index.Iter(index => context.Attributes.MaterialIndex = index.Value);
            }),
            shadows: static (context, edit) => context.Op.Catch(() => {
                context.Attributes.CastsShadows = edit.Policy.Casts;
                context.Attributes.ReceivesShadows = edit.Policy.Receives;
            }),
            wires: static (context, edit) => context.Op.Catch(() => context.Attributes.WireDensity = edit.Density),
            drawOrder: static (context, edit) => context.Op.Catch(() => context.Attributes.DisplayOrder = edit.Rank),
            decorate: static (context, edit) => context.Op.Catch(() => context.Attributes.ObjectDecoration = edit.Ends.Key),
            realm: static (context, edit) => context.Op.Catch(() => {
                context.Attributes.Space = edit.Space.Key;
                context.Attributes.ViewportId = edit.Viewport.IfNone(noneValue: Guid.Empty);
            }),
            groups: static (context, edit) => edit.Move switch {
                RosterMove<int, int>.Impose(var indices) => context.Op.Catch(() => {
                    context.Attributes.RemoveFromAllGroups();
                    _ = indices.Iter(index => context.Attributes.AddToGroup(groupIndex: index));
                }),
                RosterMove<int, int>.Extend(var indices) => context.Op.Catch(() =>
                    _ = indices.Iter(index => context.Attributes.AddToGroup(groupIndex: index))),
                RosterMove<int, int>.Retract(var indices) => context.Op.Catch(() =>
                    _ = indices.Iter(index => context.Attributes.RemoveFromGroup(groupIndex: index))),
                _ => Fin.Fail<Unit>(error: context.Op.InvalidInput()),
            },
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
            activity: static (context, edit) => edit.Move switch {
                RosterMove<Guid, Guid>.Impose(var viewports) => context.Op.Confirm(
                    success: context.Attributes.SetActiveInViewportOverrides(viewportIds: viewports.ToArray(), active: edit.Signal.On)),
                RosterMove<Guid, Guid>.Extend(var viewports) => viewports.TraverseM(viewport => context.Op.Confirm(
                    success: context.Attributes.AddActiveInViewportOverride(viewportId: viewport, active: edit.Signal.On))).As()
                    .Map(static _ => unit),
                RosterMove<Guid, Guid>.Retract(var viewports) => viewports.TraverseM(viewport => context.Op.Confirm(
                    success: context.Attributes.RemoveActiveInViewportOverride(viewportId: viewport, active: edit.Signal.On))).As()
                    .Map(static _ => unit),
                _ => Fin.Fail<Unit>(error: context.Op.InvalidInput()),
            },
            sectionSource: static (context, edit) => context.Op.Catch(() => context.Attributes.SectionAttributesSource = edit.Source.Key),
            sectionIndex: static (context, edit) => context.Op.Catch(() =>
                context.Attributes.SectionStyleIndex = edit.Index.Map(static index => index.Value).IfNone(noneValue: ResourceIndex.Absent)),
            sectionFace: static (context, edit) => context.Op.Catch(() => {
                edit.Style.Match(
                    Some: style => context.Attributes.SetCustomSectionStyle(sectionStyle: style),
                    None: () => context.Attributes.RemoveCustomSectionStyle());
            }),
            label: static (context, edit) => context.Op.Catch(() => context.Attributes.ClippingPlaneLabelStyle = edit.Style.Key),
            hatchFill: static (context, edit) => context.Op.Catch(() => {
                _ = edit.Fill.Iter(shade => context.Attributes.HatchBackgroundFillColor = AttributeShade.Rgb(shade: shade));
                _ = edit.Print.Iter(shade => context.Attributes.HatchBackgroundFillPrintColor = AttributeShade.Rgb(shade: shade));
            }),
            hatchBoundary: static (context, edit) => context.Op.Catch(() => {
                _ = edit.Visible.Iter(signal => context.Attributes.HatchBoundaryVisible = signal.On);
                _ = edit.Color.Iter(shade => context.Attributes.HatchBoundaryColor = AttributeShade.Rgb(shade: shade));
                _ = edit.PlotColor.Iter(shade => context.Attributes.HatchBoundaryPlotColor = AttributeShade.Rgb(shade: shade));
                _ = edit.ColorSource.Iter(source => context.Attributes.HatchBoundaryColorSource = source.Key);
                _ = edit.PlotColorSource.Iter(source => context.Attributes.HatchBoundaryPlotColorSource = source.Key);
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
            decals: static (context, edit) => edit.Move switch {
                RosterMove<DecalSeed, int>.Impose(var seeds) => context.Op.Catch(() => context.Attributes.Decals.RemoveAllDecals())
                    .Bind(_ => Grown(attributes: context.Attributes, seeds: seeds, key: context.Op)),
                RosterMove<DecalSeed, int>.Extend(var seeds) => Grown(attributes: context.Attributes, seeds: seeds, key: context.Op),
                RosterMove<DecalSeed, int>.Retract(var crcs) => context.Op.Catch(() =>
                    toSeq(context.Attributes.Decals)
                        .Filter(decal => crcs.Exists(crc => crc == decal.CRC))
                        .TraverseM(decal => context.Op.Confirm(success: context.Attributes.Decals.Remove(decal: decal))).As()
                        .Map(static _ => unit)),
                _ => Fin.Fail<Unit>(error: context.Op.InvalidInput()),
            },
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
            faceMaterials: static (context, edit) => edit.Move switch {
                RosterMove<MaterialRefSeed, Guid>.Impose(var seeds) => context.Op.Catch(() => context.Attributes.MaterialRefs.Clear())
                    .Bind(_ => Bound(attributes: context.Attributes, seeds: seeds, key: context.Op)),
                RosterMove<MaterialRefSeed, Guid>.Extend(var seeds) => Bound(attributes: context.Attributes, seeds: seeds, key: context.Op),
                RosterMove<MaterialRefSeed, Guid>.Retract(var plugins) => plugins.TraverseM(plugin => context.Op.Confirm(
                    success: context.Attributes.MaterialRefs.Remove(key: plugin))).As().Map(static _ => unit),
                _ => Fin.Fail<Unit>(error: context.Op.InvalidInput()),
            });

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
- Law: the program IS the `Amend` payload — `TableOp.Amend(target, program.Change, interaction)` is the one write path, where `Change` is the spine's `AttributeChange` value the program mints from its own fold: the table rail duplicates the live attribute set, the program mutates the duplicate, `ModifyAttributes` commits it under the undo bracket, and the duplicate disposes before the operation leaves the host boundary; a consumer holding a live `ObjectAttributes` and mutating it in place has no undo story and is the deleted form. `Apply` is therefore `internal` — the rail is its only caller, and a public overload would be that deleted form with a supported spelling.
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
        return from admitted in LanguageExt.Iterable<AttributeEdit>.FromSpan(edits).ToSeq()
                   .TraverseM(edit => op.Need(edit).Bind(value => value.Admit(op: op))).As()
               from _ in guard(!admitted.IsEmpty, op.InvalidInput()).ToFin()
               select new AttributeProgram(edits: admitted);
    }

    // The program's egress is the spine's `AttributeChange`, not a bare delegate: `Commands` and `Objects` both
    // sit above `Document`, so the payload TYPE seats on the spine and this owner composes it upward. `Apply`
    // stays `internal` because the change value is the only thing that leaves — a public `Apply` would be exactly
    // the in-place live-set mutation the write-path law deletes, wearing a supported spelling.
    public Fin<AttributeChange> Change =>
        AttributeChange.Validate(Apply, out AttributeChange? admitted) is null && admitted is not null
            ? Fin.Succ(value: admitted)
            : Fin.Fail<AttributeChange>(error: Op.Of(name: nameof(AttributeProgram)).InvalidInput());

    internal Fin<Unit> Apply(ObjectAttributes attributes) {
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
- Law: render-material identity is NOT an attribute-set read — `ObjectAttributes.RenderMaterial` is a SET-ONLY property whose setter resolves the content's document owner, projects it to a `Material`, and lands it on `MaterialIndex`, so the stored render-material identity reads through `MaterialIndex` against the document material table on `materials.md`'s resolution rail; a read of the write-only property is a compile error wearing the shape of a projection.
- Law: the decal read composes the axes that READ TRUE — `HorzSweep`/`VertSweep` are the host's own replacements for the deprecated latitude and longitude properties, whose names invert their meaning, so the snapshot names its columns after the true axes and matches `DecalSeed`'s write vocabulary exactly; a round trip therefore requires no consumer to know the host's inversion, which stays confined to `DecalSeed.Build`.
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
    DecalFrame Mapping,
    DecalFacing Projection,
    Point3d Origin,
    Vector3d Up,
    Vector3d Across,
    double Transparency,
    ObjectSignal MapToInside,
    bool Visible,
    double Height,
    double Radius,
    double HorzStart,
    double HorzEnd,
    double VertStart,
    double VertEnd,
    double MinU,
    double MinV,
    double MaxU,
    double MaxV,
    Guid TextureInstanceId) {
    // Railed for the HOST CALL, not for the axis reads: `GetUVBounds`/`HorzSweep`/`VertSweep` are three native
    // crossings the bracket owns. Both axis rows mirror their host enum completely, so `Get` is total over
    // anything the host returns — the same total-roster property the space partition already declares.
    internal static Fin<DecalSnapshot> Of(Decal decal, Op key) => key.Catch(() => {
        decal.GetUVBounds(out double minU, out double minV, out double maxU, out double maxV);
        decal.HorzSweep(out double horzStart, out double horzEnd);
        decal.VertSweep(out double vertStart, out double vertEnd);
        return Fin.Succ(value: new DecalSnapshot(
            Crc: decal.CRC,
            Mapping: DecalFrame.Get(key: decal.Mapping),
            Projection: DecalFacing.Get(key: decal.Projection),
            Origin: decal.Origin,
            Up: decal.VectorUp,
            Across: decal.VectorAcross,
            Transparency: decal.Transparency,
            MapToInside: ObjectSignal.Of(on: decal.MapToInside),
            Visible: decal.IsVisible,
            Height: decal.Height,
            Radius: decal.Radius,
            HorzStart: horzStart,
            HorzEnd: horzEnd,
            VertStart: vertStart,
            VertEnd: vertEnd,
            MinU: minU,
            MinV: minV,
            MaxU: maxU,
            MaxV: maxV,
            TextureInstanceId: decal.TextureInstanceId);
    });
}

public readonly record struct MaterialRefSnapshot(
    Guid DictionaryKey,
    MaterialOrigin Source,
    Guid PlugInId,
    Guid FrontId,
    Guid BackId,
    int FrontIndex,
    int BackIndex);

public sealed record AttributeSnapshot(
    Guid ObjectId,
    Option<string> Name,
    Option<string> Url,
    ResourceIndex LayerIndex,
    Option<ResourceIndex> LinetypeIndex,
    Option<ResourceIndex> MaterialIndex,
    Guid ViewportId,
    ActiveSpaceUse Space,
    ColorOrigin ColorSource,
    PlotColorOrigin PlotColorSource,
    PlotWeightOrigin PlotWeightSource,
    LinetypeSource LineSource,
    MaterialOrigin MaterialSource,
    SectionOrigin SectionSource,
    PerceptualColor ObjectColor,
    PerceptualColor PlotColor,
    double PlotWeight,
    double LinetypePatternScale,
    int WireDensity,
    int DisplayOrder,
    EndDecoration Decoration,
    ShadowPolicy Shadows,
    Seq<int> Groups,
    OverrideCensus Overrides,
    Option<ResourceIndex> SectionStyleIndex,
    SectionLabel Label,
    bool CustomSectionStyle,
    bool CustomLinetype,
    Plane Frame,
    Option<string> Meshing,
    bool HasMapping,
    bool DefinitionMember,
    HashMap<string, string> UserStrings,
    Seq<DecalSnapshot> Decals,
    Seq<MaterialRefSnapshot> MaterialRefs,
    PerceptualColor HatchFill,
    PerceptualColor HatchPrint,
    ObjectSignal HatchBoundaryVisible,
    PerceptualColor HatchBoundaryColor,
    PerceptualColor HatchBoundaryPlotColor,
    ItemColorOrigin HatchBoundaryColorSource,
    ItemColorOrigin HatchBoundaryPlotColorSource,
    double HatchBoundaryPlotWeight,
    (bool Displacement, bool EdgeSoftening, bool Thickening, bool CurvePiping, bool ShutLining) Modifiers)
    : IDetachedDocumentResult {
    internal static Fin<AttributeSnapshot> Of(ObjectAttributes attributes, Op key) =>
        key.Catch(() => {
            bool overrides = attributes.GetActiveInViewportOverrides(viewportIds: out Guid[] viewports, active: out bool active);
            File3dmMeshModifiers modifiers = attributes.File3dmMeshModifiers;
            using SectionStyle? customSection = attributes.GetCustomSectionStyle();
            using Linetype? customLine = attributes.GetCustomLinetype();
            // No `using`: the getter hands back a wrapper over the attribute set's OWN stored parameters and
            // `MeshingParameters.Dispose` unconditionally frees the pointer it holds, so bracketing this read
            // would free host-owned memory the attribute set still indexes. The encoded value detaches instead.
            MeshingParameters? customMesh = attributes.EnableCustomMeshingParameters
                ? attributes.CustomMeshingParameters
                : null;
            // Each keyed row mirrors its host enum COMPLETELY, so `Get` is total over anything the host returns
            // and reads inline — the same total-roster property `ActiveSpaceUse` already declares. What DOES bind
            // on the rail is what can genuinely refuse: the layer index, which a live object always holds, and
            // every colour, which crosses the kernel gate rather than quantizing silently. The linetype, material,
            // and section-style indexes carry the host's `-1` as an ordinary by-layer/no-style absence, so each
            // projects through `ResourceIndex.Maybe` instead of refusing the default-attributed object.
            return from layer in ResourceIndex.Admit(value: attributes.LayerIndex, key: key)
                from objectColor in AttributeShade.Of(color: attributes.ObjectColor, key: key)
                from plotColor in AttributeShade.Of(color: attributes.PlotColor, key: key)
                from hatchFill in AttributeShade.Of(color: attributes.HatchBackgroundFillColor, key: key)
                from hatchPrint in AttributeShade.Of(color: attributes.HatchBackgroundFillPrintColor, key: key)
                from boundaryColor in AttributeShade.Of(color: attributes.HatchBoundaryColor, key: key)
                from boundaryPlotColor in AttributeShade.Of(color: attributes.HatchBoundaryPlotColor, key: key)
                from decals in attributes.Decals.AsIterable().ToSeq()
                    .TraverseM(decal => DecalSnapshot.Of(decal: decal, key: key)).As()
                let materialRefs = attributes.MaterialRefs.AsIterable().ToSeq()
                    .Map(static pair => new MaterialRefSnapshot(
                        DictionaryKey: pair.Key,
                        Source: MaterialOrigin.Get(key: pair.Value.MaterialSource),
                        PlugInId: pair.Value.PlugInId,
                        FrontId: pair.Value.FrontFaceMaterialId,
                        BackId: pair.Value.BackFaceMaterialId,
                        FrontIndex: pair.Value.FrontFaceMaterialIndex,
                        BackIndex: pair.Value.BackFaceMaterialIndex))
                select new AttributeSnapshot(
                ObjectId: attributes.ObjectId,
                Name: Op.Text(attributes.Name),
                Url: Op.Text(attributes.Url),
                LayerIndex: layer,
                LinetypeIndex: ResourceIndex.Maybe(value: attributes.LinetypeIndex),
                MaterialIndex: ResourceIndex.Maybe(value: attributes.MaterialIndex),
                ViewportId: attributes.ViewportId,
                Space: ActiveSpaceUse.Get(key: attributes.Space),
                ColorSource: ColorOrigin.Get(key: attributes.ColorSource),
                PlotColorSource: PlotColorOrigin.Get(key: attributes.PlotColorSource),
                PlotWeightSource: PlotWeightOrigin.Get(key: attributes.PlotWeightSource),
                LineSource: LinetypeSource.Get(key: attributes.LinetypeSource),
                MaterialSource: MaterialOrigin.Get(key: attributes.MaterialSource),
                SectionSource: SectionOrigin.Get(key: attributes.SectionAttributesSource),
                ObjectColor: objectColor,
                PlotColor: plotColor,
                PlotWeight: attributes.PlotWeight,
                LinetypePatternScale: attributes.LinetypePatternScale,
                WireDensity: attributes.WireDensity,
                DisplayOrder: attributes.DisplayOrder,
                Decoration: EndDecoration.Get(key: attributes.ObjectDecoration),
                Shadows: ShadowPolicy.Of(casts: attributes.CastsShadows, receives: attributes.ReceivesShadows),
                Groups: toSeq(attributes.GetGroupList()),
                Overrides: new OverrideCensus(
                    HiddenInDetails: toSeq(attributes.GetHideInDetailOverrides()),
                    Activity: overrides
                        ? Some((toSeq(viewports), ObjectSignal.Of(on: active)))
                        : Option<(Seq<Guid>, ObjectSignal)>.None,
                    DetailBackgroundVisible: ObjectSignal.Of(on: attributes.DetailBackgroundVisible)),
                SectionStyleIndex: ResourceIndex.Maybe(value: attributes.SectionStyleIndex),
                Label: SectionLabel.Get(key: attributes.ClippingPlaneLabelStyle),
                CustomSectionStyle: customSection is not null,
                CustomLinetype: customLine is not null,
                Frame: attributes.ObjectFrame(),
                Meshing: Optional(customMesh).Map(static parameters => parameters.ToEncodedString()),
                HasMapping: attributes.HasMapping,
                DefinitionMember: attributes.IsInstanceDefinitionObject,
                UserStrings: TagOp.Snapshot(attributes.GetUserStrings()),
                Decals: decals,
                MaterialRefs: materialRefs,
                HatchFill: hatchFill,
                HatchPrint: hatchPrint,
                HatchBoundaryVisible: ObjectSignal.Of(on: attributes.HatchBoundaryVisible),
                HatchBoundaryColor: boundaryColor,
                HatchBoundaryPlotColor: boundaryPlotColor,
                HatchBoundaryColorSource: ItemColorOrigin.Get(key: attributes.HatchBoundaryColorSource),
                HatchBoundaryPlotColorSource: ItemColorOrigin.Get(key: attributes.HatchBoundaryPlotColorSource),
                HatchBoundaryPlotWeight: attributes.HatchBoundaryPlotWeightMillimeters,
                Modifiers: (
                    Optional(modifiers.Displacement).IsSome,
                    Optional(modifiers.EdgeSoftening).IsSome,
                    Optional(modifiers.Thickening).IsSome,
                    Optional(modifiers.CurvePiping).IsSome,
                    Optional(modifiers.ShutLining).IsSome));
        });
}

public readonly record struct EffectiveDisplay(
    Guid Id,
    PerceptualColor Draw,
    PerceptualColor Plot,
    double PlotWeight,
    Option<Guid> ModeOverride,
    Option<ObjectSignal> ActiveOverride) : IDetachedDocumentResult {
    // The scoped and document-wide reads differ only in which host overload answers, so the raw quadruple
    // resolves in one viewport dispatch and the kernel colour gate and constructor run exactly once.
    internal static Fin<EffectiveDisplay> Of(RhinoObject native, Rhino.RhinoDoc document, Option<Guid> viewport, Op key) =>
        key.Catch(() => {
            ObjectAttributes attributes = native.Attributes;
            var resolved = viewport.Case is Guid scoped
                ? (Draw: attributes.DrawColor(document: document, viewportId: scoped),
                    Plot: attributes.ComputedPlotColor(document: document, viewportId: scoped),
                    Weight: attributes.ComputedPlotWeight(document: document, viewportId: scoped),
                    Mode: attributes.HasDisplayModeOverride(viewportId: scoped)
                        ? Some(attributes.GetDisplayModeOverride(viewportId: scoped))
                        : Option<Guid>.None,
                    Active: attributes.HasActiveInViewportOverride(viewportId: scoped, active: out bool enabled)
                        ? Some(ObjectSignal.Of(on: enabled))
                        : Option<ObjectSignal>.None)
                : (Draw: attributes.DrawColor(document: document),
                    Plot: attributes.ComputedPlotColor(document: document),
                    Weight: attributes.ComputedPlotWeight(document: document),
                    Mode: Option<Guid>.None,
                    Active: Option<ObjectSignal>.None);
            return from draw in AttributeShade.Of(color: resolved.Draw, key: key)
                   from plot in AttributeShade.Of(color: resolved.Plot, key: key)
                   select new EffectiveDisplay(
                       Id: native.Id,
                       Draw: draw,
                       Plot: plot,
                       PlotWeight: resolved.Weight,
                       ModeOverride: resolved.Mode,
                       ActiveOverride: resolved.Active);
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

| [INDEX] | [CONCERN]          | [OWNER]             | [FORM]                                                   | [ENTRY]                         |
| :-----: | :----------------- | :------------------ | :------------------------------------------------------- | :------------------------------ |
|  [01]   | attribute axes     | keyed origin rows   | one row per host discriminant, `FromObject` on sources   | `.Key` write / `Get` read       |
|  [02]   | colour crossing    | `AttributeShade`    | the page's only two host-colour seams                    | `Of` / `Rgb`                    |
|  [03]   | attribute mutation | `AttributeEdit`     | admitted union with one total `Apply` over the duplicate | program payloads                |
|  [04]   | set-valued edits   | `RosterMove`        | impose/extend/retract cases carrying their own rosters   | owning edit payloads            |
|  [05]   | detached carriers  | generated products  | `DecalSeed`/`MaterialRefSeed` onto host create params    | `Decals` / `FaceMaterials`      |
|  [06]   | write program      | `AttributeProgram`  | short-circuit fold minting the spine's change payload    | `TableOp.Amend(target, Change)` |
|  [07]   | read dispatch      | `AttributeAsk`      | stored and resolved questions, one typed answer union    | `Attributes.Ask`                |
|  [08]   | stored state       | `AttributeSnapshot` | detached scalars, rosters, and catalogued carriers       | `AttributeAsk.Stored`           |
|  [09]   | resolved display   | `EffectiveDisplay`  | resolved scalars and viewport overrides                  | `AttributeAsk.Resolved`         |

## [06]-[RESEARCH]

<!-- source-only: research row template:
[TOKEN]-[OPEN|BLOCKED]: <exact question>; <verification route>.
[SPLIT_MEMBER]-[OPEN]: does `shape-core` expose `split_all`; verify against the member rail.
-->

(none)
