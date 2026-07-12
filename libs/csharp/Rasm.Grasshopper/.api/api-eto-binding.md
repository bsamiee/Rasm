# [RASM_GRASSHOPPER_API_ETO_BINDING]

`Eto.Forms` binding is the host's two-way data rail, and this catalog fixes the verified binding surface a GH2-hosted panel composes. `IndirectBinding<T>` reads and writes a value against an arbitrary object, `DirectBinding<T>` binds a fixed source, and `BindableBinding<T,TValue>` fuses a control property to a `DataContext` model through the `IBindable` seam. `DualBindingMode` selects the two-way direction, `Convert`/`Cast`/`Child`/`CatchException` reshape and guard the projection, and the `IDataStore<T>`/`ITreeGridStore<T>` carriers back the grid, list, and tree views. `DataContext` propagation walks the control tree, so one context assignment feeds every bound descendant.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `Eto`

- package: `Eto` (the cross-platform Eto.Forms UI framework, host-provided by RhinoWIP)
- license: BSD-3-Clause
- assembly: `Eto` (`Eto.dll`)
- namespace: `Eto.Forms`
- asset: host-provided — RhinoWIP ships `Eto.dll` under `RhCore.framework/Versions/A/Resources`; no NuGet admission
- rail: native UI

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: binding carriers and the bindable seam

- rail: native UI

`IBindable` owns `DataContext`, `DataContextChanged`, and its live `BindingCollection`; propagation walks this seam across descendants, and the collection updates on `DataContext` changes. `DirectBinding<T>` fixes the control source, while `IndirectBinding<T>` and `IIndirectBinding<T>` read and write a supplied model object. `BindableBinding<T,TValue>` fuses an `IBindable` owner property to a model value, and binding construction joins the two halves into a live `DualBinding<T>` that relays source and destination updates.

`DelegateBinding<T>` and `DelegateBinding<TObject,TValue>` pair getter and setter delegates with change-event registration. `PropertyBinding<T>` resolves a property name, such as `new PropertyBinding<Image>("Image")`. `IndirectChildBinding<TObject,TValue>` is internal; public child drilling enters through `IndirectBinding<T>.Child(IndirectBinding<TNew>)`.

| [INDEX] | [SYMBOL]                               | [KIND]           | [CAPABILITY]                    |
| :-----: | :------------------------------------- | :--------------- | :------------------------------ |
|  [01]   | `IBindable`                            | contract         | binding host                    |
|  [02]   | `IBinding`                             | contract         | binding identity                |
|  [03]   | `Binding`                              | abstract base    | binding base                    |
|  [04]   | `DirectBinding<T>`                     | binding          | control-side source             |
|  [05]   | `IndirectBinding<T>`                   | binding          | model-side accessor             |
|  [06]   | `IIndirectBinding<T>`                  | contract         | model-accessor contract         |
|  [07]   | `BindableBinding<T,TValue>`            | binding          | control-model fusion            |
|  [08]   | `DualBinding<T>`                       | binding          | update relay                    |
|  [09]   | `PropertyBinding<T>`                   | binding          | named-property accessor         |
|  [10]   | `DelegateBinding<T>`                   | binding          | delegate accessor               |
|  [11]   | `DelegateBinding<TObject,TValue>`      | binding          | typed delegate accessor         |
|  [12]   | `ObjectBinding<T>`                     | binding          | bound-object value              |
|  [13]   | `ObjectBinding<TObject,TValue>`        | binding          | typed bound-object value        |
|  [14]   | `ControlBinding<T,TValue>`             | binding          | control-property specialization |
|  [15]   | `IndirectChildBinding<TObject,TValue>` | internal binding | child projection kernel         |
|  [16]   | `BindingCollection`                    | collection       | live binding set                |

[PUBLIC_TYPE_SCOPE]: binding modes and update control

- rail: native UI

`DualBindingMode` admits `OneWay`, `OneWayToSource`, `TwoWay`, and `Manual`. `BindingUpdateMode` selects source-to-destination or destination-to-source for an explicit `Update` push. `BindableExtensions` owns `Bind` and `BindDataContext`; `BindingExtensions` owns `Convert`, `Cast`, and `Child`, while `BindingExtensionsNonGeneric` owns non-generic helpers.

| [INDEX] | [SYMBOL]                      | [KIND]    | [CAPABILITY]              |
| :-----: | :---------------------------- | :-------- | :------------------------ |
|  [01]   | `DualBindingMode`             | enum      | two-way direction         |
|  [02]   | `BindingUpdateMode`           | enum      | explicit update direction |
|  [03]   | `BindableExtensions`          | extension | fluent context binding    |
|  [04]   | `BindingExtensions`           | extension | binding projections       |
|  [05]   | `BindingExtensionsNonGeneric` | extension | non-generic helpers       |

[PUBLIC_TYPE_SCOPE]: data-store collection binding

