import { parse } from '@decimalturn/toml-patch';
import type { TargetConfiguration } from '@nx/devkit';
import { quoteShellArg } from '@nx/devkit/internal';
import { Array, Effect, FileSystem, Option, Path, Predicate, Record, Schema } from 'effect';
import { Ini } from 'effect/encoding';
import manifest from '../package.json' with { type: 'json' };
import type { Member } from './workspace.ts';

// --- [TYPES] ---------------------------------------------------------------------------

type Checker = keyof typeof CHECKERS;

// --- [MODELS] --------------------------------------------------------------------------

const Patterns = Schema.Struct({ python_files: Schema.optionalKey(Schema.Union([Schema.Array(Schema.String), Schema.String])) });
const Settings = Schema.Struct({ ...Patterns.fields, ini_options: Schema.optionalKey(Patterns), pytest: Schema.optionalKey(Patterns) });

// --- [CONSTANTS] -----------------------------------------------------------------------

const CHECKERS = {
    ty: {
        verb: 'check',
        distribution: 'ty',
        environment: { TY_UV: '1' },
        files: [['ty.toml'], ['pyproject.toml', 'tool', 'ty']],
        sources: ['{projectRoot}/**/*.py', '{projectRoot}/**/*.pyi'],
        folder: (workspace: string) => workspace,
        command: (at: string) => ['ty', 'check', at],
        description: 'Type-checks the member with ty, imports of undeclared dependencies included',
        help: ['ty', 'check', '--help'],
        example: ['--output-format', 'concise'],
    },
    mypy: {
        verb: 'check',
        distribution: 'mypy',
        files: [['mypy.ini'], ['.mypy.ini'], ['pyproject.toml', 'tool', 'mypy'], ['setup.cfg', 'mypy']],
        sources: ['{projectRoot}/**/*.py', '{projectRoot}/**/*.pyi'],
        folder: (workspace: string) => workspace,
        command: (at: string) => ['mypy', at],
        description: 'Type-checks the member with mypy and the plugins its configuration loads',
        help: ['mypy', '--help'],
        example: ['--output', 'json'],
    },
    refurb: {
        verb: 'check',
        distribution: 'refurb',
        files: [['pyproject.toml', 'tool', 'refurb']],
        sources: ['{projectRoot}/**/*.py', '{projectRoot}/**/*.pyi'],
        folder: (_: string, root: string) => root,
        command: (_: string, config: string) => ['refurb', '--config-file', config, '.'],
        description: 'Reports refurb findings over a mypy build of the member',
        help: ['refurb', '--help'],
        example: ['--format', 'github'],
    },
    pytest: {
        verb: 'test',
        distribution: 'pytest',
        files: [['pytest.toml', 'pytest'], ['.pytest.toml', 'pytest'], ['pytest.ini'], ['.pytest.ini'], ['pyproject.toml', 'tool', 'pytest'], ['tox.ini', 'pytest'], ['setup.cfg', 'tool:pytest']],
        sources: ['{projectRoot}/**/*', '!{projectRoot}/dist/**/*'],
        folder: (workspace: string) => workspace,
        command: (at: string) => ['pytest', at],
        description: 'Runs the member tests with pytest',
        help: ['pytest', '--help'],
        example: ['-k', '<expression>'],
    },
} as const;
const CONFIGS = Array.dedupe(Record.values(CHECKERS).flatMap(({ files }) => files.map(([file]) => file)));
const PYTHON_FILES = ['test_*.py', '*_test.py'];

// --- [OPERATIONS] ----------------------------------------------------------------------

const line = (argv: readonly string[]) => argv.map(quoteShellArg).join(' ');

const ancestors = (path: Path.Path, root: string) =>
    Array.unfold(Option.some(root), (at) => Option.map(at, (folder) => [folder, folder === '.' ? Option.none<string>() : Option.some(path.dirname(folder))] as const));

const documents = Effect.fnUntraced(function* (workspaceRoot: string, roots: readonly string[]) {
    const [path, fs] = yield* Effect.all([Path.Path, FileSystem.FileSystem]);
    const read = yield* Effect.forEach(
        Array.dedupe(roots.flatMap((root) => ancestors(path, root).flatMap((folder) => CONFIGS.map((file) => path.join(folder, file))))),
        (at) =>
            Effect.flatMap(fs.exists(path.join(workspaceRoot, at)), (exists) =>
                exists
                    ? Effect.map(fs.readFileString(path.join(workspaceRoot, at)), (text): Option.Option<readonly [string, unknown]> => Option.some([at, path.extname(at) === '.toml' ? parse(text) : Ini.parse(text)]))
                    : Effect.succeedNone,
            ),
        { concurrency: 'unbounded' },
    );
    return new Map(Array.getSomes(read));
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
                onNone: () => PYTHON_FILES,
                onSome: (files) => (typeof files === 'string' ? files.split(/\s+/).filter((file) => file.length > 0) : files),
            }),
        }),
    );
};

