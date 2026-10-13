import uFuzzy from '@leeoniya/ufuzzy';
import { Array, Boolean, Clock, Context, Deferred, Duration, Effect, Equal, Filter, Formatter, Latch, Layer, Option, pipe, Record, Ref, Schedule, Schema, Stream, Struct, SubscriptionRef } from 'effect';
import { McpSchema, McpServer, Tool } from 'effect/ai';
import { Headers, HttpRouter, HttpServerRequest, HttpServerResponse } from 'effect/http';
import { type Rpc, RpcClient, RpcClientError, type RpcGroup } from 'effect/rpc';
import { Socket } from 'effect/socket';
import { REDIAL, unit, WIRE } from './bridge.ts';

// --- [TYPES] ---------------------------------------------------------------------------

type Verb<P extends Schema.Codec<unknown, unknown> = Schema.Codec<unknown, unknown> & { readonly fields: Readonly<Record<string, Schema.Codec<unknown, unknown>>> }> = Tool.Tool<
    string,
    { readonly parameters: P; readonly success: Schema.Codec<unknown, unknown>; readonly failure: Schema.Codec<Error, unknown>; readonly failureMode: 'error' }
>;
type Verbs<V> = { readonly [K in keyof V]: Verb & { readonly name: K } };
type Work<T extends Verb<Schema.Codec<unknown, unknown>>> = Effect.Effect<readonly [T['successSchema']['Type'], readonly McpSchema.ContentBlock[]], T['failureSchema']['Type']>;
type Unit<P extends Schema.Top> = ReturnType<typeof unit<P>>['Type'];
type Handled<Target, V extends Verbs<V>, A extends readonly unknown[]> = { readonly [K in keyof V]: (target: Target, parameters: V[K]['parametersSchema']['Type'], ...context: A) => Work<V[K]> };
interface Route<A extends readonly unknown[]> {
    readonly tool: Verb;
    readonly serve: (target: unknown) => Option.Option<(payload: unknown, ...context: A) => Effect.Effect<readonly [Option.Option<string>, Work<Verb>], McpSchema.InvalidParams>>;
}
interface Source {
    readonly file: string;
    readonly postscriptName: string;
    readonly coordinates: Option.Option<Array.NonEmptyReadonlyArray<number>>;
}
type Resolve = (name: string, characters: string) => Effect.Effect<readonly [Resolver['success']['Type'], Option.Option<Source>], Error>;
interface Resolver {
    readonly success: Schema.Struct<Readonly<Record<string, Schema.Codec<unknown, unknown>>>>;
    readonly failure: Schema.Codec<Error, unknown>;
    readonly serve: (target: unknown) => Option.Option<Resolve>;
}
interface Dispatch {
    readonly linked: (target: unknown) => Option.Option<Effect.Effect<Option.Option<string>>>;
    readonly preview: Schema.Codec<unknown, unknown>;
    readonly runs: readonly Route<[Unit<Schema.Top>]>[];
    readonly verbs: readonly Route<[]>[];
    readonly face: Resolver;
}
interface Extension<E = never, R = never> {
    readonly name: string;
    readonly instructions: string;
    readonly Service: Context.Service<Dispatch, Dispatch>;
    readonly layer: Layer.Layer<Dispatch, E, R>;
}

// --- [CONSTANTS] -----------------------------------------------------------------------

const STRICT = { onExcessProperty: 'error', errors: 'all' } as const;
const READING = Context.make(Tool.Readonly, true).pipe(Context.add(Tool.Idempotent, true), Context.add(Tool.Destructive, false), Context.add(Tool.OpenWorld, false));
const WRITING = Context.make(Tool.Readonly, false).pipe(Context.add(Tool.Idempotent, false), Context.add(Tool.Destructive, true), Context.add(Tool.OpenWorld, false));

// --- [ERRORS] --------------------------------------------------------------------------

