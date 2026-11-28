import { Engine } from '@rasm/claude-code/engine';
import { Peer } from '@rasm/claude-code/peers';
import { Pane, Watch } from '@rasm/claude-code/watch';
import { ExitError, run, spawn } from '@rasm/process';
import { Array, Config, Context, Data, DateTime, Duration, Effect, FiberHandle, Layer, Number, Option, Order, Path, Record, Schedule, Schema, Stream, String, SubscriptionRef } from 'effect';
import { type ChildProcessSpawner, CommandLine } from 'effect/process';

// --- [TYPES] ---------------------------------------------------------------------------

interface Sources {
    readonly waits: readonly Pane[];
    readonly runs: readonly Pane[];
    readonly peers: readonly Peer[];
    readonly threads: readonly (typeof Thread.Type)[];
}
type Gathered = ReturnType<typeof derive>;

// --- [CONSTANTS] -----------------------------------------------------------------------

const WATCH = 'agents';
const WINDOW = Duration.hours(24);
const SUBSTANTIAL = 10;
const SHOWN = 8;
const TICK = Duration.seconds(15);
const RANKS = { waiting: 0, busy: 1, idle: 2 } as const;
const UNITS = ['days', 'hours', 'minutes', 'seconds'] as const;
const NARROW = new Intl.DurationFormat(undefined, { style: 'narrow' });
const THREADS = ['atuin', 'threads', 'list', '--style', 'ndjson'] as const;

// --- [MODELS] --------------------------------------------------------------------------

const Thread = Schema.Struct({
    harness: Schema.String,
    session_id: Schema.String,
    atuin_id: Schema.String,
    cwd: Schema.String,
    parent_kind: Schema.optionalKey(Schema.String),
    updated_at: Schema.DateTimeUtcFromString,
    message_count: Schema.Int,
    title: Schema.optionalKey(Schema.String),
    preview: Schema.optionalKey(Schema.String),
    last_reply: Schema.optionalKey(Schema.String),
});
const Line = Schema.fromJsonString(Thread);

class Entry extends Data.Class<{
    readonly key: string;
    readonly label: string;
    readonly detail: string;
    readonly opening: Option.Option<readonly string[]>;
}> {}

// --- [SERVICES] ------------------------------------------------------------------------

class Gathering extends Context.Service<
    Gathering,
    {
        readonly gathered: Stream.Stream<Gathered>;
        readonly follow: Effect.Effect<void>;
        readonly unfollow: Effect.Effect<void>;
        readonly open: (opening: readonly string[]) => Effect.Effect<void>;
    }
>()('@rasm/naqib/hooks/gathering') {
    static readonly layer = Layer.effect(
        Gathering,
        Effect.gen(function* () {
            const [path, engine, watch, context, session, { stdout: printed }] = yield* Effect.all([Path.Path, Engine, Watch, Effect.context<Engine | ChildProcessSpawner.ChildProcessSpawner | Path.Path>(), Config.String('CLAUDE_CODE_SESSION_ID'), run(['env', '-0'])]);
            const variables = Record.fromIterableWith(printed.matchAll(/(?<name>[^\0=]+)=(?<value>[^\0]*)/gu), ({ groups }) => [groups?.['name'] ?? '', groups?.['value'] ?? '']);
            const self = Option.contains(Record.get(variables, 'CLAUDE_CODE_SESSION_KIND'), 'bg') ? Option.none() : Option.flatMap(Record.get(variables, 'ZELLIJ_PANE_ID'), Number.parse);
            const root = yield* Option.match(yield* engine.repo, { onNone: () => engine.root, onSome: (repo) => Effect.succeed(repo.root) });
            const fetch = {
                waits: Option.match(self, {
                    onNone: () => Effect.succeed(Array.empty<Pane>()),
                    onSome: (own) => read('zellij panes', Pane.list(Option.none()).pipe(Effect.map(Option.getOrElse(() => Array.empty<Pane>())), Effect.map(Array.filter((pane) => pane.id !== own && pane.attention.status === 'NeedsYou' && pane.attention.muted === null)))),
                }),
                runs: read(`zellij session ${WATCH}`, watch.views),
                peers: read('claude agents', Peer.list),
                threads: read(CommandLine.display(THREADS), threads(root, session)),
            };
            const sources = yield* SubscriptionRef.make<Sources>({ waits: [], runs: [], peers: [], threads: [] });
            const all = SubscriptionRef.updateEffect(sources, () => Effect.all(fetch, { concurrency: 'unbounded' })).pipe(Effect.provideContext(context));
            const waited = SubscriptionRef.updateEffect(sources, (held) => Effect.map(fetch.waits, (waits) => ({ ...held, waits }))).pipe(Effect.provideContext(context));
            const following = yield* FiberHandle.make();
            yield* all.pipe(Effect.andThen(Effect.schedule(waited, Schedule.spaced(TICK))), Effect.forkScoped);
            return {
                gathered: sources.pipe(
                    SubscriptionRef.changes,
                    Stream.mapEffect((held) => Effect.map(DateTime.now, (now) => derive(held, now, self, path))),
                ),
                follow: Effect.asVoid(FiberHandle.run(following, Effect.repeat(all, Schedule.spaced(TICK)), { onlyIfMissing: true })),
                unfollow: FiberHandle.clear(following),
                open: (opening) =>
                    run(['zellij', ...opening]).pipe(
                        Effect.catchCause((cause) => Effect.logWarning('open failed', cause)),
                        Effect.andThen(waited),
                        Effect.provideContext(context),
                    ),
            };
        }),
    ).pipe(Layer.provideMerge(Watch.layer(WATCH)));
}

