# [RASM_BIM_API_SMINO_BCF_TOOLKIT]

`Smino.Bcf.Toolkit` owns the BCF (BIM Collaboration Format) codec — the `.bcfzip` container and BCF-API JSON round-trip behind the COORDINATION issue board. `Worker` is the one converter, its overloads discriminating by sink shape (path, `Stream`, target `BcfVersionEnum`) and async-vs-sync to fold a version-tagged `IBcf` graph to and from the zipped container. `IBcf` discriminates `Bcf30.Bcf` and `Bcf21.Bcf`, so the host-neutral `BcfTopic`/`BcfComment`/`BcfViewpoint` projection reads one interface root at any format version; `BcfVersion.TryParse` admits `2.1` and `3.0`.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `Smino.Bcf.Toolkit`
- package: `Smino.Bcf.Toolkit` (Apache-2.0, `LICENSE` file)
- assembly: `bcf-toolkit`
- namespace: `BcfToolkit`, `BcfToolkit.Model`, `BcfToolkit.Model.Bcf30`, `BcfToolkit.Model.Bcf21`, `BcfToolkit.Model.Interfaces`, `BcfToolkit.Builder.Bcf30`, `BcfToolkit.Builder.Bcf21`, `BcfToolkit.Utils`
- asset: net9.0 single TFM, bound by the net10.0 consumer as `lib/net9.0`; pure-managed AnyCPU, no `runtimes/` folder
- dependency: `Json.Net`, `Newtonsoft.Json`, `RecursiveDataAnnotationsValidation`, `Serilog`, `System.CommandLine`, `System.Text.RegularExpressions` — the `System.CommandLine` binder and `Serilog` sink drive the package's own CLI tool host, unreached by the library entrypoints
- rail: review

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: converter, version axis, header payload

| [INDEX] | [SYMBOL]         | [TYPE_FAMILY] | [CAPABILITY]                                                                     |
| :-----: | :--------------- | :------------ | :------------------------------------------------------------------------------- |
|  [01]   | `Worker`         | class         | the converter engine; file/stream/version-overloaded round-trip surface in [03]  |
|  [02]   | `BcfVersionEnum` | enum          | target-version discriminant: `Bcf21` (2.1), `Bcf30` (3.0)                        |
|  [03]   | `BcfVersion`     | static class  | version helpers — `TryParse(string/IBcf/Type)` throws on unsupported, `ToString` |
|  [04]   | `FileData`       | class         | header-file payload carrier: referenced model bytes inside the container         |

[PUBLIC_TYPE_SCOPE]: version-agnostic interface contract — `IBcf` is the discriminated root both `Bcf30.Bcf`/`Bcf21.Bcf` implement, and the interfaces are thin markers: `ITopic` declares only `Guid`, so the status/type/priority/date/label detail rides the concrete `Topic` and the projection narrows `IBcf`→`IMarkup` then reads the concrete topic for detail columns.

| [INDEX] | [SYMBOL]             | [TYPE_FAMILY] | [CAPABILITY]                                                                                      |
| :-----: | :------------------- | :------------ | :------------------------------------------------------------------------------------------------ |
|  [01]   | `IBcf`               | interface     | container root the projection narrows from (`Markups`/`Extensions`/`Project`/`Version`)           |
|  [02]   | `IMarkup`            | interface     | one topic markup unit: topic, comments, viewpoints (detail on concrete `Markup`)                  |
|  [03]   | `ITopic`             | interface     | issue-record marker declaring `Guid`; status/type/priority/dates/labels on `Topic`                |
|  [04]   | `IComment`           | interface     | threaded-comment marker (author/date/text, viewpoint back-reference on `Comment`)                 |
|  [05]   | `IViewPoint`         | interface     | viewpoint-reference marker binding a topic to its `IVisualizationInfo`                            |
|  [06]   | `IVisualizationInfo` | interface     | saved-view marker: camera, component visibility, clipping, lines, bitmaps                         |
|  [07]   | `IOrthogonalCamera`  | interface     | orthographic camera marker                                                                        |
|  [08]   | `IPerspectiveCamera` | interface     | perspective camera marker                                                                         |
|  [09]   | `IComponent`         | interface     | referenced-model-element marker (IFC GUID, authoring-tool id) in a selection                      |
|  [10]   | `IVisibility`        | interface     | default-visibility plus per-component exceptions marker for a viewpoint                           |
|  [11]   | `IClippingPlane`     | interface     | section/clipping-plane marker in a viewpoint                                                      |
|  [12]   | `IExtensions`        | interface     | project extension marker: topic types, statuses, priorities, labels, users, stages, snippet types |
|  [13]   | `IProject`           | interface     | project-identity marker (name, project guid)                                                      |
|  [14]   | `IDocumentInfo`      | interface     | BCF document-library marker                                                                       |

