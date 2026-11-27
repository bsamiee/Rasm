import { Engine, Notice } from '@rasm/claude-code/engine';
import { host } from '@rasm/claude-code/host';
import { Handles } from '@rasm/mcp/handles';
import type { On, ToolMcpServer } from 'claude-code';
import { Array, Context, Data, Duration, Effect, Equal, FiberMap, Function, HashMap, HashSet, Layer, Option, Record, Ref, Result, Schema, Stream, Struct, SynchronizedRef } from 'effect';
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
        readonly report: (agent: string) => Effect.Effect<void>;
    }
>()('@rasm/risman/hooks/register/Link') {
    static readonly layer = Layer.effect(
        Link,
        Effect.gen(function* () {
            const engine = yield* Engine;
            const send = (name: keyof typeof endpoints, url: string) => engine.fetch(url, { method: endpoints[name].method });
            const show = (handle: string, rows: Record.ReadonlyRecord<string, string>) => Effect.catchCause(engine.deliver(Notice.Band({ group: handle, rows })), Effect.logWarning);
            const [origins, residents, held, polls] = yield* Effect.all([Ref.make<Record.ReadonlyRecord<string, string>>({}), Ref.make<Record.ReadonlyRecord<string, boolean>>({}), SynchronizedRef.make(HashMap.empty<Binding, Follower>()), FiberMap.make<string>()]);
            const forget = (follower: Follower) => SynchronizedRef.update(held, (known) => (Option.exists(HashMap.get(known, follower.binding), (kept) => kept === follower) ? HashMap.remove(known, follower.binding) : known));
            const take = Effect.fnUntraced(function* (follower: Follower, revision: number, polled: typeof Handles.Polled.Type) {
                const rows = Record.fromIterableWith(polled.rows.items, ({ key, text }) => [follower.urls.press({ params: { handle: follower.handle, key } }), text]);
                yield* Effect.when(show(follower.handle, rows), Effect.succeed(polled.rows.revision !== revision));
                const idle = !(yield* engine.isRunning(follower.binding.agent));
                yield* Array.last(polled.notices).pipe(
                    Option.filter(() => !idle || polled.notices.some(Struct.get('wake'))),
                    Option.match({
                        onNone: () => Effect.void,
                        onSome: ({ sequence }) => engine.deliver(Notice.Message({ agent: follower.binding.agent, text: Array.map(polled.notices, Struct.get('text')).join('\n') })).pipe(Effect.andThen(Ref.set(follower.after, sequence))),
                    }),
                );
                return polled.rows.revision;
            });
            const round = Effect.fnUntraced(function* (follower: Follower, revision: number) {
                const { agent } = follower.binding;
                const idle = !(yield* engine.isRunning(agent));
                const turned = engine.turns.pipe(
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
            yield* Effect.forkScoped(Stream.runForEach(engine.turns, (running) => Effect.forEach(running, resume, { discard: true })));
            yield* engine.invalidate('tool.describe');
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
                report: (agent) => Option.some(agent).pipe(owns, quit),
            };
        }),
    );
}

// --- [OPERATIONS] ----------------------------------------------------------------------

const loopback = ({ type, origin }: ToolMcpServer) =>
    Option.filter(Option.fromUndefinedOr(origin), (at) => {
        const { hostname } = new URL(at);
        return (
            type === 'http' &&
            Result.match(Host.hostFromString(hostname), {
                onFailure: () => hostname === NetAddress.formatUrlHost(NetAddress.ipv6Loopback),
                onSuccess: (parsed) => (Host.isDomainName(parsed) ? Host.toRelative(parsed) === 'localhost' : NetAddress.isIpAddress(parsed) && NetAddress.isLoopback(parsed)),
            })
        );
    });
const owns =
    (agent: Option.Option<string>) =>
    ({ binding }: Follower) =>
        Equal.equals(binding.agent, agent);

// --- [COMPOSITION] ---------------------------------------------------------------------

function register(on: On) {
    host(on, {
        layer: Link.layer,
        hooks: {
            'agent.report': (e, next) =>
                Effect.andThen(
                    Link.use((linked) => linked.report(e.agentId)),
                    next(e),
                ),
            'tool.describe': (e, next) =>
                Effect.andThen(
                    Link.use((linked) => linked.describe(e.tool, Option.fromUndefinedOr(e.mcpServer))),
                    next(e),
                ),
            'tool.check': (e, next) =>
                Effect.filterOrElse(
                    next(e),
                    (verdict) => (verdict.decision === 'deny' && verdict.rule !== undefined) || verdict.ceiling === 'deny',
                    (verdict) =>
                        Effect.map(
                            Link.use((linked) => linked.resident(e.tool)),
                            (resident) => (resident ? { decision: 'allow' as const } : verdict),
                        ),
                ),
            'tool.call': (e, next) =>
                Effect.flatMap(
                    Link.use((linked) => linked.bind(e.tool, Option.fromUndefinedOr(e.agentId))),
                    Option.match({ onNone: () => next(e), onSome: (handle) => next({ ...e, handle }) }),
                ),
        },
        band: { press: (url) => Engine.use((engine) => engine.fetch(url, { method: endpoints.press.method })) },
    });
}

// --- [EXPORTS] -------------------------------------------------------------------------

export { register };
