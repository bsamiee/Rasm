# [H1][EXTERNAL_LIBS_API]
>**Dictum:** *External APIs are operating surfaces, not background dependencies.*

<br>

[IMPORTANT] External-library slices give future agents minimum durable context for approved APIs, direct integration, and wrapper-free implementation.

---
## [1][SCOPE]
>**Dictum:** *Each library entry documents the boundary it owns.*

<br>

| [INDEX] | [LIBRARY] | [STATUS] | [BOUNDARY] |
| :-----: | --------- | :------: | ---------- |
| **[1]** | `LanguageExt` | Active | Functional rails, effects, immutable collections, traits. |
| **[2]** | `Thinktecture` | Active | Value objects, smart enums, unions, generated dispatch. |
| **[3]** | `RhinoWIP` and `Grasshopper2` | External | Governed by `docs/rhino-gh2-api-ledger.md`. |

[CRITICAL] RhinoCommon and GH2 API decisions stay in `docs/rhino-gh2-api-ledger.md`. Do not duplicate those ledgers here.

---
## [2][EVIDENCE]
>**Dictum:** *Pinned local artifacts outrank broad memory.*

<br>

| [INDEX] | [ANCHOR] | [USE] |
| :-----: | -------- | ----- |
| **[1]** | `Directory.Packages.props` | Pinned package versions. |
| **[2]** | `.cache/nuget/packages/languageext.core/5.0.0-beta-77/lib/net10.0/LanguageExt.Core.xml` | Local API member surface. |
| **[3]** | `.cache/nuget/packages/thinktecture.runtime.extensions/10.2.0/lib/net9.0/Thinktecture.Runtime.Extensions.xml` | Local Thinktecture API member surface. |
| **[4]** | `.cache/nuget/packages/thinktecture.runtime.extensions.sourcegenerator/10.2.0/analyzers/dotnet/cs/Thinktecture.Runtime.Extensions.SourceGenerator.xml` | Generated-shape behavior and source-generator surface. |
| **[5]** | `.cache/nuget/packages/thinktecture.runtime.extensions.analyzers/10.2.0/analyzers/dotnet/cs/Thinktecture.Runtime.Extensions.Analyzers.xml` | Analyzer rule posture. |
| **[6]** | `.cache/nuget/packages/thinktecture.runtime.extensions/10.2.0/README.md` | Package inventory and integration orientation. |
| **[7]** | `.cache/nuget/packages/thinktecture.runtime.extensions/10.2.0/thinktecture.runtime.extensions.nuspec` | Transitive tooling package posture. |

---
## [3][FILES]
>**Dictum:** *Each slice is compact but complete enough for implementation work.*

<br>

| [INDEX] | [FILE] | [PURPOSE] |
| :-----: | ------ | --------- |
| **[1]** | `languageext/package-registry.md` | Package posture and dependency relevance. |
| **[2]** | `languageext/core-api-map.md` | Unified API family map. |
| **[3]** | `languageext/rop-effects.md` | Error rails, effects, recovery, collapse boundaries. |
| **[4]** | `languageext/traits-hkt.md` | Higher-kinded traits and effect-polymorphic algorithms. |
| **[5]** | `languageext/collections-traversal.md` | Immutable collections, folds, traversal, accumulation. |
| **[6]** | `languageext/repo-integration.md` | Rasm-specific integration map. |
| **[7]** | `thinktecture/package-registry.md` | Package posture and dependency relevance. |
| **[8]** | `thinktecture/core-api-map.md` | Unified generated-type API family map. |
| **[9]** | `thinktecture/value-objects.md` | Value-object construction, validation, comparison, conversion, and rails. |
| **[10]** | `thinktecture/smart-enums.md` | Closed vocabularies, generated lookup, dispatch, and item behavior. |
| **[11]** | `thinktecture/unions-dispatch.md` | Regular unions, ad-hoc unions, generated dispatch, and case conversion. |
| **[12]** | `thinktecture/analyzers-sourcegen.md` | Analyzer, generator, integration, and object-factory rules. |
| **[13]** | `thinktecture/repo-integration.md` | Rasm-specific generated-shape integration map. |

---
## [4][RULES]
>**Dictum:** *Use the library surface directly and deliberately.*

<br>

- Prefer approved external APIs over local reinvention.
- Extend existing repo shapes before creating new records, wrappers, or helpers.
- Keep examples aligned with `coding-csharp`: `Fin<T>`, `Validation<Error,T>`, `Eff<RT,T>`, `Seq<T>`, and generated Thinktecture dispatch.
- Keep this registry reference-oriented; do not add tutorials, copied API mirrors, or generated dumps.
