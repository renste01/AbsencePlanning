using Infra;
using Infra.Entities;
using LinqToDB;
using LinqToDB.Async;

namespace Service;

public class Authservice(DatabaseConnection db)
{
    private readonly DatabaseConnection _db = db;

    
    //method for registering a user, it checks for email, username, password and role.
    //it also checks if the email or username has been used before
    public async Task<UserAccount?> RegisterAsync(
        string firstName,
        string lastName,
        string email, 
        string password, 
        string userName)
    {
        var normalizedEmail = email.Trim().ToLowerInvariant();
        var normalizedUserName = userName.Trim().ToLowerInvariant();
        var hasher = new PasswordHasher();

        IQueryable<UserAccount> users = _db.UserAccounts;
        
        if (await users.AnyAsync(
            u => u.Email == normalizedEmail))
            return null;
        
        
        if (await users.AnyAsync(
            u => u.UserName == normalizedUserName))
            return null;
// Creates a transaction, so either all gets send or nothing
        using var transaction = _db.BeginTransaction();

        var employee = new Employee
        {
            FirstName = firstName.Trim(),
            LastName = lastName.Trim(),
            EmployeeStartDate = DateOnly.FromDateTime(DateTime.Today)
        };
        employee.EmployeeId = await _db.InsertWithInt32IdentityAsync(employee);

        var user = new UserAccount()
        {
            EmployeeId = employee.EmployeeId,
            Email = normalizedEmail,
            UserName = normalizedUserName,
            PasswordHash = hasher.Hash(password),
            RoleName = Roles.Employee
        };

        user.UserAccountId = await _db.InsertWithInt32IdentityAsync(user);
        transaction.Commit();
        return user;
    }


    public async Task<UserAccount?> LoginAsync(string userName, string password)
    {
        var normalizedUserName = userName.Trim().ToLowerInvariant();
        IQueryable<UserAccount> users = _db.UserAccounts;
        var user = await users.FirstOrDefaultAsync(u => u.UserName == normalizedUserName);
        var hasher = new PasswordHasher();
        
        
        if (user is null) 
            return null;

        return hasher.Verify(password, user.PasswordHash) ? user : null;
    }
}