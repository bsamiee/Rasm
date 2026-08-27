# [RASM_RHINO_OBJECTS_ATTRIBUTES]

Typed attribute mutation belongs to `Rasm.Rhino.Objects`. `AttributeEdit` closes every writable `ObjectAttributes` family with verified payload carriers — layer, source-resolved display, space, overrides, section and hatch state, frames, meshing, tags — and parameterizes group, decal, and face-material set operations over detached seeds. `AttributeProgram` admits and folds edits over the duplicate `TableOp.Amend` supplies; this page exposes no local write entry. One `AttributeAsk` owns the read side, where `AttributeSnapshot` captures detached stored state and `EffectiveDisplay` resolves what source dispatch answers.

## [01]-[INDEX]

- [02]-[EDIT_FAMILY]: keyed attribute-axis owners, `ObjectStance`, the `ShadowRole`/`AttachedModifier` capability vocabularies, `AttributeShade`, `RosterMove`, the detached seed carriers, and the `AttributeEdit` union — the closed mutation vocabulary with its total dispatch.
- [03]-[PROGRAM]: `AttributeProgram` — the fold, the `Amend` handoff, and the write-path law.
- [04]-[SNAPSHOT_AND_EFFECTIVE]: `AttributeAsk`, `AttributeSnapshot`, `EffectiveDisplay`, and the one read entry.
- [05]-[SURFACE_LEDGER]: owner rows for every surface this page declares.

## [02]-[EDIT_FAMILY]

- Owner: `RosterMove<TGrow, TCut>` closes impose, extend, and retract as cases carrying their own payload — grow cases hold value rosters, retract holds identity keys; `ObjectStance` closes the host mode word; `ShadowRole` and `AttachedModifier` are the two capability vocabularies this page's set-valued columns ride; `DecalSeed` and `MaterialRefSeed` are generated admitted products; `AttributeEdit` `[Union]` owns the assigned stored-attribute mutations and one total `Apply` over the working duplicate.
- Law: no raw host discriminant crosses a signature on this page. Every attribute axis re-closes as a keyed row over its host ordinal — `ColorOrigin`, `PlotColorOrigin`, `PlotWeightOrigin`, `MaterialOrigin`, `SectionOrigin`, `ItemColorOrigin`, `EndDecoration`, `SectionLabel`, `DecalFrame`, `DecalFacing`, `ObjectStance` — and the linetype axis composes `Annotation/linetype.md`'s `LinetypeSource` at BOTH touch points rather than minting a sibling. Each roster mirrors its host enum row for row, so a read is a total `Get` and a write is one `.Key`; every SOURCE axis — the four minted here and the composed `LinetypeSource` alike — carries a `FromObject` column, so source-payload coherence is the one `SourceValue` guard reading a row instead of an `is` comparison repeated per admission arm.
- Law: a plot weight is the layer plane's `PrintPen`, never a millimetre double. Rhino spells "application default" as `0.0` and "do not plot" as `-1.0` on the same scalar an authored width rides, so every weight on this page — the object weight, the hatch-boundary weight, and the source-resolved weight — admits through `PrintPen.OfHost` and writes through `ToHost`, taking the ISO 128-24 ladder rung from `Drawing/sheet`'s `LineWidth` and both sentinel postures as named cases. Three bare doubles carried them before, and each is the deleted form; `PrintPen` is `Document/layers`' owner composed, never re-derived.
- Law: adjacent presence bits ride ONE capability column each. Shadow participation is `CapabilitySet<ShadowRole>` — two independent host bits whose four corners are all legal, so the axis is a SET and a third participation bit is one row, where the four-corner enum it replaces paid a truth table to name each corner; attached foreign carriers are `CapabilitySet<AttachedModifier>` — five mesh modifiers and three carrier probes answering one question, "what hangs off this attribute set", each row owning the host read that proves it. NAMED LOSS in both: per-column compile-time exhaustiveness — a renamed row still breaks every reader, while a narrowed producer set does not — bought back by `Wire` on every snapshot and `AdmitsAll` at a consumer boundary that requires a row.
- Law: object mode is READ here and written nowhere. `ObjectStance` closes the host's four-valued `Mode` word, so the snapshot reports normal, hidden, locked, or definition membership as one row instead of re-deriving the word into separate presence bools; the WRITE stays the table pipeline's `TableOp.State` by the refusal law below.
- Law: colour is perceptual at the boundary and nowhere else. `AttributeShade.Of` admits on read and `AttributeShade.Rgb` quantizes on write, so `System.Drawing.Color` exists only inside those two members: no stored column, no edit payload, and no snapshot field carries it, which closes both the named-colour equality trap and every ad-hoc component fold at once.
- Law: source-dependent payloads admit one coherent product. Object-sourced color, plot color, plot weight, linetype, and material edits require their object value; every other source rejects that irrelevant value. `LinetypePatternScale` remains independent of source and may accompany any line-pattern edit.
- Law: mode and visibility are refused by absence — no case writes `Mode` or `Visible`, because object mode transitions are the table pipeline's `TableOp.State` and a second write path forks the undo story; `Realm` writes the catalogued space and optional viewport anchor, which no table op carries.
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
- Packages: Thinktecture.Runtime.Extensions (`libs/dotnet/.api/api-thinktecture-runtime-extensions.md` — `[SmartEnum<TKey>]`, `[ComplexValueObject]`, `[Union]`, `[ValidationError]`, `[UseDelegateFromConstructor]`, `[KeyMemberEqualityComparer<TAccessor, TKey>]`, `ComparerAccessors`); LanguageExt.Core (`api-languageext.md` — `Fin`, `Option`, `Seq`, `HashMap`, `Traverse`/`TraverseM`, `guard`); kernel `Domain/validation` (`ICapability`, `CapabilitySet`), `Domain/results` (`HostEdge.Text`, `Try.lift`, `Admit.Confirm`), `Numerics/atoms` (`PerceptualColor.OfRgb`/`ToRgb`), `Drawing/sheet` (`LineWidth` behind `PrintPen`); `Document/session` (`DraftFault`, `DocumentSession`, `SessionNeed`), `Document/layers` (`PrintPen`), `Document/tables` (`AttributeChange`, `ResourceIndex`, `TableTarget`), `Document/geometry` (`TagOp`), `Annotation/linetype` (`LinetypeSource`); RhinoCommon objects (`Rasm.Rhino/.api/api-rhinocommon-objects.md:147-177` — the attribute reads and writes, `Decals`, `MaterialRefs`, `File3dmMeshModifiers`, the decal latitude/longitude and material-ref swap traps).

