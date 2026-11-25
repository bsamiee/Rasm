import { posix } from 'node:path';
import { fileURLToPath } from 'node:url';
import { readJsonFile, type TargetConfiguration } from '@nx/devkit';
import { type PackageJson, quoteShellArg } from '@nx/devkit/internal';
import { type Anchor, above, aggregated, EXECUTORS, PLATFORM, type Stated, type Verbs } from '@rasm/nx';
import { Array, identity, Option, pipe, Record, Schema, Struct, UndefinedOr } from 'effect';
import type { Member } from '#/workspace';

// --- [TYPES] ---------------------------------------------------------------------------

type Flags = Readonly<Record<'config' | 'floor' | 'cache' | 'store', string | undefined>>;

// --- [CONSTANTS] -----------------------------------------------------------------------

const MYPY = ['mypy.ini', '.mypy.ini', 'pyproject.toml', 'setup.cfg'] as const;
const PYTEST = { 'pytest.toml': ['pytest'], '.pytest.toml': ['pytest'], 'pytest.ini': [], '.pytest.ini': [], 'pyproject.toml': ['tool', 'pytest'], 'tox.ini': ['pytest'], 'setup.cfg': ['tool:pytest'] } as const;
const FIXED: Readonly<Record<string, readonly string[]>> = {};
const CHECKERS = {
    ty: {
        verb: 'check',
        once: true,
        variables: ['TY_UV', 'TY_CONFIG_FILE'],
        plugins: undefined,
        configs: ['ty.toml', 'pyproject.toml'],
        settings: FIXED,
        table: undefined,
        matches: ['*.py', '*.pyi'],
        sources: ['*.py', '*.pyi'],
        folder: identity<string>,
        command: (paths: readonly string[]) => ['ty', 'check', ...paths],
        coverage: undefined,
        description: 'Type-checks every member of the uv workspace in one ty check run',
        help: 'ty check --help',
    },
    mypy: {
        verb: 'check',
        once: false,
        variables: ['MYPYPATH'],
        plugins: undefined,
        configs: MYPY,
        settings: FIXED,
        table: undefined,
        matches: ['*.py', '*.pyi'],
        sources: ['*.py', '*.pyi'],
        folder: (_: string, base: string) => base,
        command: (paths: readonly string[], { floor, cache }: Flags) => ['mypy', ...paths, ...flagged('--python-version', floor), ...flagged('--cache-dir', cache)],
        coverage: undefined,
        description: 'Type-checks the member with mypy and the plugins its configuration loads',
        help: 'mypy --help',
    },
    refurb: {
        verb: 'check',
        once: false,
        variables: ['MYPYPATH'],
        plugins: 'refurb.plugins',
        configs: MYPY,
        settings: FIXED,
        table: 'refurb',
        matches: ['*.py', '*.pyi'],
        sources: ['*.py', '*.pyi'],
        folder: (_: string, base: string) => base,
        command: (paths: readonly string[], { config, floor, cache }: Flags) => ['refurb', ...flagged('--config-file', config), ...flagged('--python-version', floor), ...paths, ...(cache === undefined ? [] : ['--', '--cache-dir', cache])],
        coverage: undefined,
        description: 'Reports the findings of the member with refurb over a mypy build',
        help: 'refurb --help',
    },
    pytest: {
        verb: 'test',
        once: false,
        variables: ['PYTEST_ADDOPTS', 'PYTEST_PLUGINS', 'PYTEST_DISABLE_PLUGIN_AUTOLOAD', 'COVERAGE_RCFILE'],
        plugins: undefined,
        configs: [...Record.keys(PYTEST), 'conftest.py', '.coveragerc', '.coveragerc.toml'],
        settings: PYTEST,
        table: 'coverage',
        matches: ['test_*.py', '*_test.py'],
        sources: ['*'],
        folder: identity<string>,
        command: (paths: readonly string[], { store }: Flags) => [
            'pytest',
            ...paths,
            ...flagged(
                '-o',
                UndefinedOr.map(store, (folder) => `cache_dir=${folder}`),
            ),
        ],
        coverage: 'pytest-cov',
        description: 'Runs the member tests with pytest',
        help: 'pytest --help',
    },
} as const;
const PLUGIN = readJsonFile<PackageJson>(fileURLToPath(import.meta.resolve('#/package.json'))).name;
const UV = { version: { runtime: 'uv self version --short' }, settings: 'uv.toml', pins: ['.python-version', '.python-versions'] } as const;
const CONFIGS = Record.keys(PYTEST);

