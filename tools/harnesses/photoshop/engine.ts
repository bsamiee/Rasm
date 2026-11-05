import { ownKeys } from '@rasm/extendscript/extendscript';
import { assign, attempt, batched, bridgeTalk, codec, failed, finished, globals, type Host, holds, messageOf, notified, palettes, patched, picked, prepared, reads, refusing, released, scope, sessionOf, started, statics, thrown, windows } from '#/extendscript';

// --- [CONSTANTS] -----------------------------------------------------------------------

const realApp: Host = app;
const realGlobal: Host = $.global;
const RealFile: Host = File;
const RealFolder: Host = Folder;
const RealFunction: Host = Function;
const realExecute: Host = realGlobal.executeAction;
const realOpen: Host = RealFile.prototype.open;
const ENGINE: string = new RealFile($.fileName).fsName;
const ENUMERATOR = /^([A-Za-z]\w*)\.([A-Z][A-Z0-9_]*)$/;
const PATHED = /^(?:\/|~)/;
const WRITING = /^[wae]/;
const LINES = /\r\n|\r|\n/;
const SPECIFIER = /^\/document\[@id=(\d+)\](?:\/layer\[@id=(\d+)\])?$/;
const LOGGED = /^\d{4}:\d\d:\d\d \d\d:\d\d:\d\d : (.*?) : (.*?) : (.*)$/;
const REFUSED: readonly string[] = ['bringToFront', 'showColorPicker', 'system', 'togglePalettes', 'batch'];
const PROGRESS: readonly string[] = ['doProgress', 'doForcedProgress', 'doProgressTask', 'doProgressSegmentTask', 'doProgressSubTask'];
const OPTIONS: readonly string[] = ['putCustomOptions', 'eraseCustomOptions'];
const HELD: readonly string[] = ['displayDialogs', 'playbackDisplayDialogs', 'notifiersEnabled'];
const EVENTS: readonly string[] = ['openViewlessDocument', 'systemCall', 'quit'];
const PARTS: readonly (readonly [string, string])[] = [
    ['guide', 'numberOfGuides'],
    ['channel', 'numberOfChannels'],
    ['path', 'numberOfPaths'],
];

// --- [OPERATIONS] ----------------------------------------------------------------------

// --- [HOST]
const id: Host = realGlobal.stringIDToTypeID;

// --- [FILES]
const tail = (path: string, from: number): string => {
    const file = new RealFile(path);
    file.encoding = 'UTF-8';
    if (!(file.exists && file.open('r'))) {
        return '';
    }
    file.seek(from);
    const text = file.read();
    file.close();
    return text;
};
const ensured = (folder: Host): void => {
    if (folder !== null && !folder.exists) {
        ensured(folder.parent);
        folder.create();
    }
};

// --- [DESCRIPTORS]
const parsedJson = (text: string): Host => eval(`(${text})`);
const converted = (value: Host): string => {
    const descriptor: Host = new ActionDescriptor();
    descriptor.putObject(id('object'), id('object'), value);
    return realExecute(id('convertJSONdescriptor'), descriptor, DialogModes.NO).getString(id('json'));
};
const action = (value: Host): Host => {
    const kind = value.typename;
    if (kind === 'ActionDescriptor') {
        return parsedJson(converted(value));
    }
    if (kind !== 'ActionList' && kind !== 'ActionReference') {
        return null;
    }
    const wrapper: Host = new ActionDescriptor();
    if (kind === 'ActionList') {
        wrapper.putList(id('list'), value);
    } else {
        wrapper.putReference(id('null'), value);
    }
    const whole = parsedJson(converted(wrapper));
    const reference = 'null';
    return kind === 'ActionList' ? whole.list : whole[reference];
};

