# [PY_COMPUTE_API_ONNXRUNTIME]

`onnxruntime` owns offline inference-session checking of a validated ONNX model for the compute model-asset rail: it loads a `ModelProto` into an `InferenceSession`, reads the `NodeArg` signatures, and runs sample inputs through `run` to confirm well-shaped output before graduation to the C# `Rasm.Compute` runtime, which keeps production execution, provider policy, and benchmark authority.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `onnxruntime`
- package: `onnxruntime` (MIT)
- module: `onnxruntime`
- rail: model
- capability: offline ONNX inference-session checking — session load over ordered providers, zero-copy IO binding, sample `run`, and AOT EP-context compilation

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: session and value types; `[SYMBOL]` resolves as `onnxruntime.<name>`.

| [INDEX] | [SYMBOL]           | [TYPE_FAMILY] | [CAPABILITY]                                                                              |
| :-----: | :----------------- | :------------ | :---------------------------------------------------------------------------------------- |
|  [01]   | `InferenceSession` | session root  | loads/runs a model over ordered providers from a path, bytes, or `ModelProto`             |
|  [02]   | `SessionOptions`   | build cfg     | optimization level, thread pools, profiling, config + custom-op entries                   |
|  [03]   | `RunOptions`       | run policy    | per-run `log_severity_level`, `terminate`, `add_run_config_entry(k,v)`, `active_adapters` |
|  [04]   | `NodeArg`          | io node       | named io — `name`, `type` (ONNX type string), `shape` (`None`/`str` symbolic dims)        |
|  [05]   | `OrtValue`         | value io      | `numpy()`, `shape()`, `data_type()`, `device_name()`, `is_tensor()`, `element_type()`     |
|  [06]   | `IOBinding`        | io binder     | binds pre-allocated device inputs/outputs for zero host-copy runs                         |
|  [07]   | `ModelMetadata`    | metadata      | `producer_name`, `domain`, `version`, `graph_name`, `description`, `custom_metadata_map`  |
|  [08]   | `ModelCompiler`    | compiler      | compiles a model to an EP-specific optimized artifact (EP-context cache)                  |
|  [09]   | `OrtDevice`        | device        | device handle for `IOBinding`/`OrtValue` placement                                        |
|  [10]   | `OrtMemoryInfo`    | mem info      | allocator/memory descriptor for device placement                                          |
|  [11]   | `SparseTensor`     | sparse        | COO/CSR/block-sparse value (`sparse_coo_from_numpy`, `sparse_csr_from_numpy`)             |

- `SessionOptions`: `graph_optimization_level`, `intra_op_num_threads`, `inter_op_num_threads`, `execution_mode`, `enable_profiling`, `optimized_model_filepath`, `add_session_config_entry(k,v)`, `register_custom_ops_library(path)`, `add_free_dimension_override_by_name`.

[PUBLIC_TYPE_SCOPE]: option enums

| [INDEX] | [SYMBOL]                          | [VALUES]                                                                                            |
| :-----: | :-------------------------------- | :-------------------------------------------------------------------------------------------------- |
|  [01]   | `GraphOptimizationLevel`          | `ORT_DISABLE_ALL`, `ORT_ENABLE_BASIC`, `ORT_ENABLE_EXTENDED`, `ORT_ENABLE_LAYOUT`, `ORT_ENABLE_ALL` |
|  [02]   | `ExecutionMode`                   | `ORT_SEQUENTIAL`, `ORT_PARALLEL`                                                                    |
|  [03]   | `ExecutionOrder`                  | priority versus default node ordering                                                               |
|  [04]   | `OrtAllocatorType` / `OrtMemType` | allocator kind and memory-type selectors for IO binding                                             |

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: session lifecycle and inspection on `InferenceSession`; ctor `InferenceSession(path_or_bytes, *, sess_options, providers, provider_options)`.

| [INDEX] | [SURFACE]                                                   | [SHAPE]  | [CAPABILITY]                                |
| :-----: | :---------------------------------------------------------- | :------- | :------------------------------------------ |
|  [01]   | `InferenceSession(path_or_bytes, ...)`                      | ctor     | loads a model, ordering execution providers |
|  [02]   | `run(output_names, input_feed, run_options=None)`           | instance | runs the model on a name→array feed dict    |
|  [03]   | `run_with_ort_values(output_names, feed, run_options=None)` | instance | runs on native `OrtValue` inputs            |
|  [04]   | `run_with_iobinding(iobinding, run_options=None)`           | instance | runs against a pre-bound `IOBinding`        |
|  [05]   | `get_inputs()` / `get_outputs()`                            | instance | returns the input/output `NodeArg` lists    |
|  [06]   | `get_overridable_initializers()`                            | instance | overridable-initializer `NodeArg` list      |
|  [07]   | `get_providers()` / `set_providers(providers)`              | instance | reads/sets the active execution providers   |
|  [08]   | `get_modelmeta() -> ModelMetadata`                          | instance | returns the embedded model metadata         |
|  [09]   | `io_binding() -> IOBinding`                                 | instance | creates an `IOBinding` for the session      |
|  [10]   | `end_profiling()`                                           | instance | flushes profiling; returns the trace path   |

