using static System.Console;
using System.Linq;
using System.Collections.Generic;
using NumSharp;
using System;







public class Chapter1
{
    public (int, int) GetAverageAndCount(int[] scores)
    {
        var returnTuple = (0, 0);
        return returnTuple;
    }

    public (double average, int studentCt, bool belowAvg) GetAverageAndCount(int[] scores, int threshold)
    {
        var returnTuple = (avg: 0, sCount: 0, subAvg: true);
        returnTuple = (scores.Sum() / scores.Count(), scores.Count(), returnTuple.avg.CheckIfBelowAverage(threshold));
        return returnTuple;
    }

    public void OutputInfo(object person)
    {
        if (person is Student)
        {
            Student student = (Student)person;
            WriteLine("Student {student.Name} {student.LastName} is enrolled in courses for {String.Join<int>(", ", student.CourseCodes)}");

        }
        if (person is Professor)
        {
            Professor pr = (Professor)person;
            WriteLine("Professor {pr.Name} {pr.LastName} is teached courses in {String.Join<string>(", ", pr.TeachesSubjects)}");

        }

        if (person is null)
        {
            WriteLine("Object {nameof(person)} is null");
        }
    }

    public void OutputInfoSwitch(object person)
    {
        switch (person)
        {
            case Student st when (st.CourseCodes.Contains(203)):                //Need to be CAREFUL OF THE ORDER OF THESE SWITCH STATEMENTS, inner like this must be first
                WriteLine("Student {st.Name} {st.LastName} is taking 203!");    //Otherwise -- THIS WILL NEVER BE CALLED IF AFTER STUDENT (as all students are students)
                break;
            case Student st:
                WriteLine("Student {st.Name} {st.LastName} is taking courses {String.Join<int>(", ",st.CourseCodes)}");
                break;
            case Professor pr:
                WriteLine("Professor {ps.Name} {pr.LastName} is teaching courses in {String.Join<string>(", ",pr.TeachesSubjects)}");
                break;
            case null:
                WriteLine("Object {nameof(person)} is null");
                break;
            default:
                WriteLine("Object {nameof(person)} is null");
                break;
        }
    }

}

public static class ExtensionMethods
{
    public static bool CheckIfBelowAverage(this int classAverage, int threshold)
    {
        if (classAverage < threshold)
        {
            // Notify head of department
            return true;
        }
        else
        {
            return false;
        }
    }

    public static void Deconstruct(this Student student,
         out string firstItem, out string secondItem)
    {
        firstItem = student.Name;
        secondItem = student.LastName;
    }


}


public class Student
{
    public string Name { get; set; }
    public string LastName { get; set; }
    public List<int> CourseCodes { get; set; } //= new List<int> {}; NOT NEEDED

    public void Deconstruct(out string name, out string lastName)
    {
        name = Name;
        lastName = LastName;
        /*
         * This can now be ran like this:
         * var (FirstName, Surname) = student;
         */
    }

}

public class Professor
{
    public string Name { get; set; } = "MotherFucker";   //Can initialize properties to defaults ! Like MF JOnes
    public string LastName { get; set; } = "Jones";
    public List<string> TeachesSubjects { get; set; } //Requires System.Collections.Generic tag

}


//STRUCT is identical to CLASSES, just different usage slightly -- no polymorph

public struct Structure
{
    public string Name { get; set; }
    public double Width { get; set; }
    public double Height { get; set; }
    public double Depth { get; set; }

    public Structure(string nm, double wd, double ht, double dp)
    {
        Name = nm;
        Width = wd;
        Height = ht;
        Depth = dp;

    }

    public double Diagonal => Math.Sqrt(Width * Width + Depth * Depth);


}



public readonly struct Dimension
{
    public double Length { get; }  //IF READONLY -- CANNOT USE SETTER! BECAUSE YOU CANT CHG READONLY OBVIOUSLY
    public double Width { get; }

    public Dimension(double l, double w)
    {
        Length = l;
        Width = w;
    }

    public double Diagonal => Math.Sqrt(Width * Width + Length * Length);

}

class Algo
{
    public double CatStop { get; set; } = 500.00;
    public double TrailAmt { get; set; } = 18.00;
    public double MinTgt { get; set; } = 100.00;
    public double Stdevs { get; set; } = 1;
    public double xb { get; set; } = 3.0;
    public double param { get; set; } = 0.0;

    public Algo(double cs, double trail, double trailtgt)
    {
        CatStop = cs;
        TrailAmt = trail;
        MinTgt = trailtgt;
        Console.WriteLine($"Algo Class instance created with Stop: {CatStop} Trailstop {MinTgt} at {TrailAmt} % ");
    }

    public static double GetTrailTrigger(ref Algo a, double entryPrice)
    {
        double res = entryPrice + a.MinTgt;
        return res;
    }

    public (double, double) CalcBB(List<double> ClosePrices)
    {
        double mid = ClosePrices.Average(); //Slighly wrong, but it works for now.  Need TA/TALIB / QC
        double ubb = mid + StdDev(ClosePrices) * Stdevs;
        double lbb = mid - StdDev(ClosePrices) * Stdevs;
        return (ubb, lbb);
    }

