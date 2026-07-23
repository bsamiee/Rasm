# [PY_COMPUTE_API_ONNX]

`onnx` owns the ONNX model intermediate representation for the compute model-asset rail: the protobuf graph/tensor schema and the structural gate that admits a `ModelProto`. `onnx` sits between the producer `skl2onnx` and the inference runtime `onnxruntime`, validating a model before it graduates to `Rasm.Compute`.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `onnx`
- package: `onnx` (Apache-2.0)
- module: `onnx`
- namespaces: `onnx.checker`, `onnx.shape_inference`, `onnx.helper`, `onnx.numpy_helper`, `onnx.version_converter`, `onnx.compose`, `onnx.defs`
- rail: model

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: protobuf message and error types

| [INDEX] | [SYMBOL]                         | [TYPE_FAMILY] | [CAPABILITY]                                                         |
| :-----: | :------------------------------- | :------------ | :------------------------------------------------------------------- |
|  [01]   | `onnx.ModelProto`                | message       | root with `ir_version`, `opset_import`, `graph`, `metadata_props`    |
|  [02]   | `onnx.GraphProto`                | message       | `node`, `input`, `output`, `initializer`, `value_info`               |
|  [03]   | `onnx.NodeProto`                 | message       | one operator instance with `op_type`, `input`, `output`, `attribute` |
|  [04]   | `onnx.TensorProto`               | message       | initializer/constant tensor carrying the dtype enum                  |
|  [05]   | `onnx.AttributeProto`            | message       | typed node attribute (int, float, tensor, graph, ...)                |
|  [06]   | `onnx.ValueInfoProto`            | message       | named input/output with `TypeProto` and shape                        |
|  [07]   | `onnx.TypeProto`                 | message       | element type and tensor/sequence/map/optional shape                  |
|  [08]   | `onnx.TensorShapeProto`          | message       | dimension list with sized or symbolic dims                           |
|  [09]   | `onnx.OperatorSetIdProto`        | message       | domain plus opset version pair                                       |
|  [10]   | `onnx.FunctionProto`             | message       | reusable local function definition                                   |
|  [11]   | `onnx.SparseTensorProto`         | message       | COO-encoded sparse initializer                                       |
|  [12]   | `checker.ValidationError`        | exception     | raised by `check_model` on a malformed graph                         |
|  [13]   | `shape_inference.InferenceError` | exception     | raised by `infer_shapes(strict_mode=True)` on an unpropagated shape  |

[PUBLIC_TYPE_SCOPE]: `TensorProto` element-type enum (`onnx.TensorProto.<NAME>`)

| [INDEX] | [NAME]           | [VALUE] | [DTYPE]                                         |
| :-----: | :--------------- | ------: | :---------------------------------------------- |
|  [01]   | `FLOAT`          |       1 | 32-bit float                                    |
|  [02]   | `FLOAT16`        |      10 | 16-bit float                                    |
|  [03]   | `DOUBLE`         |      11 | 64-bit float                                    |
|  [04]   | `INT8`           |       3 | 8-bit signed int                                |
|  [05]   | `INT32`          |       6 | 32-bit signed int                               |
|  [06]   | `INT64`          |       7 | 64-bit signed int                               |
|  [07]   | `UINT8`          |       2 | 8-bit unsigned int                              |
|  [08]   | `BOOL`           |       9 | boolean                                         |
|  [09]   | `STRING`         |       8 | UTF-8 string                                    |
|  [10]   | `BFLOAT16`       |      16 | bfloat16 (truncated float32)                    |
|  [11]   | `FLOAT8E4M3FN`   |      17 | 8-bit float (E4M3, finite) — quantized assets   |
|  [12]   | `FLOAT8E5M2`     |      19 | 8-bit float (E5M2)                              |
|  [13]   | `UINT4` / `INT4` |   21/22 | 4-bit packed int — sub-byte weight quantization |
|  [14]   | `FLOAT4E2M1`     |      23 | 4-bit float — sub-byte quantization type        |

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: load, save, check, and infer

| [INDEX] | [SURFACE]                                                        | [SHAPE] | [CAPABILITY]                                                |
| :-----: | :--------------------------------------------------------------- | :------ | :---------------------------------------------------------- |
|  [01]   | `load(f, format, load_external_data) -> ModelProto`              | static  | reads a model from a path or file object                    |
|  [02]   | `load_model_from_string(s, format) -> ModelProto`                | static  | parses a model from in-memory bytes                         |
|  [03]   | `save(proto, f, *, save_as_external_data, location)`             | static  | writes a model, optionally externalizing tensors            |
|  [04]   | `load_tensor(f, format)` / `save_tensor(proto, f, format)`       | static  | reads/writes a single `TensorProto`                         |
|  [05]   | `checker.check_model(model, *, full_check)`                      | static  | validates structure; `full_check=True` adds inference       |
|  [06]   | `checker.{check_graph, check_node, check_tensor}(ctx)`           | static  | per-element validation for incremental builds               |
|  [07]   | `shape_inference.infer_shapes(model, *, strict_mode, data_prop)` | static  | propagates static shapes; `data_prop=True` folds const data |
|  [08]   | `shape_inference.infer_shapes_path(in, out, *, strict_mode)`     | static  | shape inference for a path model, past the 2GB limit        |
|  [09]   | `version_converter.convert_version(model, target_version)`       | static  | rewrites the model to a target opset version                |
|  [10]   | `utils.extract_model(in, out, input_names, output_names)`        | static  | slices a sub-model between named tensors                    |
|  [11]   | `parser.parse_model(text)` / `parser.parse_graph(text)`          | static  | round-trips the human-readable ONNX text format             |
|  [12]   | `inliner.inline_local_functions(model, exclude) -> ModelProto`   | static  | inlines `FunctionProto` callsites into the graph            |

