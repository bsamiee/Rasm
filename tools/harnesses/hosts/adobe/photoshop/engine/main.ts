import { attempt, batched, failed, finished, globals, type Host, holds, palettes, reads, realApp, released, sessionOf, started, thrown } from '#/hosts/adobe/extendscript';
import { coded, executed, parsedJson } from '#/hosts/adobe/photoshop/engine/codec';
import { placed, prepare } from '#/hosts/adobe/photoshop/engine/precondition';
import { members, redirected, shadowed } from '#/hosts/adobe/photoshop/engine/shadows';
import { added, application, changed, edited, errorLog, histories, opened, optionsRestored, restored, shifted, targets } from '#/hosts/adobe/photoshop/engine/state';

// --- [CONSTANTS] -----------------------------------------------------------------------

const ENGINE: string = new File($.fileName).fsName;
const PATHED = /^(?:\/|~)/;

// --- [OPERATIONS] ----------------------------------------------------------------------

// --- [DOCUMENTS]
const idsOf = (docs: Host): Host[] => {
    const ids: Host[] = [];
    for (const doc of docs) {
        ids.push(doc.id);
    }
    return ids;
};
const closed = (kept: readonly Host[]): number => {
    let count = 0;
    for (let i = realApp.documents.length - 1; i >= 0; i -= 1) {
        if (!holds(kept, realApp.documents[i].id)) {
            realApp.documents[i].close(SaveOptions.DONOTSAVECHANGES);
            count += 1;
        }
    }
    return count;
};

// --- [CASES]
const ran = (session: Host, item: Host, doc: Host, made: Host, result: Host, evaluate: (expression: Host) => Host): Host => {
    const folder = Folder.current;
    try {
        let value: Host;
        if (item._tag === 'Probe') {
            value = evaluate(item.evaluate);
        } else {
            const script = item.source === undefined ? null : new File(item.source.script);
            if (script !== null && !script.exists) {
                throw new Error(`no script ${item.source.script}`);
            }
            if (script !== null) {
                session.shadows.Folder.current = script.parent;
            }
            const code = (script === null ? '' : item.source.code) + (item._tag === 'Call' ? `\n;(${item.call}\n).apply(undefined, harnessCase.args)` : '');
            value = session.evaluate(code, doc, made, { script, args: coded.revive(item._tag === 'Call' ? item.args : []) });
        }
        palettes(session);
        if (item._tag !== 'Run') {
            result.value = coded.kept(value);
        }
        return { _tag: 'Ran' };
    } catch (error) {
        const line = 'line';
        const completion = thrown(error);
        const file = error !== null && typeof error === 'object' && 'fileName' in error && typeof error.fileName === 'string' && PATHED.test(error.fileName) ? new File(error.fileName).fsName : '';
        const at = file !== '' && file !== ENGINE ? file : item._tag !== 'Probe' && item.source !== undefined ? item.source.script : '';
        if (at === '') {
            delete completion[line];
        } else if (Object.prototype.hasOwnProperty.call(completion, line)) {
            completion.file = at;
        }
        return completion;
    } finally {
        session.shadows.Folder.current = folder;
    }
};
const outcome = (item: Host, label: string, found: Host, shift: Host): Host => {
    const session = sessionOf(item, ['history', 'errors']);
    const result = started(item);
    let leaked: string[] = [];
    const known = idsOf(realApp.documents);
    const held = { ruler: realApp.preferences.rulerUnits, type: realApp.preferences.typeUnits, colorSettings: realApp.colorSettings };
    const errors = errorLog();
    const global = globals();
    const initial = application();
    let release: () => void = () => {};
    const made: Host = {};
    session.refresh = () => {};
    session.options = {};
    session.folder = new Folder(item.folder).fsName;
    try {
        shadowed(session, found, item);
        realApp.displayDialogs = DialogModes.NO;
        realApp.preferences.rulerUnits = coded.revive(item.precondition.units.ruler);
        realApp.preferences.typeUnits = coded.revive(item.precondition.units.type);
        if (item.precondition.colorSettings !== undefined) {
            realApp.colorSettings = item.precondition.colorSettings;
        }
        const docs = placed(item.precondition, label);
        const evaluate = (expression: Host, statement?: boolean): Host => session.evaluate(statement ? expression : `(${expression}\n)`, docs[0], made, null);
        session.refresh();
        prepare(docs[0], item.precondition, made, evaluate);
        session.refresh();
        const reading = reads(item.read, evaluate, coded.kept);
        result.before = reading.state;
        result.beforeFailed = reading.failed;
        const ids = idsOf(docs);
        const before = { application: application(), targets: targets(ids), history: histories(docs) };
        session.record.opened = opened(before.targets, docs);
        release = redirected(session);
        result.completion = ran(session, item, docs[0], made, result, evaluate);
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
        closed(known);
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
        session.record.errors = errors();
    });
    session.record.writes = coded.kept(session.record.writes);
    return finished(result, session.record, leaked);
};

// --- [COMPOSITION] ---------------------------------------------------------------------

const run = (text: string): string => {
    const dialogs = realApp.displayDialogs;
    realApp.displayDialogs = DialogModes.NO;
    try {
        const batch = parsedJson(text);
        if (batch.quit === true) {
            const count = closed([]);
            executed('quit', new ActionDescriptor());
            return coded.json({ closed: count });
        }
        const shift = shifted(batch.label, new File(batch.records).parent.fsName);
        const found = members();
        const answer: Host = { application: realApp.name, version: realApp.version, state: { dialogs: String(dialogs), shifting: shift.shifting, saving: shift.saving, unlisted: found.unlisted, defaults: shift.defaults } };
        if (batch.survey === true) {
            answer.survey = {
                units: { ruler: String(realApp.preferences.rulerUnits).split('.')[0], type: String(realApp.preferences.typeUnits).split('.')[0] },
                host: { locale: $.locale, featureAccessLevel: String(application().whole.featureAccessLevel), colorSettings: String(realApp.colorSettings) },
            };
        }
        batched(batch, coded, (item: Host, index: number) => outcome(item, `${batch.label}-${index}`, found, shift));
        return coded.json(answer);
    } catch (error) {
        return coded.json({ failed: failed(error) });
    } finally {
        realApp.displayDialogs = dialogs;
    }
};

// --- [EXPORTS] -------------------------------------------------------------------------

export { run };
