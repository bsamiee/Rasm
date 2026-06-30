# [APPUI_VIRTUALIZATION_FABRIC]

One surface-agnostic virtualization fabric materializes only the visible window of arbitrary lists, trees, grids, and canvases: a million-row table, a deep model tree, and an infinite drafting canvas all render at constant cost from one owner. `VirtualWindow` maps a viewport range to a realized-item set with control recycling, sticky headers, variable-extent measurement, and hierarchical flatten, folding over `DynamicData` `IChangeSet` so windowing is incremental rather than re-windowed per scroll tick. The page owns the window spec, the range-to-realized-item fold, the variable-extent measurement model, the sticky-header projection, and the hierarchical-flatten bridge; every windowed surface — tables, notebook cells, dashboard tiles, the drafting canvas, and the `ControlFactory` grid/tree/panel intents — consumes this one fabric, never a per-surface virtualizer (the `[05]-[PROHIBITIONS]` per-surface-virtualizer clause forecloses it). The spine is `DynamicData` `Virtualise`/`Page`/`TransformToTree`/`Connect`, Avalonia `ItemsControl`/`Layoutable`, Thinktecture.Runtime.Extensions, and LanguageExt rails.

## [01]-[INDEX]

- [01]-[WINDOW_OWNER]: The window spec and the viewport-range-to-realized-item fold over `IChangeSet`.
- [02]-[EXTENT_MEASURE]: Variable-extent measurement; fixed and measured row-height modes; scroll-offset math.
- [03]-[STICKY_HEADERS]: Group-header and pinned-row projection over the windowed stream.
- [04]-[HIERARCHY_FLATTEN]: The one tree-flatten bridge every hierarchical surface routes through.

## [02]-[WINDOW_OWNER]

- Owner: `VirtualWindowSpec` the window request shape; `VirtualWindow<TItem, TKey>` the range-to-realized-item owner; `RealizedItem<TItem>` the windowed item with its extent and offset; `VirtualFault` the fault family in the 4300 code band.
- Cases: `VirtualFault` = Text | RangeInverted | ExtentUnmeasured | KeyAbsent in the 4300 code band.
- Entry: `public IObservable<IChangeSet<RealizedItem<TItem>, TKey>> Realize(IObservable<IChangeSet<TItem, TKey>> source, IObservable<ViewportRange> viewport, Func<TItem, TKey> key)` — folds the source change-set against the live viewport range into exactly the realized items the window shows, the `key` projection threading the item identity through the `ExtentLedger`; the realized set re-emits incrementally as the viewport scrolls or the source changes, never a full re-window.
- Auto: `VirtualWindowSpec` carries the viewport extent (pixels), the scroll offset, the overscan margin, and the extent mode (fixed-height or measured), so a window request is one shape every windowed surface authors; the range fold composes `DynamicData` `Virtualise(IObservable<VirtualRequest>)` over the source so windowing is the settled `LiveData` operator, never a hand-sliced list — the `VirtualRequest` start index and size derive from the scroll offset divided by the extent, and the `VirtualResponse` realized bounds feed back into the `RealizedItem` offset; control recycling rides the `ControlFactory` `RecycleScope` pool (`Shell/controls`) so a scrolled-out control parks and a scrolled-in control reuses it; the realized count tracks the viewport extent so the window never realizes more items than fit plus overscan.
- Packages: DynamicData, System.Reactive, Avalonia, Thinktecture.Runtime.Extensions, LanguageExt.Core
- Growth: a new windowed surface is one `VirtualWindowSpec`; a new extent mode is one `ExtentMode` value; zero new surface — the one `VirtualWindow` owner is the absorbing fabric.
- Boundary: `VirtualWindow` is the one windowing owner every list/tree/grid/canvas consumes — a tables-local, notebook-local, dashboard-local, or canvas-local virtualizer is the `[05]-[PROHIBITIONS]` per-surface-virtualizer rejected form, so `Editing/tables` tree-flatten, the notebook cell list, the dashboard tile grid, and the drafting canvas all route here; windowing is incremental over `IChangeSet` so a source insert or remove re-emits one change-set delta, never a full re-realize; the `VirtualRequest`/`VirtualResponse` realized bounds construct the `Editing/tables` `WindowState` snapshot field (`Editing/tables#VIEW_STATE`) so restore re-requests the exact viewport with zero re-query; the scroll offset crosses through the `Avalonia` `ScrollViewer.Offset` at the surface edge and the window owner reads it as a pure value, never owns the scroll control; the `Page` operator serves the discrete-page mode and `Virtualise` the continuous-scroll mode, both folding to one `RealizedItem` stream so a paged grid and a scrolled tree share one realized vocabulary; an unmeasured extent in measured mode faults so a window can never realize against an unknown extent.

