import type { Schema } from 'effect';
import { signaturesOf } from '#/hosts/adobe/acrobat/engine/answer';
import { fired, opened } from '#/hosts/adobe/acrobat/engine/document';
import { adopted, scoped } from '#/hosts/adobe/acrobat/engine/scope';
import { compile, FAULTS, faultOf, root, thrown } from '#/hosts/adobe/acrobat/engine/script';
import { type Item, type Request, recorded, restored, type Session, started } from '#/hosts/adobe/acrobat/engine/session';
import { counted, type Doc, encoded, enumerated, has, invoked, isCallable, isDoc, isMembers, keyed, kindOf, type Members, revived, shaped } from '#/hosts/adobe/acrobat/engine/value';

// --- [TYPES] ---------------------------------------------------------------------------

interface Reads {
    readonly read: Record<string, Schema.Json>;
    readonly failed: Record<string, string>;
}

// --- [CONSTANTS] -----------------------------------------------------------------------

const SPREAD = '[*]';
const SURVEY_SIZE = 20;

// --- [OPERATIONS] ----------------------------------------------------------------------

const indexed = (text: string, at: number, index: number) => `${text.slice(0, at)}[${index}]${text.slice(at + SPREAD.length)}`;
const spread = (expression: string, evaluate: (code: string) => unknown): unknown => {
    const at = expression.indexOf(SPREAD);
    if (at < 0) {
        return evaluate(expression);
    }
    const head = expression.slice(0, at);
    const list = evaluate(head);
    const length = list === null || list === undefined ? 0 : shaped(list, (held): held is Members & { readonly length: number } => isMembers(held) && typeof held.length === 'number', head).length;
    return counted(length, (index) => spread(indexed(expression, at, index), evaluate));
};
const reads = (item: Item, evaluate: (code: string) => unknown): Reads => {
    const read: Record<string, Schema.Json> = {};
    const failed: Record<string, string> = {};
    Object.keys(item.read).forEach((key) => {
        try {
            const value = spread(item.read[key] ?? 'undefined', evaluate);
            const at = key.indexOf(SPREAD);
            if (at >= 0 && Array.isArray(value)) {
                value.forEach((each, index) => {
                    read[indexed(key, at, index)] = encoded(each);
                });
            } else {
                read[key] = encoded(value);
            }
        } catch (error) {
            failed[key] = thrown(error).message;
        }
    });
    return { read, failed };
};
const evaluated = (session: Session, scope: Members, target: unknown, docs: readonly Doc[], evaluate: (code: string, statement?: boolean) => unknown): unknown => {
    const { case: item } = session.request;
    if (item._tag === 'Probe') {
        return evaluate(item.evaluate);
    }
    if (item._tag === 'Run') {
        const { event } = item.precondition;
        return event === undefined ? evaluate(item.source.code, true) : fired(session, scope, target, docs, item.source.code, event);
    }
    const callee = evaluate(`${item.source?.code ?? ''}\nreturn (${item.call});`, true);
    return isCallable(callee) ? callee.apply(target, item.args.map(revived)) : callee;
};
const run = (request: Session['request'], context: unknown) => {
    const session = started(request);
    const { case: item } = request;
    const held: { made: readonly string[]; before?: Reads; after?: Reads } = { made: [] };
    const ended = (() => {
        try {
            const { docs, made } = opened(session);
            held.made = made;
            const scope: Members = {
                app: scoped(session, root.app, 'App'),
                util: scoped(session, root.util, 'Util'),
                console: scoped(session, root.console, 'Console'),
                global: Object.create(root.global, { setPersistent: { value: (name: unknown) => recorded(session, `Global.setPersistent ${String(name)}`) } }),
            };
            const [first] = docs;
            const target = first === undefined ? context : adopted(session, first, false);
            const evaluate = (code: string, statement = false) => compile(scope, statement ? code : `return (${code});`)(target);
            (item.precondition.steps ?? []).forEach((step) => evaluate(step, true));
            held.before = reads(item, evaluate);
            const result = evaluated(session, scope, target, docs, evaluate);
            held.after = reads(item, evaluate);
            return { completion: { _tag: 'Ran' as const }, value: encoded(result) };
        } catch (error) {
            const fault = faultOf(error);
            if (fault.name === FAULTS.refused) {
                return { completion: { _tag: 'Failed' as const, reason: fault.message } };
            }
            return { completion: fault.name === FAULTS.action && isMembers(error) ? thrown(error.cause, 1) : thrown(error) };
        }
    })();
    const writes = restored(session);
    return {
        outcome: {
            name: item.name,
            ...ended,
            ...(session.refused.length > 0 ? { completion: { _tag: 'Failed' as const, reason: session.refused.join('; ') } } : {}),
            ...(held.before === undefined ? {} : { before: held.before.read, beforeFailed: held.before.failed }),
            read: held.after?.read ?? {},
            readFailed: held.after?.failed ?? {},
            writes,
            ...session.records,
        },
        kept: held.made.filter((file) => !has(session.saved, file)),
    };
};
const surveyed = () => {
    const { app, util } = root;
    const doc = shaped(invoked(app, 'newDoc', []), isDoc, 'app.newDoc');
    try {
        const field = doc.addField('survey', 'text', 0, [0, SURVEY_SIZE, SURVEY_SIZE, 0]);
        const annot = doc.addAnnot({ type: 'Text', page: 0, point: [SURVEY_SIZE / 2, SURVEY_SIZE / 2] });
        const kinds = [root, app, util, root.console, root.global, doc, field, annot, doc.bookmarkRoot];
        return {
            viewer: {
                type: String(app.viewerType),
                variation: String(app.viewerVariation),
                language: String(app.language),
                plugIns: shaped(app.plugIns, (held): held is readonly Members[] => Array.isArray(held) && held.every(isMembers), 'app.plugIns').map((plugIn) => ({ name: String(plugIn.name), version: Number(plugIn.version), loaded: plugIn.loaded === true, certified: plugIn.certified === true })),
            },
            model: { members: keyed(kinds.map(kindOf), (_, index) => enumerated(kinds[index] ?? {})), signatures: signaturesOf({ App: app, Doc: doc, Util: util }) },
        };
    } finally {
        doc.closeDoc(true);
    }
};

// --- [COMPOSITION] ---------------------------------------------------------------------

const engine = function (this: unknown, request: Request): string {
    try {
        return encodeURIComponent(JSON.stringify({ _tag: 'Success', success: request._tag === 'Survey' ? surveyed() : run(request, this) }));
    } catch (error) {
        return encodeURIComponent(JSON.stringify({ _tag: 'Failure', failure: { reason: thrown(error).message } }));
    }
};

// --- [EXPORTS] -------------------------------------------------------------------------

export { engine };
