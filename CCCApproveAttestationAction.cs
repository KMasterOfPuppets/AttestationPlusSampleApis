using QBM.CompositionApi.Definition;
using VI.DB;
using VI.DB.Entities;

namespace QBM.CompositionApi
{
    public class CCCApproveAttestationAction : IApiProviderFor<QER.CompositionApi.Portal.PortalApiProject>
    {
        public void Build(IApiBuilder builder)
        {
            builder.AddMethod(Method.Define("webportalplus/approveattestation/action")
                .Handle<PostedID>("POST", async (posted, qr, ct) =>
                {
                    var strUID_Person = qr.Session.User().Uid;
                    string xkey = string.Empty;
                    string xsubkey = string.Empty;
                    bool Decision = true;
                    string Reason = null;
                    string UidJustification = null;
                    int SubLevel = -1;

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

                    var query1 = Query.From("AttestationCase").SelectAll().Where(String.Format("XObjectKey = '{0}'", xsubkey));
                    var tryget = await qr.Session.Source().TryGetAsync(query1, EntityLoadType.DelayedLogic, ct).ConfigureAwait(false);

                    IEntity attestationCase = tryget.Result;
                    
                    int num = SubLevel;
                    await attestationCase.CallMethodAsync("MakeDecision", new object[5]
                    {
                        qr.Session.User().Uid,
                        Decision,
                        Reason,
                        UidJustification,
                        num
                    }, ct).ConfigureAwait(continueOnCapturedContext: false);
                    await attestationCase.SaveAsync(qr.Session, ct).ConfigureAwait(continueOnCapturedContext: false);

                    string type = string.Empty;
                    string policyUid = attestationCase.GetValue("UID_AttestationPolicy");
                    var yearlyUid = await qr.Session.Config().GetConfigParmAsync("Custom\\DataExplorerAttestations\\Yearly", ct).ConfigureAwait(false);
                    var moverUid = await qr.Session.Config().GetConfigParmAsync("Custom\\DataExplorerAttestations\\Mover", ct).ConfigureAwait(false);
                    var externalUid = await qr.Session.Config().GetConfigParmAsync("Custom\\DataExplorerAttestations\\External", ct).ConfigureAwait(false);
                    var guestUid = await qr.Session.Config().GetConfigParmAsync("Custom\\DataExplorerAttestations\\Guest", ct).ConfigureAwait(false);
                    if (string.Equals(policyUid, yearlyUid) || string.Equals(policyUid, moverUid))
                    {
                        type = "approveAll";
                    }
                    if (string.Equals(policyUid, externalUid) || string.Equals(policyUid, guestUid))
                    {
                        type = "approveExternalOrGuest";
                    }
                    var htParameter = new Dictionary<string, object>
                    {
                        { "approverUid", strUID_Person },
                        { "type", type }
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
            public columnsarray[] element { get; set; }
            public columnsarray[] columns { get; set; }
        }
        public class columnsarray
        {
            public string column { get; set; }
            public string value { get; set; }
        }
    }
}