    private double StdDev(List<double> values)
    {
        double ret = 0;
        if (values.Count() > 0)
        {
            //Compute the Average
            double avg = values.Average();
            //Perform the Sum of (value-avg)_2_2
            double sum = values.Sum(d => Math.Pow(d - avg, 2));
            //Put it all together
            ret = Math.Sqrt((sum) / (values.Count() - 1));
        }
        return ret;
    }

    public (bool, bool) BBSignal(List<double> ClosePrices)
    {
        var (ubb, lbb) = CalcBB(ClosePrices);
        bool above = ClosePrices[0] > ubb ? true : false;
        bool below = ClosePrices[0] < lbb ? true : false;
        return (above, below);
    }

    public double PSAR(List<double> ClosePrices)
    {
        //TA.Lib.Core.SMA()
        //QuantConnect.Indicators.PSAR()
        return 0.0;
    }

    public (bool, bool) hh_ll(List<double> ClosePrices)
    {
        bool hh = false;
        bool ll = false;
        if (xb is 3.0)
        {
            if (ClosePrices[0] > ClosePrices[1] & ClosePrices[1] > ClosePrices[2] & ClosePrices[2] > ClosePrices[3])
            {
                hh = true;
            }
            else if (ClosePrices[0] < ClosePrices[1] & ClosePrices[1] < ClosePrices[2] & ClosePrices[2] < ClosePrices[3])
            {
                ll = true;
            }
            return (hh, ll);

        }
        else
        {
            Console.WriteLine("Need to update hh_ll method -- only build out for xb 3");
            return (false, false);
        }

    }

    public bool SPiv(List<double> ClosePrices, Char LowOrHi, int bb)
    {
        bool sl, sh = false;
        if (ClosePrices.Count() < bb)
        {
            System.ArgumentException argx = new System.ArgumentException("Please set BB number less than length of price list");
            throw argx;
        }
        if (LowOrHi == 'L')
        {
            sl = SPivL(ClosePrices, bb);
            return sl;
        }
        else if (LowOrHi == 'H')
        {
            sh = SPivH(ClosePrices, bb);
            return sh;
        }
        else
        {
            System.ArgumentException argex = new System.ArgumentException("Plese set LowOrHi value to L or H !");
            throw argex;
        }

    }

    public bool SPivSwitch(List<double> ClosePrices, char LowOrHi, int bb)
    {
        bool sl, sh = false;
        if (ClosePrices.Count() < bb)
        {
            System.ArgumentException agx = new System.ArgumentException("Please set BB < length of PriceList");
            throw agx;
        }
        switch (LowOrHi)
        {
            case 'L':
                sl = SPivL(ClosePrices, bb);
                return sl;
            case 'H':
                sh = SPivH(ClosePrices, bb);
                return sh;
            default:
                Console.WriteLine("Please select 'L' or 'H' for LowOrHi value");
                return false;
        }

    }


    public static bool SPivL(List<double> ClosePrices, int bb)
    {
        bool res = false;
        if (ClosePrices[0] > ClosePrices[1])
        {
            for (int i = 1; i < bb; i++)
            {
                if (ClosePrices[i] > ClosePrices[i + 1])
                {
                    break;
                }
                else
                {
                    res = true;
                }
            }
        }
        return res;
    }

    public static bool SPivH(List<double> ClosePrices, int bb)
    {
        bool res = false;
        if (ClosePrices[0] < ClosePrices[1])
        {
            for (int i = 1; i < bb; i++)
            {
                if (ClosePrices[i] < ClosePrices[i + 1])
                {
                    break;
                }
                else
                {
                    res = true;
                }
            }
        }
        return res;
    }

    //public static void Assert(bool condition, string message); Why doesnt this fucking thing work?
    public static double STD(List<double> ClosePrices)
    ///Hopefully can figure out Numsharp or Numpy? -- Done, but cannot figure out casting!
    {
        double[] arr = ClosePrices.ToArray();
        var nd = (NDArray)arr;
        return nd.std();

    }

    public static double ATR(List<double> ClosePrices, int aLen)
    {
        double diffSum = 0.0;
        if (aLen >= ClosePrices.Count())
        {
            System.ArgumentException ax = new System.ArgumentException($"Atr Length must be less than number of prices; Alen: {aLen}, Prices {ClosePrices.Count()}");
            throw ax;
        }
        for (int i = 0; i < aLen; i++)
        {
            diffSum += Math.Abs(ClosePrices[i + 1] - ClosePrices[i]);
        }
        double tr = diffSum / aLen;
        return tr;
    }

