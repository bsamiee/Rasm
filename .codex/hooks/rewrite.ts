import { homedir } from 'node:os';
import process from 'node:process';
import { NodeRuntime, NodeServices } from '@effect/platform-node';
import { Cause, Console, Effect, FileSystem, Match, Option, Path, Schema, SchemaIssue, Stdio, Stream, Struct } from 'effect';
import { ChildProcess, ChildProcessSpawner } from 'effect/process';
import manifest from '../../package.json' with { type: 'json' };
import { fault, none, ok, type Result, some } from '../../plugins/function-hooks/composition.ts';
import { SCAN } from '../../plugins/function-hooks/policies/command.ts';
import { basename, type Invocation } from '../../plugins/function-hooks/policies/invocation.ts';
import { commandDecision, type Decision, type Host } from '../../plugins/function-hooks/policies/policies.ts';
import { catalog } from '../../plugins/function-hooks/policies/targets.ts';

// --- [MODELS] --------------------------------------------------------------------------

const Call = Schema.fromJsonString(Schema.Struct({ cwd: Schema.String, tool_name: Schema.Literal('Bash'), tool_input: Schema.Struct({ command: Schema.String }) }));
const Environment = Schema.fromJsonString(Schema.Struct({ NX_WORKSPACE_DATA_DIRECTORY: Schema.String }));
const Options = Struct.map({ command: Schema.String, commands: Schema.Array(Schema.Union([Schema.String, Schema.Struct({ command: Schema.String, forwardAllArgs: Schema.optionalKey(Schema.Boolean) })])), args: Schema.Union([Schema.String, Schema.Array(Schema.String)]), cwd: Schema.String }, Schema.optionalKey);
const Target = Schema.Struct(Struct.map({ executor: Schema.String, cache: Schema.Boolean, defaultConfiguration: Schema.String, options: Schema.Struct(Options), configurations: Schema.Record(Schema.String, Schema.Struct(Struct.pick(Options, ['args']))), outputs: Schema.Array(Schema.String) }, Schema.optionalKey));
const ProjectGraph = Schema.fromJsonString(Schema.Struct({ nodes: Schema.Record(Schema.String, Schema.Struct({ data: Schema.Struct({ targets: Schema.optionalKey(Schema.Record(Schema.String, Target)) }) })) }));

// --- [OPERATIONS] ----------------------------------------------------------------------

const _decision = Effect.fnUntraced(function* (cwd: string, command: string) {
    const [fs, path, spawner, context] = yield* Effect.all([FileSystem.FileSystem, Path.Path, ChildProcessSpawner.ChildProcessSpawner, Effect.context<NodeServices.NodeServices>()]);
    const run = Effect.runPromiseWith(context);
    const root = path.resolve(import.meta.dirname, '..', '..');
    const exited = ([file, ...rest]: Invocation, input: string): Effect.Effect<Result<string>, never, ChildProcessSpawner.ChildProcessSpawner> =>
        ChildProcess.make(file, rest, { cwd: root, stdin: Stream.encodeText(Stream.make(input)) }).pipe(
            Effect.flatMap((handle) => Effect.all([Stream.mkString(Stream.decodeText(handle.stdout)), Stream.mkString(Stream.decodeText(handle.stderr)), handle.exitCode], { concurrency: 'unbounded' })),
            Effect.scoped,
            Effect.match({ onFailure: (cause) => fault<string>({ kind: 'unstarted', subject: basename(file), cause }), onSuccess: ([stdout, stderr, code]) => (code === 0 ? ok(stdout) : fault<string>({ kind: 'exited', subject: basename(file), code, output: stderr.trim() })) }),
        );
    const [program, ...args] = SCAN;
    const binary = yield* exited(['mise', 'which', program], '');
    const graph = spawner.string(ChildProcess.make('mise', ['env', '--json'], { cwd: root, stderr: 'inherit' })).pipe(
        Effect.flatMap(Schema.decodeEffect(Environment)),
        Effect.flatMap(({ NX_WORKSPACE_DATA_DIRECTORY }) => fs.readFileString(path.join(NX_WORKSPACE_DATA_DIRECTORY, 'project-graph.json'))),
        Effect.flatMap(Schema.decodeEffect(ProjectGraph)),
        Effect.map((held) => some(catalog(held, root))),
        Effect.catchCause((cause) => Effect.as(Console.error(`Nx routing off, ${Cause.pretty(cause)}`), none)),
    );
    const host = {
        scan: (text) => run(binary.kind === 'ok' ? exited([binary.value.trim(), ...args], text) : Effect.succeed(binary)),
        root: () => run(Effect.succeed(some(root))),
        exists: (entry) => run(Effect.isSuccess(fs.stat(entry))),
        cwd: () => run(Effect.succeed(cwd)),
        home: () => run(Effect.succeed(some(homedir()))),
        catalog: () => run(graph),
        read: (file) => run(Effect.match(fs.readFileString(file), { onFailure: () => none, onSuccess: some })),
    } satisfies Host;
    return yield* Effect.promise(() => commandDecision(host, command, manifest.name));
});

// --- [COMPOSITION] ---------------------------------------------------------------------

Effect.gen(function* () {
    const stdio = yield* Stdio.Stdio;
    return yield* Effect.matchEffect(Schema.decodeEffect(Call)(yield* Stream.mkString(Stream.decodeText(stdio.stdin))), {
        onFailure: (error) =>
            Effect.andThen(
                Console.error(`hook input does not hold the Codex PreToolUse form, ${SchemaIssue.makeFormatterDefault()(error.issue)}`),
                Effect.sync(() => {
                    process.exitCode = 2;
                }),
            ),
        onSuccess: Effect.fnUntraced(function* ({ cwd, tool_input: { command } }) {
            const decision = yield* _decision(cwd, command).pipe(Effect.catchCause((cause) => Effect.succeed<Decision>({ kind: 'deny', reason: `function-hooks policy did not run, ${Cause.squash(cause)}` })));
            return yield* Match.value(decision).pipe(
                Match.when({ kind: 'deny' }, ({ reason }) => ({ hookSpecificOutput: { hookEventName: 'PreToolUse', permissionDecision: 'deny', permissionDecisionReason: reason } })),
                Match.when({ rewrite: { kind: 'some' } }, ({ rewrite: { value } }) => ({
                    ...(value.notice.kind === 'some' && { systemMessage: value.notice.value }),
                    hookSpecificOutput: { hookEventName: 'PreToolUse', permissionDecision: 'allow', updatedInput: { command: value.command }, ...(value.notice.kind === 'some' && { additionalContext: value.notice.value }) },
                })),
                Match.option,
                Option.match({ onNone: () => Effect.void, onSome: (output) => Stream.run(Stream.make(JSON.stringify(output)), stdio.stdout()) }),
            );
        }),
    });
}).pipe(Effect.provide(NodeServices.layer), NodeRuntime.runMain);
