import { env } from 'cloudflare:workers';
import { it } from '@effect/vitest';
import { r2 } from '@rasm/hosting';
import { cases } from '#/tests/typescript/libs/web/hosting/cases';

// --- [COMPOSITION] ---------------------------------------------------------------------

cases(it, r2(env.BUCKET));
