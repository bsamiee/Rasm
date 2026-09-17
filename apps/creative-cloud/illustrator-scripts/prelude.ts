/// <reference types="types-for-adobe/Illustrator/2022"/>

// --- [CONTRACT] ------------------------------------------------------------------------

declare global {
    interface PageItem {
        readonly uuid: string;
    }

    interface Error {
        readonly fileName: string;
        readonly line: number;
        readonly message: string;
        readonly name: string;
        readonly number: number;
    }

    type Json = null | boolean | number | string | Json[] | JsonObject;

    interface JsonObject {
        [key: string]: Json;
    }

    interface Reading<T> {
        readonly value: T;
        readonly unavailable: JsonObject[];
    }

    interface Site {
        readonly path: string;
        readonly chain: string[];
    }

    type Reader = (at: Site) => Reading<Json>;

    interface Prelude {
        readonly all: (at: Site, readers: [string, Reader][]) => Reading<JsonObject>;
        readonly dump: (value: unknown, at: Site) => Reading<Json>;
        readonly each: <T>(at: Site, items: T[], reader: (item: T, at: Site) => Reading<Json>) => Reading<Json[]>;
        readonly members: <T extends object>(object: T) => [string, Reader][];
        readonly reference: (value: unknown, at: Site) => Reading<Json>;
        readonly run: <R extends JsonObject>(tool: (request: R, at: Site) => Reading<JsonObject>) => string;
        readonly walk: <T extends object>(at: Site, object: T, extras: [string, Reader][]) => Reading<JsonObject>;
    }
}

// --- [SEQUENCES] -----------------------------------------------------------------------

const fold = <T, A>(items: T[], initial: A, step: (accumulator: A, item: T, index: number) => A): A => {
    let accumulator = initial;
    for (let index = 0; index < items.length; index += 1) {
        accumulator = step(accumulator, items[index] as T, index);
    }
    return accumulator;
};

const collect = <T, R>(items: T[], map: (item: T, index: number) => R): R[] =>
    fold(items, [] as R[], (list, item, index): R[] => {
        list.push(map(item, index));
        return list;
    });

const select = <T>(items: T[], keep: (item: T) => boolean): T[] =>
    fold(items, [] as T[], (kept, item): T[] => {
        if (keep(item)) {
            kept.push(item);
        }
        return kept;
    });

// --- [CLASSES] -------------------------------------------------------------------------

const classOf = (value: unknown): string => Object.prototype.toString.call(new Object(value));

const isArray = (value: unknown): value is unknown[] => classOf(value) === '[object Array]';

const isString = (value: unknown): value is string => typeof value === 'string';

const isNumber = (value: unknown): value is number => typeof value === 'number';

const isObject = (value: unknown): value is JsonObject => value !== null && classOf(value) === '[object Object]';

const INHERITED = Object.prototype.reflect;

const properties = (object: object): ReflectionInfo[] => {
    try {
        const reflection = object.reflect;
        return classOf(reflection) === '[object Reflection]' ? select(reflection.properties, (info): boolean => INHERITED.find(info.name) === null) : [];
    } catch {
        return [];
    }
};

// --- [JSON] ----------------------------------------------------------------------------

const HEXADECIMAL = 16;

const unicodeEscape = (code: number): string => {
    const hex = code.toString(HEXADECIMAL);
    return '\\u0000'.slice(0, -hex.length) + hex;
};

const encode = (value: Json): string => {
    if (value === null) {
        return 'null';
    }
    if (isArray(value)) {
        return `[${collect(value, encode).join(',')}]`;
    }
    if (isString(value)) {
        let escaped = value.split('\\').join('\\\\').split('"').join('\\"');
        for (let code = 0; code < ' '.charCodeAt(0); code += 1) {
            escaped = escaped.split(String.fromCharCode(code)).join(unicodeEscape(code));
        }
        return `"${escaped}"`;
    }
    if (isNumber(value)) {
        return value * 0 === 0 ? String(value) : 'null';
    }
    if (value === true || value === false) {
        return String(value);
    }
    return `{${collect(properties(value), (info): string => `${encode(info.name)}:${encode(value[info.name] as Json)}`).join(',')}}`;
};

