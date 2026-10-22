import { unavailable } from '@rasm/hosting/http.ts';
import { Array, Effect, flow, Option, Struct } from 'effect';
import { owner } from './catalog.ts';

// --- [OPERATIONS] ----------------------------------------------------------------------

const ownerUserId = Effect.fn('ownerUserId')((email: string) => owner.select('email', [email]), Effect.map(flow(Array.head, Option.map(Struct.get('userId')))), Effect.catchTag(['SqlError', 'SchemaError'], unavailable));
const registerOwner = Effect.fn('registerOwner')(
    (row: typeof owner.Type) => owner.ensure([row]),
    Effect.map(([{ userId }]) => userId),
    Effect.catchTag(['SqlError', 'SchemaError'], unavailable),
);

// --- [EXPORTS] -------------------------------------------------------------------------

export { ownerUserId, registerOwner };
