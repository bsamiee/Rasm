import { ownKeys } from '@rasm/extendscript/extendscript';
import { type Host, holds, messageOf, realApp, realGlobal } from '#/hosts/adobe/extendscript';
import { action, coded, executed, id, layersOf, parsedJson, specifier, withId } from '#/hosts/adobe/photoshop/engine/codec';

// --- [CONSTANTS] -----------------------------------------------------------------------

const PARTS: readonly string[] = ['guide', 'channel', 'path'];
const LINES = /\r\n|\r|\n/;
const LOGGED = /^\d{4}:\d\d:\d\d \d\d:\d\d:\d\d : (.*?) : (.*?) : (.*)$/;

// --- [OPERATIONS] ----------------------------------------------------------------------

// --- [SNAPSHOTS]
const snapshot = (descriptor: Host, whole: Host): Host => {
    const text: Host = {};
    for (const key of ownKeys(whole)) {
        if (key !== '_obj') {
            text[key] = coded.json(whole[key]);
        }
    }
    return { descriptor, whole, text };
};
const described = (reference: Host): Host => {
    const descriptor: Host = realGlobal.executeActionGet(reference);
    return snapshot(descriptor, action(descriptor));
};
const application = (): Host => {
    const reference: Host = new ActionReference();
    reference.putEnumerated(id('application'), id('ordinal'), id('targetEnum'));
    return described(reference);
};
const treeOf = (docId: Host): Host => {
    const reference: Host = new ActionReference();
    reference.putProperty(id('property'), id('json'));
    reference.putIdentifier(id('document'), docId);
    const descriptor: Host = new ActionDescriptor();
    descriptor.putReference(id('null'), reference);
    descriptor.putBoolean(id('getPathData'), true);
    const answer = executed('get', descriptor);
    return snapshot(answer, parsedJson(answer.getString(id('json'))));
};
const targets = (ids: readonly Host[]): Host => {
    const rows: Host = {};
    for (const docId of ids) {
        const doc = withId(realApp.documents, docId);
        if (doc === null) {
            continue;
        }
        const prefix = String(specifier(doc));
        const reference: Host = new ActionReference();
        reference.putIdentifier(id('document'), docId);
        rows[prefix] = described(reference);
        rows[`${prefix}/json`] = treeOf(docId);
        for (const part of PARTS) {
            const count = rows[prefix].whole[`numberOf${part.charAt(0).toUpperCase()}${part.slice(1)}s`] || 0;
            for (let n = 1; n <= count; n += 1) {
                const indexed: Host = new ActionReference();
                indexed.putIndex(id(part), n);
                indexed.putIdentifier(id('document'), docId);
                rows[`${prefix}/${part}[${n}]`] = described(indexed);
            }
        }
        for (const layer of layersOf(doc)) {
            const identified: Host = new ActionReference();
            identified.putIdentifier(id('layer'), layer.id);
            identified.putIdentifier(id('document'), docId);
            rows[`${prefix}/layer[@id=${layer.id}]`] = described(identified);
        }
    }
    return rows;
};
const opened = (rows: Host, docs: readonly Host[]): Host[] => {
    const states: Host[] = [];
    for (const doc of docs) {
        const prefix = String(specifier(doc));
        const layers: Host[] = [];
        const parts: Host = {};
        for (const key of ownKeys(rows)) {
            if (key.indexOf(`${prefix}/layer[`) === 0) {
                layers.push(rows[key].whole);
            } else if (key.indexOf(`${prefix}/`) === 0) {
                parts[key.slice(prefix.length + 1)] = rows[key].whole;
            }
        }
        states.push({ document: rows[prefix].whole, layers, parts });
    }
    return states;
};

// --- [CHANGES]
const differing = (before: Host, after: Host, ignored: readonly string[]): string[] => {
    const keys: Host = {};
    const rows: string[] = [];
    for (const key of ownKeys(before.text).concat(ownKeys(after.text))) {
        if (!(Object.prototype.hasOwnProperty.call(keys, key) || holds(ignored, key)) && before.text[key] !== after.text[key]) {
            rows.push(key);
        }
        keys[key] = true;
    }
    return rows;
};
const changed = (before: Host, after: Host, ignored: readonly string[], target?: Host): Host[] => {
    const rows: Host[] = [];
    for (const key of differing(before, after, ignored)) {
        const row: Host = target === undefined ? {} : { target };
        row.member = key;
        if (Object.prototype.hasOwnProperty.call(before.whole, key)) {
            row.before = before.whole[key];
        }
        row.after = after.whole[key];
        rows.push(row);
    }
    return rows;
};
const edited = (before: Host, after: Host, ignored: readonly string[]): Host[] => {
    let rows: Host[] = [];
    for (const key of ownKeys(before)) {
        rows = rows.concat(Object.prototype.hasOwnProperty.call(after, key) ? changed(before[key], after[key], ignored, key) : [{ target: key, member: String(before[key].whole._obj), before: before[key].whole, after: undefined }]);
    }
    for (const key of ownKeys(after)) {
        if (!Object.prototype.hasOwnProperty.call(before, key)) {
            rows.push({ target: key, member: String(after[key].whole._obj), after: after[key].whole });
        }
    }
    return rows;
};
const shifted = (label: string, folder: string): Host => {
    const first = application();
    const doc = realApp.documents.add(new UnitValue(1, 'px'), new UnitValue(1, 'px'), 72, `${label}-shift`);
    const start = targets([doc.id]);
    doc.saveAs(new File(`${folder}/${label}-shift.psd`), new PhotoshopSaveOptions(), false);
    const saved = targets([doc.id]);
    doc.close(SaveOptions.DONOTSAVECHANGES);
    const second = application();
    const shifting = differing(first, second, []);
    const defaults: Host = {};
    let saving: string[] = [];
    for (const key of ownKeys(start)) {
        if (Object.prototype.hasOwnProperty.call(saved, key)) {
            saving = saving.concat(differing(start[key], saved[key], saving));
        }
    }
    for (const key of ownKeys(second.whole)) {
        if (key !== '_obj' && !holds(shifting, key)) {
            defaults[key] = second.whole[key];
        }
    }
    return { shifting, saving, defaults };
};

