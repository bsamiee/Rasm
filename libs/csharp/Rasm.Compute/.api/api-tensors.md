# [RASM_COMPUTE_API_TENSORS]

`System.Numerics.Tensors` supplies tensor owners, tensor spans, dimension
spans, native-sized indexing, marshalling, and vectorized numeric primitives
for measured Compute execution. The substrate
canonical member catalog is `libs/csharp/.api/api-tensors.md`; this overlay carries
only the Compute delta — the dispatch-table binding, device-crossing, and absent-
operator law the `Tensor` pages compose.

## [01]-[SUBSTRATE_CANONICAL]

[SUBSTRATE_CANONICAL]: `libs/csharp/.api/api-tensors.md`
- the tensor-shape/indexing/marshalling type rosters and the construction, shape-edit, primitive, reduction, bitwise, and conversion call-shape tables live on the substrate catalog — this overlay never re-states them
- rail: tensor

## [02]-[COMPUTE_BINDINGS]

[COMPUTE_BINDINGS]:
- every `Tensor/dispatch#KERNEL_DISPATCH` span row binds the `TensorPrimitives` member matching its Pascal-cased `TensorOpFamily` key into a `FrozenDictionary<TensorOpFamily, …Kernel<T>>` delegate table — kernel selection is a table read, never a per-call switch.
- elementwise operators write into a caller-supplied `Span<T>` destination; reductions return a scalar `T`; predicate masks write `Span<bool>` with `*All`/`*Any` aggregate forms — the destination discipline every dispatch row inherits.
- `Tensor<T>.GetPinnableReference`/`TensorMarshal.GetReference` root the CPU/WebGPU/ONNX crossings; `NIndex`/`NRange` own native-sized indexing at the residency boundary.

## [03]-[IMPLEMENTATION_LAW]


[TENSOR_SHAPES]:
- namespace: `System.Numerics.Tensors`
- tensor root: `Tensor<T>` with `Tensor` as the static factory and operation surface
- span root: tensor spans and dimension spans
- index root: `NIndex` and `NRange` in `System.Buffers`
- marshal root: `TensorMarshal` in `System.Runtime.InteropServices`

[NUMERIC_PRIMITIVES]:
- operation root: `TensorPrimitives`
- operation families: arithmetic, fused, interpolation, reciprocal, reduction, aggregation, extrema, sign, activation, conversion, trigonometric, exponential, logarithmic, bitwise, population-count, rotation, predicate-mask
- generic-math constraints: operators are generic over `T` bound by `INumberBase`, `IFloatingPointIeee754`, `IRootFunctions`, `IBinaryInteger`, `IBitwiseOperators`, or `IShiftOperators` as the family requires; integer-only families reject floating element types at the constraint
- destination rule: elementwise operators write into a caller-supplied `Span<T>` destination; reductions return a scalar `T`; predicate masks write `Span<bool>` with `*All` / `*Any` aggregate forms
- memory rule: primitives operate over spans and tensor spans before package-local wrappers
- benchmark rule: primitive selection requires measured receipts for hot paths

[ABSENT_OPERATORS]:
- `TensorPrimitives` exposes no `Normalize` operator; vector normalization composes from `Norm` (or `SumOfSquares` + `Sqrt`) followed by `Divide` against the reduced magnitude
- a normalization owner row that names a single `TensorPrimitives.Normalize` call is unresolvable and stays SPIKE until expressed as the `Norm`/`Divide` composition

[LOCAL_ADMISSION]:
- Compute tensor lanes use package tensor shapes and primitives as first-class execution material.
- Tensor operations stay rail-owned and cannot become loose numeric helpers.
- Shape, rank, stride, slicing, and conversion rules are explicit execution policy.
- Model and vector rails can consume tensor spans without redefining tensor ownership.

[INTEGRATION_STACKING]:
- `TensorPrimitives` is the kernel substrate for `Tensor/dispatch#KERNEL_DISPATCH`: every span row binds the `TensorPrimitives` member matching its Pascal-cased `TensorOpFamily` key into a `FrozenDictionary<TensorOpFamily, …Kernel<T>>` delegate table, and the `Activations<T>` author-folds (`ReLU`/`Gelu`/`SiLU`/`LogSoftMax`) compose `TensorPrimitives.Clamp`/`Sigmoid`/`Multiply`/`MultiplyAdd`/`Tanh`/`Max`/`Exp`/`Sum`/`Subtract` — never a per-element loop and never a fabricated `TensorPrimitives.Relu` phantom.
- The matrix family (`MatMul`, `Conv1D`/`2D`/`3D`) holds NO `TensorPrimitives` member: `Tensor/dispatch#KERNEL_DISPATCH` `Map` resolves these through the `Tensor/factor#KERNEL_LOWERING` GEMM/im2col-GEMM lowering or, under the device residency gate, the `Tensor/dispatch#DEVICE_KERNELS` WGSL `Silk.NET.WebGPU` `ComputePipeline` — a tensor span feeds a WGPU storage `Buffer` and a `Tensor<T>.GetPinnableReference`/`TensorMarshal.GetReference` root admits to an ORT device value through `Tensor/residency#ORT_BRIDGE` `OrtValue.CreateTensorValueFromSystemNumericsTensorObject<T>(Tensor<T>)`, so the same `Tensor<T>` crosses the CPU `TensorPrimitives`, the WebGPU compute (`api-silk-webgpu.md`), and the ONNX device boundary with no parallel tensor type. `OrtValue.CreateTensorValueFromSystemNumericsTensorObject<T>` and the sibling C-data residency members (`CreateTensorValueWithData`/`CreateAllocatedTensorValue`/`BindOutputToDevice`) are owned by `api-onnxruntime.md` — a forward cross-catalog dependency this note tracks.
- `Tensor.FillGaussianNormalDistribution`/`FillUniformDistribution` are the equivalence-sampler fillers `Tensor/dispatch#EQUIVALENCE_INTEROP` `EquivalenceLaw.Prove` calls (a hand-rolled sample-RNG loop is the deleted form); `Tensor.FilteredUpdate(in TensorSpan<T>, in ReadOnlyTensorSpan<bool>, in ReadOnlyTensorSpan<T>)` is the predicate-masked-write the `Mask`/`MaskedWrite` row binds (the unconditional `SetSlice` region overwrite that ignores the mask is the deleted form); `Tensor<T>.ToDenseTensor` densifies a permuted/sliced backing once before the `Pool` flat-window walk over a `GetDimensionSpan` cursor.

[RAIL_LAW]:
- Package: `System.Numerics.Tensors` (`10.0.9`, MIT)
- Owns: tensors, tensor spans, numeric primitives
- Accept: measured tensor execution; `TensorPrimitives` delegate-table binding into the dispatch kernel surface; `Tensor<T>` as the one carrier crossing CPU/WebGPU/ONNX boundaries via `TensorMarshal.GetReference`/`GetPinnableReference`
- Reject: bespoke tensor wrappers; a `DeviceTensor`/`GpuTensor` parallel type (device-ness is the `Tensor/residency#ORT_BRIDGE` residency discriminant); a single-call `TensorPrimitives.Normalize` row (it has no such member — compose `Norm` then `Divide`)
