using Application.Common;
using Domain.Entities;
using MediatR;
using NSubstitute;
using NSubstitute.ReceivedExtensions;

namespace UnitTests.Application.Common
{
    public class DomainEventDispatcherTests
    {
        private readonly IMediator mediator;


        public DomainEventDispatcherTests()
        {
            mediator = Substitute.For<IMediator>();
        }

        [Theory]
        [MemberData(nameof(TestCases))]
        public async Task Consume_Should_PublishNotification(List<IDomainEvent> domainEvents)
        {
            var dispatcher = new DomainEventDispatcher(mediator);

            dispatcher.Consume(domainEvents);

            await mediator.Received(domainEvents.Count)
                .Publish(Arg.Any<DomainEventNotification<IDomainEvent>>());
        }

        public static IEnumerable<object[]> TestCases => new List<object[]>
        {
            new object[]
            {
                new List<IDomainEvent>()
            },
            new object[]
            {
                new List<IDomainEvent>()
                {
                    Substitute.For<IDomainEvent>(),
                    Substitute.For<IDomainEvent>()
                }
            },
        };
    }
}