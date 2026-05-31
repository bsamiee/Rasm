using Rasm.Domain;
using Rasm.Rhino.Blocks;
using Rasm.Rhino.Commands;
using Rasm.Rhino.Exchange;
using Rasm.TestKit;
using Rhino;
using Rhino.Display;
using Rhino.DocObjects;
using Rhino.DocObjects.Tables;
using Rhino.Geometry;

namespace Rasm.Rhino.Tests.Blocks;

// --- [CONSTANTS] ------------------------------------------------------------------------
internal static class BlockStateCases {
    public static readonly Op Key = Op.Of(name: "blocks-state-test");
    public static readonly (LayerStyle Layer, UpdatePolicy Update, bool Applies)[] LayerMatrix =
        [(LayerStyle.None, UpdatePolicy.Static, true), (LayerStyle.None, UpdatePolicy.LinkedAndEmbedded, true),
         (LayerStyle.None, UpdatePolicy.Linked, false), (LayerStyle.Active, UpdatePolicy.Static, false),
         (LayerStyle.Active, UpdatePolicy.LinkedAndEmbedded, false), (LayerStyle.Active, UpdatePolicy.Linked, true),
         (LayerStyle.Reference, UpdatePolicy.Static, false), (LayerStyle.Reference, UpdatePolicy.LinkedAndEmbedded, false),
         (LayerStyle.Reference, UpdatePolicy.Linked, true)];
    public static readonly (ArchiveStatus Status, bool Refresh, bool CanRefresh, bool Broken)[] ArchiveMatrix =
        [(ArchiveStatus.NotALinked, false, false, false), (ArchiveStatus.LinkedFileNotReadable, true, false, true),
         (ArchiveStatus.LinkedFileNotFound, true, false, true), (ArchiveStatus.LinkedFileIsUpToDate, false, true, false),
         (ArchiveStatus.LinkedFileIsNewer, true, true, false), (ArchiveStatus.LinkedFileIsOlder, true, true, false),
         (ArchiveStatus.LinkedFileIsDifferent, true, true, false)];

    internal static LiveStats SampleLive(UpdatePolicy update, LayerStyle layer, bool isReference = false) =>
        new(
            Units: UnitSystem.Meters,
            Update: update,
            Layer: layer,
            Archive: ArchiveStatus.NotALinked,
            IsDeleted: false,
            IsTenuous: false,
            IsReference: isReference,
            SkipNestedLinked: false,
            DeletedName: Option<string>.None,
            MemberCount: 1,
            UseCountTop: 0,
            UseCountNested: 0);
}

// --- [ALGEBRAIC] ------------------------------------------------------------------------
public sealed class BlockPolicyLaws {
    [Fact]
    public void UpdateLayerAndLifecyclePoliciesAdmitOnlyNativeStates() {
        Spec.SmartEnumKeysUnique(items: [UpdatePolicy.Static, UpdatePolicy.LinkedAndEmbedded, UpdatePolicy.Linked], key: static policy => policy.Key);
        Spec.SmartEnumKeysUnique(items: [LayerStyle.None, LayerStyle.Active, LayerStyle.Reference], key: static style => style.Key);
        Spec.Cases(items: BlockStateCases.LayerMatrix, key: static row => (row.Layer.Key, row.Update.Key), law: static row =>
            Assert.Equal(expected: row.Applies, actual: row.Layer.AppliesTo(policy: row.Update)));
        Spec.Cases(items: BlockStateCases.ArchiveMatrix, key: static row => row.Status.Key, law: static row => {
            Assert.Equal(expected: row.Refresh, actual: row.Status.RequiresRefresh);
            Assert.Equal(expected: row.CanRefresh, actual: row.Status.CanRefresh);
            Assert.Equal(expected: row.Broken, actual: row.Status.IsBroken);
        });
        Assert.Same(expected: UpdatePolicy.Linked, actual: UpdatePolicy.FromNative(InstanceDefinitionUpdateType.Linked));
        Assert.Same(expected: UpdatePolicy.LinkedAndEmbedded, actual: UpdatePolicy.FromNative(InstanceDefinitionUpdateType.LinkedAndEmbedded));
        Assert.Same(expected: UpdatePolicy.Static, actual: UpdatePolicy.FromNative(InstanceDefinitionUpdateType.Static));
        Spec.Succ(DefinitionPrefix.From(value: " family ", key: BlockStateCases.Key), then: prefix =>
            Assert.Equal(expected: "family", actual: prefix.Value));
        Spec.Fail(DefinitionPrefix.From(value: "/family", key: BlockStateCases.Key));
    }

