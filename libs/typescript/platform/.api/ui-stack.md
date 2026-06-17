# [API_CATALOGUE] ui-stack

Grounded from installed `node_modules` type declarations (react 19.2.7, @types/react 19.2.17,
react-aria 3.49.0, @radix-ui/react-slot 1.2.5 / react-label 2.1.9 / react-separator 1.1.9,
@tanstack/react-table 8.21.3, @tanstack/react-virtual 3.14.2, vite 8.0.16,
@vitejs/plugin-react 6.0.2, vite-plugin-pwa 1.3.0) and unpkg-fetched type declarations for
react-aria-components 1.18.0 (not yet installed). Covers the surfaces the TS planning pages
consume — not an exhaustive module dump.

---

## [1] — react / react-dom

**Entry**: `react`, `react-dom/client`

### Core types

```ts
// react
interface ReactElement<P = unknown, T extends string | JSXElementConstructor<any> = ...> {
  type: T; props: P; key: string | null
}

type ReactNode =
  | ReactElement | string | number | bigint | Iterable<ReactNode>
  | ReactPortal | boolean | null | undefined | Promise<AwaitedReactNode>

interface FunctionComponent<P = {}> {
  (props: P): ReactNode | Promise<ReactNode>
  displayName?: string
}
type FC<P = {}> = FunctionComponent<P>

type PropsWithChildren<P = unknown> = P & { children?: ReactNode }
type ComponentProps<T extends keyof JSX.IntrinsicElements | JSXElementConstructor<any>> = ...
type ComponentPropsWithoutRef<T extends ElementType> = PropsWithoutRef<ComponentProps<T>>

interface RefObject<T> { readonly current: T | null }
type Dispatch<A> = (value: A) => void
type SetStateAction<S> = S | ((prevState: S) => S)
type DependencyList = readonly unknown[]

interface ForwardRefExoticComponent<P> extends NamedExoticComponent<P> {}
type RefAttributes<T> = { ref?: Ref<T> }
```

### Key hooks

```ts
function useState<S>(initialState: S | (() => S)): [S, Dispatch<SetStateAction<S>>]
function useState<S = undefined>(): [S | undefined, Dispatch<SetStateAction<S | undefined>>]

function useEffect(effect: () => void | Destructor, deps?: DependencyList): void
function useRef<T>(initialValue: T): RefObject<T>
function useRef<T>(initialValue: T | null): RefObject<T | null>
function useMemo<T>(factory: () => T, deps: DependencyList): T
function useCallback<T extends Function>(callback: T, deps: DependencyList): T
function useContext<T>(context: Context<T>): T

function forwardRef<T, P = {}>(
  render: ForwardRefRenderFunction<T, PropsWithoutRef<P>>
): ForwardRefExoticComponent<PropsWithoutRef<P> & RefAttributes<T>>

function memo<P extends object>(
  Component: FunctionComponent<P>,
  propsAreEqual?: (prevProps: Readonly<P>, nextProps: Readonly<P>) => boolean
): NamedExoticComponent<P>

function createContext<T>(defaultValue: T): Context<T>
```

### react-dom/client

```ts
// react-dom/client
function createRoot(
  container: Element | DocumentFragment,
  options?: RootOptions
): Root

interface Root {
  render(children: ReactNode): void
  unmount(): void
}
```

---

## [2] — react-aria-components

**Entry**: `react-aria-components`
**Declared types**: `dist/types/exports/index.d.ts`

The planning pages consume `react-aria-components` as the single accessible-primitive and
interaction-primitive surface. The backing low-level `react-aria` 3.49.0 hook library is its
implementation substrate; planning references only the component layer.

### Render-props pattern (shared across all components)

```ts
// All components with interaction states expose render-prop className
type ClassNameOrFunction<T> = string | ((state: T) => string)

// Shared
interface SlotProps { slot?: string | null }
interface RenderProps<T, DefaultEl extends ElementType = never> {
  className?: ClassNameOrFunction<T>
  children?: ReactNode | ((state: T) => ReactNode)
}
```

### Form controls

