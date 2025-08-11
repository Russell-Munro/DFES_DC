using GraphQL;
using Microsoft.Extensions.Options;
using System;

public class GraphQLExecutionOptionsConfigurator : IConfigureOptions<ExecutionOptions>
{
    private readonly ILogger<GraphQLExecutionOptionsConfigurator> _logger;

    public GraphQLExecutionOptionsConfigurator(ILogger<GraphQLExecutionOptionsConfigurator> logger)
    {
        _logger = logger;
    }

    public void Configure(ExecutionOptions options)
    {
        options.UnhandledExceptionDelegate = context =>
        {
            _logger.LogError(context.Exception, "Unhandled GraphQL error: {Message}", context.Exception.Message);
            return Task.CompletedTask;
        };
    }
}
