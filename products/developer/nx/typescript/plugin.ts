import { basename, dirname, join, posix } from 'node:path';
import { type CreateNodes, createNodesFromFiles, detectPackageManager, getPackageManagerCommand, hashArray, type ProjectConfiguration, type TargetConfiguration } from '@nx/devkit';
import { hashObject } from '@nx/devkit/internal';
import { getLockFileName, getRootTsConfigFileName } from '@nx/js';
import { anchored, fingerprinted, Folder, inferring, namedInput, run, tagged, tokened } from '@rasm/nx-distribution/inference';
import { Array, Effect, FileSystem, Match, Option, Order, Predicate, Record, type Result, Schema } from 'effect';
import { linted } from '#/eslint';
import { compiled, CONFIG, programmed } from '#/programs';
import { installed, loaded, MANIFEST, projected, tarball, worker, WORKER, workspace } from '#/workspace';

// --- [CONSTANTS] -----------------------------------------------------------------------

const PROJECT = 'project.json';

// --- [MODELS] --------------------------------------------------------------------------

const Options = Schema.Struct({
    outputPath: Folder,
    compilerPackages: Schema.NonEmptyArray(Schema.String).pipe(Schema.withDecodingDefaultKey(Effect.succeed(['typescript'] as const))),
    knipConfig: Folder,
}).annotate({
    description:
        'outputPath: workspace folder the package manager packs each tarball to, {projectName} naming the project, absent packs into the package folder. compilerPackages: npm packages every tsc target hashes as external dependencies, the first the native TypeScript package whose tsc runs and whose ./sync API reads each tsconfig.json. knipConfig: workspace file knip reads its configuration from through --config, absent leaving knip to find its own',
});

// --- [OPERATIONS] ----------------------------------------------------------------------

const own = fingerprinted(import.meta.dirname);

const merged = (prior: Option.Option<ProjectConfiguration>, next: ProjectConfiguration) => Option.some(Option.match(prior, { onNone: () => next, onSome: (held) => ({ ...held, ...next, targets: { ...held.targets, ...next.targets } }) }));

const looked = <A, E>(table: ReadonlyMap<string, Result.Result<A, E>>, at: string) => Effect.flatMap(Effect.fromNullishOr(table.get(at)), Effect.fromResult);

const nodes = inferring(Options, run, function* (files, settings, context) {
    const { workspaceRoot } = context;
    const hashed = yield* own;
    const listed = new Set(files);
    const held = new Set(Array.dedupe(files.map(dirname)).filter(Predicate.and((root: string) => root !== '.', anchored(workspaceRoot))));
    const nested = files.filter((file) => file === MANIFEST || (file === CONFIG && listed.has(MANIFEST)) || held.has(dirname(file)));
    const anchors = new Set(
        Object.values(
            Array.groupBy(
                nested.filter((file) => held.has(dirname(file))),
                dirname,
            ),
        ).map(Array.min(Order.String)),
    );
    const manager = detectPackageManager(workspaceRoot);
    const { exec } = getPackageManagerCommand(manager);
    const synced = installed('@nx/js', join(workspaceRoot, MANIFEST));
    const row = hashArray([hashed, hashObject(settings)]);
    const lock = getLockFileName(manager);
    const manifests = new Map(yield* Effect.forEach(Array.dedupe(nested.map(dirname)), (root) => loaded(workspaceRoot, root), { concurrency: 'unbounded' }));
    const programs = yield* programmed(
        workspaceRoot,
        Array.headNonEmpty(settings.compilerPackages),
        row,
        lock,
        held,
        nested.filter((file) => basename(file) === CONFIG),
    );
    const standalone = Option.filter(yield* Effect.findFirst(['src', 'lib'], (at) => FileSystem.FileSystem.use((fs) => fs.exists(join(workspaceRoot, at)))), () => listed.has(MANIFEST));
    const lints = installed('eslint', join(workspaceRoot, MANIFEST))
        ? yield* linted(workspaceRoot, Array.headNonEmpty(settings.compilerPackages), exec, lock, [...[...held].map((root) => ({ root, scope: root })), ...Option.toArray(Option.map(standalone, (scope) => ({ root: '.', scope })))], row)
        : new Map<string, TargetConfiguration>();
    const rooted = getRootTsConfigFileName();
    const chained = (root: string) => Option.match(Option.flatMap(Option.fromUndefinedOr(programs.get(posix.join(root, CONFIG))), Option.getSuccess), { onNone: () => [], onSome: ({ chain }) => chain.filter((at) => posix.relative(root, at).startsWith('..') && at !== rooted).map(tokened(root)) });
    const read = Effect.fnUntraced(function* (file: string) {
        const root = dirname(file);
        const described = yield* looked(manifests, root);
        const { manifest, stated } = described;
        const typed = (program: Parameters<typeof compiled>[1]) =>
            Option.map(
                Option.liftPredicate(program, ({ commands }) => Array.isReadonlyArrayNonEmpty(commands)),
                (checked) => projected(root, 'typescript', compiled(root, checked, settings.compilerPackages, `^${namedInput(root, context)}`, manifest.version !== undefined && installed('publint', join(workspaceRoot, file)), synced, exec), stated),
            );
        const loadable = Effect.sync(() => installed('cf', join(workspaceRoot, file)));
        const linting = Option.map(
            Option.filter(Option.fromUndefinedOr(lints.get(root)), () => anchors.has(file) || root === '.'),
            (eslint) => projected(root, 'eslint', { check: { eslint: { ...eslint, inputs: [...(eslint.inputs ?? []), ...chained(root)] } } }, stated),
        );
        const branch = yield* Match.value({ file, name: basename(file) }).pipe(
            Match.when({ file: MANIFEST }, () => workspace(workspaceRoot, manager, exec, settings.knipConfig, described)),
            Match.when({ name: PROJECT }, () => Effect.succeedNone),
            Match.when({ name: CONFIG }, () => Effect.map(looked(programs, file), typed)),
            Match.when({ name: WORKER }, () => Effect.when(worker(context, exec, stated, file), loadable)),
            Match.orElse(() => Effect.succeed(tarball(manager, exec, settings.outputPath, described, root, tagged(context.nxJsonConfiguration)))),
        );
        return Option.reduceCompact([linting, branch], Option.none<ProjectConfiguration>(), merged);
    });
    const each = (file: string) => run(Effect.map(read(file), (project) => ({ projects: Record.getSomes({ [dirname(file)]: project }) })));
    return yield* Effect.promise(() => createNodesFromFiles(each, nested, settings, context));
});

// --- [COMPOSITION] ---------------------------------------------------------------------

const createNodes: CreateNodes = [`**/{${CONFIG},${MANIFEST},${WORKER},${PROJECT}}`, nodes];

// --- [EXPORTS] -------------------------------------------------------------------------

export { createNodes };
