# [RASM_BIM_API_SMINO_BCF_TOOLKIT]

`Smino.Bcf.Toolkit` is the BCF (BIM Collaboration Format) codec backing the
`Review/issues#BCF_ARCHIVE` `.bcfzip` round-trip and the `Review/coordination#COORDINATION`
issue-board domain owner. The unifying primitive is `Worker` — a polymorphic
converter whose overloads discriminate by sink shape (file path, `Stream`, or
target `BcfVersionEnum`) and async vs. sync, folding a version-tagged `IBcf`
object root to and from the zipped container and the BCF-API JSON resource model.
Each BCF generation (2.1 and 3.0) is a typed object graph under one `IBcf`
interface, so the `BcfTopic`/`BcfComment`/`BcfViewpoint` host-neutral records
project from a single discriminated source regardless of file version.
`BcfVersion.TryParse` accepts only the version strings `"2.1"` and `"3.0"`; the
`"2.0/2.1/3.0"` phrasing in the manifest pin is not borne out — BCF 2.0 input is
rejected as `Unsupported BCF version`, so the codec's real coverage is BCF 2.1 + 3.0.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `Smino.Bcf.Toolkit`
- package: `Smino.Bcf.Toolkit`
- version: `3.2.0`
- license: file `LICENSE` (Apache-2.0)
- assembly: `bcf-toolkit`
- namespace: `BcfToolkit`, `BcfToolkit.Model`, `BcfToolkit.Model.Bcf30`, `BcfToolkit.Model.Bcf21`, `BcfToolkit.Model.Interfaces`, `BcfToolkit.Utils`
- asset: net9.0 single TFM; the net10.0 consumer binds `lib/net9.0` (pure-managed AnyCPU, no `runtimes/` folder)
- asset: managed transitive closure `Json.Net 1.0.33`, `Newtonsoft.Json 13.0.x`, `RecursiveDataAnnotationsValidation 2.x`, `Serilog 4.x`, `System.CommandLine 2.0.0-beta4` (CLI-binder surface, unused by the library entrypoints); `Json.Net` + `RecursiveDataAnnotationsValidation` + `Newtonsoft.Json` floor-pinned centrally for lockfile determinism
- rail: review

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: converter and version axis
- package: `Smino.Bcf.Toolkit`
- namespace: `BcfToolkit`, `BcfToolkit.Model`
- rail: review

| [INDEX] | [SYMBOL]         | [RAIL] | [CAPABILITY]                                                                                                       |
| :-----: | :--------------- | :----- | :--------------------------------------------------------------------------------------------------------------- |
|  [01]   | `Worker`         | review | the converter engine; file/stream/version-overloaded `Convert`/`ToBcf`/`ToJson`/`BcfFromStream` round-trip surface |
|  [02]   | `BcfVersionEnum` | review | target-version discriminant: `Bcf21` (the `Model.Bcf21` graph) and `Bcf30`; `BcfVersion.TryParse`/`ToString` map only `"2.1"`/`"3.0"` |
|  [03]   | `BcfVersion`     | review | static version helpers over `BcfVersionEnum`                                                                      |
|  [04]   | `FileData`       | review | header-file payload carrier (referenced model bytes inside the container)                                         |

[PUBLIC_TYPE_SCOPE]: version-agnostic interface contract
- package: `Smino.Bcf.Toolkit`
- namespace: `BcfToolkit.Model.Interfaces`
- rail: review

`IBcf` is the discriminated root every concrete `Bcf30.Bcf`/`Bcf21.Bcf`
implements; the host-neutral `BcfTopic`/`BcfComment` projection reads through
these interfaces so version selection never forks the projection code. The
interfaces are deliberately thin markers — `ITopic` declares only `Guid`, and the
rich status/priority/date/label fields live on the concrete `Bcf30.Topic` /
`Bcf21.Topic` classes; the projection narrows from `IBcf`→`IMarkup` then reads the
concrete topic for the detail columns.

