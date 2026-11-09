import { findPackageJSON } from 'node:module';
import type { ProjectConfiguration, TargetConfiguration } from '@nx/devkit';
import { globWithWorkspaceContext } from '@nx/devkit/internal';
import { Effect, FileSystem, Formatter, Path, Record, Schema, Stream } from 'effect';
import { ChildProcess } from 'effect/process';
import type { Run } from '#/executors/run/executor';

// --- [TYPES] ---------------------------------------------------------------------------

interface Part {
    readonly root: string;
    readonly name: string;
    readonly file: string;
    readonly technologies: readonly string[];
    readonly targets: Readonly<Record<string, TargetConfiguration>>;
    readonly release?: NonNullable<ProjectConfiguration['release']>;
}
interface Plugin {
    readonly name: string;
    readonly inputs: NonNullable<TargetConfiguration['inputs']>;
}

// --- [MODELS] --------------------------------------------------------------------------

const Manifest = Schema.fromJsonString(Schema.Struct({ name: Schema.String, version: Schema.String, optionalDependencies: Schema.Record(Schema.String, Schema.String).pipe(Schema.withDecodingDefaultKey(Effect.succeed({}))) }));

// --- [ERRORS] --------------------------------------------------------------------------

class AppleError extends Schema.TaggedError<AppleError, { readonly brand: unique symbol }>()('AppleError', { subject: Schema.String, detail: Schema.String, cause: Schema.optionalKey(Schema.Defect()) }) {
    static readonly of = (subject: string, detail: string) => (cause: unknown) => AppleError.make({ subject, detail, cause });

    override get message() {
        return [`${this.subject}: ${this.detail}`, ...(this.cause === undefined ? [] : [Formatter.format(this.cause)])].join(': ');
    }
}

// --- [OPERATIONS] ----------------------------------------------------------------------

const core = Effect.gen(function* () {
    const [path, fs] = yield* Effect.all([Path.Path, FileSystem.FileSystem]);
    const located = yield* Effect.fromNullishOr(findPackageJSON(import.meta.url));
    const { name, version, optionalDependencies } = yield* Effect.flatMap(fs.readFileString(located), Schema.decodeEffect(Manifest));
    const analyzer = Effect.flatMap(Effect.try({ try: () => new URL(import.meta.resolve('#analyzer')), catch: AppleError.of('#analyzer', 'does not resolve') }), path.fromFileUrl);
    const packages = [name, ...Record.keys(optionalDependencies)];
    const plugin = (workspaceRoot: string) =>
        Effect.map(
            Effect.promise(() => globWithWorkspaceContext(workspaceRoot, [path.relative(workspaceRoot, located)])),
            (tracked): Plugin => ({
                name,
                inputs: [{ env: 'DEVELOPER_DIR' }, { env: 'TOOLCHAINS' }, ...(tracked.length === 0 ? [{ externalDependencies: packages }] : [{ input: 'default', projects: packages }, { externalDependencies: [] }])],
            }),
        );
    return { name, version, folder: path.dirname(located), analyzer, plugin };
});

const spawned = (command: string, args: readonly string[], options: { readonly cwd: string; readonly input?: string }) =>
    Effect.scoped(
        Effect.gen(function* () {
            const handle = yield* ChildProcess.make(command, args, {
                cwd: options.cwd,
                extendEnv: true,
                ...(options.input === undefined ? {} : { stdin: Stream.encodeText(Stream.succeed(options.input)) }),
            });
            const [stdout, stderr, code] = yield* Effect.all([Stream.mkString(Stream.decodeText(handle.stdout)), Stream.mkString(Stream.decodeText(handle.stderr)), handle.exitCode], { concurrency: 'unbounded' });
            return code === 0 ? stdout : yield* AppleError.make({ subject: [command, ...args].join(' '), detail: `exited ${code}: ${stderr.trim()}` });
        }),
    );

const present = (command: string, args: readonly string[], cwd: string) =>
    spawned(command, args, { cwd }).pipe(
        Effect.asSome,
        Effect.catchReason('PlatformError', 'NotFound', () => Effect.succeedNone),
    );

const tokened = (path: Path.Path, at: string) => (path.isAbsolute(at) || at.startsWith('..') ? [] : [path.join('{workspaceRoot}', at)]);

const published = (plugin: Plugin, technology: string, folders: readonly string[], notarized: readonly string[]): TargetConfiguration => ({
    executor: `${plugin.name}:release-publish`,
    dependsOn: ['^nx-release-publish'],
    inputs: plugin.inputs,
    options: { folders, notarized },
    metadata: {
        technologies: [technology],
        description: notarized.length === 0 ? "Attaches the zips the release build wrote to the project's GitHub release" : "Attaches the zips the release build wrote to the project's GitHub release, then adds each notarized app naming SUFeedURL to its Sparkle feed",
        help: { command: 'gh release upload --help', example: { options: { dryRun: true } } },
    },
});

const target = (plugin: Plugin, runs: (typeof Run.Type)['runs'], metadata: NonNullable<TargetConfiguration['metadata']>, extra: Partial<TargetConfiguration> = {}): TargetConfiguration => ({
    executor: `${plugin.name}:run`,
    options: { runs },
    metadata,
    ...extra,
});

// --- [EXPORTS] -------------------------------------------------------------------------

export { AppleError, core, type Part, type Plugin, present, published, spawned, target, tokened };
