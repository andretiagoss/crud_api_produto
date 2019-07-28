using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ATSS.API.Data;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ApiExplorer;
using Microsoft.AspNetCore.ResponseCompression;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Swashbuckle.AspNetCore.Swagger;

namespace ATSS.API
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddMvc()
                .SetCompatibilityVersion(CompatibilityVersion.Version_2_2)
                .AddJsonOptions(opt=>{
                    //Remove propriedades nulas do response
                    opt.SerializerSettings.NullValueHandling = Newtonsoft.Json.NullValueHandling.Ignore; 
                })
                .AddXmlSerializerFormatters(); //Habilita o response em formato XML.

            //Adicionando a dependencia do contexto e a informação de que será utilizado o In Memory Database
            services.AddDbContext<ProdutoContexto>(opt=>
                opt.UseInMemoryDatabase(databaseName:"produtoInMemory")
                   .UseQueryTrackingBehavior(QueryTrackingBehavior.NoTracking));

            //Adicionando o midware de injeção de dependencia, da interface para a classe concreta.
            services.AddTransient<IProdutoRepository, ProdutoRepository>();

            //Adicionando o midware de versionamento da Api para aparecer no swagger.
            services.AddVersionedApiExplorer(opt=>
            {
                opt.GroupNameFormat = "'v'VVV";
                opt.SubstituteApiVersionInUrl = true;
            });

            //Adicionando o midware de versionamento de API
            services.AddApiVersioning();

            //Adicionando o midware de Cache
            services.AddResponseCaching();

            //Adicionando o midware de compressão.
            services.AddResponseCompression(opt=>
            {
                //Sem compressão o Json de retorno foi de 11.8kb
                // opt.Providers.Add<GzipCompressionProvider>(); //Com GZip comprimiu o arquivo em 3.2kb
                opt.Providers.Add<BrotliCompressionProvider>(); //Com Brotli comprimiu o arquivo em 1.2kb
                opt.EnableForHttps = true;
            });

            // Register the Swagger generator, defining 1 or more Swagger documents
            services.AddSwaggerGen(c =>
            {
                var provider = services.BuildServiceProvider().GetRequiredService<IApiVersionDescriptionProvider>();

                foreach (var item in provider.ApiVersionDescriptions)
                {
                    c.SwaggerDoc(item.GroupName, new Info 
                    { 
                        Title = $"API de Produtos {item.ApiVersion}", 
                        Version = item.ApiVersion.ToString() 
                    });
                }
            });
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env, IApiVersionDescriptionProvider provider)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }

            app.UseHttpsRedirection();

            //Utilização efetiva do midware de Cache
            app.UseResponseCaching();

            //Utilização efetiva do midware de Compressão
            app.UseResponseCompression();

            // Enable middleware to serve generated Swagger as a JSON endpoint.
            app.UseSwagger();

            // Enable middleware to serve swagger-ui (HTML, JS, CSS, etc.),
            // specifying the Swagger JSON endpoint.
            app.UseSwaggerUI(c =>
            {
                foreach (var item in provider.ApiVersionDescriptions)
                {
                    c.SwaggerEndpoint($"/swagger/{item.GroupName}/swagger.json", item.GroupName);
                }

                c.RoutePrefix = string.Empty;
            });

            app.UseMvc();
        }
    }
}
