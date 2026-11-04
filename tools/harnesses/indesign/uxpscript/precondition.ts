import indesign, { app } from 'adobe:indesign';
import { JsonObject, Tagged } from '@rasm/harnesses/values';
import { Array, Iterable, Match, Option, Predicate, pipe, Record, Result, Schema } from 'effect';
import { owned, preferences } from '#/state';
import { type Codec, compiled, equal, field, invoked, type Members, message, type Session, scope } from '#/values';

// --- [MODELS] --------------------------------------------------------------------------

const EnumTag = Schema.Struct({ $: Schema.TemplateLiteral(['enum:', Schema.String]) });
const same = Schema.toEquivalence(Schema.Json);

// --- [OPERATIONS] ----------------------------------------------------------------------

const given = (fake: unknown, expression: string, doc: unknown, made: Members, statement = false) => {
    const { keys, values } = scope({ app: fake, doc, made });
    return compiled(keys, statement ? expression : `return (${expression});`)(...values);
};
const assign = (fake: unknown, target: string, value: unknown, doc: unknown, made: Members) => {
    const at = target.indexOf('[*]');
    const count = at < 0 ? 1 : Number(field(given(fake, target.slice(0, at), doc, made), 'length'));
    Array.fromIterable(
        Iterable.take(
            Iterable.makeBy((index) => target.replaceAll('[*]', `[${index}]`)),
            count,
        ),
    ).forEach((path) => Reflect.set(new Object(given(fake, path.slice(0, path.lastIndexOf('.')), doc, made)), path.slice(path.lastIndexOf('.') + 1), value));
};
const differing = (codec: Codec, target: unknown, wanted: Schema.Json, path: string): readonly string[] => {
    const holds = Match.value(wanted).pipe(
        Match.when(Schema.is(EnumTag), (tag) => equal(target, codec.revived(tag))),
        Match.when(Schema.is(Tagged), () => same(codec.encoded(target), wanted)),
        Match.when(Schema.is(JsonObject), () => true),
        Match.when(Predicate.isString, (name) => same(codec.encoded(target), wanted) || field(target, 'name') === name),
        Match.orElse(() => same(codec.encoded(target), wanted)),
    );
    const nested = Schema.is(JsonObject)(wanted) && !Predicate.isString(wanted.$) ? Object.entries(wanted).flatMap(([key, value]) => differing(codec, field(target, key), value, `${path}.${key}`)) : [];
    return holds ? nested : [`${path} reads ${JSON.stringify(codec.encoded(target))} where the precondition asks ${JSON.stringify(wanted)}`];
};
const opened = (session: Session): readonly unknown[] => {
    const { units, documents: rows, fonts = [] } = session.item.precondition;
    Reflect.set(app.scriptPreferences, 'measurementUnit', session.codec.revived(units.script));
    const docs = rows.map((row, index) => {
        const doc: unknown = 'open' in row ? app.open(row.open, true) : Reflect.apply(app.documents.add, app.documents, [true, undefined, session.codec.revived(row.properties)]);
        session.documents.push(doc);
        invoked(doc, 'insertLabel', [session.batch.label, session.batch.records]);
        if ('open' in row) {
            Object.assign(new Object(field(doc, 'viewPreferences')), { horizontalMeasurementUnits: session.codec.revived(units.horizontal), verticalMeasurementUnits: session.codec.revived(units.vertical) });
        }
        const wrong = 'open' in row ? [] : differing(session.codec, doc, row.properties, `document ${index}`);
        if (wrong.length > 0) {
            throw new Error(wrong.join(', '));
        }
        return doc;
    });
    const missing = Array.filterMap(fonts, (name) => {
        const font = app.fonts.itemByName(name);
        return font.isValid && indesign.FontStatus.INSTALLED.equals(font.status) ? Result.failVoid : Result.succeed(`${name} ${font.isValid ? String(font.status) : 'absent'}`);
    });
    if (missing.length > 0) {
        throw new Error(`fonts not installed: ${missing.join(', ')}`);
    }
    if (docs.length > 1) {
        Reflect.set(app, 'activeDocument', docs[0]);
    }
    return docs;
};
const state = (codec: Codec, doc: unknown) => {
    const nested = (name: string) =>
        pipe(
            Result.try(() => field(doc, name)),
            Result.getSuccess,
            Option.filter((value) => Predicate.isObject(value) && !owned(value) && !('id' in value) && Predicate.isObject(field(value, 'properties'))),
            Option.map((value) => [name, field(value, 'properties')] as const),
        );
    return codec.encoded({
        name: field(doc, 'name'),
        ...Object.fromEntries(Iterable.getSomes(preferences(doc).map(nested))),
        cmykProfile: field(doc, 'cmykProfile'),
        rgbProfile: field(doc, 'rgbProfile'),
        appliedLanguage: field(field(field(doc, 'textDefaults'), 'appliedLanguage'), 'name'),
        fonts: { names: field(invoked(field(doc, 'fonts'), 'everyItem', []), 'name'), status: field(invoked(field(doc, 'fonts'), 'everyItem', []), 'status') },
        links: { names: field(invoked(field(doc, 'links'), 'everyItem', []), 'name'), status: field(invoked(field(doc, 'links'), 'everyItem', []), 'status') },
    });
};
const reads = (session: Session, fake: unknown, docs: readonly unknown[], made: Members) => {
    const [doc] = docs;
    const unit = app.scriptPreferences.measurementUnit;
    const frames = docs.filter((held) => field(held, 'isValid') === true).map((held) => [held, field(field(held, 'viewPreferences'), 'rulerOrigin'), field(held, 'zeroPoint')] as const);
    app.scriptPreferences.measurementUnit = indesign.MeasurementUnits.POINTS;
    frames.forEach(([held]) => {
        Object.assign(new Object(field(held, 'viewPreferences')), { rulerOrigin: indesign.RulerOrigin.PAGE_ORIGIN });
        Reflect.set(new Object(held), 'zeroPoint', [0, 0]);
    });
    try {
        const [read, failed] = Record.partition(session.item.read, (expression) => Result.try({ try: () => given(fake, expression, doc, made), catch: message }));
        return { state: session.codec.record(read), failed };
    } finally {
        frames.forEach(([held, origin, zero]) => {
            Object.assign(new Object(field(held, 'viewPreferences')), { rulerOrigin: origin });
            Reflect.set(new Object(held), 'zeroPoint', zero);
        });
        app.scriptPreferences.measurementUnit = unit;
    }
};
const prepare = (fake: unknown, doc: unknown, session: Session, made: Members) => {
    const { labels = {}, set = [], add = [], steps = [], select = [] } = session.item.precondition;
    Object.entries(labels).forEach(([label, value]) => invoked(doc, 'insertLabel', [label, value]));
    set.forEach(([target, value]) => assign(fake, target, session.codec.revived(value), doc, made));
    add.forEach((row) => Object.assign(made, { [row.as]: invoked(field(given(fake, row.to, doc, made), row.kind), 'add', [session.codec.revived(row.properties ?? {})]) }));
    steps.forEach((step) => given(fake, step, doc, made, true));
    const picks = select.map((pick) => given(fake, pick, doc, made)).filter(Predicate.isObject);
    if (picks.length > 0) {
        app.select(picks);
    }
};
const exported = (session: Session, docs: readonly unknown[], moment: string) =>
    session.item.precondition.idml === true ? docs.forEach((doc, index) => (field(doc, 'isValid') === true ? invoked(doc, 'exportFile', [indesign.ExportFormat.INDESIGN_MARKUP, `${session.item.folder}/${index}.${moment}.idml`]) : undefined)) : undefined;

// --- [EXPORTS] -------------------------------------------------------------------------

export { exported, given, opened, prepare, reads, state };