const decode = (text: string): Json => {
    let cursor = 0;
    const digits = '0123456789';
    const literals: JsonObject = {};
    literals['true'] = true;
    literals['false'] = false;
    literals['null'] = null;
    const fail = (expected: string): Error => new Error(`Expected ${expected} at ${cursor}, found ${cursor < text.length ? text.charAt(cursor) : 'end of text'}`);
    const span = (characters: string): string => {
        const start = cursor;
        while (cursor < text.length && characters.indexOf(text.charAt(cursor)) >= 0) {
            cursor += 1;
        }
        return text.slice(start, cursor);
    };
    const take = (character: string): boolean => {
        span(' \t\n\r');
        const taken = text.charAt(cursor) === character;
        cursor += taken ? 1 : 0;
        return taken;
    };
    const escaped = (): string => {
        const single = '"\\/bfnrt'.indexOf(text.charAt(cursor));
        const form = text.slice(cursor, cursor + unicodeEscape(0).length - 1);
        const code = Number(`0x${form.slice(1)}`);
        if (single >= 0) {
            cursor += 1;
            return '"\\/\b\f\n\r\t'.charAt(single);
        }
        if (`\\${form.toLowerCase()}` === unicodeEscape(code)) {
            cursor += form.length;
            return String.fromCharCode(code);
        }
        throw fail('escape');
    };
    const quoted = (): string => {
        const parts: string[] = [];
        let quote = text.indexOf('"', cursor);
        let slash = text.indexOf('\\', cursor);
        while (slash >= 0 && slash < quote) {
            parts.push(text.slice(cursor, slash));
            cursor = slash + 1;
            parts.push(escaped());
            quote = text.indexOf('"', cursor);
            slash = text.indexOf('\\', cursor);
        }
        if (quote < 0) {
            throw fail('"');
        }
        parts.push(text.slice(cursor, quote));
        cursor = quote + 1;
        return parts.join('');
    };
    const number = (): number => {
        const start = cursor;
        cursor += text.charAt(cursor) === '-' ? 1 : 0;
        const whole = span(digits);
        const zeroLed = whole.length > 1 && whole.charAt(0) === '0';
        if (whole === '' || zeroLed) {
            throw fail('digit');
        }
        if (text.charAt(cursor) === '.') {
            cursor += 1;
            if (span(digits) === '') {
                throw fail('digit');
            }
        }
        const marker = text.charAt(cursor);
        if (marker === 'e' || marker === 'E') {
            cursor += 1;
            const sign = text.charAt(cursor);
            cursor += sign === '+' || sign === '-' ? 1 : 0;
            if (span(digits) === '') {
                throw fail('digit');
            }
        }
        return Number(text.slice(start, cursor));
    };
    const sequence = (close: string, element: () => void): void => {
        if (take(close)) {
            return;
        }
        element();
        while (!take(close)) {
            if (!take(',')) {
                throw fail(`${close} or ,`);
            }
            element();
        }
    };
    const value = (): Json => {
        if (take('{')) {
            const object: JsonObject = {};
            sequence('}', (): void => {
                if (!take('"')) {
                    throw fail('key');
                }
                const key = quoted();
                if (!take(':')) {
                    throw fail(':');
                }
                object[key] = value();
            });
            return object;
        }
        if (take('[')) {
            const list: Json[] = [];
            sequence(']', (): void => {
                list.push(value());
            });
            return list;
        }
        if (take('"')) {
            return quoted();
        }
        const leading = text.charAt(cursor);
        const digit = leading >= '0' && leading <= '9';
        if (leading === '-' || digit) {
            return number();
        }
        const literal = literals[span('aeflnrstu')];
        if (literal !== undefined) {
            return literal;
        }
        throw fail('value');
    };
    const decoded = value();
    span(' \t\n\r');
    if (cursor < text.length) {
        throw fail('end of text');
    }
    return decoded;
};

// --- [READINGS] ------------------------------------------------------------------------

const failure = (error: unknown): [JsonObject, { readonly file: string; readonly line: number }] => {
    const thrown = error as Error;
    const site = { file: File(thrown.fileName).name, line: thrown.line };
    const marker = 'an Illustrator error occurred: ';
    const opening = thrown.message.indexOf(marker);
    if (opening < 0) {
        return [{ name: thrown.name, message: thrown.message, number: thrown.number }, site];
    }
    const start = opening + marker.length;
    const code = Number(thrown.message.slice(start, thrown.message.indexOf(' ', start)));
    const octet = HEXADECIMAL * HEXADECIMAL;
    let tag = '';
    for (let rest = code; rest > 0; rest = Math.floor(rest / octet)) {
        tag = String.fromCharCode(rest % octet) + tag;
    }
    return [{ name: thrown.name, message: thrown.message, number: thrown.number, code, tag }, site];
};

const present = <T>(value: T): Reading<T> => ({ value, unavailable: [] });

