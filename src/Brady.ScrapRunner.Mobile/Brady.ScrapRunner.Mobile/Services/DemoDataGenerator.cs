namespace Brady.ScrapRunner.Mobile.Services
{
    using System;
    using System.Threading.Tasks;
    using Interfaces;
    using Models;
    using MvvmCross.Plugins.Sqlite;

    public class DemoDataGenerator
    {
        private readonly IMvxSqliteConnectionFactory _sqliteConnectionFactory;
        private readonly IRepository<TripModel> _tripRepository;
        private readonly IRepository<TripSegmentModel> _tripSegmentRepository;
        private readonly IRepository<TripSegmentContainerModel> _tripSegmentContainerRepository;

        public DemoDataGenerator(
            IMvxSqliteConnectionFactory sqliteConnectionFactory,
            IRepository<TripModel> tripRepository, 
            IRepository<TripSegmentModel> tripSegmentRepository, 
            IRepository<TripSegmentContainerModel> tripSegmentContainerRepository)
        {
            _tripRepository = tripRepository;
            _tripSegmentRepository = tripSegmentRepository;
            _tripSegmentContainerRepository = tripSegmentContainerRepository;
            _sqliteConnectionFactory = sqliteConnectionFactory;
        }

        public async Task GenerateDemoDataAsync()
        {
            var asyncConnection = _sqliteConnectionFactory.GetAsyncConnection("scraprunner");
            await asyncConnection.CreateTableAsync<ContainerMasterModel>();
            await asyncConnection.CreateTableAsync<CustomerDirectionModel>();
            await asyncConnection.CreateTableAsync<DriverStatusModel>();
            await asyncConnection.CreateTableAsync<EmployeeMasterModel>();
            await asyncConnection.CreateTableAsync<PowerMasterModel>();
            await asyncConnection.CreateTableAsync<PreferenceModel>();
            await asyncConnection.CreateTableAsync<TripModel>();
            await asyncConnection.CreateTableAsync<TripSegmentModel>();
            await asyncConnection.CreateTableAsync<TripSegmentContainerModel>();

            await asyncConnection.DeleteAllAsync<ContainerMasterModel>();
            await asyncConnection.DeleteAllAsync<CustomerDirectionModel>();
            await asyncConnection.DeleteAllAsync<DriverStatusModel>();
            await asyncConnection.DeleteAllAsync<EmployeeMasterModel>();
            await asyncConnection.DeleteAllAsync<PowerMasterModel>();
            await asyncConnection.DeleteAllAsync<PreferenceModel>();
            await asyncConnection.DeleteAllAsync<TripModel>();
            await asyncConnection.DeleteAllAsync<TripSegmentModel>();
            await asyncConnection.DeleteAllAsync<TripSegmentContainerModel>();
//            await GenerateDemoTripDataAsync();
        }

//        private async Task GenerateDemoTripDataAsync()
//        {
//            await _tripRepository.InsertAsync(new TripModel
//            {
//                TripCustName = "Kaman Aerospace",
//                TripNumber = "615112",
//                TripAssignStatus = "D",
//                TripStatus = "P",
//                TripContactName = "",
//                TripCustAddress1 = "1701 Indianwood Circle",
//                TripCustAddress2 = "",
//                TripCustCity = "Maumee",
//                TripCustState = "OH",
//                TripCustZip = "43537",
//                TripCustCloseTime = new DateTime(2016, 2, 7, 20, 0, 0),
//                TripCustOpenTime = new DateTime(2016, 2, 7, 9, 0, 0),
//                TripDriverInstructions = "Do something special with this note",
//                TripType = "SW",
//                TripTypeDesc = "SWITCH"
//            });
//            await _tripSegmentRepository.InsertAsync(new TripSegmentModel
//            {
//                TripNumber = "615112",
//                TripSegNumber = "01",
//                TripSegType = "DE",
//                TripSegTypeDesc = "DROP EMPTY"
//            });
//            await _tripSegmentRepository.InsertAsync(new TripSegmentModel
//            {
//                TripNumber = "615112",
//                TripSegNumber = "02",
//                TripSegType = "PF",
//                TripSegTypeDesc = "PICKUP FULL"
//            });
//            await _tripSegmentContainerRepository.InsertAsync(new TripSegmentContainerModel
//            {
//                TripNumber = "615112",
//                TripSegNumber = "01",
//                TripSegContainerNumber = string.Empty,
//                TripSegContainerType = "LU",
//                TripSegContainerSize = "10",
//                TripSegContainerCommodityDesc = "#15 SHEARING IRON",
//                TripSegContainerLocation = "DOCK DOOR 14"
//            });
//            await _tripSegmentContainerRepository.InsertAsync(new TripSegmentContainerModel
//            {
//                TripNumber = "615112",
//                TripSegNumber = "01",
//                TripSegContainerNumber = string.Empty,
//                TripSegContainerType = "LU",
//                TripSegContainerSize = "20",
//                TripSegContainerCommodityDesc = "#20 TIN/ALUMINUM MIX",
//                TripSegContainerLocation = "DOCK DOOR 3"
//            });
//            await _tripSegmentContainerRepository.InsertAsync(new TripSegmentContainerModel
//            {
//                TripNumber = "615112",
//                TripSegNumber = "02",
//                TripSegContainerNumber = string.Empty,
//                TripSegContainerType = "LU",
//                TripSegContainerSize = "20",
//                TripSegContainerCommodityDesc = "#14 SHEARING IRON",
//                TripSegContainerLocation = "DOCK DOOR 20"
//            });
//            await _tripRepository.InsertAsync(new TripModel
//            {
//                TripCustName = "Jay's Scrap Metal",
//                TripNumber = "615113",
//                TripAssignStatus = "D",
//                TripStatus = "P",
//                TripContactName = "",
//                TripCustAddress1 = "6560 Brixton Rd",
//                TripCustAddress2 = "",
//                TripCustCity = "Maumee",
//                TripCustState = "OH",
//                TripCustZip = "43537",
//                TripCustCloseTime = new DateTime(2016, 2, 7, 20, 0, 0),
//                TripCustOpenTime = new DateTime(2016, 2, 7, 20, 0, 0),
//                TripDriverInstructions = "This should be an easy trip",
//                TripType = "RT",
//                TripTypeDesc = "RETURN TO YARD"
//            });
//            await _tripSegmentRepository.InsertAsync(new TripSegmentModel
//            {
//                TripNumber = "615113",
//                TripSegNumber = "01",
//                TripSegType = "RT",
//                TripSegTypeDesc = "RETURN TO YARD",
//                TripSegComments = "Special Notes? Special Notes.",
//                TripSegOrigCustName = "Jay's Scrap Metal",
//                TripSegOrigCustHostCode = "JAY450"
//            });
//            await _tripSegmentContainerRepository.InsertAsync(new TripSegmentContainerModel
//            {
//                TripNumber = "615113",
//                TripSegNumber = "01",
//                TripSegContainerNumber = string.Empty,
//                TripSegContainerType = "LU",
//                TripSegContainerSize = "20",
//                TripSegContainerCommodityDesc = "3210 Tin/Iron Mix",
//                TripSegContainerLocation = "DOCK DOOR 20"
//            });
//            await _tripRepository.InsertAsync(new TripModel
//            {
//                TripCustName = "Jimbo's Recycling",
//                TripNumber = "615114",
//                TripAssignStatus = "D",
//                TripStatus = "P",
//                TripContactName = "",
//                TripCustAddress1 = "1234 Dingbing Rd",
//                TripCustAddress2 = "",
//                TripCustCity = "Findlay",
//                TripCustState = "OH",
//                TripCustZip = "43900",
//                TripCustCloseTime = new DateTime(2016, 2, 7, 17, 0, 0),
//                TripCustOpenTime = new DateTime(2016, 2, 7, 9, 0, 0),
//                TripDriverInstructions = "Just drop a few containers and party",
//                TripType = "DE",
//                TripTypeDesc = "DROP EMPTY"
//            });
//            await _tripRepository.InsertAsync(new TripModel
//            {
//                TripCustName = "SIMS Metal Management",
//                TripNumber = "615115",
//                TripAssignStatus = "D",
//                TripStatus = "P",
//                TripContactName = "",
//                TripCustAddress1 = "1701 Indianwood Circle",
//                TripCustAddress2 = "",
//                TripCustCity = "Maumee",
//                TripCustState = "OH",
//                TripCustZip = "43537",
//                TripCustCloseTime = new DateTime(2016, 2, 7, 20, 0, 0),
//                TripCustOpenTime = new DateTime(2016, 2, 7, 9, 0, 0),
//                TripDriverInstructions = "WHHHHAAATTTTT? This is a test to see if the content will wrap correctly, otherwise back to the drawing board ...",
//                TripType = "SW",
//                TripTypeDesc = "SWITCH"
//            });
//
//        }

    }
}
