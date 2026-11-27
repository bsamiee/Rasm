import { Handles } from '@rasm/mcp/handles';
import { atom, type EngineInterface, type On, read, update } from 'claude-code';
import { Array, Boolean, Cache, Cause, Context, Data, Duration, Effect, Exit, FiberMap, Formatter, Function, HashSet, Layer, Logger, ManagedRuntime, MutableRef, Option, Predicate, Record, Ref, Result, Schedule, Schema, Stream, String, Struct, SubscriptionRef, SynchronizedRef } from 'effect';
import { HttpStatus } from 'effect/http';
import { HttpApiClient } from 'effect/http-api';
import { Host, NetAddress } from 'effect/net';

// --- [TYPES] ---------------------------------------------------------------------------

type Sequenced = Exclude<typeof Handles.Event.Type, { readonly _tag: 'Settled' }>;

interface Held {
    readonly seen: number;
    readonly pending: readonly Sequenced[];
    readonly unacked: Option.Option<number>;
}

interface Follower {
    readonly agent: Option.Option<string>;
    readonly urls: HttpApiClient.UrlBuilder<typeof Handles>;
    readonly bound: typeof Handles.Binding.Type;
    readonly held: SynchronizedRef.SynchronizedRef<Held>;
}

// --- [CONSTANTS] -----------------------------------------------------------------------

const PERMISSION_PROMPT_NOTICE = Duration.seconds(6);
const MCP_RECONNECT = { first: Duration.seconds(1), within: Duration.minutes(30) } as const;

// --- [MODELS] --------------------------------------------------------------------------

const Claimed = Schema.Struct({ ok: Schema.Literal(true), text: Schema.fromJsonString(Handles.Binding) });
const Event = Schema.fromJsonString(Handles.Event);
const Written = Schema.fromJsonString(Schema.Struct({ http_code: Schema.Int, errormsg: Schema.NullOr(Schema.String) }));
const alarming = Schema.is(Handles.Kind.pick(['crashed', 'hung']));

class Binding extends Data.Class<{ readonly origin: string; readonly agent: Option.Option<string> }> {}

// --- [ERRORS] --------------------------------------------------------------------------

class FollowError extends Schema.TaggedError<FollowError, { readonly brand: unique symbol }>()('FollowError', {
    reason: Schema.TaggedUnion({ Gone: {}, Dropped: { status: Schema.Int }, Unstarted: {} }),
    subject: Schema.String,
    cause: Schema.optionalKey(Schema.Defect()),
    message: Schema.String,
}) {
    static readonly of = (reason: FollowError['reason'], subject: string) => (cause?: unknown) => FollowError.make({ reason, subject, ...(cause === undefined ? {} : { cause }), message: [`following ${subject} ${String.toLowerCase(reason._tag)}`, ...(cause === undefined ? [] : [Formatter.format(cause)])].join(': ') });
}

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
const row = ({ server }: typeof Handles.Binding.Type, line: string) => `${server} · ${line}`;
const notice = ({ handle }: typeof Handles.Binding.Type, key: string) => `${handle}/${key}`;

// --- [COMPOSITION] ---------------------------------------------------------------------

