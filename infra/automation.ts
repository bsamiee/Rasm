// Automation API entry that selects the stack on a file backend, resolves credentials, and runs preview, up, and refresh

// --- [IMPORTS] -------------------------------------------------------------------------

import { homedir } from 'node:os';
import * as path from 'node:path';
import { Command, FileSystem, Terminal } from '@effect/platform';
import { NodeContext, NodeRuntime } from '@effect/platform-node';
import { LocalWorkspace, type Stack } from '@pulumi/pulumi/automation/index.js';
import { Array, Cause, Config, Console, Data, Effect, Option, Redacted, Schema, Stream } from 'effect';
import { program } from './program.ts';

// --- [CONSTANTS] -----------------------------------------------------------------------

const PROJECT = 'rasm-infra';
const STACK = 'rasm';
const PASSPHRASE_REF = 'op://Tokens/PULUMI_RASM_INFRA/password';
const GITHUB_TOKEN_REF = 'op://Tokens/GITHUB_TOKEN/token';
const USAGE = 'node infra/automation.ts preview|up|refresh [--import] [--refresh] [--expect-no-changes]';

// --- [TYPES] ---------------------------------------------------------------------------

const _Subcommand = Schema.Literal('preview', 'up', 'refresh');

type Subcommand = typeof _Subcommand.Type;

type Flags = {
    readonly import: boolean;
    readonly refresh: boolean;
    readonly expectNoChanges: boolean;
};

// --- [ERRORS] --------------------------------------------------------------------------

class ShellError extends Data.TaggedError('ShellError')<{
    readonly command: string;
    readonly cause: string;
    readonly action: string;
}> {
    override get message(): string {
        return `${this.command} failed, ${this.cause}, ${this.action}`;
    }
}

class StackError extends Data.TaggedError('StackError')<{
    readonly operation: string;
    readonly cause: string;
}> {
    override get message(): string {
        return `stack ${this.operation} failed, ${this.cause}, correct the program, the state, or the credential the Pulumi diagnostics name`;
    }
}

class UsageError extends Data.TaggedError('UsageError')<{
    readonly problem: string;
}> {
    override get message(): string {
        return `${this.problem}, the entry takes ${USAGE}, rerun with a listed subcommand and flags`;
    }
}

class Declined extends Data.TaggedError('Declined') {
    override get message(): string {
        return 'up declined at the prompt, no resource changed, rerun up and answer y to apply the plan';
    }
}

// --- [SHELL] ---------------------------------------------------------------------------

const _shell = (action: string, command: string, ...args: ReadonlyArray<string>) =>
    Effect.gen(function* () {
        const label = `${command} ${args.join(' ')}`;
        const spawned = yield* Command.start(Command.make(command, ...args));
        const [code, out, err] = yield* Effect.all(
            [spawned.exitCode, Stream.mkString(Stream.decodeText(spawned.stdout)), Stream.mkString(Stream.decodeText(spawned.stderr))],
            { concurrency: 3 },
        );
        return yield* code === 0 ? Effect.succeed(out.trim()) : new ShellError({ command: label, cause: err.trim() || `exit code ${code}`, action });
    }).pipe(
        Effect.scoped,
        Effect.catchTag('BadArgument', 'SystemError', (fault) => new ShellError({ command, cause: fault.message, action })),
    );

// --- [CREDENTIALS] ---------------------------------------------------------------------

// An ambient value short-circuits the resolution, and the state directory follows XDG with the default the specification names
const _settings = Config.all({
    passphrase: Config.option(Config.redacted('PULUMI_CONFIG_PASSPHRASE')),
    dopplerToken: Config.option(Config.redacted('DOPPLER_TOKEN')),
    githubToken: Config.option(Config.redacted('GITHUB_TOKEN')),
    ghToken: Config.option(Config.redacted('GH_TOKEN')),
    stateDir: Config.nonEmptyString('XDG_STATE_HOME').pipe(
        Config.map((root) => path.join(root, PROJECT)),
        Config.withDefault(path.join(homedir(), '.local', 'state', PROJECT)),
    ),
});

const _resolved = <R>(ambient: Option.Option<Redacted.Redacted<string>>, resolve: Effect.Effect<string, ShellError, R>) =>
    Option.match(ambient, { onSome: Effect.succeed, onNone: () => Effect.map(resolve, Redacted.make) });

const _credentials = Effect.flatMap(_settings, (cfg) =>
    Effect.all(
        {
            stateDir: Effect.succeed(cfg.stateDir),
            passphrase: _resolved(
                cfg.passphrase,
                _shell(
                    'export PULUMI_CONFIG_PASSPHRASE or create the PULUMI_RASM_INFRA password item in the Tokens vault',
                    'op',
                    'read',
                    PASSPHRASE_REF,
                ),
            ),
            dopplerToken: _resolved(
                cfg.dopplerToken,
                _shell('run doppler login or export DOPPLER_TOKEN', 'doppler', 'configure', 'get', 'token', '--plain'),
            ),
            githubToken: _resolved(
                Option.orElse(cfg.githubToken, () => cfg.ghToken),
                _shell('export GITHUB_TOKEN or create the GITHUB_TOKEN item in the Tokens vault', 'op', 'read', GITHUB_TOKEN_REF),
            ),
        },
        { concurrency: 3 },
    ),
);

