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

| [INDEX] | [SYMBOL]                                                 | [KIND]          | [CAPABILITY]                                                                                                                  |
| :-----: | :------------------------------------------------------- | :-------------- | :---------------------------------------------------------------------------------------------------------------------------- |
|  [01]   | `IBindable`                                              | contract        | binding host; `DataContext`/`DataContextChanged`, `Bindings` (`BindingCollection`) — the seam `DataContext` propagation walks |
|  [02]   | `IBinding` / `Binding`                                   | contract / base | binding identity and the abstract binding base                                                                                |
|  [03]   | `DirectBinding<T>`                                       | binding         | binds a fixed source value; the control-side half of a dual binding                                                           |
|  [04]   | `IndirectBinding<T>` / `IIndirectBinding<T>`             | binding         | reads/writes a value against a supplied object; the model-side half                                                           |
|  [05]   | `BindableBinding<T,TValue>`                              | binding         | fuses an `IBindable` owner property to a model value; `Convert`/`Cast`/`Child`/`CatchException`/`BindDataContext` projections |
|  [06]   | `DualBinding<T>`                                         | binding         | the live two-way link produced by a `Bind` call; source/destination update relay                                              |
|  [07]   | `PropertyBinding<T>`                                     | binding         | reflected property accessor by name (`new PropertyBinding<Image>("Image")`)                                                   |
|  [08]   | `DelegateBinding<T>` / `DelegateBinding<TObject,TValue>` | binding         | get/set delegates as a binding, with change-event registration                                                                |
|  [09]   | `ObjectBinding<T>` / `ObjectBinding<TObject,TValue>`     | binding         | binds a value against a bound object instance                                                                                 |
|  [10]   | `ControlBinding<T,TValue>`                               | binding         | control-property binding specialization                                                                                       |
|  [11]   | `IndirectChildBinding<TObject,TValue>`                   | binding         | internal; child drilling is reached through the public `IndirectBinding<T>.Child(IndirectBinding<TNew>)` composition          |
|  [12]   | `BindingCollection`                                      | collection      | the live bindings owned by an `IBindable`; updated on `DataContext` change                                                    |

[PUBLIC_TYPE_SCOPE]: binding modes and update control
- rail: native UI

| [INDEX] | [SYMBOL]                                            | [KIND]    | [CAPABILITY]                                                                                             |
| :-----: | :-------------------------------------------------- | :-------- | :------------------------------------------------------------------------------------------------------- |
|  [01]   | `DualBindingMode`                                   | enum      | two-way direction; `OneWay`, `OneWayToSource`, `TwoWay`, `Manual`                                        |
|  [02]   | `BindingUpdateMode`                                 | enum      | update-direction selector for an explicit `Update` push (source-to-destination or destination-to-source) |
|  [03]   | `BindableExtensions`                                | extension | `Bind`/`BindDataContext` fluent binders over `IBindable`                                                 |
|  [04]   | `BindingExtensions` / `BindingExtensionsNonGeneric` | extension | `Convert`/`Cast`/`Child` projection and non-generic binding helpers                                      |

[PUBLIC_TYPE_SCOPE]: data-store collection binding
- rail: native UI

| [INDEX] | [SYMBOL]                           | [KIND]     | [CAPABILITY]                                                                                                                            |
| :-----: | :--------------------------------- | :--------- | :-------------------------------------------------------------------------------------------------------------------------------------- |
|  [01]   | `IDataStore<T>`                    | contract   | random-access window source; `Count` + `this[int]` indexer — adapted by `DataStoreVirtualCollection<T>`, never bound to a view directly |
|  [02]   | `DataStoreCollection<T>`           | collection | observable list (`ExtendedObservableCollection<T>`); mutations refresh the bound view                                                   |
|  [03]   | `DataStoreVirtualCollection<T>`    | collection | `IList<T>` adapter over an `IDataStore<T>` window source; ctor `(IDataStore<T> store)`                                                  |
|  [04]   | `DataStoreExtensions`              | extension  | binding-side helpers over an `IDataStore<T>`                                                                                            |
|  [05]   | `ITreeStore` / `ITreeGridStore<T>` | contract   | tree item sources; `ITreeGridStore<out T> : IDataStore<T> where T : ITreeGridItem` backs `TreeGridView.DataStore`                       |

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: binding construction
- rail: native UI

