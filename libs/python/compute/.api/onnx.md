# [PY_COMPUTE_API_ONNX]

`onnx` supplies the open model intermediate-representation: the graph protobuf schema, model construction and load/save, structural checking, and shape inference for the compute model-asset rail. The package owner loads a model asset as `ModelProto`, validates it through `checker.check_model`, and propagates shapes through `shape_inference.infer_shapes` before it graduates to the C# `Rasm.Compute` runtime; it never re-implements the IR onnx owns.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `onnx`
- package: `onnx`
- import: `onnx`; submodules `onnx.checker`, `onnx.shape_inference`, `onnx.helper`, `onnx.numpy_helper`, `onnx.version_converter`, `onnx.compose`, `onnx.defs`
- owner: `compute`
- rail: model
- license: Apache-2.0
- capability: ONNX model IR — graph/tensor protobuf schema, model load/save with external-data handling, structural checking, static shape inference, opset version conversion, subgraph extraction, textual round-trip, and `TensorProto`↔`np.ndarray` conversion (including bf16/float8 dtypes)

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: protobuf message types
- rail: model

| [INDEX] | [SYMBOL]                         | [PACKAGE_ROLE]     | [CAPABILITY]                                                         |
| :-----: | :------------------------------- | :----------------- | :------------------------------------------------------------------- |
|  [01]   | `onnx.ModelProto`                | model message      | root with `ir_version`, `opset_import`, `graph`, `metadata_props`    |
|  [02]   | `onnx.GraphProto`                | graph message      | `node`, `input`, `output`, `initializer`, `value_info`               |
|  [03]   | `onnx.NodeProto`                 | node message       | one operator instance with `op_type`, `input`, `output`, `attribute` |
|  [04]   | `onnx.TensorProto`               | tensor message     | initializer/constant tensor; carries the dtype enum                  |
|  [05]   | `onnx.AttributeProto`            | attribute message  | typed node attribute (int, float, tensor, graph, ...)                |
|  [06]   | `onnx.ValueInfoProto`            | value-info message | named input/output with `TypeProto` and shape                        |
|  [07]   | `onnx.TypeProto`                 | type message       | element type and tensor/sequence/map/optional shape                  |
|  [08]   | `onnx.TensorShapeProto`          | shape message      | dimension list with sized or symbolic dims                           |
|  [09]   | `onnx.OperatorSetIdProto`        | opset id message   | domain plus opset version pair                                       |
|  [10]   | `onnx.FunctionProto`             | function message   | reusable local function definition                                   |
|  [11]   | `onnx.SparseTensorProto`         | sparse tensor      | COO-encoded sparse initializer                                       |
|  [12]   | `checker.ValidationError`        | validation error   | raised by `check_model` on a malformed graph                         |
|  [13]   | `shape_inference.InferenceError` | inference error    | raised by `infer_shapes(strict_mode=True)` on an unpropagated shape  |

[PUBLIC_TYPE_SCOPE]: TensorProto element-type enum (`onnx.TensorProto.<NAME>`)
- rail: model

| [INDEX] | [NAME]    | [VALUE] | [DTYPE]            |
| :-----: | :-------- | ------: | :----------------- |
|  [01]   | `FLOAT`   |       1 | 32-bit float       |
|  [02]   | `FLOAT16` |      10 | 16-bit float       |
|  [03]   | `DOUBLE`  |      11 | 64-bit float       |
|  [04]   | `INT8`    |       3 | 8-bit signed int   |
|  [05]   | `INT32`   |       6 | 32-bit signed int  |
|  [06]   | `INT64`   |       7 | 64-bit signed int  |
|  [07]   | `UINT8`   |       2 | 8-bit unsigned int |
|  [08]   | `BOOL`    |       9 | boolean            |
|  [09]   | `STRING`  |       8 | UTF-8 string       |
|  [10]   | `BFLOAT16` |     16 | bfloat16 (truncated float32) |
|  [11]   | `FLOAT8E4M3FN` |  17 | 8-bit float (E4M3, finite) — quantized model assets |
|  [12]   | `FLOAT8E5M2` |   19 | 8-bit float (E5M2)         |
|  [13]   | `UINT4` / `INT4` | 21/22 | 4-bit packed int — sub-byte weight quantization |
|  [14]   | `FLOAT4E2M1` | 23 | 4-bit float — newest sub-byte quantization type   |

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: load, save, check, and infer
- rail: model

