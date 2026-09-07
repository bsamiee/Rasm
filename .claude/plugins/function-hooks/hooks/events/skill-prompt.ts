// skill.prompt stamps every loaded skill under loaded/<session>/<skill> and skill/<skill>, and the ast-grep skill takes its live tree facts

// --- [IMPORTS] -------------------------------------------------------------------------

import type { FsEntry, On, SkillPromptResult } from 'claude-code';
import { forEach, map, toArray } from '../composition/option.ts';
import type { Options } from '../host/options.ts';
import { decodeSession, type Entry, type Environment, key, keys, type Session, stamp } from '../host/store.ts';
import {
    abort,
    dirs,
    type Facts,
    factsBlock,
    family,
    GRAMMAR_PATH,
    packages,
    type Run,
    ruleFile,
    rulePath,
    stale,
    TREE,
    telemetryBlock,
    type Utils,
    ymlIds,
} from '../policies/scan.ts';

// --- [REGISTRATION] --------------------------------------------------------------------

const _stamps = (on: On): void => {
    on('skill.prompt', async ($, e, next) => {
        const session = await $.session.id();
        await Promise.all([
            $.store.set(key('loaded', session, e.skill), stamp(session, $.clock.now())),
            $.store.set(key('skill', e.skill), { loadedAt: $.clock.now(), session }),
        ]);
        return next(e);
    });
};

// The live tree facts open the ast-grep skill text and the edit-time hit telemetry closes it
const _blocks = (on: On): void => {
    on('skill.prompt', { skill: 'ast-grep' }, async ($, e, next) => {
        const [session, all] = await Promise.all([$.session.id(), $.store.keys()]);
        // The session row holds the environment the version run needs, and the skill text passes through without it
        const env = map((row: Session) => row.env)(decodeSession(await $.store.get(key('session', session))));
        const list = (dir: string): Promise<readonly FsEntry[]> => $.fs.listDir(dir);
        const utils = async (): Promise<readonly Utils[]> =>
            Promise.all(
                dirs(await list(TREE.utils)).map(async (language) => ({ language, count: ymlIds(await list(`${TREE.utils}/${language}`)).length })),
            );
        const [version, grammar, rulePackages, rewrites, utilLanguages, entries] = await Promise.all([
            forEach((found: Environment) => $.process.run(['ast-grep', '--version'], { env: found }).catch(abort))(env),
            $.fs.exists(GRAMMAR_PATH),
            packages(TREE.rules, list),
            packages(TREE.rewrites, list),
            utils(),
            Promise.all(keys('scan')(all).map(async (name): Promise<Entry> => ({ key: name, value: await $.store.get(name) }))),
        ]);
        const rules = await Promise.all(
            rulePackages.flatMap((group) => group.ids.map(async (id) => ruleFile(group, id, (await $.fs.stat(rulePath(group, id))).mtimeMs))),
        );
        const now = $.clock.now();
        await Promise.all(stale(entries, rules, now).map((name) => $.store.delete(name)));
        return version.match<Promise<SkillPromptResult>>({
            some: (run: Run) => {
                const facts: Facts = { version: run, grammar, families: rulePackages.map(family), utils: utilLanguages, rewrites };
                return next({ ...e, text: [factsBlock(facts), e.text, ...toArray(telemetryBlock(entries, rules, now))].join('\n\n') });
            },
            none: () => next(e),
        });
    });
};

// The plain hook stamps every skill, the matched one hands the ast-grep skill its live facts
const skillPrompt = (on: On, _options: Options): void => {
    _stamps(on);
    _blocks(on);
};

// --- [EXPORTS] -------------------------------------------------------------------------

export { skillPrompt };
