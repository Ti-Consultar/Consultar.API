﻿using _2___Application._2_Dto_s.BusinesEntity;
using _2___Application._2_Dto_s.Company.SubCompany;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace _2___Application._2_Dto_s.Company
{
    public class InsertCompanyDto
    {

        public int GroupId { get; set; }
        public string Name { get; set; }
        public InsertBusinessEntityDto BusinessEntity { get; set; }

    }
   
}
