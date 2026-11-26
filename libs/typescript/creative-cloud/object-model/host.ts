import type { Unmatched } from '@rasm/search';
import type { Schema } from 'effect';

// --- [TYPES] ---------------------------------------------------------------------------

interface Step {
    readonly separator: '/' | '//';
    readonly kind: string;
    readonly selector?: { readonly _tag: 'Index'; readonly index: number } | { readonly _tag: 'Key'; readonly key: string; readonly value: number | string };
}
interface Specifier {
    readonly steps: readonly Step[];
    readonly end?: readonly [Step, ...Step[]];
}
interface Elements {
    items(owner: object): { readonly length: number } | null | undefined;
    readonly key?: Pick<Schema.Struct<Schema.Struct.Fields>, 'fields'>;
    readonly base?: number;
    readonly nested?: boolean;
}
type Kind = { is?(value: object): boolean; owner?(value: object): unknown } & (Elements | { property(owner: object): unknown });
interface Containment {
    readonly root: object;
    readonly kinds: Readonly<Record<string, Kind>>;
}
interface Encoder {
    readonly reference: (value: object) => { readonly type: string; readonly specifier: string; readonly valid: boolean; readonly name?: string } | undefined;
    readonly enumerator?: (value: object) => string | undefined;
    readonly collection?: (value: object) => Schema.Json | undefined;
    readonly replace?: (value: unknown) => unknown;
}

// --- [CONSTANTS] -----------------------------------------------------------------------

const RANGE = ' to ';

// --- [OPERATIONS] ----------------------------------------------------------------------

const isRecord = (value: unknown): value is Readonly<Record<string, unknown>> => (typeof value === 'object' || typeof value === 'function') && value !== null;
const isList = (value: unknown): value is readonly unknown[] => Array.isArray(value);
const isPlain = (value: unknown): value is Readonly<Record<string, unknown>> => isRecord(value) && Object.prototype.toString.call(value) === '[object Object]' && (Object.getPrototypeOf(value) ?? Object.prototype) === Object.prototype;
const attempt = <A, B>(get: (value: A) => B, value: A): B | undefined => {
    try {
        return get(value);
    } catch {
        return undefined;
    }
};
const read = (value: unknown, key: string): unknown => (isRecord(value) ? attempt((held) => held[key], value) : undefined);
const quote = (text: string) => `"${text.replace(/["\\]/g, (character) => `\\${character}`)}"`;
const unquote = (text: string) => text.replace(/\\(?:u([\dA-Fa-f]{4})|([\s\S]))/g, (_, code?: string, character?: string) => (code === undefined ? String(character) : String.fromCharCode(parseInt(code, 16))));
const formatSteps = (steps: Specifier['steps']): string =>
    steps.map(({ separator, kind, selector }) => `${separator}${kind}${selector === undefined ? '' : selector._tag === 'Index' ? `[${selector.index}]` : `[@${selector.key}=${typeof selector.value === 'number' ? selector.value : quote(selector.value)}]`}`).join('') || '/';
