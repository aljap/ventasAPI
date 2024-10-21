using System.Text.Json.Serialization;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContext<SalesDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultCOnnection")));

// Add services to the container.
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.MapGet("/companies", async (SalesDbContext db) =>
{
    return await db.Companies.ToListAsync();

});
app.MapGet("/companies/{id}", async (int id, SalesDbContext db) =>
{
    return await db.Companies.FindAsync(id) is Company company ? Results.Ok(company) : Results.NotFound();
});
app.MapPost("/companies", async (Company company, SalesDbContext db) =>
{
    db.Companies.Add(company);
    await db.SaveChangesAsync();
    return Results.Created($"/companies/{company.Id}", company);

});
app.MapPut("/companies/{id}", async (int id, Company inputCompany, SalesDbContext db) =>
{
    var company = await db.Companies.FindAsync(id);

    if (company is null) return Results.NotFound();

    company.Name = inputCompany.Name;
    company.Address = inputCompany.Address;
    company.PhoneNumber = inputCompany.PhoneNumber;

    await db.SaveChangesAsync();
    return Results.NoContent();

});
app.MapDelete("/companies/{id}", async (int id, SalesDbContext db) =>
{
    var companyDelete = await db.Companies.FindAsync(id);

    if (companyDelete is null) return Results.NotFound();

    db.Companies.Remove(companyDelete);
    await db.SaveChangesAsync();
    return Results.NoContent();

});
app.MapGet("/employees", async (SalesDbContext db) =>
{
    return await db.Employees.ToListAsync();

});
app.MapGet("/employees/{id}", async (int id, SalesDbContext db) =>
{
    return await db.Employees.FindAsync(id) is Employee article ? Results.Ok(article) : Results.NotFound();
});
app.MapPost("/employees", async (Employee employee, SalesDbContext db) =>
{
    // Validate Employee Name
    if (string.IsNullOrWhiteSpace(employee.Name))
    {
        return Results.BadRequest("Employee name is required.");
    }

    // Validate Salary (for example, it must be greater than 0)
    if (employee.Salary <= 0)
    {
        return Results.BadRequest("Salary must be greater than 0.");
    }

    // Validate CompanyId
    var companyExists = await db.Companies.AnyAsync(c => c.Id == employee.CompanyId);
    if (!companyExists)
    {
        return Results.BadRequest("Invalid CompanyId, the company does not exist.");
    }

    // Add employee to database
    db.Employees.Add(employee);
    await db.SaveChangesAsync();

    // Return Created result with the new employee resource location
    return Results.Created($"/employees/{employee.Id}", employee);
});
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

});
app.MapDelete("/employees/{id}", async (int id, SalesDbContext db) =>
{
    var employeeDelete = await db.Employees.FindAsync(id);

    if (employeeDelete is null) return Results.NotFound();

    db.Employees.Remove(employeeDelete);
    await db.SaveChangesAsync();
    return Results.NoContent();

});
app.MapGet("/articles", async (SalesDbContext db) =>
{
    return await db.Articles.ToListAsync();
});
app.MapGet("/articles/{id}", async (int id, SalesDbContext db) =>
{
    return await db.Articles.FindAsync(id) is Article article ? Results.Ok(article) : Results.NotFound();
});
app.MapPost("/articles", async (Article article, SalesDbContext db) =>
{
    db.Articles.Add(article);
    await db.SaveChangesAsync();
    return Results.Created($"/articles/{article.Id}", article);
});
app.MapPut("/articles/{id}", async (int id, Article inputArticle, SalesDbContext db) =>
{
    var article = await db.Articles.FindAsync(id);

    if (article is null) return Results.NotFound();

    article.Name = inputArticle.Name;
    article.Value = inputArticle.Value;
    article.Company = inputArticle.Company;


    await db.SaveChangesAsync();
    return Results.NoContent();

});
app.MapDelete("/articles/{id}", async (int id, SalesDbContext db) =>
{
    var articleDelete = await db.Employees.FindAsync(id);

    if (articleDelete is null) return Results.NotFound();

    db.Employees.Remove(articleDelete);
    await db.SaveChangesAsync();
    return Results.NoContent();

});
app.MapGet("/orders", async (SalesDbContext db) =>
{
    return await db.Orders.ToListAsync();
});
app.MapGet("/orders/{id}", async (int id, SalesDbContext db) =>
{
    return await db.Orders.FindAsync(id) is Order order ? Results.Ok(order) : Results.NotFound();
});
app.MapPost("/orders", async (Order order, SalesDbContext db) =>
{
    db.Orders.Add(order);
    await db.SaveChangesAsync();
    return Results.Created($"/orders/{order.Id}", order);
});
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

});
app.MapDelete("/orders/{id}", async (int id, SalesDbContext db) =>
{
    var ordersDelete = await db.Orders.FindAsync(id);

    if (ordersDelete is null) return Results.NotFound();

    db.Orders.Remove(ordersDelete);
    await db.SaveChangesAsync();
    return Results.NoContent();

});
app.MapGet("/ordersDetails", async (SalesDbContext db) =>
{
    return await db.OrderDetails.ToListAsync();
});
app.MapGet("/ordersDetails/{id}", async (int id, SalesDbContext db) =>
{
    return await db.OrderDetails.FindAsync(id) is OrderDetail orderDetail ? Results.Ok(orderDetail) : Results.NotFound();
});
app.MapPost("/ordersDetails", async (OrderDetail orderDetail, SalesDbContext db) =>
{
    db.OrderDetails.Add(orderDetail);
    await db.SaveChangesAsync();
    return Results.Created($"/ordersDetails/{orderDetail.Id}", orderDetail);
});
app.MapPut("/ordersDetails/{id}", async (int id, OrderDetail inputOrderDetail, SalesDbContext db) =>
{
    var orderDetail = await db.OrderDetails.FindAsync(id);

    if (orderDetail is null) return Results.NotFound();

    orderDetail.ArticleId = inputOrderDetail.ArticleId;
    orderDetail.OrderId = inputOrderDetail.OrderId;


    await db.SaveChangesAsync();
    return Results.NoContent();

});
app.MapDelete("/ordersDetails/{id}", async (int id, SalesDbContext db) =>
{
    var ordersDetailsDelete = await db.OrderDetails.FindAsync(id);

    if (ordersDetailsDelete is null) return Results.NotFound();

    db.OrderDetails.Remove(ordersDetailsDelete);
    await db.SaveChangesAsync();
    return Results.NoContent();

});
app.MapGet("/invoices", async (SalesDbContext db) =>
{
    return await db.Invoices.ToListAsync();
});
app.MapGet("/invoices/{id}", async (int id, SalesDbContext db) =>
{
    return await db.Invoices.FindAsync(id) is Invoice invoice ? Results.Ok(invoice) : Results.NotFound();
});
app.MapPost("/invoices", async (Invoice invoice, SalesDbContext db) =>
{
    db.Invoices.Add(invoice);
    await db.SaveChangesAsync();
    return Results.Created($"/invoices/{invoice.Id}", invoice);
});
app.MapPut("/invoices/{id}", async (int id, Invoice inputInvoice, SalesDbContext db) =>
{
    var invoice = await db.Invoices.FindAsync(id);

    if (invoice is null) return Results.NotFound();

    invoice.DeliveryDate = inputInvoice.DeliveryDate;
    invoice.OrderId = inputInvoice.OrderId;
    invoice.Status = inputInvoice.Status;


    await db.SaveChangesAsync();
    return Results.NoContent();

});
app.MapDelete("/invoices/{id}", async (int id, SalesDbContext db) =>
{
    var invoiceDelete = await db.Invoices.FindAsync(id);

    if (invoiceDelete is null) return Results.NotFound();

    db.Invoices.Remove(invoiceDelete);
    await db.SaveChangesAsync();
    return Results.NoContent();

});


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

public class SalesDbContext : DbContext
{
    public DbSet<Company> Companies { get; set; }
    public DbSet<Employee> Employees { get; set; }
    public DbSet<Article> Articles { get; set; }
    public DbSet<Order> Orders { get; set; }
    public DbSet<OrderDetail> OrderDetails { get; set; }
    public DbSet<Invoice> Invoices { get; set; }
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
