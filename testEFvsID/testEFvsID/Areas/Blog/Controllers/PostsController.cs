using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using App.Utilities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Hosting;
using testEFvsID.Areas.Blog.Models;
using testEFvsID.Areas.Identity.Models.UserViewModels;
using testEFvsID.Data;
using testEFvsID.Models;

namespace testEFvsID.Areas.Blog.Controllers
{
    [Area("Blog")]
    [Authorize(Roles = RoleName.Administrator)]
    public class PostsController : Controller
    {
        private readonly AppDBContext _context;
        private readonly IWebHostEnvironment _webhost;
        private readonly UserManager<AppUser> _userManager;
        public PostsController(AppDBContext context, IWebHostEnvironment webhost,UserManager<AppUser> userManager)
        {
            _context = context;
            _webhost = webhost;
            _userManager = userManager;
        }

        // GET: Blog/Posts
        public async Task<IActionResult> Index([FromQuery(Name ="p")]int currentPage,int pagesize)
        {
            var post = _context.Posts
                .Include(p => p.Author)
                .OrderByDescending(p => p.DateCreated);
            int totalPosts=post.Count();
            if (pagesize <= 0) pagesize = 10;
            int countpages = (int)Math.Ceiling((double)totalPosts / pagesize);

            
            if(currentPage>countpages) currentPage = countpages;
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
            ViewBag.pagingModel=pagingModel;
            ViewBag.totalPosts=totalPosts;
            ViewBag.postIndex = (currentPage - 1) * pagesize;
            //model.totalUsers = await qr.CountAsync();
            //model.countPages = (int)Math.Ceiling((double)model.totalUsers / model.ITEMS_PER_PAGE);

            //if (model.currentPage < 1)
            //    model.currentPage = 1;
            //if (model.currentPage > model.countPages)
            //    model.currentPage = model.countPages;

            var  postsInPage = await post.Skip((currentPage - 1) * pagesize)
                        .Take(pagesize)
                        .Include(p => p.PostCategories)
                        .ThenInclude(pc => pc.Category)
                        .ToListAsync();
            var categories = await _context.Categories.ToListAsync();
            ViewData["categories"] = new MultiSelectList(categories, "Id", "Title");
            ViewData["AuthorId"] = new SelectList(_context.Users, "Id", "UserName");
            return View(postsInPage);
        }

        // GET: Blog/Posts/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null || _context.Posts == null)
            {
                return NotFound();
            }

            var post = await _context.Posts
                .Include(p => p.Author)
                .FirstOrDefaultAsync(m => m.PostId == id);
            if (post == null)
            {
                return NotFound();
            }

            return View(post);
        }

