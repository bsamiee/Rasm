import { type Encoder, encode, isList, isPlain, isRecord, read } from '@rasm/object-model/host';

// --- [TYPES] ---------------------------------------------------------------------------

type Fields = Readonly<Record<string, unknown>>;

// --- [CONSTANTS] -----------------------------------------------------------------------

const UNESCAPED = /[^ !#-[\]-~]/g;
const UNREFERENCED: Encoder = { reference: () => undefined };

// --- [OPERATIONS] ----------------------------------------------------------------------

const illegalArgument = (message: string) => ({ _tag: 'IllegalArgument', message }) as const;
const keys = (value: Fields): string[] => Object.keys(value).filter((key) => value[key] !== undefined);
const quoted = (text: string) => `"${text.replace(UNESCAPED, (character) => `\\u${`000${character.charCodeAt(0).toString(16)}`.slice(-4)}`)}"`;
const written = (tree: unknown): string => {
    const items: string[] = [];
    for (const item of isList(tree) ? tree : []) {
        items.push(written(item));
    }
    for (const key of isPlain(tree) ? keys(tree) : []) {
        items.push(`${quoted(key)}:${written(read(tree, key))}`);
    }
    return typeof tree === 'string' ? quoted(tree) : typeof tree === 'number' || typeof tree === 'boolean' ? String(tree) : isList(tree) ? `[${items.join(',')}]` : isPlain(tree) ? `{${items.join(',')}}` : 'null';
};
const reply = (answer: () => unknown, encoder: Encoder = UNREFERENCED): string => {
    try {
        return written({ _tag: 'Value', value: encode(answer(), encoder) });
    } catch (error) {
        const [tag, line, number] = [read(error, '_tag'), read(error, 'line') ?? read(error, 'lineNumber'), read(error, 'number')];
        return written(tag === 'Unmatched' || tag === 'IllegalArgument' ? error : { _tag: 'Thrown', message: String(isRecord(error) ? read(error, 'message') : error), number: typeof number === 'number' ? number : undefined, line: typeof line === 'number' ? line : undefined });
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
const replaced = (target: unknown, values: Fields, prefix: string): { readonly held: Fields; readonly refused: readonly string[] } => {
    const named = keys(values);
    const held: Record<string, unknown> = {};
    let refused: readonly string[] = [];
    for (const key of named) {
        held[key] = read(target, key);
    }
    for (const key of named) {
        const [value, prior] = [values[key], held[key]];
        if (isPlain(value) && isRecord(prior)) {
            const nested = replaced(prior, value, `${prefix}${key}.`);
            [held[key], refused] = [nested.held, refused.concat(nested.refused)];
        } else {
            try {
                (target as Record<string, unknown>)[key] = value;
            } catch (error) {
                refused = refused.concat([`${prefix}${key}: ${String(read(error, 'message'))}`]);
            }
        }
    }
    return { held, refused };
};
const assign = (target: object, values: Fields): Fields => {
    const { held, refused } = replaced(target, values, '');
    if (refused.length > 0) {
        throw illegalArgument(`${Object.prototype.toString.call(target)} refused ${refused.join(', ')}`);
    }
    return held;
};
const preserve = (target: object, values: Fields): (() => void) => {
    const held = assign(target, values);
    return () => {
        assign(target, held);
    };
};

// --- [EXPORTS] -------------------------------------------------------------------------

export { assign, illegalArgument, keys, preserve, reply, within, written };
