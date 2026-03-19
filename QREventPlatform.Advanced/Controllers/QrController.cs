using Dapper;
using Microsoft.AspNetCore.Mvc;
using QRCoder;
using QREventPlatform.Advanced.Data;

[ApiController]
[Route("qr")]
public class QrController : ControllerBase
{
    private readonly DapperContext _ctx;

    public QrController(DapperContext ctx)
    {
        _ctx = ctx;
    }

    [HttpGet("{code}")]
    public IActionResult GetQr(string code)
    {
        using var db = _ctx.CreateConnection();

        // 🔐 Validate ticket
        var valid = db.ExecuteScalar<int>("""
             SELECT COUNT(*)
             FROM Tickets
             WHERE Code = @Code
               AND IsActive = 1
        """, new { Code = code });

        if (valid == 0)
            return NotFound("Invalid or inactive ticket");

        // ✅ Generate QR using platform-independent PngByteQRCode
        using var qrGen = new QRCodeGenerator();
        using var qrData = qrGen.CreateQrCode(code, QRCodeGenerator.ECCLevel.Q);
        var qrCode = new PngByteQRCode(qrData);
        var pngBytes = qrCode.GetGraphic(20);

        return File(pngBytes, "image/png");
    }
}
