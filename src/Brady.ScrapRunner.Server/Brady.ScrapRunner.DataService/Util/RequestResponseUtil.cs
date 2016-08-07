
using System;
using System.Text;
using BWF.DataServices.Domain.Models;
using BWF.DataServices.Metadata.Interfaces;
using NHibernate.Util;

namespace Brady.ScrapRunner.DataService.Util
{
    public class RequestResponseUtil
    {

        /// <summary>
        /// Return a string builder containing the first half a request/response.
        /// </summary>
        /// <typeparam name="TId"></typeparam>
        /// <typeparam name="TItem"></typeparam>
        /// <param name="changeSet"></param>
        /// <returns></returns>
        public static StringBuilder CaptureRequest<TId, TItem>(ChangeSet<TId, TItem> changeSet)
            where TItem : IHaveId<TId>
        {
            var type = typeof (TItem);
            var sb = new StringBuilder();
            sb.Append("Request " + type.Name + ":");

            if (changeSet.Update.Values.Any())
            {
                sb.Append(" Updates:");
                sb.Append(string.Join(", ", changeSet.Update.Values));
            }

            if (changeSet.Create.Values.Any())
            {
                sb.Append(" Creates:");
                sb.Append(string.Join(", ", changeSet.Create.Values));
            }

            if (changeSet.Delete.Any())
            {
                sb.Append(" Deletes:");
                sb.Append(string.Join(", ", changeSet.Delete));
            }

            return sb;
        }


        /// <summary>
        /// Capture the second half a request/response.  Append it to a string builder
        /// containing the first half a request/response.  Log it at the INFO level. 
        /// </summary>
        /// <typeparam name="TId"></typeparam>
        /// <param name="changeSetResult"></param>
        /// <param name="sb">The string builder containing the corresponding request.  If null a new one is created.</param>
        public static string CaptureResponse<TId>(ChangeSetResult<TId> changeSetResult,
            StringBuilder sb)
        {
            //var type = typeof (TId);
            if (sb == null)
            {
                sb = new StringBuilder("No incoming request?");
            }
            sb.Append(" ==> Response: ");

            if (changeSetResult.FailedUpdates.Any())
            {
                sb.Append(" Failed Updates:");
                sb.Append(String.Join(", ", changeSetResult.FailedUpdates.Values));
            }
            if (changeSetResult.FailedCreates.Any())
            {
                sb.Append(" Failed Creates:");
                sb.Append(String.Join(", ", changeSetResult.FailedCreates.Values));
            }
            if (changeSetResult.FailedDeletions.Any())
            {
                sb.Append(" Failed Deletions:");
                sb.Append(String.Join(", ", changeSetResult.FailedDeletions.Values));
            }

            if (changeSetResult.SuccessfullyUpdatedItems.Any())
            {
                sb.Append(" Successful Updates:");
                sb.Append(String.Join(", ", changeSetResult.SuccessfullyUpdatedItems.Values));
            }
            if (changeSetResult.SuccessfullyCreatedItems.Any())
            {
                sb.Append(" Successful Creations:");
                sb.Append(String.Join(", ", changeSetResult.SuccessfullyCreatedItems.Values));
            }
            if (changeSetResult.SuccessfullyDeleted.Any())
            {
                sb.Append(" Successful Deletions:");
                sb.Append(String.Join(", ", changeSetResult.SuccessfullyDeleted.GetEnumerator()));
            }

            return sb.ToString();
        }
    }
}