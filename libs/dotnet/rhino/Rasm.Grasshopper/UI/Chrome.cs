using Eto.Drawing;
using Eto.Forms;
using Grasshopper2.Doc;
using Grasshopper2.Extensions;
using Grasshopper2.UI;
using Grasshopper2.UI.Canvas;
using Grasshopper2.UI.Flex;
using Rasm.Rhino;
using Rasm.Rhino.Commands;
using Rasm.Rhino.Persistence;

namespace Rasm.Grasshopper.UI;

// --- [OPERATIONS] ----------------------------------------------------------------------

public static class Chrome {

    // --- [COMMANDS]

    public static Command Run(Nomen face, Func<Canvas, IO<Unit>> run, Keys shortcut = Keys.None, Predicate<Canvas>? enabled = null) =>
        fun((Canvas canvas) => canvas is not null && (enabled?.Invoke(canvas) ?? true)) switch {
            var ready => new Command {
                MenuText = face.Name,
                ToolTip = face.Info,
                Shortcut = shortcut,
                DelegatedCommand = new RelayCommand<Canvas>(
                    canvas => {
                        if (ready(canvas)) Shown(face, run(canvas));
                    },
                    canvas => ready(canvas)),
            },
        };

    public static CheckCommand Toggle(Nomen face, Preference<bool> state) =>
        new((sender, _) => Shown(face,
            from active in state.Read
            from written in state.Write(!active)
            from shown in IO.lift(() => Optional(sender as CheckCommand).Iter(toggle => toggle.Checked = !active))
            select unit)) {
            MenuText = face.Name,
            ToolTip = face.Info,
            Tag = state,
        };

    private static Unit Shown(Nomen face, IO<Unit> effect) =>
        Session.Run(
            effect | @catch(
                static error => !(error.Count is 1 && error.Is(Errors.Cancelled)),
                error => IO.lift(() => (error.IsExceptional
                    ? new Grasshopper2.UI.ErrorFeedback.Error("Unhandled Error", error.ToException()) { Origin = face.Name }
                    : new Grasshopper2.UI.ErrorFeedback.Error("Command Failed") { Origin = face.Name, Message = error.Message }).ShowModal()) >> IO.fail<Unit>(error)),
            unit,
            face.Name);

    private static bool Allowed(Command command, FlexControl control) =>
        Optional(command.DelegatedCommand).Match(Some: delegated => delegated.CanExecute(control), None: () => command.Enabled);

    private static Unit Execute(Command command, FlexControl control) {
        command.CommandParameter = control;
        if (Allowed(command, control)) command.Execute();
        return unit;
    }

    private static bool Refreshed(CheckCommand toggle) =>
        toggle.Tag is Preference<bool> state ? (toggle.Checked = Session.Run(state.Read, toggle.Checked)) : toggle.Checked;

    private static Unit Opened(Seq<Command> commands, bool open) =>
        commands.Iter(command => {
            if (command is CheckCommand toggle) Refreshed(toggle);
            else if (command.DelegatedCommand is RelayCommand<Canvas> relay && open) relay.UpdateCanExecute();
            else if (command.DelegatedCommand is RelayCommand<Canvas>) command.Enabled = true;
        });

    // --- [MENUS]

    public static IO<Unit> Editor(Func<Editor, IO<Unit>> attach) =>
        from scope in envIO
        from present in IO.lift(static () => Optional(Grasshopper2.UI.Editor.Instance))
        from attached in present.Match(Some: attach, None: () => Built(scope, attach))
        select attached;

    private static IO<Unit> Built(EnvIO scope, Func<Editor, IO<Unit>> attach) =>
        from waiting in use(() => EnvIO.New(token: scope.Token))
        from subscribed in IO.liftVAsync(() => ActiveCommands.Changed
            .Attach(_ => IO.lift(static () => Optional(Grasshopper2.UI.Editor.Instance))
                .Bind(built => built.Traverse(editor => IO.lift(waiting.Dispose) >> IO.liftVAsync(() => attach(editor).RunAsync(scope))).As())
                .Map(unit)).RunAsync(waiting))
        select subscribed;

