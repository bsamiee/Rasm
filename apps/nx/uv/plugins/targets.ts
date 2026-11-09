import { logger, type TargetConfiguration } from '@nx/devkit';
import { quoteShellArg } from '@nx/devkit/internal';
import { Array, Effect, FileSystem, Option, Path, Predicate, Record, Result, Schema } from 'effect';
import { Ini, Toml } from 'effect/encoding';
import type { Member } from '#/plugins/project-graph';

// --- [TYPES] ---------------------------------------------------------------------------

type Checker = keyof typeof CHECKERS;
type Targets = Readonly<Record<string, TargetConfiguration>>;

// --- [CONSTANTS] -----------------------------------------------------------------------

const CHECKERS = {
    ty: {
        verb: 'check',
        variables: ['TY_UV'],
        files: [['ty.toml'], ['pyproject.toml', 'tool', 'ty']],
        matches: ['*.py', '*.pyi'],
        folder: (workspace: string) => workspace,
        command: (at: string) => ['ty', 'check', at],
        version: '--python-version',
        description: 'Type-checks the member with ty',
        help: ['ty', 'check', '--help'],
        example: ['--output-format', 'concise'],
    },
    mypy: {
        verb: 'check',
        files: [['mypy.ini'], ['.mypy.ini'], ['pyproject.toml', 'tool', 'mypy'], ['setup.cfg', 'mypy']],
        matches: ['*.py', '*.pyi'],
        folder: (workspace: string) => workspace,
        command: (at: string) => ['mypy', at],
        version: '--python-version',
        description: 'Type-checks the member with mypy and the plugins its configuration loads',
        help: ['mypy', '--help'],
        example: ['--output', 'json'],
    },
    refurb: {
        verb: 'check',
        files: [['pyproject.toml', 'tool', 'refurb']],
        matches: ['*.py', '*.pyi'],
        folder: (_: string, root: string) => root,
        command: (_: string, config: string) => ['refurb', '--config-file', config, '.'],
        version: '--python-version',
        description: 'Reports refurb findings over a mypy build of the member',
        help: ['refurb', '--help'],
        example: ['--format', 'github'],
    },
    pytest: {
        verb: 'test',
        files: [['pytest.toml', 'pytest'], ['.pytest.toml', 'pytest'], ['pytest.ini'], ['.pytest.ini'], ['pyproject.toml', 'tool', 'pytest'], ['tox.ini', 'pytest'], ['setup.cfg', 'tool:pytest']],
        matches: ['test_*.py', '*_test.py'],
        sources: ['*'],
        folder: (workspace: string) => workspace,
        command: (at: string) => ['pytest', at],
        description: 'Runs the member tests with pytest',
        help: ['pytest', '--help'],
        example: ['-k', '<expression>'],
    },
} as const;
const CONFIGS = Array.dedupe(Record.values(CHECKERS).flatMap(({ files }) => files.map(([file]) => file)));
const SEPARATOR = /\s+/u;
const BUILD = 'build--uv';
const TECHNOLOGIES = ['python'];

// --- [MODELS] --------------------------------------------------------------------------

const Patterns = Schema.Struct({ python_files: Schema.optionalKey(Schema.Union([Schema.Array(Schema.String), Schema.String])) });
const Settings = Schema.Struct({ ...Patterns.fields, ini_options: Schema.optionalKey(Patterns), pytest: Schema.optionalKey(Patterns) });

// --- [OPERATIONS] ----------------------------------------------------------------------

const line = (argv: readonly string[]) => argv.map(quoteShellArg).join(' ');

const ancestors = (path: Path.Path, root: string) => Array.unfold(Option.some(root), (at) => Option.map(at, (folder) => [folder, folder === '.' ? Option.none<string>() : Option.some(path.dirname(folder))] as const));

const documents = Effect.fnUntraced(function* (workspaceRoot: string, roots: readonly string[]) {
    const [path, fs] = yield* Effect.all([Path.Path, FileSystem.FileSystem]);
    const present = yield* Effect.filter(
        Array.cartesianWith(Array.dedupe(roots.flatMap((root) => ancestors(path, root))), CONFIGS, (folder, file) => path.join(folder, file)),
        (at) => fs.exists(path.join(workspaceRoot, at)),
        { concurrency: 'unbounded' },
    );
    const read = Effect.fnUntraced(function* (at: string) {
        const text = yield* fs.readFileString(path.join(workspaceRoot, at));
        const parsed = path.extname(at) === '.toml' ? Toml.parse(text) : Result.succeed<unknown>(Ini.parse(text));
        yield* Result.isFailure(parsed) ? Effect.sync(() => logger.warn(`${at} does not parse (${String(parsed.failure)}), so inference reads no setting from it and its tools report it when their targets run`)) : Effect.void;
        return [at, Result.getOrElse(parsed, Record.empty)] as const;
    });
    return new Map(yield* Effect.forEach(present, read, { concurrency: 'unbounded' }));
});

const declared = (path: Path.Path, held: ReadonlyMap<string, unknown>, root: string, checker: Checker) => {
    const candidates = ancestors(path, root).flatMap((folder) => CHECKERS[checker].files.map(([file, ...keys]) => [path.join(folder, file), keys] as const));
    return Option.map(
        Array.findFirstWithIndex(candidates, ([at, keys]) =>
            Option.flatMap(
                keys.reduce<Option.Option<unknown>>((table, key) => Option.flatMap(table, (value) => (Predicate.hasProperty(value, key) ? Option.fromUndefinedOr(value[key]) : Option.none())), Option.fromUndefinedOr(held.get(at))),
                (table) => Option.map(Schema.decodeUnknownOption(Settings)(table), (settings) => [at, settings] as const),
            ),
        ),
        ([[config, settings], index]) => ({
            config,
            inputs: Array.dedupe(candidates.slice(0, index + 1).flatMap(([at]) => (held.has(at) ? [path.join('{workspaceRoot}', at)] : []))),
            patterns: Option.match(Option.fromUndefinedOr(settings.python_files ?? settings.ini_options?.python_files ?? settings.pytest?.python_files), {
                onNone: () => CHECKERS[checker].matches,
                onSome: (files) => (typeof files === 'string' ? files.split(SEPARATOR).filter((file) => file.length > 0) : files),
            }).map((pattern) => path.join(root, '**', pattern)),
        }),
    );
};

