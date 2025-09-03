using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace testEFvsID.Models
{
    
    public class Category
    {
        [Key]
        public int Id { get; set; }
       

         //tiêu đề
        [Required(ErrorMessage = "Phải có tên danh mục")]
        [StringLength(100, MinimumLength = 3, ErrorMessage = "{0} dài {1} đến {2}")]
        [Display(Name = "Tên danh mục")]
        public string Title { get; set; }
        
        //mô tả
        [DataType(DataType.Text)]
        [Display(Name = "Nội dung danh mục")]
        public string Description { get; set; }  

        //chuỗi url
        [Required(ErrorMessage ="Phải tạo url")]
        [StringLength(100,MinimumLength =3,ErrorMessage = "{0} dài {1} đến {2}")]
        [RegularExpression(@"^[a-z0-9-]*$",ErrorMessage ="Chỉ dùng các kí tự [a-z0-9-]")]
        [Display(Name ="Url hiển thị")]
        public string slug {  get; set; }

        //các category con
        public ICollection<Category> CategoryChildren { set; get; }
        //category cha (fkey)
        [Display(Name = "Danh mục cha")]
        public int? ParentCategoryId { get; set; }

        [Display(Name = "Danh mục cha")]
        [ForeignKey("ParentCategoryId")]
        public Category ParentCategory { set; get; }

        public void ChildCategoryIDs(ICollection<Category> childcates, List<int> list)
        {
            if (childcates == null)
                childcates = this.CategoryChildren;
            foreach(Category category in childcates)
            {
                list.Add(category.Id);
                ChildCategoryIDs(category.CategoryChildren,list);
            }
        }
        public List<Category> ListParents()
        {
            List<Category> li= new List<Category>();
            var parent = this.ParentCategory;
            while(parent != null)
            {
                li.Add(parent);
                parent = parent.ParentCategory;
            }
            li.Reverse();
            return li;

        }
    }
} 
