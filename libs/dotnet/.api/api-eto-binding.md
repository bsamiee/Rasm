# [RASM_API_ETO_BINDING]

`Eto.Forms` owns the two-way data-binding surface both host boundaries compose: a control property fused to a `DataContext` model through the `IBindable` interface, one projection chain reshaping and guarding the value, and the data-store carriers backing grid, list, and tree views. One `DataContext` assignment walks the control tree, so every bound descendant resolves against it. Branch-tier owner under the dual-home law — `Rasm.Grasshopper` and `Rasm.Rhino` both register this catalogue and add no binding carrier of their own.

## [01]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: binding carriers and the bindable interface

| [INDEX] | [SYMBOL]                          | [TYPE_FAMILY] | [CAPABILITY]                    |
| :-----: | :-------------------------------- | :------------ | :------------------------------ |
|  [01]   | `IBindable`                       | interface     | binding host                    |
|  [02]   | `IBinding`                        | interface     | binding identity                |
|  [03]   | `Binding`                         | class         | binding base                    |
|  [04]   | `DirectBinding<T>`                | class         | control-side source             |
|  [05]   | `IndirectBinding<T>`              | class         | model-side accessor             |
|  [06]   | `IIndirectBinding<T>`             | interface     | model-accessor contract         |
|  [07]   | `BindableBinding<T,TValue>`       | class         | control-model fusion            |
|  [08]   | `DualBinding<T>`                  | class         | update relay                    |
|  [09]   | `PropertyBinding<T>`              | class         | named-property accessor         |
|  [10]   | `DelegateBinding<T>`              | class         | delegate accessor               |
|  [11]   | `DelegateBinding<TObject,TValue>` | class         | typed delegate accessor         |
|  [12]   | `ObjectBinding<T>`                | class         | bound-object value              |
|  [13]   | `ObjectBinding<TObject,TValue>`   | class         | typed bound-object value        |
|  [14]   | `ControlBinding<T,TValue>`        | class         | control-property specialization |
|  [15]   | `BindingCollection`               | class         | live binding set                |

- `IndirectBinding<T>.Child<TNewValue>(IndirectBinding<TNewValue>)` and its `Expression<Func<T,TNewValue>>` overload are the public child-drilling entries over the `internal IndirectChildBinding<TParent,TChild>` kernel.

[PUBLIC_TYPE_SCOPE]: binding modes and projection helpers

| [INDEX] | [SYMBOL]                      | [TYPE_FAMILY] | [CAPABILITY]              |
| :-----: | :---------------------------- | :------------ | :------------------------ |
|  [01]   | `DualBindingMode`             | enum          | two-way direction         |
|  [02]   | `BindingUpdateMode`           | enum          | explicit update direction |
|  [03]   | `BindableExtensions`          | class         | fluent context binding    |
|  [04]   | `BindingExtensions`           | class         | binding projections       |
|  [05]   | `BindingExtensionsNonGeneric` | class         | non-generic helpers       |

- `DualBindingMode`: `OneWay` `TwoWay` `OneWayToSource` `OneTime` `Manual`
- `BindingUpdateMode`: `Source` `Destination`

[PUBLIC_TYPE_SCOPE]: data-store collection binding

| [INDEX] | [SYMBOL]                        | [TYPE_FAMILY] | [CAPABILITY]           |
| :-----: | :------------------------------ | :------------ | :--------------------- |
|  [01]   | `IDataStore<T>`                 | interface     | random-access window   |
|  [02]   | `DataStoreCollection<T>`        | class         | observable item source |
|  [03]   | `DataStoreVirtualCollection<T>` | class         | virtual window adapter |
|  [04]   | `DataStoreExtensions`           | class         | binding helpers        |
|  [05]   | `ITreeStore`                    | interface     | tree item source       |
|  [06]   | `ITreeGridStore<T>`             | interface     | tree-grid item source  |

- The binding sink is shaped by the view: `GridView.DataStore` and `ListControl.DataStore` are `IEnumerable<object>`, so a plain enumerable or a `DataStoreCollection<T>` binds directly, while `TreeGridView.DataStore` is `ITreeGridStore<ITreeGridItem>` and takes a tree store. `DataStoreVirtualCollection<T>` is the one-direction adapter over a virtualized `IDataStore<T>` source — its constructor takes `IDataStore<T>` and it presents `IList<T>` — so it serves a random-access window and is never interposed on a source the view already accepts.

## [02]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: binding construction

