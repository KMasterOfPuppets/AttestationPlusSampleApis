using VI.DB.Entities;
using QBM.CompositionApi.Definition;
using VI.DB.DataAccess;
using VI.DB.Sync;
using VI.DB;
using System.Xml.Linq;

namespace QBM.CompositionApi
{
    public class CCCRemoveAllMoverLAMembershipsAction : IApiProviderFor<QER.CompositionApi.Portal.PortalApiProject>, IApiProvider
    {
        public void Build(IApiBuilder builder)
        {
            builder.AddMethod(Method.Define("webportalplus/removeallmoverlamemberships/action")
                .Handle<PostedID>("POST", async (posted, qr, ct) =>
                {
                    string xkey = string.Empty;
                    string xsubkey = string.Empty;
                    var strUID_Person = qr.Session.User().Uid;
                    foreach (var column in posted.columns)
                    {
                        if (column.column == "xKey")
                        {
                            xkey = column.value;
                        }
                        if (column.column == "xSubKey")
                        {
                            xsubkey = column.value;
                        }
                    }
                    string wc = String.Format("XObjectKey = '{0}' and UID_AttestationCase in (select UID_AttestationCase from ATT_VAttestationDecisionPerson where uid_personhead = '{1}')", xsubkey, strUID_Person);
                    bool ex = await qr.Session.Source().ExistsAsync("AttestationCase", wc, ct).ConfigureAwait(false);
                    if (!ex)
                    {
                        throw new InvalidOperationException("You are not the eligible approver for this attestation case.");
                    }
                    var assignmentkeys = new List<string>();
                    var runner = qr.Session.Resolve<IStatementRunner>();
                    using (var reader = runner.SqlExecute("CCC_DE_MoverAttestationSubLocalAdmin", new[]
                    {
                            QueryParameter.Create("xkey", xkey),
                            QueryParameter.Create("xsubkey", xsubkey)
                        }))
                    {
                        while (reader.Read())
                        {
                            for (int i = 0; i < reader.FieldCount; i++)
                            {
                                var columnname = reader.GetName(i);
                                if (columnname == "XObjectKey")
                                {
                                    var fieldvalue = reader.IsDBNull(i) ? string.Empty : reader.GetValue(i).ToString();
                                    assignmentkeys.Add(fieldvalue);
                                }
                            }
                        }
                    }
                    foreach (var key in assignmentkeys)
                    {
                        var q1 = Query.From("ADSAccountInADSGroup").Where(string.Format("XObjectKey = '{0}' and ((XOrigin & 1) = 1)", key)).SelectAll();
                        var tryGet1 = await qr.Session.Source().TryGetAsync(q1, EntityLoadType.DelayedLogic).ConfigureAwait(false);
                        if (tryGet1.Success)
                        {
                            using (var u = qr.Session.StartUnitOfWork())
                            {
                                var objecttodelete = tryGet1.Result;
                                objecttodelete.MarkForDeletion();
                                await u.PutAsync(objecttodelete, ct).ConfigureAwait(false);
                                await u.CommitAsync(ct).ConfigureAwait(false);
                            }
                        }

                        var q2 = Query.From("ADSAccountInADSGroup").Where(string.Format("XObjectKey = '{0}' and ((XOrigin & 8) = 8)", key)).SelectAll();
                        var tryGet2 = await qr.Session.Source().TryGetAsync(q2, EntityLoadType.DelayedLogic).ConfigureAwait(false);
                        if (tryGet2.Success)
                        {
                            XDocument doc = XDocument.Parse(key);
                            var pValues = doc.Descendants("P").Select(p => p.Value).ToList();
                            string uidaccount = pValues[0];
                            string uidgroup = pValues[1];
                            var q3 = Query.From("ADSAccount").Where(string.Format("UID_ADSAccount = '{0}'", uidaccount)).SelectAll();
                            var q4 = Query.From("ADSGroup").Where(string.Format("UID_ADSGroup = '{0}'", uidgroup)).SelectAll();
                            var tryget3 = await qr.Session.Source().TryGetAsync(q3, EntityLoadType.DelayedLogic, ct).ConfigureAwait(false);
                            var tryget4 = await qr.Session.Source().TryGetAsync(q4, EntityLoadType.DelayedLogic, ct).ConfigureAwait(false);
                            if (tryget3.Success && tryget4.Success)
                            {
                                string uidperson = tryget3.Result.GetValue("UID_Person");
                                string groupobjectkey = tryget4.Result.GetValue("XObjectKey");
                                var q5 = Query.From("PersonWantsOrg").Where(string.Format("ObjectKeyOrdered = '{0}' and UID_PersonOrdered = '{1}'", groupobjectkey, uidperson)).OrderBy("XDateInserted desc").SelectAll();
                                var tryget5 = await qr.Session.Source().TryGetAsync(q5, EntityLoadType.DelayedLogic, ct).ConfigureAwait(false);
                                if (tryget5.Success)
                                {
                                    await tryget5.Result.CallMethodAsync("Unsubscribe", ct).ConfigureAwait(false);
                                    await tryget5.Result.SaveAsync(qr.Session, ct).ConfigureAwait(continueOnCapturedContext: false);
                                }
                            }
                        }

                        var queryAC = Query.From("AttestationCase").SelectAll().Where(String.Format("XObjectKey = '{0}'", xsubkey));
                        var trygetAC = await qr.Session.Source().TryGetAsync(queryAC, EntityLoadType.DelayedLogic, ct).ConfigureAwait(false);

                        IEntity attestationCase = trygetAC.Result;
                        var htParameter = new Dictionary<string, object>
                        {
                            { "access", key },
                            { "approverUid", strUID_Person },
                            { "type", "denySINGLE" }
                        };

                        using (var u = qr.Session.StartUnitOfWork())
                        {
                            await u.GenerateAsync(attestationCase, "CCC_AttestationHistoryDE", htParameter, ct).ConfigureAwait(false);
                            await u.CommitAsync(ct).ConfigureAwait(false);
                        };
                    }
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