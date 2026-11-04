import { env } from 'cloudflare:workers';
import { it } from '@effect/vitest';
import { ObjectStorage } from '@rasm/hosting/server';
import { cases } from '#/tests/typescript/libs/web/hosting/cases';

// --- [COMPOSITION] ---------------------------------------------------------------------

cases(it, ObjectStorage.r2(env.BUCKET));
