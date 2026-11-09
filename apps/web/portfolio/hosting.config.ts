import { App } from '@rasm/hosting/app';

// --- [COMPOSITION] ---------------------------------------------------------------------

export default App.make({ name: 'portfolio', env: { DB: App.Binding.sqlite(), BUCKET: App.Binding.bucket(), OWNER_EMAIL: App.Binding.text({ value: 'b.samiee93@gmail.com' }) } });
