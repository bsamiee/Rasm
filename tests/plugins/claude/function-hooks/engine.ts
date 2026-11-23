import { layer as NodeServices } from '@effect/platform-node/NodeServices';
import { decide, Engine } from '@rasm/function-hooks/rules/decide';
import type { Decision, Request } from '@rasm/function-hooks/rules/decision';
import { Effect, Layer } from 'effect';
import { SecureRandom } from 'effect/crypto';

// --- [OPERATIONS] ----------------------------------------------------------------------

const refusing =
    (fragment: string) =>
    (decision: typeof Decision.Type): boolean =>
        decision._tag === 'Deny' && decision.reason.includes(fragment);

// --- [COMPOSITION] ---------------------------------------------------------------------

const Decider = Layer.provideMerge(Engine, Layer.merge(NodeServices, SecureRandom.layerWebCrypto));

const decided = (command: string, cwd: string, { kind = 'foreground', session }: { readonly kind?: Request['kind']; readonly session?: string } = {}) =>
    Effect.flatMap(
        SecureRandom.SecureRandom.use((random) => Effect.all({ session: session === undefined ? random.uuidV4() : Effect.succeed(session), call: random.uuidV4() })),
        (ids) => decide({ command, cwd, kind, ...ids }),
    );

// --- [EXPORTS] -------------------------------------------------------------------------

export { decided, Decider, refusing };
