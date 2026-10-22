import { hydratePage } from '@rasm/hosting/client';
import { shell } from '#/model/shell';
import '#/site/theme.css';
import { renderSite } from '#/site/render-site';

// --- [COMPOSITION] ---------------------------------------------------------------------

hydratePage(shell, renderSite);
