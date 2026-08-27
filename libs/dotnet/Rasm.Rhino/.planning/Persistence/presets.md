# [RASM_RHINO_PERSISTENCE_PRESETS]

`Rasm.Rhino.Persistence` mints the sub-domain's one admission-refusal family here, seats the construction-plane value vocabulary the persisted preset and the live viewport read share, and closes named construction planes, named positions, and named layer states behind two entries. `Presets.Read` answers a detached census or one stored transform; `Presets.Commit` admits an ordered mutation program and frames it in the document's one commit envelope. Named views remain viewport ownership.

## [01]-[INDEX]

- [02]-[FAULT]: `PersistenceFault` — the sub-domain's one admission-refusal family on the kernel band registry, and the folder law that seats it.
- [03]-[CPLANE]: `CPlaneTrait`, `CPlaneGrid`, `CPlanePalette`, `CPlaneModel` — the construction-plane value vocabulary this page seats and `Viewport/camera` composes.
- [04]-[VOCABULARY]: `PresetName`, `LayerFacet`, `LayerRestore`, `PositionRef`, `PositionVerb`, `PresetExecution`, `PresetQuery`, `PresetOperation`, and the detached census records.
- [05]-[INTERPRETER]: `Presets.Read` and `Presets.Commit` — the census entry and the framed mutation program.

## [02]-[FAULT]

- Owner: `PersistenceFault` is the direct persistence-host family on `FaultBand.HostPersistence`; generated-value refusals cross the kernel validation bridge.
- Cases: `HostRefused`, `Diverged`, `AbsentEntry`, and `Resident` preserve the semantic boundary failure and its evidence.
- Law: generated owners stamp `[ValidationError]`; public accumulation rides `Validation<Error, T>`, and foreign errors retain their exact identity.
- Law: the generated fault-case identity supplies the numeric code, while this root's total `Message` switch supplies presentation.
- Boundary: `PersistenceFault` never represents generated validation, aggregates, categories, or wire envelopes.
- Packages: `Domain/results`, Thinktecture.Runtime.Extensions, and LanguageExt.Core.

```csharp
// --- [IMPORTS] -------------------------------------------------------------------------
using Rasm.Domain;
using Thinktecture;

namespace Rasm.Rhino.Persistence;

// --- [ERRORS] --------------------------------------------------------------------------
[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record PersistenceFault : Fault {
    private static readonly FaultBand FamilyBand = FaultBand.HostPersistence;
    private PersistenceFault() { }

    [FaultCase(0)] public sealed partial record HostRefused(string Member, string Detail) : PersistenceFault;
    [FaultCase(1)] public sealed partial record Diverged(string Subject, string Expected, string Observed) : PersistenceFault;
    [FaultCase(2)] public sealed partial record AbsentEntry(string Table, string Entry) : PersistenceFault;
    [FaultCase(3)] public sealed partial record Resident(string Subject) : PersistenceFault;

    public sealed override string Message => Switch(
        hostRefused: static fault => $"Persistence host member '{fault.Member}' refused '{fault.Key}': {fault.Detail}",
        diverged: static fault => $"Persistence subject '{fault.Subject}' diverged for '{fault.Key}': expected '{fault.Expected}', observed '{fault.Observed}'.",
        absentEntry: static fault => $"Persistence table '{fault.Table}' has no entry '{fault.Entry}' for '{fault.Key}'.",
        resident: static fault => $"Persistence subject '{fault.Subject}' is already resident for '{fault.Key}'.");
}
```

## [03]-[CPLANE]

- Owner: `CPlaneTrait` — the construction-plane display vocabulary, one `ICapability` row per host visibility bit, each carrying its host read and its host write as delegate columns; `CPlaneGrid` — spacing, snap, counts, and the held trait set; `CPlanePalette` — the five admitted inks; `CPlaneModel` — the named persisted plane. All four seat HERE, at the lowest stratum both consumers reach, and `Viewport/camera`'s live `CPlaneState` composes them (folder `RULINGS.md [02]`, E-R33).
- Entry: `CPlaneTrait.Of(ConstructionPlane)` folds the host bits into the set and `CPlaneTrait.Apply(set, ConstructionPlane)` writes them back; `CPlaneGrid.Of` admits authored metrics and `CPlaneGrid.Read` projects a host plane; `CPlanePalette.Read` admits the five inks accumulating and `CPlanePalette.Apply` egresses them accumulating; `CPlaneModel.Read` composes both halves with the name, and `CPlaneModel.Native` builds the host carrier one write can add.
- Auto: the host bit correspondence has ONE authority. Each `CPlaneTrait` row owns the property it reads and the property it writes, so `Of` and `Apply` are the same roster walked in two directions — the four-line `ShowGrid`/`ShowAxes`/`ShowZAxis`/`DepthBuffered` initializer the write spelled and the four-line predicate chain the read spelled are the same fold, and a fifth host visibility bit is one row that both directions pick up untouched.
- Law: the persisted preset and the live viewport read carry ONE value vocabulary and differ only in custody — `CPlaneModel` names a stored row addressed by `PresetName`, `Viewport/camera`'s `CPlaneState` names a live read whose name is `Option<string>` because an unnamed active plane is ordinary. A second vocabulary lets a preset round-trip through the viewport and lose a column no compiler names.
- Law: the five grid inks admit ONCE at the host read and egress ONCE at the host write, both accumulating — a raw `System.Drawing` colour never crosses out of this sub-domain (`ARCHITECTURE.md` screen-carrier ban; `PerceptualColor` is the kernel contract), and one capture reports every rejected channel together. `ToDrawing` answers `Fin`, so an out-of-gamut ink refuses at the write where the retired `ToRgb` egress clamped it silently.
- Law: `CapabilityLaw<CPlaneTrait>.Open` is the DECLARED law, not an omission — depth buffering is a raster posture independent of what the grid draws, and a plane showing nothing is the host's own blank construction plane, so all sixteen corners are legal and the law says so at the one site a later closure binds.
- Exemption: no `[Mapper]` boundary. Nine of the fifteen host crossings are fallible admissions (`PerceptualColor.OfHost`, `ToDrawing`) or a set fold over a capability roster, so Mapperly generates six trivial member copies beside nine `[MapProperty(Use = …)]` rows pointing back at the bodies below — the same exemption the kernel `AdmissionProjection` states, for the same reason: there is no member-to-member correspondence to generate.
- Growth: a new host visibility bit is one `CPlaneTrait` row; a new grid metric is one `CPlaneGrid` column and its clause; a new axis ink is one `CPlanePalette` column inside the existing accumulation.
- Boundary: this page owns the construction-plane VALUE parts and the named-preset table crossing. The live viewport borrow, its lease, and the active-plane write are `Viewport/camera`'s; nothing here holds a `RhinoViewport`.
- Packages: RhinoCommon (`libs/dotnet/Rasm.Rhino/.api/api-rhinocommon-document-state.md` — `ConstructionPlane.Plane`/`GridSpacing`/`SnapSpacing`/`GridLineCount`/`ThickLineFrequency`/`ShowGrid`/`ShowAxes`/`ShowZAxis`/`DepthBuffered`/`ThinLineColor`/`ThickLineColor`/`GridXColor`/`GridYColor`/`GridZColor`, each a plain auto property with no has-custom-colour flag); kernel `Numerics/atoms` (`PerceptualColor.OfHost`/`ToDrawing`); kernel `Domain/validation` (`ICapability`, `CapabilitySet`, `CapabilityLaw`); Thinktecture.Runtime.Extensions; LanguageExt.Core; `System.Drawing.Common` (`libs/dotnet/.api/api-system-drawing-common.md` — `Color`).

