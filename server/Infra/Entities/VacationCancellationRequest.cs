using LinqToDB.Mapping;

namespace Infra.Entities;

[Table("VacationCancellationRequest")]
public class VacationCancellationRequest
{
    [PrimaryKey, Identity]
    public int VacationCancellationRequestId { get; set; }

    [Column, NotNull]
    public int VacationRequestId { get; set; }

    [Column, NotNull]
    public string Status { get; set; } = "Pending";

    [Column, NotNull]
    public DateTime RequestedAt { get; set; }
    
    [Column]
    public int? ProcessedByUserAccountId { get; set; }

    [Column]
    public DateTime? ProcessedAt { get; set; }

    [Column]
    public string? RejectionReason { get; set; }

    [Association(ThisKey = nameof(VacationRequestId), OtherKey = nameof(VacationRequest.VacationRequestId))]
    public VacationRequest? VacationRequest { get; set; }
}