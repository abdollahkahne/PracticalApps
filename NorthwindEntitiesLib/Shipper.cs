using System;
using System.Collections.Generic;

namespace PracticalApp.NorthwindEntitiesLib
{
    public class Shipper
    {
        public int ShipperID { get; set; }
        public string ShipperName { get; set; }
        public string Phone { get; set; }
        // related entities
        public List<Order> Orders { get; set; }
    }
}