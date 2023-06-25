using Microsoft.EntityFrameworkCore;
using ProductManagement.Models;

namespace ProductManagement.EF
{
    public class ProductManagementDbContext: DbContext
    {
        public DbSet<Product> Products { get; set; }
        public DbSet<Warehouse> Warehouses { get; set; }
        public DbSet<StoredProduct> StoredProducts { get; set; }

        public ProductManagementDbContext(DbContextOptions<ProductManagementDbContext> options)
            : base(options)
        {
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Product>(entity =>
            {
                entity.ToTable("products", "public");

                entity.Property(e => e.Id)
                    .HasColumnName("id")
                    .ValueGeneratedOnAdd()
                    .IsRequired();

                entity.Property(e => e.Name)
                    .HasColumnName("name")
                    .IsRequired();

                entity.Property(e => e.Price)
                    .HasColumnName("price")
                    .HasColumnType("numeric(10,2)")
                    .HasDefaultValue(0.00m);
            });

            modelBuilder.Entity<Warehouse>(entity =>
            {
                entity.ToTable("warehouses", "public");

                entity.Property(e => e.Id)
                    .HasColumnName("id")
                    .ValueGeneratedOnAdd()
                    .IsRequired();

                entity.Property(e => e.Zip)
                    .HasColumnName("zip");
            });

            modelBuilder.Entity<StoredProduct>(entity =>
            {
                entity.ToTable("stored_products", "public");

                entity.HasKey(e => new { e.WarehouseId, e.ProductUUID, e.ProductId })
                    .HasName("stored_products_pkey");

                entity.Property(e => e.WarehouseId)
                    .HasColumnName("warehouse_id")
                    .IsRequired();

                entity.Property(e => e.ProductId)
                    .HasColumnName("product_id")
                    .IsRequired();

                entity.Property(e => e.ProductUUID)
                    .HasColumnName("product_uuid")
                    .IsRequired();

                entity.HasOne(d => d.Product)
                    .WithMany(p => p.StoredProducts)
                    .HasForeignKey(d => d.ProductId)
                    .HasConstraintName("fk_stored_products_product");

                entity.HasOne(d => d.Warehouse)
                    .WithMany(p => p.StoredProducts)
                    .HasForeignKey(d => d.WarehouseId)
                    .HasConstraintName("fk_warehouses_stored_products");
            });

            modelBuilder.Entity<OrderResult>(entity =>
            {
                entity.ToTable("order_results");
                entity.HasKey(or => or.Id);
                entity.Property(or => or.Id).HasColumnName("id");
                entity.Property(or => or.CustomerId).HasColumnName("customer_id");
                entity.Property(or => or.TotalAmount).HasColumnName("total_amount");
                entity.HasMany(or => or.OrderResultItems)
                      .WithOne(ori => ori.OrderResult)
                      .HasForeignKey(ori => ori.OrderResultId)
                      .OnDelete(DeleteBehavior.Cascade); // If an OrderResult is deleted, all associated OrderResultItems will also be deleted.
            });

            modelBuilder.Entity<OrderResultItem>(entity =>
            {
                entity.ToTable("order_result_items");
                entity.HasKey(ori => ori.Id);
                entity.Property(ori => ori.Id).HasColumnName("id");
                entity.Property(ori => ori.ProductId).HasColumnName("product_id");
                entity.Property(ori => ori.ProductUUID).HasColumnName("product_uuid");
                entity.Property(ori => ori.WarehouseId).HasColumnName("warehouse_id");
                entity.Property(ori => ori.OrderResultId).HasColumnName("order_result_id");
                entity.HasOne(ori => ori.Product)
                      .WithMany()
                      .HasForeignKey(ori => ori.ProductId)
                      .OnDelete(DeleteBehavior.Restrict); // If an OrderResultItem is deleted, the associated Product will not be deleted.
                entity.HasOne(ori => ori.Warehouse)
                      .WithMany()
                      .HasForeignKey(ori => ori.WarehouseId)
                      .OnDelete(DeleteBehavior.Restrict); // If an OrderResultItem is deleted, the associated Warehouse will not be deleted.
            });

            base.OnModelCreating(modelBuilder);
        }

    }
}