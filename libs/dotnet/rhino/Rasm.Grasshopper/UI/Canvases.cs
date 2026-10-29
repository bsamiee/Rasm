using System.Collections.Generic;
using Eto.Drawing;
using Eto.Forms;
using Grasshopper2;
using Grasshopper2.Data;
using Grasshopper2.Doc;
using Grasshopper2.Doc.Attributes;
using Grasshopper2.Eto.Drawing;
using Grasshopper2.Extensions;
using Grasshopper2.Parameters;
using Grasshopper2.Parsing;
using Grasshopper2.Types.Assistant;
using Grasshopper2.UI;
using Grasshopper2.UI.Canvas;
using Grasshopper2.UI.Flex;
using Grasshopper2.UI.Skinning;
using Grasshopper2.Undo;

namespace Rasm.Grasshopper.UI;

// --- [OPERATIONS] ----------------------------------------------------------------------
public static class Canvases {
    // --- [POLICIES]
    public static IO<Unit> Restrict(Canvas canvas, Action<CanvasActions> restrict) =>
        from lease in use(IO.lift(() => Leased(canvas, restrict)), lease => IO.lift(() => Released(canvas, lease)))
        from over in use(IO.lift(static () => Atom(Option<IResponsive>.None)), static over => IO.lift(() => ignore(Hovered(over, None))))
        from hovering in Hooks.On<MouseEventArgs>(h => canvas.MouseMove += h, h => canvas.MouseMove -= h, args => IO.lift(() =>
            Hovered(over, args.Buttons is MouseButtons.None && ReferenceEquals(canvas.FocusObject, lease.Guard)
                    ? toSeq(canvas.ResponsivesForwards).Rev().Find(responsive => responsive.Responder.IsCoincident(args.Location, canvas.Map(args.Location, CoordinateSystem.Control, CoordinateSystem.Content)))
                    : None)
                .Iter(target => target.Responder.MouseOver(new ResponseMouseArgs(canvas, args)))))
        from menu in Optional(Editor.Instance).Filter(editor => ReferenceEquals(editor.Canvas, canvas)).Match(
            Some: editor => Disabled((EditorMainMenu)editor.Menu, lease.Next),
            None: static () => unitIO)
        select unit;

    public static IO<Unit> Wiring(Canvas canvas, Func<IParameter, IParameter, bool> make, Func<IParameter, IParameter, bool> protect) =>
        Restrict(canvas, actions => {
            Option<Func<(IParameter source, IParameter target), bool>> allowed = Optional(actions.MakeWireFilter);
            Option<Func<(IParameter source, IParameter target), bool>> guarded = Optional(actions.DeleteWireFilter);
            actions.MakeWireFilter = wire => allowed.ForAll(prior => prior(wire)) && Hooks.Run(IO.lift(() => make(wire.source, wire.target)), failed: false);
            actions.DeleteWireFilter = wire => guarded.Exists(prior => prior(wire)) || Hooks.Run(IO.lift(() => protect(wire.source, wire.target)), failed: true);
            actions.AllowModifyWire = false;
            actions.AllowWireMenu = false;
        });

    private static (CanvasActions Prior, CanvasActions Next, (bool Objects, bool Wires, bool Groups, bool Dragging) Was, (bool Objects, bool Wires, bool Groups, bool Dragging) Set, Option<Document> Document, Interaction Guard) Leased(
        Canvas canvas, Action<CanvasActions> restrict) {
        CanvasActions prior = Merged(new CanvasActions(), canvas.AllowedActions, None);
        CanvasActions next = Merged(new CanvasActions(), prior, None);
        restrict(next);
        (bool Objects, bool Wires, bool Groups, bool Dragging) was = (canvas.WindowSelectObjects, canvas.WindowSelectWires, canvas.WindowSelectGroups, canvas.ViewportDragging);
        (bool Objects, bool Wires, bool Groups, bool Dragging) set =
            (was.Objects && next.AllowObjectSelect, was.Wires && next.AllowObjectSelect && next.AlloWireSelect, was.Groups && next.AllowObjectSelect, was.Dragging && next.AllowDrag);
        _ = Merged(canvas.AllowedActions, next, None);
        (canvas.WindowSelectObjects, canvas.WindowSelectWires, canvas.WindowSelectGroups, canvas.ViewportDragging) = set;
        Interaction guard = Guarded(canvas);
        canvas.PushFocus(guard);
        return (prior, next, was, set, Optional(canvas.Document), guard);
    }

