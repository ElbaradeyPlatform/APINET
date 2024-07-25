namespace Shared.DataTransferObjects
{
    [Serializable]
    public record EmployeeDto(int Id, string Name, int Age, string Position);

}
