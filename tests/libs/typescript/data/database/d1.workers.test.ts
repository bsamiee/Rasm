import { it } from '@effect/vitest';
import { d1Engine } from '@rasm/database/d1';
import { reset } from 'cloudflare:test';
import { env } from 'cloudflare:workers';
import { Effect, Layer } from 'effect';
import { reconciliation } from '#/reconciliation.sqlite';
import { rows } from '#/rows';

// --- [COMPOSITION] ---------------------------------------------------------------------

const client = d1Engine({ db: env.DB }).pipe(Layer.provide(Layer.effectDiscard(Effect.promise(reset))));

reconciliation(it, client);
rows(it, client);
