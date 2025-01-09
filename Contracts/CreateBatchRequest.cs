using System.ComponentModel.DataAnnotations;

namespace EduInsights.Server.Contracts;

public record class CreateBatchRequest(
    [Required] [StringLength(40)] string Name,
    [Required] [StringLength(40)] string InstituteId
);