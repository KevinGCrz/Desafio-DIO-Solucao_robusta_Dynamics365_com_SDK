using Microsoft.Xrm.Sdk.Query;
using Microsoft.Xrm.Sdk.Workflow;
using Microsoft.Xrm.Sdk;
using System;
using System.Activities;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProjetoDesafioSDK
{
    public class WFValidaLimiteInscricoesAluno : CodeActivity
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

            trace.Trace("curso_alunoxcursodisponivel: " + context.PrimaryEntityId);

            // OPERAÇÃO - VALIDAÇÃO

            Guid guidAlunoXCurso = context.PrimaryEntityId;

            trace.Trace("guidAlunoXCurso: " + guidAlunoXCurso);

            String fetchAlunoXCursos = "<fetch distinct='false' mapping='logical' output-format='xml-plataform' version='1.0'>";
            fetchAlunoXCursos += "<entity name='curso_alunoxcurodisponivel' >";
            fetchAlunoXCursos += "<attribute name='curso_alunoxcursodisponivelid' />";
            fetchAlunoXCursos += "<attribute name='curso_name' />";
            fetchAlunoXCursos += "<attribute name='curso_emcurso' />";
            fetchAlunoXCursos += "<attribute name='createdon' />";
            fetchAlunoXCursos += "<attribute name='curso_aluno' />";
            fetchAlunoXCursos += "<order descending= 'false' attribute = 'curso_name' />";
            fetchAlunoXCursos += "<filter type= 'and' >";
            fetchAlunoXCursos += "<condition attribute = 'curso_alunoxcursodisponivelid' value = '" + guidAlunoXCurso + "' uitype = 'curso_alunoxcursodisponivel>";
            fetchAlunoXCursos += "</filter>";
            fetchAlunoXCursos += "</entity>";
            fetchAlunoXCursos += "</fetch>";
            trace.Trace("fetchAlunoXCursos: " + fetchAlunoXCursos);

            var entityAlunoXCursos = service.RetrieveMultiple(new FetchExpression(fetchAlunoXCursos));
            trace.Trace("entityAlunoXCursos: " + entityAlunoXCursos.Entities.Count);

            Guid guidAluno = Guid.Empty;
            foreach (var item in entityAlunoXCursos.Entities)
            {
                string nomeCurso = item.Attributes["curso_name"].ToString();
                trace.Trace("nomeCurso: " + nomeCurso);

                var entityAluno = ((EntityReference)item.Attributes["curso_aluno"]).Id;
                guidAluno = ((EntityReference)item.Attributes["curso_aluno"]).Id;
                trace.Trace("entityAluno: " + entityAluno);
            }

            String fetchAlunoXCursosQtde = "<fetch distinct='false' mapping='logical' output-format='xml-plataform' version='1.0'>";
            fetchAlunoXCursosQtde += "<entity name='curso_alunoxcurodisponivel' >";
            fetchAlunoXCursosQtde += "<attribute name='curso_alunoxcursodisponivelid' />";
            fetchAlunoXCursosQtde += "<attribute name='curso_name' />";
            fetchAlunoXCursosQtde += "<attribute name='curso_emcurso' />";
            fetchAlunoXCursosQtde += "<attribute name='createdon' />";
            fetchAlunoXCursosQtde += "<attribute name='curso_aluno' />";
            fetchAlunoXCursosQtde += "<order descending= 'false' attribute = 'curso_name' />";
            fetchAlunoXCursosQtde += "<filter type= 'and' >";
            fetchAlunoXCursosQtde += "<condition attribute = 'curso_alunoxcursodisponivelid' value = '" + guidAlunoXCurso + "' uitype = 'curso_alunoxcursodisponivel>";
            fetchAlunoXCursosQtde += "</filter>";
            fetchAlunoXCursosQtde += "</entity>";
            fetchAlunoXCursosQtde += "</fetch>";
            trace.Trace("fetchAlunoXCursos: " + fetchAlunoXCursosQtde);
            var entityAlunoXCursoQtde = service.RetrieveMultiple(new FetchExpression(fetchAlunoXCursosQtde));
            trace.Trace("entityAlunoXCursoQtde: " + entityAlunoXCursoQtde.Entities.Count);
            if (entityAlunoXCursoQtde.Entities.Count > 2)
            {
                saida.Set(executionContext, "Aluno excedeu limite de cursos ativos!");
                trace.Trace("Aluno excedeu limite de cursos ativos!");
                throw new InvalidPluginExecutionException("Aluno excedeu limite de cursos ativos!");

            }

        }
    }
}
