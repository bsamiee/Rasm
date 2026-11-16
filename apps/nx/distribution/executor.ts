import { join } from 'node:path';
import { layer, type NodeServices } from '@effect/platform-node/NodeServices';
import type { ExecutorContext } from '@nx/devkit';
import { createTaskId } from '@nx/devkit/internal';
import { Array, Cause, Config, Effect, FileSystem, Function, Option, Result, Schema, type Scope, SemVer, Stream, String, Struct } from 'effect';
import { ChildProcess, type ChildProcessSpawner, CommandLine } from 'effect/process';
import { minimatch } from 'minimatch';

// --- [MODELS] --------------------------------------------------------------------------

const Dry = Schema.Struct({ dryRun: Schema.optionalKey(Schema.Boolean).annotate({ description: 'Prints each change and makes none, as NX_DRY_RUN=true does' }) });
const Upstream = Schema.Struct({ dependentTasksOutputFiles: Schema.String, transitive: Schema.optionalKey(Schema.Boolean) }).annotate({ description: 'A dependentTasksOutputFiles input of a target, matching the output files of its dependency tasks, transitive ones included where transitive is true' });
const Words = Schema.optionalKey(Schema.Array(Schema.String)).annotate({ description: 'Words after -- of the run, appended after the configured arguments', $default: { $source: 'unparsed' }, hidden: true, 'x-priority': 'internal' });

// --- [OPERATIONS] ----------------------------------------------------------------------

const dried = ({ dryRun }: typeof Dry.Type) => Effect.map(Config.Boolean('NX_DRY_RUN').pipe(Config.withDefault(false)), (stated) => dryRun === true || stated);

const credentialed = (variable: string) => Effect.map(Config.option(Config.String(variable)), Option.filter(String.isNonEmpty));

const spawned = Effect.fnUntraced(function* (argv: Array.NonEmptyReadonlyArray<string>, options: ChildProcess.CommandOptions = {}) {
    const handle = yield* ChildProcess.make(Array.headNonEmpty(argv), Array.tailNonEmpty(argv), { stdin: 'ignore', ...options, extendEnv: true, detached: false });
    const [stdout, stderr, code] = yield* Effect.all([Stream.mkString(Stream.decodeText(handle.stdout)), Stream.mkString(Stream.decodeText(handle.stderr)), handle.exitCode], { concurrency: 'unbounded' });
    return code === 0 ? stdout : yield* new Cause.UnknownError(code, [`${CommandLine.display(argv)} exits ${code}`, ...[stderr.trim()].filter(String.isNonEmpty)].join(': '));
}, Effect.scoped);

const listed = Effect.fnUntraced(function* (root: string, outputs: readonly string[]) {
    const fs = yield* FileSystem.FileSystem;
    const [kept, negated] = Array.partition(
        outputs,
        Result.liftPredicate(
            (output: string) => !output.startsWith('!'),
            (output) => output.slice(1),
        ),
    );
    const matched = yield* Effect.forEach(kept, (output) => fs.glob(`${output}{,/**}`, { root, exclude: negated, dot: true }), { concurrency: 'unbounded' });
    return yield* Effect.filter(Array.dedupe(matched.flat()), (at) => Effect.map(fs.stat(join(root, at)), ({ type }) => type === 'File'), { concurrency: 'unbounded' });
});

const packaged = Effect.fnUntraced(function* ({ root, projectName, targetName, configurationName, projectGraph, taskGraph }: ExecutorContext) {
    const [{ tasks, dependencies }, project, target] = yield* Effect.all([Effect.fromNullishOr(taskGraph), Effect.fromNullishOr(projectName), Effect.fromNullishOr(targetName)]);
    const own = createTaskId(project, target, configurationName);
    const declared = Array.filterMap(projectGraph.nodes[project]?.data.targets?.[target]?.inputs ?? [], (input) => Schema.decodeUnknownResult(Upstream)(input));
    const upstream = (ids: ReadonlySet<string>, transitive: boolean): ReadonlySet<string> => {
        const next = new Set([...ids, ...[...ids].filter((id) => transitive || id === own).flatMap((id) => (dependencies[id] ?? []).map((edge) => edge.id))]);
        return next.size === ids.size ? ids : upstream(next, transitive);
    };
    const matched = yield* Effect.forEach(
        declared,
        Effect.fnUntraced(function* ({ dependentTasksOutputFiles, transitive = false }) {
            const outputs = [...upstream(new Set([own]), transitive)].filter((id) => id !== own).map((id) => tasks[id]?.outputs ?? []);
            const files = yield* Effect.forEach(outputs, (stated) => listed(root, stated));
            return { outputs: outputs.flat(), files: files.flat().filter((file) => minimatch(file, dependentTasksOutputFiles, { dot: true })) };
        }),
    );
    const [outputs, files] = [Array.dedupe(matched.flatMap(Struct.get('outputs'))), Array.dedupe(matched.flatMap(Struct.get('files'))).toSorted()];
    return yield* outputs.length > 0 && files.length === 0
        ? Effect.fail(new Cause.UnknownError(declared, `${project} holds no file its dependentTasksOutputFiles input of ${target} matches under [${outputs.join(', ')}], the outputs of the tasks ${target} depends on; run them first`))
        : Effect.succeed({ project, target, outputs, files });
});

const released = Effect.fnUntraced(function* ({ root, projectName, nxJsonConfiguration: { release } }: ExecutorContext) {
    const project = yield* Effect.fromNullishOr(projectName);
    const independent = release?.projectsRelationship === 'independent';
    const pattern = release?.releaseTag?.pattern ?? (independent ? '{projectName}@{version}' : 'v{version}');
    const named = project
        .replaceAll(':', '/')
        .replaceAll(/[\p{Cc} ~^?*[\\]/gu, '-')
        .replaceAll(/\/{2,}/gu, '/')
        .replaceAll(/\.{2,}/gu, '.')
        .replaceAll(/^\/+|\/+$/gu, '');
    const matcher = new RegExp(`^${(independent ? pattern.replaceAll('{projectName}', named) : pattern).split('{version}').map(RegExp.escape).join('(.+)')}$`, 'u');
    const tags = (yield* spawned(['git', 'tag', '--points-at', 'HEAD'], { cwd: root, stdout: 'pipe' })).split('\n').filter((tag) => Option.exists(Option.fromNullishOr(matcher.exec(tag)?.[1]), Function.flow(SemVer.parse, Result.isSuccess)));
    return tags.length > 1 ? yield* new Cause.UnknownError(tags, `HEAD holds [${tags.join(', ')}], each matching ${pattern} for ${project}, in place of the one tag nx release makes per project`) : Array.head(tags);
});

// --- [COMPOSITION] ---------------------------------------------------------------------

const run = <A, E>(effect: Effect.Effect<A, E, NodeServices | ChildProcessSpawner.ChildProcessSpawner | Scope.Scope>) => effect.pipe(Effect.scoped, Effect.provide(layer), Effect.runPromise);

// --- [EXPORTS] -------------------------------------------------------------------------

export { credentialed, Dry, dried, listed, packaged, released, run, spawned, Words };
