# [RASM_FABRICATION_ELEMENT_INGRESS]

`ElementImport` admits baked geometry once, lowers fabrication evidence into one `ElementReceipt`, then projects without reopening `ElementGraph`. `ElementSource` admits one graph and an identity-distinct subject roster whose arity selects singular or batch outcome. `ElementPayload` admits distinct representation slots with at most one mesh carrier and derives one count-framed identity.

`FactColumn` is the ONE evidence row: a declared member name paired with the reader that pulls its value, so a path is composed by `FactScope` and a member name is never spelled beside its own access. Every path mints as a `PropertyName` through `PropertyCategory.Fabrication.Row`, which is exactly the key space `AdmittedComponent.Quantities`, `.Properties`, `ComponentLayer.MaterialKey`, and `ComponentConnection` already demand. The edge snapshot is canonically ORDERED before any ordinal reaches a path, so the component's content key is a function of the edge set rather than of graph traversal order. Independent duplicate-path conflicts accumulate with path-derived loci before `AdmittedComponent` mints, while tolerance-equal observations coalesce. `ElementReceipt.Topology` preserves the ordered `Relationship` rows, and each realizing `Connect` lowers into `AdmittedComponent.Connections`; `At` stays absent because the interface is a blob content key.

## [01]-[INDEX]

- [02]-[ELEMENT_INGRESS]: `Rasm.Element`'s composed `RepresentationSlot` roster carrying the `ElementPart.Admitted` column seated over it, the `ElementGeometry`/`ElementPayload` carrier a slot admits, `ElementSource` graph-bearing ingress, the `FactValue`/`ElementFact` evidence row, and the `ElementReceipt` sealing component, topology, canonical bytes, and fault locus.
- [03]-[FACT_COLUMNS]: `FactScope`, `FactColumn`, and the declared column tables every fabrication fact folds through.
- [04]-[LIFECYCLE]: `ElementImport.Admit` baking each distinct subject once under graph tolerance with arity-selected singular or batch admission, and `ElementImport.Project` reading the receipt alone.

## [02]-[ELEMENT_INGRESS]

- Owner: `ElementSource` owns graph-bearing ingress; `ElementSubject` owns element identity with resolved representation; `ElementReceipt` owns the admitted carrier, ordered relationship rows, typed facts, canonical property bytes, and fault locus; `ElementImport` owns admission and egress.
- Cases: `ElementGeometry` closes mesh, profile, and axis carriers; `FactValue` closes numeric, symbolic, and typed-property readings and preserves every `PropertyValue` case; `ElementAdmission` preserves singular and batch cardinality; `ElementEgress` selects `Component` · `Topology` · `Facts` · `CanonicalProperties`; `ElementProjection` returns the matching result or committed byte count.
- Law: the complete identifier vocabulary is `Rasm.Element`'s `Graph/element#NODE_MODEL` `RepresentationSlot`, COMPOSED here — the graph lookup is `representations.At(slot)` off that owner's own keyed map, so no arm re-spells an identifier string and a second declaration of the roster is the deleted form; Fabrication seats only the `ElementPart.Admitted` carrier column Element cannot hold, admitting its mesh, axis, box, and footprint carriers and explicitly refusing every opaque slot through the roster's generated total `Switch`, so a new Element row breaks this page rather than inheriting support.
- Law: a fact PATH is a `PropertyName` minted under the seam's own `PropertyCategory.Fabrication` prefix — the key space `AdmittedComponent` reads — so a bare string key never reaches the component and a `PropertyName.Create` at a write site is the deleted form.
- Auto: generated `Switch` members keep every closed family total, so equivalence and rendering dispatch through the union rather than a tuple pattern that goes silently non-total on a new case; the derived quantity and property maps are HELD, so a fold reading both pays one build.
- Receipt: `ElementReceipt` carries `AdmittedComponent`, the ordered `Seq<Relationship>`, `ElementFactSet`, count-prefixed canonical property bytes, and the element content locus; `ElementAdmission` preserves one or many receipts.
- Boundary: `ElementGraph` never crosses the receipt; `Relationship`, `PropertyValue`, `MaterialComposition`, `MaterialPropertySet`, and `MaterialUsage` remain their canonical generated owners; `NodeId` and provider types lower to strings or content keys only at fact egress; no connection line is synthesized, and a `Connect` row without a realizing element stays topology-only because `ComponentConnection` demands a realizing key; faults from `Rasm.Element` pass through unchanged and local ingress or egress conflicts mint `IngressTranslation`; canonical-property ordering and caller-buffer commit are the serialization-boundary statement kernels.

