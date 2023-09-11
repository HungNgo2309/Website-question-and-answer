using QA.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Runtime.Serialization.Json;
using System.Text;
using System.Web;
using System.Web.Mvc;

namespace QA.Controllers
{
    public class UserController : Controller
    {
        // GET: User
        QuestionAndAnswerEntities db = new QuestionAndAnswerEntities();
        public ActionResult Register()
        {
            return View();
        }
        [HttpPost]
        public ActionResult Register(FormCollection f)
        {
            var Hoten = f["username"];
            var MatKhau = f["password"];
            var NLMatKhau = f["repassword"];
            var Email = f["email"];
            int GT = Convert.ToInt32(f["gt"]);
            if (String.IsNullOrEmpty(Hoten))
            {
                ViewData["err1"] = " Họ tên không được để trống";
            }
            else if (String.IsNullOrEmpty(MatKhau))
            {
                ViewData["err2"] = "Mật khẩu không được để trống";
            }
            else if (String.IsNullOrEmpty(NLMatKhau))
            {
                ViewData["err3"] = "Phải nhập lại mật khẩu";
            }
            else if (MatKhau != NLMatKhau)
            {
                ViewData["err3"] = "Mật khẩu nhập lại không trùng khớp";
            }
            else if (String.IsNullOrEmpty(Email))
            {
                ViewData["err4"] = "Email không được để trống";
            }
            else if (db.Accounts.SingleOrDefault(n => n.Email == Email) != null)
            {
                ViewBag.ThongBao = "Tài khoản này đã tồn tại";
            }
            else
            {
                Account ac = new Account();
                ac.Email = Email;
                ac.UserName = Hoten;
                ac.PassWord = MatKhau;
                ac.Point = 0;
                ac.Gender = Convert.ToBoolean(GT);
                if(ac.Gender==true)
                {
                    ac.Avatar = "img_avatar1.png";
                }
                else
                {
                    ac.Avatar = "img_avatar5.png";
                }
                db.Accounts.Add(ac);
                db.SaveChanges();
                return RedirectToAction("Login");
            }
            return this.Register();
        }
        [HttpGet]
        public ActionResult Login()
        {
            return View();
        }
        [HttpPost]
        public ActionResult Login(FormCollection f)
        {
            if (IsValidRecaptcha(Request["g-recaptcha-response"]))
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
                    Account ac = db.Accounts.SingleOrDefault(n => n.Email == DN || n.UserName == DN && n.PassWord == Matkhau);
                    if (ac != null)
                    {
                        ViewBag.ThongBao = "Đăng nhập thành công";
                        Session["TaiKhoan"] = ac;
                        return RedirectToAction("Index", "Home");
                    }
                    else
                    {
                        ViewBag.ThongBao = "Tài khoản hoặc mật khẩu không đúng";
                    }
                }
            }
            return this.Login();
        }
        private bool IsValidRecaptcha(string recaptcha)
        {
            if (string.IsNullOrEmpty(recaptcha))
            {
                return false;
            }
            var secretKey = "6Lf-yw0jAAAAALn29HBaqIfdhm_N1vXxWFzP42sH";//Mã bí mật
            string remoteIp = Request.ServerVariables["REMOTE_ADDR"];
            string myParameters = String.Format("secret={0}&response={1}&remoteip={2}", secretKey, recaptcha, remoteIp);
            RecaptchaResult captchaResult;
            using (var wc = new WebClient())
            {
                wc.Headers[HttpRequestHeader.ContentType] = "application/x-www-form-urlencoded";
                var json = wc.UploadString("https://www.google.com/recaptcha/api/siteverify", myParameters);
                var js = new DataContractJsonSerializer(typeof(RecaptchaResult));
                var ms = new MemoryStream(Encoding.ASCII.GetBytes(json));
                captchaResult = js.ReadObject(ms) as RecaptchaResult;
                if (captchaResult != null && captchaResult.Success)
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
        }
        public ActionResult Rank()
        {
            var recent = from s in db.Accounts
                         orderby s.Point descending
                         select s;
            return PartialView(recent.Take(10));
        }
    }
}