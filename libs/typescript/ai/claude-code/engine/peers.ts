import { Array, Config, Effect, Option, Schema } from 'effect';
import { ChildProcess, ChildProcessSpawner } from 'effect/process';
import { Engine } from '#/engine/interface';

// --- [MODELS] --------------------------------------------------------------------------

class Peer extends Schema.Class<Peer, { readonly brand: unique symbol }>('@rasm/claude-code/engine/peers/Peer')({
    pid: Schema.OptionFromOptionalKey(Schema.Int),
    sessionId: Schema.OptionFromOptionalKey(Schema.String),
    name: Schema.OptionFromOptionalKey(Schema.String),
    kind: Schema.Literals(['interactive', 'background']),
    status: Schema.OptionFromOptionalKey(Schema.Literals(['waiting', 'busy', 'idle'])),
    waitingFor: Schema.OptionFromOptionalKey(Schema.String),
    startedAt: Schema.DateTimeUtcFromMillis,
}) {
    static readonly list = Effect.gen(function* () {
        const [engine, processes, self] = yield* Effect.all([Engine, ChildProcessSpawner.ChildProcessSpawner, Config.String('CLAUDE_CODE_SESSION_ID')]);
        const root = Option.match(yield* engine.repo, { onNone: () => engine.root, onSome: (repo) => Effect.succeed(repo.root) });
        const printed = yield* processes.string(ChildProcess.make('claude', ['agents', '--json', '--cwd', yield* root]));
        const listed = yield* Schema.decodeEffect(Listing)(printed);
        return listed.filter((peer) => !Option.contains(peer.sessionId, self));
    }).pipe(Effect.catchCause((cause) => Effect.as(Effect.logWarning('peers unread', cause), Array.empty<Peer>())));
}
const Listing = Peer.pipe(Schema.Array, Schema.fromJsonString);

// --- [EXPORTS] -------------------------------------------------------------------------

export { Peer };
