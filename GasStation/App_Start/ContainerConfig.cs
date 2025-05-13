using Autofac;
using Autofac.Integration.Mvc;
using System.Web.Mvc;
using System.Reflection; // Potrzebne do Assembly
using GasStation.Data; // Pamiętaj o usingu do Twojego DbContext
using GasStation.Services; // Pamiętaj o usingu do Twoich serwisów
using GasStation.Controllers; // Pamiętaj o usingu do Twoich kontrolerów

namespace GasStation
{
    public static class ContainerConfig
    {
        public static void ConfigureContainer()
        {
            var builder = new ContainerBuilder();

            // Zarejestruj wszystkie kontrolery w bieżącym zestawie (Assembly)
            // To pozwoli Autofac na wstrzykiwanie zależności do konstruktorów kontrolerów
            builder.RegisterControllers(Assembly.GetExecutingAssembly());

            // Zarejestruj GasStationDbContext
            // InstancePerRequest oznacza, że jedna instancja DbContext będzie tworzona na każde żądanie HTTP
            builder.RegisterType<GasStationDbContext>().InstancePerRequest();

            // Zarejestruj Twoje serwisy
            // AsSelf() oznacza, że serwis będzie rejestrowany pod swoją konkretną klasą (np. EmployeeService)
            // Możesz też zarejestrować pod interfejsem, jeśli go używasz (np. .As<IEmployeeService>())
            // InstancePerRequest jest często dobrym wyborem dla serwisów, które korzystają z DbContext
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