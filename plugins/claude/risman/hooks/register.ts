import { Notice, request, runtime, say } from '@rasm/claude-code/platform';
import { Handles } from '@rasm/mcp/handles';
import { atom, type EngineInterface, type On, read, type ToolMcpServer, update } from 'claude-code';
import { Array, Cause, Context, Data, Duration, Effect, Equal, FiberMap, Function, HashMap, HashSet, Layer, MutableRef, Option, Record, Ref, Result, Schema, Stream, Struct, SubscriptionRef, SynchronizedRef } from 'effect';
import { HttpStatus } from 'effect/http';
import { HttpApiClient, HttpApiError } from 'effect/http-api';
import { Host, NetAddress } from 'effect/net';

// --- [TYPES] ---------------------------------------------------------------------------

interface Follower {
    readonly binding: Binding;
    readonly handle: string;
    readonly urls: HttpApiClient.UrlBuilder<typeof Handles>;
    readonly after: Ref.Ref<number>;
}

// --- [CONSTANTS] -----------------------------------------------------------------------

const HTTP_FETCH_LIMIT = Duration.seconds(30);
const ROWS = atom({ plugin: 'risman', key: 'rows' } as const, {});
const { endpoints } = Handles.groups.handles;

// --- [MODELS] --------------------------------------------------------------------------

const Claimed = Schema.Struct({ ok: Schema.Literal(true), text: Schema.fromJsonString(Handles.Binding) });
const Answered = Schema.Struct({ ok: Schema.Literal(true), text: Schema.fromJsonString(Handles.Polled) });

class Binding extends Data.Class<{ readonly origin: string; readonly agent: Option.Option<string> }> {}

// --- [SERVICES] ------------------------------------------------------------------------

class Link extends Context.Service<
    Link,
    {
        readonly describe: (tool: string, server: Option.Option<ToolMcpServer>) => Effect.Effect<void>;
        readonly resident: (tool: string) => Effect.Effect<boolean>;
        readonly bind: (tool: string, agent: Option.Option<string>) => Effect.Effect<Option.Option<string>>;
        readonly turn: (agent: Option.Option<string>, running: boolean) => Effect.Effect<void>;
        readonly report: (agent: string) => Effect.Effect<void>;
    }
>()('@rasm/risman/hooks/register/Link') {}

// --- [OPERATIONS] ----------------------------------------------------------------------

const loopback = ({ type, origin }: ToolMcpServer) =>
    Option.filter(Option.fromUndefinedOr(origin), (at) => {
        const { hostname } = new URL(at);
        return (
            type === 'http' &&
            Result.match(Host.hostFromString(hostname), {
                onFailure: () => hostname === NetAddress.formatUrlHost(NetAddress.ipv6Loopback),
                onSuccess: (host) => (Host.isDomainName(host) ? Host.toRelative(host) === 'localhost' : NetAddress.isIpAddress(host) && NetAddress.isLoopback(host)),
            })
        );
    });
const owns =
    (agent: Option.Option<string>) =>
    ({ binding }: Follower) =>
        Equal.equals(binding.agent, agent);

// --- [COMPOSITION] ---------------------------------------------------------------------

