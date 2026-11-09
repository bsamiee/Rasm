import { reset } from 'cloudflare:test';
import { env } from 'cloudflare:workers';
import { beforeEach, it } from '@effect/vitest';
import { D1 } from '@rasm/database/d1';
import { Effect, Layer, Record } from 'effect';
import { cases } from '#/cases';

// --- [COMPOSITION] ---------------------------------------------------------------------

beforeEach(reset);
cases(
    it,
    Effect.fromOption(Record.get(env, 'DB')).pipe(
        Effect.map((db) => D1.layer({ db })),
        Layer.unwrap,
    ),
);
