using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
//using NumSharp;
using System.Diagnostics;
using System.Threading;
using System.Timers;
using Deedle;

//using System.Datetime;

using GF;
using GF.Api.Orders.Drafts;
using GF.Api.Orders.Drafts.Validation;
using GF.Api.Values.Orders;
using GF.Api.Orders;

namespace cs_test
{




    public class Connect
    {
        public GF.Api.IGFClient gfClient; // = GF.Api.Impl.GFApi.CreateClient();
        public GF.Api.Threading.GFClientRunner runner;                                 //ptr to runner?
        public GF.Api.Accounts.IAccount account;
        public int port;

        //Change to properties...
        double trail_tgt;
        double trail_dec;
        double fixed_stop;
        Dictionary<string, bool> trail_on;
        Dictionary<string, double> hi_pnl;
        Dictionary<string, double> trigger_pnl;


        public Connect(int p = 9200)
        {
            
            gfClient = GF.Api.Impl.GFApi.CreateClient();                        //Removed declaration stuff (type, object -- etc)
            account = gfClient.Accounts.Get().FirstOrDefault();
            runner = new GF.Api.Threading.GFClientRunner(gfClient);             //declared in class def
            runner.Start();
            Console.WriteLine("Runner (HB) started.");
            port = p;

            Console.WriteLine("Connecting...");

        }

        //Is this account ID?
        private string generate_UUID()
        {
            Console.WriteLine("Temporary Value -- Generate UUID later.");
            return "9e61a8bc-0a31-4542-ad85-33ebab0e4e86";
        }

        private void connect_to_gain(string username = "username", string password = "pw")
        {
            gfClient.Connection.Aggregate.LoginCompleted += (client, e) => Console.WriteLine("Connection complete");
            gfClient.Connection.Aggregate.LoginFailed += (client, e) => Console.WriteLine($"Connection failed: {e.FailReason}");
            gfClient.Connection.Aggregate.Disconnected += (client, e) => Console.WriteLine($"Disconnected: {e.Message}");
            gfClient.Logging.ErrorOccurred += (client, e) => Console.WriteLine($"{e.Exception.Message}");

            string UID = generate_UUID();
            gfClient.Connection.Aggregate.Connect(
            new GF.Api.Connection.ConnectionContextBuilder()
                .WithUserName(username)
                .WithPassword(password)
                .WithUUID(UID)
                .WithPort(port)
                .WithHost("api.gainfutures.com")
                .WithForceLogin(true)
                .Build());


        }

        public void get_acct_info()
        {
            var timer = new System.Timers.Timer { Interval = TimeSpan.FromSeconds(2).TotalMilliseconds };
            timer.Elapsed += (_, __) =>
            {
                // The timer callback is on a different thread than the GFClientRunner, so we must delegate to the runner thread
                gfClient.Threading.Invoke(() =>
                {
                    if (!gfClient.Connection.Aggregate.IsConnected)
                        return;

                    var account = gfClient.Accounts.Get().First();
                    GF.Api.Balances.IBalance totalBalance = account.TotalBalance;

                    Console.WriteLine($"Account: {account.Spec}");
                    Console.WriteLine($"\tNetLiq: {totalBalance.NetLiq:c}");
                    Console.WriteLine($"\tCash: {totalBalance.Cash:c}");
                    Console.WriteLine($"\tOpen P/L: {totalBalance.OpenPnL:c}");
                    Console.WriteLine($"\tTotal P/L: {totalBalance.RealizedPnL + totalBalance.OpenPnL:c}");
                    Console.WriteLine($"\tInitial Margin: {totalBalance.InitialMargin:c}");
                    //Console.WriteLine($"\tNet Options Value: {totalBalance.LongCallOptionsValue + totalBalance.LongPutOptionsValue + totalBalance.ShortCallOptionsValue + totalBalance.ShortPutOptionsValue:c}");
                    Console.WriteLine($"Average Positions: {account.AvgPositions.Count}");
                    Console.WriteLine($"Orders: {gfClient.Orders.Get().Count}, last one: {(gfClient.Orders.Get().Count > 0 ? gfClient.Orders.Get().Last().ToString() : string.Empty)}");
                    Console.WriteLine();
                });
            };
        }