// --- [CODEC]
const enumerator = (value: Host): string | null => {
    const parts: Host = ENUMERATOR.exec(String(value));
    return parts && realGlobal[parts[1]] !== undefined && realGlobal[parts[1]] !== null && realGlobal[parts[1]][parts[2]] == value ? `enum:${parts[1]}.${parts[2]}` : null;
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
    let doc: Host = null;
    for (const candidate of realApp.documents) {
        if (candidate.id === Number(parts[1])) {
            doc = candidate;
            break;
        }
    }
    if (doc === null || parts[2] === undefined || parts[2] === '') {
        return doc;
    }
    const found = (list: Host): Host => {
        for (const layer of list) {
            if (layer.id === Number(parts[2])) {
                return layer;
            }
            if (layer.typename === 'LayerSet') {
                const inner = found(layer.layers);
                if (inner !== null) {
                    return inner;
                }
            }
        }
        return null;
    };
    return found(doc.layers);
};
const coded = codec({
    named: (value: Host) => enumerator(value) ?? specifier(value),
    plain: (value: Host) => (value instanceof UnitValue ? { value: value.value, type: value.type } : action(value)),
    located: resolve,
});

// --- [SHADOWS]
const moved = function (session: Host, Kind: Host, path?: Host): Host {
    const made = arguments.length < 3 ? new Kind() : new Kind(path);
    const name = made.fsName;
    if (made.exists || name === session.folder || name.indexOf(`${session.folder}/`) === 0) {
        return made;
    }
    const inside = new Kind(`${session.folder}/outside${name}`);
    ensured(inside.parent);
    return inside;
};
const rooted = (session: Host, Kind: Host): Host => {
    const Shadow: Host = function (path?: Host): Host {
        return arguments.length > 0 ? moved(session, Kind, path) : moved(session, Kind);
    };
    Shadow.prototype = Kind.prototype;
    statics(Shadow, Kind, {}, false);
    if (Kind === RealFolder) {
        Shadow.watch('current', (_key: Host, _previous: Host, next: Host): Host => {
            RealFolder.current = next;
            return RealFolder.current;
        });
    }
    return Shadow;
};
const acted =
    (session: Host) =>
    (event: Host, descriptor: Host, mode: Host): Host => {
        const name = realGlobal.typeIDToStringID(event);
        if (holds(EVENTS, name)) {
            session.record.activations.push(`executeAction ${name}`);
            return new ActionDescriptor();
        }
        if (mode !== undefined && mode !== DialogModes.NO) {
            session.record.activations.push(`executeAction ${name} ${String(mode)} sent as DialogModes.NO`);
        }
        const result = realExecute(event, descriptor === undefined ? new ActionDescriptor() : descriptor, DialogModes.NO);
        session.refresh();
        return result;
    };
const built = (session: Host): Host => {
    const Shadow: Host = function (): Host {
        const names: Host[] = [];
        for (let i = 0; i < arguments.length - 1; i += 1) {
            names.push(arguments[i]);
        }
        return session.evaluate(`(function (${names.join(',')}) {\n${arguments.length > 0 ? arguments[arguments.length - 1] : ''}\n})`);
    };
    Shadow.prototype = RealFunction.prototype;
    return Shadow;
};
const fakeDocuments = (session: Host, real: Host): Host => {
    const list: Host = { typename: real.typename, parent: real.parent };
    const sync = (): void => {
        for (let i = 0; i < list.length; i += 1) {
            delete list[i];
        }
        list.length = real.length;
        for (let j = 0; j < real.length; j += 1) {
            list[j] = real[j];
        }
    };
    list.add = function (): Host {
        const made = real.add.apply(real, arguments);
        session.refresh();
        return made;
    };
    list.getByName = (name: Host): Host => real.getByName(name);
    list.sync = sync;
    sync();
    return list;
};
const custom = (key: Host): Host => {
    try {
        return realApp.getCustomOptions(key);
    } catch {
        return null;
    }
};
const optioned = (session: Host, method: string) =>
    function (key: Host, _descriptor: Host, persistent: Host): Host {
        if (!Object.prototype.hasOwnProperty.call(session.options, key)) {
            session.options[key] = { prior: custom(key), persistent: false };
        }
        if (persistent === true) {
            session.options[key].persistent = true;
        }
        return realApp[method].apply(realApp, arguments);
    };
