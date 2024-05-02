using System;
using System.Collections.Generic;
using System.Configuration;
using GAZON.Database.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using System.IO;

namespace GAZON.Database;

public partial class GazonContext : DbContext
{
    public GazonContext()
    {
    }

    public GazonContext(DbContextOptions<GazonContext> options)
        : base(options) 
    {
    }
    #region DBsets
    public virtual DbSet<Category> Categories { get; set; }

    public virtual DbSet<Item> Items { get; set; }

    public virtual DbSet<ItemCategory> ItemCategories { get; set; }

    public virtual DbSet<ItemReview> ItemReviews { get; set; }

    public virtual DbSet<Order> Orders { get; set; }

    public virtual DbSet<PickupPoint> PickupPoints { get; set; }

    public virtual DbSet<PickupPointOrder> PickupPointOrders { get; set; }

    public virtual DbSet<PointReview> PointReviews { get; set; }

    public virtual DbSet<Review> Reviews { get; set; }

    public virtual DbSet<ReviewAttachment> ReviewAttachments { get; set; }

    public virtual DbSet<Role> Roles { get; set; }

    public virtual DbSet<Staff> Staff { get; set; }

    public virtual DbSet<User> Users { get; set; }

    public virtual DbSet<UserFavorite> UserFavorites { get; set; }

    public virtual DbSet<Vendor> Vendors { get; set; }
    #endregion
    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        string connectionString = Environment.GetEnvironmentVariable("CONNECTION_STRING")!;
        optionsBuilder.UseSqlServer(connectionString);
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Category>(entity =>
        {
            entity.ToTable("CATEGORY");

            entity.Property(e => e.Id).HasColumnName("ID");
            entity.Property(e => e.MainCategory).HasColumnName("MAIN CATEGORY");
            entity.Property(e => e.Name)
                .HasMaxLength(50)
                .HasColumnName("NAME");

            entity.HasOne(d => d.MainCategoryNavigation).WithMany(p => p.InverseMainCategoryNavigation)
                .HasForeignKey(d => d.MainCategory)
                .HasConstraintName("FK_MAIN_CATEGORY");
        });

        modelBuilder.Entity<Item>(entity =>
        {
            entity.ToTable("ITEM");

            entity.Property(e => e.Id).HasColumnName("ID");
            entity.Property(e => e.Amount).HasColumnName("AMOUNT");
            entity.Property(e => e.Description)
                .HasMaxLength(250)
                .HasColumnName("DESCRIPTION");
            entity.Property(e => e.Name)
                .HasMaxLength(50)
                .HasColumnName("NAME");
            entity.Property(e => e.Picture).HasColumnName("PICTURE");
            entity.Property(e => e.Price)
                .HasColumnType("money")
                .HasColumnName("PRICE");
            entity.Property(e => e.Rating)
                .HasColumnType("decimal(2, 1)")
                .HasColumnName("RATING");
            entity.Property(e => e.Vendor).HasColumnName("VENDOR");

            entity.HasOne(d => d.VendorNavigation).WithMany(p => p.Items)
                .HasForeignKey(d => d.Vendor)
                .HasConstraintName("FK_ITEM_VENDOR");
        });

        modelBuilder.Entity<ItemCategory>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK_ITEM_CATEGORY");

            entity.ToTable("ITEM CATEGORY");

            entity.Property(e => e.Id).HasColumnName("ID");
            entity.Property(e => e.Category).HasColumnName("CATEGORY");
            entity.Property(e => e.Item).HasColumnName("ITEM");

