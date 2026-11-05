import { ownKeys } from '@rasm/extendscript/extendscript';
import { assign, attempt, batched, bridgeTalk, codec, failed, finished, globals, type Host, holds, messageOf, notified, palettes, patched, prepared, read, reads, refusing, released, scope, sessionOf, started, statics, thrown, windows } from '#/hosts/adobe/extendscript';

// --- [CONSTANTS] -----------------------------------------------------------------------

const realApp: Host = app;
const realGlobal: Host = $.global;
const RealFile: Host = File;
const RealFolder: Host = Folder;
const ENUMERATOR = /^([A-Za-z_$][\w$]*)\.([A-Za-z_$][\w$]*)$/;
const REFERENCE = /^([A-Za-z]+):(.+)$/;
const PREFERENCE = /^set(\w+)Preference$/;
const ACTION_SET = /\/name\s*\[\s*\d+\s+([0-9A-Fa-f]+)\s*\]/;
const EXTENSION = /\.[^.]*$/;
const LINE_BREAK = /\r?\n/;
const LIVE: readonly string[] = ['activeDocument', 'selection'];
const homes: Host = {};

// --- [OPERATIONS] ----------------------------------------------------------------------

// --- [FILES]
const stem = (file: Host): string => decodeURI(file.name).replace(EXTENSION, '');
const offsets = (logs: Host): Host => {
    const at: Host = {};
    for (const log of logs) {
        const file = new RealFile(log);
        at[log] = file.exists ? file.length : 0;
    }
    return at;
};
const logged = (logs: Host, from: Host): Host[] => {
    const rows: Host[] = [];
    for (const log of logs) {
        const file = new RealFile(log);
        if (!file.exists || file.length <= from[log]) {
            continue;
        }
        file.encoding = 'UTF-8';
        if (!file.open('r')) {
            continue;
        }
        file.seek(from[log], 0);
        const lines = file.read().split(LINE_BREAK);
        file.close();
        for (const line of lines) {
            if (line !== '') {
                rows.push({ file: decodeURI(file.name), line });
            }
        }
    }
    return rows;
};

