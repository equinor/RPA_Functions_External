using Microsoft.WindowsAzure.Storage.Table;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;


namespace RPA_Azure_Func_External
{
    internal class MaterialDeliveryTableOperations
    {
        private string tableName = Environment.GetEnvironmentVariable("PC243_TABLENAME");
        private CommonTable table;

        public MaterialDeliveryTableOperations()
        {
            this.table = new CommonTable();
        }

        public async Task<List<MaterialDeliveryTableEntity>> QueryMaterialDeliveryOnWebid(string webid, bool robotQuery)
        {
            if (robotQuery == false)
            {

                string guidFilter = TableQuery.GenerateFilterCondition(MaterialDeliveryConstants.WEBID_FIELD_NAME, QueryComparisons.Equal, webid);
                string statusFilter = TableQuery.GenerateFilterConditionForInt(MaterialDeliveryConstants.STATUS_FIELD_NAME, QueryComparisons.Equal, MaterialDeliveryConstants.STATUS_WAITING);

                List<MaterialDeliveryTableEntity> queryResult = await table.RetrieveEntitiesCombinedFilter<MaterialDeliveryTableEntity>(guidFilter,
                                                                                                                                    TableOperators.And,
                                                                                                                                    statusFilter,
                                                                                                                                    tableName);

                return queryResult;

            }
            else if (robotQuery == true)
            {
                List<MaterialDeliveryTableEntity> queryResult = await table.RetrieveEntities<MaterialDeliveryTableEntity>(MaterialDeliveryConstants.WEBID_FIELD_NAME, QueryComparisons.Equal, webid, tableName);

                return queryResult;
            }

            return null;
        }
        public async Task<List<MaterialDeliveryTableEntity>> QueryMaterialDeliveryOnGuid(string guid)
        {

            List<MaterialDeliveryTableEntity> queryResult = await table.RetrieveEntities<MaterialDeliveryTableEntity>(MaterialDeliveryConstants.ID_FIELD_NAME, QueryComparisons.Equal, guid, tableName);


            return queryResult;
        }

        private async Task<List<MaterialDeliveryTableEntity>> QueryMaterialDeliveryOnStatus(int status)
        {
            List<MaterialDeliveryTableEntity> queryResult = await table.RetrieveEntities<MaterialDeliveryTableEntity>(MaterialDeliveryConstants.STATUS_FIELD_NAME, QueryComparisons.Equal, status, tableName);

            return queryResult;

        }

       
        public async Task<List<MaterialDeliveryTableEntity>> QueryMaterialDeliveryOnStatusAndRemove()
        {
            List<MaterialDeliveryTableEntity> queryResult = QueryMaterialDeliveryOnStatus(MaterialDeliveryConstants.STATUS_WAITING).Result;

            foreach (MaterialDeliveryTableEntity element in queryResult)
            {
                TimeSpan ts = DateTime.Now - element.Timestamp;
                if (ts.Days >= 14)
                {
                    try
                    {

                        TableResult trRm = await table.Remove(element, tableName);

                    }
                    catch (Exception ex)
                    {

                        Console.WriteLine("Error in writing to table storage " + ex.ToString());
                    }
                }
            }
            List<MaterialDeliveryTableEntity> queryResult2 = QueryMaterialDeliveryOnStatus(MaterialDeliveryConstants.STATUS_FETCHED).Result;

            foreach (MaterialDeliveryTableEntity element in queryResult2)
            {
                TimeSpan ts = DateTime.Now - element.Timestamp;
                if (ts.Days >= 14)
                {
                    try
                    {

                        TableResult trRm = await table.Remove(element, tableName);

                    }
                    catch (Exception ex)
                    {

                        Console.WriteLine("Error in writing to table storage " + ex.ToString());
                    }
                }
            }
            return null;
        }


        public async Task<Object> UpdateMaterialDelivery(string guid, dynamic bodyData)
        {

            // Find package, if found - update else fail.
            List<MaterialDeliveryTableEntity> result = await QueryMaterialDeliveryOnGuid(guid);

            if (result.Count == 1)
            {
                MaterialDeliveryTableEntity updatedMaterialDelivery = Mappings.updateMaterialDelivery(result[0], bodyData);
                TableResult tr = await table.InsertorReplace(updatedMaterialDelivery, tableName);

                return tr.Result;

            }
            else
            {
                return null;
            }
        }
    }
}
