import { codec, type Host, messageOf, realApp, realGlobal } from '#/hosts/adobe/extendscript';

// --- [CONSTANTS] -----------------------------------------------------------------------

const id: Host = realGlobal.stringIDToTypeID;
const ENUMERATOR = /^([A-Za-z]\w*)\.([A-Z][A-Z0-9_]*)$/;
const SPECIFIER = /^\/document\[@id=(\d+)\](?:\/layer\[@id=(\d+)\])?$/;

// --- [OPERATIONS] ----------------------------------------------------------------------

// --- [ACTIONS]
const parsedJson = (text: string): Host => eval(`(${text})`);
const executed = (event: string, descriptor: Host): Host => realGlobal.executeAction(id(event), descriptor, DialogModes.NO);
const converted = (value: Host): Host => {
    const descriptor: Host = new ActionDescriptor();
    descriptor.putObject(id('object'), id('object'), value);
    return parsedJson(executed('convertJSONdescriptor', descriptor).getString(id('json')));
};
const action = (value: Host): Host => {
    const kind = value.typename;
    if (kind === 'ActionDescriptor') {
        return converted(value);
    }
    if (kind !== 'ActionList' && kind !== 'ActionReference') {
        return null;
    }
    const key = kind === 'ActionList' ? 'list' : 'null';
    const wrapper: Host = new ActionDescriptor();
    if (kind === 'ActionList') {
        wrapper.putList(id(key), value);
    } else {
        wrapper.putReference(id(key), value);
    }
    return converted(wrapper)[key];
};

// --- [OBJECTS]
const withId = (list: Host, wanted: number): Host => {
    for (const item of list) {
        if (item.id === wanted) {
            return item;
        }
    }
    return null;
};
const layersOf = (owner: Host): Host[] => {
    let layers: Host[] = [];
    for (const layer of owner.layers) {
        layers.push(layer);
        if (layer.typename === 'LayerSet') {
            layers = layers.concat(layersOf(layer));
        }
    }
    return layers;
};
const specifier = (value: Host): string | null => {
    try {
        if (value.typename === 'Document') {
            return `object:/document[@id=${value.id}]`;
        }
        if (value.typename !== 'ArtLayer' && value.typename !== 'LayerSet') {
            return null;
        }
        let owner = value.parent;
        while (owner.typename !== 'Document') {
            owner = owner.parent;
        }
        return `object:/document[@id=${owner.id}]/layer[@id=${value.id}]`;
    } catch (error) {
        return `unreadable:${messageOf(error)}`;
    }
};
const resolve = (path: string): Host => {
    const parts: Host = SPECIFIER.exec(path);
    if (!parts) {
        throw new Error(`object:${path} names no document or layer`);
    }
    const doc = withId(realApp.documents, Number(parts[1]));
    return doc === null || !parts[2] ? doc : withId(layersOf(doc), Number(parts[2]));
};

// --- [VALUES]
const enumerator = (value: Host): string | null => {
    const parts: Host = ENUMERATOR.exec(String(value));
    const owner = parts ? realGlobal[parts[1]] : null;
    return owner !== undefined && owner !== null && owner[parts[2]] == value ? `enum:${parts[1]}.${parts[2]}` : null;
};
const agrees = (read: Host, asked: Host): boolean => {
    if (typeof asked === 'number') {
        return Math.abs(Number(read) - asked) * 65_536 <= Math.max(1, Math.abs(asked));
    }
    const parts: Host = ENUMERATOR.exec(String(asked));
    return parts ? String(read).split('.').pop() === parts[2] : String(read) === String(asked);
};
const coded = codec({
    named: (value: Host) => enumerator(value) ?? specifier(value),
    plain: (value: Host) => (value instanceof UnitValue ? { value: value.value, type: value.type } : action(value)),
    located: resolve,
});

// --- [EXPORTS] -------------------------------------------------------------------------

export { action, agrees, coded, executed, id, layersOf, parsedJson, specifier, withId };
