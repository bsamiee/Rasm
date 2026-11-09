import { parse } from '@decimalturn/toml-patch';
import type { TargetConfiguration } from '@nx/devkit';
import { quoteShellArg } from '@nx/devkit/internal';
import { Array, Effect, FileSystem, Option, Path, Predicate, Record, Schema } from 'effect';
import { Ini } from 'effect/encoding';
import type { Member } from '#/plugins/workspace';

// --- [TYPES] ---------------------------------------------------------------------------

type Checker = keyof typeof CHECKERS;
type Targets = Readonly<Record<string, TargetConfiguration>>;

// --- [CONSTANTS] -----------------------------------------------------------------------

const CHECKERS = {
    ty: {
        verb: 'check',
        variables: ['TY_UV'],
        files: [['ty.toml'], ['pyproject.toml', 'tool', 'ty']],
        sources: ['{projectRoot}/**/*.py', '{projectRoot}/**/*.pyi'],
        matches: ['*.py', '*.pyi'],
        folder: (workspace: string) => workspace,
        command: (at: string) => ['ty', 'check', at],
        description: 'Type-checks the member with ty',
        help: ['ty', 'check', '--help'],
        example: ['--output-format', 'concise'],
    },
    mypy: {
        verb: 'check',
        files: [['mypy.ini'], ['.mypy.ini'], ['pyproject.toml', 'tool', 'mypy'], ['setup.cfg', 'mypy']],
        sources: ['{projectRoot}/**/*.py', '{projectRoot}/**/*.pyi'],
        matches: ['*.py', '*.pyi'],
        folder: (workspace: string) => workspace,
        command: (at: string) => ['mypy', at],
        description: 'Type-checks the member with mypy and the plugins its configuration loads',
        help: ['mypy', '--help'],
        example: ['--output', 'json'],
    },
    refurb: {
        verb: 'check',
        files: [['pyproject.toml', 'tool', 'refurb']],
        sources: ['{projectRoot}/**/*.py', '{projectRoot}/**/*.pyi'],
        matches: ['*.py', '*.pyi'],
        folder: (_: string, root: string) => root,
        command: (_: string, config: string) => ['refurb', '--config-file', config, '.'],
        description: 'Reports refurb findings over a mypy build of the member',
        help: ['refurb', '--help'],
        example: ['--format', 'github'],
    },
    pytest: {
        verb: 'test',
        files: [['pytest.toml', 'pytest'], ['.pytest.toml', 'pytest'], ['pytest.ini'], ['.pytest.ini'], ['pyproject.toml', 'tool', 'pytest'], ['tox.ini', 'pytest'], ['setup.cfg', 'tool:pytest']],
        sources: ['{projectRoot}/**/*'],
        matches: ['test_*.py', '*_test.py'],
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
const RELEASE = 'release';
const VERBS = ['build', 'check', 'fix', 'upgrade'];
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
    return new Map(yield* Effect.forEach(present, (at) => Effect.map(fs.readFileString(path.join(workspaceRoot, at)), (text) => [at, path.extname(at) === '.toml' ? parse(text) : Ini.parse(text)] as const), { concurrency: 'unbounded' }));
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

const targets = (path: Path.Path, member: Member, plugin: string, dependencies: string, found: ReadonlySet<string>, outputPath: string | undefined): Targets => {
    const at = path.relative(member.workspace, member.root) || '.';
    const root = member.root === member.workspace;
    const distributions = outputPath === undefined ? path.join(member.workspace, 'dist', member.name) : path.join(outputPath, 'uv', member.name);
    const archives = Option.map(Option.fromUndefinedOr(member.extension), ({ id, version, platforms }) => {
        const folder = outputPath === undefined ? member.root : path.join(outputPath, 'blender', id);
        return { folder, files: platforms.length === 0 ? [`${id}-${version}.zip`] : platforms.map((platform) => `${id}-${version}-${platform.replaceAll('-', '_')}.zip`) };
    });
    const built = [
        `!${path.join('{workspaceRoot}', distributions, '**', '*')}`,
        ...Option.getOrElse(
            Option.map(archives, ({ folder, files }) => [folder, path.join(folder, RELEASE)].flatMap((written) => files.map((file) => `!${path.join('{workspaceRoot}', written, file)}`))),
            () => [],
        ),
    ];
    const inferred: Targets = {
        ...Record.fromEntries(
            (member.nested ? [] : Record.collect(CHECKERS, (key, row) => [key, row] as const)).flatMap(([key, row]) =>
                Option.toArray(Option.filter(Option.fromUndefinedOr(member.checks[key]), () => found.has(key))).map(
                    (checked) =>
                        [
                            row.verb === 'check' ? `${row.verb}--${key}` : row.verb,
                            {
                                executor: 'nx:run-commands',
                                options: {
                                    command: line(['uv', 'run', '--locked', '--package', member.name, ...(checked.group === undefined ? [] : ['--group', checked.group]), ...row.command(at, path.relative(member.root, checked.config))]),
                                    cwd: row.folder(member.workspace, member.root),
                                },
                                cache: true,
                                inputs: [
                                    ...row.sources,
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
        ...(member.packaged
            ? {
                  [BUILD]: {
                      executor: 'nx:run-commands',
                      options: { command: line(['uv', 'build', '--package', member.name, '--no-sources', '--out-dir', path.relative(member.workspace, distributions), '--clear']), cwd: member.workspace },
                      cache: true,
                      inputs: ['{projectRoot}/**/*', ...built, { externalDependencies: [] }],
                      outputs: [path.join('{workspaceRoot}', distributions)],
                      metadata: { technologies: TECHNOLOGIES, description: 'Builds the member sdist and wheel with uv', help: { command: 'uv build --help', example: { args: ['--wheel'] } } },
                  },
              }
            : {}),
        ...(member.published
            ? {
                  'nx-release-publish': {
                      executor: `${plugin}:release-publish`,
                      dependsOn: ['build', '^nx-release-publish'],
                      metadata: { technologies: TECHNOLOGIES, description: 'Publishes the built distributions with uv', help: { command: 'uv publish --help', example: { options: { dryRun: true } } } },
                  },
              }
            : {}),
        ...Option.match(archives, {
            onNone: () => ({}),
            onSome: ({ folder, files }) => ({
                'build--blender': {
                    executor: `${plugin}:extension-build`,
                    options: { outputPath: folder },
                    configurations: { [RELEASE]: { bundle: true, outputPath: path.join(folder, RELEASE) } },
                    cache: true,
                    inputs: ['{projectRoot}/**/*', ...built, dependencies, { externalDependencies: [] }],
                    outputs: files.map((file) => path.join('{options.outputPath}', file)),
                    metadata: {
                        technologies: [...TECHNOLOGIES, 'blender'],
                        description: 'Builds the Blender extension archives its manifest declares, bundling locked wheels for this Blender in the release configuration',
                        help: { command: 'blender --command extension build --help', example: { options: { bundle: true } } },
                    },
                },
            }),
        }),
    };
    return {
        ...inferred,
        ...Record.fromIterableWith(
            VERBS.filter((verb) => Record.keys(inferred).some((name) => name.startsWith(`${verb}--`))),
            (verb): readonly [string, TargetConfiguration] => {
                const inputs = Record.collect(
                    Record.filter(inferred, (_, name) => name.startsWith(`${verb}--`)),
                    (_, held) => held.inputs ?? [],
                ).flat();
                return [
                    verb,
                    {
                        executor: 'nx:noop',
                        dependsOn: [`${verb}--*`],
                        inputs: ['...', ...Array.dedupe(inputs)],
                        metadata: { technologies: TECHNOLOGIES, description: `Runs every ${verb}--* target` },
                    },
                ];
            },
        ),
    };
};

// --- [EXPORTS] -------------------------------------------------------------------------

export { BUILD, CHECKERS, CONFIGS, declared, documents, targets };
