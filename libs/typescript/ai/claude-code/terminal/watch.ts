import { run } from '@rasm/process';
import { Array, Context, Effect, FileSystem, HashSet, Layer, Option, Path, Schema, String } from 'effect';
import { ChildProcessSpawner } from 'effect/process';

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
        const { stdout } = yield* run(['zellij', ...Option.match(session, { onNone: () => [], onSome: (named) => ['--session', named] }), 'action', 'list-panes', '--json']);
        const listing = yield* Effect.transposeOption(Option.map(Schema.decodeOption(Json)(stdout), Schema.decodeUnknownEffect(Listing)));
        return Option.map(listing, (panes) => panes.filter((pane) => !pane.is_plugin && !pane.exited));
    });
}
const Json = Schema.fromJsonString(Schema.Unknown);
const Listing = Schema.Array(Pane);

// --- [SERVICES] ------------------------------------------------------------------------

class Watch extends Context.Service<
    Watch,
    {
        readonly views: Effect.Effect<readonly Pane[], Effect.Error<ReturnType<typeof Pane.list>>>;
        readonly run: <A, E, R>(call: Effect.Effect<A, E, R>) => Effect.Effect<A, E, R>;
    }
>()('@rasm/claude-code/terminal/watch') {
    static readonly layer = (session: string) =>
        Layer.effect(
            Watch,
            Effect.gen(function* () {
                const [fs, path, processes] = yield* Effect.all([FileSystem.FileSystem, Path.Path, ChildProcessSpawner.ChildProcessSpawner]);
                const listed = Effect.provideService(Pane.list(Option.some(session)), ChildProcessSpawner.ChildProcessSpawner, processes);
                const folder = yield* Effect.cached(
                    Effect.gen(function* () {
                        const [uid, tmp] = yield* Effect.all([run(['id', '-u']), run(['/bin/sh', '-c', 'printf %s "${TMUX_TMPDIR:-/tmp}"'])], { concurrency: 'unbounded' });
                        return path.join(String.trim(tmp.stdout), `tmux-${String.trim(uid.stdout)}`);
                    }).pipe(Effect.provideService(ChildProcessSpawner.ChildProcessSpawner, processes)),
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
                            run(['zellij', 'list-sessions', '--short']).pipe(
                                Effect.flatMap(({ stdout }) => Effect.when(run(['zellij', 'delete-session', '--force', session]), Effect.succeed(Array.contains(String.linesIterator(stdout), session)))),
                                Effect.andThen(run(['zellij', 'attach', '--create-background', session])),
                                Effect.andThen(run(['zellij', '--session', session, 'action', 'list-tabs'])),
                                Effect.andThen(run(['zellij', '--session', session, 'action', 'mute-session'])),
                            ),
                    }),
                );
                const open = Effect.fnUntraced(
                    function* (socket: string) {
                        const [{ stdout: listing }, { stdout: viewers }] = yield* Effect.all([run(['tmux', '-L', socket, 'list-sessions', '-F', '#{session_name}']), run(['tmux', '-L', socket, 'list-clients', '-f', '#{client_readonly}', '-F', '#{client_name}'])], { concurrency: 'unbounded' });
                        const names = Array.fromIterable(String.linesIterator(listing));
                        yield* Effect.when(
                            created.pipe(
                                Effect.andThen(Effect.forEach(names, (name) => run(['zellij', '--session', session, 'run', '--close-on-exit', '--name', `${socket} ${name}`, '--', 'tmux', '-L', socket, 'attach', '-f', 'read-only,ignore-size', '-t', name]), { discard: true })),
                                Effect.andThen(Effect.log('watch opened').pipe(Effect.annotateLogs('names', names.join(' ')))),
                            ),
                            Effect.succeed(Array.isReadonlyArrayNonEmpty(names) && String.isEmpty(viewers)),
                        );
                    },
                    (opening) => Effect.catchCause(opening, (cause) => Effect.logWarning('watch failed', cause)),
                    (opening, socket) => Effect.annotateLogs(opening, { socket, session }),
                    Effect.provideService(ChildProcessSpawner.ChildProcessSpawner, processes),
                );
                return {
                    views: Effect.map(
                        listed,
                        Option.match({
                            onNone: () => [],
                            onSome: Array.filter((pane) => pane.terminal_command !== null),
                        }),
                    ),
                    run: Effect.fnUntraced(function* <A, E, R>(call: Effect.Effect<A, E, R>) {
                        const before = yield* servers;
                        const result = yield* call;
                        yield* Effect.forEach(HashSet.difference(yield* servers, before), open, { discard: true });
                        return result;
                    }),
                };
            }),
        );
}

// --- [EXPORTS] -------------------------------------------------------------------------

export { Pane, Watch };
