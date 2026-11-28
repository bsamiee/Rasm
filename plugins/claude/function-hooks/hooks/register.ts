import { enforce } from '@rasm/claude-code/enforcement';
import { git } from '@rasm/policy/git';
import { Policy } from '@rasm/policy/policy';
import type { On } from 'claude-code';
import { ghidra } from '#/rules/ghidra';
import { programs } from '#/rules/programs';
import { removal } from '#/rules/removal';
import { search } from '#/rules/search';
import { tasks } from '#/rules/tasks';
import { waits } from '#/rules/waits';

// --- [COMPOSITION] ---------------------------------------------------------------------

function register(on: On) {
    enforce(on, new Policy(git, search, removal, programs, waits, tasks, ghidra));
}

// --- [EXPORTS] -------------------------------------------------------------------------

export { register };
