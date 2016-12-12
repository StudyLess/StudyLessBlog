using SoftUniBlog.Models;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using PagedList;

namespace SoftUniBlog.Controllers
{
    public class TagController : Controller
    {
        // GET: Tag
        public ActionResult List(int? id, int? page)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            using (var database = new BlogDbContext())
            {
                // Get articles from database
                var articles = database.Tags
                    .Include(t => t.Articles.Select(a => a.Tags))
                    .Include(t => t.Articles.Select(a => a.Author))
                    .FirstOrDefault(t => t.Id == id)
                    .Articles;

                int pageSize = 3;
                int pageNumber = (page ?? 1);

                // Return the view
                return View(articles.ToPagedList(pageNumber, pageSize));
            }
        }
    }
}