using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Filters;
using System.Security.Claims;
using System.Threading.Tasks;
using AYS.Entity.Concrete;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using AYS.Entity;
using Microsoft.AspNetCore.Authorization;

public class VerifyKeyFilter : IAsyncActionFilter
{
    private readonly UserManager<AppUser> _userManager;
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly IdentityContext _dbContext;

    public VerifyKeyFilter(UserManager<AppUser> userManager, IHttpContextAccessor httpContextAccessor, IdentityContext dbContext)
    {
        _userManager = userManager;
        _httpContextAccessor = httpContextAccessor;
        _dbContext = dbContext;
    }

    public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
    {
        var httpContext = _httpContextAccessor.HttpContext;
        var path = httpContext.Request.Path.Value;

        if (path.StartsWith("/Company", StringComparison.OrdinalIgnoreCase))
        {
            var userId = httpContext.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (userId == null)
            {
                httpContext.Response.StatusCode = StatusCodes.Status401Unauthorized;
                await httpContext.Response.WriteAsync("Unauthorized: No user ID found.");
                return;
            }

            var user = await _dbContext.Users
                            .Include(u => u.VerificationKey)
                            .FirstOrDefaultAsync(u => u.Id == userId);


            if (user == null)
            {
                httpContext.Response.StatusCode = StatusCodes.Status404NotFound;
                await httpContext.Response.WriteAsync("User not found.");
                return;
            }

            if (user.VerificationKeyId == null || user.VerificationKey == null || DateTime.Now > user.VerificationKey.VerificationKeyExpirationDate)
            {
                httpContext.Response.Redirect("/Account/EnterVerificationKey");
                return;
            }
        }

        await next();
    }
}
