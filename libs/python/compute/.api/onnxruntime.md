# [PY_COMPUTE_API_ONNXRUNTIME]

`onnxruntime` supplies the inference-session runtime that loads and runs a validated ONNX model for the compute model-asset rail. The package owner loads a model asset into an `InferenceSession`, reads its `NodeArg` input/output signatures, and runs sample inputs through `run` to confirm well-shaped output before graduation to the C# `Rasm.Compute` runtime; it never re-implements the runtime onnxruntime owns.

## [1]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `onnxruntime`
- package: `onnxruntime`
- import: `onnxruntime`
- owner: `compute`
- rail: model
- installed: cp313 only (manifest pin `onnxruntime>=1.27.0; python_version<'3.15'`; no cp315 wheel)
- capability: ONNX inference runtime — session load, typed input/output binding, run execution, IO binding, model metadata, execution-provider selection, ahead-of-time compilation, and sparse-tensor values

## [2]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: session and value types
- rail: model

| [INDEX] | [SYMBOL]                       | [PACKAGE_ROLE]  | [CAPABILITY]                                                              |
| :-----: | :----------------------------- | :-------------- | :------------------------------------------------------------------------ |
|   [1]   | `onnxruntime.InferenceSession` | session root    | loads and runs a model over selected execution providers                  |
|   [2]   | `onnxruntime.SessionOptions`   | session policy  | graph-optimization level, thread counts, execution mode, profiling        |
|   [3]   | `onnxruntime.RunOptions`       | run policy      | per-run log level, termination flag, active LoRA adapter                  |
|   [4]   | `onnxruntime.NodeArg`          | io descriptor   | named input/output exposing `name`, `type`, `shape`                       |
|   [5]   | `onnxruntime.OrtValue`         | value carrier   | native typed tensor with `numpy`, `shape`, `element_type`, `device_name`  |
|   [6]   | `onnxruntime.IOBinding`        | io binder       | binds pre-allocated device inputs/outputs to a session                    |
|   [7]   | `onnxruntime.ModelMetadata`    | metadata record | `producer_name`, `domain`, `version`, `graph_name`, `custom_metadata_map` |
|   [8]   | `onnxruntime.ModelCompiler`    | aot compiler    | compiles a model to an EP-specific optimized artifact                     |
|   [9]   | `onnxruntime.SparseTensor`     | sparse value    | COO/CSR/block-sparse tensor value                                         |

[PUBLIC_TYPE_SCOPE]: option enums
- rail: model

| [INDEX] | [SYMBOL]                          | [PACKAGE_ROLE]     | [VALUES]                                                                                            |
| :-----: | :-------------------------------- | :----------------- | :-------------------------------------------------------------------------------------------------- |
|   [1]   | `GraphOptimizationLevel`          | optimization level | `ORT_DISABLE_ALL`, `ORT_ENABLE_BASIC`, `ORT_ENABLE_EXTENDED`, `ORT_ENABLE_LAYOUT`, `ORT_ENABLE_ALL` |
|   [2]   | `ExecutionMode`                   | execution mode     | `ORT_SEQUENTIAL`, `ORT_PARALLEL`                                                                    |
|   [3]   | `ExecutionOrder`                  | execution order    | priority versus default node ordering                                                               |
|   [4]   | `OrtAllocatorType` / `OrtMemType` | memory policy      | allocator kind and memory-type selectors for IO binding                                             |

## [3]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: session lifecycle and inspection
- rail: model

