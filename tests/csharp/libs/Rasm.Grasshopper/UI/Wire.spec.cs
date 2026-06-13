using Grasshopper2.Doc;
using Rasm.Grasshopper.UI;
using Rasm.TestKit;

namespace Rasm.Grasshopper.Tests.UI;

// --- [MODELS] ----------------------------------------------------------------------------
internal static class WireGens {
    public static readonly Gen<int> Caps = Gen.Int[start: 0, finish: 1024];
    public static readonly (WireEdit Edit, bool Connected, bool Source, bool Target)[] EditFlags =
        [(WireEdit.Connect, false, true, true), (WireEdit.Disconnect, true, true, true), (WireEdit.Delete, true, true, true),
         (WireEdit.DisconnectInputs, false, false, true), (WireEdit.DisconnectOutputs, false, true, false),
         (WireEdit.ConnectAt, false, true, true), (WireEdit.DisconnectInputsExcept, false, false, true),
         (WireEdit.DisconnectOutputsExcept, false, true, false), (WireEdit.CopyInputs, false, true, true),
         (WireEdit.MigrateInputs, false, true, true), (WireEdit.CopyOutputs, false, true, true),
         (WireEdit.MigrateOutputs, false, true, true), (WireEdit.ReplaceSource, true, true, true),
         (WireEdit.ReplaceTarget, true, true, true), (WireEdit.SwapSources, true, true, true),
         (WireEdit.CutOutMiddleMan, true, true, true)];
    // A flat object walk with deliberate duplicates so Distinct is load-bearing, and small element range so
    // saturation (cap >= length) is reachable.
    public static readonly Gen<int[]> Walk = Gen.Int[start: 0, finish: 12].Array[0, 48];

    // The object-count bound Wire.GraphObjects/GraphOf apply: Take(cap) over the flat walk, then Distinct.
    public static int[] Bounded(int[] walk, WireObjectLimit cap) => [.. walk.Take(count: cap.Value).Distinct()];
}

// --- [OPERATIONS] ----------------------------------------------------------------------------
public sealed class WireObjectLimitLaws {
    [Fact]
    public void AcceptsNonNegative() =>
        Spec.ValueObjectAccepts(valid: WireGens.Caps, tryCreate: static x => WireObjectLimit.TryCreate(value: x, obj: out _));

    [Fact]
    public void RejectsNegative() =>
        Spec.Cases(items: [-1, -32, int.MinValue], key: static x => x,
            law: static x => Assert.False(condition: WireObjectLimit.TryCreate(value: x, obj: out _)));

    [Fact]
    public void ValueRoundtripsAndDefaultIsThirtyTwo() {
        Spec.ValueObjectRoundtrip(validGen: WireGens.Caps, tryCreate: WireObjectLimit.TryCreate, read: static (WireObjectLimit h) => h.Value);
        Assert.Equal(expected: 32, actual: WireObjectLimit.DefaultCount);
        Assert.Equal(expected: 32, actual: WireObjectLimit.Create(value: WireObjectLimit.DefaultCount).Value);
    }
}

// Pins the object-count bounding CONTRACT that Wire.GraphObjects applies (Take-then-Distinct over a flat host
// walk) before any rename to MaxObjects: the bound is PREFIX-MONOTONE in the cap, SATURATES at the full
// distinct walk, and never yields more objects than the cap. Oracle: set containment + cardinality, independent
// of the projection. The live host-keyed walk itself is bridge-owned (needs GhObjectList).
public sealed class WireObjectBoundingLaws {
    [Fact]
    public void BoundingIsPrefixMonotone() =>
        Spec.ForAll(WireGens.Walk.Select(WireGens.Caps, WireGens.Caps), static tuple => {
            (int[] walk, int a, int b) = tuple;
            int[] small = WireGens.Bounded(walk: walk, cap: WireObjectLimit.Create(value: Math.Min(val1: a, val2: b)));
            int[] large = WireGens.Bounded(walk: walk, cap: WireObjectLimit.Create(value: Math.Max(val1: a, val2: b)));
            Assert.All(collection: small, action: x => Assert.Contains(expected: x, collection: large));
        });

    [Fact]
    public void SaturatesAtFullDistinctWalk() =>
        Spec.ForAll(WireGens.Walk, static walk =>
            Assert.Equal(expected: [.. walk.Distinct()], actual: WireGens.Bounded(walk: walk, cap: WireObjectLimit.Create(value: walk.Length))));

