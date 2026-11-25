import { basename, dirname, join, posix } from 'node:path';
import { type CreateNodes, createNodesFromFiles, detectPackageManager, hashArray, type ProjectConfiguration } from '@nx/devkit';
import { hashObject } from '@nx/devkit/internal';
import { anchored, fingerprinted, inferring, owned, run, tagged } from '@rasm/nx';
import { Array, Combiner, Effect, FileSystem, Match, Option, Order, Predicate, Record, type Result, Schema } from 'effect';
import { linted } from '#/eslint';
import { compiled, CONFIG, programmed } from '#/programs';
import { installed, loaded, MANIFEST, projected, tarball, worker, WORKER, workspace } from '#/workspace';

// --- [CONSTANTS] -----------------------------------------------------------------------

const PROJECT = 'project.json';

// --- [MODELS] --------------------------------------------------------------------------

const Options = Schema.Struct({
    outputPath: Schema.optionalKey(Schema.String),
    compilerPackages: Schema.NonEmptyArray(Schema.String).pipe(Schema.withDecodingDefaultKey(Effect.succeed(['typescript'] as const))),
    knipConfig: Schema.optionalKey(Schema.String),
}).annotate({
    description:
        'outputPath: workspace folder the package manager packs each tarball to, {projectName} naming the project, absent packs into the package folder. compilerPackages: npm packages every tsc target hashes as external dependencies, the first the native TypeScript package whose tsc runs and whose ./sync API reads each tsconfig.json. knipConfig: workspace file knip reads its configuration from through --config, absent leaving knip to find its own',
});

// --- [OPERATIONS] ----------------------------------------------------------------------

const own = fingerprinted(import.meta.dirname);

const looked = <A, E>(table: ReadonlyMap<string, Result.Result<A, E>>, at: string) => Effect.flatMap(Effect.fromNullishOr(table.get(at)), Effect.fromResult);

const nodes = inferring(Options, run, function* (files, settings, context, cached) {
    const { workspaceRoot } = context;
    const [listed, anchor] = [new Set(files), anchored(workspaceRoot)];
    const held = new Set(Array.dedupe(files.map(dirname)).filter(Predicate.and((root: string) => root !== '.', anchor.holds)));
    const nested = files.filter((file) => file === MANIFEST || (file === CONFIG && listed.has(MANIFEST)) || held.has(dirname(file)));
    const anchors = new Set(Object.values(Array.groupBy(nested, dirname)).map(Array.min(Order.String)));
    const manager = detectPackageManager(workspaceRoot);
    const synced = installed('@nx/js', join(workspaceRoot, MANIFEST));
    const row = hashArray([yield* own, hashObject(settings)]);
    const configured = yield* cached<{ readonly sources: readonly string[] }>('typescript-cloudflare.hash', row);
    const manifests = new Map(yield* Effect.forEach(Array.dedupe(nested.map(dirname)), (root) => loaded(workspaceRoot, root), { concurrency: 'unbounded' }));
    const programs = yield* programmed(workspaceRoot, Array.headNonEmpty(settings.compilerPackages), row, manager, held, nested);
    const standalone = yield* Effect.findFirst(listed.has(MANIFEST) ? ['src', 'lib'] : [], (at) => FileSystem.FileSystem.use((fs) => fs.exists(join(workspaceRoot, at))));
    const lints = yield* linted(workspaceRoot, Array.headNonEmpty(settings.compilerPackages), manager, installed('eslint', join(workspaceRoot, MANIFEST)) ? [...held.values().map((root) => ({ root, scope: root })), ...Option.toArray(Option.map(standalone, (scope) => ({ root: '.', scope })))] : [], row);
    const chained = (root: string) => Array.fromOption(Option.flatMap(Option.fromUndefinedOr(programs.get(posix.join(root, CONFIG))), Option.getSuccess)).flatMap(({ chain }) => Array.cartesianWith(['{workspaceRoot}'], chain.filter(Predicate.not(owned(root))), posix.join));
    const read = Effect.fnUntraced(function* (file: string) {
        const root = dirname(file);
        const described = yield* looked(manifests, root);
        const linting = Option.map(
            Option.filter(Option.fromUndefinedOr(lints.get(root)), () => anchors.has(file) || root === '.'),
            ({ target, typed }) => projected(root, 'eslint', { check: { eslint: { ...target, inputs: [...(target.inputs ?? []), ...(typed ? chained(root) : [])] } } }, described.stated, anchor),
        );
        const branch = yield* Match.value({ file, name: basename(file) }).pipe(
            Match.when({ file: MANIFEST }, () => Effect.asSome(workspace(anchor, manager, settings.knipConfig, row, described))),
            Match.when({ name: PROJECT }, () => Effect.succeedNone),
            Match.when({ name: CONFIG }, () => Effect.map(looked(programs, file), compiled(root, described.stated, settings.compilerPackages, described.manifest.version !== undefined && installed('publint', join(workspaceRoot, file)), synced, manager, anchor))),
            Match.when({ name: WORKER }, () => Effect.when(worker(anchor, manager, described.stated, file, configured.recall), Effect.succeed(installed('cf', join(workspaceRoot, file))))),
            Match.orElse(() => Effect.succeed(tarball(manager, settings.outputPath, described, root, tagged(context.nxJsonConfiguration), anchor))),
        );
        return { projects: Record.getSomes({ [root]: Option.makeReducer(Combiner.make<ProjectConfiguration>((prior, next) => ({ ...prior, ...next, targets: { ...prior.targets, ...next.targets } }))).combine(linting, branch) }) };
    });
    return yield* Effect.promise(() => createNodesFromFiles((file) => run(read(file)), nested, settings, context));
});

// --- [COMPOSITION] ---------------------------------------------------------------------

const createNodes: CreateNodes = [`**/{${CONFIG},${MANIFEST},${WORKER},${PROJECT}}`, nodes];

// --- [EXPORTS] -------------------------------------------------------------------------

export { createNodes };
