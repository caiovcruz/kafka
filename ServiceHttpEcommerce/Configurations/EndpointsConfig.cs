using Microsoft.AspNetCore.Mvc;
using ServiceNewOrder;
using ServiceReports;
using System.ComponentModel.DataAnnotations;

namespace ServiceHttpEcommerce.Configurations
{
    public static class EndpointsConfig
    {
        public static void RegisterEndpoints(this WebApplication app)
        {
            app.MapPost("/order", async ([Required] Model.Order order, [FromServices] INewOrderService newOrderService) =>
            {
                var result = await newOrderService.New(order.Email, order.Amount);

                return result ? Results.Ok("New order sent") : Results.BadRequest("New order NOT sent");
            })
            .WithName("PostNewOrder");

            app.MapPost("/admin/generate-reports", async ([FromServices] IGenerateAllUsersReportsService generateAllUsersReportsService) =>
            {
                var result = await generateAllUsersReportsService.Run();

                return result ? Results.Ok("Users reports requests generated") : Results.BadRequest("Users reports requests NOT generated");
            })
            .WithName("PostGenerateAllUsersReports");
        }
    }
}
