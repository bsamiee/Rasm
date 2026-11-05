import { encoded } from '@rasm/extendscript';
import { ownKeys } from '@rasm/extendscript/extendscript';

// --- [TYPES] ---------------------------------------------------------------------------

// biome-ignore lint/suspicious/noExplicitAny: reflect states ExtendScript host members at run time, and the engines shadow and revive them by name
type Host = any;

// --- [CONSTANTS] -----------------------------------------------------------------------

const RealFile: Host = File;
const RealFolder: Host = Folder;
const RealWindow: Host = Window;
const realDollar: Host = $;
const realGlobal: Host = $.global;
const BUILTINS: readonly Host[] = [Object.prototype, Array.prototype, String.prototype, Number.prototype, Boolean.prototype, Function.prototype, Date.prototype, RegExp.prototype, Error.prototype];
const ISO = /^(\d{4})-(\d\d)-(\d\d)T(\d\d):(\d\d):(\d\d)(?:\.(\d{1,3}))?Z$/;
const SCALED = /^(-?\d+)p(-?\d+)$/;
const DATED = /^date:(.+)$/;
const COUNTED = /^(\w+)#(\d+)$/;
const SPREAD = '[*]';
const ENUMERATOR_TAG = 'enum:';
const OBJECT_TAG = 'object:';

// --- [OPERATIONS] ----------------------------------------------------------------------

// --- [FILES]
const opened = (path: string, mode: string, lineFeed?: string): Host => {
    const file = new RealFile(path);
    file.encoding = 'UTF-8';
    if (lineFeed !== undefined) {
        file.lineFeed = lineFeed;
    }
    if (!file.open(mode)) {
        throw new Error(`cannot ${mode === 'r' ? 'read' : 'append'} ${path}`);
    }
    return file;
};
const read = (path: string): string => {
    const file = opened(path, 'r');
    const text = file.read();
    file.close();
    return text;
};
const append = (path: string, text: string): void => {
    const file = opened(path, 'a', 'Unix');
    file.write(text);
    file.close();
};