```csharp
// --- [RUNTIME_PRELUDE] -----------------------------------------------------------------
using System.Globalization;
using System.Linq;
using CommunityToolkit.HighPerformance.Buffers;
using LanguageExt;
using LanguageExt.Common;
using Rasm.Domain;
using Rasm.Element.Classification;
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
using PropertyBag = Rasm.Element.Properties.ValueBag<Rasm.Element.Properties.PropertyValue>;
using QuantityBag = Rasm.Element.Properties.ValueBag<Rasm.Element.Properties.MeasureValue>;

namespace Rasm.Fabrication.Ingress;

// --- [MODELS] --------------------------------------------------------------------------
[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record ElementGeometry {
    private ElementGeometry() { }

    public sealed record Mesh(MeshSpace Value) : ElementGeometry;
    public sealed record Profiles(Arr<Loop> Value) : ElementGeometry;
    public sealed record Centreline(Edge3 Value) : ElementGeometry;
}

public sealed record ElementPart(RepresentationSlot Slot, ElementGeometry Value) {
    public bool Admitted => Slot.Switch(
        state: Value,
        body: static carrier => carrier is ElementGeometry.Mesh,
        axis: static carrier => carrier is ElementGeometry.Centreline,
        footPrint: static carrier => carrier is ElementGeometry.Profiles,
        box: static carrier => carrier is ElementGeometry.Mesh,
        annotation: static _ => false,
        surface: static _ => false,
        profile: static _ => false,
        clearance: static _ => false,
        cog: static _ => false,
        lighting: static _ => false,
        reference: static _ => false);
}

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

    [BoundaryAdapter]
    static partial void ValidateFactoryArguments(ref ValidationError? validationError, ref Seq<ElementPart> parts) {
        parts = toSeq(parts.OrderBy(static part => part.Slot.Key, StringComparer.Ordinal));
        validationError = Validation(parts);
    }

    public static Fin<ElementPayload> Admit(Seq<ElementPart> parts) =>
        Validate(parts, out ElementPayload payload).Admitted(payload);

    private static ValidationError? Validation(Seq<ElementPart> parts) =>
        parts.IsEmpty ? Invalid("parts")
        : parts.Map(static part => part.Slot.Key).Distinct().Count != parts.Count ? Invalid("slot-repeat")
        : parts.Exists(static part => !part.Admitted) ? Invalid("slot-carrier")
        : parts.Count(static part => part.Value is ElementGeometry.Mesh) > 1 ? Invalid("mesh-arity")
        : parts.Exists(static part => part.Value is ElementGeometry.Mesh or ElementGeometry.Profiles) ? null
        : Invalid("fabricable");

    private static ValidationError Invalid(string slot) => new($"element-payload:{slot}");
}

public sealed record ElementSubject(NodeId Id, ElementPayload Payload);

[ComplexValueObject]
public sealed partial class ElementSource {
    public ElementGraph Graph { get; }
    public Seq<ElementSubject> Subjects { get; }
    public Op Key { get; }

    [BoundaryAdapter]
    static partial void ValidateFactoryArguments(
        ref ValidationError? validationError,
        ref ElementGraph graph,
        ref Seq<ElementSubject> subjects,
        ref Op key) {
        if (subjects.IsEmpty || subjects.Map(static subject => subject.Id.Value).Distinct().Count != subjects.Count)
            validationError = new ValidationError("element-source:subjects");
    }

    public static Fin<ElementSource> Admit(ElementGraph graph, Seq<ElementSubject> subjects, Op key) =>
        Validate(graph, subjects, key, out ElementSource source).Admitted(source);
}

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record FactValue {
    private FactValue() { }

    public sealed record Number(double Value) : FactValue;
    public sealed record Text(string Value) : FactValue;
    public sealed record Typed(PropertyValue Value) : FactValue;

    public bool Equivalent(FactValue other, double tolerance) => Switch(
        state: (Other: other, Tolerance: tolerance),
        number: static (probe, row) => probe.Other is Number peer && Math.Abs(row.Value - peer.Value) <= probe.Tolerance,
        text: static (probe, row) => probe.Other is Text peer && StringComparer.Ordinal.Equals(row.Value, peer.Value),
        typed: static (probe, row) => probe.Other is Typed peer && row.Value.Equals(peer.Value));

    public Option<double> Quantity => Switch(
        number: static row => Some(row.Value),
        text: static _ => Option<double>.None,
        typed: static _ => Option<double>.None);

    public Option<string> Rendered => Switch(
        number: static _ => Option<string>.None,
        text: static row => Some(row.Value),
        typed: static row => Some(row.Value.Render()));
}

public sealed record ElementFact(PropertyName Path, FactValue Value) {
    public bool Equivalent(ElementFact other, double tolerance) => Value.Equivalent(other.Value, tolerance);
}

[ComplexValueObject]
public sealed partial class ElementFactSet {
    public Seq<ElementFact> Rows { get; }

    [IgnoreMember]
    private Map<PropertyName, double>? quantities;

    [IgnoreMember]
    private Map<PropertyName, string>? properties;

    [IgnoreMember]
    public Map<PropertyName, double> Quantities => quantities ??= Rows
        .Choose(static row => row.Value.Quantity.Map(value => (row.Path, Value: value)))
        .Fold(Map<PropertyName, double>(), static (index, row) => index.AddOrUpdate(row.Path, row.Value));

    [IgnoreMember]
    public Map<PropertyName, string> Properties => properties ??= Rows
        .Choose(static row => row.Value.Rendered.Map(value => (row.Path, Value: value)))
        .Fold(Map<PropertyName, string>(), static (index, row) => index.AddOrUpdate(row.Path, row.Value));

    [BoundaryAdapter]
    static partial void ValidateFactoryArguments(ref ValidationError? validationError, ref Seq<ElementFact> rows) {
        if (rows.Map(static row => row.Path).Distinct().Count != rows.Count)
            validationError = new ValidationError("element-facts:path-repeat");
    }

    public static Fin<ElementFactSet> Admit(Seq<ElementFact> rows) =>
        Validate(rows, out ElementFactSet facts).Admitted(facts);
}

[ComplexValueObject]
public sealed partial class ElementReceipt {
    public AdmittedComponent Component { get; }
    public Seq<Relationship> Topology { get; }
    public ElementFactSet Facts { get; }
    public ReadOnlyMemory<byte> CanonicalProperties { get; }
    public UInt128 Locus { get; }

    [BoundaryAdapter]
    static partial void ValidateFactoryArguments(
        ref ValidationError? validationError,
        ref AdmittedComponent component,
        ref Seq<Relationship> topology,
        ref ElementFactSet facts,
        ref ReadOnlyMemory<byte> canonicalProperties,
        ref UInt128 locus) {
        if (locus == UInt128.Zero)
            validationError = new ValidationError("element-receipt:locus");
    }

    public static Fin<ElementReceipt> Admit(
        AdmittedComponent component,
        Seq<Relationship> topology,
        ElementFactSet facts,
        ReadOnlyMemory<byte> canonicalProperties,
        UInt128 locus) =>
        Validate(component, topology, facts, canonicalProperties, locus, out ElementReceipt receipt).Admitted(receipt);
}

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record ElementAdmission {
    private ElementAdmission() { }

    public sealed record One(ElementReceipt Receipt) : ElementAdmission;
    public sealed record Many(Seq<ElementReceipt> Receipts) : ElementAdmission;
}

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record ElementEgress {
    private ElementEgress() { }

    public sealed record Component : ElementEgress;
    public sealed record Topology : ElementEgress;
    public sealed record Facts : ElementEgress;
    public sealed record CanonicalProperties(ArrayPoolBufferWriter<byte> Destination) : ElementEgress;
}

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record ElementProjection {
    private ElementProjection() { }

    public sealed record Component(AdmittedComponent Value) : ElementProjection;
    public sealed record Topology(Seq<Relationship> Value) : ElementProjection;
    public sealed record Facts(ElementFactSet Value) : ElementProjection;
    public sealed record Written(int Count) : ElementProjection;
}
```

## [03]-[FACT_COLUMNS]

- Owner: `FactColumn` owns one member's row name and its reader; `FactScope` owns path composition; `ElementColumns` owns every declared table the lowering folds; `FactColumns` owns the one `Emit` fan and the `Sound` completeness census over that owner's own fields.
- Law: a member NAME appears exactly once, in the row that mints it, and the reader sits on the same line — so a fact path can never drift from the member it reports, and the interpolation that composed a root at each of a hundred and fifty sites collapses to the one join `FactScope.Row` runs.
- Law: an absent reading is `None` and emits NO row. An optional column that emitted a zero published a measured value the graph never carried, and the coalescing census would then treat two absences as agreeing readings.
- Law: every table is a HAND-KEPT MIRROR of its source type's member set, and `FactColumns.Sound` is what proves it — the census reflects THIS owner's own fields at first construction, so the proof roster cannot drift from the tables it proves, and a source member that no column names and no `[Unpublished]` carve claims throws where the tables initialize rather than dropping a fact in silence the way three of `PropertyEvidence`'s six went unpublished. Every table carries its own carve with its own reason: a member published by another table names that landing, a composite carrier's fields carve wholesale because they are the reach and never a fact, and a member nothing reaches carves with the reason it is unreachable — `Element.Observations` is the one such member, because no fabrication consumer reads a time series. NAMED LOSS: the proof matches NAMES, not readers, since a delegate body is not reflectable; the stronger form is a generated projection at the `Rasm.Element` owner, which is where the roster moves the day that owner publishes one.
- Cases: a column reads a number, a symbol, or a typed `PropertyValue`; the sources are `Element` itself, one material composition case, one `MaterialPropertySet` family, one section profile, one usage case, and one relationship case.
- Auto: banded and indicator rosters (`AcousticBand`, `ImpactCategory`, `LifecycleStage`) fold their OWN `Items`, so a new band or indicator is a generated row and never a column here.
- Growth: a new fabrication fact is one row on the owning table; a new source family is one table and one arm on the lowering fold — and either lands with its `[Unpublished]` carve, because the census refuses a table whose source carries a member no column names.
- Packages: `System.Reflection` supplies the field, generic-argument, and property census `FactColumns.Sound` runs at first construction — the one reflective read on the page, paid once per process rather than per fact; `Rasm.Element` supplies every source type the tables mirror; LanguageExt.Core supplies `Seq`/`Option`/`Unit` and the `Empty`/`More` match the proof folds through.
- Boundary: this cluster reads member VALUES only — no admission, no conversion policy, no fault. Unit lifting stays at the `Rasm.Element` measure owner, which already publishes SI scalars.

