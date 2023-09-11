using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using QA.Models;
using PagedList;

namespace QA.Controllers
{
    public class HomeController : Controller
    {
        QuestionAndAnswerEntities db = new QuestionAndAnswerEntities();
        public ActionResult Index(int? id, int? page)
        {
            string message = TempData["message"] as string;
            if (!string.IsNullOrEmpty(message))
            {
                ViewBag.ThongBao = message;
            }
            if (page == null)
                page = 1;
            ViewBag.Page = (id ?? 1);
            ViewBag.pageNumber = (page ?? 1);
            return View();
        }

        public ActionResult About()
        {
            ViewBag.Message = "Your application description page.";

            return View();
        }

        public ActionResult Contact()
        {
            ViewBag.Message = "Your contact page.";

            return View();
        }
        public ActionResult Detail(int id)
        {
            Question qs = db.Questions.SingleOrDefault(m => m.IDQuestion == id);
            qs.View = qs.View + 1;
            db.SaveChanges();
            var dt = from s in db.Questions
                     where s.IDQuestion == id
                     select s;
            string message = TempData["message"] as string;
            if (!string.IsNullOrEmpty(message))
            {
                ViewBag.ThongBao = message;
            }
            return View(dt.Single());
        }
        public JsonResult AddQuestion(string tt, string ct)
        {
            try
            {
                Question qt = new Question();
                qt.Topic = tt;
                qt.Content = ct;
                qt.IDCategory = 1;
                qt.Incognito =true;
                qt.IDAccount = 1;
                qt.Verify = true;
                db.Questions.Add(qt);
                db.SaveChanges();
                return Json(new { code = 200 }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new { code = 500 }, JsonRequestBehavior.AllowGet);
            }
        }
        [HttpPost]
        [ValidateInput(false)]
        public ActionResult AddQ(FormCollection f)
        {
            ViewBag.DM = new SelectList(db.Categories.ToList().OrderBy(n => n.Name), "IDCategory", "Name");
            if (Session["TaiKhoan"] == null)
            {
                return RedirectToAction("Login", "User");
            }
            Account ac = (Account)Session["TaiKhoan"];
            if (ac.Block == 1)
            {
                TempData["message"] = "Tài khoản của bạn đã bị khóa không thể thực hiện chức năng này";
                return RedirectToAction("Index");
            }
            else
            {
                Question qt = new Question();
                qt.Topic = f["title"];
                qt.Content = f["contentDetails"];
                qt.IDCategory = int.Parse(f["DM"]);
                qt.Datetime = DateTime.Now;
                qt.View = 0;
                if (Convert.ToInt32(f["incognito"]) == 1)
                {
                    qt.Incognito = true;
                }
                else
                {
                    qt.Incognito = false;
                }
                qt.IDAccount = ac.IDAccount;
                qt.Verify = false;
                db.Questions.Add(qt);
                db.SaveChanges();
                return RedirectToAction("Index");
            }
        }
        public ActionResult category()
        {
            var ct = from s in db.Categories
                     select s;
            return PartialView(ct);
        }
        public ActionResult loginlogout()
        {
            return PartialView();
        }
        public JsonResult GetSearchValue(string search)
        {
            QuestionAndAnswerEntities db = new QuestionAndAnswerEntities();
            search = search.ToLower();
            List<QuestionF> allsearch = db.Questions.Where(x => x.Topic.ToLower().Contains(search)).Select(x => new QuestionF
            {
                IDQuestion  = x.IDQuestion,
                Topic = x.Topic
            }).ToList();
            return new JsonResult { Data = allsearch, JsonRequestBehavior = JsonRequestBehavior.AllowGet };
        }
        public ActionResult ShowAnswer(int idqs, int idas)
        {
            Answer ans = db.Answers.SingleOrDefault(m => m.IDAnswer == idas);
            ans.Show = true;
            db.SaveChanges();

            List<Answer> nc = (from s in db.Answers
                               where s.IDQuestion == idqs
                               select s).ToList();
            foreach (Answer sm in nc)
            {
                sm.Show = true;
                db.SaveChanges();
            }
            return RedirectToAction("Detail", "Home", new { id = idqs });
        }
        public ActionResult Search(FormCollection f)
        {
            string searchString = f["search"];
            Question sr = db.Questions.SingleOrDefault(m => m.Topic == searchString);
            if(sr!=null)
            {
                return RedirectToAction("Detail", new { id = sr.IDQuestion });
            }
            else
            {
                var qs = from l in db.Questions // lấy toàn bộ liên kết
                         select l;
                if (!String.IsNullOrEmpty(searchString))
                {
                    searchString = searchString.ToLower();
                    qs = qs.Where(b => b.Content.ToLower().Contains(searchString));
                }
                return View(qs.ToList());
            }
           
        }
        public ActionResult DeatilPost(int id)
        {
            var qs = from l in db.Posts
                     where l.IDPost ==id// lấy toàn bộ liên kết
                     select l;
            return View(qs);
        }
        public ActionResult Left()
        {
            var qs = from l in db.Posts
                     where l.Postmain==1// lấy toàn bộ liên kết
                     select l;
            return PartialView(qs);
        }
        public ActionResult Right()
        {
            var qs = from l in db.Posts
                     where l.Postmain == 0// lấy toàn bộ liên kết
                     select l;
            return PartialView(qs);
        }
    }
}