```csharp
// --- [IMPORTS] -------------------------------------------------------------------------
using System.Drawing;
using Generator.Equals;
using Rasm.Domain;
using Rasm.Numerics;
using Rhino.DocObjects;
using Rhino.Geometry;
using Thinktecture;

namespace Rasm.Rhino.Persistence;

// --- [TYPES] ---------------------------------------------------------------------------
[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class CPlaneTrait : ICapability<CPlaneTrait> {
    public static readonly CPlaneTrait Grid = new(key: "grid",
        read: static plane => plane.ShowGrid,
        write: static (plane, held) => { plane.ShowGrid = held; return unit; });
    public static readonly CPlaneTrait Axes = new(key: "axes",
        read: static plane => plane.ShowAxes,
        write: static (plane, held) => { plane.ShowAxes = held; return unit; });
    public static readonly CPlaneTrait ZAxis = new(key: "z-axis",
        read: static plane => plane.ShowZAxis,
        write: static (plane, held) => { plane.ShowZAxis = held; return unit; });
    public static readonly CPlaneTrait DepthBuffered = new(key: "depth-buffered",
        read: static plane => plane.DepthBuffered,
        write: static (plane, held) => { plane.DepthBuffered = held; return unit; });

    public static CapabilityLaw<CPlaneTrait> Law => CapabilityLaw<CPlaneTrait>.Open;

    [UseDelegateFromConstructor] internal partial bool Read(ConstructionPlane source);

    [UseDelegateFromConstructor] internal partial Unit Write(ConstructionPlane target, bool held);

    internal static CapabilitySet<CPlaneTrait> Of(ConstructionPlane source) =>
        CapabilitySet<CPlaneTrait>.Of(toSeq(Items).Filter(row => row.Read(source: source)).ToArray());

    internal static Unit Apply(CapabilitySet<CPlaneTrait> held, ConstructionPlane target) =>
        toSeq(Items).Fold(unit, (_, row) => row.Write(target: target, held: held.Admits(capability: row)));
}

// --- [MODELS] --------------------------------------------------------------------------
[ComplexValueObject]
[ValidationError]
public sealed partial record CPlaneGrid(
    double Spacing,
    double Snap,
    int LineCount,
    int ThickFrequency,
    CapabilitySet<CPlaneTrait> Traits) {
    static partial void ValidateFactoryArguments(
        ref ValidationError? validationError,
        ref double spacing,
        ref double snap,
        ref int lineCount,
        ref int thickFrequency,
        ref CapabilitySet<CPlaneTrait> traits) {
        (double pitch, double quantum, int count, int frequency) = (spacing, snap, lineCount, thickFrequency);
        validationError = FactoryValidation.Of(FactoryValidation.Violated(
                (!ValidityClaim.Positive(value: pitch),
                    () => new ValidationClause(string.Join(" | ", new object?[] { op, nameof(Spacing), pitch, "a finite positive grid pitch" }))),
                (!ValidityClaim.Positive(value: quantum),
                    () => new ValidationClause(string.Join(" | ", new object?[] { op, nameof(Snap), quantum, "a finite positive snap quantum" }))),
                (count < 1,
                    () => new ValidationClause(string.Join(" | ", new object?[] { op, nameof(LineCount), count, "at least one grid line" }))),
                (frequency < 1,
                    () => new ValidationClause(string.Join(" | ", new object?[] { op, nameof(ThickFrequency), frequency, "a thick-line cadence of at least one" })))));
    }

    public static Fin<CPlaneGrid> Of(
        double spacing,
        double snap,
        int lineCount,
        int thickFrequency,
        CapabilitySet<CPlaneTrait> traits) {
        return from admitted in CPlaneTrait.Law.Admit(held: traits)
               from grid in FactoryBridge.Accept<CPlaneGrid>(
                   Validate(spacing, snap, lineCount, thickFrequency, admitted, out CPlaneGrid? value),
                   value)
               select grid;
    }

    public static Fin<CPlaneGrid> Read(ConstructionPlane source) {
        return Admit.Need(value: source).Bind(plane => Try.lift(() => Of(
            spacing: plane.GridSpacing,
            snap: plane.SnapSpacing,
            lineCount: plane.GridLineCount,
            thickFrequency: plane.ThickLineFrequency,
            traits: CPlaneTrait.Of(source: plane))).Run().Bind(static inner => inner));
    }

    internal Fin<Unit> Apply(ConstructionPlane target) {
        CPlaneGrid self = this;
        return Admit.Need(value: target).Bind(plane => Try.lift(() => {
            plane.GridSpacing = self.Spacing;
            plane.SnapSpacing = self.Snap;
            plane.GridLineCount = self.LineCount;
            plane.ThickLineFrequency = self.ThickFrequency;
            return Fin.Succ(value: CPlaneTrait.Apply(held: self.Traits, target: plane));
        }).Run().Bind(static inner => inner));
    }
}

[Equatable]
public sealed partial record CPlanePalette(
    PerceptualColor ThinLine,
    PerceptualColor ThickLine,
    PerceptualColor GridX,
    PerceptualColor GridY,
    PerceptualColor GridZ) {
    public static Fin<CPlanePalette> Read(ConstructionPlane source) {
        return Admit.Need(value: source).Bind(plane => (
                PerceptualColor.OfHost(host: plane.ThinLineColor).ToValidation(),
                PerceptualColor.OfHost(host: plane.ThickLineColor).ToValidation(),
                PerceptualColor.OfHost(host: plane.GridXColor).ToValidation(),
                PerceptualColor.OfHost(host: plane.GridYColor).ToValidation(),
                PerceptualColor.OfHost(host: plane.GridZColor).ToValidation())
            .Apply(static (thin, thick, x, y, z) => new CPlanePalette(
                ThinLine: thin, ThickLine: thick, GridX: x, GridY: y, GridZ: z))
            .As()
            .ToFin());
    }

    internal Fin<Unit> Apply(ConstructionPlane target) {
        CPlanePalette self = this;
        return Admit.Need(value: target).Bind(plane => (
                self.ThinLine.ToDrawing().ToValidation(),
                self.ThickLine.ToDrawing().ToValidation(),
                self.GridX.ToDrawing().ToValidation(),
                self.GridY.ToDrawing().ToValidation(),
                self.GridZ.ToDrawing().ToValidation())
            .Apply(static (thin, thick, x, y, z) => (Thin: thin, Thick: thick, X: x, Y: y, Z: z))
            .As()
            .ToFin()
            .Bind(inks => Try.lift(() => {
                plane.ThinLineColor = inks.Thin;
                plane.ThickLineColor = inks.Thick;
                plane.GridXColor = inks.X;
                plane.GridYColor = inks.Y;
                plane.GridZColor = inks.Z;
                return Fin.Succ(value: unit);
            }).Run().Bind(static inner => inner)));
    }
}

[ComplexValueObject]
[ValidationError]
public sealed partial record CPlaneModel(
    PresetName Name,
    Plane Plane,
    CPlaneGrid Grid,
    CPlanePalette Palette) {
    static partial void ValidateFactoryArguments(
        ref ValidationError? validationError,
        ref PresetName name,
        ref Plane plane,
        ref CPlaneGrid grid,
        ref CPlanePalette palette) {
        (Plane frame, CPlaneGrid metrics, CPlanePalette inks) = (plane, grid, palette);
        validationError = FactoryValidation.Of(FactoryValidation.Violated(
                (!ValidityClaim.All(frame.IsValid),
                    () => new ValidationClause(string.Join(" | ", new object?[] { op, nameof(Plane), "a valid construction-plane frame" }))),
                (metrics is null, () => new ValidationClause(string.Join(" | ", new object?[] { op, nameof(Grid) }))),
                (inks is null, () => new ValidationClause(string.Join(" | ", new object?[] { op, nameof(Palette) })))));
    }

    public static Fin<CPlaneModel> Of(PresetName name, Plane plane, CPlaneGrid grid, CPlanePalette palette) =>
        FactoryBridge.Accept<CPlaneModel>(
            Validate(name, plane, grid, palette, out CPlaneModel? value),
            value);

    internal static Fin<CPlaneModel> Read(ConstructionPlane source) =>
        from plane in Admit.Need(value: source)
        from name in FactoryBridge.Accept<PresetName>(candidate: plane.Name)
        from parts in (
                CPlaneGrid.Read(source: plane).ToValidation(),
                CPlanePalette.Read(source: plane).ToValidation())
            .Apply(static (grid, palette) => (Grid: grid, Palette: palette))
            .As()
            .ToFin()
        from model in Of(name: name, plane: plane.Plane, grid: parts.Grid, palette: parts.Palette)
        select model;

    internal Fin<ConstructionPlane> Native() {
        CPlaneModel self = this;
        return from plane in Try.lift(() => Fin.Succ(value: new ConstructionPlane { Name = self.Name.Value, Plane = self.Plane })).Run().Bind(static inner => inner)
               from _grid in self.Grid.Apply(target: plane)
               from _palette in self.Palette.Apply(target: plane)
               select plane;
    }
}
```

