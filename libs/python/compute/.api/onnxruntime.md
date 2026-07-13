# [PY_COMPUTE_API_ONNXRUNTIME]

`onnxruntime` supplies the inference-session runtime that loads and runs a validated ONNX model for the compute model-asset rail. The package owner loads a model asset into an `InferenceSession`, reads its `NodeArg` input/output signatures, and runs sample inputs through `run` to confirm well-shaped output before graduation to the C# `Rasm.Compute` runtime; it never re-implements the runtime onnxruntime owns.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `onnxruntime`
- package: `onnxruntime`
- import: `onnxruntime`
- owner: `compute`
- rail: model
- license: MIT
- capability: ONNX inference runtime — session load, typed input/output binding, run execution, zero-copy IO binding, model metadata, execution-provider selection with per-provider option dicts, custom-op libraries, ahead-of-time EP compilation, profiling, and sparse-tensor values

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: session and value types
- rail: model
- namespace: every `[SYMBOL]` is `onnxruntime.<name>`; the fat `[02]`/`[03]` member rosters follow the table.

| [INDEX] | [SYMBOL]           | [PACKAGE_ROLE] | [CAPABILITY]                                                                             |
| :-----: | :----------------- | :------------- | :--------------------------------------------------------------------------------------- |
|  [01]   | `InferenceSession` | session root   | loads/runs a model over ordered providers from a path, bytes, or `ModelProto`            |
|  [02]   | `SessionOptions`   | build cfg      | build/run tuning — optimization, thread pools, profiling, config + custom-op entries     |
|  [03]   | `RunOptions`       | run policy     | `log_severity_level`, `terminate`, `add_run_config_entry(k,v)`, `active_adapters`        |
|  [04]   | `NodeArg`          | io node        | named io: `name`, `type` (ONNX type string), `shape` (`None`/`str` symbolic dims)        |
|  [05]   | `OrtValue`         | value io       | `numpy()`, `shape()`, `data_type()`, `device_name()`, `is_tensor()`, `element_type()`    |
|  [06]   | `IOBinding`        | io binder      | binds pre-allocated device inputs/outputs for zero host-copy runs                        |
|  [07]   | `ModelMetadata`    | metadata       | `producer_name`, `domain`, `version`, `graph_name`, `description`, `custom_metadata_map` |
|  [08]   | `ModelCompiler`    | compiler       | compiles a model to an EP-specific optimized artifact (EP-context cache)                 |
|  [09]   | `OrtDevice`        | device         | device handle for `IOBinding`/`OrtValue` placement                                       |
|  [10]   | `OrtMemoryInfo`    | mem info       | allocator/memory descriptor for device placement                                         |
|  [11]   | `SparseTensor`     | sparse         | COO/CSR/block-sparse value (`sparse_coo_from_numpy`, `sparse_csr_from_numpy`)            |

- [02]-[SESSIONOPTIONS]: `graph_optimization_level`, `intra_op_num_threads`, `inter_op_num_threads`, `execution_mode`, `enable_profiling`, `optimized_model_filepath`, `add_session_config_entry(k,v)`, `register_custom_ops_library(path)`, `add_free_dimension_override_by_name`.
- [03]-[RUNOPTIONS]: `active_adapters` carries LoRA `AdapterFormat` adapters.

[PUBLIC_TYPE_SCOPE]: option enums
- rail: model

| [INDEX] | [SYMBOL]                          | [VALUES]                                                                                            |
| :-----: | :-------------------------------- | :-------------------------------------------------------------------------------------------------- |
|  [01]   | `GraphOptimizationLevel`          | `ORT_DISABLE_ALL`, `ORT_ENABLE_BASIC`, `ORT_ENABLE_EXTENDED`, `ORT_ENABLE_LAYOUT`, `ORT_ENABLE_ALL` |
|  [02]   | `ExecutionMode`                   | `ORT_SEQUENTIAL`, `ORT_PARALLEL`                                                                    |
|  [03]   | `ExecutionOrder`                  | priority versus default node ordering                                                               |
|  [04]   | `OrtAllocatorType` / `OrtMemType` | allocator kind and memory-type selectors for IO binding                                             |

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: session lifecycle and inspection
- rail: model
- receiver: `[SURFACE]` rows are `InferenceSession` instance methods; the constructor is `InferenceSession(path_or_bytes, sess_options=None, providers=None, provider_options=None)`.
- run: `run_with_ort_values(output_names, input_dict_ort_values, run_options=None)` runs on native `OrtValue` inputs.

| [INDEX] | [SURFACE]                                         | [ENTRY_FAMILY]   | [CAPABILITY]                                |
| :-----: | :------------------------------------------------ | :--------------- | :------------------------------------------ |
|  [01]   | `InferenceSession(path_or_bytes, ...)`            | session ctor     | loads a model, ordering execution providers |
|  [02]   | `run(output_names, input_feed, run_options=None)` | inference run    | runs the model on a name→array feed dict    |
|  [03]   | `run_with_ort_values(...)`                        | inference run    | runs on native `OrtValue` inputs            |
|  [04]   | `run_with_iobinding(iobinding, run_options=None)` | inference run    | runs against a pre-bound `IOBinding`        |
|  [05]   | `get_inputs()` / `get_outputs()`                  | signature query  | returns the input/output `NodeArg` lists    |
|  [06]   | `get_overridable_initializers()`                  | signature query  | overridable-initializer `NodeArg` list      |
|  [07]   | `get_providers()` / `set_providers(providers)`    | provider control | reads/sets the active execution providers   |
|  [08]   | `get_modelmeta() -> ModelMetadata`                | metadata query   | returns the embedded model metadata         |
|  [09]   | `io_binding() -> IOBinding`                       | binding factory  | creates an `IOBinding` for the session      |
|  [10]   | `end_profiling()`                                 | profiling        | flushes profiling; returns the trace path   |

