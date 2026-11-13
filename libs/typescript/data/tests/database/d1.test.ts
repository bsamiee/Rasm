import { reset } from 'cloudflare:test';
import { env } from 'cloudflare:workers';
import { beforeEach, it } from '@effect/vitest';
import { d1Engine } from '@rasm/database/d1';
import { cases } from '#/cases';

// --- [COMPOSITION] ---------------------------------------------------------------------

beforeEach(reset);
cases(it, d1Engine({ db: env.DB }));