        /*Entry Logic...
        private static void GFClient_OnPriceTick(GF.Api.IGFClient client, GF.Api.Contracts.PriceChangedEventArgs e)
        {
            //TRADING LOGIC !! REPLACE WITH SHIT YOU WANT.
            if (Math.Abs(e.Price.LastPrice - e.Price.BidPrice) < e.Contract.TickSize)
                PlaceOrder(client, e.Contract, OrderSide.Buy, e.Price.BidPrice, "By Bid");
            else if (Math.Abs(e.Price.LastPrice - e.Price.AskPrice) < e.Contract.TickSize)
                PlaceOrder(client, e.Contract, OrderSide.Sell, e.Price.AskPrice, "By Ask");
        }
        */

        private static void PlaceOrder(GF.Api.IGFClient client, GF.Api.Contracts.IContract contract, GF.Api.Values.Orders.OrderSide orderSide, int qty, double limitPrice = 0.0, string comments = "")
        {

            //var qty = position.Net.Volume;
            //TWEAK THIS SO THAT IF LIMITPRICE = 0.0, MARKET ORDER!!!!
            if (client.Orders.Get().Count == 0 || client.Orders.Get().Last().IsFinalState)
            {
                OrderDraft orderDraft = new OrderDraftBuilder()
                    .WithAccountID(client.Accounts.Get().First().ID)
                    .WithContractID(contract.ID)
                    .WithSide(orderSide)
                    .WithOrderType(GF.Api.Values.Orders.OrderType.Market)
                    //.WithPrice(limitPrice)
                    .WithQuantity(1)
                    .WithEnd(DateTime.UtcNow.AddMinutes(1))
                    .WithComments(comments)
                    .Build();
                IReadOnlyList<OrderDraftValidationError> validationErrors = client.Orders.Drafts.Validate(orderDraft);
                if (validationErrors.Any())
                {
                    Console.WriteLine($"ERROR. Order {orderSide} {orderDraft.Quantity} {contract.Symbol} @ {contract.PriceToString(limitPrice)} Limit is invalid:");
                    foreach (var error in validationErrors)
                        Console.WriteLine($"\t{error.Message}");
                }
                else
                {
                    GF.Api.Orders.IOrder order = client.Orders.SendOrder(orderDraft);
                    Console.WriteLine($"Order {order} was sent");
                }
            }
        }

        

        //Trailstop // Catstop portion...

        private static void RegisterOnAvgPositionChanged(GF.Api.IGFClient client)
        {
            client.Accounts.AvgPositionChanged += GFClient_OnAvgPositionChanged;
        }

        private static void GFClient_OnAvgPositionChanged(GF.Api.IGFClient client, GF.Api.Positions.PositionChangedEventArgs e)
        {
            Console.WriteLine(
                "Average Position. {0}/{1}: Net Pos: {2} @ {3}, Bought: {4}, Sold {5}, Prev Pos: {6} P/L: {7:c}",
                e.Account.Spec, e.ContractPosition.Contract.Symbol,
                e.ContractPosition.Net.Volume, e.ContractPosition.Net.Price,
                e.ContractPosition.Long.Volume, e.ContractPosition.Short.Volume,
                e.ContractPosition.Prev.Volume,
                e.ContractPosition.OTE + e.ContractPosition.Gain);
        }


        //Helper function to simplify run_cat_trail function... -- BETTER version in closeposition logic...
        public int Go_Flat(GF.Api.Positions.PositionChangedEventArgs e)
        {

            var symbol = e.ContractPosition.Contract.Symbol;
            var net_basis = e.ContractPosition.Net.Volume;
            int _basis = net_basis > 0 ?  1  :  -1;

            var close_qty = Math.Abs(net_basis);
            Console.WriteLine($"Exitting Position in -- {symbol}");
            switch (_basis)
            {
                case -1:
                    PlaceOrder(gfClient, e.ContractPosition.Contract.ElectronicContract, OrderSide.BuyToCover, close_qty);
                    return 0;

                case 1:
                    PlaceOrder(gfClient, e.ContractPosition.Contract.ElectronicContract, OrderSide.Sell, close_qty);
                    return 0;

                default:
                    return -1;
            }
        }




