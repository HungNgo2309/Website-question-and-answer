using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using QA.Models;
using PagedList;

namespace QA.Controllers
{
    public class QuestionController : Controller
    {
        // GET: Question
        QuestionAndAnswerEntities db = new QuestionAndAnswerEntities();
        public ActionResult RecentQuestion(int? page)
        {
            var recent = from s in db.Questions
                         orderby s.IDQuestion descending
                         where s.Verify==true
                         select s;
            if (page == null) page = 1;
            int pageSize = 3;
            int pageNumber = (page ?? 1);
            return PartialView(recent.ToPagedList(pageNumber, pageSize));
        }
        public ActionResult ViewQuestion(int? page)
        {
            var recent = from s in db.Questions
                         orderby s.View descending
                         where s.Verify == true
                         select s;
            if (page == null) page = 1;
            int pageSize = 3;
            int pageNumber = (page ?? 1);
            return PartialView(recent.ToPagedList(pageNumber, pageSize));
        }
        public ActionResult PostQuestion()
        {
            ViewBag.DM = new SelectList(db.Categories.ToList().OrderBy(n => n.Name), "IDCategory", "Name");
            return PartialView();
        }
        public string Tg(DateTime tgbd)
        {
            string kq = null;
            DateTime aDateTime = DateTime.Now;
            DateTime yk = new DateTime(tgbd.Year, tgbd.Month, tgbd.Day, tgbd.Hour, tgbd.Minute, tgbd.Second);
            TimeSpan interval = aDateTime.Subtract(yk);
            if (tgbd == null)
            {
                return kq;
            }
            if (interval.Days > 0)
            {
                kq = interval.Days + " ngày trước";
                return kq;
            }
            else if (interval.Hours > 0)
            {
                kq = interval.Hours + " giờ trước";
                return kq;
            }
            else if (interval.Minutes > 0)
            {
                kq = interval.Minutes + " phút trước";
                return kq;
            }
            else if (interval.Seconds > 0)
            {
                kq = "Vừa xong";
                return kq;
            }
            else
            {
                return kq;
            }

        }
        public JsonResult PostComment(int id, string nd)
        {
            try
            {
                if(nd==null)
                {
                    return Json(new { code = 500, msg = "Post Answer Empty" }, JsonRequestBehavior.AllowGet);
                }
                Account ac = (Account)Session["TaiKhoan"];
                Answer an = new Answer();
                an.IDAccount = ac.IDAccount;
                an.IDQuestion = id;
                an.Incognito = false;
                an.Content = nd;
                an.Datetime = DateTime.Now;
                an.Show = false;
                db.Answers.Add(an);
                db.SaveChanges();
                return Json(new { code = 200, msg = "Post Answer successfull" }, JsonRequestBehavior.AllowGet);
             }
            catch (Exception ex)
            {
                return Json(new { code = 500, msg = "Vui lòng đăng nhập" }, JsonRequestBehavior.AllowGet);
            }
        }
        public ActionResult Answer(int id)
        {
            var answer = from s in db.Answers
                         where s.IDQuestion== id
                         orderby s.IDAnswer descending
                         select s;
            ViewBag.IDQ = id;
            return PartialView(answer);
        }
        public JsonResult ShowAnswer(int id)
        {
            try
            {
                var ds = (from l in db.Answers.Where(x => x.IDQuestion == id)
                          select new
                          {
                              ID = l.IDAccount,
                              Anh = l.Account.Avatar,
                              Diem = l.Account.Point,
                              IDAS = l.IDAnswer,
                              Ten = l.Account.UserName,
                              Ma = l.IDQuestion,
                              ND = l.Content
                          }).ToList();
                return Json(new { code = 200, ds = ds, msg = "Post Answer fail" }, JsonRequestBehavior.AllowGet);
            }
            catch
            {
                return Json(new { code = 500, msg = "Post Answer fail" }, JsonRequestBehavior.AllowGet);
                    
            }
        }
        public ActionResult Point(int idquest)
        {
            var acc = from s in db.Questions
                         where s.IDQuestion==idquest
                         select s;
            return PartialView(acc);
        }
        public ActionResult Category(int id)
        {
            var ct = from s in db.Questions
                      where s.IDCategory == id
                     orderby s.IDQuestion descending
                     select s;
            
            ViewBag.Name = db.Categories.SingleOrDefault(m => m.IDCategory == id).Name;

            return View(ct);
        }
        public JsonResult PointAS(int idas, int idquestion)
        {
            try
            {
                Account lc = (Account)Session["TaiKhoan"];
                Question qs = db.Questions.SingleOrDefault(n => n.IDQuestion == idquestion);
                if(lc.IDAccount!=qs.IDAccount)
                {
                    return Json(new { code = 500, msg = "Post Answer fail" }, JsonRequestBehavior.AllowGet);
                }
                PointA sc = db.PointAS.SingleOrDefault(s => s.IDQuestion == idquestion);
                if(sc!=null)
                {
                    Answer anso = db.Answers.SingleOrDefault(m => m.IDAnswer == sc.IDAnswer);
                    Account aco = db.Accounts.SingleOrDefault(n => n.IDAccount == anso.IDAccount);
                    aco.Point--;
                    db.PointAS.Remove(sc);
                    db.SaveChanges();
                    if(sc.IDAnswer!=idas)
                    {
                        Answer ans = db.Answers.SingleOrDefault(m => m.IDAnswer == idas);
                        Account ac = db.Accounts.SingleOrDefault(n => n.IDAccount == ans.IDAccount);
                        ac.Point++;
                        PointA nm = new PointA();
                        nm.IDQuestion = idquestion;
                        nm.IDAnswer = idas;
                        db.PointAS.Add(nm);
                        db.SaveChanges();
                    }
                    return Json(new { code = 200, msg = "Post Answer fail" }, JsonRequestBehavior.AllowGet);
                }
                else
                {
                    Answer ans = db.Answers.SingleOrDefault(m => m.IDAnswer == idas);
                    Account ac = db.Accounts.SingleOrDefault(n => n.IDAccount == ans.IDAccount);
                    ac.Point++;
                    PointA nm = new PointA();
                    nm.IDQuestion = idquestion;
                    nm.IDAnswer = idas;
                    db.PointAS.Add(nm);
                    db.SaveChanges();
                    return Json(new { code = 200, msg = "Post Answer fail" }, JsonRequestBehavior.AllowGet);
                }
            }catch( Exception e)
            {
                return Json(new { code = 500, msg = "Post Answer fail" }, JsonRequestBehavior.AllowGet);
            }
        }
        public ActionResult NewQuestion()
        {
            var recent = from s in db.Questions
                         orderby s.IDQuestion descending
                         where s.Verify == true
                         select s;
            return PartialView(recent.Take(7));
        }
        public ActionResult Ranking()
        {
            var recent = from s in db.Accounts
                         orderby s.Point descending
                         select s;
            return PartialView(recent.Take(5));
        }
        [HttpPost]
        [ValidateInput(false)]
        public ActionResult AddA(FormCollection f,int id)
        {
            if (Session["TaiKhoan"] == null)
            {
                return RedirectToAction("Login", "User");
            }
            Account ac = (Account)Session["TaiKhoan"];
            if (ac.Block == 1)
            {
                TempData["message"] = "Tài khoản của bạn đã bị khóa không thể thực hiện chức năng này";
                return RedirectToAction("Detail","Home",new {id=id });
            }
            Answer qt = new Answer();
            qt.Content = f["content"];
            qt.IDQuestion = id;
            qt.Incognito = false;
            qt.Show = false;
            qt.IDAccount = ac.IDAccount;
            qt.Datetime = DateTime.Now;
            db.Answers.Add(qt);
            db.SaveChanges();
            return RedirectToAction("Detail","Home",new {id =id });
        }
        public ActionResult PointAnswer(int idas, int idquestion)
        {
            Account lc = (Account)Session["TaiKhoan"];
            Question qs = db.Questions.SingleOrDefault(n => n.IDQuestion == idquestion);
            if (lc.IDAccount != qs.IDAccount)
            {
                return RedirectToAction("Detail", "Home", new { id = idquestion });
            }
            PointA sc = db.PointAS.SingleOrDefault(s => s.IDQuestion == idquestion);
            if (sc != null)
            {
                Answer anso = db.Answers.SingleOrDefault(m => m.IDAnswer == sc.IDAnswer);
                Account aco = db.Accounts.SingleOrDefault(n => n.IDAccount == anso.IDAccount);
                aco.Point--;
                db.PointAS.Remove(sc);
                db.SaveChanges();
                if (sc.IDAnswer != idas)
                {
                    Answer ans = db.Answers.SingleOrDefault(m => m.IDAnswer == idas);
                    Account ac = db.Accounts.SingleOrDefault(n => n.IDAccount == ans.IDAccount);
                    ac.Point++;
                    PointA nm = new PointA();
                    nm.IDQuestion = idquestion;
                    nm.IDAnswer = idas;
                    db.PointAS.Add(nm);
                    db.SaveChanges();
                }
            }
            else
            {
                Answer ans = db.Answers.SingleOrDefault(m => m.IDAnswer == idas);
                Account ac = db.Accounts.SingleOrDefault(n => n.IDAccount == ans.IDAccount);
                ac.Point++;
                PointA nm = new PointA();
                nm.IDQuestion = idquestion;
                nm.IDAnswer = idas;
                db.PointAS.Add(nm);
                db.SaveChanges();
            }
            return RedirectToAction("Detail", "Home", new { id = idquestion });
        }
        public int TotalAs(int id)
        {
            var ct = from s in db.Answers
                     where s.IDQuestion == id
                     select s;
            return ct.Count();
        }
        public String LimitWord(String originalText)
        {
            string[] wordsArray = originalText.Split(' ');
            int maxWords = 2;
            //Nếu số từ trong mảng lớn hơn số từ tối đa, ta chỉ lấy số từ tối đa và thêm dấu chấm 3 chấm (...) vào cuối
            if (wordsArray.Length >= maxWords)
            {
                originalText = string.Join(" ", wordsArray.Take(maxWords));
                originalText += "...";
            }

            //Trả về đoạn văn bản mới đã giới hạn số từ
            return originalText;
        }
    }
}