// --- [CODEC]
const exponent = (value: number): string => {
    let [mantissa, power] = [Math.abs(value), 0];
    while (mantissa >= 9_007_199_254_740_992) {
        mantissa /= 2;
        power += 1;
    }
    while (mantissa !== Math.floor(mantissa)) {
        mantissa *= 2;
        power -= 1;
    }
    return `${value < 0 ? '-' : ''}${String(mantissa)}p${power}`;
};
const tag = ($tag: string): Host => ({ $: $tag });
const wiredNumber = (value: number): Host => {
    if (isNaN(value)) {
        return tag('NaN');
    }
    if (!isFinite(value)) {
        return tag(value > 0 ? 'Infinity' : '-Infinity');
    }
    if (value === 0 && 1 / value < 0) {
        return tag('-0');
    }
    return Number(String(value)) === value ? value : tag(exponent(value));
};
const wired = (value: Host): Host => {
    if (value === undefined) {
        return tag('undefined');
    }
    if (typeof value === 'number') {
        return wiredNumber(value);
    }
    if (typeof value === 'function') {
        return '<function>';
    }
    if (value === null || typeof value !== 'object') {
        return value;
    }
    if (value instanceof Array) {
        const items: Host[] = [];
        for (const item of value) {
            items.push(wired(item));
        }
        return items;
    }
    const object: Host = {};
    for (const key of ownKeys(value)) {
        object[key] = wired(value[key]);
    }
    return object;
};
const iso = (date: Host): string => {
    const padded = (number: number, width: number) => `000${number}`.slice(-width);
    return isNaN(date.getTime()) ? 'NaN' : `${padded(date.getUTCFullYear(), 4)}-${padded(date.getUTCMonth() + 1, 2)}-${padded(date.getUTCDate(), 2)}T${padded(date.getUTCHours(), 2)}:${padded(date.getUTCMinutes(), 2)}:${padded(date.getUTCSeconds(), 2)}.${padded(date.getUTCMilliseconds(), 3)}Z`;
};
const parsed = (text: string): Date => {
    const parts: Host = ISO.exec(text);
    return parts ? new realGlobal.Date(realGlobal.Date.UTC(Number(parts[1]), Number(parts[2]) - 1, Number(parts[3]), Number(parts[4]), Number(parts[5]), Number(parts[6]), Number(parts[7] || 0))) : new realGlobal.Date(NaN);
};
const tagged = (value: Host): boolean => ownKeys(value).length === 1 && typeof value.$ === 'string';
const literal = (text: string): { readonly value: Host } | null => {
    const numbers: Host = { undefined, NaN, Infinity, '-Infinity': -Infinity, '-0': -0 };
    const scaled: Host = SCALED.exec(text);
    const dated: Host = DATED.exec(text);
    if (Object.prototype.hasOwnProperty.call(numbers, text)) {
        return { value: numbers[text] };
    }
    if (scaled) {
        return { value: Number(scaled[1]) * 2 ** Math.ceil(Number(scaled[2]) / 2) * 2 ** Math.floor(Number(scaled[2]) / 2) };
    }
    return dated ? { value: parsed(dated[1]) } : null;
};
const reflected = (value: Host): Host => {
    if (!(value instanceof realGlobal.ReflectionInfo)) {
        const rows: Host = { name: String(value.name), properties: [], methods: [] };
        for (const property of value.properties) {
            rows.properties.push(reflected(property));
        }
        for (const method of value.methods) {
            rows.methods.push(reflected(method));
        }
        return rows;
    }
    const held: Host = value;
    const info: Host = {
        name: String(held.name),
        type: String(held.type),
        dataType: String(held.dataType),
        isCollection: held.isCollection === true,
        min: held.min === undefined ? null : String(held.min),
        max: held.max === undefined ? null : String(held.max),
        defaultValue: held.defaultValue === undefined ? null : String(held.defaultValue),
        arguments: [],
    };
    if (String(held.type) === 'method') {
        for (const argument of held.arguments) {
            info.arguments.push(reflected(argument));
        }
    }
    return info;
};
const enumerated = (path: string): Host => {
    const dot = path.indexOf('.');
    const owner = dot < 0 ? undefined : realGlobal[path.slice(0, dot)];
    const member = owner === undefined || owner === null ? undefined : owner[path.slice(dot + 1)];
    if (member === undefined) {
        throw new Error(`enum:${path} names no enumerator`);
    }
    return member;
};
const codec = (host: Host) => {
    const enumerator = host.enumerator || enumerated;
    const kept = (value: Host, path?: Host): Host => {
        if (value === null || typeof value !== 'object') {
            return value;
        }
        if (value instanceof Array) {
            const items: Host[] = [];
            for (const item of value) {
                items.push(kept(item, path));
            }
            return items;
        }
        const named = host.named(value);
        if (named !== null) {
            return { $: named };
        }
        if (value.constructor === Object) {
            const object: Host = {};
            for (const key of ownKeys(value)) {
                object[key] = kept(value[key], path);
            }
            return object;
        }
        if (value instanceof Date) {
            return { $: `date:${iso(value)}` };
        }
        if (value instanceof realGlobal.Reflection || value instanceof realGlobal.ReflectionInfo) {
            return reflected(value);
        }
        const plain = host.plain === undefined ? null : host.plain(value, path || []);
        return plain === null ? String(value) : plain;
    };
    const revive = (value: Host): Host => {
        if (value instanceof Array) {
            const list: Host[] = [];
            for (const item of value) {
                list.push(revive(item));
            }
            return list;
        }
        if (value === null || typeof value !== 'object') {
            return value;
        }
        if (tagged(value)) {
            const text = String(value.$);
            const found = literal(text);
            if (found !== null) {
                return found.value;
            }
            if (text.indexOf(ENUMERATOR_TAG) === 0) {
                return enumerator(text.slice(ENUMERATOR_TAG.length));
            }
            return text.indexOf(OBJECT_TAG) === 0 ? host.located(text.slice(OBJECT_TAG.length)) : text;
        }
        const Kind = host.constructed === true && typeof value.typename === 'string' ? realGlobal[value.typename] : undefined;
        const made = typeof Kind === 'function';
        const object: Host = made ? new Kind() : {};
        for (const key of ownKeys(value)) {
            if (!(made && key === 'typename')) {
                object[key] = revive(value[key]);
            }
        }
        return object;
    };
    return { kept, revive, json: (value: Host): string => encoded(wired(kept(value, []))) };
};

// --- [STAND_INS]
const holds = (list: readonly Host[], key: Host): boolean => {
    for (const item of list) {
        if (item === key) {
            return true;
        }
    }
    return false;
};
const statics = (fake: Host, real: Host, own: Host, forwarding: boolean): Host => {
    const callable = function (): void {};
    const forwarded = (key: string, _previous: Host, next: Host): Host => {
        real[key] = next;
        return real[key];
    };
    for (const info of real.reflect.properties) {
        const { name } = info;
        if (!(name in callable || Object.prototype.hasOwnProperty.call(own, name))) {
            try {
                fake[name] = real[name];
            } catch {
                continue;
            }
            if (forwarding) {
                fake.watch(name, forwarded);
            }
        }
    }
    for (const info of real.reflect.methods) {
        const { name } = info;
        if (!(name in callable || Object.prototype.hasOwnProperty.call(own, name)) && typeof real[name] === 'function') {
            fake[name] = function (): Host {
                return real[name].apply(real, arguments);
            };
        }
    }
    for (const key of ownKeys(own)) {
        fake[key] = own[key];
    }
    return fake;
};
const refusing = (session: Host, name: string) =>
    function (): void {
        const parts: string[] = [name];
        for (let i = 0; i < arguments.length; i += 1) {
            parts.push(String(arguments[i]));
        }
        session.record.activations.push(parts.join(' '));
    };
