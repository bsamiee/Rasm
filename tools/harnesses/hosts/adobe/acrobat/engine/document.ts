import type { Schema } from 'effect';
import { compile, FAULTS, raised, root, thrown } from '#/hosts/adobe/acrobat/engine/script';
import { type Item, posix, refuse, type Session } from '#/hosts/adobe/acrobat/engine/session';
import { counted, type Doc, encoded, enumerated, invoked, isDoc, isMembers, keyed, type Members, revived, shaped } from '#/hosts/adobe/acrobat/engine/value';

// --- [TYPES] ---------------------------------------------------------------------------

type Precondition = Item['precondition'];
interface Holder {
    readonly events: unknown[];
    readonly scope: Members;
    readonly target: unknown;
    thrown: unknown;
}

// --- [OPERATIONS] ----------------------------------------------------------------------

const state = (session: Session, doc: Doc) => {
    const tree = (mark: Members): Schema.Json => ({ name: String(mark.name), children: (Array.isArray(mark.children) ? mark.children : []).filter(isMembers).map(tree) });
    return {
        path: posix(session, doc.path),
        pages: counted(doc.numPages, (page) => ({ rotation: doc.getPageRotation(page), boxes: keyed(session.request.boxes, (box) => encoded(doc.getPageBox(box, page))) })),
        info: keyed(enumerated(doc.info), (key) => encoded(doc.info[key])),
        metadata: doc.metadata,
        annots: (doc.getAnnots() ?? []).map((annot) => encoded(annot)),
        fields: counted(doc.numFields, (index) => {
            const name = doc.getNthFieldName(index);
            const field = doc.getField(name) ?? {};
            return { name, type: encoded(field.type), page: encoded(field.page), rect: encoded(field.rect) };
        }),
        ocgs: (doc.getOCGs() ?? []).map((ocg) => ({ name: String(ocg.name), state: ocg.state === true })),
        bookmarks: tree(doc.bookmarkRoot),
        dataObjects: (Array.isArray(doc.dataObjects) ? doc.dataObjects : []).filter(isMembers).map((data) => String(data.name)),
    };
};
const created = ({ request }: Session, page: Exclude<Precondition['documents'][number], { readonly open: string }>) => {
    const doc = shaped(invoked(root.app, 'newDoc', [page.width, page.height]), isDoc, 'app.newDoc');
    counted(page.pages - 1, (index) => doc.newPage(index + 1, page.width, page.height));
    if (page.rotation !== undefined) {
        doc.setPageRotations(0, doc.numPages - 1, page.rotation);
    }
    request.boxes.forEach((box) => {
        const rect = page.boxes?.[box];
        if (rect !== undefined) {
            doc.setPageBoxes(box, 0, doc.numPages - 1, rect);
        }
    });
    return doc;
};
const furnished = ([doc]: readonly Doc[], { fields = [], annots = [], info = {}, metadata }: Precondition) => {
    if (doc === undefined) {
        return;
    }
    fields.forEach(({ name, type, page, rect, properties = {} }) => {
        const field = doc.addField(name, type, page, rect);
        Object.keys(properties).forEach((key) => {
            field[key] = revived(properties[key]);
        });
    });
    annots.forEach((properties) => doc.addAnnot(revived(properties)));
    Object.keys(info).forEach((key) => {
        doc.info[key] = info[key];
    });
    if (metadata !== undefined) {
        doc.metadata = metadata;
    }
};
const opened = (session: Session) => {
    const { precondition } = session.request.case;
    const docs = precondition.documents.map((document) => ('open' in document ? shaped(invoked(root.app, 'openDoc', [document.open]), isDoc, 'app.openDoc') : created(session, document)));
    docs.forEach((doc) => session.docs.push(doc));
    furnished(docs, precondition);
    const made = precondition.documents.reduce<readonly string[]>((files, document, index) => {
        const doc = docs[index];
        if ('open' in document || doc === undefined) {
            return files;
        }
        const file = `${precondition.sandbox}/${index}.pdf`;
        doc.saveAs(file);
        return files.concat([file]);
    }, []);
    docs.forEach((doc) => session.records.opened.push(state(session, doc)));
    return { docs, made };
};
const fired = (session: Session, scope: Members, target: unknown, [doc]: readonly Doc[], code: string, event: NonNullable<Precondition['event']>) => {
    if (doc === undefined) {
        return refuse(session, `no document holds field ${event.field}`);
    }
    const field = doc.getField(event.field);
    if (field === null) {
        return refuse(session, `no field ${event.field} on the first document`);
    }
    compile(scope, code);
    // @effect-diagnostics-next-line globalDate:off
    const key = `event${Date.now()}`;
    const holder: Holder = { events: [], scope, target, thrown: undefined };
    doc[key] = holder;
    const names = Object.keys(scope);
    invoked(field, 'setAction', [
        event.trigger,
        `try { (function (${names.join(', ')}) {\n${code}\n}).apply(this.${key}.target, [${names.map((name) => `this.${key}.scope.${name}`).join(', ')}]); } catch (error) { this.${key}.thrown = error; }\nthis.${key}.events.push({ type: event.type, name: event.name, rc: event.rc, value: event.value, change: event.change, willCommit: event.willCommit });`,
    ]);
    try {
        if (event.trigger === 'Calculate') {
            doc.calculateNow();
        } else {
            field.value = revived(event.value);
        }
    } finally {
        invoked(field, 'setAction', [event.trigger, '']);
        delete doc[key];
    }
    holder.events.forEach((each) => session.records.events.push(encoded(each)));
    if (holder.thrown !== undefined) {
        throw raised(FAULTS.action, thrown(holder.thrown, 1).message, holder.thrown);
    }
};

// --- [EXPORTS] -------------------------------------------------------------------------

export { fired, opened };