```ts
// Button
interface ButtonRenderProps {
  isHovered: boolean; isPressed: boolean; isFocused: boolean
  isFocusVisible: boolean; isDisabled: boolean; isPending: boolean
}
interface ButtonProps extends
  Omit<AriaButtonProps, 'children' | 'href' | 'target' | 'rel' | 'elementType'>,
  HoverEvents, SlotProps, RenderProps<ButtonRenderProps, 'button'> {
  className?: ClassNameOrFunction<ButtonRenderProps>
  isPending?: boolean
}
declare const Button: (props: ButtonProps & RefAttributes<HTMLButtonElement>) => ReactElement | null

// TextField
interface TextFieldProps extends
  Omit<AriaTextFieldProps, 'label' | 'placeholder' | 'description' | 'errorMessage' | ...>,
  RACValidation, SlotProps, RenderProps<TextFieldRenderProps> {
  className?: ClassNameOrFunction<TextFieldRenderProps>
  isInvalid?: boolean
}
declare const TextField: (props: TextFieldProps & RefAttributes<HTMLDivElement>) => ReactElement | null

// Form
interface FormProps extends SharedFormProps, DOMProps, GlobalDOMAttributes<HTMLFormElement> {
  className?: string
  validationBehavior?: 'aria' | 'native'
}
declare const Form: ForwardRefExoticComponent<FormProps & RefAttributes<HTMLFormElement>>

// Select — generic over item type T and selection mode M
interface SelectProps<T, M extends SelectionMode = 'single'> extends
  Omit<AriaSelectProps<T, M>, 'children' | 'label' | 'description' | 'errorMessage' | ...>,
  RACValidation, RenderProps<SelectRenderProps>, SlotProps {
  className?: ClassNameOrFunction<SelectRenderProps>
}
declare const Select: <T, M extends SelectionMode = 'single'>(
  props: SelectProps<T, M> & RefAttributes<HTMLDivElement>
) => ReactElement | null
declare const SelectValue: <T>(props: SelectValueProps<T> & RefAttributes<HTMLSpanElement>) => ReactElement | null

// ComboBox — generic over item type T and selection mode M
interface ComboBoxProps<T, M extends SelectionMode = 'single'> extends
  Omit<AriaComboBoxProps<T, M>, 'children' | 'placeholder' | 'label' | ...>,
  RACValidation, RenderProps<ComboBoxRenderProps>, SlotProps {
  className?: ClassNameOrFunction<ComboBoxRenderProps>
  defaultFilter?: (textValue: string, inputValue: string) => boolean
  formValue?: 'text' | 'key'
  allowsEmptyCollection?: boolean
}
declare const ComboBox: <T, M extends SelectionMode = 'single'>(
  props: ComboBoxProps<T, M> & RefAttributes<HTMLDivElement>
) => ReactElement | null

// NumberField, SearchField, DateField, TimeField — follow the same TextFieldProps shape
// (omit AriaXxxProps label/description/errorMessage, add RACValidation, RenderProps, SlotProps)
```

### Overlay and dialog

```ts
// Dialog
interface DialogRenderProps { close: () => void }
interface DialogProps extends AriaDialogProps, StyleProps, SlotProps {
  className?: string
  children?: ReactNode | ((opts: DialogRenderProps) => ReactNode)
}
declare const Dialog: (props: DialogProps & RefAttributes<HTMLElement>) => ReactElement | null
declare function DialogTrigger(props: { children: ReactNode } & OverlayTriggerProps): JSX.Element

// Modal / ModalOverlay
interface ModalRenderProps {
  isEntering: boolean; isExiting: boolean; state: OverlayTriggerState
}
interface ModalOverlayProps extends AriaModalOverlayProps, OverlayTriggerProps,
  RenderProps<ModalRenderProps>, SlotProps {
  className?: ClassNameOrFunction<ModalRenderProps>
  isEntering?: boolean; isExiting?: boolean
  UNSTABLE_portalContainer?: Element
}
declare const Modal: (props: ModalOverlayProps & RefAttributes<HTMLDivElement>) => ReactElement | null
declare const ModalOverlay: (props: ModalOverlayProps & RefAttributes<HTMLDivElement>) => ReactElement | null

// Popover
interface PopoverRenderProps {
  trigger: string | null; placement: PlacementAxis
  isEntering: boolean; isExiting: boolean
}
interface PopoverProps extends PositionProps, AriaPopoverProps, OverlayTriggerProps,
  RenderProps<PopoverRenderProps>, SlotProps, AriaLabelingProps {
  className?: ClassNameOrFunction<PopoverRenderProps>
  trigger?: string; triggerRef?: RefObject<Element>
  isEntering?: boolean; isExiting?: boolean; offset?: number
}
declare const Popover: (props: PopoverProps & RefAttributes<HTMLElement>) => ReactElement | null
declare const OverlayArrow: (props: OverlayArrowProps & RefAttributes<SVGSVGElement>) => ReactElement | null
```

### Collections — ListBox, GridList, Table

