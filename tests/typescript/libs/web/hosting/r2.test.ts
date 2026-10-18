import { env } from 'cloudflare:workers';
import { it } from '@effect/vitest';
import { cases } from './cases.ts';
import { r2 } from './storage.ts';

// --- [COMPOSITION] ---------------------------------------------------------------------

cases(it, r2(env.BUCKET));
