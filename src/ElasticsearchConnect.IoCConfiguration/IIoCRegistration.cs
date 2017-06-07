using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace ElasticsearchConnect.Api.IoCConfiguration
{
    public interface IIoCRegistration : IServiceRegistration
    {
        /// <summary>
        /// Get current IoC Container
        /// </summary>
        object Container { get; }

        /// <summary>
        /// Register all services
        /// </summary>
        void Register();
    }

    public interface IServiceRegistration : IDipendencyRegistration
    {
        /// <summary>
        /// Register service using concurrent instance with specific service name
        /// </summary>
        void Instance<TService, TImplementation>(TImplementation @class, string serviceName) where TImplementation : TService;

        /// <summary>
        /// Register service using concurrent instance
        /// </summary>
        void Instance<TService, TImplementation>(TImplementation @class) where TImplementation : TService;

        /// <summary>
        /// Register service using concurrent class with specific service name
        /// </summary>
        void Instance<TService, TImplementation>(string serviceName) where TImplementation : TService;

        /// <summary>
        /// Register service without service name
        /// </summary>
        void Instance<TService, TImplementation>() where TImplementation : TService;

        /// <summary>
        /// Custom registration using service name
        /// Used for inject custom property and  constructor args
        /// </summary>
        IDipendencyRegistration CustomInstance(string serviceName, string scope=null);
    }

    public interface IDipendencyRegistration
    {
        /// <summary>
        /// Inject Property with custom value
        /// </summary>
        IDipendencyRegistration InjectProperty<TProperty>(string propertyName, TProperty value);

        /// <summary>
        /// Inject Property with specific type
        /// </summary>
        IDipendencyRegistration InjectProperty(string propertyName, Type type);

        /// <summary>
        /// Inject Property using expression selector and custom value
        /// </summary>
        IDipendencyRegistration InjectProperty<TImplementation, TProperty>(Expression<Func<TImplementation, TProperty>> selector, TProperty value);

        /// <summary>
        /// Inject Property using expression selector and specific type
        /// </summary>
        IDipendencyRegistration InjectProperty<TImplementation>(Expression<Func<TImplementation, object>> selector, Type type);

        /// <summary>
        /// Inject primtive value into constructor 
        /// </summary>
        IDipendencyRegistration InjectConstructor(params KeyValuePair<string, object>[] args);

        /// <summary>
        /// Complete registration 
        /// </summary>
        IServiceRegistration Complete<TService, TImplementation>() where TImplementation : TService;
    }

}