const targets = (path: Path.Path, member: Member, dependencies: string, tested: boolean): Readonly<Record<string, TargetConfiguration>> => {
    const at = path.relative(member.workspace, member.root) || '.';
    const root = member.root === member.workspace;
    const inferred: Readonly<Record<string, TargetConfiguration>> = {
        ...Record.fromEntries(
            (member.nested ? [] : Record.collect(CHECKERS, (key, row) => [key, row] as const)).flatMap(([key, row]) =>
                Option.toArray(Option.filter(Option.fromUndefinedOr(member.checks[key]), () => tested || row.verb === 'check')).map(
                    (checked) =>
                        [
                            row.verb === 'check' ? `check--${key}` : row.verb,
                            {
                                executor: 'nx:run-commands',
                                options: {
                                    command: line(['uv', 'run', '--locked', '--package', member.name, ...(checked.group === undefined ? [] : ['--group', checked.group]), ...row.command(at, path.relative(member.root, checked.config))]),
                                    cwd: row.folder(member.workspace, member.root),
                                    ...('environment' in row ? { env: { ...row.environment } } : {}),
                                },
                                cache: true,
                                inputs: [
                                    ...row.sources,
                                    ...checked.inputs,
                                    path.join('{workspaceRoot}', member.workspace, '.python-version'),
                                    dependencies,
                                    ...(checked.projects.length === 0 ? [] : [{ input: 'default', projects: [...checked.projects] }]),
                                    { externalDependencies: [...checked.externals] },
                                ],
                                metadata: { technologies: ['python'], description: row.description, help: { command: line(['uv', 'run', ...row.help]), example: { args: [...row.example] } } },
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
                      inputs: [...member.manifests.map((manifest) => path.join('{workspaceRoot}', manifest)), '{projectRoot}/uv.lock', '{projectRoot}/uv.toml', '{projectRoot}/.python-version', { externalDependencies: [] }],
                      metadata: { technologies: ['python'], description: 'Fails when uv.lock no longer matches the workspace manifests', help: { command: 'uv lock --help', example: { args: ['--offline'] } } },
                  },
              }
            : {}),
    };
    const checks = Record.collect(Record.filter(inferred, (_, name) => name.startsWith('check--')), (_, target) => target.inputs ?? []).flat();
    return {
        ...inferred,
        ...(checks.length === 0
            ? {}
            : {
                  check: {
                      executor: 'nx:noop',
                      dependsOn: ['check--*'],
                      inputs: [
                          '...',
                          ...Array.dedupe(checks.filter((input) => !Predicate.hasProperty(input, 'externalDependencies'))),
                          { externalDependencies: Array.dedupe(checks.flatMap((input) => (Predicate.hasProperty(input, 'externalDependencies') ? input.externalDependencies : []))) },
                      ],
                      metadata: { technologies: ['python'], description: 'Runs every check--* target' },
                  },
              }),
        ...(root
            ? {
                  'upgrade--uv': {
                      executor: 'nx:run-commands',
                      options: { command: line(['uv', 'lock', '--upgrade']), cwd: member.workspace },
                      outputs: ['{projectRoot}/uv.lock'],
                      metadata: { technologies: ['python'], description: 'Moves every locked package to its newest allowed release', help: { command: 'uv lock --help', example: { args: ['--upgrade-package', '<package>'] } } },
                  },
                  upgrade: { executor: 'nx:noop', dependsOn: ['upgrade--*'], metadata: { technologies: ['python'], description: 'Runs every upgrade--* target' } },
              }
            : {}),
        ...(member.packaged
            ? {
                  build: {
                      executor: 'nx:run-commands',
                      options: { command: line(['uv', 'build', '--package', member.name, '--no-sources', '--out-dir', path.join(at, 'dist'), '--clear']), cwd: member.workspace },
                      cache: true,
                      inputs: ['{projectRoot}/**/*', '!{projectRoot}/dist/**/*', { externalDependencies: [] }],
                      outputs: ['{projectRoot}/dist'],
                      metadata: { technologies: ['python'], description: 'Builds the member sdist and wheel with uv', help: { command: 'uv build --help', example: { args: ['--wheel'] } } },
                  },
                  'nx-release-publish': {
                      executor: `${manifest.name}:release-publish`,
                      dependsOn: ['build', '^nx-release-publish'],
                      metadata: { technologies: ['python'], description: 'Publishes the built distributions with uv', help: { command: 'uv publish --help', example: { options: { dryRun: true } } } },
                  },
              }
            : {}),
    };
};

// --- [EXPORTS] -------------------------------------------------------------------------

export { CHECKERS, CONFIGS, declared, documents, targets };