const LinkReason = Schema.TaggedUnion({ Unlinked: {}, Dropped: { cause: RpcClientError.RpcClientError }, Stalled: { seconds: Schema.Finite } });
class LinkError extends Schema.TaggedError<LinkError>()('LinkError', { peer: Schema.String, reason: LinkReason }) {
    override get message(): string {
        return LinkReason.match(this.reason, {
            Unlinked: () => `${this.peer} is not linked: start it, then retry`,
            Dropped: ({ cause }) => `${this.peer} dropped mid-call (${cause.message}): the call may have taken effect, check before repeating it`,
            Stalled: ({ seconds }) => `${this.peer} took the call ${seconds} s ago and has not started it: an open menu, dialog, or drag holds it, or it is hung; the call stays queued, check before repeating it`,
        });
    }
}

// --- [SERVICES] ------------------------------------------------------------------------

class Executions extends Context.Service<Executions, { readonly keep: (call: { readonly tool: string; readonly arguments: Effect.Effect<typeof Schema.Json.Type> }, answer: Effect.Effect<McpSchema.CallToolResult>) => Effect.Effect<McpSchema.CallToolResult> }>()('@rasm/ai/extension/Executions') {
    static readonly layer = (uri: string): Layer.Layer<Executions, never, McpServer.McpServer> =>
        Layer.effect(
            this,
            Effect.gen(function* () {
                const Row = Schema.Struct({ tool: Schema.String, arguments: Schema.Json, result: Schema.OptionFromOptionalKey(McpSchema.CallToolResult) });
                const [server, scope, entries] = yield* Effect.all([McpServer.McpServer, Effect.scope, Ref.make(Array.empty<{ readonly session: Option.Option<string>; readonly row: Effect.Effect<typeof Row.Type> }>())]);
                const session = Effect.map(
                    Effect.serviceOption(HttpServerRequest.HttpServerRequest),
                    Option.flatMap(({ headers }) => Headers.get(headers, 'mcp-session-id')),
                );
                const encode = Schema.encodeEffect(Schema.fromJsonString(Schema.toCodecJson(Schema.Array(Row))));
                yield* McpServer.registerResource({
                    uri,
                    name: 'Executions',
                    description: 'Calls of this session cancelled before their response, each with its arguments and, once finished, its result; a finished call is listed once',
                    mimeType: 'application/json',
                    content: Effect.gen(function* () {
                        const [own, held] = yield* Effect.all([session, Ref.get(entries)]);
                        const mine = Array.filter(held, (entry) => Equal.equals(entry.session, own));
                        const rows = yield* Effect.forEach(mine, Struct.get('row'));
                        const finished = pipe(
                            Array.zip(mine, rows),
                            Array.filter(([, { result }]) => Option.isSome(result)),
                            Array.map(([entry]) => entry),
                        );
                        yield* Ref.update(entries, Array.difference(finished));
                        return yield* Effect.orDie(encode(rows));
                    }),
                });
                return {
                    keep: ({ tool, arguments: given }, answer) =>
                        Effect.flatMap(Effect.all([session, Deferred.make<McpSchema.CallToolResult>()]), ([own, done]) => {
                            const row = Effect.all({ tool: Effect.succeed(tool), arguments: given, result: Effect.flatMap(Deferred.poll(done), Effect.transposeOption) });
                            const settled = Effect.andThen(Deferred.await(done), server.notifications['notifications/resources/updated']({ uri }));
                            return Effect.uninterruptibleMask((restore) =>
                                Effect.andThen(
                                    Effect.forkIn(Deferred.complete(done, answer), scope, { uninterruptible: true }),
                                    Effect.onInterrupt(restore(Deferred.await(done)), () => Effect.andThen(Ref.update(entries, Array.append({ session: own, row })), Effect.forkIn(settled, scope))),
                                ),
                            );
                        }),
                };
            }),
        );
}

// --- [OPERATIONS] ----------------------------------------------------------------------

