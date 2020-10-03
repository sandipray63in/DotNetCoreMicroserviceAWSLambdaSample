using System;
using System.Xml.Serialization;

namespace Infrastructure.ExceptionHandling.PollyBasedExceptionHandling
{
    [Serializable]
    [XmlRoot("PollyTransientFailures")]
    public class PollyTransientFailureExceptions
    {
        [XmlArray("PollyTransientFailures")]
        [XmlArrayItem("TransientFailureException", typeof(TransientFailureException))]
        public TransientFailureException[] TransientFailureExceptions { get; set; }
    }
}
