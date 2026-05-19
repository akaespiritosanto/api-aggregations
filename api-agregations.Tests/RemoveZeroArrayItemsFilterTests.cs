using api_aggregations.Filters;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Abstractions;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Routing;

namespace api_agregations.Tests;

public class RemoveZeroArrayItemsFilterTests
{
    [Fact]
    public void OnActionExecuted_RemovesItemsWithZeroFields_FromReturnedLists()
    {
        var returnedItems = new List<TestItem>
        {
            new TestItem { id = 0, name = "remove", valor = 10 },
            new TestItem { id = 1, name = "keep", valor = 0 }
        };

        var context = CreateActionExecutedContext(returnedItems);
        var filter = new RemoveZeroArrayItemsFilter();

        filter.OnActionExecuted(context);

        Assert.Single(returnedItems);
        Assert.Equal(1, returnedItems[0].id);
    }

    private static ActionExecutedContext CreateActionExecutedContext(object resultValue)
    {
        var httpContext = new DefaultHttpContext();
        var actionContext = new ActionContext(
            httpContext,
            new RouteData(),
            new ActionDescriptor());

        return new ActionExecutedContext(
            actionContext,
            new List<IFilterMetadata>(),
            controller: new object())
        {
            Result = new ObjectResult(resultValue)
        };
    }

    private sealed class TestItem
    {
        public required int id { get; init; }
        public required string name { get; init; }
        public required decimal valor { get; init; }
    }
}
