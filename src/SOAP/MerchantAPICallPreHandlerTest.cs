using System.Collections.Generic;
using System.Xml;
using PayPal.Manager;
using PayPal.Exception;
using PayPal.Authentication;
using PayPal.SOAP;

#if NUnit
using NUnit.Framework;

namespace PayPal.NUnitTest
{
    [TestFixture]
    class MerchantAPICallPreHandlerTest
    {
        private DefaultSOAPAPICallHandler defaultSoapHandler;
        private CredentialManager credentialMngr;
        private ICredential credential;
        private MerchantAPICallPreHandler soapHandler;
        private Dictionary<string, string> accountConfig;

        public MerchantAPICallPreHandlerTest()
        {
            defaultSoapHandler = new DefaultSOAPAPICallHandler(ConfigManager.Instance.GetProperties(), "<Request>test</Request>", null, null);
            credentialMngr = CredentialManager.Instance;

            accountConfig = new Dictionary<string, string>();
            accountConfig.Add("account1.apiUsername", Constants.APIUserName);
            accountConfig.Add("account1.apiPassword", Constants.APIPassword);
            accountConfig.Add("account1.applicationId", Constants.ApplicationID);
            accountConfig.Add("account1.apiSignature", Constants.APISignature);
            accountConfig.Add("account2.apiUsername", Constants.CertificateAPIUserName);
            accountConfig.Add("account2.apiPassword", Constants.CertificateAPIPassword);
            accountConfig.Add("account2.applicationId", Constants.ApplicationID);
            accountConfig.Add("account2.apiCertificate", Constants.CertificatePath);
            accountConfig.Add("account2.privateKeyPassword", Constants.CertificatePassword);
        }

        [Test]
        public void GetHeaderMapSignature()
        {
            credential = credentialMngr.GetCredentials(ConfigManager.Instance.GetProperties(), Constants.APIUserName);
            soapHandler = new MerchantAPICallPreHandler(ConfigManager.Instance.GetProperties(), defaultSoapHandler, credential);
            Dictionary<string, string> headers = soapHandler.GetHeaderMap();
            Assert.IsNotNull(headers);
            Assert.IsTrue(headers.Count > 0);
            Assert.AreEqual(Constants.APIUserName, headers[BaseConstants.PayPalSecurityUserIDHeader]);
            Assert.AreEqual(Constants.APIPassword, headers[BaseConstants.PayPalSecurityPasswordHeader]);
            Assert.AreEqual(Constants.APISignature, headers[BaseConstants.PayPalSecuritySignatureHeader]);
            Assert.AreEqual(BaseConstants.SOAP, headers[BaseConstants.PayPalRequestDataFormatHeader]);
            Assert.AreEqual(BaseConstants.SOAP, headers[BaseConstants.PayPalResponseDataFormatHeader]);
        }

        [Test]
        public void GetHeaderMapCertificate()
        {
            credential = credentialMngr.GetCredentials(ConfigManager.Instance.GetProperties(), Constants.CertificateAPIUserName);
            soapHandler = new MerchantAPICallPreHandler(ConfigManager.Instance.GetProperties(), defaultSoapHandler, credential);
            Dictionary<string, string> headers = soapHandler.GetHeaderMap();
            Assert.IsNotNull(headers);
            Assert.IsTrue(headers.Count > 0);
            Assert.AreEqual(Constants.CertificateAPIUserName, headers[BaseConstants.PayPalSecurityUserIDHeader]);
            Assert.AreEqual(Constants.CertificateAPIPassword, headers[BaseConstants.PayPalSecurityPasswordHeader]);
            Assert.AreEqual(BaseConstants.SOAP, headers[BaseConstants.PayPalRequestDataFormatHeader]);
            Assert.AreEqual(BaseConstants.SOAP, headers[BaseConstants.PayPalResponseDataFormatHeader]);
            Assert.AreEqual(soapHandler.SDKName + "-" + soapHandler.SDKVersion, headers[BaseConstants.PayPalRequestSourceHeader]);
        }

