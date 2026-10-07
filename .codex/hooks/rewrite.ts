import { homedir } from 'node:os';
import { parseArgs } from 'node:util';
import { NodeRuntime, NodeServices } from '@effect/platform-node';
import { Effect, FileSystem, Path, Schema, Stdio, Stream } from 'effect';
import { ChildProcess, ChildProcessSpawner } from 'effect/process';
import { SCAN } from '../../plugins/function-hooks/hooks/command.ts';
import { fault, none, ok, type Result, some } from '../../plugins/function-hooks/hooks/composition.ts';
import type { Invocation } from '../../plugins/function-hooks/hooks/invocation.ts';
import { commandDecision, type Decision, type Host, pathRefusal } from '../../plugins/function-hooks/hooks/policies.ts';

// --- [CONSTANTS] -----------------------------------------------------------------------

const _ADDED = /^\*\*\* Add File: (?<path>.+)$/gmu;

// --- [MODELS] --------------------------------------------------------------------------

const Call = Schema.Struct({ tool_name: Schema.Literals(['Bash', 'apply_patch']), tool_input: Schema.Struct({ command: Schema.String }) });

// --- [SERVICES] ------------------------------------------------------------------------

const _host = Effect.gen(function* () {
    const fs = yield* FileSystem.FileSystem;
    const path = yield* Path.Path;
    const spawner = yield* ChildProcessSpawner.ChildProcessSpawner;
    const ran = ([program, ...args]: Invocation, input: string): Effect.Effect<Result<string>> =>
        Effect.scoped(
            Effect.gen(function* () {
                const handle = yield* spawner.spawn(ChildProcess.make(program, args, { stdin: Stream.make(input).pipe(Stream.encodeText) }));
                const [stdout, stderr, code] = yield* Effect.all([Stream.mkString(Stream.decodeText(handle.stdout)), Stream.mkString(Stream.decodeText(handle.stderr)), handle.exitCode], { concurrency: 'unbounded' });
                return code === 0 ? ok(stdout) : fault<string>({ kind: 'exited', subject: program, code, stderr: stderr.trim() });
            }),
        ).pipe(Effect.orElseSucceed((cause) => fault<string>({ kind: 'unstarted', subject: program, cause })));
    const host: Host = {
        scan: (text) => ran(SCAN, text).pipe(Effect.runPromise),
        read: (file) => fs.readFileString(file).pipe(Effect.match({ onFailure: (cause) => fault<string>({ kind: 'unread', subject: file, cause }), onSuccess: ok }), Effect.runPromise),
        repo: () =>
            ran(['git', 'rev-parse', '--path-format=absolute', '--git-common-dir'], '').pipe(
                Effect.map((found) => (found.kind === 'ok' ? some(path.dirname(found.value.trim())) : none)),
                Effect.runPromise,
            ),
        make: (folders) => Effect.forEach(folders, (folder) => fs.makeDirectory(folder, { recursive: true })).pipe(Effect.match({ onFailure: (cause) => fault<unknown>({ kind: 'unwritten', subject: folders.join(' '), cause }), onSuccess: ok }), Effect.runPromise),
        exists: (file) => fs.exists(file).pipe(Effect.runPromise),
        real: (file) => fs.realPath(file).pipe(Effect.match({ onFailure: () => none, onSuccess: some }), Effect.runPromise),
        home: async () => some(homedir()),
    };
    return host;
});

// --- [OPERATIONS] ----------------------------------------------------------------------

const _decision = (host: Host, { tool_name, tool_input: { command } }: typeof Call.Type, walking: boolean): Effect.Effect<Decision> => {
    if (tool_name === 'Bash') {
        return Effect.promise(() => commandDecision(host, 'Bash', command, walking));
    }
    const refusal = pathRefusal([...command.matchAll(_ADDED)].flatMap(({ groups }) => groups?.path ?? []));
    return Effect.succeed(refusal.kind === 'some' ? { kind: 'deny', reason: refusal.value } : { kind: 'pass' });
};

const _answer = (decision: Exclude<Decision, { readonly kind: 'pass' }>): object =>
    decision.kind === 'deny'
        ? { hookSpecificOutput: { hookEventName: 'PreToolUse', permissionDecision: 'deny', permissionDecisionReason: decision.reason } }
        : { systemMessage: decision.note, hookSpecificOutput: { hookEventName: 'PreToolUse', permissionDecision: 'allow', updatedInput: { command: decision.command }, additionalContext: decision.context } };

// --- [COMPOSITION] ---------------------------------------------------------------------

Effect.gen(function* () {
    const stdio = yield* Stdio.Stdio;
    const call = yield* stdio.stdin.pipe(Stream.decodeText(), Stream.mkString, Effect.flatMap(Schema.decodeUnknownEffect(Schema.fromJsonString(Call))));
    const decision = yield* _decision(yield* _host, call, parseArgs({ options: { 'walk-policy': { type: 'boolean', default: false } } }).values['walk-policy']);
    if (decision.kind !== 'pass') {
        yield* Stream.run(Stream.succeed(JSON.stringify(_answer(decision))), stdio.stdout());
    }
}).pipe(Effect.provide(NodeServices.layer), NodeRuntime.runMain);
