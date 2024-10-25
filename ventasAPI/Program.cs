using System.Text.Json.Serialization;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using System.Security.Claims;
using System.IdentityModel.Tokens.Jwt;
using Microsoft.OpenApi.Models;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContext<SalesDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultCOnnection")));

// Add services to the container.
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();

builder.Services.AddAuthentication(options =>
    {
        options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
        options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
    })
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = "yourdomain.com",
            ValidAudience = "yourdomain.com",
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes("vainitaOMGclavelargaysegura_a234243423423awda"))
        };
    });
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "Mi API con JWT", Version = "v1" });

    // Configuración para agregar el token JWT en Swagger
    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        Scheme = "Bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "Introduzca 'Bearer' [espacio] seguido de su token JWT."
    });

    c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            new string[] {}
        }
    });
});
builder.Services.AddAuthorization();
var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();

string GenerateJwtToken()
{
    var claims = new[]
    {
        new Claim(JwtRegisteredClaimNames.Sub, "test"),
        new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
        new Claim("User","Mi usuario")
    };

    var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes("vainitaOMGclavelargaysegura_a234243423423awda"));
    var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

    var token = new JwtSecurityToken(
        issuer: "yourdomain.com",
        audience: "yourdomain.com",
        claims: claims,
        expires: DateTime.Now.AddMinutes(30),
        signingCredentials: creds);

    return new JwtSecurityTokenHandler().WriteToken(token);
}

app.UseWhen(context => context.Request.Path.StartsWithSegments("/theone"), (appBuilder) =>
{
    appBuilder.Use(async (context, next) =>
    {
        if (context.Request.Headers.ContainsKey("Key") && context.Request.Headers["Key"].ToString() == "vainitaOMG")
        {
            await next();
        }
        else
        {
            context.Response.StatusCode = 400;
            await context.Response.WriteAsync("Falta el header X-Custom-Header.");
        }
    });
});

app.MapPost("/login", async (UserLogin login, SalesDbContext db) =>
{
    var user = await db.UserLogins.FirstOrDefaultAsync(u => u.Username == login.Username && u.Password == login.Password);
    if (user != null)
    {
        var token = GenerateJwtToken();
        return Results.Ok(new { token });
    }
    return Results.Unauthorized();
});

