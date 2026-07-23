# [TS_UI_API_TANSTACK_REACT_VIRTUAL]

`@tanstack/react-virtual` is the windowing engine the `view/compose` collection rows compose beside the headless table: given an item `count` and a size estimate it computes which items intersect the viewport and their absolute offsets, so a 100k-row table or list mounts only the visible span plus a small overscan. It is headless and measurement-driven — no wrapper component, no fixed row height: `measureElement` reads real element sizes through a `ResizeObserver` so variable-height rows settle after render. This React package thinly adapts the framework-agnostic `@tanstack/virtual-core` (identical core+adapter split to `@tanstack/react-table`), and it carries an advanced direct-DOM path (`directDomUpdates`) that writes item positions and container size straight to the DOM for scroll-only updates, re-rendering React only when the visible index range or `isScrolling` flips. Multi-column grids and masonry are one option (`lanes`); sticky headers, custom overscan, and pinned indices are one parameterized function (`rangeExtractor`), never special-cased code.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `@tanstack/react-virtual`
- package: `@tanstack/react-virtual` (MIT)
- module format: ESM (`type: module`), `sideEffects: false`; first-party bundled `.d.ts` (`dist/esm/index.d.ts`) with `export * from '@tanstack/virtual-core'`
- runtime target: React render tree over a DOM scroll element or the window; DPR-aware measurement via `ResizeObserver`; the core is DOM-free and framework-agnostic
- peer: `react catalog`, `react-dom catalog` — satisfied by the folder React 19 spine
- asset: pure-TypeScript library, fully declaration-shaped; the React hooks and the whole `virtual-core` surface ship as first-party bundled `.d.ts`
- rail: view composition plane — the windowing half of the `view/compose` table/virtual rows

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: the virtualizer instance and the virtual item
- rail: view

| [INDEX] | [SYMBOL]                                                                                   | [TYPE_FAMILY]        |
| :-----: | :----------------------------------------------------------------------------------------- | :------------------- |
|  [01]   | `Virtualizer<TScrollElement, TItemElement>`                                                | virtualizer          |
|  [02]   | `VirtualItem` (`{ key, index, start, end, size, lane }`)                                   | window item          |
|  [03]   | `ReactVirtualizer` (adds `containerRef`)                                                   | React virtualizer    |
|  [04]   | `Range` / `ScrollToOptions` (`{ align, behavior }`) / `ScrollAlignment` / `ScrollBehavior` | window / scroll opts |

- [01]-[VIRTUALIZER]: the framework-agnostic engine; the React hook returns a `ReactVirtualizer` wrapping it.
- [02]-[WINDOW_ITEM]: one visible item's absolute offset (`start`), measured `size`, and grid `lane`; the render loop maps these.
- [03]-[REACT_VIRTUALIZER]: the hook return; `containerRef` binds the inner size container for the direct-DOM path.
- [04]-[WINDOW_SCROLL_OPTS]: the computed index range and the imperative scroll-target options; `ScrollAlignment` is `'start'`/`'center'`/`'end'`/`'auto'` and `ScrollBehavior` is `'auto'`/`'smooth'`/`'instant'`.

[PUBLIC_TYPE_SCOPE]: the options bag — one parameterized windowing contract
- rail: view

| [INDEX] | [SYMBOL]                                                                                                             | [TYPE_FAMILY]    |
| :-----: | :------------------------------------------------------------------------------------------------------------------- | :--------------- |
|  [01]   | `VirtualizerOptions` / `ReactVirtualizerOptions` / `PartialKeys`                                                     | options bag      |
|  [02]   | `count` / `getScrollElement` / `estimateSize(index)` / `overscan` / `horizontal`                                     | core options     |
|  [03]   | `measureElement` / `lanes` / `gap` / `paddingStart` / `paddingEnd` / `scrollMargin`                                  | layout options   |
|  [04]   | `getItemKey(index)` / `rangeExtractor(range)` / `scrollToFn` / `initialOffset` / `enabled` / `isScrollingResetDelay` | control hooks    |
|  [05]   | `anchorTo` / `followOnAppend` / `initialMeasurementsCache` / `laneAssignmentMode`                                    | stream + restore |
|  [06]   | `directDomUpdates` / `directDomUpdatesMode` (`'transform'\|'position'`) / `useFlushSync`                             | perf options     |