| [INDEX] | [SURFACE]                                                                                                               | [ENTRY_FAMILY]   | [CAPABILITY]                                         |
| :-----: | :---------------------------------------------------------------------------------------------------------------------- | :--------------- | :--------------------------------------------------- |
|  [01]   | `load(f, format=None, load_external_data=True) -> ModelProto`                                                           | model load       | reads a model from a path or file object             |
|  [02]   | `load_model_from_string(s, format='protobuf') -> ModelProto`                                                            | model load       | parses a model from in-memory bytes                  |
|  [03]   | `save(proto, f, *, save_as_external_data=False, location, size_threshold=1024) -> None`                                 | model save       | writes a model, optionally externalizing tensors     |
|  [04]   | `load_tensor(f, format=None)` / `save_tensor(proto, f, format=None)`                                                    | tensor io        | reads/writes a single `TensorProto`                  |
|  [05]   | `checker.check_model(model, full_check=False, skip_opset_compatibility_check=False, check_custom_domain=False) -> None` | structural check | validates model structure, raising `ValidationError`; `full_check=True` also runs shape inference |
|  [06]   | `checker.check_graph(graph, ctx)` / `check_node(node, ctx)` / `check_tensor(tensor, ctx)`                               | structural check | per-element validation for incrementally built graphs |
|  [07]   | `shape_inference.infer_shapes(model, check_type=False, strict_mode=False, data_prop=False) -> ModelProto`               | shape inference  | propagates static shapes through the graph; `data_prop=True` propagates constant data through shape ops |
|  [08]   | `shape_inference.infer_shapes_path(model_path, output_path, *, check_type, strict_mode, data_prop)`                     | shape inference  | infers shapes for a path-based model (avoids the 2GB protobuf limit) |
|  [09]   | `version_converter.convert_version(model, target_version) -> ModelProto`                                                | opset convert    | rewrites the model to a target opset version         |
|  [10]   | `utils.extract_model(input_path, output_path, input_names, output_names, *, check_model=True)`                          | subgraph extract | slices a sub-model between named tensors             |
|  [11]   | `parser.parse_model(text) -> ModelProto` / `parser.parse_graph(text)`                                                  | textual io       | round-trips the human-readable ONNX text format      |
|  [12]   | `inliner.inline_local_functions(model, exclude=None) -> ModelProto`                                                    | function inline  | inlines `FunctionProto` callsites into the main graph |
|  [13]   | `external_data_helper.{load_external_data_for_model,convert_model_to_external_data,write_external_data_tensors}`        | external data    | streams large initializers to side files (>2GB models) |

[ENTRYPOINT_SCOPE]: construction and array conversion
- rail: model

| [INDEX] | [SURFACE]                                                                                                    | [ENTRY_FAMILY]     | [CAPABILITY]                                     |
| :-----: | :----------------------------------------------------------------------------------------------------------- | :----------------- | :----------------------------------------------- |
|  [01]   | `helper.make_node(op_type, inputs, outputs, name, domain, **kwargs) -> NodeProto`                            | node builder       | builds one operator node                         |
|  [02]   | `helper.make_graph(nodes, name, inputs, outputs, initializer, value_info, sparse_initializer) -> GraphProto` | graph builder      | assembles a graph from nodes and io              |
|  [03]   | `helper.make_model(graph, **kwargs) -> ModelProto`                                                           | model builder      | wraps a graph in a model with opset imports      |
|  [04]   | `helper.make_tensor(name, data_type, dims, vals, raw=False) -> TensorProto`                                  | tensor builder     | builds an initializer/constant tensor            |
|  [05]   | `helper.make_tensor_value_info(name, elem_type, shape) -> ValueInfoProto`                                    | value-info builder | builds a typed graph input/output descriptor     |
|  [06]   | `helper.make_attribute(key, value, attr_type=None) -> AttributeProto`                                        | attribute builder  | builds a typed node attribute                    |
|  [07]   | `helper.make_opsetid(domain, version)` / `helper.make_function(domain, fname, inputs, outputs, nodes, opset_imports, attributes) -> FunctionProto` | model builder | builds opset-id pairs and reusable local functions |
|  [08]   | `helper.tensor_dtype_to_np_dtype(t)` / `np_dtype_to_tensor_dtype(d)` / `tensor_dtype_to_field(t)`            | dtype map          | bridges the `TensorProto` element-type enum to NumPy dtypes (replaces the deprecated `mapping.TENSOR_TYPE_MAP`) |
|  [09]   | `helper.printable_graph(graph, prefix='') -> str`                                                            | graph printer      | renders a human-readable graph dump              |
|  [10]   | `numpy_helper.to_array(tensor, base_dir='')` / `from_array(array, name=None)`                                | array convert      | converts between `TensorProto` and `np.ndarray`, including bf16/float8 via ml_dtypes |
|  [11]   | `numpy_helper.{to_list,from_list,to_dict,from_dict,to_optional,bfloat16_to_float32,float8e4m3_to_float32}`   | array convert      | sequence/map/optional tensor bridges and sub-byte float decoders |
|  [12]   | `compose.merge_models(m1, m2, io_map)` / `add_prefix(model, prefix)` / `expand_out_dim(model, dim_idx)`      | model compose      | merges two models, namespaces a model, or adds a batch dim |
|  [13]   | `defs.onnx_opset_version()` / `defs.get_all_schemas()` / `defs.get_schema(op_type, max_version, domain)`     | opset registry     | reports the supported opset and operator schemas (`OpSchema`) |

