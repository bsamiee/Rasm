using Color = System.Drawing.Color;
using DrawingPoint = System.Drawing.Point;
using Result = Rhino.Commands.Result;

namespace Rasm.Rhino;

// --- [MODELS] ---------------------------------------------------------------------------
public abstract record CommandInputPolicy {
    private protected CommandInputPolicy() { }

    public static CommandInputPolicy Prompt(string value) => new ConfigureCase(Run: getter => Effect(action: () => getter.SetCommandPrompt(value)));
    public static CommandInputPolicy PromptDefault(string value) => new ConfigureCase(Run: getter => Effect(action: () => getter.SetCommandPromptDefault(value)));
    public static CommandInputPolicy Default(Point3d value) => new ConfigureCase(Run: getter => Effect(action: () => getter.SetDefaultPoint(value)));
    public static CommandInputPolicy Default(double value) => new ConfigureCase(Run: getter => Effect(action: () => getter.SetDefaultNumber(value)));
    public static CommandInputPolicy Default(int value) => new ConfigureCase(Run: getter => Effect(action: () => getter.SetDefaultInteger(value)));
    public static CommandInputPolicy Default(string value) => new ConfigureCase(Run: getter => Effect(action: () => getter.SetDefaultString(value)));
    public static CommandInputPolicy Default(Color value) => new ConfigureCase(Run: getter => Effect(action: () => getter.SetDefaultColor(value)));
    public static CommandInputPolicy Timeout(int milliseconds) => new ConfigureCase(Run: getter => Effect(action: () => getter.SetWaitDuration(milliseconds)));
    public static CommandInputPolicy Accept(GetResult result, bool acceptZero = true) => new ConfigureCase(Run: getter => result switch {
        GetResult.Nothing => Effect(action: () => getter.AcceptNothing(enable: true)),
        GetResult.Undo => Effect(action: () => getter.AcceptUndo(enable: true)),
        GetResult.Number => Effect(action: () => getter.AcceptNumber(enable: true, acceptZero: acceptZero)),
        GetResult.Point => Effect(action: () => getter.AcceptPoint(enable: true)),
        GetResult.Color => Effect(action: () => getter.AcceptColor(enable: true)),
        GetResult.String => Effect(action: () => getter.AcceptString(enable: true)),
        GetResult.CustomMessage => Effect(action: () => getter.AcceptCustomMessage(enable: true)),
        _ => Fin.Fail<Unit>(error: Op.Of(name: nameof(Accept)).InvalidInput()),
    });
    public static CommandInputPolicy EnterWhenDone => new ConfigureCase(Run: static getter => Effect(action: () => getter.AcceptEnterWhenDone(enable: true)));
    public static CommandInputPolicy TransparentCommands(bool enabled = true) => new ConfigureCase(Run: getter => Effect(action: () => getter.EnableTransparentCommands(enable: enabled)));
    public static CommandInputPolicy Object(
        ObjectType filter = ObjectType.AnyObject,
        GeometryAttributeFilter attributeFilter = default,
        bool preSelect = true,
        bool ignoreUnacceptablePreselectedObjects = true,
        bool postSelect = true,
        bool deselectAllBeforePostSelect = false,
        bool oneByOnePostSelect = false,
        bool subObjectSelect = false,
        bool groupSelect = false,
        bool referenceObjectSelect = false,
        bool lockedObjectSelect = false,
        bool alreadySelectedObjectSelect = false,
        bool selPrevious = true,
        bool highlight = true,
        bool clearObjectsOnEntry = false,
        bool unselectObjectsOnExit = false) =>
        new ObjectCase(Run: getter => Effect(action: () => {
            getter.GeometryFilter = filter;
            getter.GeometryAttributeFilter = attributeFilter;
            getter.EnablePreSelect(preSelect, ignoreUnacceptablePreselectedObjects);
            getter.EnablePostSelect(enable: postSelect);
            getter.DeselectAllBeforePostSelect = deselectAllBeforePostSelect;
            getter.OneByOnePostSelect = oneByOnePostSelect;
            getter.SubObjectSelect = subObjectSelect;
            getter.GroupSelect = groupSelect;
            getter.ReferenceObjectSelect = referenceObjectSelect;
            getter.LockedObjectSelect = lockedObjectSelect;
            getter.AlreadySelectedObjectSelect = alreadySelectedObjectSelect;
            getter.EnableSelPrevious(enable: selPrevious);
            getter.EnableHighlight(enable: highlight);
            getter.EnableClearObjectsOnEntry(enable: clearObjectsOnEntry);
            getter.EnableUnselectObjectsOnExit(enable: unselectObjectsOnExit);
        }));
    public static CommandInputPolicy Options(params CommandOption[] options) => new OptionsCase(Values: toSeq(options));