app.MapGet("/companies", async (SalesDbContext db) =>
{
    return await db.Companies.ToListAsync();

}).RequireAuthorization();
app.MapGet("/companies/{id}", async (int id, SalesDbContext db) =>
{
    return await db.Companies.FindAsync(id) is Company company ? Results.Ok(company) : Results.NotFound();
}).RequireAuthorization();
app.MapPost("/companies", async (Company company, SalesDbContext db) =>
{
    db.Companies.Add(company);
    await db.SaveChangesAsync();
    return Results.Created($"/companies/{company.Id}", company);

}).RequireAuthorization();
app.MapPut("/companies/{id}", async (int id, Company inputCompany, SalesDbContext db) =>
{
    var company = await db.Companies.FindAsync(id);

    if (company is null) return Results.NotFound();

    company.Name = inputCompany.Name;
    company.Address = inputCompany.Address;
    company.PhoneNumber = inputCompany.PhoneNumber;

    await db.SaveChangesAsync();
    return Results.NoContent();

}).RequireAuthorization();
app.MapDelete("/companies/{id}", async (int id, SalesDbContext db) =>
{
    var companyDelete = await db.Companies.FindAsync(id);

    if (companyDelete is null) return Results.NotFound();

    db.Companies.Remove(companyDelete);
    await db.SaveChangesAsync();
    return Results.NoContent();

}).RequireAuthorization();
app.MapGet("/employees", async (SalesDbContext db) =>
{
    return await db.Employees.ToListAsync();

}).RequireAuthorization();
app.MapGet("/employees/{id}", async (int id, SalesDbContext db) =>
{
    return await db.Employees.FindAsync(id) is Employee article ? Results.Ok(article) : Results.NotFound();
}).RequireAuthorization();
app.MapPost("/employees", async (Employee employee, SalesDbContext db) =>
{

    if (string.IsNullOrWhiteSpace(employee.Name))
    {
        return Results.BadRequest("Employee name is required.");
    }


    if (employee.Salary <= 0)
    {
        return Results.BadRequest("Salary must be greater than 0.");
    }


    var companyExists = await db.Companies.AnyAsync(c => c.Id == employee.CompanyId);
    if (!companyExists)
    {
        return Results.BadRequest("Invalid CompanyId, the company does not exist.");
    }


    db.Employees.Add(employee);
    await db.SaveChangesAsync();


    return Results.Created($"/employees/{employee.Id}", employee);
}).RequireAuthorization();
app.MapPut("/employees/{id}", async (int id, Employee inputEmployee, SalesDbContext db) =>
{
    var employee = await db.Employees.FindAsync(id);

    if (employee is null) return Results.NotFound();

    employee.Name = inputEmployee.Name;
    employee.Salary = inputEmployee.Salary;
    employee.Position = inputEmployee.Position;
    employee.Company = inputEmployee.Company;


    await db.SaveChangesAsync();
    return Results.NoContent();

}).RequireAuthorization();
app.MapDelete("/employees/{id}", async (int id, SalesDbContext db) =>
{
    var employeeDelete = await db.Employees.FindAsync(id);

    if (employeeDelete is null) return Results.NotFound();

    db.Employees.Remove(employeeDelete);
    await db.SaveChangesAsync();
    return Results.NoContent();

}).RequireAuthorization();
app.MapGet("/articles", async (SalesDbContext db) =>
{
    return await db.Articles.ToListAsync();
}).RequireAuthorization();
app.MapGet("/articles/{id}", async (int id, SalesDbContext db) =>
{
    return await db.Articles.FindAsync(id) is Article article ? Results.Ok(article) : Results.NotFound();
}).RequireAuthorization();
app.MapPost("/articles", async (Article article, SalesDbContext db) =>
{
    db.Articles.Add(article);
    await db.SaveChangesAsync();
    return Results.Created($"/articles/{article.Id}", article);
}).RequireAuthorization();
app.MapPut("/articles/{id}", async (int id, Article inputArticle, SalesDbContext db) =>
{
    var article = await db.Articles.FindAsync(id);

    if (article is null) return Results.NotFound();

    article.Name = inputArticle.Name;
    article.Value = inputArticle.Value;
    article.Company = inputArticle.Company;


    await db.SaveChangesAsync();
    return Results.NoContent();

}).RequireAuthorization();
app.MapDelete("/articles/{id}", async (int id, SalesDbContext db) =>
{
    var articleDelete = await db.Employees.FindAsync(id);

    if (articleDelete is null) return Results.NotFound();

    db.Employees.Remove(articleDelete);
    await db.SaveChangesAsync();
    return Results.NoContent();

}).RequireAuthorization();
app.MapGet("/orders", async (SalesDbContext db) =>
{
    return await db.Orders.ToListAsync();
}).RequireAuthorization();
app.MapGet("/orders/{id}", async (int id, SalesDbContext db) =>
{
    return await db.Orders.FindAsync(id) is Order order ? Results.Ok(order) : Results.NotFound();
}).RequireAuthorization();
app.MapPost("/orders", async (OrderRequest request, SalesDbContext db) =>
{

    var order = request.Order;
    var orderDetails = request.OrderDetails;
    if (order == null || orderDetails == null)
    {
        return Results.BadRequest("Bad request");
    }

    if (!orderDetails.Any())
    {
        return Results.BadRequest("An order must have at least one associated article");
    }
    var employee = await db.Employees.FindAsync(order.EmployeeId);
    if (employee is null) return Results.NotFound("Employee not found");

    foreach (var detail in orderDetails)
    {
        var article = await db.Articles.FindAsync(detail.ArticleId);
        if (article is null)
        {
            return Results.BadRequest("Article not found.");
        }

        if (article.CompanyId != employee.CompanyId)
        {
            return Results.BadRequest("All articles must belong to the same company as the employee.");
        }
    }

    using var transaction = await db.Database.BeginTransactionAsync();
    try
    {
        db.Orders.Add(order);
        db.OrderDetails.AddRange(orderDetails);
        await db.SaveChangesAsync();
        await transaction.CommitAsync();
    }
    catch
    {
        await transaction.RollbackAsync();
        return Results.Problem("An error occurred while creating the order.", statusCode: 500);
    }

    return Results.Created($"/orders/{order.Id}", order);
}).RequireAuthorization();
app.MapPut("/orders/{id}", async (int id, Order inputOrder, SalesDbContext db) =>
{
    var order = await db.Orders.FindAsync(id);

    if (order is null) return Results.NotFound();

    order.Employee = inputOrder.Employee;
    order.TotalValue = inputOrder.TotalValue;
    order.Status = inputOrder.Status;
    order.Invoice = inputOrder.Invoice;


    await db.SaveChangesAsync();
    return Results.NoContent();

}).RequireAuthorization();
app.MapDelete("/orders/{id}", async (int id, SalesDbContext db) =>
{
    var ordersDelete = await db.Orders.FindAsync(id);

    if (ordersDelete is null) return Results.NotFound();

    db.Orders.Remove(ordersDelete);
    await db.SaveChangesAsync();
    return Results.NoContent();

}).RequireAuthorization();
app.MapGet("/ordersDetails", async (SalesDbContext db) =>
{
    return await db.OrderDetails.ToListAsync();
}).RequireAuthorization();
app.MapGet("/ordersDetails/{id}", async (int id, SalesDbContext db) =>
{
    return await db.OrderDetails.FindAsync(id) is OrderDetail orderDetail ? Results.Ok(orderDetail) : Results.NotFound();
}).RequireAuthorization();
app.MapPost("/ordersDetails", async (OrderDetail orderDetail, SalesDbContext db) =>
{
    db.OrderDetails.Add(orderDetail);
    await db.SaveChangesAsync();
    return Results.Created($"/ordersDetails/{orderDetail.Id}", orderDetail);
}).RequireAuthorization();
app.MapPut("/ordersDetails/{id}", async (int id, OrderDetail inputOrderDetail, SalesDbContext db) =>
{
    var orderDetail = await db.OrderDetails.FindAsync(id);

    if (orderDetail is null) return Results.NotFound();

    orderDetail.ArticleId = inputOrderDetail.ArticleId;
    orderDetail.OrderId = inputOrderDetail.OrderId;


    await db.SaveChangesAsync();
    return Results.NoContent();

}).RequireAuthorization();
app.MapDelete("/ordersDetails/{id}", async (int id, SalesDbContext db) =>
{
    var ordersDetailsDelete = await db.OrderDetails.FindAsync(id);

    if (ordersDetailsDelete is null) return Results.NotFound();

    db.OrderDetails.Remove(ordersDetailsDelete);
    await db.SaveChangesAsync();
    return Results.NoContent();

}).RequireAuthorization();
app.MapGet("/invoices", async (SalesDbContext db) =>
{
    return await db.Invoices.ToListAsync();
}).RequireAuthorization();
app.MapGet("/invoices/{id}", async (int id, SalesDbContext db) =>
{
    return await db.Invoices.FindAsync(id) is Invoice invoice ? Results.Ok(invoice) : Results.NotFound();
}).RequireAuthorization();
/* app.MapPost("/invoices", async (Invoice invoice, SalesDbContext db) =>
{
    db.Invoices.Add(invoice);
    await db.SaveChangesAsync();
    return Results.Created($"/invoices/{invoice.Id}", invoice);
}); */
/* app.MapPut("/invoices/{id}", async (int id, Invoice inputInvoice, SalesDbContext db) =>
{
    var invoice = await db.Invoices.FindAsync(id);

    if (invoice is null) return Results.NotFound();

    invoice.DeliveryDate = inputInvoice.DeliveryDate;
    invoice.OrderId = inputInvoice.OrderId;
    invoice.Status = inputInvoice.Status;


    await db.SaveChangesAsync();
    return Results.NoContent();

}); */
app.MapDelete("/invoices/{id}", async (int id, SalesDbContext db) =>
{
    var invoiceDelete = await db.Invoices.FindAsync(id);

    if (invoiceDelete is null) return Results.NotFound();

    db.Invoices.Remove(invoiceDelete);
    await db.SaveChangesAsync();
    return Results.NoContent();

}).RequireAuthorization();

