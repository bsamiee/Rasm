import { findPackageJSON } from 'node:module';
import { pathToFileURL } from 'node:url';
import { layer } from '@effect/platform-node/NodeServices';
import { AggregateCreateNodesError, type CreateNodes, type CreateNodesContext, type CreateNodesResultArray, hashArray, type TargetConfiguration } from '@nx/devkit';
import { calculateHashesForCreateNodes, hashObject, hashWithWorkspaceContext, PluginCache, quoteShellArg, workspaceDataDirectory } from '@nx/devkit/internal';
import { API, type ParsedCommandLine } from '@typescript/native/sync';
import { Array, Effect, FileSystem, Option, Path, Record, Schema } from 'effect';

// --- [TYPES] ---------------------------------------------------------------------------

type Program = Effect.Success<ReturnType<typeof programs>>[number];

// --- [CONSTANTS] -----------------------------------------------------------------------

const CONFIG = 'tsconfig.json';
const WORKSPACE = 'pnpm-workspace.yaml';
const TECHNOLOGIES = ['typescript'];

// --- [MODELS] --------------------------------------------------------------------------

const Manifest = Schema.fromJsonString(Schema.Struct({ name: Schema.String })).annotate({ description: 'The package.json closest to this module, its own package in the tree and in dist alike' });

// --- [OPERATIONS] ----------------------------------------------------------------------

const line = (argv: readonly string[]) => argv.map(quoteShellArg).join(' ');

const programs = Effect.fnUntraced(function* (workspaceRoot: string, roots: readonly string[]) {
    const path = yield* Path.Path;
    const relative = (at: string) => path.relative(workspaceRoot, at);
    const referenced = ({ projectReferences = [] }: ParsedCommandLine) => projectReferences.map(({ path: at }) => (path.extname(at) === '.json' ? at : path.join(at, CONFIG)));
    const api = yield* Effect.acquireRelease(
        Effect.sync(() => new API({ cwd: workspaceRoot })),
        (opened) => Effect.sync(() => opened.close()),
    );
    const solutions = yield* Effect.try(() => roots.map((root) => ({ root, configs: [path.join(workspaceRoot, root, CONFIG), ...referenced(api.parseConfigFile(path.join(workspaceRoot, root, CONFIG)))] })));
    const configs = solutions.flatMap((solution) => solution.configs);
    const projects = yield* Effect.try(() => {
        const snapshot = api.createSnapshot({ openProjects: configs });
        return configs.map((config) => snapshot.getConfiguredProject(config));
    });
    const loaded = Record.fromEntries(Array.zipWith(configs, projects, (config, project) => [config, project === undefined ? Option.none() : Option.some({ config, parsed: project.parsedCommandLine, chain: project.program.getConfigFileNames() })] as const));
    return solutions.map(({ root, configs: owned }) => {
        const present = Array.getSomes(owned.map((config) => Option.flatten(Record.get(loaded, config))));
        const holding = present.filter(({ parsed }) => parsed.fileNames.length > 0);
        const checked = Option.match(
            Array.findFirst(holding, ({ config }) => config === owned[0]),
            { onSome: Array.of, onNone: () => holding },
        );
        return {
            root,
            commands: checked.map(({ config, parsed }) => line(['tsc', referenced(parsed).length > 0 ? '--build' : '--project', path.relative(path.join(workspaceRoot, root), config)])),
            outputs: Array.dedupe(
                holding
                    .filter(({ parsed }) => parsed.options.noEmit !== true)
                    .flatMap(({ parsed: { options } }) => [
                        ...(options.outDir === undefined ? [] : [path.join(relative(options.outDir), options.resolveJsonModule === true ? '**/*.{js,cjs,mjs,jsx,json,d.ts,d.cts,d.mts}{,.map}' : '**/*.{js,cjs,mjs,jsx,d.ts,d.cts,d.mts}{,.map}')]),
                        ...(options.declarationDir === undefined ? [] : [path.join(relative(options.declarationDir), '**/*.{d.ts,d.cts,d.mts}{,.map}')]),
                    ]),
            ),
            chain: Array.dedupe(present.flatMap(({ chain }) => chain.map(relative))).filter((at) => !at.startsWith(`${root}/`)),
        };
    });
}, Effect.scoped);

const aggregated = (targets: Record.ReadonlyRecord<string, TargetConfiguration>): Record.ReadonlyRecord<string, TargetConfiguration> => ({
    ...targets,
    ...Record.map(
        Array.groupBy(
            Record.toEntries(targets).filter(([name]) => name.includes('--')),
            ([name]) => name.slice(0, name.indexOf('--')),
        ),
        (members, verb): TargetConfiguration => ({
            executor: 'nx:noop',
            dependsOn: [`${verb}--*`],
            inputs: ['...', ...Array.dedupe(members.flatMap(([, member]) => member.inputs ?? []))],
            metadata: { technologies: TECHNOLOGIES, description: `Runs every ${verb}--* target` },
        }),
    ),
});