    internal static Fin<Seq<CommandOption>> Apply(Seq<CommandInputPolicy> policies, GetBaseClass getter) =>
        Optional(getter)
            .ToFin(Fail: Op.Of(name: nameof(CommandInputPolicy)).InvalidInput())
            .Bind(g => policies.TraverseM(policy => policy.Apply(getter: g)).As())
            .Map(static values => values.Bind(static options => options));

    internal static Fin<Unit> ApplyObjectDefaults(GetObject getter) =>
        Object().Apply(getter: getter).Map(static _ => unit);

    private Fin<Seq<CommandOption>> Apply(GetBaseClass getter) =>
        this switch {
            ConfigureCase configure => configure.Run(arg: getter).Map(static _ => Seq<CommandOption>()),
            ObjectCase objects when getter is GetObject getObject => objects.Run(arg: getObject).Map(static _ => Seq<CommandOption>()),
            ObjectCase => Fin.Fail<Seq<CommandOption>>(error: Op.Of(name: nameof(Object)).InvalidInput()),
            OptionsCase options => Fin.Succ(value: options.Values),
            _ => Fin.Fail<Seq<CommandOption>>(error: Op.Of(name: nameof(CommandInputPolicy)).InvalidInput()),
        };

    private static Fin<Unit> Effect(Action action) {
        action();
        return Fin.Succ(value: unit);
    }

    private sealed record ConfigureCase(Func<GetBaseClass, Fin<Unit>> Run) : CommandInputPolicy;
    private sealed record OptionsCase(Seq<CommandOption> Values) : CommandInputPolicy;
    private sealed record ObjectCase(Func<GetObject, Fin<Unit>> Run) : CommandInputPolicy;
}

public readonly record struct CommandGet<T>(
    GetResult Raw,
    Result CommandResult,
    CommandGet<T>.Terminal Input,
    bool GotDefault,
    Option<RhinoView> View,
    Option<DrawingPoint> WindowPoint) {
    public Option<T> Value => Input switch {
        Terminal.Primary primary => Some(primary.Value),
        Terminal.SelectedOption { Value: T typed } => Some(typed),
        _ => Option<T>.None,
    };
    public Option<CommandOptionValue> Option => Input switch {
        Terminal.SelectedOption option => Some(option.Value),
        _ => Option<CommandOptionValue>.None,
    };

    internal static CommandGet<T> Of(GetBaseClass getter, GetResult raw, Option<T> value, Option<CommandOptionValue> option) =>
        new(
            Raw: raw,
            CommandResult: getter.CommandResult(),
            Input: TerminalOf(getter: getter, raw: raw, value: value, option: option),
            GotDefault: getter.GotDefault(),
            View: Optional(getter.View()),
            WindowPoint: raw switch { GetResult.Point or GetResult.Point2d => Some<DrawingPoint>(getter.Point2d()), _ => Option<DrawingPoint>.None });

    private static Terminal TerminalOf(GetBaseClass getter, GetResult raw, Option<T> value, Option<CommandOptionValue> option) =>
        option.Match(
            Some: static selected => (Terminal)new Terminal.SelectedOption(Value: selected),
            None: () => value.Match(
                Some: static primary => (Terminal)new Terminal.Primary(Value: primary),
                None: () => raw switch {
                    GetResult.Nothing => new Terminal.NoInput(),
                    GetResult.Number => new Terminal.Number(Value: getter.Number()),
                    GetResult.Color => new Terminal.ColorValue(Value: getter.Color()),
                    GetResult.Undo => new Terminal.Undo(),
                    GetResult.Point => new Terminal.Point(Value: getter.Point()),
                    GetResult.String => new Terminal.TextValue(Value: getter.StringResult()),
                    GetResult.CustomMessage => new Terminal.CustomMessage(Value: getter.CustomMessage()),
                    GetResult.Timeout => new Terminal.Timeout(),
                    _ => new Terminal.Other(Raw: raw),
                }));

    public abstract record Terminal {
        private protected Terminal() { }
        public sealed record Primary(T Value) : Terminal;
        public sealed record SelectedOption(CommandOptionValue Value) : Terminal;
        public sealed record NoInput : Terminal;
        public sealed record Number(double Value) : Terminal;
        public sealed record Point(Point3d Value) : Terminal;
        public sealed record ColorValue(Color Value) : Terminal;
        public sealed record TextValue(string Value) : Terminal;
        public sealed record Undo : Terminal;
        public sealed record Timeout : Terminal;
        public sealed record CustomMessage(object? Value) : Terminal;
        public sealed record Other(GetResult Raw) : Terminal;
    }
}

