using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using QA.Models;

namespace QA.Areas.Admin.Controllers
{
    public class PostController : Controller
    {
        // GET: Admin/Post
        QuestionAndAnswerEntities db = new QuestionAndAnswerEntities();
        public ActionResult Index()
        {
            var qs = from l in db.Posts// lấy toàn bộ liên kết
                     select l;
            return View(qs);
        }
        public ActionResult AddMain()
        {
            return View();
        }
        [HttpPost]
        [ValidateInput(false)]
        public ActionResult AddMain(FormCollection f, HttpPostedFileBase fFileUpload)
        {
            Post ps = new Post();
            ps.Topic = f["title"];
            ps.Description = f["contentDetails"];
            ps.Postmain = 1;
            var sFileName = Path.GetFileName(fFileUpload.FileName);
            var path = Path.Combine(Server.MapPath("~/Content/Images"), sFileName);
            if (!System.IO.File.Exists(path))
            {
                fFileUpload.SaveAs(path);
            }
            ps.Images = sFileName;
            db.Posts.Add(ps);
            db.SaveChanges();
            return this.Index();
        }
        public ActionResult Add()
        {
            return View();
        }
        [HttpPost]
        [ValidateInput(false)]
        public ActionResult Add(FormCollection f, HttpPostedFileBase fFileUpload)
        {
            Post ps = new Post();
            ps.Topic = f["title"];
            ps.Description = f["contentDetails"];
            ps.Postmain = 0;
            var sFileName = Path.GetFileName(fFileUpload.FileName);
            var path = Path.Combine(Server.MapPath("~/Content/Images"), sFileName);
            if (!System.IO.File.Exists(path))
            {
                fFileUpload.SaveAs(path);
            }
            ps.Images = sFileName;
            db.Posts.Add(ps);
            db.SaveChanges();
            return this.Index();
        }
        public ActionResult Detail(int id)
        {
            Post vn = db.Posts.SingleOrDefault(m => m.IDPost == id);
            return View(vn);
        }
        [HttpPost]
        [ValidateInput(false)]
        public ActionResult Detail(FormCollection f, HttpPostedFileBase fFileUpload)
        {
            int id = Convert.ToInt32(f["id"]);

            Post ps = db.Posts.SingleOrDefault(n => n.IDPost == id);
            ps.Topic = f["title"];
            ps.Description = f["contentDetails"];
            if (ModelState.IsValid)
            {
                if (fFileUpload != null)
                {
                    var sFileName = Path.GetFileName(fFileUpload.FileName);
                    var path = Path.Combine(Server.MapPath("~/Content/Images"), sFileName);
                    if (!System.IO.File.Exists(path))
                    {
                        fFileUpload.SaveAs(path);
                    }
                    ps.Images = sFileName;
                }
                db.SaveChanges();
            }
            db.SaveChanges();
            return this.Index();
        }
        public ActionResult Delete(int id)
        {
            Post ps = db.Posts.SingleOrDefault(n => n.IDPost == id);
            db.Posts.Remove(ps);
            db.SaveChanges();
            return Index();
        }
    }
}