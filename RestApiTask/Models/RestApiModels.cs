using System.ComponentModel.DataAnnotations;

namespace RestApiTask.Models;

public class ItemDetail
{
    [Required(ErrorMessage = "PartnerItemRef is required")]
    [StringLength(50, ErrorMessage = "PartnerItemRef cannot exceed 50 characters")]
    public string PartnerItemRef { get; set; } = string.Empty;

    [Required(ErrorMessage = "Name is required")]
    [StringLength(100, ErrorMessage = "Name cannot exceed 100 characters")]
    public string Name { get; set; } = string.Empty;

    [Range(1, 5, ErrorMessage = "Quantity must be between 1 and 5")]
    public int Qty { get; set; }

    [Range(1, long.MaxValue, ErrorMessage = "UnitPrice must be positive")]
    public long UnitPrice { get; set; }
}

public class RestApiRequest
{
    [Required(ErrorMessage = "PartnerKey is required")]
    [StringLength(50, ErrorMessage = "PartnerKey cannot challenged to 50 characters")]
    public string PartnerKey { get; set; } = string.Empty;

    [Required(ErrorMessage = "PartnerRefNo is required")]
    [StringLength(50, ErrorMessage = "PartnerRefNo cannot exceed 50 characters")]
    public string PartnerRefNo { get; set; } = string.Empty;

    [Required(ErrorMessage = "PartnerPassword is required")]
    [StringLength(50, ErrorMessage = "PartnerPassword cannot exceed 50 characters")]
    public string PartnerPassword { get; set; } = string.Empty;

    [Range(1, long.MaxValue, ErrorMessage = "TotalAmount must be positive")]
    public long TotalAmount { get; set; }

    public List<ItemDetail>? Items { get; set; }

    [Required(ErrorMessage = "Timestamp is required")]
    public string Timestamp { get; set; } = string.Empty;

    [Required(ErrorMessage = "Signature is required")]
    public string Sig { get; set; } = string.Empty;
}

public class RestApiResponse
{
    public int Result { get; set; }
    public long TotalAmount { get; set; }
    public long TotalDiscount { get; set; }
    public long FinalAmount { get; set; }
    public string? ResultMessage { get; set; }
}