using System;
using System.Collections.Generic;
using System.Linq;
using GF;
using GF.Api.Orders.Drafts;
using GF.Api.Orders.Drafts.Validation;
using GF.Api.Values.Orders;

namespace gain
{

    internal class PositionCloser
    {
        private readonly GF.Api.IGFClient _client;

        private readonly Dictionary<GF.Api.Positions.IPosition, Action> _resultActions = new Dictionary<GF.Api.Positions.IPosition, Action>();

        public PositionCloser(GF.Api.IGFClient client)
        {
            _client = client;
            _client.Connection.Aggregate.Disconnected += GFClient_OnDisconnected;
            _client.Orders.OrderStateChanged += GFClient_OnOrderStateChanged;
        }

        public IEnumerable<GF.Api.Orders.IOrder> WorkingOrders
        {
            get { return _client.Orders.Get().Where(order => !order.IsFinalState); }
        }

        public void Close(GF.Api.Positions.IPosition position, Action action)
        {
            if (_resultActions.ContainsKey(position))
                return;

            var orders = GetPositionOrders(position).ToList();
            if (orders.Any())
            {
                CancelAll(orders);
            }
            else
            {
                if (position.Net.Volume == 0)
                {
                    action();
                    return;
                }

                PlaceExitOrder(position);
            }

            _resultActions[position] = action;
        }

        public void CancelAll(IEnumerable<GF.Api.Orders.IOrder> orders)
        {
            foreach (var order in orders.Where(o => !HasPendingCommands(o)))
                _client.Orders.CancelOrder(order.ID, SubmissionType.Automatic);
        }

        public IEnumerable<GF.Api.Orders.IOrder> GetPositionOrders(GF.Api.Positions.IPosition position)
        {
            return GetPositionOrders(position.Account, position.Contract);
        }

        public IEnumerable<GF.Api.Orders.IOrder> GetPositionOrders(GF.Api.Accounts.IAccount account, GF.Api.Contracts.IContract contract)
        {
            return WorkingOrders.Where(order =>
                order.Account.ID == account.ID
                && order.Contract.PositionContract.ID == contract.PositionContract.ID);
        }

        public void Dispose()
        {
            _resultActions.Clear();
            _client.Connection.Aggregate.Disconnected -= GFClient_OnDisconnected;
        }

        private void GFClient_OnDisconnected(GF.Api.IGFClient client, GF.Api.Connection.DisconnectedEventArgs disconnectedEventArgs)
        {
            _resultActions.Clear();
        }

        private void GFClient_OnOrderStateChanged(GF.Api.IGFClient client, GF.Api.Orders.OrderStateChangedEventArgs e)
        {
            if (!e.Order.IsFinalState)
                return;

            var avgPosition = e.Order.Account.AvgPositions[e.Order.Contract];
            if (avgPosition == null)
                return;

            if (_resultActions.TryGetValue(avgPosition, out var action))
            {
                var orders = GetPositionOrders(e.Order.Account, e.Order.Contract).ToList();
                if (orders.Any())
                {
                    CancelAll(orders);
                }
                else
                {
                    if (avgPosition.Net.Volume == 0)
                    {
                        _resultActions.Remove(avgPosition);
                        action();
                    }
                    else
                    {
                        PlaceExitOrder(avgPosition);
                    }
                }
            }
        }

        private void PlaceExitOrder(GF.Api.Positions.IPosition avgPosition)
        {
            OrderDraft orderDraft = new OrderDraftBuilder()
                .WithAccountID(avgPosition.Account.ID)
                .WithContractID(avgPosition.Contract.ID)
                .WithSide(avgPosition.Net.Volume > 0 ? OrderSide.Sell : OrderSide.Buy)
                .WithOrderType(OrderType.Market)
                .WithQuantity(Math.Abs(avgPosition.Net.Volume))
                .WithComments("Exit")
                .Build();

            IReadOnlyList<OrderDraftValidationError> validationErrors = _client.Orders.Drafts.Validate(orderDraft);
            if (validationErrors.Any())
                throw new Exception($"Exit Order Draft validation error: {validationErrors.First()}");

            _client.Orders.SendOrder(orderDraft);
        }

        private bool HasPendingCommands(GF.Api.Orders.IOrder order)
        {
            return order.Commands.Any(cmd => cmd.State == CommandState.Sent);
        }
    }
}

/*
 *  main snippet of the program ? still makes no sense... Why is it inside a class?
internal class PositionCloserProgram
{
    private static PositionCloserProgram _positionCloser;

    private static void Main(string[] args)
    {
        var client = GF.Api.Impl.GFApi.CreateClient();
        _positionCloser = new PositionCloserProgram(client);

        var runner = new GF.Api.Threading.GFClientRunner(client);
        runner.Start(); //HB Start...

        client.Connection.Aggregate.LoginCompleted += GFClient_OnLoginCompleted; // IS THIS AN EVENT ?! is this all it is ? SEE BELOW -- client + e args, but not USED?

        Console.WriteLine("Connecting...");

        client.Connection.Aggregate.Connect(
            new GF.Api.Connection.ConnectionContextBuilder()
                .WithUserName("username")
                .WithPassword("password")
                .WithPort(9210)
                .WithHost("api.gainfutures.com")
                .WithUUID("9e61a8bc-0a31-4542-ad85-33ebab0e4e86")
                .WithForceLogin(true)
                .Build());

        Console.WriteLine("Press any key to exit");
        Console.ReadKey();

        runner.Stop(); //HB Stop...
    }

    //Seems this is simply called by appending onto the client.Connection.Aggregate.LoginCompleted object...
    private static void GFClient_OnLoginCompleted(GF.Api.IGFClient client, GF.Api.Connection.LoginCompleteEventArgs e)
    {
        var position = client.Accounts.Get().First().AvgPositions.First().Value;
        if (position != null)
            _positionCloser.Close(position, () => Console.WriteLine($"{position.Account.Spec}:{position.Contract.Symbol} closed!"));
    }
}


//Module Name Class -- Ignore
class CloseExample
{
    public CloseExample()
    {
    }
}

}

    */
