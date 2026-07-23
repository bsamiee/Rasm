# [PY_GEOMETRY_API_BCF_CLIENT]

`bcf-client` owns BCF (BIM Collaboration Format) v2/v3 file authoring, reading, and the v3 REST client for the IFC-analysis rail's coordination-issue exchange leg. A version-detecting `bcf.bcfxml.load` returns the matching versioned document root that owns the topic, comment, and viewpoint lifecycle; viewpoint authoring resolves `ifcopenshell` element selection and visibility into BCF GlobalId lists. `xsdata` owns the BCF-XML round-trip and this package owns the BCF zip contract, so the analysis owner composes the surface directly.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `bcf-client`
- package: `bcf-client` (GPLv3)
- import: `import bcf`
- owner: `geometry`
- rail: ifc-companion / bcf-exchange
- depends: `xsdata` (BCF-XML bind/parse), `ifcopenshell` (viewpoint element selection), `requests` (v3 REST), `numpy`
- entry points: none (library only)
- capability: BCF v2/v3 read/write, schema-version detection, topic/comment/viewpoint lifecycle, IFC-driven viewpoint selection and visibility, and the v3 BCF REST client

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: document root family

`BcfXml` is a `Union[BcfXml2, BcfXml3]` alias; `BcfXml2`/`BcfXml3` re-export `bcf.v2`/`bcf.v3` `bcfxml.BcfXml`. Import the versioned classes for explicit typing.

| [INDEX] | [SYMBOL]               | [TYPE_FAMILY] | [CAPABILITY]                                    |
| :-----: | :--------------------- | :------------ | :---------------------------------------------- |
|  [01]   | `bcf.bcfxml.BcfXml`    | union alias   | `Union[BcfXml2, BcfXml3]` return type of `load` |
|  [02]   | `bcf.bcfxml.BcfXml2`   | v2 re-export  | re-export of `bcf.v2.bcfxml.BcfXml`             |
|  [03]   | `bcf.bcfxml.BcfXml3`   | v3 re-export  | re-export of `bcf.v3.bcfxml.BcfXml`             |
|  [04]   | `bcf.v2.bcfxml.BcfXml` | v2 document   | full v2 BCF file lifecycle                      |
|  [05]   | `bcf.v3.bcfxml.BcfXml` | v3 document   | full v3 BCF file lifecycle                      |

[PUBLIC_TYPE_SCOPE]: topic and viewpoint handlers

| [INDEX] | [SYMBOL]                                  | [TYPE_FAMILY] | [CAPABILITY]                      |
| :-----: | :---------------------------------------- | :------------ | :-------------------------------- |
|  [01]   | `bcf.v2.topic.TopicHandler`               | issue handler | v2 topic comments plus viewpoints |
|  [02]   | `bcf.v3.topic.TopicHandler`               | issue handler | v3 topic comments plus viewpoints |
|  [03]   | `bcf.v2.visinfo.VisualizationInfoHandler` | viewpoint     | v2 camera, selection, visibility  |
|  [04]   | `bcf.v3.visinfo.VisualizationInfoHandler` | viewpoint     | v3 camera, selection, visibility  |

[PUBLIC_TYPE_SCOPE]: v2 and v3 markup model family

Markup models are `xsdata` dataclasses from the BCF XSD; `DocumentReference` is v3-only, the rest shared. `TopicHandler.topic.title` is the canonical issue-title accessor, and each model resolves as `bcf.v2.model.markup.<Name>` and `bcf.v3.model.markup.<Name>`.

| [INDEX] | [SYMBOL]            | [TYPE_FAMILY]  | [CAPABILITY]                                                    |
| :-----: | :------------------ | :------------- | :-------------------------------------------------------------- |
|  [01]   | `Topic`             | `xsdata` model | issue metadata: `title`/`guid`/`topic_status`/`creation_author` |
|  [02]   | `Comment`           | `xsdata` model | issue comment                                                   |
|  [03]   | `Markup`            | `xsdata` model | topic container                                                 |
|  [04]   | `ViewPoint`         | `xsdata` model | viewpoint reference                                             |
|  [05]   | `Header`            | `xsdata` model | IFC file header reference                                       |
|  [06]   | `BimSnippet`        | `xsdata` model | linked BIM snippet                                              |
|  [07]   | `DocumentReference` | `xsdata` model | linked document reference (v3 only)                             |

[PUBLIC_TYPE_SCOPE]: v3 REST API family — `bcf.v3.bcfapi`

| [INDEX] | [SYMBOL]           | [TYPE_FAMILY] | [CAPABILITY]                             |
| :-----: | :----------------- | :------------ | :--------------------------------------- |
|  [01]   | `BcfClient`        | REST client   | BCF API v3 topic/comment/viewpoint ops   |
|  [02]   | `FoundationClient` | REST client   | BCF API foundation auth/version services |
|  [03]   | `OAuthReceiver`    | auth helper   | OAuth2 local callback receiver           |

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: BCF file I/O

