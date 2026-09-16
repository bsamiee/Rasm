# [RESEARCH]

Five folders hold what was read off the products the Creative Cloud work replaces or integrates with, and the product inventory of the machine those products run on. Each folder's README is the record of its subject; this file indexes them and holds the facts no one folder owns.

Readability class says how much of a product is legible: source means the code is present and readable, decoded means bytecode was decompiled into readable form, manuals only means the shipped code is compiled and the record rests on documentation and extracted strings, absent means the product is not on this machine.

## [01]-[FOLDERS]

| [INDEX] | [FOLDER]                | [HOLDS]                                                                                   | [READABILITY]           |
| :-----: | :---------------------- | :----------------------------------------------------------------------------------------- | :---------------------- |
|  [01]   | `grid-calculator/`      | Grid Calculator Publishing Edition installer, unpacked plug-in bundles, plug-in strings, vendor manual and site pages, layout wizard samples, page sizes | Manuals only            |
|  [02]   | `scribedoor/`           | ScribeDOOR 2026 user guides for InDesign and Illustrator, older guides, vendor pages, Middle Eastern DOM extracts | Absent on this machine  |
|  [03]   | `sidekick/`             | Sidekick server build output, impl bundle, enum mappings, UXP panel and manifest            | Source                  |
|  [04]   | `illustrator-scripts/`  | Record of fourteen Illustrator scripts and eleven action sets held read-only on the Drive    | Decoded and source      |
|  [05]   | `sources/`              | Accessibility inspectors per host with their JSON output, host probe scripts for Illustrator, InDesign, and Photoshop, the prettified Sidekick panel, the ES3 controller pipeline, font inventory | Source |

## [02]-[HOST_ADD_ONS]

Add-ons installed into the Beta builds, with the scripting surface each exposes.

| [INDEX] | [PRODUCT]                 | [SURFACE]                                                                                              |
| :-----: | :------------------------ | :------------------------------------------------------------------------------------------------------ |
|  [01]   | Illustrator Mockup        | Plug-ins `Extensions/PlanetX.aip` and `Deform.aip`, loader `OnDemandModule.aip`; menus `menu_1189 Make Vector Edge`, `menu_1190 Release Vector Edge`, `menu_1191 Edit Vector Edge` under Object > Mockup, `menu_1491 Adobe Vector Edge Panel` under Window > Mockup; model `MockupPanel-meshes-1.0` |
|  [02]   | Illustrator Retype        | `Extensions/ReType.aip`; `menu_2094 ReTypeTypeMenu` under Type > Retype at minVersion 30.5, `menu_1496 ReTypeWindowMenu` under Window > Retype; model `Retype-Inpainting-model-1.0` |
|  [03]   | Illustrator Masking       | `Extensions/MaskingAiCore.aip` and `ImageSegmentation.aip`; no dedicated menu row, the native clipping-mask rows `menu_1195`, `menu_1196`, `menu_1197` alone; no tool id; model `masking-ai-model-1.0` |
|  [04]   | Illustrator Mapping       | No `.aip`, no menu row, no tool id, so no scripting surface; models `depthmap-model-1.0` and `gen_expand_preset` |
|  [05]   | Photoshop Remove Tool     | Models under `/Library/Application Support/Adobe/Adobe Photoshop (Beta)/AddOnModules/sensei_model_cache/inpainting_ai/{super_caf,ultra_caf}`; `toolID "removeTool"`, tooltip `Remove Tool (J)`, advanced branch `ultra_caf`, preference enum `imageProcessingRemoveToolProcessingPrefsStr` |
|  [06]   | VectorScribe              | Not installed; the utility graphic-style names come from `Site Analysis Actions.aia` instead            |

Other Illustrator plug-ins that matter to the work: `Extensions/MCPToolkit.aip` is the in-host half of the official MCP, and `Extensions/ScriptingSupport.aip`, `Extensions/ScriptsMenu.aip`, and `Illustrator UI/MenuConfigurator.aip` carry the scripting and menu surfaces.

Mapping and Masking expose nothing to a script, so the passes state that and place nothing for them.

## [03]-[UXP_INSTALL_PRECEDENT]

