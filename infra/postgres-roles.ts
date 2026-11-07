import { Extension, Grant, Provider, type ProviderArgs, Role, type RoleArgs, Schema } from '@pulumi/postgresql';
import { type Input, interpolate, type Output } from '@pulumi/pulumi';
import { RandomPassword } from '@pulumi/random';

// --- [OPERATIONS] ----------------------------------------------------------------------

const postgresRoles = (database: string, variable: string, server: Required<Pick<ProviderArgs, 'host' | 'port' | 'password'>>, endpoint: Input<string>, protect: boolean): Readonly<Record<string, Output<string>>> => {
    const catalog = { database: 'postgres' } as const;
    const provider = new Provider(database, { ...server, ...catalog, username: catalog.database, sslmode: 'disable' });
    const login = (role: string, settings: RoleArgs) => {
        const key = new RandomPassword(`${database}-${role}`, { length: 40, special: false }).result;
        return { name: new Role(`${database}-${role}`, { name: `${database}-${role}`, login: true, password: key, ...settings }, { provider }).name, key };
    };
    const migrator = login('migrator', { idleInTransactionSessionTimeout: 60_000 });
    const schema = new Schema(database, { name: migrator.name, owner: migrator.name, ...catalog }, { provider, protect });
    const runtime = login('runtime', { statementTimeout: 15_000, idleInTransactionSessionTimeout: 30_000, searchPaths: [schema.name] });
    const statistics = new Extension(`${database}-pg-stat-statements`, { name: 'pg_stat_statements', ...catalog }, { provider });
    const url = (user: Input<string>, key: Input<string>) => interpolate`postgres://${user}:${key}@${endpoint}/${catalog.database}`;
    return {
        [variable]: url(new Grant(`${database}-runtime`, { ...catalog, role: runtime.name, objectType: 'schema', schema: schema.name, privileges: ['USAGE'] }, { provider }).role, runtime.key),
        [`${variable}_MIGRATOR`]: url(new Grant(`${database}-migrator`, { ...catalog, role: schema.owner, objectType: 'database', privileges: ['CREATE'] }, { provider, dependsOn: statistics }).role, migrator.key),
    };
};

// --- [EXPORTS] -------------------------------------------------------------------------

export { postgresRoles };
