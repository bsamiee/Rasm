using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Rasm.Rhino.Objects;
using Rasm.Rhino.UI;
using Rhino;
using Rhino.Collections;
using Rhino.Commands;
using Rhino.DocObjects;

namespace Rasm.Rhino.Commands;

// --- [TYPES] ---------------------------------------------------------------------------
public interface ICommandInput<TSelf> where TSelf : CommandInput, ICommandInput<TSelf> {
    internal static abstract Command Plain(CommandDefinition<TSelf> definition);
}

// --- [MODELS] --------------------------------------------------------------------------
[Union]
public abstract partial record SelectionAnswer {
    public sealed record Whole : SelectionAnswer;

    public sealed record Subobjects(IterableNE<ComponentIndex> Indices) : SelectionAnswer;

    public sealed record Skipped : SelectionAnswer;
}

[Union]
public abstract partial record CommandInput {
    public sealed record Run(Func<RhinoDoc, RunMode, Command, IO<Unit>> Body) : CommandInput, ICommandInput<Run> {
        static Command ICommandInput<Run>.Plain(CommandDefinition<Run> definition) => new PlainRunCommand(definition);
    }

    public sealed record Selection(Func<RhinoObject, bool> Filter) : CommandInput, ICommandInput<Selection> {
        static Command ICommandInput<Selection>.Plain(CommandDefinition<Selection> definition) => new PlainSelectionCommand(definition);
    }

    public sealed record Subobjects(Func<RhinoObject, SelectionAnswer> Filter) : CommandInput, ICommandInput<Subobjects> {
        static Command ICommandInput<Subobjects>.Plain(CommandDefinition<Subobjects> definition) => new PlainSubobjectsCommand(definition);
    }

    public sealed record Transformation(string Prompt, ObjectType Filter, Func<RhinoDoc, RunMode, TransformObjectList, IO<(Transform Xform, bool Copy)>> Body)
        : CommandInput, ICommandInput<Transformation> {
        static Command ICommandInput<Transformation>.Plain(CommandDefinition<Transformation> definition) => new PlainTransformationCommand(definition);
    }
}

public abstract record PlugInCommand(string EnglishName) {
    public Option<HistoryReplay> History { get; init; }
    public Option<HelpTopic> Help { get; init; }

    public static PlugInCommand Of<TCommand, TInput>() where TCommand : Command, IDefined<CommandDefinition<TInput>>, new() where TInput : CommandInput, ICommandInput<TInput> =>
        TCommand.Definition with { Declared = Some<Func<Command>>(static () => new TCommand()) };

    internal abstract Command Realized();
}

public sealed record CommandDefinition<TInput>(string EnglishName, TInput Input) : PlugInCommand(EnglishName) where TInput : CommandInput, ICommandInput<TInput> {
    internal Option<Func<Command>> Declared { get; init; }

    internal override Command Realized() => Declared.Match(Some: static create => create(), None: () => TInput.Plain(this));
}

// --- [OPERATIONS] ----------------------------------------------------------------------
public static class Results {
    // --- [ENDINGS]
    private static readonly Seq<Result> Precedence = [Result.ExitRhino, Result.Cancel, Result.Nothing, Result.UnknownCommand];

    private static readonly FrozenDictionary<RhinoReason, Result> Endings =
        Precedence.Choose(static result => Ended(result) is Fin<Unit>.Fail(RhinoFault fault) ? Some(KeyValuePair.Create(fault.Reason, result)) : None).ToFrozenDictionary();

    public static Fin<Unit> Ended(Result result, [CallerArgumentExpression(nameof(result))] string call = "") =>
        result switch {
            Result.Success => unit,
            Result.Cancel or Result.CancelModelessDialog => Errors.Cancelled,
            Result.Nothing => new RhinoFault(call, RhinoReason.Nothing),
            Result.ExitRhino => new RhinoFault(call, RhinoReason.Exited),
            Result.UnknownCommand => new RhinoFault(call, RhinoReason.Unknown),
            _ => new RhinoFault(call, RhinoReason.Declined),
        };

    public static IO<T> Value<T>(Func<(Result Result, T Value)> call, [CallerArgumentExpression(nameof(call))] string member = "") =>
        IO.lift(() => call() switch {
            var (result, value) => Ended(result, member).Map(_ => value),
        });

    public static Fin<Result> Of(Fin<Unit> settled) =>
        settled.Map(static _ => Result.Success).BindFail(static error => Precedence.Find(toHashSet(error.AsIterable().Choose(Ending)).Contains).ToFin(error));

    public static IO<Result> Of(IO<Unit> effect) =>
        effect.Map(static _ => Result.Success).Catch(static error => IO.lift(Of(Fin.Fail<Unit>(error)))).As();

    internal static bool Ends(Error error) => error.AsIterable().Exists(static leaf => Reason(leaf).IsSome);

    private static Option<Result> Ending(Error leaf) => Reason(leaf) || Conversions.Present(leaf.Is(Errors.Cancelled), Result.Cancel);

    private static Option<Result> Reason(Error leaf) =>
        leaf.Filter<RhinoFault>().Head is RhinoFault fault && Endings.TryGetValue(fault.Reason, out Result result) ? Some(result) : None;
}

public static class ActiveCommands {
    // --- [OBSERVATION]
    public static readonly HostEvent<Seq<Guid>> Changed =
        Subscriptions.Merged(
                Subscriptions.On<CommandEventArgs>(static h => Command.BeginCommand += h, static h => Command.BeginCommand -= h),
                Subscriptions.On<CommandEventArgs>(static h => Command.EndCommand += h, static h => Command.EndCommand -= h))
            .Choose(static _ => Some(Conversions.Rows(Command.GetCommandStack())));

    // --- [PROXY]
    public static IO<Unit> RunProxyCommand(IO<Unit> body) =>
        from running in IO.lift(Command.InCommand)
        from answer in running ? body : Products.Captured<Unit>(capture => IO.lift(() => Command.RunProxyCommand(
            (_, _, _) => {
                Fin<Unit> result = Session.RunSafe(body, CancellationToken.None);
                capture(result);
                return Results.Of(result).IfFail(Result.Failure);
            }, doc: null, data: null)))
        select answer;
}
