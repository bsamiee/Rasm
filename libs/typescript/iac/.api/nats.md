# [TS_IAC_API_NATS]

`nats` is the fanout engine's server: a StatefulSet whose config document the chart assembles from a typed `config` tree, with a `merge`/`patch` pair at every level for the raw server directives no values key spells. Two facts rule a fence against it — the config tree renders the server file, so a directive the chart already emits is not a values key, and the release name alone does not name the Service.

## [01]-[CHART_VALUES]

| [INDEX] | [KEY]                               | [CAPABILITY]                                                                           |
| :-----: | :---------------------------------- | :------------------------------------------------------------------------------------- |
|  [01]   | `config.cluster`                    | the route mesh; `replicas` must be 2+ once JetStream is on                             |
|  [02]   | `config.jetstream`                  | the persistence engine, OFF by default                                                 |
|  [03]   | `config.jetstream.fileStore.pvc`    | the data claim, DEFAULT 10Gi at `/data`                                                |
|  [04]   | `config.jetstream.merge`            | the escape hatch for server directives with no values key — `sync_interval` among them |
|  [05]   | `config.nats`                       | `{ port, tls }` — the client listener, 4222                                            |
|  [06]   | `config.websocket`                  | the browser and node listener, 8080; OFF by default                                    |
|  [07]   | `config.*` remainder                | the remaining listener and topology families                                           |
|  [08]   | `service.ports.<name>.enabled`      | which listeners the Service publishes — a config port unrowed here routes nothing      |
|  [09]   | `service.{merge,patch,name}`        | the Service escape hatches and its name override                                       |
|  [10]   | `natsBox.enabled`                   | `boolean` DEFAULT TRUE — a CLI shell Deployment beside the server                      |
|  [11]   | `promExporter` `reloader`           | the two sidecars                                                                       |
|  [12]   | `{name,fullname,namespace}Override` | FLAT top-level                                                                         |

[ESCAPE_HATCHES]: `statefulSet` `podTemplate` `headlessService` `configMap` `podDisruptionBudget` `serviceAccount`, each `{ merge, patch, name }`
[CONFIG_REMAINDER]: `config.leafnodes` `config.mqtt` `config.gateway` `config.monitor` `config.profiling` `config.resolver`, each carrying its own `enabled`/`port`/`tls` trio and `merge`/`patch` pair
[MERGE_PATCH]: every config family and every rendered object carries a `merge` map and a `patch` list. `merge` deep-merges into the chart's own document and `patch` applies a JSON-patch list, so a raw NATS directive the values tree does not model rides `merge` at its owning level. This is the ONLY admitted route for such a directive — a sibling key beside `port` is dropped silently, because each config template reads its own named fields and nothing else.
[RENDERED_DIRECTIVES]: the chart emits `no_tls: true` on a listener whose `tls.enabled` is false, and `max_file_store`/`max_memory_store` from the JetStream store sizes. Spelling any of them as a values key states a fact the chart already renders and reads nowhere.
[PORT_DUALITY]: a listener is reachable only where BOTH halves agree — `config.<listener>.enabled` opens the server port and `service.ports.<listener>.enabled` publishes it. The websocket, leafnode, and mqtt Service rows default ON while their config rows default OFF, so the pair is stated together or a door advertises nothing.

[FULLNAME]: the standard collapse scaffold with flat `nameOverride`/`fullnameOverride`. Absent a pin, a release named `fanout` renders `fanout-nats`, `fanout-nats-headless`, and `fanout-nats-config`, so a websocket origin spelled off the release name alone resolves to nothing.
[SERVICE_NAME]: with the pin, the client Service is `<fullname>` and the peer Service `<fullname>-headless`; `service.name` and `headlessService.name` override each independently. The websocket ingress renders as `<fullname>-ws`.

## [02]-[IMPLEMENTATION_LAW]

[TOPOLOGY]:
- Deployment posture and topic policy are two owners. This tier states the storage envelope, the replica count, and the durability posture; retention, dedup windows, ack posture, and replay depth are the runtime fanout owner's topic rows, so a topic change never touches the deploy plane.
- Durability is hardened at the server and priced as data: the file store runs `sync_interval: always` through the JetStream merge key, because the engine's default periodic fsync loses acknowledged writes under coordinated power failure. The stream is still never the system of record — the data journal is.
- Quorum is the replica row. Three is the file-store default (R3 tolerates one node loss) and a single-replica dev profile is a deliberate spec delta; stream-level replica counts stay the runtime's own `jsm.streams.add` fact.

[STACKING]:
- `@pulumi/kubernetes`(`.api/pulumi-kubernetes.md`): `helm.v4.Chart` renders the StatefulSet and its ConfigMap; the reloader sidecar picks up a config change without a rollout.
- `kube/data#FANOUT_STORE`: the `Nats` tier pins `fullnameOverride`, arms cluster and JetStream off the profile, opens the websocket listener, disables `natsBox`, and projects `ws://<pinned>.<ns>.svc:8080` as the origin the `fanout` output plane publishes.
- `runtime/net/pubsub#JETSTREAM_ROW`: the consuming engine — no client speaks the bare NATS port here, so the websocket listener is the one door and the browser and node lanes share it.
- `proc/config#SETTING_OWNER`: the published origin lands on the `RUNTIME_FANOUT_ORIGIN` variable through the `fanout.origin` channel row, which is where the deploy plane and the process plane meet.

[LOCAL_ADMISSION]:
- Pin `fullnameOverride` to the release name; every address this cluster publishes is derived from that pin.
- Route a raw server directive through the owning level's `merge`; a bare sibling key is dropped without a warning.
- Never spell a directive the chart renders itself — `no_tls` follows from `tls.enabled: false` and nothing else.
- State both halves of every listener: the config row opens the port and the service row publishes it.
- Disable `natsBox`; it is a CLI shell Deployment no tier declared and nothing dials.
- Set `config.cluster.replicas` at 2 or higher whenever JetStream is on; the server refuses a one-node cluster with the file store armed.