```ts
// ListBox
interface ListBoxProps<T> extends
  Omit<AriaListBoxProps<T>, 'children' | 'label'>, CollectionProps<T>,
  StyleRenderProps<ListBoxRenderProps>, SlotProps {
  className?: ClassNameOrFunction<ListBoxRenderProps>
  selectionBehavior?: SelectionBehavior
  dragAndDropHooks?: DragAndDropHooks<NoInfer<T>>
  renderEmptyState?: (props: ListBoxRenderProps) => ReactNode
  layout?: 'stack' | 'grid'
  orientation?: Orientation
}
declare const ListBox: <T>(props: ListBoxProps<T> & RefAttributes<HTMLDivElement>) => ReactElement | null

interface ListBoxItemProps<T = object> extends
  PossibleLinkDOMRenderProps<'div', ListBoxItemRenderProps>, LinkDOMProps,
  HoverEvents, PressEvents, KeyboardEvents {
  className?: ClassNameOrFunction<ListBoxItemRenderProps>
  id?: Key; value?: T; textValue?: string
  isDisabled?: boolean; onAction?: () => void
}
declare const ListBoxItem: <T>(props: ListBoxItemProps<T> & RefAttributes<HTMLDivElement>) => ReactElement | null

interface ListBoxSectionProps<T> extends SectionProps<T> { className?: string }
declare const ListBoxSection: <T>(props: ListBoxSectionProps<T> & RefAttributes<HTMLElement>) => ReactElement | null

// GridList
interface GridListProps<T> extends
  Omit<AriaGridListProps<T>, 'children'>, CollectionProps<T>,
  StyleRenderProps<GridListRenderProps>, SlotProps {
  className?: ClassNameOrFunction<GridListRenderProps>
  selectionBehavior?: SelectionBehavior
  dragAndDropHooks?: DragAndDropHooks<NoInfer<T>>
  renderEmptyState?: (props: GridListRenderProps) => ReactNode
  layout?: 'stack' | 'grid'; orientation?: Orientation
  disallowTypeAhead?: boolean
}
declare const GridList: <T>(props: GridListProps<T> & RefAttributes<HTMLDivElement>) => ReactElement | null

interface GridListItemProps<T = object> extends
  RenderProps<GridListItemRenderProps>, LinkDOMProps, HoverEvents, PressEvents {
  className?: ClassNameOrFunction<GridListItemRenderProps>
  id?: Key; value?: T; textValue?: string
  isDisabled?: boolean; onAction?: () => void
}
declare const GridListItem: <T>(props: GridListItemProps<T> & RefAttributes<HTMLDivElement>) => ReactElement | null

// Table
interface TableProps extends
  Omit<SharedTableProps<any>, 'children'>, StyleRenderProps<TableRenderProps, 'table' | 'div'>,
  SlotProps, AriaLabelingProps {
  className?: ClassNameOrFunction<TableRenderProps>
  children?: ReactNode
  selectionBehavior?: SelectionBehavior; disabledBehavior?: DisabledBehavior
  onRowAction?: (key: Key) => void
  dragAndDropHooks?: DragAndDropHooks
}
declare const Table: ForwardRefExoticComponent<TableProps & RefAttributes<HTMLDivElement | HTMLTableElement>>

interface ColumnProps extends RenderProps<ColumnRenderProps, 'th' | 'div'> {
  id?: Key; allowsSorting?: boolean; isRowHeader?: boolean; textValue?: string
  width?: ColumnSize | null; defaultWidth?: ColumnSize | null
  minWidth?: ColumnStaticSize | null; maxWidth?: ColumnStaticSize | null
}
declare const Column: (props: ColumnProps & RefAttributes<HTMLDivElement | HTMLTableCellElement>) => ReactElement | null

interface RowProps<T> extends StyleRenderProps<RowRenderProps, 'tr' | 'div'>,
  LinkDOMProps, HoverEvents, PressEvents {
  columns?: Iterable<T>; children?: ReactNode | ((item: T) => ReactElement)
  value?: T; textValue?: string; isDisabled?: boolean
  id?: Key; hasChildItems?: boolean; onAction?: () => void
}
declare const Row: <T>(props: RowProps<T> & RefAttributes<HTMLDivElement | HTMLTableRowElement>) => ReactElement | null

interface CellProps extends RenderProps<CellRenderProps, 'td' | 'div'> {
  id?: Key; textValue?: string; colSpan?: number
}
declare const Cell: (props: CellProps & RefAttributes<HTMLDivElement | HTMLTableCellElement>) => ReactElement | null

interface TableHeaderProps<T> extends StyleRenderProps<TableHeaderRenderProps, 'thead' | 'div'>,
  HoverEvents {
  columns?: Iterable<T>; children?: ReactNode | ((item: T) => ReactElement)
}
declare const TableHeader: <T>(props: TableHeaderProps<T> & RefAttributes<...>) => ReactElement | null

interface TableBodyProps<T> extends Omit<CollectionProps<T>, 'disabledKeys'>,
  StyleRenderProps<TableBodyRenderProps, 'tbody' | 'div'> {
  renderEmptyState?: (props: TableBodyRenderProps) => ReactNode
}
declare const TableBody: <T>(props: TableBodyProps<T> & RefAttributes<...>) => ReactElement | null
```

### Navigation and layout

