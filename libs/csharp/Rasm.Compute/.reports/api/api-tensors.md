# [RASM_COMPUTE_API_TENSORS]

`System.Numerics.Tensors` supplies vectorized tensor primitives over spans for measured numeric execution.

## [1]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `System.Numerics.Tensors`
- package: `System.Numerics.Tensors`
- assembly: `System.Numerics.Tensors`
- namespace: `System.Numerics.Tensors`
- asset: runtime library
- rail: tensor

## [2]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: tensor family
- rail: tensor

| [INDEX] | [SYMBOL]                | [PACKAGE_ROLE]    | [CAPABILITY]            |
| :-----: | :---------------------- | :---------------- | :---------------------- |
|   [1]   | `TensorPrimitives`      | primitive surface | anchors tensor contract |
|   [2]   | `Tensor<T>`             | tensor value      | bounds payload shape    |
|   [3]   | `ReadOnlyTensorSpan<T>` | tensor span       | bounds payload shape    |
|   [4]   | `TensorSpan<T>`         | tensor span       | bounds payload shape    |

## [3]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: tensor operations
- rail: tensor

| [INDEX] | [SURFACE]          | [CALL_SHAPE]         | [CAPABILITY]         |
| :-----: | :----------------- | :------------------- | :------------------- |
|   [1]   | `Add`              | numeric primitive    | adds vectors         |
|   [2]   | `Subtract`         | numeric primitive    | subtracts vectors    |
|   [3]   | `Multiply`         | numeric primitive    | multiplies vectors   |
|   [4]   | `Divide`           | numeric primitive    | divides vectors      |
|   [5]   | `Dot`              | numeric primitive    | computes dot product |
|   [6]   | `CosineSimilarity` | similarity primitive | compares direction   |
|   [7]   | `Distance`         | distance primitive   | computes distance    |
|   [8]   | `Norm`             | norm primitive       | computes magnitude   |
|   [9]   | `Sum`              | sum primitive        | reduces vector       |
|  [10]   | `SoftMax`          | activation primitive | normalizes logits    |

## [4]-[IMPLEMENTATION_LAW]

[RAIL_LAW]:
- Package: `System.Numerics.Tensors`
- Owns: numeric tensor primitives
- Accept: tensor work emits equivalence receipts
- Reject: handwritten SIMD loops