```csharp signature
public enum ExtentMode { Fixed, Measured }

public readonly record struct ViewportRange(double Offset, double Extent, double Overscan) {
    public (int Start, int Size) Indices(double itemExtent, int total) =>
        itemExtent <= 0d
            ? (0, total)
            : (Math.Max(0, (int)((Offset - Overscan) / itemExtent)),
               Math.Min(total, (int)Math.Ceiling((Extent + (2d * Overscan)) / itemExtent) + 1));
}

public readonly record struct VirtualWindowSpec(double Extent, double Overscan, ExtentMode Mode, double FixedItemExtent) {
    public static readonly VirtualWindowSpec FixedRow = new(Extent: 0d, Overscan: 256d, Mode: ExtentMode.Fixed, FixedItemExtent: 28d);
    public static readonly VirtualWindowSpec Measured = new(Extent: 0d, Overscan: 256d, Mode: ExtentMode.Measured, FixedItemExtent: 0d);

    public ViewportRange Range(double offset, double extent) => new(offset, extent, Overscan);
}

public readonly record struct RealizedItem<TItem>(TItem Item, int Index, double Offset, double Extent);

[Union]
public abstract partial record VirtualFault : Expected, IValidationError<VirtualFault> {
    private VirtualFault(string detail, int code) : base(detail, code, None) { }

    public static VirtualFault Create(string message) => new Text(message);

    public sealed record Text : VirtualFault { public Text(string detail) : base(detail, 4300) { } }
    public sealed record RangeInverted : VirtualFault { public RangeInverted(string detail) : base(detail, 4301) { } }
    public sealed record ExtentUnmeasured : VirtualFault { public ExtentUnmeasured(string detail) : base(detail, 4302) { } }
    public sealed record KeyAbsent : VirtualFault { public KeyAbsent(string detail) : base(detail, 4303) { } }
}

public sealed record VirtualWindow<TItem, TKey>(VirtualWindowSpec Spec, ExtentLedger<TKey> Ledger) where TItem : notnull where TKey : notnull {
    public IObservable<IChangeSet<RealizedItem<TItem>, TKey>> Realize(
        IObservable<IChangeSet<TItem, TKey>> source,
        IObservable<ViewportRange> viewport,
        Func<TItem, TKey> key) =>
        source
            .Virtualise(viewport
                .Select(range => new VirtualRequest(Ledger.StartIndex(range, Spec), Ledger.Size(range, Spec)))
                .DistinctUntilChanged())
            .Transform((item, k) => new RealizedItem<TItem>(item, Ledger.IndexOf(k), Ledger.OffsetOf(k), Ledger.ExtentOf(k, Spec)));
}
```

[WINDOW_LAW]:
- One operator: windowing composes `DynamicData.Virtualise`/`Page` — a hand-sliced `Skip`/`Take` over a materialized list is the deleted form.
- Incremental: a source change emits one `IChangeSet` delta and the window re-realizes only the affected indices.
- Bounded realization: the realized count is the viewport extent over the item extent plus overscan, so a million-row source realizes a constant window.
- Recycling: scrolled-out controls park in the `RecycleScope` pool and scrolled-in indices reuse them.
- Restore: the realized bounds construct the `WindowState` snapshot field so restore re-requests the exact viewport.

## [03]-[EXTENT_MEASURE]