const fakeApp = (session: Host, found: Host, executed: Host): Host => {
    const fake: Host = {};
    const writable: string[] = [];
    const documents = fakeDocuments(session, realApp.documents);
    const held = (key: string, previous: Host, next: Host): Host => {
        session.record.writes.push({ member: key, before: coded.kept(previous), after: coded.kept(next) });
        session.record.activations.push(`${key} ${String(next)} held at ${String(previous)}`);
        return previous;
    };
    const forwarded = (key: string, _previous: Host, next: Host): Host => {
        realApp[key] = next;
        session.refresh(key);
        return realApp[key];
    };
    const refresh = (except?: string): void => {
        for (const member of writable) {
            if (member === except) {
                continue;
            }
            fake.unwatch(member);
            try {
                fake[member] = realApp[member];
            } catch {
                delete fake[member];
            }
            fake.watch(member, holds(HELD, member) ? held : forwarded);
        }
        documents.sync();
    };
    const progressing = (name: string) =>
        function (): Host {
            session.record.activations.push(name);
            return session.evaluate(String(arguments[arguments.length - 1]));
        };
    const own: Host = {
        documents,
        executeAction: executed,
        open(): Host {
            const made = realApp.open.apply(realApp, arguments);
            refresh();
            return made;
        },
        openDialog: () => picked(session, 'open', RealFile, true),
        updateProgress: () => true,
        changeProgressText: () => {},
    };
    for (const member of REFUSED) {
        if (Object.prototype.hasOwnProperty.call(found.methods, member)) {
            own[member] = refusing(session, member);
        }
    }
    for (const member of PROGRESS) {
        if (Object.prototype.hasOwnProperty.call(found.methods, member)) {
            own[member] = progressing(member);
        }
    }
    for (const member of OPTIONS) {
        if (Object.prototype.hasOwnProperty.call(found.methods, member)) {
            own[member] = optioned(session, member);
        }
    }
    for (const name of ownKeys(found.properties)) {
        if (name === '__proto__' || Object.prototype.hasOwnProperty.call(own, name)) {
            continue;
        }
        if (found.properties[name] === 'readwrite') {
            writable.push(name);
            continue;
        }
        try {
            fake[name] = realApp[name];
        } catch {}
    }
    for (const method of ownKeys(found.methods)) {
        if (Object.prototype.hasOwnProperty.call(own, method)) {
            continue;
        }
        fake[method] = function (): Host {
            const result = realApp[method].apply(realApp, arguments);
            refresh();
            return result;
        };
    }
    for (const key of ownKeys(own)) {
        fake[key] = own[key];
    }
    session.refresh = refresh;
    refresh();
    return fake;
};
const shadowed = (session: Host, found: Host, item: Host): void => {
    const executed = acted(session);
    const Fake = windows(session, notified);
    const shadows: Host = { Window: Fake, alert: Fake.alert, confirm: Fake.confirm, prompt: Fake.prompt, BridgeTalk: bridgeTalk(session), executeAction: executed, Function: built(session), File: rooted(session, RealFile), Folder: rooted(session, RealFolder) };
    shadows.app = fakeApp(session, found, executed);
    const global = scope(session, item, shadows);
    const names: string[] = [];
    const values: Host[] = [];
    for (const key of ownKeys(shadows)) {
        names.push(key);
        values.push(shadows[key]);
    }
    const scoped = RealFunction.apply(null, names.concat(['doc', 'made', 'harnessCase', 'harnessSource', 'return eval(harnessSource);']));
    let context: Host[] = [undefined, {}, null];
    session.shadows = shadows;
    session.evaluate = function (code: Host, doc: Host, made: Host, held: Host): Host {
        if (arguments.length > 1) {
            context = [doc, made, held];
        }
        return scoped.apply(global, values.concat(context, [code]));
    };
};
const redirected = (session: Host): Host => {
    const opening = function (this: Host, mode: Host): Host {
        const name = this.fsName;
        if (WRITING.test(String(mode)) && name.indexOf(`${session.folder}/`) !== 0) {
            this.changePath(`${session.folder}/outside${name}`);
            ensured(this.parent);
        }
        return realOpen.apply(this, arguments);
    };
    const release = patched(session);
    RealFile.prototype.open = opening;
    return (): void => {
        RealFile.prototype.open = realOpen;
        release();
    };
};

