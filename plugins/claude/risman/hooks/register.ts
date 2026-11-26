import { Handles } from '@rasm/mcp/handles';
import type { EngineInterface, On } from 'claude-code';
import { Array, Cache, Cause, Context, Data, Duration, Effect, Exit, Function, HashSet, Layer, Logger, ManagedRuntime, MutableRef, Option, Predicate, Record, Ref, Result, Schema, Stream, Struct } from 'effect';
import { HttpApiClient } from 'effect/http-api';
import { Host, NetAddress } from 'effect/net';

// --- [MODELS] --------------------------------------------------------------------------

const Claimed = Schema.Struct({ ok: Schema.Literal(true), text: Schema.fromJsonString(Handles.Binding) });
const Event = Schema.fromJsonString(Handles.Event);

class Binding extends Data.Class<{ readonly origin: string; readonly agent: Option.Option<string> }> {}

// --- [SERVICES] ------------------------------------------------------------------------

class Link extends Context.Service<
    Link,
    {
        readonly describe: (tool: string, origin: Option.Option<string>) => Effect.Effect<void>;
        readonly handle: (tool: string, agent: Option.Option<string>) => Effect.Effect<Option.Option<string>>;
        readonly held: (tool: string, agent: Option.Option<string>) => Effect.Effect<Option.Option<string>>;
        readonly turn: (agent: Option.Option<string>, running: boolean) => Effect.Effect<void>;
    }
>()('@rasm/risman/hooks/register/Link') {}

// --- [OPERATIONS] ----------------------------------------------------------------------

const request = <A>(call: string, run: () => PromiseLike<A>) => Effect.tryPromise({ try: run, catch: (cause) => new Cause.UnknownError(cause, `${call} failed`) });

// --- [COMPOSITION] ---------------------------------------------------------------------

function link($: EngineInterface) {
    return Layer.effect(
        Link,
        Effect.gen(function* () {
            const [scope, hosts, origins, busy] = yield* Effect.all([
                Effect.scope,
                Effect.map(
                    Effect.orElseSucceed(
                        request('reading /etc/hosts', () => $.fs.read('/etc/hosts')),
                        Function.constant(''),
                    ),
                    Host.parseHostsFile,
                ),
                Ref.make<Record.ReadonlyRecord<string, string>>({}),
                Ref.make(HashSet.empty<Option.Option<string>>()),
            ]);
            const resident = (origin: string) =>
                Result.match(Host.hostPortFromString(new URL(origin).host), {
                    onFailure: Function.constFalse,
                    onSuccess: ({ host }) => (Host.isDomainName(host) ? (hosts.get(Host.toRelative(host)) ?? []) : Array.filter([host], NetAddress.isIpAddress)).some(NetAddress.isLoopback),
                });
            const deliver =
                (agent: Option.Option<string>, urls: HttpApiClient.UrlBuilder<typeof Handles>, { handle, server }: typeof Handles.Binding.Type) =>
                (lines: Array.NonEmptyReadonlyArray<typeof Handles.Event.Type>) => {
                    const text = [`MCP server ${server} reports:`, ...Array.map(lines, Struct.get('line'))].join('\n');
                    const last = Array.lastNonEmpty(lines);
                    return Effect.when(
                        request('acknowledging events', () => $.http.fetch(urls.ack({ params: { handle }, query: { through: last.sequence } }), { method: Handles.groups.handles.endpoints.ack.method })).pipe(Effect.filterOrFail(Struct.get('ok'))),
                        Ref.getAndUpdate(busy, HashSet.add(agent)).pipe(
                            Effect.flatMap((running) =>
                                HashSet.has(running, agent)
                                    ? Effect.map(
                                          request('session.append', () => $.session.append({ message: { type: 'user', content: [{ type: 'text', text }] }, ...Record.getSomes({ agentId: agent }) })),
                                          ({ deny }) => deny === undefined,
                                      )
                                    : Option.match(agent, {
                                          onNone: () =>
                                              Effect.as(
                                                  request('prompt.submit', () => $.prompt.submit({ text })),
                                                  true,
                                              ),
                                          onSome: (agentId) =>
                                              Effect.map(
                                                  request('session.send', () => $.session.send({ to: { agentId }, text })),
                                                  Struct.get('isDelivered'),
                                              ),
                                      }),
                            ),
                            Effect.tap((delivered) => (delivered ? Effect.void : Ref.update(busy, HashSet.remove(agent)))),
                        ),
                    ).pipe(Effect.andThen(Effect.sync(() => $.ui.status(`${server} ${last.line}`))));
                };
            const bindings: Cache.Cache<Binding, string, Cause.UnknownError | Schema.SchemaError> = yield* Cache.makeWith(
                Effect.fnUntraced(function* (binding: Binding) {
                    const urls = HttpApiClient.urlBuilder(Handles, { baseUrl: binding.origin });
                    const { text: bound } = yield* Effect.flatMap(
                        request('binding a handle', () => $.http.fetch(urls.bind(), { method: Handles.groups.handles.endpoints.bind.method })),
                        Schema.decodeUnknownEffect(Claimed),
                    );
                    yield* Stream.fromAsyncIterable($.process.spawn({ argv: ['curl', '--silent', '--fail', '--no-buffer', urls.follow({ params: { handle: bound.handle } })] }), (cause) => new Cause.UnknownError(cause)).pipe(
                        Stream.filterMap((chunk) => (chunk.stream === 'stdout' ? Result.succeed(chunk.text) : Result.failVoid)),
                        Stream.splitLines,
                        Stream.mapEffect((line) => Schema.decodeEffect(Event)(line)),
                        Stream.chunks,
                        Stream.runForEach(deliver(binding.agent, urls, bound)),
                        Effect.catchCause((cause) => Effect.logWarning(`stopped following ${bound.server}`, cause)),
                        Effect.ensuring(Cache.invalidate(bindings, binding)),
                        Effect.forkIn(scope),
                    );
                    return bound.handle;
                }),
                { capacity: Number.POSITIVE_INFINITY, timeToLive: (exit) => (Exit.isSuccess(exit) || Option.exists(Exit.findErrorOption(exit), Predicate.isTagged('SchemaError')) ? Duration.infinity : Duration.zero) },
            );
            const lookup = (read: (binding: Binding) => Effect.Effect<Option.Option<string>>) => (tool: string, agent: Option.Option<string>) =>
                Effect.flatMap(Ref.get(origins), (known) => Option.match(Record.get(known, tool), { onNone: () => Effect.succeedNone, onSome: (origin) => read(new Binding({ origin, agent })) }));
            return {
                describe: (tool, given) => Ref.update(origins, (known) => Option.match(Option.filter(given, resident), { onNone: () => Record.remove(known, tool), onSome: (local) => Record.set(known, tool, local) })),
                handle: lookup((binding) => Effect.option(Cache.get(bindings, binding))),
                held: lookup((binding) => Cache.getSuccess(bindings, binding)),
                turn: (agent, running) => Ref.update(busy, running ? HashSet.add(agent) : HashSet.remove(agent)),
            };
        }),
    ).pipe(Layer.provideMerge(Logger.layer([Logger.map(Logger.formatSimple, (line) => $.ui.log(line, { to: 'debug' }))])));
}

