# [BIM_IFC_TAXONOMY]

`Rasm.Bim` owns the IFC entity-class vocabulary as the SOLE GeometryGym/IFC owner, and it is GENERATED: the `IfcClass` `[SmartEnum<string>]` row region commits the `Model/emitter#TAXONOMY_EMITTER` offline pass, covering every published-schema `IfcObjectDefinition`-rooted entity at full buildingSMART breadth with the draft surface excluded at entity and token grain alike. Availability rides two tiers — the row's `SchemaSpan` window gates the entity, each `PredefinedRow` span gates the sub-kind — and one `EgressEligibility` column decides whether the class authors at all. Three gates share this one authority: the Gate-0 Materials `IfcBinding` stamp audit at emit time, the Gate-1 `Projection/semantic#GRAPH_LEGALITY` vocabulary arms at composition, and the Gate-2 `AdmitPredefined` fold at egress. `ReleaseMap` seats HERE as the ONE GG-to-seam release currency the emitter and the runtime `ReleaseLower`/`ReleaseRaise`/`Sniff` lowerings share, so an unmapped member fails the emit and rails `BimFault.Refused` with `BimReason.Codec` at runtime rather than falling back silently. `BimElement` and `BimModel` stay retired; the consumer-facing element is the seam `Bake(objectNode)` fold. Growth inverts the hand-row economy: a new entity class costs one regeneration and at most one overlay row, never a hand row and never a per-element-class type.

## [01]-[INDEX]

- [02]-[IFC_CLASS]: `IfcClass` the generated `[SmartEnum<string>]` entity-class vocabulary over the `IfcDomain` discipline partition, the `IfcSchema` release table, the `ReleaseMap` GG-to-seam currency, the `EgressEligibility` authoring verdict, the `PredefinedToken` admitted-token family, and the `PredefinedRow` per-token spans — `Resolve` the strict ingress, `TryGet` the ONE lookup, `AdmitPredefined` the egress gate [PREDEFINED_TOKEN_RULING].
- [03]-[REPRESENTATION_KEYS]: `IfcRepresentation` the geometry-reference content-keyer projecting an `IfcProduct`/`IfcTypeProduct` representation set onto the seam `RepresentationContentHash` keyed map (axis/body/box/footprint → kernel `XxHash128` content hash) [M2], composing the kernel `CanonicalWriter` framing under the seed-zero `ContentHash.Of`, never a second hasher.

## [02]-[IFC_CLASS]