// --- [LINKS]
const listen = Effect.fn('listen')(function* <Rpcs extends Rpc.Any, Peer, I extends Readonly<Record<string, string | readonly string[] | undefined>>>(
    group: RpcGroup.RpcGroup<Rpcs>,
    { path, origin, identity, peer }: { readonly path: HttpRouter.PathInput; readonly origin: string; readonly identity: Schema.Codec<Peer, I>; readonly peer: string },
) {
    interface Entry {
        readonly peer: Peer;
        readonly client: RpcClient.RpcClient<Rpcs, RpcClientError.RpcClientError>;
    }
    const [router, links] = yield* Effect.all([HttpRouter.HttpRouter, SubscriptionRef.make(Array.empty<Entry>())]);
    yield* router.add('GET', path, (request) =>
        request.headers.origin === origin
            ? Effect.gen(function* () {
                  const [found, socket, made, ended] = yield* Effect.all([HttpServerRequest.schemaSearchParams(identity), request.upgrade, Deferred.make<Entry>(), Latch.make()]);
                  yield* RpcClient.make(group, { disableTracing: true }).pipe(
                      Effect.flatMap((client) => Deferred.succeed(made, { peer: found, client })),
                      Effect.provideServiceEffect(RpcClient.Protocol, RpcClient.makeProtocolSocket({ pingInterval: Duration.infinity, retryPolicy: Schedule.recurs(0).pipe(Schedule.tap(() => ended.open)) })),
                      Effect.provideService(RpcClient.ConnectionHooks, {
                          onConnect: Effect.flatMap(Deferred.await(made), (entry) => SubscriptionRef.update(links, Array.prepend(entry))),
                          onDisconnect: Effect.flatMap(Deferred.await(made), (entry) => SubscriptionRef.update(links, Array.difference([entry]))),
                      }),
                      Effect.provideService(Socket.Socket, socket),
                      Effect.provide(WIRE),
                  );
                  return yield* Effect.as(ended.await, HttpServerResponse.empty());
              })
            : Effect.succeed(HttpServerResponse.empty({ status: 403 })),
    );
    return {
        links: Effect.map(SubscriptionRef.get(links), Array.map(Struct.get('peer'))),
        call: <A, E, R>(select: (peer: Peer) => boolean, use: (client: Entry['client']) => Effect.Effect<A, E, R>): Effect.Effect<A, Exclude<E, RpcClientError.RpcClientError> | LinkError, R> =>
            SubscriptionRef.changes(links).pipe(
                Stream.filterMap(Filter.fromPredicateOption(Array.findFirst((entry: Entry) => select(entry.peer)))),
                Stream.timeout(REDIAL),
                Stream.runHead,
                Effect.flatMap(Effect.fromOption(() => new LinkError({ peer, reason: { _tag: 'Unlinked' } }))),
                Effect.flatMap(({ client }) => use(client)),
                Effect.catchFilter(Filter.instanceOf(RpcClientError.RpcClientError), (cause) => Effect.fail(new LinkError({ peer, reason: { _tag: 'Dropped', cause } })), Effect.fail),
            ),
    };
});
const watched = <A, E, R, E2, R2>(peer: string, work: Effect.Effect<A, E, R>, waiting: Effect.Effect<boolean, E2, R2>): Effect.Effect<A, E | E2 | LinkError, R | R2> => {
    const seconds = 30;
    const first = Duration.millis(2);
    return Stream.fromSchedule(Schedule.min([Schedule.exponential(first), Schedule.spaced(Duration.seconds(1))])).pipe(
        Stream.mapEffect(() => Effect.zip(waiting, Clock.currentTimeMillis), { concurrency: 'unbounded', unordered: true }),
        Stream.mapAccum(Option.none<number>, (since, [held, at]) => {
            const from = held ? Option.orElseSome(since, () => at) : Option.none();
            return [from, Option.toArray(Option.filter(from, (start) => at - start >= Duration.toMillis(Duration.seconds(seconds))))] as const;
        }),
        Stream.runHead,
        Effect.andThen(Effect.fail(new LinkError({ peer, reason: { _tag: 'Stalled', seconds } }))),
        Effect.raceFirst(work),
    );
};

