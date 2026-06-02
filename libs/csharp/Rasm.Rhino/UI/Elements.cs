using Eto.Forms;

namespace Rasm.Rhino.UI;

// --- [MODELS] -----------------------------------------------------------------------------
public abstract record UiElement<TState> {
    private UiElement() { }

    internal abstract Fin<Control> Build(Atom<TState> state);

    public sealed record Native(Func<Atom<TState>, Fin<Control>> Create) : UiElement<TState> {
        internal override Fin<Control> Build(Atom<TState> state) =>
            Op.Of(name: nameof(Native)).Need(Create).Bind(create => create(arg: state));
    }

    public sealed record ControlCase(Control Value) : UiElement<TState> {
        internal override Fin<Control> Build(Atom<TState> state) =>
            Op.Of(name: nameof(ControlCase)).Need(Value);
    }

    public sealed record Field<T>(UiField<TState, T> Spec) : UiElement<TState> {
        internal override Fin<Control> Build(Atom<TState> state) =>
            Op.Of(name: nameof(Field<>)).Need(Spec).Bind(spec => spec.Build(state: state));
    }

    public sealed record Group(Orientation Orientation, Seq<UiElement<TState>> Children, int Spacing = 6) : UiElement<TState> {
        internal override Fin<Control> Build(Atom<TState> state) =>
            Children.TraverseM(child => child.Build(state: state)).As().Map(controls => {
                StackLayout layout = new() { Orientation = Orientation, Spacing = Spacing };
                foreach (Control control in controls) layout.Items.Add(control);
                return (Control)layout;
            });
    }

    public sealed record Dynamic(Seq<UiElement<TState>> Rows, int Spacing = 6) : UiElement<TState> {
        internal override Fin<Control> Build(Atom<TState> state) =>
            Rows.TraverseM(row => row.Build(state: state)).As().Map(rows => {
                DynamicLayout layout = new() { Spacing = new Eto.Drawing.Size(Spacing, Spacing) };
                foreach (Control row in rows) _ = layout.AddRow(row);
                return (Control)layout;
            });
    }

    public sealed record Table(Seq<Seq<UiElement<TState>>> Rows, int Spacing = 6) : UiElement<TState> {
        internal override Fin<Control> Build(Atom<TState> state) =>
            Rows.TraverseM(row => row.TraverseM(cell => cell.Build(state: state)).As()).As().Map(rows => {
                TableLayout layout = new() { Spacing = new Eto.Drawing.Size(Spacing, Spacing) };
                foreach (Seq<Control> row in rows) {
                    layout.Rows.Add(new TableRow([.. row.Map(static control => new TableCell(control, scaleWidth: true))]));
                }
                return (Control)layout;
            });
    }

    public sealed record Section(string Title, UiElement<TState> Body) : UiElement<TState> {
        internal override Fin<Control> Build(Atom<TState> state) =>
            Body.Build(state: state).Map(body => (Control)new GroupBox { Text = Title, Content = body });
    }

    public sealed record Canvas(Func<TState, Eto.Drawing.Size, Fin<UiHud>> Paint, UiRenderHint Hint = default) : UiElement<TState> {
        internal override Fin<Control> Build(Atom<TState> state) =>
            Op.Of(name: nameof(Canvas)).Need(Paint)
                .Map(paint => (Control)new UiCanvas<TState>(initial: state.Value, paint: paint, hint: Hint));
    }
}

