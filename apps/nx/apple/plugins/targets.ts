import { findPackageJSON } from 'node:module';
import type { ProjectConfiguration, TargetConfiguration } from '@nx/devkit';
import { Effect, FileSystem, Path, Schema, Stream } from 'effect';
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
const ANALYZER = 'nx-apple-analyzer';

// --- [MODELS] --------------------------------------------------------------------------

const Manifest = Schema.fromJsonString(Schema.Struct({ name: Schema.String, version: Schema.String }));
const Editor = Schema.fromJsonString(Schema.Struct({ swiftPM: Schema.Struct({ scratchPath: Schema.String }) }));

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
    const folder = path.dirname(located);
    const { name, version } = yield* Effect.flatMap(fs.readFileString(located), Schema.decodeEffect(Manifest));
    const analyzer = Effect.map(Effect.flatMap(fs.readFileString(path.join(folder, '.sourcekit-lsp', 'config.json')), Schema.decodeEffect(Editor)), (editor) => path.resolve(folder, editor.swiftPM.scratchPath, 'release', ANALYZER));
    return { name, version, folder, analyzer };
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
