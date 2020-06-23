using System;

namespace gain
{

    public class Connect {

        public Connect{
            GF.Api.IGFClient gfClient = GF.Api.Impl.GFApi.CreateClient();

            var runner = new GF.Api.Threading.GFClientRunner(gfClient);
            runner.Start();
            };

            Console.WriteLine("Connecting...");

            gfClient.Connection.Aggregate.LoginCompleted += (client, e) => Console.WriteLine("Connection complete");
            gfClient.Connection.Aggregate.LoginFailed += (client, e) => Console.WriteLine($"Connection failed: {e.FailReason}");
            gfClient.Connection.Aggregate.Disconnected += (client, e) => Console.WriteLine($"Disconnected: {e.Message}");
            gfClient.Logging.ErrorOccurred += (client, e) => Console.WriteLine($"{e.Exception.Message}");

            gfClient.Connection.Aggregate.Connect(
                new GF.Api.Connection.ConnectionContextBuilder()
                    .WithUserName("username")
                    .WithPassword("password")
                    .WithUUID("9e61a8bc-0a31-4542-ad85-33ebab0e4e86")
                    .WithPort(9200)
                    .WithHost("api.gainfutures.com")
                    .WithForceLogin(true)
                    .Build());

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
                    Console.WriteLine($"\tNet Options Value: {totalBalance.LongCallOptionsValue + totalBalance.LongPutOptionsValue + totalBalance.ShortCallOptionsValue + totalBalance.ShortPutOptionsValue:c}");
                    Console.WriteLine($"Average Positions: {account.AvgPositions.Count}");
                    Console.WriteLine($"Orders: {gfClient.Orders.Get().Count}, last one: {(gfClient.Orders.Get().Count > 0 ? gfClient.Orders.Get().Last().ToString() : string.Empty)}");
                    Console.WriteLine();
                });
            };

    public class Connect
    {
        public Connect()
        {
        }
    }
}
