# [BIM_ENERGY_RESULTS]

`EnergyResults.Admit` is the simulation-RESULTS admission: `Rasm.Compute/Analysis/energy#SIMULATION_RUN` emits its typed results off its `SqlFile` read, and this owner lands them on the seam `Rasm.Element/Graph/element#ELEMENT_GRAPH` `ElementGraph` as producer-authored `Pset_EnergyResults` bag nodes bound to the subject each magnitude addresses. Results become ordinary graph content, so annual end-use loads, peak demands, EUI, and unmet-hours tallies outlive the run directory.

This owner is the return leg of `Energy/exchange#ENERGY_EXCHANGE`: that page lowers a model and content-keys the artifact, Compute runs it, and the answer lands back on the graph the lowering came from — joined by the `EnergyArtifact.ArtifactKey` address alone, never a shared client or a second store.

Composition arrives settled. Every admitted magnitude is a seam `Rasm.Element/Properties/quantity#MEASURE_VALUE` `MeasureValue` minted under its QTO identity; every bag is a `Rasm.Element/Properties/property#PROPERTY_BAG` `PropertyBag` bound through the neutral `Rasm.Element/Relations/relation#EDGE_ALGEBRA` `Relationship.Assign(AssignKind.PropertyDefinition)` edge — the `Exchange/reconstruct#RECONSTRUCTION` `Pset_Reconstruction` producer-authored precedent exactly, down to the content-minted `NodeId`.

## [01]-[INDEX]

- [02]-[RESULTS_ADMISSION]: `ResultScope` the closed target vocabulary, the `ResultFuel` × `ResultEndUse` axes and the `ResultMeasure` physics roster they compose into a `ResultQuantity`, `EnergyResult` the admission record, and the `EnergyResults.Admit` fold landing one `Pset_EnergyResults` bag per scope target.

## [02]-[RESULTS_ADMISSION]