```ts
// Tabs
interface TabsProps extends AriaTabListProps, RenderProps<TabsRenderProps> {
  className?: ClassNameOrFunction<TabsRenderProps>
}
declare const Tabs: (props: TabsProps & RefAttributes<HTMLDivElement>) => ReactElement | null

interface TabListProps<T> extends StyleRenderProps<TabListRenderProps>, CollectionProps<T> {
  className?: ClassNameOrFunction<TabListRenderProps>
}
declare const TabList: <T>(props: TabListProps<T> & RefAttributes<HTMLDivElement>) => ReactElement | null

interface TabProps extends HoverEvents, FocusEvents, PressEvents {
  className?: ClassNameOrFunction<TabRenderProps>
  id?: Key; isDisabled?: boolean
}
declare const Tab: (props: TabProps & RefAttributes<HTMLDivElement>) => ReactElement | null

interface TabPanelProps extends AriaTabPanelProps, RenderProps<TabPanelRenderProps> {
  className?: ClassNameOrFunction<TabPanelRenderProps>
  shouldForceMount?: boolean
}
declare const TabPanel: (props: TabPanelProps & RefAttributes<HTMLDivElement>) => ReactElement | null
// TabPanels — container for TabPanel collection (same generic shape as TabList)
declare const TabPanels: <T>(props: TabPanelsProps<T> & RefAttributes<HTMLDivElement>) => ReactElement | null

// Menu
declare const Menu: <T>(props: MenuProps<T> & RefAttributes<HTMLElement>) => ReactElement | null
declare const MenuTrigger: (props: MenuTriggerProps & { children: ReactNode }) => JSX.Element
declare const MenuItem: <T>(props: MenuItemProps<T> & RefAttributes<HTMLElement>) => ReactElement | null
declare const MenuSection: <T>(props: MenuSectionProps<T> & RefAttributes<HTMLElement>) => ReactElement | null
declare const SubmenuTrigger: (props: SubmenuTriggerProps) => JSX.Element

// Breadcrumbs
declare const Breadcrumbs: <T>(props: BreadcrumbsProps<T> & RefAttributes<HTMLElement>) => ReactElement | null
declare const Breadcrumb: (props: BreadcrumbProps & RefAttributes<HTMLElement>) => ReactElement | null
```

### Disclosure, toolbar, tooltip

```ts
// Disclosure / DisclosureGroup
declare const Disclosure: (props: DisclosureProps & RefAttributes<HTMLDivElement>) => ReactElement | null
declare const DisclosurePanel: (props: DisclosurePanelProps & RefAttributes<HTMLElement>) => ReactElement | null
declare const DisclosureGroup: (props: DisclosureGroupProps & RefAttributes<HTMLDivElement>) => ReactElement | null

// Toolbar
declare const Toolbar: (props: ToolbarProps & RefAttributes<HTMLDivElement>) => ReactElement | null

// Tooltip
declare const Tooltip: (props: TooltipProps & RefAttributes<HTMLElement>) => ReactElement | null
declare const TooltipTrigger: (props: TooltipTriggerComponentProps) => JSX.Element
```

### Progress and feedback

```ts
// ProgressBar
declare const ProgressBar: (props: ProgressBarProps & RefAttributes<HTMLDivElement>) => ReactElement | null

// Meter
declare const Meter: (props: MeterProps & RefAttributes<HTMLDivElement>) => ReactElement | null

// Link
declare const Link: (props: LinkProps & RefAttributes<HTMLAnchorElement>) => ReactElement | null
```

### Tree (hierarchical list)

```ts
declare const Tree: <T>(props: TreeProps<T> & RefAttributes<HTMLDivElement>) => ReactElement | null
declare const TreeItem: <T>(props: TreeItemProps<T> & RefAttributes<HTMLDivElement>) => ReactElement | null
declare const TreeItemContent: (props: TreeItemContentProps & RefAttributes<HTMLDivElement>) => ReactElement | null
```

### Tag group

```ts
declare const TagGroup: <T>(props: TagGroupProps<T> & RefAttributes<HTMLDivElement>) => ReactElement | null
declare const TagList: <T>(props: TagListProps<T> & RefAttributes<HTMLDivElement>) => ReactElement | null
declare const Tag: (props: TagProps & RefAttributes<HTMLDivElement>) => ReactElement | null
```

### File interaction

```ts
declare const FileTrigger: (props: FileTriggerProps & RefAttributes<HTMLInputElement>) => ReactElement | null
declare const DropZone: (props: DropZoneProps & RefAttributes<HTMLDivElement>) => ReactElement | null
```

### Data management hooks

```ts
// useListData — local list state with CRUD
function useListData<T>(options: ListDataOptions<T>): ListData<T>
// ListData exposes: items, selectedKeys, getItem, insert, append, prepend, remove,
//   move, moveAfter, moveBefore, update, setSelectedKeys

// useTreeData — hierarchical variant
function useTreeData<T>(options: TreeDataOptions<T>): TreeData<T>

// useAsyncList — server-side sorting/filtering/loading
function useAsyncList<T>(options: AsyncListOptions<T>): AsyncListData<T>
// AsyncListData: items, loadingState, loadMore, sort, reload, error, selectedKeys, setSelectedKeys

// Drag and drop
function useDragAndDrop<T extends object>(options: DragAndDropOptions<T>): DragAndDropHooks<T>
```

### Internationalisation and context

