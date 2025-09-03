using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Diagnostics;
using testEFvsID.Models;
using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Hosting;

namespace testEFvsID.Controllers
{
    
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly AppDBContext _context;
        public HomeController(ILogger<HomeController> logger, AppDBContext context)
        {
            _logger = logger;
            _context = context;
        }
        [Route("/{categoryslug?}")]
        public IActionResult Index(string categoryslug, [FromQuery(Name = "p")] int currentPage, int pagesize)
        {
            var categories = GetCategory();
            ViewData["categories"] =categories;
            ViewBag.categoryslug=categoryslug;
           
            Category category = null;

            if(!string.IsNullOrEmpty(categoryslug))
            {
                category = _context.Categories.Where(c=>c.slug==categoryslug)
                    .Include(c=>c.CategoryChildren)
                    .FirstOrDefault();
                if(category == null)
                {
                    return NotFound("NOT Found");
                }
            }
            var posts = _context.Posts
                .Include(p => p.Author)
                .Include(p => p.PostCategories)
                .ThenInclude(p => p.Category)
                .AsQueryable();
            posts.OrderByDescending(p => p.DateUpdated);

            if(category != null)
            {
                var ids= new List<int>();
                category.ChildCategoryIDs(null, ids);
                ids.Add(category.Id);

                posts=posts.Where(p => p.PostCategories.Where(pc => ids.Contains(pc.CategoryID)).Any()).OrderByDescending(p => p.DateUpdated);
            }

            int totalPosts = posts.Count();
            if (pagesize <= 0) pagesize = 10;
            int countpages = (int)Math.Ceiling((double)totalPosts / pagesize);


            if (currentPage > countpages) currentPage = countpages;
            if (currentPage < 1) currentPage = 1;

            var pagingModel = new PagingModel()
            {
                countpages = countpages,
                currentpage = currentPage,
                generateUrl = (pageNumber) => Url.Action("Index", new
                {
                    p = pageNumber,
                    pagesize = pagesize
                })
            };
            var postsInPage = posts.OrderByDescending(p => p.DateUpdated)
                        .Skip((currentPage - 1) * pagesize)
                        .Take(pagesize);
                        
                       
            ViewBag.pagingModel = pagingModel;
            ViewBag.totalPosts = totalPosts;

            var newslider= _context.Posts.Where(p=>p.Slider==true)
                .Include(p => p.PostCategories)
                .ThenInclude(pc => pc.Category)
                .Skip(1).ToList();
            var firstslider=_context.Posts.Where(p=>p.Slider==true)
                .Include(p => p.PostCategories)
                .ThenInclude(pc => pc.Category)
                .FirstOrDefault();
            var newshot = _context.Posts.Where(p => p.HotNew == true)
                .Include(p => p.Author)
                .ToList();
            var newview= _context.Posts
                .Include(p => p.Author)
                .OrderByDescending(p => p.View).Take(10).ToList();
            var newflash = _context.Posts.Where(p => p.FlashNew == true)
                .Include(p => p.Author)
                .ToList();
            var random = new Random();
            var number=_context.Posts.Count();
            int randNB = random.Next(1, number-15);
            var newcustom = _context.Posts.Skip(randNB).Take(15).ToList();
            ViewBag.newview = newview; 
            ViewBag.firstslider= firstslider;
            ViewBag.newslider = newslider;
            ViewBag.newshot=newshot;
            ViewBag.category = category;
            ViewBag.newflash = newflash;
            ViewBag.newcustom = newcustom;
            return View(postsInPage.ToList());
        }
        [Route("/{postslug}.html")]
        public IActionResult Detail(string postslug)
        {
            var categories = GetCategory();
            ViewBag.categories = categories;
            var newview = _context.Posts
                .Include(p => p.Author)
                .OrderByDescending(p => p.View).Take(10).ToList();
            ViewBag.newview = newview;
            var post = _context.Posts.Where(p => p.Slug == postslug)
                .Include(p => p.Author)
                .Include(p => p.PostCategories)
                .ThenInclude(pc => pc.Category)
                .FirstOrDefault();
            var posts = _context.Posts
                .Include(p => p.Author)
                .Include(p => p.PostCategories)
                .ThenInclude(p => p.Category)
                .AsQueryable();
            posts.OrderByDescending(p => p.DateUpdated);
            int totalPosts = posts.Count();
            var random = new Random();
            int randNB = random.Next(1, totalPosts - 15);
            var newcustom = _context.Posts.Skip(randNB).Take(15).ToList();
            ViewBag.newcustom = newcustom;
            if (post == null)
            {
                return NotFound("Không thấy bài viết");
            }
            post.View++;
            _context.SaveChanges();
            
            Category category=post.PostCategories.FirstOrDefault()?.Category;
            ViewBag.category = category;
            return View(post);
        }
        private List<Category> GetCategory()
        {
            var categories = _context.Categories
                .Include(c => c.CategoryChildren)
                .AsEnumerable()
                .Where(c => c.ParentCategory == null)
                .ToList();
            return categories;
        }
        [HttpGet]        
        public async Task<IActionResult> Search(string search)
        {
            var post= await _context.Posts.Where(x=>x.Title.Contains(search) || x.Description.Contains(search)
                || x.Content.Contains(search)
            ).Take(10).ToListAsync();
            if (post.Count()>0)
            {
                return Json(new {data=post,result=true});
            }else            
                return Json(new { mess = "Không tìm thấy bài viết" ,result=false});
            
        }
        
        public IActionResult SearchResult(string search)
        {
            var categories = GetCategory();
            ViewBag.categories = categories;
            var random = new Random();
            var number = _context.Posts.Count();
            int randNB = random.Next(1, number - 3);
            var newcustom = _context.Posts.Skip(randNB).Take(3).ToList();
            var newview = _context.Posts
               .Include(p => p.Author)
               .OrderByDescending(p => p.View).Take(10).ToList();
            ViewBag.newview = newview;
            var post = _context.Posts.Where(x => x.Title.Contains(search) || x.Description.Contains(search)
                || x.Content.Contains(search))
                .Include(x => x.Author)
                .Include(x => x.PostCategories)
                .ThenInclude(x => x.Category)
                .ToList();
            ViewBag.Total=post.Count();
            ViewBag.key = search;
            ViewBag.newcustom = newcustom;
            return View(post);
        }
        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}