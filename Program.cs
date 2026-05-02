
using System;
using System.Numerics;
using System.Text.RegularExpressions;
using Azure;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting.Server;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace MedAppApi
{
    public class Program
    {
        static Regex reDate = new Regex("^(?<year>[0-9]{2,2}|[0-9]{4,4})-(?<month>[0-9]{1,2})-(?<day>[0-9]{1,2})",RegexOptions.Compiled);
        public static void Main( string[] args )
        {
            var builder = WebApplication.CreateBuilder(args);

            // Add services to the container.
            builder.Services.AddAuthorization();

            // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen();
            //builder.Services.AddOpenApi("internal");

            // This tells the framework: "AppDbContext is a Service, NOT something to look for in the body"
            builder.Services.AddDbContext<AppDbContext>( options =>
                options.UseSqlServer( builder.Configuration.GetConnectionString( "DefaultConnection" ) ) );

      

            // Add CORS services
            builder.Services.AddCors( options =>
            {
                options.AddPolicy( "AllowFrontend", policy =>
                {
                    policy.AllowAnyOrigin()
                    //policy.WithOrigins(
                    //"http://dandland.com",
                    //"http://medapp.dandland.com",
                    //"http://medappapi.dandland.com",
                    //"http://localhost:3000",
                    //"http://localhost:5173",
                    //"http://localhost:5063/",
                    //"http://localhost:5063/swagger/",
                    //"http://localhost:5063/swagger",
                    //"http://localhost:5063/swagger/index.html",
                    //"localhost:5063",
                    //"http://localhost:5063",
                    //"[::1]:5063",
                    //"http://localhost:5063/meds",
                    //"http://localhost:5063/favicon.ico")
                    .AllowAnyHeader()
                    .AllowAnyMethod();
                } );
            } );

            var app = builder.Build();

            // GLOBAL GUARD
            app.Use( async ( context, next ) =>
            {
                Console.WriteLine("In Use...");
                const string HeaderName = "X-DandlandOnly"; // Name of header
                const string SecretValue = "dandlandonly";        // Value to check

                var referer = context.Request.Headers["Referer"].ToString();

                // Check if the request is originating from the swagger page
                bool isSwagger = referer.Contains("/swagger/index.html") ||
                     context.Request.Path.StartsWithSegments("/swagger");

                if(isSwagger)
                {
                    Console.WriteLine("Swagger client...  Don't block");
                    await next(); // Let the request continue to your MapGet/MapPost    
                    return;
                }

                if(HttpMethods.IsOptions( context.Request.Method ))
                {
                    await next();
                    return;
                }

                Console.WriteLine($"{context.Request.Headers.Origin}");
//
//                  if( context.Request.Headers.Origin == "http://localhost:3000")
//                  {
//                      Console.WriteLine("Localhost... ok");
//                      await next();
//                      return; 
//                  }
//  


                if(!context.Request.Headers.TryGetValue( HeaderName, out var extractedValue ) ||
                    extractedValue != SecretValue)
                {
                    Console.WriteLine("Ooops!  Big trouble Little china...");
                    Console.WriteLine($"{HeaderName} == [{extractedValue}]");
                    context.Response.StatusCode = 401;
                    await context.Response.WriteAsync( "Unauthorized: Missing or invalid secret." );
                    return; // Stop the request here!
                }

                if(context.Request.Headers.Origin == "http://localhost:3000")
                {
                    Console.WriteLine( "Localhost... ok" );
                    await next();
                    return;
                }



                Console.WriteLine("Made it through use...");
                await next(); // Let the request continue to your MapGet/MapPost
            } );


            app.UseCors("AllowFrontend");

            // Configure the HTTP request pipeline.
            if(app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI();
            //    app.MapOpenApi("/docs/internal.json");
            }

            app.UseAuthorization();

            // Change this line:
            app.MapPost( "/meds", async ([FromBody] MedTable med,  AppDbContext db ) =>
            {
                db.MedTable.Add( med );
                await db.SaveChangesAsync();
                return Results.Created( $"/meds/{med.ID}", med );
            } );

            app.MapGet( "/meds/all", async ( AppDbContext db ) =>
            {
                return await db.MedTable.ToListAsync();
            });



            //Read
            app.MapGet( "/meds/{dateValue?}", async ( string? dateValue, AppDbContext db ) =>
            {   
                DateTime dtSearchDate = DateTime.Now;

                if( string.IsNullOrWhiteSpace(dateValue))
                {
                    dateValue = "";
                }

                Match m = reDate.Match(dateValue) ;

                if(m.Success)
                {
                    string year     = m.Groups["year"].Value;
                    string month    = m.Groups["month"].Value;
                    string day      = m.Groups["day"].Value;

                    if( year.Length < 3)
                    {
                        year = "20" + year; 
                    }
                    dateValue = $"{year}-{month}-{day}";
                    dtSearchDate = new DateTime(int.Parse(year),int.Parse(month),int.Parse(day));
                }
                
                if( !m.Success)
                {
                    if( !string.IsNullOrWhiteSpace(dateValue) )
                    {
                        return null;
                    }
                }

                MedTable t = await db.MedTable.FirstOrDefaultAsync<MedTable>( x =>
                    x.MedDate.Year  == dtSearchDate.Year &&
                    x.MedDate.Day   == dtSearchDate.Day  &&
                    x.MedDate.Month == dtSearchDate.Month );

                if(t == null)
                {
                    t =  new MedTable();
                    t.Description = "Take meds...";
                    t.MedDate =dtSearchDate;
                    t.am= false;
                    t.pm=false; 
                    if( string.IsNullOrWhiteSpace(dateValue) )
                    {                        
                        return null;
                    }
                    db.MedTable.Add( t );
                    try
                    {

                        db.SaveChanges();
                    }
                    catch(Exception ex)
                    {
                        return null; 
                    }
                }

                return t;
            } );

            // Update
            app.MapPut( "/meds", async ([FromBody] MedTable input, AppDbContext db ) =>
            {
                try
                {
                    MedTable med = await db.MedTable.FindAsync(input.ID);
                    if(med is null)
                    {
                        return Results.NotFound();
                    }
                    med.Description = input.Description;
                    med.am = input.am;
                    med.pm = input.pm;
                    await db.SaveChangesAsync();
                    return Results.NoContent();
                }
                catch
                {
                    return Results.BadRequest("Bad mojo");
                }
            } );
                  
            // Delete
            app.MapDelete( "/meds/{id}", async ( int id, AppDbContext db ) =>
            {
                var med = await db.MedTable.FindAsync(id);
                if(med is null)
                {
                    return Results.NotFound();
                }

                db.MedTable.Remove( med );
                await db.SaveChangesAsync();
                return Results.NoContent();
            } );
               
            app.MapGet( "/ping",  ( ) =>
            {
                string s= DateTime.Now.ToString();
                return s;
            } );

            app.Run();

        }
    }
}
