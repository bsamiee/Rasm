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
    }

    type ProcessSpec = { readonly [M in keyof Channels]: { readonly model: M; readonly values: Channels[M] } }[keyof Channels];

    type ColorSpec = ProcessSpec | { readonly model: 'Spot'; readonly name: string; readonly tint: number; readonly ink: ProcessSpec };

    interface SwatchSpec {
        readonly name: string;
        readonly global: boolean;
        readonly color: ProcessSpec;
    }

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
        readonly collect: <T, R>(list: T[], map: (item: T, index: number) => R) => R[];
        readonly color: (doc: Document, spec: ColorSpec) => Color;
        readonly contains: <T>(list: T[], value: T) => boolean;
        readonly created: (colorSpace: 'RGB' | 'CMYK', width: number, height: number, raster: RasterSpec) => Document;
        readonly dump: (value: unknown, at: Site) => Reading<Json>;
        readonly each: <T>(at: Site, list: T[], reader: (item: T, at: Site) => Reading<Json>) => Reading<Json[]>;
        readonly flatMap: <T, R>(list: T[], map: (item: T, index: number) => R[]) => R[];
        readonly flatten: (list: PageItem[]) => PageItem[];
        readonly fold: <T, A>(list: T[], initial: A, step: (accumulator: A, item: T, index: number) => A) => A;
        readonly hosted: (value: unknown) => value is Hosted;
        readonly isArray: (value: unknown) => value is unknown[];
        readonly items: <T>(collection: { readonly length: number; readonly [index: number]: T }) => T[];
        readonly members: <T extends object>(object: T) => [string, Reader][];
        readonly named: <T>(collection: { readonly getByName: (name: string) => T }, name: string) => T[];
        readonly nth: <T>(collection: { readonly length: number; readonly [index: number]: T }, position: number) => T;
        readonly owned: <T extends { name: string }>(collection: { readonly getByName: (name: string) => T; readonly add: () => T }, name: string) => T;
        readonly paths: (item: PageItem) => PathItem[];
        readonly preference: { readonly [K in keyof PreferenceValue]: { readonly read: (key: string) => PreferenceValue[K]; readonly write: (key: string, value: PreferenceValue[K]) => void } };
        readonly present: <T>(value: T) => Reading<T>;
        readonly properties: (object: object) => ReflectionInfo[];
        readonly range: (count: number) => number[];
        readonly reference: (value: unknown, at: Site) => Reading<Json>;
        readonly reflection: (object: object) => Reflection[];
        readonly run: <R extends object>(tool: (request: R, at: Site) => Reading<JsonObject>) => string;
        readonly saved: (doc: Document, path: string) => File;
        readonly select: {
            <T, S extends T>(list: T[], keep: (item: T) => item is S): S[];
            <T>(list: T[], keep: (item: T) => boolean): T[];
        };
        readonly spec: (value: Color) => ColorSpec[];
        readonly split: (rows: JsonObject[]) => Applied;
        readonly swatches: (doc: Document, palette: SwatchPalette, replaceByName: boolean) => JsonObject[];
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