- [01]-[OPTIONS_BAG]: the full option contract; the React variant defaults `observeElementRect`/`observeElementOffset`/`scrollToFn` so the hook needs only `count`/`getScrollElement`/`estimateSize`.
- [02]-[CORE_OPTIONS]: item count, the scroll container ref, per-index size estimate, off-screen buffer, and axis.
- [03]-[LAYOUT_OPTIONS]: dynamic measurement, multi-column count, inter-item gap, padding, and the window-offset margin.
- [04]-[CONTROL_HOOKS]: stable keys, the overscan/sticky-index extractor, custom scroll animation, SSR offset, and enable gate.
- [05]-[STREAM_RESTORE]: tail-anchor (`anchorTo: 'end'`) and follow-on-append for growing chat/log streams; the measured-item cache seed that restores scroll across remount; per-item lane assignment by estimate or measurement.
- [06]-[PERF_OPTIONS]: the scroll-only direct-DOM fast path and its positioning mode; the advanced surface for long lists.

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: constructing a virtualizer and rendering the window
- rail: view

| [INDEX] | [SURFACE]                                                                            | [ENTRY_FAMILY]  |
| :-----: | :----------------------------------------------------------------------------------- | :-------------- |
|  [01]   | `useVirtualizer({ count, getScrollElement, estimateSize, overscan, ... })`           | build (element) |
|  [02]   | `useWindowVirtualizer({ count, estimateSize, scrollMargin, ... })`                   | build (window)  |
|  [03]   | `virtualizer.getVirtualItems()` / `.getTotalSize()`                                  | derive window   |
|  [04]   | `virtualizer.getVirtualIndexes()` / `virtualizer.range` (`{ startIndex, endIndex }`) | derive range    |

- [01]-[BUILD_ELEMENT]: virtualize inside a scroll container; the common table/list case.
- [02]-[BUILD_WINDOW]: virtualize against document scroll (no inner scroll container); `scrollMargin` offsets by the list's page position.
- [03]-[DERIVE_WINDOW]: the visible-item array and the full scroll height/width for the spacer element.
- [04]-[DERIVE_RANGE]: the raw index span before item materialization, for prefetch/telemetry.

[ENTRYPOINT_SCOPE]: dynamic measurement and imperative scroll
- rail: view

