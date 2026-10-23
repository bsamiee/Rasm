using Rasm.Rhino.Objects;
using Rasm.Rhino.PlugIns;
using Rasm.Rhino.UI;
using Rhino;
using Rhino.Commands;
using Rhino.DocObjects;
using Rhino.PlugIns;
using Rhino.UI;
using Riok.Mapperly.Abstractions;

namespace Rasm.Rhino.Commands;

// --- [MODELS] --------------------------------------------------------------------------
public sealed record CommandHistory(int Version, bool HistoryReplayOnObjectAttributeChange, Func<ReplayHistoryData, Seq<ReplayHistoryResult>, IO<Seq<HistoryOutput>>> Outputs);

[Union]
public abstract partial record SelectionAnswer {
    public sealed record Whole : SelectionAnswer;

    public sealed record Subobjects(IterableNE<ComponentIndex> Indices) : SelectionAnswer;

    public sealed record Skipped : SelectionAnswer;
}

// --- [OPERATIONS] ----------------------------------------------------------------------
[Mapper(RequiredMappingStrategy = RequiredMappingStrategy.Source)]
internal static partial class CommandMapper {
    internal static partial void Update((bool TestLights, bool TestGrips, bool BeQuiet) options, SelCommand command);
}

public static class CommandRegistry {
    // --- [HISTORY]
    public static IO<Option<(Guid Id, Result Result)>> LastCommand() =>
        IO.lift(static () => Conversions.Present(Command.LastCommandId).Map(static id => (Id: id, Result: Command.LastCommandResult)));

    public static IO<Seq<(Option<string> DisplayString, Option<string> Macro)>> GetMostRecentCommands() =>
        IO.lift(static () => toSeq(Command.GetMostRecentCommands())
            .Map(static description => (Conversions.Present(description.DisplayString), Conversions.Present(description.Macro)))
            .Strict());

    // --- [REGISTRATION]
    public static IO<Seq<Command>> GetCommands(Guid plugInId) =>
        IO.lift(() => Refused.UnlessFound(PlugIn.Find(plugInId), nameof(PlugIn.Find)).Map(static plugIn => toSeq(plugIn.GetCommands())));

    // --- [IDENTITY]
    extension(Command command) {
        internal string LocalizedName => Localization.LocalizeCommandName(command.EnglishName, command.PlugIn);

        internal string HelpUrl(CallbackSite site) =>
            Callbacks.Answer(PlugInRegistry.ReadPlugIn(command.PlugIn.Id).Map(static state => Conversions.Unset(state.Bind(static held => held.WebSite))), static () => "", site);

        internal void ShowHelp(Option<HelpTopic> topic, CallbackSite site) =>
            Callbacks.Run(
                from views in IO.lift(Conversions.Cast<IPlugInViews>(command.PlugIn))
                from shown in (topic || views.Help).Match(Some: static held => held.Show, None: static () => IO.pure(unit))
                select shown,
                site);
    }
}

// --- [COMPOSITION] ---------------------------------------------------------------------
public abstract class HostCommand(IPlugInSink sink, Guid id, string englishName, Option<CommandHistory> history) : Command {
    // --- [IDENTITY]
    public sealed override Guid Id { get; } = id;

    public sealed override string EnglishName { get; } = englishName;

    public sealed override string LocalName => this.LocalizedName;

    protected virtual Option<HelpTopic> Help => None;

    protected sealed override string CommandContextHelpUrl => this.HelpUrl(new CallbackSite(sink, GetType(), nameof(CommandContextHelpUrl)));

    protected sealed override void OnHelp() => this.ShowHelp(Help, new CallbackSite(sink, GetType(), nameof(OnHelp)));

    // --- [EXECUTION]
    protected abstract IO<Unit> Run(RhinoDoc doc, RunMode mode, CallbackSite site);

