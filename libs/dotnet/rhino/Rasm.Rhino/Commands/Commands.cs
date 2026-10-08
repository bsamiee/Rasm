using Rasm.Rhino.Objects.Authored;
using Rhino;
using Rhino.Commands;
using Rhino.DocObjects;
using Rhino.PlugIns;
using Rhino.UI;

namespace Rasm.Rhino.Commands;

// --- [MODELS] --------------------------------------------------------------------------
public sealed record RecentCommand(Option<string> DisplayString, Option<string> Macro);

public sealed record CommandHistory(int Version, bool HistoryReplayOnObjectAttributeChange, Func<ReplayHistoryData, Seq<ReplayHistoryResult>, IO<Seq<HistoryOutput>>> Outputs);

[Union]
public abstract partial record SelectionAnswer {
    public sealed record Whole : SelectionAnswer;

    public sealed record Components(IterableNE<ComponentIndex> Indices) : SelectionAnswer;

    public sealed record Skipped : SelectionAnswer;
}

// --- [OPERATIONS] ----------------------------------------------------------------------
public static class CommandRegistry {
    public static IO<Option<(Guid Id, Result Result)>> LastCommand() =>
        IO.lift(static () => Conversions.Present(Command.LastCommandId).Map(static id => (Id: id, Result: Command.LastCommandResult)));

    public static IO<Seq<RecentCommand>> GetMostRecentCommands() =>
        IO.lift(static () => toSeq(Command.GetMostRecentCommands())
            .Map(static description => new RecentCommand(Conversions.Present(description.DisplayString), Conversions.Present(description.Macro)))
            .Strict());

    public static IO<Seq<Command>> GetCommands(Guid plugInId) =>
        IO.lift(() => Missing.Unless(PlugIn.Find(plugInId), nameof(PlugIn.Find)).Map(static plugIn => toSeq(plugIn.GetCommands())));
}

// --- [COMPOSITION] ---------------------------------------------------------------------
public abstract class HostCommand(IPlugInSink sink, Guid id, string englishName, Option<CommandHistory> history) : Command {
    public sealed override Guid Id => id;

    public sealed override string EnglishName => englishName;

    public sealed override string LocalName => Localization.LocalizeCommandName(EnglishName, PlugIn);

    protected abstract IO<Unit> Run(RhinoDoc doc, RunMode mode, CallbackSite site);

    protected sealed override Result RunCommand(RhinoDoc doc, RunMode mode) =>
        new CallbackSite(sink, GetType(), nameof(RunCommand)) switch {
            var site => Callbacks.Answer(
                Conversions.ToResult(
                    IO.lift(() => { HistoryReplayOnObjectAttributeChange = history.Exists(static row => row.HistoryReplayOnObjectAttributeChange); })
                        .Bind(_ => Run(doc, mode, site))),
                static () => Result.Failure,
                site),
        };

    protected sealed override bool ReplayHistory(ReplayHistoryData replayData) =>
        Callbacks.Succeeded(
            history.Map(row => Histories.Replay(replayData, row.Version, results => row.Outputs(replayData, results))),
            () => base.ReplayHistory(replayData),
            new CallbackSite(sink, GetType(), nameof(ReplayHistory)));
}

public abstract class HostSelectionCommand(IPlugInSink sink, Guid id, string englishName, bool testLights, bool testGrips, bool beQuiet) : SelCommand {
    public sealed override Guid Id => id;

    public sealed override string EnglishName => englishName;

    public sealed override string LocalName => Localization.LocalizeCommandName(EnglishName, PlugIn);

    protected abstract SelectionAnswer Select(RhinoObject candidate);

    protected sealed override Result RunCommand(RhinoDoc doc, RunMode mode) =>
        Callbacks.Answer(
            Conversions.ToResult(IO.lift(() => { (TestLights, TestGrips, BeQuiet) = (testLights, testGrips, beQuiet); })),
            static () => Result.Failure,
            new CallbackSite(sink, GetType(), nameof(RunCommand)));

    protected sealed override bool SelFilter(RhinoObject rhObj) =>
        Callbacks.Answer(
            IO.lift(() => Select(rhObj).Map(whole: true, components: false, skipped: false)),
            static () => false,
            new CallbackSite(sink, GetType(), nameof(SelFilter)));

    protected sealed override bool SelSubObjectFilter(RhinoObject rhObj, List<ComponentIndex> indicesToSelect) =>
        Callbacks.Answer(
            IO.lift(() => Select(rhObj).Switch(
                indicesToSelect,
                whole: static (_, _) => false,
                components: static (indices, answer) => {
                    indices.AddRange(answer.Indices);
                    return true;
                },
                skipped: static (_, _) => false)),
            static () => false,
            new CallbackSite(sink, GetType(), nameof(SelSubObjectFilter)));
}

public abstract class HostTransformCommand(IPlugInSink sink, Guid id, string englishName) : TransformCommand {
    public sealed override Guid Id => id;

    public sealed override string EnglishName => englishName;

    public sealed override string LocalName => Localization.LocalizeCommandName(EnglishName, PlugIn);

    protected abstract IO<Unit> Run(RhinoDoc doc, RunMode mode, CallbackSite site);

    protected sealed override Result RunCommand(RhinoDoc doc, RunMode mode) =>
        new CallbackSite(sink, GetType(), nameof(RunCommand)) switch {
            var site => Callbacks.Answer(Conversions.ToResult(IO.pure(site).Bind(held => Run(doc, mode, held))), static () => Result.Failure, site),
        };
}