- Owner: `IfcClass` the generated closed buildingSMART entity-class vocabulary keyed on the IFC entity-type string, each row carrying its `IfcDomain` discipline, its seam-owned `SchemaSpan` class window (`Graph/element#NODE_MODEL` — the SAME window record the projector stamps onto a node at ingress, never a parallel Bim copy) [H8], its `EgressEligibility` verdict, and its `Seq<PredefinedRow>` token set. `IfcDomain` is the IFC SCHEMA-domain partition — the buildingSMART domains the emitter's inheritance DAG resolves — and each row composes the kernel `Rasm.Drawing` `DisciplineDesignator` so a discipline reaches its NCS sheet letter without a second discipline vocabulary. `IfcSchema` owns the ONE release table both the `Covers` window gate and the per-token rank read, derived from the seam `ReleaseVersion` roster's own declaration order. `ReleaseMap` is the ONE GG-to-seam release correspondence, `Lower` the fold and `Raise` its underscore-erased exact-name preimage.
- Entry: `IfcClass.Resolve(string entityType, Op key)` is the strict ingress the projector composes — `Canonical` folds the IFC4-deprecated `*StandardCase`/`*ElementedCase` subtypes onto their base row, then the entity-type string resolves to the row supplying the generic `Classification(System, Key)` the seam `Object` node carries, faulting `element-class-miss` on a class the roster omits; `IfcClass.EntityClass` is that value DERIVED once per row through the seam's ONE `Classification.Of` admission, so a rail-free authoring helper reads the admitted pair rather than re-minting it per element or reaching a throwing `Classification.Create`. A projector preferring a permissive ingress reads `TryGet(entityType).IfNone(BuildingElementProxy)` through the SAME Option-lift, so an unrostered future-schema leaf lands the proxy row rather than aborting the import. `AdmitPredefined(string token, string objectType, ReleaseVersion schema, Op key)` is the egress gate: `Admits` decides the class against the target schema, `PredefinedToken.Admit` admits the raw token ONCE into its typed case, and the `Named` arm gates on the token's own `IntroducedIn` rank.
- Cases: `EgressEligibility` names the two states the published roster carries — `Authorable`, whose gate is the class window, and `Vocabulary`, the EXPRESS-abstract supertypes that classify and never author, whose refusal is release-independent. A draft arm is unreachable by construction: the emitter's published-membership gate drops draft surface before a row commits. `PredefinedToken` names the three shapes a raw token admits to — `Canonical` (the empty and `NOTDEFINED` spellings), `UserDefined` carrying the required label, and `Named` carrying a schema token the row's set decides.
- Auto: `Resolve` reads the generated `TryGet` by key; the projector folds its result into the generic `Classification` value-object so the seam node carries a `(system, code)` pair rather than the `IfcClass` type itself, keeping the seam IFC-schema-free. `AdmitPredefined` admits the token once at the boundary — `USERDEFINED` requires a non-empty `objectType` label (the projector authors the IFC `ObjectType` from it; there is no `OBJECTTYPE` token), an empty token set constrains nothing, and a set member passes only when the target schema ranks at or past the token's own `IntroducedIn`, so `WAVEWALL` against an IFC2x3 emit faults `predefined-out-of-schema` where the retired class-level gate wrongly passed it [H8]. The admitted token folds into the seam node content hash through `Node.ToCanonicalBytes` [PREDEFINED_TOKEN_RULING].
- Packages: GeometryGymIFC_Core, `Rasm` (the kernel `Op`; `Rasm.Drawing` `DisciplineDesignator`), Rasm.Element (the seam `SchemaSpan`/`ReleaseVersion`/`PredefinedType`/`Classification`), Thinktecture.Runtime.Extensions (`[SmartEnum<string>]`, `[Union]`, `[UseDelegateFromConstructor]`), LanguageExt.Core
- Growth: a new entity class, sub-kind token, or schema availability is one emitter REGENERATION — the committed-region diff; a new IFC release is one `ReleaseMap` row over a seam roster row that ranks and spans itself; never a hand row in the generated region, never a per-element-class type, never a `Get<Domain>` family, never the retired `BimElement` record.
- Boundary: `BimElement` and `BimModel` are RETIRED — the typed data that record stranded rides the seam `PropertySet`/`QuantitySet`/`Material` nodes the `Bake` fold reaches. `IfcClass` is the vocabulary the projector composes, NOT a field on a seam node — a typed `IfcClass` on the node is the named seam violation [PREDEFINED_TOKEN_RULING]. A hand edit inside the generated-rows region dies at the next regeneration; the `Model/emitter#VOCABULARY_OVERLAYS` tiers are the sole hand surface. Sourcing the authoring verdict from `!Type.IsAbstract` alone is the named defect — the CLR flag is EXPRESS-faithful on every published member but `IfcTransportationDevice`, and only the overlay carries that one. Predefined validity is an EGRESS gate and silent acceptance of an out-of-schema token is the named defect. The schema currency is the SEAM `ReleaseVersion` ranked through its own roster order — a bare `>=` over the SmartEnum, a GeometryGym `ReleaseVersion` in the gate signature, or a hand ordinal table a new seam release leaves unranked are each the deleted form; likewise a raw entity-type string crossing a seam signature.
- Boundary: `AdmitPredefined` is the whole gate and NO whole-model preflight sits beside it — the retired `AuditTarget` folded these same two reads over every `"ifc"`-classified node against a CALLER-CHOSEN target schema, and every entry in this folder emits at `graph.Header.Schema` alone, so it previewed a gate no caller reaches. NAMED LOSS: the accumulated complete-violation set a schema-retarget deliverable decision reads ahead of the emit. WITNESS: `Admits` and `AdmitPredefined` hold the per-node authority, and a retarget entry folds them over `graph.ObjectNodes` in one `Traverse` on the accumulating rail the moment such an entry lands.