        // GET: Blog/Posts/Create
        public async Task<IActionResult> CreateAsync()
        {
            var categories= await _context.Categories.ToListAsync();
            ViewData["categories"] = new MultiSelectList(categories, "Id", "Title");
            ViewData["AuthorId"] = new SelectList(_context.Users, "Id", "UserName");
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("PostId,Title,Description,Slug,Content,Published,HotNew,FlashNew,Slider,AuthorId,CategoryIDs")] CreatePostModel post,IFormFile file)
        {
            string folder = "/uploads/";
            string uploadsPath = Path.Combine(_webhost.WebRootPath, "uploads");
            string uniqueFileName = Guid.NewGuid().ToString() + "_" + file.FileName;
            string filePath = Path.Combine(uploadsPath, uniqueFileName);
            var categories = await _context.Categories.ToListAsync();
            ViewData["categories"] = new MultiSelectList(categories, "Id", "Title");
            if (await _context.Posts.AnyAsync(p => p.Slug == post.Slug))
            {
                ModelState.AddModelError("Slug", "Nhập chuỗi url khác");
                return View(post);
            }
            if (post.Slug == null)
            {
                post.Slug = AppUtilities.GenerateSlug(post.Title);
            }
            if (!ModelState.IsValid)
            {
                var user= await _userManager.GetUserAsync(User);
                using (FileStream stream = new FileStream(filePath, FileMode.Create))
                {
                    await file.CopyToAsync(stream);
                }

                post.Thumbnail = folder + uniqueFileName;
                post.DateCreated = DateTime.Now;
                post.DateUpdated = DateTime.Now;

                //post.AuthorId = user.Id; lấy tên tài khoản đăng bài 
                _context.Add(post);
                if (post.CategoryIDs != null)
                {
                    foreach(var cate in post.CategoryIDs)
                    {
                        _context.Add(new PostCategory()
                        {
                            CategoryID = cate,
                            Post=post
                        });
                    }
                }

                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }

            ViewData["AuthorId"] = new SelectList(_context.Users, "Id", "Id", post.AuthorId);

            return View(post);
        }

        public IActionResult _CreateAjax(CreatePostModel post)
        {
            var categories = _context.Categories.ToList();
            ViewData["categories"] = new MultiSelectList(categories, "Id", "Title");
            ViewData["AuthorId"] = new SelectList(_context.Users, "Id", "Id", post.AuthorId);
            return PartialView("_CreateAjax");
        }
        [HttpPost]
        public IActionResult TestAjax(CreatePostModel post)
        {

            try
            {
                return Json(new {data = post});
            }
            catch
            {
                return Json(new { });
            }
            
        }
       
        // GET: Blog/Posts/Edit/5
        //public async Task<IActionResult> Edit(int? id)
        //{
        //    if (id == null || _context.Posts == null)
        //    {
        //        return NotFound();
        //    }

        //    //var post = await _context.Posts.FindAsync(id);
        //    var post= await _context.Posts.Include(p=>p.PostCategories).FirstOrDefaultAsync(p=>p.PostId==id);
        //    if (post == null)
        //    {
        //        return NotFound();
        //    }

        //    var postEdit = new CreatePostModel()
        //    {
        //        PostId=post.PostId,
        //        Title=post.Title,
        //        Content= post.Content,
        //        Description=post.Description,
        //        Slug=post.Slug,
        //        Published=post.Published,
        //        HotNew=post.HotNew,
        //        Slider=post.Slider,
        //        FlashNew=post.FlashNew,
        //        CategoryIDs=post.PostCategories.Select(pc=>pc.CategoryID).ToArray()
        //    };
        //    var categories = await _context.Categories.ToListAsync();
        //    ViewData["categories"] = new MultiSelectList(categories, "Id", "Title");
        //    ViewData["AuthorId"] = new SelectList(_context.Users, "Id", "UserName", post.AuthorId);
        //    return PartialView(postEdit);
        //}

        // POST: Blog/Posts/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("PostId,Title,Description,Slug,Content,Published,HotNew,FlashNew,Slider,AuthorId,DateCreated,CategoryIDs")] CreatePostModel post, IFormFile file)
        {
            if (id != post.PostId)
            {
                return NotFound();
            }
            var categories = await _context.Categories.ToListAsync();
            ViewData["categories"] = new MultiSelectList(categories, "Id", "Title");
            if (post.Slug == null)
            {
                post.Slug = AppUtilities.GenerateSlug(post.Title);
            }
            if (await _context.Posts.AnyAsync(p => p.Slug == post.Slug && p.PostId!=id))
            {
                ModelState.AddModelError("Slug", "Nhập chuỗi url khác");
                return View(post);
            }
            if (!ModelState.IsValid)
            {
                try
                {
                    var postUpdate = await _context.Posts.Include(p => p.PostCategories).FirstOrDefaultAsync(p => p.PostId == id);
                    if (postUpdate == null)
                    {
                        return NotFound();
                    }
                    string folder = "/uploads/";
                    string uploadsPath = Path.Combine(_webhost.WebRootPath, "uploads");
                    string uniqueFileName = Guid.NewGuid().ToString() + "_" + file.FileName;
                    string filePath = Path.Combine(uploadsPath, uniqueFileName);
                    using (FileStream stream = new FileStream(filePath, FileMode.Create))
                    {
                        await file.CopyToAsync(stream);
                    }
                    post.Thumbnail = folder+uniqueFileName;
                    postUpdate.Title=post.Title;
                    postUpdate.Thumbnail=post.Thumbnail;
                    postUpdate.Description=post.Description;
                    postUpdate.Content=post.Content;
                    postUpdate.Published=post.Published;
                    postUpdate.HotNew=post.HotNew;
                    postUpdate.FlashNew=post.FlashNew;
                    postUpdate.Slider=post.Slider;
                    postUpdate.DateUpdated=DateTime.Now;
                    if (post.CategoryIDs == null) post.CategoryIDs = new int[] { };
                    var oldCate=postUpdate.PostCategories.Select(c => c.CategoryID).ToArray();
                    var newCate = post.CategoryIDs;
                    var removeCate= from postCate in postUpdate.PostCategories 
                                    where (!newCate.Contains(postCate.CategoryID))
                                    select postCate;
                    _context.PostCategories.RemoveRange(removeCate);
                    var addCate = from CateId in newCate
                                  where !oldCate.Contains(CateId)
                                  select CateId;
                    foreach(var cate in addCate)
                    {
                        _context.PostCategories.Add(new PostCategory()
                        {
                            PostID=id,
                            CategoryID=cate
                        });
                    }
                    _context.Update(postUpdate);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!PostExists(post.PostId))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }
            ViewData["AuthorId"] = new SelectList(_context.Users, "Id", "UserName", post.AuthorId);
            return View(post);
        }

        // GET: Blog/Posts/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null || _context.Posts == null)
            {
                return NotFound();
            }

            var post = await _context.Posts
                .Include(p => p.Author)
                .FirstOrDefaultAsync(m => m.PostId == id);
            if (post == null)
            {
                return NotFound();
            }

            return View(post);
        }

        // POST: Blog/Posts/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            if (_context.Posts == null)
            {
                return Problem("Entity set 'AppDBContext.Posts'  is null.");
            }
            var post = await _context.Posts.FindAsync(id);
            if (post != null)
            {
                _context.Posts.Remove(post);
            }
            
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool PostExists(int id)
        {
          return (_context.Posts?.Any(e => e.PostId == id)).GetValueOrDefault();
        }
    }
}
