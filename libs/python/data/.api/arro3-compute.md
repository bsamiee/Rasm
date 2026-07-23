# [PY_DATA_API_ARRO3_COMPUTE]

`arro3.compute` runs Arrow arithmetic, aggregation, selection, cast, boolean, dictionary, and temporal kernels directly on the `arro3.core` memory model, each intaking any PyCapsule producer through its declared `arro3.core.types` protocol so a `pyarrow`, `polars`, or ADBC frame flows in with no producer-named branch. Stream-overloaded kernels chain a lazy `ArrayReader`; array-only kernels take a materialized `ArrayInput`, and no path re-imports `pyarrow.compute`.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `arro3-compute`
- package: `arro3-compute` (MIT OR Apache-2.0)
- owner: `data`
- module: `arro3.compute`
- asset: native extension (Rust/PyO3) over `arrow-rs` `arrow-arith`, `arrow-select`, `arrow-cast`, `arrow-ord`
- rail: arrow-compute

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: temporal part vocabulary (`arro3.compute.enums`, `arro3.compute.types`)

`date_part` takes the temporal field as a `DatePart` string-enum member or the equivalent `DatePartT` literal — one bounded vocabulary in two spellings.

| [INDEX] | [SYMBOL]    | [TYPE_FAMILY] | [ROLE]                                           |
| :-----: | :---------- | :------------ | :----------------------------------------------- |
|  [01]   | `DatePart`  | `str`-enum    | temporal extraction vocabulary                   |
|  [02]   | `DatePartT` | literal alias | lowercase-string form of every `DatePart` member |

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: arithmetic kernels (`arro3.compute._arith`)

Binary kernels take two `ArrayInput` operands, unary negation one; each returns an `Array`.

| [INDEX] | [SURFACE]                                          | [ENTRY_FAMILY]  | [CAPABILITY]                            |
| :-----: | :------------------------------------------------- | :-------------- | :-------------------------------------- |
|  [01]   | `add(lhs, rhs)`                                    | checked binary  | `lhs + rhs`, error on overflow          |
|  [02]   | `sub(lhs, rhs)`                                    | checked binary  | `lhs - rhs`, error on overflow          |
|  [03]   | `mul(lhs, rhs)`                                    | checked binary  | `lhs * rhs`, error on overflow          |
|  [04]   | `div(lhs, rhs)`                                    | binary          | `lhs / rhs`, no overflow guard          |
|  [05]   | `rem(lhs, rhs)`                                    | binary          | `lhs % rhs`, no overflow guard          |
|  [06]   | `add_wrapping/sub_wrapping/mul_wrapping(lhs, rhs)` | wrapping binary | integer arithmetic wrapping on overflow |
|  [07]   | `neg(array)`                                       | checked unary   | element negation, error on overflow     |
|  [08]   | `neg_wrapping(array)`                              | wrapping unary  | integer negation wrapping on overflow   |

[ENTRYPOINT_SCOPE]: aggregation kernels (`arro3.compute._aggregate`)

Each reduction folds an `ArrayInput | ArrowStreamExportable` to one `arro3.core.Scalar` with no materialized intermediate.

| [INDEX] | [SURFACE]    | [ENTRY_FAMILY] | [CAPABILITY]               |
| :-----: | :----------- | :------------- | :------------------------- |
|  [01]   | `sum(input)` | reduction      | `Scalar` sum of values     |
|  [02]   | `min(input)` | reduction      | `Scalar` minimum of values |
|  [03]   | `max(input)` | reduction      | `Scalar` maximum of values |

[ENTRYPOINT_SCOPE]: selection kernels (`arro3.compute._filter`, `_take`)

`filter` gates `values` by a boolean `predicate` and is stream-dispatched; `take` gathers `values` at a numeric `indices` array, array-only and always `Array`.

| [INDEX] | [SURFACE]                   | [ENTRY_FAMILY] | [CAPABILITY]                                                          |
| :-----: | :-------------------------- | :------------- | :-------------------------------------------------------------------- |
|  [01]   | `filter(values, predicate)` | boolean gate   | keep `values` where `predicate` is true; array→`Array`, stream→reader |
|  [02]   | `take(values, indices)`     | gather         | select `values` at numeric `indices`, returns `Array`                 |

