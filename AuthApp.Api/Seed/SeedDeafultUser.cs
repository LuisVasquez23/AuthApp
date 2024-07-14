using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace AuthApp.Api.Seed
{
    public class SeedDeafultUser
    {
        public static void RegisterDefaultUser(ModelBuilder modelBuilder)
        {
            var user = UserInfo();
            var adminRole = RoleAdmin();
            var userRoles = RoleUser();
            var userRole = UserRole(user.Id, adminRole.Id);
            modelBuilder.Entity<IdentityUser>().HasData(user);
            modelBuilder.Entity<IdentityRole>().HasData(adminRole, userRoles);
            modelBuilder.Entity<IdentityUserRole<string>>().HasNoKey().HasData(userRole);

        }


        #region USER INFO 
        private static IdentityUser UserInfo()
        {
            var user = new IdentityUser
            {
                Id = Guid.NewGuid().ToString(),
                Email = "admin@admin.com",
                EmailConfirmed = true,
                UserName = "admin@admin.com"
            };

            PasswordHasher<IdentityUser> hasher = new PasswordHasher<IdentityUser>();
            user.PasswordHash = hasher.HashPassword(user, "admin_1234");

            return user;
        }
        #endregion

        #region ADMIN  
        private static IdentityRole RoleAdmin()
        {
            var role = new IdentityRole
            {
                Id = Guid.NewGuid().ToString(),
                Name = "admin",
                NormalizedName = "Admin"
            };

            return role;
        }
        #endregion

        #region USER  
        private static IdentityRole RoleUser()
        {
            var role = new IdentityRole
            {
                Id = Guid.NewGuid().ToString(),
                Name = "user",
                NormalizedName = "User"
            };

            return role;
        }
        #endregion

        #region USER ROLE 
        private static IdentityUserRole<string> UserRole(string userId , string roleId)
        {
            var userRole = new IdentityUserRole<string>
            {
                RoleId = roleId,
                UserId = userId
            };

            return userRole;
        }
        #endregion


    }
}
