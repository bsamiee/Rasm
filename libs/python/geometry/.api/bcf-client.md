# [PY_GEOMETRY_API_BCF_CLIENT]

`bcf-client` (import `bcf`) supplies BCF (BIM Collaboration Format) v2/v3 file authoring, reading, and REST API client access for the IFC-analysis rail: a `bcf.bcfxml.load` version-detecting loader, versioned `BcfXml` document roots that own the topic/comment/viewpoint lifecycle, `TopicHandler` and `VisualizationInfoHandler` for issue and viewpoint authoring, and the v3 `BcfClient`/`FoundationClient` REST surface. The package owner composes `load` plus the versioned `create_new` factories and the viewpoint handlers into the BCF exchange leg of the IFC-analysis owner; it never hand-rolls BCF zip construction or BCF-XML serialization.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `bcf-client`
- package: `bcf-client`
- import: `import bcf`
- owner: `geometry`
- rail: ifc-companion / bcf-exchange
- installed: `0.8.5` reflected via `import bcf` on cp313
- entry points: none (library only)
- capability: BCF v2/v3 file read/write, schema-version detection, topic/comment lifecycle, viewpoint authoring with camera/selection/visibility, project and extension metadata, and a v3 REST client for the BCF API

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: document root family
- rail: bcf-exchange

`bcf.bcfxml.load` resolves to the versioned `BcfXml`; `BcfXml2`/`BcfXml3` are re-exports of the versioned classes — import the versioned forms for explicit typing.

| [INDEX] | [SYMBOL]               | [TYPE_FAMILY] | [CAPABILITY]                    |
| :-----: | :--------------------- | :------------ | :------------------------------ |
|  [01]   | `bcf.bcfxml.BcfXml2`   | v2 alias      | schema alias resolved by `load` |
|  [02]   | `bcf.bcfxml.BcfXml3`   | v3 alias      | schema alias resolved by `load` |
|  [03]   | `bcf.v2.bcfxml.BcfXml` | v2 document   | full v2 BCF file lifecycle      |
|  [04]   | `bcf.v3.bcfxml.BcfXml` | v3 document   | full v3 BCF file lifecycle      |

[PUBLIC_TYPE_SCOPE]: topic and viewpoint handlers
- rail: bcf-exchange

| [INDEX] | [SYMBOL]                                  | [TYPE_FAMILY] | [CAPABILITY]                      |
| :-----: | :---------------------------------------- | :------------ | :-------------------------------- |
|  [01]   | `bcf.v2.topic.TopicHandler`               | issue handler | v2 topic comments plus viewpoints |
|  [02]   | `bcf.v3.topic.TopicHandler`               | issue handler | v3 topic comments plus viewpoints |
|  [03]   | `bcf.v2.visinfo.VisualizationInfoHandler` | viewpoint     | v2 camera, selection, visibility  |
|  [04]   | `bcf.v3.visinfo.VisualizationInfoHandler` | viewpoint     | v3 camera, selection, visibility  |

[PUBLIC_TYPE_SCOPE]: v2 and v3 markup model family
- rail: bcf-exchange schema

The v3 markup adds `DocumentReference`; both versions share `Topic`/`Comment`/`Markup`/`ViewPoint`/`Header`/`BimSnippet`.

| [INDEX] | [SYMBOL]                                                    | [TYPE_FAMILY] | [CAPABILITY]              |
| :-----: | :---------------------------------------------------------- | :------------ | :------------------------ |
|  [01]   | `v2.model.markup.Topic` / `v3.model.markup.Topic`           | data class    | issue metadata            |
|  [02]   | `v2.model.markup.Comment` / `v3.model.markup.Comment`       | data class    | issue comment             |
|  [03]   | `v2.model.markup.Markup` / `v3.model.markup.Markup`         | data class    | topic container           |
|  [04]   | `v2.model.markup.ViewPoint` / `v3.model.markup.ViewPoint`   | data class    | viewpoint reference       |
|  [05]   | `v2.model.markup.Header` / `v3.model.markup.Header`         | data class    | IFC file header reference |
|  [06]   | `v2.model.markup.BimSnippet` / `v3.model.markup.BimSnippet` | data class    | linked BIM snippet        |
|  [07]   | `v3.model.markup.DocumentReference`                         | data class    | linked document reference |

[PUBLIC_TYPE_SCOPE]: v3 REST API family — `bcf.v3.bcfapi`
- rail: bcf-exchange rest

| [INDEX] | [SYMBOL]           | [TYPE_FAMILY] | [CAPABILITY]                             |
| :-----: | :----------------- | :------------ | :--------------------------------------- |
|  [01]   | `BcfClient`        | REST client   | BCF API v3 topic/comment/viewpoint ops   |
|  [02]   | `FoundationClient` | REST client   | BCF API foundation auth/version services |
|  [03]   | `OAuthReceiver`    | auth helper   | OAuth2 local callback receiver           |

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: BCF file I/O
- rail: bcf-exchange

