import { posix } from 'node:path';
import { fileURLToPath } from 'node:url';
import { type RawProjectGraphDependency, readJsonFile, type TargetConfiguration } from '@nx/devkit';
import { type PackageJson, quoteShellArg } from '@nx/devkit/internal';
import { above, aggregated, PLATFORM, runner, type Verbs } from '@rasm/nx-distribution/inference';
import distribution from '@rasm/nx-distribution/package.json' with { type: 'json' };
import { Array, Context, identity, Layer, Option, pipe, Record, Schema, SchemaTransformation, String, Struct, UndefinedOr } from 'effect';
import type { Member } from '#/workspace';

// --- [CONSTANTS] -----------------------------------------------------------------------

const MYPY = [['mypy.ini'], ['.mypy.ini'], ['pyproject.toml', 'tool', 'mypy'], ['setup.cfg', 'mypy']] as const;
const CHECKERS = {
    ty: {
        verb: 'check',
        variables: ['TY_UV'],
        hashes: ['platform'],
        imports: true,
        plugins: undefined,
        configs: { cwd: [['ty.toml'], ['pyproject.toml', 'tool', 'ty']], root: [] },
        matches: ['*.py', '*.pyi'],
        sources: ['*.py', '*.pyi'],
        folder: identity<string>,
        isolated: false,
        command: (at: string) => ['ty', 'check', at],
        coverage: undefined,
        description: 'Type-checks the member with ty check',
        help: 'ty check --help',
        example: ['--output-format', 'concise'],
    },
    mypy: {
        verb: 'check',
        variables: [],
        hashes: ['platform', 'python'],
        imports: true,
        plugins: undefined,
        configs: { cwd: MYPY, root: [] },
        matches: ['*.py', '*.pyi'],
        sources: ['*.py', '*.pyi'],
        folder: (_: string, base: string) => base,
        isolated: false,
        command: (at: string, _config: string | undefined, version: string | undefined, cache: string) => ['mypy', at, ...flagged('--python-version', version), '--cache-dir', cache],
        coverage: undefined,
        description: 'Type-checks the member with mypy and the plugins its configuration loads',
        help: 'mypy --help',
        example: ['--output', 'json'],
    },
    refurb: {
        verb: 'check',
        variables: [],
        hashes: ['platform', 'python'],
        imports: true,
        plugins: 'refurb.plugins',
        configs: { cwd: MYPY, root: [['pyproject.toml', 'tool', 'refurb']] },
        matches: ['*.py', '*.pyi'],
        sources: ['*.py', '*.pyi'],
        folder: (_: string, base: string) => base,
        isolated: false,
        command: (at: string, config?: string, version?: string) => ['refurb', ...(config === undefined ? [] : ['--config-file', config]), ...flagged('--python-version', version), at],
        coverage: undefined,
        description: 'Reports the findings of the member with refurb over a mypy build',
        help: 'refurb --help',
        example: ['--format', 'github'],
    },
    vulture: {
        verb: 'check',
        variables: [],
        hashes: [],
        imports: false,
        plugins: undefined,
        configs: { cwd: [], root: [['pyproject.toml', 'tool', 'vulture']] },
        matches: ['*.py'],
        sources: ['*.py'],
        folder: identity<string>,
        isolated: false,
        command: (at: string, config?: string) => ['vulture', at, ...(config === undefined ? [] : ['--config', config])],
        coverage: undefined,
        description: 'Reports the functions, classes, methods, and attributes of the member that nothing reads with vulture',
        help: 'vulture --help',
        example: ['--min-confidence', '80'],
    },
    pytest: {
        verb: 'test',
        variables: ['PYTEST_ADDOPTS', 'PYTEST_PLUGINS', 'PYTEST_DISABLE_PLUGIN_AUTOLOAD'],
        hashes: ['platform', 'python'],
        imports: true,
        plugins: undefined,
        configs: { cwd: [], root: [['pytest.toml', 'pytest'], ['.pytest.toml', 'pytest'], ['pytest.ini'], ['.pytest.ini'], ['pyproject.toml', 'tool', 'pytest'], ['tox.ini', 'pytest'], ['setup.cfg', 'tool:pytest']] },
        matches: ['test_*.py', '*_test.py'],
        sources: ['*'],
        isolated: true,
        folder: identity<string>,
        command: (at: string) => ['pytest', at],
        coverage: 'pytest-cov',
        description: 'Runs the member tests with pytest',
        help: 'pytest --help',
        example: ['-k', '<expression>'],
    },
} as const;
const PIXI = 'pixi.toml';
const PLUGIN = readJsonFile<PackageJson>(fileURLToPath(import.meta.resolve('#/package.json'))).name;
const UV = { version: { runtime: 'uv self version --short' }, settings: 'uv.toml', pins: ['.python-version', '.python-versions'] } as const;
const CONFIGS = Array.dedupe([...Object.values(CHECKERS).flatMap(({ configs }) => [...configs.cwd, ...configs.root].map(([file]) => file)), PIXI]);