[PUBLIC_TYPE_SCOPE]: concrete schema object graphs — each row names the type in `Bcf30` and, unless the row prefixes `Bcf30.`, its `Bcf21` twin

| [INDEX] | [SYMBOL]                  | [TYPE_FAMILY] | [CAPABILITY]                                                                         |
| :-----: | :------------------------ | :------------ | :----------------------------------------------------------------------------------- |
|  [01]   | `Bcf30.Bcf`               | class         | BCF root; members in `[01]-[BCFROOT]`                                                |
|  [02]   | `Bcf21.Bcf`               | class         | BCF root, the same shape minus the document library                                  |
|  [03]   | `Markup`                  | class         | concrete markup: `Topic`, `Comment` list, `ViewPoint` list                           |
|  [04]   | `Topic`                   | class         | issue record; status/type/priority/labels/assignee/dates/related/document-refs       |
|  [05]   | `Comment`                 | class         | concrete threaded comment; `CommentViewpoint` back-reference                         |
|  [06]   | `Bcf30.VisualizationInfo` | class         | concrete saved view; members in `[06]-[VIZINFO]`                                     |
|  [07]   | `Bcf30.Components`        | class         | selection/visibility/coloring component sets inside a viewpoint                      |
|  [08]   | `ComponentSelection`      | class         | the selection component set                                                          |
|  [09]   | `ComponentVisibility`     | class         | the visibility component set                                                         |
|  [10]   | `Bcf30.Point`             | class         | camera position vector                                                               |
|  [11]   | `Bcf30.Direction`         | class         | camera direction/up vector                                                           |
|  [12]   | `Bcf30.Extensions`        | class         | project extensions; enum vocabularies the `SignOff` machine and topic-type axis read |

- [01]-[BCFROOT]: `Bcf30.Bcf` — `ConcurrentBag<Markup> Markups`, `DocumentInfo? Document`, `Extensions`, `ProjectInfo? Project`, `Version`.
- [06]-[VIZINFO]: `Bcf30.VisualizationInfo` — `OrthogonalCamera`/`PerspectiveCamera`, `Components`, lines, clipping planes, bitmaps.

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: `Worker` — round-trip conversion

| [INDEX] | [SURFACE]                                                        | [SHAPE]  | [CAPABILITY]                                            |
| :-----: | :--------------------------------------------------------------- | :------- | :------------------------------------------------------ |
|  [01]   | `Convert(string, string)`                                        | instance | path→path, auto-detecting source/target by extension    |
|  [02]   | `BcfFromStream(Stream) -> Task<Bcf>`                             | instance | reads a `.bcfzip` stream into the typed `Bcf` root      |
|  [03]   | `ToBcf(IBcf, string)`                                            | instance | writes a model to a `.bcfzip` file                      |
|  [04]   | `ToBcf(IBcf, BcfVersionEnum) -> Task<Stream>`                    | instance | in-memory `.bcfzip` stream at a chosen version          |
|  [05]   | `ToBcf(IBcf, BcfVersionEnum, CancellationToken) -> Task<Stream>` | instance | cancellable stream serialization                        |
|  [06]   | `ToBcf(IBcf, BcfVersionEnum, Stream[, CancellationToken])`       | instance | sync write to a caller-owned stream                     |
|  [07]   | `ToJson(IBcf, string)`                                           | instance | writes the BCF-API JSON resource model to a folder/file |

[ENTRYPOINT_SCOPE]: `BcfExtensions` — per-part stream parse beneath `Worker`, reading a single container member without a full deserialize

| [INDEX] | [SURFACE]                                                                           | [SHAPE] | [CAPABILITY]                   |
| :-----: | :---------------------------------------------------------------------------------- | :------ | :----------------------------- |
|  [01]   | `GetVersionFromStreamArchive(Stream) -> Task<BcfVersionEnum?>`                      | static  | sniffs the container version   |
|  [02]   | `ParseMarkups<TMarkup, TVisualizationInfo>(Stream) -> Task<ConcurrentBag<TMarkup>>` | static  | streams markups and viewpoints |
|  [03]   | `ParseExtensions<TExtensions>(Stream) -> Task<TExtensions>`                         | static  | extension vocabularies         |
|  [04]   | `ParseProject<TProjectInfo>(Stream) -> Task<TProjectInfo?>`                         | static  | project identity member        |
|  [05]   | `ParseDocuments<TDocumentInfo>(Stream) -> Task<TDocumentInfo?>`                     | static  | BCF document library member    |

[ENTRYPOINT_SCOPE]: `BcfBuilder` — fluent authoring, the surface the `ClashProposal`→`BcfTopic` fold composes; every builder implements `IBuilder<T>.Build()`/`IDefaultBuilder<T>.WithDefaults()` and the root also `IFromStreamBuilder<T>.BuildFromStream(Stream)`, nested `Action<TBuilder>` closures authoring the markup/topic/comment/component/camera tree