| [INDEX] | [SYMBOL]             | [RAIL] | [CAPABILITY]                                                                                       |
| :-----: | :------------------- | :----- | :------------------------------------------------------------------------------------------------ |
|  [01]   | `IBcf`               | review | container root the projection narrows from (concrete `Markups`/`Extensions`/`Project`/`Version`)   |
|  [02]   | `IMarkup`            | review | one topic markup unit marker: the topic + its comments + viewpoints (detail on the concrete `Markup`) |
|  [03]   | `ITopic`             | review | issue-record marker; declares `Guid` — status/type/priority/assignee/dates/labels are on the concrete `Topic` |
|  [04]   | `IComment`           | review | threaded-comment marker (author/date/text + viewpoint back-reference on the concrete `Comment`)     |
|  [05]   | `IViewPoint`         | review | viewpoint-reference marker binding a topic to its `IVisualizationInfo`                              |
|  [06]   | `IVisualizationInfo` | review | saved-view marker: camera + component visibility + clipping + lines + bitmaps                        |
|  [07]   | `IOrthogonalCamera` / `IPerspectiveCamera` | review | the orthographic / perspective camera markers                                       |
|  [08]   | `IComponent`         | review | referenced-model-element marker (IFC GUID + authoring-tool id) inside a viewpoint selection/visibility |
|  [09]   | `IVisibility`        | review | default-visibility + per-component exceptions marker for a viewpoint                                 |
|  [10]   | `IClippingPlane`     | review | section/clipping-plane marker in a viewpoint                                                        |
|  [11]   | `IExtensions`        | review | project extension marker: topic types, statuses, priorities, labels, users, stages, snippet types   |
|  [12]   | `IProject`           | review | project-identity marker (name + project guid)                                                       |
|  [13]   | `IDocumentInfo`      | review | BCF 3.0 document-library marker                                                                     |

[PUBLIC_TYPE_SCOPE]: concrete schema object graphs
- package: `Smino.Bcf.Toolkit`
- namespace: `BcfToolkit.Model.Bcf30`, `BcfToolkit.Model.Bcf21`
- rail: review

| [INDEX] | [SYMBOL]                              | [RAIL] | [CAPABILITY]                                                                                            |
| :-----: | :------------------------------------ | :----- | :----------------------------------------------------------------------------------------------------- |
|  [01]   | `Bcf30.Bcf`                           | review | BCF 3.0 root: `ConcurrentBag<Markup> Markups`, `DocumentInfo? Document`, `Extensions`, `ProjectInfo? Project`, `Version` |
|  [02]   | `Bcf21.Bcf`                           | review | BCF 2.1 root: the same shape minus the 3.0 document library                                             |
|  [03]   | `Bcf30.Markup` / `Bcf21.Markup`       | review | concrete markup: `Topic` + `Comment` list + `ViewPoint` list                                            |
|  [04]   | `Bcf30.Topic` / `Bcf21.Topic`         | review | concrete issue record; status/type/priority/labels/assignee/dates/related-topics/document-references     |
|  [05]   | `Bcf30.Comment` / `Bcf21.Comment`     | review | concrete threaded comment; `CommentViewpoint` back-reference                                            |
|  [06]   | `Bcf30.VisualizationInfo`             | review | concrete saved view: `OrthogonalCamera`/`PerspectiveCamera`, `Components`, lines, clipping planes, bitmaps |
|  [07]   | `Bcf30.Components` / `ComponentSelection` / `ComponentVisibility` | review | the selection/visibility/coloring component sets inside a viewpoint                       |
|  [08]   | `Bcf30.Point` / `Direction`           | review | camera position/direction/up vectors (the geometry of `OrthogonalCamera`/`PerspectiveCamera`)          |
|  [09]   | `Bcf30.Extensions`                    | review | concrete project extensions (the enum vocabularies the `SignOff` state machine and topic-type axis read) |

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: Worker — round-trip conversion
- package: `Smino.Bcf.Toolkit`
- namespace: `BcfToolkit`
- rail: review

