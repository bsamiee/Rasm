import { randomUUID } from 'node:crypto';
import { layer as NodeServices } from '@effect/platform-node/NodeServices';
import { decide, Engine } from '@rasm/function-hooks/rules/decide';
import type { Decision, Request } from '@rasm/function-hooks/rules/decision';
import { Effect, Layer } from 'effect';

// --- [OPERATIONS] ----------------------------------------------------------------------

const refusing =
    (fragment: string) =>
    (decision: typeof Decision.Type): boolean =>
        decision._tag === 'Deny' && decision.reason.includes(fragment);

// --- [COMPOSITION] ---------------------------------------------------------------------

const Decider = Layer.provideMerge(Engine, NodeServices);

const decided = (command: string, cwd: string, { kind = 'foreground', session }: { readonly kind?: Request['kind']; readonly session?: string } = {}) =>
    Effect.flatMap(
        Effect.sync(() => ({ session: session ?? randomUUID(), call: randomUUID() })),
        (ids) => decide({ command, cwd, kind, ...ids }),
    );

// --- [EXPORTS] -------------------------------------------------------------------------

export { decided, Decider, refusing };
