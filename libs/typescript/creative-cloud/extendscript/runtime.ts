import type { Completion } from '@rasm/cep';

// --- [TYPES] ---------------------------------------------------------------------------

type Replied<Tag extends (typeof Completion.Encoded)['_tag']> = Extract<typeof Completion.Encoded, { readonly _tag: Tag }>;

// --- [CONSTANTS] -----------------------------------------------------------------------

const HEX = 16;
const HEX_WIDTH = -4;
const UNESCAPED = /[^ !#-[\]-~]/g;

// --- [OPERATIONS] ----------------------------------------------------------------------

const isRecord = (value: unknown): value is Readonly<Record<string, unknown>> => typeof value === 'object' && value !== null;
const isList = (value: unknown): value is readonly unknown[] => Object.prototype.toString.call(value) === '[object Array]';
const isPlain = (value: unknown): value is Readonly<Record<string, unknown>> => isRecord(value) && Object.prototype.toString.call(value) === '[object Object]';
const finite = (value: unknown): number | undefined => (typeof value === 'number' && isFinite(value) ? value : undefined);
const message = (thrown: unknown): string => String(isRecord(thrown) ? thrown.message : thrown);
const attempt = <A>(body: () => A): A | undefined => {
    try {
        return body();
    } catch {
        return undefined;
    }
};
const map = <A, B>(list: readonly A[], each: (item: A) => B): B[] => {
    const out: B[] = [];
    for (const item of list) {
        out.push(each(item));
    }
    return out;
};
const recordKeys = (value: Readonly<Record<string, unknown>>): string[] => {
    const found: string[] = [];
    for (const key in value) {
        if (Object.prototype.hasOwnProperty.call(value, key) && value[key] !== undefined) {
            found.push(key);
        }
    }
    return found;
};
const readProperty = (value: unknown, key: string): unknown => attempt(() => (isRecord(value) ? value[key] : undefined));
const unmatched = (subject: string, query: string, candidates: readonly string[]): Replied<'Unmatched'> => ({ _tag: 'Unmatched', subject, query, candidates });
const illegalArgument = (text: string): Replied<'IllegalArgument'> => ({ _tag: 'IllegalArgument', message: text });
const escaped = (text: string): string => `"${text.replace(UNESCAPED, (character) => `\\u${`000${character.charCodeAt(0).toString(HEX)}`.slice(HEX_WIDTH)}`)}"`;
const encoded = (value: unknown, specifier: (value: object) => string | undefined = () => undefined): string => {
    if (typeof value === 'string') {
        return escaped(value);
    }
    if (typeof value === 'boolean' || finite(value) !== undefined) {
        return String(value);
    }
    if (value === null || value === undefined || typeof value === 'number') {
        return 'null';
    }
    if (isList(value)) {
        return `[${map(value, (item) => encoded(item, specifier)).join(',')}]`;
    }
    return isPlain(value) ? `{${map(recordKeys(value), (key) => `${escaped(key)}:${encoded(value[key], specifier)}`).join(',')}}` : described(value, specifier);
};
const described = (value: unknown, specifier: (value: object) => string | undefined): string => {
    const at = isRecord(value) ? specifier(value) : undefined;
    const [type, name] = [readProperty(readProperty(value, 'reflect'), 'name'), readProperty(value, 'name')];
    return at === undefined ? escaped(String(value)) : encoded({ type: typeof type === 'string' ? type : String(value), specifier: at, name: typeof name === 'string' ? name : undefined, valid: true }, specifier);
};
const reply = (answer: () => unknown, specifier = (_value: object): string | undefined => undefined, evaluated = (_thrown: unknown): boolean => true): string => {
    try {
        return encoded({ _tag: 'Value' satisfies Replied<'Value'>['_tag'], value: answer() }, specifier);
    } catch (thrown) {
        if (isRecord(thrown) && (thrown._tag === 'Unmatched' || thrown._tag === 'IllegalArgument')) {
            return encoded(thrown, specifier);
        }
        const line = evaluated(thrown) ? finite(readProperty(thrown, 'line') ?? readProperty(thrown, 'lineNumber')) : undefined;
        return encoded({ _tag: 'Thrown', message: message(thrown), number: finite(readProperty(thrown, 'number')), line } satisfies Omit<Replied<'Thrown'>, 'number' | 'line'> & Record<'number' | 'line', number | undefined>, specifier);
    }
};

// --- [EXPORTS] -------------------------------------------------------------------------

export { attempt, encoded, escaped, finite, illegalArgument, isList, isPlain, isRecord, map, message, readProperty, recordKeys, reply, unmatched };
