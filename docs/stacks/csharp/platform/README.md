# [CSHARP_PLATFORM]

This folder owns platform truth for C# stack work. It separates graph and build facts from BCL/System replacement decisions so package state does not leak into coding-policy pages.

## [1][CHOOSE]

This table is a lookup by platform question.

| [INDEX] | [QUESTION]                                     | [READ]                                      |
| :-----: | :--------------------------------------------- | :------------------------------------------ |
|   [1]   | Which manifest or build owner controls a fact? | [build and packages](build-and-packages.md) |
|   [2]   | Which system API replaces local machinery?     | [system APIs](system-apis.md)               |
|   [3]   | Which C# syntax expresses the shape?           | [language](../language.md)                  |
|   [4]   | Which capability page owns coding policy?      | [C# stack](../README.md)                    |
|   [5]   | Which proof rail owns a test concern?          | [testing](../testing/README.md)             |
|   [6]   | Which cross-stack owner wins?                  | [usage](../../../usage/README.md)           |

## [2][BOUNDARY]

Keep package versions, graph state, host references, global usings, analyzers, tools, and adoption gates in [build and packages](build-and-packages.md). Keep BCL/System replacements in [system APIs](system-apis.md). Move coding guidance to the root C# capability page that owns the concern.
