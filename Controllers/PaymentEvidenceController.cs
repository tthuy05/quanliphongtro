using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TroiSinhVien.Services.Interfaces;

namespace TroiSinhVien.Controllers;

[Authorize]
[Route("PaymentEvidence")]
public sealed class PaymentEvidenceController(IPaymentService payments) : Controller
{
    [HttpGet("{paymentId:int}")]
    public async Task<IActionResult> Download(int paymentId, CancellationToken ct)
    {
        var result = await payments.GetEvidenceAsync(paymentId, ct);
        if (!result.Succeeded || result.Value is null) return NotFound();
        return File(result.Value.Content, result.Value.ContentType, result.Value.FileName);
    }
}
