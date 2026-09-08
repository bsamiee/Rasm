// Pure decisions over the findings rows and the cleaned stamps for the classifier, prompt.context, the close tool, the status line, and the timer

// --- [IMPORTS] -------------------------------------------------------------------------

import type { InvalidatableEventName, McpToolInputs, RenderSurface, ToolSpec } from 'claude-code';
import {
    flatMap,
    fromBoolean,
    fromNullable,
    fromPredicate,
    getOrElse,
    liftPredicate,
    map,
    type Option,
    optional,
    some,
    struct,
} from '../composition/option.ts';
import {
    type Cleaned,
    cleanedOf,
    type Dispatch,
    decodeFindings,
    decodeKeyedFindings,
    type Entry,
    type Finding,
    id,
    isCleaned,
    isPart,
    isString,
    type KeyedFinding,
    type Kind,
    key,
    PART_NAMES,
    type Part,
    type Status,
    type Summary,
    suffix,
} from '../host/store.ts';
import type { Batch } from './agents.ts';
import type { Fields } from './kinds.ts';

// --- [TYPES] ---------------------------------------------------------------------------

interface FindingInput {
    readonly session: string;
    readonly kind: Kind;
    readonly fields: Fields;
    readonly now: number;
    readonly random: string;
}

interface Closing {
    readonly writes: readonly Entry[];
    readonly result: unknown;
}

interface Question {
    readonly name: 'openQuestions';
    readonly text: string;
}

type CloseInput = McpToolInputs['mcp__function-hooks__close'];

// --- [CONSTANTS] -----------------------------------------------------------------------

const _OPEN: readonly Status[] = ['open', 'open-question'];
const _QUESTION_HEADER = 'Open questions from earlier findings, answer the ones you can:';
// The fields of one close row, the tool's schema and the row refinement read the one shape
const _ROW = { id: isString, verdict: isString, landedFile: isString, lines: isString, proof: isString };
const _ROW_FIELDS: readonly string[] = Object.keys(_ROW);
const _MS_PER_SECOND = 1000;
const _SECONDS_PER_MINUTE = 60;
const _MINUTES_PER_HOUR = 60;
const _HOURS_PER_DAY = 24;
const MS_PER_HOUR = _MINUTES_PER_HOUR * _SECONDS_PER_MINUTE * _MS_PER_SECOND;
const MS_PER_DAY = _HOURS_PER_DAY * MS_PER_HOUR;
const _DUE_DAYS = 30;
const DUE_MS = _DUE_DAYS * MS_PER_DAY;
// Open rows at the threshold make a kind batch
const _THRESHOLD = 5;

// The cached answers a finding change invalidates, the band and the open-question block both read the findings rows
const FINDING_VIEWS: readonly InvalidatableEventName[] = ['ui.render', 'prompt.context'];

const CLOSE: ToolSpec = {
    name: 'close',
    description:
        'Closes the findings of a batch. batchId alone lists the open rows. rows lands or closes each row. cleaned sets or resets the stamp of a guidance part, null resets it',
    inputSchema: {
        type: 'object',
        properties: {
            batchId: { type: 'string' },
            rows: {
                type: 'array',
                items: {
                    type: 'object',
                    properties: Object.fromEntries(_ROW_FIELDS.map((field) => [field, { type: 'string' }])),
                    required: _ROW_FIELDS,
                },
            },
            cleaned: { type: 'object', additionalProperties: { type: ['string', 'null'] } },
        },
        required: ['batchId'],
    },
};

// --- [OPERATIONS] ----------------------------------------------------------------------

const _isOpen = (row: Finding): boolean => _OPEN.includes(row.status);

const _idOf = suffix('findings');

const _isRow = struct(_ROW);

const _isRows = (value: unknown): value is NonNullable<CloseInput['rows']> => Array.isArray(value) && value.every(_isRow);

// The close tool's input, the served tool and the editor's final message both hold it
const _isClose: (value: unknown) => value is CloseInput = struct({ batchId: isString, rows: optional(_isRows), cleaned: optional(isCleaned) });

const decodeClose: (value: unknown) => Option<CloseInput> = fromPredicate(_isClose);

const _isList = fromPredicate(
    (input: CloseInput): input is CloseInput & { rows?: undefined; cleaned?: undefined } => input.rows === undefined && input.cleaned === undefined,
);

// One findings/<id> row from the classifier's fields
const finding = (input: FindingInput): KeyedFinding => {
    const rowId = id(input.now, input.random);
    return {
        key: key('findings', rowId),
        row: {
            session: input.session,
            file: input.fields.file,
            section: input.fields.section,
            evidence: input.fields.evidence,
            change: input.fields.change,
            kind: input.kind,
            // Changes that end in a question mark wait for the user's answer
            status: fromBoolean(input.fields.change.endsWith('?')).match<Status>({ some: () => 'open-question', none: () => 'open' }),
            proof: '',
            ts: input.now,
        },
    };
};

// Open-question rows wait for the person, the count and the batches read the open rows alone
const open = (rows: readonly Finding[]): number => rows.filter((row) => row.status === 'open').length;

const _questionLine = (row: Finding): string => `- ${row.file} ${row.section}: ${row.change}`;

// The summary row over every findings row, the snapshot the band and the open-question block read in place of a scan
const summarize = (rows: readonly Finding[]): Summary => ({
    open: open(rows),
    questions: rows.filter((row) => row.status === 'open-question').map(_questionLine),
});

// The summary after one new row, the classifier's write beside its findings row
const withFinding = (summary: Summary, row: Finding): Summary => {
    const added = summarize([row]);
    return { open: summary.open + added.open, questions: [...summary.questions, ...added.questions] };
};

