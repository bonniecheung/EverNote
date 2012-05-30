using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SampleApp.Domain.Objects
{
    public class NoteEntry
    {
        public virtual int id { get; set; }
        public virtual string Guid { get; set; }
        public virtual string Line1 { get; set; }
        public virtual string Line2 { get; set; }
        public virtual string Line3 { get; set; }

    }
}