```csharp
// --- [IMPORTS] -------------------------------------------------------------------------
using Rasm.Domain;
using Rasm.Numerics;
using Rasm.Rhino.Annotation;
using Rasm.Rhino.Document;
using Rhino.DocObjects;
using Rhino.Display;
using Rhino.Geometry;
using Rhino.Render;

namespace Rasm.Rhino.Objects;

// --- [TYPES] ---------------------------------------------------------------------------
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

[SmartEnum<Rhino.DocObjects.ObjectMode>]
public sealed partial class ObjectStance {
    public static readonly ObjectStance Normal = new(key: Rhino.DocObjects.ObjectMode.Normal);
    public static readonly ObjectStance Hidden = new(key: Rhino.DocObjects.ObjectMode.Hidden);
    public static readonly ObjectStance Locked = new(key: Rhino.DocObjects.ObjectMode.Locked);
    public static readonly ObjectStance DefinitionMember = new(key: Rhino.DocObjects.ObjectMode.InstanceDefinitionObject);
}

[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class ShadowRole : ICapability<ShadowRole> {
    public static readonly ShadowRole Cast = new(key: "cast");
    public static readonly ShadowRole Receive = new(key: "receive");

    internal static CapabilitySet<ShadowRole> Of(ObjectAttributes attributes) {
        CapabilitySet<ShadowRole> held = CapabilitySet<ShadowRole>.Of();
        held = attributes.CastsShadows ? held.With(capability: Cast) : held;
        return attributes.ReceivesShadows ? held.With(capability: Receive) : held;
    }
}

[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class AttachedModifier : ICapability<AttachedModifier> {
    public static readonly AttachedModifier Displacement = new(
        key: "displacement", held: static attributes => attributes.File3dmMeshModifiers.Displacement is not null);
    public static readonly AttachedModifier EdgeSoftening = new(
        key: "edge-softening", held: static attributes => attributes.File3dmMeshModifiers.EdgeSoftening is not null);
    public static readonly AttachedModifier Thickening = new(
        key: "thickening", held: static attributes => attributes.File3dmMeshModifiers.Thickening is not null);
    public static readonly AttachedModifier CurvePiping = new(
        key: "curve-piping", held: static attributes => attributes.File3dmMeshModifiers.CurvePiping is not null);
    public static readonly AttachedModifier ShutLining = new(
        key: "shut-lining", held: static attributes => attributes.File3dmMeshModifiers.ShutLining is not null);
    public static readonly AttachedModifier SectionStyle = new(
        key: "section-style", held: static attributes => {
            using Rhino.DocObjects.SectionStyle? style = attributes.GetCustomSectionStyle();
            return style is not null;
        });
    public static readonly AttachedModifier Linetype = new(
        key: "linetype", held: static attributes => {
            using Rhino.DocObjects.Linetype? pattern = attributes.GetCustomLinetype();
            return pattern is not null;
        });
    public static readonly AttachedModifier Mapping = new(key: "mapping", held: static attributes => attributes.HasMapping);

    [UseDelegateFromConstructor]
    public partial bool Held(ObjectAttributes attributes);

    internal static CapabilitySet<AttachedModifier> Of(ObjectAttributes attributes) =>
        CapabilitySet<AttachedModifier>.Of([.. toSeq(Items).Filter(row => row.Held(attributes: attributes))]);
}

public static class AttributeShade {
    internal static Fin<PerceptualColor> Of(System.Drawing.Color color) =>
        PerceptualColor.OfRgb(color.R, color.G, color.B, alpha: color.A);

    internal static System.Drawing.Color Rgb(PerceptualColor shade) =>
        shade.ToRgb() switch {
            var (red, green, blue, alpha) => System.Drawing.Color.FromArgb(alpha: alpha, red: red, green: green, blue: blue),
        };
}

public abstract record RosterMove<TGrow, TCut> {
    private RosterMove() { }
    public sealed record Impose(Seq<TGrow> Values) : RosterMove<TGrow, TCut>;
    public sealed record Extend(Seq<TGrow> Values) : RosterMove<TGrow, TCut>;
    public sealed record Retract(Seq<TCut> Keys) : RosterMove<TGrow, TCut>;

    internal Fin<RosterMove<TGrow, TCut>> Admit(Func<TGrow, bool> grow, Func<TCut, bool> cut) => this switch {
        Impose(var values) => Roster(values: values, valid: grow, floor: 0)
            .Map(static admitted => (RosterMove<TGrow, TCut>)new Impose(Values: admitted)),
        Extend(var values) => Roster(values: values, valid: grow, floor: 1)
            .Map(static admitted => (RosterMove<TGrow, TCut>)new Extend(Values: admitted)),
        Retract(var keys) => Roster(values: keys, valid: cut, floor: 1)
            .Map(static admitted => (RosterMove<TGrow, TCut>)new Retract(Keys: admitted)),
        _ => Fin.Fail<RosterMove<TGrow, TCut>>(error: new KernelFault.InvalidInput()),
    };

    private static Fin<Seq<T>> Roster<T>(Seq<T> values, Func<T, bool> valid, int floor) =>
        from roster in values.TraverseM(value => valid(value)
            ? Fin.Succ(value: value)
            : Fin.Fail<T>(error: new KernelFault.InvalidInput())).As()
        let admitted = roster.Distinct()
        from _ in guard(admitted.Count >= floor, new KernelFault.InvalidInput())
        select admitted;
}

[ComplexValueObject]
[ValidationError]
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
        (Point3d Seat, Vector3d Up, Vector3d Across) frame = (origin, up, across);
        (double Transparency, double Height, double Radius) scalar = (transparency, height, radius);
        (double HorzStart, double HorzEnd, double VertStart, double VertEnd) sweep = (horzStart, horzEnd, vertStart, vertEnd);
        (double MinU, double MinV, double MaxU, double MaxV) bounds = (minU, minV, maxU, maxV);
        (DecalFrame Mapping, DecalFacing Projection, ObjectSignal Inside) rows = (mapping, projection, mapToInside);
        validationError = FactoryValidation.Of(FactoryValidation.Violated(
                (texture == Guid.Empty, () => new ValidationClause(string.Join(" | ", new object?[] { op, nameof(Texture) }))),
                (rows.Mapping is null || rows.Projection is null || rows.Inside is null,
                    () => new ValidationClause(string.Join(" | ", new object?[] { op, nameof(DecalSeed) }))),
                (!frame.Seat.IsValid || !frame.Up.IsValid || !frame.Across.IsValid, () => new ValidationClause(string.Join(" | ", new object?[] { op, nameof(Origin), 0d, "a valid decal frame" }))),
                (scalar.Transparency is not (>= 0.0 and <= 1.0), () => new ValidationClause(string.Join(" | ", new object?[] { op, nameof(Transparency), scalar.Transparency, "a unit fraction" }))),
                (!ValidityClaim.Finite(scalar.Height).Holds || scalar.Height <= 0.0, () => new ValidationClause(string.Join(" | ", new object?[] { op, nameof(Height), scalar.Height, "a positive finite height" }))),
                (!ValidityClaim.Finite(scalar.Radius).Holds || scalar.Radius <= 0.0, () => new ValidationClause(string.Join(" | ", new object?[] { op, nameof(Radius), scalar.Radius, "a positive finite radius" }))),
                (!ValidityClaim.Finite(sweep.HorzStart).Holds || !ValidityClaim.Finite(sweep.HorzEnd).Holds
                    || !ValidityClaim.Finite(sweep.VertStart).Holds || !ValidityClaim.Finite(sweep.VertEnd).Holds,
                    () => new ValidationClause(string.Join(" | ", new object?[] { op, nameof(HorzStart), sweep.HorzStart, "finite sweep bounds" }))),
                (!ValidityClaim.Finite(bounds.MinU).Holds || !ValidityClaim.Finite(bounds.MinV).Holds
                    || !ValidityClaim.Finite(bounds.MaxU).Holds || !ValidityClaim.Finite(bounds.MaxV).Holds
                    || bounds.MinU > bounds.MaxU || bounds.MinV > bounds.MaxV,
                    () => new ValidationClause(string.Join(" | ", new object?[] { op, nameof(MinU), bounds.MinU, "an ordered finite uv window" })))));
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
[ValidationError]
public sealed partial class MaterialRefSeed {
    public Guid PlugIn { get; }
    public MaterialOrigin Source { get; }
    public Guid FrontId { get; }
    public Guid BackId { get; }
    public int FrontIndex { get; }
    public int BackIndex { get; }

    static partial void ValidateFactoryArguments(
        ref ValidationError? validationError,
        ref Guid plugIn,
        ref MaterialOrigin source,
        ref Guid frontId,
        ref Guid backId,
        ref int frontIndex,
        ref int backIndex) {
        (Guid Front, Guid Back, int FrontIndex, int BackIndex) face = (frontId, backId, frontIndex, backIndex);
        MaterialOrigin row = source;
        validationError = FactoryValidation.Of(FactoryValidation.Violated(
                (plugIn == Guid.Empty, () => new ValidationClause(string.Join(" | ", new object?[] { op, nameof(PlugIn) }))),
                (row is null, () => new ValidationClause(string.Join(" | ", new object?[] { op, nameof(Source) }))),
                (face.FrontIndex < ResourceIndex.Absent || face.BackIndex < ResourceIndex.Absent,
                    () => new ValidationClause(string.Join(" | ", new object?[] { op, nameof(FrontIndex), face.FrontIndex, "a table index at or above the host absence sentinel" }))),
                (face.Front == Guid.Empty && face.FrontIndex < 0, () => new ValidationClause(string.Join(" | ", new object?[] { op, nameof(FrontId), "a front material identity or a front table index" }))),
                (face.Back == Guid.Empty && face.BackIndex < 0, () => new ValidationClause(string.Join(" | ", new object?[] { op, nameof(BackId), "a back material identity or a back table index" })))));
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
    public sealed record PlotWeight(PlotWeightOrigin Source, Option<PrintPen> Pen = default) : AttributeEdit;
    public sealed record LinePattern(LinetypeSource Source, Option<ResourceIndex> Index = default, Option<double> PatternScale = default) : AttributeEdit;
    public sealed record CustomLine(Option<Linetype> Pattern) : AttributeEdit;
    public sealed record MaterialBind(MaterialOrigin Source, Option<ResourceIndex> Index = default) : AttributeEdit;
    public sealed record Shadows(CapabilitySet<ShadowRole> Roles) : AttributeEdit;
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
        Option<PrintPen> Pen = default) : AttributeEdit;
    public sealed record AnchorFrame(Plane Frame) : AttributeEdit;
    public sealed record AnchorMove(Transform Motion) : AttributeEdit;
    public sealed record Meshing(Option<string> Encoded) : AttributeEdit;
    public sealed record RenderingReset : AttributeEdit;
    public sealed record Decals(RosterMove<DecalSeed, int> Move) : AttributeEdit;
    public sealed record Tag(TagOp Operation) : AttributeEdit;
    public sealed record FaceMaterials(RosterMove<MaterialRefSeed, Guid> Move) : AttributeEdit;

    internal Fin<AttributeEdit> Admit() =>
        Switch(identity: static (key, edit) =>
                from name in edit.Name.Traverse(text => Acceptance.Text(value: text)).As()
                from url in edit.Url.Traverse(text => Acceptance.Text(value: text)).As()
                from _ in guard(name.IsSome || url.IsSome, new KernelFault.InvalidInput())
                select (AttributeEdit)new Identity(Name: name, Url: url),
            layer: static (_, edit) => Fin.Succ<AttributeEdit>(edit),
            paint: static (key, edit) => Admit.Need(edit.Source)
                .Bind(source => SourceValue(source.FromObject, edit.Value, edit)),
            plot: static (key, edit) => Admit.Need(edit.Source)
                .Bind(source => SourceValue(source.FromObject, edit.Value, edit)),
            plotWeight: static (key, edit) => Admit.Need(edit.Source)
                .Bind(source => SourceValue(source.FromObject, edit.Pen, edit)),
            linePattern: static (edit) =>
                from source in Admit.Need(edit.Source)
                from admitted in SourceValue(source.FromObject, edit.Index, edit)
                from _ in guard(edit.PatternScale
                    .Map(static value => double.IsFinite(value) && value > 0.0)
                    .IfNone(noneValue: true), new KernelFault.InvalidInput())
                select admitted,
            customLine: static (_, edit) => Fin.Succ<AttributeEdit>(edit),
            materialBind: static (key, edit) => Admit.Need(edit.Source)
                .Bind(source => SourceValue(source.FromObject, edit.Index, edit)),
            shadows: static (_, edit) => Fin.Succ<AttributeEdit>(edit),
            wires: static (_, edit) => Fin.Succ<AttributeEdit>(edit),
            drawOrder: static (_, edit) => Fin.Succ<AttributeEdit>(edit),
            decorate: static (key, edit) => Admit.Need(edit.Ends).Map(_ => (AttributeEdit)edit),
            realm: static (edit) => guard(
                edit.Space is not null && edit.Space != ActiveSpaceUse.None
                && edit.Viewport.Map(static value => value != Guid.Empty).IfNone(noneValue: true),
                new KernelFault.InvalidInput()).ToFin().Map(_ => (AttributeEdit)edit),
            groups: static (key, edit) => Admit.Need(edit.Move)
                .Bind(move => move.Admit(grow: static index => index >= 0, cut: static index => index >= 0))
                .Map(static move => (AttributeEdit)new Groups(Move: move)),
            modeOverride: static (edit) => guard(
                edit.Viewport.Map(static value => value != Guid.Empty).IfNone(noneValue: true)
                && edit.Mode.Map(static value => value != Guid.Empty).IfNone(noneValue: true),
                new KernelFault.InvalidInput()).ToFin().Map(_ => (AttributeEdit)edit),
            detailHide: static (edit) =>
                from _ in guard(edit.Detail != Guid.Empty, new KernelFault.InvalidInput()).ToFin()
                from __ in Admit.Need(edit.Signal)
                select (AttributeEdit)edit,
            detailBackground: static (key, edit) => Admit.Need(edit.Signal).Map(_ => (AttributeEdit)edit),
            activity: static (edit) =>
                from signal in Admit.Need(edit.Signal)
                from move in Admit.Need(edit.Move)
                from admitted in move.Admit(grow: static id => id != Guid.Empty, cut: static id => id != Guid.Empty)
                select (AttributeEdit)new Activity(Move: admitted, Signal: signal),
            sectionSource: static (key, edit) => Admit.Need(edit.Source).Map(_ => (AttributeEdit)edit),
            sectionIndex: static (_, edit) => Fin.Succ<AttributeEdit>(edit),
            sectionFace: static (_, edit) => Fin.Succ<AttributeEdit>(edit),
            label: static (key, edit) => Admit.Need(edit.Style).Map(_ => (AttributeEdit)edit),
            hatchFill: static (key, edit) => guard(edit.Fill.IsSome || edit.Print.IsSome, new KernelFault.InvalidInput()).ToFin().Map(_ => (AttributeEdit)edit),
            hatchBoundary: static (edit) => guard(
                edit.Visible.IsSome || edit.Color.IsSome || edit.PlotColor.IsSome || edit.ColorSource.IsSome
                || edit.PlotColorSource.IsSome || edit.Pen.IsSome,
                new KernelFault.InvalidInput()).ToFin().Map(_ => (AttributeEdit)edit),
            anchorFrame: static (key, edit) => Acceptance.Input(value: edit.Frame).Map(_ => (AttributeEdit)edit),
            anchorMove: static (key, edit) => Acceptance.Input(value: edit.Motion).Map(_ => (AttributeEdit)edit),
            meshing: static (edit) => edit.Encoded
                .Traverse(text =>
                    from accepted in Acceptance.Text(value: text)
                    from normalized in Try.lift(() => {
                        using MeshingParameters? parameters = MeshingParameters.FromEncodedString(accepted);
                        return parameters is null
                            ? Fin.Fail<string>(error: new KernelFault.InvalidInput())
                            : Fin.Succ(value: parameters.ToEncodedString());
                    }).Run().Bind(static inner => inner)
                    select normalized)
                .As()
                .Map(encoded => (AttributeEdit)new Meshing(Encoded: encoded)),
            renderingReset: static (_, edit) => Fin.Succ<AttributeEdit>(edit),
            decals: static (key, edit) => Admit.Need(edit.Move)
                .Bind(move => move.Admit(grow: static seed => seed is not null, cut: static crc => crc != 0))
                .Map(static move => (AttributeEdit)new Decals(Move: move)),
            tag: static (edit) =>
                from operation in Admit.Need(edit.Operation)
                from _ in guard(operation.Mutates, new KernelFault.InvalidInput())
                select (AttributeEdit)edit,
            faceMaterials: static (key, edit) => Admit.Need(edit.Move)
                .Bind(move => move.Admit(grow: static seed => seed is not null, cut: static plugin => plugin != Guid.Empty))
                .Map(static move => (AttributeEdit)new FaceMaterials(Move: move)));

    private static Fin<AttributeEdit> SourceValue<TValue>(bool requires, Option<TValue> value, AttributeEdit edit) =>
        guard(requires == value.IsSome, new KernelFault.InvalidInput()).ToFin().Map(_ => edit);

    internal Fin<Unit> Apply(ObjectAttributes attributes) =>
        Switch(
            attributes,
            identity: static (context, edit) => Try.lift(() => {
                _ = edit.Name.Iter(name => context.Name = name);
                _ = edit.Url.Iter(url => context.Url = url);
            }).Run().Bind(static inner => inner),
            layer: static (context, edit) => Try.lift(() => context.LayerIndex = edit.Index.Value).Run().Bind(static inner => inner),
            paint: static (context, edit) => Try.lift(() => {
                context.ColorSource = edit.Source.Key;
                _ = edit.Value.Iter(shade => context.ObjectColor = AttributeShade.Rgb(shade: shade));
            }).Run().Bind(static inner => inner),
            plot: static (context, edit) => Try.lift(() => {
                context.PlotColorSource = edit.Source.Key;
                _ = edit.Value.Iter(shade => context.PlotColor = AttributeShade.Rgb(shade: shade));
            }).Run().Bind(static inner => inner),
            plotWeight: static (context, edit) => Try.lift(() => {
                context.PlotWeightSource = edit.Source.Key;
                _ = edit.Pen.Iter(pen => context.PlotWeight = pen.ToHost());
            }).Run().Bind(static inner => inner),
            linePattern: static (context, edit) => Try.lift(() => {
                context.LinetypeSource = edit.Source.Key;
                _ = edit.Index.Iter(index => context.LinetypeIndex = index.Value);
                _ = edit.PatternScale.Iter(scale => context.LinetypePatternScale = scale);
            }).Run().Bind(static inner => inner),
            customLine: static (context, edit) => Try.lift(() => {
                edit.Pattern.Match(
                    Some: pattern => context.SetCustomLinetype(linetype: pattern),
                    None: () => context.RemoveCustomLinetype());
            }).Run().Bind(static inner => inner),
            materialBind: static (context, edit) => Try.lift(() => {
                context.MaterialSource = edit.Source.Key;
                _ = edit.Index.Iter(index => context.MaterialIndex = index.Value);
            }).Run().Bind(static inner => inner),
            shadows: static (context, edit) => Try.lift(() => {
                context.CastsShadows = edit.Roles.Admits(capability: ShadowRole.Cast);
                context.ReceivesShadows = edit.Roles.Admits(capability: ShadowRole.Receive);
            }).Run().Bind(static inner => inner),
            wires: static (context, edit) => Try.lift(() => context.WireDensity = edit.Density).Run().Bind(static inner => inner),
            drawOrder: static (context, edit) => Try.lift(() => context.DisplayOrder = edit.Rank).Run().Bind(static inner => inner),
            decorate: static (context, edit) => Try.lift(() => context.ObjectDecoration = edit.Ends.Key).Run().Bind(static inner => inner),
            realm: static (context, edit) => Try.lift(() => {
                context.Space = edit.Space.Key;
                context.ViewportId = edit.Viewport.IfNone(noneValue: Guid.Empty);
            }).Run().Bind(static inner => inner),
            groups: static (context, edit) => edit.Move switch {
                RosterMove<int, int>.Impose(var indices) => Try.lift(() => {
                    context.RemoveFromAllGroups();
                    _ = indices.Iter(index => context.AddToGroup(groupIndex: index));
                }).Run().Bind(static inner => inner),
                RosterMove<int, int>.Extend(var indices) => Try.lift(() =>
                    _ = indices.Iter(index => context.AddToGroup(groupIndex: index))).Run().Bind(static inner => inner),
                RosterMove<int, int>.Retract(var indices) => Try.lift(() =>
                    _ = indices.Iter(index => context.RemoveFromGroup(groupIndex: index))).Run().Bind(static inner => inner),
                _ => Fin.Fail<Unit>(error: new KernelFault.InvalidInput()),
            },
            modeOverride: static (context, edit) => edit.Mode
                .Traverse(id => Optional(DisplayModeDescription.GetDisplayMode(id)).ToFin(Fail: new KernelFault.MissingContext()))
                .As()
                .Bind(mode => (mode.Case, edit.Viewport.Case) switch {
                    (DisplayModeDescription resolved, Guid viewport) => Admit.Confirm(
                        success: context.SetDisplayModeOverride(mode: resolved, rhinoViewportId: viewport)),
                    (DisplayModeDescription resolved, null) => Admit.Confirm(
                        success: context.SetDisplayModeOverride(mode: resolved)),
                    (null, Guid viewport) => Try.lift(() => context.RemoveDisplayModeOverride(rhinoViewportId: viewport)).Run().Bind(static inner => inner),
                    _ => Try.lift(() => context.RemoveDisplayModeOverride()).Run().Bind(static inner => inner),
                }),
            detailHide: static (context, edit) => Admit.Confirm(success: edit.Signal.On
                ? context.AddHideInDetailOverride(detailId: edit.Detail)
                : context.RemoveHideInDetailOverride(detailId: edit.Detail)),
            detailBackground: static (context, edit) => Try.lift(() => context.DetailBackgroundVisible = edit.Signal.On).Run().Bind(static inner => inner),
            activity: static (context, edit) => edit.Move switch {
                RosterMove<Guid, Guid>.Impose(var viewports) => Admit.Confirm(
                    success: context.SetActiveInViewportOverrides(viewportIds: viewports.ToArray(), active: edit.Signal.On)),
                RosterMove<Guid, Guid>.Extend(var viewports) => viewports.TraverseM(viewport => Admit.Confirm(
                    success: context.AddActiveInViewportOverride(viewportId: viewport, active: edit.Signal.On))).As()
                    .Map(static _ => unit),
                RosterMove<Guid, Guid>.Retract(var viewports) => viewports.TraverseM(viewport => Admit.Confirm(
                    success: context.RemoveActiveInViewportOverride(viewportId: viewport, active: edit.Signal.On))).As()
                    .Map(static _ => unit),
                _ => Fin.Fail<Unit>(error: new KernelFault.InvalidInput()),
            },
            sectionSource: static (context, edit) => Try.lift(() => context.SectionAttributesSource = edit.Source.Key).Run().Bind(static inner => inner),
            sectionIndex: static (context, edit) => Try.lift(() =>
                context.SectionStyleIndex = edit.Index.Map(static index => index.Value).IfNone(noneValue: ResourceIndex.Absent)).Run().Bind(static inner => inner),
            sectionFace: static (context, edit) => Try.lift(() => {
                edit.Style.Match(
                    Some: style => context.SetCustomSectionStyle(sectionStyle: style),
                    None: () => context.RemoveCustomSectionStyle());
            }).Run().Bind(static inner => inner),
            label: static (context, edit) => Try.lift(() => context.ClippingPlaneLabelStyle = edit.Style.Key).Run().Bind(static inner => inner),
            hatchFill: static (context, edit) => Try.lift(() => {
                _ = edit.Fill.Iter(shade => context.HatchBackgroundFillColor = AttributeShade.Rgb(shade: shade));
                _ = edit.Print.Iter(shade => context.HatchBackgroundFillPrintColor = AttributeShade.Rgb(shade: shade));
            }).Run().Bind(static inner => inner),
            hatchBoundary: static (context, edit) => Try.lift(() => {
                _ = edit.Visible.Iter(signal => context.HatchBoundaryVisible = signal.On);
                _ = edit.Color.Iter(shade => context.HatchBoundaryColor = AttributeShade.Rgb(shade: shade));
                _ = edit.PlotColor.Iter(shade => context.HatchBoundaryPlotColor = AttributeShade.Rgb(shade: shade));
                _ = edit.ColorSource.Iter(source => context.HatchBoundaryColorSource = source.Key);
                _ = edit.PlotColorSource.Iter(source => context.HatchBoundaryPlotColorSource = source.Key);
                _ = edit.Pen.Iter(pen => context.HatchBoundaryPlotWeightMillimeters = pen.ToHost());
            }).Run().Bind(static inner => inner),
            anchorFrame: static (context, edit) => Try.lift(() => context.SetObjectFrame(plane: edit.Frame)).Run().Bind(static inner => inner),
            anchorMove: static (context, edit) => Try.lift(() => context.SetObjectFrame(xform: edit.Motion)).Run().Bind(static inner => inner),
            meshing: static (context, edit) => Try.lift(() => {
                if (edit.Encoded.Case is string encoded) {
                    using MeshingParameters? parameters = MeshingParameters.FromEncodedString(encoded);
                    if (parameters is null) { return Fin.Fail<Unit>(new KernelFault.InvalidResult()); }
                    context.CustomMeshingParameters = parameters;
                    context.EnableCustomMeshingParameters = true;
                } else {
                    context.EnableCustomMeshingParameters = false;
                    context.CustomMeshingParameters = null;
                }
                return Fin.Succ(value: unit);
            }).Run().Bind(static inner => inner),
            renderingReset: static (context, _) => Try.lift(() => context.ClearRenderingAttributes()).Run().Bind(static inner => inner),
            decals: static (context, edit) => edit.Move switch {
                RosterMove<DecalSeed, int>.Impose(var seeds) => Try.lift(() => context.Decals.RemoveAllDecals()).Run().Bind(static inner => inner)
                    .Bind(_ => Grown(attributes: context, seeds: seeds)),
                RosterMove<DecalSeed, int>.Extend(var seeds) => Grown(attributes: context, seeds: seeds),
                RosterMove<DecalSeed, int>.Retract(var crcs) => Try.lift(() =>
                    toSeq(context.Decals)
                        .Filter(decal => crcs.Exists(crc => crc == decal.CRC))
                        .TraverseM(decal => Admit.Confirm(success: context.Decals.Remove(decal: decal))).As()
                        .Map(static _ => unit)).Run().Bind(static inner => inner),
                _ => Fin.Fail<Unit>(error: new KernelFault.InvalidInput()),
            },
            tag: static (context, edit) => edit.Operation.Switch(
                (context),
                set: static (held, verb) =>
                    from key in Acceptance.Text(value: verb.Key)
                    from _ in Admit.Confirm(success: held.SetUserString(value: verb.Value))
                    select unit,
                read: static (held, _) => Fin.Fail<Unit>(error: new KernelFault.InvalidInput()),
                readAll: static (held, _) => Fin.Fail<Unit>(error: new KernelFault.InvalidInput()),
                delete: static (held, verb) =>
                    from key in Acceptance.Text(value: verb.Key)
                    from _ in Admit.Confirm(success: held.DeleteUserString())
                    select unit,
                clear: static (held, _) => Try.lift(() => held.DeleteAllUserStrings()).Run().Bind(static inner => inner)),
            faceMaterials: static (context, edit) => edit.Move switch {
                RosterMove<MaterialRefSeed, Guid>.Impose(var seeds) => Try.lift(() => context.MaterialRefs.Clear()).Run().Bind(static inner => inner)
                    .Bind(_ => Bound(attributes: context, seeds: seeds)),
                RosterMove<MaterialRefSeed, Guid>.Extend(var seeds) => Bound(attributes: context, seeds: seeds),
                RosterMove<MaterialRefSeed, Guid>.Retract(var plugins) => plugins.TraverseM(plugin => Admit.Confirm(
                    success: context.MaterialRefs.Remove(key: plugin))).As().Map(static _ => unit),
                _ => Fin.Fail<Unit>(error: new KernelFault.InvalidInput()),
            });

    private static Fin<Unit> Grown(ObjectAttributes attributes, Seq<DecalSeed> seeds) =>
        seeds.TraverseM(seed => Try.lift(() => {
                using Decal? minted = Decal.Create(createParams: seed.Build());
                return minted is null
                    ? Fin.Fail<Unit>(error: new KernelFault.InvalidResult())
                    : guard(attributes.Decals.Add(decal: minted) != 0u, new KernelFault.InvalidResult()).ToFin();
            }).Run().Bind(static inner => inner)).As()
            .Map(static _ => unit);

    private static Fin<Unit> Bound(ObjectAttributes attributes, Seq<MaterialRefSeed> seeds) =>
        seeds.TraverseM(seed => Try.lift(() => {
                using MaterialRef minted = attributes.MaterialRefs.Create(createParams: seed.Build());
                attributes.MaterialRefs.Add(key: seed.PlugIn, value: minted);
            }).Run().Bind(static inner => inner)).As()
            .Map(static _ => unit);
}
```

