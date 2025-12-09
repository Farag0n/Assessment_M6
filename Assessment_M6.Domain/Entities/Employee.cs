using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Assessment_M6.Domain.Enums;

namespace Assessment_M6.Domain.Entities;

public class Employee
{
    [Key] public int Id { get; set; }
    [Column(TypeName = "varchar(100)")] public string Name { get; set; }
    [Column(TypeName = "varchar(100)")] public string LastName { get; set; }
    public int Age { get; set; }
    [Column(TypeName = "varchar(100)")] public string DocNumber { get; set; }
    [Column(TypeName = "varchar(100)")] public string Email { get; set; }
    [Column(TypeName = "varchar(50)")] public string PhoneNumber { get; set; }
    public EmployeeState State { get; set; }
    public DateTime RegisteredAt { get; set; } = DateTime.UtcNow;
    
    //FK
    public int DepartmentId { get; set; }
    public Department? Department { get; set; }
}