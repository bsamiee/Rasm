import { run } from '@rasm/process';
import { Array, Config, Context, Effect, FileSystem, HashSet, Layer, Option, Path, Schema, String } from 'effect';
import { ChildProcessSpawner } from 'effect/process';

// --- [CONSTANTS] -----------------------------------------------------------------------

const OUTSIDE = { ZELLIJ: undefined, ZELLIJ_SESSION_NAME: undefined, ZELLIJ_PANE_ID: undefined } as const;

// --- [MODELS] --------------------------------------------------------------------------

class Pane extends Schema.Class<Pane, { readonly brand: unique symbol }>('@rasm/claude-code/terminal/watch/Pane')({
    id: Schema.Int,
    is_plugin: Schema.Boolean,
    exited: Schema.Boolean,
    title: Schema.String,
    tab_name: Schema.String,
    terminal_command: Schema.NullOr(Schema.String),
    attention: Schema.Struct({ status: Schema.String, source: Schema.NullOr(Schema.String), text: Schema.String, since_ms: Schema.Int, muted: Schema.NullOr(Schema.String) }),
}) {
    static readonly list = Effect.fnUntraced(function* (session: Option.Option<string>) {
        const { stdout } = yield* Option.match(session, {
            onNone: () => run(['zellij', 'action', 'list-panes', '--json']),
            onSome: (named) => zellij(['--session', named, 'action', 'list-panes', '--json']),
        });
        return yield* Effect.transposeOption(Option.map(Schema.decodeOption(Json)(stdout), Schema.decodeUnknownEffect(Listing)));
    });
}
const Json = Schema.fromJsonString(Schema.Unknown);
const Listing = Schema.Array(Pane);
const Opened = Schema.TemplateLiteral(['terminal_', Schema.Int]);

// --- [SERVICES] ------------------------------------------------------------------------

class Watch extends Context.Service<
    Watch,
    {
        readonly views: Effect.Effect<readonly Pane[], Effect.Error<ReturnType<typeof Pane.list>>>;
        readonly during: <A, E, R>(call: Effect.Effect<A, E, R>) => Effect.Effect<A, E, R>;
    }
>()('@rasm/claude-code/terminal/watch') {
    static readonly layer = (session: string) =>
        Layer.effect(
            Watch,
            Effect.gen(function* () {
                const [fs, path, processes] = yield* Effect.all([FileSystem.FileSystem, Path.Path, ChildProcessSpawner.ChildProcessSpawner]);
                const listed = Pane.list(Option.some(session));
                const folder = yield* Effect.cached(
                    run(['id', '-u']).pipe(
                        Effect.flatMap(({ stdout }) => Schema.decodeEffect(Schema.FiniteFromString)(String.trim(stdout))),
                        Effect.flatMap((uid) => Effect.map(Config.String('TMUX_TMPDIR').pipe(Config.withDefault('/tmp')), (tmp) => path.join(tmp, `tmux-${uid}`))),
                        Effect.provideService(ChildProcessSpawner.ChildProcessSpawner, processes),
                    ),
                );
                const servers = folder.pipe(
                    Effect.flatMap(fs.readDirectory),
                    Effect.catchReason('PlatformError', 'NotFound', () => Effect.succeed(Array.empty<string>())),
                    Effect.map(HashSet.fromIterable),
                    Effect.catchCause((cause) => Effect.as(Effect.logWarning('watch servers unread', cause), HashSet.empty<string>())),
                );
                const created = Effect.flatMap(
                    listed,
                    Option.match({
                        onSome: () => Effect.void,
                        onNone: () =>
                            zellij(['list-sessions', '--short']).pipe(
                                Effect.flatMap(({ stdout }) => Effect.when(zellij(['delete-session', '--force', session]), Effect.succeed(Array.contains(Array.fromIterable(String.linesIterator(stdout)), session)))),
                                Effect.andThen(zellij(['attach', '--create-background', session])),
                            ),
                    }),
                );
                const view = (socket: string, name: string) =>
                    zellij(['--session', session, 'run', '--close-on-exit', '--name', `${socket} ${name}`, '--', 'tmux', '-L', socket, 'attach', '-f', 'read-only,ignore-size', '-t', name]).pipe(
                        Effect.flatMap(({ stdout }) => Schema.decodeUnknownEffect(Opened)(String.trim(stdout))),
                        Effect.tap((pane) => zellij(['--session', session, 'action', 'mute-pane', '--pane-id', pane])),
                    );
                const open = (socket: string) =>
                    Effect.all({ names: tmux(socket, ['list-sessions', '-F', '#{session_name}']), viewers: tmux(socket, ['list-clients', '-f', '#{client_readonly}', '-F', '#{client_name}']) }, { concurrency: 'unbounded' }).pipe(
                        Effect.flatMap(({ names, viewers }) =>
                            Array.isReadonlyArrayNonEmpty(viewers)
                                ? Effect.succeed(Array.empty<typeof Opened.Type>())
                                : Effect.andThen(
                                      Effect.when(created, Effect.succeed(Array.isReadonlyArrayNonEmpty(names))),
                                      Effect.forEach(names, (name) => view(socket, name)),
                                  ),
                        ),
                        Effect.tap((panes) => Effect.when(Effect.log('watch opened').pipe(Effect.annotateLogs({ socket, session, panes: panes.join(' ') })), Effect.succeed(Array.isReadonlyArrayNonEmpty(panes)))),
                        Effect.catchCause((cause) => Effect.logWarning('watch failed', cause).pipe(Effect.annotateLogs({ socket, session }))),
                        Effect.provideService(ChildProcessSpawner.ChildProcessSpawner, processes),
                    );
                return {
                    views: listed.pipe(
                        Effect.map(
                            Option.match({
                                onNone: () => [],
                                onSome: Array.filter((pane) => !pane.is_plugin && !pane.exited && pane.terminal_command !== null),
                            }),
                        ),
                        Effect.provideService(ChildProcessSpawner.ChildProcessSpawner, processes),
                    ),
                    during: Effect.fnUntraced(function* <A, E, R>(call: Effect.Effect<A, E, R>) {
                        const before = yield* servers;
                        const result = yield* call;
                        yield* Effect.forEach(HashSet.difference(yield* servers, before), open, { discard: true });
                        return result;
                    }),
                };
            }),
        );
}

// --- [OPERATIONS] ----------------------------------------------------------------------

const zellij = (args: readonly string[]) => run(['zellij', ...args], { env: OUTSIDE });
const tmux = (socket: string, args: readonly string[]) => Effect.map(run(['tmux', '-L', socket, ...args]), ({ stdout }) => Array.fromIterable(String.linesIterator(stdout)));

// --- [EXPORTS] -------------------------------------------------------------------------

export { Pane, Watch };
