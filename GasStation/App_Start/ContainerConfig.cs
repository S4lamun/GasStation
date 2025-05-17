using Autofac;
using Autofac.Integration.Mvc;
using System.Web.Mvc;
using System.Reflection; 
using GasStation.Data; 
using GasStation.Services; 
using GasStation.Controllers; 

namespace GasStation
{
    public static class ContainerConfig
    {
        public static void ConfigureContainer()
        {
            var builder = new ContainerBuilder();

        
            builder.RegisterControllers(Assembly.GetExecutingAssembly());

         
            builder.RegisterType<GasStationDbContext>().InstancePerRequest();

            builder.RegisterType<EmployeeService>().AsSelf().InstancePerRequest();
            builder.RegisterType<CustomerService>().AsSelf().InstancePerRequest();
            builder.RegisterType<FuelService>().AsSelf().InstancePerRequest();
            builder.RegisterType<FuelPriceHistoryService>().AsSelf().InstancePerRequest();
            builder.RegisterType<OrderService>().AsSelf().InstancePerRequest();
            builder.RegisterType<ProductService>().AsSelf().InstancePerRequest();

            // Zarejestruj inne serwisy, jeśli je masz...

            // Buduj kontener
            var container = builder.Build();

            // Ustaw Resolver zależności dla MVC
            DependencyResolver.SetResolver(new AutofacDependencyResolver(container));
        }
    }
}