- rail: native UI

`IDataStore<T>` exposes `Count` and `this[int]`; views consume it through the `IList<T>` adapter `DataStoreVirtualCollection<T>`, never directly. `DataStoreVirtualCollection<T>` constructs from `(IDataStore<T> store)`, and `DataStoreExtensions` adds binding helpers over the store contract. `DataStoreCollection<T>` extends `ExtendedObservableCollection<T>`, and its mutations refresh the bound view. `ITreeGridStore<out T> : IDataStore<T> where T : ITreeGridItem` backs `TreeGridView.DataStore`.

| [INDEX] | [SYMBOL]                        | [KIND]     | [CAPABILITY]           |
| :-----: | :------------------------------ | :--------- | :--------------------- |
|  [01]   | `IDataStore<T>`                 | contract   | random-access window   |
|  [02]   | `DataStoreCollection<T>`        | collection | observable item source |
|  [03]   | `DataStoreVirtualCollection<T>` | collection | virtual window adapter |
|  [04]   | `DataStoreExtensions`           | extension  | binding helpers        |
|  [05]   | `ITreeStore`                    | contract   | tree item source       |
|  [06]   | `ITreeGridStore<T>`             | contract   | tree-grid item source  |

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: binding construction

- rail: native UI

`PropertyBinding<T>` constructs from `(string property)`, while `BindableBinding<T,TValue>` constructs from `(T owner, Func<T,TValue> get, Action<T,TValue> set, addChange, removeChange)`. `IBindable.DataContext` is get/set; assignment propagates to bound descendants and fires `DataContextChanged`.

| [INDEX] | [SURFACE]                         | [CALL_SHAPE] | [CAPABILITY]           |
| :-----: | :-------------------------------- | :----------- | :--------------------- |
|  [01]   | `new PropertyBinding<T>`          | constructor  | named-property binding |
|  [02]   | `new BindableBinding<T,TValue>`   | constructor  | change-aware binding   |
|  [03]   | `TextBox.TextBinding`             | get          | text binding           |
|  [04]   | `CheckBox.CheckedBinding`         | get          | checked-state binding  |
|  [05]   | `NumericStepper.ValueBinding`     | get          | numeric-value binding  |
|  [06]   | `Grid.SelectedItemBinding`        | get          | grid-selection binding |
|  [07]   | `TabControl.SelectedIndexBinding` | get          | tab-selection binding  |
|  [08]   | `IBindable.DataContext`           | get/set      | model assignment       |

[ENTRYPOINT_SCOPE]: two-way and data-context binding

- rail: native UI

`BindableExtensions.Bind<T>` takes `(IBindable, IndirectBinding<T>, DirectBinding<T>, DualBindingMode)` and returns `DualBinding<T>`. `BindableExtensions.Bind<TValue>` takes an `IBindable`, an `IndirectBinding<TValue>`, and an explicit `object dataContext`. Its model projection is `IndirectBinding<TValue> dcBinding`; its policy inputs are `DualBindingMode`, `TValue defaultControl`, and `TValue defaultContext`. It returns `DualBinding<TValue>`.

`BindableExtensions.BindDataContext<T>` takes an `IBindable`, `IndirectBinding<T>`, `IndirectBinding<T> dcBinding`, `DualBindingMode`, `T defaultControl`, and `T defaultContext`; it returns `DualBinding<T>`. `BindableBinding.BindDataContext` takes `IndirectBinding<TValue>`, `DualBindingMode`, and default values, then returns `DualBinding<TValue>`.

| [INDEX] | [SURFACE]                               | [KIND] | [CAPABILITY]                   |
| :-----: | :-------------------------------------- | :----- | :----------------------------- |
|  [01]   | `BindableExtensions.Bind<T>`            | bind   | explicit-source binding        |
|  [02]   | `BindableExtensions.Bind<TValue>`       | bind   | supplied-context binding       |
|  [03]   | `BindableExtensions.BindDataContext<T>` | bind   | ambient-context binding        |
|  [04]   | `BindableBinding.BindDataContext`       | bind   | fluent control-context binding |

[ENTRYPOINT_SCOPE]: projection and exception guarding

- rail: native UI

`BindableBinding.Convert<TNewValue>` takes `Func<TValue,TNewValue> toValue` and optional `Func<TNewValue,TValue> fromValue = null`, then returns `BindableBinding<T,TNewValue>`. `BindableBinding.Cast<TNewValue>()` returns `BindableBinding<T,TNewValue>`, and `BindableBinding.Child<TNewValue>(IndirectBinding<TNewValue>)` returns the same projection type.

`BindableBinding.CatchException<TException>` takes optional `Func<TException, bool> exceptionHandler = null` and returns `BindableBinding<T,TValue>`; the handler returns `true` for a handled conversion or access fault.

