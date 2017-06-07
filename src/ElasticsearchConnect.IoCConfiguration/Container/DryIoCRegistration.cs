using ElasticsearchConnect.Repository;
using DryIoc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace ElasticsearchConnect.Api.IoCConfiguration
{
    public class DryIoCRegistration : IIoCRegistration
    {
        #region private properties

        private Container container;
        private PropertiesAndFieldsSelector currentPropertiesSelector;
        private ParameterSelector currentParameterSelector;
        private string currentServiceName;
        private string currentScope;

        #endregion

        public object Container
        {
            get { return container; }
        }

        public DryIoCRegistration()
        {
            container = new Container();
        }

        /// <summary>
        /// Custom Register for the specific IoC 
        /// </summary>
        public void Register()
        {

        }

        #region IIoCRegistration
        public void Instance<TService, TImplementation>(TImplementation @class, string serviceName) where TImplementation : TService
        {
            this.container.RegisterDelegate<TService>(d => @class, serviceKey: serviceName);
        }

        public void Instance<TService, TImplementation>(TImplementation @class) where TImplementation : TService
        {
            this.container.RegisterDelegate<TService>(d => @class);
        }

        public void Instance<TService, TImplementation>(string serviceName)
            where TImplementation : TService
        {
            this.container.Register<TService, TImplementation>(serviceKey: serviceName);
        }

        public IDipendencyRegistration InjectProperty<TProperty>(string propertyName, TProperty value)
        {
            string serviceKey = string.Format("{0}{1}", this.currentServiceName, propertyName);

            if (this.currentPropertiesSelector == null)
                this.currentPropertiesSelector = PropertiesAndFields.Of.Name(propertyName, serviceKey: serviceKey);
            else
                this.currentPropertiesSelector = PropertiesAndFields.Of.Name(propertyName, serviceKey: serviceKey).And(this.currentPropertiesSelector);

            this.container.RegisterInstance(value, serviceKey: serviceKey);
            return this;
        }
        public IDipendencyRegistration InjectProperty<TImplementation, TProperty>(System.Linq.Expressions.Expression<Func<TImplementation, TProperty>> selector, TProperty value)
        {
            var memeber = selector.Body as MemberExpression;
            string propertyName = memeber.Member.Name;
            return this.InjectProperty(propertyName, value);
        }

        public IDipendencyRegistration InjectProperty(string propertyName, Type type)
        {
            if (this.currentPropertiesSelector == null)
                this.currentPropertiesSelector = PropertiesAndFields.Of.Name(propertyName, type);
            else
                this.currentPropertiesSelector = PropertiesAndFields.Of.Name(propertyName, type).And(this.currentPropertiesSelector);
            return this;
        }

        public IDipendencyRegistration InjectProperty<TImplementation>(Expression<Func<TImplementation, object>> selector, Type type)
        {
            var memeber = selector.Body as MemberExpression;
            string propertyName = memeber.Member.Name;
            return this.InjectProperty(propertyName, type);
        }

        public IDipendencyRegistration InjectConstructor(params KeyValuePair<string, object>[] args)
        {
            this.currentParameterSelector = GetParameterSelector(args);
            return this;
        }

        public void Instance<TService, TImplementation>() where TImplementation : TService
        {
            this.container.Register<TService, TImplementation>();
        }

        public IDipendencyRegistration CustomInstance(string serviceName, string scope = null)
        {
            this.currentServiceName = serviceName;
            this.currentScope = scope;
            return this;
        }

        private ParameterSelector GetParameterSelector(params KeyValuePair<string, object>[] args)
        {
            ParameterSelector parameterSelector = null;
            if (parameterSelector == null)
                parameterSelector = Parameters.Of.Details((par, req) => null);

            foreach (var arg in args)
            {
                string serviceKey = $"{ this.currentServiceName}{arg.Key}";
                this.container.RegisterInstance(arg.Value, serviceKey: serviceKey);

                //if (parameterSelector != null)
                //    parameterSelector.And(Parameters.Of.Name(arg.Key, serviceKey: serviceKey));
                //else
                parameterSelector = Parameters.Of.Name(arg.Key, requiredServiceType: arg.Value.GetType(), serviceKey: serviceKey);
            }
            return parameterSelector;
        }

        public IServiceRegistration Complete<TService, TImplementation>() where TImplementation : TService
        {
            this.container.Register<TService, TImplementation>(serviceKey: this.currentServiceName, made: Made.Of(parameters: this.currentParameterSelector, propertiesAndFields: this.currentPropertiesSelector));

            this.currentServiceName = null;
            this.currentParameterSelector = null;
            this.currentPropertiesSelector = null;
            return this;
        }
        #endregion
    }
}
