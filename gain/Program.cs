using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
//using NumSharp;
using System.Diagnostics;
using System.Threading;
using System.Timers;
using Deedle;
//using Deedle.Internal;

namespace cs_test
{

    class Product
    {
        public string Name { get; private set; }

        //public decimal Price{ get; private set; }

        //Helpful, now it can be blank ! if there is no price : )
        decimal? price;
        public decimal? Price
        {
            get { return price; }
            private set { price = value; }
        }


        public Product(string name, decimal price)
        {
            Name = name;
            Price = price;
        }

        public static List<Product> GetSampleProducts()
        {
            return new List<Product>
            {
                new Product(name: "West Side Story",price: 100m),
                new Product(name: "Assassins",price: 10m),
                new Product(name: "Finding Nemo",price: 55m),
                new Product(name: "Finding Dory",price: 65m)
            };
        }

        public override string ToString()
        {
            return string.Format("{0} : {1}", Name, Price);
        }


        static string Reverse(string input)
        {
            char[] chars = input.ToCharArray();
            Array.Reverse(chars);
            return new string(chars);
        }




    }




    class Staff
    {
        private string nameOfStaff;
        private const int hourlyRate = 30;
        private int hWorked;
        public List<string> staffList;

        //Properties -- where a field is NEEDED BY OTHER CLASSES
        public int hoursWorked
        {
            get { return hWorked; }
            //set { (value > 0) ? hWorked = value : hWorked = 0; } Cant do with properties it seems
            set
            {
                if (value > 0)
                {
                    hWorked = value;
                }
                else
                {
                    hWorked = 0;
                }
            }
        }

        public List<string> StaffList { get => staffList; set => staffList = value; }


        //Constructor ! Knew this was missing (although implicit)
        public Staff(string name)
        {
            nameOfStaff = name;
            Console.WriteLine("\n" + nameOfStaff);
            Console.WriteLine(" ____________________ ");
        }

        //Alternate constructor (for 2 argument name!)
        public Staff(string first, string last)
        {
            nameOfStaff = first + " " + last;
            Console.WriteLine("\n" + nameOfStaff);
            Console.WriteLine(" ____________________ ");
        }




        //Autoimplemented property:  MUCH simpler IF NO SPECIFICS LIKE THIS IF
        //public int hoursWorked { get; private set; }

        //Methods !


        public void PrintMessage()
        {
            Console.WriteLine("Calculating pay...");
        }

        public int CalcPay()
        {
            PrintMessage(); // Call message method ! (calculating...)

            int staffPay;
            staffPay = hWorked * hourlyRate;

            if (hWorked > 0)
                return staffPay;
            else
                return 0;
        }

        public int CalcPay(int bonus, int allowance = 0)
        {
            PrintMessage();
            if (hWorked > 0)
                return hWorked * hourlyRate + bonus + allowance;
            else
                return 0;
        }

        //Override built in string class (like python __str__() class) -- str representation of class objects
        public override string ToString()
        {
            return "Name of staff: " + nameOfStaff + ", Hourly Rate: " + hourlyRate + " Hours Worked: " + hWorked;
        }


        public int CalculateCrunchies(int pricePerUnit, int unitQty)
        {
            int pay = CalcPay();
            return ((pay * 12) / pricePerUnit) * unitQty;
        }



        public void SortStaff()
        {
            StaffList.Sort();
            Console.WriteLine(StaffList);
        }

        public List<string> PrintStaff()         //HOW TO DECLARE THE RETURN TYPE AS A LIST !!
        {
            if (staffList.Count != 0)
                foreach (string s in staffList)
                {
                    Console.WriteLine(s);
                }
            else
                Console.WriteLine("Sorry, stafflist is empty!");
            return staffList.Count > 0 ? staffList : null;   //Prettay damn smart!

        }


        //Array as an argument
        public static void printArrElement(int[] a)
        {
            Console.WriteLine(a[0]);
        }

