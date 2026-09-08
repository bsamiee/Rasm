// Store keys and decoders, one key per row under a namespace because the store has no compare-and-set

// --- [IMPORTS] -------------------------------------------------------------------------

import { fromPredicate, getOrElse, isRecord, liftPredicate, map, none, type Option, some, struct, toArray } from '../composition/option.ts';

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

// --- [TYPES] ---------------------------------------------------------------------------

type Namespace = (typeof NAMESPACES)[number];

type Kind = (typeof KIND_NAMES)[number];

type Status = (typeof STATUS)[number];

type Part = (typeof PART_NAMES)[number];

type Secrets = Readonly<Record<string, string>>;

// The variables `mise env --json` answers in the session's working directory, PATH and the mise.toml [env] rows, the init.env of every $.process.run
type Environment = Readonly<Record<string, string>>;

interface Session {
    readonly startedAt: number;
    readonly claudeChain: readonly string[];
    readonly memoryDir: string | null;
    readonly remoteOwner: string | null;
    readonly env: Environment;
}

interface Notice {
    readonly text: string;
}

// The snapshot over every findings row, the open count and one line per open-question row, the one key the band and the block read
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

type Cleaned = Readonly<Partial<Record<Part, string | null>>>;

// The value under the once-per-session keys of injected, loaded, snapshot, and dns, and under roslyn/<session> the last .cs write
interface Stamp {
    readonly session: string;
    readonly at: number;
}

// The batch the timer spawned the editor over, under dispatch/<batchId>: a kind batch over open rows, or a part cleaning batch with none
interface KindDispatch {
    readonly rows: readonly string[];
    readonly spawnedAt: number;
    readonly kind: Kind;
}

interface PartDispatch {
    readonly rows: readonly [];
    readonly spawnedAt: number;
    readonly part: Part;
}

type Dispatch = KindDispatch | PartDispatch;

// The durable stamp under skill/<name>, the newest session that loaded the skill, survives session.start's pruning
interface Skill {
    readonly loadedAt: number;
    readonly session: string;
}

// One rule hit of an edit-time scan under the durable scan/<iso>-<rand>, the key's stamp is the time of the hit
interface Scan {
    readonly ruleId: string;
    readonly file: string;
}

// Finding with the key it sits under, the shape the close tool reads and writes
interface KeyedFinding {
    readonly key: string;
    readonly row: Finding;
}

interface Entry {
    readonly key: string;
    readonly value: unknown;
}

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

// The clock value an id() row id opens with, none when its ISO stamp does not parse
const stampOf = (rowId: string): Option<number> => liftPredicate<number>((at) => !Number.isNaN(at))(Date.parse(rowId.slice(0, _ISO_LENGTH)));

// The value every once-per-session key holds, from the session id and a $.clock.now() value
const stamp = (session: string, at: number): Stamp => ({ session, at });

// --- [REFINEMENTS] ---------------------------------------------------------------------

const isString = (value: unknown): value is string => typeof value === 'string';

const isNumber = (value: unknown): value is number => typeof value === 'number';

const isNullableString = (value: unknown): value is string | null => value === null || isString(value);

const _isStringArray = (value: unknown): value is readonly string[] => Array.isArray(value) && value.every(isString);

// The field a dispatch row of the other kind lacks, and the empty rows of a part dispatch
const _isUndefined = (value: unknown): value is undefined => value === undefined;

const _isEmptyArray = (value: unknown): value is readonly [] => Array.isArray(value) && value.length === 0;

const isKind = (value: unknown): value is Kind => KIND_NAMES.some((kind) => kind === value);

const isPart = (value: unknown): value is Part => PART_NAMES.some((part) => part === value);

const _isStatus = (value: unknown): value is Status => STATUS.some((status) => status === value);

const _isStringRecord = (value: unknown): value is Readonly<Record<string, string>> => isRecord(value) && Object.values(value).every(isString);

const _isSecrets: (value: unknown) => value is Secrets = _isStringRecord;

const _isEnvironment: (value: unknown) => value is Environment = _isStringRecord;

const _isSession: (value: unknown) => value is Session = struct({
    startedAt: isNumber,
    claudeChain: _isStringArray,
    memoryDir: isNullableString,
    remoteOwner: isNullableString,
    env: _isEnvironment,
});

const _isNotice: (value: unknown) => value is Notice = struct({ text: isString });

const _isSummary: (value: unknown) => value is Summary = struct({ open: isNumber, questions: _isStringArray });

