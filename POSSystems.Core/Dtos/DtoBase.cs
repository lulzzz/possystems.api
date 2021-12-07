using Humanizer;
using System;

namespace POSSystems.Core.Dtos
{
    public abstract class DtoBase
    {
        public DateTime CreatedDate { get; set; }

        public string CreatedBy { get; set; }

        public DateTime ModifiedDate { get; set; }

        public string ModifiedBy { get; set; }

        private string _status = "A";

        public string Status
        {
            get => _status.DehumanizeTo<Statuses>().ToString();
            set => _status = value;
        }
    }
}