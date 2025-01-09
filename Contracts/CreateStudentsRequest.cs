using System.ComponentModel.DataAnnotations;

namespace EduInsights.Server.Contracts;

public record CreateStudentsRequest(
    [Required] [StringLength(40)] string IndexNumber,
    [Required] [StringLength(40)] string UserId
);