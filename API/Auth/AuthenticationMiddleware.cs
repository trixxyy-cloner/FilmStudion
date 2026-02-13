using System;
using API.Data;
using Microsoft.EntityFrameworkCore;

namespace API.Auth;

public class AuthenticationMiddleware
{
    private readonly RequestDelegate _next;
    public AuthenticationMiddleware(RequestDelegate next) => _next = next;


    // Den här middleware:n kör på varje request.
    // Om requesten har en Authentication-header med en token,
    // så slår vi upp användaren och sparar en session i HttpContext.Items.
    public async Task InvokeAsync(HttpContext context, FilmStudionDbContext db)
    {
        // Token skickas från frontend/API.http i headern "Authentication"
        var token = context.Request.Headers["Authentication"].ToString();

        // Om token finns: kolla om någon user i DB har den tokenen
        // (vi använder AsNoTracking eftersom vi bara läser)

        if (!string.IsNullOrWhiteSpace(token))
        {
            var user = await db.Users.AsNoTracking()
                .FirstOrDefaultAsync(u => u.AuthToken == token);

            if (user != null)
            {
                // Om user hittas: spara en liten session för controllers att använda
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
