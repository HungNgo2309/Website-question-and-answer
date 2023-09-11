using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using QA.Common;
using QA.Models;
using System.Net;
using System.Runtime.Serialization.Json;
using System.IO;
using System.Text;

namespace QA.Controllers
{
    public class InfoController : Controller
    {
        // GET: Info

        QuestionAndAnswerEntities db = new QuestionAndAnswerEntities();
        public JsonResult TPointQ(int idquestion)
        {
            try
            {
                Account ac = (Account)Session["TaiKhoan"];
                Question qs = db.Questions.SingleOrDefault(n => n.IDQuestion == idquestion);
                Point po = db.Points.SingleOrDefault(n => n.IDAccount == ac.IDAccount && n.IDQuestion == idquestion);
                if (po != null)
                {
                    po.Increase = true;
                    Account sh = db.Accounts.SingleOrDefault(n => n.IDAccount == qs.IDAccount);
                    sh.Point++;
                    db.SaveChanges();
                    return Json(new { code = 200 }, JsonRequestBehavior.AllowGet);
                }
                    Point p = new Point();
                    p.IDAccount = ac.IDAccount;
                    p.IDQuestion = qs.IDQuestion;
                    p.Increase = true;
                    db.Points.Add(p);
                    db.SaveChanges();
                    Account ch = db.Accounts.SingleOrDefault(n => n.IDAccount == qs.IDAccount);
                    ch.Point++;
                    db.SaveChanges();
                    return Json(new { code = 200 }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new { code = 500 }, JsonRequestBehavior.AllowGet);
            }
        }
        public JsonResult GPointQ(int idquestion)
        {
            try
            {
                Account ac = (Account)Session["TaiKhoan"];
                Question qs = db.Questions.SingleOrDefault(n => n.IDQuestion == idquestion);
                Point po = db.Points.SingleOrDefault(n => n.IDAccount == ac.IDAccount && n.IDQuestion == idquestion);
                if (po != null)
                {
                    po.Increase = false;
                    Account sh = db.Accounts.SingleOrDefault(n => n.IDAccount == qs.IDAccount);
                    sh.Point--;
                    db.SaveChanges();
                    return Json(new { code = 200 }, JsonRequestBehavior.AllowGet);
                }
                Point p = new Point();
                p.IDAccount = ac.IDAccount;
                p.IDQuestion = idquestion;
                p.Increase = false;
                db.Points.Add(p);
                Account ch = db.Accounts.SingleOrDefault(n => n.IDAccount == qs.IDAccount);
                ch.Point--;
                db.SaveChanges();
                return Json(new { code = 200 }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new { code = 500 }, JsonRequestBehavior.AllowGet);
            }
        }
        public JsonResult ShowPoint(int id)
        {
            try
            {
                Account ac = db.Accounts.SingleOrDefault(n => n.IDAccount == id);
                return Json(new { code = 200, resp = ac.Point }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new { code = 500 }, JsonRequestBehavior.AllowGet);
            }
        }
        public ActionResult Info()
        {

            Account ac = (Account)Session["TaiKhoan"];
            if (ac == null)
            {
                return RedirectToAction("Index", "Home");
            }
            Account fi = db.Accounts.SingleOrDefault(n => n.IDAccount == ac.IDAccount);
            return View(fi);
        }
        public ActionResult Notify()
        {
            Account ac = (Account)Session["TaiKhoan"];
            var recent = from s in db.Answers
                         where s.Question.IDAccount == ac.IDAccount && s.IDAccount != ac.IDAccount
                         orderby s.IDAnswer descending
                         select s;
           
            return PartialView(recent);
        }
        [HttpPost]
        [ValidateInput(false)]
        public ActionResult Info(FormCollection f, HttpPostedFileBase fFileUpload)
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

