import { escaped } from '@rasm/extendscript';
import { ownKeys } from '@rasm/extendscript/extendscript';
import { assign, attempt, batched, clicked, codec, dollar, failed, finished, globals, type Host, holds, messageOf, palettes, patched, prepared, read, reads, released, sessionOf, started, tagged, thrown, windows } from '#/extendscript';

// --- [TYPES] ---------------------------------------------------------------------------

// --- [CONSTANTS] -----------------------------------------------------------------------

const realApp: Host = app;
const RealFile: Host = File;
const RealFolder: Host = Folder;
const realGlobal: Host = $.global;
const VALUED: Host = {
    textEditboxes: 'editContents',
    measurementEditboxes: 'editValue',
    integerEditboxes: 'editValue',
    realEditboxes: 'editValue',
    percentEditboxes: 'editValue',
    angleEditboxes: 'editValue',
    comboboxes: 'editContents',
    integerComboboxes: 'editValue',
    realComboboxes: 'editValue',
    percentComboboxes: 'editValue',
    angleComboboxes: 'editValue',
    measurementComboboxes: 'editValue',
    dropdowns: 'selectedIndex',
    checkboxControls: 'checkedState',
    radiobuttonGroups: 'selectedButton',
    enablingGroups: 'checkedState',
};
const CONTAINERS: readonly string[] = ['dialogColumns', 'dialogRows', 'borderPanels', 'enablingGroups', 'radiobuttonGroups'];
const WIDGETS: readonly string[] = [
    'staticTexts',
    'textEditboxes',
    'measurementEditboxes',
    'integerEditboxes',
    'realEditboxes',
    'percentEditboxes',
    'angleEditboxes',
    'comboboxes',
    'integerComboboxes',
    'realComboboxes',
    'percentComboboxes',
    'angleComboboxes',
    'measurementComboboxes',
    'dropdowns',
    'checkboxControls',
    'radiobuttonControls',
];
const LIVE: readonly string[] = ['activeDocument', 'activeWindow', 'layoutWindows', 'windows', 'selection'];

// --- [OPERATIONS] ----------------------------------------------------------------------

// --- [CODEC]
let owners: Host = {};
const enumerated = (enumerations: Host): void => {
    owners = {};
    for (const enumeration of ownKeys(enumerations)) {
        for (const member of enumerations[enumeration]) {
            owners[member] = owners[member] || [];
            owners[member].push(enumeration);
        }
    }
};
const candidates = (name: string): Host[] => {
    const found: Host[] = [];
    const listed: Host = Object.prototype.hasOwnProperty.call(owners, name) ? owners[name] : [];
    for (const owner of listed) {
        const held = realGlobal[owner];
        if (held !== undefined && held[name] !== undefined) {
            found.push([owner, held[name]]);
        }
    }
    return found;
};
const qualified = (value: Host): string => {
    const name = String(value);
    const holding: Host[] = [];
    for (const row of candidates(name)) {
        if (Number(row[1]) === Number(value)) {
            holding.push(row[0]);
        }
    }
    return holding.length === 1 ? `enum:${holding[0]}.${name}` : `enum:${name}`;
};
const resolved = (path: string): Host => {
    const dot = path.indexOf('.');
    const found: Host = dot < 0 ? candidates(path) : [[path.slice(0, dot), realGlobal[path.slice(0, dot)] === undefined ? undefined : realGlobal[path.slice(0, dot)][path.slice(dot + 1)]]];
    if (found.length === 0 || found[0][1] === undefined) {
        throw new Error(`enum:${path} names no enumerator`);
    }
    for (let i = 1; i < found.length; i += 1) {
        if (Number(found[i][1]) !== Number(found[0][1])) {
            throw new Error(`enum:${path} names enumerators of ${(owners[path] || []).join(', ')} with different codes`);
        }
    }
    return found[0][1];
};
const coded = codec({
    named: (value: Host): Host => {
        if (value.constructor && value.constructor.name === 'Enumerator') {
            return qualified(value);
        }
        if (typeof value.toSpecifier !== 'function') {
            return null;
        }
        try {
            return `object:${value.toSpecifier()}`;
        } catch (error) {
            return `unreadable:${messageOf(error)}`;
        }
    },
    enumerator: resolved,
    located: (path: Host): Host => realGlobal.resolve.call(realGlobal, path),
});

