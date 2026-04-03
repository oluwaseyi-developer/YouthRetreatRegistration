using ClosedXML.Excel;
using Microsoft.EntityFrameworkCore;
using YouthRetreatRegistration.Components;
using YouthRetreatRegistration.Data;
using YouthRetreatRegistration.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

// Use PostgreSQL in production (Render), SQLite for local development
var databaseUrl = Environment.GetEnvironmentVariable("DATABASE_URL");
if (!string.IsNullOrEmpty(databaseUrl))
{
    var connectionString = ConvertDatabaseUrl(databaseUrl);
    builder.Services.AddDbContext<AppDbContext>(options =>
        options.UseNpgsql(connectionString));
}
else
{
    var dbPath = Path.Combine(builder.Environment.ContentRootPath, "Data", "registrations.db");
    Directory.CreateDirectory(Path.GetDirectoryName(dbPath)!);
    builder.Services.AddDbContext<AppDbContext>(options =>
        options.UseSqlite($"Data Source={dbPath}"));
}

builder.Services.AddScoped<IRegistrationService, RegistrationService>();
builder.Services.AddScoped<IAdminAuthService, AdminAuthService>();

var app = builder.Build();

// Auto-migrate database on startup
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    db.Database.EnsureCreated();

    // Add new columns to existing tables (EnsureCreated won't alter existing schema)
    var conn = db.Database.GetDbConnection();
    await conn.OpenAsync();
    using var cmd = conn.CreateCommand();

    // Check if HasAttended column exists; add it if missing
    if (!string.IsNullOrEmpty(databaseUrl))
    {
        // PostgreSQL
        cmd.CommandText = """
            DO $$
            BEGIN
                IF NOT EXISTS (
                    SELECT 1 FROM information_schema.columns
                    WHERE table_name = 'Registrations' AND column_name = 'HasAttended'
                ) THEN
                    ALTER TABLE "Registrations" ADD COLUMN "HasAttended" boolean NOT NULL DEFAULT false;
                    ALTER TABLE "Registrations" ADD COLUMN "AttendedAt" timestamp with time zone;
                END IF;
            END $$;
            """;
    }
    else
    {
        // SQLite — no IF NOT EXISTS, so we catch the error if column already exists
        cmd.CommandText = """
            CREATE TABLE IF NOT EXISTS _migration_check (id INTEGER);
            DROP TABLE _migration_check;
            """;
        await cmd.ExecuteNonQueryAsync();

        try
        {
            cmd.CommandText = @"ALTER TABLE ""Registrations"" ADD COLUMN ""HasAttended"" INTEGER NOT NULL DEFAULT 0;";
            await cmd.ExecuteNonQueryAsync();
            cmd.CommandText = @"ALTER TABLE ""Registrations"" ADD COLUMN ""AttendedAt"" TEXT;";
            await cmd.ExecuteNonQueryAsync();
        }
        catch (Microsoft.Data.Sqlite.SqliteException)
        {
            // Columns already exist — safe to ignore
        }
        conn.Close();
        goto skipExec;
    }

    await cmd.ExecuteNonQueryAsync();
    conn.Close();
    skipExec:;
}

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    app.UseHsts();
}
app.UseStatusCodePagesWithReExecute("/not-found", createScopeForStatusCodePages: true);

// Skip HTTPS redirect when behind a reverse proxy (Render handles TLS)
if (string.IsNullOrEmpty(Environment.GetEnvironmentVariable("DATABASE_URL")))
{
    app.UseHttpsRedirection();
}

app.UseAntiforgery();

app.MapStaticAssets();
app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

