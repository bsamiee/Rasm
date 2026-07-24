# [TS_UI_API_TANSTACK_REACT_VIRTUAL]

`@tanstack/react-virtual` owns viewport windowing: from an item `count` and a size estimate it computes the intersecting items and their absolute offsets, so a hundred-thousand-row list mounts only the visible span and a small overscan. Headless and measurement-driven, it adapts the framework-agnostic `@tanstack/virtual-core` as the windowing half of the `view/compose` rows beside the headless table.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `@tanstack/react-virtual`
- package: `@tanstack/react-virtual` (MIT)
- module: ESM (`type: module`), `sideEffects: false`; first-party bundled `.d.ts` re-exporting the whole `@tanstack/virtual-core` surface
- runtime: React render tree over a DOM scroll element or the window; the core is DOM-free and framework-agnostic; peer `react`/`react-dom` via the folder React spine
- rail: view composition plane — the windowing half of the `view/compose` table/virtual rows

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: the virtualizer instance and the virtual item

| [INDEX] | [SYMBOL]                                                                                   | [TYPE_FAMILY]        |
| :-----: | :----------------------------------------------------------------------------------------- | :------------------- |
|  [01]   | `Virtualizer<TScrollElement, TItemElement>`                                                | virtualizer          |
|  [02]   | `VirtualItem` (`{ key, index, start, end, size, lane }`)                                   | window item          |
|  [03]   | `ReactVirtualizer` (adds `containerRef`)                                                   | React virtualizer    |
|  [04]   | `Range` / `ScrollToOptions` (`{ align, behavior }`) / `ScrollAlignment` / `ScrollBehavior` | window / scroll opts |

- [01]-[VIRTUALIZER]: `Virtualizer` is the framework-agnostic engine; `useVirtualizer` returns a `ReactVirtualizer` wrapping it.
- [02]-[WINDOW_ITEM]: `VirtualItem` carries one visible item's absolute `start`, measured `size`, and grid `lane`; the render loop maps each.
- [03]-[REACT_VIRTUALIZER]: `ReactVirtualizer` is the hook return; `containerRef` binds the inner size container for the direct-DOM path.
- [04]-[WINDOW_SCROLL_OPTS]: `Range` is the computed index span; `ScrollToOptions` binds scroll targets — `ScrollAlignment` `'start'`/`'center'`/`'end'`/`'auto'`, `ScrollBehavior` `'auto'`/`'smooth'`/`'instant'`.

[PUBLIC_TYPE_SCOPE]: the options bag — one parameterized windowing contract

| [INDEX] | [SYMBOL]                                                                                                             | [TYPE_FAMILY]    |
| :-----: | :------------------------------------------------------------------------------------------------------------------- | :--------------- |
|  [01]   | `VirtualizerOptions` / `ReactVirtualizerOptions` / `PartialKeys`                                                     | options bag      |
|  [02]   | `count` / `getScrollElement` / `estimateSize(index)` / `overscan` / `horizontal`                                     | core options     |
|  [03]   | `measureElement` / `lanes` / `gap` / `paddingStart` / `paddingEnd` / `scrollMargin`                                  | layout options   |
|  [04]   | `getItemKey(index)` / `rangeExtractor(range)` / `scrollToFn` / `initialOffset` / `enabled` / `isScrollingResetDelay` | control hooks    |
|  [05]   | `anchorTo` / `followOnAppend` / `initialMeasurementsCache` / `laneAssignmentMode`                                    | stream + restore |
|  [06]   | `directDomUpdates` / `directDomUpdatesMode` (`'transform'\|'position'`) / `useFlushSync`                             | perf options     |