    [Fact]
    public void PublicPoliciesRejectInvalidDirectConstruction() {
        Spec.Fail((PreviewSpec.Default with { Width = 8 }).Admit(key: BlockStateCases.Key));
        Spec.Fail(new WatchPolicy(Debounce: TimeSpan.Zero).Admit(key: BlockStateCases.Key));
        Spec.Fail(new DepthPolicy(MaxDepth: 0).Admit(key: BlockStateCases.Key));
        Spec.Fail(new DepthPolicy(MaxDepth: 65).Admit(key: BlockStateCases.Key));
        Spec.Succ(DepthPolicy.Reach.Admit(key: BlockStateCases.Key), then: policy =>
            Assert.Equal(expected: 8, actual: policy.MaxDepth));
        Spec.Fail(new LinkCreatePolicy(Update: UpdatePolicy.Static, Layer: LayerStyle.Reference).Admit(key: BlockStateCases.Key));
        Spec.Succ(new LinkCreatePolicy(Update: UpdatePolicy.Linked, Layer: LayerStyle.Reference).Admit(key: BlockStateCases.Key), then: policy =>
            Assert.Equal(expected: LinkReloadPolicy.NestedQuiet, actual: policy.EffectiveReload));
        TimeProvider clock = TimeProvider.System;
        Spec.Succ(new WatchPolicy(Debounce: TimeSpan.FromMilliseconds(value: 250), Clock: clock).Admit(key: BlockStateCases.Key), then: policy =>
            Assert.Same(expected: clock, actual: policy.EffectiveClock));
        LiveStats linkedLive = BlockStateCases.SampleLive(update: UpdatePolicy.Linked, layer: LayerStyle.Reference);
        LiveStats staticLive = BlockStateCases.SampleLive(update: UpdatePolicy.Static, layer: LayerStyle.None);
        Spec.Succ(Placement.Of(xform: Transform.Identity, reference: true).Admit(live: linkedLive, key: BlockStateCases.Key));
        Spec.Fail(Placement.Of(xform: Transform.Identity, reference: true).Admit(live: staticLive, key: BlockStateCases.Key));
        Spec.Succ(FileEndpoint.From(path: "/tmp/rasm-blocks-source.3dm"), then: endpoint => {
            DefinitionRef refer = DefinitionRef.Of(DefinitionName.Create(value: "Linked"));
            LinkMap valid = new(Ref: refer, Source: endpoint, Update: UpdatePolicy.LinkedAndEmbedded, Reload: LinkReloadPolicy.ShallowVerbose);
            Spec.Succ(valid.Admit(key: BlockStateCases.Key), then: admitted =>
                Assert.Equal(expected: LinkReloadPolicy.ShallowVerbose, actual: admitted.EffectiveReload));
            Spec.Fail((valid with { Update = UpdatePolicy.Static }).Admit(key: BlockStateCases.Key));
        });
    }
}

public sealed class PreviewSpecLaws {
    [Fact]
    public void FingerprintChangesForEveryNativePreviewAxis() {
        PreviewSpec baseline = PreviewSpec.Default;
        Assert.NotEqual(expected: baseline.Fingerprint, actual: (baseline with { Width = 320 }).Fingerprint);
        Assert.NotEqual(expected: baseline.Fingerprint, actual: (baseline with { Height = 320 }).Fingerprint);
        Assert.NotEqual(expected: baseline.Fingerprint, actual: (baseline with { Projection = DefinedViewportProjection.Top }).Fingerprint);
        Assert.NotEqual(expected: baseline.Fingerprint, actual: (baseline with { Camera = IsometricCamera.Southwest }).Fingerprint);
        Assert.NotEqual(expected: baseline.Fingerprint, actual: (baseline with { DisplayMode = DisplayModeRef.Of(id: Guid.Parse(input: "3f1e9f89-f4e6-4e5c-95f7-a848a0833cfa")) }).Fingerprint);
        Assert.NotEqual(expected: baseline.Fingerprint, actual: (baseline with { DrawDecorations = true }).Fingerprint);
        Assert.NotEqual(expected: baseline.Fingerprint, actual: (baseline with { ApplyDpiScaling = true }).Fingerprint);
        Assert.NotEqual(expected: baseline.Fingerprint, actual: (baseline with { HighlightMemberId = Some(Guid.Parse(input: "7d3842a5-c31a-44c7-8a2d-1a7c67c0c81d")) }).Fingerprint);
    }

