using Eto.Drawing;
using Eto.Forms;
using Rasm.Rhino.UI.Rows;
using Rhino.UI;
using Rhino.UI.Controls;

namespace Rasm.Rhino.UI.Inputs;

// --- [MODELS] --------------------------------------------------------------------------
public sealed record BrowserRow<TItem, TKey>(Func<TItem, TKey> Key, Func<TItem, string> Label, Func<RowScope, TItem, Size, IO<Image>> Tile, Seq<TItem> Items, Size TileSize, string Empty)
    : IRow<BrowserRow<TItem, TKey>>
    where TItem : notnull
    where TKey : notnull {
    public Option<TKey> Current { get; init; }
    public Func<Edit<TKey>, IO<Unit>> Chosen { get; init; } = static _ => IO.pure(unit);
    public Option<ListSearch<TItem>> Search { get; init; }
    public Func<IO<Seq<TItem>>, Seq<Command<Invoke>>> Commands { get; init; } = static _ => [];

    public RowShape Shape { get; init; } = RowShape.Filled;

    public static IO<RowCells> Realize(IO<BrowserRow<TItem, TKey>> latest, LocalizeStringPair label, RowScope scope) =>
        from first in latest
        from tiles in IO.lift(static () => Ref(HashMap<TKey, Owned<Image>>()))
        from seated in IO.lift(static () => Ref(HashMap<TKey, Owned<ImageButton>>()))
        from keep in atomIO<Func<TItem, bool>>(static _ => true)
        from owned in use(static () => EnvIO.New(token: CancellationToken.None))
        from loads in IO.lift(static () => Ref(Option<Owned<Unit>>.None))
        from grid in Containers.Frame(static () => new ControlGridLayout { Wrap = true, GridWrapMode = GridWrapMode.Horizontal, ItemPadding = RhinoLayout.Padding(RhinoLayout.PaddingType.ButtonRow) })
        from scroll in Containers.Frame(static () => new Scrollable { Border = BorderType.None, ExpandContentWidth = true, ExpandContentHeight = false })
        from empty in use(() => new Label { Text = LocalText.Of(first.Empty).Local, TextColor = SystemColors.DisabledText, TextAlignment = TextAlignment.Center })
        from stack in Containers.Frame(static () => new StackLayout { HorizontalContentAlignment = HorizontalAlignment.Stretch })
        from placed in IO.lift(() => {
            scroll.Content = grid;
            stack.Items.Add(new StackLayoutItem(scroll, expand: true));
            stack.Items.Add(empty);
        })
        from commands in Lists.Commanded(None, first.Commands(from current in latest from kept in keep.ValueIO select current.Items.Filter(kept)), scope)
        let shown =
            from current in latest
            from kept in keep.ValueIO
            let listed = current.Items.Filter(kept)
            from loaded in Products.Clear(loads).Bind(_ => Products.Replace(loads, Loaded(latest, tiles, listed, scope, owned))).Scoped(owned)
            from reseated in Seated(grid, latest, seated, tiles, listed, commands.Menu, scope, owned)
            from emptied in IO.lift(() => { (scroll.Visible, empty.Visible, grid.ItemSize) = (!listed.IsEmpty, listed.IsEmpty, current.TileSize); })
            select unit
        from keys in Subscriptions.Inline<KeyEventArgs>(h => grid.KeyDown += h, h => grid.KeyDown -= h).Attach(args => CommandRows.Pressed(commands.Keyed, args))
        from content in Lists.Searched(stack, first.Search, kept => writeIO(keep, kept).Bind(_ => shown))
        select new RowCells(content.Control, latest.Bind(current => content.Shown(current.Search)).Bind(_ => shown).Bind(_ => commands.Refresh)) { Menu = commands.Menu };

    private static IO<Unit> Seated(
        ControlGridLayout grid, IO<BrowserRow<TItem, TKey>> latest, Ref<HashMap<TKey, Owned<ImageButton>>> seated, Ref<HashMap<TKey, Owned<Image>>> tiles,
        Seq<TItem> listed, Seq<Seq<Command<Invoke>>> menu, RowScope scope, EnvIO owned) =>
        from current in latest
        let wanted = listed.Map(current.Key)
        let chosen = toHashSet(wanted)
        from prior in atomicIO(() => seated.Value)
        from made in wanted.Filter(key => !prior.ContainsKey(key)).TraverseM(key => Products.Replace(seated, key, Tiled(latest, key, menu, scope)).Scoped(owned)).As()
        from held in atomicIO(() => seated.Value)
        from images in atomicIO(() => tiles.Value)
        from faced in listed.TraverseM(item => held.Find(current.Key(item)).Traverse(seat => IO.lift(() => {
            (seat.Value.Size, seat.Value.ToolTip, seat.Value.Image) = (current.TileSize, current.Label(item), images.Find(current.Key(item)).Map(static tile => tile.Value).ValueUnsafe());
            seat.Value.BackgroundColor = current.Current == Some(current.Key(item)) ? SystemColors.Highlight : Colors.Transparent;
        })).As()).As()
        let controls = wanted.Choose(key => held.Find(key).Map(static seat => (Control)seat.Value))
        from ordered in unless(toSeq(grid.Items) == controls, IO.lift(() => {
            grid.Items.Clear();
            _ = controls.Iter(control => grid.Items.Add(control));
        })).As()
        from dropped in toSeq(held.Keys).Filter(key => !chosen.Contains(key)).TraverseM(key => Products.Clear(seated, key).Scoped(owned)).As()
        select unit;

    private static IO<ImageButton> Tiled(IO<BrowserRow<TItem, TKey>> latest, TKey key, Seq<Seq<Command<Invoke>>> menu, RowScope scope) =>
        from made in use(static () => new ImageButton())
        from pressed in Subscriptions.On<EventArgs>(h => made.Click += h, h => made.Click -= h).Attach(_ => latest.Bind(last => last.Chosen(EditPhase.Commit.Of(key))))
        from popup in unless(menu.IsEmpty, CommandRows.Menu(menu, scope).Bind(held => IO.lift(() => { made.ContextMenu = held.Menu; }))).As()
        select made;

    private static IO<Unit> Loaded(IO<BrowserRow<TItem, TKey>> latest, Ref<HashMap<TKey, Owned<Image>>> tiles, Seq<TItem> listed, RowScope scope, EnvIO owned) =>
        from current in latest
        let wanted = toHashSet(listed.Map(current.Key))
        from prior in atomicIO(() => tiles.Value)
        from pruned in toSeq(prior.Keys).Filter(key => !wanted.Contains(key)).TraverseM(key => Products.Clear(tiles, key).Scoped(owned)).As()
        from forked in listed.Filter(item => !prior.ContainsKey(current.Key(item))).TraverseM(item =>
            use(((from made in current.Tile(scope, item, current.TileSize)
                  from cached in Products.Replace(tiles, current.Key(item), use(() => made)).Scoped(owned)
                  select unit)
                    | @catch(static error => !Session.Cancellation(error), static error => IO.lift(() => Session.Failed(error, nameof(BrowserRow<,>)))))
                .Fork(), static fork => fork.Cancel)).As()
        select unit;
}