// The prompt.context block over the summary's question lines, none when there is none
const questions = (summary: Summary): Option<Question> =>
    map((asked: readonly string[]): Question => ({ name: 'openQuestions', text: [_QUESTION_HEADER, ...asked].join('\n') }))(
        liftPredicate<readonly string[]>((lines) => lines.length > 0)(summary.questions),
    );

// The findings/ keys of landed or closed rows older than the due window, the prune that keeps one window of closed history
const expiredFindings = (entries: readonly Entry[], now: number): readonly string[] =>
    decodeKeyedFindings(entries)
        .filter((keyed) => !_isOpen(keyed.row) && now - keyed.row.ts > DUE_MS)
        .map((keyed) => keyed.key);

// The count line the status line and the band share
const openLine = (count: number): string => `${count} open findings`;

// Parts are due when their stamp is absent, null, unparsed, or older than the due window, with or without an open row
const due = (cleaned: Cleaned, now: number): readonly Part[] =>
    PART_NAMES.filter((part) =>
        // A stamp Date.parse reads as NaN compares false against the window and reads as due
        fromNullable(cleaned[part]).match<boolean>({ some: (stamp) => !(now - Date.parse(stamp) <= DUE_MS), none: () => true }),
    );

// The status line draws on a surface a person reads, a -p or SDK session starts with none
const status = (surface: RenderSurface | null, openCount: number, dueParts: readonly Part[]): Option<string> =>
    liftPredicate<string>(() => surface !== null && (openCount > 0 || dueParts.length > 0))(
        `${openLine(openCount)}${getOrElse(() => '')(liftPredicate<string>(() => dueParts.length > 0)(`, due: ${dueParts.join(' ')}`))}`,
    );

const _count = (rows: readonly Finding[], kind: Kind): number => rows.filter((row) => row.kind === kind).length;

// The most frequent open kind, none without an open row, the first seen wins a tie
const _batchKind = (rows: readonly Finding[]): Option<Kind> =>
    fromNullable([...new Set(rows.map((row) => row.kind))].toSorted((left, right) => _count(rows, right) - _count(rows, left))[0]);

const _kindBatch = (openEntries: readonly Entry[], openRows: readonly Finding[], now: number): Option<Dispatch> =>
    flatMap((kind: Kind) =>
        liftPredicate<Dispatch>(() => openEntries.length >= _THRESHOLD)({ rows: openEntries.map((entry) => entry.key), spawnedAt: now, kind }),
    )(_batchKind(openRows));

const _partBatch = (dueParts: readonly Part[], now: number): Option<Dispatch> =>
    map((part: Part): Dispatch => ({ rows: [], spawnedAt: now, part }))(fromNullable(dueParts[0]));

// The batch a tick dispatches, a kind batch when the open rows reach the threshold, else a part batch when a part is due, none otherwise
const batch = (entries: readonly Entry[], stamps: unknown, now: number, random: string): Option<Batch> => {
    const openEntries = entries.filter((entry) => open(decodeFindings([entry.value])) > 0);
    const openRows = decodeFindings(openEntries.map((entry) => entry.value));
    return map((dispatch: Dispatch): Batch => ({ batchId: id(now, random), dispatch }))(
        _kindBatch(openEntries, openRows, now).match<Option<Dispatch>>({
            some,
            none: () => _partBatch(due(cleanedOf(stamps), now), now),
        }),
    );
};

const _closedRows = (input: CloseInput, rows: readonly KeyedFinding[], now: number): readonly KeyedFinding[] =>
    (input.rows ?? []).flatMap((update) =>
        rows
            .filter((keyed) => _idOf(keyed.key) === update.id)
            .map((keyed) => ({
                key: keyed.key,
                row: {
                    ...keyed.row,
                    status: fromBoolean(update.verdict === 'landed').match<Status>({ some: () => 'landed', none: () => 'closed' }),
                    proof: update.proof,
                    ts: now,
                },
            })),
    );

// The stamps come from the input alone, a closed row names no part
const _stamps = (input: CloseInput, cleaned: Cleaned): Cleaned => ({
    ...cleaned,
    ...Object.fromEntries(Object.entries(input.cleaned ?? {}).filter(([name]) => isPart(name))),
});

// The close handler as a pure function, the writes the adapter hands to $.store.set and the tool's result
const close = (input: CloseInput, rows: readonly KeyedFinding[], cleaned: Cleaned, now: number): Closing =>
    _isList(input).match<Closing>({
        some: () => ({
            writes: [],
            result: { rows: rows.filter((keyed) => _isOpen(keyed.row)).map((keyed) => ({ id: _idOf(keyed.key), ...keyed.row })) },
        }),
        none: () => {
            const closed = _closedRows(input, rows, now);
            const closedKeys = closed.map((keyed) => keyed.key);
            const after = [...rows.filter((keyed) => !closedKeys.includes(keyed.key)), ...closed];
            return {
                writes: [
                    ...closed.map((keyed): Entry => ({ key: keyed.key, value: keyed.row })),
                    { key: key('summary'), value: summarize(after.map((keyed) => keyed.row)) },
                    { key: key('cleaned'), value: _stamps(input, cleaned) },
                ],
                result: { closed: closed.length },
            };
        },
    });

// --- [EXPORTS] -------------------------------------------------------------------------

export type { CloseInput, Closing, FindingInput, Question };
export {
    batch,
    CLOSE,
    close,
    DUE_MS,
    decodeClose,
    due,
    expiredFindings,
    FINDING_VIEWS,
    finding,
    MS_PER_DAY,
    MS_PER_HOUR,
    open,
    openLine,
    questions,
    status,
    summarize,
    withFinding,
};
