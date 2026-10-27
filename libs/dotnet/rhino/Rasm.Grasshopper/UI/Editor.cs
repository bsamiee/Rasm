using Grasshopper2.UI;
using Rhino.Commands;

namespace Rasm.Grasshopper.UI;

// --- [OPERATIONS] ----------------------------------------------------------------------
public static class Editors {
    // --- [ATTACHMENT]
    public static IO<Unit> Attach(IGrasshopperPlugin owner, Func<Editor, IO<Unit>> attach) =>
        IO.lift(static () => Optional(Editor.Instance))
            .Bind(found => found.Match(Some: attach, None: () => Pending(owner, attach)));

    private static IO<Unit> Pending(IGrasshopperPlugin owner, Func<Editor, IO<Unit>> attach) =>
        from scope in use(static () => EnvIO.New(token: CancellationToken.None))
        from slot in IO.lift(static () => Ref(Option<Lease<Unit>>.None))
        from waiting in Hooks.On<CommandEventArgs>(static h => Command.EndCommand += h, static h => Command.EndCommand -= h, _ => Built(slot, attach).Under(scope), owner)
        select unit;

    private static IO<Unit> Built(Ref<Option<Lease<Unit>>> slot, Func<Editor, IO<Unit>> attach) =>
        IO.lift(() => Optional(Editor.Instance).Filter(_ => slot.Value.IsNone))
            .Bind(found => found.Traverse(editor => Leases.Swapped(slot, attach(editor))).As())
            .Map(unit);
}
