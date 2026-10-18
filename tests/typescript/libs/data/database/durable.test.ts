import { runInDurableObject } from 'cloudflare:test';
import { exports } from 'cloudflare:workers';
import { SqliteClient } from '@effect/sql-sqlite-do';
import { it } from '@effect/vitest';
import { Context, Effect, Layer, type Scope } from 'effect';
import { cases } from './cases.ts';

// --- [SERVICES] ------------------------------------------------------------------------

class Storage extends Context.Service<Storage, DurableObjectStorage>()('@rasm/database/durable.test/Storage') {}

// --- [COMPOSITION] ---------------------------------------------------------------------

cases(
    it,
    (options) => Layer.unwrap(Storage.useSync((storage) => SqliteClient.layer({ storage, ...options }))),
    <A, E>(effect: Effect.Effect<A, E, Scope.Scope | Storage>) => Effect.flatten(Effect.promise(() => runInDurableObject(exports.Store.get(exports.Store.newUniqueId()), (_, { storage }) => effect.pipe(Effect.provideService(Storage, storage), Effect.scoped, Effect.runPromiseExit)))),
    () => Effect.void,
);