        [Test]
        public void GetPayLoadSignature()
        {
            credential = credentialMngr.GetCredentials(ConfigManager.Instance.GetProperties(), Constants.APIUserName);
            soapHandler = new MerchantAPICallPreHandler(ConfigManager.Instance.GetProperties(), defaultSoapHandler, credential);
            string payload = soapHandler.GetPayLoad();
            XmlDocument xmlDoc = GetXmlDocument(payload);
            XmlNodeList xmlNodeListUsername = xmlDoc.GetElementsByTagName("Username");
            Assert.IsTrue(xmlNodeListUsername.Count > 0);
            Assert.AreEqual(Constants.APIUserName, xmlNodeListUsername[0].InnerXml);
            XmlNodeList xmlNodeListPassword = xmlDoc.GetElementsByTagName("Password");
            Assert.IsTrue(xmlNodeListPassword.Count > 0);
            Assert.AreEqual(Constants.APIPassword, xmlNodeListPassword[0].InnerXml);
            XmlNodeList xmlNodeListSignature = xmlDoc.GetElementsByTagName("Signature");
            Assert.IsTrue(xmlNodeListSignature.Count > 0);
            Assert.AreEqual(Constants.APISignature, xmlNodeListSignature[0].InnerXml);
            XmlNodeList xmlNodeListRequest = xmlDoc.GetElementsByTagName("Request");
            Assert.IsTrue(xmlNodeListRequest.Count > 0);
            Assert.AreEqual("test", xmlNodeListRequest[0].InnerXml);
        }

        [Test]
        public void GetPayLoadForCertificate()
        {
            credential = credentialMngr.GetCredentials(ConfigManager.Instance.GetProperties(), Constants.CertificateAPIUserName);
            soapHandler = new MerchantAPICallPreHandler(ConfigManager.Instance.GetProperties(), defaultSoapHandler, credential);
            string payload = soapHandler.GetPayLoad();
            XmlDocument xmlDoc = GetXmlDocument(payload);
            XmlNodeList xmlNodeListUsername = xmlDoc.GetElementsByTagName("Username");
            Assert.IsTrue(xmlNodeListUsername.Count > 0);
            Assert.AreEqual(Constants.CertificateAPIUserName, xmlNodeListUsername[0].InnerXml);
            XmlNodeList xmlNodeListPassword = xmlDoc.GetElementsByTagName("Password");
            Assert.IsTrue(xmlNodeListPassword.Count > 0);
            Assert.AreEqual(Constants.CertificateAPIPassword, xmlNodeListPassword[0].InnerXml);
            XmlNodeList xmlNodeListRequest = xmlDoc.GetElementsByTagName("Request");
            Assert.IsTrue(xmlNodeListRequest.Count > 0);
            Assert.AreEqual("test", xmlNodeListRequest[0].InnerXml);
        }

        [Test]
        public void SDKName()
        {
            credential = credentialMngr.GetCredentials(ConfigManager.Instance.GetProperties(), Constants.CertificateAPIUserName);
            MerchantAPICallPreHandler soapHandler = new MerchantAPICallPreHandler(ConfigManager.Instance.GetProperties(), defaultSoapHandler, credential);
            soapHandler.SDKName = "testsdk";
            Assert.AreEqual("testsdk", soapHandler.SDKName);
        }

        [Test]
        public void SDKVersion()
        {
            credential = credentialMngr.GetCredentials(ConfigManager.Instance.GetProperties(), Constants.CertificateAPIUserName);
            MerchantAPICallPreHandler soapHandler = new MerchantAPICallPreHandler(ConfigManager.Instance.GetProperties(), defaultSoapHandler, credential);
            soapHandler.SDKVersion = "1.0.0";
            Assert.AreEqual("1.0.0", soapHandler.SDKVersion);
        }