// --- [TOOLS]
const decoded = <S extends Schema.Codec<unknown, unknown>>(schema: S): ((payload: unknown) => Effect.Effect<S['Type'], McpSchema.InvalidParams>) => {
    const decode = Schema.decodeUnknownEffect(schema);
    return (payload) => Effect.mapError(decode(payload, STRICT), ({ message }) => new McpSchema.InvalidParams({ message }));
};
const register = Effect.fn('register')(function* <T extends Verb<Schema.Codec<unknown, unknown>>>(tool: T, prepare: (parameters: T['parametersSchema']['Type']) => Effect.Effect<readonly [Option.Option<string>, Work<T>], T['failureSchema']['Type'] | McpSchema.InvalidParams>) {
    const interval = Duration.minutes(1);
    const document = (schema: Schema.Top): unknown => {
        const { schema: root, definitions } = Schema.toJsonSchemaDocument(schema, { onExcessProperty: STRICT.onExcessProperty });
        return { ...root, type: 'object', ...(Record.isEmptyRecord(definitions) ? {} : { $defs: definitions }) };
    };
    const hint = (reference: Context.Reference<boolean>): boolean => Context.get(tool.annotations, reference);
    const codec = Schema.toCodecJson(tool.parametersSchema);
    const [server, executions, inputSchema, outputSchema] = yield* Effect.all([McpServer.McpServer, Executions, Effect.orDie(Schema.decodeUnknownEffect(McpSchema.ToolJson)(document(tool.parametersSchema))), Effect.orDie(Schema.decodeUnknownEffect(McpSchema.ToolOutputJson)(document(tool.successSchema)))]);
    const [decode, echo, encode, token] = [decoded(codec), Schema.encodeEffect(codec), Schema.encodeUnknownEffect(Schema.toCodecJson(tool.successSchema)), Schema.decodeUnknownOption(Schema.Struct({ progressToken: McpSchema.ProgressToken }))];
    yield* server.addTool({
        annotations: tool.annotations,
        tool: new McpSchema.Tool({
            name: tool.name,
            description: Tool.getDescription(tool),
            inputSchema,
            outputSchema,
            annotations: { readOnlyHint: hint(Tool.Readonly), destructiveHint: hint(Tool.Destructive), idempotentHint: hint(Tool.Idempotent), openWorldHint: hint(Tool.OpenWorld) },
        }),
        handle: (payload: unknown) =>
            decode(payload).pipe(
                Effect.flatMap((parameters) =>
                    Effect.map(
                        Effect.catchIf(prepare(parameters), Schema.is(McpSchema.InvalidParams), Effect.fail, (error) => Effect.succeed<readonly [Option.Option<string>, Work<T>]>([Option.none(), Effect.fail(error)])),
                        ([activity, work]) => [parameters, activity, work] as const,
                    ),
                ),
                Effect.flatMap(([parameters, activity, work]) => {
                    const answer = work.pipe(
                        Effect.flatMap(([success, blocks]) => Effect.map(Effect.orDie(encode(success)), (structuredContent) => [structuredContent, blocks] as const)),
                        Effect.match({
                            onSuccess: ([structuredContent, blocks]) => new McpSchema.CallToolResult({ structuredContent, content: [McpSchema.TextContent.make({ text: JSON.stringify(structuredContent) }), ...blocks] }),
                            onFailure: (error) => new McpSchema.CallToolResult({ isError: true, content: [McpSchema.TextContent.make({ text: error.message })] }),
                        }),
                    );
                    const kept = hint(Tool.Destructive) ? executions.keep({ tool: tool.name, arguments: Effect.orDie(echo(parameters)) }, answer) : answer;
                    return Effect.flatMap(McpSchema.McpRequestContext, ({ requestMetadata }) =>
                        Option.match(token(requestMetadata), {
                            onNone: () => kept,
                            onSome: ({ progressToken }) =>
                                Effect.flatMap(Schedule.CurrentMetadata, ({ attempt }) => server.notifications['notifications/progress']({ progressToken, progress: attempt, message: Option.getOrElse(activity, () => tool.name) })).pipe(Effect.repeat(Schedule.spaced(interval)), Effect.forkChild, Effect.andThen(kept)),
                        }),
                    );
                }),
                Effect.catchDefect((defect) => Effect.as(Effect.logError(defect), new McpSchema.CallToolResult({ isError: true, content: [McpSchema.TextContent.make({ text: Formatter.format(defect) })] }))),
            ),
    });
});

