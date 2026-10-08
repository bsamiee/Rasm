import { decoded, map, type Result } from '../composition.ts';
import type { Service } from '../hooks/state.d.ts';
import type { Invocation } from '../policies/invocation.ts';

// --- [CONSTANTS] -----------------------------------------------------------------------

const LAUNCHD_AGENTS: Invocation = ['yq', '-o=json', '.bootstrap.macos.launchd.agents // {}', 'mise.toml'];
const LISTENERS: Invocation = ['lsof', '-nP', '-iTCP', '-sTCP:LISTEN', '-Fn'];
const UID: Invocation = ['id', '-u'];

// --- [OPERATIONS] ----------------------------------------------------------------------

const services = (printed: Result<string>): Result<readonly Service[]> =>
    map(decoded<Readonly<Record<string, { readonly args: readonly string[] }>>>('yq', printed), (rows) =>
        Object.entries(rows).flatMap(([name, { args }]) => {
            const port = args.find((_arg, index) => args[index - 1] === '--port');
            return port === undefined ? [] : [{ name, port }];
        }),
    );

const down = (known: readonly Service[], listeners: string): readonly Service[] => {
    const ports = new Set(listeners.split('\n').flatMap((line) => (line.startsWith('n') ? [line.slice(line.lastIndexOf(':') + 1)] : [])));
    return known.filter(({ port }) => !ports.has(port));
};

const remaining = (held: readonly Service[], restarted: ReadonlySet<string>): readonly Service[] => held.filter(({ name }) => !restarted.has(name));

const kickstart = (name: string, uid: string): Invocation => ['launchctl', 'kickstart', '-k', `gui/${uid.trim()}/dev.mise.${name}`];

// --- [EXPORTS] -------------------------------------------------------------------------

export { down, kickstart, LAUNCHD_AGENTS, LISTENERS, remaining, services, UID };
