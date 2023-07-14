namespace Application.Tests.Url.Commands;

using HashidsNet;
using Microsoft.EntityFrameworkCore;
using Moq;
using UrlShortenerService.Application.Common.Interfaces;
using UrlShortenerService.Application.Url.Commands;
using UrlShortenerService.Domain.Entities;

public class CreateShortUrlCommandTest
{
    private Mock<Hashids> hashIds { get; set; }
    private Mock<IApplicationDbContext> context { get; set; }
    private readonly string ServerAddress = "https://localhost:7072";
    public CreateShortUrlCommandTest()
    {
        //TODO: Better to set DI for environment for Hashids Configuration
        context = new Mock<IApplicationDbContext>();
        context.Setup(t => t.Urls).Returns(new Mock<DbSet<Url>>().Object);
        hashIds = new Mock<Hashids>("LPMUtoRmwaRvEosIo", 6, "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ1234567890", "cfhistuCFHISTU");

    }

    [Fact]
    public async Task DoesCreateShortUrlCommandHandlerWork()
    {
        //Arrange
        CreateShortUrlCommand command = new CreateShortUrlCommand()
        {
            Url = "https://www.enpal.de/"
        };
        CreateShortUrlCommandHandler createShortUrlCommandHandler = new CreateShortUrlCommandHandler(context.Object, hashIds.Object);
        //Act
        var resultFromHandler = await createShortUrlCommandHandler.Handle(command, CancellationToken.None);
        //Arrange
        var urlEntityFromDb = await context.Object.Urls.SingleOrDefaultAsync(t => t.OriginalUrl == command.Url);
        string expectedResult = ServerAddress + hashIds.Object.EncodeLong(urlEntityFromDb?.Id ?? 0);
        //Assert
        Assert.Equal(expectedResult, resultFromHandler);
    }
}
