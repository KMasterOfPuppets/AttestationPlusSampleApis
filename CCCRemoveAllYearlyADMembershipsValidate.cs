using QBM.CompositionApi.Definition;

namespace QBM.CompositionApi
{
    public class CCCRemoveAllYearlyADMembershipsValidate : IApiProviderFor<QER.CompositionApi.Portal.PortalApiProject>
    {
        public void Build(IApiBuilder builder)
        {
            builder.AddMethod(Method.Define("webportalplus/removeallyearlyadmemberships/validate")
                .Handle<PostedID, object[]>("POST", async (posted, qr, ct) =>
                {
                    List<object> objects = new List<object>();
                    foreach (var column in posted.columns)
                    {
                        objects.Add(new { column = column.column });

                    }
                    object[] array = objects.ToArray();
                    return array;
                }));
        }
        public class PostedID
        {
            public columnsarray[] columns { get; set; }
        }
        public class columnsarray
        {
            public string column { get; set; }
            public string value { get; set; }
        }
    }
}