```csharp signature
// --- [RUNTIME_PRELUDE] --------------------------------------------------------------------
using System.Collections.Frozen;
using GeometryGym.Ifc;
using LanguageExt;
using Rasm.Drawing;
using Rasm.Element.Classification;
using Rasm.Element.Graph;
using Thinktecture;
using static LanguageExt.Prelude;
using GGRelease = GeometryGym.Ifc.ReleaseVersion;
using Op = Rasm.Domain.Op;
// The seam schema currency the Header carries, disambiguated from GeometryGym.Ifc.ReleaseVersion, which
// rides the GGRelease alias on the release-map and IFC-text codec legs alone.
using ReleaseVersion = Rasm.Element.Graph.ReleaseVersion;

namespace Rasm.Bim.Model;

// --- [TYPES] ------------------------------------------------------------------------------
// The IFC SCHEMA-domain partition the emitter's inheritance DAG resolves, folded to query-grade rows. The
// element disciplines alone cannot legalize the reflected roster: General owns the kernel/spatial/actor/group
// backbone, Controls the IfcDistributionControlElement line, Construction the IfcProcess/IfcResource branch;
// a roster member no claim row reaches FAILS the emit. Designator is the NCS sheet letter each domain draws
// on, composed from the kernel drafting owner rather than re-lettered here — the IFC schema domain is the
// discriminant that survives, and it is COARSER than NCS in two places the rows name.
[SmartEnum<string>]
public sealed partial class IfcDomain {
    public static readonly IfcDomain Architecture   = new("Architecture",   DisciplineDesignator.Architectural);
    public static readonly IfcDomain Structural     = new("Structural",     DisciplineDesignator.Structural);
    // IFC fuses HVAC and fire protection into one schema domain where NCS letters them M and F; the sheet
    // designator takes the dominant M and a fire-protection sheet set re-letters at the sheet owner.
    public static readonly IfcDomain HvacFire       = new("HvacFire",       DisciplineDesignator.Mechanical);
    public static readonly IfcDomain Electrical     = new("Electrical",     DisciplineDesignator.Electrical);
    public static readonly IfcDomain Plumbing       = new("Plumbing",       DisciplineDesignator.Plumbing);
    public static readonly IfcDomain Infrastructure = new("Infrastructure", DisciplineDesignator.Civil);
    public static readonly IfcDomain Geotechnical   = new("Geotechnical",   DisciplineDesignator.Geotechnical);
    public static readonly IfcDomain General        = new("General",        DisciplineDesignator.General);
    // Building controls draw the electrical sheet letter; IFC keeps them a schema domain of their own.
    public static readonly IfcDomain Controls       = new("Controls",       DisciplineDesignator.Electrical);
    public static readonly IfcDomain Construction   = new("Construction",   DisciplineDesignator.Contractor);

    public DisciplineDesignator Designator { get; }
}

// The authoring verdict every committed row carries, replacing the abstractness bool the egress gate and the
// window check read separately. The gate is a ROW COLUMN, not a switch body: Authorable defers to the class
// window, Vocabulary refuses in every release because an EXPRESS-abstract supertype is legal classification
// vocabulary and an illegal egress class. A Draft row is unreachable — the emitter's published-membership
// gate drops draft surface before a row commits.
[SmartEnum<string>]
public sealed partial class EgressEligibility {
    public static readonly EgressEligibility Authorable = new("authorable",
        gate: static (window, schema, cls, key) => window.Covers(schema)
            ? Fin.Succ(unit)
            : Fin.Fail<Unit>(new BimFault.Refused(key, BimScope.Model, BimReason.Unmapped, string.Join(':', new object?[] { "class-out-of-schema", cls, schema.Key }))));

    public static readonly EgressEligibility Vocabulary = new("vocabulary",
        gate: static (_, _, cls, key) => Fin.Fail<Unit>(new BimFault.Refused(key, BimScope.Projection, BimReason.Unmapped, string.Join(':', new object?[] { "abstract-class-at-egress", cls }))));

    [UseDelegateFromConstructor]
    public partial Fin<Unit> Gate(SchemaSpan window, ReleaseVersion schema, string cls, Op key);
}

// The predefined token admitted ONCE at the boundary: the interior reads cases, never the "" / NOTDEFINED /
// USERDEFINED sentinel strings. Admit is the only mint, so the label obligation USERDEFINED carries is
// discharged at admission — the projector authors IFC ObjectType from that label and there is no OBJECTTYPE
// token, so a node carrying none is refused before any downstream arm can substitute a Name for it.
[Union]
public abstract partial record PredefinedToken {
    private PredefinedToken() { }

    public sealed record Canonical : PredefinedToken;
    public sealed record UserDefined(string Label) : PredefinedToken;
    public sealed record Named(string Token) : PredefinedToken;

    public static Fin<PredefinedToken> Admit(string token, string objectType, string cls, Op key) =>
        token.Trim().ToUpperInvariant() switch {
            "" or "NOTDEFINED" => Fin.Succ<PredefinedToken>(new Canonical()),
            "USERDEFINED"      => objectType.Trim() is { Length: > 0 } label
                                      ? Fin.Succ<PredefinedToken>(new UserDefined(label))
                                      : Fin.Fail<PredefinedToken>(new BimFault.Refused(key, BimScope.Model, BimReason.Unmapped, string.Join(':', new object?[] { "predefined-objecttype-miss", cls }))),
            var value          => Fin.Succ<PredefinedToken>(new Named(value)),
        };
}

// --- [TABLES] -----------------------------------------------------------------------------
// ONE release table over the seam roster answers both reads the gates need. Rank IS the roster's DECLARATION
// ORDER — the generated Items list is chronological at its owner — so rank and span are total over every row
// by derivation and a new seam release ranks and spans itself. The per-class window is the SEAM-owned
// SchemaSpan (Graph/element#NODE_MODEL): the row's window and the stamped node span are ONE type [H8].
internal static class IfcSchema {
    private static readonly FrozenDictionary<ReleaseVersion, (int Rank, SchemaSpan Span)> Releases =
        ReleaseVersion.Items.Index().ToFrozenDictionary(
            static row => row.Item, static row => (Rank: row.Index, Span: SchemaSpan.From(row.Item)));

    internal static int Rank(ReleaseVersion value) => Releases[value].Rank;

    // The open-window mint the generated region spells per row; a closed window renders its SchemaSpan pair
    // directly. Naming the seam row at the call site retires the anchor-name/roster-key drift a per-release
    // static field carried, so no render-time name map can exist.
    internal static SchemaSpan Of(ReleaseVersion introduced) => Releases[introduced].Span;

    extension(SchemaSpan span) {
        public bool Covers(ReleaseVersion schema) =>
            Rank(schema) >= Rank(span.IntroducedIn) && span.RemovedIn.Match(Some: removed => Rank(schema) < Rank(removed), None: () => true);
    }
}

// The ONE GG-to-seam release map, read by the emitter AND the runtime ReleaseLower/ReleaseRaise/Sniff
// lowerings, retiring both silent fallbacks. IFC4X4_DRAFT stays unmapped by law — the published-membership
// gate drops draft surface before any lowering, so an unmapped release reaching Lower is a genuine fault: it
// FAILS the emit and rails BimFault.Refused at runtime.
internal static class ReleaseMap {
    public static readonly FrozenDictionary<GGRelease, ReleaseVersion> Lower = new Dictionary<GGRelease, ReleaseVersion> {
        [GGRelease.IFC2X] = ReleaseVersion.Ifc2X3, [GGRelease.IFC2x2] = ReleaseVersion.Ifc2X3, [GGRelease.IFC2x3] = ReleaseVersion.Ifc2X3,
        [GGRelease.IFC4] = ReleaseVersion.Ifc4, [GGRelease.IFC4A1] = ReleaseVersion.Ifc4, [GGRelease.IFC4A2] = ReleaseVersion.Ifc4,
        [GGRelease.IFC4X1] = ReleaseVersion.Ifc4X1, [GGRelease.IFC4X2] = ReleaseVersion.Ifc4X1,
        [GGRelease.IFC4X3_RC1] = ReleaseVersion.Ifc4X3, [GGRelease.IFC4X3_RC2] = ReleaseVersion.Ifc4X3,
        [GGRelease.IFC4X3_RC3] = ReleaseVersion.Ifc4X3, [GGRelease.IFC4X3_RC4] = ReleaseVersion.Ifc4X3,
        [GGRelease.IFC4X3] = ReleaseVersion.Ifc4X3, [GGRelease.IFC4X3_ADD2] = ReleaseVersion.Ifc4X3Add2,
    }.ToFrozenDictionary();

    // The preimage compare is UNDERSCORE-ERASED ordinal-ignore-case: the GG member spells IFC4X3_ADD2 where the
    // seam key spells Ifc4X3Add2, so a bare name compare silently drops the Add2 raise and every Ifc4X3Add2
    // egress target faults as unmapped; erasing "_" restores the exact preimage while the RC/A-suffixed members
    // stay excluded. The seam Ifc5 therefore has NO GG image — an Ifc5 egress target is the typed fault.
    public static readonly FrozenDictionary<ReleaseVersion, GGRelease> Raise =
        Lower.Where(static pair => string.Equals(pair.Key.ToString().Replace("_", ""), pair.Value.Key, StringComparison.OrdinalIgnoreCase))
             .ToFrozenDictionary(static pair => pair.Value, static pair => pair.Key);
}

// --- [MODELS] -----------------------------------------------------------------------------
// The per-token availability row: IntroducedIn is the SEAM ReleaseVersion sourced dotted-pin-first from the
// committed EXPRESS-diff index (the correction tier: on IfcWallTypeEnum GG annotates the 2x3-era
// STANDARD/POLYGONAL/ELEMENTEDWALL as IFC4 and leaves the IFC4-added MOVABLE/PARAPET/PARTITIONING unannotated
// — schema-divergent both ways), else the per-field GG [VersionAdded] lowered through ReleaseMap, else the
// class floor. A token absent from every published enum never commits a row.
public readonly record struct PredefinedRow(string Token, ReleaseVersion IntroducedIn);

[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinalIgnoreCase, string>]
[KeyMemberComparer<ComparerAccessors.StringOrdinalIgnoreCase, string>]
public sealed partial class IfcClass {
    // The classification SYSTEM key every seam Object node this taxonomy stamps carries — declared at the S0
    // vocabulary owner because a bare "ifc" literal beside a `==` compares outside the OrdinalIgnoreCase key space
    // the rows themselves declare, so "IFC" and "ifc" read as two systems. Semantics' ClassificationSystem row and
    // every view-side system gate compose this one token.
    public const string System = "ifc";

    // The region between these markers is COMMITTED GENERATOR OUTPUT, replaced whole by
    // Model/emitter#REGENERATION on a GeometryGym pin bump or a new IFC release. ONE exemplar row stands as
    // the line-shape contract; Model/emitter#TAXONOMY_EMITTER Render emits exactly this shape, so the marker
    // pair plus this row reconstruct any member. A hand edit here dies at the next regeneration.
    // <generated-rows>
    public static readonly IfcClass ActionRequest = new("IfcActionRequest", IfcDomain.General, IfcSchema.Of(ReleaseVersion.Ifc2X3), EgressEligibility.Authorable, Seq<PredefinedRow>());
    // <end generated-rows>

    public IfcDomain Domain { get; }
    public SchemaSpan Span { get; }
    public EgressEligibility Eligibility { get; }
    public Seq<PredefinedRow> ValidPredefined { get; }

    // Every row's seam classification value DERIVED once from the primary (System, Key) correspondence — the pair
    // the projector stamps onto a Node.Object. Both tokens are roster constants no call site can blank, so the seam's
    // ONE admission runs at roster init and a rail-free authoring helper reads the admitted value instead of
    // re-running the door per element or bypassing it with a throwing Classification.Create.
    private static readonly Op Seat = Op.Of(name: nameof(IfcClass));

    private static readonly FrozenDictionary<IfcClass, Classification> EntityClasses =
        Items.ToFrozenDictionary(static row => row, static row => Classification.Of(System, row.Key, Seat).ThrowIfFail());

    public Classification EntityClass => EntityClasses[this];

    // The Option-lift over the generated bool/out try-pattern — the ONE lookup spelling every consumer folds;
    // the generated TryGet(string?, out IfcClass?) stays the raw seam beneath it, never a second resolver.
    public static Option<IfcClass> TryGet(string entityType) =>
        TryGet(entityType, out IfcClass? row) && row is { } hit ? Some(hit) : None;

    // The typed element-class-miss lifts BARE onto the Fin rail (band 2600 owns the generated Code; no .ToError()
    // hop). A permissive ingress instead reads TryGet(entityType).IfNone(BuildingElementProxy).
    public static Fin<IfcClass> Resolve(string entityType, Op key) =>
        TryGet(Canonical(entityType)).ToFin(new BimFault.Refused(key, BimScope.Model, BimReason.Unmapped, string.Join(':', new object?[] { "element-class-miss", entityType })));

    // IFC4 collapsed the retired *StandardCase/*ElementedCase implementation subtypes into the base class plus
    // PredefinedType; ParserIfc.IdentifyIfcClass does NOT fold them, so a 2x3 IfcWallStandardCase the projector
    // reads off p.GetType().Name resolves the IfcWall row here rather than aborting the import on the most
    // common real entity. The subtypes still hold committed rows, and the fold decides WHO reads them: the
    // permissive TryGet ingress resolves the subtype row itself and stamps its OWN SchemaSpan as the node's
    // span (the source entity's true window [H8]), while every Resolve consumer reads the folded BASE row, so a
    // deprecated 2x3 StandardCase re-emits as the surviving base class instead of authoring a retired form.
    private static readonly FrozenSet<string> CaseSuffixes =
        new[] { "StandardCase", "ElementedCase" }.ToFrozenSet(StringComparer.Ordinal);

    private static string Canonical(string entityType) =>
        CaseSuffixes.AsIterable()
            .Find(suffix => entityType.EndsWith(suffix, StringComparison.Ordinal))
            .Match(Some: suffix => entityType[..^suffix.Length], None: () => entityType);

    // The ONE class-availability read both the egress author and any future retarget fold take: the row's
    // eligibility column decides against the target Header schema, so abstractness and window refusals retain
    // distinct detail evidence from one call site instead of two.
    public Fin<Unit> Admits(ReleaseVersion schema, Op key) => Eligibility.Gate(Span, schema, Key, key);

    // The egress gate, PER-TOKEN [PREDEFINED_TOKEN_RULING][H8]: the class verdict gates the entity, the token
    // admits ONCE into its typed case, then each set member gates on ITS OWN IntroducedIn rank — WAVEWALL on an
    // IFC2x3 emit faults predefined-out-of-schema. An empty set constrains nothing.
    public Fin<string> AdmitPredefined(string token, string objectType, ReleaseVersion schema, Op key) =>
        Admits(schema, key)
            .Bind(_ => PredefinedToken.Admit(token, objectType, Key, key))
            .Bind(admitted => admitted.Switch(
                state: (Row: this, schema, key),
                canonical:   static (_, _) => Fin.Succ(PredefinedType.NotDefined.Token),
                userDefined: static (_, _) => Fin.Succ("USERDEFINED"),
                named:       static (s, n) => s.Row.Ranked(n.Token, s.schema, s.key)));

    private Fin<string> Ranked(string token, ReleaseVersion schema, Op key) =>
        ValidPredefined.IsEmpty
            ? Fin.Succ(token)
            : ValidPredefined.Find(row => row.Token == token).Match(
                Some: row => IfcSchema.Rank(schema) >= IfcSchema.Rank(row.IntroducedIn)
                    ? Fin.Succ(token)
                    : Fin.Fail<string>(new BimFault.Refused(key, BimScope.Model, BimReason.Unmapped, string.Join(':', new object?[] { "predefined-out-of-schema", Key, token, schema.Key }))),
                None: () => Fin.Fail<string>(new BimFault.Refused(key, BimScope.Model, BimReason.Unmapped, string.Join(':', new object?[] { "predefined-reject", Key, token }))));
}
```

