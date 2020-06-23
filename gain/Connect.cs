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
using GF.Api.Orders.Drafts;
using GF.Api.Orders.Drafts.Validation;
using GF.Api.Values.Orders;

namespace cs_test
{




    public class Connect
    {
        public GF.Api.IGFClient gfClient; // = GF.Api.Impl.GFApi.CreateClient();
        GF.Api.Threading.GFClientRunner runner;                                 //ptr to runner?
        int port;

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
            runner = new GF.Api.Threading.GFClientRunner(gfClient);             //declared in class def
            runner.Start();
            Console.WriteLine("Runner (HB) started.");
            port = p;

            Console.WriteLine("Connecting...");

        }

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


        private static void GFClient_OnPriceTick(GF.Api.IGFClient client, GF.Api.Contracts.PriceChangedEventArgs e)
        {
            //TRADING LOGIC !! REPLACE WITH SHIT YOU WANT.
            if (Math.Abs(e.Price.LastPrice - e.Price.BidPrice) < e.Contract.TickSize)
                PlaceOrder(client, e.Contract, OrderSide.Buy, e.Price.BidPrice, "By Bid");
            else if (Math.Abs(e.Price.LastPrice - e.Price.AskPrice) < e.Contract.TickSize)
                PlaceOrder(client, e.Contract, OrderSide.Sell, e.Price.AskPrice, "By Ask");
        }

        private static void PlaceOrder(GF.Api.IGFClient client, GF.Api.Contracts.IContract contract, GF.Api.Values.Orders.OrderSide orderSide, double limitPrice = 0.0, string comments = "")
        {

            //TWEAK THIS SO THAT IF LIMITPRICE = 0.0, MARKET ORDER!!!!
            if (client.Orders.Get().Count == 0 || client.Orders.Get().Last().IsFinalState)
            {
                OrderDraft orderDraft = new OrderDraftBuilder()
                    .WithAccountID(client.Accounts.Get().First().ID)
                    .WithContractID(contract.ID)
                    .WithSide(orderSide)
                    .WithOrderType(GF.Api.Values.Orders.OrderType.Limit)
                    .WithPrice(limitPrice)
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

        private double get_position_pnl(GF.Api.IGFClient client, GF.Api.Positions.PositionChangedEventArgs e)
        {
            //SEEMS LIKE IT NEEDS A LOOP... NO?
            //foreach(var pos in GF.Api.Positions....)

            return e.ContractPosition.OTE + e.ContractPosition.Gain;
            //IF DOES NOT WORK -- WILL NEED TO CALCULATE IT LIKE IN IB WITH AVGPRICE AND MID ... would be frustrating
        }


        public int run_cat_trail(GF.Api.Positions.PositionChangedEventArgs e)
        {
            var ret = 0;
            //Should be inside loop ? Called inside loop?
            foreach (var pos in e.AsArray()) //?
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
                    if (e.ContractPosition.Net.Volume < 0)
                    {
                        PlaceOrder(gfClient, e.ContractPosition.Contract.ElectronicContract, OrderSide.BuyToCover);
                        Console.WriteLine("SX order sent.");
                    }
                    if (e.ContractPosition.Net.Volume > 0)
                    {
                        PlaceOrder(gfClient, e.ContractPosition.Contract.ElectronicContract, OrderSide.Sell);
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




    }
}