| [INDEX] | [SURFACE]            | [CALL_SHAPE]                                                          | [CAPABILITY]                                                  |
| :-----: | :------------------- | :------------------------------------------------------------------- | :----------------------------------------------------------- |
|  [01]   | `Worker.Convert`     | `(string source, string target)` → `Task`                           | path→path conversion auto-detecting source/target by extension/version |
|  [02]   | `Worker.BcfFromStream` | `(Stream stream)` → `Task<Bcf>`                                    | reads a `.bcfzip` stream into the typed `Bcf` object root     |
|  [03]   | `Worker.ToBcf`       | `(IBcf bcf, string target)` → `Task`                                | writes a model to a `.bcfzip` file                           |
|  [04]   | `Worker.ToBcf`       | `(IBcf bcf, BcfVersionEnum targetVersion)` → `Task<Stream>`         | serializes to an in-memory `.bcfzip` stream at a chosen version |
|  [05]   | `Worker.ToBcf`       | `(IBcf bcf, BcfVersionEnum targetVersion, CancellationToken)` → `Task<Stream>` | cancellable stream serialization                  |
|  [06]   | `Worker.ToBcf`       | `(IBcf bcf, BcfVersionEnum targetVersion, Stream stream[, CancellationToken])` | synchronous write into a caller-owned stream       |
|  [07]   | `Worker.ToJson`      | `(IBcf bcf, string target)` → `Task`                                | writes the BCF-API JSON resource model to a folder/file      |

[ENTRYPOINT_SCOPE]: BcfExtensions — streaming part parse
- package: `Smino.Bcf.Toolkit`
- namespace: `BcfToolkit.Utils`
- rail: review

`BcfExtensions` exposes the per-part stream parse beneath `Worker` for callers
that read a single container member without a full deserialize.

| [INDEX] | [SURFACE]                              | [CALL_SHAPE]                                                            | [CAPABILITY]                                            |
| :-----: | :------------------------------------- | :--------------------------------------------------------------------- | :---------------------------------------------------- |
|  [01]   | `BcfExtensions.GetVersionFromStreamArchive` | `(Stream stream)` → `Task<BcfVersionEnum?>`                       | sniffs the container version before a typed read       |
|  [02]   | `BcfExtensions.ParseMarkups<TMarkup, TVisualizationInfo>` | `(Stream)` → `Task<ConcurrentBag<TMarkup>>`      | streams every markup+viewpoint out of the archive      |
|  [03]   | `BcfExtensions.ParseExtensions<TExtensions>` | `(Stream)` → `Task<TExtensions>`                              | parses the project extension vocabularies              |
|  [04]   | `BcfExtensions.ParseProject<TProjectInfo>` | `(Stream)` → `Task<TProjectInfo?>`                             | parses the project identity member                     |
|  [05]   | `BcfExtensions.ParseDocuments<TDocumentInfo>` | `(Stream)` → `Task<TDocumentInfo?>`                          | parses the BCF 3.0 document library member             |

[ENTRYPOINT_SCOPE]: fluent builders — authoring BCF from scratch
- package: `Smino.Bcf.Toolkit`
- namespace: `BcfToolkit.Builder.Bcf30`, `BcfToolkit.Builder.Bcf21`
- rail: review

The `Builder.Bcf30`/`Builder.Bcf21` namespaces are the fluent authoring surface
the `ClashProposal`→`BcfTopic` fold composes; every builder implements
`IBuilder<T>.Build()` / `IDefaultBuilder<T>.WithDefaults()` and the root also
`IFromStreamBuilder<T>.BuildFromStream(Stream)`. Nested `Action<TBuilder>`
closures author the markup/topic/comment/component/camera tree without
materializing a partial object graph by hand.

| [INDEX] | [SURFACE]                  | [CALL_SHAPE]                                          | [CAPABILITY]                                                         |
| :-----: | :------------------------- | :--------------------------------------------------- | :----------------------------------------------------------------- |
|  [01]   | `BcfBuilder.AddMarkup`     | `(Action<MarkupBuilder> builder)` → `BcfBuilder`     | adds one authored topic markup (chainable); `AddMarkups(List<Markup>, bool update)` bulk form |
|  [02]   | `BcfBuilder.SetProject` / `SetExtensions` / `SetDocument` | `(Action<…Builder>)` or `(concrete)` → `BcfBuilder` | sets project identity / extension vocabularies / document library |
|  [03]   | `BcfBuilder.WithDefaults`  | `()` → `BcfBuilder`                                  | seeds default extension vocabularies                               |
|  [04]   | `BcfBuilder.Build`         | `()` → `Bcf`                                         | materializes the typed `IBcf` root for `Worker.ToBcf`              |
|  [05]   | `BcfBuilder.BuildFromStream` | `(Stream source)` → `Task<Bcf>`                    | reads an existing `.bcfzip` stream into a builder for edit-then-`Build` |
|  [06]   | `MarkupBuilder` / `CommentBuilder` / `ComponentBuilder` / `CameraBuilder` / `ViewPointBuilder` | `Action<TBuilder>` leaves | per-node fluent builders nested inside `AddMarkup` |

