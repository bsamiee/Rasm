import type { TargetConfiguration } from '@nx/devkit';
import { Array, Effect, Option, type Path, Schema, Stream } from 'effect';
import { ChildProcess } from 'effect/process';
import type { Invocation } from '../executors/run/executor.ts';
import manifest from '../package.json' with { type: 'json' };

// --- [TYPES] ---------------------------------------------------------------------------

interface Options {
    readonly outputPath?: string;
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

const FOLDERS = { scratch: 'swiftpm', derived: 'xcode', results: 'results', describe: 'describe' } as const;

// --- [ERRORS] --------------------------------------------------------------------------

class AppleError extends Schema.TaggedError<AppleError, { readonly brand: unique symbol }>()('AppleError', { subject: Schema.String, detail: Schema.String }) {
    override get message() {
        return `${this.subject}: ${this.detail}`;
    }
}

// --- [OPERATIONS] ----------------------------------------------------------------------

const spawned = (command: string, args: readonly string[], options: { readonly cwd: string; readonly env?: Readonly<Record<string, string>>; readonly input?: string }) =>
    Effect.scoped(
        Effect.gen(function* () {
            const handle = yield* ChildProcess.make(command, args, {
                cwd: options.cwd,
                extendEnv: true,
                ...(options.env === undefined ? {} : { env: options.env }),
                ...(options.input === undefined ? {} : { stdin: Stream.encodeText(Stream.succeed(options.input)) }),
            });
            const [stdout, stderr, code] = yield* Effect.all([Stream.mkString(Stream.decodeText(handle.stdout)), Stream.mkString(Stream.decodeText(handle.stderr)), handle.exitCode], { concurrency: 'unbounded' });
            return code === 0 ? stdout : yield* AppleError.make({ subject: [command, ...args].join(' '), detail: `exited ${code}: ${stderr.trim()}` });
        }),
    );

const tokened = (path: Path.Path, at: string) => (path.isAbsolute(at) || at.startsWith('..') ? [] : [path.join('{workspaceRoot}', at)]);

const target = (runs: readonly (typeof Invocation.Type)[], extra: Partial<TargetConfiguration> = {}): TargetConfiguration => ({ executor: `${manifest.name}:run`, options: { runs }, ...extra });

const linted = (path: Path.Path, root: string, paths: readonly string[]): Readonly<Record<string, TargetConfiguration>> => ({
    lint: target([{ command: ['swift', 'format', 'lint', '--strict', '--parallel', '--recursive', ...paths] }], {
        cache: true,
        inputs: [
            ...paths.flatMap((at) => (path.extname(at) === '.swift' ? [path.join('{workspaceRoot}', at)] : [path.join('{workspaceRoot}', at, '**/*.swift'), path.join('{workspaceRoot}', at, '**/.swift-format*')])),
            ...Array.unfold(Option.some(root), (at) => Option.map(at, (folder) => [folder, folder === '.' ? Option.none<string>() : Option.some(path.dirname(folder))] as const)).flatMap((folder) =>
                ['.swift-format', '.swift-format-ignore'].map((name) => path.join('{workspaceRoot}', folder, name)),
            ),
            { runtime: 'swift format --version' },
        ],
    }),
    format: target([{ command: ['swift', 'format', 'format', '--in-place', '--parallel', '--recursive', ...paths] }]),
});

// --- [EXPORTS] -------------------------------------------------------------------------

export { AppleError, FOLDERS, linted, type Options, type Part, spawned, target, tokened };
