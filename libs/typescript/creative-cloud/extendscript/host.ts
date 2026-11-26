import { type Encoder, encode, isList, isPlain, isRecord, read } from '@rasm/object-model/host';

// --- [CONSTANTS] -----------------------------------------------------------------------

const UNREFERENCED: Encoder = { reference: () => undefined };

// --- [OPERATIONS] ----------------------------------------------------------------------

const keys = (value: Readonly<Record<string, unknown>>): string[] => Object.keys(value).filter((key) => value[key] !== undefined);
const quote = (text: string) => `"${text.replace(/[^ !#-[\]-~]/g, (character) => `\\u${(character.charCodeAt(0) + 0x1_00_00).toString(16).slice(1)}`)}"`;
const stringify = (tree: unknown): string => {
    const items: string[] = [];
    for (const item of isList(tree) ? tree : []) {
        items.push(stringify(item));
    }
    for (const key of isPlain(tree) ? keys(tree) : []) {
        items.push(`${quote(key)}:${stringify(read(tree, key))}`);
    }
    return isList(tree) ? `[${items.join(',')}]` : isPlain(tree) ? `{${items.join(',')}}` : typeof tree === 'string' ? quote(tree) : (typeof tree === 'number' && isFinite(tree)) || typeof tree === 'boolean' ? String(tree) : 'null';
};
const reply = (answer: () => unknown, encoder: Encoder = UNREFERENCED): string => {
    try {
        return stringify({ _tag: 'Value', value: encode(answer(), encoder) });
    } catch (error) {
        const [tag, line, number] = [read(error, '_tag'), read(error, 'line') ?? read(error, 'lineNumber'), read(error, 'number')];
        return stringify(tag === 'Unmatched' || tag === 'IllegalArgument' ? error : { _tag: 'Thrown', message: String(read(error, 'message') ?? error), number: typeof number === 'number' ? number : undefined, line: typeof line === 'number' ? line : undefined });
    }
};
const replace = (target: unknown, values: Readonly<Record<string, unknown>>, prefix: string): { readonly held: Readonly<Record<string, unknown>>; readonly refused: readonly string[] } => {
    const named = keys(values);
    const held: Record<string, unknown> = {};
    let refused: readonly string[] = [];
    for (const key of named) {
        held[key] = read(target, key);
    }
    for (const key of named) {
        const [value, prior] = [values[key], held[key]];
        if (isPlain(value) && isRecord(prior)) {
            const nested = replace(prior, value, `${prefix}${key}.`);
            [held[key], refused] = [nested.held, refused.concat(nested.refused)];
        } else {
            try {
                (target as Record<string, unknown>)[key] = value;
            } catch (error) {
                [held[key], refused] = [undefined, refused.concat([`${prefix}${key}: ${String(read(error, 'message') ?? error)}`])];
            }
        }
    }
    return { held, refused };
};
const assign = (target: object, values: Readonly<Record<string, unknown>>): Readonly<Record<string, unknown>> => {
    const applied = replace(target, values, '');
    if (applied.refused.length > 0) {
        throw { _tag: 'IllegalArgument', message: `${Object.prototype.toString.call(target)} refused ${applied.refused.concat(replace(target, applied.held, '').refused).join(', ')}` };
    }
    return applied.held;
};
const preserve = <A>(target: object, values: Readonly<Record<string, unknown>>, body: () => A): A => {
    const held = assign(target, values);
    try {
        return body();
    } finally {
        assign(target, held);
    }
};

// --- [EXPORTS] -------------------------------------------------------------------------

export { assign, preserve, reply, stringify };
