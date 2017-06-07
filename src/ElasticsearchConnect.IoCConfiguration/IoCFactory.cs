using ElasticsearchConnect.Api.IoCConfigurator;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ElasticsearchConnect.Api.IoCConfiguration
{
    public class IoCFactory
    {
        static IIoCRegistration currentIocRegistration;
        static IInstanceContainer currentContainer;

        internal static IIoCRegistration GetIoCRegistration()
        {
            if (currentIocRegistration == null)
                currentIocRegistration = new DryIoCRegistration();
            return currentIocRegistration;
        }

        internal static T GetServiceIoCContainer<T>()
            where T : class
        {
            return currentIocRegistration.Container as T;
        }

        public static IInstanceContainer GetContainer()
        {
            if (currentIocRegistration == null)
            {
                GetIoCRegistration();
                CommonIoCRegistration.Register();
            }

            if (currentContainer == null)
                currentContainer = new DryIoCContainer();

            return currentContainer;
        }
    }
}
