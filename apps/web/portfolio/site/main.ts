import { hydrate } from '@rasm/hosting/hydrate';
import { Effect } from 'effect';
import '#/site/theme.css';
import { shell } from '#/site/shell.tsx';

// --- [COMPOSITION] ---------------------------------------------------------------------

Effect.runSync(hydrate(shell));
