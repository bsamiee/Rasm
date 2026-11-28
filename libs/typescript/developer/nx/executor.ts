import type { dlopen, getInt32 } from 'node:ffi';
import { constants, cp } from 'node:fs/promises';
import { constants as system } from 'node:os';
import { basename, dirname, join } from 'node:path';
import { platform } from 'node:process';
import { getSystemErrorMessage } from 'node:util';
import { Worker } from 'node:worker_threads';
import { createProjectFileMapUsingProjectGraph, type ExecutorContext } from '@nx/devkit';
import { createTaskId, matchGlobPaths } from '@nx/devkit/internal';
import { Array, Config, Effect, FileSystem, Function, Graph, Option, PlatformError, Record, Schema, String, Struct } from 'effect';
import { createNxReleaseConfig } from 'nx/src/command-line/release/config/config';
import { getLatestGitTagForPattern, sanitizeProjectNameForGitTag } from 'nx/src/command-line/release/utils/git';
import { RepoGitTags } from 'nx/src/command-line/release/utils/repository-git-tags';
import { getFilesForOutputsBatch } from 'nx/src/native';
import { createTaskGraph } from 'nx/src/tasks-runner/create-task-graph';
import { inside, NxError, request, spawned } from '#/inference';

// --- [MODELS] --------------------------------------------------------------------------

const Dry = Schema.Struct({ dryRun: Schema.optionalKey(Schema.Boolean).annotate({ description: 'Prints each change and makes none, as NX_DRY_RUN=true does' }) });
const Upstream = Schema.Struct({ dependentTasksOutputFiles: Schema.String, transitive: Schema.optionalKey(Schema.Boolean) }).annotate({ description: 'A dependentTasksOutputFiles input of a target, matching the output files of its dependency tasks, transitive ones included where transitive is true' });
const Exchanged = Schema.Struct({ images: Schema.Array(Schema.String), refused: Schema.NullOr(Schema.Struct({ index: Schema.Int, syscall: Schema.String, code: Schema.Int })) }).annotate({
    description:
        'What the swap worker answers: the path each running process launched its executable from, read through KERN_PROCARGS2 on darwin and listing none elsewhere, and the first staged pair whose rename the system refused with its errno, the pairs before it swapped, or null once every pair took its target path',
});

// --- [OPERATIONS] ----------------------------------------------------------------------

const forwarded = (words: readonly string[]) => {
    const owned = new RegExp(`^--(?:no-)?(?:${Array.flatMap(Struct.keys(Dry.fields), (key) => [key, String.kebabCase(key)].map(RegExp.escape)).join('|')})(?:=.*)?$`, 'u');
    return words.filter((word) => !owned.test(word));
};

const credentialed = (variables: string | readonly string[]) => Effect.map(Config.all(Array.ensure(variables).map((variable) => Config.option(Config.String(variable)))), Array.findFirst(Option.filter(String.isNonEmpty)));

const packaged = Effect.fnUntraced(function* ({ root, projectName, targetName, configurationName, projectGraph }: ExecutorContext) {
    const [project, target] = yield* Effect.all([Effect.fromNullishOr(projectName), Effect.fromNullishOr(targetName)]);
    const own = createTaskId(project, target, configurationName);
    const { tasks, dependencies } = yield* Effect.try({ try: () => createTaskGraph(projectGraph, {}, [project], [target], configurationName, {}), catch: NxError.of({ _tag: 'Failed', step: 'task graph' }, own) });
    const declared = Array.filterMap(projectGraph.nodes[project]?.data.targets?.[target]?.inputs ?? [], (input) => Schema.decodeUnknownResult(Upstream)(input));
    const listed = Record.keys(tasks);
    const positions = new Map(listed.map((id, index) => [id, index] as const));
    const edges = listed.flatMap((id, source) => (dependencies[id] ?? []).map((edge) => ({ source, id: edge.id }))).flatMap(({ source, id }) => Array.fromNullishOr(positions.get(id)).map((to) => ({ source, target: to, data: undefined })));
    const graph = Graph.fromSnapshot({ type: 'directed', nodes: listed.map((data, index) => ({ index, data })), edges: edges.map((edge, index) => ({ ...edge, index })) });
    const matched = declared.map(({ dependentTasksOutputFiles, transitive = false }) => {
        const ids = graph.pipe(Graph.dfs({ start: Array.fromNullishOr(positions.get(own)), radius: transitive ? Infinity : 1 }), Graph.values, Array.fromIterable).filter((id) => id !== own);
        const outputs = ids.map((id) => tasks[id]?.outputs ?? []);
        const found = getFilesForOutputsBatch(root, outputs).flat();
        const selected = matchGlobPaths([dependentTasksOutputFiles], found);
        return { input: dependentTasksOutputFiles, tasks: ids, outputs: outputs.flat(), files: found.filter((_, index) => selected[index]) };
    });
    const [outputs, files] = [Array.dedupe(matched.flatMap(Struct.get('outputs'))), Array.dedupe(matched.flatMap(Struct.get('files'))).toSorted()];
    return outputs.length > 0 && files.length === 0 ? yield* NxError.of({ _tag: 'Unmatched', input: matched.map(Struct.get('input')), outputs, tasks: Array.dedupe(matched.flatMap(Struct.get('tasks'))) }, own)() : { project, target, outputs, files };
});

