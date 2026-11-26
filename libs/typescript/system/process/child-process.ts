import { toEnvironment } from '@rasm/telemetry/node';
import { Array, Effect, Option, Schema, Stream, String } from 'effect';
import { ChildProcess, CommandLine } from 'effect/process';

// --- [ERRORS] --------------------------------------------------------------------------

class ExitError extends Schema.TaggedError<ExitError, { readonly brand: unique symbol }>()('ExitError', { command: Schema.String, code: Schema.Int, stdout: Schema.String, stderr: Schema.String, message: Schema.String }) {
    static readonly of = (exited: Pick<ExitError, 'command' | 'code' | 'stdout' | 'stderr'>) => ExitError.make({ ...exited, message: [`${exited.command} exited ${exited.code}`, ...Option.toArray(Array.findFirst([exited.stderr.trim(), exited.stdout.trim()], String.isNonEmpty))].join(': ') });
}

// --- [OPERATIONS] ----------------------------------------------------------------------

const spawn = Effect.fn('spawn')(function* (argv: Array.NonEmptyReadonlyArray<string>, options?: ChildProcess.CommandOptions) {
    const traced = yield* toEnvironment;
    return yield* ChildProcess.make(Array.headNonEmpty(argv), Array.tailNonEmpty(argv), { extendEnv: true, ...options, env: { ...traced, ...options?.env } });
});
const run = Effect.fn('run')(function* (argv: Array.NonEmptyReadonlyArray<string>, options?: ChildProcess.CommandOptions) {
    const handle = yield* spawn(argv, { stdin: 'ignore', ...options, detached: false });
    const [stdout, stderr, code] = yield* Effect.all([Stream.mkString(Stream.decodeText(handle.stdout)), Stream.mkString(Stream.decodeText(handle.stderr)), handle.exitCode], { concurrency: 'unbounded' });
    return code === 0 ? { stdout, stderr } : yield* ExitError.of({ command: CommandLine.display(argv), code, stdout, stderr });
}, Effect.scoped);

// --- [EXPORTS] -------------------------------------------------------------------------

export { ExitError, run, spawn };
