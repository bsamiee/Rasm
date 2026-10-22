import { reset } from 'cloudflare:test';
import { env } from 'cloudflare:workers';
import { D1Client } from '@effect/sql-d1';
import { beforeEach, it } from '@effect/vitest';
import { Effect } from 'effect';
import { cases } from '#/tests/typescript/libs/data/database/cases';

// --- [COMPOSITION] ---------------------------------------------------------------------

beforeEach(reset);
cases(
    it,
    (options) => D1Client.layer({ db: env.DB, ...options }),
    () => Effect.void,
);
