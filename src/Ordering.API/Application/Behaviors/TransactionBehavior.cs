namespace eShop.Ordering.API.Application.Behaviors;

using Microsoft.Extensions.Logging;

public class TransactionBehavior<TRequest, TResponse>(
    OrderingContext dbContext,
    IOrderingIntegrationEventService orderingIntegrationEventService,
    ILogger<TransactionBehavior<TRequest, TResponse>> logger)
    : IPipelineBehavior<TRequest, TResponse> where TRequest : IRequest<TResponse>
{
    public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
    {
        var response = default(TResponse);
        var typeName = request.GetGenericTypeName();

        try
        {
            if (dbContext.HasActiveTransaction)
            {
                return await next();
            }

            var strategy = dbContext.Database.CreateExecutionStrategy();

            await strategy.ExecuteAsync(async () =>
            {
                Guid transactionId;

                await using var transaction = await dbContext.BeginTransactionAsync();
                using (logger.BeginScope(new List<KeyValuePair<string, object>> { new("TransactionContext", transaction.TransactionId) }))
                {
                    logger.LogInformation("Begin transaction {TransactionId} for {CommandName} ({@Command})", transaction.TransactionId, typeName, request);

                    response = await next();

                    logger.LogInformation("Commit transaction {TransactionId} for {CommandName}", transaction.TransactionId, typeName);

                    await dbContext.CommitTransactionAsync(transaction);

                    transactionId = transaction.TransactionId;
                }

                await orderingIntegrationEventService.PublishEventsThroughEventBusAsync(transactionId);
            });

            return response;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error Handling transaction for {CommandName} ({@Command})", typeName, request);

            throw;
        }
    }
}
