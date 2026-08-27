# [BIM_MODEL_DIFF]

The GlobalId-stable federation diff over the element graph: one `ModelDiff` change-set carries the baseline and revision `ContentAddress.OfGraph` identities and folds both `Rasm.Element/Graph/element#ELEMENT_GRAPH` snapshots into the `ElementChange` added/removed/modified/moved arms, joining by the Bim-stored `Rasm.Element/Graph/element#NODE_MODEL` `Node.Object.ExternalId` (the IFC `GlobalId` [H6] — the ONE identity two federated submissions share, because the neutral kernel `NodeId` is minted afresh per ingest and never coincides across parties) and classifying each matched pair by the shared `Rasm.Element/Projection/address#CONTENT_ADDRESS` content and placement keys so an unchanged element dedups by content address and a re-check bakes only the changed elements. PLACEMENT-SPACE LAW: the placement key folds the element's OBJECT PLACEMENT TRANSFORM explicitly beside its content-hashed representations, and every geometry hash it reads is stated in the representation's OWN local space — an IFC representation is authored local and positioned by `IfcLocalPlacement`, so a rigid relocation leaves every representation hash byte-identical and a placement key over hashes ALONE reads a moved element as unchanged. The transform enters the key through the shared `CanonicalWriter` at model-space tolerance, so the `Moved` discriminant answers the question its name asks. The diff consumes two `ElementGraph` snapshots as settled vocabulary and mints no second element shape; the consumer element is the `Rasm.Element/Graph/element#ELEMENT_GRAPH` `Bake(objectNode)` fold, the retired `BimModel`/`BimElement` snapshot pair GONE.

The diff is the cross-party twin of two same-lineage owners and re-derives neither. The `Rasm.Persistence/Version/merge#STRUCTURAL_DIFF` `StructuralMerge` is the NodeId-keyed (re-ingest `Reconcile`-aligned on `ExternalId`) version-lineage THREE-way merge over one model's history; the `Review/versioning#VERSION_GRAPH` commit-DAG is the branching revision graph. This page is the PAIRWISE two-way federation diff over two INDEPENDENT submissions whose neutral `NodeId`s never coincide, so the join is the IFC `GlobalId` directly (no `Reconcile`); all three compose the one `Generator.Equals` `Inequalities` member-diff substrate and the one shared `ContentAddress` codec, none re-deriving another. Every terminal diff rejection lifts `BimFault.Refused`; the package remains host-neutral and crosses through generated `ModelDiffWire`.

## [01]-[INDEX]

- [02]-[MODEL_DIFF]: the `ModelDiff` change-set, the `ElementChange` closed `[Union]` (Added/Removed/Modified/Moved/Split/Merged), the `ChangeKind` audit token, the `ElementFingerprint` content/placement keys the join dedups on, the `Generator.Equals` `Inequalities` `AspectDelta` member-level delta the `Modified` arm carries, and the generated `Bim.ModelDiffWire` `Seal`/`Admit` crossing.
- [03]-[AUDIT]: the chained content-addressed `AuditEntry` log folding `ModelDiff` change-sets across a version sequence into a tamper-evident per-element mutation trail, the `AuditTrail.For(globalId)` lifecycle query, and `AuditTrail.Verify()`.

## [02]-[MODEL_DIFF]

