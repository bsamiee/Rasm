# [PY_COMPUTE_API_JAXTYPING]

`jaxtyping` supplies shape-and-dtype array annotations (`Float[Array, "n m"]`, `Int`, `Bool`, `Shaped`, and the wider dtype ladder) over JAX arrays, numpy arrays, and any Array-API operand, plus the `PyTree[T]` structure type and the `jaxtyped` decorator that runtime-checks the annotations through a pluggable typechecker. Compute admits it for exactly the gated-carrier discipline: the x64-gated JAX carriers and the process-lane spec payloads annotate their array operands with shape/dtype contracts checked through `jaxtyped(typechecker=beartype(conf=FAULT_CONF))` — the existing beartype fence, so a rank/dtype breach rails at the boundary beside the finiteness refinement rather than surfacing as a mid-solve XLA shape error. A bare `object`/untyped-`Array` operand annotation on a JAX-gated public entry is the deleted form the annotations replace.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `jaxtyping`
- package: `jaxtyping` (MIT, pure Python, no compiled extension)
- import: `jaxtyping`
- owner: `compute`
- rail: array shape/dtype contracts
- manifest: `[COMPUTE]` under the `python_version<'3.15'` marker of the JAX-stack band it annotates (the same gate as `jax`/`equinox`/`lineax`); import sites are the gated carriers, so the marker never strands a runtime-floor page
- consumer: `solvers/linear.md` (`LinearEngine.residual` — the first gated-carrier fence), `solvers/differential.md` (`_diffrax_receipt` under the `PyTree[Float[Array, "..."]]` `Pytree` state contract), the sibling gated carriers (`NonlinearEngine`/`DiffEngine`/`QuadEngine`) and process-lane spec payloads as their contracts land
- capability: per-argument shape grammars (fixed axes, named symbolic axes shared across arguments, variadic `...`, broadcast `#` and optional `*` modifiers), dtype-classed array annotations, pytree structure typing, and runtime enforcement through an external typechecker — annotation-driven, zero runtime cost when unchecked

## [02]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: annotations and the checking weave
- rail: array shape/dtype contracts

| [INDEX] | [SURFACE]                                         | [ENTRY_FAMILY] | [CAPABILITY]                                                   |
| :-----: | :------------------------------------------------ | :------------- | :------------------------------------------------------------- |
|  [01]   | `Float[Array, "..."]` / `Float[Array, "n m"]`     | dtype class    | floating-point array of the declared shape grammar (workhorse) |
|  [02]   | `Int[...]` / `Bool[...]` / `Shaped[...]`          | dtype class    | integer, boolean, and any-dtype shape-checked arrays           |
|  [03]   | `Complex[...]` / `Num[...]`                       | dtype class    | complex and any-number dtype-classed arrays                    |
|  [04]   | `Real[...]` / `Inexact[...]`                      | dtype class    | real and floating/complex dtype-classed arrays                 |
|  [05]   | `Array`                                           | array type     | array type the dtype classes subscript (`np.ndarray` too)      |
|  [06]   | `ArrayLike` / `Scalar` / `PRNGKeyArray`           | array type     | coercible operands, 0-d scalars, typed JAX PRNG key            |
|  [07]   | `PyTree[T]` / `PyTree[Float[Array, "..."]]`       | structure type | pytree-of-leaves structure contract on spec payloads           |
|  [08]   | `jaxtyped(typechecker=beartype(conf=FAULT_CONF))` | decorator      | the runtime weave; shared axis names bind across arguments     |
|  [09]   | `TypeCheckError`                                  | failure        | annotation-breach raise the `boundary` fence converts          |

## [03]-[IMPLEMENTATION_LAW]

[RAIL_LAW]:
- Package: `jaxtyping`
- Owns (as admitted): shape/dtype array contracts on the x64-gated carriers and the process-lane crossing operands, checked through the existing beartype fence
- Accept: `jaxtyped(typechecker=beartype(conf=FAULT_CONF))` on a gated carrier's public array-consuming entry; named symbolic axes shared across arguments so a rank/shape mismatch rails at the boundary; `PyTree[...]` on a spec payload that crosses the process lane
- Reject: a second typechecker beside the one shared `FAULT_CONF` beartype fence; `install_import_hook` (module-wide hooking — compute annotates the gated entries explicitly); a bare `object`/untyped-`Array` operand annotation on a JAX-gated public entry (the deleted form); a shape assertion hand-rolled inside a solve body where the boundary annotation owns it