    [Fact]
    public void FingerprintCanonicalizesDisplayNameCaseButDistinguishesIdMode() {
        PreviewSpec named = PreviewSpec.Default with { DisplayMode = DisplayModeRef.Of(name: "Rendered") };
        PreviewSpec cased = PreviewSpec.Default with { DisplayMode = DisplayModeRef.Of(name: "rendered") };
        PreviewSpec id = PreviewSpec.Default with { DisplayMode = DisplayModeRef.Of(id: Guid.Parse(input: "3f1e9f89-f4e6-4e5c-95f7-a848a0833cfa")) };

        Assert.Equal(expected: named.Fingerprint, actual: cased.Fingerprint);
        Assert.NotEqual(expected: named.Fingerprint, actual: id.Fingerprint);
    }
}

public sealed class BlockStateAdmissionLaws {
    [Fact]
    public void DefinitionNamesPathsAndArchivesNormalizeOrRejectAtBoundaries() {
        Spec.Succ(DefinitionPrefix.From(value: " Family ", key: BlockStateCases.Key), then: prefix =>
            Assert.Equal(expected: "Family", actual: prefix.Value));
        Spec.Fail(DefinitionPrefix.From(value: string.Empty, key: BlockStateCases.Key));
        Spec.Fail(DefinitionPrefix.From(value: new string(c: 'a', count: DefinitionPrefix.MaxLength + 1), key: BlockStateCases.Key));
        Spec.Fail(DefinitionPrefix.From(value: "Family/Unit", key: BlockStateCases.Key));
        Spec.Fail(ArchivePath.From(value: "relative/source.3dm", key: BlockStateCases.Key));
        string anchor = Path.Combine(path1: Path.GetTempPath(), path2: "rasm-blocks-anchor");
        string absolute = Path.Combine(path1: anchor, path2: "source.3dm");
        Spec.Succ(ArchiveLink.Resolve(raw: "source.3dm", anchorDirectory: Some(anchor), key: BlockStateCases.Key), then: link => {
            Assert.Equal(expected: Path.GetFullPath(path: absolute), actual: link.Full.Value);
            Spec.Some(link.Relative, relative => Assert.Equal(expected: "source.3dm", actual: relative));
            Assert.True(condition: ArchiveLink.Matches(stored: "source.3dm", candidate: absolute, anchorDirectory: Some(anchor)));
        });
    }

    [Fact]
    public void TransformPoliciesAndMutationReceiptsPreserveIndependentReceiptSlots() {
        Guid instanceId = Guid.Parse(input: "11111111-1111-1111-1111-111111111111");
        Guid resultId = Guid.Parse(input: "22222222-2222-2222-2222-222222222222");
        DocumentReceipt copy = TransformPolicy.Copy.InstanceTransform(instanceId: instanceId, resultId: resultId);
        DocumentReceipt move = TransformPolicy.Move.InstanceTransform(instanceId: instanceId, resultId: resultId);
        DocumentReceipt history = TransformPolicy.History.InstanceTransform(instanceId: instanceId, resultId: resultId);
        Assert.Equal(expected: Seq(resultId), actual: copy.Created);
        Assert.Equal(expected: Seq(resultId), actual: copy.Transformed);
        Assert.Equal(expected: Seq(resultId), actual: move.Created);
        Assert.Equal(expected: Seq(instanceId), actual: move.Deleted);
        Assert.Equal(expected: Seq(resultId), actual: history.Created);
        Assert.True(condition: history.Deleted.IsEmpty && history.Transformed.IsEmpty);
        MutationReceipt left = MutationReceipt.Named(name: "A");
        MutationReceipt right = MutationReceipt.Lifecycle(id: instanceId, name: "B");
        MutationReceipt combined = left + right;
        Assert.Equal(expected: 2, actual: combined.Document.ResourceChanged.Count);
        Assert.Equal(expected: Seq(instanceId), actual: combined.Document.LifecycleChanged);
    }

