using LinqToDB.Mapping;

namespace Infra.Entities;

[Table("VacationRequestDay")]
public class VacationRequestDay
{
    [PrimaryKey, Identity]
    public int VacationRequestDayId { get; set; }

    [Column, NotNull]
    public int VacationRequestId { get; set; }

    [Column, NotNull]
    public DateOnly Date { get; set; }

    [Column(Precision = 8, Scale = 2), NotNull]
    public decimal Hours { get; set; }

    [Association(ThisKey = nameof(VacationRequestId), OtherKey = nameof(VacationRequest.VacationRequestId))]
    public VacationRequest? VacationRequest { get; set; }
}