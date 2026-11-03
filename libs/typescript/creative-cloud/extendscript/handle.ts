import type { Envelope } from '@rasm/cep/contract';
import { runtime } from '#/runtime';

// --- [TYPES] ---------------------------------------------------------------------------

type Payload<Tag extends Envelope['tag']> = Extract<Envelope, { readonly tag: Tag }>['payload'];
type Fields = Readonly<Record<string, unknown>>;
interface Host<Held> {
    readonly indexBase: number;
    readonly dialogs: { readonly suppress: () => Held; readonly restore: (held: Held) => void };
    readonly undoGroup?: { readonly begin: (label: string) => void; readonly end: () => void };
    readonly specifier: (value: object) => string | undefined;
    readonly open: (payload: Payload<'open'>) => object;
    readonly render: (payload: Payload<'render'>, resolve: (specifier: string) => readonly Fields[]) => void;
}

// --- [OPERATIONS] ----------------------------------------------------------------------

const { isRecord, isList, isPlain, finite, message, attempt, keys, read, unmatched, illegal, escaped, reply } = runtime();
const within = <Held, A>(enter: () => Held, leave: (held: Held) => void, body: () => A): A => {
    const held = enter();
    try {
        return body();
    } finally {
        leave(held);
    }
};
const reflected = (value: unknown): Reflection | undefined => {
    const isReflection = (each: unknown): each is Reflection => isRecord(each) && typeof each.name === 'string' && isList(each.properties) && isList(each.methods) && isList(each.staticProperties);
    return attempt(() => {
        const reflection: unknown = (typeof value === 'function' || isRecord(value)) && 'reflect' in value ? value.reflect : undefined;
        return isReflection(reflection) ? reflection : undefined;
    });
};
const enumerated = (reflection: Reflection): boolean => reflection.staticProperties.length > 0 && reflection.properties.length === 0 && reflection.methods.length === 0;
const written = (target: object, values: Fields, prefix: string): string[] => {
    const refused: string[] = [];
    for (const key of keys(values)) {
        const [value, held] = [values[key], read(target, key)];
        if (isPlain(value) && isRecord(held)) {
            for (const field of written(held, value, `${prefix}${key}.`)) {
                refused.push(field);
            }
        } else {
            try {
                // biome-ignore lint/nursery/noUnsafeTypeAssertion: types-for-adobe declares host option fields as statics, the host instance takes each
                (target as unknown as Record<string, unknown>)[key] = value;
            } catch (thrown) {
                refused.push(`${prefix}${key}: ${message(thrown)}`);
            }
        }
    }
    return refused;
};
const assign = <T extends object>(target: T, ...layers: readonly (Fields | undefined)[]): T => {
    const merged: Record<string, unknown> = {};
    for (const values of layers) {
        for (const key of keys(values ?? {})) {
            merged[key] = values?.[key];
        }
    }
    const refused = written(target, merged, '');
    if (refused.length > 0) {
        throw illegal(`${reflected(target)?.name ?? Object.prototype.toString.call(target)} refused ${refused.join(', ')}`);
    }
    return target;
};
const file = (url: string): File => {
    const located = url.slice(url.indexOf('//') + 2);
    const constructor: FileConstructor = $.global.File;
    return new constructor(located.slice(located.indexOf('/')));
};
const unescaped = (quoted: string): string => {
    const hex = 16;
    const digits = 4;
    let out = '';
    for (let at = 1; at < quoted.length - 1; at += 1) {
        const character = quoted.charAt(at);
        const next = quoted.charAt(at + 1);
        if (character !== '\\') {
            out += character;
        } else if (next === 'u') {
            out += String.fromCharCode(parseInt(quoted.slice(at + 2, at + 2 + digits), hex));
            at += digits + 1;
        } else {
            out += next;
            at += 1;
        }
    }
    return out;
};
const indexed = (kind: string, index: number): string => `/${kind}[${index}]`;
const step = (kind: string, selector?: readonly [key: string, value: string | number]): string => {
    if (selector === undefined) {
        return `/${kind}`;
    }
    const [key, value] = selector;
    return `/${kind}[@${key}=${typeof value === 'number' ? value : escaped(value)}]`;
};
const member = (collection: Fields, base: number, selector: string): unknown => {
    const [split, length] = [selector.indexOf('='), Number(collection.length)];
    if (selector.charAt(0) !== '@' || split < 0) {
        const index = Number(selector);
        return attempt(() => collection[(index < 0 ? length + index : index) + base]);
    }
    const [key, raw] = [selector.slice(1, split), selector.slice(split + 1)];
    const value = raw.charAt(0) === '"' ? unescaped(raw) : raw;
    for (let at = base; at < length + base; at += 1) {
        const item = attempt(() => collection[at]);
        if (isRecord(item) && String(read(item, key)) === value) {
            return item;
        }
    }
    return undefined;
};
const steps = (specifier: string): string[] => {
    const found: string[] = [];
    let [text, quoted] = ['', false];
    for (let at = 0; at < specifier.length; at += 1) {
        const character = specifier.charAt(at);
        if (quoted && character === '\\') {
            text += character + specifier.charAt(at + 1);
            at += 1;
        } else if (character === '/' && !quoted) {
            found.push(text);
            text = '';
        } else {
            quoted = character === '"' ? !quoted : quoted;
            text += character;
        }
    }
    found.push(text);
    return found;
};
const stepped = (base: number, specifier: string) => {
    const named = (collection: Fields): string[] => {
        const names: string[] = [];
        for (let at = base; at < Number(collection.length) + base; at += 1) {
            try {
                names.push(String(read(collection[at], 'name')));
            } catch (thrown) {
                names.push(`[${at - base}] ${message(thrown)}`);
            }
        }
        return names;
    };
    const properties = (container: Fields): string[] => {
        const names: string[] = [];
        for (const { name } of reflected(container)?.properties ?? []) {
            names.push(name);
        }
        return names;
    };
    return (container: Fields, text: string): Fields => {
        const open = text.indexOf('[');
        const kind = open < 0 ? text : text.slice(0, open);
        const held = read(container, open < 0 ? kind : `${kind}s`);
        const found = open < 0 || !isRecord(held) ? held : member(held, base, text.slice(open + 1, -1));
        if (!isRecord(found)) {
            throw unmatched(kind, specifier, open >= 0 && isRecord(held) ? named(held) : properties(container));
        }
        return found;
    };
};
const resolver =
    (base: number) =>
    (specifier: string): Fields[] => {
        const next = stepped(base, specifier);
        const root: unknown = app;
        const chain: Fields[] = [];
        for (const text of steps(specifier)) {
            if (text !== '') {
                chain.push(next(chain[chain.length - 1] ?? (isRecord(root) ? root : {}), text));
            }
        }
        return chain;
    };