public sealed class CommandInputRequest<T> {
    private readonly Func<CommandInput, Fin<CommandGet<T>>> read;

    private CommandInputRequest(Func<CommandInput, Fin<CommandGet<T>>> read) => this.read = read;

    internal static CommandInputRequest<T> Of(Func<CommandInput, Fin<CommandGet<T>>> read) => new(read: read);
    internal Fin<CommandGet<T>> Read(CommandInput input) => read(arg: input);
    internal readonly record struct ReadState(Option<CommandOptionValue> Selected, Seq<Guid> Preselected);
    internal readonly record struct ReadTransition(ReadState State, bool Repeat);
}

public static class CommandInputs {
    public static CommandInputRequest<CommandSelection> Objects(int minimum = 1, int maximum = 1, params CommandInputPolicy[] policies) =>
        Request(
            policies: toSeq(policies),
            create: static () => new GetObject(),
            configure: static getter => CommandInputPolicy.ApplyObjectDefaults(getter: getter),
            receive: getter => getter.GetMultiple(minimumNumber: minimum, maximumNumber: maximum),
            value: static (input, getter, raw, preselected) => SelectionOf(document: input.Document, getter: getter, raw: raw, preselected: preselected),
            transition: ObjectTransition);

    public static CommandInputRequest<Point3d> Point(params CommandInputPolicy[] policies) =>
        Request(
            policies: toSeq(policies),
            create: static () => new GetPoint(),
            receive: static getter => getter.Get(),
            value: static (_, getter, raw, _) => raw switch { GetResult.Point => Some(getter.Point()), _ => Option<Point3d>.None });

    public static CommandInputRequest<string> Text(bool literal = false, params CommandInputPolicy[] policies) =>
        Request(
            policies: toSeq(policies),
            create: static () => new GetString(),
            receive: getter => literal ? getter.GetLiteralString() : getter.Get(),
            value: static (_, getter, raw, _) => raw switch { GetResult.String => Optional(getter.StringResult()), _ => Option<string>.None });

    public static CommandInputRequest<double> Number(Option<double> lower = default, Option<double> upper = default, bool strictlyLower = false, bool strictlyUpper = false, params CommandInputPolicy[] policies) =>
        Request(
            policies: toSeq(policies),
            create: static () => new GetNumber(),
            configure: getter => Limits(getter: getter, lower: lower, upper: upper, strictlyLower: strictlyLower, strictlyUpper: strictlyUpper),
            receive: static getter => getter.Get(),
            value: static (_, getter, raw, _) => raw switch { GetResult.Number => Some(getter.Number()), _ => Option<double>.None });

