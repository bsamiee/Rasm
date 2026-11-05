import { attempt, batched, failed, finished, globals, type Host, holds, palettes, patched, RealFile, RealFolder, reads, realApp, released, sessionOf, started, thrown } from '#/hosts/adobe/extendscript';
import { plain, restored, settled, snapshot, surveyed } from '#/hosts/adobe/illustrator/engine/application';
import { coded } from '#/hosts/adobe/illustrator/engine/codec';
import { placed } from '#/hosts/adobe/illustrator/engine/precondition';
import { shadow } from '#/hosts/adobe/illustrator/engine/shadows';

// --- [CONSTANTS] -----------------------------------------------------------------------

const LINE_BREAK = /\r?\n/;

// --- [OPERATIONS] ----------------------------------------------------------------------

// --- [LOGS]
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

// --- [DOCUMENTS]
const keyOf = (doc: Host): string => String(doc.fullName);
const keys = (): string[] => {
    const list: string[] = [];
    for (const doc of realApp.documents) {
        list.push(keyOf(doc));
    }
    return list;
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

// --- [CASES]
const ran = (session: Host, item: Host, evaluate: (expression: string) => Host, result: Host): Host => {
    try {
        const value = item._tag === 'Probe' ? evaluate(item.evaluate) : session.evaluate(item._tag === 'Call' ? `${item.source === undefined ? '' : item.source.code}\n;(${item.call}).apply(this, arguments);` : item.source.code, coded.revive(item.args));
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
    const session = sessionOf(item, []);
    const result = started(item);
    const known = keys();
    const global = globals();
    const base = snapshot();
    const from = offsets(batch.logs);
    const mine: string[] = [];
    const leaked: string[] = [];
    let release: () => void = () => {};
    let start = base;
    session.loaded = [];
    session.preferences = {};
    try {
        shadow(session, item);
        release = patched(session);
        const evaluate = placed(session, item, `${batch.label}-${index}`, {});
        start = snapshot();
        const before = reads(item.read, evaluate, coded.kept);
        result.before = before.state;
        result.beforeFailed = before.failed;
        result.completion = ran(session, item, evaluate, result);
        const after = reads(item.read, evaluate, coded.kept);
        result.read = after.state;
        result.readFailed = after.failed;
    } catch (outer) {
        result.completion = { _tag: 'Failed', reason: failed(outer) };
    }
    attempt(leaked, 'prototypes', release);
    attempt(leaked, 'globals', () => {
        leaked.push(...released(global));
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
    if (item.precondition.colorSettings !== undefined) {
        attempt(leaked, 'color settings', () => {
            realApp.loadColorSettings(new RealFile(batch.colorSettings));
        });
    }
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
    for (const [label, restore] of [
        ['preferences', () => settled(session.preferences)],
        ['application', () => restored(base, start)],
    ] as const) {
        attempt(leaked, label, () => {
            const rows = restore();
            session.record.writes.push(...rows.writes);
            leaked.push(...rows.leaked);
        });
    }
    session.record.writes = coded.kept(session.record.writes, []);
    session.record.logs = logged(batch.logs, from);
    return finished(result, session.record, leaked);
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
        } else if (batch._tag === 'Closed') {
            closing(batch.folders);
        } else {
            const folders: Host[] = [];
            for (const item of batch.cases) {
                folders.push(item.folder);
            }
            closing(folders, batch.label);
            batched(batch, coded, (item: Host, index: number): Host => outcome(item, batch, index));
        }
        return coded.json(answer);
    } catch (error) {
        return coded.json({ failed: failed(error) });
    } finally {
        realApp.userInteractionLevel = level;
    }
};

// --- [EXPORTS] -------------------------------------------------------------------------

export { run };
