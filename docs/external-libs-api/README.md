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
| [1] | `LanguageExt` | Active | Functional rails, effects, immutable collections, traits. |
| [2] | `Thinktecture` | Active | Value objects, smart enums, unions, generated dispatch. |
| [3] | `MathNet` | Active | Managed numerical algorithms, symbolic expressions, linear algebra, statistics, fitting, integration, interpolation. |

---
## [2][EVIDENCE]
>**Dictum:** *Pinned local artifacts outrank broad memory.*

<br>

| [INDEX] | [ANCHOR] | [USE] |
| :-----: | -------- | ----- |
| [1] | `Directory.Packages.props` | Pinned package versions. |
| [2] | `.cache/nuget/packages/languageext.core/5.0.0-beta-77/lib/net10.0/LanguageExt.Core.xml` | Local API member surface. |
| [3] | `.cache/nuget/packages/thinktecture.runtime.extensions/10.2.0/lib/net9.0/Thinktecture.Runtime.Extensions.xml` | Local Thinktecture API member surface. |
| [4] | `.cache/nuget/packages/thinktecture.runtime.extensions.sourcegenerator/10.2.0/analyzers/dotnet/cs/Thinktecture.Runtime.Extensions.SourceGenerator.xml` | Generated-shape behavior and source-generator surface. |
| [5] | `.cache/nuget/packages/thinktecture.runtime.extensions.analyzers/10.2.0/analyzers/dotnet/cs/Thinktecture.Runtime.Extensions.Analyzers.xml` | Analyzer rule posture. |
| [6] | `.cache/nuget/packages/thinktecture.runtime.extensions/10.2.0/README.md` | Package inventory and integration orientation. |
| [7] | `.cache/nuget/packages/thinktecture.runtime.extensions/10.2.0/thinktecture.runtime.extensions.nuspec` | Transitive tooling package posture. |
| [8] | `.cache/nuget/packages/mathnet.numerics/6.0.0-beta2/mathnet.numerics.nuspec` | Active MathNet Numerics package metadata. |
| [9] | `.cache/nuget/packages/mathnet.numerics/6.0.0-beta2/lib/net8.0/MathNet.Numerics.xml` | Active MathNet Numerics API member surface. |
| [10] | `.cache/nuget/packages/mathnet.symbolics/0.25.0/mathnet.symbolics.nuspec` | Active MathNet Symbolics dependency closure. |
| [11] | `.cache/nuget/packages/mathnet.symbolics/0.25.0/lib/net8.0/MathNet.Symbolics.xml` | Active MathNet Symbolics API member surface. |
| [12] | `https://www.nuget.org/packages/MathNet.Numerics/6.0.0-beta2` | Active Numerics prerelease package metadata. |
| [13] | `https://www.nuget.org/packages/MathNet.Symbolics/0.25.0` | Active Symbolics package metadata. |
| [14] | `https://developer.rhino3d.com/en/guides/rhinocommon/moving-to-dotnet-core/` | McNeel .NET Core runtime and package loading guidance. |

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
## [4][RULES]
>**Dictum:** *Use the library surface directly and deliberately.*

<br>

- Prefer approved external APIs over local reinvention.
- Extend existing repo shapes before creating new records, wrappers, or helpers.
- Keep examples aligned with `coding-csharp`: `Fin<T>`, `Validation<Error,T>`, `Eff<RT,T>`, `Seq<T>`, and generated Thinktecture dispatch.
- Keep MathNet values and symbolic expressions behind Rasm-owned records and `Fin<T>` failure rails.
- Keep this registry reference-oriented; do not add tutorials, copied API mirrors, or generated dumps.
