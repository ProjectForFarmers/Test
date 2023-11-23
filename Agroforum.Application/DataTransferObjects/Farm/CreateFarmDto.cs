﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Agroforum.Application.DataTransferObjects.Farm
{
    public class CreateFarmDto
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public string ContactEmail { get; set; }
        public Guid OwnerId { get; set; }
        public string Region { get; set; }
        public string Settlement { get; set; }
        public string Street { get; set; }
        public string HouseNumber { get; set; }
        public string PostalCode { get; set; }
        public ICollection<byte[]>? Images { get; set; }
    }
}