using System.Collections.Generic;
using Rhino;
using Rhino.Collections;
using Rhino.Commands;
using Rhino.DocObjects;
using Rhino.PlugIns;
using Rhino.UI;
using UUIDNext;

namespace Rasm.Rhino.Commands;

// --- [SERVICES] ------------------------------------------------------------------------
internal sealed class CommandBody<TInput>(CommandDefinition<TInput> definition) where TInput : CommandInput, ICommandInput<TInput> {
    // --- [IDENTITY]
    private readonly PlugIn host = Session.PlugIn;

    public CommandDefinition<TInput> Definition => definition;

    public Guid Id { get; } = Uuid.NewNameBased(Session.PlugIn.Id, definition.EnglishName);

    public string LocalName => Localization.LocalizeCommandName(definition.EnglishName, host);

    // --- [HELP]
    public string HelpUrl => Conversions.Unset(Optional(PlugIn.GetPlugInInfo(host.Id)).Bind(static info => Conversions.Present(info.WebSite)));

    public void Help() =>
        _ = Session.Run(definition.Help.Match(Some: static topic => topic.Show, None: () => IO.lift(() => ignore(host.DisplayHelp(windowHandle: 0)))), member: definition.EnglishName);

    // --- [EXECUTION]
    public Result Run(IO<Unit> body) => Session.Run(Results.Of(body), Result.Failure, member: definition.EnglishName);

    public bool Replay(ReplayHistoryData data, Func<ReplayHistoryData, bool> inherited) =>
        Session.Run(
            definition.History.Match(
                Some: row => row.Replay(data).Map(value: true),
                None: () => IO.lift(() => inherited(data))),
            failed: false,
            member: definition.EnglishName);
}

// --- [COMPOSITION] ---------------------------------------------------------------------
public abstract class RunHost : Command {
    private readonly CommandBody<CommandInput.Run> body;

    private protected RunHost(CommandDefinition<CommandInput.Run> definition) => body = new(definition);

    public sealed override Guid Id => body.Id;

    public sealed override string EnglishName => body.Definition.EnglishName;

    public sealed override string LocalName => body.LocalName;

    protected sealed override string CommandContextHelpUrl => body.HelpUrl;

    protected sealed override void OnHelp() => body.Help();

    protected sealed override bool ReplayHistory(ReplayHistoryData replayData) => body.Replay(replayData, base.ReplayHistory);

    protected sealed override Result RunCommand(RhinoDoc doc, RunMode mode) => body.Run(IO.lift(() => body.Definition.Input.Body(doc, mode, this)).Flatten());
}

public abstract class SelCommandHost<TInput> : SelCommand where TInput : CommandInput, ICommandInput<TInput> {
    private protected SelCommandHost(CommandDefinition<TInput> definition) => Body = new(definition);

    public sealed override Guid Id => Body.Id;

    public sealed override string EnglishName => Body.Definition.EnglishName;

    public sealed override string LocalName => Body.LocalName;

    private protected CommandBody<TInput> Body { get; }

    protected sealed override string CommandContextHelpUrl => Body.HelpUrl;

    protected sealed override void OnHelp() => Body.Help();

    protected sealed override bool ReplayHistory(ReplayHistoryData replayData) => Body.Replay(replayData, base.ReplayHistory);
}

public abstract class SelectionHost : SelCommandHost<CommandInput.Selection> {
    private protected SelectionHost(CommandDefinition<CommandInput.Selection> definition) : base(definition) { }

    protected sealed override bool SelFilter(RhinoObject rhObj) => Body.Definition.Input.Filter(rhObj);
}

public abstract class SubobjectsHost : SelCommandHost<CommandInput.Subobjects> {
    private protected SubobjectsHost(CommandDefinition<CommandInput.Subobjects> definition) : base(definition) { }

    protected sealed override bool SelFilter(RhinoObject rhObj) => Body.Definition.Input.Filter(rhObj) is SelectionAnswer.Whole;

    protected sealed override bool SelSubObjectFilter(RhinoObject rhObj, List<ComponentIndex> indicesToSelect) =>
        Body.Definition.Input.Filter(rhObj) is SelectionAnswer.Subobjects parts && Selected(indicesToSelect, parts.Indices);

    private static bool Selected(List<ComponentIndex> indices, IterableNE<ComponentIndex> parts) {
        indices.AddRange(parts);
        return true;
    }
}

public abstract class TransformationHost : TransformCommand {
    private readonly CommandBody<CommandInput.Transformation> body;

    private protected TransformationHost(CommandDefinition<CommandInput.Transformation> definition) => body = new(definition);

    public sealed override Guid Id => body.Id;

    public sealed override string EnglishName => body.Definition.EnglishName;

    public sealed override string LocalName => body.LocalName;

    protected sealed override string CommandContextHelpUrl => body.HelpUrl;

    protected sealed override void OnHelp() => body.Help();

    protected sealed override bool ReplayHistory(ReplayHistoryData replayData) => body.Replay(replayData, base.ReplayHistory);

    protected sealed override Result RunCommand(RhinoDoc doc, RunMode mode) =>
        body.Run(
            (from list in use(static () => new TransformObjectList())
             from selected in IO.lift(() => Results.Ended(SelectObjects(LocalText.Of(body.Definition.Input.Prompt).Local, body.Definition.Input.Filter, list)))
             from moved in body.Definition.Input.Body(doc, mode, list)
             from applied in IO.lift(() => TransformObjects(list, moved.Xform, moved.Copy, autoHistory: true))
             select unit).Bracket());
}

public abstract class DefinedRunCommand<TSelf>() : RunHost(TSelf.Definition) where TSelf : DefinedRunCommand<TSelf>, IDefined<CommandDefinition<CommandInput.Run>>;

public abstract class DefinedSelectionCommand<TSelf>() : SelectionHost(TSelf.Definition) where TSelf : DefinedSelectionCommand<TSelf>, IDefined<CommandDefinition<CommandInput.Selection>>;

public abstract class DefinedSubobjectsCommand<TSelf>() : SubobjectsHost(TSelf.Definition) where TSelf : DefinedSubobjectsCommand<TSelf>, IDefined<CommandDefinition<CommandInput.Subobjects>>;

public abstract class DefinedTransformationCommand<TSelf>() : TransformationHost(TSelf.Definition) where TSelf : DefinedTransformationCommand<TSelf>, IDefined<CommandDefinition<CommandInput.Transformation>>;

internal sealed class PlainRunCommand(CommandDefinition<CommandInput.Run> definition) : RunHost(definition);

internal sealed class PlainSelectionCommand(CommandDefinition<CommandInput.Selection> definition) : SelectionHost(definition);

internal sealed class PlainSubobjectsCommand(CommandDefinition<CommandInput.Subobjects> definition) : SubobjectsHost(definition);

internal sealed class PlainTransformationCommand(CommandDefinition<CommandInput.Transformation> definition) : TransformationHost(definition);
