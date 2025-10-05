using OSMetricsRetriever.MetricsPlugins;
using Moq;
using OSMetricsRetriever.Providers;
using System.Management;

namespace OSMetricsRetrieverTests.MetricsPlugins
{
    [TestClass]
    public class CPUUtilizationPluginTests
    {
        private CPUUtilizationPlugin _plugin;
        private Mock<IWMIProvider> _wmiProviderMock;

        [TestInitialize]
        public void Setup()
        {
            _plugin = new CPUUtilizationPlugin();
            _wmiProviderMock = new Mock<IWMIProvider>();
        }

        [TestMethod]
        public void Name_ReturnsCorrectValue()
        {
            // Assert
            Assert.AreEqual("CPU Utilization", CPUUtilizationPlugin.Name);
        }

        [TestMethod]
        public void GetMetric_UsesCorrectWMIQuery()
        {
            // Arrange
            ObjectQuery capturedQuery = null;
            
            _wmiProviderMock
                .Setup(p => p.QueryWMI(It.IsAny<ObjectQuery>()))
                .Callback<ObjectQuery>(query => capturedQuery = query)
                .Returns(new List<ManagementObject>());

            // Act
            try
            {
                _plugin.GetMetric(_wmiProviderMock.Object);
            }
            catch (InvalidOperationException)
            {
                // Expected when empty list is returned - ignore for this test
            }

            // Assert
            Assert.IsNotNull(capturedQuery);
            Assert.IsTrue(capturedQuery.QueryString.Contains("SELECT LoadPercentage FROM Win32_Processor"));
        }

        [TestMethod]
        [ExpectedException(typeof(NullReferenceException))]
        public void GetMetric_ThrowsNullReferenceException_WhenProviderIsNull()
        {
            // Act & Assert - The plugin doesn't validate null input, so it throws NullReferenceException
            _plugin.GetMetric(null);
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public void GetMetric_ThrowsInvalidOperationException_WhenNoProcessorsFound()
        {
            // Arrange
            _wmiProviderMock
                .Setup(p => p.QueryWMI(It.IsAny<ObjectQuery>()))
                .Returns(new List<ManagementObject>());

            // Act & Assert - The plugin throws InvalidOperationException when Average() is called on empty sequence
            _plugin.GetMetric(_wmiProviderMock.Object);
        }

        [TestMethod]
        [ExpectedException(typeof(ManagementException))]
        public void GetMetric_ThrowsManagementException_WhenWMIProviderThrowsException()
        {
            // Arrange
            _wmiProviderMock
                .Setup(p => p.QueryWMI(It.IsAny<ObjectQuery>()))
                .Throws(new ManagementException("WMI query failed"));

            // Act & Assert - The plugin doesn't wrap exceptions, so ManagementException is thrown directly
            _plugin.GetMetric(_wmiProviderMock.Object);
        }
    }
}