## [04]-[VOCABULARY]

- Owner: `PresetName` admits the host key text; `LayerFacet` is the fifteen-row property vocabulary `RestoreLayerProperties` publishes and `LayerRestore` the two-case restore scope over it; `PositionRef` addresses a named position by id or name and `PositionVerb` carries the two reapply verbs with their execution posture; `PresetExecution` is the commit posture roster; `PresetQuery` and `PresetOperation` are the two request families; `PositionObject`, `PositionSnapshot`, `LayerStateSnapshot`, and `PresetSnapshot` are the detached census records.
- Entry: every request mints through a `public static` factory answering `Fin` — the case records are `internal`, so the factory is the only ingress and the generated `Switch` the only egress (`[SEALED_ADMISSION]`). The prior nine-member `Admit` family that re-admitted an already-constructed operation deletes whole: a request that exists is a request that passed its clauses.
- Auto: `Execution` is a total projection off the operation family, so a new mutation verb declares its posture on the case and the commit fold derives its session needs.
- Auto: `LayerFacet.Bit` is the one authority for the host flag word — `CapabilitySet<LayerFacet>.Mask(row => row.Bit)` composes it forward and `OfMask` inverts it, so the hand `Fold` over `|` and the private `Native` column both delete.
- Law: reads and mutations are TWO request families because they share no session need, no commit framing, and no answer shape. The prior single family paid for that with fifteen `InvalidInput` refusal arms, one per case the other verb refuses, and a reader ran the switch to learn which half a case belonged to. NAMED LOSS: one entrypoint; witness — `Presets.Read`'s two arms and `Presets.Commit`'s twelve are exactly the arms that carried a body before, with every refusal arm gone.
- Law: `PresetName` REFUSES surrounding whitespace rather than trimming it. The admitted name IS the host key — `Delete`, `Rename`, and every roster lookup address the table by it — so a trim keys the host on a name it does not hold, silently misses the mutation, and reports a false refusal.
- Law: `LayerRestore.AllCase` stays a case rather than folding into a full capability set, because the host sentinel is `RestoreLayerProperties.All = uint.MaxValue` — a word that names every bit including the fourteen the enum does not define. `CapabilitySet<LayerFacet>.All.Mask(bit)` answers `0x7FFF`, so the two are different requests to the host and the case is the discriminant.
- Law: every address column takes its Document spine owner — `ResourceId` for object and position identity, `DocumentPath` for the import archive — so a host member reporting failure as an empty guid can never seat a request the interpreter then addresses. The `.3dm` requirement rides the import factory's own clause, because "an archive this table can read" is a preset fact and not a property of every document path.
- Growth: a new host property group is one `LayerFacet` row; a new position verb is one `PositionVerb` row with its posture.
- Boundary: named views stay in `Viewport/operations`; the layer tree — topology, face, and per-detail overrides — lives on `Document/layers`, and `LayerRestore` consumes only the host's state mask.
- Packages: RhinoCommon (`libs/dotnet/Rasm.Rhino/.api/api-rhinocommon-document-state.md` — `NamedConstructionPlaneTable.Add`/`Find`/`Delete`/indexer, `NamedPositionTable.Save`/`Restore`/`Update`/`Append`/`Rename`/`Delete`/`ObjectXform`/`ObjectIds`/`Ids`/`Names`/`Id`/`Name`, `NamedLayerStateTable.Save`/`Restore`/`Rename`/`Delete`/`Import`/`Names`, `[Flags] RestoreLayerProperties : uint`); `Document/session` (`DocumentSession`, `SessionNeed`, `UndoCustody`, `DocumentPath`); `Document/tables` (`ResourceId`); `Document/commit` (`RedrawPolicy`); kernel `Domain/validation` (`CapabilitySet`, `CapabilityLaw`); Thinktecture.Runtime.Extensions; LanguageExt.Core.

