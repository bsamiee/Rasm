import { reset } from 'cloudflare:test';
import { env } from 'cloudflare:workers';
import { beforeEach, it } from '@effect/vitest';
import { D1 } from '@rasm/database/d1';
import { cases } from '#/cases';

// --- [COMPOSITION] ---------------------------------------------------------------------

beforeEach(reset);
cases(it, D1.layer({ db: env.DB }));