| [INDEX] | [SURFACE]                                                                                         | [ENTRY_FAMILY] |
| :-----: | :------------------------------------------------------------------------------------------------ | :------------- |
|  [01]   | `virtualizer.measureElement(node)` (as each item's `ref`) / `virtualizer.resizeItem(index, size)` | measure        |
|  [02]   | `virtualizer.scrollToIndex(index, { align, behavior })` / `.scrollToOffset(px, opts)`             | scroll to      |
|  [03]   | `virtualizer.scrollToEnd({ behavior })` / `.scrollBy(delta, opts)` / `.isAtEnd(threshold?)`       | tail           |
|  [04]   | `virtualizer.getOffsetForIndex(index, align)` / `.getDistanceFromEnd()` / `.takeSnapshot()`       | offset / probe |
|  [05]   | `virtualizer.measure()`                                                                           | reset          |

- [01]-[MEASURE]: real-size measurement via `ResizeObserver`; variable-height rows need no `estimateSize` precision.
- [02]-[SCROLL_TO]: jump/smooth-scroll a row into view; `align: 'center'` for a selected `GlobalId` row reveal.
- [03]-[TAIL]: jump to the stream tail, nudge by a delta, and test tail proximity — the append-follow and infinite-scroll trigger pair.
- [04]-[OFFSET_PROBE]: `getOffsetForIndex` returns the `[offset, align]` target tuple; `getDistanceFromEnd` feeds the next-page threshold; `takeSnapshot` emits the measured-item cache for restore.
- [05]-[RESET]: force a full re-measure when `estimateSize`/data changes.

[ENTRYPOINT_SCOPE]: framework-agnostic core primitives (`@tanstack/virtual-core`)
- rail: view

| [INDEX] | [SURFACE]                                                                                   | [ENTRY_FAMILY]   |
| :-----: | :------------------------------------------------------------------------------------------ | :--------------- |
|  [01]   | `elementScroll` / `windowScroll`                                                            | scroll defaults  |
|  [02]   | `observeElementRect` / `observeElementOffset` / `observeWindowRect` / `observeWindowOffset` | observe defaults |
|  [03]   | `defaultRangeExtractor(range)` / `defaultKeyExtractor(index)`                               | default hook     |
|  [04]   | `measureElement` / `notUndefined` / `approxEqual` / `debounce` / `memo`                     | core util        |

- [01]-[SCROLL_DEFAULTS]: the `scrollToFn` implementations the React hook wires by default; overridable for a custom scroll container.
- [02]-[OBSERVE_DEFAULTS]: the rect/offset observers the hook wires by default for an element or window scroll source.
- [03]-[DEFAULT_HOOK]: the baseline overscan and key strategy a custom `rangeExtractor`/`getItemKey` extends (e.g. union a pinned index for a sticky header).
- [04]-[CORE_UTIL]: the DPR-aware element measurer and the internal fold/compare utilities the core exposes.

## [04]-[IMPLEMENTATION_LAW]

[VIRTUAL_TOPOLOGY]:
- Headless and measurement-first: the virtualizer computes offsets from `count` + `estimateSize`, then corrects with real measurements when each item element is passed to `measureElement` as its `ref`. Variable-height content needs no exact estimate — the `ResizeObserver` settles it — so `ui` renders a scroll container of `getTotalSize()` and absolutely positions each `getVirtualItems()` entry at its `start`.
- Core + adapter split: `@tanstack/virtual-core` owns the `Virtualizer` class, options, item math, and the scroll/observe primitives; `@tanstack/react-virtual` adds only `useVirtualizer`/`useWindowVirtualizer` (wire the core into a React refresh) and the `ReactVirtualizer` direct-DOM extension. Identical architecture to `@tanstack/react-table` over `table-core`, so the two collection halves compose without impedance.
- `lanes` makes grid/masonry one option: a multi-column grid or a Pinterest-style masonry is `lanes: n` — each `VirtualItem` carries its `lane`, and `ui` positions the cross-axis (`left: lane * (100/lanes)%`), while the engine still virtualizes the main axis. No separate grid virtualizer.
- `rangeExtractor` is the single parameterization for overscan and pinned items: it receives the computed `{ startIndex, endIndex, overscan, count }` and returns the index array to render. Sticky headers, always-mounted pinned rows, and custom overscan are all one extractor fn unioning indices onto `defaultRangeExtractor`, never a special code path.
- `directDomUpdates` is the scroll-perf escape hatch: with it enabled the virtualizer writes each item's position (`transform: translate3d` by default, or `top`/`left` in `'position'` mode) and the container's main-axis size straight to the DOM during scroll, re-rendering React only when the visible index range or `isScrolling` changes — the fast path for long lists, at the cost of a strict item-styling contract (absolute-positioned items, `containerRef` on the size container).

[STACKS_WITH]:
- `@tanstack/react-table` (`.api/tanstack-react-table.md`): the sibling half of the `view/compose` collection rows — `useVirtualizer({ count: table.getRowModel().rows.length, estimateSize, measureElement })` windows the derived rows into a virtualized data grid; sort/filter/group derivation stays in the table, windowing stays here, and the two share the headless-core + React-adapter shape.
- `react-aria` / `react-aria-components` (`.api/react-aria-components.md`): virtualization must not break the accessibility tree — the grid keeps its `aria-rowcount`/`aria-rowindex` on the full logical count while only the visible rows mount, and `scrollToIndex` pairs with react-aria focus management so keyboard navigation reveals off-screen rows.
- `@floating-ui/react` (`.api/floating-ui-react.md`): the reciprocal combobox/listbox seam — because only `getVirtualItems()` mount, `useListNavigation` must run `virtual: true` so focus stays on the input via `aria-activedescendant` (an unmounted off-screen row cannot hold real DOM focus), and its single `activeIndex`/`onNavigate` drives `scrollToIndex(activeIndex, { align })` to reveal the active row. floating-ui's own `scrollItemIntoView` is turned off — it can only scroll a mounted node — so this engine's imperative scroll owns row reveal; `useListNavigation`'s `listRef` holds only the windowed items while nav math runs over the full `count`, and `inner`/`useInnerOffset` anchors the fixed-height float geometry at the reference while this engine owns the inner `getTotalSize()` spacer and per-row offsets.
- `@effect-atom/atom-react` (`.api/effect-atom-atom-react.md`): the scroll offset and selected-index derive from state atoms; a `GlobalId` selection atom drives `scrollToIndex(align: 'center')` to reveal the row the `viewer/mark/selection` set just picked, keeping table selection and viewport in sync through one fold, and `takeSnapshot()`/`initialMeasurementsCache` round-trip the measured window through a persistence atom so a remount restores exact scroll and item sizes.
- `effect` (`.api/effect.md`): infinite/windowed data streams from an `AtomHttpApi` feed `count` and the row buffer; `getDistanceFromEnd()`/`isAtEnd()` gate the next `Effect` page fetch and `anchorTo: 'end'` + `followOnAppend` pin a growing log/chat stream to its tail, so windowing and data loading compose on one rail rather than an ad-hoc scroll listener.
- `@use-gesture/react` (`.api/use-gesture-react.md`): momentum/drag gestures on touch surfaces drive `scrollToOffset` with a custom `scrollToFn`, so the virtualizer's scroll animation and the gesture plane share one motion model.

[LOCAL_ADMISSION]:
- Use `useVirtualizer`/`useWindowVirtualizer` with `measureElement` as each item's `ref`; never hard-code a fixed row height or hand-roll an `IntersectionObserver` windowing loop — the measurement path handles variable content.
- Model grids and masonry with `lanes`; never nest two virtualizers or fork a grid-specific virtualizer where one `lanes` option fits.
- Put sticky headers, pinned rows, and custom overscan in one `rangeExtractor` over `defaultRangeExtractor`; never special-case a pinned index in the render loop.
- Window a table over `table.getRowModel().rows` and keep derivation in `@tanstack/react-table`; never re-implement sort/filter beside the virtualizer.
- For a virtualized combobox/listbox, run `@floating-ui/react` `useListNavigation` with `virtual: true` and drive `scrollToIndex` from its `onNavigate`; never leave floating-ui's `scrollItemIntoView` on over a windowed list — it can only scroll a mounted row, so off-screen active rows never reveal.
- Reach for `directDomUpdates` only when a profiled long list needs it, and honor its item-styling contract (absolute items, `containerRef`, no author-set main-axis size); never toggle it at runtime.

[RAIL_LAW]:
- Package: `@tanstack/react-virtual` (over `@tanstack/virtual-core`)
- Owns: the `Virtualizer` windowing engine, the `VirtualItem` offset model, dynamic `ResizeObserver` measurement, `lanes` multi-column layout, the `rangeExtractor` overscan/pin control, imperative `scrollToIndex`/`scrollToOffset`/`scrollToEnd`, tail probes (`isAtEnd`/`getDistanceFromEnd`) with `anchorTo`/`followOnAppend`, `takeSnapshot`/`initialMeasurementsCache` restore, and the `directDomUpdates` scroll-perf path
- Accept: headless measurement-driven windowing with `ui`-owned markup, `lanes` for grids/masonry, one `rangeExtractor` for sticky/overscan, react-table row windowing, `@effect-atom`-driven scroll/selection sync and snapshot restore, `Effect` paged data feeding `count` with tail-probe paging and `anchorTo: 'end'` streams, a `virtual: true` combobox binding `useListNavigation`'s `activeIndex`/`onNavigate` to `scrollToIndex`, the direct-DOM path under its styling contract
- Reject: fixed row heights or hand-rolled observer windowing, nested/forked grid virtualizers, per-render pinned-index special cases, sort/filter re-implemented beside windowing, runtime toggling of `directDomUpdates` or violating its item-styling contract, real-DOM-focus (non-`virtual`) `useListNavigation` or floating-ui `scrollItemIntoView` over a windowed combobox
