using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Data.Entity;
using WebApplication1.Models;

namespace WebApplication1.DataAccessLayer
{
    public class DBInitializer : DropCreateDatabaseAlways<UniDBContext>
    {
        protected override void Seed(UniDBContext context)
        {
            Populate p = new Populate();
            context.Items.AddRange(p.getClips());
            context.Items.AddRange(p.getEnvelopes());
            context.Items.AddRange(p.getErasers());
            context.Items.AddRange(p.getExercises());
            context.Items.AddRange(p.getFiles());
            context.Items.AddRange(p.getPens());
            context.Items.AddRange(p.getPunchers());
            context.Items.AddRange(p.getPads());
            context.Items.AddRange(p.getPapers());
            context.Items.AddRange(p.getRulersAndScissor());
            context.Items.AddRange(p.getTapesAndSharpener());
            context.Items.AddRange(p.getShorthands());
            context.Items.AddRange(p.getStaplers());
            context.Items.AddRange(p.getTackAndTrays());
            context.Items.AddRange(p.getTparency());
            context.SaveChanges();

            List<Item> items = context.Items.ToList();
            foreach(var i in items)
            {
                i.Quantity = 100;
            }
            context.SaveChanges();

            PopulateUser u = new PopulateUser();
            u.populateUsers(context);

            PopulateSupplier sup = new PopulateSupplier();
            sup.populateSuppliers(context);

            SetRepresentatives sr = new SetRepresentatives();
            sr.setReps();

            PopulatePickUpPoint.PopulatePoints(context);

            User storeclerk1 = context.Users.Where(us => us.Username == "storeclerk1").SingleOrDefault();
            List<PickUpPoint> pts1 = context.PickUpPoints.Where(pi => pi.PickUpPointId == 1 || pi.PickUpPointId == 2).ToList();

            storeclerk1.PickUpPoints = pts1;

            User storeclerk2 = context.Users.Where(us => us.Username == "storeclerk2").SingleOrDefault();
            List<PickUpPoint> pts2 = context.PickUpPoints.Where(pi => pi.PickUpPointId == 3 || pi.PickUpPointId == 4).ToList();

            storeclerk2.PickUpPoints = pts2;

            User storeclerk3 = context.Users.Where(us => us.Username == "storeclerk3").SingleOrDefault();
            List<PickUpPoint> pts3 = context.PickUpPoints.Where(pi => pi.PickUpPointId == 5 || pi.PickUpPointId == 6).ToList();

            storeclerk3.PickUpPoints = pts3;

            List<Department> allDepartments = context.Departments.ToList();
            PickUpPoint pick = context.PickUpPoints.Where(pi => pi.PickUpPointId == 1).SingleOrDefault();

            foreach(var d in allDepartments)
            {
                d.PickupPoint = pick;
            }


            User csE1 = context.Users.Include("Department").Where(us => us.Username == "cse1").SingleOrDefault();
            Department csDep = csE1.Department;
            csDep.Representative = csE1;


            context.SaveChanges();


            base.Seed(context);
        }


    }
}