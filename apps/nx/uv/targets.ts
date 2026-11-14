import { dirname, extname, join, relative } from 'node:path';
import { layer, type NodeServices } from '@effect/platform-node/NodeServices';
import { type RawProjectGraphDependency, readJsonFile, type TargetConfiguration } from '@nx/devkit';
import { mergeTargetConfigurations, type PackageJson, quoteShellArg } from '@nx/devkit/internal';
import distribution from '@rasm/nx-distribution/package.json' with { type: 'json' };
import { Array, Context, Effect, FileSystem, Formatter, identity, Layer, ManagedRuntime, Option, pipe, Record, Result, Schema, SchemaTransformation, type Scope, Stream, String, Struct, UndefinedOr } from 'effect';
import { Ini, Toml } from 'effect/encoding';
import { ChildProcess, CommandLine } from 'effect/process';
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
const SHARED = distribution.name;
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
const Reason = Schema.TaggedUnion({ Exited: { code: Schema.Int, said: Schema.String }, Undecoded: { form: Schema.String }, Missing: { what: Schema.String }, Refused: { rule: Schema.String } });

// --- [ERRORS] --------------------------------------------------------------------------

class UvError extends Schema.TaggedError<UvError, { readonly brand: unique symbol }>()('UvError', {
    reason: Reason,
    subject: Schema.String,
    cause: Schema.optionalKey(Schema.Defect()),
}) {
    static readonly of = (reason: UvError['reason'], subject: string) => (cause?: unknown) => UvError.make({ reason, subject, ...(cause === undefined ? {} : { cause }) });

    override get message() {
        const reason = Reason.match(this.reason, { Exited: ({ code, said }) => [`exits ${code}`, ...(said === '' ? [] : [said])].join('\n'), Undecoded: ({ form }) => `does not read as ${form}`, Missing: ({ what }) => `states no ${what}`, Refused: ({ rule }) => rule });
        return [`${this.subject}: ${reason}`, ...(this.cause === undefined ? [] : [Formatter.format(this.cause)])].join(': ');
    }
}

// --- [SERVICES] ------------------------------------------------------------------------

class Linked extends Context.Service<Linked, Map<string, readonly RawProjectGraphDependency[]>>()('@rasm/nx-uv/targets/Linked') {
    static readonly layer = Layer.sync(Linked, () => new Map());
}

// --- [OPERATIONS] ----------------------------------------------------------------------

const printed = Effect.fnUntraced(function* (command: string, args: readonly string[], options: ChildProcess.CommandOptions) {
    const handle = yield* ChildProcess.make(command, args, { extendEnv: true, detached: false, stdin: 'ignore', ...options });
    const [stdout, said, code] = yield* Effect.all([Stream.mkString(Stream.decodeText(handle.stdout)), Stream.mkString(Stream.decodeText(handle.stderr)), handle.exitCode], { concurrency: 'unbounded' });
    return code === 0 ? { stdout, said } : yield* UvError.of({ _tag: 'Exited', code, said }, CommandLine.display([command, ...args]))();
}, Effect.scoped);

const manifested = (workspaceRoot: string, root: string) =>
    Effect.map(
        Effect.flatMap(FileSystem.FileSystem, (fs) => fs.exists(join(workspaceRoot, root, 'package.json'))),
        (held): Readonly<Record<string, TargetConfiguration>> => (held ? (readJsonFile<PackageJson>(join(workspaceRoot, root, 'package.json')).nx?.targets ?? {}) : {}),
    );

const flagged = (flag: string, value: string | undefined): readonly string[] => (value === undefined ? [] : [flag, value]);

const ancestors = (folder: string): readonly string[] => (folder === '.' ? [folder] : [folder, ...ancestors(dirname(folder))]);

