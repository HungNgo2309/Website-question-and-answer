using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using QA.Models;

namespace QA.Areas.Admin.Controllers
{
    public class ManagerController : Controller
    {
        // GET: Admin/Manager
        QuestionAndAnswerEntities db = new QuestionAndAnswerEntities();
        public ActionResult Index()
        {
            if(Session["Admin"]== null)
            {
                return RedirectToAction("Login", "UserAD");
            }
            return View();
        }
        public ActionResult Confirm()
        {
            var ac = from s in db.Questions
                     orderby s.IDQuestion descending
                  select s;
            return View(ac);
        }
        public ActionResult Verify(int id)
        {
                Question qs = db.Questions.SingleOrDefault(n => n.IDQuestion == id);
                if(qs.Verify==true)
                {
                    qs.Verify = false;
                }
                else
                {
                    qs.Verify = true;
                }
                db.SaveChanges();
            return RedirectToAction("Confirm");
        }
        public ActionResult Detail(int id)
        {
            Question qs = db.Questions.SingleOrDefault(m => m.IDQuestion == id);
            ViewBag.DM = new SelectList(db.Categories.ToList().OrderBy(n => n.Name), "IDCategory", "Name", qs.IDCategory);
            return View(qs);
        }
        [HttpPost]
        [ValidateInput(false)]
        public ActionResult Detail(FormCollection f, int id)
        {
            Question qt = db.Questions.SingleOrDefault(n => n.IDQuestion == id);
            qt.Topic = f["title"];
            qt.Content = f["contentDetails"];
            qt.IDCategory = int.Parse(f["DM"]);
            if (Convert.ToInt32(f["incognito"]) == 1)
            {
                qt.Incognito = true;
            }
            else
            {
                qt.Incognito = false;
            }
            db.SaveChanges();
            return RedirectToAction("Confirm");
        }
        [HttpPost]
        public JsonResult Xoa(int id)
        {
            try
            {
                Question qs = db.Questions.SingleOrDefault(n => n.IDQuestion == id);
                if (qs != null)
                {
                    db.Questions.Remove(qs);
                    db.SaveChanges();
                    return Json(new { code = 200, msg = "Xóa thành công" }, JsonRequestBehavior.AllowGet);
                }
                return Json(new { code = 500, msg = "Không tìm thấy câu hỏi muốn xóa" }, JsonRequestBehavior.AllowGet);
            }
            catch
            {
                return Json(new { code = 500, msg = "Xóa không thành công" }, JsonRequestBehavior.AllowGet);

            }
        }
        public ActionResult Rank()
        {
            var n = from s in db.Accounts
                    orderby s.Point descending
                  select s;
            return View(n);
        }
        public ActionResult Delete(int id)
        {
            Question an = db.Questions.SingleOrDefault(n => n.IDQuestion == id);
            db.Questions.Remove(an);
            db.SaveChanges();
            return RedirectToAction("Index");
        }
        public ActionResult DeleteCT(int idct)
        {
            Category an = db.Categories.SingleOrDefault(n => n.IDCategory == idct);
            db.Categories.Remove(an);
            db.SaveChanges();
            return RedirectToAction("Category");
        }
        public ActionResult Category()
        {
            var n = from s in db.Categories
                    select s;
            return View(n);
        }
        [HttpPost]
        public ActionResult AddCT(FormCollection f)
        {
            Category ct = new Category();
            ct.Name = f["danhmuc"];
            db.Categories.Add(ct);
            db.SaveChanges();
            return RedirectToAction("Category");
        }
        [HttpPost]
        public ActionResult EditCt(FormCollection f,int id)
        {
            Category ct = db.Categories.SingleOrDefault(n => n.IDCategory == id);
            ct.Name = f["danhmuc"];
            db.SaveChanges();
            return View("Category");
        }
        public int SumQuestion()
        {
            List<Question> lstpost = (from s in db.Questions
                                      select s).ToList();
            return lstpost.Count;
        }
        public int SumAccount()
        {
            List<Account> lstpost = (from s in db.Accounts
                                  select s).ToList();
            return lstpost.Count;
        }
        public int Activity()
        {
            List<Account> lstpost = (from s in db.Accounts
                                     where s.Point>0
                                  select s).ToList();
            return lstpost.Count;
        }
        public int SumAnswer()
        {
            List<Answer> lstpost = (from s in db.Answers
                                     select s).ToList();
            return lstpost.Count;
        }
    }
}