```ts
function useLocale(): { locale: string; direction: 'ltr' | 'rtl' }
function isRTL(locale: string): boolean
function useFilter(options?: Intl.CollatorOptions): Filter
// Filter: contains, startsWith, endsWith — (string, string) => boolean

// Provider — compose multiple context values
declare const Provider: (props: { values: [...]; children: ReactNode }) => JSX.Element
declare const I18nProvider: (props: I18nProviderProps) => JSX.Element
// RouterProvider — links react-router to RAC Link / useLinkProps
declare const RouterProvider: (props: RouterConfig) => JSX.Element

// compose render props helper
function composeRenderProps<T>(
  value: T | ((state: S) => T),
  wrap: (prev: T, state: S) => T
): (state: S) => T

// useContextProps — slot-based prop forwarding
function useContextProps<T, S extends SlotProps, R>(
  props: T & S, ref: RefObject<R>, context: Context<ContextValue<S, R>>
): [T, RefObject<R>]
```

### Key shared types

```ts
type Key = string | number
type Selection = 'all' | Set<Key>
type SelectionMode = 'none' | 'single' | 'multiple'
type SelectionBehavior = 'toggle' | 'replace'
type Orientation = 'horizontal' | 'vertical'
type SortDirection = 'ascending' | 'descending'
interface SortDescriptor { column?: Key; direction?: SortDirection }

type Placement =
  | 'bottom' | 'bottom left' | 'bottom right' | 'bottom start' | 'bottom end'
  | 'top' | 'top left' | 'top right' | 'top start' | 'top end'
  | 'left' | 'left top' | 'left bottom' | 'start' | 'start top' | 'start bottom'
  | 'right' | 'right top' | 'right bottom' | 'end' | 'end top' | 'end bottom'

interface ValidationResult { isInvalid: boolean; validationErrors: string[] }
type ContextValue<T, E extends Element> = T & { ref?: RefObject<E> } | null
```

---

## [3] — @radix-ui/react-* (unstyled primitives)

**Installed packages**: `react-slot` 1.2.5, `react-label` 2.1.9, `react-separator` 1.1.9
**Catalog-admitted**: `react-visually-hidden` 2.5 (not yet installed)

### @radix-ui/react-slot

Enables the "slot" pattern — a child component merges its props and ref onto the single child element, allowing component composition without introducing extra DOM nodes.

```ts
// @radix-ui/react-slot
interface SlotProps extends HTMLAttributes<HTMLElement> { children?: ReactNode }
declare function createSlot(ownerName: string): ForwardRefExoticComponent<SlotProps & RefAttributes<HTMLElement>>
declare const Slot: ForwardRefExoticComponent<SlotProps & RefAttributes<HTMLElement>>

// Slottable — marks a child as the element onto which Slot merges props
type SlottableChildrenProps = { children: ReactNode }
type SlottableRenderFnProps = { child: ReactNode; children: (slottable: ReactNode) => ReactNode }
type SlottableProps = SlottableRenderFnProps | SlottableChildrenProps
interface SlottableComponent extends FC<SlottableProps> { __radixId: symbol }
declare function createSlottable(ownerName: string): SlottableComponent
declare const Slottable: SlottableComponent

// named export alias
export { Slot as Root }
```

### @radix-ui/react-label

Accessible `<label>` with native-input association and pointer-event normalization.

```ts
// @radix-ui/react-label
type PrimitiveLabelProps = ComponentPropsWithoutRef<typeof Primitive.label>
interface LabelProps extends PrimitiveLabelProps {}
declare const Label: ForwardRefExoticComponent<LabelProps & RefAttributes<HTMLLabelElement>>
export { Label, Root }  // Root === Label
```

### @radix-ui/react-separator

Accessible horizontal/vertical visual or semantic separator.

```ts
// @radix-ui/react-separator
type Orientation = 'horizontal' | 'vertical'
interface SeparatorProps extends ComponentPropsWithoutRef<typeof Primitive.div> {
  orientation?: Orientation   // default 'horizontal'
  decorative?: boolean        // removes from accessibility tree when true
}
declare const Separator: ForwardRefExoticComponent<SeparatorProps & RefAttributes<HTMLDivElement>>
export { Separator, Root }  // Root === Separator
```

### @radix-ui/react-visually-hidden (catalog-admitted, not installed)

Renders children invisible-to-sighted-users but accessible to screen readers.

```ts
// @radix-ui/react-visually-hidden
// Canonical form from Radix docs; exact d.ts resolution deferred to stage-[2.C] install
interface VisuallyHiddenProps extends HTMLAttributes<HTMLSpanElement> {}
declare const VisuallyHidden: ForwardRefExoticComponent<VisuallyHiddenProps & RefAttributes<HTMLSpanElement>>
```

---

## [4] — @tanstack/react-table

**Entry**: `@tanstack/react-table`
**Version**: 8.21.3 — re-exports all of `@tanstack/table-core`

