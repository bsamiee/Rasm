# [PY_GEOMETRY_API_BCF]

`bcf` (dist `bcf-client`) supplies BCF (BIM Collaboration Format) v2/v3 file authoring, reading, and REST API client access: it exposes `BcfXml` document roots for both schema versions, `TopicHandler` for issue lifecycle management, `VisualizationInfoHandler` for viewpoint authoring, and a v3 `BcfClient`/`FoundationClient` REST surface.

## [1]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `bcf-client`
- package: `bcf-client`
- module: `bcf`
- asset: runtime library
- rail: ifc-companion / bcf-exchange

## [2]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: document root family
- rail: bcf-exchange

| [INDEX] | [SYMBOL]               | [TYPE_FAMILY] | [RAIL]                           |
| :-----: | :--------------------- | :------------ | :------------------------------- |
|   [1]   | `BcfXml` (base)        | abstract root | shared `copy_with` base          |
|   [2]   | `BcfXml2`              | v2 document   | schema alias; resolved by `load` |
|   [3]   | `BcfXml3`              | v3 document   | schema alias; resolved by `load` |
|   [4]   | `bcf.v2.bcfxml.BcfXml` | v2 impl       | full v2 BCF file lifecycle       |
|   [5]   | `bcf.v3.bcfxml.BcfXml` | v3 impl       | full v3 BCF file lifecycle       |

[PUBLIC_TYPE_SCOPE]: topic and viewpoint handlers
- rail: bcf-exchange

| [INDEX] | [SYMBOL]                                  | [TYPE_FAMILY] | [RAIL]                             |
| :-----: | :---------------------------------------- | :------------ | :--------------------------------- |
|   [1]   | `bcf.v2.topic.TopicHandler`               | issue handler | v2 topic comments + viewpoints     |
|   [2]   | `bcf.v3.topic.TopicHandler`               | issue handler | v3 topic comments + viewpoints     |
|   [3]   | `bcf.v2.visinfo.VisualizationInfoHandler` | viewpoint     | v2 camera + selection + visibility |
|   [4]   | `bcf.v3.visinfo.VisualizationInfoHandler` | viewpoint     | v3 camera + selection + visibility |

[PUBLIC_TYPE_SCOPE]: v2 model family
- rail: bcf-exchange schema

| [INDEX] | [SYMBOL]                     | [TYPE_FAMILY] | [RAIL]                    |
| :-----: | :--------------------------- | :------------ | :------------------------ |
|   [1]   | `v2.model.markup.Topic`      | data class    | issue metadata            |
|   [2]   | `v2.model.markup.Comment`    | data class    | issue comment             |
|   [3]   | `v2.model.markup.Markup`     | data class    | topic container           |
|   [4]   | `v2.model.markup.ViewPoint`  | data class    | viewpoint reference       |
|   [5]   | `v2.model.markup.Header`     | data class    | IFC file header reference |
|   [6]   | `v2.model.markup.BimSnippet` | data class    | linked BIM snippet        |

[PUBLIC_TYPE_SCOPE]: v3 model family
- rail: bcf-exchange schema

| [INDEX] | [SYMBOL]                            | [TYPE_FAMILY] | [RAIL]                    |
| :-----: | :---------------------------------- | :------------ | :------------------------ |
|   [1]   | `v3.model.markup.Topic`             | data class    | issue metadata            |
|   [2]   | `v3.model.markup.Comment`           | data class    | issue comment             |
|   [3]   | `v3.model.markup.Markup`            | data class    | topic container           |
|   [4]   | `v3.model.markup.ViewPoint`         | data class    | viewpoint reference       |
|   [5]   | `v3.model.markup.Header`            | data class    | IFC file header reference |
|   [6]   | `v3.model.markup.DocumentReference` | data class    | linked document reference |
|   [7]   | `v3.model.markup.BimSnippet`        | data class    | linked BIM snippet        |

[PUBLIC_TYPE_SCOPE]: v3 REST API family
- rail: bcf-exchange rest

| [INDEX] | [SYMBOL]           | [TYPE_FAMILY] | [RAIL]                              |
| :-----: | :----------------- | :------------ | :---------------------------------- |
|   [1]   | `BcfClient`        | REST client   | BCF API v3 topic/comment operations |
|   [2]   | `FoundationClient` | REST client   | BCF API foundation services         |
|   [3]   | `OAuthReceiver`    | auth helper   | OAuth2 local callback receiver      |

## [3]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: BCF file I/O
- rail: bcf-exchange

| [INDEX] | [SURFACE]                                                                                         | [ENTRY_FAMILY] | [RAIL]                              |
| :-----: | :------------------------------------------------------------------------------------------------ | :------------- | :---------------------------------- |
|   [1]   | `bcf.bcfxml.load(filepath: Path, xml_handler=None) -> BcfXml2 \| BcfXml3`                         | loader         | open BCF file, detect schema        |
|   [2]   | `bcf.v2.bcfxml.BcfXml.create_new(project_name=None, xml_handler=None) -> BcfXml`                  | factory        | new v2 BCF document                 |
|   [3]   | `bcf.v3.bcfxml.BcfXml.create_new(project_name=None, extensions=None, xml_handler=None) -> BcfXml` | factory        | new v3 BCF document                 |
|   [4]   | `BcfXml.load(filename: Path, xml_handler=None) -> Optional[BcfXml]`                               | loader         | open BCF file into version instance |
|   [5]   | `BcfXml.save(filename: Path=None, keep_open: bool=False) -> None`                                 | serializer     | write BCF archive to disk           |
|   [6]   | `BcfXml.close() -> None`                                                                          | lifecycle      | close and release zip resources     |

