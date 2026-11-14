import { userInfo } from 'node:os';
import { basename } from 'node:path';
import { runMain } from '@effect/platform-node/NodeRuntime';
import { layer } from '@effect/platform-node/NodeServices';
import { Array, Cause, Console, Effect, Function, Layer, Option, Schema, Stdio, Stream } from 'effect';
import manifest from '#/manifest' with { type: 'json' };
import { decide, due, Engine } from '#/rules/decide';
import { Decision } from '#/rules/decision';

// --- [CONSTANTS] -----------------------------------------------------------------------

const EVENT = 'PreToolUse';

// --- [MODELS] --------------------------------------------------------------------------

const Input = Schema.fromJsonString(Schema.Struct({ session_id: Schema.String, cwd: Schema.String, tool_input: Schema.Struct({ command: Schema.String }) }));
const Output = Schema.fromJsonString(Schema.Unknown);

// --- [OPERATIONS] ----------------------------------------------------------------------

const rendered: (decision: typeof Decision.Type) => Option.Option<object> = Decision.match({
    Allow: () => Option.none(),
    Deny: ({ reason }) => Option.some({ hookSpecificOutput: { hookEventName: EVENT, permissionDecision: 'deny', permissionDecisionReason: reason } }),
    Rewrite: ({ command, note }) => Option.some({ systemMessage: note, hookSpecificOutput: { hookEventName: EVENT, permissionDecision: 'allow', updatedInput: { command }, additionalContext: note } }),
});

// --- [COMPOSITION] ---------------------------------------------------------------------

Effect.gen(function* () {
    const { stdin } = yield* Stdio.Stdio;
    const { session_id: agent, cwd, tool_input } = yield* Schema.decodeEffect(Input)(yield* Stream.mkString(Stream.decodeText(stdin)));
    const shell = Option.exists(Option.fromNullOr(userInfo().shell), (path) => basename(path) === 'zsh') ? 'zsh' : 'bash';
    const answer = yield* decide({ command: tool_input.command, cwd, kind: 'plain', agent, shell }).pipe(
        Effect.timeout('25 seconds'),
        Effect.map(rendered),
        Effect.catchCause(
            Effect.fnUntraced(function* (cause) {
                const line = `${manifest.name} did not judge this command: ${Cause.pretty(cause).split('\n', 1).join('')}`;
                yield* Console.error(line);
                return Option.map(Array.head(yield* due({ agent }, new Map([[line, line]]))), (additionalContext) => ({ hookSpecificOutput: { hookEventName: EVENT, additionalContext } }));
            }),
        ),
    );
    yield* Option.match(answer, { onNone: () => Effect.void, onSome: (output) => Effect.flatMap(Schema.encodeEffect(Output)(output), Console.log) });
}).pipe(Effect.provide(Layer.provideMerge(Engine, layer)), Effect.catchCause(Function.flow(Cause.pretty, Console.error)), runMain({ disableErrorReporting: true }));