- [01]-[OPTIONS_BAG]: `VirtualizerOptions` is the full contract; `useVirtualizer` defaults `observeElementRect`/`observeElementOffset`/`scrollToFn`, so a call needs only `count`/`getScrollElement`/`estimateSize`.
- [02]-[CORE_OPTIONS]: item `count`, scroll container ref, per-index size estimate, off-screen buffer, and axis.
- [03]-[LAYOUT_OPTIONS]: dynamic measurement, multi-column count, inter-item `gap`, padding, and the window-offset `scrollMargin`.
- [04]-[CONTROL_HOOKS]: stable keys, the `rangeExtractor` overscan/sticky control, custom `scrollToFn` animation, SSR `initialOffset`, and enable gate.
- [05]-[STREAM_RESTORE]: `anchorTo: 'end'` and `followOnAppend` pin a growing chat/log stream to its tail; `initialMeasurementsCache` seeds measured items to restore scroll across remount; `laneAssignmentMode` assigns lanes by estimate or measurement.
- [06]-[PERF_OPTIONS]: `directDomUpdates` is the scroll-only direct-DOM fast path; `directDomUpdatesMode` picks its positioning, the advanced surface for long lists.

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: constructing a virtualizer and rendering the window

| [INDEX] | [SURFACE]                                                                            | [ENTRY_FAMILY]  |
| :-----: | :----------------------------------------------------------------------------------- | :-------------- |
|  [01]   | `useVirtualizer({ count, getScrollElement, estimateSize, overscan, ... })`           | build (element) |
|  [02]   | `useWindowVirtualizer({ count, estimateSize, scrollMargin, ... })`                   | build (window)  |
|  [03]   | `virtualizer.getVirtualItems()` / `.getTotalSize()`                                  | derive window   |
|  [04]   | `virtualizer.getVirtualIndexes()` / `virtualizer.range` (`{ startIndex, endIndex }`) | derive range    |

- [01]-[BUILD_ELEMENT]: `useVirtualizer` virtualizes inside a scroll container — the common table/list case.
- [02]-[BUILD_WINDOW]: `useWindowVirtualizer` virtualizes against document scroll; `scrollMargin` offsets by the list's page position.
- [03]-[DERIVE_WINDOW]: `getVirtualItems` yields the visible-item array, `getTotalSize` the full scroll extent for the spacer element.
- [04]-[DERIVE_RANGE]: `getVirtualIndexes`/`range` give the raw index span before item materialization, for prefetch/telemetry.

[ENTRYPOINT_SCOPE]: dynamic measurement and imperative scroll

