using AutoMapper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NHibernate;
using NHibernate.Util;
using BWF.DataServices.Core.Concrete.ChangeSets;
using BWF.DataServices.Metadata.Attributes.Actions;
using BWF.DataServices.Support.NHibernate.Abstract;
using BWF.DataServices.Core.Interfaces;
using BWF.DataServices.Core.Models;
using BWF.DataServices.Domain.Models;
using BWF.DataServices.Metadata.Models;
using Brady.ScrapRunner.Domain;
using Brady.ScrapRunner.Domain.Models;
using Brady.ScrapRunner.Domain.Process;
using Brady.ScrapRunner.DataService.Interfaces;
using Brady.ScrapRunner.DataService.Validators;
using Brady.ScrapRunner.DataService.Util;
using Brady.ScrapRunner.Domain.Enums;
using System.IO;
using System.Drawing;
using System.Drawing.Imaging;


namespace Brady.ScrapRunner.DataService.ProcessTypes
{
    /// <summary>
    /// Processing for a driver image.  Call this process "withoutrequery".
    /// </summary> 
    /// Note this processes is relatively independent of the "trivial" backing query and results
    /// are simply built up in memory.  As such, make this service call using the form of 
    /// PUT .../{dataServiceName}/{typeName}/{id}/withoutrequery
    /// 
    /// cURL example: 
    ///     PUT https://maunb-stm10.bradyplc.com:7776//api/scraprunner/DriverImageProcess/001/withoutrequery
    /// Portable Client example: 
    ///     var updateResult = client.UpdateAsync(itemToUpdate, requeryUpdated:false).Result;
    ///  
    /// This mode will prevent the Nancy.DataServiceModule from issuing an automatic re-retrieve via getSingleAsync() 
    /// within the postSingleAsync().  These re-retrieves of a trival query clobber our post-processed ChangeSetResult
    /// in memory.

