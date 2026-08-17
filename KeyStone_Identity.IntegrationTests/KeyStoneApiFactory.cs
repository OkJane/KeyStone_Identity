using KeyStone_Identity.Infrastructure.Data;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using System;
using System.Collections.Generic;
using System.Text;

namespace KeyStone_Identity.IntegrationTests
{
    public class KeyStoneApiFactory :   WebApplicationFactory<Program>
    {
        private readonly SqliteConnection _connection = new("Filename=:memory:");

        public KeyStoneApiFactory()
        {
            _connection.Open();
        }

        protected override void ConfigureWebHost(IWebHostBuilder builder)
        {
            builder.ConfigureServices(services =>
            {
                // find and remove the real DbContext registration
                services.RemoveAll<DbContextOptions<KeyStone_Identity_DbContext>>();
                services.RemoveAll<KeyStone_Identity_DbContext>();

                // EF Core 9+: also strip the leftover Npgsql configuration action itself,
                // otherwise it gets merged with our new Sqlite one
                services.RemoveAll(typeof(IDbContextOptionsConfiguration<KeyStone_Identity_DbContext>));


                // register test one instead, pointed at the open SQLite connection
                services.AddDbContext<KeyStone_Identity_DbContext>(options => options.UseSqlite(_connection));

                // build the DB schema fresh
                using var scope = services.BuildServiceProvider().CreateScope();
                var db = scope.ServiceProvider.GetRequiredService<KeyStone_Identity_DbContext>();
                db.Database.EnsureCreated();
            });
        }
        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);
            _connection.Close();
        }
    }
}
