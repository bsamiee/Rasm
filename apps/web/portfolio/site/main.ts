import { hydratePage } from '@rasm/hosting/rendering/client.ts';
import { shell } from '../model/shell.ts';
import './theme.css';
import { renderSite } from './render-site.tsx';

// --- [COMPOSITION] ---------------------------------------------------------------------

hydratePage(shell, renderSite);
