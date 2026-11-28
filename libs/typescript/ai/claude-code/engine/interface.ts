import type { AgentCallArgs, AgentInfo, AgentLimits, AgentStatus, EngineInterface, HttpInit, HttpResponse, InvalidatableEventName, SessionMessage, ToolCallResult, UiFocusArgs, UiScrollArgs } from 'claude-code';
import { Array, Context, Data, Effect, Formatter, HashSet, Layer, Match, Option, PlatformError, Record, Ref, Semaphore, type Stream, SubscriptionRef } from 'effect';
import { call } from '#/engine/services';

// --- [TYPES] ---------------------------------------------------------------------------

type Notice = Data.TaggedEnum<{
    Toast: { readonly text: string };
    Notification: { readonly key: string; readonly text: Option.Option<string> };
    Band: { readonly group: string; readonly rows: Record.ReadonlyRecord<string, string> };
    Message: { readonly text: string; readonly agent: Option.Option<string> };
    Note: { readonly text: string; readonly agent: Option.Option<string> };
}>;

// --- [CONSTANTS] -----------------------------------------------------------------------

const ENDED = { pending: false, running: false, waiting: false, idle: false, completed: true, failed: true, killed: true } as const satisfies Record<AgentStatus, boolean>;

// --- [MODELS] --------------------------------------------------------------------------

const Notice = Data.taggedEnum<Notice>();

// --- [SERVICES] ------------------------------------------------------------------------

class Engine extends Context.Service<
    Engine,
    {
        readonly deliver: (notice: Notice) => Effect.Effect<void, PlatformError.PlatformError>;
        readonly invalidate: (event: InvalidatableEventName) => Effect.Effect<void>;
        readonly compile: (bytes: Uint8Array) => Effect.Effect<WebAssembly.Module, PlatformError.PlatformError>;
        readonly focus: (args: UiFocusArgs) => Effect.Effect<void, PlatformError.PlatformError>;
        readonly scroll: (args: UiScrollArgs) => Effect.Effect<void, PlatformError.PlatformError>;
        readonly fetch: (url: string, init?: HttpInit) => Effect.Effect<HttpResponse, PlatformError.PlatformError>;
        readonly callTool: (args: AgentCallArgs) => Effect.Effect<ToolCallResult, PlatformError.PlatformError>;
        readonly cwd: Effect.Effect<string, PlatformError.PlatformError>;
        readonly root: Effect.Effect<string, PlatformError.PlatformError>;
        readonly agents: Effect.Effect<Record.ReadonlyRecord<string, AgentInfo>, PlatformError.PlatformError>;
        readonly limits: Effect.Effect<AgentLimits, PlatformError.PlatformError>;
        readonly messages: (agent: Option.Option<string>) => Effect.Effect<readonly SessionMessage[], PlatformError.PlatformError>;
        readonly isRunning: (agent: Option.Option<string>) => Effect.Effect<boolean>;
        readonly turns: Stream.Stream<HashSet.HashSet<Option.Option<string>>>;
    }
>()('@rasm/claude-code/engine/interface/Engine') {}

// --- [COMPOSITION] ---------------------------------------------------------------------