        //Array as a return value !
        public static int[] retArrElement()
        {
            int[] a = new int[3];
            for (int i = 0; i < a.Length; i++)
            {
                Console.WriteLine("Enter an integer:");
                a[i] = Convert.ToInt32(Console.ReadLine());
                Console.WriteLine("{0} added to array", a[i]);
            }

            return a;
        }

        //List as argument
        public static void printListElement(List<int> L)
        {
            Console.WriteLine(L[0]);
        }


        //Multiple / Unkown arguments (Like *args,**kwargs)
        public void multParams(params string[] names)
        {
            for (int i = 0; i < names.Length; i++)
            {
                Console.Write(names[i] + " ");
            }
            Console.WriteLine();
        }


        /*In C# -- STATIC means we can access it WITHOUT A CLASS INSTANCE -- outside of the class
         * STATICS are simply accessed with the class name.. EX: MyClass.Age; Staff.StaticMethod
         *
         * CAN ALSO HAVE STATIC CLASSES =
         * can ONLY CONTAIN STATIC FIELDS, METHODS, STATIC MEMBERS !
         *
         * ABSTRACT CLASSES CANNOT HAVE STATIC MEMBERS !!
         *
         * PASS BY REFERENCE (not only ptr, even the above array index is PASS BY REF)
         * MAINTAINS CHANGE OUTSIDE METHOD, whereas pass by value, the change is ONLY present INSIDE THE METHOD SCOPE!
         *
         *INTERNAL access mod means ONLY IN THIS ASSEMBLY (Project? Module?)
         *
         * Structs are VALUE types, classes are REF
         * Structs CAN use INTERFACES, but NO INHERITANCE !
         *
         */


    }





