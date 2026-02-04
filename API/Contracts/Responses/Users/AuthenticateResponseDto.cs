namespace API.Contracts.Responses.Users;

public record class AuthenticateResponseDto(
    string Token,
    AuthenticatedUserDto User
);
