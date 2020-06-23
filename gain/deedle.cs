using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
//using NumSharp;
using System.Diagnostics;
using Deedle;
using FSharp;

namespace cs_test
{


    //REQUIRES NUGET FSHARP.CORE !!
    public class Deedle
    {
        public string root = "/Users/zoakes/Downloads/SPY_historical.csv";

        public object msftRaw = Frame.ReadCsv("/Users/zoakes/Downloads/MSFT_hist.csv");
        public object fbRaw = Frame.ReadCsv("/Users/zoakes/Downloads/FB_hist.csv");

        //public Frame<DateTime,double> msft = new Frame.ReadCsv("/Users/zoakes/Downloads/MSFT_hist.csv"); doesnt work this way either FUCK

        //var spy = Frame.ReadCsv(root).GroupRowsBy<object>("Date");



        public Deedle()
        {

            var spy = Frame.ReadCsv(Path.Combine(root)).GroupRowsBy<DateTime>("Date"); //THIS ISN"T WORKINg -- no idea why REQUIRED FSHARP.CORE PACKAGE!
            //var msftRaw = Frame.ReadCsv("/Users/zoakes/Downloads/MSFT_hist.csv");
            //var fbRaw = Frame.ReadCsv("/Users/zoakes/Downloads/FB_hist.csv");
            var byClass = spy.GetColumn<double>("close");
            spy.Print();
            Console.WriteLine($"Deedle Dee created!");
        }



        public void CreateDeed()
        {
            var rand = new Random();
            var obj = Enumerable.Range(0, 10).Select(i => new { Key = "ID_" + i.ToString(), Number = rand.Next() });

            var dfObjects = Frame.FromRecords(obj);
            dfObjects.Print();

        }


        public void GenDeedle()
        {
            var rows = Enumerable.Range(0, 100).Select(i =>
            {
                var sb = new SeriesBuilder<string>();
                sb.Add("Index", i);
                sb.Add("Sin", Math.Sin(i / 100.0));
                sb.Add("Cos", Math.Cos(i / 100.0));
                return KeyValue.Create(i, sb.Series);
            });
        }



        public static void CleanData()
        {
            var msftRaw = Frame.ReadCsv("/Users/zoakes/Downloads/MSFT_hist.csv");
            var fbRaw = Frame.ReadCsv("/Users/zoakes/Downloads/FB_hist.csv");
            //Clean data
            var msft = msftRaw.IndexRows<DateTime>("Date").SortRowsByKey();
            var fb = fbRaw.IndexRows<DateTime>("Date").SortRowsByKey();

            //Rename cols
            msft.RenameColumns(stt => "Msft" + stt);
            fb.RenameColumns(stt => "Fb" + stt);

            msft.Print();
            fb.Print();
        }


        public static void MergeDF()
        {
            //SAME SHIT FROM CLEAN, cannot make it global -- dynamic typed
            var msftRaw = Frame.ReadCsv("/Users/zoakes/Downloads/MSFT_hist.csv");
            var fbRaw = Frame.ReadCsv("/Users/zoakes/Downloads/FB_hist.csv");
            //Clean data
            var msft = msftRaw.IndexRows<DateTime>("Date").SortRowsByKey();
            var fb = fbRaw.IndexRows<DateTime>("Date").SortRowsByKey();

            //Rename cols
            msft.RenameColumns(stt => "Msft" + stt);
            fb.RenameColumns(stt => "Fb" + stt);

            // THIS IS THE IMPORTANT PART!
            //Intersection of dates, combine!
            var joinIn = msft.Join(fb, JoinKind.Inner);
            joinIn.Print();

        }



        public static void ColOperations()
        {
            //Old BS
            var msftRaw = Frame.ReadCsv("/Users/zoakes/Downloads/MSFT_hist.csv");
            var fbRaw = Frame.ReadCsv("/Users/zoakes/Downloads/FB_hist.csv");
            var msft = msftRaw.IndexRows<DateTime>("Date").SortRowsByKey();
            var fb = fbRaw.IndexRows<DateTime>("Date").SortRowsByKey();
            msft.RenameColumns(stt => "Msft" + stt);
            fb.RenameColumns(stt => "Fb" + stt);
            var joinIn = msft.Join(fb, JoinKind.Inner);
            //End Old stuff -- DF's created:


            //Get Columns:
            var msOpen = joinIn.GetColumn<double>("MsftOpen");
            var msClose = joinIn.GetColumn<double>("MsftPrice");
            //Test
            var msDiff = msClose - msOpen;
            Console.WriteLine("MSFT Open DF:\n");
            msOpen.Print();

            //Operations // SHIFT
            var manual_Pct = msClose.Diff(1) / msClose * 100;
            manual_Pct.Print();

            //Test how accurate manually calculated pct change was -- NOT bad!
            var pct = joinIn.GetColumn<double>("MsftChange %");
            var auto_man = manual_Pct - pct;
            auto_man.Print();

            //Round -- notice the INPUT OF HOW SERIES IS USED AS ARG !!
            var round = msft.ColumnApply((Series<DateTime, double> numeric) =>
                        numeric.Select(kvp => Math.Round(kvp.Value, 2)));

        }


