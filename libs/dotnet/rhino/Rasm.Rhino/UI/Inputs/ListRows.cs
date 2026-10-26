using Eto.Drawing;
using Eto.Forms;
using Rasm.Imaging.Pixels;
using Rasm.Rhino.Events;
using Rasm.Rhino.Persistence.Settings;
using Rasm.Rhino.Persistence.Stores;
using Rasm.Rhino.UI.Numeric;
using Rasm.Rhino.UI.Rows;
using Rasm.Rhino.UI.Styling;
using Rasm.Rhino.UI.Surfaces;
using Rasm.UI;
using Rasm.UI.Imaging;
using Rasm.UI.Styling;
using Rhino.UI;
using Rhino.UI.Controls;
using SkiaSharp;
using Orientation = Rasm.UI.Orientation;

namespace Rasm.Rhino.UI.Inputs;

// --- [MODELS] --------------------------------------------------------------------------
[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record ListColumn<TItem> where TItem : notnull {
    private ListColumn(Option<string> caption) => Caption = caption;

    public Option<string> Caption { get; }

    public bool Renames => this is Label { Rename.IsSome: true };

    public bool Edits => Renames || this is Check;

    public bool Stretches => Map(label: true, check: false, glyph: false, progress: true);

    public static ListColumn<TItem> Named<TValue, TRaw>(Option<string> caption, Lens<TItem, TValue> name, Option<Func<TItem, GlyphSource>> icon)
        where TValue : IObjectFactory<TValue, TRaw, ValidationError>, IConvertible<TRaw>
        where TRaw : notnull, ISpanParsable<TRaw> =>
        new Label(
            caption,
            item => StoredText.Capture<TValue, TRaw>(name.Get(item)),
            icon,
            Some<Func<TItem, string, Fin<TItem>>>((item, text) => StoredText.Recall<TValue, TRaw>(text).Map(value => name.Set(value, item))));

    public static ListColumn<TItem> Fraction<TValue, TKey>(Option<string> caption, Func<TItem, Option<TValue>> read)
        where TValue : System.Numerics.IMinMaxValue<TValue>, IConvertible<TKey>
        where TKey : System.Numerics.INumber<TKey> =>
        new Progress(caption, item => read(item).Map(static value => float.CreateChecked(
            (double.CreateChecked(value.ToValue()) - double.CreateChecked(TValue.MinValue.ToValue()))
            / (double.CreateChecked(TValue.MaxValue.ToValue()) - double.CreateChecked(TValue.MinValue.ToValue())))));

    public sealed record Label(Option<string> Caption, Func<TItem, string> Read, Option<Func<TItem, GlyphSource>> Icon, Option<Func<TItem, string, Fin<TItem>>> Rename)
        : ListColumn<TItem>(Caption);

    public sealed record Check(Option<string> Caption, Lens<TItem, bool> Value) : ListColumn<TItem>(Caption);

    public sealed record Glyph(Option<string> Caption, Func<TItem, GlyphSource> Icon) : ListColumn<TItem>(Caption);

    public sealed record Progress(Option<string> Caption, Func<TItem, Option<float>> Read) : ListColumn<TItem>(Caption);
}

public sealed record ListSearch<TItem>(string Placeholder, Func<RowScope, TItem, string> Text, Option<RowSource<bool>> Shown) where TItem : notnull;

public sealed record ListBranches<TItem, TKey>(
    Lens<TItem, Seq<TItem>> Children,
    (RowSource<LanguageExt.HashSet<TKey>> Source, RowField<LanguageExt.HashSet<TKey>> Field) Expanded)
    where TItem : notnull
    where TKey : notnull;

public sealed record ListRow<TItem, TKey>(
    IterableNE<ListColumn<TItem>> Columns,
    Func<TItem, TKey> Key,
    (RowSource<Seq<TItem>> Source, RowField<Seq<TItem>> Field) Items,
    (RowSource<LanguageExt.HashSet<TKey>> Source, RowField<LanguageExt.HashSet<TKey>> Field) Selected)
    where TItem : notnull
    where TKey : notnull {
    public Option<ListBranches<TItem, TKey>> Branches { get; init; }
    public Option<(RowSource<Option<TKey>> Source, RowField<Option<TKey>> Field)> Current { get; init; }
    public Option<Func<TItem, string>> Ordered { get; init; }
    public Option<ListSearch<TItem>> Search { get; init; }
    public Option<Func<TItem, bool>> Available { get; init; }
    public Option<Func<TItem, IO<Unit>>> Activate { get; init; }
    public Option<string> Empty { get; init; }
    public Option<(Command<RowRule, Invoke>.Run Add, Command<RowRule, Invoke>.Run Remove)> Pair { get; init; }
    public Func<IO<Seq<TItem>>, Seq<Command<RowRule, Invoke>>> Commands { get; init; } = static _ => [];
    public bool AllowMultipleSelection { get; init; }
}

[ValueObject<float>(SkipIParsable = true, AdditionOperators = OperatorsGeneration.None, SubtractionOperators = OperatorsGeneration.None, MultiplyOperators = OperatorsGeneration.None, DivisionOperators = OperatorsGeneration.None)]
public readonly partial struct TileSize : System.Numerics.IMinMaxValue<TileSize> {
    public static TileSize MinValue { get; } = new(32f);
    public static TileSize MaxValue { get; } = new(256f);
    public static Presentation<float> Presentation { get; } = new() { Step = 1f, Decimals = 0 };

    public static TileSize Clamped(float width) => new(float.Clamp(width, MinValue._value, MaxValue._value));

    static partial void ValidateFactoryArguments(ref ValidationError? validationError, ref float value) => validationError = ValidationResults.OutsideRange(value, MinValue, MaxValue);
}

[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
[KeyMemberComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class BrowserMode : ICaptioned {
    public static readonly BrowserMode Grid = new("grid", Icons.Embedded(typeof(BrowserMode).Assembly, "grid.svg"), ImagingRoles.OverlayCaption,
        static tile => new Tracks.AutoFill(new Sizing.Fixed(tile)),
        static width => TileSize.Clamped(width / 3f),
        static (_, image, caption, number, marks) => new Node.Overlay(image with { Width = ListRows.Grown }, [
            new Node.Flex(Orientation.Vertical, [
                new Node.Flex(Orientation.Horizontal, number.ToSeq().Add(new Node.Box { Width = ListRows.Grown }) + marks) { Width = ListRows.Grown },
                new Node.Box { Height = ListRows.Grown },
                caption,
            ]) { Width = ListRows.Grown, Height = ListRows.Grown },
        ]));
    public static readonly BrowserMode List = new("list", Icons.Embedded(typeof(BrowserMode).Assembly, "list.svg"), Role.Label,
        static _ => new Tracks.Explicit([ListRows.Grown]),
        static _ => TileSize.Clamped(48f),
        static (tile, image, caption, number, marks) => new Node.Flex(Orientation.Horizontal, Seq(image with { Width = new Sizing.Fixed(tile) }) + number.ToSeq() + Seq(caption) + marks));

    public GlyphSource Glyph { get; }

    public Role Captioned { get; }

    [UseDelegateFromConstructor]
    public partial Tracks Across(float tile);

    [UseDelegateFromConstructor]
    public partial TileSize Fit(float width);

    [UseDelegateFromConstructor]
    public partial Node Tiled(float tile, Node image, Node caption, Option<Node> number, Seq<Node> marks);
}

internal sealed record BrowserLayout(Option<TileSize> Size, Option<BrowserMode> Mode) : IStateRecord<BrowserLayout, BrowserLayoutParameter> {
    public static BrowserLayout Default { get; } = new(None, None);
}

[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
[KeyMemberComparer<ComparerAccessors.StringOrdinal, string>]
internal sealed partial class BrowserLayoutParameter : IStateParameter<BrowserLayout> {
    public static readonly BrowserLayoutParameter Size = new("tile-size", new StateParameter<BrowserLayout>.OptionalKeyed<TileSize, float>(
            Lens<BrowserLayout, Option<TileSize>>.New(static layout => layout.Size, static size => layout => layout with { Size = size })),
        new("Tile size", "Thumbnail width, fitted to browser when absent"));
    public static readonly BrowserLayoutParameter Mode = new("mode", new StateParameter<BrowserLayout>.OptionalChoice<BrowserMode>(
            Lens<BrowserLayout, Option<BrowserMode>>.New(static layout => layout.Mode, static mode => layout => layout with { Mode = mode }), "Default"),
        new("View", "Thumbnail arrangement, row default when absent"));

    public StateParameter<BrowserLayout> Kind { get; }

    public ParameterText Text { get; }
}

public sealed record TileMark<TItem>(string Caption, GlyphSource On, GlyphSource Off, Lens<TItem, bool> Value) where TItem : notnull;

public sealed record BrowserRow<TItem, TKey>(
    Func<TItem, TKey> Key,
    Func<RowScope, TItem, string> Label,
    Func<RowScope, TItem, PixelExtent, IO<SKImage>> Tile,
    Func<RowScope, ValueStore<Seq<TItem>>> Items,
    Func<RowScope, IO<Option<TKey>>> Current,
    Func<RowScope, TKey, IO<Unit>> Choose,
    BrowserMode Mode,
    ReadoutSource<Size> Extent,
    string Empty)
    where TItem : notnull
    where TKey : notnull {
    public bool Numbered { get; init; }
    public bool Reorders { get; init; }
    public Option<ListSearch<TItem>> Search { get; init; }
    public Option<Func<TItem, string, Fin<TItem>>> Rename { get; init; }
    public Seq<TileMark<TItem>> Marks { get; init; }
    public Option<Func<TItem, Option<string>>> File { get; init; }
    public Option<Func<RowScope, TItem, Action<IDisposable>, IO<Unit>>> Preview { get; init; }
    public Seq<HostEvent<Unit>> Reloads { get; init; }
    public Func<IO<Seq<TItem>>, Seq<Command<RowRule, Invoke>>> Commands { get; init; } = static _ => [];
}

// --- [SERVICES] ------------------------------------------------------------------------
internal sealed class RowItem<TItem> : TreeGridItem where TItem : notnull {
    internal RowItem(TItem row, int depth, Seq<RowItem<TItem>> kids) : base(kids.Map<ITreeGridItem>(static kid => kid)) =>
        (Row, Depth, Kids) = (row, depth, kids);

    public TItem Row { get; internal set; }
    public int Depth { get; }
    public Seq<RowItem<TItem>> Kids { get; }
}

// --- [OPERATIONS] ----------------------------------------------------------------------
public static class ListRows {
    // --- [LISTS]
    public static ControlRow List<TItem, TKey>(string caption, string help, ListRow<TItem, TKey> row, RowRules rules)
        where TItem : notnull
        where TKey : notnull =>
        new ControlRow.Readout(RowShape.Filled, (scope, retain) => DisposalOps.Acquire(owned =>
            from grid in DisposalOps.Acquire(IO.lift(static () => new TreeGridView()), owned)
            from faced in IO.lift(() => {
                (grid.ShowHeader, grid.GridLines, grid.Border, grid.RowHeight, grid.AllowMultipleSelection, grid.AllowEmptySelection) =
                    (toSeq(row.Columns).Exists(static column => column.Caption.IsSome), GridLines.None, BorderType.None, 20, row.AllowMultipleSelection, true);
            })
            from emphasis in IO.lift(static () => SystemFonts.Bold(SystemFonts.Label().Size))
            from list in IO.lift(static () => Atom(new ListState<TItem>(Seq<RowItem<TItem>>(), [], Seq<(RowItem<TItem> Node, TItem Row)>())))
            from keep in atomIO<Func<TItem, bool>>(static _ => true)
            from columns in Columns(row, grid, scope.Sink, owned)
            from commands in Commanded(row.Pair, row.Commands(IO.lift(() => Chosen(Folded(row, list.Value.Nodes).Visible, toHashSet(grid.SelectedRows)).Map(static node => node.Row))), scope, owned)
            from selection in Selected(grid, list, row, scope, owned)
            from items in Items(grid, list, keep, row, selection.Shown, scope, owned)
            from expanded in row.Branches.Traverse(branches => Expansion(grid, list, row, branches, scope, owned)).As()
            from current in row.Current.Traverse(held => Watched(held.Source, held.Source.Keys(held.Field.Parameter),
                shown => IO.lift(() => Indices(row, Folded(row, list.Value.Nodes).Visible, toHashSet(shown.ToSeq())).Head.Iter(grid.ScrollToRow)),
                scope, new CallbackSite(scope.Sink, typeof(TreeGridView), nameof(TreeGridView.ScrollToRow)), owned)).As()
            let brackets = Seq((items.Keys, items.Shown), (selection.Keys, selection.Shown)) + expanded.ToSeq() + current.ToSeq()
            from gestures in Gestures(grid, list, row, emphasis, commands.Keyed, scope, owned)
            from emptied in row.Empty.Traverse(text => Emptied(grid, list, RowText.Localize(text, scope.Sink).Local, scope.Sink, owned)).As()
            from paired in Paired(emptied.IfNone(grid), commands.Pair, owned)
            from content in Searched(paired, row.Search, kept => writeIO(keep, kept).Bind(_ => items.Shown), scope, owned)
            from listened in Listened(brackets + content.Brackets, scope, owned)
            select new RowCells(content.Control, None, None, None, [], [], new RowHelp(None, None),
                Callbacks.Each((brackets + content.Brackets).Map(static bracket => bracket.Shown)).Bind(_ => commands.Refresh),
                None, Some(ChoiceRows.Context(grid, commands.Menu, Seq(commands.Refresh)))), retain),
            caption, help, rules);

    // --- [BROWSERS]
    private const string Arranged = "thumbnail-browser";

    internal static readonly Sizing Grown = new Sizing.Grow(Factor.Create(1f));

    private static readonly Key RenameBox = new Key.Named("rename");
    private static readonly Func<int, Key> Ordinal = static index => new Key.Ordinal((uint)index);

    private static readonly RowSource<BrowserLayout> Layout = RowSource.Fields(Arranged, [.. BrowserLayoutParameter.Items],
        static scope => IO.pure(scope.Settings.Map(static node => PlugInSettings.Record<BrowserLayout>(node.Child(Arranged), Applied.Live)).ToSeq()));

    private static readonly ScalarText<TileSize, float> Spoken = NumberText.Scalar(NumericDomain.Bounded<TileSize, float>(TileSize.Presentation), "");

    public static Command<RowRule, Invoke>.Choice Arrangement(BrowserMode initial) =>
        CommandRows.Choices<BrowserMode>(
            new CommandFace($"{Arranged}-{BrowserLayoutParameter.Mode.Key}", BrowserLayoutParameter.Mode.Text.Caption, Some(BrowserLayoutParameter.Mode.Text.Help), None),
            None,
            mode => RowRule.When(Layout, held => held.Mode.IfNone(initial) == mode, BrowserLayoutParameter.Mode),
            static (scope, mode) => Written(scope, held => held with { Mode = Some(mode) }),
            static mode => new CommandFace($"{Arranged}-{mode.Key}", ((ICaptioned)mode).Caption, None, Some<GlyphSource>(mode.Glyph)));

    public static ControlRow Browser<TItem, TKey>(string caption, string help, BrowserRow<TItem, TKey> row, RowRules rules)
        where TItem : notnull
        where TKey : notnull =>
        RowSource.Opaque(caption, nameof(BrowserRow<,>.Items), Seq<TItem>(), row.Items) switch {
            var items => new ControlRow.Readout(RowShape.Filled, (scope, retain) => DisposalOps.Acquire(owned =>
                from held in atomIO(new GalleryState<TItem, TKey>([], new Size(1, 1), BrowserLayout.Default))
                from wired in atomIO(Option<Wiring<TItem, TKey>>.None)
                from knot in atomIO(Option<Gallery<TItem, TKey>>.None)
                from loads in Bin(owned)
                from dwell in Bin(owned)
                from editing in Bin(owned)
                let site = new CallbackSite(scope.Sink, typeof(SceneView), nameof(Browser))
                from view in TextRows.Live(row.Extent,
                    (extent, keep) =>
                        from extended in held.SwapIO(state => state with { Extent = extent })
                        from made in DisposalOps.Acquire(IO.lift(() => new SceneView(scope, new Node.Box()) {
                            Slots = _ => knot.ValueIO.Bind(static tied => IO.lift(tied.ToFin(new Refused(nameof(SceneView.Slots), Refused.Absent)))).Bind(static each => Renamer(each)),
                        }), keep)
                        select made,
                    (_, extent) => knot.ValueIO.Bind(tied => tied.Traverse(each => Advanced(each, state => state with { Extent = extent })).As()).Map(static _ => unit),
                    scope, owned)
                let gallery = new Gallery<TItem, TKey>(row, scope, view, held, wired, loads, dwell, editing)
                from tied in writeIO(knot, Some(gallery))
                from released in DisposalOps.Acquire(IO.pure(unit), _ => held.ValueIO.Bind(static state => DisposalOps.Release(Held(state.Tiles))), owned)
                from edit in RowEdit.Bind<Seq<TItem>, Action>(items.Source, IterableNE.create(items.Field), static _ => { }, static _ => { }, static _ => static () => { },
                    shown => Filled(gallery, shown.ToSeq().Flatten()), scope, site, owned)
                from current in Current(gallery, site, owned)
                from laid in Watched(Layout, Named(Layout, scope), layout => Advanced(gallery, state => state with { Layout = layout }), scope, site, owned)
                from commands in Commanded(None, row.Commands(held.ValueIO.Map(state => Picked(row, state))), scope, owned)
                from host in CommandRows.Host(scope.Hosted(view), owned)
                from bound in writeIO(wired, Some(new Wiring<TItem, TKey>(
                    change => edit.Take(change, static (rearrange, latest) => rearrange(latest)).Map(static _ => unit), current.Choose, host, commands.Menu)))
                from keys in Subscriptions.Attach(h => view.KeyDown += h, h => view.KeyDown -= h,
                    Callbacks.Synchronous<KeyEventArgs>(args => Pressed(gallery, commands.Keyed, args), new CallbackSite(scope.Sink, typeof(SceneView), nameof(Control.KeyDown))), owned)
                from sized in Subscriptions.Attach(h => view.SizeChanged += h, h => view.SizeChanged -= h,
                    Callbacks.Handler<EventArgs>(_ => Placed(gallery), new CallbackSite(scope.Sink, typeof(SceneView), nameof(Control.SizeChanged))), owned)
                from loaded in Subscriptions.Attach(h => view.LoadComplete += h, h => view.LoadComplete -= h,
                    Callbacks.Handler<EventArgs>(_ => Placed(gallery), new CallbackSite(scope.Sink, typeof(SceneView), nameof(Control.LoadComplete))), owned)
                from reloads in Callbacks.Each(row.Reloads.Map(reload => reload.Choose(static _ => Some((unit, unit)))
                    .Through(Subscriptions.Idle<Unit, Unit>(_ => Reloaded(gallery, static state => state).Bind(_ => edit.Shown).Bind(_ => current.Shown)), scope.Sink, owned)))
                from content in Searched(view, row.Search, keep => Advanced(gallery, state => state with { Keep = keep }), scope, owned)
                let brackets = Seq((edit.Keys, edit.Shown), laid) + content.Brackets
                from listened in Listened(brackets, scope, owned)
                from placed in Placed(gallery)
                select new RowCells(content.Control, None, None, None, [], [], new RowHelp(None, None),
                    Callbacks.Each(brackets.Map(static bracket => bracket.Shown)).Bind(_ => current.Shown).Bind(_ => commands.Refresh),
                    None, Some(ChoiceRows.Context(view, commands.Menu, Seq(commands.Refresh)))) {
                    Availability = Some<Func<bool, IO<Unit>>>(enabled => Advanced(gallery, state => state with { Enabled = enabled })),
                }, retain),
                caption, help, rules),
        };

    private sealed record Tile<TItem>(Content<SKImage> Content, TItem Item, int Revision, SKSizeI Pixels) where TItem : notnull {
        public bool Serves(TItem item, int revision, SKSizeI pixels) => Revision == revision && Pixels == pixels && EqualityComparer<TItem>.Default.Equals(Item, item);
    }

    private sealed record GalleryState<TItem, TKey>(Seq<TItem> Items, Size Extent, BrowserLayout Layout) where TItem : notnull where TKey : notnull {
        public Func<TItem, bool> Keep { get; init; } = static _ => true;
        public Option<TileSize> Resizing { get; init; }
        public LanguageExt.HashSet<TKey> Picked { get; init; }
        public Option<TKey> Anchor { get; init; }
        public Option<TKey> Focus { get; init; }
        public Option<TKey> Current { get; init; }
        public Option<TKey> Hovered { get; init; }
        public Option<TKey> Renaming { get; init; }
        public LanguageExt.HashSet<TKey> Viewed { get; init; }
        public HashMap<TKey, Tile<TItem>> Tiles { get; init; }
        public int Revision { get; init; }
        public float Width { get; init; }
        public Option<float> Scale { get; init; }
        public bool Enabled { get; init; } = true;
    }

    private sealed record Wiring<TItem, TKey>(
        Func<Edit<Func<Seq<TItem>, Fin<Seq<TItem>>>>, IO<Unit>> Items, Func<Edit<TKey>, IO<Unit>> Choose, CommandHost<RowRule, Invoke> Host, Seq<Seq<Command<RowRule, Invoke>>> Menu)
        where TItem : notnull where TKey : notnull;

    private sealed record Gallery<TItem, TKey>(
        BrowserRow<TItem, TKey> Row, RowScope Scope, SceneView View, Atom<GalleryState<TItem, TKey>> State, Atom<Option<Wiring<TItem, TKey>>> Wired,
        Retained<IDisposable> Loads, Retained<IDisposable> Dwell, Retained<IDisposable> Editing)
        where TItem : notnull where TKey : notnull;

    private static IO<Retained<IDisposable>> Bin(Action<IDisposable> retain) =>
        DisposalOps.Acquire(IO.lift(static () => new Retained<IDisposable>(DisposalOps.Release)), retain);

    // --- [GALLERY]
    private static IO<Unit> Advanced<TItem, TKey>(Gallery<TItem, TKey> gallery, Func<GalleryState<TItem, TKey>, GalleryState<TItem, TKey>> next)
        where TItem : notnull
        where TKey : notnull =>
        from moved in IO.lift(() => (Before: gallery.State.Value, After: gallery.State.Swap(state => Normalized(gallery.Row, next(state)))))
        from shown in gallery.View.Show(Drawn(gallery, moved.After))
        let kept = Held(moved.After.Tiles)
        from released in DisposalOps.Release(Held(moved.Before.Tiles).Filter(image => !kept.Exists(held => ReferenceEquals(held, image))))
        from requested in Requested(gallery)
        select unit;

    private static IO<Unit> Reloaded<TItem, TKey>(Gallery<TItem, TKey> gallery, Func<GalleryState<TItem, TKey>, GalleryState<TItem, TKey>> next)
        where TItem : notnull
        where TKey : notnull =>
        gallery.Loads.Drain.Bind(_ => Advanced(gallery, state => next(state) with { Revision = state.Revision + 1 }));

    private static IO<Unit> Filled<TItem, TKey>(Gallery<TItem, TKey> gallery, Seq<TItem> items)
        where TItem : notnull
        where TKey : notnull =>
        gallery.State.ValueIO.Bind(state => state.Items == items ? IO.pure(unit) : Advanced(gallery, held => held with { Items = items }));

    private static IO<Unit> Placed<TItem, TKey>(Gallery<TItem, TKey> gallery)
        where TItem : notnull
        where TKey : notnull =>
        IO.lift(() => (Width: (float)gallery.View.Width, Scale: Optional(gallery.View.ParentWindow).Map(static window => window.LogicalPixelSize)))
            .Bind(at => Advanced(gallery, state => state with { Width = at.Width, Scale = at.Scale }));

    private static GalleryState<TItem, TKey> Normalized<TItem, TKey>(BrowserRow<TItem, TKey> row, GalleryState<TItem, TKey> state)
        where TItem : notnull
        where TKey : notnull =>
        (Keys: toHashSet(state.Items.Map(row.Key)), Shown: toHashSet(state.Items.Filter(state.Keep).Map(row.Key))) switch {
            var held => state with {
                Picked = state.Picked.Intersect(held.Shown),
                Anchor = state.Anchor.Filter(held.Shown.Contains),
                Focus = state.Focus.Filter(held.Shown.Contains),
                Current = state.Current.Filter(held.Keys.Contains),
                Hovered = state.Hovered.Filter(held.Shown.Contains),
                Renaming = state.Renaming.Filter(held.Shown.Contains),
                Viewed = state.Viewed.Intersect(held.Shown),
                Tiles = state.Tiles.Filter((key, _) => held.Shown.Contains(key) && state.Viewed.Contains(key)),
            },
        };

    private static Seq<IDisposable> Held<TItem, TKey>(HashMap<TKey, Tile<TItem>> tiles) where TItem : notnull where TKey : notnull =>
        toSeq(tiles.Values).Bind(static tile => tile.Content.Held.ToSeq()).Map(static image => (IDisposable)image);

    private static BrowserMode Mode<TItem, TKey>(BrowserRow<TItem, TKey> row, GalleryState<TItem, TKey> state)
        where TItem : notnull
        where TKey : notnull =>
        state.Layout.Mode.IfNone(row.Mode);

    private static SKSize Boxed<TItem, TKey>(GalleryState<TItem, TKey> state, TileSize tile)
        where TItem : notnull
        where TKey : notnull =>
        new((float)tile, (float)tile * state.Extent.Height / state.Extent.Width);

    private static Option<TItem> Listed<TItem, TKey>(BrowserRow<TItem, TKey> row, GalleryState<TItem, TKey> state, TKey key)
        where TItem : notnull
        where TKey : notnull =>
        state.Items.Find(item => EqualityComparer<TKey>.Default.Equals(row.Key(item), key));

    private static Seq<TItem> Picked<TItem, TKey>(BrowserRow<TItem, TKey> row, GalleryState<TItem, TKey> state)
        where TItem : notnull
        where TKey : notnull =>
        state.Items.Filter(state.Keep).Filter(item => state.Picked.Contains(row.Key(item)));

    // --- [DRAW]
    private static Node.Flex Drawn<TItem, TKey>(Gallery<TItem, TKey> gallery, GalleryState<TItem, TKey> state)
        where TItem : notnull
        where TKey : notnull =>
        Placement(gallery, state) switch {
            var laid => toHashMap(laid.Keys.Map(static (key, at) => (key, Ordinal(at)))) switch {
                var index => new Node.Flex(Orientation.Vertical, [
                    Sized(gallery, laid.Tile),
                    laid.Shown.IsEmpty
                        ? new Node.Text([RowText.Localize(gallery.Row.Empty, gallery.Scope.Sink).Local]) { Role = Role.Label, Align = UnitPoint.Center, Width = Grown, Height = Grown }
                        : new Node.Scroll(Orientation.Vertical, new Part.Choice(
                                laid.Keys.Map(static (_, at) => Ordinal(at)),
                                new Selection.Many(toHashSet(toSeq(state.Picked).Choose(index.Find)), state.Anchor.Bind(index.Find), state.Focus.Bind(index.Find)),
                                change => Selecting(gallery, laid.Keys, change)) {
                                Reorder = gallery.Row.Reorders ? Some<Func<Edit<Seq<Key>>, IO<Unit>>>(change => Reordered(gallery, laid.Keys, change)) : None,
                                Exported = gallery.Row.File.Map(file => (Func<Seq<Key>, IO<Unit>>)(moved => Exported(gallery, file, Resolved(laid.Keys, moved)))),
                                Menu = Some<Func<Seq<Key>, SKRect, IO<Unit>>>((targeted, frame) => Menued(gallery, Resolved(laid.Keys, targeted), frame)),
                                Hover = Some<Func<Option<Key>, IO<Unit>>>(hovered => Hovered(gallery, hovered.Bind(key => Resolved(laid.Keys, key)))),
                                Viewed = Some<Func<LanguageExt.HashSet<Key>, IO<Unit>>>(seen => Advanced(gallery, current => current with { Viewed = toHashSet(Resolved(laid.Keys, toSeq(seen))) })),
                            }.Drawn(
                                key => laid.Entries[key] switch {
                                    var entry => Tiled(gallery, state, laid.Mode, (float)laid.Tile, entry.Item, entry.Number),
                                },
                                Orientation.Vertical, laid.Mode.Across(laid.Box.Width), laid.Box.Height, toHashSet(toSeq(state.Viewed).Choose(index.Find)))) { Width = Grown, Height = Grown },
                ]) {
                    Width = Grown,
                    Height = Grown,
                    Conditions = state.Enabled ? [] : [Condition.Disabled],
                },
            },
        };

    private static (BrowserMode Mode, TileSize Tile, Seq<TKey> Keys, HashMap<Key, (TItem Item, int Number)> Entries, Seq<(TItem Item, int Number)> Shown, SKSize Box) Placement<TItem, TKey>(
        Gallery<TItem, TKey> gallery, GalleryState<TItem, TKey> state)
        where TItem : notnull
        where TKey : notnull =>
        (Mode: Mode(gallery.Row, state), Shown: state.Items.Map(static (item, at) => (Item: item, Number: at + 1)).Filter(entry => state.Keep(entry.Item))) switch {
            var held => (state.Resizing | state.Layout.Size).IfNone(() => held.Mode.Fit(state.Width)) switch {
                var tile => (held.Mode, tile, held.Shown.Map(entry => gallery.Row.Key(entry.Item)), toHashMap(held.Shown.Map(static (entry, at) => (Ordinal(at), entry))), held.Shown, Boxed(state, tile)),
            },
        };

    private static Node Tiled<TItem, TKey>(Gallery<TItem, TKey> gallery, GalleryState<TItem, TKey> state, BrowserMode mode, float tile, TItem item, int number)
        where TItem : notnull
        where TKey : notnull =>
        (Key: gallery.Row.Key(item), Label: gallery.Row.Label(gallery.Scope, item)) switch {
            var held => mode.Tiled(
                tile,
                new Node.Overlay(new Node.Box { Role = ImagingRoles.Checker, Width = Grown, Height = Grown }, [
                    state.Tiles.Find(held.Key).Map(static shot => shot.Content).IfNone(static () => new Content<SKImage>.Pending(None, None))
                        .Drawn(static image => new Node.Image(image) { Fit = ImageFit.Contain, Width = Grown, Height = Grown }, RowText.Localize("Thumbnail unavailable").Local),
                ]) { Height = Grown },
                state.Renaming == Some(held.Key)
                    ? new Node.Slot { Key = RenameBox, Role = Role.Field, Width = Grown }
                    : new Node.Text([held.Label]) { Role = mode.Captioned, MaxLines = 1, Width = Grown } switch {
                        var caption => gallery.Row.Rename.IsSome && state.Current == Some(held.Key)
                            ? new Part.Press(_ => Renaming(gallery, Some(held.Key))).Drawn(caption)
                            : caption,
                    },
                gallery.Row.Numbered ? Some<Node>(new Node.Text([number.ToString(RowText.Culture)]) { Role = mode.Captioned }) : None,
                gallery.Row.Marks.Choose(mark => mark.Value.Get(item) switch {
                    var on when on || state.Hovered == Some(held.Key) => Some<Node>(new Part.Press(_ => Revised(gallery, mark.Value.Update(static set => !set, item))).Drawn(
                        new Node.Glyph(on ? mark.On : mark.Off) {
                            Accessible = AccessRole.CheckBox.Marked(new Accessible { Role = AccessRole.CheckBox, Label = RowText.Localize(mark.Caption, gallery.Scope.Sink).Local }, on ? Check.On : Check.Off),
                        })),
                    _ => None,
                })) with {
                Role = HostTheme.ListRow,
                Width = Grown,
                Height = Grown,
                Conditions = state.Current == Some(held.Key) ? [Condition.Modified] : [],
                Accessible = new Accessible {
                    Label = held.Label,
                    Shortcut = gallery.Row.Numbered && number <= 9 ? Some((Keys.D1 + (number - 1)).ToShortcutString()) : None,
                },
            },
        };

    private static Node Sized<TItem, TKey>(Gallery<TItem, TKey> gallery, TileSize tile)
        where TItem : notnull
        where TKey : notnull =>
        new PlotPlane.Track(Orientation.Horizontal, new Axis.Linear(AxisRange.Create((float)TileSize.MinValue, (float)TileSize.MaxValue))) switch {
            var plane => new Handles<float>((float)tile, Points<float>.Scalar(plane, Lens.identity<float>(), static value => Spoken.Shown(Some(value))), change => Resized(gallery, change))
                .Drawn(plane, [new Node.Box { Role = Role.Track, Width = Grown }]) with {
                Width = Grown,
                Height = Sizing.Fit.Content,
                Accessible = new Accessible { Label = RowText.Localize(BrowserLayoutParameter.Size.Text.Caption, gallery.Scope.Sink).Local },
            },
        };

    private static Option<TKey> Resolved<TKey>(Seq<TKey> keys, Key key) =>
        key.Switch(keys, named: static (_, _) => Option<TKey>.None, ordinal: static (held, ordinal) => held.At((int)ordinal.Index), handle: static (_, _) => Option<TKey>.None);

    private static Seq<TKey> Resolved<TKey>(Seq<TKey> keys, Seq<Key> moved) => moved.Choose(key => Resolved(keys, key));

    // --- [TILES]
    private static Option<SKSizeI> Pixels<TItem, TKey>(BrowserRow<TItem, TKey> row, GalleryState<TItem, TKey> state)
        where TItem : notnull
        where TKey : notnull =>
        state.Scale.Map(scale => Boxed(state, state.Layout.Size.IfNone(() => Mode(row, state).Fit(state.Width))) switch {
            var box => new SKSizeI((int)MathF.Ceiling(box.Width * scale), (int)MathF.Ceiling(box.Height * scale)),
        });

    private static IO<Unit> Requested<TItem, TKey>(Gallery<TItem, TKey> gallery)
        where TItem : notnull
        where TKey : notnull =>
        from state in gallery.State.ValueIO
        let wanted = (from pixels in Pixels(gallery.Row, state).ToSeq()
                      from item in state.Items
                      let key = gallery.Row.Key(item)
                      where state.Viewed.Contains(key) && !state.Tiles.Find(key).Exists(tile => tile.Serves(item, state.Revision, pixels))
                      select (Item: item, Key: key, Pixels: pixels)).Strict()
        from loaded in when(!wanted.IsEmpty,
            from marked in Advanced(gallery, held => held with {
                Tiles = wanted.Fold(held.Tiles, (tiles, ask) => tiles.AddOrUpdate(ask.Key, new Tile<TItem>(
                    tiles.Find(ask.Key).Map(static tile => tile.Content.Requested(None)).IfNone(static () => new Content<SKImage>.Pending(None, None)), ask.Item, state.Revision, ask.Pixels))),
            })
            from forked in wanted.TraverseM(ask => DisposalOps.Fork(Reported(gallery, Loaded(gallery, ask.Item, ask.Key, state.Revision, ask.Pixels)), gallery.Loads.Retain)).As()
            select unit).As()
        select unit;

    private static IO<Unit> Loaded<TItem, TKey>(Gallery<TItem, TKey> gallery, TItem item, TKey key, int revision, SKSizeI pixels)
        where TItem : notnull
        where TKey : notnull =>
        (IO.lift(PixelExtent.Validated(pixels.Width, pixels.Height)).Bind(extent => gallery.Row.Tile(gallery.Scope, item, extent)).Map(static image => Fin.Succ(image))
         | @catch(static error => !ErrorOps.Cancellation(error), static error => IO.pure(Fin.Fail<SKImage>(error))))
            .Bind(arrival => Arrived(gallery, item, key, revision, pixels, arrival).OnUiThread());

    private static IO<Unit> Arrived<TItem, TKey>(Gallery<TItem, TKey> gallery, TItem item, TKey key, int revision, SKSizeI pixels, Fin<SKImage> arrival)
        where TItem : notnull
        where TKey : notnull =>
        gallery.State.ValueIO.Map(state => state.Tiles.Find(key).Exists(tile => tile.Serves(item, revision, pixels) && tile.Content is Content<SKImage>.Pending))
            .Bind(accepted => accepted
                ? Advanced(gallery, state => state with { Tiles = state.Tiles.AddOrUpdate(key, new Tile<TItem>(Content<SKImage>.Arrived(arrival), item, revision, pixels)) })
                : IO.lift(() => arrival.Iter(static image => image.Dispose())));

    private static IO<Unit> Reported<TItem, TKey>(Gallery<TItem, TKey> gallery, IO<Unit> work)
        where TItem : notnull
        where TKey : notnull =>
        work | @catch(static error => !ErrorOps.Cancellation(error), error => IO.lift(() => gallery.Scope.Sink.Report(error, typeof(ListRows), nameof(Browser))));

    // --- [CHOICE]
    private static IO<Unit> Selecting<TItem, TKey>(Gallery<TItem, TKey> gallery, Seq<TKey> keys, Edit<Selection> change)
        where TItem : notnull
        where TKey : notnull =>
        from moved in change.Carried.Traverse(value => Advanced(gallery, state => value.Switch(
            (State: state, Keys: keys),
            one: static (held, one) => held.State with {
                Picked = toHashSet(one.Picked.Bind(key => Resolved(held.Keys, key)).ToSeq()),
                Anchor = one.Picked.Bind(key => Resolved(held.Keys, key)),
                Focus = one.Picked.Bind(key => Resolved(held.Keys, key)),
            },
            many: static (held, many) => held.State with {
                Picked = toHashSet(Resolved(held.Keys, toSeq(many.Picked))),
                Anchor = many.Anchor.Bind(key => Resolved(held.Keys, key)),
                Focus = many.Focus.Bind(key => Resolved(held.Keys, key)),
            }))).As()
        from state in gallery.State.ValueIO
        from chosen in change.Settled.Bind(_ => state.Focus.Filter(focus => state.Picked.Count is 1 && state.Picked.Contains(focus) && state.Current != Some(focus)))
            .Traverse(focus => gallery.Wired.ValueIO.Bind(wired => wired.Traverse(held => held.Choose(change.Map(_ => focus).As())).As())).As()
        select unit;

    private static IO<Unit> Reordered<TItem, TKey>(Gallery<TItem, TKey> gallery, Seq<TKey> keys, Edit<Seq<Key>> change)
        where TItem : notnull
        where TKey : notnull =>
        Edited(gallery, change.Map(order => Rearranged(gallery.Row.Key, Resolved(keys, order))).As());

    private static Func<Seq<TItem>, Fin<Seq<TItem>>> Rearranged<TItem, TKey>(Func<TItem, TKey> key, Seq<TKey> order) where TKey : notnull =>
        latest => (Moved: toHashSet(order), Items: toHashMap(latest.Map(item => (key(item), item)))) switch {
            var held => latest.Fold((Placed: Seq<TItem>(), Next: order), (placed, item) => held.Moved.Contains(key(item))
                    ? placed.Next.Head.Bind(held.Items.Find).Match(Some: next => (placed.Placed.Add(next), placed.Next.Tail), None: () => (placed.Placed.Add(item), placed.Next))
                    : (placed.Placed.Add(item), placed.Next))
                .Placed,
        };

    private static IO<Unit> Revised<TItem, TKey>(Gallery<TItem, TKey> gallery, Fin<TItem> item)
        where TItem : notnull
        where TKey : notnull =>
        Edited(gallery, new Edit<Func<Seq<TItem>, Fin<Seq<TItem>>>>.Commit(latest => item.Map(held => Replaced(gallery.Row.Key, None, latest, held))));

    private static IO<Unit> Edited<TItem, TKey>(Gallery<TItem, TKey> gallery, Edit<Func<Seq<TItem>, Fin<Seq<TItem>>>> change)
        where TItem : notnull
        where TKey : notnull =>
        gallery.Wired.ValueIO.Bind(wired => wired.Traverse(held => held.Items(change)).As()).Map(static _ => unit);

    private static IO<Unit> Exported<TItem, TKey>(Gallery<TItem, TKey> gallery, Func<TItem, Option<string>> file, Seq<TKey> moved)
        where TItem : notnull
        where TKey : notnull =>
        gallery.State.ValueIO.Bind(state => toHashSet(moved) switch {
            var keys => Transfer.Drag(gallery.View, state.Items.Filter(item => keys.Contains(gallery.Row.Key(item))).Choose(file), None),
        });

    private static IO<Unit> Menued<TItem, TKey>(Gallery<TItem, TKey> gallery, Seq<TKey> targeted, SKRect frame)
        where TItem : notnull
        where TKey : notnull =>
        from moved in Advanced(gallery, state => state with { Picked = toHashSet(targeted), Anchor = targeted.Head, Focus = targeted.Head })
        from shown in gallery.Wired.ValueIO.Bind(wired => wired.Traverse(held => held.Host.Menu(held.Menu, frame)).As())
        select unit;

    // --- [HOVER]
    private static IO<Unit> Hovered<TItem, TKey>(Gallery<TItem, TKey> gallery, Option<TKey> key)
        where TItem : notnull
        where TKey : notnull =>
        from moved in Advanced(gallery, state => state with { Hovered = key })
        from drained in gallery.Dwell.Drain
        from dwelt in (from preview in gallery.Row.Preview from held in key select (Preview: preview, Key: held)).Traverse(hover =>
            DisposalOps.Acquire(keep => DisposalOps.Fork(Reported(gallery, Dwelled(gallery, hover.Key, hover.Preview)), keep).Map(static _ => unit), (_, lease) => gallery.Dwell.Retain(lease))).As()
        select unit;

    private static IO<Unit> Dwelled<TItem, TKey>(Gallery<TItem, TKey> gallery, TKey key, Func<RowScope, TItem, Action<IDisposable>, IO<Unit>> preview)
        where TItem : notnull
        where TKey : notnull =>
        from delay in Accessors.StepEventsDelay
        from waited in IO.yieldFor(TimeSpan.FromSeconds(delay))
        from shown in (from state in gallery.State.ValueIO
                       from visible in gallery.Scope.Visibility
                       from previewed in (from item in Listed(gallery.Row, state, key) where visible && state.Enabled && state.Hovered == Some(key) select item)
                           .Traverse(item => preview(gallery.Scope, item, gallery.Dwell.Retain)).As()
                       select unit).OnUiThread()
        select unit;

    // --- [RENAME]
    private static IO<Unit> Renaming<TItem, TKey>(Gallery<TItem, TKey> gallery, Option<TKey> key)
        where TItem : notnull
        where TKey : notnull =>
        gallery.Editing.Drain.Bind(_ => Advanced(gallery, state => state with { Renaming = key.Filter(_ => gallery.Row.Rename.IsSome) }));

    private static IO<Control> Renamer<TItem, TKey>(Gallery<TItem, TKey> gallery)
        where TItem : notnull
        where TKey : notnull =>
        from state in gallery.State.ValueIO
        from item in IO.lift(state.Renaming.Bind(key => Listed(gallery.Row, state, key)).ToFin(new Refused(nameof(SceneView.Slots), Refused.Absent)))
        from box in IO.lift(() => new TextBox { Text = gallery.Row.Label(gallery.Scope, item) })
        from keyed in Subscriptions.Attach(h => box.KeyDown += h, h => box.KeyDown -= h,
            Callbacks.Synchronous<KeyEventArgs>(args => Typed(gallery, box, args), new CallbackSite(gallery.Scope.Sink, typeof(TextBox), nameof(Control.KeyDown))), gallery.Editing.Retain)
        from lost in Subscriptions.Attach(h => box.LostFocus += h, h => box.LostFocus -= h,
            Callbacks.Handler<EventArgs>(_ => Renamed(gallery, box.Text), new CallbackSite(gallery.Scope.Sink, typeof(TextBox), nameof(Control.LostFocus))), gallery.Editing.Retain)
        from loaded in Subscriptions.Attach(h => box.LoadComplete += h, h => box.LoadComplete -= h,
            Callbacks.Handler<EventArgs>(_ => IO.lift(() => {
                box.Focus();
                box.SelectAll();
            }), new CallbackSite(gallery.Scope.Sink, typeof(TextBox), nameof(Control.LoadComplete))), gallery.Editing.Retain)
        select (Control)box;

    private static IO<Unit> Typed<TItem, TKey>(Gallery<TItem, TKey> gallery, TextBox box, KeyEventArgs args)
        where TItem : notnull
        where TKey : notnull =>
        args.KeyData switch {
            Keys.Enter => IO.lift(() => { args.Handled = true; }).Bind(_ => Renamed(gallery, box.Text)).Bind(_ => IO.lift(gallery.View.Focus)),
            Keys.Escape => IO.lift(() => { args.Handled = true; }).Bind(_ => Renaming(gallery, None)).Bind(_ => IO.lift(gallery.View.Focus)),
            _ => IO.pure(unit),
        };

    private static IO<Unit> Renamed<TItem, TKey>(Gallery<TItem, TKey> gallery, string text)
        where TItem : notnull
        where TKey : notnull =>
        from state in gallery.State.ValueIO
        from closed in Renaming(gallery, None)
        from committed in (from key in state.Renaming
                           from item in Listed(gallery.Row, state, key)
                           from rename in gallery.Row.Rename
                           where !string.Equals(text, gallery.Row.Label(gallery.Scope, item), StringComparison.Ordinal)
                           select Revised(gallery, rename(item, text))).Traverse(static revised => revised).As()
        select unit;

    // --- [KEYS]
    private static IO<Unit> Pressed<TItem, TKey>(Gallery<TItem, TKey> gallery, Func<Keys, IO<bool>> keyed, KeyEventArgs args)
        where TItem : notnull
        where TKey : notnull =>
        args.KeyData switch {
            Keys.F2 when gallery.Row.Rename.IsSome => IO.lift(() => { args.Handled = true; }).Bind(_ => gallery.State.ValueIO).Bind(state => Renaming(gallery, state.Focus)),
            var key => Keyed(keyed, key, args),
        };

    // --- [SIZE]
    private static IO<Unit> Resized<TItem, TKey>(Gallery<TItem, TKey> gallery, Edit<float> change)
        where TItem : notnull
        where TKey : notnull =>
        change.Switch(
            gallery,
            preview: static (held, preview) => Advanced(held, state => state with { Resizing = Some(TileSize.Clamped(preview.Value)) }),
            step: static (held, step) => Stored(held, TileSize.Clamped(step.Value)),
            commit: static (held, commit) => Stored(held, TileSize.Clamped(commit.Value)),
            cancel: static (held, _) => Advanced(held, static state => state with { Resizing = None }));

    private static IO<Unit> Stored<TItem, TKey>(Gallery<TItem, TKey> gallery, TileSize size)
        where TItem : notnull
        where TKey : notnull =>
        from moved in Advanced(gallery, state => state with { Resizing = None, Layout = state.Layout with { Size = Some(size) } })
        from written in Written(gallery.Scope, held => held with { Size = Some(size) })
        select unit;

    private static IO<Unit> Written(RowScope scope, Func<BrowserLayout, BrowserLayout> next) =>
        from held in scope.Read(Layout)
        from stored in scope.Store(Layout).Write(Some(next(held)))
        from changed in scope.Changed(Named(Layout, scope))
        select unit;

    // --- [CURRENT]
    private static IO<(Func<Edit<TKey>, IO<Unit>> Choose, IO<Unit> Shown)> Current<TItem, TKey>(Gallery<TItem, TKey> gallery, CallbackSite site, Action<IDisposable> retain)
        where TItem : notnull
        where TKey : notnull =>
        from pending in atomIO(Option<TKey>.None)
        from timer in DisposalOps.Acquire(IO.lift(static () => new UITimer()), retain)
        let shown = gallery.Row.Current(gallery.Scope).Bind(current => Advanced(gallery, state => state with { Current = current }))
        let settled = Settled(gallery.Row, gallery.Scope, timer, pending).Bind(_ => shown)
        from closing in DisposalOps.Acquire(IO.pure(unit), _ => settled, retain)
        from elapsed in Subscriptions.Attach(h => timer.Elapsed += h, h => timer.Elapsed -= h, Callbacks.Handler<EventArgs>(_ => settled, site), retain)
        from changed in gallery.Scope.Listen(_ => shown, retain)
        select ((Func<Edit<TKey>, IO<Unit>>)(change => change.Switch(
            (Timer: timer, Pending: pending, Shown: shown, Settled: settled),
            preview: static (_, _) => IO.pure(unit),
            step: static (held, step) =>
                from delay in Accessors.StepEventsDelay
                from queued in writeIO(held.Pending, Some(step.Value))
                from started in IO.lift(() => {
                    held.Timer.Interval = delay;
                    held.Timer.Start();
                })
                select unit,
            commit: static (held, commit) => writeIO(held.Pending, Some(commit.Value)).Bind(_ => held.Settled),
            cancel: static (held, _) => IO.lift(held.Timer.Stop).Bind(_ => writeIO(held.Pending, Option<TKey>.None)).Bind(_ => held.Shown))), shown);

    private static IO<Unit> Settled<TItem, TKey>(BrowserRow<TItem, TKey> row, RowScope scope, UITimer timer, Atom<Option<TKey>> pending)
        where TItem : notnull
        where TKey : notnull =>
        DisposalOps.Transacted(
            IO.lift(timer.Stop).Bind(_ => pending.ValueIO),
            held => held.Traverse(key => row.Choose(scope, key)).As(),
            static _ => unitIO,
            (held, _) => when(held.IsSome, writeIO(pending, Option<TKey>.None).Map(unit)).As());

    // --- [NODES]
    private sealed record ListState<TItem>(Seq<RowItem<TItem>> Nodes, TreeGridItemCollection Store, Seq<(RowItem<TItem> Node, TItem Row)> Reloads) where TItem : notnull;

    private static Seq<RowItem<TItem>> Built<TItem, TKey>(
        ListRow<TItem, TKey> row, Seq<TItem> items, LanguageExt.HashSet<TKey> open, Func<TItem, bool> keep, int depth)
        where TItem : notnull
        where TKey : notnull =>
        Ordered(row.Ordered, items)
            .Map(item => (Item: item, Kids: Built(row, row.Branches.ToSeq().Bind(branches => branches.Children.Get(item)), open, keep, depth + 1)))
            .Filter(pair => !pair.Kids.IsEmpty || keep(pair.Item))
            .Map(pair => new RowItem<TItem>(pair.Item, depth, pair.Kids) { Expanded = open.Contains(row.Key(pair.Item)) });

    private static (Seq<RowItem<TItem>> Preorder, Seq<RowItem<TItem>> Visible, Seq<(TKey Key, int Depth)> Outline, LanguageExt.HashSet<TKey> Open) Folded<TItem, TKey>(
        ListRow<TItem, TKey> row, Seq<RowItem<TItem>> nodes)
        where TItem : notnull
        where TKey : notnull =>
        nodes.Fold(
            (Preorder: Seq<RowItem<TItem>>(), Visible: Seq<RowItem<TItem>>(), Outline: Seq<(TKey Key, int Depth)>(), Open: LanguageExt.HashSet<TKey>.Empty),
            (held, node) => Folded(row, node.Kids) switch {
                var kids => (
                    held.Preorder.Add(node) + kids.Preorder,
                    held.Visible.Add(node) + (node.Expanded ? kids.Visible : Seq<RowItem<TItem>>()),
                    held.Outline.Add((row.Key(node.Row), node.Depth)) + kids.Outline,
                    (node.Expanded ? held.Open.Add(row.Key(node.Row)) : held.Open).Union(kids.Open)),
            });

    private static Seq<TItem> Ordered<TItem>(Option<Func<TItem, string>> ordered, Seq<TItem> items) =>
        ordered.Match(Some: name => toSeq(items.OrderBy(name, Comparer<string>.Create(Localization.LogicalSort))), None: () => items);

    private static Seq<RowItem<TItem>> Chosen<TItem>(Seq<RowItem<TItem>> visible, LanguageExt.HashSet<int> rows) where TItem : notnull =>
        visible.Choose((index, node) => Callbacks.Found(rows.Contains(index), node));

    private static Seq<int> Indices<TItem, TKey>(ListRow<TItem, TKey> row, Seq<RowItem<TItem>> visible, LanguageExt.HashSet<TKey> keys)
        where TItem : notnull
        where TKey : notnull =>
        visible.Choose((index, node) => Callbacks.Found(keys.Contains(row.Key(node.Row)), index));

    private static Option<RowItem<TItem>> Resolved<TItem, TKey>(ListRow<TItem, TKey> row, Seq<RowItem<TItem>> nodes, object item)
        where TItem : notnull
        where TKey : notnull =>
        Folded(row, nodes).Preorder.Find(node => ReferenceEquals(node, item));

    // --- [SELECTION]
    private static IO<(Seq<EntryKey> Keys, IO<Unit> Shown)> Selected<TItem, TKey>(
        TreeGridView grid, Atom<ListState<TItem>> list, ListRow<TItem, TKey> row, RowScope scope, Action<IDisposable> retain)
        where TItem : notnull
        where TKey : notnull =>
        RowEdit.Bind(row.Selected.Source, IterableNE.create(row.Selected.Field),
                h => grid.SelectionChanged += h, h => grid.SelectionChanged -= h,
                edit => Callbacks.Handler<EventArgs>(
                    _ => from keys in IO.lift(() => toHashSet(from node in Chosen(Folded(row, list.Value.Nodes).Visible, toHashSet(grid.SelectedRows)) select row.Key(node.Row)))
                         from committed in edit.Commit(keys)
                         select unit,
                    new CallbackSite(scope.Sink, typeof(TreeGridView), nameof(TreeGridView.SelectionChanged))),
                agreed => Reselected(grid, list, row, agreed),
                scope, new CallbackSite(scope.Sink, typeof(TreeGridView), nameof(TreeGridView.SelectedRows)), retain)
            .Map(static bound => (bound.Keys, bound.Shown));

    private static IO<Unit> Reselected<TItem, TKey>(TreeGridView grid, Atom<ListState<TItem>> list, ListRow<TItem, TKey> row, Option<LanguageExt.HashSet<TKey>> agreed)
        where TItem : notnull
        where TKey : notnull =>
        from state in list.ValueIO
        from stored in IO.lift(() => ReferenceEquals(grid.DataStore, state.Store))
        from placed in unless(stored, IO.lift(fun(() => { grid.DataStore = state.Store; }))).As()
        from rows in IO.lift(() => toHashSet(grid.SelectedRows))
        from wanted in IO.lift(() => agreed.ToSeq().Bind(keys => Indices(row, Folded(row, state.Nodes).Visible, keys)))
        from selected in when(toHashSet(wanted) != rows, IO.lift(fun(() => { grid.SelectedRows = wanted; }))).As()
        select unit;

    // --- [FILL]
    private static IO<(Seq<EntryKey> Keys, IO<Unit> Shown)> Items<TItem, TKey>(
        TreeGridView grid, Atom<ListState<TItem>> list, Atom<Func<TItem, bool>> keep, ListRow<TItem, TKey> row, IO<Unit> reselect, RowScope scope, Action<IDisposable> retain)
        where TItem : notnull
        where TKey : notnull =>
        RowEdit.Bind(row.Items.Source, IterableNE.create(row.Items.Field),
                h => grid.CellEdited += h, h => grid.CellEdited -= h,
                edit => Callbacks.Handler<GridViewCellEventArgs>(
                    args => from found in IO.lift(() => Resolved(row, list.Value.Nodes, args.Item))
                            from stored in scope.Read(row.Items.Source)
                            from committed in found.Match(Some: node => edit.Commit(Replaced(row.Key, row.Branches, stored, node.Row)).Map(unit), None: static () => IO.pure(unit))
                            select unit,
                    new CallbackSite(scope.Sink, typeof(TreeGridView), nameof(TreeGridView.CellEdited))),
                agreed => Fill(grid, list, keep, row, reselect, agreed.ToSeq().Flatten()),
                scope, new CallbackSite(scope.Sink, typeof(TreeGridView), nameof(TreeGridView.DataStore)), retain)
            .Map(static bound => (bound.Keys, bound.Shown));

    private static IO<Unit> Fill<TItem, TKey>(
        TreeGridView grid, Atom<ListState<TItem>> list, Atom<Func<TItem, bool>> keep, ListRow<TItem, TKey> row, IO<Unit> reselect, Seq<TItem> items)
        where TItem : notnull
        where TKey : notnull =>
        from kept in keep.ValueIO
        from next in list.SwapIO(state => Filled(row, state, items, kept))
        from reloaded in next.Reloads.TraverseM(reload => IO.lift(fun(() => {
            reload.Node.Row = reload.Row;
            grid.ReloadItem(reload.Node, reloadChildren: false);
        }))).As()
        from stored in IO.lift(() => ReferenceEquals(grid.DataStore, next.Store))
        from reselected in unless(stored, reselect).As()
        select unit;

    private static ListState<TItem> Filled<TItem, TKey>(ListRow<TItem, TKey> row, ListState<TItem> state, Seq<TItem> items, Func<TItem, bool> keep)
        where TItem : notnull
        where TKey : notnull =>
        Folded(row, state.Nodes) switch {
            var before => Built(row, items, before.Open, keep, 0) switch {
                var after => Folded(row, after) switch {
                    var shaped when shaped.Outline == before.Outline => state with {
                        Reloads = before.Preorder.Zip(shaped.Preorder)
                            .Filter(static pair => !EqualityComparer<TItem>.Default.Equals(pair.First.Row, pair.Second.Row))
                            .Map(static pair => (pair.First, pair.Second.Row)),
                    },
                    _ => new ListState<TItem>(after, new TreeGridItemCollection(after.Map<ITreeGridItem>(static node => node)), []),
                },
            },
        };

    private static IO<Unit> Columns<TItem, TKey>(ListRow<TItem, TKey> row, TreeGridView grid, IPlugInSink sink, Action<IDisposable> retain)
        where TItem : notnull
        where TKey : notnull =>
        toSeq(row.Columns).Map(static (column, index) => (Column: column, Index: index)).TraverseM(entry =>
            from cell in CellFor(entry.Column, sink, retain)
            from column in DisposalOps.Acquire(IO.lift(static () => new GridColumn()), retain)
            from added in IO.lift(() => {
                (column.HeaderText, column.DataCell, column.Editable, column.Resizable, column.Expand) =
                    (Conversions.Unset(entry.Column.Caption.Map(caption => RowText.Localize(caption, sink).Local)), cell, entry.Column.Edits, false,
                     entry.Column.Stretches && toSeq(row.Columns).Skip(entry.Index + 1).ForAll(static later => !later.Stretches));
                grid.Columns.Add(column);
            })
            select unit).As().Map(unit);

    // --- [EDITS]
    private static Seq<TItem> Replaced<TItem, TKey>(Func<TItem, TKey> key, Option<ListBranches<TItem, TKey>> branches, Seq<TItem> items, TItem edited)
        where TItem : notnull
        where TKey : notnull =>
        items.Map(item => EqualityComparer<TKey>.Default.Equals(key(item), key(edited))
            ? edited
            : branches.Fold(item, (held, tree) => tree.Children.Update(kids => Replaced(key, branches, kids, edited), held)));

    private static IO<Cell> CellFor<TItem>(ListColumn<TItem> column, IPlugInSink sink, Action<IDisposable> retain) where TItem : notnull =>
        IconSlot.Small.Size().Bind(size => HostTheme.Scale.Bind(scale => column.Switch((Sink: sink, Retain: retain, Size: new Size(size, size), Scale: scale),
            label: static (held, label) => label.Icon.Match(
                Some: icon =>
                    from cell in DisposalOps.Acquire(IO.lift(static () => new ImageTextCell()), held.Retain)
                    from bound in IO.lift(() => { (cell.ImageBinding, cell.TextBinding) = (Pictured(icon, held.Size, held.Scale), Written(label)); })
                    select (Cell)cell,
                None: () =>
                    from cell in DisposalOps.Acquire(IO.lift(static () => new TextBoxCell()), held.Retain)
                    from bound in IO.lift(() => cell.Binding = Written(label))
                    select (Cell)cell),
            check: static (held, check) =>
                from cell in DisposalOps.Acquire(IO.lift(static () => new CheckBoxCell()), held.Retain)
                from bound in IO.lift(() => cell.Binding = Binding.Delegate<RowItem<TItem>, bool?>(node => check.Value.Get(node.Row), (node, value) => node.Row = check.Value.Set(value is true, node.Row)))
                select (Cell)cell,
            glyph: static (held, glyph) =>
                from cell in DisposalOps.Acquire(IO.lift(static () => new ImageViewCell()), held.Retain)
                from bound in IO.lift(() => cell.Binding = Pictured(glyph.Icon, held.Size, held.Scale))
                select (Cell)cell,
            progress: static (held, progress) =>
                from cell in DisposalOps.Acquire(IO.lift(static () => new ProgressCell()), held.Retain)
                from bound in IO.lift(() => cell.Binding = Binding.Delegate<RowItem<TItem>, float?>(node => progress.Read(node.Row).ToNullable()))
                select (Cell)cell)));

    private static IndirectBinding<Image> Pictured<TItem>(Func<TItem, GlyphSource> icon, Size logicalSize, float scale) where TItem : notnull =>
        Binding.Delegate<RowItem<TItem>, Image>(node => Icons.Frame(icon(node.Row), logicalSize, scale));

    private static IndirectBinding<string> Written<TItem>(ListColumn<TItem>.Label label) where TItem : notnull =>
        Binding.Delegate<RowItem<TItem>, string>(
            node => label.Read(node.Row),
            (node, value) => _ = label.Rename.Bind(rename => rename(node.Row, value).ToOption()).Iter(item => node.Row = item));

    // --- [EXPANSION]
    private static IO<(Seq<EntryKey> Keys, IO<Unit> Shown)> Expansion<TItem, TKey>(
        TreeGridView grid, Atom<ListState<TItem>> list, ListRow<TItem, TKey> row, ListBranches<TItem, TKey> branches, RowScope scope, Action<IDisposable> retain)
        where TItem : notnull
        where TKey : notnull =>
        RowEdit.Bind(branches.Expanded.Source, IterableNE.create(branches.Expanded.Field),
                h => Callbacks.Each(Seq(IO.lift(() => grid.Expanded += h), IO.lift(() => grid.Collapsed += h))).Run(),
                h => Callbacks.Each(Seq(IO.lift(() => grid.Expanded -= h), IO.lift(() => grid.Collapsed -= h))).Run(),
                edit => Callbacks.Handler<TreeGridViewItemEventArgs>(
                    _ => from state in list.ValueIO
                         from committed in edit.Commit(Folded(row, state.Nodes).Open)
                         select unit,
                    new CallbackSite(scope.Sink, typeof(TreeGridView), nameof(TreeGridView.Expanded))),
                open => Expand(grid, list, row, open),
                scope, new CallbackSite(scope.Sink, typeof(TreeGridView), nameof(TreeGridView.ReloadItem)), retain)
            .Map(static bound => (bound.Keys, bound.Shown));

    private static IO<Unit> Expand<TItem, TKey>(TreeGridView grid, Atom<ListState<TItem>> list, ListRow<TItem, TKey> row, Option<LanguageExt.HashSet<TKey>> open)
        where TItem : notnull
        where TKey : notnull =>
        from state in list.ValueIO
        from toggled in Folded(row, state.Nodes).Preorder
            .Filter(node => node.Expanded != open.Exists(keys => keys.Contains(row.Key(node.Row))))
            .TraverseM(node => IO.lift(fun(() => {
                node.Expanded = !node.Expanded;
                grid.ReloadItem(node, reloadChildren: true);
            })))
            .As()
        select unit;

    // --- [GESTURES]
    private static IO<Unit> Gestures<TItem, TKey>(
        TreeGridView grid, Atom<ListState<TItem>> list, ListRow<TItem, TKey> row, Font emphasis, Func<Keys, IO<bool>> keyed, RowScope scope, Action<IDisposable> retain)
        where TItem : notnull
        where TKey : notnull =>
        Callbacks.Each(Seq(
            Subscriptions.Attach(h => grid.KeyDown += h, h => grid.KeyDown -= h,
                Callbacks.Synchronous<KeyEventArgs>(args => Pressed(grid, list, row, keyed, args), new CallbackSite(scope.Sink, typeof(TreeGridView), nameof(Control.KeyDown))), retain),
            Subscriptions.Attach(h => grid.MouseDown += h, h => grid.MouseDown -= h,
                Callbacks.Synchronous<MouseEventArgs>(args => Pointed(grid, list, row, args), new CallbackSite(scope.Sink, typeof(TreeGridView), nameof(Control.MouseDown))), retain),
            Subscriptions.Attach(h => grid.CellFormatting += h, h => grid.CellFormatting -= h,
                Callbacks.Synchronous<GridCellFormatEventArgs>(args => Formatted(list, row, emphasis, args), new CallbackSite(scope.Sink, typeof(TreeGridView), nameof(TreeGridView.CellFormatting))), retain),
            HostTheme.In(grid).Inline(_ => IO.lift(grid.ReloadData), scope.Sink, retain))
            + row.Activate.Map(activate => Subscriptions.Attach(h => grid.Activated += h, h => grid.Activated -= h,
                Callbacks.Handler<TreeGridViewItemEventArgs>(_ =>
                    from head in IO.lift(() => Chosen(Folded(row, list.Value.Nodes).Visible, toHashSet(grid.SelectedRows)).Head)
                    from ran in head.Match(Some: node => activate(node.Row), None: static () => IO.pure(unit))
                    select ran, new CallbackSite(scope.Sink, typeof(TreeGridView), nameof(TreeGridView.Activated))), retain)).ToSeq()).Map(unit);

    private static IO<Unit> Pressed<TItem, TKey>(
        TreeGridView grid, Atom<ListState<TItem>> list, ListRow<TItem, TKey> row, Func<Keys, IO<bool>> keyed, KeyEventArgs args)
        where TItem : notnull
        where TKey : notnull =>
        from rows in IO.lift(() => toSeq(grid.SelectedRows).Strict())
        from shown in IO.lift(() => !Folded(row, list.Value.Nodes).Visible.IsEmpty)
        from handled in args.KeyData switch {
            Keys.F2 => Handled(args,
                from at in rows.Head
                from renaming in toSeq(row.Columns).Map(static (each, index) => (Column: each, Index: index)).Find(static entry => entry.Column.Renames)
                select (Action)(() => grid.BeginEdit(at, renaming.Index))),
            Keys.Up or Keys.Down when rows.IsEmpty && shown => Handled(args, Some(() => grid.SelectRow(0))),
            var key => Keyed(keyed, key, args),
        }
        select handled;

    private static IO<Unit> Keyed(Func<Keys, IO<bool>> keyed, Keys key, KeyEventArgs args) =>
        keyed(key).Bind(handled => IO.lift(() => { args.Handled = handled; }));

    private static IO<Unit> Handled(KeyEventArgs args, Option<Action> action) =>
        IO.lift(fun(() => {
            _ = action.Iter(static run => run());
            args.Handled = action.IsSome;
        }));

    private static IO<Unit> Pointed<TItem, TKey>(TreeGridView grid, Atom<ListState<TItem>> list, ListRow<TItem, TKey> row, MouseEventArgs args)
        where TItem : notnull
        where TKey : notnull =>
        when(args.Buttons is MouseButtons.Alternate,
            from pressed in IO.lift(() => (grid.GetCellAt(args.Location).Item, Rows: toHashSet(grid.SelectedRows)))
            from found in IO.lift(() => Folded(row, list.Value.Nodes).Visible
                .Choose((index, node) => Callbacks.Found(ReferenceEquals(node, pressed.Item) && !pressed.Rows.Contains(index), index))
                .Head)
            from selected in IO.lift(() => found.Iter(at => grid.SelectedRows = [at]))
            select selected).As();

    private static IO<Unit> Formatted<TItem, TKey>(Atom<ListState<TItem>> list, ListRow<TItem, TKey> row, Font emphasis, GridCellFormatEventArgs args)
        where TItem : notnull
        where TKey : notnull =>
        IO.lift(() => Resolved(row, list.Value.Nodes, args.Item).Iter(node =>
            (args.Font, args.ForegroundColor) = (
                node.Kids.IsEmpty ? args.Font : emphasis,
                row.Available.ForAll(available => available(node.Row)) ? args.ForegroundColor : PaintSlot.TextDisabled.Read())));

    // --- [COMMANDS]
    private static IO<(Option<Control> Pair, Seq<Seq<Command<RowRule, Invoke>>> Menu, Func<Keys, IO<bool>> Keyed, IO<Unit> Refresh)> Commanded(
        Option<(Command<RowRule, Invoke>.Run Add, Command<RowRule, Invoke>.Run Remove)> pair, Seq<Command<RowRule, Invoke>> commands, RowScope scope, Action<IDisposable> retain) =>
        from paired in pair.Traverse(held => ButtonRows.AddRemove(held.Add, held.Remove, scope, retain)).As()
        let menu = pair.Map(static held => Seq<Command<RowRule, Invoke>>(held.Add, held.Remove)).ToSeq().Add(commands)
        from keyed in CommandRows.Keyed(menu, scope, retain)
        select (paired.Map(static held => (Control)held.Control), menu, keyed, paired.Map(static held => held.Refresh).IfNone(IO.pure(unit)));

    // --- [PLACEMENT]
    private static IO<Control> Emptied<TItem>(TreeGridView grid, Atom<ListState<TItem>> list, string text, IPlugInSink sink, Action<IDisposable> retain) where TItem : notnull =>
        from label in DisposalOps.Acquire(IO.lift(() => new Eto.Forms.Label { Text = text, TextColor = SystemColors.DisabledText, TextAlignment = Eto.Forms.TextAlignment.Center }), retain)
        from stack in Containers.Frame(static () => new StackLayout { HorizontalContentAlignment = HorizontalAlignment.Stretch }, retain)
        let shown = fun((ListState<TItem> state) => IO.lift(() => (grid.Visible, label.Visible) = (!state.Nodes.IsEmpty, state.Nodes.IsEmpty)).Map(unit))
        from placed in IO.lift(() => {
            stack.Items.Add(new StackLayoutItem(grid, expand: true));
            stack.Items.Add(label);
        })
        from heard in Subscriptions.Changed(list, nameof(list.Change)).Inline(shown, sink, retain)
        from current in list.ValueIO.Bind(shown)
        select (Control)stack;

    private static IO<Control> Paired(Control content, Option<Control> pair, Action<IDisposable> retain) =>
        pair.Match(Some: buttons => Stacked(retain, new StackLayoutItem(content, expand: true), new StackLayoutItem(buttons, HorizontalAlignment.Left)), None: () => IO.pure(content));

    private static IO<Control> Stacked(Action<IDisposable> retain, params Seq<StackLayoutItem> items) =>
        Containers.Frame(() => new StackLayout([.. items]) {
            HorizontalContentAlignment = HorizontalAlignment.Stretch,
            Spacing = RhinoLayout.StackedSpacing(Eto.Forms.Orientation.Vertical, RhinoLayout.SpacingType.Panel),
        }, retain).Map(static stack => (Control)stack);

    private static IO<(Control Control, Seq<(Seq<EntryKey> Keys, IO<Unit> Shown)> Brackets)> Searched<TItem>(
        Control content, Option<ListSearch<TItem>> search, Func<Func<TItem, bool>, IO<Unit>> keep, RowScope scope, Action<IDisposable> retain) where TItem : notnull =>
        search.Match(Some: held =>
            from field in ChoiceRows.Search(RowText.Localize(held.Placeholder, scope.Sink).Local, matches => keep(item => matches(held.Text(scope, item))), scope.Sink, retain)
            from shown in held.Shown.Traverse(source => Watched(source, Named(source, scope), visible => IO.lift(() => { field.Visible = visible; }), scope,
                new CallbackSite(scope.Sink, typeof(SearchBox), nameof(SearchBox.Visible)), retain)).As()
            from stack in Stacked(retain, new StackLayoutItem(field), new StackLayoutItem(content, expand: true))
            select (stack, shown.ToSeq()),
            None: () => IO.pure((content, Seq<(Seq<EntryKey> Keys, IO<Unit> Shown)>())));

    private static IO<(Seq<EntryKey> Keys, IO<Unit> Shown)> Watched<TValue>(
        RowSource<TValue> source, Seq<EntryKey> keys, Func<TValue, IO<Unit>> show, RowScope scope, CallbackSite site, Action<IDisposable> retain) where TValue : notnull =>
        from signals in source.Signals(scope)
        let shown = scope.Read(source).Bind(show)
        from held in Callbacks.Each(signals.Map(signal => signal.Choose(static _ => Some((unit, unit))).Through(Subscriptions.Idle<Unit, Unit>(_ => shown), site.Sink, retain)))
        select (keys, shown);

    private static Seq<EntryKey> Named(RowSource source, RowScope scope) =>
        source.Values(scope).Paths().Map(path => new EntryKey(source.Owner, path));

    private static IO<Unit> Listened(Seq<(Seq<EntryKey> Keys, IO<Unit> Shown)> brackets, RowScope scope, Action<IDisposable> retain) =>
        scope.Listen(changed => Callbacks.Each(brackets.Filter(bracket => bracket.Keys.Exists(key => changed.Exists(held => held == key))).Map(static bracket => bracket.Shown)).Map(unit), retain);
}
