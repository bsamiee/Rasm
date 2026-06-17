using Eto.Forms;

namespace Rasm.Rhino.UI;

// --- [TYPES] ------------------------------------------------------------------------------
[Union]
public abstract partial record UiGridColumn<TRow> {
    public sealed record Text(string Header, Func<TRow, string> Value, int Width = 120) : UiGridColumn<TRow>;
    public sealed record Check(string Header, Func<TRow, bool> Value, int Width = 40) : UiGridColumn<TRow>;
    public sealed record Number(string Header, Func<TRow, double> Value, string Format = "G", int Width = 80) : UiGridColumn<TRow>;
    public sealed record Custom(string Header, Func<TRow, Action<Eto.Drawing.Graphics, Eto.Drawing.RectangleF>> Paint, int Width = 120) : UiGridColumn<TRow>;
    public sealed record Children(Func<TRow, Seq<TRow>> Get) : UiGridColumn<TRow>;

    private UiGridColumn() { }

    internal bool IsTree => this is Children;
    internal Option<GridColumn> ToColumn(int index) => Switch(   // BOUNDARY ADAPTER — Eto cell wiring is structural OOP; columns bind by index against GridItem.Values
        state: index,
        text: static (i, t) => Some(new GridColumn { HeaderText = t.Header, Width = t.Width, DataCell = new TextBoxCell(i) }),
        check: static (i, c) => Some(new GridColumn { HeaderText = c.Header, Width = c.Width, DataCell = new CheckBoxCell(i) }),
        number: static (i, n) => Some(new GridColumn { HeaderText = n.Header, Width = n.Width, DataCell = new TextBoxCell(i) }),
        custom: static (_, cc) => {
            CustomCell cell = new();
            cell.Paint += (_, e) => _ = e.Item is GridItem { Tag: TRow row } ? Op.Side(() => cc.Paint(arg: row)(arg1: e.Graphics, arg2: e.ClipRectangle)) : unit;
            return Some(new GridColumn { HeaderText = cc.Header, Width = cc.Width, DataCell = cell });
        },
        children: static (_, _) => Option<GridColumn>.None);
    internal object? CellValue(TRow row) => Switch(   // GridItem.Values is index-addressed; project each visible column to its display value
        state: row,
        text: static (r, t) => (object?)t.Value(arg: r),
        check: static (r, c) => c.Value(arg: r),
        number: static (r, n) => n.Value(arg: r).ToString(n.Format, System.Globalization.CultureInfo.InvariantCulture),
        custom: static (_, _) => null,
        children: static (_, _) => null);
}

public abstract record UiElement<TState> {
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

    public sealed record Absolute(Seq<(UiElement<TState> Child, Eto.Drawing.Point At)> Children) : UiElement<TState> {
        internal override Fin<Control> Build(Atom<TState> state) =>
            Children.TraverseM(placed => placed.Child.Build(state: state).Map(control => (Control: control, placed.At))).As().Map(placed => (Control)((Func<PixelLayout>)(() => {   // BOUNDARY ADAPTER — Eto PixelLayout absolute placement is structural OOP
                PixelLayout layout = new();
                _ = placed.Iter(item => layout.Add(item.Control, item.At));
                return layout;
            }))());
    }

    public sealed record Section(string Title, UiElement<TState> Body) : UiElement<TState> {
        internal override Fin<Control> Build(Atom<TState> state) =>
            Body.Build(state: state).Map(body => (Control)new GroupBox { Text = Title, Content = body });
    }

    public sealed record Canvas(Func<TState, Eto.Drawing.Size, Fin<UiHud>> Paint, UiRenderHint Hint = default) : UiElement<TState> {
        internal override Fin<Control> Build(Atom<TState> state) =>
            Op.Of(name: nameof(Canvas)).Need(Paint)
                .Map(paint => (Control)new UiCanvas<TState>(state: state, paint: paint, hint: Hint));   // share the window atom so sibling field commits repaint the canvas
    }

    public sealed record Split(Orientation Orientation, UiElement<TState> Primary, UiElement<TState> Secondary, int Position = 200, SplitterFixedPanel Fixed = SplitterFixedPanel.None, Option<Func<TState, int, TState>> OnPosition = default) : UiElement<TState> {
        internal override Fin<Control> Build(Atom<TState> state) =>
            from primary in Primary.Build(state: state)
            from secondary in Secondary.Build(state: state)
            select ((Func<Control>)(() => {   // BOUNDARY ADAPTER — Eto Splitter construction + event subscription is structural OOP
                Splitter splitter = new() { Orientation = Orientation, Panel1 = primary, Panel2 = secondary, Position = Position, FixedPanel = Fixed };
                _ = OnPosition.Iter(sink => splitter.PositionChanged += (_, _) => _ = state.Swap(current => sink(arg1: current, arg2: splitter.Position)));
                return splitter;
            }))();
    }

