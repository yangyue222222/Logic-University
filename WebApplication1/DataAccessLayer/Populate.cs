using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using WebApplication1.Models;

namespace WebApplication1.DataAccessLayer
{
    public class Populate
    {
        public ICollection<Item> getClips()
        {
            var cat = "Clip";
            var uni = "Dozen";
            ICollection<Item> clips = new List<Item>();
            Item i1 = new Item()
            {
                Category = cat,
                Description = "Clips Double 1 in",
                UnitOfMeasure = uni
            };
            Item i2 = new Item()
            {
                Category = cat,
                Description = "Clips Double 2 in",
                UnitOfMeasure = uni
            };
            Item i3 = new Item()
            {
                Category = cat,
                Description = "Clips Double 3/4 in",
                UnitOfMeasure = uni
            };
            Item i4 = new Item()
            {
                Category = cat,
                Description = "Clips Paper Large",
                UnitOfMeasure = "Box"
            };
            Item i5 = new Item()
            {
                Category = cat,
                Description = "Clip Paper Medium",
                UnitOfMeasure = "Box"
            };
            Item i6 = new Item()
            {
                Category = cat,
                Description = "Clip Paper Small",
                UnitOfMeasure = "Box"
            };


            clips.Add(i1);
            clips.Add(i2);
            clips.Add(i3);
            clips.Add(i4);
            clips.Add(i5);
            clips.Add(i6);

            return clips;
        }

        public ICollection<Item> getEnvelopes()
        {
            ICollection<Item> envelopes = new List<Item>();
            var cat = "Envelope";
            var uni = "Each";

            string[] descriptions = new string[8]
            {
                "Envelope Brown(3in x 6in)",
                "Envelope Brown(3in x 6in) w/ Window",
                "Envelope Brown(5in x 7in)",
                "Envelope Brown(3in x 6in) w/ Window",
                "Envelope White(3in x 6in)",
                "Envelope Brown(3in x 6in) w/ Window",
                "Envelope Brown(5in x 7in)",
                "Envelope Brown(5in x 7in) w/ Window",
            };

            foreach (var s in descriptions)
            {
                Item i = new Item()
                {
                    Category = cat,
                    UnitOfMeasure = uni,
                    Description = s
                };
                envelopes.Add(i);
            }
            return envelopes;
        }

        public ICollection<Item> getErasers()
        {
            ICollection<Item> erasers = new List<Item>();
            var cat = "Eraser";
            var uni = "Each";
            string[] descriptions = {"Eraser (hard)","Eraser (soft)" };

            foreach (var s in descriptions)
            {
                Item i = new Item()
                {
                    Category = cat,
                    UnitOfMeasure = uni,
                    Description = s
                };
                erasers.Add(i);
            }
            return erasers;
        }

        public ICollection<Item> getExercises()
        {
            ICollection<Item> exercises = new List<Item>();
            var cat = "Exercise";
            var uni = "Each";
            string[] descriptions = {
                "Exercise Book (100 pg)",
                "Exercise Book (120 pg)" ,
                "Exercise Book A4 Hardcover (100 pg)",
                "Exercise Book A4 Hardcover (120 pg)",
                "Exercise Book A4 Hardcover (200 pg)",
                "Exercise Book Hardcover (100 pg)",
                "Exercise Book Hardcover (120 pg)"
            };

            foreach (var s in descriptions)
            {
                Item i = new Item()
                {
                    Category = cat,
                    UnitOfMeasure = uni,
                    Description = s
                };
                exercises.Add(i);
            }

            return exercises;
        }


        public ICollection<Item> getFiles()
        {
            ICollection<Item> files = new List<Item>();
            
            var cat = "File";
            var uni = "Each";

            Item i1 = new Item()
            {
                Category = cat,
                Description = "File Separator",
                UnitOfMeasure = "Set"
            };
            files.Add(i1);
            string[] descriptions = {
                "File-Blue Plain",
                "File-Blue with Logo" ,
                "File-Brown w/o Logo",
                "File-Brown with Logo",
                "Folder Plastic Blue",
                "Folder Plastic Clear",
                "Folder Plastic Green",
                "Folder Plastic Pink",
                "Folder Plastic Yellow"
            };

            foreach (var s in descriptions)
            {
                Item i = new Item()
                {
                    Category = cat,
                    UnitOfMeasure = uni,
                    Description = s
                };
                files.Add(i);
            }

            return files;
        }

        public ICollection<Item> getPens()
        {
            ICollection<Item> pens = new List<Item>();

            var cat = "Pen";
            var uni = "Box";

            Item i1 = new Item()
            {
                Category = cat,
                Description = "Pen Transparency Soluble",
                UnitOfMeasure = "Packet"
            };
            pens.Add(i1);
            
            string[] descriptions = {
                "Highlighter Blue",
                "Highlighter Green" ,
                "Highlighter Pink",
                "Highlighter Yellow",
                "Pen Whiteboard Marker Black",
                "Pen Whiteboard Marker Blue",
                "Pen Whiteboard Marker Green",
                "Pen Whiteboard Marker Red",
            };

            var un = "Dozen";
            string[] desc =
            {
                "Pencil 2B",
                "Pencil 2B with Eraser End",
                "Pencil 4H",
                "Pencil B",
                "Pencil B With Eraser End",
            };

            foreach (var s in descriptions)
            {
                Item i = new Item()
                {
                    Category = cat,
                    UnitOfMeasure = uni,
                    Description = s
                };
                pens.Add(i);
            }

            foreach (var s in desc)
            {
                Item i = new Item()
                {
                    Category = cat,
                    UnitOfMeasure = un,
                    Description = s
                };
                pens.Add(i);
            }

            return pens;
        }

