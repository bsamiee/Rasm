import type { Unmatched } from '@rasm/search';
import type { Schema } from 'effect';

// --- [TYPES] ---------------------------------------------------------------------------

type Step = NonNullable<ReturnType<typeof stepped>>[number];
interface Path {
    readonly steps: readonly Step[];
    readonly end?: readonly Step[];
}
interface Contained {
    readonly collection?: string;
    readonly keys?: readonly string[];
    readonly base?: number;
    readonly nested?: boolean;
    readonly is: (value: object) => boolean;
    readonly owner?: (value: object) => unknown;
}
interface Containment {
    readonly root: object;
    readonly type: (value: object) => string;
    readonly kinds: Readonly<Record<string, Contained>>;
}
interface Encoder {
    readonly reference: (value: object) => { readonly type: string; readonly specifier: string; readonly valid: boolean; readonly name?: string } | undefined;
    readonly enumerator?: (value: object) => string | undefined;
    readonly collection?: (value: object) => Schema.Json | undefined;
    readonly replace?: (value: unknown) => unknown;
}

// --- [CONSTANTS] -----------------------------------------------------------------------

const KEYS = ['id', 'uuid', 'fullName', 'name'] as const;
const RANGE = ' to ';
const INTEGER = /^-?\d+$/;

// --- [OPERATIONS] ----------------------------------------------------------------------