    public sealed record Collapse(string Header, UiElement<TState> Body, Func<TState, bool> Expanded, Func<TState, bool, TState> Toggle) : UiElement<TState> {
        internal override Fin<Control> Build(Atom<TState> state) =>
            Body.Build(state: state).Map(body => (Control)((Func<Expander>)(() => {   // BOUNDARY ADAPTER — Eto Expander construction + event subscription is structural OOP
                Expander expander = new() { Header = new Label { Text = Header }, Content = body, Expanded = Expanded(arg: state.Value) };
                expander.ExpandedChanged += (_, _) => _ = state.Swap(current => Toggle(arg1: current, arg2: expander.Expanded));
                return expander;
            }))());
    }

    public sealed record Tabs(Seq<(string Title, UiElement<TState> Body)> Pages, Func<TState, int> Selected, Func<TState, int, TState> OnSelect) : UiElement<TState> {
        internal override Fin<Control> Build(Atom<TState> state) =>
            Pages.TraverseM(page => page.Body.Build(state: state).Map(control => new DocumentPage(control) { Text = page.Title })).As().Map(pages => (Control)((Func<DocumentControl>)(() => {   // BOUNDARY ADAPTER — Eto DocumentControl construction + two-way SelectedIndex binding is structural OOP
                DocumentControl tabs = new();
                _ = pages.Iter(tabs.Pages.Add);
                tabs.SelectedIndex = Math.Clamp(value: Selected(arg: state.Value), min: 0, max: Math.Max(val1: 0, val2: pages.Count - 1));
                tabs.SelectedIndexChanged += (_, _) => _ = state.Swap(current => OnSelect(arg1: current, arg2: tabs.SelectedIndex));
                return tabs;
            }))());
    }

    public sealed record Browser(Func<TState, Option<Uri>> Url, Option<Func<TState, string>> Html = default, Option<Func<TState, Uri, TState>> OnLoaded = default) : UiElement<TState> {
        internal override Fin<Control> Build(Atom<TState> state) =>
            Op.Of(name: nameof(Browser)).Need(Url).Map(_url => (Control)((Func<WebView>)(() => {   // BOUNDARY ADAPTER — Eto WebView construction + DocumentLoaded projection is structural OOP
                WebView view = new();
                _ = Url(arg: state.Value).Case switch { Uri address => Op.Side(() => view.Url = address), _ => Html.Iter(html => view.LoadHtml(html(arg: state.Value))) };
                _ = OnLoaded.Iter(sink => view.DocumentLoaded += (_, args) => _ = Optional(args.Uri).Iter(address => _ = state.Swap(current => sink(arg1: current, arg2: address))));
                return view;
            }))());
    }

    public sealed record Grid<TRow>(Seq<UiGridColumn<TRow>> Columns, Func<TState, Seq<TRow>> GetRows, Option<Func<TState, Seq<TRow>, TState>> OnSelection = default) : UiElement<TState> {
        internal override Fin<Control> Build(Atom<TState> state) =>
            Op.Of(name: nameof(Grid<>)).Catch(() => {   // BOUNDARY ADAPTER — Eto Grid/TreeGrid construction binds visible columns by index against GridItem.Values + Tag carries the row
                Seq<UiGridColumn<TRow>> visible = Columns.Filter(static column => !column.IsTree);
                object?[] Values(TRow row) => [.. visible.Map(column => column.CellValue(row: row))];
                Unit Configure(Grid grid) {
                    _ = visible.Map(static (column, index) => column.ToColumn(index: index)).Iter(eto => _ = eto.Iter(grid.Columns.Add));
                    return OnSelection.Iter(sink => grid.SelectionChanged += (_, _) => _ = state.Swap(current => sink(arg1: current, arg2: toSeq(grid.SelectedItems).Choose(static row => Optional((row as GridItem)?.Tag).Bind(static tag => tag is TRow value ? Some(value) : Option<TRow>.None)))));
                }
                return Columns.Find(static column => column.IsTree).Bind(static column => column is UiGridColumn<TRow>.Children tree ? Some(tree) : Option<UiGridColumn<TRow>.Children>.None).Case switch {
                    UiGridColumn<TRow>.Children tree => Fin.Succ<Control>(value: ((Func<TreeGridView>)(() => {
                        TreeGridItem Node(TRow row) => new(children: tree.Get(arg: row).Map(Node).Cast<ITreeGridItem>(), values: Values(row: row)) { Tag = row };
                        TreeGridView grid = new() { DataStore = new TreeGridItemCollection(items: GetRows(arg: state.Value).Map(Node).Cast<ITreeGridItem>()) };
                        _ = Configure(grid: grid);
                        return grid;
                    }))()),
                    _ => Fin.Succ<Control>(value: ((Func<GridView>)(() => {
                        GridView grid = new() { DataStore = GetRows(arg: state.Value).Map(row => (object)new GridItem(values: Values(row: row)) { Tag = row }).AsIterable() };
                        _ = Configure(grid: grid);
                        return grid;
                    }))()),
                };
            });
    }

