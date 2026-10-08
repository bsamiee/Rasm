using Eto.Drawing;
using Eto.Forms;
using Rasm.Imaging.Pixels;
using Rasm.Rhino.Events;
using Rasm.Rhino.UI.Assets;
using Rasm.Rhino.UI.Components;
using Rhino.Resources;

namespace Rasm.Rhino.UI.Viewers;

// --- [MODELS] --------------------------------------------------------------------------
[ValueObject<float>(SkipIParsable = true, AdditionOperators = OperatorsGeneration.None, SubtractionOperators = OperatorsGeneration.None, MultiplyOperators = OperatorsGeneration.None, DivisionOperators = OperatorsGeneration.None)]
[ValidationError<InvalidRhinoValue>]
public readonly partial struct TileSize : System.Numerics.IMinMaxValue<TileSize> {
    public static TileSize MinValue { get; } = new(32f);
    public static TileSize MaxValue { get; } = new(256f);
    public static TileSize List { get; } = new(48f);
    public static Presentation<TileSize, float> Presentation { get; } = new() { Step = 1f, Decimals = 0 };

    public static TileSize Clamped(float width) => new(float.Clamp(width, MinValue._value, MaxValue._value));

    static partial void ValidateFactoryArguments(ref InvalidRhinoValue? validationError, ref float value) =>
        validationError = value.CompareTo(MinValue._value) >= 0 && value.CompareTo(MaxValue._value) <= 0 ? null : new InvalidRhinoValue();
}

public readonly record struct TileFlow(int Columns, SizeF Cell) {
    public const float Spacing = 1f;

    public RectangleF Slot(int index) =>
        new(Spacing + (index % Columns * (Cell.Width + Spacing)), Spacing + (index / Columns * (Cell.Height + Spacing)), Cell.Width, Cell.Height);

    public float Height(int slots) => Spacing + ((slots + Columns - 1) / Columns * (Cell.Height + Spacing));

    public int Row(float y) => (int)MathF.Floor((y - Spacing) / (Cell.Height + Spacing));

    public (int First, int Count) Within(RectangleF view) =>
        (int.Max(0, Row(view.Top)), Row(view.Bottom)) switch {
            var (top, bottom) => (top * Columns, int.Max(0, bottom - top + 1) * Columns),
        };
}

public readonly record struct TileFace(RectangleF Image, RectangleF Caption, RectangleF Band, Option<RectangleF> Number, FormattedTextAlignment Align, bool Ruled) {
    public const float Inset = 4f;

    public RectangleF Star =>
        IconSlot.ListCell.Master.Points switch {
            var side => new RectangleF(Image.Right - Inset - side, Image.Top + Inset, side, side),
        };
}