        //Helper function for cat_Trail
        private double get_position_pnl(GF.Api.IGFClient client, GF.Api.Positions.PositionChangedEventArgs e)
        {
            //SEEMS LIKE IT NEEDS A LOOP... NO?
            //foreach(var pos in GF.Api.Positions....)

            return e.ContractPosition.OTE; //+ e.ContractPosition.Gain;  //Think Gain is Realized PNL, we want UNREALIZED only.
            //THINK from documentation that it's only OTE for OpenPNL
        }



        public int run_cat_trail(GF.Api.Positions.PositionChangedEventArgs e)
        {
            
            //var account = gfClient.Accounts.Get().FirstOrDefault();
            //Should be inside loop ? Called inside loop?
            //var positions = GetOpenPositions(account, e);                     //Just in case we need more detail / Safety...

            int ret = 0;
            foreach (var pos in e.AsArray())
            {
                ret = check_ts_cs(pos);
                switch (ret)
                {
                    case 0:
                        break;

                    case -1:
                        Console.WriteLine($"CatStop Triggered -- Exiting position in {e.ContractPosition.Contract.Symbol}");
                        break;

                    case 1:
                        Console.WriteLine($"TrailStop Triggered -- Exiting position in {e.ContractPosition.Contract.Symbol}");
                        break;

                    default:
                        Console.WriteLine($"Error -- Please check position in {e.ContractPosition.Contract.Symbol}");
                        break;
                }

                if (ret != 0)
                {
                    var qty = Math.Abs(e.ContractPosition.Net.Volume);
                    if (e.ContractPosition.Net.Volume < 0)
                    {
                        PlaceOrder(gfClient, e.ContractPosition.Contract.ElectronicContract, OrderSide.BuyToCover, qty);
                        Console.WriteLine("SX order sent.");
                    }
                    if (e.ContractPosition.Net.Volume > 0)
                    {
                        PlaceOrder(gfClient, e.ContractPosition.Contract.ElectronicContract, OrderSide.Sell, qty);
                        Console.WriteLine("LX order sent.");
                    }

                }
            }
            return ret;
        }



        //THIS SHOULD BE CALLED ON TICK MOST LIKELY... whereas the entries on BARS
        public int check_ts_cs(GF.Api.Positions.PositionChangedEventArgs e)
        {
            //Called inside of loop of positions in Array  : )
            var opl = get_position_pnl(gfClient, e);
            var sym = e.ContractPosition.Contract.Symbol;

            var def = false;
            var idef = 0.0;

            //If no value in dict, save them in -- (Avoid errors)
            if (hi_pnl.TryGetValue(sym, out idef))                              //Make sure this doesnt write over any current value?
            {
                hi_pnl[sym] = 0;
                trigger_pnl[sym] = 0;
            }


            //If new high, write over past hi_pnl
            if (opl > hi_pnl[sym])                                          //MIGHT need to init this dictionary when position opens?
            {
                hi_pnl[sym] = opl;

            }

            //Update Trigger price...
            trigger_pnl[sym] = hi_pnl[sym] * (1 - trail_dec);

            //Begin Trailing with Trigger...
            if (opl >= trail_tgt)
            {

                if (!trail_on.TryGetValue(sym, out def))
                    trail_on[sym] = true;

            }

            if (trail_on[sym])
            {
                if (opl <= trigger_pnl[sym])
                {
                    Console.WriteLine($"Exitting {sym} at {opl}");
                    //PlaceOrder(gfClient, e.ContractPosition.Contract.ElectronicContract, OrderSide.BuyToCover);  -- Replace w MKT order (or STOP ordeR?)
                    return 1;                                                   //signals time to exit... and logs -- Cleaner.
                }

            }

            if (opl <= fixed_stop)
            {
                return -1;
            }




            return 0;
        }