function link($: EngineInterface) {
    return Layer.effect(
        Link,
        Effect.gen(function* () {
            const [scope, hosts, origins, busy, pings] = yield* Effect.all([
                Effect.scope,
                Effect.map(
                    Effect.orElseSucceed(
                        request('reading /etc/hosts', () => $.fs.read('/etc/hosts')),
                        Function.constant(''),
                    ),
                    Host.parseHostsFile,
                ),
                Ref.make<Record.ReadonlyRecord<string, string>>({}),
                SubscriptionRef.make(HashSet.empty<Option.Option<string>>()),
                FiberMap.make<string>(),
            ]);
            const resident = (origin: string) =>
                Result.match(Host.hostPortFromString(new URL(origin).host), {
                    onFailure: Function.constFalse,
                    onSuccess: ({ host }) => (Host.isDomainName(host) ? (hosts.get(Host.toRelative(host)) ?? []) : Array.filter([host], NetAddress.isIpAddress)).some(NetAddress.isLoopback),
                });
            const note = (handle: string, change: (keyed: Record.ReadonlyRecord<string, string>) => Record.ReadonlyRecord<string, string>) =>
                request('updating waiting prompts', () =>
                    update($, atom({ plugin: 'risman', key: 'waiting' } as const, {}), (held) => {
                        const keyed = change(held[handle] ?? {});
                        return Record.isEmptyRecord(keyed) ? Record.remove(held, handle) : Record.set(held, handle, keyed);
                    }),
                );
            const ack = ({ urls, bound }: Follower, through: number) =>
                request('acknowledging events', () => $.http.fetch(urls.ack({ params: { handle: bound.handle }, query: { through } }), { method: Handles.groups.handles.endpoints.ack.method })).pipe(
                    Effect.filterOrFail(Struct.get('ok'), ({ status }) => new Cause.UnknownError(status, `acknowledging events answered ${status}`)),
                );
            const settle = Effect.fnUntraced(function* (follower: Follower, held: Held) {
                const { agent, bound } = follower;
                const lines = Array.filter(held.pending, Handles.Event.guards.Line);
                const awake = HashSet.has(yield* SubscriptionRef.get(busy), agent);
                const told = awake ? held.pending : lines;
                const text = [`MCP server ${bound.server} reports:`, ...Array.map(told, Struct.get('line'))].join('\n');
                const delivered =
                    !Array.isReadonlyArrayNonEmpty(told) ||
                    (yield* (
                        awake
                            ? Effect.map(
                                  request('session.append', () => $.session.append({ message: { type: 'user', content: [{ type: 'text', text }] }, ...Record.getSomes({ agentId: agent }) })),
                                  ({ deny }) => deny === undefined,
                              )
                            : Option.match(agent, {
                                  onNone: () =>
                                      Effect.map(
                                          request('prompt.submit', () => $.prompt.submit({ text })),
                                          ({ drop }) => drop === undefined,
                                      ),
                                  onSome: (agentId) =>
                                      Effect.map(
                                          request('session.send', () => $.session.send({ to: { agentId }, text })),
                                          Struct.get('isDelivered'),
                                      ),
                              })
                    ).pipe(Effect.tapCause(Effect.logWarning), Effect.orElseSucceed(Function.constFalse)));
                yield* Effect.when(SubscriptionRef.update(busy, HashSet.add(agent)), Effect.succeed(delivered && !awake && Array.isReadonlyArrayNonEmpty(lines)));
                const through = Option.orElse(Option.map(Array.last(held.pending), Struct.get('sequence')), () => held.unacked);
                return delivered
                    ? {
                          ...held,
                          pending: [],
                          unacked: yield* Option.match(through, {
                              onNone: () => Effect.succeedNone,
                              onSome: (sequence) =>
                                  ack(follower, sequence).pipe(
                                      Effect.as(Option.none<number>()),
                                      Effect.tapCause(Effect.logWarning),
                                      Effect.orElseSucceed(() => Option.some(sequence)),
                                  ),
                          }),
                      }
                    : held;
            });
            const absorb = Effect.fnUntraced(function* (follower: Follower, held: Held, events: Array.NonEmptyReadonlyArray<typeof Handles.Event.Type>) {
                const { bound } = follower;
                const fresh = yield* Effect.filterMapEffect(
                    Array.filter(events, (event) => Handles.Event.guards.Settled(event) || event.sequence > held.seen),
                    Handles.Event.match({
                        Line: (event) => Effect.as(alarming(event.kind) ? Effect.sync(() => $.ui.toast(row(bound, event.line))) : Effect.void, Result.succeed<Sequenced>(event)),
                        Waiting: (event) => {
                            const shown = row(bound, event.line);
                            const key = notice(bound, event.key);
                            const ping = Effect.sleep(PERMISSION_PROMPT_NOTICE).pipe(Effect.andThen(Effect.uninterruptible(request('notifying', () => $.ui.notify(shown, { key })))), Effect.tapCause(Effect.logWarning));
                            return note(bound.handle, Record.set(event.key, shown)).pipe(Effect.andThen(FiberMap.run(pings, key, ping)), Effect.catchCause(Effect.logWarning), Effect.as(Result.succeed<Sequenced>(event)));
                        },
                        Settled: (event) => {
                            const key = notice(bound, event.key);
                            return note(bound.handle, Record.remove(event.key)).pipe(Effect.andThen(FiberMap.remove(pings, key)), Effect.andThen(request('cancelling a notification', () => $.ui.cancelNotify(key))), Effect.catchCause(Effect.logWarning), Effect.as(Result.failVoid));
                        },
                    }),
                );
                return yield* settle(follower, { seen: Math.max(held.seen, ...Array.map(fresh, Struct.get('sequence'))), pending: [...held.pending, ...fresh], unacked: held.unacked });
            });
            const receive = (follower: Follower) => (events: Array.NonEmptyReadonlyArray<typeof Handles.Event.Type>) => SynchronizedRef.updateEffect(follower.held, (held) => absorb(follower, held, events));
            const flush = (follower: Follower) => SynchronizedRef.updateEffect(follower.held, (held) => settle(follower, held));
            const attempt = Effect.fnUntraced(function* (follower: Follower) {
                const { urls, bound } = follower;
                const written = yield* Ref.make('');
                yield* Stream.fromAsyncIterable($.process.spawn({ argv: ['curl', '--silent', '--no-buffer', '--write-out', '%{stderr}%{json}', urls.follow({ params: { handle: bound.handle } })] }), FollowError.of({ _tag: 'Unstarted' }, bound.server)).pipe(
                    Stream.filterMapEffect((chunk) => (chunk.stream === 'stdout' ? Effect.succeed(Result.succeed(chunk.text)) : Effect.as(Ref.update(written, String.concat(chunk.text)), Result.failVoid))),
                    Stream.splitLines,
                    Stream.mapEffect((line) => Schema.decodeEffect(Event)(line)),
                    Stream.chunks,
                    Stream.runForEach(receive(follower)),
                );
                const { http_code: status, errormsg } = yield* Effect.flatMap(Ref.get(written), Schema.decodeEffect(Written));
                return yield* status === HttpStatus.fromLiteral('OK') ? Effect.void : FollowError.of(status === HttpStatus.fromLiteral('NotFound') ? { _tag: 'Gone' } : { _tag: 'Dropped', status }, bound.server)(errormsg ?? undefined);
            });
            const follow = (follower: Follower) =>
                Effect.raceFirst(
                    attempt(follower).pipe(Effect.retry({ while: (error) => Predicate.isTagged(error, 'FollowError') && error.reason._tag === 'Dropped', schedule: Schedule.exponential(MCP_RECONNECT.first).pipe(Schedule.upTo({ duration: MCP_RECONNECT.within })) }), Effect.forever),
                    SubscriptionRef.changes(busy).pipe(
                        Stream.map(HashSet.has(follower.agent)),
                        Stream.changes,
                        Stream.filter(Boolean.not),
                        Stream.runForEach(() => flush(follower)),
                    ),
                );
            const bindings: Cache.Cache<Binding, string, Cause.UnknownError | Schema.SchemaError> = yield* Cache.makeWith(
                Effect.fnUntraced(function* (binding: Binding) {
                    const urls = HttpApiClient.urlBuilder(Handles, { baseUrl: binding.origin });
                    const { text: bound } = yield* Effect.flatMap(
                        request('binding a handle', () => $.http.fetch(urls.bind(), { method: Handles.groups.handles.endpoints.bind.method })),
                        Schema.decodeUnknownEffect(Claimed),
                    );
                    const held = yield* SynchronizedRef.make<Held>({ seen: Number.NEGATIVE_INFINITY, pending: [], unacked: Option.none() });
                    yield* follow({ agent: binding.agent, urls, bound, held }).pipe(
                        Effect.ensuring(Effect.orDie(note(bound.handle, Record.empty))),
                        Effect.catchCause((cause) => Effect.logWarning(`stopped following ${bound.server}`, cause)),
                        Effect.ensuring(Cache.invalidate(bindings, binding)),
                        Effect.forkIn(scope),
                    );
                    return bound.handle;
                }),
                { capacity: Number.POSITIVE_INFINITY, timeToLive: (exit) => (Exit.isSuccess(exit) || Option.exists(Exit.findErrorOption(exit), Predicate.isTagged('SchemaError')) ? Duration.infinity : Duration.zero) },
            );
            const lookup = (find: (binding: Binding) => Effect.Effect<Option.Option<string>>) => (tool: string, agent: Option.Option<string>) =>
                Effect.flatMap(Ref.get(origins), (known) => Option.match(Record.get(known, tool), { onNone: () => Effect.succeedNone, onSome: (origin) => find(new Binding({ origin, agent })) }));
            return {
                describe: (tool, given) => Ref.update(origins, (known) => Option.match(Option.filter(given, resident), { onNone: () => Record.remove(known, tool), onSome: (local) => Record.set(known, tool, local) })),
                handle: lookup((binding) => Effect.option(Cache.get(bindings, binding))),
                held: lookup((binding) => Cache.getSuccess(bindings, binding)),
                turn: (agent, running) => SubscriptionRef.update(busy, running ? HashSet.add(agent) : HashSet.remove(agent)),
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
    on('ui.render', { component: 'AbovePrompt' }, ($, e, next) =>
        read($, atom({ plugin: 'risman', key: 'waiting' } as const, {})).then((held) => {
            const rows = Record.values(held).flatMap(Record.values);
            const { Box, Text } = $.ui.resolve(e);
            return e.props.hasSurvey || !Array.isReadonlyArrayNonEmpty(rows) ? next(e) : Box({ flexDirection: 'column', children: rows.map((shown) => Text({ wrap: 'truncate-end', children: shown })) });
        }),
    );
}

// --- [EXPORTS] -------------------------------------------------------------------------

export { register };
