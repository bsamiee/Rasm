import { attempt } from '@rasm/extendscript';
import { ownKeys } from '@rasm/extendscript/extendscript';
import { bridgeTalk, type Host, holds, notified, RealFile, read, realApp, refusing, scope, windows } from '#/hosts/adobe/extendscript';
import { preferences } from '#/hosts/adobe/illustrator/engine/application';
import { unlisted } from '#/hosts/adobe/illustrator/engine/precondition';

// --- [CONSTANTS] -----------------------------------------------------------------------

const ACTION_SET = /\/name\s*\[\s*\d+\s+([0-9A-Fa-f]+)\s*\]/;

// --- [OPERATIONS] ----------------------------------------------------------------------

// --- [APPLICATION]
const fakeDocuments = (made: (doc: Host) => Host): Host => {
    const real = realApp.documents;
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
    const pasted = function (): Host {
        session.record.writes.push({ _tag: 'Pasteboard', member: name });
        return forward(arguments);
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
        copy: pasted,
        cut: pasted,
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
    const made = (doc: Host): Host => {
        session.documents.push(doc);
        refresh();
        return doc;
    };
    const documents: Host = fakeDocuments(made);
    const own: Host = {
        documents,
        preferences: preferences(session),
        open(file: Host, colorSpace: Host, _options: Host): Host {
            return made(realApp.open.apply(realApp, arguments.length < 3 ? [file, colorSpace, unlisted({})] : arguments));
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
            attempt(() => {
                fake[name] = realApp[name];
            });
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

// --- [SCOPE]
const scoped: Host = eval('(function (Window, alert, confirm, prompt, app, $, BridgeTalk, self, source, extra) { return (function () { return eval(source); }).apply(self, extra); })');
const shadow = (session: Host, item: Host): void => {
    const Fake = windows(session, notified);
    const shadows: Host = { Window: Fake, alert: Fake.alert, confirm: Fake.confirm, prompt: Fake.prompt, app: fakeApp(session, item), BridgeTalk: bridgeTalk(session) };
    const global = scope(session, item, shadows);
    session.evaluate = (code: Host, extra: Host): Host => scoped(shadows.Window, shadows.alert, shadows.confirm, shadows.prompt, shadows.app, shadows.$, shadows.BridgeTalk, global, code, extra);
};

// --- [EXPORTS] -------------------------------------------------------------------------

export { shadow };
