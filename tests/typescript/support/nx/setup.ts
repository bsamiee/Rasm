import { mkdtempDisposableSync } from 'node:fs';
import { tmpdir } from 'node:os';
import { join } from 'node:path';
import { env } from 'node:process';
import { Effect } from 'effect';
import { afterAll, vi } from 'vitest';

// --- [TYPES] ---------------------------------------------------------------------------

type Services = typeof import('@effect/platform-node/NodeServices');

// --- [COMPOSITION] ---------------------------------------------------------------------

const data = mkdtempDisposableSync(join(tmpdir(), 'nx-workspace-data-'));

Object.assign(env, { NX_WORKSPACE_DATA_DIRECTORY: data.path, NX_DAEMON: 'false', NX_ISOLATE_PLUGINS: 'false' });
Reflect.deleteProperty(env, 'NX_CACHE_PROJECT_GRAPH');

vi.doMock('@effect/platform-node/NodeServices', (original) => Effect.runPromise(Effect.map(Effect.all([Effect.promise((): Promise<Services> => original()), Effect.promise(() => import('#/nx/tools'))]), ([services, tools]) => ({ ...services, layer: tools.layer(services.layer) }))));

afterAll(() => data.remove());