    private UiElement() { }

    internal abstract Fin<Control> Build(Atom<TState> state);
}

public static class UiElement {
    public static UiElement<TState> Of<TState>(Control control) => new UiElement<TState>.ControlCase(Value: control);
    public static UiElement<TState> Of<TState>(Func<Atom<TState>, Fin<Control>> create) => new UiElement<TState>.Native(Create: create);
    public static UiElement<TState> Form<TState>(params ReadOnlySpan<UiElement<TState>> rows) => new UiElement<TState>.Dynamic(Rows: toSeq(rows.ToArray()));
    public static UiElement<TState> Column<TState>(params ReadOnlySpan<UiElement<TState>> children) => new UiElement<TState>.Group(Orientation: Orientation.Vertical, Children: toSeq(children.ToArray()));
    public static UiElement<TState> Row<TState>(params ReadOnlySpan<UiElement<TState>> children) => new UiElement<TState>.Group(Orientation: Orientation.Horizontal, Children: toSeq(children.ToArray()));
    public static UiElement<TState> Absolute<TState>(params ReadOnlySpan<(UiElement<TState> Child, Eto.Drawing.Point At)> children) =>
        new UiElement<TState>.Absolute(Children: toSeq(children.ToArray()));
    public static UiElement<TState> Split<TState>(Orientation orientation, UiElement<TState> primary, UiElement<TState> secondary, int position = 200, SplitterFixedPanel fixedPanel = SplitterFixedPanel.None, Option<Func<TState, int, TState>> onPosition = default) =>
        new UiElement<TState>.Split(Orientation: orientation, Primary: primary, Secondary: secondary, Position: position, Fixed: fixedPanel, OnPosition: onPosition);
    public static UiElement<TState> Collapse<TState>(string header, UiElement<TState> body, Func<TState, bool> expanded, Func<TState, bool, TState> toggle) =>
        new UiElement<TState>.Collapse(Header: header, Body: body, Expanded: expanded, Toggle: toggle);
    public static UiElement<TState> Tabs<TState>(Func<TState, int> selected, Func<TState, int, TState> onSelect, params ReadOnlySpan<(string Title, UiElement<TState> Body)> pages) =>
        new UiElement<TState>.Tabs(Pages: toSeq(pages.ToArray()), Selected: selected, OnSelect: onSelect);
    public static UiElement<TState> Browser<TState>(Func<TState, Option<Uri>> url, Option<Func<TState, string>> html = default, Option<Func<TState, Uri, TState>> onLoaded = default) =>
        new UiElement<TState>.Browser(Url: url, Html: html, OnLoaded: onLoaded);
    public static UiElement<TState> Grid<TState, TRow>(Seq<UiGridColumn<TRow>> columns, Func<TState, Seq<TRow>> rows, Option<Func<TState, Seq<TRow>, TState>> onSelection = default) =>
        new UiElement<TState>.Grid<TRow>(Columns: columns, GetRows: rows, OnSelection: onSelection);
}

