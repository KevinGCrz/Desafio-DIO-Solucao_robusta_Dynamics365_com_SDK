using Microsoft.Xrm.Sdk;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProjetoDesafioSDK
{
    public class PluginAccountPostOperation : IPlugin
    {
        public void Execute(IServiceProvider serviceProvider)
        {
            // CONEXÃO

            try
            {
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

                    if (!entidadeContexto.Contains("websiteurl")) 
                    {
                        throw new InvalidPluginExecutionException("Campo websiteurl é obrigatorio!"); 
                    }

                    var Task = new Entity("task");

                    Task.Attributes["ownerid"] = new EntityReference("systemuser", context.UserId);
                    Task.Attributes["regardingobjectid"] = new EntityReference("account", context.PrimaryEntityId);
                    Task.Attributes["subject"] = "Visite nosso site: " + entidadeContexto["websiteurl"];
                    Task.Attributes["description"] = "TASK criada via Plugin Post Operation";

                    serviceAdmin.Create(Task);

                }

            }
            catch (InvalidPluginExecutionException ex)
            {

                throw new InvalidPluginExecutionException("Erro ocorrido: " + ex.Message);
            }
        }
    }
}