        [Test]
        public void GetEndpoint()
        {
            credential = credentialMngr.GetCredentials(ConfigManager.Instance.GetProperties(), Constants.CertificateAPIUserName);
            MerchantAPICallPreHandler soapHandler = new MerchantAPICallPreHandler(ConfigManager.Instance.GetProperties(), defaultSoapHandler, credential);
            string endpoint = soapHandler.GetEndpoint();
            Assert.AreEqual(Constants.APIEndpointNVP, endpoint);
        }

        [Test]
        public void GetEndpointForSandboxMode()
        {
            Dictionary<string, string> config = new Dictionary<string, string>(accountConfig);
            config.Add(BaseConstants.ApplicationModeConfig, BaseConstants.LiveMode);

            credential = credentialMngr.GetCredentials(config, Constants.CertificateAPIUserName);
            MerchantAPICallPreHandler soapHandler = new MerchantAPICallPreHandler(config, defaultSoapHandler, credential);
            Assert.AreEqual(BaseConstants.MerchantCertificateLiveEndpoint, soapHandler.GetEndpoint());

            credential = credentialMngr.GetCredentials(config, Constants.APIUserName);
            soapHandler = new MerchantAPICallPreHandler(config, defaultSoapHandler, credential);
            Assert.AreEqual(BaseConstants.MerchantSignatureLiveEndpoint, soapHandler.GetEndpoint());
        }

        [Test]
        public void GetEndpointForLiveMode()
        {
            Dictionary<string, string> config = new Dictionary<string, string>(accountConfig);
            config.Add(BaseConstants.ApplicationModeConfig, BaseConstants.SandboxMode);

            credential = credentialMngr.GetCredentials(config, Constants.CertificateAPIUserName);
            MerchantAPICallPreHandler soapHandler = new MerchantAPICallPreHandler(config, defaultSoapHandler, credential);
            Assert.AreEqual(BaseConstants.MerchantCertificateSandboxEndpoint, soapHandler.GetEndpoint());

            credential = credentialMngr.GetCredentials(config, Constants.APIUserName);
            soapHandler = new MerchantAPICallPreHandler(config, defaultSoapHandler, credential);
            Assert.AreEqual(BaseConstants.MerchantSignatureSandboxEndpoint, soapHandler.GetEndpoint());
        }

        [ExpectedException(typeof(ConfigException))]
        [Test]
        public void GetEndpointForDefaultModeWithoutEndpoint()
        {
            Dictionary<string, string> config = new Dictionary<string, string>(accountConfig);

            credential = credentialMngr.GetCredentials(config, Constants.CertificateAPIUserName);
            MerchantAPICallPreHandler soapHandler = new MerchantAPICallPreHandler(config, defaultSoapHandler, credential);
            soapHandler.GetEndpoint();
        }

        [Test]
        public void GetEndpointForDefaultModeWithExplicitEndpoint()
        {
            Dictionary<string, string> config = new Dictionary<string, string>(accountConfig);
            config.Add(BaseConstants.EndpointConfig, Constants.APIEndpointNVP);

            credential = credentialMngr.GetCredentials(config, Constants.CertificateAPIUserName);
            MerchantAPICallPreHandler soapHandler = new MerchantAPICallPreHandler(config, defaultSoapHandler, credential);
            Assert.AreEqual(Constants.APIEndpointNVP, soapHandler.GetEndpoint());

            config.Add("PayPalAPI", Constants.APIEndpointSOAP);
            credential = credentialMngr.GetCredentials(config, Constants.CertificateAPIUserName);
            soapHandler = new MerchantAPICallPreHandler(config, defaultSoapHandler, credential);
            soapHandler.PortName = "PayPalAPI";
            Assert.AreEqual(Constants.APIEndpointSOAP, soapHandler.GetEndpoint());
        }