    public static CommandInputRequest<int> Integer(Option<int> lower = default, Option<int> upper = default, bool strictlyLower = false, bool strictlyUpper = false, params CommandInputPolicy[] policies) =>
        Request(
            policies: toSeq(policies),
            create: static () => new GetInteger(),
            configure: getter => Limits(getter: getter, lower: lower, upper: upper, strictlyLower: strictlyLower, strictlyUpper: strictlyUpper),
            receive: static getter => getter.Get(),
            value: static (_, getter, raw, _) => raw switch { GetResult.Number => Some(getter.Number()), _ => Option<int>.None });

    public static CommandInputRequest<CommandOptionValue> Options(params CommandInputPolicy[] policies) =>
        Request(
            policies: toSeq(policies),
            create: static () => new GetOption(),
            receive: static getter => getter.Get(),
            value: static (_, _, _, _) => Option<CommandOptionValue>.None,
            terminalOptions: true);

    private static CommandInputRequest<T> Request<T, TGetter>(
        Seq<CommandInputPolicy> policies,
        Func<TGetter> create,
        Func<TGetter, GetResult> receive,
        Func<CommandInput, TGetter, GetResult, Seq<Guid>, Option<T>> value,
        Func<TGetter, Fin<Unit>>? configure = null,
        Func<GetBaseClass, GetResult, CommandInputRequest<T>.ReadState, CommandInputRequest<T>.ReadTransition>? transition = null,
        bool terminalOptions = false) where TGetter : GetBaseClass, IDisposable =>
        CommandInputRequest<T>.Of(read: input => {
            using TGetter getter = create();
            return from _ in configure switch { Func<TGetter, Fin<Unit>> apply => apply(arg: getter), _ => Fin.Succ(value: unit) }
                   from options in CommandInputPolicy.Apply(policies: policies, getter: getter)
                   from result in input.ReadWith(
                       getter: getter,
                       options: options,
                       receive: () => receive(arg: getter),
                       value: (g, raw, preselected) => value(arg1: input, arg2: (TGetter)g, arg3: raw, arg4: preselected),
                       transition: transition,
                       terminalOptions: terminalOptions)
                   select result;
        });

    [System.Diagnostics.CodeAnalysis.SuppressMessage("Reliability", "CA2000", Justification = "CommandGet transfers CommandSelection ownership to the command caller.")]
    private static Option<CommandSelection> SelectionOf(RhinoDoc document, GetObject getter, GetResult raw, Seq<Guid> preselected) =>
        raw switch {
            GetResult.Object => Optional(CommandSelection.From(
                document: document,
                references: toSeq(Enumerable.Range(start: 0, count: getter.ObjectCount).Select(index => getter.Object(index))),
                preselected: preselected)),
            _ => Option<CommandSelection>.None,
        };

    private static CommandInputRequest<T>.ReadTransition ObjectTransition<T>(GetBaseClass getter, GetResult raw, CommandInputRequest<T>.ReadState state) =>
        (getter, raw) switch {
            (GetObject objects, GetResult.Option) => DisablePreSelect(getter: objects, state: state, repeat: true),
            (GetObject objects, GetResult.Object) when objects.ObjectsWerePreselected => DisablePreSelect(
                getter: objects,
                state: state with { Preselected = state.Preselected + toSeq(Enumerable.Range(start: 0, count: objects.ObjectCount).Select(index => objects.Object(index).ObjectId)) },
                repeat: true),
            _ => new CommandInputRequest<T>.ReadTransition(State: state, Repeat: false),
        };

    private static CommandInputRequest<T>.ReadTransition DisablePreSelect<T>(GetObject getter, CommandInputRequest<T>.ReadState state, bool repeat) {
        getter.EnablePreSelect(false, true);
        return new CommandInputRequest<T>.ReadTransition(State: state, Repeat: repeat);
    }

    private static Fin<Unit> Limits(GetNumber getter, Option<double> lower, Option<double> upper, bool strictlyLower, bool strictlyUpper) {
        _ = lower.Iter(value => getter.SetLowerLimit(value, strictlyLower));
        _ = upper.Iter(value => getter.SetUpperLimit(value, strictlyUpper));
        return Fin.Succ(value: unit);
    }

