using CommunityToolkit.Mvvm.Input;
using Eto.Forms;
using Rasm.Rhino.Document.Files;
using Rasm.Rhino.UI.Assets;

namespace Rasm.Rhino.UI.Rows;

// --- [MODELS] --------------------------------------------------------------------------
public sealed record CommandFace(string Name, string Caption, string Help, Option<(IGlyph Glyph, int Master)> Glyph, Option<Keys> Shortcut) {
    public Wording Wording { get; init; } = Wording.English;
}

public sealed record Realized<T>(T Commands, IO<Unit> Refresh);

[Union]
public abstract partial record CommandRow {
    // --- [FACE]
    private CommandRow(CommandFace face, Option<RowRule> enabled) => (Face, Enabled) = (face, enabled);

    public CommandFace Face { get; }
    public Option<RowRule> Enabled { get; }

    // --- [DECLARE]
    public static Run Macro(CommandFace face, string script, Option<RowRule> enabled) =>
        new(face, enabled, scope =>
            from document in IO.lift(scope.Document.ToFin(new Missing(nameof(RowScope.Document))))
            from ran in Documents.RunScript(document, script, echo: false, display: Some(face.Wording.Shown(face.Caption, scope.Sink)))
            select ran);

    public static Radio Choices<TValue, TError>(
        CommandFace face, Option<RowRule> enabled, Func<RowScope, IO<TValue>> read, Func<RowScope, TValue, IO<Unit>> write, Func<TValue, CommandFace> member)
        where TValue : class, ISmartEnum<string, TValue, TError>
        where TError : Error, IValidationError<TError> =>
        new(face, enabled, toSeq(TValue.Items).Map(item =>
            new Check(member(item), None, scope => read(scope).Map(item.Equals), (scope, on) => when(on, write(scope, item)).As())));

    // --- [REALIZE]
    public IO<Realized<Seq<Command>>> Commands(RowScope scope, Action<IDisposable> retain) =>
        Switch(
            (Scope: scope, Retain: retain),
            run: static (held, run) => run.Realize(held.Scope, held.Retain).Map(static done => new Realized<Seq<Command>>(Seq(done.Commands), done.Refresh)),
            check: static (held, check) => check.Realize(held.Scope, held.Retain).Map(static done => new Realized<Seq<Command>>(Seq<Command>(done.Commands), done.Refresh)),
            radio: static (held, radio) => radio.Realize(held.Scope, held.Retain).Map(static done =>
                new Realized<Seq<Command>>(done.Commands.Map(static member => (Command)member.Command), done.Refresh)));

    public static (Seq<Seq<Command>> Groups, Realized<Seq<Command>> Joined) Menu(Seq<Seq<Realized<Seq<Command>>>> groups) =>
        groups.Bind(static @group => @group) switch {
            var realized => (groups.Map(static @group => @group.Bind(static held => held.Commands)),
                             new Realized<Seq<Command>>(
                                 realized.Bind(static held => held.Commands),
                                 Callbacks.Each(realized.Map(static held => held.Refresh)).Map(unit))),
        };

