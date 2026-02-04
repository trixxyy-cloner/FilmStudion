namespace API.Contracts.Responses.Users;

public record class AdminRegisteredUserDto(
    int UserId,
    string Username,
    string Role
);
