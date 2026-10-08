import type { AgentSpawnArgs, AgentSpawnResult } from 'claude-code';
import { counted } from '../composition.ts';

// --- [TYPES] ---------------------------------------------------------------------------

type Spawn = { readonly kind: 'range'; readonly agent: string; readonly from: number; readonly to: number } | { readonly kind: 'category'; readonly agent: string; readonly category: string };

interface Boundary {
    readonly key: string;
    readonly spawns: readonly Spawn[];
    readonly findings: readonly string[];
}

// --- [OPERATIONS] ----------------------------------------------------------------------

const request = (spawn: Spawn, key: string, cwd: string): AgentSpawnArgs => ({
    subagentType: spawn.agent,
    cwd,
    ...(spawn.kind === 'range' ? { prompt: `range ${key} ${spawn.from} ${spawn.to}`, description: 'judge edits' } : { prompt: `category ${spawn.category} lineage ${key}`, description: 'build category rule' }),
});

const outcome = (spawn: Spawn, result: AgentSpawnResult): string => {
    const subject = spawn.kind === 'range' ? `${spawn.from}..${spawn.to}` : spawn.category;
    return result.deny === undefined ? `spawned ${spawn.agent}${result.agentId === undefined ? '' : ` ${result.agentId}`} over ${subject}` : `${spawn.agent} spawn refused over ${subject}, ${result.deny}`;
};

const delivered = (findings: readonly string[], branch: string): readonly string[] => (findings.length === 0 ? [] : [`${counted(findings.length, 'finding', 'findings')} on ${branch}, ids ${findings.join(', ')}. Use observation skill for delivered findings`]);

// --- [EXPORTS] -------------------------------------------------------------------------

export type { Boundary, Spawn };
export { delivered, outcome, request };
