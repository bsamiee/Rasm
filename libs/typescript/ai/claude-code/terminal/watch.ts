import { Array, Config, Context, Effect, FileSystem, HashSet, Layer, Option, Path, String } from 'effect';
import { ChildProcess, ChildProcessSpawner } from 'effect/process';

// --- [CONSTANTS] -----------------------------------------------------------------------

const OUTSIDE = { ZELLIJ: undefined, ZELLIJ_SESSION_NAME: undefined, ZELLIJ_PANE_ID: undefined } as const;

// --- [SERVICES] ------------------------------------------------------------------------

class Watch extends Context.Service<
    Watch,
    {
        readonly during: <A, E, R>(call: Effect.Effect<A, E, R>) => Effect.Effect<A, E, R>;
    }
>()('@rasm/claude-code/terminal/watch') {
    static readonly layer = (pool: string) =>
        Layer.effect(
            Watch,
            Effect.gen(function* () {
                const [fs, path, processes] = yield* Effect.all([FileSystem.FileSystem, Path.Path, ChildProcessSpawner.ChildProcessSpawner]);
                const uid = yield* processes.string(ChildProcess.make('id', ['-u'])).pipe(Effect.map(String.trim));
                const sockets = path.join(yield* Config.String('TMUX_TMPDIR').pipe(Config.withDefault('/tmp')), `tmux-${uid}`);
                const tmux = (socket: string, args: readonly string[]) => processes.lines(ChildProcess.make('tmux', ['-L', socket, ...args]));
                const create = processes.exitCode(zellij(['attach', '--create-background', pool]));
                const pooled = processes.lines(zellij(['list-sessions', '--no-formatting'])).pipe(
                    Effect.map(Array.findFirst(String.startsWith(`${pool} [`))),
                    Effect.flatMap(
                        Option.match({
                            onNone: () => create,
                            onSome: (row) => (row.includes('EXITED') ? Effect.andThen(processes.exitCode(zellij(['delete-session', '--force', pool])), create) : Effect.void),
                        }),
                    ),
                );
                const view = (socket: string, session: string) =>
                    processes.lines(zellij(['--session', pool, 'run', '--close-on-exit', '--name', `${socket} ${session}`, '--', 'tmux', '-L', socket, 'attach', '-f', 'read-only,ignore-size', '-t', session])).pipe(
                        Effect.map(Array.filter(String.isNonEmpty)),
                        Effect.tap((panes) => Effect.forEach(panes, (pane) => processes.exitCode(zellij(['--session', pool, 'action', 'mute-pane', '--pane-id', pane])), { discard: true })),
                    );
                const servers = fs.readDirectory(sockets).pipe(
                    Effect.map(HashSet.fromIterable),
                    Effect.catchCause(() => Effect.succeed(HashSet.empty<string>())),
                );
                const open = (socket: string) =>
                    Effect.all([tmux(socket, ['list-sessions', '-F', '#{session_name}']), tmux(socket, ['list-clients', '-F', '#{client_readonly}'])], { concurrency: 'unbounded' }).pipe(
                        Effect.flatMap(([sessions, readers]) =>
                            readers.includes('1') || !Array.isReadonlyArrayNonEmpty(sessions)
                                ? Effect.succeed(Array.empty<string>())
                                : Effect.andThen(
                                      pooled,
                                      Effect.forEach(sessions, (session) => view(socket, session)),
                                  ).pipe(Effect.map(Array.flatten)),
                        ),
                        Effect.tap((panes) => Effect.log(`watch ${socket}: ${Array.isReadonlyArrayNonEmpty(panes) ? `${panes.join(', ')} in ${pool}` : 'already watched or ended'}`)),
                        Effect.catchCause((cause) => Effect.as(Effect.logWarning(`watch ${socket} failed`, cause), Array.empty<string>())),
                    );
                return {
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

const zellij = (args: readonly string[]) => ChildProcess.make('zellij', args, { env: OUTSIDE, extendEnv: true });

// --- [EXPORTS] -------------------------------------------------------------------------

export { Watch };