const gather = <T, R>(items: T[], site: (item: T, index: number) => Site, reader: (item: T, at: Site) => Reading<R>, put: (value: R, item: T) => void): JsonObject[] =>
    fold(items, [] as JsonObject[], (unavailable, item, index): JsonObject[] => {
        const at = site(item, index);
        try {
            const member = reader(item, at);
            put(member.value, item);
            return unavailable.concat(member.unavailable);
        } catch (error) {
            const [row] = failure(error);
            row['path'] = at.path;
            return unavailable.concat([row]);
        }
    });

const each: Prelude['each'] = (at, items, reader) => {
    const value: Json[] = [];
    const unavailable = gather(
        items,
        (_item, index): Site => ({ path: `${at.path}[${index}]`, chain: at.chain }),
        reader,
        (member): void => {
            value.push(member);
        },
    );
    return { value, unavailable };
};

const all: Prelude['all'] = (at, readers) => {
    const value: JsonObject = {};
    const unavailable = gather(
        readers,
        ([key]): Site => ({ path: at.path === '' ? key : `${at.path}.${key}`, chain: at.chain }),
        ([, read], site): Reading<Json> => read(site),
        (member, [key]): void => {
            value[key] = member;
        },
    );
    return { value, unavailable };
};

const reference: Prelude['reference'] = (value, at) => {
    if (isArray(value)) {
        return each(at, value, reference);
    }
    if (value === undefined || value === null) {
        return present(null);
    }
    if (isString(value) || isNumber(value) || value === true || value === false) {
        return present(value);
    }
    if (isObject(value)) {
        return present({});
    }
    const kind = classOf(value);
    if (kind === '[object File]' || kind === '[object Folder]') {
        return present((value as File | Folder).fsName);
    }
    if (kind.indexOf('[object ') === 0 || kind.charAt(0) !== '[') {
        return present(String(value));
    }
    const host = value as { readonly typename: string; readonly name: string; readonly length: unknown };
    if (kind !== `[${host.typename}]`) {
        return present({ typename: host.typename, name: host.name });
    }
    return present(isNumber(host.length) ? { typename: host.typename, length: host.length } : { typename: host.typename });
};

const members: Prelude['members'] = (object) =>
    collect(
        select(properties(object), (info): boolean => info.name !== 'parent'),
        (info): [string, Reader] => {
            const key = info.name as keyof typeof object;
            const read = info.type === 'readwrite' ? dump : reference;
            return [info.name, (at): Reading<Json> => read(object[key], at)];
        },
    );

const walk: Prelude['walk'] = (at, object, extras) => all(at, members(object).concat(extras));

const dump: Prelude['dump'] = (value, at) => {
    if (isArray(value)) {
        return each(at, value, dump);
    }
    const kind = classOf(value);
    const host = kind.indexOf('[object ') < 0;
    const own = host || kind === '[object Object]';
    const object = new Object(value);
    const listed = object === value && own ? members(object) : [];
    if (listed.length === 0) {
        return reference(value, at);
    }
    if (!host) {
        return all(at, listed);
    }
    const { name } = object.reflect;
    const linked = fold<string, boolean>(at.chain, false, (found, link): boolean => found || link === name);
    return linked ? reference(value, at) : all({ path: at.path, chain: at.chain.concat([name]) }, listed);
};

// --- [JOB] -----------------------------------------------------------------------------

const withFile = <T>(path: string, mode: 'r' | 'w', act: (file: File) => T): T => {
    const file = new File(path);
    file.encoding = 'UTF-8';
    if (!file.open(mode)) {
        throw new Error(`File open failed at ${path}: ${file.error}`);
    }
    try {
        return act(file);
    } finally {
        file.close();
    }
};

const run = <R extends JsonObject>(tool: (request: R, at: Site) => Reading<JsonObject>): string => {
    const [request, response]: [string, string] = $.global.arguments;
    const level = app.userInteractionLevel;
    app.userInteractionLevel = UserInteractionLevel.DONTDISPLAYALERTS;
    let output: string;
    try {
        const decoded = decode(withFile(request, 'r', (file): string => file.read()));
        if (!isObject(decoded)) {
            throw new Error('Expected object at 0');
        }
        const reading = tool(decoded as R, { path: '', chain: [] });
        reading.value['unavailable'] = reading.unavailable;
        output = encode(reading.value);
    } catch (error) {
        const [row, site] = failure(error);
        row['kind'] = 'error';
        row['file'] = site.file;
        row['line'] = site.line;
        output = encode(row);
    } finally {
        app.userInteractionLevel = level;
    }
    return String(
        withFile(response, 'w', (file): File => {
            file.write(output);
            return file;
        }).length,
    );
};

// --- [EXPORTS] -------------------------------------------------------------------------

((): Prelude => ({ all, dump, each, members, reference, run, walk }))();
