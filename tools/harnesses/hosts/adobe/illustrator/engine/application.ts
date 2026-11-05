import { ownKeys } from '@rasm/extendscript/extendscript';
import { type Host, holds, messageOf, realApp, statics } from '#/hosts/adobe/extendscript';
import { classed, coded, collected, expanded, kindOf, properties } from '#/hosts/adobe/illustrator/engine/codec';

// --- [CONSTANTS] -----------------------------------------------------------------------

const PREFERENCE = /^set(\w+)Preference$/;
const EXTENSION = /\.[^.]*$/;
const LIVE: readonly string[] = ['activeDocument', 'selection'];

// --- [OPERATIONS] ----------------------------------------------------------------------

// --- [STATE]
const members = (owner: Host, path: string, kinds: Host, rows: Host, unread: string[]): Host => {
    properties(
        owner,
        (property, value) => {
            if (property.type === 'readwrite') {
                rows.push({ path, key: property.name, text: coded.json(value) });
            } else if (classed(property) && kindOf(value) !== null && !collected(value) && !holds(kinds, kindOf(value))) {
                members(value, `${path}${property.name}.`, kinds.concat([kindOf(value)]), rows, unread);
            }
        },
        (property) => !(path === '' && holds(LIVE, property.name)),
        (property, error) => {
            unread.push(`app.${path}${property.name}: ${messageOf(error)}`);
        },
    );
    return rows;
};
const snapshot = (unread?: string[]): Host => members(realApp, '', ['Application'], [], unread ?? []);
const ownerAt = (path: string): Host => {
    let owner = realApp;
    for (const step of path.split('.').slice(0, -1)) {
        owner = owner[step];
    }
    return owner;
};
const plain = (rows: Host): Host => {
    const object: Host = {};
    for (const row of rows) {
        object[row.path + row.key] = eval(`(${row.text})`);
    }
    return object;
};
const differences = (rows: Host): Host[] => {
    const found: Host[] = [];
    for (const row of rows) {
        const now = coded.json(ownerAt(row.path)[row.key]);
        if (now !== row.text) {
            found.push({ row, member: row.path + row.key, before: eval(`(${row.text})`), after: eval(`(${now})`) });
        }
    }
    return found;
};
const restored = (base: Host, start: Host): Host => {
    const rows: Host = { writes: [], leaked: [] };
    for (const change of differences(start)) {
        rows.writes.push({ _tag: 'Application', member: change.member, before: change.before, after: change.after });
    }
    for (const change of differences(base)) {
        try {
            ownerAt(change.row.path)[change.row.key] = coded.revive(change.before);
        } catch (error) {
            rows.leaked.push(`app.${change.member}: ${messageOf(error)}`);
        }
    }
    for (const left of differences(base)) {
        rows.leaked.push(`app.${left.member} reads ${coded.json(left.after)}`);
    }
    return rows;
};

// --- [PREFERENCES]
const remember = (held: Host, key: string, kind: string, touched: boolean): void => {
    const real = realApp.preferences;
    if (!held.hasOwnProperty(key)) {
        held[key] = real.preferenceExists(key) ? { kind, before: real[`get${kind}Preference`](key) } : { kind };
    }
    if (touched) {
        held[key].touched = true;
    }
};
const preferences = (session: Host): Host => {
    const real = realApp.preferences;
    const held = session.preferences;
    const own: Host = {
        removePreference: (key: Host): Host => {
            if (!held.hasOwnProperty(key) && real.preferenceExists(key)) {
                session.record.activations.push(`removePreference ${key}`);
                return;
            }
            return real.removePreference(key);
        },
    };
    for (const method of real.reflect.methods) {
        const parts: Host = PREFERENCE.exec(method.name);
        if (!parts) {
            continue;
        }
        const kind: string = parts[1];
        own[`get${kind}Preference`] = (key: Host): Host => {
            const existed = real.preferenceExists(key);
            const value = real[`get${kind}Preference`](key);
            if (!(existed || held.hasOwnProperty(key)) && real.preferenceExists(key)) {
                real.removePreference(key);
            }
            return value;
        };
        own[`set${kind}Preference`] = (key: Host, value: Host): Host => {
            remember(held, key, kind, true);
            return real[`set${kind}Preference`](key, value);
        };
    }
    return statics({}, real, own, false);
};
const settled = (held: Host): Host => {
    const real = realApp.preferences;
    const rows: Host = { writes: [], leaked: [] };
    for (const key of ownKeys(held)) {
        const row = held[key];
        const present = real.preferenceExists(key);
        const after = present ? real[`get${row.kind}Preference`](key) : null;
        try {
            if (row.hasOwnProperty('before')) {
                real[`set${row.kind}Preference`](key, row.before);
            } else if (present) {
                real.removePreference(key);
            }
        } catch (error) {
            rows.leaked.push(`preference ${key}: ${messageOf(error)}`);
        }
        if (row.touched) {
            rows.writes.push(row.hasOwnProperty('before') ? { _tag: 'Preference', key, before: row.before, after } : { _tag: 'Preference', key, after });
        }
    }
    return rows;
};

// --- [SURVEY]
const surveyed = (types: Host): Host => {
    const presetTypes: Host = DocumentPresetType;
    const presets: Host = {};
    const application: Host = {};
    const methods: string[] = [];
    for (const name of realApp.startupPresetsList) {
        presets[name] = { types: [], settings: expanded(realApp.getPresetSettings(name), []) };
    }
    for (const type of types) {
        const file = realApp.getPresetFileOfType(presetTypes[type]);
        const name = file === null || file === undefined ? '' : decodeURI(file.name).replace(EXTENSION, '');
        if (presets.hasOwnProperty(name)) {
            presets[name].types.push(type);
        }
    }
    properties(
        realApp,
        (property, value) => {
            if (kindOf(value) === null && !collected(value)) {
                application[property.name] = coded.kept(value, []);
            }
        },
        (property) => property.type === 'readonly',
    );
    for (const method of realApp.reflect.methods) {
        methods.push(method.name);
    }
    return { presets, application, methods, options: expanded(new OpenOptions(), []) };
};

// --- [EXPORTS] -------------------------------------------------------------------------

export { plain, preferences, remember, restored, settled, snapshot, surveyed };
