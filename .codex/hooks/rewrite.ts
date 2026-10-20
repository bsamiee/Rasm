import { homedir } from 'node:os';
import { NodeRuntime, NodeServices } from '@effect/platform-node';
import { Cause, Console, Effect, FileSystem, Match, Path, Schema, Stdio, Stream } from 'effect';
import { ChildProcess } from 'effect/process';
import manifest from '../../package.json' with { type: 'json' };
import { none, some } from '../../plugins/function-hooks/composition.ts';
import { commandDecision, type System, unrun } from '../../plugins/function-hooks/policies/policies.ts';

// --- [COMPOSITION] ---------------------------------------------------------------------

Effect.gen(function* () {
    const [fs, path, stdio, context] = yield* Effect.all([FileSystem.FileSystem, Path.Path, Stdio.Stdio, Effect.context<NodeServices.NodeServices>()]);
    const run = Effect.runPromiseWith(context);
    const spawn = (...[[file, ...rest], { cwd, stdin }]: Parameters<System['run']>) =>
        ChildProcess.make(file, rest, { cwd, stdin: Stream.encodeText(Stream.make(stdin)) }).pipe(
            Effect.flatMap((handle) => Effect.all({ stdout: Stream.mkString(Stream.decodeText(handle.stdout)), stderr: Stream.mkString(Stream.decodeText(handle.stderr)), exitCode: handle.exitCode }, { concurrency: 'unbounded' })),
            Effect.scoped,
        );
    const system = {
        run: (argv, init) => run(spawn(argv, init)),
        exists: (entry) => run(fs.exists(entry)),
        plugin: path.resolve(import.meta.dirname, '..', '..', 'plugins', 'function-hooks'),
        home: some(homedir()),
    } satisfies Omit<System, 'cwd' | 'root'>;
    const decision = yield* Effect.gen(function* () {
        const {
            cwd,
            tool_input: { command },
        } = yield* Stream.mkString(Stream.decodeText(stdio.stdin)).pipe(Effect.flatMap(Schema.decodeEffect(Schema.fromJsonString(Schema.Struct({ cwd: Schema.String, tool_name: Schema.Literal('Bash'), tool_input: Schema.Struct({ command: Schema.String }) })))));
        const { exitCode, stdout } = yield* spawn(['git', 'rev-parse', '--show-toplevel'], { cwd, stdin: '' });
        return yield* Effect.promise(() => commandDecision({ ...system, cwd, root: exitCode === 0 ? some(stdout.trimEnd()) : none }, command, manifest.name));
    }).pipe(Effect.catchCause((cause) => Effect.succeed({ kind: 'deny' as const, reason: unrun(String(Cause.squash(cause))) })));
    yield* Match.value(decision).pipe(
        Match.discriminatorsExhaustive('kind')({
            deny: ({ reason }) => Console.log(JSON.stringify({ hookSpecificOutput: { hookEventName: 'PreToolUse', permissionDecision: 'deny', permissionDecisionReason: reason } })),
            allow: () => Effect.void,
            rewrite: ({ command, notice }) =>
                Console.log(
                    JSON.stringify({
                        ...(notice.kind === 'some' && { systemMessage: notice.value }),
                        hookSpecificOutput: { hookEventName: 'PreToolUse', permissionDecision: 'allow', updatedInput: { command }, ...(notice.kind === 'some' && { additionalContext: notice.value }) },
                    }),
                ),
        }),
    );
}).pipe(Effect.provide(NodeServices.layer), NodeRuntime.runMain);
