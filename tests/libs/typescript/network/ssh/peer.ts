import { runMain } from '@effect/platform-node/NodeRuntime';
import { layer } from '@effect/platform-node/NodeServices';
import { Ssh } from '@rasm/ssh/client';
import { Forward } from '@rasm/ssh/forward';
import { Host } from '@rasm/ssh/host';
import { Array, Console, Effect, Layer, Option, Schema, Stdio } from 'effect';

// --- [MODELS] --------------------------------------------------------------------------

const Asked = Schema.fromJsonString(Schema.Struct({ folder: Schema.String, host: Host, forward: Schema.OptionFromOptionalKey(Forward), calls: Schema.Int, remote: Schema.NonEmptyArray(Schema.String) }));
const Reached = Schema.String.pipe(Schema.Array, Schema.fromJsonString);

// --- [COMPOSITION] ---------------------------------------------------------------------

const held = Effect.gen(function* () {
    const args = yield* Effect.flatMap(Stdio.Stdio, (stdio) => stdio.args);
    const { folder, host, forward, calls, remote } = yield* Effect.flatMap(Effect.fromOption(Array.last(args)), Schema.decodeEffect(Asked));
    const ssh = yield* Ssh.make(folder);
    yield* Effect.transposeOption(Option.map(forward, (wanted) => ssh.forward(host, wanted)));
    const reached = yield* Effect.forEach(Array.range(1, calls), () => ssh.run(host, remote), { concurrency: 'unbounded' });
    return yield* Effect.flatMap(Schema.encodeEffect(Reached)(reached), Console.log);
});

if (import.meta.main) {
    runMain(held.pipe(Layer.effectDiscard, Layer.launch, Effect.provide(layer)));
}

// --- [EXPORTS] -------------------------------------------------------------------------

export { Asked, Reached };
