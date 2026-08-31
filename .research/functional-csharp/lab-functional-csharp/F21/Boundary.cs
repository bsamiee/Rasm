namespace Lab.F21;

internal sealed record Ticket(string Holder);

internal static class Boundary {
    public static Fin<Ticket> Issue(Person person) => person.Age >= 18 ? new Ticket(person.Name) : new Underage();

    public static Fin<Ticket> Handle(string name, int years) => Concerns.Register(name, years).ToFin().Bind(Issue);

    public static string Respond(string name, int years) => Handle(name, years).Match(Succ: static ticket => ticket.Holder, Fail: static error => error.Message);
}