    private static Unit Released(
        Canvas canvas, (CanvasActions Prior, CanvasActions Next, (bool Objects, bool Wires, bool Groups, bool Dragging) Was, (bool Objects, bool Wires, bool Groups, bool Dragging) Set, Option<Document> Document, Interaction Guard) lease) {
        canvas.PopFocus(lease.Guard);
        _ = Merged(canvas.AllowedActions, lease.Prior, Some(lease.Next));
        (canvas.WindowSelectObjects, canvas.WindowSelectWires, canvas.WindowSelectGroups) = (
            Kept(canvas.WindowSelectObjects, lease.Set.Objects, lease.Was.Objects),
            Kept(canvas.WindowSelectWires, lease.Set.Wires, lease.Was.Wires),
            Kept(canvas.WindowSelectGroups, lease.Set.Groups, lease.Was.Groups));
        if (lease.Document.Exists(document => ReferenceEquals(document, canvas.Document))) canvas.ViewportDragging = Kept(canvas.ViewportDragging, lease.Set.Dragging, lease.Was.Dragging);
        return unit;
    }

    private static CanvasActions Merged(CanvasActions live, CanvasActions source, Option<CanvasActions> installed) {
        (live.AllowDrag, live.AlloWireSelect, live.AllowObjectSelect, live.AllowMakeWire, live.AllowDeleteWire, live.AllowModifyWire) = (
            Pick(static actions => actions.AllowDrag), Pick(static actions => actions.AlloWireSelect), Pick(static actions => actions.AllowObjectSelect),
            Pick(static actions => actions.AllowMakeWire), Pick(static actions => actions.AllowDeleteWire), Pick(static actions => actions.AllowModifyWire));
        (live.MakeWireFilter, live.DeleteWireFilter) = (Pick(static actions => actions.MakeWireFilter), Pick(static actions => actions.DeleteWireFilter));
        (live.AllowMakeObject, live.AllowDeleteObject, live.AllowObjectResponse, live.AllowDropFile) = (
            Pick(static actions => actions.AllowMakeObject), Pick(static actions => actions.AllowDeleteObject), Pick(static actions => actions.AllowObjectResponse), Pick(static actions => actions.AllowDropFile));
        (live.AllowWireMenu, live.AllowObjectMenu, live.AllowCanvasMenu) = (
            Pick(static actions => actions.AllowWireMenu), Pick(static actions => actions.AllowObjectMenu), Pick(static actions => actions.AllowCanvasMenu));
        return live;

        T Pick<T>(Func<CanvasActions, T> field) => installed.Match(Some: held => Kept(field(live), field(held), field(source)), None: () => field(source));
    }

    private static T Kept<T>(T live, T installed, T prior) => EqualityComparer<T>.Default.Equals(live, installed) ? prior : live;

    private static Option<IResponsive> Hovered(Atom<Option<IResponsive>> over, Option<IResponsive> next) {
        Option<IResponsive> prior = over.Value;
        _ = over.Swap(_ => next);
        _ = prior.Filter(held => !next.Exists(target => ReferenceEquals(target, held))).Iter(static held => held.Responder.MouseLeave());
        return next;
    }

    private static IO<Unit> Disabled(EditorMainMenu menu, CanvasActions next) =>
        Seq<(MenuItem Item, bool Allowed)>(
                (menu.EditCut, next.AllowDeleteObject),
                (menu.EditPaste, next.AllowMakeObject),
                (menu.EditPasteExotic, next.AllowMakeObject),
                (menu.EditDelete, next.AllowDeleteObject && next.AllowDeleteWire && next.DeleteWireFilter is null),
                (menu.EditDeleteData, next.AllowObjectResponse),
                (menu.EditSelectAll, next.AllowObjectSelect),
                (menu.EditGroup, next.AllowMakeObject),
                (menu.EditChain, next.AllowMakeObject && next.AllowDeleteObject),
                (menu.EditCluster, next.AllowMakeObject && next.AllowDeleteObject))
            .Filter(static gate => !gate.Allowed)
            .Map(static gate => gate.Item)
            .Strict() switch {
                var barred =>
                    from shortcuts in use(
                        IO.lift(() => barred.Map(static item => (Item: item, item.Shortcut)).Strict()),
                        static held => IO.lift(() => held.Iter(static entry => entry.Item.Shortcut = Kept(entry.Item.Shortcut, Keys.None, entry.Shortcut))))
                    from cleared in IO.lift(() => barred.Iter(static item => item.Shortcut = Keys.None))
                    from opening in Hooks.On<EventArgs>(h => menu.Edit.Opening += h, h => menu.Edit.Opening -= h, _ => IO.lift(() => barred.Iter(static item => item.Enabled = false)))
                    select unit,
            };