```csharp
// --- [IMPORTS] -------------------------------------------------------------------------
using System.IO;
using Generator.Equals;
using Rasm.Domain;
using Rasm.Rhino.Document;
using Rhino;
using Rhino.DocObjects.Tables;
using Rhino.Geometry;
using Thinktecture;

namespace Rasm.Rhino.Persistence;

// --- [TYPES] ---------------------------------------------------------------------------
[ValueObject<string>(KeyMemberName = "Value", KeyMemberAccessModifier = AccessModifier.Public)]
[ValidationError]
public readonly partial struct PresetName : IDisallowDefaultValue {
    static partial void ValidateFactoryArguments(ref ValidationError? validationError, ref string value) {
        string candidate = value ?? string.Empty;
        validationError = FactoryValidation.Of(FactoryValidation.Violated(
                (string.IsNullOrWhiteSpace(candidate), () => new ValidationClause(string.Join(" | ", new object?[] { op, nameof(PresetName) }))),
                (candidate.Length != candidate.Trim().Length,
                    () => new ValidationClause(string.Join(" | ", new object?[] { op, nameof(PresetName), "a name carrying no surrounding whitespace" })))));
    }
}

[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class LayerFacet : ICapability<LayerFacet> {
    public static readonly LayerFacet Current = new(key: "current", bit: (int)RestoreLayerProperties.Current);
    public static readonly LayerFacet Visible = new(key: "visible", bit: (int)RestoreLayerProperties.Visible);
    public static readonly LayerFacet Locked = new(key: "locked", bit: (int)RestoreLayerProperties.Locked);
    public static readonly LayerFacet Color = new(key: "color", bit: (int)RestoreLayerProperties.Color);
    public static readonly LayerFacet Linetype = new(key: "linetype", bit: (int)RestoreLayerProperties.Linetype);
    public static readonly LayerFacet PrintColor = new(key: "print-color", bit: (int)RestoreLayerProperties.PrintColor);
    public static readonly LayerFacet PrintWidth = new(key: "print-width", bit: (int)RestoreLayerProperties.PrintWidth);
    public static readonly LayerFacet ViewportVisible = new(key: "viewport-visible", bit: (int)RestoreLayerProperties.ViewportVisible);
    public static readonly LayerFacet ViewportColor = new(key: "viewport-color", bit: (int)RestoreLayerProperties.ViewportColor);
    public static readonly LayerFacet ViewportPrintColor = new(key: "viewport-print-color", bit: (int)RestoreLayerProperties.ViewportPrintColor);
    public static readonly LayerFacet ViewportPrintWidth = new(key: "viewport-print-width", bit: (int)RestoreLayerProperties.ViewportPrintWidth);
    public static readonly LayerFacet RenderMaterial = new(key: "render-material", bit: (int)RestoreLayerProperties.RenderMaterial);
    public static readonly LayerFacet SectionStyle = new(key: "section-style", bit: (int)RestoreLayerProperties.SectionStyle);
    public static readonly LayerFacet NewDetailOn = new(key: "new-detail-on", bit: (int)RestoreLayerProperties.NewDetailOn);
    public static readonly LayerFacet Expanded = new(key: "expanded", bit: (int)RestoreLayerProperties.Expanded);

    internal int Bit { get; }

    public static CapabilityLaw<LayerFacet> Law => CapabilityLaw<LayerFacet>.Open;
}

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record LayerRestore {
    private LayerRestore() { }

    internal sealed record AllCase : LayerRestore;
    internal sealed record SelectedCase(CapabilitySet<LayerFacet> Facets) : LayerRestore;

    public static LayerRestore All() => new AllCase();

    public static Fin<LayerRestore> Selected(CapabilitySet<LayerFacet> facets) {
        return from admitted in LayerFacet.Law.Admit(held: facets)
               from _held in guard(!admitted.Held.IsEmpty,
                   (Error)new KernelFault.InvalidValue(nameof(LayerRestore), string.Join(" | ", new object?[] { op, "at least one restored property group" })))
               select (LayerRestore)new SelectedCase(Facets: admitted);
    }

    internal RestoreLayerProperties ToNative() => Switch<RestoreLayerProperties>(
        allCase: static _ => RestoreLayerProperties.All,
        selectedCase: static selected => (RestoreLayerProperties)selected.Facets.Mask(bit: static row => row.Bit));
}

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record PositionRef {
    private PositionRef() { }

    internal sealed record IdCase(ResourceId Id) : PositionRef;
    internal sealed record NameCase(PresetName Name) : PositionRef;

    public static Fin<PositionRef> Of(Guid id) =>
        ResourceId.Admit(value: id).Map<PositionRef>(static admitted => new IdCase(Id: admitted));

    public static Fin<PositionRef> Of(string name) =>
        FactoryBridge.Accept<PresetName>(candidate: name)
            .Map<PositionRef>(static admitted => new NameCase(Name: admitted));
}

[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class PositionVerb {
    public static readonly PositionVerb Restore = new(
        key: "restore",
        execution: PresetExecution.Restore,
        apply: static (table, id) => table.Restore(id));
    public static readonly PositionVerb Update = new(
        key: "update",
        execution: PresetExecution.Mutate,
        apply: static (table, id) => table.Update(id));

    internal PresetExecution Execution { get; }

    [UseDelegateFromConstructor] internal partial bool Apply(NamedPositionTable table, Guid id);
}

[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class PresetExecution {
    public static readonly PresetExecution Mutate = new(key: "mutate", rank: 0, redraw: RedrawPolicy.None);
    public static readonly PresetExecution Restore = new(key: "restore", rank: 1, redraw: RedrawPolicy.Continuous);

    internal int Rank { get; }

    internal RedrawPolicy Redraw { get; }

    internal Seq<SessionNeed> Needs() => SessionNeed.Mutation(custody: UndoCustody.Recorded, redraw: Redraw);

    internal static PresetExecution Strongest(Seq<PresetExecution> postures) =>
        postures.Fold(Mutate, static (held, row) => row.Rank > held.Rank ? row : held);
}

// --- [MODELS] --------------------------------------------------------------------------
[Equatable]
public sealed partial record PositionObject(ResourceId ObjectId, Transform Transform);

[Equatable]
public sealed partial record PositionSnapshot(
    ResourceId Id,
    PresetName Name,
    [property: OrderedEquality] Seq<PositionObject> Objects);

[Equatable]
public sealed partial record LayerStateSnapshot([property: OrderedEquality] Seq<PresetName> Names);

[Equatable]
public sealed partial record PresetSnapshot(
    [property: OrderedEquality] Seq<CPlaneModel> ConstructionPlanes,
    [property: OrderedEquality] Seq<PositionSnapshot> Positions,
    LayerStateSnapshot LayerStates);

// --- [BOUNDARIES] ----------------------------------------------------------------------
[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record PresetQuery {
    private PresetQuery() { }

    internal sealed record CensusCase : PresetQuery;
    internal sealed record TransformCase(PositionRef Position, ResourceId ObjectId) : PresetQuery;

    public static PresetQuery Census() => new CensusCase();

    public static Fin<PresetQuery> Transform(PositionRef position, Guid objectId) {
        return (Admit.Need(value: position).ToValidation(), ResourceId.Admit(value: objectId).ToValidation())
            .Apply(static (address, id) => (PresetQuery)new TransformCase(Position: address, ObjectId: id))
            .As()
            .ToFin();
    }
}

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record PresetOperation {
    private PresetOperation() { }

    internal sealed record PutCPlaneCase(CPlaneModel Model) : PresetOperation;
    internal sealed record DeleteCPlaneCase(PresetName Name) : PresetOperation;
    internal sealed record SavePositionCase(PresetName Name, Seq<ResourceId> ObjectIds) : PresetOperation;
    internal sealed record ApplyPositionCase(PositionRef Position, PositionVerb Verb) : PresetOperation;
    internal sealed record AppendPositionCase(PositionRef Position, Seq<ResourceId> ObjectIds) : PresetOperation;
    internal sealed record RenamePositionCase(PositionRef Position, PresetName Name) : PresetOperation;
    internal sealed record DeletePositionCase(PositionRef Position) : PresetOperation;
    internal sealed record SaveLayerStateCase(PresetName Name, Option<Guid> ViewportId) : PresetOperation;
    internal sealed record RestoreLayerStateCase(PresetName Name, LayerRestore Properties, Option<Guid> ViewportId) : PresetOperation;
    internal sealed record RenameLayerStateCase(PresetName Current, PresetName Next) : PresetOperation;
    internal sealed record DeleteLayerStateCase(PresetName Name) : PresetOperation;
    internal sealed record ImportLayerStatesCase(DocumentPath Path) : PresetOperation;

    public static Fin<PresetOperation> PutCPlane(CPlaneModel model) =>
        Admit.Need(value: model).Map<PresetOperation>(static admitted => new PutCPlaneCase(Model: admitted));

    public static Fin<PresetOperation> DeleteCPlane(string name) =>
        Named(name: name).Map<PresetOperation>(static admitted => new DeleteCPlaneCase(Name: admitted));

    public static Fin<PresetOperation> SavePosition(string name, params ReadOnlySpan<Guid> objectIds) {
        return (Named(name: name).ToValidation(), Participants(ids: objectIds).ToValidation())
            .Apply(static (admitted, ids) => (PresetOperation)new SavePositionCase(Name: admitted, ObjectIds: ids))
            .As()
            .ToFin();
    }

    public static Fin<PresetOperation> ApplyPosition(PositionRef position, PositionVerb verb) {
        return (Admit.Need(value: position).ToValidation(), Admit.Need(value: verb).ToValidation())
            .Apply(static (address, row) => (PresetOperation)new ApplyPositionCase(Position: address, Verb: row))
            .As()
            .ToFin();
    }

    public static Fin<PresetOperation> AppendPosition(PositionRef position, params ReadOnlySpan<Guid> objectIds) {
        return (Admit.Need(value: position).ToValidation(), Participants(ids: objectIds).ToValidation())
            .Apply(static (address, ids) => (PresetOperation)new AppendPositionCase(Position: address, ObjectIds: ids))
            .As()
            .ToFin();
    }

    public static Fin<PresetOperation> RenamePosition(PositionRef position, string name) {
        return (Admit.Need(value: position).ToValidation(), Named(name: name).ToValidation())
            .Apply(static (address, admitted) => (PresetOperation)new RenamePositionCase(Position: address, Name: admitted))
            .As()
            .ToFin();
    }

    public static Fin<PresetOperation> DeletePosition(PositionRef position) =>
        Admit.Need(value: position).Map<PresetOperation>(static address => new DeletePositionCase(Position: address));

    public static Fin<PresetOperation> SaveLayerState(string name, Option<Guid> viewportId = default) {
        return (Named(name: name).ToValidation(), Viewport(viewport: viewportId).ToValidation())
            .Apply(static (admitted, view) => (PresetOperation)new SaveLayerStateCase(Name: admitted, ViewportId: view))
            .As()
            .ToFin();
    }

    public static Fin<PresetOperation> RestoreLayerState(string name, LayerRestore properties, Option<Guid> viewportId = default) {
        return (Named(name: name).ToValidation(), Admit.Need(value: properties).ToValidation(), Viewport(viewport: viewportId).ToValidation())
            .Apply(static (admitted, scope, view) => (PresetOperation)new RestoreLayerStateCase(Name: admitted, Properties: scope, ViewportId: view))
            .As()
            .ToFin();
    }

    public static Fin<PresetOperation> RenameLayerState(string current, string next) {
        return (Named(name: current).ToValidation(), Named(name: next).ToValidation())
            .Apply(static (from, to) => (PresetOperation)new RenameLayerStateCase(Current: from, Next: to))
            .As()
            .ToFin();
    }

    public static Fin<PresetOperation> DeleteLayerState(string name) =>
        Named(name: name).Map<PresetOperation>(static admitted => new DeleteLayerStateCase(Name: admitted));

    public static Fin<PresetOperation> ImportLayerStates(string path) {
        return from admitted in DocumentPath.Of(value: path)
               from _archive in guard(
                   string.Equals(Path.GetExtension(admitted.Value), ".3dm", StringComparison.OrdinalIgnoreCase),
                   (Error)new KernelFault.InvalidValue(nameof(ImportLayerStates), string.Join(" | ", new object?[] { op, "an absolute .3dm archive path" })))
               select (PresetOperation)new ImportLayerStatesCase(Path: admitted);
    }

    internal PresetExecution Execution => Switch<PresetExecution>(
        putCPlaneCase:          static _ => PresetExecution.Mutate,
        deleteCPlaneCase:       static _ => PresetExecution.Mutate,
        savePositionCase:       static _ => PresetExecution.Mutate,
        applyPositionCase:      static value => value.Verb.Execution,
        appendPositionCase:     static _ => PresetExecution.Mutate,
        renamePositionCase:     static _ => PresetExecution.Mutate,
        deletePositionCase:     static _ => PresetExecution.Mutate,
        saveLayerStateCase:     static _ => PresetExecution.Mutate,
        restoreLayerStateCase:  static _ => PresetExecution.Restore,
        renameLayerStateCase:   static _ => PresetExecution.Mutate,
        deleteLayerStateCase:   static _ => PresetExecution.Mutate,
        importLayerStatesCase:  static _ => PresetExecution.Mutate);

    private static Fin<PresetName> Named(string name) =>
        FactoryBridge.Accept<PresetName>(candidate: name);

    private static Fin<Seq<ResourceId>> Participants(ReadOnlySpan<Guid> ids) =>
        from admitted in toSeq(ids.ToArray())
            .Traverse(id => ResourceId.Admit(value: id).ToValidation())
            .As()
            .ToFin()
        from _distinct in guard(
            !admitted.IsEmpty && admitted.Distinct().Count == admitted.Count,
            (Error)new KernelFault.InvalidValue("ObjectIds", string.Join(" | ", new object?[] { key, "a non-empty roster of distinct object ids" })))
        select admitted;

    private static Fin<Option<Guid>> Viewport(Option<Guid> viewport) =>
        viewport.Traverse(id => ResourceId.Admit(value: id)).As().Map(static held => held.Map(static row => row.Value));
}
```

