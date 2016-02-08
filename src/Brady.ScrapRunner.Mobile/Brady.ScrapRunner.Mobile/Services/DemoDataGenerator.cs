namespace Brady.ScrapRunner.Mobile.Services
{
    using System;
    using System.Threading.Tasks;
    using Interfaces;
    using Models;

    public class DemoDataGenerator
    {
        private readonly IRepository<TripModel> _tripRepository;
        private readonly IRepository<TripSegmentModel> _tripSegmentRepository;
        private readonly IRepository<TripSegmentContainerModel> _tripSegmentContainerRepository;

        public DemoDataGenerator(
            IRepository<TripModel> tripRepository, 
            IRepository<TripSegmentModel> tripSegmentRepository, 
            IRepository<TripSegmentContainerModel> tripSegmentContainerRepository)
        {
            _tripRepository = tripRepository;
            _tripSegmentRepository = tripSegmentRepository;
            _tripSegmentContainerRepository = tripSegmentContainerRepository;
        }

        public async Task GenerateDemoDataAsync()
        {
            await GenerateDemoTripDataAsync();
        }

        private async Task GenerateDemoTripDataAsync()
        {
            await _tripRepository.InsertAsync(new TripModel
            {
                TripCustName = "Kaman Aerospace",
                TripNumber = "615112",
                TripAssignStatus = "D",
                TripStatus = "P",
                TripContactName = "",
                TripCustAddress1 = "1701 Indianwood Circle",
                TripCustAddress2 = "",
                TripCustCity = "Maumee",
                TripCustState = "OH",
                TripCustZip = "43537",
                TripCustCloseTime = new DateTime(2016, 2, 7, 20, 0, 0),
                TripCustOpenTime = new DateTime(2016, 2, 7, 9, 0, 0),
                TripDriverInstructions = "Do something special with this note",
                TripType = "SW",
                TripTypeDesc = "SWITCH"
            });
            await _tripSegmentRepository.InsertAsync(new TripSegmentModel
            {
                TripNumber = "615112",
                TripSegNumber = "01",
                TripSegType = "DE",
                TripSegTypeDesc = "DROP EMPTY"
            });
            await _tripSegmentRepository.InsertAsync(new TripSegmentModel
            {
                TripNumber = "615112",
                TripSegNumber = "02",
                TripSegType = "PF",
                TripSegTypeDesc = "PICKUP FULL"
            });
            await _tripSegmentContainerRepository.InsertAsync(new TripSegmentContainerModel
            {
                TripNumber = "615112",
                TripSegNumber = "01",
                TripSegContainerNumber = string.Empty,
                TripSegContainerType = "LU",
                TripSegContainerSize = "10",
                TripSegContainerCommodityDesc = "#15 SHEARING IRON",
                TripSegContainerLocation = "DOCK DOOR 14"
            });
            await _tripSegmentContainerRepository.InsertAsync(new TripSegmentContainerModel
            {
                TripNumber = "615112",
                TripSegNumber = "02",
                TripSegContainerNumber = string.Empty,
                TripSegContainerType = "LU",
                TripSegContainerSize = "20",
                TripSegContainerCommodityDesc = "#14 SHEARING IRON",
                TripSegContainerLocation = "DOCK DOOR 20"
            });
            await _tripRepository.InsertAsync(new TripModel
            {
                TripCustName = "Jay's Scrap Metal",
                TripNumber = "615113",
                TripAssignStatus = "D",
                TripStatus = "P",
                TripContactName = "",
                TripCustAddress1 = "6560 Brixton Rd",
                TripCustAddress2 = "",
                TripCustCity = "Maumee",
                TripCustState = "OH",
                TripCustZip = "43537",
                TripCustCloseTime = new DateTime(2016, 2, 7, 20, 0, 0),
                TripCustOpenTime = new DateTime(2016, 2, 7, 20, 0, 0),
                TripDriverInstructions = "This should be an easy trip",
                TripType = "RT",
                TripTypeDesc = "RETURN TO YARD"
            });
            await _tripSegmentRepository.InsertAsync(new TripSegmentModel
            {
                TripNumber = "615113",
                TripSegNumber = "01",
                TripSegType = "RT",
                TripSegTypeDesc = "RETURN TO YARD"
            });
            await _tripRepository.InsertAsync(new TripModel
            {
                TripCustName = "Jimbo's Recycling",
                TripNumber = "615114",
                TripAssignStatus = "D",
                TripStatus = "P",
                TripContactName = "",
                TripCustAddress1 = "1234 Dingbing Rd",
                TripCustAddress2 = "",
                TripCustCity = "Findlay",
                TripCustState = "OH",
                TripCustZip = "43900",
                TripCustCloseTime = new DateTime(2016, 2, 7, 17, 0, 0),
                TripCustOpenTime = new DateTime(2016, 2, 7, 9, 0, 0),
                TripDriverInstructions = "Just drop a few containers and party",
                TripType = "DE",
                TripTypeDesc = "DROP EMPTY"
            });
            await _tripRepository.InsertAsync(new TripModel
            {
                TripCustName = "SIMS Metal Management",
                TripNumber = "615115",
                TripAssignStatus = "D",
                TripStatus = "P",
                TripContactName = "",
                TripCustAddress1 = "1701 Indianwood Circle",
                TripCustAddress2 = "",
                TripCustCity = "Maumee",
                TripCustState = "OH",
                TripCustZip = "43537",
                TripCustCloseTime = new DateTime(2016, 2, 7, 20, 0, 0),
                TripCustOpenTime = new DateTime(2016, 2, 7, 9, 0, 0),
                TripDriverInstructions = "WHHHHAAATTTTT? This is a test to see if the content will wrap correctly, otherwise back to the drawing board ...",
                TripType = "SW",
                TripTypeDesc = "SWITCH"
            });

        }
    }
}
