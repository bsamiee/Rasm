using Eto.Drawing;
using Eto.Forms;
using Rasm.Rhino.UI.Rows;
using Rasm.Telemetry;
using Rhino.UI;
using Rhino.UI.Controls;

namespace Rasm.Rhino.UI.Inputs;

// --- [MODELS] --------------------------------------------------------------------------

public sealed record BrowserRow<TItem, TKey>(Func<TItem, TKey> Key, Func<TItem, string> Label, Func<RowScope, TItem, Size, IO<Image>> Tile, Seq<TItem> Items, Size TileSize)
    : IRow<BrowserRow<TItem, TKey>>
    where TItem : notnull
    where TKey : notnull {
    public Option<TKey> Current { get; init; }
    public Func<Edit<TKey>, IO<Unit>> Chosen { get; init; } = static _ => IO.pure(unit);
    public Option<ListSearch<TItem>> Search { get; init; }
    public Func<IO<Seq<TItem>>, Seq<Command<Invoke>>> Commands { get; init; } = static _ => [];

    public RowShape Shape { get; init; } = RowShape.Filled;

    public static IO<RowCells> Realize(IO<BrowserRow<TItem, TKey>> latest, LocalizeStringPair label, RowScope scope) =>
        Products.Serialized(run =>
        from owned in IO.env
        from first in latest
        from tiles in IO.lift(static () => Ref(HashMap<(TKey Key, TItem Item, Size Size), Owned<Image>>()))
        from buttons in IO.lift(static () => Ref(HashMap<TKey, Owned<ImageButton>>()))
        from keep in atomIO<Func<TItem, bool>>(static _ => true)
        from loads in IO.lift(static () => Ref(Option<Owned<Unit>>.None))
        from grid in Containers.Frame(static () => new ControlGridLayout { Wrap = true, GridWrapMode = GridWrapMode.Horizontal, ItemPadding = RhinoLayout.Padding(RhinoLayout.PaddingType.ButtonRow) })
        from scroll in Containers.Frame(static () => new Scrollable { Border = BorderType.None, ExpandContentWidth = true, ExpandContentHeight = false })
        from placed in IO.lift(() => { scroll.Content = grid; })
        from commands in Lists.Commanded(None, first.Commands(from current in latest from kept in keep.ValueIO select current.Items.Filter(kept)), scope)
        from post in Mailboxes.Idle<Unit, Unit>(_ => run(
            from current in latest
            from kept in keep.ValueIO
            from arranged in Arranged(grid, current, latest, buttons, tiles, current.Items.Filter(kept), commands.Menu, scope, owned)
            select unit))
        let shown =
            from current in latest
            from kept in keep.ValueIO
            let listed = current.Items.Filter(kept)
            from loaded in Products.Clear(loads).Bind(_ => Products.Replace(loads, Loaded(current, tiles, listed, scope, owned, post(unit, unit)))).Scoped(owned)
            from arranged in Arranged(grid, current, latest, buttons, tiles, listed, commands.Menu, scope, owned)
            from sized in IO.lift(() => { grid.ItemSize = current.TileSize; })
            select unit
        from keys in Subscriptions.Inline<KeyEventArgs>(h => grid.KeyDown += h, h => grid.KeyDown -= h).Attach(args => run(CommandRows.Pressed(commands.Keyed, args)))
        from content in Lists.Searched(scroll, first.Search, kept => run(writeIO(keep, kept).Bind(_ => shown)))
        select (new RowCells(content.Control, run(latest.Bind(current => content.Shown(current.Search)).Bind(_ => shown).Bind(_ => commands.Refresh))) { Menu = commands.Menu },
            IO.lift(grid.Items.Clear).Finally(Products.Clear(loads))));

    private static IO<Unit> Arranged(
        ControlGridLayout grid, BrowserRow<TItem, TKey> current, IO<BrowserRow<TItem, TKey>> latest, Ref<HashMap<TKey, Owned<ImageButton>>> buttons, Ref<HashMap<(TKey Key, TItem Item, Size Size), Owned<Image>>> tiles,
        Seq<TItem> listed, Seq<Seq<Command<Invoke>>> menu, RowScope scope, EnvIO owned) =>
        from wanted in IO.pure(listed.Map(current.Key))
        let chosen = toHashSet(wanted)
        let requests = toHashSet(listed.Map(item => (Key: current.Key(item), Item: item, Size: current.TileSize)))
        from prior in atomicIO(() => buttons.Value)
        from made in wanted.Filter(key => !prior.ContainsKey(key)).TraverseM(key => Products.Replace(buttons, key, Tiled(latest, key, menu, scope)).Scoped(owned)).As()
        from held in atomicIO(() => buttons.Value)
        from images in atomicIO(() => tiles.Value)
        from faced in listed.TraverseM(item => held.Find(current.Key(item)).Traverse(button => IO.lift(() => {
            (button.Value.Size, button.Value.ToolTip, button.Value.Image) = (current.TileSize, current.Label(item), images.Find((current.Key(item), item, current.TileSize)).Map(static tile => tile.Value).ValueUnsafe());
            button.Value.BackgroundColor = current.Current == Some(current.Key(item)) ? SystemColors.Highlight : Colors.Transparent;
        })).As()).As()
        let controls = wanted.Choose(key => held.Find(key).Map(static button => (Control)button.Value))
        from ordered in unless(toSeq(grid.Items) == controls, IO.lift(() => {
            grid.Items.Clear();
            _ = controls.Iter(control => grid.Items.Add(control));
        })).As()
        from dropped in toSeq(held.Keys).Filter(key => !chosen.Contains(key)).TraverseM(key => Products.Clear(buttons, key).Scoped(owned)).As()
        from pruned in toSeq(images.Keys).Filter(request => !requests.Contains(request)).TraverseM(request => Products.Clear(tiles, request).Scoped(owned)).As()
        select unit;

    private static IO<ImageButton> Tiled(IO<BrowserRow<TItem, TKey>> latest, TKey key, Seq<Seq<Command<Invoke>>> menu, RowScope scope) =>
        from made in use(static () => new ImageButton())
        from pressed in Subscriptions.On<EventArgs>(h => made.Click += h, h => made.Click -= h).Attach(_ => latest.Bind(last => last.Chosen(new Edit<TKey>.Changed(Applies: true, key))))
        from popup in unless(menu.IsEmpty, CommandRows.Menu(menu, scope).Bind(held => IO.lift(() => { made.ContextMenu = held.Menu; }))).As()
        select made;

    private static IO<Unit> Loaded(BrowserRow<TItem, TKey> current, Ref<HashMap<(TKey Key, TItem Item, Size Size), Owned<Image>>> tiles, Seq<TItem> listed, RowScope scope, EnvIO owned, IO<Unit> refreshed) =>
        from prior in atomicIO(() => tiles.Value)
        from forked in listed.Filter(item => !prior.ContainsKey((current.Key(item), item, current.TileSize))).TraverseM(item =>
            Products.Forked((from cached in Products.Replace(tiles, (current.Key(item), item, current.TileSize), Products.Acquired(current.Tile(scope, item, current.TileSize))).Scoped(owned.Resources)
                             from posted in refreshed
                             select unit)
                    | @catch(static error => !error.IsCancellation, static error => IO.lift(() => Session.Failed(error, nameof(BrowserRow<,>)))))).As()
        select unit;
}
