import { layer } from '@effect/vitest';
import { d1Engine } from '@rasm/database/d1';
import { reset } from 'cloudflare:test';
import { env } from 'cloudflare:workers';
import { Effect, Function, Layer, Predicate } from 'effect';
import { reconciling } from '#/reconciling';
import { type Relational, rows } from '#/rows';
import { storage, tables } from '#/storage.sqlite';

// --- [CONSTANTS] -----------------------------------------------------------------------

const failed = /(?<kind>CHECK|UNIQUE|FOREIGN KEY) constraint failed/u;
const store: Relational = {
    tables,
    fault: ({ reason: { cause } }) => (Predicate.hasProperty(cause, 'message') && Predicate.isString(cause.message) ? failed.exec(cause.message)?.groups?.['kind'] : cause),
    check: 'CHECK',
    unique: 'UNIQUE',
    reference: 'FOREIGN KEY',
    command: Function.identity,
    isolated: 1,
};

// --- [COMPOSITION] ---------------------------------------------------------------------

layer(d1Engine({ db: env.DB }).pipe(Layer.provide(Layer.effectDiscard(Effect.promise(reset)))), { buildPerTest: true, excludeTestServices: true })('d1', (it) => {
    reconciling(it, store);
    rows(it, store);
    storage(it, store);
});
