namespace Shared.DataTransferObjects
{
   [Serializable]
   public record CompanyDto(int Id, string Name, string Address, string Country);

    ////[Serializable]
    //public record CompanyDto
    //{
    //    public int Id { get; init; }
    //    public string? Name { get; init; }
    //    public string? FullAddress { get; init; }
    //}


}
