
using System;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Text.RegularExpressions;
using System.Numerics;

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
                    .AllowAnyHeader()
                    .AllowAnyMethod();
                } );
            } );

            var app = builder.Build();             
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