        public static void ILocs()
        {
            var msftRaw = Frame.ReadCsv("/Users/zoakes/Downloads/MSFT_hist.csv");
            var msft = msftRaw.IndexRows<DateTime>("Date").SortRowsByKey();
            msft.RenameColumns(stt => "Msft" + stt);


            var row = msft.Rows[new DateTime(2019, 9, 3)]; //Confirm the row is present obviously ! (only market days... not ANY day)
            Console.WriteLine("ILoc Result for 2019.9.3: \n");
            row.Print();

            var msLo = row.GetAs<double>("MsftLow");  //Row operation
            //cant print this?
            var msHi = row.Get("MsftHigh"); //Row operation

            var postAug = msft.Rows.Get(new DateTime(2019, 9, 1), Lookup.ExactOrGreater);
            postAug.Print(); //Doesnt seem to print much

            //Iloc (COLS, ROWS)
            var colRow = msft["MsftHigh", new DateTime(2019, 9, 20)];
            Console.WriteLine($"High: {colRow}");

            var diff = msft["MsftPrice", new DateTime(2019, 9, 3)];
            Console.WriteLine($"Diff {diff}");

        }



        public static void AddRemCols()
        {
            var msftRaw = Frame.ReadCsv("/Users/zoakes/Downloads/MSFT_hist.csv");
            var fbRaw = Frame.ReadCsv("/Users/zoakes/Downloads/FB_hist.csv");
            var msft = msftRaw.IndexRows<DateTime>("Date").SortRowsByKey();
            var fb = fbRaw.IndexRows<DateTime>("Date").SortRowsByKey();
            msft.RenameColumns(stt => "Msft" + stt);
            fb.RenameColumns(stt => "Fb" + stt);


            var msOpen = msft.GetColumn<double>("MsftOpen");
            var msClose = msft.GetColumn<double>("MsftPrice");
            //Test
            var msDiff = msClose - msOpen;

            //ADD COLUMN
            msft.AddColumn("MsftDiff", msDiff);

            //Create fb diff col
            var fbC = fb.GetColumn<double>("FbPrice");
            var fbO = fb.GetColumn<double>("FbOpen");
            var fbDiff = fbC - fbO;
            //Add!
            fb.AddColumn("FbDiff", fbDiff);


            // DROP COLUMN

            msft.DropColumn("MsftVol.");
            fb.DropColumn("FbVol.");
            fb.Print();
            msft.Print();
        }

        public static void LinqDF()
        {
            var msftRaw = Frame.ReadCsv("/Users/zoakes/Downloads/MSFT_hist.csv");
            var fbRaw = Frame.ReadCsv("/Users/zoakes/Downloads/FB_hist.csv");
            var msft = msftRaw.IndexRows<DateTime>("Date").SortRowsByKey();
            var fb = fbRaw.IndexRows<DateTime>("Date").SortRowsByKey();
            msft.RenameColumns(stt => "Msft" + stt);
            fb.RenameColumns(stt => "Fb" + stt);

            var joinIn = msft.Join(fb, JoinKind.Inner);


            //Using LINQ: -- CREATING A NEW SERIES (rather than add column, broadcast into new SERIES!)
            var diffs = joinIn.Rows.Select(kvp => kvp.Value.GetAs<double>("MsftOpen") - kvp.Value.GetAs<double>("FbOpen"));

            //Bool filters!
            var msftGreaterRows = joinIn.Rows.Select(kvp => kvp.Value.GetAs<double>("MsftOpen") > kvp.Value.GetAs<double>("FbOpen"));

            msftGreaterRows.Print();
            //SHOULD create new df -- not working though : /
            //var msftGtDf = Frame.FromRows<DateTime,Boolean>(msftGreaterRows); //Cannot get this to work.. hmmm
            //msftGtDf.Print();

        }




        public void AllInOneTimeSeries()
        {
            //var msftRaw = Frame.ReadCsv(Path.Combine(root, "../data/stocks/msft.csv"));
            //var fbRaw = Frame.ReadCsv(Path.Combine(root, "../data/stocks/fb.csv"));
            var msftRaw = Frame.ReadCsv("/Users/zoakes/Downloads/MSFT_hist.csv");
            var fbRaw = Frame.ReadCsv("/Users/zoakes/Downloads/FB_hist.csv");


            //Clean data
            var msft = msftRaw.IndexRows<DateTime>("Date").SortRowsByKey();
            var fb = fbRaw.IndexRows<DateTime>("Date").SortRowsByKey();

            //Rename cols
            msft.RenameColumns(s => "Msft" + s);
            msft.RenameColumns(s => "Fb" + s);

            //Join dataframes
            //Intersection of dates, combine!
            var joinIn = msft.Join(fb, JoinKind.Inner);
            //Take union, fill missing
            var joinOut = msft.Join(fb, JoinKind.Outer);

            //Shift values / edit values
            var msftShift = msft.SelectRowKeys(k => k.Key.AddHours(1.0));
            //Values don't match now, so cannot inner join:

            //This WILL work...
            var joinLeft = fb.Join(msftShift, JoinKind.Left, Lookup.ExactOrSmaller);
            joinLeft.Print();

            //Operations

            var msOpen = joinIn.GetColumn<double>("MsftOpen");
            var msClose = joinIn.GetColumn<double>("MsftClose");
            var msDiff = msClose - msOpen;

            //Similiar to iloc:

            var row = joinIn.Rows[new DateTime(2013, 1, 4)];    //Rows[]
            var msLo = row.GetAs<double>("MsftLow");
            var msHi = row.GetAs<double>("MsftHigh");

            //Other ex

            var postJan = joinIn.Rows.Get(new DateTime(2013, 1, 1), Lookup.ExactOrGreater); // Rows.Get()

            //Double digit iloc (row and col)
            var diff = joinIn["msDiff", new DateTime(2013, 1, 4)]; //Column, Row format (BACKWARDS from python I believe!!

            //LINQ w dataframes... LATER

            //Calculations
            var msft_ret = msft.Diff(1) / msft * 100;
            //Round
            var round = msft_ret.ColumnApply((Series<DateTime, double> numeric) =>
                        numeric.Select(kvp => Math.Round(kvp.Value, 2)));


        }


    }






}