| [INDEX] | [SURFACE]                       | [CALL_SHAPE]                                                                   | [CAPABILITY]                                                                                                                                   |
| :-----: | :------------------------------ | :----------------------------------------------------------------------------- | :--------------------------------------------------------------------------------------------------------------------------------------------- |
|  [01]   | `new PropertyBinding<T>`        | `(string property)`                                                            | reflected named-property binding                                                                                                               |
|  [02]   | `new BindableBinding<T,TValue>` | `(T owner, Func<T,TValue> get, Action<T,TValue> set, addChange, removeChange)` | control-property binding with change registration                                                                                              |
|  [03]   | control `*Binding` accessors    | get                                                                            | `TextBox.TextBinding`, `CheckBox.CheckedBinding`, `NumericStepper.ValueBinding`, `Grid.SelectedItemBinding`, `TabControl.SelectedIndexBinding` |
|  [04]   | `IBindable.DataContext`         | get/set                                                                        | assigns the model; propagates to bound descendants and fires `DataContextChanged`                                                              |

[ENTRYPOINT_SCOPE]: two-way and data-context binding
- rail: native UI

| [INDEX] | [SURFACE]                               | [CALL_SHAPE]                                                                                                                                                                         | [CAPABILITY]                                        |
| :-----: | :-------------------------------------- | :----------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- | :-------------------------------------------------- |
|  [01]   | `BindableExtensions.Bind<T>`            | `(IBindable, IndirectBinding<T>, DirectBinding<T>, DualBindingMode)` → `DualBinding<T>`                                                                                              | binds a control value to an explicit source         |
|  [02]   | `BindableExtensions.Bind<TValue>`       | `(IBindable, IndirectBinding<TValue>, object dataContext, IndirectBinding<TValue> dcBinding, DualBindingMode, TValue defaultControl, TValue defaultContext)` → `DualBinding<TValue>` | binds against a supplied data context with defaults |
|  [03]   | `BindableExtensions.BindDataContext<T>` | `(IBindable, IndirectBinding<T>, IndirectBinding<T> dcBinding, DualBindingMode, T defaultControl, T defaultContext)` → `DualBinding<T>`                                              | binds a control to the ambient `DataContext` model  |
|  [04]   | `BindableBinding.BindDataContext`       | `(IndirectBinding<TValue>, DualBindingMode, …)` → `DualBinding<TValue>`                                                                                                              | fluent data-context bind from a control `*Binding`  |

[ENTRYPOINT_SCOPE]: projection and exception guarding
- rail: native UI

| [INDEX] | [SURFACE]                                    | [CALL_SHAPE]                                                                                                 | [CAPABILITY]                                                        |
| :-----: | :------------------------------------------- | :----------------------------------------------------------------------------------------------------------- | :------------------------------------------------------------------ |
|  [01]   | `BindableBinding.Convert<TNewValue>`         | `(Func<TValue,TNewValue> toValue, Func<TNewValue,TValue> fromValue = null)` → `BindableBinding<T,TNewValue>` | bidirectional value transform                                       |
|  [02]   | `BindableBinding.Cast<TNewValue>`            | `()` → `BindableBinding<T,TNewValue>`                                                                        | reference/value cast projection                                     |
|  [03]   | `BindableBinding.Child<TNewValue>`           | `(IndirectBinding<TNewValue>)` → `BindableBinding<T,TNewValue>`                                              | drills into a child value of the bound model                        |
|  [04]   | `BindableBinding.CatchException<TException>` | `(Func<TException, bool> exceptionHandler = null)` → `BindableBinding<T,TValue>`                             | traps a conversion/access fault; handler returns `true` for handled |

[ENTRYPOINT_SCOPE]: data-store collection binding
- rail: native UI

| [INDEX] | [SURFACE]                                        | [CALL_SHAPE]                          | [CAPABILITY]                                          |
| :-----: | :----------------------------------------------- | :------------------------------------ | :---------------------------------------------------- |
|  [01]   | `new DataStoreCollection<T>`                     | `()` / `(IEnumerable<T>)`             | observable item source for a grid or list `DataStore` |
|  [02]   | `GridView.DataStore` / `ListControl.DataStore`   | set (`IEnumerable<object>`)           | assigns the enumerable backing a view                 |
|  [03]   | `TreeGridView.DataStore`                         | set (`ITreeGridStore<ITreeGridItem>`) | assigns the tree item source                          |
|  [04]   | `ListControl.ItemTextBinding` / `ItemKeyBinding` | get/set (`IIndirectBinding<...>`)     | projects an item's display text and key               |

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