## [03]-[REPRESENTATION_KEYS]

- Owner: `IfcRepresentation` the geometry-reference content-keyer [M2] projecting an `IfcProduct`/`IfcTypeProduct` representation set onto the seam `RepresentationContentHash` keyed map — `RepresentationIdentifier` (`Axis`/`Body`/`Box`/`FootPrint`) → the kernel seed-zero `XxHash128` content hash of the representation STEP — so the seam `Object` node references its geometry by content key per representation, never an IFC name leak and never an in-process BRep evaluation. Bim owns the IFC representation mapping and the `IfcRepresentationMap`/`IfcMappedItem` instancing per representation; the seam holds the neutral keyed map.
- Entry: `IfcRepresentation.Keys(IfcObjectDefinition? definition)` is ONE polymorphic content-keyer discriminating on the input shape — an occurrence `IfcProduct` folds its `IfcProductDefinitionShape.Representations` into the map keyed by `RepresentationIdentifier`; a type `IfcTypeProduct` folds its `RepresentationMaps` `IfcMappedItem` instancing onto the same map so an occurrence instancing a type representation shares the content key rather than re-keying; any other definition (or a null) yields `RepresentationContentHash.Empty`. There is no `KeysOf`/`MapKeys` family — the occurrence-versus-type distinction is the input case, never a name suffix.
- Auto: the occurrence arm reads each `IfcShapeRepresentation.RepresentationIdentifier`, serializes the representation to its STEP record line, and content-keys it through the kernel `ContentHash.Of` field-stream leg — the tag and the STEP text each cross `CanonicalWriter.String`, which prefixes its int32-LE UTF-8 byte count, so a tag/STEP pair cannot collide with any other split of the same characters [PREIMAGE_FRAMING]. The type arm keys the `IfcRepresentationMap.MappedRepresentation` once so every `IfcMappedItem` occurrence referencing that SAME map entity shares the content key — the instanced-geometry reuse the `Exchange/reconstruct#RECONSTRUCTION` lane mirrors, never a per-occurrence re-key.
- Packages: GeometryGymIFC_Core, Rasm.Element, `Rasm` (`Rasm.Domain` `ContentHash`/`CanonicalWriter`), LanguageExt.Core
- Growth: a new representation identifier is one `RepresentationIdentifier` key the map carries; the content-key seed is the kernel's single seed-zero `XxHash128`; never a second hasher and never a geometry blob on the seam node — only the content key.
- Boundary: the geometry reference is the content-keyed map [M2] and an inlined geometry blob, a stored `GeometryHandle`, or an IFC representation name on the seam node is the deleted form; the content key composes the kernel hasher over `CanonicalWriter` and a second hasher (or the strata-violating `Rasm.Compute` `InterchangeIdentity` consumed up-stratum) is the named defect [H7]; a separator-joined preimage is the retired form — a space inside a STEP string shifts the split, so the framing is the writer's, never a `string.Concat`; the representation STEP is keyed, NOT evaluated — an in-process BRep tessellation here is the named seam violation (geometry realization routes the `Exchange/tessellation#TESSELLATION_BRIDGE` companion rail); the type representation-map instancing shares one content key across occurrences and a per-occurrence re-key is the deleted form; the content-stable realized-geometry identity across distinct entities is the kernel `GeometryHash` at the GLB wire, a separate key this serialization key never duplicates.

```csharp signature
// --- [RUNTIME_PRELUDE] --------------------------------------------------------------------
using GeometryGym.Ifc;
using LanguageExt;
using Rasm.Domain;
using Rasm.Element.Classification;
using Rasm.Element.Graph;
using static LanguageExt.Prelude;

namespace Rasm.Bim.Model;

// --- [OPERATIONS] -------------------------------------------------------------------------
public static class IfcRepresentation {
    // ONE polymorphic entry over IfcObjectDefinition discriminates the occurrence product (its direct
    // representations) from the type product (the IfcRepresentationMap instanced-geometry library) on the input
    // case. The seam Object node references geometry by this content key only.
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

    // Two LENGTH-FRAMED fields through the kernel writer, tag first, so a direct shape and a mapped library
    // shape never collide and no separator can shift the split. ONE hasher: no second hasher, no up-stratum
    // InterchangeIdentity, and the realized-geometry GeometryHash at the GLB wire stays separate.
    private static UInt128 RepKey(string tag, BaseClassIfc entity) =>
        ContentHash.Of((Tag: tag, Entity: entity), static (state, writer) =>
            writer.String(state.Tag).String(state.Entity.StringSTEP()));
}
```

## [04]-[RESEARCH]

(none)
