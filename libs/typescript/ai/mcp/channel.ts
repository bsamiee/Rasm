import { Crypto, Deferred, Effect, Function, flow, HashMap, Match, PubSub, Ref, Result, Schema, Stream, Tuple } from 'effect';
import { McpProtocol, McpSchema } from 'effect/ai';
import { Sse } from 'effect/encoding';
import { Headers, HttpRouter, HttpServerRequest, HttpServerResponse, HttpStatus } from 'effect/http';

// --- [MODELS] --------------------------------------------------------------------------

const Message = Schema.Struct({ content: Schema.String, meta: Schema.Record(Schema.String, Schema.String) });
const Envelope = Schema.Struct({ jsonrpc: Schema.tag('2.0'), id: Schema.optionalKey(Schema.Union([Schema.String, Schema.Finite])), method: Schema.String, params: Schema.optionalKey(Schema.Json) });
const Notification = Schema.Struct({ jsonrpc: Envelope.fields.jsonrpc, method: Schema.tag('notifications/claude/channel'), params: Message });
const Reply = Schema.Union([Schema.Struct({ result: Schema.Union([McpSchema.Initialize.successSchema, McpSchema.Ping.successSchema]) }), Schema.Struct({ error: McpSchema.McpErrorBase })]).mapMembers(Tuple.map(Schema.fieldsAssign({ jsonrpc: Envelope.fields.jsonrpc, id: Envelope.fields.id.schema })));

// --- [OPERATIONS] ----------------------------------------------------------------------

const channel = Effect.fn('channel')(function* <R>({ name, version, path, instructions, messages }: { readonly name: string; readonly version: string; readonly path: HttpRouter.PathInput; readonly instructions: string; readonly messages: Stream.Stream<typeof Message.Type, never, R> }) {
    const [router, crypto, published, sessions] = yield* Effect.all([HttpRouter.HttpRouter, Crypto.Crypto, PubSub.unbounded<string>(), Ref.make(HashMap.empty<string, Deferred.Deferred<void>>())]);
    const header = 'mcp-session-id';
    const { protocolVersion } = McpProtocol.v2025_11_25;
    const initialized = McpSchema.InitializeResult.make({ protocolVersion, capabilities: McpSchema.ServerCapabilities.make({ experimental: { 'claude/channel': {} } }), serverInfo: McpSchema.Implementation.make({ name, version }), instructions });
    const reply = HttpServerResponse.schemaJson(Reply);
    const encode = Schema.encodeEffect(Schema.fromJsonString(Schema.toCodecJson(Notification)));
    const answer = (status: HttpStatus.Literal) => Effect.succeed(HttpServerResponse.empty({ status: HttpStatus.fromLiteral(status) }));
    const known = Effect.gen(function* () {
        const [{ headers }, table] = yield* Effect.all([HttpServerRequest.HttpServerRequest, Ref.get(sessions)]);
        const id = Headers.has(headers, 'origin') ? Result.fail(answer('Forbidden')) : Result.fromOption(Headers.get(headers, header), () => answer('BadRequest'));
        return Result.all([
            id,
            Result.flatMap(
                id,
                flow(
                    (found: string) => HashMap.get(table, found),
                    Result.fromOption(() => answer('NotFound')),
                ),
            ),
        ]);
    });
    const held = (handle: (id: string, done: Deferred.Deferred<void>) => Effect.Effect<HttpServerResponse.HttpServerResponse>) => Effect.flatMap(known, Result.match({ onFailure: Function.identity, onSuccess: ([id, done]) => handle(id, done) }));
    yield* Effect.forkScoped(
        Effect.tapCause(
            Stream.runIntoPubSub(
                Stream.mapEffect(messages, (params) => Effect.orDie(encode(Notification.make({ params })))),
                published,
            ),
            Effect.logError,
        ),
    );
    yield* router.add(
        'GET',
        path,
        held((_, done) =>
            Effect.succeed(
                HttpServerResponse.stream(
                    Stream.fromPubSub(published).pipe(
                        Stream.map((data) => Sse.encoder.write({ _tag: 'Event', event: 'message', id: undefined, data })),
                        Stream.merge(Stream.tick('15 seconds').pipe(Stream.drop(1), Stream.as(': keepalive\n\n')), { haltStrategy: 'left' }),
                        Stream.interruptWhen(Deferred.await(done)),
                        Stream.encodeText,
                    ),
                    { contentType: 'text/event-stream' },
                ),
            ),
        ),
    );
    yield* router.add(
        'DELETE',
        path,
        held((id, done) => Ref.update(sessions, HashMap.remove(id)).pipe(Effect.andThen(Deferred.succeed(done, undefined)), Effect.as(HttpServerResponse.empty({ status: 200 })))),
    );
    yield* router.add(
        'POST',
        path,
        Effect.matchEffect(Effect.all([HttpServerRequest.schemaBodyJson(Envelope), known]), {
            onFailure: () => answer('BadRequest'),
            onSuccess: ([envelope, session]) => {
                const admitted = <E, R2>(response: Effect.Effect<HttpServerResponse.HttpServerResponse, E, R2>) => Result.match(session, { onFailure: Function.identity, onSuccess: () => response });
                return Match.value(envelope).pipe(
                    Match.when({ method: 'initialize', id: Match.defined }, ({ jsonrpc, id }) =>
                        Effect.flatMap(Effect.all([crypto.randomUUIDv7(), Deferred.make<void>()]), ([created, done]) => Effect.andThen(Ref.update(sessions, HashMap.set(created, done)), reply({ jsonrpc, id, result: initialized }, { headers: { [header]: created } }))),
                    ),
                    Match.when({ method: 'server/discover', id: Match.defined }, ({ jsonrpc, id }) => reply({ jsonrpc, id, error: McpSchema.McpErrorBase.make({ code: -32_022, message: 'Unsupported protocol version', data: { supported: [protocolVersion] } }) }, { status: 400 })),
                    Match.when({ method: 'ping', id: Match.defined }, ({ jsonrpc, id }) => admitted(reply({ jsonrpc, id, result: {} }))),
                    Match.when({ id: Match.defined }, ({ jsonrpc, id }) => admitted(reply({ jsonrpc, id, error: McpSchema.McpErrorBase.make({ code: McpSchema.METHOD_NOT_FOUND_ERROR_CODE, message: 'Method not found' }) }))),
                    Match.orElse(() => admitted(answer('Accepted'))),
                );
            },
        }).pipe(Effect.orDie),
    );
});

// --- [EXPORTS] -------------------------------------------------------------------------

export { channel, Message };
