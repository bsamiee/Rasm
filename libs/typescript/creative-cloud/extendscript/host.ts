import { type Encoder, encode, isList, isPlain, isRecord, read } from '@rasm/object-model/host';

// --- [TYPES] ---------------------------------------------------------------------------

type Fields = Readonly<Record<string, unknown>>;

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
        throw illegalArgument(`${String(read(read(target, 'reflect'), 'name') ?? Object.prototype.toString.call(target))} refused ${refused.join(', ')}`);
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
const located = (url: string): string => {
    const after = url.slice(url.indexOf('//') + 2);
    return after.slice(after.indexOf('/'));
};
const reflection = (value: unknown): unknown => {
    const found = read(value, 'reflect');
    return typeof read(found, 'name') === 'string' && isList(read(found, 'properties')) && isList(read(found, 'methods')) && isList(read(found, 'staticProperties')) ? found : undefined;
};
const enumerated = (found: unknown): boolean => Number(read(read(found, 'staticProperties'), 'length')) > 0 && Number(read(read(found, 'properties'), 'length')) === 0 && Number(read(read(found, 'methods'), 'length')) === 0;
const infos = (found: unknown, list: 'properties' | 'methods' | 'staticProperties' | 'arguments'): unknown[] => {
    const held = read(found, list);
    return isList(held) ? held.slice(0) : [];
};
const described = (info: unknown) => ({ name: String(read(info, 'name')), description: read(info, 'description') === '' ? undefined : read(info, 'description') });
const typed = (info: unknown, extra: Fields) => {
    const [type, minimum, maximum] = [String(read(info, 'dataType')), read(info, 'min'), read(info, 'max')];
    return assign(described(info), { type, types: [type], array: read(info, 'isCollection') === true, minimum: type === 'number' && typeof minimum === 'number' ? minimum : undefined, maximum: type === 'number' && typeof maximum === 'number' ? maximum : undefined }, extra);
};
const declared = (found: unknown) => {
    const [name, fields, methods, members] = [String(read(found, 'name')), [] as unknown[], [] as unknown[], [] as unknown[]];
    for (const info of infos(found, 'properties')) {
        fields.push(typed(info, { settable: read(info, 'type') === 'readwrite' }));
    }
    for (const info of infos(found, 'methods')) {
        const parameters: unknown[] = [];
        for (const argument of infos(info, 'arguments')) {
            parameters.push(typed(argument, { optional: read(argument, 'defaultValue') !== undefined, default: read(argument, 'defaultValue') }));
        }
        methods.push(assign(described(info), { parameters, returns: { type: String(read(info, 'dataType')) } }));
    }
    for (const info of infos(found, 'staticProperties')) {
        members.push(described(info));
    }
    return enumerated(found) ? { _tag: 'Enumeration', name, members } : { _tag: 'Class', name, parents: [], fields, methods };
};
const declaration = (global: unknown, root: object, base: number, name: string | undefined, dictionary: Readonly<Record<string, unknown>> = {}): unknown => {
    const found: Record<string, unknown> = {};
    const named = (type: string): unknown => {
        const constructor = read(global, type);
        const held = typeof constructor === 'function' ? reflection(constructor) : undefined;
        return read(held, 'name') === type ? held : undefined;
    };
    const followed = (item: unknown, held: unknown): void => {
        const type = String(read(held, 'name'));
        if (held === undefined || found[type] !== undefined) {
            return;
        }
        found[type] = held;
        for (const info of infos(held, 'properties')) {
            const kind = String(read(info, 'dataType'));
            const live = kind.charAt(0) !== kind.charAt(0).toLowerCase() && kind !== 'Object' && found[kind] === undefined ? read(item, String(read(info, 'name'))) : null;
            if (live === undefined) {
                followed(undefined, named(kind));
            }
            if (live !== undefined && live !== null) {
                visit(live);
            }
        }
    };
    const visit = (value: unknown): void => {
        const member = read(value, String(base));
        const item = reflection(value) === undefined ? member : value;
        const owner = reflection(read(item, 'constructor'));
        followed(item, reflection(item));
        followed(item, owner !== undefined && enumerated(owner) ? owner : undefined);
        if (item === value && isRecord(member) && found[String(read(reflection(member), 'name'))] === undefined) {
            visit(member);
        }
    };
    visit(root);
    for (const each in isRecord(global) ? global : {}) {
        followed(undefined, named(each));
    }
    for (const info of infos(reflection(global), 'properties')) {
        followed(undefined, named(String(read(info, 'name'))));
    }
    for (const key of keys(dictionary)) {
        found[key] ??= dictionary[key];
    }
    const [classes, enumerations]: [string[], string[]] = [[], []];
    for (const key of keys(found)) {
        if (key.toLowerCase() === name?.toLowerCase()) {
            return declared(found[key]);
        }
        (enumerated(found[key]) ? enumerations : classes).push(key);
    }
    if (name !== undefined) {
        throw { _tag: 'Unmatched', subject: 'class or enumeration', query: name, candidates: classes.concat(enumerations) };
    }
    return { _tag: 'Index', classes, enumerations };
};

// --- [EXPORTS] -------------------------------------------------------------------------

export { assign, declaration, illegalArgument, keys, located, preserve, reply, within, written };