[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
[KeyMemberComparer<ComparerAccessors.StringOrdinal, string>]
[ValidationError<InvalidRhinoValue>]
public sealed partial class BrowserMode {
    private const float LabelRow = 20f;

    public static readonly BrowserMode Grid = new("grid", GlyphRole.Grid,
        static (width, image) => new TileFlow(int.Max(1, (int)((width - TileFlow.Spacing) / (image.Width + TileFlow.Spacing))), new SizeF(image.Width, image.Height + LabelRow)),
        static (cell, image, _) => new RectangleF(cell.X, cell.Y + image.Height, image.Width, LabelRow) switch {
            var caption => new TileFace(new RectangleF(cell.Location, image), caption, caption, Option<RectangleF>.None, FormattedTextAlignment.Center, Ruled: true),
        },
        static (pointer, flow) =>
            (flow.Row(pointer.Y) * flow.Columns) + int.Clamp((int)MathF.Round((pointer.X - TileFlow.Spacing) / (flow.Cell.Width + TileFlow.Spacing), MidpointRounding.ToEven), 0, flow.Columns),
        static width => TileSize.Clamped(((width - TileFlow.Spacing) / 3f) - TileFlow.Spacing));
    public static readonly BrowserMode List = new("list", GlyphRole.List,
        static (width, image) => new TileFlow(1, new SizeF(width - (2f * TileFlow.Spacing), image.Height + (2f * TileFace.Inset))),
        static (cell, image, number) => (cell.X + TileFace.Inset + image.Width + 8f) switch {
            var start => new TileFace(
                new RectangleF(cell.X + TileFace.Inset, cell.Y + TileFace.Inset, image.Width, image.Height),
                RectangleF.FromSides(number > 0f ? start + number + 5f : start, cell.Y, cell.Right - TileFace.Inset, cell.Bottom),
                cell,
                number > 0f ? Some(new RectangleF(start, cell.Y, number, cell.Height)) : Option<RectangleF>.None,
                FormattedTextAlignment.Left,
Ruled: false),
        },
        static (pointer, flow) => (int)MathF.Round((pointer.Y - TileFlow.Spacing) / (flow.Cell.Height + TileFlow.Spacing), MidpointRounding.ToEven),
        static _ => TileSize.List);

    public GlyphRole Glyph { get; }

    public string Caption => Map(grid: RowText.Localize("Grid").Local, list: RowText.Localize("List").Local);

    [UseDelegateFromConstructor]
    public partial TileFlow Flow(float width, SizeF image);

    [UseDelegateFromConstructor]
    public partial TileFace Face(RectangleF cell, SizeF image, float number);

    [UseDelegateFromConstructor]
    public partial int Insertion(PointF pointer, TileFlow flow);

    [UseDelegateFromConstructor]
    public partial TileSize Fit(float width);
}

public sealed record BrowserSource<TItem, TKey>(Func<TItem, TKey> Key, Func<TItem, string> Label, Func<Control, TItem, Size, IO<Disposal<Thumbnail>>> Tile, string Empty)
    where TItem : notnull where TKey : notnull {
    public bool Numbered { get; init; }
    public Option<Func<TItem, bool>> Favourite { get; init; }
    public Option<Func<Option<TItem>, IO<Unit>>> Preview { get; init; }
    public Option<Func<Control, Seq<TItem>, Option<Thumbnail>, IO<Unit>>> Export { get; init; }
    public bool Renames { get; init; }
    public bool Reorders { get; init; }
}

public sealed record DragGap<TKey>(Seq<TKey> Moved, int Before) where TKey : notnull;

public sealed record BrowserState<TItem, TKey>(Seq<TItem> Items, Option<TKey> Current, Option<TileSize> Size, BrowserMode Mode, PixelExtent Extent)
    where TItem : notnull where TKey : notnull {
    public Func<TItem, bool> Filter { get; init; } = static _ => true;
    public Option<RectangleF> Viewport { get; init; }
    public Option<TKey> Proposed { get; init; }
    public LanguageExt.HashSet<TKey> Selected { get; init; }
    public Option<TKey> Anchor { get; init; }
    public Option<TKey> Lead { get; init; }
    public LanguageExt.HashSet<TKey> Requested { get; init; }
    public HashMap<TKey, Disposal<Thumbnail>> Loaded { get; init; }
    public Option<DragGap<TKey>> Gap { get; init; }
    public Option<Seq<TKey>> Exported { get; init; }
    public Option<TKey> Renaming { get; init; }
}

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record BrowserPart<TKey> where TKey : notnull {
    public Option<TKey> Item =>
        Switch(
            ground: static _ => Option<TKey>.None,
            tile: static tile => Some(tile.Key),
            caption: static caption => Some(caption.Key),
            star: static star => Some(star.Key));

    public sealed record Ground : BrowserPart<TKey>;
    public sealed record Tile(TKey Key) : BrowserPart<TKey>;
    public sealed record Caption(TKey Key) : BrowserPart<TKey>;
    public sealed record Star(TKey Key) : BrowserPart<TKey>;
}

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record BrowserEdit<TItem, TKey> where TItem : notnull where TKey : notnull {
    public sealed record Applied(TKey Key) : BrowserEdit<TItem, TKey>;
    public sealed record Reordered(Seq<TKey> Keys) : BrowserEdit<TItem, TKey>;
    public sealed record Renamed(TItem Item, string Name) : BrowserEdit<TItem, TKey>;
    public sealed record Favourited(TItem Item) : BrowserEdit<TItem, TKey>;
}

// --- [SERVICES] ------------------------------------------------------------------------
public sealed class ThumbnailBrowser<TItem, TKey> : ComponentControl<BrowserState<TItem, TKey>, BrowserPart<TKey>, BrowserEdit<TItem, TKey>>
    where TItem : notnull where TKey : notnull {
    // --- [STATE]
    private static readonly Func<BrowserEdit<TItem, TKey>, Edit<BrowserEdit<TItem, TKey>>> Step = static edit => new Edit<BrowserEdit<TItem, TKey>>.Step(edit);
    private static readonly Func<BrowserEdit<TItem, TKey>, Edit<BrowserEdit<TItem, TKey>>> Commit = static edit => new Edit<BrowserEdit<TItem, TKey>>.Commit(edit);

    private readonly BrowserSource<TItem, TKey> source;
    private readonly TextBox editor = new() { Visible = false, Font = EtoFonts.SmallFont };
    private readonly PixelLayout overlay = new();
    private Option<IDisposable> held;
    private Option<Scrollable> scroll;
    private HashMap<GlyphRole, Icon> stars;
    private Dwell dwell = new(None, Shown: false);

    public ThumbnailBrowser(IPlugInSink sink, BrowserSource<TItem, TKey> source, BrowserState<TItem, TKey> initial) : base(sink, initial, None) {
        this.source = source;
        overlay.Add(editor, 0, 0);
        Content = overlay;
    }

    public IO<Unit> Items(Seq<TItem> items) => Advance(state => new(Normalized(state with { Items = items }), None));
    public IO<Unit> TileWidth(Option<TileSize> size) => Advance(state => new(Normalized(state with { Size = size, Requested = [] }), None));
    public IO<Unit> Mode(BrowserMode mode) => Advance(state => new(Normalized(state with { Mode = mode, Requested = [] }), None));
    public IO<Unit> Extent(PixelExtent extent) => Advance(state => new(Normalized(state with { Extent = extent, Requested = [] }), None));
    public IO<Unit> Filter(Func<TItem, bool> filter) => Advance(state => new(Normalized(state with { Filter = filter }), None));
    public IO<Unit> Reload => Advance(state => new(Normalized(state with { Requested = [] }), None));
    public IO<Unit> TargetPointer => Advance(state => new(Targeted(state), None));
    public Seq<TItem> Selection => Shown(State).Filter(entry => State.Selected.Contains(entry.Key)).Map(static entry => entry.Item);
    public TileSize ImageWidth => State.Size.IfNone(() => State.Mode.Fit(Width));

    protected override BrowserState<TItem, TKey> Received(BrowserState<TItem, TKey> state, Option<BrowserEdit<TItem, TKey>> value) =>
        Normalized(value.Match(
            Some: edit => edit.Switch(
                state,
                applied: static (current, applied) => current with { Current = Some(applied.Key), Proposed = None, Gap = None },
                reordered: static (current, _) => current,
                renamed: static (current, _) => current,
                favourited: static (current, _) => current),
            None: () => state with { Current = None, Proposed = None, Gap = None }));

    protected override Option<float> Fitted(BrowserState<TItem, TKey> state, float width) =>
        scroll.Map(_ => state.Viewport.Fold(Geometry(state, width).Flow.Height(Slots(state).Count), static (height, view) => float.Max(height, view.Height)));

    private sealed record Dwell(Option<ForkIO<Unit>> Pending, bool Shown);

    // --- [LAYOUT]
    private sealed record Entry(TItem Item, TKey Key, int Number);

    private sealed record Placed(Entry Entry, RectangleF Cell, TileFace Face);

    private Seq<Entry> Entries(BrowserState<TItem, TKey> state) => state.Items.Map((item, index) => new Entry(item, source.Key(item), index + 1));

    private new Seq<Entry> Shown(BrowserState<TItem, TKey> state) => Entries(state).Filter(entry => state.Filter(entry.Item));

    private Seq<TKey> Order(BrowserState<TItem, TKey> state) => Shown(state).Map(static entry => entry.Key);

    private Option<Entry> Listed(BrowserState<TItem, TKey> state, TKey key) => toHashMap(Entries(state).Map(static entry => (entry.Key, entry))).Find(key);

    private Seq<Option<Entry>> Slots(BrowserState<TItem, TKey> state) =>
        state.Gap.Match(
            Some: gap => toHashSet(gap.Moved) switch {
                var moved => Shown(state).Filter(entry => !moved.Contains(entry.Key)).Map(static entry => Some(entry)) switch {
                    var rest => rest.Take(gap.Before).Add(None).Concat(rest.Skip(gap.Before)),
                },
            },
            None: () => Shown(state).Map(static entry => Some(entry)));

    private float Number(BrowserState<TItem, TKey> state) =>
        source.Numbered ? EtoFonts.SmallFont.MeasureString(state.Items.Count.ToString(RowText.Culture)).Width : 0f;

    private Seq<Placed> Placements(BrowserState<TItem, TKey> state, float width, RectangleF view) =>
        (Geometry(state, width), Number(state)) switch {
            var ((flow, image), number) => flow.Within(view) switch {
                var (first, count) =>
                    from slot in Slots(state).Map(static (held, index) => (Entry: held, Index: index)).Skip(first).Take(count)
                    from entry in slot.Entry.ToSeq()
                    let cell = flow.Slot(slot.Index)
                    select new Placed(entry, cell, state.Mode.Face(cell, image, number)),
            },
        };

    private Option<RectangleF> Cell(BrowserState<TItem, TKey> state, float width, TKey key) =>
        toHashMap(Slots(state).Map(static (slot, index) => slot.Map(entry => (entry.Key, index))).Somes())
            .Find(key)
            .Map(index => Geometry(state, width).Flow.Slot(index));

    private static Option<Placed> Locate(Seq<Placed> placed, TKey key) => toHashMap(placed.Map(static at => (at.Entry.Key, at))).Find(key);

    private static SizeF ImageBox(BrowserState<TItem, TKey> state, float width) =>
        (float)state.Size.IfNone(() => state.Mode.Fit(width)) switch {
            var side => new SizeF(side, side * state.Extent.Height / state.Extent.Width),
        };

    private static (TileFlow Flow, SizeF Image) Geometry(BrowserState<TItem, TKey> state, float width) =>
        ImageBox(state, width) switch {
            var image => (state.Mode.Flow(width, image), image),
        };

    private static Option<Size> RequestBox(BrowserState<TItem, TKey> state) => state.Viewport.Map(view => Size.Ceiling(ImageBox(state, view.Width)));

    private static RectangleF View(BrowserState<TItem, TKey> state, SizeF size) => state.Viewport.IfNone(new RectangleF(size));

    private static int Position(Seq<TKey> keys, Option<TKey> key) => keys.TakeWhile(each => Some(each) != key).Count;

    protected override Seq<PlotMark<BrowserPart<TKey>>> Layout(BrowserState<TItem, TKey> state, SizeF size, float scale) =>
        new PlotMark<BrowserPart<TKey>>(new MarkShape.Band(new RectangleF(size)), None, new MarkPart<BrowserPart<TKey>>(new BrowserPart<TKey>.Ground(), 0f))
            .Cons(Placements(state, size.Width, View(state, size)).Bind(at => Marked(state, at)));

    private Seq<PlotMark<BrowserPart<TKey>>> Marked(BrowserState<TItem, TKey> state, Placed at) =>
        Seq(Some(new PlotMark<BrowserPart<TKey>>(new MarkShape.Band(at.Cell), None, new MarkPart<BrowserPart<TKey>>(new BrowserPart<TKey>.Tile(at.Entry.Key), 0f))),
                at.Face.Ruled
                    ? Some(new PlotMark<BrowserPart<TKey>>(new MarkShape.Band(at.Face.Caption with { Height = 1f }), new MarkStyle.Fill(PaintSlot.ContentListEnabledText, 1f), None))
                    : None,
                state.Selected.Contains(at.Entry.Key)
                    ? Some(new PlotMark<BrowserPart<TKey>>(new MarkShape.Band(at.Face.Band), new MarkStyle.Fill(PaintSlot.Highlight, 1f), None))
                    : None,
                source.Renames && (state.Proposed | state.Current) == Some(at.Entry.Key)
                    ? Some(new PlotMark<BrowserPart<TKey>>(new MarkShape.Band(at.Face.Caption), None, new MarkPart<BrowserPart<TKey>>(new BrowserPart<TKey>.Caption(at.Entry.Key), 0f)))
                    : None,
                source.Favourite.IsSome
                    ? Some(new PlotMark<BrowserPart<TKey>>(new MarkShape.Band(at.Face.Star), None, new MarkPart<BrowserPart<TKey>>(new BrowserPart<TKey>.Star(at.Entry.Key), 0f)))
                    : None)
            .Somes();

    protected override PartFacet Facet(BrowserState<TItem, TKey> state, BrowserPart<TKey> key) =>
        key.Switch(
            (Browser: this, State: state),
            ground: static (held, _) => new PartFacet(PartRole.Image, Cursors.Default, held.Browser.source.Empty, None, None),
            tile: static (held, tile) => held.Browser.Listed(held.State, tile.Key).Match(
                Some: entry => new PartFacet(
                    PartRole.Cell, Cursors.Default, held.Browser.source.Label(entry.Item), held.Browser.source.Numbered ? Some(entry.Number.ToString(RowText.Culture)) : None, None),
                None: static () => new PartFacet(PartRole.Cell, Cursors.Default, "", None, None)),
            caption: static (_, _) => new PartFacet(PartRole.Button, Cursors.IBeam, RowText.Localize("Rename").Local, None, None),
            star: static (held, star) => new PartFacet(
                PartRole.Button,
                Cursors.Pointer,
                RowText.Localize("Favourite").Local,
                from entry in held.Browser.Listed(held.State, star.Key)
                from favourite in held.Browser.source.Favourite
                select RowText.Localize(favourite(entry.Item) ? "On" : "Off").Local,
                None));

    // --- [TRANSITIONS]
    private BrowserState<TItem, TKey> Normalized(BrowserState<TItem, TKey> state) =>
        (Keys: toHashSet(state.Items.Map(source.Key)),
         Shown: toHashSet(Order(state)),
         Visible: toHashSet(state.Viewport.ToSeq().Bind(view => Placements(state, view.Width, view)).Map(static at => at.Entry.Key))) switch {
             var (keys, shown, visible) => Retained(state.Loaded.Filter((key, _) => keys.Contains(key)), visible) switch {
                 var loaded => state with {
                     Selected = state.Selected.Intersect(shown),
                     Requested = state.Requested.Filter(key => keys.Contains(key) && (loaded.ContainsKey(key) || !state.Loaded.ContainsKey(key))).Union(visible),
                     Loaded = loaded,
                     Anchor = state.Anchor.Filter(keys.Contains),
                     Lead = state.Lead.Filter(keys.Contains),
                     Proposed = state.Proposed.Filter(keys.Contains),
                     Current = state.Current.Filter(keys.Contains),
                     Renaming = state.Renaming.Filter(keys.Contains),
                 },
             },
         };

    private static HashMap<TKey, Disposal<Thumbnail>> Retained(HashMap<TKey, Disposal<Thumbnail>> loaded, LanguageExt.HashSet<TKey> visible) =>
        loaded.Count > 500 ? loaded.Filter((key, _) => visible.Contains(key)) : loaded;

    private BrowserState<TItem, TKey> Viewed(BrowserState<TItem, TKey> state, Option<RectangleF> view) =>
        (state with { Viewport = view }) switch {
            var next => Normalized(RequestBox(next) == RequestBox(state) ? next : next with { Requested = [] }),
        };

    private static BrowserState<TItem, TKey> Only(BrowserState<TItem, TKey> state, TKey key) =>
        state with { Selected = [key], Anchor = Some(key), Lead = Some(key) };

    private static Transition<BrowserState<TItem, TKey>, BrowserEdit<TItem, TKey>> Chosen(
        BrowserState<TItem, TKey> state, TKey key, Func<BrowserEdit<TItem, TKey>, Edit<BrowserEdit<TItem, TKey>>> kind) =>
        Only(state, key) switch {
            var picked when state.Current == Some(key) && state.Proposed.IsNone => new(picked, None),
            var picked => new(picked with { Proposed = Some(key) }, kind(new BrowserEdit<TItem, TKey>.Applied(key))),
        };

    private static LanguageExt.HashSet<TKey> Spanned(Seq<TKey> keys, Option<TKey> origin, TKey target) =>
        (Position(keys, origin.Filter(anchor => keys.Exists(key => EqualityComparer<TKey>.Default.Equals(key, anchor))) | Some(target)), Position(keys, Some(target))) switch {
            var (start, end) => toHashSet(keys.Skip(int.Min(start, end)).Take(int.Abs(start - end) + 1)),
        };

    private BrowserState<TItem, TKey> Picked(BrowserState<TItem, TKey> state, TKey key, Keys modifiers) =>
        ((modifiers & Application.Instance.CommonModifier) != Keys.None, modifiers.HasFlag(Keys.Shift)) switch {
            (true, _) => state with { Selected = state.Selected.Contains(key) ? state.Selected.Remove(key) : state.Selected.Add(key), Anchor = Some(key), Lead = Some(key) },
            (_, true) => state with { Selected = Spanned(Order(state), state.Anchor, key), Anchor = state.Anchor | Some(key), Lead = Some(key) },
            _ when state.Selected.Contains(key) => state with { Anchor = Some(key), Lead = Some(key) },
            _ => Only(state, key),
        };

    private BrowserState<TItem, TKey> Targeted(BrowserState<TItem, TKey> state) =>
        Pointer.Bind(at => Placements(state, Width, View(state, Size)).Find(placed => placed.Cell.Contains(at)))
            .Filter(placed => !state.Selected.Contains(placed.Entry.Key))
            .Match(Some: placed => Only(state, placed.Entry.Key), None: () => state);

    private Transition<BrowserState<TItem, TKey>, BrowserEdit<TItem, TKey>> Gapped(BrowserState<TItem, TKey> state, float width, Seq<TKey> keys, Seq<TKey> moved, PointF pointer) =>
        new DragGap<TKey>(moved, int.Clamp(state.Mode.Insertion(pointer, Geometry(state, width).Flow), 0, keys.Count - moved.Count)) switch {
            var gap when state.Gap == Some(gap) => new(state, None),
            var gap => new(state with { Gap = gap }, new Edit<BrowserEdit<TItem, TKey>>.Preview(new BrowserEdit<TItem, TKey>.Reordered(Arranged(state, keys, gap)))),
        };

    private Seq<TKey> Arranged(BrowserState<TItem, TKey> state, Seq<TKey> keys, DragGap<TKey> gap) =>
        toHashSet(gap.Moved) switch {
            var moved => state.Items.Map(source.Key).Filter(key => !moved.Contains(key)) switch {
                var rest => Position(rest, keys.Filter(key => !moved.Contains(key)).At(gap.Before)) switch {
                    var index => rest.Take(index) + gap.Moved + rest.Skip(index),
                },
            },
        };

    // --- [INPUT]
    protected override Option<Transition<BrowserState<TItem, TKey>, BrowserEdit<TItem, TKey>>> Pressed(
        BrowserState<TItem, TKey> state, SizeF size, float scale, Option<BrowserPart<TKey>> part, MouseEventArgs e) =>
        from pressed in part
        where e.Buttons == MouseButtons.Primary
        select new Transition<BrowserState<TItem, TKey>, BrowserEdit<TItem, TKey>>(
            pressed.Item.Match(Some: key => Picked(state, key, e.Modifiers), None: () => state with { Selected = [], Anchor = None }) with { Exported = None },
            None);

    protected override Transition<BrowserState<TItem, TKey>, BrowserEdit<TItem, TKey>> Dragged(
        BrowserState<TItem, TKey> state, SizeF size, float scale, Option<BrowserPart<TKey>> part, PointF origin, PointF previous, MouseEventArgs e) =>
        part.Bind(static pressed => pressed.Item)
            .Filter(_ => state.Exported.IsNone && (source.Reorders || source.Export.IsSome))
            .Map(key => Order(state) switch {
                var keys => keys.Filter((state.Selected.Contains(key) ? state.Selected : [key]).Contains) switch {
                    var moved when source.Export.IsSome && !(source.Reorders && new RectangleF(size).Contains(e.Location)) =>
                        new Transition<BrowserState<TItem, TKey>, BrowserEdit<TItem, TKey>>(
                            state with { Gap = None, Exported = Some(moved) },
                            source.Reorders ? Some<Edit<BrowserEdit<TItem, TKey>>>(new Edit<BrowserEdit<TItem, TKey>>.Cancel()) : None),
                    var moved => Gapped(state, size.Width, keys, moved, e.Location),
                },
            })
            .IfNone(() => new(state, None));

    protected override Transition<BrowserState<TItem, TKey>, BrowserEdit<TItem, TKey>> Released(
        BrowserState<TItem, TKey> state, SizeF size, float scale, Option<BrowserPart<TKey>> part, MouseEventArgs e) =>
        part.Filter(_ => state.Gap.IsNone && state.Exported.IsNone && e.Modifiers == Keys.None)
            .Map(pressed => pressed.Switch(
                (Browser: this, State: state),
                ground: static (held, _) => new Transition<BrowserState<TItem, TKey>, BrowserEdit<TItem, TKey>>(held.State, None),
                tile: static (held, tile) => Chosen(held.State, tile.Key, Commit),
                caption: static (held, caption) =>
                    held.Browser.HasFocus ? new(held.State with { Renaming = Some(caption.Key) }, None) : Chosen(held.State, caption.Key, Commit),
                star: static (held, star) => new Transition<BrowserState<TItem, TKey>, BrowserEdit<TItem, TKey>>(
                    held.State, held.Browser.Listed(held.State, star.Key).Map(static entry => Commit(new BrowserEdit<TItem, TKey>.Favourited(entry.Item))))))
            .IfNone(() => new(state, None));

    protected override Option<Transition<BrowserState<TItem, TKey>, BrowserEdit<TItem, TKey>>> KeyPressed(
        BrowserState<TItem, TKey> state, SizeF size, float scale, Option<BrowserPart<TKey>> part, KeyEventArgs e) =>
        Order(state) switch {
            var keys => e.Key switch {
                Keys.Left or Keys.Right or Keys.Up or Keys.Down or Keys.Home or Keys.End =>
                    keys.At(Target(keys, state.Lead, e.Key, Geometry(state, size.Width).Flow.Columns)).Map(key => e.Modifiers.HasFlag(Keys.Shift)
                        ? new Transition<BrowserState<TItem, TKey>, BrowserEdit<TItem, TKey>>(
                            state with { Selected = Spanned(keys, state.Anchor | state.Lead, key), Anchor = state.Anchor | state.Lead | Some(key), Lead = Some(key) }, None)
                        : Chosen(state, key, Step)),
                Keys.Enter => state.Lead.Map(lead => Chosen(state, lead, Commit)),
                Keys.F2 when source.Renames => state.Lead.Map(lead => new Transition<BrowserState<TItem, TKey>, BrowserEdit<TItem, TKey>>(state with { Renaming = Some(lead) }, None)),
                >= Keys.D1 and <= Keys.D9 when source.Numbered && e.Modifiers == Keys.None =>
                    state.Items.At(e.Key - Keys.D1).Filter(state.Filter).Map(item => Chosen(state, source.Key(item), Commit)),
                Keys.A when (e.Modifiers & Application.Instance.CommonModifier) != Keys.None =>
                    Some(new Transition<BrowserState<TItem, TKey>, BrowserEdit<TItem, TKey>>(state with { Selected = toHashSet(keys) }, None)),
                _ => None,
            },
        };

    protected override Option<Transition<BrowserState<TItem, TKey>, BrowserEdit<TItem, TKey>>> Activated(
        BrowserState<TItem, TKey> state, SizeF size, float scale, BrowserPart<TKey> part) =>
        part.Switch(
            (Browser: this, State: state),
            ground: static (_, _) => Option<Transition<BrowserState<TItem, TKey>, BrowserEdit<TItem, TKey>>>.None,
            tile: static (held, tile) => Some(Chosen(held.State, tile.Key, Step)),
            caption: static (held, caption) => Some(new Transition<BrowserState<TItem, TKey>, BrowserEdit<TItem, TKey>>(held.State with { Renaming = Some(caption.Key) }, None)),
            star: static (held, star) => held.Browser.Listed(held.State, star.Key).Map(entry =>
                new Transition<BrowserState<TItem, TKey>, BrowserEdit<TItem, TKey>>(held.State, Step(new BrowserEdit<TItem, TKey>.Favourited(entry.Item)))));

    private static int Target(Seq<TKey> keys, Option<TKey> lead, Keys key, int columns) =>
        key switch {
            Keys.Home => 0,
            Keys.End => keys.Count - 1,
            _ => lead.Map(_ => int.Min(int.Max(Position(keys, lead) + key switch { Keys.Left => -1, Keys.Right => 1, Keys.Up => -columns, _ => columns }, 0), keys.Count - 1)).IfNone(0),
        };

    private IO<Unit> Keyed(KeyEventArgs args) =>
        args.Key switch {
            Keys.Enter => IO.lift(() => args.Handled = true).Bind(_ => Renamed),
            Keys.Escape => IO.lift(() => args.Handled = true).Bind(_ => Advance(static state => new(state with { Renaming = None }, None))),
            _ => IO.pure(unit),
        };

    private IO<Unit> Renamed =>
        IO.lift(() => editor.Text).Bind(text => Advance(state => state.Renaming.Bind(key => Listed(state, key)).Match(
            Some: entry => new Transition<BrowserState<TItem, TKey>, BrowserEdit<TItem, TKey>>(
                state with { Renaming = None }, string.Equals(text, source.Label(entry.Item), StringComparison.Ordinal) ? None : Commit(new BrowserEdit<TItem, TKey>.Renamed(entry.Item, text))),
            None: () => new Transition<BrowserState<TItem, TKey>, BrowserEdit<TItem, TKey>>(state, None))));

    private IO<Unit> Viewing => IO.lift(() => VisibleBounds).Bind(view => Advance(state => new(Viewed(state, state.Viewport.Map(_ => view)), None)));

    private RectangleF VisibleBounds => scroll.Match(Some: static view => (RectangleF)view.VisibleRect, None: () => new RectangleF(Size));

    protected override void OnSizeChanged(EventArgs e) {
        base.OnSizeChanged(e);
        _ = Callbacks.Answer(Viewing, static () => unit, Site());
    }

    // --- [RECONCILE]
    protected override IO<Unit> Reconcile(
        BrowserState<TItem, TKey> before, BrowserState<TItem, TKey> after, Interaction<BrowserPart<TKey>> was, Interaction<BrowserPart<TKey>> now) =>
        Callbacks.Each(Seq(
                Loading(before, after),
                Releasing(before, after),
                from preview in source.Preview
                where was.Hovered.Bind(static part => part.Item) != now.Hovered.Bind(static part => part.Item)
                select Hovered(preview, now.Hovered.Bind(static part => part.Item).Bind(key => Listed(after, key))),
                before.Renaming == after.Renaming ? None : Some(after.Renaming.Match(Some: key => Editing(after, key), None: () => Closing)),
                from view in scroll
                from lead in after.Lead
                where before.Lead != after.Lead
                from cell in Cell(after, Width, lead)
                select IO.lift(() => { view.ScrollPosition = view.ScrollPosition with { Y = Revealed(view.VisibleRect, cell) }; }),
                from export in source.Export
                from moved in after.Exported
                where before.Exported.IsNone
                select toHashSet(moved) switch {
                    var keys => export(
                        this, after.Items.Filter(item => keys.Contains(source.Key(item))), after.Lead.Bind(after.Loaded.Find).Bind(static thumbnail => thumbnail.Held)),
                })
            .Somes())
            .Map(static _ => unit);

    private Option<IO<Unit>> Loading(BrowserState<TItem, TKey> before, BrowserState<TItem, TKey> after) =>
        from box in RequestBox(after)
        let gained = after.Requested.Except(before.Requested)
        where !gained.IsEmpty
        select after.Items.Filter(item => gained.Contains(source.Key(item)))
            .Traverse(item => source.Tile(this, item, box).Bind(arrival => Arrived(source.Key(item), box, arrival).Post()).Catch(Reported).As())
            .As()
            .Fork()
            .Map(static _ => unit);

    private IO<Unit> Arrived(TKey key, Size box, Disposal<Thumbnail> arrival) =>
        IO.lift(() => RequestBox(State) == Some(box) && State.Requested.Contains(key))
            .Bind(accepted => accepted
                ? Advance(state => new(Normalized(state with { Loaded = state.Loaded.AddOrUpdate(key, arrival) }), None))
                : IO.lift(arrival.Dispose));

    private static Option<IO<Unit>> Releasing(BrowserState<TItem, TKey> before, BrowserState<TItem, TKey> after) =>
        before.Loaded.Filter((key, thumbnail) => !after.Loaded.Find(key).Exists(kept => ReferenceEquals(kept, thumbnail))) switch {
            var dropped => dropped.IsEmpty ? None : Some(DisposalOps.Release(toSeq(dropped.Values))),
        };

    private IO<Unit> Hovered(Func<Option<TItem>, IO<Unit>> preview, Option<Entry> hovered) =>
        from restored in Restored
        from pending in hovered.Match(
            Some: entry => IO.yieldFor(TimeSpan.FromSeconds(0.2)).Bind(_ => Previewed(preview, entry.Item).Post()).Catch(Reported).As().Fork().Map(static fork => Some(fork)),
            None: static () => IO.pure(Option<ForkIO<Unit>>.None))
        from recorded in IO.lift(() => { dwell = dwell with { Pending = pending }; })
        select unit;

    private IO<Unit> Previewed(Func<Option<TItem>, IO<Unit>> preview, TItem item) =>
        from suspended in IO.lift(static () => Keyboard.Modifiers.HasFlag(Keys.Shift))
        from shown in unless(suspended, preview(Some(item))).As()
        from recorded in IO.lift(() => { dwell = dwell with { Shown = !suspended }; })
        select unit;

    private IO<Unit> Restored =>
        from current in IO.lift(() => dwell)
        from cancelled in current.Pending.Match(Some: static pending => pending.Cancel, None: static () => IO.pure(unit))
        from restored in when(current.Shown, source.Preview.Match(Some: static preview => preview(None), None: static () => IO.pure(unit))).As()
        from cleared in IO.lift(() => { dwell = new Dwell(None, Shown: false); })
        select unit;

    private IO<Unit> Editing(BrowserState<TItem, TKey> state, TKey key) =>
        IO.lift(() => Locate(Placements(state, Width, View(state, Size)), key).Iter(at => {
            overlay.Move(editor, Eto.Drawing.Point.Round(at.Face.Caption.Location));
            editor.Size = Size.Round(at.Face.Caption.Size);
            editor.Text = source.Label(at.Entry.Item);
            editor.Visible = true;
            editor.Focus();
            editor.SelectAll();
        }));

    private IO<Unit> Closing =>
        from focused in IO.lift(() => editor.HasFocus)
        from hidden in IO.lift(() => { editor.Visible = false; })
        from refocused in when(focused, IO.lift(Focus)).As()
        select unit;

    private IO<Unit> Reported(Error error) => IO.lift(() => Sink.Report(error, GetType(), nameof(Reconcile)));

    private static int Revealed(Rectangle shown, RectangleF cell) =>
        cell.Top < shown.Top ? (int)MathF.Floor(cell.Top)
        : cell.Bottom > shown.Bottom ? (int)MathF.Ceiling(cell.Bottom - shown.Height)
        : shown.Top;

    // --- [PAINT]
    protected override IO<Unit> Draw(
        PlotCanvas canvas, RectangleF bounds, BrowserState<TItem, TKey> state, Seq<PlotMark<BrowserPart<TKey>>> marks, Interaction<BrowserPart<TKey>> interaction) =>
        (Placed: Placements(state, bounds.Width, View(state, bounds.Size)),
         Hovered: canvas.Enabled ? interaction.Hovered.Bind(static part => part.Item) : None,
         Framed: state.Proposed | state.Current) switch {
             var (placed, hovered, framed) => Callbacks.Each(Seq(
                     Some(Plots.Well(canvas, bounds)),
                     Some(Tiled(canvas, state, placed)),
                     Some(Plots.Paint(canvas, marks)),
                     from key in hovered
                     where !state.Selected.Contains(key) && framed != Some(key)
                     from at in Locate(placed, key)
                     select Plots.Paint(canvas, Seq(
                         new PlotMark<BrowserPart<TKey>>(new MarkShape.Band(at.Face.Band), new MarkStyle.Fill(PaintSlot.ContentHighlightHover, 1f), None),
                         Outline(at.Cell, PaintSlot.ContentHighlightHover, 1f))),
                     Some(Lettered(canvas, bounds, state, placed)),
                     from key in framed
                     from at in Locate(placed, key)
                     select Plots.Paint(canvas, Seq(Outline(at.Cell, PaintSlot.Highlight, canvas.Focused ? 2f : 1f))),
                     from favourite in source.Favourite
                     where canvas.Enabled
                     select Starred(canvas, placed, hovered, favourite))
                 .Somes())
                 .Map(static _ => unit),
         };

    private static IO<Unit> Tiled(PlotCanvas canvas, BrowserState<TItem, TKey> state, Seq<Placed> placed) =>
        IO.lift(() => canvas.Graphics.ClipBounds)
            .Bind(clip => placed.Filter(at => at.Cell.Intersects(clip))
                .Choose(at => state.Loaded.Find(at.Entry.Key).Bind(static loaded => loaded.Held).Map(thumbnail => (Thumbnail: thumbnail, Box: Contained(thumbnail, at.Face.Image))))
                .TraverseM(tile => Thumbnails.Checker(canvas, tile.Box, Checkerboard.Content).Bind(_ => tile.Thumbnail.Paint(canvas, tile.Box, PaintSlot.ContentListEnabledText)))
                .As())
            .Map(static _ => unit);

    private IO<Unit> Lettered(PlotCanvas canvas, RectangleF bounds, BrowserState<TItem, TKey> state, Seq<Placed> placed) =>
        use(() => new FormattedText {
            Font = canvas.Font,
            ForegroundBrush = Brushes.Cached(Plots.Resolve(canvas, new MarkStyle.Fill(PaintSlot.ContentListEnabledText, 1f))),
            Trimming = FormattedTextTrimming.CharacterEllipsis,
        })
            .Bind(text => IO.lift(() => Runs(bounds, state, placed).Iter(run => Typeset(canvas.Graphics, text, run))))
            .Bracket();

    private Seq<(string Text, RectangleF Box, FormattedTextAlignment Align, FormattedTextWrapMode Wrap)> Runs(RectangleF bounds, BrowserState<TItem, TKey> state, Seq<Placed> placed) =>
        state.Items.IsEmpty
            ? [(source.Empty, bounds.Width > 200f ? RectangleF.Inset(bounds, new PaddingF(50f, 0f)) : bounds, FormattedTextAlignment.Center, FormattedTextWrapMode.Word)]
            : placed.Bind(at => (source.Label(at.Entry.Item), at.Face.Caption, at.Face.Align, FormattedTextWrapMode.None)
                .Cons(at.Face.Number.Map(column => (at.Entry.Number.ToString(RowText.Culture), column, FormattedTextAlignment.Right, FormattedTextWrapMode.None)).ToSeq()));

    private IO<Unit> Starred(PlotCanvas canvas, Seq<Placed> placed, Option<TKey> hovered, Func<TItem, bool> favourite) =>
        IO.lift(() => placed
            .Map(at => (At: at, On: favourite(at.Entry.Item)))
            .Filter(star => star.On || hovered == Some(star.At.Entry.Key))
            .Iter(star => stars.Find(star.On ? GlyphRole.FavouriteOn : GlyphRole.FavouriteOff).Iter(icon => canvas.Graphics.DrawImage(icon, star.At.Face.Star))));

    private static PlotMark<BrowserPart<TKey>> Outline(RectangleF bounds, PaintSlot slot, float width) =>
        new(new MarkShape.Band(RectangleF.Inset(bounds, new PaddingF(width / 2f))), new MarkStyle.Stroke(slot, 1f, width), None);

    private static RectangleF Contained(Thumbnail thumbnail, RectangleF box) =>
        (SizeF)thumbnail.Switch(raster: static raster => raster.Pixels.Size, linework: static linework => linework.Extent, hatching: static hatching => hatching.Extent) switch {
            var extent => RectangleF.FromCenter(box.Center, extent * float.Min(box.Width / extent.Width, box.Height / extent.Height)),
        };

    private static void Typeset(Graphics graphics, FormattedText text, (string Text, RectangleF Box, FormattedTextAlignment Align, FormattedTextWrapMode Wrap) run) {
        (text.Text, text.Alignment, text.Wrap, text.MaximumWidth) = (run.Text, run.Align, run.Wrap, run.Box.Width);
        graphics.DrawText(text, new PointF(run.Box.X, run.Box.Y + ((run.Box.Height - text.Measure().Height) / 2f)));
    }

    // --- [LIFETIME]
    protected override void OnLoad(EventArgs e) {
        base.OnLoad(e);
        scroll = Optional(Parent as Scrollable);
        held = Callbacks.Answer(Acquired.Map(static acquired => Some(acquired)), static () => Option<IDisposable>.None, Site());
        _ = Callbacks.Answer(IO.lift(() => VisibleBounds).Bind(view => Advance(state => new(Viewed(state, Some(view)), None))), static () => unit, Site());
    }

    protected override void OnUnLoad(EventArgs e) {
        _ = Callbacks.Answer(Unloaded, static () => unit, Site());
        base.OnUnLoad(e);
    }

    protected override void Dispose(bool disposing) {
        if (disposing) {
            editor.Dispose();
            overlay.Dispose();
        }
        base.Dispose(disposing);
    }

    private IO<IDisposable> Acquired =>
        DisposalOps.AcquireAll(
                scroll.ToSeq()
                    .Bind(view => Seq(
                        Subscriptions.Attach(
                            handler => view.Scroll += handler, handler => view.Scroll -= handler, Callbacks.Handler<ScrollEventArgs>(_ => Viewing, Site(nameof(Scrollable.Scroll)))),
                        Subscriptions.Attach(
                            handler => view.SizeChanged += handler, handler => view.SizeChanged -= handler, Callbacks.Handler<EventArgs>(_ => Viewing, Site(nameof(SizeChanged))))))
                    .Concat(Seq(
                        Subscriptions.Attach(
                            handler => editor.KeyDown += handler, handler => editor.KeyDown -= handler, Callbacks.Handler<KeyEventArgs>(Keyed, Site(nameof(KeyDown)))),
                        Subscriptions.Attach(
                            handler => editor.LostFocus += handler, handler => editor.LostFocus -= handler, Callbacks.Handler<EventArgs>(_ => Renamed, Site(nameof(LostFocus))))))
                    .Concat(source.Favourite.ToSeq().Bind(static _ => Seq(GlyphRole.FavouriteOn, GlyphRole.FavouriteOff)).Map(role => Icons.Themed(Sink, role, IconSlot.ListCell, icon => {
                        stars = stars.AddOrUpdate(role, icon);
                        Invalidate();
                    }))),
                DisposalOps.Release)
            .Map(acquired => DisposalOps.Composite(acquired, Site(nameof(OnUnLoad))));

    private IO<Unit> Unloaded =>
        from restored in Restored
        from released in IO.lift(() => held.Iter(static acquired => acquired.Dispose()))
        from cleared in IO.lift(() => { held = None; })
        from emptied in Advance(static state => new(state with { Viewport = None, Loaded = [], Requested = [] }, None))
        select unit;
}
