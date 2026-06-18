# [API_CATALOGUE] lucide-react

`lucide-react` ships ~1 500 SVG icon components as named `ForwardRefExoticComponent` exports, all typed as `LucideIcon`. The `LucideProps` interface extends standard SVG attributes with `size`, `absoluteStrokeWidth`, and `strokeWidth`. `Icon` is a dynamic runtime component that accepts `iconNode` data; `createLucideIcon` produces a new `LucideIcon` from raw node data. `LucideProvider` injects default prop values via context.

## [1]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `lucide-react`
- package: `lucide-react`
- namespace: `lucide-react`
- asset: runtime component library
- rail: ui-icons

## [2]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: icon prop and component types
- rail: ui-icons

| [INDEX] | [SYMBOL]              | [TYPE_FAMILY] | [RAIL]                                                                                  |
| :-----: | :-------------------- | :------------ | :-------------------------------------------------------------------------------------- |
|   [1]   | `LucideProps`         | interface     | `ElementAttributes & { size?, absoluteStrokeWidth? }`                                   |
|   [2]   | `LucideIcon`          | type alias    | `ForwardRefExoticComponent<Omit<LucideProps,'ref'> & RefAttributes<SVGSVGElement>>`     |
|   [3]   | `LucideConfig`        | type alias    | global default prop config shape for `LucideProvider`                                   |
|   [4]   | `LucideProviderProps` | type alias    | `LucideConfig & { children: ReactNode }`                                                |
|   [5]   | `IconNode`            | type alias    | `[elementName: SVGElementType, attrs: Record<string, string>][]`                        |
|   [6]   | `SVGAttributes`       | type alias    | `Partial<SVGProps<SVGSVGElement>>`                                                      |
|   [7]   | `ElementAttributes`   | type alias    | `RefAttributes<SVGSVGElement> & SVGAttributes`                                          |
|   [8]   | `SVGElementType`      | type alias    | `'circle' \| 'ellipse' \| 'g' \| 'line' \| 'path' \| 'polygon' \| 'polyline' \| 'rect'` |

## [3]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: icon components (representative sample)
- rail: ui-icons

| [INDEX] | [SURFACE]                              | [ENTRY_FAMILY] | [RAIL]                                                             |
| :-----: | :------------------------------------- | :------------- | :----------------------------------------------------------------- |
|   [1]   | `<IconName />`                         | icon component | any of the ~1 500 named `LucideIcon` exports                       |
|   [2]   | `Icon`                                 | dynamic icon   | `ForwardRefExoticComponent<IconComponentProps>` — takes `iconNode` |
|   [3]   | `createLucideIcon(iconName, iconNode)` | factory        | produces a `LucideIcon` from raw `IconNode` data                   |

[ENTRYPOINT_SCOPE]: context and utility
- rail: ui-icons

| [INDEX] | [SURFACE]                                                                                | [ENTRY_FAMILY] | [RAIL]                                    |
| :-----: | :--------------------------------------------------------------------------------------- | :------------- | :---------------------------------------- |
|   [1]   | `LucideProvider({ children, size, color, strokeWidth, absoluteStrokeWidth, className })` | provider       | injects default `LucideProps` via context |
|   [2]   | `useLucideContext()`                                                                     | hook           | returns the active `LucideProps` defaults |

## [4]-[IMPLEMENTATION_LAW]

[ICON_TOPOLOGY]:
- all named icon exports are `ForwardRefExoticComponent<Omit<LucideProps,'ref'> & RefAttributes<SVGSVGElement>>`; the ref forwards to the `<svg>` element
- `size` controls both `width` and `height`; defaults to `24` when not set via props or `LucideProvider`
- `absoluteStrokeWidth` computes stroke as `strokeWidth / size` so lines stay constant thickness at all sizes
- `strokeWidth` default is `2`; override per icon or via `LucideProvider`
- `createLucideIcon(name, iconNode)` produces a stable `LucideIcon`; use it to register custom icons in the same shape
- `Icon` accepts an `iconNode` prop for fully dynamic icon rendering without a named import
- tree-shaking is supported: named imports from `lucide-react` are individually bundled; no side-effect-laden barrel

[LOCAL_ADMISSION]:
- Import named icons directly: `import { Activity } from 'lucide-react'`.
- Wrap with `LucideProvider` at the app root to set project-wide defaults for `size`, `strokeWidth`, and `color`.
- Use `Icon` + dynamic `iconNode` only for icons chosen at runtime; prefer static named imports for all other cases.

[RAIL_LAW]:
- package: `lucide-react`
- owns: SVG icon components with consistent `LucideProps` surface
- accept: named icon imports, `LucideProvider` for default overrides, `createLucideIcon` for custom entries
- reject: hand-crafting SVG icon components for icons already present in this package
