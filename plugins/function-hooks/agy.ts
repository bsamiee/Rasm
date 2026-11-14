import { runMain } from '@effect/platform-node/NodeRuntime';
import { layer } from '@effect/platform-node/NodeServices';
import { Cause, Console, Effect, Function, Layer, Option, Schema, Stdio, Stream } from 'effect';
import { decide, Engine } from '#/rules/decide';
import { Decision } from '#/rules/decision';

// --- [MODELS] --------------------------------------------------------------------------

const Input = Schema.fromJsonString(Schema.Struct({ conversationId: Schema.String, cwd: Schema.String, toolCall: Schema.Struct({ args: Schema.Struct({ CommandLine: Schema.String }) }) }));
const Output = Schema.fromJsonString(Schema.Unknown);

// --- [OPERATIONS] ----------------------------------------------------------------------

const rendered: (decision: typeof Decision.Type) => Option.Option<object> = Decision.match({
    Allow: () => Option.none(),
    Deny: ({ reason }) => Option.some({ decision: 'deny', reason }),
    Rewrite: ({ command }) => Option.some({ decision: 'allow', overwrite: { CommandLine: command } }),
});

// --- [COMPOSITION] ---------------------------------------------------------------------

Effect.gen(function* () {
    const { stdin } = yield* Stdio.Stdio;
    const { conversationId: agent, cwd, toolCall } = yield* Schema.decodeEffect(Input)(yield* Stream.mkString(Stream.decodeText(stdin)));
    const answer = yield* decide({ command: toolCall.args.CommandLine, cwd, kind: 'plain', agent }).pipe(Effect.timeout('25 seconds'), Effect.map(rendered));
    yield* Option.match(answer, { onNone: () => Effect.void, onSome: (output) => Effect.flatMap(Schema.encodeEffect(Output)(output), Console.log) });
}).pipe(Effect.provide(Layer.provideMerge(Engine, layer)), Effect.catchCause(Function.flow(Cause.pretty, Console.error)), runMain({ disableErrorReporting: true }));
