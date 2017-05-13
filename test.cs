//test.cs
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Demo_1
{
    class Program
    {
        static void Main(string[] args)
        {
            List<char> lstTempSeg1 = new List<char>();//临时段1
            List<char> lstTempSeg2 = new List<char>();//临时段2
            char a = Convert.ToChar("0");

                lstTempSeg1.Add(a);
                lstTempSeg1.Add(a);
            String strSeg1 = new String(lstTempSeg1.ToArray());
            String strSeg2 = new String(lstTempSeg2.ToArray());

            Console.WriteLine(strSeg1);
            Console.WriteLine(strSeg2);

                int intItemValue = Convert.ToInt32(strSeg1, 2) + Convert.ToInt32(strSeg2, 2);
            Console.WriteLine(intItemValue);
            
        }
    }
}