## [03]-[PROGRAM]

- Owner: `AttributeProgram` — the admitted edit sequence with one fold: `Apply(ObjectAttributes) : Fin<Unit>` runs every edit in declaration order over the working set and short-circuits on the first refusal, matching the `TableOp.Amend` change-callback contract exactly.
- Law: the program IS the `Amend` payload — `TableOp.Amend(target, program.Change, interaction)` is the one write path, where `Change` is the spine's `AttributeChange` value the program mints from its own fold: the table pipeline duplicates the live attribute set, the program mutates the duplicate, `ModifyAttributes` commits it under the undo bracket, and the duplicate disposes before the operation leaves the host boundary; a consumer holding a live `ObjectAttributes` and mutating it in place has no undo story and is the deleted form. `Apply` is therefore `internal` — the pipeline is its only caller, and a public overload IS that deleted form under a supported spelling.
- Law: the fold is short-circuit by construction — a program is one attribute transaction, so a mid-sequence refusal abandons the working duplicate uncommitted and the live object never sees a half-applied program; accumulation belongs to the caller batching programs across objects on the table pipeline's traversal.
- Law: `Tag` read verbs are refused at admission — `Of` rejects a program carrying a non-mutating `TagOp` so the refusal is a construction fact, never a mid-commit surprise.
- Growth: a new edit case rides every existing program untouched; a program-level policy is a field on this record, never a parallel program type.

