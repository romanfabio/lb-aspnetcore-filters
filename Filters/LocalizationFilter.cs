namespace lb_aspnetcore_filters.Filters;

public class LocalizationFilter : IEndpointFilter
{
    public ValueTask<object?> InvokeAsync(EndpointFilterInvocationContext context, EndpointFilterDelegate next)
    {
        var language = "en";

        if (context.HttpContext.Request.Query.TryGetValue("lang", out var lang))
        {
            language = lang.ToString();
        }
        
        context.HttpContext.Items.Add("lang", language);

        return next(context);
    }
}