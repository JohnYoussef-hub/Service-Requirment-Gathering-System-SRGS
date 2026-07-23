namespace SRGS.Application.Common.Features.Requests.Requests.Dtos;

public class RequestDto
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public int RequestTypeId { get; set; }
    public int ModuleTypeId { get; set; }
    public int? BusinessAnalystId { get; set; }
    public int? DeveloperId { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}