[ENTRYPOINT_SCOPE]: cast, boolean-null, dictionary, and temporal kernels

`cast`/`is_null`/`is_not_null`/`dictionary_encode` overload-dispatch on the array-vs-stream axis; `can_cast_types` is a pure type predicate; `date_part` extracts an int32 field under the `DatePart`/`DatePartT` vocabulary.

| [INDEX] | [SURFACE]                               | [ENTRY_FAMILY]  | [CAPABILITY]                                                          |
| :-----: | :-------------------------------------- | :-------------- | :-------------------------------------------------------------------- |
|  [01]   | `cast(input, to_type)`                  | type projection | cast to `ArrowSchemaExportable` target (`DataType`/`Field`/`Schema`)  |
|  [02]   | `can_cast_types(from_type, to_type)`    | type predicate  | `bool` — whether a cast is defined                                    |
|  [03]   | `is_null(input)` / `is_not_null(input)` | boolean mask    | non-null boolean array of per-element validity                        |
|  [04]   | `dictionary_encode(array)`              | encode          | dictionary-encoded array; no-op when already dictionary               |
|  [05]   | `date_part(input, part)`                | temporal        | int32 extraction of `part` from date/time/timestamp/interval/duration |
|  [06]   | `concat(input)`                         | consolidation   | collapse a `ChunkedArray`/stream exportable into one `Array`          |

## [04]-[IMPLEMENTATION_LAW]

[TOPOLOGY]:
- namespace: `arro3.compute` re-exports kernels from the internal `_arith`/`_aggregate`/`_cast`/`_boolean`/`_filter`/`_take`/`_dictionary`/`_temporal` extension modules beside the `enums` (`DatePart`) and `types` (`DatePartT`) vocabulary submodules
- dispatch axis: `cast`, `is_null`, `is_not_null`, `dictionary_encode`, `filter`, and `date_part` overload on `ArrayInput` vs `ArrowStreamExportable` — array input materializes an `Array`, stream input returns a lazy `ArrayReader`
- overflow policy: arithmetic splits checked (`add`/`sub`/`mul`/`neg`) against wrapping (`add_wrapping`/`sub_wrapping`/`mul_wrapping`/`neg_wrapping`); checked kernels error, wrapping kernels wrap integer types, and `div`/`rem` carry neither guard

[LOCAL_ADMISSION]:
- Route each PyCapsule producer through its kernel's declared protocol; a stream producer stays on a stream-overloaded kernel, and an array-only kernel such as `take` or the arithmetic binaries receives an explicit `ArrayInput` projection.

[STACKING]:
- `arro3-core`(`.api/arro3-core.md`): kernels consume `Array`/`ChunkedArray`/`ArrayReader`/`ArrayInput` and emit `Array`/`ArrayReader`/`Scalar` into the same memory model, so a `Table`/`ChunkedArray` round-trips through a kernel with no copy.
- `arro3-io`(`.api/arro3-io.md`): a `read_*` `RecordBatchReader` feeds `filter`/`cast`/`date_part` streaming, and a kernel `Array` lowers back through a `write_*` on one memory model.
- streaming spine: hold the reader and chain `filter`/`cast`/`dictionary_encode`/`date_part` lazy until a terminal `read_all`; a key-sorted materialize split drains the filtered reader, consolidates its values and numeric indices to `ArrayInput`, then `take`.

[RAIL_LAW]:
- Package: `arro3-compute`
- Owns: arithmetic, aggregation, cast, boolean-null, selection, dictionary-encode, and temporal-part kernels over the `arro3.core` Arrow memory model
- Accept: any PyCapsule producer through the declared `arro3.core.types` protocol; stream input only where an `ArrowStreamExportable` overload exists
- Reject: a `pyarrow.compute` re-import where a kernel exists here; sort and order kernels, absent from this surface
