import { execFile, spawnSync } from 'node:child_process';
import { readFile } from 'node:fs/promises';
import { dirname } from 'node:path';
import { promisify } from 'node:util';
import { NodeRuntime, NodeStdio } from '@effect/platform-node';
import { Effect, Schema, Stdio, Stream } from 'effect';
import { parse, SCAN, type Scanner } from '../../plugins/function-hooks/hooks/command.ts';
import { fault, none, type Option, ok, type Result, rendered, some } from '../../plugins/function-hooks/hooks/composition.ts';
import type { Invocation } from '../../plugins/function-hooks/hooks/invocation.ts';
import { commandRewrite, locks } from '../../plugins/function-hooks/hooks/policies.ts';

// --- [TYPES] ---------------------------------------------------------------------------

type Answer = { readonly permissionDecision: 'deny'; readonly permissionDecisionReason: string } | { readonly permissionDecision: 'allow'; readonly updatedInput: { readonly command: string }; readonly additionalContext: string };

// --- [MODELS] --------------------------------------------------------------------------

const Call = Schema.Struct({ tool_input: Schema.Struct({ command: Schema.String }) });

// --- [OPERATIONS] ----------------------------------------------------------------------

const _run = (argv: Invocation, stdin?: string): Result<string> => {
    const { error, status, signal, stdout, stderr } = spawnSync(argv[0], argv.slice(1), { input: stdin, encoding: 'utf8' });
    if (error !== undefined) {
        return fault({ kind: 'unstarted', subject: argv[0], cause: error });
    }
    return status === 0 ? ok(stdout) : fault({ kind: 'exited', subject: argv[0], code: status ?? String(signal), stderr: stderr.trim() });
};

const _decision = Effect.fnUntraced(function* (command: string) {
    const scan: Scanner = async (input) => _run(SCAN, input);
    const read = (path: string): Promise<Result<string>> => readFile(path, 'utf8').then(ok, (cause: unknown) => fault<string>({ kind: 'unread', subject: path, cause }));
    const exec = promisify(execFile);
    const repo = (): Promise<Option<string>> =>
        exec('git', ['rev-parse', '--path-format=absolute', '--git-common-dir']).then(
            ({ stdout }) => some(dirname(stdout.trim())),
            () => none,
        );
    const make = (folders: readonly string[]): Promise<Result<unknown>> => exec('mkdir', ['-p', ...folders]).then(ok, (cause: unknown) => fault({ kind: 'unwritten', subject: folders.join(' '), cause }));
    const parsed = yield* Effect.promise(() => parse(scan, command));
    if (parsed.kind === 'fault') {
        return some<Answer>({ permissionDecision: 'deny', permissionDecisionReason: `command not parsed, ${rendered(parsed.fault)}` });
    }
    const held = yield* Effect.promise(() => locks(scan, read, repo, make, parsed.value.commands));
    if (held.kind === 'fault') {
        return some<Answer>({ permissionDecision: 'deny', permissionDecisionReason: `command not queued, ${rendered(held.fault)}` });
    }
    const rewrite = commandRewrite(parsed.value.commands, command, held.value);
    return rewrite.kind === 'none' ? none : some<Answer>({ permissionDecision: 'allow', updatedInput: { command: rewrite.value.command }, additionalContext: rewrite.value.context });
});

// --- [COMPOSITION] ---------------------------------------------------------------------

Effect.gen(function* () {
    const stdio = yield* Stdio.Stdio;
    const call = yield* stdio.stdin.pipe(Stream.decodeText(), Stream.mkString, Effect.flatMap(Schema.decodeUnknownEffect(Schema.fromJsonString(Call))));
    const answer = yield* _decision(call.tool_input.command);
    if (answer.kind === 'some') {
        yield* Stream.run(Stream.succeed(JSON.stringify({ hookSpecificOutput: { hookEventName: 'PreToolUse', ...answer.value } })), stdio.stdout());
    }
}).pipe(Effect.provide(NodeStdio.layer), NodeRuntime.runMain);
