using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.IO;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using _29_7_2024.Models;

namespace _29_7_2024.Controllers
{
    public class AccountController : Controller
    {
        private UserINFOEntities db = new UserINFOEntities();

        // GET: Account
        public ActionResult Index()
        {
            return View(db.userinfoes.ToList());
        }



        private readonly (string Email, string Password)[] validUsers = new[]
       {
            (Email: "dima@gmail.com", Password: "123123"),
            (Email: "lujain@gmail.com", Password: "123")
            // Add other valid users as needed
        };

        public ActionResult Login() { return View(); }
        [HttpPost]
        public ActionResult Login(string Email, string password)
        {
            //bool isValidUser = validUsers.Any(X => X.Email == Email && X.Password == password);
            var user = db.userinfoes.SingleOrDefault(u => u.userEmail == Email);
            if (user != null && user.userPassword == password)
            {
                Session["userId"] = user.userID;
                return RedirectToAction("Profile");
            }
            //if (isValidUser)
            //{
            //    Session["User"] = Email;
            //    return RedirectToAction("Index", "Home");.
            //}

            ViewBag.ErrorMessage = "Invalid Email or password.";
            return View();
        }

        // GET: Login/Logout
        public ActionResult Logout()
        {
            Session["userId"] = null;
            return RedirectToAction("Index", "Home");
        }

        // Logout action


        // Profile page, requires authorization
        public ActionResult Profile()
        {
            var id = Session["userId"];
            var user = db.userinfoes.Find(id);
            return View(user);
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Profile(HttpPostedFileBase imageFile)
        {
            var userId = Session["userId"];
            if (userId == null)
            {
                return RedirectToAction("Login");
            }

            var user = db.userinfoes.Find(userId);
            if (user == null)
            {
                return HttpNotFound();
            }

            if (imageFile != null && imageFile.ContentLength > 0)
            {
                var fileName = Path.GetFileName(imageFile.FileName);
                var uniqueFileName = Guid.NewGuid().ToString() + "_" + fileName;
                var path = Path.Combine(Server.MapPath("~/Images/"), uniqueFileName);

                imageFile.SaveAs(path);

                // Delete the old image file if it exists
                if (!string.IsNullOrEmpty(user.image))
                {
                    var oldImagePath = Path.Combine(Server.MapPath("~/Images"), user.image);
                    if (System.IO.File.Exists(oldImagePath))
                    {
                        System.IO.File.Delete(oldImagePath);
                    }
                }

                user.image = uniqueFileName;
                db.Entry(user).State = EntityState.Modified;
                db.SaveChanges();
            }

            return RedirectToAction("Profile");
        }
        public ActionResult Register()
        {
            return View();
        }
        // GET: Account/Details/5
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            userinfo userinfo = db.userinfoes.Find(id);
            if (userinfo == null)
            {
                return HttpNotFound();
            }
            return View(userinfo);
        }

        // GET: Account/Create
        public ActionResult Create()
        {
            return View();
        }

        // POST: Account/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Register(userinfo userinfo, string confirmPassword)
        {
            if (ModelState.IsValid)
            {
                if (userinfo.userPassword != confirmPassword)
                    return View();
                db.userinfoes.Add(userinfo);
                db.SaveChanges();
                return RedirectToAction("Login");
            }

            return View(userinfo);
        }

        // GET: Account/Edit/5
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            userinfo userinfo = db.userinfoes.Find(id);
            if (userinfo == null)
            {
                return HttpNotFound();
            }
            return View(userinfo);
        }

        // POST: Account/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "userID,userName,userEmail,userPassword,image")] userinfo userinfo)
        {
            if (ModelState.IsValid)
            {
                db.Entry(userinfo).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            return View(userinfo);
        }
        // GET: Account/Delete/5
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            userinfo userinfo = db.userinfoes.Find(id);
            if (userinfo == null)
            {
                return HttpNotFound();
            }
            return View(userinfo);
        }

        // POST: Account/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            userinfo userinfo = db.userinfoes.Find(id);
            db.userinfoes.Remove(userinfo);
            db.SaveChanges();
            return RedirectToAction("Index");
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
            }
            base.Dispose(disposing);
        }
        public ActionResult ResetPassword()
        {
            var userId = Session["userId"];
            if (userId == null)
            {
                return RedirectToAction("Login");
            }
            return View();
        }
        [HttpPost]
[ValidateAntiForgeryToken]
        public ActionResult ResetPassword(string oldPassword, string newPassword, string confirmPassword)
        {
            var userId = Session["userId"];
            if (userId == null)
            {
                return RedirectToAction("Login");
            }

            var user = db.userinfoes.Find(userId);
            if (user == null)
            {
                return HttpNotFound();
            }

            if (user.userPassword != oldPassword)
            {
                ModelState.AddModelError("", "Old password is incorrect.");
                return View();
            }

            if (newPassword != confirmPassword)
            {
                ModelState.AddModelError("", "New password and confirmation password do not match.");
                return View();
            }

            user.userPassword = newPassword;
            db.Entry(user).State = EntityState.Modified;
            db.SaveChanges();

            ViewBag.SuccessMessage = "Password has been reset successfully.";
            return View();
        }
    }
}