            entity.HasOne(d => d.CategoryNavigation).WithMany(p => p.ItemCategories)
                .HasForeignKey(d => d.Category)
                .HasConstraintName("FK_CATEGORY_ITEM_CATEGORY");
        });

        modelBuilder.Entity<ItemReview>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK_ITEM_REVIEW");

            entity.ToTable("ITEM REVIEW");

            entity.HasIndex(e => e.Review, "UN_ITEM_REVIEW").IsUnique();

            entity.Property(e => e.Id).HasColumnName("ID");
            entity.Property(e => e.Item).HasColumnName("ITEM");
            entity.Property(e => e.Review).HasColumnName("REVIEW");

            entity.HasOne(d => d.ItemNavigation).WithMany(p => p.ItemReviews)
                .HasForeignKey(d => d.Item)
                .HasConstraintName("FK_ITEM_REVIEW_ITEM");
        });

        modelBuilder.Entity<Order>(entity =>
        {
            entity.ToTable("ORDER");

            entity.Property(e => e.Id).HasColumnName("ID");
            entity.Property(e => e.Item).HasColumnName("ITEM");
            entity.Property(e => e.OrderDate)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime")
                .HasColumnName("ORDER_DATE");
            entity.Property(e => e.PickupPoint).HasColumnName("PICKUP_POINT");
            entity.Property(e => e.User).HasColumnName("USER");

            entity.HasOne(d => d.ItemNavigation).WithMany(p => p.Orders)
                .HasForeignKey(d => d.Item)
                .HasConstraintName("FK_ORDER_ITEM");

            entity.HasOne(d => d.PickupPointNavigation).WithMany(p => p.Orders)
                .HasForeignKey(d => d.PickupPoint)
                .HasConstraintName("FK_ORDER_PICKUP_POINT");

            entity.HasOne(d => d.UserNavigation).WithMany(p => p.Orders)
                .HasForeignKey(d => d.User)
                .HasConstraintName("FK_ORDER_USER");
        });

        modelBuilder.Entity<PickupPoint>(entity =>
        {
            entity.ToTable("PICKUP_POINT");

            entity.Property(e => e.Id).HasColumnName("ID");
            entity.Property(e => e.Address)
                .HasMaxLength(30)
                .HasColumnName("ADDRESS");
            entity.Property(e => e.Picture)
                .HasMaxLength(1)
                .HasColumnName("PICTURE");
            entity.Property(e => e.Rating)
                .HasColumnType("decimal(2, 1)")
                .HasColumnName("RATING");
        });

        modelBuilder.Entity<PickupPointOrder>(entity =>
        {
            entity.ToTable("PICKUP_POINT_ORDER");

            entity.Property(e => e.Id).HasColumnName("ID");
            entity.Property(e => e.Order).HasColumnName("ORDER");
            entity.Property(e => e.PickupDate)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime")
                .HasColumnName("PICKUP_DATE");
            entity.Property(e => e.PickupPoint).HasColumnName("PICKUP_POINT");
            entity.Property(e => e.Staff).HasColumnName("STAFF");
            entity.Property(e => e.User).HasColumnName("USER");

            entity.HasOne(d => d.OrderNavigation).WithMany(p => p.PickupPointOrders)
                .HasForeignKey(d => d.Order)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_PICKUP_POINT_ORDER_ORDER");

            entity.HasOne(d => d.PickupPointNavigation).WithMany(p => p.PickupPointOrders)
                .HasForeignKey(d => d.PickupPoint)
                .HasConstraintName("FK_PICKUP_POINT_ORDER_PICKUP_POINT");

            entity.HasOne(d => d.StaffNavigation).WithMany(p => p.PickupPointOrders)
                .HasForeignKey(d => d.Staff)
                .HasConstraintName("FK_PICKUP_POINT_ORDER_STAFF");

            entity.HasOne(d => d.UserNavigation).WithMany(p => p.PickupPointOrders)
                .HasForeignKey(d => d.User)
                .HasConstraintName("FK_PICKUP_POINT_ORDER_USER");
        });

        modelBuilder.Entity<PointReview>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK_POINT_REVIEW");

            entity.ToTable("POINT REVIEW");

            entity.HasIndex(e => e.Review, "UN_POINT_REVIEW").IsUnique();

            entity.Property(e => e.Id).HasColumnName("ID");
            entity.Property(e => e.Point).HasColumnName("POINT");
            entity.Property(e => e.Review).HasColumnName("REVIEW");

            entity.HasOne(d => d.PointNavigation).WithMany(p => p.PointReviews)
                .HasForeignKey(d => d.Point)
                .HasConstraintName("FK_POINT_REVIEW_PICKUP_POINT");
        });

        modelBuilder.Entity<Review>(entity =>
        {
            entity.ToTable("REVIEW");

            entity.Property(e => e.Id).HasColumnName("ID");
            entity.Property(e => e.Content).HasColumnName("CONTENT");
            entity.Property(e => e.Date)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime")
                .HasColumnName("DATE");
            entity.Property(e => e.Rating)
                .HasColumnType("decimal(2, 1)")
                .HasColumnName("RATING");
            entity.Property(e => e.User).HasColumnName("USER");

            entity.HasOne(d => d.UserNavigation).WithMany(p => p.Reviews)
                .HasForeignKey(d => d.User)
                .HasConstraintName("FK_REVIEW_USER");
        });

        modelBuilder.Entity<ReviewAttachment>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK_ATTACHMENT");

            entity.ToTable("REVIEW ATTACHMENT");

            entity.Property(e => e.Id).HasColumnName("ID");
            entity.Property(e => e.Date)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime")
                .HasColumnName("DATE");
            entity.Property(e => e.Image).HasColumnName("IMAGE");
            entity.Property(e => e.Review).HasColumnName("REVIEW");

            entity.HasOne(d => d.ReviewNavigation).WithMany(p => p.ReviewAttachments)
                .HasForeignKey(d => d.Review)
                .HasConstraintName("FK_ATTACHMENT_REVIEW");
        });

        modelBuilder.Entity<Role>(entity =>
        {
            entity.ToTable("ROLE");

            entity.Property(e => e.Id).HasColumnName("ID");
            entity.Property(e => e.Name)
                .HasMaxLength(30)
                .HasColumnName("NAME");
        });

        modelBuilder.Entity<Staff>(entity =>
        {
            entity.ToTable("STAFF");

            entity.Property(e => e.Id).HasColumnName("ID");
            entity.Property(e => e.PickupPoint).HasColumnName("PICKUP_POINT");
            entity.Property(e => e.Picture)
                .HasMaxLength(1)
                .HasColumnName("PICTURE");
            entity.Property(e => e.User).HasColumnName("USER");

            entity.HasOne(d => d.PickupPointNavigation).WithMany(p => p.Staff)
                .HasForeignKey(d => d.PickupPoint)
                .HasConstraintName("FK_STAFF_PICKUP_POINT");

            entity.HasOne(d => d.UserNavigation).WithMany(p => p.Staff)
                .HasForeignKey(d => d.User)
                .HasConstraintName("FK_STAFF_USER");
        });

        modelBuilder.Entity<User>(entity =>
        {
            entity.ToTable("USER");

            entity.Property(e => e.Id).HasColumnName("ID");
            entity.Property(e => e.Email)
                .HasMaxLength(250)
                .HasColumnName("EMAIL");
            entity.Property(e => e.Login)
                .HasMaxLength(20)
                .HasColumnName("LOGIN");
            entity.Property(e => e.Name)
                .HasMaxLength(50)
                .HasColumnName("NAME");
            entity.Property(e => e.Password)
                .HasMaxLength(250)
                .HasColumnName("PASSWORD");
            entity.Property(e => e.Role).HasColumnName("ROLE");

            entity.HasOne(d => d.RoleNavigation).WithMany(p => p.Users)
                .HasForeignKey(d => d.Role)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_USER_ROLE");
        });

        modelBuilder.Entity<UserFavorite>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK_FAVORITE");

            entity.ToTable("USER FAVORITE");

            entity.Property(e => e.Id).HasColumnName("ID");
            entity.Property(e => e.Item).HasColumnName("ITEM");
            entity.Property(e => e.User).HasColumnName("USER");

            entity.HasOne(d => d.ItemNavigation).WithMany(p => p.UserFavorites)
                .HasForeignKey(d => d.Item)
                .HasConstraintName("FK_ITEM_FAVORITE");

            entity.HasOne(d => d.UserNavigation).WithMany(p => p.UserFavorites)
                .HasForeignKey(d => d.User)
                .HasConstraintName("FK_USER_FAVORITE");
        });

        modelBuilder.Entity<Vendor>(entity =>
        {
            entity.ToTable("VENDOR");

            entity.Property(e => e.Id).HasColumnName("ID");
            entity.Property(e => e.Description)
                .HasMaxLength(250)
                .HasColumnName("DESCRIPTION");
            entity.Property(e => e.Name)
                .HasMaxLength(50)
                .HasColumnName("NAME");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);

    
}
