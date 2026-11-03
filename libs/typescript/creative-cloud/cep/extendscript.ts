import type { Requests } from '@rasm/automation/requests';
import type { Unmatched } from '@rasm/search/unmatched';
import type { Rpc, RpcGroup } from 'effect/rpc';

// --- [TYPES] ---------------------------------------------------------------------------

type Request = RpcGroup.Rpcs<typeof Requests>;
type Payload<Tag extends Request['_tag']> = Rpc.ExtractTag<Request, Tag>['payloadSchema']['Encoded'];
type Fields = Readonly<Record<string, unknown>>;
interface Scope {
    readonly resolve: (specifier: string) => unknown;
    readonly assign: <T extends object>(target: T, values: Fields) => T;
    readonly path: (url: string) => string;
}
interface Host {
    readonly base: number;
    readonly suppressed: <A>(body: () => A) => A;
    readonly undone: <A>(label: string, body: () => A) => A;
    readonly open: (payload: Payload<'open'>, scope: Scope) => Rpc.ExtractTag<Request, 'open'>['successSchema']['Encoded'];
    readonly render: (payload: Payload<'render'>, scope: Scope) => void;
}

// --- [OPERATIONS] ----------------------------------------------------------------------

const isRecord = (value: unknown): value is Fields => typeof value === 'object' && value !== null;
const isList = (value: unknown): value is readonly unknown[] => Object.prototype.toString.call(value) === '[object Array]';
const attempt = <A>(body: () => A): A | undefined => {
    try {
        return body();
    } catch {
        return undefined;
    }
};
const keys = (value: object): string[] => {
    const found: string[] = [];
    // biome-ignore lint/suspicious/noForIn: ExtendScript holds no Object.keys
    for (const key in value) {
        // biome-ignore lint/suspicious/noPrototypeBuiltins: ExtendScript holds no Object.hasOwn
        if (Object.prototype.hasOwnProperty.call(value, key)) {
            found.push(key);
        }
    }
    return found;
};
const assign = <T extends object>(target: T, values: Fields): T => {
    for (const key of keys(values)) {
        // biome-ignore lint/nursery/noUnsafeTypeAssertion: types-for-adobe declares host option fields as statics, the host instance takes each
        (target as unknown as Record<string, unknown>)[key] = values[key];
    }
    return target;
};
const unmatched = (subject: string, query: string, candidates: readonly string[]): typeof Unmatched.Encoded => ({ _tag: 'Unmatched', subject, query, candidates });
const illegal = (message: string) => ({ _tag: 'IllegalArgument', message });
const read = (value: unknown, key: string): unknown => attempt(() => (isRecord(value) ? value[key] : undefined));
const reflected = (value: unknown): Reflection | undefined => {
    const isReflection = (each: unknown): each is Reflection => isRecord(each) && typeof each.name === 'string' && isList(each.properties) && isList(each.methods) && isList(each.staticProperties);
    return attempt(() => {
        const reflection: unknown = (typeof value === 'function' || isRecord(value)) && 'reflect' in value ? value.reflect : undefined;
        return isReflection(reflection) ? reflection : undefined;
    });
};
const enumerated = (reflection: Reflection): boolean => reflection.staticProperties.length > 0 && reflection.properties.length === 0 && reflection.methods.length === 0;
const escaped = (text: string): string => {
    const control = 0x20;
    const line = 0x20_28;
    const paragraph = 0x20_29;
    const hex = 16;
    const digits = -4;
    let out = '';
    for (let at = 0; at < text.length; at += 1) {
        const code = text.charCodeAt(at);
        const character = text.charAt(at);
        out += code < control || character === '"' || character === '\\' || code === line || code === paragraph ? `\\u${`000${code.toString(hex)}`.slice(digits)}` : character;
    }
    return `"${out}"`;
};
const encoded = (value: unknown): string => {
    if (typeof value === 'string') {
        return escaped(value);
    }
    if (typeof value === 'boolean' || (typeof value === 'number' && Math.abs(value) < Number.POSITIVE_INFINITY)) {
        return String(value);
    }
    if (value === null || value === undefined || typeof value === 'number') {
        return 'null';
    }
    return isRecord(value) && (isList(value) || Object.prototype.toString.call(value) === '[object Object]') ? joined(value, isList(value)) : escaped(String(value));
};
const joined = (value: Fields, list: boolean): string => {
    const parts: string[] = [];
    for (const key of keys(value)) {
        if (list) {
            parts.push(encoded(value[key]));
        } else if (value[key] !== undefined) {
            parts.push(`${escaped(key)}:${encoded(value[key])}`);
        }
    }
    return list ? `[${parts.join(',')}]` : `{${parts.join(',')}}`;
};
const items = (collection: Fields, base: number): Fields[] => {
    const found: Fields[] = [];
    for (let at = base; at < Number(collection.length) + base; at += 1) {
        const item = attempt(() => collection[at]);
        if (isRecord(item)) {
            found.push(item);
        }
    }
    return found;
};
const member = (collection: Fields, base: number, selector: string): unknown => {
    const split = selector.indexOf('=');
    const form = split < 0 ? '' : selector.slice(0, split);
    const value = selector.slice(split + 1);
    if (form === '@id') {
        for (const item of items(collection, base)) {
            if (item.id === Number(value)) {
                return item;
            }
        }
        return undefined;
    }
    return form === '@name' ? attempt((): unknown => (typeof collection.getByName === 'function' ? collection.getByName(value.slice(1, -1)) : undefined)) : attempt(() => collection[Number(selector) + base]);
};
const stepped = (base: number, specifier: string) => {
    const named = (collection: Fields): string[] => {
        const names: string[] = [];
        for (const item of items(collection, base)) {
            names.push(String(item.name));
        }
        return names;
    };
    return (container: Fields, step: string): Fields => {
        const open = step.indexOf('[');
        const kind = open < 0 ? step : step.slice(0, open);
        const held = read(container, open < 0 ? kind : `${kind}s`);
        const collection: Fields = isRecord(held) ? held : {};
        const found = open < 0 ? held : member(collection, base, step.slice(open + 1, -1));
        if (step !== '' && !isRecord(found)) {
            throw unmatched(kind, specifier, named(collection));
        }
        return isRecord(found) ? found : container;
    };
};
const resolver = (base: number) => (specifier: string) => {
    const next = stepped(base, specifier);
    let container: unknown = app;
    for (const step of specifier.split('/')) {
        container = next(isRecord(container) ? container : {}, step);
    }
    return container;
};
const declaration = (reflection: Reflection) => {
    const described = (info: ReflectionInfo) => ({ name: info.name, type: info.dataType, types: [info.dataType], description: info.description === '' ? undefined : info.description, array: info.isCollection ? true : undefined });
    const fields: unknown[] = [];
    const methods: unknown[] = [];
    for (const info of reflection.properties) {
        fields.push(assign(described(info), { settable: info.type === 'readwrite' }));
    }
    for (const info of reflection.methods) {
        const parameters: unknown[] = [];
        for (const argument of info.arguments) {
            parameters.push(assign(described(argument), { optional: argument.defaultValue !== undefined }));
        }
        methods.push({ name: info.name, description: info.description === '' ? undefined : info.description, parameters, returns: { type: info.dataType } });
    }
    return { _tag: 'Class', name: reflection.name, parents: [], fields, methods };
};
const reflections = (base: number): Record<string, Reflection | undefined> => {
    const found: Record<string, Reflection | undefined> = {};
    const followed = (item: unknown, reflection: Reflection): void => {
        found[reflection.name] = reflection;
        for (const { name, dataType } of reflection.properties) {
            if (dataType.charAt(0) !== dataType.charAt(0).toLowerCase() && dataType !== 'Object' && found[dataType] === undefined) {
                visit(read(item, name));
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
    ({ base, suppressed, undone, open, render }: Host) =>
    (request: { readonly [Tag in Request['_tag']]: { readonly tag: Tag; readonly label: string; readonly payload: Payload<Tag> } }[Request['_tag']]): string => {
        const resolve = resolver(base);
        const scope = { resolve, assign, path: (url: string): string => decodeURIComponent(url.slice(url.indexOf('//') + 2)) };
        const answer = (): unknown => {
            switch (request.tag) {
                case 'execute_script': {
                    // biome-ignore lint/nursery/noImpliedEval: execute_script compiles caller source to run in the host
                    const body: unknown = new Function('alert', 'confirm', 'prompt', 'resolve', 'code', 'return eval(code)')(
                        () => undefined,
                        () => false,
                        () => null,
                        resolve,
                        request.payload.code,
                    );
                    if (typeof body !== 'function') {
                        throw illegal('code holds no body function');
                    }
                    return undone(request.label, () => ({ result: body() }));
                }
                case 'open':
                    return open(request.payload, scope);
                case 'render':
                    return undone(request.label, () => render(request.payload, scope));
                case 'object_model':
                    return model(base, request.payload.name);
                default:
                    return request satisfies never;
            }
        };
        try {
            const value = suppressed(answer);
            return value === undefined ? '{"_tag":"Value"}' : `{"_tag":"Value","value":${encoded(value)}}`;
        } catch (thrown) {
            if (isRecord(thrown) && (thrown._tag === 'Unmatched' || thrown._tag === 'IllegalArgument')) {
                return encoded(thrown);
            }
            return encoded(isRecord(thrown) ? { _tag: 'Thrown', message: String(thrown.message), number: thrown.number, line: thrown.line } : { _tag: 'Thrown', message: String(thrown) });
        }
    };

// --- [EXPORTS] -------------------------------------------------------------------------

export { handle, illegal, unmatched };