## [04]-[IMPLEMENTATION_LAW]

[IDENTITY_PROFILE]:
- namespace: `BcfToolkit`
- converter root: `Worker` (sink-shape + version overloaded)
- model root: `IBcf` discriminated over `Bcf30.Bcf` / `Bcf21.Bcf`
- version axis: `BcfVersionEnum` (`Bcf21` = 2.1, `Bcf30` = 3.0; no 2.0 parse path)
- receipt root: source version, topic count, and target serialization shape

[CODEC_COMPOSE]:
- version-agnostic projection: the host-neutral `BcfTopic`/`BcfComment`/`BcfViewpoint` records project from `IBcf`/`ITopic`/`IComment`/`IViewPoint`, NOT from a concrete `Bcf30`/`Bcf21` type, so the projection fold is written once and `BcfVersionEnum` selects only the read/write asset — the discriminated interface is the collapse point that keeps the COORDINATION owner free of per-version arms.
- one converter, three sinks: `Worker.ToBcf` discriminates path vs. in-memory `Stream` vs. caller-owned `Stream`, and async vs. sync, on overload — the design composes the cancellable `Task<Stream>` form under the structured-concurrency rail and folds any throw to `BcfFault.CodecReject` via the `Fin<T>` boundary, never letting a `Worker` exception escape the codec seam.
- version migration: `Worker.ToBcf(bcf, Bcf30)` over a `Bcf21`-sourced `IBcf` is the 2.1→3.0 up-conversion the COORDINATION board uses to normalize mixed-version issue exchange to a single internal generation.
- streaming sniff-then-read: `BcfExtensions.GetVersionFromStreamArchive` selects the version before `ParseMarkups`/`ParseExtensions` materialize only the needed container members — the ISSUES import avoids a full object-graph deserialize when it needs only the topic roster.
- review-rail stack: the BCF `Extensions` status/type/priority vocabularies feed the `SignOff` `[SmartEnum]` BCF state machine in COORDINATION; the topic GUID joins a `BcfTopic` to the `ElementSet` it references through `IComponent` IFC GUIDs, and the `ClashProposal` fold authors a `BcfTopic` from `Model/systems#INTERFERENCE` clash evidence via `BcfBuilder.AddMarkup(...)` → `Build()` → `Worker.ToBcf(bcf, Bcf30)`. The same topic-author fold also lands an IDS audit failure on the board: the `api-ids-lib` `Audit` (over the `Xbim.InformationSpecifications` spec model) emits `BufferedValidationIssue` rows whose `Line`/`Position`/`Message` author one `BcfTopic`/`BcfComment` per non-conformance (the provenance carried as topic/comment free-text — BCF has no structured line column). So BCF is the issue-exchange wire and IDS is the requirement-audit wire, meeting at TWO seams: the shared `Model/query#ELEMENT_SET` `ElementPredicate` algebra at read time, and the audit-failure-to-`BcfTopic` handoff at write time.

[LOCAL_ADMISSION]:
- `Smino.Bcf.Toolkit` reads and writes BCF containers and the BCF-API JSON model only; it carries no issue-board lifecycle, no clash detection, and no IFC model graph.
- The BCF state machine, the clash→topic authoring fold, and the `ElementSet` join are COORDINATION domain concerns, never the codec's.
- Source `BcfVersionEnum`, topic count, and target serialization shape are receipt facts the ISSUES/COORDINATION fold records.
- The bundled `System.CommandLine` binder + `Serilog` sink are the package's own CLI tool surface; the design composes `Worker`/`BcfExtensions` directly and never invokes the CLI.

[RAIL_LAW]:
- Package: `Smino.Bcf.Toolkit`
- Owns: BCF 2.1/3.0 container and BCF-API JSON round-trip
- Accept: the `.bcfzip` ISSUES codec and the COORDINATION issue-exchange wire
- Reject: issue-board lifecycle/state, clash detection, IFC model graph, the bundled CLI host