The headless table machinery. Drives the `Table` / `GridList` virtualized surfaces in the
view-surfaces ATOM_BINDING_AND_PRIMITIVES cluster.

### Core hook

```ts
// @tanstack/react-table
function useReactTable<TData extends RowData>(options: TableOptions<TData>): Table<TData>

type Renderable<TProps> = ReactNode | ComponentType<TProps>
function flexRender<TProps extends object>(
  Comp: Renderable<TProps>, props: TProps
): ReactNode | JSX.Element
```

### Key types (from @tanstack/table-core)

```ts
// RowData — constraint for the data row type
type RowData = unknown | object | any[]

// ColumnDef — the column definition
type ColumnDef<TData extends RowData, TValue = unknown> =
  | DisplayColumnDef<TData, TValue>
  | GroupColumnDef<TData, TValue>
  | AccessorKeyColumnDef<TData, TValue>
  | AccessorFnColumnDef<TData, TValue>

interface ColumnDefBase<TData extends RowData, TValue = unknown> {
  columns?: ColumnDef<TData, any>[]
  header?: ColumnDefTemplate<HeaderContext<TData, TValue>>
  footer?: ColumnDefTemplate<HeaderContext<TData, TValue>>
  cell?: ColumnDefTemplate<CellContext<TData, TValue>>
  meta?: ColumnMeta<TData, TValue>
  id?: string
  enableSorting?: boolean
  enableColumnFilter?: boolean
  enableGlobalFilter?: boolean
  enableGrouping?: boolean
  enablePinning?: boolean
  enableHiding?: boolean
  enableResizing?: boolean
  size?: number; minSize?: number; maxSize?: number
}

// TableOptions — passed to useReactTable
interface TableOptions<TData extends RowData> extends
  TableOptionsResolved<TData>,
  Partial<TableFeatures<...>>,
  Partial<ExtractFeatureDefs<...>> {}

// critical required options:
interface TableOptionsResolved<TData extends RowData> {
  data: TData[]
  columns: ColumnDef<TData, any>[]
  getCoreRowModel: (table: Table<TData>) => () => RowModel<TData>
  state?: Partial<TableState>
  onStateChange?: OnChangeFn<TableState>
  renderFallbackValue?: unknown
  getSubRows?: (originalRow: TData, index: number) => TData[] | undefined
  getRowId?: (originalRow: TData, index: number, parent?: Row<TData>) => string
  debugAll?: boolean
}

// Table — the object returned by useReactTable
interface Table<TData extends RowData> {
  // row model accessors
  getHeaderGroups(): HeaderGroup<TData>[]
  getFooterGroups(): HeaderGroup<TData>[]
  getRowModel(): RowModel<TData>
  getPrePaginationRowModel(): RowModel<TData>
  // state
  getState(): TableState
  setState(updater: Updater<TableState>): void
  setOptions(updater: Updater<TableOptionsResolved<TData>>): void
  // column management
  getAllColumns(): Column<TData>[]
  getColumn(columnId: string): Column<TData> | undefined
  // pagination
  setPageIndex(index: number): void
  nextPage(): void; previousPage(): void
  getPageCount(): number; getCanNextPage(): boolean; getCanPreviousPage(): boolean
  // sorting
  setSorting(updater: Updater<SortingState>): void
  // filtering
  setGlobalFilter(value: unknown): void
  setColumnFilters(updater: Updater<ColumnFiltersState>): void
}

// Row and Cell
interface Row<TData extends RowData> {
  id: string; index: number; depth: number
  original: TData
  getValue<TValue>(columnId: string): TValue
  renderValue<TValue>(columnId: string): TValue
  getVisibleCells(): Cell<TData, unknown>[]
  getIsSelected(): boolean; getIsExpanded(): boolean
  subRows: Row<TData>[]
}

interface Cell<TData extends RowData, TValue = unknown> {
  id: string; column: Column<TData, TValue>; row: Row<TData>
  getValue(): TValue; renderValue(): TValue
  getContext(): CellContext<TData, TValue>
}

interface Header<TData extends RowData, TValue = unknown> {
  id: string; index: number; depth: number
  column: Column<TData, TValue>
  colSpan: number; rowSpan: number
  isPlaceholder: boolean
  getContext(): HeaderContext<TData, TValue>
}

// TableState — all feature states merged
interface TableState {
  sorting: SortingState
  columnFilters: ColumnFiltersState
  globalFilter: unknown
  grouping: GroupingState
  columnPinning: ColumnPinningState
  columnVisibility: VisibilityState
  columnSizing: ColumnSizingState
  expanded: ExpandedState
  pagination: PaginationState
  rowSelection: RowSelectionState
}

// Sort / filter state
type SortingState = ColumnSort[]
interface ColumnSort { id: string; desc: boolean }
type ColumnFiltersState = ColumnFilter[]
interface ColumnFilter { id: string; value: unknown }

// Row model factory — required option
// import { getCoreRowModel } from '@tanstack/react-table'
function getCoreRowModel<TData extends RowData>(): (table: Table<TData>) => () => RowModel<TData>
function getSortedRowModel<TData extends RowData>(): (table: Table<TData>) => () => RowModel<TData>
function getFilteredRowModel<TData extends RowData>(): (table: Table<TData>) => () => RowModel<TData>
function getPaginationRowModel<TData extends RowData>(): (table: Table<TData>) => () => RowModel<TData>
function getGroupedRowModel<TData extends RowData>(): (table: Table<TData>) => () => RowModel<TData>
function getExpandedRowModel<TData extends RowData>(): (table: Table<TData>) => () => RowModel<TData>
```

