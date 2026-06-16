# [PY_RUNTIME_API_SPECKLEPY]

`specklepy` supplies the Speckle exchange client: a `SpeckleClient` over the GraphQL API with account/token authentication, a `ServerTransport` for object send/receive, the `Base` object model with detach/chunk semantics, and the high-level `send`/`receive`/`serialize`/`deserialize` operations. It is the runtime owner for the Speckle remote AEC transport row.

## [1]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `specklepy`
- package: `specklepy`
- import: `specklepy`
- version: `3.2.8`
- owner: `runtime`
- rail: transport
- namespaces: `specklepy.api.client`, `specklepy.api.operations`, `specklepy.transports.server`, `specklepy.objects.base`, `specklepy.core`, `specklepy.serialization`
- capability: Speckle GraphQL client, account/token auth, object send/receive transport, `Base` object model, serialize/deserialize operations

## [2]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: client and resource family
- rail: transport

| [INDEX] | [SYMBOL] | [TYPE_FAMILY] | [RAIL] |
| :-----: | :------- | :------------ | :----- |
| [1] | `api.client.SpeckleClient` | client | GraphQL API client |
| [2] | `api.client.CoreSpeckleClient` | client | low-level core client |
| [3] | `api.client.Account` | credential | server account/token |
| [4] | `api.client.ProjectResource` | resource | project query/mutation surface |
| [5] | `api.client.ModelResource` | resource | model query/mutation surface |
| [6] | `api.client.VersionResource` | resource | version query/mutation surface |
| [7] | `api.client.ServerResource` | resource | server-info surface |
| [8] | `api.client.SubscriptionResource` | resource | live-subscription surface |

[PUBLIC_TYPE_SCOPE]: transport and object family
- rail: transport

| [INDEX] | [SYMBOL] | [TYPE_FAMILY] | [RAIL] |
| :-----: | :------- | :------------ | :----- |
| [1] | `transports.server.ServerTransport` | transport | remote object store transport |
| [2] | `objects.base.Base` | object model | detach/chunk-aware base object |
| [3] | `objects.base.DataChunk` | object model | chunked-array payload |
| [4] | `objects.base.SpeckleException` | fault | object/exchange error |

## [3]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: exchange operations
- rail: transport

| [INDEX] | [SURFACE] | [ENTRY_FAMILY] | [RAIL] |
| :-----: | :-------- | :------------- | :----- |
| [1] | `SpeckleClient.authenticate_with_token` | auth | authenticate with a token |
| [2] | `SpeckleClient.authenticate_with_account` | auth | authenticate with an account |
| [3] | `SpeckleClient.execute_query` | query | run a raw GraphQL query |
| [4] | `api.operations.send` | send | serialize and transmit objects |
| [5] | `api.operations.receive` | receive | fetch and deserialize objects |
| [6] | `api.operations.serialize` | serialize | object tree to JSON |
| [7] | `api.operations.deserialize` | deserialize | JSON to object tree |
| [8] | `ServerTransport.save_object` | transport | persist an object to the server |
| [9] | `ServerTransport.get_object` | transport | fetch an object by id |
| [10] | `ServerTransport.has_objects` | transport | existence check by id set |

## [4]-[IMPLEMENTATION_LAW]

[TRANSPORT_TOPOLOGY]:
- client law: one authenticated `SpeckleClient` per server is constructed from a settings-model account/token; resource access goes through the typed `*Resource` surfaces, never raw GraphQL strings except where `execute_query` is the only contract.
- exchange law: object movement uses `operations.send`/`receive` with a `ServerTransport`; the runtime never re-implements the detach/chunk hashing — the `Base` object model owns it.
- credential law: tokens and accounts arrive from the caller-owned settings model; no hard-coded server or token.
- boundary law: a `SpeckleException` is lifted into `Error(BoundaryFault(...))` at the exchange boundary.
- store law: Speckle is a remote AEC transport, never a durable store the runtime owns — send/receive cross the companion seam, results flow back as values.

[LOCAL_ADMISSION]:
- The transport surface composes specklepy for the Speckle remote-exchange row alongside httpx/asyncssh transports; the runtime owns no second Speckle client.
- The `Base` object tree is admitted at the seam and converted to the branch's canonical shapes; Speckle's model never leaks into interior domain logic.

[RAIL_LAW]:
- Package: `specklepy`
- Owns: the Speckle remote AEC transport row — authenticated client, object send/receive, and the `Base` object model
- Accept: settings-model credentials, typed resource access, `send`/`receive` with `ServerTransport`, boundary-lifted exceptions
- Reject: hard-coded servers/tokens, raw GraphQL where a resource exists, Speckle as a durable store, `Base` leakage into domain logic
