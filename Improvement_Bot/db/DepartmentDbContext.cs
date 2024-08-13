using Microsoft.EntityFrameworkCore;
 
namespace Improvement_Bot.db;

 using Improvement_Bot.db;
using Improvement_Bot;
using Microsoft.EntityFrameworkCore;


public partial class DepartmentDbContext : DbContext
{
    public DepartmentDbContext()
    {
    }
    public DepartmentDbContext(DbContextOptions<DepartmentDbContext> options)
      : base(options)
    {

    }

    public virtual DbSet<User> User { get; set; }

    public virtual DbSet<Report> Report { get; set; }

    public virtual DbSet<Role> Role { get; set; }


    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Report>(entity =>
        {
            entity.HasKey(e => e.id_Report);

            entity.Property(e => e.id_Report).HasColumnName("id_Report");
            entity.Property(e => e.Manager_Comment).HasColumnType("text");

            entity.HasOne(d => d.id_OrderNavigation)
                  .WithMany(p => p.Reports)
                  .HasForeignKey(d => d.id_Order)
                  .OnDelete(DeleteBehavior.Cascade)
                  .HasConstraintName("FK_Reports_Orders");
            entity.HasMany(u => u.Images)
                .WithOne(r => r.id_ReportNavigation)
                .HasForeignKey(r => r.id_Report_Image)
                .OnDelete(DeleteBehavior.Cascade)
                .HasConstraintName("FK_Reports_Images");
        });
        modelBuilder.Entity<Report_Image>(entity =>
        {
            entity.HasKey(e => e.id_Report_Image);


            entity.HasOne(d => d.id_ReportNavigation)
                  .WithMany(p => p.Images)
                  .HasForeignKey(d => d.id_Report)
                  .OnDelete(DeleteBehavior.Cascade)
                  .HasConstraintName("FK_Reports_Images");

        });

        modelBuilder.Entity<Line>(entity =>
        {
        
            entity.HasKey(e => e.id_Line);

            entity.Property(e => e.id_Line).HasColumnName("id_Line");
 
            entity.HasOne(d => d.id_Point1Navigation)
                  .WithMany(p => p.Lines1)
                  .HasForeignKey(d => d.id_Point1)
                  .OnDelete(DeleteBehavior.Cascade)
                  .HasConstraintName("FK_Lines_Points1");

            entity.HasOne(d => d.id_Point2Navigation)
                  .WithMany(p => p.Lines2)
                  .HasForeignKey(d => d.id_Point2)
                  .OnDelete(DeleteBehavior.Cascade)
                  .HasConstraintName("FK_Lines_Points2");
        });

        modelBuilder.Entity<Role>(entity =>
        {
            entity.HasKey(e => e.id_Role);

            entity.Property(e => e.id_Role).HasColumnName("id_Role");

            entity.HasMany(u => u.Users)
                 .WithOne(r => r.id_RoleNavigation)
                 .HasForeignKey(r => r.id_Role)
                 .OnDelete(DeleteBehavior.Cascade)
                 .HasConstraintName("FK_Roles_Users");
        });

        modelBuilder.Entity<Point>(entity =>
        {
            entity.HasKey(e => e.id_Point);

            entity.Property(e => e.id_Point).HasColumnName("id_Point");
            //   entity.Property(e => e.Manager_Comment).HasColumnType("text");

            entity.HasOne(d => d.id_Point_ImageNavigation)
                  .WithMany(p => p.Points)
                  .HasForeignKey(d => d.Image)
                  .OnDelete(DeleteBehavior.Cascade)
                  .HasConstraintName("FK_Images_Points");

            entity.HasOne(d => d.id_UserNavigation)
       .WithMany(p => p.Points) // Предполагаем, что у User есть свойство PointsWithUser
       .HasForeignKey(d => d.id_User)
       .OnDelete(DeleteBehavior.Cascade)
       .HasConstraintName("FK_Users_Points");
        });

        modelBuilder.Entity<Order>(entity =>
        {
            entity.HasKey(e => e.id_Order);


            entity.HasMany(u => u.Reports)
                 .WithOne(r => r.id_OrderNavigation)
                 .HasForeignKey(r => r.id_Order)
                 .OnDelete(DeleteBehavior.Cascade)
                 .HasConstraintName("FK_Orders_Reports");

            entity.HasOne(d => d.id_ExecutorNavigation)
       .WithMany(p => p.Orders1)  
       .HasForeignKey(d => d.id_Executor)
       .OnDelete(DeleteBehavior.Cascade)
       .HasConstraintName("FK_Orders_Users1");
            entity.HasOne(d => d.id_SupervisorNavigation)
    .WithMany(p => p.Orders2)
    .HasForeignKey(d => d.id_Supervisor)
    .OnDelete(DeleteBehavior.Cascade)
    .HasConstraintName("FK_Orders_Users2");
        });
        modelBuilder.Entity<Point_Image>(entity =>
        {
            entity.HasKey(e => e.id_Point_Image);

            entity.Property(e => e.id_Point_Image).HasColumnName("id_Point_Image");

            entity.HasMany(u => u.Points)
                 .WithOne(r => r.id_Point_ImageNavigation)
                 .HasForeignKey(r => r.Image)
                 .OnDelete(DeleteBehavior.Cascade)
                 .HasConstraintName("FK_Point_Images");
        });


        modelBuilder.Entity<User>(entity =>
        {
            entity.HasKey(e => e.id_User);

            entity.Property(e => e.id_User).HasColumnName("id_User");
            entity.Property(e => e.FirstName).HasColumnName("FirstName");
            entity.Property(e => e.MiddleName).HasColumnName("MiddleName");
            entity.Property(e => e.LastName).HasColumnName("LastName");
            entity.Property(e => e.Login).HasColumnName("Login");
            entity.Property(e => e.Password).HasColumnName("Password");

            entity.HasMany(u => u.Orders1)
                  .WithOne(r => r.id_ExecutorNavigation)
                  .HasForeignKey(r => r.id_Executor)
                  .OnDelete(DeleteBehavior.Cascade)
                  .HasConstraintName("FK_Order_Users1");
            entity.HasMany(u => u.Orders2)
                  .WithOne(r => r.id_SupervisorNavigation)
                  .HasForeignKey(r => r.id_Supervisor)
                  .OnDelete(DeleteBehavior.Cascade)
                  .HasConstraintName("FK_Order_Users2");
            entity.HasMany(u => u.Points)
                  .WithOne(r => r.id_UserNavigation)
                  .HasForeignKey(r => r.id_User)
                  .OnDelete(DeleteBehavior.Cascade)
                  .HasConstraintName("FK_Users_Points");
            entity.HasOne(d => d.id_RoleNavigation)
                 .WithMany(p => p.Users)
                 .HasForeignKey(d => d.id_Role)
                 .OnDelete(DeleteBehavior.Cascade)
                 .HasConstraintName("FK_Roles_Users");
        });
    }

    public DbSet<Point> Point { get; set; } = default!;

    public DbSet<Point_Image> Point_Image { get; set; } = default!;

}