| [INDEX] | [SURFACE]                                    | [KIND]     | [CAPABILITY]              |
| :-----: | :------------------------------------------- | :--------- | :------------------------ |
|  [01]   | `BindableBinding.Convert<TNewValue>`         | projection | bidirectional transform   |
|  [02]   | `BindableBinding.Cast<TNewValue>`            | projection | reference or value cast   |
|  [03]   | `BindableBinding.Child<TNewValue>`           | projection | child-value drilling      |
|  [04]   | `BindableBinding.CatchException<TException>` | guard      | conversion-fault handling |

[ENTRYPOINT_SCOPE]: data-store collection binding

- rail: native UI

`DataStoreCollection<T>` constructs with `()` or `(IEnumerable<T>)`. `GridView.DataStore` and `ListControl.DataStore` set `IEnumerable<object>`, while `TreeGridView.DataStore` sets `ITreeGridStore<ITreeGridItem>`. `ListControl.ItemTextBinding` and `ListControl.ItemKeyBinding` get or set `IIndirectBinding<...>`.

| [INDEX] | [SURFACE]                     | [CALL_SHAPE] | [CAPABILITY]             |
| :-----: | :---------------------------- | :----------- | :----------------------- |
|  [01]   | `new DataStoreCollection<T>`  | constructor  | grid or list item source |
|  [02]   | `GridView.DataStore`          | set          | enumerable view source   |
|  [03]   | `ListControl.DataStore`       | set          | enumerable view source   |
|  [04]   | `TreeGridView.DataStore`      | set          | tree item source         |
|  [05]   | `ListControl.ItemTextBinding` | get/set      | item-text projection     |
|  [06]   | `ListControl.ItemKeyBinding`  | get/set      | item-key projection      |

## [04]-[IMPLEMENTATION_LAW]

[BINDING_TOPOLOGY]:

- a binding is two halves: an `IndirectBinding<T>` reads/writes the model value, a `DirectBinding<T>` reads/writes the control value, and `Bind`/`BindDataContext` join them into a live `DualBinding<T>`
- `BindableBinding<T,TValue>` is the fluent control-side entry: a control exposes its `*Binding`, and `Convert`/`Cast`/`Child`/`CatchException` reshape the value before it reaches the model
- `DualBindingMode` fixes direction — `TwoWay` relays both edges, `OneWay` pushes model-to-control, `OneWayToSource` pushes control-to-model, `Manual` defers to an explicit update
- `DataContext` is the ambient model: assigning it on a container walks the `Bindings` of every descendant `IBindable`, so `BindDataContext` bindings resolve without a per-control source assignment
- collection views bind through `IDataStore<T>`: `DataStoreCollection<T>` for eager sources, `DataStoreVirtualCollection<T>` for large ones, `ITreeGridStore<T>` for the tree

[STACKING]:

- `api-languageext`(`libs/csharp/.api/api-languageext.md`): a model-bound field validates in the `Convert` to-source direction through a `Validation<Error,A>` gate that exits `.ToFin()` before the model accepts; `CatchException<TException>` traps a conversion fault that the folder re-lands as an `Error` on `Fin<A>`; a `DataContext` swap is an `Eff` marshalled onto the UI thread
- `api-thinktecture-runtime-extensions`(`libs/csharp/.api/api-thinktecture-runtime-extensions.md`): the bound model field is a `[ValueObject<T>]` and `BindableBinding.Convert` maps the control primitive to and from it; a `[SmartEnum<TKey>]` case binds to a `DropDown`/`SegmentedButton` selection through `Convert`, the smart enum owning the display-label projection
- `api-eto-forms`(`libs/csharp/Rasm.Grasshopper/.api/api-eto-forms.md`): every control `*Binding` is the `BindableBinding` this rail consumes, and the grid/list/tree `DataStore` properties are the `IDataStore<T>` sinks it fills
- `api-eto-runtime`(`libs/csharp/Rasm.Grasshopper/.api/api-eto-runtime.md`): `DataContext` propagation and binding update run on the UI thread through `Application.Instance`

[LOCAL_ADMISSION]:

- binding is host-provided and composed directly — a field binds through its `*Binding` and `BindDataContext`, never a hand-rolled property-change observer beside the Eto binding
- a bound model value is a typed `[ValueObject<T>]` or `[SmartEnum<TKey>]` reached through `Convert`, never a stringly round-trip
- a collection view fills through `IDataStore<T>`, never a manual per-row control rebuild

[RAIL_LAW]:

- Package: `Eto`
- Owns: the two-way data-binding rail and data-store collection binding for GH2-hosted panels
- Accept: control-to-model property binding, `DataContext` propagation, two-way modes with conversion and exception guarding, grid/list/tree collection binding
- Reject: control construction and layout (`api-eto-forms`), runtime dispatch and timers (`api-eto-runtime`), the 2D drawing surface (`api-eto-drawing`)