---

## [5] — @tanstack/react-virtual

**Entry**: `@tanstack/react-virtual`
**Version**: 3.14.2 — re-exports all of `@tanstack/virtual-core`

Drives the ATOM_BINDING_AND_PRIMITIVES virtualization machinery — oversized receipt timelines,
large table bodies, geo-series lists.

```ts
// @tanstack/react-virtual
function useVirtualizer<TScrollElement extends Element, TItemElement extends Element>(
  options: PartialKeys<
    ReactVirtualizerOptions<TScrollElement, TItemElement>,
    'observeElementRect' | 'observeElementOffset' | 'scrollToFn'
  >
): ReactVirtualizer<TScrollElement, TItemElement>

function useWindowVirtualizer<TItemElement extends Element>(
  options: PartialKeys<
    ReactVirtualizerOptions<Window, TItemElement>,
    'getScrollElement' | 'observeElementRect' | 'observeElementOffset' | 'scrollToFn'
  >
): ReactVirtualizer<Window, TItemElement>

// ReactVirtualizerOptions — extends VirtualizerOptions
type ReactVirtualizerOptions<
  TScrollElement extends Element | Window,
  TItemElement extends Element
> = VirtualizerOptions<TScrollElement, TItemElement> & {
  useFlushSync?: boolean
  directDomUpdates?: boolean
  directDomUpdatesMode?: 'position' | 'transform'
}

// ReactVirtualizer — extends Virtualizer with containerRef for directDomUpdates
type ReactVirtualizer<
  TScrollElement extends Element | Window,
  TItemElement extends Element
> = Virtualizer<TScrollElement, TItemElement> & {
  containerRef: (node: HTMLElement | null) => void
}

// Key types from @tanstack/virtual-core
interface VirtualizerOptions<TScrollElement extends Element | Window, TItemElement extends Element> {
  count: number
  getScrollElement: () => TScrollElement | null
  estimateSize: (index: number) => number
  overscan?: number
  horizontal?: boolean
  paddingStart?: number; paddingEnd?: number; paddingLane?: number
  gap?: number
  initialOffset?: number | (() => number)
  lanes?: number                              // multi-lane (grid / masonry)
  rangeExtractor?: (range: Range) => number[]
  scrollMargin?: number
  indexAttribute?: string
  initialMeasurementsCache?: VirtualItem[]
  getItemKey?: (index: number) => Key
  onChange?: (instance: Virtualizer<TScrollElement, TItemElement>, sync: boolean) => void
  // measurement observers — provided automatically by useVirtualizer
  observeElementRect: (
    instance: Virtualizer<TScrollElement, TItemElement>,
    cb: (rect: Rect) => void
  ) => void | (() => void)
  observeElementOffset: (
    instance: Virtualizer<TScrollElement, TItemElement>,
    cb: (offset: number, isScrolling: boolean) => void
  ) => void | (() => void)
  scrollToFn: (
    offset: number,
    options: { adjustments?: number; behavior?: ScrollBehavior },
    instance: Virtualizer<TScrollElement, TItemElement>
  ) => void
}

// Virtualizer — the object returned by the hooks
interface Virtualizer<TScrollElement extends Element | Window, TItemElement extends Element> {
  // primary render surface
  getVirtualItems(): VirtualItem[]
  getTotalSize(): number
  // scroll control
  scrollToIndex(index: number, options?: ScrollToIndexOptions): void
  scrollToOffset(offset: number, options?: ScrollToOffsetOptions): void
  // state
  isScrolling: boolean
  range: { startIndex: number; endIndex: number } | null
  scrollOffset: number
  // measurement
  measureElement(el: TItemElement | null | undefined): void
  resizeItem(index: number, size: number): void
}

interface VirtualItem {
  index: number; key: Key; lane: number
  start: number; end: number; size: number
}

interface ScrollToIndexOptions {
  align?: 'start' | 'center' | 'end' | 'auto'
  behavior?: ScrollBehavior
}
```

---

## [6] — vite (build/dev server config surface)

**Entry**: `vite` (node)
**Version**: 8.0.16 — rolldown-backed, OxC default minifier, baseline-widely-available default target

### defineConfig

```ts
// vite
function defineConfig(config: UserConfig): UserConfig
function defineConfig(config: Promise<UserConfig>): Promise<UserConfig>
function defineConfig(config: UserConfigFn): UserConfigFn
function defineConfig(config: UserConfigExport): UserConfigExport
```