function link($: EngineInterface) {
    const tell = say($);
    const send = (name: keyof typeof endpoints, url: string) => request('http', name, url, () => $.http.fetch(url, { method: endpoints[name].method }));
    const show = (handle: string, rows: Record.ReadonlyRecord<string, string>) =>
        request('store', 'read', ROWS.ref.key, () => read($, ROWS)).pipe(
            Effect.map((shown) => Record.toEntries(Record.difference(rows, shown[handle] ?? {}))),
            Effect.tap(request('store', 'update', ROWS.ref.key, () => update($, ROWS, (shown) => (Record.isEmptyRecord(rows) ? Record.remove(shown, handle) : Record.set(shown, handle, rows))))),
            Effect.flatMap(Effect.forEach(([press, text]: readonly [string, string]) => tell(Record.has(rows, press) ? Notice.Notify({ text, key: press }) : Notice.Withdraw({ key: press })), { discard: true })),
            Effect.catchCause(Effect.logWarning),
            Effect.uninterruptible,
        );
    return Layer.effect(
        Link,
        Effect.gen(function* () {
            const [origins, residents, busy, held, polls] = yield* Effect.all([
                Ref.make<Record.ReadonlyRecord<string, string>>({}),
                Ref.make<Record.ReadonlyRecord<string, boolean>>({}),
                SubscriptionRef.make(HashSet.empty<Option.Option<string>>()),
                SynchronizedRef.make(HashMap.empty<Binding, Follower>()),
                FiberMap.make<string>(),
            ]);
            yield* request('store', 'read', ROWS.ref.key, () => read($, ROWS)).pipe(
                Effect.flatMap((shown) => Effect.forEach(Record.keys(shown), (handle) => show(handle, {}), { discard: true })),
                Effect.catchCause(Effect.logWarning),
            );
            const forget = (follower: Follower) => SynchronizedRef.update(held, (known) => (Option.exists(HashMap.get(known, follower.binding), (kept) => kept === follower) ? HashMap.remove(known, follower.binding) : known));
            const take = Effect.fnUntraced(function* (follower: Follower, revision: number, polled: typeof Handles.Polled.Type) {
                const rows = Record.fromIterableWith(polled.rows.items, ({ key, text }) => [follower.urls.press({ params: { handle: follower.handle, key } }), text]);
                yield* Effect.when(show(follower.handle, rows), Effect.succeed(polled.rows.revision !== revision));
                const idle = !HashSet.has(yield* SubscriptionRef.get(busy), follower.binding.agent);
                yield* Array.last(polled.notices).pipe(
                    Option.filter(() => !idle || polled.notices.some(Struct.get('wake'))),
                    Option.match({
                        onNone: () => Effect.void,
                        onSome: ({ sequence }) =>
                            tell(Notice.Model({ agent: follower.binding.agent, idle, text: Array.map(polled.notices, Struct.get('text')).join('\n') })).pipe(
                                Effect.filterOrFail(Function.identity, () => new Cause.UnknownError(sequence, 'notices undelivered')),
                                Effect.andThen(Ref.set(follower.after, sequence)),
                            ),
                    }),
                );
                return polled.rows.revision;
            });
            const round = Effect.fnUntraced(function* (follower: Follower, revision: number) {
                const { agent } = follower.binding;
                const idle = !HashSet.has(yield* SubscriptionRef.get(busy), agent);
                const turned = SubscriptionRef.changes(busy).pipe(
                    Stream.filter((running) => HashSet.has(running, agent) === idle),
                    Stream.runHead,
                    Effect.as(Option.none<typeof Handles.Polled.Type>()),
                );
                const query = { after: yield* Ref.get(follower.after), rows: revision, idle, wait: Duration.toSeconds(HTTP_FETCH_LIMIT) / 2 };
                const polled = yield* send('poll', follower.urls.poll({ params: { handle: follower.handle }, query })).pipe(
                    Effect.filterOrFail(
                        ({ status }) => status !== HttpStatus.fromLiteral('NotFound'),
                        () => HttpApiError.NotFound.make(),
                    ),
                    Effect.flatMap(Schema.decodeUnknownEffect(Answered)),
                    Effect.tapError(() => show(follower.handle, {})),
                    Effect.tapErrorTag('NotFound', () => forget(follower)),
                    Effect.map(({ text }) => Option.some(text)),
                    Effect.raceFirst(turned),
                );
                const next = yield* Option.match(polled, { onNone: () => Effect.succeed(revision), onSome: (answer) => take(follower, revision, answer) });
                return [Array.empty<never>(), Option.some(next)] as const;
            });
            const follow = (follower: Follower) =>
                Stream.paginate(0, (revision: number) => round(follower, revision)).pipe(
                    Stream.runDrain,
                    Effect.catchCause((cause) => Effect.logWarning('poll ended', cause)),
                    Effect.annotateLogs({ origin: follower.binding.origin, handle: follower.handle }),
                );
            const claim = Effect.fnUntraced(function* (binding: Binding, bindings: HashMap.HashMap<Binding, Follower>) {
                const urls = HttpApiClient.urlBuilder(Handles, { baseUrl: binding.origin });
                const answer = yield* send('bind', urls.bind());
                const claimed = yield* Effect.result(Schema.decodeUnknownEffect(Claimed)(answer));
                yield* Effect.when(
                    Ref.update(residents, (known) => Record.set(known, binding.origin, Result.isSuccess(claimed))),
                    Effect.succeed(Result.isSuccess(claimed) || answer.status < HttpStatus.fromLiteral('InternalServerError')),
                );
                const { text } = yield* Effect.fromResult(claimed);
                const follower: Follower = { binding, handle: text.handle, urls, after: yield* Ref.make(0) };
                yield* FiberMap.run(polls, text.handle, follow(follower));
                return [text.handle, HashMap.set(bindings, binding, follower)] as const;
            });
            const resume = (agent: Option.Option<string>) => SynchronizedRef.get(held).pipe(Effect.flatMap((known) => Effect.forEach(HashMap.values(HashMap.filter(known, owns(agent))), (follower) => FiberMap.run(polls, follower.handle, follow(follower), { onlyIfMissing: true }), { discard: true })));
            const quit = (owned: (follower: Follower) => boolean) =>
                SynchronizedRef.getAndUpdate(held, (known) => HashMap.filter(known, (follower) => !owned(follower))).pipe(
                    Effect.flatMap((known) =>
                        Effect.forEach(
                            HashMap.values(HashMap.filter(known, owned)),
                            (follower) => FiberMap.remove(polls, follower.handle).pipe(Effect.andThen(show(follower.handle, {})), Effect.andThen(send('release', follower.urls.release({ params: { handle: follower.handle } }))), Effect.catchCause(Effect.logWarning)),
                            { concurrency: 'unbounded', discard: true },
                        ),
                    ),
                );
            yield* Effect.addFinalizer(() => quit(Function.constTrue));
            return {
                describe: (tool, server) => Ref.update(origins, (known) => server.pipe(Option.flatMap(loopback), Option.match({ onNone: () => Record.remove(known, tool), onSome: (origin) => Record.set(known, tool, origin) }))),
                resident: (tool) => Ref.get(origins).pipe(Effect.map((known) => Record.has(known, tool))),
                bind: Effect.fnUntraced(function* (tool: string, agent: Option.Option<string>) {
                    yield* resume(agent);
                    const servers = yield* Ref.get(residents);
                    const origin = yield* Effect.fromOption(Option.filter(Record.get(yield* Ref.get(origins), tool), (at) => Option.getOrElse(Record.get(servers, at), Function.constTrue)));
                    const binding = new Binding({ origin, agent });
                    return yield* SynchronizedRef.modifyEffect(held, (known) =>
                        Option.match(HashMap.get(known, binding), {
                            onNone: () => claim(binding, known),
                            onSome: (follower) => Effect.succeed([follower.handle, known] as const),
                        }),
                    );
                }, Effect.option),
                turn: (agent, running) => SubscriptionRef.update(busy, running ? HashSet.add(agent) : HashSet.remove(agent)).pipe(Effect.andThen(Effect.when(resume(agent), Effect.succeed(running))), Effect.asVoid),
                report: (agent) => Option.some(agent).pipe(owns, quit),
            };
        }),
    );
}

