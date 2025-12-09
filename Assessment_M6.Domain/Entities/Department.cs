using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Assessment_M6.Domain.Entities;

public class Department
{
    [Key] public int Id { get; set; }
    [Column(TypeName = "varchar(100)")] public string Name { get; set; }
    [Column(TypeName = "varchar(200)")] public string Description { get; set; }
    
    //FK
    public LinkedList<Employee> Employees { get; set; } = new();
}