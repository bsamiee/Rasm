/// <reference path="./prelude.ts"/>

// --- [HOST] ----------------------------------------------------------------------------

declare const $: $;
declare const app: Application;

declare global {
    enum DocumentColorSpace {}
    enum SaveOptions {}
    enum VariableKind {}
}

// --- [PRELUDE] -------------------------------------------------------------------------

const { all, each, fold, hosted, only, reference, run, typed }: Prelude = $.evalFile(new File(`${new File($.fileName).path}/prelude.jsx`));

const listed = (value: unknown): value is unknown[] => Object.prototype.toString.call(new Object(value)) === '[object Array]';

const reflected = (object: Hosted): [ReflectionInfo[], ReflectionInfo[]] => {
    try {
        const reflection = object.reflect;
        return Object.prototype.toString.call(reflection) === '[object Reflection]' ? [reflection.properties, reflection.methods] : [[], []];
    } catch {
        return [[], []];
    }
};

const read = (object: Hosted, name: string): unknown[] => {
    try {
        const value = object[name];
        return typeof value === 'undefined' ? [] : [value];
    } catch {
        return [];
    }
};

const spelled = (container: { readonly [key: string]: unknown }, member: string, expected: string): boolean => {
    try {
        return String(container[member]) === expected;
    } catch {
        return false;
    }
};

// --- [READERS] -------------------------------------------------------------------------

const record = (at: Site, object: Hosted, candidates: string[]): [Reading<Json>, unknown[]] => {
    const [properties, methods] = reflected(object);
    let found: unknown[] = typeof object.length === 'number' && object.length > 0 ? read(object, '0') : [];
    const reading = all(at, [
        [
            'properties',
            (site): Reading<Json> =>
                each(site, properties, (info, inner): Reading<Json> => {
                    found = found.concat(read(object, info.name));
                    return all(inner, [
                        ['name', (deeper): Reading<Json> => reference(info.name, deeper)],
                        ['type', (deeper): Reading<Json> => reference(info.type, deeper)],
                        ['dataType', (deeper): Reading<Json> => reference(info.dataType, deeper)],
                    ]);
                }),
        ],
        ['methods', (site): Reading<Json> => each(site, methods, (info, inner): Reading<Json> => reference(info.name, inner))],
        ['probed', (site): Reading<Json> => each(site, properties.length === 0 ? candidates : [], (name, inner): Reading<Json> => reference(read(object, name).length > 0, inner))],
    ]);
    return [reading, found];
};

const walk = (at: Site, seeds: unknown[], candidates: { readonly [name: string]: string[] }): Reading<JsonObject> => {
    const classes: JsonObject = {};
    let unavailable: JsonObject[] = [];
    let queue = seeds.slice(0);
    while (queue.length > 0) {
        const value = queue.shift();
        if (hosted(value) && classes[value.typename] === undefined) {
            const probes = candidates[value.typename];
            const [reading, found] = record({ path: `${at.path}.${value.typename}`, chain: at.chain }, value, probes === undefined ? [] : probes);
            classes[value.typename] = reading.value;
            unavailable = unavailable.concat(reading.unavailable);
            queue = queue.concat(found);
        } else if (!hosted(value) && listed(value)) {
            queue = queue.concat(value);
        }
    }
    return { value: classes, unavailable };
};

const confirm = (at: Site, name: string, members: string[]): Reading<Json> => {
    const enumeration: { readonly [key: string]: unknown } = $.global[name];
    if (String(enumeration) !== name) {
        throw new Error(`No enumeration ${name}`);
    }
    return each(at, members, (member, site): Reading<Json> => reference(spelled(enumeration, member, `${name}.${member}`), site));
};

// --- [ENTRY] ---------------------------------------------------------------------------

const SIDE = 100;