const _isStamp: (value: unknown) => value is Stamp = struct({ session: isString, at: isNumber });

const _isFinding: (value: unknown) => value is Finding = struct({
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

// The cleaned stamps as the store holds them and as the close input carries them, one refinement for both readers
const isCleaned = (value: unknown): value is Cleaned =>
    isRecord(value) && Object.entries(value).every(([name, part]) => isPart(name) && isNullableString(part));

const isKindDispatch: (value: unknown) => value is KindDispatch = struct({
    rows: _isStringArray,
    spawnedAt: isNumber,
    kind: isKind,
    part: _isUndefined,
});

const isPartDispatch: (value: unknown) => value is PartDispatch = struct({
    rows: _isEmptyArray,
    spawnedAt: isNumber,
    part: isPart,
    kind: _isUndefined,
});

const _isDispatch = (value: unknown): value is Dispatch => isKindDispatch(value) || isPartDispatch(value);

const _isSkill: (value: unknown) => value is Skill = struct({ loadedAt: isNumber, session: isString });

const _isScan: (value: unknown) => value is Scan = struct({ ruleId: isString, file: isString });

// --- [DECODERS] ------------------------------------------------------------------------

const decodeSecrets: (value: unknown) => Option<Secrets> = fromPredicate(_isSecrets);

const decodeEnvironment: (value: unknown) => Option<Environment> = fromPredicate(_isEnvironment);

const decodeSession: (value: unknown) => Option<Session> = fromPredicate(_isSession);

const decodeNotice: (value: unknown) => Option<Notice> = fromPredicate(_isNotice);

const decodeSummary: (value: unknown) => Option<Summary> = fromPredicate(_isSummary);

// The roslyn/<session> row, the once keys are read by presence and never decoded
const decodeStamp: (value: unknown) => Option<Stamp> = fromPredicate(_isStamp);

const decodeFinding: (value: unknown) => Option<Finding> = fromPredicate(_isFinding);

const decodeCleaned: (value: unknown) => Option<Cleaned> = fromPredicate(isCleaned);

const decodeDispatch: (value: unknown) => Option<Dispatch> = fromPredicate(_isDispatch);

const decodeSkill: (value: unknown) => Option<Skill> = fromPredicate(_isSkill);

const decodeScan: (value: unknown) => Option<Scan> = fromPredicate(_isScan);

// Every value under findings/ that decodes, the rows the events read together
const decodeFindings = (values: readonly unknown[]): readonly Finding[] => values.flatMap((value) => toArray(decodeFinding(value)));

const decodeKeyedFindings = (entries: readonly Entry[]): readonly KeyedFinding[] =>
    entries.flatMap((entry) => toArray(map((row: Finding): KeyedFinding => ({ key: entry.key, row }))(decodeFinding(entry.value))));

// The plugin's one JSON boundary, every text from the host or the model crosses it and a throw reads as none
const decodeJson = (text: string): Option<unknown> => {
    try {
        return some(JSON.parse(text));
    } catch {
        return none();
    }
};

// Absent or malformed secrets or cleaned values read as the empty record, the seed and the first close are outside the plugin's own writes
const secretsOf = (value: unknown): Secrets => getOrElse((): Secrets => ({}))(decodeSecrets(value));

const cleanedOf = (value: unknown): Cleaned => getOrElse((): Cleaned => ({}))(decodeCleaned(value));

// --- [EXPORTS] -------------------------------------------------------------------------

export type {
    Cleaned,
    Dispatch,
    Entry,
    Environment,
    Finding,
    KeyedFinding,
    Kind,
    KindDispatch,
    Namespace,
    Notice,
    Part,
    PartDispatch,
    Scan,
    Secrets,
    Session,
    Skill,
    Stamp,
    Status,
    Summary,
};
export {
    cleanedOf,
    decodeCleaned,
    decodeDispatch,
    decodeEnvironment,
    decodeFinding,
    decodeFindings,
    decodeJson,
    decodeKeyedFindings,
    decodeNotice,
    decodeScan,
    decodeSecrets,
    decodeSession,
    decodeSkill,
    decodeStamp,
    decodeSummary,
    id,
    ids,
    isCleaned,
    isKind,
    isKindDispatch,
    isNullableString,
    isNumber,
    isPart,
    isPartDispatch,
    isString,
    KIND_NAMES,
    key,
    keys,
    NAMESPACES,
    PART_NAMES,
    STATUS,
    secretsOf,
    stamp,
    stampOf,
    suffix,
};
