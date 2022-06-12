using System.Xml.Serialization;
using System.Collections.Generic;

namespace AzureStorageSample.Models.XMLModels
{
    [XmlRoot(ElementName = "Properties")]
    public class Properties
    {
        [XmlElement(ElementName = "Last-Modified")]
        public string LastModified { get; set; }

        [XmlElement(ElementName = "Etag")]
        public string Etag { get; set; }

        [XmlElement(ElementName = "Content-Length")]
        public int ContentLength { get; set; }

        [XmlElement(ElementName = "Content-Type")]
        public string ContentType { get; set; }

        [XmlElement(ElementName = "Content-Encoding")]
        public object ContentEncoding { get; set; }

        [XmlElement(ElementName = "Content-Language")]
        public object ContentLanguage { get; set; }

        [XmlElement(ElementName = "Content-MD5")]
        public string ContentMD5 { get; set; }

        [XmlElement(ElementName = "Cache-Control")]
        public object CacheControl { get; set; }

        [XmlElement(ElementName = "BlobType")]
        public string BlobType { get; set; }

        [XmlElement(ElementName = "LeaseStatus")]
        public string LeaseStatus { get; set; }

        [XmlElement(ElementName = "LeaseState")]
        public string LeaseState { get; set; }
    }

    [XmlRoot(ElementName = "Blob")]
    public class Blob
    {

        [XmlElement(ElementName = "Name")]
        public string Name { get; set; }

        [XmlElement(ElementName = "Url")]
        public string Url { get; set; }

        [XmlElement(ElementName = "Properties")]
        public Properties Properties { get; set; }
    }

    [XmlRoot(ElementName = "Blobs")]
    public class Blobs
    {

        [XmlElement(ElementName = "Blob")]
        public List<Blob> Blob { get; set; }
    }

    [XmlRoot(ElementName = "EnumerationResults")]
    public class EnumerationResults
    {

        [XmlElement(ElementName = "Blobs")]
        public Blobs Blobs { get; set; }

        [XmlElement(ElementName = "NextMarker")]
        public object NextMarker { get; set; }

        [XmlAttribute(AttributeName = "ContainerName")]
        public string ContainerName { get; set; }

        [XmlText]
        public string Text { get; set; }
    }

}