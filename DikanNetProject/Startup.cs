using System;
using System.Threading.Tasks;
using Microsoft.Owin;
using NonFactors.Mvc.Grid;
using System.Web.Mvc;
using Owin;

[assembly: OwinStartup(typeof(DikanNetProject.Startup))]

namespace DikanNetProject
{
    public partial class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            ConfigureAuth(app);
            (MvcGrid.Filters as GridFilters).BooleanTrueOptionText();
        }
        
    }
}
