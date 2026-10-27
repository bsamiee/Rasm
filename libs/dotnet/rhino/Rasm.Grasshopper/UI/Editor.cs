using Grasshopper2.UI;
using Grasshopper2.UI.Canvas;
using Rasm.RhinoCommon;
using Rasm.RhinoCommon.Commands;
using Rasm.RhinoCommon.Events;
using Rasm.RhinoCommon.PlugIns;
using Rhino;

namespace Rasm.Grasshopper.UI;

// --- [OPERATIONS] ----------------------------------------------------------------------
public static class Editors {
    // --- [EDITOR]
    public static IO<Unit> Attach(IPlugInLifetime owner, Func<Editor, IO<Unit>> attach) =>
        IO.lift(static () => Optional(Editor.Instance))
            .Bind(found => found.Match(Some: attach, None: () => Pending(owner, attach)));

    private static IO<Unit> Pending(IPlugInLifetime owner, Func<Editor, IO<Unit>> attach) =>
        from scope in use(static () => EnvIO.New(token: CancellationToken.None))
        from slot in IO.lift(static () => Ref(Option<Lease<Unit>>.None))
        from waiting in EventKind.EndCommand.Inline(_ => Built(slot, attach).Under(scope), owner)
        select unit;

    private static IO<Unit> Built(Ref<Option<Lease<Unit>>> slot, Func<Editor, IO<Unit>> attach) =>
        IO.lift(() => Optional(Editor.Instance).Filter(_ => slot.Value.IsNone))
            .Bind(found => found.Traverse(editor => Products.Swapped(slot, attach(editor))).As())
            .Map(unit);

    public static IO<Editor> Show(RhinoDoc document) =>
        IO.lift(static () => Optional(Editor.Instance)).Bind(found => found.Match(
            Some: static _ => IO.lift(static () => Editor.ShowEditor()),
            None: () => Launched(document)));

    private static IO<Editor> Launched(RhinoDoc document) =>
        CommandOps.ExecuteCommand(document, "_GH2").Bind(static _ => IO.lift(static () => Conversions.Found(Editor.Instance)));

    public static IO<Canvas> Canvas => IO.lift(static () => Conversions.Found(Editor.Instance).Map(static editor => editor.Canvas));

    // --- [GETTERS]
    public static IO<T> RhinoGetter<T>(RhinoDoc document, IO<T> get) =>
        IO.lift(() => GrasshopperFault.Unless(Editor.BeginRhinoGetter(document))).Bind(_ => get);
}