// --- [STATE]
const wholeOf = (descriptor: Host, whole: Host): Host => {
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
    return wholeOf(descriptor, parsedJson(converted(descriptor)));
};
const application = (): Host => {
    const reference: Host = new ActionReference();
    reference.putEnumerated(id('application'), id('ordinal'), id('targetEnum'));
    return described(reference);
};
const treeOf = (doc: Host): Host => {
    const reference: Host = new ActionReference();
    reference.putProperty(id('property'), id('json'));
    reference.putIdentifier(id('document'), doc);
    const descriptor: Host = new ActionDescriptor();
    descriptor.putReference(id('null'), reference);
    descriptor.putBoolean(id('getPathData'), true);
    const answer = realExecute(id('get'), descriptor, DialogModes.NO);
    return wholeOf(answer, parsedJson(answer.getString(id('json'))));
};
const layersOf = (doc: Host): Host[] => {
    const ids: Host[] = [];
    const walk = (list: Host): void => {
        for (const layer of list) {
            ids.push(layer.id);
            if (layer.typename === 'LayerSet') {
                walk(layer.layers);
            }
        }
    };
    walk(doc.layers);
    return ids;
};
const targets = (ids: Host): Host => {
    const rows: Host = {};
    for (const docId of ids) {
        const doc = resolve(`/document[@id=${docId}]`);
        if (doc === null) {
            continue;
        }
        const prefix = `object:/document[@id=${docId}]`;
        const reference: Host = new ActionReference();
        reference.putIdentifier(id('document'), docId);
        rows[prefix] = described(reference);
        rows[`${prefix}/json`] = treeOf(docId);
        for (const row of PARTS) {
            const count = rows[prefix].whole[row[1]] || 0;
            for (let n = 1; n <= count; n += 1) {
                const part: Host = new ActionReference();
                part.putIndex(id(row[0]), n);
                part.putIdentifier(id('document'), docId);
                rows[`${prefix}/${row[0]}[${n}]`] = described(part);
            }
        }
        for (const layerId of layersOf(doc)) {
            const layer: Host = new ActionReference();
            layer.putIdentifier(id('layer'), layerId);
            layer.putIdentifier(id('document'), docId);
            rows[`${prefix}/layer[@id=${layerId}]`] = described(layer);
        }
    }
    return rows;
};
const differing = (before: Host, after: Host, shifting: Host): string[] => {
    const keys: Host = {};
    const rows: string[] = [];
    for (const key of ownKeys(before.text)) {
        keys[key] = true;
    }
    for (const key of ownKeys(after.text)) {
        keys[key] = true;
    }
    for (const key of ownKeys(keys)) {
        if (!Object.prototype.hasOwnProperty.call(shifting, key) && before.text[key] !== after.text[key]) {
            rows.push(key);
        }
    }
    return rows;
};
const changed = (before: Host, after: Host, shifting: Host, target?: Host): Host[] => {
    const rows: Host[] = [];
    for (const key of differing(before, after, shifting)) {
        const row: Host = target === undefined ? {} : { target };
        row.member = key;
        if (Object.prototype.hasOwnProperty.call(before.whole, key)) {
            row.before = before.whole[key];
        }
        row.after = Object.prototype.hasOwnProperty.call(after.whole, key) ? after.whole[key] : { $: 'undefined' };
        rows.push(row);
    }
    return rows;
};
const edited = (before: Host, after: Host, saving: Host): Host[] => {
    let rows: Host[] = [];
    for (const key of ownKeys(before)) {
        rows = rows.concat(Object.prototype.hasOwnProperty.call(after, key) ? changed(before[key], after[key], saving, key) : [{ target: key, member: String(before[key].whole._obj), before: before[key].whole, after: { $: 'undefined' } }]);
    }
    for (const key of ownKeys(after)) {
        if (!Object.prototype.hasOwnProperty.call(before, key)) {
            rows.push({ target: key, member: String(after[key].whole._obj), after: after[key].whole });
        }
    }
    return rows;
};
const restorable = (snapshot: Host, key: string): Host => {
    const typed = key.charAt(0) === '$' ? realGlobal.charIDToTypeID(key.slice(1)) : id(key);
    return snapshot.descriptor.hasKey(typed) && snapshot.descriptor.getType(typed) === DescValueType.OBJECTTYPE ? typed : null;
};
const restored = (initial: Host, shifting: Host): string[] => {
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
            realExecute(id('set'), target, DialogModes.NO);
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
const optionsRestored = (session: Host): void => {
    for (const key of ownKeys(session.options)) {
        const held = session.options[key];
        const after = custom(key);
        const row: Host = { target: 'customOptions', member: key };
        if (held.prior !== null) {
            row.before = action(held.prior);
        }
        row.after = after === null ? { $: 'undefined' } : action(after);
        session.record.writes.push(row);
        if (held.prior !== null) {
            realApp.putCustomOptions(key, held.prior, held.persistent);
        } else if (after !== null) {
            realApp.eraseCustomOptions(key);
        }
    }
};
const open = (): Host => {
    const ids: Host = {};
    for (const doc of realApp.documents) {
        ids[doc.id] = doc;
    }
    return ids;
};
const stackFrames = (stack: Host): string[] => {
    const frames: string[] = [];
    for (const offsets of stack.offsets) {
        for (const module of ownKeys(offsets)) {
            for (const offset of offsets[module]) {
                frames.push(`${module} +0x${offset.toString(16)}`);
            }
        }
    }
    return frames;
};
const logged = (text: string): Host[] => {
    const entries: Host[] = [];
    let entry: Host = null;
    for (const line of text.split(LINES)) {
        const parts: Host = LOGGED.exec(line);
        if (!parts) {
            continue;
        }
        if (parts[1] === 'Version') {
            entry = { version: parts[3], id: '', code: '', message: '', frames: [] };
            entries.push(entry);
        } else if (entry !== null && parts[1] === 'Stack') {
            entry.frames = entry.frames.concat(stackFrames(parsedJson(parts[3])));
        } else if (entry !== null) {
            entry.id = parts[1];
            entry.code = parts[2];
            entry.message = parts[3];
        }
    }
    return entries;
};

// --- [PRECONDITION]
const evaluated = (session: Host, expression: Host, doc: Host, made: Host, statement?: boolean): Host => session.evaluate(statement ? expression : `(${expression}\n)`, doc, made, null);
const agrees = (read: Host, asked: Host): boolean => {
    if (typeof asked === 'number') {
        return Math.abs(Number(read) - asked) * 65_536 <= Math.max(1, Math.abs(asked));
    }
    const parts: Host = ENUMERATOR.exec(String(asked));
    return parts ? String(read).split('.').pop() === parts[2] : String(read) === String(asked);
};
const newDocument = (row: Host, name: string, at: number): Host => {
    const asked = coded.revive(row.arguments);
    const doc = realApp.documents.add(asked.width, asked.height, asked.resolution, name, asked.mode, asked.initialFill, asked.pixelAspectRatio, asked.bitsPerChannel, asked.colorProfileName);
    for (const key of ownKeys(asked)) {
        if (key in doc && !agrees(doc[key], asked[key])) {
            throw new Error(`documents[${at}] ${key} reads ${String(doc[key])}, asked ${String(asked[key])}`);
        }
    }
    return doc;
};
const opened = (item: Host, batch: Host, index: number): Host[] => {
    const condition = item.precondition;
    const docs: Host[] = [];
    const flags = condition.features || {};
    const fonts = condition.fonts || [];
    const missing: string[] = [];
    for (const flag of ownKeys(flags)) {
        if (realApp.featureEnabled(flag) !== flags[flag]) {
            throw new Error(`feature ${flag} reads ${!flags[flag]}`);
        }
    }
    for (const font of fonts) {
        try {
            realApp.fonts.getByName(font);
        } catch {
            missing.push(font);
        }
    }
    if (missing.length > 0) {
        throw new Error(`fonts not installed: ${missing.join(', ')}`);
    }
    for (let i = 0; i < condition.documents.length; i += 1) {
        const row = condition.documents[i];
        docs.push(row._tag === 'Opened' ? realApp.open(new RealFile(row.open)) : newDocument(row, `${batch.label}-${index}-${i}`, i));
    }
    if (docs.length > 1) {
        realApp.activeDocument = docs[0];
    }
    return docs;
};
const selected = (doc: Host, chosen: Host, evaluate: (expression: Host) => Host): void => {
    const layers = chosen.layers || [];
    if (layers.length > 0) {
        const reference: Host = new ActionReference();
        for (const layer of layers) {
            reference.putIdentifier(id('layer'), evaluate(layer).id);
        }
        const target: Host = new ActionDescriptor();
        target.putReference(id('null'), reference);
        target.putBoolean(id('makeVisible'), false);
        realExecute(id('select'), target, DialogModes.NO);
    }
    if (chosen.region !== undefined) {
        doc.selection.select(chosen.region);
    }
};
const prepare = (session: Host, doc: Host, condition: Host, made: Host): void => {
    const evaluate = (expression: Host): Host => evaluated(session, expression, doc, made);
    prepared([
        [
            'set',
            condition.set,
            (row: Host) => {
                assign(row[0], row[1], evaluate, coded.revive);
            },
        ],
        [
            'add',
            condition.add,
            (row: Host) => {
                const item = evaluate(row.to)[row.kind].add();
                const properties = coded.revive(row.properties || {});
                for (const key of ownKeys(properties)) {
                    item[key] = properties[key];
                }
                made[row.as] = item;
            },
        ],
        [
            'steps',
            condition.steps,
            (step: Host) => {
                evaluated(session, step, doc, made, true);
            },
        ],
        [
            'select',
            condition.select === undefined ? [] : [condition.select],
            (chosen: Host) => {
                selected(doc, chosen, evaluate);
            },
        ],
    ]);
    session.refresh();
};
const histories = (docs: Host): Host => {
    const counts: Host = {};
    for (const doc of docs) {
        counts[doc.id] = doc.historyStates.length;
    }
    return counts;
};
const added = (counts: Host): string[] => {
    const names: string[] = [];
    for (const key of ownKeys(counts)) {
        const doc = resolve(`/document[@id=${key}]`);
        if (doc === null) {
            continue;
        }
        for (let i = doc.historyStates.length - 1; i >= counts[key]; i -= 1) {
            names.push(doc.historyStates[i].name);
        }
    }
    return names;
};

// --- [RUN]
const located = (error: Host, item: Host): Host => {
    const line = 'line';
    const completion = thrown(error);
    const file = error !== null && typeof error === 'object' && typeof error.fileName === 'string' && PATHED.test(error.fileName) ? new RealFile(error.fileName).fsName : '';
    const loaded = item._tag !== 'Probe' && item.source !== undefined ? item.source.script : '';
    const at = file !== '' && file !== ENGINE ? file : loaded;
    if (at === '') {
        delete completion[line];
    } else if (Object.prototype.hasOwnProperty.call(completion, line)) {
        completion.file = at;
    }
    return completion;
};
const ran = (session: Host, item: Host, docs: Host, made: Host, result: Host): Host => {
    const folder = RealFolder.current;
    try {
        let value: Host;
        if (item._tag === 'Probe') {
            value = evaluated(session, item.evaluate, docs[0], made);
        } else {
            const script = item.source === undefined ? null : new RealFile(item.source.script);
            if (script !== null && !script.exists) {
                throw new Error(`no script ${item.source.script}`);
            }
            if (script !== null) {
                session.shadows.Folder.current = script.parent;
            }
            const code = (script === null ? '' : item.source.code) + (item._tag === 'Call' ? `\n;(${item.call}\n).apply(undefined, harnessCase.args)` : '');
            value = session.evaluate(code, docs[0], made, { script, args: coded.revive(item._tag === 'Call' ? item.args : []) });
        }
        palettes(session);
        if (item._tag !== 'Run') {
            result.value = coded.kept(value);
        }
        return { _tag: 'Ran' };
    } catch (error) {
        return located(error, item);
    } finally {
        session.shadows.Folder.current = folder;
    }
};
const outcome = (item: Host, batch: Host, index: number, found: Host, shift: Host): Host => {
    const session = sessionOf(item, ['history', 'errors']);
    const result = started(item);
    let leaked: string[] = [];
    const known = open();
    const held = { ruler: realApp.preferences.rulerUnits, type: realApp.preferences.typeUnits, colorSettings: realApp.colorSettings };
    const log = new RealFile(`${realApp.preferencesFolder.fsName}/PSErrorLog.txt`);
    const offset = Math.max(log.length, 0);
    const global = globals();
    const initial = application();
    let release: () => void = () => {};
    const made: Host = {};
    session.refresh = () => {};
    session.options = {};
    session.folder = new RealFolder(item.folder).fsName;
    try {
        shadowed(session, found, item);
        realApp.displayDialogs = DialogModes.NO;
        realApp.preferences.rulerUnits = coded.revive(item.precondition.units.ruler);
        realApp.preferences.typeUnits = coded.revive(item.precondition.units.type);
        if (item.precondition.colorSettings !== undefined) {
            realApp.colorSettings = item.precondition.colorSettings;
        }
        const docs = opened(item, batch, index);
        const evaluate = (expression: Host): Host => evaluated(session, expression, docs[0], made);
        session.refresh();
        prepare(session, docs[0], item.precondition, made);
        const reading = reads(item.read, evaluate, coded.kept);
        result.before = reading.state;
        result.beforeFailed = reading.failed;
        const ids: Host[] = [];
        for (const doc of docs) {
            ids.push(doc.id);
        }
        const before = { application: application(), targets: targets(ids), history: histories(docs) };
        for (const docId of ids) {
            const prefix = `object:/document[@id=${docId}]`;
            const layers: Host[] = [];
            const parts: Host = {};
            for (const key of ownKeys(before.targets)) {
                if (key.indexOf(`${prefix}/`) !== 0) {
                    continue;
                }
                if (key.indexOf(`${prefix}/layer[`) === 0) {
                    layers.push(before.targets[key].whole);
                } else {
                    parts[key.slice(prefix.length + 1)] = before.targets[key].whole;
                }
            }
            session.record.opened.push({ document: before.targets[prefix].whole, layers, parts });
        }
        release = redirected(session);
        result.completion = ran(session, item, docs, made, result);
        attempt(leaked, 'shadows', release);
        release = () => {};
        const after = reads(item.read, evaluate, coded.kept);
        result.read = after.state;
        result.readFailed = after.failed;
        session.record.writes = session.record.writes.concat(changed(before.application, application(), shift.shifting), edited(before.targets, targets(ids), shift.saving));
        session.record.history = added(before.history);
    } catch (outer) {
        result.completion = { _tag: 'Failed', reason: failed(outer) };
    }
    attempt(leaked, 'shadows', release);
    for (const shown of session.created) {
        attempt(leaked, 'window', () => {
            shown.close();
        });
    }
    attempt(leaked, 'documents', () => {
        const now = open();
        for (const key of ownKeys(now)) {
            if (!Object.prototype.hasOwnProperty.call(known, key)) {
                now[key].close(SaveOptions.DONOTSAVECHANGES);
            }
        }
    });
    attempt(leaked, 'customOptions', () => {
        optionsRestored(session);
    });
    attempt(leaked, 'preferences', () => {
        realApp.displayDialogs = DialogModes.NO;
        realApp.preferences.rulerUnits = held.ruler;
        realApp.preferences.typeUnits = held.type;
        if (realApp.colorSettings !== held.colorSettings) {
            realApp.colorSettings = held.colorSettings;
        }
    });
    attempt(leaked, 'globals', () => {
        leaked = leaked.concat(released(global));
    });
    attempt(leaked, 'application', () => {
        leaked = leaked.concat(restored(initial, shift.shifting));
    });
    attempt(leaked, 'errors', () => {
        session.record.errors = logged(tail(log.fsName, offset));
    });
    session.record.writes = coded.kept(session.record.writes);
    return finished(result, session.record, leaked);
};
const keysOf = (list: Host): Host => {
    const keys: Host = {};
    for (const key of list) {
        keys[key] = true;
    }
    return keys;
};
const shifted = (label: string, folder: string): Host => {
    const first = application();
    const doc = realApp.documents.add(new UnitValue(1, 'px'), new UnitValue(1, 'px'), 72, `${label}-shift`);
    const start = targets([doc.id]);
    doc.saveAs(new RealFile(`${folder}/${label}-shift.psd`), new PhotoshopSaveOptions(), false);
    const saved = targets([doc.id]);
    doc.close(SaveOptions.DONOTSAVECHANGES);
    const second = application();
    let saving: string[] = [];
    for (const key of ownKeys(start)) {
        if (Object.prototype.hasOwnProperty.call(saved, key)) {
            saving = saving.concat(differing(start[key], saved[key], keysOf(saving)));
        }
    }
    return { keys: differing(first, second, {}), saving, defaults: second.whole };
};
const unitOf = (value: Host): string => {
    const parts: Host = ENUMERATOR.exec(String(value));
    return parts[1];
};
const survey = (shift: Host): Host => ({ units: { ruler: unitOf(realApp.preferences.rulerUnits), type: unitOf(realApp.preferences.typeUnits) }, host: { locale: $.locale, featureAccessLevel: String(shift.defaults.featureAccessLevel), colorSettings: String(realApp.colorSettings) } });
const declared = (list: Host): Host => {
    const names: Host = {};
    for (const row of list) {
        names[row.name] = row.type;
    }
    return names;
};
const quitting = (): Host => {
    const closed = realApp.documents.length;
    for (let i = closed - 1; i >= 0; i -= 1) {
        realApp.documents[i].close(SaveOptions.DONOTSAVECHANGES);
    }
    realExecute(id('quit'), new ActionDescriptor(), DialogModes.NO);
    return { closed };
};

// --- [COMPOSITION] ---------------------------------------------------------------------

const run = (text: string): string => {
    const dialogs = realApp.displayDialogs;
    realApp.displayDialogs = DialogModes.NO;
    try {
        const batch = parsedJson(text);
        if (batch.quit === true) {
            return coded.json(quitting());
        }
        const shift = shifted(batch.label, new RealFile(batch.records).parent.fsName);
        const defaults: Host = {};
        const shifting = keysOf(shift.keys);
        const moving = { shifting, saving: keysOf(shift.saving) };
        for (const key of ownKeys(shift.defaults)) {
            if (key !== '_obj' && !Object.prototype.hasOwnProperty.call(shifting, key)) {
                defaults[key] = shift.defaults[key];
            }
        }
        const found = { properties: declared(realApp.reflect.properties), methods: declared(realApp.reflect.methods) };
        const unlisted: string[] = [];
        for (const member of REFUSED.concat(PROGRESS, OPTIONS)) {
            if (!Object.prototype.hasOwnProperty.call(found.methods, member)) {
                unlisted.push(member);
            }
        }
        const answer: Host = { application: realApp.name, version: realApp.version, state: { dialogs: String(dialogs), shifting: shift.keys, saving: shift.saving, unlisted, defaults } };
        if (batch.survey === true) {
            answer.survey = survey(shift);
        }
        batched(batch, coded, (item: Host, index: number) => outcome(item, batch, index, found, moving));
        return coded.json(answer);
    } catch (error) {
        return coded.json({ failed: failed(error) });
    } finally {
        realApp.displayDialogs = dialogs;
    }
};

// --- [EXPORTS] -------------------------------------------------------------------------

export { run };
