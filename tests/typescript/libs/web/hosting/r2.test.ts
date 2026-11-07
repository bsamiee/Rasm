import { env } from 'cloudflare:workers';
import { it } from '@effect/vitest';
import { r2Storage } from '@rasm/hosting/worker';
import { cases } from '#/cases.ts';

// --- [COMPOSITION] ---------------------------------------------------------------------

cases(it, r2Storage(env.BUCKET));
