// Store keys and the refinements that read a stored value, one key per row under a namespace because the store has no compare-and-set

// --- [TYPES] ---------------------------------------------------------------------------

type Namespace = (typeof NAMESPACES)[number];

type Kind = (typeof KIND_NAMES)[number];

type Status = (typeof STATUS)[number];

type Part = (typeof PART_NAMES)[number];

type Guard<T> = (value: unknown) => value is T;

// A record of field refinements and the record type it reads
type Shape = Readonly<Record<string, Guard<unknown>>>;

type Struct<S extends Shape> = { readonly [K in keyof S]: S[K] extends Guard<infer T> ? T : never };

// The value of secrets, and of session/<session>: the variables `mise env --json` answers, the init.env of every $.process.run
type StringRecord = Readonly<Record<string, string>>;

interface Notice {
    readonly text: string;
}

// The snapshot over every findings row, the one key the band and the open-question block read
interface Summary {
    readonly open: number;
    readonly questions: readonly string[];
}

interface Finding {
    readonly session: string;
    readonly file: string;
    readonly section: string;
    readonly evidence: string;
    readonly change: string;
    readonly kind: Kind;
    readonly status: Status;
    readonly proof: string;
    readonly ts: number;
}

// The ISO time each guidance part was last cleaned, null for a reset stamp
type Cleaned = Readonly<Partial<Record<Part, string | null>>>;

// The batch one session start spawned the orchestrator over: open rows of one kind, or a guidance part due for cleaning
type Dispatch =
    | { readonly spawnedAt: number; readonly rows: readonly string[]; readonly kind: Kind }
    | { readonly spawnedAt: number; readonly rows: readonly []; readonly part: Part };

// One rule hit of an edit-time scan under scan/<iso>-<rand>, the key's stamp is the time of the hit
interface Scan {
    readonly ruleId: string;
    readonly file: string;
}

interface Entry {
    readonly key: string;
    readonly value: unknown;
}

// --- [CONSTANTS] -----------------------------------------------------------------------

const NAMESPACES = [
    'secrets',
    'session',
    'injected',
    'loaded',
    'snapshot',
    'dns',
    'prompt',
    'findings',
    'summary',
    'notice',
    'cleaned',
    'dispatch',
    'skill',
    'scan',
    'roslyn',
] as const;

const KIND_NAMES = ['stale', 'wrong', 'narrowing', 'anchoring', 'scattered', 'restated', 'coined', 'missing', 'unread', 'narrative'] as const;

const STATUS = ['open', 'open-question', 'landed', 'closed'] as const;

// The parts of guidance a cleaning pass runs over, the keys of the cleaned stamps
const PART_NAMES = ['memory', 'rules', 'skills', 'agents', 'hooks', 'prose'] as const;

const _RANDOM_LENGTH = 8;
// The ISO stamp id() writes before the random tail, stampOf reads it back
const _ISO_LENGTH = new Date(0).toISOString().length;

// --- [KEYS] ----------------------------------------------------------------------------

const key = (namespace: Namespace, ...parts: readonly string[]): string => [namespace, ...parts].join('/');

// The keys under a namespace and its parts in a key list
const keys =
    (namespace: Namespace, ...parts: readonly string[]): ((all: readonly string[]) => readonly string[]) =>
    (all: readonly string[]): readonly string[] =>
        all.filter((candidate) => candidate.startsWith(`${key(namespace, ...parts)}/`));

// The id part of a key after the namespace and the parts
const suffix =
    (namespace: Namespace, ...parts: readonly string[]): ((storeKey: string) => string) =>
    (storeKey: string): string =>
        storeKey.slice(key(namespace, ...parts).length + 1);

// The ids under a namespace and its parts in a key list
const ids =
    (namespace: Namespace, ...parts: readonly string[]): ((all: readonly string[]) => readonly string[]) =>
    (all: readonly string[]): readonly string[] =>
        keys(namespace, ...parts)(all).map(suffix(namespace, ...parts));

// Row ids from a $.clock.now() value and a crypto.randomUUID() value
const id = (now: number, random: string): string => `${new Date(now).toISOString()}-${random.slice(0, _RANDOM_LENGTH)}`;

// The clock value an id() row id opens with, undefined when its ISO stamp does not parse
const stampOf = (rowId: string): number | undefined => {
    const at = Date.parse(rowId.slice(0, _ISO_LENGTH));
    return Number.isNaN(at) ? undefined : at;
};