const reflection = (object: object): Reflection[] => {
    try {
        const reflected = object.reflect;
        return classOf(reflected) === '[object Reflection]' ? [reflected] : [];
    } catch {
        return [];
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

const failure = (error: unknown): [JsonObject, { readonly file: string; readonly line: number }] => {
    const thrown = isError(error) ? error : new Error(String(error));
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

const gather = <T, R>(list: T[], site: (item: T, index: number) => Site, reader: (item: T, at: Site) => Reading<R>, put: (value: R, item: T) => void): JsonObject[] =>
    fold<T, JsonObject[]>(list, [], (unavailable, item, index): JsonObject[] => {
        const at = site(item, index);
        try {
            const member = reader(item, at);
            put(member.value, item);
            return unavailable.concat(member.unavailable);
        } catch (error) {
            const [row, thrown] = failure(error);
            row['path'] = at.path;
            row['file'] = thrown.file;
            row['line'] = thrown.line;
            return unavailable.concat([row]);
        }
    });

const each: Prelude['each'] = (at, list, reader) => {
    const value: Json[] = [];
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

const items = <T>(collection: { readonly length: number; readonly [index: number]: T }): T[] => Array.prototype.slice.call(collection, 0);

const nth = <T>(collection: { readonly length: number; readonly [index: number]: T }, position: number): T => Array.prototype.slice.call(collection, position, position + 1)[0];

const named: Prelude['named'] = (collection, name) => {
    try {
        return [collection.getByName(name)];
    } catch {
        return [];
    }
};

const owned: Prelude['owned'] = (collection, name) => {
    const [existing] = named(collection, name);
    if (existing !== undefined) {
        return existing;
    }
    const added = collection.add();
    added.name = name;
    return added;
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
    const gray = new GrayColor();
    gray.gray = nth(values, 0);
    return gray;
};

const definedSpot = (doc: Document, name: string, ink: ProcessSpec): Spot => {
    const added = doc.spots.add();
    added.name = name;
    added.colorType = ColorModel.PROCESS;
    added.color = color(doc, ink);
    return added;
};

const color: Prelude['color'] = (doc, chosen) => {
    if (chosen.model === 'Spot') {
        const [existing] = named(doc.spots, chosen.name);
        const tinted = new SpotColor();
        tinted.spot = existing === undefined ? definedSpot(doc, chosen.name, chosen.ink) : existing;
        tinted.tint = chosen.tint;
        return tinted;
    }
    if (chosen.model === 'GRAY') {
        return built(chosen.model, chosen.values);
    }
    const space = doc.documentColorSpace === DocumentColorSpace.CMYK ? 'CMYK' : 'RGB';
    if (chosen.model === space) {
        return built(chosen.model, chosen.values);
    }
    return built(space, app.convertSampleColor(ImageColorSpace[chosen.model], chosen.values, ImageColorSpace[space], ColorConvertPurpose.defaultpurpose));
};

const spec: Prelude['spec'] = (value) => {
    if (typed<RGBColor>('RGBColor')(value)) {
        return [{ model: 'RGB', values: [value.red, value.green, value.blue] }];
    }
    if (typed<CMYKColor>('CMYKColor')(value)) {
        return [{ model: 'CMYK', values: [value.cyan, value.magenta, value.yellow, value.black] }];
    }
    if (typed<GrayColor>('GrayColor')(value)) {
        return [{ model: 'GRAY', values: [value.gray] }];
    }
    if (!typed<SpotColor>('SpotColor')(value)) {
        return [];
    }
    const { spot, tint } = value;
    return collect(
        select(spec(spot.color), (ink): ink is ProcessSpec => ink.model !== 'Spot'),
        (ink): ColorSpec => ({ model: 'Spot', name: spot.name, tint, ink }),
    );
};

const swatches: Prelude['swatches'] = (doc, palette, replaceByName) => {
    const rooted = collect(palette.root, (swatch): { readonly owner: SwatchGroup[]; readonly row: JsonObject; readonly swatch: SwatchSpec } => ({ owner: [], row: { name: swatch.name }, swatch }));
    const grouped = flatMap(palette.groups, (group): { readonly owner: SwatchGroup[]; readonly row: JsonObject; readonly swatch: SwatchSpec }[] => {
        const owner = [owned(doc.swatchGroups, group.name)];
        return collect(group.swatches, (swatch) => ({ owner, row: { group: group.name, name: swatch.name }, swatch }));
    });
    return collect(rooted.concat(grouped), ({ owner, row, swatch }): JsonObject => {
        const [existing] = named(doc.swatches, swatch.name);
        if (existing !== undefined && !replaceByName) {
            row['reason'] = 'nameCollision';
            return row;
        }
        if (existing !== undefined) {
            existing.color = color(doc, swatch.color);
            return row;
        }
        if (swatch.global) {
            const spot = definedSpot(doc, swatch.name, swatch.color);
            visit(owner, (holder): void => {
                holder.addSpot(spot);
            });
            return row;
        }
        const added = doc.swatches.add();
        added.name = swatch.name;
        added.color = color(doc, swatch.color);
        visit(owner, (holder): void => {
            holder.addSwatch(added);
        });
        return row;
    });
};

// --- [DOCUMENT] ------------------------------------------------------------------------

const preference: Prelude['preference'] = {
    bool: { read: (key): boolean => app.preferences.getBooleanPreference(key), write: (key, value): void => app.preferences.setBooleanPreference(key, value) },
    integer: { read: (key): number => app.preferences.getIntegerPreference(key), write: (key, value): void => app.preferences.setIntegerPreference(key, value) },
    real: { read: (key): number => app.preferences.getRealPreference(key), write: (key, value): void => app.preferences.setRealPreference(key, value) },
    string: { read: (key): string => app.preferences.getStringPreference(key), write: (key, value): void => app.preferences.setStringPreference(key, value) },
};

const created: Prelude['created'] = (colorSpace, width, height, raster) => {
    const doc = app.documents.add(DocumentColorSpace[colorSpace], width, height, 1);
    const settings = doc.rasterEffectSettings;
    settings.resolution = raster.resolution;
    settings.antiAliasing = raster.antiAliasing;
    settings.padding = raster.padding;
    doc.rasterEffectSettings = settings;
    return doc;
};

const saved: Prelude['saved'] = (doc, path) => {
    const options = new IllustratorSaveOptions();
    options.embedICCProfile = true;
    options.pdfCompatible = true;
    options.compatibility = Compatibility.ILLUSTRATOR24;
    const file = new File(path);
    doc.saveAs(file, options);
    doc.close(SaveOptions.DONOTSAVECHANGES);
    return file;
};

// --- [EXPORTS] -------------------------------------------------------------------------

((): Prelude => ({
    all,
    assign,
    collect,
    color,
    contains,
    created,
    dump,
    each,
    flatMap,
    flatten,
    fold,
    hosted,
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
    reference,
    reflection,
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
