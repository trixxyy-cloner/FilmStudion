using System;
using API.Data;
using Microsoft.EntityFrameworkCore;

namespace API.Auth;

public class AuthenticationMiddleware
{
    private readonly RequestDelegate _next;
    public AuthenticationMiddleware(RequestDelegate next) => _next = next;

    public async Task InvokeAsync(HttpContext context, FilmStudionDbContext db)
    {
        var token = context.Request.Headers["Authentication"].ToString();

        if (!string.IsNullOrWhiteSpace(token))
        {
            var user = await db.Users.AsNoTracking()
                .FirstOrDefaultAsync(u => u.AuthToken == token);

            if (user != null)
            {
                context.Items["session"] = new AuthSession(
                    user.UserId,
                    user.Username,
                    user.Role,
                    user.FilmStudioId
                );
            }
        }

        await _next(context);
    }
}
