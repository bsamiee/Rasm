using Rasm.Domain;
using Rasm.Rhino.Blocks;
using Rasm.Rhino.Exchange;
using Rasm.TestKit;
using Rhino;
using Rhino.Display;
using Rhino.DocObjects;
using Rhino.Geometry;

namespace Rasm.Rhino.Tests.Blocks;

// --- [CONSTANTS] ------------------------------------------------------------------------
internal static class BlockStateCases {
    public static readonly Op Key = Op.Of(name: "blocks-state-test");

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
        Assert.True(condition: LayerStyle.None.AppliesTo(policy: UpdatePolicy.Static));
        Assert.True(condition: LayerStyle.None.AppliesTo(policy: UpdatePolicy.LinkedAndEmbedded));
        Assert.True(condition: LayerStyle.Active.AppliesTo(policy: UpdatePolicy.Linked));
        Assert.True(condition: LayerStyle.Reference.AppliesTo(policy: UpdatePolicy.Linked));
        Assert.False(condition: LayerStyle.Reference.AppliesTo(policy: UpdatePolicy.LinkedAndEmbedded));
        Assert.True(condition: ArchiveStatus.LinkedFileIsDifferent.RequiresRefresh);
        Assert.True(condition: ArchiveStatus.LinkedFileIsDifferent.CanRefresh);
        Assert.True(condition: ArchiveStatus.LinkedFileNotFound.IsBroken);
        Assert.False(condition: ArchiveStatus.NotALinked.RequiresRefresh);
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

        _ = Assert.IsType<BlockOp.Place>(@object: place);
        _ = Assert.IsType<BlockOp.Graph>(@object: graph);
        _ = Assert.IsType<BlockOp.Bounds>(@object: bounds);
        _ = Assert.IsType<BlockOp.AttributeMatrix>(@object: matrix);
        _ = Assert.IsType<BlockOutcome.Receipt>(@object: receipt);
        _ = Assert.IsType<BlockOutcome.Bounds>(@object: boundsOutcome);
        _ = Assert.IsType<BlockOutcome.AttributeMatrix>(@object: attributes);
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