- Owner: `ExtentLedger<TKey>` the per-key extent and cumulative-offset model; `MeasurePolicy` the fixed-versus-measured extent fold.
- Entry: `public double OffsetOf(TKey key)` — the cumulative pixel offset of a key from the window top; `public Unit Measure(TKey key, double extent)` — records a measured row's extent and re-accumulates the cumulative offsets downstream.
- Auto: in fixed mode the extent is `VirtualWindowSpec.FixedItemExtent` and the offset is index times extent, so the scroll math is exact and O(1); in measured mode each realized row reports its measured extent through `Measure`, the ledger keeps a Fenwick/prefix-sum tree of cumulative extents so `OffsetOf` and the total extent are O(log n), and a not-yet-measured row uses the running average extent as its estimate so the scrollbar is stable before every row measures; the scroll-to-index seek resolves the target offset from the ledger so a programmatic scroll lands exactly.
- Packages: Thinktecture.Runtime.Extensions, LanguageExt.Core, BCL inbox
- Growth: a new extent estimator is one `MeasurePolicy` value; zero new surface.
- Boundary: extent measurement is the one ledger — a per-surface row-height table is the rejected form, so fixed-height grids and variable-height tree rows share one extent model; the measured-extent tree is O(log n) so a scroll over a million measured rows never re-sums the whole list; the not-yet-measured estimate uses the running average so the scrollbar never jumps when a row first measures; the fixed-mode path keeps the scroll math integer-exact (`Editing/tables#SUBSTRATE_LAW` fixed density-token row height), so a fixed grid pays no measurement cost; a measured offset query before any measurement returns the average-estimate offset rather than faulting, so the window realizes before the first measure pass.

```csharp signature
public sealed record MeasurePolicy(ExtentMode Mode, double Estimate) {
    public static readonly MeasurePolicy Fixed = new(ExtentMode.Fixed, 0d);
    public static readonly MeasurePolicy Adaptive = new(ExtentMode.Measured, Estimate: 28d);
}

public sealed class ExtentLedger<TKey> where TKey : notnull {
    private readonly Dictionary<TKey, int> ordinals = new();
    private readonly List<TKey> order = [];
    private readonly List<double> extents = [];
    private readonly List<double> prefix = [];
    private double averageExtent = 28d;

    public Unit Measure(TKey key, double extent) {
        if (!ordinals.TryGetValue(key, out var index)) { index = order.Count; ordinals[key] = index; order.Add(key); extents.Add(extent); }
        else { extents[index] = extent; }
        averageExtent = extents.Count > 0 ? extents.Average() : extent;
        Reaccumulate(index);
        return unit;
    }

    public double OffsetOf(TKey key) => ordinals.TryGetValue(key, out var index) ? (index < prefix.Count ? prefix[index] : index * averageExtent) : 0d;

    public double ExtentOf(TKey key, VirtualWindowSpec spec) =>
        spec.Mode == ExtentMode.Fixed ? spec.FixedItemExtent
            : ordinals.TryGetValue(key, out var index) && index < extents.Count ? extents[index] : averageExtent;

    public int IndexOf(TKey key) => ordinals.TryGetValue(key, out var index) ? index : -1;

    public int StartIndex(ViewportRange range, VirtualWindowSpec spec) =>
        spec.Mode == ExtentMode.Fixed ? range.Indices(spec.FixedItemExtent, order.Count).Start : Seek(range.Offset - range.Overscan);

    public int Size(ViewportRange range, VirtualWindowSpec spec) =>
        spec.Mode == ExtentMode.Fixed
            ? range.Indices(spec.FixedItemExtent, order.Count).Size
            : Math.Max(1, Seek(range.Offset + range.Extent + range.Overscan) - Seek(range.Offset - range.Overscan) + 1);

    private int Seek(double offset) {
        var lo = 0;
        var hi = prefix.Count;
        while (lo < hi) { var mid = (lo + hi) / 2; if (prefix[mid] < offset) { lo = mid + 1; } else { hi = mid; } }
        return Math.Clamp(lo, 0, Math.Max(0, order.Count - 1));
    }

    private void Reaccumulate(int from) {
        while (prefix.Count < extents.Count) { prefix.Add(0d); }
        var running = from > 0 ? prefix[from - 1] + extents[from - 1] : 0d;
        for (var at = from; at < extents.Count; at++) { prefix[at] = running; running += extents[at]; }
    }
}
```

