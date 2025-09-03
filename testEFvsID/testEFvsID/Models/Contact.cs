using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace testEFvsID.Models
{
    public class Contact
    {
        [Key]
        public int Id { get; set; }
        [Column(TypeName = "nvarchar")]
        [StringLength(50)]
        [Required(ErrorMessage="Ko bỏ trống")]
        [Display(Name ="Họ tên")]
        public string FullName { get; set; }
        [StringLength(50)]
        [Required]
        public string Email { get; set; }
        public DateTime DateSent { get; set; }
        [Display(Name = "Nội dung")]
        public string Message { get; set; }
        [StringLength(50)]
        [Required(ErrorMessage = "Ko bỏ trống")]
        [Display(Name = "Số điện thoại")]
        public string Phone { get; set; }
    }
}
