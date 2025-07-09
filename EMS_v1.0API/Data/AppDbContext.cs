using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

public class AppDbContext : DbContext
{
    public DbSet<User> Users { get; set; }
    public DbSet<OrganizationUnit> OrganizationUnits { get; set; }
    public DbSet<Position> Positions { get; set; }
    public DbSet<Employee> Employees { get; set; }
    public DbSet<Employee_CD> Employee_CDs { get; set; }
    public DbSet<Employee_AS> Employee_ASs { get; set; }
    public DbSet<Employee_CL> Employee_CLs { get; set; }
    public DbSet<Employee_Application> Employee_Applications { get; set; }
    public DbSet<Employee_Relatives> Employee_Relatives { get; set; }
    public DbSet<Employee_Todolist> Employee_Todolists { get; set; }
    public DbSet<Employee_Salary> Employee_Salaries { get; set; }
    public DbSet<Employee_Insurance> Employee_Insurances { get; set; }

    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // User configuration
        modelBuilder.Entity<User>()
            .HasIndex(u => u.Username)
            .IsUnique();

        // OrganizationUnit configuration
        modelBuilder.Entity<OrganizationUnit>()
            .Property(o => o.UnitType)
            .HasConversion<string>();

        // Employee configuration
        modelBuilder.Entity<Employee>()
            .HasIndex(e => e.Email)
            .IsUnique();

        modelBuilder.Entity<Employee>()
            .Property(e => e.Gender)
            .HasConversion<string>();

        modelBuilder.Entity<Employee>()
            .Property(e => e.Source)
            .HasConversion<string>();

        // Employee_CD configuration
        modelBuilder.Entity<Employee_CD>()
            .HasIndex(e => e.IdNumber)
            .IsUnique();

        // Position configuration
        modelBuilder.Entity<Position>()
            .HasIndex(p => p.PositionName)
            .IsUnique();

        // Employee_Relatives configuration
        modelBuilder.Entity<Employee_Relatives>()
            .Property(e => e.RelId)
            .ValueGeneratedOnAdd(); // Cấu hình RelId tự tăng

        // Check constraints
        modelBuilder.Entity<OrganizationUnit>()
            .ToTable(t => t.HasCheckConstraint("CK_OrganizationUnit_UnitType",
                "[UnitType] IN (N'PhongBan', N'Nhom')"));

        modelBuilder.Entity<Employee>()
            .ToTable(t => t.HasCheckConstraint("CK_Employee_Gender",
                "[Gender] IN (N'Nam', N'Nữ', N'Khác') OR [Gender] IS NULL"));

        modelBuilder.Entity<Employee>()
            .ToTable(t => t.HasCheckConstraint("CK_Employee_Source",
                "[Source] IN (N'Nội tuyến', N'Ngoại tuyến')"));

        modelBuilder.Entity<Employee_Application>()
            .ToTable(t => t.HasCheckConstraint("CK_Employee_Application_Type",
                "[ApplicationType] IN (N'xác nhận', N'đơn', N'hợp đồng lao động')"));

        modelBuilder.Entity<Employee_Relatives>()
            .ToTable(t => t.HasCheckConstraint("CK_Employee_Relatives_Type",
                "[Type] IN (0, 1)"));

        modelBuilder.Entity<Employee_Todolist>()
            .ToTable(t => t.HasCheckConstraint("CK_Employee_Todolist_Status",
                "[Status] IN (0, 1)"));

        modelBuilder.Entity<Employee_Insurance>()
            .ToTable(t => t.HasCheckConstraint("CK_Employee_Insurance_ContributePercent",
                "[ContributePercent] BETWEEN 0 AND 100"));

        modelBuilder.Entity<Employee_CL>()
            .ToTable(t => t.HasCheckConstraint("CK_Employee_CL_Status",
                "[Status] IN (N'Hiệu lực', N'Không Hiệu lực')"));