## [05]-[INTERPRETER]

- Owner: `Presets` — `Read` answers a detached census or one stored transform, and `Commit` admits an ordered mutation program, resolves its strongest posture, and frames it in `DocumentCommit.Sealed`.
- Entry: `Presets.Read(DocumentSession, PresetQuery)`; `Presets.Commit(DocumentSession, params ReadOnlySpan<PresetOperation>)`. The key precedes the span because an optional parameter after `params` forecloses the positional spread.
- Auto: the program's session needs and redraw policy DERIVE from `PresetExecution.Strongest` over the operations' own posture rows — a program mixing a layer-state restore with a rename is framed once, under the restore's continuous redraw, rather than opening two brackets or silently using the first operation's posture.
- Law: the commit is one envelope over the whole program, so a caller applies construction-plane, position, and layer-state changes in one undo step.
- Law: every host mutation settles on EVIDENCE, never on the returned `bool` alone: `Add` and `Save` refuse a negative index or an empty guid, the `bool` results go through `Admit.Confirm`, and the `ref`-parameter transform read rides `HostEdge.Probe` so the host's seed-and-check idiom appears once. A member that answers `-1` or `Guid.Empty` on rejection is exactly a member whose success value is indistinguishable from its failure value, which is why the admission is typed at the boundary.
- Law: name resolution is TOTAL for both address forms — an id resolves by membership in `Ids` and a name resolves through `Id(name)` and then by the same membership test — so an absent preset is a typed `AbsentEntry` naming the table and the entry, and the host's empty-guid miss never reaches a mutation.
- Boundary: Rhino's table mutation, its `ref`-parameter transform read, and its undo and redraw calls form the platform-forced statement block. `Presets` composes `DocumentSession` and `DocumentCommit` directly and holds no host handle beyond the demand window.
- Packages: RhinoCommon (`libs/dotnet/Rasm.Rhino/.api/api-rhinocommon-document-state.md` — `RhinoDoc.NamedConstructionPlanes`/`NamedPositions`/`NamedLayerStates`, `NamedPositionTable.ObjectXform(Guid, Guid, ref Transform)`, `NamedConstructionPlaneTable.Add(ConstructionPlane)` answering `-1` on rejection); `Document/session` (`DocumentSession.Demand`, `SessionNeed`); `Document/commit` (`DocumentCommit.Sealed`, `RedrawPolicy`); kernel `Domain/results` (`Try.lift`, `Admit.Confirm`, `HostEdge.Probe`); LanguageExt.Core (`Fin`, `Validation` applicative, `TraverseM`).

