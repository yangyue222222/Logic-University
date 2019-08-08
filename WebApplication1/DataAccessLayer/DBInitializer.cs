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
            sr.setReps(context);

            base.Seed(context);
        }
    }
}