    [EditAction("DriverImageProcess")]
    public class DriverImageProcessRecordType : ChangeableRecordType
        <DriverImageProcess, string, DriverImageProcessValidator, DriverImageProcessDeletionValidator>
    {
        /// <summary>
        /// Mandatory implementation of virtual base class method.
        /// </summary>
        public override void ConfigureMapper()
        {
            Mapper.CreateMap<DriverImageProcess, DriverImageProcess>();
        }

        /// <summary>
        /// This is the deprecated signature. 
        /// </summary>
        /// <param name="dataService"></param>
        /// <param name="token"></param>
        /// <param name="username"></param>
        /// <param name="changeSet"></param>
        /// <param name="persistChanges"></param>
        /// <returns></returns>
        public override ChangeSetResult<string> ProcessChangeSet(IDataService dataService, string token, string username,
                        ChangeSet<string, DriverImageProcess> changeSet, bool persistChanges)
        {
            return ProcessChangeSet(dataService, changeSet, new ProcessChangeSetSettings(token, username, persistChanges));
        }
        /// <summary>
        /// Perform the driver image processing.
        /// </summary>
        /// <param name="dataService"></param>
        /// <param name="changeSet"></param>
        /// <param name="settings"></param>
        /// <returns></returns>
        public override ChangeSetResult<string> ProcessChangeSet(IDataService dataService,
                        ChangeSet<string, DriverImageProcess> changeSet, ProcessChangeSetSettings settings)
        {
            ISession session = null;
            ITransaction transaction = null;

            // If session isn't passed in and changes are being persisted  then open a new session
            if (settings.Session == null && settings.PersistChanges)
            {
                var srRepository = (ISRRepository)repository;
                session = srRepository.OpenSession();
                transaction = session.BeginTransaction();
                settings.Session = session;
            }

            // Running the base process changeset first in this case.  This should give up the benefit of validators, 
            // auditing, security, pipelines, etc. 
            ChangeSetResult<string> changeSetResult = base.ProcessChangeSet(dataService, changeSet, settings);

            // If no problems, we are free to process.
            // We only process one record at a time but in the more general cases we could be processing multiple records.
            // So we loop over the one to many keys in the changeSetResult.SuccessfullyUpdated
            if (!changeSetResult.FailedCreates.Any() && !changeSetResult.FailedUpdates.Any() &&
                !changeSetResult.FailedDeletions.Any())
            {
                foreach (String key in changeSetResult.SuccessfullyUpdated)
                {
                    DataServiceFault fault;
                    string msgKey = key;

                    var driverImageProcess = (DriverImageProcess)changeSetResult.GetSuccessfulUpdateForId(key);

                    // TODO:  Determine userCulture and userRoleIds on a per user basis.
                    string userCulture = "en-GB";
                    IEnumerable<long> userRoleIds = Enumerable.Empty<long>().ToList();

                    // It appears, in the general case, I may need to backfill any additional user input values other than driverID.
                    // They will get clobbered by the call to the base process method.
                    DriverImageProcess backfillDriverImageProcess;
                    if (changeSet.Update.TryGetValue(key, out backfillDriverImageProcess))
                    {
                        // Generally use a mapper?  May not always be the best approach.
                        Mapper.Map(backfillDriverImageProcess, driverImageProcess);
                    }
                    else
                    {
                        changeSetResult.FailedUpdates.Add(msgKey, new MessageSet("DriverImageProcess:Unable to process image for DriverId: " + driverImageProcess.EmployeeId));
                        break;
                    }

                    ////////////////////////////////////////////////
                    //DriverImageProcess has been called
                    log.DebugFormat("SRTEST:DriverImageProcess Called by {0}", key);
                    log.DebugFormat("SRTEST:DriverImageProcess Driver:{0} Trip:{1} Seg:{2} DT:{3} PrintedName:{4} ImageType:{5}",
                                     driverImageProcess.EmployeeId, driverImageProcess.TripNumber,
                                     driverImageProcess.TripSegNumber, driverImageProcess.ActionDateTime,
                                     driverImageProcess.PrintedName, driverImageProcess.ImageType);


                    ////////////////////////////////////////////////
                    // Validate driver id / Get the EmployeeMaster record
                    var employeeMaster = Common.GetEmployeeDriver(dataService, settings, userCulture, userRoleIds,
                                                  driverImageProcess.EmployeeId, out fault);
                    if (null != fault)
                    {
                        changeSetResult.FailedUpdates.Add(msgKey, new MessageSet("Server fault: " + fault.Message));
                        break;
                    }
                    if (null == employeeMaster)
                    {
                        changeSetResult.FailedUpdates.Add(msgKey, new MessageSet("DriverImageProcess:Invalid DriverId: "
                                        + driverImageProcess.EmployeeId));
                        break;
                    }
                    ////////////////////////////////////////////////
                    // Get the Trip record
                    var currentTrip = Common.GetTrip(dataService, settings, userCulture, userRoleIds,
                                                  driverImageProcess.TripNumber, out fault);
                    if (null != fault)
                    {
                        changeSetResult.FailedUpdates.Add(msgKey, new MessageSet("Server fault: " + fault.Message));
                        break;
                    }
                    if (null == currentTrip)
                    {
                        changeSetResult.FailedUpdates.Add(msgKey, new MessageSet("DriverEnrouteProcess:Invalid TripNumber: "
                                        + driverImageProcess.TripNumber));
                        break;
                    }

                    ////////////////////////////////////////////////
                    //ImageType must be P=Picture or S=Signature
                    if (driverImageProcess.ImageType != ImageTypeConstants.Picture &&
                        driverImageProcess.ImageType != ImageTypeConstants.Signature)
                    {
                        changeSetResult.FailedUpdates.Add(msgKey, new MessageSet("ImageType must be P or S. "
                                + driverImageProcess.TripNumber + "-" + driverImageProcess.TripSegNumber));
                        break;
                    }

                    ////////////////////////////////////////////////
                    //Signature Image Types must have a Printed Name
                    if (driverImageProcess.ImageType == ImageTypeConstants.Signature)
                    {
                        if (driverImageProcess.PrintedName == null)
                        {
                            changeSetResult.FailedUpdates.Add(msgKey, new MessageSet("PrintedName is required.. "
                                    + driverImageProcess.TripNumber + "-" + driverImageProcess.TripSegNumber));
                            break;
                        }
                    }
                    //////////////////////////////////////////////
                    //Get the path from the system preference DEFSignatureCapturePath to store the image
                    string prefImageCapturePath = Common.GetPreferenceByParameter(dataService, settings, userCulture, userRoleIds,
                                                   Constants.SystemTerminalId, PrefSystemConstants.DEFSignatureCapturePath, out fault);
                    if (null != fault)
                    {
                        changeSetResult.FailedUpdates.Add(msgKey, new MessageSet("Server fault: " + fault.Message));
                        break;
                    }
                    //////////////////////////////////////////////
                    //Calculate the next sequence number.
                    var tripSegmentImageMax = new TripSegmentImage();
                    tripSegmentImageMax = Common.GetTripSegmentImageLast(dataService, settings, userCulture, userRoleIds,
                                                  driverImageProcess.TripNumber, driverImageProcess.TripSegNumber, out fault);
                    if (null != fault)
                    {
                        changeSetResult.FailedUpdates.Add(msgKey, new MessageSet("Server fault: " + fault.Message));
                        break;
                    }
                    //Determine the next sequence number
                    int imageSeqId = 0;
                    if (null != tripSegmentImageMax)
                    {
                        imageSeqId = ++tripSegmentImageMax.TripSegImageSeqId;
                    }

                    //////////////////////////////////////////////
                    string fullImagePath = "";
                    //Processing for pictures
                    if (driverImageProcess.ImageType == ImageTypeConstants.Picture)
                    {
                        //Create the full path to the file, using the trip number
                        string imagePath = prefImageCapturePath + Common.CreateDirectoryStringForImage(driverImageProcess.TripNumber);
                        string seqId = imageSeqId.ToString("D3");
                        //Assemble full path with file name
                        fullImagePath = imagePath +
                                        driverImageProcess.TripNumber + "-" +
                                        driverImageProcess.TripSegNumber + "-" +
                                        seqId + "." +
                                        ImageExtConstants.Picture;
                      
                        try
                        {
                            //////////////////////////////////////////////
                            //Convert the byte array to an image and save to the server
                            //Reads sample picture from a file into a byte array for testing
                            //Comment out when finished testing
                            //driverImageProcess.ImageByteArray = File.ReadAllBytes(@"C:\Scrap\Samples\013322-03-000.jpg");

                            //Converts the byte array to an image 
                            MemoryStream ms = new MemoryStream(driverImageProcess.ImageByteArray);
                            Image pictureImage = Image.FromStream(ms);
                            //Create the directory if it does not already exist
                            System.IO.FileInfo file = new System.IO.FileInfo(imagePath);
                            file.Directory.Create(); 
                            //Save the image to a file of type jpeg
                            pictureImage.Save(fullImagePath, ImageFormat.Jpeg);
                        }
                        catch (Exception e)
                        {
                            changeSetResult.FailedUpdates.Add(msgKey, new MessageSet("Operation failed for "
                                               + imagePath + ": " + e.Message));
                            break;
                        }

                    }
                    else
                    {
                        //Create the full path to the file, using the trip number
                        string imagePath = prefImageCapturePath + Common.CreateDirectoryStringForImage(driverImageProcess.TripNumber);
                        string seqId = imageSeqId.ToString("D3");
                        //Assemble full path with file name
                        fullImagePath = imagePath +
                                        driverImageProcess.TripNumber + "-" +
                                        driverImageProcess.TripSegNumber + "-" +
                                        seqId + "." +
                                        ImageExtConstants.Signature;

                        try
                        {
                            //////////////////////////////////////////////
                            //Convert the byte array to an image and save to the server
                            //Reads sample picture from a file into a byte array for testing
                            //Comment out when finished testing
                            //driverImageProcess.ImageByteArray = File.ReadAllBytes(@"C:\Scrap\Samples\000010-02-000.png");

                            //Converts the byte array to an image 
                            MemoryStream ms = new MemoryStream(driverImageProcess.ImageByteArray);
                            Image signatureImage = Image.FromStream(ms);
                            //Create the directory if it does not already exist
                            System.IO.FileInfo file = new System.IO.FileInfo(imagePath);
                            file.Directory.Create();
                            //Save the image to a file of type png
                            signatureImage.Save(fullImagePath, ImageFormat.Png);
                        }
                        catch (Exception e)
                        {
                            changeSetResult.FailedUpdates.Add(msgKey, new MessageSet("Operation failed for "
                                               + imagePath + ": " + e.Message));
                            break;
                        }

                    }
                    //////////////////////////////////////////////
                    //Insert a record into the TripSegmentImage table
                    var tripSegmentImage = new TripSegmentImage();
                    tripSegmentImage.TripSegImageSeqId = imageSeqId;
                    tripSegmentImage.TripNumber = driverImageProcess.TripNumber;
                    tripSegmentImage.TripSegNumber = driverImageProcess.TripSegNumber;
                    tripSegmentImage.TripSegImageType = driverImageProcess.ImageType;
                    tripSegmentImage.TripSegImageLocation = fullImagePath;
                    tripSegmentImage.TripSegImageActionDateTime = driverImageProcess.ActionDateTime;
                    if(tripSegmentImage.TripSegImageType == ImageTypeConstants.Signature)
                    {
                        tripSegmentImage.TripSegImagePrintedName = driverImageProcess.PrintedName;
                    }

                    //Do the insert
                    changeSetResult = Common.UpdateTripSegmentImage(dataService, settings, tripSegmentImage);
                    log.DebugFormat("SRTEST:Saving TripSegmentImage for Trip:{0}-{1} - Image Add.",
                                    tripSegmentImage.TripNumber, tripSegmentImage.TripSegNumber);
                    if (Common.LogChangeSetFailure(changeSetResult, tripSegmentImage, log))
                    {
                        var s = string.Format("Could not add TripSegmentImage for Trip:{0}-{1}.",
                                     tripSegmentImage.TripNumber,tripSegmentImage.TripSegNumber);
                        changeSetResult.FailedUpdates.Add(msgKey, new MessageSet(s));
                        break;
                    }

                    //////////////////////////////////////////////
                    //Set the TripImageFlag to the value from the mobile app, P or S
                    //Or if the TripImageFlag was already set to a different value, set it to S/P 
                    //to indicate this trip has both a signature and a picture.
                    if (currentTrip.TripImageFlag == null)
                    {
                        currentTrip.TripImageFlag = driverImageProcess.ImageType;
                    }
                    else
                    {
                        if (!currentTrip.TripImageFlag.Contains(driverImageProcess.ImageType))
                        {
                            currentTrip.TripImageFlag = ImageTypeConstants.Both;
                        }
                    }                 

                    //Do the update
                    changeSetResult = Common.UpdateTrip(dataService, settings, currentTrip);
                    log.DebugFormat("SRTEST:DriverImageProcess:Saving Trip Record for Trip:{0} - Image.",
                                    currentTrip.TripNumber);
                    if (Common.LogChangeSetFailure(changeSetResult, currentTrip, log))
                    {
                        var s = string.Format("DriverImageProcess:Could not update Trip for Trip:{0}.",
                            currentTrip.TripNumber);
                        changeSetResult.FailedUpdates.Add(msgKey, new MessageSet(s));
                        break;
                    }
                    ////////////////////////////////////////////////
                    //Add entry to Event Log – Driver Added Image. 
                    StringBuilder sbComment = new StringBuilder();
                    if (driverImageProcess.ImageType == ImageTypeConstants.Picture)
                    {
                        sbComment.Append(EventCommentConstants.ReceivedDriverPicture);
                    }
                    else
                    {
                        sbComment.Append(EventCommentConstants.ReceivedDriverSignature);
                    }
                    sbComment.Append(" HH:");
                    sbComment.Append(driverImageProcess.ActionDateTime);
                    sbComment.Append(" Trip:");
                    sbComment.Append(driverImageProcess.TripNumber);
                    sbComment.Append("-");
                    sbComment.Append(driverImageProcess.TripSegNumber);
                    sbComment.Append(" Drv:");
                    sbComment.Append(driverImageProcess.EmployeeId);
                    sbComment.Append(" Type:");
                    sbComment.Append(driverImageProcess.ImageType);
                    string comment = sbComment.ToString().Trim();

                    var eventLog = new EventLog()
                    {
                        EventDateTime = driverImageProcess.ActionDateTime,
                        EventSeqNo = 0,
                        EventTerminalId = employeeMaster.TerminalId,
                        EventRegionId = employeeMaster.RegionId,
                        //These are not populated in the current system.
                        // EventEmployeeId = driverStatus.EmployeeId,
                        // EventEmployeeName = Common.GetEmployeeName(employeeMaster),
                        EventTripNumber = driverImageProcess.TripNumber,
                        EventProgram = EventProgramConstants.Services,
                        //These are not populated in the current system.
                        //EventScreen = null,
                        //EventAction = null,
                        EventComment = comment,
                    };

                    ChangeSetResult<int> eventChangeSetResult;
                    eventChangeSetResult = Common.UpdateEventLog(dataService, settings, eventLog);
                    log.Debug("SRTEST:Saving EventLog Record - Image");
                    log.DebugFormat("SRTEST:Saving EventLog Record for Trip:{0}-{1} - Image.",
                                    driverImageProcess.TripNumber, driverImageProcess.TripSegNumber);
                    //Check for EventLog failure.
                    if (Common.LogChangeSetFailure(eventChangeSetResult, eventLog, log))
                    {
                        var s = string.Format("DriverImageProcess:Could not update EventLog for Trip:{0}-{1}.",
                                driverImageProcess.TripNumber, driverImageProcess.TripSegNumber);
                        changeSetResult.FailedUpdates.Add(msgKey, new MessageSet(s));
                        break;
                    }

                } //end of foreach...
            }//end of if (!changeSetResult.Failed...

            // If our local session variable is set then it is our session/txn to deal with
            // otherwise we simply return the result.
            if (session == null)
            {
                return changeSetResult;
            }

            if (changeSetResult.FailedCreates.Any() || changeSetResult.FailedUpdates.Any() ||
                changeSetResult.FailedDeletions.Any())
            {
                transaction.Rollback();
                log.Debug("SRTEST:DriverImageProcess:Transaction Rollback - Image");
            }
            else
            {
                transaction.Commit();
                log.Debug("SRTEST:DriverImageProcess:Transaction Committed - Image");
                // We need to notify that data has changed for any types we have updated
                // We always need to notify for the current type
                dataService.NotifyOfExternalChangesToData();
            }
            transaction.Dispose();
            session.Dispose();
            settings.Session = null;

            return changeSetResult;
        }
    }
}