    private static Fin<Unit> Limits(GetInteger getter, Option<int> lower, Option<int> upper, bool strictlyLower, bool strictlyUpper) {
        _ = lower.Iter(value => getter.SetLowerLimit(value, strictlyLower));
        _ = upper.Iter(value => getter.SetUpperLimit(value, strictlyUpper));
        return Fin.Succ(value: unit);
    }
}

public sealed record CommandInput {
    public CommandInput(RhinoDoc document) {
        ArgumentNullException.ThrowIfNull(argument: document);
        Document = document;
    }

    public RhinoDoc Document { get; }

    public Fin<CommandGet<T>> Read<T>(CommandInputRequest<T> request) =>
        Optional(request)
            .ToFin(Fail: Op.Of(name: nameof(Read)).InvalidInput())
            .Bind(input => input.Read(input: this));

    internal Fin<CommandGet<T>> ReadWith<T>(
        GetBaseClass getter,
        Seq<CommandOption> options,
        Func<GetResult> receive,
        Func<GetBaseClass, GetResult, Seq<Guid>, Option<T>> value,
        Func<GetBaseClass, GetResult, CommandInputRequest<T>.ReadState, CommandInputRequest<T>.ReadTransition>? transition = null,
        bool terminalOptions = false) =>
        Optional(getter)
            .ToFin(Fail: Op.Of(name: nameof(ReadWith)).InvalidInput())
            .Bind(g => CommandOption.Bind(options: options, getter: g).Bind(scope => {
                using CommandOptionScope active = scope;
                return ReadLoop(
                    getter: g,
                    scope: active,
                    receive: receive,
                    value: value,
                    state: new CommandInputRequest<T>.ReadState(Selected: Option<CommandOptionValue>.None, Preselected: Seq<Guid>()),
                    transition: transition ?? Stay,
                    terminalOptions: terminalOptions);
            }));

    private static Fin<CommandGet<T>> ReadLoop<T>(
        GetBaseClass getter,
        CommandOptionScope scope,
        Func<GetResult> receive,
        Func<GetBaseClass, GetResult, Seq<Guid>, Option<T>> value,
        CommandInputRequest<T>.ReadState state,
        Func<GetBaseClass, GetResult, CommandInputRequest<T>.ReadState, CommandInputRequest<T>.ReadTransition> transition,
        bool terminalOptions) =>
        receive() switch {
            GetResult.Cancel => Fin.Fail<CommandGet<T>>(error: new Fault.Cancelled()),
            GetResult.Option when terminalOptions => scope.Snapshot(getter: getter).Bind(option => option is T typed
                ? Fin.Succ(value: CommandGet<T>.Of(getter: getter, raw: GetResult.Option, value: Some(typed), option: Some(option)))
                : Fin.Fail<CommandGet<T>>(error: Op.Of(name: nameof(ReadLoop)).InvalidResult())),
            GetResult.Option => scope.Snapshot(getter: getter).Bind(option => transition(getter, GetResult.Option, state with { Selected = Some(option) }).State switch {
                CommandInputRequest<T>.ReadState next => ReadLoop(getter: getter, scope: scope, receive: receive, value: value, state: next, transition: transition, terminalOptions: terminalOptions),
            }),
            GetResult raw => transition(getter, raw, state) switch {
                { Repeat: true, State: CommandInputRequest<T>.ReadState next } => ReadLoop(getter: getter, scope: scope, receive: receive, value: value, state: next, transition: transition, terminalOptions: terminalOptions),
                { State: CommandInputRequest<T>.ReadState next } => Fin.Succ(value: CommandGet<T>.Of(getter: getter, raw: raw, value: value(getter, raw, next.Preselected), option: next.Selected)),
            },
        };

    private static CommandInputRequest<T>.ReadTransition Stay<T>(GetBaseClass getter, GetResult raw, CommandInputRequest<T>.ReadState state) =>
        new(State: state, Repeat: false);
}
