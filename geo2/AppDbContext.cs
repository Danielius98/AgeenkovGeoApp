using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;

public class AppDbContext : DbContext
{
    public DbSet<Client> Clients { get; set; }
    public DbSet<Project> Projects { get; set; }
    public DbSet<Area> Areas { get; set; }
    public DbSet<Profile> Profiles { get; set; }
    public DbSet<Measurement> Measurements { get; set; }
    public DbSet<ProjectProfile> ProjectProfiles { get; set; }
    public DbSet<User> Users { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        optionsBuilder.UseSqlServer(@"Server=DESKTOP-PU1AE3A\QWE;Database=AeroSpectroDB4;Trusted_Connection=True;TrustServerCertificate=True");
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<ProjectProfile>()
            .HasKey(pp => new { pp.ProjectID, pp.ProfileID });

        modelBuilder.Entity<ProjectProfile>()
            .HasOne(pp => pp.Project)
            .WithMany(p => p.ProjectProfiles)
            .HasForeignKey(pp => pp.ProjectID);

        modelBuilder.Entity<ProjectProfile>()
            .HasOne(pp => pp.Profile)
            .WithMany(p => p.ProjectProfiles)
            .HasForeignKey(pp => pp.ProfileID);

        modelBuilder.Entity<Project>()
            .HasOne(p => p.Client)
            .WithMany(c => c.Projects)
            .HasForeignKey(p => p.ClientID);

        modelBuilder.Entity<Area>()
            .HasOne(a => a.Project)
            .WithMany(p => p.Areas)
            .HasForeignKey(a => a.ProjectID);

        modelBuilder.Entity<Profile>()
            .HasOne(p => p.Area)
            .WithMany(a => a.Profiles)
            .HasForeignKey(p => p.AreaID);

        modelBuilder.Entity<Measurement>()
            .HasOne(m => m.Profile)
            .WithMany(p => p.Measurements)
            .HasForeignKey(m => m.ProfileID);

        modelBuilder.Entity<Measurement>()
            .Property(m => m.SpectrumData)
            .HasColumnType("nvarchar(max)");

        modelBuilder.Entity<Measurement>()
            .Property(m => m.SpectrumChannels)
            .HasDefaultValue(0);

        modelBuilder.Entity<Measurement>()
            .Property(m => m.SpectrumEnergyMin)
            .HasDefaultValue(0);

        modelBuilder.Entity<Measurement>()
            .Property(m => m.SpectrumEnergyMax)
            .HasDefaultValue(0);

        modelBuilder.Entity<User>()
            .HasIndex(u => u.Username)
            .IsUnique();
    }
}

public class Client
{
    public int ClientID { get; set; }
    public int? UserID { get; set; } // Внешний ключ на таблицу Users
    public string Name { get; set; }
    public string ContactInfo { get; set; }
    public virtual User User { get; set; } // Навигационное свойство
    public virtual ICollection<Project> Projects { get; set; } = new HashSet<Project>();
}

public class Project
{
    public int ProjectID { get; set; }
    public int ClientID { get; set; }
    public string Name { get; set; }
    public string ContractNumber { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public string Description { get; set; }
    public virtual Client Client { get; set; }
    public virtual ICollection<Area> Areas { get; set; } = new HashSet<Area>();
    public virtual ICollection<ProjectProfile> ProjectProfiles { get; set; } = new HashSet<ProjectProfile>();
}

public class Area
{
    public int AreaID { get; set; }
    public int ProjectID { get; set; }
    public string Name { get; set; }
    public string Coordinates { get; set; }
    public virtual Project Project { get; set; }
    public virtual ICollection<Profile> Profiles { get; set; } = new HashSet<Profile>();
}

public class Profile
{
    public int ProfileID { get; set; }
    public int? AreaID { get; set; }
    public string Name { get; set; }
    public string Type { get; set; }
    public string StartCoordinates { get; set; }
    public string EndCoordinates { get; set; }
    public virtual Area Area { get; set; }
    public virtual ICollection<ProjectProfile> ProjectProfiles { get; set; } = new HashSet<ProjectProfile>();
    public virtual ICollection<Measurement> Measurements { get; set; } = new HashSet<Measurement>();
}

public class Measurement
{
    public int MeasurementID { get; set; }
    public int ProfileID { get; set; }
    public DateTime Timestamp { get; set; }
    public double Latitude { get; set; }
    public double Longitude { get; set; }
    public double GammaValue { get; set; }
    public double Altitude { get; set; }
    public string SpectrumData { get; set; }
    public int SpectrumChannels { get; set; }
    public double SpectrumEnergyMin { get; set; }
    public double SpectrumEnergyMax { get; set; }
    public virtual Profile Profile { get; set; }
}

public class ProjectProfile
{
    public int ProjectID { get; set; }
    public int ProfileID { get; set; }
    public virtual Project Project { get; set; }
    public virtual Profile Profile { get; set; }
}

public class User
{
    public int UserID { get; set; }
    public string Username { get; set; }
    public string PasswordHash { get; set; }

    public static string HashPassword(string password)
    {
        using (var sha256 = SHA256.Create())
        {
            var bytes = Encoding.UTF8.GetBytes(password);
            var hash = sha256.ComputeHash(bytes);
            return Convert.ToBase64String(hash);
        }
    }

    public static bool VerifyPassword(string password, string passwordHash)
    {
        return HashPassword(password) == passwordHash;
    }
}