```csharp
// --- [MODELS] --------------------------------------------------------------------------
public sealed class AttributeProgram {
    private AttributeProgram(Seq<AttributeEdit> edits) => Edits = edits;

    public Seq<AttributeEdit> Edits { get; }

    public static Fin<AttributeProgram> Of(params ReadOnlySpan<AttributeEdit> edits) {
        return from admitted in LanguageExt.Iterable<AttributeEdit>.FromSpan(edits).ToSeq()
                   .TraverseM(edit => Admit.Need(edit).Bind(value => value.Admit())).As()
               from _ in guard(!admitted.IsEmpty, new KernelFault.InvalidInput())
               select new AttributeProgram(edits: admitted);
    }

    public Fin<AttributeChange> Change =>
        FactoryBridge.Accept<AttributeChange>(
            AttributeChange.Validate(Apply, out AttributeChange? admitted), admitted);

    internal Fin<Unit> Apply(ObjectAttributes attributes) {
        return from working in Admit.Need(attributes)
               from _ in Edits.TraverseM(edit => edit.Apply(attributes: working)).As()
               select unit;
    }
}
```

## [04]-[SNAPSHOT_AND_EFFECTIVE]

- Owner: `AttributeAsk` `[Union]` closes stored and source-resolved questions; `AttributeAnswer` `[Union]` owns their detached rosters; `AttributeSnapshot` captures stored scalar state, group and override rosters, shadow roles, normalized meshing policy, render-material identity, user strings, complete decal rows, material-reference rows, and the attached-carrier census; `EffectiveDisplay` captures resolved color, plot, mode, and activity values.
- Entry: `Attributes.Ask(DocumentSession, TableTarget, AttributeAsk) : Fin<AttributeAnswer>` — one entry resolves through the state page's object fold and reads inside one `SessionNeed.Read` grant.
- Law: stored and effective are different questions — the snapshot reports what the attribute set declares, `EffectiveDisplay` reports what `DrawColor`/`ComputedPlotColor`/`ComputedPlotWeight` resolve after source dispatch against layer, parent, and material; a consumer diffing the two reads exactly which sources defer.
- Law: detail-hide is census membership — `HasHideInDetailOverrideSet(detailId)` is set membership in `GetHideInDetailOverrides()`, so the snapshot's `HiddenInDetails` roster answers any detail id and `EffectiveDisplay` stays a pure viewport question; a detail object id passed where a viewport id belongs is the conflation this split forecloses.
- Law: snapshot products contain detached values only, and a read product IS the write payload. Decals and material references project onto the SAME `DecalSeed` and `MaterialRefSeed` owners an edit carries, so a round trip re-imposes what was read without a column-by-column re-spelling, and a host row that cannot satisfy the seed's own construction law refuses at the read instead of entering a snapshot no write reproduces. Custom meshing round-trips through the normalized encoded value; every other foreign-owner carrier is one `AttachedModifier` row in the attached census.
- Law: render-material identity is NOT an attribute-set read — `ObjectAttributes.RenderMaterial` is a SET-ONLY property whose setter resolves the content's document owner, projects it to a `Material`, and lands it on `MaterialIndex`, so the stored render-material identity reads through `MaterialIndex` against the document material table on `materials.md`'s resolution pipeline; a read of the write-only property is a compile error wearing the shape of a projection.
- Law: the decal read composes the axes that READ TRUE — `HorzSweep`/`VertSweep` are the host's own replacements for the deprecated latitude and longitude properties, whose names invert their meaning, so the snapshot names its columns after the true axes and matches `DecalSeed`'s write vocabulary exactly; a round trip therefore requires no consumer to know the host's inversion, which stays confined to `DecalSeed.Build`.
- Boundary: `ComputedSectionStyle` demands a sectioner's attributes and stays a direct host call at the display boundary; this page resolves the three display scalars every consumer needs.