// --- [MODELS] --------------------------------------------------------------------------

class Options extends Schema.Opaque<Options, unknown>()(
    Schema.Struct({ outputPath: Schema.optionalKey(Schema.String), coveragePath: Schema.optionalKey(Schema.String), environmentPath: Schema.optionalKey(Schema.String) }).annotate({
        description:
            "outputPath: workspace folder uv build writes a member's distributions to, {projectName} naming the member, absent leaves uv its own dist folder of the uv workspace. coveragePath: workspace folder the coverage configuration of pytest writes its lcov.info report to through --cov-report=lcov:<path> and its .coverage data file to through COVERAGE_FILE, {projectName} naming the member, absent writes both to the folder pytest runs from. environmentPath: workspace folder holding the environment pytest runs in through UV_PROJECT_ENVIRONMENT, which uv run --exact keeps at the member's locked selection between runs, {projectName} naming the member, absent builds a temporary environment per run through uv run --isolated",
    }),
) {}
const Table = Schema.Record(Schema.String, Schema.Unknown);
const Patterns = Schema.Struct({
    python_files: Schema.optionalKey(
        Schema.Union([Schema.Array(Schema.String), Schema.String.pipe(Schema.decodeTo(Schema.Array(Schema.String), SchemaTransformation.transform({ decode: (text): readonly string[] => text.split(/\s+/u).filter(String.isNonEmpty), encode: (files: readonly string[]) => files.join(' ') })))]),
    ),
});
const Settings = Patterns.mapFields(Struct.assign({ ini_options: Schema.optionalKey(Patterns), pytest: Schema.optionalKey(Patterns), cache_dir: Schema.optionalKey(Schema.String) }));

// --- [SERVICES] ------------------------------------------------------------------------

class Linked extends Context.Service<Linked, Map<string, readonly RawProjectGraphDependency[]>>()(`${PLUGIN}/targets/Linked`) {
    static readonly layer = Layer.sync(Linked, () => new Map());
}

// --- [OPERATIONS] ----------------------------------------------------------------------

const flagged = (flag: string, value: string | undefined): readonly string[] => (value === undefined ? [] : [flag, value]);

const declared = (documented: ReadonlyMap<string, unknown>, folders: { readonly cwd: string; readonly root: string }, checker: keyof typeof CHECKERS) => {
    const configs: Readonly<Record<keyof typeof folders, readonly (readonly [file: string, ...keys: string[]])[]>> = CHECKERS[checker].configs;
    const searched = Record.map(configs, (files, start) => {
        const candidates = above(folders[start]).flatMap((folder) => files.map(([file, ...keys]) => [posix.join(folder, file), keys] as const));
        const found = Array.findFirstWithIndex(candidates, ([at, keys]) =>
            Option.map(
                Option.flatMap(
                    keys.reduce<Option.Option<unknown>>((table, key) => table.pipe(Option.flatMap(Schema.decodeUnknownOption(Table)), Option.flatMap(Record.get(key))), Option.fromUndefinedOr(documented.get(at))),
                    Schema.decodeUnknownOption(Settings),
                ),
                (decoded) => [at, decoded] as const,
            ),
        );
        return { found, read: candidates.slice(0, Option.match(found, { onNone: () => candidates.length, onSome: ([, index]) => index + 1 })) };
    });
    const found = Option.orElse(searched.root.found, () => searched.cwd.found);
    const settings = Option.getOrUndefined(Option.map(found, ([[, stated]]) => stated));
    return {
        config: Option.getOrUndefined(Option.map(found, ([[config]]) => config)),
        cache: settings?.cache_dir,
        inputs: Array.dedupe([...searched.root.read, ...searched.cwd.read].map(([at]) => posix.join('{workspaceRoot}', at))),
        patterns: (settings?.python_files ?? settings?.ini_options?.python_files ?? settings?.pytest?.python_files ?? CHECKERS[checker].matches).map((pattern) => posix.join(folders.root, '**', pattern)),
    };
};

const line = (argv: readonly string[]) => argv.map(quoteShellArg).join(' ');

