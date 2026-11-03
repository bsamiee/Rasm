import { hydrate } from '@rasm/hosting/page/hydrate';
import '#/site/theme.css';
import { shell } from '#/site/shell.tsx';

// --- [COMPOSITION] ---------------------------------------------------------------------

hydrate(shell);
