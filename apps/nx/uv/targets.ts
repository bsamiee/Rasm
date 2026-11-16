import { dirname, extname, join, relative } from 'node:path';
import { fileURLToPath } from 'node:url';
import { layer, type NodeServices } from '@effect/platform-node/NodeServices';
import { type RawProjectGraphDependency, readJsonFile, type TargetConfiguration } from '@nx/devkit';
import { mergeTargetConfigurations, type PackageJson, quoteShellArg } from '@nx/devkit/internal';
import distribution from '@rasm/nx-distribution/package.json' with { type: 'json' };
import { Array, Cause, Context, Effect, FileSystem, identity, Layer, ManagedRuntime, Option, Predicate, pipe, Record, Result, Schema, SchemaTransformation, type Scope, Stream, String, Struct, UndefinedOr } from 'effect';
import { Ini, Toml } from 'effect/encoding';
import { ChildProcess, CommandLine } from 'effect/process';
import type { Member } from '#/workspace';

// --- [TYPES] ---------------------------------------------------------------------------

type Checker = keyof typeof CHECKERS;
type Verbs = Readonly<Record<string, Readonly<Record<string, TargetConfiguration>>>>;

// --- [CONSTANTS] -----------------------------------------------------------------------

const CHECKERS = {
    ty: {
        verb: 'check',
        variables: ['TY_UV'],
        hashes: ['platform'],
        files: [['ty.toml'], ['pyproject.toml', 'tool', 'ty']],
        matches: ['*.py', '*.pyi'],
        sources: ['*.py', '*.pyi'],
        folder: identity<string>,
        run: [],
        command: (at: string) => ['ty', 'check', at],
        mypy: undefined,
        coverage: undefined,
        description: 'Type-checks the member with ty check',
        help: 'ty check --help',
        example: ['--output-format', 'concise'],
    },
    mypy: {
        verb: 'check',
        variables: [],
        hashes: ['platform', 'python'],
        files: [['mypy.ini'], ['.mypy.ini'], ['pyproject.toml', 'tool', 'mypy'], ['setup.cfg', 'mypy']],
        matches: ['*.py', '*.pyi'],
        sources: ['*.py', '*.pyi'],
        folder: identity<string>,
        run: [],
        command: (at: string) => ['mypy', at],
        mypy: [],
        coverage: undefined,
        description: 'Type-checks the member with mypy and the plugins its configuration loads',
        help: 'mypy --help',
        example: ['--output', 'json'],
    },
    refurb: {
        verb: 'check',
        variables: [],
        hashes: ['platform', 'python'],
        files: [['pyproject.toml', 'tool', 'refurb']],
        matches: ['*.py', '*.pyi'],
        sources: ['*.py', '*.pyi'],
        folder: (_: string, base: string) => base,
        run: [],
        command: (at: string, config?: string) => ['refurb', ...(config === undefined ? [] : ['--config-file', config]), at],
        mypy: ['--'],
        coverage: undefined,
        description: 'Reports the findings of the member with refurb over a mypy build',
        help: 'refurb --help',
        example: ['--format', 'github'],
    },
    vulture: {
        verb: 'check',
        variables: [],
        hashes: [],
        files: [['pyproject.toml', 'tool', 'vulture']],
        matches: ['*.py'],
        sources: ['*.py'],
        folder: identity<string>,
        run: [],
        command: (at: string, config?: string) => ['vulture', at, ...(config === undefined ? [] : ['--config', config])],
        mypy: undefined,
        coverage: undefined,
        description: 'Reports the functions, classes, methods, and attributes of the member that nothing reads with vulture',
        help: 'vulture --help',
        example: ['--min-confidence', '80'],
    },
    pytest: {
        verb: 'test',
        variables: [],
        hashes: ['platform', 'python'],
        files: [['pytest.toml', 'pytest'], ['.pytest.toml', 'pytest'], ['pytest.ini'], ['.pytest.ini'], ['pyproject.toml', 'tool', 'pytest'], ['tox.ini', 'pytest'], ['setup.cfg', 'tool:pytest']],
        matches: ['test_*.py', '*_test.py'],
        sources: ['*'],
        run: ['--isolated'],
        folder: identity<string>,
        command: (at: string) => ['pytest', at],
        mypy: undefined,
        coverage: 'pytest-cov',
        description: 'Runs the member tests with pytest',
        help: 'pytest --help',
        example: ['-k', '<expression>'],
    },
} as const;
const PIXI = 'pixi.toml';
const PLUGIN = readJsonFile<PackageJson>(fileURLToPath(import.meta.resolve('#package'))).name;
const PLATFORM = { runtime: 'node -p process.platform+process.arch' } as const;
const UV = { runtime: 'uv self version --short' } as const;
const CONFIGS = Array.dedupe([...Object.values(CHECKERS).flatMap(({ files }) => files.map(([file]) => file)), PIXI]);
const SEPARATOR = /\s+/u;

