// skill.prompt stamps every loaded skill under loaded/<session>/<skill> and skill/<skill>, and the ast-grep skill takes its live tree facts

// --- [IMPORTS] -------------------------------------------------------------------------

import type { FsEntry, On } from 'claude-code';
import { decode, type Entry, isStringRecord, key, keys } from '../host/store.ts';
import { abort, factsBlock, GRAMMAR_PATH, packages, TREE, telemetryBlock } from '../policies/scan.ts';

// --- [REGISTRATION] --------------------------------------------------------------------

const _stamps = (on: On): void => {
    on('skill.prompt', async ($, e, next) => {
        const session = await $.session.id();
        await Promise.all([
            $.store.set(key('loaded', session, e.skill), $.clock.now()),
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
        const env = decode(isStringRecord)(await $.store.get(key('session', session)));
        if (env === undefined) {
            return next(e);
        }
        const list = (dir: string): Promise<readonly FsEntry[]> => $.fs.listDir(dir);
        const [version, grammar, rules, rewrites, utils, entries] = await Promise.all([
            $.process.run(['ast-grep', '--version'], { env }).catch(abort),
            $.fs.exists(GRAMMAR_PATH),
            packages(TREE.rules, list),
            packages(TREE.rewrites, list),
            packages(TREE.utils, list),
            Promise.all(keys('scan')(all).map(async (name): Promise<Entry> => ({ key: name, value: await $.store.get(name) }))),
        ]);
        const telemetry = telemetryBlock(
            entries,
            rules.flatMap((group) => group.ids),
            $.clock.now(),
        );
        return next({
            ...e,
            text: [factsBlock({ version, grammar, rules, rewrites, utils }), e.text, ...(telemetry === undefined ? [] : [telemetry])].join('\n\n'),
        });
    });
};

// The plain hook stamps every skill, the matched one hands the ast-grep skill its live facts
const skillPrompt = (on: On): void => {
    _stamps(on);
    _blocks(on);
};

// --- [EXPORTS] -------------------------------------------------------------------------

export { skillPrompt };
