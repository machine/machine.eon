using System;
using System.Collections.Generic;
using System.Linq;

using Machine.Eon.Mapping;
using Type = Machine.Eon.Mapping.Type;

using Machine.Specifications;

namespace Machine.Eon.Specs
{
  public class with_eon
  {
    protected static QueryRoot qr;
    protected static Type systemVoid;
    protected static Type systemString;

    Establish context = () =>
    {
      log4net.Config.XmlConfigurator.Configure();
      if (qr == null)
      {
        Mapper mapper = new Mapper();
        mapper.Include(typeof(Mapper).Assembly.Location);
        mapper.Include(typeof(with_eon).Assembly.Location);
        qr = mapper.ToQueryRoot();
        systemVoid = qr.FromSystemType(typeof(void));
        systemString = qr.FromSystemType(typeof(string));
      }
    };
  }
}