Nine TK9 plug-ins sit under `~/Library/Application Support/Adobe/UXP/Plugins/External/` as `com.tk.{comboV8,cxV8,export,multimask,myactionsV8,myactionstab1,myactionstab2,myactionstab3,myactionstab4}_4.0.0`. Their manifests and HTML are readable and set the install route; `index.js` is obfuscated in every one, so the logic is no reference.

| [INDEX] | [MANIFEST KEY]                       | [VALUE]                                          |
| :-----: | :----------------------------------- | :----------------------------------------------- |
|  [01]   | `manifestVersion`                    | 5                                                |
|  [02]   | `host`                               | `{app: "PS", minVersion: "23.3.0"}`              |
|  [03]   | `requiredPermissions.localFileSystem`| `request`, and `fullAccess` in Export            |
|  [04]   | `requiredPermissions.network`        | `domains: ["https://goodlight.us"]`              |
|  [05]   | `requiredPermissions.launchProcess`  | `schemes: ["http","https"]` with an `extensions` list |
|  [06]   | `allowCodeGenerationFromStrings`     | `true`                                           |
|  [07]   | `clipboard`                          | `readAndWrite`                                   |

## [04]-[ILLUSTRATOR_MCP_TOOLS]

The official Illustrator MCP exposes 47 tools, listed in `plan/inputs/illustrator/mcp-tools.json`. It has no script tool and no execute tool.

| [INDEX] | [GROUP]              | [TOOLS]                                                                                                                                                        |
| :-----: | :------------------- | :--------------------------------------------------------------------------------------------------------------------------------------------------------------- |
|  [01]   | Artboards            | ListArtboards, CreateArtboard, SetArtboardProperties, DuplicateArtboard, GetActiveArtboard, ScaleArtboards, FitArtboard, DeleteArtboard, SetActiveArtboard, MoveArtboards, GetArtboardProperties, GetArtboardStructure |
|  [02]   | Documents and output | CreateDocument, OpenDocument, ListDocuments, SwitchDocument, Export, CapturePreview, Vectorize                                                                     |
|  [03]   | Reads                | GetCanvasStructure, GetObjectStructure, GetVisualAppearance, GetGeometry, GetTypographyMetrics, GetBounds, GetSwatches, FindObjects, VisualizeSelection, RunPreflightChecks |
|  [04]   | Mutations            | AlignObjects, DistributeObjects, SetAppearance, MoveObjects, RotateObjects, ScaleObjects, DuplicateObjects, DeleteObjects, SelectObjects, RenameObject, ArrangeArt, CreateLayer, CreateGroup, MoveObjectsToContainer, CreateClippingMask, CleanupPath, ReplaceText, ReplaceFont |

Gaps the tool list leaves: preferences, swatch, spot, symbol, brush, style, and pattern creation, `saveAs` options, templates, menu execution, right-to-left attributes, resource dumps, and captures above 512 by 512.

## [05]-[PHOTOSHOP_ACTIONS]

Photoshop action files are binary `.atn`. They are played, not inspected, so no pack below is a functional reference.

| [INDEX] | [SOURCE]                                                                        | [COUNT] |
| :-----: | :------------------------------------------------------------------------------ | :-----: |
|  [01]   | `/Applications/Adobe Photoshop (Beta)/Presets/Actions/`                          |    9    |
|  [02]   | `~/Library/Application Support/design-tools/tk9-v4/package/…/David Tillett TK9 example actions/` | 4 |
|  [03]   | `<drive.root>/03.Digital Asset Database/**/*.atn`                                |   47    |

The nine shipped sets are Commands, Frames, Image Effects, LAB - Black & White Technique, Production, Stars Trails, Text Effects, Textures, Video Actions. The four TK9 example sets are My TK9 Actions, TK9 Actions Non-English, TK9 Blend If actions, TK9 actions. Of the 47 Drive sets, 18 are per-pack Overlay Actions sets; the rest include f64 Gradients Course 2023, Design Syndrome's Glassmorph and Noise Graphics, the JHN architectural style sets, and per-effect packs for bitmap, risograph, film, and scan treatments.

## [06]-[SCRIBEDOOR]

ScribeDOOR is absent from this machine: no installer, no plug-in, no receipt. Its record is the 2026 user guides under `plan/research/scribedoor/docs/`, which map every panel control onto a documented InDesign or Illustrator DOM property, and the live probe of the World-Ready composer properties that the InDesign unit's research step runs.
