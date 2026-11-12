import type { TargetConfiguration } from '@nx/devkit';
import { ancestors, type Identity, line, type Project } from '@rasm/nx-devkit/plugin';
import { ToolError } from '@rasm/nx-devkit/process';
import { Array, Effect, FileSystem, identity, Option, Path, Record, Result, Schema, SchemaTransformation, String, Struct, UndefinedOr } from 'effect';
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
        help: ['ty', 'check', '--help'],
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
        help: ['mypy', '--help'],
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
        help: ['refurb', '--help'],
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
        help: ['vulture', '--help'],
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
        help: ['pytest', '--help'],
        example: ['-k', '<expression>'],
    },
} as const;
const PIXI = 'pixi.toml';
const CONFIGS = Array.dedupe([...Object.values(CHECKERS).flatMap(({ files }) => files.map(([file]) => file)), PIXI]);
const SEPARATOR = /\s+/u;
const TOOL = { runtime: 'uv --version' } as const;

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

const documents = Effect.fnUntraced(function* (workspaceRoot: string, roots: readonly string[]) {
    const [path, fs] = yield* Effect.all([Path.Path, FileSystem.FileSystem]);
    const present = yield* Effect.filter(
        Array.cartesianWith(Array.dedupe(roots.flatMap((root) => ancestors(path, root))), CONFIGS, (folder, file) => path.join(folder, file)),
        (at) => fs.exists(path.join(workspaceRoot, at)),
        { concurrency: 'unbounded' },
    );
    const read = yield* Effect.forEach(present, (at) => Effect.map(fs.readFileString(path.join(workspaceRoot, at)), (text) => [at, path.extname(at) === '.toml' ? Toml.parse(text) : Result.succeed<unknown>(Ini.parse(text))] as const), { concurrency: 'unbounded' });
    return {
        documented: new Map(read.map(([at, parsed]) => [at, Result.getOrElse(parsed, Record.empty)] as const)),
        unparsed: Array.filterMap(read, ([at, parsed]) => Result.map(Result.flip(parsed), (cause) => [at, ToolError.of(at, 'does not parse')(cause)] as const)),
    };
});

const declared = (path: Path.Path, documented: ReadonlyMap<string, unknown>, root: string, checker: Checker) => {
    const candidates = ancestors(path, root).flatMap((folder) => CHECKERS[checker].files.map(([file, ...keys]) => [path.join(folder, file), keys] as const));
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
                        .map(([at]) => path.join('{workspaceRoot}', at)),
                ),
        }),
        patterns: (settings?.python_files ?? settings?.ini_options?.python_files ?? settings?.pytest?.python_files ?? CHECKERS[checker].matches).map((pattern) => path.join(root, '**', pattern)),
    };
};

