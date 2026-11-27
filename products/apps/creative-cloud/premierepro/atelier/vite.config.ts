import { pluginConfig } from '@rasm/uxp/vite';
import { defineConfig } from 'vite';

// --- [COMPOSITION] ---------------------------------------------------------------------

export default defineConfig(pluginConfig({ harness: new URL('../../../../../plugins/shared/uxp/.mcp.json', import.meta.url), name: 'atelier' }));