| [INDEX] | [SURFACE]                                                                                     | [ENTRY_FAMILY]   | [CAPABILITY]                                     |
| :-----: | :-------------------------------------------------------------------------------------------- | :--------------- | :----------------------------------------------- |
|   [1]   | `InferenceSession(path_or_bytes, sess_options=None, providers=None, provider_options=None)`   | session ctor     | loads a model, ordering execution providers      |
|   [2]   | `InferenceSession.run(output_names, input_feed, run_options=None)`                            | inference run    | runs the model on a name→array feed dict         |
|   [3]   | `InferenceSession.run_with_ort_values(output_names, input_dict_ort_values, run_options=None)` | inference run    | runs on native `OrtValue` inputs                 |
|   [4]   | `InferenceSession.run_with_iobinding(iobinding, run_options=None)`                            | inference run    | runs against a pre-bound `IOBinding`             |
|   [5]   | `InferenceSession.get_inputs()` / `get_outputs()`                                             | signature query  | returns the input/output `NodeArg` lists         |
|   [6]   | `InferenceSession.get_overridable_initializers()`                                             | signature query  | returns overridable initializer `NodeArg` list   |
|   [7]   | `InferenceSession.get_providers()` / `set_providers(providers)`                               | provider control | reads or sets the active execution providers     |
|   [8]   | `InferenceSession.get_modelmeta() -> ModelMetadata`                                           | metadata query   | returns the embedded model metadata              |
|   [9]   | `InferenceSession.io_binding() -> IOBinding`                                                  | binding factory  | creates an `IOBinding` for the session           |
|  [10]   | `InferenceSession.end_profiling()`                                                            | profiling        | flushes the profiling trace and returns its path |

[ENTRYPOINT_SCOPE]: value construction and runtime queries
- rail: model

| [INDEX] | [SURFACE]                                                                             | [ENTRY_FAMILY] | [CAPABILITY]                                      |
| :-----: | :------------------------------------------------------------------------------------ | :------------- | :------------------------------------------------ |
|   [1]   | `OrtValue.ortvalue_from_numpy(arr, device_type, device_id)`                           | value build    | builds a native `OrtValue` from a NumPy array     |
|   [2]   | `OrtValue.ortvalue_from_shape_and_type(shape, element_type, ...)`                     | value build    | allocates an uninitialized device value           |
|   [3]   | `OrtValue.numpy()`                                                                    | value read     | copies a native value back to a NumPy array       |
|   [4]   | `IOBinding.bind_input(name, device_type, device_id, element_type, shape, buffer_ptr)` | binding        | binds a device input buffer                       |
|   [5]   | `IOBinding.bind_output(name, ...)` / `copy_outputs_to_cpu()`                          | binding        | binds outputs and retrieves results to CPU        |
|   [6]   | `get_available_providers()` / `get_all_providers()`                                   | provider query | lists installed and known execution providers     |
|   [7]   | `get_device()`                                                                        | device query   | returns the build's primary device string         |
|   [8]   | `set_default_logger_severity(level)`                                                  | logging        | sets the global runtime log severity              |
|   [9]   | `ModelCompiler(sess_options, input_model_path_or_bytes, ...).compile_to_file(...)`    | aot compile    | compiles a model to an optimized on-disk artifact |

## [4]-[IMPLEMENTATION_LAW]

[SESSION_TOPOLOGY]:
- `InferenceSession(path, providers=[...])` loads and optimizes the model; provider order is preference order, and `get_providers()` reports the providers actually assigned.
- `run(output_names, input_feed)` takes `output_names=None` to fetch all outputs and a `{input_name: np.ndarray}` feed; it returns a list of NumPy arrays aligned to `output_names`.
- `get_inputs()`/`get_outputs()` return `NodeArg` objects whose `name`, `type`, and `shape` are the model signature checked before a sample run.
- `SessionOptions` sets `graph_optimization_level`, `intra_op_num_threads`, `inter_op_num_threads`, and `execution_mode`; `RunOptions` carries per-run logging and a `terminate` flag.
- `IOBinding` and `OrtValue` keep tensors on-device across runs; `run_with_iobinding` avoids host copies for GPU/accelerator providers.

[STUDY_ROUTING]:
- A model asset is loaded into an `InferenceSession`, its `NodeArg` signatures are read, and a sample `run` confirms executable, well-shaped output before graduation.
- The model-asset receipt captures `get_available_providers()`, the input/output signatures, and the sample-run result.
- onnxruntime is the offline runtime-check half of the model-asset rail, paired with the `onnx` structural half; production model execution, provider policy, and benchmark authority stay in `Rasm.Compute`.

[RAIL_LAW]:
- Package: `onnxruntime`
- Owns: offline ONNX inference-session runtime checking for the model-asset rail
- Accept: a model asset that loads and produces well-shaped output through an `InferenceSession` sample run with captured provider and signature receipts
- Reject: production inference claims; provider selection hidden inside helpers; wrapper-renames of the session API
