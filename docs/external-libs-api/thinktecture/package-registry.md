# [H1][THINKTECTURE_PACKAGE_REGISTRY]
>**Dictum:** *Thinktecture package posture starts from the pinned generator contract.*

<br>

[IMPORTANT] Rasm currently pins `Thinktecture.Runtime.Extensions` `10.2.0` through central package management. Treat that version as the implementation baseline unless `Directory.Packages.props` changes.

---
## [1][ACTIVE_BASELINE]
>**Dictum:** *The core package is both API and code generation infrastructure.*

<br>

| [INDEX] | [PACKAGE] | [VERSION] | [POSTURE] | [ROLE] |
| :-----: | --------- | --------: | :-------: | ---- |
| [1] | `Thinktecture.Runtime.Extensions` | `10.2.0` | Use | Primary API for `[ValueObject<T>]`, `[ComplexValueObject]`, `[SmartEnum<T>]`, `[Union]`, `[AdHocUnion]`, `ValidationError`, comparers, generated factories, and dispatch. |
| [2] | `Thinktecture.Runtime.Extensions.SourceGenerator` | `10.2.0` | Transitive | `developmentDependency`; `.NETStandard2.0` generator package excluded from runtime, compile, and build asset flow. |
| [3] | `Thinktecture.Runtime.Extensions.Analyzers` | `10.2.0` | Transitive | `developmentDependency`; `.NETStandard2.0` analyzer and code-fix package excluded from runtime, compile, and build asset flow. |
| [4] | `Thinktecture.Runtime.Extensions.Refactorings` | `10.2.0` | Transitive | `developmentDependency`; `.NETStandard2.0` refactoring package excluded from runtime, compile, and build asset flow. |

[CRITICAL] Do not add analyzer, source-generator, or refactoring package references manually. The core package brings the matching `10.2.0` tooling transitively.

---
## [2][OPTIONAL_INTEGRATIONS]
>**Dictum:** *Integration packages are boundary tools, not domain dependencies.*

<br>

| [INDEX] | [PACKAGE] | [PINNED_COMPATIBLE] | [PRERELEASE_AVAILABLE] | [POSTURE] | [ACTIVATION_TRIGGER] |
| :-----: | --------- | ------------------: | ---------------------: | :-------: | -------------------- |
| [1] | `Thinktecture.Runtime.Extensions.Json` | `10.2.0` | `10.3.0-beta01` | Defer | System.Text.Json boundary serializes generated types. |
| [2] | `Thinktecture.Runtime.Extensions.Newtonsoft.Json` | `10.2.0` | `10.3.0-beta01` | Defer | Newtonsoft.Json remains required by host protocol. |
| [3] | `Thinktecture.Runtime.Extensions.MessagePack` | `10.2.0` | `10.3.0-beta01` | Defer | Binary MessagePack wire format appears. |
| [4] | `Thinktecture.Runtime.Extensions.AspNetCore` | `10.2.0` | `10.3.0-beta01` | Defer | MVC model binding handles generated types. |
| [5] | `Thinktecture.Runtime.Extensions.EntityFrameworkCore8` | `10.2.0` | `10.3.0-beta01` | Defer | EF Core 8 persistence stores generated types. |
| [6] | `Thinktecture.Runtime.Extensions.EntityFrameworkCore9` | `10.2.0` | `10.3.0-beta01` | Defer | EF Core 9 persistence stores generated types. |
| [7] | `Thinktecture.Runtime.Extensions.EntityFrameworkCore10` | `10.2.0` | `10.3.0-beta01` | Defer | EF Core 10 persistence stores generated types. |
| [8] | `Thinktecture.Runtime.Extensions.Swashbuckle` | `10.2.0` | `10.3.0-beta01` | Defer | OpenAPI describes generated-type request or response contracts. |

[IMPORTANT] Add optional packages only at boundary projects that actually serialize, bind, persist, or describe generated types. Domain libraries stay on the core package.

---
## [3][LEGACY_PACKAGES]
>**Dictum:** *Old split packages do not describe v10 code.*

<br>

| [INDEX] | [PACKAGE_FAMILY] | [POSTURE] | [REASON] |
| :-----: | --------------- | :-------: | ------ |
| [1] | `Thinktecture.Runtime.Extensions.Abstractions` | Ignore | Legacy split package outside the v10 core model. |
| [2] | Unsuffixed or pre-v8 EF packages | Ignore | Use only EF-major packages matching the active host. |
| [3] | Old `.SourceGenerator` integration packages | Ignore | v10 integration packages own converter and binder generation. |
| [4] | `Thinktecture.Runtime.Extensions.ProtoBuf` | Ignore | Older prerelease integration with no current repo protocol need. |

---
## [4][RULES]
>**Dictum:** *Generated shape replaces local ceremony only when the generated contract is the contract.*

<br>

- Use core `10.2.0` surfaces directly for generated domain shape.
- Keep `10.3.0-beta01` deferred while the central package pin remains `10.2.0`.
- Add optional integration packages only through central package management.
- Prefer Thinktecture generated factories, lookup, comparers, and dispatch over hand-written wrappers.
- Ignore legacy package families unless a migration task explicitly targets old code.
