import { env } from 'cloudflare:workers';
import { it } from '@effect/vitest';
import { r2 } from '@rasm/hosting/storage.ts';
import { cases } from './cases.ts';

// --- [COMPOSITION] ---------------------------------------------------------------------

cases(it, r2(env.BUCKET));
