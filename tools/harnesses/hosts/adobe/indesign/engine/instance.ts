import { ownKeys } from '@rasm/extendscript/extendscript';
import { type Host, holds, messageOf, realApp } from '#/hosts/adobe/extendscript';
import { coded } from '#/hosts/adobe/indesign/engine/codec';

// --- [CONSTANTS] -----------------------------------------------------------------------

const STRUCTURAL: readonly string[] = ['__proto__', 'parent', 'properties', 'reflect'];
const POINTS: Host = { measurementUnit: MeasurementUnits.POINTS };
const DOCUMENT_SPECIFIER = '{"$":"object:/document';

// --- [OPERATIONS] ----------------------------------------------------------------------

// --- [MEMBERS]
const readable = (object: Host): string[] => {
    const names: string[] = [];
    for (const { name } of object.reflect.properties) {
        if (!holds(STRUCTURAL, name)) {
            names.push(name);
        }
    }
    return names;
};
const identified = (value: Host): Host => value.reflect.find.call(value.reflect, 'id');
const surveyed = (object: Host): Host => {
    const found: Host = { preferences: {}, collections: [], unread: [] };
    for (const name of readable(object)) {
        let value: Host;
        try {
            value = object[name];
        } catch (error) {
            found.unread.push(`${name}: ${messageOf(error)}`);
            continue;
        }
        if (value === null || typeof value !== 'object' || value instanceof Array || value instanceof File || !value.reflect) {
            continue;
        }
        if (typeof value.everyItem === 'function') {
            found.collections.push({ name, list: value });
            continue;
        }
        if (identified(value)) {
            continue;
        }
        try {
            if (value.properties !== null && typeof value.properties === 'object') {
                found.preferences[name] = value.properties;
            }
        } catch (error) {
            found.unread.push(`${name}: ${messageOf(error)}`);
        }
    }
    return found;
};
const valuesOf = (list: Host, key: string): Host => list.everyItem()[key];
const keyOf = (list: Host): string | null => {
    for (const key of ['id', 'name']) {
        try {
            valuesOf(list, key);
            return key;
        } catch {}
    }
    return null;
};
const closes = (list: Host): boolean => {
    let methods: Host;
    let removable = false;
    try {
        ({ methods } = list.everyItem().reflect);
    } catch {
        return true;
    }
    for (const { name } of methods) {
        if (name === 'close') {
            return true;
        }
        removable = removable || name === 'remove';
    }
    return !removable;
};
const members = (): Host => {
    const found = surveyed(realApp);
    const collections: Host[] = [];
    for (const { name, list } of found.collections) {
        const key = closes(list) ? undefined : keyOf(list);
        if (key === null) {
            found.unread.push(`${name}: items hold neither id nor name`);
        } else if (key !== undefined) {
            collections.push({ name, key });
        }
    }
    found.preferences.scriptPreferences = realApp.scriptPreferences.properties;
    return { preferences: found.preferences, collections, unread: found.unread };
};
const header = (found: Host): Host => {
    const settings = realApp.colorSettings;
    return { application: realApp.name, version: realApp.version, state: { modal: realApp.modalState, cmyk: settings.workingSpaceCMYK, rgb: settings.workingSpaceRGB, defaults: found.preferences, unread: found.unread } };
};

// --- [SNAPSHOT]
const pinned = (values: Host, work: () => Host): Host => {
    const preferences = realApp.scriptPreferences;
    const previous: Host = {};
    for (const key of ownKeys(values)) {
        previous[key] = preferences[key];
        preferences[key] = values[key];
    }
    try {
        return work();
    } finally {
        for (const key of ownKeys(previous)) {
            preferences[key] = previous[key];
        }
    }
};
const snapshot = (found: Host): Host =>
    pinned(POINTS, (): Host => {
        const held: Host = { preferences: [], collections: [] };
        for (const name of ownKeys(found.preferences)) {
            const target = realApp[name];
            const saved = target.properties;
            const encoded: Host = {};
            for (const key of ownKeys(saved)) {
                const text = key === 'measurementUnit' ? '' : coded.json(saved[key]);
                if (text !== '' && text.indexOf(DOCUMENT_SPECIFIER) !== 0) {
                    encoded[key] = text;
                }
            }
            held.preferences.push({ name, target, saved, encoded });
        }
        for (const row of found.collections) {
            const list = realApp[row.name];
            const known: Host = {};
            for (const present of valuesOf(list, row.key)) {
                known[present] = true;
            }
            held.collections.push({ name: row.name, key: row.key, list, known });
        }
        return held;
    });
const differences = (held: Host): Host[] => {
    const rows: Host[] = [];
    for (const row of held.preferences) {
        const current = row.target.properties;
        for (const key of ownKeys(row.encoded)) {
            const now = coded.json(current[key]);
            if (now.indexOf(DOCUMENT_SPECIFIER) !== 0 && now !== row.encoded[key]) {
                rows.push({ member: `${row.name}.${key}`, before: row.saved[key], after: current[key], target: row.target, key });
            }
        }
    }
    for (const tracked of held.collections) {
        const seen: Host = {};
        for (const present of valuesOf(tracked.list, tracked.key)) {
            seen[present] = true;
            if (!tracked.known[present]) {
                const item = tracked.key === 'id' ? tracked.list.itemByID(present) : tracked.list.itemByName(present);
                rows.push({ member: tracked.name, after: item, item });
            }
        }
        for (const gone of ownKeys(tracked.known)) {
            if (!seen[gone]) {
                rows.push({ member: `${tracked.name}.${tracked.key}=${gone}`, before: gone, after: null });
            }
        }
    }
    return rows;
};
const writes = (held: Host): Host[] =>
    pinned(POINTS, (): Host[] => {
        const out: Host[] = [];
        for (const row of differences(held)) {
            out.push(row.before === undefined ? { member: row.member, after: coded.kept(row.after) } : { member: row.member, before: coded.kept(row.before), after: coded.kept(row.after) });
        }
        return out;
    });
const restore = (held: Host): string[] =>
    pinned(POINTS, (): string[] => {
        const leaked: string[] = [];
        for (const row of differences(held)) {
            try {
                if (row.target === realApp.scriptPreferences) {
                    row.target[row.key] = row.before;
                } else if (row.item) {
                    row.item.remove();
                }
            } catch (error) {
                leaked.push(`${row.member}: ${messageOf(error)}`);
            }
        }
        for (const row of differences(held)) {
            leaked.push(row.member);
        }
        return leaked;
    });

// --- [DOCUMENTS]
const activeOf = (): Host => {
    try {
        return realApp.documents.length > 0 ? realApp.activeDocument : null;
    } catch {
        return null;
    }
};
const owns = (session: Host, doc: Host): boolean => {
    for (const held of session.documents) {
        if (held.isValid && doc.isValid && held.id === doc.id) {
            return true;
        }
    }
    return false;
};
const recovered = (label: string): void => {
    if (realApp.modalState) {
        return;
    }
    for (const open of realApp.documents.everyItem().getElements()) {
        if (open.extractLabel(label) !== '') {
            open.close(SaveOptions.NO);
        }
    }
};

// --- [EXPORTS] -------------------------------------------------------------------------

export { activeOf, header, members, owns, POINTS, pinned, readable, recovered, restore, snapshot, surveyed, writes };
