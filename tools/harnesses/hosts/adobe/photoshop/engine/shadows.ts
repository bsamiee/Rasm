import { ownKeys } from '@rasm/extendscript/extendscript';
import { bridgeTalk, type Host, holds, notified, patched, picked, realApp, realGlobal, refusing, scope, statics, windows } from '#/hosts/adobe/extendscript';
import { coded } from '#/hosts/adobe/photoshop/engine/codec';
import { custom } from '#/hosts/adobe/photoshop/engine/state';

// --- [CONSTANTS] -----------------------------------------------------------------------

const realOpen: Host = File.prototype.open;
const WRITING = /^[wae]/;
const HELD: readonly string[] = ['displayDialogs', 'playbackDisplayDialogs', 'notifiersEnabled'];
const EVENTS: readonly string[] = ['openViewlessDocument', 'systemCall', 'quit'];

// --- [OPERATIONS] ----------------------------------------------------------------------

// --- [FILES]
const ensured = (folder: Host): void => {
    if (folder !== null && !folder.exists) {
        ensured(folder.parent);
        folder.create();
    }
};
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
    if (Kind === Folder) {
        Shadow.watch('current', (_key: Host, _previous: Host, next: Host): Host => {
            Folder.current = next;
            return Folder.current;
        });
    }
    return Shadow;
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
    File.prototype.open = opening;
    return (): void => {
        File.prototype.open = realOpen;
        release();
    };
};

// --- [APPLICATION]
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
        const result = realGlobal.executeAction(event, descriptor === undefined ? new ActionDescriptor() : descriptor, DialogModes.NO);
        session.refresh();
        return result;
    };
const progressing = (session: Host, name: string) =>
    function (): Host {
        session.record.activations.push(name);
        return session.evaluate(String(arguments[arguments.length - 1]));
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
const REPLACED: Host = {
    bringToFront: refusing,
    showColorPicker: refusing,
    system: refusing,
    togglePalettes: refusing,
    batch: refusing,
    doProgress: progressing,
    doForcedProgress: progressing,
    doProgressTask: progressing,
    doProgressSegmentTask: progressing,
    doProgressSubTask: progressing,
    putCustomOptions: optioned,
    eraseCustomOptions: optioned,
};
const declared = (list: Host): Host => {
    const names: Host = {};
    for (const row of list) {
        names[row.name] = row.type;
    }
    return names;
};
const members = (): Host => {
    const methods = declared(realApp.reflect.methods);
    const unlisted: string[] = [];
    for (const member of ownKeys(REPLACED)) {
        if (!Object.prototype.hasOwnProperty.call(methods, member)) {
            unlisted.push(member);
        }
    }
    return { properties: declared(realApp.reflect.properties), methods, unlisted };
};
const documentsOf = (session: Host): Host => {
    const real = realApp.documents;
    const list: Host = { typename: real.typename, parent: real.parent };
    list.add = function (): Host {
        const made = real.add.apply(real, arguments);
        session.refresh();
        return made;
    };
    list.getByName = (name: Host): Host => real.getByName(name);
    list.sync = (): void => {
        for (let i = 0; i < list.length; i += 1) {
            delete list[i];
        }
        list.length = real.length;
        for (let j = 0; j < real.length; j += 1) {
            list[j] = real[j];
        }
    };
    list.sync();
    return list;
};
const appOf = (session: Host, found: Host, executeAction: Host): Host => {
    const fake: Host = {};
    const writable: string[] = [];
    const documents = documentsOf(session);
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
    const own: Host = {
        documents,
        executeAction,
        open(): Host {
            const made = realApp.open.apply(realApp, arguments);
            refresh();
            return made;
        },
        openDialog: () => picked(session, 'open', File, true),
        updateProgress: () => true,
        changeProgressText: () => {},
    };
    for (const member of ownKeys(REPLACED)) {
        if (Object.prototype.hasOwnProperty.call(found.methods, member)) {
            own[member] = REPLACED[member](session, member);
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

// --- [SCOPE]
const built = (session: Host): Host => {
    const Shadow: Host = function (): Host {
        const names: Host[] = [];
        for (let i = 0; i < arguments.length - 1; i += 1) {
            names.push(arguments[i]);
        }
        return session.evaluate(`(function (${names.join(',')}) {\n${arguments.length > 0 ? arguments[arguments.length - 1] : ''}\n})`);
    };
    Shadow.prototype = Function.prototype;
    return Shadow;
};
const shadowed = (session: Host, found: Host, item: Host): void => {
    const executeAction = acted(session);
    const Fake = windows(session, notified);
    const shadows: Host = {
        Window: Fake,
        alert: Fake.alert,
        confirm: Fake.confirm,
        prompt: Fake.prompt,
        BridgeTalk: bridgeTalk(session),
        executeAction,
        Function: built(session),
        File: rooted(session, File),
        Folder: rooted(session, Folder),
    };
    shadows.app = appOf(session, found, executeAction);
    const global = scope(session, item, shadows);
    const names: Host = [];
    const values: Host[] = [];
    for (const key of ownKeys(shadows)) {
        names.push(key);
        values.push(shadows[key]);
    }
    const scoped = Function.apply(null, names.concat(['doc', 'made', 'harnessCase', 'harnessSource', 'return eval(harnessSource);']));
    let context: Host[] = [undefined, {}, null];
    session.shadows = shadows;
    session.evaluate = function (code: Host, doc: Host, made: Host, held: Host): Host {
        if (arguments.length > 1) {
            context = [doc, made, held];
        }
        return scoped.apply(global, values.concat(context, [code]));
    };
};

// --- [EXPORTS] -------------------------------------------------------------------------

export { members, redirected, shadowed };