        //Need to get this PositionChangedEventArg shit somewhere ? No clue where...
        public void Run(GF.Api.Positions.PositionChangedEventArgs e,int EOD = 1600)
        {
            //Gets Datetime and uses inf loop to check for new positions, and manage the trailstop -- Eventually could also use OnBar for entries.
            //Maybe should be using onBar bc new position only on bars...
            DateTime now = DateTime.Now;
            DateTime saveUtcNow = DateTime.UtcNow; //If needed later...
            TimeSpan FUT_gap = new TimeSpan(0, 2, 0, 0); //0 days, 10 hours, 5 minutes and 1 second
            //tempDate.ToString("MMMM dd, yyyy")
            var ns = now.ToString("HH:mm:ss");

            Console.WriteLine("Checking for Market Open...");
            if(now.Hour == EOD) { Console.WriteLine("Waiting for Market Reopen."); }
            while(now.Hour == EOD)
            {
                now = DateTime.Now;
                Thread.Sleep(1000 * 60 * 5);                                    //5min Sleep -- waiting for re-open

            }
            Console.WriteLine("Market now Open.");
            while (true)
            {

                now = DateTime.Now;
                //need to call heartbeat? (runner?)

                //Check if Closed FIRST -- so don't accidentally exit in non-hours
                if (now.Hour == EOD)
                {
                    now = DateTime.Now;
                    Thread.Sleep(1000 * 60 * 60);
                }

                //Entry Stuff / OnBar could also be called right here...  might want to check that flat tho (return from onBar?)

                ns = now.ToString("HH:mm:ss");
                Debug.WriteLine($"Running check_ts_cs -- {ns}");
                run_cat_trail(e);



                Debug.WriteLine("Iteration Done ...");

            }
        }

        /*Initially -- Try to use JUST the above code...  IF you require more detail, then integrate the below stuff (tweaked examples) */
        //Goal was to find and loop thrugh all open positions... return them as an iterable object --  upon PositionsChangedEvent
        public IEnumerable<GF.Api.Positions.IPosition> GetPositions(GF.Api.Accounts.IAccount account, GF.Api.Positions.PositionChangedEventArgs e)
        {
            
            return e.ContractPosition.AsArray();
            
        }

        public IEnumerable<GF.Api.Positions.IPosition> GetOpenPositions(GF.Api.Accounts.IAccount account, GF.Api.Positions.PositionChangedEventArgs e)
        {
            GF.Api.Positions.IPosition[] active = { };                          //Should I make this a fixed memory level ? Will Dynamic create overflow?
            int idx = 0;
            foreach (var pos in e.AsArray())
            {
                //If Matched Account, Has fills, and has Open Position PNL... Add to active.
                if (e.ContractPosition.Account.ID == account.ID & e.ContractPosition.Fills != null & e.ContractPosition.OTE != 0)
                    //Console.WriteLine(pos);
                    active[idx] = pos.ContractPosition;
                idx += 1;


            }
            //Real purpose -- return changed positions -- > To feed into TrailStop.
            return active;

        }



        private int GetPositionVolume(GF.Api.Positions.IPosition position)
        {
            //            return position.Net.Volume;  //Does this not work??
            return position.Short.Volume - position.Long.Volume;
        }

        public void ExitPosition(GF.Api.Positions.IPosition position, GF.Api.Positions.PositionChangedEventArgs e)
        {
            var volume = GetPositionVolume(position);
            var side = volume > 0 ? OrderSide.Sell : OrderSide.BuyToCover;
            Console.WriteLine("Initiating order...");
            PlaceOrder(gfClient, e.ContractPosition.Contract.ElectronicContract, OrderSide.Sell, Math.Abs(volume));
            
        }



        /*
         *
         * TODO
         *
         * 
         *Reference STATICS from scratch to implement --- using Algo.SPiv() for example...
         * Initially just build out the TRAILSTOP + RUN methods
         * NEED to find how the fucking e's are being returned, and where -- that's the big missing piece.
         *
         * Also may need to build up the safety around this, this is pretty damn basic.
         */

    }
}
