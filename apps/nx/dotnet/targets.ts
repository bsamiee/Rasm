import { basename, dirname, join, relative } from 'node:path';
import { layer, type NodeServices } from '@effect/platform-node/NodeServices';
import { type RawProjectGraphDependency, readJsonFile, type TargetConfiguration } from '@nx/devkit';
import { type PackageJson, quoteShellArg } from '@nx/devkit/internal';
import distribution from '@rasm/nx-distribution/package.json' with { type: 'json' };
import { Array, Context, Effect, FileSystem, Formatter, Layer, ManagedRuntime, Record, Ref, Schema, type Scope, Stream } from 'effect';
import { ChildProcess, CommandLine } from 'effect/process';

// --- [TYPES] ---------------------------------------------------------------------------

type Node = typeof Node.Type;
type Verbs = Readonly<Record<string, Readonly<Record<string, TargetConfiguration>>>>;

// --- [CONSTANTS] -----------------------------------------------------------------------

const PLUGIN = '@rasm/nx-dotnet';

// --- [MODELS] --------------------------------------------------------------------------

const Mode = Schema.Struct({ configuration: Schema.String, outputs: Schema.Array(Schema.String), published: Schema.Array(Schema.String), reports: Schema.Array(Schema.String), packed: Schema.String });
const Node = Schema.Struct({
    file: Schema.String,
    name: Schema.String,
    language: Schema.String,
    test: Schema.Boolean,
    packable: Schema.Boolean,
    publishable: Schema.Boolean,
    pushed: Schema.Boolean,
    evaluated: Mode,
    declared: Schema.Array(Mode),
    references: Schema.Array(Schema.Struct({ root: Schema.String, declared: Schema.String })),
    inputs: Schema.Array(Schema.String),
    bases: Schema.Array(Schema.String),
    packages: Schema.Array(Schema.Struct({ id: Schema.String, version: Schema.String, hash: Schema.String, direct: Schema.Boolean, dependencies: Schema.Array(Schema.String) })),
    locks: Schema.Array(Schema.String),
    restored: Schema.Array(Schema.String),
    results: Schema.Array(Schema.String),
    platforms: Schema.Array(Schema.String),
    implicit: Schema.Array(Schema.String),
    releases: Schema.NullOr(Schema.Struct({ shipped: Schema.String, unshipped: Schema.String })),
});

const Reason = Schema.TaggedUnion({ Exited: { code: Schema.Int, said: Schema.String }, Undecoded: { form: Schema.String }, Missing: { what: Schema.String }, Refused: { rule: Schema.String } });

// --- [ERRORS] --------------------------------------------------------------------------

class DotnetError extends Schema.TaggedError<DotnetError, { readonly brand: unique symbol }>()('DotnetError', {
    reason: Reason,
    subject: Schema.String,
    cause: Schema.optionalKey(Schema.Defect()),
}) {
    static readonly of = (reason: DotnetError['reason'], subject: string) => (cause?: unknown) => DotnetError.make({ reason, subject, ...(cause === undefined ? {} : { cause }) });

    override get message() {
        const reason = Reason.match(this.reason, { Exited: ({ code, said }) => [`exits ${code}`, ...(said === '' ? [] : [said])].join('\n'), Undecoded: ({ form }) => `does not read as ${form}`, Missing: ({ what }) => `states no ${what}`, Refused: ({ rule }) => rule });
        return [`${this.subject}: ${reason}`, ...(this.cause === undefined ? [] : [Formatter.format(this.cause)])].join(': ');
    }
}

// --- [SERVICES] ------------------------------------------------------------------------

class Edges extends Context.Service<Edges, Ref.Ref<readonly RawProjectGraphDependency[]>>()('@rasm/nx-dotnet/targets/Edges') {
    static readonly layer = Layer.effect(Edges, Ref.make<readonly RawProjectGraphDependency[]>([]));
}

// --- [OPERATIONS] ----------------------------------------------------------------------

const ran = Effect.fnUntraced(function* (command: string, args: readonly string[], options: ChildProcess.CommandOptions) {
    const handle = yield* ChildProcess.make(command, args, { extendEnv: true, detached: false, stdin: 'ignore', ...options });
    const [stdout, said, code] = yield* Effect.all([Stream.mkString(Stream.decodeText(handle.stdout)), Stream.mkString(Stream.decodeText(handle.stderr)), handle.exitCode], { concurrency: 'unbounded' });
    return { invoked: CommandLine.display([command, ...args]), stdout, said, code };
}, Effect.scoped);

