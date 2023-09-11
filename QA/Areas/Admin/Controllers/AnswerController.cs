using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using QA.Models;

namespace QA.Areas.Admin.Controllers
{
    public class AnswerController : Controller
    {
        // GET: Admin/Answer
        QuestionAndAnswerEntities db = new QuestionAndAnswerEntities();
        public ActionResult ManagerAnswer(int idqs)
        {
            var n = from s in db.Answers
                    where s.IDQuestion == idqs
                    orderby s.IDAnswer descending
                    select s;
            Question qs = db.Questions.SingleOrDefault(s=>s.IDQuestion == idqs);
            ViewBag.Topic = qs.Topic;
            return View(n);
        }
        public ActionResult Detail(int idas)
        {
            Answer qs = db.Answers.SingleOrDefault(m => m.IDAnswer == idas);
            return View(qs);
        }
        [HttpPost]
        [ValidateInput(false)]
        public ActionResult Detail(FormCollection f, int id)
        {
            Answer qt = db.Answers.SingleOrDefault(n => n.IDAnswer == id);
            qt.Content = f["contentDetails"];
            if (Convert.ToInt32(f["incognito"]) == 1)
            {
                qt.Incognito = true;
            }
            else
            {
                qt.Incognito = false;
            }
            db.SaveChanges();
            return RedirectToAction("Detail","Answer",new { idas=id});
        }
        public ActionResult Delete(int id)
        {
            Answer an = db.Answers.SingleOrDefault(n => n.IDAnswer == id);
            int idqs = an.IDQuestion;
            db.Answers.Remove(an);
            db.SaveChanges();
            return RedirectToAction("ManagerAnswer", new { id = idqs });
        }
    }
}