const declaration = (reflection: Reflection) => {
    const described = (info: ReflectionInfo) => {
        const numeric = info.dataType === 'number';
        return { name: info.name, type: info.dataType, types: [info.dataType], description: info.description === '' ? undefined : info.description, array: info.isCollection, minimum: numeric ? finite(info.min) : undefined, maximum: numeric ? finite(info.max) : undefined };
    };
    const fields: unknown[] = [];
    const methods: unknown[] = [];
    for (const info of reflection.properties) {
        fields.push(assign(described(info), { settable: info.type === 'readwrite' }));
    }
    for (const info of reflection.methods) {
        const parameters: unknown[] = [];
        for (const argument of info.arguments) {
            parameters.push(assign(described(argument), { optional: argument.defaultValue !== undefined, default: argument.defaultValue }));
        }
        methods.push({ name: info.name, description: info.description === '' ? undefined : info.description, parameters, returns: { type: info.dataType } });
    }
    return { _tag: 'Class', name: reflection.name, parents: [], fields, methods };
};
const reflections = (base: number): Record<string, Reflection | undefined> => {
    const found: Record<string, Reflection | undefined> = {};
    const global: unknown = $.global;
    const names: string[] = [];
    const declared = (name: string): Reflection | undefined => {
        const constructor = read(global, name);
        const reflection = typeof constructor === 'function' ? reflected(constructor) : undefined;
        return reflection?.name === name ? reflection : undefined;
    };
    const followed = (item: unknown, reflection: Reflection): void => {
        found[reflection.name] = reflection;
        for (const { name, dataType } of reflection.properties) {
            if (dataType.charAt(0) !== dataType.charAt(0).toLowerCase() && dataType !== 'Object' && found[dataType] === undefined) {
                const live = read(item, name);
                const constructed = live === undefined ? declared(dataType) : undefined;
                if (constructed === undefined) {
                    visit(live);
                } else {
                    followed(undefined, constructed);
                }
            }
        }
    };
    const visit = (value: unknown): void => {
        const item = reflected(value) === undefined ? read(value, String(base)) : value;
        const owner = reflected(read(item, 'constructor'));
        for (const reflection of [reflected(item), owner !== undefined && enumerated(owner) ? owner : undefined]) {
            if (reflection !== undefined && found[reflection.name] === undefined) {
                followed(item, reflection);
            }
        }
    };
    visit(app);
    for (const name in isRecord(global) ? global : {}) {
        names.push(name);
    }
    for (const { name } of reflected(global)?.properties ?? []) {
        names.push(name);
    }
    for (const name of names) {
        const reflection = declared(name);
        if (reflection !== undefined && found[name] === undefined) {
            followed(undefined, reflection);
        }
    }
    return found;
};
const model = (base: number, name: string | undefined): unknown => {
    const found = reflections(base);
    const [classes, enumerations]: [string[], string[]] = [[], []];
    for (const key of keys(found)) {
        const reflection = found[key];
        (reflection !== undefined && enumerated(reflection) ? enumerations : classes).push(key);
    }
    if (name === undefined) {
        return { _tag: 'Index', classes, enumerations };
    }
    let match: Reflection | undefined;
    for (const key of keys(found)) {
        match = match ?? (key.toLowerCase() === name.toLowerCase() ? found[key] : undefined);
    }
    if (match === undefined) {
        throw unmatched('class or enumeration', name, classes.concat(enumerations));
    }
    const members: unknown[] = [];
    for (const info of match.staticProperties) {
        members.push({ name: info.name, description: info.description === '' ? undefined : info.description });
    }
    return enumerated(match) ? { _tag: 'Enumeration', name: match.name, members } : declaration(match);
};

