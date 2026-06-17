# [PY_COMPUTE_API_ONNX]

`onnx` supplies the open model intermediate-representation: the graph protobuf schema, model construction and loading, shape inference, and structural checking for the compute model-asset rail. The package owner validates the structure and metadata of a model asset before it graduates to the C# `Rasm.Compute` runtime; it never re-implements the IR onnx owns. The distribution is marker-gated `python_version<'3.15'` and is absent from the cp315 lock; member spellings are uncaptured pending a reflectable install (see UN_REFLECTED).

## [1]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `onnx`
- package: `onnx`
- import: `onnx`; submodules `onnx.checker`, `onnx.shape_inference`, `onnx.helper`
- owner: `compute`
- rail: model
- installed: ABSENT on cp315 (manifest pin `onnx>=1.22.0; python_version<'3.15'`; no cp315 wheel — manifest gaps 1+2)
- capability: ONNX model IR — graph/tensor protobuf schema, model load/save, structural checking, and shape inference

## [2]-[CAPTURE]

[PUBLIC_TYPE_SCOPE]: model-IR owners
- rail: model

| [INDEX] | [SYMBOL]               | [PACKAGE_ROLE]         | [CAPABILITY]                                |
| :-----: | :--------------------- | :--------------------- | :------------------------------------------ |
|   [1]   | `onnx.ModelProto`      | model message          | full model protobuf root                    |
|   [2]   | `onnx.GraphProto`      | graph message          | the model computation graph                 |
|   [3]   | `onnx.TensorProto`     | tensor message         | initializer/constant tensor with dtype enum |
|   [4]   | `onnx.checker`         | validation submodule   | structural model checking                   |
|   [5]   | `onnx.shape_inference` | inference submodule    | static shape propagation                    |
|   [6]   | `onnx.helper`          | construction submodule | node/graph/tensor builders                  |

[ENTRYPOINTS]:
- UN_REFLECTED: exact callable spellings (`onnx.load`, `onnx.save`, `onnx.checker.check_model`, `onnx.shape_inference.infer_shapes`) and verified signatures require a reflectable install to capture; type/submodule names above are stable package facts, not reflected members.

[IMPLEMENTATION_LAW]:
- validation: a model asset is loaded as `ModelProto`, structurally checked through `onnx.checker`, and shape-inferred through `onnx.shape_inference` before graduation.
- evidence: the model-asset receipt captures the opset, the graph input/output signatures, and the check result.
- boundary: onnx validation is the offline structural half of the model-asset rail; runtime inference is the `onnxruntime` half, and product model execution stays in `Rasm.Compute`.

## [3]-[LOCAL_ADMISSION]

[RAIL_LAW]:
- Package: `onnx`
- Owns: offline ONNX model IR construction, loading, structural checking, and shape inference for the model-asset rail
- Accept: a model asset validated through `onnx.checker` with a captured opset and signature receipt
- Reject: hand-parsed protobuf; wrapper-renames of the checker/inference calls; runtime inference claims onnxruntime owns
