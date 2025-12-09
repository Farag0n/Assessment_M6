using Assessment_M6.Domain.Enums;

namespace Assessment_M6.Application.DTOs;

public class EmployeeDtos
{
    public class EmployeeCreateDto
    {
        public string Name { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public int Age { get; set; }
        public string DocNumber { get; set; }
        public string Email { get; set; }
        public string PhoneNumber { get; set; }
        public EmployeeState State { get; set; }
        public int DepartmentId { get; set; }
    }

    public class EmployeeUpdateDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public int Age { get; set; }
        public string DocNumber { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string PhoneNumber { get; set; } = string.Empty;
        public EmployeeState State { get; set; }
        public int DepartmentId { get; set; }
    }
    
    public class EmployeeResponseDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public int Age { get; set; }
        public string DocNumber { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string PhoneNumber { get; set; } = string.Empty;
        public EmployeeState State { get; set; }
        public DateTime RegisteredAt { get; set; }
        public DepartmentDtos.DepartmentResponseDto Department { get; set; }
    }
}