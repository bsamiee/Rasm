import type { Unmatched } from '@rasm/search/unmatched';
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
    find(owner: object, key: string, value: number | string): unknown;
    readonly key?: Pick<Schema.Struct<Schema.Struct.Fields>, 'fields'>;
    readonly base?: number;
}
type Kind = { is?(value: object): boolean; owner?(value: object): unknown } & (Elements | { property(owner: object): unknown });
type Keys<F, T> = { readonly [K in keyof F]: K extends keyof T ? (F[K] extends Schema.Top ? (T[K] extends F[K]['Type'] | undefined ? F[K] : never) : never) : never };
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
const classOf = (value: object): string => {
    const named = attempt(({ typename, constructor }: { readonly typename?: unknown; readonly constructor?: unknown }) => (typeof typename === 'string' ? typename : read(constructor, 'name')), value);
    return typeof named === 'string' ? named : '';
};
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
const elements = (container: unknown, row: Pick<Elements, 'items' | 'base'> | undefined) => {
    const collection = row !== undefined && isRecord(container) ? attempt(row.items, container) : undefined;
    const length = Number(read(collection, 'length') ?? 0);
    return { length, item: (index: number) => (index >= 0 && index < length ? read(collection, String(index + (row?.base ?? 0))) : undefined) };
};
const items = (container: unknown, row: Pick<Elements, 'items' | 'base'>): unknown[] => {
    const [{ length, item }, listed] = [elements(container, row), [] as unknown[]];
    for (let at = 0; at < length; at += 1) {
        listed.push(item(at));
    }
    return listed;
};
const findByKey = (container: unknown, row: Pick<Elements, 'items' | 'base'> & { readonly nested?: boolean }, key: string, value: number | string): unknown => {
    const { length, item } = elements(container, row);
    for (let at = 0; at < length; at += 1) {
        const each = item(at);
        const found = String(read(each, key)) === String(value) ? each : row.nested === true ? findByKey(each, row, key, value) : undefined;
        if (found !== undefined) {
            return found;
        }
    }
    return undefined;
};
const resolve = ({ root, kinds }: Containment, path: Specifier): unknown => {
    let [at, missed]: [object, { readonly subject: string; readonly candidates: readonly string[] } | undefined] = [root, path.end === undefined ? undefined : { subject: 'range', candidates: [] }];
    for (const { kind, selector } of missed === undefined ? path.steps : []) {
        const row = kinds[kind];
        const listed = row !== undefined && 'items' in row ? row : undefined;
        const { length, item } = elements(at, selector?._tag === 'Index' ? listed : undefined);
        const found = selector === undefined ? attempt(row !== undefined && 'property' in row ? row.property : (owner: object) => read(owner, kind.replace(/^@/, '')), at) : selector._tag === 'Key' ? listed?.find(at, selector.key, selector.value) : item(selector.index < 0 ? length + selector.index : selector.index);
        if (!isRecord(found)) {
            missed = { subject: length > 0 ? `${kind} among indexes 0 to ${length - 1}` : kind, candidates: row === undefined ? Object.keys(kinds) : selector?._tag === 'Key' && listed !== undefined ? items(at, listed).map((each) => String(read(each, selector.key))) : [] };
            break;
        }
        at = found;
    }
    return missed === undefined ? at : ({ _tag: 'Unmatched', query: format(path), ...missed } satisfies Pick<Unmatched, '_tag' | 'subject' | 'candidates'> & { readonly query: string });
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
const specify = (containment: Containment, value: object): Step[] | undefined => {
    if (value === containment.root) {
        return [];
    }
    const named = classOf(value);
    const own = named.charAt(0).toLowerCase() + named.slice(1);
    for (const kind in containment.kinds) {
        const row = containment.kinds[kind];
        const owner = row !== undefined && (row.is === undefined ? kind === own : row.is(value)) ? (row.owner === undefined ? containment.root : attempt(row.owner, value)) : undefined;
        const above = isRecord(owner) ? specify(containment, owner) : undefined;
        const listed = row !== undefined && 'items' in row ? row : undefined;
        const chosen = above !== undefined && listed !== undefined ? identify(listed, owner, value) : undefined;
        if (above !== undefined && (listed === undefined || chosen !== undefined)) {
            return above.concat([{ separator: '/', kind, ...(chosen === undefined ? {} : { selector: chosen }) }]);
        }
    }
    return undefined;
};
const Containment = {
    kind: <O extends object, T extends object, const F extends Keys<F, T> = Record<never, never>>(
        row: { readonly is?: (value: object) => value is T; owner?(value: NoInfer<T>): unknown } & (
            | {
                  items(owner: O): ArrayLike<T> | { readonly length: number } | null | undefined;
                  readonly key?: { readonly fields: F };
                  readonly by?: { readonly [K in keyof NoInfer<F>]?: (owner: O, value: number | string) => unknown };
                  readonly base?: number;
                  readonly nested?: boolean;
              }
            | { property(owner: O): T | null | undefined }
        ),
    ): Kind => {
        if (!('items' in row)) {
            return row;
        }
        const lookups: Readonly<Record<string, ((owner: O, value: number | string) => unknown) | undefined>> = row.by ?? {};
        return { ...row, find: (owner: O, key: string, value: number | string) => attempt((held) => (lookups[key] ?? ((each: O) => findByKey(each, row, key, value)))(held, value), owner) };
    },
};
const reference = (containment: Containment, value: object) => {
    const [steps, name] = [specify(containment, value), read(value, 'name')];
    return steps === undefined ? undefined : { type: classOf(value), specifier: formatSteps(steps), valid: true, ...(typeof name === 'string' ? { name } : {}) };
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
    if (described !== undefined) {
        return described;
    }
    const replaced = host.replace === undefined ? value : host.replace(value);
    const serializer = read(value, 'toJSON');
    const serialized: unknown = replaced === value && typeof serializer === 'function' ? serializer.call(value, '') : replaced;
    if (serialized !== value) {
        return serialized === undefined ? undefined : encode(serialized, host);
    }
    if (!isPlain(value)) {
        return String(value);
    }
    const record: Record<string, unknown> = {};
    for (const key in value) {
        const field = Object.prototype.hasOwnProperty.call(value, key) ? encode(read(value, key), host) : undefined;
        if (field !== undefined) {
            Object.defineProperty(record, key, { value: field, enumerable: true, writable: true, configurable: true });
        }
    }
    return record;
};

// --- [EXPORTS] -------------------------------------------------------------------------

export { Containment, type Encoder, encode, format, isList, isPlain, isRecord, type Kind, parse, read, reference, resolve };
