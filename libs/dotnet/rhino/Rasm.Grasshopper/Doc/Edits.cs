using Eto.Drawing;
using Grasshopper2.Components;
using Grasshopper2.Data;
using Grasshopper2.Diagnostics;
using Grasshopper2.Doc;
using Grasshopper2.Framework;
using Grasshopper2.Parameters;
using Grasshopper2.UI;
using Grasshopper2.UI.Canvas;
using Grasshopper2.UI.Skinning;
using Grasshopper2.Undo;
using Grasshopper2.Undo.Actions;
using Rasm.Rhino;

namespace Rasm.Grasshopper.Doc;

// --- [OPERATIONS] ----------------------------------------------------------------------

public static class DocumentEdits {

    // --- [HISTORY]

    public static IO<A> Commit<A>(Document document, VerbNoun name, Func<ActionList, IO<A>> body) =>
        from actions in IO.lift(static () => ActionList.Empty)
        from value in body(actions).Catch(error => Closed(document, actions, name, record => record.Undo(document)) >> IO.fail<A>(error)).As()
        from closed in Closed(document, actions, name, record => {
            document.Undo.Do(record);
            toSeq(actions.Actions.OfType<ObjectAction>())
                .Fold(HashMap<Guid, Effects>(), static (held, action) => held.AddOrUpdate(action.Id, prior => prior | action.Response, action.Response))
                .Iter((id, response) => Optional(document.Objects.Find(id)).Iter(target => {
                    if (response.HasFlag(Effects.ExpireSolution)) target.Expire();
                    if (response.HasFlag(Effects.ExpireLayout)) {
                        target.OnLayoutChanged();
                        target.Attributes?.InvalidateLayout();
                    }
                    if (response.HasFlag(Effects.ExpireDisplay)) target.OnDisplayChanged();
                    if (response is not Effects.ExpireLayout) target.SetModificationInfo();
                }));
        })
        select value;

    public static IO<Unit> Assign<T>(Parameter<T> parameter, Tree<T> data, VerbNoun name) =>
        Located(parameter).Bind(document => Commit(document, name, actions => IO.lift(() => {
            actions.Add(new PersistentDataAction(parameter, Guid.Empty));
            parameter.Set(data);
        })));

    public static IO<Unit> Restore(History history, Node node) =>
        IO.lift(() => {
            if (node.IsRoot || node.Record.State is Grasshopper2.Undo.State.Undo) history.Undo(node);
            else history.Redo(node);
        });

    public static IO<Seq<(Node Node, Grasshopper2.Undo.Action Action)>> Actions(History history) =>
        IO.lift(() => Seq((Nodes: history.CentralUndoSequence, Actions: history.UndoActions), (Nodes: history.CentralRedoSequence, Actions: history.RedoActions))
            .Bind(static side => toSeq(side.Nodes)
                .Bind(static node => LanguageExt.Seq.repeat(node, node.Record.Count))
                .Zip(toSeq(side.Actions), static (node, action) => (Node: node, Action: action)))
            .Strict());

    private static IO<Unit> Closed(Document document, ActionList actions, VerbNoun name, Action<Record> close) =>
        IO.lift(() => Optional(actions.ToRecord(name)).Iter(record => {
            close(record);
            document.File.AutoSave(toSeq(actions.Actions).Fold(AutoSaveReason.Unspecified, static (reason, action) => reason | action switch {
                CreateObjectAction => AutoSaveReason.ObjectAdded,
                DeleteObjectAction => AutoSaveReason.ObjectRemoved,
                ReplaceObjectAction => AutoSaveReason.ObjectAdded | AutoSaveReason.ObjectRemoved,
                ConnectAction => AutoSaveReason.WireAdded,
                DisconnectAction => AutoSaveReason.WireRemoved,
                PinAttachAction or PinDetachAction => AutoSaveReason.PinChanged,
                ObjectActivityAction => AutoSaveReason.ObjectEnabled,
                ParameterModifierAction => AutoSaveReason.ModifiersChanged,
                ExpressionEnabledAction or ExpressionTextAction => AutoSaveReason.ExpressionChanged,
                _ => AutoSaveReason.ObjectChanged,
            }));
            if (record.Response.HasFlag(Effects.ExpireLayout) || record.Response.HasFlag(Effects.InvalidateLayout)) Optional(Editor.Instance).Iter(static editor => editor.Canvas.Invalidate());
            if (record.Response.HasFlag(Effects.ExpireSolution) || record.Response.HasFlag(Effects.InvalidateSolution)) _ = document.Solution.Start();
            if (record.Response.HasFlag(Effects.ExpireDisplay) || record.Response.HasFlag(Effects.InvalidateDisplay)) document.Display.UpdateDisplay();
        }));

    // --- [OBJECTS]

    public static IO<T> Add<T>(Document document, T target) where T : IDocumentObject =>
        IO.lift(() => Fault.Unless(document.Objects.Add(target), RhinoReason.Declined, target.Nomen.Name).Map(_ => target))
        | @catch(static error => error.HasException<LoggedException>(), error => IO.fail<T>(Fault.Of(RhinoReason.Declined, target.Nomen.Name, inner: error)));

    // --- [GRAPHS]