```csharp
// --- [TYPES] ---------------------------------------------------------------------------
[Union(SwitchMapStateParameterName = "context", ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record AttributeAsk {
    private AttributeAsk() { }
    public sealed record Stored : AttributeAsk;
    public sealed record Resolved(Option<Guid> Viewport = default) : AttributeAsk;

    internal Fin<AttributeAsk> Admit() =>
        Switch(stored: static (_, ask) => Fin.Succ<AttributeAsk>(ask),
            resolved: static (ask) => guard(
                ask.Viewport.Map(static value => value != Guid.Empty).IfNone(noneValue: true),
                new KernelFault.InvalidInput()).ToFin().Map(_ => (AttributeAsk)ask));
}

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record AttributeAnswer : IDetachedDocumentResult {
    private AttributeAnswer() { }
    public sealed record Declared(Seq<AttributeSnapshot> Rows) : AttributeAnswer;
    public sealed record Effective(Seq<EffectiveDisplay> Rows) : AttributeAnswer;
}

// --- [MODELS] --------------------------------------------------------------------------
public sealed record OverrideCensus(
    Seq<Guid> HiddenInDetails,
    Option<(Seq<Guid> Viewports, ObjectSignal Signal)> Activity,
    ObjectSignal DetailBackgroundVisible);

public readonly record struct DecalSnapshot(int Crc, ObjectSignal Visible, DecalSeed Seed) {
    internal static Fin<DecalSnapshot> Of(Decal decal) => Try.lift(() => {
        decal.GetUVBounds(out double minU, out double minV, out double maxU, out double maxV);
        decal.HorzSweep(out double horzStart, out double horzEnd);
        decal.VertSweep(out double vertStart, out double vertEnd);
        return FactoryBridge.Accept<DecalSeed>(
                fault: DecalSeed.Validate(
                    texture: decal.TextureInstanceId,
                    mapping: DecalFrame.Get(key: decal.Mapping),
                    projection: DecalFacing.Get(key: decal.Projection),
                    origin: decal.Origin,
                    up: decal.VectorUp,
                    across: decal.VectorAcross,
                    transparency: decal.Transparency,
                    mapToInside: ObjectSignal.Of(on: decal.MapToInside),
                    height: decal.Height,
                    radius: decal.Radius,
                    horzStart: horzStart,
                    horzEnd: horzEnd,
                    vertStart: vertStart,
                    vertEnd: vertEnd,
                    minU: minU,
                    minV: minV,
                    maxU: maxU,
                    maxV: maxV,
                    out DecalSeed? admitted),
                admitted: admitted)
            .Map(seed => new DecalSnapshot(
                Crc: decal.CRC, Visible: ObjectSignal.Of(on: decal.IsVisible), Seed: seed));
    }).Run().Bind(static inner => inner);
}

public readonly record struct MaterialRefSnapshot(Guid DictionaryKey, MaterialRefSeed Seed) {
    internal static Fin<MaterialRefSnapshot> Of(Guid key, MaterialRef live) =>
        FactoryBridge.Accept<MaterialRefSeed>(
                fault: MaterialRefSeed.Validate(
                    plugIn: live.PlugInId,
                    source: MaterialOrigin.Get(key: live.MaterialSource),
                    frontId: live.FrontFaceMaterialId,
                    backId: live.BackFaceMaterialId,
                    frontIndex: live.FrontFaceMaterialIndex,
                    backIndex: live.BackFaceMaterialIndex,
                    out MaterialRefSeed? admitted),
                admitted: admitted)
            .Map(seed => new MaterialRefSnapshot(DictionaryKey: key, Seed: seed));
}

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
    PrintPen Print,
    double LinetypePatternScale,
    int WireDensity,
    int DisplayOrder,
    EndDecoration Decoration,
    ObjectStance Stance,
    CapabilitySet<ShadowRole> Shadows,
    CapabilitySet<AttachedModifier> Attached,
    Seq<int> Groups,
    OverrideCensus Overrides,
    Option<ResourceIndex> SectionStyleIndex,
    SectionLabel Label,
    Plane Frame,
    Option<string> Meshing,
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
    PrintPen HatchBoundaryPen) : IDetachedDocumentResult {
    internal static Fin<AttributeSnapshot> Of(ObjectAttributes attributes) =>
        Try.lift(() => {
            bool overrides = attributes.GetActiveInViewportOverrides(viewportIds: out Guid[] viewports, active: out bool active);
            MeshingParameters? customMesh = attributes.EnableCustomMeshingParameters
                ? attributes.CustomMeshingParameters
                : null;
            return from layer in ResourceIndex.Admit(value: attributes.LayerIndex)
                from print in PrintPen.OfHost(weight: attributes.PlotWeight)
                from boundaryPen in PrintPen.OfHost(weight: attributes.HatchBoundaryPlotWeightMillimeters)
                from objectColor in AttributeShade.Of(color: attributes.ObjectColor)
                from plotColor in AttributeShade.Of(color: attributes.PlotColor)
                from hatchFill in AttributeShade.Of(color: attributes.HatchBackgroundFillColor)
                from hatchPrint in AttributeShade.Of(color: attributes.HatchBackgroundFillPrintColor)
                from boundaryColor in AttributeShade.Of(color: attributes.HatchBoundaryColor)
                from boundaryPlotColor in AttributeShade.Of(color: attributes.HatchBoundaryPlotColor)
                from decals in attributes.Decals.AsIterable().ToSeq()
                    .TraverseM(decal => DecalSnapshot.Of(decal: decal)).As()
                from materialRefs in attributes.MaterialRefs.AsIterable().ToSeq()
                    .TraverseM(pair => MaterialRefSnapshot.Of(live: pair.Value)).As()
                select new AttributeSnapshot(
                ObjectId: attributes.ObjectId,
                Name: HostEdge.Text(attributes.Name),
                Url: HostEdge.Text(attributes.Url),
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
                Print: print,
                LinetypePatternScale: attributes.LinetypePatternScale,
                WireDensity: attributes.WireDensity,
                DisplayOrder: attributes.DisplayOrder,
                Decoration: EndDecoration.Get(key: attributes.ObjectDecoration),
                Stance: ObjectStance.Get(key: attributes.Mode),
                Shadows: ShadowRole.Of(attributes: attributes),
                Attached: AttachedModifier.Of(attributes: attributes),
                Groups: toSeq(attributes.GetGroupList()),
                Overrides: new OverrideCensus(
                    HiddenInDetails: toSeq(attributes.GetHideInDetailOverrides()),
                    Activity: overrides
                        ? Some((toSeq(viewports), ObjectSignal.Of(on: active)))
                        : Option<(Seq<Guid>, ObjectSignal)>.None,
                    DetailBackgroundVisible: ObjectSignal.Of(on: attributes.DetailBackgroundVisible)),
                SectionStyleIndex: ResourceIndex.Maybe(value: attributes.SectionStyleIndex),
                Label: SectionLabel.Get(key: attributes.ClippingPlaneLabelStyle),
                Frame: attributes.ObjectFrame(),
                Meshing: Optional(customMesh).Map(static parameters => parameters.ToEncodedString()),
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
                HatchBoundaryPen: boundaryPen);
        }).Run().Bind(static inner => inner);
}

public readonly record struct EffectiveDisplay(
    Guid Id,
    PerceptualColor Draw,
    PerceptualColor Plot,
    PrintPen Print,
    Option<Guid> ModeOverride,
    Option<ObjectSignal> ActiveOverride) : IDetachedDocumentResult {
    internal static Fin<EffectiveDisplay> Of(RhinoObject native, Rhino.RhinoDoc document, Option<Guid> viewport) =>
        Try.lift(() => {
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
            return from draw in AttributeShade.Of(color: resolved.Draw)
                   from plot in AttributeShade.Of(color: resolved.Plot)
                   from print in PrintPen.OfHost(weight: resolved.Weight)
                   select new EffectiveDisplay(
                       Id: native.Id,
                       Draw: draw,
                       Plot: plot,
                       Print: print,
                       ModeOverride: resolved.Mode,
                       ActiveOverride: resolved.Active);
        }).Run().Bind(static inner => inner);
}

// --- [OPERATIONS] ----------------------------------------------------------------------
public static class Attributes {
    public static Fin<AttributeAnswer> Ask(DocumentSession session, TableTarget target, AttributeAsk ask) {
        return from active in Admit.Need(ask).Bind(value => value.Admit())
               from answer in session.Demand(
                   use: document =>
                       from natives in Objects.Resolve(document: document, target: target)
                       from folded in active.Switch(
                           (Document: document, Natives: natives),
                           stored: static (ctx, _) => ctx.Natives
                               .TraverseM(native => AttributeSnapshot.Of(attributes: native.Attributes)).As()
                               .Map(static rows => (AttributeAnswer)new AttributeAnswer.Declared(Rows: rows)),
                           resolved: static (ctx, ask) => ctx.Natives
                               .TraverseM(native => EffectiveDisplay.Of(
                                   native: native, document: ctx.Document, viewport: ask.Viewport)).As()
                               .Map(static rows => (AttributeAnswer)new AttributeAnswer.Effective(Rows: rows)))
                       select folded,
                   needs: [SessionNeed.Read])
               select answer;
    }
}
```

