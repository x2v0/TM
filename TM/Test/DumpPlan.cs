using System;
using System.Collections.Generic;
using TM;

namespace D
{
   public class Plan
   {
      public Dictionary<int, PlanSpot> Load()
      {
         var plan = TMClient.LoadPlanData("test_plan.txt");
         Console.WriteLine("\nPress any key to continue ...");
         Console.ReadKey();
         return plan;
      }
   }
}