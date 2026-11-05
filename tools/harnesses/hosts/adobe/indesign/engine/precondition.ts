import { ownKeys } from '@rasm/extendscript/extendscript';
import { assign, type Host, prepared, reads, realApp, tagged } from '#/hosts/adobe/extendscript';
import { coded } from '#/hosts/adobe/indesign/engine/codec';
import { POINTS, pinned, surveyed } from '#/hosts/adobe/indesign/engine/instance';

// --- [OPERATIONS] ----------------------------------------------------------------------

// --- [EXPRESSIONS]
const evaluated = (session: Host, expression: string, doc: Host, made: Host, statement?: boolean): Host => new Function('doc', 'app', 'made', statement ? expression : `return (${expression});`)(doc, session.app, made);
const differing = (target: Host, wanted: Host, path: string): string[] => {
    if (wanted !== null && typeof wanted === 'object' && !(wanted instanceof Array) && !tagged(wanted)) {
        let rows: string[] = [];
        for (const key of ownKeys(wanted)) {
            rows = rows.concat(differing(target[key], wanted[key], `${path}.${key}`));
        }
        return rows;
    }
    const same = wanted !== null && typeof wanted === 'object' && tagged(wanted) && wanted.$.indexOf('enum:') === 0 ? Number(target) === Number(coded.revive(wanted)) : coded.json(target) === coded.json(wanted) || (typeof wanted === 'string' && target !== null && typeof target === 'object' && target.name === wanted);
    return same ? [] : [`${path} reads ${coded.json(target)} where the precondition asks ${coded.json(wanted)}`];
};

// --- [DOCUMENTS]
const opened = (session: Host, item: Host, batch: Host): Host[] => {
    const condition = item.precondition;
    realApp.scriptPreferences.measurementUnit = coded.revive(condition.units.script);
    const docs: Host[] = [];
    for (let i = 0; i < condition.documents.length; i += 1) {
        const row = condition.documents[i];
        const doc = row.open === undefined ? realApp.documents.add(true, undefined, coded.revive(row.properties)) : realApp.open(new File(row.open), true);
        session.documents.push(doc);
        doc.insertLabel(batch.label, batch.records);
        if (row.open !== undefined) {
            doc.viewPreferences.horizontalMeasurementUnits = coded.revive(condition.units.horizontal);
            doc.viewPreferences.verticalMeasurementUnits = coded.revive(condition.units.vertical);
        }
        const wrong = row.open === undefined ? differing(doc, row.properties, `document ${i}`) : [];
        if (wrong.length > 0) {
            throw new Error(wrong.join(', '));
        }
        docs.push(doc);
    }
    const missing: string[] = [];
    for (const name of condition.fonts || []) {
        const font = realApp.fonts.itemByName(name);
        const status = font.isValid ? font.status : undefined;
        if (status === undefined || Number(status) !== Number(FontStatus.INSTALLED)) {
            missing.push(`${name} ${String(status === undefined ? 'absent' : status)}`);
        }
    }
    if (missing.length > 0) {
        throw new Error(`fonts not installed: ${missing.join(', ')}`);
    }
    if (docs.length > 1) {
        realApp.activeDocument = docs[0];
    }
    session.refresh();
    return docs;
};
const state = (doc: Host): Host => {
    const found = surveyed(doc);
    const row: Host = found.preferences;
    row.name = doc.name;
    row.unread = found.unread;
    row.cmykProfile = doc.cmykProfile;
    row.rgbProfile = doc.rgbProfile;
    row.appliedLanguage = doc.textDefaults.appliedLanguage.name;
    row.fonts = { names: doc.fonts.everyItem().name, status: doc.fonts.everyItem().status };
    row.links = { names: doc.links.everyItem().name, status: doc.links.everyItem().status };
    return coded.kept(row);
};
const exported = (item: Host, docs: Host, moment: string): void => {
    if (item.precondition.idml !== true) {
        return;
    }
    for (let i = 0; i < docs.length; i += 1) {
        if (docs[i].isValid) {
            docs[i].exportFile(ExportFormat.INDESIGN_MARKUP, new File(`${item.folder}/${i}.${moment}.idml`));
        }
    }
};

// --- [STEPS]
const prepare = (session: Host, doc: Host, condition: Host, made: Host): void => {
    const evaluate = (expression: string): Host => evaluated(session, expression, doc, made);
    const labels = condition.labels || {};
    const selected: Host[] = [];
    for (const label of ownKeys(labels)) {
        doc.insertLabel(label, labels[label]);
    }
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
                made[row.as] = evaluate(row.to)[row.kind].add(coded.revive(row.properties || {}));
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
            (pick: Host): void => {
                selected.push(evaluate(pick));
            },
        ],
    ]);
    if (selected.length > 0) {
        realApp.select(selected);
    }
    session.refresh();
};
const readings = (session: Host, docs: Host, wanted: Host, made: Host): Host =>
    pinned(POINTS, (): Host => {
        const frames: Host[] = [];
        for (const doc of docs) {
            if (doc.isValid) {
                frames.push([doc, doc.viewPreferences.rulerOrigin, doc.zeroPoint]);
                doc.viewPreferences.rulerOrigin = RulerOrigin.PAGE_ORIGIN;
                doc.zeroPoint = [0, 0];
            }
        }
        try {
            return reads(wanted, (expression: string): Host => evaluated(session, expression, docs[0], made), coded.kept);
        } finally {
            for (const frame of frames) {
                if (frame[0].isValid) {
                    frame[0].viewPreferences.rulerOrigin = frame[1];
                    frame[0].zeroPoint = frame[2];
                }
            }
        }
    });

// --- [EXPORTS] -------------------------------------------------------------------------

export { exported, opened, prepare, readings, state };
