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
    automapper.CreateMap<PlanningRoomModel, PlanningRoom>().ReverseMap();
    automapper.CreateMap<EstimateValueModel, EstimateValue>().ReverseMap();
    automapper.CreateMap<EstimateValueCategoryModel, EstimateValueCategory>().ReverseMap();
    automapper.CreateMap<PlanningRoomUsers, PlanningRoomUserModel>().ForAllMembers(a => a.MapFrom(b => b.User));
    automapper.CreateMap<AspNetUsers, PlanningRoomUserModel>();
    automapper.CreateMap<ProductBacklogItemModel, ProductBacklogItem>().ReverseMap();
    automapper.CreateMap<ProductBacklogItemStatusModel, ProductBacklogItemStatus>().ReverseMap();
}, typeof(PlanningPokerContext).Assembly);

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

app.MapFallbackToFile("index.html");

app.Run();
