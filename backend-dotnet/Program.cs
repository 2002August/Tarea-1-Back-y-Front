using System.Text.RegularExpressions;
using Backend.Data;
using Backend.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

var builder = WebApplication.CreateBuilder(args);

// ========= DB Context (MySQL) + LOGS EF =========
builder.Services.AddDbContext<UmgContext>(opt =>
{
    var cs = builder.Configuration.GetConnectionString("MySql");
    opt.UseMySql(cs, ServerVersion.AutoDetect(cs));
    opt.EnableDetailedErrors();
    opt.EnableSensitiveDataLogging();                    // solo DEV
    opt.LogTo(Console.WriteLine, LogLevel.Information);  // SQL y eventos EF
});

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// CORS (desarrollo)
builder.Services.AddCors(opt =>
{
    opt.AddPolicy("open", p => p.AllowAnyOrigin().AllowAnyHeader().AllowAnyMethod());
});

var app = builder.Build();

app.UseSwagger();
app.UseSwaggerUI();
app.UseCors("open");

if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
}

// ===== Middleware para capturar errores no controlados =====
app.Use(async (ctx, next) =>
{
    try { await next(); }
    catch (Exception ex)
    {
        Console.Error.WriteLine($"[UNHANDLED] {ex}");
        ctx.Response.StatusCode = 500;
        await ctx.Response.WriteAsJsonAsync(new { title = "Unhandled error", detail = ex.Message });
    }
});

// ===== Healthcheck =====
app.MapGet("/health/db", async (UmgContext db) =>
{
    try
    {
        var ok = await db.Database.CanConnectAsync();
        return ok ? Results.Ok("db ok") : Results.Problem("db no conecta");
    }
    catch (Exception ex)
    {
        return Results.Problem(ex.ToString());
    }
});

// ===== Utilidades =====
bool CarneOk(string? c) =>
    !string.IsNullOrWhiteSpace(c) &&
    Regex.IsMatch(c!, @"^E(00[1-9]|0[1-9][0-9]|[1-9][0-9]{2})$");

int? ToIntOrNull(object? v)
{
    if (v is null) return null;
    if (v is int i) return i;
    if (int.TryParse(v.ToString(), out var j)) return j;
    return null;
}

// ===== Tipos de sangre =====
app.MapGet("/api/tipos-sangre", async (UmgContext db, ILogger<Program> log) =>
{
    log.LogInformation("GET /api/tipos-sangre");
    return Results.Ok(await db.TiposSangre.ToListAsync());
});

// ===== Estudiantes: listado (LEFT JOIN con nombre de tipo) =====
app.MapGet("/api/estudiantes", async (UmgContext db, ILogger<Program> log) =>
{
    log.LogInformation("GET /api/estudiantes");
    var lista = await db.Estudiantes
        .GroupJoin(
            db.TiposSangre,
            e => e.Id_Tipo_Sangre,
            t => t.Id_Tipo_Sangre,
            (e, ts) => new { e, t = ts.FirstOrDefault() }
        )
        .Select(x => new
        {
            id_Estudiante      = x.e.Id_Estudiante,
            carne              = x.e.Carne,
            nombres            = x.e.Nombres,
            apellidos          = x.e.Apellidos,
            direccion          = x.e.Direccion,
            telefono           = x.e.Telefono,
            correo_Electronico = x.e.Correo_Electronico,
            id_Tipo_Sangre     = x.e.Id_Tipo_Sangre,
            fecha_Nacimiento   = x.e.Fecha_Nacimiento,
            tipoSangre         = x.t != null ? new { sangre = x.t.Sangre } : null
        })
        .ToListAsync();

    return Results.Ok(lista);
});

// ===== Estudiante por id =====
app.MapGet("/api/estudiantes/{id:int}", async (int id, UmgContext db, ILogger<Program> log) =>
{
    log.LogInformation("GET /api/estudiantes/{Id}", id);
    var item = await db.Estudiantes
        .GroupJoin(
            db.TiposSangre,
            e => e.Id_Tipo_Sangre,
            t => t.Id_Tipo_Sangre,
            (e, ts) => new { e, t = ts.FirstOrDefault() }
        )
        .Where(x => x.e.Id_Estudiante == id)
        .Select(x => new
        {
            id_Estudiante      = x.e.Id_Estudiante,
            carne              = x.e.Carne,
            nombres            = x.e.Nombres,
            apellidos          = x.e.Apellidos,
            direccion          = x.e.Direccion,
            telefono           = x.e.Telefono,
            correo_Electronico = x.e.Correo_Electronico,
            id_Tipo_Sangre     = x.e.Id_Tipo_Sangre,
            fecha_Nacimiento   = x.e.Fecha_Nacimiento,
            tipoSangre         = x.t != null ? new { sangre = x.t.Sangre } : null
        })
        .FirstOrDefaultAsync();

    return item is null ? Results.NotFound("Estudiante no encontrado.") : Results.Ok(item);
});

