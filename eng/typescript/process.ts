import { Effect, Schema, Stream } from 'effect';
import { ChildProcess } from 'effect/process';

// --- [ERRORS] --------------------------------------------------------------------------

class ExitError extends Schema.TaggedError<ExitError, { readonly brand: unique symbol }>()('ExitError', { command: Schema.String, args: Schema.Array(Schema.String), code: Schema.Finite }) {
    override get message(): string {
        return `${[this.command, ...this.args].join(' ')} exited with code ${this.code}`;
    }
}

// --- [OPERATIONS] ----------------------------------------------------------------------

const stdout = Effect.fn('stdout')(function* (command: string, args: readonly string[], options?: ChildProcess.CommandOptions) {
    const handle = yield* ChildProcess.make(command, args, { stdin: 'ignore', stderr: 'inherit', ...options });
    const [text, code] = yield* Effect.all([Stream.mkString(Stream.decodeText(handle.stdout)), handle.exitCode], { concurrency: 'unbounded' });
    return code === 0 ? text : yield* ExitError.make({ command, args, code });
}, Effect.scoped);

// --- [EXPORTS] -------------------------------------------------------------------------

export { stdout };
