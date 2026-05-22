# [H1][EXTERNAL_LIBS_API]
>**Dictum:** *External APIs are operating surfaces, not background dependencies.*

<br>

[IMPORTANT] This folder indexes approved non-`System.*` external library APIs for direct, wrapper-free implementation.

[IMPORTANT] Baseline: `Directory.Packages.props` owns pinned package versions. Child docs own package-specific source anchors. `../system-api-map/README.md` owns BCL and `System.*` policy.

---
## [1][SCOPE]
>**Dictum:** *Each library entry documents the boundary it owns.*

<br>

| [INDEX] | [LIBRARY] | [BOUNDARY] |
| :-----: | --------- | ---------- |
| [1] | `LanguageExt` | Functional rails, effects, immutable collections, traits. |
| [2] | `Thinktecture` | Value objects, smart enums, unions, generated dispatch. |
| [3] | `MathNet` | Numerical algorithms, symbolic expressions, linear algebra, statistics. |

---
## [2][EVIDENCE]
>**Dictum:** *Pinned local artifacts outrank broad memory.*

<br>

| [INDEX] | [ANCHOR] | [USE] |
| :-----: | -------- | ----- |
| [1] | `Directory.Packages.props` | Current pinned package truth. |
| [2] | `languageext/*.md` | LanguageExt API and Rasm integration evidence. |
| [3] | `thinktecture/*.md` | Thinktecture API and generated-shape evidence. |
| [4] | `mathnet/*.md` | MathNet API and Rhino/GH2 integration evidence. |
| [5] | `../system-api-map/README.md` | BCL, `System.*`, package/reference, and RhinoWIP host policy. |

---
## [3][FILES]
>**Dictum:** *Each slice is compact but complete enough for implementation work.*

<br>

| [INDEX] | [FILE] | [PURPOSE] |
| :-----: | ------ | --------- |
| [1] | `languageext/core-api-map.md` | Unified API family map. |
| [2] | `languageext/rop-effects.md` | Error rails, effects, recovery, collapse boundaries. |
| [3] | `languageext/traits-hkt.md` | Higher-kinded traits and effect-polymorphic algorithms. |
| [4] | `languageext/collections-traversal.md` | Immutable collections, folds, traversal, accumulation. |
| [5] | `languageext/repo-integration.md` | Rasm-specific integration map. |
| [6] | `thinktecture/core-api-map.md` | Unified generated-type API family map. |
| [7] | `thinktecture/value-objects.md` | Value-object construction, validation, comparison, conversion, and rails. |
| [8] | `thinktecture/smart-enums.md` | Closed vocabularies, generated lookup, dispatch, and item behavior. |
| [9] | `thinktecture/unions-dispatch.md` | Regular unions, ad-hoc unions, generated dispatch, and case conversion. |
| [10] | `thinktecture/analyzers-sourcegen.md` | Analyzer, generator, integration, and object-factory rules. |
| [11] | `thinktecture/repo-integration.md` | Rasm-specific generated-shape integration map. |
| [12] | `mathnet/api.md` | Unified MathNet API family map. |
| [13] | `mathnet/linear-algebra-rasm-vectors.md` | Rasm vector and matrix integration. |
| [14] | `mathnet/rhino-geometry-numerics.md` | Rhino geometry and numerical boundary rules. |
| [15] | `mathnet/gh2-numerics-boundaries.md` | GH2 numerical output and determinism rules. |
| [16] | `mathnet/symbolics-rhino-gh2.md` | Symbolics definition, runtime loading, and Rhino/GH2 integration. |
| [17] | `mathnet/advanced-rhino-math-stack.md` | Advanced RhinoWIP, GH2, Numerics, Symbolics, LanguageExt, and Thinktecture stack. |

---
## [4][RELATED_POLICY]
>**Dictum:** *Adjacent policy owns platform APIs and package adoption.*

<br>

| [INDEX] | [FILE] | [PURPOSE] |
| :-----: | ------ | --------- |
| [1] | `../system-api-map/README.md` | System API, BCL, `System.*`, and package adoption hub. |

---
## [5][RULES]
>**Dictum:** *Use the library surface directly and deliberately.*

<br>

- Use approved external APIs directly when they own the concern.
- Extend existing repo shapes before adding wrappers or duplicate records.
- Keep library-specific examples in child docs.
- Route BCL and `System.*` decisions through `../system-api-map/README.md`.
- Keep this registry reference-oriented; do not add tutorials, copied API mirrors, or generated dumps.
