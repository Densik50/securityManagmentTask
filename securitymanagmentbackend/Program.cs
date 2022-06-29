using Microsoft.EntityFrameworkCore;

void Automatizer(SecDb db)
{

    System.Threading.Thread.Sleep(5000);
    Automatizer(db);
}

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddDbContext<SecDb>(opt => opt.UseInMemoryDatabase("SecurityDevices"));
builder.Services.AddDatabaseDeveloperPageExceptionFilter();
var app = builder.Build();

app.MapGet("/sdevices", async (SecDb db) =>
    await db.Sdevices.ToListAsync());

app.MapGet("/sdevices/{id}", async (int id, SecDb db) =>
    await db.Sdevices.FindAsync(id)
        is Sdevice sdevice
            ? Results.Ok(sdevice)
            : Results.NotFound());

app.MapGet("/sdevices/{id}/open", async (int id, SecDb db) =>
{
    var sdevice = await db.Sdevices.FindAsync(id);

    if (sdevice is null) return Results.NotFound();



    if(sdevice.Type != "Doors") { 
        if(sdevice.Type != "Window")
        {
            return Results.BadRequest();
        }    
    }
    if (sdevice.State != "Closed" || sdevice.isAutomatic == true) return Results.BadRequest();

    sdevice.State = "Opening";
    await db.SaveChangesAsync();
    while (sdevice.Value < 100)
    {
        sdevice.Value += 1;
        System.Threading.Thread.Sleep(50);
        await db.SaveChangesAsync();
    }
    sdevice.State = "Open";
    sdevice.Value = 100;
    await db.SaveChangesAsync();
    
    return Results.NoContent();
});

app.MapGet("/sdevices/{id}/close", async (int id, SecDb db) =>
{
    var sdevice = await db.Sdevices.FindAsync(id);

    if (sdevice is null) return Results.NotFound();

    if (sdevice.Type != "Doors")
    {
        if (sdevice.Type != "Window")
        {
            return Results.BadRequest();
        }
    }
    if (sdevice.State != "Open" || sdevice.isAutomatic == true) return Results.BadRequest();

    sdevice.State = "Closing";
    await db.SaveChangesAsync();
    while (sdevice.Value > 0)
    {
        sdevice.Value -= 1;
        System.Threading.Thread.Sleep(50);
        await db.SaveChangesAsync();
    }
    sdevice.State = "Closed";
    sdevice.Value = 0;
    await db.SaveChangesAsync();

    return Results.NoContent();
});

app.MapGet("/sdevices/{id}/move_to/{floor}", async (int id, int floor, SecDb db) =>
{
    var sdevice = await db.Sdevices.FindAsync(id);

    if (sdevice is null) return Results.NotFound();

    if (sdevice.Type != "Elevator" || sdevice.State == "Moving" || sdevice.isAutomatic == true) return Results.BadRequest();

    sdevice.State = "Moving";
    
    await db.SaveChangesAsync();
    while (sdevice.Value != floor)
    {
        if(sdevice.Value > floor)
        {
            sdevice.Value--;
        } else
        {
            sdevice.Value++;
        }
        System.Threading.Thread.Sleep(1000);
        await db.SaveChangesAsync();
    }
    sdevice.State = "Stationary";
    await db.SaveChangesAsync();

    return Results.NoContent();
});

app.MapPost("/sdevices", async (Sdevice sdevice, SecDb db) =>
{
    db.Sdevices.Add(sdevice);
    await db.SaveChangesAsync();

    return Results.Created($"/sdevices/{sdevice.Id}", sdevice);
});

app.MapPut("/sdevices/{id}", async (int id, Sdevice inputSdevice, SecDb db) =>
{
    var sdevice = await db.Sdevices.FindAsync(id);

    if (sdevice is null) return Results.NotFound();

    if(sdevice.Type == "Window" || sdevice.Type == "Doors")
    {
        if (sdevice.State != "Closed" || sdevice.State != "Open") return Results.BadRequest();
    } else if(sdevice.Type == "Elevator")
    {
        if (sdevice.Type != "Stationary") return Results.BadRequest(); 
    }

    sdevice.Name = inputSdevice.Name;
    sdevice.Type = inputSdevice.Type;
    sdevice.Value = inputSdevice.Value;
    sdevice.isAutomatic = inputSdevice.isAutomatic;

    await db.SaveChangesAsync();

    return Results.NoContent();
});

app.MapDelete("/sdevices/{id}", async (int id, SecDb db) =>
{
    if (await db.Sdevices.FindAsync(id) is Sdevice sdevice)
    {
        db.Sdevices.Remove(sdevice);
        await db.SaveChangesAsync();
        return Results.Ok(sdevice);
    }

    return Results.NotFound();
});

app.Run();

class Sdevice
{
    public int Id { get; set; }
    public string? Name { get; set; }
    public string? Type { get; set; }
    public int? Value { get; set; }
    public string? State { get; set; }
    public Boolean? isAutomatic { get; set; }
}

class SecDb : DbContext
{
    public SecDb(DbContextOptions<SecDb> options)
        : base(options) { }

    public DbSet<Sdevice> Sdevices => Set<Sdevice>();
}