- Owner: `ModelDiff` the change-set carrying the baseline and revision graph addresses beside one change sequence and the unchanged count; `ElementChange` the closed `[Union]` over IFC `GlobalId`; `ChangeKind` the audit vocabulary projected from those arms; `ElementFingerprint` the `(GlobalId, content key, placement key)` triple; `AspectDelta` the path, `DeltaShape`, and typed `DeltaValue` evidence. A moved arm carries `Option<PlacementTransform>` directly—the shared value, not a flattened Bim mirror.
- Auto: `Between` `Federate`s each graph into a `GlobalId`-keyed map over the `ExternalId`-bearing `Object` nodes (the `Review/coordination#COORDINATION` `ExternalId` `Choose`-discard-`None` law — an authored Object with no IFC `GlobalId` sits off the federation surface, never a fault), `Fingerprint`s each through the shared `ContentAddress`, then partitions the common set: a differing `ContentKey` is `Modified`, an equal `ContentKey` with a differing `PlacementKey` is `Moved`, both equal is unchanged. The content key folds the `Object`'s semantic head (kind/classification/predefined/name/tag) with the order-independent content addresses of its bound non-`Object` nodes (`PropertySet`/`QuantitySet`/`Material`/`Assessment`/`Appearance`/`Coverage`) and its outgoing-edge structure; the placement key folds the `Object`'s geometry through the `RepresentationContentHash` map ALONE — EVERY geometry content-hashed there, the heavy display `Body` AND the lightweight analytical `Axis`/`FootPrint` the structural/energy disciplines resolve one-hop by content key — so a relocation moves the geometry bytes, the content hashes, and thus the placement key, while the semantic content key stays stable; an inline `BoundaryPolygon`/`Axis` coordinate read is the named contract violation (the contract carries no raw coordinate field — `Graph/element#NODE_MODEL` M2). The `Modified` arm carries BOTH currencies — the content-key pair AND the placement-key pair, so a content edit that also relocates the element keeps the axis the `Review/versioning#VERSION_GRAPH` merge weighs — plus `Generator.Equals` `Inequalities` over the two baked `Element`s as `AspectDelta` rows (the `Id`/`ExternalId`/`History`/`Parts` noise axes excluded at the `Rasm.Element` owner's own `[IgnoreEquality]` declarations, so the comparer composes bare and every consumer agrees by construction), each row's terminal `MemberPathSegment` projected onto the `DeltaShape` token so a downstream consumer reads the exact `Properties[Pset].FireRating` member that moved AND the shape of the move — a scalar `Replace` distinguished from an ordered-collection `Index`, a keyed-bag `Key`, or a set-membership `Added`/`Removed` — with each side's leaf kept typed, not an opaque content-key delta and not two rendered strings.
- Output: the `ModelDiff` change-set is the incremental federation evidence; a `Review/issues#BCF_ARCHIVE` `BcfTopic` anchors a `BcfViewpoint.SelectedGlobalIds` on the `Modified`/`Moved` element `GlobalId`s this diff names, the `Review/coordination#COORDINATION` `Coordination.Between` folds two change-sets into the downstream-affected element/task/cost sets, and the `Review/versioning#VERSION_GRAPH` `BimCommit` keys its `Map<string, ElementFingerprint>` on the SAME `ElementFingerprint`; the generated `ModelDiffWire` is the cross-runtime artifact a future app may render or decode.
- Boundary: federation joins on `Node.Object.ExternalId`, never the locally minted `NodeId`; content and placement keys compose the shared `ContentAddress`, never another hasher. A `Moved` arm carries the placement keys and the shared `PlacementTransform` options, then `Seal` projects them through the shared generated converter—no `PlacementPose` mirror. `Modified` keeps both content and placement currencies and typed member deltas. `Split`/`Merged` derive from content-key groups. The sole cross-runtime value is generated `ModelDiffWire`; Bim neither serializes JSON nor references AppHost, and no `Payload`, `DiffWire`, hand discriminator family, or transport option exists here.

```csharp
// --- [IMPORTS] -------------------------------------------------------------------------
using System.Collections.Frozen;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using Generator.Equals;
using Google.Protobuf.WellKnownTypes;
using LanguageExt;
// Contracts are retired from this logic.
using Rasm.Domain;
using Rasm.Element.Classification;
using Rasm.Element.Graph;
using Rasm.Element.Projection;
using Rasm.Element.Properties;
using Rasm.Element.Relations;
using Thinktecture;
using static LanguageExt.Prelude;
using static Rasm.Element.Graph.BoundaryConverters;

namespace Rasm.Bim;

// --- [TYPES] ---------------------------------------------------------------------------
[SmartEnum<string>]
public sealed partial class ChangeKind {
    public static readonly ChangeKind Added = new(nameof(Added));
    public static readonly ChangeKind Removed = new(nameof(Removed));
    public static readonly ChangeKind Modified = new(nameof(Modified));
    public static readonly ChangeKind Moved = new(nameof(Moved));
    public static readonly ChangeKind Split = new(nameof(Split));
    public static readonly ChangeKind Merged = new(nameof(Merged));
}

[SmartEnum<string>]
public sealed partial class DeltaShape {
    public static readonly DeltaShape Replace = new("replace");
    public static readonly DeltaShape Index   = new("index");
    public static readonly DeltaShape Key     = new("key");
    public static readonly DeltaShape Added   = new("added");
    public static readonly DeltaShape Removed = new("removed");
    public static readonly DeltaShape Unknown = new("unknown");

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

[Union]
public abstract partial record DeltaValue {
    private DeltaValue() { }

    public sealed record Measure(MeasureValue Value) : DeltaValue;
    public sealed record Address(ContentAddress Value) : DeltaValue;
    public sealed record Label(string Value) : DeltaValue;
    public sealed record Absent : DeltaValue;

    public static DeltaValue Of(object? leaf) => leaf switch {
        null                       => new Absent(),
        MeasureValue measure       => new Measure(measure),
        PropertyValue.Measure p    => new Measure(p.Value),
        ContentAddress address     => new Address(address),
        PropertyValue value        => new Label(value.Render()),
        _                          => new Label(leaf.ToString() ?? string.Empty),
    };
}

// --- [MODELS] --------------------------------------------------------------------------
public readonly record struct AspectDelta(string Path, DeltaShape Shape, DeltaValue Before, DeltaValue After);

public readonly record struct ElementFingerprint(string GlobalId, ContentAddress ContentKey, ContentAddress PlacementKey);

[Union]
public abstract partial record ElementChange {
    private ElementChange() { }

    public abstract string GlobalId { get; }

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
    public sealed record Moved(string GlobalId, ContentAddress BaselinePlacement, ContentAddress RevisionPlacement,
        Option<PlacementTransform> BaselinePose, Option<PlacementTransform> RevisionPose) : ElementChange;
    public sealed record Split(string GlobalId, ContentAddress Content, ImmutableArray<string> Into) : ElementChange;
    public sealed record Merged(string GlobalId, ContentAddress Content, ImmutableArray<string> From) : ElementChange;
}

public sealed record ModelDiff(
    ContentAddress Baseline,
    ContentAddress Revision,
    Seq<ElementChange> Changes,
    int UnchangedCount) {
    public static Fin<ModelDiff> Between(ElementGraph baseline, ElementGraph revision) =>
        from prior in Federate(baseline)
        from next in Federate(revision)
        from diff in Classify(baseline, revision, prior, next)
        select diff;

    static Fin<ModelDiff> Classify(
        ElementGraph baseline, ElementGraph revision,
        Map<string, (Node.Object Obj, ElementFingerprint Fp)> prior,
        Map<string, (Node.Object Obj, ElementFingerprint Fp)> next) {
        ContentAddress baselineAddress = ContentAddress.OfGraph(baseline);
        ContentAddress revisionAddress = ContentAddress.OfGraph(revision);
        var (added, removed, reidentified) = Reidentify(
            next.Keys.Filter(id => !prior.ContainsKey(id)).ToSeq().Map(id => (Id: id, Entry: next[id])),
            prior.Keys.Filter(id => !next.ContainsKey(id)).ToSeq().Map(id => (Id: id, Entry: prior[id])));
        var common = prior.Keys.Filter(next.ContainsKey).ToSeq();
        var moved = common
            .Filter(id => prior[id].Fp.ContentKey == next[id].Fp.ContentKey && prior[id].Fp.PlacementKey != next[id].Fp.PlacementKey)
            .Map(id => (ElementChange)new ElementChange.Moved(
                id, prior[id].Fp.PlacementKey, next[id].Fp.PlacementKey,
                prior[id].Obj.Placement, next[id].Obj.Placement));
        int unchanged = common.Count(id => prior[id].Fp == next[id].Fp);
        return common
            .Filter(id => prior[id].Fp.ContentKey != next[id].Fp.ContentKey)
            .TraverseM(id =>
                from before in baseline.Bake(prior[id].Obj.Id)
                from after in revision.Bake(next[id].Obj.Id)
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

    static (Seq<ElementChange> Added, Seq<ElementChange> Removed, Seq<ElementChange> Reidentified) Reidentify(
        Seq<(string Id, (Node.Object Obj, ElementFingerprint Fp) Entry)> added,
        Seq<(string Id, (Node.Object Obj, ElementFingerprint Fp) Entry)> removed) {
        var keys = toSeq(added.Map(static r => r.Entry.Fp.ContentKey).Concat(removed.Map(static r => r.Entry.Fp.ContentKey)).Distinct());
        var pairs = keys.Map(key => (In: toSeq(added.Filter(r => r.Entry.Fp.ContentKey == key).Map(static r => r.Id).OrderBy(static id => id, StringComparer.Ordinal)),
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

    public static Fin<ModelDiffWire> Seal(ModelDiff diff) {
        if (diff.UnchangedCount < 0) { return Rejected<ModelDiffWire>("diff-unchanged-count-negative"); }
        ModelDiffWire wire = new() {
            Baseline = ToWire(diff.Baseline.ToValue()),
            Revision = ToWire(diff.Revision.ToValue()),
            UnchangedCount = (uint)diff.UnchangedCount,
        };
        wire.Changes.AddRange(diff.Changes.Map(SealChange));
        return Fin.Succ(wire);
    }

    public static Fin<ModelDiff> Admit(ModelDiffWire? wire) => wire is null
        ? Rejected<ModelDiff>("diff-message-absent")
        : from baseline in ToKey(wire.Baseline)
          from revision in ToKey(wire.Revision)
          from unchanged in wire.UnchangedCount <= int.MaxValue
              ? Fin.Succ((int)wire.UnchangedCount)
              : Rejected<int>("diff-unchanged-count-overflow")
          from changes in toSeq(wire.Changes).TraverseM(change => AdmitChange(change)).As()
          select new ModelDiff(ContentAddress.Create(baseline), ContentAddress.Create(revision), changes, unchanged);

    static ElementChangeWire SealChange(ElementChange change) => change.Switch(
        added: static value => new ElementChangeWire { Added = SealEnd(value.GlobalId, value.Class, value.Predefined, value.Content) },
        removed: static value => new ElementChangeWire { Removed = SealEnd(value.GlobalId, value.Class, value.Predefined, value.Content) },
        modified: static value => SealModified(value),
        moved: static value => SealMoved(value),
        split: static value => SealRegroup(value.GlobalId, value.Content, value.Into, split: true),
        merged: static value => SealRegroup(value.GlobalId, value.Content, value.From, split: false));

    static DiffEndWire SealEnd(string globalId, Classification classification, PredefinedType predefined, ContentAddress content) => new() {
        GlobalId = globalId,
        Classification = ToWire(classification),
        Predefined = predefined.ToValue(),
        Content = ToWire(content.ToValue()),
    };

    static ElementChangeWire SealModified(ElementChange.Modified value) {
        DiffModifiedWire wire = new() {
            GlobalId = value.GlobalId,
            BaselineContent = ToWire(value.BaselineContent.ToValue()),
            RevisionContent = ToWire(value.RevisionContent.ToValue()),
            BaselinePlacement = ToWire(value.BaselinePlacement.ToValue()),
            RevisionPlacement = ToWire(value.RevisionPlacement.ToValue()),
        };
        wire.Deltas.AddRange(value.Deltas.Select(SealDelta));
        return new ElementChangeWire { Modified = wire };
    }

    static ElementChangeWire SealMoved(ElementChange.Moved value) {
        DiffMovedWire wire = new() {
            GlobalId = value.GlobalId,
            BaselinePlacement = ToWire(value.BaselinePlacement.ToValue()),
            RevisionPlacement = ToWire(value.RevisionPlacement.ToValue()),
        };
        value.BaselinePose.IfSome(pose => wire.BaselinePose = ToWire(pose));
        value.RevisionPose.IfSome(pose => wire.RevisionPose = ToWire(pose));
        return new ElementChangeWire { Moved = wire };
    }

    static ElementChangeWire SealRegroup(
        string globalId, ContentAddress content, ImmutableArray<string> counterparts, bool split) {
        DiffRegroupWire wire = new() { GlobalId = globalId, Content = ToWire(content.ToValue()) };
        wire.Counterparts.AddRange(counterparts);
        return split ? new ElementChangeWire { Split = wire } : new ElementChangeWire { Merged = wire };
    }

    static AspectDeltaWire SealDelta(AspectDelta value) => new() {
        Path = value.Path,
        Shape = SealShape(value.Shape),
        Before = SealDeltaValue(value.Before),
        After = SealDeltaValue(value.After),
    };

    static Rasm.Contracts.Bim.DeltaShape SealShape(DeltaShape value) =>
        Enum.TryParse(value.Key, ignoreCase: true, out Rasm.Contracts.Bim.DeltaShape wire)
        && wire != Rasm.Contracts.Bim.DeltaShape.Unspecified
            ? wire
            : throw new InvalidOperationException($"<diff-delta-shape:{value.Key}>");

    static DeltaValueWire SealDeltaValue(DeltaValue value) => value.Switch(
        measure: static item => new DeltaValueWire { Measure = ToWire(item.Value) },
        address: static item => new DeltaValueWire { Address = ToWire(item.Value.ToValue()) },
        label: static item => new DeltaValueWire { Label = item.Value },
        absent: static _ => new DeltaValueWire { Absent = new Empty() });

    static Fin<ElementChange> AdmitChange(ElementChangeWire? wire) => wire is null
        ? Rejected<ElementChange>("diff-change-message-absent")
        : wire.KindCase switch {
            ElementChangeWire.KindOneofCase.Added => AdmitEnd(wire.Added)
                .Map(static ElementChange (value) => new ElementChange.Added(value.GlobalId, value.Classification, value.Predefined, value.Content)),
            ElementChangeWire.KindOneofCase.Removed => AdmitEnd(wire.Removed)
                .Map(static ElementChange (value) => new ElementChange.Removed(value.GlobalId, value.Classification, value.Predefined, value.Content)),
            ElementChangeWire.KindOneofCase.Modified => AdmitModified(wire.Modified),
            ElementChangeWire.KindOneofCase.Moved => AdmitMoved(wire.Moved),
            ElementChangeWire.KindOneofCase.Split => AdmitRegroup(wire.Split)
                .Map(static ElementChange (value) => new ElementChange.Split(value.GlobalId, value.Content, value.Counterparts)),
            ElementChangeWire.KindOneofCase.Merged => AdmitRegroup(wire.Merged)
                .Map(static ElementChange (value) => new ElementChange.Merged(value.GlobalId, value.Content, value.Counterparts)),
            _ => Rejected<ElementChange>("diff-change-kind-unset"),
        };

    static Fin<(string GlobalId, Classification Classification, PredefinedType Predefined, ContentAddress Content)> AdmitEnd(
        DiffEndWire? wire) => wire is null
        ? Rejected<(string, Classification, PredefinedType, ContentAddress)>("diff-end-message-absent")
        : from classification in ToClassification(wire.Classification)
          from content in ToKey(wire.Content)
          select (wire.GlobalId, classification, PredefinedType.Create(wire.Predefined), ContentAddress.Create(content));

    static Fin<ElementChange> AdmitModified(DiffModifiedWire? wire) => wire is null
        ? Rejected<ElementChange>("diff-modified-message-absent")
        : from baselineContent in ToKey(wire.BaselineContent)
          from revisionContent in ToKey(wire.RevisionContent)
          from baselinePlacement in ToKey(wire.BaselinePlacement)
          from revisionPlacement in ToKey(wire.RevisionPlacement)
          from deltas in toSeq(wire.Deltas).TraverseM(delta => AdmitDelta(delta)).As()
          select (ElementChange)new ElementChange.Modified(
              wire.GlobalId, ContentAddress.Create(baselineContent), ContentAddress.Create(revisionContent),
              ContentAddress.Create(baselinePlacement), ContentAddress.Create(revisionPlacement), [.. deltas]);

    static Fin<ElementChange> AdmitMoved(DiffMovedWire? wire) => wire is null
        ? Rejected<ElementChange>("diff-moved-message-absent")
        : from baselinePlacement in ToKey(wire.BaselinePlacement)
          from revisionPlacement in ToKey(wire.RevisionPlacement)
          from baselinePose in Optional(wire.BaselinePose).Traverse(pose => ToPlacement(pose)).As()
          from revisionPose in Optional(wire.RevisionPose).Traverse(pose => ToPlacement(pose)).As()
          select (ElementChange)new ElementChange.Moved(
              wire.GlobalId, ContentAddress.Create(baselinePlacement), ContentAddress.Create(revisionPlacement),
              baselinePose, revisionPose);

    static Fin<(string GlobalId, ContentAddress Content, ImmutableArray<string> Counterparts)> AdmitRegroup(
        DiffRegroupWire? wire) => wire is null
        ? Rejected<(string, ContentAddress, ImmutableArray<string>)>("diff-regroup-message-absent")
        : ToKey(wire.Content).Map(content =>
            (wire.GlobalId, ContentAddress.Create(content), wire.Counterparts.ToImmutableArray()));

    static Fin<AspectDelta> AdmitDelta(AspectDeltaWire? wire) => wire is null
        ? Rejected<AspectDelta>("diff-delta-message-absent")
        : from shape in AdmitShape(wire.Shape)
          from before in AdmitDeltaValue(wire.Before)
          from after in AdmitDeltaValue(wire.After)
          select new AspectDelta(wire.Path, shape, before, after);

    static Fin<DeltaShape> AdmitShape(Rasm.Contracts.Bim.DeltaShape wire) => wire switch {
        Rasm.Contracts.Bim.DeltaShape.Replace => Fin.Succ(DeltaShape.Replace),
        Rasm.Contracts.Bim.DeltaShape.Index => Fin.Succ(DeltaShape.Index),
        Rasm.Contracts.Bim.DeltaShape.Key => Fin.Succ(DeltaShape.Key),
        Rasm.Contracts.Bim.DeltaShape.Added => Fin.Succ(DeltaShape.Added),
        Rasm.Contracts.Bim.DeltaShape.Removed => Fin.Succ(DeltaShape.Removed),
        Rasm.Contracts.Bim.DeltaShape.Unknown => Fin.Succ(DeltaShape.Unknown),
        _ => Rejected<DeltaShape>("diff-delta-shape-undefined"),
    };

    static Fin<DeltaValue> AdmitDeltaValue(DeltaValueWire? wire) => wire is null
        ? Rejected<DeltaValue>("diff-delta-value-absent")
        : wire.ValueCase switch {
            DeltaValueWire.ValueOneofCase.Measure => ToMeasure(wire.Measure)
                .Map(static DeltaValue (value) => new DeltaValue.Measure(value)),
            DeltaValueWire.ValueOneofCase.Address => ToKey(wire.Address)
                .Map(static DeltaValue (value) => new DeltaValue.Address(ContentAddress.Create(value))),
            DeltaValueWire.ValueOneofCase.Label => Fin.Succ<DeltaValue>(new DeltaValue.Label(wire.Label)),
            DeltaValueWire.ValueOneofCase.Absent => Fin.Succ<DeltaValue>(new DeltaValue.Absent()),
            _ => Rejected<DeltaValue>("diff-delta-value-unset"),
        };

    static Fin<T> Rejected<T>(string detail) =>
        Fin.Fail<T>(new BimFault.Refused(BimScope.Review, BimReason.Rejected, detail));

    public static ElementFingerprint Fingerprint(ElementGraph graph, Node.Object node) =>
        new(node.ExternalId.IfNone(node.Id.ToValue()), ContentKey(graph, node), PlacementKey(node, graph.Header.Tolerance));

    static Fin<Map<string, (Node.Object Obj, ElementFingerprint Fp)>> Federate(ElementGraph graph) {
        Seq<(string GlobalId, (Node.Object Obj, ElementFingerprint Fp) Entry)> rows = graph.ObjectNodes
            .Choose(node => node.ExternalId.Map(globalId => (globalId, (Obj: node, Fp: Fingerprint(graph, node)))));
        Seq<string> collided = toSeq(rows.Map(static r => r.GlobalId).GroupBy(identity)).Filter(g => g.Count() > 1).Map(static g => g.Key);
        return collided.IsEmpty
            ? Fin.Succ(rows.Map(static r => (r.GlobalId, r.Entry)).ToMap())
            : Fin.Fail<Map<string, (Node.Object Obj, ElementFingerprint Fp)>>(
                new BimFault.Refused(BimScope.Review, BimReason.Rejected, string.Join(':', new object?[] { "duplicate-globalid", "diff", string.Join(',', collided) })));
    }

    static ElementChange Added(string globalId, (Node.Object Obj, ElementFingerprint Fp) entry) =>
        new ElementChange.Added(globalId, entry.Obj.Classification, entry.Obj.PredefinedType, entry.Fp.ContentKey);

    static ElementChange Removed(string globalId, (Node.Object Obj, ElementFingerprint Fp) entry) =>
        new ElementChange.Removed(globalId, entry.Obj.Classification, entry.Obj.PredefinedType, entry.Fp.ContentKey);

    static ContentAddress ContentKey(ElementGraph graph, Node.Object node) {
        double tolerance = graph.Header.Tolerance;
        Seq<UInt128> contributions = toSeq(toSeq(graph.EdgesAt(node.Id))
            .Filter(edge => edge.Relating == node.Id)
            .Map(edge => BoundContribution(graph, node.Id, edge, tolerance))
            .OrderBy(static contribution => contribution));
        return ContentAddress.Create(ContentHash.Of((Node: node, Contributions: contributions), static (state, w) => {
            w.String(state.Node.Kind.Key).String(state.Node.Classification.System).String(state.Node.Classification.Code)
                .String(state.Node.PredefinedType.ToValue()).String(state.Node.Name).String(state.Node.Tag)
                .Ordinal(state.Contributions.Count);
            foreach (UInt128 contribution in state.Contributions) { w.U128(contribution); }
        }));
    }

    static UInt128 BoundContribution(ElementGraph graph, NodeId self, Relationship edge, double tolerance) {
        NodeId far = edge.Relating == self ? edge.Related : edge.Relating;
        Option<UInt128> bound = graph.Find(far).Bind(node => node is not Node.Object ? Some(ContentAddress.Of(node, tolerance).ToValue()) : None);
        return ContentHash.Of((Edge: ContentAddress.Of(edge, tolerance).ToValue(), Bound: bound), static (state, w) => {
            w.U128(state.Edge).Bool(state.Bound.IsSome);
            state.Bound.IfSome(value => w.U128(value));
        });
    }

    static ContentAddress PlacementKey(Node.Object node, double tolerance) =>
        ContentAddress.Of(node, tolerance, static (n, w) => {
            w.Bool(n.Placement.IsSome);
            n.Placement.IfSome(placement => placement.CanonicalBytes(w));
            w.Ordinal(n.Representations.ByIdentifier.Count);
            foreach (var (slot, hash) in n.Representations.ByIdentifier.OrderBy(static pair => pair.Key.Key)) {
                w.Ordinal(slot.Key).U128(hash);
            }
        });

    static ImmutableArray<AspectDelta> Deltas(Element baseline, Element revision) =>
        [.. Element.EqualityComparer.Default.Inequalities(baseline, revision)
            .Select(static inequality => new AspectDelta(
                inequality.Path.ToString(),
                inequality.Path.Segments is [.., var terminal] ? DeltaShape.Of(terminal) : DeltaShape.Replace,
                DeltaValue.Of(inequality.Left),
                DeltaValue.Of(inequality.Right)))];

}
```

## [03]-[AUDIT]

- Owner: `AuditEntry` the immutable mutation-log row carrying the element `GlobalId`, the typed `ChangeKind`, the baseline/revision `ContentAddress` pair, the author, the `Instant`, the version pointer, and the chained `EntryKey` content address keyed on the prior entry's key so a retroactive edit breaks the chain; `AuditVersion` the per-version authoring metadata; `AuditTrail` the append-only log folding the per-version `ModelDiff` change-sets into the chained entry sequence, queryable by element `GlobalId`. The trail is a model-mutation log (who/when/what-changed-semantically), explicitly distinct from the geometry-asset XMP lineage (who-minted-this-GLB) and from the branching commit-DAG.
- Entry: `AuditTrail.Fold(Seq<(AuditVersion Version, ModelDiff Diff)> history)` folds a version sequence of `ModelDiff` change-sets into the chained `AuditTrail` — each `ElementChange` arm in each version's diff projects onto one `AuditEntry` (the typed `ChangeKind`, the version's baseline/revision content keys, the version's author and `Instant`), and the `EntryKey` chains on the prior entry's key through the shared `Rasm.Element/Projection/address#CONTENT_ADDRESS` codec so the log is tamper-evident — re-folding a tampered history yields a divergent terminal `EntryKey`; the fold is total, pure, no result. `AuditTrail.For(string globalId)` folds every entry an element underwent into its lifecycle history in chain order, and `AuditTrail.Verify()` re-derives the chain to witness no retroactive edit broke it.
- Auto: `Fold` threads the `(Prior, Rows)` accumulator across the version sequence — for each version it folds the version's `ModelDiff.Changes` onto `AuditEntry` rows in `GlobalId`-ordinal order (the order-stable chain `Verify` re-derives) (each carrying the element `GlobalId` and `ChangeKind` the change names through its base accessors, the version content keys decomposed by the one `Keys` switch, the version pointer and author/`Instant`, and the prior `EntryKey` as `ParentKey`), computes each entry's `EntryKey` as the shared `ContentAddress` over the prior key concatenated with the entry's canonical content, and threads the new key as the next entry's parent so the chain is a content-addressed Merkle-like sequence — the same content-key idiom the `Review/versioning#VERSION_GRAPH` `BimCommit.CommitKey` and the `Rasm.Persistence/Version/provenance#ATTESTED_LEDGER` `AttestedEntry.Chain` carry; `For(globalId)` filters the folded entries to the element preserving chain order; `Verify` re-folds the entry contents recomputing each `EntryKey` from the recorded parent through the SAME `EntryKey` projection `Fold` used and compares the recomputed key against the stored one, so a single retroactive field edit diverges every downstream key and the boolean witnesses chain integrity without a separate stored checksum.
- Output: the `AuditTrail` chained `Seq<AuditEntry>` is the compliance evidence (who/when/from-what per element) the federation and compliance consumer read, `AuditTrail.For(globalId)` the per-element lifecycle history anchoring a `Review/issues#BCF_ARCHIVE` topic and the `Review/versioning#VERSION_GRAPH` merge, and `AuditTrail.Verify()` the tamper-evidence witness; the durable append-only store is the `Rasm.Persistence/Version/provenance#ATTESTED_LEDGER` concern joined at the `Review/diff → Rasm.Persistence/Version/provenance # [CONTENT_KEY]: AuditEntry chained ElementChange mutation log` contract by the content-key, this owner producing the chained host-neutral log and its content-key identity, the durable signed ledger riding the Persistence ripple.
- Packages: Rasm.Element, Thinktecture.Runtime.Extensions, NodaTime, LanguageExt.Core, Rasm
- Growth: a new audit field is one column on `AuditEntry` folded into the `EntryKey` content; a new version-metadata dimension is one column on `AuditVersion`; a new lifecycle query is one fold over the same chained entry sequence; never a per-change-kind audit record, never a second mutation store, and never a checksum beside the chained `EntryKey`.
- Boundary: the audit trail keys on the `[02]-[MODEL_DIFF]` `ElementChange` and the version lineage, explicitly distinct from the Wave A tile-XMP geometry-asset provenance — the two stay separate, the audit trail never keying on the export artifact content-key; the chain is the content-addressed `EntryKey` through the kernel `Rasm/Domain/identity#CONTENT_KEY` `CanonicalWriter` codec hashed by the shared `Projection/address#CONTENT_ADDRESS` `ContentAddress` (the kernel seed-zero `XxHash128`, the ONE hasher), and the retired hand-rolled `XxHash128.HashToUInt128(Encoding.UTF8.GetBytes($"..."))` string-interpolation chain plus a separate stored checksum or a mutable sequence-number beside the `EntryKey` are the deleted forms; the `ChangeKind` is the typed `[SmartEnum<string>]` projected by `ElementChange.Kind` and a stringly `"added"`/`"removed"` literal is the deleted form; the keys are `ContentAddress` values, never raw `UInt128`; the fold consumes the `[02]-[MODEL_DIFF]` `ModelDiff` change-sets as settled vocabulary and mints no second diff or element shape; each version's change set sorts by `GlobalId` ordinal before it chains and folding `ModelDiff.Changes` in its native partition order is the deleted form — the chain is order-dependent, so two folds of one history mint divergent terminal keys and `Verify` reports tampering that never happened; the trail is a pure fold over the version-and-diff sequence, never an imperative append loop with mutable accumulation; the audit trail is the LINEAR per-element who/when/what Merkle chain, distinct from the `Review/versioning#VERSION_GRAPH` branching commit-DAG, neither re-derived from the other, both reading the one content-key space; the durable append-only store is the `Rasm.Persistence/Version/provenance#ATTESTED_LEDGER` concern joined at the content-key contract and a durable store minted here is the named contract violation; the fold is TOTAL and carries no fault channel — it consumes the `[02]-[MODEL_DIFF]` `ModelDiff` change-sets whose `GlobalId`s the diff already resolved from real graph nodes, so a dangling reference cannot arise at the audit boundary (the diff's `Bake` composition is the one place a corrupt subgraph returns `Rasm.Element/Projection/fault#FAULT_BAND` `ElementFault`), and a fabricated `Fin`/`BimFault` result the body never produces is the illusory form.

