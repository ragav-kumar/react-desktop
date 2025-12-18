namespace ReactDesktop.Rpc;

public sealed record EmptyDto;
public sealed record ConnectionStringDto(string ConnectionString);

public sealed record GetLogLinesParamsDto(int Skip, int Take);

public sealed record WriteLogLineParamsDto(string Message);
