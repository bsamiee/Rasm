using Rasm.Rhino.Document;
using Rhino;
using Rhino.Commands;
using Rhino.DocObjects;
using Riok.Mapperly.Abstractions;

[assembly: UseStaticMapper(typeof(Answers))]

namespace Rasm.Rhino.Commands;

// --- [MODELS] --------------------------------------------------------------------------
public sealed record RecentCommand(Option<string> DisplayString, Option<string> Macro);

// --- [OPERATIONS] ----------------------------------------------------------------------
[Mapper]
public static partial class CommandRegistry {
    // --- [READS]
    public static IO<Option<Guid>> LookupCommandId(string name, bool english) =>
        IO.lift(() => Answers.Present(Command.LookupCommandId(name, english)));

    public static IO<Option<string>> LookupCommandName(Guid id, bool english) =>
        IO.lift(() => Answers.Present(Command.LookupCommandName(id, english)));

    public static IO<Seq<RecentCommand>> GetMostRecentCommands() =>
        IO.lift(static () => toSeq(Command.GetMostRecentCommands()).Map(ToRecent).Strict());

    public static IO<Seq<Guid>> GetCommandStack() =>
        IO.lift(static () => toSeq(Command.GetCommandStack()));

    private static partial RecentCommand ToRecent(MostRecentCommandDescription description);

    // --- [SCRIPTING]
    public static IO<Unit> ExecuteCommand(RhinoDoc doc, string commandName) =>
        IO.lift(() => Answers.FromResult(RhinoApp.ExecuteCommand(doc, commandName), commandName));

    public static IO<Unit> RunProxyCommand(RhinoDoc doc, Func<RhinoDoc, RunMode, IO<Unit>> body) =>
        IO.lift(() => Answers.Captured<Unit>(
            capture => Command.RunProxyCommand(
                (document, mode, _) => {
                    Fin<Unit> ran = body(document, mode).RunSafe();
                    capture(ran);
                    return Answers.ToResult(ran);
                },
                doc,
                data: null),
            nameof(Command.RunProxyCommand)));
}

// --- [COMPOSITION] ---------------------------------------------------------------------
public abstract class HostCommand : Command {
    protected abstract IO<Unit> Run(RhinoDoc doc, RunMode mode);

    protected virtual Option<IO<Unit>> Replay(ReplayHistoryData data) => None;

    protected sealed override Result RunCommand(RhinoDoc doc, RunMode mode) =>
        Answers.ToResult(Run(doc, mode).RunSafe(), ErrorOps.Report);

    protected sealed override bool ReplayHistory(ReplayHistoryData replayData) =>
        Answers.Succeeded(Replay(replayData), ErrorOps.Report, () => base.ReplayHistory(replayData));
}