    private static Interaction Guarded(Canvas canvas) {
        Interaction guard = new();
        guard.KeyDownHook += e => Hooks.Run(IO.lift(() => e is { IsChar: true, Key: Keys.Delete or Keys.Backspace } ? Gated(canvas) : Response.Ignored), Response.Ignored);
        guard.MouseDownHook += e => e.Buttons is MouseButtons.Primary
            && ((canvas.CursorMode is CursorMode.EditActive && !canvas.AllowedActions.AllowObjectResponse)
                || (!canvas.AllowedActions.AlloWireSelect && canvas.ResolvePick(e.ContentLocation, includeGrips: false, includeForeground: true, includeBackground: true, includeWires: true, recursive: true).Kind is Pick.Wire))
            ? Response.Handled
            : Response.Ignored;
        return guard;
    }

    private static Response Gated(Canvas canvas) {
        CanvasActions actions = canvas.AllowedActions;
        _ = Optional(canvas.Document).Filter(static document => !toSeq(document.Objects.Forwards).Exists(static item => item.Attributes is IResponsive { Responder.HasFocus: true })).Iter(document => {
            if (!actions.AllowDeleteWire) document.Objects.DeselectAllWires();
            _ = Optional(actions.DeleteWireFilter).Filter(_ => actions.AllowDeleteWire).Iter(protect => toSeq(document.Objects.SelectedWires)
                .Filter(wire => (from source in Optional(document.Objects.FindParameter(wire.Source))
                                 from target in Optional(document.Objects.FindParameter(wire.Target))
                                 select (source, target)).Exists(protect))
                .Iter(wire => document.Objects.DeselectWire(wire)));
            if (!actions.AllowDeleteObject) _ = document.Methods.DeselectAll();
        });
        return Response.Ignored;
    }

    // --- [EDITING]
    public static IO<Unit> Inline<T>(Canvas canvas, RectangleF frame, IDocumentObject owner, T current, Func<T, IO<Unit>> assign, Func<Grasshopper2.Undo.Action> snapshot) =>
        from assistant in IO.lift(static () => Optional(TypeAssistantServer.Find<T>()).ToFin(new GrasshopperFault(typeof(T).Name, GrasshopperReason.Missing)))
        from undo in IO.lift(snapshot)
        let recorded = memo(() => Optional(owner.Document).Bind(document => {
            Option<Node> before = Optional(document.Undo.FirstUndo);
            owner.AddUndoRecord(new VerbNoun("Edit", owner.Nomen.Name), undo);
            return Optional(document.Undo.FirstUndo).Filter(after => !before.Exists(prior => ReferenceEquals(prior, after))).Map(after => (Document: document, Node: after));
        }))
        from last in IO.lift(() => Atom((Value: current, Changed: false, Recorded: false)))
        from shown in IO.lift(() => canvas.ShowInlineEditor(frame, assistant.Format(current),
            text => Hooks.Run(
                (from parsed in IO.lift(() => assistant.Parse(null, text, Garden.Pear(current)))
                 from changed in IO.lift(() => parsed.Success && last.Swap(prior => (parsed.Value, !assistant.Same(parsed.Value, prior.Value), prior.Recorded || !assistant.Same(parsed.Value, prior.Value))).Changed)
                 from applied in when(changed, assign(parsed.Value) >> IO.lift(() => ignore(recorded.Value))).As()
                 select (IResult)parsed).Catch(error => IO.pure<IResult>(Result<T>.Fail(error.Message, text))).As(),
                Result<T>.Fail("", text)),
            () => Hooks.Run(
                when(!assistant.Same(last.Value.Value, current), assign(current)).As()
                >> when(last.Value.Recorded, IO.lift(() => recorded.Value.Iter(static held => held.Document.Undo.Remove(held.Node)))).As(),
                unit)))
        select unit;

