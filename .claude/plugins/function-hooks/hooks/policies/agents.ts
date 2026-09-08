// The brief appended to a spawned agent's prompt by its type

// --- [CONSTANTS] -----------------------------------------------------------------------

// The lines every brief of the type shares, one sentence per line, the prompt holds the task alone
const AGENTS: Readonly<Partial<Record<string, readonly string[]>>> = {
    fork: [
        'Never rewrite a whole file in one move: read the file whole, write one scoped change, read the result.',
        'Read the skill the step names and the reference it touches before the first edit, apply each change as an exact-string replacement that asserts one match, and land any defect you meet in the same file in the same step.',
        'Write in clean-prose.',
        'Report result: done, partial, or not started, changes: with file and line, open: with the evidence, gate: with each check the task names and its result line, and suggestions: on the brief or a preloaded skill, in at most 12 lines.',
    ],
    'general-purpose': [
        'Never rewrite a whole file in one move: read, one scoped change, read.',
        'Correct only what you can justify, and report result: done, partial, or not started, changes: with file, line, and reason, open: with the evidence, gate: with each check the task names and its result line, and suggestions: on the brief or a preloaded skill, in at most 20 lines.',
    ],
};

// --- [OPERATIONS] ----------------------------------------------------------------------

// The prompt with the type's brief appended, unchanged for a type without one
const briefed = (subagentType: string, prompt: string): string => {
    const brief = AGENTS[subagentType];
    return brief === undefined ? prompt : [prompt, '', ...brief].join('\n');
};

// --- [EXPORTS] -------------------------------------------------------------------------

export { AGENTS, briefed };