const printed = (command: string, args: readonly string[], options: ChildProcess.CommandOptions) => Effect.flatMap(ran(command, args, options), ({ invoked, stdout, said, code }) => (code === 0 ? Effect.succeed(stdout) : Effect.fail(DotnetError.of({ _tag: 'Exited', code, said: [said, stdout].join('') }, invoked)())));

const manifested = (workspaceRoot: string, root: string) =>
    Effect.map(
        Effect.flatMap(FileSystem.FileSystem, (fs) => fs.exists(join(workspaceRoot, root, 'package.json'))),
        (held): Readonly<Record<string, TargetConfiguration>> => (held ? (readJsonFile<PackageJson>(join(workspaceRoot, root, 'package.json')).nx?.targets ?? {}) : {}),
    );

const line = (argv: readonly string[]) => argv.map(quoteShellArg).join(' ');

const metadata = (verb: string, description: string) => ({ description, help: { command: `dotnet ${verb} --help`, example: { args: ['--verbosity', 'detailed'] } } });

const targets = (node: Node): Verbs => {
    const root = dirname(node.file);
    const project = join('{projectRoot}', basename(node.file));
    const inputs: NonNullable<TargetConfiguration['inputs']> = [
        'default',
        '^default',
        ...node.inputs.map((input) => join('{workspaceRoot}', input)),
        ...node.bases.filter((base) => base.startsWith(`${root}/`)).map((base) => `!${join('{projectRoot}', relative(root, base), '**')}`),
        { env: 'NUGET_PACKAGES' },
        { externalDependencies: [] },
    ];
    const moded = <A extends object>(each: (mode: Node['evaluated']) => A) => ({ options: each(node.evaluated), configurations: Record.fromIterableWith(node.declared, (mode) => [mode.configuration.toLowerCase(), each(mode)] as const) });
    const written = (...modes: readonly { readonly outputs: readonly string[] }[]) => Array.makeBy(Math.max(0, ...modes.map(({ outputs }) => outputs.length)), (index) => `{options.outputs.${index}}`);
    const verb = node.publishable ? 'publish' : 'build';
    const built = moded(({ configuration, outputs, published, reports }) => ({ project, command: [verb, '--configuration', configuration], outputs: [...outputs, ...published, ...reports, ...node.restored] }));
    const packed = moded(({ configuration, packed: folder }) => ({ project, command: ['pack', '--no-build', '-restore', '--configuration', configuration], outputs: [join(folder, '*.nupkg')] }));
    const tested = moded(({ configuration }) => ({ commands: [{ command: line(['dotnet', 'restore', `-p:Configuration=${configuration}`]), forwardAllArgs: false }, line(['dotnet', 'test', '--no-build', '--configuration', configuration])] }));
    const cleaned = moded(({ configuration }) => ({ commands: [['dotnet', 'clean', node.file, '--configuration', configuration, '-p:BuildProjectReferences=false']] }));
    const formatted = (commands: Array.NonEmptyReadonlyArray<Array.NonEmptyReadonlyArray<string>>, description: string): TargetConfiguration => ({
        executor: `${PLUGIN}:format`,
        options: { commands, project },
        dependsOn: ['build--dotnet'],
        inputs,
        metadata: metadata(Array.headNonEmpty(Array.headNonEmpty(commands)), description),
    });
    const tools: Verbs = {
        build: {
            dotnet: {
                executor: `${PLUGIN}:build`,
                ...built,
                outputs: written(built.options, ...Object.values(built.configurations)),
                dependsOn: ['^build--dotnet'],
                cache: true,
                inputs,
                metadata: metadata(verb, `${node.publishable ? 'Builds and publishes' : 'Builds'} ${node.name} with its references in one MSBuild run, in configuration ${node.evaluated.configuration} unless the run names another the project declares`),
            },
        },
        pack: {
            ...(node.packable && {
                dotnet: {
                    executor: `${PLUGIN}:build`,
                    ...packed,
                    outputs: written(packed.options, ...Object.values(packed.configurations)),
                    dependsOn: ['build--dotnet'],
                    cache: true,
                    inputs,
                    metadata: metadata('pack', `Packs the build build--dotnet wrote for ${node.name} into its PackageOutputPath with dotnet pack --no-build, which compiles nothing`),
                },
            }),
        },
        check: {
            'dotnet-build': {
                executor: `${PLUGIN}:check`,
                ...moded(({ reports }) => ({ reports })),
                dependsOn: ['build--dotnet'],
                cache: true,
                inputs: [{ dependentTasksOutputFiles: '**/*.sarif', transitive: false }, { externalDependencies: [] }],
                metadata: { description: `Fails on each warning or error in the SARIF report the compile of ${node.name} wrote, printed as file(line,col): level id: text` },
            },
            'dotnet-format': { ...formatted([['format', 'style', '--verify-no-changes']], `Fails on any code style finding in ${node.name}, the IDE rules no build runs included, with dotnet format style --verify-no-changes`), cache: true },
        },
        test: {
            ...(node.test && {
                dotnet: {
                    executor: 'nx:run-commands',
                    options: { parallel: false, cwd: '{projectRoot}', ...tested.options },
                    configurations: tested.configurations,
                    dependsOn: ['build--dotnet'],
                    cache: true,
                    inputs: [...inputs, { dependentTasksOutputFiles: '**/*.runtimeconfig.json' }, { runtime: 'node -p process.platform+process.arch' }],
                    outputs: node.results.map((results) => join('{workspaceRoot}', results)),
                    metadata: metadata('test', `Tests the build build--dotnet wrote for ${node.name} with dotnet test --no-build, after a restore that fetches only packages the machine lacks`),
                },
            }),
        },
        fix: {
            'dotnet-format': formatted(
                [
                    ['format', 'style'],
                    ['format', 'analyzers'],
                ],
                `Applies the code style and analyzer fixes of ${node.name} with dotnet format style and analyzers`,
            ),
        },
    };
    return {
        ...tools,
        clean: {
            dotnet: {
                executor: `${distribution.name}:clean`,
                inputs,
                options: { ...cleaned.options, targets: Record.collect(tools, (named, held) => Record.keys(Record.filter(held, ({ outputs = [] }) => outputs.length > 0)).map((tool) => `${named}--${tool}`)).flat() },
                configurations: cleaned.configurations,
                metadata: metadata('clean', `Removes the build outputs of ${node.name} with dotnet clean`),
            },
        },
        attach: {
            ...(node.packable && {
                gh: {
                    executor: `${distribution.name}:attach`,
                    dependsOn: ['pack--dotnet'],
                    inputs: [{ dependentTasksOutputFiles: '**/*' }],
                    metadata: { description: "Uploads the files pack--dotnet wrote to the GitHub release of the version the project's manifest states through gh release upload", help: { command: 'gh release upload --help', example: { options: { dryRun: true } } } },
                },
            }),
        },
    };
};

