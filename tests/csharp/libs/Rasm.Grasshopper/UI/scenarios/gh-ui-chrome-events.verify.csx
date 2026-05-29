using System;
using Rasm.Grasshopper.UI;
using Rhino;

// Consolidates gh-ui-events-core + gh-ui-subscription-marshal and adds Section-C chrome/input surfaces:
// CursorKind Size* resolution, InputClipboardOp byte round-trip, ToolbarItem panel projection,
// Bar.AddColours spectrum, FloatingButton dispatch, and SolutionEventKind.EnabledChanged subscribe/teardown.
Scenario.Run("gh-ui-chrome-events", CAPTURE_PATH, (key, facts) => {
    GrasshopperUi ui = new();
    facts.Add("rhino.mainThread", RhinoApp.IsOnMainThread);
    Probe.Expect(result: ui.Use(intent: GhUi.Editor(op: EditorOp.EnsureVisible)), label: "ensure grasshopper editor");

    // Subscription marshal/composite algebra (pure, deterministic).
    int detached = 0;
    Subscription.Atom(detach: () => detached++, marshalToUi: true).Dispose();
    Probe.Require(condition: detached == 1, message: $"marshal detach count={detached}");
    int composite = 0;
    (Subscription.Atom(detach: () => composite += 1, marshalToUi: true) | Subscription.Atom(detach: () => composite += 10, marshalToUi: true)).Dispose();
    Probe.Require(condition: composite == 11, message: $"composite detach sum={composite}");
    facts.Add("marshal.detached", detached);
    facts.Add("composite.sum", composite);

    // CursorKind Size* resolution: the 8 directional resize cursors resolve + assign to the canvas.
    foreach (CursorKind cursor in new[] {
        CursorKind.SizeLeft, CursorKind.SizeRight, CursorKind.SizeTop, CursorKind.SizeBottom,
        CursorKind.SizeTopLeft, CursorKind.SizeTopRight, CursorKind.SizeBottomLeft, CursorKind.SizeBottomRight,
    }) {
        CursorKind resolved = Probe.Expect(
            result: ui.Use(intent: GhUi.Input(request: new InputRequest<CursorKind>.Cursor(Kind: cursor))),
            label: $"cursor {cursor}");
        facts.Add($"cursor.{cursor}.key", resolved.Key);
    }
    Probe.Expect(result: ui.Use(intent: GhUi.Input(request: new InputRequest<CursorKind>.Cursor(Kind: CursorKind.Default))), label: "cursor reset");

    // InputClipboardOp byte round-trip through Clipboard.SetData/GetData(byte[], type).
    byte[] payload = new byte[] { 1, 2, 3, 4, 5 };
    const string clipType = "com.rasm.test.bytes";
    Probe.Expect(
        result: ui.Use(intent: GhUi.Input(request: new InputRequest<InputClipboardSnapshot>.Clipboard(Op: InputClipboardOp.WriteData(data: payload, type: clipType)))),
        label: "clipboard write data");
    InputClipboardSnapshot readBack = Probe.Expect(
        result: ui.Use(intent: GhUi.Input(request: new InputRequest<InputClipboardSnapshot>.Clipboard(Op: InputClipboardOp.ReadData(type: clipType)))),
        label: "clipboard read data");
    byte[] roundTrip = readBack.Data.IfNone(new byte[0]);
    facts.Add("clipboard.roundTripBytes", roundTrip.Length);
    Probe.Require(condition: roundTrip.Length == payload.Length && roundTrip[0] == payload[0] && roundTrip[4] == payload[4], message: "clipboard byte round-trip");

    // ToolbarItem Label/Check/Text projected onto an InputPanel via the InputPanel command surface.
    CommandPlan panelPlan = new(Items: Seq(
        ToolbarItem.Label("Rasm Label"),
        ToolbarItem.Check(name: "Rasm Check", state: true, changed: static _ => Fin.Succ(unit)),
        ToolbarItem.Text(name: "Rasm Text", value: "value", changed: static _ => Fin.Succ(unit))));
    InputPanelSnapshot panel = Probe.Expect(
        result: ui.Use(intent: GhUi.Input(request: new InputRequest<InputPanelSnapshot>.CommandPanel(Plan: panelPlan))),
        label: "command panel projection");
    facts.Add("panel.count", panel.Count);
    Probe.Require(condition: panel.Count >= 3, message: $"panel native widget count={panel.Count}");

    // Bar.AddColours spectrum (ToolbarItem.Spectrum) is owned by the static gate: its OpenColor.Family
    // payload is a Grasshopper2 type the host-filtered csx cannot name, so it is not driven here.

    // FloatingButton dispatch: status snapshot + FindByName(absent) -> None (InfoOf now projects NumericValue;
    // exercising a Some NumericValue needs a live numeric button, which is IIcon-dependent and out of scope).
    FloatingButtonSnapshot fb = Probe.Expect(
        result: ui.Use(intent: GhUi.FloatingButton(op: new FloatingButtonOp.StatusCase())),
        label: "floating button status") switch {
        CanvasChromeResult.FloatingButtonStatusCase value => value.Snapshot,
        CanvasChromeResult other => throw new InvalidOperationException(message: $"unexpected fb status: {other.GetType().Name}"),
    };
    facts.Add("floatingButton.count", fb.Count);
    Option<FloatingButtonInfo> absent = Probe.Expect(
        result: ui.Use(intent: GhUi.FloatingButton(op: new FloatingButtonOp.FindByNameCase(Name: "rasm-absent-button"))),
        label: "floating button find absent") switch {
        CanvasChromeResult.FloatingButtonFoundCase value => value.Info,
        CanvasChromeResult other => throw new InvalidOperationException(message: $"unexpected fb found: {other.GetType().Name}"),
    };
    facts.Add("floatingButton.absentFound", absent.IsSome);
    Probe.Require(condition: absent.IsNone, message: "absent floating button find returns none");

    // SolutionEventKind.EnabledChanged: dedicated static-EventHandler leg attaches + detaches cleanly.
    int enabledTicks = 0;
    Subscription enabledSub = Probe.ExpectCase(
        result: ui.Use(intent: GhUi.Event(uiEvent: UiEvent.Solution(kind: SolutionEventKind.EnabledChanged, handler: snap => { enabledTicks++; return Fin.Succ(unit); }))),
        label: "solution enabled-changed subscribe",
        select: static value => value switch { Subscription sub => sub, _ => Option<Subscription>.None });
    using (enabledSub) { }
    facts.Add("solution.enabledChanged.subscribeOk", true);
    facts.Add("solution.enabledChanged.ticks", enabledTicks);

    // Document subscription regression (events-core).
    Subscription docSub = Probe.ExpectCase(
        result: ui.Use(intent: GhUi.Event(uiEvent: UiEvent.DocumentChanged(handler: static _ => Fin.Succ(unit)))),
        label: "document changed subscribe",
        select: static value => value switch { Subscription sub => sub, _ => Option<Subscription>.None });
    using (docSub) { }
    facts.Add("document.subscribeOk", true);
});
