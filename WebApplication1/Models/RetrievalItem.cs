﻿using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace WebApplication1.Models
{
    [NotMapped]
    public class RetrievalItem
    {
        public string Description { get; set; }
        public int Quantity { get; set; }
        public int ItemId { get; set; }
    }
}