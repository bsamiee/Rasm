import { NodeRuntime, NodeServices } from '@effect/platform-node';
import { Array, Console, Effect, FileSystem, Path, Stream } from 'effect';
import { ChildProcess } from 'effect/process';
import { CheckError, diagnoseProjects } from './checks.ts';

// --- [COMPOSITION] ---------------------------------------------------------------------

const lint = Effect.gen(function* () {
    const [fs, path] = yield* Effect.all([FileSystem.FileSystem, Path.Path]);
    const cwd = path.resolve();
    const handle = yield* ChildProcess.make('git', ['ls-files', '--cached', '--others', '--exclude-standard', '-z', '--', ':(glob)**/tsconfig.json'], { cwd, stderr: 'inherit' });
    const [listed, code] = yield* Effect.all([Stream.mkString(Stream.decodeText(handle.stdout)), handle.exitCode], { concurrency: 'unbounded' });
    const configs =
        code === 0
            ? yield* Effect.filter(
                  listed.split('\0').filter((entry) => entry !== ''),
                  (config) => fs.exists(path.join(cwd, config)),
                  { concurrency: 'unbounded' },
              )
            : yield* CheckError.make({ reason: { _tag: 'Unlisted', code } });
    return yield* Array.match(yield* diagnoseProjects(cwd, configs), {
        onEmpty: () => Effect.void,
        onNonEmpty: (lines) => Effect.andThen(Console.log(lines.join('\n')), Effect.fail(CheckError.make({ reason: { _tag: 'Found', count: lines.length } }))),
    });
}).pipe(Effect.scoped);
lint.pipe(Effect.provide(NodeServices.layer), NodeRuntime.runMain);
