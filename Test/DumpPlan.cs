
using System;
using System.IO;
using System.Text;
using System.Collections.Generic;
using TM;
using TM.Properties;

namespace Dump
{
	public class Plan
	{
		public Dictionary<int, PlanSpot> Load()
      {  
         var plan = TMClient.LoadPlanData("test_plan.txt");

         Console.WriteLine("\nПродолжить ...");
         Console.WriteLine("__________________________________________________________________________");
         Console.ReadKey();
         
         return plan;
		}
	}
}