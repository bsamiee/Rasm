namespace Lab.F07;

internal sealed record SqlTemplate(string Text);

internal sealed record Employee(Guid Id, string LastName);

internal interface ConnectionIO {
    public Seq<T> Query<T>(SqlTemplate template, object parameters);
}

internal static class Queries {
    public static readonly SqlTemplate EmployeeById = new("select * from employee where id = @Id");

    public static Func<SqlTemplate, object, Eff<RT, Seq<T>>> Query<RT, T>() where RT : Has<Eff<RT>, ConnectionIO> =>
        static (template, parameters) => Has<Eff<RT>, RT, ConnectionIO>.ask.As().Map(connection => connection.Query<T>(template, parameters));
}

internal static class Lookups<RT> where RT : Has<Eff<RT>, ConnectionIO> {
    private static readonly Func<SqlTemplate, object, Eff<RT, Seq<Employee>>> QueryEmployees = Queries.Query<RT, Employee>();
    private static readonly Func<object, Eff<RT, Seq<Employee>>> QueryById = par(QueryEmployees, Queries.EmployeeById);
    public static readonly Func<Guid, Eff<RT, Option<Employee>>> LookupEmployee = static id => QueryById(new { Id = id }).Map(static rows => rows.Head);
}
