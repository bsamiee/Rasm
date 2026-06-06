# [THINKTECTURE_UNIONS]

[IMPORTANT] Prefer named regular unions for domain variants. Use ad-hoc unions only for compact local alternatives.

## [1][UNION_TYPES]

| [INDEX] | [SURFACE]                    | [USE]                                                         |
| :-----: | ---------------------------- | ------------------------------------------------------------- |
|   [1]   | `[Union]`                    | Named record cases with domain meaning and payload.           |
|   [2]   | `[Union<T1,...>]`            | Type-list cases where type identity is enough.                |
|   [3]   | `[Union<TypeParamRef1,...>]` | Generic member references.                                    |
|   [4]   | `[AdHocUnion(typeof(...))]`  | Local compact choice across up to verified generated members. |

## [2][DISPATCH]

Use generated `Switch` and `Map` directly. Prefer state-threaded overloads and `static` lambdas where they remove closure allocations. Partial overload generation uses `SwitchMethods` / `MapMethods` and `[UnionSwitchMapOverload]` — default to exhaustive dispatch.

State-threaded dispatch: `[Union(SwitchMapStateParameterName = "…")]` — see `union-attributes.md` §1.

Local union SelfOp policy: see `rasm.md` §4.

## [3][BOUNDARIES]

- Keep union serialization policy explicit at the boundary.
- Convert generated validation once into LanguageExt rails.
- Do not export duplicate DTOs for union cases unless external protocol demands them.
- Do not use ad-hoc unions as a public replacement for named domain variants.