const documents = Effect.fnUntraced(function* (workspaceRoot: string, roots: readonly string[]) {
    const fs = yield* FileSystem.FileSystem;
    const present = yield* Effect.filter(Array.cartesianWith(Array.dedupe(roots.flatMap(ancestors)), CONFIGS, join), (at) => fs.exists(join(workspaceRoot, at)), { concurrency: 'unbounded' });
    const read = yield* Effect.forEach(present, (at) => Effect.map(fs.readFileString(join(workspaceRoot, at)), (text) => [at, extname(at) === '.toml' ? Toml.parse(text) : Result.succeed<unknown>(Ini.parse(text))] as const), { concurrency: 'unbounded' });
    return {
        documented: new Map(read.map(([at, parsed]) => [at, Result.getOrElse(parsed, Record.empty)] as const)),
        unparsed: Array.filterMap(read, ([at, parsed]) => Result.map(Result.flip(parsed), UvError.of({ _tag: 'Undecoded', form: 'TOML or INI' }, at))),
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

const targets = (member: Member, outputPath: Option.Option<string>, manifest: Readonly<Record<string, TargetConfiguration>>): Record<string, TargetConfiguration> => {
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
                        ...(row.verb === 'test' ? [{ runtime: 'node -p process.platform+process.arch' }] : []),
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
                return { verb: row.verb, key, target };
            }),
        ),
    );
    const published = ['uv', 'publish', ...flagged('--index', member.index)];
    const locking: NonNullable<TargetConfiguration['inputs']> = [...member.manifests.map((file) => join('{workspaceRoot}', file)), join('{workspaceRoot}', member.lock), ...['uv.toml', '.python-version'].map((file) => join('{projectRoot}', file)), { externalDependencies: [] }];
    const rooted: Readonly<Record<string, Readonly<Record<string, TargetConfiguration>>>> = root
        ? {
              check: {
                  uv: {
                      command: 'uv lock --check',
                      options: { cwd: member.root },
                      cache: true,
                      inputs: locking,
                      metadata: { description: 'Resolves the workspace and fails where uv.lock lacks a change the manifests state, without writing it', help: { command: 'uv lock --help', example: { args: ['--offline'] } } },
                  },
              },
              upgrade: {
                  uv: {
                      command: 'uv lock --upgrade',
                      options: { cwd: member.root },
                      inputs: locking,
                      metadata: { description: 'Moves every locked package to its newest allowed release', help: { command: 'uv lock --help', example: { args: ['--upgrade-package', '<package>'] } } },
                  },
                  ...(member.pixi && {
                      pixi: {
                          command: 'pixi update',
                          options: { cwd: member.root },
                          inputs: [PIXI, 'pyproject.toml', 'pixi.lock'].map((file) => join('{projectRoot}', file)),
                          metadata: { description: 'Moves every package of the Pixi environments to the newest release its requirement allows and rewrites pixi.lock', help: { command: 'pixi update --help', example: { args: ['--dry-run'] } } },
                      },
                  }),
              },
          }
        : {};
    const joined = (held: Readonly<Record<string, TargetConfiguration>>, added: Readonly<Record<string, TargetConfiguration>>) => ({ ...held, ...added });
    const tools: typeof rooted = pipe(
        Array.groupBy(checked, ({ verb }): string => verb),
        Record.map(Record.fromIterableWith(({ key, target }) => [key, target] as const)),
        Record.union(rooted, joined),
        Record.union(
            {
                build: {
                    ...(member.packaged && {
                        uv: {
                            command: line(['uv', 'build', '--package', member.name, '--no-sources', '--out-dir', relative(member.workspace, distributions), '--clear', '--no-create-gitignore']),
                            options: { cwd: member.workspace },
                            cache: true,
                            inputs: [...member.sources, join('{workspaceRoot}', member.lock), ...built, { externalDependencies: [] }],
                            outputs: [join('{workspaceRoot}', distributions)],
                            metadata: { description: 'Builds the member sdist and wheel with uv', help: { command: 'uv build --help', example: { args: ['--wheel'] } } },
                        },
                    }),
                },
                install: {
                    ...(member.scripted && {
                        uv: {
                            command: line(['uv', 'tool', 'install', '--reinstall', ...flagged('--python', member.python), '.']),
                            options: { cwd: member.root },
                            inputs: [...member.sources, ...built, { externalDependencies: [] }],
                            metadata: { description: "Builds the member from the checkout and installs its scripts into uv's tool folder, on an interpreter its requires-python accepts", help: { command: 'uv tool install --help', example: { args: ['--editable'] } } },
                        },
                    }),
                },
            },
            joined,
        ),
    );
    const writing = Record.collect(tools, (verb, held) =>
        Record.collect(
            Record.filter(held, ({ outputs = [] }) => outputs.length > 0),
            (tool, row) => ({ label: `${verb}--${tool}`, row }),
        ),
    ).flat();
    const verbs: typeof tools = {
        ...tools,
        clean: Array.match(writing, {
            onEmpty: () => ({}),
            onNonEmpty: (written) => ({ uv: { executor: `${SHARED}:clean`, inputs: Array.dedupe(written.flatMap(({ row }) => row.inputs ?? [])), options: { targets: written.map(({ label }) => label) }, metadata: { description: `Removes the outputs ${written.map(({ label }) => label).join(', ')} declare` } } }),
        }),
        attach: {
            ...(member.packaged && {
                gh: {
                    executor: `${SHARED}:attach`,
                    dependsOn: ['build--uv'],
                    inputs: [{ dependentTasksOutputFiles: '**/*' }],
                    metadata: { description: "Uploads the files build--uv wrote to the GitHub release of the version the project's manifest states through gh release upload", help: { command: 'gh release upload --help', example: { options: { dryRun: true } } } },
                },
            }),
        },
    };
    return {
        ...Record.fromEntries(
            Record.collect(
                Record.filter(verbs, (held) => !Record.isEmptyRecord(held)),
                (verb, held): readonly (readonly [string, TargetConfiguration])[] => [
                    ...Record.collect(held, (tool, row): readonly [string, TargetConfiguration] => [`${verb}--${tool}`, { ...row, metadata: { technologies: ['python'], ...row.metadata } }]),
                    [verb, { dependsOn: [`${verb}--*`], inputs: ['...', ...Array.dedupe(Record.collect(held, (tool, row) => mergeTargetConfigurations(manifest[`${verb}--${tool}`] ?? {}, row, undefined, undefined, undefined, false).inputs ?? ['default', '^default']).flat())] }],
                ],
            ).flat(),
        ),
        ...(member.published && {
            'nx-release-publish': {
                executor: `${SHARED}:publish`,
                options: { publish: published, preview: ['--dry-run'], credential: 'UV_PUBLISH_TOKEN' },
                dependsOn: ['build--uv', '^nx-release-publish'],
                inputs: [{ dependentTasksOutputFiles: '**/*' }],
                metadata: { description: 'Publishes the distributions build--uv wrote with uv publish, which reads UV_PUBLISH_TOKEN', help: { command: 'uv publish --help', example: { options: { dryRun: true } } } },
            },
        }),
    };
};

// --- [COMPOSITION] ---------------------------------------------------------------------

const runtime = ManagedRuntime.make(Layer.merge(Linked.layer, layer));

const run = <A, E>(effect: Effect.Effect<A, E, Linked | NodeServices | Scope.Scope>) => runtime.runPromise(Effect.scoped(effect));

// --- [EXPORTS] -------------------------------------------------------------------------

export { CHECKERS, CONFIGS, declared, documents, Linked, manifested, PIXI, printed, run, SHARED, targets, UvError };