function current($: EngineInterface, live: MutableRef.MutableRef<Option.Option<ManagedRuntime.ManagedRuntime<Link, never>>>) {
    return Option.getOrElse(MutableRef.get(live), () => {
        const made = ManagedRuntime.make(link($));
        MutableRef.set(live, Option.some(made));
        $.ui.invalidate('tool.describe');
        return made;
    });
}

function register(on: On) {
    const live = MutableRef.make(Option.none<ManagedRuntime.ManagedRuntime<Link, never>>());
    on('session.start', ($, e, next) =>
        current($, live)
            .runPromise(Effect.void)
            .then(() => next(e)),
    );
    on('session.end', (_, e, next) => Promise.all(Option.toArray(Option.map(MutableRef.getAndSet(live, Option.none()), (ended) => ended.dispose()))).then(() => next(e)));
    on('turn.start', ($, e, next) =>
        current($, live)
            .runPromise(Link.use((linked) => linked.turn(Option.fromUndefinedOr(e.agentId), true)))
            .then(() => next(e)),
    );
    on('turn.complete', ($, e, next) =>
        current($, live)
            .runPromise(Link.use((linked) => linked.turn(Option.fromUndefinedOr(e.agentId), false)))
            .then(() => next(e)),
    );
    on('tool.describe', ($, e, next) =>
        current($, live)
            .runPromise(Link.use((linked) => linked.describe(e.tool, Option.fromUndefinedOr(e.mcpServer?.origin))))
            .then(() => next(e)),
    );
    on('tool.check', ($, e, next) =>
        current($, live)
            .runPromise(Link.use((linked) => linked.held(e.tool, Option.fromUndefinedOr(e.agentId))))
            .then(Option.match({ onNone: () => next(e), onSome: () => ({ decision: 'allow' as const }) })),
    );
    on('tool.call', ($, e, next) =>
        Predicate.hasProperty(e, 'handle')
            ? next(e)
            : current($, live)
                  .runPromise(Link.use((linked) => linked.handle(e.tool, Option.fromUndefinedOr(e.agentId))).pipe(Effect.timeoutOption(Duration.millis(next.budget.remainingMs)), Effect.map(Option.flatten)), { signal: next.signal })
                  .then(Option.match({ onNone: () => next(e), onSome: (handle) => next({ ...e, handle }) })),
    );
}

// --- [EXPORTS] -------------------------------------------------------------------------

export { register };
