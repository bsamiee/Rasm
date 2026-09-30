using Eto.Forms;
using Rasm.Rhino.Document;

namespace Rasm.Rhino.Ui;

// --- [MODELS] --------------------------------------------------------------------------
public sealed record TreeShape<TRow, TKey>(Func<TRow, TKey> Key, Func<TRow, Seq<TRow>> Children, Func<TRow, bool> Expanded) where TKey : notnull;

public sealed class RowItem<TRow> : TreeGridItem {
    internal RowItem(TRow row, Seq<ITreeGridItem> children) : base(children) => Row = row;

    public TRow Row { get; }
}

// --- [OPERATIONS] ----------------------------------------------------------------------
public static class TreeRows {
    // --- [READS]
    public static Seq<TRow> Selection<TRow>(TreeGridView grid) =>
        toSeq(grid.SelectedItems.Cast<RowItem<TRow>>()).Map(static item => item.Row).Strict();

    // --- [WRITES]
    public static IO<Unit> Fill<TRow, TKey>(TreeGridView grid, TreeShape<TRow, TKey> shape, Seq<TRow> roots) where TKey : notnull =>
        IO.lift(() => {
            LanguageExt.HashSet<TKey> selected = toHashSet(Selection<TRow>(grid).Map(shape.Key));
            grid.DataStore = new TreeGridItemCollection(roots.Map<ITreeGridItem>(row => Item(shape, row)));
            grid.SelectedRows = Visible(shape, roots).Choose((index, row) => Answers.Found(selected.Contains(shape.Key(row)), index)).Strict();
        });

    private static RowItem<TRow> Item<TRow, TKey>(TreeShape<TRow, TKey> shape, TRow row) where TKey : notnull =>
        new(row, shape.Children(row).Map<ITreeGridItem>(child => Item(shape, child))) { Expanded = shape.Expanded(row) };

    private static Seq<TRow> Visible<TRow, TKey>(TreeShape<TRow, TKey> shape, Seq<TRow> rows) where TKey : notnull =>
        rows.Bind(row => row.Cons(shape.Expanded(row) ? Visible(shape, shape.Children(row)) : Seq<TRow>()));

    // --- [ROUTES]
    public static IO<IDisposable> Expansion<TRow>(TreeGridView grid, Func<TRow, bool, IO<Unit>> expand, Action<Error> reject) =>
        Events.AttachAll(
            Seq(
                Routed<TRow>(h => grid.Expanded += h, h => grid.Expanded -= h, row => expand(row, true), reject),
                Routed<TRow>(h => grid.Collapsed += h, h => grid.Collapsed -= h, row => expand(row, false), reject)),
            reject);

    public static IO<IDisposable> Activation<TRow>(TreeGridView grid, Func<TRow, IO<Unit>> activate, Action<Error> reject) =>
        Routed(h => grid.Activated += h, h => grid.Activated -= h, activate, reject);

    public static IO<IDisposable> Menu<TRow>(TreeGridView grid, Func<Seq<TRow>, Seq<MenuItem>> items, Action<Error> reject) =>
        from prior in IO.lift(() => grid.ContextMenu)
        from menu in IO.lift(static () => new ContextMenu())
        from attached in Events.AttachAll(
            Seq(
                IO.lift(() => { grid.ContextMenu = menu; }).Map<IDisposable>(_ => new Disposal(() => {
                    grid.ContextMenu = prior;
                    menu.Dispose();
                })),
                Events.Attach(
                    h => menu.Opening += h,
                    h => menu.Opening -= h,
                    Answers.Handler<EventArgs>(
                        _ => IO.lift(() => {
                            menu.Items.Clear();
                            menu.Items.AddRange(items(Selection<TRow>(grid)));
                        }),
                        reject))),
            reject)
        select attached;

    private static IO<IDisposable> Routed<TRow>(
        Action<EventHandler<TreeGridViewItemEventArgs>> subscribe,
        Action<EventHandler<TreeGridViewItemEventArgs>> unsubscribe,
        Func<TRow, IO<Unit>> deliver,
        Action<Error> reject) =>
        Events.Attach(subscribe, unsubscribe, Answers.Handler<TreeGridViewItemEventArgs>(args => deliver(((RowItem<TRow>)args.Item).Row), reject));
}
