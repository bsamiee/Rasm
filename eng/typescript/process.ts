import { Array, Effect, Schema, Stream, String } from 'effect';
import { ChildProcess, CommandLine } from 'effect/process';

// --- [ERRORS] --------------------------------------------------------------------------

class ExitError extends Schema.TaggedError<ExitError, { readonly brand: unique symbol }>()('ExitError', { argv: Schema.NonEmptyArray(Schema.String), code: Schema.Int }) {
    override get message(): string {
        return `${CommandLine.display(this.argv)} exited with code ${this.code}`;
    }
}

// --- [OPERATIONS] ----------------------------------------------------------------------

const output = Effect.fn('output')(function* (command: string, args: readonly string[], options?: ChildProcess.CommandOptions) {
    const handle = yield* ChildProcess.make(command, args, { stdin: 'ignore', stderr: 'inherit', ...options });
    const [text, code] = yield* Effect.all([Stream.mkString(Stream.decodeText(handle.stdout)), handle.exitCode], { concurrency: 'unbounded' });
    return { text, code };
}, Effect.scoped);
const run = Effect.fn('run')(function* (command: string, args: readonly string[], options?: ChildProcess.CommandOptions) {
    const { text, code } = yield* output(command, args, options);
    return code === 0 ? text : yield* ExitError.make({ argv: [command, ...args], code });
});
const records = (separator: '\n' | '\0', command: string, args: readonly string[], options?: ChildProcess.CommandOptions) => Effect.map(run(command, args, options), (text) => Array.filter(String.split(text, separator), String.isNonEmpty));

// --- [EXPORTS] -------------------------------------------------------------------------

export { output, records, run };
