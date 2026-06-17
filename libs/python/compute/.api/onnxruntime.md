# [PY_COMPUTE_API_ONNXRUNTIME]

`onnxruntime` supplies the inference-session runtime that loads and runs a validated ONNX model for the compute model-asset rail. The package owner runs a model asset against sample inputs to confirm it executes and produces well-shaped outputs before graduation to the C# `Rasm.Compute` runtime; it never re-implements the runtime onnxruntime owns. The distribution is marker-gated `python_version<'3.15'` and is absent from the cp315 lock; member spellings are uncaptured pending a reflectable install (see UN_REFLECTED).

## [1]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `onnxruntime`
- package: `onnxruntime`
- import: `onnxruntime`
- owner: `compute`
- rail: model
- installed: ABSENT on cp315 (manifest pin `onnxruntime>=1.27.0; python_version<'3.15'`; no cp315 wheel — manifest gaps 1+2)
- capability: ONNX inference runtime — session load, typed input/output binding, run execution, model metadata, and execution-provider selection

## [2]-[CAPTURE]

[PUBLIC_TYPE_SCOPE]: session owners
- rail: model

| [INDEX] | [SYMBOL]                       | [PACKAGE_ROLE] | [CAPABILITY]                              |
| :-----: | :----------------------------- | :------------- | :---------------------------------------- |
|   [1]   | `onnxruntime.InferenceSession` | session root   | loads and runs a model                    |
|   [2]   | `onnxruntime.SessionOptions`   | session policy | configures graph optimization and threads |
|   [3]   | `onnxruntime.RunOptions`       | run policy     | configures a single inference run         |
|   [4]   | `onnxruntime.NodeArg`          | io descriptor  | named input/output with type and shape    |
|   [5]   | `onnxruntime.OrtValue`         | value carrier  | native typed tensor value                 |

[ENTRYPOINTS]:
- UN_REFLECTED: exact method spellings (`InferenceSession.run`, `InferenceSession.get_inputs`, `InferenceSession.get_outputs`, `get_providers`) and verified signatures require a reflectable install to capture; type names above are stable package facts, not reflected members.

[IMPLEMENTATION_LAW]:
- validation: a model asset is loaded into an `InferenceSession`, its input/output `NodeArg` signatures are read, and a sample run confirms executable, well-shaped output before graduation.
- evidence: the model-asset receipt captures the available execution providers, the input/output signatures, and the sample-run result.
- boundary: onnxruntime is the offline runtime-check half of the model-asset rail (paired with the `onnx` structural half); production model execution, provider policy, and benchmark authority stay in `Rasm.Compute`.

## [3]-[LOCAL_ADMISSION]

[RAIL_LAW]:
- Package: `onnxruntime`
- Owns: offline ONNX inference-session runtime checking for the model-asset rail
- Accept: a model asset that loads and produces well-shaped output through an `InferenceSession` sample run
- Reject: production inference claims; provider selection hidden inside helpers; wrapper-renames of the session API
