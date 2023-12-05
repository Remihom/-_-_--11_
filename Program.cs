using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

public class Animal
{
    public int Id { get; set; }
    public string Name { get; set; }
    public int Age { get; set; }
    public bool IsHealthy { get; set; }

    public int FarmId { get; set; } // Додали поле для зв'язку з фермою
    public Farm Farm { get; set; } // Додали посилання на ферму
}

public class Farm
{
    public int Id { get; set; }
    public string Name { get; set; }
    public List<Animal> Animals { get; set; }
}

public class MyDbContext : DbContext
{
    public DbSet<Animal> Animals { get; set; }
    public DbSet<Farm> Farms { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        var configuration = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.json")
            .Build();

        var connectionString = configuration.GetConnectionString("MyDbContext");
        optionsBuilder.UseSqlServer(connectionString);
      
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Animal>()
            .HasKey(a => a.Id); 

      /*  modelBuilder.Entity<Farm>()
            .HasKey(f => f.Id)
            .HasMany(f => f.Animals)
            .WithOne(a => a.Farm) // Вказали зв'язок з тваринами
            .HasForeignKey(a => a.FarmId)
            .OnDelete(DeleteBehavior.Cascade);*/ // Опціонально: вказали видалення тварин при видаленні ферми

        modelBuilder.Entity<Animal>()
            .Property(a => a.Name)
            .IsRequired()
            .HasMaxLength(50);
    }
}

public class DatabaseOperations
{
    private readonly MyDbContext _context;

    public DatabaseOperations(MyDbContext context)
    {
        _context = context;
        _context.Database.EnsureCreated();

    }

    public void SeedData()
    {
        var farm = new Farm { Name = "Farm1" };
        var animals = new List<Animal>
        {
            new Animal { Name = "Fluffy", Age = 3, IsHealthy = true, Farm = farm },
            new Animal { Name = "Buddy", Age = 5, IsHealthy = false, Farm = farm },
            // Додаткові тварини
        };

        _context.Animals.AddRange(animals);
        _context.SaveChanges();
    }

    public void AddAnimal(Animal animal)
    {
        _context.Animals.Add(animal);
        _context.SaveChanges();
    }

    public void UpdateAnimalAge(int animalId, int newAge)
    {
        var animal = _context.Animals.FirstOrDefault(a => a.Id == animalId);
        if (animal != null)
        {
            animal.Age = newAge;
            _context.SaveChanges();
        }
    }

    public void RemoveAnimal(int animalId)
    {
        var animal = _context.Animals.FirstOrDefault(a => a.Id == animalId);
        if (animal != null)
        {
            _context.Animals.Remove(animal);
            _context.SaveChanges();
        }
    }
}

class Program
{
    static void Main(string[] args)
    {
        using var context = new MyDbContext();

        var dbOperations = new DatabaseOperations(context);

        // Виклики методів для роботи з базою даних
        dbOperations.SeedData();

        // Приклад додавання нової тварини
        dbOperations.AddAnimal(new Animal { Name = "Snowball", Age = 2, IsHealthy = true, FarmId = 1 });

        // Приклад оновлення віку тварини
        dbOperations.UpdateAnimalAge(2, 4);

        // Приклад видалення тварини
        dbOperations.RemoveAnimal(1);

        // Додаткові операції з базою даних

        Console.WriteLine("Операції з базою даних завершено.");
    }
}