const bridgeTalk = (session: Host): Host => {
    const RealBridgeTalk = realGlobal.BridgeTalk;
    const BridgeTalk: Host = function (): void {};
    BridgeTalk.prototype.send = function (this: Host): boolean {
        session.record.activations.push(`BridgeTalk.send ${String(this.target)}`);
        return false;
    };
    return statics(BridgeTalk, RealBridgeTalk, { launch: refusing(session, 'BridgeTalk.launch'), bringToFront: refusing(session, 'BridgeTalk.bringToFront'), ping: refusing(session, 'BridgeTalk.ping') }, false);
};
const dollar = (session: Host, item: Host, global: Host): Host => {
    const printed = function (): void {
        session.record.console.push(Array.prototype.join.call(arguments, ''));
    };
    const own: Host = {
        write: printed,
        writeln: printed,
        fileName: item.source === undefined ? '' : item.source.script,
        evalFile: (file: Host) => session.evaluate(read(file instanceof RealFile ? file.fsName : String(file))),
    };
    if (global !== undefined) {
        own.global = global;
    }
    return statics({}, realDollar, own, global === undefined);
};
const scope = (session: Host, item: Host, shadows: Host): Host => {
    const global: Host = {};
    for (const { name } of realGlobal.reflect.properties) {
        if (name !== '__proto__') {
            try {
                global[name] = realGlobal[name];
            } catch {}
        }
    }
    shadows.$ = dollar(session, item, global);
    for (const key of ownKeys(shadows)) {
        global[key] = shadows[key];
    }
    return global;
};

