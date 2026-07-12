# [PY_GEOMETRY_API_BCF_CLIENT]

`bcf-client` (import `bcf`) supplies BCF (BIM Collaboration Format) v2/v3 file authoring, reading, and REST API client access for the IFC-analysis rail: a `bcf.bcfxml.load` version-detecting loader, versioned `BcfXml` document roots that own the topic/comment/viewpoint lifecycle, `TopicHandler` and `VisualizationInfoHandler` for issue and viewpoint authoring, and the v3 `BcfClient`/`FoundationClient` REST surface. It stacks directly on `ifcopenshell`: `TopicHandler.add_viewpoint(element)` and `VisualizationInfoHandler.set_selected_elements/set_visible_elements/set_hidden_elements` take `ifcopenshell.entity_instance` products and resolve their IFC GlobalIds into the viewpoint's selection/visibility GUID lists, so the BCF exchange leg authors coordination issues from the same IFC model the `ifc/analysis` clash/IDS run produces. The package owner composes `load` plus the versioned `create_new` factories and the viewpoint handlers into the BCF exchange leg of the IFC-analysis owner; it never hand-rolls BCF zip construction or BCF-XML serialization (the XML round-trip is owned by `xsdata` through the `AbstractXmlParserSerializer` handler).

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `bcf-client`
- package: `bcf-client`
- import: `import bcf`
- owner: `geometry`
- rail: ifc-companion / bcf-exchange
- installed: `0.8.5`
- license: GPLv3 (published metadata classifier `GNU General Public License v3`; the in-tree source headers declare LGPLv3 — treat the published GPLv3 classifier as the binding admission flag, the strongest copyleft reading)
- depends: `xsdata>=24.4` (BCF-XML bind/parse), `numpy`, `ifcopenshell` (viewpoint element selection), `requests` (the v3 REST client)
- entry points: none (library only)
- capability: BCF v2/v3 file read/write, schema-version detection, topic/comment lifecycle, viewpoint authoring with camera/selection/visibility from IFC elements, project and extension metadata, and a v3 REST client covering projects/topics/comments/viewpoints/documents/events for the BCF API

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: document root family
- rail: bcf-exchange

`bcf.bcfxml.BcfXml` is a `Union[BcfXml2, BcfXml3]` type alias, not a class; `BcfXml2`/`BcfXml3` are re-exports of `bcf.v2.bcfxml.BcfXml`/`bcf.v3.bcfxml.BcfXml`. `bcf.bcfxml.load` reads the `bcf.version` zip entry, returns the matching versioned instance, and raises `ValueError` on an unsupported version (only `2.0`/`2.1`/`3.0` are accepted). Import the versioned classes for explicit typing.

| [INDEX] | [SYMBOL]               | [TYPE_FAMILY] | [CAPABILITY]                                    |
| :-----: | :--------------------- | :------------ | :---------------------------------------------- |
|  [01]   | `bcf.bcfxml.BcfXml`    | union alias   | `Union[BcfXml2, BcfXml3]` return type of `load` |
|  [02]   | `bcf.bcfxml.BcfXml2`   | v2 re-export  | re-export of `bcf.v2.bcfxml.BcfXml`             |
|  [03]   | `bcf.bcfxml.BcfXml3`   | v3 re-export  | re-export of `bcf.v3.bcfxml.BcfXml`             |
|  [04]   | `bcf.v2.bcfxml.BcfXml` | v2 document   | full v2 BCF file lifecycle                      |
|  [05]   | `bcf.v3.bcfxml.BcfXml` | v3 document   | full v3 BCF file lifecycle                      |

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

The markup models are `xsdata` dataclasses generated from the BCF XSD. The v3 markup adds `DocumentReference`; both versions share `Topic`/`Comment`/`Markup`/`ViewPoint`/`Header`/`BimSnippet`. `Topic` carries the `title: str`, `guid`, `topic_type`, `topic_status`, `creation_author`, `creation_date`, and `description` fields the issue-authoring rail reads (`TopicHandler.topic.title` is the canonical issue title accessor).

| [INDEX] | [SYMBOL]                                                    | [TYPE_FAMILY]  | [CAPABILITY]                                                                               |
| :-----: | :---------------------------------------------------------- | :------------- | :----------------------------------------------------------------------------------------- |
|  [01]   | `v2.model.markup.Topic` / `v3.model.markup.Topic`           | `xsdata` model | issue metadata: `title`/`guid`/`topic_type`/`topic_status`/`creation_author`/`description` |
|  [02]   | `v2.model.markup.Comment` / `v3.model.markup.Comment`       | `xsdata` model | issue comment                                                                              |
|  [03]   | `v2.model.markup.Markup` / `v3.model.markup.Markup`         | `xsdata` model | topic container                                                                            |
|  [04]   | `v2.model.markup.ViewPoint` / `v3.model.markup.ViewPoint`   | `xsdata` model | viewpoint reference                                                                        |
|  [05]   | `v2.model.markup.Header` / `v3.model.markup.Header`         | `xsdata` model | IFC file header reference                                                                  |
|  [06]   | `v2.model.markup.BimSnippet` / `v3.model.markup.BimSnippet` | `xsdata` model | linked BIM snippet                                                                         |
|  [07]   | `v3.model.markup.DocumentReference`                         | `xsdata` model | linked document reference                                                                  |

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

