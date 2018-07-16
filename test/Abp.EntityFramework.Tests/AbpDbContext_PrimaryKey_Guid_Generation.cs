using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Common;
using System.Data.Entity;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Abp.Domain.Entities;
using Xunit;
using Xunit.Abstractions;

namespace Abp.EntityFramework.Tests
{
    public class AbpDbContext_PrimaryKey_Guid_Generation
    {

        [Fact]
        public void Entity_Create() {

            Database.SetInitializer<TestDbContext>(new DropCreateDatabaseIfModelChanges<TestDbContext>());


            var sqlConnectionStringBuilder = new SqlConnectionStringBuilder {
                DataSource = "*****",
                InitialCatalog = "abpEF",
                UserID = "sa",
                Password = "******"
            };

            using (TestDbContext testDbContext = new TestDbContext(sqlConnectionStringBuilder.ToString())) {
                var guid = Guid.NewGuid();

                var testGuidGenerator = new TestGuidGenerator(guid);
                testDbContext.GuidGenerator = testGuidGenerator;

                var entity = testDbContext.Set<GuidEntity>().Add(new GuidEntity {
                    Number = 1
                });
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
            public TestDbContext(string nameOrConnectionString) : base(nameOrConnectionString) {
            }

            protected override void OnModelCreating(DbModelBuilder modelBuilder) {
                modelBuilder.Entity<GuidEntity>().ToTable("GuidTable");
                modelBuilder.Entity<GuidEntity>().Property(e => e.Id).HasDatabaseGeneratedOption(DatabaseGeneratedOption.Identity);
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