const reconcile = (
    request: {
        readonly image: string;
        readonly creatable: string[];
        readonly classes: { readonly [name: string]: string[] };
        readonly enumerations: { readonly name: string; readonly members: string[] }[];
    },
    at: Site,
): Reading<JsonObject> => {
    const doc = app.documents.add(DocumentColorSpace.RGB);
    const seeds: unknown[] = [app, doc];
    const keep = (item: { readonly typename: string }, site: Site): Reading<Json> => {
        seeds.push(item);
        return reference(item.typename, site);
    };
    try {
        const rect = doc.pathItems.rectangle(0, 0, SIDE, SIDE);
        return all(at, [
            ['kind', (site): Reading<Json> => reference('reconciliation', site)],
            [
                'created',
                (site): Reading<Json> =>
                    all(site, [
                        [
                            'textFrame',
                            (inner): Reading<Json> => {
                                const frame = doc.textFrames.add();
                                frame.contents = 'Rasm';
                                return reference(frame.typename, inner);
                            },
                        ],
                        [
                            'textPath',
                            (inner): Reading<Json> =>
                                fold<TextFrame, Reading<Json>>(
                                    only(only([doc.textFrames.pathText(doc.pathItems.ellipse(0, 0, SIDE, SIDE))], hosted), typed<TextFrame>('TextFrame')),
                                    reference(null, inner),
                                    (_absent, frame): Reading<Json> => keep(frame.textPath, inner),
                                ),
                        ],
                        ['compoundPathItem', (inner): Reading<Json> => reference(doc.compoundPathItems.add().typename, inner)],
                        ['groupItem', (inner): Reading<Json> => reference(doc.groupItems.add().typename, inner)],
                        ['symbolItem', (inner): Reading<Json> => reference(doc.symbolItems.add(doc.symbols.add(rect)).typename, inner)],
                        ['swatchGroup', (inner): Reading<Json> => reference(doc.swatchGroups.add().typename, inner)],
                        ['gradient', (inner): Reading<Json> => reference(doc.gradients.add().typename, inner)],
                        ['spot', (inner): Reading<Json> => reference(doc.spots.add().typename, inner)],
                        ['characterStyle', (inner): Reading<Json> => reference(doc.characterStyles.add('Rasm').typename, inner)],
                        ['paragraphStyle', (inner): Reading<Json> => reference(doc.paragraphStyles.add('Rasm').typename, inner)],
                        ['tag', (inner): Reading<Json> => reference(rect.tags.add().typename, inner)],
                        [
                            'dataSet',
                            (inner): Reading<Json> => {
                                const variable = doc.variables.add();
                                variable.kind = VariableKind.VISIBILITY;
                                rect.visibilityVariable = variable;
                                return reference(doc.dataSets.add().typename, inner);
                            },
                        ],
                        [
                            'placedItem',
                            (inner): Reading<Json> => {
                                const placed = doc.placedItems.add();
                                placed.file = new File(request.image);
                                return reference(placed.typename, inner);
                            },
                        ],
                        [
                            'rasterItem',
                            (inner): Reading<Json> => {
                                const placed = doc.placedItems.add();
                                placed.file = new File(request.image);
                                placed.embed();
                                return each(inner, doc.rasterItems, (item, deeper): Reading<Json> => reference(item.typename, deeper));
                            },
                        ],
                        ['tracing', (inner): Reading<Json> => each(inner, doc.rasterItems, (item, deeper): Reading<Json> => reference(item.trace().typename, deeper))],
                        ['constructed', (inner): Reading<Json> => each(inner, request.creatable, (name, deeper): Reading<Json> => keep(new $.global[name](), deeper))],
                    ]),
            ],
            ['classes', (site): Reading<Json> => walk(site, seeds, request.classes)],
            [
                'enumerations',
                (site): Reading<Json> => each(site, request.enumerations, (row, inner): Reading<Json> => all(inner, [[row.name, (deeper): Reading<Json> => confirm(deeper, row.name, row.members)]])),
            ],
            [
                'globals',
                (site): Reading<Json> =>
                    each(
                        site,
                        $.global.reflect.properties,
                        (info: ReflectionInfo, inner): Reading<Json> => all(inner, [[info.name, (deeper): Reading<Json> => reference(spelled($.global, info.name, info.name), deeper)]]),
                    ),
            ],
            ['inherited', (site): Reading<Json> => each(site, Object.prototype.reflect.properties, (info, inner): Reading<Json> => reference(info.name, inner))],
        ]);
    } finally {
        doc.close(SaveOptions.DONOTSAVECHANGES);
    }
};

run(reconcile);