`bcf.bcfxml.load` detects the schema version and returns the matching versioned `BcfXml`; the versioned `create_new` factories author a fresh document. Every I/O and factory member accepts `xml_handler=None`.

| [INDEX] | [SURFACE]                                                  | [SHAPE]  | [CAPABILITY]                     |
| :-----: | :--------------------------------------------------------- | :------- | :------------------------------- |
|  [01]   | `bcf.bcfxml.load(filepath) -> BcfXml \| None`              | static   | detect schema, open BCF          |
|  [02]   | `BcfXml.load(filename) -> BcfXml \| None`                  | factory  | open BCF into a version instance |
|  [03]   | `v2.BcfXml.create_new(project_name=None)`                  | factory  | new v2 BCF document              |
|  [04]   | `v3.BcfXml.create_new(project_name=None, extensions=None)` | factory  | new v3 BCF document              |
|  [05]   | `BcfXml.save(filename=None, keep_open=False)`              | instance | write BCF archive to disk        |
|  [06]   | `BcfXml.close()`                                           | instance | close and release zip resources  |

[ENTRYPOINT_SCOPE]: topic and comment lifecycle

`add_topic` returns a `TopicHandler`; comment and project mutations accumulate in memory until `save`.

| [INDEX] | [SURFACE]                                                         | [SHAPE]       | [CAPABILITY]                    |
| :-----: | :---------------------------------------------------------------- | :------------ | :------------------------------ |
|  [01]   | `add_topic(title, description, author, topic_type, topic_status)` | instance      | author a new topic              |
|  [02]   | `get_topics() -> dict[str, TopicHandler]`                         | instance      | all topics keyed by GUID        |
|  [03]   | `get_topic(guid)`                                                 | instance      | one topic by GUID               |
|  [04]   | `add_comment(topic, comment=None)`                                | instance      | append a comment to a topic     |
|  [05]   | `edit_comment()`                                                  | instance      | edit an existing comment        |
|  [06]   | `get_project` / `edit_project` / `save_project`                   | instance      | project metadata read and write |
|  [07]   | `documents`                                                       | property (v3) | v3 `DocumentsHandler` access    |

[ENTRYPOINT_SCOPE]: `TopicHandler` operations

`TopicHandler` binds viewpoints and comments to a topic and exposes its markup model.

| [INDEX] | [SURFACE]                                              | [SHAPE]  | [CAPABILITY]                                               |
| :-----: | :----------------------------------------------------- | :------- | :--------------------------------------------------------- |
|  [01]   | `add_viewpoint(element) -> VisualizationInfoHandler`   | instance | attach an element viewpoint to the topic                   |
|  [02]   | `add_viewpoint_from_point_and_guids(position, *guids)` | instance | viewpoint from a camera point plus GUIDs                   |
|  [03]   | `add_visinfo_handler(handler)`                         | instance | bind a prepared `VisualizationInfoHandler`                 |
|  [04]   | `topic` / `markup` / `comments` / `viewpoints`         | property | topic model (`.title`), markup, comment/viewpoint handlers |
|  [05]   | `header` / `bim_snippet` / `reference_files` / `guid`  | property | IFC header, snippet bytes, reference-file bytes, GUID      |
|  [06]   | `create_new` / `save`                                  | factory  | in-package topic factory; persist topic into the BCF zip   |

[ENTRYPOINT_SCOPE]: `VisualizationInfoHandler` operations

`VisualizationInfoHandler` authors a viewpoint's camera, selection, and visibility from `ifcopenshell` elements.

| [INDEX] | [SURFACE]                                        | [SHAPE]  | [CAPABILITY]                                       |
| :-----: | :----------------------------------------------- | :------- | :------------------------------------------------- |
|  [01]   | `create_new(element)`                            | factory  | viewpoint from an element                          |
|  [02]   | `create_from_point_and_guids(position, *guids)`  | factory  | viewpoint from camera point plus GUIDs             |
|  [03]   | `from_topic_viewpoints` / `load` / `save`        | factory  | reconstruct from markup; read/write viewpoint      |
|  [04]   | `set_selected_elements(elements)`                | instance | mark selected IFC elements                         |
|  [05]   | `set_visible_elements` / `set_hidden_elements`   | instance | show/hide IFC elements in the viewpoint            |
|  [06]   | `set_visibility(default_visible, exceptions)`    | instance | default visibility plus exception elements         |
|  [07]   | `get_selected_guids` / `get_elements_visibility` | instance | selected GUIDs; (default_visible, exception GUIDs) |

[ENTRYPOINT_SCOPE]: v3 REST API — `bcf.v3.bcfapi`

`BcfClient` is the `requests`-backed REST surface: typed CRUD methods compose over the `get`/`post`/`put`/`delete` primitives (GET returns `Any`, mutations return `(status, text)`). `FoundationClient` carries auth and version negotiation; `OAuthReceiver` services the OAuth2 authorization-code callback.