app.MapPut("/orders/{id}/complete", async (int id, SalesDbContext db) =>
{
    var order = await db.Orders.FindAsync(id);

    if (order is null) return Results.NotFound();


    order.Status = "Completed";
    await db.SaveChangesAsync();


    var invoice = new Invoice
    {
        OrderId = order.Id,
        Status = "Pending",
        DeliveryDate = DateTime.UtcNow.AddDays(7)
    };

    db.Invoices.Add(invoice);
    await db.SaveChangesAsync();

    return Results.Ok(invoice);
}).RequireAuthorization();

app.Run();

public class Company
{
    public int Id { get; set; }
    public required string Name { get; set; }
    public string? Address { get; set; }
    public string? PhoneNumber { get; set; }
    public required ICollection<Employee> Employees { get; set; }
    public ICollection<Article>? Articles { get; set; }
}
public class Employee
{
    public int Id { get; set; }
    public required string Name { get; set; }
    public string? Position { get; set; }
    public required decimal Salary { get; set; }
    public int CompanyId { get; set; }
    [JsonIgnore]
    public Company? Company { get; set; }
    public ICollection<Order>? Orders { get; set; }
}
public class Article
{
    public int Id { get; set; }
    public required string Name { get; set; }
    public required decimal Value { get; set; }
    public int CompanyId { get; set; }
    [JsonIgnore]
    public Company? Company { get; set; }
}

