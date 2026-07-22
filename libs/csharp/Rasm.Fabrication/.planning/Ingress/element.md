# [RASM_FABRICATION_ELEMENT_INGRESS]

`ElementImport` admits baked geometry once, lowers fabrication evidence into one `ElementReceipt`, then projects without reopening `ElementGraph`. `ElementSource` admits one graph and an identity-distinct subject roster whose arity selects singular or batch outcome. `ElementPayload` admits distinct representation slots with at most one mesh carrier and derives one count-framed identity.

`ElementFact` is the numeric, symbolic, and typed-property row family. Independent duplicate-path conflicts accumulate with path-derived loci before `AdmittedComponent` mints, while tolerance-equal observations coalesce. `PropertyValue.Render()` and `CanonicalBytes(CanonicalWriter)` remain the sole value projections. `ElementReceipt.Topology` preserves canonical `Relationship` rows, and each realizing `Connect` lowers into `AdmittedComponent.Connections`; `At` stays absent because the interface is a blob content key.

## [01]-[INDEX]

| [INDEX] | [OWNER]              | [OWNS]                                                   |
| :-----: | :------------------- | :------------------------------------------------------- |
|  [01]   | `RepresentationSlot` | representation identifiers and their graph-key accessors |
|  [02]   | `ElementGeometry`    | the resolved carrier a slot admits                       |
|  [03]   | `ElementPayload`     | the distinct-slot part roster and its combined identity  |
|  [04]   | `ElementSource`      | one graph, a distinct-subject roster, and the derive key |
|  [05]   | `ElementFact`        | numeric, symbolic, and typed-property evidence rows      |
|  [06]   | `ElementReceipt`     | component, topology, facts, canonical bytes, and locus   |
|  [07]   | `ElementImport`      | admission, fact folding, connection lowering, and egress |
|  [08]   | `ElementEgress`      | the parameterized projection request                     |

## [02]-[ELEMENT_INGRESS]

- Owner: `ElementSource` owns graph-bearing ingress; `ElementSubject` owns element identity with resolved representation; `ElementReceipt` owns the admitted carrier, canonical relationship rows, typed facts, canonical property bytes, and fault locus; `ElementImport` owns admission and egress.
- Rows: `RepresentationSlot` closes `Body` · `Axis` · `Box` · `FootPrint`, each row carrying the `RepresentationContentHash` accessor its key names, so a new identifier is one row and no arm re-spells an identifier string.
- Cases: `ElementGeometry` closes mesh, profile, and axis carriers; `ElementFact.Property` preserves every `PropertyValue` case; `ElementAdmission` preserves singular and batch cardinality; `ElementEgress` selects `Component` · `Topology` · `Facts` · `CanonicalProperties`; `ElementProjection` returns the matching result or committed byte count.
- Entry: `ElementImport.Admit(ElementSource)` bakes each subject once and returns `Fin<ElementAdmission>`; `ElementImport.Project(ElementReceipt, ElementEgress)` returns `Fin<ElementProjection>` without graph access.
- Auto: generated `Switch` members keep every closed family total; `ElementSource` and `ElementPayload` admit non-empty distinct rosters through their generated factories; `Validation<Error, _>` accumulates independent batch and duplicate-path faults; `CanonicalWriter` owns value bytes and `ArrayPoolBufferWriter<byte>` owns the caller-scoped pooled egress buffer.
- Receipt: `ElementReceipt` carries `AdmittedComponent`, `Seq<Relationship>`, `ElementFactSet`, count-prefixed canonical property bytes, and the element content locus; `ElementAdmission` preserves one or many receipts.
- Boundary: `ElementGraph` never crosses the receipt; `Relationship`, `PropertyValue`, `MaterialComposition`, `MaterialPropertySet`, and `MaterialUsage` remain their canonical generated owners; `NodeId` and provider types lower to strings or content keys only at fact egress; no connection line is synthesized, and a `Connect` row without a realizing element stays topology-only because `ComponentConnection` demands a realizing key; faults from `Rasm.Element` pass through unchanged and local ingress or egress conflicts mint `IngressTranslation`; canonical-property ordering and caller-buffer commit are the serialization-boundary statement kernels.

