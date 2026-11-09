import { findPackageJSON } from 'node:module';
import type { ProjectConfiguration, TargetConfiguration } from '@nx/devkit';
import { Effect, FileSystem, Formatter, Path, Schema, Stream } from 'effect';
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

// --- [CONSTANTS] -----------------------------------------------------------------------

const TOOLCHAIN: NonNullable<TargetConfiguration['inputs']> = [{ env: 'DEVELOPER_DIR' }, { env: 'TOOLCHAINS' }, { externalDependencies: [] }];

// --- [MODELS] --------------------------------------------------------------------------

const Manifest = Schema.fromJsonString(Schema.Struct({ name: Schema.String, version: Schema.String }));

// --- [ERRORS] --------------------------------------------------------------------------

class AppleError extends Schema.TaggedError<AppleError, { readonly brand: unique symbol }>()('AppleError', { subject: Schema.String, detail: Schema.String }) {
    override get message() {
        return `${this.subject}: ${this.detail}`;
    }
}

// --- [OPERATIONS] ----------------------------------------------------------------------

const core = Effect.gen(function* () {
    const [path, fs] = yield* Effect.all([Path.Path, FileSystem.FileSystem]);
    const located = yield* Effect.fromNullishOr(findPackageJSON(import.meta.url));
    const { name, version } = yield* Effect.flatMap(fs.readFileString(located), Schema.decodeEffect(Manifest));
    const analyzer = Effect.flatMap(Effect.try({ try: () => new URL(import.meta.resolve('#analyzer')), catch: (cause) => AppleError.make({ subject: '#analyzer', detail: Formatter.format(cause) }) }), path.fromFileUrl);
    return { name, version, folder: path.dirname(located), analyzer };
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

const tokened = (path: Path.Path, at: string) => (path.isAbsolute(at) || at.startsWith('..') ? [] : [path.join('{workspaceRoot}', at)]);

const target = (plugin: string, runs: (typeof Run.Type)['runs'], metadata: NonNullable<TargetConfiguration['metadata']>, extra: Partial<TargetConfiguration> = {}): TargetConfiguration => ({
    executor: `${plugin}:run`,
    options: { runs },
    metadata,
    ...extra,
});

// --- [EXPORTS] -------------------------------------------------------------------------

export { AppleError, core, type Part, spawned, TOOLCHAIN, target, tokened };
