namespace hotelier_core_app.Model.Interfaces
{
    public interface IBaseEntity
    {
        string? CreatedBy { get; set; }
        string? ModifiedBy { get; set; }
        DateTime CreationDate { get; set; }
        DateTime? LastModifiedDate { get; set; }
        bool IsDeleted { get; set; }
    }
}