// --- [EXTENSIONS]
const make = <const U extends Verbs<U>, const V extends Verbs<V>, S extends Schema.Top, P extends Schema.Codec<unknown, unknown>, F extends Resolver['success'], X extends Resolver['failure'], E, R>(
    definition: {
        readonly name: string;
        readonly serves: S;
        readonly instructions: string;
        readonly preview: P;
        readonly runs: U;
        readonly verbs: V;
        readonly activity: { readonly [K in keyof V]?: (parameters: V[K]['parametersSchema']['Type']) => string };
        readonly face: { readonly success: F; readonly failure: X };
    },
    handlers: Effect.Effect<
        {
            readonly linked?: (target: S['Type']) => Effect.Effect<Option.Option<string>>;
            readonly runs: Handled<S['Type'], U, [Unit<P>]>;
            readonly verbs: Handled<S['Type'], V, []>;
            readonly face: (target: S['Type'], name: string, characters: string) => Effect.Effect<readonly [F['Type'], Option.Option<Source>], X['Type']>;
        },
        E,
        R
    >,
): Extension<E, R> => {
    const Service = Context.Service<Dispatch>(`@rasm/ai/extension/${definition.name}`);
    const served = Option.liftPredicate(Schema.is(definition.serves));
    const previewed = Schema.is(unit(definition.preview).fields.preview);
    const hosted = <W extends Verbs<W>, A extends readonly unknown[]>(forms: W, prepare: <K extends keyof W & string>(name: K, target: S['Type'], parameters: W[K]['parametersSchema']['Type'], ...context: A) => Effect.Effect<readonly [Option.Option<string>, Work<Verb>], McpSchema.InvalidParams>): readonly Route<A>[] =>
        Array.map(Struct.keys(forms), <K extends keyof W & string>(name: K): Route<A> => {
            const decode = decoded(forms[name].parametersSchema);
            return {
                tool: forms[name],
                serve: (target) =>
                    Option.map(
                        served(target),
                        (narrowed) =>
                            (payload, ...context) =>
                                Effect.flatMap(decode(payload), (parameters) => prepare(name, narrowed, parameters, ...context)),
                    ),
            };
        });
    return {
        name: definition.name,
        instructions: definition.instructions,
        Service,
        layer: Layer.effect(
            Service,
            Effect.map(handlers, ({ linked, runs, verbs, face }) => ({
                linked: (target) => Option.flatMap(served(target), (narrowed) => Option.map(Option.fromUndefinedOr(linked), (read) => read(narrowed))),
                preview: definition.preview,
                runs: hosted<U, [Unit<Schema.Top>]>(definition.runs, (name, target, parameters, { description, preview }) =>
                    previewed(preview) ? Effect.succeed([Option.some(description), runs[name](target, parameters, { description, preview })] as const) : Effect.fail(new McpSchema.InvalidParams({ message: `preview is no target ${definition.name} renders` })),
                ),
                verbs: hosted<V, []>(definition.verbs, (name, target, parameters) => Effect.succeed([Option.fromUndefinedOr(definition.activity[name]?.(parameters)), verbs[name](target, parameters)] as const)),
                face: { ...definition.face, serve: (target) => Option.map(served(target), (narrowed) => (name, characters) => face(narrowed, name, characters)) },
            })),
        ),
    };
};
const routed = Effect.fn('routed')(function* (extensions: readonly Extension<unknown, unknown>[]) {
    const dispatches = yield* Effect.forEach(extensions, Struct.get('Service'));
    const joined = (members: Array.NonEmptyReadonlyArray<Schema.Codec<unknown, unknown>>): Schema.Codec<unknown, unknown> => {
        const [only, ...rest] = Array.dedupeWith(members, (left, right) => left === right);
        return Array.isReadonlyArrayNonEmpty(rest) ? Schema.Union([only, ...rest]) : only;
    };
    const folded = <A extends readonly unknown[]>(forms: (dispatch: Dispatch) => readonly Route<A>[]): readonly Route<A>[] =>
        pipe(
            Array.flatMap(dispatches, forms),
            Array.groupBy(({ tool }) => tool.name),
            Record.values,
            Array.map((hosts): Route<A> => {
                const hints = (reference: Context.Reference<boolean>): readonly boolean[] => Array.map(hosts, ({ tool }) => Context.get(tool.annotations, reference));
                const fields = pipe(
                    Array.flatMap(hosts, ({ tool }) => Record.toEntries(tool.parametersSchema.fields)),
                    Array.groupBy(([key]) => key),
                    Record.map((entries) => {
                        const field = joined(Array.map(entries, ([, schema]) => schema));
                        return entries.length === hosts.length ? field : Schema.optionalKey(field);
                    }),
                    Schema.Struct,
                );
                const encode = Schema.encodeUnknownEffect(fields);
                return {
                    tool: Tool.make(Array.headNonEmpty(hosts).tool.name, {
                        description: Option.getOrUndefined(Array.findFirst(hosts, ({ tool }) => Option.fromUndefinedOr(Tool.getDescription(tool)))),
                        parameters: fields,
                        success: joined(Array.map(hosts, ({ tool }) => tool.successSchema)),
                        failure: Schema.Union(Array.map(hosts, ({ tool }) => tool.failureSchema)),
                    }).annotateMerge(Context.make(Tool.Readonly, Boolean.every(hints(Tool.Readonly))).pipe(Context.add(Tool.Idempotent, Boolean.every(hints(Tool.Idempotent))), Context.add(Tool.Destructive, Boolean.some(hints(Tool.Destructive))), Context.add(Tool.OpenWorld, Boolean.some(hints(Tool.OpenWorld))))),
                    serve: (target) =>
                        Option.map(
                            Array.findFirst(hosts, (host) => host.serve(target)),
                            (run) =>
                                (parameters, ...context) =>
                                    Effect.flatMap(Effect.orDie(encode(parameters)), (encoded) => run(encoded, ...context)),
                        ),
                };
            }),
        );
    return {
        runs: folded(Struct.get('runs')),
        verbs: folded(Struct.get('verbs')),
        faces: Array.map(dispatches, Struct.get('face')),
        resolve: (target: unknown): Option.Option<Resolve> =>
            Array.reduceRight(Array.getSomes(Array.map(dispatches, ({ face }) => face.serve(target))), Option.none<Resolve>(), (next, own) =>
                Option.some((name, characters) =>
                    Option.match(next, {
                        onNone: () => own(name, characters),
                        onSome: (fallback) =>
                            Effect.catchIf(
                                own(name, characters),
                                (error) => Schema.is(LinkError)(error) && error.reason._tag === 'Unlinked',
                                () => fallback(name, characters),
                            ),
                    }),
                ),
            ),
        preview: Array.match(
            Array.flatMap(dispatches, ({ runs, preview }) => (Array.isReadonlyArrayNonEmpty(runs) ? [preview] : [])),
            { onEmpty: (): Schema.Codec<unknown, unknown> => Schema.Never, onNonEmpty: joined },
        ),
    };
});
const publish = Effect.fn('publish')(function* <F extends Schema.Struct.Fields, Target, E extends Error>(
    routes: readonly Route<[]>[],
    { fields, failure, route, unserved }: { readonly fields: Schema.Struct<F>; readonly failure: Schema.Codec<E, unknown>; readonly route: (routing: Schema.Struct<F>['Type']) => Effect.Effect<Target, E>; readonly unserved: (target: Target) => E },
) {
    yield* Effect.forEach(
        routes,
        ({ tool, serve }) =>
            register(tool.setParameters(Schema.Struct(tool.parametersSchema.fields).pipe(Schema.fieldsAssign(fields.fields))).setFailure(Schema.Union([failure, tool.failureSchema])), (parameters) =>
                route(parameters).pipe(
                    Effect.flatMap((target) => Effect.fromOption(serve(target), () => unserved(target))),
                    Effect.flatMap((run) => run(parameters)),
                ),
            ),
        { discard: true },
    );
});

// --- [RANKING]
const ranked = (query: string, candidates: readonly string[]): readonly string[] => {
    const completions = 100;
    const [matched, info, order] = new uFuzzy({ intraMode: 1 }).search([...candidates], query, 0, Number.POSITIVE_INFINITY);
    const indexes = order === null ? matched : Array.getSomes(Array.map(order, (position) => Array.get(info.idx, position)));
    return Array.take(indexes === null ? candidates : Array.getSomes(Array.map(indexes, (index) => Array.get(candidates, index))), completions);
};

// --- [EXPORTS] -------------------------------------------------------------------------

export { Executions, type Extension, LinkError, listen, make, publish, READING, ranked, register, routed, WRITING, watched };
