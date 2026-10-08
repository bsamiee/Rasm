using Rhino;
using Rhino.Commands;

namespace Rasm.Rhino.Commands;

// --- [OPERATIONS] ----------------------------------------------------------------------
public static class Macros {
    // --- [NAMED_COMMANDS]
    public static IO<Unit> ExecuteCommand(RhinoDoc document, string commandName) =>
        IO.lift(() => Conversions.FromResult(RhinoApp.ExecuteCommand(document, commandName), nameof(RhinoApp.ExecuteCommand)));

    // --- [PROXY_COMMANDS]
    public static IO<Unit> RunProxyCommand(IO<Unit> body) =>
        IO.lift(static () => Command.InCommand())
            .Bind(running => running ? body : Callbacks.Captured<Unit>(capture => Proxied(body, capture), nameof(Command.RunProxyCommand)));

    private static IO<Unit> Proxied(IO<Unit> body, Action<Fin<Unit>> capture) =>
        IO.lift(() => Command.RunProxyCommand(
            (_, _, _) => {
                Fin<Unit> ran = Try.lift(body.Run).Run();
                capture(ran);
                return Conversions.ToResult(IO.lift(ran)).IfFail(Result.Failure);
            },
            doc: null,
            data: null));
}
