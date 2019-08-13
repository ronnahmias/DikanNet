using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Web;

namespace DataEntities.DB
{
    public class DikanDbContext : DbContext
    {
        public DikanDbContext() : base ("name=DikanNetDB")
        { }

        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Countries>()
                .HasMany(s => s.Student)
                .WithRequired(s => s.Country)
                .HasForeignKey(s => s.CountryBirthId);

            modelBuilder.Entity<SpDefinition>()
             .HasMany(s => s.Socioeconomicc)
            .WithRequired(s => s.ScholarshipDefinition)
            .HasForeignKey(s => s.ScholarshipId);

            modelBuilder.Entity<SpDefinition>()
           .HasMany(f => f.ExcellenceStudents)
           .WithRequired(s => s.ScholarshipDefinition)
           .HasForeignKey(s => s.ScholarshipId);

            modelBuilder.Entity<SpDefinition>()
           .HasMany(f => f.InPractices)
           .WithRequired(s => s.ScholarshipDefinition)
           .HasForeignKey(s => s.ScholarshipId);

            modelBuilder.Entity<VolunteerPlaces>()
                .HasMany(s => s.InPracticess)
                .WithRequired(s => s.VolunteerPlacess)
                .HasForeignKey(s => s.Volunteer1Id);

            modelBuilder.Entity<VolunteerPlaces>()
                .HasMany(s => s.InPracticess)
                .WithRequired(s => s.VolunteerPlacess)
                .HasForeignKey(s => s.Volunteer2Id);

            modelBuilder.Entity<SpDefinition>()
              .HasMany(f => f.StudentFinances)
              .WithRequired(s => s.SpDefinition)
              .HasForeignKey(s => s.SpId);

            modelBuilder.Entity<SpDefinition>()
              .HasMany(f => f.FamilyStudentFinances)
              .WithRequired(s => s.SpDefinition)
              .HasForeignKey(s => s.SpId);
        }
        public DbSet<Student> Students { get; set; }
        public DbSet<Countries> Countries { get; set; }
        public DbSet<Major> Majors { get; set; }
        public DbSet<HeadMajor> HeadMajor { get; set; }
        public DbSet<Users> Users { get; set; }
        public DbSet<FamilyMember> FamilyMembers { get; set; }
        public DbSet<FamilyStudentFinance> FamilyStudentFinances { get; set; }
        public DbSet<Funding> Fundings { get; set; }
        public DbSet<StudentFinance> StudentFinances { get; set; }
        public DbSet<CarStudent> CarStudents { get; set; }
        public DbSet<SpSocio> Socio { get; set; }
        public DbSet<SpDefinition> SpDef { get; set; }
        public DbSet<Cities> Cities { get; set; }
        public DbSet<VolunteerPlaces> VolunteerPlaces { get; set; }
        public DbSet<SpHalacha> Halacha { get; set; }
        public DbSet<SpExcellence> Ecellence { get; set; }

    }
}