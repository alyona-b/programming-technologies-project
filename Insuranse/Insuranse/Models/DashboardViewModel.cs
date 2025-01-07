using System;
using System.Collections.Generic;
using Insurance.Models;

namespace Insurance.Models
{
    public class DashboardViewModel
    {
        public List<Contract> Contracts { get; set; }
        public List<Contract> ProblematicContracts { get; set; }
    }
}