const targets = (member: Member, { outputPath, coveragePath, environmentPath }: Options, manifest: Readonly<Record<string, Pick<TargetConfiguration, 'inputs'>>>, workspaceRoot: string): Record<string, TargetConfiguration> => {
    const distributions = Option.map(Option.fromUndefinedOr(outputPath), (template) => template.replaceAll('{projectName}', member.name));
    const folder = Option.getOrElse(distributions, () => posix.join(member.workspace, 'dist'));
    const environed = Option.map(Option.fromUndefinedOr(environmentPath), (template) => template.replaceAll('{projectName}', member.name));
    const built = [posix.relative(member.root, folder)].filter((held) => !held.startsWith('..')).map((held) => `!${posix.join('{projectRoot}', held, '**', '*')}`);
    const hashed = { platform: PLATFORM, python: { runtime: line(['uv', 'python', 'find', '--show-version', '--directory', member.workspace]) } };
    const project = [...[...UV.pins, UV.settings].map((file) => posix.join('{workspaceRoot}', member.workspace, file)), posix.join('{workspaceRoot}', member.lock)];
    const checked = member.checks.map((stated) => {
        const row = CHECKERS[stated.key];
        const { cwd } = stated;
        const outside = posix.relative(member.workspace, cwd).startsWith('..');
        const config = UndefinedOr.map(stated.config, (file) => posix.relative(cwd, file));
        const environment = Option.filter(environed, () => row.isolated);
        const argv = [
            'uv',
            'run',
            '--no-active',
            '--locked',
            ...(row.isolated ? [Option.isSome(environment) ? '--exact' : '--isolated'] : []),
            ...(outside ? ['--project', posix.relative(cwd, member.workspace)] : []),
            '--package',
            member.name,
            '--all-extras',
            ...flagged('--group', stated.group),
            ...row.command(
                posix.relative(cwd, member.root) || '.',
                config,
                UndefinedOr.map(member.python, (range) => /(?:>=|==|~=)\s*(?<floor>\d+\.\d+)/u.exec(range)?.groups?.['floor']),
                posix.join((stated.cache ?? '.mypy_cache').replace('$MYPY_CONFIG_FILE_DIR', posix.dirname(config ?? '.')), member.root, stated.key),
            ),
        ];
        const reported = coveragePath ?? cwd;
        const selected = Option.match(environment, { onNone: () => ({}), onSome: (at) => ({ UV_PROJECT_ENVIRONMENT: posix.relative(cwd, at) }) });
        const covered = {
            command: line([...argv, '--cov', `--cov-report=lcov:${posix.join(posix.relative(cwd, reported), 'lcov.info')}`]),
            env: { ...selected, COVERAGE_FILE: posix.join(posix.relative(cwd, reported), '.coverage') },
            outputs: [posix.join('{workspaceRoot}', reported, '{lcov.info,.coverage}')],
        };
        const target: TargetConfiguration = {
            command: line(argv),
            options: { cwd, ...(Option.isSome(environment) && { env: selected }) },
            cache: true,
            inputs: [
                ...row.sources.map((glob) => posix.join('{projectRoot}', '**', glob)),
                ...built,
                ...stated.inputs,
                ...row.variables.map((env) => ({ env })),
                ...row.hashes.map((hash) => hashed[hash]),
                ...(row.imports ? ['^default'] : []),
                ...project,
                ...Array.match(row.imports ? stated.projects.filter((name) => name !== member.name) : [], {
                    onEmpty: () => [],
                    onNonEmpty: (projects) => [
                        { input: 'default', projects },
                        { input: 'default', projects, dependencies: true },
                    ],
                }),
                { externalDependencies: [] },
            ],
            ...(stated.covered && { outputs: ['{options.outputs.0}'], configurations: { coverage: covered } }),
            metadata: { description: row.description, help: { command: `uv run ${row.help}`, example: { args: [...row.example] } } },
        };
        return { verb: row.verb, key: stated.key, target };
    });
    const locking: NonNullable<TargetConfiguration['inputs']> = [...member.manifests, ...project, { externalDependencies: [] }];
    const rooted: Verbs =
        member.root === member.workspace
            ? {
                  check: {
                      uv: {
                          command: 'uv lock --check',
                          options: { cwd: member.root },
                          cache: true,
                          inputs: [...locking, UV.version, { env: 'UV_PREVIEW_FEATURES' }],
                          metadata: { description: 'Checks that uv.lock holds every change the manifests state with uv lock --check, writing nothing', help: { command: 'uv lock --help', example: { args: ['--offline'] } } },
                      },
                  },
                  upgrade: {
                      uv: {
                          executor: `${distribution.name}:upgrade`,
                          options: { cwd: member.root, commands: [{ apply: ['uv', 'lock', '--upgrade'], preview: ['uv', 'lock', '--upgrade', '--dry-run'] }, { apply: ['uv', 'cache', 'prune', '--force'] }] },
                          inputs: locking,
                          metadata: {
                              description: 'Moves every locked package to its newest allowed release with uv lock --upgrade, then removes dangling cache entries with uv cache prune --force, as a running uvx holds the cache lock for its whole run',
                              help: { command: 'uv lock --help', example: { options: { dryRun: true } } },
                          },
                      },
                      ...(member.pixi && {
                          pixi: {
                              executor: `${distribution.name}:upgrade`,
                              options: { cwd: member.root, commands: [{ apply: ['pixi', 'update'], preview: ['pixi', 'update', '--dry-run'] }] },
                              inputs: [...[PIXI, 'pyproject.toml', 'pixi.lock'].map((file) => posix.join('{projectRoot}', file)), { externalDependencies: [] }],
                              metadata: { description: 'Moves every package of the Pixi environments to the newest release its requirement allows and rewrites pixi.lock with pixi update', help: { command: 'pixi update --help', example: { options: { dryRun: true } } } },
                          },
                      }),
                  },
              }
            : {};
    const tools: Verbs = pipe(
        Array.groupBy(checked, ({ verb }): string => verb),
        Record.map(Record.fromIterableWith(({ key, target }) => [key, target] as const)),
        Record.union(rooted, Struct.assign<Verbs[string], Verbs[string]>),
        Record.union(
            {
                build: {
                    ...(member.packaged && {
                        uv: {
                            command: line(['uv', 'build', '--package', member.name, '--no-sources', ...Option.match(distributions, { onNone: () => [], onSome: (at) => ['--out-dir', posix.relative(member.workspace, at), '--clear', '--no-create-gitignore'] })]),
                            options: { cwd: member.workspace },
                            cache: true,
                            inputs: [...member.sources, ...built, UV.version, { externalDependencies: [] }],
                            outputs: Option.match(distributions, {
                                onNone: () => [...['.tar.gz', '-*.whl'].map((suffix) => posix.join('{workspaceRoot}', folder, `${member.distribution}${suffix}`)), posix.join('{workspaceRoot}', folder, '.gitignore')],
                                onSome: (at) => [posix.join('{workspaceRoot}', at)],
                            }),
                            metadata: { description: 'Builds the member sdist and wheel with uv build', help: { command: 'uv build --help', example: { args: ['--wheel'] } } },
                        },
                    }),
                },
                install: {
                    ...(member.scripted && {
                        uv: {
                            command: `${line(['uv', 'export', '--locked', '--package', member.name, '--no-default-groups', '--no-emit-local', '--no-hashes', '--quiet'])} | ${line(['uv', 'tool', 'install', '--constraints', '-', ...member.locals.flatMap((name) => ['--reinstall-package', name]), ...flagged('--python', member.python), '.'])}`,
                            options: { cwd: member.root },
                            inputs: [...member.sources, ...built, posix.join('{workspaceRoot}', member.lock), { externalDependencies: [] }],
                            metadata: {
                                description: "Installs the member scripts from the checkout into uv's tool folder at the versions uv.lock pins, on an interpreter its requires-python accepts, with uv export piped to uv tool install --constraints",
                                help: { command: 'uv tool install --help', example: { args: ['--editable'] } },
                            },
                        },
                    }),
                },
            },
            Struct.assign<Verbs[string], Verbs[string]>,
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
                    inputs: ['default', '^default', posix.join('{workspaceRoot}', member.lock), { externalDependencies: [] }],
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
                    metadata: {
                        description: "Uploads the files build--uv wrote to the GitHub release of the version the project's manifest states with gh release upload, creating that release with gh release create where none exists",
                        help: { command: 'gh release upload --help', example: { options: { dryRun: true } } },
                    },
                },
            }),
        },
    };
    return aggregated(verbs, manifest, 'python', workspaceRoot, {
        ...(member.published && {
            'nx-release-publish': {
                executor: `${distribution.name}:publish`,
                options: { publish: ['uv', 'publish', ...flagged('--index', member.index)], preview: ['--dry-run'], credential: ['ACTIONS_ID_TOKEN_REQUEST_URL', 'UV_PUBLISH_TOKEN'] },
                dependsOn: ['build--uv', '^nx-release-publish'],
                inputs: [{ dependentTasksOutputFiles: '**/*' }, { externalDependencies: [] }],
                metadata: { description: 'Publishes the distributions build--uv wrote with uv publish, through PyPI trusted publishing where GitHub Actions grants an OIDC token, else UV_PUBLISH_TOKEN', help: { command: 'uv publish --help', example: { options: { dryRun: true } } } },
            },
        }),
    });
};

// --- [COMPOSITION] ---------------------------------------------------------------------

const run = runner(Linked.layer);

// --- [EXPORTS] -------------------------------------------------------------------------

export { CHECKERS, CONFIGS, declared, Linked, Options, PIXI, PLUGIN, run, targets, UV };