    public static bool TrailStop(List<double> ClosePrices, double entryPrice, double pct = .20, double tgt = 100)
    {
        //Writing would require Pandas.. which is possible, but I don't know how ! MAYBE use a FSM, this is kind of a shitty one already
        var trigger = entryPrice + tgt;
        bool trig = false;
        bool ts = false;
        foreach (var c in ClosePrices)
        {
            var profit = c - entryPrice;
            //List of profits if trigger hit
            List<double> plst = new List<double> { };
            if (trig) { plst.Add(profit); }
            if (profit >= tgt) //Trigger hit
            {
                trig = true;
                //ts = true;


            }
            if (c <= .8 * plst.Max())
            {
                ts = true;
            }
        }
        double lstp = 0.0;
        double diff = tgt * pct; //20 initially
        //If new high, and trigger met, kick up the low stop.
        if (ClosePrices[0] > ClosePrices[1] & trig)
        {
            trigger = ClosePrices[0]; //if new high, set new trigger
            diff = (trigger - entryPrice) * pct; //reset
            lstp = trigger - diff;
        }
        return ts;
    }

    public static double CalcTS(List<double> ClosePrices, double entryPrice, double pct = .2, double tgt = 100)
    {
        var trig = entryPrice + tgt;
        var lst = entryPrice + (tgt * (1 - pct)); //entry + .8 of difference -- only active if hi
        bool hi = false;
        bool lo = false;
        for (int i = ClosePrices.Count() - 1; i > 0; i--) //loop backwards, in order of P&L trajectory.
        {

            if (ClosePrices[i] < ClosePrices[i - 1]) //if incr close prices (-1 goes further along rev loop!)
            {
                var profit = ClosePrices[i] - entryPrice;
                if (profit > tgt) { hi = true; }
                if (hi) { lst = entryPrice + (ClosePrices[i - 1] - entryPrice) * (1 - pct); }
            }

            if (hi & ClosePrices[i] > ClosePrices[i - 1]) // decr + tgt hit, LOOK for lstp
            {
                if (ClosePrices[i - 1] < lst) { lo = true; }
                if (lo) { Console.WriteLine($"Trailstop lo met lst: {lst} Close: {ClosePrices[i + 1]}"); }
            }
        }
        return lst;

    }


    public static bool hl(List<double> ClosePrices, char HorL, int bb)
    {
        switch (HorL)
        {
            case 'H':
                for (int i = 0; i < bb; i++)
                {
                    if (ClosePrices[i] < ClosePrices[i + 1])
                    {
                        return false;
                    }
                }
                return true;
            case 'L':
                for (int i = 0; i < bb; i++)
                {
                    if (ClosePrices[i] > ClosePrices[i + 1])
                    {
                        return false;
                    }
                }
                return true;
            default:
                Console.WriteLine("Error -- Please Select H or L for HorL input!");
                return false;
        }

    }

    //FYI in c# THIS HAS TO USE + VALUES, NOT NEGATIVES ! cannot index with negs ! (bc this is lists, not DF)
    //FINAL -- this version works 100 % : )
    public static (int, double) Pivot(List<double> ClosePrices, int HiLo = -1, int LSTR = 4, int RSTR = 1, int PIV_LEN = 5, int inst = 1)
    {
        if (PIV_LEN > RSTR + 1) { PIV_LEN = RSTR + 1; }  //Make sure PIV_LEN not too large
        var instCtr = 0;
        var instTest = false;
        var lenCtr = RSTR;
        //var piv = -1;
        var pBar = -1;
        var pPrice = -1.0;
        //Left Count
        while (lenCtr < PIV_LEN & !(instTest))
        {
            //HiLo 1 = Hi, -1 = Low
            var candPrice = ClosePrices[lenCtr];
            var pivTest = true;
            var strCtr = lenCtr + 1;
            while (pivTest & (strCtr - lenCtr <= LSTR))
            {
                if ((HiLo == 1 & candPrice < ClosePrices[strCtr]) | (HiLo == -1 & candPrice > ClosePrices[strCtr]))
                {
                    pivTest = false;
                }
                else
                {
                    strCtr++; //Increment to keep trying to disprove
                }
            }
            //If Survived left comparison, check RIGHT comparison (only 1 -- could make this simple: if ClosePrices[0] > ClosePrices[1]
            strCtr = lenCtr - 1;
            while (pivTest & lenCtr - strCtr <= RSTR)
            {
                if ((HiLo == 1 & candPrice <= ClosePrices[strCtr]) | (HiLo == -1 & candPrice >= ClosePrices[strCtr]))
                {
                    pivTest = false;
                }
                else
                {
                    strCtr--; //Decrement to move right (closer to current)
                }
            }
            //{ IF CandidatePrice SURVIVED OVERALL AS Pivot, INCREMENT InstanceCntr: }
            if (pivTest) { instCtr++; }
            if (instCtr == inst) { instTest = true; } //If reached correct instance, THIS is desired pivot!
            else { lenCtr++; } //Else -- incr len to keep LOOKING for next candPrice!
            if (instTest)
            {
                pPrice = candPrice;
                pBar = lenCtr; //+ RSTR; // CONFIRM THIS IS CORRECT!
                //piv = 1;
            }
            return (pBar, pPrice);
        }
        return (-1, -1.0);

    }


    public static bool Piv_Helper(List<double> ClosePrices, int HiLo = -1)
    {
        var res = Algo.Pivot(ClosePrices, HiLo);
        if (res != (-1, -1.0))
            return true;
        return false;
    }




}
