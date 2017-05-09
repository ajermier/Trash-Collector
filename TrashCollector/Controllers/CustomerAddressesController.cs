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
    public class CustomerAddressesController : Controller
    {
        private ApplicationDbContext db = new ApplicationDbContext();

        // GET: CustomerAddresses1
        public ActionResult Index()
        {
            var customerAddresses = db.CustomerAddresses.Include(c => c.ApplicationUser);
            return View(customerAddresses.ToList());
        }

        // GET: CustomerAddresses1/Details/5
        public ActionResult Details(string id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            CustomerAddress customerAddress = db.CustomerAddresses.Find(id);
            if (customerAddress == null)
            {
                return HttpNotFound();
            }
            return View(customerAddress);
        }

        // GET: CustomerAddresses1/Create/6546464
        public ActionResult Create()
        {
            //ViewBag.UserId = new SelectList(db.ApplicationUsers, userId);
            return View("Create");
        }

        // POST: CustomerAddresses1/Create/654564
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult CreateAddress(CreateViewModel model)
        {
            var manager = new UserManager<ApplicationUser>(new UserStore<ApplicationUser>(ApplicationDbContext.Create()));
            var user = manager.FindById(User.Identity.GetUserId());
            if (ModelState.IsValid)
            {                
                var address = new CustomerAddress() { UserId = user.Id, Address1 = model.Address1, Address2 = model.Address2, City = model.City, State = model.State, Zip = model.Zip };
                db.CustomerAddresses.Add(address);
                db.SaveChanges();
                return RedirectToAction("Create", "CustomerDates");
            }

            //ViewBag.UserId = new SelectList(db.ApplicationUsers, "Id", "FirstName", customerAddress.UserId);
            return View("Index");
        }

        // GET: CustomerAddresses/Edit/5
        public ActionResult Edit()
        {
            var manager = new UserManager<ApplicationUser>(new UserStore<ApplicationUser>(ApplicationDbContext.Create()));
            var user = manager.FindById(User.Identity.GetUserId());

            if (user.Id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            CustomerAddress customerAddress = db.CustomerAddresses.Find(user.Id);
            if (customerAddress == null)
            {
                return RedirectToAction("Create");
            }
            else
            {
                //ViewBag.UserId = new SelectList(db.ApplicationUsers, "Id", user.Id, customerAddress.UserId);
                return View(customerAddress);
            }
        }

        // POST: CustomerAddresses1/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "UserId,Address1,Address2,City,State,Zip")] CustomerAddress customerAddress)
        {
            if (ModelState.IsValid)
            {
                db.Entry(customerAddress).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index", "Manage", new { Message = ManageMessageId.UpdateAddressSuccess });
            }
            //ViewBag.UserId = new SelectList(db.ApplicationUsers, "Id", "FirstName", customerAddress.UserId);
            return View();
        }

        // GET: CustomerAddresses1/Delete/5
        public ActionResult Delete(string id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            CustomerAddress customerAddress = db.CustomerAddresses.Find(id);
            if (customerAddress == null)
            {
                return HttpNotFound();
            }
            return View(customerAddress);
        }

        // POST: CustomerAddresses1/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(string id)
        {
            CustomerAddress customerAddress = db.CustomerAddresses.Find(id);
            db.CustomerAddresses.Remove(customerAddress);
            db.SaveChanges();
            return RedirectToAction("Index", "Manage");
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