// Excel export endpoint
app.MapGet("/api/export", async (IRegistrationService svc) =>
{
    var registrations = await svc.GetAllRegistrationsAsync();

    using var workbook = new XLWorkbook();
    var ws = workbook.Worksheets.Add("Registrations");

    // Title row
    ws.Cell(1, 1).Value = "Youth Retreat Registration — C&S Ayo Ni O, Ikotun-Egbe";
    ws.Range(1, 1, 1, 9).Merge();
    ws.Cell(1, 1).Style
        .Font.SetBold(true).Font.SetFontSize(16).Font.SetFontColor(XLColor.White)
        .Fill.SetBackgroundColor(XLColor.FromHtml("#0a1f5c"))
        .Alignment.SetHorizontal(XLAlignmentHorizontalValues.Center);
    ws.Row(1).Height = 36;

    // Subtitle
    ws.Cell(2, 1).Value = $"Exported on {DateTime.Now:dddd, dd MMMM yyyy 'at' HH:mm}";
    ws.Range(2, 1, 2, 9).Merge();
    ws.Cell(2, 1).Style
        .Font.SetItalic(true).Font.SetFontSize(10).Font.SetFontColor(XLColor.FromHtml("#475569"))
        .Alignment.SetHorizontal(XLAlignmentHorizontalValues.Center);
    ws.Row(2).Height = 22;

    // Header row
    var headers = new[] { "#", "Full Name", "Phone Number", "Gender", "Age Range", "Branch", "Expectations", "Date Registered", "Attended" };
    for (var i = 0; i < headers.Length; i++)
    {
        var cell = ws.Cell(4, i + 1);
        cell.Value = headers[i];
        cell.Style
            .Font.SetBold(true).Font.SetFontSize(11).Font.SetFontColor(XLColor.White)
            .Fill.SetBackgroundColor(XLColor.FromHtml("#6b21a8"))
            .Alignment.SetHorizontal(XLAlignmentHorizontalValues.Center)
            .Border.SetOutsideBorder(XLBorderStyleValues.Thin)
            .Border.SetOutsideBorderColor(XLColor.FromHtml("#4c1d95"));
    }
    ws.Row(4).Height = 28;

    // Data rows
    for (var row = 0; row < registrations.Count; row++)
    {
        var reg = registrations[row];
        var r = row + 5;
        var isEven = row % 2 == 0;
        var bgColor = isEven ? XLColor.FromHtml("#f8f9fc") : XLColor.White;

        ws.Cell(r, 1).Value = row + 1;
        ws.Cell(r, 2).Value = reg.FullName;
        ws.Cell(r, 3).Value = reg.PhoneNumber;
        ws.Cell(r, 4).Value = reg.Gender.ToString();
        ws.Cell(r, 5).Value = reg.AgeRange;
        ws.Cell(r, 6).Value = reg.BranchName;
        ws.Cell(r, 7).Value = reg.Expectations;
        ws.Cell(r, 8).Value = reg.RegisteredAt.ToString("dd MMM yyyy, HH:mm");
        ws.Cell(r, 9).Value = reg.HasAttended ? "Yes" : "No";

        for (var c = 1; c <= 9; c++)
        {
            ws.Cell(r, c).Style
                .Fill.SetBackgroundColor(bgColor)
                .Border.SetOutsideBorder(XLBorderStyleValues.Thin)
                .Border.SetOutsideBorderColor(XLColor.FromHtml("#e2e8f0"))
                .Alignment.SetVertical(XLAlignmentVerticalValues.Center);
            ws.Cell(r, c).Style.Font.SetFontSize(10);
        }
        ws.Cell(r, 1).Style.Alignment.SetHorizontal(XLAlignmentHorizontalValues.Center);
        ws.Cell(r, 4).Style.Alignment.SetHorizontal(XLAlignmentHorizontalValues.Center);
        ws.Cell(r, 5).Style.Alignment.SetHorizontal(XLAlignmentHorizontalValues.Center);
        ws.Cell(r, 8).Style.Alignment.SetHorizontal(XLAlignmentHorizontalValues.Center);
        ws.Cell(r, 9).Style.Alignment.SetHorizontal(XLAlignmentHorizontalValues.Center);
        ws.Row(r).Height = 22;
    }

    // Summary row
    var summaryRow = registrations.Count + 6;
    ws.Cell(summaryRow, 1).Value = $"Total: {registrations.Count} registrations";
    ws.Range(summaryRow, 1, summaryRow, 9).Merge();
    ws.Cell(summaryRow, 1).Style
        .Font.SetBold(true).Font.SetFontSize(11).Font.SetFontColor(XLColor.FromHtml("#0a1f5c"))
        .Fill.SetBackgroundColor(XLColor.FromHtml("#fef3c7"))
        .Alignment.SetHorizontal(XLAlignmentHorizontalValues.Center);

    // Column widths
    ws.Column(1).Width = 6;
    ws.Column(2).Width = 28;
    ws.Column(3).Width = 18;
    ws.Column(4).Width = 12;
    ws.Column(5).Width = 14;
    ws.Column(6).Width = 20;
    ws.Column(7).Width = 35;
    ws.Column(8).Width = 22;
    ws.Column(9).Width = 12;

    var stream = new MemoryStream();
    workbook.SaveAs(stream);
    stream.Position = 0;

    return Results.File(stream,
        "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
        $"YouthRetreatRegistrations_{DateTime.Now:yyyyMMdd_HHmm}.xlsx");
});

app.Run();

// Converts Render's DATABASE_URL (postgres://user:pass@host/db) to Npgsql connection string
static string ConvertDatabaseUrl(string databaseUrl)
{
    var uri = new Uri(databaseUrl);
    var userInfo = uri.UserInfo.Split(':');
    var port = uri.Port > 0 ? uri.Port : 5432;
    return $"Host={uri.Host};Port={port};Database={uri.AbsolutePath.TrimStart('/')};Username={userInfo[0]};Password={userInfo[1]};SSL Mode=Require;Trust Server Certificate=true";
}
