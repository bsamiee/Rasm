# [BIM_IFC_TAXONOMY]

The IFC entity-class vocabulary `Rasm.Bim` owns as the SOLE GeometryGym/IFC owner — GENERATED, never hand-rostered: the `IfcClass` `[SmartEnum<string>]` row region is the committed output of the offline `IfcVocabularyEmitter` reflection pass over the GeometryGymIFC_Core attribute surface — every `IfcObjectDefinition`-rooted entity (474 at pin `25.7.30`, the full buildingSMART breadth superseding the retired 73-row hand slice), each row carrying its `IfcDomain` discipline, its seam `SchemaSpan` class window, its `Instantiable` schema-abstract flag, and its `Seq<PredefinedRow>` valid-token set with PER-TOKEN `IntroducedIn` spans [H8]. Enforcement is ONE authority behind three gates: Gate 0 at emit time (the Materials `IfcBinding` stamp audit — an orphan `(Entity, Predefined)` seed pair FAILS the emit, so a typo'd stamp dies before any graph exists; Materials never references `Rasm.Bim`), Gate 1 at composition time (the `Projection/semantic#GRAPH_LEGALITY` `IfcLegality : IGraphConstraint` vocabulary arms over the delta's `AddedNodes` — no new interface, no new constraint class), Gate 2 at egress (`AdmitPredefined` validating the class window AND the per-token span — `WAVEWALL` on an IFC2x3 emit faults — beside the `Projection/egress#IFC_EGRESS` `Instantiable` check). `ReleaseMap`, owned HERE, is the ONE GG-to-seam release currency the emitter and the runtime `ReleaseLower`/`ReleaseRaise` lowerings share — an unmapped member FAILS the emit and rails `BimFault.CodecReject` at runtime, retiring both silent fallbacks. The retired `BimElement` element record and `BimModel` collection stay GONE: the consumer-facing element is the seam `Bake(objectNode)` fold over the reachable `ElementGraph` subgraph, never a second stored record keyed by `GlobalId`. Growth inverts the hand-row economy: a new IFC entity class is one emitter regeneration plus at most one overlay row — never a hand row, never three parallel surfaces, never a per-element-class type.

## [01]-[INDEX]

- [01]-[IFC_CLASS]: `IfcClass` the generated `[SmartEnum<string>]` entity-class vocabulary — the `IfcDomain` discipline partition, the seam `SchemaSpan` class window [H8], the `Instantiable` schema-abstract flag, the `PredefinedRow` per-token spans, the `Resolve` ingress lookup, the per-token `AdmitPredefined` egress gate over the seam-owned `PredefinedType` value-object [PREDEFINED_TOKEN_RULING], and the `AuditTarget` whole-model schema-retarget preflight accumulating the complete violation set over `Validation<Error, Unit>`.
- [02]-[TAXONOMY_EMITTER]: `IfcVocabularyEmitter` the offline reflection emitter committing the `IfcClass` row region — the `ReleaseMap` GG-to-seam release table, the `VocabularyRow` row currency, the QuikGraph inheritance-DAG `DomainAtlas`, the overlay tiers (`ClassIntroductions` with dotted token pins, `AbstractSupertypes`, `Retirements`, `DomainRoots`+`DomainOverrides`), and the Gate 0 Materials stamp audit.
- [03]-[REPRESENTATION_KEYS]: `IfcRepresentation` the geometry-reference content-keyer projecting an `IfcProduct`/`IfcTypeProduct` representation set onto the seam `RepresentationContentHash` keyed map (axis/body/box/footprint → kernel `XxHash128` content hash) [M2], composing the kernel seed-zero `ContentHash.Of` identity, never a second hasher.

## [02]-[IFC_CLASS]