```csharp
// --- [IMPORTS] -------------------------------------------------------------------------
using Rasm.Domain;
using Rasm.Rhino.Document;
using Rhino;
using Rhino.DocObjects;
using Rhino.DocObjects.Tables;
using Rhino.Geometry;

namespace Rasm.Rhino.Persistence;

// --- [MODELS] --------------------------------------------------------------------------
[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record PresetAnswer : IDetachedDocumentResult {
    private PresetAnswer() { }

    public sealed record CensusCase(PresetSnapshot Snapshot) : PresetAnswer;
    public sealed record TransformCase(PositionObject Object) : PresetAnswer;
}

// --- [OPERATIONS] ----------------------------------------------------------------------
public static class Presets {
    public static Fin<PresetAnswer> Read(DocumentSession session, PresetQuery query) {
        return from owner in Admit.Need(value: session)
               from request in Admit.Need(value: query)
               from answer in Admit.Demand(
                   use: document => request.Switch<(RhinoDoc Document), Fin<PresetAnswer>>(
                       state: (document),
                       censusCase: static (state, _) => Census(document: state.Document)
                           .Map<PresetAnswer>(static value => new PresetAnswer.CensusCase(Snapshot: value)),
                       transformCase: static (state, read) =>
                           from id in Resolve(table: state.Document.NamedPositions, position: read.Position)
                           from transform in Stored(table: state.Document.NamedPositions, id: id, objectId: read.ObjectId)
                           select (PresetAnswer)new PresetAnswer.TransformCase(
                               Object: new PositionObject(ObjectId: read.ObjectId, Transform: transform))),
                   needs: [SessionNeed.Read])
               select answer;
    }

    public static Fin<Unit> Commit(DocumentSession session, params ReadOnlySpan<PresetOperation> operations) {
        return from owner in Admit.Need(value: session)
               from program in toSeq(operations.ToArray())
                   .Traverse(value => Admit.Need(value: value).ToValidation())
                   .As()
                   .ToFin()
               from _nonempty in guard(!program.IsEmpty,
                   (Error)new KernelFault.InvalidValue(nameof(operations), string.Join(" | ", new object?[] { op, "at least one persistence operation" })))
               let posture = PresetExecution.Strongest(postures: program.Map(static value => value.Execution))
               from completed in Admit.Demand(
                   use: document => DocumentCommit.Sealed(
                       document: document,
                       name: nameof(Commit),
                       recordsUndo: true,
                       redraw: posture.Redraw,
                       run: () => Run(document: document, program: program),
                       project: Fin.Succ),
                   needs: posture.Needs().ToArray())
               select completed;
    }

    private static Fin<Unit> Run(RhinoDoc document, Seq<PresetOperation> program) =>
        program
            .TraverseM(operation => Apply(document: document, operation: operation))
            .As()
            .Map(static _ => unit);

    private static Fin<Unit> Apply(RhinoDoc document, PresetOperation operation) =>
        operation.Switch<(RhinoDoc Document), Fin<Unit>>(
            state: (document, key),
            putCPlaneCase: static (state, put) =>
                from native in put.Model.Native()
                from index in Try.lift(() => Fin.Succ(value: state.Document.NamedConstructionPlanes.Add(native))).Run().Bind(static inner => inner)
                from _added in Landed(accepted: index >= 0, member: "NamedConstructionPlaneTable.Add", detail: put.Model.Name.Value)
                select unit,
            deleteCPlaneCase: static (state, delete) => Confirmed(
                    mutate: () => state.Document.NamedConstructionPlanes.Delete(delete.Name.Value),
                    member: "NamedConstructionPlaneTable.Delete",
                    detail: delete.Name.Value),
            savePositionCase: static (state, save) =>
                from raw in Try.lift(() => Fin.Succ(value: state.Document.NamedPositions.Save(
                    save.Name.Value,
                    save.ObjectIds.Map(static id => id.Value)))).Run().Bind(static inner => inner)
                from _id in ResourceId.Admit(value: raw)
                select unit,
            applyPositionCase: static (state, apply) =>
                from id in Resolve(table: state.Document.NamedPositions, position: apply.Position)
                from name in Named(table: state.Document.NamedPositions, id: id)
                from _applied in Confirmed(
                    mutate: () => apply.Verb.Apply(table: state.Document.NamedPositions, id: id.Value),
                    member: $"NamedPositionTable.{apply.Verb.Key}",
                    detail: name.Value)
                select unit,
            appendPositionCase: static (state, append) =>
                from id in Resolve(table: state.Document.NamedPositions, position: append.Position)
                from name in Named(table: state.Document.NamedPositions, id: id)
                from _appended in Confirmed(
                    mutate: () => state.Document.NamedPositions.Append(id.Value, append.ObjectIds.Map(static value => value.Value)),
                    member: "NamedPositionTable.Append",
                    detail: name.Value)
                select unit,
            renamePositionCase: static (state, rename) =>
                from id in Resolve(table: state.Document.NamedPositions, position: rename.Position)
                from _renamed in Confirmed(
                    mutate: () => state.Document.NamedPositions.Rename(id.Value, rename.Name.Value),
                    member: "NamedPositionTable.Rename",
                    detail: rename.Name.Value)
                select unit,
            deletePositionCase: static (state, delete) =>
                from id in Resolve(table: state.Document.NamedPositions, position: delete.Position)
                from name in Named(table: state.Document.NamedPositions, id: id)
                from _deleted in Confirmed(
                    mutate: () => state.Document.NamedPositions.Delete(id.Value),
                    member: "NamedPositionTable.Delete",
                    detail: name.Value)
                select unit,
            saveLayerStateCase: static (state, save) =>
                from index in Try.lift(() => Fin.Succ(value: save.ViewportId.Match(
                    Some: viewport => state.Document.NamedLayerStates.Save(save.Name.Value, viewport),
                    None: () => state.Document.NamedLayerStates.Save(save.Name.Value)))).Run().Bind(static inner => inner)
                from _saved in Landed(accepted: index >= 0, member: "NamedLayerStateTable.Save", detail: save.Name.Value)
                select unit,
            restoreLayerStateCase: static (state, restore) => Confirmed(
                    mutate: () => restore.ViewportId.Match(
                        Some: viewport => state.Document.NamedLayerStates.Restore(restore.Name.Value, restore.Properties.ToNative(), viewport),
                        None: () => state.Document.NamedLayerStates.Restore(restore.Name.Value, restore.Properties.ToNative())),
                    member: "NamedLayerStateTable.Restore",
                    detail: restore.Name.Value),
            renameLayerStateCase: static (state, rename) => Confirmed(
                    mutate: () => state.Document.NamedLayerStates.Rename(rename.Current.Value, rename.Next.Value),
                    member: "NamedLayerStateTable.Rename",
                    detail: rename.Next.Value),
            deleteLayerStateCase: static (state, delete) => Confirmed(
                    mutate: () => state.Document.NamedLayerStates.Delete(delete.Name.Value),
                    member: "NamedLayerStateTable.Delete",
                    detail: delete.Name.Value),
            importLayerStatesCase: static (state, import) =>
                from count in Try.lift(() => Fin.Succ(value: state.Document.NamedLayerStates.Import(import.Path.Value))).Run().Bind(static inner => inner)
                from _imported in Landed(accepted: count >= 0, member: "NamedLayerStateTable.Import", detail: import.Path.Value)
                select unit);

    private static Fin<PresetSnapshot> Census(RhinoDoc document) =>
        from planes in Project(
            source: () => document.NamedConstructionPlanes,
            project: value => CPlaneModel.Read(source: value))
        from positions in Project(
            source: () => document.NamedPositions.Ids,
            project: id => Captured(table: document.NamedPositions, id: id))
        from states in Names(source: () => document.NamedLayerStates.Names)
        select new PresetSnapshot(
            ConstructionPlanes: planes,
            Positions: positions,
            LayerStates: new LayerStateSnapshot(Names: states));

    private static Fin<PositionSnapshot> Captured(NamedPositionTable table, Guid id) =>
        from address in ResourceId.Admit(value: id)
        from name in Named(table: table, id: address)
        from objects in Project(
            source: () => table.ObjectIds(id),
            project: objectId => ResourceId.Admit(value: objectId)
                .Bind(participant => Stored(table: table, id: address, objectId: participant)
                    .Map(transform => new PositionObject(ObjectId: participant, Transform: transform))))
        select new PositionSnapshot(Id: address, Name: name, Objects: objects);

    private static Fin<Transform> Stored(NamedPositionTable table, ResourceId id, ResourceId objectId) =>
        Try.lift(() => Admit.Probe(
            probe: () => {
                Transform stored = Transform.Unset;
                return (table.ObjectXform(id.Value, objectId.Value, ref stored) && stored.IsValid, stored);
            },
            label: nameof(NamedPositionTable.ObjectXform),
            key: $"{id.Value}/{objectId.Value}")).Run().Bind(static inner => inner);

    private static Fin<ResourceId> Resolve(NamedPositionTable table, PositionRef position) =>
        from candidate in position.Switch<(NamedPositionTable Table), Fin<Guid>>(
            state: (table, key),
            idCase: static (_, address) => Fin.Succ(address.Id.Value),
            nameCase: static (state, named) => Try.lift(() => Fin.Succ(value: state.Table.Id(named.Name.Value))).Run().Bind(static inner => inner))
        from present in Try.lift(() => Fin.Succ(value: table.Ids.Contains(candidate))).Run().Bind(static inner => inner)
        from _member in guard(present,
            (Error)new PersistenceFault.AbsentEntry(Table: nameof(NamedPositionTable), Entry: candidate.ToString()))
        from admitted in ResourceId.Admit(value: candidate)
        select admitted;

    private static Fin<PresetName> Named(NamedPositionTable table, ResourceId id) =>
        from raw in Try.lift(() => Fin.Succ(value: HostEdge.Text(value: table.Name(id.Value)))).Run().Bind(static inner => inner)
        from present in raw.ToFin(Fail: new PersistenceFault.AbsentEntry(Table: nameof(NamedPositionTable), Entry: id.Value.ToString()))
        from admitted in FactoryBridge.Accept<PresetName>(candidate: present)
        select admitted;

    private static Fin<Seq<PresetName>> Names(Func<IEnumerable<string>> source) =>
        Project(source: source, project: name => FactoryBridge.Accept<PresetName>(candidate: name))
            .Map(static values => toSeq(values.OrderBy(static value => value.Value, StringComparer.Ordinal)));

    private static Fin<Seq<TResult>> Project<TSource, TResult>(
        Func<IEnumerable<TSource>> source,
        Func<TSource, Fin<TResult>> project) =>
        Try.lift(() => toSeq(source())
            .Traverse(value => project(arg: value).ToValidation())
            .As()
            .ToFin()).Run().Bind(static inner => inner);

    private static Fin<Unit> Confirmed(Func<bool> mutate, string member, string detail) =>
        Try.lift(() => Fin.Succ(value: mutate())).Run().Bind(static inner => inner)
            .Bind(accepted => Landed(accepted: accepted, member: member, detail: detail));

    private static Fin<Unit> Landed(bool accepted, string member, string detail) => accepted
        ? Fin.Succ(value: unit)
        : Fin.Fail<Unit>(error: new PersistenceFault.HostRefused(Member: member, Detail: detail));
}
```
