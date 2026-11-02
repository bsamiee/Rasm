import { Relation } from '@rasm/database/relation';
import { unavailable } from '@rasm/hosting/response';
import { Array, Effect, flow, Option, Struct } from 'effect';
import { owner } from '#/server/store/catalog';

// --- [OPERATIONS] ----------------------------------------------------------------------

const ownerUserId = Effect.fn('ownerUserId')((email: string) => Relation.select(owner, 'email', [email]), Effect.map(flow(Array.head, Option.map(Struct.get('userId')))), Effect.catchTag(['SqlError', 'SchemaError', 'IllegalArgumentError'], unavailable));
const registerOwner = Effect.fn('registerOwner')(
    (row: typeof owner.schema.Type) => Relation.ensure(owner, [row]),
    Effect.map(([{ userId }]) => userId),
    Effect.catchTag(['SqlError', 'SchemaError', 'IllegalArgumentError'], unavailable),
);

// --- [EXPORTS] -------------------------------------------------------------------------

export { ownerUserId, registerOwner };