const released = Effect.fnUntraced(function* ({ root, projectName, projectGraph, nxJsonConfiguration }: ExecutorContext) {
    const project = yield* Effect.fromNullishOr(projectName);
    const files = yield* Effect.promise(() => createProjectFileMapUsingProjectGraph(projectGraph));
    const { error, nxReleaseConfig } = yield* Effect.promise(() => createNxReleaseConfig(projectGraph, files, nxJsonConfiguration.release));
    const group = error === null ? Array.findFirst(Record.toEntries(nxReleaseConfig?.groups ?? {}), ([, { projects }]) => projects.includes(project)) : yield* NxError.of({ _tag: 'Invalid', code: error.code }, 'nx.json release')(error);
    const [head, ordered] = yield* Effect.all([spawned(['git', 'tag', '--points-at', 'HEAD'], { cwd: root }), Effect.promise(() => RepoGitTags.create().resolveTags())]);
    const tags = Array.intersection(ordered, head.split('\n'));
    const held = yield* Effect.transposeOption(Option.map(group, ([name, { releaseTag }]) => Effect.promise(() => getLatestGitTagForPattern(releaseTag.pattern, { projectName: sanitizeProjectNameForGitTag(project), releaseGroupName: name }, Function.constant(Promise.resolve(tags)), releaseTag))));
    const tag = Option.flatMap(held, Option.fromNullishOr);
    yield* Option.isNone(tag) ? Effect.annotateLogs(Effect.logInfo('untagged'), { project }) : Effect.void;
    return tag;
});

const swap = (ffi: { readonly dlopen: typeof dlopen; readonly getInt32: typeof getInt32 }, { system: host, pairs }: { readonly system: NodeJS.Platform; readonly pairs: readonly (readonly [from: string, to: string, held: boolean])[] }): typeof Exchanged.Encoded => {
    const hosts = {
        darwin: () => {
            const {
                renamex_np: rename,
                __error: errno,
                proc_listallpids: listed,
                sysctl,
                sysctlbyname,
            } = ffi.dlopen(null, {
                renamex_np: { arguments: ['string', 'string', 'u32'], return: 'i32' },
                __error: { return: 'pointer' },
                proc_listallpids: { arguments: ['buffer', 'i32'], return: 'i32' },
                sysctl: { arguments: ['buffer', 'u32', 'buffer', 'buffer', 'pointer', 'u64'], return: 'i32' },
                sysctlbyname: { arguments: ['string', 'buffer', 'buffer', 'pointer', 'u64'], return: 'i32' },
            }).functions;
            const [CTL_KERN, KERN_PROCARGS2, argmax, decoder] = [1, 49, new Int32Array(1), new TextDecoder()];
            return {
                syscall: 'renamex_np',
                exchange: (from: string, to: string, held: boolean) => rename(from, to, held ? 0x2 : 0),
                errno,
                images: () => {
                    const [pids, read] = [new Int32Array(listed(null, 0)), new BigUint64Array([BigInt(argmax.byteLength)])];
                    sysctlbyname('kern.argmax', argmax, read, null, 0n);
                    const args = new Uint8Array(argmax.at(0) ?? 0);
                    return globalThis.Array.from(pids.subarray(0, listed(pids, pids.byteLength)), (pid) => {
                        read[0] = BigInt(args.byteLength);
                        return sysctl(new Int32Array([CTL_KERN, KERN_PROCARGS2, pid]), 3, args, read, null, 0n) === 0 ? decoder.decode(args.subarray(Int32Array.BYTES_PER_ELEMENT, args.indexOf(0, Int32Array.BYTES_PER_ELEMENT))) : '';
                    }).filter((image) => image !== '');
                },
            };
        },
        linux: () => {
            const { renameat2: rename, __errno_location: errno } = ffi.dlopen(null, { renameat2: { arguments: ['i32', 'string', 'i32', 'string', 'u32'], return: 'i32' }, __errno_location: { return: 'pointer' } }).functions;
            const AT_FDCWD = -100;
            return { syscall: 'renameat2', exchange: (from: string, to: string, held: boolean) => rename(AT_FDCWD, from, AT_FDCWD, to, held ? 0x2 : 0), errno, images: (): readonly string[] => [] };
        },
    };
    const { syscall, exchange, errno, images } = hosts[host === 'darwin' ? 'darwin' : 'linux']();
    const refused = pairs.reduce<typeof Exchanged.Encoded.refused>((found, [from, to, held], index) => found ?? (exchange(from, to, held) === 0 ? null : { index, syscall, code: ffi.getInt32(errno()) }), null);
    return { images: images(), refused };
};

