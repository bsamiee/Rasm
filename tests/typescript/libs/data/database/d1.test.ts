import { reset } from 'cloudflare:test';
import { env } from 'cloudflare:workers';
import { D1Client } from '@effect/sql-d1';
import { it } from '@effect/vitest';
import { Effect } from 'effect';
import { cases } from './cases.ts';

// --- [COMPOSITION] ---------------------------------------------------------------------

cases(
    it,
    (options) => D1Client.layer({ db: env.DB, ...options }),
    (effect) => Effect.andThen(Effect.promise(reset), effect),
    () => Effect.void,
);
