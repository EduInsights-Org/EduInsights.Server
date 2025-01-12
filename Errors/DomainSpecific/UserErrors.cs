using EduInsights.Server.Errors.Common;

namespace EduInsights.Server.Errors.DomainSpecific;

public class UserErrors
{
    public static readonly Error AlreadyExistUserName = new Error(
        "User.AlreadyExistUserName", "Can't add already exists user name");

    public static readonly Error CreationFailed = new Error(
        "User.CreationFailed", "An error occurred while creating the user.");

    public static readonly Error ValidationFailed = new Error(
        "User.ValidationFailed", "Missing Fields required");
}