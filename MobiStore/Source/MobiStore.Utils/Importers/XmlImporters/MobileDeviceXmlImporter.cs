﻿using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Serialization;

using MobiStore.Data.Contracts;
using MobiStore.Factories.Contracts;
using MobiStore.Factories.Factories;
using MobiStore.Models.MobileDevices;
using MobiStore.Models.MobileDevices.Components;

using MongoDB.Driver;

namespace MobiStore.Utilities.Importers.XmlImporters
{
    public class MobileDeviceXmlImporter : XmlImporter
    {
        private IModelsFactory mobileDeviceFactory;

        public MobileDeviceXmlImporter(ISqlServerDb sqlServerData, IMongoDatabase mongoData, XmlSerializer serializer)
            : base(sqlServerData, mongoData, serializer)
        {
            this.mobileDeviceFactory = new MsSqlModelsFactory(sqlServerData);
        }

        public override void Import(DirectoryInfo xmlFilePath)
        {
            using (var fileStream = new FileStream(xmlFilePath.FullName, FileMode.Open))
            {
                var shop = (XmlModels.Shop)this.XmlSerializer.Deserialize(fileStream);
                var mobileDevices = shop.MobileDevices.ToList();

                var batteriesToInsert = new List<Battery>(mobileDevices.Count);
                var displaysToInsert = new List<Display>(mobileDevices.Count);
                var processorsToInsert = new List<Processor>(mobileDevices.Count);
                var mobileDevicesToInsert = new List<MobileDevice>(mobileDevices.Count);

                foreach (var mobileDevice in mobileDevices)
                {
                    Battery battery = this.mobileDeviceFactory.CreateBattery(
                        mobileDevice.Battery.Type,
                        mobileDevice.Battery.Capacity);
                    batteriesToInsert.Add(battery);

                    Display display = this.mobileDeviceFactory.CreateDisplay(
                        mobileDevice.Display.Type,
                        mobileDevice.Display.Size,
                        mobileDevice.Display.Resolution);
                    displaysToInsert.Add(display);

                    Processor processor = this.mobileDeviceFactory.CreateProcessor(
                        mobileDevice.Processor.CacheMemory,
                        mobileDevice.Processor.ClockSpeed);
                    processorsToInsert.Add(processor);

                    MobileDevice mobileDeviceToInsert = this.mobileDeviceFactory.CreateMobileDevice(
                        mobileDevice.Brand,
                        mobileDevice.Model,
                        display,
                        battery,
                        processor);
                    mobileDevicesToInsert.Add(mobileDeviceToInsert);
                }

                this.InsertCollectionIntoMongoAsync<Battery>(batteriesToInsert, "batteries");
                this.InsertCollectionIntoMongoAsync<Display>(displaysToInsert, "displays");
                this.InsertCollectionIntoMongoAsync<Processor>(processorsToInsert, "processors");
                this.InsertCollectionIntoMongoAsync<MobileDevice>(mobileDevicesToInsert, "mobileDevices");

                this.SqlServerDatabase.Context.Batteries.AddRange(batteriesToInsert);
                this.SqlServerDatabase.Context.Displays.AddRange(displaysToInsert);
                this.SqlServerDatabase.Context.Processors.AddRange(processorsToInsert);
                this.SqlServerDatabase.Context.MobileDevices.AddRange(mobileDevicesToInsert);

                this.SqlServerDatabase.SaveChanges();
            }
        }

        private async void InsertCollectionIntoMongoAsync<T>(IEnumerable<object> collectionToInsert, string documentName)
        {
            var collection = this.MongoDatabase.GetCollection<T>(documentName);
            await collection.InsertManyAsync(collectionToInsert as IEnumerable<T>);
        }
    }
}