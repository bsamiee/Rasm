// --- [IMPORTS] -------------------------------------------------------------------------

import { NodeRuntime, NodeServices } from '@effect/platform-node';
import { Array, Cause, Config, Console, Data, Effect, Equal, FileSystem, flow, Path, Schema, Stream } from 'effect';
import { ChildProcess, ChildProcessSpawner } from 'effect/unstable/process';

// --- [CONSTANTS] -----------------------------------------------------------------------

const _REPOSITORY = 'mcneel/RhinoAI';
const _BRANCH = 'rhino-9.x';
const _PACKAGE = 'Rhino-MCP-Platform';

// --- [ERRORS] --------------------------------------------------------------------------

class ToolError extends Data.TaggedError('ToolError')<{ readonly tool: string; readonly exitCode: ChildProcessSpawner.ExitCode }> {
    override get message(): string {
        return `\`${this.tool}\` exited with code ${this.exitCode}`;
    }
}

// --- [MODELS] --------------------------------------------------------------------------

const _Version = Schema.Trim.pipe(Schema.check(Schema.isPattern(/^\d+\.\d+\.\d+(?:\.\d+|(?:-[0-9A-Za-z.-]+)?(?:\+[0-9A-Za-z.-]+)?)$/u)), Schema.brand('Version'));
const _Router = Schema.TemplateLiteralParser([Schema.String, `${_PACKAGE}/`, _Version, '/router', Schema.String]);

// --- [ENTRY] ---------------------------------------------------------------------------

Effect.gen(function* () {
    const fs = yield* FileSystem.FileSystem;
    const path = yield* Path.Path;
    const spawner = yield* ChildProcessSpawner.ChildProcessSpawner;
    const output = Effect.fnUntraced(function* (tool: string, ...args: string[]) {
        const handle = yield* spawner.spawn(ChildProcess.make(tool, args, { stderr: 'inherit' }));
        const stdout = yield* Stream.mkString(Stream.decodeText(handle.stdout));
        yield* Effect.filterOrFail(handle.exitCode, Equal.equals(0), (exitCode) => new ToolError({ tool, exitCode }));
        return stdout;
    }, Effect.scoped);
    const run = yield* Schema.decodeEffect(Schema.NumberFromString.pipe(Schema.check(Schema.isInt())))(
        yield* output('gh', 'run', 'list', '-R', _REPOSITORY, '-w', 'Build', '-b', _BRANCH, '-s', 'success', '-L', '1', '--json', 'databaseId', '--jq', '.[0].databaseId'),
    );
    const artifact = yield* fs.makeTempDirectoryScoped();
    yield* output('gh', 'run', 'download', `${run}`, '-R', _REPOSITORY, '-n', 'rhino-mcp-platform-R9-osx', '-D', artifact);
    const [archive] = yield* Schema.decodeUnknownEffect(Schema.Tuple([Schema.String.pipe(Schema.check(Schema.isEndsWith('.yak')))]))(yield* fs.readDirectory(artifact));
    yield* output('yak', 'install', path.join(artifact, archive));
    const version = yield* Schema.decodeEffect(_Version)(
        yield* fs.readFileString(path.join(yield* Config.String('HOME'), 'Library', 'Application Support', 'McNeel', 'Rhinoceros', 'packages', '9.0', _PACKAGE, 'manifest.txt')),
    );
    const [failures] = yield* Effect.partition(
        ['.mcp.json', '.codex/config.toml'],
        Effect.fnUntraced(function* (harness: string) {
            const file = path.resolve(import.meta.dirname, '..', '..', harness);
            const [prefix, name, _, ...suffix] = yield* Schema.decodeUnknownEffect(_Router)(yield* fs.readFileString(file));
            yield* fs.writeFileString(file, yield* Schema.encodeEffect(_Router)([prefix, name, version, ...suffix]));
        }),
        { concurrency: 'unbounded' },
    );
    yield* Array.match(failures, { onEmpty: () => Effect.void, onNonEmpty: Effect.fail });
    yield* Console.log(`${_PACKAGE} ${version} from ${_BRANCH} run ${run}`);
}).pipe(
    Effect.scoped,
    Effect.tapError((error) => Effect.forEach(Array.ensure(error), (failure) => Console.error(`${failure._tag}: ${failure.message}`))),
    Effect.tapDefect(flow(Cause.die, Cause.pretty, Console.error)),
    Effect.provide(NodeServices.layer),
    NodeRuntime.runMain({ disableErrorReporting: true }),
);
