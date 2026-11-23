using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using PMPetCare.Models;
using System.Reflection.Emit;

namespace PMPetCare.Data
{
    public class ApplicationDbContext : IdentityDbContext<User>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }
        public DbSet<Pet> Pets { get; set; }
        public DbSet<Service> Services { get; set; }
        public DbSet<Appointment> Appointments { get; set; }
        public DbSet<CareNote> CareNotes { get; set; }
        public DbSet<Review> Reviews { get; set; }


        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);
            builder.Entity<Service>()
                .Property(s => s.Price)
                .HasPrecision(18, 0);

            // Pet - Customer
            builder.Entity<Pet>()
                .HasOne(p => p.Customer)
                .WithMany(u => u.Pets)
                .HasForeignKey(p => p.CustomerId)
                .OnDelete(DeleteBehavior.NoAction);

            // Appointment - Customer
            builder.Entity<Appointment>()
                .HasOne(a => a.Customer)
                .WithMany()
                .HasForeignKey(a => a.CustomerId)
                .OnDelete(DeleteBehavior.NoAction);

            // Appointment - Pet
            builder.Entity<Appointment>()
                .HasOne(a => a.Pet)
                .WithMany(p => p.Appointments)
                .HasForeignKey(a => a.PetId)
                .OnDelete(DeleteBehavior.NoAction);

            // Appointment - Service
            builder.Entity<Appointment>()
                .HasOne(a => a.Service)
                .WithMany(s => s.Appointments)
                .HasForeignKey(a => a.ServiceId)
                .OnDelete(DeleteBehavior.NoAction);

            // Appointment - Staff
            builder.Entity<Appointment>()
                .HasOne(a => a.Staff)
                .WithMany()
                .HasForeignKey(a => a.StaffId)
                .OnDelete(DeleteBehavior.NoAction);

            // CareNote - Pet
            builder.Entity<CareNote>()
                .HasOne(c => c.Pet)
                .WithMany(p => p.CareNotes)
                .HasForeignKey(c => c.PetId)
                .OnDelete(DeleteBehavior.NoAction);

            // CareNote - Appointment
            builder.Entity<CareNote>()
                .HasOne(c => c.Appointment)
                .WithMany(a => a.CareNotes)
                .HasForeignKey(c => c.AppointmentId)
                .OnDelete(DeleteBehavior.NoAction);

            // CareNote - Staff
            builder.Entity<CareNote>()
                .HasOne(c => c.Staff)
                .WithMany()
                .HasForeignKey(c => c.StaffId)
                .OnDelete(DeleteBehavior.NoAction);

            // Appointment - Review
            builder.Entity<Appointment>()
                .HasOne(a => a.Review)
                .WithOne(r => r.Appointment)
                .HasForeignKey<Review>(r => r.AppointmentId)
                .OnDelete(DeleteBehavior.NoAction);

            // Review - Customer
            builder.Entity<Review>()
                .HasOne(r => r.Customer)
                .WithMany()
                .HasForeignKey(r => r.CustomerId)
                .OnDelete(DeleteBehavior.NoAction);

            // Mỗi lịch hẹn chỉ được đánh giá một lần
            builder.Entity<Review>()
                .HasIndex(r => r.AppointmentId)
                .IsUnique();

            
              
        }

    }
}
