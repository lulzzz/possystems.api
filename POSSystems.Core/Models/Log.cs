using System;
using System.ComponentModel.DataAnnotations;

namespace POSSystems.Core.Models
{
    public class Log
    {
        public Log()
        {
        }

        public Log(string performedBy, string action,
            string controller, string description)
        {
            PerformedBy = performedBy;
            Action = action;
            Controller = controller;
            Description = description;
            PerformedAt = DateTime.Now;
        }

        [Key]
        public int LogID { get; set; }

        public DateTime PerformedAt { get; set; }
        public string Controller { get; set; }
        public string Action { get; set; }
        public string PerformedBy { get; set; }
        public string Description { get; set; }
    }
}