const targets = (path: Path.Path, member: Member, plugin: Identity, placed: (tool: string, name: string) => Option.Option<string>): Pick<Project, 'members' | 'packaged' | 'publish'> => {
    const root = member.root === member.workspace;
    const named = Option.fromUndefinedOr(member.name);
    const distributions = Option.map(named, (name) => Option.getOrElse(placed('uv', name), () => path.join(member.workspace, 'dist', name)));
    const built = Option.toArray(distributions)
        .map((folder) => path.relative(member.root, folder))
        .filter((folder) => !folder.startsWith('..'))
        .map((folder) => `!${path.join('{projectRoot}', folder, '**', '*')}`);
    const coverage = Option.flatMap(named, (name) => placed('coverage', name));
    const checkers = Array.groupBy(
        member.nested
            ? []
            : Array.getSomes(
                  Record.collect(CHECKERS, (key, row) =>
                      Option.gen(function* () {
                          const checked = yield* Option.filter(Option.fromUndefinedOr(member.checks[key]), ({ locked }) => locked);
                          const folder = row.folder(member.workspace, yield* Option.fromUndefinedOr(checked.base));
                          const { cache: stated = '.mypy_cache', config } = Option.getOrElse(Record.get(member.checks, 'mypy'), () => checked);
                          const cache = path.join(stated.replace('$MYPY_CONFIG_FILE_DIR', path.relative(folder, config === undefined ? folder : path.dirname(config)) || '.'), member.root, key);
                          return { key, row, checked, folder, cache };
                      }),
                  ),
              ),
        ({ row }) => row.verb,
    );
    const run = (group: (typeof checkers)['check' | 'test']) =>
        Record.fromIterableWith(group ?? [], ({ key, row, checked, folder, cache }) => {
            const argv = [
                'uv',
                'run',
                '--no-active',
                ...row.run,
                ...(path.relative(member.workspace, folder).startsWith('..') ? ['--project', path.relative(folder, member.workspace)] : []),
                ...flagged('--package', member.name),
                ...flagged('--group', checked.group),
                ...row.command(
                    path.relative(folder, member.root) || '.',
                    UndefinedOr.map(checked.config, (file) => path.relative(folder, file)),
                ),
                ...(row.mypy === undefined ? [] : [...row.mypy, '--cache-dir', cache]),
            ];
            return [
                key,
                {
                    command: line(argv),
                    options: { cwd: folder },
                    cache: true,
                    inputs: [
                        ...row.sources.map((glob) => path.join('{projectRoot}', '**', glob)),
                        ...built,
                        ...checked.inputs,
                        ...row.variables.map((env) => ({ env })),
                        TOOL,
                        '^default',
                        path.join('{workspaceRoot}', member.workspace, '.python-version'),
                        path.join('{workspaceRoot}', member.lock),
                        ...Array.match(
                            checked.projects.filter((name) => name !== member.name),
                            { onEmpty: () => [], onNonEmpty: (projects) => [{ input: 'default', projects }] },
                        ),
                    ],
                    ...Option.match(
                        Option.filter(coverage, () => checked.covered),
                        {
                            onNone: () => ({}),
                            onSome: (at) => ({
                                outputs: [path.join('{workspaceRoot}', at)],
                                configurations: { coverage: { command: line([...argv, '--cov', `--cov-report=lcov:${path.join(path.relative(folder, at), 'coverage.lcov')}`]), env: { COVERAGE_FILE: path.join(path.relative(folder, at), '.coverage') } } },
                            }),
                        },
                    ),
                    metadata: { description: row.description, help: { command: line(['uv', 'run', ...row.help]), example: { args: [...row.example] } } },
                } satisfies TargetConfiguration,
            ] as const;
        });
    const published = ['uv', 'publish', ...flagged('--index', member.index)];
    return {
        members: {
            check: {
                ...run(checkers.check),
                ...(root && {
                    uv: {
                        command: line(['uv', 'lock', '--check']),
                        cache: true,
                        inputs: [...member.manifests.map((file) => path.join('{workspaceRoot}', file)), path.join('{workspaceRoot}', member.lock), ...['uv.toml', '.python-version'].map((file) => path.join('{projectRoot}', file)), TOOL],
                        metadata: { description: 'Resolves the workspace and fails where uv.lock lacks a change the manifests state, without writing it', help: { command: 'uv lock --help', example: { args: ['--offline'] } } },
                    },
                }),
            },
            test: run(checkers.test),
            upgrade: root
                ? {
                      ...(member.pixi && {
                          pixi: {
                              command: line(['pixi', 'update']),
                              metadata: { description: 'Moves every package of the Pixi environments to the newest release its requirement allows and rewrites pixi.lock', help: { command: 'pixi update --help', example: { args: ['--dry-run'] } } },
                          },
                      }),
                      uv: {
                          command: line(['uv', 'lock', '--upgrade']),
                          metadata: { description: 'Moves every locked package to its newest allowed release', help: { command: 'uv lock --help', example: { args: ['--upgrade-package', '<package>'] } } },
                      },
                  }
                : {},
            build: Option.match(
                Option.filter(Option.all({ name: named, folder: distributions }), () => member.packaged),
                {
                    onNone: () => ({}),
                    onSome: ({ name, folder }) => ({
                        uv: {
                            command: line(['uv', 'build', '--package', name, '--no-sources', '--out-dir', path.relative(member.workspace, folder), '--clear']),
                            options: { cwd: member.workspace },
                            cache: true,
                            inputs: ['{projectRoot}/**/*', ...built, TOOL],
                            outputs: [path.join('{workspaceRoot}', folder)],
                            metadata: { description: 'Builds the member sdist and wheel with uv', help: { command: 'uv build --help', example: { args: ['--wheel'] } } },
                        },
                    }),
                },
            ),
            install: member.scripted
                ? {
                      uv: {
                          command: line(['uv', 'tool', 'install', '--reinstall', ...flagged('--python', member.python), '.']),
                          metadata: { description: "Builds the member from the checkout and installs its scripts into uv's tool folder, on an interpreter its requires-python accepts", help: { command: 'uv tool install --help', example: { args: ['--editable'] } } },
                      },
                  }
                : {},
        },
        ...(member.published &&
            Option.isSome(distributions) && {
                packaged: ['build--uv'],
                publish: {
                    executor: plugin.publisher,
                    options: { publish: published, preview: [...published, '--dry-run'] },
                    metadata: { description: 'Publishes the distributions build--uv wrote with uv publish, which reads UV_PUBLISH_TOKEN', help: { command: 'uv publish --help', example: { options: { dryRun: true } } } },
                },
            }),
    };
};

// --- [EXPORTS] -------------------------------------------------------------------------

export { CHECKERS, CONFIGS, declared, documents, PIXI, targets };