| [INDEX] | [SURFACE]                                                                                        | [SHAPE]  | [CAPABILITY]           |
| :-----: | :----------------------------------------------------------------------------------------------- | :------- | :--------------------- |
|  [01]   | `new PropertyBinding<T>(string)`                                                                 | ctor     | named-property binding |
|  [02]   | `new BindableBinding<T,TValue>(dataItem, getValue, setValue, addChangeEvent, removeChangeEvent)` | ctor     | change-aware binding   |
|  [03]   | `new BindableBinding<T,TValue>(T, IndirectBinding<TValue>)`                                      | ctor     | accessor-backed fusion |
|  [04]   | `TextBox.TextBinding`                                                                            | property | text binding           |
|  [05]   | `CheckBox.CheckedBinding`                                                                        | property | checked-state binding  |
|  [06]   | `NumericStepper.ValueBinding`                                                                    | property | numeric-value binding  |
|  [07]   | `Grid.SelectedItemBinding`                                                                       | property | grid-selection binding |
|  [08]   | `TabControl.SelectedIndexBinding`                                                                | property | tab-selection binding  |
|  [09]   | `IBindable.DataContext`                                                                          | property | model assignment       |

- `IBindable.DataContext`: assignment propagates to every descendant `IBindable` and fires `DataContextChanged`.
- change-aware ctor types: `dataItem: T`, `getValue: Func<T,TValue>`, `setValue: Action<T,TValue>`, `addChangeEvent`/`removeChangeEvent: Action<T, EventHandler<EventArgs>>`; `setValue` and both hooks default `null`, admitting read-only or change-blind sources.

[ENTRYPOINT_SCOPE]: two-way and data-context binding

Each surface returns a live `DualBinding<T>` and carries a trailing `DualBindingMode` with optional default control and context values.

| [INDEX] | [SURFACE]                                                                       | [SHAPE]  | [CAPABILITY]                   |
| :-----: | :------------------------------------------------------------------------------ | :------- | :----------------------------- |
|  [01]   | `BindableExtensions.Bind<T>(IndirectBinding<T>, DirectBinding<T>)`              | static   | explicit-source binding        |
|  [02]   | `BindableExtensions.Bind<T>(IndirectBinding<T>, object, IndirectBinding<T>)`    | static   | supplied-context binding       |
|  [03]   | `BindableExtensions.BindDataContext<T>(IndirectBinding<T>, IndirectBinding<T>)` | static   | ambient-context binding        |
|  [04]   | `BindableBinding.BindDataContext(IndirectBinding<TValue>)`                      | instance | fluent control-context binding |

- `BindableBinding.BindDataContext` overloads admit a `string propertyName`, an `Expression<Func<TObject,TValue>>`, and a `Func`/`Action` accessor pair with `addChangeEvent`/`removeChangeEvent` hooks.

[ENTRYPOINT_SCOPE]: projection and exception guarding

Each projection returns a `BindableBinding<T,TNew>`, so the chain composes.

| [INDEX] | [SURFACE]                                                                            | [SHAPE]  | [CAPABILITY]              |
| :-----: | :----------------------------------------------------------------------------------- | :------- | :------------------------ |
|  [01]   | `BindableBinding.Convert<TNewValue>(Func<TValue,TNewValue>, Func<TNewValue,TValue>)` | instance | bidirectional transform   |
|  [02]   | `BindableBinding.Cast<TNewValue>()`                                                  | instance | reference or value cast   |
|  [03]   | `BindableBinding.Child<TNewValue>(IndirectBinding<TNewValue>)`                       | instance | child-value drilling      |
|  [04]   | `BindableBinding.CatchException<TException>(Func<TException,bool>)`                  | instance | conversion-fault handling |