// --- [DIALOGS]
const collection = (kind: string, owner: Host, dialog: Host): Host => {
    const list: Host = [];
    list.kind = kind;
    list.add = (props: Host): Host => {
        const made = node(kind, owner, dialog);
        const given = props || {};
        for (const key in given) {
            made[key] = given[key];
        }
        if (kind === 'radiobuttonControls' && given.checkedState === true) {
            owner.selectedButton = list.length;
        }
        list.push(made);
        dialog.flat.push({ kind, node: made });
        return made;
    };
    list.item = (index: Host): Host => list[index];
    list.firstItem = (): Host => list[0];
    list.lastItem = (): Host => list[list.length - 1];
    list.everyItem = (): Host => ({ getElements: (): Host => list.slice(0) });
    return list;
};
const node = (kind: string, owner: Host, dialog: Host): Host => {
    const made: Host = {
        parent: owner,
        isValid: true,
        kind,
        destroy: (): void => {
            made.isValid = false;
        },
        staticLabel: '',
        editContents: '',
        editValue: 0,
        selectedIndex: 0,
        stringList: [],
        checkedState: kind === 'enablingGroups',
        selectedButton: 0,
        minWidth: 0,
        minimumValue: 0,
        maximumValue: 0,
        smallNudge: 1,
        largeNudge: 10,
    };
    for (const member of CONTAINERS.concat(WIDGETS)) {
        made[member] = collection(member, made, dialog || made);
    }
    return made;
};
const fakeDialogs = (session: Host): Host => {
    const list: Host = [];
    list.add = (props: Host): Host => {
        const dialog = node('dialog', realApp, null);
        dialog.flat = [];
        dialog.name = '';
        dialog.canCancel = true;
        dialog.label = '';
        const given = props || {};
        for (const key in given) {
            dialog[key] = given[key];
        }
        dialog.show = (): Host => {
            const answer = session.dialogs.shift() || {};
            const values = answer.fields || {};
            const counters: Host = {};
            const before: Host[] = [];
            for (const row of dialog.flat) {
                const member = VALUED[row.kind];
                const seen = counters[row.kind] || 0;
                counters[row.kind] = seen + 1;
                const rowKey = `${row.kind}#${seen}`;
                if (row.kind === 'staticTexts') {
                    before.push({ kind: row.kind, index: seen, label: row.node.staticLabel });
                    continue;
                }
                if (row.kind === 'radiobuttonControls') {
                    before.push({ kind: row.kind, index: seen, label: row.node.staticLabel, checked: row.node.checkedState });
                    continue;
                }
                before.push({ kind: row.kind, index: seen, label: row.node.staticLabel, list: row.kind === 'dropdowns' ? row.node.stringList : undefined, value: member ? row.node[member] : undefined });
                if (member === 'editValue' && row.node.editContents === '') {
                    row.node.editContents = String(row.node.editValue);
                }
                if (values[rowKey] !== undefined) {
                    row.node[member] = values[rowKey];
                    if (member === 'editValue') {
                        row.node.editContents = String(values[rowKey]);
                    }
                }
            }
            session.record.dialogs.push({ name: dialog.name, controls: before });
            return answer.answer === undefined ? true : answer.answer;
        };
        list.push(dialog);
        return dialog;
    };
    list.item = (index: Host): Host => list[index];
    return list;
};