public sealed record UiField<TState, T>(
    string Label,
    Func<TState, T> Get,
    Func<TState, T, TState> Set,
    Func<T, Control> Create,
    Func<Control, T> Read,
    Option<Func<T, Fin<Unit>>> Validate = default,
    Option<Func<Fin<Unit>>> Admit = default,
    bool CommitOnChange = true) {
    internal Fin<Control> Build(Atom<TState> state) =>
        from get in Op.Of(name: nameof(UiField<,>)).Need(Get)
        from set in Op.Of(name: nameof(UiField<,>)).Need(Set)
        from create in Op.Of(name: nameof(UiField<,>)).Need(Create)
        from read in Op.Of(name: nameof(UiField<,>)).Need(Read)
        from _admit in Admit.Map(run => run()).IfNone(Fin.Succ(value: unit))
        let editor = create(arg: get(arg: state.Value))
        from valid in Op.Of(name: nameof(UiField<,>)).Need(editor)
        select Wire(state: state, control: Row(editor: valid), editor: valid, read: read, set: set);

    private Control Row(Control editor) {
        if (string.IsNullOrWhiteSpace(value: Label)) return editor;
        StackLayout row = new() { Orientation = Orientation.Horizontal, Spacing = 6 };
        row.Items.Add(new Label { Text = Label, VerticalAlignment = VerticalAlignment.Center });
        row.Items.Add(editor);
        return row;
    }

    private Control Wire(Atom<TState> state, Control control, Control editor, Func<Control, T> read, Func<TState, T, TState> set) {
        void Commit() =>
            _ = RhinoUi.Protect(valid: () =>
                from value in Validate.Map(check => check(arg: read(arg: editor)).Map(_ => read(arg: editor))).IfNone(Fin.Succ(value: read(arg: editor)))
                select state.Swap(current => set(arg1: current, arg2: value))).IfFail(error => {
                    editor.ToolTip = error.Message;
                    return state.Value;
                });
        if (CommitOnChange) {
            switch (editor) {
                case TextBox box: box.TextChanged += (_, _) => Commit(); break;
                case TextArea area: area.TextChanged += (_, _) => Commit(); break;
                case CheckBox box: box.CheckedChanged += (_, _) => Commit(); break;
                case NumericStepper box: box.ValueChanged += (_, _) => Commit(); break;
                case DropDown drop: drop.SelectedIndexChanged += (_, _) => Commit(); break;
            }
        }
        editor.LostFocus += (_, _) => Commit();
        return control;
    }
}

public sealed record UiRequest<T> {
    private readonly Func<RhinoUi.Scope, Fin<T>> run;

    internal UiRequest(string name, Func<RhinoUi.Scope, Fin<T>> run, bool interactive) =>
        (Name, this.run, Interactive) = (name, run, interactive);

    public string Name { get; }
    public bool Interactive { get; }

    internal Fin<T> Run(RhinoUi.Scope scope) =>
        Op.Of(name: Name).Need(run).Bind(valid => valid(arg: scope));
}

public sealed record UiWindowSpec<TState, TResult>(
    string Title,
    TState Initial,
    UiElement<TState> Content,
    Func<TState, TResult> Result,
    UiWindowMode Mode = UiWindowMode.Modal,
    bool RestoreLocation = false,
    bool SavePosition = false,
    Option<Eto.Drawing.Size> ClientSize = default) {
    public UiRequest<TResult> Request(string name = nameof(UiWindowSpec<,>)) =>
        UiRequest.OfScope(
            name: name,
            run: scope =>
                from title in Op.Of(name: name).AcceptText(value: Title).MapFail(_ => Op.Of(name: name).InvalidInput())
                from body in Op.Of(name: name).Need(Content)
                from result in Op.Of(name: name).Need(Result)
                let state = Atom(Initial)
                from control in body.Build(state: state)
                from dialog in RhinoUi.Protect(valid: () => {
                    Dialog<TResult> window = new() { Title = title, Content = control };
                    _ = ClientSize.Iter(size => window.ClientSize = size);
                    global::Rhino.UI.EtoExtensions.UseRhinoStyle(window);
                    _ = Op.SideWhen(RestoreLocation, () => global::Rhino.UI.EtoExtensions.RestorePosition(window: window));
                    _ = Op.SideWhen(SavePosition, () => window.Closed += (_, _) => global::Rhino.UI.EtoExtensions.SavePosition(window: window));
                    Button ok = new() { Text = "OK" };
                    Button cancel = new() { Text = "Cancel" };
                    ok.Click += (_, _) => window.Close(result(arg: state.Value));
                    cancel.Click += (_, _) => window.Close();
                    window.PositiveButtons.Add(ok);
                    window.NegativeButtons.Add(cancel);
                    window.DefaultButton = ok;
                    window.AbortButton = cancel;
                    return Fin.Succ(value: window);
                })
                from shown in UiIntent.Window(dialog: dialog, mode: Mode).Run(scope: scope)
                select shown,
            interactive: true);

    public UiIntent<TResult> Intent(string name = nameof(UiWindowSpec<,>)) =>
        UiIntent.Request(request: Request(name: name));
}