    [Fact]
    public void ClosureDepthPolicyAcceptsOnlyBoundedArchiveWalks() {
        Spec.Succ(ClosureValidationPolicy.Default.Admit(key: BlockStateCases.Key));
        Spec.Succ(new ClosureValidationPolicy(DetectCycles: false, MaxDepth: Archive.LinkedArchiveClosureMaxDepth).Admit(key: BlockStateCases.Key));
        Spec.Fail(new ClosureValidationPolicy(DetectCycles: true, MaxDepth: 0).Admit(key: BlockStateCases.Key));
        Spec.Fail(new ClosureValidationPolicy(DetectCycles: true, MaxDepth: Archive.LinkedArchiveClosureMaxDepth + 1).Admit(key: BlockStateCases.Key));
    }
}

public sealed class BlockHealthAndSubscriptionLaws {
    private static Definition DefinitionOf(LiveStats live, Option<ArchiveLink> source = default) =>
        new(
            Id: DefinitionId.From(value: Guid.Parse(input: "8983d56c-7e2f-41bf-b365-4c2863f4c82c")).IfFail(error => throw new InvalidOperationException(message: error.Message)),
            Index: Option<DefinitionIndex>.None,
            Name: DefinitionName.Create(value: "Linked"),
            Description: Option<string>.None,
            Url: Option<ArchivePath>.None,
            UrlDescription: Option<string>.None,
            Source: source,
            MemberIds: [],
            Live: Some(live));

    [Fact]
    public void LinkedHealthSeparatesInvalidLayerAndArchiveDiagnostics() {
        LiveStats invalidLayer = BlockStateCases.SampleLive(update: UpdatePolicy.Static, layer: LayerStyle.Reference);
        LiveStats brokenLink = BlockStateCases.SampleLive(update: UpdatePolicy.Linked, layer: LayerStyle.Reference) with {
            Archive = ArchiveStatus.LinkedFileNotFound,
        };
        LinkedHealth invalid = DefinitionOf(live: invalidLayer).ToLinkedHealth().IfNone(() => throw new InvalidOperationException(message: "missing health"));
        LinkedHealth broken = DefinitionOf(live: brokenLink).ToLinkedHealth().IfNone(() => throw new InvalidOperationException(message: "missing health"));

        Assert.Contains(collection: invalid.Diagnostics, filter: static diagnostic => diagnostic is BlockDiagnostic.InvalidLayerStyle);
        Assert.Contains(collection: invalid.Diagnostics, filter: static diagnostic => diagnostic is BlockDiagnostic.LinkedArchiveIssue);
        Assert.Contains(collection: broken.Diagnostics, filter: static diagnostic => diagnostic is BlockDiagnostic.LinkedArchiveIssue { Status: { } status } && status == ArchiveStatus.LinkedFileNotFound);
    }

    [Fact]
    public void RefreshPlanPartitionsRefreshableAndBlockedLinkedDefinitions() {
        ArchiveLink source = ArchiveLink.Resolve(raw: "/tmp/linked.3dm", anchorDirectory: Option<string>.None, key: BlockStateCases.Key)
            .IfFail(error => throw new InvalidOperationException(message: error.Message));
        LinkedHealth refreshable = DefinitionOf(
            live: BlockStateCases.SampleLive(update: UpdatePolicy.Linked, layer: LayerStyle.Reference) with { Archive = ArchiveStatus.LinkedFileIsNewer },
            source: Some(source)).ToLinkedHealth().IfNone(() => throw new InvalidOperationException(message: "missing refreshable"));
        LinkedHealth blocked = DefinitionOf(
            live: BlockStateCases.SampleLive(update: UpdatePolicy.Linked, layer: LayerStyle.Reference) with { Archive = ArchiveStatus.LinkedFileNotFound },
            source: Some(source)).ToLinkedHealth().IfNone(() => throw new InvalidOperationException(message: "missing blocked"));
        RefreshPlan plan = new(Items: Seq(refreshable, blocked));

        Assert.Equal(expected: 1, actual: plan.Refreshable.Count);
        Assert.Equal(expected: ArchiveStatus.LinkedFileIsNewer, actual: plan.Refreshable[0].Archive);
        Assert.Equal(expected: 1, actual: plan.Blocked.Count);
        Assert.False(condition: plan.Blocked[0].Diagnostics.IsEmpty);
    }