        modelBuilder.Entity<Employee_CL>()
            .ToTable(t => t.HasCheckConstraint("CK_Employee_CL_Type",
                "[Type] IN (N'Thử việc', N'Có thời hạn', N'Vô thời hạn')"));

        // Relationships
        modelBuilder.Entity<User>()
            .HasOne(u => u.Employee)
            .WithOne(e => e.User)
            .HasForeignKey<User>(u => u.Eid)
            .IsRequired(false);

        modelBuilder.Entity<Employee>()
            .HasOne(e => e.OrganizationUnit)
            .WithMany(o => o.Employees)
            .HasForeignKey(e => e.UnitId)
            .IsRequired(true);

        modelBuilder.Entity<Employee>()
            .HasOne(e => e.Position)
            .WithMany(p => p.Employees)
            .HasForeignKey(e => e.PositionId)
            .IsRequired(true);

        modelBuilder.Entity<OrganizationUnit>()
            .HasOne(o => o.Parent)
            .WithMany(o => o.Children)
            .HasForeignKey(o => o.ParentId)
            .IsRequired(false);

        modelBuilder.Entity<Employee_CD>()
            .HasOne(e => e.Employee)
            .WithOne(e => e.EmployeeCD)
            .HasForeignKey<Employee_CD>(e => e.Eid)
            .IsRequired(false);

        modelBuilder.Entity<Employee_AS>()
            .HasOne(e => e.Employee)
            .WithOne(e => e.EmployeeAS)
            .HasForeignKey<Employee_AS>(e => e.Eid)
            .IsRequired(false);

        modelBuilder.Entity<Employee_CL>()
            .HasOne(e => e.Employee)
            .WithOne(e => e.EmployeeCL)
            .HasForeignKey<Employee_CL>(e => e.Eid)
            .IsRequired(false);

        modelBuilder.Entity<Employee_Salary>()
            .HasOne(e => e.Employee)
            .WithOne(e => e.EmployeeSalary)
            .HasForeignKey<Employee_Salary>(e => e.Eid)
            .IsRequired(false);

        modelBuilder.Entity<Employee_Insurance>()
            .HasOne(e => e.Employee)
            .WithOne(e => e.EmployeeInsurance)
            .HasForeignKey<Employee_Insurance>(e => e.Eid)
            .IsRequired(false);

        modelBuilder.Entity<Employee_Application>()
            .HasOne(e => e.Employee)
            .WithMany(e => e.Applications)
            .HasForeignKey(e => e.Eid)
            .IsRequired(false);

        modelBuilder.Entity<Employee_Relatives>()
            .HasOne(e => e.Employee)
            .WithMany(e => e.Relatives)
            .HasForeignKey(e => e.Eid)
            .IsRequired(false);

        modelBuilder.Entity<Employee_Todolist>()
            .HasOne(e => e.Employee)
            .WithMany(e => e.TodoList)
            .HasForeignKey(e => e.Eid)
            .IsRequired(false);

        // Seed data - Sample Organization Units
        modelBuilder.Entity<OrganizationUnit>().HasData(
            new OrganizationUnit { UnitId = 1, UnitName = "Phòng Nhân Sự", UnitType = "PhongBan", ParentId = null },
            new OrganizationUnit { UnitId = 2, UnitName = "Phòng Kỹ Thuật", UnitType = "PhongBan", ParentId = null },
            new OrganizationUnit { UnitId = 3, UnitName = "Nhóm Phát Triển", UnitType = "Nhom", ParentId = 2 }
        );

        // Seed data - Sample Positions
        modelBuilder.Entity<Position>().HasData(
            new Position { PositionId = 1, PositionName = "Giám Đốc" },
            new Position { PositionId = 2, PositionName = "Trưởng Phòng" },
            new Position { PositionId = 3, PositionName = "Trưởng nhóm" },
            new Position { PositionId = 4, PositionName = "Nhân viên" },
            new Position { PositionId = 5, PositionName = "Cán bộ nhân sự" },
            new Position { PositionId = 6, PositionName = "Cán bộ lương" },
            new Position { PositionId = 7, PositionName = "Cán bộ bảo hiểm" }
        );