```csharp
// --- [MODELS] --------------------------------------------------------------------------
// --- [FACT_COLUMNS]
public readonly record struct FactScope(string Prefix) {
    public static readonly FactScope Root = new(string.Empty);

    public FactScope Then(string segment) => new(Prefix.Length == 0 ? segment : $"{Prefix}.{segment}");

    public FactScope Then(string segment, int ordinal) => Then(segment).Then(ordinal.ToString(CultureInfo.InvariantCulture));

    public PropertyName Row(string column) =>
        PropertyCategory.Fabrication.Row(Prefix.Length == 0 ? column : $"{Prefix}.{column}");
}

public interface IFactColumn {
    string Name { get; }
}

public sealed record FactColumn<TSource>(string Name, Func<TSource, Option<FactValue>> Read) : IFactColumn {
    public Option<ElementFact> Of(FactScope scope, TSource source) =>
        Read(source).Map(value => new ElementFact(scope.Row(Name), value));
}

[AttributeUsage(AttributeTargets.Field)]
public sealed class UnpublishedAttribute(string reason, params string[] members) : Attribute {
    public string Reason { get; } = reason;
    public IReadOnlyList<string> Members { get; } = members;
}

public static class FactColumns {
    public static Seq<ElementFact> Emit<TSource>(this Seq<FactColumn<TSource>> columns, FactScope scope, TSource source) =>
        columns.Choose(column => column.Of(scope, source));

    public static Unit Sound(Type owner) =>
        toSeq(owner.GetFields(BindingFlags.Public | BindingFlags.Static))
            .Filter(static field => field.FieldType.IsGenericType
                && field.FieldType.GetGenericTypeDefinition() == typeof(Seq<>)
                && field.FieldType.GetGenericArguments()[0].IsGenericType
                && field.FieldType.GetGenericArguments()[0].GetGenericTypeDefinition() == typeof(FactColumn<>))
            .Bind(field => Uncovered(field).Map(member => $"{field.Name}.{member}"))
            .Match(
                Empty: static () => unit,
                More: static gaps => throw new InvalidOperationException(string.Create(
                    provider: CultureInfo.InvariantCulture,
                    $"fact tables drop members with no carve: {string.Join(", ", gaps)}")));

    private static Seq<string> Uncovered(FieldInfo field) {
        Type source = field.FieldType.GetGenericArguments()[0].GetGenericArguments()[0];
        Seq<string> segments = toSeq((IEnumerable<IFactColumn>)field.GetValue(obj: null)!)
            .Bind(static column => toSeq(column.Name.Split('.')))
            .Distinct();
        Seq<string> carved = toSeq(field.GetCustomAttribute<UnpublishedAttribute>()?.Members ?? []);
        return toSeq(source.GetProperties(BindingFlags.Public | BindingFlags.Instance))
            .Map(static member => member.Name)
            .Filter(member => !segments.Contains(member) && !carved.Contains(member));
    }
}

public static class ElementColumns {
    static ElementColumns() => ignore(FactColumns.Sound(typeof(ElementColumns)));

    public const string MaterialRow = "material";

    [Unpublished("published by their own tables and folds",
        nameof(Element.Classifications), nameof(Element.Representations), nameof(Element.Materials),
        nameof(Element.Properties), nameof(Element.Quantities), nameof(Element.Parts),
        nameof(Element.Assessments), nameof(Element.Coverages), nameof(Element.Appearance),
        nameof(Element.History), nameof(Element.Type), nameof(Element.Observations))]
    public static readonly Seq<FactColumn<Element>> Identity = Seq(
        Sym<Element>("Element.Id", static row => row.Id.Value),
        Sym<Element>("Element.Kind", static row => row.Kind.Key),
        Sym<Element>("Element.PredefinedType", static row => row.PredefinedType.Key),
        Sym<Element>("Element.Name", static row => row.Name),
        Sym<Element>("Element.Tag", static row => row.Tag),
        Sym<Element>("Element.Classification.System", static row => row.Classification.System),
        Sym<Element>("Element.Classification.Code", static row => row.Classification.Code),
        Sym<Element>("Element.Classification.Edition", static row => row.Classification.Edition),
        SymOpt<Element>("Element.ExternalId", static row => row.ExternalId),
        SymOpt<Element>("Element.TypeId", static row => row.TypeId.Map(static id => id.Value)));

    public static readonly Seq<FactColumn<Classification>> Classification = Seq(
        Sym<Classification>("System", static row => row.System),
        Sym<Classification>("Code", static row => row.Code),
        Sym<Classification>("Edition", static row => row.Edition));

    [Unpublished("composite carrier: its fields are the reach, never a fact",
        nameof(ComponentCensus.Baked), nameof(ComponentCensus.Topology), nameof(ComponentCensus.Connections))]
    public static readonly Seq<FactColumn<ComponentCensus>> Census = Seq(
        Num<ComponentCensus>("Component.Parts", static row => row.Baked.Parts.Count),
        Num<ComponentCensus>("Component.Materials", static row => row.Baked.Materials.Count),
        Num<ComponentCensus>("Component.Properties", static row => row.Baked.Properties.Count),
        Num<ComponentCensus>("Component.Quantities", static row => row.Baked.Quantities.Count),
        Num<ComponentCensus>("Component.Assessments", static row => row.Baked.Assessments.Count),
        Num<ComponentCensus>("Component.Coverages", static row => row.Baked.Coverages.Count),
        Num<ComponentCensus>("Component.Relations", static row => row.Topology.Count),
        Num<ComponentCensus>("Component.Connections", static row => row.Connections.Count),
        Num<ComponentCensus>("Component.Openings", static row => row.Topology.Count(static relation => relation is Relationship.Void)),
        Num<ComponentCensus>("Component.HasAppearance", static row => row.Baked.Appearance.IsSome ? 1.0 : 0.0),
        Num<ComponentCensus>("Component.HasHistory", static row => row.Baked.History.IsSome ? 1.0 : 0.0));

    public static readonly Seq<FactColumn<MaterialComposition.Single>> Single = Seq(
        Sym<MaterialComposition.Single>("Material", static row => row.Material.Value));

    public static readonly Seq<FactColumn<MaterialLayer>> Layer = Seq(
        Sym<MaterialLayer>("Material", static row => row.Material.Value),
        Sym<MaterialLayer>("Name", static row => row.LayerName),
        Num<MaterialLayer>("Thickness", static row => row.Thickness.Si));

    public static readonly Seq<FactColumn<MaterialComposition.ProfileSet>> ProfileSet = Seq(
        Sym<MaterialComposition.ProfileSet>("Material", static row => row.Material.Value),
        Sym<MaterialComposition.ProfileSet>("Profile.Standard", static row => row.Profile.Standard),
        Sym<MaterialComposition.ProfileSet>("Profile.Designation", static row => row.Profile.Designation),
        Sym<MaterialComposition.ProfileSet>("Profile.ContentKey", static row => row.Profile.ContentKey.ToString()));

    public static readonly Seq<FactColumn<MaterialConstituent>> Constituent = Seq(
        Sym<MaterialConstituent>("Material", static row => row.Material.Value),
        Sym<MaterialConstituent>("Category", static row => row.Category),
        Num<MaterialConstituent>("Fraction", static row => row.Fraction));

    public static readonly Seq<FactColumn<PropertyEvidence>> Evidence = Seq(
        Sym<PropertyEvidence>("Evidence.Source", static row => row.Source),
        SymOpt<PropertyEvidence>("Evidence.Reference", static row => row.Reference),
        SymOpt<PropertyEvidence>("Evidence.ValidUntil",
            static row => row.ValidUntil.Map(static date => date.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture))),
        Sym<PropertyEvidence>("Evidence.Grade", static row => row.Grade.Token),
        Num<PropertyEvidence>("Evidence.Attributable", static row => row.Grade.Attributable ? 1.0 : 0.0),
        SymOpt<PropertyEvidence>("Evidence.Attested.Credential",
            static row => row.Attested.Map(static seal => seal.Credential)),
        SymOpt<PropertyEvidence>("Evidence.Run.Author", static row => row.Run.Map(static run => run.Author)),
        SymOpt<PropertyEvidence>("Evidence.Run.Tool", static row => row.Run.Map(static run => run.Tool)),
        SymOpt<PropertyEvidence>("Evidence.Run.Version", static row => row.Run.Map(static run => run.Version)));

    public static readonly Seq<FactColumn<MaterialPropertySet.Mechanical>> Mechanical = Seq(
        Num<MaterialPropertySet.Mechanical>("Density", static row => row.Density.Si),
        Num<MaterialPropertySet.Mechanical>("YoungsModulus", static row => row.YoungsModulus.Si),
        Num<MaterialPropertySet.Mechanical>("ShearModulus", static row => row.ShearModulus.Si),
        Num<MaterialPropertySet.Mechanical>("YieldStrength", static row => row.YieldStrength.Si),
        Num<MaterialPropertySet.Mechanical>("UltimateStrength", static row => row.UltimateStrength.Si),
        Num<MaterialPropertySet.Mechanical>("PoissonsRatio", static row => row.PoissonsRatio),
        Num<MaterialPropertySet.Mechanical>("ThermalExpansionPerK", static row => row.ThermalExpansionPerK));

    public static readonly Seq<FactColumn<MaterialPropertySet.Orthotropic>> Orthotropic = Seq(
        Num<MaterialPropertySet.Orthotropic>("Density", static row => row.Density.Si),
        Num<MaterialPropertySet.Orthotropic>("E1Parallel", static row => row.E1Parallel.Si),
        Num<MaterialPropertySet.Orthotropic>("E2Perpendicular", static row => row.E2Perpendicular.Si),
        Num<MaterialPropertySet.Orthotropic>("ShearModulus", static row => row.ShearModulus.Si),
        Num<MaterialPropertySet.Orthotropic>("Strength1Parallel", static row => row.Strength1Parallel.Si),
        Num<MaterialPropertySet.Orthotropic>("Strength2Perpendicular", static row => row.Strength2Perpendicular.Si),
        Num<MaterialPropertySet.Orthotropic>("ThermalExpansionPerK", static row => row.ThermalExpansionPerK));

    public static readonly Seq<FactColumn<MaterialPropertySet.Thermal>> Thermal = Seq(
        Num<MaterialPropertySet.Thermal>("Conductivity", static row => row.Conductivity.Si),
        Num<MaterialPropertySet.Thermal>("SpecificHeat", static row => row.SpecificHeat.Si),
        Num<MaterialPropertySet.Thermal>("UValue", static row => row.UValue.Si),
        Num<MaterialPropertySet.Thermal>("VapourResistanceFactor", static row => row.VapourResistanceFactor));

    public static readonly Seq<FactColumn<MaterialPropertySet.Acoustic>> Acoustic = Seq(
        Num<MaterialPropertySet.Acoustic>("Nrc", static row => row.Nrc),
        Num<MaterialPropertySet.Acoustic>("Saa", static row => row.Saa),
        Num<MaterialPropertySet.Acoustic>("StcWeighted", static row => row.StcWeighted),
        Num<MaterialPropertySet.Acoustic>("Rw", static row => row.Rw),
        Opt<MaterialPropertySet.Acoustic>("DynamicStiffnessMNPerM3", static row => row.DynamicStiffnessMNPerM3),
        Opt<MaterialPropertySet.Acoustic>("FlowResistivityPaSPerM2", static row => row.FlowResistivityPaSPerM2),
        Opt<MaterialPropertySet.Acoustic>("LossFactor", static row => row.LossFactor));

    public static readonly Seq<FactColumn<AcousticReading>> Band = Seq(
        Num<AcousticReading>("Absorption", static row => row.Set.At(row.Band)),
        Num<AcousticReading>("SoundReductionIndexDb", static row => row.Set.SriAt(row.Band)));

    public static readonly Seq<FactColumn<MaterialPropertySet.Fire>> Fire = Seq(
        Num<MaterialPropertySet.Fire>("LoadBearingMinutes", static row => row.Resistance.LoadBearingMinutes),
        Num<MaterialPropertySet.Fire>("IntegrityMinutes", static row => row.Resistance.IntegrityMinutes),
        Num<MaterialPropertySet.Fire>("InsulationMinutes", static row => row.Resistance.InsulationMinutes),
        Sym<MaterialPropertySet.Fire>("Reaction", static row => row.Reaction.Key),
        Sym<MaterialPropertySet.Fire>("Smoke", static row => row.Smoke.Key),
        Sym<MaterialPropertySet.Fire>("Droplets", static row => row.Droplets.Key));

    public static readonly Seq<FactColumn<MaterialPropertySet.Environmental>> Environmental = Seq(
        Num<MaterialPropertySet.Environmental>("RecycledContent", static row => row.RecycledContent),
        Num<MaterialPropertySet.Environmental>("EndOfLifeRecovery", static row => row.EndOfLifeRecovery),
        Num<MaterialPropertySet.Environmental>("WholeLifeGwp", static row => row.WholeLifeGwp),
        Sym<MaterialPropertySet.Environmental>("Basis", static row => row.Basis.Key));

    public static readonly Seq<FactColumn<ImpactReading>> Impact = Seq(
        Num<ImpactReading>("Indicator", static row => row.Set.IndicatorAt(row.Category, row.Stage)));

    public static readonly Seq<FactColumn<MaterialPropertySet.Cost>> Cost = Seq(
        Num<MaterialPropertySet.Cost>("SupplyPerUnit", static row => row.SupplyPerUnit),
        Num<MaterialPropertySet.Cost>("InstallPerUnit", static row => row.InstallPerUnit),
        Num<MaterialPropertySet.Cost>("LifecyclePerUnit", static row => row.LifecyclePerUnit),
        Sym<MaterialPropertySet.Cost>("Basis", static row => row.Basis.Key),
        Sym<MaterialPropertySet.Cost>("Currency", static row => row.Currency.Value));

    public static readonly Seq<FactColumn<MaterialPropertySet.Damping>> Damping = Seq(
        Num<MaterialPropertySet.Damping>("DampingRatio", static row => row.DampingRatio),
        Num<MaterialPropertySet.Damping>("StructuralLossFactor", static row => row.StructuralLossFactor),
        Opt<MaterialPropertySet.Damping>("RayleighAlphaPerS", static row => row.Rayleigh.Map(static pair => pair.AlphaPerS)),
        Opt<MaterialPropertySet.Damping>("RayleighBetaS", static row => row.Rayleigh.Map(static pair => pair.BetaS)));

    public static readonly Seq<FactColumn<MaterialPropertySet.Hygrothermal>> Hygrothermal = Seq(
        Num<MaterialPropertySet.Hygrothermal>("Porosity", static row => row.Porosity),
        Num<MaterialPropertySet.Hygrothermal>("WaterContent80Rh", static row => row.WaterContent80Rh.Si),
        Num<MaterialPropertySet.Hygrothermal>("FreeWaterSaturation", static row => row.FreeWaterSaturation.Si),
        Opt<MaterialPropertySet.Hygrothermal>("WaterAbsorptionKgPerM2SqrtS", static row => row.WaterAbsorptionKgPerM2SqrtS));

    public static readonly Seq<FactColumn<MaterialPropertySet.Durability>> Durability = Seq(
        Num<MaterialPropertySet.Durability>("CarbonationRateMmPerSqrtYear", static row => row.CarbonationRateMmPerSqrtYear),
        Num<MaterialPropertySet.Durability>("ChlorideDiffusion", static row => row.ChlorideDiffusion.Si),
        Num<MaterialPropertySet.Durability>("AgeingExponent", static row => row.AgeingExponent));

    public static readonly Seq<FactColumn<MaterialPropertySet.Optical>> Optical = Seq(
        Num<MaterialPropertySet.Optical>("VisibleTransmittance", static row => row.VisibleTransmittance),
        Num<MaterialPropertySet.Optical>("VisibleReflectanceFront", static row => row.VisibleReflectanceFront),
        Num<MaterialPropertySet.Optical>("VisibleReflectanceBack", static row => row.VisibleReflectanceBack),
        Num<MaterialPropertySet.Optical>("SolarTransmittance", static row => row.SolarTransmittance),
        Num<MaterialPropertySet.Optical>("SolarReflectanceFront", static row => row.SolarReflectanceFront),
        Num<MaterialPropertySet.Optical>("SolarReflectanceBack", static row => row.SolarReflectanceBack),
        Num<MaterialPropertySet.Optical>("SolarAbsorptanceFront", static row => row.SolarAbsorptanceFront),
        Num<MaterialPropertySet.Optical>("SolarAbsorptanceBack", static row => row.SolarAbsorptanceBack),
        Num<MaterialPropertySet.Optical>("ThermalIrTransmittance", static row => row.ThermalIrTransmittance),
        Num<MaterialPropertySet.Optical>("ThermalIrEmissivityFront", static row => row.ThermalIrEmissivityFront),
        Num<MaterialPropertySet.Optical>("ThermalIrEmissivityBack", static row => row.ThermalIrEmissivityBack));

    public static readonly Seq<FactColumn<CurveSample>> Sample = Seq(
        Num<CurveSample>("Axis", static row => row.Axis),
        Num<CurveSample>("Value", static row => row.Value));

    public static readonly Seq<FactColumn<SectionProperties>> Section = Seq(
        Num<SectionProperties>("Area", static row => row.Area.Si),
        Num<SectionProperties>("Iyy", static row => row.Iyy.Si),
        Num<SectionProperties>("Izz", static row => row.Izz.Si),
        Num<SectionProperties>("J", static row => row.J.Si),
        Num<SectionProperties>("Iw", static row => row.Iw.Si),
        Num<SectionProperties>("Wely", static row => row.Wely.Si),
        Num<SectionProperties>("Welz", static row => row.Welz.Si),
        Num<SectionProperties>("Wply", static row => row.Wply.Si),
        Num<SectionProperties>("Wplz", static row => row.Wplz.Si),
        Num<SectionProperties>("AvY", static row => row.AvY.Si),
        Num<SectionProperties>("AvZ", static row => row.AvZ.Si),
        Num<SectionProperties>("RadiusOfGyrationMajor", static row => row.RadiusOfGyrationMajor.Si),
        Num<SectionProperties>("RadiusOfGyrationMinor", static row => row.RadiusOfGyrationMinor.Si),
        Num<SectionProperties>("Depth", static row => row.Depth.Si),
        Num<SectionProperties>("Width", static row => row.Width.Si),
        Num<SectionProperties>("HeatedPerimeter", static row => row.HeatedPerimeter.Si),
        Num<SectionProperties>("AxisDistance", static row => row.AxisDistance.Si),
        Num<SectionProperties>("ShearCentreY", static row => row.ShearCentreY.Si),
        Num<SectionProperties>("ShearCentreZ", static row => row.ShearCentreZ.Si),
        Num<SectionProperties>("MonosymmetryFactor", static row => row.MonosymmetryFactor));

    public static readonly Seq<FactColumn<MaterialUsage.LayerSet>> LayerUsage = Seq(
        Sym<MaterialUsage.LayerSet>("Direction", static row => row.Direction.Key),
        Sym<MaterialUsage.LayerSet>("Sense", static row => row.Sense.Key),
        Measure<MaterialUsage.LayerSet>("OffsetFromReferenceLine", static row => row.OffsetFromReferenceLine),
        Measure<MaterialUsage.LayerSet>("ReferenceExtent", static row => row.ReferenceExtent));

    public static readonly Seq<FactColumn<MaterialUsage.ProfileSet>> ProfileUsage = Seq(
        SymOpt<MaterialUsage.ProfileSet>("CardinalPoint", static row => row.CardinalPoint.Map(static point => point.Key)),
        Measure<MaterialUsage.ProfileSet>("ReferenceExtent", static row => row.ReferenceExtent));

    public static readonly Seq<FactColumn<PropertyBag>> Bag = Seq(
        Sym<PropertyBag>("Inheritance", static row => row.Inheritance.Key),
        Sym<PropertyBag>("Source", static row => row.Source.Token));

    public static readonly Seq<FactColumn<Relationship.Compose>> Compose = Seq(
        Sym<Relationship.Compose>("Whole", static row => row.Whole.Value),
        Sym<Relationship.Compose>("Part", static row => row.Part.Value),
        Sym<Relationship.Compose>("Kind", static row => row.SubKind.Key),
        Opt<Relationship.Compose>("Ordinal", static row => row.Ordinal.Map(static value => (double)value)));

    public static readonly Seq<FactColumn<Relationship.Assign>> Assign = Seq(
        Sym<Relationship.Assign>("Subject", static row => row.Subject.Value),
        Sym<Relationship.Assign>("Definition", static row => row.Definition.Value),
        Sym<Relationship.Assign>("Kind", static row => row.SubKind.Key));

    public static readonly Seq<FactColumn<Relationship.Associate>> Associate = Seq(
        Sym<Relationship.Associate>("Subject", static row => row.Subject.Value),
        Sym<Relationship.Associate>("Resource", static row => row.Resource.Value));

    public static readonly Seq<FactColumn<Relationship.Connect>> Connect = Seq(
        Sym<Relationship.Connect>("From", static row => row.From.Value),
        Sym<Relationship.Connect>("To", static row => row.To.Value),
        Sym<Relationship.Connect>("Kind", static row => row.SubKind.Key),
        SymOpt<Relationship.Connect>("Realizing", static row => row.Realizing.Map(static node => node.Value)),
        SymOpt<Relationship.Connect>("Interface", static row => row.Interface.Map(static key => key.ToString())));

    public static readonly Seq<FactColumn<Relationship.Void>> Opening = Seq(
        Sym<Relationship.Void>("Host", static row => row.Host.Value),
        Sym<Relationship.Void>("Feature", static row => row.Feature.Value),
        Sym<Relationship.Void>("Kind", static row => row.SubKind.Key));

    public static readonly Seq<FactColumn<Relationship.Generic>> Generic = Seq(
        Sym<Relationship.Generic>("WireName", static row => row.WireName),
        Sym<Relationship.Generic>("Source", static row => row.Source.Value),
        Sym<Relationship.Generic>("Target", static row => row.Target.Value));

    public static readonly Seq<FactColumn<RelationshipParticipant>> Participant = Seq(
        Sym<RelationshipParticipant>("Node", static row => row.Node.Value),
        Sym<RelationshipParticipant>("Role", static row => row.Role),
        Opt<RelationshipParticipant>("Ordinal", static row => row.Ordinal.Map(static value => (double)value)));

    private static FactColumn<TSource> Num<TSource>(string name, Func<TSource, double> read) =>
        new(name, source => Some<FactValue>(new FactValue.Number(read(source))));

    private static FactColumn<TSource> Opt<TSource>(string name, Func<TSource, Option<double>> read) =>
        new(name, source => read(source).Map(static value => (FactValue)new FactValue.Number(value)));

    private static FactColumn<TSource> Measure<TSource>(string name, Func<TSource, Option<MeasureValue>> read) =>
        new(name, source => read(source).Map(static value => (FactValue)new FactValue.Number(value.Si)));

    private static FactColumn<TSource> Sym<TSource>(string name, Func<TSource, string> read) =>
        new(name, source => Some<FactValue>(new FactValue.Text(read(source))));

    private static FactColumn<TSource> SymOpt<TSource>(string name, Func<TSource, Option<string>> read) =>
        new(name, source => read(source).Map(static value => (FactValue)new FactValue.Text(value)));

    private static FactColumn<TSource> Typed<TSource>(string name, Func<TSource, PropertyValue> read) =>
        new(name, source => Some<FactValue>(new FactValue.Typed(read(source))));
}

public readonly record struct ComponentCensus(Element Baked, Seq<Relationship> Topology, Arr<ComponentConnection> Connections);

public readonly record struct AcousticReading(MaterialPropertySet.Acoustic Set, AcousticBand Band);

public readonly record struct ImpactReading(MaterialPropertySet.Environmental Set, ImpactCategory Category, LifecycleStage Stage);

public readonly record struct CurveSample(double Axis, double Value);
```