// --- [OPERATIONS] ----------------------------------------------------------------------

const firstLine = (text: string) => String.trim(text.split('\n', 1)[0] ?? '');
const elapsed = (span: Duration.Duration) => {
    const parts = Duration.parts(span);
    const first = Math.max(
        0,
        UNITS.findIndex((unit) => parts[unit] > 0),
    );
    return NARROW.format(Object.fromEntries(UNITS.slice(first, first + 2).map((unit) => [unit, parts[unit]])));
};
const read = <A, E, R>(source: string, reading: Effect.Effect<readonly A[], E, R>) => Effect.catchCause(reading, (cause) => Effect.as(Effect.logWarning('source unread', cause).pipe(Effect.annotateLogs({ source })), Array.empty<A>()));
const threads = Effect.fnUntraced(function* (root: string, session: string) {
    const [path, now] = yield* Effect.all([Path.Path, DateTime.now]);
    const handle = yield* spawn(THREADS, { stdin: 'ignore' });
    const ended = Effect.all([handle.exitCode, handle.stderr.pipe(Stream.decodeText, Stream.mkString)], { concurrency: 'unbounded' }).pipe(Effect.flatMap(([code, stderr]) => (code === 0 ? Effect.void : ExitError.of({ command: CommandLine.display(THREADS), code, stdout: '', stderr }))));
    return yield* handle.stdout.pipe(
        Stream.decodeText,
        Stream.splitLines,
        Stream.concat(Stream.fromEffectDrain(ended)),
        Stream.filterMap((line) => Schema.decodeResult(Line)(line)),
        Stream.takeWhile((thread) => Duration.isLessThanOrEqualTo(DateTime.distance(thread.updated_at, now), WINDOW)),
        Stream.filter((thread) => thread.parent_kind === undefined && thread.session_id !== session && Option.isSome(path.resolveWithin(root, path.relative(root, thread.cwd)))),
        Stream.runCollect,
    );
}, Effect.scoped);
const derive = (held: Sources, now: DateTime.Utc, self: Option.Option<number>, path: Path.Path) => {
    const live = new Set(Array.getSomes(held.peers.map(({ sessionId }) => sessionId)));
    const told = new Map(held.threads.map((thread) => [thread.session_id, thread] as const));
    const ago = (at: DateTime.Utc) => elapsed(DateTime.distance(at, now));
    const floating = (title: string, command: readonly string[]) => Option.as(self, ['run', '--floating', '--close-on-exit', '--name', title, '--', ...command]);
    return {
        waits: Array.sort(
            held.waits,
            Order.mapInput(Order.Number, (waiting: Pane) => waiting.attention.since_ms),
        ).map((waiting) => new Entry({ key: `pane-${waiting.id}`, label: waiting.title, detail: [waiting.tab_name, waiting.attention.text === '' ? (waiting.attention.source ?? 'needs you') : waiting.attention.text].join(' · '), opening: Option.as(self, ['action', 'focus-pane-id', `terminal_${waiting.id}`]) })),
        sessions: Array.sort(
            held.peers,
            Order.combine(
                Order.mapInput(Order.Number, (peer: Peer) => Option.match(peer.status, { onNone: () => Record.size(RANKS), onSome: (status) => RANKS[status] })),
                Order.mapInput(Order.flip(DateTime.Order), (peer: Peer) => peer.startedAt),
            ),
        ).map((peer) => {
            const title = Option.getOrElse(
                Option.orElse(peer.name, () => Option.map(peer.sessionId, String.takeLeft(8))),
                () => peer.kind,
            );
            const state = Option.getOrElse(peer.status, () => peer.kind);
            const reply = peer.sessionId.pipe(
                Option.flatMapNullishOr((id) => told.get(id)?.last_reply),
                Option.map(firstLine),
            );
            return new Entry({
                key: Option.getOrElse(peer.sessionId, () => title),
                label: title,
                detail: [Option.match(peer.waitingFor, { onNone: () => state, onSome: (wanted) => `${state}: ${wanted}` }), ago(peer.startedAt), ...Option.toArray(reply)].join(' · '),
                opening: Option.flatMap(
                    Option.filter(peer.sessionId, () => peer.kind === 'background'),
                    (session) => floating(title, ['claude', 'attach', session]),
                ),
            });
        }),
        runs: held.runs.map((view) => new Entry({ key: `run-${view.id}`, label: view.title, detail: `${WATCH} · ${view.attention.status === 'Idle' ? 'quiet' : 'drawing'}`, opening: Option.flatMap(Option.fromNullOr(view.terminal_command), (command) => floating(view.title, ['sh', '-c', command])) })),
        threads: held.threads
            .filter((thread) => thread.message_count >= SUBSTANTIAL && !live.has(thread.session_id))
            .slice(0, SHOWN)
            .map((thread) => {
                const title = thread.title ?? Option.match(Option.fromUndefinedOr(thread.preview), { onNone: () => String.takeLeft(thread.session_id, 8), onSome: firstLine });
                return new Entry({
                    key: thread.atuin_id,
                    label: `${thread.harness} · ${path.basename(thread.cwd)}`,
                    detail: [ago(thread.updated_at), `${thread.message_count} messages`, title].join(' · '),
                    opening: floating(title, ['atuin', 'threads', 'resume', thread.atuin_id]),
                });
            }),
    };
};

// --- [EXPORTS] -------------------------------------------------------------------------

export { type Entry, type Gathered, Gathering };
