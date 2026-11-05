import { PID, WATCHER } from '../policies/internal/apps.ts';

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

const storedInstances = (value: unknown): readonly AppInstance[] => (Array.isArray(value) ? value.flatMap(records).flatMap(({ pid, bundle, agent }) => (typeof pid === 'number' && typeof bundle === 'string' ? [{ pid, bundle, ...(typeof agent === 'string' && { agent }) }] : [])) : []);

const createdPids = (result: unknown): readonly number[] => {
    const blocks: readonly unknown[] = Array.isArray(result) ? result : records(result).flatMap(({ content }) => (Array.isArray(content) ? content : []));
    return blocks
        .flatMap(records)
        .flatMap(({ type, text }) => (type === 'text' && typeof text === 'string' ? parseJson(text) : []))
        .flatMap(records)
        .flatMap((answer) => [answer, ...records(answer.launched)])
        .flatMap(({ pid, created }) => (typeof pid === 'number' && created === true ? [pid] : []));
};

// --- [CHANGES]

const outputChange = (output: unknown): Change => {
    const states = [output, ...records(output).map(({ stdout }) => stdout)]
        .flatMap(jsonLines)
        .flatMap(records)
        .flatMap((line) => Object.entries(line))
        .flatMap(([bundle, instances]) => records(instances).map((found) => ({ bundle, found })))
        .flatMap(({ bundle, found }) => Object.entries(found).flatMap(([pid, state]) => (PID.test(pid) ? [{ pid: Number(pid), bundle, state }] : [])))
        .flatMap(({ pid, bundle, state }) => records(state).map((fields) => ({ pid, bundle, state: fields })));
    return {
        created: states.flatMap(({ pid, bundle, state }) => (records(state.launched).some(({ created }) => created === true) ? [{ pid, bundle }] : [])),
        exited: new Set(states.flatMap(({ pid, state }) => ('exited' in state ? [pid] : []))),
    };
};

const applyChange = (instances: readonly AppInstance[], { created, exited }: Change): readonly AppInstance[] => [...new Map([...instances, ...created].map((instance) => [instance.pid, instance])).values()].filter(({ pid }) => !exited.has(pid));

// --- [QUIT]

const detachedQuit = (pids: readonly number[]): string => pids.map((pid) => `(${WATCHER} quit --pid ${pid} || ${WATCHER} quit --pid ${pid} --force) </dev/null >/dev/null 2>&1 &`).join('\n');

// --- [EXPORTS] -------------------------------------------------------------------------

export { type AppInstance, applyChange, type Change, createdPids, detachedQuit, outputChange, storedInstances };