```csharp signature
// --- [RUNTIME_PRELUDE] ------------------------------------------------------------------------------------------------------------------------------
using System.Globalization;
using System.Text;
using CommunityToolkit.HighPerformance.Buffers;
using LanguageExt;
using LanguageExt.Common;
using Rasm.Domain;
using Rasm.Element.Composition;
using Rasm.Element.Graph;
using Rasm.Element.Projection;
using Rasm.Element.Properties;
using Rasm.Element.Relations;
using Rasm.Fabrication.Process;
using Rasm.Meshing;
using Thinktecture;
using UnitsNet;
using static LanguageExt.Prelude;

namespace Rasm.Fabrication.Ingress;

// --- [TYPES] ----------------------------------------------------------------------------------------------------------------------------------------
// Key IS the RepresentationContentHash identifier, so the graph lookup and the canonical key fold both read the row
// rather than re-spelling the identifier string at either site.
[SmartEnum<string>]
public sealed partial class RepresentationSlot {
    public static readonly RepresentationSlot Body = new("Body", static row => row.Body, static value => value is ElementGeometry.Mesh);
    public static readonly RepresentationSlot Axis = new("Axis", static row => row.Axis, static value => value is ElementGeometry.Centreline);
    public static readonly RepresentationSlot Box = new("Box", static row => row.Box, static value => value is ElementGeometry.Mesh);
    public static readonly RepresentationSlot FootPrint = new("FootPrint", static row => row.FootPrint, static value => value is ElementGeometry.Profiles);

    public Func<RepresentationContentHash, Option<UInt128>> Locate { get; }
    public Func<ElementGeometry, bool> Admits { get; }
}

// --- [MODELS] ---------------------------------------------------------------------------------------------------------------------------------------
[Union]
public abstract partial record ElementGeometry {
    public sealed partial record Mesh(MeshSpace Value) : ElementGeometry;
    public sealed partial record Profiles(Arr<Loop> Value) : ElementGeometry;
    public sealed partial record Centreline(Edge3 Value) : ElementGeometry;
}

public sealed record ElementPart(RepresentationSlot Slot, ElementGeometry Value);

[ComplexValueObject]
public sealed partial class ElementPayload {
    public Seq<ElementPart> Parts { get; }

    [IgnoreMember]
    public Option<MeshSpace> Mesh => Parts
        .Choose(static part => part.Value is ElementGeometry.Mesh mesh ? Some(mesh.Value) : None)
        .Head;

    [IgnoreMember]
    public Arr<Loop> Profiles => Parts
        .Choose(static part => part.Value is ElementGeometry.Profiles profiles ? Some(profiles.Value) : None)
        .Head
        .IfNone(Arr<Loop>());

    static partial void ValidateFactoryArguments(ref ValidationError? validationError, ref Seq<ElementPart> parts) {
        parts = parts.OrderBy(static part => part.Slot.Key, StringComparer.Ordinal).ToSeq();
        validationError = parts.IsEmpty
            ? new ValidationError("element payload carries no representation part")
            : parts.GroupBy(static part => part.Slot.Key, StringComparer.Ordinal).Count() != parts.Count
                ? new ValidationError("element payload repeats a representation slot")
                : parts.Exists(static part => !part.Slot.Admits(part.Value))
                    ? new ValidationError("element payload geometry does not match its representation slot")
                : parts.Count(static part => part.Value is ElementGeometry.Mesh) > 1
                    ? new ValidationError("element payload carries multiple mesh representations")
                : parts.Exists(static part => part.Value is ElementGeometry.Mesh or ElementGeometry.Profiles)
                    ? null
                    : new ValidationError("element payload carries no fabricable carrier");
    }
}

public sealed record ElementSubject(NodeId Id, ElementPayload Payload);

[ComplexValueObject]
public sealed partial class ElementSource {
    public ElementGraph Graph { get; }
    public Seq<ElementSubject> Subjects { get; }
    public Op Key { get; }

    static partial void ValidateFactoryArguments(
        ref ValidationError? validationError,
        ref ElementGraph graph,
        ref Seq<ElementSubject> subjects,
        ref Op key) =>
        validationError = subjects.IsEmpty
            ? new ValidationError("element source carries no subject")
            : subjects.GroupBy(static subject => subject.Id.Value, StringComparer.Ordinal).Count() != subjects.Count
                ? new ValidationError("element source repeats a subject identity")
                : null;
}

[Union]
public abstract partial record ElementFact {
    public sealed partial record Numeric(string Path, double Value) : ElementFact;
    public sealed partial record Symbolic(string Path, string Value) : ElementFact;
    public sealed partial record Property(string Path, PropertyValue Value) : ElementFact;

    public string Locus => Switch(
        numeric: static value => value.Path,
        symbolic: static value => value.Path,
        property: static value => value.Path);

    // Two readings of one path arrive through independent unit conversions, so numeric agreement is a tolerance test;
    // exact bit equality would fault a graph whose quantities merely round differently.
    public bool Equivalent(ElementFact other, double tolerance) => (this, other) switch {
        (Numeric left, Numeric right) => Math.Abs(left.Value - right.Value) <= tolerance,
        (Symbolic left, Symbolic right) => StringComparer.Ordinal.Equals(left.Value, right.Value),
        (Property left, Property right) => left.Value.Equals(right.Value),
        _ => false,
    };
}

[ComplexValueObject]
public sealed partial class ElementFactSet {
    public Seq<ElementFact> Rows { get; }

    [IgnoreMember]
    public Map<string, double> Quantities => Rows
        .Choose(static row => row is ElementFact.Numeric numeric ? Some((numeric.Path, numeric.Value)) : None)
        .Fold(Map<string, double>(), static (map, row) => map.Add(row.Path, row.Value));

    [IgnoreMember]
    public Map<string, string> Properties => Rows
        .Choose(static row => row switch {
            ElementFact.Symbolic symbolic => Some((Path: symbolic.Path, Value: symbolic.Value)),
            ElementFact.Property property => Some((Path: property.Path, Value: property.Value.Render())),
            _ => None,
        })
        .Fold(Map<string, string>(), static (map, row) => map.Add(row.Path, row.Value));
}

[ComplexValueObject]
public sealed partial class ElementReceipt {
    public AdmittedComponent Component { get; }
    public Seq<Relationship> Topology { get; }
    public ElementFactSet Facts { get; }
    public ReadOnlyMemory<byte> CanonicalProperties { get; }
    public UInt128 Locus { get; }
}

[Union]
public abstract partial record ElementAdmission {
    public sealed partial record One(ElementReceipt Receipt) : ElementAdmission;
    public sealed partial record Many(Seq<ElementReceipt> Receipts) : ElementAdmission;
}

[Union]
public abstract partial record ElementEgress {
    public sealed partial record Component : ElementEgress;
    public sealed partial record Topology : ElementEgress;
    public sealed partial record Facts : ElementEgress;
    public sealed partial record CanonicalProperties(ArrayPoolBufferWriter<byte> Destination) : ElementEgress;
}

[Union]
public abstract partial record ElementProjection {
    public sealed partial record Component(AdmittedComponent Value) : ElementProjection;
    public sealed partial record Topology(Seq<Relationship> Value) : ElementProjection;
    public sealed partial record Facts(ElementFactSet Value) : ElementProjection;
    public sealed partial record Written(int Count) : ElementProjection;
}

// --- [OPERATIONS] -----------------------------------------------------------------------------------------------------------------------------------
public static class ElementImport {
    const string MaterialRow = "material";

    // Arity alone selects the outcome case; the admitted source proves the roster is non-empty and identity-distinct,
    // so a singular request can never arrive as a vacuous batch.
    public static Fin<ElementAdmission> Admit(ElementSource source) =>
        source.Subjects
            .Map(subject => AdmitOne(source.Graph, subject, source.Key).ToValidation())
            .Traverse(static receipt => receipt)
            .As()
            .ToFin()
            .Map(static receipts => receipts.Head
                .Filter(_ => receipts.Count == 1)
                .Match(
                    Some: static receipt => (ElementAdmission)new ElementAdmission.One(receipt),
                    None: () => new ElementAdmission.Many(receipts)));

    public static Fin<ElementProjection> Project(ElementReceipt receipt, ElementEgress egress) =>
        egress.Switch(
            component: static (_, state) => Fin.Succ<ElementProjection>(new ElementProjection.Component(state.Component)),
            topology: static (_, state) => Fin.Succ<ElementProjection>(new ElementProjection.Topology(state.Topology)),
            facts: static (_, state) => Fin.Succ<ElementProjection>(new ElementProjection.Facts(state.Facts)),
            canonicalProperties: static (request, state) => Try.lift<ElementProjection>(() => {
                int before = request.Destination.WrittenCount;
                request.Destination.Write(state.CanonicalProperties.Span);
                return new ElementProjection.Written(request.Destination.WrittenCount - before);
            }).Run().MapFail(_ => Translation(state.Locus)),
            state: receipt);

    static Fin<ElementReceipt> AdmitOne(ElementGraph graph, ElementSubject subject, Op key) =>
        from baked in graph.Bake(subject.Id, key)
        let topology = toSeq(graph.EdgesAt(baked.Id))
        let tolerance = graph.Header.Tolerance
        let locus = LocusOf(baked.Id, path: string.Empty, tolerance)
        let fault = Translation(locus)
        from representation in Resolve(baked, subject.Payload, tolerance, fault)
        let connections = ConnectionsOf(topology)
        from facts in FactsOf(baked, topology, connections, tolerance, fault)
        from component in AdmittedComponent.Admit(
            representation,
            subject.Payload.Mesh,
            subject.Payload.Profiles,
            SheetOf(baked),
            LayersOf(baked),
            connections,
            facts.Quantities,
            facts.Properties).MapFail(_ => fault)
        from receipt in Try.lift(() => ElementReceipt.Create(
                component,
                topology,
                facts,
                CanonicalProperties(graph, baked),
                locus))
            .Run()
            .MapFail(_ => fault)
        select receipt;

    // Combined representation key count-frames the roster and length-frames each slot key, so a one-part and a
    // two-part payload can never collide and a slot rename cannot silently reuse an existing identity.
    static Fin<UInt128> Resolve(Element baked, ElementPayload payload, double tolerance, Error fault) =>
        payload.Parts
            .Map(part => part.Slot.Locate(baked.Representations)
                .Map(key => (Slot: part.Slot.Key, Key: key))
                .ToFin(fault)
                .ToValidation())
            .Traverse(static row => row)
            .As()
            .ToFin()
            .Map(rows => rows
                .Fold(new CanonicalWriter(tolerance).Ordinal(rows.Count),
                    static (writer, row) => writer.String(row.Slot).U128(row.Key)))
            .Map(static writer => ContentHash.Of(writer.ToBytes().Span));

    // Realizing element makes a connection fabricable; bare adjacency remains receipt topology.
    // ComponentConnection carries the interface blob key and leaves At absent.
    static Arr<ComponentConnection> ConnectionsOf(Seq<Relationship> topology) =>
        topology.Choose(static relation => relation is Relationship.Connect connect
            ? connect.Realizing.Bind(realizing => connect.Interface.Map(key => new ComponentConnection(
                key.ToString(CultureInfo.InvariantCulture), realizing.Value, Option<Edge3>.None)))
            : None).ToArr();

    static Arr<ComponentLayer> LayersOf(Element baked) =>
        baked.Materials.Bind(static material => material.Material.Composition.Switch(
            single: static _ => Seq<ComponentLayer>(),
            layerSet: static set => set.Layers.Map(static layer => new ComponentLayer(
                layer.LayerName,
                Length.FromMeters(layer.Thickness.Si).Millimeters,
                layer.Material.Value)),
            profileSet: static _ => Seq<ComponentLayer>(),
            constituentSet: static _ => Seq<ComponentLayer>())).ToArr();

    static Option<double> SheetOf(Element baked) {
        Seq<double> stacks = baked.Materials
            .Choose(static material => material.Material.Composition is MaterialComposition.LayerSet set
                ? Some(Length.FromMeters(set.TotalThickness).Millimeters)
                : None);
        return stacks.Count == 1 ? Some(stacks.Head) : None;
    }

    static Fin<ElementFactSet> FactsOf(
        Element baked,
        Seq<Relationship> topology,
        Arr<ComponentConnection> connections,
        double tolerance,
        Error fault) {
        Seq<ElementFact> rows = IdentityRows(baked)
            + CompositionRows(baked)
            + MaterialRows(baked)
            + UsageRows(baked)
            + SectionRows(baked)
            + QuantityRows(baked)
            + PropertyRows(baked)
            + TopologyRows(topology)
            + Seq<ElementFact>(
                new ElementFact.Numeric("Component.Parts", baked.Parts.Count),
                new ElementFact.Numeric("Component.Materials", baked.Materials.Count),
                new ElementFact.Numeric("Component.Properties", baked.Properties.Count),
                new ElementFact.Numeric("Component.Quantities", baked.Quantities.Count),
                new ElementFact.Numeric("Component.Assessments", baked.Assessments.Count),
                new ElementFact.Numeric("Component.Coverages", baked.Coverages.Count),
                new ElementFact.Numeric("Component.Relations", topology.Count),
                new ElementFact.Numeric("Component.Connections", connections.Count),
                new ElementFact.Numeric("Component.Openings", topology.Count(static relation => relation is Relationship.Void)),
                new ElementFact.Numeric("Component.HasAppearance", baked.Appearance.IsSome ? 1.0 : 0.0),
                new ElementFact.Numeric("Component.HasHistory", baked.History.IsSome ? 1.0 : 0.0));

        // One grouping serves both the conflict census and the coalesced store, and each conflict carries its own
        // path-derived locus so an accumulated batch names every offending path instead of repeating one error.
        Seq<(string Path, Seq<ElementFact> Rows)> grouped = toSeq(rows.GroupBy(static row => row.Locus, StringComparer.Ordinal))
            .Map(static group => (Path: group.Key, Rows: toSeq(group)));
        Seq<Validation<Error, Unit>> conflicts = grouped
            .Choose(group => group.Rows.ForAll(row => group.Rows.ForAll(other => row.Equivalent(other, tolerance)))
                ? None
                : Some(Fin.Fail<Unit>(Translation(LocusOf(baked.Id, group.Path, tolerance))).ToValidation()))
            + baked.Properties.Bind(static bag => bag.Values.Pairs)
                .Choose(pair => pair.Key.Value == MaterialRow && pair.Value is not PropertyValue.Text
                    ? Some(Fin.Fail<Unit>(Translation(LocusOf(baked.Id, MaterialRow, tolerance))).ToValidation())
                    : None);

        return conflicts.Traverse(static conflict => conflict)
            .As()
            .ToFin()
            .Bind(_ => Try.lift(() => ElementFactSet.Create(grouped.Choose(static group => group.Rows.Head)))
                .Run()
                .MapFail(_ => fault));
    }

    static UInt128 LocusOf(NodeId id, string path, double tolerance) =>
        ContentHash.Of(new CanonicalWriter(tolerance).String(id.Value).String(path).ToBytes().Span);

    static Seq<ElementFact> IdentityRows(Element baked) => Seq<ElementFact>(
        new ElementFact.Symbolic("Element.Id", baked.Id.Value),
        new ElementFact.Symbolic("Element.Kind", baked.Kind.Key),
        new ElementFact.Symbolic("Element.PredefinedType", baked.PredefinedType.Key),
        new ElementFact.Symbolic("Element.Name", baked.Name),
        new ElementFact.Symbolic("Element.Tag", baked.Tag),
        new ElementFact.Symbolic("Element.Classification.System", baked.Classification.System),
        new ElementFact.Symbolic("Element.Classification.Code", baked.Classification.Code),
        new ElementFact.Symbolic("Element.Classification.Edition", baked.Classification.Edition))
        + baked.ExternalId.Map(value => Seq<ElementFact>(new ElementFact.Symbolic("Element.ExternalId", value))).IfNone(Seq<ElementFact>())
        + baked.TypeId.Map(value => Seq<ElementFact>(new ElementFact.Symbolic("Element.TypeId", value.Value))).IfNone(Seq<ElementFact>())
        + baked.Classifications.Map((classification, index) => Seq<ElementFact>(
            new ElementFact.Symbolic($"Element.Classification.{index}.System", classification.System),
            new ElementFact.Symbolic($"Element.Classification.{index}.Code", classification.Code),
            new ElementFact.Symbolic($"Element.Classification.{index}.Edition", classification.Edition))).Bind(identity)
        + baked.Representations.ByIdentifier.Pairs.Map(pair =>
            (ElementFact)new ElementFact.Symbolic($"Element.Representation.{pair.Key}", pair.Value.ToString()));

    static Seq<ElementFact> CompositionRows(Element baked) =>
        baked.Materials.Bind(material => {
            string root = $"Material.{material.Material.MaterialKey.Value}.Composition";
            return material.Material.Composition.Switch(
                single: single => Seq<ElementFact>(
                    new ElementFact.Symbolic($"{root}.Kind", nameof(MaterialComposition.Single)),
                    new ElementFact.Symbolic($"{root}.Material", single.Material.Value)),
                layerSet: set => Seq<ElementFact>(new ElementFact.Symbolic($"{root}.Kind", nameof(MaterialComposition.LayerSet)))
                    + set.Layers.Map((layer, index) => Seq<ElementFact>(
                        new ElementFact.Symbolic($"{root}.Layer.{index}.Material", layer.Material.Value),
                        new ElementFact.Symbolic($"{root}.Layer.{index}.Name", layer.LayerName),
                        new ElementFact.Numeric($"{root}.Layer.{index}.Thickness", layer.Thickness.Si))).Bind(identity),
                profileSet: profile => Seq<ElementFact>(
                    new ElementFact.Symbolic($"{root}.Kind", nameof(MaterialComposition.ProfileSet)),
                    new ElementFact.Symbolic($"{root}.Material", profile.Material.Value),
                    new ElementFact.Symbolic($"{root}.Profile.Standard", profile.Profile.Standard),
                    new ElementFact.Symbolic($"{root}.Profile.Designation", profile.Profile.Designation),
                    new ElementFact.Symbolic($"{root}.Profile.ContentKey", profile.Profile.ContentKey.ToString())),
                constituentSet: set => Seq<ElementFact>(new ElementFact.Symbolic($"{root}.Kind", nameof(MaterialComposition.ConstituentSet)))
                    + set.Constituents.Map((constituent, index) => Seq<ElementFact>(
                        new ElementFact.Symbolic($"{root}.Constituent.{index}.Material", constituent.Material.Value),
                        new ElementFact.Symbolic($"{root}.Constituent.{index}.Category", constituent.Category),
                        new ElementFact.Numeric($"{root}.Constituent.{index}.Fraction", constituent.Fraction))).Bind(identity));
        });

    static Seq<ElementFact> MaterialRows(Element baked) =>
        baked.Materials.Bind(material => material.Material.Properties.Bind(property =>
            PropertySetRows(material.Material.MaterialKey.Value, property)));

    static Seq<ElementFact> PropertySetRows(string material, MaterialPropertySet property) {
        string family = property.Map(
            mechanical: nameof(MaterialPropertySet.Mechanical),
            orthotropic: nameof(MaterialPropertySet.Orthotropic),
            thermal: nameof(MaterialPropertySet.Thermal),
            acoustic: nameof(MaterialPropertySet.Acoustic),
            fire: nameof(MaterialPropertySet.Fire),
            environmental: nameof(MaterialPropertySet.Environmental),
            cost: nameof(MaterialPropertySet.Cost),
            damping: nameof(MaterialPropertySet.Damping),
            hygrothermal: nameof(MaterialPropertySet.Hygrothermal),
            durability: nameof(MaterialPropertySet.Durability),
            optical: nameof(MaterialPropertySet.Optical));
        string root = $"Material.{material}.{family}";
        Seq<ElementFact> evidence = Seq<ElementFact>(
            new ElementFact.Symbolic($"{root}.Evidence.Source", property.Evidence.Source),
            new ElementFact.Symbolic($"{root}.Evidence.Reference", property.Evidence.Reference))
            + property.Evidence.ValidUntil.Map(date => Seq<ElementFact>(
                new ElementFact.Symbolic($"{root}.Evidence.ValidUntil", date.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture)))).IfNone(Seq<ElementFact>());

        return evidence + property.Switch(
            mechanical: value => Numbers(root, ("Density", value.Density.Si), ("YoungsModulus", value.YoungsModulus.Si),
                ("ShearModulus", value.ShearModulus.Si), ("YieldStrength", value.YieldStrength.Si),
                ("UltimateStrength", value.UltimateStrength.Si), ("PoissonsRatio", value.PoissonsRatio),
                ("ThermalExpansionPerK", value.ThermalExpansionPerK)),
            orthotropic: value => Numbers(root, ("Density", value.Density.Si), ("E1Parallel", value.E1Parallel.Si),
                ("E2Perpendicular", value.E2Perpendicular.Si), ("ShearModulus", value.ShearModulus.Si),
                ("Strength1Parallel", value.Strength1Parallel.Si), ("Strength2Perpendicular", value.Strength2Perpendicular.Si),
                ("ThermalExpansionPerK", value.ThermalExpansionPerK)),
            thermal: value => Numbers(root, ("Conductivity", value.Conductivity.Si), ("SpecificHeat", value.SpecificHeat.Si),
                ("UValue", value.UValue.Si), ("VapourResistanceFactor", value.VapourResistanceFactor)),
            acoustic: value => Numbers(root, ("Nrc", value.Nrc), ("Saa", value.Saa), ("StcWeighted", value.StcWeighted), ("Rw", value.Rw))
                + toSeq(AcousticBand.Items).Bind(band => Numbers(root,
                    ($"Absorption.{band.CenterHz}Hz", value.At(band)),
                    ($"SoundReductionIndexDb.{band.CenterHz}Hz", value.SriAt(band))))
                + OptionalNumber(root, "DynamicStiffnessMNPerM3", value.DynamicStiffnessMNPerM3)
                + OptionalNumber(root, "FlowResistivityPaSPerM2", value.FlowResistivityPaSPerM2)
                + OptionalNumber(root, "LossFactor", value.LossFactor),
            fire: value => Numbers(root, ("LoadBearingMinutes", value.Resistance.LoadBearingMinutes),
                ("IntegrityMinutes", value.Resistance.IntegrityMinutes), ("InsulationMinutes", value.Resistance.InsulationMinutes))
                + Symbols(root, ("Reaction", value.Reaction.Key), ("Smoke", value.Smoke.Key), ("Droplets", value.Droplets.Key)),
            environmental: value => Numbers(root, ("RecycledContent", value.RecycledContent),
                ("EndOfLifeRecovery", value.EndOfLifeRecovery), ("WholeLifeGwp", value.WholeLifeGwp))
                + Symbols(root, ("Basis", value.Basis.Key))
                + toSeq(ImpactCategory.Items).Bind(category => toSeq(LifecycleStage.Items).Map(stage =>
                    (ElementFact)new ElementFact.Numeric($"{root}.Impact.{category.Name}.{stage.Module}", value.IndicatorAt(category, stage)))),
            cost: value => Numbers(root, ("SupplyPerUnit", value.SupplyPerUnit), ("InstallPerUnit", value.InstallPerUnit),
                ("LifecyclePerUnit", value.LifecyclePerUnit)) + Symbols(root, ("Basis", value.Basis.Key), ("Currency", value.Currency.Value)),
            damping: value => Numbers(root, ("DampingRatio", value.DampingRatio), ("StructuralLossFactor", value.StructuralLossFactor))
                + value.Rayleigh.Map(pair => Numbers(root, ("RayleighAlphaPerS", pair.AlphaPerS), ("RayleighBetaS", pair.BetaS))).IfNone(Seq<ElementFact>()),
            hygrothermal: value => Numbers(root, ("Porosity", value.Porosity), ("WaterContent80Rh", value.WaterContent80Rh.Si),
                ("FreeWaterSaturation", value.FreeWaterSaturation.Si))
                + OptionalNumber(root, "WaterAbsorptionKgPerM2SqrtS", value.WaterAbsorptionKgPerM2SqrtS)
                + Curve(root, "SorptionIsotherm", value.SorptionIsotherm)
                + Curve(root, "LiquidTransport", value.LiquidTransport)
                + Curve(root, "MoistureConductivity", value.MoistureConductivity),
            durability: value => Numbers(root, ("CarbonationRateMmPerSqrtYear", value.CarbonationRateMmPerSqrtYear),
                ("ChlorideDiffusion", value.ChlorideDiffusion.Si), ("AgeingExponent", value.AgeingExponent)),
            optical: value => Numbers(root, ("VisibleTransmittance", value.VisibleTransmittance),
                ("VisibleReflectanceFront", value.VisibleReflectanceFront), ("VisibleReflectanceBack", value.VisibleReflectanceBack),
                ("SolarTransmittance", value.SolarTransmittance), ("SolarReflectanceFront", value.SolarReflectanceFront),
                ("SolarReflectanceBack", value.SolarReflectanceBack), ("SolarAbsorptanceFront", value.SolarAbsorptanceFront),
                ("SolarAbsorptanceBack", value.SolarAbsorptanceBack), ("ThermalIrTransmittance", value.ThermalIrTransmittance),
                ("ThermalIrEmissivityFront", value.ThermalIrEmissivityFront), ("ThermalIrEmissivityBack", value.ThermalIrEmissivityBack)));
    }

    static Seq<ElementFact> UsageRows(Element baked) =>
        baked.Materials.Bind(material => Usage($"Usage.{material.Material.MaterialKey.Value}", material.Usage));

    static Seq<ElementFact> SectionRows(Element baked) =>
        baked.Materials.Bind(material => material.Material.Composition is MaterialComposition.ProfileSet profile
            ? profile.Section.Map(section => Numbers($"Material.{material.Material.MaterialKey.Value}.Section",
                ("Area", section.Area.Si), ("Iyy", section.Iyy.Si), ("Izz", section.Izz.Si), ("J", section.J.Si), ("Iw", section.Iw.Si),
                ("Wely", section.Wely.Si), ("Welz", section.Welz.Si), ("Wply", section.Wply.Si), ("Wplz", section.Wplz.Si),
                ("AvY", section.AvY.Si), ("AvZ", section.AvZ.Si), ("RadiusOfGyrationMajor", section.RadiusOfGyrationMajor.Si),
                ("RadiusOfGyrationMinor", section.RadiusOfGyrationMinor.Si), ("Depth", section.Depth.Si), ("Width", section.Width.Si),
                ("HeatedPerimeter", section.HeatedPerimeter.Si), ("AxisDistance", section.AxisDistance.Si),
                ("ShearCentreY", section.ShearCentreY.Si), ("ShearCentreZ", section.ShearCentreZ.Si),
                ("MonosymmetryFactor", section.MonosymmetryFactor))).IfNone(Seq<ElementFact>())
            : Seq<ElementFact>());

    static Seq<ElementFact> QuantityRows(Element baked) =>
        baked.Quantities.Bind(bag => Seq<ElementFact>(
            new ElementFact.Symbolic($"Quantity.{bag.SetName}.Inheritance", bag.Inheritance.Key),
            new ElementFact.Symbolic($"Quantity.{bag.SetName}.Source", bag.Source.Token))
            + bag.Values.Pairs.Map(pair =>
                (ElementFact)new ElementFact.Numeric($"Quantity.{bag.SetName}.{pair.Key.Value}", pair.Value.Si)));

    static Seq<ElementFact> PropertyRows(Element baked) {
        Seq<ElementFact> authored = baked.Properties.Bind(bag => Seq<ElementFact>(
            new ElementFact.Symbolic($"Property.{bag.SetName}.Inheritance", bag.Inheritance.Key),
            new ElementFact.Symbolic($"Property.{bag.SetName}.Source", bag.Source.Token))
            + bag.Values.Pairs.Map(pair =>
                (ElementFact)new ElementFact.Property($"Property.{bag.SetName}.{pair.Key.Value}", pair.Value)));
        Seq<string> candidates = toSeq(baked.Materials
            .Map(static row => row.Material.MaterialKey.Value)
            .GroupBy(static key => key, StringComparer.Ordinal))
            .Map(static group => group.Key);
        Option<string> fallback = candidates.Head.Filter(_ => candidates.Count == 1);
        Option<string> material = baked.Properties
            .Bind(static bag => bag.Values.Pairs)
            .Choose(static pair => pair.Key.Value == MaterialRow && pair.Value is PropertyValue.Text text ? Some(text.Value) : None)
            .Head
            | fallback;
        return authored + material.Map(value => Seq<ElementFact>(new ElementFact.Symbolic(MaterialRow, value))).IfNone(Seq<ElementFact>());
    }

    static Seq<ElementFact> TopologyRows(Seq<Relationship> topology) =>
        topology.Map((relation, index) => relation.Switch(
            compose: value => Symbols($"Relation.{index}",
                    ("Case", nameof(Relationship.Compose)), ("Whole", value.Whole.Value), ("Part", value.Part.Value),
                    ("Kind", value.SubKind.Key))
                + value.Ordinal.Map(ordinal => Numbers($"Relation.{index}", ("Ordinal", ordinal))).IfNone(Seq<ElementFact>()),
            assign: value => Symbols($"Relation.{index}",
                ("Case", nameof(Relationship.Assign)), ("Subject", value.Subject.Value),
                ("Definition", value.Definition.Value), ("Kind", value.SubKind.Key)),
            associate: value => Symbols($"Relation.{index}",
                    ("Case", nameof(Relationship.Associate)), ("Subject", value.Subject.Value), ("Resource", value.Resource.Value))
                + Usage($"Relation.{index}.Usage", value.Usage),
            connect: value => Symbols($"Relation.{index}",
                    ("Case", nameof(Relationship.Connect)), ("From", value.From.Value), ("To", value.To.Value),
                    ("Kind", value.SubKind.Key))
                + value.Realizing.Map(realizing => Symbols($"Relation.{index}", ("Realizing", realizing.Value))).IfNone(Seq<ElementFact>())
                + value.Interface.Map(key => Symbols($"Relation.{index}", ("Interface", key.ToString()))).IfNone(Seq<ElementFact>()),
            @void: value => Symbols($"Relation.{index}",
                ("Case", nameof(Relationship.Void)), ("Host", value.Host.Value), ("Feature", value.Feature.Value),
                ("Kind", value.SubKind.Key)),
            generic: value => Symbols($"Relation.{index}",
                    ("Case", nameof(Relationship.Generic)), ("WireName", value.WireName),
                    ("Source", value.Source.Value), ("Target", value.Target.Value))
                + value.Attributes.Pairs.Map(pair =>
                    (ElementFact)new ElementFact.Property($"Relation.{index}.Attribute.{pair.Key.Value}", pair.Value))
                + value.Participants.Map((participant, participantIndex) => Seq<ElementFact>(
                    new ElementFact.Symbolic($"Relation.{index}.Participant.{participantIndex}.Node", participant.Node.Value),
                    new ElementFact.Symbolic($"Relation.{index}.Participant.{participantIndex}.Role", participant.Role))
                    + participant.Ordinal.Map(ordinal => Numbers(
                        $"Relation.{index}.Participant.{participantIndex}", ("Ordinal", ordinal))).IfNone(Seq<ElementFact>())).Bind(identity)))
            .Bind(identity);

    static ReadOnlyMemory<byte> CanonicalProperties(ElementGraph graph, Element baked) {
        CanonicalWriter writer = new(graph.Header.Tolerance);
        Seq<PropertyBag> bags = baked.Properties.OrderBy(static bag => bag.SetName, StringComparer.Ordinal).ToSeq();
        writer.Ordinal(bags.Count);
        foreach (PropertyBag bag in bags) {
            writer.String(bag.SetName).Ordinal(bag.Values.Count);
            foreach ((PropertyName name, PropertyValue value) in bag.Values.Pairs.OrderBy(static pair => pair.Key.Value, StringComparer.Ordinal)) {
                writer.String(name.Value);
                value.CanonicalBytes(writer);
            }
        }
        return writer.ToBytes();
    }

    static Seq<ElementFact> Numbers(string root, params (string Name, double Value)[] values) =>
        toSeq(values).Map(value => (ElementFact)new ElementFact.Numeric($"{root}.{value.Name}", value.Value));

    static Seq<ElementFact> Symbols(string root, params (string Name, string Value)[] values) =>
        toSeq(values).Map(value => (ElementFact)new ElementFact.Symbolic($"{root}.{value.Name}", value.Value));

    // OptionalNumber, never Optional: a member named Optional captures the Prelude combinator inside this type.
    static Seq<ElementFact> OptionalNumber(string root, string name, Option<double> value) =>
        value.Map(number => Numbers(root, (name, number))).IfNone(Seq<ElementFact>());

    static Seq<ElementFact> OptionalMeasure(string root, string name, Option<MeasureValue> value) =>
        value.Map(measure => Numbers(root, (name, measure.Si))).IfNone(Seq<ElementFact>());

    static Seq<ElementFact> Curve(string root, string name, Option<SampledCurve> curve) =>
        curve.Map(value => toSeq(value.Axis).Zip(toSeq(value.Values))
            .Map((pair, index) => Seq<ElementFact>(
                new ElementFact.Numeric($"{root}.{name}.{index}.Axis", pair.Item1),
                new ElementFact.Numeric($"{root}.{name}.{index}.Value", pair.Item2)))
            .Bind(identity)).IfNone(Seq<ElementFact>());

    static Seq<ElementFact> Usage(string root, MaterialUsage usage) => usage.Switch(
        none: _ => Symbols(root, ("Kind", nameof(MaterialUsage.None))),
        layerSet: value => Symbols(root,
                ("Kind", nameof(MaterialUsage.LayerSet)), ("Direction", value.Direction.Key), ("Sense", value.Sense.Key))
            + OptionalMeasure(root, "OffsetFromReferenceLine", value.OffsetFromReferenceLine)
            + OptionalMeasure(root, "ReferenceExtent", value.ReferenceExtent),
        profileSet: value => Symbols(root, ("Kind", nameof(MaterialUsage.ProfileSet)))
            + value.CardinalPoint.Map(point => Numbers(root, ("CardinalPoint", point.Key))).IfNone(Seq<ElementFact>())
            + OptionalMeasure(root, "ReferenceExtent", value.ReferenceExtent));

    static Error Translation(UInt128 locus) =>
        new FabricationFault.IngressTranslation(new SourceLocus.ElementNode(locus));
}
```

## [03]-[LIFECYCLE]

`ElementSource` admits a distinct, non-empty subject roster over one graph. Each subject bakes once, snapshots `EdgesAt`, derives one representation identity, coalesces facts under graph tolerance, lowers realizing connections, and seals `ElementReceipt`; roster arity selects singular or batch admission. `ElementImport.Project` reads only the receipt, rails buffer failures through the retained locus, and leaves writer disposal with the caller.

## [04]-[RESEARCH]

<!-- source-only: research row template:
[TOKEN]-[OPEN|BLOCKED]: <exact question>; <verification route>.
[SPLIT_MEMBER]-[OPEN]: does `shape-core` expose `split_all`; verify against the member rail.
-->

(none)
