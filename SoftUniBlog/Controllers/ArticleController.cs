using SoftUniBlog.Models;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using PagedList;    // first must be installed PagedList.MVC NuGet Package - Tools->NuGet Package Manager->Package Manager Console->write this: Install-Package PagedList.Mvc
using SoftUniBlog.Extensions; // View->Other Windows->Package Manager Console->write this: Install-Package BootstrapNotifications

namespace SoftUniBlog.Controllers
{
    public class ArticleController : Controller
    {
        private object database;

        //
        // GET: Article
        public ActionResult Index()
        {
            return RedirectToAction("List");
        }

        //
        // GET: Article/List
        public ActionResult List(string sortOrder, string currentFilter, string searchString, int? page) //add new parameters
        {
            using (var database = new BlogDbContext())
            {
                ViewBag.CurrentSort = sortOrder;

                // if we change the searchString, the page has to be reset to 1
                if (searchString != null)
                {
                    page = 1;
                }
                else
                {
                    searchString = currentFilter;
                }

                // provides the view with the current filter string
                ViewBag.CurrentFilter = searchString;

                // Get articles from database
                var articles = database.Articles
                    .Include(a => a.Author)
                    .Include(a => a.Tags); // deleted .ToList()

                //checking if the title or the content of the article contains the searchString
                if (!string.IsNullOrEmpty(searchString)) 
                {
                    articles = articles.Where(s => s.Title.Contains(searchString) || s.Content.Contains(searchString));
                }

                switch (sortOrder)
                {
                    case "title_desc":
                        articles = articles.OrderByDescending(s => s.Title);
                        break;
                    case "Author":
                        articles = articles.OrderBy(s => s.Author);
                        break;
                    case "author_desc":
                        articles = articles.OrderByDescending(s => s.Author);
                        break;
                    default:
                        articles = articles.OrderBy(s => s.Title);
                        break;
                }

                int pageSize = 3;
                int pageNumber = (page ?? 1);

                // converts the articles in special collection type
                return View(articles.ToPagedList(pageNumber, pageSize));
            }    
        }

        //
        // GET: Article/Details
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }


            using (var database = new BlogDbContext())
            {
                // Get the article from database
                var article = database.Articles
                    .Where(a => a.Id == id)
                    .Include(a => a.Author)
                    .Include(a => a.Tags)
                    .First();

            if (article == null)
                {
                    return HttpNotFound();
                }

                return View(article);
            }
        }

        //
        // GET: Article/Create
        [Authorize]
        public ActionResult Create()
        {
            using (var database = new BlogDbContext())
            {
                var model = new ArticleViewModel();
                model.Categories = database.Categories
                    .OrderBy(c => c.Name)
                    .ToList();

                return View(model);
            }
        }

        //
        // POST: Article/Create
        [HttpPost]
        [Authorize]
        public ActionResult Create(ArticleViewModel model)
        {
            if (ModelState.IsValid)
            {
                using (var database = new BlogDbContext())
                {
                    // Get author id
                    var authorId = database.Users
                        .Where(u => u.UserName == this.User.Identity.Name)
                        .First()
                        .Id;

                    // Set articles author
                    var article = new Article(authorId, model.Title, model.Content, model.CategoryId);

                    this.SetArticleTags(article, model, database);

                    // Save article in DB
                    database.Articles.Add(article);
                    database.SaveChanges();
                    //will show message when article is created
                    this.AddNotification("Article created.", NotificationType.SUCCESS); 

                    return RedirectToAction("Index");
                }
            }

            return View(model);
        }

        //
        // GET: Article/Delete
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            using (var database = new BlogDbContext())
            {
                // Get article from database
                var article = database.Articles
                    .Where(a => a.Id == id)
                    .Include(a => a.Author)
                    .Include(a => a.Category)
                    .First();

                if (! IsUserAuthorizedToEdit(article))
                {
                    return new HttpStatusCodeResult(HttpStatusCode.Forbidden);
                }

                ViewBag.TagsString = string.Join(", ", article.Tags.Select(t => t.Name));

                // Chech if article exists
                if (article == null)
                {
                    return HttpNotFound();
                }

                // Pass article to view
                return View(article);
            }
        }

        //
        // POST: Article/Delete
        [HttpPost]
        [ActionName("Delete")]
        public ActionResult DeleteConfirmed(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            using (var database = new BlogDbContext())
            {
                // Get article from database
                var article = database.Articles
                    .Where(a => a.Id == id)
                    .Include(a => a.Author)
                    .First();

                // Check if article exists
                if (article == null)
                {
                    return HttpNotFound();
                }

                // Delete article from database
                database.Articles.Remove(article);
                database.SaveChanges();
                //will show message when article is deleted
                this.AddNotification("Article deleted.", NotificationType.WARNING);

                // Redirect to index page
                return RedirectToAction("Index");
            }
        }

        //
        // GET: Article/Edit
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            using (var database = new BlogDbContext())
            {
                // Get article from the database
                var article = database.Articles
                    .Where(a => a.Id == id)
                    .First();

                if (!IsUserAuthorizedToEdit(article))
                {
                    return new HttpStatusCodeResult(HttpStatusCode.Forbidden);
                }

                // Check if article exists
                if (article == null)
                {
                    return HttpNotFound();
                }

                // Create the view model
                var model = new ArticleViewModel();
                model.Id = article.Id;
                model.Title = article.Title;
                model.Content = article.Content;
                model.CategoryId = article.CategoryId;
                model.Categories = database.Categories
                    .OrderBy(c => c.Name)
                    .ToList();
                model.Tags = string.Join(", ", article.Tags.Select(t => t.Name));

                // Pass the view model to view
                return View(model);
            }
        }

        //
        // POST: Article/Edit
        [HttpPost]
        public ActionResult Edit(ArticleViewModel model)
        {
            // Check if model state is valid
            if (ModelState.IsValid)
            {
                using (var database = new BlogDbContext())
                {
                    // Get article from database
                    var article = database.Articles
                        .FirstOrDefault(a => a.Id == model.Id);

                    // Set article properties
                    article.Title = model.Title;
                    article.Content = model.Content;
                    article.CategoryId = model.CategoryId;
                    this.SetArticleTags(article, model, database);

                    // Save article state in database
                    database.Entry(article).State = EntityState.Modified;
                    database.SaveChanges();
                    //will show message when article is edited
                    this.AddNotification("Article edited.", NotificationType.INFO);

                    // Redirect to the index page
                    return RedirectToAction("Index");
                }
            }

            // If model state is invalid, return the same view
            return View(model);
        }

        private bool IsUserAuthorizedToEdit(Article article)
        {
            bool isAdmin = this.User.IsInRole("Admin");
            bool isAuthor = article.IsAuthor(this.User.Identity.Name);

            return isAdmin || isAuthor;
        }

        private void SetArticleTags(Article article, ArticleViewModel model, BlogDbContext db)
        {
            // Split tags
            var tagsStrings = model.Tags
                .Split(new char[] { ',', ' ' }, StringSplitOptions.RemoveEmptyEntries)
                .Select(t => t.ToLower())
                .Distinct();

            // Clear current article tags
            article.Tags.Clear();

            // Set new article tags
            foreach (var tagString in tagsStrings)
            {
                // Get tag from db by its name
                Tag tag = db.Tags.FirstOrDefault(t => t.Name.Equals(tagString));

                // If the tag is null, create new tag
                if (tag == null)
                {
                    tag = new Tag() { Name = tagString };
                    db.Tags.Add(tag);
                }

                // Add tag to article tags
                article.Tags.Add(tag);
            }
        }
    }
}