| [INDEX] | [SURFACE]                                                                                         | [ENTRY_FAMILY] |
| :-----: | :------------------------------------------------------------------------------------------------ | :------------- |
|  [01]   | `virtualizer.measureElement(node)` (as each item's `ref`) / `virtualizer.resizeItem(index, size)` | measure        |
|  [02]   | `virtualizer.scrollToIndex(index, { align, behavior })` / `.scrollToOffset(px, opts)`             | scroll to      |
|  [03]   | `virtualizer.scrollToEnd({ behavior })` / `.scrollBy(delta, opts)` / `.isAtEnd(threshold?)`       | tail           |
|  [04]   | `virtualizer.getOffsetForIndex(index, align)` / `.getDistanceFromEnd()` / `.takeSnapshot()`       | offset / probe |
|  [05]   | `virtualizer.measure()`                                                                           | reset          |

- [01]-[MEASURE]: `measureElement` reads real sizes via `ResizeObserver`; variable-height rows need no `estimateSize` precision.
- [02]-[SCROLL_TO]: `scrollToIndex`/`scrollToOffset` jump or smooth-scroll a row into view; `align: 'center'` reveals a selected `GlobalId` row.
- [03]-[TAIL]: `scrollToEnd` jumps to the stream tail, `scrollBy` nudges by a delta, `isAtEnd` tests tail proximity — the append-follow and infinite-scroll trigger pair.
- [04]-[OFFSET_PROBE]: `getOffsetForIndex` returns the `[offset, align]` target tuple; `getDistanceFromEnd` feeds the next-page threshold; `takeSnapshot` emits the measured-item cache for restore.
- [05]-[RESET]: `measure` forces a full re-measure when `estimateSize` or data changes.

[ENTRYPOINT_SCOPE]: framework-agnostic core primitives (`@tanstack/virtual-core`)

| [INDEX] | [SURFACE]                                                                                   | [ENTRY_FAMILY]   |
| :-----: | :------------------------------------------------------------------------------------------ | :--------------- |
|  [01]   | `elementScroll` / `windowScroll`                                                            | scroll defaults  |
|  [02]   | `observeElementRect` / `observeElementOffset` / `observeWindowRect` / `observeWindowOffset` | observe defaults |
|  [03]   | `defaultRangeExtractor(range)` / `defaultKeyExtractor(index)`                               | default hook     |
|  [04]   | `measureElement` / `notUndefined` / `approxEqual` / `debounce` / `memo`                     | core util        |

- [01]-[SCROLL_DEFAULTS]: `elementScroll`/`windowScroll` are the `scrollToFn` defaults the React hook wires; override for a custom scroll container.
- [02]-[OBSERVE_DEFAULTS]: rect/offset observers the hook wires by default for an element or window scroll source.
- [03]-[DEFAULT_HOOK]: `defaultRangeExtractor`/`defaultKeyExtractor` are the baselines a custom `rangeExtractor`/`getItemKey` extends — union a pinned index for a sticky header.
- [04]-[CORE_UTIL]: `measureElement` is the DPR-aware element measurer; `notUndefined`/`approxEqual`/`debounce`/`memo` are the exposed fold/compare utilities.

## [04]-[IMPLEMENTATION_LAW]

[TOPOLOGY]:
- `Virtualizer` computes offsets from `count` + `estimateSize`, then corrects with real measurements as each item element passes to `measureElement` as its `ref`; the `ResizeObserver` settles variable-height content, so `ui` renders a `getTotalSize()` scroll container and absolutely positions each `getVirtualItems()` entry at its `start`.
- `@tanstack/virtual-core` owns the `Virtualizer` class, options, item math, and the scroll/observe primitives; `@tanstack/react-virtual` adds only `useVirtualizer`/`useWindowVirtualizer` and the `ReactVirtualizer` direct-DOM extension.
- `lanes: n` makes a grid or masonry one option: each `VirtualItem` carries its `lane` and `ui` positions the cross-axis (`left: lane * (100/lanes)%`) while the engine virtualizes the main axis — no separate grid virtualizer.
- `rangeExtractor` is the single parameterization for overscan and pinned items: it receives `{ startIndex, endIndex, overscan, count }` and returns the render index array, so sticky headers, always-mounted pinned rows, and custom overscan are one fn unioning indices onto `defaultRangeExtractor`.
- `directDomUpdates` writes each item's position (`transform: translate3d` by default, or `top`/`left` in `'position'` mode) and the container main-axis size straight to the DOM during scroll, re-rendering React only when the visible index range or `isScrolling` changes — the long-list fast path, under a strict item-styling contract (absolute items, `containerRef` on the size container).

[STACKING]:
- `@tanstack/react-table` (`.api/tanstack-react-table.md`): the sibling half of the `view/compose` collection rows — `useVirtualizer({ count: table.getRowModel().rows.length, estimateSize, measureElement })` windows the derived rows into a virtualized data grid; sort/filter/group derivation stays in the table, windowing stays here, and the two share the headless-core + React-adapter shape.
- `react-aria` / `react-aria-components` (`.api/react-aria-components.md`): the grid keeps its `aria-rowcount`/`aria-rowindex` on the full logical count while only visible rows mount, and `scrollToIndex` pairs with react-aria focus management so keyboard navigation reveals off-screen rows.
- `@floating-ui/react` (`.api/floating-ui-react.md`): the combobox/listbox seam — because only `getVirtualItems()` mount, `useListNavigation` runs `virtual: true` so focus rides `aria-activedescendant` on the input, and its `activeIndex`/`onNavigate` drives `scrollToIndex(activeIndex, { align })`; floating-ui's `scrollItemIntoView` stays off (it scrolls only a mounted node), this engine's imperative scroll owning reveal, `listRef` holding the windowed items while nav math runs over the full `count`, and `inner`/`useInnerOffset` anchoring the float geometry while this engine owns the `getTotalSize()` spacer.
- `@effect-atom/atom-react` (`.api/effect-atom-atom-react.md`): a `GlobalId` selection atom drives `scrollToIndex(align: 'center')` to reveal the row the `viewer/mark/selection` set picked, keeping table selection and viewport in sync through one fold; `takeSnapshot()`/`initialMeasurementsCache` round-trip the measured window through a persistence atom so a remount restores exact scroll and item sizes.
- `effect` (`libs/typescript/.api/effect.md`): infinite/windowed streams from an `AtomHttpApi` feed `count` and the row buffer; `getDistanceFromEnd()`/`isAtEnd()` gate the next `Effect` page fetch and `anchorTo: 'end'` + `followOnAppend` pin a growing log/chat stream to its tail, so windowing and data loading compose on one rail.
- `@use-gesture/react` (`.api/use-gesture-react.md`): momentum/drag gestures on touch surfaces drive `scrollToOffset` through a custom `scrollToFn`, so the virtualizer's scroll animation and the gesture plane share one motion model.

[LOCAL_ADMISSION]:
- Use `useVirtualizer`/`useWindowVirtualizer` with `measureElement` as each item's `ref`; the measurement path handles variable content, never a fixed row height or a hand-rolled `IntersectionObserver` windowing loop.
- Model grids and masonry with `lanes`: one option owns the cross-axis, never a nested or grid-specific virtualizer.
- Put sticky headers, pinned rows, and custom overscan in `rangeExtractor` over `defaultRangeExtractor`, never a pinned-index special case in the render loop.
- Window a table over `table.getRowModel().rows` and keep derivation in `@tanstack/react-table`, never sort/filter re-implemented beside the virtualizer.
- Run a virtualized combobox/listbox through `@floating-ui/react` `useListNavigation` with `virtual: true` and drive `scrollToIndex` from its `onNavigate`; floating-ui's `scrollItemIntoView` scrolls only a mounted row, so this engine's imperative scroll owns off-screen reveal.
- Reach for `directDomUpdates` on a profiled long list and honor its item-styling contract (absolute items, `containerRef`, no author-set main-axis size); set it once at mount, never toggled at runtime.

[RAIL_LAW]:
- Package: `@tanstack/react-virtual` (over `@tanstack/virtual-core`)
- Owns: the `Virtualizer` windowing engine, the `VirtualItem` offset model, dynamic `ResizeObserver` measurement, `lanes` multi-column layout, the `rangeExtractor` overscan/pin control, imperative `scrollToIndex`/`scrollToOffset`/`scrollToEnd`, tail probes (`isAtEnd`/`getDistanceFromEnd`) with `anchorTo`/`followOnAppend`, `takeSnapshot`/`initialMeasurementsCache` restore, and the `directDomUpdates` scroll-perf path
- Accept: headless measurement-driven windowing with `ui`-owned markup, `lanes` for grids/masonry, one `rangeExtractor` for sticky/overscan, react-table row windowing, `@effect-atom`-driven scroll/selection sync and snapshot restore, `Effect` paged data feeding `count` with tail-probe paging and `anchorTo: 'end'` streams, a `virtual: true` combobox binding `useListNavigation`'s `activeIndex`/`onNavigate` to `scrollToIndex`, the direct-DOM path under its styling contract
- Reject: fixed row heights or hand-rolled observer windowing, nested/forked grid virtualizers, per-render pinned-index special cases, sort/filter re-implemented beside windowing, runtime toggling of `directDomUpdates` or violating its item-styling contract, real-DOM-focus (non-`virtual`) `useListNavigation` or floating-ui `scrollItemIntoView` over a windowed combobox
