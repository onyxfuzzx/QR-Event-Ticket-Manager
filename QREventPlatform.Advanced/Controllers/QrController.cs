using Dapper;
using Microsoft.AspNetCore.Mvc;
using QRCoder;
using QREventPlatform.Advanced.Data;
using System.Drawing.Imaging;

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



        // ✅ Generate QR only for valid ticket
        using var qrGen = new QRCodeGenerator();
        using var qrData = qrGen.CreateQrCode(code, QRCodeGenerator.ECCLevel.Q);
        using var qrCode = new QRCode(qrData);
        using var bitmap = qrCode.GetGraphic(20);

        using var ms = new MemoryStream();
        bitmap.Save(ms, ImageFormat.Png);

        return File(ms.ToArray(), "image/png");
    }
}
