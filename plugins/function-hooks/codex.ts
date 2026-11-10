import { tmpdir } from 'node:os';
import { join } from 'node:path';
import { runMain } from '@effect/platform-node/NodeRuntime';
import { layer } from '@effect/platform-node/NodeServices';
import { Array, Cause, Console, Effect, Function, Layer, Option, Schema, Stdio, Stream } from 'effect';
import { KeyValueStore } from 'effect/persistence';
import { Decision } from '#/decision';
import { unjudged } from '#/hooks/register';
import { decide, live, Notes } from '#/policies/rules';

// --- [CONSTANTS] -----------------------------------------------------------------------

const EVENT = 'PreToolUse';

// --- [MODELS] --------------------------------------------------------------------------

const Input = Schema.fromJsonString(Schema.Struct({ session_id: Schema.String, cwd: Schema.String, tool_input: Schema.Struct({ command: Schema.String }) }));
const Output = Schema.fromJsonString(Schema.Unknown);

// --- [OPERATIONS] ----------------------------------------------------------------------

const rendered: (decision: Decision) => Option.Option<object> = Decision.match({
    Allow: () => Option.none(),
    Deny: ({ reason }) => Option.some({ hookSpecificOutput: { hookEventName: EVENT, permissionDecision: 'deny', permissionDecisionReason: reason } }),
    Rewrite: ({ command, notice }) => Option.some({ systemMessage: notice, hookSpecificOutput: { hookEventName: EVENT, permissionDecision: 'allow', updatedInput: { command }, additionalContext: notice } }),
});

const noted: (additionalContext: string) => object = (additionalContext) => ({ hookSpecificOutput: { hookEventName: EVENT, additionalContext } });

const told = (agent: string, line: string) =>
    Notes.use((notes) => notes.due(agent, new Map([[EVENT, line]]))).pipe(
        Effect.tap(() => Console.error(line)),
        Effect.map(Function.flow(Array.head, Option.map(noted))),
    );

const judged = ({ session_id, cwd, tool_input: { command } }: (typeof Input)['Type']) =>
    decide({ command, cwd, kind: 'plain', agent: session_id }).pipe(
        Effect.timeout('25 seconds'),
        Effect.map(rendered),
        Effect.catchCause((cause) => told(session_id, unjudged(Cause.pretty(cause)))),
    );

// --- [COMPOSITION] ---------------------------------------------------------------------

const Told = KeyValueStore.layerFileSystem(join(tmpdir(), 'function-hooks')).pipe(Layer.catchCause(() => KeyValueStore.layerMemory));

Stdio.Stdio.use(({ stdin }) => Stream.mkString(Stream.decodeText(stdin))).pipe(
    Effect.flatMap(Schema.decodeEffect(Input)),
    Effect.flatMap(judged),
    Effect.provide(Layer.provideMerge(live, Layer.provideMerge(Told, layer))),
    Effect.flatMap(
        Option.match({
            onNone: () => Effect.void,
            onSome: (answer) => Effect.flatMap(Schema.encodeEffect(Output)(answer), Console.log),
        }),
    ),
    Effect.catchCause(Function.flow(Cause.pretty, unjudged, Console.error)),
    runMain({ disableErrorReporting: true }),
);
