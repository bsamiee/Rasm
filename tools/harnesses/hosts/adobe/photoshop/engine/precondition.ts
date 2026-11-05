import { ownKeys } from '@rasm/extendscript/extendscript';
import { assign, type Host, prepared, realApp } from '#/hosts/adobe/extendscript';
import { agrees, coded, executed, id } from '#/hosts/adobe/photoshop/engine/codec';

// --- [OPERATIONS] ----------------------------------------------------------------------

// --- [DOCUMENTS]
const created = (row: Host, name: string, at: number): Host => {
    const asked = coded.revive(row.arguments);
    const doc = realApp.documents.add(asked.width, asked.height, asked.resolution, name, asked.mode, asked.initialFill, asked.pixelAspectRatio, asked.bitsPerChannel, asked.colorProfileName);
    for (const key of ownKeys(asked)) {
        if (key in doc && !agrees(doc[key], asked[key])) {
            throw new Error(`documents[${at}] ${key} reads ${String(doc[key])}, asked ${String(asked[key])}`);
        }
    }
    return doc;
};
const placed = (condition: Host, label: string): Host[] => {
    const docs: Host[] = [];
    const flags = condition.features || {};
    const missing: string[] = [];
    for (const flag of ownKeys(flags)) {
        if (realApp.featureEnabled(flag) !== flags[flag]) {
            throw new Error(`feature ${flag} reads ${!flags[flag]}`);
        }
    }
    for (const font of condition.fonts || []) {
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
        docs.push(row._tag === 'Opened' ? realApp.open(new File(row.open)) : created(row, `${label}-${i}`, i));
    }
    if (docs.length > 1) {
        realApp.activeDocument = docs[0];
    }
    return docs;
};

// --- [STEPS]
const prepare = (doc: Host, condition: Host, made: Host, evaluate: (expression: Host, statement?: boolean) => Host): void => {
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
                evaluate(step, true);
            },
        ],
        [
            'select',
            condition.select === undefined ? [] : [condition.select],
            (chosen: Host) => {
                const reference: Host = new ActionReference();
                for (const layer of chosen.layers || []) {
                    reference.putIdentifier(id('layer'), evaluate(layer).id);
                }
                if (chosen.layers !== undefined && chosen.layers.length > 0) {
                    const target: Host = new ActionDescriptor();
                    target.putReference(id('null'), reference);
                    target.putBoolean(id('makeVisible'), false);
                    executed('select', target);
                }
                if (chosen.region !== undefined) {
                    doc.selection.select(chosen.region);
                }
            },
        ],
    ]);
};

// --- [EXPORTS] -------------------------------------------------------------------------

export { placed, prepare };