// --- [STATE]
const identified = (value: Host): Host => value.reflect.find.call(value.reflect, 'id');
const valuesOf = (list: Host, key: string): Host => list.everyItem()[key];
const keyOf = (list: Host): string | null => {
    try {
        valuesOf(list, 'id');
        return 'id';
    } catch {}
    try {
        valuesOf(list, 'name');
        return 'name';
    } catch {
        return null;
    }
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
    const found: Host = { preferences: [], collections: [], unread: [] };
    for (const { name } of realApp.reflect.properties) {
        let value: Host;
        if (name === '__proto__' || name === 'parent' || name === 'properties') {
            continue;
        }
        try {
            value = realApp[name];
        } catch (error) {
            found.unread.push(`${name}: ${messageOf(error)}`);
            continue;
        }
        if (value === null || typeof value !== 'object' || value instanceof Array || value instanceof RealFile || !value.reflect) {
            continue;
        }
        if (typeof value.everyItem === 'function') {
            const key = closes(value) ? undefined : keyOf(value);
            if (key === null) {
                found.unread.push(`${name}: items hold neither id nor name`);
            } else if (key !== undefined) {
                found.collections.push({ name, key });
            }
        } else if (!identified(value) && name !== 'scriptPreferences') {
            try {
                if (value.properties !== null && typeof value.properties === 'object') {
                    found.preferences.push(name);
                }
            } catch (error) {
                found.unread.push(`${name}: ${messageOf(error)}`);
            }
        }
    }
    found.preferences.push('scriptPreferences');
    return found;
};
const pinned = (work: () => Host): Host => {
    const unit = realApp.scriptPreferences.measurementUnit;
    realApp.scriptPreferences.measurementUnit = MeasurementUnits.POINTS;
    try {
        return work();
    } finally {
        realApp.scriptPreferences.measurementUnit = unit;
    }
};
const snapshot = (found: Host): Host =>
    pinned((): Host => {
        const held: Host = { preferences: [], collections: [] };
        for (const name of found.preferences) {
            const target = realApp[name];
            const saved = target.properties;
            const encoded: Host = {};
            for (const key of ownKeys(saved)) {
                const text = key === 'measurementUnit' ? '' : coded.json(saved[key]);
                if (text !== '' && text.indexOf('{"$":"object:/document') !== 0) {
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
const added = (tracked: Host, present: Host): Host => {
    const item = tracked.key === 'id' ? tracked.list.itemByID(present) : tracked.list.itemByName(present);
    return { member: tracked.name, after: item, item };
};
const differences = (held: Host): Host =>
    pinned((): Host => {
        const rows: Host[] = [];
        for (const row of held.preferences) {
            const current = row.target.properties;
            for (const key of ownKeys(row.encoded)) {
                const now = coded.json(current[key]);
                if (now.indexOf('{"$":"object:/document') !== 0 && now !== row.encoded[key]) {
                    rows.push({ member: `${row.name}.${key}`, before: row.saved[key], after: current[key], target: row.target, key });
                }
            }
        }
        for (const tracked of held.collections) {
            const seen: Host = {};
            for (const present of valuesOf(tracked.list, tracked.key)) {
                seen[present] = true;
                if (!tracked.known[present]) {
                    rows.push(added(tracked, present));
                }
            }
            for (const gone of ownKeys(tracked.known)) {
                if (!seen[gone]) {
                    rows.push({ member: `${tracked.name}.${tracked.key}=${gone}`, before: gone, after: null });
                }
            }
        }
        return rows;
    });
const written = (rows: Host): Host[] => {
    const out: Host[] = [];
    for (const row of rows) {
        out.push(row.before === undefined ? { member: row.member, after: coded.kept(row.after) } : { member: row.member, before: coded.kept(row.before), after: coded.kept(row.after) });
    }
    return out;
};
const restore = (held: Host): Host =>
    pinned((): Host => {
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
const docIds = (): Host => {
    const ids: Host = {};
    for (const id of realApp.documents.everyItem().id) {
        ids[id] = true;
    }
    return ids;
};
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

// --- [APP]
const forwarded = (key: string, _previous: Host, next: Host): Host => {
    realApp[key] = next;
    return realApp[key];
};
const fakeDocuments = (session: Host): Host => {
    const list: Host = [];
    const sync = (): void => {
        const valid: Host[] = [];
        for (const doc of session.documents) {
            if (doc.isValid && doc.reflect.name === 'Document') {
                valid.push(doc);
            }
        }
        for (let j = 0; j < valid.length; j += 1) {
            list[j] = valid[j];
        }
        list.length = valid.length;
    };
    const at = (index: number): Host => list[index < 0 ? list.length + index : index];
    const missing = (): Host => realApp.documents.itemByID(-1);
    list.add = (showing: Host, preset: Host, properties: Host): Host => {
        const made = typeof showing === 'object' && showing !== null ? realApp.documents.add(true, undefined, showing) : realApp.documents.add(showing === undefined ? true : showing, preset, properties);
        session.documents.push(made);
        sync();
        return made;
    };
    list.count = (): Host => list.length;
    list.item = (key: Host): Host => (typeof key === 'number' ? at(key) || missing() : list.itemByName(key));
    list.itemByName = (name: Host): Host => {
        for (const doc of list) {
            if (doc.name === name) {
                return doc;
            }
        }
        return missing();
    };
    list.itemByID = (id: Host): Host => {
        for (const doc of list) {
            if (doc.id === id) {
                return doc;
            }
        }
        return missing();
    };
    list.itemByRange = (from: Host, to: Host): Host => {
        const start = typeof from === 'number' ? from : list.indexOf(from);
        const end = typeof to === 'number' ? to : list.indexOf(to);
        return list.slice(Math.min(start, end), Math.max(start, end) + 1);
    };
    list.indexOf = (doc: Host): number => {
        for (let i = 0; i < list.length; i += 1) {
            if (list[i].id === doc.id) {
                return i;
            }
        }
        return -1;
    };
    list.firstItem = (): Host => at(0) || missing();
    list.lastItem = (): Host => at(-1) || missing();
    list.anyItem = (): Host => at(0) || missing();
    list.middleItem = (): Host => at(Math.floor(list.length / 2)) || missing();
    list.previousItem = (doc: Host): Host => at(list.indexOf(doc) - 1) || missing();
    list.nextItem = (doc: Host): Host => {
        const index = list.indexOf(doc);
        return index < 0 ? missing() : at(index + 1) || missing();
    };
    list.everyItem = (): Host => {
        const every: Host = { getElements: (): Host => list.slice(0) };
        for (const { name } of realApp.documents.everyItem().reflect.properties) {
            if (name === '__proto__') {
                continue;
            }
            try {
                const values: Host[] = [];
                for (const doc of list) {
                    values.push(doc[name]);
                }
                every[name] = values;
            } catch {}
        }
        return every;
    };
    list.sync = sync;
    sync();
    return list;
};
const fakeMenus = (session: Host): Host => {
    const menu: Host = {
        submenus: { item: (): Host => menu, itemByName: (): Host => menu },
        menuItems: {
            item: (name: Host): Host => ({ name, isValid: true }),
            itemByName: (name: Host): Host => ({ name, isValid: true }),
            add: (action: Host): Host => {
                session.record.menus.push({ added: action ? action.title : action });
                return {};
            },
        },
        menuSeparators: { add: (): Host => ({}) },
        isValid: true,
    };
    const actions: Host = {
        add: (title: Host): Host => {
            const action: Host = { title, name: title, enabled: true, checked: false, isValid: true, listeners: {} };
            action.addEventListener = (type: Host, handler: Host): Host => {
                action.listeners[type] = handler;
                return {};
            };
            action.remove = (): void => {
                action.isValid = false;
            };
            session.actions.push(action);
            return action;
        },
        itemByName: (): Host => ({ isValid: false }),
        everyItem: (): Host => ({ getElements: (): Host => session.actions.slice(0) }),
    };
    return { scriptMenuActions: actions, menus: { item: (): Host => menu, itemByName: (): Host => menu } };
};
const fakePreferences = (): Host => {
    const real = realApp.scriptPreferences;
    const held: Host = {};
    const names: string[] = [];
    const assigned = (key: string, _previous: Host, next: Host): Host => {
        if (key === 'userInteractionLevel') {
            return next;
        }
        real[key] = next;
        return real[key];
    };
    for (const { name } of real.reflect.properties) {
        if (name !== '__proto__' && name !== 'parent' && name !== 'properties' && name !== 'reflect') {
            names.push(name);
        }
    }
    const synced = (): void => {
        for (const name of names) {
            held.unwatch(name);
            try {
                held[name] = real[name];
            } catch {
                continue;
            }
            held.watch(name, assigned);
        }
    };
    synced();
    return { held, synced };
};
const fakeApp = (session: Host, item: Host): Host => {
    const fake: Host = {};
    const preferences = fakePreferences();
    const own: Host = {
        dialogs: fakeDialogs(session),
        scriptPreferences: preferences.held,
        documents: fakeDocuments(session),
        activate: (): void => {
            session.record.activations.push('activate');
        },
        quit: (): void => {
            session.record.activations.push('quit');
        },
        doScript(script: Host, language: Host, withArguments: Host): Host {
            if (typeof script === 'function') {
                return realApp.doScript.apply(realApp, arguments);
            }
            if (language !== undefined && language !== ScriptLanguage.JAVASCRIPT) {
                throw new Error(`doScript in ${String(language)} runs outside the harness shadow`);
            }
            return session.evaluate(script instanceof RealFile ? read(script.fsName) : String(script), withArguments || []);
        },
        open: (file: Host, showing: Host, options: Host): Host => {
            const result = realApp.open(file, showing === undefined ? true : showing, options);
            for (const doc of result instanceof Array ? result : [result]) {
                session.documents.push(doc);
            }
            refresh();
            return result;
        },
    };
    if (item.source !== undefined) {
        own.activeScript = new RealFile(item.source.script);
    }
    if (session.menus) {
        const menus = fakeMenus(session);
        own.scriptMenuActions = menus.scriptMenuActions;
        own.menus = menus.menus;
    }
    const held = (name: string, value: Host): void => {
        fake.unwatch(name);
        if (value === undefined) {
            delete fake[name];
        } else {
            fake[name] = value;
        }
        fake.watch(name, (key: string, _previous: Host, next: Host): Host => {
            realApp[key] = next;
            refresh();
            return fake[key];
        });
    };
    const windowed = (shown: Host): Host[] => {
        const kept: Host[] = [];
        for (const row of shown) {
            if (owns(session, row.parent)) {
                kept.push(row);
            }
        }
        return kept;
    };
    const refresh = (): void => {
        const active = activeOf();
        const owned = active !== null && owns(session, active);
        let fallback: Host;
        for (let i = session.documents.length - 1; i >= 0 && fallback === undefined; i -= 1) {
            if (session.documents[i].isValid && session.documents[i].reflect.name === 'Document') {
                fallback = session.documents[i];
            }
        }
        held('activeDocument', owned ? active : fallback);
        held('selection', owned ? realApp.selection : []);
        held('activeWindow', owned ? realApp.activeWindow : undefined);
        held('layoutWindows', windowed(realApp.layoutWindows.everyItem().getElements()));
        held('windows', windowed(realApp.windows.everyItem().getElements()));
        own.documents.sync();
        preferences.synced();
    };
    const reflection = realApp.reflect;
    for (const { name } of reflection.properties) {
        if (name === '__proto__' || Object.prototype.hasOwnProperty.call(own, name) || holds(LIVE, name)) {
            continue;
        }
        try {
            fake[name] = realApp[name];
        } catch {
            continue;
        }
        fake.watch(name, forwarded);
    }
    for (const { name } of reflection.methods) {
        if (name === 'toSource' || name === 'toSpecifier' || Object.prototype.hasOwnProperty.call(own, name)) {
            continue;
        }
        fake[name] = function (): Host {
            const result = realApp[name].apply(realApp, arguments);
            refresh();
            return result;
        };
    }
    for (const key of ownKeys(own)) {
        fake[key] = own[key];
    }
    refresh();
    session.refresh = refresh;
    return fake;
};

// --- [PRECONDITION]
const evaluated = (session: Host, expression: string, doc: Host, made: Host, statement?: boolean): Host => new Function('doc', 'app', 'made', statement ? expression : `return (${expression});`)(doc, session.app, made);
const differing = (target: Host, wanted: Host, path: string): string[] => {
    if (wanted !== null && typeof wanted === 'object' && !(wanted instanceof Array) && !tagged(wanted)) {
        let rows: string[] = [];
        for (const key of ownKeys(wanted)) {
            rows = rows.concat(differing(target[key], wanted[key], `${path}.${key}`));
        }
        return rows;
    }
    const same = wanted !== null && typeof wanted === 'object' && tagged(wanted) && wanted.$.indexOf('enum:') === 0 ? Number(target) === Number(coded.revive(wanted)) : coded.json(target) === coded.json(wanted) || (typeof wanted === 'string' && target !== null && typeof target === 'object' && target.name === wanted);
    return same ? [] : [`${path} reads ${coded.json(target)} where the precondition asks ${coded.json(wanted)}`];
};
const opened = (session: Host, item: Host, batch: Host): Host[] => {
    const condition = item.precondition;
    realApp.scriptPreferences.measurementUnit = coded.revive(condition.units.script);
    const docs: Host[] = [];
    for (let i = 0; i < condition.documents.length; i += 1) {
        const row = condition.documents[i];
        const doc = row.open === undefined ? realApp.documents.add(true, undefined, coded.revive(row.properties)) : realApp.open(new RealFile(row.open), true);
        session.documents.push(doc);
        doc.insertLabel(batch.label, batch.records);
        if (row.open !== undefined) {
            doc.viewPreferences.horizontalMeasurementUnits = coded.revive(condition.units.horizontal);
            doc.viewPreferences.verticalMeasurementUnits = coded.revive(condition.units.vertical);
        }
        const wrong = row.open === undefined ? differing(doc, row.properties, `document ${i}`) : [];
        if (wrong.length > 0) {
            throw new Error(wrong.join(', '));
        }
        docs.push(doc);
    }
    const missing: string[] = [];
    for (const name of condition.fonts || []) {
        const font = realApp.fonts.itemByName(name);
        const status = font.isValid ? font.status : undefined;
        if (status === undefined || Number(status) !== Number(FontStatus.INSTALLED)) {
            missing.push(`${name} ${String(status === undefined ? 'absent' : status)}`);
        }
    }
    if (missing.length > 0) {
        throw new Error(`fonts not installed: ${missing.join(', ')}`);
    }
    if (docs.length > 1) {
        realApp.activeDocument = docs[0];
    }
    session.refresh();
    return docs;
};
const state = (doc: Host): Host => {
    const row: Host = { name: doc.name };
    for (const { name } of doc.reflect.properties) {
        let value: Host;
        if (name === '__proto__' || name === 'parent' || name === 'properties') {
            continue;
        }
        try {
            value = doc[name];
        } catch {
            continue;
        }
        if (value === null || typeof value !== 'object' || value instanceof Array || !value.reflect || typeof value.everyItem === 'function' || identified(value)) {
            continue;
        }
        try {
            row[name] = value.properties;
        } catch (error) {
            row[name] = `unread: ${messageOf(error)}`;
        }
    }
    row.cmykProfile = doc.cmykProfile;
    row.rgbProfile = doc.rgbProfile;
    row.appliedLanguage = doc.textDefaults.appliedLanguage.name;
    row.fonts = { names: doc.fonts.everyItem().name, status: doc.fonts.everyItem().status };
    row.links = { names: doc.links.everyItem().name, status: doc.links.everyItem().status };
    return coded.kept(row);
};
const prepare = (session: Host, doc: Host, condition: Host, made: Host): void => {
    const evaluate = (expression: string): Host => evaluated(session, expression, doc, made);
    const labels = condition.labels || {};
    const selected: Host[] = [];
    for (const label of ownKeys(labels)) {
        doc.insertLabel(label, labels[label]);
    }
    prepared([
        [
            'set',
            condition.set,
            (row: Host): void => {
                assign(row[0], row[1], evaluate, coded.revive);
            },
        ],
        [
            'add',
            condition.add,
            (row: Host): void => {
                made[row.as] = evaluate(row.to)[row.kind].add(coded.revive(row.properties || {}));
            },
        ],
        [
            'steps',
            condition.steps,
            (step: Host): void => {
                evaluated(session, step, doc, made, true);
            },
        ],
        [
            'select',
            condition.select,
            (pick: Host): void => {
                selected.push(evaluate(pick));
            },
        ],
    ]);
    if (selected.length > 0) {
        realApp.select(selected);
    }
    session.refresh();
};
const readings = (session: Host, docs: Host, wanted: Host, made: Host): Host => {
    const unit = realApp.scriptPreferences.measurementUnit;
    const frames: Host[] = [];
    realApp.scriptPreferences.measurementUnit = MeasurementUnits.POINTS;
    for (const doc of docs) {
        if (!doc.isValid) {
            continue;
        }
        frames.push([doc, doc.viewPreferences.rulerOrigin, doc.zeroPoint]);
        doc.viewPreferences.rulerOrigin = RulerOrigin.PAGE_ORIGIN;
        doc.zeroPoint = [0, 0];
    }
    try {
        return reads(wanted, (expression: string): Host => evaluated(session, expression, docs[0], made), coded.kept);
    } finally {
        for (const frame of frames) {
            if (frame[0].isValid) {
                frame[0].viewPreferences.rulerOrigin = frame[1];
                frame[0].zeroPoint = frame[2];
            }
        }
        realApp.scriptPreferences.measurementUnit = unit;
    }
};
const exported = (item: Host, docs: Host, moment: string): void => {
    if (item.precondition.idml !== true) {
        return;
    }
    for (let i = 0; i < docs.length; i += 1) {
        if (docs[i].isValid) {
            docs[i].exportFile(ExportFormat.INDESIGN_MARKUP, new RealFile(`${item.folder}/${i}.${moment}.idml`));
        }
    }
};

// --- [RUN]
const Exited: Host = function (): void {};
const scoped: Host = eval('(function (Window, alert, confirm, prompt, app, File, Folder, exit, $, document, source, extra) { return (function () { return eval(source); }).apply(undefined, extra); })');
const body = (item: Host): string => {
    if (item._tag === 'Probe') {
        return `(function (doc, made) { return (${item.evaluate}); }).apply(undefined, arguments);`;
    }
    if (item._tag === 'Call') {
        const source = item.source === undefined ? '' : item.source.code.indexOf('@JSXBIN@') === 0 ? `eval(${escaped(item.source.code)});` : item.source.code;
        return `${source}\n;(${item.call}).apply(undefined, arguments);`;
    }
    return item.source.code;
};
const sessionFor = (item: Host): Host => {
    const session = sessionOf(item, ['dialogs', 'menus', 'undo']);
    session.actions = [];
    session.dialogs = (item.dialogs || []).slice(0);
    session.exited = false;
    session.menus = item.precondition.menus === true;
    return session;
};
const interacting = (item: Host, work: () => Host): Host => {
    if (item.interact !== true) {
        return work();
    }
    realApp.scriptPreferences.userInteractionLevel = UserInteractionLevels.INTERACT_WITH_ALL;
    try {
        return work();
    } finally {
        realApp.scriptPreferences.userInteractionLevel = UserInteractionLevels.NEVER_INTERACT;
    }
};
const ran = (session: Host, item: Host, extra: Host, result: Host): Host => {
    try {
        const value = interacting(item, (): Host => session.evaluate(body(item), extra));
        palettes(session);
        for (const action of session.actions) {
            const handler = action.listeners.onInvoke;
            session.record.menus.push({ title: action.title, invoked: typeof handler === 'function' });
            if (typeof handler === 'function') {
                handler.call(action, { eventType: 'onInvoke' });
            }
        }
        if (item._tag !== 'Run') {
            result.value = coded.kept(value);
        }
        return { _tag: 'Ran' };
    } catch (error) {
        return error instanceof Exited || session.exited ? { _tag: 'Exited' } : thrown(error);
    }
};
const closedDocument = (open: Host): void => {
    if (open.isValid) {
        if (open.reflect.name === 'Library') {
            open.close();
        } else {
            open.close(SaveOptions.NO);
        }
    }
};
const outcome = (item: Host, batch: Host, found: Host): Host => {
    const session = sessionFor(item);
    const result = started(item);
    let leaked: string[] = [];
    const unit = realApp.scriptPreferences.measurementUnit;
    const active = activeOf();
    const activeId = active === null ? null : active.id;
    const known = docIds();
    let held: Host = null;
    const global = globals();
    let release: Host = (): void => {};
    const made: Host = {};
    try {
        held = snapshot(found);
        const Fake: Host = windows(session, clicked);
        const shadows: Host = [
            Fake,
            Fake.alert,
            Fake.confirm,
            Fake.prompt,
            fakeApp(session, item),
            RealFile,
            RealFolder,
            (): void => {
                session.exited = true;
                throw new Exited();
            },
            dollar(session, item, undefined),
        ];
        session.app = shadows[4];
        session.evaluate = (code: Host, extra: Host): Host => scoped.apply(undefined, shadows.concat([session.app.activeDocument, code, extra]));
        release = patched(session);
        const docs = opened(session, item, batch);
        for (const doc of docs) {
            session.record.opened.push(state(doc));
        }
        prepare(session, docs[0], item.precondition, made);
        const before = readings(session, docs, item.read, made);
        result.before = before.state;
        result.beforeFailed = before.failed;
        const inner: Host = {};
        try {
            realApp.doScript(
                (): void => {
                    try {
                        exported(item, docs, 'before');
                        const history: Host[] = [];
                        for (const doc of docs) {
                            history.push(doc.undoHistory.length);
                        }
                        inner.completion = ran(session, item, item._tag === 'Probe' ? [docs[0], made] : coded.revive(item.args), result);
                        inner.undo = [];
                        for (let d = 0; d < docs.length; d += 1) {
                            if (!docs[d].isValid) {
                                continue;
                            }
                            const now = docs[d].undoHistory;
                            for (let u = 0; u < now.length - history[d]; u += 1) {
                                inner.undo.push(now[u]);
                            }
                        }
                        inner.after = readings(session, docs, item.read, made);
                        inner.writes = written(differences(held));
                        exported(item, docs, 'after');
                    } catch (error) {
                        inner.error = error;
                    }
                    throw new Error('rollback');
                },
                ScriptLanguage.JAVASCRIPT,
                [],
                UndoModes.ENTIRE_SCRIPT,
                item.name,
            );
        } catch (rollback) {
            if (inner.error !== undefined) {
                throw inner.error;
            }
            if (inner.completion === undefined) {
                throw rollback;
            }
        }
        result.completion = inner.completion;
        result.read = inner.after.state;
        result.readFailed = inner.after.failed;
        session.record.writes = inner.writes;
        session.record.undo = inner.undo;
    } catch (outer) {
        result.completion = { _tag: 'Failed', reason: failed(outer) };
    }
    attempt(leaked, 'prototypes', release);
    for (const shown of session.created) {
        attempt(leaked, 'window', (): void => {
            shown.close();
        });
    }
    for (const open of session.documents) {
        attempt(leaked, 'close', (): void => closedDocument(open));
    }
    attempt(leaked, 'documents', (): void => {
        for (const open of realApp.documents.everyItem().getElements()) {
            if (!known[open.id]) {
                leaked.push(`document ${open.name}${owns(session, open) ? ' stays open after close' : ' opened outside the case'}`);
            }
        }
    });
    attempt(leaked, 'restore', (): void => {
        if (held !== null) {
            leaked = leaked.concat(restore(held));
        }
    });
    attempt(leaked, 'globals', (): void => {
        leaked = leaked.concat(released(global));
    });
    attempt(leaked, 'scriptPreferences', (): void => {
        realApp.scriptPreferences.measurementUnit = unit;
    });
    attempt(leaked, 'activeDocument', (): void => {
        const now = activeOf();
        if (activeId !== null && (now === null || now.id !== activeId)) {
            realApp.activeDocument = realApp.documents.itemByID(activeId);
        }
    });
    return finished(result, session.record, leaked);
};
const header = (found: Host): Host => {
    const defaults: Host = {};
    for (const name of found.preferences) {
        defaults[name] = realApp[name].properties;
    }
    const settings = realApp.colorSettings;
    return { application: realApp.name, version: realApp.version, state: { modal: realApp.modalState, cmyk: settings.workingSpaceCMYK, rgb: settings.workingSpaceRGB, defaults, unread: found.unread } };
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

// --- [COMPOSITION] ---------------------------------------------------------------------

const run = (text: string): string => {
    const level = realApp.scriptPreferences.userInteractionLevel;
    realApp.scriptPreferences.userInteractionLevel = UserInteractionLevels.NEVER_INTERACT;
    try {
        const batch = eval(`(${text})`);
        enumerated(batch.enumerations);
        recovered(batch.label);
        const found = members();
        const answer = header(found);
        batched(batch, coded, (item: Host): Host => outcome(item, batch, found));
        return coded.json(answer);
    } catch (error) {
        return coded.json({ failed: failed(error) });
    } finally {
        realApp.scriptPreferences.userInteractionLevel = level;
    }
};

// --- [EXPORTS] -------------------------------------------------------------------------

export { run };
