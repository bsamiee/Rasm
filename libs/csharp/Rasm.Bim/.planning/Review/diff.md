# [BIM_MODEL_DIFF]

The GlobalId-stable federation diff over the seam graph: one `ModelDiff` change-set carries the baseline and revision `ContentAddress.OfGraph` identities and folds both `Rasm.Element/Graph/element#ELEMENT_GRAPH` snapshots into the `ElementChange` added/removed/modified/moved arms, joining by the Bim-stored `Rasm.Element/Graph/element#NODE_MODEL` `Node.Object.ExternalId` (the IFC `GlobalId` [H6] — the ONE identity two federated submissions share, because the neutral kernel `NodeId` is minted afresh per ingest and never coincides across parties) and classifying each matched pair by the seam `Rasm.Element/Projection/address#CONTENT_ADDRESS` content and placement keys so an unchanged element dedups by content address and a re-check bakes only the changed elements. PLACEMENT-SPACE LAW: the placement key folds the element's OBJECT PLACEMENT TRANSFORM explicitly beside its content-hashed representations, and every geometry hash it reads is stated in the representation's OWN local space — an IFC representation is authored local and positioned by `IfcLocalPlacement`, so a rigid relocation leaves every representation hash byte-identical and a placement key over hashes ALONE reads a moved element as unchanged. The transform enters the key through the seam `CanonicalWriter` at model-space tolerance, so the `Moved` discriminant answers the question its name asks. The diff consumes two `ElementGraph` snapshots as settled vocabulary and mints no second element shape; the consumer element is the `Rasm.Element/Graph/element#ELEMENT_GRAPH` `Bake(objectNode)` fold, the retired `BimModel`/`BimElement` snapshot pair GONE.

The diff is the cross-party twin of two same-lineage owners and re-derives neither. The `Rasm.Persistence/Version/merge#STRUCTURAL_DIFF` `StructuralMerge` is the NodeId-keyed (re-ingest `Reconcile`-aligned on `ExternalId`) version-lineage THREE-way merge over one model's history; the `Review/versioning#VERSION_GRAPH` commit-DAG is the branching revision graph. This page is the PAIRWISE two-way federation diff over two INDEPENDENT submissions whose neutral `NodeId`s never coincide, so the join is the IFC `GlobalId` directly (no `Reconcile`); all three compose the one `Generator.Equals` `Inequalities` member-diff substrate and the one seam `ContentAddress` codec, none re-deriving another. Every diff rejection lifts the `Model/faults#FAULT_BAND` `BimFault` band BARE (the `Expected`-derived case IS the `Error`, no `.ToError()` hop, the ctor `(Op key, string detail)`). The page is HOST-LOCAL; the `ModelDiff.Encode`/`Decode` cross-runtime wire payload is HOST-FREE.

## [01]-[INDEX]

- [02]-[MODEL_DIFF]: the `ModelDiff` change-set, the `ElementChange` closed `[Union]` (Added/Removed/Modified/Moved/Split/Merged) with the `ChangeKind` token projection that also owns the wire discriminators, the `ElementFingerprint` content/placement keys the join dedups on, the `Generator.Equals` `Inequalities` `AspectDelta` member-level delta the `Modified` arm carries (each delta shaped by the `DeltaShape` terminal-segment token and valued by the typed `DeltaValue` leaf), and the `ModelDiff.Encode`/`Decode` host-free cross-runtime projection.
- [03]-[AUDIT]: the chained content-addressed `AuditEntry` log folding `ModelDiff` change-sets across a version sequence into a tamper-evident per-element mutation trail, the `AuditTrail.For(globalId)` lifecycle query, and `AuditTrail.Verify()`.

## [02]-[MODEL_DIFF]

