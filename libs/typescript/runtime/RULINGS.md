# [TS_RUNTIME_RULINGS]

`typescript/runtime` rulings settle package-scoped decisions.

## [01]-[PACKAGES]

- `@opentelemetry/*` HOLDS as two matched version lines — the api/SDK line and the exporter/instrumentation line ride the upstream's two release tracks and move as one wave; unifying them to a single version is the refuted consistency fix, reopened only when upstream collapses the two tracks.
- `@effect/cluster-node` runner welding stays never-admitted — the runner binding is a runtime-row selection (`NodeClusterSocket.layer`/`BunClusterSocket.layer`) keeping node and bun peer rows over the admitted `@effect/cluster` work plane; a survey re-proposes the node-welded family and deletes the bun row. Reopens only on a runner-neutral upstream cluster transport.
- Rpc serving on its own listener stays never-admitted — `@effect/rpc` and `@connectrpc/connect-node` are the admitted outbound dial, and Connect serving lands through serve's foreign-protocol `Mount` port, so HTTP serving keeps one front door; a standalone rpc listener mints a second public surface beside `serve`.
- `@confluentinc/kafka-javascript` is admitted over pure-JS `kafkajs` — the librdkafka client matches the C# branch's `Confluent.Kafka` on the shared broker plane, so both languages speak one client family's protocol, config, and delivery semantics; a survey reading the manifest alone re-proposes `kafkajs` blind to the parity constraint, forking the broker-plane client from its C# counterpart. Reopens only when the C# branch leaves `Confluent.Kafka`.

## [02]-[SHAPE]

- (none)

## [03]-[COLLAPSE]

- (none)

## [04]-[STRUCTURE]

- (none)

## [05]-[PROCESS]

- (none)