// --- [STACK] ---------------------------------------------------------------------------

const _selectStack = (flags: Flags) =>
    Effect.gen(function* () {
        const credentials = yield* _credentials;
        const fs = yield* FileSystem.FileSystem;
        yield* fs.makeDirectory(credentials.stateDir, { recursive: true });
        yield* fs.chmod(credentials.stateDir, 0o700);
        const backendUrl = `file://${credentials.stateDir}`;
        return yield* Effect.tryPromise({
            // BOUNDARY: the Automation API is promise-native, and secrets unwrap only into the engine's environment and the provider inputs
            try: () =>
                LocalWorkspace.createOrSelectStack(
                    {
                        stackName: STACK,
                        projectName: PROJECT,
                        program: program(flags, { dopplerToken: credentials.dopplerToken, githubToken: credentials.githubToken }),
                    },
                    {
                        projectSettings: { name: PROJECT, runtime: 'nodejs', backend: { url: backendUrl } },
                        secretsProvider: 'passphrase',
                        envVars: { PULUMI_CONFIG_PASSPHRASE: Redacted.value(credentials.passphrase), PULUMI_BACKEND_URL: backendUrl },
                    },
                ),
            catch: (cause) => new StackError({ operation: 'select', cause: String(cause) }),
        });
    });

const _operation = <A>(operation: string, run: () => Promise<A>) =>
    Effect.tryPromise({ try: run, catch: (cause) => new StackError({ operation, cause: String(cause) }) });

// BOUNDARY: the engine streams its output to a void callback
const _echo = (chunk: string): void => {
    process.stdout.write(chunk);
};

// --refresh diffs against refreshed live state, and --expect-no-changes turns a steady state into a gate
const _modes = (flags: Flags): { readonly refresh?: true; readonly expectNoChanges?: true } => ({
    ...(flags.refresh ? { refresh: true as const } : {}),
    ...(flags.expectNoChanges ? { expectNoChanges: true as const } : {}),
});

const _preview = (stack: Stack, flags: Flags) =>
    Effect.tap(
        _operation('preview', () => stack.preview({ diff: true, onOutput: _echo, ..._modes(flags) })),
        (result) => Console.log(JSON.stringify(result.changeSummary)),
    );

const _confirmed = Effect.flatMap(Terminal.Terminal, (terminal) =>
    Effect.map(
        Effect.zipRight(
            terminal.display('Apply this plan? [y/N] '),
            Effect.catchTag(terminal.readLine, 'QuitException', () => Effect.succeed('')),
        ),
        (answer) => answer.trim().toLowerCase() === 'y',
    ),
);

// --- [SUBCOMMANDS] ---------------------------------------------------------------------

const _subcommands = {
    preview: (flags: Flags) => Effect.asVoid(Effect.flatMap(_selectStack(flags), (stack) => _preview(stack, flags))),
    up: (flags: Flags) =>
        Effect.gen(function* () {
            const stack = yield* _selectStack(flags);
            yield* _preview(stack, flags);
            const confirmed = yield* _confirmed;
            const result = yield* confirmed ? _operation('up', () => stack.up({ onOutput: _echo, ..._modes(flags) })) : new Declined();
            yield* Console.log(JSON.stringify(result.summary.resourceChanges ?? {}));
        }),
    refresh: (flags: Flags) =>
        Effect.flatMap(
            Effect.flatMap(_selectStack(flags), (stack) =>
                _operation('refresh', () => stack.refresh({ onOutput: _echo, ..._modes({ ...flags, refresh: false }) })),
            ),
            (result) => Console.log(JSON.stringify(result.summary.resourceChanges ?? {})),
        ),
} satisfies Record<Subcommand, (flags: Flags) => Effect.Effect<void, unknown, NodeContext.NodeContext>>;

// --- [ENTRY] ---------------------------------------------------------------------------

const _FLAGS = { import: '--import', refresh: '--refresh', expectNoChanges: '--expect-no-changes' } as const;

const _main = Effect.gen(function* () {
    const argv = process.argv.slice(2);
    const positional = Array.filter(argv, (arg) => !arg.startsWith('--'));
    const stray = Array.findFirst(argv, (arg) => arg.startsWith('--') && !Array.contains(Object.values(_FLAGS), arg));
    const flags: Flags = {
        import: Array.contains(argv, _FLAGS.import),
        refresh: Array.contains(argv, _FLAGS.refresh),
        expectNoChanges: Array.contains(argv, _FLAGS.expectNoChanges),
    };
    return yield* Option.match(stray, {
        onSome: (flag) => new UsageError({ problem: `unknown flag ${flag}` }),
        onNone: () =>
            Option.match(Schema.decodeUnknownOption(_Subcommand)(positional[0]), {
                onNone: () => new UsageError({ problem: `no subcommand among ${positional.join(' ') || '(none)'}` }),
                onSome: (subcommand) => _subcommands[subcommand](flags),
            }),
    });
});

// A typed failure prints its one message, and a defect prints its full cause
NodeRuntime.runMain(
    Effect.tapErrorCause(Effect.provide(_main, NodeContext.layer), (cause) =>
        Console.error(Option.match(Cause.failureOption(cause), { onNone: () => Cause.pretty(cause), onSome: (error) => error.message })),
    ),
    { disableErrorReporting: true },
);
