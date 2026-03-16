namespace hotelier_core_app.Model.DTOs.Request;

public class PaginationInputDTO
{
    /// <summary>
    /// Gets or sets the current page number. Defaults to 1.
    /// </summary>
    public int PageNumber { get; set; } = 1;

    /// <summary>
    /// Gets or sets the number of items per page. Defaults to 10.
    /// </summary>
    public int PageSize { get; set; } = 10;
}