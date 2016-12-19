// View->Other Windows->Package Manager Console->write this: Install-Package BootstrapNotifications
using SoftUniBlog.Extensions; 
using SoftUniBlog.Models;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;

namespace SoftUniBlog.Controllers.Admin
{
    public class CategoryController : Controller
    {
        // GET: Category
        public ActionResult Index()
        {
            return RedirectToAction("List");
        }

        //
        // GET: Category/List
        public ActionResult List()
        {
            using (var database = new BlogDbContext())
            {
                var categories = database.Categories
                    .ToList();

                return View(categories);
            }
        }

        //
        // GET: Category/Create
        public ActionResult Create()
        {
            return View();
        }

        //
        // POST: Category/Create
        [HttpPost]
        public ActionResult Create(Category category)
        {
            if (ModelState.IsValid)
            {
                using (var database = new BlogDbContext())
                {
                    database.Categories.Add(category);
                    database.SaveChanges();
                    //will show message when category is created
                    this.AddNotification("Category created.", NotificationType.SUCCESS);

                    return RedirectToAction("Index");
                }
            }

            return View(category);
        }

        //
        // GET: Category/Edit
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            using (var database = new BlogDbContext())
            {
                var category = database.Categories
                    .FirstOrDefault(c => c.Id == id);

                if (category == null)
                {
                    return HttpNotFound();
                }

                return View(category);
            }
        }

        //
        // POST: Category/Edit
        [HttpPost]
        public ActionResult Edit(Category category)
        {
            if (ModelState.IsValid)
            {
                using (var database = new BlogDbContext())
                {
                    database.Entry(category).State = EntityState.Modified;
                    database.SaveChanges();
                    //will show message when category is edited
                    this.AddNotification("Category edited.", NotificationType.INFO);

                    return RedirectToAction("Index");
                }
            }

            return View(category);
        }

        //
        // GET: Category/Delete
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            using (var database = new BlogDbContext())
            {
                var category = database.Categories
                    .FirstOrDefault(c => c.Id == id);

                if (category == null)
                {
                    return HttpNotFound();
                }

                return View(category);
            }
        }

        //
        // POST: Category/Delete
        [HttpPost]
        [ActionName("Delete")]
        public ActionResult DeleteConfirmed(int? id)
        {
            using (var database = new BlogDbContext())
            {
                var category = database.Categories
                    .FirstOrDefault(c => c.Id == id);

                var categoryArticles = category.Articles
                    .ToList();

                foreach (var article in categoryArticles)
                {
                    database.Articles.Remove(article);
                }

                database.Categories.Remove(category);
                database.SaveChanges();
                //will show message when category is deleted
                this.AddNotification("Category deleted.", NotificationType.WARNING);

                return RedirectToAction("Index");
            }
        }
    }
}