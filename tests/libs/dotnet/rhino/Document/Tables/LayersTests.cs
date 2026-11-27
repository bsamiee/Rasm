using System.Drawing;
using CsCheck;
using Rasm.Rhino.Document;
using Rasm.Rhino.Document.Tables;
using Rasm.Testing;
using Rhino;
using Rhino.DocObjects;
using Xunit;

namespace Rasm.Rhino.Tests.Document.Tables;

// --- [OPERATIONS] ----------------------------------------------------------------------

public sealed class LayersTests(Fixtures fixtures) {

    // --- [RECONCILE]

    [Fact]
    public Task ReconcileCreatesEachAncestorOnceBeforeItsChildrenThenConverges() =>
        Gen.OneOf("a", "A", "b", "c").Array[1, 3].Select(static names => string.Join(ModelComponent.NamePathSeparator, names)).Array[1, 6].SampleAsync(static async leaves => {
            Seq<string> paths = toSeq(leaves).Map(static leaf => $"R{ModelComponent.NamePathSeparator}{leaf}").Distinct(StringComparer.OrdinalIgnoreCase).AsIterable().ToSeq().Strict();
            Seq<string> ancestry = paths.Bind(static path => path.Split(ModelComponent.NamePathSeparator) switch {
                var names => toSeq(Enumerable.Range(1, names.Length)).Map(depth => string.Join(ModelComponent.NamePathSeparator, names.Take(depth))),
            }).Distinct(StringComparer.OrdinalIgnoreCase).AsIterable().ToSeq().Strict();
            Seq<RowSpec<Layer>> specs = paths.Map(static path => new RowSpec<Layer>(path, [])).Strict();
            Seq<(string Child, string Parent)> nested = ancestry.Filter(static path => path.Contains(ModelComponent.NamePathSeparator, StringComparison.Ordinal))
                .Map(static path => (Child: path, Parent: path[..path.LastIndexOf(ModelComponent.NamePathSeparator, StringComparison.Ordinal)])).Strict();
            (Seq<string> created, int again, Seq<(Guid Parent, Guid Above)> links, (bool Valid, string Log) audit) = await RhinoHost.RunAsync(RhinoHost.Headless(doc =>
                from first in Rows.Reconcile(doc, Layers.Kind, specs, Prune.Keep, new RedrawPolicy.Silent())
                from second in Rows.Reconcile(doc, Layers.Kind, specs, Prune.Keep, new RedrawPolicy.Silent())
                from linked in nested.TraverseM(edge => (Layers.Resolve(doc, new LayerRef.Path(edge.Child)), Layers.Resolve(doc, new LayerRef.Path(edge.Parent))).Apply(static (child, parent) => (child.ParentLayerId, parent.Id)).As()).As()
                select (first.Value.Choose(static step => step is RowPlan<Layer, RowSpec<Layer>>.Create create ? Some(create.Spec.Key) : None).Strict(), second.Value.Count, linked, RhinoHost.Audit(doc))));
            Assert.Equal(ancestry.Order(StringComparer.OrdinalIgnoreCase), created.Order(StringComparer.OrdinalIgnoreCase), StringComparer.OrdinalIgnoreCase);
            Assert.All(created.Map(static (path, at) => (path, at)), each =>
                Assert.All(created.Take(each.at), earlier => Assert.False(earlier.StartsWith(each.path + ModelComponent.NamePathSeparator, StringComparison.OrdinalIgnoreCase))));
            Assert.Equal(0, again);
            Assert.All(links, static link => Assert.Equal(link.Above, link.Parent));
            Assert.True(audit.Valid, audit.Log);
        });

    [Fact]
    public async Task ReconcileOfAMalformedLeafFailsAsUnqualifiedBeforeWritingAnyLayer() {
        RhinoDoc doc = await fixtures.TakeAsync(RhinoHost.Document);
        Fault<RhinoReason> fault = await Expect<Fault<RhinoReason>>.FailAsync(
            Rows.Reconcile(doc, Layers.Kind, Seq("Good", "").Map(static key => new RowSpec<Layer>(key, [])), Prune.Keep, new RedrawPolicy.Silent()).OnUiThread());
        Assert.Equal((RhinoReason.Unqualified, (object?)""), (fault.Reason, fault.Subject));
        Assert.Equal(RhinoReason.Missing, (await Expect<Fault<RhinoReason>>.FailAsync(Layers.Resolve(doc, new LayerRef.Path("Good")).OnUiThread())).Reason);
    }

    // --- [ADDRESSES]

    [Fact]
    public async Task UnknownLayerPathFailsAsMissing() {
        RhinoDoc doc = await fixtures.TakeAsync(RhinoHost.Document);
        Assert.Equal(RhinoReason.Missing, (await Expect<Fault<RhinoReason>>.FailAsync(Layers.Resolve(doc, new LayerRef.Path($"Absent{ModelComponent.NamePathSeparator}Layer")).OnUiThread())).Reason);
    }

    [Fact]
    public async Task LayerLookupByNameFailsAsUnsupported() {
        RhinoDoc doc = await fixtures.TakeAsync(RhinoHost.Document);
        Assert.Equal(RhinoReason.Unsupported, (await Expect<Fault<RhinoReason>>.FailAsync(Rows.Find(doc.Layers, new ComponentRef<Layer>.ByName("Default")).OnUiThread())).Reason);
    }

    // --- [EDITS]