| [INDEX] | [SURFACE]                                        | [SHAPE]  | [CAPABILITY]                                                     |
| :-----: | :----------------------------------------------- | :------- | :--------------------------------------------------------------- |
|  [01]   | `AddMarkup(Action<MarkupBuilder>) -> BcfBuilder` | instance | adds a topic markup; `AddMarkups(List<Markup>, bool)` bulk form  |
|  [02]   | `SetProject(Action<ProjectInfoBuilder>)`         | instance | sets project identity; `ProjectInfo?` concrete overload          |
|  [03]   | `SetExtensions(Action<ExtensionsBuilder>)`       | instance | sets extension vocabularies; `Extensions` concrete overload      |
|  [04]   | `SetDocument(Action<DocumentInfoBuilder>)`       | instance | sets document library; `DocumentInfo?` concrete overload         |
|  [05]   | `WithDefaults() -> BcfBuilder`                   | instance | seeds default extension vocabularies                             |
|  [06]   | `Build() -> Bcf`                                 | instance | materializes the typed `IBcf` root for `Worker.ToBcf`            |
|  [07]   | `BuildFromStream(Stream) -> Task<Bcf>`           | instance | reads an existing `.bcfzip` into a builder for edit-then-`Build` |

- [08]-[NESTED]: per-node builders nested inside `AddMarkup`, each an `Action<TBuilder>` leaf with chainable setters — `MarkupBuilder` (`SetGuid`/`SetTitle`/`SetTopicType`/`SetTopicStatus`/`SetPriority`/`SetCreationAuthor`/`AddComment`/`AddViewPoint`), `CommentBuilder` (`SetGuid`/`SetAuthor`/`SetComment`/`SetDate`/`SetViewPointGuid`), `ViewPointBuilder` (`SetGuid`), `ComponentBuilder`, `CameraBuilder`.

## [04]-[IMPLEMENTATION_LAW]

[TOPOLOGY]:
- `Worker` folds a version-tagged `IBcf` graph, and the host-neutral `BcfTopic`/`BcfComment`/`BcfViewpoint` projection reads through `IBcf`/`ITopic`/`IComment`/`IViewPoint` — never a concrete `Bcf30`/`Bcf21` type — so the projection is written once and `BcfVersionEnum` selects only the read/write asset.
- `Worker.ToBcf` discriminates path vs. in-memory `Stream` vs. caller-owned `Stream`, async vs. sync, on overload; `Worker.ToBcf(bcf, Bcf30)` over a `Bcf21`-sourced `IBcf` up-converts mixed-version exchange to one internal generation.
- `BcfExtensions.GetVersionFromStreamArchive` sniffs the container version before `ParseMarkups`/`ParseExtensions` materialize only the needed members, so a topic-roster import skips the full object-graph deserialize.

[STACKING]:
- `ids-lib`(`.api/api-ids-lib.md`): `Coordination.Raise` folds each non-conforming `IdsAudit` (the ids-lib `Audit` `BufferedValidationIssue` `Level`/`Message`/`Line`/`Position` per facet) onto one `BcfTopic` with one `BcfComment` per failing facet — the write-time IDS→BCF handoff, provenance carried as topic/comment free-text since BCF has no structured line column.
- COORDINATION owner: the `ClashProposal` fold authors a `BcfTopic` from `Model/systems#INTERFERENCE` clash evidence through `BcfBuilder.AddMarkup(...)` → `Build()` → `Worker.ToBcf(bcf, Bcf30)`; the BCF `Extensions` status/type/priority vocabularies feed the `SignOff` `[SmartEnum]` state machine; the topic GUID joins a `BcfTopic` to its `ElementSet` through `IComponent` IFC GUIDs on the shared `Model/query#ELEMENT_SET` `ElementPredicate` algebra at read time; any `Worker` throw folds to `Model/faults#FAULT_BAND` `BimFault.CodecReject` at the Persistence `Ingest/issue` codec boundary.

[LOCAL_ADMISSION]:
- `Smino.Bcf.Toolkit` reads and writes BCF containers and the BCF-API JSON model only — the issue-board lifecycle, clash detection, IFC model graph, state machine, clash→topic fold, and `ElementSet` join are COORDINATION concerns.
- source `BcfVersionEnum`, topic count, and target serialization shape are the receipt facts the ISSUES/COORDINATION fold records.
- `System.CommandLine` binder and `Serilog` sink form the package's own CLI host; the design composes `Worker`/`BcfExtensions` directly.

[RAIL_LAW]:
- Package: `Smino.Bcf.Toolkit`
- Owns: BCF container and BCF-API JSON round-trip
- Accept: the `.bcfzip` ISSUES codec and the COORDINATION issue-exchange wire
- Reject: issue-board lifecycle/state, clash detection, IFC model graph, the bundled CLI host
