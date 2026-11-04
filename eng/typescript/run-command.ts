import { Effect, Schema, Stream, String } from 'effect';
import { ChildProcess, CommandLine } from 'effect/process';

// --- [ERRORS] --------------------------------------------------------------------------

class ExitError extends Schema.TaggedError<ExitError, { readonly brand: unique symbol }>()('ExitError', { argv: Schema.NonEmptyArray(Schema.String), code: Schema.Int, output: Schema.String }) {
    override get message(): string {
        return [`${CommandLine.display(this.argv)} exited with code ${this.code}`, this.output.trim()].filter(String.isNonEmpty).join('\n');
    }
}

// --- [OPERATIONS] ----------------------------------------------------------------------

const run = Effect.fn('run')(function* (command: string, args: readonly string[], options?: ChildProcess.CommandOptions) {
    const handle = yield* ChildProcess.make(command, args, { stdin: 'ignore', stderr: 'inherit', ...options });
    const [output, code] = yield* Effect.all([Stream.mkString(Stream.decodeText(handle.stdout)), handle.exitCode], { concurrency: 'unbounded' });
    return code === 0 ? output : yield* ExitError.make({ argv: [command, ...args], code, output });
}, Effect.scoped);
const runCommand = Object.assign(run, {
    records: (separator: string, command: string, args: readonly string[], options?: ChildProcess.CommandOptions) => Effect.map(run(command, args, options), (text) => text.split(separator).filter(String.isNonEmpty)),
    shown: (command: string, args: readonly string[]) => run(command, args, { stdout: 'inherit' }),
});

// --- [EXPORTS] -------------------------------------------------------------------------

export { runCommand };