// --- [OPERATIONS] -------------------------------------------------------------------------
public static class UiElement {
    public static UiElement<TState> Of<TState>(Control control) => new UiElement<TState>.ControlCase(Value: control);
    public static UiElement<TState> Of<TState>(Func<Atom<TState>, Fin<Control>> create) => new UiElement<TState>.Native(Create: create);
    public static UiElement<TState> Form<TState>(params UiElement<TState>[] rows) => new UiElement<TState>.Dynamic(Rows: toSeq(rows));
    public static UiElement<TState> Column<TState>(params UiElement<TState>[] children) => new UiElement<TState>.Group(Orientation: Orientation.Vertical, Children: toSeq(children));
    public static UiElement<TState> Row<TState>(params UiElement<TState>[] children) => new UiElement<TState>.Group(Orientation: Orientation.Horizontal, Children: toSeq(children));
}

public static class UiField {
    public static UiField<TState, string> Text<TState>(string label, Func<TState, string> get, Func<TState, string, TState> set, bool multiline = false) =>
        new(
            Label: label,
            Get: get,
            Set: set,
            Create: value => multiline ? new TextArea { Text = value } : new TextBox { Text = value },
            Read: static control => control switch {
                TextArea area => area.Text,
                TextBox box => box.Text,
                _ => string.Empty,
            });

    public static UiField<TState, bool> Toggle<TState>(string label, Func<TState, bool> get, Func<TState, bool, TState> set) =>
        new(
            Label: label,
            Get: get,
            Set: set,
            Create: value => new CheckBox { Checked = value },
            Read: static control => control is CheckBox box && box.Checked == true);

    public static UiField<TState, double> Number<TState>(string label, Func<TState, double> get, Func<TState, double, TState> set, double increment = 1.0) =>
        new(
            Label: label,
            Get: get,
            Set: set,
            Create: value => new NumericStepper { Value = value, Increment = increment },
            Read: static control => control is NumericStepper box ? box.Value : 0.0);

    public static UiField<TState, TChoice> Choice<TState, TChoice>(string label, Func<TState, TChoice> get, Func<TState, TChoice, TState> set, Seq<TChoice> choices) =>
        new(
            Label: label,
            Get: get,
            Set: set,
            Create: value => {
                DropDown drop = new() { DataStore = choices.Map(static item => (object?)item).AsIterable() };
                int index = -1;
                foreach ((TChoice item, int i) in choices.AsIterable().Select((item, i) => (item, i))) {
                    if (EqualityComparer<TChoice>.Default.Equals(x: item, y: value)) { index = i; break; }
                }
                drop.SelectedIndex = index < 0 ? 0 : index;
                return drop;
            },
            Read: control => control is DropDown { SelectedIndex: int index } && index >= 0 && index < choices.Count ? choices[index] : choices[0],
            Admit: Some(() => guard(!choices.IsEmpty, Op.Of(name: nameof(Choice)).InvalidInput()).ToFin()));
}

public static class UiRequest {
    internal static UiRequest<T> OfScope<T>(string name, Func<RhinoUi.Scope, Fin<T>> run, bool interactive = true) =>
        new(name: string.IsNullOrWhiteSpace(value: name) ? nameof(UiRequest<>) : name, run: run, interactive: interactive);

    public static UiRequest<T> Native<T>(string name, Func<RhinoDoc, RunMode, Fin<T>> run, bool interactive = true) =>
        OfScope(
            name: name,
            run: scope => Op.Of(name: name).Need(run).Bind(valid => valid(arg1: scope.Document, arg2: scope.Mode)),
            interactive: interactive);
}