            return RedirectToAction("Info");
        } 
        public string InfoRank(int id)
        {
            string kq = null;
            if(RankAn(id)>0)
            {
                int lstGioHang = RankAn(id);
                if (lstGioHang >= 100)
                {
                    kq=kq+ "Hội viên xuất sắc";
                }
                else if (lstGioHang >= 50)
                {
                    kq =kq+ "Hội viên thông thạo";
                }
                else if (lstGioHang >= 25)
                {
                    kq =kq+ "Hội viên có sự đóng góp tốt";
                }
            }
            if (RankQu(id) >0)
            {
                int lstGioHang = RankQu(id);
                if (lstGioHang > 10000)
                {
                    kq=kq+ "Hội viên nổi tiếng";
                }
            }
            if (RankPointQu(id) >0)
            {
                int lstGioHang = RankPointQu(id);
                if (lstGioHang >= 100)
                {
                    kq=kq+ "Hội viên nổi tiếng";
                }
            }
            if(kq==null)
            {
                kq = "Hội viên";
            }
            return kq;
        }
        public int RankAn(int id)
        {
            // số lượng câu trả lời
            List<Answer> lstGioHang = (from s in db.Answers
                                           where  s.IDAccount==id && s.PointAS.FirstOrDefault(n=>n.IDAnswer==s.IDAnswer) != null 
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

        public ActionResult Logout()
        {
            Session["TaiKhoan"] = null;
            return RedirectToAction("Index", "Home");
        }
        [HttpGet]
        public ActionResult ChangePass()
        {
            if ((Request.QueryString["id"]) != Session["random"].ToString())
            {
                return RedirectToAction("Index", "Home");
            }
            if (Session["TaiKhoan"]==null)
            {
                return RedirectToAction("Index", "Home");
            }
            return View();
        }
        [HttpPost]
        public ActionResult ChangePass(FormCollection f)
        {
            Account ac = (Account)Session["TaiKhoan"];
             if(f["passnew"]!= f["passenter"])
            {
                ViewData["err3"] = "Mật khẩu nhập lại không trùng khớp";
                return RedirectToAction("ChangePass");
            }
            else if (f["passnew"] == null)
            {
                ViewData["err2"] = "Mật khẩu mới không được để trống";
                return RedirectToAction("ChangePass");
            }
            else if ( f["passenter"]==null)
            {
                ViewData["err3"] = "Vui lòng xác nhận mật khẩu";
                return RedirectToAction("ChangePass");
            }
            else
            {
                Account ad = db.Accounts.SingleOrDefault(m => m.IDAccount == ac.IDAccount);
                ad.PassWord = f["passnew"];
                db.SaveChanges();
                Session["random"] = null;
                return RedirectToAction("Info");
            }
        }
        public ActionResult pass()
        {
            Session["random"] = null;
            Account ac = (Account)Session["TaiKhoan"];
            string content = System.IO.File.ReadAllText(Server.MapPath("~/Content/template/changpass.html"));
            content = content.Replace("{{CustomerName}}", ac.UserName);
            content = content.Replace("{{Email}}", ac.Email);
            content = content.Replace("{{Date}}", DateTime.Now.ToString("MM/dd/yyyy"));
            var random = RandomString();
            Session["random"] = random;
            var noidung = "http://localhost:51654/Info/ChangePass?id=" + random;
            content = content.Replace("{{Link}}", noidung.ToString());

            var toEmail = ConfigurationManager.AppSettings["ToEmailAddress"].ToString();

            // Để Gmail cho phép SmtpClient kết nối đến server SMTP của nó với xác thực 
            //là tài khoản gmail của bạn, bạn cần thiết lập tài khoản email của bạn như sau:
            //Vào địa chỉ https://myaccount.google.com/security  Ở menu trái chọn mục Bảo mật, sau đó tại mục Quyền truy cập 
            //của ứng dụng kém an toàn phải ở chế độ bật
            //  Đồng thời tài khoản Gmail cũng cần bật IMAP
            //Truy cập địa chỉ https://mail.google.com/mail/#settings/fwdandpop

            new MailHelper().SendMail(ac.Email, "Thay đổi mật khẩu", content);
            return RedirectToAction("Info");
        }
        private static Random random = new Random();
        public static string RandomString()
        {
            int length = 8;
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
            return new string(Enumerable.Repeat(chars, length)
                .Select(s => s[random.Next(s.Length)]).ToArray());
        }
        public ActionResult resetpass()
        {
            return View();
        }
        [HttpPost]
        public ActionResult resetpass(FormCollection f)
        {
            string email = f["email"];
            Account tk = db.Accounts.FirstOrDefault(m => m.Email == email);
            if (tk == null)
            {
                ViewBag.ThongBao = "Email không phù hợp.Vui lòng nhập lại hoặc tạo tài khoản mới";
                return this.resetpass();
            }
            string content = System.IO.File.ReadAllText(Server.MapPath("~/Content/template/changpass.html"));
            content = content.Replace("{{CustomerName}}", tk.UserName);
            content = content.Replace("{{Email}}", tk.Email);
            content = content.Replace("{{Date}}", DateTime.Now.ToString("MM/dd/yyyy"));
            var random = RandomString();
            Session["reset"] = random;
            var noidung = "http://localhost:51654/Info/Reset?id=" + random + "&ms=" + tk.IDAccount;
            content = content.Replace("{{Link}}", noidung.ToString());

            // Để Gmail cho phép SmtpClient kết nối đến server SMTP của nó với xác thực 
            //là tài khoản gmail của bạn, bạn cần thiết lập tài khoản email của bạn như sau:
            //Vào địa chỉ https://myaccount.google.com/security  Ở menu trái chọn mục Bảo mật, sau đó tại mục Quyền truy cập 
            //của ứng dụng kém an toàn phải ở chế độ bật
            //  Đồng thời tài khoản Gmail cũng cần bật IMAP
            //Truy cập địa chỉ https://mail.google.com/mail/#settings/fwdandpop

            new MailHelper().SendMail(tk.Email, "Đặt lại mật khẩu", content);
            ViewBag.ThongBao = "Vui lòng kiểm tra Email của bạn";
            return RedirectToAction("Login","User");
        }
        [HttpGet]
        public ActionResult Reset()
        {
            if ((Request.QueryString["id"]) != Session["reset"].ToString())
            {
                return RedirectToAction("Index", "Home");
            }
            int ms = int.Parse(Request.QueryString["ms"]);
            Account find = db.Accounts.Single(m => m.IDAccount == ms);
            return View(find);
        }
        [HttpPost]
        public ActionResult Reset(FormCollection f)
        {
            int mus = Convert.ToInt32(f["iduser"]);
            Account ac = db.Accounts.First(n => n.IDAccount == mus);
            if (f["newpass"] != f["enterpass"])
            {
                ViewBag.ThongBao = "Mật khẩu không trùng khớp";
                return this.ChangePass();
            }
            else
            {
                ac.PassWord = f["newpass"];
                db.SaveChanges();
                Session["reset"] = null;
                return RedirectToAction("Login", "User");
            }
        }
    }
   
}