        modelBuilder.Entity<Employee>().HasData(
            new Employee
            {
                Eid = 1,
                Name = "Nguyễn Văn An",
                Email = "an.nguyen@example.com",
                UnitId = 1,
                PositionId = 2,
                Gender = "Nam",
                Source = "Nội tuyến",
                DoB = new DateTime(1985, 5, 15),
                Address = "123 Đường Láng, Hà Nội",
                Phone = "0901234567",
                Experience = 10,
                Bank = "Vietcombank",
                BankNumber = "1234567890",
                Img = "an_nguyen.jpg"
            },
            new Employee
            {
                Eid = 2,
                Name = "Trần Thị Hương",
                Email = "huong.tran@example.com",
                UnitId = 1,
                PositionId = 5,
                Gender = "Nữ",
                Source = "Nội tuyến",
                DoB = new DateTime(1990, 8, 22),
                Address = "456 Đường Giải Phóng, Hà Nội",
                Phone = "0912345678",
                Experience = 5,
                Bank = "Techcombank",
                BankNumber = "2345678901",
                Img = "huong_tran.jpg"
            },
            new Employee
            {
                Eid = 3,
                Name = "Lê Minh Tuấn",
                Email = "tuan.le@example.com",
                UnitId = 2,
                PositionId = 2,
                Gender = "Nam",
                Source = "Nội tuyến",
                DoB = new DateTime(1982, 3, 10),
                Address = "789 Đường Nguyễn Trãi, Hà Nội",
                Phone = "0923456789",
                Experience = 12,
                Bank = "BIDV",
                BankNumber = "3456789012",
                Img = "tuan_le.jpg"
            },
            new Employee
            {
                Eid = 4,
                Name = "Phạm Thái Mai",
                Email = "mai.ph@example.com",
                UnitId = 3,
                PositionId = 3,
                Gender = "Nữ",
                Source = "Ngoại tuyến",
                DoB = new DateTime(1988, 12, 5),
                Address = "101 Đường Lê Lợi, Hà Nội",
                Phone = "0934567890",
                Experience = 7,
                Bank = "MB Bank",
                BankNumber = "4567890123",
                Img = "mai_pham.jpg"
            },
            new Employee
            {
                Eid = 5,
                Name = "Hoàng Văn Nam",
                Email = "nam.hoang@example.com",
                UnitId = 3,
                PositionId = 4,
                Gender = "Nam",
                Source = "Nội tuyến",
                DoB = new DateTime(1995, 7, 30),
                Address = "202 Đường Trần Phú, Hà Nội",
                Phone = "0945678901",
                Experience = 3,
                Bank = "VP Bank",
                BankNumber = "5678901234",
                Img = "nam_hoang.jpg"
            }
        );

        // Seed data - Users
        modelBuilder.Entity<User>().HasData(
            new User { Uid = 1, Username = "admin", Password = BCrypt.Net.BCrypt.HashPassword("admin123"), Eid = null },
            new User { Uid = 2, Username = "an.nguyen", Password = BCrypt.Net.BCrypt.HashPassword("password123"), Eid = 1 },
            new User { Uid = 3, Username = "huong.tran", Password = BCrypt.Net.BCrypt.HashPassword("password123"), Eid = 2 },
            new User { Uid = 4, Username = "tuan.le", Password = BCrypt.Net.BCrypt.HashPassword("password123"), Eid = 3 },
            new User { Uid = 5, Username = "mai.ph", Password = BCrypt.Net.BCrypt.HashPassword("password123"), Eid = 4 },
            new User { Uid = 6, Username = "nam.hoang", Password = BCrypt.Net.BCrypt.HashPassword("password123"), Eid = 5 }
        );
    }
}