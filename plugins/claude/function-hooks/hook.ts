import { userInfo } from 'node:os';
import { basename } from 'node:path';
import { runMain } from '@effect/platform-node/NodeRuntime';
import { layer } from '@effect/platform-node/NodeServices';
import { layerNodeFromConfig, teardown } from '@rasm/telemetry/node';
import { Array, Cause, Console, Effect, Function, Layer, Option, Schema, Stdio, Stream, Struct } from 'effect';
import manifest from '#/manifest' with { type: 'json' };
import { decide, due, Engine } from '#/rules/decide';
import { Decision, type Request } from '#/rules/decision';

// --- [CONSTANTS] -----------------------------------------------------------------------

const EVENT = 'PreToolUse';
const SHELL = Option.exists(Option.fromNullOr(userInfo().shell), (path) => basename(path) === 'zsh') ? 'zsh' : 'bash';

// --- [MODELS] --------------------------------------------------------------------------

const Codex = Schema.fromJsonString(Schema.Struct({ session_id: Schema.String, cwd: Schema.String, tool_use_id: Schema.optionalKey(Schema.String), tool_input: Schema.Struct({ command: Schema.String }) }));
const Antigravity = Schema.fromJsonString(Schema.Struct({ conversationId: Schema.String, cwd: Schema.String, toolCall: Schema.Struct({ args: Schema.Struct({ CommandLine: Schema.String }) }) }));
const Output = Schema.fromJsonString(Schema.Unknown);

// --- [TABLES] --------------------------------------------------------------------------

const HOSTS = {
    codex: {
        request: Function.flow(
            Schema.decodeEffect(Codex),
            Effect.map(({ session_id, cwd, tool_use_id, tool_input }): typeof Request.Type => ({ command: tool_input.command, cwd, kind: 'plain', shell: SHELL, session: session_id, ...(tool_use_id === undefined ? {} : { call: tool_use_id }) })),
        ),
        rendered: Decision.match({
            Allow: () => Option.none<object>(),
            Deny: ({ reason }) => Option.some({ hookSpecificOutput: { hookEventName: EVENT, permissionDecision: 'deny', permissionDecisionReason: reason } }),
            Rewrite: ({ command, note }) => Option.some({ systemMessage: note, hookSpecificOutput: { hookEventName: EVENT, permissionDecision: 'allow', updatedInput: { command }, additionalContext: note } }),
        }),
        unjudged: (line: string): Option.Option<object> => Option.some({ hookSpecificOutput: { hookEventName: EVENT, additionalContext: line } }),
    },
    agy: {
        request: Function.flow(
            Schema.decodeEffect(Antigravity),
            Effect.map(({ conversationId, cwd, toolCall }): typeof Request.Type => ({ command: toolCall.args.CommandLine, cwd, kind: 'plain', shell: SHELL, session: conversationId })),
        ),
        rendered: Decision.match({
            Allow: () => Option.none<object>(),
            Deny: ({ reason }) => Option.some({ decision: 'deny', reason }),
            Rewrite: ({ command }) => Option.some({ decision: 'allow', overwrite: { CommandLine: command } }),
        }),
        unjudged: (_line: string): Option.Option<object> => Option.none(),
    },
} as const;
const Host = Schema.Literals(Struct.keys(HOSTS));

// --- [COMPOSITION] ---------------------------------------------------------------------

Effect.gen(function* () {
    const { args, stdin } = yield* Stdio.Stdio;
    const named = HOSTS[yield* Effect.flatMap(args, Function.flow(Array.head, Effect.fromOption, Effect.flatMap(Schema.decodeUnknownEffect(Host))))];
    const request = yield* named.request(yield* Stream.mkString(Stream.decodeText(stdin)));
    const answer = yield* decide(request).pipe(
        Effect.timeout('25 seconds'),
        Effect.map(named.rendered),
        Effect.catchCause((cause) => {
            const line = `${manifest.name} did not judge this command: ${Cause.pretty(cause).split('\n', 1).join('')}`;
            return Effect.logError(cause).pipe(Effect.andThen(due(request, new Map([[line, line]]))), Effect.map(Function.flow(Array.head, Option.flatMap(named.unjudged))));
        }),
    );
    yield* Option.match(answer, { onNone: () => Effect.void, onSome: (output) => Effect.flatMap(Schema.encodeEffect(Output)(output), Console.log) });
}).pipe(Effect.provide(Layer.provideMerge(Layer.provideMerge(Engine, layer), layerNodeFromConfig({ resource: { serviceName: manifest.name, serviceVersion: manifest.version }, target: 'file' }))), runMain({ teardown }));