### UserConfig

```ts
interface UserConfig extends DefaultEnvironmentOptions {
  root?: string
  base?: string
  publicDir?: string | false
  cacheDir?: string
  mode?: string
  plugins?: PluginOption[]
  html?: HTMLOptions
  css?: CSSOptions
  json?: JsonOptions
  oxc?: OxcOptions | false
  esbuild?: ESBuildOptions | false    // deprecated, prefer oxc
  assetsInclude?: string | RegExp | (string | RegExp)[]
  server?: ServerOptions
  preview?: PreviewOptions
  experimental?: ExperimentalOptions
  logLevel?: LogLevel
  clearScreen?: boolean
  envDir?: string | false
  build?: BuildOptions
}
```

### BuildOptions (= BuildEnvironmentOptions)

```ts
type BuildOptions = BuildEnvironmentOptions

interface BuildEnvironmentOptions {
  target?: 'baseline-widely-available' | EsbuildTarget | false  // default 'baseline-widely-available'
  outDir?: string                    // default 'dist'
  assetsDir?: string                 // default 'assets'
  assetsInlineLimit?: number | ((filePath: string, content: Buffer) => boolean | undefined)
  cssCodeSplit?: boolean
  sourcemap?: boolean | 'inline' | 'hidden'
  minify?: boolean | 'oxc' | 'terser' | 'esbuild'  // default 'oxc'
  terserOptions?: TerserOptions
  rolldownOptions?: RolldownOptions
  write?: boolean
  emptyOutDir?: boolean | null
  copyPublicDir?: boolean
  manifest?: boolean | string
  lib?: LibraryOptions | false
  modulePreload?: boolean | ModulePreloadOptions
  ssr?: boolean | string
  rollupOptions?: RolldownOptions    // deprecated alias
}
```

### ServerOptions

```ts
interface ServerOptions extends CommonServerOptions {
  hmr?: HmrOptions | boolean
  warmup?: { clientFiles?: string[]; ssrFiles?: string[] }
  watch?: WatchOptions | null
  middlewareMode?: boolean | { server: HttpServer }
}
```

### Plugin surface

```ts
type PluginOption =
  | Thenable<Plugin | { name: string } | false | null | undefined | PluginOption[]>

interface Plugin<A = any> extends Rolldown.Plugin<A> {
  enforce?: 'pre' | 'post'
  apply?: 'build' | 'serve' | ((config: UserConfig, env: ConfigEnv) => boolean)
  // lifecycle hooks (from Rolldown.Plugin):
  //   buildStart, resolveId, load, transform, buildEnd, closeBundle
  //   configureServer, transformIndexHtml, handleHotUpdate
}
```

### @vitejs/plugin-react (6.0.2)

React Compiler + Fast Refresh. Returns `Plugin[]`.

```ts
// @vitejs/plugin-react
interface Options {
  include?: string | RegExp | Array<string | RegExp>
  exclude?: string | RegExp | Array<string | RegExp>
  jsxImportSource?: string    // default 'react'
  jsxRuntime?: 'classic' | 'automatic'   // default 'automatic'
  reactRefreshHost?: string
}
declare function viteReact(opts?: Options): Plugin[]
declare namespace viteReact { var preambleCode: string }
export default viteReact
```

### vite-plugin-pwa (1.3.0)

Offline PWA service worker generation and manifest injection. Returns `Plugin[]`.

```ts
// vite-plugin-pwa
interface VitePWAOptions {
  mode?: 'development' | 'production'
  srcDir?: string               // default 'public'
  outDir?: string               // default 'dist'
  filename?: string             // default 'sw.js'
  manifestFilename?: string     // default 'manifest.webmanifest'
  strategies?: 'generateSW' | 'injectManifest'  // default 'generateSW'
  scope?: string                // default same as vite base
  registerType?: 'prompt' | 'autoUpdate' | 'inline' | 'script' | 'script-defer' | false
  manifest?: Partial<ManifestOptions> | false
  workbox?: GenerateSWOptions   // workbox-build GenerateSW options
  injectManifest?: InjectManifestOptions
  devOptions?: { enabled?: boolean; type?: 'classic' | 'module'; navigateFallback?: string }
  includeAssets?: string | string[]
  base?: string
}
declare function VitePWA(userOptions?: Partial<VitePWAOptions>): Plugin[]
```

---

## [7] — Admissions scope note

`@tanstack/react-query` and `@tanstack/react-router` appear in the task prompt but are **absent from the workspace catalog** (`pnpm-workspace.yaml`) and unreferenced in the planning corpus. The TS branch uses Effect atoms (`@effect-atom/atom`, `@effect-atom/atom-react`) for all store subscriptions and no router library is admitted. Their catalogue pages are deferred to the stage-[2.C] pass that admits and installs them, at which point the extraction probe resolves against installed `node_modules`.
