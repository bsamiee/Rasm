namespace Lab.F11;

internal sealed record Request(string Account, decimal Amount);

internal sealed record ValidRequest(string Account, decimal Amount);

internal sealed record Model(string Account, decimal Balance, decimal Amount);

internal sealed record UpdatedModel(string Account, decimal Balance);

internal static class Workflow {
    public static Fin<ValidRequest> Validate(Request request) =>
        request.Amount > 0 ? new ValidRequest(request.Account, request.Amount) : Error.New("amount must be positive");

    public static Fin<Model> Load(ValidRequest request) =>
        string.Equals(request.Account, "ACC-1", StringComparison.Ordinal) ? new Model(request.Account, 100m, request.Amount) : Error.New("account not found");

    public static Fin<UpdatedModel> Update(Model model) =>
        model.Balance >= model.Amount ? new UpdatedModel(model.Account, model.Balance - model.Amount) : Error.New("insufficient funds");

    public static Fin<Unit> Save(UpdatedModel model) =>
        model.Balance <= 1_000_000m ? unit : Error.New("balance exceeds the reporting limit");

    public static Fin<Unit> Handle(Request request) =>
        Validate(request)
            .Bind(Load)
            .Bind(Update)
            .Bind(Save);
}
