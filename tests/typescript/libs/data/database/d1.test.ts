import { reset } from 'cloudflare:test';
import { env } from 'cloudflare:workers';
import { beforeEach, it } from '@effect/vitest';
import { d1Engine } from '@rasm/database/d1';
import { Effect, Layer, Record } from 'effect';
import { cases } from '#/cases';

// --- [COMPOSITION] ---------------------------------------------------------------------

beforeEach(reset);
cases(
    it,
    Effect.fromOption(Record.get(env, 'DB')).pipe(
        Effect.map((db) => d1Engine({ db })),
        Layer.unwrap,
    ),
);
