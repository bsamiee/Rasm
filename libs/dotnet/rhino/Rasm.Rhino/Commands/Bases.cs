using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Rasm.Rhino.Objects;
using Rasm.Rhino.PlugIns;
using Rasm.Rhino.UI;
using Rhino;
using Rhino.Commands;
using Rhino.DocObjects;
using Rhino.UI;

namespace Rasm.Rhino.Commands;

// --- [MODELS] --------------------------------------------------------------------------
public sealed record CommandHistory(int Version, bool HistoryReplayOnObjectAttributeChange, Func<ReplayHistoryData, Seq<ReplayHistoryResult>, IO<Seq<HistoryOutput>>> Outputs);

public sealed record CommandEnd(Guid Id, Result Result);

public sealed record RecentCommand(Option<string> DisplayString, Option<string> Macro);

[Union]
public abstract partial record SelectionAnswer {
    public sealed record Whole : SelectionAnswer;

    public sealed record Subobjects(IterableNE<ComponentIndex> Indices) : SelectionAnswer;

    public sealed record Skipped : SelectionAnswer;
}

// --- [OPERATIONS] ----------------------------------------------------------------------
public static class Results {
    public static Fin<Unit> Ended(Result result, [CallerArgumentExpression(nameof(result))] string call = "") =>
        result switch {
            Result.Success => unit,
            Result.Cancel or Result.CancelModelessDialog => Errors.Cancelled,
            _ => new RhinoFault(call, toSeq(RhinoReason.Items).Find(reason => reason.Ending == Some(result)).IfNone(RhinoReason.Declined)),
        };

    public static Fin<Result> Of(Fin<Unit> settled) =>
        settled.Map(static _ => Result.Success).BindFail(static error =>
            Seq(Result.ExitRhino, Result.Cancel, Result.Nothing, Result.UnknownCommand)
                .Find(toHashSet(toSeq(error.AsIterable()).Choose(static leaf => leaf is RhinoFault fault ? fault.Reason.Ending : Conversions.Found(leaf.Is(Errors.Cancelled), Result.Cancel))).Contains)
                .ToFin(error));

    public static IO<Result> Of(IO<Unit> effect) =>
        effect.Map(static _ => Result.Success).Catch(static error => IO.lift(Of(Fin.Fail<Unit>(error)))).As();
}

public static class CommandOps {
    // --- [EXECUTION]
    public static IO<Unit> ExecuteCommand(RhinoDoc document, string commandName) =>
        IO.lift(() => Results.Ended(RhinoApp.ExecuteCommand(document, commandName)));

    public static IO<Unit> RunProxyCommand(IO<Unit> body) =>
        from running in IO.lift(Command.InCommand)
        from answer in running ? body : Callbacks.Captured<Unit>(capture => IO.lift(() => Command.RunProxyCommand(
            (_, _, _) => {
                Fin<Unit> result = Callbacks.Settled(body, CancellationToken.None);
                capture(result);
                return Results.Of(result).IfFail(Result.Failure);
            }, doc: null, data: null)))
        select answer;

    // --- [HISTORY]
    public static IO<Option<CommandEnd>> LastCommand() =>
        IO.lift(static () => Conversions.Present(Command.LastCommandId).Map(static id => new CommandEnd(id, Command.LastCommandResult)));

    public static IO<Seq<RecentCommand>> GetMostRecentCommands() =>
        IO.lift(static () => toSeq(Command.GetMostRecentCommands())
            .Map(static description => new RecentCommand(Conversions.Present(description.DisplayString), Conversions.Present(description.Macro)))
            .Strict());

    // --- [IDENTITY]
    extension(Command command) {
        internal string HelpUrl([CallerMemberName] string member = "") =>
            Conversions.Unset(command.PlugIn.Answer(PlugInRegistry.ReadPlugIn(command.PlugIn.Id).Map(static state => state.Bind(static held => held.WebSite)), Option<string>.None, member));

