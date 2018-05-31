using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using IoTHubMonitoring.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Devices;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Microsoft.WindowsAzure.Storage;
using System.IO;
using System.Data.SqlClient;

namespace IoTHubMonitoring.Controllers
{
    public class DevicesController : Controller
    {
        private IConfiguration _configuration;

        public DevicesController(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public async Task<IActionResult> Index()
        {
            var rm = RegistryManager.CreateFromConnectionString(_configuration["IoTHubConnectionString"]);

            var devicesQuery = rm.CreateQuery("SELECT DeviceId FROM devices");
            var devices = await devicesQuery.GetNextAsJsonAsync();

            var list = new List<DeviceListDto>();

            foreach (var device in devices)
            {
                var json = JsonConvert.DeserializeObject<JObject>(device);
                var dto = new DeviceListDto
                {
                    DeviceId = json.Value<string>("DeviceId")
                };
                list.Add(dto);
            }

            return View(list);
        }
        public async Task<IActionResult> Details(string id)
        {
            var dto = new DeviceDetailsDTO();
            dto.DeviceId = id;
            return View(dto);
        }

        public async Task<JsonResult> CurrentValue(string id)
        {
            var dto = new DeviceCurrentValueDTO();
            dto.DeviceId = id;

            var storageAccount = CloudStorageAccount.Parse(_configuration["StorageConnectionString"]);

            var blobClient = storageAccount.CreateCloudBlobClient();

            var eventStoreContainer = blobClient.GetContainerReference("averages");

            var blob = eventStoreContainer.GetBlockBlobReference($"{id}.json");

            var json = await blob.DownloadTextAsync();
            var current = JsonConvert.DeserializeObject<JObject>(json);

            dto.CurrentValue = Math.Round(current.Value<decimal>("Average"), 2);
            dto.Timestamp = current.Value<DateTime>("Timestamp");


            return Json(dto);
        }

        public async Task<JsonResult> LastValues(string id)
        {
            var dto = new DeviceLastValuesDTO();

            var builder = new ConfigurationBuilder()
                        .SetBasePath(Directory.GetCurrentDirectory())
                        .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true);

            var configuration = builder.Build();

            List<Object> list = new List<Object>();



            SqlConnection conn = new SqlConnection(configuration["SqlConnectionString"]);

            try
            {
                conn.Open();
                SqlCommand cmd = new SqlCommand("SELECT TOP 10 * FROM Averages ORDER BY Timestamp DESC", conn);
                SqlDataReader reader = cmd.ExecuteReader();
                while (reader.Read())
                {
                    list.Add(new DeviceLastValuesDTO()
                    {
                        DeviceId = reader.GetString(reader.GetOrdinal("DeviceId")),
                        Average = reader.GetDecimal(reader.GetOrdinal("Value")),
                        Timestamp = reader.GetDateTimeOffset(reader.GetOrdinal("Timestamp"))
                    });
            }

                reader.Close();
            }
            catch
            {
                return null;

            }
            finally
            {
                conn.Close();
            }

            return Json(list);


        }

        public async Task<JsonResult> LastValue()
        {
            var dto = new DeviceLastValuesDTO();

            var builder = new ConfigurationBuilder()
                        .SetBasePath(Directory.GetCurrentDirectory())
                        .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true);

            var configuration = builder.Build();

            SqlConnection conn = new SqlConnection(configuration["SqlConnectionString"]);

            try
            {
                conn.Open();
                SqlCommand cmd = new SqlCommand("SELECT TOP 1 * FROM Averages ORDER BY Id DESC", conn);
                SqlDataReader reader = cmd.ExecuteReader();
                while (reader.Read())
                {

                    dto.DeviceId = reader.GetString(reader.GetOrdinal("DeviceId"));
                    dto.Average = reader.GetDecimal(reader.GetOrdinal("Value"));
                    dto.Timestamp = reader.GetDateTimeOffset(reader.GetOrdinal("Timestamp"));
                    
                }

                reader.Close();
            }
            catch
            {
                return null;

            }
            finally
            {
                conn.Close();
            }

            return Json(dto);

        }

    }
}