using Bogus;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using testEFvsID.Data;
using testEFvsID.Models;

namespace testEFvsID.Areas.Database.Controllers
{
    [Area("Database")]
    [Route("/database-manage/[action]")]
    public class DBManageController : Controller
    {
        private readonly AppDBContext _dbContext;
        private readonly UserManager<AppUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;

        public DBManageController(AppDBContext dbContext, UserManager<AppUser> userManager, RoleManager<IdentityRole> roleManager)
        {
            _dbContext = dbContext;
            _userManager = userManager;
            _roleManager = roleManager;
        }

        public IActionResult Index()
        {
            return View();
        }
        [HttpGet]
        public IActionResult DeleteDb()
        {
            return View();
        }
        [TempData]
        public string StatusMessage {  get; set; }
        [HttpPost]
        public async Task<IActionResult> DeleteDbAsync()
        {
           var success= await _dbContext.Database.EnsureDeletedAsync();
            StatusMessage = success ? "Xoa thanh cong" : "Ko xoa duoc";
            return RedirectToAction("Index");
        }
        [HttpPost]
        public async Task<IActionResult> Migrate()
        {
            await _dbContext.Database.MigrateAsync();
            StatusMessage = "Cập nhật thành công";
            return RedirectToAction("Index");
        }
        public async Task<IActionResult> SeedDataAsync()
        {
            var rolenames = typeof(RoleName).GetFields().ToList();
            foreach (var r in rolenames)
            {
                var rolename=(string)r.GetRawConstantValue();
                var rfound = await _roleManager.FindByNameAsync(rolename);
                if(rfound == null)
                {
                    await _roleManager.CreateAsync(new IdentityRole(rolename));
                }

            }
            //admin, pass=admin123, admin@example.com
            var useradmin = await _userManager.FindByEmailAsync("admin@example.com");
            if(useradmin== null)
            {
                useradmin = new AppUser()
                {
                    UserName = "admin",
                    Email = "admin@example.com",
                    EmailConfirmed = true,
                };
                await _userManager.CreateAsync(useradmin,"admin123");
                await _userManager.AddToRoleAsync(useradmin, RoleName.Administrator);
            }
            SeedPostCategory();
            StatusMessage = "VUA SEED DATA";
            return RedirectToAction("index");
        }
        //tạo category data fake
        private void SeedPostCategory()
        {
            _dbContext.Categories.RemoveRange(_dbContext.Categories.Where(c => c.Description.Contains("[fakeData]")));
            _dbContext.Posts.RemoveRange(_dbContext.Posts.Where(c => c.Content.Contains("[fakeData]")));

            var fakeCategory = new Faker<Category>();
            int cm = 1;
            fakeCategory.RuleFor(c => c.Title, fk => $"CM{cm++} " + fk.Lorem.Sentence(1, 2).Trim('.'));
            fakeCategory.RuleFor(c => c.Description, fk => fk.Lorem.Sentence(5) + "[fakeData]");
            fakeCategory.RuleFor(c => c.slug, fk => fk.Lorem.Slug());

            var cate1 = fakeCategory.Generate();
            var cate11 = fakeCategory.Generate();
            var cate12= fakeCategory.Generate();
            var cate2=fakeCategory.Generate();
            var cate21=fakeCategory.Generate();
            var cate211=fakeCategory.Generate();

            cate11.ParentCategory = cate1;
            cate12.ParentCategory = cate1;
            cate21.ParentCategory = cate2;
            cate211.ParentCategory = cate21;

            var categories = new Category[] { cate1, cate2, cate12, cate11, cate21, cate211 };
            _dbContext.Categories.AddRange(categories);

            /// fake post data
            var rCateIndex = new Random();
            int bv = 1;
            var user = _userManager.GetUserAsync(this.User).Result;
            var fakePost = new Faker<Post>();
            fakePost.RuleFor(p => p.AuthorId, f => user.Id);
            fakePost.RuleFor(p => p.Content, f => f.Lorem.Paragraphs(7)+"[fakeData]");
            fakePost.RuleFor(p => p.Thumbnail, f => "/uploads/No_Image_Available.jpg");
            fakePost.RuleFor(p => p.DateCreated, f => f.Date.Between(new DateTime(2021,1,1),new DateTime(2021,7,1)));
            fakePost.RuleFor(p => p.Description, f => f.Lorem.Paragraphs(3));
            fakePost.RuleFor(p => p.Published, f => true);
            fakePost.RuleFor(p => p.Slug, f => f.Lorem.Slug());
            fakePost.RuleFor(p => p.Title, f => $"Bài {bv++}"+ f.Lorem.Sentence(3,4).Trim('.'));


            List<Post> posts = new List<Post>();
            List<PostCategory> post_categories=new List<PostCategory>();

            for(int i = 0; i < 40; i++)
            {
                var post = fakePost.Generate();
                post.DateUpdated = post.DateCreated;
                posts.Add(post);
                post_categories.Add(new PostCategory(){
                    Post = post,
                    Category = categories[rCateIndex.Next(5)]
                });
            }
            _dbContext.AddRange(posts);
            _dbContext.AddRange(post_categories);  
            //end post
            _dbContext.SaveChanges();
        }

    }
}
