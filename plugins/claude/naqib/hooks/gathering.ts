import { Engine } from '@rasm/claude-code/engine';
import { Peer } from '@rasm/claude-code/peers';
import { Pane, Watch } from '@rasm/claude-code/watch';
import { ExitError, run, spawn } from '@rasm/process';
import { Array, Context, Data, DateTime, Duration, Effect, FiberHandle, Layer, Number, Option, Order, Path, Queue, Record, Ref, Schedule, Schema, Stream, String, SubscriptionRef } from 'effect';
import { ChildProcessSpawner } from 'effect/process';

// --- [TYPES] ---------------------------------------------------------------------------

type Opening = Data.TaggedEnum<{
    Focus: { readonly pane: number };
    Watch: { readonly title: string; readonly command: string };
    Resume: { readonly thread: string; readonly cwd: string; readonly title: string };
    Attach: { readonly session: string; readonly title: string };
}>;
type Stance = (typeof STANCES)[number];
type Reach = 'waits' | 'all';
interface Read {
    readonly waits: readonly Pane[];
    readonly runs: readonly Pane[];
    readonly peers: readonly Peer[];
    readonly threads: readonly (typeof Thread.Type)[];
}

// --- [CONSTANTS] -----------------------------------------------------------------------

const STANCES = ['waits', 'sessions', 'runs', 'threads'] as const;
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

const Opening = Data.taggedEnum<Opening>();
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
    readonly stance: Stance;
    readonly label: string;
    readonly detail: string;
    readonly opening: Option.Option<Opening>;
}> {}
class Sources extends Data.Class<Read> {}

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
const zellij = (args: readonly string[]) => run(['zellij', ...args]);
const read = <A, E, R>(source: string, reading: Effect.Effect<readonly A[], E, R>) => Effect.catchCause(reading, (cause) => Effect.as(Effect.logWarning('source unread', cause).pipe(Effect.annotateLogs({ source })), Array.empty<A>()));
const threads = Effect.fnUntraced(function* (within: (cwd: string) => boolean) {
    const now = yield* DateTime.now;
    const handle = yield* spawn(THREADS, { stdin: 'ignore' });
    const ended = Effect.all([handle.exitCode, handle.stderr.pipe(Stream.decodeText, Stream.mkString)], { concurrency: 'unbounded' }).pipe(Effect.flatMap(([code, stderr]) => (code === 0 ? Effect.void : ExitError.of({ command: THREADS.join(' '), code, stdout: '', stderr }))));
    return yield* handle.stdout.pipe(
        Stream.decodeText,
        Stream.splitLines,
        Stream.concat(Stream.fromEffectDrain(ended)),
        Stream.filterMap((line) => Schema.decodeResult(Line)(line)),
        Stream.takeWhile((thread) => Duration.isLessThanOrEqualTo(DateTime.distance(thread.updated_at, now), WINDOW)),
        Stream.filter((thread) => thread.parent_kind === undefined && within(thread.cwd)),
        Stream.runCollect,
    );
}, Effect.scoped);
const derive = (held: Sources, now: DateTime.Utc, self: Option.Option<number>, path: Path.Path): readonly Entry[] => {
    const opened = (opening: Opening) => (Option.isSome(self) ? Option.some(opening) : Option.none());
    const live = new Set(Array.getSomes(held.peers.map(({ sessionId }) => sessionId)));
    const told = new Map(held.threads.map((thread) => [thread.session_id, thread] as const));
    const waits = Array.sort(
        held.waits,
        Order.mapInput(Order.Number, (waiting: Pane) => waiting.attention.since_ms),
    ).map((waiting) => new Entry({ key: `pane-${waiting.id}`, stance: 'waits', label: waiting.title, detail: [waiting.tab_name, waiting.attention.text === '' ? (waiting.attention.source ?? 'needs you') : waiting.attention.text].join(' · '), opening: opened(Opening.Focus({ pane: waiting.id })) }));
    const peers = Array.sort(
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
        const status = Option.match(Option.all({ status: peer.status, wanted: peer.waitingFor }), { onNone: () => Option.getOrElse(peer.status, () => peer.kind), onSome: ({ status: state, wanted }) => `${state}: ${wanted}` });
        const reply = Option.flatMapNullishOr(
            Option.flatMapNullishOr(peer.sessionId, (id) => told.get(id)),
            (thread) => thread.last_reply,
        );
        return new Entry({
            key: Option.getOrElse(peer.sessionId, () => title),
            stance: 'sessions',
            label: title,
            detail: [status, elapsed(DateTime.distance(peer.startedAt, now)), ...Option.toArray(Option.map(reply, firstLine))].join(' · '),
            opening: Option.flatMap(
                Option.filter(peer.sessionId, () => peer.kind === 'background'),
                (session) => opened(Opening.Attach({ session, title })),
            ),
        });
    });
    const runs = held.runs.map(
        (view) => new Entry({ key: `run-${view.id}`, stance: 'runs', label: view.title, detail: `${WATCH} · ${view.attention.status === 'Idle' ? 'quiet' : 'drawing'}`, opening: Option.flatMap(Option.fromNullOr(view.terminal_command), (command) => opened(Opening.Watch({ title: view.title, command }))) }),
    );
    const ended = held.threads
        .filter((thread) => thread.message_count >= SUBSTANTIAL && !live.has(thread.session_id))
        .slice(0, SHOWN)
        .map((thread) => {
            const title = thread.title ?? Option.getOrElse(Option.map(Option.fromUndefinedOr(thread.preview), firstLine), () => thread.session_id.slice(0, 8));
            return new Entry({
                key: thread.atuin_id,
                stance: 'threads',
                label: `${thread.harness} · ${path.basename(thread.cwd)}`,
                detail: [elapsed(DateTime.distance(thread.updated_at, now)), `${thread.message_count} messages`, title].join(' · '),
                opening: opened(Opening.Resume({ thread: thread.atuin_id, cwd: thread.cwd, title })),
            });
        });
    return [...waits, ...peers, ...runs, ...ended];
};

