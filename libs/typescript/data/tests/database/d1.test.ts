import { it } from '@effect/vitest';
import { d1Engine } from '@rasm/database/d1';
import { reset } from 'cloudflare:test';
import { env } from 'cloudflare:workers';
import { Effect, Layer } from 'effect';
import { cases } from '#/cases';

// --- [COMPOSITION] ---------------------------------------------------------------------

cases(it, d1Engine({ db: env.DB }).pipe(Layer.provide(Layer.effectDiscard(Effect.promise(reset)))));
