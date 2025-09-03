using System.ComponentModel.DataAnnotations;
using testEFvsID.Models;

namespace testEFvsID.Areas.Blog.Models
{
    public class CreatePostModel:Post
    {
        [Display(Name ="Chuyên mục")]
        public int[] CategoryIDs {  get; set; }
    }
}