// --- [CODEC]
const enumerator = (value: Host): Host => {
    const parts: Host = ENUMERATOR.exec(String(value));
    if (!parts) {
        return null;
    }
    const owner = realGlobal[parts[1]];
    try {
        return owner !== undefined && owner !== null && owner[parts[2]] === value ? `enum:${parts[1]}.${parts[2]}` : null;
    } catch {
        return null;
    }
};
const kindOf = (value: Host): Host => {
    try {
        return typeof value.typename === 'string' ? value.typename : null;
    } catch {
        return null;
    }
};
const reflects = (value: Host, name: string): boolean => {
    try {
        const found = value.reflect.find.call(value.reflect, name);
        return Boolean(found);
    } catch {
        return false;
    }
};
const collected = (value: Host): boolean => {
    try {
        return typeof value.getByName === 'function' && typeof value.length === 'number';
    } catch {
        return false;
    }
};
const classed = (info: Host): boolean => {
    const first = info.dataType.charAt(0);
    return first !== first.toLowerCase() && info.dataType !== 'Object';
};
const home = (value: Host, above: string, kind: string): Host => {
    const { parent } = value;
    const known = homes[`${above}>${kind}`];
    if (known !== undefined) {
        return known;
    }
    for (const property of parent.reflect.properties) {
        if (!classed(property)) {
            continue;
        }
        try {
            const held = parent[property.name];
            if (collected(held) && held.getByName(value.name).typename === kind) {
                homes[`${above}>${kind}`] = property.name;
                return property.name;
            }
        } catch {}
    }
    return null;
};
const anchored: Host = { Application: () => 'app', Document: (value: Host) => `Document:${value.name}` };
const specifier = (value: Host): Host => {
    const kind = kindOf(value);
    if (kind === null) {
        return null;
    }
    if (anchored.hasOwnProperty(kind)) {
        return anchored[kind](value);
    }
    let uuid: Host;
    try {
        ({ uuid } = value);
    } catch {}
    if (uuid !== undefined) {
        return `${kind}:${uuid}`;
    }
    if (!(reflects(value, 'parent') && reflects(value, 'name'))) {
        return null;
    }
    const above = specifier(value.parent);
    const collection = above === null ? null : home(value, above, kind);
    return collection === null ? null : `${kind}:${above}/${collection}/${encodeURIComponent(value.name)}`;
};
const expanded = (value: Host, path: Host): Host => {
    const row: Host = { typename: kindOf(value) };
    const below = path.concat([row.typename]);
    for (const property of value.reflect.properties) {
        const { name } = property;
        if (name === '__proto__' || name === 'parent' || name === 'typename') {
            continue;
        }
        let held: Host;
        try {
            held = value[name];
        } catch {
            continue;
        }
        if (!collected(held)) {
            row[name] = coded.kept(held, below);
        }
    }
    return row;
};
const hosted = (value: Host, path: Host): Host => {
    if (value instanceof RealFile || value instanceof RealFolder) {
        const held: Host = value;
        return held.fsName;
    }
    if (collected(value)) {
        const listed: Host[] = [];
        for (const entry of value) {
            listed.push(coded.kept(entry, path));
        }
        return listed;
    }
    const held = specifier(value);
    if (held !== null) {
        return { $: `object:${held}` };
    }
    const kind = kindOf(value);
    return kind !== null && !holds(path, kind) ? expanded(value, path) : null;
};
const located = (reference: string): Host => {
    if (reference === 'app') {
        return realApp;
    }
    const parts: Host = REFERENCE.exec(reference);
    if (!parts) {
        throw new Error(`object:${reference} reads neither Document:<name>, <typename>:<uuid>, nor <typename>:<container>/<collection>/<name>`);
    }
    if (parts[1] === 'Document') {
        return realApp.documents.getByName(parts[2]);
    }
    const at = parts[2].lastIndexOf('/');
    if (at >= 0) {
        const head = parts[2].slice(0, at);
        const from = head.lastIndexOf('/');
        return located(head.slice(0, from))[head.slice(from + 1)].getByName(decodeURIComponent(parts[2].slice(at + 1)));
    }
    for (const open of realApp.documents) {
        try {
            return open.getPageItemFromUuid(parts[2]);
        } catch {}
    }
    throw new Error(`object:${reference} names no item of an open document`);
};
const coded = codec({ named: enumerator, plain: hosted, located, constructed: true });

