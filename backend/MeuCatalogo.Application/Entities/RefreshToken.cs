using System.ComponentModel.DataAnnotations;

namespace MeuCatalogo.Application.Entities;

public class RefreshToken : BaseEntity
{
    [Required]
    public string Token { get; set; }
    
    [Required]
    public string UserId { get; set; }
    
    public DateTime ExpiresAt { get; set; }
    
    public bool IsRevoked { get; set; }
    
    public DateTime? RevokedAt { get; set; }
    
    // Navigation property
    public virtual ApplicationUser User { get; set; }
}