## [04]-[LIFECYCLE]

- Owner: `ElementImport` owns admission, the fact lowering, and receipt-only egress.
- Law: the edge snapshot is ORDERED before any ordinal reaches a fact path. `EdgesAt` hands edges in graph traversal order, so an index-keyed path re-keys the whole component the day the graph re-orders its adjacency; sorting on the relation's own discriminant and endpoints makes each ordinal a function of the edge SET, and two rows tying on every ordering column are indistinguishable to every consumer.
- Entry: `ElementImport.Admit(ElementSource)` bakes each subject once and returns `Fin<ElementAdmission>`; `ElementImport.Project(ElementReceipt, ElementEgress)` returns `Fin<ElementProjection>` without graph access.
- Auto: arity alone selects the outcome case — the admitted source already proved the roster non-empty and identity-distinct, so a singular request can never arrive as a vacuous batch. `Validation<Error, _>` accumulates independent batch and duplicate-path faults; every generated owner crosses through the one `Admitted` bridge.
- Receipt: one grouping serves both the conflict census and the coalesced store, and each conflict carries its own path-derived locus so an accumulated batch names every offending path instead of repeating one error.
- Exemption: `CanonicalProperties` is a statement kernel — the ordered bag walk is the serialization boundary itself, and `CanonicalWriter` is mutable-fluent, so the loop IS the byte law; it opens through the RETAINING mint and closes on the rail, because the codec publishes no public constructor and only one of its two mints holds a buffer to hand back.
- Packages: `CommunityToolkit.HighPerformance` (`ArrayPoolBufferWriter<byte>` egress destination), `Rasm.Element` (`ElementGraph.Bake`, `CanonicalWriter.Retaining` and its `Fin`-answering `ToBytes` close, `PropertyCategory`); `Process/owner` `FabricationCanon.Ordered` for the two identity digests, LanguageExt.Core rails, `UnitsNet` at the layer-thickness projection.
- Boundary: writer disposal stays with the caller; a buffer failure rails through the retained locus rather than escaping the `Fin` return.

