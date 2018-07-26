using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;
using System;
using System.Collections.Generic;
using System.Linq;

namespace DeleteEntityReferenceBug.Plugins
{
    public class PreOperateAccountDelete : IPlugin
    {
        public void Execute(IServiceProvider serviceProvider)
        {
            var tracer = (ITracingService)serviceProvider.GetService(typeof(ITracingService));
            var context = (IPluginExecutionContext)serviceProvider.GetService(typeof(IPluginExecutionContext));
            var factory = (IOrganizationServiceFactory)serviceProvider.GetService(typeof(IOrganizationServiceFactory));
            var service = factory.CreateOrganizationService(context.UserId);

            var accountReference = (EntityReference)context.InputParameters["Target"];

            var accountName = GetAccountName(accountReference, service);

            var childContacts = GetContacts(accountReference, service);

            //throw new InvalidPluginExecutionException($"Found {childContacts.Count()} contacts, with parentcustomerid fields set to ({string.Join(", ", childContacts.Select(c => c.Contains("parentcustomerid") ? ((EntityReference)c["parentcustomerid"]).Id.ToString() : "NULL").Distinct())}).");
            throw new InvalidPluginExecutionException($"Found {childContacts.Count()} child contacts for {accountName}.");
        }

        private string GetAccountName(EntityReference accountReference, IOrganizationService service)
        {
            var account = service.Retrieve("account", accountReference.Id, new ColumnSet("name"));
            return (string)account["name"];
        }

        private IEnumerable<Entity> GetContacts(EntityReference accountReference, IOrganizationService service)
        {
            var query = new QueryExpression("contact")
            {
                ColumnSet = new ColumnSet()//"parentcustomerid")
            };

            query.Criteria.AddCondition("parentcustomerid", ConditionOperator.Equal, accountReference.Id);

            return service.RetrieveMultiple(query).Entities;
        }
    }
}