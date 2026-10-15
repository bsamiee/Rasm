import { NodeRuntime, NodeServices } from '@effect/platform-node';
import { normalizePath, workspaceRoot } from '@nx/devkit';
import { Array, Config, Effect, FileSystem, Option, Path, Schema, Stdio } from 'effect';
import { ChildProcess, ChildProcessSpawner } from 'effect/process';

// --- [CONSTANTS] -----------------------------------------------------------------------

const WRITERS: readonly { readonly target: string; readonly extensions: Option.Option<readonly string[]> }[] = [
    { target: 'format:biome', extensions: Option.none() },
    { target: 'format:ruff', extensions: Option.some(['.py', '.pyi', '.pyw', '.ipynb']) },
    { target: 'format:yamlfmt', extensions: Option.some(['.yaml', '.yml']) },
    { target: 'format:tombi', extensions: Option.some(['.toml']) },
    { target: 'format:google-java-format', extensions: Option.some(['.java']) },
    { target: 'format:swift-format', extensions: Option.some(['.swift']) },
    { target: 'format:rustfmt', extensions: Option.some(['.rs']) },
];

// --- [ERRORS] --------------------------------------------------------------------------

class WriterExited extends Schema.TaggedError<WriterExited, { readonly brand: unique symbol }>()('WriterExited', { target: Schema.String, code: Schema.Finite }) {}

// --- [COMPOSITION] ---------------------------------------------------------------------

const format = Effect.gen(function* () {
    const [fs, path, stdio, spawner, project] = yield* Effect.all([FileSystem.FileSystem, Path.Path, Stdio.Stdio, ChildProcessSpawner.ChildProcessSpawner, Config.String('NX_TASK_TARGET_PROJECT')]);
    const [argued, files] = yield* Effect.all(
        [
            Effect.map(stdio.args, (args) => args.map((entry) => normalizePath(path.relative(workspaceRoot, path.resolve(entry))))),
            Effect.map(spawner.string(ChildProcess.make('git', ['ls-files', '--cached', '--others', '--exclude-standard', '-z'], { cwd: workspaceRoot })), (text) => text.split('\0').filter((entry) => entry !== '')),
        ],
        { concurrency: 'unbounded' },
    );
    const paths = yield* Effect.filter(Array.intersection(argued, files), (entry) => fs.exists(path.resolve(workspaceRoot, entry)), { concurrency: 'unbounded' });
    return yield* Effect.validate(
        WRITERS,
        ({ target, extensions }) => {
            const named = Option.match(extensions, { onNone: () => paths, onSome: (listed) => paths.filter((entry) => listed.includes(path.extname(entry))) });
            return named.length === 0
                ? Effect.void
                : Effect.filterOrFail(
                      spawner.exitCode(ChildProcess.make('nx', ['run', `${project}:${target}`, '--configuration=files', '--', ...named], { stdout: 'inherit', stderr: 'inherit' })),
                      (code) => code === 0,
                      (code) => WriterExited.make({ target, code }),
                  );
        },
        { concurrency: 'unbounded', discard: true },
    );
});
format.pipe(Effect.provide(NodeServices.layer), NodeRuntime.runMain);
