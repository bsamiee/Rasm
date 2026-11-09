import { reset } from 'cloudflare:test';
import { env } from 'cloudflare:workers';
import { D1Client } from '@effect/sql-d1';
import { beforeEach, it } from '@effect/vitest';
import { cases } from '#/cases';

// --- [COMPOSITION] ---------------------------------------------------------------------

beforeEach(reset);
cases(it, (options) => D1Client.layer({ db: env.DB, ...options }));
