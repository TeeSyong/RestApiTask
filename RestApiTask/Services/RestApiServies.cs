using RestApiTask.Models;
using System.Diagnostics.Eventing.Reader;
using System.Security.Cryptography;
using System.Text;
namespace RestApiTask.Services;

public class RestApiService
{
    private readonly Dictionary<string, (string PartnerNo, string Password)> _partners = new()
    {
        { "FAKEGOOGLE", ("FG-00001", "FAKEPASSWORD1234") },
        { "FAKEPEOPLE", ("FG-00002", "FAKEPASSWORD4578") }
    };

    public RestApiResponse ProcessRestApi(RestApiRequest request)
    {
        // All Validation for partner 
        if (!_partners.TryGetValue(request.PartnerKey, out var partner))
            return new RestApiResponse { Result = 0, ResultMessage = "Access Denied!" };

        string decodedPassword = Encoding.UTF8.GetString(Convert.FromBase64String(request.PartnerPassword));
        if (decodedPassword != partner.Password)
            return new RestApiResponse { Result = 0, ResultMessage = "Invalid Password" };

        DateTime timestamp = DateTime.Parse(request.Timestamp).ToUniversalTime();
        DateTime serverTime = DateTime.UtcNow;
        TimeSpan timeDifference = serverTime - timestamp;
        if (timeDifference.TotalMinutes < -5 || timeDifference.TotalMinutes > 5)
            return new RestApiResponse { Result = 0, ResultMessage = "Expired." };

        if (!VerifySignature(request))
            return new RestApiResponse { Result = 0, ResultMessage = "Access Denied!" };

        // Calculate total and discount for the items
        long calculatedTotal = 0;
        if (request.Items != null)
        {
            foreach (var item in request.Items)
            {
                calculatedTotal += item.Qty * item.UnitPrice;
            }
        }

        if (calculatedTotal != request.TotalAmount)
            return new RestApiResponse { Result = 0, ResultMessage = "Invalid Total Amount." };

        (long totalDiscount, long finalAmount) = CalculateDiscounts(request.TotalAmount);

        return new RestApiResponse
        {
            Result = 1,
            TotalAmount = request.TotalAmount,
            TotalDiscount = 0,
            FinalAmount = request.TotalAmount
        };
    }

    private bool VerifySignature(RestApiRequest request)
    {
        try
        {
            DateTime timestamp = DateTime.Parse(request.Timestamp).ToUniversalTime();
            string sigTimestamp = timestamp.ToString("yyyyMMddHHmmss");

            string concatString = $"{sigTimestamp}{request.PartnerKey}{request.PartnerRefNo}{request.TotalAmount}{request.PartnerPassword}";

            using SHA256 sha256 = SHA256.Create();
            byte[] hashBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(concatString));
            string hash = BitConverter.ToString(hashBytes).Replace("-", "").ToLower();
            string base64Hash = Convert.ToBase64String(Encoding.UTF8.GetBytes(hash));

            return base64Hash == request.Sig;
        }
        catch
        {
            return false;
        }
    }

    private (long totalDiscount, long finalAmount) CalculateDiscounts(long totalAmount)
    {
        //Convert to ringgit
        double totalAmountMYR = totalAmount / 100.0;

        // Base discount logics
        double baseDiscountPercent = 0;
        if (totalAmountMYR >= 200 && totalAmountMYR <= 500)
            baseDiscountPercent = 5;
        else if (totalAmountMYR >= 501 && totalAmountMYR <= 800)
            baseDiscountPercent = 7;
        else if (totalAmountMYR >= 801 && totalAmountMYR <= 1200)
            baseDiscountPercent = 10;
        else if (totalAmountMYR > 1200)
            baseDiscountPercent = 15;


        // Conditional discounts logicss
        double conditionalDiscountPercent = 0;
        if (totalAmountMYR > 500 && IsPrime((long)totalAmountMYR))
            conditionalDiscountPercent += 8;
        if (totalAmountMYR > 900 && totalAmount % 100 == 5) 
            conditionalDiscountPercent += 10;

        // Total discount logics
        double totalDiscountPercent = baseDiscountPercent;
        if ((baseDiscountPercent + conditionalDiscountPercent) > 20)
            totalDiscountPercent = 20;
        else
            totalDiscountPercent += conditionalDiscountPercent;

        // Final Discount in cents
        long totalDiscount = (long)Math.Round(totalAmount * totalDiscountPercent / 100);
        long finalAmount = totalAmount - totalDiscount;

        return (totalDiscount, finalAmount);
    }

    private bool IsPrime(long number)
    {
        if (number <= 1) return false;
        if (number == 2) return true;
        if (number % 2 == 0) return false;

        for (long i = 3; i <= Math.Sqrt(number); i += 2)
        {
            if (number % i == 0) return false;
        }
        return true;
    }
}