        internal Unit ShowHelp(Option<HelpTopic> topic, [CallerMemberName] string member = "") =>
            command.PlugIn.Run(
                from views in IO.lift(Conversions.Cast<IPlugInViews>(command.PlugIn))
                from shown in (topic || views.Help).Match(Some: static held => held.Show, None: static () => IO.pure(unit))
                select shown,
                member);
    }
}

// --- [COMPOSITION] ---------------------------------------------------------------------
public abstract class HostCommand : Command {
    // --- [IDENTITY]
    private readonly Option<CommandHistory> history;

    protected HostCommand(Guid id, string englishName, Option<CommandHistory> history) {
        (Id, EnglishName, this.history) = (id, englishName, history);
        HistoryReplayOnObjectAttributeChange = history.Exists(static row => row.HistoryReplayOnObjectAttributeChange);
    }

    public sealed override Guid Id { get; }

    public sealed override string EnglishName { get; }

    public sealed override string LocalName => Localization.LocalizeCommandName(EnglishName, PlugIn);

    protected virtual Option<HelpTopic> Help => None;

    protected sealed override string CommandContextHelpUrl => this.HelpUrl();

    protected sealed override void OnHelp() => _ = this.ShowHelp(Help);

    // --- [EXECUTION]
    protected abstract IO<Unit> Run(RhinoDoc doc, RunMode mode);

    protected sealed override Result RunCommand(RhinoDoc doc, RunMode mode) => PlugIn.Answer(Results.Of(IO.lift(() => Run(doc, mode)).Flatten()), Result.Failure);

    // --- [HISTORY]
    protected sealed override bool ReplayHistory(ReplayHistoryData replayData) =>
        PlugIn.Answer(
            history.Map(row => Histories.Replay(replayData, row.Version, results => row.Outputs(replayData, results)).Map(value: true)).IfNone(() => IO.lift(() => base.ReplayHistory(replayData))),
            failed: false);
}

public abstract class HostSelectionCommand(Guid id, string englishName) : SelCommand {
    // --- [IDENTITY]
    public sealed override Guid Id { get; } = id;

    public sealed override string EnglishName { get; } = englishName;

    public sealed override string LocalName => Localization.LocalizeCommandName(EnglishName, PlugIn);

    protected virtual Option<HelpTopic> Help => None;

    protected sealed override string CommandContextHelpUrl => this.HelpUrl();

    protected sealed override void OnHelp() => _ = this.ShowHelp(Help);

    // --- [EXECUTION]
    protected abstract SelectionAnswer Select(RhinoObject candidate);

    protected sealed override bool SelFilter(RhinoObject rhObj) => PlugIn.Answer(IO.lift(() => Select(rhObj).Map(whole: true, subobjects: false, skipped: false)), failed: false);

    protected sealed override bool SelSubObjectFilter(RhinoObject rhObj, List<ComponentIndex> indicesToSelect) =>
        PlugIn.Answer(
            IO.lift(() => Select(rhObj).Switch(
                indicesToSelect,
                whole: static (_, _) => false,
                subobjects: static (indices, answer) => {
                    indices.AddRange(answer.Indices);
                    return true;
                },
                skipped: static (_, _) => false)),
            failed: false);
}

public abstract class HostTransformCommand(Guid id, string englishName) : TransformCommand {
    // --- [IDENTITY]
    public sealed override Guid Id { get; } = id;

    public sealed override string EnglishName { get; } = englishName;

    public sealed override string LocalName => Localization.LocalizeCommandName(EnglishName, PlugIn);

    protected virtual Option<HelpTopic> Help => None;

    protected sealed override string CommandContextHelpUrl => this.HelpUrl();

    protected sealed override void OnHelp() => _ = this.ShowHelp(Help);

    // --- [EXECUTION]
    protected abstract IO<Unit> Run(RhinoDoc doc, RunMode mode);

    protected sealed override Result RunCommand(RhinoDoc doc, RunMode mode) => PlugIn.Answer(Results.Of(IO.lift(() => Run(doc, mode)).Flatten()), Result.Failure);
}