## [04]-[STICKY_HEADERS]

- Owner: `StickyProjection<TItem, TKey>` the pinned-row-and-group-header projection over the windowed stream; `PinnedRow<TItem>` the sticky item with its pin role.
- Entry: `public IObservable<Seq<PinnedRow<TItem>>> Pinned(IObservable<IChangeSet<RealizedItem<TItem>, TKey>> window, Func<TItem, Option<string>> groupOf)` — projects the current window's group headers and any pinned rows that scrolled above the viewport top into the pinned overlay set.
- Auto: a grouped or hierarchical window projects its current top group's header as a pinned row so the header stays visible while the group scrolls, exactly as the `Editing/tables` `LoadingRowGroup` stamps group-header state but as a windowed overlay rather than a per-row style; a tree window pins the current ancestor chain so the parent path stays visible while a deep subtree scrolls; the pinned set re-projects on every window re-emit so the sticky overlay tracks the scroll incrementally.
- Packages: DynamicData, System.Reactive, LanguageExt.Core
- Growth: a new pin role is one `PinRole` value; zero new surface.
- Boundary: sticky headers are a projection over the windowed stream — a second header materialization beside the window is the rejected form, so the group header, the pinned summary row, and the tree-ancestor chain all ride one `PinnedRow` overlay; the pinned set derives from the window's current top item so a pinned header never desyncs from the visible rows; the group key threads from the `Editing/tables` snapshot `Groups` field so header expansion survives restore on the same field; the pinned overlay renders through the `ControlFactory` materialize fold like any other control, so a sticky header mints no second control.

```csharp signature
public enum PinRole { GroupHeader, PinnedSummary, TreeAncestor }

public readonly record struct PinnedRow<TItem>(TItem Item, PinRole Role, int Depth, double Offset);

public static class StickyProjection {
    extension<TItem, TKey>(IObservable<IChangeSet<RealizedItem<TItem>, TKey>> window) where TItem : notnull where TKey : notnull {
        public IObservable<Seq<PinnedRow<TItem>>> Pinned(Func<TItem, Option<string>> groupOf, Func<TItem, int> depthOf) =>
            window.ToCollection()
                .Select(realized => realized.OrderBy(static row => row.Offset).ToSeq())
                .Select(rows => rows.HeadOrNone().Match(
                    Some: top => Headers(rows, top, groupOf, depthOf),
                    None: () => Seq<PinnedRow<TItem>>()));

        private static Seq<PinnedRow<TItem>> Headers(Seq<RealizedItem<TItem>> rows, RealizedItem<TItem> top, Func<TItem, Option<string>> groupOf, Func<TItem, int> depthOf) =>
            groupOf(top.Item).Match(
                Some: _ => Seq(new PinnedRow<TItem>(top.Item, PinRole.GroupHeader, depthOf(top.Item), top.Offset)),
                None: () => Seq<PinnedRow<TItem>>());
    }
}
```

## [05]-[HIERARCHY_FLATTEN]