- Owner: `ModelDiff` carries baseline and revision graph addresses with one change set and unchanged count.
- Owner: `ElementChange` closes `Added`, `Removed`, `Modified`, `Moved`, `Split`, and `Merged` over IFC `GlobalId`.
- Owner: `ChangeKind` projects each change arm into audit and wire vocabularies.
- Owner: `ElementFingerprint` carries the `GlobalId`, content key, and placement key used by the join.
- Owner: `PlacementPose` carries the rigid frame a `Moved` arm needs without re-fetching either snapshot.
- Owner: `AspectDelta` carries member path, change shape, and typed before/after leaves for `Modified`.
- Entry: `ModelDiff.Between(ElementGraph baseline, ElementGraph revision, Op key)` folds the two snapshots into one `ModelDiff` — a `GlobalId` present in the revision but not the baseline is `Added`, present in the baseline but not the revision is `Removed`, present in both with a differing content key is `Modified` (or `Moved` when only the placement key differs), and present in both with both keys identical dedups as unchanged; the added/removed partition then re-folds by CONTENT key — the content key excludes geometry, so one source and its fragments carry ONE content signature — lifting a one-removed-to-many-added group onto `Split` and a many-removed-to-one-added group onto `Merged`, each carrying its counterpart set and leaving the add/remove partition; `Fin<T>` because the `Modified` enrichment bakes the changed elements through `Rasm.Element/Graph/element#ELEMENT_GRAPH` `Bake` (which rails `Rasm.Element/Projection/fault#FAULT_BAND` `ElementFault` on a corrupt subgraph), so an unchanged element never bakes and a re-check costs only the changed elements. `ModelDiff.Encode(diff, key)`/`Decode(json, key)` is the host-free cross-runtime projection and `ModelDiff.Fingerprint(graph, node)` the per-element content fingerprint the `Review/versioning#VERSION_GRAPH` commit-DAG and this diff both key on.
- Auto: `Between` `Federate`s each graph into a `GlobalId`-keyed map over the `ExternalId`-bearing `Object` nodes (the `Review/coordination#COORDINATION` `ExternalId` `Choose`-discard-`None` law — an authored Object with no IFC `GlobalId` sits off the federation surface, never a fault), `Fingerprint`s each through the seam `ContentAddress`, then partitions the common set: a differing `ContentKey` is `Modified`, an equal `ContentKey` with a differing `PlacementKey` is `Moved`, both equal is unchanged. The content key folds the `Object`'s semantic head (kind/classification/predefined/name/tag) with the order-independent content addresses of its bound non-`Object` nodes (`PropertySet`/`QuantitySet`/`Material`/`Assessment`/`Appearance`/`Coverage`) and its outgoing-edge structure; the placement key folds the `Object`'s geometry through the `RepresentationContentHash` map ALONE — EVERY geometry content-hashed there, the heavy display `Body` AND the lightweight analytical `Axis`/`FootPrint` the structural/energy disciplines resolve one-hop by content key — so a relocation moves the geometry bytes, the content hashes, and thus the placement key, while the semantic content key stays stable; an inline `BoundaryPolygon`/`Axis` coordinate read is the named seam violation (the seam carries no raw coordinate field — `Graph/element#NODE_MODEL` M2). The `Modified` arm carries BOTH currencies — the content-key pair AND the placement-key pair, so a content edit that also relocates the element keeps the axis the `Review/versioning#VERSION_GRAPH` merge weighs — plus `Generator.Equals` `Inequalities` over the two baked `Element`s as `AspectDelta` rows (the `Id`/`ExternalId`/`History`/`Parts` noise axes excluded at the `Rasm.Element` owner's own `[IgnoreEquality]` declarations, so the comparer composes bare and every consumer agrees by construction), each row's terminal `MemberPathSegment` projected onto the `DeltaShape` token so a downstream consumer reads the exact `Properties[Pset].FireRating` member that moved AND the shape of the move — a scalar `Replace` distinguished from an ordered-collection `Index`, a keyed-bag `Key`, or a set-membership `Added`/`Removed` — with each side's leaf kept typed, not an opaque content-key delta and not two rendered strings.
- Receipt: the `ModelDiff` change-set is the incremental federation evidence; a `Review/issues#BCF_ARCHIVE` `BcfTopic` anchors a `BcfViewpoint.SelectedGlobalIds` on the `Modified`/`Moved` element `GlobalId`s this diff names, the `Review/coordination#COORDINATION` `Coordination.Between` folds two change-sets into the downstream-affected element/task/cost sets, and the `Review/versioning#VERSION_GRAPH` `BimCommit` keys its `Map<string, ElementFingerprint>` on the SAME `ElementFingerprint` so a commit, a diff, and the audit chain carry one content-key identity; the `ModelDiff.Encode` payload is the one cross-runtime contract the `ts:ui/bcf-anchor` live-binding decodes to highlight the changed `GlobalId`s.
- Packages: Rasm.Element, Generator.Equals, Thinktecture.Runtime.Extensions, Thinktecture.Runtime.Extensions.Json, LanguageExt.Core, Rasm, BCL `System.Text.Json`
- Growth: a new change kind is one `ElementChange` union arm plus one `ChangeKind` row plus one `[JsonDerivedType]` row reading that row's own `nameof` plus one `AuditTrail.Keys` arm (an unregistered leaf fails serialization loudly, never a silent slice); a new content dimension is one column folded into the content key over the same seam `ContentAddress` codec; the join keys by `GlobalId` plus the content/placement keys so a new identity dimension is a content-key field, never a second identity scheme; a new delta projection is one richer `AspectDelta` over the same `Inequalities`, a new change shape is one `DeltaShape` row plus its factory-keyed correspondence entry, and a new leaf carrier is one `DeltaValue` arm plus its wire row; never a per-change-kind type and never a parallel diff record.
- Boundary: the federation join is the `Node.Object.ExternalId` IFC `GlobalId` [H6] and a join on the neutral `NodeId` is the deleted form — two independent submissions re-mint rooted `NodeId`s, so only the `GlobalId` is cross-party stable; the content and placement keys are the seam `ContentAddress` over the ONE `Rasm.Element/Projection/address#CONTENT_ADDRESS` codec [H7], and the retired `Rasm.Compute/Runtime/codecs` `InterchangeIdentity.Key` consumed up-stratum AND a hand-rolled `XxHash128`/`Encoding.UTF8` string-join hasher are the named defects — the diff content bytes and the `NodeId` content hash share the one seam projection; the `Moved` arm is distinguished from `Modified` by the placement-key delta (the placement TRANSFORM plus the geometry bucket — representations and analytical geometry — moved while the content bucket held), and collapsing the two buckets is the deleted form; the `Moved` arm carries the two rigid `PlacementPose` frames beside the key pair — a content address is opaque, so a key-pair-only `Moved` forced every peer to re-fetch both snapshots to draw a relocation it had just been told about, and the pose crosses in the layout the seam `ObjectWire` placement field already carries so the JSON and protobuf crossings state one frame; a consumer re-deriving a transform from the placement key is the deleted form; the placement key folds the transform EXPLICITLY and a key over representation hashes alone is the deleted form — an IFC representation is authored in local space and positioned by `IfcLocalPlacement`, so a rigid relocation leaves every hash byte-identical and the `Moved` arm becomes unreachable while the element silently reads unchanged; `Split`/`Merged` derive from CONTENT-key matching across the added/removed partition (the content key excludes geometry, so a source and its fragments share one signature while their placements diverge), one-to-many lifting `Split` and many-to-one `Merged`, and reporting a split as unrelated adds beside an unrelated remove is the deleted form that discards the only correspondence the federation surface holds — a 1:1 content match is a re-identification the add/remove pair already states and an N:M group names no source and no survivor, so both stay plain; the `Modified` delta is the `Generator.Equals` `Inequalities` member-path projection carrying the terminal segment kind as the typed `DeltaShape` token and each side's leaf as a typed `DeltaValue` — a string-formatted whole-record diff, a consumer re-parsing the rendered `Path` brackets to recover the change shape, a `"<absent>"` string sentinel indistinguishable from a real value, and a `_` floor that reads an unmodelled foreign segment kind as a scalar replace are the deleted forms; the noise axes are `[IgnoreEquality]` declarations on the `Rasm.Element` owner and the wire discriminators read through `nameof`, so a call-site member-name roster — hardcoded or `nameof`-derived — is the deleted form beside the owner's own exclusion; the `Modified` arm carries the placement pair beside the content pair, matching the `Review/versioning#VERSION_GRAPH` law that weighs both axes on the same currency, and a content-only `Modified` that discards a simultaneous relocation is the deleted form; the `ElementChange` family is a closed `[Union]` and a per-change-kind class is the deleted form; the consumer element is the `Bake` fold and the retired `BimModel`/`BimElement` snapshot pair is GONE (a diff that re-stores a second element record off the seam graph is the deleted form); the cross-runtime wire rides `ModelDiff.Encode`/`Decode` (HOST-FREE — `GlobalId` strings, the seam `ContentAddress` keys, the typed deltas, never a host geometry type) — the `[Union]` crossing on its per-leaf `[JsonDerivedType]` `kind` discriminant (a regular `[Union]` carries no key metadata, so the `ThinktectureJsonConverterFactory` cannot convert it — a factory-only wire that slices the abstract root on write and faults on read is the deleted illusory form), the keyed `[SmartEnum]`/`[ValueObject]` owners on the factory's key conversion — and the retired `Exchange/wire#WIRE_PROJECTION` `BimWireContext`/`BimWireOptions.Json` (GONE — the strata-leaking generic-model serializer is retired, `wire.md` now owning the `IfcWire` IFC interchange wire) plus a parallel `DiffWire` record duplicating `ModelDiff`'s shape are the deleted forms — the seam-graph snapshot wire is `Rasm.Persistence/Element/codec#CODEC_AXIS` `SnapshotCodec`'s, not minted here; this page is the cross-party PAIRWISE diff and re-deriving the `Rasm.Persistence/Version/merge#STRUCTURAL_DIFF` NodeId-keyed three-way merge here is the deleted form.

```csharp signature
// --- [RUNTIME_PRELUDE] --------------------------------------------------------------------
using System.Collections.Frozen;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;
using Generator.Equals;
using LanguageExt;
using Rasm.Element.Classification;
using Rasm.Element.Graph;
using Rasm.Element.Projection;
using Rasm.Element.Properties;
using Rasm.Element.Relations;
using Thinktecture;
using Thinktecture.Text.Json.Serialization;
using static LanguageExt.Prelude;
using Op = Rasm.Domain.Op;

namespace Rasm.Bim;

// --- [TYPES] ------------------------------------------------------------------------------
// The neutral change-kind token an audit row persists and a TS decode switches on — the [SmartEnum] projection
// of the ElementChange union case (the Relations/relation#EDGE_ALGEBRA Relationship.Kind idiom), never a
// stringly "added"/"removed" literal and never a per-call-site discriminant. The rows are ALSO the wire
// discriminator roster: ElementChange's [JsonDerivedType] rows spell these same keys, so one vocabulary owns both
// the audit token and the polymorphic wire tag.
[SmartEnum<string>]
public sealed partial class ChangeKind {
    public static readonly ChangeKind Added = new(nameof(Added));
    public static readonly ChangeKind Removed = new(nameof(Removed));
    public static readonly ChangeKind Modified = new(nameof(Modified));
    public static readonly ChangeKind Moved = new(nameof(Moved));
    public static readonly ChangeKind Split = new(nameof(Split));
    public static readonly ChangeKind Merged = new(nameof(Merged));
}

// The member-change SHAPE token: the seam projection of the foreign Generator.Equals MemberPathSegmentKind read
// off a delta path's TERMINAL segment, so a TS/BCF consumer distinguishes an ordered-collection Index, a keyed-bag
// Key, and a set-membership Added/Removed from a scalar Replace without re-parsing the rendered Path brackets.
[SmartEnum<string>]
public sealed partial class DeltaShape {
    public static readonly DeltaShape Replace = new("replace");
    public static readonly DeltaShape Index   = new("index");
    public static readonly DeltaShape Key     = new("key");
    public static readonly DeltaShape Added   = new("added");
    public static readonly DeltaShape Removed = new("removed");
    public static readonly DeltaShape Unknown = new("unknown");

    // The one seam correspondence, keyed by the kinds the foreign FACTORIES project rather than by transcribed
    // enum case spellings — a foreign case rename breaks this table at compile time where a transcription would
    // silently re-route to the wrong shape. Property and Field roster EXPLICITLY as the scalar Replace, so the
    // table is TOTAL over the vocabulary as it stands and a kind no factory mints reads Unknown: the prior `_`
    // floor made an unmodelled future shape indistinguishable from a genuine scalar replace.
    static readonly FrozenDictionary<MemberPathSegmentKind, DeltaShape> ByKind =
        new Dictionary<MemberPathSegmentKind, DeltaShape> {
            [MemberPathSegment.Property(string.Empty).Kind] = Replace,
            [MemberPathSegment.Field(string.Empty).Kind]    = Replace,
            [MemberPathSegment.Index(0).Kind]               = Index,
            [MemberPathSegment.Key(string.Empty).Kind]      = Key,
            [MemberPathSegment.Added().Kind]                = Added,
            [MemberPathSegment.Removed().Kind]              = Removed,
        }.ToFrozenDictionary();

    public static DeltaShape Of(MemberPathSegment segment) => ByKind.GetValueOrDefault(segment.Kind, Unknown);
}

// The typed delta leaf. A rendered string destroys exactly the evidence a downstream consumer computes on, so a
// dimensioned leaf keeps its whole seam MeasureValue (SI magnitude, QuantityType, 7-vector) and a content-keyed
// leaf its ContentAddress; only a leaf with no richer carrier falls to the canonical text its own owner publishes
// (PropertyValue.Render for a Pset value, ToString otherwise). Absent is the TYPED absence a membership delta
// carries on the side it has no value for, replacing the "<absent>" sentinel a real string value could forge; the
// wire needs a discriminated absence because no LanguageExt Option converter rides this payload.
[Union]
[JsonPolymorphic(TypeDiscriminatorPropertyName = "kind", UnknownDerivedTypeHandling = JsonUnknownDerivedTypeHandling.FailSerialization)]
[JsonDerivedType(typeof(Measure), "measure")]
[JsonDerivedType(typeof(Address), "address")]
[JsonDerivedType(typeof(Label), "label")]
[JsonDerivedType(typeof(Absent), "absent")]
public abstract partial record DeltaValue {
    private DeltaValue() { }

    public sealed record Measure(MeasureValue Value) : DeltaValue;
    public sealed record Address(ContentAddress Value) : DeltaValue;
    public sealed record Label(string Value) : DeltaValue;
    public sealed record Absent : DeltaValue;

    // The one leaf projection: the Inequality carries object? because the foreign comparer walks every member
    // type, so this is the boundary that re-types it. A PropertyValue.Measure unwraps to its MeasureValue rather
    // than rendering, because the quantity identity is the whole point of keeping the leaf typed.
    public static DeltaValue Of(object? leaf) => leaf switch {
        null                       => new Absent(),
        MeasureValue measure       => new Measure(measure),
        PropertyValue.Measure p    => new Measure(p.Value),
        ContentAddress address     => new Address(address),
        PropertyValue value        => new Label(value.Render()),
        _                          => new Label(leaf.ToString() ?? string.Empty),
    };
}

// --- [MODELS] -----------------------------------------------------------------------------
// One member-level delta projected from the Generator.Equals Inequalities member diff: Path is the dotted/bracketed
// MemberPath (e.g. "Properties[Pset_WallCommon].FireRating"), Shape the terminal-segment change-shape token, and
// Before/After the TYPED leaf values — the typed evidence the Modified arm carries so a BCF topic anchors the exact
// aspect that changed, how, and with what magnitudes, not "something changed" and never two strings a quantity
// consumer must re-parse.
public readonly record struct AspectDelta(string Path, DeltaShape Shape, DeltaValue Before, DeltaValue After);

// The per-element content fingerprint the join dedups on: the IFC GlobalId plus the seam ContentAddress content
// and placement keys (the ONE Projection/address#CANONICAL_WRITER codec hashed by the ONE
// Projection/address#CONTENT_ADDRESS hasher — never a second hasher). Two fingerprints
// are equal iff the same element addresses identically, so an unchanged element never enters the change set; the
// SAME carrier the Review/versioning#VERSION_GRAPH BimCommit keys its fingerprint map on.
public readonly record struct ElementFingerprint(string GlobalId, ContentAddress ContentKey, ContentAddress PlacementKey);

// The rigid pose a Moved entry crosses WHOLE: the nine ordered doubles the seam PlacementTransform holds, in the
// EXACT layout the seam Graph/wire ObjectWire optional placement field flattens, so this JSON crossing and the
// protobuf one state one frame and a peer reading either renders the same relocation. Plain doubles because a seam
// [ComplexValueObject] exposes no key for ThinktectureJsonConverterFactory to bind and Vector3 is not a wire shape;
// Of lowers the seam value verbatim, so the pose is a projection and never a second placement authority.
public readonly record struct PlacementPose(
    double LocationX, double LocationY, double LocationZ,
    double AxisX, double AxisY, double AxisZ,
    double RefDirectionX, double RefDirectionY, double RefDirectionZ) {
    public static PlacementPose Of(PlacementTransform frame) =>
        new(frame.Location.X, frame.Location.Y, frame.Location.Z,
            frame.Axis.X, frame.Axis.Y, frame.Axis.Z,
            frame.RefDirection.X, frame.RefDirection.Y, frame.RefDirection.Z);
}

// The closed federation change family — each arm carries the IFC GlobalId through the base accessor and its own
// typed evidence: Added/Removed the Classification + PredefinedType + content key, Modified BOTH currencies (the
// baseline/revision content keys AND the placement pair, because a content edit that also relocates the element is
// one change and dropping its placement axis loses the same evidence the version merge weighs), Moved the placement
// key pair PLUS the two rigid poses the relocation ran between. Kind projects the neutral token. The wire discriminant is the per-leaf [JsonDerivedType]
// row whose string is READ off the ChangeKind row rather than re-typed (a regular [Union] carries no key metadata,
// so ThinktectureJsonConverterFactory refuses it — the abstract root would slice on write and fault on read
// without the polymorphic rows); an unregistered leaf FAILS serialization, and the computed Kind is [JsonIgnore]d
// so the one "kind" property on the wire is the discriminator itself.
[Union]
[JsonPolymorphic(TypeDiscriminatorPropertyName = "kind", UnknownDerivedTypeHandling = JsonUnknownDerivedTypeHandling.FailSerialization)]
[JsonDerivedType(typeof(Added), nameof(ChangeKind.Added))]
[JsonDerivedType(typeof(Removed), nameof(ChangeKind.Removed))]
[JsonDerivedType(typeof(Modified), nameof(ChangeKind.Modified))]
[JsonDerivedType(typeof(Moved), nameof(ChangeKind.Moved))]
[JsonDerivedType(typeof(Split), nameof(ChangeKind.Split))]
[JsonDerivedType(typeof(Merged), nameof(ChangeKind.Merged))]
public abstract partial record ElementChange {
    private ElementChange() { }

    public abstract string GlobalId { get; }

    [JsonIgnore]
    public ChangeKind Kind => Switch(
        added:    static _ => ChangeKind.Added,
        removed:  static _ => ChangeKind.Removed,
        modified: static _ => ChangeKind.Modified,
        moved:    static _ => ChangeKind.Moved,
        split:    static _ => ChangeKind.Split,
        merged:   static _ => ChangeKind.Merged);

    public sealed record Added(string GlobalId, Classification Class, PredefinedType Predefined, ContentAddress Content) : ElementChange;
    public sealed record Removed(string GlobalId, Classification Class, PredefinedType Predefined, ContentAddress Content) : ElementChange;
    public sealed record Modified(string GlobalId, ContentAddress BaselineContent, ContentAddress RevisionContent, ContentAddress BaselinePlacement, ContentAddress RevisionPlacement, ImmutableArray<AspectDelta> Deltas) : ElementChange;
    // Moved carries the placement KEYS (the discriminant that classified it) AND the two POSES the relocation ran
    // between: a content address is opaque, so a peer holding key pairs alone must re-fetch both snapshots to draw
    // the move it was just told about. A pose is NULL where the node declares no placement — a Moved verdict also
    // arises from a re-hashed representation under an absent frame — and null cannot forge a value here because a
    // real pose is a JSON object, so the DeltaValue.Absent discriminated-absence precedent buys nothing.
    public sealed record Moved(string GlobalId, ContentAddress BaselinePlacement, ContentAddress RevisionPlacement,
        PlacementPose? BaselinePose, PlacementPose? RevisionPose) : ElementChange;
    // Re-identification: the CONTENT key excludes geometry, so a source and its fragments share ONE content
    // signature while their placement keys diverge — that is what makes a split recoverable from the add/remove
    // partition at all. GlobalId is the SOURCE for a Split and the SURVIVOR for a Merged; the counterpart set is
    // the other side of the same correspondence, ImmutableArray because the wire carries no LanguageExt converter.
    public sealed record Split(string GlobalId, ContentAddress Content, ImmutableArray<string> Into) : ElementChange;
    public sealed record Merged(string GlobalId, ContentAddress Content, ImmutableArray<string> From) : ElementChange;
}

public sealed record ModelDiff(
    ContentAddress Baseline,
    ContentAddress Revision,
    Seq<ElementChange> Changes,
    int UnchangedCount) {
    // The pairwise federation diff: join two ElementGraph snapshots by the Bim-stored Node.Object.ExternalId (the IFC
    // GlobalId [H6] — the one cross-party identity, the neutral NodeId being local per ingest), classify each matched
    // pair by the content/placement keys (unchanged when both match, Moved when only the placement key moved, Modified
    // when the content key moved), and enrich each Modified arm with the Generator.Equals member delta over the two
    // baked Elements. Fin because the Modified enrichment bakes the changed elements (Bake rails ElementFault on a
    // corrupt subgraph); an unchanged element (matching content key) never bakes, so a re-check costs only the changes.
    public static Fin<ModelDiff> Between(ElementGraph baseline, ElementGraph revision, Op key) =>
        from prior in Federate(baseline, key)
        from next in Federate(revision, key)
        from diff in Classify(baseline, revision, prior, next, key)
        select diff;

    static Fin<ModelDiff> Classify(
        ElementGraph baseline, ElementGraph revision,
        Map<string, (Node.Object Obj, ElementFingerprint Fp)> prior,
        Map<string, (Node.Object Obj, ElementFingerprint Fp)> next, Op key) {
        ContentAddress baselineAddress = ContentAddress.OfGraph(baseline);
        ContentAddress revisionAddress = ContentAddress.OfGraph(revision);
        // The add/remove partition re-folds by CONTENT key before it lands: the content key excludes geometry, so
        // a source element and the fragments it became carry ONE content signature while their placement keys
        // diverge — that correspondence is the only evidence of a split or a merge the federation surface holds,
        // and reporting the fragments as unrelated adds beside an unrelated remove discards it. One removed to
        // MANY added is a Split, MANY removed to one added a Merged; every other shape (1:1, N:M) stays a plain
        // add and remove, because a 1:1 content match is a re-identification the pair already states and an N:M
        // group names no source and no survivor.
        var (added, removed, reidentified) = Reidentify(
            next.Keys.Filter(id => !prior.ContainsKey(id)).ToSeq().Map(id => (Id: id, Entry: next[id])),
            prior.Keys.Filter(id => !next.ContainsKey(id)).ToSeq().Map(id => (Id: id, Entry: prior[id])));
        var common = prior.Keys.Filter(next.ContainsKey).ToSeq();
        var moved = common
            .Filter(id => prior[id].Fp.ContentKey == next[id].Fp.ContentKey && prior[id].Fp.PlacementKey != next[id].Fp.PlacementKey)
            .Map(id => (ElementChange)new ElementChange.Moved(
                id, prior[id].Fp.PlacementKey, next[id].Fp.PlacementKey,
                Pose(prior[id].Obj), Pose(next[id].Obj)));
        int unchanged = common.Count(id => prior[id].Fp == next[id].Fp);
        return common
            .Filter(id => prior[id].Fp.ContentKey != next[id].Fp.ContentKey)
            .TraverseM(id =>
                from before in baseline.Bake(prior[id].Obj.Id, key)
                from after in revision.Bake(next[id].Obj.Id, key)
                select (ElementChange)new ElementChange.Modified(
                    id, prior[id].Fp.ContentKey, next[id].Fp.ContentKey,
                    prior[id].Fp.PlacementKey, next[id].Fp.PlacementKey, Deltas(before, after)))
            .As()
            .Map(modified => new ModelDiff(
                baselineAddress,
                revisionAddress,
                added + removed + reidentified + moved + modified,
                unchanged));
    }

    // The seam Object's own placement column lowered at the wire boundary: Option to nullable, because the payload
    // carries no LanguageExt converter and the pose is read, never re-derived — the diff states the frame the graph
    // holds and no consumer recomputes one from the opaque placement key.
    static PlacementPose? Pose(Node.Object node) =>
        node.Placement.Match<PlacementPose?>(Some: static frame => PlacementPose.Of(frame), None: static () => null);

    // The one re-identification fold: group both partitions by content key, lift the 1:N and N:1 groups onto
    // Split/Merged, and return the residue as plain adds and removes. The counterpart ids sort ordinal so a
    // re-diff of one pair of snapshots is byte-stable regardless of map enumeration order.
    static (Seq<ElementChange> Added, Seq<ElementChange> Removed, Seq<ElementChange> Reidentified) Reidentify(
        Seq<(string Id, (Node.Object Obj, ElementFingerprint Fp) Entry)> added,
        Seq<(string Id, (Node.Object Obj, ElementFingerprint Fp) Entry)> removed) {
        var keys = toSeq(added.Map(static r => r.Entry.Fp.ContentKey).Concat(removed.Map(static r => r.Entry.Fp.ContentKey)).Distinct());
        var pairs = keys.Map(key => (
            Key: key,
            In: toSeq(added.Filter(r => r.Entry.Fp.ContentKey == key).Map(static r => r.Id).OrderBy(static id => id, StringComparer.Ordinal)),
            Out: toSeq(removed.Filter(r => r.Entry.Fp.ContentKey == key).Map(static r => r.Id).OrderBy(static id => id, StringComparer.Ordinal))));
        var splits = pairs.Filter(static p => p.Out.Count == 1 && p.In.Count > 1)
            .Map(static p => (ElementChange)new ElementChange.Split(p.Out[0], p.Key, [.. p.In]));
        var merges = pairs.Filter(static p => p.In.Count == 1 && p.Out.Count > 1)
            .Map(static p => (ElementChange)new ElementChange.Merged(p.In[0], p.Key, [.. p.Out]));
        var claimed = toHashSet(splits.Bind(static c => ((ElementChange.Split)c).Into.ToSeq().Add(c.GlobalId))
            + merges.Bind(static c => ((ElementChange.Merged)c).From.ToSeq().Add(c.GlobalId)));
        return (added.Filter(r => !claimed.Contains(r.Id)).Map(static r => Added(r.Id, r.Entry)),
                removed.Filter(r => !claimed.Contains(r.Id)).Map(static r => Removed(r.Id, r.Entry)),
                splits + merges);
    }

    // The cross-runtime projection the ts:ui/bcf-anchor live-binding decodes — HOST-FREE (GlobalId strings, the seam
    // ContentAddress keys, the typed member deltas). Both [Union]s ride their per-leaf [JsonDerivedType] rows (the
    // ElementChange "kind" discriminator IS the ChangeKind row name, the DeltaValue rows its leaf carriers) so a TS
    // decode switches on the kind string; the KEYED owners — the [SmartEnum] DeltaShape, the [ValueObject<UInt128>]
    // ContentAddress — ride the ThinktectureJsonConverterFactory key conversion, and the [ComplexValueObject]
    // Classification and MeasureValue their generator-emitted attribute-bound converters.
    // A malformed payload faults BimFault.ModelRejected (the Model/faults#FAULT_BAND wire-admission arm) BARE, the
    // Seq projecting through an array at the boundary so no LanguageExt Seq converter is required.
    public static Fin<byte[]> Encode(ModelDiff diff, Op key) =>
        Try.lift(() => JsonSerializer.SerializeToUtf8Bytes(
            new Payload(diff.Baseline, diff.Revision, [.. diff.Changes], diff.UnchangedCount), Wire)).Run()
            .MapFail(error => new BimFault.ModelRejected(key, $"diff-wire-encode:{error.Message}"));

    public static Fin<ModelDiff> Decode(ReadOnlyMemory<byte> json, Op key) =>
        Try.lift(() => JsonSerializer.Deserialize<Payload>(json.Span, Wire)).Run()
            .MapFail(error => new BimFault.ModelRejected(key, $"diff-wire-decode:{error.Message}"))
            .Bind(payload => payload.Baseline is null || payload.Revision is null
                || payload.Changes is null || payload.Changes.Any(static change => change is null)
                || payload.UnchangedCount < 0
                    ? Fin.Fail<ModelDiff>(new BimFault.ModelRejected(key, "diff-wire-decode:shape"))
                    : Fin.Succ(new ModelDiff(
                        payload.Baseline,
                        payload.Revision,
                        toSeq(payload.Changes),
                        payload.UnchangedCount)));

    // The per-element content fingerprint over the seam ContentAddress codec: the content key folds the Object's
    // non-geometry semantics with its bound nodes, the placement key folds its geometry — the split distinguishing a
    // relocation from a content edit. The SAME content-key the Review/versioning#VERSION_GRAPH commit-DAG keys on, so a
    // commit, a diff, and the audit chain share one identity. GlobalId falls back to the NodeId string only off the
    // federation surface (a never-emitted authored Object), the federation diff itself only fingerprinting ExternalId-bearing nodes.
    public static ElementFingerprint Fingerprint(ElementGraph graph, Node.Object node) =>
        new(node.ExternalId.IfNone(node.Id.Value), ContentKey(graph, node), PlacementKey(node, graph.Header.Tolerance));

    // The typed federation-identity admission MIRRORING Review/versioning#VERSION_GRAPH's commit-duplicate-globalid
    // gate: two federation surfaces carrying ONE IFC GlobalId is a data defect the rail names, never a LanguageExt
    // ToMap ArgumentException escaping a domain signature — the two owners state one federation-identity law.
    static Fin<Map<string, (Node.Object Obj, ElementFingerprint Fp)>> Federate(ElementGraph graph, Op key) {
        Seq<(string GlobalId, (Node.Object Obj, ElementFingerprint Fp) Entry)> rows = graph.ObjectNodes
            .Choose(node => node.ExternalId.Map(globalId => (globalId, (Obj: node, Fp: Fingerprint(graph, node)))))
            .ToSeq();
        Seq<string> collided = toSeq(rows.Map(static r => r.GlobalId).GroupBy(identity)).Filter(g => g.Count() > 1).Map(static g => g.Key);
        return collided.IsEmpty
            ? Fin.Succ(rows.Map(static r => (r.GlobalId, r.Entry)).ToMap())
            : Fin.Fail<Map<string, (Node.Object Obj, ElementFingerprint Fp)>>(
                new BimFault.ModelRejected(key, $"diff-duplicate-globalid:{string.Join(',', collided)}"));
    }

    static ElementChange Added(string globalId, (Node.Object Obj, ElementFingerprint Fp) entry) =>
        new ElementChange.Added(globalId, entry.Obj.Classification, entry.Obj.PredefinedType, entry.Fp.ContentKey);

    static ElementChange Removed(string globalId, (Node.Object Obj, ElementFingerprint Fp) entry) =>
        new ElementChange.Removed(globalId, entry.Obj.Classification, entry.Obj.PredefinedType, entry.Fp.ContentKey);

    // The semantic content key: the Object's non-geometry head plus the order-independent fold of its outgoing edges'
    // contributions (each = the edge canonical bytes plus, when the far endpoint is a bound NON-Object node, that
    // node's content address — so a property/material edit moves the key while a part/type Object, diffed
    // independently, contributes only its binding structure). Geometry is EXCLUDED (it rides the placement key).
    static ContentAddress ContentKey(ElementGraph graph, Node.Object node) {
        double tolerance = graph.Header.Tolerance;
        Seq<UInt128> contributions = toSeq(toSeq(graph.EdgesAt(node.Id))
            .Filter(edge => edge.Relating == node.Id)
            .Map(edge => BoundContribution(graph, node.Id, edge, tolerance))
            .OrderBy(static contribution => contribution));
        CanonicalWriter writer = new(tolerance);
        writer.String(node.Kind.Key).String(node.Classification.System).String(node.Classification.Code)
            .String(node.PredefinedType.Token).String(node.Name).String(node.Tag).Ordinal(contributions.Count);
        foreach (UInt128 contribution in contributions) { writer.U128(contribution); }
        return ContentAddress.Of(writer.ToBytes().Span);
    }

    // The presence flag delimits the raw-append join: without it an edge whose canonical bytes happen to end in
    // sixteen address-shaped bytes and a shorter edge plus a bound-node address hash identically — the same
    // injectivity law the count-prefixed collection folds observe.
    static UInt128 BoundContribution(ElementGraph graph, NodeId self, Relationship edge, double tolerance) {
        NodeId far = edge.Relating == self ? edge.Related : edge.Relating;
        Option<UInt128> bound = graph.Find(far).Bind(node => node is not Node.Object ? Some(ContentAddress.Of(node, tolerance).Value) : None);
        CanonicalWriter writer = new(tolerance);
        writer.Raw(edge.ToCanonicalBytes(tolerance).Span).Bool(bound.IsSome);
        bound.IfSome(value => writer.U128(value));
        return ContentAddress.Of(writer.ToBytes().Span).Value;
    }

    // The geometry/placement key: the Object's PLACEMENT TRANSFORM folded explicitly beside its content-hashed
    // RepresentationContentHash map. The transform is what makes Moved decidable — an IFC representation is authored
    // in the element's OWN local space and positioned by IfcLocalPlacement, so a rigid relocation leaves every
    // representation hash byte-identical and a key over hashes ALONE reads a moved element as unchanged, collapsing
    // the whole Moved arm into silence. Both currencies enter: the transform at model-space tolerance (a sub-tolerance
    // jitter is not a move) and EVERY geometry hash, the heavy display Body AND the lightweight analytical
    // Axis/FootPrint the structural/energy disciplines resolve one-hop by content key, so a local re-model with no
    // relocation still moves this key while the semantic content key stays stable. The geometry is referenced BY
    // content key, never inline coordinates (an inline BoundaryPolygon/Axis read is the Graph/element#NODE_MODEL M2
    // seam violation). The projection rides the ONE CanonicalWriter codec — presence-flagged and count-prefixed like
    // the seam's own folds, the self-delimiting law every collection fold observes.
    static ContentAddress PlacementKey(Node.Object node, double tolerance) {
        CanonicalWriter writer = new(tolerance);
        writer.Bool(node.Placement.IsSome);
        node.Placement.IfSome(placement => placement.CanonicalBytes(writer)); // the carrier's ONE sibling-fold projection; the writer already carries the tolerance
        writer.Ordinal(node.Representations.ByIdentifier.Count);
        foreach (var (identifier, hash) in node.Representations.ByIdentifier.OrderBy(static pair => pair.Key, StringComparer.Ordinal)) {
            writer.String(identifier).U128(hash);
        }
        return ContentAddress.Of(writer.ToBytes().Span);
    }

    // The Modified delta: the Generator.Equals member diff over the two baked Elements, composed BARE — the
    // Id/ExternalId/History local-identity/provenance members and the nested Parts (each part a rooted Object
    // diffed independently by its own GlobalId) are excluded at the Rasm.Element owner's own [IgnoreEquality]
    // declarations, so every consumer of this comparer agrees on the noise axes by construction and no call-site
    // roster exists to drift; the bound Material/PropertySet/Quantity nodes are content-addressed (stable across
    // ingests) so they surface — the SAME Inequalities substrate the
    // Rasm.Persistence/Version/merge#STRUCTURAL_DIFF three-way merge reads. Each row's terminal segment kind
    // projects the DeltaShape token (an empty path — no member named — degrades to the scalar Replace).
    static ImmutableArray<AspectDelta> Deltas(Element baseline, Element revision) =>
        [.. Element.EqualityComparer.Default.Inequalities(baseline, revision)
            .Select(static inequality => new AspectDelta(
                inequality.Path.ToString(),
                inequality.Path.Segments is [.., var terminal] ? DeltaShape.Of(terminal) : DeltaShape.Replace,
                DeltaValue.Of(inequality.Left),
                DeltaValue.Of(inequality.Right)))];

    static readonly JsonSerializerOptions Wire = new(JsonSerializerDefaults.Web) {
        Converters = { new ThinktectureJsonConverterFactory() },
    };

    readonly record struct Payload(
        ContentAddress Baseline,
        ContentAddress Revision,
        ElementChange[] Changes,
        int UnchangedCount);
}
```

## [03]-[AUDIT]

- Owner: `AuditEntry` the immutable mutation-log row carrying the element `GlobalId`, the typed `ChangeKind`, the baseline/revision `ContentAddress` pair, the author, the `Instant`, the version pointer, and the chained `EntryKey` content address keyed on the prior entry's key so a retroactive edit breaks the chain; `AuditVersion` the per-version authoring metadata; `AuditTrail` the append-only log folding the per-version `ModelDiff` change-sets into the chained entry sequence, queryable by element `GlobalId`. The trail is a model-mutation log (who/when/what-changed-semantically), explicitly distinct from the geometry-asset XMP lineage (who-minted-this-GLB) and from the branching commit-DAG.
- Entry: `AuditTrail.Fold(Seq<(AuditVersion Version, ModelDiff Diff)> history)` folds a version sequence of `ModelDiff` change-sets into the chained `AuditTrail` — each `ElementChange` arm in each version's diff projects onto one `AuditEntry` (the typed `ChangeKind`, the version's baseline/revision content keys, the version's author and `Instant`), and the `EntryKey` chains on the prior entry's key through the seam `Rasm.Element/Projection/address#CONTENT_ADDRESS` codec so the log is tamper-evident — re-folding a tampered history yields a divergent terminal `EntryKey`; the fold is total, pure, no rail. `AuditTrail.For(string globalId)` folds every entry an element underwent into its lifecycle history in chain order, and `AuditTrail.Verify()` re-derives the chain to witness no retroactive edit broke it.
- Auto: `Fold` threads the `(Prior, Rows)` accumulator across the version sequence — for each version it folds the version's `ModelDiff.Changes` onto `AuditEntry` rows in `GlobalId`-ordinal order (the order-stable chain `Verify` re-derives) (each carrying the element `GlobalId` and `ChangeKind` the change names through its base accessors, the version content keys decomposed by the one `Keys` switch, the version pointer and author/`Instant`, and the prior `EntryKey` as `ParentKey`), computes each entry's `EntryKey` as the seam `ContentAddress` over the prior key concatenated with the entry's canonical content, and threads the new key as the next entry's parent so the chain is a content-addressed Merkle-like sequence — the same content-key idiom the `Review/versioning#VERSION_GRAPH` `BimCommit.CommitKey` and the `Rasm.Persistence/Version/provenance#ATTESTED_LEDGER` `AttestedEntry.Chain` carry; `For(globalId)` filters the folded entries to the element preserving chain order; `Verify` re-folds the entry contents recomputing each `EntryKey` from the recorded parent through the SAME `EntryKey` projection `Fold` used and compares the recomputed key against the stored one, so a single retroactive field edit diverges every downstream key and the boolean witnesses chain integrity without a separate stored checksum.
- Receipt: the `AuditTrail` chained `Seq<AuditEntry>` is the compliance evidence (who/when/from-what per element) the federation and compliance consumer read, `AuditTrail.For(globalId)` the per-element lifecycle history anchoring a `Review/issues#BCF_ARCHIVE` topic and the `Review/versioning#VERSION_GRAPH` merge, and `AuditTrail.Verify()` the tamper-evidence witness; the durable append-only residence is the `Rasm.Persistence/Version/provenance#ATTESTED_LEDGER` concern joined at the `Review/diff → Rasm.Persistence/Version/provenance # [CONTENT_KEY]: AuditEntry chained ElementChange mutation log` seam by the content-key, this owner producing the chained host-neutral log and its content-key identity, the durable signed ledger riding the Persistence ripple.
- Packages: Rasm.Element, Thinktecture.Runtime.Extensions, NodaTime, LanguageExt.Core, Rasm
- Growth: a new audit field is one column on `AuditEntry` folded into the `EntryKey` content; a new version-metadata dimension is one column on `AuditVersion`; a new lifecycle query is one fold over the same chained entry sequence; never a per-change-kind audit record, never a second mutation store, and never a checksum beside the chained `EntryKey`.
- Boundary: the audit trail keys on the `[02]-[MODEL_DIFF]` `ElementChange` and the version lineage, explicitly distinct from the Wave A tile-XMP geometry-asset provenance — the two stay separate, the audit trail never keying on the export artifact content-key; the chain is the content-addressed `EntryKey` through the seam `Projection/address#CANONICAL_WRITER` `CanonicalWriter` codec hashed by the `Projection/address#CONTENT_ADDRESS` `ContentAddress` (the kernel seed-zero `XxHash128`, the ONE hasher), and the retired hand-rolled `XxHash128.HashToUInt128(Encoding.UTF8.GetBytes($"..."))` string-interpolation chain plus a separate stored checksum or a mutable sequence-number beside the `EntryKey` are the deleted forms; the `ChangeKind` is the typed `[SmartEnum<string>]` projected by `ElementChange.Kind` and a stringly `"added"`/`"removed"` literal is the deleted form; the keys are `ContentAddress` values, never raw `UInt128`; the fold consumes the `[02]-[MODEL_DIFF]` `ModelDiff` change-sets as settled vocabulary and mints no second diff or element shape; each version's change set sorts by `GlobalId` ordinal before it chains and folding `ModelDiff.Changes` in its native partition order is the deleted form — the chain is order-dependent, so two folds of one history would mint divergent terminal keys and `Verify` would report tampering that never happened; the trail is a pure fold over the version-and-diff sequence, never an imperative append loop with mutable accumulation; the audit trail is the LINEAR per-element who/when/what Merkle chain, distinct from the `Review/versioning#VERSION_GRAPH` branching commit-DAG, neither re-derived from the other, both reading the one content-key space; the durable append-only residence is the `Rasm.Persistence/Version/provenance#ATTESTED_LEDGER` concern joined at the content-key seam and a durable store minted here is the named seam violation; the fold is TOTAL and carries no fault rail — it consumes the `[02]-[MODEL_DIFF]` `ModelDiff` change-sets whose `GlobalId`s the diff already resolved from real graph nodes, so a dangling reference cannot arise at the audit boundary (the diff's `Bake` composition is the one place a corrupt subgraph rails `Rasm.Element/Projection/fault#FAULT_BAND` `ElementFault`), and a fabricated `Fin`/`BimFault` rail the body never produces is the illusory form.