// --- [SCRIPTUI]
const run = (handler: Host, target: Host): Host => (typeof handler === 'function' ? handler.call(target) : undefined);
const clicked = (control: Host): void => {
    run(control.onClick, control);
};
const notified = (control: Host): void => {
    control.notify('onClick');
};
const descendants = (root: Host): Host[] => {
    const rows: Host[] = [];
    const walk = (control: Host): void => {
        for (const child of control.children || []) {
            rows.push(child);
            walk(child);
        }
    };
    walk(root);
    return rows;
};
const nameOf = (control: Host): string => (control.properties && control.properties.name !== undefined ? String(control.properties.name) : '');
const listing = (control: Host): boolean => control.items !== undefined && typeof control.find === 'function';
const match = (root: Host, key: string): Host => {
    const parts: Host = COUNTED.exec(key);
    const typed: Host[] = [];
    for (const row of descendants(root)) {
        if (parts ? row.type === parts[1] : nameOf(row) === key || row.text === key || row.title === key) {
            typed.push(row);
        }
    }
    const found = parts ? typed[Number(parts[2])] : typed[0];
    if (found !== undefined) {
        return found;
    }
    throw new Error(`no control ${key} in ${root.text}`);
};
const cancelling = (root: Host): Host => {
    if (root.cancelElement) {
        return root.cancelElement;
    }
    for (const row of descendants(root)) {
        if (row.type === 'button' && (nameOf(row).toLowerCase() === 'cancel' || String(row.text).toLowerCase() === 'cancel')) {
            return row;
        }
    }
    return null;
};
const fill = (root: Host, fields: Host): void => {
    for (const key of ownKeys(fields)) {
        const control = match(root, key);
        const value = fields[key];
        if (listing(control)) {
            control.selection = typeof value === 'number' ? value : control.find.call(control, value);
        } else if (typeof control.value === 'boolean') {
            control.value = value;
            run(control.onClick, control);
        } else {
            if (typeof control.value === 'number') {
                control.value = value;
            } else {
                control.text = String(value);
            }
            run(control.onChanging, control);
            run(control.onChange, control);
        }
    }
};
const tree = (root: Host): Host[] => {
    const rows: Host[] = [];
    for (const control of descendants(root)) {
        const row: Host = { type: control.type, text: control.text, name: nameOf(control), bounds: control.bounds ? [control.bounds[0], control.bounds[1], control.bounds[2], control.bounds[3]] : null };
        if (listing(control)) {
            row.items = [];
            for (const item of control.items) {
                row.items.push(item.text);
            }
            row.selection = control.selection ? control.selection.index : null;
        } else if (typeof control.value === 'boolean' || typeof control.value === 'number') {
            row.value = control.value;
        }
        rows.push(row);
    }
    return rows;
};
const next = (queue: Host, fallback: Host): Host => (queue && queue.length > 0 ? queue.shift() : fallback);
const answered = (shown: Host): Host => {
    let result = shown.answer.answer === undefined ? 1 : shown.answer.answer;
    if (shown.answer.fields) {
        fill(shown.window, shown.answer.fields);
    }
    for (const press of shown.answer.presses || []) {
        const control = match(shown.window, press);
        shown.click(control);
        result = control === cancelling(shown.window) ? 2 : 1;
    }
    shown.row.filled = tree(shown.window);
    return result;
};
const palettes = (session: Host): void => {
    for (const shown of session.palettes) {
        answered(shown);
    }
};
const windows = (session: Host, click: (control: Host) => void): Host => {
    const alert = (message: Host, title: Host): void => {
        if (!session.exited) {
            session.record.alerts.push(title === undefined ? { text: String(message) } : { text: String(message), title: String(title) });
        }
    };
    const confirm = (message: Host): Host => {
        session.record.confirms.push(String(message));
        return next(session.confirms, true);
    };
    const prompt = (message: Host, initial: Host): Host => {
        session.record.prompts.push(String(message));
        return next(session.prompts, initial);
    };
    const Fake: Host = function (type: Host, title: Host, bounds: Host, properties: Host): Host {
        const count = arguments.length;
        const made = count < 2 ? new RealWindow(type) : count < 3 ? new RealWindow(type, title) : count < 4 ? new RealWindow(type, title, bounds) : new RealWindow(type, title, bounds, properties);
        session.created.push(made);
        made.show = (): Host => {
            made.layout.layout(true);
            if (made.location === undefined) {
                made.center();
            }
            const shown: Host = { window: made, answer: next(session.windows, {}), click, row: { title: made.text, type: made.type, tree: tree(made) } };
            session.record.windows.push(shown.row);
            run(made.onShow, made);
            if (made.type !== 'palette') {
                return answered(shown);
            }
            session.palettes.push(shown);
            return 0;
        };
        return made;
    };
    Fake.prototype = RealWindow.prototype;
    return statics(Fake, RealWindow, { alert, confirm, prompt }, false);
};

// --- [PICKERS]
const picked = (session: Host, kind: string, Kind: Host, many: boolean): Host => {
    session.record.files.push(kind);
    const answer = next(session.files, null);
    if (answer === null) {
        return null;
    }
    const made = new Kind(answer);
    return many ? [made] : made;
};
const patched = (session: Host) => {
    const executed = function (this: Host): boolean {
        session.record.activations.push(`execute ${this.fsName}`);
        return true;
    };
    const opening = (_prompt: Host, _filter: Host, many: Host) => picked(session, 'open', RealFile, many === true);
    const saving = () => picked(session, 'save', RealFile, false);
    const choosing = () => picked(session, 'folder', RealFolder, false);
    const rows: Host = [
        [RealFile, 'openDialog', opening],
        [RealFile, 'saveDialog', saving],
        [RealFolder, 'selectDialog', choosing],
        [RealFile.prototype, 'openDlg', opening],
        [RealFile.prototype, 'saveDlg', saving],
        [RealFolder.prototype, 'selectDlg', choosing],
        [RealFile.prototype, 'execute', executed],
        [RealFolder.prototype, 'execute', executed],
    ];
    const held: Host[] = [];
    for (const row of rows) {
        held.push(row[0][row[1]]);
        row[0][row[1]] = row[2];
    }
    return (): void => {
        for (let j = 0; j < rows.length; j += 1) {
            rows[j][0][rows[j][1]] = held[j];
        }
    };
};

// --- [GLOBALS]
const globals = (): Host[] => {
    const held: Host[] = [];
    for (const owner of [realGlobal].concat(BUILTINS)) {
        const keys: Host = {};
        for (const key in owner) {
            keys[key] = { value: owner[key] };
        }
        held.push({ owner, keys });
    }
    return held;
};
const released = (held: readonly Host[]): string[] => {
    const leaked: string[] = [];
    for (const { owner, keys } of held) {
        for (const key in owner) {
            try {
                if (!Object.prototype.hasOwnProperty.call(keys, key)) {
                    delete owner[key];
                } else if (owner[key] !== keys[key].value) {
                    owner[key] = keys[key].value;
                }
            } catch (error) {
                leaked.push(`global ${key}: ${messageOf(error)}`);
            }
        }
    }
    return leaked;
};