// --- [MODELS] --------------------------------------------------------------------------

const Options = Schema.Struct({ outputPath: Schema.optionalKey(Schema.String), coveragePath: Schema.optionalKey(Schema.String) }).annotate({
    description:
        "outputPath: workspace folder uv build writes a member's distributions to, {projectName} naming the member, absent leaves uv its own dist folder of the uv workspace. coveragePath: workspace folder the coverage configuration of pytest writes its coverage.lcov report to through --cov-report=lcov:<path> and its .coverage data file to through COVERAGE_FILE, {projectName} naming the member, absent writes both to the folder pytest runs from",
});
const Input = Schema.declare((input: unknown): input is NonNullable<TargetConfiguration['inputs']>[number] => Predicate.isString(input) || Predicate.isObject(input));
const Overrides = Schema.fromJsonString(Schema.Struct({ nx: Schema.optionalKey(Schema.Struct({ targets: Schema.optionalKey(Schema.Record(Schema.String, Schema.Struct({ inputs: Schema.optionalKey(Schema.mutable(Schema.Array(Input))) }))) })) })).annotate({
    description: "The nx.targets rows of a project's package.json, whose inputs Nx merges over the inferred targets after every plugin",
});
const Table = Schema.Record(Schema.String, Schema.Unknown);
const Patterns = Schema.Struct({
    python_files: Schema.optionalKey(
        Schema.Union([Schema.Array(Schema.String), Schema.String.pipe(Schema.decodeTo(Schema.Array(Schema.String), SchemaTransformation.transform({ decode: (text): readonly string[] => text.split(SEPARATOR).filter(String.isNonEmpty), encode: (files: readonly string[]) => files.join(' ') })))]),
    ),
});
const Settings = Patterns.mapFields(Struct.assign({ ini_options: Schema.optionalKey(Patterns), pytest: Schema.optionalKey(Patterns), cache_dir: Schema.optionalKey(Schema.String) }));

// --- [SERVICES] ------------------------------------------------------------------------

class Linked extends Context.Service<Linked, Map<string, readonly RawProjectGraphDependency[]>>()(`${PLUGIN}/targets/Linked`) {
    static readonly layer = Layer.sync(Linked, () => new Map());
}

// --- [OPERATIONS] ----------------------------------------------------------------------

const spawned = Effect.fnUntraced(function* (argv: Array.NonEmptyReadonlyArray<string>, options: ChildProcess.CommandOptions = {}) {
    const handle = yield* ChildProcess.make(Array.headNonEmpty(argv), Array.tailNonEmpty(argv), { stdin: 'ignore', ...options, extendEnv: true, detached: false });
    const [stdout, stderr, code] = yield* Effect.all([Stream.mkString(Stream.decodeText(handle.stdout)), Stream.mkString(Stream.decodeText(handle.stderr)), handle.exitCode], { concurrency: 'unbounded' });
    return code === 0 ? stdout : yield* new Cause.UnknownError(code, [`${CommandLine.display(argv)} exits ${code}`, ...[stderr.trim()].filter(String.isNonEmpty)].join(': '));
}, Effect.scoped);

const manifested = Effect.fnUntraced(function* (workspaceRoot: string, root: string) {
    const fs = yield* FileSystem.FileSystem;
    const manifest = join(workspaceRoot, root, 'package.json');
    return (yield* fs.exists(manifest)) ? ((yield* Effect.flatMap(fs.readFileString(manifest), Schema.decodeEffect(Overrides))).nx?.targets ?? {}) : {};
});

