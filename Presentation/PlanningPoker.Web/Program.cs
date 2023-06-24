using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI;
using Microsoft.EntityFrameworkCore;
using PlanningPoker.Data;
using PlanningPoker.Domain;
using PlanningPoker.Infrastructure;
using PlanningPoker.Models;
using AutoMapper;
using AutoMapper.EquivalencyExpression;
using PlanningPoker.Web.Models;
using PlanningPoker.Web.SignalrHub;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection") ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(connectionString));
builder.Services.AddDatabaseDeveloperPageExceptionFilter();

builder.Services.AddDefaultIdentity<ApplicationUser>(options => options.SignIn.RequireConfirmedAccount = true)
    .AddEntityFrameworkStores<ApplicationDbContext>();

builder.Services.AddIdentityServer()
    .AddApiAuthorization<ApplicationUser, ApplicationDbContext>();

builder.Services.AddAuthentication()
    .AddIdentityServerJwt();

builder.Services.AddDbContext<PlanningPokerContext>(options =>
    options.UseSqlServer(connectionString));

builder.Services.AddControllersWithViews();
builder.Services.AddRazorPages();

builder.Services.AddAutoMapper((serviceProvider, automapper) =>
{
    automapper.AddCollectionMappers();
    automapper.UseEntityFrameworkCoreModel<PlanningPokerContext>(serviceProvider);
    automapper.CreateMap<PlanningRoom, PlanningRoomDto>();
    automapper.CreateMap<PlanningRoomModel, PlanningRoom>();
    automapper.CreateMap<EstimateValue, EstimateValueDto>();
    automapper.CreateMap<EstimateValueCategory, EstimateValueCategoryDto>();
    automapper.CreateMap<AspNetUsers, PlanningRoomUserDto>();
    automapper.CreateMap<PlanningRoomUsers, PlanningRoomUserDto>()
        .IncludeMembers(p => p.User);

    automapper.CreateMap<ProductBacklogItemModel, ProductBacklogItem>();
    automapper.CreateMap<ProductBacklogItem, ProductBacklogItemDto>();
    automapper.CreateMap<ProductBacklogItemEstimate, ProductBacklogItemEstimateDto>();
    automapper.CreateMap<ProductBacklogItemEstimateModel, ProductBacklogItemEstimate>();

    automapper.CreateMap<ProductBacklogItemStatus, ProductBacklogItemStatusDto>();

}, typeof(PlanningPokerContext).Assembly);

builder.Services.AddSignalR();
builder.Services.AddSingleton<ConnectionManager>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseMigrationsEndPoint();
}
else
{
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();

app.UseAuthentication();
app.UseIdentityServer();
app.UseAuthorization();


app.MapControllerRoute(
    name: "default",
    pattern: "{controller}/{action=Index}/{id?}");
app.MapRazorPages();

app.MapHub<PlanningRoomHub>("/planningRoomHub");

app.MapFallbackToFile("index.html");


app.Run();
