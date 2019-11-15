using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace P1Test
{
    class program
    {
        static void Main(string[] args)
        {
            string[] arr = new string[10];
            int i = 0;
            for (int j = 0; j < arr.Length; i++)
            {
                arr[j] = get(i+1);
                i++;
            }
            while (i>=0)
            {
                Console.WriteLine(arr[i]);
                i--;
            }
        }

        int get(int i)
        {
            return i.ToString();
        }
    }

    public class checNams
    {

    }
}
