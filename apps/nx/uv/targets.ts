import { dirname, extname, join, relative } from 'node:path';
import type { TargetConfiguration } from '@nx/devkit';
import { quoteShellArg } from '@nx/devkit/internal';
import { Array, Cause, Effect, FileSystem, identity, Option, Record, Result, Schema, SchemaTransformation, String, Struct, UndefinedOr } from 'effect';
import { Ini, Toml } from 'effect/encoding';
import type { Member } from '#/workspace';

// --- [TYPES] ---------------------------------------------------------------------------

type Checker = keyof typeof CHECKERS;

// --- [CONSTANTS] -----------------------------------------------------------------------

const CHECKERS = {
    ty: {
        verb: 'check',
        variables: ['TY_UV'],
        files: [['ty.toml'], ['pyproject.toml', 'tool', 'ty']],
        matches: ['*.py', '*.pyi'],
        sources: ['*.py', '*.pyi'],
        folder: identity<string>,
        run: [],
        command: (at: string) => ['ty', 'check', at],
        mypy: undefined,
        coverage: undefined,
        description: 'Type-checks the member with ty',
        help: 'ty check --help',
        example: ['--output-format', 'concise'],
    },
    mypy: {
        verb: 'check',
        variables: [],
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
        files: [['pyproject.toml', 'tool', 'refurb']],
        matches: ['*.py', '*.pyi'],
        sources: ['*.py', '*.pyi'],
        folder: (_: string, base: string) => base,
        run: [],
        command: (at: string, config?: string) => ['refurb', ...(config === undefined ? [] : ['--config-file', config]), at],
        mypy: ['--'],
        coverage: undefined,
        description: 'Reports refurb findings over a mypy build of the member',
        help: 'refurb --help',
        example: ['--format', 'github'],
    },
    vulture: {
        verb: 'check',
        variables: [],
        files: [['pyproject.toml', 'tool', 'vulture']],
        matches: ['*.py'],
        sources: ['*.py'],
        folder: identity<string>,
        run: [],
        command: (at: string, config?: string) => ['vulture', at, ...(config === undefined ? [] : ['--config', config])],
        mypy: undefined,
        coverage: undefined,
        description: 'Reports the functions, classes, methods, and attributes of the member that nothing reads',
        help: 'vulture --help',
        example: ['--min-confidence', '80'],
    },
    pytest: {
        verb: 'test',
        variables: [],
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
const SHARED = '@rasm/nx-distribution';
const CONFIGS = Array.dedupe([...Object.values(CHECKERS).flatMap(({ files }) => files.map(([file]) => file)), PIXI]);
const SEPARATOR = /\s+/u;

// --- [MODELS] --------------------------------------------------------------------------

const Table = Schema.Record(Schema.String, Schema.Unknown);
const Patterns = Schema.Struct({
    python_files: Schema.optionalKey(
        Schema.Union([Schema.Array(Schema.String), Schema.String.pipe(Schema.decodeTo(Schema.Array(Schema.String), SchemaTransformation.transform({ decode: (text): readonly string[] => text.split(SEPARATOR).filter(String.isNonEmpty), encode: (files: readonly string[]) => files.join(' ') })))]),
    ),
});
const Settings = Patterns.mapFields(Struct.assign({ ini_options: Schema.optionalKey(Patterns), pytest: Schema.optionalKey(Patterns), cache_dir: Schema.optionalKey(Schema.String) }));

// --- [OPERATIONS] ----------------------------------------------------------------------

const flagged = (flag: string, value: string | undefined): readonly string[] => (value === undefined ? [] : [flag, value]);

const refused = (subject: string, detail: string) => (cause?: unknown) => new Cause.UnknownError(cause, `${subject}: ${detail}`);

const ancestors = (folder: string): readonly string[] => (folder === '.' ? [folder] : [folder, ...ancestors(dirname(folder))]);

const documents = Effect.fnUntraced(function* (workspaceRoot: string, roots: readonly string[]) {
    const fs = yield* FileSystem.FileSystem;
    const present = yield* Effect.filter(Array.cartesianWith(Array.dedupe(roots.flatMap(ancestors)), CONFIGS, join), (at) => fs.exists(join(workspaceRoot, at)), { concurrency: 'unbounded' });
    const read = yield* Effect.forEach(present, (at) => Effect.map(fs.readFileString(join(workspaceRoot, at)), (text) => [at, extname(at) === '.toml' ? Toml.parse(text) : Result.succeed<unknown>(Ini.parse(text))] as const), { concurrency: 'unbounded' });
    return {
        documented: new Map(read.map(([at, parsed]) => [at, Result.getOrElse(parsed, Record.empty)] as const)),
        unparsed: Array.filterMap(read, ([at, parsed]) => Result.map(Result.flip(parsed), refused(at, 'does not parse'))),
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

const targets = (member: Member, outputPath: Option.Option<string>): Record<string, TargetConfiguration> => {
    const line = (argv: readonly string[]) => argv.map(quoteShellArg).join(' ');
    const root = member.root === member.workspace;
    const distributions = Option.match(outputPath, { onNone: () => join(member.workspace, 'dist', member.name), onSome: (at) => join(at, 'uv', member.name) });
    const built = [relative(member.root, distributions)].filter((folder) => !folder.startsWith('..')).map((folder) => `!${join('{projectRoot}', folder, '**', '*')}`);
    const coverage = Option.map(outputPath, (at) => join(at, 'coverage', member.name));
    const checked = Array.getSomes(
        Record.collect(CHECKERS, (key, row) =>
            Option.gen(function* () {
                const stated = yield* Option.filter(Option.fromUndefinedOr(member.checks[key]), ({ locked }) => locked && !member.nested);
                const folder = row.folder(member.workspace, yield* Option.fromUndefinedOr(stated.base));
                const { cache = '.mypy_cache', config } = Option.getOrElse(Record.get(member.checks, 'mypy'), () => stated);
                const argv = [
                    'uv',
                    'run',
                    '--no-active',
                    '--locked',
                    ...row.run,
                    ...(relative(member.workspace, folder).startsWith('..') ? ['--project', relative(folder, member.workspace)] : []),
                    '--package',
                    member.name,
                    '--all-extras',
                    ...flagged('--group', stated.group),
                    ...row.command(
                        relative(folder, member.root) || '.',
                        UndefinedOr.map(stated.config, (file) => relative(folder, file)),
                    ),
                    ...(row.mypy === undefined ? [] : [...row.mypy, '--cache-dir', join(cache.replace('$MYPY_CONFIG_FILE_DIR', relative(folder, config === undefined ? folder : dirname(config)) || '.'), member.root, key)]),
                ];
                const target: TargetConfiguration = {
                    command: line(argv),
                    options: { cwd: folder },
                    cache: true,
                    inputs: [
                        ...row.sources.map((glob) => join('{projectRoot}', '**', glob)),
                        ...built,
                        ...stated.inputs,
                        ...row.variables.map((env) => ({ env })),
                        '^default',
                        join('{workspaceRoot}', member.workspace, '.python-version'),
                        join('{workspaceRoot}', member.lock),
                        ...Array.match(
                            stated.projects.filter((name) => name !== member.name),
                            { onEmpty: () => [], onNonEmpty: (projects) => [{ input: 'default', projects }] },
                        ),
                        { externalDependencies: [] },
                    ],
                    ...Option.match(
                        Option.filter(coverage, () => stated.covered),
                        {
                            onNone: () => ({}),
                            onSome: (at) => ({
                                outputs: [join('{workspaceRoot}', at)],
                                configurations: { coverage: { command: line([...argv, '--cov', `--cov-report=lcov:${join(relative(folder, at), 'coverage.lcov')}`]), env: { COVERAGE_FILE: join(relative(folder, at), '.coverage') } } },
                            }),
                        },
                    ),
                    metadata: { description: row.description, help: { command: `uv run ${row.help}`, example: { args: [...row.example] } } },
                };
                return [`${row.verb}--${key}`, target] as const;
            }),
        ),
    );
    const published = ['uv', 'publish', ...flagged('--index', member.index)];
    const tools: Record<string, TargetConfiguration> = {
        ...Record.fromEntries(checked),
        ...(root && {
            'check--uv': {
                command: 'uv lock --check',
                options: { cwd: member.root },
                cache: true,
                inputs: [...member.manifests.map((file) => join('{workspaceRoot}', file)), join('{workspaceRoot}', member.lock), ...['uv.toml', '.python-version'].map((file) => join('{projectRoot}', file)), { externalDependencies: [] }],
                metadata: { description: 'Resolves the workspace and fails where uv.lock lacks a change the manifests state, without writing it', help: { command: 'uv lock --help', example: { args: ['--offline'] } } },
            },
            'upgrade--uv': {
                command: 'uv lock --upgrade',
                options: { cwd: member.root },
                metadata: { description: 'Moves every locked package to its newest allowed release', help: { command: 'uv lock --help', example: { args: ['--upgrade-package', '<package>'] } } },
            },
        }),
        ...(root &&
            member.pixi && {
                'upgrade--pixi': {
                    command: 'pixi update',
                    options: { cwd: member.root },
                    metadata: { description: 'Moves every package of the Pixi environments to the newest release its requirement allows and rewrites pixi.lock', help: { command: 'pixi update --help', example: { args: ['--dry-run'] } } },
                },
            }),
        ...(member.packaged && {
            'build--uv': {
                command: line(['uv', 'build', '--package', member.name, '--no-sources', '--out-dir', relative(member.workspace, distributions), '--clear', '--no-create-gitignore']),
                options: { cwd: member.workspace },
                cache: true,
                inputs: [...member.sources, ...built, { externalDependencies: [] }],
                outputs: [join('{workspaceRoot}', distributions)],
                metadata: { description: 'Builds the member sdist and wheel with uv', help: { command: 'uv build --help', example: { args: ['--wheel'] } } },
            },
        }),
        ...(member.scripted && {
            'install--uv': {
                command: line(['uv', 'tool', 'install', '--reinstall', ...flagged('--python', member.python), '.']),
                options: { cwd: member.root },
                metadata: { description: "Builds the member from the checkout and installs its scripts into uv's tool folder, on an interpreter its requires-python accepts", help: { command: 'uv tool install --help', example: { args: ['--editable'] } } },
            },
        }),
    };
    const written = Record.keys(Record.filter(tools, ({ outputs = [] }) => outputs.length > 0));
    const delivered: Record<string, TargetConfiguration> = {
        ...tools,
        ...(written.length > 0 && { 'clean--uv': { executor: `${SHARED}:clean`, options: { targets: written }, metadata: { description: `Removes the outputs ${written.join(', ')} declare` } } }),
        ...(member.published && {
            'attach--gh': {
                executor: `${SHARED}:attach`,
                dependsOn: ['build--uv'],
                metadata: { description: "Uploads the files build--uv wrote to the GitHub release of the version the project's manifest states through gh release upload", help: { command: 'gh release upload --help', example: { options: { dryRun: true } } } },
            },
        }),
    };
    return {
        ...Record.map(delivered, (tool): TargetConfiguration => ({ ...tool, metadata: { technologies: ['python'], ...tool.metadata } })),
        ...Record.fromIterableWith(Array.dedupe(Record.keys(delivered).map((name) => name.slice(0, name.indexOf('--')))), (verb): [string, TargetConfiguration] => [verb, { dependsOn: [`${verb}--*`] }]),
        ...(member.published && {
            'nx-release-publish': {
                executor: `${SHARED}:publish`,
                options: { publish: published, preview: [...published, '--dry-run'] },
                dependsOn: ['build--uv', '^nx-release-publish'],
                metadata: { description: 'Publishes the distributions build--uv wrote with uv publish, which reads UV_PUBLISH_TOKEN', help: { command: 'uv publish --help', example: { options: { dryRun: true } } } },
            },
        }),
    };
};

// --- [EXPORTS] -------------------------------------------------------------------------

export { CHECKERS, CONFIGS, declared, documents, PIXI, refused, SHARED, targets };