- Owner: `EnergyResults` the one admission entry; `EnergyResult` the admitted result row — the `EnergyArtifact.ArtifactKey` content-key address the run consumed, the `ResultScope` it addresses, its `ResultQuantity`, its typed `MeasureValue`, and the producing run's `Instant`; `ResultScope` the closed `[Union]` target vocabulary (`Building`/`Zone`/`Space`); `ResultQuantity` the two-axis magnitude point over `ResultFuel` × `ResultEndUse` beside the `ResultMeasure` roster carrying the seam `QuantityType`, the `Dimension`, and the `UnitProvenance` its mint stamps through; `ResultTargets` the three-index scope resolver folded once per admission. The result row's SHAPE stays Compute-owned — its `[ENERGY_RESULTS_WIRE]` counterpart mints the record off the `SqlFile` read — and `EnergyResult` is THIS owner's admission record over that column set, so a Compute-side column addition lands here as one axis row and nothing else.
- Cases: `ResultScope` arms are the three granularities a simulation answers at and no other — `Building` the whole-model total, `Zone(string ZoneName)` the thermal-zone row keyed by its authored name, `Space(string GlobalId)` the per-space row keyed by the IFC identity the model already carries — so the case IS the resolution modality and a `(string TargetKind, string Target)` pair is the deleted stringly form. `ResultFuel` and `ResultEndUse` are the two axes a published magnitude is a POINT on and `ResultMeasure` the physics owner (annual energy, peak demand, area-normalized intensity, duration tally), so a new fuel or a new end-use is one row on its own axis and never a product of rows.
- Entry: `EnergyResults.Admit(Seq<EnergyResult> results, ElementGraph graph, Op key)` → `Fin<GraphDelta>` folds a whole run onto the graph, and `EnergyResult.Of(artifactKey, scope, quantity, si, at, key)` → `Fin<EnergyResult>` is the row's own admission gate, minting the magnitude through `ResultQuantity.Admit` so no caller carries a raw `double` past this boundary. `Admit` returns the delta alone; the caller applies it through the seam `ElementGraph.Apply`, exactly as a projector's contribution merges. Scope resolution against a subject the graph does not hold lifts `BimFault.Refused` with `BimReason.DanglingReference` under the `Model/faults#FAULT_BAND` `BimReason.DanglingReference` and aborts the whole admission — a half-landed result set reports a building total against zone rows that never arrived.
- Auto: `ResultTargets.Of` folds the three scope indexes ONCE per admission — the rank-0 `SpatialClass.Project` context root, the `ExternalId`-keyed occurrence index the `Space` arm resolves against, and the `ZoneProjection.All` name-keyed grouping index — because a run publishes a row per `(scope, quantity)` pair and a per-row graph scan is O(results × nodes); the zone index takes the last row on a name collision, grouping names being an authoring-side vocabulary this owner reads rather than governs. Rows GROUP by `(target, artifact key)`, so one scope under one run carries ONE bag rather than a bag per quantity, and the group's instant is the run's. `ResultQuantity.Admit` mints through `MeasureValue.OfSi(QuantityType, Dimension, double, Option<UnitProvenance>, Op?)` — the QTO-identity mint carrying the row's own provenance, never the dimension-anonymous `OfSi(Dimension, double)` whose stamp answers `None` to every downstream `As(QuantityType)` read and fails the type-equality gate a `Sum` against a stored quantity needs; the seam's own finite gate and registry dimension check rail a malformed magnitude before it reaches the canonical bytes. Bag identity is the kernel content hash over the bag's own canonical bytes (the id EXCLUDED from `ToCanonicalBytes`, so the empty-probe id is overwritten), so re-admitting an identical run dedups to the same node instead of accreting a second bag beside the first.
- Output: `GraphDelta` is this owner's whole contribution, and once applied the results are ORDINARY graph content — the AppUi report, an IDS facet over a result threshold, and the `Semantics/properties#TEMPLATE_AUDIT` graph fold all read them through the same `PropertiesOf`/`Bake` reads every authored property answers, with no results-side accessor. Because the bag is an ordinary seam `PropertySet`, the standing `Projection/egress#IFC_EGRESS` `ReauthorProperties` re-emits it as an `IfcPropertySet` with ZERO new egress code, each typed `PropertyValue.Measure` raising its own `IfcValue` — results round-trip into IFC and survive re-export. `EnergyArtifact` and `SimulatedAt` provenance rows ride the bag itself, so a reader answers WHICH lowered model and WHICH run produced a magnitude without a side ledger.
- Packages: Rasm (the kernel `Op` operation key and the content-hash mint the `NodeId` seeds from), Rasm.Element (the seam `ElementGraph`/`GraphDelta`/`Node`/`NodeId`/`Relationship`/`PropertyBag`/`PropertyValue`/`PropertyName`/`MeasureValue`/`QuantityType`/`Dimension`/`UnitProvenance`/`TemporalValue`), LanguageExt.Core (`Fin`/`Seq`/`Option`/`Map`/`HashMap`), NodaTime (`Instant`), Thinktecture.Runtime.Extensions (`[Union]`/`[SmartEnum<string>]`).
- Growth: a new published magnitude is one row on whichever AXIS it widens — a fuel on `ResultFuel`, a service on `ResultEndUse`, a physical reading on `ResultMeasure` — and admission, bag key, Pset re-emission, and report read all resolve the composed point with zero further edits; a new granularity is one `ResultScope` case with one `ResultTargets` index column and one `Resolve` arm the generated total `Switch` breaks on at compile; a new bag-provenance column is one row on the authored bag and a new unit posture one `UnitProvenance` case on a measure row; a Compute-side result column measuring something new is one axis row, never a second admission entry. Never a per-quantity result type, never an `AdmitZoneResults`/`AdmitSpaceResults` operation family, and never a per-scope bag name.
- Boundary: `SqlFile` decode stays `Rasm.Compute`'s under the standing simulation ruling — this owner consumes the typed results, and touching SQLite, a run directory, or an EnergyPlus output file here is the named strata violation, exactly as `Rasm.Compute` project references in either direction are; `rasm.bim` mints NO simulation context (`SimulationParameter`, run periods, conditioning policy, weather), which is `Energy/exchange#ENERGY_EXCHANGE`'s frozen boundary and Compute's to author. Second results stores beside the graph — a `ResultTable`, a per-run keyed side index, a `BimZone.Results` column — are the deleted form: the graph IS the store, so a result is queryable, diffable, versionable, and re-exportable by every owner that already reads a property bag, and a parallel store answers none of that. Producer-authored derived evidence does NOT route through the `Semantics/properties#PROPERTY_TEMPLATES` template authority: that owner is the buildingSMART oracle over AUTHORED model properties and its `TemplateAudit` fold reads resolved templates, so routing a computed result through it demands a template no catalogue declares and an applicable-class scope no analysis has — `Pset_Reconstruction` and `Pset_SiteContext` are the landed precedent, and a `PropertyKey.Resolve` hop here is the deleted form. Bag stamps are `EvidenceGrade.Derived` and `InheritanceMode.OccurrenceWins` because a result belongs to the occurrence it was computed for and no type bag overrides it; a `EvidenceGrade.Import` stamp — the site-context ingress form — ranks computed evidence beneath an authored value it must supersede. Scope targets resolve through the settled views — the `Model/spatial#SPATIAL_STRUCTURE` rank-0 row and the `Model/zones#ZONE_GRAPH` overlay — and a private entity-name set or a second grouping vocabulary here is the deleted form; the zone target is the grouping node the overlay already publishes, so per-zone results ride it with no zones-side edit. Unresolvable targets lift `BimFault.Refused` with `BimReason.DanglingReference` BARE, and a `.ToError()` lowering hop OR a hand-built `Error.New(2600, …)` bypassing the typed case is the named defect; a silently dropped row is doubly the deleted form, because a report cannot distinguish a zone with no cooling load from a zone whose cooling row never landed.

