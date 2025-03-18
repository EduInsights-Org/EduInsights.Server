using System.ComponentModel.DataAnnotations;

namespace EduInsights.Server.Contracts;

public record LoginUserRequest(
    [Required(ErrorMessage = "Username is required.")]
    [StringLength(40, ErrorMessage = "Username can't be longer than 40 characters.")]
    string Email,
    [Required(ErrorMessage = "Password is required.")]
    [StringLength(40, ErrorMessage = "Password can't be longer than 40 characters.")]
    string Password
);