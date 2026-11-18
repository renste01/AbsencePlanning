using System.ComponentModel.DataAnnotations;
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
        string email, 
        string password, 
        string userName,
        string role)
    {
        var normalizedEmail = email.Trim().ToLowerInvariant();
        var normalizedUserName = userName.Trim();
        var hasher = new PasswordHasher();

        IQueryable<UserAccount> users = _db.UserAccounts;
        
        var emailExists = await users.AnyAsync(
            u => u.Email == normalizedEmail);

        if (emailExists)
            return null;
        
        var usernameExists = await users.AnyAsync(
            u => u.UserName == normalizedUserName);

        if (usernameExists)
            return null;

        var isFirstUser = !await users.AnyAsync();

        var user = new UserAccount()
        {
            Email = normalizedEmail,
            UserName = normalizedEmail,
            PasswordHash = hasher.Hash(password),
            RoleName = role
        };

        user.UserAccountId = await _db.InsertWithInt32IdentityAsync(user);
        user.EmployeeId = await _db.InsertWithInt32IdentityAsync(user);
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