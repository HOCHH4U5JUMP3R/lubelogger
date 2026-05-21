using CarCareTracker.Filter;
using CarCareTracker.Helper;
using CarCareTracker.Models;
using Microsoft.AspNetCore.Mvc;
using System.Globalization;
using System.Text;

namespace CarCareTracker.Controllers
{
    public partial class VehicleController
    {
        [TypeFilter(typeof(CollaboratorFilter))]
        [HttpGet]
        public IActionResult GetTripsByVehicleId(int vehicleId)
        {
            var result = _noteDataAccess.GetNotesByVehicleId(vehicleId)
                .Where(x => (x.Tags ?? new List<string>()).Contains("trip"))
                .OrderByDescending(x => x.Id)
                .ToList();
            return PartialView("Trip/_Trips", result);
        }


        [TypeFilter(typeof(CollaboratorFilter))]
        [HttpGet]
        public IActionResult GetTripModal(int vehicleId, int noteId = 0)
        {
            Note tripRecord;
            if (noteId > 0)
            {
                tripRecord = _noteDataAccess.GetNoteById(noteId);
                if (tripRecord == null)
                {
                    return NotFound();
                }
            }
            else
            {
                tripRecord = new Note { VehicleId = vehicleId, Tags = new List<string> { "trip" }, Files = new List<UploadedFiles>() };
            }
            return PartialView("Trip/_TripModal", tripRecord);
        }
        [HttpPost]
        public IActionResult SaveTripToVehicleId(Note note)
        {
            if (!_userLogic.UserCanEditVehicle(GetUserID(), note.VehicleId, HouseholdPermission.Edit))
            {
                return Json(OperationResponse.Failed("Access Denied"));
            }
            note.Tags = note.Tags.Append("trip").Distinct().ToList();
            note.Files = (note.Files ?? new List<UploadedFiles>()).Select(x => new UploadedFiles { Name = x.Name, Location = _fileHelper.MoveFileFromTemp(x.Location, "documents/") }).ToList();
            var result = _noteDataAccess.SaveNoteToVehicle(note);
            return Json(OperationResponse.Conditional(result, string.Empty, StaticHelper.GenericErrorMessage));
        }

        [HttpPost]
        public IActionResult ImportTripsCsv(int vehicleId, string csvContent)
        {
            if (!_userLogic.UserCanEditVehicle(GetUserID(), vehicleId, HouseholdPermission.Edit))
            {
                return Json(OperationResponse.Failed("Access Denied"));
            }
            var lines = csvContent.Split('\n', StringSplitOptions.RemoveEmptyEntries).Skip(1);
            var success = 0;
            foreach (var line in lines)
            {
                var cols = ParseCsvLine(line.Trim().Trim('\r'));
                if (cols.Count < 9) continue;
                var date = cols[0];
                var start = cols[1];
                var end = cols[2];
                var distance = cols[3];
                var duration = cols[4];
                var avg = cols[5];
                var max = cols[6];
                var maxAlt = cols[7];
                var gain = cols[8];
                var note = new Note
                {
                    VehicleId = vehicleId,
                    Description = $"{date}: {start} -> {end}",
                    NoteText = $"Distance (km): {distance}\nDuration (s): {duration}\nAvg Speed (km/h): {avg}\nMax Speed (km/h): {max}\nMax Altitude (m): {maxAlt}\nElevation Gain (m): {gain}",
                    Tags = new List<string> { "trip" }
                };
                if (_noteDataAccess.SaveNoteToVehicle(note)) success++;
            }
            return Json(OperationResponse.Succeed($"{success} trips imported"));
        }

        private static List<string> ParseCsvLine(string line)
        {
            var result = new List<string>();
            var current = new StringBuilder();
            var inQuotes = false;

            foreach (var ch in line)
            {
                if (ch == '"')
                {
                    inQuotes = !inQuotes;
                    continue;
                }

                if (ch == ',' && !inQuotes)
                {
                    result.Add(current.ToString().Trim());
                    current.Clear();
                    continue;
                }

                current.Append(ch);
            }

            result.Add(current.ToString().Trim());
            return result;
        }
    }
}
