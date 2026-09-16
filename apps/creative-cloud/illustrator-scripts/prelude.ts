/// <reference types="types-for-adobe/Illustrator/2022"/>

// --- [CONTRACT] ------------------------------------------------------------------------

declare global {
    interface PageItem {
        readonly uuid: string;
    }

    type Json = null | boolean | number | string | Json[] | JsonObject;

    interface JsonObject {
        [key: string]: Json;
    }

    interface Unavailable extends JsonObject {
        readonly path: string;
        readonly reason: string;
    }

    interface Reading<T> {
        readonly value: T;
        readonly unavailable: Unavailable[];
    }

    interface Site {
        readonly path: string;
        readonly chain: string;
    }

    type Reader = (at: Site) => Reading<Json>;

    interface Prelude {
        readonly all: (at: Site, readers: [string, Reader][]) => Reading<JsonObject>;
        readonly dump: (value: unknown, at: Site) => Reading<Json>;
        readonly each: <T>(at: Site, items: T[], reader: (item: T, at: Site) => Reading<Json>) => Reading<Json[]>;
        readonly members: <T extends object>(object: T) => [string, Reader][];
        readonly run: (tool: (request: JsonObject, at: Site) => Json) => string;
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

const classOf = (value: unknown): string => Object.prototype.toString.call(new Object(value));

const INHERITED = fold(Object.prototype.reflect.properties, '|', (names, info): string => `${names}${info.name}|`);

const properties = (object: object): ReflectionInfo[] => {
    try {
        return fold(classOf(object.reflect) === '[object Reflection]' ? object.reflect.properties : [], [] as ReflectionInfo[], (own, info): ReflectionInfo[] => {
            if (INHERITED.indexOf(`|${info.name}|`) < 0) {
                own.push(info);
            }
            return own;
        });
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

const quote = (text: string): string =>
    `"${collect(text.split(''), (character): string => {
        if (character === '"' || character === '\\') {
            return `\\${character}`;
        }
        return character < ' ' ? unicodeEscape(character.charCodeAt(0)) : character;
    }).join('')}"`;

const encode = (value: Json): string => {
    const kind = classOf(value);
    if (kind === '[object Array]') {
        return `[${collect(value as Json[], encode).join(',')}]`;
    }
    if (kind === '[object String]') {
        return quote(value as string);
    }
    if (kind === '[object Number]') {
        return (value as number) * 0 === 0 ? String(value) : 'null';
    }
    if (value !== null && kind === '[object Object]') {
        const object = value as JsonObject;
        return `{${collect(properties(object), (info): string => `${quote(info.name)}:${encode(object[info.name] as Json)}`).join(',')}}`;
    }
    return String(value);
};

const undecodable = (text: string, at: number, expected: string): Error => new Error(`expected ${expected} at ${at}, found ${at < text.length ? text.charAt(at) : 'end of text'}`);

const skipSpace = (text: string, at: number): number => {
    let cursor = at;
    while (cursor < text.length && ' \t\n\r'.indexOf(text.charAt(cursor)) >= 0) {
        cursor += 1;
    }
    return cursor;
};

const decodeEscape = (text: string, at: number): [string, number] => {
    const single = '"\\/bfnrt'.indexOf(text.charAt(at));
    if (single >= 0) {
        return ['"\\/\b\f\n\r\t'.charAt(single), at + 1];
    }
    const form = text.slice(at - 1, at - 1 + unicodeEscape(0).length);
    const code = Number(`0x${form.slice('\\u'.length)}`);
    if (unicodeEscape(code) === form.toLowerCase()) {
        return [String.fromCharCode(code), at - 1 + form.length];
    }
    throw undecodable(text, at, 'an escape');
};

const within = (character: string, first: string, last: string): boolean => character >= first && character <= last;

const digits = (text: string, at: number): number => {
    let cursor = at;
    while (cursor < text.length && within(text.charAt(cursor), '0', '9')) {
        cursor += 1;
    }
    return cursor;
};

const decodeNumber = (text: string, at: number): [number, number] => {
    const integer = text.charAt(at) === '-' ? at + 1 : at;
    const whole = text.charAt(integer) === '0' ? integer + 1 : digits(text, integer);
    if (whole === integer) {
        throw undecodable(text, integer, 'a digit');
    }
    const fraction = text.charAt(whole) === '.' ? digits(text, whole + 1) : whole;
    if (fraction === whole + 1) {
        throw undecodable(text, fraction, 'a digit');
    }
    const marker = text.charAt(fraction);
    const mark = marker === 'e' || marker === 'E' ? fraction + 1 : fraction;
    if (mark === fraction) {
        return [Number(text.slice(at, fraction)), fraction];
    }
    const sign = text.charAt(mark);
    const signed = sign === '+' || sign === '-' ? mark + 1 : mark;
    const end = digits(text, signed);
    if (end === signed) {
        throw undecodable(text, signed, 'a digit');
    }
    return [Number(text.slice(at, end)), end];
};

const decodeString = (text: string, at: number): [string, number] => {
    const parts: string[] = [];
    let cursor = at + 1;
    while (cursor < text.length && text.charAt(cursor) !== '"') {
        const character = text.charAt(cursor);
        const [piece, next]: [string, number] = character === '\\' ? decodeEscape(text, cursor + 1) : [character, cursor + 1];
        parts.push(piece);
        cursor = next;
    }
    if (cursor === text.length) {
        throw undecodable(text, cursor, '"');
    }
    return [parts.join(''), cursor + 1];
};

const sequence = (text: string, at: number, close: string, element: (start: number) => number): number => {
    let cursor = skipSpace(text, at);
    if (text.charAt(cursor) === close) {
        return cursor + 1;
    }
    while (cursor < text.length) {
        cursor = skipSpace(text, element(cursor));
        const separator = text.charAt(cursor);
        if (separator === close) {
            return cursor + 1;
        }
        if (separator !== ',') {
            throw undecodable(text, cursor, `, or ${close}`);
        }
        cursor += 1;
    }
    throw undecodable(text, cursor, close);
};

const decodeValue = (text: string, at: number): [Json, number] => {
    const cursor = skipSpace(text, at);
    const character = text.charAt(cursor);
    if (character === '{') {
        const object: JsonObject = {};
        const end = sequence(text, cursor + 1, '}', (start): number => {
            const opening = skipSpace(text, start);
            if (text.charAt(opening) !== '"') {
                throw undecodable(text, opening, 'a key');
            }
            const [key, afterKey] = decodeString(text, opening);
            const colon = skipSpace(text, afterKey);
            if (text.charAt(colon) !== ':') {
                throw undecodable(text, colon, ':');
            }
            const [member, next] = decodeValue(text, colon + 1);
            object[key] = member;
            return next;
        });
        return [object, end];
    }
    if (character === '[') {
        const list: Json[] = [];
        const end = sequence(text, cursor + 1, ']', (start): number => {
            const [item, next] = decodeValue(text, start);
            list.push(item);
            return next;
        });
        return [list, end];
    }
    if (character === '"') {
        return decodeString(text, cursor);
    }
    let stop = cursor;
    while (stop < text.length && within(text.charAt(stop), 'a', 'z')) {
        stop += 1;
    }
    if (stop === cursor) {
        return decodeNumber(text, cursor);
    }
    const word = text.slice(cursor, stop);
    const [literal, matched] = fold([true, false, null] as Json[], [null, false] as [Json, boolean], (found, candidate): [Json, boolean] => (encode(candidate) === word ? [candidate, true] : found));
    if (matched) {
        return [literal, stop];
    }
    throw undecodable(text, cursor, 'a value');
};

// --- [READINGS] ------------------------------------------------------------------------

const reference = (value: unknown): Json => {
    if (value === null) {
        return value;
    }
    const kind = classOf(value);
    if (kind === '[object Array]') {
        return collect(value as unknown[], reference);
    }
    if (kind === '[object Number]' || kind === '[object String]' || kind === '[object Boolean]') {
        return value as number | string | boolean;
    }
    if (kind === '[object File]' || kind === '[object Folder]') {
        return (value as File | Folder).fsName;
    }
    if (kind.indexOf('[object ') === 0 || kind.charAt(0) !== '[') {
        return value === undefined ? reference(null) : String(value);
    }
    const host = value as { readonly typename: string; readonly name: string };
    return kind === `[${host.typename}]` ? { typename: host.typename } : { typename: host.typename, name: host.name };
};

const gather = <T, R>(items: T[], site: (item: T, index: number) => Site, reader: (item: T, at: Site) => Reading<R>, put: (value: R, item: T) => void): Unavailable[] =>
    fold(items, [] as Unavailable[], (unavailable, item, index): Unavailable[] => {
        const at = site(item, index);
        try {
            const member = reader(item, at);
            put(member.value, item);
            return fold(member.unavailable, unavailable, (rows, row): Unavailable[] => {
                rows.push(row);
                return rows;
            });
        } catch (error) {
            unavailable.push({ path: at.path, reason: String(error) });
            return unavailable;
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

const members: Prelude['members'] = (object) =>
    collect(properties(object), (info): [string, Reader] => {
        const key = info.name as keyof typeof object;
        return [info.name, info.type === 'readwrite' ? (at): Reading<Json> => dump(object[key], at) : (): Reading<Json> => ({ value: reference(object[key]), unavailable: [] })];
    });

const dump: Prelude['dump'] = (value, at) => {
    const kind = classOf(value);
    if (kind === '[object Array]') {
        return each(at, value as unknown[], dump);
    }
    const object = new Object(value);
    const host = kind.indexOf('[object ') < 0;
    const own = host || kind === '[object Object]';
    const listed = object === value && own ? members(object) : [];
    const chained = listed.length > 0 && host && `${at.chain}/`.indexOf(`/${object.reflect.name}/`) >= 0;
    if (listed.length === 0 || chained) {
        return { value: reference(value), unavailable: [] };
    }
    return all({ path: at.path, chain: host ? `${at.chain}/${object.reflect.name}` : at.chain }, listed);
};

// --- [JOB] -----------------------------------------------------------------------------

const through = <T>(path: string, mode: 'r' | 'w', act: (file: File) => T): T => {
    const file = new File(path);
    file.encoding = 'UTF-8';
    if (!file.open(mode)) {
        throw new Error(`${path}: ${file.error}`);
    }
    try {
        return act(file);
    } finally {
        file.close();
    }
};

const run: Prelude['run'] = (tool) => {
    const [request, response]: [string, string] = $.global.arguments;
    const text = through(request, 'r', (file): string => file.read());
    const start = skipSpace(text, 0);
    if (text.charAt(start) !== '{') {
        throw undecodable(text, start, 'an object');
    }
    const [decoded, next] = decodeValue(text, start);
    const end = skipSpace(text, next);
    if (end !== text.length) {
        throw undecodable(text, end, 'end of text');
    }
    const level = app.userInteractionLevel;
    app.userInteractionLevel = UserInteractionLevel.DONTDISPLAYALERTS;
    try {
        const output = encode(tool(decoded as JsonObject, { path: '', chain: '' }));
        return String(
            through(response, 'w', (file): File => {
                file.write(output);
                return file;
            }).length,
        );
    } finally {
        app.userInteractionLevel = level;
    }
};

// --- [EXPORTS] -------------------------------------------------------------------------

((): Prelude => ({ all, dump, each, members, run }))();
