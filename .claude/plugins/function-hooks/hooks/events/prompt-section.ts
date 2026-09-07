// prompt.section, one named section of the system prompt (PromptSectionInput, PromptSectionResult), cached per session, no section row yet

// --- [IMPORTS] -------------------------------------------------------------------------

import type { On } from 'claude-code';
import type { Options } from '../host/options.ts';

// --- [REGISTRATION] --------------------------------------------------------------------

const promptSection = (_on: On, _options: Options): void => undefined;

// --- [EXPORTS] -------------------------------------------------------------------------

export { promptSection };
