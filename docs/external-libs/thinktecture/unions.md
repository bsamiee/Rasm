# [H1][THINKTECTURE_UNIONS]
>**Dictum:** *Unions make closed-world choice exhaustive and generated.*

<br>

[IMPORTANT] Prefer named regular unions for domain variants. Use ad-hoc unions only for compact local alternatives.

---
## [1][UNION_TYPES]
>**Dictum:** *Union shape follows how much meaning each case carries.*

<br>

| [INDEX] | [SURFACE] | [USE] |
| :-----: | --------- | ----- |
| [1] | `[Union]` | Named record cases with domain meaning and payload. |
| [2] | `[Union<T1,...>]` | Type-list cases where type identity is enough. |
| [3] | `[Union<TypeParamRef1,...>]` | Generic member references. |
| [4] | `[AdHocUnion(typeof(...))]` | Local compact choice across up to verified generated members. |

---
## [2][DISPATCH]
>**Dictum:** *Generated dispatch replaces visitors and repeated switch arms.*

<br>

Use generated `Switch` and `Map` directly. Prefer state-threaded overloads and `static` lambdas where they remove closure allocations. Partial overload generation is an explicit customization with `SwitchMapMethodsGeneration` and overload attributes; default Rasm guidance is exhaustive dispatch.

State-threaded dispatch: `[Union(SwitchMapStateParameterName = "…")]` — see `union-attributes.md` §1.

Rasm SelfOp policy: `[SkipUnionOps]` / `[GenerateUnionOps]` emit or skip per-case `Op SelfOp` — **not** Thinktecture union operators. Hand `operator +`/`|` may exist on separate types. See `union-attributes.md` §2.

---
## [3][BOUNDARIES]
>**Dictum:** *A union boundary chooses one wire model.*

<br>

- Keep union serialization policy explicit at the boundary.
- Convert generated validation once into LanguageExt rails.
- Do not export duplicate DTOs for union cases unless external protocol demands them.
- Do not use ad-hoc unions as a public replacement for named domain variants.