[ENTRYPOINT_SCOPE]: topic lifecycle
- rail: bcf-exchange

| [INDEX] | [SURFACE]                                                                                      | [ENTRY_FAMILY] |
| :-----: | :--------------------------------------------------------------------------------------------- | :------------- |
|   [1]   | `BcfXml.add_topic(title, description, author, topic_type='', topic_status='') -> TopicHandler` | mutator        |
|   [2]   | `BcfXml.get_topics() -> dict[str, TopicHandler]`                                               | query          |
|   [3]   | `BcfXml.get_topic(guid: str) -> TopicHandler`                                                  | query          |
|   [4]   | `BcfXml.edit_topic() -> None`                                                                  | mutator        |
|   [5]   | `BcfXml.add_comment(_topic, _comment=None) -> None`                                            | mutator        |
|   [6]   | `BcfXml.edit_comment(_topic, _comment) -> None`                                                | mutator        |
|   [7]   | `BcfXml.get_project() -> ...`                                                                  | query          |
|   [8]   | `BcfXml.edit_project(...) -> None`                                                             | mutator        |
|   [9]   | `BcfXml.save_project(...) -> None`                                                             | mutator        |
|  [10]   | `BcfXml.get_version() -> ...`                                                                  | query          |
|  [11]   | `BcfXml.edit_version(...) -> None`                                                             | mutator        |

[ENTRYPOINT_SCOPE]: TopicHandler operations
- rail: bcf-exchange

| [INDEX] | [SURFACE]                                                               | [ENTRY_FAMILY] |
| :-----: | :---------------------------------------------------------------------- | :------------- |
|   [1]   | `TopicHandler.add_viewpoint(viewpoint_file) -> None`                    | mutator        |
|   [2]   | `TopicHandler.add_viewpoint_from_point_and_guids(point, guids) -> None` | mutator        |
|   [3]   | `TopicHandler.add_visinfo_handler(handler) -> None`                     | mutator        |
|   [4]   | `TopicHandler.topic`                                                    | property       |
|   [5]   | `TopicHandler.comments`                                                 | property       |
|   [6]   | `TopicHandler.viewpoints`                                               | property       |
|   [7]   | `TopicHandler.guid`                                                     | property       |
|   [8]   | `TopicHandler.save() -> None`                                           | serializer     |

[ENTRYPOINT_SCOPE]: VisualizationInfoHandler operations
- rail: bcf-exchange

| [INDEX] | [SURFACE]                                                            | [ENTRY_FAMILY] |
| :-----: | :------------------------------------------------------------------- | :------------- |
|   [1]   | `VisualizationInfoHandler.create_new() -> VisualizationInfoHandler`  | factory        |
|   [2]   | `VisualizationInfoHandler.create_from_point_and_guids(point, guids)` | factory        |
|   [3]   | `VisualizationInfoHandler.from_topic_viewpoints(topic, viewpoints)`  | factory        |
|   [4]   | `VisualizationInfoHandler.set_selected_elements(guids)`              | mutator        |
|   [5]   | `VisualizationInfoHandler.set_hidden_elements(guids)`                | mutator        |
|   [6]   | `VisualizationInfoHandler.set_visible_elements(guids)`               | mutator        |
|   [7]   | `VisualizationInfoHandler.set_visibility(visible: bool)`             | mutator        |
|   [8]   | `VisualizationInfoHandler.get_selected_guids() -> list[str]`         | query          |
|   [9]   | `VisualizationInfoHandler.get_elements_visibility()`                 | query          |
|  [10]   | `VisualizationInfoHandler.load(path) -> VisualizationInfoHandler`    | I/O            |
|  [11]   | `VisualizationInfoHandler.save(path) -> None`                        | I/O            |

## [4]-[IMPLEMENTATION_LAW]

[BCF_TOPOLOGY]:
- subpackages: `bcfxml` (version-dispatch), `v2` (v2 impl), `v3` (v3 impl), `xml_parser`, `extensions`, `geometry`, `inmemory_zipfile`
- `bcf.bcfxml.load` detects schema version from the `bcf.version` file inside the zip and returns either `v2.BcfXml` or `v3.BcfXml`
- `BcfXml2`/`BcfXml3` are re-exports of the versioned `BcfXml` classes; import the versioned forms for explicit typing
- `xml_handler` parameter accepts any `AbstractXmlParserSerializer` implementation for custom XML parsing
- v3 adds `documents` property on `BcfXml` for `DocumentsHandler`; v2 has `extension_schema` and `extensions`
- `TopicHandler.create_new` is the in-package factory; `BcfXml.add_topic` is the document-level factory

[LOCAL_ADMISSION]:
- Top-level `bcf.bcfxml.load` for reading any BCF file; use versioned `BcfXml.create_new` for explicit version authoring
- All mutations accumulate in memory; call `BcfXml.save` to flush to disk
- `BcfClient` and `FoundationClient` are in `bcf.v3.bcfapi` and require HTTP credentials

[RAIL_LAW]:
- Package: `bcf-client`
- Owns: BCF v2/v3 file I/O, topic/comment/viewpoint lifecycle, element selection/visibility in viewpoints
- Accept: `Path` for file I/O; `ifcopenshell` GUIDs for element selection in viewpoints
- Reject: manual BCF zip construction or XML generation outside this package
