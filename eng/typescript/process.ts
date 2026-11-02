import { Effect, Schema, Stream } from 'effect';
import { ChildProcess, CommandLine } from 'effect/process';

// --- [ERRORS] --------------------------------------------------------------------------

class ExitError extends Schema.TaggedError<ExitError, { readonly brand: unique symbol }>()('ExitError', { command: Schema.String, args: Schema.Array(Schema.String), code: Schema.Finite }) {
    override get message(): string {
        return `${CommandLine.display([this.command, ...this.args])} exited with code ${this.code}`;
    }
}

// --- [OPERATIONS] ----------------------------------------------------------------------

const output = Effect.fn('output')(function* (command: string, args: readonly string[], options?: ChildProcess.CommandOptions) {
    const handle = yield* ChildProcess.make(command, args, { stdin: 'ignore', stderr: 'inherit', ...options });
    const [text, code] = yield* Effect.all([Stream.mkString(Stream.decodeText(handle.stdout)), handle.exitCode], { concurrency: 'unbounded' });
    return { text, code };
}, Effect.scoped);
const stdout = Effect.fn('stdout')(function* (command: string, args: readonly string[], options?: ChildProcess.CommandOptions) {
    const { text, code } = yield* output(command, args, options);
    return code === 0 ? text : yield* ExitError.make({ command, args, code });
});

// --- [EXPORTS] -------------------------------------------------------------------------

export { output, stdout };