const installed = Effect.fnUntraced(function* <E = never, R = never>(pairs: readonly (readonly [from: string, to: string])[], prepared: (staged: string) => Effect.Effect<unknown, E, R> = () => Effect.void) {
    const fs = yield* FileSystem.FileSystem;
    const staged = yield* Effect.forEach(pairs, ([from, to]) =>
        Effect.gen(function* () {
            yield* fs.makeDirectory(dirname(to), { recursive: true });
            const at = join(yield* fs.makeTempDirectoryScoped({ directory: dirname(to), prefix: `.${basename(to)}-` }), basename(to));
            yield* Effect.tryPromise({ try: () => cp(from, at, { recursive: true, verbatimSymlinks: true, preserveTimestamps: true, mode: constants.COPYFILE_FICLONE }), catch: NxError.of({ _tag: 'Failed', step: 'copy' }, from) });
            yield* prepared(at);
            return { from: at, to, held: yield* fs.exists(to) };
        }),
    );
    const { images, rest } =
        platform === 'win32' || !Array.isReadonlyArrayNonEmpty(staged)
            ? { images: Array.empty<string>(), rest: staged }
            : yield* request(
                  Effect.sync(
                      () =>
                          new Worker(`import { dlopen, getInt32 } from 'node:ffi'; import { parentPort, workerData } from 'node:worker_threads'; parentPort.postMessage((${swap.toString()})({ dlopen, getInt32 }, workerData));`, {
                              eval: true,
                              execArgv: ['--disable-warning=ExperimentalWarning'],
                              workerData: { system: platform, pairs: staged.map(({ from, to, held }) => [from, to, held] as const) },
                          }),
                  ),
                  Exchanged,
                  NxError.of({ _tag: 'Failed', step: 'swap' }, staged.map(Struct.get('to')).join(' ')),
              ).pipe(
                  Effect.flatMap(({ images: running, refused }) =>
                      refused === null || [system.errno.ENOTSUP, system.errno.EINVAL, system.errno.EXDEV].includes(refused.code)
                          ? Effect.succeed({ images: running, rest: staged.slice(refused?.index ?? staged.length) })
                          : PlatformError.systemError({ _tag: 'Unknown', module: 'FileSystem', method: 'installed', syscall: refused.syscall, pathOrDescriptor: staged[refused.index]?.to, description: getSystemErrorMessage(-refused.code) }),
                  ),
              );
    yield* Effect.forEach(
        rest,
        ({ from, to, held }) =>
            held
                ? Effect.andThen(
                      Effect.flatMap(fs.makeTempDirectory({ directory: dirname(from) }), (aside) => fs.rename(to, join(aside, basename(to)))),
                      fs.rename(from, to),
                  )
                : fs.rename(from, to),
        { discard: true },
    );
    return images.filter((image) => staged.some(({ to }) => inside(to)(image)));
}, Effect.scoped);

// --- [EXPORTS] -------------------------------------------------------------------------

export { credentialed, Dry, forwarded, installed, packaged, released };