// --- [PREFERENCES]
const kinds = (): string[] => {
    const found: string[] = [];
    for (const method of realApp.preferences.reflect.methods) {
        const parts: Host = PREFERENCE.exec(method.name);
        if (parts) {
            found.push(parts[1]);
        }
    }
    return found;
};
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
    for (const kind of kinds()) {
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

// --- [STATE]
const members = (owner: Host, path: string, kinds: Host, rows: Host, unread: string[]): Host => {
    for (const property of owner.reflect.properties) {
        const { name } = property;
        if (name === '__proto__' || name === 'parent' || name === 'typename' || (path === '' && holds(LIVE, name))) {
            continue;
        }
        let value: Host;
        try {
            value = owner[name];
        } catch (error) {
            unread.push(`app.${path}${name}: ${messageOf(error)}`);
            continue;
        }
        if (property.type === 'readwrite') {
            rows.push({ path, key: name, text: coded.json(value) });
        } else if (classed(property) && kindOf(value) !== null && !collected(value) && !holds(kinds, kindOf(value))) {
            members(value, `${path}${name}.`, kinds.concat([kindOf(value)]), rows, unread);
        }
    }
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
const restored = (rows: Host): string[] => {
    const leaked: string[] = [];
    for (const change of differences(rows)) {
        try {
            ownerAt(change.row.path)[change.row.key] = coded.revive(change.before);
        } catch (error) {
            leaked.push(`app.${change.member}: ${messageOf(error)}`);
        }
    }
    for (const left of differences(rows)) {
        leaked.push(`app.${left.member} reads ${coded.json(left.after)}`);
    }
    return leaked;
};
const keyOf = (doc: Host): string => String(doc.fullName);
const keys = (): string[] => {
    const list: string[] = [];
    for (const doc of realApp.documents) {
        list.push(keyOf(doc));
    }
    return list;
};

// --- [APP]
const fakeDocuments = (session: Host, refresh: () => void): Host => {
    const real = realApp.documents;
    const made = (doc: Host): Host => {
        session.documents.push(doc);
        refresh();
        return doc;
    };
    const list: Host = {
        typename: real.typename,
        parent: real.parent,
        add(): Host {
            return made(real.add.apply(real, arguments));
        },
        addDocument: (preset: Host, settings: Host): Host => made(real.addDocument(preset, settings, false)),
        addDocumentWithDialogOption: (preset: Host): Host => made(real.addDocumentWithDialogOption(preset, false)),
        getByName: (name: Host): Host => real.getByName(name),
        index: (doc: Host): Host => real.index(doc),
        sync: (): void => {
            for (let i = 0; i < list.length; i += 1) {
                delete list[i];
            }
            list.length = real.length;
            for (let j = 0; j < real.length; j += 1) {
                list[j] = real[j];
            }
        },
    };
    list.sync();
    return list;
};
const sent = (session: Host, name: string, refresh: () => void): Host => {
    const forward = (args: Host): Host => {
        const result = realApp[name].apply(realApp, args);
        refresh();
        return result;
    };
    const rows: Host = {
        loadAction(file: Host): Host {
            const result = forward(arguments);
            const parts: Host = ACTION_SET.exec(read(file instanceof RealFile ? file.fsName : String(file)));
            if (parts) {
                session.loaded.push(decodeURIComponent(parts[1].replace(/(..)/g, '%$1')));
            }
            return result;
        },
        selectTool(): Host {
            if (session.tool === undefined) {
                session.tool = realApp.getSelectedToolName();
            }
            return forward(arguments);
        },
        copy(): Host {
            session.record.writes.push({ _tag: 'Pasteboard', member: name });
            return forward(arguments);
        },
        cut(): Host {
            session.record.writes.push({ _tag: 'Pasteboard', member: name });
            return forward(arguments);
        },
    };
    return rows.hasOwnProperty(name)
        ? rows[name]
        : function (): Host {
              return forward(arguments);
          };
};
const fakeApp = (session: Host, item: Host): Host => {
    const fake: Host = {};
    const writable: string[] = [];
    const forwarded = (key: string, _previous: Host, next: Host): Host => {
        realApp[key] = next;
        return realApp[key];
    };
    const refresh = (): void => {
        for (const member of writable) {
            fake.unwatch(member);
            try {
                fake[member] = realApp[member];
            } catch {
                delete fake[member];
            }
            fake.watch(member, forwarded);
        }
        documents.sync();
    };
    const documents: Host = fakeDocuments(session, refresh);
    const own: Host = {
        documents,
        preferences: preferences(session),
        open(file: Host, colorSpace: Host, _options: Host): Host {
            const made = realApp.open.apply(realApp, arguments.length < 3 ? [file, colorSpace, unlisted({})] : arguments);
            session.documents.push(made);
            refresh();
            return made;
        },
    };
    const sends = item.precondition.sends || [];
    for (const property of realApp.reflect.properties) {
        const { name } = property;
        if (name === '__proto__' || own.hasOwnProperty(name)) {
            continue;
        }
        if (property.type === 'readwrite') {
            writable.push(name);
        } else {
            try {
                fake[name] = realApp[name];
            } catch {}
        }
    }
    for (const method of realApp.reflect.methods) {
        const { name } = method;
        if (!own.hasOwnProperty(name)) {
            own[name] = holds(sends, name) ? sent(session, name, refresh) : refusing(session, name);
        }
    }
    for (const key of ownKeys(own)) {
        fake[key] = own[key];
    }
    refresh();
    session.refresh = refresh;
    return fake;
};
const shadowsFor = (session: Host, item: Host): Host => {
    const Fake = windows(session, notified);
    const shadows: Host = { Window: Fake, alert: Fake.alert, confirm: Fake.confirm, prompt: Fake.prompt, app: fakeApp(session, item), BridgeTalk: bridgeTalk(session) };
    shadows.global = scope(session, item, shadows);
    return shadows;
};

// --- [PRECONDITION]
const scoped: Host = eval('(function (Window, alert, confirm, prompt, app, $, BridgeTalk, self, source, extra) { return (function () { return eval(source); }).apply(self, extra); })');
const evaluated = (session: Host, expression: string, doc: Host, made: Host, statement?: boolean): Host => session.evaluate(`(function (doc, made) { ${statement ? expression : `return (${expression});`}\n}).apply(this, arguments);`, [doc, made]);
const added = (opened: Host, before: number, label: string): Host => {
    if (opened === null || opened === undefined || opened.typename !== 'Document' || realApp.documents.length <= before) {
        throw new Error(`${label} left documents.length at ${realApp.documents.length}`);
    }
    return opened;
};
const unlisted = (rows: Host): Host => {
    const options: Host = new OpenOptions();
    options.addToRecentFiles = false;
    const given = coded.revive(rows);
    for (const key of ownKeys(given)) {
        options[key] = given[key];
    }
    return options;
};
const created = (session: Host, row: Host, title: string, made: Host): Host => {
    const preset = realApp.getPresetSettings(row.preset);
    const settings = coded.revive(row.settings);
    for (const key of ownKeys(settings)) {
        preset[key] = settings[key];
    }
    preset.title = title;
    const before = realApp.documents.length;
    const doc = added(realApp.documents.addDocument(row.preset, preset, false), before, `addDocument ${row.preset}`);
    const evaluate = (expression: string): Host => evaluated(session, expression, doc, made);
    session.documents.push(doc);
    for (const write of row.writes) {
        assign(write[0], write[1], evaluate, coded.revive);
    }
    const wrong: string[] = [];
    for (const expected of row.expects) {
        const now = coded.json(evaluate(expected[0]));
        const wanted = coded.json(coded.revive(expected[1]));
        if (now !== wanted) {
            wrong.push(`${title} ${expected[0]} reads ${now} where the precondition asks ${wanted}`);
        }
    }
    if (wrong.length > 0) {
        throw new Error(wrong.join(', '));
    }
    return doc;
};
const opened = (session: Host, item: Host, batch: Host, index: number, made: Host): Host[] => {
    const condition = item.precondition;
    const docs: Host[] = [];
    for (let i = 0; i < condition.documents.length; i += 1) {
        const row = condition.documents[i];
        const before = realApp.documents.length;
        if (row.open === undefined) {
            docs.push(created(session, row, `${batch.label}-${index}-${i}`, made));
        } else {
            docs.push(added(realApp.open(new RealFile(row.open), row.colorSpace === undefined ? undefined : coded.revive(row.colorSpace), unlisted(row.options)), before, `open ${row.open}`));
            session.documents.push(docs[i]);
        }
    }
    const missing: string[] = [];
    for (const font of condition.fonts || []) {
        try {
            realApp.textFonts.getByName(font);
        } catch {
            missing.push(font);
        }
    }
    if (missing.length > 0) {
        throw new Error(`fonts not installed: ${missing.join(', ')}`);
    }
    if (docs.length > 1) {
        realApp.activeDocument = docs[0];
    }
    return docs;
};
const prepare = (session: Host, doc: Host, condition: Host, made: Host): void => {
    const evaluate = (expression: string): Host => evaluated(session, expression, doc, made);
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
                const collection = evaluate(row.to)[row.kind];
                const method = row.method === undefined ? 'add' : row.method;
                if (typeof collection[method] !== 'function') {
                    throw new Error(`${row.to}.${row.kind} holds no method ${method}`);
                }
                const args: Host[] = [];
                for (const argument of row.args || []) {
                    args.push(evaluate(argument));
                }
                const item = collection[method].apply(collection, args);
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
            (step: Host): void => {
                evaluated(session, step, doc, made, true);
            },
        ],
        [
            'select',
            condition.select,
            (pick: Host, index: number): void => {
                const picked = evaluate(pick);
                if (reflects(picked, 'select')) {
                    picked.select(index > 0);
                } else {
                    picked.selected = true;
                }
            },
        ],
    ]);
    session.refresh();
};

