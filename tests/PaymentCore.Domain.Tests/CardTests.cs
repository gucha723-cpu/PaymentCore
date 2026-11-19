using PaymentCore.Domain.Entities;
using PaymentCore.Domain.Services;
using PaymentCore.Domain.Entities.Enums;
using PaymentCore.Domain.Exceptions;

namespace PaymentCore.Domain.Tests;

public class CardTests
{
    [Fact]
    public void Block_ActiveCard_ChangesStatusToBlocked()
    {
        var expiryDate = DateOnly.FromDateTime(DateTime.UtcNow).AddYears(2);

        var card = new DebitCard(Guid.NewGuid(), CardNumberGenerator.Generate(), expiryDate);

        card.Block();

        Assert.Equal(CardStatus.Blocked, card.Status);
    }

    [Fact]
    public void Block_AlreadyBlockedCard_DoesNotUpdateTimestamp()
    {
        var expiryDate = DateOnly.FromDateTime(DateTime.UtcNow).AddYears(2);

        var card = new DebitCard(Guid.NewGuid(), CardNumberGenerator.Generate(), expiryDate);

        card.Block();

        var updateAt = card.UpdatedAt;

        Thread.Sleep(20);

        card.Block();

        Assert.Equal(updateAt, card.UpdatedAt);

    }

    [Fact]
    public void Unblock_BlockedCard_ChangesStatusToActive()
    {
        var expiryDate = DateOnly.FromDateTime(DateTime.UtcNow).AddYears(2);

        var card = new DebitCard(Guid.NewGuid(), CardNumberGenerator.Generate(), expiryDate);

        card.Block();

        card.Unblock();
        
        Assert.Equal(CardStatus.Active, card.Status);
    }

    [Fact]
    public void Unblock_ExpiredCard_ThrowsCardExpiredException()
    {
        var expiryDate = DateOnly.FromDateTime(DateTime.UtcNow).AddDays(-1);

        var card = new DebitCard(Guid.NewGuid(), CardNumberGenerator.Generate(), expiryDate);

        Assert.Throws<CardExpiredException>(() => card.Unblock());

        Assert.Equal(CardStatus.Expired, card.Status);
    }

    [Fact]
    public void Block_ExpiredCard_ChangesStatusToBlocked()
    {
        var expiryDate = DateOnly.FromDateTime(DateTime.UtcNow).AddDays(-1);
        var card = new DebitCard(Guid.NewGuid(), CardNumberGenerator.Generate(), expiryDate);

        card.Block();

        Assert.Equal(CardStatus.Blocked, card.Status);
    }

    [Fact]
    public void Unblock_ExpiredCard_ThrowsExceptionAndChangesStatusToExpired()
    {
        var expiryDate = DateOnly.FromDateTime(DateTime.UtcNow).AddDays(-1);
        var card = new DebitCard(Guid.NewGuid(), CardNumberGenerator.Generate(), expiryDate);

        card.Block();

        Assert.Throws<CardExpiredException>(() => card.Unblock());

        Assert.Equal(CardStatus.Expired, card.Status);
    }
}