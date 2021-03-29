using System;
using System.Collections.Generic;
using TM;

namespace My
{
   public class Plan
   {  
      public Dictionary<int, PlanSpot> Load(string file)
      {
         return TMClient.LoadPlanData(file);
      }
   }
}