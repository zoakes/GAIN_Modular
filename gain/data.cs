using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
//using NumSharp;
using System.Diagnostics;
using System.Threading;
using System.Timers;
using Deedle;

using GF;
namespace cs_test
{
    public class Data : Connect
    {
        //GF.Api.IFGClient client;
        //Bar b; Where is this bar object?
        //int lookback = 3;
        //Properties -- where a field is NEEDED BY OTHER CLASSES
        GF.Api.Contracts.Lookup.SymbolLookupEventArgs e;
        GF.Api.Subscriptions.Bars.Bar b;
        Data d = new Data();

        List<double> O;
        List<double> H;
        List<double> L;
        List<double> C;

        List<GF.Api.Subscriptions.Bars.Bar> hist;                              //Make Property?

        private int lb;

        public int lookback
        {
            get { return lb; }
            //set { (value > 0) ? hWorked = value : hWorked = 0; } Cant do with properties it seems
            set
            {
                if (value > 0)
                {
                    lb = value;
                }
                else
                {
                    lb = 0;
                }
            }
        }

        public Data()  //Attempt at simplifying... not sure if working..
        {
            RegisterOnBarsReceived(gfClient);
            //GFClient_OnSymbolLookupReceived(gfClient, "ESU20");
            //GFClient_OnBarsReceived(gfClient,e); //E ? Arg?
            //Fill_Hist_Bars();



        }

        private static void RegisterOnBarsReceived(GF.Api.IGFClient client)
        {
            client.Subscriptions.Bars.BarsReceived += GFClient_OnBarsReceived;

        }

        private static void GFClient_OnSymbolLookupReceived(GF.Api.IGFClient client, GF.Api.Contracts.Lookup.SymbolLookupEventArgs e)
        {
            Data d = new Data();                                                //to get property -- must be an easier way?
            // Subscribe 10-min bars and loads previous 3 days of historical bars
            client.Subscriptions.Bars.Subscribe(
                e.Contracts.First().ID,
                client.Subscriptions.Bars.Duration.Create(DateTime.UtcNow.AddDays(-d.lookback)),
                client.Subscriptions.Bars.Description.CreateMinutes(10));
        }

        private static void GFClient_OnBarsReceived(GF.Api.IGFClient client, GF.Api.Subscriptions.Bars.BarsReceivedEventArgs e)
        {
            Console.WriteLine($"{e.Bars.Count} bars received for {e.Subscription.Contract.Symbol} {e.Subscription.Description.Type} {e.Subscription.Description.Interval}");
            foreach (var bar in e.Bars)
                DisplayBar(e.Subscription.Contract, bar);
        }

        public void Fill_Hist_Bars(GF.Api.IGFClient client, GF.Api.Subscriptions.Bars.BarsReceivedEventArgs e)
        {

            //Data d = new Data();
            Console.WriteLine("Getting past (lb) bars...");  //CHECK WHAT ORDER THEY ARE ADDED...
            foreach (var bar in e.Bars)
            {
                d.hist.Add(bar);
            }
            Console.WriteLine("historical bars populated");
        }



        public List<GF.Api.Subscriptions.Bars.Bar> Get_Hist_Bars()
        {
            Console.WriteLine("Fetching historical bars -- List of Bar Objects.");

            return d.hist;
        }



        public void OnBar(GF.Api.IGFClient client, GF.Api.Subscriptions.Bars.BarsReceivedEventArgs e)
        {
            foreach (var bar in e.Bars)
                Fill_OHLC(e.Subscription.Contract, bar);

        }

        public void Fill_OHLC(GF.Api.Contracts.IContract contract, GF.Api.Subscriptions.Bars.Bar bar)
        {

            L.Add(bar.Low);
            H.Add(bar.High);
            O.Add(bar.Open);
            C.Add(bar.Close);
            Console.WriteLine("OHLC Lists Filled");


        }




        private static void DisplayBar(GF.Api.Contracts.IContract contract, GF.Api.Subscriptions.Bars.Bar bar)
        {
            Console.WriteLine(
                "{0} {1} O:{2} H:{3} L:{4} C:{5} Vol:{6} Ticks:{7} UpVol:{8} DownVol:{9}",
                contract.Symbol,
                bar.OpenTimestamp.ToLocalTime(),
                contract.PriceToString(bar.Open),
                contract.PriceToString(bar.High),
                contract.PriceToString(bar.Low),
                contract.PriceToString(bar.Close),
                bar.Volume,
                bar.Ticks,
                bar.UpVolume,
                bar.DownVolume);
        }
    }
}
