using FluentAssertions;
using Transfers.Domain.Common;
using Transfers.Domain.Transfers;
using Transfers.Domain.Transfers.Events;

namespace Transfers.UnitTests.Domain;

public sealed class TransferTests
{
    private static readonly Guid SourceId = Guid.NewGuid();
    private static readonly Guid DestinationId = Guid.NewGuid();

    [Fact]
    public void CreateImmediate_StartsInProcessing()
    {
        var transfer = Transfer.CreateImmediate(SourceId, DestinationId, 100, "key", TestData.Now);

        transfer.Status.Should().Be(TransferStatus.Processing);
        transfer.Type.Should().Be(TransferType.Immediate);
        transfer.IdempotencyKey.Should().Be("key");
    }

    [Fact]
    public void Create_WithSameSourceAndDestination_Throws()
    {
        var act = () => Transfer.CreateImmediate(SourceId, SourceId, 100, null, TestData.Now);

        act.Should().Throw<DomainException>().Which.Code.Should().Be("same_source_and_destination");
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void Create_WithNonPositiveAmount_Throws(decimal amount)
    {
        var act = () => Transfer.CreateImmediate(SourceId, DestinationId, amount, null, TestData.Now);

        act.Should().Throw<DomainException>().Which.Code.Should().Be("amount_must_be_positive");
    }

    [Fact]
    public void Schedule_InTheFuture_RaisesScheduledEvent()
    {
        var transfer = Transfer.Schedule(SourceId, DestinationId, 100, TestData.Now.AddHours(1), null, TestData.Now);

        transfer.Status.Should().Be(TransferStatus.Scheduled);
        transfer.ScheduledFor.Should().Be(TestData.Now.AddHours(1));
        transfer.DomainEvents.Should().ContainSingle().Which.Should().BeOfType<TransferScheduled>();
    }

    [Fact]
    public void Schedule_InThePast_Throws()
    {
        var act = () => Transfer.Schedule(SourceId, DestinationId, 100, TestData.Now.AddMinutes(-1), null, TestData.Now);

        act.Should().Throw<DomainException>().Which.Code.Should().Be("schedule_must_be_in_future");
    }

    [Fact]
    public void Schedule_WithOffset_StoresUtc()
    {
        var localTime = new DateTimeOffset(2026, 10, 20, 18, 0, 0, TimeSpan.FromHours(-3));

        var transfer = Transfer.Schedule(SourceId, DestinationId, 100, localTime, null, TestData.Now);

        transfer.ScheduledFor!.Value.Offset.Should().Be(TimeSpan.Zero);
        transfer.ScheduledFor.Should().Be(localTime);
    }

    [Fact]
    public void Cancel_WhenScheduled_MarksAsCancelled()
    {
        var transfer = Transfer.Schedule(SourceId, DestinationId, 100, TestData.Now.AddHours(1), null, TestData.Now);

        transfer.Cancel(TestData.Now);

        transfer.Status.Should().Be(TransferStatus.Cancelled);
        transfer.CancelledAt.Should().Be(TestData.Now);
    }

    [Fact]
    public void Cancel_WhenNotScheduled_Throws()
    {
        var transfer = Transfer.CreateImmediate(SourceId, DestinationId, 100, null, TestData.Now);

        var act = () => transfer.Cancel(TestData.Now);

        act.Should().Throw<DomainException>().Which.Code.Should().Be("invalid_transfer_status_transition");
    }

    [Fact]
    public void StartProcessing_BeforeScheduledDate_Throws()
    {
        var transfer = Transfer.Schedule(SourceId, DestinationId, 100, TestData.Now.AddHours(1), null, TestData.Now);

        var act = () => transfer.StartProcessing(TestData.Now.AddMinutes(30));

        act.Should().Throw<DomainException>().Which.Code.Should().Be("transfer_not_due");
    }

    [Fact]
    public void StartProcessing_WhenDue_MovesToProcessing()
    {
        var transfer = Transfer.Schedule(SourceId, DestinationId, 100, TestData.Now.AddHours(1), null, TestData.Now);

        transfer.StartProcessing(TestData.Now.AddHours(1));

        transfer.Status.Should().Be(TransferStatus.Processing);
    }
}
