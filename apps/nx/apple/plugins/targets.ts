import type { TargetConfiguration } from '@nx/devkit';
import { Effect, type Path, Schema, Stream } from 'effect';
import { ChildProcess } from 'effect/process';
import type { Run } from '../executors/run/executor.ts';
import manifest from '../package.json' with { type: 'json' };

// --- [TYPES] ---------------------------------------------------------------------------

interface Options {
    readonly trustPackages?: boolean;
}
interface Part {
    readonly root: string;
    readonly name: string;
    readonly file: string;
    readonly technologies: readonly string[];
    readonly targets: Readonly<Record<string, TargetConfiguration>>;
}

// --- [CONSTANTS] -----------------------------------------------------------------------

const TOOLCHAIN: NonNullable<TargetConfiguration['inputs']> = [{ env: 'DEVELOPER_DIR' }, { env: 'TOOLCHAINS' }, { externalDependencies: [] }];

// --- [ERRORS] --------------------------------------------------------------------------

class AppleError extends Schema.TaggedError<AppleError, { readonly brand: unique symbol }>()('AppleError', { subject: Schema.String, detail: Schema.String }) {
    override get message() {
        return `${this.subject}: ${this.detail}`;
    }
}

// --- [OPERATIONS] ----------------------------------------------------------------------

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

const target = (runs: (typeof Run.Type)['runs'], metadata: NonNullable<TargetConfiguration['metadata']>, extra: Partial<TargetConfiguration> = {}): TargetConfiguration => ({
    executor: `${manifest.name}:run`,
    options: { runs },
    metadata,
    ...extra,
});

// --- [EXPORTS] -------------------------------------------------------------------------

export { AppleError, type Options, type Part, spawned, TOOLCHAIN, target, tokened };
