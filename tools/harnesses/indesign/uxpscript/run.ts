import fs from 'adobe:fs';
import indesign, { app } from 'adobe:indesign';
import { thrown } from '@rasm/harnesses/harness-error';
import { Encoded, fields, type Held, HostCase, HostOutcome, Recorded, Reply } from '@rasm/harnesses/indesign/contract';
import { Array, identity, Match, Option, Predicate, Record, Result, Schema } from 'effect';
import { exported, given, opened, prepare, reads, state } from '#/precondition';
import { installed } from '#/shadows';
import { activeOf, differences, globals, pinned, restore, scanned, snapshot, survey } from '#/state';
import { type Batch, Codec, compiled, field, type Item, invoked, type Members, message, type Scripted, type Session, Strings } from '#/values';

// --- [CONSTANTS] -----------------------------------------------------------------------

const STACK_LINE = /<anonymous>:(?<line>\d+):\d+/gu;

// --- [OPERATIONS] ----------------------------------------------------------------------

const scripted = (item: Item): item is Scripted => !HostCase.isAnyOf(['Panel'])(item);
const strings = (value: unknown): readonly string[] => Option.getOrElse(Schema.decodeUnknownOption(Strings)(value), () => []);
const lines = (error: unknown) => Array.map(Array.fromIterable(String(field(error, 'stack') ?? '').matchAll(STACK_LINE)), (found) => Number(found.groups?.line));
const HEAD = (() => {
    try {
        compiled([], 'return function () {throw new Error();\n};')()();
        return 0;
    } catch (error) {
        return (lines(error)[0] ?? 1) - 1;
    }
})();
const ran = (session: Session<Scripted>, { fake, evaluate }: ReturnType<typeof installed>, docs: readonly unknown[], made: Members) => {
    const { item } = session;
    const threw = (error: unknown) => {
        const stack = lines(error).map((line) => ({ line: line - HEAD }));
        return { _tag: 'Thrown' as const, message: thrown(error), ...(stack.length === 0 ? {} : { line: stack[0]?.line ?? 0, stack }) };
    };
    try {
        app.scriptPreferences.userInteractionLevel = item.interact ? indesign.UserInteractionLevels.INTERACT_WITH_ALL : indesign.UserInteractionLevels.NEVER_INTERACT;
        const value = (() => {
            try {
                return Match.valueTags(item, {
                    Probe: ({ evaluate: expression }) => given(fake, expression, docs[0], made),
                    Call: ({ source, call, args }) => evaluate(`${source?.code ?? ''}\n;return (${call}).apply(null, arguments);`, args.map(session.codec.revived)),
                    Run: ({ source, args }) => evaluate(source.code, args.map(session.codec.revived)),
                });
            } finally {
                app.scriptPreferences.userInteractionLevel = indesign.UserInteractionLevels.NEVER_INTERACT;
            }
        })();
        if (Predicate.isPromiseLike(value)) {
            return { completion: { _tag: 'Failed' as const, reason: 'original answered a thenable that settles after the answer, its rejection in logs' } };
        }
        session.actions.forEach((action) => {
            const handler = field(field(action, 'listeners'), 'onInvoke');
            session.record.menus.push({ title: String(action.title), invoked: Predicate.isFunction(handler) });
            if (Predicate.isFunction(handler)) {
                Reflect.apply(handler, action, [{ eventType: 'onInvoke' }]);
            }
        });
        return { completion: { _tag: 'Ran' as const }, value: session.codec.encoded(item._tag === 'Run' ? Option.getOrUndefined(session.value) : value) };
    } catch (error) {
        return { completion: threw(error) };
    }
};
const sessionOf = <I extends Item>(item: I, batch: Batch, codec: Codec): Session<I> => ({
    item,
    batch,
    codec,
    documents: [],
    confirms: [...(item.confirms ?? [])],
    prompts: [...(item.prompts ?? [])],
    files: [...(item.files ?? [])],
    actions: [],
    record: Record.map(fields, () => []),
    dialogs: [...(item.dialogs ?? [])],
    value: Option.none(),
});
const heldOf = (item: Item, index: number, batch: Batch, codec: Codec): typeof Held.Type => {
    const session = sessionOf(item, batch, codec);
    const unit = app.scriptPreferences.measurementUnit;
    try {
        const docs = opened(session);
        prepare(app, docs[0], session, {});
        return { index, opened: docs.map((doc) => state(codec, doc)) };
    } catch (error) {
        return { index, failed: message(error) };
    } finally {
        app.scriptPreferences.measurementUnit = unit;
    }
};
const outcomeOf = (item: Scripted, batch: Batch, found: ReturnType<typeof scanned>, codec: Codec): typeof HostOutcome.Type => {
    const session = sessionOf(item, batch, codec);
    const unit = app.scriptPreferences.measurementUnit;
    const active = Option.map(activeOf(), ({ id }) => id);
    const held = pinned(() => snapshot(found, codec));
    const unglobal = globals();
    const leaked: string[] = [];
    const made: Members = {};
    const attempt = (label: string, work: () => void) => {
        try {
            work();
        } catch (error) {
            leaked.push(`${label}: ${message(error)}`);
        }
    };
    const shadows = installed(session);
    const result = ((): Omit<Parameters<typeof HostOutcome.make>[0], 'name' | 'answered' | 'leaked' | keyof typeof fields> => {
        try {
            const docs = opened(session);
            session.record.opened = docs.map((doc) => state(codec, doc));
            prepare(shadows.fake, docs[0], session, made);
            const before = reads(session, shadows.fake, docs, made);
            const inner: { done?: ReturnType<typeof ran>; after?: ReturnType<typeof reads> } = {};
            const rolled = Result.try(
                app.doScript.bind(
                    app,
                    () => {
                        exported(session, docs, 'before');
                        const history = docs.map((doc) => strings(field(doc, 'undoHistory')));
                        inner.done = ran(session, shadows, docs, made);
                        session.record.undo = docs.flatMap((doc, index) => (field(doc, 'isValid') === true ? strings(field(doc, 'undoHistory')).slice(0, strings(field(doc, 'undoHistory')).length - (history[index]?.length ?? 0)) : []));
                        inner.after = reads(session, shadows.fake, docs, made);
                        session.record.writes = pinned(() => differences(held, codec)).map(({ member, before: previous, after: now }) => (previous === undefined ? { member, after: codec.encoded(now) } : { member, before: codec.encoded(previous), after: codec.encoded(now) }));
                        exported(session, docs, 'after');
                        throw new Error('rollback');
                    },
                    indesign.ScriptLanguage.UXPSCRIPT,
                    [],
                    indesign.UndoModes.ENTIRE_SCRIPT,
                    item.name,
                ),
            );
            const { done, after } = Option.getOrThrowWith(Option.all({ done: Option.fromUndefinedOr(inner.done), after: Option.fromUndefinedOr(inner.after) }), () => Result.match(rolled, { onFailure: identity, onSuccess: () => new Error('case did not finish') }));
            return { ...done, before: before.state, beforeFailed: before.failed, read: after.state, readFailed: after.failed };
        } catch (error) {
            return { completion: { _tag: 'Failed', reason: message(error) } };
        }
    })();
    attempt('shadows', shadows.release);
    session.documents.forEach((doc) => attempt('close', () => (field(doc, 'isValid') === true ? invoked(doc, 'close', field(doc, 'constructorName') === 'Library' ? [] : [indesign.SaveOptions.NO]) : undefined)));
    leaked.push(...session.documents.filter((doc) => field(doc, 'isValid') === true).map((doc) => `document ${String(field(doc, 'name'))} stays open after close`));
    attempt('scriptPreferences', () => Object.assign(app.scriptPreferences, { measurementUnit: unit }));
    attempt('restore', () => leaked.push(...restore(held, codec)));
    attempt('globals', () => leaked.push(...unglobal()));
    attempt('activeDocument', () =>
        Option.map(
            Option.filter(active, (id) => app.documents.length > 0 && app.activeDocument.id !== id),
            (id) => Object.assign(app, { activeDocument: app.documents.itemByID(id) }),
        ),
    );
    return HostOutcome.make({ name: item.name, ...result, ...session.record, leaked });
};
const written = Schema.encodeResult(Recorded);
const answered = (batch: Batch): typeof Reply.Type => {
    const level = app.scriptPreferences.userInteractionLevel;
    app.scriptPreferences.userInteractionLevel = indesign.UserInteractionLevels.NEVER_INTERACT;
    try {
        app.documents
            .everyItem()
            .getElements()
            .filter((doc) => !app.modalState && doc.extractLabel(batch.label) !== '')
            .forEach((doc) => doc.close(indesign.SaveOptions.NO));
        const found = scanned();
        const coder = Codec(batch.enumerations);
        const settings = app.colorSettings;
        const header = {
            application: app.name,
            version: app.version,
            state: { modal: app.modalState, cmyk: settings.workingSpaceCMYK, rgb: settings.workingSpaceRGB, defaults: coder.record(Record.fromIterableWith(found.preferences, (name) => [name, field(field(app, name), 'properties')])), unread: found.unread },
            ...(batch.survey ? { survey: survey() } : {}),
        };
        const appended = (line: string) => fs.writeFileSync(batch.records, `${String(fs.readFileSync(batch.records, { encoding: 'utf-8' }))}${line}\n`);
        const [run, after] = Array.span(batch.cases.slice(batch.from), scripted);
        const started = (at: number) => appended(Result.getOrThrow(written({ _tag: 'Started', index: at })));
        run.forEach((item, at) => {
            started(batch.from + at);
            appended(Result.getOrThrow(Result.orElse(written({ _tag: 'Finished', outcome: outcomeOf(item, batch, found, coder) }), ({ message: reason }) => written({ _tag: 'Finished', outcome: HostOutcome.make({ name: item.name, completion: { _tag: 'Failed', reason } }) }))));
        });
        const index = batch.from + run.length;
        const held = Option.map(Array.head(after), (item) => {
            started(index);
            return heldOf(item, index, batch, coder);
        });
        return { ...header, ...Record.getSomes({ held }) };
    } catch (error) {
        return { failed: message(error) };
    } finally {
        app.scriptPreferences.userInteractionLevel = level;
    }
};
const answerBatch = (argument: unknown) => Result.getOrThrow(Schema.encodeResult(Reply)(Result.match(Schema.decodeUnknownResult(Encoded)(argument), { onFailure: ({ message: failed }) => ({ failed }), onSuccess: answered })));

// --- [EXPORTS] -------------------------------------------------------------------------

export { answerBatch };
