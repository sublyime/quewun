using System;
using DataQuillDesktop.Services;
using DataQuillDesktop.Models;

namespace DataQuillDesktop
{
    class TestModbus
    {
        public static async Task TestModbusTcp()
        {
            try
            {
                Console.WriteLine("=== Testing Modbus TCP Functionality ===");
                
                var dataSourceService = new DataSourceService();
                
                // Create a test data source for your Modbus TCP simulator
                var testDataSource = new DataSource
                {
                    Name = "Test Modbus TCP",
                    InterfaceType = InterfaceType.TCP,
                    ProtocolType = ProtocolType.ModbusTCP,
                    Configuration = new DataSourceConfiguration
                    {
                        Host = "127.0.0.1",  // Change this to your simulator's IP if different
                        Port = 502,          // Standard Modbus TCP port
                        SlaveId = 1,         // Your slave ID
                        Timeout = 5
                    }
                };
                
                Console.WriteLine($"Testing connection to {testDataSource.Configuration.Host}:{testDataSource.Configuration.Port} (Slave ID: {testDataSource.Configuration.SlaveId})");
                
                bool isConnected = await dataSourceService.TestConnectionAsync(testDataSource);
                
                if (isConnected)
                {
                    Console.WriteLine("✅ SUCCESS: Modbus TCP connection test passed!");
                    Console.WriteLine("Your simulator is responding correctly.");
                }
                else
                {
                    Console.WriteLine("❌ FAILED: Could not connect to Modbus TCP simulator");
                    Console.WriteLine("Check that:");
                    Console.WriteLine("1. Your simulator is running");
                    Console.WriteLine("2. IP address and port are correct");
                    Console.WriteLine("3. Slave ID matches your simulator");
                }
                
                Console.WriteLine("=== Test Complete ===");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error during test: {ex.Message}");
            }
        }
    }
}