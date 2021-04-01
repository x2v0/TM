using System.Collections.Generic;
using TM;

namespace My
{
   public class Plan
   {  
      public List<PlanSpot> Load(string file)
      {
         return TMClient.LoadPlanData(file);
      }
   }
}