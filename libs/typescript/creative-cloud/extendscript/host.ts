import { type Containment, type Encoder, encode, isList, isPlain, isRecord, parse, read, reference, walk } from '@rasm/object-model/host';

// --- [TYPES] ---------------------------------------------------------------------------

type Fields = Readonly<Record<string, unknown>>;

interface Host {
    readonly containment: Containment;
    readonly session: () => () => void;
    readonly undo?: (label: string) => () => void;
}
interface Scope {
    readonly resolve: (target: string) => object;
    readonly grouped: <A>(body: () => A) => A;
}

// --- [CONSTANTS] -----------------------------------------------------------------------

const UNESCAPED = /[^ !#-[\]-~]/g;
const UNREFERENCED: Encoder = { reference: () => undefined };

// --- [OPERATIONS] ----------------------------------------------------------------------

const illegalArgument = (message: string) => ({ _tag: 'IllegalArgument', message }) as const;
const keys = (value: Fields): string[] => {
    const found: string[] = [];
    for (const key in value) {
        if (Object.prototype.hasOwnProperty.call(value, key) && value[key] !== undefined) {
            found.push(key);
        }
    }
    return found;
};
const quoted = (text: string) => `"${text.replace(UNESCAPED, (character) => `\\u${`000${character.charCodeAt(0).toString(16)}`.slice(-4)}`)}"`;
const written = (tree: unknown): string => {
    const parts: string[] = [];
    for (const item of isList(tree) ? tree : []) {
        parts.push(written(item ?? null));
    }
    for (const key of isPlain(tree) ? keys(tree) : []) {
        parts.push(`${quoted(key)}:${written(read(tree, key))}`);
    }
    return typeof tree === 'string' ? quoted(tree) : typeof tree === 'number' || typeof tree === 'boolean' ? String(tree) : isList(tree) ? `[${parts.join(',')}]` : isPlain(tree) ? `{${parts.join(',')}}` : 'null';
};
const reply = (answer: () => unknown, encoder: Encoder = UNREFERENCED, evaluated: (thrown: unknown) => boolean = () => true): string => {
    try {
        return written({ _tag: 'Value', value: encode(answer(), encoder) });
    } catch (error) {
        const [tag, line, number] = [read(error, '_tag'), read(error, 'line') ?? read(error, 'lineNumber'), read(error, 'number')];
        const message = String(isRecord(error) ? read(error, 'message') : error);
        return tag === 'Unmatched' || tag === 'IllegalArgument' ? written(error) : written({ _tag: 'Thrown', message, number: typeof number === 'number' ? number : undefined, line: evaluated(error) && typeof line === 'number' ? line : undefined });
    }
};
const within = <A>(acquire: () => () => void, body: () => A): A => {
    const release = acquire();
    try {
        return body();
    } finally {
        release();
    }
};
const refusals = (target: unknown, values: Fields, prefix: string): string[] => {
    let refused: string[] = [];
    for (const key of keys(values)) {
        const [value, held] = [values[key], read(target, key)];
        if (isPlain(value) && isRecord(held)) {
            refused = refused.concat(refusals(held, value, `${prefix}${key}.`));
        } else {
            try {
                // oxlint-disable-next-line no-param-reassign, typescript/no-unsafe-type-assertion -- ES3 has no Reflect.set, the host instance takes each key
                (target as Record<string, unknown>)[key] = value;
            } catch (error) {
                refused.push(`${prefix}${key}: ${String(read(error, 'message'))}`);
            }
        }
    }
    return refused;
};
const assign = <T extends object>(target: T, ...layers: readonly (Fields | undefined)[]): T => {
    let refused: string[] = [];
    for (const values of layers) {
        refused = refused.concat(refusals(target, values ?? {}, ''));
    }
    if (refused.length > 0) {
        throw illegalArgument(`${Object.prototype.toString.call(target)} refused ${refused.join(', ')}`);
    }
    return target;
};
const preserve = (target: object, values: Fields): (() => void) => {
    const held: Record<string, unknown> = {};
    for (const key of keys(values)) {
        held[key] = read(target, key);
    }
    assign(target, values);
    return () => {
        assign(target, held);
    };
};
const serve = <Payloads extends Fields>({ containment, session, undo }: Host, requests: { readonly [Tag in keyof Payloads]: (payload: Payloads[Tag], scope: Scope) => unknown }) => {
    const encoder: Encoder = { reference: (value) => reference(containment, value) };
    const resolve = (target: string): object => {
        const path = parse(target);
        const found = path === undefined ? undefined : walk(containment, path);
        if (isRecord(found) && read(found, '_tag') === 'Unmatched') {
            throw found;
        }
        if (!isRecord(found)) {
            throw illegalArgument(`${target} is no object specifier`);
        }
        return found;
    };
    return <Tag extends keyof Payloads>({ tag, label, payload }: { readonly tag: Tag; readonly label: string; readonly payload: Payloads[Tag] }): string =>
        reply(
            () => within(session, () => requests[tag](payload, { resolve, grouped: (body) => (undo === undefined ? body() : within(() => undo(label), body)) })),
            encoder,
            () => false,
        );
};

// --- [EXPORTS] -------------------------------------------------------------------------

export { assign, illegalArgument, keys, preserve, reply, serve, within, written };
