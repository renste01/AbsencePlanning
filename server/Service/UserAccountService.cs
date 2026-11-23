using Infra;
using Infra.Entities;
using LinqToDB;
using LinqToDB.Async;

namespace Service;

public class UserAccountService(DatabaseConnection db)
{
    private readonly DatabaseConnection _db = db;

    public async Task<List<UserAccount>> GetAllAsync()
    {
        IQueryable<UserAccount> users = _db.UserAccounts;
        return await users.ToListAsync();
    }

    public async Task<UserAccount?> GetByIdAsync(int id)
    {
        IQueryable<UserAccount> users = _db.UserAccounts;
        return await users.FirstOrDefaultAsync(u => u.UserAccountId == id);
    }

    public async Task<UserAccount?> CreateAsync(
        int employeeId,
        string roleName,
        string email,
        string userName,
        string password)
    {
        var normalizedEmail = email.Trim().ToLowerInvariant();
        var normalizedUserName = userName.Trim();

        IQueryable<UserAccount> users = _db.UserAccounts;

        if (await users.AnyAsync(u => u.Email == normalizedEmail))
            return null;

        if (await users.AnyAsync(u => u.UserName == normalizedUserName))
            return null;

        var hasher = new PasswordHasher();
        var user = new UserAccount
        {
            EmployeeId = employeeId,
            RoleName = roleName,
            Email = normalizedEmail,
            UserName = normalizedUserName,
            PasswordHash = hasher.Hash(password)
        };

        user.UserAccountId = await _db.InsertWithInt32IdentityAsync(user);
        return user;
    }

    public async Task<UserAccount?> UpdateAsync(
        int id,
        int employeeId,
        string roleName,
        string email,
        string userName,
        string password)
    {
        var user = await GetByIdAsync(id);
        if (user is null)
            return null;

        var normalizedEmail = email.Trim().ToLowerInvariant();
        var normalizedUserName = userName.Trim();

        IQueryable<UserAccount> users = _db.UserAccounts;

        if (await users.AnyAsync(u =>
            u.UserAccountId != id &&
            u.Email == normalizedEmail))
            return null;

        if (await users.AnyAsync(u =>
            u.UserAccountId != id &&
            u.UserName == normalizedUserName))
            return null;

        user.EmployeeId = employeeId;
        user.RoleName = roleName;
        user.Email = normalizedEmail;
        user.UserName = normalizedUserName;

        if (!string.IsNullOrWhiteSpace(password))
        {
            var hasher = new PasswordHasher();
            user.PasswordHash = hasher.Hash(password);
        }

        await _db.UserAccounts
            .Where(u => u.UserAccountId == id)
            .UpdateAsync(u => new UserAccount
            {
                EmployeeId = user.EmployeeId,
                RoleName = user.RoleName,
                Email = user.Email,
                UserName = user.UserName,
                PasswordHash = user.PasswordHash
            });

        return user;
    }

    public async Task<bool> DeleteAsync(int id)
    {
        var deletedRows = await _db.UserAccounts
            .Where(u => u.UserAccountId == id)
            .DeleteAsync();

        return deletedRows > 0;
    }
}
