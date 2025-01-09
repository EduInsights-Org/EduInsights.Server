using System.ComponentModel.DataAnnotations;

namespace EduInsights.Server.Contracts;

public record class RegisterUserRequest(
    [Required(ErrorMessage = "First Name is required.")]
    [StringLength(40, ErrorMessage = "First Name can't be longer than 40 characters.")]
    string FirstName,
    [Required(ErrorMessage = "Last Name is required.")]
    [StringLength(40, ErrorMessage = "Last Name can't be longer than 40 characters.")]
    string LastName,
    [Required(ErrorMessage = "Username is required.")]
    [StringLength(40, ErrorMessage = "Username can't be longer than 40 characters.")]
    string UserName,
    [Required(ErrorMessage = "Institute Name is required.")]
    [StringLength(40, ErrorMessage = "Institute Name can't be longer than 40 characters.")]
    string InstituteName,
    [Required(ErrorMessage = "Password is required.")]
    [StringLength(40, ErrorMessage = "Password can't be longer than 40 characters.")]
    string Password,
    [Required(ErrorMessage = "Role is required.")]
    [StringLength(40, ErrorMessage = "Role can't be longer than 40 characters.")]
    string Role
);