// --- [RUN]
const messageOf = (error: Host): string => String(error.message);
const failed = (error: Host): string => messageOf(error) + (error.line ? ` at line ${error.line}` : '');
const thrown = (error: Host): Host => {
    if (error === null || typeof error !== 'object') {
        return { _tag: 'Thrown', message: String(error) };
    }
    const completion: Host = { _tag: 'Thrown', message: `${error.name ? `${error.name}: ` : ''}${String(error.message)}` };
    if (typeof error.number === 'number') {
        completion.number = error.number;
    }
    if (typeof error.line === 'number') {
        completion.line = error.line;
    }
    return completion;
};
const attempt = (leaked: string[], label: string, work: () => void): void => {
    try {
        work();
    } catch (error) {
        leaked.push(`${label}: ${messageOf(error)}`);
    }
};
const sessionOf = (item: Host, recorded: readonly string[]): Host => {
    const record: Host = { alerts: [], confirms: [], prompts: [], windows: [], files: [], activations: [], console: [], writes: [], opened: [] };
    for (const name of recorded) {
        record[name] = [];
    }
    return { palettes: [], created: [], documents: [], windows: (item.windows || []).slice(0), confirms: (item.confirms || []).slice(0), prompts: (item.prompts || []).slice(0), files: (item.files || []).slice(0), record };
};
const started = (item: Host): Host => ({ name: item.name, completion: { _tag: 'Failed', reason: 'case did not finish' } });
const finished = (result: Host, record: Host, leaked: readonly string[]): Host => {
    for (const key of ownKeys(record)) {
        result[key] = record[key];
    }
    result.leaked = leaked;
    return result;
};
const batched = (batch: Host, coded: Host, outcome: (item: Host, index: number) => Host): void => {
    for (let i = 0; i < batch.cases.length; i += 1) {
        append(batch.records, `${coded.json({ _tag: 'Started', index: i })}\n`);
        append(batch.records, `${coded.json({ _tag: 'Finished', outcome: outcome(batch.cases[i], i) })}\n`);
    }
};

// --- [PRECONDITION]
const indexed = (text: string, index: number): string => text.split(SPREAD).join(`[${index}]`);
const spread = (expression: string, evaluate: (text: string) => Host): string[] => {
    const at = expression.indexOf(SPREAD);
    if (at < 0) {
        return [expression];
    }
    const paths: string[] = [];
    const count = evaluate(expression.slice(0, at)).length;
    for (let i = 0; i < count; i += 1) {
        paths.push(indexed(expression, i));
    }
    return paths;
};
const assign = (target: string, value: Host, evaluate: (text: string) => Host, revive: (value: Host) => Host): void => {
    for (const path of spread(target, evaluate)) {
        const dot = path.lastIndexOf('.');
        evaluate(path.slice(0, dot))[path.slice(dot + 1)] = revive(value);
    }
};
const prepared = (rows: Host): void => {
    for (const row of rows) {
        const list = row[1] || [];
        for (let i = 0; i < list.length; i += 1) {
            try {
                row[2](list[i], i);
            } catch (error) {
                throw new Error(`${row[0]}[${i}]: ${failed(error)}`);
            }
        }
    }
};
const reads = (wanted: Host, evaluate: (text: string) => Host, kept: (value: Host, path: Host) => Host): Host => {
    const values: Host = {};
    const lost: Host = {};
    for (const key of ownKeys(wanted)) {
        try {
            const paths: Host = spread(wanted[key], evaluate);
            const listed = wanted[key].indexOf(SPREAD) >= 0;
            const each = listed && key.indexOf(SPREAD) >= 0;
            const list: Host[] = [];
            for (let i = 0; i < paths.length; i += 1) {
                if (each) {
                    try {
                        values[indexed(key, i)] = evaluate(paths[i]);
                    } catch (inner) {
                        lost[indexed(key, i)] = messageOf(inner);
                    }
                } else {
                    list.push(evaluate(paths[i]));
                }
            }
            if (!each) {
                values[key] = listed ? list : list[0];
            }
        } catch (error) {
            lost[key] = messageOf(error);
        }
    }
    return { state: kept(values, []), failed: lost };
};

// --- [EXPORTS] -------------------------------------------------------------------------

export { assign, attempt, batched, bridgeTalk, clicked, codec, dollar, failed, finished, globals, type Host, holds, messageOf, notified, palettes, patched, picked, prepared, read, reads, refusing, released, scope, sessionOf, started, statics, tagged, thrown, windows };