const targets = (path: Path.Path, member: Member, plugin: string, dependencies: string, found: ReadonlySet<Member['checks'][string]>, outputPath: string | undefined): Targets => {
    const at = path.relative(member.workspace, member.root) || '.';
    const root = member.root === member.workspace;
    const named = Option.fromUndefinedOr(member.name);
    const distributions = Option.map(named, (name) => (outputPath === undefined ? path.join(member.workspace, 'dist', name) : path.join(outputPath, 'uv', name)));
    const built = Option.toArray(distributions).map((folder) => `!${path.join('{workspaceRoot}', folder, '**', '*')}`);
    const inferred: Targets = {
        ...Record.fromEntries(
            (member.nested ? [] : Record.collect(CHECKERS, (key, row) => [key, row] as const)).flatMap(([key, row]) =>
                Option.toArray(Option.filter(Option.fromUndefinedOr(member.checks[key]), (checked) => found.has(checked))).map(
                    (checked) =>
                        [
                            row.verb === 'check' ? `${row.verb}--${key}` : row.verb,
                            {
                                executor: 'nx:run-commands',
                                options: {
                                    command: line([
                                        'uv',
                                        'run',
                                        '--locked',
                                        ...Option.match(named, { onNone: () => [], onSome: (name) => ['--package', name] }),
                                        ...(checked.group === undefined ? [] : ['--group', checked.group]),
                                        ...row.command(at, path.relative(member.root, checked.config)),
                                        ...('version' in row && member.floor !== undefined ? [row.version, member.floor] : []),
                                    ]),
                                    cwd: row.folder(member.workspace, member.root),
                                },
                                cache: true,
                                inputs: [
                                    ...('sources' in row ? row.sources : row.matches).map((glob) => path.join('{projectRoot}', '**', glob)),
                                    ...built,
                                    ...checked.inputs,
                                    ...('variables' in row ? row.variables.map((env) => ({ env })) : []),
                                    path.join('{workspaceRoot}', member.workspace, '.python-version'),
                                    dependencies,
                                    ...(checked.projects.length === 0 ? [] : [{ input: 'default', projects: [...checked.projects] }]),
                                    { externalDependencies: [...checked.externals] },
                                ],
                                metadata: { technologies: TECHNOLOGIES, description: row.description, help: { command: line(['uv', 'run', ...row.help]), example: { args: [...row.example] } } },
                            } satisfies TargetConfiguration,
                        ] as const,
                ),
            ),
        ),
        ...(root
            ? {
                  'check--uv': {
                      executor: 'nx:run-commands',
                      options: { command: line(['uv', 'lock', '--check']), cwd: member.workspace },
                      cache: true,
                      inputs: [...member.manifests.map((pyproject) => path.join('{workspaceRoot}', pyproject)), '{projectRoot}/uv.lock', '{projectRoot}/uv.toml', '{projectRoot}/.python-version', { externalDependencies: [] }],
                      metadata: { technologies: TECHNOLOGIES, description: 'Fails when uv.lock no longer matches the workspace manifests', help: { command: 'uv lock --help', example: { args: ['--offline'] } } },
                  },
                  'upgrade--uv': {
                      executor: 'nx:run-commands',
                      options: { command: line(['uv', 'lock', '--upgrade']), cwd: member.workspace },
                      outputs: ['{projectRoot}/uv.lock'],
                      metadata: { technologies: TECHNOLOGIES, description: 'Moves every locked package to its newest allowed release', help: { command: 'uv lock --help', example: { args: ['--upgrade-package', '<package>'] } } },
                  },
              }
            : {}),
        ...Option.match(
            Option.filter(Option.all({ name: named, folder: distributions }), () => member.packaged),
            {
                onNone: () => ({}),
                onSome: ({ name, folder }) => ({
                    [BUILD]: {
                        executor: 'nx:run-commands',
                        options: { command: line(['uv', 'build', '--package', name, '--no-sources', '--out-dir', path.relative(member.workspace, folder), '--clear']), cwd: member.workspace },
                        cache: true,
                        inputs: ['{projectRoot}/**/*', ...built, { externalDependencies: [] }],
                        outputs: [path.join('{workspaceRoot}', folder)],
                        metadata: { technologies: TECHNOLOGIES, description: 'Builds the member sdist and wheel with uv', help: { command: 'uv build --help', example: { args: ['--wheel'] } } },
                    },
                }),
            },
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
    return {
        ...inferred,
        ...Record.map(
            Array.groupBy(
                Record.toEntries(inferred).filter(([name]) => name.includes('--')),
                ([name]) => name.slice(0, name.indexOf('--')),
            ),
            (members, verb): TargetConfiguration => ({
                executor: 'nx:noop',
                dependsOn: [`${verb}--*`],
                inputs: ['...', ...Array.dedupe(members.flatMap(([, held]) => held.inputs ?? []))],
                metadata: { technologies: TECHNOLOGIES, description: `Runs every ${verb}--* target` },
            }),
        ),
    };
};

// --- [EXPORTS] -------------------------------------------------------------------------

export { BUILD, CHECKERS, CONFIGS, declared, documents, targets };
