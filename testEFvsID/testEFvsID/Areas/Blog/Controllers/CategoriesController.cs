using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using testEFvsID.Data;
using testEFvsID.Models;

namespace testEFvsID.Areas.Blog.Controllers
{
    [Area("Blog")]
    [Route("admin/blog/category/[action]/{id?}")]
    [Authorize(Roles = RoleName.Administrator)]
    public class CategoriesController : Controller
    {
        private readonly AppDBContext _context;

        public CategoriesController(AppDBContext context)
        {
            _context = context;
        }

        // GET: Blog/Categories
        public async Task<IActionResult> Index()
        {
            var qr=(from c in _context.Categories select c).Include(c=>c.ParentCategory).Include(c=>c.CategoryChildren);
            var categories = (await qr.ToListAsync()).Where(c=>c.ParentCategory==null).ToList();
            return View(categories);
        }

        // GET: Blog/Categories/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null || _context.Categories == null)
            {
                return NotFound();
            }

            var category = await _context.Categories
                .Include(c => c.ParentCategory)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (category == null)
            {
                return NotFound();
            }

            return View(category);
        }
        private void CreateSelectItem(List<Category> source,List<Category> des,int level)
        {
            string prefix = string.Concat(Enumerable.Repeat("----", level));
            foreach (var category in source)
            {
                //category.Title=prefix+" "+category.Title;
                des.Add(new Category()
                {
                    Id= category.Id,
                    Title= prefix + " " + category.Title
            });
                //des.Add(category);
                if (category.CategoryChildren?.Count > 0)
                {
                    CreateSelectItem(category.CategoryChildren.ToList(), des, level+1);
                }
            }
        }

        // GET: Blog/Categories/Create
        public async Task<IActionResult> CreateAsync()
        {
            var qr = (from c in _context.Categories select c).Include(c => c.ParentCategory).Include(c => c.CategoryChildren);
            var categories = (await qr.ToListAsync()).Where(c => c.ParentCategory == null).ToList();

            categories.Add(new Category()
            {
                Id = -1,
                Title = "Ko co danh muc cha"
            });
            var item = new List<Category>();
            CreateSelectItem(categories, item, 0);
            var selectList = new SelectList(item, "Id", "Title");


            ViewData["ParentCategoryId"] = selectList;

            return View();
        }

        // POST: Blog/Categories/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,Title,Description,slug,ParentCategoryId")] Category category)
        {
            if (!ModelState.IsValid)
            {
                if(category.ParentCategoryId==-1) category.ParentCategoryId = null;
                _context.Add(category);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            var qr = (from c in _context.Categories select c).Include(c => c.ParentCategory).Include(c => c.CategoryChildren);
            var categories = (await qr.ToListAsync()).Where(c => c.ParentCategory == null).ToList();

            categories.Add(new Category()
            {
                Id = -1,
                Title = "Ko co danh muc cha"
            });
            var item = new List<Category>();
            CreateSelectItem(categories, item, 0);
            var selectList = new SelectList(item, "Id", "Title");
            ViewData["ParentCategoryId"] = selectList;
            return View(category);
        }

        // GET: Blog/Categories/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null || _context.Categories == null)
            {
                return NotFound();
            }

            var category = await _context.Categories.FindAsync(id);
            if (category == null)
            {
                return NotFound();
            }
            var qr = (from c in _context.Categories select c).Include(c => c.ParentCategory).Include(c => c.CategoryChildren);
            var categories = (await qr.ToListAsync()).Where(c => c.ParentCategory == null).ToList();

            categories.Add(new Category()
            {
                Id = -1,
                Title = "Ko co danh muc cha"
            });
            var item = new List<Category>();
            CreateSelectItem(categories, item, 0);
            var selectList = new SelectList(item, "Id", "Title");
            ViewData["ParentCategoryId"] = selectList;
            return View(category);
        }

        // POST: Blog/Categories/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Title,Description,slug,ParentCategoryId")] Category category)
        {
            if (id != category.Id)
            {
                return NotFound();
            }
            bool canUpdate = true;
            if(category.ParentCategoryId == category.Id)
            {
                ModelState.AddModelError(string.Empty, "Phai chon danh muc cha khac");
                canUpdate = false;
            }
            //kiem tra danh muc cha phu hop
            if(canUpdate && category.ParentCategoryId != null)
            {
                var childCates = (from c in _context.Categories select c)
                    .Include(c => c.CategoryChildren)
                    .ToList()
                    .Where(c => c.ParentCategoryId == category.Id);
                Func<List<Category>, bool> checkCateIds = null;
                checkCateIds = (cates) =>
                {
                    foreach (var cate in cates)
                    {
                        if (cate.Id == category.ParentCategoryId)
                        {
                            canUpdate = false;
                            ModelState.AddModelError(string.Empty, "Phai chon danh muc cha khac");
                            return true;
                        }
                        if (cate.CategoryChildren != null) return checkCateIds(cate.CategoryChildren.ToList());
                    }
                    return false;
                };
                checkCateIds(childCates.ToList());
            }


            if (!ModelState.IsValid && canUpdate)
            {
                try
                {
                    if (category.ParentCategoryId == -1) category.ParentCategoryId = null;
                    _context.Update(category);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!CategoryExists(category.Id))
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
            var qr = (from c in _context.Categories select c).Include(c => c.ParentCategory).Include(c => c.CategoryChildren);
            var categories = (await qr.ToListAsync()).Where(c => c.ParentCategory == null).ToList();

            categories.Add(new Category()
            {
                Id = -1,
                Title = "Ko co danh muc cha"
            });
            var item = new List<Category>();
            CreateSelectItem(categories, item, 0);
            var selectList = new SelectList(item, "Id", "Title");
            ViewData["ParentCategoryId"] = selectList;
            return View(category);
        }

        // GET: Blog/Categories/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null || _context.Categories == null)
            {
                return NotFound();
            }

            var category = await _context.Categories
                .Include(c => c.ParentCategory)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (category == null)
            {
                return NotFound();
            }

            return View(category);
        }

        // POST: Blog/Categories/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            if (_context.Categories == null)
            {
                return Problem("Entity set 'AppDBContext.Categories'  is null.");
            }
            var category = await _context.Categories
                .Include(c=>c.CategoryChildren)
                .FirstOrDefaultAsync(c=>c.Id==id);


            if (category != null)
            {
                foreach(var cCategory in category.CategoryChildren)
                {
                    cCategory.ParentCategoryId = category.ParentCategoryId;

                }
                _context.Categories.Remove(category);
            }
            else return NotFound();
            
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool CategoryExists(int id)
        {
          return (_context.Categories?.Any(e => e.Id == id)).GetValueOrDefault();
        }
    }
}