`bcf.bcfxml.load` detects the schema version from the `bcf.version` file in the zip and returns the matching versioned `BcfXml`; the versioned `create_new` factories author a fresh document.

| [INDEX] | [SURFACE]                                                                                         | [ENTRY_FAMILY] | [CAPABILITY]                     |
| :-----: | :------------------------------------------------------------------------------------------------ | :------------- | :------------------------------- |
|  [01]   | `bcf.bcfxml.load(filepath: Path, xml_handler=None) -> BcfXml \| None`                             | loader         | open BCF file, detect schema     |
|  [02]   | `BcfXml.load(filename: Path, xml_handler=None) -> BcfXml \| None`                                 | loader         | open BCF into a version instance |
|  [03]   | `bcf.v2.bcfxml.BcfXml.create_new(project_name=None, xml_handler=None) -> BcfXml`                  | factory        | new v2 BCF document              |
|  [04]   | `bcf.v3.bcfxml.BcfXml.create_new(project_name=None, extensions=None, xml_handler=None) -> BcfXml` | factory        | new v3 BCF document              |
|  [05]   | `BcfXml.save(filename: Path \| None=None, keep_open: bool=False) -> None`                         | serializer     | write BCF archive to disk        |
|  [06]   | `BcfXml.close() -> None`                                                                          | lifecycle      | close and release zip resources  |

[ENTRYPOINT_SCOPE]: topic and comment lifecycle
- rail: bcf-exchange

`add_topic` returns a `TopicHandler`; comment and project mutations accumulate in memory until `save`.

| [INDEX] | [SURFACE]                                                                                      | [ENTRY_FAMILY] | [CAPABILITY]                    |
| :-----: | :--------------------------------------------------------------------------------------------- | :------------- | :------------------------------ |
|  [01]   | `BcfXml.add_topic(title, description, author, topic_type='', topic_status='') -> TopicHandler` | mutator        | author a new topic              |
|  [02]   | `BcfXml.get_topics() -> dict[str, TopicHandler]`                                               | query          | all topics keyed by GUID        |
|  [03]   | `BcfXml.get_topic(guid: str) -> TopicHandler`                                                  | query          | one topic by GUID               |
|  [04]   | `BcfXml.add_comment(_topic, _comment=None) -> None`                                            | mutator        | append a comment to a topic     |
|  [05]   | `BcfXml.edit_comment() -> None`                                                                | mutator        | edit an existing comment        |
|  [06]   | `BcfXml.get_project()` / `BcfXml.edit_project(...)` / `BcfXml.save_project(...)`               | project        | project metadata read and write |
|  [07]   | `BcfXml.documents`                                                                             | property (v3)  | v3 `DocumentsHandler` access    |

[ENTRYPOINT_SCOPE]: TopicHandler and viewpoint operations
- rail: bcf-exchange

`VisualizationInfoHandler` authors the camera, selection, and visibility of a viewpoint; `TopicHandler` binds viewpoints to a topic.

| [INDEX] | [SURFACE]                                                                                                              | [ENTRY_FAMILY] | [CAPABILITY]                             |
| :-----: | :--------------------------------------------------------------------------------------------------------------------- | :------------- | :--------------------------------------- |
|  [01]   | `TopicHandler.add_viewpoint(element) -> VisualizationInfoHandler`                                                      | mutator        | attach an element viewpoint to the topic |
|  [02]   | `TopicHandler.add_viewpoint_from_point_and_guids(position, *guids) -> VisualizationInfoHandler`                        | mutator        | viewpoint from a camera point plus GUIDs |
|  [03]   | `TopicHandler.topic` / `comments` / `viewpoints` / `guid`                                                              | property       | topic, comments, viewpoints, GUID        |
|  [04]   | `TopicHandler.save(destination_zip) -> None`                                                                           | serializer     | persist the topic into the BCF zip       |
|  [05]   | `VisualizationInfoHandler.create_new(element, xml_handler=None) -> VisualizationInfoHandler`                           | factory        | viewpoint from an element                |
|  [06]   | `VisualizationInfoHandler.create_from_point_and_guids(position, *guids, xml_handler=None) -> VisualizationInfoHandler` | factory        | viewpoint from camera point plus GUIDs   |
|  [07]   | `VisualizationInfoHandler.set_selected_elements(elements) -> None`                                                     | mutator        | mark selected elements                   |
|  [08]   | `VisualizationInfoHandler.set_visible_elements(elements)` / `set_hidden_elements(elements) -> None`                    | mutator        | element visibility                       |
|  [09]   | `VisualizationInfoHandler.get_selected_guids() -> list[str] \| None`                                                   | query          | selected element GUIDs                   |
|  [10]   | `VisualizationInfoHandler.load(topic_dir, vpt, xml_handler=None)` / `save(bcf_zip, topic_dir, vpt) -> None`            | I/O            | viewpoint read and write                 |