const compiled = (path: Path.Path, program: Program, external: readonly string[], linted: boolean): Record.ReadonlyRecord<string, TargetConfiguration> => {
    const metadata = (description: string, example: readonly string[]) => ({ technologies: TECHNOLOGIES, description, help: { command: 'tsc --help', example: { args: [...example] } } });
    const tsc: TargetConfiguration = {
        executor: 'nx:run-commands',
        options: { commands: program.commands, cwd: program.root },
        cache: true,
        inputs: ['default', '^default', ...program.chain.map((at) => path.join('{workspaceRoot}', at)), { externalDependencies: ['@typescript/native', ...external] }],
        outputs: [],
    };
    return Array.match(program.outputs, {
        onEmpty: () => ({ 'check--tsc': { ...tsc, metadata: metadata('Type-checks every program of the project with tsc', ['--extendedDiagnostics']) } }),
        onNonEmpty: (outputs) => ({
            'build--tsc': { ...tsc, outputs: outputs.map((at) => path.join('{workspaceRoot}', at)), metadata: metadata('Emits JavaScript and declarations with tsc, type-checking every program it builds', ['--listEmittedFiles']) },
            'check--tsc': { executor: 'nx:noop', dependsOn: ['build--tsc'], inputs: tsc.inputs ?? [], metadata: metadata('Type-checks through the build--tsc compile', ['--extendedDiagnostics']) },
            ...(linted
                ? {
                      'check--publint': {
                          executor: 'nx:run-commands',
                          options: { command: 'publint --strict', cwd: program.root },
                          dependsOn: ['build--*'],
                          cache: true,
                          inputs: ['default', { dependentTasksOutputFiles: '**/*' }, { externalDependencies: ['publint'] }],
                          outputs: [],
                          metadata: { technologies: TECHNOLOGIES, description: 'Lints the tarball the package manager packs against its package.json with publint, warnings failing through --strict', help: { command: 'publint run --help', example: { args: ['--level', 'warning'] } } },
                      },
                  }
                : {}),
        }),
    });
};

const inferred = Effect.fnUntraced(function* (files: readonly string[], options: unknown, context: CreateNodesContext) {
    const [path, fs] = yield* Effect.all([Path.Path, FileSystem.FileSystem]);
    const { workspaceRoot } = context;
    const beside = (marker: string) => Effect.filter(Array.dedupe(files.filter((file) => path.basename(file) === marker).map(path.dirname)).toSorted(), (root) => fs.exists(path.join(workspaceRoot, root, 'package.json')), { concurrency: 'unbounded' });
    const [roots, workspaces] = yield* Effect.all([beside(CONFIG), beside(WORKSPACE)]);
    const installed = (specifier: string) => Option.isSome(Option.liftThrowable(findPackageJSON)(specifier, pathToFileURL(path.join(workspaceRoot, 'package.json'))));
    const external = installed('@effect/tsgo') ? ['@effect/tsgo'] : [];
    const located = yield* Effect.fromNullishOr(findPackageJSON(import.meta.url));
    const { name } = yield* Effect.flatMap(fs.readFileString(located), Schema.decodeEffect(Manifest));
    const source = hashArray(yield* Effect.forEach((yield* fs.readDirectory(import.meta.dirname)).toSorted(), (module) => fs.readFileString(path.join(import.meta.dirname, module))));
    const hashes = yield* Effect.promise(() => calculateHashesForCreateNodes(roots, { options, source }, context));
    const cache = new PluginCache<Program & { readonly digest: string }>(path.join(workspaceDataDirectory, `${path.basename(name)}-${hashObject(options ?? {})}.hash`));
    const digest = (chain: readonly string[]) => Effect.promise(() => hashWithWorkspaceContext(workspaceRoot, [...chain]));
    const held = Array.zipWith(roots, hashes, (root, hash) => ({ root, hash, held: Option.fromNullishOr(cache.get(hash)) }));
    const current = yield* Effect.forEach(held, (project) => digest(Option.match(project.held, { onNone: () => [], onSome: ({ chain }) => chain })));
    const keyed = Array.zipWith(held, current, (project, now) => ({ ...project, entry: Option.filter(project.held, ({ digest: then }) => then === now) }));
    const missed = keyed.filter(({ entry }) => Option.isNone(entry));
    const read = yield* Array.match(missed, {
        onEmpty: () => Effect.succeed([]),
        onNonEmpty: (rows) =>
            programs(
                workspaceRoot,
                rows.map(({ root }) => root),
            ),
    });
    const digests = yield* Effect.forEach(read, (program) => digest(program.chain));
    yield* Effect.sync(() => Array.zip(missed, Array.zip(read, digests)).forEach(([{ hash }, [program, chained]]) => cache.set(hash, { ...program, digest: chained })));
    yield* Effect.sync(() => cache.writeToDisk());
    const linted = installed('publint');
    const upgrade = {
        executor: `${name}:upgrade`,
        cache: false,
        outputs: [`{projectRoot}/${WORKSPACE}`, '{projectRoot}/pnpm-lock.yaml'],
        metadata: { technologies: TECHNOLOGIES, description: 'Moves every catalog row to the highest version its dist-tags name, then refreshes the lockfile with pnpm update', help: { command: 'pnpm update --help', example: { args: ['--recursive'] } } },
    } satisfies TargetConfiguration;
    return [
        ...[...Array.getSomes(keyed.map(({ entry }) => entry)), ...read].map((program): CreateNodesResultArray[number] => [path.join(program.root, CONFIG), { projects: { [program.root]: { metadata: { technologies: TECHNOLOGIES }, targets: aggregated(compiled(path, program, external, linted)) } } }]),
        ...workspaces.map((root): CreateNodesResultArray[number] => [path.join(root, WORKSPACE), { projects: { [root]: { targets: aggregated({ 'upgrade--pnpm': upgrade }) } } }]),
    ];
});

// --- [COMPOSITION] ---------------------------------------------------------------------

const createNodes: CreateNodes = [
    `**/{${CONFIG},${WORKSPACE}}`,
    (files, options, context) =>
        inferred(files, options, context).pipe(
            Effect.mapError((error) => new AggregateCreateNodesError([[null, error]], [])),
            Effect.provide(layer),
            Effect.runPromise,
        ),
];

// --- [EXPORTS] -------------------------------------------------------------------------

export { createNodes };