    public static IO<Unit> Connect(IParameter source, IParameter target, ActionList actions, int atSource = int.MaxValue, int atTarget = int.MaxValue) =>
        from shared in IO.lift(() => Fault.Unless(source.Document is not null && source.Document == target.Document, RhinoReason.Invalid, (source.Nomen.Name, target.Nomen.Name)))
        from wired in IO.lift(() => target.Inputs.Forwards.Contains(source.InstanceId) && source.Outputs.Forwards.Contains(target.InstanceId)
            ? unit
            : ignore(Connections.Connect(source, target, atSource, atTarget, actions)))
        select unit;

    public static IO<IParameter> Intercept(IParameter parameter, Side side, IParameter inserted, ActionList actions) =>
        from document in Located(parameter)
        from ends in IO.lift(Conversions.Required(parameter.Attributes as IParameterAttributes, subject: parameter.Nomen.Name))
        from offset in IO.lift(() => {
            inserted.Attributes.Layout(Shape.Default);
            return new SizeF(SnappingSettings.Current.HorizontalGapSize + (inserted.Attributes.Bounds.Width / 2f), 0f);
        })
        from dropped in IO.lift(Fault.Unless(
            document.Methods.DropObject(inserted, side is Side.Input ? ends.Inlet - offset : ends.Outlet + offset, actions), RhinoReason.Declined, inserted.Nomen.Name))
        from wired in side is Side.Input
            ? IO.lift(() => ignore(Connections.MigrateAllInputs(parameter, inserted, actions))) >> Connect(inserted, parameter, actions)
            : Replaced(document, parameter, inserted, actions) >> Connect(parameter, inserted, actions)
        select inserted;

    public static IO<Unit> Splice(IParameter middle, ActionList actions) =>
        from document in Floating(middle)
        from ends in IO.lift(() => (Sources: Ends(document, middle.Inputs), Targets: Ends(document, middle.Outputs)))
        from paired in IO.lift(Fault.Unless(!ends.Sources.IsEmpty && !ends.Targets.IsEmpty, RhinoReason.Invalid, middle.Nomen.Name))
        from wired in (from right in ends.Targets from left in ends.Sources select (Left: left, Right: right))
            .TraverseM(link => IO.lift(() => (Source: link.Left.Outputs.IndexOf(middle.InstanceId), Target: link.Right.Inputs.IndexOf(middle.InstanceId)))
                .Bind(at => Connect(link.Left, link.Right, actions, at.Source, at.Target))).As()
        from deleted in IO.lift(() => ignore(document.Methods.DeleteObjects([middle], wires: null, actions)))
        select unit;

    public static IO<IParameter> Retype(IParameter relay, ActionList actions) =>
        from document in Floating(relay)
        from typed in IO.lift(() => ObjectProxies.CreateOverlappingParameter(Ends(document, relay.Inputs) is { IsEmpty: false } sources ? sources : Ends(document, relay.Outputs)))
        from copied in IO.lift(() => {
            typed.UserName = relay.UserName;
            AbstractParameter.SiphonProperties(relay, typed, StateAspects.Revertable);
        })
        from dropped in IO.lift(Fault.Unless(document.Methods.DropObject(typed, relay.Attributes.Bounds.Center, actions), RhinoReason.Declined, typed.Nomen.Name))
        from migrated in IO.lift(() => ignore(Connections.MigrateAllInputs(relay, typed, actions))) >> Replaced(document, relay, typed, actions)
        from deleted in IO.lift(() => ignore(document.Methods.DeleteObjects([relay], wires: null, actions)))
        select typed;

    public static IO<Unit> Rename(IParameter start, Side side, string userName, ActionList actions) =>
        from document in Located(start)
        from renamed in IO.lift(() => Wired(document, side, Seq(start), toHashSet(Seq(start.InstanceId)), Seq(start)).Run().Iter(member => {
            actions.Add(new RenameAction(member));
            member.UserName = userName;
        }))
        select unit;

    private static IO<Document> Located(IDocumentObject target) =>
        IO.lift(() => Conversions.Required(target.Document, subject: target.Nomen.Name));

    private static IO<Document> Floating(IParameter parameter) =>
        from document in Located(parameter)
        from floating in IO.lift(Fault.Unless(parameter.Kind is Kind.Floating, RhinoReason.Invalid, parameter.Kind))
        select document;

    private static IO<Unit> Replaced(Document document, IParameter source, IParameter replacement, ActionList actions) =>
        IO.lift(() => Ends(document, source.Outputs).Iter(target => ignore(Connections.ReplaceSource(source, replacement, target, actions))));

    private static Trampoline<Seq<IParameter>> Wired(Document document, Side side, Seq<IParameter> frontier, HashSet<Guid> seen, Seq<IParameter> reached) =>
        frontier.IsEmpty
            ? Trampoline.Pure(reached)
            : frontier.Bind(member => toSeq((side is Side.Input ? member.Inputs : member.Outputs).Forwards)).Fold((Seen: seen, Found: Seq<IParameter>()), (state, id) =>
                state.Seen.Contains(id)
                    ? state
                    : Optional(document.Objects.FindParameter(id)).Match(Some: found => (Seen: state.Seen.Add(id), Found: state.Found.Add(found)), None: () => state)) switch {
                        var step => Trampoline.More(() => Wired(document, side, step.Found.Filter(static member => member.Kind is Kind.Floating), step.Seen, reached + step.Found)),
                    };

    private static Seq<IParameter> Ends(Document document, Connections connections) =>
        toSeq(connections.Forwards).Choose(id => Optional(document.Objects.FindParameter(id))).Strict();
}