[ENTRYPOINT_SCOPE]: v3 REST API — `bcf.v3.bcfapi`
- rail: bcf-exchange rest

`BcfClient` carries CRUD operations over BCF API v3 resources; `FoundationClient` carries auth and version negotiation; `OAuthReceiver` services the OAuth2 callback.

| [INDEX] | [SURFACE]                                                                                         | [ENTRY_FAMILY] | [CAPABILITY]                         |
| :-----: | :------------------------------------------------------------------------------------------------ | :------------- | :----------------------------------- |
|  [01]   | `BcfClient.get_projects()` / `get_project()`                                                      | query          | project listing and detail           |
|  [02]   | `BcfClient.get_topics()` / `get_topic()` / `create_topic()` / `update_topic()` / `delete_topic()` | topic          | topic CRUD                           |
|  [03]   | `BcfClient.get_comments()` / `create_comments()` / `update_comment()` / `delete_comment()`        | comment        | comment CRUD                         |
|  [04]   | `BcfClient.get_viewpoints()` / `create_viewpoints()` / `delete_viewpoint()`                       | viewpoint      | viewpoint CRUD                       |
|  [05]   | `BcfClient.get_snapshot()` / `get_bitmap()` / `get_selection()` / `get_visibility()`              | media          | viewpoint media and state            |
|  [06]   | `FoundationClient.login()` / `get_access_token()` / `get_new_access_token()`                      | auth           | OAuth2 token lifecycle               |
|  [07]   | `FoundationClient.get_auth_methods()` / `get_versions()`                                          | negotiate      | server auth methods and API versions |

## [04]-[IMPLEMENTATION_LAW]

[BCF_TOPOLOGY]:
- import: `import bcf` at boundary scope only; module-level import is banned by the manifest import policy. The distribution is `bcf-client`; the top-level package is `bcf`, never `bcfxml`/`bcfapi`.
- subpackages: `bcfxml` (version dispatch), `v2` and `v3` (per-schema implementations with `bcfxml`/`model`/`topic`/`visinfo`, plus `document`/`bcfapi` on v3), `xml_parser`, `extensions`, `geometry`, `inmemory_zipfile`.
- version dispatch: `bcf.bcfxml.load` reads the `bcf.version` entry in the zip and returns either `bcf.v2.bcfxml.BcfXml` or `bcf.v3.bcfxml.BcfXml`; `BcfXml2`/`BcfXml3` are re-exports — import the versioned classes for explicit typing.
- v3 additions: `BcfXml.documents` exposes the `DocumentsHandler`; v2 carries `extension_schema` and `extensions` instead.
- parser: the `xml_handler` parameter accepts any `AbstractXmlParserSerializer` implementation for custom BCF-XML parsing or serialization.

[BCF_EXCHANGE]:
- in-memory mutation: `add_topic`, `add_comment`, and viewpoint mutations accumulate in memory; call `BcfXml.save` to flush the BCF archive to disk.
- factory split: `TopicHandler.create_new` is the in-package factory; `BcfXml.add_topic` is the document-level factory that wires the topic into the markup tree.
- REST surface: `BcfClient` and `FoundationClient` live in `bcf.v3.bcfapi` and require HTTP credentials; `OAuthReceiver` services the local OAuth2 callback.

## [05]-[LOCAL_ADMISSION]

[RAIL_LAW]:
- Package: `bcf-client`
- Owns: BCF v2/v3 file I/O, schema-version detection, topic/comment/viewpoint lifecycle, element selection and visibility in viewpoints, and the v3 BCF API REST client
- Accept: `Path` for file I/O; `ifcopenshell` GUIDs for element selection in viewpoints; HTTP credentials for the REST surface
- Reject: manual BCF zip construction or BCF-XML generation outside this package; wrapper-renames of `load`/`create_new`/`save`; a hand-rolled BCF REST client where `bcf.v3.bcfapi` is admitted

[CAPTURE_GAP]:
- floor: `bcf-client 0.8.5` ships a pure-Python `py3-none-any` wheel and stays in the IFC-analysis companion lane alongside `ifcopenshell`; it installs on any interpreter, and reflection ran on a cp313 companion interpreter
- members: verified by introspection against the installed distribution; the top-level `bcf` package, the version-dispatch `load`, the v2/v3 `BcfXml` lifecycle, the handler classes, and the `bcf.v3.bcfapi` client surface resolve against the live module — no phantom
