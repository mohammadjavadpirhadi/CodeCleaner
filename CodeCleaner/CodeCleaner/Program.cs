using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CodeCleaner
{
    class Program
    {
        static void Main(string[] args)
        {
            Scanner scanner = new Scanner("../../assets/Test.cs");
            Parser parser = new Parser(scanner);
            parser.Parse();
            Console.WriteLine("Number of errors detected: {0}", parser.errors.count);
            Console.ReadKey();
        }
    }
}