    public static IO<Unit> Menu(Seq<Command> commands) =>
        unless(commands.IsEmpty, Editor(editor =>
            from host in IO.lift(() => (EditorMainMenu)editor.Menu)
            from menu in use(IO.lift(() => {
                SubMenuItem submenu = new() { Text = Session.PlugIn.Name };
                commands.Iter(command => {
                    command.CommandParameter = editor.Canvas;
                    submenu.Items.Add(command.CreateMenuItem());
                });
                host.Items.Insert(host.Items.IndexOf(host.Help), submenu);
                return submenu;
            }), attached => IO.lift(() => ignore(host.Items.Remove(attached))))
            from opening in Subscriptions.Inline<EventArgs>(h => menu.Opening += h, h => menu.Opening -= h).Attach(_ => IO.lift(() => Opened(commands, open: true)))
            from closing in Subscriptions.Inline<EventArgs>(h => menu.Closing += h, h => menu.Closing -= h).Attach(_ => IO.lift(() => Opened(commands, open: false)))
            from closed in IO.lift(() => Opened(commands, open: false))
            select unit)).As();

    public static IO<Unit> Context(Canvas canvas, Pick kind, Func<SelectionResult, Seq<Command>> rows) =>
        Subscriptions.Inline<PopulateContextMenuEventArgs>(h => canvas.PopulateContextMenu += h, h => canvas.PopulateContextMenu -= h).Attach(args => IO.lift(() => {
            SelectionResult pick = canvas.ResolvePick(canvas.Map(args.MouseEvent.Location, CoordinateSystem.Control, CoordinateSystem.Content), includeGrips: false, includeForeground: true, includeBackground: true, includeWires: true, recursive: true);
            Seq<Command> commands = pick.Kind == kind && !(canvas.AllowedActions.AllowObjectMenu && Optional(canvas.Document).Exists(document => document.Objects.Find(pick.ObjectUnderPick) is not null))
                ? rows(pick)
                : Seq<Command>();
            commands.Head.Iter(_ => args.Menu.AddSeparator());
            Opened(commands, open: true);
            commands.Iter(command => {
                command.CommandParameter = canvas;
                args.Menu.Items.Add(command.CreateMenuItem());
            });
        }));

    public static IO<Unit> Floating(FlexControl control, FloatingPosition position, Seq<Command> commands) =>
        commands.TraverseM(command => use(IO.lift(() => {
            Memo<Color> rest = memo(() => control.FloatingButtons[command.MenuText].Colour);
            control.FloatingButtons.Add(position, command.MenuText, command.ToolTip, click: (button, _) => Clicked(button));
            _ = rest.Value;
            Shaded(control.FloatingButtons[command.MenuText]);
            return command.MenuText;

            Unit Shaded(FloatingButton button) => Optional(command as CheckCommand).Iter(toggle => button.ModifyColour(Refreshed(toggle) ? SystemColors.Highlight : rest.Value));

            Response Clicked(FloatingButton button) {
                Execute(command, control);
                Shaded(button);
                return Response.Handled;
            }
        }), name => IO.lift(() => control.FloatingButtons.Close(name)))).As().Map(unit);

    public static IO<Unit> Shortcuts(Canvas canvas, Seq<Command> commands) =>
        Subscriptions.Inline<KeyEventArgs>(h => canvas.KeyDown += h, h => canvas.KeyDown -= h).Attach(args => IO.lift(() =>
            commands.Find(command => !args.Handled && command.Shortcut is not Keys.None && command.Shortcut == args.KeyData && Allowed(command, canvas)).Iter(command => {
                Execute(command, canvas);
                args.Handled = true;
            })));
}
