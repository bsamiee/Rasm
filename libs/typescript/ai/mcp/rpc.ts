import { Array, Context, Effect, flow, Option, pipe, Record, Schema, Stream } from 'effect';
import { McpSchema, McpServer, Tool } from 'effect/ai';
import { Url, UrlParams } from 'effect/http';
import type { Rpc, RpcGroup } from 'effect/rpc';
import { type Dependency, type DependencyOptions, resourceValues } from '#/resource';
import { orInternalError, Scheme } from '#/server';
import { type Route, registerRoutedTool } from '#/tool';

// --- [OPERATIONS] ----------------------------------------------------------------------

const registerRpcGroup = Effect.fnUntraced(function* <Rpcs extends Rpc.Any & { readonly payloadSchema: Schema.Codec<Rpc.Payload<Rpcs>, unknown> & { readonly fields: Schema.Struct.Fields }; readonly successSchema: Schema.Codec<Rpc.Success<Rpcs>, unknown> }, Chosen, Fields extends Schema.Struct.Fields, R>(
    group: RpcGroup.RpcGroup<Rpcs>,
    {
        tools,
        resources,
        route,
        invoke,
    }: {
        readonly tools: { readonly [Tag in Rpcs['_tag']]?: { readonly description: string; readonly annotations?: Context.Context<never> | undefined; readonly success?: Schema.Codec<Rpc.Success<Rpc.ExtractTag<Rpcs, Tag>>, unknown> | undefined } };
        readonly resources: {
            readonly [Tag in Rpcs['_tag']]?: DependencyOptions<Rpc.Success<Rpc.ExtractTag<Rpcs, Tag>>> & {
                readonly description: string;
                readonly complete: { readonly [Field in keyof Rpc.ExtractTag<Rpcs, Tag>['payloadSchema']['fields'] & string]?: (success: Rpc.Success<Rpc.ExtractTag<Rpcs, Tag>>) => readonly string[] };
            };
        };
        readonly route: (rpc: Rpcs) => Route<Chosen, Fields, R>;
        readonly invoke: (rpc: Rpcs) => (payload: Rpc.Payload<Rpcs>, chosen: Chosen) => Effect.Effect<Rpc.Success<Rpcs>, Error, R>;
    },
) {
    const [server, scheme, services] = yield* Effect.all([McpServer.McpServer, Scheme, Effect.context<R | Schema.Struct<Fields>['DecodingServices'] | Schema.Struct<Fields>['EncodingServices']>()]);
    const answered = (completed: (input: string, context: typeof McpSchema.Complete.payloadSchema.Type.context) => Effect.Effect<readonly string[], Error, R | Schema.Struct<Fields>['DecodingServices']>) => (input: string, context: typeof McpSchema.Complete.payloadSchema.Type.context) =>
        completed(input, context).pipe(
            Effect.map((values) => McpSchema.CompleteResult.make({ completion: { values } })),
            orInternalError,
            Effect.provideContext(services),
        );
    const published = <Row>(rows: Readonly<Record<string, Row | undefined>>) =>
        Array.flatMap([...group.requests.values()], (rpc) =>
            pipe(
                Record.get(rows, rpc._tag),
                Option.flatMap(Option.fromUndefinedOr),
                Option.map((row) => [rpc, row] as const),
                Option.toArray,
            ),
        );
    const linked = yield* Effect.forEach(
        published<DependencyOptions<Rpc.Success<Rpcs>> & { readonly description: string; readonly complete: Readonly<Record<string, ((success: Rpc.Success<Rpcs>) => readonly string[]) | undefined>> }>(resources),
        Effect.fnUntraced(function* ([rpc, { description, complete, ...acts }]) {
            const routing = route(rpc);
            const uri = scheme.uri(rpc._tag);
            const defined = { name: rpc._tag, description, mimeType: 'application/json' };
            const { read, depend } = yield* resourceValues(rpc.successSchema, defined.mimeType, Stream.empty);
            const given = Schema.RecordFromUrlParams.pipe(Schema.decodeTo(Schema.toCodecStringTree(Schema.Struct(routing.fields))));
            const fields = { ...routing.fields, ...rpc.payloadSchema.fields };
            const call = Schema.Struct({ route: given, payload: Schema.RecordFromUrlParams.pipe(Schema.decodeTo(Schema.toCodecStringTree(rpc.payloadSchema))) });
            const decoded = (values: UrlParams.UrlParams) => Schema.decodeEffect(call)({ route: values, payload: values }, { errors: 'all' });
            const called = ({ route: chosen, payload }: typeof call.Type) => Effect.flatMap(routing.resolve(chosen), (found) => invoke(rpc)(payload, found)).pipe(Effect.provideContext(services));
            const answer = (at: string) =>
                Effect.flatMap(Schema.decodeEffect(Schema.URLFromString)(at), flow(Url.urlParams, decoded)).pipe(
                    Effect.mapError(({ message }) => McpSchema.InvalidParams.make({ message })),
                    Effect.flatMap((parsed) => read(at, called(parsed))),
                    Effect.provideContext(services),
                );
            const resolved = (_: string, context: typeof McpSchema.Complete.payloadSchema.Type.context) => Effect.flatMap(decoded(UrlParams.fromInput(context?.arguments ?? {})), called);
            const completions = {
                ...Record.getSomes(Record.map(routing.completion, flow(Option.fromUndefinedOr, Option.map(answered)))),
                ...Record.getSomes(
                    Record.map(
                        complete,
                        flow(
                            Option.fromUndefinedOr,
                            Option.map((select) => answered(flow(resolved, Effect.map(select)))),
                        ),
                    ),
                ),
            };
            yield* Schema.is(Schema.toEncoded(Schema.Struct(fields)))({}) ? server.addResource({ resource: McpSchema.Resource.make({ ...defined, uri }), annotations: group.annotations, handle: Effect.catchTag(answer(uri), 'InvalidParams', Effect.die) }) : Effect.void;
            yield* Array.match(Record.keys(fields), {
                onEmpty: () => Effect.void,
                onNonEmpty: (keys) => server.addResourceTemplate({ template: McpSchema.ResourceTemplate.make({ ...defined, uriTemplate: `${uri}{?${keys.join(',')}}` }), routerPath: uri.replace(':', '::'), annotations: group.annotations, completions, handle: answer }),
            });
            const base = yield* Effect.fromResult(Url.fromString(uri));
            const dependency = (routed: Schema.Struct.Type<Fields>) => {
                const named = Effect.provideContext(Schema.encodeEffect(given)(routed), services);
                return depend(
                    Effect.map(named, (params) => Url.setUrlParams(base, params).href),
                    () => named.pipe(Effect.flatMap(decoded), Effect.flatMap(called), Effect.provideContext(services)),
                )(acts);
            };
            return (chosen: Chosen, routed: Schema.Struct.Type<Fields>): readonly Dependency[] => (routing.serves(chosen) ? [dependency(routed)] : []);
        }),
    );
    const joined = (chosen: Chosen, routed: Schema.Struct.Type<Fields>) => Array.flatMap(linked, (dependency) => dependency(chosen, routed));
    yield* Effect.forEach(
        published<{ readonly description: string; readonly annotations?: Context.Context<never> | undefined; readonly success?: Schema.Codec<Rpc.Success<Rpcs>, unknown> | undefined }>(tools),
        ([rpc, { description, annotations = Context.empty(), success }]) => {
            const routing = route(rpc);
            return registerRoutedTool({
                ...routing,
                depends: (chosen, routed) => Effect.map(routing.depends(chosen, routed), Array.appendAll(joined(chosen, routed))),
            })(Tool.make(rpc._tag, { description, parameters: rpc.payloadSchema, success: success ?? rpc.successSchema }).annotateMerge(Context.merge(group.annotations, annotations)), invoke(rpc));
        },
        { discard: true },
    );
});

// --- [EXPORTS] -------------------------------------------------------------------------

export { registerRpcGroup };