- Owner: `IfcClass` the generated `[SmartEnum<string>]` closed buildingSMART entity-class vocabulary keyed on the IFC entity-type string — the committed emitter output, regenerated on GeometryGym pin bumps, hand edits landing ONLY in the emitter overlays. Each row carries its `IfcDomain` discipline, its seam-owned `SchemaSpan` class window (`Graph/element#NODE_MODEL` — the SAME window record the projector stamps onto a node at ingress, never a parallel Bim copy) [H8], its `Instantiable` flag (schema-abstract supertypes such as `IfcBuiltElement`/`IfcDistributionElement` are FIRST-CLASS rows flagged `Instantiable: false` — legal CLASSIFICATION vocabulary the Materials profiled families stamp by design, illegal EGRESS classes), and its `Seq<PredefinedRow>` valid-token set where each token carries its OWN `IntroducedIn` release [H8] — so the availability axis is two-tier: the class window gates the entity, the token span gates the sub-kind. `IfcSchema` owns only the IFC release chronology (the seam `[SmartEnum]` declaration order is not chronological), the `SchemaSpan.Covers` gate the class window and the stamped node span share, and the `Rank` the per-token gate reads. The `PredefinedType` sub-class discriminant is the seam-carried `PredefinedType` `[ValueObject<string>]` on the `Object` node, of which `IfcClass` is the IFC validation authority — the semantics live HERE (the generated token sets + `AdmitPredefined`), the seam node carrying only the typed token value-object [PREDEFINED_TOKEN_RULING].
- Entry: `IfcClass.Resolve(string entityType, Op key)` is the strict ingress lookup the projector composes — it `Canonical`-folds the deprecated `*StandardCase`/`*ElementedCase` subtypes onto their base row, then resolves the IFC entity-type string (the `ParserIfc.IdentifyIfcClass` class half) to the row that supplies the generic `Classification("ifc", row.Key)` stamped on the seam `Object` node, faulting `Model/faults#FAULT_BAND` `BimFault.UnmappedClass` on a class the roster does not carry — the typed case (band 2600, `Expected`-derived) lifting BARE onto the `Fin` rail with no `.ToError()` hop; a projector that prefers a permissive ingress instead reads `IfcClass.TryGet(entityType).IfNone(IfcClass.BuildingElementProxy)` so an unrostered future-schema leaf lands the proxy row rather than aborting the import, the two paths sharing the ONE `TryGet` Option-lift over the generated bool/out try-pattern and never a parallel resolver — the full reflected roster shrinks that fallback to genuinely-foreign names. `IfcClass.AdmitPredefined(string token, string objectType, ReleaseVersion schema, Op key)` is the egress gate — it admits the predefined token against the row's generated token set AND the token's own `IntroducedIn` span AND the class window against the target `Header` schema, returning the validated IFC predefined token the projector authors or faulting `BimFault.UnmappedClass`. `IfcClass.AuditTarget(ElementGraph graph, ReleaseVersion schema, Op key)` is the whole-model schema-retarget PREFLIGHT — one accumulating `Validation<Error, Unit>` pass folding every `"ifc"`-classified node's occurrence-instantiability applicatively with the SAME per-token `AdmitPredefined` gate against a caller-chosen target schema, so a federation deliverable decision (an IFC4.3 authoring model owed as an IFC2x3 coordination deliverable) reads the COMPLETE typed violation set before the short-circuiting `Projection/egress#IFC_EGRESS` `Emit` surfaces only the first.
- Auto: `Resolve` reads the `Items` table by key through the generated `TryGet`; the projector folds its result into the generic `Classification` value-object so the seam node carries a `(system, code)` pair (`"ifc"`, `"IfcWall"`) rather than the `IfcClass` type itself, keeping the seam IFC-schema-free; `AdmitPredefined` trims and upper-cases the token, routes `""`/`"NOTDEFINED"` to the canonical `PredefinedType.NotDefined.Token`, routes `"USERDEFINED"` to the validated `USERDEFINED` marker requiring a non-empty `objectType` label (the projector authors the IFC `ObjectType` from it — there is no `OBJECTTYPE` token), accepts any token when the row's set is empty (the schema not constraining it — 147 of the 474 rows declare no own predefined enum), and otherwise gates PER-TOKEN: a set member passes only when `IfcSchema.Rank(schema) >= IfcSchema.Rank(row.IntroducedIn)` — matching GeometryGym's own `validPredefinedType(value, release)` setter guard — so `WAVEWALL` against an IFC2x3 emit faults `predefined-out-of-schema` where the retired class-level gate wrongly passed it [H8]; a non-member faults `predefined-reject`; the class-window check compares the seam `ReleaseVersion` the `Header` carries through the frozen chronological rank because the `[SmartEnum]` carries no ordinal; the admitted token folds into the seam node content hash through the seam `Node.ToCanonicalBytes` [PREDEFINED_TOKEN_RULING].
- Packages: GeometryGymIFC_Core, `Rasm` (the kernel `Op` operation key the `Resolve`/`AdmitPredefined` faults carry), Rasm.Element, Thinktecture.Runtime.Extensions, LanguageExt.Core
- Growth: a new entity class, sub-kind token, or schema availability is one emitter REGENERATION — the committed-region diff — plus at most one overlay row (`ClassIntroductions` when GG leaves the class unattributed, a dotted token pin when GG leaves a grown token unattributed); a new IFC release is one `ReleaseMap` row plus one `Chronology` rank; never a hand row in the generated region, never a per-element-class type, never a `Get<Domain>` family, and never the retired `BimElement` record.
- Boundary: `BimElement` and `BimModel` are RETIRED — an owner that re-stores a `BimElement(GlobalId, IfcClass, …)` record off the seam graph is the deleted form; `IfcClass` is the IFC-schema vocabulary the projector composes, NOT a field on a seam node — the seam `Object` node carries the generic `Classification("ifc", code)` value-object and a typed `IfcClass` on the node is the named seam violation [PREDEFINED_TOKEN_RULING]; a hand edit inside the generated-rows region is the named defect (it dies at the next regeneration — the overlays are the sole hand surface); an `Instantiable: false` row is legal classification vocabulary and an illegal egress class — the `Projection/egress#IFC_EGRESS` gate faults it `abstract-class-at-egress`, and sourcing the flag from `!Type.IsAbstract` alone is the named defect (GG declares schema-abstract supertypes as concrete C# classes); the predefined validity is an EGRESS gate (validated when the IFC entity is authored, against the token set + the per-token span + the class window [PREDEFINED_TOKEN_RULING][H8]) and silent acceptance of an out-of-schema token is the named defect; the schema currency is the SEAM `ReleaseVersion` ranked through the frozen chronology and a bare `>=` over the SmartEnum or a GeometryGym `ReleaseVersion` leak into the gate signature is the named defect — an unranked seam release fails `IfcSchema` type initialization (the `FaultBand` registry pattern), never a silent rank sentinel; a raw entity-type string crossing a seam signature is the named defect.

```csharp signature
// --- [RUNTIME_PRELUDE] --------------------------------------------------------------------
using System.Collections.Frozen;
using GeometryGym.Ifc;
using LanguageExt;
using Rasm.Element.Classification;
using Rasm.Element.Graph;
using Thinktecture;
using static LanguageExt.Prelude;
using Op = Rasm.Domain.Op;                            // the kernel operation key each typed BimFault case carries
using ReleaseVersion = Rasm.Element.Graph.ReleaseVersion;   // the seam schema currency the Header carries — disambiguated
                                                      // from GeometryGym.Ifc.ReleaseVersion, which rides the GGRelease
                                                      // alias on the [TAXONOMY_EMITTER] and IFC-text codec legs only.

namespace Rasm.Bim.Model;

// --- [TYPES] ------------------------------------------------------------------------------
// The discipline partition over the FULL IfcObjectDefinition closure the emitter commits — the buildingSMART schema
// domains folded to query-grade rows. The element disciplines alone cannot legalize the reflected roster: General owns
// the kernel/spatial/actor/group backbone (IfcProject/IfcSite/IfcGroup/IfcActor), Controls the building-controls branch
// (the IfcDistributionControlElement sensors/actuators/controllers line), Construction the process/resource branch
// (IfcProcess/IfcConstructionResource); a roster member no DomainRoots row reaches still FAILS the emit.
[SmartEnum<string>]
public sealed partial class IfcDomain {
    public static readonly IfcDomain Architecture   = new("Architecture");
    public static readonly IfcDomain Structural     = new("Structural");
    public static readonly IfcDomain HvacFire       = new("HvacFire");
    public static readonly IfcDomain Electrical     = new("Electrical");
    public static readonly IfcDomain Plumbing       = new("Plumbing");
    public static readonly IfcDomain Infrastructure = new("Infrastructure");
    public static readonly IfcDomain Geotechnical   = new("Geotechnical");
    public static readonly IfcDomain General        = new("General");
    public static readonly IfcDomain Controls       = new("Controls");
    public static readonly IfcDomain Construction   = new("Construction");
}

// --- [TABLES] -----------------------------------------------------------------------------
// The per-class availability window is the SEAM-owned SchemaSpan (Graph/element#NODE_MODEL): the
// row's class window and the stamped node span are ONE type — never a parallel Bim copy [H8]. Bim
// owns only the release chronology the seam [SmartEnum] declaration order does not encode, the
// Covers gate both windows share, and the Rank the per-token AdmitPredefined gate reads — internal,
// never private, because the token gate ranks OUTSIDE this class. The gate ranks chronologically
// (the SmartEnum has no ordinal), never bare `>=`.
internal static class IfcSchema {
    // Anchor names EQUAL the seam ReleaseVersion keys so Render emits `IfcSchema.{row.IntroducedIn.Key}` symbolically
    // (SYMBOLIC_REFERENCE) — a case-drifted anchor name would force a render-time name map.
    public static readonly SchemaSpan Ifc2X3     = SchemaSpan.From(ReleaseVersion.Ifc2X3);
    public static readonly SchemaSpan Ifc4       = SchemaSpan.From(ReleaseVersion.Ifc4);
    public static readonly SchemaSpan Ifc4X1     = SchemaSpan.From(ReleaseVersion.Ifc4X1);
    public static readonly SchemaSpan Ifc4X3     = SchemaSpan.From(ReleaseVersion.Ifc4X3);
    public static readonly SchemaSpan Ifc4X3Add2 = SchemaSpan.From(ReleaseVersion.Ifc4X3Add2);

    private static readonly FrozenDictionary<string, int> Chronology = new Dictionary<string, int> {
        [ReleaseVersion.Ifc2X3.Key] = 0, [ReleaseVersion.Ifc4.Key] = 1, [ReleaseVersion.Ifc4X1.Key] = 2,
        [ReleaseVersion.Ifc4X3.Key] = 3, [ReleaseVersion.Ifc4X3Add2.Key] = 4, [ReleaseVersion.Ifc5.Key] = 5,
    }.ToFrozenDictionary();

    // Rank completeness is type-init law (the FaultBand registry pattern): a seam ReleaseVersion
    // row without a Chronology rank dies HERE at first touch — never a silent rank sentinel that
    // flips Covers (an unranked schema would pass every open window and fail every closed one).
    static IfcSchema() {
        Seq<string> unranked = toSeq(ReleaseVersion.Items).Filter(static v => !Chronology.ContainsKey(v.Key)).Map(static v => v.Key);
        if (!unranked.IsEmpty) throw new InvalidOperationException($"<chronology-unranked:{string.Join(',', unranked)}>");
    }

    internal static int Rank(ReleaseVersion value) => Chronology[value.Key];

    extension(SchemaSpan span) {
        public bool Covers(ReleaseVersion schema) =>
            Rank(schema) >= Rank(span.IntroducedIn) && span.RemovedIn.Match(Some: removed => Rank(schema) < Rank(removed), None: () => true);
    }
}

// --- [MODELS] -----------------------------------------------------------------------------
// The per-token availability row [H8]: IntroducedIn is the SEAM ReleaseVersion — the one schema
// currency — sourced from the per-member GG [VersionAdded] lowered through ReleaseMap; an
// unattributed token inherits its class IntroducedIn; a dotted ClassIntroductions overlay row
// pins a verified GG attribution gap (IfcWall.PARAPET).
public readonly record struct PredefinedRow(string Token, ReleaseVersion IntroducedIn);

[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinalIgnoreCase, string>]
[KeyMemberComparer<ComparerAccessors.StringOrdinalIgnoreCase, string>]
public sealed partial class IfcClass {
    // <generated-rows>
    public static readonly IfcClass BuiltElement = new("IfcBuiltElement", IfcDomain.Architecture, IfcSchema.Ifc4, instantiable: false, Seq<PredefinedRow>());   // AbstractSupertypes overlay: CLR-concrete, schema-abstract
    public static readonly IfcClass Wall = new("IfcWall", IfcDomain.Architecture, IfcSchema.Ifc2X3, instantiable: true, Seq(
        new PredefinedRow("ELEMENTEDWALL", ReleaseVersion.Ifc4), new PredefinedRow("MOVABLE", ReleaseVersion.Ifc2X3),
        new PredefinedRow("PARAPET", ReleaseVersion.Ifc4X3),           // dotted overlay pin — GG leaves the IFC4X3-grown token unattributed
        new PredefinedRow("PARTITIONING", ReleaseVersion.Ifc2X3), new PredefinedRow("PLUMBINGWALL", ReleaseVersion.Ifc2X3),
        new PredefinedRow("POLYGONAL", ReleaseVersion.Ifc4), new PredefinedRow("RETAININGWALL", ReleaseVersion.Ifc4X1),
        new PredefinedRow("SHEAR", ReleaseVersion.Ifc2X3), new PredefinedRow("SOLIDWALL", ReleaseVersion.Ifc4),
        new PredefinedRow("STANDARD", ReleaseVersion.Ifc4), new PredefinedRow("WAVEWALL", ReleaseVersion.Ifc4X3)));
    public static readonly IfcClass Bridge = new("IfcBridge", IfcDomain.Infrastructure, IfcSchema.Ifc4X1, instantiable: true, Seq(   // class-level [VersionAdded(IFC4X2)] → seam Ifc4X1; class-birth tokens inherit it
        new PredefinedRow("ARCHED", ReleaseVersion.Ifc4X1), new PredefinedRow("CABLE_STAYED", ReleaseVersion.Ifc4X1),
        new PredefinedRow("CANTILEVER", ReleaseVersion.Ifc4X1), new PredefinedRow("CULVERT", ReleaseVersion.Ifc4X1),
        new PredefinedRow("FRAMEWORK", ReleaseVersion.Ifc4X1), new PredefinedRow("GIRDER", ReleaseVersion.Ifc4X1),
        new PredefinedRow("SUSPENSION", ReleaseVersion.Ifc4X1), new PredefinedRow("TRUSS", ReleaseVersion.Ifc4X1)));
    public static readonly IfcClass WallStandardCase = new("IfcWallStandardCase", IfcDomain.Architecture, new SchemaSpan(ReleaseVersion.Ifc2X3, Some(ReleaseVersion.Ifc4X3)), instantiable: false, Seq<PredefinedRow>());   // Retirements overlay: Canonical folds its ingress onto Wall; the closed window rejects its egress
    public static readonly IfcClass BuildingElementProxy = new("IfcBuildingElementProxy", IfcDomain.Architecture, IfcSchema.Ifc2X3, instantiable: true, Seq(   // the permissive-ingress fallback ROW the TryGet Option-lift names — committed in the sample slice
        new PredefinedRow("COMPLEX", ReleaseVersion.Ifc2X3), new PredefinedRow("ELEMENT", ReleaseVersion.Ifc2X3),
        new PredefinedRow("PARTIAL", ReleaseVersion.Ifc2X3), new PredefinedRow("PROVISIONFORSPACE", ReleaseVersion.Ifc4),
        new PredefinedRow("PROVISIONFORVOID", ReleaseVersion.Ifc4)));
    // … every remaining IfcObjectDefinition-rooted entity, one generated row each …
    // <end generated-rows>

    public IfcDomain Domain { get; }
    public SchemaSpan Span { get; }
    public bool Instantiable { get; }
    public Seq<PredefinedRow> ValidPredefined { get; }

    // The Option-lift over the generated bool/out try-pattern — the ONE lookup spelling every consumer folds
    // (Resolve, the permissive ingress, the Gate-1 vocabulary arm, the ByDomain query read); the generated
    // TryGet(string?, out IfcClass?) stays the raw seam beneath it, never a second resolver.
    public static Option<IfcClass> TryGet(string entityType) =>
        TryGet(entityType, out IfcClass? row) && row is { } hit ? Some(hit) : None;

    // The strict ingress lookup the projector composes: entity-type string -> the row supplying the generic
    // Classification("ifc", Key) the seam Object node carries, Canonical folding the deprecated *StandardCase/
    // *ElementedCase subtypes onto their base row first. The typed UnmappedClass case lifts BARE onto the Fin rail
    // (band 2600 IS the Expected Code; no .ToError() hop) on a class the roster omits; a permissive ingress instead
    // reads TryGet(entityType).IfNone(BuildingElementProxy) through the ONE Option-lift (no 2nd resolver).
    public static Fin<IfcClass> Resolve(string entityType, Op key) =>
        TryGet(Canonical(entityType)).ToFin(new BimFault.UnmappedClass(key, $"element-class-miss:{entityType}"));

    // IFC4 collapsed the deprecated *StandardCase/*ElementedCase implementation subtypes (IfcWallStandardCase,
    // IfcSlabElementedCase, …) into the base class + PredefinedType; ParserIfc.IdentifyIfcClass does NOT fold them,
    // so a 2x3 IfcWallStandardCase the projector reads off p.GetType().Name resolves the IfcWall row here (its
    // predefined token rides AdmitPredefined separately) rather than aborting the import on the most common real
    // entity. The retired subtypes still hold committed rows whose CLOSED SchemaSpan rejects them at egress.
    private static string Canonical(string entityType) =>
        entityType.EndsWith("StandardCase", StringComparison.Ordinal)    ? entityType[..^"StandardCase".Length]
        : entityType.EndsWith("ElementedCase", StringComparison.Ordinal) ? entityType[..^"ElementedCase".Length]
        : entityType;

    // The egress gate, PER-TOKEN [PREDEFINED_TOKEN_RULING][H8]: the class window gates the entity against the target Header schema, then
    // each set member gates on ITS OWN IntroducedIn rank — WAVEWALL on an IFC2x3 emit faults predefined-out-of-schema,
    // matching GeometryGym's own validPredefinedType(value, release) setter guard. USERDEFINED requires a non-empty
    // ObjectType label (the projector authors ObjectType=objectType); an empty set constrains nothing. The schema
    // currency is the SEAM ReleaseVersion, ranked chronologically because the SmartEnum has no ordinal.
    public Fin<string> AdmitPredefined(string token, string objectType, ReleaseVersion schema, Op key) =>
        !Span.Covers(schema)
            ? Fin.Fail<string>(new BimFault.UnmappedClass(key, $"class-out-of-schema:{Key}:{schema.Key}"))
            : token.Trim().ToUpperInvariant() switch {
                "" or "NOTDEFINED" => Fin.Succ(PredefinedType.NotDefined.Token),
                "USERDEFINED"      => objectType.Trim() is { Length: > 0 }
                                          ? Fin.Succ("USERDEFINED")
                                          : Fin.Fail<string>(new BimFault.UnmappedClass(key, $"predefined-objecttype-miss:{Key}")),
                var value when ValidPredefined.IsEmpty => Fin.Succ(value),
                var value => ValidPredefined.Find(row => row.Token == value).Match(
                    Some: row => IfcSchema.Rank(schema) >= IfcSchema.Rank(row.IntroducedIn)
                        ? Fin.Succ(value)
                        : Fin.Fail<string>(new BimFault.UnmappedClass(key, $"predefined-out-of-schema:{Key}:{value}:{schema.Key}")),
                    None: () => Fin.Fail<string>(new BimFault.UnmappedClass(key, $"predefined-reject:{Key}:{value}"))),
            };

    // The whole-model schema-retarget PREFLIGHT: every "ifc"-classified node audited against a caller-chosen
    // target ReleaseVersion in ONE accumulating pass — the Instantiable-at-egress occurrence check fanned in
    // through the tuple `.Apply` with the SAME per-token AdmitPredefined gate the Emit egress runs (the node Name
    // standing as the USERDEFINED ObjectType label exactly as egress authors it) — so a federation manager reads
    // the COMPLETE typed violation set (class-out-of-schema, predefined-out-of-schema, abstract-at-egress,
    // retired-window) BEFORE any emit, where the short-circuiting Emit TraverseM surfaces only the first fault.
    // An unrostered code is itself a retarget violation: the preflight is complete evidence, never a filter that
    // silently removes the one class whose target-schema status cannot be decided.
    public static Validation<Error, Unit> AuditTarget(ElementGraph graph, ReleaseVersion schema, Op key) =>
        graph.ObjectNodes
            .Filter(static o => o.Classification.System == "ifc")
            .Traverse(o => TryGet(Canonical(o.Classification.Code)).Match(
                None: () => Fail<Error, Unit>(new BimFault.UnmappedClass(
                    key, $"schema-retarget-class-miss:{o.Classification.Code}:{o.ExternalId.IfNone(o.Id.Value)}")),
                Some: cls => (
                    !cls.Instantiable && o.Kind == ObjectKind.Occurrence
                        ? Fail<Error, Unit>(new BimFault.UnmappedClass(key, $"abstract-class-at-egress:{cls.Key}:{o.ExternalId.IfNone(o.Id.Value)}"))
                        : Success<Error, Unit>(unit),
                    cls.AdmitPredefined(o.PredefinedType.Token, o.Name, schema, key).ToValidation())
                    .Apply(static (_, _) => unit).As()))
            .As().Map(static _ => unit);
}
```

## [03]-[TAXONOMY_EMITTER]

- Owner: `IfcVocabularyEmitter` the OFFLINE BCL-reflection emitter — a repo tool run on GeometryGym pin bumps whose output is the committed `IfcClass` row region; the committed table is the system of record, so there is no runtime reflection, no Roslyn generator, and no sidecar file. `ReleaseMap` the ONE GG-to-seam release table (the `Lower` fold and its exact-name-preimage `Raise` inverse) the emitter AND the runtime `Projection/semantic#SEMANTIC_PROJECTOR` `ReleaseLower` / `Projection/egress#IFC_EGRESS` `ReleaseRaise` lowerings share. `VocabularyRow` the emitter row currency one render line commits. The overlays are hand-curated FROZEN data — `ClassIntroductions` (class rows for the unattributed IFC2x3-era floor, seeded once from the 73 retired hand rows' spans, PLUS dotted `"IfcWall.PARAPET"` token rows pinning verified GG attribution gaps — one dictionary, the key shape discriminating the tier), `AbstractSupertypes` (the schema-abstract set the CLR flag misreports), `Retirements` (the `*StandardCase`/`*ElementedCase` closed windows — no `VersionRemoved` attribute exists), and `DomainRoots`+`DomainOverrides` (the hierarchy-root discipline claims spanning the WHOLE `IfcObjectDefinition` closure — the element disciplines plus the `General`/`Controls`/`Construction` backbone roots — plus the per-entity re-pins).
- Entry: `IfcVocabularyEmitter.Emit(Assembly gg, FrozenDictionary<string, ReleaseVersion> classIntroductions, FrozenSet<string> abstractSupertypes, FrozenDictionary<string, ReleaseVersion> retirements, FrozenDictionary<string, IfcDomain> domainRoots, FrozenDictionary<string, IfcDomain> domainOverrides, FrozenSet<(string Entity, string Predefined)> materialsStamps, Op key)` → `Fin<string>` — one entrypoint owning the whole regeneration: reflect the roster, resolve disciplines, source spans, audit, render.
- Auto: `Emit` reflects the `IfcObjectDefinition` closure through `Try.lift`, resolves domain inheritance through one QuikGraph DAG, lowers class and token release attributes through `ReleaseMap`, applies explicit attribution and schema-abstraction overlays, audits orphaned stamps and identifier collisions, then renders deterministic declarations under stable generated-region markers. The committed rows carry capability; the marker carries no version or provenance fact.
- Packages: GeometryGymIFC_Core (the reflected attribute surface), QuikGraph (the inheritance DAG), `Rasm` (the kernel `Op` operation key the emitter faults carry), Rasm.Element (the seam `ReleaseVersion` currency), LanguageExt.Core
- Growth: a new IFC release is one `ReleaseMap` row plus one `IfcSchema.Chronology` rank; a new discipline claim is one `DomainRoots` or `DomainOverrides` row; a GG attribution gap is one dotted `ClassIntroductions` row; a newly retired entity is one `Retirements` row — the grouping `IfcElectricalCircuit`/`IfcCondition` `[Obsolete] DEPRECATED IFC4` closed windows are standing rows beside the `*StandardCase`/`*ElementedCase` set, so the `Model/zones#ZONE_GRAPH` grouping overlay derives its retired windows from this one roster; everything else is the regeneration diff.
- Boundary: the emitter never runs at runtime and its output is never hand-edited — the overlays are the sole hand surface; `Instantiable` sourced from `!Type.IsAbstract` ALONE is the named defect (the CLR flag is schema-falsified on the supertypes); `IFC4X4_DRAFT` is excluded by law and any GG member `ReleaseMap.Lower` omits FAILS the emit — at runtime the same miss rails `BimFault.CodecReject`, and the `?? ReleaseVersion.Ifc4X3Add2` / `GGRelease.IFC4X3_ADD2` silent fallbacks are the retired forms; `Raise` derives as `Lower`'s underscore-erased exact-name preimage (`IFC4X3_ADD2` ← `Ifc4X3Add2` — a bare name compare silently drops the Add2 raise) and the seam `Ifc5` therefore has NO GG image — an `Ifc5` egress target is the typed fault, never a default; the stamp audit reads the Materials seed pairs as DATA (Materials never references `Rasm.Bim`); a per-entity hand base-chain walk beside the `DomainAtlas` DAG is the rejected second walk, and the per-root claim is the `BreadthFirstSearchAlgorithm` `DiscoverVertex` event fold — the all-vertex `TryFunc` path-probe sweep the spatial view names deleted is deleted here too.

```csharp signature
// --- [RUNTIME_PRELUDE] --------------------------------------------------------------------
// The OFFLINE emitter leg — BCL reflection over the GG assembly, run on pin bumps, output
// committed above — plus the ONE release table the runtime lowerings share.
using System.Collections.Frozen;
using System.Reflection;
using GeometryGym.Ifc;
using LanguageExt;
using QuikGraph;
using QuikGraph.Algorithms;
using QuikGraph.Algorithms.Search;
using Rasm.Element.Classification;
using Rasm.Element.Graph;
using static LanguageExt.Prelude;
using GGRelease = GeometryGym.Ifc.ReleaseVersion;     // the GeometryGym release enum — emitter + codec legs only
using Op = Rasm.Domain.Op;
using ReleaseVersion = Rasm.Element.Graph.ReleaseVersion;

namespace Rasm.Bim.Model;

// --- [TABLES] ------------------------------------------------------------------------------
// The ONE GG-to-seam release map — owned here, read by the emitter AND the runtime
// ReleaseLower/ReleaseRaise lowerings, retiring both silent fallbacks. IFC4X4_DRAFT is excluded
// by law; an unmapped member FAILS the emit and rails BimFault.CodecReject at runtime. Raise
// derives as Lower's exact-name preimage (IFC2x3 ← Ifc2X3, IFC4 ← Ifc4, …); the seam Ifc5 has no
// GG member, so a Raise miss is the typed egress fault, never a default.
internal static class ReleaseMap {
    public static readonly FrozenDictionary<GGRelease, ReleaseVersion> Lower = new Dictionary<GGRelease, ReleaseVersion> {
        [GGRelease.IFC2X] = ReleaseVersion.Ifc2X3, [GGRelease.IFC2x2] = ReleaseVersion.Ifc2X3, [GGRelease.IFC2x3] = ReleaseVersion.Ifc2X3,
        [GGRelease.IFC4] = ReleaseVersion.Ifc4, [GGRelease.IFC4A1] = ReleaseVersion.Ifc4, [GGRelease.IFC4A2] = ReleaseVersion.Ifc4,
        [GGRelease.IFC4X1] = ReleaseVersion.Ifc4X1, [GGRelease.IFC4X2] = ReleaseVersion.Ifc4X1,
        [GGRelease.IFC4X3_RC1] = ReleaseVersion.Ifc4X3, [GGRelease.IFC4X3_RC2] = ReleaseVersion.Ifc4X3,
        [GGRelease.IFC4X3_RC3] = ReleaseVersion.Ifc4X3, [GGRelease.IFC4X3_RC4] = ReleaseVersion.Ifc4X3,
        [GGRelease.IFC4X3] = ReleaseVersion.Ifc4X3, [GGRelease.IFC4X3_ADD2] = ReleaseVersion.Ifc4X3Add2,
    }.ToFrozenDictionary();

    // The preimage compare is UNDERSCORE-ERASED ordinal-ignore-case: the GG member spells IFC4X3_ADD2 where the seam
    // key spells Ifc4X3Add2, so a bare name compare silently drops the Add2 raise and every Ifc4X3Add2 egress target
    // faults as unmapped; erasing "_" restores the exact preimage while the RC/A-suffixed members stay excluded.
    public static readonly FrozenDictionary<ReleaseVersion, GGRelease> Raise =
        Lower.Where(static pair => string.Equals(pair.Key.ToString().Replace("_", ""), pair.Value.Key, StringComparison.OrdinalIgnoreCase))
             .ToFrozenDictionary(static pair => pair.Value, static pair => pair.Key);
}

// --- [MODELS] ------------------------------------------------------------------------------
// The emitter row currency — one row renders as one committed IfcClass declaration line.
public readonly record struct VocabularyRow(
    string Entity, IfcDomain Domain, ReleaseVersion IntroducedIn, Option<ReleaseVersion> RemovedIn,
    bool Instantiable, Seq<PredefinedRow> Tokens);

// --- [OPERATIONS] --------------------------------------------------------------------------
// Design-time emitter; output commits into the generated region.
public static class IfcVocabularyEmitter {
    public static Fin<string> Emit(
        Assembly gg,
        FrozenDictionary<string, ReleaseVersion> classIntroductions,   // "IfcWall" class rows + dotted "IfcWall.PARAPET" token pins
        FrozenSet<string> abstractSupertypes,
        FrozenDictionary<string, ReleaseVersion> retirements,
        FrozenDictionary<string, IfcDomain> domainRoots,
        FrozenDictionary<string, IfcDomain> domainOverrides,
        FrozenSet<(string Entity, string Predefined)> materialsStamps,
        Op key) =>
        Try.lift(() => gg.GetExportedTypes()
                .Where(static t => typeof(IfcObjectDefinition).IsAssignableFrom(t) && !t.IsGenericType).ToSeq()).Run()
            .MapFail(error => new BimFault.ModelRejected(key, $"gg-reflect:{error.Message}"))
            .Bind(roster => DomainAtlas(roster, domainRoots, domainOverrides, key)
                .Bind(domains => roster.AsIterable()
                    .Traverse(entity => RowOf(entity, domains, classIntroductions, abstractSupertypes, retirements, key)).As()
                    .Bind(rows => Audit(rows.ToSeq(), materialsStamps,
                        toSeq(classIntroductions.Keys) + toSeq(abstractSupertypes) + toSeq(retirements.Keys) + toSeq(domainRoots.Keys) + toSeq(domainOverrides.Keys), key))
                    .Map(rows => Render(rows.OrderBy(static row => row.Entity, StringComparer.Ordinal)))));

    // ONE QuikGraph inheritance DAG replaces N per-entity base-chain walks: edges base → derived
    // over the reflected roster; each DomainRoots row claims its reachability closure in
    // TopologicalSort (base-first) order so the NEAREST (most-derived) root wins by overwrite;
    // DomainOverrides re-pin single entities last — IfcBearing sits under IfcBuiltElement, so its
    // Structural verdict is an override row, the tier the MEP duct/pipe/cable splits already prove
    // load-bearing. The root set spans the WHOLE closure: the element disciplines plus the General
    // backbone roots (IfcSpatialElement/IfcGroup/IfcActor), the Controls root
    // (IfcDistributionControlElement), and the Construction roots (IfcProcess/IfcResource).
    // A vertex no root reaches FAILS the emit into the R5 census.
    private static Fin<HashMap<Type, IfcDomain>> DomainAtlas(
        Seq<Type> roster, FrozenDictionary<string, IfcDomain> roots, FrozenDictionary<string, IfcDomain> overrides, Op key) {
        BidirectionalGraph<Type, SEdge<Type>> dag = new(allowParallelEdges: false);
        dag.AddVertexRange(roster);
        roster.Iter(entity => Optional(entity.BaseType).Filter(dag.ContainsVertex).Iter(super => dag.AddEdge(new SEdge<Type>(super, entity))));
        HashMap<Type, IfcDomain> claimed = dag.TopologicalSort().AsIterable()
            .Filter(vertex => roots.ContainsKey(vertex.Name))
            .Fold(HashMap<Type, IfcDomain>(), (map, root) => Claim(dag, map, root, roots[root.Name]));
        HashMap<Type, IfcDomain> pinned = roster.Fold(claimed, (map, vertex) =>
            overrides.TryGetValue(vertex.Name, out IfcDomain domain) ? map.AddOrUpdate(vertex, domain) : map);
        return roster.Filter(vertex => !pinned.ContainsKey(vertex)) is { IsEmpty: false } uncovered
            ? Fin.Fail<HashMap<Type, IfcDomain>>(new BimFault.UnmappedClass(key, $"domain-root-miss:{string.Join(',', uncovered.Map(static v => v.Name))}"))
            : Fin.Succ(pinned);
    }

    // One reachability traversal per root through the BreadthFirstSearchAlgorithm DiscoverVertex event fold —
    // O(reachable) including the root vertex itself, the SAME accumulation form the spatial view's Reachable
    // holds; an all-vertex TryFunc path-probe sweep re-recovering a path per roster member is the deleted form.
    private static HashMap<Type, IfcDomain> Claim(
        BidirectionalGraph<Type, SEdge<Type>> dag, HashMap<Type, IfcDomain> map, Type root, IfcDomain domain) {
        BreadthFirstSearchAlgorithm<Type, SEdge<Type>> search = new(dag);
        HashMap<Type, IfcDomain> claimed = map;
        search.DiscoverVertex += vertex => claimed = claimed.AddOrUpdate(vertex, domain);
        search.Compute(root);
        return claimed;
    }

    // Class IntroducedIn: class-level [VersionAdded] lowered through ReleaseMap when present — the
    // post-2x3 additions carry it (IfcAlignment IFC4X1; IfcBridge/IfcBearing IFC4X2; IfcCourse/
    // IfcMarineFacility IFC4X3) — else the REQUIRED ClassIntroductions row (the unattributed
    // IFC2x3-era floor, seeded once from the 73 retired hand rows); missing-both FAILS the emit.
    // Instantiable: !IsAbstract AND !AbstractSupertypes — the CLR flag's honest half plus the
    // overlay for the schema-abstract concretes. RemovedIn: the Retirements overlay alone (no
    // VersionRemoved attribute exists; the *StandardCase/*ElementedCase windows close here).
    private static Fin<VocabularyRow> RowOf(
        Type entity, HashMap<Type, IfcDomain> domains, FrozenDictionary<string, ReleaseVersion> introductions,
        FrozenSet<string> abstractSupertypes, FrozenDictionary<string, ReleaseVersion> retirements, Op key) =>
        Introduced(entity.Name, entity.GetCustomAttribute<VersionAddedAttribute>(), introductions, inherit: None, key)
            .Bind(introduced => Tokens(entity, introduced, introductions, key)
                .Map(tokens => new VocabularyRow(
                    entity.Name, domains[entity], introduced,
                    retirements.TryGetValue(entity.Name, out ReleaseVersion? removed) && removed is { } gone ? Some(gone) : None,
                    Instantiable: !entity.IsAbstract && !abstractSupertypes.Contains(entity.Name), tokens)));

    // Token rows reflect the entity's NEAREST-declared PredefinedType enum — its own, else the closest base
    // declaration up the chain (IfcDistributionCircuit and IfcStructuralLoadCase inherit their parent enum; a
    // DeclaredOnly-only read committed them EMPTY sets, unconstraining every circuit/load-case token at egress) —
    // walked per level so a subtype SHADOWING the property with a new enum never trips AmbiguousMatchException;
    // an enum-less chain commits the empty set (AdmitPredefined's unconstrained arm). Per-member [VersionAdded]
    // lowers through ReleaseMap; an unattributed member inherits the class IntroducedIn — exact for a class-birth
    // enum (IfcBridgeTypeEnum is wholly unattributed and wholly inherited) — and a dotted overlay row pins a
    // verified gap on a GROWN enum (IfcWall.PARAPET is IFC4X3-grown yet unattributed). NOTDEFINED/USERDEFINED
    // never commit as rows — AdmitPredefined routes them.
    private static Option<Type> TokenEnum(Type? entity) =>
        Optional(entity).Bind(t =>
            Optional(t.GetProperty(nameof(IfcWall.PredefinedType), BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly))
                .Map(static property => property.PropertyType).Filter(static pt => pt.IsEnum)
            | TokenEnum(t.BaseType));

    private static Fin<Seq<PredefinedRow>> Tokens(
        Type entity, ReleaseVersion classIntroduced, FrozenDictionary<string, ReleaseVersion> introductions, Op key) =>
        TokenEnum(entity)
            .Match(
                None: () => Fin.Succ(Seq<PredefinedRow>()),
                Some: tokenEnum => Enum.GetNames(tokenEnum).ToSeq()
                    .Filter(static name => name is not ("NOTDEFINED" or "USERDEFINED"))
                    .Traverse(name => Introduced(
                            string.Concat(entity.Name, ".", name), tokenEnum.GetField(name)?.GetCustomAttribute<VersionAddedAttribute>(),
                            introductions, inherit: Some(classIntroduced), key)
                        .Map(introduced => new PredefinedRow(name, introduced))).As());

    // The ONE span-sourcing rail for class and token alike: the overlay row FIRST (the pin wins
    // over a wrong or absent attribution), else the [VersionAdded].Release lowered through
    // ReleaseMap (an IFC4X4_DRAFT or unmapped member FAILS), else the inherited class span —
    // tokens only; a class with neither attribute nor row fails the emit.
    private static Fin<ReleaseVersion> Introduced(
        string overlayKey, VersionAddedAttribute? attribute, FrozenDictionary<string, ReleaseVersion> introductions,
        Option<ReleaseVersion> inherit, Op key) =>
        introductions.TryGetValue(overlayKey, out ReleaseVersion? pinned) && pinned is { } pin ? Fin.Succ(pin)
        : attribute is { Release: var release }
            ? ReleaseMap.Lower.TryGetValue(release, out ReleaseVersion? lowered) && lowered is { } low
                ? Fin.Succ(low)
                : Fin.Fail<ReleaseVersion>(new BimFault.CodecReject(key, $"release-unmapped:{overlayKey}:{release}"))
        : inherit.ToFin(new BimFault.UnmappedClass(key, $"introduction-miss:{overlayKey}"));

    // Gate 0, the emit-time drift audit (defense-in-depth under the composition + egress gates):
    // every overlay key (class row, dotted token pin, abstract-supertype, retirement, domain root,
    // domain override) must bind a roster member — a dotted key both its class half and its token —
    // so a stale overlay row dies as loudly as a typo'd stamp (a stale DomainRoots key would
    // otherwise mis-claim its subtree SILENTLY to a broader root, never a domain-root-miss);
    // every Materials IfcBinding seed pair must resolve an emitted row with a legal token, so an
    // orphan stamp FAILS the emit before any graph exists; and the Ifc-stripped field-identifier
    // space must stay collision-free, so a colliding pin-bump entity name FAILS here rather than
    // committing a region that cannot compile — Render stays a pure total fold.
    private static Fin<Seq<VocabularyRow>> Audit(
        Seq<VocabularyRow> rows, FrozenSet<(string Entity, string Predefined)> stamps, Seq<string> overlayKeys, Op key) {
        HashMap<string, VocabularyRow> byEntity = rows.Fold(HashMap<string, VocabularyRow>(), static (map, row) => map.Add(row.Entity, row));
        Seq<string> stale = overlayKeys.Filter(overlay => overlay.Split('.') is var half && byEntity.Find(half[0]).Match(
            Some: row => half.Length > 1 && !row.Tokens.Exists(t => t.Token == half[1]),
            None: static () => true));
        Seq<(string Entity, string Predefined)> orphans = stamps.AsIterable().ToSeq().Filter(stamp => byEntity.Find(stamp.Entity).Match(
            Some: row => stamp.Predefined is not ("" or "NOTDEFINED" or "USERDEFINED")
                && !row.Tokens.IsEmpty && !row.Tokens.Exists(t => t.Token == stamp.Predefined),
            None: static () => true));
        Seq<string> collided = toSeq(rows.Map(Identifier).GroupBy(static id => id, StringComparer.Ordinal)
            .Where(static group => group.Count() > 1).Select(static group => group.Key));
        return (stale.IsEmpty, orphans.IsEmpty, collided.IsEmpty) switch {
            (false, _, _) => Fin.Fail<Seq<VocabularyRow>>(new BimFault.UnmappedClass(key, $"overlay-stale:{string.Join(',', stale)}")),
            (_, false, _) => Fin.Fail<Seq<VocabularyRow>>(new BimFault.UnmappedClass(key, $"stamp-orphan:{string.Join(',', orphans.Map(static s => $"{s.Entity}.{s.Predefined}"))}")),
            (_, _, false) => Fin.Fail<Seq<VocabularyRow>>(new BimFault.UnmappedClass(key, $"identifier-collision:{string.Join(',', collided)}")),
            _             => Fin.Succ(rows),
        };
    }

    // Audit proves field identifiers collision-free before this deterministic row fold.
    private static string Identifier(VocabularyRow row) => row.Entity["Ifc".Length..];

    private static string Render(IEnumerable<VocabularyRow> rows) =>
        toSeq(rows)
            .Map(static row =>
                $"    public static readonly IfcClass {Identifier(row)} = new(\"{row.Entity}\", IfcDomain.{row.Domain}, {Window(row)}, instantiable: {(row.Instantiable ? "true" : "false")}, {Tokens(row.Tokens)});")   // token rows ordinal by Token — diff-stable
            .Fold(
                "    // <generated-rows>",
                static (region, line) => string.Concat(region, "\n", line))
        + "\n    // <end generated-rows>";

    private static string Window(VocabularyRow row) => row.RemovedIn.Match(
        Some: removed => $"new SchemaSpan(ReleaseVersion.{row.IntroducedIn.Key}, Some(ReleaseVersion.{removed.Key}))",
        None: () => $"IfcSchema.{row.IntroducedIn.Key}");

    private static string Tokens(Seq<PredefinedRow> tokens) => tokens.IsEmpty
        ? "Seq<PredefinedRow>()"
        : $"Seq({string.Join(", ", tokens.OrderBy(static t => t.Token, StringComparer.Ordinal).Map(static t => $"new PredefinedRow(\"{t.Token}\", ReleaseVersion.{t.IntroducedIn.Key})"))})";
}
```

## [04]-[REPRESENTATION_KEYS]

- Owner: `IfcRepresentation` the geometry-reference content-keyer [M2] projecting an `IfcProduct`/`IfcTypeProduct` representation set onto the seam `RepresentationContentHash` keyed map — `RepresentationIdentifier` (`Axis`/`Body`/`Box`/`FootPrint`) → the kernel seed-zero `XxHash128` content hash of the representation STEP — so the seam `Object` node references its geometry by content key per representation, never an IFC name leak and never an in-process BRep evaluation. Bim owns the IFC representation mapping and the `IfcRepresentationMap`/`IfcMappedItem` instancing per representation; the seam holds the neutral keyed map.
- Entry: `IfcRepresentation.Keys(IfcObjectDefinition? definition)` is ONE polymorphic content-keyer discriminating on the input shape — an occurrence `IfcProduct` folds its `IfcProductDefinitionShape.Representations` into the `RepresentationContentHash` map keyed by `RepresentationIdentifier`, content-keying each representation's STEP through the kernel `ContentHash.Of`; a type `IfcTypeProduct` folds its `RepresentationMaps` `IfcMappedItem` instancing onto the same map so an occurrence instancing a type representation shares the content key rather than re-keying; any other definition (or a null) yields `RepresentationContentHash.Empty`. There is no `KeysOf`/`MapKeys` operation family — the occurrence-versus-type distinction is the input case, never a name suffix.
- Auto: the occurrence arm reads each `IfcShapeRepresentation.RepresentationIdentifier` (the IFC axis/body/box/footprint discriminant), serializes the representation to its STEP record line, and content-keys it through the kernel seed-zero `ContentHash.Of` (`Rasm.Domain.ContentHash`, the single `XxHash128` seed) tag-namespaced so a direct shape and a mapped library shape never collide; the type arm keys the `IfcRepresentationMap.MappedRepresentation` once so every `IfcMappedItem` occurrence referencing that SAME map entity shares the content key — the instanced-geometry reuse the `Exchange/reconstruct#RECONSTRUCTION` lane mirrors, never a per-occurrence re-key. The content key is over the entity STEP serialization, so the SAME entity (a shared `IfcShapeRepresentation`, or one `IfcRepresentationMap` instanced by every occurrence) keys identically; the content-stable realized-geometry identity across distinct entities is the kernel `GeometryHash` over the tessellated geometry at the `Exchange/tessellation#TESSELLATION_BRIDGE` GLB wire, a separate content key this serialization key never duplicates.
- Packages: GeometryGymIFC_Core, Rasm.Element, Rasm, LanguageExt.Core
- Growth: a new representation identifier is one `RepresentationIdentifier` key the map carries; the content-key seed is the kernel's single seed-zero `XxHash128` `ContentHash.Of`; never a second hasher and never a geometry blob on the seam node — only the content key.
- Boundary: the geometry reference is the content-keyed map [M2] and an inlined geometry blob, a stored `GeometryHandle`, or an IFC representation name on the seam node is the deleted form; the content key composes the kernel seed-zero `XxHash128` through `Rasm.Domain.ContentHash.Of` and a second hasher (or the strata-violating `Rasm.Compute` `InterchangeIdentity` consumed up-stratum) is the named defect [H7]; the representation STEP is keyed, NOT evaluated — an in-process BRep tessellation here is the named seam violation (geometry realization routes the `Exchange/tessellation#TESSELLATION_BRIDGE` companion rail); the type representation-map instancing shares one content key across occurrences and a per-occurrence re-key is the deleted form; the occurrence-versus-type modality is the input case of one `Keys` entry and a `KeysOf`/`MapKeys` name family is the deleted form.

```csharp signature
// --- [RUNTIME_PRELUDE] --------------------------------------------------------------------
using System.Text;
using GeometryGym.Ifc;
using LanguageExt;
using Rasm.Domain;
using Rasm.Element.Classification;
using Rasm.Element.Graph;
using static LanguageExt.Prelude;

namespace Rasm.Bim.Model;

// --- [OPERATIONS] -------------------------------------------------------------------------
public static class IfcRepresentation {
    // The keyed geometry map [M2]: ONE polymorphic entry over IfcObjectDefinition discriminates the occurrence
    // product (its direct representations) from the type product (the IfcRepresentationMap instanced-geometry
    // library) on the input case, each representation keyed by its content key. The seam Object node references
    // geometry by this content key only — never an inlined geometry blob, never a second hasher.
    public static RepresentationContentHash Keys(IfcObjectDefinition? definition) =>
        definition switch {
            IfcProduct product => Optional(product.Representation).Match(
                None: () => RepresentationContentHash.Empty,
                Some: shape => shape.Representations.AsIterable()
                    .Choose(rep => Optional(rep.RepresentationIdentifier).Map(id => (Key: id, Hash: RepKey("ifc-rep", rep))))
                    .Fold(RepresentationContentHash.Empty, static (map, pair) => map.With(pair.Key, pair.Hash))),
            IfcTypeProduct type => type.RepresentationMaps.AsIterable()
                .Choose(map => Optional(map.MappedRepresentation)
                    .Bind(rep => Optional(rep.RepresentationIdentifier).Map(id => (Key: id, Hash: RepKey("ifc-repmap", rep)))))
                .Fold(RepresentationContentHash.Empty, static (acc, pair) => acc.With(pair.Key, pair.Hash)),
            _ => RepresentationContentHash.Empty,
        };

    // The content key composes the kernel seed-zero XxHash128 (Rasm.Domain.ContentHash.Of) over the entity STEP
    // record line, tag-namespaced so a direct shape and a mapped library shape never collide. ONE hasher, no second
    // hasher and no up-stratum InterchangeIdentity; the realized-geometry GeometryHash at the GLB wire is separate.
    private static UInt128 RepKey(string tag, BaseClassIfc entity) =>
        ContentHash.Of(Encoding.UTF8.GetBytes(string.Concat(tag, " ", entity.StringSTEP())));
}
```

## [05]-[RESEARCH]

- [CLASS_TAXONOMY]: the generated roster grounds against the decompile-verified GeometryGymIFC_Core `25.7.30` reflection surface — `gg.GetExportedTypes()` filtered to `IfcObjectDefinition`-assignable non-generic types yields 474 entities (54 CLR-abstract), `327` of which declare their OWN enum-typed `PredefinedType` property, an enum-less subtype committing its NEAREST base declaration (`IfcDistributionCircuit` inherits `IfcDistributionSystemEnum`, `IfcStructuralLoadCase` inherits `IfcLoadGroupTypeEnum` — the per-level `DeclaredOnly` walk that stopped at the subtype committed those sets empty and unconstrained their egress tokens), and a wholly enum-less chain committing the empty set; the `Ifc`-stripped field-identifier space carries ZERO collisions across the roster; `IfcBuiltElement` and `IfcDistributionElement` are CLR-CONCRETE (`IsAbstract == false`) despite being schema-abstract, proving the `AbstractSupertypes` overlay against the `!IsAbstract` reflex; the ten `*StandardCase`/`*ElementedCase` retired subtypes (`IfcWallStandardCase`, `IfcSlabElementedCase`, `IfcBeamStandardCase`, …) exist as real GG types, so their committed rows carry `Retirements`-closed windows while `Canonical` folds their ingress onto the base rows. The full-roster commit mechanically closes the hand-slice omissions: `IfcMarineFacilityTypeEnum` verifiably carries `SHIPLOCK`/`BARRIERBEACH`/`WATERWAY` (omitted from the retired hand row) and `IfcCourseTypeEnum` carries `BALLASTBED`/`CORE` likewise; the permissive-`BuildingElementProxy` long tail (`IfcBurner`/`IfcSolarDevice`/`IfcMedicalDevice`/`IfcCableFitting`/`IfcAudioVisualAppliance`/…) becomes first-class rows.
- [VERSION_ATTRIBUTION]: `VersionAddedAttribute.Release` supplies class and token introduction spans. `ClassIntroductions` supplies only missing class or dotted-token facts, reflected values win collisions, and an entity with neither source fails emission.
- [RELEASE_MAP]: `ReleaseMap.Lower` admits supported stable `ReleaseVersion` members and `Raise` derives the underscore-erased exact-name preimage. Unmapped, candidate, and draft releases remain typed failures, including names whose suffix has no seam image.
- [PREDEFINED_GATE]: the per-token `AdmitPredefined` egress gate [PREDEFINED_TOKEN_RULING][H8] grounds against the seam `Rasm.Element/Graph/element#NODE_MODEL` `PredefinedType` `[ValueObject<string>]` (Bim owns the token sets + the gate; the seam owns the value-object) and GeometryGym's own `validPredefinedType(value, release)` setter guard — the per-token span check is the managed mirror of the guard GG applies when an entity enum is SET, so an emit through this gate never authors a token the target schema forbids; the gate's schema argument is the SEAM `ReleaseVersion` the projector passes as `graph.Header` schema, ranked through the frozen `Chronology` because neither SmartEnum declaration order nor GG enum ordinal is chronological; the `Instantiable`-at-egress check is the `Projection/egress#IFC_EGRESS` gate reading this page's column (`abstract-class-at-egress`).
- [REPRESENTATION_KEY]: the `IfcRepresentation` content-keyer [M2] grounds against the kernel `Rasm.Domain.ContentHash.Of(ReadOnlySpan<byte>) → UInt128` seed-zero `XxHash128` content-identity (`libs/csharp/.api/api-hashing` + the kernel owner the seam `NodeId`/`ContentAddress` also compose), composed once, never a second hasher and never the up-stratum `Rasm.Compute` `InterchangeIdentity` [H7]; the `IfcShapeRepresentation.RepresentationIdentifier`/`IfcProductDefinitionShape.Representations`/`IfcRepresentationMap.MappedRepresentation`/`IfcMappedItem.MappingSource` member spellings and both `BaseClassIfc.StringSTEP()` overloads confirm against `.api/api-geometrygym-ifc`; the `StringSTEP` content key keys the SAME entity identically (instanced reuse), the content-stable realized-geometry dedup across distinct entities being the kernel `GeometryHash` at the `Exchange/tessellation#TESSELLATION_BRIDGE` GLB wire, not this serialization key.
- [ELEMENT_RETIREMENT]: the `BimElement`/`BimModel` retirement grounds against the one-property-graph collapse — the consumer-facing `Element` is a derived `Bake(objectNode)` fold over the reachable subgraph, never a second stored record — so the typed data formerly stranded on `BimElement.Properties`/`Quantities`/`Materials` rides the seam `PropertySet`/`QuantitySet`/`Material` nodes and the `Bake` fold reads one flat Option-typed field at the wire.
- [EMITTER_CENSUS]: carried R5 items the FIRST emitter run enumerates against the committed diff — the full `ClassIntroductions` floor seed (every unattributed 2x3-era class, cross-checked against the 73 retired hand rows' spans), the complete `AbstractSupertypes` roster (the schema-abstract concretes beyond the two proven members), the `DomainRoots` claim set plus the `DomainOverrides` tier (the element-discipline roots plus the `General` backbone rows `IfcSpatialElement`/`IfcGroup`/`IfcActor`, the `Controls` `IfcDistributionControlElement` root, and the `Construction` `IfcProcess`/`IfcResource` roots; `IfcBearing` → `Structural` the proven override instance beside the MEP duct/pipe/cable splits), the `Retirements` window values (deprecation vs removal release per retired subtype), and the dotted token-pin roster beyond `IfcWall.PARAPET` (every GROWN-enum member GG leaves unattributed, found by diffing emitted token spans against the retired hand rows' knowledge).
