import type { ObjectModel } from '@rasm/automation';
import type { Envelope } from '@rasm/cep';
import { attempt, escaped, finite, illegalArgument, isList, isPlain, isRecord, map, message, readProperty, recordKeys, reply, unmatched } from '#/index.ts';

// --- [TYPES] ---------------------------------------------------------------------------

type Payload<Tag extends Envelope['tag']> = Extract<Envelope, { readonly tag: Tag }>['payload'];
type Fields = Readonly<Record<string, unknown>>;
type Declared = (typeof ObjectModel.Result.Encoded)['_tag'];
interface ApplicationHooks {
    readonly indexBase: number;
    readonly dialogs: () => () => void;
    readonly undoGroup?: (label: string) => () => void;
    readonly specifier: (value: object) => string | undefined;
    readonly open: (payload: Payload<'open'>) => object;
    readonly render: (payload: Payload<'render'>, resolve: (specifier: string) => readonly Fields[]) => void;
}

// --- [OPERATIONS] ----------------------------------------------------------------------

const ownKeys = (value: object): string[] => {
    const found: string[] = [];
    for (const key in value) {
        if (Object.prototype.hasOwnProperty.call(value, key)) {
            found.push(key);
        }
    }
    return found;
};
const within = <A>(acquire: () => () => void, body: () => A): A => {
    const release = acquire();
    try {
        return body();
    } finally {
        release();
    }
};
const reflectionOf = (value: unknown): Reflection | undefined => {
    const isReflection = (each: unknown): each is Reflection => isRecord(each) && typeof each.name === 'string' && isList(each.properties) && isList(each.methods) && isList(each.staticProperties);
    const reflection = readProperty(value, 'reflect');
    return isReflection(reflection) ? reflection : undefined;
};
const enumerated = (reflection: Reflection): boolean => reflection.staticProperties.length > 0 && reflection.properties.length === 0 && reflection.methods.length === 0;
const written = (target: object, values: Fields, prefix: string): string[] => {
    let refused: string[] = [];
    for (const key of recordKeys(values)) {
        const [value, held] = [values[key], readProperty(target, key)];
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
const assignProperties = <T extends object>(target: T, ...layers: readonly (Fields | undefined)[]): T => {
    let refused: string[] = [];
    for (const values of layers) {
        refused = refused.concat(written(target, values ?? {}, ''));
    }
    if (refused.length > 0) {
        throw illegalArgument(`${reflectionOf(target)?.name ?? Object.prototype.toString.call(target)} refused ${refused.join(', ')}`);
    }
    return target;
};
const preserve = (target: object, values: Fields): (() => void) => {
    const held: Record<string, unknown> = {};
    for (const key of recordKeys(values)) {
        held[key] = readProperty(target, key);
    }
    assignProperties(target, values);
    return () => {
        assignProperties(target, held);
    };
};
const fileFromUrl = (url: string): File => {
    const located = url.slice(url.indexOf('//') + 2);
    return new File(located.slice(located.indexOf('/')));
};
const specifierStep = (kind: string, selector?: readonly [index: number] | readonly [key: string, value: string | number]): string => {
    if (selector === undefined) {
        return `/${kind}`;
    }
    if (selector.length === 1) {
        return `/${kind}[${selector[0]}]`;
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
        if (isRecord(item) && String(readProperty(item, key)) === value) {
            return item;
        }
    }
    return undefined;
};
const named = (collection: Fields, base: number): string[] => {
    const names: string[] = [];
    for (let at = base; at < Number(collection.length) + base; at += 1) {
        try {
            names.push(String(readProperty(collection[at], 'name')));
        } catch (thrown) {
            names.push(`[${at - base}] ${message(thrown)}`);
        }
    }
    return names;
};
const stepped = (base: number, specifier: string, container: object, text: string): Fields => {
    const open = text.indexOf('[');
    const kind = open < 0 ? text : text.slice(0, open);
    const held = readProperty(container, open < 0 ? kind : `${kind}s`);
    const found = open < 0 || !isRecord(held) ? held : member(held, base, text.slice(open + 1, -1));
    if (!isRecord(found)) {
        throw unmatched(kind, specifier, open >= 0 && isRecord(held) ? named(held, base) : map(reflectionOf(container)?.properties ?? [], ({ name }) => name));
    }
    return found;
};
const resolver =
    (base: number) =>
    (specifier: string): Fields[] => {
        const chain: Fields[] = [];
        for (const text of specifier.match(/(?:[^/"]|"(?:[^"\\]|\\[\s\S])*"?)+/g) ?? []) {
            chain.push(stepped(base, specifier, chain[chain.length - 1] ?? app, text));
        }
        return chain;
    };
const declaration = (reflection: Reflection): unknown => {
    const described = (info: ReflectionInfo) => ({ name: info.name, description: info.description === '' ? undefined : info.description });
    const typed = (info: ReflectionInfo, extra: Fields) => {
        const numeric = info.dataType === 'number';
        return assignProperties(described(info), { type: info.dataType, types: [info.dataType], array: info.isCollection, minimum: numeric ? finite(info.min) : undefined, maximum: numeric ? finite(info.max) : undefined }, extra);
    };
    return enumerated(reflection)
        ? { _tag: 'Enumeration' satisfies Declared, name: reflection.name, members: map(reflection.staticProperties, described) }
        : {
              _tag: 'Class' satisfies Declared,
              name: reflection.name,
              parents: [],
              fields: map(reflection.properties, (info) => typed(info, { settable: info.type === 'readwrite' })),
              methods: map(reflection.methods, (info) => assignProperties(described(info), { parameters: map(info.arguments, (argument) => typed(argument, { optional: argument.defaultValue !== undefined, default: argument.defaultValue })), returns: { type: info.dataType } })),
          };
};
const reflections = (base: number): Record<string, Reflection> => {
    const found: Record<string, Reflection> = {};
    const declared = (name: string): Reflection | undefined => {
        const constructor = readProperty($.global, name);
        const reflection = typeof constructor === 'function' ? reflectionOf(constructor) : undefined;
        return reflection?.name === name ? reflection : undefined;
    };
    const followed = (item: unknown, reflection: Reflection | undefined): void => {
        if (reflection === undefined || found[reflection.name] !== undefined) {
            return;
        }
        found[reflection.name] = reflection;
        for (const { name, dataType } of reflection.properties) {
            if (dataType.charAt(0) !== dataType.charAt(0).toLowerCase() && dataType !== 'Object' && found[dataType] === undefined) {
                const live = readProperty(item, name);
                if (live === undefined) {
                    followed(undefined, declared(dataType));
                } else {
                    visit(live);
                }
            }
        }
    };
    const visit = (value: unknown): void => {
        const item = reflectionOf(value) === undefined ? readProperty(value, String(base)) : value;
        const owner = reflectionOf(readProperty(item, 'constructor'));
        followed(item, reflectionOf(item));
        followed(item, owner !== undefined && enumerated(owner) ? owner : undefined);
    };
    visit(app);
    for (const name in $.global) {
        followed(undefined, declared(name));
    }
    for (const { name } of reflectionOf($.global)?.properties ?? []) {
        followed(undefined, declared(name));
    }
    return found;
};
const model = (base: number, name: string | undefined): unknown => {
    const found = reflections(base);
    const [classes, enumerations]: [string[], string[]] = [[], []];
    for (const key of recordKeys(found)) {
        const reflection = found[key];
        if (reflection !== undefined && key.toLowerCase() === name?.toLowerCase()) {
            return declaration(reflection);
        }
        (reflection !== undefined && enumerated(reflection) ? enumerations : classes).push(key);
    }
    if (name !== undefined) {
        throw unmatched('class or enumeration', name, classes.concat(enumerations));
    }
    return { _tag: 'Index' satisfies Declared, classes, enumerations };
};

// --- [COMPOSITION] ---------------------------------------------------------------------

const handleEnvelope = ({ indexBase, dialogs, undoGroup, specifier, open, render }: ApplicationHooks) => {
    const resolve = resolver(indexBase);
    return (request: Envelope): string => {
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
                        throw illegalArgument(`${request.tag} code evaluates to ${typeof body}, not a function`);
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
        const grouped = group === undefined ? answer : () => within(() => group(request.label), answer);
        return reply(
            () => within(dialogs, grouped),
            specifier,
            (thrown) => request.tag === 'execute_script' && readProperty(thrown, 'source') === request.payload.code,
        );
    };
};

// --- [EXPORTS] -------------------------------------------------------------------------

export { assignProperties, fileFromUrl, handleEnvelope, ownKeys, preserve, reflectionOf, specifierStep };