    class MainClass
    {
        public static void Main(string[] args)
        {
            Console.WriteLine("Hello World!");
            Console.WriteLine("Fuck yeah C#5 !");

            /*
            //Product p;
            // ("Leo",2500);
            Product p = new Product("Leo", 100m);
            Console.WriteLine(p.Name);

            //Needs to be static to be called like this ! (Static means it can be called outside!) BUT IT WORKS !
            Console.WriteLine(Person.Rev("Fuck Yea C#"));

            Console.WriteLine(Person.Rev("Leo is the best cat in the world!"));
            */



            /*
            ArrayList products = Product.GetSampleProducts();
            foreach(Product product in products)
            {
                if(product.Price > 10m)
                {
                    Console.WriteLine(product.Price);
                }
            }
            */

            List<Product> products = Product.GetSampleProducts();
            products.Sort((x, y) => string.Compare(x.Name, y.Name, StringComparison.Ordinal));
            foreach (Product product in products)
            {
                if (product.Price > 10m)
                {
                    Console.WriteLine(product);

                }
            }


            foreach (Product product in products)
            {
                int ct = 0;
                if (product.Name == "Finding Nemo")
                {
                    Console.WriteLine("Product Found! {0} is the {1} item in Product list", product, ct);
                }
                else if (product.Name != "Finding Nemo")
                {
                    ct++;
                }
                else
                {
                    ct++;
                }
            }


            /*
            List<Product> ps = Product.GetSampleProducts();
            foreach (Product product in products.Where(p => p.price > 10))
            {
                Console.WriteLine(product);
            }*/

            //Arrays: (Braces are on type, rather than var name !  (NOT like c++)
            int[] arrVar = { 1, 2, 3, 4, 5 };

            int[] newArr = new int[10]; //Can declare, BUT ARRAYS NEED A FIXED LENGTH!
            int idx = arrVar[2];
            Console.WriteLine(idx); //Prints 3

            arrVar[2] = arrVar[2] + 100;
            Console.WriteLine(arrVar[2]);
            Console.WriteLine("{0} {1}", arrVar[1], arrVar[2]);

            int[] dest = { 10, 20, 30, 40 };
            Array.Copy(arrVar, dest, 1);
            Console.WriteLine("{0} {1} {2} {3}", dest[0], dest[1], dest[2], dest[3]);


            int[] n = { -12, 1, 0, 5, 3, 17 };
            Console.WriteLine("{0} {1} {2} {3}", n[0], n[1], n[2], n[3]);


            int find = Array.IndexOf(n, 17);
            Console.WriteLine("{0}", find);

            Console.WriteLine(n.Length);

            //Strings

            string s = "Hello World";
            string ss = s.Substring(0, 5); //Idx, Length of substring
            Console.WriteLine(ss);

            bool TF = s.Equals(ss);
            Console.WriteLine(TF);

            //Lists

            List<int> newList = new List<int> { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10 };
            newList.Add(11);
            newList.Add(12);

            int len = newList.Count;

            newList.Insert(0, 0);

            Console.WriteLine("{0} {1} {2}", newList, len, newList.Count);


            newList.Remove(0); //Removes item  0 !

            newList.RemoveAt(5);

            newList.Contains(20);

            //newList.Clear(); //Deletes all elements!

            //Input:

            //string input = Console.ReadLine(); TAKE INPUT !
            //Convert !

            //int userInput = Convert.ToInt32(input); Type Conversion!

            //Ifs, Fors, and Switches IDENTICAL to c++

            char[] clist = { 'H', 'E', 'L', 'L', 'O' }; //Okay, simple enough


            for (int i = 0; i < clist.Length; i++)
            {
                Console.WriteLine(i);
            }

            /*
            //This isnt working for some reason... hm...
            foreach (char c in clist)
            {
                Console.WriteLine(clist[c]);
            }
            */

            int counter = 5;
            while (counter > 0)
            {
                Console.WriteLine(counter);
                counter--;
            }

            /*

            //Using CLASSES !!

            int pay;
            Staff s1 = new Staff("Leo");
            s1.hoursWorked = 160;
            pay = s1.CalcPay(1000, 400);
            Console.WriteLine("{0}'s pay is {1} ", "Leo", pay);
            Console.WriteLine("Congratulations Leo ! You\'re making {0} per Year! Good Boy !", pay * 12);

            decimal cpy = s1.CalculateCrunchies(4, 150);
            Console.WriteLine("Crunchies Per Year: {0}", cpy);



            //Inheritance  + Polymorphims

            NormalMember mem1 = new NormalMember("Special Rate for Special Mits", "Leo Mazz", 1, 2014);
            VIPMember vm1 = new VIPMember("King Meeta", 2, 2015);

            mem1.CalculateAnnualFee();
            vm1.CalculateAnnualFee();

            int totalMeetaFee = mem1.CalculateAnnualFee() + vm1.CalculateAnnualFee();

            //PolyMoprh

            //Array of members... very clever !

            Member[] clubMembers = new Member[5];

            clubMembers[0] = new NormalMember("Special Rate", "King", 2, 2019);
            clubMembers[1] = new NormalMember("Normal Rate", "Cal", 3, 2019);
            clubMembers[2] = new VIPMember("Smit", 0, 2013);

            

            foreach(Member m in clubMembers)
            {
                m.CalculateAnnualFee();
            }
            

            //GET TYPE (determining which class each instance is ! helpful!!

            if (clubMembers[0].GetType() == typeof(VIPMember))
                Console.WriteLine("VIP");
            else
                Console.WriteLine("Normal");

            
            //For some reason, still having an issue with foreach here! Why?
            //More logical version:
            //foreach(Member m in clubMembers)
            //{
             //   if (m.GetType() == typeof(VIPMember))
             //       Console.WriteLine("VIP");
              //  else
               //     Console.WriteLine("Normal");
            //}
            

            // ~~~~~~~~~~ LINQ ~~~~~~~~~~~~~~ //

            int[] numbers = { 0, 1, 2, 3, 4, 5 };
            //This is assigned to a variable, evenNumQuery , for use later!
            var evenNumQuery =
                from num in numbers
                where num % 2 == 0
                select num;



            foreach (int i in evenNumQuery)
            {
                Console.WriteLine(i);
            }

            Console.WriteLine("WOW I cant believe LINQ is this fucking simple !");
            Console.WriteLine("UN FUCKING BELIEVABLY COOL ");


            //INSANE class based list query !! crazy fucking cool !
            List<Customers> customers = new List<Customers>();
            customers.Add(new Customers("Leo", 3, "Chicago", 0));
            customers.Add(new Customers("King", 4, "NYC", 1000));
            customers.Add(new Customers("Cal", 4, "NYC", 1000));

            */
            /* Errors in here...
            //Rewrite AND REMEMBER IT USES FROM !! FROM   -- NOT for.
            var query =
                from c in customers
                where c.Balance > 0
                orderby c.Balance
                select new { c.Name, c.Balance };


            foreach (var c in query)
            {
                Console.WriteLine("Name: {0}, Balance {1}", c.Name, c.Balance);
            }

            */

            // IOSTREAM // Writefile // Readfile streamreading !

            //
            /*
            using(StreamReader sr = new StreamReader(path))
            {
                while(sr.EndOfStream != true)
                {
                    Console.WriteLine(sr.ReadLine());
                }
                sr.Close();
            }

            //Option 1 (better w/ exception / error handling !

            try
            {
                using (StreamReader sr = new StreamReader(path))
                {
                    while(sr.EndOfStream != true)
                    {
                        Console.WriteLine(sr.ReadLine());
                    }
                    sr.Close();
                }
            }
            catch (FileNotFoundException e)
            {
                Console.WriteLine(e.Message);
            }

            //Option 2 (With an if/else and exits()
            */
            string path = "\\myfile.txt";

            if (File.Exists(path))
            {
                using (StreamReader sr = new StreamReader(path))
                {
                    while (sr.EndOfStream != true)
                    {
                        Console.WriteLine(sr.ReadLine());
                    }
                    sr.Close();
                }
            }
            else
            {
                Console.WriteLine("Path Not Found! Check your Path variable (Dumbass)");
            }


            /*
            Human h = new Human("Leo", 5);
            h.print_list();
            Human.add_to_list("Lauren");
            Human.add_to_list("Zach");
            h.print_list();
            Console.WriteLine(h.Age);
            Console.WriteLine(h.Name);
            */


            //CHAPTER 1 COOKBOOK

            int[] scores = { 12, 21, 24, 42, 35, 53 };
            Chapter1 ch1 = new Chapter1();
            var sss = ch1.GetAverageAndCount(scores);
            Console.WriteLine($"Average was {sss.Item1} across {sss.Item2} students.");
            //Console.ReadLine(); //What the fuck does this do?

            //Modified (overloaded)
            int threshold = 51;
            var (average, studentCount, belowAvg) = ch1.GetAverageAndCount(scores, threshold);
            Console.WriteLine($"Average was {average} across {studentCount} students.  " +
                $"{(average < threshold ? "Student Scores Below Average" : "Student Scores above Average")}"); //Inline if !! GENIUS !
            //Console.ReadLine();

            Student st = new Student();
            st.Name = "Dirk";
            st.LastName = "Diggler";
            st.CourseCodes = new List<int> { 101, 141, 201, 202, 342 };


            Professor pr = new Professor();
            pr.Name = "Myron";
            pr.LastName = "Scholes";
            pr.TeachesSubjects = new List<string> { "E202", "F305", "Economics", "Cryptography" };


            //ch1.OutputInfo(pr);
            //ch1.OutputInfo(st); THESE DON'T WORK !! thats okay, don't really get the fucking point anyway.


            //Made possible with the DECONSTRUCT method in student class!
            var (name, lastName) = st;
            Console.WriteLine($"Student is named {name} {lastName}");

            foreach (var i in s)
            {
                Console.WriteLine(i);
            }

            //STRUCTS -- kinda look like C++ classes (Although not working, says you can?)
            //Structure stru;
            var stru = new Structure();
            stru.Width = 10;
            stru.Depth = 20;
            double struct_var = stru.Width;
            double diag = stru.Diagonal;
            Console.WriteLine($"Testing out struct vars: Width = {struct_var} diagonal = {diag}");

            //Readonly structs
            var di = new Dimension(10, 30);
            //di.Width = 30; //Cannot do this -- readonly
            Console.WriteLine($"Testing out readonly structs: Width = {di.Width}, Diagonal = {di.Diagonal}");

            //Null types
            int x1 = 1;
            int? x2 = null;

            //Okay
            int? x3 = x1;

            //Reverse is not true, MUST BE CAST
            int x4 = (int)x3;

            //Exception if null, so can do this
            int x5 = x2.HasValue ? x2.Value : -1;
            //Can be shortened in syntax!!
            int x6 = x2 ?? -1;  //SAME AS ^^
            Console.WriteLine($"x1 = {x1} x2: {x2} x3: {x3} x4: {x4} x5: {x5} x6: {x6}");



            //ENUMS:
            /*
            public enum Color //By default, this would be int
            {
            Red = 1,
            Blue = 2,
            Green = 3
            }*/

            //Algo Class !

            List<double> prices = new List<double> { 99.95, 100.00, 99.95, 97.25, 96.55, 95.45 };
            List<double> plo = new List<double> { 101.01, 99.05, 100.1, 101.54, 102.44, 103.21 };



            Algo a = new Algo(500, 20, 100);
            a.CalcBB(prices);
            bool SpivHiResult = a.SPiv(prices, 'H', 4);
            bool SpivLoRes = a.SPiv(plo, 'L', 3);

            Console.WriteLine($"SPivH Result : {SpivHiResult}");
            Console.WriteLine($"SPivL Result : {SpivLoRes}");

            List<double> pminlo = new List<Double> { 101.01, 100.10, 101.05, 102.25 };
            bool sminl = a.SPiv(pminlo, 'L', 3);
            Console.WriteLine($"SPiv Min Lo : {sminl}");

            List<double> pbelowmin = new List<double> { 101.01, 100.1, 101.1 };
            //bool spivBelow = a.SPiv(pbelowmin, 'L', 4); Correct exception thrown!
            bool spivBelow2 = a.SPiv(pbelowmin, 'L', 2);
            Console.WriteLine($"SPivL JUST large enough -- test edge case : {spivBelow2}");

            bool spivl_shi = a.SPiv(prices, 'L', 4);
            Console.WriteLine($"SPivLow list with SPivHi -- should be False {spivl_shi}");

            //Test switch version
            bool spswitch = a.SPivSwitch(prices, 'H', 4);
            Console.WriteLine($"SPivH Switch Ver: {spswitch}");

            Console.WriteLine($"I am a C# Master Programmer ! Simply a matter of days.  Bow down to my newfound .Net Mastery!");

            double tr = Algo.ATR(prices, 5);
            Console.WriteLine($"ATR: {tr}");

            //double trTooLong = Algo.ATR(prices, 6);
            //Console.WriteLine($"This should be an exception: {trTooLong}");

            int[] ar = { 1, 2, 3 };

            //TYPECASTING AN ND ARRAY W/ NUMSHARP !! fucking FINALLY -- cannot do with lists... only arrays

            var nd = (Array)ar;
            //var sd = nd.std();
            //Console.WriteLine($"Stdev: {sd}");

            List<int> tl = new List<int> { 1, 2, 3 };
            //var ndl = (NDArray)tl; -- TYPECASTING DOES NOT WORK WITH LISTS !! must be array!

            //CAN do this however -- little more involved -- ToArray() (ALSO remember lists have the [] between type and name, unlike C++!
            int[] arrcast = tl.ToArray();
            //var ndcast = (NDArray)arrcast;
            //var stddev = ndcast.std();
            //Console.WriteLine($"Cast List -> Arr -> NDArr + numsharp: {stddev}");

            //Built in my own version:
            double[] darr = { 1.0, 2.0, 3.0, 4.0, 5.0, 6.0, 7.0 };
            List<double> dlst = new List<double> { 1.0, 2.0, 3.0, 4.0, 5.0, 6.0, 7.0 };
            var standard = Algo.STD(dlst);
            Console.WriteLine($"Static method of STD (my own builtin--making lists to ndarrs.std() ) -- {standard}"); //2?

            //Testing it, just because this seems wrong... ITS CORRECT!
           // var nd4 = (NDArray)dlst.ToArray();
            //var nd5 = nd4.std();
            //Console.WriteLine($"Manually done stdev: {nd5}");


            double tsclc = Algo.CalcTS(prices, -2); //NOT QUITE RIGHT -- not sure why... hmmm pretty DAMN close though.
            Console.WriteLine($"Trailstop: {tsclc}");


            for (int i = prices.Count() - 1; i > 0; i--)
            {
                var it = i;
            }
            var id = prices.Count() - 1;
            Console.WriteLine(id);
            Console.WriteLine(prices[id]);



            /* **BIG CONFUSING CAST THAT DOESNT WORK! REPLACED W/ TOARRAY() **
            foreach(var t in tl)
            {
                arrcast[k] = t;
                k++;

                }
            var ndcst = (NDArray)arrcast;
            var newstd = ndcst.std();
            Console.WriteLine($"New std: {newstd}");
            */


            //Pivot (50 us approx -- could cut this down!)

            var piv_res = Algo.Pivot(plo);
            Console.WriteLine($"Pivot result is: (for pBar) : {piv_res}"); //BINGO!

            var phi_res = Algo.Pivot(prices, 1);
            Console.WriteLine($"Pivot Hi result : {phi_res}"); //Bingo!  FUCKING WORKS !

            var ph_test = Algo.Piv_Helper(prices);
            Console.WriteLine($"PHelper: {ph_test}");
            var ph_t = Algo.Piv_Helper(plo);
            Console.WriteLine($"PHelper True Test: {ph_t}");

            // FUCKING TIMERS
            //FINAL

            Stopwatch sw = new Stopwatch();
            sw.Start();
            // Do something you want to time
            Algo.Pivot(prices, 1);
            sw.Stop();

            long microseconds = sw.ElapsedTicks / (Stopwatch.Frequency / (1000L * 1000L));
            //long nanoseconds = sw.ElapsedTicks / (Stopwatch.Frequency / (1000L * 1000L * 1000L));

            Console.WriteLine("Operation completed in: " + microseconds + " (us)");
            //Console.WriteLine("Operation completed in: " + nanoseconds + " (ns)");


            sw.Start();

            Algo.Pivot(plo);
            sw.Stop();

            long micros = sw.ElapsedTicks / (Stopwatch.Frequency / (1000L * 1000L));
            Console.WriteLine($"Operation completed in : {micros} (us)");

            sw.Start();
            a.SPiv(plo, 'L', 4);
            sw.Stop();
            micros = sw.ElapsedTicks / (Stopwatch.Frequency / (1000L * 1000L));
            Console.WriteLine($"SPIV completed in {micros} (us)"); //74 us

            sw.Start();
            Algo.SPivH(prices, 4);
            sw.Stop();
            micros = sw.ElapsedTicks / (Stopwatch.Frequency / (1000L * 1000L));
            Console.WriteLine($"SPIVH completed in {micros} (us)"); //70 us -- same

            sw.Start();
            a.hh_ll(prices);
            sw.Stop();
            micros = sw.ElapsedTicks / (Stopwatch.Frequency / (1000L * 1000L));
            //micros = sw.ElapsedTicks / (Stopwatch.Frequency / (1000 * 1000));
            Console.WriteLine($"HL completed in {micros} (us)"); // ATR: 80us // HL: 350us -- YIKES




            Stopwatch t = new Stopwatch();
            t.Start();
            //run
            t.Stop();
            micros = t.ElapsedTicks / (Stopwatch.Frequency / (1000 * 1000));  //NEED TO DEFINE AFTER EACH TIME! can't define micros ONE TIME,
            Console.WriteLine($"Elapsed time {micros} (us)");                 // each instance holds the recent interval


            //Deedle


            //let df1 = Frame(["first"; "second"], [first; second])
            Deedle d = new Deedle();

            d.CreateDeed();

            Deedle.AddRemCols();
            Deedle.CleanData();
            Deedle.LinqDF();
            Deedle.MergeDF();
            Deedle.ColOperations();
            Deedle.ILocs();

            /* REPLICATED IN DEEDLE.CS -- much more organized!

            var msftRaw = Frame.ReadCsv("/Users/zoakes/Downloads/MSFT_hist.csv");
            var fbRaw = Frame.ReadCsv("/Users/zoakes/Downloads/FB_hist.csv");

            //Clean data
            var msft = msftRaw.IndexRows<DateTime>("Date").SortRowsByKey();
            var fb = fbRaw.IndexRows<DateTime>("Date").SortRowsByKey();

            //Rename cols
            msft.RenameColumns(stt => "Msft" + stt);
            fb.RenameColumns(stt => "Fb" + stt);

            msft.Print();
            //Intersection of dates, combine!
            var joinIn = msft.Join(fb, JoinKind.Inner);

            //Get Columns:
            var msOpen = joinIn.GetColumn<double>("MsftOpen");
            var msClose = joinIn.GetColumn<double>("MsftPrice");
            //Test
            Console.WriteLine("MSFT Open DF:\n");
            msOpen.Print();
            //Operations
            Console.WriteLine("MSFT Diff DF:\n");
            var msDiff = msClose - msOpen;
            msDiff.Print();


            //ILocs
            var row = joinIn.Rows[new DateTime(2019, 9, 3)]; //Confirm the row is present obviously ! (only market days... not ANY day)

            Console.WriteLine("ILoc Result for 2019.9.3: \n");
            row.Print();

            var msLo = row.GetAs<double>("MsftLow");
            //msLo.Print(); cant print this?

            var msHi = row.Get("MsftHigh");

            var postAug = joinIn.Rows.Get(new DateTime(2019, 9, 1), Lookup.ExactOrGreater);
            postAug.Print(); //Doesnt seem to print much

            //Iloc (COLS, ROWS)
            var diff = joinIn["MsftPrice", new DateTime(2019, 9, 3)];
            Console.WriteLine($"Diff {diff}");


            //ADD COLUMN

            joinIn.AddColumn("MsftDiff", msDiff);

            //Create fb diff col
            var fbC = joinIn.GetColumn<double>("FbPrice");
            var fbO = joinIn.GetColumn<double>("FbOpen");
            var fbDiff = fbC - fbO;
            //Add!
            joinIn.AddColumn("FbDiff", fbDiff);


            // DROP COLUMN

            joinIn.DropColumn("MsftVol.");
            joinIn.DropColumn("FbVol.");
            joinIn.Print();


            var colRow = joinIn["MsftHigh",new DateTime(2019,9,20)];
            Console.WriteLine($"High: {colRow}");

            //Operations / Shift !

            var manual_Pct = msClose.Diff(1) / msClose * 100;
            manual_Pct.Print();

            //Test how accurate manually calculated pct change was -- NOT bad!
            var pct = joinIn.GetColumn<double>("MsftChange %");
            var auto_man = manual_Pct - pct;
            auto_man.Print();

            //var man_v_auto = manual_Pct - pct;
            //man_v_auto.Print();

            //Using LINQ: -- CREATING A NEW SERIES (rather than add column, broadcast into new SERIES!)
            var diffs = joinIn.Rows.Select(kvp => kvp.Value.GetAs<double>("MsftOpen") - kvp.Value.GetAs<double>("FbOpen"));

            //Bool filters!
            var msftGreaterRows = joinIn.Rows.Select(kvp => kvp.Value.GetAs<double>("MsftOpen") > kvp.Value.GetAs<double>("FbOpen"));

            //Put INTO NEW DATAFRAME!
            //var msftGtDf = Frame.FromRows<DateTime,Boolean>(msftGreaterRows); Cannot get this to work.. hmmm
            //var msftGreaterDf = Frame.FromRows(msftGreaterRows);
            //var exAugust = Frame.FromRows<DateTime,Double>(postAug);

            //var newDF = Frame.FromValues<DateTime,Boolean>(msftGreaterRows); none of these are working
            //var df = Frame.FromColumns<double>("MsftOpen");

            */


            //BEGINNING OF GAIN ALGO --

            Connect c = new Connect();
            c.get_acct_info();





        }
    }
}
