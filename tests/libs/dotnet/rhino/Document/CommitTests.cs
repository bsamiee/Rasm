using Rasm.Rhino.Document;
using Rasm.Rhino.Document.Tables;
using Rasm.Testing;
using Rhino;
using Rhino.DocObjects;
using Xunit;

namespace Rasm.Rhino.Tests.Document;

// --- [OPERATIONS] ----------------------------------------------------------------------

public sealed class CommitTests(Fixtures fixtures) {

    // --- [RECORDS]

    [Fact]
    public async Task CommitGroupsItsWritesInOneNamedUndoRecord() {
        (Option<uint> record, Seq<(uint Serial, string Description)> undo) = await RhinoHost.RunAsync(RhinoHost.Headless(static doc =>
            from committed in Commits.Commit(doc, "Add layers", new RedrawPolicy.Silent(), Rows.Add(doc, Layers.Kind, Some("A"), []) >> Rows.Add(doc, Layers.Kind, Some("B"), []))
            select (committed.Record, toSeq(doc.GetUndoRecords()).Map(static entry => (entry.SerialNumber, entry.Description)).Strict())));
        uint serial = Assert.Single(record.ToSeq());
        Assert.Equal((serial, "Add layers"), Assert.Single(undo));
    }

    [Fact]
    public async Task UndoingTheRecordRemovesEveryWriteOfTheCommit() =>
        Assert.Equal((true, 0), await RhinoHost.RunAsync(RhinoHost.Headless(static doc =>
            from committed in Commits.Commit(doc, "Add layers", new RedrawPolicy.Silent(), Rows.Add(doc, Layers.Kind, Some("A"), []) >> Rows.Add(doc, Layers.Kind, Some("B"), []))
            from reverted in IO.lift(doc.Undo)
            select (reverted, doc.Layers.Count(static layer => !layer.IsDeleted && layer.Name is "A" or "B")))));

    [Fact]
    public async Task CommitWithAnEmptyBodyLeavesNoRecord() =>
        Assert.Equal((Option<uint>.None, 0), await RhinoHost.RunAsync(RhinoHost.Headless(static doc =>
            from committed in Commits.Commit(doc, "Nothing", new RedrawPolicy.Silent(), IO.pure(unit))
            select (committed.Record, doc.GetUndoRecords().Length))));

    [Fact]
    public async Task FailedCommitUndoesItsWritesKeepsTheBodyErrorAndLeavesNoRecord() {
        RhinoDoc doc = await fixtures.TakeAsync(RhinoHost.Document);
        RhinoFault refused = new(nameof(FailedCommitUndoesItsWritesKeepsTheBodyErrorAndLeavesNoRecord), RhinoReason.Stated);
        Assert.Same(refused, await Expect<Error>.FailAsync(Commits.Commit(doc, "Add layer", new RedrawPolicy.Silent(), Rows.Add(doc, Layers.Kind, Some("A"), []) >> IO.fail<Layer>(refused)).OnUiThread()));
        Assert.Equal(RhinoReason.Missing, (await Expect<RhinoFault>.FailAsync(Layers.Resolve(doc, new LayerRef.Path("A")).OnUiThread())).Reason);
        Assert.Empty(await RhinoHost.RunAsync(IO.lift(() => toSeq(doc.GetUndoRecords()).Filter(static record => record.Description is "Add layer").Strict()).OnUiThread()));
    }

    [Fact]
    public async Task FailedCommitAfterACommittedOneKeepsTheEarlierRecordAndItsWrite() {
        RhinoDoc doc = await fixtures.TakeAsync(RhinoHost.Document);
        RhinoFault refused = new(nameof(FailedCommitAfterACommittedOneKeepsTheEarlierRecordAndItsWrite), RhinoReason.Stated);
        Option<uint> kept = (await RhinoHost.RunAsync(Commits.Commit(doc, "Add A", new RedrawPolicy.Silent(), Rows.Add(doc, Layers.Kind, Some("A"), [])).OnUiThread())).Record;
        Assert.Same(refused, await Expect<Error>.FailAsync(Commits.Commit(doc, "Refused", new RedrawPolicy.Silent(), IO.fail<Layer>(refused)).OnUiThread()));
        (Option<uint> newest, bool present) = await RhinoHost.RunAsync(IO.lift(() => (toSeq(doc.GetUndoRecords()).Head.Map(static entry => entry.SerialNumber), doc.Layers.FindByFullPath("A", RhinoMath.UnsetIntIndex) >= 0)).OnUiThread());
        Assert.Equal((kept, true), (newest, present));
        Assert.True(kept.IsSome);
    }

    [Fact]
    public async Task CommitInADocumentWithUndoRecordingOffFailsAsUnavailableBeforeWriting() {
        RhinoDoc doc = await fixtures.TakeAsync(RhinoHost.Document);
        RhinoFault fault = await Expect<RhinoFault>.FailAsync((IO.lift(() => { doc.UndoRecordingEnabled = false; })
            >> Commits.Commit(doc, "Add layer", new RedrawPolicy.Silent(), Rows.Add(doc, Layers.Kind, Some("A"), []))).OnUiThread());
        Assert.Equal(RhinoReason.Unavailable, fault.Reason);
        Assert.Equal(RhinoReason.Missing, (await Expect<RhinoFault>.FailAsync(Layers.Resolve(doc, new LayerRef.Path("A")).OnUiThread())).Reason);
    }

    // --- [REDRAW]

    [Fact]
    public async Task SuppressedRedrawIsOffDuringTheBodyAndRestoredAfterItSucceeds() {
        RhinoDoc doc = await fixtures.TakeAsync(RhinoHost.Document);
        (bool prior, bool held, bool restored) = await RhinoHost.RunAsync((
            from before in IO.lift(() => doc.Views.RedrawEnabled)
            from during in Commits.WithinRedraw(doc, new RedrawPolicy.Suppressed(Deferred: false, RepaintLayers: false), IO.lift(() => doc.Views.RedrawEnabled))
            select (before, during, doc.Views.RedrawEnabled)).OnUiThread());
        Assert.Equal((false, prior), (held, restored));
    }

    [Fact]
    public async Task SuppressedRedrawIsRestoredAfterTheBodyFails() {
        RhinoDoc doc = await fixtures.TakeAsync(RhinoHost.Document);
        RhinoFault refused = new(nameof(SuppressedRedrawIsRestoredAfterTheBodyFails), RhinoReason.Stated);
        bool prior = await RhinoHost.RunAsync(IO.lift(() => doc.Views.RedrawEnabled).OnUiThread());
        Assert.Same(refused, await Expect<RhinoFault>.FailAsync(Commits.WithinRedraw(doc, new RedrawPolicy.Suppressed(Deferred: false, RepaintLayers: false), IO.fail<Unit>(refused)).OnUiThread()));
        Assert.Equal(prior, await RhinoHost.RunAsync(IO.lift(() => doc.Views.RedrawEnabled).OnUiThread()));
    }
}