const publishing = (node: Node): Readonly<Record<string, TargetConfiguration>> =>
    node.packable && node.pushed
        ? {
              'nx-release-publish': {
                  executor: `${distribution.name}:publish`,
                  options: { publish: ['dotnet', 'nuget', 'push', '--skip-duplicate'], credential: 'NUGET_API_KEY', flag: '--api-key' },
                  dependsOn: ['pack--dotnet', '^nx-release-publish'],
                  inputs: [{ dependentTasksOutputFiles: '**/*' }],
                  metadata: {
                      technologies: ['dotnet', node.language],
                      description: `Pushes the package pack--dotnet wrote for ${node.name} with dotnet nuget push, under the key NUGET_API_KEY holds, to the default push source NuGet resolves, which skips a version the source holds`,
                      help: { command: 'dotnet nuget push --help', example: { options: { dryRun: true } } },
                  },
              },
          }
        : {};

// --- [COMPOSITION] ---------------------------------------------------------------------

const runtime = ManagedRuntime.make(Layer.merge(Edges.layer, layer));

const run = <A, E>(effect: Effect.Effect<A, E, Edges | NodeServices | Scope.Scope>) => runtime.runPromise(Effect.scoped(effect));

const streamed = <A, E>(results: Stream.Stream<A, E, NodeServices>) => Stream.toReadableStream(Stream.provide(results, layer)).values();

// --- [EXPORTS] -------------------------------------------------------------------------

export { DotnetError, Edges, manifested, Node, PLUGIN, printed, publishing, ran, run, streamed, targets, type Verbs };
