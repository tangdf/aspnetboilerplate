using System;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data.SqlClient;
using Abp.Domain.Entities;
using Abp.EntityFrameworkCore.Tests.Ef;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Xunit;
using Xunit.Abstractions;

namespace Abp.EntityFrameworkCore.Tests
{
    public class AbpDbContext_PrimaryKey_Guid_Generation
    {

        [Fact]
        public void Entity_Create() {

            var sqlConnectionStringBuilder = new SqlConnectionStringBuilder {
                DataSource = "*****",
                InitialCatalog = "abpDfCore",
                UserID = "sa",
                Password = "*****"
            };

            var builder = new DbContextOptionsBuilder<TestDbContext>();


            builder.UseSqlServer(sqlConnectionStringBuilder.ToString());


            using (TestDbContext testDbContext = new TestDbContext(builder.Options)) {

                testDbContext.Database.EnsureDeleted();
                testDbContext.Database.EnsureCreated();

                var guid = Guid.NewGuid();

                var testGuidGenerator = new TestGuidGenerator(guid);

                testDbContext.GuidGenerator = testGuidGenerator;

                var entity = new GuidEntity {
                    Number = 1
                };

                testDbContext.Set<GuidEntity>().Add(entity);

                testDbContext.SaveChanges();

                Assert.True(testGuidGenerator.CreateCalled);

                Assert.Equal(guid, entity.Id);
            }
        }


        public class TestGuidGenerator : IGuidGenerator
        {
            private Guid _guid;

            public TestGuidGenerator(Guid guid) {
                _guid = guid;
            }

            public Guid Create() {
                CreateCalled = true;
                return _guid;
            }

            public bool CreateCalled { get; private set; }
        }

        public class TestDbContext : AbpDbContext
        {
            public TestDbContext(DbContextOptions options) : base(options) {
            }

            protected override void OnModelCreating(ModelBuilder modelBuilder) {
                modelBuilder.Entity<GuidEntity>().ToTable("GuidTable");
                modelBuilder.Entity<GuidEntity>().Property(e => e.Id);
                base.OnModelCreating(modelBuilder);
            }
        }

        public class GuidEntity : Entity<Guid>
        {
            public override Guid Id { get; set; }
            public int Number { get; set; }
        }
    }
}