    // --- [PAINTING]
    public static IO<Unit> Overlay(Canvas canvas, Func<IDocumentObject, Option<Action<Context, Skin>>> mark) =>
        Hooks.On<CanvasPaintEventArgs>(h => canvas.AfterPaintObjects += h, h => canvas.AfterPaintObjects -= h, args =>
            (from state in use(args.Graphics.ProjectionState)
             from drawn in IO.lift(() => Optional(args.Canvas.Document).Iter(document => {
                 Context context = args.Graphics.ContentContext;
                 _ = toSeq(document.Objects.Forwards)
                     .Filter(item => item.Attributes.Intersects(args.Canvas.VisibleFrame))
                     .Iter(item => mark(item).Iter(draw => draw(context, args.Skin)));
             }))
             select unit).Bracket());

    // --- [VIEWING]
    public static IO<Unit> Frame(Canvas canvas, Seq<Guid> objects) =>
        from bounds in IO.lift(() => Bounds(canvas, objects))
        from framed in IO.lift(() => canvas.Navigate(bounds.AdjustSides(20f), (canvas.MinimumZoom, Math.Max(1f, canvas.Projection.Zoom)), Grasshopper2.UI.Animation.Duration.Normal))
        select unit;

    public static IO<Bitmap> Capture(Canvas canvas, Option<Seq<Guid>> objects, Size size, bool background = true, bool wires = true, bool messages = true) =>
        from region in IO.lift(() => objects.Match(Some: ids => Bounds(canvas, ids), None: static () => (Fin<RectangleF>)RectangleF.Empty))
        from bitmap in IO.lift(() => Optional(canvas.DrawToBitmap(region, size.Width, size.Height, background, wires, messages))
            .ToFin(new GrasshopperFault(objects.ToString(), GrasshopperReason.Unfinished)))
        select bitmap;

    private static Fin<RectangleF> Bounds(Canvas canvas, Seq<Guid> objects) =>
        Optional(canvas.Document)
            .Map(document => objects.Choose(id => Optional(document.Objects.Find(id))).Map(static item => item.Attributes.AggregateBounds))
            .Filter(static boxes => !boxes.IsEmpty)
            .Map(static boxes => boxes.BoundingBox())
            .ToFin(new GrasshopperFault(objects.ToString(), GrasshopperReason.Missing));

    // --- [TRACKING]
    public static IO<Unit> Track(FlexControl control, RectangleF bounds, SizeF minimum, SizeF maximum, Func<RectangleF, IO<Unit>> placed) =>
        use(IO.lift(() => Tracked(control, bounds, minimum, maximum, placed)), region => IO.lift(() => {
            control.PopFocus(region);
            control.UnregisterIResponsive(region);
            control.Cursor = Cursors.Default;
            _ = Cleared(control);
        })).Map(unit);

