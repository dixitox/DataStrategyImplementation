namespace Microsoft.DataStrategy.Api
{
    public static class AlwaysOn
    {
        /// <summary>
        /// Prevent 404 on the application root URL. Azure Web Apps with Always On enabled invoke the root of the application every 5 minutes
        /// to keep your web app awake. If you run an API-only app you likely have nothing listening to /, which results in a 404 response
        /// that shows up in monitoring. This endpoint returns 200 with content Ok only for the always on check.
        /// </summary>
        /// <param name="builder">The application builder.</param>
        /// <returns>The application builder.</returns>
        public static IApplicationBuilder UseAlwaysOn(this IApplicationBuilder builder)
        {
            builder.Use(next => async context =>
            {
                if (context.Request.Path == "/" && context.Request.Headers.TryGetValue("user-agent", out var value) && value.Equals("AlwaysOn"))
                {
                    await context.Response.WriteAsync("Ok");
                    return;
                }

                await next(context);
            });

            return builder;
        }
    }
}
