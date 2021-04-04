using System.Collections.Generic;
using TMPlan;

namespace My
{
   public class Plan
   {  
      public List<Spot> Load(string file)
      {

         return PlanClient.LoadPlan(file);
      }
   }
}