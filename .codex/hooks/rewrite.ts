import { homedir } from 'node:os';
import { NodeRuntime, NodeServices } from '@effect/platform-node';
import { Cause, Console, Effect, FileSystem, flow, Match, Option, Path, Schema, Stdio, Stream } from 'effect';
import { ChildProcess } from 'effect/process';
import manifest from '../../package.json' with { type: 'json' };
import { some } from '../../plugins/function-hooks/composition.ts';
import { host, type System, unrun } from '../../plugins/function-hooks/host.ts';
import { commandDecision, type Decision } from '../../plugins/function-hooks/policies/policies.ts';

// --- [MODELS] --------------------------------------------------------------------------

const Call = Schema.fromJsonString(Schema.Struct({ cwd: Schema.String, tool_name: Schema.Literal('Bash'), tool_input: Schema.Struct({ command: Schema.String }) }));

// --- [OPERATIONS] ----------------------------------------------------------------------

const _decision = Effect.fnUntraced(function* ({ cwd, tool_input: { command } }: typeof Call.Type) {
    const [fs, path, context] = yield* Effect.all([FileSystem.FileSystem, Path.Path, Effect.context<NodeServices.NodeServices>()]);
    const run = Effect.runPromiseWith(context);
    const root = some(path.resolve(import.meta.dirname, '..', '..'));
    const system = {
        run: ([file, ...rest], init) =>
            run(
                ChildProcess.make(file, rest, { cwd: init.cwd, stdin: Stream.encodeText(Stream.make(init.stdin)) }).pipe(
                    Effect.flatMap((handle) => Effect.all({ stdout: Stream.mkString(Stream.decodeText(handle.stdout)), stderr: Stream.mkString(Stream.decodeText(handle.stderr)), exitCode: handle.exitCode }, { concurrency: 'unbounded' })),
                    Effect.scoped,
                ),
            ),
        exists: (entry) => run(fs.exists(entry)),
        root: () => run(Effect.succeed(root)),
        cwd: () => run(Effect.succeed(cwd)),
        home: () => run(Effect.succeed(some(homedir()))),
    } satisfies System;
    return yield* Effect.promise(() => commandDecision(host(system), command, manifest.name));
});

// --- [COMPOSITION] ---------------------------------------------------------------------

Effect.gen(function* () {
    const decision = yield* Stream.mkString(Stream.decodeText((yield* Stdio.Stdio).stdin)).pipe(
        Effect.flatMap(Schema.decodeEffect(Call)),
        Effect.flatMap(_decision),
        Effect.catchCause((cause) => Effect.succeed<Decision>({ kind: 'deny', reason: unrun(String(Cause.squash(cause))) })),
    );
    return yield* Match.value(decision).pipe(
        Match.when({ kind: 'deny' }, ({ reason }) => ({ hookSpecificOutput: { hookEventName: 'PreToolUse', permissionDecision: 'deny', permissionDecisionReason: reason } })),
        Match.when({ rewrite: { kind: 'some' } }, ({ rewrite: { value } }) => ({
            ...(value.notice.kind === 'some' && { systemMessage: value.notice.value }),
            hookSpecificOutput: { hookEventName: 'PreToolUse', permissionDecision: 'allow', updatedInput: { command: value.command }, ...(value.notice.kind === 'some' && { additionalContext: value.notice.value }) },
        })),
        Match.option,
        Option.match({ onNone: () => Effect.void, onSome: flow(JSON.stringify, Console.log) }),
    );
}).pipe(Effect.provide(NodeServices.layer), NodeRuntime.runMain);