// --- [MODELS] -----------------------------------------------------------------------------
public sealed record UiField<TState, T>(
    string Label,
    Func<TState, T> Get,
    Func<TState, T, TState> Set,
    Func<T, Control> Create,
    Func<Control, T> Read,
    Option<Func<T, Fin<Unit>>> Validate = default,
    Option<Func<Fin<Unit>>> Admit = default,
    Option<Func<T, Fin<T>>> Activate = default,   // modal editors (color picker) open on click, write the editor, then commit through the shared LostFocus rail
    bool CommitOnChange = true) {
    internal Fin<Control> Build(Atom<TState> state) {
        Control MakeRow(Control editor) =>
            string.IsNullOrWhiteSpace(value: Label)
                ? editor
                : new StackLayout {
                    Orientation = Orientation.Horizontal, Spacing = 6,
                    Items = { new Label { Text = Label, VerticalAlignment = VerticalAlignment.Center }, editor }
                };

        Control WireEvents(Control control, Control editor) {
            void Commit() =>
                _ = RhinoUi.Protect(valid: () => {
                    T raw = Read(arg: editor);   // single capture
                    return Validate
                        .Map(check => check(arg: raw).Map(_ => raw))
                        .IfNone(Fin.Succ(value: raw))
                        .Map(value => state.Swap(current => Set(arg1: current, arg2: value)));
                }).IfFail(error => { editor.ToolTip = error.Message; return state.Value; });
            _ = CommitOnChange ? (editor switch {   // BOUNDARY ADAPTER — Eto event subscription is structural OOP, not domain dispatch
                TextBox box => Op.Side(() => box.TextChanged += (_, _) => Commit()),
                TextArea area => Op.Side(() => area.TextChanged += (_, _) => Commit()),
                CheckBox box => Op.Side(() => box.CheckedChanged += (_, _) => Commit()),
                NumericStepper s => Op.Side(() => s.ValueChanged += (_, _) => Commit()),
                DropDown drop => Op.Side(() => drop.SelectedIndexChanged += (_, _) => Commit()),
                _ => unit,
            }) : unit;
            _ = Activate.Iter(open => editor.MouseUp += (_, _) =>   // BOUNDARY ADAPTER — modal editor opens on click; write the editor tag, then route through Commit
                _ = RhinoUi.Protect(valid: () => open(arg: Read(arg: editor)).Map(value => { editor.Tag = value; editor.Invalidate(); Commit(); return unit; })));
            editor.LostFocus += (_, _) => Commit();
            return control;
        }

        return
            from get in Op.Of(name: nameof(UiField<,>)).Need(Get)
            from set in Op.Of(name: nameof(UiField<,>)).Need(Set)
            from create in Op.Of(name: nameof(UiField<,>)).Need(Create)
            from read in Op.Of(name: nameof(UiField<,>)).Need(Read)
            from _admit in Admit.Map(run => run()).IfNone(Fin.Succ(value: unit))
            let editor = create(arg: get(arg: state.Value))
            from valid in Op.Of(name: nameof(UiField<,>)).Need(editor)
            select WireEvents(control: MakeRow(editor: valid), editor: valid);
    }
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
            Create: value => new DropDown {
                DataStore = choices.Map(static item => (object?)item).AsIterable(),
                SelectedIndex = choices
                    .Choose((i, item) => EqualityComparer<TChoice>.Default.Equals(x: item, y: value) ? Some(i) : Option<int>.None)
                    .Head
                    .IfNone(noneValue: 0),
            },
            Read: control => control is DropDown { SelectedIndex: int index } && index >= 0 && index < choices.Count ? choices[index] : choices[0],
            Admit: Some(() => guard(!choices.IsEmpty, Op.Of().InvalidInput()).ToFin()));

    public static UiField<TState, Color4f> Color<TState>(string label, Func<TState, Color4f> get, Func<TState, Color4f, TState> set, bool allowAlpha = false, Option<global::Rhino.UI.NamedColorList> named = default) =>
        new(
            Label: label,
            Get: get,
            Set: set,
            CommitOnChange: false,   // the modal picker is the only edit path; commit fires through Activate, not a continuous-change event
            Create: value => {
                Drawable swatch = new() { Width = 64, Height = 22, Tag = value };
                swatch.Paint += (_, e) => {   // BOUNDARY ADAPTER — Eto draw callback paints the live color sample from the editor tag
                    Color4f c = swatch.Tag is Color4f tagged ? tagged : Color4f.Black;
                    Eto.Drawing.RectangleF area = new(Eto.Drawing.PointF.Empty, e.Graphics.ClipBounds.Size);
                    e.Graphics.FillRectangle(Eto.Drawing.Color.FromArgb((int)(c.R * 255f), (int)(c.G * 255f), (int)(c.B * 255f), allowAlpha ? (int)(c.A * 255f) : 255), area);
                    e.Graphics.DrawRectangle(global::Rhino.Runtime.HostUtils.RunningInDarkMode ? Eto.Drawing.Color.FromArgb(180, 180, 180) : Eto.Drawing.Color.FromArgb(80, 80, 80), area);
                };
                return swatch;
            },
            Read: static control => control.Tag is Color4f value ? value : Color4f.Black,
            Activate: Some<Func<Color4f, Fin<Color4f>>>(current => {
                Color4f picked = current;
                global::Rhino.UI.NamedColorList? list = named.Case switch { global::Rhino.UI.NamedColorList value => value, _ => null };
                return global::Rhino.UI.Dialogs.ShowColorDialog(parent: null, color: ref picked, allowAlpha: allowAlpha, namedColorList: list, colorCallback: null)
                    ? Fin.Succ(value: picked)
                    : Fin.Fail<Color4f>(error: new Fault.Cancelled());
            }));
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

public static class UiRequest {
    internal static UiRequest<T> OfScope<T>(string name, Func<RhinoUi.Scope, Fin<T>> run, bool interactive = true) =>
        new(name: string.IsNullOrWhiteSpace(value: name) ? nameof(UiRequest<>) : name, run: run, interactive: interactive);

    public static UiRequest<T> Native<T>(string name, Func<RhinoDoc, RunMode, Fin<T>> run, bool interactive = true) =>
        OfScope(
            name: name,
            run: scope => Op.Of(name: name).Need(run).Bind(valid => valid(arg1: scope.Document, arg2: scope.Mode)),
            interactive: interactive);
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