        public ICollection<Item> getPunchers()
        {
            ICollection<Item> punchers = new List<Item>();
            var cat = "Puncher";
            var uni = "Each";
            string[] desc = {
                "Hole Puncher 2 holes",
                "Hole Puncher 3 holes",
                "Hole puncher Adjustable"
            };
            foreach (var d in desc)
            {
                Item i = new Item()
                {
                    Category = cat,
                    UnitOfMeasure = uni,
                    Description = d
                };
                punchers.Add(i);
            }

            return punchers;
        }

        public ICollection<Item> getPads()
        {
            ICollection<Item> pads = new List<Item>();
            var cat = "Pad";
            var uni = "Packet";
            string[] desc = {
                "Pad Postit Memo 1inx2in",
                "Pad Postit Memo 1/2in x 1in",
                "Pad Postit Memo 1/2inx2in",
                "Pad Postit Memo 2in x 3in",
                "Pad Postit Memo 2in x 4in",
                "Pad Postit Memo 2in x 5in",
                "Pad Postit Memo 3/4in x 2in"
            };
            
            foreach (var s in desc)
            {
                Item i = new Item()
                {
                    Description = s,
                    Category = cat,
                    UnitOfMeasure = uni
                };

                pads.Add(i);

            }

            return pads;
        }

        public ICollection<Item> getPapers()
        {
            ICollection<Item> papers = new List<Item>();
            Item i1 = new Item()
            {
                Category = "Paper",
                Description = "Paper Photostat A3",
                UnitOfMeasure = "Box"
            };

            Item i2 = new Item()
            {
                Category = "Paper",
                Description = "Paper Photostat A4",
                UnitOfMeasure = "Box"
            };

            papers.Add(i1);
            papers.Add(i2);

            return papers;
        }

        public ICollection<Item> getRulersAndScissor()
        {
            ICollection<Item> rulers = new List<Item>();
            Item i1 = new Item()
            {
                Description = "Ruler 12 in",
                Category = "Ruler",
                UnitOfMeasure = "Dozen"
            };

            Item i2 = new Item()
            {
                Description = "Ruler 6 in",
                Category = "Ruler",
                UnitOfMeasure = "Dozen"
            };
            Item i3 = new Item()
            {
                Category = "Scissors",
                Description = "Scissors",
                UnitOfMeasure = "Each"
            };

            rulers.Add(i1);
            rulers.Add(i2);
            rulers.Add(i3);

            return rulers;
        }

        public ICollection<Item> getTapesAndSharpener()
        {
            ICollection<Item> tapes = new List<Item>();
            Item i1 = new Item() {
                Category = "Tape",
                Description = "Scotch Tape",
                UnitOfMeasure = "Each",
            };

            Item i2 = new Item()
            {
                Category = "Tape",
                Description = "Scotch Tape Dispenser",
                UnitOfMeasure = "Each",
            };

            Item i3 = new Item()
            {
                Category = "Sharpener",
                Description = "Sharpener",
                UnitOfMeasure = "Each",
            };
            tapes.Add(i1);
            tapes.Add(i2);
            tapes.Add(i3);

            return tapes;
        }

        public ICollection<Item> getShorthands()
        {
            ICollection<Item> shorthands = new List<Item>();
            var cat = "Shorthand";
            var uni = "Each";
            string[] desc =
            {
                "Shorthand Book (100 pg)",
                "Shorthand Book (120 pg)",
                "Shorthand Book (80 pg)"
            };

            foreach(var s in desc)
            {
                Item i = new Item()
                {
                    Category = cat,
                    Description = s,
                    UnitOfMeasure = uni
                };
                shorthands.Add(i);
            }

            return shorthands;
        }

        public ICollection<Item> getStaplers()
        {
            ICollection<Item> staplers = new List<Item>();

            Item i1 = new Item()
            {
                Category = "Stapler",
                Description = "Stapler No. 28",
                UnitOfMeasure = "Each"
            };

            Item i2 = new Item()
            {
                Category = "Stapler",
                Description = "Stapler No. 36",
                UnitOfMeasure = "Each"
            };


            Item i3 = new Item()
            {
                Category = "Stapler",
                Description = "Stapler No. 28",
                UnitOfMeasure = "Box"
            };

            Item i4 = new Item()
            {
                Category = "Stapler",
                Description = "Stapler No. 36",
                UnitOfMeasure = "Box"
            };

            staplers.Add(i1);
            staplers.Add(i2);
            staplers.Add(i3);
            staplers.Add(i4);

            return staplers;
        }

        public ICollection<Item> getTackAndTrays()
        {
            var cat = "Tacks";
            var uni = "Box";
            ICollection<Item> tacks = new List<Item>();
            string[] desc = {
                "Thumb Tacks Large",
                "Thumb Tacks Medium",
                "Thumb Tacks Small"
            };

            foreach (var s in desc)
            {
                Item i = new Item()
                {
                    Category = cat,
                    UnitOfMeasure = uni,
                    Description = s
                };
                tacks.Add(i);
            }

            Item i1 = new Item()
            {
                Category = "Tray",
                Description = "Trays In/Out",
                UnitOfMeasure = "Set"
            };

            tacks.Add(i1);
            return tacks;
        }

        public ICollection<Item> getTparency()
        {
            var cat = "Tparency";
            var uni = "Box";
            ICollection<Item> t = new List<Item>();
            string[] desc =
            {
                "Transparency Blue",
                "Transparency Clear",
                "Transparency Green",
                "Transparency Red",
                "Transparency Reverse Blue",
                "Transparency Cover 3M"
            };

            foreach(var s in desc)
            {
                Item i = new Item()
                {
                    Description = s,
                    UnitOfMeasure = uni,
                    Category = cat
                };

                t.Add(i);
            }

            return t;
        }

    }
}