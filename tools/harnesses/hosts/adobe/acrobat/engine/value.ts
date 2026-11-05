import type { Schema } from 'effect';

// --- [TYPES] ---------------------------------------------------------------------------

type Members = Record<string, unknown>;
type Callable = (...args: readonly unknown[]) => unknown;
interface Doc extends Members {
    readonly path: string;
    readonly numPages: number;
    readonly numFields: number;
    readonly info: Members;
    readonly bookmarkRoot: Members;
    metadata: string;
    closeDoc: (noSave: boolean) => void;
    saveAs: (path: string) => void;
    newPage: (page: number, width: number, height: number) => void;
    setPageRotations: (start: number, end: number, rotation: number) => void;
    setPageBoxes: (box: string, start: number, end: number, rect: readonly number[]) => void;
    getPageBox: (box: string, page: number) => unknown;
    getPageRotation: (page: number) => number;
    getNthFieldName: (index: number) => string;
    getField: (name: string) => Members | null;
    addField: (name: string, type: string, page: number, rect: readonly number[]) => Members;
    addAnnot: (properties: unknown) => Members;
    getAnnots: () => readonly Members[] | null;
    getOCGs: () => readonly Members[] | null;
    calculateNow: () => void;
}

// --- [CONSTANTS] -----------------------------------------------------------------------

const TAGS: Members = { undefined, NaN: Number.NaN, Infinity: Number.POSITIVE_INFINITY, '-Infinity': Number.NEGATIVE_INFINITY, '-0': -0 };
const DATED = 'date:';
const TAGGED = '[object ';
const VALUED = ['Object', 'Array', 'Date', 'Error', 'Function', 'Boolean', 'Number', 'String', 'RegExp'];

// --- [OPERATIONS] ----------------------------------------------------------------------

const has = <T>(list: readonly T[], item: T) => list.indexOf(item) >= 0;
const starts = (text: string, prefix: string) => text.lastIndexOf(prefix, 0) === 0;
const same = (left: unknown, right: unknown) => {
    if (typeof left !== 'number' || typeof right !== 'number') {
        return left === right;
    }
    return left === right ? 1 / left === 1 / right : Number.isNaN(left) && Number.isNaN(right);
};
const isMembers = (value: unknown): value is Members => value !== null && typeof value === 'object';
const isCallable = (value: unknown): value is Callable => typeof value === 'function';
const kindOf = (value: unknown) => Object.prototype.toString.call(value).slice(TAGGED.length, -1);
const isDate = (value: unknown): value is Date => kindOf(value) === 'Date';
const isDoc = (value: unknown): value is Doc => isMembers(value) && kindOf(value) === 'Doc';
const shaped = <T>(value: unknown, is: (held: unknown) => held is T, what: string): T => {
    if (is(value)) {
        return value;
    }
    throw new Error(`${what} is ${kindOf(value)}`);
};
const invoked = (target: Members, name: string, args: readonly unknown[]) => shaped(target[name], isCallable, name).apply(target, [...args]);
const enumerated = (target: Members) => {
    const names: string[] = [];
    // biome-ignore lint/suspicious/noForIn: native Acrobat objects enumerate their prototype members through for-in alone
    // biome-ignore lint/suspicious/useGuardForIn: native Acrobat objects enumerate their prototype members through for-in alone
    for (const key in target) {
        names.push(key);
    }
    return names;
};
const keyed = <T>(names: readonly string[], each: (key: string, index: number) => T): Record<string, T> => {
    const made: Record<string, T> = {};
    names.forEach((name, index) => {
        made[name] = each(name, index);
    });
    return made;
};
const counted = <T>(count: number, each: (index: number) => T): T[] => {
    const made: T[] = [];
    for (let index = 0; index < count; index += 1) {
        made.push(each(index));
    }
    return made;
};
const named = (value: Members) => {
    try {
        return typeof value.name === 'string' ? `/${value.name}` : '';
    } catch {
        return '';
    }
};
const scalar = (value: unknown): Schema.Json => {
    const [tag] = Object.keys(TAGS).filter((key) => same(TAGS[key], value));
    if (tag !== undefined) {
        return { $: tag };
    }
    if (value === null || typeof value === 'string' || typeof value === 'number' || typeof value === 'boolean') {
        return value;
    }
    return isCallable(value) ? '<function>' : String(value);
};
const described = (value: Members, inner: readonly unknown[]): Schema.Json => {
    const kind = kindOf(value);
    if (kind === 'Object') {
        return keyed(Object.keys(value), (key) => encoded(value[key], inner));
    }
    if (has(VALUED, kind)) {
        return String(invoked(value, 'toString', []));
    }
    if (kind === 'Annot' && isCallable(value.getProps)) {
        const props = shaped(value.getProps(), isMembers, 'Annot.getProps');
        return { $: `object:Annot${named(value)}`, ...keyed(Object.keys(props), (key) => encoded(props[key], inner)) };
    }
    return { $: `object:${kind}${isDoc(value) ? `/${value.path}` : named(value)}` };
};
const encoded = (value: unknown, seen: readonly unknown[] = []): Schema.Json => {
    if (isDate(value)) {
        return { $: `${DATED}${Number.isNaN(value.getTime()) ? 'NaN' : value.toISOString()}` };
    }
    if (!isMembers(value) || has(seen, value)) {
        return scalar(value);
    }
    const inner = seen.concat([value]);
    return Array.isArray(value) ? value.map((item) => encoded(item, inner)) : described(value, inner);
};
const revived = (value: unknown): unknown => {
    if (Array.isArray(value)) {
        return value.map(revived);
    }
    if (!isMembers(value)) {
        return value;
    }
    const names = Object.keys(value);
    const { $: tag } = value;
    if (typeof tag !== 'string' || names.length !== 1) {
        return keyed(names, (key) => revived(value[key]));
    }
    if (tag in TAGS) {
        return TAGS[tag];
    }
    // @effect-diagnostics-next-line globalDate:off
    return starts(tag, DATED) ? new Date(tag.slice(DATED.length)) : { $: tag };
};

// --- [EXPORTS] -------------------------------------------------------------------------

export { type Callable, counted, type Doc, encoded, enumerated, has, invoked, isCallable, isDoc, isMembers, keyed, kindOf, type Members, revived, shaped, starts };
