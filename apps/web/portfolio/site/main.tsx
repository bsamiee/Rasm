import { hydratePage } from '@rasm/hosting/page/hydration';
import '#/site/theme.css';
import { shell } from '#/site/shell';

// --- [COMPOSITION] ---------------------------------------------------------------------

hydratePage(shell);