- `BcfClient.get(endpoint, params=None, is_auth_required=False)`, `post`/`put`/`delete(endpoint, data, params)` are the primitives every CRUD row composes over.

| [INDEX] | [SURFACE]                                                                                    | [CAPABILITY]   |
| :-----: | :------------------------------------------------------------------------------------------- | :------------- |
|  [01]   | `get` / `post` / `put` / `delete`                                                            | http primitive |
|  [02]   | `set_version`                                                                                | config         |
|  [03]   | `get_projects` / `get_project` / `update_project` / `get_extensions`                         | project        |
|  [04]   | `get_topics` / `get_topic` / `create_topic` / `update_topic` / `delete_topic`                | topic          |
|  [05]   | `get_comments` / `get_comment` / `create_comments` / `update_comment` / `delete_comment`     | comment        |
|  [06]   | `get_viewpoints` / `get_viewpoint` / `create_viewpoints` / `delete_viewpoint`                | viewpoint      |
|  [07]   | `get_snapshot` / `get_bitmap` / `get_selection` / `get_coloring` / `get_visibility`          | media          |
|  [08]   | `get_snippet` / `update_snippet` / `get_files_information` / `get_files` / `update_files`    | snippet/files  |
|  [09]   | `get_related_topics` / `update_related_topics`                                               | relations      |
|  [10]   | `get_document_references` / `create_document_reference` / `update_document_references`       | doc-refs       |
|  [11]   | `get_documents` / `get_document` / `create_document`                                         | documents      |
|  [12]   | `get_topics_events` / `get_topic_events` / `get_comments_events` / `get_comment_events`      | events         |
|  [13]   | `FoundationClient.login` / `get_access_token` / `get_refresh_token` / `get_new_access_token` | auth           |
|  [14]   | `FoundationClient.set_auth_method` / `set_tokens_from_response`                              | auth           |
|  [15]   | `FoundationClient.get_auth_methods` / `get_versions`                                         | negotiate      |
|  [16]   | `OAuthReceiver.do_GET`                                                                       | auth callback  |

## [04]-[IMPLEMENTATION_LAW]

[TOPOLOGY]:
- import: `bcf-client` ships the top-level `bcf` package, never `bcfxml`/`bcfapi`; import at boundary scope only.
- version dispatch: `bcf.bcfxml.load` reads the `bcf.version` zip entry and returns `bcf.v2.bcfxml.BcfXml` or `bcf.v3.bcfxml.BcfXml`, raising `ValueError` outside `{2.0, 2.1, 3.0}`.
- v3 delta: `BcfXml.documents` exposes the `DocumentsHandler` and markup adds `DocumentReference`; v2 carries `extension_schema`/`extensions` instead.
- parser: `xml_handler` takes any `AbstractXmlParserSerializer`; the default `XmlParserSerializer` binds the BCF XSD models through `xsdata`.
- factory split: `TopicHandler.create_new` is the in-package factory; `BcfXml.add_topic(title, description, author, topic_type='', topic_status='')` wires the topic into the markup tree and returns its `TopicHandler`. `add_topic`, `add_comment`, and viewpoint mutations accumulate in memory until `BcfXml.save` flushes the archive.

[STACKING]:
- `ifcopenshell`(`.api/ifcopenshell.md`): `file.by_type`/`by_guid` `entity_instance` products feed `TopicHandler.add_viewpoint(element)` and `VisualizationInfoHandler.set_selected_elements`/`set_visible_elements`/`set_hidden_elements`, which resolve IFC GlobalIds into the viewpoint's selection and visibility GUID lists — a viewpoint authored from a clash or IDS result selects the same elements by GlobalId with no parallel identity map.
- `ifc/analysis` owner: composes `bcf.bcfxml.load`, the versioned `create_new` factories, and the topic/viewpoint handlers into the BCF exchange leg, authoring coordination issues from the same IFC model the clash/IDS run produces.

[LOCAL_ADMISSION]:
- `ifc/analysis` owner admits `load`, the versioned `create_new`, and the topic/viewpoint handlers as the BCF exchange leg: a `Path` opens files, `ifcopenshell` GUIDs drive viewpoint selection, and HTTP credentials reach the v3 REST surface.

[RAIL_LAW]:
- Package: `bcf-client`
- Owns: BCF v2/v3 file I/O, schema-version detection, the topic/comment/viewpoint lifecycle, viewpoint element selection and visibility, and the v3 BCF REST client.
- Accept: `Path` for file I/O; `ifcopenshell` `entity_instance` GUIDs for viewpoint selection; HTTP credentials for the REST surface.
- Reject: hand-rolled BCF zip construction or BCF-XML serialization; a parallel BCF REST client where `bcf.v3.bcfapi` is admitted.