// --- [COMPOSITION] ---------------------------------------------------------------------

const handle =
    <Held>({ indexBase, dialogs, undoGroup, specifier, open, render }: Host<Held>) =>
    (request: Envelope): string => {
        const resolve = resolver(indexBase);
        const group = request.tag === 'execute_script' || request.tag === 'render' ? undoGroup : undefined;
        const answer = (): unknown => {
            switch (request.tag) {
                case 'execute_script': {
                    // biome-ignore lint/nursery/noImpliedEval: execute_script compiles caller source to run in the host
                    const body: unknown = new Function('alert', 'confirm', 'prompt', 'resolve', 'code', 'return eval(code)')(
                        () => undefined,
                        () => false,
                        () => null,
                        (target: string): unknown => {
                            const chain = resolve(target);
                            return chain[chain.length - 1] ?? app;
                        },
                        request.payload.code,
                    );
                    if (typeof body !== 'function') {
                        throw illegal(`${request.tag} code evaluates to ${typeof body}, not a function`);
                    }
                    return { result: body() };
                }
                case 'open':
                    return open(request.payload);
                case 'render':
                    return render(request.payload, resolve);
                case 'object_model':
                    return model(indexBase, request.payload.name);
                default:
                    return request satisfies never;
            }
        };
        return reply(
            () => within(dialogs.suppress, dialogs.restore, () => (group === undefined ? answer() : within(() => group.begin(request.label), group.end, answer))),
            specifier,
            (thrown) => request.tag === 'execute_script' && read(thrown, 'source') === request.payload.code,
        );
    };

// --- [EXPORTS] -------------------------------------------------------------------------

export { assign, file, handle, illegal, indexed, keys, read, reflected, step };
