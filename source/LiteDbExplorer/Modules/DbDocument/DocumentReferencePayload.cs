using LiteDbExplorer.Core;
using LiteDbExplorer.Wpf.Framework;

namespace LiteDbExplorer.Modules.DbDocument
{
    public class DocumentReferencePayload : IReferenceId
    {
        public DocumentReferencePayload(DocumentReference documentReference)
        {
            InstanceId = documentReference.InstanceId;
            DocumentReference = documentReference;
        }

        public string InstanceId { get; }

        public DocumentReference DocumentReference { get; }
    }
}