using System.Collections.Generic;
using System.Linq;
using ClinicVetsAvalonia.Data;
using ClinicVetsAvalonia.Models;

namespace ClinicVetsAvalonia.Services
{
    // Derives the latest veterinarian note for an animal from its visits, so the animal
    // card/details can show the most recent note without duplicating it onto the Animal model.
    public static class AnimalNoteService
    {
        public const string NoNoteText = "אין הערה";

        // Returns the ArrivalNote of the animal's most recent visit (by date) that has a
        // non-empty note, or an empty string when the animal has no notes.
        public static string GetLatestVetNote(string chipNumber, IEnumerable<Visit> visits)
        {
            if (string.IsNullOrWhiteSpace(chipNumber))
                return "";

            return visits
                .Where(visit => visit.AnimalChipNumber == chipNumber &&
                                !string.IsNullOrWhiteSpace(visit.ArrivalNote))
                .OrderByDescending(visit => visit.VisitDate)
                .Select(visit => visit.ArrivalNote.Trim())
                .FirstOrDefault() ?? "";
        }

        // UI convenience overload that reads the in-memory visits cache.
        public static string GetLatestVetNote(string chipNumber)
        {
            return GetLatestVetNote(chipNumber, AppData.Visits);
        }

        // Same as GetLatestVetNote but returns the "אין הערה" placeholder when there is no note.
        public static string GetLatestVetNoteOrPlaceholder(string chipNumber, IEnumerable<Visit> visits)
        {
            string note = GetLatestVetNote(chipNumber, visits);
            return string.IsNullOrWhiteSpace(note) ? NoNoteText : note;
        }

        public static string GetLatestVetNoteOrPlaceholder(string chipNumber)
        {
            return GetLatestVetNoteOrPlaceholder(chipNumber, AppData.Visits);
        }
    }
}
