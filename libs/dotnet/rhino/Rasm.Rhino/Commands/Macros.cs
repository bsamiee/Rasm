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
        from running in IO.lift(Command.InCommand)
        from answer in running ? body : Callbacks.Captured<Unit>(capture => IO.lift(() => Command.RunProxyCommand(
            (_, _, _) => {
                Fin<Unit> result = Callbacks.Settled(body, CancellationToken.None);
                capture(result);
                return Conversions.ToResult(result).IfFail(Result.Failure);
            }, doc: null, data: null)), nameof(Command.RunProxyCommand))
        select answer;
}
