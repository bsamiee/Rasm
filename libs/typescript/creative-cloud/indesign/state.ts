import type { Document } from 'adobe:indesign';
import indesign from 'adobe:indesign';
import { host } from 'adobe:uxp';
import { Array, Effect, Option, Predicate, Schema, Struct } from 'effect';
import type { Rpc } from 'effect/rpc';
import { Inspect, Menu, Preflight } from './contract.ts';
import { active, inspect as described, missing, read, scope } from './dom.ts';

// --- [OPERATIONS] ----------------------------------------------------------------------

const problems = Effect.fn('problems')(function* (document: Document, named: Option.Option<string>) {
    const { app } = indesign;
    const seconds = 30;
    const [profiles, working] = [app.preflightProfiles.everyItem().getElements(), document.preflightOptions.preflightWorkingProfile];
    const name = Option.getOrElse(named, () => (Predicate.isString(working) ? working : working.name));
    const profile = yield* Effect.fromOption(
        Array.findFirst(profiles, (each) => each.name === name),
        () => missing(`preflight profile ${name}`, name, Array.map(profiles, Struct.get('name'))),
    );
    const process = yield* Effect.acquireRelease(
        Effect.sync(() => app.preflightProcesses.add(document, profile)),
        (added) => Effect.sync(() => added.remove()),
    );
    return yield* read(Preflight.successSchema, () => {
        process.waitForProcess(seconds);
        const [, , rows] = process.aggregatedResults;
        return {
            profile: name,
            rules: profile.preflightProfileRules.everyItem().name,
            problems: Array.flatten(
                Array.mapAccum(rows, '', (rule, [, label, page, , details]) =>
                    Array.match(details, {
                        onEmpty: () => [label, []],
                        onNonEmpty: ([[, problem], ...rest]) => [rule, [{ rule, problem, page, fix: Option.getOrUndefined(Option.map(Array.head(rest), ([, fix]) => fix)) }]],
                    }),
                )[1],
            ),
        };
    });
}, Effect.scoped);
const state = Effect.fn('state')(function* () {
    const { app, StyleType } = indesign;
    const { State } = Inspect.successSchema.cases;
    const modal = yield* read(Schema.Boolean, () => app.modalState);
    yield* modal ? Effect.void : scope;
    const documents = modal ? [] : app.documents.everyItem().getElements();
    const paths = yield* Effect.forEach(documents, (document) => (document.saved ? Effect.asSome(Effect.promise(() => document.fullName)) : Effect.succeedNone));
    const live = yield* Effect.transposeOption(
        Option.map(
            Option.filter(Option.liftThrowable(() => app.activeDocument)(), () => !modal),
            (document) => Effect.map(problems(document, Option.none()), (found) => ({ document, found })),
        ),
    );
    return yield* read(State, () => ({
        _tag: 'State',
        version: host.version,
        status: modal
            ? { _tag: 'Modal' }
            : {
                  _tag: 'Ready',
                  tool: app.toolBoxTools.currentTool,
                  panels: Array.flatMap(
                      Array.makeBy(app.panels.length, (index) => app.panels.item(index)),
                      (panel) => Option.getOrElse(Option.liftThrowable(() => (panel.visible ? [panel.name] : []))(), () => []),
                  ),
                  documents: Array.zipWith(documents, paths, (document, path) => ({
                      specifier: document,
                      name: document.name,
                      modified: document.modified,
                      saved: document.saved,
                      path: Option.getOrUndefined(Option.map(path, Struct.get('nativePath'))),
                      windows: document.windows.everyItem().getElements(),
                  })),
                  active: Option.getOrUndefined(
                      Option.map(live, ({ document, found }) => {
                          const stories = document.stories.everyItem();
                          const text = (item: object, name: string): string | undefined => Option.getOrUndefined(Schema.decodeUnknownOption(Schema.String)(Reflect.get(item, name)));
                          return {
                              document,
                              window: app.activeWindow,
                              selection: Array.map(app.selection, (item) => ({
                                  type: item.constructorName,
                                  specifier: item,
                                  name: text(item, 'name'),
                                  contents: text(item, 'contents'),
                                  story: 'parentStory' in item ? item.parentStory : undefined,
                                  overrides: 'textHasOverrides' in item ? Option.getOrUndefined(Option.liftThrowable(() => ({ paragraph: item.textHasOverrides(StyleType.PARAGRAPH_STYLE_TYPE), character: item.textHasOverrides(StyleType.CHARACTER_STYLE_TYPE) }))()) : undefined,
                              })),
                              overset: Array.flatMap(Array.zip(stories.overflows, stories.id), ([overflows, id]) => (overflows === true ? [document.stories.itemByID(id)] : [])),
                              preflight: { on: !document.preflightOptions.preflightOff, profile: found.profile, problems: found.problems.length },
                              undo: { mode: app.activeScriptUndoMode, name: document.undoName, history: document.undoHistory },
                          };
                      }),
                  ),
              },
    }));
}, Effect.scoped);
const inspect = Effect.fnUntraced(function* ({ target, fields }: Rpc.Payload<typeof Inspect>) {
    return yield* Option.match(target, { onNone: state, onSome: (named) => Effect.andThen(scope, described(named, fields)) });
}, Effect.scoped);
const menu = Effect.fnUntraced(function* ({ area }: Rpc.Payload<typeof Menu>) {
    yield* scope;
    return yield* read(Menu.successSchema, () => {
        const every = indesign.app.menuActions.everyItem();
        return Array.filter(
            Array.zipWith(Array.zip(every.id, every.name), every.area, ([id, name], within) => ({ id, name, area: within })),
            (action) => action.area.startsWith(Option.getOrElse(area, () => '')),
        );
    });
}, Effect.scoped);
const preflight = Effect.fnUntraced(function* ({ profile }: Rpc.Payload<typeof Preflight>) {
    yield* scope;
    return yield* problems(yield* active, profile);
}, Effect.scoped);

// --- [EXPORTS] -------------------------------------------------------------------------

export { inspect, menu, preflight };