function engine($: EngineInterface, turns: SubscriptionRef.SubscriptionRef<HashSet.HashSet<Option.Option<string>>>, band: Option.Option<Ref.Ref<Record.ReadonlyRecord<string, Record.ReadonlyRecord<string, string>>>>) {
    const permits = Semaphore.makeUnsafe(1);
    const notification = (key: string, text: Option.Option<string>) =>
        Option.match(text, {
            onNone: () => Effect.asVoid(call('ui.cancelNotify', key, () => $.ui.cancelNotify(key))),
            onSome: (body) =>
                Effect.flatMap(
                    call('ui.notify', key, () => $.ui.notify(body, { key })),
                    (sent) => (sent.isSent ? Effect.void : Effect.log('notification not sent').pipe(Effect.annotateLogs({ key, reason: sent.reason }))),
                ),
        });
    const note = (text: string, agent: Option.Option<string>) =>
        Effect.asVoid(
            call(
                'session.append',
                Option.getOrElse(agent, () => 'main'),
                () => $.session.append({ message: { type: 'user', content: [{ type: 'text', text }] }, ...Option.match(agent, { onNone: () => ({}), onSome: (agentId) => ({ agentId }) }) }),
                ({ deny }) => deny,
            ),
        );
    const show = (group: string, shown: Record.ReadonlyRecord<string, string>) =>
        Effect.fromOption(band, () => PlatformError.badArgument({ module: 'Engine', method: 'deliver', description: `no band, group ${group}` })).pipe(
            Effect.flatMap((rows) => Ref.modify(rows, (held) => [Record.keys(Record.difference(shown, held[group] ?? {})), Record.isEmptyReadonlyRecord(shown) ? Record.remove(held, group) : Record.set(held, group, shown)] as const)),
            Effect.tap(Effect.sync(() => $.ui.invalidate('ui.render'))),
            Effect.flatMap((changed) => Effect.forEach(changed, (key) => notification(key, Record.get(shown, key)), { discard: true })),
            Semaphore.withPermit(permits),
            Effect.uninterruptible,
        );
    const isRunning = (agent: Option.Option<string>) => Effect.map(SubscriptionRef.get(turns), HashSet.has(agent));
    return Layer.effect(
        Engine,
        Effect.as(
            Effect.addFinalizer(() =>
                Effect.forEach(Option.toArray(band), Ref.get).pipe(
                    Effect.flatMap((held) => Effect.forEach(held.flatMap(Record.keys), (group) => show(group, {}), { discard: true })),
                    Effect.catchCause(Effect.logWarning),
                ),
            ),
            {
                deliver: (notice: Notice) =>
                    Notice.$match(notice, {
                        Toast: ({ text }) => Effect.sync(() => $.ui.toast(text)),
                        Notification: ({ key, text }) => notification(key, text),
                        Band: ({ group, rows }) => show(group, rows),
                        Note: ({ text, agent }) => note(text, agent),
                        Message: ({ text, agent }) =>
                            Effect.flatMap(isRunning(agent), (busy) =>
                                Match.value({ busy, agentId: Option.getOrUndefined(agent) }).pipe(
                                    Match.when({ busy: true }, () => note(text, agent)),
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
                compile: (bytes: Uint8Array) => call('wasm.compile', `${bytes.byteLength} bytes`, () => $.wasm.compile(bytes)),
                focus: (args: UiFocusArgs) =>
                    Effect.asVoid(
                        call(
                            'ui.focus',
                            args.key,
                            () => $.ui.focus(args),
                            ({ deny }) => deny,
                        ),
                    ),
                scroll: (args: UiScrollArgs) =>
                    Effect.asVoid(
                        call(
                            'ui.scroll',
                            args.in ?? Formatter.format(args.to),
                            () => $.ui.scroll(args),
                            ({ deny }) => deny,
                        ),
                    ),
                fetch: (url: string, init?: HttpInit) => call('http.fetch', url, () => $.http.fetch(url, init)),
                callTool: (args: AgentCallArgs) => call('agent.call', args.tool, (signal) => $.agent.call(args, { signal })),
                cwd: call('session.cwd', 'main', () => $.session.cwd()),
                root: call('session.root', 'main', () => $.session.root()),
                agents: Effect.map(
                    call('agent.list', 'session', () => $.agent.list()),
                    (listed) =>
                        Record.fromIterableWith(
                            Array.filter(listed, (info) => !ENDED[info.status]),
                            (info) => [info.id, info],
                        ),
                ),
                limits: call('agent.limits', 'session', () => $.agent.limits()),
                messages: (agent: Option.Option<string>) =>
                    call(
                        'session.messages',
                        Option.getOrElse(agent, () => 'main'),
                        () => Option.match(agent, { onNone: () => $.session.messages(), onSome: (agentId) => $.session.messages({ agentId }) }),
                        (read) => (globalThis.Array.isArray(read) ? undefined : read.deny),
                    ).pipe(Effect.filterOrElse((read): read is SessionMessage[] => globalThis.Array.isArray(read), Effect.die)),
                isRunning,
                turns: SubscriptionRef.changes(turns),
            },
        ),
    );
}

// --- [EXPORTS] -------------------------------------------------------------------------

export { Engine, engine, Notice };
