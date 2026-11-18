using LinqToDB.Mapping;

namespace Infra.Entities;

[Table("WorkSchedule")]
public class WorkSchedule
{
    [PrimaryKey, Identity]
    public int WorkScheduleId { get; set; }

    [Column] 
    public TimeOnly? MondayFrom { get; set; }
    
    [Column] 
    public TimeOnly? MondayTo { get; set; }
    
    [Column] 
    public TimeOnly? TuesdayFrom { get; set; }
    
    [Column] 
    public TimeOnly? TuesdayTo { get; set; }
    
    [Column] 
    public TimeOnly? WednesdayFrom { get; set; }
    
    [Column] 
    public TimeOnly? WednesdayTo { get; set; }
    
    [Column] 
    public TimeOnly? ThursdayFrom { get; set; }
    
    [Column] 
    public TimeOnly? ThursdayTo { get; set; }
    
    [Column] 
    public TimeOnly? FridayFrom { get; set; }
    
    [Column] 
    public TimeOnly? FridayTo { get; set; }
}