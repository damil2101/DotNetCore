using System;
using System.Security.Claims;
using System.Threading.Tasks;
using DatingApp.api.Data;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.DependencyInjection;

namespace DatingApp.api.Helpers
{
    public class LogUserActivity : IAsyncActionFilter
    {

        //use of action filters
        public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
        {
            var resultContext = await next();
            var userId = int.Parse(resultContext.HttpContext.User.FindFirst(ClaimTypes.NameIdentifier).Value);
            var repo = resultContext.HttpContext.RequestServices.GetService<IDatingRepository>();
            var user = await repo.GetUser(userId);
            user.LastActive = DateTime.Now;
            await repo.SaveAll();
        }
    }
}