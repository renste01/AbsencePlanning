using LinqToDB;
using LinqToDB.Data;
using Infra.Entities;

namespace Infra;

public class DatabaseConnection : DataConnection
{
    public DatabaseConnection(string connectionString)
        : base(new DataOptions().UsePostgreSQL(connectionString))
    {
    }

    public ITable<Employee> Employees => this.GetTable<Employee>();
    public ITable<HolidayRule> HolidayRules => this.GetTable<HolidayRule>();
    public ITable<UserAccount> UserAccounts => this.GetTable<UserAccount>();
    public ITable<VacationBalance> VacationBalances => this.GetTable<VacationBalance>();
    public ITable<PublicHoliday> PublicHolidays => this.GetTable<PublicHoliday>();
    public ITable<VacationCancellationRequest> VacationCancellationRequests => this.GetTable<VacationCancellationRequest>();
    public ITable<VacationRequest> VacationRequests => this.GetTable<VacationRequest>();
    public ITable<VacationRequestDay> VacationRequestDays => this.GetTable<VacationRequestDay>();
    public ITable<VacationTransaction> VacationTransactions => this.GetTable<VacationTransaction>();
    public ITable<VacationTransferRequest> VacationTransferRequests => this.GetTable<VacationTransferRequest>();
    public ITable<WorkSchedule> WorkSchedules => this.GetTable<WorkSchedule>();
    public ITable<WorkSchedulePeriod> WorkSchedulePeriods => this.GetTable<WorkSchedulePeriod>();
}
