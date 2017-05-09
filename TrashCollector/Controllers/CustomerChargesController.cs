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
using System.Web.Security;
using TrashCollector.Models;
using static TrashCollector.Controllers.ManageController;

namespace TrashCollector.Controllers
{
    public class CustomerChargesController : Controller
    {
        private ApplicationDbContext db = new ApplicationDbContext();

        // GET: CustomerCharges
        public ActionResult Index()
        {
            var customerCharges = db.CustomerCharges.Include(c => c.ApplicationUser);
            return View(customerCharges.ToList());
        }

        // GET: CustomerCharges/Details/5
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            CustomerCharges customerCharges = db.CustomerCharges.Find(id);
            if (customerCharges == null)
            {
                return HttpNotFound();
            }
            return View(customerCharges);
        }

        // GET: CustomerCharges/Create
        public ActionResult Create(string id)
        {
            var model = new CustomerCharges();
            var user = db.Users.First(u => u.Id == id);
            model.ApplicationUserId = id;
            ViewBag.UserName = user.UserName;
            return View(model);
        }

        // POST: CustomerCharges/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "Date,Charge,ApplicationUserId")] CustomerCharges model)
        {
            if (ModelState.IsValid)
            {
                db.CustomerCharges.Add(model);
                db.SaveChanges();
                return RedirectToAction("Index", "Manage", new { Message = ManageMessageId.AddedChargeSuccess });
            }

            return View(model);
        }

        // GET: CustomerCharges/Edit/5
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            CustomerCharges customerCharges = db.CustomerCharges.Find(id);
            if (customerCharges == null)
            {
                return HttpNotFound();
            }
            ViewBag.ApplicationUserId = new SelectList(db.ApplicationUsers, "Id", "FirstName", customerCharges.ApplicationUserId);
            return View(customerCharges);
        }

        // POST: CustomerCharges/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "Id,Date,Charge,ApplicationUserId")] CustomerCharges customerCharges)
        {
            if (ModelState.IsValid)
            {
                db.Entry(customerCharges).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            ViewBag.ApplicationUserId = new SelectList(db.ApplicationUsers, "Id", "FirstName", customerCharges.ApplicationUserId);
            return View(customerCharges);
        }

        // GET: CustomerCharges/Delete/5
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            CustomerCharges customerCharges = db.CustomerCharges.Find(id);
            if (customerCharges == null)
            {
                return HttpNotFound();
            }
            return View(customerCharges);
        }

        // POST: CustomerCharges/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            CustomerCharges customerCharges = db.CustomerCharges.Find(id);
            db.CustomerCharges.Remove(customerCharges);
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
