using EnsyInc.Loom.Api.Bootstrap;
using EnsyInc.Loom.Api.Middleware;

using FluentValidation;

using Microsoft.AspNetCore.Mvc;

var builder = WebApplication.CreateBuilder(args)
    .InitializeApplication();

builder.Services.AddExceptionHandler<GlobalExceptionHandler>();
builder.Services.AddProblemDetails();
builder.Services.AddValidatorsFromAssemblyContaining<Program>();
builder.Services.Configure<MvcOptions>(opt =>
    opt.SuppressImplicitRequiredAttributeForNonNullableReferenceTypes = true);

var app = builder.Build()
    .ConfigureApplication();

app.RunApplication();
