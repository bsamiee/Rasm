import { NodeRuntime, NodeServices } from '@effect/platform-node';
import { commandDecision, none, type System, some, unrun } from '@rasm/function-hooks';
import { Cause, Config, Console, Effect, FileSystem, Match, Option, Path, Schema, Stdio, Stream } from 'effect';
import { ChildProcess } from 'effect/process';
import manifest from '#/package.json' with { type: 'json' };

// --- [MODELS] --------------------------------------------------------------------------

const Input = Schema.fromJsonString(Schema.Struct({ cwd: Schema.String, tool_name: Schema.Literal('Bash'), tool_input: Schema.Struct({ command: Schema.String }) }));
const Output = Schema.fromJsonString(
    Schema.Struct({
        systemMessage: Schema.OptionFromOptionalKey(Schema.String),
        hookSpecificOutput: Schema.Union([
            Schema.Struct({ hookEventName: Schema.Literal('PreToolUse'), permissionDecision: Schema.Literal('deny'), permissionDecisionReason: Schema.String }),
            Schema.Struct({ hookEventName: Schema.Literal('PreToolUse'), permissionDecision: Schema.Literal('allow'), updatedInput: Schema.Struct({ command: Schema.String }), additionalContext: Schema.OptionFromOptionalKey(Schema.String) }),
        ]),
    }),
);

// --- [COMPOSITION] ---------------------------------------------------------------------

Effect.gen(function* () {
    const [fs, path, stdio, home, context] = yield* Effect.all([FileSystem.FileSystem, Path.Path, Stdio.Stdio, Config.String('HOME'), Effect.context<NodeServices.NodeServices>()]);
    const run = Effect.runPromiseWith(context);
    const root = path.resolve(import.meta.dirname, '..', '..');
    const spawn = (...[[file, ...rest], { cwd, stdin }]: Parameters<System['run']>) =>
        ChildProcess.make(file, rest, { cwd, stdin: Stream.encodeText(Stream.make(stdin)) }).pipe(
            Effect.flatMap((handle) => Effect.all({ stdout: Stream.mkString(Stream.decodeText(handle.stdout)), stderr: Stream.mkString(Stream.decodeText(handle.stderr)), exitCode: handle.exitCode }, { concurrency: 'unbounded' })),
            Effect.scoped,
        );
    const system = {
        run: (argv, init) => run(spawn(argv, init)),
        exists: (entry) => run(fs.exists(entry)),
        read: (entry) => run(fs.readFileString(entry)),
        scanner: 'ast-grep',
        plugin: path.join(root, 'plugins', 'function-hooks'),
        home: some(home),
    } satisfies Omit<System, 'cwd' | 'root'>;
    const decision = yield* Stream.mkString(Stream.decodeText(stdio.stdin)).pipe(
        Effect.flatMap(Schema.decodeEffect(Input)),
        Effect.flatMap(({ cwd, tool_input: { command } }) => Effect.promise(() => commandDecision({ ...system, cwd, root: path.relative(root, cwd).startsWith('..') ? none : some(root) }, command, manifest.name))),
        Effect.catchCause((cause) => Effect.succeed({ kind: 'deny' as const, reason: unrun(String(Cause.squash(cause))) })),
    );
    const reply = Match.value(decision).pipe(
        Match.discriminatorsExhaustive('kind')({
            deny: ({ reason }) => Option.some({ systemMessage: Option.none(), hookSpecificOutput: { hookEventName: 'PreToolUse' as const, permissionDecision: 'deny' as const, permissionDecisionReason: reason } }),
            allow: () => Option.none(),
            rewrite: ({ command, notice }) => {
                const message = notice.kind === 'some' ? Option.some(notice.value) : Option.none();
                return Option.some({ systemMessage: message, hookSpecificOutput: { hookEventName: 'PreToolUse' as const, permissionDecision: 'allow' as const, updatedInput: { command }, additionalContext: message } });
            },
        }),
    );
    yield* Option.match(reply, { onNone: () => Effect.void, onSome: (value) => Schema.encodeEffect(Output)(value).pipe(Effect.orDie, Effect.flatMap(Console.log)) });
}).pipe(Effect.provide(NodeServices.layer), NodeRuntime.runMain);
