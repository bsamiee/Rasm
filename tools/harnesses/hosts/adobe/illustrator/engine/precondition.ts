import { attempt } from '@rasm/extendscript';
import { assignProperties } from '@rasm/extendscript/extendscript';
import { assign, type Host, prepared, RealFile, realApp } from '#/hosts/adobe/extendscript';
import { remember } from '#/hosts/adobe/illustrator/engine/application';
import { coded, expanded, reflects } from '#/hosts/adobe/illustrator/engine/codec';

// --- [OPERATIONS] ----------------------------------------------------------------------

// --- [DOCUMENTS]
const unlisted = (rows: Host): Host => assignProperties(new OpenOptions(), { addToRecentFiles: false }, coded.revive(rows));
const added = (session: Host, label: string, open: () => Host): Host => {
    const before = realApp.documents.length;
    const doc = open();
    if (doc === null || doc === undefined || doc.typename !== 'Document' || realApp.documents.length <= before) {
        throw new Error(`${label} left documents.length at ${realApp.documents.length}`);
    }
    session.documents.push(doc);
    return doc;
};
const evaluator =
    (session: Host, doc: Host, made: Host) =>
    (expression: string, statement?: boolean): Host =>
        session.evaluate(`(function (doc, made) { ${statement ? expression : `return (${expression});`}\n}).apply(this, arguments);`, [doc, made]);
const created = (session: Host, row: Host, title: string, made: Host): Host => {
    const preset = assignProperties(realApp.getPresetSettings(row.preset), coded.revive(row.settings), { title });
    const doc = added(session, `addDocument ${row.preset}`, () => realApp.documents.addDocument(row.preset, preset, false));
    const evaluate = evaluator(session, doc, made);
    const wrong: string[] = [];
    for (const write of row.writes) {
        assign(write[0], write[1], evaluate, coded.revive);
    }
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

// --- [STEPS]
const prepare = (evaluate: (expression: string, statement?: boolean) => Host, condition: Host, made: Host): void => {
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
                const method = row.method || 'add';
                if (typeof collection[method] !== 'function') {
                    throw new Error(`${row.to}.${row.kind} holds no method ${method}`);
                }
                const args: Host[] = [];
                for (const argument of row.args || []) {
                    args.push(evaluate(argument));
                }
                made[row.as] = assignProperties(collection[method].apply(collection, args), coded.revive(row.properties));
            },
        ],
        [
            'steps',
            condition.steps,
            (step: Host): void => {
                evaluate(step, true);
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
};
const placed = (session: Host, item: Host, label: string, made: Host): ((expression: string) => Host) => {
    const condition = item.precondition;
    const docs: Host[] = [];
    const missing: string[] = [];
    realApp.coordinateSystem = coded.revive(condition.coordinates);
    realApp.userInteractionLevel = coded.revive(condition.interaction);
    if (condition.colorSettings !== undefined) {
        realApp.loadColorSettings(new RealFile(condition.colorSettings));
    }
    for (const row of condition.preferences || []) {
        remember(session.preferences, row.key, row.kind, false);
        realApp.preferences[`set${row.kind}Preference`](row.key, row.value);
    }
    for (let i = 0; i < condition.documents.length; i += 1) {
        const row = condition.documents[i];
        docs.push(row.open === undefined ? created(session, row, `${label}-${i}`, made) : added(session, `open ${row.open}`, () => realApp.open(new RealFile(row.open), coded.revive(row.colorSpace), unlisted(row.options))));
    }
    for (const font of condition.fonts || []) {
        if (attempt(() => realApp.textFonts.getByName(font)) === undefined) {
            missing.push(font);
        }
    }
    if (missing.length > 0) {
        throw new Error(`fonts not installed: ${missing.join(', ')}`);
    }
    if (docs.length > 1) {
        realApp.activeDocument = docs[0];
    }
    for (const doc of docs) {
        session.record.opened.push(expanded(doc, []));
    }
    session.refresh();
    const evaluate = evaluator(session, docs[0], made);
    if (docs.length > 0) {
        prepare(evaluate, condition, made);
        session.refresh();
    }
    return evaluate;
};

// --- [EXPORTS] -------------------------------------------------------------------------

export { placed, unlisted };