```csharp
// --- [IMPORTS] -------------------------------------------------------------------------
using LanguageExt;
using NodaTime;
using Rasm.Bim.Model;
using Rasm.Domain;
using Rasm.Element.Graph;
using Rasm.Element.Properties;
using Rasm.Element.Relations;
using Thinktecture;
using static LanguageExt.Prelude;

namespace Rasm.Bim;

// --- [TYPES] ---------------------------------------------------------------------------
[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record ResultScope {
    private ResultScope() { }

    public sealed record Building : ResultScope;
    public sealed record Zone(string ZoneName) : ResultScope;
    public sealed record Space(string GlobalId) : ResultScope;

    public static readonly ResultScope Whole = new Building();
}

[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
[KeyMemberComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class ResultFuel {
    public static readonly ResultFuel Total = new("Total");
    public static readonly ResultFuel Electricity = new("Electricity");
    public static readonly ResultFuel NaturalGas = new("NaturalGas");
    public static readonly ResultFuel DistrictHeating = new("DistrictHeating");
    public static readonly ResultFuel DistrictCooling = new("DistrictCooling");
}

[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
[KeyMemberComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class ResultEndUse {
    public static readonly ResultEndUse Whole = new("Whole");
    public static readonly ResultEndUse Heating = new("Heating");
    public static readonly ResultEndUse Cooling = new("Cooling");
    public static readonly ResultEndUse Lighting = new("Lighting");
    public static readonly ResultEndUse Equipment = new("Equipment");
    public static readonly ResultEndUse Fans = new("Fans");
    public static readonly ResultEndUse Pumps = new("Pumps");
    public static readonly ResultEndUse WaterSystems = new("WaterSystems");
}

[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
[KeyMemberComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class ResultMeasure {
    static readonly Dimension EnergyDim = Dimension.Create(2, 1, -2, 0, 0, 0, 0);
    static readonly Dimension PowerDim = Dimension.Create(2, 1, -3, 0, 0, 0, 0);
    static readonly Dimension IntensityDim = Dimension.Create(0, 1, -2, 0, 0, 0, 0);
    static readonly QuantityType EnergyType = QuantityType.Create("Energy");
    static readonly QuantityType PowerType = QuantityType.Create("Power");
    static readonly QuantityType IntensityType = QuantityType.Create("EnergyUseIntensity");

    public static readonly ResultMeasure Annual = new("Annual", EnergyType, EnergyDim, UnitProvenance.Derive);
    public static readonly ResultMeasure Peak = new("Peak", PowerType, PowerDim, UnitProvenance.Derive);
    public static readonly ResultMeasure Intensity = new("Intensity", IntensityType, IntensityDim, UnitProvenance.Label("J/m2"));
    public static readonly ResultMeasure UnmetHours = new("UnmetHours", QuantityType.Duration, Dimension.DurationDim, UnitProvenance.Derive);
    public static readonly ResultMeasure ComfortHours = new("ComfortHours", QuantityType.Duration, Dimension.DurationDim, UnitProvenance.Derive);

    public QuantityType Type { get; }
    public Dimension Dimension { get; }
    public UnitProvenance Provenance { get; }

    private ResultMeasure(string key, QuantityType type, Dimension dimension, UnitProvenance provenance) : this(key) =>
        (Type, Dimension, Provenance) = (type, dimension, provenance);

    public Fin<MeasureValue> Admit(double si, Op key) => MeasureValue.OfSi(Type, Dimension, si, Some(Provenance), key);
}

public readonly record struct ResultQuantity(ResultMeasure Measure, ResultFuel Fuel, ResultEndUse Use) {
    public static ResultQuantity Of(ResultMeasure measure, ResultFuel fuel, ResultEndUse use) => new(measure, fuel, use);

    public string Key =>
        $"{Measure.Key}{(Fuel == ResultFuel.Total ? "" : Fuel.Key)}{(Use == ResultEndUse.Whole ? "" : Use.Key)}";

    public Fin<MeasureValue> Admit(double si, Op key) => Measure.Admit(si, key);
}

// --- [MODELS] --------------------------------------------------------------------------
public sealed record EnergyResult(
    ArtifactKey Artifact, ResultScope Scope, ResultQuantity Quantity, MeasureValue Value, Instant At) {

    public static Fin<EnergyResult> Of(ArtifactKey artifact, ResultScope scope, ResultQuantity quantity, double si, Instant at, Op key) =>
        quantity.Admit(si, key).Map(value => new EnergyResult(artifact, scope, quantity, value, at));
}

// --- [OPERATIONS] ----------------------------------------------------------------------
internal readonly record struct ResultTargets(
    Option<NodeId> Context, Map<string, NodeId> Spaces, Map<string, NodeId> Zones) {

    public static ResultTargets Of(ElementGraph graph) =>
        new(graph.ObjectNodes
                .Find(static o => StringComparer.OrdinalIgnoreCase.Equals(o.Classification.System, IfcClass.System) && o.Classification.Code == SpatialClass.Project.Key)
                .Map(static o => o.Id),
            graph.ObjectNodes
                .Filter(static o => StringComparer.OrdinalIgnoreCase.Equals(o.Classification.System, IfcClass.System) && o.Classification.Code == SpatialClass.Space.Key)
                .Fold(Map<string, NodeId>(), static (index, o) =>
                    o.ExternalId.Match(Some: id => index.AddOrUpdate(id, o.Id), None: () => index)),
            ZoneProjection.All(graph).Fold(Map<string, NodeId>(), static (index, zone) =>
                index.AddOrUpdate(zone.Name, zone.Id)));

    public Fin<NodeId> Resolve(ResultScope scope, Op key) => scope.Switch(
        building: _ => Context.ToFin(Miss(key, "context", "")),
        zone: z => Zones.Find(z.ZoneName).ToFin(Miss(key, "zone", z.ZoneName)),
        space: s => Spaces.Find(s.GlobalId).ToFin(Miss(key, "space", s.GlobalId)));

    static BimFault Miss(Op key, string modality, string subject) =>
        new BimFault.Refused(key, BimScope.Energy, BimReason.DanglingReference, string.Join(':', new object?[] { "energy-result-target-miss", modality, subject }));
}

public static class EnergyResults {
    public const string SetName = "Pset_EnergyResults";

    public static Fin<GraphDelta> Admit(Seq<EnergyResult> results, ElementGraph graph, Op key) {
        ResultTargets targets = ResultTargets.Of(graph);
        return results
            .Fold(Fin.Succ(HashMap<(NodeId Target, ArtifactKey Run), (Instant At, Map<string, EnergyResult> Rows)>()),
                (rail, result) => rail.Bind(grouped => targets.Resolve(result.Scope, key).Bind(target =>
                    grouped.Find((target, result.Artifact)) is { IsSome: true, Case: (Instant at, Map<string, EnergyResult> rows) }
                        ? rows.ContainsKey(result.Quantity.Key)
                            ? Fin.Fail<HashMap<(NodeId, ArtifactKey), (Instant, Map<string, EnergyResult>)>>(
                                new BimFault.Refused(key, BimScope.Energy, BimReason.Rejected, string.Join(':', new object?[] { "energy-result-duplicate", result.Artifact.Value, result.Quantity.Key })))
                            : Fin.Succ(grouped.AddOrUpdate((target, result.Artifact), (at, rows.Add(result.Quantity.Key, result))))
                        : Fin.Succ(grouped.AddOrUpdate((target, result.Artifact),
                            (result.At, Map((result.Quantity.Key, result))))))))
            .Map(grouped => toSeq(grouped).Fold(GraphDelta.Empty, (delta, group) => {
                Node.PropertySet bag = Author(
                    group.Key.Run, group.Value.At, toSeq(group.Value.Rows.Values), graph.Header.Tolerance);
                return delta.Put(bag)
                    .Link(new Relationship.Assign(group.Key.Target, bag.Id, AssignKind.PropertyDefinition));
            }));
    }

    static readonly PropertyName ArtifactRow = PropertyCategory.Seam.Row("EnergyArtifact");
    static readonly PropertyName SimulatedAt = PropertyCategory.Seam.Row("SimulatedAt");

    static Node.PropertySet Author(ArtifactKey run, Instant at, Seq<EnergyResult> rows, double tolerance) {
        PropertyBag bag = new(SetName,
            rows.Fold(
                Map<PropertyName, PropertyValue>(
                    (ArtifactRow, new PropertyValue.Text(run.Value)),
                    (SimulatedAt, new PropertyValue.Temporal(new TemporalValue.Stamp(at)))),
                static (values, row) => values.AddOrUpdate(
                    PropertyCategory.Seam.Row(row.Quantity.Key), new PropertyValue.Measure(row.Value))),
            InheritanceMode.OccurrenceWins, EvidenceGrade.Derived);
        Node.PropertySet probe = new(NodeId.Of(new NodeSeed.Placement()), bag);
        return probe with { Id = NodeId.Of(new NodeSeed.Content(probe, tolerance)) };
    }
}
```

## [03]-[RESEARCH]

(none)
