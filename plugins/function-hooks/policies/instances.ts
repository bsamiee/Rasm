import { Option } from '../composition.ts';

// --- [TYPES] ---------------------------------------------------------------------------

interface Ledger {
    readonly runs: ReadonlyMap<number, boolean>;
    readonly instances: ReadonlyMap<number, Instance>;
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

const identified = (output: string): Option<string> => Option.given(IDENTIFIER.exec(output)?.groups?.bundle);

const changed = (lines: readonly unknown[]): { readonly created: readonly Instance[]; readonly exited: ReadonlySet<number> } => {
    const states = lines
        .flatMap(record)
        .flatMap((line) => Object.entries(line))
        .flatMap(([bundle, instances]) => record(instances).flatMap((found) => Object.entries(found).flatMap(([pid, state]) => (PID.test(pid) ? [{ pid: Number(pid), bundle, state }] : []))))
        .flatMap(({ pid, bundle, state }) => record(state).map((settled) => ({ pid, bundle, state: settled })));
    return {
        created: states.flatMap(({ pid, bundle, state }) => (record(state.running).some(({ created }) => created === true) ? [{ pid, bundle }] : [])),
        exited: new Set(states.flatMap(({ pid, state }) => ('exited' in state ? [pid] : []))),
    };
};

const reported = (output: unknown): { readonly created: readonly Instance[]; readonly exited: ReadonlySet<number> } =>
    changed(
        [output, ...record(output).map(({ stdout }) => stdout)]
            .flatMap((text) => (typeof text === 'string' ? text.split('\n') : []))
            .filter((line) => line.startsWith('{'))
            .flatMap(decoded),
    );

const tallied = (instances: ReadonlyMap<number, Instance>, launching: boolean, { created, exited }: ReturnType<typeof changed>): ReadonlyMap<number, Instance> => new Map([...instances, ...(launching ? created : []).map((instance) => [instance.pid, instance] as const)].filter(([pid]) => !exited.has(pid)));

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
        return line === undefined ? { runs: others, instances } : { runs, instances: tallied(instances, launching, changed([line])) };
    };

const logged = (text: string, session: string): readonly Instance[] => [
    ...text
        .split('\n')
        .filter((line) => line.startsWith('{'))
        .flatMap(decoded)
        .flatMap(record)
        .reduce<Ledger>(entered(session), { runs: new Map(), instances: new Map() })
        .instances.values(),
];

const launched = (result: unknown): readonly number[] => {
    const blocks: readonly unknown[] = Array.isArray(result) ? result : record(result).flatMap(({ content }) => (Array.isArray(content) ? content : []));
    return blocks
        .flatMap(record)
        .flatMap(({ type, text }) => (type === 'text' && typeof text === 'string' ? decoded(text) : []))
        .flatMap(record)
        .flatMap(({ pid, created }) => (typeof pid === 'number' && created === true ? [pid] : []));
};

const held = (value: unknown): readonly Instance[] => (Array.isArray(value) ? value.flatMap(record).flatMap(({ pid, bundle, agent }) => (typeof pid === 'number' && typeof bundle === 'string' ? [{ pid, bundle, ...(typeof agent === 'string' && { agent }) }] : [])) : []);

const recorded = (kept: readonly Instance[], created: readonly Instance[], exited: ReadonlySet<number>): readonly Instance[] => [...new Map([...kept, ...created].map((instance) => [instance.pid, instance])).values()].filter(({ pid }) => !exited.has(pid));

// --- [EXPORTS] -------------------------------------------------------------------------

export { held, type Instance, identified, launched, logged, recorded, reported };
