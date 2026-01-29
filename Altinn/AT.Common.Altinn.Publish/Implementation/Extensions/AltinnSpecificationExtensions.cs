using Arbeidstilsynet.Common.Altinn.Model.Adapter;
using Arbeidstilsynet.Common.Altinn.Model.Api.Response;

namespace Arbeidstilsynet.Common.Altinn.Implementation.Extensions;

internal static class AltinnSpecificationExtensions
{
    extension(AltinnAppSpecification appSpec)
    {
        public FileMetadata CreateFileMetadata(DataElement dataElement)
        {
            return new FileMetadata
            {
                ContentType = dataElement.ContentType,
                DataType = dataElement.DataType,
                Filename = appSpec.GetFilename(dataElement),
                FileScanResult = dataElement.FileScanResult,
            };
        }

        public (DataElement mainData, DataElement? structuredData, IEnumerable<DataElement> attachmentData)
            GetDataElementsBySignificance(AltinnInstance instance)
        {
            var mainData = instance.Data.FirstOrDefault(d => d.DataType == appSpec.MainPdfDataTypeId) ?? throw new InvalidOperationException(
                $"Main document with data type '{appSpec.MainPdfDataTypeId}' not found in instance '{instance.Id}'. The instance was from app '{instance.AppId}'. Existing data types: [{string.Join(", ", instance.Data.Select(d => d.DataType))}]"
            );
        
            DataElement? structuredData = null;
            List<DataElement> attachmentData = [];

            foreach (var dataElement in instance.Data.Where(d => d.Id != mainData.Id))
            {
                if (dataElement.DataType == appSpec.StructuredDataTypeId)
                {
                    structuredData = dataElement;
                }
                else
                {
                    attachmentData.Add(dataElement);
                }
            }

            return (mainData, structuredData, attachmentData);
        
        }

        private string GetFilename(DataElement dataElement)
        {
            if (appSpec.MainPdfDataTypeId == dataElement.DataType)
            {
                return appSpec.MainPdfFileName;
            }
        
            if (appSpec.StructuredDataTypeId == dataElement.DataType)
            {
                return appSpec.StructuredDataFileName;
            }
        
            return dataElement.Filename;
        }
    }
}