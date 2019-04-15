﻿using System;
using System.ComponentModel.DataAnnotations;

namespace BangazonWorkForceManagement.Models
{

    public class Computer
    {

        public int Id { get; set; }

        [Required]
        public DateTime PurchaseDate { get; set; }

        [Required]
        public DateTime? DecommissionDate { get; set; }

        [Required]
        public string Make { get; set; }

        [Required]
        public string Manufacturer { get; set; }
    }
}
