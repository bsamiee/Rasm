# [BIM_IFC_TAXONOMY]

The IFC entity-class vocabulary `Rasm.Bim` owns as the SOLE GeometryGym/IFC owner — GENERATED, never hand-rostered: the `IfcClass` `[SmartEnum<string>]` row region is the committed output of the offline `IfcVocabularyEmitter` reflection pass over the GeometryGymIFC_Core attribute surface — every `IfcObjectDefinition`-rooted entity (474 at pin `25.7.30`, the full buildingSMART breadth superseding the retired 73-row hand slice), each row carrying its `IfcDomain` discipline, its seam `SchemaSpan` class window, its `Instantiable` schema-abstract flag, and its `Seq<PredefinedRow>` valid-token set with PER-TOKEN `IntroducedIn` spans [H8]. Enforcement is ONE authority behind three gates: Gate 0 at emit time (the Materials `IfcBinding` stamp audit — an orphan `(Entity, Predefined)` seed pair FAILS the emit, so a typo'd stamp dies before any graph exists; Materials never references `Rasm.Bim`), Gate 1 at composition time (the `Projection/semantic#GRAPH_LEGALITY` `IfcLegality : IGraphConstraint` vocabulary arms over the delta's `AddedNodes` — no new interface, no new constraint class), Gate 2 at egress (`AdmitPredefined` validating the class window AND the per-token span — `WAVEWALL` on an IFC2x3 emit faults — beside the `Projection/egress#IFC_EGRESS` `Instantiable` check). `ReleaseMap`, owned HERE, is the ONE GG-to-seam release currency the emitter and the runtime `ReleaseLower`/`ReleaseRaise` lowerings share — an unmapped member FAILS the emit and rails `BimFault.CodecReject` at runtime, retiring both silent fallbacks. The retired `BimElement` element record and `BimModel` collection stay GONE: the consumer-facing element is the seam `Bake(objectNode)` fold over the reachable `ElementGraph` subgraph, never a second stored record keyed by `GlobalId`. Growth inverts the hand-row economy: a new IFC entity class is one emitter regeneration plus at most one overlay row — never a hand row, never three parallel surfaces, never a per-element-class type.

## [01]-[INDEX]

- [01]-[IFC_CLASS]: `IfcClass` the generated `[SmartEnum<string>]` entity-class vocabulary — the `IfcDomain` discipline partition, the seam `SchemaSpan` class window [H8], the `Instantiable` schema-abstract flag, the `PredefinedRow` per-token spans, the `Resolve` ingress lookup, and the per-token `AdmitPredefined` egress gate over the seam-owned `PredefinedType` value-object [C6].
- [02]-[TAXONOMY_EMITTER]: `IfcVocabularyEmitter` the offline reflection emitter committing the `IfcClass` row region — the `ReleaseMap` GG-to-seam release table, the `VocabularyRow` row currency, the QuikGraph inheritance-DAG `DomainAtlas`, the overlay tiers (`ClassIntroductions` with dotted token pins, `AbstractSupertypes`, `Retirements`, `DomainRoots`+`DomainOverrides`), and the Gate 0 Materials stamp audit.
- [03]-[REPRESENTATION_KEYS]: `IfcRepresentation` the geometry-reference content-keyer projecting an `IfcProduct`/`IfcTypeProduct` representation set onto the seam `RepresentationContentHash` keyed map (axis/body/box/footprint → kernel `XxHash128` content hash) [M2], composing the kernel seed-zero `ContentHash.Of` identity, never a second hasher.

## [02]-[IFC_CLASS]