// --- [SERVICES] ------------------------------------------------------------------------

class Gathering extends Context.Service<
    Gathering,
    {
        readonly entries: SubscriptionRef.SubscriptionRef<readonly Entry[]>;
        readonly refresh: (reach: Reach) => Effect.Effect<void>;
        readonly follow: Effect.Effect<void>;
        readonly unfollow: Effect.Effect<void>;
        readonly open: (opening: Opening) => Effect.Effect<void>;
    }
>()('@rasm/naqib/hooks/gathering') {
    static readonly layer = Layer.effect(
        Gathering,
        Effect.gen(function* () {
            const [path, engine, watch, processes, { stdout: printed }] = yield* Effect.all([Path.Path, Engine, Watch, ChildProcessSpawner.ChildProcessSpawner, run(['env', '-0'])]);
            const variables = Record.fromIterableWith(printed.matchAll(/(?<name>[^\0=]+)=(?<value>[^\0]*)/gu), ({ groups }) => [groups?.['name'] ?? '', groups?.['value'] ?? '']);
            const self = Option.contains(Record.get(variables, 'CLAUDE_CODE_SESSION_KIND'), 'bg') ? Option.none() : Option.flatMap(Record.get(variables, 'ZELLIJ_PANE_ID'), Number.parse);
            const root = yield* Option.match(yield* engine.repo, { onNone: () => engine.root, onSome: (repo) => Effect.succeed(repo.root) });
            const fetch = {
                waits: Option.match(self, {
                    onNone: () => Effect.succeed(Array.empty<Pane>()),
                    onSome: (own) => read('zellij panes', Pane.list(Option.none()).pipe(Effect.map(Option.getOrElse(() => Array.empty<Pane>())), Effect.map(Array.filter((pane) => !pane.is_plugin && !pane.exited && pane.id !== own && pane.attention.status === 'NeedsYou' && pane.attention.muted === null)))),
                }),
                runs: read(`zellij session ${WATCH}`, watch.views),
                peers: read('claude agents', Peer.list),
                threads: read(
                    THREADS.join(' '),
                    threads((cwd) => Option.isSome(path.resolveWithin(root, path.relative(root, cwd)))),
                ),
            };
            const sources = yield* Ref.make(new Sources({ waits: [], runs: [], peers: [], threads: [] }));
            const entries = yield* SubscriptionRef.make<readonly Entry[]>([]);
            const pokes = yield* Queue.unbounded<Reach>();
            const following = yield* FiberHandle.make();
            const refresh = Effect.fnUntraced(function* (reach: Reach) {
                const fetched = reach === 'waits' ? { waits: yield* fetch.waits } : yield* Effect.all(fetch, { concurrency: 'unbounded' });
                const held = yield* Ref.updateAndGet(sources, (known) => new Sources({ ...known, ...fetched }));
                yield* SubscriptionRef.set(entries, derive(held, yield* DateTime.now, self, path));
            });
            yield* Queue.takeAll(pokes).pipe(
                Effect.flatMap((reaches) => refresh(Array.contains(reaches, 'all') ? 'all' : 'waits')),
                Effect.forever,
                Effect.forkScoped,
            );
            yield* Queue.offer(pokes, 'all');
            return {
                entries,
                refresh: (reach) => Effect.asVoid(Queue.offer(pokes, reach)),
                follow: Effect.asVoid(FiberHandle.run(following, Effect.repeat(Queue.offer(pokes, 'all'), Schedule.spaced(TICK)), { onlyIfMissing: true })),
                unfollow: FiberHandle.clear(following),
                open: (opening) =>
                    Opening.$match(opening, {
                        Focus: ({ pane }) => zellij(['action', 'focus-pane-id', `terminal_${pane}`]),
                        Watch: ({ title, command }) => zellij(['run', '--floating', '--close-on-exit', '--name', title, '--', 'sh', '-c', command]),
                        Resume: ({ thread, cwd, title }) => zellij(['run', '--floating', '--close-on-exit', '--cwd', cwd, '--name', title, '--', 'atuin', 'threads', 'resume', thread]),
                        Attach: ({ session, title }) => zellij(['run', '--floating', '--close-on-exit', '--name', title, '--', 'claude', 'attach', session]),
                    }).pipe(
                        Effect.catchCause((cause) => Effect.logWarning('open failed', cause).pipe(Effect.annotateLogs({ opening: opening._tag }))),
                        Effect.andThen(Queue.offer(pokes, 'waits')),
                        Effect.provideService(ChildProcessSpawner.ChildProcessSpawner, processes),
                    ),
            };
        }),
    ).pipe(Layer.provideMerge(Watch.layer(WATCH)));
}

// --- [EXPORTS] -------------------------------------------------------------------------

export { type Entry, Gathering, STANCES };
