using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using QA.Models;

namespace QA.Controllers
{
    public class HistoryController : Controller
    {
        // GET: History
        QuestionAndAnswerEntities db = new QuestionAndAnswerEntities();
        public ActionResult History()
        {
            if(Session["TaiKhoan"]== null)
            {
                return RedirectToAction("Login", "User");
            }
            Account ac = (Account)Session["TaiKhoan"];
            var ct = from s in db.Questions
                     where s.IDAccount == ac.IDAccount
                     orderby s.IDQuestion descending
                     select s;
            return View(ct);
        }
        public JsonResult Delete(int id)
        {
            try
            {
                Question qs = db.Questions.SingleOrDefault(n => n.IDQuestion==id);
                if(qs!=null)
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
        public ActionResult Edit(int id)
        {
            Question ed= db.Questions.SingleOrDefault(n=>n.IDQuestion==id);
            ViewBag.DM = new SelectList(db.Categories.ToList().OrderBy(n => n.Name), "IDCategory", "Name",ed.IDCategory);
            return View(ed);
        }
        [HttpPost]
        [ValidateInput(false)]
        public ActionResult Edit(FormCollection f,int id)
        {
            Question qt = db.Questions.SingleOrDefault(n => n.IDQuestion==id);
            qt.Topic = f["title"];
            qt.Content = f["contentDetails"];
            qt.IDCategory = int.Parse(f["DM"]);
            
                qt.Incognito = false;
            db.SaveChanges();
            return RedirectToAction("History");
        }
    }
}