```csharp
// --- [OPERATIONS] ----------------------------------------------------------------------
public static class ElementImport {
    private const char Field = '\u001F';

    public static Fin<ElementAdmission> Admit(ElementSource source) =>
        source.Subjects
            .Traverse(subject => AdmitOne(source.Graph, subject, source.Key).ToValidation())
            .As()
            .ToFin()
            .Map(static receipts => receipts.Head
                .Filter(_ => receipts.Count == 1)
                .Match(
                    Some: static receipt => (ElementAdmission)new ElementAdmission.One(receipt),
                    None: () => new ElementAdmission.Many(receipts)));

    public static Fin<ElementProjection> Project(ElementReceipt receipt, ElementEgress egress) =>
        egress.Switch(
            state: receipt,
            component: static (state, _) => Fin.Succ<ElementProjection>(new ElementProjection.Component(state.Component)),
            topology: static (state, _) => Fin.Succ<ElementProjection>(new ElementProjection.Topology(state.Topology)),
            facts: static (state, _) => Fin.Succ<ElementProjection>(new ElementProjection.Facts(state.Facts)),
            canonicalProperties: static (state, request) => Op.Of(name: "element:projection").Catch(() => {
                int before = request.Destination.WrittenCount;
                request.Destination.Write(state.CanonicalProperties.Span);
                return Fin.Succ<ElementProjection>(new ElementProjection.Written(request.Destination.WrittenCount - before));
            }));

    private static Fin<ElementReceipt> AdmitOne(ElementGraph graph, ElementSubject subject, Op key) =>
        from baked in graph.Bake(subject.Id, key)
        let topology = Ordered(graph.EdgesAt(baked.Id))
        let tolerance = graph.Header.Tolerance
        let locus = LocusOf(baked.Id, FactScope.Root.Row(nameof(Element)), tolerance)
        let fault = Translation(locus, "element:admission")
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
            facts.Properties)
        from properties in CanonicalProperties(graph, baked, key)
        from receipt in ElementReceipt.Admit(component, topology, facts, properties, locus)
        select receipt;

    private static Fin<UInt128> Resolve(Element baked, ElementPayload payload, double tolerance, Error fault) =>
        payload.Parts
            .Traverse(part => baked.Representations.At(part.Slot)
                .Map(key => (Slot: part.Slot.Key, Key: key))
                .ToFin(fault)
                .ToValidation())
            .As()
            .ToFin()
            .Map(rows => FabricationCanon.Ordered(tolerance, writer => rows
                .Fold(writer.Ordinal(rows.Count), static (target, row) => target.String(row.Slot).U128(row.Key))));

    private static Seq<Relationship> Ordered(IEnumerable<Relationship> edges) =>
        toSeq(toSeq(edges).OrderBy(OrderKey, StringComparer.Ordinal));

    private static string OrderKey(Relationship relation) => relation.Switch(
        compose: static row => Join(nameof(Relationship.Compose), row.Whole.Value, row.Part.Value, row.SubKind.Key),
        assign: static row => Join(nameof(Relationship.Assign), row.Subject.Value, row.Definition.Value, row.SubKind.Key),
        associate: static row => Join(nameof(Relationship.Associate), row.Subject.Value, row.Resource.Value, string.Empty),
        connect: static row => Join(nameof(Relationship.Connect), row.From.Value, row.To.Value, row.SubKind.Key),
        @void: static row => Join(nameof(Relationship.Void), row.Host.Value, row.Feature.Value, row.SubKind.Key),
        generic: static row => Join(nameof(Relationship.Generic), row.Source.Value, row.Target.Value, row.WireName));

    private static string Join(string discriminant, string first, string second, string qualifier) =>
        string.Join(Field, discriminant, first, second, qualifier);

    private static Arr<ComponentConnection> ConnectionsOf(Seq<Relationship> topology) =>
        topology.Choose(static relation => relation is Relationship.Connect connect
            ? connect.Realizing.Bind(realizing => connect.Interface.Map(key => new ComponentConnection(
                PropertyCategory.Fabrication.Row(key.ToString(CultureInfo.InvariantCulture)),
                PropertyCategory.Fabrication.Row(realizing.Value),
                Option<Edge3>.None)))
            : None).ToArr();

    private static Arr<ComponentLayer> LayersOf(Element baked) =>
        baked.Materials.Bind(static material => material.Material.Composition.Switch(
            single: static _ => Seq<ComponentLayer>(),
            layerSet: static set => set.Layers.Map(static layer => new ComponentLayer(
                layer.LayerName,
                Length.FromMeters(layer.Thickness.Si).Millimeters,
                PropertyCategory.Fabrication.Row(layer.Material.Value))),
            profileSet: static _ => Seq<ComponentLayer>(),
            constituentSet: static _ => Seq<ComponentLayer>())).ToArr();

    private static Option<double> SheetOf(Element baked) {
        Seq<double> stacks = baked.Materials
            .Choose(static material => material.Material.Composition is MaterialComposition.LayerSet set
                ? Some(Length.FromMeters(set.TotalThickness).Millimeters)
                : None);
        return stacks.Head.Filter(_ => stacks.Count == 1);
    }

    private static Fin<ElementFactSet> FactsOf(
        Element baked,
        Seq<Relationship> topology,
        Arr<ComponentConnection> connections,
        double tolerance,
        Error fault) {
        Seq<ElementFact> rows =
            ElementColumns.Identity.Emit(FactScope.Root, baked)
            + baked.Classifications.Map((row, index) =>
                ElementColumns.Classification.Emit(FactScope.Root.Then("Element.Classification", index), row)).Bind(identity)
            + baked.Representations.ByIdentifier.Pairs.Map(pair => new ElementFact(
                FactScope.Root.Then("Element.Representation").Row(pair.Key.Token), new FactValue.Text(pair.Value.ToString())))
            + ElementColumns.Census.Emit(FactScope.Root, new ComponentCensus(baked, topology, connections))
            + baked.Materials.Bind(MaterialRows)
            + baked.Quantities.Bind(QuantityRows)
            + baked.Properties.Bind(PropertyRows)
            + MaterialFallback(baked)
            + topology.Map(RelationRows).Bind(identity);

        Seq<(PropertyName Path, Seq<ElementFact> Rows)> grouped = toSeq(rows.GroupBy(static row => row.Path))
            .Map(static group => (Path: group.Key, Rows: toSeq(group)));
        Seq<Validation<Error, Unit>> conflicts = grouped
            .Choose(group => group.Rows.ForAll(row => group.Rows.ForAll(other => row.Equivalent(other, tolerance)))
                ? None
                : Some(Fin.Fail<Unit>(Translation(
                    LocusOf(baked.Id, group.Path, tolerance), "element:fact-conflict")).ToValidation()))
            + baked.Properties.Bind(static bag => bag.Values.Pairs)
                .Choose(pair => pair.Key.Value == ElementColumns.MaterialRow && pair.Value is not PropertyValue.Text
                    ? Some(Fin.Fail<Unit>(Translation(LocusOf(baked.Id,
                        FactScope.Root.Row(ElementColumns.MaterialRow), tolerance), "element:material-row")).ToValidation())
                    : None);

        return conflicts.Traverse(static conflict => conflict)
            .As()
            .ToFin()
            .Bind(_ => ElementFactSet.Admit(grouped.Choose(static group => group.Rows.Head)));
    }

    private static UInt128 LocusOf(NodeId id, PropertyName path, double tolerance) =>
        FabricationCanon.Ordered(tolerance, writer => writer.String(id.Value).String(path.Value));

    private static Seq<ElementFact> MaterialRows(BakedMaterial material) {
        string key = material.Material.MaterialKey.Value;
        FactScope root = FactScope.Root.Then("Material").Then(key);
        FactScope composition = root.Then("Composition");
        return material.Material.Composition.Switch(
            state: composition,
            single: static (scope, row) => Kind(scope, nameof(MaterialComposition.Single))
                + ElementColumns.Single.Emit(scope, row),
            layerSet: static (scope, row) => Kind(scope, nameof(MaterialComposition.LayerSet))
                + row.Layers.Map((layer, index) => ElementColumns.Layer.Emit(scope.Then("Layer", index), layer)).Bind(identity),
            profileSet: static (scope, row) => Kind(scope, nameof(MaterialComposition.ProfileSet))
                + ElementColumns.ProfileSet.Emit(scope, row),
            constituentSet: static (scope, row) => Kind(scope, nameof(MaterialComposition.ConstituentSet))
                + row.Constituents.Map((constituent, index) =>
                    ElementColumns.Constituent.Emit(scope.Then("Constituent", index), constituent)).Bind(identity))
            + material.Material.Properties.Bind(property => PropertySetRows(root, property))
            + SectionRows(root, material.Material.Composition)
            + UsageRows(FactScope.Root.Then("Usage").Then(key), material.Usage);
    }

    private static Seq<ElementFact> PropertySetRows(FactScope root, MaterialPropertySet property) {
        FactScope scope = root.Then(property.Map(
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
            optical: nameof(MaterialPropertySet.Optical)));
        return ElementColumns.Evidence.Emit(scope, property.Evidence) + property.Switch(
            state: scope,
            mechanical: static (at, row) => ElementColumns.Mechanical.Emit(at, row),
            orthotropic: static (at, row) => ElementColumns.Orthotropic.Emit(at, row),
            thermal: static (at, row) => ElementColumns.Thermal.Emit(at, row),
            acoustic: static (at, row) => ElementColumns.Acoustic.Emit(at, row)
                + toSeq(AcousticBand.Items).Map(band => ElementColumns.Band
                    .Fold(at.Then(band.CenterHz.ToString(CultureInfo.InvariantCulture)), new AcousticReading(row, band)))
                    .Bind(identity),
            fire: static (at, row) => ElementColumns.Fire.Emit(at, row),
            environmental: static (at, row) => ElementColumns.Environmental.Emit(at, row)
                + toSeq(ImpactCategory.Items).Bind(category => toSeq(LifecycleStage.Items).Map(stage => ElementColumns.Impact
                    .Fold(at.Then("Impact").Then(category.Name).Then(stage.Module), new ImpactReading(row, category, stage))))
                    .Bind(identity),
            cost: static (at, row) => ElementColumns.Cost.Emit(at, row),
            damping: static (at, row) => ElementColumns.Damping.Emit(at, row),
            hygrothermal: static (at, row) => ElementColumns.Hygrothermal.Emit(at, row)
                + CurveRows(at.Then("SorptionIsotherm"), row.SorptionIsotherm)
                + CurveRows(at.Then("LiquidTransport"), row.LiquidTransport)
                + CurveRows(at.Then("MoistureConductivity"), row.MoistureConductivity),
            durability: static (at, row) => ElementColumns.Durability.Emit(at, row),
            optical: static (at, row) => ElementColumns.Optical.Emit(at, row));
    }

    private static Seq<ElementFact> SectionRows(FactScope root, MaterialComposition composition) =>
        composition is MaterialComposition.ProfileSet profile
            ? profile.Section.Map(section => ElementColumns.Section.Emit(root.Then("Section"), section)).IfNone(Seq<ElementFact>())
            : Seq<ElementFact>();

    private static Seq<ElementFact> UsageRows(FactScope scope, MaterialUsage usage) => usage.Switch(
        state: scope,
        unbound: static (at, _) => Kind(at, nameof(MaterialUsage.Unbound)),
        layerSet: static (at, row) => Kind(at, nameof(MaterialUsage.LayerSet)) + ElementColumns.LayerUsage.Emit(at, row),
        profileSet: static (at, row) => Kind(at, nameof(MaterialUsage.ProfileSet)) + ElementColumns.ProfileUsage.Emit(at, row));

    private static Seq<ElementFact> QuantityRows(QuantityBag bag) {
        FactScope scope = FactScope.Root.Then("Quantity").Then(bag.SetName);
        return ElementColumns.Bag.Emit(scope, bag)
            + bag.Values.Pairs.Map(pair => new ElementFact(scope.Row(pair.Key.Value), new FactValue.Number(pair.Value.Si)));
    }

    private static Seq<ElementFact> PropertyRows(PropertyBag bag) {
        FactScope scope = FactScope.Root.Then("Property").Then(bag.SetName);
        return ElementColumns.Bag.Emit(scope, bag)
            + bag.Values.Pairs.Map(pair => new ElementFact(scope.Row(pair.Key.Value), new FactValue.Typed(pair.Value)));
    }

    private static Seq<ElementFact> MaterialFallback(Element baked) {
        Seq<string> candidates = baked.Materials.Map(static row => row.Material.MaterialKey.Value).Distinct();
        Option<string> elected = baked.Properties
            .Bind(static bag => bag.Values.Pairs)
            .Choose(static pair => pair.Key.Value == ElementColumns.MaterialRow && pair.Value is PropertyValue.Text text
                ? Some(text.Value)
                : None)
            .Head
            | candidates.Head.Filter(_ => candidates.Count == 1);
        return elected
            .Map(static value => Seq<ElementFact>(new ElementFact(
                FactScope.Root.Row(ElementColumns.MaterialRow), new FactValue.Text(value))))
            .IfNone(Seq<ElementFact>());
    }

    private static Seq<ElementFact> RelationRows(Relationship relation, int index) {
        FactScope scope = FactScope.Root.Then("Relation", index);
        return relation.Switch(
            state: scope,
            compose: static (at, row) => Kind(at, nameof(Relationship.Compose)) + ElementColumns.Compose.Emit(at, row),
            assign: static (at, row) => Kind(at, nameof(Relationship.Assign)) + ElementColumns.Assign.Emit(at, row),
            associate: static (at, row) => Kind(at, nameof(Relationship.Associate))
                + ElementColumns.Associate.Emit(at, row) + UsageRows(at.Then("Usage"), row.Usage),
            connect: static (at, row) => Kind(at, nameof(Relationship.Connect)) + ElementColumns.Connect.Emit(at, row),
            @void: static (at, row) => Kind(at, nameof(Relationship.Void)) + ElementColumns.Opening.Emit(at, row),
            generic: static (at, row) => Kind(at, nameof(Relationship.Generic)) + ElementColumns.Generic.Emit(at, row)
                + row.Attributes.Pairs.Map(pair => new ElementFact(
                    at.Then("Attribute").Row(pair.Key.Value), new FactValue.Typed(pair.Value)))
                + row.Participants.Map((participant, ordinal) =>
                    ElementColumns.Participant.Emit(at.Then("Participant", ordinal), participant)).Bind(identity));
    }

    private static Seq<ElementFact> CurveRows(FactScope scope, Option<SampledCurve> curve) =>
        curve.Map(value => toSeq(value.Axis).Zip(toSeq(value.Values))
            .Map((pair, index) => ElementColumns.Sample.Emit(scope.Then(index.ToString(CultureInfo.InvariantCulture)),
                new CurveSample(pair.Item1, pair.Item2)))
            .Bind(identity)).IfNone(Seq<ElementFact>());

    private static Seq<ElementFact> Kind(FactScope scope, string discriminant) =>
        Seq<ElementFact>(new ElementFact(scope.Row("Kind"), new FactValue.Text(discriminant)));

    private static Fin<ReadOnlyMemory<byte>> CanonicalProperties(ElementGraph graph, Element baked, Op key) {
        CanonicalWriter writer = CanonicalWriter.Retaining(graph.Header.Tolerance);
        Seq<PropertyBag> bags = toSeq(baked.Properties.OrderBy(static bag => bag.SetName, StringComparer.Ordinal));
        writer.Ordinal(bags.Count);
        foreach (PropertyBag bag in bags) {
            writer.String(bag.SetName).Ordinal(bag.Values.Count);
            foreach ((PropertyName name, PropertyValue value) in bag.Values.Pairs.OrderBy(static pair => pair.Key.Value, StringComparer.Ordinal)) {
                writer.String(name.Value);
                value.CanonicalBytes(writer);
            }
        }
        return writer.ToBytes(key);
    }

    private static Error Translation(UInt128 locus, string detail) =>
        FabricationFault.Sourced(new SourceLocus.ElementNode(locus), detail);
}
```

## [05]-[RESEARCH]

<!-- source-only: research row template:
[TOKEN]-[OPEN|BLOCKED]: <exact question>; <verification route>.
[SPLIT_MEMBER]-[OPEN]: does `shape-core` expose `split_all`; verify against the member rail.
-->

(none)