```csharp signature
// --- [RUNTIME_PRELUDE] --------------------------------------------------------------------
using LanguageExt;
using NodaTime;
using Rasm.Element.Classification;
using Rasm.Element.Graph;
using Rasm.Element.Projection;
using Rasm.Element.Properties;
using Rasm.Element.Relations;
using static LanguageExt.Prelude;

namespace Rasm.Bim;

// --- [MODELS] -----------------------------------------------------------------------------
// The per-version authoring metadata a folded diff stamps onto each entry — the identity the chain binds.
public sealed record AuditVersion(string VersionId, string Author, Instant At);

// One immutable mutation-log row: the element GlobalId, the typed ChangeKind, the baseline/revision content
// addresses, the version pointer plus author/instant, and the chained EntryKey keyed on the prior row's key so
// a retroactive edit — the version pointer included — diverges every downstream key. Every key is a seam
// ContentAddress (the ONE codec), never a raw UInt128.
public readonly record struct AuditEntry(
    string GlobalId,
    ChangeKind Kind,
    ContentAddress BaselineKey,
    ContentAddress RevisionKey,
    string VersionId,
    string Author,
    Instant At,
    ContentAddress ParentKey,
    ContentAddress EntryKey);

public sealed record AuditTrail(Seq<AuditEntry> Entries) {
    public static readonly AuditTrail Empty = new(Seq<AuditEntry>());
    static readonly ContentAddress Genesis = ContentAddress.Of(UInt128.Zero);

    // Fold a version sequence of ModelDiff change-sets into the chained trail: each ElementChange projects onto one
    // AuditEntry whose EntryKey chains on the prior row's key, so re-folding a tampered history yields a divergent
    // terminal key. Each version's change set sorts by GlobalId ORDINAL before it chains, because the chain is
    // order-dependent by construction and ModelDiff.Changes carries the diff's own partition order (added, then
    // removed, then re-identified, then moved, then modified, each in map-enumeration order) — two folds of the
    // SAME history would otherwise mint divergent terminal keys and Verify would report tampering where none
    // occurred. Pure fold over the version-and-diff sequence, never an imperative append loop.
    public static AuditTrail Fold(Seq<(AuditVersion Version, ModelDiff Diff)> history) =>
        new(history.Fold((Prior: Genesis, Rows: Seq<AuditEntry>()), static (state, step) =>
            toSeq(step.Diff.Changes
                    .OrderBy(static c => c.GlobalId, StringComparer.Ordinal)
                    .ThenBy(static c => c.Kind.Key, StringComparer.Ordinal))
                .Fold(state, (acc, change) => {
                AuditEntry entry = Chain(acc.Prior, change, step.Version);
                return (entry.EntryKey, acc.Rows.Add(entry));
            })).Rows);

    // Every entry an element underwent in chain order — its lifecycle history (add -> modify -> move -> remove).
    // The GlobalId compare pins Ordinal: an IFC GlobalId is a base64 token whose case is significant, and a
    // culture-sensitive default would join two distinct elements under a locale's casing rules.
    public Seq<AuditEntry> For(string globalId) =>
        Entries.Filter(entry => string.Equals(entry.GlobalId, globalId, StringComparison.Ordinal));

    // Re-derive the chain to witness no retroactive edit broke it: recompute each EntryKey from the recorded parent
    // through the SAME EntryKey projection Chain used, so a single edited field diverges every downstream key.
    public bool Verify() =>
        Entries.Fold((Prior: Genesis, Ok: true), static (state, entry) => (
            entry.EntryKey,
            state.Ok
                && EntryKey(state.Prior, entry.GlobalId, entry.Kind, entry.BaselineKey, entry.RevisionKey, entry.VersionId, entry.Author, entry.At) == entry.EntryKey
                && entry.ParentKey == state.Prior)).Ok;

    static AuditEntry Chain(ContentAddress prior, ElementChange change, AuditVersion version) {
        var (baseline, revision) = Keys(change);
        ContentAddress entryKey = EntryKey(prior, change.GlobalId, change.Kind, baseline, revision, version.VersionId, version.Author, version.At);
        return new AuditEntry(change.GlobalId, change.Kind, baseline, revision, version.VersionId, version.Author, version.At, prior, entryKey);
    }

    // The baseline/revision content addresses per arm: an Added has no baseline, a Removed no revision, a Modified its
    // two content keys (its placement pair stays on the change for a consumer that needs both axes; the audit row
    // chains the semantic currency), a Moved its two placement keys — the one Switch the audit row decomposes through.
    static (ContentAddress Baseline, ContentAddress Revision) Keys(ElementChange change) => change.Switch(
        added:    static c => (Genesis, c.Content),
        removed:  static c => (c.Content, Genesis),
        modified: static c => (c.BaselineContent, c.RevisionContent),
        moved:    static c => (c.BaselinePlacement, c.RevisionPlacement),
        split:    static c => (c.Content, Genesis),
        merged:   static c => (Genesis, c.Content));

    // The content-addressed chain link through the seam ContentAddress codec (the kernel seed-zero XxHash128, the ONE
    // hasher) — the prior key, the element GlobalId, the typed kind, the key pair, the version pointer, and the
    // authorship fold into one canonical projection, so the chain is a Merkle sequence and a separate stored checksum
    // is the deleted form. The instant rides the writer's own fixed-width I64 lane: ticks are a signed 64-bit
    // quantity, and the widen-through-unsigned cast that dressed them as a U128 addressed a pre-epoch instant
    // identically to a far-future one.
    static ContentAddress EntryKey(ContentAddress prior, string globalId, ChangeKind kind, ContentAddress baseline, ContentAddress revision, string versionId, string author, Instant at) {
        CanonicalWriter writer = new(0.0);
        writer.U128(prior.Value).String(globalId).String(kind.Key).U128(baseline.Value).U128(revision.Value)
            .String(versionId).String(author).I64(at.ToUnixTimeTicks());
        return ContentAddress.Of(writer.ToBytes().Span);
    }
}
```

## [04]-[RESEARCH]

(none)
