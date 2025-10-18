using System.ComponentModel.DataAnnotations;

namespace Tagom.Domain.Entities
{
    public class Supplier 
    {
        public int SupplierId { get; set; }
        public string Name { get; set; } = null!;
        [Phone,MinLength(11),MaxLength(11)]
        [RegularExpression(@"^01[0-2,5]{1}[0-9]{8}$", ErrorMessage = "Invalid phone number")]
        public string? Phone { get; set; }
    }

}