// --- [HISTORY]
const histories = (docs: readonly Host[]): Host => {
    const counts: Host = {};
    for (const doc of docs) {
        counts[doc.id] = doc.historyStates.length;
    }
    return counts;
};
const added = (counts: Host): string[] => {
    const names: string[] = [];
    for (const key of ownKeys(counts)) {
        const doc = withId(realApp.documents, Number(key));
        if (doc === null) {
            continue;
        }
        for (let i = doc.historyStates.length - 1; i >= counts[key]; i -= 1) {
            names.push(doc.historyStates[i].name);
        }
    }
    return names;
};

// --- [RESTORES]
const restorable = (initial: Host, key: string): Host => {
    const typed = key.charAt(0) === '$' ? realGlobal.charIDToTypeID(key.slice(1)) : id(key);
    return initial.descriptor.hasKey(typed) && initial.descriptor.getType(typed) === DescValueType.OBJECTTYPE ? typed : null;
};
const restored = (initial: Host, shifting: readonly string[]): string[] => {
    const refusals: Host = {};
    for (const name of differing(initial, application(), shifting)) {
        const key = restorable(initial, name);
        if (key === null) {
            continue;
        }
        const reference: Host = new ActionReference();
        reference.putProperty(id('property'), key);
        reference.putEnumerated(id('application'), id('ordinal'), id('targetEnum'));
        const target: Host = new ActionDescriptor();
        target.putReference(id('null'), reference);
        target.putObject(id('to'), initial.descriptor.getObjectType(key), initial.descriptor.getObjectValue(key));
        try {
            executed('set', target);
        } catch (refused) {
            refusals[name] = messageOf(refused);
        }
    }
    const leaked: string[] = [];
    for (const name of differing(initial, application(), shifting)) {
        if (restorable(initial, name) !== null) {
            leaked.push(Object.prototype.hasOwnProperty.call(refusals, name) ? `${name}: ${refusals[name]}` : name);
        }
    }
    return leaked;
};
const custom = (key: Host): Host => {
    try {
        return realApp.getCustomOptions(key);
    } catch {
        return null;
    }
};
const optionsRestored = (session: Host): void => {
    for (const key of ownKeys(session.options)) {
        const held = session.options[key];
        const after = custom(key);
        const row: Host = { target: 'customOptions', member: key };
        if (held.prior !== null) {
            row.before = action(held.prior);
        }
        row.after = after === null ? undefined : action(after);
        session.record.writes.push(row);
        if (held.prior !== null) {
            realApp.putCustomOptions(key, held.prior, held.persistent);
        } else if (after !== null) {
            realApp.eraseCustomOptions(key);
        }
    }
};

// --- [ERRORS]
const errorLog = (): (() => Host[]) => {
    const log: Host = new File(`${realApp.preferencesFolder.fsName}/PSErrorLog.txt`);
    const offset = Math.max(log.length, 0);
    return () => {
        const entries: Host[] = [];
        let entry: Host = null;
        log.encoding = 'UTF-8';
        if (!(log.exists && log.open('r'))) {
            return entries;
        }
        log.seek(offset);
        const text: string = log.read();
        log.close();
        for (const line of text.split(LINES)) {
            const parts: Host = LOGGED.exec(line);
            if (!parts) {
                continue;
            }
            if (parts[1] === 'Version') {
                entry = { version: parts[3], id: '', code: '', message: '', frames: [] };
                entries.push(entry);
            } else if (entry !== null && parts[1] === 'Stack') {
                for (const offsets of parsedJson(parts[3]).offsets) {
                    for (const module of ownKeys(offsets)) {
                        for (const at of offsets[module]) {
                            entry.frames.push(`${module} +0x${at.toString(16)}`);
                        }
                    }
                }
            } else if (entry !== null) {
                entry.id = parts[1];
                entry.code = parts[2];
                entry.message = parts[3];
            }
        }
        return entries;
    };
};

// --- [EXPORTS] -------------------------------------------------------------------------

export { added, application, changed, custom, edited, errorLog, histories, opened, optionsRestored, restored, shifted, targets };