// ===== Crear =====
app.MapPost("/api/estudiantes", async (Estudiante e, UmgContext db, ILogger<Program> log) =>
{
    log.LogInformation("POST /api/estudiantes BODY: {@e}", e);
    try
    {
        if (!CarneOk(e.Carne)) return Results.BadRequest("Carné inválido (E001..E999).");

        var ts = ToIntOrNull(e.Id_Tipo_Sangre);
        if (ts is null || ts <= 0) return Results.BadRequest("id_Tipo_Sangre inválido.");
        var tipoOk = await db.TiposSangre.AnyAsync(t => t.Id_Tipo_Sangre == ts);
        if (!tipoOk) return Results.BadRequest("Tipo de sangre no existe.");
        e.Id_Tipo_Sangre = ts.Value;

        if (await db.Estudiantes.AnyAsync(x => x.Carne == e.Carne))
            return Results.BadRequest("El carné ya está usado por otro estudiante.");

        db.Estudiantes.Add(e);
        await db.SaveChangesAsync();
        return Results.Created($"/api/estudiantes/{e.Id_Estudiante}", e);
    }
    catch (DbUpdateException ex)
    {
        var msg = ex.InnerException?.Message ?? ex.Message;
        return Results.BadRequest($"DB error (POST): {msg}");
    }
    catch (Exception ex)
    {
        return Results.BadRequest($"Unhandled (POST): {ex.GetBaseException().Message}");
    }
});

// ===== Actualizar (parcial/total; acepta ID **o NOMBRE** de tipo de sangre) =====
app.MapPut("/api/estudiantes/{id:int}", async (int id, HttpRequest req, UmgContext db, ILogger<Program> log) =>
{
    log.LogInformation("PUT /api/estudiantes/{{Id}} (entrando) id={Id}", id);
    try
    {
        // Leer y deserializar el body dentro del try para capturar errores de parseo
        var e = await req.ReadFromJsonAsync<Estudiante>();
        if (e is null) return Results.BadRequest("Body inválido o vacío.");

        log.LogInformation("PUT /api/estudiantes/{Id} BODY: {@e}", id, e);

        var current = await db.Estudiantes.FindAsync(id);
        if (current is null) return Results.BadRequest("No existe el estudiante a actualizar.");

        // --- CARNE (si viene) ---
        if (!string.IsNullOrWhiteSpace(e.Carne))
        {
            if (!CarneOk(e.Carne)) return Results.BadRequest("Carné inválido (E001..E999).");

            if (!string.Equals(e.Carne, current.Carne, StringComparison.OrdinalIgnoreCase))
            {
                var carneUsado = await db.Estudiantes
                    .AnyAsync(x => x.Carne == e.Carne && x.Id_Estudiante != id);
                if (carneUsado) return Results.BadRequest("El carné ya está usado por otro estudiante.");
                current.Carne = e.Carne;
            }
        }

        // --- CAMPOS TEXTO (si vienen) ---
        if (!string.IsNullOrWhiteSpace(e.Nombres))             current.Nombres = e.Nombres;
        if (!string.IsNullOrWhiteSpace(e.Apellidos))           current.Apellidos = e.Apellidos;
        if (!string.IsNullOrWhiteSpace(e.Direccion))           current.Direccion = e.Direccion;
        if (!string.IsNullOrWhiteSpace(e.Telefono))            current.Telefono = e.Telefono;
        if (!string.IsNullOrWhiteSpace(e.Correo_Electronico))  current.Correo_Electronico = e.Correo_Electronico;

        // --- FECHA (si viene) ---
        if (e.Fecha_Nacimiento.HasValue) current.Fecha_Nacimiento = e.Fecha_Nacimiento;

        // --- TIPO DE SANGRE: por ID o por NOMBRE ---
        int? tsMaybe = ToIntOrNull(e.Id_Tipo_Sangre);
        string? tsNombre = e.TipoSangre?.Sangre;

        if (string.IsNullOrWhiteSpace(tsNombre))
        {
            if (req.HasFormContentType && req.Form.ContainsKey("tipoSangreNombre"))
                tsNombre = req.Form["tipoSangreNombre"].ToString();
        }

        if (tsMaybe.HasValue)
        {
            var ts = tsMaybe.Value;
            if (ts <= 0) return Results.BadRequest("id_Tipo_Sangre inválido.");
            var tipoOk = await db.TiposSangre.AnyAsync(t => t.Id_Tipo_Sangre == ts);
            if (!tipoOk) return Results.BadRequest("Tipo de sangre no existe.");
            current.Id_Tipo_Sangre = ts;
        }
        else if (!string.IsNullOrWhiteSpace(tsNombre))
        {
            var tipo = await db.TiposSangre
                .FirstOrDefaultAsync(t => t.Sangre == tsNombre!.Trim());
            if (tipo is null)
                return Results.BadRequest("Tipo de sangre (por nombre) no existe.");
            current.Id_Tipo_Sangre = tipo.Id_Tipo_Sangre;
        }

        await db.SaveChangesAsync();
        return Results.NoContent();
    }
    catch (DbUpdateException ex)
    {
        var msg = ex.InnerException?.Message ?? ex.Message;
        log.LogError(ex, "DB error en PUT id={Id}", id);
        return Results.BadRequest($"DB error (PUT): {msg}");
    }
    catch (Exception ex)
    {
        log.LogError(ex, "Unhandled en PUT id={Id}", id);
        return Results.BadRequest($"Unhandled (PUT): {ex.GetBaseException().Message}");
    }
});

// ===== Eliminar =====
app.MapDelete("/api/estudiantes/{id:int}", async (int id, UmgContext db, ILogger<Program> log) =>
{
    log.LogInformation("DELETE /api/estudiantes/{Id}", id);
    var existing = await db.Estudiantes.FindAsync(id);
    if (existing is null) return Results.NotFound("Estudiante no encontrado.");
    db.Estudiantes.Remove(existing);
    await db.SaveChangesAsync();
    return Results.NoContent();
});

app.Run();