| [INDEX] | [SURFACE]                                                                                                                                   | [ENTRY_FAMILY] | [CAPABILITY]                                                                                                        |
| :-----: | :------------------------------------------------------------------------------------------------------------------------------------------ | :------------- | :------------------------------------------------------------------------------------------------------------------ |
|  [01]   | `TopicHandler.add_viewpoint(element: entity_instance) -> VisualizationInfoHandler`                                                          | mutator        | attach an element viewpoint to the topic                                                                            |
|  [02]   | `TopicHandler.add_viewpoint_from_point_and_guids(position, *guids) -> VisualizationInfoHandler`                                             | mutator        | viewpoint from a camera point plus GUIDs                                                                            |
|  [03]   | `TopicHandler.add_visinfo_handler(handler) -> None`                                                                                         | mutator        | bind a prepared `VisualizationInfoHandler`                                                                          |
|  [04]   | `TopicHandler.topic` / `markup` / `comments` / `viewpoints` / `header` / `bim_snippet` / `reference_files` / `guid`                         | property       | topic model (`.title`), markup, comments, viewpoint handlers, IFC header, snippet bytes, reference-file bytes, GUID |
|  [05]   | `TopicHandler.create_new(...)` / `save(destination_zip) -> None`                                                                            | factory / I/O  | in-package topic factory; persist topic into the BCF zip                                                            |
|  [06]   | `VisualizationInfoHandler.create_new(element, xml_handler=None) -> VisualizationInfoHandler`                                                | factory        | viewpoint from an element                                                                                           |
|  [07]   | `VisualizationInfoHandler.create_from_point_and_guids(position, *guids, xml_handler=None) -> VisualizationInfoHandler`                      | factory        | viewpoint from camera point plus GUIDs                                                                              |
|  [08]   | `VisualizationInfoHandler.from_topic_viewpoints(...)` / `load(topic_dir, vpt, xml_handler=None)` / `save(bcf_zip, topic_dir, vpt) -> None`  | factory / I/O  | reconstruct from markup viewpoints; viewpoint read and write                                                        |
|  [09]   | `VisualizationInfoHandler.set_selected_elements(elements: list[entity_instance]) -> None`                                                   | mutator        | mark selected IFC elements                                                                                          |
|  [10]   | `VisualizationInfoHandler.set_visible_elements(elements)` / `set_hidden_elements(elements)` / `set_visibility(default_visible, exceptions)` | mutator        | element visibility from IFC elements                                                                                |
|  [11]   | `VisualizationInfoHandler.get_selected_guids() -> list[str] \| None` / `get_elements_visibility() -> tuple[bool, list[str]] \| None`        | query          | selected GUIDs; default-visibility + exception GUIDs                                                                |

[ENTRYPOINT_SCOPE]: v3 REST API — `bcf.v3.bcfapi`
- rail: bcf-exchange rest

`BcfClient` is the polymorphic `requests`-backed REST surface: the typed CRUD methods compose over the `get`/`post`/`put`/`delete` HTTP primitives (carrying endpoint, params, and auth; GET returns `Any`, mutations return `(status, text)`). `FoundationClient` carries auth and version negotiation; `OAuthReceiver` is the `BaseHTTPRequestHandler` servicing the OAuth2 authorization-code callback.

| [INDEX] | [SURFACE]                                                                                                                                                               | [ENTRY_FAMILY] | [CAPABILITY]                                               |
| :-----: | :---------------------------------------------------------------------------------------------------------------------------------------------------------------------- | :------------- | :--------------------------------------------------------- |
|  [01]   | `BcfClient.get(endpoint, params=None, is_auth_required=False)` / `post(endpoint, data, params)` / `put(...)` / `delete(...)`                                            | http primitive | the polymorphic HTTP verbs every CRUD method composes over |
|  [02]   | `BcfClient.set_version(version)`                                                                                                                                        | config         | pin the API version dict                                   |
|  [03]   | `BcfClient.get_projects()` / `get_project(project_id)` / `update_project(project_id, data)` / `get_extensions(project_id)`                                              | project        | project listing/detail/update + extensions                 |
|  [04]   | `BcfClient.get_topics(...)` / `get_topic(project_id, topic_id)` / `create_topic(...)` / `update_topic(...)` / `delete_topic(...)`                                       | topic          | topic CRUD                                                 |
|  [05]   | `BcfClient.get_comments(...)` / `get_comment(...)` / `create_comments(...)` / `update_comment(...)` / `delete_comment(...)`                                             | comment        | comment CRUD                                               |
|  [06]   | `BcfClient.get_viewpoints(...)` / `get_viewpoint(...)` / `create_viewpoints(...)` / `delete_viewpoint(...)`                                                             | viewpoint      | viewpoint CRUD                                             |
|  [07]   | `BcfClient.get_snapshot(...)` / `get_bitmap(...)` / `get_selection(...)` / `get_coloring(...)` / `get_visibility(...)`                                                  | media          | viewpoint media and state                                  |
|  [08]   | `BcfClient.get_snippet(...)` / `update_snippet(...)` / `get_files_information(...)` / `get_files(...)` / `update_files(...)`                                            | snippet/files  | BIM snippet and referenced file transfer                   |
|  [09]   | `BcfClient.get_related_topics(...)` / `update_related_topics(...)`                                                                                                      | relations      | topic cross-references                                     |
|  [10]   | `BcfClient.get_document_references(...)` / `create_document_reference(...)` / `update_document_references(...)`                                                         | doc-refs       | topic document-reference CRUD                              |
|  [11]   | `BcfClient.get_documents(...)` / `get_document(...)` / `create_document(...)`                                                                                           | documents      | project document store                                     |
|  [12]   | `BcfClient.get_topics_events(...)` / `get_topic_events(...)` / `get_comments_events(...)` / `get_comment_events(...)`                                                   | events         | topic and comment audit events                             |
|  [13]   | `FoundationClient.login()` / `get_access_token()` / `get_refresh_token()` / `get_new_access_token()` / `set_auth_method(method)` / `set_tokens_from_response(response)` | auth           | OAuth2 authorization-code token lifecycle                  |
|  [14]   | `FoundationClient.get_auth_methods()` / `get_versions()`                                                                                                                | negotiate      | server auth methods and API versions                       |
|  [15]   | `OAuthReceiver.do_GET()`                                                                                                                                                | auth callback  | local HTTP handler capturing the OAuth2 redirect           |