    [Fact]
    public void BoundedCountNeverExceedsCap() =>
        Spec.ForAll(WireGens.Walk.Select(WireGens.Caps), static tuple => {
            (int[] walk, int cap) = tuple;
            Assert.True(condition: WireGens.Bounded(walk: walk, cap: WireObjectLimit.Create(value: cap)).Length <= cap);
        });
}

public sealed class WireEditVocabularyLaws {
    [Fact]
    public void NativeRewireVerbsAreInTheEditAlgebra() =>
        Spec.Cases(
            items: [WireEdit.ReplaceSource, WireEdit.ReplaceTarget, WireEdit.SwapSources, WireEdit.CutOutMiddleMan],
            key: static edit => edit.Key,
            law: static edit => Assert.Contains(expected: edit, collection: WireEdit.Items));

    [Fact]
    public void EditAdmissionFlagsMatchEndpointSemanticsForEveryVerb() {
        Spec.SmartEnumKeysUnique(items: WireEdit.Items, key: static edit => edit.Key);
        Spec.Cases(items: WireGens.EditFlags, key: static row => row.Edit.Key, law: static row => {
            Assert.Equal(expected: row.Connected, actual: row.Edit.RequiresConnectedPair);
            Assert.Equal(expected: row.Source, actual: row.Edit.RequiresSource);
            Assert.Equal(expected: row.Target, actual: row.Edit.RequiresTarget);
        });
    }
}

public sealed class WireQueryAndPolicyLaws {
    [Fact]
    public void GraphQueryDefaultsAreBidirectionalAndBoundedToDefaultLimit() {
        Guid id = Guid.NewGuid();
        WireQuery.GraphCase graph = Assert.IsType<WireQuery.GraphCase>(@object: WireQuery.Graph(anchor: GraphKey.Owner(id)));
        _ = Assert.IsType<GraphKey.OwnerCase>(@object: graph.Anchor);
        Assert.Same(expected: WireTraversal.Bidirectional, actual: graph.Direction);
        Assert.Equal(expected: WireObjectLimit.DefaultCount, actual: graph.MaxObjects.Value);
        WireQuery.GraphCase limited = Assert.IsType<WireQuery.GraphCase>(@object: WireQuery.Graph(anchor: GraphKey.Parameter(id), direction: WireTraversal.ImmediateUpstream, maxObjects: 4));
        Assert.Same(expected: WireTraversal.ImmediateUpstream, actual: limited.Direction);
        Assert.Equal(expected: 4, actual: limited.MaxObjects.Value);
    }

    [Fact]
    public void GraphKeysPreserveSeedIdentityAcrossOwnerAndParameterModes() {
        Guid id = Guid.NewGuid();
        GraphKey.OwnerCase owner = Assert.IsType<GraphKey.OwnerCase>(@object: GraphKey.Owner(id: id));
        GraphKey.ParameterCase parameter = Assert.IsType<GraphKey.ParameterCase>(@object: GraphKey.Parameter(id: id));
        Assert.Equal(expected: id, actual: owner.SeedId);
        Assert.Equal(expected: id, actual: parameter.SeedId);
    }

    [Fact]
    public void WireIntentPoliciesSeparateDocumentQueriesCanvasHooksAndMutations() {
        Guid source = Guid.NewGuid();
        Guid target = Guid.NewGuid();
        WireSnapshot.ConnectedCase wire = new(Source: source, Target: target, SourceResolved: true, TargetResolved: true, Connected: true, Selected: false);
        WireOp[] documentQueries = [WireOp.Query(WireQuery.All)];
        WireOp[] canvasQueries = [WireOp.InstallShape(shapeType: typeof(object))];
        WireOp[] canvasHooks = [WireOp.Overlay(style: new WireOverlayStyle(Style: PaintStyle.Style(edge: Colors.Transparent))), WireOp.WirePaintObserve()];
        WireOp[] mutating = [WireOp.Select(WireSelectionOp.DeselectAll), WireOp.Split(wire: wire, location: PointF.Empty), WireOp.Edit(wire: wire, edit: WireEdit.Disconnect), WireOp.EditBatch((wire, WireEdit.Disconnect, default))];
        Assert.All(collection: documentQueries, action: op => {
            GrasshopperUiPolicy policy = GhUi.Wire(op: op).Policy;
            Assert.True(condition: policy.RequireCanvas && policy.RequireDocument);
            _ = Assert.IsType<RepaintRequest.NoneCase>(@object: policy.RepaintOrNone);
        });
        Assert.All(collection: canvasQueries, action: op => {
            GrasshopperUiPolicy policy = GhUi.Wire(op: op).Policy;
            Assert.True(condition: policy.RequireCanvas);
            Assert.False(condition: policy.RequireDocument);
            _ = Assert.IsType<RepaintRequest.NoneCase>(@object: policy.RepaintOrNone);
        });
        Assert.All(collection: canvasHooks, action: op => {
            GrasshopperUiPolicy policy = GhUi.Wire(op: op).Policy;
            Assert.True(condition: policy.RequireCanvas);
            Assert.False(condition: policy.RequireDocument);
            _ = Assert.IsType<RepaintRequest.NoneCase>(@object: policy.RepaintOrNone);
        });
        Assert.All(collection: mutating, action: op => {
            GrasshopperUiPolicy policy = GhUi.Wire(op: op).Policy;
            Assert.True(condition: policy.RequireCanvas && policy.RequireDocument);
            _ = Assert.IsType<RepaintRequest.CanvasCase>(@object: policy.RepaintOrNone);
        });
    }

