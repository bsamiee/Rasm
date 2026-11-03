import { NodeRuntime, NodeServices } from '@effect/platform-node';
import { commandDecision, none, type System, some, unrun } from '@rasm/function-hooks';
import { Cause, Config, Console, Effect, FileSystem, Match, Path, Schema, Stdio, Stream } from 'effect';
import { ChildProcess } from 'effect/process';
import manifest from '#/package.json' with { type: 'json' };

// --- [MODELS] --------------------------------------------------------------------------

const Input = Schema.fromJsonString(Schema.Struct({ cwd: Schema.String, tool_name: Schema.Literal('Bash'), tool_input: Schema.Struct({ command: Schema.String }) }));

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
    yield* Match.value(decision).pipe(
        Match.discriminatorsExhaustive('kind')({
            deny: ({ reason }) => Console.log(JSON.stringify({ hookSpecificOutput: { hookEventName: 'PreToolUse', permissionDecision: 'deny', permissionDecisionReason: reason } })),
            allow: () => Effect.void,
            rewrite: ({ command, notice }) => Console.log(JSON.stringify({ ...(notice.kind === 'some' && { systemMessage: notice.value }), hookSpecificOutput: { hookEventName: 'PreToolUse', permissionDecision: 'allow', updatedInput: { command }, ...(notice.kind === 'some' && { additionalContext: notice.value }) } })),
        }),
    );
}).pipe(Effect.provide(NodeServices.layer), NodeRuntime.runMain);
