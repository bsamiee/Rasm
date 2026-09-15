// --- [IMPORTS] -------------------------------------------------------------------------

import { Command, type CommandExecutor, FileSystem, Path } from '@effect/platform';
import { NodeContext, NodeRuntime } from '@effect/platform-node';
import { Array, Cause, Config, Console, Data, Effect, Equal, flow, Schema, Stream } from 'effect';

// --- [CONSTANTS] -----------------------------------------------------------------------

const _REPOSITORY = 'mcneel/RhinoAI';
const _BRANCH = 'rhino-9.x';
const _PACKAGE = 'Rhino-MCP-Platform';

// --- [ERRORS] --------------------------------------------------------------------------

class ToolError extends Data.TaggedError('ToolError')<{ readonly tool: string; readonly exitCode: CommandExecutor.ExitCode }> {
    override get message(): string {
        return `\`${this.tool}\` exited with code ${this.exitCode}`;
    }
}

// --- [MODELS] --------------------------------------------------------------------------

const _Version = Schema.Trim.pipe(Schema.pattern(/^\d+\.\d+\.\d+(?:\.\d+|(?:-[0-9A-Za-z.-]+)?(?:\+[0-9A-Za-z.-]+)?)$/u), Schema.brand('Version'));
const _Router = Schema.TemplateLiteralParser(Schema.String, `${_PACKAGE}/`, _Version, '/router', Schema.String);

// --- [ENTRY] ---------------------------------------------------------------------------

Effect.gen(function* () {
    const fs = yield* FileSystem.FileSystem;
    const path = yield* Path.Path;
    const output = Effect.fnUntraced(function* (tool: string, ...args: string[]) {
        const handle = yield* Command.start(Command.make(tool, ...args).pipe(Command.stderr('inherit')));
        const stdout = yield* Stream.mkString(Stream.decodeText(handle.stdout));
        yield* Effect.filterOrFail(handle.exitCode, Equal.equals(0), (exitCode) => new ToolError({ tool, exitCode }));
        return stdout;
    }, Effect.scoped);
    const run = yield* Schema.decode(Schema.NumberFromString.pipe(Schema.int()))(
        yield* output('gh', 'run', 'list', '-R', _REPOSITORY, '-w', 'Build', '-b', _BRANCH, '-s', 'success', '-L', '1', '--json', 'databaseId', '--jq', '.[0].databaseId'),
    );
    const artifact = yield* fs.makeTempDirectoryScoped();
    yield* output('gh', 'run', 'download', `${run}`, '-R', _REPOSITORY, '-n', 'rhino-mcp-platform-R9-osx', '-D', artifact);
    const [archive] = yield* Schema.decodeUnknown(Schema.Tuple(Schema.String.pipe(Schema.endsWith('.yak'))))(yield* fs.readDirectory(artifact));
    yield* output('yak', 'install', path.join(artifact, archive));
    const version = yield* Schema.decode(_Version)(
        yield* fs.readFileString(path.join(yield* Config.string('HOME'), 'Library', 'Application Support', 'McNeel', 'Rhinoceros', 'packages', '9.0', _PACKAGE, 'manifest.txt')),
    );
    yield* Effect.validateAll(
        ['.mcp.json', '.codex/config.toml'],
        Effect.fnUntraced(function* (harness: string) {
            const file = path.resolve(import.meta.dirname, '..', '..', harness);
            const [prefix, name, _, ...suffix] = yield* Schema.decodeUnknown(_Router)(yield* fs.readFileString(file));
            yield* fs.writeFileString(file, yield* Schema.encode(_Router)([prefix, name, version, ...suffix]));
        }),
        { concurrency: 'unbounded', discard: true },
    );
    yield* Console.log(`${_PACKAGE} ${version} from ${_BRANCH} run ${run}`);
}).pipe(
    Effect.scoped,
    Effect.tapError((error) => Effect.forEach(Array.ensure(error), (failure) => Console.error(`${failure._tag}: ${failure.message}`))),
    Effect.tapDefect(flow(Cause.pretty, Console.error)),
    Effect.provide(NodeContext.layer),
    NodeRuntime.runMain({ disableErrorReporting: true }),
);
