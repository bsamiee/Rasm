import { layer as platform } from '@effect/platform-node/NodeServices';
import type { Policy } from '@rasm/policy/policy';
import { Shell } from '@rasm/shell';
import { Effect, FileSystem, Layer, Option, Path, Stream } from 'effect';
import { ChildProcess, ChildProcessSpawner } from 'effect/process';
import { processor } from 'sh-syntax/sync';

// --- [TYPES] ---------------------------------------------------------------------------

type Action = Parameters<Policy['decide']>[0];
type Probe = ReturnType<Shell['Service']['dialect']>;

// --- [CONSTANTS] -----------------------------------------------------------------------

const variables = { PATH: '/usr/bin:/bin', GIT_CONFIG_GLOBAL: '/dev/null', GIT_CONFIG_SYSTEM: '/dev/null', GIT_AUTHOR_NAME: 'suite', GIT_AUTHOR_EMAIL: 'suite@example.invalid', GIT_COMMITTER_NAME: 'suite', GIT_COMMITTER_EMAIL: 'suite@example.invalid' };

// --- [OPERATIONS] ----------------------------------------------------------------------

const services = Layer.provideMerge(Layer.effect(Shell, Shell.make({ processor })), platform);
const environment = (cwd: string = import.meta.dirname): Effect.Effect<{ readonly dialect: Effect.Success<Probe>; readonly cwd: string }, Effect.Error<Probe>, Shell> =>
    Effect.map(
        Shell.use((shell) => shell.dialect({ shell: '/bin/bash', environment: { PATH: variables.PATH }, timeout: '1 minute' })),
        (dialect) => ({ dialect, cwd }),
    );
const shell = (command: string, agent: Action['agent'] = Option.none()): Action => ({ _tag: 'Shell', call: command, agent, command, run: 'foreground' });
const printed = (command: string, cwd: string) =>
    ChildProcessSpawner.ChildProcessSpawner.use((spawner) => Effect.scoped(Effect.flatMap(spawner.spawn(ChildProcess.make('/bin/bash', ['-c', command], { cwd, env: variables, extendEnv: false })), (handle) => Effect.all([Stream.mkString(Stream.decodeText(handle.all)), handle.exitCode]))));
const folder = Effect.fnUntraced(function* (files: Readonly<Record<string, string>>) {
    const [fs, path] = yield* Effect.all([FileSystem.FileSystem, Path.Path]);
    const made = yield* fs.realPath(yield* fs.makeTempDirectoryScoped());
    yield* Effect.forEach(Object.entries(files), ([name, text]) => fs.writeFileString(path.join(made, name), text), { discard: true });
    return made;
});

// --- [EXPORTS] -------------------------------------------------------------------------

export { environment, folder, printed, services, shell };
