# [API_CATALOGUE] @tanstack/react-virtual

`@tanstack/react-virtual` supplies React hook wrappers over `@tanstack/virtual-core` that bind a `Virtualizer` to a React component's scroll element. `useVirtualizer` targets a container `Element`; `useWindowVirtualizer` targets `Window`. Both return a `ReactVirtualizer` — a `Virtualizer` extended with `containerRef` for `directDomUpdates` mode — and re-export the full `virtual-core` surface including `VirtualItem`, `VirtualizerOptions`, `Range`, `Rect`, and all default-observer/scroll helpers.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `@tanstack/react-virtual`
- package: `@tanstack/react-virtual`
- module: `@tanstack/react-virtual`
- asset: `dist/esm/index.d.ts` — re-exports `@tanstack/virtual-core`
- rail: viewport

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: react-specific extension types
- rail: viewport

| [INDEX] | [SYMBOL]                                         | [TYPE_FAMILY]  | [RAIL]                              |
| :-----: | :----------------------------------------------- | :------------- | :---------------------------------- |
|  [01]   | `ReactVirtualizer<TScrollElement, TItemElement>` | extended class | `Virtualizer` + `containerRef`      |
|  [02]   | `ReactVirtualizerOptions<TScrollElement, TItem>` | options type   | `VirtualizerOptions` + react fields |

[PUBLIC_TYPE_SCOPE]: virtual-core item and range types
- rail: viewport

| [INDEX] | [SYMBOL]                                           | [TYPE_FAMILY] | [RAIL]                              |
| :-----: | :------------------------------------------------- | :------------ | :---------------------------------- |
|  [01]   | `VirtualItem`                                      | interface     | rendered item descriptor            |
|  [02]   | `Range`                                            | interface     | start/end/overscan/count window     |
|  [03]   | `Rect`                                             | interface     | `{ width: number; height: number }` |
|  [04]   | `ScrollToOptions`                                  | interface     | `{ align?, behavior? }`             |
|  [05]   | `VirtualizerOptions<TScrollElement, TItemElement>` | interface     | full constructor options            |

[PUBLIC_TYPE_SCOPE]: virtual-core utility types
- rail: viewport

| [INDEX] | [SYMBOL]            | [TYPE_FAMILY] | [RAIL]                 |
| :-----: | :------------------ | :------------ | :--------------------- |
|  [01]   | `PartialKeys<T, K>` | type utility  | marks keys optional    |
|  [02]   | `NoInfer<T>`        | type utility  | prevents type-widening |

[PUBLIC_TYPE_SCOPE]: Virtualizer class
- rail: viewport

| [INDEX] | [SYMBOL]                                    | [TYPE_FAMILY] | [RAIL]                                    |
| :-----: | :------------------------------------------ | :------------ | :---------------------------------------- |
|  [01]   | `Virtualizer<TScrollElement, TItemElement>` | class         | scroll-position aware item window manager |

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: React hooks
- rail: viewport

| [INDEX] | [SURFACE]                       | [ENTRY_FAMILY] | [RAIL]                               |
| :-----: | :------------------------------ | :------------- | :----------------------------------- |
|  [01]   | `useVirtualizer(options)`       | element hook   | `ReactVirtualizer<Element, Element>` |
|  [02]   | `useWindowVirtualizer(options)` | window hook    | `ReactVirtualizer<Window, Element>`  |

[ENTRYPOINT_SCOPE]: Virtualizer instance operations
- rail: viewport