[ENTRYPOINT_SCOPE]: value construction, provider/device queries, and AOT compile via `ModelCompiler(session_options, input_model, *, embed_compiled_data_into_model).compile_to_file(output_path)`.

| [INDEX] | [SURFACE]                                                         | [SHAPE]  | [CAPABILITY]                                     |
| :-----: | :---------------------------------------------------------------- | :------- | :----------------------------------------------- |
|  [01]   | `OrtValue.ortvalue_from_numpy(arr, device_type, device_id)`       | factory  | builds a native `OrtValue` from a NumPy array    |
|  [02]   | `OrtValue.ortvalue_from_shape_and_type(shape, element_type, ...)` | factory  | allocates an uninitialized device value          |
|  [03]   | `OrtValue.numpy()`                                                | instance | copies a native value back to a NumPy array      |
|  [04]   | `IOBinding.bind_input(name, device_type, device_id, ...)`         | instance | binds a device input buffer                      |
|  [05]   | `IOBinding.bind_output(name, ...)` / `copy_outputs_to_cpu()`      | instance | binds outputs and retrieves results to CPU       |
|  [06]   | `get_available_providers()` / `get_all_providers()`               | static   | lists installed vs all known execution providers |
|  [07]   | `get_device()`                                                    | static   | returns the build's primary device (`CPU`/`GPU`) |
|  [08]   | `set_default_logger_severity(level)`                              | static   | sets the global runtime log severity             |
|  [09]   | `set_default_logger_verbosity(level)`                             | static   | sets the global runtime log verbosity            |
|  [10]   | `preload_dlls(cuda=True, cudnn=True, msvc=True, directory=None)`  | static   | preloads CUDA/cuDNN before session creation      |
|  [11]   | `ModelCompiler(...).compile_to_file(output_path)`                 | instance | compiles to an EP-context on-disk artifact       |

## [04]-[IMPLEMENTATION_LAW]

[TOPOLOGY]:
- `InferenceSession(path, providers=[...])` loads and optimizes the model; provider order is preference order, and `get_providers()` reports the assigned providers — a requested EP silently falls back to CPU when unavailable.
- `get_inputs()`/`get_outputs()` return `NodeArg` objects carrying the model signature (`name`, `type`, `shape`) read before a sample run; `run(output_names=None, feed)` fetches all outputs, returning arrays aligned to `output_names`.
- `IOBinding` and `OrtValue` hold tensors on-device across runs, so `run_with_iobinding` skips the host copy for GPU/accelerator providers.

[STACKING]:
- `onnx`(`.api/onnx.md`) / `skl2onnx`(`.api/skl2onnx.md`) → onnxruntime: the runtime half consumes the exact `ModelProto` the structural half validated — `model.SerializeToString()` or the same path into `InferenceSession(...)`. A model passing `checker.check_model(full_check=True)` yet failing `InferenceSession` construction is a runtime-only fault (unsupported op/opset for the build) the structural check cannot catch.
- onnxruntime → `numpy`(`libs/python/.api/numpy.md`): `run(output_names, {name: np.ndarray})` is the canonical feed, input dtypes matching the `NodeArg.type` strings (`tensor(float)`, `tensor(int64)`) the session reports; `OrtValue.ortvalue_from_numpy(arr, 'cuda', 0)` is the zero-copy device path bypassing the host feed.
- within-lib: `providers=[('CUDAExecutionProvider', {'device_id': 0}), 'CPUExecutionProvider']` orders preference with per-EP option dicts, and the receipt records the assigned-not-requested list `get_providers()` returns.

[LOCAL_ADMISSION]:
- Model-asset receipt captures `get_available_providers()`, the `get_providers()` assignment, the input/output signatures, and the sample-run result — the offline runtime-check half beside the `onnx` structural half and the `skl2onnx` producer.

[RAIL_LAW]:
- Package: `onnxruntime`
- Owns: offline ONNX inference-session runtime checking for the model-asset rail — session load, provider selection with per-EP option dicts, IO binding, and the sample run
- Accept: the `onnx`-validated / `skl2onnx`-produced `ModelProto` loaded into an `InferenceSession`, producing well-shaped output from a sample `run`, with the `get_providers()` assignment and input/output signatures captured as the receipt
- Reject: production inference claims; provider selection hidden inside helpers; a requested EP assumed assigned without reading `get_providers()`; wrapper-renames of the session API