    private static Interaction Tracked(FlexControl control, RectangleF bounds, SizeF minimum, SizeF maximum, Func<RectangleF, IO<Unit>> placed) {
        Interaction region = new() { RegionBoundary = bounds };
        Padding edges = new(
            minimum.Width.Equals(maximum.Width) ? 0 : ResizableAttributes<IDocumentObject>.EdgeSize,
            minimum.Height.Equals(maximum.Height) ? 0 : ResizableAttributes<IDocumentObject>.EdgeSize);
        Atom<Option<(RectangleF Origin, Func<PointF, RectangleF> Step)>> track = Atom(Option<(RectangleF Origin, Func<PointF, RectangleF> Step)>.None);
        region.MouseOverHook += e => control.Cursor = Optional(new ResizingFrame(region.RegionBoundary, minimum, maximum).CursorAt(e.ContentLocation, edges)).IfNone(Cursors.Default);
        region.MouseLeaveHook += () => control.Cursor = Cursors.Default;
        region.MouseDownHook += e => e.Buttons is MouseButtons.Primary ? Began(e.ContentLocation) : Response.Ignored;
        region.MouseDragHook += e => track.Value.Match(Some: held => Shown(held.Step(e.ContentLocation), Response.Handled), None: static () => Response.Ignored);
        region.MouseUpHook += _ => track.Value.Match(Some: _ => Ended(Response.Release), None: static () => Response.Ignored);
        region.KeyDownHook += e => track.Value.Match(
            Some: held => (e.Key, e.Modifiers) switch {
                (Keys.Escape, _) => Shown(held.Origin, Ended(Response.Release)),
                (Keys.S, Keys.None) => Toggled(),
                _ => Response.Handled,
            },
            None: static () => Response.Ignored);
        region.LostFocus += (_, _) => track.Value.Iter(held => Shown(held.Origin, Ended(Response.Release)));
        control.RegisterIResponsive(region);
        return region;

        Response Began(PointF point) {
            RectangleF start = region.RegionBoundary;
            Option<(Canvas Canvas, SnappingConstraints Constraints)> snapping = Optional(control as Canvas)
                .Filter(static canvas => canvas.Document is not null)
                .Map(static canvas => (canvas, SnappingConstraints.CreateFromDocument(canvas.Document)));
            ResizingFrame frame = new(start, minimum, maximum, snapping.Filter(static held => ReferenceEquals(held.Canvas, Editor.Instance?.Canvas)).Map(static held => held.Constraints).ValueUnsafe(), SnappingSettings.Current);
            Option<Func<PointF, RectangleF>> step = frame.Begin(point, edges)
                ? Some<Func<PointF, RectangleF>>(location => {
                    frame.Continue(location);
                    return frame.Resized;
                })
                : start.Contains(point) ? Some<Func<PointF, RectangleF>>(location => Moved(snapping, start, point.SizeTo(location), FlexControl.ShiftKeyDown)) : None;
            _ = track.Swap(_ => step.Map(next => (start, next)));
            return step.IsSome ? Response.Capture : Response.Ignored;
        }

        Response Shown(RectangleF next, Response response) {
            region.RegionBoundary = next;
            _ = Hooks.Run(placed(next), unit);
            region.OnRedrawRequired();
            return response;
        }

        Response Ended(Response response) {
            _ = track.Swap(static _ => None);
            _ = Cleared(control);
            return response;
        }

        Response Toggled() {
            Settings.CanvasSnapToObjects.Value = !Settings.CanvasSnapToObjects.Value;
            _ = Cleared(control);
            return Response.Handled;
        }
    }

    private static RectangleF Moved(Option<(Canvas Canvas, SnappingConstraints Constraints)> snapping, RectangleF start, SizeF offset, bool orthogonal) =>
        snapping.Match(
            Some: held => {
                SnappingAction? edgeX = null;
                SnappingAction? edgeY = null;
                if (Settings.CanvasSnapToObjects.Value) held.Constraints.SnapRectangle(start.Move(offset.Width, offset.Height), SnappingSettings.Current, held.Canvas.VisibleFrame, out edgeX, out edgeY);
                bool wide = Math.Abs(offset.Width) > Math.Abs(offset.Height);
                SnappingAction? x = orthogonal && !wide ? SnappingAction.VerticalOrthoAction(start.Center.Round(), held.Canvas.VisibleFrame) : edgeX;
                SnappingAction? y = orthogonal && wide ? SnappingAction.HorizontalOrthoAction(start.Center.Round(), held.Canvas.VisibleFrame) : edgeY;
                (held.Canvas.SnapXAction, held.Canvas.SnapYAction) = (x, y);
                return start.Move((orthogonal && !wide ? 0f : offset.Width) + (x?.ΔX ?? 0f), (orthogonal && wide ? 0f : offset.Height) + (y?.ΔY ?? 0f));
            },
            None: () => start.Move(offset.Width, offset.Height));

    private static Unit Cleared(FlexControl control) =>
        Optional(control as Canvas).Iter(static canvas => (canvas.SnapXAction, canvas.SnapYAction) = (null, null));

    // --- [INTERACTIONS]
    private sealed class Interaction : Responses, IResponsive {
        public Responses Responder => this;
    }
}
