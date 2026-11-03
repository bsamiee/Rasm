import { Option } from '../composition.ts';

// --- [TYPES] ---------------------------------------------------------------------------

interface Ledger {
    readonly runs: ReadonlyMap<number, boolean>;
    readonly instances: readonly Instance[];
}

// --- [CONSTANTS] -----------------------------------------------------------------------

const IDENTIFIER = /bundleID="(?<bundle>[^"]+)"/u;
const PID = /^\d+$/u;

// --- [MODELS] --------------------------------------------------------------------------

interface Instance {
    readonly pid: number;
    readonly bundle: string;
    readonly agent?: string;
}
interface Change {
    readonly created: readonly Instance[];
    readonly exited: ReadonlySet<number>;
}

// --- [OPERATIONS] ----------------------------------------------------------------------

const keyed = (value: unknown): value is Readonly<Record<string, unknown>> => typeof value === 'object' && value !== null && !Array.isArray(value);

const record = (value: unknown): readonly Readonly<Record<string, unknown>>[] => (keyed(value) ? [value] : []);

const decoded = (text: string): readonly unknown[] => {
    try {
        return [JSON.parse(text)];
    } catch {
        return [];
    }
};

const jsonLines = (text: unknown): readonly unknown[] => (typeof text === 'string' ? text.split('\n').flatMap((line) => (line.startsWith('{') ? decoded(line) : [])) : []);

const identified = (output: string): Option<string> => Option.given(IDENTIFIER.exec(output)?.groups?.bundle);

const changed = (lines: readonly unknown[]): Change => {
    const states = lines
        .flatMap(record)
        .flatMap((line) => Object.entries(line))
        .flatMap(([bundle, instances]) => record(instances).flatMap((found) => Object.entries(found).flatMap(([pid, state]) => (PID.test(pid) ? [{ pid: Number(pid), bundle, state }] : []))))
        .flatMap(({ pid, bundle, state }) => record(state).map((fields) => ({ pid, bundle, state: fields })));
    return {
        created: states.flatMap(({ pid, bundle, state }) => (record(state.running).some(({ created }) => created === true) ? [{ pid, bundle }] : [])),
        exited: new Set(states.flatMap(({ pid, state }) => ('exited' in state ? [pid] : []))),
    };
};

const reported = (output: unknown): Change => changed([output, ...record(output).map(({ stdout }) => stdout)].flatMap(jsonLines));

const recorded = (kept: readonly Instance[], { created, exited }: Change): readonly Instance[] => [...new Map([...kept, ...created].map((instance) => [instance.pid, instance])).values()].filter(({ pid }) => !exited.has(pid));

const entered =
    (session: string) =>
    ({ runs, instances }: Ledger, { pid: run, session: owner, argv, line }: Readonly<Record<string, unknown>>): Ledger => {
        const launching = typeof run === 'number' ? runs.get(run) : undefined;
        const others = new Map([...runs].filter(([key]) => key !== run));
        if (typeof run === 'number' && Array.isArray(argv)) {
            return { runs: owner === session ? new Map([...others, [run, argv[1] === 'launch']]) : others, instances };
        }
        if (launching === undefined) {
            return { runs, instances };
        }
        return line === undefined ? { runs: others, instances } : { runs, instances: recorded(instances, { ...changed([line]), ...(!launching && { created: [] }) }) };
    };

const logged = (text: string, session: string): readonly Instance[] => jsonLines(text).flatMap(record).reduce<Ledger>(entered(session), { runs: new Map(), instances: [] }).instances;

const launched = (result: unknown): readonly number[] => {
    const blocks: readonly unknown[] = Array.isArray(result) ? result : record(result).flatMap(({ content }) => (Array.isArray(content) ? content : []));
    return blocks
        .flatMap(record)
        .flatMap(({ type, text }) => (type === 'text' && typeof text === 'string' ? decoded(text) : []))
        .flatMap(record)
        .flatMap(({ pid, created }) => (typeof pid === 'number' && created === true ? [pid] : []));
};

const held = (value: unknown): readonly Instance[] => (Array.isArray(value) ? value.flatMap(record).flatMap(({ pid, bundle, agent }) => (typeof pid === 'number' && typeof bundle === 'string' ? [{ pid, bundle, ...(typeof agent === 'string' && { agent }) }] : [])) : []);

// --- [EXPORTS] -------------------------------------------------------------------------

export { type Change, held, type Instance, identified, launched, logged, recorded, reported };
