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

            modelBuilder.Entity<ScholarshipDefinition>()
            .HasOptional(f => f.Socioeconomic)
            .WithRequired(s => s.ScholarshipDefinition);

            modelBuilder.Entity<ScholarshipDefinition>()
           .HasOptional(f => f.ExcellenceStudent)
           .WithRequired(s => s.ScholarshipDefinition);

            modelBuilder.Entity<ScholarshipDefinition>()
           .HasOptional(f => f.InPractice)
           .WithRequired(s => s.ScholarshipDefinition);

            modelBuilder.Entity<VolunteerPlaces>()
                .HasMany(s => s.InPracticess)
                .WithRequired(s => s.VolunteerPlacess)
                .HasForeignKey(s => s.Volunteer1Id);

            modelBuilder.Entity<VolunteerPlaces>()
                .HasMany(s => s.InPracticess)
                .WithRequired(s => s.VolunteerPlacess)
                .HasForeignKey(s => s.Volunteer2Id);
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
        public DbSet<Socioeconomic> Socioeconomics { get; set; }
        public DbSet<ScholarshipDefinition> ScholarshipDefinitions { get; set; }
        public DbSet<Cities> Cities { get; set; }
        public DbSet<VolunteerPlaces> VolunteerPlaces { get; set; }
        public DbSet<InPractice> InPractice { get; set; }
        public DbSet<ExcellenceStudent> ExcellenceStudent { get; set; }

    }
}