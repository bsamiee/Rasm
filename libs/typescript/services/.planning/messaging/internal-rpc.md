# [SERVICES_INTERNAL_RPC]

The internal TypeScript-to-TypeScript RPC surface — `InternalRpc`, the one `@effect/rpc` `RpcGroup` with `WorkflowProxy` derived from it over the durable workflows. The internal RPC surface is distinct from the .NET wire — the protobuf-and-msgpack .NET wire is structurally absent, and `InternalRpc` serializes against the SAME wire `Schema` the durable workflows use, never a parallel surface. The capture-event client-stream dialed on node rides the `interchange` clients, not this surface. This page crosses no .NET wire.

## [1]-[INDEX]

One cluster: `[2]-[INTERNAL_RPC]` owns the one `RpcGroup` and the `WorkflowProxy` projection over the durable workflows.

## [2]-[INTERNAL_RPC]

- Owner: `InternalRpc`, the internal RPC surface — each procedure is a Schema-typed request with its own success and error schemas aggregated into ONE `RpcGroup`; the serialization row selects the in-process codec for the TS-to-TS edge (Schema-serialized, the protobuf-and-msgpack .NET wire structurally absent); the transport binds the group over the platform socket or HTTP layer with a client and a server half. `WorkflowProxy` is DERIVED from the durable workflow set, not a hand-built parallel surface.
- Cases: the durable workflows become callable over this surface through the workflow-proxy projection — `WorkflowProxy.toRpcGroup` converts the workflow set into the request group whose three procedures per workflow are the base execute, a `${Name}Discard` fire-and-forget, and a `${Name}Resume` resume-by-execution-id, and `WorkflowProxyServer.layerRpcHandlers` installs the handlers as one RPC-handler layer, so the host-to-worker dispatch and the panel-to-host start and resume signalling both ride one group rather than a parallel surface; the `RpcClient` and `RpcServer` halves serialize against the SAME `Schema` the workflow defines, so the wire shape is the workflow's own success/error schema, never a re-minted RPC DTO.
- Entry: each procedure is a Schema-typed request; the node host-to-worker edge consumes the same group; the workflow proxy (`durable-execution/saga#SAGA` derives its saga procedures here) is the sole mechanism by which a durable unit becomes a callable procedure.
- Packages: `@effect/rpc` for the `RpcGroup`/`RpcSerialization`/`RpcClient`/`RpcServer` surface, `@effect/workflow` for the `WorkflowProxy.toRpcGroup`/`WorkflowProxyServer.layerRpcHandlers` projection, and `@effect/platform-node` for the transport.
- Growth: a new internal procedure lands as one request on the existing group; a new workflow becomes callable by extending the proxied workflow set, never a hand-built procedure. The addressable-`Entity` actor model is the SECOND modality for this sub-domain — a stateful per-key actor surface beside the request-driven proxy — owned by the sibling `messaging/entity#ENTITY` page.
- Boundary: `InternalRpc` is the `@effect/rpc` surface distinct from the .NET wire, its only consumers being this domain's owners; a second internal RPC surface is the named defect; this is a node-only surface, never browser-reachable.

```ts contract
interface InternalRpc<Procedures extends Rpc.Any> {
  readonly group: RpcGroup.RpcGroup<Procedures>;
  readonly serialization: Layer.Layer<RpcSerialization.RpcSerialization>;
  readonly server: Layer.Layer<never, never, RpcServer.Protocol>;
  readonly client: Effect.Effect<RpcClient.RpcClient<Procedures>, never, RpcServer.Protocol>;
  readonly fromWorkflows: <const W extends NonEmptyReadonlyArray<Workflow.Any>>(workflows: W) => RpcGroup.RpcGroup<WorkflowProxy.ConvertRpcs<W[number], "">>;
  readonly handlers: <const W extends NonEmptyReadonlyArray<Workflow.Any>>(workflows: W) => Layer.Layer<WorkflowProxyServer.RpcHandlers<W[number], "">, never, WorkflowEngine | Workflow.Requirements<W[number]>>;
}
```
