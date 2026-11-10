import type { TargetConfiguration } from '@nx/devkit';
import { aggregated, ancestors, line } from '@rasm/nx-devkit/inference';
import { ToolError } from '@rasm/nx-devkit/process';
import { Array, Effect, FileSystem, identity, Option, Path, Record, Result, Schema, SchemaTransformation, String, Struct } from 'effect';
import { Ini, Toml } from 'effect/encoding';
import type { Member } from '#/plugins/project-graph';

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
        version: '--python-version',
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
        version: '--python-version',
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
        command: (at: string, config: string) => ['refurb', '--config-file', config, at],
        mypy: ['--'],
        version: '--python-version',
        description: 'Reports refurb findings over a mypy build of the member',
        help: ['refurb', '--help'],
        example: ['--format', 'github'],
    },
    pytest: {
        verb: 'test',
        variables: [],
        files: [['pytest.toml', 'pytest'], ['.pytest.toml', 'pytest'], ['pytest.ini'], ['.pytest.ini'], ['pyproject.toml', 'tool', 'pytest'], ['tox.ini', 'pytest'], ['setup.cfg', 'tool:pytest']],
        matches: ['test_*.py', '*_test.py'],
        sources: ['*'],
        run: ['--isolated'],
        folder: identity<string>,
        version: undefined,
        command: (at: string) => ['pytest', at],
        mypy: undefined,
        description: 'Runs the member tests with pytest',
        help: ['pytest', '--help'],
        example: ['-k', '<expression>'],
    },
} as const;
const PIXI = 'pixi.toml';
const CONFIGS = Array.dedupe([...Object.values(CHECKERS).flatMap(({ files }) => files.map(([file]) => file)), PIXI]);
const SEPARATOR = /\s+/u;
const BUILD = 'build--uv';
const TECHNOLOGIES = ['python'];
const TOOL = [{ env: 'UV_LOCKED' }, { runtime: 'uv --version' }] as const;

// --- [MODELS] --------------------------------------------------------------------------

const Table = Schema.Record(Schema.String, Schema.Unknown);
const Patterns = Schema.Struct({
    python_files: Schema.optionalKey(
        Schema.Union([Schema.Array(Schema.String), Schema.String.pipe(Schema.decodeTo(Schema.Array(Schema.String), SchemaTransformation.transform({ decode: (text): readonly string[] => text.split(SEPARATOR).filter(String.isNonEmpty), encode: (files: readonly string[]) => files.join(' ') })))]),
    ),
});
const Settings = Patterns.mapFields(Struct.assign({ ini_options: Schema.optionalKey(Patterns), pytest: Schema.optionalKey(Patterns), cache_dir: Schema.optionalKey(Schema.String) }));

// --- [OPERATIONS] ----------------------------------------------------------------------

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
    return Option.map(
        Array.findFirstWithIndex(candidates, ([at, keys]) =>
            Option.flatMap(
                keys.reduce<Option.Option<unknown>>((table, key) => table.pipe(Option.flatMap(Schema.decodeUnknownOption(Table)), Option.flatMap(Record.get(key))), Option.fromUndefinedOr(documented.get(at))),
                (table) => Option.map(Schema.decodeUnknownOption(Settings)(table), (settings) => [at, settings] as const),
            ),
        ),
        ([[config, settings], index]) => ({
            config,
            cache: settings.cache_dir,
            inputs: Array.dedupe(
                candidates
                    .slice(0, index + 1)
                    .filter(([at]) => documented.has(at))
                    .map(([at]) => path.join('{workspaceRoot}', at)),
            ),
            patterns: (settings.python_files ?? settings.ini_options?.python_files ?? settings.pytest?.python_files ?? CHECKERS[checker].matches).map((pattern) => path.join(root, '**', pattern)),
        }),
    );
};

