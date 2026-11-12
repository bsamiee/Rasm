import { env } from 'cloudflare:workers';
import { it } from '@effect/vitest';
import { r2Storage } from '@rasm/object-storage/r2';
import { cases } from '#/cases';

// --- [COMPOSITION] ---------------------------------------------------------------------

cases(it, r2Storage({ bucket: env.BUCKET }));
