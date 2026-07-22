# [PY_DATA_API_ARRO3_COMPUTE]

`arro3.compute` runs arithmetic, aggregation, cast, boolean-null, selection, dictionary-encode, and temporal-extraction kernels directly on `arro3.core` `Array`/`ChunkedArray`/`ArrayReader` inputs without a `pyarrow.compute` re-import. Each kernel intakes through its declared `arro3.core.types` structural protocol, so a `pyarrow`, `nanoarrow`, `polars`, or ADBC frame flows in by PyCapsule with no producer-named branch. Stream-overloaded kernels return a lazy `ArrayReader`; array-only kernels, including `take`, receive explicitly materialized or consolidated `ArrayInput` values.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `arro3-compute`
- package: `arro3-compute`
- owner: `data`
- module: `arro3.compute`
- asset: native extension (Rust/PyO3) over `arrow-rs` `arrow-arith`, `arrow-select`, `arrow-cast`, `arrow-ord`
- license: `MIT OR Apache-2.0`
- rail: arrow-compute

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: temporal part vocabulary (`arro3.compute.enums`, `arro3.compute.types`)
- rail: arrow-compute

`date_part` discriminates on a bounded vocabulary supplied either as the `DatePart` string-enum member or the equivalent `DatePartT` literal; both forms name the same extractable temporal field.

| [INDEX] | [SYMBOL]    | [TYPE_FAMILY] | [ROLE]                                           |
| :-----: | :---------- | :------------ | :----------------------------------------------- |
|  [01]   | `DatePart`  | `str`-enum    | temporal extraction vocabulary                   |
|  [02]   | `DatePartT` | literal alias | lowercase-string form of every `DatePart` member |

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: arithmetic kernels (`arro3.compute._arith`)
- rail: arrow-compute

Binary kernels take two `ArrayInput` operands and return an `Array`; the checked form errors on overflow, the `_wrapping` form wraps for integer types. Unary negation follows the same checked/wrapping split.

| [INDEX] | [SURFACE]                                          | [ENTRY_FAMILY]  | [CAPABILITY]                            |
| :-----: | :------------------------------------------------- | :-------------- | :-------------------------------------- |
|  [01]   | `add(lhs, rhs)`                                    | checked binary  | `lhs + rhs`, error on overflow          |
|  [02]   | `sub(lhs, rhs)`                                    | checked binary  | `lhs - rhs`, error on overflow          |
|  [03]   | `mul(lhs, rhs)`                                    | checked binary  | `lhs * rhs`, error on overflow          |
|  [04]   | `div(lhs, rhs)`                                    | binary          | `lhs / rhs`                             |
|  [05]   | `rem(lhs, rhs)`                                    | binary          | `lhs % rhs`                             |
|  [06]   | `add_wrapping/sub_wrapping/mul_wrapping(lhs, rhs)` | wrapping binary | integer arithmetic wrapping on overflow |
|  [07]   | `neg(array)`                                       | checked unary   | element negation, error on overflow     |
|  [08]   | `neg_wrapping(array)`                              | wrapping unary  | integer negation wrapping on overflow   |

[ENTRYPOINT_SCOPE]: aggregation kernels (`arro3.compute._aggregate`)
- rail: arrow-compute

Aggregations reduce an `ArrayInput | ArrowStreamExportable` to one `arro3.core.Scalar`, absorbing a chunked or streamed column without a materialized intermediate.

| [INDEX] | [SURFACE]    | [ENTRY_FAMILY] | [CAPABILITY]               |
| :-----: | :----------- | :------------- | :------------------------- |
|  [01]   | `sum(input)` | reduction      | `Scalar` sum of values     |
|  [02]   | `min(input)` | reduction      | `Scalar` minimum of values |
|  [03]   | `max(input)` | reduction      | `Scalar` maximum of values |

[ENTRYPOINT_SCOPE]: selection kernels (`arro3.compute._filter`, `_take`)
- rail: arrow-compute

`filter` and `take` are the partition-split primitives: `filter` gates `values` by a boolean `predicate`, `take` gathers `values` at a numeric `indices` array. `filter` is stream-dispatched (an `ArrowStreamExportable` pair returns an `ArrayReader`); `take` is array-only and returns an `Array`.

| [INDEX] | [SURFACE]                   | [ENTRY_FAMILY] | [CAPABILITY]                                                          |
| :-----: | :-------------------------- | :------------- | :-------------------------------------------------------------------- |
|  [01]   | `filter(values, predicate)` | boolean gate   | keep `values` where `predicate` is true; array→`Array`, stream→reader |
|  [02]   | `take(values, indices)`     | gather         | select `values` at numeric `indices`, returns `Array`                 |

