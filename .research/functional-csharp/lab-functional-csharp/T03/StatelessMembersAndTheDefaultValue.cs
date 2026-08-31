namespace Lab.T03;

internal sealed record SuccessResponse(string Data);
internal readonly record struct NotFoundError;

[Union<SuccessResponse, NotFoundError>(T1Name = "Success", T2Name = "NotFound", T2IsStateless = true)]
internal sealed partial class ApiResponse;
