import { Database, DefaultPrivileges, Extension, Provider, type ProviderArgs, Role, type RoleArgs } from '@pulumi/postgresql';
import { all, type Input, type Output } from '@pulumi/pulumi';
import { RandomPassword } from '@pulumi/random';
import { Record } from 'effect';

// --- [OPERATIONS] ----------------------------------------------------------------------

const postgresRoles = (database: string, variable: string, server: Required<Pick<ProviderArgs, 'host' | 'port' | 'username' | 'password'>>, endpoints: Readonly<Record<'runtime' | 'migrator', URL>>, protect: boolean): Readonly<Record<string, Output<string>>> => {
    const provided = { provider: new Provider(database, { ...server, sslmode: 'disable' }) };
    const login = (role: keyof typeof endpoints, settings: RoleArgs) => {
        const key = new RandomPassword(`${database}-${role}`, { length: 40, special: false }).result;
        return { name: new Role(`${database}-${role}`, { name: `${database}-${role}`, login: true, password: key, ...settings }, provided).name, key };
    };
    const migrator = login('migrator', { idleInTransactionSessionTimeout: 60_000 });
    const runtime = login('runtime', { statementTimeout: 15_000, idleInTransactionSessionTimeout: 30_000 });
    const owned = new Database(database, { name: database, owner: migrator.name }, { ...provided, protect });
    const statistics = new Extension(database, { name: 'pg_stat_statements', database: owned.name }, provided);
    const granted = Record.map({ table: ['SELECT', 'INSERT', 'UPDATE', 'DELETE'], sequence: ['USAGE', 'SELECT'] }, (privileges, objectType) => new DefaultPrivileges(`${database}-${objectType}`, { database: owned.name, owner: migrator.name, role: runtime.name, objectType, privileges }, provided));
    const url = (endpoint: URL, role: Input<string>, key: Input<string>) =>
        all([role, key, owned.name]).apply(([username, password, pathname]) => {
            const target = Object.assign(new URL(endpoint), { username, password, pathname });
            target.searchParams.set('sslmode', 'disable');
            return target.href;
        });
    return {
        [variable]: url(
            endpoints.runtime,
            all([granted.table.role, granted.sequence.role]).apply(([role]) => role),
            runtime.key,
        ),
        [`${variable}_MIGRATOR`]: url(
            endpoints.migrator,
            all([migrator.name, statistics.name]).apply(([role]) => role),
            migrator.key,
        ),
    };
};

// --- [EXPORTS] -------------------------------------------------------------------------

export { postgresRoles };
