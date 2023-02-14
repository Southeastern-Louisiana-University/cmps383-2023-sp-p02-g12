using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using SP23.P02.Web.Features;

namespace SP23.P02.Web.Data;

public class DataContext : IdentityDbContext<User, Role, int, IdentityUserClaim<int>, UserRole, IdentityUserLogin<int>, IdentityRoleClaim<int>, IdentityUserToken<int>>
{
    public DataContext(DbContextOptions<DataContext> options) : base(options)
    {
    }

    public DataContext()
    {
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.ApplyConfigurationsFromAssembly(typeof(DataContext).Assembly);
        
        base.OnModelCreating(builder);
        var userRoleBuilder = builder.Entity<UserRole>();

        userRoleBuilder.HasKey(x => new { x.UserId, x.Role });

        userRoleBuilder.HasOne(x.Role)
                       .WithMany(x => x.Users)
                       .HasForeignKey(x => x.RoleId);

        userRoleBuilder.HasOne(x => x.User)
            .WithMany(x => x.Roles)
            .HasForeignKey(x => x.UserId);


    } 


    }
    
