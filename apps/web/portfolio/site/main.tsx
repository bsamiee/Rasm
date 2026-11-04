import { hydrate } from '@rasm/hosting/browser';
import '#/site/theme.css';
import { shell } from '#/site/shell.tsx';

// --- [COMPOSITION] ---------------------------------------------------------------------

hydrate(shell);
