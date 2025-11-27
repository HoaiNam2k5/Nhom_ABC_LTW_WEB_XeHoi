using System;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity;
using System.Linq;

namespace WebCar.Models
{
    public partial class Model1 : DbContext
    {
        public Model1()
            : base("name=Carsale_DB")
        {
        }

        public virtual DbSet<ACCOUNT_ROLE> ACCOUNT_ROLE { get; set; }
        public virtual DbSet<AUDIT_LOG> AUDIT_LOG { get; set; }
        public virtual DbSet<CAR> CARs { get; set; }
        public virtual DbSet<CUSTOMER> CUSTOMERs { get; set; }
        public virtual DbSet<FEEDBACK> FEEDBACKs { get; set; }
        public virtual DbSet<ORDER_DETAIL> ORDER_DETAIL { get; set; }
        public virtual DbSet<ORDER> ORDERS { get; set; }

        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            modelBuilder.Entity<CAR>()
                .Property(e => e.GIA)
                .HasPrecision(12, 2);

            modelBuilder.Entity<CAR>()
                .HasMany(e => e.FEEDBACKs)
                .WithRequired(e => e.CAR)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<CAR>()
                .HasMany(e => e.ORDER_DETAIL)
                .WithRequired(e => e.CAR)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<CUSTOMER>()
                .HasMany(e => e.ACCOUNT_ROLE)
                .WithRequired(e => e.CUSTOMER)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<CUSTOMER>()
                .HasMany(e => e.FEEDBACKs)
                .WithRequired(e => e.CUSTOMER)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<CUSTOMER>()
                .HasMany(e => e.ORDERS)
                .WithRequired(e => e.CUSTOMER)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<ORDER_DETAIL>()
                .Property(e => e.DONGIA)
                .HasPrecision(12, 2);

            modelBuilder.Entity<ORDER>()
                .Property(e => e.TONGTIEN)
                .HasPrecision(12, 2);

            modelBuilder.Entity<ORDER>()
                .HasMany(e => e.ORDER_DETAIL)
                .WithRequired(e => e.ORDER)
                .WillCascadeOnDelete(false);
        }
    }
}