    [Fact]
    public void SubscriptionPolicyConjoinsFiltersAndKeepsIdleDeferralSticky() {
        BlockSubscriptionPolicy namedOnly = new(Filter: Some<Func<BlockTableEvent, bool>>(e => e.New.Map(static d => string.Equals(a: d.Name.Value, b: "Linked", comparisonType: StringComparison.Ordinal)).IfNone(false)), DeferToIdle: false);
        BlockSubscriptionPolicy combined = BlockSubscriptionPolicy.MutationsOnly | namedOnly;
        BlockTableEvent sorted = new(Kind: InstanceDefinitionTableEventType.Sorted, Index: 0, New: Option<Definition>.None, Old: Option<Definition>.None);
        BlockTableEvent added = new(Kind: InstanceDefinitionTableEventType.Added, Index: 0, New: Some(DefinitionOf(live: BlockStateCases.SampleLive(update: UpdatePolicy.Linked, layer: LayerStyle.Reference))), Old: Option<Definition>.None);

        Assert.True(condition: combined.DeferToIdle);
        Assert.False(condition: combined.Filter.IfNone(static _ => true)(arg: sorted));
        Assert.True(condition: combined.Filter.IfNone(static _ => false)(arg: added));
    }
}

public sealed class BlockRailLaws {
    [Fact]
    public void UnifiedOperationRailCarriesMutationAndQueryOutcomes() {
        DefinitionName name = DefinitionName.Create(value: "UnitBlock");
        DefinitionRef refer = DefinitionRef.Of(name);
        BlockOp place = new BlockOp.Place(
            Ref: refer,
            At: Seq(Placement.Of(xform: Transform.Identity)),
            Policy: BatchPolicy.Default);
        BlockOp graph = new BlockOp.Graph(Query: new GraphQuery.Members(Ref: refer));
        BlockOp bounds = new BlockOp.Bounds(Ref: refer, Policy: BoundsPolicy.Default);
        BlockOp matrix = new BlockOp.AttributeMatrix(
            Refs: Some(Seq(refer)),
            Scope: ReferenceScope.TopAndNested);
        BlockOp validate = new BlockOp.ValidateArchiveClosure(
            Source: FileEndpoint.From(path: "parent.3dm").IfFail(error => throw new InvalidOperationException(message: error.Message)));
        BlockOp writeAttributes = new BlockOp.WriteAttributeFields(
            Ref: refer,
            Values: HashMap<string, string>().AddOrUpdate(key: "Mark", value: "A"),
            Policy: ConstraintPolicy.Schema);
        BlockOutcome closure = new BlockOutcome.ClosureReport(Value: new ArchiveClosureReport(
            Valid: true,
            Edges: Seq<Archive.LinkedArchiveEdge>(),
            Broken: Seq<ArchivePath>(),
            Cycles: Seq<Seq<ArchivePath>>()));
        BlockOutcome receipt = new BlockOutcome.Receipt(Value: MutationReceipt.Named(name: name.Value));
        BlockOutcome boundsOutcome = new BlockOutcome.Bounds(Value: BoundingBox.Empty);
        AttributeCell cell = new(
            DefId: DefinitionId.From(value: Guid.Parse(input: "7d3842a5-c31a-44c7-8a2d-1a7c67c0c81d")).IfFail(error => throw new InvalidOperationException(message: error.Message)),
            DefName: name,
            InstanceId: Option<Guid>.None,
            Key: "Mark",
            Prompt: "Mark",
            DefaultValue: "A");
        BlockOutcome attributes = new BlockOutcome.AttributeMatrix(Values: Seq(cell));

        BlockOp.Place placed = Assert.IsType<BlockOp.Place>(@object: place);
        Assert.Equal(expected: refer, actual: placed.Ref);
        _ = Assert.Single(collection: placed.At);
        Assert.Equal(expected: BatchPolicy.Default, actual: placed.Policy);
        Assert.Equal(expected: refer, actual: Assert.IsType<GraphQuery.Members>(@object: Assert.IsType<BlockOp.Graph>(@object: graph).Query).Ref);
        Assert.Equal(expected: BoundsPolicy.Default, actual: Assert.IsType<BlockOp.Bounds>(@object: bounds).Policy);
        Assert.Equal(expected: ReferenceScope.TopAndNested, actual: Assert.IsType<BlockOp.AttributeMatrix>(@object: matrix).Scope);
        Assert.Equal(expected: "parent.3dm", actual: Path.GetFileName(path: Assert.IsType<FileEndpoint>(@object: Assert.IsType<BlockOp.ValidateArchiveClosure>(@object: validate).Source).Path));
        Assert.Equal(expected: "A", actual: Assert.IsType<BlockOp.WriteAttributeFields>(@object: writeAttributes).Values.Find("Mark").IfNone(string.Empty));
        Assert.True(condition: Assert.IsType<BlockOutcome.ClosureReport>(@object: closure).Value.Valid);
        Assert.Equal(expected: name.Value, actual: Assert.IsType<BlockOutcome.Receipt>(@object: receipt).Value.Document.ResourceChanged[0].Name);
        Assert.Equal(expected: BoundingBox.Empty, actual: Assert.IsType<BlockOutcome.Bounds>(@object: boundsOutcome).Value);
        Assert.Equal(expected: "Mark", actual: Assert.IsType<BlockOutcome.AttributeMatrix>(@object: attributes).Values[0].Key);
    }

