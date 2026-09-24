using LanguageExt.UnsafeValueAccess;
using Rasm.Rhino.Document;
using Rhino;
using Rhino.Commands;
using Rhino.DocObjects;
using Rhino.UI;
using Riok.Mapperly.Abstractions;

[assembly: UseStaticMapper(typeof(Answers))]

namespace Rasm.Rhino.Commands;

// --- [MODELS] --------------------------------------------------------------------------
public sealed record RecentCommand(Option<string> DisplayString, Option<string> Macro);

// --- [OPERATIONS] ----------------------------------------------------------------------
public static class CommandRegistry {
    // --- [READS]
    public static IO<Option<Guid>> LookupCommandId(string name, bool english) =>
        IO.lift(() => Answers.Present(Command.LookupCommandId(name, english)));

    public static IO<Option<string>> LookupCommandName(Guid id, bool english) =>
        IO.lift(() => Optional(Command.LookupCommandName(id, english)));

    public static IO<Seq<RecentCommand>> GetMostRecentCommands() =>
        IO.lift(static () => toSeq(Command.GetMostRecentCommands()).Map(static row => CommandMapper.ToRecent(row)).Strict());

    public static IO<Seq<Guid>> GetCommandStack() =>
        IO.lift(static () => toSeq(Command.GetCommandStack()));

    // --- [PROMPT]
    public static IO<Unit> SetCommandPrompt(LocalizeStringPair prompt, Option<string> promptDefault) =>
        IO.lift(() => RhinoApp.SetCommandPrompt(prompt.Local, promptDefault.ValueUnsafe()));

    // --- [SCRIPTING]
    public static IO<Unit> RunScript(RhinoDoc doc, string script, bool echo, Option<string> mruDisplayString) =>
        IO.lift(() => Refused.Unless(
            mruDisplayString.Match(
                Some: display => RhinoApp.RunScript(doc.RuntimeSerialNumber, script, display, echo),
                None: () => RhinoApp.RunScript(doc.RuntimeSerialNumber, script, echo)),
            nameof(RhinoApp.RunScript)));

    public static IO<Unit> ExecuteCommand(RhinoDoc doc, string commandName) =>
        IO.lift(() => Answers.FromResult(RhinoApp.ExecuteCommand(doc, commandName), commandName));

    public static IO<Unit> RunProxyCommand(RhinoDoc doc, Func<RhinoDoc, RunMode, IO<Unit>> body) =>
        IO.lift(() => Answers.Captured<Unit>(
            capture => Command.RunProxyCommand(
                (document, mode, _) => {
                    Fin<Unit> ran = body(document, mode).RunSafe();
                    capture(ran);
                    return Answers.ToResult(ran, Thinktecture.Empty.Action);
                },
                doc,
                data: null),
            nameof(Command.RunProxyCommand)));
}

// --- [COMPOSITION] ---------------------------------------------------------------------
public abstract class HostCommand : Command {
    public override string EnglishName => GetType().Name;

    protected abstract IO<Unit> Run(RhinoDoc doc, RunMode mode);

    protected virtual IO<bool> Replay(ReplayHistoryData data) => IO.pure(value: false);

    protected sealed override Result RunCommand(RhinoDoc doc, RunMode mode) =>
        Answers.ToResult(Run(doc, mode).RunSafe(), ErrorOps.Report);

    protected sealed override bool ReplayHistory(ReplayHistoryData replayData) =>
        Answers.Answer(Replay(replayData), ErrorOps.Report, fallback: false);
}

[Mapper]
internal static partial class CommandMapper {
    internal static partial RecentCommand ToRecent(MostRecentCommandDescription description);
}
