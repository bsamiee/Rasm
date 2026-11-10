import { runMain } from '@effect/platform-node/NodeRuntime';
import { layer } from '@effect/platform-node/NodeServices';
import { Forward, Host, Ssh } from '@rasm/ssh';
import { Array, Console, Effect, Layer, Schema, Stdio } from 'effect';

// --- [MODELS] --------------------------------------------------------------------------

const Asked = Schema.fromJsonString(Schema.Struct({ folder: Schema.String, host: Host, forward: Forward, calls: Schema.Int, remote: Schema.NonEmptyArray(Schema.String) }));
const Reached = Schema.fromJsonString(Schema.Array(Schema.String));

// --- [COMPOSITION] ---------------------------------------------------------------------

const held = Effect.gen(function* () {
    const args = yield* Effect.flatMap(Stdio.Stdio, (stdio) => stdio.args);
    const { folder, host, forward, calls, remote } = yield* Effect.flatMap(Effect.fromOption(Array.last(args)), Schema.decodeEffect(Asked));
    const ssh = yield* Ssh.make(folder);
    yield* ssh.forward(host, forward);
    const reached = yield* Effect.forEach(Array.range(1, calls), () => ssh.run(host, remote), { concurrency: 'unbounded' });
    return yield* Effect.flatMap(Schema.encodeEffect(Reached)(reached), Console.log);
});

if (import.meta.main) {
    Layer.launch(Layer.effectDiscard(held)).pipe(Effect.provide(layer), runMain);
}

// --- [EXPORTS] -------------------------------------------------------------------------

export { Asked, Reached };
