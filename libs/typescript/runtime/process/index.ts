import { constants, userInfo } from 'node:os';
import { Data, Effect, Record, Schema, Stream } from 'effect';
import { ChildProcess, type ChildProcessSpawner, CommandLine } from 'effect/process';

// --- [TYPES] ---------------------------------------------------------------------------

interface Launchctl {
    readonly absent: readonly number[];
    readonly argv: (domain: string, subject: string) => readonly string[];
}

// --- [CONSTANTS] -----------------------------------------------------------------------

const UNKNOWN_SERVICE = 113;

// --- [MODELS] --------------------------------------------------------------------------

class Completed extends Data.Class<{ readonly command: ChildProcess.Command; readonly code: ChildProcessSpawner.ExitCode; readonly stdout: string; readonly stderr: string }> {
    static readonly run = Effect.fn('Completed.run')(function* (command: ChildProcess.Command) {
        const handle = yield* command;
        const [stdout, stderr, code] = yield* Effect.all([Stream.mkString(Stream.decodeText(handle.stdout)), Stream.mkString(Stream.decodeText(handle.stderr)), handle.exitCode], { concurrency: 'unbounded' });
        return new Completed({ command, code, stdout, stderr });
    }, Effect.scoped);

    static readonly output = (command: ChildProcess.Command) => Effect.flatMap(Completed.run(command), (completed) => completed.checked);

    get checked(): Effect.Effect<string, ExitError> {
        return this.code === 0 ? Effect.succeed(this.stdout) : Effect.fail(ExitError.make({ command: displayed(this.command), code: this.code, stdout: this.stdout, stderr: this.stderr }));
    }
}

// --- [ERRORS] --------------------------------------------------------------------------

class ExitError extends Schema.TaggedError<ExitError, { readonly brand: unique symbol }>()('ExitError', { command: Schema.String, code: Schema.Int, stdout: Schema.String, stderr: Schema.String }) {
    override get message(): string {
        return `${this.command} exited ${this.code}: ${this.stderr.trim()}`;
    }
}

// --- [OPERATIONS] ----------------------------------------------------------------------

const displayed = (command: ChildProcess.Command): string => (ChildProcess.isStandardCommand(command) ? CommandLine.display([command.command, ...command.args]) : `${displayed(command.left)} | ${displayed(command.right)}`);

const LoginAgent = Record.map(
    {
        loaded: { absent: [UNKNOWN_SERVICE], argv: (domain, label) => ['print', `${domain}/${label}`] },
        stop: { absent: [constants.errno.ESRCH], argv: (domain, label) => ['bootout', `${domain}/${label}`] },
        restart: { absent: [UNKNOWN_SERVICE], argv: (domain, label) => ['kickstart', '-k', `${domain}/${label}`] },
        start: { absent: [], argv: (domain, propertyList) => ['bootstrap', domain, propertyList] },
    } satisfies Record<string, Launchctl>,
    ({ absent, argv }: Launchctl, operation) =>
        Effect.fn(`LoginAgent.${operation}`)((subject: string) =>
            Completed.output(ChildProcess.make('launchctl', argv(`gui/${userInfo().uid}`, subject), { stdin: 'ignore' })).pipe(
                Effect.as(true),
                Effect.catchTag('ExitError', (error) => (absent.includes(error.code) ? Effect.succeed(false) : Effect.fail(error))),
            ),
        ),
);

// --- [EXPORTS] -------------------------------------------------------------------------

export { Completed, ExitError, LoginAgent };