// --- [MODELS] --------------------------------------------------------------------------

class Options extends Schema.Opaque<Options, unknown>()(
    Schema.Struct({ outputPath: Schema.optionalKey(Schema.String), coveragePath: Schema.optionalKey(Schema.String), environmentPath: Schema.optionalKey(Schema.String) }).annotate({
        description:
            "outputPath: workspace folder uv build writes a member's distributions to, {projectName} naming the project, absent leaves uv its own dist folder of the uv workspace. coveragePath: workspace folder the coverage configuration of pytest writes its lcov.info report to through --cov-report=lcov:<path> and its .coverage data file to through COVERAGE_FILE, {projectName} naming the project, absent writes both to the folder pytest runs from. environmentPath: workspace folder holding what the checkers and tests of a project keep between runs, {projectName} naming the project: one environment per dependency group offering a tool, in a folder named for the group, or .venv where the project's own dependencies hold the tool, each selected through UV_PROJECT_ENVIRONMENT and kept at its locked selection by uv run --exact, beside the cache folder of each tool (.mypy_cache, .refurb_cache, .pytest_cache), absent builds a temporary environment per run through uv run --isolated and leaves each tool its own cache folder. The tasks of a uv root select every package of its workspace (--all-packages), so a uv root that is itself a checked member checks in that wider environment",
    }),
) {}

// --- [OPERATIONS] ----------------------------------------------------------------------

const flagged = (flag: string, value: string | undefined): readonly string[] => (value === undefined ? [] : [flag, value]);

const line = (argv: readonly string[]) => argv.map(quoteShellArg).join(' ');

