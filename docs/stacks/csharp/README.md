# [STACKS_CSHARP]

This folder is the C# stack decision atlas. It routes language, platform, dependency-backed capability, numeric, sparse, and proof-tool decisions to the document that owns the coding choice.

## [1][CHOOSE]

This table is a lookup by reader decision.

| [INDEX] | [DECISION]              | [READ]                                               |
| :-----: | :---------------------- | :--------------------------------------------------- |
|   [1]   | language shape          | [language](language.md)                              |
|   [2]   | result flow             | [rails and effects](rails-and-effects.md)            |
|   [3]   | domain shape            | [domain shapes](domain-shapes.md)                    |
|   [4]   | numeric algorithm       | [numeric algorithms](numeric-algorithms.md)          |
|   [5]   | direct sparse solve     | [sparse factorization](sparse-factorization.md)      |
|   [6]   | package or build truth  | [build and packages](platform/build-and-packages.md) |
|   [7]   | system API replacement  | [system APIs](platform/system-apis.md)               |
|   [8]   | proof rail              | [testing](testing/README.md)                         |
|   [9]   | cross-stack precedence  | [usage](../../usage/README.md)                       |
|  [10]   | next C# stack page work | [roadmap](ROADMAP.md)                                |

## [2][OWNER_RULE]

Package versions, references, injected globals, local tools, and graph admission live in [build and packages](platform/build-and-packages.md). System API replacement decisions live in [system APIs](platform/system-apis.md). Coding decisions live in the capability page that owns the concern, even when an approved package implements the behavior.

First-class library capability is written as normal C# stack policy. LanguageExt owns result flow, Thinktecture owns generated domain shape, MathNet owns dense and iterative numeric algorithms, CSparse owns direct sparse factorization, and test packages own proof rails through [testing](testing/README.md).
