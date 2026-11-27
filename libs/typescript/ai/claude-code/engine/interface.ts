import type { AgentCallArgs, AgentInfo, AgentStatus, EngineInterface, HttpInit, HttpResponse, InvalidatableEventName, ToolCallResult } from 'claude-code';
import { Array, Context, Data, Effect, HashSet, Layer, Match, Option, PlatformError, Record, Schema, Semaphore, type Stream, SubscriptionRef } from 'effect';
import { call } from '#/engine/services';

// --- [TYPES] ---------------------------------------------------------------------------

type Loop = Option.Option<string>;
type Notice = Data.TaggedEnum<{
    Toast: { readonly text: string };
    Notification: { readonly key: string; readonly text: Option.Option<string> };
    Band: { readonly group: string; readonly rows: Readonly<Record<string, string>> };
    Message: { readonly text: string; readonly agent: Loop };
}>;
interface Band {
    readonly notify: boolean;
}

// --- [CONSTANTS] -----------------------------------------------------------------------

const bandKey = 'band';
const ended = { pending: false, running: false, waiting: false, idle: false, completed: true, failed: true, killed: true } as const satisfies Record<AgentStatus, boolean>;

// --- [MODELS] --------------------------------------------------------------------------

const Notice = Data.taggedEnum<Notice>();
const Rows = Schema.UndefinedOr(Schema.Record(Schema.String, Schema.Record(Schema.String, Schema.String)));

// --- [SERVICES] ------------------------------------------------------------------------

class Engine extends Context.Service<
    Engine,
    {
        readonly deliver: (notice: Notice) => Effect.Effect<void, PlatformError.PlatformError | Schema.SchemaError>;
        readonly invalidate: (event: InvalidatableEventName) => Effect.Effect<void>;
        readonly fetch: (url: string, init?: HttpInit) => Effect.Effect<HttpResponse, PlatformError.PlatformError>;
        readonly callTool: (args: AgentCallArgs) => Effect.Effect<ToolCallResult, PlatformError.PlatformError>;
        readonly compile: (source: string) => Effect.Effect<WebAssembly.Module, PlatformError.PlatformError>;
        readonly cwd: Effect.Effect<string, PlatformError.PlatformError>;
        readonly findAgent: (id: string) => Effect.Effect<Option.Option<AgentInfo>, PlatformError.PlatformError>;
        readonly isRunning: (agent: Loop) => Effect.Effect<boolean>;
        readonly turns: Stream.Stream<HashSet.HashSet<Loop>>;
    }
>()('@rasm/claude-code/engine/interface/Engine') {}

// --- [COMPOSITION] ---------------------------------------------------------------------

function engine($: EngineInterface, turns: SubscriptionRef.SubscriptionRef<HashSet.HashSet<Loop>>, band: Option.Option<Band>) {
    const permits = Semaphore.makeUnsafe(1);
    const notification = (key: string, text: Option.Option<string>) =>
        Option.match(text, {
            onNone: () => Effect.asVoid(call('ui.cancelNotify', key, () => $.ui.cancelNotify(key))),
            onSome: (body) =>
                Effect.flatMap(
                    call('ui.notify', key, () => $.ui.notify(body, { key })),
                    (sent) => (sent.isSent ? Effect.void : Effect.log(`notification ${key} not sent: ${sent.reason}`)),
                ),
        });
    const show = ({ notify }: Band, group: string, shown: Readonly<Record<string, string>>) =>
        stored($).pipe(
            Effect.tap((held) => call('store.set', bandKey, () => $.store.set(bandKey, Record.isEmptyRecord(shown) ? Record.remove(held, group) : Record.set(held, group, shown)))),
            Effect.tap(Effect.sync(() => $.ui.invalidate('ui.render'))),
            Effect.map((held) => Record.keys(Record.difference(shown, held[group] ?? {}))),
            Effect.flatMap((changed) => Effect.forEach(notify ? changed : [], (key) => notification(key, Record.get(shown, key)), { discard: true })),
            Semaphore.withPermit(permits),
            Effect.uninterruptible,
        );
    const isRunning = (agent: Loop) => Effect.map(SubscriptionRef.get(turns), HashSet.has(agent));
    return Layer.effect(
        Engine,
        Effect.as(
            Effect.forEach(
                Option.toArray(band),
                (held) =>
                    stored($).pipe(
                        Effect.flatMap((rows) => Effect.forEach(Record.keys(rows), (group) => show(held, group, {}), { discard: true })),
                        Effect.catchCause(Effect.logWarning),
                    ),
                { discard: true },
            ),
            {
                deliver: (notice: Notice) =>
                    Notice.$match(notice, {
                        Toast: ({ text }) => Effect.sync(() => $.ui.toast(text)),
                        Notification: ({ key, text }) => notification(key, text),
                        Band: ({ group, rows }) =>
                            Option.match(band, {
                                onNone: () => PlatformError.badArgument({ module: 'Engine', method: 'deliver', description: `band rows of ${group}, as this mod draws no band` }),
                                onSome: (held) => show(held, group, rows),
                            }),
                        Message: ({ text, agent }) =>
                            Effect.flatMap(isRunning(agent), (busy) =>
                                Match.value({ busy, agentId: Option.getOrUndefined(agent) }).pipe(
                                    Match.when({ busy: true }, ({ agentId }) =>
                                        call(
                                            'session.append',
                                            agentId ?? 'main',
                                            () => $.session.append({ message: { type: 'user', content: [{ type: 'text', text }] }, ...(agentId === undefined ? {} : { agentId }) }),
                                            ({ deny }) => deny,
                                        ),
                                    ),
                                    Match.when({ agentId: Match.string }, ({ agentId }) =>
                                        call(
                                            'session.send',
                                            agentId,
                                            () => $.session.send({ to: { agentId }, text }),
                                            ({ reason }) => reason,
                                        ),
                                    ),
                                    Match.orElse(() =>
                                        call(
                                            'prompt.submit',
                                            'main',
                                            () => $.prompt.submit({ text }),
                                            ({ drop }) => drop,
                                        ),
                                    ),
                                    Effect.asVoid,
                                ),
                            ),
                    }),
                invalidate: (event: InvalidatableEventName) => Effect.sync(() => $.ui.invalidate(event)),
                fetch: (url: string, init?: HttpInit) => call('http.fetch', url, () => $.http.fetch(url, init)),
                callTool: (args: AgentCallArgs) => call('agent.call', args.tool, (signal) => $.agent.call(args, { signal })),
                compile: (source: string) => call('wasm.compile', source, () => $.wasm.compile(source)),
                cwd: call('session.cwd', 'main', () => $.session.cwd()),
                findAgent: (id: string) =>
                    Effect.map(
                        call('agent.list', id, () => $.agent.list()),
                        Array.findFirst((info) => info.id === id && !ended[info.status]),
                    ),
                isRunning,
                turns: SubscriptionRef.changes(turns),
            },
        ),
    );
}

function stored($: EngineInterface) {
    return call('store.get', bandKey, () => $.store.get(bandKey)).pipe(
        Effect.flatMap(Schema.decodeUnknownEffect(Rows)),
        Effect.map((rows) => rows ?? {}),
    );
}

// --- [EXPORTS] -------------------------------------------------------------------------

export { type Band, Engine, engine, Notice, stored };
