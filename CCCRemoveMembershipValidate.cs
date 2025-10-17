using QBM.CompositionApi.Definition;
using System.Xml.Linq;
using VI.DB.Entities;

namespace QBM.CompositionApi
{
    public class CCCRemoveMembershipValidate : IApiProviderFor<QER.CompositionApi.Portal.PortalApiProject>
    {
        public void Build(IApiBuilder builder)
        {
            builder.AddMethod(Method.Define("webportalplus/removemembership/validate")
                .Handle<PostedID, object[]>("POST", async (posted, qr, ct) =>
                {
                    List<object> objects = new List<object>();
                    string objectkey = string.Empty;
                    string group = string.Empty;
                    string xkey = string.Empty;
                    foreach (var column in posted.columns)
                    {
                        if (column.column == "XObjectKey")
                        {
                            objectkey = column.value;
                        }
                        if (column.column == "xDisplay")
                        {
                            group = column.value;
                        }
                    }

                    if (objectkey.StartsWith("<Key><T>PersonInOrg</T>", StringComparison.OrdinalIgnoreCase))
                    {
                        var q1 = Query.From("PersonInOrg").Where(string.Format("XObjectKey = '{0}' and ((XOrigin & 1) = 1)", objectkey)).SelectAll();
                        var q2 = Query.From("PersonInOrg").Where(string.Format("XObjectKey = '{0}' and ((XOrigin & 8) = 8)", objectkey)).SelectAll();
                        var tryGet1 = await qr.Session.Source().TryGetAsync(q1, EntityLoadType.DelayedLogic).ConfigureAwait(false);
                        var tryGet2 = await qr.Session.Source().TryGetAsync(q2, EntityLoadType.DelayedLogic).ConfigureAwait(false);
                        if (!tryGet1.Success && !tryGet2.Success)
                        {
                            foreach (var column in posted.columns)
                            {
                                if (column.column == "xDisplay")
                                {
                                    objects.Add(new { column = column.value, errorMsg = "#LDS#Assignment not found. Please reload the data" });
                                }
                                else
                                {
                                    objects.Add(new { column = column.column });
                                }
                            }
 
                        } else
                        {
                            foreach (var column in posted.columns)
                            {
                                objects.Add(new { column = column.column });

                            }
                        }
                    }

                    if (objectkey.StartsWith("<Key><T>PersonInAERole</T>", StringComparison.OrdinalIgnoreCase))
                    {
                        var q1 = Query.From("PersonInAERole").Where(string.Format("XObjectKey = '{0}' and ((XOrigin & 1) = 1)", objectkey)).SelectAll();
                        var q2 = Query.From("PersonInAERole").Where(string.Format("XObjectKey = '{0}' and ((XOrigin & 8) = 8)", objectkey)).SelectAll();
                        var tryGet1 = await qr.Session.Source().TryGetAsync(q1, EntityLoadType.DelayedLogic).ConfigureAwait(false);
                        var tryGet2 = await qr.Session.Source().TryGetAsync(q2, EntityLoadType.DelayedLogic).ConfigureAwait(false);
                        if (!tryGet1.Success && !tryGet2.Success)
                        {
                            foreach (var column in posted.columns)
                            {
                                if (column.column == "xDisplay")
                                {
                                    objects.Add(new { column = column.value, errorMsg = "#LDS#Assignment not found. Please reload the data" });
                                }
                                else
                                {
                                    objects.Add(new { column = column.column });
                                }
                            }

                        }
                        else
                        {
                            foreach (var column in posted.columns)
                            {
                                objects.Add(new { column = column.column });

                            }
                        }
                    }

                    if (objectkey.StartsWith("<Key><T>ADSAccountInADSGroup</T>", StringComparison.OrdinalIgnoreCase))
                    {
                        XDocument doc = XDocument.Parse(objectkey);
                        var pValues = doc.Descendants("P").Select(p => p.Value).ToList();
                        string uidaccount = pValues[0];
                        string uidgroup = pValues[1];
                        string uidperson = string.Empty;
                        var q3 = Query.From("ADSAccount").Where(string.Format("UID_ADSAccount = '{0}'", uidaccount)).SelectAll();
                        var tryget3 = await qr.Session.Source().TryGetAsync(q3, EntityLoadType.DelayedLogic, ct).ConfigureAwait(false);
                        if (tryget3.Success)
                        {
                            uidperson = tryget3.Result.GetValue("UID_Person");
                        }
                        var q2 = Query.From("ADSGroup").Where(string.Format("UID_ADSGroup = '{0}' and UID_ADSGroup in (SELECT UID_ADSGroup FROM EX0DL) and XObjectKey in (select ObjectKeyOrdered from PersonWantsOrg " +
                                                                            "where OrderState = 'Assigned' and UID_PersonOrdered = '{1}')", uidgroup, uidperson)).SelectAll();

                        var q1 = Query.From("ADSAccountInADSGroup").Where(string.Format("XObjectKey = '{0}' and ((XOrigin & 1) = 1)", objectkey)).SelectAll();
                        var tryGet1 = await qr.Session.Source().TryGetAsync(q1, EntityLoadType.DelayedLogic).ConfigureAwait(false);
                        var tryGet2 = await qr.Session.Source().TryGetAsync(q2, EntityLoadType.DelayedLogic).ConfigureAwait(false);
                        if (!tryGet1.Success && !tryGet2.Success)
                        {
                            foreach (var column in posted.columns)
                            {
                                if (column.column == "xDisplay")
                                {
                                    objects.Add(new { column = column.value, errorMsg = "#LDS#Assignment not found. Please reload the data" });
                                }
                                else
                                {
                                    objects.Add(new { column = column.column });
                                }
                            }

                        }
                        else
                        {
                            foreach (var column in posted.columns)
                            {
                                objects.Add(new { column = column.column });

                            }
                        }
                    }

                    if (objectkey.StartsWith("<Key><T>AADUserInGroup</T>", StringComparison.OrdinalIgnoreCase))
                    {
                        XDocument doc = XDocument.Parse(objectkey);
                        var pValues = doc.Descendants("P").Select(p => p.Value).ToList();
                        string uidaccount = pValues[1];
                        string uidgroup = pValues[0];
                        string uidperson = string.Empty;
                        var q3 = Query.From("AADUser").Where(string.Format("UID_AADUser = '{0}'", uidaccount)).SelectAll();
                        var tryget3 = await qr.Session.Source().TryGetAsync(q3, EntityLoadType.DelayedLogic, ct).ConfigureAwait(false);
                        if (tryget3.Success)
                        {
                            uidperson = tryget3.Result.GetValue("UID_Person");
                        }
                       
                        var q1 = Query.From("AADUserInGroup").Where(string.Format("XObjectKey = '{0}' and ((XOrigin & 1) = 1)", objectkey)).SelectAll();
                        var tryGet1 = await qr.Session.Source().TryGetAsync(q1, EntityLoadType.DelayedLogic).ConfigureAwait(false);
                        if (!tryGet1.Success)
                        {
                            foreach (var column in posted.columns)
                            {
                                if (column.column == "xDisplay")
                                {
                                    objects.Add(new { column = column.value, errorMsg = "#LDS#Assignment not found. Please reload the data" });
                                }
                                else
                                {
                                    objects.Add(new { column = column.column });
                                }
                            }
                        }
                        else
                        {
                            foreach (var column in posted.columns)
                            {
                                objects.Add(new { column = column.column });
                            }
                        }
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