    [Fact]
    public async Task ColorEditSetsColorAndKeepsNameAndParent() {
        (string name, Guid parent, Guid expected, Color color) = await RhinoHost.RunAsync(RhinoHost.Headless(static doc =>
            from placed in Rows.Reconcile(doc, Layers.Kind, Seq(new RowSpec<Layer>($"A{ModelComponent.NamePathSeparator}B", [])), Prune.Keep, new RedrawPolicy.Silent())
            from leaf in Layers.Resolve(doc, new LayerRef.Path($"A{ModelComponent.NamePathSeparator}B"))
            from changed in Rows.Edit(doc, Layers.Kind, new ComponentRef<Layer>.ById(leaf.Id), [Rows.Set<Layer, Color>(static row => row.Color, static (row, color) => row.Color = color, Color.Red)])
            from read in Layers.Resolve(doc, new LayerRef.Row(new ComponentRef<Layer>.ById(leaf.Id)))
            from above in Layers.Resolve(doc, new LayerRef.Path("A"))
            select (read.Name, read.ParentLayerId, above.Id, read.Color)));
        Assert.Equal(("B", Color.Red.ToArgb()), (name, color.ToArgb()));
        Assert.Equal(expected, parent);
    }

    // --- [MERGE]

    [Fact]
    public async Task MergeMovesObjectsAndChildrenToTheTargetAndDeletesTheSourceInOneRecord() {
        (Guid kept, Guid onLayer, Guid childParent, bool sourceGone, Seq<uint> records, Option<uint> record, (bool Valid, string Log) audit) = await RhinoHost.RunAsync(RhinoHost.Headless(static doc =>
            from placed in Rows.Reconcile(doc, Layers.Kind, Seq($"S{ModelComponent.NamePathSeparator}C", "K").Map(static key => new RowSpec<Layer>(key, [])), Prune.Keep, new RedrawPolicy.Silent())
            from source in Layers.Resolve(doc, new LayerRef.Path("S"))
            from target in Layers.Resolve(doc, new LayerRef.Path("K"))
            let point = doc.Objects.AddPoint(Point3d.Origin, new ObjectAttributes { LayerIndex = source.Index })
            let before = toSeq(doc.GetUndoRecords()).Map(static entry => entry.SerialNumber).Strict()
            from merged in Layers.Merge(doc, new LayerRef.Path("S"), new LayerRef.Path("K"), "Merge", new RedrawPolicy.Silent())
            from child in Layers.Resolve(doc, new LayerRef.Path($"K{ModelComponent.NamePathSeparator}C"))
            select (
                target.Id,
                doc.Layers[doc.Objects.FindId(point).Attributes.LayerIndex].Id,
                child.ParentLayerId,
                doc.Layers.FindByFullPath("S", RhinoMath.UnsetIntIndex) < 0,
                toSeq(doc.GetUndoRecords()).Map(static entry => entry.SerialNumber).Filter(serial => !before.Exists(held => held == serial)).Strict(),
                merged.Record,
                RhinoHost.Audit(doc))));
        Assert.Equal((kept, kept, true), (onLayer, childParent, sourceGone));
        Assert.Equal(record.ToSeq(), records);
        Assert.Single(records);
        Assert.True(audit.Valid, audit.Log);
    }

    [Fact]
    public async Task MergeOfTheCurrentLayerIsDeclinedAndMovesNothing() {
        RhinoDoc doc = await fixtures.TakeAsync(RhinoHost.Document);
        (Guid source, Guid point) = await RhinoHost.RunAsync((
            from placed in Rows.Reconcile(doc, Layers.Kind, Seq($"S{ModelComponent.NamePathSeparator}C", "K").Map(static key => new RowSpec<Layer>(key, [])), Prune.Keep, new RedrawPolicy.Silent())
            from held in Layers.Resolve(doc, new LayerRef.Path("S"))
            from current in Products.Accepted(() => doc.Layers.SetCurrentLayerIndex(held.Index, quiet: true))
            select (held.Id, doc.Objects.AddPoint(Point3d.Origin, new ObjectAttributes { LayerIndex = held.Index }))).OnUiThread());
        Assert.Equal(RhinoReason.Declined, (await Expect<Fault<RhinoReason>>.FailAsync(Layers.Merge(doc, new LayerRef.Path("S"), new LayerRef.Path("K"), "Merge", new RedrawPolicy.Silent()).OnUiThread())).Reason);
        (Guid onLayer, Guid childParent, int targetObjects, int targetChildren) = await RhinoHost.RunAsync((
            from child in Layers.Resolve(doc, new LayerRef.Path($"S{ModelComponent.NamePathSeparator}C"))
            from target in Layers.Resolve(doc, new LayerRef.Path("K"))
            select (doc.Layers[doc.Objects.FindId(point).Attributes.LayerIndex].Id, child.ParentLayerId, doc.Objects.FindByLayer(target).Length, target.GetChildren()?.Length ?? 0)).OnUiThread());
        Assert.Equal((source, source, 0, 0), (onLayer, childParent, targetObjects, targetChildren));
    }

    [Fact]
    public async Task MergeIntoItsOwnChildFailsAsInvalid() {
        RhinoDoc doc = await fixtures.TakeAsync(RhinoHost.Document);
        Assert.Equal(RhinoReason.Invalid, (await Expect<Fault<RhinoReason>>.FailAsync((
            Rows.Reconcile(doc, Layers.Kind, Seq(new RowSpec<Layer>($"S{ModelComponent.NamePathSeparator}C", [])), Prune.Keep, new RedrawPolicy.Silent())
            >> Layers.Merge(doc, new LayerRef.Path("S"), new LayerRef.Path($"S{ModelComponent.NamePathSeparator}C"), "Merge", new RedrawPolicy.Silent())).OnUiThread())).Reason);
    }
}
