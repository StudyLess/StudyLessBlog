using Microsoft.Owin;
using Owin;
using SoftUniBlog.Migrations;
using SoftUniBlog.Models;
using System.Data.Entity;

[assembly: OwinStartupAttribute(typeof(SoftUniBlog.Startup))]
namespace SoftUniBlog
{
    public partial class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            Database.SetInitializer(
                new MigrateDatabaseToLatestVersion<BlogDbContext, Configuration>());

            ConfigureAuth(app);
        }
    }
}
