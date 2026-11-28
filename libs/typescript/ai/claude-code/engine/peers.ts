import { run } from '@rasm/process';
import { Config, Effect, Option, Schema } from 'effect';
import { Engine } from '#/engine/interface';

// --- [MODELS] --------------------------------------------------------------------------

class Peer extends Schema.Class<Peer, { readonly brand: unique symbol }>('@rasm/claude-code/engine/peers/Peer')({
    sessionId: Schema.OptionFromOptionalKey(Schema.String),
    name: Schema.OptionFromOptionalKey(Schema.String),
    kind: Schema.Literals(['interactive', 'background']),
    status: Schema.OptionFromOptionalKey(Schema.Literals(['waiting', 'busy', 'idle'])),
    waitingFor: Schema.OptionFromOptionalKey(Schema.String),
    startedAt: Schema.DateTimeUtcFromMillis,
}) {
    static readonly list = Effect.gen(function* () {
        const [engine, self] = yield* Effect.all([Engine, Config.String('CLAUDE_CODE_SESSION_ID')]);
        const root = yield* Option.match(yield* engine.repo, { onNone: () => engine.root, onSome: (repo) => Effect.succeed(repo.root) });
        const { stdout } = yield* run(['claude', 'agents', '--json', '--cwd', root]);
        const listed = yield* Peer.pipe(Schema.Array, Schema.fromJsonString, Schema.decodeEffect)(stdout);
        return listed.filter((peer) => !Option.contains(peer.sessionId, self));
    });
}

// --- [EXPORTS] -------------------------------------------------------------------------

export { Peer };