// --- [REFINEMENTS] ---------------------------------------------------------------------

const isString: Guard<string> = (value): value is string => typeof value === 'string';

const isNumber: Guard<number> = (value): value is number => typeof value === 'number';

const isNullableString: Guard<string | null> = (value): value is string | null => value === null || isString(value);

const isRecord: Guard<Readonly<Record<string, unknown>>> = (value): value is Readonly<Record<string, unknown>> =>
    typeof value === 'object' && value !== null && !Array.isArray(value);

const isStringArray: Guard<readonly string[]> = (value): value is readonly string[] => Array.isArray(value) && value.every(isString);

const isKind: Guard<Kind> = (value): value is Kind => KIND_NAMES.some((kind) => kind === value);

const isPart: Guard<Part> = (value): value is Part => PART_NAMES.some((part) => part === value);

const _isStatus: Guard<Status> = (value): value is Status => STATUS.some((status) => status === value);

// The refinement widened to admit an absent value, the form of an optional field in a shape
const optional =
    <T>(guard: Guard<T>): Guard<T | undefined> =>
    (value): value is T | undefined =>
        value === undefined || guard(value);

// One refinement over unknown from a shape of field refinements, holds on a record whose every named field passes its own
const struct =
    <S extends Shape>(shape: S): Guard<Struct<S>> =>
    (value): value is Struct<S> =>
        isRecord(value) && Object.entries(shape).every(([field, guard]) => guard(value[field]));

const isStringRecord: Guard<StringRecord> = (value): value is StringRecord => isRecord(value) && Object.values(value).every(isString);

const isNotice: Guard<Notice> = struct({ text: isString });

const isSummary: Guard<Summary> = struct({ open: isNumber, questions: isStringArray });

const isFinding: Guard<Finding> = struct({
    session: isString,
    file: isString,
    section: isString,
    evidence: isString,
    change: isString,
    kind: isKind,
    status: _isStatus,
    proof: isString,
    ts: isNumber,
});

const isCleaned: Guard<Cleaned> = (value): value is Cleaned =>
    isRecord(value) && Object.entries(value).every(([name, part]) => isPart(name) && isNullableString(part));

const _isKindDispatch: Guard<Extract<Dispatch, { kind: Kind }>> = struct({ spawnedAt: isNumber, rows: isStringArray, kind: isKind });

const _isPartDispatch: Guard<Extract<Dispatch, { part: Part }>> = struct({
    spawnedAt: isNumber,
    rows: (value): value is readonly [] => Array.isArray(value) && value.length === 0,
    part: isPart,
});

const isDispatch: Guard<Dispatch> = (value): value is Dispatch => _isKindDispatch(value) || _isPartDispatch(value);

const isScan: Guard<Scan> = struct({ ruleId: isString, file: isString });

// --- [DECODERS] ------------------------------------------------------------------------

// The plugin's one JSON boundary, every text from the host or the model crosses it and a throw reads as undefined
const decodeJson = (text: string): unknown => {
    try {
        return JSON.parse(text);
    } catch {
        return undefined;
    }
};

// The value under a refinement, undefined for a value outside it
const decode =
    <T>(guard: Guard<T>): ((value: unknown) => T | undefined) =>
    (value: unknown): T | undefined =>
        guard(value) ? value : undefined;

// The findings rows of a list of entries with their keys, an entry outside the shape drops
const findings = (entries: readonly Entry[]): readonly (Entry & { readonly row: Finding })[] =>
    entries.flatMap((entry) => (isFinding(entry.value) ? [{ ...entry, row: entry.value }] : []));

// --- [EXPORTS] -------------------------------------------------------------------------

export type { Cleaned, Dispatch, Entry, Finding, Guard, Kind, Namespace, Notice, Part, Scan, Status, StringRecord, Summary };
export {
    decode,
    decodeJson,
    findings,
    id,
    ids,
    isCleaned,
    isDispatch,
    isFinding,
    isKind,
    isNotice,
    isNullableString,
    isNumber,
    isPart,
    isRecord,
    isScan,
    isString,
    isStringArray,
    isStringRecord,
    isSummary,
    KIND_NAMES,
    key,
    keys,
    NAMESPACES,
    optional,
    PART_NAMES,
    STATUS,
    stampOf,
    struct,
    suffix,
};
