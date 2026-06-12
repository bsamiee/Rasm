# [RASM_APPUI_API_GH2]

`Grasshopper2` supplies GH2 document, canvas, component, parameter, data access, solve, bake, attribute, and UI surfaces for AppUi host adapters.

## [1]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: host assembly `Grasshopper2`
- package: `Grasshopper2`
- assembly: `Grasshopper2`
- namespace: `Grasshopper2`
- namespace: `Grasshopper2.Components`
- namespace: `Grasshopper2.Doc`
- namespace: `Grasshopper2.Parameters`
- namespace: `Grasshopper2.Bake`
- asset: host assembly
- rail: host-gh2

## [2]-[PUBLIC_TYPES]

[DOCUMENT_AND_CANVAS_TYPES]: GH2 document and UI surface
- rail: host-gh2

| [INDEX] | [SYMBOL]                      | [RAIL]         |
| :-----: | :---------------------------- | :------------- |
|   [1]   | `Document`                    | document root  |
|   [2]   | `Canvas`                      | canvas surface |
|   [3]   | `ComponentAttributes`         | component UI   |
|   [4]   | `ChainAttributes`             | chain UI       |
|   [5]   | `FloatingParameterAttributes` | parameter UI   |
|   [6]   | `WireData`                    | wire model     |

[COMPONENT_TYPES]: component, parameter, and solve surface
- rail: host-gh2

| [INDEX] | [SYMBOL]                | [RAIL]            |
| :-----: | :---------------------- | :---------------- |
|   [1]   | `Component`             | component root    |
|   [2]   | `ModularComponent`      | modular component |
|   [3]   | `ComponentParameters`   | parameter set     |
|   [4]   | `ParameterEventArgs`    | parameter event   |
|   [5]   | `IDataAccess`           | data access       |
|   [6]   | `Iteration`             | solve iteration   |
|   [7]   | `ThreadingState`        | solve threading   |
|   [8]   | `VerificationException` | solve validation  |

[BAKE_TYPES]: bake and Rhino interop surface
- rail: host-gh2

| [INDEX] | [SYMBOL]             | [RAIL]        |
| :-----: | :------------------- | :------------ |
|   [1]   | `BakeContext`        | bake context  |
|   [2]   | `BakeKey`            | bake identity |
|   [3]   | `BakeDataState`      | bake state    |
|   [4]   | `BakeUpdateMode`     | bake update   |
|   [5]   | `IBakeAware`         | bake contract |
|   [6]   | `BakePropertiesForm` | bake UI       |

## [3]-[ENTRYPOINTS]

[COMPONENT_ENTRYPOINTS]: component and solve operations
- rail: host-gh2

| [INDEX] | [SURFACE]                | [SURFACE_ROOT]        | [RAIL]           |
| :-----: | :----------------------- | :-------------------- | :--------------- |
|   [1]   | `CreateAttributes`       | `Component`           | UI attributes    |
|   [2]   | `AddInputs`              | `Component`           | input parameter  |
|   [3]   | `AddOutputs`             | `Component`           | output parameter |
|   [4]   | `Layout`                 | `ComponentAttributes` | canvas layout    |
|   [5]   | `IDataAccess` operations | `IDataAccess`         | data transfer    |

[DOCUMENT_ENTRYPOINTS]: document, canvas, and bake operations
- rail: host-gh2

| [INDEX] | [SURFACE]          | [SURFACE_ROOT]   | [RAIL]             |
| :-----: | :----------------- | :--------------- | :----------------- |
|   [1]   | `BakeObject`       | `BakeContext`    | object bake        |
|   [2]   | `BakeTree`         | `BakeContext`    | tree bake          |
|   [3]   | `FindBakedObjects` | `BakeContext`    | bake lookup        |
|   [4]   | `WithProcess`      | `BakeContext`    | bake process       |
|   [5]   | `Document`         | `Canvas`         | canvas document    |
|   [6]   | `Solution`         | `Document`       | solution server    |
|   [7]   | `Expire`           | `DocumentObject` | solve invalidation |

## [4]-[IMPLEMENTATION_LAW]

[HOST_BOUNDARY_LAW]:
- Package: `Grasshopper2`
- Owns: GH2 document, canvas, component, parameter, data, bake, solve, and UI adapter surface
- Accept: GH2 APIs stay boundary-owned and emit typed UI receipts
- Reject: GH2 concepts as AppUi public types

[MODALITY_LAW]:
- Package: `Grasshopper2`
- Owns: GH2 panel, canvas, component, parameter, menu, and bake entrypoints for AppUi host mode
- Accept: GH2 host behavior is one adapter rail beside Rhino and Eto
- Reject: GH2 implementation vocabulary in shell, screen, command, theme, or live-state rails