function current($: EngineInterface, live: MutableRef.MutableRef<Option.Option<ReturnType<typeof runtime<Link, never>>>>) {
    return Option.getOrElse(MutableRef.get(live), () => {
        const made = runtime($, link($));
        MutableRef.set(live, Option.some(made));
        $.ui.invalidate('tool.describe');
        return made;
    });
}

function register(on: On) {
    const live = MutableRef.make(Option.none<ReturnType<typeof runtime<Link, never>>>());
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
    on('agent.report', ($, e, next) =>
        current($, live)
            .runPromise(Link.use((linked) => linked.report(e.agentId)))
            .then(() => next(e)),
    );
    on('tool.describe', ($, e, next) =>
        current($, live)
            .runPromise(Link.use((linked) => linked.describe(e.tool, Option.fromUndefinedOr(e.mcpServer))))
            .then(() => next(e)),
    );
    on('tool.check', ($, e, next) =>
        next(e).then((verdict) =>
            (verdict.decision === 'deny' && verdict.rule !== undefined) || verdict.ceiling === 'deny'
                ? verdict
                : current($, live)
                      .runPromise(
                          Link.use((linked) => linked.resident(e.tool)),
                          { signal: next.signal },
                      )
                      .then((resident) => (resident ? { decision: 'allow' as const } : verdict)),
        ),
    );
    on('tool.call', ($, e, next) =>
        current($, live)
            .runPromise(
                Link.use((linked) => linked.bind(e.tool, Option.fromUndefinedOr(e.agentId))),
                { signal: next.signal },
            )
            .then(Option.match({ onNone: () => next(e), onSome: (handle) => next({ ...e, handle }) })),
    );
    on('ui.render', { component: 'AbovePrompt' }, ($, e, next) =>
        read($, ROWS).then((held) => {
            const rows = Record.values(held).flatMap(Record.toEntries);
            const { Box, Button } = $.ui.resolve(e);
            return e.props.hasSurvey || !Array.isReadonlyArrayNonEmpty(rows) ? next(e) : Box({ flexDirection: 'column', children: rows.map(([press, text]) => Button({ key: press, label: text, onPress: () => $.http.fetch(press, { method: endpoints.press.method }) })) });
        }),
    );
}

// --- [EXPORTS] -------------------------------------------------------------------------

export { register };
