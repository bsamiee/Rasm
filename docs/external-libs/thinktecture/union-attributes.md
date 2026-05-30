# [H1][THINKTECTURE_UNION_ATTRIBUTES]
>**Dictum:** *Union attributes control Thinktecture dispatch; Rasm attributes control SelfOp emission.*

<br>

[IMPORTANT] Rasm pins `Thinktecture.Runtime.Extensions` `10.2.0`. Thinktecture `[Union]` generates `.Switch()`/`.Map()` — **not** `operator +`/`|` in TT 10.2.0. Rasm `[SkipUnionOps]` / `[GenerateUnionOps]` are **Rasm.Domain** analyzer attributes — see `tools/cs-analyzer/Generators/UnionOpsGenerator.cs`.

---
## [1][THINKTECTURE_UNION_ATTRIBUTES]
>**Dictum:** *Generated dispatch replaces repeated switch arms.*

<br>

| [INDEX] | [ATTRIBUTE / PROPERTY] | [GENERATES] |
| :-----: | ---------------------- | ----------- |
| [1] | `[Union]` | Case types, `.Switch()`, `.Map()` |
| [2] | `[Union(SwitchMapStateParameterName = "ctx")]` | State-threaded `.Switch(ctx, case => …)` |
| [3] | `SwitchMapMethodsGeneration` + overload attrs | Partial custom switch overload set |
| [4] | `[UseDelegateFromConstructor]` | Delegate on SmartEnum items **or** union case partials |

Common `SwitchMapStateParameterName` values in production: `ctx`, `context`, `state`, `scope`, `viewport`, `runtime`, `settings`, `page`, `owner`.

Example (actual parameter name):

```csharp
[Union(SwitchMapStateParameterName = "context")]
public abstract partial record FileSheetEdit { /* cases */ }
```

(`FileSheetEdit` — `Rasm.Rhino/Exchange/Sheets.cs`.)

---
## [2][RASM_UNION_OPS_ATTRIBUTES]
>**Dictum:** *CSP0802 governs SelfOp emission — not Thinktecture union operators.*

<br>

Defined in `libs/csharp/Rasm/Domain/Validation.cs`. Enforced by CSP0802 in `Rasm.Domain` and `Rasm.Analysis` namespaces only.

| [INDEX] | [ATTRIBUTE] | [EFFECT] |
| :-----: | ----------- | -------- |
| [1] | `[GenerateUnionOps]` | Rasm source generator emits `internal static readonly Op SelfOp = Op.Of(name: nameof(Case))` per sealed case |
| [2] | `[SkipUnionOps]` | Opt out of CSP0802 SelfOp requirement; signals hand-composed semantics elsewhere |

**Not generated:** `operator +` or `operator |` on union types. Hand operators live on **separate structs/records** or plain types:

| [INDEX] | [TYPE] | [OPERATOR] | `[SkipUnionOps]` |
| :-----: | ------ | ---------- | ---------------- |
| [1] | `RepaintRequest`, GH `Subscription` | hand `\|` | yes |
| [2] | `FileOverride<T>` | hand `\|` | **no** — plain struct, not `[Union]` |
| [3] | `RedrawRequest`, Blocks `Subscription` (Rhino) | hand `\|` | **no** |
| [4] | `VectorField`, `ScalarField` | hand `+/-/ *` | **no** — `Rasm.Vectors` out of CSP0802 scope |

`[GenerateUnionOps]` production unions include: `Fault`, GH `CanvasOp`, `DocumentOp`, `WireOp`, `WireQuery`, `LayoutOp`, `EditorOp`, `ToolbarItem`, `DrawMark`.

---
## [3][WHEN_NOT_TO_USE_UNION]
>**Dictum:** *Generic or ref-struct constraints block Thinktecture union codegen.*

<br>

Use plain `abstract record` + manual `this switch` when:

- Generic over state (`PromptTransition<TState>`, `MotionSpec<TValue>`, `InteractionStep<TState>`, `GhUiRequest<T>`, …).
- `allows ref struct` conflicts with generated case shapes.

Document the escape hatch in the owning module.

---
## [4][RULES]
>**Dictum:** *Dispatch attributes are architecture, not decoration.*

<br>

- Read Rasm Skip/Generate before assuming SelfOp exists on union cases.
- Prefer `SwitchMapStateParameterName` over duplicating context in every case payload.
- Keep exhaustive dispatch: generated `.Switch` / `.Map` or manual total `switch`.
- Bridge Thinktecture validation once into LanguageExt rails — see `objects.md` §5.1.
