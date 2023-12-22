using Microsoft.Xrm.Sdk.Query;
using Microsoft.Xrm.Sdk;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProjetoDesafioSDK
{
    public class PluginAccountPreOperation : IPlugin
    {
        public void Execute(IServiceProvider serviceProvider)
        {
            // CONEXÃO

            IPluginExecutionContext context =
                (IPluginExecutionContext)serviceProvider.GetService(typeof(IPluginExecutionContext));

            IOrganizationServiceFactory serviceFactory =
                (IOrganizationServiceFactory)serviceProvider.GetService(typeof(IOrganizationServiceFactory));

            IOrganizationService serviceAdmin = serviceFactory.CreateOrganizationService(null);

            ITracingService trace = (ITracingService)serviceProvider.GetService(typeof(ITracingService));

            // OPERAÇÃO

            if (context.InputParameters.Contains("Target") && context.InputParameters["Target"] is Entity)
            {
                Entity entidadeContexto = (Entity)context.InputParameters["Target"];

                if (entidadeContexto.LogicalName == "account") 
                {
                    if (entidadeContexto.Attributes.Contains("teelephone1")) 
                    {
                        var phone1 = entidadeContexto["telephone1"].ToString();

                        string FetchContact = @"<?xml version='1.0'?>" +
                        "<fetch distinct='false' mapping='logical' output-format='xml-plataform' version='1.0'>" +
                        "<entity name='contact'>" +
                        "<attribute name='fullname'/>" +
                        "<attribute name='telephone1'/>" +
                        "<attribute name='contactid'/>" +
                        "<order descending='false' attribute='fullname'/>" +
                        "<filter type='and'>" +
                        "<condition attribute='telephone1' value='" + phone1 + "' operator='eq'/>" +
                        "</filter>" +
                        "</entity>" +
                        "</fetch>";

                        trace.Trace("FetchContact: " + FetchContact); 

                        var primarycontact = serviceAdmin.RetrieveMultiple(new FetchExpression(FetchContact));

                        if (primarycontact.Entities.Count > 0) 
                        {
                            foreach (var entityContact in primarycontact.Entities)
                            {
                                entidadeContexto["primarycontactid"] = new EntityReference("contact", entityContact.Id);
                            }

                        }
                    }
                }

            }
        }
    }
}