- `BindableBinding.Convert`: a null `fromValue` (the parameter's default) makes the projection one-way, read-only to the model.
- `BindableBinding.Child` also admits an `Expression<Func<T,TNewValue>>` property path; both overloads return `BindableBinding<T,TNewValue>`.
- `CatchException<TException>(Func<TException,bool>)` is DECLARED on `DirectBinding<T>`/`IndirectBinding<T>` and returns that binding shape; `BindableBinding` reaches it only as the inherited `DirectBinding` form, so the guard attaches on the source side of a dual link and the fluent `BindableBinding` chain ends at the trap. A non-generic `Func<Exception,bool>` overload traps every fault; a handler returning `true` swallows the conversion or access fault.

[ENTRYPOINT_SCOPE]: accessor factories, value accessors, and propagation control

| [INDEX] | [SURFACE]                                                | [SHAPE]  | [CAPABILITY]                   |
| :-----: | :------------------------------------------------------- | :------- | :----------------------------- |
|  [01]   | `Binding.Property<T,TValue>(Expression<Func<T,TValue>>)` | static   | property-path accessor factory |
|  [02]   | `Binding.Delegate<TValue>(get, set, add, remove)`        | static   | callback accessor factory      |
|  [03]   | `Binding.Delegate<T,TValue>(get, set, add, remove, …)`   | static   | item-callback accessor factory |
|  [04]   | `IndirectBinding<T>.GetValue` / `.SetValue`              | instance | data-item value accessor       |
|  [05]   | `IndirectBinding<T>.AfterDelay(TimeSpan, bool)`          | instance | debounced write propagation    |
|  [06]   | `DirectBinding<T>.DataValue` / `.DataValueChanged`       | property | source value and change event  |
|  [07]   | `DualBinding<T>.Unbind` / `.Update`                      | instance | link teardown and forced push  |
|  [08]   | `IBindable.UpdateBindings(BindingUpdateMode)`            | instance | tree-wide propagation          |

- `Bind`/`BindDataContext` `Expression<Func<…>>` selector forms default to `DualBindingMode.TwoWay`; `Convert`/`Delegate` setters receive the source value — or the data item on the two-generic `Delegate` — never a binding instance.

[ENTRYPOINT_SCOPE]: data-store collection binding

| [INDEX] | [SURFACE]                                    | [SHAPE]  | [CAPABILITY]             |
| :-----: | :------------------------------------------- | :------- | :----------------------- |
|  [01]   | `new DataStoreCollection<T>(IEnumerable<T>)` | ctor     | grid or list item source |
|  [02]   | `GridView.DataStore`                         | property | enumerable view source   |
|  [03]   | `ListControl.DataStore`                      | property | enumerable view source   |
|  [04]   | `TreeGridView.DataStore`                     | property | tree item source         |
|  [05]   | `ListControl.ItemTextBinding`                | property | item-text projection     |
|  [06]   | `ListControl.ItemKeyBinding`                 | property | item-key projection      |

## [03]-[IMPLEMENTATION_LAW]

[TOPOLOGY]:
- a binding is two halves — `IndirectBinding<T>` over the model value, `DirectBinding<T>` over the control value — that `Bind`/`BindDataContext` join into a live `DualBinding<T>`, and `DualBindingMode` fixes which edges relay
- `BindableBinding<T,TValue>` is the fluent control-side entry: a control exposes its `*Binding`, and its projection chain reshapes and guards the value before it reaches the model
- `DataContext` is the ambient model: assigning it on a container walks every descendant `IBindable`, so a `BindDataContext` binding resolves without a per-control source assignment
- a collection view binds by source shape: an enumerable source reaches the enumerable `DataStore` sinks unadapted, a virtualized `IDataStore<T>` window reaches them through `DataStoreVirtualCollection<T>`, and a tree reaches `TreeGridView` as an `ITreeGridStore<T>`

[STACKING]:
- `api-languageext`(`libs/dotnet/.api/api-languageext.md`): a model-bound field validates in the `Convert` to-source direction through a `Validation<Error,A>` gate exiting `.ToFin()` before the model accepts; callback-only `CatchException<TException>` contains a host update and publishes `Error.New(raised.Message, (Exception)raised)` to the owning failure sink, avoiding the generic exception's ambiguous `Error` conversion without becoming a substitute for `Try.lift` on a result-returning call; a `DataContext` swap is an `Eff` marshalled onto the UI thread after admission.
- `api-thinktecture-runtime-extensions`(`libs/dotnet/.api/api-thinktecture-runtime-extensions.md`): a bound model field is a `[ValueObject<T>]` and `BindableBinding.Convert` maps the control primitive to and from it; a `[SmartEnum<TKey>]` case binds to a `DropDown`/`SegmentedButton` selection through `Convert`, the smart enum owning the display-label projection
- `api-eto-forms`(`libs/dotnet/.api/api-eto-forms.md`): every control `*Binding` is the `BindableBinding` this surface consumes, and the grid/list/tree `DataStore` properties are the `IDataStore<T>` sinks it fills
- `api-eto-runtime`(`libs/dotnet/.api/api-eto-runtime.md`): `DataContext` propagation and binding update run on the UI thread through `Application.Instance`
- one panel `DataContext` drives every editable field through `BindDataContext`, so a model feeds its whole inspector without per-field source wiring

[LOCAL_ADMISSION]:
- binding is host-provided and composed directly — a field binds through its `*Binding` and `BindDataContext`
- a bound model value is a typed `[ValueObject<T>]` or `[SmartEnum<TKey>]` reached through `Convert`
- a collection view fills through the sink its view declares — an enumerable, a virtualized `IDataStore<T>` behind its adapter, or an `ITreeGridStore<T>`