const aggregated = (verbs: Verbs, stated: Readonly<Record<string, Pick<TargetConfiguration, 'inputs'>>>, technology: string): Record<string, TargetConfiguration> =>
    Record.fromEntries(
        Record.collect(Record.filter(verbs, Predicate.not(Record.isEmptyRecord)), (verb, tools): readonly (readonly [string, TargetConfiguration])[] => [
            [
                verb,
                {
                    dependsOn: [`${verb}--*`],
                    inputs: [
                        '...',
                        ...Array.dedupe(
                            Record.collect(tools, (tool, target) => mergeTargetConfigurations(stated[`${verb}--${tool}`] ?? {}, { ...target, inputs: target.inputs ?? ['default', '^default'] }).inputs ?? [])
                                .flat()
                                .map((input) => (Predicate.isObject(input) && 'dependentTasksOutputFiles' in input ? { ...input, transitive: true } : input)),
                        ),
                    ],
                },
            ],
            ...Record.collect(tools, (tool, target): readonly [string, TargetConfiguration] => [`${verb}--${tool}`, { ...target, metadata: { technologies: [technology], ...target.metadata } }]),
        ]).flat(),
    );

const flagged = (flag: string, value: string | undefined): readonly string[] => (value === undefined ? [] : [flag, value]);

const ancestors = (folder: string): readonly string[] => (folder === '.' ? [folder] : [folder, ...ancestors(dirname(folder))]);

const documents = Effect.fnUntraced(function* (workspaceRoot: string, roots: readonly string[]) {
    const fs = yield* FileSystem.FileSystem;
    const present = yield* Effect.filter(Array.cartesianWith(Array.dedupe(roots.flatMap(ancestors)), CONFIGS, join), (at) => fs.exists(join(workspaceRoot, at)), { concurrency: 'unbounded' });
    const read = yield* Effect.forEach(present, (at) => Effect.map(fs.readFileString(join(workspaceRoot, at)), (text) => [at, extname(at) === '.toml' ? Toml.parse(text) : Result.succeed<unknown>(Ini.parse(text))] as const), { concurrency: 'unbounded' });
    return {
        documented: new Map(read.map(([at, parsed]) => [at, Result.getOrElse(parsed, Record.empty)] as const)),
        unparsed: Array.filterMap(read, ([at, parsed]) => Result.map(Result.flip(parsed), (cause) => new Cause.UnknownError(cause, `${at} does not read as TOML or INI`))),
    };
});

const declared = (documented: ReadonlyMap<string, unknown>, root: string, checker: Checker) => {
    const candidates = ancestors(root).flatMap((folder) => CHECKERS[checker].files.map(([file, ...keys]) => [join(folder, file), keys] as const));
    const found = Array.findFirstWithIndex(candidates, ([at, keys]) =>
        Option.flatMap(
            keys.reduce<Option.Option<unknown>>((table, key) => table.pipe(Option.flatMap(Schema.decodeUnknownOption(Table)), Option.flatMap(Record.get(key))), Option.fromUndefinedOr(documented.get(at))),
            (table) => Option.map(Schema.decodeUnknownOption(Settings)(table), (decoded) => [at, decoded] as const),
        ),
    );
    const settings = Option.getOrUndefined(Option.map(found, ([[, stated]]) => stated));
    return {
        config: Option.getOrUndefined(Option.map(found, ([[config]]) => config)),
        cache: settings?.cache_dir,
        inputs: Option.match(found, {
            onNone: () => [],
            onSome: ([, index]) =>
                Array.dedupe(
                    candidates
                        .slice(0, index + 1)
                        .filter(([at]) => documented.has(at))
                        .map(([at]) => join('{workspaceRoot}', at)),
                ),
        }),
        patterns: (settings?.python_files ?? settings?.ini_options?.python_files ?? settings?.pytest?.python_files ?? CHECKERS[checker].matches).map((pattern) => join(root, '**', pattern)),
    };
};

const line = (argv: readonly string[]) => argv.map(quoteShellArg).join(' ');

const joined = (held: Readonly<Record<string, TargetConfiguration>>, added: Readonly<Record<string, TargetConfiguration>>) => ({ ...held, ...added });