    protected sealed override Result RunCommand(RhinoDoc doc, RunMode mode) =>
        (new CallbackSite(sink, GetType(), nameof(RunCommand)), history.Exists(static row => row.HistoryReplayOnObjectAttributeChange)) switch {
            (var site, var replayOnAttributeChange) => Callbacks.Answer(
                Conversions.ToResult(
                    from configured in IO.lift(() => { HistoryReplayOnObjectAttributeChange = replayOnAttributeChange; })
                    from result in Run(doc, mode, site)
                    select result),
                static () => Result.Failure,
                site),
        };

    // --- [HISTORY]
    protected sealed override bool ReplayHistory(ReplayHistoryData replayData) =>
        Callbacks.Succeeded(
            history.Map(row => Histories.Replay(replayData, row.Version, results => row.Outputs(replayData, results))),
            () => base.ReplayHistory(replayData),
            new CallbackSite(sink, GetType(), nameof(ReplayHistory)));
}

public abstract class HostSelectionCommand(IPlugInSink sink, Guid id, string englishName, bool testLights, bool testGrips, bool beQuiet) : SelCommand {
    // --- [IDENTITY]
    public sealed override Guid Id { get; } = id;

    public sealed override string EnglishName { get; } = englishName;

    public sealed override string LocalName => this.LocalizedName;

    protected virtual Option<HelpTopic> Help => None;

    protected sealed override string CommandContextHelpUrl => this.HelpUrl(new CallbackSite(sink, GetType(), nameof(CommandContextHelpUrl)));

    protected sealed override void OnHelp() => this.ShowHelp(Help, new CallbackSite(sink, GetType(), nameof(OnHelp)));

    // --- [EXECUTION]
    protected abstract SelectionAnswer Select(RhinoObject candidate);

    protected sealed override Result RunCommand(RhinoDoc doc, RunMode mode) =>
        Callbacks.Answer(
            Conversions.ToResult(IO.lift(() => CommandMapper.Update((testLights, testGrips, beQuiet), this))),
            static () => Result.Failure,
            new CallbackSite(sink, GetType(), nameof(RunCommand)));

    protected sealed override bool SelFilter(RhinoObject rhObj) =>
        Callbacks.Answer(
            IO.lift(() => Select(rhObj).Map(whole: true, subobjects: false, skipped: false)),
            static () => false,
            new CallbackSite(sink, GetType(), nameof(SelFilter)));

    protected sealed override bool SelSubObjectFilter(RhinoObject rhObj, List<ComponentIndex> indicesToSelect) =>
        Callbacks.Answer(
            IO.lift(() => Select(rhObj).Switch(
                indicesToSelect,
                whole: static (_, _) => false,
                subobjects: static (indices, answer) => {
                    indices.AddRange(answer.Indices);
                    return true;
                },
                skipped: static (_, _) => false)),
            static () => false,
            new CallbackSite(sink, GetType(), nameof(SelSubObjectFilter)));
}

public abstract class HostTransformCommand(IPlugInSink sink, Guid id, string englishName) : TransformCommand {
    // --- [IDENTITY]
    public sealed override Guid Id { get; } = id;

    public sealed override string EnglishName { get; } = englishName;

    public sealed override string LocalName => this.LocalizedName;

    protected virtual Option<HelpTopic> Help => None;

    protected sealed override string CommandContextHelpUrl => this.HelpUrl(new CallbackSite(sink, GetType(), nameof(CommandContextHelpUrl)));

    protected sealed override void OnHelp() => this.ShowHelp(Help, new CallbackSite(sink, GetType(), nameof(OnHelp)));

    // --- [EXECUTION]
    protected abstract IO<Unit> Run(RhinoDoc doc, RunMode mode, CallbackSite site);

    protected sealed override Result RunCommand(RhinoDoc doc, RunMode mode) =>
        new CallbackSite(sink, GetType(), nameof(RunCommand)) switch {
            var site => Callbacks.Answer(Conversions.ToResult(IO.lift(() => Run(doc, mode, site)).Flatten()), static () => Result.Failure, site),
        };
}