// --- [RUN]
const body = (item: Host): string => {
    if (item._tag === 'Probe') {
        return `(function (doc, made) { return (${item.evaluate}); }).apply(this, arguments);`;
    }
    if (item._tag === 'Call') {
        return `${item.source === undefined ? '' : item.source.code}\n;(${item.call}).apply(this, arguments);`;
    }
    return item.source.code;
};
const sessionFor = (item: Host): Host => {
    const fresh = sessionOf(item, []);
    fresh.loaded = [];
    fresh.preferences = {};
    return fresh;
};
const ran = (session: Host, item: Host, extra: Host, result: Host): Host => {
    try {
        const value = session.evaluate(body(item), extra);
        palettes(session);
        if (item._tag !== 'Run') {
            result.value = coded.kept(value, []);
        }
        return { _tag: 'Ran' };
    } catch (error) {
        return thrown(error);
    }
};
const outcome = (item: Host, batch: Host, index: number): Host => {
    const session = sessionFor(item);
    const result = started(item);
    let leaked: string[] = [];
    const known = keys();
    const global = globals();
    let release: () => void = () => {};
    const made: Host = {};
    const base = snapshot();
    let start = base;
    const from = offsets(batch.logs);
    let docs: Host[] = [];
    try {
        const shadows = shadowsFor(session, item);
        session.app = shadows.app;
        session.evaluate = (code: Host, extra: Host): Host => scoped(shadows.Window, shadows.alert, shadows.confirm, shadows.prompt, shadows.app, shadows.$, shadows.BridgeTalk, shadows.global, code, extra);
        release = patched(session);
        realApp.coordinateSystem = coded.revive(item.precondition.coordinates);
        realApp.userInteractionLevel = coded.revive(item.precondition.interaction);
        if (item.precondition.colorSettings !== undefined) {
            session.colored = true;
            realApp.loadColorSettings(new RealFile(item.precondition.colorSettings));
        }
        for (const row of item.precondition.preferences || []) {
            remember(session.preferences, row.key, row.kind, false);
            realApp.preferences[`set${row.kind}Preference`](row.key, row.value);
        }
        docs = opened(session, item, batch, index, made);
        for (const doc of docs) {
            session.record.opened.push(expanded(doc, []));
        }
        session.refresh();
        if (docs.length > 0) {
            prepare(session, docs[0], item.precondition, made);
        }
        start = snapshot();
        const evaluate = (expression: string): Host => evaluated(session, expression, docs[0], made);
        const before = reads(item.read, evaluate, coded.kept);
        result.before = before.state;
        result.beforeFailed = before.failed;
        result.completion = ran(session, item, item._tag === 'Probe' ? [docs[0], made] : coded.revive(item.args), result);
        const after = reads(item.read, evaluate, coded.kept);
        result.read = after.state;
        result.readFailed = after.failed;
    } catch (outer) {
        result.completion = { _tag: 'Failed', reason: failed(outer) };
    }
    attempt(leaked, 'prototypes', release);
    attempt(leaked, 'globals', () => {
        leaked = leaked.concat(released(global));
    });
    for (const shown of session.created) {
        attempt(leaked, 'window', () => {
            shown.close();
        });
    }
    for (const set of session.loaded) {
        attempt(leaked, 'action', () => {
            realApp.unloadAction(set, '');
        });
    }
    if (session.tool !== undefined) {
        attempt(leaked, 'tool', () => {
            realApp.selectTool(session.tool);
        });
    }
    if (session.colored === true) {
        attempt(leaked, 'color settings', () => {
            realApp.loadColorSettings(new RealFile(batch.colorSettings));
        });
    }
    const mine: string[] = [];
    for (const open of session.documents) {
        attempt(leaked, 'close', () => {
            mine.push(keyOf(open));
            if (holds(keys(), keyOf(open))) {
                open.close(SaveOptions.DONOTSAVECHANGES);
            }
        });
    }
    attempt(leaked, 'documents', () => {
        for (let o = realApp.documents.length - 1; o >= 0; o -= 1) {
            const doc = realApp.documents[o];
            if (!(holds(known, keyOf(doc)) || holds(mine, keyOf(doc)))) {
                leaked.push(`document ${doc.name} opened outside the case`);
                doc.close(SaveOptions.DONOTSAVECHANGES);
            }
        }
    });
    attempt(leaked, 'preferences', () => {
        const rows = settled(session.preferences);
        session.record.writes = session.record.writes.concat(rows.writes);
        leaked = leaked.concat(rows.leaked);
    });
    attempt(leaked, 'application', () => {
        for (const change of differences(start)) {
            session.record.writes.push({ _tag: 'Application', member: change.member, before: change.before, after: change.after });
        }
        leaked = leaked.concat(restored(base));
    });
    session.record.writes = coded.kept(session.record.writes, []);
    session.record.logs = logged(batch.logs, from);
    return finished(result, session.record, leaked);
};
const closing = (folders: Host, label?: string): void => {
    for (let i = realApp.documents.length - 1; i >= 0; i -= 1) {
        const doc = realApp.documents[i];
        let stale: boolean = label !== undefined && doc.name.indexOf(`${label}-`) === 0;
        for (let f = 0; f < folders.length && !stale; f += 1) {
            stale = keyOf(doc).indexOf(`${new RealFolder(folders[f]).fullName}/`) === 0;
        }
        if (stale) {
            doc.close(SaveOptions.DONOTSAVECHANGES);
        }
    }
};
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
        const name = file === null || file === undefined ? '' : stem(file);
        if (presets.hasOwnProperty(name)) {
            presets[name].types.push(type);
        }
    }
    for (const property of realApp.reflect.properties) {
        const { name } = property;
        if (property.type !== 'readonly' || name === 'typename' || name === 'parent') {
            continue;
        }
        let value: Host;
        try {
            value = realApp[name];
        } catch {
            continue;
        }
        if (kindOf(value) === null && !collected(value)) {
            application[name] = coded.kept(value, []);
        }
    }
    for (const method of realApp.reflect.methods) {
        methods.push(method.name);
    }
    return { presets, application, methods, options: expanded(new OpenOptions(), []) };
};

// --- [COMPOSITION] ---------------------------------------------------------------------

const run = (text: string): string => {
    const batch: Host = eval(`(${text})`);
    const level = realApp.userInteractionLevel;
    const unread: string[] = [];
    const answer: Host = { application: realApp.name, version: realApp.version, state: { documents: realApp.documents.length, application: plain(snapshot(unread)), unread } };
    realApp.userInteractionLevel = UserInteractionLevel.DONTDISPLAYALERTS;
    try {
        if (batch._tag === 'Survey') {
            answer.survey = surveyed(batch.enumerations.DocumentPresetType || []);
            return coded.json(answer);
        }
        if (batch._tag === 'Closed') {
            closing(batch.folders);
            return coded.json(answer);
        }
        const folders: Host[] = [];
        for (const item of batch.cases) {
            folders.push(item.folder);
        }
        closing(folders, batch.label);
        batched(batch, coded, (item: Host, index: number): Host => outcome(item, batch, index));
        return coded.json(answer);
    } catch (error) {
        return coded.json({ failed: failed(error) });
    } finally {
        realApp.userInteractionLevel = level;
    }
};

// --- [EXPORTS] -------------------------------------------------------------------------

export { run };
