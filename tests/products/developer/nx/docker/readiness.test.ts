import { readFileSync } from 'node:fs';
import { join } from 'node:path';
import { waitForDevServer } from '@rasm/nx-docker/ready-check';
import { it, pathed } from '@rasm/testing/nx';
import { describe, expect, vi } from 'vitest';

// --- [CONSTANTS] -----------------------------------------------------------------------

const API = (health: string) => `{"Service":"api","State":"running","Health":"${health}"}`;

// --- [COMPOSITION] ---------------------------------------------------------------------

describe('service readiness', () => {
    it.for([
        { case: 'a healthcheck still starting', services: ['api'], first: [API('starting')], later: [API('healthy')], polls: 2 },
        { case: 'one of its services not yet listed', services: ['api', 'worker'], first: [API('healthy')], later: [API('healthy'), '{"Service":"worker","State":"running","Health":""}'], polls: 2 },
        { case: 'a running service with no healthcheck', services: ['worker'], first: ['{"Service":"worker","State":"running","Health":""}'], later: ['{"Service":"worker","State":"running","Health":""}'], polls: 1 },
    ])('polls docker compose ps through $case until every service runs healthy', { timeout: 10_000 }, async ({ services, first, later, polls }, { workspace }) => {
        const [calls, polled] = [join(workspace.root, 'calls'), join(workspace.root, 'polled')];
        workspace.write({ 'infra/compose.yaml': 'services: {}\n' });
        pathed(join(workspace.root, 'bin'), { docker: `echo "$PWD $*" >> ${calls}\nif [ -f ${polled} ]; then printf '%s\\n' '${later.join("' '")}'; else : > ${polled}; printf '%s\\n' '${first.join("' '")}'; fi` });
        vi.stubEnv('NX_TASK_OPTIONS', JSON.stringify({ cwd: 'infra' }));

        await waitForDevServer(...services);

        expect(readFileSync(calls, 'utf-8')).toBe(`${join(workspace.root, 'infra')} compose ps --format json ${services.join(' ')}\n`.repeat(polls));
    });
});