    [Fact]
    public void CollapsedGraphExplodeAndFilterSurfacesCarryNativeSemantics() {
        DefinitionRef refer = DefinitionRef.Of(DefinitionName.Create(value: "Probe"));
        Assert.True(condition: new ExplodePolicy.Native().UsesNativePieces);
        Assert.False(condition: new ExplodePolicy.All().UsesNativePieces);
        Assert.Equal(expected: BoundsPolicy.Default, actual: new BoundsPolicy(Accurate: true));
        Assert.Equal(expected: BakePolicy.Default.EffectiveConflict, actual: ConflictPolicy.Rename);
        _ = Assert.IsType<GraphQuery.Members>(@object: new GraphQuery.Members(Ref: refer));
        _ = Assert.IsType<GraphQuery.Audit>(@object: new GraphQuery.Audit());
        _ = Assert.IsType<GraphQuery.Plan>(@object: new GraphQuery.Plan());
        _ = Assert.IsType<GraphQuery.Stats>(@object: new GraphQuery.Stats());
        _ = Assert.IsType<GraphQuery.Health>(@object: new GraphQuery.Health());
        _ = Assert.IsType<GraphQuery.Reach>(@object: new GraphQuery.Reach(Ref: refer, Scope: ReferenceScope.Top));
        _ = Assert.IsType<BlockSubscriptionPolicy>(@object: BlockSubscriptionPolicy.MutationsOnly);
        _ = Assert.IsType<Members.FromConstruction>(@object: new Members.FromConstruction(Geometry: Seq<GeometryBase>(), Attributes: Seq<ObjectAttributes>()));
        _ = Assert.IsType<BlockOutcome.ReachInserts>(@object: new BlockOutcome.ReachInserts(Values: Seq<ReachInsert>()));
        _ = Assert.IsType<BlockOp.TransformInstance>(@object: new BlockOp.TransformInstance(InstanceId: Guid.Empty, Xform: Transform.Identity));
        _ = Assert.IsType<BlockOutcome.TableStats>(@object: new BlockOutcome.TableStats(Count: 1, ActiveCount: 1));
        _ = Assert.IsType<BlockOutcome.Plan>(@object: new BlockOutcome.Plan(Order: Seq<DefinitionId>()));
        _ = Assert.IsType<BlockFilter>(@object: BlockFilter.All);
        _ = Assert.IsType<DependencyTarget.InUse>(@object: new DependencyTarget.InUse(Scope: ReferenceScope.Top));
    }
}

