using System;
using System.Collections.Generic;
using System.Text;
using Xunit;
using RestApiTask.Models;
using RestApiTask.Services;

namespace RestApi.Tests;

public class RestApiServiceTests
{
    private readonly RestApiService _service;

    public RestApiServiceTests()
    {
        _service = new RestApiService();
    }

    [Fact]
    public void ProcessRestApi_ValidRequest_ReturnsSuccess()
    {
        var request = new RestApiRequest
        {
            PartnerKey = "FAKEGOOGLE",
            PartnerRefNo = "FG-00001",
            PartnerPassword = Convert.ToBase64String(Encoding.UTF8.GetBytes("FAKEPASSWORD1234")),
            TotalAmount = 1000,
            Items = new List<ItemDetail>
            {
                new ItemDetail { PartnerItemRef = "i-00001", Name = "Pen", Qty = 4, UnitPrice = 200 },
                new ItemDetail { PartnerItemRef = "i-00002", Name = "Ruler", Qty = 2, UnitPrice = 100 }
            },
            Timestamp = DateTime.UtcNow.ToString("o"),
            Sig = "any_signature"
        };

        var result = _service.ProcessRestApi(request);

        Assert.Equal(1, result.Result);
        Assert.Equal(1000, result.TotalAmount);
        Assert.Equal(0, result.TotalDiscount); // MYR 10, no discount
        Assert.Equal(1000, result.FinalAmount);
        Assert.Equal("Success.", result.ResultMessage);
    }

    [Fact]
    public void ProcessRestApi_SampleCase_ReturnsCorrectDiscount()
    {
        var request = new RestApiRequest
        {
            PartnerKey = "FAKEGOOGLE",
            PartnerRefNo = "FG-00001",
            PartnerPassword = Convert.ToBase64String(Encoding.UTF8.GetBytes("FAKEPASSWORD1234")),
            TotalAmount = 100000,
            Items = new List<ItemDetail>
            {
                new ItemDetail { PartnerItemRef = "i-00001", Name = "Item", Qty = 1, UnitPrice = 100000 }
            },
            Timestamp = DateTime.UtcNow.ToString("o"),
            Sig = "any_signature"
        };

        var result = _service.ProcessRestApi(request);

        Assert.Equal(1, result.Result);
        Assert.Equal(100000, result.TotalAmount);
        Assert.Equal(10000, result.TotalDiscount); // MYR 1000, 10% discount
        Assert.Equal(90000, result.FinalAmount);
        Assert.Equal("Success.", result.ResultMessage);
    }

    [Fact]
    public void ProcessRestApi_InvalidTotalAmount_ReturnsFailure()
    {
        var request = new RestApiRequest
        {
            PartnerKey = "FAKEGOOGLE",
            PartnerRefNo = "FG-00001",
            PartnerPassword = Convert.ToBase64String(Encoding.UTF8.GetBytes("FAKEPASSWORD1234")),
            TotalAmount = 900, // Sum = 4*200 + 2*100 = 1000
            Items = new List<ItemDetail>
            {
                new ItemDetail { PartnerItemRef = "i-00001", Name = "Pen", Qty = 4, UnitPrice = 200 },
                new ItemDetail { PartnerItemRef = "i-00002", Name = "Ruler", Qty = 2, UnitPrice = 100 }
            },
            Timestamp = DateTime.UtcNow.ToString("o"),
            Sig = "any_signature"
        };

        var result = _service.ProcessRestApi(request);

        Assert.Equal(0, result.Result);
        Assert.Equal("Invalid Total Amount.", result.ResultMessage);
    }

    [Fact]
    public void ProcessRestApi_InvalidPassword_ReturnsFailure()
    {
        var request = new RestApiRequest
        {
            PartnerKey = "FAKEGOOGLE",
            PartnerRefNo = "FG-00001",
            PartnerPassword = "V1JPTkdQQVNTV09SRA==",
            TotalAmount = 1000,
            Items = new List<ItemDetail>
            {
                new ItemDetail { PartnerItemRef = "i-00001", Name = "Pen", Qty = 4, UnitPrice = 200 },
                new ItemDetail { PartnerItemRef = "i-00002", Name = "Ruler", Qty = 2, UnitPrice = 100 }
            },
            Timestamp = DateTime.UtcNow.ToString("o"),
            Sig = "any_signature"
        };

        var result = _service.ProcessRestApi(request);

        Assert.Equal(0, result.Result);
        Assert.Equal("Invalid Password", result.ResultMessage);
    }

    [Fact]
    public void ProcessRestApi_NullRequest_ReturnsFailure()
    {
        var result = _service.ProcessRestApi(null);

        Assert.Equal(0, result.Result);
        Assert.Equal("Request is null", result.ResultMessage);
    }
}