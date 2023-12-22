using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;
using Microsoft.Xrm.Sdk.Workflow;
using System;
using System.Activities;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProjetoDesafioSDK
{
    public class WFCriarCalendarioAluno : CodeActivity
    {
        #region PARAMETROS_DE_ENTRADA

        [Input("Usuario")]
        [ReferenceTarget("systemuser")]

        public InArgument<EntityReference> usuarioEntrada { get; set; }

        [Input("AlunoXCursoDisponivel")]
        [ReferenceTarget("curos_alunoxcursodisponivel")]

        public InArgument<EntityReference> RegistroContexto { get; set; }

        [Output("Saída")]
        public OutArgument<string> saida { get; set; }

        #endregion

        protected override void Execute(CodeActivityContext executionContext)
        {
            // CONEXÃO

            IWorkflowContext context = executionContext.GetExtension<IWorkflowContext>();
            IOrganizationServiceFactory serviceFactory = executionContext.GetExtension<IOrganizationServiceFactory>();
            IOrganizationService service = serviceFactory.CreateOrganizationService(context.UserId);
            ITracingService trace = executionContext.GetExtension<ITracingService>();

            Guid guidAlunoXCurso = context.PrimaryEntityId;

            trace.Trace("guidAlunoXCurso: " + guidAlunoXCurso);

            // OPERAÇÃO - CALENDARIO

            Entity entityAlunoXCursoDisponivel = service.Retrieve("curso_alunoxcursodisponivel", guidAlunoXCurso,
                new ColumnSet("cuso_cursoselecionado", "dio_periodo", "dio_datainicio"));

            Guid guidCurso = Guid.Empty;

            var PeriodoCurso = string.Empty;

            DateTime dataInicio = new DateTime();

            if (entityAlunoXCursoDisponivel == null)
            {

                guidCurso = ((EntityReference)entityAlunoXCursoDisponivel.Attributes["curso_cursoselecionado"]).Id;
                trace.Trace("guidCurso: " + guidCurso);
                if (entityAlunoXCursoDisponivel.Attributes.Contains("dio_periodo"))
                {
                    trace.Trace("Periodo: " + ((OptionSetValue)entityAlunoXCursoDisponivel["dio_periodo"]).Value);
                    if (((OptionSetValue)entityAlunoXCursoDisponivel["dio_periodo"]).Value == 914300000)
                    {
                        PeriodoCurso = "Diurno";
                        trace.Trace("Periodo Diurno");
                    }
                    else
                    {
                        PeriodoCurso = "Noturno";
                        trace.Trace("Periodo Noturno");
                    }
                }
                if (entityAlunoXCursoDisponivel.Attributes.Contains("dio_datadeinicio"))
                {
                    DateTime varDataInicio = ((DateTime)entityAlunoXCursoDisponivel["dio_datadeinicio"]);
                    dataInicio = new DateTime(varDataInicio.Year, dataInicio.Month, varDataInicio.Day);
                    trace.Trace("dataInicio: " + dataInicio);
                    trace.Trace("Dia da semana: " + dataInicio.ToString("ddd"));
                }
            }

            if (guidCurso != Guid.Empty)
            {
                Entity entityCurso = service.Retrieve("curso_cursosDisponiveis", guidCurso, new ColumnSet("dio_duracao"));
                int horasDuracao = 0;
                if (entityCurso != null && entityCurso.Attributes.Contains("dio_duracao"))
                {
                    horasDuracao = Convert.ToInt32(entityCurso.Attributes["dio_duracao"].ToString());
                }
                trace.Trace("horasDuracao: " + horasDuracao);

                int diasNecessarios = 0;
                if (PeriodoCurso == "Diurno")
                {
                    diasNecessarios = horasDuracao / 8;
                    trace.Trace("diasNecessarios: " + diasNecessarios);
                }
                else if (PeriodoCurso == "Noturno")
                {
                    diasNecessarios = horasDuracao / 4;
                    trace.Trace("diasNecessarios: " + diasNecessarios);
                }

                if (diasNecessarios > 0)
                {
                    for (int i = 0; i < diasNecessarios; i++)
                    {
                        if (dataInicio.ToString("ddd") == "Sat" && PeriodoCurso == "Noturno")
                        {
                            dataInicio = dataInicio.AddDays(2);
                        }
                        Entity entCalendarioAluno = new Entity("dio_calendariodoaluno");
                        entCalendarioAluno["dio_name"] = "Aula do dia " + dataInicio.ToString("ddd") + " - " + dataInicio;
                        entCalendarioAluno["dio_data"] = dataInicio;
                        entCalendarioAluno["dio_alunoxcursodisponivel"] = new EntityReference("curso_alunoxcursodisponivel", guidAlunoXCurso);
                        service.Create(entCalendarioAluno);

                        trace.Trace("Aula: " + i.ToString() + " - Data: " + dataInicio);

                        dataInicio = dataInicio.AddDays(1);
                    }
                }
            }

        }
    }
}
