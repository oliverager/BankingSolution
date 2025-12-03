using System.Net;
using System.Net.Http.Json;
using Banking.Api.Contracts;

namespace Banking.ApiTests;

public class TransferApiTests : IClassFixture<BankingApiFactory>
{
    private readonly HttpClient _client;

    public TransferApiTests(BankingApiFactory factory)
    {
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task Transfer_ReturnsBadRequest_WhenAccountDoesNotExist()
    {
        var request = new TransferRequest
        {
            FromAccountId = Guid.NewGuid(),
            ToAccountId = Guid.NewGuid(),
            Amount = 100m
        };

        var response = await _client.PostAsJsonAsync("/transfers", request);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }
}