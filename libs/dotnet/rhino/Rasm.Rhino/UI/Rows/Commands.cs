using CommunityToolkit.Mvvm.Input;
using Eto.Forms;
using Rasm.Rhino.Document.Files;
using Rasm.Rhino.UI.Styling;

namespace Rasm.Rhino.UI.Rows;

// --- [MODELS] --------------------------------------------------------------------------
public sealed record CommandFace(string Name, string Caption, string Help, Option<IGlyph> Glyph, Option<Keys> Shortcut);

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
            from document in scope.DocumentIO
            from ran in Documents.RunScript(document, script, echo: false, display: Some(RowText.Localize(face.Caption, scope.Sink).Local))
            select ran);

    public static Radio Choices<TValue>(
        CommandFace face, Option<RowRule> enabled, Func<RowScope, IO<TValue>> read, Func<RowScope, TValue, IO<Unit>> write, Func<TValue, CommandFace> member)
        where TValue : class, ISmartEnum<string, TValue, ValidationError> =>
        new(face, enabled, toSeq(TValue.Items).Map(item =>
            new Check(member(item), None, scope => read(scope).Map(item.Equals), (scope, on) => when(on, write(scope, item)).As())));

    // --- [REALIZE]
    public IO<Realized<Seq<(Command Command, CommandFace Face)>>> Commands(RowScope scope, Action<IDisposable> retain) =>
        Switch(
            (Scope: scope, Retain: retain),
            run: static (held, run) => run.Realize(held.Scope, held.Retain).Map(done =>
                new Realized<Seq<(Command Command, CommandFace Face)>>(Seq<(Command Command, CommandFace Face)>((done.Commands, run.Face)), done.Refresh)),
            check: static (held, check) => check.Realize(held.Scope, held.Retain).Map(done =>
                new Realized<Seq<(Command Command, CommandFace Face)>>(Seq<(Command Command, CommandFace Face)>((done.Commands, check.Face)), done.Refresh)),
            radio: static (held, radio) => radio.Realize(held.Scope, held.Retain).Map(static done =>
                new Realized<Seq<(Command Command, CommandFace Face)>>(done.Commands.Map<(Command Command, CommandFace Face)>(static member => (member.Command, member.Face)), done.Refresh)));

    public static (Seq<Seq<Command>> Groups, IO<Unit> Refresh) Menu(Seq<Seq<Realized<Seq<(Command Command, CommandFace Face)>>>> groups) =>
        (groups.Map(static @group => @group.Bind(static held => held.Commands).Map(static member => member.Command)),
         Callbacks.Each(groups.Bind(static @group => @group).Map(static held => held.Refresh)).Map(unit));

    private protected IO<Realized<TCommand>> Commanded<TCommand>(RowScope scope, TCommand command, Func<IO<Unit>> deliver, IO<Unit> refresh, Action<IDisposable> retain)
        where TCommand : Command =>
        from faced in IO.lift(() => RowText.Localize(Face.Caption, scope.Sink).Local switch {
            var caption => (command.ID, command.MenuText, command.ToolBarText, command.ToolTip, command.Shortcut) =
                (Face.Name, caption, caption, RowText.Localize(Face.Help, scope.Sink).Local, Face.Shortcut.IfNone(Keys.None)),
        })
        from themed in Face.Glyph.Traverse(glyph => Icons.Themed(scope.Sink, glyph, IconSlot.Small, HostTheme.Changed, image => command.Image = image, retain)).As()
        from parent in cancelToken
        from lifetime in DisposalOps.Acquire(IO.lift(() => CancellationTokenSource.CreateLinkedTokenSource(parent)),
            static source => Callbacks.Each(Seq(IO.lift(source.Cancel), IO.lift(source.Dispose))).Map(unit), retain)
        from attached in DisposalOps.Acquire(IO.lift(() => command.DelegatedCommand = new AsyncRelayCommand(async () => Callbacks.Run(
                await DisposalOps.Within(
                    () => EnvIO.New(token: lifetime.Token),
                    environment => IO.liftVAsync(() => IO.lift(deliver).Bind(static effect => effect).RunAsync(environment)),
                    static environment => Callbacks.Each(Seq(IO.lift(environment.DisposeResources), IO.lift(environment.DisposeNonResources)))
                        .Map(unit)).RunSafeAsync().ConfigureAwait(continueOnCapturedContext: true),
                new(scope.Sink, typeof(Command), Face.Name)), AsyncRelayCommandOptions.AllowConcurrentExecutions)),
            _ => IO.lift(() => { command.DelegatedCommand = null; }), retain)
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
            Enabled.Map(parent => member.Enabled.Map(own => parent & own).IfNone(parent)) | member.Enabled,
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
