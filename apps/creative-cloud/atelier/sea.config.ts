import { sea } from '../../../vite.shared.ts';

// --- [COMPOSITION] ---------------------------------------------------------------------

export default sea(import.meta.dirname, { executable: 'ATELIER_PATH', listen: 'MCP_LISTEN' });