    [Fact]
    public void EditBatchCapturesAllRowsInCallOrder() {
        WireSnapshot.ConnectedCase first = new(Source: Guid.NewGuid(), Target: Guid.NewGuid(), SourceResolved: true, TargetResolved: true, Connected: true, Selected: false);
        WireSnapshot.ConnectedCase second = new(Source: Guid.NewGuid(), Target: Guid.NewGuid(), SourceResolved: true, TargetResolved: true, Connected: true, Selected: false);
        WireOp.EditBatchCase batch = Assert.IsType<WireOp.EditBatchCase>(
            @object: WireOp.EditBatch((first, WireEdit.ReplaceSource, new WireEditArgs(Source: first.Source)), (second, WireEdit.ReplaceTarget, new WireEditArgs(Target: second.Target))));

        Assert.Equal(expected: 2, actual: batch.Edits.Count);
        Assert.Equal(expected: WireEdit.ReplaceSource, actual: batch.Edits[0].Kind);
        Assert.Equal(expected: first.Source, actual: batch.Edits[0].Args.Source);
        Assert.Equal(expected: WireEdit.ReplaceTarget, actual: batch.Edits[1].Kind);
        Assert.Equal(expected: second.Target, actual: batch.Edits[1].Args.Target);
    }
}

public sealed class PickToleranceLaws {
    [Fact]
    public void AcceptsNonNegativeFinite() =>
        Spec.ValueObjectAccepts(valid: Gen.Double[start: 0.0, finish: 1.0e3].Select(static (double d) => (float)d),
            tryCreate: static x => PickTolerance.TryCreate(value: x, obj: out _));

    [Fact]
    public void RejectsNegativeAndNonFinite() =>
        Spec.Cases(items: [-1f, -0.001f, float.NaN, float.PositiveInfinity, float.NegativeInfinity], key: static x => x,
            law: static x => Assert.False(condition: PickTolerance.TryCreate(value: x, obj: out _)));
}

public sealed class WireOverlayStyleLaws {
    [Fact]
    public void EntrySpecificStyleSelectorOverridesFallback() {
        Guid selectedId = Guid.NewGuid();
        WireDrawnEntry selected = new(Pair: new WireEnds(source: selectedId, target: Guid.NewGuid()), Kind: default, Bounds: RectangleF.Empty, SourceBounds: RectangleF.Empty, TargetBounds: RectangleF.Empty, Fade: 1f, Route: default);
        WireDrawnEntry normal = selected with { Pair = new WireEnds(source: Guid.NewGuid(), target: selected.TargetId) };
        PaintStyle fallback = PaintStyle.Style(edge: Colors.Transparent);
        PaintStyle highlight = PaintStyle.Style(edge: Colors.Red, thickness: 4f);
        WireOverlayStyle style = new(Style: fallback, Select: Some<Func<WireDrawnEntry, PaintStyle>>(entry => entry.SourceId == selectedId ? highlight : fallback));

        Assert.Equal(expected: highlight, actual: style.For(entry: selected));
        Assert.Equal(expected: fallback, actual: style.For(entry: normal));
    }

    [Fact]
    public void DrawnSnapshotProjectsDocumentModificationStamp() {
        WireDrawnSnapshot snapshot = new(
            Entries: Seq<WireDrawnEntry>(),
            Stamp: new WireDrawnStamp(DocumentId: Guid.NewGuid(), DocumentHash: Guid.NewGuid(), Modifications: 42, ProjectionCentre: PointF.Empty, ProjectionZoom: 1f, DrawInnerFrame: RectangleF.Empty),
            FreshFromWirePaint: true);
        Assert.Equal(expected: 42, actual: snapshot.DocumentModifications);
    }
}