[ENTRYPOINT_SCOPE]: value construction and runtime queries
- rail: model
- bind: `IOBinding.bind_input(name, device_type, device_id, element_type, shape, buffer_ptr)` binds a device input buffer.
- compile: `ModelCompiler(session_options, input_model_path_or_bytes, embed_compiled_data_into_model=False, ...).compile_to_file(output_path)` writes the EP-context artifact.
- providers: `get_available_providers()`/`get_all_providers()` return names like `CPUExecutionProvider`, `CUDAExecutionProvider`, `CoreMLExecutionProvider`, `TensorrtExecutionProvider`.

| [INDEX] | [SURFACE]                                                         | [ENTRY_FAMILY] | [CAPABILITY]                                     |
| :-----: | :---------------------------------------------------------------- | :------------- | :----------------------------------------------- |
|  [01]   | `OrtValue.ortvalue_from_numpy(arr, device_type, device_id)`       | value build    | builds a native `OrtValue` from a NumPy array    |
|  [02]   | `OrtValue.ortvalue_from_shape_and_type(shape, element_type, ...)` | value build    | allocates an uninitialized device value          |
|  [03]   | `OrtValue.numpy()`                                                | value read     | copies a native value back to a NumPy array      |
|  [04]   | `IOBinding.bind_input(...)`                                       | binding        | binds a device input buffer                      |
|  [05]   | `IOBinding.bind_output(name, ...)` / `copy_outputs_to_cpu()`      | binding        | binds outputs and retrieves results to CPU       |
|  [06]   | `get_available_providers()` / `get_all_providers()`               | provider query | lists installed vs all known execution providers |
|  [07]   | `get_device()`                                                    | device query   | returns the build's primary device (`CPU`/`GPU`) |
|  [08]   | `set_default_logger_severity(level)`                              | logging        | sets the global runtime log severity             |
|  [09]   | `set_default_logger_verbosity(level)`                             | logging        | sets the global runtime log verbosity            |
|  [10]   | `preload_dlls(cuda=True, cudnn=True, msvc=True, directory=None)`  | dll preload    | preloads CUDA/cuDNN before session creation      |
|  [11]   | `ModelCompiler(...).compile_to_file(output_path)`                 | aot compile    | compiles to an EP-context on-disk artifact       |

## [04]-[IMPLEMENTATION_LAW]

[SESSION_TOPOLOGY]:
- `InferenceSession(path, providers=[...])` loads and optimizes the model; provider order is preference order, and `get_providers()` reports the providers actually assigned.
- `run(output_names, input_feed)` takes `output_names=None` to fetch all outputs and a `{input_name: np.ndarray}` feed; it returns a list of NumPy arrays aligned to `output_names`.
- `get_inputs()`/`get_outputs()` return `NodeArg` objects whose `name`, `type`, and `shape` are the model signature checked before a sample run.
- `SessionOptions` sets `graph_optimization_level`, `intra_op_num_threads`, `inter_op_num_threads`, and `execution_mode`; `RunOptions` carries per-run logging and a `terminate` flag.
- `IOBinding` and `OrtValue` keep tensors on-device across runs; `run_with_iobinding` avoids host copies for GPU/accelerator providers.

[STACKING_TOPOLOGY]:
- `onnx`/`skl2onnx` → onnxruntime: the runtime half consumes the exact `ModelProto` the `onnx` structural half validated — pass `model.SerializeToString()` (or the same path) into `InferenceSession(...)`. A model that passes `onnx.checker.check_model(full_check=True)` but fails `InferenceSession` construction is a runtime-only fault (unsupported op/opset for the build) the structural check cannot catch, which is precisely why both halves run.
- onnxruntime → `numpy`: `run(output_names, {name: np.ndarray})` is the canonical feed; the input arrays' dtypes must match the `NodeArg.type` strings (`tensor(float)`, `tensor(int64)`, ...) the session reports, and the producer's `onnx` initializer dtypes. `OrtValue.ortvalue_from_numpy(arr, 'cuda', 0)` is the zero-copy device path that bypasses the host `np.ndarray` feed.
- Provider stacking: `providers=[('CUDAExecutionProvider', {'device_id': 0}), 'CPUExecutionProvider']` orders preference with per-EP option dicts; `get_providers()` reports what was actually assigned (a requested EP silently falls back to CPU if unavailable), so the receipt records the assigned-not-requested provider list.

[STUDY_ROUTING]:
- A model asset is loaded into an `InferenceSession`, its `NodeArg` signatures are read, and a sample `run` confirms executable, well-shaped output before graduation.
- The model-asset receipt captures `get_available_providers()`, the providers actually assigned by `get_providers()`, the input/output signatures, and the sample-run result.
- onnxruntime is the offline runtime-check half of the model-asset rail, paired with the `onnx` structural half and the `skl2onnx` producer; production model execution, provider policy, and benchmark authority stay in `Rasm.Compute`.

[RAIL_LAW]:
- Package: `onnxruntime`
- Owns: offline ONNX inference-session runtime checking for the model-asset rail — session load, provider selection with per-EP option dicts, IO binding, and the sample run
- Accept: the `onnx`-validated / `skl2onnx`-produced `ModelProto` loaded into an `InferenceSession`, producing well-shaped output from a sample `run`, with the assigned `get_providers()` list and input/output signatures captured as the receipt
- Reject: production inference claims; provider selection hidden inside helpers; assuming a requested EP was assigned without reading `get_providers()`; wrapper-renames of the session API