## [04]-[IMPLEMENTATION_LAW]

[BCF_TOPOLOGY]:
- import: `import bcf` at boundary scope only; module-level import is banned by the manifest import policy. The distribution is `bcf-client`; the top-level package is `bcf`, never `bcfxml`/`bcfapi`.
- subpackages: `bcfxml` (version dispatch), `v2` and `v3` (per-schema implementations with `bcfxml`/`model`/`topic`/`visinfo`, plus `document`/`bcfapi` on v3), `agnostic` (version-neutral `extensions`/`model`/`topic`/`visinfo` shared logic), `xml_parser`, `extensions`, `geometry`, `inmemory_zipfile`.
- version dispatch: `bcf.bcfxml.load` reads the `bcf.version` entry in the zip and returns either `bcf.v2.bcfxml.BcfXml` or `bcf.v3.bcfxml.BcfXml`, raising `ValueError` for any version outside `{2.0, 2.1, 3.0}`; `BcfXml = Union[BcfXml2, BcfXml3]` is a type alias and `BcfXml2`/`BcfXml3` are re-exports — import the versioned classes for explicit typing.
- v3 additions: `BcfXml.documents` exposes the `DocumentsHandler`; v2 carries `extension_schema` and `extensions` instead.
- parser: the `xml_handler` parameter accepts any `AbstractXmlParserSerializer` implementation; the default `XmlParserSerializer` binds the BCF XSD models through `xsdata`, so BCF-XML parse/serialize is owned by `xsdata`, never hand-rolled.

[BCF_EXCHANGE]:
- in-memory mutation: `add_topic`, `add_comment`, and viewpoint mutations accumulate in memory; call `BcfXml.save` to flush the BCF archive to disk.
- factory split: `TopicHandler.create_new` is the in-package factory; `BcfXml.add_topic(title, description, author, topic_type="", topic_status="")` is the document-level factory that wires the topic into the markup tree and returns its `TopicHandler`.
- ifcopenshell stacking: `TopicHandler.add_viewpoint(element)` and the `VisualizationInfoHandler.set_*_elements`/`get_selected_guids` surface take and return `ifcopenshell.entity_instance` products and IFC GlobalId GUIDs, so a viewpoint authored from an `ifc/analysis` clash result selects the same elements by GlobalId without a parallel identity map.
- REST surface: `BcfClient`/`FoundationClient`/`OAuthReceiver` live in `bcf.v3.bcfapi` over `requests`; the typed CRUD methods compose over the `get`/`post`/`put`/`delete` primitives, so a new endpoint extends the existing client rather than a parallel HTTP layer.

## [05]-[LOCAL_ADMISSION]

[RAIL_LAW]:
- Package: `bcf-client`
- Owns: BCF v2/v3 file I/O, schema-version detection, topic/comment/viewpoint lifecycle, element selection and visibility in viewpoints, and the v3 BCF API REST client
- Accept: `Path` for file I/O; `ifcopenshell` GUIDs for element selection in viewpoints; HTTP credentials for the REST surface
- Reject: manual BCF zip construction or BCF-XML generation outside this package; wrapper-renames of `load`/`create_new`/`save`; a hand-rolled BCF REST client where `bcf.v3.bcfapi` is admitted

[CAPTURE_GAP]:
- dependencies: `xsdata>=24.4` (BCF-XML bind/parse), `numpy`, `ifcopenshell` (viewpoint element selection), `requests` (v3 REST) — the manifest comment naming an "lxml" dependency is stale; the XML codec is `xsdata`.
