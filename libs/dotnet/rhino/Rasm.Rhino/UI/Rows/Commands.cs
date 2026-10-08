using Eto.Forms;
using Rasm.Rhino.Document.Files;
using Rasm.Rhino.Events;
using Rasm.Rhino.UI.Assets;

namespace Rasm.Rhino.UI.Rows;

// --- [MODELS] --------------------------------------------------------------------------
public sealed record CommandFace(string Name, string Caption, string Help, Option<IGlyph> Glyph, Option<Keys> Shortcut) {
    public Wording Wording { get; init; } = Wording.English;
}

public sealed record Realized<T>(T Commands, IO<Unit> Refresh, IDisposable Release);

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
            new Check(member(item), enabled, scope => read(scope).Map(item.Equals), (scope, on) => when(on, write(scope, item)).As())));

    // --- [REALIZE]
    public IO<Realized<Seq<Command>>> Commands(RowScope scope) =>
        Switch(
            scope,
            run: static (held, run) => run.Realize(held).Map(static done => new Realized<Seq<Command>>(Seq(done.Commands), done.Refresh, done.Release)),
            check: static (held, check) => check.Realize(held).Map(static done => new Realized<Seq<Command>>(Seq<Command>(done.Commands), done.Refresh, done.Release)),
            radio: static (held, radio) => radio.Realize(held).Map(static done =>
                new Realized<Seq<Command>>(done.Commands.Map(static member => (Command)member.Command), done.Refresh, done.Release)));

    public static (Seq<Seq<Command>> Groups, Realized<Seq<Command>> Joined) Menu(Seq<Seq<Realized<Seq<Command>>>> groups, CallbackSite site) =>
        groups.Bind(static group => group) switch {
            var realized => (groups.Map(static group => group.Bind(static held => held.Commands)),
                             new Realized<Seq<Command>>(
                                 realized.Bind(static held => held.Commands),
                                 realized.TraverseM(static held => held.Refresh).As().Map(static _ => unit),
                                 DisposalOps.Composite(realized.Map(static held => held.Release), site))),
        };

    private protected IO<Realized<TCommand>> Commanded<TCommand>(RowScope scope, TCommand command, HostEvent<EventArgs> raised, IO<Unit> deliver, IO<Unit> refresh)
        where TCommand : Command =>
        from faced in IO.lift(() => Face.Wording.Shown(Face.Caption, scope.Sink) switch {
            var caption => (command.ID, command.MenuText, command.ToolBarText, command.ToolTip, command.Shortcut) =
                (Face.Name, caption, caption, Face.Wording.Shown(Face.Help, scope.Sink), Face.Shortcut.IfNone(Keys.None)),
        })
        from held in DisposalOps.AcquireAll(
            Face.Glyph.Map(glyph => Icons.Themed(scope.Sink, glyph, IconSlot.MenuItem, image => command.Image = image)).ToSeq().Add(raised.Inline(_ => deliver, scope.Sink)),
            DisposalOps.Release)
        select new Realized<TCommand>(
            command,
            RowRules.Holds(Enabled, scope).Bind(on => IO.lift(() => command.Enabled = on)).Bind(_ => refresh),
            DisposalOps.Composite(held, new CallbackSite(scope.Sink, typeof(Command), Face.Name)));

    public sealed record Run(CommandFace Face, Option<RowRule> Enabled, Func<RowScope, IO<Unit>> Execute) : CommandRow(Face, Enabled) {
        public IO<Realized<Command>> Realize(RowScope scope) =>
            IO.lift(static () => new Command()).Bind(command => Commanded(
                scope, command, Subscriptions.Host<EventArgs>(typeof(Command), h => command.Executed += h, h => command.Executed -= h, Face.Name), Execute(scope), IO.pure(unit)));
    }

    public sealed record Check(CommandFace Face, Option<RowRule> Enabled, Func<RowScope, IO<bool>> Read, Func<RowScope, bool, IO<Unit>> Write) : CommandRow(Face, Enabled) {
        public IO<Realized<CheckCommand>> Realize(RowScope scope) => IO.lift(static () => new CheckCommand()).Bind(command => Realize(scope, command));

        internal IO<Realized<TCommand>> Realize<TCommand>(RowScope scope, TCommand command) where TCommand : CheckCommand =>
            Commanded(
                scope,
                command,
                Subscriptions.Host<EventArgs>(typeof(Command), h => command.CheckedChanged += h, h => command.CheckedChanged -= h, Face.Name),
                IO.lift(() => command.Checked).Bind(on => Write(scope, on)),
                Read(scope).Bind(on => IO.lift(() => command.Checked = on)));
    }

    public sealed record Radio(CommandFace Face, Option<RowRule> Enabled, Seq<Check> Members) : CommandRow(Face, Enabled) {
        public IO<Realized<Seq<(RadioCommand Command, CommandFace Face)>>> Realize(RowScope scope) =>
            from commands in IO.lift(() => Members.Fold(Seq<RadioCommand>(), static (made, _) => made.Add(new RadioCommand { Controller = made.Head.ValueUnsafe() })))
            from members in DisposalOps.AcquireAll(
                Members.Zip(commands).Map(pair => pair.First.Realize(scope, pair.Second)),
                static held => DisposalOps.Release(held.Map(static done => done.Release)))
            select new Realized<Seq<(RadioCommand Command, CommandFace Face)>>(
                Members.Zip(members).Map(static pair => (pair.Second.Commands, pair.First.Face)),
                members.TraverseM(static done => done.Refresh).As().Map(static _ => unit),
                DisposalOps.Composite(members.Map(static done => done.Release), new CallbackSite(scope.Sink, typeof(RadioCommand), Face.Name)));
    }
}
