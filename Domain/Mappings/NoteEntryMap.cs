using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using FluentNHibernate.Mapping;
using SampleApp.Domain.Objects;

namespace SampleApp.Domain.Mappings
{
    public class NoteEntryMap : ClassMap<NoteEntry>
    {

        public NoteEntryMap()
        {
            Schema("evernote");
            Table("notes");

            Id(x => x.id).Column("id");
            Map(x => x.Guid);
            Map(x => x.Line1);
            Map(x => x.Line2);
            Map(x => x.Line3);
        }
    }

}