        private XmlDocument GetXmlDocument(string xmlString)
        {
            XmlDocument xmlDoc = new XmlDocument();
            xmlString = xmlString.Replace("soapenv:", string.Empty);
            xmlString = xmlString.Replace("ebl:", string.Empty);
            xmlDoc.LoadXml(xmlString);
            return xmlDoc;
        }
    }
}
#else
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace PayPal.UnitTest
{
    [TestClass]
    public class MerchantAPICallPreHandlerTest
    {
        private DefaultSOAPAPICallHandler defaultSoapHandler;
        private CredentialManager credentialMngr;
        private ICredential credential;
        private MerchantAPICallPreHandler soapHandler;
        private Dictionary<string, string> accountConfig;

        public MerchantAPICallPreHandlerTest()
        {
            defaultSoapHandler = new DefaultSOAPAPICallHandler(ConfigManager.Instance.GetProperties(), "<Request>test</Request>", null, null);
            credentialMngr = CredentialManager.Instance;

            accountConfig = new Dictionary<string, string>();
            accountConfig.Add("account1.apiUsername", Constants.APIUserName);
            accountConfig.Add("account1.apiPassword", Constants.APIPassword);
            accountConfig.Add("account1.applicationId", Constants.ApplicationID);
            accountConfig.Add("account1.apiSignature", Constants.APISignature);
            accountConfig.Add("account2.apiUsername", Constants.CertificateAPIUserName);
            accountConfig.Add("account2.apiPassword", Constants.CertificateAPIPassword);
            accountConfig.Add("account2.applicationId", Constants.ApplicationID);
            accountConfig.Add("account2.apiCertificate", Constants.CertificatePath);
            accountConfig.Add("account2.privateKeyPassword", Constants.CertificatePassword);
        }

        [TestMethod]
        public void GetHeaderMapSignature()
        {
            credential = credentialMngr.GetCredentials(ConfigManager.Instance.GetProperties(), Constants.APIUserName);
            soapHandler = new MerchantAPICallPreHandler(ConfigManager.Instance.GetProperties(), defaultSoapHandler, credential);
            Dictionary<string, string> headers = soapHandler.GetHeaderMap();
            Assert.IsNotNull(headers);
            Assert.IsTrue(headers.Count > 0);
            Assert.AreEqual(Constants.APIUserName, headers[BaseConstants.PayPalSecurityUserIDHeader]);
            Assert.AreEqual(Constants.APIPassword, headers[BaseConstants.PayPalSecurityPasswordHeader]);
            Assert.AreEqual(Constants.APISignature, headers[BaseConstants.PayPalSecuritySignatureHeader]);
            Assert.AreEqual(BaseConstants.SOAP, headers[BaseConstants.PayPalRequestDataFormatHeader]);
            Assert.AreEqual(BaseConstants.SOAP, headers[BaseConstants.PayPalResponseDataFormatHeader]);
        }

        [TestMethod]
        public void GetHeaderMapCertificate()
        {
            credential = credentialMngr.GetCredentials(ConfigManager.Instance.GetProperties(), Constants.CertificateAPIUserName);
            soapHandler = new MerchantAPICallPreHandler(ConfigManager.Instance.GetProperties(), defaultSoapHandler, credential);
            Dictionary<string, string> headers = soapHandler.GetHeaderMap();
            Assert.IsNotNull(headers);
            Assert.IsTrue(headers.Count > 0);
            Assert.AreEqual(Constants.CertificateAPIUserName, headers[BaseConstants.PayPalSecurityUserIDHeader]);
            Assert.AreEqual(Constants.CertificateAPIPassword, headers[BaseConstants.PayPalSecurityPasswordHeader]);
            Assert.AreEqual(BaseConstants.SOAP, headers[BaseConstants.PayPalRequestDataFormatHeader]);
            Assert.AreEqual(BaseConstants.SOAP, headers[BaseConstants.PayPalResponseDataFormatHeader]);
            Assert.AreEqual(soapHandler.SDKName + "-" + soapHandler.SDKVersion, headers[BaseConstants.PayPalRequestSourceHeader]);
        }

        [TestMethod]
        public void GetPayLoadSignature()
        {
            credential = credentialMngr.GetCredentials(ConfigManager.Instance.GetProperties(), Constants.APIUserName);
            soapHandler = new MerchantAPICallPreHandler(ConfigManager.Instance.GetProperties(), defaultSoapHandler, credential);
            string payload = soapHandler.GetPayLoad();
            XmlDocument xmlDoc = GetXmlDocument(payload);
            XmlNodeList xmlNodeListUsername = xmlDoc.GetElementsByTagName("Username");
            Assert.IsTrue(xmlNodeListUsername.Count > 0);
            Assert.AreEqual(Constants.APIUserName, xmlNodeListUsername[0].InnerXml);
            XmlNodeList xmlNodeListPassword = xmlDoc.GetElementsByTagName("Password");
            Assert.IsTrue(xmlNodeListPassword.Count > 0);
            Assert.AreEqual(Constants.APIPassword, xmlNodeListPassword[0].InnerXml);
            XmlNodeList xmlNodeListSignature = xmlDoc.GetElementsByTagName("Signature");
            Assert.IsTrue(xmlNodeListSignature.Count > 0);
            Assert.AreEqual(Constants.APISignature, xmlNodeListSignature[0].InnerXml);
            XmlNodeList xmlNodeListRequest = xmlDoc.GetElementsByTagName("Request");
            Assert.IsTrue(xmlNodeListRequest.Count > 0);
            Assert.AreEqual("test", xmlNodeListRequest[0].InnerXml);
        }

        [TestMethod]
        public void GetPayLoadForCertificate()
        {
            credential = credentialMngr.GetCredentials(ConfigManager.Instance.GetProperties(), Constants.CertificateAPIUserName);
            soapHandler = new MerchantAPICallPreHandler(ConfigManager.Instance.GetProperties(), defaultSoapHandler, credential);
            string payload = soapHandler.GetPayLoad();
            XmlDocument xmlDoc = GetXmlDocument(payload);
            XmlNodeList xmlNodeListUsername = xmlDoc.GetElementsByTagName("Username");
            Assert.IsTrue(xmlNodeListUsername.Count > 0);
            Assert.AreEqual(Constants.CertificateAPIUserName, xmlNodeListUsername[0].InnerXml);
            XmlNodeList xmlNodeListPassword = xmlDoc.GetElementsByTagName("Password");
            Assert.IsTrue(xmlNodeListPassword.Count > 0);
            Assert.AreEqual(Constants.CertificateAPIPassword, xmlNodeListPassword[0].InnerXml);
            XmlNodeList xmlNodeListRequest = xmlDoc.GetElementsByTagName("Request");
            Assert.IsTrue(xmlNodeListRequest.Count > 0);
            Assert.AreEqual("test", xmlNodeListRequest[0].InnerXml);
        }

        [TestMethod]
        public void SDKName()
        {
            credential = credentialMngr.GetCredentials(ConfigManager.Instance.GetProperties(), Constants.CertificateAPIUserName);
            MerchantAPICallPreHandler soapHandler = new MerchantAPICallPreHandler(ConfigManager.Instance.GetProperties(), defaultSoapHandler, credential);
            soapHandler.SDKName = "testsdk";
            Assert.AreEqual("testsdk", soapHandler.SDKName);
        }

        [TestMethod]
        public void SDKVersion()
        {
            credential = credentialMngr.GetCredentials(ConfigManager.Instance.GetProperties(), Constants.CertificateAPIUserName);
            MerchantAPICallPreHandler soapHandler = new MerchantAPICallPreHandler(ConfigManager.Instance.GetProperties(), defaultSoapHandler, credential);
            soapHandler.SDKVersion = "1.0.0";
            Assert.AreEqual("1.0.0", soapHandler.SDKVersion);
        }

        [TestMethod]
        public void GetEndpoint()
        {
            credential = credentialMngr.GetCredentials(ConfigManager.Instance.GetProperties(), Constants.CertificateAPIUserName);
            MerchantAPICallPreHandler soapHandler = new MerchantAPICallPreHandler(ConfigManager.Instance.GetProperties(), defaultSoapHandler, credential);
            string endpoint = soapHandler.GetEndpoint();
            Assert.AreEqual(Constants.APIEndpointNVP, endpoint);
        }

        [TestMethod]
        public void GetEndpointForSandboxMode()
        {
            Dictionary<string, string> config = new Dictionary<string, string>(accountConfig);
            config.Add(BaseConstants.ApplicationModeConfig, BaseConstants.LiveMode);

            credential = credentialMngr.GetCredentials(config, Constants.CertificateAPIUserName);
            MerchantAPICallPreHandler soapHandler = new MerchantAPICallPreHandler(config, defaultSoapHandler, credential);
            Assert.AreEqual(BaseConstants.MerchantCertificateLiveEndpoint, soapHandler.GetEndpoint());

            credential = credentialMngr.GetCredentials(config, Constants.APIUserName);
            soapHandler = new MerchantAPICallPreHandler(config, defaultSoapHandler, credential);
            Assert.AreEqual(BaseConstants.MerchantSignatureLiveEndpoint, soapHandler.GetEndpoint());
        }

        [TestMethod]
        public void GetEndpointForLiveMode()
        {
            Dictionary<string, string> config = new Dictionary<string, string>(accountConfig);
            config.Add(BaseConstants.ApplicationModeConfig, BaseConstants.SandboxMode);

            credential = credentialMngr.GetCredentials(config, Constants.CertificateAPIUserName);
            MerchantAPICallPreHandler soapHandler = new MerchantAPICallPreHandler(config, defaultSoapHandler, credential);
            Assert.AreEqual(BaseConstants.MerchantCertificateSandboxEndpoint, soapHandler.GetEndpoint());

            credential = credentialMngr.GetCredentials(config, Constants.APIUserName);
            soapHandler = new MerchantAPICallPreHandler(config, defaultSoapHandler, credential);
            Assert.AreEqual(BaseConstants.MerchantSignatureSandboxEndpoint, soapHandler.GetEndpoint());
        }


        [TestMethod]
        [ExpectedException(typeof(ConfigException))]
        public void GetEndpointForDefaultModeWithoutEndpoint()
        {
            try
            {
                Dictionary<string, string> config = new Dictionary<string, string>(accountConfig);

                credential = credentialMngr.GetCredentials(config, Constants.CertificateAPIUserName);
                MerchantAPICallPreHandler soapHandler = new MerchantAPICallPreHandler(config, defaultSoapHandler, credential);
                soapHandler.GetEndpoint();
            }
            catch (ConfigException ex)
            {
                Assert.AreEqual("You must specify one of mode(live/sandbox) OR endpoint in the configuration", ex.Message);
                throw;
            }
        }

        [TestMethod]
        public void GetEndpointForDefaultModeWithExplicitEndpoint()
        {
            Dictionary<string, string> config = new Dictionary<string, string>(accountConfig);
            config.Add(BaseConstants.EndpointConfig, Constants.APIEndpointNVP);

            credential = credentialMngr.GetCredentials(config, Constants.CertificateAPIUserName);
            MerchantAPICallPreHandler soapHandler = new MerchantAPICallPreHandler(config, defaultSoapHandler, credential);
            Assert.AreEqual(Constants.APIEndpointNVP, soapHandler.GetEndpoint());

            config.Add("PayPalAPI", Constants.APIEndpointSOAP);
            credential = credentialMngr.GetCredentials(config, Constants.CertificateAPIUserName);
            soapHandler = new MerchantAPICallPreHandler(config, defaultSoapHandler, credential);
            soapHandler.PortName = "PayPalAPI";
            Assert.AreEqual(Constants.APIEndpointSOAP, soapHandler.GetEndpoint());
        }

        private XmlDocument GetXmlDocument(string xmlString)
        {
            XmlDocument xmlDoc = new XmlDocument();
            xmlString = xmlString.Replace("soapenv:", string.Empty);
            xmlString = xmlString.Replace("ebl:", string.Empty);
            xmlDoc.LoadXml(xmlString);
            return xmlDoc;
        }
    }
}
#endif