using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;

namespace WEB.Models
{
    public partial class ApplicationDbContext : IdentityDbContext<User, AppRole, Guid>
    {
        //public DbSet<Error> Errors { get; set; }
        //public DbSet<ErrorException> ErrorExceptions { get; set; }
        //public DbSet<Settings> Settings { get; set; }

        public ApplicationDbContext()
        {
            ChangeTracker.AutoDetectChangesEnabled = false;

            //if (ConfigurationManager.AppSettings["RootUrl"].ToString().StartsWith("http://localhost:"))
            //{
            //    // auto migrate if local
            //    Database.SetInitializer(new MigrateDatabaseToLatestVersion<ApplicationDbContext, Migrations.DevelopmentConfiguration>());
            //}
            //else
            //{
            //    Database.SetInitializer(new MigrateDatabaseToLatestVersion<ApplicationDbContext, Migrations.DevelopmentConfiguration>());
            //    // no migrations, for optimal startup
            //    /* note: this requires the dbinterceptor to be in web.config, so it gets applied (no seed method to apply it on) */
            //    //Database.SetInitializer<ApplicationDbContext>(null);
            //}
        }

        public ApplicationDbContext(DbContextOptions options) : base(options)
        {
            ChangeTracker.AutoDetectChangesEnabled = false;
        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseLazyLoadingProxies(false);

            base.OnConfiguring(optionsBuilder);
        }



        public static ApplicationDbContext Create()
        {
            return new ApplicationDbContext();
        }

        //public override int SaveChanges()
        //{
        //    try
        //    {
        //        return base.SaveChanges();
        //    }
        //    catch (EntityValidationException e)
        //    {
        //        foreach (var eve in e.EntityValidationErrors)
        //        {
        //            e.Data.Add(Guid.NewGuid().ToString(), string.Format("Entity of type \"{0}\" in state \"{1}\" has the following validation errors:",
        //                eve.Entry.Entity.GetType().Name, eve.Entry.State));

        //            foreach (var ve in eve.ValidationErrors)
        //            {
        //                e.Data.Add(ve.PropertyName, ve.ErrorMessage);
        //            }
        //        }
        //        throw;
        //    }
        //}


        private void CreateComputedColumn(string tableName, string fieldName, string calculation)
        {
            // todo: this should be parameterised

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
            Database.ExecuteSqlCommand(sql);
            // drop column
            Database.ExecuteSqlCommand($"IF EXISTS(SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = '{tableName}' AND COLUMN_NAME = '{fieldName}') ALTER TABLE {tableName} DROP COLUMN {fieldName};");
            // add column
            Database.ExecuteSqlCommand($"ALTER TABLE {tableName} ADD {fieldName} AS {calculation};");
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // todo: check if this works...
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

        //private void AddUser(AppUserManager userManager, RoleManager<AppRole, Guid> roleManager, string email, string firstName, string lastName, string password, List<string> roles)
        //{
        //    User user;
        //    if (userManager.FindByEmail(email) == null)
        //    {
        //        user = new User
        //        {
        //            UserName = email,
        //            Email = email,
        //            EmailConfirmed = true,
        //            FirstName = firstName,
        //            LastName = lastName,
        //            Enabled = true
        //        };
        //        userManager.Create(user, password);
        //    }

        //    user = userManager.FindByEmail(email);

        //    foreach (var role in roles)
        //    {
        //        if (!userManager.IsInRole(user.Id, role))
        //        {
        //            userManager.AddToRole(user.Id, role);
        //        }
        //    }
        //}

    }
}