const isRecord = (value: unknown): value is Readonly<Record<string, unknown>> => (typeof value === 'object' || typeof value === 'function') && value !== null;
const isList = (value: unknown): value is readonly unknown[] => Object.prototype.toString.call(value) === '[object Array]';
const isPlain = (value: unknown): value is Readonly<Record<string, unknown>> => isRecord(value) && Object.prototype.toString.call(value) === '[object Object]' && value.constructor === Object;
const read = (value: unknown, key: string): unknown => {
    try {
        return isRecord(value) ? value[key] : undefined;
    } catch {
        return undefined;
    }
};
const quoted = (text: string) => `"${text.replace(/["\\]/g, (character) => `\\${character}`)}"`;
const unquoted = (text: string) => text.replace(/\\(?:u([\dA-Fa-f]{4})|([\s\S]))/g, (_, code?: string, character?: string) => (code === undefined ? String(character) : String.fromCharCode(parseInt(code, 16))));
const written = (steps: Path['steps']): string => {
    let text = '';
    for (const { separator, kind, selector } of steps) {
        text += `${separator}${kind}${selector === undefined ? '' : selector._tag === 'Index' ? `[${selector.index}]` : `[@${selector.key}=${typeof selector.value === 'number' ? selector.value : quoted(selector.value)}]`}`;
    }
    return text === '' ? '/' : text;
};
const format = ({ steps, end }: Path): string => (end === undefined ? written(steps) : `(${written(steps)}${RANGE}${written(end)})`);
const closing = (text: string, open: number): number => {
    let quoting = false;
    for (let at = open + 1; at < text.length; at += 1) {
        const character = text.charAt(at);
        if (character === ']' && !quoting) {
            return at;
        }
        quoting = character === '"' ? !quoting : quoting;
        at += character === '\\' && quoting ? 1 : 0;
    }
    return -1;
};
const selector = (inside: string) => {
    const [split, raw] = [inside.indexOf('='), inside.slice(inside.indexOf('=') + 1)];
    const [counted, quoting] = [INTEGER.test(raw), raw.length > 1 && raw.charAt(0) === '"' && raw.charAt(raw.length - 1) === '"'];
    return INTEGER.test(inside) ? ({ _tag: 'Index', index: Number(inside) } as const) : inside.charAt(0) === '@' && split > 1 && (counted || quoting) ? ({ _tag: 'Key', key: inside.slice(1, split), value: counted ? Number(raw) : unquoted(raw.slice(1, -1)) } as const) : undefined;
};
const stepped = (text: string) => {
    const pattern = /(\/\/?)(@?[A-Za-z][\w-]*)/g;
    const steps: { readonly separator: '/' | '//'; readonly kind: string; readonly selector?: NonNullable<ReturnType<typeof selector>> }[] = [];
    let at = 0;
    for (let found = pattern.exec(text); found?.index === at; found = pattern.exec(text)) {
        const [separator, kind, open] = [found[1] === '//' ? ('//' as const) : ('/' as const), String(found[2]), pattern.lastIndex];
        const close = text.charAt(open) === '[' ? closing(text, open) : open - 1;
        const chosen = close > open ? selector(text.slice(open + 1, close)) : undefined;
        if (close < open - 1 || (close > open && chosen === undefined)) {
            at = -1;
            break;
        }
        steps.push(chosen === undefined ? { separator, kind } : { separator, kind, selector: chosen });
        at = close + 1;
        pattern.lastIndex = at;
    }
    return at === text.length ? steps : undefined;
};
const parse = (text: string) => {
    const split = text.indexOf(RANGE);
    const ranged = text.charAt(0) === '(' && text.charAt(text.length - 1) === ')' && split > 0;
    const steps = ranged ? stepped(text.slice(1, split)) : text === '/' ? [] : stepped(text);
    const end = ranged ? stepped(text.slice(split + RANGE.length, -1)) : undefined;
    return steps === undefined || (ranged && end === undefined) ? undefined : end === undefined ? { steps } : { steps, end };
};
const items = (container: unknown, row: Contained): unknown[] => {
    const [collection, base, listed] = [read(container, String(row.collection)), row.base ?? 0, [] as unknown[]];
    for (let at = base; at < Number(read(collection, 'length')) + base; at += 1) {
        listed.push(read(collection, String(at)));
    }
    return listed;
};
const keyed = (container: unknown, row: Contained, key: string, value: number | string): unknown => {
    for (const item of items(container, row)) {
        const found = String(read(item, key)) === String(value) ? item : row.nested === true ? keyed(item, row, key, value) : undefined;
        if (found !== undefined) {
            return found;
        }
    }
    return undefined;
};
const walk = ({ root, kinds }: Containment, path: Path): unknown => {
    let at: unknown = root;
    for (const { kind, selector: chosen } of path.end === undefined ? path.steps : []) {
        const row = kinds[kind];
        const collection = row === undefined ? undefined : read(at, String(row.collection));
        const length = chosen?._tag === 'Index' ? Number(read(collection, 'length')) : 0;
        const found =
            chosen === undefined
                ? read(at, kind.replace(/^@/, ''))
                : row === undefined
                  ? undefined
                  : chosen._tag === 'Key'
                    ? keyed(at, row, chosen.key, chosen.value)
                    : chosen.index >= -length && chosen.index < length
                      ? read(collection, String((chosen.index < 0 ? length + chosen.index : chosen.index) + (row.base ?? 0)))
                      : undefined;
        if (!isRecord(found)) {
            const candidates: string[] = [];
            for (const name in row === undefined ? kinds : {}) {
                candidates.push(name);
            }
            for (const item of row !== undefined && chosen?._tag === 'Key' ? items(at, row) : []) {
                candidates.push(String(read(item, String(read(chosen, 'key')))));
            }
            return { _tag: 'Unmatched', subject: length > 0 ? `${kind} among indexes 0 to ${length - 1}` : kind, query: format(path), candidates } satisfies Pick<Unmatched, '_tag' | 'subject' | 'candidates'> & { readonly query: string };
        }
        at = found;
    }
    return path.end === undefined ? at : ({ _tag: 'Unmatched', subject: 'range', query: format(path), candidates: [] } satisfies Pick<Unmatched, '_tag' | 'subject' | 'candidates'> & { readonly query: string });
};
const identified = (row: Contained, owner: unknown, value: object): Step['selector'] => {
    for (const key of row.keys ?? KEYS) {
        const held = read(value, key);
        if (typeof held === 'number' || (typeof held === 'string' && held !== '') || isRecord(held)) {
            return { _tag: 'Key', key, value: typeof held === 'number' ? held : String(held) };
        }
    }
    const listed = items(owner, row);
    for (let index = 0; index < listed.length; index += 1) {
        if (listed[index] === value) {
            return { _tag: 'Index', index };
        }
    }
    return undefined;
};
const specified = (containment: Containment, value: unknown): Step[] | undefined => {
    for (const kind in isRecord(value) && value !== containment.root ? containment.kinds : {}) {
        const row = containment.kinds[kind];
        const owner = isRecord(value) && row?.is(value) === true ? (row.owner === undefined ? containment.root : row.owner(value)) : undefined;
        const above = owner === undefined ? undefined : specified(containment, owner);
        const chosen = row?.collection === undefined || !isRecord(value) ? undefined : identified(row, owner, value);
        if (above !== undefined && (row?.collection === undefined || chosen !== undefined)) {
            return above.concat([chosen === undefined ? { separator: '/', kind } : { separator: '/', kind, selector: chosen }]);
        }
    }
    return value === containment.root ? [] : undefined;
};
const reference = (containment: Containment, value: object) => {
    const [steps, name] = [specified(containment, value), read(value, 'name')];
    return steps === undefined ? undefined : typeof name === 'string' ? { type: containment.type(value), specifier: written(steps), name, valid: true } : { type: containment.type(value), specifier: written(steps), valid: true };
};
const encode = (value: unknown, host: Encoder): unknown => {
    const described = !isRecord(value) || isList(value) ? undefined : (host.enumerator?.(value) ?? host.collection?.(value) ?? host.reference(value));
    const replaced = !isRecord(value) || isList(value) || host.replace === undefined ? value : host.replace(value);
    const list: unknown[] = [];
    const record: Record<string, unknown> = {};
    for (const item of isList(value) ? value : []) {
        list.push(encode(item, host) ?? null);
    }
    for (const key in described === undefined && replaced === value && isPlain(value) ? value : {}) {
        const field = Object.prototype.hasOwnProperty.call(value, key) ? encode(read(value, key), host) : undefined;
        if (field !== undefined) {
            record[key] = field;
        }
    }
    if (typeof value === 'number' || !isRecord(value)) {
        return typeof value === 'number' ? (isFinite(value) ? value : null) : typeof value === 'bigint' || typeof value === 'symbol' ? String(value) : value;
    }
    return isList(value) ? list : described !== undefined || replaced !== value ? (described ?? (replaced === undefined ? undefined : encode(replaced, host))) : isPlain(value) ? record : String(value);
};

// --- [EXPORTS] -------------------------------------------------------------------------

export { type Containment, type Encoder, encode, format, isList, isPlain, isRecord, parse, read, reference, walk };