## [05]-[SURFACE_LEDGER]

| [INDEX] | [CONCERN]          | [OWNER]             | [FORM]                                                 | [ENTRY]                    |
| :-----: | :----------------- | :------------------ | :----------------------------------------------------- | :------------------------- |
|  [01]   | attribute axes     | keyed origin rows   | one row per host discriminant, `FromObject` on sources | `.Key` write / `Get` read  |
|  [02]   | object mode        | `ObjectStance`      | the four-valued host word as one read row              | `AttributeAsk.Stored`      |
|  [03]   | colour crossing    | `AttributeShade`    | the only two host-colour boundaries on the page        | `Of` / `Rgb`               |
|  [04]   | plot weight        | `PrintPen`          | layer-plane pen, rung or named host posture            | `OfHost` / `ToHost`        |
|  [05]   | shadow roles       | `ShadowRole`        | cast and receive as one held column                    | `CapabilitySet.Admits`     |
|  [06]   | attached carriers  | `AttachedModifier`  | eight presence rows owning their host reads            | `CapabilitySet.Admits`     |
|  [07]   | attribute mutation | `AttributeEdit`     | admitted union, one total `Apply` over the duplicate   | program payloads           |
|  [08]   | set-valued edits   | `RosterMove`        | impose/extend/retract cases carrying their own rosters | owning edit payloads       |
|  [09]   | detached carriers  | generated seeds     | `DecalSeed`/`MaterialRefSeed` on both read and write   | `Decals` / `FaceMaterials` |
|  [10]   | write program      | `AttributeProgram`  | short-circuit fold minting the spine's change payload  | `TableOp.Amend`            |
|  [11]   | read dispatch      | `AttributeAsk`      | stored and resolved questions, one typed answer union  | `Attributes.Ask`           |
|  [12]   | stored state       | `AttributeSnapshot` | detached scalars, rosters, and admitted seeds          | `AttributeAsk.Stored`      |
|  [13]   | resolved display   | `EffectiveDisplay`  | resolved colour, pen, and viewport overrides           | `AttributeAsk.Resolved`    |

## [06]-[RESEARCH]

<!-- source-only: research row template:
[TOKEN]-[OPEN|BLOCKED]: <exact question>; <verification route>.
-->

(none)