public class Order
{
    public int Id { get; set; }
    public required string Name { get; set; }
    public decimal TotalValue { get; set; }
    public required string Status { get; set; }
    public int EmployeeId { get; set; }
    [JsonIgnore]
    public Employee? Employee { get; set; }
    public ICollection<OrderDetail>? OrderDetails { get; set; }
    public Invoice? Invoice { get; set; }
}

public class OrderDetail
{
    public int Id { get; set; }
    public int ArticleId { get; set; }
    [JsonIgnore]
    public Article? Article { get; set; }
    public int OrderId { get; set; }
    [JsonIgnore]
    public Order? Order { get; set; }
}

public class Invoice
{
    public int Id { get; set; }
    public required string Status { get; set; }
    public DateTime DeliveryDate { get; set; }

    public int OrderId { get; set; }
    [JsonIgnore]
    public Order? Order { get; set; }
}

public class EmployeeDto
{
    public int Id { get; set; }
    public required string Name { get; set; }
    public string? Position { get; set; }
    public required decimal Salary { get; set; }
    public int CompanyId { get; set; }

}
public class OrderRequest
{
    public required Order Order { get; set; }
    public required List<OrderDetail> OrderDetails { get; set; }
}
public class UserLogin
{
    public int Id { get; set; }
    public required string Username { get; set; }
    public required string Password { get; set; }
}


public class SalesDbContext : DbContext
{
    public DbSet<Company> Companies { get; set; }
    public DbSet<Employee> Employees { get; set; }
    public DbSet<Article> Articles { get; set; }
    public DbSet<Order> Orders { get; set; }
    public DbSet<OrderDetail> OrderDetails { get; set; }
    public DbSet<Invoice> Invoices { get; set; }
    public DbSet<UserLogin> UserLogins { get; set; }
    public SalesDbContext(DbContextOptions<SalesDbContext> options) : base(options)
    {
    }


    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {

        base.OnModelCreating(modelBuilder);

        /* modelBuilder.Entity<Article>()
            .Property(a => a.Value)
            .HasPrecision(18, 4); */
    }
}