- external-data streaming (static): `external_data_helper.load_external_data_for_model` `convert_model_to_external_data` `write_external_data_tensors` — side-files the >2GB initializers.

[ENTRYPOINT_SCOPE]: construction and array conversion

| [INDEX] | [SURFACE]                                                             | [SHAPE] | [CAPABILITY]                                          |
| :-----: | :-------------------------------------------------------------------- | :------ | :---------------------------------------------------- |
|  [01]   | `helper.make_node(op_type, inputs, outputs, **kwargs) -> NodeProto`   | factory | builds one operator node                              |
|  [02]   | `helper.make_graph(nodes, name, inputs, outputs, initializer)`        | factory | assembles a graph from nodes and io                   |
|  [03]   | `helper.make_model(graph, **kwargs) -> ModelProto`                    | factory | wraps a graph in a model with opset imports           |
|  [04]   | `helper.make_tensor(name, data_type, dims, vals, raw)`                | factory | builds an initializer/constant tensor                 |
|  [05]   | `helper.make_tensor_value_info(name, elem_type, shape)`               | factory | builds a typed graph input/output descriptor          |
|  [06]   | `helper.make_attribute(key, value, attr_type)`                        | factory | builds a typed node attribute                         |
|  [07]   | `helper.make_opsetid(domain, version)` / `make_function(...)`         | factory | builds opset-id pairs and reusable local functions    |
|  [08]   | `helper.printable_graph(graph, prefix) -> str`                        | static  | renders a human-readable graph dump                   |
|  [09]   | `numpy_helper.to_array(tensor, base_dir)` / `from_array(array, name)` | static  | converts `TensorProto`↔`np.ndarray`, incl bf16/float8 |
|  [10]   | `numpy_helper.{to_list, from_list, to_dict, from_dict, to_optional}`  | static  | sequence/map/optional tensor bridges                  |
|  [11]   | `compose.{merge_models, add_prefix, expand_out_dim}`                  | static  | merges, namespaces, or adds a batch dim to a model    |
|  [12]   | `defs.{onnx_opset_version, get_all_schemas, get_schema}`              | static  | reports the supported opset and operator schemas      |

- dtype-enum↔NumPy bridges (static): `helper.tensor_dtype_to_np_dtype` `np_dtype_to_tensor_dtype` `tensor_dtype_to_field` — maps the `TensorProto` element-type enum onto NumPy dtypes.

## [04]-[IMPLEMENTATION_LAW]

[TOPOLOGY]:
- A model asset loads as `ModelProto`; `checker.check_model(model, full_check=True)` raises `ValidationError` on a malformed graph, mismatched opset, or unbound value.
- `shape_inference.infer_shapes(model, strict_mode=True)` returns a new `ModelProto` with inferred `value_info` and raises `InferenceError` on an unpropagated shape; a structural validator folds both errors into one failed verdict.
- `make_model` builds bottom-up from `make_node`, `make_graph`, and `make_tensor_value_info`; `make_opsetid` with the `opset_import` field pins the operator set.
- `version_converter.convert_version(model, target_version)` rewrites a model across opset versions; the installed opset is `defs.onnx_opset_version()`.

[STACKING]:
- `skl2onnx`(`.api/skl2onnx.md`): `skl2onnx.to_onnx(fitted_estimator, X, target_opset=onnx.defs.onnx_opset_version())` produces the `ModelProto` this surface gates; the chosen `target_opset` never exceeds `defs.onnx_opset_version()`.
- `onnxruntime`(`.api/onnxruntime.md`): a `ModelProto` passing the gate feeds `onnxruntime.InferenceSession(model.SerializeToString()).run(...)`; onnx is the structural gate ahead of the runtime.
- `numpy`(`libs/python/.api/numpy.md`): `numpy_helper.to_array`/`from_array` bridges an initializer `TensorProto` to an `np.ndarray`, and `helper.tensor_dtype_to_np_dtype` maps the element-type enum so a validator compares an initializer's declared dtype against the sample-run array.
- within-lib: the sub-byte enum members (`FLOAT8E4M3FN`, `INT4`, `FLOAT4E2M1`) make the dtype table exhaustive so a structural validator admits a quantized graph instead of rejecting it as unknown.

[LOCAL_ADMISSION]:
- A model asset admits after `load`, `check_model(full_check=True)`, and `infer_shapes(strict_mode=True)`; the model-asset receipt captures the opset (`defs.onnx_opset_version()`) and the graph input/output signatures.

[RAIL_LAW]:
- Package: `onnx`
- Owns: offline ONNX model IR — the structural gate, bottom-up construction, and `TensorProto`↔NumPy bridging for the model-asset rail
- Accept: a model asset validated through `check_model(full_check=True)` + `infer_shapes(strict_mode=True)` with a captured opset and input/output signature receipt; producer output from `skl2onnx.to_onnx`
- Reject: hand-parsed protobuf; hand-written `TensorProto`↔NumPy conversion `numpy_helper` owns; wrapper-renames of the checker/inference calls; runtime inference `onnxruntime` owns
