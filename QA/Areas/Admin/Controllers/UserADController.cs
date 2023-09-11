using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using QA.Models;

namespace QA.Areas.Admin.Controllers
{
    public class UserADController : Controller
    {
        // GET: Admin/User
        QuestionAndAnswerEntities db = new QuestionAndAnswerEntities();
        public ActionResult Login()
        {
            return View();
        }
        [HttpPost]
        public ActionResult Login(FormCollection f)
        {           
                var DN = f["login"];
                var Matkhau = f["password"];
                if (String.IsNullOrEmpty(DN))
                {
                    ViewData["err1"] = "Tài khoản không được để trống";
                }
                else if (String.IsNullOrEmpty(Matkhau))
                {
                    ViewData["err2"] = "Mật khẩu không được để trống";
                }
                else
                {
                    var ac = db.Admins.SingleOrDefault(n => n.UserName == DN  && n.PassWord == Matkhau);
                    if (ac != null)
                    {
                        ViewBag.ThongBao = "Đăng nhập thành công";
                        Session["Admin"] = ac;
                        return RedirectToAction("Index", "Manager");
                    }
                    else
                    {
                        ViewBag.ThongBao = "Tài khoản hoặc mật khẩu không đúng";
                    }
                }
            
            return this.Login();
        }
        public ActionResult ManagerUser()
        {
            var nd = from s in db.Accounts
                     orderby s.IDAccount descending
                     select s;
            return View(nd);
        }
        public ActionResult Detail(int id)
        {
            Account qs = db.Accounts.SingleOrDefault(m => m.IDAccount == id);
            return View(qs);
        }
        public int RankAn(int id)
        {
            // số lượng câu trả lời
            List<Answer> lstGioHang = (from s in db.Answers
                                       where s.IDAccount == id && s.PointAS.FirstOrDefault(n => n.IDAnswer == s.IDAnswer) != null
                                       select s).ToList();
            return lstGioHang.Count;
        }
        public int RankQu(int id)
        {
            // số lượt xem
            List<Question> lstGioHang = (from s in db.Questions
                                         where s.IDAccount == id
                                         select s).ToList();

            return lstGioHang.Sum(n => n.View);

        }
        public int RankPointQu(int id)
        {
            // Số lượng câu hỏi đc cho điêm
            List<Point> lstGioHang = (from s in db.Points
                                      where s.Question.IDAccount == id
                                      select s).ToList();
            return lstGioHang.Count;

        }
        public string InfoRank(int id)
        {
            string kq = null;
            if (RankAn(id) > 0)
            {
                int lstGioHang = RankAn(id);
                if (lstGioHang >= 100)
                {
                    kq = kq + "Hội viên xuất sắc";
                }
                else if (lstGioHang >= 50)
                {
                    kq = kq + "Hội viên thông thạo";
                }
                else if (lstGioHang >= 25)
                {
                    kq = kq + "Hội viên có sự đóng góp tốt";
                }
            }
            if (RankQu(id) > 0)
            {
                int lstGioHang = RankQu(id);
                if (lstGioHang > 10000)
                {
                    kq = kq + "Hội viên nổi tiếng";
                }
            }
            if (RankPointQu(id) > 0)
            {
                int lstGioHang = RankPointQu(id);
                if (lstGioHang >= 100)
                {
                    kq = kq + "Hội viên nổi tiếng";
                }
            }
            if (kq == null)
            {
                kq = "Hội viên";
            }
            return kq;
        }
        [HttpPost]
        [ValidateInput(false)]
        public ActionResult Detail(FormCollection f, HttpPostedFileBase fFileUpload)
        {
            int id = Int32.Parse(f["id"].ToString());
            Account ac = db.Accounts.SingleOrDefault(n => n.IDAccount == id);
            if (ModelState.IsValid)
            {
                if (fFileUpload != null)
                {
                    var sFileName = Path.GetFileName(fFileUpload.FileName);
                    var path = Path.Combine(Server.MapPath("~/Content/Images/avatar"), sFileName);
                    if (!System.IO.File.Exists(path))
                    {
                        fFileUpload.SaveAs(path);
                    }
                    ac.Avatar = sFileName;
                }
                ac.Email = f["email"].ToString();
                ac.UserName = f["name"].ToString();
                db.SaveChanges();
            }

            return RedirectToAction("Detail",new { id=id });
        }
        public ActionResult Delete(int id)
        {
            Account ac = db.Accounts.SingleOrDefault(n => n.IDAccount==id);
            db.Accounts.Remove(ac);
            db.SaveChanges();
            return RedirectToAction("ManagerUser");
        }
        public ActionResult Block(int id)
        {
            Account ac = db.Accounts.SingleOrDefault(n => n.IDAccount == id);
            if(ac.Block==0)
            {
                ac.Block = 1;
            }
            else { ac.Block = 0; }
            db.SaveChanges();
            return RedirectToAction("ManagerUser");
        }
    }
}