import { Array, Cause, Duration, Effect, Option, Record, Schema, Stream, String } from 'effect';
import { HttpTraceContext } from 'effect/http';
import { ChildProcess, CommandLine } from 'effect/process';

// --- [ERRORS] --------------------------------------------------------------------------

class ExitError extends Schema.TaggedError<ExitError, { readonly brand: unique symbol }>()('ExitError', { command: Schema.String, code: Schema.Int, stdout: Schema.String, stderr: Schema.String, message: Schema.String }) {
    static readonly of = (exited: Pick<ExitError, 'command' | 'code' | 'stdout' | 'stderr'>) => ExitError.make({ ...exited, message: [`${exited.command} exited ${exited.code}`, ...Option.toArray(Array.findFirst([exited.stderr.trim(), exited.stdout.trim()], String.isNonEmpty))].join(': ') });
}

// --- [OPERATIONS] ----------------------------------------------------------------------

const spawn = Effect.fn('spawn')(function* (argv: Array.NonEmptyReadonlyArray<string>, options?: ChildProcess.CommandOptions) {
    const traced = Option.match(yield* Effect.option(Effect.currentSpan), { onNone: () => ({}), onSome: (span) => Record.mapKeys(HttpTraceContext.toHeaders(span), String.toUpperCase) });
    return yield* ChildProcess.make(Array.headNonEmpty(argv), Array.tailNonEmpty(argv), { extendEnv: true, ...options, env: { ...traced, ...options?.env } });
});
const run = Effect.fn('run')(function* (argv: Array.NonEmptyReadonlyArray<string>, { within = Duration.infinity, ...options }: ChildProcess.CommandOptions & { readonly within?: Duration.Input | undefined } = {}) {
    const command = CommandLine.display(argv);
    const deadline = Duration.fromInputUnsafe(within);
    const bounded = Duration.isFinite(deadline);
    const handle = yield* spawn(argv, { stdin: 'ignore', forceKillAfter: bounded ? '100 millis' : undefined, ...options, detached: bounded ? undefined : false });
    const [stdout, stderr, code] = yield* Effect.all([handle.stdout.pipe(Stream.decodeText, Stream.mkString), handle.stderr.pipe(Stream.decodeText, Stream.mkString), handle.exitCode], { concurrency: 'unbounded' }).pipe(
        Effect.timeoutOrElse({ duration: deadline, orElse: () => new Cause.TimeoutError(`${command} ran past ${Duration.format(deadline)}`) }),
    );
    const exited = { command, code, stdout, stderr };
    return code === 0 ? exited : yield* ExitError.of(exited);
}, Effect.scoped);

// --- [EXPORTS] -------------------------------------------------------------------------

export { ExitError, run, spawn };
