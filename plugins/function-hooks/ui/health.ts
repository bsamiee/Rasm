import { decoded, map, type Result } from '../composition.ts';
import type { Service } from '../hooks/state.d.ts';
import type { Invocation } from '../policies/invocation.ts';

// --- [CONSTANTS] -----------------------------------------------------------------------

const AGENTS: Invocation = ['mise', 'bootstrap', 'macos', 'launchd-agents', 'status', '--json'];
const JOBS: Invocation = ['launchctl', 'list'];
const UID: Invocation = ['id', '-u'];
const _PID = /^\d+$/u;

// --- [OPERATIONS] ----------------------------------------------------------------------

const services = (printed: Result<string>): Result<readonly Service[]> => map(decoded<{ readonly launchd: { readonly available: boolean; readonly agents: readonly Service[] } }>('mise', printed), ({ launchd }) => (launchd.available ? launchd.agents.map(({ name, label }) => ({ name, label })) : []));

const down = (known: readonly Service[], jobs: string): readonly Service[] => {
    const running = new Set(
        jobs.split('\n').flatMap((line) => {
            const [pid, , label] = line.split('\t');
            return pid === undefined || label === undefined || !_PID.test(pid) ? [] : [label];
        }),
    );
    return known.filter(({ label }) => !running.has(label));
};

const kickstart = ({ label }: Service, uid: string): Invocation => ['launchctl', 'kickstart', '-k', `gui/${uid.trim()}/${label}`];

// --- [EXPORTS] -------------------------------------------------------------------------

export { AGENTS, down, JOBS, kickstart, services, UID };