[ENTRYPOINT_SCOPE]: cast, boolean-null, dictionary, and temporal kernels
- rail: arrow-compute

`cast`, `is_null`, `is_not_null`, and `dictionary_encode` are `@overload`-dispatched on the array-vs-stream axis; `can_cast_types` is a pure type predicate; `date_part` extracts an int32 field from a temporal array under the `DatePart`/`DatePartT` vocabulary.

| [INDEX] | [SURFACE]                               | [ENTRY_FAMILY]  | [CAPABILITY]                                                          |
| :-----: | :-------------------------------------- | :-------------- | :-------------------------------------------------------------------- |
|  [01]   | `cast(input, to_type)`                  | type projection | cast to `ArrowSchemaExportable` target (`DataType`/`Field`/`Schema`)  |
|  [02]   | `can_cast_types(from_type, to_type)`    | type predicate  | `bool` — whether a cast is defined                                    |
|  [03]   | `is_null(input)` / `is_not_null(input)` | boolean mask    | non-null boolean array of per-element validity                        |
|  [04]   | `dictionary_encode(array)`              | encode          | dictionary-encoded array; no-op when already dictionary               |
|  [05]   | `date_part(input, part)`                | temporal        | int32 extraction of `part` from date/time/timestamp/interval/duration |
|  [06]   | `concat(input)`                         | consolidation   | collapse a `ChunkedArray`/stream exportable into one `Array`          |

## [04]-[IMPLEMENTATION_LAW]

[ARROW_COMPUTE_TOPOLOGY]:
- namespace: `arro3.compute` re-exports kernels from the internal `_arith`/`_aggregate`/`_cast`/`_boolean`/`_filter`/`_take`/`_dictionary`/`_temporal` extension modules beside the `enums` (`DatePart`) and `types` (`DatePartT`) vocabulary submodules
- dispatch axis: `cast`, `is_null`, `is_not_null`, `dictionary_encode`, `filter`, and `date_part` are overload-typed on `ArrayInput` vs `ArrowStreamExportable` — array input materializes an `Array`, stream input returns a lazy `ArrayReader`
- overflow policy: arithmetic splits checked (`add`/`sub`/`mul`/`neg`) against wrapping (`add_wrapping`/`sub_wrapping`/`mul_wrapping`/`neg_wrapping`); checked kernels error, wrapping kernels wrap integer types
- aggregation collapses any array or stream to one `arro3.core.Scalar`; `filter` gates arrays or paired streams, while `take` gathers only materialized arrays by numeric index array

[LOCAL_ADMISSION]:
- Intake each PyCapsule producer through the protocol its kernel declares; stream producers stay on stream-overloaded kernels, and array-only kernels receive an explicit array projection.
- Keep the stream form lazy: composing `filter`/`cast`/`dictionary_encode`/`date_part` onto an `ArrowStreamExportable` chains `ArrayReader`s and defers work to a terminal `read_all`.
- Reject a `pyarrow.compute` re-import where a kernel exists here; sort ordering is not in this surface and stays on its own path.

[INTEGRATION_RAILS]:
- materialize CDF split: the `load_cdf` `arro3.core.RecordBatchReader` stays streaming through `filter`; gather drains the filtered reader, selects the target column, and consolidates values and numeric indices to `ArrayInput` before `take`, closing the `pa.table(...)` PyCapsule re-import before the key-sorted split.
- arro3-core ↔ arro3-compute: kernels consume `arro3.core` types and emit `Array`/`ArrayReader`/`Scalar` back into the same memory model, so a `Table`/`ChunkedArray` round-trips through a kernel with no copy and no producer library named.
- streaming spine: pair a stream-typed kernel (`filter`, `cast`, `date_part`) with `ArrayReader`/`RecordBatchReader` so mask, cast, and temporal extraction stay lazy until a terminal `read_all`/`to_numpy`.

[RAIL_LAW]:
- Package: `arro3-compute`
- Owns: arithmetic, aggregation, cast, boolean-null, selection, dictionary-encode, and temporal-part kernels over the `arro3.core` Arrow memory model
- Accept: any PyCapsule producer through the declared `arro3.core.types` protocol; stream input only where an `ArrowStreamExportable` overload exists
- Reject: a `pyarrow.compute` re-import where a kernel exists here; sort/order kernels absent from this surface
