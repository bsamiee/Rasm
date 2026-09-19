/// <reference types="types-for-adobe/Illustrator/2022"/>

// --- [CONTRACT] ------------------------------------------------------------------------

declare global {
    interface Color {
        readonly typename: string;
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

    interface Hosted {
        readonly typename: string;
        readonly name: string;
        readonly length: unknown;
        readonly reflect: Reflection;
        readonly [name: string]: unknown;
    }

    interface Channels {
        readonly RGB: [number, number, number];
        readonly CMYK: [number, number, number, number];
        readonly GRAY: [number];
        readonly LAB: [number, number, number];
    }

    type ProcessSpec = { readonly [M in keyof Channels]: { readonly model: M; readonly values: Channels[M] } }[keyof Channels];

    type ColorSpec = ProcessSpec | { readonly model: 'Spot'; readonly name: string; readonly colorType: 'PROCESS' | 'SPOT'; readonly tint: number; readonly ink: ProcessSpec };

    type SwatchSpec = { readonly name: string; readonly color: ProcessSpec } | { readonly color: ColorSpec & { readonly model: 'Spot' } };

    interface SwatchPalette {
        readonly root: SwatchSpec[];
        readonly groups: { readonly name: string; readonly swatches: SwatchSpec[] }[];
    }

    interface RasterSpec {
        readonly resolution: number;
        readonly antiAliasing: boolean;
        readonly padding: number;
    }

    interface PreferenceValue {
        readonly bool: boolean;
        readonly integer: number;
        readonly real: number;
        readonly string: string;
    }

    interface Applied extends JsonObject {
        readonly kind: 'applied';
        readonly applied: JsonObject[];
        readonly rejected: JsonObject[];
    }

    interface Prelude {
        readonly all: (at: Site, readers: [string, Reader][]) => Reading<JsonObject>;
        readonly assign: (target: object, name: string, value: unknown) => void;
        readonly channelScale: { readonly rgb: number; readonly percentage: number };
        readonly collect: <T, R>(list: T[], map: (item: T, index: number) => R) => R[];
        readonly colors: (
            doc: Document,
            specs: ColorSpec[],
            replaceByName: boolean,
            at: Site,
        ) => Reading<{ readonly values: Color[] } | { readonly rejected: { readonly name: string; readonly reason: 'colorDefinitionConflict' | 'unsupportedColor' }[] }>;
        readonly contains: <T>(list: T[], value: T) => boolean;
        readonly dump: (value: unknown, at: Site) => Reading<Json>;
        readonly each: <T, R extends Json>(at: Site, list: T[], reader: (item: T, at: Site) => Reading<R>) => Reading<R[]>;
        readonly failure: (error: unknown, at: Site) => JsonObject;
        readonly flatMap: <T, R>(list: T[], map: (item: T, index: number) => R[]) => R[];
        readonly flatten: (list: PageItem[]) => PageItem[];
        readonly fold: <T, A>(list: T[], initial: A, step: (accumulator: A, item: T, index: number) => A) => A;
        readonly hosted: (value: unknown) => value is Hosted;
        readonly isArray: (value: unknown) => value is unknown[];
        readonly ink: (spot: Spot) => ProcessSpec & { readonly model: 'RGB' | 'CMYK' | 'LAB' };
        readonly items: <T>(collection: { readonly length: number; readonly [index: number]: T }) => T[];
        readonly members: <T extends object>(object: T) => [string, Reader][];
        readonly named: <T>(collection: { readonly getByName: (name: string) => T }, name: string) => T[];
        readonly nth: <T>(collection: { readonly length: number; readonly [index: number]: T }, position: number) => T;
        readonly owned: <T extends { name: string; readonly remove: () => void }>(collection: { readonly getByName: (name: string) => T; readonly add: () => T }, name: string) => T;
        readonly paths: (item: PageItem) => PathItem[];
        readonly preference: { readonly [K in keyof PreferenceValue]: { readonly read: (key: string) => PreferenceValue[K]; readonly write: (key: string, value: PreferenceValue[K]) => void } };
        readonly present: <T>(value: T) => Reading<T>;
        readonly properties: (object: object) => ReflectionInfo[];
        readonly range: (count: number) => number[];
        readonly rasterOptions: (doc: Document, raster: RasterSpec) => RasterEffectOptions;
        readonly reference: (value: unknown, at: Site) => Reading<Json>;
        readonly reflection: (object: object) => Reflection[];
        readonly replacement: (
            doc: Document,
            name: string,
            value: Color | ColorSpec,
            replaceByName: boolean,
        ) => { readonly swatches: Swatch[] } | { readonly reason: 'nameCollision' | 'reservedColor' | 'resourceTypeConflict' | 'colorDefinitionConflict' };
        readonly replaceStops: (gradient: Gradient, stops: Pick<GradientStop, 'rampPoint' | 'midPoint' | 'opacity' | 'color'>[]) => number;
        readonly run: <R extends object>(tool: (request: R, at: Site) => Reading<JsonObject>) => string;
        readonly saved: (doc: Document, path: string) => File;
        readonly select: {
            <T, S extends T>(list: T[], keep: (item: T) => item is S): S[];
            <T>(list: T[], keep: (item: T) => boolean): T[];
        };
        readonly spec: (value: Color) => ColorSpec[];
        readonly split: (rows: JsonObject[]) => Applied;
        readonly swatches: (doc: Document, palette: SwatchPalette, replaceByName: boolean, at: Site) => Reading<JsonObject[]>;
        readonly typed: <S extends { readonly typename: string }>(typename: string) => (item: { readonly typename: string }) => item is S;
        readonly visit: <T>(list: T[], act: (item: T, index: number) => void) => void;
        readonly walk: <T extends object>(at: Site, object: T, extras: [string, Reader][]) => Reading<JsonObject>;
    }
}

// --- [SEQUENCES] -----------------------------------------------------------------------

const fold = <T, A>(list: T[], initial: A, step: (accumulator: A, item: T, index: number) => A): A => {
    let accumulator = initial;
    for (let index = 0; index < list.length; index += 1) {
        accumulator = step(accumulator, list[index] as T, index);
    }
    return accumulator;
};

const range = (count: number): number[] => {
    const list: number[] = [];
    for (let index = 0; index < count; index += 1) {
        list.push(index);
    }
    return list;
};

const collect = <T, R>(list: T[], map: (item: T, index: number) => R): R[] =>
    fold<T, R[]>(list, [], (mapped, item, index): R[] => {
        mapped.push(map(item, index));
        return mapped;
    });

const flatMap = <T, R>(list: T[], map: (item: T, index: number) => R[]): R[] => fold<T, R[]>(list, [], (flat, item, index): R[] => flat.concat(map(item, index)));

const select: Prelude['select'] = <T, S extends T>(list: T[], keep: (item: T) => boolean): S[] =>
    fold<T, S[]>(list, [], (kept, item): S[] => {
        if (keep(item)) {
            kept.push(item as S);
        }
        return kept;
    });

const visit = <T>(list: T[], act: (item: T, index: number) => void): void => fold<T, void>(list, undefined, (_visited, item, index): void => act(item, index));

const contains = <T>(list: T[], value: T): boolean => select(list, (item): boolean => item === value).length > 0;

// --- [CLASSES] -------------------------------------------------------------------------

const classOf = (value: unknown): string => Object.prototype.toString.call(new Object(value));

const isArray = (value: unknown): value is unknown[] => classOf(value) === '[object Array]';

const isString = (value: unknown): value is string => typeof value === 'string';

const isNumber = (value: unknown): value is number => typeof value === 'number';

const isObject = (value: unknown): value is JsonObject => value !== null && classOf(value) === '[object Object]';

const isError = (value: unknown): value is Error => classOf(value) === '[object Error]';

const isFileSystem = (value: unknown): value is File | Folder => {
    const kind = classOf(value);
    return kind === '[object File]' || kind === '[object Folder]';
};

const hosted = (value: unknown): value is Hosted => {
    const kind = classOf(value);
    return kind.charAt(0) === '[' && kind.indexOf('[object ') !== 0;
};

const typed =
    <S extends { readonly typename: string }>(typename: string) =>
    (item: { readonly typename: string }): item is S =>
        item.typename === typename;

const INHERITED = Object.prototype.reflect;
const NO_SUCH_ELEMENT = 1302;

const reflection = (object: object): Reflection[] => {
    try {
        const reflected = object.reflect;
        return classOf(reflected) === '[object Reflection]' ? [reflected] : [];
    } catch (error) {
        if (isError(error) && error.number === NO_SUCH_ELEMENT) {
            return [];
        }
        throw error;
    }
};

const properties = (object: object): ReflectionInfo[] =>
    fold<Reflection, ReflectionInfo[]>(reflection(object), [], (_none, reflected): ReflectionInfo[] => select(reflected.properties, (info): boolean => INHERITED.find(info.name) === null));

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
    const entries = select(
        collect(properties(value), (info): [string, Json | undefined] => [info.name, value[info.name]]),
        (entry): entry is [string, Json] => entry[1] !== undefined,
    );
    return `{${collect(entries, ([name, member]): string => `${encode(name)}:${encode(member)}`).join(',')}}`;
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
        const dotted = text.charAt(cursor) === '.';
        cursor += dotted ? 1 : 0;
        if (dotted && span(digits) === '') {
            throw fail('digit');
        }
        const marker = text.charAt(cursor);
        const exponent = marker === 'e' || marker === 'E';
        cursor += exponent ? 1 : 0;
        const sign = text.charAt(cursor);
        const signed = sign === '+' || sign === '-';
        cursor += exponent && signed ? 1 : 0;
        if (exponent && span(digits) === '') {
            throw fail('digit');
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
    const pair = (): [string, Json] => {
        if (!take('"')) {
            throw fail('key');
        }
        const key = quoted();
        if (!take(':')) {
            throw fail(':');
        }
        return [key, value()];
    };
    const value = (): Json => {
        if (take('{')) {
            const object: JsonObject = {};
            sequence('}', (): void => {
                const [key, member] = pair();
                object[key] = member;
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

const failure: Prelude['failure'] = (error, at) => {
    const thrown = isError(error) ? error : new Error(String(error));
    return { name: thrown.name, message: thrown.message, number: thrown.number, path: at.path, file: File(thrown.fileName).name, line: thrown.line };
};

const present = <T>(value: T): Reading<T> => ({ value, unavailable: [] });

const gather = <T, R>(list: T[], site: (item: T, index: number) => Site, reader: (item: T, at: Site) => Reading<R>, put: (value: R, item: T) => void): JsonObject[] =>
    fold<T, JsonObject[]>(list, [], (unavailable, item, index): JsonObject[] => {
        const at = site(item, index);
        try {
            const member = reader(item, at);
            put(member.value, item);
            return unavailable.concat(member.unavailable);
        } catch (error) {
            return unavailable.concat([failure(error, at)]);
        }
    });

const each = <T, R extends Json>(at: Site, list: T[], reader: (item: T, at: Site) => Reading<R>): Reading<R[]> => {
    const value: R[] = [];
    const unavailable = gather(
        list,
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
    if (isFileSystem(value)) {
        return present(value.fsName);
    }
    if (!hosted(value)) {
        return present(String(value));
    }
    if (classOf(value) !== `[${value.typename}]`) {
        return present({ typename: value.typename, name: value.name });
    }
    return present(isNumber(value.length) ? { typename: value.typename, length: value.length } : { typename: value.typename });
};

const members: Prelude['members'] = (object) => {
    const readers = collect(
        select(properties(object), (info): boolean => info.name !== 'parent'),
        (info): [string, Reader] => {
            const key = info.name as keyof typeof object;
            const read = info.type === 'readwrite' ? dump : reference;
            return [info.name, (at): Reading<Json> => read(object[key], at)];
        },
    );
    if (hosted(object) && typed<Spot>('Spot')(object)) {
        readers.push(['ink', (at): Reading<Json> => (object.colorType === ColorModel.REGISTRATION ? present(null) : dump(ink(object), at))]);
    }
    return readers;
};

const walk: Prelude['walk'] = (at, object, extras) => all(at, members(object).concat(extras));

const assign: Prelude['assign'] = (target, name, value) => {
    (target as { [key: string]: unknown })[name] = value;
};

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
    return contains(at.chain, name) ? reference(value, at) : all({ path: at.path, chain: at.chain.concat([name]) }, listed);
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

const run = <R extends object>(tool: (request: R, at: Site) => Reading<JsonObject>): string => {
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
        const row = failure(error, { path: '', chain: [] });
        row['kind'] = 'error';
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

const items = <T>(collection: { readonly length: number; readonly [index: number]: T }): T[] => Array.prototype.slice.call(collection, 0);

const nth = <T>(collection: { readonly length: number; readonly [index: number]: T }, position: number): T => Array.prototype.slice.call(collection, position, position + 1)[0];

const named: Prelude['named'] = (collection, name) => {
    try {
        return [collection.getByName(name)];
    } catch (error) {
        if (isError(error) && error.number === NO_SUCH_ELEMENT) {
            return [];
        }
        throw error;
    }
};

const owned: Prelude['owned'] = (collection, name) => {
    const [existing] = named(collection, name);
    if (existing !== undefined) {
        return existing;
    }
    const added = collection.add();
    try {
        added.name = name;
        return added;
    } catch (error) {
        added.remove();
        throw error;
    }
};

const flatten: Prelude['flatten'] = (list) =>
    fold<PageItem, PageItem[]>(list, [], (flat, item): PageItem[] => (typed<GroupItem>('GroupItem')(item) ? flat.concat(flatten(items(item.pageItems))) : flat.concat([item])));

const paths: Prelude['paths'] = (item) => {
    if (typed<PathItem>('PathItem')(item)) {
        return [item];
    }
    return typed<CompoundPathItem>('CompoundPathItem')(item) ? items(item.pathItems) : [];
};

const split: Prelude['split'] = (rows) => ({
    kind: 'applied',
    applied: select(rows, (row): boolean => row['reason'] === undefined),
    rejected: select(rows, (row): boolean => row['reason'] !== undefined),
});

// --- [COLOR] ---------------------------------------------------------------------------

const built = (model: keyof Channels, values: number[]): Color => {
    if (model === 'RGB') {
        const rgb = new RGBColor();
        rgb.red = nth(values, 0);
        rgb.green = nth(values, 1);
        rgb.blue = nth(values, 2);
        return rgb;
    }
    if (model === 'CMYK') {
        const cmyk = new CMYKColor();
        cmyk.cyan = nth(values, 0);
        cmyk.magenta = nth(values, 1);
        cmyk.yellow = nth(values, 2);
        cmyk.black = nth(values, values.length - 1);
        return cmyk;
    }
    if (model === 'LAB') {
        const lab = new LabColor();
        lab.l = nth(values, 0);
        lab.a = nth(values, 1);
        lab.b = nth(values, 2);
        return lab;
    }
    const gray = new GrayColor();
    gray.gray = (1 - nth(values, 0)) * channelScale.percentage;
    return gray;
};

const BINARY32 = { fraction: 23, exponent: 8 } as const;
const channelScale = { rgb: 255, percentage: 100 } as const;

const storedChannel = (value: number): number => {
    const magnitude = Math.abs(value);
    if (!magnitude) {
        return value;
    }
    const limit = 2 ** (BINARY32.exponent - 1);
    const minimum = 2 - limit - BINARY32.fraction;
    let lower = minimum;
    let upper = limit;
    while (lower + 1 < upper) {
        const middle = Math.floor((lower + upper) / 2);
        if (magnitude < 2 ** middle) {
            upper = middle;
        } else {
            lower = middle;
        }
    }
    const unit = 2 ** Math.max(minimum, lower - BINARY32.fraction);
    const significand = magnitude / unit;
    const integer = Math.floor(significand);
    const fraction = significand - integer;
    const halfway = fraction === 1 / 2 && integer % 2 !== 0;
    const stored = (integer + Number(fraction > 1 / 2 || halfway)) * unit;
    const finite = stored < 2 ** limit ? stored : Number.POSITIVE_INFINITY;
    return value < 0 ? -finite : finite;
};

const sameInk = (left: ProcessSpec, right: ProcessSpec): boolean => {
    const percentage = left.model === 'LAB' || left.model === 'GRAY' ? 1 : channelScale.percentage;
    const scale = left.model === 'RGB' ? channelScale.rgb : percentage;
    return (
        left.model === right.model &&
        left.values.length === right.values.length &&
        fold(left.values, true, (same, value, index): boolean => same && storedChannel(value / scale) === storedChannel(nth(right.values, index) / scale))
    );
};

const setInk = (doc: Document, name: string, value: ProcessSpec & { readonly model: 'RGB' | 'CMYK' | 'LAB' }, colorType: ColorModel): void => {
    const spot = doc.spots.getByName(name);
    if (value.model === 'LAB') {
        spot.colorType = ColorModel.SPOT;
    }
    spot.color = built(value.model, value.values);
    spot.colorType = colorType;
};

const colors: Prelude['colors'] = (doc, specs, replaceByName, at) => {
    const space = doc.documentColorSpace === DocumentColorSpace.CMYK ? 'CMYK' : 'RGB';
    const definitions = flatMap(specs, (chosen): (ColorSpec & { readonly model: 'Spot' })[] => (chosen.model === 'Spot' ? [chosen] : []));
    const planned = collect(definitions, (chosen) => {
        let requested: ProcessSpec & { readonly model: 'RGB' | 'CMYK' | 'LAB' };
        if (chosen.ink.model === 'GRAY') {
            const converted = app.convertSampleColor(ImageColorSpace.GrayScale, [(1 - chosen.ink.values[0]) * channelScale.percentage], ImageColorSpace[space], ColorConvertPurpose.defaultpurpose);
            requested =
                space === 'RGB'
                    ? { model: space, values: [nth(converted, 0), nth(converted, 1), nth(converted, 2)] }
                    : { model: space, values: [nth(converted, 0), nth(converted, 1), nth(converted, 2), nth(converted, converted.length - 1)] };
        } else {
            requested = chosen.ink;
        }
        return { chosen, requested, held: collect(named(doc.spots, chosen.name), (spot) => ({ spot, colorType: spot.colorType, ink: ink(spot) })) };
    });
    const rejected = flatMap(planned, (plan): { readonly name: string; readonly reason: 'colorDefinitionConflict' | 'unsupportedColor' }[] => {
        if (plan.chosen.colorType === 'PROCESS' && plan.requested.model === 'LAB') {
            return [{ name: plan.chosen.name, reason: 'unsupportedColor' }];
        }
        const duplicates = select(planned, (other): boolean =>
            other.chosen.name === plan.chosen.name ? other.chosen.colorType !== plan.chosen.colorType || !sameInk(other.requested, plan.requested) : false,
        );
        const occupied = select(plan.held, (before): boolean => before.colorType !== ColorModel[plan.chosen.colorType] || !sameInk(before.ink, plan.requested));
        const retained = !replaceByName && occupied.length > 0;
        return duplicates.length > 0 || retained ? [{ name: plan.chosen.name, reason: 'colorDefinitionConflict' }] : [];
    });
    if (rejected.length > 0) {
        return present({ rejected });
    }
    const unique = fold(planned, [] as typeof planned, (found, plan): typeof planned =>
        select(found, (before): boolean => before.chosen.name === plan.chosen.name).length === 0 ? found.concat([plan]) : found,
    );
    const created: Spot[] = [];
    try {
        visit(unique, ({ chosen, requested, held }): void => {
            if (!replaceByName && held.length > 0) {
                return;
            }
            const [before] = held;
            const spot = before === undefined ? doc.spots.add() : before.spot;
            if (before === undefined) {
                created.push(spot);
                spot.name = chosen.name;
            }
            setInk(doc, chosen.name, requested, ColorModel[chosen.colorType]);
        });
        return present({
            values: collect(specs, (chosen): Color => {
                if (chosen.model === 'Spot') {
                    const value = new SpotColor();
                    value.spot = doc.spots.getByName(chosen.name);
                    value.tint = chosen.tint;
                    return value;
                }
                const unconverted = chosen.model === 'GRAY' || chosen.model === space;
                return unconverted
                    ? built(chosen.model, chosen.values)
                    : built(space, app.convertSampleColor(ImageColorSpace[chosen.model], chosen.values, ImageColorSpace[space], ColorConvertPurpose.defaultpurpose));
            }),
        });
    } catch (error) {
        const removed = each(at, created.reverse(), (spot): Reading<null> => {
            spot.remove();
            return present(null);
        });
        const restored = each(
            at,
            flatMap(unique, ({ held }) => held),
            (before): Reading<null> => {
                setInk(doc, before.spot.name, before.ink, before.colorType);
                return present(null);
            },
        );
        return { value: { rejected: [] }, unavailable: [failure(error, at)].concat(removed.unavailable, restored.unavailable) };
    }
};

const ink: Prelude['ink'] = (spot) => {
    const values = spot.getInternalColor();
    switch (spot.spotKind) {
        case SpotColorKind.SPOTRGB:
            return { model: 'RGB', values: [nth(values, 0), nth(values, 1), nth(values, 2)] };
        case SpotColorKind.SPOTCMYK:
            return { model: 'CMYK', values: [nth(values, 0), nth(values, 1), nth(values, 2), nth(values, values.length - 1)] };
        case SpotColorKind.SPOTLAB:
            return { model: 'LAB', values: [nth(values, 0), nth(values, 1), nth(values, 2)] };
    }
};

const spec: Prelude['spec'] = (value) => {
    if (typed<RGBColor>('RGBColor')(value)) {
        return [{ model: 'RGB', values: [value.red, value.green, value.blue] }];
    }
    if (typed<CMYKColor>('CMYKColor')(value)) {
        return [{ model: 'CMYK', values: [value.cyan, value.magenta, value.yellow, value.black] }];
    }
    if (typed<GrayColor>('GrayColor')(value)) {
        return [{ model: 'GRAY', values: [1 - value.gray / channelScale.percentage] }];
    }
    if (typed<LabColor>('LabColor')(value)) {
        return [{ model: 'LAB', values: [value.l, value.a, value.b] }];
    }
    if (!typed<SpotColor>('SpotColor')(value) || value.spot.colorType === ColorModel.REGISTRATION) {
        return [];
    }
    const { spot, tint } = value;
    return [{ model: 'Spot', name: spot.name, colorType: spot.colorType === ColorModel.PROCESS ? 'PROCESS' : 'SPOT', tint, ink: ink(spot) }];
};

const replacement: Prelude['replacement'] = (doc, name, value, replaceByName) => {
    const existing = named(doc.swatches, name);
    const kinds = collect<Color | ColorSpec, string>([value].concat(collect(existing, ({ color }): Color => color)), (member): string => {
        if ('model' in member) {
            return member.model === 'Spot' ? 'SpotColor' : 'process';
        }
        const registration = typed<SpotColor>('SpotColor')(member) && member.spot.colorType === ColorModel.REGISTRATION;
        if (member.typename === 'NoColor' || registration) {
            return 'reserved';
        }
        return contains(['RGBColor', 'CMYKColor', 'GrayColor', 'LabColor'], member.typename) ? 'process' : member.typename;
    });
    const [incoming, previous] = kinds;
    if (contains(kinds, 'reserved')) {
        return { reason: 'reservedColor' };
    }
    const retained = !replaceByName || incoming === 'PatternColor';
    if (previous !== undefined && retained) {
        return { reason: 'nameCollision' };
    }
    if (previous !== undefined && previous !== incoming) {
        return { reason: 'resourceTypeConflict' };
    }
    const stops = !('model' in value) && typed<GradientColor>('GradientColor')(value) ? items(value.gradient.gradientStops) : [];
    const dependencies = flatMap(
        select(
            collect(stops, ({ color }): Color => color),
            typed<SpotColor>('SpotColor'),
        ),
        ({ spot }) => collect(named(doc.spots, spot.name), (target) => ({ source: spot, target })),
    );
    const conflicting = select(dependencies, ({ source, target }): boolean =>
        source.colorType === target.colorType ? source.colorType !== ColorModel.REGISTRATION && !sameInk(ink(source), ink(target)) : true,
    );
    return conflicting.length > 0 ? { reason: 'colorDefinitionConflict' } : { swatches: existing };
};

const swatches: Prelude['swatches'] = (doc, palette, replaceByName, at) => {
    const rooted = collect(palette.root, (swatch): { readonly owner: string[]; readonly swatch: SwatchSpec } => ({ owner: [], swatch }));
    const grouped = flatMap(palette.groups, (group) => collect(group.swatches, (swatch) => ({ owner: [group.name], swatch })));
    const reading = each(at, rooted.concat(grouped), ({ owner, swatch }, site): Reading<JsonObject[]> => {
        const name = 'name' in swatch ? swatch.name : swatch.color.name;
        const [group] = owner;
        const row: JsonObject = group === undefined ? { name } : { name, group };
        const checked = replacement(doc, name, swatch.color, replaceByName);
        if ('reason' in checked) {
            row['reason'] = checked.reason;
            return present([row]);
        }
        const [existing] = checked.swatches;
        const resolved = colors(doc, [swatch.color], replaceByName, site);
        if ('rejected' in resolved.value) {
            return { value: collect(resolved.value.rejected, ({ reason }): JsonObject => (group === undefined ? { name, reason } : { name, group, reason })), unavailable: resolved.unavailable };
        }
        const added = existing === undefined ? owned(doc.swatches, name) : existing;
        added.color = nth(resolved.value.values, 0);
        visit(owner, (holder): void => {
            owned(doc.swatchGroups, holder).addSwatch(added);
        });
        row['name'] = added.name;
        return present([row]);
    });
    return { value: Array.prototype.concat.apply([], reading.value), unavailable: reading.unavailable };
};

const replaceStops: Prelude['replaceStops'] = (gradient, wanted) => {
    const stops = gradient.gradientStops;
    const added = Math.max(0, wanted.length - stops.length);
    visit(range(added), (): void => {
        stops.add();
    });
    visit(items(stops).slice(wanted.length).reverse(), (stop): void => {
        stop.remove();
    });
    const indices = range(stops.length);
    visit(indices, (index): void => {
        const stop = nth(stops, index);
        stop.rampPoint = Math.min(stop.rampPoint, nth(wanted, index).rampPoint);
    });
    visit(indices.reverse(), (index): void => {
        const stop = nth(stops, index);
        const chosen = nth(wanted, index);
        stop.rampPoint = chosen.rampPoint;
        stop.midPoint = chosen.midPoint;
        stop.opacity = chosen.opacity;
        stop.color = chosen.color;
    });
    return added;
};

// --- [DOCUMENT] ------------------------------------------------------------------------

const preference: Prelude['preference'] = {
    bool: { read: (key): boolean => app.preferences.getBooleanPreference(key), write: (key, value): void => app.preferences.setBooleanPreference(key, value) },
    integer: { read: (key): number => app.preferences.getIntegerPreference(key), write: (key, value): void => app.preferences.setIntegerPreference(key, value) },
    real: { read: (key): number => app.preferences.getRealPreference(key), write: (key, value): void => app.preferences.setRealPreference(key, value) },
    string: { read: (key): string => app.preferences.getStringPreference(key), write: (key, value): void => app.preferences.setStringPreference(key, value) },
};

const rasterOptions: Prelude['rasterOptions'] = (doc, raster) => {
    const settings = doc.rasterEffectSettings;
    settings.resolution = raster.resolution;
    settings.antiAliasing = raster.antiAliasing;
    settings.padding = raster.padding;
    return settings;
};

const saved: Prelude['saved'] = (doc, path) => {
    const options = new IllustratorSaveOptions();
    options.embedICCProfile = true;
    options.pdfCompatible = true;
    const file = new File(path);
    doc.saveAs(file, options);
    return file;
};

// --- [EXPORTS] -------------------------------------------------------------------------

((): Prelude => ({
    all,
    assign,
    channelScale,
    collect,
    colors,
    contains,
    dump,
    each,
    failure,
    flatMap,
    flatten,
    fold,
    hosted,
    ink,
    isArray,
    items,
    members,
    named,
    nth,
    owned,
    paths,
    preference,
    present,
    properties,
    range,
    rasterOptions,
    reference,
    reflection,
    replacement,
    replaceStops,
    run,
    saved,
    select,
    spec,
    split,
    swatches,
    typed,
    visit,
    walk,
}))();
