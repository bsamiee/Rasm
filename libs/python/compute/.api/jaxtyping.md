# [PY_COMPUTE_API_JAXTYPING]

`jaxtyping` mints shape-and-dtype array annotations over JAX, NumPy, and any Array-API operand — the dtype ladder (`Float`, `Int`, `Bool`, `Shaped`, and wider), the `PyTree[T]` structure type, and the `jaxtyped` decorator that runtime-checks them through a pluggable typechecker. Compute binds it to the x64-gated JAX carriers and the process-lane spec payloads: their array operands carry shape/dtype contracts checked through the shared beartype fence, so a rank or dtype breach rails at the boundary beside the finiteness refinement instead of surfacing as a mid-solve XLA shape error.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `jaxtyping`
- package: `jaxtyping` (MIT, pure Python, no compiled extension)
- import: `jaxtyping`
- owner: `compute`
- rail: array shape/dtype contracts
- capability: per-argument shape grammars (fixed axes, named symbolic axes shared across arguments, variadic `...`, broadcast `#`, optional `*`), dtype-classed array annotations, pytree structure typing, and runtime enforcement through an external typechecker — annotation-driven, zero runtime cost unchecked

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: dtype-classed array annotations, array operand types, the pytree structure contract, and the breach failure
- rail: array shape/dtype contracts

| [INDEX] | [SYMBOL]                                  | [TYPE_FAMILY]  | [CAPABILITY]                                                   |
| :-----: | :---------------------------------------- | :------------- | :------------------------------------------------------------- |
|  [01]   | `Float[Array, "n m"]`                     | dtype class    | floating-point array of the declared shape grammar (workhorse) |
|  [02]   | `Int` / `Bool` / `Shaped`                 | dtype class    | integer, boolean, and any-dtype shape-checked arrays           |
|  [03]   | `Complex` / `Num`                         | dtype class    | complex and any-number dtype-classed arrays                    |
|  [04]   | `Real` / `Inexact`                        | dtype class    | real and floating/complex dtype-classed arrays                 |
|  [05]   | `Array`                                   | array type     | array type the dtype classes subscript (`np.ndarray` too)      |
|  [06]   | `ArrayLike` / `Scalar` / `PRNGKeyArray`   | array type     | coercible operands, 0-d scalars, typed JAX PRNG key            |
|  [07]   | `PyTree[T]` / `PyTree[Float[Array, ...]]` | structure type | pytree-of-leaves structure contract on spec payloads           |
|  [08]   | `TypeCheckError`                          | error type     | annotation breach the `boundary` fence converts to its rail    |

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: the runtime checking weave
- rail: array shape/dtype contracts

| [INDEX] | [SURFACE]                      | [SHAPE]   | [CAPABILITY]                                                                       |
| :-----: | :----------------------------- | :-------- | :--------------------------------------------------------------------------------- |
|  [01]   | `jaxtyped(fn, *, typechecker)` | decorator | binds the check over one call; shared axis names fix ranks/shapes across arguments |

## [04]-[IMPLEMENTATION_LAW]

[TOPOLOGY]:
- Annotations stay inert type metadata until `jaxtyped` weaves a typechecker over the call; shared symbolic axis names then bind ranks and shapes across every argument in one call scope, so a mismatch rails at entry rather than mid-solve.

[STACKING]:
- `beartype`(`../../.api/beartype.md`): `jaxtyped(typechecker=beartype(conf=FAULT_CONF))` folds the shape/dtype check into the one `FAULT_CONF` boundary fence, so a breach rails on the same typed failure as the finiteness refinement.
- `jax`(`jax.md`): the dtype classes subscript `jax.Array`; `PRNGKeyArray` resolves to the `jax.random.key` shape and `Scalar` to `Shaped[jax.Array, ""]`.
- `equinox`(`equinox.md`): an `equinox.Module` is a valid pytree leaf-carrier, so `PyTree[Float[Array, ...]]` types an Equinox-carried state payload crossing the process lane.
- within-lib: the x64-gated carriers (`LinearEngine`, `NonlinearEngine`, `DiffEngine`, `QuadEngine`) and process-lane spec payloads annotate every public array operand, and `jaxtyped` weaves the check only at those gated entries.

[LOCAL_ADMISSION]:
- A JAX-gated public array entry carries a dtype-classed shape annotation with named symbolic axes shared across arguments; a spec payload crossing the process lane carries `PyTree[...]`; the check weaves through `jaxtyped(typechecker=beartype(conf=FAULT_CONF))`.

[RAIL_LAW]:
- Package: `jaxtyping`
- Owns: shape/dtype array contracts on the x64-gated carriers and process-lane crossing operands, checked through the one beartype fence
- Accept: `jaxtyped(typechecker=beartype(conf=FAULT_CONF))` on a gated carrier's public array entry; named symbolic axes shared across arguments; `PyTree[...]` on a process-lane spec payload
- Reject: a second typechecker beside the shared `FAULT_CONF` beartype fence; `install_import_hook` module-wide hooking where explicit gated-entry annotation owns the weave; a shape assertion hand-rolled in a solve body where the boundary annotation owns it
