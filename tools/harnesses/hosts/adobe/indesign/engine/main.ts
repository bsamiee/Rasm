import { escaped } from '@rasm/extendscript';
import { attempt, batched, clicked, dollar, failed, finished, globals, type Host, palettes, patched, realApp, released, sessionOf, started, thrown, windows } from '#/hosts/adobe/extendscript';
import { fakeApp } from '#/hosts/adobe/indesign/engine/application';
import { coded, enumerated } from '#/hosts/adobe/indesign/engine/codec';
import { activeOf, header, members, owns, pinned, recovered, restore, snapshot, writes } from '#/hosts/adobe/indesign/engine/instance';
import { exported, opened, prepare, readings, state } from '#/hosts/adobe/indesign/engine/precondition';

// --- [CONSTANTS] -----------------------------------------------------------------------

const scoped: Host = eval('(function (Window, alert, confirm, prompt, app, File, Folder, exit, $, document, source, extra) { return (function () { return eval(source); }).apply(undefined, extra); })');

// --- [OPERATIONS] ----------------------------------------------------------------------

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
const ran = (session: Host, item: Host, extra: Host, result: Host): Host => {
    try {
        const value = pinned({ userInteractionLevel: item.interact === true ? UserInteractionLevels.INTERACT_WITH_ALL : UserInteractionLevels.NEVER_INTERACT }, (): Host => session.evaluate(body(item), extra));
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
        return session.exited ? { _tag: 'Exited' } : thrown(error);
    }
};
const outcome = (item: Host, batch: Host, found: Host): Host => {
    const session = sessionOf(item, ['dialogs', 'menus', 'undo']);
    const result = started(item);
    const unit = realApp.scriptPreferences.measurementUnit;
    const active = activeOf();
    const activeId = active === null ? null : active.id;
    const known: Host = {};
    const global = globals();
    const made: Host = {};
    let leaked: string[] = [];
    let held: Host = null;
    let release: Host = (): void => {};
    for (const id of realApp.documents.everyItem().id) {
        known[id] = true;
    }
    session.actions = [];
    session.dialogs = (item.dialogs || []).slice(0);
    session.exited = false;
    session.menus = item.precondition.menus === true;
    try {
        held = snapshot(found);
        const Fake: Host = windows(session, clicked);
        const exit = (): void => {
            session.exited = true;
            throw new Error('exit');
        };
        const shadowed = dollar(session, item, undefined);
        session.app = fakeApp(session, item);
        session.evaluate = (code: Host, extra: Host): Host => scoped(Fake, Fake.alert, Fake.confirm, Fake.prompt, session.app, File, Folder, exit, shadowed, session.app.activeDocument, code, extra);
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
                            if (docs[d].isValid) {
                                const now = docs[d].undoHistory;
                                inner.undo = inner.undo.concat(now.slice(0, now.length - history[d]));
                            }
                        }
                        inner.after = readings(session, docs, item.read, made);
                        inner.writes = writes(held);
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
        attempt(leaked, 'close', (): void => {
            if (open.isValid) {
                open.close.apply(open, open.reflect.name === 'Library' ? [] : [SaveOptions.NO]);
            }
        });
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

// --- [COMPOSITION] ---------------------------------------------------------------------

const run = (text: string): string => {
    try {
        return pinned({ userInteractionLevel: UserInteractionLevels.NEVER_INTERACT }, (): string => {
            const batch = eval(`(${text})`);
            enumerated(batch.enumerations);
            recovered(batch.label);
            const found = members();
            const answer = header(found);
            batched(batch, coded, (item: Host): Host => outcome(item, batch, found));
            return coded.json(answer);
        });
    } catch (error) {
        return coded.json({ failed: failed(error) });
    }
};

// --- [EXPORTS] -------------------------------------------------------------------------

export { run };