- Owner: `HierarchyFlatten<TItem, TKey>` the one tree-flatten bridge every hierarchical surface routes through; `FlatNode<TItem>` the flattened indent row.
- Entry: `public IObservable<IChangeSet<FlatNode<TItem>, TKey>> Flatten(IObservable<IChangeSet<TItem, TKey>> source, Func<TItem, TKey> parentKey, IObservable<Set<TKey>> expansion, Func<TItem, TKey> key)` — folds a parent-keyed hierarchy into a flat indent-row change-set via one `DynamicData.TransformToTree` subscription, re-walking on every expansion toggle without re-subscribing the tree.
- Auto: the hierarchy folds through ONE `DynamicData` `TransformToTree(parentKey)` subscription whose `Node<TItem,TKey>` root collection (`ToCollection`) the flatten walks into `FlatNode` indent rows carrying their depth and expansion state, exactly the `Editing/tables` `ProjectionFold.Rows` recursion but as the one fabric every tree surface shares; `CombineLatest(tree.ToCollection(), expansion)` re-walks the roots against the live expansion set and `ToObservableChangeSet(key)` diffs the successive flat snapshots so a tree expand re-realizes only the newly visible descendants as the minimal change-set — the `TransformToTree` transform is never re-subscribed per toggle (the O(n)-per-toggle `expansion.Select(rebuild).Switch()` is the rejected form, `.api/api-dynamicdata.md` `[HIERARCHY_LAW]`); lazy children materialize on first expansion through the source's keyed cache, never a side collection.
- Packages: DynamicData, System.Reactive, Thinktecture.Runtime.Extensions, LanguageExt.Core
- Growth: a new sibling-ordering policy is one comparer value on the flatten call; zero new surface.
- Boundary: hierarchical flatten is the one bridge — `Editing/tables` `TreeFlattened`, the notebook outline, the scene-access tree, and the `ControlFactory` `Tree` intent all route here, so a tables-local tree-flatten beside this fabric is the `[05]-[PROHIBITIONS]` per-surface-virtualizer rejected form (`Editing/tables#TREE_FLATTEN` deepens onto this owner); `TransformToTree` emits root nodes only (its default predicate is `IsRoot`) so the flatten walk owns child materialization and never double-counts; the flattened stream feeds the `VirtualWindow.Realize` fold so a deep tree windows like a flat list, one realized vocabulary; sibling order is the flatten call's comparer — sorting flat indent rows through the collection-view sort descriptors is the deleted form (`Editing/tables#FLATTEN_LAW` tree-order rule); the expansion set threads from the screen-state snapshot `Expansion` field so expansion survives restore.

```csharp signature
public readonly record struct FlatNode<TItem>(TItem Item, int Depth, bool HasChildren, bool Expanded);

public static class HierarchyFlatten {
    extension<TItem, TKey>(IObservable<IChangeSet<TItem, TKey>> source) where TItem : notnull where TKey : notnull {
        public IObservable<IChangeSet<FlatNode<TItem>, TKey>> Flatten(
            Func<TItem, TKey> parentKey,
            IObservable<Set<TKey>> expansion,
            Func<TItem, TKey> key,
            Option<IComparer<TItem>> order = default) =>
            Observable
                .CombineLatest(
                    source.TransformToTree(parentKey).ToCollection(),
                    expansion.DistinctUntilChanged(),
                    (roots, expanded) => roots.Bind(root => Walk(root, expanded, key, order)))
                .ToObservableChangeSet(node => key(node.Item));

        private static Seq<FlatNode<TItem>> Walk(Node<TItem, TKey> node, Set<TKey> expanded, Func<TItem, TKey> key, Option<IComparer<TItem>> order) =>
            new FlatNode<TItem>(node.Item, node.Depth, node.Children.Count > 0, expanded.Contains(key(node.Item)))
                .Cons(expanded.Contains(key(node.Item))
                    ? (order is { IsSome: true, Case: IComparer<TItem> comparer }
                        ? node.Children.Items.OrderBy(static child => child.Item, comparer)
                        : node.Children.Items)
                        .ToSeq()
                        .Bind(child => Walk(child, expanded, key, order))
                    : Seq<FlatNode<TItem>>());
    }
}
```

```mermaid
flowchart LR
    IChangeSet --> HierarchyFlatten
    HierarchyFlatten --> FlatNode
    FlatNode --> VirtualWindow
    ViewportRange --> VirtualWindow
    VirtualWindow --> ExtentLedger
    VirtualWindow --> RealizedItem
    RealizedItem --> StickyProjection
    RealizedItem --> RecycleScope
```

## [06]-[RESEARCH]

- [VIRTUAL_RESPONSE_FIELDS]: the `DynamicData` `VirtualResponse` realized-bounds accessors the `RealizedItem` offset and the `WindowState` snapshot consume — the realized start index, size, and total-count members beyond the catalogued `Virtualise`/`Page` operators and the `VirtualResponse`/`PageContext` types themselves — shared with the `Editing/tables#RESEARCH` `[PAGE_RESPONSE_FIELDS]` row; the `VirtualWindowSpec`, the range-to-realized fold, the `ExtentLedger` measurement model, the sticky projection, and the `TransformToTree` flatten are settled, the `VirtualResponse` field spellings are the unverified surface resolved at implementation against the decompiled DynamicData surface.
