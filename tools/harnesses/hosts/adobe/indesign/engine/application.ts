import { ownKeys } from '@rasm/extendscript/extendscript';
import { type Host, holds, read, realApp } from '#/hosts/adobe/extendscript';
import { activeOf, owns, readable } from '#/hosts/adobe/indesign/engine/instance';

// --- [CONSTANTS] -----------------------------------------------------------------------

const CONTROLS: Host = {
    dialogColumns: '',
    dialogRows: '',
    borderPanels: '',
    enablingGroups: 'checkedState',
    radiobuttonGroups: 'selectedButton',
    staticTexts: '',
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
    radiobuttonControls: 'checkedState',
};
const LIVE: readonly string[] = ['activeDocument', 'activeWindow', 'layoutWindows', 'windows', 'selection'];

// --- [OPERATIONS] ----------------------------------------------------------------------

// --- [DIALOGS]
const collection = (kind: string, owner: Host, dialog: Host): Host => {
    const list: Host = [];
    list.kind = kind;
    list.add = (props: Host): Host => {
        const made = control(kind, owner, dialog);
        const given = props || {};
        for (const key in given) {
            made[key] = given[key];
        }
        if (kind === 'radiobuttonControls' && given.checkedState === true) {
            owner.selectedButton = list.length;
        }
        list.push(made);
        dialog.flat.push(made);
        return made;
    };
    list.item = (index: Host): Host => list[index];
    list.firstItem = (): Host => list[0];
    list.lastItem = (): Host => list[list.length - 1];
    list.everyItem = (): Host => ({ getElements: (): Host => list.slice(0) });
    return list;
};
const control = (kind: string, owner: Host, dialog: Host): Host => {
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
    for (const member of ownKeys(CONTROLS)) {
        made[member] = collection(member, made, dialog || made);
    }
    return made;
};
const fakeDialogs = (session: Host): Host => {
    const list: Host = [];
    list.add = (props: Host): Host => {
        const dialog = control('dialog', realApp, null);
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
            const fields = answer.fields || {};
            const counters: Host = {};
            const controls: Host[] = [];
            for (const made of dialog.flat) {
                const member = CONTROLS[made.kind];
                const index = counters[made.kind] || 0;
                const field = fields[`${made.kind}#${index}`];
                const row: Host = { kind: made.kind, index, label: made.staticLabel };
                counters[made.kind] = index + 1;
                controls.push(row);
                if (made.kind === 'dropdowns') {
                    row.list = made.stringList;
                }
                if (member === '') {
                    continue;
                }
                row.value = made[member];
                if (field !== undefined) {
                    made[member] = field;
                }
                if (member === 'editValue' && (field !== undefined || made.editContents === '')) {
                    made.editContents = String(made.editValue);
                }
            }
            session.record.dialogs.push({ name: dialog.name, controls });
            return answer.answer === undefined ? true : answer.answer;
        };
        list.push(dialog);
        return dialog;
    };
    list.item = (index: Host): Host => list[index];
    return list;
};

// --- [DOCUMENTS]
const fakeDocuments = (session: Host): Host => {
    const list: Host = [];
    const sync = (): void => {
        let count = 0;
        for (const doc of session.documents) {
            if (doc.isValid && doc.reflect.name === 'Document') {
                list[count] = doc;
                count += 1;
            }
        }
        list.length = count;
    };
    const missing = (): Host => realApp.documents.itemByID(-1);
    const at = (index: number): Host => list[index < 0 ? list.length + index : index] || missing();
    const where = (key: string, value: Host): Host => {
        for (const doc of list) {
            if (doc[key] === value) {
                return doc;
            }
        }
        return missing();
    };
    list.add = (showing: Host, preset: Host, properties: Host): Host => {
        const made = typeof showing === 'object' && showing !== null ? realApp.documents.add(true, undefined, showing) : realApp.documents.add(showing === undefined ? true : showing, preset, properties);
        session.documents.push(made);
        sync();
        return made;
    };
    list.count = (): Host => list.length;
    list.item = (key: Host): Host => (typeof key === 'number' ? at(key) : where('name', key));
    list.itemByName = (name: Host): Host => where('name', name);
    list.itemByID = (id: Host): Host => where('id', id);
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
    list.firstItem = (): Host => at(0);
    list.lastItem = (): Host => at(-1);
    list.anyItem = (): Host => at(0);
    list.middleItem = (): Host => at(Math.floor(list.length / 2));
    list.previousItem = (doc: Host): Host => at(list.indexOf(doc) - 1);
    list.nextItem = (doc: Host): Host => {
        const index = list.indexOf(doc);
        return index < 0 ? missing() : at(index + 1);
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

// --- [MENUS]
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

// --- [PREFERENCES]
const fakePreferences = (): Host => {
    const real = realApp.scriptPreferences;
    const held: Host = {};
    const names = readable(real);
    const assigned = (key: string, _previous: Host, next: Host): Host => {
        if (key === 'userInteractionLevel') {
            return next;
        }
        real[key] = next;
        return real[key];
    };
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

// --- [APPLICATION]
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
            return session.evaluate(script instanceof File ? read(script.fsName) : String(script), withArguments || []);
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
        own.activeScript = new File(item.source.script);
    }
    if (session.menus) {
        const menus = fakeMenus(session);
        own.scriptMenuActions = menus.scriptMenuActions;
        own.menus = menus.menus;
    }
    const forwarded = (key: string, _previous: Host, next: Host): Host => {
        realApp[key] = next;
        return realApp[key];
    };
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
    for (const { name } of realApp.reflect.properties) {
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
    for (const { name } of realApp.reflect.methods) {
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

// --- [EXPORTS] -------------------------------------------------------------------------

export { fakeApp };