    private protected IO<Realized<TCommand>> Commanded<TCommand>(RowScope scope, TCommand command, Func<IO<Unit>> deliver, IO<Unit> refresh, Action<IDisposable> retain)
        where TCommand : Command =>
        from faced in IO.lift(() => Face.Wording.Shown(Face.Caption, scope.Sink) switch {
            var caption => (command.ID, command.MenuText, command.ToolBarText, command.ToolTip, command.Shortcut) =
                (Face.Name, caption, caption, Face.Wording.Shown(Face.Help, scope.Sink), Face.Shortcut.IfNone(Keys.None)),
        })
        from themed in Face.Glyph.Traverse(glyph => Icons.Themed(scope.Sink, glyph, IconSlot.MenuItem, HostTheme.Changed, image => command.Image = image, retain)).As()
        from parent in cancelToken
        from lifetime in DisposalOps.Acquire(IO.lift(() => {
            CancellationTokenSource source = CancellationTokenSource.CreateLinkedTokenSource(parent);
            return (source.Token, Release: new Disposal<CancellationTokenSource>(source, static held =>
                Callbacks.Each(Seq(IO.lift(held.Cancel), IO.lift(held.Dispose))).Run()));
        }), static held => IO.lift(held.Release.Dispose), held => retain(held.Release))
        from attached in DisposalOps.Acquire(
            IO.lift(() => new Disposal<TCommand>(command, static held => held.DelegatedCommand = null)),
            static lease => IO.lift(lease.Dispose), lease => {
                command.DelegatedCommand = new AsyncRelayCommand(async () => Callbacks.Run(
                    await DisposalOps.Within(
                        () => EnvIO.New(token: lifetime.Token),
                        environment => IO.liftVAsync(() => IO.lift(deliver).Bind(static effect => effect).RunAsync(environment)),
                        static environment => Callbacks.Each(Seq(IO.lift(environment.DisposeResources), IO.lift(environment.DisposeNonResources)))
                            .Map(unit)).RunSafeAsync().ConfigureAwait(continueOnCapturedContext: true),
                    new(scope.Sink, typeof(Command), Face.Name)), AsyncRelayCommandOptions.AllowConcurrentExecutions);
                retain(lease);
            })
        select new Realized<TCommand>(
            command,
            Callbacks.Each(Seq(RowRules.Holds(Enabled, scope).Bind(enabled => IO.lift(() => { command.Enabled = enabled; })), refresh)).Map(unit));

    public sealed record Run(CommandFace Face, Option<RowRule> Enabled, Func<RowScope, IO<Unit>> Execute) : CommandRow(Face, Enabled) {
        public IO<Realized<Command>> Realize(RowScope scope, Action<IDisposable> retain) =>
            IO.lift(static () => new Command()).Bind(command => Commanded(
                scope, command, () => Execute(scope), IO.pure(unit), retain));
    }

    public sealed record Check(CommandFace Face, Option<RowRule> Enabled, Func<RowScope, IO<bool>> Read, Func<RowScope, bool, IO<Unit>> Write) : CommandRow(Face, Enabled) {
        public IO<Realized<CheckCommand>> Realize(RowScope scope, Action<IDisposable> retain) => IO.lift(static () => new CheckCommand()).Bind(command => Realize(scope, command, retain));

        internal IO<Realized<TCommand>> Realize<TCommand>(RowScope scope, TCommand command, Action<IDisposable> retain) where TCommand : CheckCommand =>
            Commanded(
                scope,
                command,
                () => IO.pure(command.Checked).Bind(on => Write(scope, on)),
                Read(scope).Bind(on => IO.lift(() => { command.Checked = on; })), retain);
    }

    public sealed record Radio : CommandRow {
        public Radio(CommandFace face, Option<RowRule> enabled, Seq<Check> members) : base(face, enabled) => Members = members;

        public Seq<Check> Members => field.Map(member => new Check(
            member.Face,
            Enabled.Match(Some: parent => Some(member.Enabled.Match(Some: own => parent & own, None: () => parent)), None: () => member.Enabled),
            member.Read, member.Write));

        public IO<Realized<Seq<(RadioCommand Command, CommandFace Face)>>> Realize(RowScope scope, Action<IDisposable> retain) =>
            Members.FoldM(Seq<(RadioCommand Command, CommandFace Face, IO<Unit> Refresh)>(), (made, row) =>
                from command in IO.lift(() => made.Head.Match(
                    Some: static first => new RadioCommand { Controller = first.Command }, None: static () => new RadioCommand()))
                from member in row.Realize(scope, command, retain)
                select made.Add((member.Commands, row.Face, member.Refresh))).As()
                .Map(static members => new Realized<Seq<(RadioCommand Command, CommandFace Face)>>(
                    members.Map(static member => (member.Command, member.Face)),
                    Callbacks.Each(members.Map(static member => member.Refresh)).Map(unit)));
    }
}
