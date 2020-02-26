using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace WEB.Models
{
    public partial class ApplicationDbContext : IdentityDbContext<User, AppRole, Guid>
    {
        public DbSet<Error> Errors { get; set; }
        public DbSet<ErrorException> Exceptions { get; set; }
        //public DbSet<Settings> Settings { get; set; }

        public ApplicationDbContext()
        {
            ChangeTracker.AutoDetectChangesEnabled = false;
        }

        public ApplicationDbContext(DbContextOptions options) : base(options)
        {
            ChangeTracker.AutoDetectChangesEnabled = false;
        }

        public static ApplicationDbContext Create()
        {
            return new ApplicationDbContext();
        }

        internal async Task InitAsync(UserManager<User> um, RoleManager<AppRole> rm)
        {
            //// if not using migrations:
            //Database.EnsureDeleted();
            //Database.EnsureCreated();
            //AddComputedColumns();

            //// if using migrations:
            //Database.EnsureCreated();
            //Database.Migrate();
            //AddComputedColumns();

            await SeedAsync(um, rm);

            await DeleteErrors(-7);
        }

        private async Task DeleteErrors(int since)
        {
            var cutoff = DateTime.Now.AddDays(since);
            foreach (var error in Errors.Where(o => o.DateUtc < cutoff))
            {
                Entry(error).State = EntityState.Deleted;
                Guid? exceptionId = error.ExceptionId;
                while (exceptionId != null)
                {
                    var exception = await Exceptions.FirstAsync(o => o.Id == exceptionId);
                    Entry(exception).State = EntityState.Deleted;
                    exceptionId = exception.InnerExceptionId;
                }
            }
            await SaveChangesAsync();
        }

        internal async Task SeedAsync(UserManager<User> um, RoleManager<AppRole> rm)
        {
            var roles = Enum.GetNames(typeof(Roles));
            foreach (var role in roles)
                if (!await rm.RoleExistsAsync(role)) await rm.CreateAsync(new AppRole { Name = role });
            await CreateUserAsync(um, "abc@xyz.com", "ABC@xyz!", "Abc", "Xyz", roles);
        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseLazyLoadingProxies(false);

            base.OnConfiguring(optionsBuilder);
        }

        private async Task CreateUserAsync(UserManager<User> um, string email, string password, string firstName, string lastName, string[] roles)
        {
            User user = await um.FindByEmailAsync(email);
            if (user == null)
            {
                user = new User
                {
                    UserName = email,
                    Email = email,
                    FirstName = firstName,
                    LastName = lastName,
                    EmailConfirmed = true,
                    Enabled = true
                };
                var result = await um.CreateAsync(user, password);
                if (!result.Succeeded) throw new Exception(string.Join(", ", result.Errors));
            }

            foreach (var role in roles)
                if (!await um.IsInRoleAsync(user, role))
                    await um.AddToRoleAsync(user, role);
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            //AddComputedColumns();

            ConfigureModelBuilder(modelBuilder);

            modelBuilder.Entity<User>(o => o.HasMany(u => u.Roles).WithOne().HasForeignKey(ur => ur.UserId).IsRequired());

            //using (var roleStore = new RoleStore<AppRole, Guid, AppUserRole>(context))
            //using (var roleManager = new RoleManager<AppRole, Guid>(roleStore))
            //using (var userManager = new AppUserManager(new AppUserStore(context)))
            //{
            //    var allRoles = Enum.GetNames(typeof(Roles)).ToList();
            //    foreach (var role in allRoles)
            //    {
            //        if (!roleManager.RoleExists(role.ToLower()))
            //        {
            //            roleManager.Create(new AppRole() { Name = role, Id = Guid.NewGuid() });
            //        }
            //    }

            //    AddUser(userManager, roleManager, "seanmatthewwalsh@gmail.com", "Sean", "Walsh", "P2ssw0rd!", allRoles);

            //    if (!context.Settings.Any())
            //    {
            //        var settings = new Settings();
            //        context.Entry(settings).State = EntityState.Added;
            //        context.SaveChanges();
            //    }
            //}

            // add custom indices here with fluent api

            foreach (var relationship in modelBuilder.Model.GetEntityTypes().SelectMany(e => e.GetForeignKeys()))
            {
                relationship.DeleteBehavior = DeleteBehavior.Restrict;
            }
        }

        private void CreateComputedColumn(string tableName, string fieldName, string calculation)
        {
            // drop default
            var sql = $@"declare @Command  nvarchar(1000)
                    select @Command = 'ALTER TABLE dbo.{tableName} drop constraint ' + d.name
                     from sys.tables t
                      join    sys.default_constraints d
                       on d.parent_object_id = t.object_id
                      join    sys.columns c
                       on c.object_id = t.object_id
                        and c.column_id = d.parent_column_id
                     where t.name = '{tableName}'
                      and t.schema_id = schema_id('dbo')
                      and c.name = '{fieldName}'

                    execute (@Command);";
            Database.ExecuteSqlRaw(sql);
            // drop column
            Database.ExecuteSqlRaw($"IF EXISTS(SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = '{tableName}' AND COLUMN_NAME = '{fieldName}') ALTER TABLE {tableName} DROP COLUMN {fieldName};");
            // add column
            Database.ExecuteSqlRaw($"ALTER TABLE {tableName} ADD {fieldName} AS {calculation};");
        }

    }
}