namespace MassTransit.Registration.Futures
{
    using System;


    public interface IFutureRequestDefinition<TRequest>
        where TRequest : class
    {
        Uri RequestAddress { get; }
    }
}
