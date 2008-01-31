using System;
using NUnit.Framework;
using NUnit.Framework.SyntaxHelpers;
using MassTransit.ServiceBus.Subscriptions;
using Rhino.Mocks;

namespace MassTransit.ServiceBus.Tests
{
    using Internal;

    [TestFixture]
    public class When_A_Message_Is_Received_On_The_Service_Bus
    {
        private MockRepository mocks;
        private ServiceBus _serviceBus;
        private IMessageQueueEndpoint mockServiceBusEndPoint;
        private IMessageReceiver mockReceiver;
        private string queueName = @".\private$\test_servicebus";
        
        [SetUp]
        public void SetUp()
        {
            ServiceBusSetupFixture.ValidateAndPurgeQueue(queueName);
            mocks = new MockRepository();
            mockServiceBusEndPoint = mocks.CreateMock<IMessageQueueEndpoint>();
            mockReceiver = mocks.CreateMock<IMessageReceiver>();
            _serviceBus = new ServiceBus(mockServiceBusEndPoint, new LocalSubscriptionCache());
        }

        [TearDown]
        public void TearDown()
        {
            mocks = null;
            _serviceBus = null;
            mockServiceBusEndPoint = null;
        }

        [Test]
        public void An_Event_Handler_Should_Be_Called()
        {
            using(mocks.Record())
            {
                
                Expect.Call(mockServiceBusEndPoint.Uri).Return(new Uri("msmq://localhost/test_servicebus")).Repeat.Any(); //stupid log4net
                Expect.Call(mockServiceBusEndPoint.Receiver).Return(mockReceiver);
                Expect.Call(delegate { mockReceiver.Subscribe(null); }).IgnoreArguments().Repeat.Any();
            }
            using (mocks.Playback())
            {

                bool _received = false;

                MessageReceivedCallback<PingMessage> handler = delegate
                                                                   {
                                                                       _received = true;
                                                                   };

                _serviceBus.Subscribe(handler);

                IEnvelope envelope = new Envelope(mockServiceBusEndPoint, new PingMessage());
                _serviceBus.Deliver(envelope);

                Assert.That(_received, Is.True);
            }
        }

        [Test]
        public void What_Happens_If_No_Subscriptions()
        {
            using(mocks.Record())
            {
                Expect.Call(mockServiceBusEndPoint.Uri).Return(new Uri("msmq://localhost/test_servicebus")).Repeat.Any(); //stupid log4net
            }
            using (mocks.Playback())
            {
                _serviceBus = new ServiceBus(mockServiceBusEndPoint, new LocalSubscriptionCache());

                bool _received = false;

                IEnvelope envelope = new Envelope(mockServiceBusEndPoint, new PingMessage());
                _serviceBus.Deliver(envelope);

                Assert.That(_received, Is.False);
            }
        }
    }
}