const format = ({ steps, end }: Specifier): string => (end === undefined ? formatSteps(steps) : `(${formatSteps(steps)}${RANGE}${formatSteps(end)})`);
const parseSteps = (text: string, from: number) => {
    const pattern = /(\/\/?)(@?[A-Za-z][\w-]*)(?:\[(?:(-?\d+)|@([^="\]]+)=(?:(-?\d+)|"((?:[^"\\]|\\[\s\S])*(?:"[^"\]]*"(?:[^"\\]|\\[\s\S])*)*)"))\])?/g;
    const steps: Step[] = [];
    let at = from;
    pattern.lastIndex = from;
    for (let found = pattern.exec(text); found?.index === at; found = pattern.exec(text)) {
        const [, separator, kind = '', index, key, number, value = ''] = found;
        const selector = index === undefined ? (key === undefined ? undefined : ({ _tag: 'Key', key, value: number === undefined ? unquote(value) : Number(number) } as const)) : ({ _tag: 'Index', index: Number(index) } as const);
        steps.push({ separator: separator === '//' ? '//' : '/', kind, ...(selector === undefined ? {} : { selector }) });
        at = pattern.lastIndex;
    }
    return { steps, at };
};
const isNonEmpty = (steps: readonly Step[]): steps is readonly [Step, ...Step[]] => steps.length > 0;
const parse = (text: string): Specifier | undefined => {
    const ranged = text.charAt(0) === '(';
    const start = parseSteps(text, ranged ? 1 : 0);
    const end = ranged && text.slice(start.at, start.at + RANGE.length) === RANGE ? parseSteps(text, start.at + RANGE.length) : undefined;
    if (!ranged) {
        return text === '/' || start.at === text.length ? { steps: start.steps } : undefined;
    }
    return end?.at === text.length - 1 && text.charAt(end.at) === ')' && isNonEmpty(start.steps) && isNonEmpty(end.steps) ? { steps: start.steps, end: end.steps } : undefined;
};
const elements = (container: unknown, row: Elements) => {
    const collection = isRecord(container) ? attempt(row.items, container) : undefined;
    return { length: Number(attempt((held) => held?.length, collection)), item: (index: number) => read(collection, String(index + (row.base ?? 0))) };
};
const items = (container: unknown, row: Elements): unknown[] => {
    const [{ length, item }, listed] = [elements(container, row), [] as unknown[]];
    for (let at = 0; at < length; at += 1) {
        listed.push(item(at));
    }
    return listed;
};
const findByKey = (container: unknown, row: Elements, key: string, value: number | string): unknown => {
    for (const item of items(container, row)) {
        const found = String(read(item, key)) === String(value) ? item : row.nested === true ? findByKey(item, row, key, value) : undefined;
        if (found !== undefined) {
            return found;
        }
    }
    return undefined;
};
const resolve = ({ root, kinds }: Containment, path: Specifier): unknown => {
    if (path.end !== undefined) {
        return { _tag: 'Unmatched', subject: 'range', query: format(path), candidates: [] } satisfies Pick<Unmatched, '_tag' | 'subject' | 'candidates'> & { readonly query: string };
    }
    let at: object = root;
    for (const { kind, selector: chosen } of path.steps) {
        const row = kinds[kind];
        const listed = row !== undefined && 'items' in row ? row : undefined;
        const { length, item } = chosen?._tag === 'Index' && listed !== undefined ? elements(at, listed) : { length: 0, item: (): unknown => undefined };
        const found =
            chosen === undefined
                ? row !== undefined && 'property' in row
                    ? attempt(row.property, at)
                    : read(at, kind.replace(/^@/, ''))
                : listed === undefined
                  ? undefined
                  : chosen._tag === 'Key'
                    ? findByKey(at, listed, chosen.key, chosen.value)
                    : chosen.index >= -length && chosen.index < length
                      ? item(chosen.index < 0 ? length + chosen.index : chosen.index)
                      : undefined;
        if (!isRecord(found)) {
            const candidates = row === undefined ? Object.keys(kinds) : chosen?._tag === 'Key' && listed !== undefined ? items(at, listed).map((each) => String(read(each, chosen.key))) : [];
            return { _tag: 'Unmatched', subject: length > 0 ? `${kind} among indexes 0 to ${length - 1}` : kind, query: format(path), candidates } satisfies Pick<Unmatched, '_tag' | 'subject' | 'candidates'> & { readonly query: string };
        }
        at = found;
    }
    return at;
};
const identify = (row: Elements, owner: unknown, value: object): Step['selector'] => {
    for (const key of row.key === undefined ? [] : Object.keys(row.key.fields)) {
        const held = read(value, key);
        if (typeof held === 'number' || (typeof held === 'string' && held !== '') || isRecord(held)) {
            return { _tag: 'Key', key, value: typeof held === 'number' ? held : String(held) };
        }
    }
    const index = items(owner, row).indexOf(value);
    return index === -1 ? undefined : { _tag: 'Index', index };
};
const specify = (containment: Containment, value: unknown): Step[] | undefined => {
    if (value === containment.root) {
        return [];
    }
    for (const kind in isRecord(value) ? containment.kinds : {}) {
        const row = containment.kinds[kind];
        const owner = isRecord(value) && row?.is?.(value) === true ? (row.owner === undefined ? containment.root : attempt(row.owner, value)) : undefined;
        const above = owner === undefined ? undefined : specify(containment, owner);
        const listed = row !== undefined && 'items' in row ? row : undefined;
        const chosen = above === undefined || listed === undefined || !isRecord(value) ? undefined : identify(listed, owner, value);
        if (above !== undefined && (listed === undefined || chosen !== undefined)) {
            return above.concat([{ separator: '/', kind, ...(chosen === undefined ? {} : { selector: chosen }) }]);
        }
    }
    return undefined;
};
const reference = (containment: Containment, value: object, type: string) => {
    const [steps, name] = [specify(containment, value), attempt(({ name: held }: { readonly name?: unknown }) => held, value)];
    return steps === undefined ? undefined : { type, specifier: formatSteps(steps), valid: true, ...(typeof name === 'string' ? { name } : {}) };
};
const encode = (value: unknown, host: Encoder): unknown => {
    if (!isRecord(value)) {
        return typeof value === 'number' ? (isFinite(value) ? value : null) : typeof value === 'bigint' || typeof value === 'symbol' ? String(value) : value;
    }
    if (isList(value)) {
        const encoded: unknown[] = [];
        for (const item of value) {
            encoded.push(encode(item, host) ?? null);
        }
        return encoded;
    }
    const described = host.enumerator?.(value) ?? host.collection?.(value) ?? host.reference(value);
    const replaced = described !== undefined || host.replace === undefined ? value : host.replace(value);
    const serializer = read(value, 'toJSON');
    const serialized: unknown = described === undefined && replaced === value && typeof serializer === 'function' ? serializer.call(value, '') : replaced;
    if (described !== undefined || serialized !== value) {
        return described ?? (serialized === undefined ? undefined : encode(serialized, host));
    }
    const record: Record<string, unknown> = {};
    for (const key in isPlain(value) ? value : {}) {
        const field = Object.prototype.hasOwnProperty.call(value, key) ? encode(read(value, key), host) : undefined;
        if (field !== undefined) {
            Object.defineProperty(record, key, { value: field, enumerable: true, writable: true, configurable: true });
        }
    }
    return isPlain(value) ? record : String(value);
};

// --- [EXPORTS] -------------------------------------------------------------------------

export { type Containment, type Encoder, encode, format, isList, isPlain, isRecord, parse, read, reference, resolve };
