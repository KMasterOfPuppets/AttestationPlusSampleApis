using VI.Base;
using VI.DB;
using VI.DB.Entities;
using QBM.CompositionApi.Definition;
using System.Xml.Linq;

namespace QBM.CompositionApi
{
    public class CCCRemoveMembershipAction : IApiProviderFor<QER.CompositionApi.Portal.PortalApiProject>, IApiProvider
    {
        public void Build(IApiBuilder builder)
        {
            builder.AddMethod(Method.Define("webportalplus/removemembership/action")
                .Handle<PostedID>("POST", async (posted, qr, ct) =>
                {
                    var strUID_Person = qr.Session.User().Uid;
                    string objectkey = string.Empty;
                    string xAttKey = string.Empty;
                    string riskindex = string.Empty;
                    string affectedright = string.Empty;
                    string affectedPersonObjectKey = string.Empty;

                    foreach (var column in posted.columns)
                    {
                        if (column.column == "XObjectKey")
                        {
                            objectkey = column.value;
                        }
                        if (column.column == "xAttKey")
                        {
                            xAttKey = column.value;
                        }
                        if (column.column == "xRisk")
                        {
                            riskindex = column.value;
                        }
                        if (column.column == "#LDS#Affected right")
                        {
                            affectedright = column.value;
                        }
                    }

                    string wc = String.Format("XObjectKey = '{0}' and UID_AttestationCase in (select UID_AttestationCase from ATT_VAttestationDecisionPerson where uid_personhead = '{1}')", xAttKey, strUID_Person);
                    bool ex = await qr.Session.Source().ExistsAsync("AttestationCase", wc, ct).ConfigureAwait(false);
                    if (!ex)
                    {
                        throw new InvalidOperationException("You are not the eligible approver for this attestation case.");
                    }

                    if (objectkey.StartsWith("<Key><T>PersonInOrg</T>", StringComparison.OrdinalIgnoreCase))
                    {
                        var q1 = Query.From("PersonInOrg").Where(string.Format("XObjectKey = '{0}' and ((XOrigin & 1) = 1)", objectkey)).SelectAll();
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

                        var q2 = Query.From("PersonInOrg").Where(string.Format("XObjectKey = '{0}' and ((XOrigin & 8) = 8)", objectkey)).SelectAll();
                        var tryGet2 = await qr.Session.Source().TryGetAsync(q2, EntityLoadType.DelayedLogic).ConfigureAwait(false);
                        if (tryGet2.Success)
                        {
                            var q3 = Query.From("PersonWantsOrg").Where(string.Format("ObjectKeyAssignment = '{0}' and OrderState = 'Assigned'", objectkey)).OrderBy("XDateInserted desc").SelectAll();
                            var tryget3 = await qr.Session.Source().TryGetAsync(q3, EntityLoadType.DelayedLogic, ct).ConfigureAwait(false);
                            if (tryget3.Success)
                            {
                                await tryget3.Result.CallMethodAsync("Unsubscribe", ct).ConfigureAwait(false);
                                await tryget3.Result.SaveAsync(qr.Session, ct).ConfigureAwait(continueOnCapturedContext: false);
                            }
                        }
                    }

                    if (objectkey.StartsWith("<Key><T>PersonInAERole</T>", StringComparison.OrdinalIgnoreCase))
                    {
                        var q1 = Query.From("PersonInAERole").Where(string.Format("XObjectKey = '{0}' and ((XOrigin & 1) = 1)", objectkey)).SelectAll();
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

                        var q2 = Query.From("PersonInAERole").Where(string.Format("XObjectKey = '{0}' and ((XOrigin & 8) = 8)", objectkey)).SelectAll();
                        var tryGet2 = await qr.Session.Source().TryGetAsync(q2, EntityLoadType.DelayedLogic).ConfigureAwait(false);
                        if (tryGet2.Success)
                        {
                            var q3 = Query.From("PersonWantsOrg").Where(string.Format("ObjectKeyAssignment = '{0}' and OrderState = 'Assigned'", objectkey)).OrderBy("XDateInserted desc").SelectAll();
                            var tryget3 = await qr.Session.Source().TryGetAsync(q3, EntityLoadType.DelayedLogic, ct).ConfigureAwait(false);
                            if (tryget3.Success)
                            {
                                await tryget3.Result.CallMethodAsync("Unsubscribe", ct).ConfigureAwait(false);
                                await tryget3.Result.SaveAsync(qr.Session, ct).ConfigureAwait(continueOnCapturedContext: false);
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
                            uidperson = await tryget3.Result.GetValueAsync<string>("UID_Person").ConfigureAwait(false);
                        }
                        var q1 = Query.From("ADSAccountInADSGroup").Where(string.Format("XObjectKey = '{0}' and ((XOrigin & 1) = 1)", objectkey)).SelectAll();
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

                        var q2 = Query.From("ADSGroup").Where(string.Format("UID_ADSGroup = '{0}' and UID_ADSGroup in (SELECT UID_ADSGroup FROM EX0DL) and XObjectKey in (select ObjectKeyOrdered from PersonWantsOrg " +
                                                                            "where OrderState = 'Assigned' and UID_PersonOrdered = '{1}')", uidgroup, uidperson)).SelectAll();
                        var tryGet2 = await qr.Session.Source().TryGetAsync(q2, EntityLoadType.DelayedLogic).ConfigureAwait(false);
                        if (tryGet2.Success)
                        {               
                            var q4 = Query.From("ADSGroup").Where(string.Format("UID_ADSGroup = '{0}'", uidgroup)).SelectAll();                  
                            var tryget4 = await qr.Session.Source().TryGetAsync(q4, EntityLoadType.DelayedLogic, ct).ConfigureAwait(false);
                            if (tryget4.Success)
                            {
                                string groupobjectkey = await tryget4.Result.GetValueAsync<string>("XObjectKey").ConfigureAwait(false);
                                var q5 = Query.From("PersonWantsOrg").Where(string.Format("ObjectKeyOrdered = '{0}' and UID_PersonOrdered = '{1}' and OrderState = 'Assigned'", groupobjectkey, uidperson)).OrderBy("XDateInserted desc").SelectAll();
                                var tryget5 = await qr.Session.Source().TryGetAsync(q5, EntityLoadType.DelayedLogic, ct).ConfigureAwait(false);
                                if (tryget5.Success)
                                {
                                    await tryget5.Result.CallMethodAsync("Unsubscribe", ct).ConfigureAwait(false);
                                    await tryget5.Result.SaveAsync(qr.Session, ct).ConfigureAwait(continueOnCapturedContext: false);
                                }                                    
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
                            uidperson = await tryget3.Result.GetValueAsync<string>("UID_Person").ConfigureAwait(false);
                        }
                        var q1 = Query.From("AADUserInGroup").Where(string.Format("XObjectKey = '{0}' and ((XOrigin & 1) = 1)", objectkey)).SelectAll();
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
                    }

                    var queryAC = Query.From("AttestationCase").SelectAll().Where(String.Format("XObjectKey = '{0}'", xAttKey));
                    var trygetAC = await qr.Session.Source().TryGetAsync(queryAC, EntityLoadType.DelayedLogic, ct).ConfigureAwait(false);

                    IEntity attestationCase = trygetAC.Result;
                    var htParameter = new Dictionary<string, object>
                        {
                            { "access", objectkey },
                            { "datehead", DateTime.Now },
                            { "approverUid", strUID_Person },
                            { "affectedright", affectedright },
                            { "riskindex", riskindex },
                            { "type", "denySINGLE" }
                        };

                    using (var u = qr.Session.StartUnitOfWork())
                    {
                        await u.GenerateAsync(attestationCase, "CCC_AttestationHistoryDE", htParameter, ct).ConfigureAwait(false);
                        await u.CommitAsync(ct).ConfigureAwait(false);
                    };
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