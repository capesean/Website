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
        public DbSet<DbSettings> Settings { get; set; }

        public ApplicationDbContext()
        {
            // disabling tracking entirely messes up openiddict's sign-in behaviour: https://github.com/openiddict/openiddict-core/issues/565
            //ChangeTracker.QueryTrackingBehavior = QueryTrackingBehavior.NoTracking;
            ChangeTracker.AutoDetectChangesEnabled = false;
        }

        public ApplicationDbContext(DbContextOptions options) : base(options)
        {
            //ChangeTracker.QueryTrackingBehavior = QueryTrackingBehavior.NoTracking;
            ChangeTracker.AutoDetectChangesEnabled = false;
        }

        public static ApplicationDbContext Create()
        {
            return new ApplicationDbContext();
        }

        internal async Task InitAsync(UserManager<User> um, RoleManager<AppRole> rm, Settings settings, DbContextOptions options)
        {
            if (settings.IsDevelopment)
            {
                // if not using migrations:
                //Database.EnsureDeleted();
                //Database.EnsureCreated();
                //AddComputedColumns();
                //AddNullableUniqueIndexes();
                //await SeedAsync(um, rm);//, settings, options);

                // if using migrations:
                //Database.EnsureCreated();
                //Database.Migrate();
                //AddComputedColumns();
                //AddNullableUniqueIndexes();
                //await SeedAsync(um, rm);
            }
            else
            {
                Database.Migrate();
            }

            await DeleteErrors(-7);
        }

        private async Task DeleteErrors(int since)
        {
            var cutoff = DateTime.Now.AddDays(since);
            foreach (var error in Errors.Where(o => o.DateUtc < cutoff).ToList())
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
            optionsBuilder.EnableSensitiveDataLogging();
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

            ConfigureModelBuilder(modelBuilder);

            modelBuilder.Entity<User>(o => o.HasMany(u => u.Roles).WithOne().HasForeignKey(ur => ur.UserId).IsRequired());
            modelBuilder.Entity<DbSettings>(o => o.ToTable("Settings"));

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

        private void CreateNullableUniqueIndex(string tableName, string fieldName)
        {
            Database.ExecuteSqlRaw($"DROP INDEX IF EXISTS IX_{tableName}_{fieldName} ON {tableName};");
            Database.ExecuteSqlRaw($"CREATE UNIQUE NONCLUSTERED INDEX IX_{tableName}_{fieldName} ON {tableName}({fieldName}) WHERE {fieldName} IS NOT NULL;");
        }

    }
}