const targets = (path: Path.Path, member: Member, roots: ReadonlyMap<string, Member>, plugin: string, cleaner: string, dependencies: string, present: ReadonlyMap<Member['checks'][string], string>, outputPath: string | undefined): Readonly<Record<string, TargetConfiguration>> => {
    const root = member.root === member.workspace;
    const named = Option.fromUndefinedOr(member.name);
    const distributions = Option.map(named, (name) => (outputPath === undefined ? path.join(member.workspace, 'dist', name) : path.join(outputPath, 'uv', name)));
    const built = Option.toArray(distributions).map((folder) => `!${path.join('{workspaceRoot}', folder, '**', '*')}`);
    const checkers = Array.groupBy(
        member.nested
            ? []
            : Array.getSomes(
                  Record.collect(CHECKERS, (key, row) =>
                      Option.gen(function* () {
                          const checked = yield* Option.fromUndefinedOr(member.checks[key]);
                          const folder = row.folder(member.workspace, yield* Option.fromUndefinedOr(present.get(checked)));
                          const host = checked.locked ? member : yield* Option.fromUndefinedOr(roots.get(path.dirname(checked.config)));
                          const tool = yield* Option.filter(Option.fromUndefinedOr(host.checks[key]), ({ locked }) => locked);
                          const typed = Record.get(member.checks, 'mypy');
                          const read = checked.locked ? checked : yield* Option.filter(typed, ({ locked }) => locked && row.mypy !== undefined);
                          const { cache: stated, config } = Option.getOrElse(typed, () => checked);
                          const cache = path.join((stated ?? '.mypy_cache').replace('$MYPY_CONFIG_FILE_DIR', path.relative(folder, path.dirname(config)) || '.'), member.root, key);
                          const interpreter = checked.locked ? [] : [CHECKERS.mypy.version, yield* Option.fromUndefinedOr(checked.version)];
                          return [key, row, checked, folder, host, tool, read, row.mypy === undefined ? [] : [...row.mypy, '--cache-dir', cache, ...interpreter]] as const;
                      }),
                  ),
              ),
        ([, row]) => row.verb,
    );
    const run = (group: (typeof checkers)['check' | 'test']) =>
        Record.fromIterableWith(
            group ?? [],
            ([key, row, checked, folder, host, tool, read, forwarded]) =>
                [
                    key,
                    {
                        executor: 'nx:run-commands',
                        options: {
                            command: line([
                                'uv',
                                'run',
                                ...row.run,
                                ...(path.relative(member.workspace, folder).startsWith('..') ? ['--project', path.relative(folder, member.workspace)] : []),
                                ...Option.match(named, { onNone: () => [], onSome: (name) => ['--package', name] }),
                                ...(read.group === undefined ? [] : ['--group', read.group]),
                                ...(checked.locked ? [] : ['uv', 'run', '--quiet', '--project', path.relative(folder, host.root) || '.', ...(tool.group === undefined ? [] : ['--group', tool.group])]),
                                ...row.command(path.relative(folder, member.root) || '.', path.relative(folder, checked.config)),
                                ...(row.version === undefined || checked.version === undefined ? [] : [row.version, checked.version]),
                                ...forwarded,
                            ]),
                            cwd: folder,
                        },
                        cache: true,
                        inputs: [
                            ...row.sources.map((glob) => path.join('{projectRoot}', '**', glob)),
                            ...built,
                            ...checked.inputs,
                            ...row.variables.map((env) => ({ env })),
                            ...TOOL,
                            dependencies,
                            ...Array.dedupe([member, host]).flatMap(({ workspace, lock }): NonNullable<TargetConfiguration['inputs']> => [path.join('{workspaceRoot}', workspace, '.python-version'), path.join('{workspaceRoot}', lock)]),
                            ...Array.match(
                                Array.dedupe([...read.projects, ...tool.projects]).filter((name) => name !== member.name),
                                { onEmpty: () => [], onNonEmpty: (projects) => [{ input: 'default', projects }] },
                            ),
                            { externalDependencies: [] },
                        ],
                        metadata: { technologies: TECHNOLOGIES, description: row.description, help: { command: line(['uv', 'run', ...row.help]), example: { args: [...row.example] } } },
                    } satisfies TargetConfiguration,
                ] as const,
        );
    return {
        ...aggregated(
            {
                check: {
                    ...run(checkers.check),
                    ...(root
                        ? {
                              uv: {
                                  executor: 'nx:run-commands',
                                  options: { command: line(['uv', 'lock', '--dry-run']), cwd: member.workspace },
                                  cache: true,
                                  inputs: [
                                      ...[...roots.values()].filter(({ workspace }) => workspace === member.workspace).map(({ root: at }) => path.join('{workspaceRoot}', at, 'pyproject.toml')),
                                      path.join('{workspaceRoot}', member.lock),
                                      ...['uv.toml', '.python-version'].map((file) => path.join('{projectRoot}', file)),
                                      ...TOOL,
                                      { externalDependencies: [] },
                                  ],
                                  metadata: { technologies: TECHNOLOGIES, description: 'Resolves the workspace and reports the changes uv.lock lacks without writing it, failing on a stale lock under UV_LOCKED', help: { command: 'uv lock --help', example: { args: ['--offline'] } } },
                              },
                          }
                        : {}),
                },
                test: run(checkers.test),
                upgrade: root
                    ? {
                          ...(member.pixi
                              ? {
                                    pixi: {
                                        executor: 'nx:run-commands',
                                        options: { command: line(['pixi', 'update']), cwd: member.workspace },
                                        outputs: ['{projectRoot}/pixi.lock'],
                                        metadata: { technologies: TECHNOLOGIES, description: 'Moves every package of the Pixi environments to the newest release its requirement allows and rewrites pixi.lock', help: { command: 'pixi update --help', example: { args: ['--dry-run'] } } },
                                    },
                                }
                              : {}),
                          uv: {
                              executor: 'nx:run-commands',
                              dependsOn: member.pixi ? ['upgrade--pixi'] : [],
                              options: { command: line(['uv', 'lock', '--upgrade']), cwd: member.workspace },
                              outputs: ['{projectRoot}/uv.lock'],
                              metadata: { technologies: TECHNOLOGIES, description: 'Moves every locked package to its newest allowed release', help: { command: 'uv lock --help', example: { args: ['--upgrade-package', '<package>'] } } },
                          },
                      }
                    : {},
                clean: root
                    ? {
                          'uv-cache': {
                              executor: cleaner,
                              options: { commands: [['uv', 'cache', 'prune']], cwd: member.workspace },
                              configurations: { all: { commands: [['uv', 'cache', 'clean']] } },
                              metadata: { technologies: TECHNOLOGIES, description: 'Removes the unused entries of the uv cache, every entry under the all configuration', help: { command: 'uv cache prune --help', example: { args: ['--ci'] } } },
                          },
                      }
                    : {},
                build: Option.match(
                    Option.filter(Option.all({ name: named, folder: distributions }), () => member.packaged),
                    {
                        onNone: () => ({}),
                        onSome: ({ name, folder }) => ({
                            uv: {
                                executor: 'nx:run-commands',
                                options: { command: line(['uv', 'build', '--package', name, '--no-sources', '--out-dir', path.relative(member.workspace, folder), '--clear']), cwd: member.workspace },
                                cache: true,
                                inputs: ['{projectRoot}/**/*', ...built, ...TOOL, { externalDependencies: [] }],
                                outputs: [path.join('{workspaceRoot}', folder)],
                                metadata: { technologies: TECHNOLOGIES, description: 'Builds the member sdist and wheel with uv', help: { command: 'uv build --help', example: { args: ['--wheel'] } } },
                            },
                        }),
                    },
                ),
                install: member.scripted
                    ? {
                          uv: {
                              executor: 'nx:run-commands',
                              options: { command: line(['uv', 'tool', 'install', '--reinstall', ...(member.python === undefined ? [] : ['--python', member.python]), '.']), cwd: member.root },
                              metadata: { technologies: TECHNOLOGIES, description: "Builds the member from the checkout and installs its scripts into uv's tool folder, on an interpreter its requires-python accepts", help: { command: 'uv tool install --help', example: { args: ['--editable'] } } },
                          },
                      }
                    : {},
            },
            TECHNOLOGIES,
            { executor: cleaner, tool: 'uv' },
        ),
        ...(member.published
            ? {
                  'nx-release-publish': {
                      executor: `${plugin}:release-publish`,
                      dependsOn: [BUILD, '^nx-release-publish'],
                      metadata: { technologies: TECHNOLOGIES, description: 'Publishes the built distributions with uv', help: { command: 'uv publish --help', example: { options: { dryRun: true } } } },
                  },
              }
            : {}),
    };
};

// --- [EXPORTS] -------------------------------------------------------------------------

export { BUILD, CHECKERS, CONFIGS, declared, documents, PIXI, targets };