const targets = (member: Member, { outputPath, coveragePath }: typeof Options.Type, manifest: Readonly<Record<string, Pick<TargetConfiguration, 'inputs'>>>): Record<string, TargetConfiguration> => {
    const named = (template: string) => template.replaceAll('{projectName}', member.name);
    const root = member.root === member.workspace;
    const distributions = Option.map(Option.fromUndefinedOr(outputPath), named);
    const folder = Option.getOrElse(distributions, () => join(member.workspace, 'dist'));
    const built = [relative(member.root, folder)].filter((held) => !held.startsWith('..')).map((held) => `!${join('{projectRoot}', held, '**', '*')}`);
    const hashed = { platform: PLATFORM, python: { runtime: line(['uv', 'python', 'find', '--show-version', '--directory', member.workspace]) } };
    const checked = Array.getSomes(
        Record.collect(CHECKERS, (key, row) =>
            Option.gen(function* () {
                const stated = yield* Option.filter(Option.fromUndefinedOr(member.checks[key]), ({ locked }) => locked && !member.nested);
                const base = row.folder(member.workspace, yield* Option.fromUndefinedOr(stated.base));
                const { cache = '.mypy_cache', config } = Option.getOrElse(Record.get(member.checks, 'mypy'), () => stated);
                const argv = [
                    'uv',
                    'run',
                    '--no-active',
                    '--locked',
                    ...row.run,
                    ...(relative(member.workspace, base).startsWith('..') ? ['--project', relative(base, member.workspace)] : []),
                    '--package',
                    member.name,
                    '--all-extras',
                    ...flagged('--group', stated.group),
                    ...row.command(
                        relative(base, member.root) || '.',
                        UndefinedOr.map(stated.config, (file) => relative(base, file)),
                    ),
                    ...(row.mypy === undefined ? [] : [...row.mypy, '--cache-dir', join(cache.replace('$MYPY_CONFIG_FILE_DIR', relative(base, config === undefined ? base : dirname(config)) || '.'), member.root, key)]),
                ];
                const reported = coveragePath ?? base;
                const covered = {
                    command: line([...argv, '--cov', `--cov-report=lcov:${join(relative(base, reported), 'coverage.lcov')}`]),
                    env: { COVERAGE_FILE: join(relative(base, reported), '.coverage') },
                    outputs: [join('{workspaceRoot}', reported, '{coverage.lcov,.coverage}')],
                };
                const target: TargetConfiguration = {
                    command: line(argv),
                    options: { cwd: base },
                    cache: true,
                    inputs: [
                        ...row.sources.map((glob) => join('{projectRoot}', '**', glob)),
                        ...built,
                        ...stated.inputs,
                        ...row.variables.map((env) => ({ env })),
                        ...row.hashes.map((hash) => hashed[hash]),
                        '^default',
                        join('{workspaceRoot}', member.workspace, '.python-version'),
                        join('{workspaceRoot}', member.lock),
                        ...Array.match(
                            stated.projects.filter((name) => name !== member.name),
                            { onEmpty: () => [], onNonEmpty: (projects) => [{ input: 'default', projects }] },
                        ),
                        { externalDependencies: [] },
                    ],
                    ...(stated.covered && { outputs: ['{options.outputs.0}'], configurations: { coverage: covered } }),
                    metadata: { description: row.description, help: { command: `uv run ${row.help}`, example: { args: [...row.example] } } },
                };
                return { verb: row.verb, key, target };
            }),
        ),
    );
    const locking: NonNullable<TargetConfiguration['inputs']> = [...member.manifests.map((file) => join('{workspaceRoot}', file)), join('{workspaceRoot}', member.lock), ...['uv.toml', '.python-version'].map((file) => join('{projectRoot}', file)), { externalDependencies: [] }];
    const upgrading = {
        command: 'uv lock --upgrade',
        options: { cwd: member.root },
        inputs: locking,
        metadata: { description: 'Moves every locked package to its newest allowed release with uv lock --upgrade', help: { command: 'uv lock --help', example: { args: ['--upgrade-package', '<package>'] } } },
    };
    const rooted: Verbs = root
        ? {
              check: {
                  uv: {
                      command: 'uv lock --check',
                      options: { cwd: member.root },
                      cache: true,
                      inputs: [...locking, UV, { env: 'UV_PREVIEW_FEATURES' }],
                      metadata: { description: 'Checks that uv.lock holds every change the manifests state with uv lock --check, writing nothing', help: { command: 'uv lock --help', example: { args: ['--offline'] } } },
                  },
              },
              upgrade: {
                  uv: upgrading,
                  ...(member.pixi && {
                      pixi: {
                          command: 'pixi update',
                          options: { cwd: member.root },
                          inputs: [...[PIXI, 'pyproject.toml', 'pixi.lock'].map((file) => join('{projectRoot}', file)), { externalDependencies: [] }],
                          metadata: { description: 'Moves every package of the Pixi environments to the newest release its requirement allows and rewrites pixi.lock with pixi update', help: { command: 'pixi update --help', example: { args: ['--dry-run'] } } },
                      },
                  }),
              },
          }
        : {};
    const tools: Verbs = pipe(
        Array.groupBy(checked, ({ verb }): string => verb),
        Record.map(Record.fromIterableWith(({ key, target }) => [key, target] as const)),
        Record.union(rooted, joined),
        Record.union(
            {
                build: {
                    ...(member.packaged && {
                        uv: {
                            command: line(['uv', 'build', '--package', member.name, '--no-sources', ...Option.match(distributions, { onNone: () => [], onSome: (at) => ['--out-dir', relative(member.workspace, at), '--clear', '--no-create-gitignore'] })]),
                            options: { cwd: member.workspace },
                            cache: true,
                            inputs: [...member.sources, ...built, UV, { externalDependencies: [] }],
                            outputs: Option.match(distributions, {
                                onNone: () => [...['.tar.gz', '-*.whl'].map((suffix) => join('{workspaceRoot}', folder, `${member.distribution}${suffix}`)), join('{workspaceRoot}', folder, '.gitignore')],
                                onSome: (at) => [join('{workspaceRoot}', at)],
                            }),
                            metadata: { description: 'Builds the member sdist and wheel with uv build', help: { command: 'uv build --help', example: { args: ['--wheel'] } } },
                        },
                    }),
                },
                install: {
                    ...(member.scripted && {
                        uv: {
                            command: `${line(['uv', 'export', '--locked', '--package', member.name, '--no-default-groups', '--no-emit-local', '--no-hashes', '--no-header', '--no-annotate', '--quiet'])} | ${line(['uv', 'tool', 'install', '--constraints', '-', ...member.locals.flatMap((name) => ['--reinstall-package', name]), ...flagged('--python', member.python), '.'])}`,
                            options: { cwd: member.root },
                            inputs: [...member.sources, ...built, join('{workspaceRoot}', member.lock), { externalDependencies: [] }],
                            metadata: {
                                description: "Installs the member scripts from the checkout into uv's tool folder at the versions uv.lock pins, on an interpreter its requires-python accepts, with uv export piped to uv tool install --constraints",
                                help: { command: 'uv tool install --help', example: { args: ['--editable'] } },
                            },
                        },
                    }),
                },
            },
            joined,
        ),
    );
    const removed = Record.collect(tools, (verb, held) => Record.keys(Record.filter(held, ({ outputs = [] }) => outputs.length > 0)).map((tool) => `${verb}--${tool}`)).flat();
    const verbs: Verbs = {
        ...tools,
        clean: Array.match(removed, {
            onEmpty: () => ({}),
            onNonEmpty: (labels) => ({
                uv: {
                    executor: `${distribution.name}:clean`,
                    options: { targets: labels },
                    inputs: ['default', '^default', join('{workspaceRoot}', member.lock), { externalDependencies: [] }],
                    metadata: { description: `Removes the outputs ${labels.join(', ')} declare`, help: { command: 'nx run --help', example: { options: { targets: labels } } } },
                },
            }),
        }),
        attach: {
            ...(member.packaged && {
                gh: {
                    executor: `${distribution.name}:attach`,
                    dependsOn: ['build--uv'],
                    inputs: [{ dependentTasksOutputFiles: '**/*' }, { externalDependencies: [] }],
                    metadata: { description: "Uploads the files build--uv wrote to the GitHub release of the version the project's manifest states with gh release upload", help: { command: 'gh release upload --help', example: { options: { dryRun: true } } } },
                },
            }),
        },
    };
    return {
        ...aggregated(verbs, manifest, 'python'),
        ...(member.published && {
            'nx-release-publish': {
                executor: `${distribution.name}:publish`,
                options: { publish: ['uv', 'publish', ...flagged('--index', member.index)], preview: ['--dry-run'], credential: 'UV_PUBLISH_TOKEN' },
                dependsOn: ['build--uv', '^nx-release-publish'],
                inputs: [{ dependentTasksOutputFiles: '**/*' }, { externalDependencies: [] }],
                metadata: { description: 'Publishes the distributions build--uv wrote with uv publish, which reads UV_PUBLISH_TOKEN', help: { command: 'uv publish --help', example: { options: { dryRun: true } } } },
            },
        }),
    };
};

// --- [COMPOSITION] ---------------------------------------------------------------------

const runtime = ManagedRuntime.make(Layer.merge(Linked.layer, layer));

const run = <A, E>(effect: Effect.Effect<A, E, Linked | NodeServices | Scope.Scope>) => runtime.runPromise(Effect.scoped(effect));

// --- [EXPORTS] -------------------------------------------------------------------------

export { ancestors, CHECKERS, CONFIGS, declared, documents, Linked, manifested, Options, PIXI, PLUGIN, run, spawned, targets };
