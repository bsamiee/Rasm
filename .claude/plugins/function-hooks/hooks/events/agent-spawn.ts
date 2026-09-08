// agent.spawn, the brief of the spawned type joins the prompt, with the newest batch on a dispatch row

// --- [IMPORTS] -------------------------------------------------------------------------

import type { AgentSpawnResult, On } from 'claude-code';
import { absurd } from '../composition/decision.ts';
import { forEach, fromBoolean, fromNullable, getOrElse, map, none, type Option } from '../composition/option.ts';
import type { Options } from '../host/options.ts';
import { type Dispatch, decodeDispatch, ids, key } from '../host/store.ts';
import { type Batch, spawnRule } from '../policies/agents.ts';

// --- [OPERATIONS] ----------------------------------------------------------------------

// The newest dispatch key is the last one, the store keeps insertion order
const _batchId = (all: readonly string[]): Option<string> => fromNullable(ids('dispatch')(all).at(-1));

const _batch = (batchId: string, value: unknown): Option<Batch> => map((dispatch: Dispatch): Batch => ({ batchId, dispatch }))(decodeDispatch(value));

// --- [REGISTRATION] --------------------------------------------------------------------

const agentSpawn = (on: On, _options: Options): void => {
    on('agent.spawn', async ($, e, next) => {
        const batch = getOrElse<Option<Batch>>(none)(
            await forEach(async (batchId: string) => _batch(batchId, await $.store.get(key('dispatch', batchId))))(_batchId(await $.store.keys())),
        );
        return spawnRule<typeof e>(batch)(e).match<Promise<AgentSpawnResult>>({
            rewrite: (input, context) => {
                fromBoolean(context.length > 0).match<void>({ some: () => $.ui.notice(e.tool_use_id, context.join(' ')), none: () => undefined });
                return next(input);
            },
            deny: absurd,
            answer: absurd,
        });
    });
};

// --- [EXPORTS] -------------------------------------------------------------------------

export { agentSpawn };