- Owner: `IfcClass` the generated `[SmartEnum<string>]` closed buildingSMART entity-class vocabulary keyed on the IFC entity-type string — the committed emitter output, regenerated on GeometryGym pin bumps, hand edits landing ONLY in the emitter overlays. Each row carries its `IfcDomain` discipline, its seam-owned `SchemaSpan` class window (`Graph/element#NODE_MODEL` — the SAME window record the projector stamps onto a node at ingress, never a parallel Bim copy) [H8], its `Instantiable` flag (schema-abstract supertypes such as `IfcBuiltElement`/`IfcDistributionElement` are FIRST-CLASS rows flagged `Instantiable: false` — legal CLASSIFICATION vocabulary the Materials profiled families stamp by design, illegal EGRESS classes), and its `Seq<PredefinedRow>` valid-token set where each token carries its OWN `IntroducedIn` release [H8] — so the availability axis is two-tier: the class window gates the entity, the token span gates the sub-kind. `IfcSchema` owns only the IFC release chronology (the seam `[SmartEnum]` declaration order is not chronological), the `SchemaSpan.Covers` gate the class window and the stamped node span share, and the `Rank` the per-token gate reads. The `PredefinedType` sub-class discriminant is the seam-carried `PredefinedType` `[ValueObject<string>]` on the `Object` node, of which `IfcClass` is the IFC validation authority — the semantics live HERE (the generated token sets + `AdmitPredefined`), the seam node carrying only the typed token value-object [C6].
- Entry: `IfcClass.Resolve(string entityType, Op key)` is the strict ingress lookup the projector composes — it `Canonical`-folds the deprecated `*StandardCase`/`*ElementedCase` subtypes onto their base row, then resolves the IFC entity-type string (the `ParserIfc.IdentifyIfcClass` class half) to the row that supplies the generic `Classification("ifc", row.Key)` stamped on the seam `Object` node, faulting `Model/faults#FAULT_BAND` `BimFault.UnmappedClass` on a class the roster does not carry — the typed case (band 2600, `Expected`-derived) lifting BARE onto the `Fin` rail with no `.ToError()` hop; a projector that prefers a permissive ingress instead reads `IfcClass.TryGet(entityType).IfNone(IfcClass.BuildingElementProxy)` so an unrostered future-schema leaf lands the proxy row rather than aborting the import, the two paths sharing the ONE `TryGet` Option-lift over the generated bool/out try-pattern and never a parallel resolver — the full reflected roster shrinks that fallback to genuinely-foreign names. `IfcClass.AdmitPredefined(string token, string objectType, ReleaseVersion schema, Op key)` is the egress gate — it admits the predefined token against the row's generated token set AND the token's own `IntroducedIn` span AND the class window against the target `Header` schema, returning the validated IFC predefined token the projector authors or faulting `BimFault.UnmappedClass`.
- Auto: `Resolve` reads the `Items` table by key through the generated `TryGet`; the projector folds its result into the generic `Classification` value-object so the seam node carries a `(system, code)` pair (`"ifc"`, `"IfcWall"`) rather than the `IfcClass` type itself, keeping the seam IFC-schema-free; `AdmitPredefined` trims and upper-cases the token, routes `""`/`"NOTDEFINED"` to the canonical `PredefinedType.NotDefined.Token`, routes `"USERDEFINED"` to the validated `USERDEFINED` marker requiring a non-empty `objectType` label (the projector authors the IFC `ObjectType` from it — there is no `OBJECTTYPE` token), accepts any token when the row's set is empty (the schema not constraining it — 147 of the 474 rows declare no own predefined enum), and otherwise gates PER-TOKEN: a set member passes only when `IfcSchema.Rank(schema) >= IfcSchema.Rank(row.IntroducedIn)` — matching GeometryGym's own `validPredefinedType(value, release)` setter guard — so `WAVEWALL` against an IFC2x3 emit faults `predefined-out-of-schema` where the retired class-level gate wrongly passed it [H8]; a non-member faults `predefined-reject`; the class-window check compares the seam `ReleaseVersion` the `Header` carries through the frozen chronological rank because the `[SmartEnum]` carries no ordinal; the admitted token folds into the seam node content hash through the seam `Node.ToCanonicalBytes` [C6].
- Packages: GeometryGymIFC_Core, `Rasm` (the kernel `Op` operation key the `Resolve`/`AdmitPredefined` faults carry), Rasm.Element, Thinktecture.Runtime.Extensions, LanguageExt.Core
- Growth: a new entity class, sub-kind token, or schema availability is one emitter REGENERATION — the committed-region diff — plus at most one overlay row (`ClassIntroductions` when GG leaves the class unattributed, a dotted token pin when GG leaves a grown token unattributed); a new IFC release is one `ReleaseMap` row plus one `Chronology` rank; never a hand row in the generated region, never a per-element-class type, never a `Get<Domain>` family, and never the retired `BimElement` record.
- Boundary: `BimElement` and `BimModel` are RETIRED — an owner that re-stores a `BimElement(GlobalId, IfcClass, …)` record off the seam graph is the deleted form; `IfcClass` is the IFC-schema vocabulary the projector composes, NOT a field on a seam node — the seam `Object` node carries the generic `Classification("ifc", code)` value-object and a typed `IfcClass` on the node is the named seam violation [C6]; a hand edit inside the generated-rows region is the named defect (it dies at the next regeneration — the overlays are the sole hand surface); an `Instantiable: false` row is legal classification vocabulary and an illegal egress class — the `Projection/egress#IFC_EGRESS` gate faults it `abstract-class-at-egress`, and sourcing the flag from `!Type.IsAbstract` alone is the named defect (GG declares schema-abstract supertypes as concrete C# classes); the predefined validity is an EGRESS gate (validated when the IFC entity is authored, against the token set + the per-token span + the class window [C6][H8]) and silent acceptance of an out-of-schema token is the named defect; the schema currency is the SEAM `ReleaseVersion` ranked through the frozen chronology and a bare `>=` over the SmartEnum or a GeometryGym `ReleaseVersion` leak into the gate signature is the named defect — an unranked seam release fails `IfcSchema` type initialization (the `FaultBand` registry pattern), never a silent rank sentinel; a raw entity-type string crossing a seam signature is the named defect.

```csharp signature
// --- [RUNTIME_PRELUDE] --------------------------------------------------------------------
using System.Collections.Frozen;
using GeometryGym.Ifc;
using LanguageExt;
using Rasm.Element;
using Thinktecture;
using static LanguageExt.Prelude;
using Op = Rasm.Domain.Op;                            // the kernel operation key each typed BimFault case carries
using ReleaseVersion = Rasm.Element.ReleaseVersion;   // the seam schema currency the Header carries — disambiguated
                                                      // from GeometryGym.Ifc.ReleaseVersion, which rides the GGRelease
                                                      // alias on the [TAXONOMY_EMITTER] and IFC-text codec legs only.

namespace Rasm.Bim;

// --- [TYPES] ------------------------------------------------------------------------------
// The discipline partition over the FULL IfcObjectDefinition closure the emitter commits — the buildingSMART schema
// domains folded to query-grade rows. The element disciplines alone cannot legalize the reflected roster: General owns
// the kernel/spatial/actor/group backbone (IfcProject/IfcSite/IfcGroup/IfcActor), Controls the building-controls branch
// (the IfcDistributionControlElement sensors/actuators/controllers line), Construction the process/resource branch
// (IfcProcess/IfcConstructionResource); a roster member no DomainRoots row reaches still FAILS the emit.
public enum IfcDomain : byte {
    Architecture = 0, Structural = 1, HvacFire = 2, Electrical = 3, Plumbing = 4, Infrastructure = 5, Geotechnical = 6,
    General = 7, Controls = 8, Construction = 9,
}

// --- [TABLES] -----------------------------------------------------------------------------
// The per-class availability window is the SEAM-owned SchemaSpan (Graph/element#NODE_MODEL): the
// row's class window and the stamped node span are ONE type — never a parallel Bim copy [H8]. Bim
// owns only the release chronology the seam [SmartEnum] declaration order does not encode, the
// Covers gate both windows share, and the Rank the per-token AdmitPredefined gate reads — internal,
// never private, because the token gate ranks OUTSIDE this class. The gate ranks chronologically
// (the SmartEnum has no ordinal), never bare `>=`.
internal static class IfcSchema {
    public static readonly SchemaSpan Ifc2x3     = SchemaSpan.From(ReleaseVersion.Ifc2X3);
    public static readonly SchemaSpan Ifc4       = SchemaSpan.From(ReleaseVersion.Ifc4);
    public static readonly SchemaSpan Ifc4x1     = SchemaSpan.From(ReleaseVersion.Ifc4X1);
    public static readonly SchemaSpan Ifc4x3     = SchemaSpan.From(ReleaseVersion.Ifc4X3);
    public static readonly SchemaSpan Ifc4x3Add2 = SchemaSpan.From(ReleaseVersion.Ifc4X3Add2);

    static readonly FrozenDictionary<string, int> Chronology = new Dictionary<string, int> {
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

    internal static int Rank(ReleaseVersion v) => Chronology[v.Key];

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
    // <generated-rows: IfcVocabularyEmitter over GeometryGymIFC_Core <assembly version from the
    //  central pin, stamped on every regeneration> — committed output, regenerated on pin bump;
    //  hand edits land in the emitter overlays (ClassIntroductions incl. dotted token pins /
    //  AbstractSupertypes / Retirements / DomainRoots+DomainOverrides), NEVER here. The four rows
    //  below pin the row grammar; the committed region carries every IfcObjectDefinition-rooted
    //  entity — 474 at pin 25.7.30, field identifier = entity name minus the Ifc prefix.>
    public static readonly IfcClass BuiltElement = new("IfcBuiltElement", IfcDomain.Architecture, IfcSchema.Ifc4, instantiable: false, Seq<PredefinedRow>());   // AbstractSupertypes overlay: CLR-concrete, schema-abstract
    public static readonly IfcClass Wall = new("IfcWall", IfcDomain.Architecture, IfcSchema.Ifc2x3, instantiable: true, Seq(
        new PredefinedRow("ELEMENTEDWALL", ReleaseVersion.Ifc4), new PredefinedRow("MOVABLE", ReleaseVersion.Ifc2X3),
        new PredefinedRow("PARAPET", ReleaseVersion.Ifc4X3),           // dotted overlay pin — GG leaves the IFC4X3-grown token unattributed
        new PredefinedRow("PARTITIONING", ReleaseVersion.Ifc2X3), new PredefinedRow("PLUMBINGWALL", ReleaseVersion.Ifc2X3),
        new PredefinedRow("POLYGONAL", ReleaseVersion.Ifc4), new PredefinedRow("RETAININGWALL", ReleaseVersion.Ifc4X1),
        new PredefinedRow("SHEAR", ReleaseVersion.Ifc2X3), new PredefinedRow("SOLIDWALL", ReleaseVersion.Ifc4),
        new PredefinedRow("STANDARD", ReleaseVersion.Ifc4), new PredefinedRow("WAVEWALL", ReleaseVersion.Ifc4X3)));
    public static readonly IfcClass Bridge = new("IfcBridge", IfcDomain.Infrastructure, IfcSchema.Ifc4x1, instantiable: true, Seq(   // class-level [VersionAdded(IFC4X2)] → seam Ifc4X1; class-birth tokens inherit it
        new PredefinedRow("ARCHED", ReleaseVersion.Ifc4X1), new PredefinedRow("CABLE_STAYED", ReleaseVersion.Ifc4X1),
        new PredefinedRow("CANTILEVER", ReleaseVersion.Ifc4X1), new PredefinedRow("CULVERT", ReleaseVersion.Ifc4X1),
        new PredefinedRow("FRAMEWORK", ReleaseVersion.Ifc4X1), new PredefinedRow("GIRDER", ReleaseVersion.Ifc4X1),
        new PredefinedRow("SUSPENSION", ReleaseVersion.Ifc4X1), new PredefinedRow("TRUSS", ReleaseVersion.Ifc4X1)));
    public static readonly IfcClass WallStandardCase = new("IfcWallStandardCase", IfcDomain.Architecture, new SchemaSpan(ReleaseVersion.Ifc2X3, Some(ReleaseVersion.Ifc4X3)), instantiable: false, Seq<PredefinedRow>());   // Retirements overlay: Canonical folds its ingress onto Wall; the closed window rejects its egress
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
    static string Canonical(string entityType) =>
        entityType.EndsWith("StandardCase", StringComparison.Ordinal)    ? entityType[..^"StandardCase".Length]
        : entityType.EndsWith("ElementedCase", StringComparison.Ordinal) ? entityType[..^"ElementedCase".Length]
        : entityType;

    // The egress gate, PER-TOKEN [C6][H8]: the class window gates the entity against the target Header schema, then
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
}
```

## [03]-[TAXONOMY_EMITTER]

- Owner: `IfcVocabularyEmitter` the OFFLINE BCL-reflection emitter — a repo tool run on GeometryGym pin bumps whose output is the committed `IfcClass` row region; the committed table is the system of record, so there is no runtime reflection, no Roslyn generator, and no sidecar file. `ReleaseMap` the ONE GG-to-seam release table (the `Lower` fold and its exact-name-preimage `Raise` inverse) the emitter AND the runtime `Projection/semantic#SEMANTIC_PROJECTOR` `ReleaseLower` / `Projection/egress#IFC_EGRESS` `ReleaseRaise` lowerings share. `VocabularyRow` the emitter row currency one render line commits. The overlays are hand-curated FROZEN data — `ClassIntroductions` (class rows for the unattributed IFC2x3-era floor, seeded once from the 73 retired hand rows' spans, PLUS dotted `"IfcWall.PARAPET"` token rows pinning verified GG attribution gaps — one dictionary, the key shape discriminating the tier), `AbstractSupertypes` (the schema-abstract set the CLR flag misreports), `Retirements` (the `*StandardCase`/`*ElementedCase` closed windows — no `VersionRemoved` attribute exists), and `DomainRoots`+`DomainOverrides` (the hierarchy-root discipline claims spanning the WHOLE `IfcObjectDefinition` closure — the element disciplines plus the `General`/`Controls`/`Construction` backbone roots — plus the per-entity re-pins).
- Entry: `IfcVocabularyEmitter.Emit(Assembly gg, FrozenDictionary<string, ReleaseVersion> classIntroductions, FrozenSet<string> abstractSupertypes, FrozenDictionary<string, ReleaseVersion> retirements, FrozenDictionary<string, IfcDomain> domainRoots, FrozenDictionary<string, IfcDomain> domainOverrides, FrozenSet<(string Entity, string Predefined)> materialsStamps, Op key)` → `Fin<string>` — one entrypoint owning the whole regeneration: reflect the roster, resolve disciplines, source spans, audit, render.
- Auto: the roster is `gg.GetExportedTypes()` filtered to `IfcObjectDefinition`-assignable non-generic types, captured through the `Try.lift` boundary funnel; `DomainAtlas` folds the roster's `BaseType` chains into ONE QuikGraph `BidirectionalGraph<Type, SEdge<Type>>` inheritance DAG and resolves every discipline from it — each `DomainRoots` row claims its `TreeBreadthFirstSearch` reachability closure in `TopologicalSort` (base-first) order so the NEAREST root wins by overwrite, `DomainOverrides` re-pins single entities last, and a root-less vertex FAILS the emit — replacing N per-entity base-chain walks with one graph fold; class `IntroducedIn` reads the class-level `[VersionAdded]` lowered through `ReleaseMap` when present, else the REQUIRED `ClassIntroductions` row, and missing-both FAILS the emit — never a silent `Ifc2X3` default, so a NEW unattributed entity from a pin bump dies loudly; token spans read the per-member `[VersionAdded]` the same way, a dotted overlay row pinning a verified attribution gap and an unattributed member inheriting the class `IntroducedIn`; `Instantiable` is `!IsAbstract && !AbstractSupertypes(name)` — the CLR flag contributes its honest half (54 CLR-abstract types cannot be authored), the overlay the schema-abstract set the flag misreports; the Gate 0 audit rejects any Materials stamp pair the emitted roster cannot legalize AND any overlay key that binds no roster member (a stale overlay row dies as loudly as an orphan stamp); `Render` commits one declaration line per row ordered `StringComparer.Ordinal`, field identifier = entity name minus the `Ifc` prefix (zero collisions across the 474-name roster), the GG assembly version stamped into the region header so a pin bump is a visible diff.
- Packages: GeometryGymIFC_Core (the reflected attribute surface), QuikGraph (the inheritance DAG), `Rasm` (the kernel `Op` operation key the emitter faults carry), Rasm.Element (the seam `ReleaseVersion` currency), LanguageExt.Core
- Growth: a new IFC release is one `ReleaseMap` row plus one `IfcSchema.Chronology` rank; a new discipline claim is one `DomainRoots` or `DomainOverrides` row; a GG attribution gap is one dotted `ClassIntroductions` row; a newly retired entity is one `Retirements` row; everything else is the regeneration diff.
- Boundary: the emitter never runs at runtime and its output is never hand-edited — the overlays are the sole hand surface; `Instantiable` sourced from `!Type.IsAbstract` ALONE is the named defect (the CLR flag is schema-falsified on the supertypes); `IFC4X4_DRAFT` is excluded by law and any GG member `ReleaseMap.Lower` omits FAILS the emit — at runtime the same miss rails `BimFault.CodecReject`, and the `?? ReleaseVersion.Ifc4X3Add2` / `GGRelease.IFC4X3_ADD2` silent fallbacks are the retired forms; `Raise` derives as `Lower`'s exact-name preimage and the seam `Ifc5` therefore has NO GG image — an `Ifc5` egress target is the typed fault, never a default; the stamp audit reads the Materials seed pairs as DATA (Materials never references `Rasm.Bim`); a per-entity hand base-chain walk beside the `DomainAtlas` DAG is the rejected second walk.

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
using Rasm.Element;
using static LanguageExt.Prelude;
using GGRelease = GeometryGym.Ifc.ReleaseVersion;     // the GeometryGym release enum — emitter + codec legs only
using Op = Rasm.Domain.Op;
using ReleaseVersion = Rasm.Element.ReleaseVersion;

namespace Rasm.Bim;

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

    public static readonly FrozenDictionary<ReleaseVersion, GGRelease> Raise =
        Lower.Where(static pair => string.Equals(pair.Key.ToString(), pair.Value.Key, StringComparison.OrdinalIgnoreCase))
             .ToFrozenDictionary(static pair => pair.Value, static pair => pair.Key);
}

// --- [MODELS] ------------------------------------------------------------------------------
// The emitter row currency — one row renders as one committed IfcClass declaration line.
public readonly record struct VocabularyRow(
    string Entity, IfcDomain Domain, ReleaseVersion IntroducedIn, Option<ReleaseVersion> RemovedIn,
    bool Instantiable, Seq<PredefinedRow> Tokens);

// --- [OPERATIONS] --------------------------------------------------------------------------
// Design-time only (run on pin bump; output committed into [IFC_CLASS]; the region header stamps
// the exact GG assembly version so a bump is a visible diff). BCL reflection, no Roslyn generator,
// no runtime read, no new package beyond the admitted QuikGraph substrate.
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
                    .Map(rows => Render(gg.GetName().Version, rows.OrderBy(static r => r.Entity, StringComparer.Ordinal)))));

    // ONE QuikGraph inheritance DAG replaces N per-entity base-chain walks: edges base → derived
    // over the reflected roster; each DomainRoots row claims its TreeBreadthFirstSearch closure in
    // TopologicalSort (base-first) order so the NEAREST (most-derived) root wins by overwrite;
    // DomainOverrides re-pins single entities last — IfcBearing sits under IfcBuiltElement, so its
    // Structural verdict is an override row, the tier the MEP duct/pipe/cable splits already prove
    // load-bearing. The root set spans the WHOLE closure: the element disciplines plus the General
    // backbone roots (IfcSpatialElement/IfcGroup/IfcActor), the Controls root
    // (IfcDistributionControlElement), and the Construction roots (IfcProcess/IfcResource).
    // A vertex no root reaches FAILS the emit into the R5 census.
    static Fin<HashMap<Type, IfcDomain>> DomainAtlas(
        Seq<Type> roster, FrozenDictionary<string, IfcDomain> roots, FrozenDictionary<string, IfcDomain> overrides, Op key) {
        BidirectionalGraph<Type, SEdge<Type>> dag = new(allowParallelEdges: false);
        dag.AddVertexRange(roster);
        roster.Iter(entity => Optional(entity.BaseType).Filter(dag.ContainsVertex).Iter(super => dag.AddEdge(new SEdge<Type>(super, entity))));
        HashMap<Type, IfcDomain> claimed = dag.TopologicalSort().AsIterable()
            .Filter(vertex => roots.ContainsKey(vertex.Name))
            .Fold(HashMap<Type, IfcDomain>(), (map, root) => Claim(dag, roster, map, root, roots[root.Name]));
        HashMap<Type, IfcDomain> pinned = roster.Fold(claimed, (map, vertex) =>
            overrides.TryGetValue(vertex.Name, out IfcDomain domain) ? map.AddOrUpdate(vertex, domain) : map);
        return roster.Filter(vertex => !pinned.ContainsKey(vertex)) is { IsEmpty: false } uncovered
            ? Fin.Fail<HashMap<Type, IfcDomain>>(new BimFault.UnmappedClass(key, $"domain-root-miss:{string.Join(',', uncovered.Map(static v => v.Name))}"))
            : Fin.Succ(pinned);
    }

    static HashMap<Type, IfcDomain> Claim(
        BidirectionalGraph<Type, SEdge<Type>> dag, Seq<Type> roster, HashMap<Type, IfcDomain> map, Type root, IfcDomain domain) {
        TryFunc<Type, IEnumerable<SEdge<Type>>> reach = dag.TreeBreadthFirstSearch(root);
        return roster.Fold(map, (acc, vertex) => vertex == root || reach(vertex, out _) ? acc.AddOrUpdate(vertex, domain) : acc);
    }

    // Class IntroducedIn: class-level [VersionAdded] lowered through ReleaseMap when present — the
    // post-2x3 additions carry it (IfcAlignment IFC4X1; IfcBridge/IfcBearing IFC4X2; IfcCourse/
    // IfcMarineFacility IFC4X3) — else the REQUIRED ClassIntroductions row (the unattributed
    // IFC2x3-era floor, seeded once from the 73 retired hand rows); missing-both FAILS the emit.
    // Instantiable: !IsAbstract AND !AbstractSupertypes — the CLR flag's honest half plus the
    // overlay for the schema-abstract concretes. RemovedIn: the Retirements overlay alone (no
    // VersionRemoved attribute exists; the *StandardCase/*ElementedCase windows close here).
    static Fin<VocabularyRow> RowOf(
        Type entity, HashMap<Type, IfcDomain> domains, FrozenDictionary<string, ReleaseVersion> introductions,
        FrozenSet<string> abstractSupertypes, FrozenDictionary<string, ReleaseVersion> retirements, Op key) =>
        Introduced(entity.Name, entity.GetCustomAttribute<VersionAddedAttribute>(), introductions, inherit: None, key)
            .Bind(introduced => Tokens(entity, introduced, introductions, key)
                .Map(tokens => new VocabularyRow(
                    entity.Name, domains[entity], introduced,
                    retirements.TryGetValue(entity.Name, out ReleaseVersion? removed) && removed is { } gone ? Some(gone) : None,
                    Instantiable: !entity.IsAbstract && !abstractSupertypes.Contains(entity.Name), tokens)));

    // Token rows reflect the entity's OWN PredefinedType enum (327 of the 474 declare one; the rest
    // commit an empty set — AdmitPredefined's unconstrained arm). Per-member [VersionAdded] lowers
    // through ReleaseMap; an unattributed member inherits the class IntroducedIn — exact for a
    // class-birth enum (IfcBridgeTypeEnum is wholly unattributed and wholly inherited) — and a
    // dotted overlay row pins a verified gap on a GROWN enum (IfcWall.PARAPET is IFC4X3-grown yet
    // unattributed). NOTDEFINED/USERDEFINED never commit as rows — AdmitPredefined routes them.
    static Fin<Seq<PredefinedRow>> Tokens(
        Type entity, ReleaseVersion classIntroduced, FrozenDictionary<string, ReleaseVersion> introductions, Op key) =>
        Optional(entity.GetProperty(nameof(IfcWall.PredefinedType), BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly))
            .Map(static property => property.PropertyType).Filter(static t => t.IsEnum)
            .Match(
                None: () => Fin.Succ(Seq<PredefinedRow>()),
                Some: tokenEnum => Enum.GetNames(tokenEnum).ToSeq()
                    .Filter(static name => name is not ("NOTDEFINED" or "USERDEFINED"))
                    .Traverse(name => Introduced(
                            string.Concat(entity.Name, ".", name), tokenEnum.GetField(name)!.GetCustomAttribute<VersionAddedAttribute>(),
                            introductions, inherit: Some(classIntroduced), key)
                        .Map(introduced => new PredefinedRow(name, introduced))).As());

    // The ONE span-sourcing rail for class and token alike: the overlay row FIRST (the pin wins
    // over a wrong or absent attribution), else the [VersionAdded].Release lowered through
    // ReleaseMap (an IFC4X4_DRAFT or unmapped member FAILS), else the inherited class span —
    // tokens only; a class with neither attribute nor row fails the emit.
    static Fin<ReleaseVersion> Introduced(
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
    // otherwise mis-claim its subtree SILENTLY to a broader root, never a domain-root-miss); and
    // every Materials IfcBinding seed pair must resolve an emitted row with a legal token, so an
    // orphan stamp FAILS the emit before any graph exists.
    static Fin<Seq<VocabularyRow>> Audit(
        Seq<VocabularyRow> rows, FrozenSet<(string Entity, string Predefined)> stamps, Seq<string> overlayKeys, Op key) {
        HashMap<string, VocabularyRow> byEntity = rows.Fold(HashMap<string, VocabularyRow>(), static (map, row) => map.Add(row.Entity, row));
        Seq<string> stale = overlayKeys.Filter(overlay => overlay.Split('.') is var half && byEntity.Find(half[0]).Match(
            Some: row => half.Length > 1 && !row.Tokens.Exists(t => t.Token == half[1]),
            None: static () => true));
        Seq<(string Entity, string Predefined)> orphans = stamps.AsIterable().ToSeq().Filter(stamp => byEntity.Find(stamp.Entity).Match(
            Some: row => stamp.Predefined is not ("" or "NOTDEFINED" or "USERDEFINED")
                && !row.Tokens.IsEmpty && !row.Tokens.Exists(t => t.Token == stamp.Predefined),
            None: static () => true));
        return (stale.IsEmpty, orphans.IsEmpty) switch {
            (false, _) => Fin.Fail<Seq<VocabularyRow>>(new BimFault.UnmappedClass(key, $"overlay-stale:{string.Join(',', stale)}")),
            (_, false) => Fin.Fail<Seq<VocabularyRow>>(new BimFault.UnmappedClass(key, $"stamp-orphan:{string.Join(',', orphans.Map(static s => $"{s.Entity}.{s.Predefined}"))}")),
            _          => Fin.Succ(rows),
        };
    }

    // One IfcClass declaration line per row: field identifier = Entity["Ifc".Length..] (zero
    // collisions across the 474-name roster — a collision FAILS the render), token rows ordinal by
    // Token so a regeneration is diff-stable, open windows as IfcSchema anchors, closed windows as
    // new SchemaSpan(introduced, Some(removed)), the GG pin version in the region header.
    static string Render(Version? pin, IEnumerable<VocabularyRow> rows) => /* the committed generated-rows region */ ;
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
using Rasm.Element;
using static LanguageExt.Prelude;

namespace Rasm.Bim;

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
    static UInt128 RepKey(string tag, BaseClassIfc entity) =>
        ContentHash.Of(Encoding.UTF8.GetBytes(string.Concat(tag, " ", entity.StringSTEP())));
}
```

## [05]-[RESEARCH]

- [CLASS_TAXONOMY]: the generated roster grounds against the decompile-verified GeometryGymIFC_Core `25.7.30` reflection surface — `gg.GetExportedTypes()` filtered to `IfcObjectDefinition`-assignable non-generic types yields 474 entities (54 CLR-abstract), `327` of which declare their OWN enum-typed `PredefinedType` property (the rest commit empty token sets); the `Ifc`-stripped field-identifier space carries ZERO collisions across the roster; `IfcBuiltElement` and `IfcDistributionElement` are CLR-CONCRETE (`IsAbstract == false`) despite being schema-abstract, proving the `AbstractSupertypes` overlay against the `!IsAbstract` reflex; the ten `*StandardCase`/`*ElementedCase` retired subtypes (`IfcWallStandardCase`, `IfcSlabElementedCase`, `IfcBeamStandardCase`, …) exist as real GG types, so their committed rows carry `Retirements`-closed windows while `Canonical` folds their ingress onto the base rows. The full-roster commit mechanically closes the hand-slice omissions: `IfcMarineFacilityTypeEnum` verifiably carries `SHIPLOCK`/`BARRIERBEACH`/`WATERWAY` (omitted from the retired hand row) and `IfcCourseTypeEnum` carries `BALLASTBED`/`CORE` likewise; the permissive-`BuildingElementProxy` long tail (`IfcBurner`/`IfcSolarDevice`/`IfcMedicalDevice`/`IfcCableFitting`/`IfcAudioVisualAppliance`/…) becomes first-class rows.
- [VERSION_ATTRIBUTION]: `VersionAddedAttribute` is decompile-verified as `(ReleaseVersion release)` ctor + `Release` property, `AttributeUsage: All` — and its population REFINES the emitter's sourcing law: class-level attribution EXISTS on the post-IFC2x3 additions (`IfcAlignment` → `IFC4X1`, `IfcBridge`/`IfcBearing` → `IFC4X2`, `IfcCourse`/`IfcMarineFacility` → `IFC4X3`) and is absent on the IFC2x3-era floor (`IfcWall`/`IfcElement`/`IfcBuiltElement` carry none), so the REFLECTED win covers every post-2x3 class and the `ClassIntroductions` overlay carries only the unattributed floor (seeded once from the 73 retired hand rows) — missing-both still FAILS the emit so a NEW unattributed entity from a pin bump dies loudly. Per-member token attribution is real on GROWN enums (`IfcWallTypeEnum`: `STANDARD`/`POLYGONAL`/`ELEMENTEDWALL`/`SOLIDWALL` → `IFC4`, `RETAININGWALL` → `IFC4X2`, `WAVEWALL` → `IFC4X3`) and absent on class-birth enums (`IfcBridgeTypeEnum` wholly unattributed — inheritance from the class span is exact); the verified attribution GAP class — `IfcWall.PARAPET`, IFC4X3-grown yet unattributed, which inheritance would mis-span to `Ifc2X3` — is pinned by the dotted `ClassIntroductions` token row, the overlay tier that keeps the buildingSMART truth without hand-editing the generated region. Reflected spans that WIDEN a retired hand row (`IfcBridge` and `IfcBearing` hand-pinned `Ifc4x3`, reflected `IFC4X2` → seam `Ifc4X1`) are reflected-wins; the seeding diff enumerates every such class at first emitter run.
- [RELEASE_MAP]: the `ReleaseMap.Lower` domain grounds against the decompile-verified GG `ReleaseVersion` member set (`IFC2X`, `IFC2x2`, `IFC2x3`, `IFC4`, `IFC4A1`, `IFC4A2`, `IFC4X1`, `IFC4X2`, `IFC4X3_RC1`..`RC4`, `IFC4X3`, `IFC4X3_ADD2`, `IFC4X4_DRAFT`) — fourteen mapped members, `IFC4X4_DRAFT` excluded by law; `Raise` derives as the exact-name preimage (`IFC2x3` ← `Ifc2X3`, `IFC4` ← `Ifc4`, `IFC4X1` ← `Ifc4X1`, `IFC4X3` ← `Ifc4X3`, `IFC4X3_ADD2` ← `Ifc4X3Add2`) so a seam schema with no GG image (`Ifc5`) faults typed at egress; the runtime consumers are the `Projection/semantic#SEMANTIC_PROJECTOR` `ReleaseLower` (header lowering at `Sniff`) and the `Projection/egress#IFC_EGRESS` `ReleaseRaise` (`new DatabaseIfc` target), whose `?? ReleaseVersion.Ifc4X3Add2` and `GGRelease.IFC4X3_ADD2` silent fallbacks this table retires.
- [PREDEFINED_GATE]: the per-token `AdmitPredefined` egress gate [C6][H8] grounds against the seam `Rasm.Element/Graph/element#NODE_MODEL` `PredefinedType` `[ValueObject<string>]` (Bim owns the token sets + the gate; the seam owns the value-object) and GeometryGym's own `validPredefinedType(value, release)` setter guard — the per-token span check is the managed mirror of the guard GG applies when an entity enum is SET, so an emit through this gate never authors a token the target schema forbids; the gate's schema argument is the SEAM `ReleaseVersion` the projector passes as `graph.Header` schema, ranked through the frozen `Chronology` because neither SmartEnum declaration order nor GG enum ordinal is chronological; the `Instantiable`-at-egress check is the `Projection/egress#IFC_EGRESS` gate reading this page's column (`abstract-class-at-egress`).
- [REPRESENTATION_KEY]: the `IfcRepresentation` content-keyer [M2] grounds against the kernel `Rasm.Domain.ContentHash.Of(ReadOnlySpan<byte>) → UInt128` seed-zero `XxHash128` content-identity (`libs/csharp/.api/api-hashing` + the kernel owner the seam `NodeId`/`ContentAddress` also compose), composed once, never a second hasher and never the up-stratum `Rasm.Compute` `InterchangeIdentity` [H7]; the `IfcShapeRepresentation.RepresentationIdentifier`/`IfcProductDefinitionShape.Representations`/`IfcRepresentationMap.MappedRepresentation`/`IfcMappedItem.MappingSource` member spellings and both `BaseClassIfc.StringSTEP()` overloads confirm against `.api/api-geometrygym-ifc`; the `StringSTEP` content key keys the SAME entity identically (instanced reuse), the content-stable realized-geometry dedup across distinct entities being the kernel `GeometryHash` at the `Exchange/tessellation#TESSELLATION_BRIDGE` GLB wire, not this serialization key.
- [ELEMENT_RETIREMENT]: the `BimElement`/`BimModel` retirement grounds against the one-property-graph collapse — the consumer-facing `Element` is a derived `Bake(objectNode)` fold over the reachable subgraph, never a second stored record — so the typed data formerly stranded on `BimElement.Properties`/`Quantities`/`Materials` rides the seam `PropertySet`/`QuantitySet`/`Material` nodes and the `Bake` fold reads one flat Option-typed field at the wire.
- [EMITTER_CENSUS]: carried R5 items the FIRST emitter run enumerates against the committed diff — the full `ClassIntroductions` floor seed (every unattributed 2x3-era class, cross-checked against the 73 retired hand rows' spans), the complete `AbstractSupertypes` roster (the schema-abstract concretes beyond the two proven members), the `DomainRoots` claim set plus the `DomainOverrides` tier (the element-discipline roots plus the `General` backbone rows `IfcSpatialElement`/`IfcGroup`/`IfcActor`, the `Controls` `IfcDistributionControlElement` root, and the `Construction` `IfcProcess`/`IfcResource` roots; `IfcBearing` → `Structural` the proven override instance beside the MEP duct/pipe/cable splits), the `Retirements` window values (deprecation vs removal release per retired subtype), and the dotted token-pin roster beyond `IfcWall.PARAPET` (every GROWN-enum member GG leaves unattributed, found by diffing emitted token spans against the retired hand rows' knowledge).