const targets = (member: Member, { outputPath, coveragePath, environmentPath }: Options, manifest: Stated, anchor: Anchor): Record<string, TargetConfiguration> => {
    const distributions = Option.fromUndefinedOr(outputPath);
    const folder = Option.getOrElse(distributions, () => posix.join(member.workspace, 'dist'));
    const environed = Option.fromUndefinedOr(environmentPath);
    const built = [posix.relative(member.root, folder)].filter((held) => !held.startsWith('..')).map((held) => `!${posix.join('{projectRoot}', held, '**', '*')}`);
    const settings = [...UV.pins, UV.settings].map((file) => posix.join('{workspaceRoot}', member.workspace, file));
    const project = [...settings, posix.join('{workspaceRoot}', member.lock)];
    const checked = member.checks.map((stated) => {
        const row = CHECKERS[stated.key];
        const { cwd } = stated;
        const outside = posix.relative(member.workspace, cwd).startsWith('..');
        const config = UndefinedOr.map(stated.config, (file) => posix.relative(cwd, file));
        const [environment, kept] = [Option.map(environed, (at) => posix.join(at, stated.group ?? '.venv')), Option.map(environed, (at) => posix.join(at, `.${stated.key}_cache`))];
        const argv = [
            'uv',
            'run',
            '--frozen',
            '--no-active',
            Option.isSome(environment) ? '--exact' : '--isolated',
            ...(outside ? ['--project', posix.relative(cwd, member.workspace)] : []),
            ...member.selection,
            '--all-extras',
            ...flagged('--group', stated.group),
            ...row.command(
                stated.paths.map((root) => posix.relative(cwd, root) || '.'),
                {
                    config,
                    floor: member.floor,
                    cache: Option.getOrUndefined(Option.map(kept, (at) => posix.relative(cwd, at))),
                    store: Option.getOrUndefined(Option.map(kept, (at) => posix.join('$NX_WORKSPACE_ROOT', at))),
                },
            ),
        ];
        const reported = coveragePath ?? cwd;
        const selected = Option.match(environment, { onNone: () => ({}), onSome: (at) => ({ UV_PROJECT_ENVIRONMENT: posix.relative(member.workspace, at) }) });
        const covered = {
            command: line([...argv, '--cov', ...flagged('--cov-config', config), `--cov-report=lcov:${posix.join(posix.relative(cwd, reported), 'lcov.info')}`]),
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
                ...Array.cartesianWith(Array.dedupe([cwd, ...stated.paths].flatMap(above)), row.configs, (at, file) => posix.join('{workspaceRoot}', at, file)),
                ...row.variables.map((env) => ({ env })),
                PLATFORM,
                { runtime: line(['uv', 'python', 'find', '--show-version', '--directory', member.workspace]) },
                '^default',
                ...project,
                ...Array.match(
                    stated.projects.filter((name) => name !== member.name),
                    {
                        onEmpty: () => [],
                        onNonEmpty: (projects) => [
                            { input: 'default', projects },
                            { input: 'default', projects, dependencies: true },
                        ],
                    },
                ),
                { externalDependencies: [] },
            ],
            ...(stated.covered && { outputs: ['{options.outputs.0}'], configurations: { coverage: covered } }),
            metadata: { description: row.description, help: { command: `uv run ${row.help}`, example: {} } },
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
                          executor: EXECUTORS.upgrade,
                          options: { cwd: member.root, commands: [{ apply: ['uv', 'lock', '--upgrade'], preview: ['uv', 'lock', '--upgrade', '--dry-run'] }, { apply: ['uv', 'cache', 'prune', '--force'] }] },
                          inputs: locking,
                          metadata: {
                              description: 'Moves every locked package to its newest allowed release with uv lock --upgrade, then removes dangling cache entries with uv cache prune --force, as a running uvx holds the cache lock for its whole run',
                              help: { command: 'uv lock --help', example: { options: { dryRun: true } } },
                          },
                      },
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
                            inputs: ['default', ...built, posix.join('{workspaceRoot}', member.workspace, 'pyproject.toml'), ...settings, UV.version, { externalDependencies: [] }],
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
                            inputs: ['default', ...built, ...project, { externalDependencies: [] }],
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
                    executor: EXECUTORS.clean,
                    options: { targets: labels },
                    inputs: ['default', '^default', posix.join('{workspaceRoot}', member.lock), { externalDependencies: [] }],
                    metadata: { description: `Removes the outputs ${labels.join(', ')} declare`, help: { command: 'nx run --help', example: { options: { targets: labels } } } },
                },
            }),
        }),
        attach: {
            ...(member.packaged && {
                gh: {
                    executor: EXECUTORS.attach,
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
    return aggregated(verbs, manifest, 'python', anchor, {
        ...(member.published && {
            'nx-release-publish': {
                executor: EXECUTORS.publish,
                options: { publish: ['uv', 'publish', ...flagged('--index', member.index)], preview: ['--dry-run'], credential: ['ACTIONS_ID_TOKEN_REQUEST_URL', 'UV_PUBLISH_TOKEN'] },
                dependsOn: ['build--uv', '^nx-release-publish'],
                inputs: [{ dependentTasksOutputFiles: '**/*' }, { externalDependencies: [] }],
                metadata: { description: 'Publishes the distributions build--uv wrote with uv publish, through PyPI trusted publishing where GitHub Actions grants an OIDC token, else UV_PUBLISH_TOKEN', help: { command: 'uv publish --help', example: { options: { dryRun: true } } } },
            },
        }),
    });
};

// --- [EXPORTS] -------------------------------------------------------------------------

export { CHECKERS, CONFIGS, Options, PLUGIN, targets, UV };
