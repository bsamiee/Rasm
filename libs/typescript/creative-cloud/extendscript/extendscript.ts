import type { Envelope } from '@rasm/cep';
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

const { isRecord, isList, isPlain, finite, message, attempt, map, keys, read, unmatched, illegal, escaped, reply } = runtime();
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
    let refused: string[] = [];
    for (const key of keys(values)) {
        const [value, held] = [values[key], read(target, key)];
        if (isPlain(value) && isRecord(held)) {
            refused = refused.concat(written(held, value, `${prefix}${key}.`));
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
    let refused: string[] = [];
    for (const values of layers) {
        refused = refused.concat(written(target, values ?? {}, ''));
    }
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
const indexed = (kind: string, index: number): string => `/${kind}[${index}]`;
const step = (kind: string, selector?: readonly [key: string, value: string | number]): string => {
    if (selector === undefined) {
        return `/${kind}`;
    }
    return `/${kind}[@${selector[0]}=${typeof selector[1] === 'number' ? selector[1] : escaped(selector[1])}]`;
};
const member = (collection: Fields, base: number, selector: string): unknown => {
    const [split, length] = [selector.indexOf('='), Number(collection.length)];
    if (selector.charAt(0) !== '@' || split < 0) {
        const index = Number(selector);
        return attempt(() => collection[(index < 0 ? length + index : index) + base]);
    }
    const [key, raw] = [selector.slice(1, split), selector.slice(split + 1)];
    const hex = 16;
    const value = raw.charAt(0) === '"' ? raw.slice(1, -1).replace(/\\(?:u([\da-f]{4})|([\s\S]))/gi, (_, code?: string, character?: string) => (character === undefined || character === '' ? String.fromCharCode(parseInt(String(code), hex)) : character)) : raw;
    for (let at = base; at < length + base; at += 1) {
        const item = attempt(() => collection[at]);
        if (isRecord(item) && String(read(item, key)) === value) {
            return item;
        }
    }
    return undefined;
};
const named = (collection: Fields, base: number): string[] => {
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
const stepped = (base: number, specifier: string, container: Fields, text: string): Fields => {
    const open = text.indexOf('[');
    const kind = open < 0 ? text : text.slice(0, open);
    const held = read(container, open < 0 ? kind : `${kind}s`);
    const found = open < 0 || !isRecord(held) ? held : member(held, base, text.slice(open + 1, -1));
    if (!isRecord(found)) {
        throw unmatched(kind, specifier, open >= 0 && isRecord(held) ? named(held, base) : map(reflected(container)?.properties ?? [], ({ name }) => name));
    }
    return found;
};
const resolver =
    (base: number) =>
    (specifier: string): Fields[] => {
        const root: unknown = app;
        const chain: Fields[] = [];
        for (const text of specifier.match(/(?:[^/"]|"(?:[^"\\]|\\[\s\S])*"?)+/g) ?? []) {
            chain.push(stepped(base, specifier, chain[chain.length - 1] ?? (isRecord(root) ? root : {}), text));
        }
        return chain;
    };
const declaration = (reflection: Reflection): unknown => {
    const described = (info: ReflectionInfo) => ({ name: info.name, description: info.description === '' ? undefined : info.description });
    const typed = (info: ReflectionInfo, extra: Fields) => {
        const numeric = info.dataType === 'number';
        return assign(described(info), { type: info.dataType, types: [info.dataType], array: info.isCollection, minimum: numeric ? finite(info.min) : undefined, maximum: numeric ? finite(info.max) : undefined }, extra);
    };
    return enumerated(reflection)
        ? { _tag: 'Enumeration', name: reflection.name, members: map(reflection.staticProperties, described) }
        : {
              _tag: 'Class',
              name: reflection.name,
              parents: [],
              fields: map(reflection.properties, (info) => typed(info, { settable: info.type === 'readwrite' })),
              methods: map(reflection.methods, (info) => assign(described(info), { parameters: map(info.arguments, (argument) => typed(argument, { optional: argument.defaultValue !== undefined, default: argument.defaultValue })), returns: { type: info.dataType } })),
          };
};
const reflections = (base: number): Record<string, Reflection> => {
    const found: Record<string, Reflection> = {};
    const global: unknown = $.global;
    const declared = (name: string): Reflection | undefined => {
        const constructor = read(global, name);
        const reflection = typeof constructor === 'function' ? reflected(constructor) : undefined;
        return reflection?.name === name ? reflection : undefined;
    };
    const followed = (item: unknown, reflection: Reflection | undefined): void => {
        if (reflection === undefined || found[reflection.name] !== undefined) {
            return;
        }
        found[reflection.name] = reflection;
        for (const { name, dataType } of reflection.properties) {
            if (dataType.charAt(0) !== dataType.charAt(0).toLowerCase() && dataType !== 'Object' && found[dataType] === undefined) {
                const live = read(item, name);
                if (live === undefined) {
                    followed(undefined, declared(dataType));
                } else {
                    visit(live);
                }
            }
        }
    };
    const visit = (value: unknown): void => {
        const item = reflected(value) === undefined ? read(value, String(base)) : value;
        const owner = reflected(read(item, 'constructor'));
        followed(item, reflected(item));
        followed(item, owner !== undefined && enumerated(owner) ? owner : undefined);
    };
    visit(app);
    for (const name in isRecord(global) ? global : {}) {
        followed(undefined, declared(name));
    }
    for (const { name } of reflected(global)?.properties ?? []) {
        followed(undefined, declared(name));
    }
    return found;
};
const model = (base: number, name: string | undefined): unknown => {
    const found = reflections(base);
    const [classes, enumerations]: [string[], string[]] = [[], []];
    for (const key of keys(found)) {
        const reflection = found[key];
        if (reflection !== undefined && key.toLowerCase() === name?.toLowerCase()) {
            return declaration(reflection);
        }
        (reflection !== undefined && enumerated(reflection) ? enumerations : classes).push(key);
    }
    if (name !== undefined) {
        throw unmatched('class or enumeration', name, classes.concat(enumerations));
    }
    return { _tag: 'Index', classes, enumerations };
};

// --- [COMPOSITION] ---------------------------------------------------------------------

const handleEnvelope =
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
                        (target: string): unknown => resolve(target).pop() ?? app,
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
        const grouped = group === undefined ? answer : () => within(() => group.begin(request.label), group.end, answer);
        return reply(
            () => within(dialogs.suppress, dialogs.restore, grouped),
            specifier,
            (thrown) => request.tag === 'execute_script' && read(thrown, 'source') === request.payload.code,
        );
    };

// --- [EXPORTS] -------------------------------------------------------------------------

export { assign, file, handleEnvelope, illegal, indexed, keys, read, reflected, step };
