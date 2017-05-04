using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using TrashCollector.Models;
using static TrashCollector.Controllers.ManageController;

namespace TrashCollector.Controllers
{
    public class CustomerDatesController : Controller
    {
        private ApplicationDbContext db = new ApplicationDbContext();

        // GET: CustomerDates
        public ActionResult Index()
        {
            var customerDates = db.CustomerDates.Include(c => c.ApplicationUser);
            return View(customerDates.ToList());
        }

        // GET: CustomerDates/Details/5
        public ActionResult Details(string id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            CustomerDates customerDates = db.CustomerDates.Find(id);
            if (customerDates == null)
            {
                return HttpNotFound();
            }
            return View(customerDates);
        }

        // GET: CustomerDates/Create
        public ActionResult Create()
        {
            //ViewBag.UserId = new SelectList(db.ApplicationUsers, "Id", "FirstName");
            return View("Create");
        }

        // POST: CustomerDates/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(CustomerDates customerDates)
        {
            var manager = new UserManager<ApplicationUser>(new UserStore<ApplicationUser>(ApplicationDbContext.Create()));
            var user = manager.FindById(User.Identity.GetUserId());
            customerDates.UserId = user.Id;
            if (ModelState.IsValid)
            {
                db.CustomerDates.Add(customerDates);
                db.SaveChanges();
                return RedirectToAction("Index", "Manage", new { Message = ManageMessageId.UpdateDatesSuccess });
            }

            //ViewBag.UserId = new SelectList(db.ApplicationUsers, "Id", "FirstName", customerDates.UserId);
            return View(customerDates);
        }

        // GET: CustomerDates/Edit/5
        public ActionResult Edit()
        {
            var manager = new UserManager<ApplicationUser>(new UserStore<ApplicationUser>(ApplicationDbContext.Create()));
            var user = manager.FindById(User.Identity.GetUserId());

            if (user.Id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            CustomerDates customerDates = db.CustomerDates.Find(user.Id);
            if (customerDates == null)
            {
                return RedirectToAction("Create");
            }
            //ViewBag.UserId = new SelectList(db.ApplicationUsers, "Id", "FirstName", customerDates.UserId);
            return View(customerDates);
        }

        // POST: CustomerDates/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "UserId,DefaultDay,AlternatePickup,VacationStart,VacationEnd")] CustomerDates customerDates)
        {
            if (ModelState.IsValid)
            {
                db.Entry(customerDates).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index", "Manage", new { Message = ManageMessageId.UpdateDatesSuccess });
            }
            //ViewBag.UserId = new SelectList(db.ApplicationUsers, "Id", "FirstName", customerDates.UserId);
            return View(customerDates);
        }

        // GET: CustomerDates/Delete/5
        public ActionResult Delete(string id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            CustomerDates customerDates = db.CustomerDates.Find(id);
            if (customerDates == null)
            {
                return HttpNotFound();
            }
            return View(customerDates);
        }

        // POST: CustomerDates/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(string id)
        {
            CustomerDates customerDates = db.CustomerDates.Find(id);
            db.CustomerDates.Remove(customerDates);
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
    }
}
