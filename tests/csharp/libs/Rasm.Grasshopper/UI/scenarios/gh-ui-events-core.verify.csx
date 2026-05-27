using System;
using LanguageExt;
using Rasm.Grasshopper.UI;
using Rasm.TestKit.Scenarios;
using Rhino;
using static LanguageExt.Prelude;

Scenario.Run("gh-ui-events-core", CAPTURE_PATH, (key, facts) => {
    GrasshopperUi ui = new();
    facts.Add("rhino.mainThread", RhinoApp.IsOnMainThread);

    Probe.Expect(result: ui.Use(intent: GhUi.Editor(op: EditorOp.EnsureVisible)), label: "ensure visible");

    CanvasSnapshot canvas = Probe.ExpectCase(
        result: ui.Use(intent: GhUi.Canvas(op: CanvasOp.Snapshot())),
        label: "canvas snapshot",
        select: static value => value switch {
            CanvasResult.SnapshotResult snap => snap.Snapshot,
            _ => Option<CanvasSnapshot>.None,
        });

    facts.Add("canvas.logicalPixelSize", canvas.LogicalPixelSize);
    facts.Add("canvas.pointsPerPixel", canvas.PointsPerPixel);
    Probe.Require(
        condition: canvas.LogicalPixelSize > 0f && canvas.PointsPerPixel > 0f,
        message: "canvas DPI scale facts");

    int modifierTicks = 0;
    Subscription modifierSub = Probe.ExpectCase(
        result: ui.Use(intent: GhUi.Event(uiEvent: UiEvent.KeyboardModifiers(handler: _ => {
            modifierTicks++;
            return Fin.Succ(unit);
        }))),
        label: "keyboard modifiers",
        select: static value => value switch {
            Subscription sub => sub,
            _ => Option<Subscription>.None,
        });
    using (modifierSub) { }
    facts.Add("modifiers.initialTick", modifierTicks >= 1);
    Probe.Require(condition: modifierTicks >= 1, message: "keyboard modifiers initial tick");

    int documentTicks = 0;
    int lastModifications = -1;
    Subscription documentSub = Probe.ExpectCase(
        result: ui.Use(intent: GhUi.Event(uiEvent: UiEvent.DocumentChanged(handler: doc => {
            documentTicks++;
            lastModifications = doc.Modifications;
            return Fin.Succ(unit);
        }))),
        label: "document changed",
        select: static value => value switch {
            Subscription sub => sub,
            _ => Option<Subscription>.None,
        });
    using (documentSub) { }
    facts.Add("document.subscribeOk", true);
    facts.Add("document.ticks", documentTicks);
    facts.Add("document.lastModifications", lastModifications);

    int solutionTicks = 0;
    Subscription solutionSub = Probe.ExpectCase(
        result: ui.Use(intent: GhUi.Event(uiEvent: UiEvent.Solution(
            kind: SolutionEventKind.Started,
            handler: snap => {
                solutionTicks++;
                facts.Add("solution.documentModifications", snap.Document.Modifications);
                return Fin.Succ(unit);
            }))),
        label: "solution started",
        select: static value => value switch {
            Subscription sub => sub,
            _ => Option<Subscription>.None,
        });
    Probe.Expect(result: ui.Use(intent: GhUi.Canvas(op: CanvasOp.Invalidate(repaint: RepaintRequest.Scheduled))), label: "wake solution");
    using (solutionSub) { }
    facts.Add("solution.subscribeOk", true);
    facts.Add("solution.ticks", solutionTicks);

    int undoTicks = 0;
    int undoHistoryCount = -1;
    Subscription undoSub = Probe.ExpectCase(
        result: ui.Use(intent: GhUi.Event(uiEvent: UiEvent.Undo(
            kind: UndoEventKind.Modified,
            handler: snap => {
                undoTicks++;
                undoHistoryCount = snap.History.UndoCount;
                return Fin.Succ(unit);
            }))),
        label: "undo modified",
        select: static value => value switch {
            Subscription sub => sub,
            _ => Option<Subscription>.None,
        });
    using (undoSub) { }
    facts.Add("undo.subscribeOk", true);
    facts.Add("undo.ticks", undoTicks);
    facts.Add("undo.historyCount", undoHistoryCount);
});