```csharp
// --- [IMPORTS] -------------------------------------------------------------------------
using LanguageExt;
using NodaTime;
using Rasm.Domain;
using Rasm.Element.Classification;
using Rasm.Element.Graph;
using Rasm.Element.Projection;
using Rasm.Element.Properties;
using Rasm.Element.Relations;
using static LanguageExt.Prelude;

namespace Rasm.Bim;

// --- [MODELS] --------------------------------------------------------------------------
public sealed record AuditVersion(string VersionId, string Author, Instant At);

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
    static readonly ContentAddress Genesis = ContentAddress.Create(UInt128.Zero);

    public static AuditTrail Fold(Seq<(AuditVersion Version, ModelDiff Diff)> history) =>
        new(history.Fold((Prior: Genesis, Rows: Seq<AuditEntry>()), static (state, step) =>
            toSeq(step.Diff.Changes
                    .OrderBy(static c => c.GlobalId, StringComparer.Ordinal)
                    .ThenBy(static c => c.Kind.Key, StringComparer.Ordinal))
                .Fold(state, (acc, change) => {
                AuditEntry entry = Chain(acc.Prior, change, step.Version);
                return (entry.EntryKey, acc.Rows.Add(entry));
            })).Rows);

    public Seq<AuditEntry> For(string globalId) =>
        Entries.Filter(entry => string.Equals(entry.GlobalId, globalId, StringComparison.Ordinal));

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

    static (ContentAddress Baseline, ContentAddress Revision) Keys(ElementChange change) => change.Switch(
        added:    static c => (Genesis, c.Content),
        removed:  static c => (c.Content, Genesis),
        modified: static c => (c.BaselineContent, c.RevisionContent),
        moved:    static c => (c.BaselinePlacement, c.RevisionPlacement),
        split:    static c => (c.Content, Genesis),
        merged:   static c => (Genesis, c.Content));

    static ContentAddress EntryKey(ContentAddress prior, string globalId, ChangeKind kind, ContentAddress baseline, ContentAddress revision, string versionId, string author, Instant at) =>
        ContentAddress.Create(ContentHash.Of(
            (Prior: prior, GlobalId: globalId, Kind: kind, Baseline: baseline, Revision: revision, VersionId: versionId, Author: author, At: at),
            static (s, w) => w.U128(s.Prior.ToValue()).String(s.GlobalId).String(s.Kind.Key).U128(s.Baseline.ToValue()).U128(s.Revision.ToValue())
                .String(s.VersionId).String(s.Author).I64(s.At.ToUnixTimeTicks())));
}
```

## [04]-[RESEARCH]

(none)
