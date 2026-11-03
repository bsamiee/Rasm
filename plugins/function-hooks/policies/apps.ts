import { Option } from '../monads.ts';

// --- [TYPES] ---------------------------------------------------------------------------

interface AppInstance {
    readonly pid: number;
    readonly bundle: string;
    readonly agent?: string;
}
interface Change {
    readonly created: readonly AppInstance[];
    readonly exited: ReadonlySet<number>;
}
interface LogState {
    readonly runs: ReadonlyMap<number, boolean>;
    readonly instances: readonly AppInstance[];
}

// --- [CONSTANTS] -----------------------------------------------------------------------

const BUNDLE_ID = /bundleID="(?<bundle>[^"]+)"/u;
const PID = /^\d+$/u;

// --- [OPERATIONS] ----------------------------------------------------------------------

// --- [DECODE]

const isRecord = (value: unknown): value is Readonly<Record<string, unknown>> => typeof value === 'object' && value !== null && !Array.isArray(value);

const records = (value: unknown): readonly Readonly<Record<string, unknown>>[] => (isRecord(value) ? [value] : []);

const parseJson = (text: string): readonly unknown[] => {
    try {
        return [JSON.parse(text)];
    } catch {
        return [];
    }
};

const jsonLines = (text: unknown): readonly unknown[] => (typeof text === 'string' ? text.split('\n').flatMap((line) => (line.startsWith('{') ? parseJson(line) : [])) : []);

const bundleId = (output: string): Option<string> => Option.fromNullable(BUNDLE_ID.exec(output)?.groups?.bundle);

const storedInstances = (value: unknown): readonly AppInstance[] => (Array.isArray(value) ? value.flatMap(records).flatMap(({ pid, bundle, agent }) => (typeof pid === 'number' && typeof bundle === 'string' ? [{ pid, bundle, ...(typeof agent === 'string' && { agent }) }] : [])) : []);

const createdPids = (result: unknown): readonly number[] => {
    const blocks: readonly unknown[] = Array.isArray(result) ? result : records(result).flatMap(({ content }) => (Array.isArray(content) ? content : []));
    return blocks
        .flatMap(records)
        .flatMap(({ type, text }) => (type === 'text' && typeof text === 'string' ? parseJson(text) : []))
        .flatMap(records)
        .flatMap(({ pid, created }) => (typeof pid === 'number' && created === true ? [pid] : []));
};

// --- [CHANGES]

const lineChange = (lines: readonly unknown[]): Change => {
    const states = lines
        .flatMap(records)
        .flatMap((line) => Object.entries(line))
        .flatMap(([bundle, instances]) => records(instances).map((found) => ({ bundle, found })))
        .flatMap(({ bundle, found }) => Object.entries(found).flatMap(([pid, state]) => (PID.test(pid) ? [{ pid: Number(pid), bundle, state }] : [])))
        .flatMap(({ pid, bundle, state }) => records(state).map((fields) => ({ pid, bundle, state: fields })));
    return {
        created: states.flatMap(({ pid, bundle, state }) => (records(state.running).some(({ created }) => created === true) ? [{ pid, bundle }] : [])),
        exited: new Set(states.flatMap(({ pid, state }) => ('exited' in state ? [pid] : []))),
    };
};

const outputChange = (output: unknown): Change => lineChange([output, ...records(output).map(({ stdout }) => stdout)].flatMap(jsonLines));

const applyChange = (instances: readonly AppInstance[], { created, exited }: Change): readonly AppInstance[] => [...new Map([...instances, ...created].map((instance) => [instance.pid, instance])).values()].filter(({ pid }) => !exited.has(pid));

// --- [LOG]

const applyLogLine =
    (session: string) =>
    ({ runs, instances }: LogState, { pid: run, session: owner, argv, line }: Readonly<Record<string, unknown>>): LogState => {
        const launching = typeof run === 'number' ? runs.get(run) : undefined;
        const others = new Map([...runs].filter(([key]) => key !== run));
        if (typeof run === 'number' && Array.isArray(argv)) {
            return { runs: owner === session ? new Map([...others, [run, argv[1] === 'launch']]) : others, instances };
        }
        if (launching === undefined) {
            return { runs, instances };
        }
        return line === undefined ? { runs: others, instances } : { runs, instances: applyChange(instances, { ...lineChange([line]), ...(!launching && { created: [] }) }) };
    };

const logInstances = (text: string, session: string): readonly AppInstance[] => jsonLines(text).flatMap(records).reduce<LogState>(applyLogLine(session), { runs: new Map(), instances: [] }).instances;

// --- [EXPORTS] -------------------------------------------------------------------------

export { type AppInstance, applyChange, bundleId, type Change, createdPids, logInstances, outputChange, storedInstances };
