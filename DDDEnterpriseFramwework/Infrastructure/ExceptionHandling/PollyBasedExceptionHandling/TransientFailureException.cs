using System;
using System.Xml.Serialization;

namespace Infrastructure.ExceptionHandling.PollyBasedExceptionHandling
{
    [Serializable]
    public class TransientFailureException
    {
        [XmlAttribute(AttributeName = "assemblyName")]
        public string AssemblyName { get; set; }

        [XmlAttribute(AttributeName = "partialOrFullExceptionNames")]
        public string CommaSeperatedTransientFailureExceptions { get; set; }

        [XmlText]
        public string CommaSeperatedPollyPoliciesNames { get; set; }
    }
}