## [04]-[IMPLEMENTATION_LAW]

[VALIDATION_TOPOLOGY]:
- A model asset loads as `ModelProto` through `load`; `checker.check_model(model, full_check=True)` raises `ValidationError` on a malformed graph, mismatched opset, or unbound value.
- `shape_inference.infer_shapes` returns a new `ModelProto` with inferred `value_info`; `strict_mode=True` raises `shape_inference.InferenceError` on inference failures rather than leaving shapes unset, so a structural validator catches both `checker.ValidationError` and `shape_inference.InferenceError` to fold an unpropagated shape into a failed structural verdict.
- The dtype enum on `TensorProto` (`FLOAT=1`, `INT64=7`, ...) names initializer element types; `numpy_helper` bridges those tensors to NumPy arrays.
- `make_model` is built bottom-up from `make_node`, `make_graph`, and `make_tensor_value_info`; `make_opsetid` plus the `opset_import` field pin the operator set.
- `version_converter.convert_version` rewrites a model across opset versions; the installed opset is `defs.onnx_opset_version()`.

[STACKING_TOPOLOGY]:
- `skl2onnx` → onnx → `onnxruntime`: the export-validate-execute rail is `skl2onnx.to_onnx(fitted_estimator, X, target_opset=onnx.defs.onnx_opset_version())` → `ModelProto`, then `onnx.checker.check_model(model, full_check=True)` + `onnx.shape_inference.infer_shapes(model, strict_mode=True)`, then `onnxruntime.InferenceSession(model.SerializeToString()).run(...)`. onnx is the structural gate between the producer (`skl2onnx`) and the runtime (`onnxruntime`); the target opset chosen for `to_onnx` must not exceed `defs.onnx_opset_version()`.
- onnx → `numpy`: `numpy_helper.to_array`/`from_array` is the only sanctioned bridge between an initializer `TensorProto` and an `np.ndarray`; `helper.tensor_dtype_to_np_dtype` maps the element-type enum so a validator can compare an initializer's declared dtype against the NumPy array a sample run feeds `onnxruntime`.
- Quantized/LLM assets: the sub-byte enum members (`FLOAT8E4M3FN`, `INT4`, `FLOAT4E2M1`) are why the dtype table is exhaustive — a structural validator must recognize them to check a quantized graph rather than reject it as unknown.

[STUDY_ROUTING]:
- A model asset is loaded, structurally checked (`check_model(full_check=True)`), and shape-inferred (`infer_shapes(strict_mode=True)`); the model-asset receipt captures the opset (`defs.onnx_opset_version()`), the graph input/output signatures, and both error rails folded into one structural verdict.
- ONNX validation is the offline structural half of the model-asset rail; runtime inference is the `onnxruntime` half, the producer is `skl2onnx`, and product model execution stays in `Rasm.Compute`.

[RAIL_LAW]:
- Package: `onnx`
- Owns: offline ONNX model IR construction, load/save, structural checking, shape inference, subgraph extraction, function inlining, opset conversion, and `TensorProto`↔NumPy conversion for the model-asset rail
- Accept: a model asset validated through `checker.check_model(full_check=True)` + `shape_inference.infer_shapes(strict_mode=True)` with a captured opset and input/output signature receipt; producer output from `skl2onnx.to_onnx`
- Reject: hand-parsed protobuf; hand-written `TensorProto`↔NumPy conversion that `numpy_helper` owns; wrapper-renames of the checker/inference calls; runtime inference claims `onnxruntime` owns