| [INDEX] | [SURFACE]                          | [ENTRY_FAMILY] | [RAIL]                             |
| :-----: | :--------------------------------- | :------------- | :--------------------------------- |
|  [01]   | `getVirtualItems()`                | item query     | rendered `VirtualItem[]`           |
|  [02]   | `getTotalSize()`                   | size query     | total scrollable axis size in px   |
|  [03]   | `scrollToIndex(index, options?)`   | imperative     | scrolls to item at index           |
|  [04]   | `scrollToOffset(offset, options?)` | imperative     | scrolls to pixel offset            |
|  [05]   | `scrollBy(delta, options?)`        | imperative     | relative scroll by delta           |
|  [06]   | `scrollToEnd(options?)`            | imperative     | scrolls to end of list             |
|  [07]   | `measureElement(node)`             | measurement    | measures DOM element for item size |
|  [08]   | `resizeItem(index, size)`          | measurement    | overrides item size imperatively   |
|  [09]   | `takeSnapshot()`                   | serialization  | `VirtualItem[]` for cache restore  |
|  [10]   | `getVirtualItemForOffset(offset)`  | point query    | `VirtualItem \| undefined`         |
|  [11]   | `getDistanceFromEnd()`             | scroll state   | pixels remaining to end            |
|  [12]   | `isAtEnd(threshold?)`              | scroll state   | boolean end detection              |
|  [13]   | `calculateRange()`                 | range query    | `{ startIndex, endIndex } \| null` |
|  [14]   | `measure()`                        | remeasure      | forces full remeasure pass         |

[ENTRYPOINT_SCOPE]: virtual-core helpers (re-exported)
- rail: viewport

| [INDEX] | [SURFACE]               | [ENTRY_FAMILY] | [RAIL]                               |
| :-----: | :---------------------- | :------------- | :----------------------------------- |
|  [01]   | `observeElementRect`    | observer       | attaches `ResizeObserver` on element |
|  [02]   | `observeWindowRect`     | observer       | observes window size                 |
|  [03]   | `observeElementOffset`  | observer       | attaches scroll listener on element  |
|  [04]   | `observeWindowOffset`   | observer       | attaches scroll listener on window   |
|  [05]   | `measureElement`        | measurement    | reads `ResizeObserverEntry` size     |
|  [06]   | `elementScroll`         | scroll fn      | programmatic element scroll          |
|  [07]   | `windowScroll`          | scroll fn      | programmatic window scroll           |
|  [08]   | `defaultKeyExtractor`   | key fn         | `(index) => index`                   |
|  [09]   | `defaultRangeExtractor` | range fn       | default visible-index extraction     |

## [04]-[IMPLEMENTATION_LAW]

[VIEWPORT_TOPOLOGY]:
- `useVirtualizer` pre-fills `observeElementRect`, `observeElementOffset`, and `scrollToFn` — consumers provide `count`, `getScrollElement`, and `estimateSize` only
- `useWindowVirtualizer` additionally pre-fills `getScrollElement` pointing to `window`
- `ReactVirtualizerOptions.directDomUpdates: true` writes `top`/`left` or `transform: translate3d(…)` to item elements directly, bypassing React re-renders for scroll-only updates
- `directDomUpdatesMode: 'transform'` (default) requires `position: absolute; top: 0; left: 0` on items; `'position'` requires `position: absolute` only
- `directDomUpdates` requires `containerRef` on the inner size container element; the container must not set `height`/`width` in style
- `ReactVirtualizer.containerRef` is meaningful only when `directDomUpdates: true`
- `VirtualItem.lane` carries the lane index for multi-lane (masonry/grid) layouts; `options.lanes` controls lane count
- `initialMeasurementsCache` accepts a `VirtualItem[]` snapshot from `takeSnapshot()` + a saved `scrollOffset` to restore position after navigation

[LOCAL_ADMISSION]:
- Pass `getScrollElement` returning `null` during SSR; virtualizer handles null safely.
- Item element measurement wires through `virtualizer.measureElement` as a React callback ref.
- `indexAttribute` (default `"data-index"`) must match the attribute on each item element for `indexFromElement` to function.

[RAIL_LAW]:
- package: `@tanstack/react-virtual` (core: `@tanstack/virtual-core`)
- Owns: React-integrated list/grid/masonry virtualization
- Accept: `useVirtualizer` / `useWindowVirtualizer` for all scroll-element virtualization; `directDomUpdates` for scroll-performance-critical lists
- Reject: hand-rolled intersection-observer virtualization; manual visible-window computation
