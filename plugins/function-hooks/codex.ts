import { runMain } from '@effect/platform-node/NodeRuntime';
import { layer } from '@effect/platform-node/NodeServices';
import { Console, Effect, Layer, Option, Schema, Stdio, Stream } from 'effect';
import { Decision, unjudged } from '#/decision.ts';
import { decide, live } from '#/policies/rules.ts';

// --- [CONSTANTS] -----------------------------------------------------------------------

const EVENT = 'PreToolUse';

// --- [MODELS] --------------------------------------------------------------------------

const Input = Schema.fromJsonString(Schema.Struct({ cwd: Schema.String, tool_input: Schema.Struct({ command: Schema.String }) }));
const Output = Schema.fromJsonString(Schema.Unknown);

// --- [OPERATIONS] ----------------------------------------------------------------------

const rendered = Decision.match({
    Allow: () => Option.none(),
    Deny: ({ reason }) => Option.some({ hookSpecificOutput: { hookEventName: EVENT, permissionDecision: 'deny', permissionDecisionReason: reason } }),
    Rewrite: ({ command, notice }) => Option.some({ systemMessage: notice, hookSpecificOutput: { hookEventName: EVENT, permissionDecision: 'allow', updatedInput: { command }, additionalContext: notice } }),
});

// --- [COMPOSITION] ---------------------------------------------------------------------

Stdio.Stdio.use(({ stdin }) => Stream.mkString(Stream.decodeText(stdin))).pipe(
    Effect.flatMap(Schema.decodeEffect(Input)),
    Effect.flatMap(({ cwd, tool_input: { command } }) => decide({ command, cwd, kind: 'plain' })),
    Effect.timeout('25 seconds'),
    Effect.provide(Layer.provideMerge(live, layer)),
    Effect.catchCause(unjudged),
    Effect.map(rendered),
    Effect.flatMap(
        Option.match({
            onNone: () => Effect.void,
            onSome: (answer) => Schema.encodeEffect(Output)(answer).pipe(Effect.orDie, Effect.flatMap(Console.log)),
        }),
    ),
    runMain({ disableErrorReporting: true }),
);
